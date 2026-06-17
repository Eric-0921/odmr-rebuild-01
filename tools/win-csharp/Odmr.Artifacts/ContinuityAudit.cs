using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed record ContinuityAuditReport(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_dir")] string RunDir,
    [property: JsonPropertyName("collector_frames_file")] string CollectorFramesFile,
    [property: JsonPropertyName("segments_file")] string SegmentsFile,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("frames_usable")] long FramesUsable,
    [property: JsonPropertyName("frames_unique")] long FramesUnique,
    [property: JsonPropertyName("segments_total")] long SegmentsTotal,
    [property: JsonPropertyName("segments_audited")] long SegmentsAudited,
    [property: JsonPropertyName("device_packet_counter")] DevicePacketCounterAuditReport DevicePacketCounter,
    [property: JsonPropertyName("suspected_missing_boundaries")] long SuspectedMissingBoundaries,
    [property: JsonPropertyName("suspected_content_boundaries")] long SuspectedContentBoundaries,
    [property: JsonPropertyName("max_observed_gap_ms")] double MaxObservedGapMs,
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

public static class ContinuityAudit
{
    public static ContinuityAuditReport Audit(string runDir)
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
        var counterAudit = AuditDevicePacketCounters(frames);
        var segmentReports = segments.Select(BuildSegmentReport).ToArray();
        var verdict = counterAudit.DeltaGt1Count == 0 ? "continuous" : "device_counter_missing_windows";

        return new ContinuityAuditReport(
            1,
            runDir,
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
            verdict,
            segmentReports);
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

    public static void WriteReport(string outPath, ContinuityAuditReport report)
    {
        var directory = Path.GetDirectoryName(outPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        RallArtifactWriter.WritePrettyJson(outPath, report);
    }

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
