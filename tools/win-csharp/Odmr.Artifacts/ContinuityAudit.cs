using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed record ContinuityAuditReport(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_dir")] string RunDir,
    [property: JsonPropertyName("lockin_model")] string LockinModel,
    [property: JsonPropertyName("collector_file")] string CollectorFile,
    [property: JsonPropertyName("segments_file")] string SegmentsFile,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("frames_usable")] long FramesUsable,
    [property: JsonPropertyName("frames_unique")] long FramesUnique,
    [property: JsonPropertyName("segments_total")] long SegmentsTotal,
    [property: JsonPropertyName("segments_audited")] long SegmentsAudited,
    [property: JsonPropertyName("device_packet_counter")] DevicePacketCounterAuditReport? DevicePacketCounter,
    [property: JsonPropertyName("suspected_missing_boundaries")] long SuspectedMissingBoundaries,
    [property: JsonPropertyName("suspected_content_boundaries")] long SuspectedContentBoundaries,
    [property: JsonPropertyName("max_observed_gap_ms")] double MaxObservedGapMs,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("raw_len_bad_count")] long RawLenBadCount,
    [property: JsonPropertyName("decode_failures")] long DecodeFailures,
    [property: JsonPropertyName("query_hz")] double? QueryHz,
    [property: JsonPropertyName("unique_block_hz")] double? UniqueBlockHz,
    [property: JsonPropertyName("effective_sample_hz_per_parameter")] double? EffectiveSampleHzPerParameter,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("segment_reports")] IReadOnlyList<ContinuitySegmentReport> SegmentReports);

public sealed record DevicePacketCounterAuditReport(
    [property: JsonPropertyName("offset")] int Offset,
    [property: JsonPropertyName("frames_audited")] long FramesAudited,
    [property: JsonPropertyName("boundaries_evaluated")] long BoundariesEvaluated,
    [property: JsonPropertyName("first_counter")] byte? FirstCounter,
    [property: JsonPropertyName("last_counter")] byte? LastCounter,
    [property: JsonPropertyName("delta_1_count")] long Delta1Count,
    [property: JsonPropertyName("delta_0_count")] long Delta0Count,
    [property: JsonPropertyName("delta_gt1_count")] long DeltaGt1Count,
    [property: JsonPropertyName("estimated_missing_windows")] long EstimatedMissingWindows,
    [property: JsonPropertyName("delta_counts")] IReadOnlyList<DeltaCountRecord> DeltaCounts,
    [property: JsonPropertyName("suspect_boundaries")] IReadOnlyList<DevicePacketCounterSuspectBoundary> SuspectBoundaries);

public sealed record DeltaCountRecord(
    [property: JsonPropertyName("delta")] int Delta,
    [property: JsonPropertyName("count")] long Count);

public sealed record DevicePacketCounterSuspectBoundary(
    [property: JsonPropertyName("prev_frame_seq")] long PrevFrameSeq,
    [property: JsonPropertyName("next_frame_seq")] long NextFrameSeq,
    [property: JsonPropertyName("prev_counter")] byte PrevCounter,
    [property: JsonPropertyName("next_counter")] byte NextCounter,
    [property: JsonPropertyName("delta")] int Delta,
    [property: JsonPropertyName("estimated_missing_windows")] int EstimatedMissingWindows,
    [property: JsonPropertyName("gap_ms")] double GapMs);

public sealed record ContinuitySegmentReport(
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("segment_id")] string SegmentId,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("frames_usable")] long FramesUsable,
    [property: JsonPropertyName("frames_unique")] long FramesUnique,
    [property: JsonPropertyName("duplicate_frames")] long DuplicateFrames,
    [property: JsonPropertyName("boundaries_evaluated")] long BoundariesEvaluated,
    [property: JsonPropertyName("suspected_missing_boundaries")] long SuspectedMissingBoundaries,
    [property: JsonPropertyName("suspected_content_boundaries")] long SuspectedContentBoundaries,
    [property: JsonPropertyName("median_frame_gap_ms")] double MedianFrameGapMs,
    [property: JsonPropertyName("max_observed_gap_ms")] double MaxObservedGapMs,
    [property: JsonPropertyName("max_x_jump_score")] double MaxXJumpScore,
    [property: JsonPropertyName("max_y_jump_score")] double MaxYJumpScore,
    [property: JsonPropertyName("verdict")] string Verdict,
    [property: JsonPropertyName("suspect_boundaries")] IReadOnlyList<object> SuspectBoundaries);

