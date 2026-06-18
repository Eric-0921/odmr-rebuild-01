using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivi.Visa;
using Odmr.Artifacts;
using Odmr.Devices;

namespace Odmr.Runtime;

public sealed record SweepOnlyRunOptions(
    string OeResource,
    int OeBaudRate,
    string SmbResource,
    int RepeatCount,
    string OutDir);

public sealed record CollectorCursor(
    long NextFrameSeq,
    long NextSampleIndex,
    string Timestamp,
    ulong MonotonicNs,
    long NextUniqueFrameSeq = 0);

public sealed record SweepOnlyRunSummary(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("oe_resource")] string OeResource,
    [property: JsonPropertyName("oe_baud_rate")] int OeBaudRate,
    [property: JsonPropertyName("smb_resource")] string SmbResource,
    [property: JsonPropertyName("frame_bytes")] int FrameBytes,
    [property: JsonPropertyName("post_write_delay_ms")] int PostWriteDelayMs,
    [property: JsonPropertyName("visa_timeout_ms")] int VisaTimeoutMs,
    [property: JsonPropertyName("started_at")] string StartedAt,
    [property: JsonPropertyName("finished_at")] string FinishedAt,
    [property: JsonPropertyName("elapsed_ms")] long ElapsedMs,
    [property: JsonPropertyName("read_attempts")] long ReadAttempts,
    [property: JsonPropertyName("frames_ok")] long FramesOk,
    [property: JsonPropertyName("read_errors")] long ReadErrors,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("raw_len_bad_count")] long RawLenBadCount,
    [property: JsonPropertyName("samples_total")] long SamplesTotal,
    [property: JsonPropertyName("collector_frames_path")] string CollectorFramesPath,
    [property: JsonPropertyName("parameter_values_path")] string ParameterValuesPath,
    [property: JsonPropertyName("sample_values_path")] string SampleValuesPath,
    [property: JsonPropertyName("segments_path")] string SegmentsPath,
    [property: JsonPropertyName("sweep")] SmbSweepSpec Sweep,
    [property: JsonPropertyName("repeat_count")] int RepeatCount,
    [property: JsonPropertyName("sweep_observations")] IReadOnlyList<SmbSweepObservation> SweepObservations,
    [property: JsonPropertyName("packet_counter")] PacketCounterSummary PacketCounter);

public static class SweepOnlyRun
{
    private const int SmbCommandSettleMs = 500;

