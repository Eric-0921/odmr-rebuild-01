using System.Globalization;
using System.Text.Json;
using Odmr.Artifacts;
using Odmr.Devices;
using Odmr.Runtime;

namespace Odmr.WinProbe;

internal static class RunCliCommands
{
    internal static bool TryExecute(string command, IReadOnlyDictionary<string, string> options, out int exitCode)
    {
        switch (command)
        {
            case "run-resolve":
                exitCode = RunResolveCommand(options);
                return true;
            case "run-execute":
                exitCode = RunExecuteCommand(options);
                return true;
            case "resume-run":
                exitCode = ResumeRunCommand(options);
                return true;
            case "artifact-check":
                exitCode = ArtifactCheckCommand(options);
                return true;
            case "audit-continuity":
                exitCode = AuditContinuityCommand(options);
                return true;
            case "device-command-check":
                exitCode = DeviceCommandCheck();
                return true;
            case "live-replay":
                exitCode = LiveReplayCommand(options);
                return true;
            default:
                exitCode = 0;
                return false;
        }
    }

    private static int RunResolveCommand(IReadOnlyDictionary<string, string> options)
    {
        var bundle = RunConfigLoader.Load(
            CliSupport.GetRequiredOption(options, "station"),
            CliSupport.GetRequiredOption(options, "calibration"),
            CliSupport.GetRequiredOption(options, "plan"),
            CliSupport.GetRequiredOption(options, "smb-profile"),
            CliSupport.GetRequiredOption(options, "oe-profile"),
            CliSupport.GetRequiredOption(options, "laser-profile"));

        Console.WriteLine(JsonSerializer.Serialize(bundle.ToSummary(), JsonOptions.Pretty));
        return 0;
    }

    private static int RunExecuteCommand(IReadOnlyDictionary<string, string> options)
    {
        var planPath = CliSupport.GetRequiredOption(options, "plan");
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");
        var plan = RunConfigLoader.ReadJson<AcquisitionRunPlan>(planPath);
        var progressPath = CliSupport.GetOptionalOption(options, "progress-jsonl");
        using var progress = string.IsNullOrWhiteSpace(progressPath)
            ? null
            : new ProgressJsonlWriter(progressPath, plan.RunId);
        using var stopAfterPointCancellation = new CancellationTokenSource();
        using var emergencyCancellation = new CancellationTokenSource();
        using var watcherCancellation = new CancellationTokenSource();
        var stopWatcher = StartRequestFileWatcher(CliSupport.GetOptionalOption(options, "stop-request-file"), stopAfterPointCancellation, watcherCancellation.Token);
        var emergencyWatcher = StartRequestFileWatcher(CliSupport.GetOptionalOption(options, "emergency-stop-file"), emergencyCancellation, watcherCancellation.Token);

        RunSummaryRecord summary;
        try
        {
            summary = ConfigDrivenRun.Execute(new ConfigDrivenRunOptions(
                CliSupport.GetRequiredOption(options, "station"),
                CliSupport.GetRequiredOption(options, "calibration"),
                planPath,
                CliSupport.GetRequiredOption(options, "smb-profile"),
                CliSupport.GetRequiredOption(options, "oe-profile"),
                CliSupport.GetRequiredOption(options, "laser-profile"),
                outDir,
                progress,
                stopAfterPointCancellation.Token,
                emergencyCancellation.Token));
        }
        finally
        {
            watcherCancellation.Cancel();
            WaitForWatcher(stopWatcher);
            WaitForWatcher(emergencyWatcher);
        }

        Console.WriteLine($"run-execute done: run_id={summary.RunId}, status={summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames_ok={summary.FramesTotal}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, {ContinuityMetric(summary)}, out_dir={outDir}");
        return RunSummaryAccepted(summary) ? 0 : 2;
    }

