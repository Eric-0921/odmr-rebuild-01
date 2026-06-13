using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivi.Visa;

const string DefaultResource = "ASRL8::INSTR";
const int DefaultBaudRate = 921600;
const int RallFrameBytes = 12288;
const int DevicePacketCounterOffset = 12287;
const int RallPostWriteDelayMs = 30;
const int VisaTimeoutMs = 300;
const string OeIdnRequiredPrefix = "SSI LIA-OE1022D";
const string OeIdnRequiredSerial = "SN:D6522078";

var exitCode = Run(args);
Environment.Exit(exitCode);

static int Run(string[] args)
{
    try
    {
        if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
        {
            PrintUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var command = args[0];
        var options = ParseOptions(args.Skip(1).ToArray());

        return command switch
        {
            "visa-list" => VisaList(),
            "oe-idn" => OeIdn(options),
            "oe-rall" => OeRall(options),
            _ => Fail($"unknown command: {command}")
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

static int VisaList()
{
    foreach (var resource in GlobalResourceManager.Find())
    {
        Console.WriteLine(resource);
    }

    return 0;
}

static int OeIdn(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", DefaultResource);
    var baudRate = GetIntOption(options, "baud", DefaultBaudRate);

    using var resource = OpenOeSession(resourceName, baudRate);
    var session = (IMessageBasedSession)resource;
    session.RawIO.Write(Encoding.ASCII.GetBytes("*IDN?\r"));
    var idn = ReadAsciiLine(session, 4096);

    Console.WriteLine(idn);

    if (!idn.Contains(OeIdnRequiredPrefix, StringComparison.Ordinal) ||
        !idn.Contains(OeIdnRequiredSerial, StringComparison.Ordinal))
    {
        return Fail($"OE1022D identity mismatch: expected `{OeIdnRequiredPrefix}` and `{OeIdnRequiredSerial}`");
    }

    return 0;
}

static int OeRall(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", DefaultResource);
    var baudRate = GetIntOption(options, "baud", DefaultBaudRate);
    var durationSec = GetIntOption(options, "duration-sec", 300);
    var outDir = GetRequiredOption(options, "out-dir");

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    var rawDir = Path.Combine(outDir, "raw");
    Directory.CreateDirectory(rawDir);

    var rawPath = Path.Combine(rawDir, "oe1022d.rall");
    var indexPath = Path.Combine(rawDir, "oe1022d.frames.idx.jsonl");
    var segmentsPath = Path.Combine(outDir, "segments.jsonl");
    var summaryPath = Path.Combine(outDir, "summary.json");

    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    var nextRawOffset = 0L;
    var frameSeq = 0L;
    var stats = new ProbeStats();
    var packetAudit = new PacketCounterAudit();
    string? firstFrameTs = null;
    string? lastFrameTs = null;
    ulong? firstFrameMonotonicNs = null;
    ulong? lastFrameMonotonicNs = null;

    using var resource = OpenOeSession(resourceName, baudRate);
    var session = (IMessageBasedSession)resource;

    using var raw = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024);
    using var index = new StreamWriter(
        new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
        new UTF8Encoding(false));
    var rallCommand = Encoding.ASCII.GetBytes("RALL?\r");
    var payload = new byte[RallFrameBytes];

    while (Stopwatch.GetTimestamp() < deadline)
    {
        stats.ReadAttempts++;

        try
        {
            session.RawIO.Write(rallCommand);
            Thread.Sleep(RallPostWriteDelayMs);

            session.RawIO.Read(payload, 0, RallFrameBytes, out var bytesRead, out _);
            if (bytesRead != RallFrameBytes)
            {
                stats.RawLenBadCount++;
                stats.ReadErrors++;
                continue;
            }

            var monotonicNs = MonotonicNsSince(processStart);
            var ts = UtcNowString();
            raw.Write(payload, 0, payload.Length);
            firstFrameTs ??= ts;
            firstFrameMonotonicNs ??= monotonicNs;
            lastFrameTs = ts;
            lastFrameMonotonicNs = monotonicNs;

            var counter = payload[DevicePacketCounterOffset];
            packetAudit.Record(counter);
            WriteFrameIndexRecord(index, frameSeq, ts, monotonicNs, nextRawOffset, payload.Length, counter);

            nextRawOffset += payload.Length;
            frameSeq++;
            stats.FramesOk++;
        }
        catch (IOTimeoutException)
        {
            stats.TimeoutCount++;
            stats.ReadErrors++;
        }
        catch (Exception ex)
        {
            stats.ReadErrors++;
            Console.Error.WriteLine($"RALL read error: {ex.Message}");
        }
    }

    raw.Flush(true);
    index.Flush();

    var finishedAt = UtcNowString();
    WriteWholeProbeSegment(
        segmentsPath,
        outDir,
        firstFrameTs ?? startedAt,
        lastFrameTs ?? finishedAt,
        firstFrameMonotonicNs ?? 0,
        lastFrameMonotonicNs ?? MonotonicNsSince(processStart),
        nextRawOffset,
        stats.FramesOk);

    var elapsedMs = (long)(Stopwatch.GetElapsedTime(processStart).TotalMilliseconds);
    var rawBytesWritten = new FileInfo(rawPath).Length;
    var summary = new ProbeSummary(
        "oe-rall",
        resourceName,
        baudRate,
        RallFrameBytes,
        RallPostWriteDelayMs,
        VisaTimeoutMs,
        durationSec,
        startedAt,
        finishedAt,
        elapsedMs,
        stats.ReadAttempts,
        stats.FramesOk,
        stats.ReadErrors,
        stats.TimeoutCount,
        stats.RawLenBadCount,
        rawBytesWritten,
        rawBytesWritten == stats.FramesOk * RallFrameBytes,
        PathRelative(outDir, rawPath),
        PathRelative(outDir, indexPath),
        PathRelative(outDir, segmentsPath),
        packetAudit.ToSummary());

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));

    Console.WriteLine($"oe-rall done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, delta_gt1={packetAudit.DeltaGt1Count}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && packetAudit.DeltaGt1Count == 0 ? 0 : 2;
}

static IVisaSession OpenOeSession(string resourceName, int baudRate)
{
    var resource = GlobalResourceManager.Open(resourceName);
    resource.TimeoutMilliseconds = VisaTimeoutMs;

    if (resource is not IMessageBasedSession session)
    {
        resource.Dispose();
        throw new InvalidOperationException($"resource is not message-based: {resourceName}");
    }

    session.TerminationCharacterEnabled = false;

    if (resource is ISerialSession serial)
    {
        serial.BaudRate = baudRate;
        serial.DataBits = 8;
        serial.Parity = SerialParity.None;
        serial.StopBits = SerialStopBitsMode.One;
        serial.FlowControl = SerialFlowControlModes.None;
        serial.ReadTermination = SerialTerminationMethod.None;
        serial.WriteTermination = SerialTerminationMethod.None;
        serial.Flush(IOBuffers.Read, true);
    }

    return resource;
}

static string ReadAsciiLine(IMessageBasedSession session, int maxBytes)
{
    var buffer = new List<byte>(128);

    while (buffer.Count < maxBytes)
    {
        var chunk = session.RawIO.Read(1);
        if (chunk.Length == 0)
        {
            if (buffer.Count == 0)
            {
                throw new IOException("empty ASCII response");
            }

            break;
        }

        var value = chunk[0];
        if (value is 10 or 13)
        {
            break;
        }

        buffer.Add(value);
    }

    if (buffer.Count >= maxBytes)
    {
        throw new IOException($"ASCII response exceeds {maxBytes} bytes");
    }

    return Encoding.ASCII.GetString(buffer.ToArray()).Trim();
}

static void WriteFrameIndexRecord(
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

static void WriteWholeProbeSegment(
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

static Dictionary<string, string> ParseOptions(string[] args)
{
    var options = new Dictionary<string, string>(StringComparer.Ordinal);

    for (var i = 0; i < args.Length; i++)
    {
        var key = args[i];
        if (!key.StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"unexpected argument: {key}");
        }

        if (i + 1 >= args.Length)
        {
            throw new ArgumentException($"missing value for {key}");
        }

        options[key[2..]] = args[++i];
    }

    return options;
}

static string GetRequiredOption(IReadOnlyDictionary<string, string> options, string key)
{
    if (!options.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
    {
        throw new ArgumentException($"missing required option --{key}");
    }

    return value;
}

static string GetOption(IReadOnlyDictionary<string, string> options, string key, string defaultValue) =>
    options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;

static int GetIntOption(IReadOnlyDictionary<string, string> options, string key, int defaultValue)
{
    if (!options.TryGetValue(key, out var value))
    {
        return defaultValue;
    }

    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
    {
        throw new ArgumentException($"--{key} must be an integer");
    }

    return parsed;
}

static int Fail(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}

static string UtcNowString() => DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

static ulong MonotonicNsSince(long startTimestamp)
{
    var ticks = Stopwatch.GetTimestamp() - startTimestamp;
    return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
}

static string PathRelative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

static void PrintUsage()
{
    Console.WriteLine("""
    Odmr.WinProbe

    Usage:
      Odmr.WinProbe visa-list
      Odmr.WinProbe oe-idn [--resource ASRL8::INSTR] [--baud 921600]
      Odmr.WinProbe oe-rall [--resource ASRL8::INSTR] [--baud 921600] --duration-sec 300 --out-dir <dir>
    """);
}

sealed class ProbeStats
{
    public long ReadAttempts { get; set; }
    public long FramesOk { get; set; }
    public long ReadErrors { get; set; }
    public long TimeoutCount { get; set; }
    public long RawLenBadCount { get; set; }
}

sealed class PacketCounterAudit
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

sealed record PacketCounterSummary(
    [property: JsonPropertyName("first_counter")] byte? FirstCounter,
    [property: JsonPropertyName("last_counter")] byte? LastCounter,
    [property: JsonPropertyName("delta0_count")] long Delta0Count,
    [property: JsonPropertyName("delta1_count")] long Delta1Count,
    [property: JsonPropertyName("delta_gt1_count")] long DeltaGt1Count,
    [property: JsonPropertyName("estimated_missing_windows")] long EstimatedMissingWindows);

sealed record ProbeSummary(
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

sealed record SegmentRecord(
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

static class JsonOptions
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