    public static SweepOnlyRunSummary Execute(SweepOnlyRunOptions options)
    {
        Directory.CreateDirectory(options.OutDir);
        var collectorFramesPath = Path.Combine(options.OutDir, "collector_frames.jsonl");
        var parameterValuesPath = Path.Combine(options.OutDir, "parameter_values.csv");
        var sampleValuesPath = Path.Combine(options.OutDir, "sample_values.csv");
        var segmentsPath = Path.Combine(options.OutDir, "segments.jsonl");
        var summaryPath = Path.Combine(options.OutDir, "summary.json");
        if (File.Exists(segmentsPath))
        {
            File.Delete(segmentsPath);
        }

        var startedAt = UtcNowString();
        var processStart = Stopwatch.GetTimestamp();
        using var collector = new OeRallCollector(
            options.OeResource,
            options.OeBaudRate,
            collectorFramesPath,
            parameterValuesPath,
            sampleValuesPath,
            processStart);
        collector.Start();
        collector.WaitForFirstFrame(TimeSpan.FromSeconds(5));

        if (options.RepeatCount <= 0)
        {
            throw new ArgumentException("--repeat must be positive");
        }

        var sweepObservations = new List<SmbSweepObservation>(options.RepeatCount);

        using (var smb = Smb100aVisa.Open(options.SmbResource))
        {
            try
            {
                smb.ApplyDefaultFixedProfile(SmbCommandSettleMs);
                smb.ConfigureDefaultSweep(SmbCommandSettleMs);

                for (var sweepIndex = 0; sweepIndex < options.RepeatCount; sweepIndex++)
                {
                    var segmentStartTs = UtcNowString();
                    var segmentStartMonotonicNs = MonotonicNsSince(processStart);
                    var segmentStart = collector.Cursor();

                    var sweepObservation = smb.ExecuteDefaultSweep();

                    var segmentEndTs = UtcNowString();
                    var segmentEndMonotonicNs = MonotonicNsSince(processStart);
                    var segmentEnd = collector.Cursor();
                    sweepObservations.Add(sweepObservation);

                    var runId = Path.GetFileName(Path.GetFullPath(options.OutDir));
                    var framesInSegment = segmentEnd.NextFrameSeq - segmentStart.NextFrameSeq;
                    RallArtifactWriter.AppendSegmentRecord(
                        segmentsPath,
                        new SegmentRecord(
                            1,
                            runId,
                        $"seg_smb_sweep_only_{sweepIndex:0000}",
                        "smb_sweep_only",
                        "oe1022d_main",
                        segmentStartTs,
                        segmentEndTs,
                        segmentStartMonotonicNs,
                        segmentEndMonotonicNs,
                        "sample_values.csv",
                        framesInSegment > 0 ? segmentStart.NextFrameSeq : null,
                        framesInSegment > 0 ? segmentEnd.NextFrameSeq - 1 : null,
                        segmentStart.NextSampleIndex,
                        segmentEnd.NextSampleIndex));
                }
            }
            finally
            {
                smb.Cleanup();
            }
        }

        collector.Stop();
        var snapshot = collector.Snapshot();
        var finishedAt = UtcNowString();
        var elapsedMs = (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds;
        var samplesTotal = snapshot.SamplesWritten;

        var summary = new SweepOnlyRunSummary(
            "sweep-only-run",
            options.OeResource,
            options.OeBaudRate,
            options.SmbResource,
            Oe1022dDefaults.RallFrameBytes,
            Oe1022dDefaults.RallPostWriteDelayMs,
            Oe1022dDefaults.VisaTimeoutMs,
            startedAt,
            finishedAt,
            elapsedMs,
            snapshot.Stats.ReadAttempts,
            snapshot.Stats.FramesOk,
            snapshot.Stats.ReadErrors,
            snapshot.Stats.TimeoutCount,
            snapshot.Stats.RawLenBadCount,
            samplesTotal,
            PathRelative(options.OutDir, collectorFramesPath),
            PathRelative(options.OutDir, parameterValuesPath),
            PathRelative(options.OutDir, sampleValuesPath),
            PathRelative(options.OutDir, segmentsPath),
            SmbSweepSpec.Default,
            options.RepeatCount,
            sweepObservations,
            snapshot.PacketCounter.ToSummary());

        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
        return summary;
    }

    private static string UtcNowString() => DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture);

    private static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }

    private static string PathRelative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}

public sealed record OeRallCollectorSnapshot(ProbeStats Stats, PacketCounterAudit PacketCounter, long SamplesWritten);

public sealed class OeRallCollector : ILockinCollector
{
    private readonly string resourceName;
    private readonly int baudRate;
    private readonly string collectorFramesPath;
    private readonly string parameterValuesPath;
    private readonly string sampleValuesPath;
    private readonly long processStart;
    private readonly object sync = new();
    private readonly ManualResetEventSlim firstFrameReady = new(false);
    private readonly ProbeStats stats = new();
    private readonly PacketCounterAudit packetAudit = new();
    private volatile bool stopRequested;
    private Thread? thread;
    private Exception? failure;
    private long frameSeq;
    private long nextUniqueFrameSeq;
    private long nextSampleIndex;
    private string lastTs;
    private ulong lastMonotonicNs;

    public OeRallCollector(
        string resourceName,
        int baudRate,
        string collectorFramesPath,
        string parameterValuesPath,
        string sampleValuesPath,
        long processStart)
    {
        this.resourceName = resourceName;
        this.baudRate = baudRate;
        this.collectorFramesPath = collectorFramesPath;
        this.parameterValuesPath = parameterValuesPath;
        this.sampleValuesPath = sampleValuesPath;
        this.processStart = processStart;
        lastTs = SweepOnlyRunCollectorTime.ExecuteTimestampForCollector();
    }

    public string LockinModel => LockinModelNames.Oe1022d;

    public string DeviceId => "oe1022d_main";

    public string CollectorContract => RuntimeContracts.Oe1022dFrozenRallHotPath;

    public string CollectorArtifactFileName => "collector_frames.jsonl";

    public void Start()
    {
        thread = new Thread(Run) { IsBackground = true, Name = "oe-rall-collector" };
        thread.Start();
    }

    public void WaitForFirstFrame(TimeSpan timeout)
    {
        if (!firstFrameReady.Wait(timeout))
        {
            ThrowIfFailed();
            throw new TimeoutException("OE RALL collector did not produce a frame before SMB sweep");
        }

        ThrowIfFailed();
    }

