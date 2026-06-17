using System.Text.Json;
using Odmr.Artifacts;

namespace Odmr.Runtime;

public sealed record ResumePlan(
    string PreviousRunDir,
    string PreviousStatus,
    string ResumeOutDir,
    string ResumeFromPointId,
    int ResumeFromPointIndex,
    int PointsCompleted,
    int PointsTotal,
    long? EstimatedRemainingRunDurationMs);

public static class ResumeRun
{
    public static ResumePlan Prepare(string previousRunDir, string resumeOutDir)
    {
        var bundle = RunConfigLoader.LoadFromRunSnapshots(previousRunDir);
        var previousStatus = ReadPreviousStatus(previousRunDir);
        if (string.Equals(previousStatus, "completed", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"run is already terminal and not resumable: status={previousStatus}");
        }

        if (string.Equals(previousStatus, "aborted", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"run is already terminal and not resumable: status={previousStatus}");
        }

        if (!string.IsNullOrWhiteSpace(previousStatus) &&
            !string.Equals(previousStatus, "failed", StringComparison.Ordinal) &&
            !string.Equals(previousStatus, "paused", StringComparison.Ordinal) &&
            !string.Equals(previousStatus, "completed_with_failed_points", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"run status is not resumable: status={previousStatus}");
        }

        var completedPointIds = ReadCompletedPointIds(previousRunDir);
        var resumeFromIndex = FindResumePointIndex(bundle.ResolvedPlan.Points, completedPointIds);
        if (resumeFromIndex >= bundle.ResolvedPlan.Points.Count)
        {
            throw new InvalidOperationException("run has no remaining points to resume");
        }

        var resumePoint = bundle.ResolvedPlan.Points[resumeFromIndex];
        return new ResumePlan(
            Path.GetFullPath(previousRunDir),
            string.IsNullOrWhiteSpace(previousStatus) ? "process_exited" : previousStatus,
            Path.GetFullPath(resumeOutDir),
            resumePoint.PointId,
            resumeFromIndex,
            completedPointIds.Count,
            bundle.ResolvedPlan.Points.Count,
            EstimateRemainingRunDurationMs(bundle, resumeFromIndex));
    }

    public static RunSummaryRecord Execute(
        string previousRunDir,
        string resumeOutDir,
        IProgress<RunProgressEvent>? progress = null,
        CancellationToken cancellationToken = default,
        CancellationToken emergencyStopToken = default)
    {
        var bundle = RunConfigLoader.LoadFromRunSnapshots(previousRunDir);
        var plan = Prepare(previousRunDir, resumeOutDir);
        Directory.CreateDirectory(resumeOutDir);
        RallArtifactWriter.WritePrettyJson(
            Path.Combine(resumeOutDir, "resume_manifest.json"),
            new
            {
                schema_version = 1,
                previous_run_dir = plan.PreviousRunDir,
                previous_status = plan.PreviousStatus,
                resume_from_point_id = plan.ResumeFromPointId,
                resume_from_point_index = plan.ResumeFromPointIndex,
                points_completed = plan.PointsCompleted,
                points_total = plan.PointsTotal,
                estimated_remaining_run_duration_ms = plan.EstimatedRemainingRunDurationMs
            });

        return ConfigDrivenRun.Execute(
            bundle,
            new ConfigDrivenRunOptions(
                "",
                "",
                "",
                "",
                "",
                "",
                resumeOutDir,
                progress,
                cancellationToken,
                emergencyStopToken,
                plan.ResumeFromPointIndex,
                plan.PointsCompleted,
                plan.EstimatedRemainingRunDurationMs));
    }

    private static string ReadPreviousStatus(string previousRunDir)
    {
        var summaryPath = Path.Combine(previousRunDir, "summary.json");
        if (File.Exists(summaryPath))
        {
            using var summaryDoc = JsonDocument.Parse(File.ReadAllText(summaryPath));
            if (summaryDoc.RootElement.TryGetProperty("status", out var summaryStatus))
            {
                return summaryStatus.GetString() ?? string.Empty;
            }
        }

        var manifestPath = Path.Combine(previousRunDir, "run_manifest.json");
        if (File.Exists(manifestPath))
        {
            using var manifestDoc = JsonDocument.Parse(File.ReadAllText(manifestPath));
            if (manifestDoc.RootElement.TryGetProperty("status", out var manifestStatus))
            {
                return manifestStatus.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static HashSet<string> ReadCompletedPointIds(string previousRunDir)
    {
        var pointIds = ReadPointIds(Path.Combine(previousRunDir, "points.jsonl"));
        var passedQualityIds = ReadPassedQualityIds(Path.Combine(previousRunDir, "quality.jsonl"));
        var completedEventIds = ReadCompletedEventIds(Path.Combine(previousRunDir, "events.jsonl"));
        pointIds.IntersectWith(passedQualityIds);
        pointIds.IntersectWith(completedEventIds);
        return pointIds;
    }

    private static HashSet<string> ReadPointIds(string path)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (!File.Exists(path))
        {
            return result;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var point = JsonSerializer.Deserialize<PointRecord>(line);
            if (!string.IsNullOrWhiteSpace(point?.PointId))
            {
                result.Add(point.PointId);
            }
        }

        return result;
    }

    private static HashSet<string> ReadPassedQualityIds(string path)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (!File.Exists(path))
        {
            return result;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var quality = JsonSerializer.Deserialize<QualityRecord>(line);
            if (quality is not null &&
                string.Equals(quality.QualityStatus, "passed", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(quality.PointId))
            {
                result.Add(quality.PointId);
            }
        }

        return result;
    }

    private static HashSet<string> ReadCompletedEventIds(string path)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (!File.Exists(path))
        {
            return result;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<EventRecord>(line);
            if (record is not null &&
                string.Equals(record.Event, "point_completed", StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(record.PointId))
            {
                result.Add(record.PointId);
            }
        }

        return result;
    }

    private static int FindResumePointIndex(IReadOnlyList<RunPointPlan> points, HashSet<string> completedPointIds)
    {
        for (var index = 0; index < points.Count; index++)
        {
            if (!completedPointIds.Contains(points[index].PointId))
            {
                return index;
            }
        }

        return points.Count;
    }

    private static long? EstimateRemainingRunDurationMs(RunConfigBundle bundle, int resumeFromIndex)
    {
        if (resumeFromIndex >= bundle.ResolvedPlan.Points.Count)
        {
            return 0;
        }

        long total = 0;
        for (var index = resumeFromIndex; index < bundle.ResolvedPlan.Points.Count; index++)
        {
            var sweep = bundle.SmbProfile.DefaultSweep.ApplyOverride(bundle.ResolvedPlan.Points[index].SmbOverride);
            total += sweep.EstimatedSweepDurationMs + bundle.Plan.PointSettleMs + bundle.SmbProfile.EstimatedPointConfigurationMs;
        }

        return total;
    }
}
