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
    [property: JsonPropertyName("source_file")] string SourceFile,
    [property: JsonPropertyName("block_seq_start")] long? BlockSeqStart,
    [property: JsonPropertyName("block_seq_end")] long? BlockSeqEnd,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd);

public sealed record CollectorFrameRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("frame_seq")] long FrameSeq,
    [property: JsonPropertyName("ts")] string Ts,
    [property: JsonPropertyName("monotonic_ns")] ulong MonotonicNs,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd,
    [property: JsonPropertyName("samples_per_frame")] int SamplesPerFrame,
    [property: JsonPropertyName("device_packet_counter")] byte DevicePacketCounter,
    [property: JsonPropertyName("b_ref_source_code")] byte BRefSourceCode,
    [property: JsonPropertyName("b_ref_slope_code")] byte BRefSlopeCode,
    [property: JsonPropertyName("b_ref_current_freq_hz")] double BRefCurrentFreqHz,
    [property: JsonPropertyName("b_input_overload")] byte BInputOverload,
    [property: JsonPropertyName("b_gain_overload")] byte BGainOverload,
    [property: JsonPropertyName("b_pll_locked")] byte BPllLocked);

public sealed record CollectorBlockRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("rall_index")] long RallIndex,
    [property: JsonPropertyName("ts")] string Ts,
    [property: JsonPropertyName("monotonic_ns")] ulong MonotonicNs,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd,
    [property: JsonPropertyName("unique_block")] bool UniqueBlock,
    [property: JsonPropertyName("unique_block_index")] long UniqueBlockIndex);

public sealed record BaselineAxisSnapshot(
    [property: JsonPropertyName("axis")] string Axis,
    [property: JsonPropertyName("zero_offset_setpoint_a")] double ZeroOffsetSetpointA,
    [property: JsonPropertyName("zero_offset_measured_samples_a")] IReadOnlyList<double> ZeroOffsetMeasuredSamplesA,
    [property: JsonPropertyName("locked_zero_offset_current_a")] double? LockedZeroOffsetCurrentA);

public sealed record BaselineSnapshot(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("baseline_locked_at")] string BaselineLockedAt,
    [property: JsonPropertyName("settle_ms")] int SettleMs,
    [property: JsonPropertyName("readback_samples")] int ReadbackSamples,
    [property: JsonPropertyName("settle_tolerance_a")] double SettleToleranceA,
    [property: JsonPropertyName("axes")] IReadOnlyList<BaselineAxisSnapshot> Axes);

public sealed record SmbSweepRecord(
    [property: JsonPropertyName("start_hz")] long StartHz,
    [property: JsonPropertyName("stop_hz")] long StopHz,
    [property: JsonPropertyName("step_hz")] long StepHz,
    [property: JsonPropertyName("dwell_ms")] int DwellMs,
    [property: JsonPropertyName("power_dbm")] double PowerDbm,
    [property: JsonPropertyName("sweep_mode")] string SweepMode,
    [property: JsonPropertyName("spacing")] string Spacing,
    [property: JsonPropertyName("shape")] string Shape,
    [property: JsonPropertyName("trigger_source")] string TriggerSource,
    [property: JsonPropertyName("output_voltage_start_v")] double OutputVoltageStartV,
    [property: JsonPropertyName("output_voltage_stop_v")] double OutputVoltageStopV,
    [property: JsonPropertyName("rf_output_enabled")] bool RfOutputEnabled);

public sealed record SettleRecord(
    [property: JsonPropertyName("policy")] string Policy,
    [property: JsonPropertyName("started_at")] string StartedAt,
    [property: JsonPropertyName("settled_at")] string SettledAt,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("measured_current_a")] IReadOnlyList<double>? MeasuredCurrentA);

public sealed record PointRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("point_kind")] string PointKind,
    [property: JsonPropertyName("magnetic_mode")] string MagneticMode,
    [property: JsonPropertyName("m8812_commanded")] bool M8812Commanded,
    [property: JsonPropertyName("target_b_nt")] IReadOnlyList<double>? TargetBNt,
    [property: JsonPropertyName("baseline_current_a")] IReadOnlyList<double>? BaselineCurrentA,
    [property: JsonPropertyName("calibrated_delta_current_a")] IReadOnlyList<double>? CalibratedDeltaCurrentA,
    [property: JsonPropertyName("target_current_a")] IReadOnlyList<double>? TargetCurrentA,
    [property: JsonPropertyName("rf")] SmbSweepRecord Rf,
    [property: JsonPropertyName("settle")] SettleRecord Settle);

