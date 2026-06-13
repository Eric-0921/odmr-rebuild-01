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
    string SmbHost,
    int SmbPort,
    string OutDir);

public sealed record CollectorCursor(
    long NextFrameSeq,
    long NextRawOffset,
    string Timestamp,
    ulong MonotonicNs);

public sealed record SweepOnlyRunSummary(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("oe_resource")] string OeResource,
    [property: JsonPropertyName("oe_baud_rate")] int OeBaudRate,
    [property: JsonPropertyName("smb_host")] string SmbHost,
    [property: JsonPropertyName("smb_port")] int SmbPort,
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
    [property: JsonPropertyName("raw_bytes_written")] long RawBytesWritten,
    [property: JsonPropertyName("raw_size_matches_frames_ok")] bool RawSizeMatchesFramesOk,
    [property: JsonPropertyName("raw_path")] string RawPath,
    [property: JsonPropertyName("index_path")] string IndexPath,
    [property: JsonPropertyName("segments_path")] string SegmentsPath,
    [property: JsonPropertyName("sweep")] SmbSweepSpec Sweep,
    [property: JsonPropertyName("sweep_observation")] SmbSweepObservation SweepObservation,
    [property: JsonPropertyName("packet_counter")] PacketCounterSummary PacketCounter);

public static class SweepOnlyRun
{
    private const int SmbCommandSettleMs = 500;

    public static SweepOnlyRunSummary Execute(SweepOnlyRunOptions options)
    {
        var rawDir = Path.Combine(options.OutDir, "raw");
        Directory.CreateDirectory(rawDir);

        var rawPath = Path.Combine(rawDir, "oe1022d.rall");
        var indexPath = Path.Combine(rawDir, "oe1022d.frames.idx.jsonl");
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
            rawPath,
            indexPath,
            processStart);
        collector.Start();
        collector.WaitForFirstFrame(TimeSpan.FromSeconds(5));

        SmbSweepObservation? sweepObservation = null;
        CollectorCursor? segmentStart = null;
        CollectorCursor? segmentEnd = null;
        var segmentStartTs = startedAt;
        var segmentEndTs = startedAt;
        ulong segmentStartMonotonicNs = 0;
        ulong segmentEndMonotonicNs = 0;

        using (var smb = Smb100aTcp.Open(options.SmbHost, options.SmbPort))
        {
            smb.ApplyDefaultFixedProfile(SmbCommandSettleMs);
            smb.ConfigureDefaultSweep(SmbCommandSettleMs);

            segmentStartTs = UtcNowString();
            segmentStartMonotonicNs = MonotonicNsSince(processStart);
            segmentStart = collector.Cursor();

            sweepObservation = smb.ExecuteDefaultSweep();

            segmentEndTs = UtcNowString();
            segmentEndMonotonicNs = MonotonicNsSince(processStart);
            segmentEnd = collector.Cursor();
            smb.Cleanup();
        }

        collector.Stop();
        var snapshot = collector.Snapshot();
        var finishedAt = UtcNowString();
        var elapsedMs = (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds;
        var rawBytesWritten = new FileInfo(rawPath).Length;

        var runId = Path.GetFileName(Path.GetFullPath(options.OutDir));
        var framesInSegment = segmentEnd.NextFrameSeq - segmentStart.NextFrameSeq;
        RallArtifactWriter.AppendSegmentRecord(
            segmentsPath,
            new SegmentRecord(
                1,
                runId,
                "seg_smb_sweep_only_0000",
                "smb_sweep_only",
                "oe1022d_main",
                segmentStartTs,
                segmentEndTs,
                segmentStartMonotonicNs,
                segmentEndMonotonicNs,
                "raw/oe1022d.rall",
                segmentStart.NextRawOffset,
                segmentEnd.NextRawOffset,
                framesInSegment > 0 ? segmentStart.NextFrameSeq : null,
                framesInSegment > 0 ? segmentEnd.NextFrameSeq - 1 : null));

        var summary = new SweepOnlyRunSummary(
            "sweep-only-run",
            options.OeResource,
            options.OeBaudRate,
            options.SmbHost,
            options.SmbPort,
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
            rawBytesWritten,
            rawBytesWritten == snapshot.Stats.FramesOk * Oe1022dDefaults.RallFrameBytes,
            PathRelative(options.OutDir, rawPath),
            PathRelative(options.OutDir, indexPath),
            PathRelative(options.OutDir, segmentsPath),
            SmbSweepSpec.Default,
            sweepObservation!,
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

public sealed record OeRallCollectorSnapshot(ProbeStats Stats, PacketCounterAudit PacketCounter);

public sealed class OeRallCollector : IDisposable
{
    private readonly string resourceName;
    private readonly int baudRate;
    private readonly string rawPath;
    private readonly string indexPath;
    private readonly long processStart;
    private readonly object sync = new();
    private readonly ManualResetEventSlim firstFrameReady = new(false);
    private readonly ProbeStats stats = new();
    private readonly PacketCounterAudit packetAudit = new();
    private volatile bool stopRequested;
    private Thread? thread;
    private Exception? failure;
    private long nextRawOffset;
    private long frameSeq;
    private string lastTs;
    private ulong lastMonotonicNs;

    public OeRallCollector(string resourceName, int baudRate, string rawPath, string indexPath, long processStart)
    {
        this.resourceName = resourceName;
        this.baudRate = baudRate;
        this.rawPath = rawPath;
        this.indexPath = indexPath;
        this.processStart = processStart;
        lastTs = SweepOnlyRunCollectorTime.ExecuteTimestampForCollector();
    }

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
            return new CollectorCursor(frameSeq, nextRawOffset, lastTs, lastMonotonicNs);
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
                packetAudit);
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
            using var raw = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024);
            using var index = new StreamWriter(
                new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
                new UTF8Encoding(false));
            var payload = new byte[Oe1022dDefaults.RallFrameBytes];

            // Frozen LabVIEW-like RALL hot path: write, 30ms delay, exact blocking read,
            // append raw, append frame index. No parser, retry, GUI, or async fan-out here.
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
                        lock (sync)
                        {
                            stats.RawLenBadCount++;
                            stats.ReadErrors++;
                        }

                        continue;
                    }

                    var monotonicNs = MonotonicNsSince(processStart);
                    var ts = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture);
                    raw.Write(payload, 0, payload.Length);
                    var counter = payload[Oe1022dDefaults.DevicePacketCounterOffset];

                    lock (sync)
                    {
                        packetAudit.Record(counter);
                        RallArtifactWriter.WriteFrameIndexRecord(index, frameSeq, ts, monotonicNs, nextRawOffset, payload.Length, counter);
                        nextRawOffset += payload.Length;
                        frameSeq++;
                        stats.FramesOk++;
                        lastTs = ts;
                        lastMonotonicNs = monotonicNs;
                    }

                    firstFrameReady.Set();
                }
                catch (IOTimeoutException)
                {
                    lock (sync)
                    {
                        stats.TimeoutCount++;
                        stats.ReadErrors++;
                    }
                }
            }

            raw.Flush(true);
            index.Flush();
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
