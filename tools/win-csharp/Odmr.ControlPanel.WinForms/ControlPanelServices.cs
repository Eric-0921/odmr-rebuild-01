using Odmr.Artifacts;
using Odmr.Runtime;

namespace Odmr.ControlPanel.WinForms;

internal sealed record ConfigSelection(
    string StationPath,
    string CalibrationPath,
    string PlanPath,
    string SmbProfilePath,
    string OeProfilePath,
    string LaserProfilePath);

internal sealed record RunLaunchSnapshot(
    RuntimeState State,
    string EventName,
    string Message,
    string? PointId,
    int? PointIndex,
    int? PointsTotal,
    long? FramesTotal,
    long? TimeoutCount,
    long? RawLenBadCount,
    long? DeltaGt1Count,
    string? QualityStatus);

internal sealed class ConfigCatalogService
{
    public string RepoRoot { get; }

    public ConfigCatalogService(string? startDirectory = null)
    {
        RepoRoot = FindRepoRoot(startDirectory ?? Environment.CurrentDirectory);
    }

    public IReadOnlyList<string> Stations() => Files("configs", "stations", "*.json");

    public IReadOnlyList<string> Calibrations() => Files("configs", "calibrations", "*.json");

    public IReadOnlyList<string> Plans() => Files("configs", "plans", "*.json");

    public IReadOnlyList<string> Profiles(string contains) => Files("configs", "profiles", $"*{contains}*.json");

    public string DefaultOutputRoot()
    {
        var path = Path.Combine(RepoRoot, "runs");
        Directory.CreateDirectory(path);
        return path;
    }

    private IReadOnlyList<string> Files(params string[] parts)
    {
        var pattern = parts[^1];
        var dirParts = new string[parts.Length];
        dirParts[0] = RepoRoot;
        Array.Copy(parts, 0, dirParts, 1, parts.Length - 1);
        var dir = Path.Combine(dirParts);
        return Directory.Exists(dir)
            ? Directory.GetFiles(dir, pattern).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()
            : Array.Empty<string>();
    }

    private static string FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "configs")) &&
                Directory.Exists(Path.Combine(dir.FullName, "tools", "win-csharp")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("could not find repository root containing configs and tools/win-csharp");
    }
}

internal sealed class RunLaunchService
{
    private CancellationTokenSource? cancellation;

    public bool IsRunning { get; private set; }

    public void RequestStopAfterCurrentPoint() => cancellation?.Cancel();

    public async Task<RunSummaryRecord> RunAsync(
        ConfigSelection selection,
        string outDir,
        IProgress<RunLaunchSnapshot> progress)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("a run is already active");
        }

        if (Directory.Exists(outDir))
        {
            throw new InvalidOperationException($"out-dir already exists: {outDir}");
        }

        using var runCancellation = new CancellationTokenSource();
        cancellation = runCancellation;
        IsRunning = true;
        try
        {
            var runtimeProgress = new Progress<RunProgressEvent>(evt =>
            {
                progress.Report(new RunLaunchSnapshot(
                    evt.State,
                    evt.EventName,
                    evt.Message,
                    evt.PointId,
                    evt.PointIndex,
                    evt.PointsTotal,
                    evt.FramesTotal,
                    evt.TimeoutCount,
                    evt.RawLenBadCount,
                    evt.DeltaGt1Count,
                    evt.QualityStatus));
            });

            return await Task.Run(() => ConfigDrivenRun.Execute(new ConfigDrivenRunOptions(
                selection.StationPath,
                selection.CalibrationPath,
                selection.PlanPath,
                selection.SmbProfilePath,
                selection.OeProfilePath,
                selection.LaserProfilePath,
                outDir,
                runtimeProgress,
                runCancellation.Token)));
        }
        finally
        {
            IsRunning = false;
            cancellation = null;
        }
    }
}