public sealed record SmbSweepExecutionRecord(
    [property: JsonPropertyName("estimated_sweep_duration_ms")] long EstimatedSweepDurationMs,
    [property: JsonPropertyName("opc_wait_ms")] long OpcWaitMs,
    [property: JsonPropertyName("fallback_used")] bool FallbackUsed);

public sealed record RfExposureWindowRecord(
    [property: JsonPropertyName("policy")] string Policy,
    [property: JsonPropertyName("started_ts")] string StartedTs,
    [property: JsonPropertyName("ended_ts")] string EndedTs,
    [property: JsonPropertyName("started_monotonic_ns")] ulong StartedMonotonicNs,
    [property: JsonPropertyName("ended_monotonic_ns")] ulong EndedMonotonicNs,
    [property: JsonPropertyName("segment_start_monotonic_ns")] ulong SegmentStartMonotonicNs,
    [property: JsonPropertyName("segment_end_monotonic_ns")] ulong SegmentEndMonotonicNs);

public sealed record SegmentBindingRecord(
    [property: JsonPropertyName("segment_id")] string SegmentId,
    [property: JsonPropertyName("block_seq_start")] long? BlockSeqStart,
    [property: JsonPropertyName("block_seq_end")] long? BlockSeqEnd,
    [property: JsonPropertyName("sample_index_start")] long SampleIndexStart,
    [property: JsonPropertyName("sample_index_end")] long SampleIndexEnd);

public sealed record DeviceStateRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("point_index")] int PointIndex,
    [property: JsonPropertyName("point_kind")] string PointKind,
    [property: JsonPropertyName("magnetic_mode")] string MagneticMode,
    [property: JsonPropertyName("m8812_commanded")] bool M8812Commanded,
    [property: JsonPropertyName("target_b_nt")] IReadOnlyList<double>? TargetBNt,
    [property: JsonPropertyName("target_current_a")] IReadOnlyList<double>? TargetCurrentA,
    [property: JsonPropertyName("measured_current_a")] IReadOnlyList<double>? MeasuredCurrentA,
    [property: JsonPropertyName("smb_profile_id")] string SmbProfileId,
    [property: JsonPropertyName("smb_sweep")] SmbSweepRecord SmbSweep,
    [property: JsonPropertyName("smb_configure_error")] string SmbConfigureError,
    [property: JsonPropertyName("smb_sweep_execution")] SmbSweepExecutionRecord SmbSweepExecution,
    [property: JsonPropertyName("rf_exposure")] RfExposureWindowRecord RfExposure,
    [property: JsonPropertyName("segment")] SegmentBindingRecord Segment,
    [property: JsonPropertyName("laser_profile_id")] string LaserProfileId,
    [property: JsonPropertyName("laser_mode")] string LaserMode,
    [property: JsonPropertyName("laser_power_mw")] int LaserPowerMw,
    [property: JsonPropertyName("oe_profile_id")] string OeProfileId);

public sealed record QualityRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("segment_id")] string SegmentId,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("frames_unique")] long FramesUnique,
    [property: JsonPropertyName("duplicate_count")] long DuplicateCount,
    [property: JsonPropertyName("duplicate_ratio")] double DuplicateRatio,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("last_frame_age_ms")] long LastFrameAgeMs,
    [property: JsonPropertyName("min_frames")] int MinFrames,
    [property: JsonPropertyName("estimated_frames_expected")] long? EstimatedFramesExpected,
    [property: JsonPropertyName("frame_coverage_ratio")] double? FrameCoverageRatio,
    [property: JsonPropertyName("collector_health")] string CollectorHealth,
    [property: JsonPropertyName("timeout_budget_remaining")] long TimeoutBudgetRemaining,
    [property: JsonPropertyName("quality_status")] string QualityStatus);

