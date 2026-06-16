using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
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
            "oe1300-idn" => Oe1300Idn(options),
            "oe1300-rall" => Oe1300Rall(options),
            "oe1300-net-idn" => Oe1300NetIdn(options),
            "oe1300-net-rall" => Oe1300NetRall(options),
            "oe1300-net-collector-demo" => Oe1300NetCollectorDemo(options),
            "oe1300-net-outp-demo" => Oe1300NetOutpDemo(options),
            "oe1300-net-ascii-demo" => Oe1300NetAsciiDemo(options),
            "oe1300-net-labview-demo" => Oe1300NetLabviewDemo(options),
            "oe1300-net-raw-analyze" => Oe1300NetRawAnalyze(options),
            "smb-probe" => SmbProbe(options),
            "sweep-only-run" => SweepOnlyRunCommand(options),
            "minimal-3point-run" => Minimal3PointRunCommand(options),
            "run-resolve" => RunResolveCommand(options),
            "run-execute" => RunExecuteCommand(options),
            "artifact-check" => ArtifactCheckCommand(options),
            "audit-continuity" => AuditContinuityCommand(options),
            "device-command-check" => DeviceCommandCheck(),
            "live-replay" => LiveReplayCommand(options),
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

static int Oe1300Idn(IReadOnlyDictionary<string, string> options)
{
    var portName = GetRequiredOption(options, "port");
    var baudRate = GetIntOption(options, "baud", Oe1300Defaults.SerialBaudRate);

    using var oe = Oe1300Serial.Open(portName, baudRate);
    var idn = oe.QueryIdn();
    Console.WriteLine(idn);

    if (!idn.Contains(Oe1300Defaults.RequiredIdnPrefix, StringComparison.Ordinal))
    {
        return Fail($"OE1300 identity mismatch: expected prefix `{Oe1300Defaults.RequiredIdnPrefix}`");
    }

    return 0;
}

static int Oe1300NetIdn(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);

    using var oe = Oe1300Tcp.Open(host, port);
    var idn = oe.QueryIdn();
    Console.WriteLine(idn);

    if (!idn.Contains(Oe1300Defaults.RequiredIdnPrefix, StringComparison.Ordinal))
    {
        return Fail($"OE1300 TCP identity mismatch: expected prefix `{Oe1300Defaults.RequiredIdnPrefix}`");
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
    var repeat = GetIntOption(options, "repeat", 1);
    var outDir = GetRequiredOption(options, "out-dir");

    var summary = SweepOnlyRun.Execute(new SweepOnlyRunOptions(resourceName, baudRate, host, port, repeat, outDir));
    Console.WriteLine($"sweep-only-run done: repeat={summary.RepeatCount}, frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
    return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
}

static int Minimal3PointRunCommand(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", Oe1022dDefaults.Resource);
    var baudRate = GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
    var host = GetOption(options, "host", Smb100aDefaults.Host);
    var port = GetIntOption(options, "port", Smb100aDefaults.Port);
    var xPort = GetOption(options, "x", M8812Defaults.XPort);
    var yPort = GetOption(options, "y", M8812Defaults.YPort);
    var zPort = GetOption(options, "z", M8812Defaults.ZPort);
    var cycles = GetIntOption(options, "cycles", 1);
    var enableLaser = GetBoolOption(options, "laser-background", false);
    var laserPort = GetOption(options, "laser-port", CniLaserDefaults.Port);
    var laserPowerMw = GetIntOption(options, "laser-power-mw", 50);
    var outDir = GetRequiredOption(options, "out-dir");

    var summary = Minimal3PointRun.Execute(new Minimal3PointRunOptions(resourceName, baudRate, host, port, xPort, yPort, zPort, cycles, enableLaser, laserPort, laserPowerMw, outDir));
    Console.WriteLine($"minimal-3point-run done: points={summary.PointCount}, cycles={summary.Cycles}, laser={summary.LaserEnabled}, frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
    return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
}

static int RunResolveCommand(IReadOnlyDictionary<string, string> options)
{
    var bundle = RunConfigLoader.Load(
        GetRequiredOption(options, "station"),
        GetRequiredOption(options, "calibration"),
        GetRequiredOption(options, "plan"),
        GetRequiredOption(options, "smb-profile"),
        GetRequiredOption(options, "oe-profile"),
        GetRequiredOption(options, "laser-profile"));

    Console.WriteLine(JsonSerializer.Serialize(bundle.ToSummary(), JsonOptions.Pretty));
    return 0;
}

static int RunExecuteCommand(IReadOnlyDictionary<string, string> options)
{
    var planPath = GetRequiredOption(options, "plan");
    var outDir = GetRequiredOption(options, "out-dir");
    var plan = RunConfigLoader.ReadJson<AcquisitionRunPlan>(planPath);
    var progressPath = GetOptionalOption(options, "progress-jsonl");
    using var progress = string.IsNullOrWhiteSpace(progressPath)
        ? null
        : new ProgressJsonlWriter(progressPath, plan.RunId);
    using var stopAfterPointCancellation = new CancellationTokenSource();
    using var emergencyCancellation = new CancellationTokenSource();
    using var watcherCancellation = new CancellationTokenSource();
    var stopWatcher = StartRequestFileWatcher(GetOptionalOption(options, "stop-request-file"), stopAfterPointCancellation, watcherCancellation.Token);
    var emergencyWatcher = StartRequestFileWatcher(GetOptionalOption(options, "emergency-stop-file"), emergencyCancellation, watcherCancellation.Token);

    RunSummaryRecord summary;
    try
    {
        summary = ConfigDrivenRun.Execute(new ConfigDrivenRunOptions(
            GetRequiredOption(options, "station"),
            GetRequiredOption(options, "calibration"),
            planPath,
            GetRequiredOption(options, "smb-profile"),
            GetRequiredOption(options, "oe-profile"),
            GetRequiredOption(options, "laser-profile"),
            outDir,
            progress,
            stopAfterPointCancellation.Token,
            emergencyCancellation.Token));
    }
    finally
    {
        watcherCancellation.Cancel();
        WaitForWatcher(stopWatcher);
        WaitForWatcher(emergencyWatcher);
    }

    Console.WriteLine($"run-execute done: run_id={summary.RunId}, status={summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames_ok={summary.FramesTotal}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
    return summary.Status is "completed" or "completed_with_failed_points" &&
        summary.TimeoutCount == 0 &&
        summary.RawLenBadCount == 0 &&
        summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
}

static Task? StartRequestFileWatcher(string? requestPath, CancellationTokenSource requestCancellation, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(requestPath))
    {
        return null;
    }

    return Task.Run(async () =>
    {
        while (!cancellationToken.IsCancellationRequested && !requestCancellation.IsCancellationRequested)
        {
            if (File.Exists(requestPath))
            {
                requestCancellation.Cancel();
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken).ConfigureAwait(false);
        }
    }, cancellationToken);
}

static void WaitForWatcher(Task? watcher)
{
    if (watcher is null)
    {
        return;
    }

    try
    {
        watcher.Wait(TimeSpan.FromSeconds(1));
    }
    catch (AggregateException ex) when (ex.InnerExceptions.All(inner => inner is TaskCanceledException or OperationCanceledException))
    {
    }
}

static int ArtifactCheckCommand(IReadOnlyDictionary<string, string> options)
{
    var report = ArtifactCheck.Check(GetRequiredOption(options, "run"));
    Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions.Pretty));
    return report.Passed ? 0 : 2;
}