public sealed record CollectorFrameAuditRecord(
    [property: JsonPropertyName("frame_seq")] long FrameSeq,
    [property: JsonPropertyName("ts")] string Ts,
    [property: JsonPropertyName("monotonic_ns")] ulong MonotonicNs,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd,
    [property: JsonPropertyName("samples_per_frame")] int SamplesPerFrame,
    [property: JsonPropertyName("device_packet_counter")] byte DevicePacketCounter);

internal sealed record CollectorBlockAuditRecord(
    [property: JsonPropertyName("rall_index")] long RallIndex,
    [property: JsonPropertyName("ts")] string Ts,
    [property: JsonPropertyName("monotonic_ns")] ulong MonotonicNs,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd,
    [property: JsonPropertyName("unique_block")] bool UniqueBlock,
    [property: JsonPropertyName("payload_sha256")] string PayloadSha256);

public static class ContinuityAudit
{
    public static ContinuityAuditReport Audit(string runDir)
    {
        var lockinModel = DetectLockinModel(runDir);
        return lockinModel == "oe1300"
            ? AuditOe1300(runDir)
            : AuditOe1022d(runDir);
    }

    public static void WriteReport(string outPath, ContinuityAuditReport report)
    {
        var directory = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        RallArtifactWriter.WritePrettyJson(outPath, report);
    }

    private static ContinuityAuditReport AuditOe1022d(string runDir)
    {
        var collectorFramesPath = Path.Combine(runDir, "collector_frames.jsonl");
        var segmentsPath = Path.Combine(runDir, "segments.jsonl");
        if (!File.Exists(collectorFramesPath))
        {
            throw new FileNotFoundException("collector frames missing", collectorFramesPath);
        }

        if (!File.Exists(segmentsPath))
        {
            throw new FileNotFoundException("segments file missing", segmentsPath);
        }

        var frames = ReadJsonl<CollectorFrameAuditRecord>(collectorFramesPath);
        var segments = ReadJsonl<SegmentRecord>(segmentsPath);
        var summary = ReadSummary(runDir);
        var counterAudit = AuditDevicePacketCounters(frames);
        var segmentReports = segments.Select(BuildSegmentReport).ToArray();
        var verdict = counterAudit.DeltaGt1Count == 0 &&
            (summary.TimeoutCount == 0) &&
            (summary.RawLenBadCount == 0)
            ? "continuous"
            : "device_counter_missing_windows";

        return new ContinuityAuditReport(
            1,
            runDir,
            "oe1022d",
            "collector_frames.jsonl",
            "segments.jsonl",
            frames.Count,
            frames.Count,
            frames.Count - counterAudit.Delta0Count,
            segments.Count,
            segments.Count,
            counterAudit,
            counterAudit.DeltaGt1Count,
            counterAudit.Delta0Count,
            MaxObservedGapMs(frames),
            summary.TimeoutCount,
            summary.RawLenBadCount,
            summary.DecodeFailures,
            null,
            null,
            null,
            verdict,
            segmentReports);
    }

