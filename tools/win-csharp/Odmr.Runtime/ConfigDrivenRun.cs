using System.Diagnostics;
using System.Globalization;
using Odmr.Artifacts;
using Odmr.Devices;

namespace Odmr.Runtime;

public sealed record ConfigDrivenRunOptions(
    string StationPath,
    string CalibrationPath,
    string PlanPath,
    string SmbProfilePath,
    string OeProfilePath,
    string LaserProfilePath,
    string OutDir,
    IProgress<RunProgressEvent>? Progress = null,
    CancellationToken CancellationToken = default);

internal sealed record ConfigMagAxis(
    string AxisId,
    StationDeviceSpec Device,
    M8812AxisSession Session);

public static class ConfigDrivenRun
{
    private const string RuntimeVersion = "win-csharp-config-runtime-v1";

    public static RunSummaryRecord Execute(ConfigDrivenRunOptions options)
    {
        var bundle = RunConfigLoader.Load(
            options.StationPath,
            options.CalibrationPath,
            options.PlanPath,
            options.SmbProfilePath,
            options.OeProfilePath,
            options.LaserProfilePath);

        PrepareRunDirectory(options.OutDir);
        WriteSnapshots(options.OutDir, bundle);

        var rawPath = Path.Combine(options.OutDir, "raw", "oe1022d.rall");
        var indexPath = Path.Combine(options.OutDir, "raw", "oe1022d.frames.idx.jsonl");
        var segmentsPath = Path.Combine(options.OutDir, "segments.jsonl");
        var pointsPath = Path.Combine(options.OutDir, "points.jsonl");
        var qualityPath = Path.Combine(options.OutDir, "quality.jsonl");
        var deviceStatePath = Path.Combine(options.OutDir, "device_state.jsonl");
        var eventsPath = Path.Combine(options.OutDir, "events.jsonl");
        var baselinePath = Path.Combine(options.OutDir, "baseline_snapshot.json");
        var manifestPath = Path.Combine(options.OutDir, "run_manifest.json");
        var summaryPath = Path.Combine(options.OutDir, "summary.json");

        var startedAt = UtcNowString();
        var processStart = Stopwatch.GetTimestamp();
        var manifest = new RunManifestRecord(
            1,
            bundle.Plan.RunId,
            startedAt,
            bundle.Plan.Operator,
            bundle.Station.StationId,
            RuntimeVersion,
            bundle.Calibration.CalibrationId,
            "running",
            bundle.SmbProfile.ProfileId,
            bundle.OeProfile.ProfileId,
            bundle.LaserProfile.ProfileId,
            bundle.ResolvedPlan.SourceKind,
            bundle.ResolvedPlan.ResolvedPointCount,
            bundle.ResolvedPlan.EstimatedRunDurationMs);
        RallArtifactWriter.WritePrettyJson(manifestPath, manifest);

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "run_opened", "run", null, null, new
        {
            output_dir = options.OutDir,
            plan_source_kind = bundle.ResolvedPlan.SourceKind,
            resolved_point_count = bundle.ResolvedPlan.ResolvedPointCount
        });
        ReportProgress(
            options,
            RuntimeState.RunOpened,
            "run_opened",
            $"Run opened: {bundle.Plan.RunId}",
            null,
            null,
            bundle.ResolvedPlan.ResolvedPointCount,
            null,
            null,
            null,
            null,
            null);

        ApplyOeFixedProfile(bundle, eventsPath, processStart);

        using var collector = new OeRallCollector(
            bundle.Connections.OeResource,
            bundle.Connections.OeBaudRate,
            rawPath,
            indexPath,
            processStart);
        using var smb = Smb100aTcp.Open(bundle.Connections.SmbHost, bundle.Connections.SmbPort);
        var magAxes = OpenMagAxes(bundle);
        CniLaserSession? laser = null;
        var pointsPassed = 0;
        var pointsFailed = 0;
        string? failure = null;