    public CollectorCursor Cursor()
    {
        lock (sync)
        {
            return new CollectorCursor(frameSeq, nextSampleIndex, lastTs, lastMonotonicNs, nextUniqueFrameSeq);
        }
    }

    public OeRallCollectorSnapshot Snapshot()
    {
        lock (sync)
        {
            return new OeRallCollectorSnapshot(
                new ProbeStats
                {
                    ReadAttempts = stats.ReadAttempts,
                    FramesOk = stats.FramesOk,
                    ReadErrors = stats.ReadErrors,
                    TimeoutCount = stats.TimeoutCount,
                    RawLenBadCount = stats.RawLenBadCount
                },
                packetAudit,
                nextSampleIndex);
        }
    }

    LockinCollectorSnapshot ILockinCollector.Snapshot()
    {
        lock (sync)
        {
            return new LockinCollectorSnapshot(
                LockinModel,
                new ProbeStats
                {
                    ReadAttempts = stats.ReadAttempts,
                    FramesOk = stats.FramesOk,
                    ReadErrors = stats.ReadErrors,
                    TimeoutCount = stats.TimeoutCount,
                    RawLenBadCount = stats.RawLenBadCount
                },
                nextSampleIndex,
                lastMonotonicNs,
                packetAudit.ToSummary(),
                null,
                nextUniqueFrameSeq,
                Math.Max(0, stats.FramesOk - nextUniqueFrameSeq),
                null,
                null,
                null);
        }
    }

    public void Stop()
    {
        stopRequested = true;
        thread?.Join();
        ThrowIfFailed();
    }

    public void Dispose()
    {
        stopRequested = true;
        thread?.Join();
        firstFrameReady.Dispose();
    }

