using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
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

internal sealed record ConfigMagAxis(
    string AxisId,
    StationDeviceSpec Device,
    M8812AxisSession Session);

internal sealed record RuntimeResolvedConnections(
    LockinConnectionFacts Lockin,
    string SmbTransport,
    string? SmbHost,
    int? SmbPort,
    string? SmbResource,
    IReadOnlyDictionary<string, string> MagPorts,
    string? LaserPort);

internal sealed class EmergencyStopException : OperationCanceledException
{
    public EmergencyStopException()
        : base("emergency stop requested")
    {
    }
}

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

    private static RuntimeResolvedConnections ResolveRuntimeConnections(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var smbDevice = bundle.Station.Devices.First(device => device.DeviceId == "smb100a_main");
        var lockinConnection = ResolveLockinConnection(bundle, eventsPath, processStart);
        var resolvedSmbConnection = ResolveSmbConnection(smbDevice, bundle.Connections, eventsPath, processStart, bundle.Plan.RunId);
        var magPorts = bundle.ResolvedPlan.RequiresMagneticControl
            ? ResolveMagPorts(bundle, eventsPath, processStart)
            : new Dictionary<string, string>();

        return new RuntimeResolvedConnections(
            lockinConnection,
            resolvedSmbConnection.Transport,
            resolvedSmbConnection.Host,
            resolvedSmbConnection.Port,
            resolvedSmbConnection.Resource,
            magPorts,
            bundle.Connections.LaserPort);
    }

    private static LockinConnectionFacts ResolveLockinConnection(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var lockinDevice = bundle.Station.Devices.First(device => device.DeviceId == bundle.Connections.Lockin.DeviceId);
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => bundle.Connections.Lockin with
            {
                Resource = ResolveOe1022dResource(lockinDevice, bundle.Connections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate, eventsPath, processStart, bundle.Plan.RunId)
            },
            LockinModelNames.Oe1300 => bundle.Connections.Lockin with
            {
                Host = ResolveOe1300Host(lockinDevice, bundle.Connections.Lockin.Port ?? Oe1300Defaults.TcpPort, eventsPath, processStart, bundle.Plan.RunId),
                Port = bundle.Connections.Lockin.Port ?? Oe1300Defaults.TcpPort
            },
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static List<ConfigMagAxis> OpenMagAxes(RunConfigBundle bundle, RuntimeResolvedConnections resolvedConnections)
    {
        var axes = new List<ConfigMagAxis>
        {
            OpenMagAxis(bundle, "mag_x", resolvedConnections.MagPorts["mag_x"]),
            OpenMagAxis(bundle, "mag_y", resolvedConnections.MagPorts["mag_y"]),
            OpenMagAxis(bundle, "mag_z", resolvedConnections.MagPorts["mag_z"])
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

    private static void ApplyOeFixedProfile(RunConfigBundle bundle, RuntimeResolvedConnections resolvedConnections, string eventsPath, long processStart)
    {
        switch (bundle.OeProfile.NormalizedModel)
        {
            case LockinModelNames.Oe1022d:
            {
                using var oe = Oe1022dVisa.Open(
                    resolvedConnections.Lockin.Resource ?? throw new InvalidOperationException("OE1022D resource missing"),
                    resolvedConnections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate);
                var fixedProfile = bundle.OeProfile.GetOe1022dFixed();
                var commands = BuildOeFixedCommands(fixedProfile);
                foreach (var command in commands)
                {
                    oe.SendAsciiCommand(command);
                    Thread.Sleep(bundle.OeProfile.CommandSettleMs);
                }

                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "oe_profile_applied", "profile", null, resolvedConnections.Lockin.DeviceId, new
                {
                    profile_id = bundle.OeProfile.ProfileId,
                    lockin_model = bundle.OeProfile.NormalizedModel,
                    fixed_commands_sent = true,
                    command_count = commands.Length,
                    channel = fixedProfile.Channel,
                    resource = resolvedConnections.Lockin.Resource
                });
                return;
            }
            case LockinModelNames.Oe1300:
            {
                using var oe = Oe1300Tcp.Open(
                    resolvedConnections.Lockin.Host ?? throw new InvalidOperationException("OE1300 host missing"),
                    resolvedConnections.Lockin.Port ?? Oe1300Defaults.TcpPort);
                var fixedProfile = bundle.OeProfile.GetOe1300Fixed();
                var commands = BuildOe1300FixedCommands(fixedProfile);
                foreach (var command in commands)
                {
                    oe.SendAsciiCommand(command);
                    Thread.Sleep(bundle.OeProfile.CommandSettleMs);
                }

                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "oe_profile_applied", "profile", null, resolvedConnections.Lockin.DeviceId, new
                {
                    profile_id = bundle.OeProfile.ProfileId,
                    lockin_model = bundle.OeProfile.NormalizedModel,
                    fixed_commands_sent = true,
                    command_count = commands.Length,
                    host = resolvedConnections.Lockin.Host,
                    port = resolvedConnections.Lockin.Port
                });
                return;
            }
            default:
                throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}");
        }
    }

    private static string ResolveOe1022dResource(StationDeviceSpec device, int baudRate, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Resource);
        foreach (var candidate in device.TransportHint.ResourceCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in Oe1022dVisa.ListResources())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var resource in candidates)
        {
            try
            {
                using var oe = Oe1022dVisa.Open(resource, baudRate);
                var idn = oe.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "visa_resource",
                        resource,
                        idn
                    });
                    return resource;
                }

                failures.Add($"{resource}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{resource}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve OE1022D resource for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static string ResolveOe1300Host(StationDeviceSpec device, int port, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Host);
        foreach (var candidate in device.TransportHint.HostCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var host in candidates)
        {
            try
            {
                using var oe = Oe1300Tcp.Open(host, port);
                var idn = oe.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "tcp_socket",
                        host,
                        port,
                        idn
                    });
                    return host;
                }

                failures.Add($"{host}:{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{host}:{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve OE1300 host for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbConnection(
        StationDeviceSpec device,
        StationConnectionFacts connections,
        string eventsPath,
        long processStart,
        string runId)
    {
        return connections.SmbTransport switch
        {
            "visa_resource" => ResolveSmbResource(device, eventsPath, processStart, runId),
            "tcp_socket" => ResolveSmbHost(device, connections.SmbPort ?? Smb100aDefaults.Port, eventsPath, processStart, runId),
            _ => throw new InvalidOperationException($"unsupported SMB transport: {connections.SmbTransport}")
        };
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbHost(StationDeviceSpec device, int port, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Host);
        foreach (var candidate in device.TransportHint.HostCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var host in candidates)
        {
            try
            {
                using var smb = Smb100aTcp.Open(host, port, Smb100aDefaults.TimeoutMs);
                var idn = smb.Query(Smb100aCommands.QueryIdn);
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "tcp_socket",
                        host,
                        port,
                        idn
                    });
                    return ("tcp_socket", host, port, null);
                }

                failures.Add($"{host}:{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{host}:{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve SMB100A host for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbResource(StationDeviceSpec device, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Resource);
        foreach (var candidate in device.TransportHint.ResourceCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in Smb100aVisa.ListResources())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var resource in candidates)
        {
            try
            {
                using var smb = Smb100aVisa.Open(resource);
                var idn = smb.Query(Smb100aCommands.QueryIdn);
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "visa_resource",
                        resource,
                        idn
                    });
                    return ("visa_resource", null, null, resource);
                }

                failures.Add($"{resource}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{resource}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve SMB100A VISA resource for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static ILockinCollector CreateLockinCollector(
        RunConfigBundle bundle,
        RuntimeResolvedConnections resolvedConnections,
        string collectorPath,
        string parameterValuesPath,
        string sampleValuesPath,
        long processStart)
    {
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => new OeRallCollector(
                resolvedConnections.Lockin.Resource ?? throw new InvalidOperationException("OE1022D resource missing"),
                resolvedConnections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate,
                collectorPath,
                parameterValuesPath,
                sampleValuesPath,
                processStart),
            LockinModelNames.Oe1300 => new Oe1300TcpCollector(
                resolvedConnections.Lockin.Host ?? throw new InvalidOperationException("OE1300 host missing"),
                resolvedConnections.Lockin.Port ?? Oe1300Defaults.TcpPort,
                collectorPath,
                parameterValuesPath,
                sampleValuesPath,
                processStart,
                bundle.OeProfile.GetOe1300Collector()),
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static object BuildCollectorStartedData(RunConfigBundle bundle)
    {
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => new
            {
                frame_exact_bytes = bundle.OeProfile.GetOe1022dCollector().FrameExactBytes,
                rall_post_write_delay_ms = bundle.OeProfile.GetOe1022dCollector().RallPostWriteDelayMs,
                ring_capacity_frames = bundle.OeProfile.GetOe1022dCollector().RingCapacityFrames
            },
            LockinModelNames.Oe1300 => new
            {
                tcp_expected_bytes = bundle.OeProfile.GetOe1300Collector().TcpExpectedBytes,
                tcp_payload_bytes = bundle.OeProfile.GetOe1300Collector().TcpPayloadBytes,
                parameter_count = bundle.OeProfile.GetOe1300Collector().ParameterCount,
                samples_per_parameter = bundle.OeProfile.GetOe1300Collector().SamplesPerParameter,
                rall_post_write_delay_ms = bundle.OeProfile.GetOe1300Collector().RallPostWriteDelayMs,
                drain_before_write = bundle.OeProfile.GetOe1300Collector().DrainBeforeWrite
            },
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static ISmb100aSession OpenSmbSession(RuntimeResolvedConnections resolvedConnections)
    {
        return resolvedConnections.SmbTransport switch
        {
            "visa_resource" => Smb100aVisa.Open(resolvedConnections.SmbResource ?? throw new InvalidOperationException("SMB VISA resource missing")),
            "tcp_socket" => Smb100aTcp.Open(
                resolvedConnections.SmbHost ?? throw new InvalidOperationException("SMB TCP host missing"),
                resolvedConnections.SmbPort ?? throw new InvalidOperationException("SMB TCP port missing")),
            _ => throw new InvalidOperationException($"unsupported SMB transport: {resolvedConnections.SmbTransport}")
        };
    }

    private static IReadOnlyDictionary<string, string> ResolveMagPorts(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var axisId in new[] { "mag_x", "mag_y", "mag_z" })
        {
            var device = bundle.Station.Devices.First(device => device.DeviceId == axisId);
            result[axisId] = ResolveMagPort(device, eventsPath, processStart, bundle.Plan.RunId);
        }

        return result;
    }

    private static string ResolveMagPort(StationDeviceSpec device, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.PortPath);
        foreach (var candidate in device.TransportHint.PortCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var port in candidates)
        {
            try
            {
                using var session = M8812Serial.Open(port);
                session.Clear();
                var idn = session.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "serial_port",
                        port,
                        idn
                    });
                    return port;
                }

                failures.Add($"{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve M8812 port for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static bool IdentityMatches(StationIdentity? identity, string idn)
    {
        if (identity is null)
        {
            return true;
        }

        var containsAll = identity.ContainsAll ?? Array.Empty<string>();
        var containsAny = identity.ContainsAny ?? Array.Empty<string>();
        if (containsAll.Any(token => !idn.Contains(token, StringComparison.Ordinal)))
        {
            return false;
        }

        return containsAny.Count == 0 || containsAny.Any(token => idn.Contains(token, StringComparison.Ordinal));
    }

    private static void AppendUnique(List<string> values, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (!values.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            values.Add(candidate);
        }
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
        ReportProgress(
            options,
            RuntimeState.PointRunning,
            "sweep_started",
            $"Sweep started: {point.PointId}",
            point.PointId,
            index,
            bundle.ResolvedPlan.ResolvedPointCount,
            collector.Snapshot().Stats.FramesOk,
            null,
            null,
            null,
            null,
            bundle.ResolvedPlan.EstimatedRunDurationMs,
            sweep.EstimatedSweepDurationMs + bundle.Plan.PointSettleMs + bundle.SmbProfile.EstimatedPointConfigurationMs,
            sweep.EstimatedSweepDurationMs,
            sweep.SweepPoints,
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            lockinModel: bundle.OeProfile.NormalizedModel,
            collectorContract: RunConfigLoader.CollectorContractFor(bundle.OeProfile.NormalizedModel));
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
        ReportProgress(
            options,
            RuntimeState.PointRunning,
            "sweep_completed",
            $"Sweep completed: {point.PointId}",
            point.PointId,
            index,
            bundle.ResolvedPlan.ResolvedPointCount,
            collector.Snapshot().Stats.FramesOk,
            null,
            null,
            null,
            null,
            bundle.ResolvedPlan.EstimatedRunDurationMs,
            sweep.EstimatedSweepDurationMs + bundle.Plan.PointSettleMs + bundle.SmbProfile.EstimatedPointConfigurationMs,
            sweepObservation.EstimatedSweepDurationMs,
            sweep.SweepPoints,
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            lockinModel: bundle.OeProfile.NormalizedModel,
            collectorContract: RunConfigLoader.CollectorContractFor(bundle.OeProfile.NormalizedModel));

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
            "sample_values.csv",
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

    private static string[] BuildOe1300FixedCommands(Oe1300FixedProfile fixedProfile) =>
    [
        Oe1300Commands.SetInputSource(fixedProfile.InputSource),
        Oe1300Commands.SetInputCoupling(fixedProfile.InputCoupling),
        Oe1300Commands.SetInputRange(fixedProfile.InputRange),
        Oe1300Commands.SetReferenceSource(fixedProfile.ReferenceSource),
        Oe1300Commands.SetReferenceFrequency(fixedProfile.ReferenceFrequencyHz),
        Oe1300Commands.SetReferenceSlope(fixedProfile.ReferenceSlope),
        Oe1300Commands.SetSensitivity(fixedProfile.SensitivityIndex),
        Oe1300Commands.SetTimeConstant(fixedProfile.TimeConstantSeconds),
        Oe1300Commands.SetFilterSlope(fixedProfile.FilterSlope),
        Oe1300Commands.SetSync(fixedProfile.SyncEnabled),
        Oe1300Commands.SetSineOutputEnabled(fixedProfile.SineOutputEnabled),
        Oe1300Commands.SetSineOutputVoltageVrms(fixedProfile.SineOutputVoltageVrms)
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
            framesUnique = framesInSegment;
            duplicateCount = 0;
            duplicateRatio = 0.0;
            collectorHealth = pointTimeouts == 0
                ? "clean"
                : pointTimeouts <= thresholds.MaxTimeoutCount ? "recovered_timeout" : "degraded_timeout";
            qualityStatus = framesInSegment == 0
                ? "failed_no_frames"
                : framesInSegment < thresholds.MinFrames ? "failed_min_frames"
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
                ? "failed_no_blocks"
                : framesUnique == 0
                ? "failed_duplicate_only"
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
