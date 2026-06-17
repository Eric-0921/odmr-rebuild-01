using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Odmr.Artifacts;
using Odmr.Devices;

namespace Odmr.Runtime;

public sealed record LockinCollectorSnapshot(
    string LockinModel,
    ProbeStats Stats,
    long SamplesWritten,
    ulong LastMonotonicNs,
    PacketCounterSummary? PacketCounter,
    long? DecodeFailures,
    long? UniqueBlocks,
    long? DuplicateBlocks,
    double? QueryHz,
    double? UniqueBlockHz,
    double? EffectiveSampleHzPerParameter);

internal interface ILockinCollector : IDisposable
{
    string LockinModel { get; }
    string DeviceId { get; }
    string CollectorContract { get; }
    string CollectorArtifactFileName { get; }

    void Start();
    void WaitForFirstFrame(TimeSpan timeout);
    CollectorCursor Cursor();
    LockinCollectorSnapshot Snapshot();
    void Stop();
}

public sealed class Oe1300TcpCollector : ILockinCollector
{
    private readonly string host;
    private readonly int port;
    private readonly string collectorBlocksPath;
    private readonly string parameterValuesPath;
    private readonly string sampleValuesPath;
    private readonly long processStart;
    private readonly Oe1300CollectorConfig collectorConfig;
    private readonly object sync = new();
    private readonly ManualResetEventSlim firstFrameReady = new(false);
    private readonly ProbeStats stats = new();
    private volatile bool stopRequested;
    private Thread? thread;
    private Exception? failure;
    private long rallIndex;
    private long nextSampleIndex;
    private long nextUniqueBlockIndex;
    private long duplicateBlocks;
    private long decodeFailures;
    private string lastTs;
    private ulong lastMonotonicNs;
    private string? previousPayloadSha256;
    private ulong? firstSuccessfulMonotonicNs;

    public Oe1300TcpCollector(
        string host,
        int port,
        string collectorBlocksPath,
        string parameterValuesPath,
        string sampleValuesPath,
        long processStart,
        Oe1300CollectorConfig collectorConfig)
    {
        this.host = host;
        this.port = port;
        this.collectorBlocksPath = collectorBlocksPath;
        this.parameterValuesPath = parameterValuesPath;
        this.sampleValuesPath = sampleValuesPath;
        this.processStart = processStart;
        this.collectorConfig = collectorConfig;
        lastTs = SweepOnlyRunCollectorTime.ExecuteTimestampForCollector();
    }

    public string LockinModel => LockinModelNames.Oe1300;

    public string DeviceId => "oe1300_main";

    public string CollectorContract => RuntimeContracts.Oe1300FrozenRallHotPath;

    public string CollectorArtifactFileName => "collector_blocks.jsonl";

    public void Start()
    {
        thread = new Thread(Run) { IsBackground = true, Name = "oe1300-tcp-collector" };
        thread.Start();
    }

    public void WaitForFirstFrame(TimeSpan timeout)
    {
        if (!firstFrameReady.Wait(timeout))
        {
            ThrowIfFailed();
            throw new TimeoutException("OE1300 TCP collector did not produce a block before SMB sweep");
        }

        ThrowIfFailed();
    }

    public CollectorCursor Cursor()
    {
        lock (sync)
        {
            return new CollectorCursor(rallIndex, nextSampleIndex, lastTs, lastMonotonicNs, nextUniqueBlockIndex);
        }
    }