    private void Run()
    {
        try
        {
            using var oe = Oe1022dVisa.Open(resourceName, baudRate);
            using var collectorFrames = new StreamWriter(
                new FileStream(collectorFramesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
                new UTF8Encoding(false));
            using var parameterValues = new StreamWriter(
                new FileStream(parameterValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
                new UTF8Encoding(false));
            using var sampleValues = new StreamWriter(
                new FileStream(sampleValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024),
                new UTF8Encoding(false));
            var payload = new byte[Oe1022dDefaults.RallFrameBytes];
            var fieldSums = new double[Oe1022dDirectDecode.MeasurementFields.Length];
            var fieldCounts = new long[Oe1022dDirectDecode.MeasurementFields.Length];

            parameterValues.Write("frame_seq,monotonic_ns,device_packet_counter,b_ref_source_code,b_ref_slope_code,b_ref_current_freq_hz,b_input_overload,b_gain_overload,b_pll_locked");
            foreach (var field in Oe1022dDirectDecode.MeasurementFields)
            {
                parameterValues.Write(',');
                parameterValues.Write(field.Key);
            }
            parameterValues.WriteLine();

            sampleValues.Write("frame_seq,sample_in_frame,global_sample_index,monotonic_ns,device_packet_counter,b_ref_source_code,b_ref_slope_code,b_ref_current_freq_hz,b_input_overload,b_gain_overload,b_pll_locked");
            foreach (var field in Oe1022dDirectDecode.MeasurementFields)
            {
                sampleValues.Write(',');
                sampleValues.Write(field.Key);
            }
            sampleValues.WriteLine();

            // Frozen direct-decode hot path: write, 30ms delay, exact blocking read,
            // same-thread判重 + decode，collector_frames保留全query轨迹，
            // parameter/sample CSV只写unique frame。这里不引入retry、异步fan-out或第二线程。
            while (!stopRequested)
            {
                lock (sync)
                {
                    stats.ReadAttempts++;
                }

                try
                {
                    oe.WriteRallQuery();
                    Thread.Sleep(Oe1022dDefaults.RallPostWriteDelayMs);

                    var bytesRead = oe.ReadRallFrame(payload);
                    if (bytesRead != Oe1022dDefaults.RallFrameBytes)
                    {
                        if (stopRequested)
                        {
                            break;
                        }

                        lock (sync)
                        {
                            stats.RawLenBadCount++;
                            stats.ReadErrors++;
                        }

                        continue;
                    }

                    var monotonicNs = MonotonicNsSince(processStart);
                    var ts = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture);
                    var counter = payload[Oe1022dDefaults.DevicePacketCounterOffset];
                    var status = Oe1022dDirectDecode.ReadStatus(payload);
                    var uniqueFrame = packetAudit.WouldBeUnique(counter);
                    var frameSampleStart = nextSampleIndex;
                    var frameSampleEnd = uniqueFrame
                        ? frameSampleStart + Oe1022dDirectDecode.SamplesPerFrame
                        : frameSampleStart;
                    var uniqueFrameIndex = uniqueFrame
                        ? nextUniqueFrameSeq
                        : Math.Max(0, nextUniqueFrameSeq - 1);

                    if (uniqueFrame)
                    {
                        Array.Clear(fieldSums, 0, fieldSums.Length);
                        Array.Clear(fieldCounts, 0, fieldCounts.Length);

                        for (var sampleIndex = 0; sampleIndex < Oe1022dDirectDecode.SamplesPerFrame; sampleIndex++)
                        {
                            sampleValues.Write(frameSeq.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(sampleIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write((frameSampleStart + sampleIndex).ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(monotonicNs.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(counter.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BRefSourceCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BRefSlopeCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BRefCurrentFreqHz.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BInputOverload.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BGainOverload.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            sampleValues.Write(',');
                            sampleValues.Write(status.BPllLocked.ToString(System.Globalization.CultureInfo.InvariantCulture));

                            for (var fieldIndex = 0; fieldIndex < Oe1022dDirectDecode.MeasurementFields.Length; fieldIndex++)
                            {
                                var value = Oe1022dDirectDecode.ReadMeasurementValue(payload, fieldIndex, sampleIndex);
                                if (double.IsFinite(value))
                                {
                                    fieldSums[fieldIndex] += value;
                                    fieldCounts[fieldIndex]++;
                                }

                                sampleValues.Write(',');
                                sampleValues.Write(value.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                            }

                            sampleValues.WriteLine();
                        }

                        parameterValues.Write(frameSeq.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(monotonicNs.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(counter.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BRefSourceCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BRefSlopeCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BRefCurrentFreqHz.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BInputOverload.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BGainOverload.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        parameterValues.Write(',');
                        parameterValues.Write(status.BPllLocked.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        for (var fieldIndex = 0; fieldIndex < Oe1022dDirectDecode.MeasurementFields.Length; fieldIndex++)
                        {
                            var mean = fieldCounts[fieldIndex] > 0
                                ? fieldSums[fieldIndex] / fieldCounts[fieldIndex]
                                : double.NaN;
                            parameterValues.Write(',');
                            parameterValues.Write(mean.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                        }
                        parameterValues.WriteLine();
                    }

                    lock (sync)
                    {
                        packetAudit.Record(counter);
                        RallArtifactWriter.WriteJsonlRecord(
                            collectorFrames,
                            new CollectorFrameRecord(
                                1,
                                "oe1022d_main",
                                frameSeq,
                                ts,
                                monotonicNs,
                                frameSampleStart,
                                frameSampleEnd,
                                Oe1022dDirectDecode.SamplesPerFrame,
                                counter,
                                status.BRefSourceCode,
                                status.BRefSlopeCode,
                                status.BRefCurrentFreqHz,
                                status.BInputOverload,
                                status.BGainOverload,
                                status.BPllLocked,
                                uniqueFrame,
                                uniqueFrameIndex));
                        if (uniqueFrame)
                        {
                            nextSampleIndex = frameSampleEnd;
                            nextUniqueFrameSeq++;
                        }
                        frameSeq++;
                        stats.FramesOk++;
                        lastTs = ts;
                        lastMonotonicNs = monotonicNs;
                    }

                    firstFrameReady.Set();
                }
                catch (IOTimeoutException)
                {
                    if (stopRequested)
                    {
                        break;
                    }

                    lock (sync)
                    {
                        stats.TimeoutCount++;
                        stats.ReadErrors++;
                    }
                }
            }

            collectorFrames.Flush();
            parameterValues.Flush();
            sampleValues.Flush();
        }
        catch (Exception ex)
        {
            failure = ex;
            firstFrameReady.Set();
        }
    }

    private void ThrowIfFailed()
    {
        if (failure is not null)
        {
            throw new InvalidOperationException("OE RALL collector failed", failure);
        }
    }

    private static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }
}

internal static class SweepOnlyRunCollectorTime
{
    public static string ExecuteTimestampForCollector() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture);
}
