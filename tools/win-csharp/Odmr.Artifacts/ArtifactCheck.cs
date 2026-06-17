using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed record ArtifactCheckReport(
    [property: JsonPropertyName("run_dir")] string RunDir,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("run_id")] string? RunId,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("samples_total")] long SamplesTotal,
    [property: JsonPropertyName("collector_frame_rows")] long CollectorFrameRows,
    [property: JsonPropertyName("collector_rows_match_frames")] bool CollectorRowsMatchFrames,
    [property: JsonPropertyName("parameter_rows")] long ParameterRows,
    [property: JsonPropertyName("parameter_rows_match_frames")] bool ParameterRowsMatchFrames,
    [property: JsonPropertyName("sample_rows")] long SampleRows,
    [property: JsonPropertyName("sample_rows_match_total")] bool SampleRowsMatchTotal,
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
        "collector_frames.jsonl",
        "parameter_values.csv",
        "sample_values.csv",
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
        var collectorFramesPath = Path.Combine(runDir, "collector_frames.jsonl");
        var parameterValuesPath = Path.Combine(runDir, "parameter_values.csv");
        var sampleValuesPath = Path.Combine(runDir, "sample_values.csv");
        var segmentsPath = Path.Combine(runDir, "segments.jsonl");
        var pointsPath = Path.Combine(runDir, "points.jsonl");
        var qualityPath = Path.Combine(runDir, "quality.jsonl");
        var deviceStatePath = Path.Combine(runDir, "device_state.jsonl");
        var eventsPath = Path.Combine(runDir, "events.jsonl");

        var summary = File.Exists(summaryPath) ? ReadObject(summaryPath) : null;
        var manifest = File.Exists(manifestPath) ? ReadObject(manifestPath) : null;
        var framesTotal = GetInt64(summary, "frames_total") ?? GetInt64(summary, "frames_ok") ?? 0;
        var samplesTotal = GetInt64(summary, "samples_total") ?? 0;
        var runId = GetString(summary, "run_id");
        var summaryStatus = GetString(summary, "status");
        var manifestStatus = GetString(manifest, "status");
        var collectorFrameRows = File.Exists(collectorFramesPath) ? CountLines(collectorFramesPath) : 0;
        var parameterRows = File.Exists(parameterValuesPath) ? CountCsvDataRows(parameterValuesPath) : 0;
        var sampleRows = File.Exists(sampleValuesPath) ? CountCsvDataRows(sampleValuesPath) : 0;
        var segmentsCount = File.Exists(segmentsPath) ? CountLines(segmentsPath) : 0;
        var pointsCount = File.Exists(pointsPath) ? CountLines(pointsPath) : 0;
        var qualityCount = File.Exists(qualityPath) ? CountLines(qualityPath) : 0;
        var deviceStateCount = File.Exists(deviceStatePath) ? CountLines(deviceStatePath) : 0;
        var qualityStatusCounts = File.Exists(qualityPath)
            ? CountJsonlStringProperty(qualityPath, "quality_status")
            : new Dictionary<string, long>(StringComparer.Ordinal);
        var aborted = string.Equals(summaryStatus, "aborted", StringComparison.Ordinal) ||
            string.Equals(manifestStatus, "aborted", StringComparison.Ordinal);
        var deviceStateIssues = ValidateDeviceState(pointsPath, segmentsPath, deviceStatePath);
        var deviceStateConsistent = deviceStateIssues.Count == 0;
        var rfExposureWindowsCovered = !deviceStateIssues.Any(issue =>
            issue.Contains("rf_exposure", StringComparison.Ordinal) ||
            issue.Contains("segment window", StringComparison.Ordinal));
        var eventsPresent = File.Exists(eventsPath)
            ? DistinctJsonlStringProperty(eventsPath, "event")
            : [];
        var eventSet = eventsPresent.ToHashSet(StringComparer.Ordinal);
        var requiredEvents = aborted
            ? RequiredEventNames
                .Where(eventName => eventName is
                    "run_opened" or
                    "collector_started" or
                    "oe_profile_applied" or
                    "laser_profile_applied" or
                    "collector_stopped" or
                    "cleanup_completed")
                .ToArray()
            : RequiredEventNames;
        var missingEvents = requiredEvents
            .Where(eventName => !eventSet.Contains(eventName))
            .ToList();
        if (!eventSet.Contains("run_completed") && !eventSet.Contains("run_failed") && !eventSet.Contains("run_aborted"))
        {
            missingEvents.Add("run_completed|run_failed|run_aborted");
        }

        var collectorRowsMatchFrames = collectorFrameRows == framesTotal;
        var parameterRowsMatchFrames = parameterRows == framesTotal;
        var sampleRowsMatchTotal = sampleRows == samplesTotal;
        var recordCountsConsistent = aborted
            ? segmentsCount == pointsCount &&
                pointsCount == qualityCount &&
                qualityCount == deviceStateCount
            : segmentsCount == pointsCount &&
                pointsCount == qualityCount &&
                qualityCount == deviceStateCount;
        var manifestStatusMatchesSummary = summaryStatus is not null &&
            manifestStatus is not null &&
            string.Equals(summaryStatus, manifestStatus, StringComparison.Ordinal);
        var passed = missingFiles.Length == 0 &&
            missingSnapshots.Length == 0 &&
            collectorRowsMatchFrames &&
            parameterRowsMatchFrames &&
            sampleRowsMatchTotal &&
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
            samplesTotal,
            collectorFrameRows,
            collectorRowsMatchFrames,
            parameterRows,
            parameterRowsMatchFrames,
            sampleRows,
            sampleRowsMatchTotal,
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

    private sealed record PointIndex(string PointId, int? Index, string? MagneticMode, bool? M8812Commanded);

    private sealed record SegmentIndex(
        string PointId,
        string SegmentId,
        ulong StartMonotonicNs,
        ulong EndMonotonicNs,
        long? BlockSeqStart,
        long? BlockSeqEnd,
        long SampleIndexStart,
        long SampleIndexEnd);

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
            else
            {
                if (GetNullableInt32(root, "point_index") is { } pointIndex && point.Index is { } expectedIndex && pointIndex != expectedIndex)
                {
                    issues.Add($"device_state point_id {pointId} point_index {pointIndex} != points index {expectedIndex}");
                }

                ValidateDevicePointContext(root, pointId, point, issues);
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

            var magneticMode = GetRequiredString(root, "magnetic_mode", issues, $"points line {lineNumber}");
            var m8812Commanded = GetNullableBool(root, "m8812_commanded");
            ValidatePointContext(root, pointId, magneticMode, m8812Commanded, issues, $"points line {lineNumber}");

            result.Add(pointId, new PointIndex(pointId, GetNullableInt32(root, "index"), magneticMode, m8812Commanded));
        }

        return result;
    }

    private static void ValidatePointContext(
        JsonElement root,
        string pointId,
        string? magneticMode,
        bool? m8812Commanded,
        List<string> issues,
        string context)
    {
        var pointKind = GetRequiredString(root, "point_kind", issues, context);
        if (!string.Equals(pointKind, "acquisition_step", StringComparison.Ordinal))
        {
            issues.Add($"{context} point_id {pointId} point_kind {pointKind} != acquisition_step");
        }

        if (m8812Commanded is null)
        {
            issues.Add($"{context} point_id {pointId} missing m8812_commanded");
        }

        switch (magneticMode)
        {
            case "controlled":
                if (m8812Commanded != true)
                {
                    issues.Add($"{context} point_id {pointId} controlled point must have m8812_commanded=true");
                }
                RequireArrayLength(root, "target_b_nt", 3, pointId, issues, context);
                RequireArrayLength(root, "baseline_current_a", 3, pointId, issues, context);
                RequireArrayLength(root, "calibrated_delta_current_a", 3, pointId, issues, context);
                RequireArrayLength(root, "target_current_a", 3, pointId, issues, context);
                break;
            case "none":
                if (m8812Commanded != false)
                {
                    issues.Add($"{context} point_id {pointId} none point must have m8812_commanded=false");
                }
                RequireNull(root, "target_b_nt", pointId, issues, context);
                RequireNull(root, "baseline_current_a", pointId, issues, context);
                RequireNull(root, "calibrated_delta_current_a", pointId, issues, context);
                RequireNull(root, "target_current_a", pointId, issues, context);
                break;
            default:
                issues.Add($"{context} point_id {pointId} unsupported magnetic_mode {magneticMode}");
                break;
        }
    }

    private static void ValidateDevicePointContext(
        JsonElement root,
        string pointId,
        PointIndex point,
        List<string> issues)
    {
        var magneticMode = GetRequiredString(root, "magnetic_mode", issues, $"device_state point_id {pointId}");
        var m8812Commanded = GetNullableBool(root, "m8812_commanded");
        ValidateDeviceContextFields(root, pointId, magneticMode, m8812Commanded, issues);

        if (!string.Equals(magneticMode, point.MagneticMode, StringComparison.Ordinal))
        {
            issues.Add($"device_state point_id {pointId} magnetic_mode {magneticMode} != points {point.MagneticMode}");
        }
        if (m8812Commanded != point.M8812Commanded)
        {
            issues.Add($"device_state point_id {pointId} m8812_commanded {m8812Commanded?.ToString() ?? "null"} != points {point.M8812Commanded?.ToString() ?? "null"}");
        }
    }

    private static void ValidateDeviceContextFields(
        JsonElement root,
        string pointId,
        string? magneticMode,
        bool? m8812Commanded,
        List<string> issues)
    {
        var pointKind = GetRequiredString(root, "point_kind", issues, $"device_state point_id {pointId}");
        if (!string.Equals(pointKind, "acquisition_step", StringComparison.Ordinal))
        {
            issues.Add($"device_state point_id {pointId} point_kind {pointKind} != acquisition_step");
        }

        switch (magneticMode)
        {
            case "controlled":
                if (m8812Commanded != true)
                {
                    issues.Add($"device_state point_id {pointId} controlled point must have m8812_commanded=true");
                }
                RequireArrayLength(root, "target_b_nt", 3, pointId, issues, "device_state");
                RequireArrayLength(root, "target_current_a", 3, pointId, issues, "device_state");
                RequireArrayLength(root, "measured_current_a", 3, pointId, issues, "device_state");
                break;
            case "none":
                if (m8812Commanded != false)
                {
                    issues.Add($"device_state point_id {pointId} none point must have m8812_commanded=false");
                }
                RequireNull(root, "target_b_nt", pointId, issues, "device_state");
                RequireNull(root, "target_current_a", pointId, issues, "device_state");
                RequireNull(root, "measured_current_a", pointId, issues, "device_state");
                break;
            default:
                issues.Add($"device_state point_id {pointId} unsupported magnetic_mode {magneticMode}");
                break;
        }
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
                GetNullableInt64(root, "block_seq_start"),
                GetNullableInt64(root, "block_seq_end"),
                GetRequiredInt64(root, "sample_index_start", issues, $"segments point_id {pointId}"),
                GetRequiredInt64(root, "sample_index_end", issues, $"segments point_id {pointId}")));
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

        CompareInt64(binding, "sample_index_start", segment.SampleIndexStart, pointId, issues);
        CompareInt64(binding, "sample_index_end", segment.SampleIndexEnd, pointId, issues);
        CompareNullableInt64(binding, "block_seq_start", segment.BlockSeqStart, pointId, issues);
        CompareNullableInt64(binding, "block_seq_end", segment.BlockSeqEnd, pointId, issues);
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

    private static long CountCsvDataRows(string path)
    {
        var count = CountLines(path);
        return count > 0 ? count - 1 : 0;
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

    private static bool? GetNullableBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    private static void RequireArrayLength(JsonElement element, string propertyName, int length, string pointId, List<string> issues, string context)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array ||
            property.GetArrayLength() != length)
        {
            issues.Add($"{context} point_id {pointId} {propertyName} must be array[{length}]");
        }
    }

    private static void RequireNull(JsonElement element, string propertyName, string pointId, List<string> issues, string context)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Null)
        {
            issues.Add($"{context} point_id {pointId} {propertyName} must be null");
        }
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
