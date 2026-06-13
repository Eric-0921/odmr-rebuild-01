using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Ivi.Visa;
using Odmr.Artifacts;
using Odmr.Devices;
using Odmr.Runtime;

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
            "smb-probe" => SmbProbe(options),
            "sweep-only-run" => SweepOnlyRunCommand(options),
            "m8812-probe" => M8812Probe(options),
            "laser-probe" => LaserProbe(options),
            _ => Fail($"unknown command: {command}")
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 1;
    }
}

static int LaserProbe(IReadOnlyDictionary<string, string> options)
{
    var port = GetOption(options, "port", CniLaserDefaults.Port);
    if (!GetBoolOption(options, "off-only", defaultValue: false))
    {
        return Fail("laser-probe requires --off-only");
    }

    var result = CniLaserSerial.ProbeOffOnly(port);
    Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

    if (!result.EchoMatched)
    {
        return Fail("CNI laser output_off echo mismatch");
    }

    return 0;
}

static int M8812Probe(IReadOnlyDictionary<string, string> options)
{
    var xPort = GetOption(options, "x", M8812Defaults.XPort);
    var yPort = GetOption(options, "y", M8812Defaults.YPort);
    var zPort = GetOption(options, "z", M8812Defaults.ZPort);
    var result = M8812Serial.Probe(xPort, yPort, zPort);

    Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

    if (!result.AllIdentityMatched)
    {
        return Fail("M8812 identity mismatch");
    }

    if (!result.CleanupOk)
    {
        return Fail("M8812 cleanup failed");
    }

    return 0;
}

static int VisaList()
{
    foreach (var resource in Oe1022dVisa.ListResources())
    {
        Console.WriteLine(resource);
    }

    return 0;
}

static int OeIdn(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", Oe1022dDefaults.Resource);
    var baudRate = GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);

    using var oe = Oe1022dVisa.Open(resourceName, baudRate);
    var idn = oe.QueryIdn();

    Console.WriteLine(idn);

    if (!idn.Contains(Oe1022dDefaults.RequiredIdnPrefix, StringComparison.Ordinal) ||
        !idn.Contains(Oe1022dDefaults.RequiredSerial, StringComparison.Ordinal))
    {
        return Fail($"OE1022D identity mismatch: expected `{Oe1022dDefaults.RequiredIdnPrefix}` and `{Oe1022dDefaults.RequiredSerial}`");
    }

    return 0;
}

static int SmbProbe(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Smb100aDefaults.Host);
    var port = GetIntOption(options, "port", Smb100aDefaults.Port);
    var result = Smb100aTcp.Probe(host, port);

    Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

    if (!result.IdentityMatched)
    {
        return Fail($"SMB100A identity mismatch: expected `{Smb100aDefaults.RequiredVendor}` and `{Smb100aDefaults.RequiredModel}`");
    }

    if (!result.ErrorQueueClean)
    {
        return Fail($"SMB100A error queue is not clean: {result.SystemError}");
    }

    return 0;
}

static int SweepOnlyRunCommand(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", Oe1022dDefaults.Resource);
    var baudRate = GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
    var host = GetOption(options, "host", Smb100aDefaults.Host);
    var port = GetIntOption(options, "port", Smb100aDefaults.Port);
    var outDir = GetRequiredOption(options, "out-dir");

    var summary = SweepOnlyRun.Execute(new SweepOnlyRunOptions(resourceName, baudRate, host, port, outDir));
    Console.WriteLine($"sweep-only-run done: frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
    return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
}

static int OeRall(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", Oe1022dDefaults.Resource);
    var baudRate = GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
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

    using var oe = Oe1022dVisa.Open(resourceName, baudRate);
    using var raw = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024);
    using var index = new StreamWriter(
        new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
        new UTF8Encoding(false));
    var payload = new byte[Oe1022dDefaults.RallFrameBytes];

    // Frozen LabVIEW-like RALL hot path: do not add parser, retry, poll sleep,
    // per-frame console output, GUI publish, or async/multi-reader behavior here.
    while (Stopwatch.GetTimestamp() < deadline)
    {
        stats.ReadAttempts++;

        try
        {
            oe.WriteRallQuery();
            Thread.Sleep(Oe1022dDefaults.RallPostWriteDelayMs);

            var bytesRead = oe.ReadRallFrame(payload);
            if (bytesRead != Oe1022dDefaults.RallFrameBytes)
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

            var counter = payload[Oe1022dDefaults.DevicePacketCounterOffset];
            packetAudit.Record(counter);
            RallArtifactWriter.WriteFrameIndexRecord(index, frameSeq, ts, monotonicNs, nextRawOffset, payload.Length, counter);

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
    RallArtifactWriter.WriteWholeProbeSegment(
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
        Oe1022dDefaults.RallFrameBytes,
        Oe1022dDefaults.RallPostWriteDelayMs,
        Oe1022dDefaults.VisaTimeoutMs,
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
        rawBytesWritten == stats.FramesOk * Oe1022dDefaults.RallFrameBytes,
        PathRelative(outDir, rawPath),
        PathRelative(outDir, indexPath),
        PathRelative(outDir, segmentsPath),
        packetAudit.ToSummary());

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));

    Console.WriteLine($"oe-rall done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, delta_gt1={packetAudit.DeltaGt1Count}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && packetAudit.DeltaGt1Count == 0 ? 0 : 2;
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

        if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            options[key[2..]] = "true";
            continue;
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

static bool GetBoolOption(IReadOnlyDictionary<string, string> options, string key, bool defaultValue)
{
    if (!options.TryGetValue(key, out var value))
    {
        return defaultValue;
    }

    if (!bool.TryParse(value, out var parsed))
    {
        throw new ArgumentException($"--{key} must be true or false");
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
      Odmr.WinProbe smb-probe [--host 169.254.2.20] [--port 5025]
      Odmr.WinProbe sweep-only-run [--resource ASRL8::INSTR] [--baud 921600] [--host 169.254.2.20] [--port 5025] --out-dir <dir>
      Odmr.WinProbe m8812-probe [--x COM4] [--y COM6] [--z COM3]
      Odmr.WinProbe laser-probe [--port COM9] --off-only
    """);
}
