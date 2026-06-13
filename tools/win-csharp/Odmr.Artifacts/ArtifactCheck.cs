using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed record ArtifactCheckReport(
    [property: JsonPropertyName("run_dir")] string RunDir,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("run_id")] string? RunId,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("raw_bytes")] long RawBytes,
    [property: JsonPropertyName("raw_size_matches_frames")] bool RawSizeMatchesFrames,
    [property: JsonPropertyName("idx_lines")] long IndexLines,
    [property: JsonPropertyName("idx_lines_match_frames")] bool IndexLinesMatchFrames,
    [property: JsonPropertyName("segments_count")] long SegmentsCount,
    [property: JsonPropertyName("points_count")] long PointsCount,
    [property: JsonPropertyName("quality_count")] long QualityCount,
    [property: JsonPropertyName("record_counts_consistent")] bool RecordCountsConsistent,
    [property: JsonPropertyName("quality_status_counts")] IReadOnlyDictionary<string, long> QualityStatusCounts,
    [property: JsonPropertyName("events_present")] IReadOnlyList<string> EventsPresent,
    [property: JsonPropertyName("missing_events")] IReadOnlyList<string> MissingEvents,
    [property: JsonPropertyName("manifest_status")] string? ManifestStatus,
    [property: JsonPropertyName("summary_status")] string? SummaryStatus,
    [property: JsonPropertyName("manifest_status_matches_summary")] bool ManifestStatusMatchesSummary,
    [property: JsonPropertyName("required_files_missing")] IReadOnlyList<string> RequiredFilesMissing,
    [property: JsonPropertyName("snapshots_missing")] IReadOnlyList<string> SnapshotsMissing,
    [property: JsonPropertyName("passed")] bool Passed);

public static class ArtifactCheck
{
    private static readonly string[] RequiredRuntimeFiles =
    [
        "summary.json",
        "run_manifest.json",
        "events.jsonl",
        "raw/oe1022d.rall",
        "raw/oe1022d.frames.idx.jsonl",
        "segments.jsonl",
        "points.jsonl",
        "quality.jsonl"
    ];

    private static readonly string[] SnapshotFiles =
    [
        "station_snapshot.json",
        "plan_snapshot.json",
        "calibration_snapshot.json",
        "smb_profile_snapshot.json",
        "oe_profile_snapshot.json",
        "laser_profile_snapshot.json"
    ];

    private static readonly string[] RequiredEventNames =
    [
        "run_opened",
        "collector_started",
        "oe_profile_applied",
        "laser_profile_applied",
        "point_prepare_started",
        "point_stable",
        "sweep_started",
        "sweep_completed",
        "point_completed",
        "collector_stopped",
        "cleanup_completed"
    ];