        try
        {
            collector.Start();
            collector.WaitForFirstFrame(TimeSpan.FromSeconds(5));
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "collector_started", "collector", null, "oe1022d_main", new
            {
                frame_exact_bytes = bundle.OeProfile.Collector.FrameExactBytes,
                rall_post_write_delay_ms = bundle.OeProfile.Collector.RallPostWriteDelayMs,
                ring_capacity_frames = bundle.OeProfile.Collector.RingCapacityFrames
            });
            var collectorSnapshot = collector.Snapshot();
            ReportProgress(
                options,
                RuntimeState.CollectorRunning,
                "collector_started",
                "OE RALL collector started",
                null,
                null,
                bundle.ResolvedPlan.ResolvedPointCount,
                collectorSnapshot.Stats.FramesOk,
                collectorSnapshot.Stats.TimeoutCount,
                collectorSnapshot.Stats.RawLenBadCount,
                collectorSnapshot.PacketCounter.DeltaGt1Count,
                null);

            laser = ApplyLaserProfile(bundle, eventsPath, processStart);
            var baseline = LockBaseline(bundle, magAxes, baselinePath);
            var baselineCurrentA = baseline.Axes
                .Select(axis => axis.LockedZeroOffsetCurrentA ?? axis.ZeroOffsetSetpointA)
                .ToArray();

            ApplySmbFixedProfile(smb, bundle.SmbProfile);
            ApplySmbRunRfOutput(smb, bundle, eventsPath, processStart);

            for (var index = 0; index < bundle.ResolvedPlan.Points.Count; index++)
            {
                if (options.CancellationToken.IsCancellationRequested)
                {
                    var stoppingSnapshot = collector.Snapshot();
                    AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "stop_after_current_point_requested", "run", null, null, new
                    {
                        stopped_before_point_index = index
                    });
                    ReportProgress(
                        options,
                        RuntimeState.Stopping,
                        "stop_after_current_point_requested",
                        "Stop requested before next point",
                        null,
                        index,
                        bundle.ResolvedPlan.ResolvedPointCount,
                        stoppingSnapshot.Stats.FramesOk,
                        stoppingSnapshot.Stats.TimeoutCount,
                        stoppingSnapshot.Stats.RawLenBadCount,
                        stoppingSnapshot.PacketCounter.DeltaGt1Count,
                        null);
                    break;
                }

