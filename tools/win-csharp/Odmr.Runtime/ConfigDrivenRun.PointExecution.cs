using System.Globalization;
using Odmr.Artifacts;
using Odmr.Devices;

namespace Odmr.Runtime;

public static partial class ConfigDrivenRun
{
    private static BaselineSnapshot LockBaseline(RunConfigBundle bundle, IReadOnlyList<ConfigMagAxis> axes, string baselinePath)
    {
        var policy = bundle.Plan.MagBaselinePolicy;
        if (policy.BaselineCurrentA.Length != 3)
        {
            throw new InvalidOperationException("mag_baseline_policy.baseline_current_a must contain 3 values");
        }

        var snapshots = new List<BaselineAxisSnapshot>(axes.Count);
        for (var index = 0; index < axes.Count; index++)
        {
            var axis = axes[index];
            EnsureCurrentWithinM8812Range($"baseline_current_a[{index}]", policy.BaselineCurrentA[index]);
            if (policy.VoltageV.HasValue)
            {
                axis.Session.SetVoltage(policy.VoltageV.Value);
            }
            if (policy.VoltageProtectionV.HasValue)
            {
                axis.Session.SetVoltageProtection(policy.VoltageProtectionV.Value);
            }

            axis.Session.SetCurrent(policy.BaselineCurrentA[index]);
            axis.Session.SetOutput(policy.OutputEnabled);
            Thread.Sleep(policy.SettleMs);

            var readbacks = new List<double>(policy.ReadbackSamples);
            for (var i = 0; i < policy.ReadbackSamples; i++)
            {
                readbacks.Add(axis.Session.MeasureCurrent());
                Thread.Sleep(100);
            }

            var locked = readbacks.Average();
            if (Math.Abs(locked - policy.BaselineCurrentA[index]) > policy.SettleToleranceA)
            {
                throw new InvalidOperationException($"{axis.AxisId} baseline lock exceeds tolerance: set={policy.BaselineCurrentA[index]}A locked={locked}A");
            }

            snapshots.Add(new BaselineAxisSnapshot(axis.AxisId, policy.BaselineCurrentA[index], readbacks, locked));
        }

        var baseline = new BaselineSnapshot(
            1,
            "locked_zero_offset",
            UtcNowString(),
            policy.SettleMs,
            policy.ReadbackSamples,
            policy.SettleToleranceA,
            snapshots);
        RallArtifactWriter.WritePrettyJson(baselinePath, baseline);
        return baseline;
    }

    private static QualityRecord RunPoint(
        RunConfigBundle bundle,
        int index,
        RunPointPlan point,
        IReadOnlyList<double>? baselineCurrentA,
        IReadOnlyList<ConfigMagAxis> magAxes,
        ISmb100aSession smb,
        ILockinCollector collector,
        string segmentsPath,
        string pointsPath,
        string qualityPath,
        string deviceStatePath,
        string eventsPath,
        long processStart,
        ConfigDrivenRunOptions options)
    {
        ThrowIfEmergencyRequested(options);
        var settleStarted = UtcNowString();
        double[]? deltaCurrentA = null;
        double[]? targetCurrentA = null;
        double[]? measuredCurrentA = null;
        var m8812Commanded = false;
        if (point.UsesMagneticControl)
        {
            if (baselineCurrentA is null)
            {
                throw new InvalidOperationException($"point {point.PointId} requires magnetic control but baseline was not locked");
            }

            deltaCurrentA = bundle.Calibration.DeltaCurrentA(point.TargetBNt!);
            targetCurrentA = bundle.Calibration.TargetCurrentA(baselineCurrentA, point.TargetBNt!);
            EnsureTargetCurrentsWithinM8812Range(point.PointId, targetCurrentA);

            for (var axisIndex = 0; axisIndex < magAxes.Count; axisIndex++)
            {
                magAxes[axisIndex].Session.SetCurrent(targetCurrentA[axisIndex]);
                magAxes[axisIndex].Session.SetOutput(true);
            }

            m8812Commanded = true;
            SleepInterruptibly(bundle.Plan.PointSettleMs, options.EmergencyStopToken);
            ThrowIfEmergencyRequested(options);
            measuredCurrentA = magAxes.Select(axis => axis.Session.MeasureCurrent()).ToArray();
        }
        var settleEnded = UtcNowString();

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "point_stable", "settle", point.PointId, null, new
        {
            point_kind = "acquisition_step",
            magnetic_mode = point.EffectiveMagneticMode,
            m8812_commanded = m8812Commanded,
            target_current_a = targetCurrentA,
            measured_current_a = measuredCurrentA
        });

