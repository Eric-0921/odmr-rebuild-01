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
    [property: JsonPropertyName("device_state_count")] long DeviceStateCount,
    [property: JsonPropertyName("record_counts_consistent")] bool RecordCountsConsistent,
    [property: JsonPropertyName("device_state_consistent")] bool DeviceStateConsistent,
    [property: JsonPropertyName("rf_exposure_windows_covered")] bool RfExposureWindowsCovered,
    [property: JsonPropertyName("device_state_issues")] IReadOnlyList<string> DeviceStateIssues,
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
        "quality.jsonl",
        "device_state.jsonl"
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
        var deviceStatePath = Path.Combine(runDir, "device_state.jsonl");
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
        var deviceStateCount = File.Exists(deviceStatePath) ? CountLines(deviceStatePath) : 0;
        var qualityStatusCounts = File.Exists(qualityPath)
            ? CountJsonlStringProperty(qualityPath, "quality_status")
            : new Dictionary<string, long>(StringComparer.Ordinal);
        var deviceStateIssues = ValidateDeviceState(pointsPath, segmentsPath, deviceStatePath);
        var deviceStateConsistent = deviceStateIssues.Count == 0;
        var rfExposureWindowsCovered = !deviceStateIssues.Any(issue =>
            issue.Contains("rf_exposure", StringComparison.Ordinal) ||
            issue.Contains("segment window", StringComparison.Ordinal));
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
        var recordCountsConsistent = segmentsCount == pointsCount &&
            pointsCount == qualityCount &&
            qualityCount == deviceStateCount;
        var manifestStatusMatchesSummary = summaryStatus is not null &&
            manifestStatus is not null &&
            string.Equals(summaryStatus, manifestStatus, StringComparison.Ordinal);
        var passed = missingFiles.Length == 0 &&
            missingSnapshots.Length == 0 &&
            rawSizeMatchesFrames &&
            idxLinesMatchFrames &&
            recordCountsConsistent &&
            deviceStateConsistent &&
            rfExposureWindowsCovered &&
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
            deviceStateCount,
            recordCountsConsistent,
            deviceStateConsistent,
            rfExposureWindowsCovered,
            deviceStateIssues,
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

    private sealed record PointIndex(string PointId, int? Index);

    private sealed record SegmentIndex(
        string PointId,
        string SegmentId,
        ulong StartMonotonicNs,
        ulong EndMonotonicNs,
        long RawOffsetStart,
        long RawOffsetEnd,
        long? FrameSeqStart,
        long? FrameSeqEnd);

    private static IReadOnlyList<string> ValidateDeviceState(
        string pointsPath,
        string segmentsPath,
        string deviceStatePath)
    {
        var issues = new List<string>();
        if (!File.Exists(pointsPath) || !File.Exists(segmentsPath) || !File.Exists(deviceStatePath))
        {
            return issues;
        }

        var points = ReadPointIndexes(pointsPath, issues);
        var segments = ReadSegmentIndexes(segmentsPath, issues);
        var seenDevicePoints = new HashSet<string>(StringComparer.Ordinal);
        var lineNumber = 0;

        foreach (var line in File.ReadLines(deviceStatePath))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var pointId = GetRequiredString(root, "point_id", issues, $"device_state line {lineNumber}");
            if (string.IsNullOrEmpty(pointId))
            {
                continue;
            }

            if (!seenDevicePoints.Add(pointId))
            {
                issues.Add($"device_state duplicate point_id {pointId}");
            }

            if (!points.TryGetValue(pointId, out var point))
            {
                issues.Add($"device_state point_id {pointId} missing from points.jsonl");
            }
            else if (GetNullableInt32(root, "point_index") is { } pointIndex && point.Index is { } expectedIndex && pointIndex != expectedIndex)
            {
                issues.Add($"device_state point_id {pointId} point_index {pointIndex} != points index {expectedIndex}");
            }

            if (!segments.TryGetValue(pointId, out var segment))
            {
                issues.Add($"device_state point_id {pointId} missing matching segment");
                continue;
            }

            ValidateSegmentBinding(root, pointId, segment, issues);
            ValidateRfExposure(root, pointId, segment, issues);
        }

        foreach (var pointId in points.Keys)
        {
            if (!seenDevicePoints.Contains(pointId))
            {
                issues.Add($"points.jsonl point_id {pointId} missing from device_state.jsonl");
            }
        }

        return issues;
    }

    private static Dictionary<string, PointIndex> ReadPointIndexes(string path, List<string> issues)
    {
        var result = new Dictionary<string, PointIndex>(StringComparer.Ordinal);
        var lineNumber = 0;
        foreach (var line in File.ReadLines(path))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var pointId = GetRequiredString(root, "point_id", issues, $"points line {lineNumber}");
            if (string.IsNullOrEmpty(pointId))
            {
                continue;
            }

            if (result.ContainsKey(pointId))
            {
                issues.Add($"points.jsonl duplicate point_id {pointId}");
                continue;
            }

            result.Add(pointId, new PointIndex(pointId, GetNullableInt32(root, "index")));
        }

        return result;
    }

    private static Dictionary<string, SegmentIndex> ReadSegmentIndexes(string path, List<string> issues)
    {
        var result = new Dictionary<string, SegmentIndex>(StringComparer.Ordinal);
        var lineNumber = 0;
        foreach (var line in File.ReadLines(path))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var pointId = GetRequiredString(root, "point_id", issues, $"segments line {lineNumber}");
            var segmentId = GetRequiredString(root, "segment_id", issues, $"segments line {lineNumber}");
            if (string.IsNullOrEmpty(pointId) || string.IsNullOrEmpty(segmentId))
            {
                continue;
            }

            if (result.ContainsKey(pointId))
            {
                issues.Add($"segments.jsonl duplicate point_id {pointId}");
                continue;
            }

            result.Add(pointId, new SegmentIndex(
                pointId,
                segmentId,
                GetRequiredUInt64(root, "start_monotonic_ns", issues, $"segments point_id {pointId}"),
                GetRequiredUInt64(root, "end_monotonic_ns", issues, $"segments point_id {pointId}"),
                GetRequiredInt64(root, "raw_offset_start", issues, $"segments point_id {pointId}"),
                GetRequiredInt64(root, "raw_offset_end", issues, $"segments point_id {pointId}"),
                GetNullableInt64(root, "frame_seq_start"),
                GetNullableInt64(root, "frame_seq_end")));
        }

        return result;
    }

    private static void ValidateSegmentBinding(
        JsonElement root,
        string pointId,
        SegmentIndex segment,
        List<string> issues)
    {
        if (!root.TryGetProperty("segment", out var binding) || binding.ValueKind != JsonValueKind.Object)
        {
            issues.Add($"device_state point_id {pointId} missing segment binding");
            return;
        }

        var boundSegmentId = GetRequiredString(binding, "segment_id", issues, $"device_state point_id {pointId} segment");
        if (!string.Equals(boundSegmentId, segment.SegmentId, StringComparison.Ordinal))
        {
            issues.Add($"device_state point_id {pointId} segment_id {boundSegmentId} != segments {segment.SegmentId}");
        }

        CompareInt64(binding, "raw_offset_start", segment.RawOffsetStart, pointId, issues);
        CompareInt64(binding, "raw_offset_end", segment.RawOffsetEnd, pointId, issues);
        CompareNullableInt64(binding, "frame_seq_start", segment.FrameSeqStart, pointId, issues);
        CompareNullableInt64(binding, "frame_seq_end", segment.FrameSeqEnd, pointId, issues);
    }

    private static void ValidateRfExposure(
        JsonElement root,
        string pointId,
        SegmentIndex segment,
        List<string> issues)
    {
        if (!root.TryGetProperty("rf_exposure", out var exposure) || exposure.ValueKind != JsonValueKind.Object)
        {
            issues.Add($"device_state point_id {pointId} missing rf_exposure");
            return;
        }

        var started = GetRequiredUInt64(exposure, "started_monotonic_ns", issues, $"device_state point_id {pointId} rf_exposure");
        var ended = GetRequiredUInt64(exposure, "ended_monotonic_ns", issues, $"device_state point_id {pointId} rf_exposure");
        var segmentStart = GetRequiredUInt64(exposure, "segment_start_monotonic_ns", issues, $"device_state point_id {pointId} rf_exposure");
        var segmentEnd = GetRequiredUInt64(exposure, "segment_end_monotonic_ns", issues, $"device_state point_id {pointId} rf_exposure");

        if (segmentStart != segment.StartMonotonicNs)
        {
            issues.Add($"device_state point_id {pointId} rf_exposure segment_start_monotonic_ns {segmentStart} != segments {segment.StartMonotonicNs}");
        }

        if (segmentEnd != segment.EndMonotonicNs)
        {
            issues.Add($"device_state point_id {pointId} rf_exposure segment_end_monotonic_ns {segmentEnd} != segments {segment.EndMonotonicNs}");
        }

        if (started > ended)
        {
            issues.Add($"device_state point_id {pointId} rf_exposure start after end");
        }

        if (started < segment.StartMonotonicNs || ended > segment.EndMonotonicNs)
        {
            issues.Add($"device_state point_id {pointId} rf_exposure outside segment window");
        }
    }

    private static void CompareInt64(JsonElement element, string propertyName, long expected, string pointId, List<string> issues)
    {
        var actual = GetRequiredInt64(element, propertyName, issues, $"device_state point_id {pointId} segment");
        if (actual != expected)
        {
            issues.Add($"device_state point_id {pointId} segment {propertyName} {actual} != segments {expected}");
        }
    }

    private static void CompareNullableInt64(JsonElement element, string propertyName, long? expected, string pointId, List<string> issues)
    {
        var actual = GetNullableInt64(element, propertyName);
        if (actual != expected)
        {
            issues.Add($"device_state point_id {pointId} segment {propertyName} {actual?.ToString() ?? "null"} != segments {expected?.ToString() ?? "null"}");
        }
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

    private static string GetRequiredString(JsonElement element, string propertyName, List<string> issues, string context)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            issues.Add($"{context} missing string {propertyName}");
            return "";
        }

        return property.GetString() ?? "";
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

    private static long GetRequiredInt64(JsonElement element, string propertyName, List<string> issues, string context)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetInt64(out var parsed))
        {
            return parsed;
        }

        issues.Add($"{context} missing number {propertyName}");
        return 0;
    }

    private static long? GetNullableInt64(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var parsed)
            ? parsed
            : null;
    }

    private static int? GetNullableInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    private static ulong GetRequiredUInt64(JsonElement element, string propertyName, List<string> issues, string context)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Number &&
            property.TryGetUInt64(out var parsed))
        {
            return parsed;
        }

        issues.Add($"{context} missing number {propertyName}");
        return 0;
    }
}