    public static ArtifactCheckReport Check(string runDir)
    {
        var missingFiles = RequiredRuntimeFiles
            .Where(path => !File.Exists(Path.Combine(runDir, path)))
            .ToArray();
        var missingSnapshots = SnapshotFiles
            .Where(path => !File.Exists(Path.Combine(runDir, path)))
            .ToArray();

        var summaryPath = Path.Combine(runDir, "summary.json");
        var manifestPath = Path.Combine(runDir, "run_manifest.json");
        var rawPath = Path.Combine(runDir, "raw", "oe1022d.rall");
        var indexPath = Path.Combine(runDir, "raw", "oe1022d.frames.idx.jsonl");
        var segmentsPath = Path.Combine(runDir, "segments.jsonl");
        var pointsPath = Path.Combine(runDir, "points.jsonl");
        var qualityPath = Path.Combine(runDir, "quality.jsonl");
        var eventsPath = Path.Combine(runDir, "events.jsonl");

        var summary = File.Exists(summaryPath) ? ReadObject(summaryPath) : null;
        var manifest = File.Exists(manifestPath) ? ReadObject(manifestPath) : null;
        var framesTotal = GetInt64(summary, "frames_total") ?? GetInt64(summary, "frames_ok") ?? 0;
        var runId = GetString(summary, "run_id");
        var summaryStatus = GetString(summary, "status");
        var manifestStatus = GetString(manifest, "status");
        var rawBytes = File.Exists(rawPath) ? new FileInfo(rawPath).Length : 0;
        var idxLines = File.Exists(indexPath) ? CountLines(indexPath) : 0;
        var segmentsCount = File.Exists(segmentsPath) ? CountLines(segmentsPath) : 0;
        var pointsCount = File.Exists(pointsPath) ? CountLines(pointsPath) : 0;
        var qualityCount = File.Exists(qualityPath) ? CountLines(qualityPath) : 0;
        var qualityStatusCounts = File.Exists(qualityPath)
            ? CountJsonlStringProperty(qualityPath, "quality_status")
            : new Dictionary<string, long>(StringComparer.Ordinal);
        var eventsPresent = File.Exists(eventsPath)
            ? DistinctJsonlStringProperty(eventsPath, "event")
            : [];
        var eventSet = eventsPresent.ToHashSet(StringComparer.Ordinal);
        var missingEvents = RequiredEventNames
            .Where(eventName => !eventSet.Contains(eventName))
            .ToList();
        if (!eventSet.Contains("run_completed") && !eventSet.Contains("run_failed"))
        {
            missingEvents.Add("run_completed|run_failed");
        }

        var rawSizeMatchesFrames = rawBytes == framesTotal * 12288;
        var idxLinesMatchFrames = idxLines == framesTotal;
        var recordCountsConsistent = segmentsCount == pointsCount && pointsCount == qualityCount;
        var manifestStatusMatchesSummary = summaryStatus is not null &&
            manifestStatus is not null &&
            string.Equals(summaryStatus, manifestStatus, StringComparison.Ordinal);
        var passed = missingFiles.Length == 0 &&
            missingSnapshots.Length == 0 &&
            rawSizeMatchesFrames &&
            idxLinesMatchFrames &&
            recordCountsConsistent &&
            missingEvents.Count == 0 &&
            manifestStatusMatchesSummary;

        return new ArtifactCheckReport(
            runDir,
            passed ? "passed" : "failed",
            runId,
            framesTotal,
            rawBytes,
            rawSizeMatchesFrames,
            idxLines,
            idxLinesMatchFrames,
            segmentsCount,
            pointsCount,
            qualityCount,
            recordCountsConsistent,
            qualityStatusCounts,
            eventsPresent,
            missingEvents,
            manifestStatus,
            summaryStatus,
            manifestStatusMatchesSummary,
            missingFiles,
            missingSnapshots,
            passed);
    }

    private static JsonElement? ReadObject(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    private static long CountLines(string path)
    {
        var count = 0L;
        using var reader = File.OpenText(path);
        while (reader.ReadLine() is not null)
        {
            count++;
        }

        return count;
    }

    private static Dictionary<string, long> CountJsonlStringProperty(string path, string propertyName)
    {
        var counts = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var value in ReadJsonlStringProperty(path, propertyName))
        {
            counts[value] = counts.TryGetValue(value, out var current) ? current + 1 : 1;
        }

        return counts;
    }

    private static IReadOnlyList<string> DistinctJsonlStringProperty(string path, string propertyName) =>
        ReadJsonlStringProperty(path, propertyName)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IEnumerable<string> ReadJsonlStringProperty(string path, string propertyName)
    {
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            if (document.RootElement.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String)
            {
                yield return property.GetString() ?? "";
            }
        }
    }

    private static string? GetString(JsonElement? element, string propertyName)
    {
        if (element is not { } value ||
            !value.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return property.GetString();
    }

    private static long? GetInt64(JsonElement? element, string propertyName)
    {
        if (element is not { } value ||
            !value.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var parsed)
            ? parsed
            : null;
    }
}