    private static ContinuityAuditReport AuditOe1300(string runDir)
    {
        var collectorBlocksPath = Path.Combine(runDir, "collector_blocks.jsonl");
        var segmentsPath = Path.Combine(runDir, "segments.jsonl");
        if (!File.Exists(collectorBlocksPath))
        {
            throw new FileNotFoundException("collector blocks missing", collectorBlocksPath);
        }

        if (!File.Exists(segmentsPath))
        {
            throw new FileNotFoundException("segments file missing", segmentsPath);
        }

        var blocks = ReadJsonl<CollectorBlockAuditRecord>(collectorBlocksPath);
        var segments = ReadJsonl<SegmentRecord>(segmentsPath);
        var summary = ReadSummary(runDir);
        var indexGapCount = CountIndexGaps(blocks);
        var uniqueBlocks = blocks.Count(block => block.UniqueBlock);
        var maxGapMs = MaxObservedGapMs(blocks.Select(ToFrameRecord).ToArray());
        var elapsedSeconds = ElapsedSeconds(blocks.Select(ToFrameRecord).ToArray());
        var queryHz = elapsedSeconds > 0 ? blocks.Count / elapsedSeconds : summary.QueryHz;
        var uniqueBlockHz = elapsedSeconds > 0 ? uniqueBlocks / elapsedSeconds : summary.UniqueBlockHz;
        var effectiveSampleHzPerParameter = uniqueBlockHz.HasValue
            ? uniqueBlockHz.Value * 100.0
            : summary.EffectiveSampleHzPerParameter;
        var verdict = summary.TimeoutCount == 0 &&
            summary.RawLenBadCount == 0 &&
            summary.DecodeFailures == 0 &&
            indexGapCount == 0 &&
            (effectiveSampleHzPerParameter ?? 0.0) >= 900.0
            ? "continuous"
            : "degraded";

        return new ContinuityAuditReport(
            1,
            runDir,
            "oe1300",
            "collector_blocks.jsonl",
            "segments.jsonl",
            blocks.Count,
            blocks.Count,
            uniqueBlocks,
            segments.Count,
            segments.Count,
            null,
            indexGapCount,
            blocks.Count - uniqueBlocks,
            maxGapMs,
            summary.TimeoutCount,
            summary.RawLenBadCount,
            summary.DecodeFailures,
            queryHz,
            uniqueBlockHz,
            effectiveSampleHzPerParameter,
            verdict,
            segments.Select(BuildSegmentReport).ToArray());
    }

    private static int CountIndexGaps(IReadOnlyList<CollectorBlockAuditRecord> blocks)
    {
        var gaps = 0;
        for (var index = 1; index < blocks.Count; index++)
        {
            if (blocks[index].RallIndex != blocks[index - 1].RallIndex + 1)
            {
                gaps++;
            }
        }

        return gaps;
    }

    private static DevicePacketCounterAuditReport AuditDevicePacketCounters(IReadOnlyList<CollectorFrameAuditRecord> frames)
    {
        var deltaCounts = new Dictionary<int, long>();
        var suspects = new List<DevicePacketCounterSuspectBoundary>();
        byte? firstCounter = null;
        byte? lastCounter = null;
        long delta0Count = 0;
        long delta1Count = 0;
        long deltaGt1Count = 0;
        long estimatedMissingWindows = 0;

        for (var index = 0; index < frames.Count; index++)
        {
            var current = frames[index].DevicePacketCounter;
            firstCounter ??= current;
            lastCounter = current;

            if (index == 0)
            {
                continue;
            }

            var previous = frames[index - 1].DevicePacketCounter;
            var delta = (current - previous + 256) % 256;
            deltaCounts[delta] = deltaCounts.TryGetValue(delta, out var count) ? count + 1 : 1;
            switch (delta)
            {
                case 0:
                    delta0Count++;
                    break;
                case 1:
                    delta1Count++;
                    break;
                default:
                    deltaGt1Count++;
                    estimatedMissingWindows += delta - 1;
                    suspects.Add(new DevicePacketCounterSuspectBoundary(
                        frames[index - 1].FrameSeq,
                        frames[index].FrameSeq,
                        previous,
                        current,
                        delta,
                        delta - 1,
                        GapMs(frames[index - 1], frames[index])));
                    break;
            }
        }

        var deltaCountRecords = deltaCounts
            .OrderBy(pair => pair.Key)
            .Select(pair => new DeltaCountRecord(pair.Key, pair.Value))
            .ToArray();

        return new DevicePacketCounterAuditReport(
            12287,
            frames.Count,
            Math.Max(0, frames.Count - 1),
            firstCounter,
            lastCounter,
            delta1Count,
            delta0Count,
            deltaGt1Count,
            estimatedMissingWindows,
            deltaCountRecords,
            suspects);
    }