static int AuditContinuityCommand(IReadOnlyDictionary<string, string> options)
{
    var runDir = GetRequiredOption(options, "run");
    var outPath = GetRequiredOption(options, "out");
    var report = ContinuityAudit.Audit(runDir);
    ContinuityAudit.WriteReport(outPath, report);
    Console.WriteLine($"continuity audit done: verdict={report.Verdict}, frames={report.FramesTotal}, delta_gt1={report.DevicePacketCounter.DeltaGt1Count}, out={outPath}");
    return report.Verdict == "continuous" ? 0 : 2;
}

static int DeviceCommandCheck()
{
    var report = DeviceCommandCatalog.Check();
    Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions.Pretty));
    return report.Passed ? 0 : 2;
}

static int LiveReplayCommand(IReadOnlyDictionary<string, string> options)
{
    var snapshot = LiveReplay.Replay(
        GetRequiredOption(options, "run"),
        GetIntOption(options, "tail-events", 20));
    Console.WriteLine(JsonSerializer.Serialize(snapshot, JsonOptions.Pretty));
    return snapshot.CollectorHealth == "clean" ? 0 : 2;
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

static int Oe1300Rall(IReadOnlyDictionary<string, string> options)
{
    var portName = GetRequiredOption(options, "port");
    var baudRate = GetIntOption(options, "baud", Oe1300Defaults.SerialBaudRate);
    var count = GetIntOption(options, "count", 1);
    var outDir = GetRequiredOption(options, "out-dir");

    if (count <= 0)
    {
        return Fail("--count must be positive");
    }

    Directory.CreateDirectory(outDir);
    using var oe = Oe1300Serial.Open(portName, baudRate);
    var latencies = new List<double>(count);
    var captures = new List<object>(count);

    for (var i = 0; i < count; i++)
    {
        var started = Stopwatch.GetTimestamp();
        var snapshot = oe.QueryRall();
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        latencies.Add(elapsedMs);

        var captureBase = Path.Combine(outDir, $"capture_{i:0000}.rall");
        var rawPath = captureBase + ".txt";
        var jsonPath = captureBase + ".json";
        File.WriteAllText(rawPath, snapshot.RawResponse + Environment.NewLine, new UTF8Encoding(false));
        File.WriteAllText(jsonPath, JsonSerializer.Serialize(snapshot, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));

        captures.Add(new
        {
            index = i,
            latency_ms = elapsedMs,
            raw_path = PathRelative(outDir, rawPath),
            json_path = PathRelative(outDir, jsonPath),
            field_count = snapshot.Values.Count,
            first_values = snapshot.Values.Take(4).ToArray(),
            last_values = snapshot.Values.Skip(Math.Max(0, snapshot.Values.Count - 3)).ToArray()
        });
    }

    var summary = new
    {
        transport = "serial_ascii",
        port = portName,
        baud = baudRate,
        count,
        started_at = UtcNowString(),
        mean_latency_ms = latencies.Average(),
        min_latency_ms = latencies.Min(),
        max_latency_ms = latencies.Max(),
        estimated_hz = 1000.0 / latencies.Average(),
        field_names = Oe1300Defaults.SerialRallFieldNames,
        captures
    };

    var summaryPath = Path.Combine(outDir, "summary.json");
    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-rall done: count={count}, mean_latency_ms={latencies.Average():0.###}, estimated_hz={1000.0 / latencies.Average():0.###}, out_dir={outDir}");
    return 0;
}

static int Oe1300NetRall(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);
    var count = GetIntOption(options, "count", 1);
    var outDir = GetRequiredOption(options, "out-dir");

    if (count <= 0)
    {
        return Fail("--count must be positive");
    }

    Directory.CreateDirectory(outDir);
    using var oe = Oe1300Tcp.Open(host, port);
    var latencies = new List<double>(count);
    var byteCounts = new List<int>(count);
    var captures = new List<object>(count);

    for (var i = 0; i < count; i++)
    {
        var started = Stopwatch.GetTimestamp();
        var payload = oe.QueryRallBinary();
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        latencies.Add(elapsedMs);
        byteCounts.Add(payload.Length);

        var captureBase = Path.Combine(outDir, $"capture_{i:0000}.rall");
        var rawPath = captureBase + ".bin";
        File.WriteAllBytes(rawPath, payload);

        string? jsonPath = null;
        Oe1300TcpRallCapture? decode = null;
        if (payload.Length >= Oe1300Defaults.TcpRallExpectedBytes)
        {
            decode = Oe1300Parsers.DecodeTcpRall(payload);
            jsonPath = captureBase + ".json";
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(decode, JsonOptions.PrettyNamedFloatingPoint) + Environment.NewLine, new UTF8Encoding(false));
        }

        captures.Add(new
        {
            index = i,
            latency_ms = elapsedMs,
            bytes = payload.Length,
            raw_path = PathRelative(outDir, rawPath),
            json_path = jsonPath is null ? null : PathRelative(outDir, jsonPath),
            decoded = decode is not null,
            head_hex = Convert.ToHexString(payload, 0, Math.Min(16, payload.Length)).ToLowerInvariant()
        });
    }

    var summary = new
    {
        transport = "tcp_binary",
        host,
        port,
        count,
        started_at = UtcNowString(),
        mean_latency_ms = latencies.Average(),
        min_latency_ms = latencies.Min(),
        max_latency_ms = latencies.Max(),
        mean_bytes = byteCounts.Average(),
        min_bytes = byteCounts.Min(),
        max_bytes = byteCounts.Max(),
        expected_bytes = Oe1300Defaults.TcpRallExpectedBytes,
        labview_payload_bytes = Oe1300Defaults.TcpRallPayloadBytes,
        labview_frame_bytes = Oe1300Defaults.TcpRallFrameBytes,
        decode_mode = "frame_mean_big_endian_v1",
        field_names = Oe1300Defaults.SerialRallFieldNames,
        captures
    };

    var summaryPath = Path.Combine(outDir, "summary.json");
    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-rall done: count={count}, mean_latency_ms={latencies.Average():0.###}, mean_bytes={byteCounts.Average():0.###}, out_dir={outDir}");
    return 0;
}

static int Oe1300NetCollectorDemo(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);
    var durationSec = GetIntOption(options, "duration-sec", 0);
    var outDir = GetRequiredOption(options, "out-dir");
    var postWriteDelayMs = GetIntOption(options, "post-write-delay-ms", 5);
    var decodeInLoop = GetBoolOption(options, "decode-in-loop", false);
    var writeArtifacts = GetBoolOption(options, "write-artifacts", true);
    var drainBeforeWrite = GetBoolOption(options, "drain-before-write", true);

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    if (postWriteDelayMs < 0)
    {
        return Fail("--post-write-delay-ms must be non-negative");
    }

    var rawDir = Path.Combine(outDir, "raw");
    Directory.CreateDirectory(rawDir);

    var rawPath = Path.Combine(rawDir, "oe1300_tcp.rall");
    var indexPath = Path.Combine(rawDir, "oe1300_tcp.frames.idx.jsonl");
    var segmentsPath = Path.Combine(outDir, "segments.jsonl");
    var summaryPath = Path.Combine(outDir, "summary.json");

    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    var nextRawOffset = 0L;
    var frameSeq = 0L;
    var stats = new ProbeStats();
    var decodeFramesOk = 0L;
    var decodeFailures = 0L;
    string? firstFrameTs = null;
    string? lastFrameTs = null;
    ulong? firstFrameMonotonicNs = null;
    ulong? lastFrameMonotonicNs = null;

    using var oe = Oe1300Tcp.Open(host, port);
    using var raw = writeArtifacts
        ? new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024)
        : null;
    using var index = writeArtifacts
        ? new StreamWriter(
            new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
            new UTF8Encoding(false))
        : null;
    var payload = new byte[Oe1300Defaults.TcpRallExpectedBytes];

    // Minimal OE1300 TCP collector hot path: write RALL?, wait, read-until-32768B,
    // append raw, append frame index. No decode, retry, GUI, or per-frame JSON here.
    while (Stopwatch.GetTimestamp() < deadline)
    {
        stats.ReadAttempts++;

        try
        {
            var bytesRead = oe.ReadRallFrame(
                payload,
                Oe1300Defaults.TcpRallExpectedBytes,
                postWriteDelayMs,
                drainBeforeWrite: drainBeforeWrite);
            if (bytesRead != Oe1300Defaults.TcpRallExpectedBytes)
            {
                stats.RawLenBadCount++;
                stats.ReadErrors++;
                continue;
            }

            var monotonicNs = MonotonicNsSince(processStart);
            var ts = writeArtifacts ? UtcNowString() : string.Empty;
            raw?.Write(payload, 0, bytesRead);
            firstFrameTs ??= writeArtifacts ? ts : startedAt;
            firstFrameMonotonicNs ??= monotonicNs;
            lastFrameTs = writeArtifacts ? ts : startedAt;
            lastFrameMonotonicNs = monotonicNs;

            if (decodeInLoop)
            {
                try
                {
                    _ = Oe1300Parsers.DecodeTcpRall(payload);
                    decodeFramesOk++;
                }
                catch
                {
                    decodeFailures++;
                    stats.ReadErrors++;
                    continue;
                }
            }

            if (index is not null)
            {
                WriteFrameIndexRecordNoCounter(index, frameSeq, ts, monotonicNs, nextRawOffset, bytesRead);
            }
            nextRawOffset += bytesRead;
            frameSeq++;
            stats.FramesOk++;
        }
        catch (IOException)
        {
            stats.TimeoutCount++;
            stats.ReadErrors++;
        }
        catch (Exception ex)
        {
            stats.ReadErrors++;
            Console.Error.WriteLine($"OE1300 TCP RALL read error: {ex.Message}");
        }
    }

    raw?.Flush(true);
    index?.Flush();

    var finishedAt = UtcNowString();
    if (writeArtifacts)
    {
        WriteWholeProbeSegment(
            segmentsPath,
            outDir,
            "oe1300_tcp_demo",
            "oe1300_tcp",
            "raw/oe1300_tcp.rall",
            firstFrameTs ?? startedAt,
            lastFrameTs ?? finishedAt,
            firstFrameMonotonicNs ?? 0,
            lastFrameMonotonicNs ?? MonotonicNsSince(processStart),
            nextRawOffset,
            stats.FramesOk);
    }

    var elapsedMs = (long)(Stopwatch.GetElapsedTime(processStart).TotalMilliseconds);
    var rawBytesWritten = writeArtifacts ? new FileInfo(rawPath).Length : stats.FramesOk * (long)Oe1300Defaults.TcpRallExpectedBytes;
    var meanFrameMs = stats.FramesOk > 0 ? elapsedMs / (double)stats.FramesOk : 0.0;
    var estimatedHz = meanFrameMs > 0 ? 1000.0 / meanFrameMs : 0.0;
    var summary = new
    {
        command = "oe1300-net-collector-demo",
        transport = "tcp_binary",
        host,
        port,
        frame_bytes = Oe1300Defaults.TcpRallExpectedBytes,
        post_write_delay_ms = postWriteDelayMs,
        decode_in_loop = decodeInLoop,
        write_artifacts = writeArtifacts,
        drain_before_write = drainBeforeWrite,
        read_timeout_ms = Oe1300Defaults.TcpReadTimeoutMs,
        duration_sec = durationSec,
        started_at = startedAt,
        finished_at = finishedAt,
        elapsed_ms = elapsedMs,
        read_attempts = stats.ReadAttempts,
        frames_ok = stats.FramesOk,
        read_errors = stats.ReadErrors,
        timeout_count = stats.TimeoutCount,
        raw_len_bad_count = stats.RawLenBadCount,
        decode_frames_ok = decodeFramesOk,
        decode_failures = decodeFailures,
        raw_bytes_written = rawBytesWritten,
        raw_size_matches_frames_ok = rawBytesWritten == stats.FramesOk * Oe1300Defaults.TcpRallExpectedBytes,
        mean_frame_ms = meanFrameMs,
        estimated_query_hz = estimatedHz,
        raw_path = writeArtifacts ? PathRelative(outDir, rawPath) : null,
        index_path = writeArtifacts ? PathRelative(outDir, indexPath) : null,
        segments_path = writeArtifacts ? PathRelative(outDir, segmentsPath) : null
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-collector-demo done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, query_hz={estimatedHz:0.###}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 ? 0 : 2;
}

static int Oe1300NetRawAnalyze(IReadOnlyDictionary<string, string> options)
{
    var rawPath = GetRequiredOption(options, "raw");
    var frameBytes = GetIntOption(options, "frame-bytes", Oe1300Defaults.TcpRallExpectedBytes);
    var maxFrames = GetIntOption(options, "max-frames", 0);
    var durationSec = GetDoubleOption(options, "duration-sec", 0.0);

    if (!File.Exists(rawPath))
    {
        return Fail($"raw file not found: {rawPath}");
    }

    if (frameBytes <= 0)
    {
        return Fail("--frame-bytes must be positive");
    }

    using var stream = new FileStream(rawPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024 * 1024);
    using var sha256 = SHA256.Create();
    var payload = new byte[frameBytes];
    string? previousHash = null;
    long framesRead = 0;
    long transitions = 0;
    long adjacentUniqueRuns = 0;
    var firstHashes = new List<string>(10);

    while (true)
    {
        if (maxFrames > 0 && framesRead >= maxFrames)
        {
            break;
        }

        var bytesRead = stream.Read(payload, 0, payload.Length);
        if (bytesRead == 0)
        {
            break;
        }

        if (bytesRead != payload.Length)
        {
            return Fail($"raw file tail is not a full OE1300 TCP frame: expected={payload.Length}, actual={bytesRead}, offset={stream.Position - bytesRead}");
        }

        framesRead++;
        var hash = Convert.ToHexString(sha256.ComputeHash(payload));

        if (firstHashes.Count < 10)
        {
            firstHashes.Add(hash[..16]);
        }

        if (previousHash is null)
        {
            adjacentUniqueRuns++;
        }
        else if (!string.Equals(previousHash, hash, StringComparison.Ordinal))
        {
            transitions++;
            adjacentUniqueRuns++;
        }

        previousHash = hash;
    }

    var summary = new
    {
        command = "oe1300-net-raw-analyze",
        raw_path = rawPath,
        frame_bytes = frameBytes,
        frames_read = framesRead,
        transitions,
        adjacent_unique_runs = adjacentUniqueRuns,
        duration_sec = durationSec > 0 ? durationSec : (double?)null,
        transition_hz = durationSec > 0 ? transitions / durationSec : (double?)null,
        adjacent_unique_hz = durationSec > 0 ? adjacentUniqueRuns / durationSec : (double?)null,
        first_hashes = firstHashes
    };

    Console.WriteLine(JsonSerializer.Serialize(summary, JsonOptions.Pretty));
    return 0;
}

static int Oe1300NetOutpDemo(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);
    var paramIndex = GetIntOption(options, "param-index", 0);
    var durationSec = GetIntOption(options, "duration-sec", 0);
    var outDir = GetRequiredOption(options, "out-dir");
    var writeValues = GetBoolOption(options, "write-values", true);

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    Directory.CreateDirectory(outDir);
    var valuesPath = Path.Combine(outDir, "values.csv");
    var summaryPath = Path.Combine(outDir, "summary.json");
    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    var command = Oe1300Commands.QueryOutput(paramIndex);
    long samplesOk = 0;
    long errors = 0;
    double minValue = double.PositiveInfinity;
    double maxValue = double.NegativeInfinity;
    string? lastValueText = null;
    double? firstLatencyMs = null;
    var warmLatencyMsSum = 0.0;
    long warmLatencyCount = 0;

    using var oe = Oe1300Tcp.Open(host, port);
    using var writer = writeValues
        ? new StreamWriter(new FileStream(valuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;

    if (writer is not null)
    {
        writer.WriteLine("sample_index,monotonic_ns,value");
    }

    while (Stopwatch.GetTimestamp() < deadline)
    {
        var readStart = Stopwatch.GetTimestamp();
        try
        {
            var response = oe.QueryAsciiLine(command);
            var latencyMs = Stopwatch.GetElapsedTime(readStart).TotalMilliseconds;
            var value = double.Parse(response, CultureInfo.InvariantCulture);
            var monotonicNs = MonotonicNsSince(processStart);

            if (firstLatencyMs is null)
            {
                firstLatencyMs = latencyMs;
            }
            else
            {
                warmLatencyMsSum += latencyMs;
                warmLatencyCount++;
            }

            if (value < minValue)
            {
                minValue = value;
            }

            if (value > maxValue)
            {
                maxValue = value;
            }

            if (writer is not null)
            {
                writer.Write(samplesOk.ToString(CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.WriteLine(value.ToString("R", CultureInfo.InvariantCulture));
            }

            lastValueText = response;
            samplesOk++;
        }
        catch
        {
            errors++;
        }
    }

    writer?.Flush();

    var elapsedMs = (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds;
    var meanQueryMs = samplesOk > 0 ? elapsedMs / (double)samplesOk : 0.0;
    var estimatedQueryHz = meanQueryMs > 0 ? 1000.0 / meanQueryMs : 0.0;
    var warmMeanLatencyMs = warmLatencyCount > 0 ? warmLatencyMsSum / warmLatencyCount : firstLatencyMs ?? 0.0;
    var warmQueryHz = warmMeanLatencyMs > 0 ? 1000.0 / warmMeanLatencyMs : 0.0;
    var summary = new
    {
        command = "oe1300-net-outp-demo",
        transport = "tcp_ascii",
        host,
        port,
        outp_command = command,
        param_index = paramIndex,
        duration_sec = durationSec,
        write_values = writeValues,
        started_at = startedAt,
        finished_at = UtcNowString(),
        elapsed_ms = elapsedMs,
        samples_ok = samplesOk,
        errors,
        first_latency_ms = firstLatencyMs,
        warm_mean_latency_ms = warmMeanLatencyMs,
        warm_query_hz = warmQueryHz,
        estimated_query_hz = estimatedQueryHz,
        min_value = samplesOk > 0 ? minValue : (double?)null,
        max_value = samplesOk > 0 ? maxValue : (double?)null,
        last_value_text = lastValueText,
        values_path = writeValues ? PathRelative(outDir, valuesPath) : null
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-outp-demo done: param_index={paramIndex}, samples_ok={samplesOk}, errors={errors}, warm_query_hz={warmQueryHz:0.###}, out_dir={outDir}");
    return errors == 0 ? 0 : 2;
}

static int Oe1300NetAsciiDemo(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);
    var mode = GetOption(options, "mode", "outp").Trim().ToLowerInvariant();
    var paramIndex = GetIntOption(options, "param-index", 0);
    var snapIndicesText = GetOption(options, "snap-indices", "0,1,2,3,34");
    var durationSec = GetIntOption(options, "duration-sec", 0);
    var outDir = GetRequiredOption(options, "out-dir");
    var parseInLoop = GetBoolOption(options, "parse-in-loop", true);
    var writeValues = GetBoolOption(options, "write-values", false);

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    string command;
    int expectedValueCount;
    int[] parameterIndices;

    switch (mode)
    {
        case "outp":
            parameterIndices = [paramIndex];
            expectedValueCount = 1;
            command = Oe1300Commands.QueryOutput(paramIndex);
            break;
        case "snap":
            parameterIndices = ParseIntList(snapIndicesText, "--snap-indices");
            if (parameterIndices.Length == 0)
            {
                return Fail("--snap-indices must contain at least one parameter index");
            }

            expectedValueCount = parameterIndices.Length;
            command = Oe1300Commands.QuerySnap(parameterIndices);
            break;
        default:
            return Fail("--mode must be `outp` or `snap`");
    }

    Directory.CreateDirectory(outDir);
    var valuesPath = Path.Combine(outDir, "values.csv");
    var summaryPath = Path.Combine(outDir, "summary.json");
    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    long samplesOk = 0;
    long errors = 0;
    long parseFailures = 0;
    double? firstLatencyMs = null;
    var warmLatencyMsSum = 0.0;
    long warmLatencyCount = 0;
    var firstValueMin = double.PositiveInfinity;
    var firstValueMax = double.NegativeInfinity;
    string? lastValueText = null;

    using var oe = Oe1300Tcp.Open(host, port);
    using var writer = writeValues
        ? new StreamWriter(new FileStream(valuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;

    if (writer is not null)
    {
        writer.Write("sample_index,monotonic_ns");
        for (var i = 0; i < expectedValueCount; i++)
        {
            writer.Write(",value_");
            writer.Write(i.ToString(CultureInfo.InvariantCulture));
        }

        writer.WriteLine();
    }

    while (Stopwatch.GetTimestamp() < deadline)
    {
        var readStart = Stopwatch.GetTimestamp();
        try
        {
            var response = oe.QueryAsciiLine(command);
            var latencyMs = Stopwatch.GetElapsedTime(readStart).TotalMilliseconds;
            var monotonicNs = MonotonicNsSince(processStart);

            if (firstLatencyMs is null)
            {
                firstLatencyMs = latencyMs;
            }
            else
            {
                warmLatencyMsSum += latencyMs;
                warmLatencyCount++;
            }

            double[]? values = null;
            if (parseInLoop || writer is not null)
            {
                values = ParseAsciiFloatList(response, expectedValueCount);
            }

            if (values is not null && values.Length > 0)
            {
                if (values[0] < firstValueMin)
                {
                    firstValueMin = values[0];
                }

                if (values[0] > firstValueMax)
                {
                    firstValueMax = values[0];
                }
            }

            if (writer is not null)
            {
                if (values is null)
                {
                    values = ParseAsciiFloatList(response, expectedValueCount);
                }

                writer.Write(samplesOk.ToString(CultureInfo.InvariantCulture));
                writer.Write(',');
                writer.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
                foreach (var value in values)
                {
                    writer.Write(',');
                    writer.Write(value.ToString("R", CultureInfo.InvariantCulture));
                }

                writer.WriteLine();
            }

            lastValueText = response;
            samplesOk++;
        }
        catch (FormatException)
        {
            parseFailures++;
            errors++;
        }
        catch (IOException)
        {
            errors++;
        }
        catch
        {
            errors++;
        }
    }

    writer?.Flush();

    var elapsedMs = (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds;
    var meanQueryMs = samplesOk > 0 ? elapsedMs / (double)samplesOk : 0.0;
    var estimatedQueryHz = meanQueryMs > 0 ? 1000.0 / meanQueryMs : 0.0;
    var warmMeanLatencyMs = warmLatencyCount > 0 ? warmLatencyMsSum / warmLatencyCount : firstLatencyMs ?? 0.0;
    var warmQueryHz = warmMeanLatencyMs > 0 ? 1000.0 / warmMeanLatencyMs : 0.0;
    var summary = new
    {
        command = "oe1300-net-ascii-demo",
        transport = "tcp_ascii",
        host,
        port,
        mode,
        ascii_command = command,
        param_index = mode == "outp" ? paramIndex : (int?)null,
        snap_indices = mode == "snap" ? parameterIndices : null,
        expected_value_count = expectedValueCount,
        parse_in_loop = parseInLoop,
        write_values = writeValues,
        duration_sec = durationSec,
        started_at = startedAt,
        finished_at = UtcNowString(),
        elapsed_ms = elapsedMs,
        samples_ok = samplesOk,
        errors,
        parse_failures = parseFailures,
        first_latency_ms = firstLatencyMs,
        warm_mean_latency_ms = warmMeanLatencyMs,
        warm_query_hz = warmQueryHz,
        estimated_query_hz = estimatedQueryHz,
        first_value_min = samplesOk > 0 && double.IsFinite(firstValueMin) ? firstValueMin : (double?)null,
        first_value_max = samplesOk > 0 && double.IsFinite(firstValueMax) ? firstValueMax : (double?)null,
        last_value_text = lastValueText,
        values_path = writeValues ? PathRelative(outDir, valuesPath) : null
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-ascii-demo done: mode={mode}, samples_ok={samplesOk}, errors={errors}, warm_query_hz={warmQueryHz:0.###}, out_dir={outDir}");
    return errors == 0 ? 0 : 2;
}

static int Oe1300NetLabviewDemo(IReadOnlyDictionary<string, string> options)
{
    var host = GetOption(options, "host", Oe1300Defaults.Host);
    var port = GetIntOption(options, "port", Oe1300Defaults.TcpPort);
    var durationSec = GetIntOption(options, "duration-sec", 0);
    var outDir = GetRequiredOption(options, "out-dir");
    var postWriteDelayMs = GetIntOption(options, "post-write-delay-ms", 5);
    var drainBeforeWrite = GetBoolOption(options, "drain-before-write", false);
    var writeValues = GetBoolOption(options, "write-values", false);
    var previewParamIndex = GetIntOption(options, "preview-param-index", 0);

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    if (previewParamIndex < 0 || previewParamIndex >= Oe1300Defaults.TcpRallLabviewParameterCount)
    {
        return Fail($"--preview-param-index must be between 0 and {Oe1300Defaults.TcpRallLabviewParameterCount - 1}");
    }

    Directory.CreateDirectory(outDir);
    var valuesPath = Path.Combine(outDir, "preview_values.csv");
    var parameterValuesPath = Path.Combine(outDir, "parameter_values.csv");
    var summaryPath = Path.Combine(outDir, "summary.json");
    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    var payload = new byte[Oe1300Defaults.TcpRallExpectedBytes];
    var previewParamName = Oe1300Defaults.SerialRallFieldNames[previewParamIndex];

    var stats = new ProbeStats();
    long decodedRallsOk = 0;
    long decodeFailures = 0;
    long decodedSamplesPerParameter = 0;
    long globalSampleIndex = 0;
    long previewFiniteCount = 0;
    long previewNonFiniteCount = 0;
    double previewMin = double.PositiveInfinity;
    double previewMax = double.NegativeInfinity;
    double previewSum = 0.0;
    byte? lastStatus = null;
    string? lastStatusHex = null;
    byte? lastTrigCount = null;

    using var oe = Oe1300Tcp.Open(host, port);
    using var previewWriter = writeValues
        ? new StreamWriter(new FileStream(valuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;
    using var parameterWriter = new StreamWriter(new FileStream(parameterValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false));

    if (previewWriter is not null)
    {
        previewWriter.WriteLine("rall_index,sample_in_rall,global_sample_index,monotonic_ns,param_index,param_name,value");
    }

    parameterWriter.Write("rall_index,monotonic_ns,status_hex,status_byte,trig_count");
    foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
    {
        parameterWriter.Write(',');
        parameterWriter.Write(fieldName);
    }
    parameterWriter.WriteLine();

    while (Stopwatch.GetTimestamp() < deadline)
    {
        stats.ReadAttempts++;
        try
        {
            var bytesRead = oe.ReadRallFrame(
                payload,
                Oe1300Defaults.TcpRallExpectedBytes,
                postWriteDelayMs,
                drainBeforeWrite: drainBeforeWrite);

            if (bytesRead != Oe1300Defaults.TcpRallExpectedBytes)
            {
                stats.RawLenBadCount++;
                stats.ReadErrors++;
                continue;
            }

            var namedSeries = Oe1300Parsers.DecodeTcpRallLabviewNamedSeries(payload);
            var previewSeries = namedSeries[previewParamName];
            var monotonicNs = MonotonicNsSince(processStart);
            var statusHex = Convert.ToHexString(payload, Oe1300Defaults.TcpRallStatusOffset, Oe1300Defaults.TcpRallStatusByteCount).ToLowerInvariant();
            var statusByte = payload[Oe1300Defaults.TcpRallStatusOffset];
            var trigCount = payload[Oe1300Defaults.TcpRallTrigCountOffset];

            foreach (var value in previewSeries)
            {
                if (!double.IsFinite(value))
                {
                    previewNonFiniteCount++;
                    continue;
                }

                previewFiniteCount++;

                if (value < previewMin)
                {
                    previewMin = value;
                }

                if (value > previewMax)
                {
                    previewMax = value;
                }

                previewSum += value;
            }

            if (previewWriter is not null)
            {
                for (var sampleIndexInRall = 0; sampleIndexInRall < previewSeries.Length; sampleIndexInRall++)
                {
                    previewWriter.Write(decodedRallsOk.ToString(CultureInfo.InvariantCulture));
                    previewWriter.Write(',');
                    previewWriter.Write(sampleIndexInRall.ToString(CultureInfo.InvariantCulture));
                    previewWriter.Write(',');
                    previewWriter.Write(globalSampleIndex.ToString(CultureInfo.InvariantCulture));
                    previewWriter.Write(',');
                    previewWriter.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
                    previewWriter.Write(',');
                    previewWriter.Write(previewParamIndex.ToString(CultureInfo.InvariantCulture));
                    previewWriter.Write(',');
                    previewWriter.Write(previewParamName);
                    previewWriter.Write(',');
                    previewWriter.WriteLine(previewSeries[sampleIndexInRall].ToString("R", CultureInfo.InvariantCulture));
                    globalSampleIndex++;
                }
            }
            else
            {
                globalSampleIndex += previewSeries.Length;
            }

            parameterWriter.Write(decodedRallsOk.ToString(CultureInfo.InvariantCulture));
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
                var series = namedSeries[fieldName];
                var mean = series.Average();
                parameterWriter.Write(',');
                parameterWriter.Write(mean.ToString("R", CultureInfo.InvariantCulture));
            }
            parameterWriter.WriteLine();

            decodedSamplesPerParameter += previewSeries.Length;
            lastStatus = statusByte;
            lastStatusHex = statusHex;
            lastTrigCount = trigCount;
            decodedRallsOk++;
            stats.FramesOk++;
        }
        catch (IOException)
        {
            stats.TimeoutCount++;
            stats.ReadErrors++;
        }
        catch (Exception)
        {
            decodeFailures++;
            stats.ReadErrors++;
        }
    }

    previewWriter?.Flush();
    parameterWriter.Flush();

    var elapsedMs = (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds;
    var meanRallMs = decodedRallsOk > 0 ? elapsedMs / (double)decodedRallsOk : 0.0;
    var queryHz = meanRallMs > 0 ? 1000.0 / meanRallMs : 0.0;
    var effectivePerParameterHz = queryHz * Oe1300Defaults.TcpRallLabviewSamplesPerParameter;
    var effectiveTotalScalarHz = effectivePerParameterHz * Oe1300Defaults.TcpRallLabviewParameterCount;
    var previewMean = previewFiniteCount > 0 ? previewSum / previewFiniteCount : 0.0;

    var summary = new
    {
        command = "oe1300-net-labview-demo",
        transport = "tcp_binary",
        host,
        port,
        duration_sec = durationSec,
        post_write_delay_ms = postWriteDelayMs,
        drain_before_write = drainBeforeWrite,
        write_values = writeValues,
        preview_param_index = previewParamIndex,
        preview_param_name = previewParamName,
        decode_mode = "labview_37_parameters_x_100_samples_big_endian_v2",
        rall_payload_bytes = Oe1300Defaults.TcpRallPayloadBytes,
        rall_frame_bytes = Oe1300Defaults.TcpRallFrameBytes,
        labview_parameter_count = Oe1300Defaults.TcpRallLabviewParameterCount,
        labview_frames_per_parameter = Oe1300Defaults.TcpRallLabviewFramesPerParameter,
        labview_samples_per_parameter = Oe1300Defaults.TcpRallLabviewSamplesPerParameter,
        started_at = startedAt,
        finished_at = UtcNowString(),
        elapsed_ms = elapsedMs,
        read_attempts = stats.ReadAttempts,
        ralls_ok = decodedRallsOk,
        read_errors = stats.ReadErrors,
        timeout_count = stats.TimeoutCount,
        raw_len_bad_count = stats.RawLenBadCount,
        decode_failures = decodeFailures,
        query_hz = queryHz,
        decoded_samples_per_parameter_total = decodedSamplesPerParameter,
        effective_sample_hz_per_parameter = effectivePerParameterHz,
        effective_total_scalar_hz = effectiveTotalScalarHz,
        preview_finite_count = previewFiniteCount,
        preview_non_finite_count = previewNonFiniteCount,
        preview_value_min = previewFiniteCount > 0 && double.IsFinite(previewMin) ? previewMin : (double?)null,
        preview_value_max = previewFiniteCount > 0 && double.IsFinite(previewMax) ? previewMax : (double?)null,
        preview_value_mean = previewFiniteCount > 0 && double.IsFinite(previewMean) ? previewMean : (double?)null,
        last_status_hex = lastStatusHex,
        last_status_byte = lastStatus,
        last_trig_count = lastTrigCount,
        parameter_values_path = PathRelative(outDir, parameterValuesPath),
        values_path = writeValues ? PathRelative(outDir, valuesPath) : null
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-labview-demo done: ralls_ok={decodedRallsOk}, query_hz={queryHz:0.###}, effective_sample_hz_per_parameter={effectivePerParameterHz:0.###}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && decodeFailures == 0 ? 0 : 2;
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

static string? GetOptionalOption(IReadOnlyDictionary<string, string> options, string key) =>
    options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

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

static double GetDoubleOption(IReadOnlyDictionary<string, string> options, string key, double defaultValue)
{
    if (!options.TryGetValue(key, out var value))
    {
        return defaultValue;
    }

    if (!double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsed))
    {
        throw new ArgumentException($"--{key} must be a number");
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

static int[] ParseIntList(string text, string optionName)
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return [];
    }

    var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    var values = new int[parts.Length];
    for (var i = 0; i < parts.Length; i++)
    {
        if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out values[i]))
        {
            throw new ArgumentException($"{optionName} contains a non-integer value: {parts[i]}");
        }
    }

    return values;
}

static double[] ParseAsciiFloatList(string response, int expectedCount)
{
    var fields = response.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (fields.Length != expectedCount)
    {
        throw new FormatException($"ASCII response field count mismatch: expected {expectedCount}, actual={fields.Length}");
    }

    var values = new double[fields.Length];
    for (var i = 0; i < fields.Length; i++)
    {
        values[i] = double.Parse(fields[i], CultureInfo.InvariantCulture);
    }

    return values;
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

static void WriteFrameIndexRecordNoCounter(
    StreamWriter writer,
    long frameSeq,
    string ts,
    ulong monotonicNs,
    long rawOffset,
    int rawLen)
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
    writer.Write(",\"device_packet_counter\":null,\"parse_status\":\"not_parsed\",\"duplicate_of\":null}");
    writer.WriteLine();
}

static void WriteWholeProbeSegment(
    string segmentsPath,
    string outDir,
    string pointId,
    string source,
    string rawFile,
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
        "seg_probe_0000",
        pointId,
        source,
        startTs,
        endTs,
        startMonotonicNs,
        endMonotonicNs,
        rawFile,
        0,
        rawOffsetEnd,
        framesOk > 0 ? 0 : null,
        framesOk > 0 ? framesOk - 1 : null);

    File.WriteAllText(segmentsPath, JsonSerializer.Serialize(segment, JsonOptions.Default) + Environment.NewLine, new UTF8Encoding(false));
}

static void PrintUsage()
{
    Console.WriteLine("""
    Odmr.WinProbe

    Usage:
      Odmr.WinProbe visa-list
      Odmr.WinProbe oe-idn [--resource ASRL8::INSTR] [--baud 921600]
      Odmr.WinProbe oe-rall [--resource ASRL8::INSTR] [--baud 921600] --duration-sec 300 --out-dir <dir>
      Odmr.WinProbe oe1300-idn --port <COMx> [--baud 115200]
      Odmr.WinProbe oe1300-rall --port <COMx> [--baud 115200] [--count 1] --out-dir <dir>
      Odmr.WinProbe oe1300-net-idn [--host 192.168.1.1] [--port 10001]
      Odmr.WinProbe oe1300-net-rall [--host 192.168.1.1] [--port 10001] [--count 1] --out-dir <dir>
      Odmr.WinProbe oe1300-net-collector-demo [--host 192.168.1.1] [--port 10001] [--post-write-delay-ms 5] [--decode-in-loop true|false] [--write-artifacts true|false] [--drain-before-write true|false] --duration-sec 60 --out-dir <dir>
      Odmr.WinProbe oe1300-net-outp-demo [--host 192.168.1.1] [--port 10001] [--param-index 0] [--write-values true|false] --duration-sec 10 --out-dir <dir>
      Odmr.WinProbe oe1300-net-ascii-demo [--host 192.168.1.1] [--port 10001] [--mode outp|snap] [--param-index 0] [--snap-indices 0,1,2,3,34] [--parse-in-loop true|false] [--write-values true|false] --duration-sec 10 --out-dir <dir>
      Odmr.WinProbe oe1300-net-labview-demo [--host 192.168.1.1] [--port 10001] [--post-write-delay-ms 5] [--drain-before-write true|false] [--preview-param-index 0] [--write-values true|false] --duration-sec 10 --out-dir <dir>
      Odmr.WinProbe oe1300-net-raw-analyze --raw <raw/oe1300_tcp.rall> [--frame-bytes 32768] [--max-frames 5000] [--duration-sec 60]
      Odmr.WinProbe smb-probe [--host 169.254.2.20] [--port 5025]
      Odmr.WinProbe sweep-only-run [--resource ASRL8::INSTR] [--baud 921600] [--host 169.254.2.20] [--port 5025] [--repeat 1] --out-dir <dir>
      Odmr.WinProbe minimal-3point-run [--resource ASRL8::INSTR] [--baud 921600] [--host 169.254.2.20] [--port 5025] [--x COM4] [--y COM6] [--z COM3] [--cycles 1] [--laser-background] [--laser-port COM9] [--laser-power-mw 50] --out-dir <dir>
      Odmr.WinProbe run-resolve --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json>
      Odmr.WinProbe run-execute --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json> --out-dir <dir> [--progress-jsonl <path>] [--stop-request-file <path>] [--emergency-stop-file <path>]
      Odmr.WinProbe artifact-check --run <run-dir>
      Odmr.WinProbe audit-continuity --run <run-dir> --out <json>
      Odmr.WinProbe device-command-check
      Odmr.WinProbe live-replay --run <run-dir> [--tail-events 20]
      Odmr.WinProbe m8812-probe [--x COM4] [--y COM6] [--z COM3]
      Odmr.WinProbe laser-probe [--port COM9] --off-only
    """);
}

sealed class ProgressJsonlWriter : IProgress<RunProgressEvent>, IDisposable
{
    private readonly string runId;
    private readonly StreamWriter writer;
    private readonly object gate = new();
    private bool failed;

    public ProgressJsonlWriter(string path, string runId)
    {
        this.runId = runId;
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        writer = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false))
        {
            AutoFlush = true
        };
    }

    public void Report(RunProgressEvent value)
    {
        if (failed)
        {
            return;
        }

        try
        {
            var record = new
            {
                schema_version = 1,
                ts = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture),
                pid = Environment.ProcessId,
                run_id = runId,
                state = value.State.ToString(),
                event_name = value.EventName,
                message = value.Message,
                point_id = value.PointId,
                point_index = value.PointIndex,
                points_total = value.PointsTotal,
                frames_total = value.FramesTotal,
                timeout_count = value.TimeoutCount,
                raw_len_bad_count = value.RawLenBadCount,
                delta_gt1_count = value.DeltaGt1Count,
                quality_status = value.QualityStatus,
                estimated_run_duration_ms = value.EstimatedRunDurationMs,
                estimated_point_duration_ms = value.EstimatedPointDurationMs,
                estimated_sweep_duration_ms = value.EstimatedSweepDurationMs,
                sweep_points = value.SweepPoints,
                start_hz = value.StartHz,
                stop_hz = value.StopHz,
                step_hz = value.StepHz,
                dwell_ms = value.DwellMs
            };

            lock (gate)
            {
                writer.WriteLine(JsonSerializer.Serialize(record, JsonOptions.Default));
            }
        }
        catch
        {
            failed = true;
        }
    }

    public void Dispose() => writer.Dispose();
}
