using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed class ProbeStats
{
    public long ReadAttempts { get; set; }
    public long FramesOk { get; set; }
    public long ReadErrors { get; set; }
    public long TimeoutCount { get; set; }
    public long RawLenBadCount { get; set; }
}

public sealed class PacketCounterAudit
{
    private byte? previous;

    public byte? FirstCounter { get; private set; }
    public byte? LastCounter { get; private set; }
    public long Delta0Count { get; private set; }
    public long Delta1Count { get; private set; }
    public long DeltaGt1Count { get; private set; }
    public long EstimatedMissingWindows { get; private set; }

    public void Record(byte counter)
    {
        FirstCounter ??= counter;

        if (previous.HasValue)
        {
            var delta = (counter - previous.Value + 256) % 256;
            switch (delta)
            {
                case 0:
                    Delta0Count++;
                    break;
                case 1:
                    Delta1Count++;
                    break;
                default:
                    DeltaGt1Count++;
                    EstimatedMissingWindows += delta - 1;
                    break;
            }
        }

        previous = counter;
        LastCounter = counter;
    }

    public PacketCounterSummary ToSummary() =>
        new(FirstCounter, LastCounter, Delta0Count, Delta1Count, DeltaGt1Count, EstimatedMissingWindows);
}

public sealed record PacketCounterSummary(
    [property: JsonPropertyName("first_counter")] byte? FirstCounter,
    [property: JsonPropertyName("last_counter")] byte? LastCounter,
    [property: JsonPropertyName("delta0_count")] long Delta0Count,
    [property: JsonPropertyName("delta1_count")] long Delta1Count,
    [property: JsonPropertyName("delta_gt1_count")] long DeltaGt1Count,
    [property: JsonPropertyName("estimated_missing_windows")] long EstimatedMissingWindows);

public sealed record ProbeSummary(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("resource")] string Resource,
    [property: JsonPropertyName("baud_rate")] int BaudRate,
    [property: JsonPropertyName("frame_bytes")] int FrameBytes,
    [property: JsonPropertyName("post_write_delay_ms")] int PostWriteDelayMs,
    [property: JsonPropertyName("visa_timeout_ms")] int VisaTimeoutMs,
    [property: JsonPropertyName("duration_sec")] int DurationSec,
    [property: JsonPropertyName("started_at")] string StartedAt,
    [property: JsonPropertyName("finished_at")] string FinishedAt,
    [property: JsonPropertyName("elapsed_ms")] long ElapsedMs,
    [property: JsonPropertyName("read_attempts")] long ReadAttempts,
    [property: JsonPropertyName("frames_ok")] long FramesOk,
    [property: JsonPropertyName("read_errors")] long ReadErrors,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("raw_len_bad_count")] long RawLenBadCount,
    [property: JsonPropertyName("raw_bytes_written")] long RawBytesWritten,
    [property: JsonPropertyName("raw_size_matches_frames_ok")] bool RawSizeMatchesFramesOk,
    [property: JsonPropertyName("raw_path")] string RawPath,
    [property: JsonPropertyName("index_path")] string IndexPath,
    [property: JsonPropertyName("segments_path")] string SegmentsPath,
    [property: JsonPropertyName("packet_counter")] PacketCounterSummary PacketCounter);

public sealed record SegmentRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("segment_id")] string SegmentId,
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("start_ts")] string StartTs,
    [property: JsonPropertyName("end_ts")] string EndTs,
    [property: JsonPropertyName("start_monotonic_ns")] ulong StartMonotonicNs,
    [property: JsonPropertyName("end_monotonic_ns")] ulong EndMonotonicNs,
    [property: JsonPropertyName("raw_file")] string RawFile,
    [property: JsonPropertyName("raw_offset_start")] long RawOffsetStart,
    [property: JsonPropertyName("raw_offset_end")] long RawOffsetEnd,
    [property: JsonPropertyName("frame_seq_start")] long? FrameSeqStart,
    [property: JsonPropertyName("frame_seq_end")] long? FrameSeqEnd);

public static class RallArtifactWriter
{
    public static void WriteFrameIndexRecord(
        StreamWriter writer,
        long frameSeq,
        string ts,
        ulong monotonicNs,
        long rawOffset,
        int rawLen,
        byte devicePacketCounter)
    {
        writer.Write("{\"frame_seq\":");
        writer.Write(frameSeq.ToString(CultureInfo.InvariantCulture));
        writer.Write(",\"ts\":\"");
        writer.Write(ts);
        writer.Write("\",\"monotonic_ns\":");
        writer.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
        writer.Write(",\"raw_offset\":");
        writer.Write(rawOffset.ToString(CultureInfo.InvariantCulture));
        writer.Write(",\"raw_len\":");
        writer.Write(rawLen.ToString(CultureInfo.InvariantCulture));
        writer.Write(",\"device_packet_counter\":");
        writer.Write(devicePacketCounter.ToString(CultureInfo.InvariantCulture));
        writer.Write(",\"parse_status\":\"not_parsed\",\"duplicate_of\":null}");
        writer.WriteLine();
    }

    public static void WriteWholeProbeSegment(
        string segmentsPath,
        string outDir,
        string startTs,
        string endTs,
        ulong startMonotonicNs,
        ulong endMonotonicNs,
        long rawOffsetEnd,
        long framesOk)
    {
        var segment = new SegmentRecord(
            1,
            Path.GetFileName(Path.GetFullPath(outDir)),
            "seg_oe_rall_probe_0000",
            "oe_rall_probe",
            "oe1022d_main",
            startTs,
            endTs,
            startMonotonicNs,
            endMonotonicNs,
            "raw/oe1022d.rall",
            0,
            rawOffsetEnd,
            framesOk > 0 ? (long?)0 : null,
            framesOk > 0 ? framesOk - 1 : null);

        File.WriteAllText(segmentsPath, JsonSerializer.Serialize(segment, JsonOptions.Default) + Environment.NewLine, new UTF8Encoding(false));
    }
}

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public static readonly JsonSerializerOptions Pretty = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true
    };
}