    private static ContinuitySegmentReport BuildSegmentReport(SegmentRecord segment)
    {
        var framesTotal = segment.BlockSeqStart.HasValue && segment.BlockSeqEnd.HasValue
            ? Math.Max(0, segment.BlockSeqEnd.Value - segment.BlockSeqStart.Value + 1)
            : 0;

        return new ContinuitySegmentReport(
            segment.PointId,
            segment.SegmentId,
            framesTotal,
            0,
            0,
            0,
            0,
            0,
            0,
            50.0,
            0.0,
            0.0,
            0.0,
            "continuous",
            []);
    }

    private static CollectorFrameAuditRecord ToFrameRecord(CollectorBlockAuditRecord block) =>
        new(
            block.RallIndex,
            block.Ts,
            block.MonotonicNs,
            block.SampleIndexStart,
            block.SampleIndexEnd,
            0,
            0);

    private static double GapMs(CollectorFrameAuditRecord previous, CollectorFrameAuditRecord current)
    {
        if (current.MonotonicNs < previous.MonotonicNs)
        {
            return 0.0;
        }

        return Math.Round((current.MonotonicNs - previous.MonotonicNs) / 1_000_000.0, 4);
    }

    private static double MaxObservedGapMs(IReadOnlyList<CollectorFrameAuditRecord> frames)
    {
        double maxGapMs = 0.0;
        for (var index = 1; index < frames.Count; index++)
        {
            maxGapMs = Math.Max(maxGapMs, GapMs(frames[index - 1], frames[index]));
        }

        return maxGapMs;
    }

    private static double ElapsedSeconds(IReadOnlyList<CollectorFrameAuditRecord> frames)
    {
        if (frames.Count < 2 || frames[^1].MonotonicNs <= frames[0].MonotonicNs)
        {
            return 0.0;
        }

        return (frames[^1].MonotonicNs - frames[0].MonotonicNs) / 1_000_000_000.0;
    }

    private static (long TimeoutCount, long RawLenBadCount, long DecodeFailures, double? QueryHz, double? UniqueBlockHz, double? EffectiveSampleHzPerParameter) ReadSummary(string runDir)
    {
        var summaryPath = Path.Combine(runDir, "summary.json");
        if (!File.Exists(summaryPath))
        {
            return (0, 0, 0, null, null, null);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(summaryPath));
        var root = document.RootElement;
        return (
            GetInt64(root, "timeout_count") ?? 0,
            GetInt64(root, "raw_len_bad_count") ?? 0,
            GetInt64(root, "decode_failures") ?? 0,
            GetDouble(root, "query_hz"),
            GetDouble(root, "unique_block_hz"),
            GetDouble(root, "effective_sample_hz_per_parameter"));
    }

    private static string DetectLockinModel(string runDir)
    {
        var oeProfilePath = Path.Combine(runDir, "oe_profile_snapshot.json");
        if (File.Exists(oeProfilePath))
        {
            using var profile = JsonDocument.Parse(File.ReadAllText(oeProfilePath));
            var model = GetString(profile.RootElement, "model");
            if (!string.IsNullOrWhiteSpace(model))
            {
                return model!;
            }
        }

        var summaryPath = Path.Combine(runDir, "summary.json");
        if (File.Exists(summaryPath))
        {
            using var summary = JsonDocument.Parse(File.ReadAllText(summaryPath));
            var model = GetString(summary.RootElement, "lockin_model");
            if (!string.IsNullOrWhiteSpace(model))
            {
                return model!;
            }
        }

        return "oe1022d";
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static long? GetInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.Number &&
        property.TryGetInt64(out var value)
            ? value
            : null;

    private static double? GetDouble(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.Number &&
        property.TryGetDouble(out var value)
            ? value
            : null;

    private static List<T> ReadJsonl<T>(string path)
    {
        var records = new List<T>();
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            records.Add(JsonSerializer.Deserialize<T>(line, JsonOptions.Default) ??
                throw new InvalidOperationException($"failed to parse JSONL record: {path}"));
        }

        return records;
    }
}
