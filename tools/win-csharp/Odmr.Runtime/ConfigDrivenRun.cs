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
    CancellationToken CancellationToken = default,
    CancellationToken EmergencyStopToken = default,
    int StartPointIndex = 0,
    int CompletedPointsOffset = 0,
    long? EstimatedRunDurationMsOverride = null);

internal sealed class EmergencyStopException : OperationCanceledException
{
    public EmergencyStopException()
        : base("emergency stop requested")
    {
    }
}

public static partial class ConfigDrivenRun
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

        return Execute(bundle, options);
    }

    public static RunSummaryRecord Execute(RunConfigBundle bundle, ConfigDrivenRunOptions options)
    {
        if (options.StartPointIndex < 0 || options.StartPointIndex > bundle.ResolvedPlan.Points.Count)
        {
            throw new InvalidOperationException($"invalid start point index: {options.StartPointIndex}");
        }

        var lockinModel = bundle.OeProfile.NormalizedModel;
        var collectorContract = RunConfigLoader.CollectorContractFor(lockinModel);
        var effectiveEstimatedRunDurationMs = options.EstimatedRunDurationMsOverride ?? bundle.ResolvedPlan.EstimatedRunDurationMs;

        PrepareRunDirectory(options.OutDir);
        WriteSnapshots(options.OutDir, bundle);

        var collectorFileName = lockinModel == LockinModelNames.Oe1300
            ? "collector_blocks.jsonl"
            : "collector_frames.jsonl";
        var collectorFramesPath = Path.Combine(options.OutDir, collectorFileName);
        var parameterValuesPath = Path.Combine(options.OutDir, "parameter_values.csv");
        var sampleValuesPath = Path.Combine(options.OutDir, "sample_values.csv");
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
            lockinModel,
            collectorContract,
            RuntimeVersion,
            bundle.Calibration.CalibrationId,
            "running",
            bundle.SmbProfile.ProfileId,
            bundle.OeProfile.ProfileId,
            bundle.LaserProfile.ProfileId,
            bundle.ResolvedPlan.SourceKind,
            bundle.ResolvedPlan.ResolvedPointCount,
            effectiveEstimatedRunDurationMs);
        RallArtifactWriter.WritePrettyJson(manifestPath, manifest);

        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "run_opened", "run", null, null, new
            {
                output_dir = options.OutDir,
                plan_source_kind = bundle.ResolvedPlan.SourceKind,
                resolved_point_count = bundle.ResolvedPlan.ResolvedPointCount,
                lockin_model = lockinModel,
                collector_contract = collectorContract
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
            null,
            effectiveEstimatedRunDurationMs,
            bundle.ResolvedPlan.EstimatedPointDurationMs,
            bundle.ResolvedPlan.EstimatedSweep?.SweepDurationMs,
            bundle.ResolvedPlan.EstimatedSweep?.SweepPoints,
            null,
            null,
            null,
            null,
            lockinModel: lockinModel,
            collectorContract: collectorContract);

        var resolvedConnections = ResolveRuntimeConnections(bundle, eventsPath, processStart);

        ApplyOeFixedProfile(bundle, resolvedConnections, eventsPath, processStart);

        using ILockinCollector collector = CreateLockinCollector(
            bundle,
            resolvedConnections,
            collectorFramesPath,
            parameterValuesPath,
            sampleValuesPath,
            processStart);
        using var smb = OpenSmbSession(resolvedConnections);
        var magAxes = bundle.ResolvedPlan.RequiresMagneticControl ? OpenMagAxes(bundle, resolvedConnections) : [];
        CniLaserSession? laser = null;
        var pointsPassed = options.CompletedPointsOffset;
        var pointsFailed = 0;
        string? failure = null;
        var aborted = false;
        var paused = false;
        var emergencyEventRecorded = false;

        try
        {
            collector.Start();
            collector.WaitForFirstFrame(TimeSpan.FromSeconds(5));
            var collectorSnapshot = collector.Snapshot();
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "collector_started", "collector", null, collector.DeviceId, new
            {
                collector_file = collector.CollectorArtifactFileName,
                lockin_model = collector.LockinModel,
                collector_contract = collector.CollectorContract,
                read_contract = BuildCollectorStartedData(bundle)
            });
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
                collectorSnapshot.PacketCounter?.DeltaGt1Count,
                null,
                lockinModel: lockinModel,
                collectorContract: collectorContract,
                decodeFailures: collectorSnapshot.DecodeFailures,
                effectiveSampleHzPerParameter: collectorSnapshot.EffectiveSampleHzPerParameter);

            laser = ApplyLaserProfile(bundle, eventsPath, processStart);
            double[]? baselineCurrentA = null;
            if (bundle.ResolvedPlan.RequiresMagneticControl)
            {
                var baseline = LockBaseline(bundle, magAxes, baselinePath);
                baselineCurrentA = baseline.Axes
                    .Select(axis => axis.LockedZeroOffsetCurrentA ?? axis.ZeroOffsetSetpointA)
                    .ToArray();
            }
            else
            {
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "magnetic_control_skipped", "magnetic", null, null, new
                {
                    reason = "all_points_magnetic_mode_none"
                });
            }

            ApplySmbFixedProfile(smb, bundle.SmbProfile);
            ApplySmbInitialRfOutputOff(smb, bundle, eventsPath, processStart);

            for (var index = options.StartPointIndex; index < bundle.ResolvedPlan.Points.Count; index++)
            {
                ThrowIfEmergencyRequested(options);
                if (options.CancellationToken.IsCancellationRequested)
                {
                    paused = true;
                    var stoppingSnapshot = collector.Snapshot();
                    AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "pause_after_current_point_requested", "run", null, null, new
                    {
                        stopped_before_point_index = index
                    });
                    ReportProgress(
                        options,
                        RuntimeState.Stopping,
                        "pause_after_current_point_requested",
                        "Pause requested before next point",
                        null,
                        index,
                        bundle.ResolvedPlan.ResolvedPointCount,
                        stoppingSnapshot.Stats.FramesOk,
                        stoppingSnapshot.Stats.TimeoutCount,
                        stoppingSnapshot.Stats.RawLenBadCount,
                        stoppingSnapshot.PacketCounter?.DeltaGt1Count,
                        null,
                        lockinModel: lockinModel,
                        collectorContract: collectorContract,
                        decodeFailures: stoppingSnapshot.DecodeFailures,
                        effectiveSampleHzPerParameter: stoppingSnapshot.EffectiveSampleHzPerParameter);
                    break;
                }

                var point = bundle.ResolvedPlan.Points[index];
                var beforePointSnapshot = collector.Snapshot();
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "point_prepare_started", "point", point.PointId, null, new
                {
                    index,
                    point_kind = "acquisition_step",
                    magnetic_mode = point.EffectiveMagneticMode,
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
                    beforePointSnapshot.PacketCounter?.DeltaGt1Count,
                    null,
                    lockinModel: lockinModel,
                    collectorContract: collectorContract,
                    decodeFailures: beforePointSnapshot.DecodeFailures,
                    effectiveSampleHzPerParameter: beforePointSnapshot.EffectiveSampleHzPerParameter);

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
                    processStart,
                    options);
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
                    afterPointSnapshot.PacketCounter?.DeltaGt1Count,
                    quality.QualityStatus,
                    lockinModel: lockinModel,
                    collectorContract: collectorContract,
                    decodeFailures: afterPointSnapshot.DecodeFailures,
                    effectiveSampleHzPerParameter: afterPointSnapshot.EffectiveSampleHzPerParameter);
            }
        }
        catch (EmergencyStopException ex)
        {
            aborted = true;
            failure = ex.Message;
            emergencyEventRecorded = true;
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "emergency_stop_requested", "run", null, null, new
            {
                reason = ex.Message
            });
            ReportProgress(
                options,
                RuntimeState.Aborted,
                "emergency_stop_requested",
                ex.Message,
                null,
                null,
                bundle.ResolvedPlan.ResolvedPointCount,
                null,
                null,
                null,
                null,
                null,
                lockinModel: lockinModel,
                collectorContract: collectorContract);
        }
        catch (OperationCanceledException ex) when (options.EmergencyStopToken.IsCancellationRequested)
        {
            aborted = true;
            failure = "emergency stop requested";
            emergencyEventRecorded = true;
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "emergency_stop_requested", "run", null, null, new
            {
                reason = ex.Message
            });
            ReportProgress(
                options,
                RuntimeState.Aborted,
                "emergency_stop_requested",
                "emergency stop requested",
                null,
                null,
                bundle.ResolvedPlan.ResolvedPointCount,
                null,
                null,
                null,
                null,
                null,
                lockinModel: lockinModel,
                collectorContract: collectorContract);
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
                null,
                lockinModel: lockinModel,
                collectorContract: collectorContract);
        }
        finally
        {
            Exception? cleanupFailure = null;
            if (options.EmergencyStopToken.IsCancellationRequested && !emergencyEventRecorded)
            {
                aborted = true;
                emergencyEventRecorded = true;
                failure ??= "emergency stop requested";
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "emergency_stop_requested", "run", null, null, new
                {
                    reason = "emergency stop requested"
                });
            }

            try
            {
                smb.Cleanup();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

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
                collector.Stop();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

            var snapshot = collector.Snapshot();
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "collector_stopped", "collector", null, collector.DeviceId, new
            {
                frames_total = snapshot.Stats.FramesOk,
                timeout_count = snapshot.Stats.TimeoutCount,
                raw_len_bad_count = snapshot.Stats.RawLenBadCount,
                decode_failures = snapshot.DecodeFailures,
                effective_sample_hz_per_parameter = snapshot.EffectiveSampleHzPerParameter
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
                snapshot.PacketCounter?.DeltaGt1Count,
                null,
                lockinModel: lockinModel,
                collectorContract: collectorContract,
                decodeFailures: snapshot.DecodeFailures,
                effectiveSampleHzPerParameter: snapshot.EffectiveSampleHzPerParameter);

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
        var status = paused
            ? "paused"
            : aborted
            ? "aborted"
            : failure is not null
            ? "failed"
            : pointsFailed > 0 ? "completed_with_failed_points" : "completed";

        var terminalEvent = status switch
        {
            "paused" => "run_paused",
            "aborted" => "run_aborted",
            "failed" => "run_failed",
            _ => "run_completed"
        };
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, terminalEvent, "run", null, null, new
        {
            status,
            points_passed = pointsPassed,
            points_failed = pointsFailed,
            failure
        });
        ReportProgress(
            options,
            status == "paused"
                ? RuntimeState.Paused
                : status == "aborted"
                ? RuntimeState.Aborted
                : status == "failed"
                ? RuntimeState.Failed
                : RuntimeState.Completed,
            terminalEvent,
            $"Run {status}: {bundle.Plan.RunId}",
            null,
            null,
            bundle.ResolvedPlan.ResolvedPointCount,
            finalSnapshot.Stats.FramesOk,
            finalSnapshot.Stats.TimeoutCount,
            finalSnapshot.Stats.RawLenBadCount,
            finalSnapshot.PacketCounter?.DeltaGt1Count,
            null,
            lockinModel: lockinModel,
            collectorContract: collectorContract,
            decodeFailures: finalSnapshot.DecodeFailures,
            effectiveSampleHzPerParameter: finalSnapshot.EffectiveSampleHzPerParameter);

        RallArtifactWriter.WritePrettyJson(
            manifestPath,
            manifest with { Status = status });

        var summary = new RunSummaryRecord(
            bundle.Plan.RunId,
            status,
            lockinModel,
            collectorContract,
            bundle.ResolvedPlan.ResolvedPointCount,
            pointsPassed,
            pointsFailed,
            finalSnapshot.Stats.FramesOk,
            finalSnapshot.SamplesWritten,
            startedAt,
            endedAt,
            failure,
            finalSnapshot.Stats.ReadAttempts,
            finalSnapshot.Stats.TimeoutCount,
            finalSnapshot.Stats.RawLenBadCount,
            finalSnapshot.DecodeFailures,
            lockinModel == LockinModelNames.Oe1022d ? PathRelative(options.OutDir, collectorFramesPath) : null,
            lockinModel == LockinModelNames.Oe1300 ? PathRelative(options.OutDir, collectorFramesPath) : null,
            PathRelative(options.OutDir, parameterValuesPath),
            PathRelative(options.OutDir, sampleValuesPath),
            finalSnapshot.PacketCounter,
            finalSnapshot.QueryHz,
            finalSnapshot.UniqueBlockHz,
            finalSnapshot.EffectiveSampleHzPerParameter);
        RallArtifactWriter.WritePrettyJson(summaryPath, summary);

        if (status == "failed")
        {
            throw new InvalidOperationException($"run-execute failed: {failure}");
        }

        return summary;
    }

    private static void PrepareRunDirectory(string outDir)
    {
        Directory.CreateDirectory(outDir);
        foreach (var path in new[]
        {
            "segments.jsonl",
            "points.jsonl",
            "quality.jsonl",
            "device_state.jsonl",
            "events.jsonl",
            "collector_frames.jsonl",
            "collector_blocks.jsonl",
            "parameter_values.csv",
            "sample_values.csv",
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
        string? qualityStatus,
        long? estimatedRunDurationMs = null,
        long? estimatedPointDurationMs = null,
        long? estimatedSweepDurationMs = null,
        long? sweepPoints = null,
        long? startHz = null,
        long? stopHz = null,
        long? stepHz = null,
        int? dwellMs = null,
        string? lockinModel = null,
        string? collectorContract = null,
        long? decodeFailures = null,
        double? effectiveSampleHzPerParameter = null)
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
            qualityStatus,
            estimatedRunDurationMs,
            estimatedPointDurationMs,
            estimatedSweepDurationMs,
            sweepPoints,
            startHz,
            stopHz,
            stepHz,
            dwellMs,
            lockinModel,
            collectorContract,
            decodeFailures,
            effectiveSampleHzPerParameter));
    }

    private static string PathRelative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static void ThrowIfEmergencyRequested(ConfigDrivenRunOptions options)
    {
        if (options.EmergencyStopToken.IsCancellationRequested)
        {
            throw new EmergencyStopException();
        }
    }

    private static void SleepInterruptibly(int milliseconds, CancellationToken cancellationToken)
    {
        if (milliseconds <= 0)
        {
            return;
        }

        if (cancellationToken.WaitHandle.WaitOne(milliseconds))
        {
            throw new EmergencyStopException();
        }
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