public sealed record RunManifestRecord(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("station_id")] string StationId,
    [property: JsonPropertyName("lockin_model")] string LockinModel,
    [property: JsonPropertyName("collector_contract")] string CollectorContract,
    [property: JsonPropertyName("runtime_version")] string RuntimeVersion,
    [property: JsonPropertyName("calibration_id")] string CalibrationId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("smb_profile_id")] string SmbProfileId,
    [property: JsonPropertyName("oe_profile_id")] string OeProfileId,
    [property: JsonPropertyName("laser_profile_id")] string LaserProfileId,
    [property: JsonPropertyName("plan_source_kind")] string PlanSourceKind,
    [property: JsonPropertyName("resolved_point_count")] int ResolvedPointCount,
    [property: JsonPropertyName("estimated_run_duration_ms")] long? EstimatedRunDurationMs);

public sealed record RunSummaryRecord(
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("lockin_model")] string LockinModel,
    [property: JsonPropertyName("collector_contract")] string CollectorContract,
    [property: JsonPropertyName("points_total")] int PointsTotal,
    [property: JsonPropertyName("points_passed")] int PointsPassed,
    [property: JsonPropertyName("points_failed")] int PointsFailed,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("samples_total")] long SamplesTotal,
    [property: JsonPropertyName("started_at")] string StartedAt,
    [property: JsonPropertyName("ended_at")] string EndedAt,
    [property: JsonPropertyName("failure")] string? Failure,
    [property: JsonPropertyName("read_attempts")] long ReadAttempts,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("raw_len_bad_count")] long RawLenBadCount,
    [property: JsonPropertyName("decode_failures")] long? DecodeFailures,
    [property: JsonPropertyName("collector_frames_path")] string? CollectorFramesPath,
    [property: JsonPropertyName("collector_blocks_path")] string? CollectorBlocksPath,
    [property: JsonPropertyName("parameter_values_path")] string ParameterValuesPath,
    [property: JsonPropertyName("sample_values_path")] string SampleValuesPath,
    [property: JsonPropertyName("packet_counter")] PacketCounterSummary? PacketCounter,
    [property: JsonPropertyName("query_hz")] double? QueryHz,
    [property: JsonPropertyName("unique_block_hz")] double? UniqueBlockHz,
    [property: JsonPropertyName("effective_sample_hz_per_parameter")] double? EffectiveSampleHzPerParameter);

public sealed record EventRecord(
    [property: JsonPropertyName("ts")] string Ts,
    [property: JsonPropertyName("monotonic_ns")] ulong MonotonicNs,
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("point_id")] string? PointId,
    [property: JsonPropertyName("device")] string? Device,
    [property: JsonPropertyName("phase")] string Phase,
    [property: JsonPropertyName("data")] object Data);

public static class RallArtifactWriter
{
    public static void WriteJsonlRecord<T>(StreamWriter writer, T record)
    {
        writer.WriteLine(JsonSerializer.Serialize(record, JsonOptions.Default));
    }

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

    public static void AppendCollectorFrameRecord(string collectorFramesPath, CollectorFrameRecord record)
    {
        AppendJsonl(collectorFramesPath, record);
    }

    public static void AppendCollectorBlockRecord(string collectorBlocksPath, CollectorBlockRecord record)
    {
        AppendJsonl(collectorBlocksPath, record);
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
            framesOk > 0 ? (long?)0 : null,
            framesOk > 0 ? framesOk - 1 : null,
            0,
            framesOk * 50);

        File.WriteAllText(segmentsPath, JsonSerializer.Serialize(segment, JsonOptions.Default) + Environment.NewLine, new UTF8Encoding(false));
    }

    public static void AppendSegmentRecord(string segmentsPath, SegmentRecord segment)
    {
        File.AppendAllText(segmentsPath, JsonSerializer.Serialize(segment, JsonOptions.Default) + Environment.NewLine, new UTF8Encoding(false));
    }

    public static void AppendJsonl<T>(string path, T record)
    {
        File.AppendAllText(path, JsonSerializer.Serialize(record, JsonOptions.Default) + Environment.NewLine, new UTF8Encoding(false));
    }

    public static void WritePrettyJson<T>(string path, T record)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(record, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
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

    public static readonly JsonSerializerOptions PrettyNamedFloatingPoint = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };
}