    private static int ResumeRunCommand(IReadOnlyDictionary<string, string> options)
    {
        var previousRunDir = CliSupport.GetRequiredOption(options, "previous-run");
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");
        var progressPath = CliSupport.GetOptionalOption(options, "progress-jsonl");
        using var progress = string.IsNullOrWhiteSpace(progressPath)
            ? null
            : new ProgressJsonlWriter(progressPath, Path.GetFileName(Path.GetFullPath(outDir)));
        using var stopAfterPointCancellation = new CancellationTokenSource();
        using var emergencyCancellation = new CancellationTokenSource();
        using var watcherCancellation = new CancellationTokenSource();
        var stopWatcher = StartRequestFileWatcher(CliSupport.GetOptionalOption(options, "stop-request-file"), stopAfterPointCancellation, watcherCancellation.Token);
        var emergencyWatcher = StartRequestFileWatcher(CliSupport.GetOptionalOption(options, "emergency-stop-file"), emergencyCancellation, watcherCancellation.Token);

        RunSummaryRecord summary;
        try
        {
            summary = ResumeRun.Execute(
                previousRunDir,
                outDir,
                progress,
                stopAfterPointCancellation.Token,
                emergencyCancellation.Token);
        }
        finally
        {
            watcherCancellation.Cancel();
            WaitForWatcher(stopWatcher);
            WaitForWatcher(emergencyWatcher);
        }

        Console.WriteLine($"resume-run done: previous_run={previousRunDir}, status={summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames_ok={summary.FramesTotal}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, {ContinuityMetric(summary)}, out_dir={outDir}");
        return RunSummaryAccepted(summary) ? 0 : 2;
    }

    private static int ArtifactCheckCommand(IReadOnlyDictionary<string, string> options)
    {
        var report = ArtifactCheck.Check(CliSupport.GetRequiredOption(options, "run"));
        Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions.Pretty));
        return report.Passed ? 0 : 2;
    }

    private static int AuditContinuityCommand(IReadOnlyDictionary<string, string> options)
    {
        var runDir = CliSupport.GetRequiredOption(options, "run");
        var outPath = CliSupport.GetRequiredOption(options, "out");
        var report = ContinuityAudit.Audit(runDir);
        ContinuityAudit.WriteReport(outPath, report);
        var continuityMetric = report.LockinModel == "oe1300"
            ? $"effective_sample_hz_per_parameter={report.EffectiveSampleHzPerParameter:0.###}"
            : $"delta_gt1={report.DevicePacketCounter?.DeltaGt1Count ?? 0}";
        Console.WriteLine($"continuity audit done: verdict={report.Verdict}, frames={report.FramesTotal}, {continuityMetric}, out={outPath}");
        return report.Verdict == "continuous" ? 0 : 2;
    }

    private static int DeviceCommandCheck()
    {
        var report = DeviceCommandCatalog.Check();
        Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions.Pretty));
        return report.Passed ? 0 : 2;
    }

    private static int LiveReplayCommand(IReadOnlyDictionary<string, string> options)
    {
        var snapshot = LiveReplay.Replay(
            CliSupport.GetRequiredOption(options, "run"),
            CliSupport.GetIntOption(options, "tail-events", 20));
        Console.WriteLine(JsonSerializer.Serialize(snapshot, JsonOptions.Pretty));
        return snapshot.CollectorHealth == "clean" ? 0 : 2;
    }

    private static string ContinuityMetric(RunSummaryRecord summary) =>
        summary.LockinModel == "oe1300"
            ? $"effective_sample_hz_per_parameter={summary.EffectiveSampleHzPerParameter:0.###}"
            : $"delta_gt1={summary.PacketCounter?.DeltaGt1Count ?? 0}";

    private static bool RunSummaryAccepted(RunSummaryRecord summary)
    {
        if (summary.Status is not ("completed" or "completed_with_failed_points" or "paused"))
        {
            return false;
        }

        if (summary.LockinModel == "oe1300")
        {
            return ContinuityAudit.Oe1300CollectorAccepted(
                summary.TimeoutCount,
                summary.RawLenBadCount,
                summary.DecodeFailures ?? 0,
                summary.EffectiveSampleHzPerParameter);
        }

        return summary.TimeoutCount == 0 &&
               summary.RawLenBadCount == 0 &&
               (summary.PacketCounter?.DeltaGt1Count ?? 0) == 0;
    }

    private static Task? StartRequestFileWatcher(string? requestPath, CancellationTokenSource requestCancellation, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return null;
        }

        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested && !requestCancellation.IsCancellationRequested)
            {
                if (File.Exists(requestPath))
                {
                    requestCancellation.Cancel();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }

    private static void WaitForWatcher(Task? watcher)
    {
        if (watcher is null)
        {
            return;
        }

        try
        {
            watcher.Wait(TimeSpan.FromSeconds(1));
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(inner => inner is TaskCanceledException or OperationCanceledException))
        {
        }
    }
}