                var point = bundle.ResolvedPlan.Points[index];
                var beforePointSnapshot = collector.Snapshot();
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "point_prepare_started", "point", point.PointId, null, new
                {
                    index,
                    target_b_nt = point.TargetBNt
                });
                ReportProgress(
                    options,
                    RuntimeState.PointRunning,
                    "point_prepare_started",
                    $"Point {index + 1}/{bundle.ResolvedPlan.ResolvedPointCount}: {point.PointId}",
                    point.PointId,
                    index,
                    bundle.ResolvedPlan.ResolvedPointCount,
                    beforePointSnapshot.Stats.FramesOk,
                    beforePointSnapshot.Stats.TimeoutCount,
                    beforePointSnapshot.Stats.RawLenBadCount,
                    beforePointSnapshot.PacketCounter.DeltaGt1Count,
                    null);

                var quality = RunPoint(
                    bundle,
                    index,
                    point,
                    baselineCurrentA,
                    magAxes,
                    smb,
                    collector,
                    segmentsPath,
                    pointsPath,
                    qualityPath,
                    deviceStatePath,
                    eventsPath,
                    processStart);
                if (quality.QualityStatus == "passed")
                {
                    pointsPassed++;
                }
                else
                {
                    pointsFailed++;
                    if (!string.Equals(bundle.Plan.FailurePolicy, "continue", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                var afterPointSnapshot = collector.Snapshot();
                ReportProgress(
                    options,
                    RuntimeState.PointRunning,
                    "point_completed",
                    $"Point completed: {point.PointId} ({quality.QualityStatus})",
                    point.PointId,
                    index,
                    bundle.ResolvedPlan.ResolvedPointCount,
                    afterPointSnapshot.Stats.FramesOk,
                    afterPointSnapshot.Stats.TimeoutCount,
                    afterPointSnapshot.Stats.RawLenBadCount,
                    afterPointSnapshot.PacketCounter.DeltaGt1Count,
                    quality.QualityStatus);
            }
        }
        catch (Exception ex)
        {
            failure = ex.Message;
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "run_failed", "run", null, null, new { error = ex.Message });
            ReportProgress(
                options,
                RuntimeState.Failed,
                "run_failed",
                ex.Message,
                null,
                null,
                bundle.ResolvedPlan.ResolvedPointCount,
                null,
                null,
                null,
                null,
                null);
        }
        finally
        {
            Exception? cleanupFailure = null;
            if (laser is not null)
            {
                try
                {
                    laser.OutputOff();
                }
                catch (Exception ex)
                {
                    cleanupFailure ??= ex;
                }
                finally
                {
                    try
                    {
                        laser.Dispose();
                    }
                    catch (Exception ex)
                    {
                        cleanupFailure ??= ex;
                    }
                }
            }

            try
            {
                CleanupMagAxes(magAxes);
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }
            finally
            {
                foreach (var axis in magAxes)
                {
                    try
                    {
                        axis.Session.Dispose();
                    }
                    catch (Exception ex)
                    {
                        cleanupFailure ??= ex;
                    }
                }
            }

            try
            {
                smb.Cleanup();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

            try
            {
                collector.Stop();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

            var snapshot = collector.Snapshot();
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "collector_stopped", "collector", null, "oe1022d_main", new
            {
                frames_total = snapshot.Stats.FramesOk,
                timeout_count = snapshot.Stats.TimeoutCount,
                raw_len_bad_count = snapshot.Stats.RawLenBadCount
            });
            ReportProgress(
                options,
                RuntimeState.Stopping,
                "collector_stopped",
                "OE RALL collector stopped",
                null,
                null,
                bundle.ResolvedPlan.ResolvedPointCount,
                snapshot.Stats.FramesOk,
                snapshot.Stats.TimeoutCount,
                snapshot.Stats.RawLenBadCount,
                snapshot.PacketCounter.DeltaGt1Count,
                null);

            if (cleanupFailure is null)
            {
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "cleanup_completed", "cleanup", null, null, new { });
            }
            else
            {
                failure = failure is null ? cleanupFailure.Message : $"{failure}; cleanup: {cleanupFailure.Message}";
            }
        }

        var finalSnapshot = collector.Snapshot();
        var endedAt = UtcNowString();
        var rawBytesWritten = File.Exists(rawPath) ? new FileInfo(rawPath).Length : 0;
        var status = failure is not null
            ? "failed"
            : pointsFailed > 0 ? "completed_with_failed_points" : "completed";

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, status == "failed" ? "run_failed" : "run_completed", "run", null, null, new
        {
            status,
            points_passed = pointsPassed,
            points_failed = pointsFailed,
            failure
        });
        ReportProgress(
            options,
            status == "failed" ? RuntimeState.Failed : RuntimeState.Completed,
            status == "failed" ? "run_failed" : "run_completed",
            $"Run {status}: {bundle.Plan.RunId}",
            null,
            null,
            bundle.ResolvedPlan.ResolvedPointCount,
            finalSnapshot.Stats.FramesOk,
            finalSnapshot.Stats.TimeoutCount,
            finalSnapshot.Stats.RawLenBadCount,
            finalSnapshot.PacketCounter.DeltaGt1Count,
            null);

        RallArtifactWriter.WritePrettyJson(
            manifestPath,
            manifest with { Status = status });

        var summary = new RunSummaryRecord(
            bundle.Plan.RunId,
            status,
            bundle.ResolvedPlan.ResolvedPointCount,
            pointsPassed,
            pointsFailed,
            finalSnapshot.Stats.FramesOk,
            startedAt,
            endedAt,
            failure,
            finalSnapshot.Stats.ReadAttempts,
            finalSnapshot.Stats.TimeoutCount,
            finalSnapshot.Stats.RawLenBadCount,
            rawBytesWritten,
            rawBytesWritten == finalSnapshot.Stats.FramesOk * Oe1022dDefaults.RallFrameBytes,
            finalSnapshot.PacketCounter.ToSummary());
        RallArtifactWriter.WritePrettyJson(summaryPath, summary);

        if (status == "failed")
        {
            throw new InvalidOperationException($"run-execute failed: {failure}");
        }

        return summary;
    }

    private static void PrepareRunDirectory(string outDir)
    {
        Directory.CreateDirectory(Path.Combine(outDir, "raw"));
        foreach (var path in new[]
        {
            "segments.jsonl",
            "points.jsonl",
            "quality.jsonl",
            "device_state.jsonl",
            "events.jsonl",
            "point_fields.jsonl"
        })
        {
            var fullPath = Path.Combine(outDir, path);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }

    private static void WriteSnapshots(string outDir, RunConfigBundle bundle)
    {
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "station_snapshot.json"), bundle.Station);
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "plan_snapshot.json"), bundle.BuildPlanSnapshot());
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "calibration_snapshot.json"), bundle.Calibration);
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "smb_profile_snapshot.json"), bundle.SmbProfile);
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "oe_profile_snapshot.json"), bundle.OeProfile);
        RallArtifactWriter.WritePrettyJson(Path.Combine(outDir, "laser_profile_snapshot.json"), bundle.LaserProfile);
    }

    private static List<ConfigMagAxis> OpenMagAxes(RunConfigBundle bundle)
    {
        var axes = new List<ConfigMagAxis>
        {
            OpenMagAxis(bundle, "mag_x", bundle.Connections.XPort),
            OpenMagAxis(bundle, "mag_y", bundle.Connections.YPort),
            OpenMagAxis(bundle, "mag_z", bundle.Connections.ZPort)
        };

        foreach (var axis in axes)
        {
            axis.Session.Clear();
            var idn = axis.Session.QueryIdn();
            foreach (var token in axis.Device.Identity?.ContainsAll ?? Array.Empty<string>())
            {
                if (!idn.Contains(token, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{axis.AxisId} identity mismatch: expected token `{token}`, idn={idn}");
                }
            }

            axis.Session.SetRemote();
        }

        return axes;
    }

    private static ConfigMagAxis OpenMagAxis(RunConfigBundle bundle, string axisId, string port)
    {
        var device = bundle.Station.Devices.First(device => device.DeviceId == axisId);
        return new ConfigMagAxis(axisId, device, M8812Serial.Open(port));
    }

    private static CniLaserSession? ApplyLaserProfile(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var mode = bundle.LaserProfile.Mode;
        if (mode == "off_background")
        {
            if (bundle.Connections.LaserPort is null)
            {
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, null, new
                {
                    mode,
                    power_mw = bundle.LaserProfile.PowerMw,
                    status = "no_laser_device"
                });
                return null;
            }

            var laser = CniLaserSerial.Open(bundle.Connections.LaserPort);
            laser.OutputOff();
            Thread.Sleep(bundle.LaserProfile.SettleMs);
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, "cni_laser_main", new
            {
                mode,
                power_mw = bundle.LaserProfile.PowerMw
            });
            return laser;
        }

        if (mode != "on_background")
        {
            throw new InvalidOperationException($"unsupported laser profile mode: {mode}");
        }

        if (bundle.Connections.LaserPort is null)
        {
            throw new InvalidOperationException("laser profile requires on_background, but station has no laser port");
        }

        var session = CniLaserSerial.Open(bundle.Connections.LaserPort);
        session.SetPowerMw(bundle.LaserProfile.PowerMw);
        Thread.Sleep(bundle.LaserProfile.SettleMs);
        session.OutputOn();
        Thread.Sleep(bundle.LaserProfile.SettleMs);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, "cni_laser_main", new
        {
            mode,
            power_mw = bundle.LaserProfile.PowerMw
        });
        return session;
    }

    private static void ApplyOeFixedProfile(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        using var oe = Oe1022dVisa.Open(bundle.Connections.OeResource, bundle.Connections.OeBaudRate);
        var commands = BuildOeFixedCommands(bundle.OeProfile.Fixed);
        foreach (var command in commands)
        {
            oe.SendAsciiCommand(command);
            Thread.Sleep(bundle.OeProfile.CommandSettleMs);
        }

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "oe_profile_applied", "profile", null, "oe1022d_main", new
        {
            profile_id = bundle.OeProfile.ProfileId,
            fixed_commands_sent = true,
            command_count = commands.Length,
            channel = bundle.OeProfile.Fixed.Channel
        });
    }

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
        IReadOnlyList<double> baselineCurrentA,
        IReadOnlyList<ConfigMagAxis> magAxes,
        Smb100aSession smb,
        OeRallCollector collector,
        string segmentsPath,
        string pointsPath,
        string qualityPath,
        string deviceStatePath,
        string eventsPath,
        long processStart)
    {
        var deltaCurrentA = bundle.Calibration.DeltaCurrentA(point.TargetBNt);
        var targetCurrentA = bundle.Calibration.TargetCurrentA(baselineCurrentA, point.TargetBNt);
        EnsureNonnegativeTargetCurrents(point.PointId, targetCurrentA);

        for (var axisIndex = 0; axisIndex < magAxes.Count; axisIndex++)
        {
            magAxes[axisIndex].Session.SetCurrent(targetCurrentA[axisIndex]);
            magAxes[axisIndex].Session.SetOutput(true);
        }

        var settleStarted = UtcNowString();
        Thread.Sleep(bundle.Plan.PointSettleMs);
        var measuredCurrentA = magAxes.Select(axis => axis.Session.MeasureCurrent()).ToArray();
        var settleEnded = UtcNowString();

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "point_stable", "settle", point.PointId, null, new
        {
            target_current_a = targetCurrentA,
            measured_current_a = measuredCurrentA
        });

        var sweep = bundle.SmbProfile.DefaultSweep.ApplyOverride(point.SmbOverride);
        var smbConfigureError = ConfigureSmbSweepForPointPreparation(smb, bundle.SmbProfile, sweep);

        var segmentId = $"seg_{point.PointId}_0000";
        var timeoutsBefore = collector.Snapshot().Stats.TimeoutCount;
        var segmentStartTs = UtcNowString();
        var segmentStartMonotonicNs = MonotonicNsSince(processStart);
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
            rf_output_policy = "run_level_on",
            sweep_window_policy = "segment_scoped_sweep"
        });

        var rfExposureStartedTs = UtcNowString();
        var rfExposureStartedMonotonicNs = MonotonicNsSince(processStart);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_exposure_started", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            output_state = "run_level_on",
            frequency_mode = "SWE",
            start_hz = sweep.StartHz
        });
        var sweepObservation = ExecuteSmbSweepInsideSegment(smb, bundle.SmbProfile, sweep);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_sweep_executed", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            estimated_sweep_duration_ms = sweepObservation.EstimatedSweepDurationMs,
            opc_wait_ms = sweepObservation.OpcWaitMs,
            fallback_used = sweepObservation.FallbackUsed
        });
        ReturnSmbToCwStart(smb, bundle.SmbProfile, sweep);
        var rfExposureEndedTs = UtcNowString();
        var rfExposureEndedMonotonicNs = MonotonicNsSince(processStart);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "rf_exposure_ended", "rf", point.PointId, "smb100a_main", new
        {
            segment_id = segmentId,
            frequency_mode = "CW",
            cw_frequency_hz = sweep.StartHz
        });

        var segmentEndTs = UtcNowString();
        var segmentEndMonotonicNs = MonotonicNsSince(processStart);
        var segmentEnd = collector.Cursor();
        var timeoutsAfter = collector.Snapshot().Stats.TimeoutCount;
        var pointTimeouts = timeoutsAfter - timeoutsBefore;
        var framesInSegment = segmentEnd.NextFrameSeq - segmentStart.NextFrameSeq;
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "sweep_completed", "sweep", point.PointId, null, new
        {
            segment_id = segmentId,
            estimated_sweep_duration_ms = sweepObservation.EstimatedSweepDurationMs,
            opc_wait_ms = sweepObservation.OpcWaitMs,
            fallback_used = sweepObservation.FallbackUsed,
            smb_configure_error = smbConfigureError,
            rf_exposure_started_ts = rfExposureStartedTs,
            rf_exposure_ended_ts = rfExposureEndedTs,
            rf_exposure_started_monotonic_ns = rfExposureStartedMonotonicNs,
            rf_exposure_ended_monotonic_ns = rfExposureEndedMonotonicNs
        });

        var segment = new SegmentRecord(
            1,
            bundle.Plan.RunId,
            segmentId,
            point.PointId,
            "oe1022d_main",
            segmentStartTs,
            segmentEndTs,
            segmentStartMonotonicNs,
            segmentEndMonotonicNs,
            "raw/oe1022d.rall",
            segmentStart.NextRawOffset,
            segmentEnd.NextRawOffset,
            framesInSegment > 0 ? segmentStart.NextFrameSeq : null,
            framesInSegment > 0 ? segmentEnd.NextFrameSeq - 1 : null);
        RallArtifactWriter.AppendSegmentRecord(segmentsPath, segment);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "segment_completed", "segment", point.PointId, null, new
        {
            segment_id = segmentId,
            raw_offset_start = segment.RawOffsetStart,
            raw_offset_end = segment.RawOffsetEnd
        });

        var sweepRecord = ToSweepRecord(sweep);
        RallArtifactWriter.AppendJsonl(
            pointsPath,
            new PointRecord(
                1,
                bundle.Plan.RunId,
                point.PointId,
                index,
                point.TargetBNt,
                baselineCurrentA.ToArray(),
                deltaCurrentA,
                targetCurrentA,
                sweepRecord,
                new SettleRecord("fixed_delay_with_readback", settleStarted, settleEnded, "passed", measuredCurrentA)));

        RallArtifactWriter.AppendJsonl(
            deviceStatePath,
            new DeviceStateRecord(
                1,
                bundle.Plan.RunId,
                point.PointId,
                index,
                point.TargetBNt,
                targetCurrentA,
                measuredCurrentA,
                bundle.SmbProfile.ProfileId,
                sweepRecord,
                smbConfigureError,
                new SmbSweepExecutionRecord(sweepObservation.EstimatedSweepDurationMs, sweepObservation.OpcWaitMs, sweepObservation.FallbackUsed),
                new RfExposureWindowRecord(
                    "run_level_output_on_segment_scoped_sweep",
                    rfExposureStartedTs,
                    rfExposureEndedTs,
                    rfExposureStartedMonotonicNs,
                    rfExposureEndedMonotonicNs,
                    segmentStartMonotonicNs,
                    segmentEndMonotonicNs),
                new SegmentBindingRecord(
                    segmentId,
                    segment.FrameSeqStart,
                    segment.FrameSeqEnd,
                    segment.RawOffsetStart,
                    segment.RawOffsetEnd),
                bundle.LaserProfile.ProfileId,
                bundle.LaserProfile.Mode,
                bundle.LaserProfile.PowerMw,
                bundle.OeProfile.ProfileId));

        var quality = ComputeQuality(bundle, point, segmentId, framesInSegment, pointTimeouts, segmentEndMonotonicNs, segmentEnd.MonotonicNs, sweep);
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

    private static void ApplySmbFixedProfile(Smb100aSession smb, Smb100aRunProfile profile)
    {
        foreach (var command in BuildSmbFixedCommands(profile.Fixed))
        {
            SendSmbProfileCommand(smb, profile, command);
        }
    }

    private static void ApplySmbRunRfOutput(Smb100aSession smb, RunConfigBundle bundle, string eventsPath, long processStart)
    {
        SendSmbProfileCommand(smb, bundle.SmbProfile, Smb100aCommands.OutputOn);
        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "1")
        {
            throw new IOException($"SMB100A run-level output state mismatch: expected 1, observed {output}");
        }

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "smb_rf_output_enabled", "rf", null, "smb100a_main", new
        {
            output_state = output.Trim(),
            policy = "run_level_on"
        });
    }

    private static string ConfigureSmbSweepForPointPreparation(Smb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep)
    {
        if (!sweep.RfOutputEnabled)
        {
            throw new InvalidOperationException("config-driven runtime requires smb sweep rf_output_enabled=true for run-level RF output policy");
        }

        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeCw);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.SetFrequencyHz(sweep.StartHz));
        foreach (var command in sweep.ToCommands())
        {
            SendSmbProfileCommandWithoutErrorCheck(smb, profile, command);
        }

        var error = smb.Query(Smb100aCommands.QuerySystemError);
        if (!Smb100aTcp.ErrorIsClean(error))
        {
            throw new IOException($"SMB100A point sweep configuration error: {error}");
        }

        var output = smb.Query(Smb100aCommands.QueryOutput);
        if (output.Trim() != "1")
        {
            throw new IOException($"SMB100A output state mismatch before segment: expected 1, observed {output}");
        }

        var frequencyMode = smb.Query(Smb100aCommands.QueryFrequencyMode);
        if (!IsCwFrequencyMode(frequencyMode))
        {
            throw new IOException($"SMB100A frequency mode mismatch before segment: expected CW, observed {frequencyMode}");
        }

        return error;
    }

    private static void SendSmbProfileCommand(Smb100aSession smb, Smb100aRunProfile profile, string command)
    {
        smb.Send(command);
        Thread.Sleep(profile.CommandSettleMs);
        if (profile.ErrorCheckAfterWrite)
        {
            smb.EnsureNoError();
        }
    }

    private static void SendSmbProfileCommandWithoutErrorCheck(Smb100aSession smb, Smb100aRunProfile profile, string command)
    {
        smb.Send(command);
        Thread.Sleep(profile.CommandSettleMs);
    }

    private static SmbSweepObservation ExecuteSmbSweepInsideSegment(Smb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep)
    {
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeSweep);
        return smb.ExecuteSweep(sweep);
    }

    private static void ReturnSmbToCwStart(Smb100aSession smb, Smb100aRunProfile profile, SmbSweepSpec sweep)
    {
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.FrequencyModeCw);
        SendSmbProfileCommandWithoutErrorCheck(smb, profile, Smb100aCommands.SetFrequencyHz(sweep.StartHz));
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

    private static string[] BuildOeFixedCommands(Oe1022dFixedProfile fixedProfile) =>
    [
        Oe1022dCommands.SetInputSource(fixedProfile.Channel, fixedProfile.InputSource),
        Oe1022dCommands.SetInputGrounding(fixedProfile.Channel, fixedProfile.InputGrounding),
        Oe1022dCommands.SetInputCoupling(fixedProfile.Channel, fixedProfile.InputCoupling),
        Oe1022dCommands.SetLineNotchFilter(fixedProfile.Channel, fixedProfile.LineNotchFilter),
        Oe1022dCommands.SetReferenceSource(fixedProfile.Channel, fixedProfile.ReferenceSource),
        Oe1022dCommands.SetReferenceSlope(fixedProfile.Channel, fixedProfile.ReferenceSlope),
        Oe1022dCommands.SetPhaseDeg(fixedProfile.Channel, fixedProfile.PhaseDeg),
        Oe1022dCommands.SetHarmonic(fixedProfile.Channel, 1, fixedProfile.Harmonic1),
        Oe1022dCommands.SetHarmonic(fixedProfile.Channel, 2, fixedProfile.Harmonic2),
        Oe1022dCommands.SetDynamicReserve(fixedProfile.Channel, fixedProfile.DynamicReserve),
        Oe1022dCommands.SetSensitivityIndex(fixedProfile.Channel, fixedProfile.SensitivityIndex),
        Oe1022dCommands.SetTimeConstantIndex(fixedProfile.Channel, fixedProfile.TimeConstantIndex),
        Oe1022dCommands.SetFilterSlope(fixedProfile.Channel, fixedProfile.FilterSlope),
        Oe1022dCommands.SetSyncFilter(fixedProfile.Channel, fixedProfile.SyncFilter),
        Oe1022dCommands.SetSineOutputMode(fixedProfile.Channel, fixedProfile.SineOutputMode),
        Oe1022dCommands.SetSineOutputVoltageVrms(fixedProfile.Channel, fixedProfile.SineOutputVoltageVrms)
    ];

    private static QualityRecord ComputeQuality(
        RunConfigBundle bundle,
        RunPointPlan point,
        string segmentId,
        long framesInSegment,
        long pointTimeouts,
        ulong segmentEndMonotonicNs,
        ulong lastFrameMonotonicNs,
        SmbSweepSpec sweep)
    {
        var thresholds = bundle.Plan.QualityThresholds;
        var lastFrameAgeMs = framesInSegment > 0
            ? (long)(SaturatingSub(segmentEndMonotonicNs, lastFrameMonotonicNs) / 1_000_000)
            : long.MaxValue;
        var estimatedFrames = bundle.OeProfile.Collector.PollIntervalMs > 0
            ? (long)Math.Ceiling(sweep.EstimatedSweepDurationMs / (double)bundle.OeProfile.Collector.PollIntervalMs)
            : (long?)null;
        var frameCoverage = estimatedFrames is > 0 ? framesInSegment / (double)estimatedFrames.Value : (double?)null;
        var collectorHealth = pointTimeouts == 0
            ? "clean"
            : pointTimeouts <= thresholds.MaxTimeoutCount ? "recovered_timeout" : "degraded_timeout";
        var qualityStatus = framesInSegment == 0
            ? "failed_no_frames"
            : framesInSegment < thresholds.MinFrames ? "failed_min_frames"
            : pointTimeouts > thresholds.MaxTimeoutCount ? "failed_timeout"
            : lastFrameAgeMs > thresholds.MaxLastFrameAgeMs ? "failed_quality"
            : "passed";

        return new QualityRecord(
            1,
            bundle.Plan.RunId,
            point.PointId,
            segmentId,
            framesInSegment,
            framesInSegment,
            0,
            0.0,
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

    private static void EnsureNonnegativeTargetCurrents(string pointId, IReadOnlyList<double> currents)
    {
        for (var index = 0; index < currents.Count; index++)
        {
            if (currents[index] < 0.0)
            {
                throw new InvalidOperationException($"point {pointId} target current must be nonnegative: axis_index={index}, current={currents[index]}A");
            }
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

    private static void AppendEvent(
        string eventsPath,
        long processStart,
        string runId,
        string eventName,
        string phase,
        string? pointId,
        string? device,
        object data)
    {
        RallArtifactWriter.AppendJsonl(
            eventsPath,
            new EventRecord(UtcNowString(), MonotonicNsSince(processStart), eventName, runId, pointId, device, phase, data));
    }

    private static void ReportProgress(
        ConfigDrivenRunOptions options,
        RuntimeState state,
        string eventName,
        string message,
        string? pointId,
        int? pointIndex,
        int? pointsTotal,
        long? framesTotal,
        long? timeoutCount,
        long? rawLenBadCount,
        long? deltaGt1Count,
        string? qualityStatus)
    {
        options.Progress?.Report(new RunProgressEvent(
            state,
            eventName,
            message,
            pointId,
            pointIndex,
            pointsTotal,
            framesTotal,
            timeoutCount,
            rawLenBadCount,
            deltaGt1Count,
            qualityStatus));
    }

    private static string UtcNowString() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

    private static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }

    private static ulong SaturatingSub(ulong lhs, ulong rhs) =>
        lhs >= rhs ? lhs - rhs : 0UL;
}