    public LockinCollectorSnapshot Snapshot()
    {
        lock (sync)
        {
            var elapsedSeconds = ElapsedSeconds();
            var queryHz = elapsedSeconds > 0 ? stats.FramesOk / elapsedSeconds : (double?)null;
            var uniqueBlockHz = elapsedSeconds > 0 ? nextUniqueBlockIndex / elapsedSeconds : (double?)null;
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
                null,
                decodeFailures,
                nextUniqueBlockIndex,
                duplicateBlocks,
                queryHz,
                uniqueBlockHz,
                uniqueBlockHz.HasValue ? uniqueBlockHz.Value * Oe1300Defaults.TcpRallLabviewSamplesPerParameter : null);
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
            using var oe = Oe1300Tcp.Open(host, port);
            using var collectorBlocksWriter = new StreamWriter(
                new FileStream(collectorBlocksPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
                new UTF8Encoding(false));
            using var parameterWriter = new StreamWriter(
                new FileStream(parameterValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
                new UTF8Encoding(false));
            using var sampleWriter = new StreamWriter(
                new FileStream(sampleValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024),
                new UTF8Encoding(false));
            var payload = new byte[collectorConfig.TcpExpectedBytes];

            parameterWriter.Write("rall_index,monotonic_ns,status_hex,status_byte,trig_count");
            foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
            {
                parameterWriter.Write(',');
                parameterWriter.Write(fieldName);
            }
            parameterWriter.WriteLine();

            sampleWriter.Write("rall_index,sample_in_rall,global_sample_index,monotonic_ns,status_hex,status_byte,trig_count");
            foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
            {
                sampleWriter.Write(',');
                sampleWriter.Write(fieldName);
            }
            sampleWriter.WriteLine();

            while (!stopRequested)
            {
                lock (sync)
                {
                    stats.ReadAttempts++;
                }

                try
                {
                    var bytesRead = oe.ReadRallFrame(
                        payload,
                        collectorConfig.TcpExpectedBytes,
                        collectorConfig.RallPostWriteDelayMs,
                        collectorConfig.DrainBeforeWrite);
                    if (bytesRead != collectorConfig.TcpExpectedBytes)
                    {
                        lock (sync)
                        {
                            stats.RawLenBadCount++;
                            stats.ReadErrors++;
                        }

                        continue;
                    }

                    var namedSeries = Oe1300Parsers.DecodeTcpRallLabviewNamedSeries(payload);
                    var monotonicNs = MonotonicNsSince(processStart);
                    var ts = SweepOnlyRunCollectorTime.ExecuteTimestampForCollector();
                    var statusHex = Convert.ToHexString(
                        payload,
                        Oe1300Defaults.TcpRallStatusOffset,
                        Oe1300Defaults.TcpRallStatusByteCount).ToLowerInvariant();
                    var statusByte = payload[Oe1300Defaults.TcpRallStatusOffset];
                    var trigCount = payload[Oe1300Defaults.TcpRallTrigCountOffset];
                    var statusZoneHex = Convert.ToHexString(
                        payload,
                        Oe1300Defaults.TcpRallPayloadBytes,
                        payload.Length - Oe1300Defaults.TcpRallPayloadBytes).ToLowerInvariant();
                    var statusZoneSha256 = Convert.ToHexString(
                        SHA256.HashData(payload.AsSpan(Oe1300Defaults.TcpRallPayloadBytes, payload.Length - Oe1300Defaults.TcpRallPayloadBytes))).ToLowerInvariant();
                    var payloadSha256 = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
                    var uniqueBlock = !string.Equals(previousPayloadSha256, payloadSha256, StringComparison.Ordinal);
                    var sampleIndexStart = nextSampleIndex;

                    for (var sampleIndexInRall = 0; sampleIndexInRall < collectorConfig.SamplesPerParameter; sampleIndexInRall++)
                    {
                        sampleWriter.Write(rallIndex.ToString(CultureInfo.InvariantCulture));
                        sampleWriter.Write(',');
                        sampleWriter.Write(sampleIndexInRall.ToString(CultureInfo.InvariantCulture));
                        sampleWriter.Write(',');
                        sampleWriter.Write(nextSampleIndex.ToString(CultureInfo.InvariantCulture));
                        sampleWriter.Write(',');
                        sampleWriter.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
                        sampleWriter.Write(',');
                        sampleWriter.Write(statusHex);
                        sampleWriter.Write(',');
                        sampleWriter.Write(statusByte.ToString(CultureInfo.InvariantCulture));
                        sampleWriter.Write(',');
                        sampleWriter.Write(trigCount.ToString(CultureInfo.InvariantCulture));
                        foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
                        {
                            sampleWriter.Write(',');
                            sampleWriter.Write(namedSeries[fieldName][sampleIndexInRall].ToString("R", CultureInfo.InvariantCulture));
                        }
                        sampleWriter.WriteLine();
                        nextSampleIndex++;
                    }

                    parameterWriter.Write(rallIndex.ToString(CultureInfo.InvariantCulture));
                    parameterWriter.Write(',');
                    parameterWriter.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
                    parameterWriter.Write(',');
                    parameterWriter.Write(statusHex);
                    parameterWriter.Write(',');
                    parameterWriter.Write(statusByte.ToString(CultureInfo.InvariantCulture));
                    parameterWriter.Write(',');
                    parameterWriter.Write(trigCount.ToString(CultureInfo.InvariantCulture));
                    foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
                    {
                        parameterWriter.Write(',');
                        parameterWriter.Write(namedSeries[fieldName].Average().ToString("R", CultureInfo.InvariantCulture));
                    }
                    parameterWriter.WriteLine();

                    if (uniqueBlock)
                    {
                        nextUniqueBlockIndex++;
                    }
                    else
                    {
                        duplicateBlocks++;
                    }

                    RallArtifactWriter.WriteJsonlRecord(
                        collectorBlocksWriter,
                        new CollectorBlockRecord(
                            1,
                            DeviceId,
                            rallIndex,
                            ts,
                            monotonicNs,
                            sampleIndexStart,
                            nextSampleIndex,
                            collectorConfig.SamplesPerParameter,
                            collectorConfig.ParameterCount,
                            statusHex,
                            statusByte,
                            trigCount,
                            payloadSha256,
                            statusZoneSha256,
                            statusZoneHex,
                            uniqueBlock,
                            uniqueBlock ? nextUniqueBlockIndex - 1 : Math.Max(0, nextUniqueBlockIndex - 1)));

                    lock (sync)
                    {
                        previousPayloadSha256 = payloadSha256;
                        firstSuccessfulMonotonicNs ??= monotonicNs;
                        rallIndex++;
                        stats.FramesOk++;
                        lastTs = ts;
                        lastMonotonicNs = monotonicNs;
                    }

                    firstFrameReady.Set();
                }
                catch (IOException)
                {
                    lock (sync)
                    {
                        stats.TimeoutCount++;
                        stats.ReadErrors++;
                    }
                }
                catch (Exception)
                {
                    lock (sync)
                    {
                        decodeFailures++;
                        stats.ReadErrors++;
                    }
                }
            }

            collectorBlocksWriter.Flush();
            parameterWriter.Flush();
            sampleWriter.Flush();
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
            throw new InvalidOperationException("OE1300 TCP collector failed", failure);
        }
    }

    private double ElapsedSeconds()
    {
        if (!firstSuccessfulMonotonicNs.HasValue || lastMonotonicNs <= firstSuccessfulMonotonicNs.Value)
        {
            return 0.0;
        }

        return (lastMonotonicNs - firstSuccessfulMonotonicNs.Value) / 1_000_000_000.0;
    }

    private static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }
}