        var sweep = bundle.SmbProfile.DefaultSweep.ApplyOverride(point.SmbOverride);
        var smbConfigureError = ConfigureSmbSweepForPointPreparation(smb, bundle.SmbProfile, sweep, options.EmergencyStopToken);
        ThrowIfEmergencyRequested(options);

        var segmentId = $"seg_{point.PointId}_0000";
        var timeoutsBefore = collector.Snapshot().Stats.TimeoutCount;
        var decodeFailuresBefore = collector.Snapshot().DecodeFailures ?? 0;

        PrepareSmbRfForSegment(smb, bundle.SmbProfile, sweep, options.EmergencyStopToken);
        ThrowIfEmergencyRequested(options);
        var segmentStartTs = UtcNowString();
        var segmentStartMonotonicNs = MonotonicNsSince(processStart);
        var rfExposureStartedTs = segmentStartTs;
        var rfExposureStartedMonotonicNs = segmentStartMonotonicNs;
        var segmentStart = collector.Cursor();
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "segment_started", "segment", point.PointId, null, new
        {
            segment_id = segmentId
        });
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "sweep_started", "sweep", point.PointId, null, new
        {
            segment_id = segmentId,
            resolved_point_count = bundle.ResolvedPlan.ResolvedPointCount,
            estimated_sweep_duration_ms = sweep.EstimatedSweepDurationMs,
            sweep_points = sweep.SweepPoints,
            start_hz = sweep.StartHz,
            stop_hz = sweep.StopHz,
            step_hz = sweep.StepHz,
            dwell_ms = sweep.DwellMs,
            rf_output_policy = "point_output_on_after_sweep_config_segment_scoped_execute",
            sweep_window_policy = "segment_scoped_execute_only"
        });
        var sweepStartSnapshot = collector.Snapshot();
        var effectiveEstimatedRunDurationMs = options.EstimatedRunDurationMsOverride ?? bundle.ResolvedPlan.EstimatedRunDurationMs;
        ReportProgress(
            options,
            RuntimeState.PointRunning,
            "sweep_started",
            $"Sweep started: {point.PointId}",
            point.PointId,
            index,
            bundle.ResolvedPlan.ResolvedPointCount,
            sweepStartSnapshot.Stats.FramesOk,
            null,
            null,
            null,
            null,
            effectiveEstimatedRunDurationMs,
            sweep.EstimatedSweepDurationMs + bundle.Plan.PointSettleMs + bundle.SmbProfile.EstimatedPointConfigurationMs,
            sweep.EstimatedSweepDurationMs,
            sweep.SweepPoints,
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            lockinModel: bundle.OeProfile.NormalizedModel,
            collectorContract: RunConfigLoader.CollectorContractFor(bundle.OeProfile.NormalizedModel),
            decodeFailures: sweepStartSnapshot.DecodeFailures,
            effectiveSampleHzPerParameter: sweepStartSnapshot.EffectiveSampleHzPerParameter,
            samplesTotal: sweepStartSnapshot.SamplesWritten);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_exposure_started", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            output_state = "point_output_on_after_sweep_config_segment_scoped_execute",
            frequency_mode = "SWE",
            start_hz = sweep.StartHz
        });
        var sweepObservation = ExecuteSmbSweepInsideSegment(smb, bundle.SmbProfile, sweep, options.EmergencyStopToken);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_sweep_executed", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            estimated_sweep_duration_ms = sweepObservation.EstimatedSweepDurationMs,
            opc_wait_ms = sweepObservation.OpcWaitMs,
            fallback_used = sweepObservation.FallbackUsed
        });
        StopSmbRfAfterSegment(smb, bundle.SmbProfile, sweep);
        ThrowIfEmergencyRequested(options);
        var rfExposureEndedTs = UtcNowString();
        var rfExposureEndedMonotonicNs = MonotonicNsSince(processStart);
        var segmentEndTs = rfExposureEndedTs;
        var segmentEndMonotonicNs = rfExposureEndedMonotonicNs;
        var segmentEnd = collector.Cursor();
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_exposure_ended", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            frequency_mode = "CW",
            cw_frequency_hz = sweep.StartHz
        });

        var timeoutsAfter = collector.Snapshot().Stats.TimeoutCount;
        var pointTimeouts = timeoutsAfter - timeoutsBefore;
        var decodeFailuresAfter = collector.Snapshot().DecodeFailures ?? 0;
        var pointDecodeFailures = decodeFailuresAfter - decodeFailuresBefore;
        var framesInSegment = segmentEnd.NextFrameSeq - segmentStart.NextFrameSeq;
        var uniqueBlocksInSegment = segmentEnd.NextUniqueFrameSeq - segmentStart.NextUniqueFrameSeq;
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "sweep_completed", "sweep", point.PointId, null, new
        {
            segment_id = segmentId,
            estimated_sweep_duration_ms = sweepObservation.EstimatedSweepDurationMs,
            sweep_points = sweep.SweepPoints,
            start_hz = sweep.StartHz,
            stop_hz = sweep.StopHz,
            step_hz = sweep.StepHz,
            dwell_ms = sweep.DwellMs,
            opc_wait_ms = sweepObservation.OpcWaitMs,
            fallback_used = sweepObservation.FallbackUsed,
            smb_configure_error = smbConfigureError,
            rf_exposure_started_ts = rfExposureStartedTs,
            rf_exposure_ended_ts = rfExposureEndedTs,
            rf_exposure_started_monotonic_ns = rfExposureStartedMonotonicNs,
            rf_exposure_ended_monotonic_ns = rfExposureEndedMonotonicNs
        });
        var sweepCompletedSnapshot = collector.Snapshot();
        ReportProgress(
            options,
            RuntimeState.PointRunning,
            "sweep_completed",
            $"Sweep completed: {point.PointId}",
            point.PointId,
            index,
            bundle.ResolvedPlan.ResolvedPointCount,
            sweepCompletedSnapshot.Stats.FramesOk,
            null,
            null,
            null,
            null,
            effectiveEstimatedRunDurationMs,
            sweep.EstimatedSweepDurationMs + bundle.Plan.PointSettleMs + bundle.SmbProfile.EstimatedPointConfigurationMs,
            sweepObservation.EstimatedSweepDurationMs,
            sweep.SweepPoints,
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            lockinModel: bundle.OeProfile.NormalizedModel,
            collectorContract: RunConfigLoader.CollectorContractFor(bundle.OeProfile.NormalizedModel),
            decodeFailures: sweepCompletedSnapshot.DecodeFailures,
            effectiveSampleHzPerParameter: sweepCompletedSnapshot.EffectiveSampleHzPerParameter,
            samplesTotal: sweepCompletedSnapshot.SamplesWritten);

        var segment = new SegmentRecord(
            1,
            bundle.Plan.RunId,
            segmentId,
            point.PointId,
            collector.DeviceId,
            segmentStartTs,
            segmentEndTs,
            segmentStartMonotonicNs,
            segmentEndMonotonicNs,
            collector.CollectorArtifactFileName,
            framesInSegment > 0 ? segmentStart.NextFrameSeq : null,
            framesInSegment > 0 ? segmentEnd.NextFrameSeq - 1 : null,
            segmentStart.NextSampleIndex,
            segmentEnd.NextSampleIndex);
        RallArtifactWriter.AppendSegmentRecord(segmentsPath, segment);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "segment_completed", "segment", point.PointId, null, new
        {
            segment_id = segmentId,
            sample_index_start = segment.SampleIndexStart,
            sample_index_end = segment.SampleIndexEnd
        });

        var sweepRecord = ToSweepRecord(sweep);
        RallArtifactWriter.AppendJsonl(
            pointsPath,
            new PointRecord(
                1,
                bundle.Plan.RunId,
                point.PointId,
                index,
                "acquisition_step",
                point.EffectiveMagneticMode,
                m8812Commanded,
                point.TargetBNt,
                baselineCurrentA?.ToArray(),
                deltaCurrentA,
                targetCurrentA,
                sweepRecord,
                new SettleRecord(
                    point.UsesMagneticControl ? "fixed_delay_with_readback" : "no_magnetic_control",
                    settleStarted,
                    settleEnded,
                    "passed",
                    measuredCurrentA)));

        RallArtifactWriter.AppendJsonl(
            deviceStatePath,
            new DeviceStateRecord(
                1,
                bundle.Plan.RunId,
                point.PointId,
                index,
                "acquisition_step",
                point.EffectiveMagneticMode,
                m8812Commanded,
                point.TargetBNt,
                targetCurrentA,
                measuredCurrentA,
                bundle.SmbProfile.ProfileId,
                sweepRecord,
                smbConfigureError,
                new SmbSweepExecutionRecord(sweepObservation.EstimatedSweepDurationMs, sweepObservation.OpcWaitMs, sweepObservation.FallbackUsed),
                new RfExposureWindowRecord(
                    "point_output_on_after_sweep_config_segment_scoped_execute",
                    rfExposureStartedTs,
                    rfExposureEndedTs,
                    rfExposureStartedMonotonicNs,
                    rfExposureEndedMonotonicNs,
                    segmentStartMonotonicNs,
                    segmentEndMonotonicNs),
                new SegmentBindingRecord(
                    segmentId,
                    segment.BlockSeqStart,
                    segment.BlockSeqEnd,
                    segment.SampleIndexStart,
                    segment.SampleIndexEnd),
                bundle.LaserProfile.ProfileId,
                bundle.LaserProfile.Mode,
                bundle.LaserProfile.PowerMw,
                bundle.OeProfile.ProfileId));

        var quality = ComputeQuality(
            bundle,
            point,
            segmentId,
            framesInSegment,
            uniqueBlocksInSegment,
            pointTimeouts,
            pointDecodeFailures,
            segmentEndMonotonicNs,
            segmentEnd.MonotonicNs,
            sweep);
        RallArtifactWriter.AppendJsonl(qualityPath, quality);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "point_completed", "point", point.PointId, null, new
        {
            quality_status = quality.QualityStatus,
            frames_total = quality.FramesTotal,
            collector_health = quality.CollectorHealth,
            timeout_count = quality.TimeoutCount
        });
        return quality;
    }

    private static void ApplySmbFixedProfile(ISmb100aSession smb, Smb100aRunProfile profile)
    {
        foreach (var command in BuildSmbFixedCommands(profile.Fixed))
        {
            SendSmbProfileCommand(smb, profile, command);
        }
    }

    private static void ApplySmbInitialRfOutputOff(ISmb100aSession smb, RunConfigBundle bundle, string eventsPath, long processStart)
    {
        SendSmbProfileCommand(smb, bundle.SmbProfile, Smb100aCommands.OutputOff);
        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "0")
        {
            throw new IOException($"SMB100A initial output state mismatch: expected 0, observed {output}");
        }

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "smb_rf_output_disabled", "rf", null, "smb100a_main", new
        {
            output_state = output.Trim(),
            policy = "run_level_off_point_level_on"
        });
    }

    private static string ConfigureSmbSweepForPointPreparation(ISmb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep, CancellationToken emergencyToken)
    {
        if (!sweep.RfOutputEnabled)
        {
            throw new InvalidOperationException("config-driven runtime requires smb sweep rf_output_enabled=true for run-level RF output policy");
        }

        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.OutputOff, emergencyToken);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeCw, emergencyToken);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.SetFrequencyHz(sweep.StartHz), emergencyToken);
        foreach (var command in sweep.ToCommands())
        {
            SendSmbProfileCommandWithoutErrorCheck(smb, profile, command, emergencyToken);
        }

        emergencyToken.ThrowIfCancellationRequested();
        var error = smb.Query(Smb100aCommands.QuerySystemError);
        if (!Smb100aTcp.ErrorIsClean(error))
        {
            throw new IOException($"SMB100A point sweep configuration error: {error}");
        }

        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "0")
        {
            throw new IOException($"SMB100A output state mismatch before point RF enable: expected 0, observed {output}");
        }

        var frequencyMode = smb.Query(Smb100aCommands.QueryFrequencyMode);
        if (!IsCwFrequencyMode(frequencyMode))
        {
            throw new IOException($"SMB100A frequency mode mismatch before segment: expected CW, observed {frequencyMode}");
        }

        return error;
    }

    private static void SendSmbProfileCommand(ISmb100aSession smb, Smb100aRunProfile profile, string command, CancellationToken emergencyToken = default)
    {
        emergencyToken.ThrowIfCancellationRequested();
        smb.Send(command);
        SleepInterruptibly(profile.CommandSettleMs, emergencyToken);
        if (profile.ErrorCheckAfterWrite)
        {
            emergencyToken.ThrowIfCancellationRequested();
            smb.EnsureNoError();
        }
    }

    private static void SendSmbProfileCommandWithoutErrorCheck(ISmb100aSession smb, Smb100aRunProfile profile, string command, CancellationToken emergencyToken = default)
    {
        emergencyToken.ThrowIfCancellationRequested();
        smb.Send(command);
        SleepInterruptibly(profile.CommandSettleMs, emergencyToken);
    }

    private static SmbSweepObservation ExecuteSmbSweepInsideSegment(ISmb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep, CancellationToken emergencyToken)
    {
        return smb.ExecuteSweep(sweep, emergencyToken);
    }

    private static void PrepareSmbRfForSegment(ISmb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep, CancellationToken emergencyToken)
    {
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.OutputOn, emergencyToken);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeSweep, emergencyToken);
        emergencyToken.ThrowIfCancellationRequested();
        var error = smb.Query(Smb100aCommands.QuerySystemError);
        if (!Smb100aTcp.ErrorIsClean(error))
        {
            throw new IOException($"SMB100A segment RF prepare error: {error}");
        }

        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "1")
        {
            throw new IOException($"SMB100A output state mismatch before sweep execute: expected 1, observed {output}");
        }

        var frequencyMode = smb.Query(Smb100aCommands.QueryFrequencyMode);
        if (frequencyMode.Trim() != "SWE")
        {
            throw new IOException($"SMB100A frequency mode mismatch before sweep execute: expected SWE, observed {frequencyMode}");
        }
    }

    private static void StopSmbRfAfterSegment(ISmb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep, CancellationToken emergencyToken = default)
    {
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.OutputOff, emergencyToken);
        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "0")
        {
            throw new IOException($"SMB100A output state mismatch after sweep execute: expected 0, observed {output}");
        }

        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeCw, emergencyToken);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.SetFrequencyHz(sweep.StartHz), emergencyToken);
        var error = smb.Query(Smb100aCommands.QuerySystemError);
        if (!Smb100aTcp.ErrorIsClean(error))
        {
            throw new IOException($"SMB100A return-to-CW error: {error}");
        }
    }

    private static bool IsCwFrequencyMode(string value)
    {
        var normalized = value.Trim();
        return normalized.Equals("CW", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("FIX", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("FIXED", StringComparison.OrdinalIgnoreCase);
    }

    private static string[] BuildSmbFixedCommands(SmbFixedProfile fixedProfile) =>
    [
        fixedProfile.ModulationEnabled ? "MOD:STAT ON" : "MOD:STAT OFF",
        fixedProfile.FmEnabled ? "FM:STAT ON" : "FM:STAT OFF",
        $"FM:SOUR {fixedProfile.FmSource}",
        $"FM:MODE {fixedProfile.FmMode}",
        $"FM:DEV {fixedProfile.FmDeviationHz.ToString(CultureInfo.InvariantCulture)}Hz",
        fixedProfile.LfOutputEnabled ? "LFO ON" : "LFO OFF",
        $"LFO:VOLT {fixedProfile.LfVoltageMv.ToString(CultureInfo.InvariantCulture)}mV",
        $"LFO:FREQ {fixedProfile.LfFrequencyHz.ToString(CultureInfo.InvariantCulture)}Hz",
        $"LFO:SHAP {fixedProfile.LfShape}",
        $"SOUR:LFO:SIMP {fixedProfile.LfSourceImpedance}"
    ];

    private static QualityRecord ComputeQuality(
        RunConfigBundle bundle,
        RunPointPlan point,
        string segmentId,
        long framesInSegment,
        long uniqueBlocksInSegment,
        long pointTimeouts,
        long pointDecodeFailures,
        ulong segmentEndMonotonicNs,
        ulong lastFrameMonotonicNs,
        SmbSweepSpec sweep)
    {
        var thresholds = bundle.Plan.QualityThresholds;
        var lastFrameAgeMs = framesInSegment > 0
            ? (long)(SaturatingSub(segmentEndMonotonicNs, lastFrameMonotonicNs) / 1_000_000)
            : long.MaxValue;
        long? estimatedFrames = null;
        double? frameCoverage = null;
        long framesUnique;
        long duplicateCount;
        double duplicateRatio;
        string collectorHealth;
        string qualityStatus;

        if (bundle.OeProfile.NormalizedModel == LockinModelNames.Oe1022d)
        {
            var collector = bundle.OeProfile.GetOe1022dCollector();
            estimatedFrames = collector.PollIntervalMs > 0
                ? (long)Math.Ceiling(sweep.EstimatedSweepDurationMs / (double)collector.PollIntervalMs)
                : null;
            frameCoverage = estimatedFrames is > 0 ? framesInSegment / (double)estimatedFrames.Value : null;
            framesUnique = Math.Max(0, uniqueBlocksInSegment);
            duplicateCount = Math.Max(0, framesInSegment - framesUnique);
            duplicateRatio = framesInSegment > 0 ? duplicateCount / (double)framesInSegment : 1.0;
            collectorHealth = pointTimeouts == 0
                ? "clean"
                : pointTimeouts <= thresholds.MaxTimeoutCount ? "recovered_timeout" : "degraded_timeout";
            qualityStatus = framesInSegment == 0
                ? "failed_no_frames"
                : framesUnique == 0 ? "failed_duplicate_only"
                : framesUnique < thresholds.MinFrames ? "failed_min_frames"
                : pointTimeouts > thresholds.MaxTimeoutCount ? "failed_timeout"
                : lastFrameAgeMs > thresholds.MaxLastFrameAgeMs ? "failed_quality"
                : "passed";
        }
        else
        {
            framesUnique = Math.Max(0, uniqueBlocksInSegment);
            duplicateCount = Math.Max(0, framesInSegment - framesUnique);
            duplicateRatio = framesInSegment > 0 ? duplicateCount / (double)framesInSegment : 1.0;
            collectorHealth = pointDecodeFailures > 0
                ? "degraded_decode"
                : pointTimeouts == 0
                ? "clean"
                : pointTimeouts <= thresholds.MaxTimeoutCount ? "recovered_timeout" : "degraded_timeout";
            qualityStatus = framesInSegment == 0
                ? "failed_no_frames"
                : framesUnique == 0
                ? "failed_duplicate_only"
                : framesUnique < thresholds.MinFrames
                ? "failed_min_frames"
                : pointTimeouts > thresholds.MaxTimeoutCount
                ? "failed_timeout"
                : pointDecodeFailures > 0
                ? "failed_decode"
                : lastFrameAgeMs > thresholds.MaxLastFrameAgeMs
                ? "failed_quality"
                : "passed";
        }

        return new QualityRecord(
            1,
            bundle.Plan.RunId,
            point.PointId,
            segmentId,
            framesInSegment,
            framesUnique,
            duplicateCount,
            duplicateRatio,
            pointTimeouts,
            lastFrameAgeMs,
            thresholds.MinFrames,
            estimatedFrames,
            frameCoverage,
            collectorHealth,
            Math.Max(0, thresholds.MaxTimeoutCount - pointTimeouts),
            qualityStatus);
    }

    private static void CleanupMagAxes(IReadOnlyList<ConfigMagAxis> axes)
    {
        foreach (var axis in axes)
        {
            axis.Session.SetCurrent(0.0);
            axis.Session.SetOutput(false);
            Thread.Sleep(100);
            axis.Session.SetLocal();
        }
    }

    private static void EnsureTargetCurrentsWithinM8812Range(string pointId, IReadOnlyList<double> currents)
    {
        for (var index = 0; index < currents.Count; index++)
        {
            EnsureCurrentWithinM8812Range($"point {pointId} target_current_a[{index}]", currents[index]);
        }
    }

    private static void EnsureCurrentWithinM8812Range(string label, double currentA)
    {
        if (currentA < 0.0)
        {
            throw new InvalidOperationException($"{label} must be nonnegative: current={currentA}A");
        }

        if (currentA > 2.0)
        {
            throw new InvalidOperationException($"{label} exceeds M8812 2A limit: current={currentA}A");
        }
    }

    private static SmbSweepRecord ToSweepRecord(SmbSweepSpec sweep) =>
        new(
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            sweep.PowerDbm,
            sweep.SweepMode,
            sweep.Spacing,
            sweep.Shape,
            sweep.TriggerSource,
            sweep.OutputVoltageStartV,
            sweep.OutputVoltageStopV,
            sweep.RfOutputEnabled);
}
