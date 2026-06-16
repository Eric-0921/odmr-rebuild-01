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
            "oe1300-idn" => Oe1300Idn(options),
            "oe1300-rall" => Oe1300Rall(options),
            "oe1300-net-idn" => Oe1300NetIdn(options),
            "oe1300-net-rall" => Oe1300NetRall(options),
            "oe1300-net-collector-demo" => Oe1300NetCollectorDemo(options),
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
            var bytesRead = oe.ReadRallFrame(payload, Oe1300Defaults.TcpRallExpectedBytes, postWriteDelayMs);
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
        estimated_hz = estimatedHz,
        raw_path = writeArtifacts ? PathRelative(outDir, rawPath) : null,
        index_path = writeArtifacts ? PathRelative(outDir, indexPath) : null,
        segments_path = writeArtifacts ? PathRelative(outDir, segmentsPath) : null
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
    Console.WriteLine($"oe1300-net-collector-demo done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, estimated_hz={estimatedHz:0.###}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 ? 0 : 2;
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
      Odmr.WinProbe oe1300-net-collector-demo [--host 192.168.1.1] [--port 10001] [--post-write-delay-ms 5] [--decode-in-loop true|false] [--write-artifacts true|false] --duration-sec 60 --out-dir <dir>
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
