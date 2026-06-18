using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Buffers.Binary;
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
            "oe1300-net-labview-demo" => Oe1300NetLabviewDemo(options),
            "smb-probe" => SmbProbe(options),
            "sweep-only-run" => SweepOnlyRunCommand(options),
            "minimal-3point-run" => Minimal3PointRunCommand(options),
            "run-resolve" => RunResolveCommand(options),
            "run-execute" => RunExecuteCommand(options),
            "resume-run" => ResumeRunCommand(options),
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
    if (GetBoolOption(options, "list-resources", false))
    {
        foreach (var visaResource in Smb100aVisa.ListResources())
        {
            Console.WriteLine(visaResource);
        }

        return 0;
    }

    var resource = GetOptionalOption(options, "resource") ?? ResolveSmbVisaResource();
    var result = Smb100aVisa.Probe(resource);

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
    var smbResource = GetOptionalOption(options, "smb-resource") ?? ResolveSmbVisaResource();
    var repeat = GetIntOption(options, "repeat", 1);
    var outDir = GetRequiredOption(options, "out-dir");

    var summary = SweepOnlyRun.Execute(new SweepOnlyRunOptions(resourceName, baudRate, smbResource, repeat, outDir));
    Console.WriteLine($"sweep-only-run done: repeat={summary.RepeatCount}, frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
    return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
}

static int Minimal3PointRunCommand(IReadOnlyDictionary<string, string> options)
{
    var resourceName = GetOption(options, "resource", Oe1022dDefaults.Resource);
    var baudRate = GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
    var smbResource = GetOptionalOption(options, "smb-resource") ?? ResolveSmbVisaResource();
    var xPort = GetOption(options, "x", M8812Defaults.XPort);
    var yPort = GetOption(options, "y", M8812Defaults.YPort);
    var zPort = GetOption(options, "z", M8812Defaults.ZPort);
    var cycles = GetIntOption(options, "cycles", 1);
    var enableLaser = GetBoolOption(options, "laser-background", false);
    var laserPort = GetOption(options, "laser-port", CniLaserDefaults.Port);
    var laserPowerMw = GetIntOption(options, "laser-power-mw", 50);
    var outDir = GetRequiredOption(options, "out-dir");

    var summary = Minimal3PointRun.Execute(new Minimal3PointRunOptions(resourceName, baudRate, smbResource, xPort, yPort, zPort, cycles, enableLaser, laserPort, laserPowerMw, outDir));
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

    var runExecuteContinuityMetric = summary.LockinModel == "oe1300"
        ? $"effective_sample_hz_per_parameter={summary.EffectiveSampleHzPerParameter:0.###}"
        : $"delta_gt1={summary.PacketCounter?.DeltaGt1Count ?? 0}";
    Console.WriteLine($"run-execute done: run_id={summary.RunId}, status={summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames_ok={summary.FramesTotal}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, {runExecuteContinuityMetric}, out_dir={outDir}");
    return summary.LockinModel == "oe1300"
        ? summary.Status is "completed" or "completed_with_failed_points" or "paused" &&
            summary.TimeoutCount == 0 &&
            summary.RawLenBadCount == 0 &&
            (summary.DecodeFailures ?? 0) == 0 &&
            (summary.EffectiveSampleHzPerParameter ?? 0) >= 900 ? 0 : 2
        : summary.Status is "completed" or "completed_with_failed_points" or "paused" &&
            summary.TimeoutCount == 0 &&
            summary.RawLenBadCount == 0 &&
            (summary.PacketCounter?.DeltaGt1Count ?? 0) == 0 ? 0 : 2;
}

static int ResumeRunCommand(IReadOnlyDictionary<string, string> options)
{
    var previousRunDir = GetRequiredOption(options, "previous-run");
    var outDir = GetRequiredOption(options, "out-dir");
    var progressPath = GetOptionalOption(options, "progress-jsonl");
    using var progress = string.IsNullOrWhiteSpace(progressPath)
        ? null
        : new ProgressJsonlWriter(progressPath, Path.GetFileName(Path.GetFullPath(outDir)));
    using var stopAfterPointCancellation = new CancellationTokenSource();
    using var emergencyCancellation = new CancellationTokenSource();
    using var watcherCancellation = new CancellationTokenSource();
    var stopWatcher = StartRequestFileWatcher(GetOptionalOption(options, "stop-request-file"), stopAfterPointCancellation, watcherCancellation.Token);
    var emergencyWatcher = StartRequestFileWatcher(GetOptionalOption(options, "emergency-stop-file"), emergencyCancellation, watcherCancellation.Token);

    RunSummaryRecord summary;
    try
    {
        summary = ResumeRun.Execute(
            previousRunDir,
            outDir,
            progress,
            stopAfterPointCancellation.Token,
            emergencyCancellation.Token);
    }
    finally
    {
        watcherCancellation.Cancel();
        WaitForWatcher(stopWatcher);
        WaitForWatcher(emergencyWatcher);
    }

    var resumeContinuityMetric = summary.LockinModel == "oe1300"
        ? $"effective_sample_hz_per_parameter={summary.EffectiveSampleHzPerParameter:0.###}"
        : $"delta_gt1={summary.PacketCounter?.DeltaGt1Count ?? 0}";
    Console.WriteLine($"resume-run done: previous_run={previousRunDir}, status={summary.Status}, points={summary.PointsPassed}/{summary.PointsTotal}, frames_ok={summary.FramesTotal}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, {resumeContinuityMetric}, out_dir={outDir}");
    return summary.LockinModel == "oe1300"
        ? summary.Status is "completed" or "completed_with_failed_points" or "paused" &&
            summary.TimeoutCount == 0 &&
            summary.RawLenBadCount == 0 &&
            (summary.DecodeFailures ?? 0) == 0 &&
            (summary.EffectiveSampleHzPerParameter ?? 0) >= 900 ? 0 : 2
        : summary.Status is "completed" or "completed_with_failed_points" or "paused" &&
            summary.TimeoutCount == 0 &&
            summary.RawLenBadCount == 0 &&
            (summary.PacketCounter?.DeltaGt1Count ?? 0) == 0 ? 0 : 2;
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
    var continuityMetric = report.LockinModel == "oe1300"
        ? $"effective_sample_hz_per_parameter={report.EffectiveSampleHzPerParameter:0.###}"
        : $"delta_gt1={report.DevicePacketCounter?.DeltaGt1Count ?? 0}";
    Console.WriteLine($"continuity audit done: verdict={report.Verdict}, frames={report.FramesTotal}, {continuityMetric}, out={outPath}");
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
    var inThreadProcessMode = GetOption(options, "in-thread-process-mode", "none").Trim().ToLowerInvariant();
    var writeRaw = GetBoolOption(options, "write-raw", true);
    var writeValues = GetBoolOption(options, "write-values", false);
    var previewFieldIndex = GetIntOption(options, "preview-field-index", Oe1022dProbeLayout.DefaultPreviewFieldIndex);

    if (durationSec <= 0)
    {
        return Fail("--duration-sec must be positive");
    }

    if (inThreadProcessMode is not ("none" or "measurement-means" or "field-decode-csv"))
    {
        return Fail("--in-thread-process-mode must be `none`, `measurement-means`, or `field-decode-csv`");
    }

    if (previewFieldIndex < 0 || previewFieldIndex >= Oe1022dProbeLayout.MeasurementFields.Length)
    {
        return Fail($"--preview-field-index must be between 0 and {Oe1022dProbeLayout.MeasurementFields.Length - 1}");
    }

    Directory.CreateDirectory(outDir);

    var rawDir = Path.Combine(outDir, "raw");
    var rawPath = Path.Combine(rawDir, "oe1022d.rall");
    var indexPath = Path.Combine(rawDir, "oe1022d.frames.idx.jsonl");
    var collectorFramesPath = Path.Combine(outDir, "collector_frames.jsonl");
    var segmentsPath = Path.Combine(outDir, "segments.jsonl");
    var parameterValuesPath = Path.Combine(outDir, "parameter_values.csv");
    var previewValuesPath = Path.Combine(outDir, "preview_values.csv");
    var summaryPath = Path.Combine(outDir, "summary.json");
    var previewField = Oe1022dProbeLayout.MeasurementFields[previewFieldIndex];

    var startedAt = UtcNowString();
    var processStart = Stopwatch.GetTimestamp();
    var deadline = processStart + durationSec * Stopwatch.Frequency;
    var nextRawOffset = 0L;
    var frameSeq = 0L;
    var stats = new ProbeStats();
    var packetAudit = new PacketCounterAudit();
    long processedFrames = 0;
    long processedFiniteValues = 0;
    long processedNonFiniteValues = 0;
    double processedValueSum = 0.0;
    long processingTotalTicks = 0;
    long globalSampleIndex = 0;
    string? firstFrameTs = null;
    string? lastFrameTs = null;
    ulong? firstFrameMonotonicNs = null;
    ulong? lastFrameMonotonicNs = null;
    byte? lastBRefSourceCode = null;
    byte? lastBRefSlopeCode = null;
    double? lastBRefCurrentFreqHz = null;
    byte? lastBInputOverload = null;
    byte? lastBGainOverload = null;
    byte? lastBPllLocked = null;

    using var oe = Oe1022dVisa.Open(resourceName, baudRate);
    if (writeRaw)
    {
        Directory.CreateDirectory(rawDir);
    }

    using var raw = writeRaw
        ? new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024)
        : null;
    using var index = writeRaw
        ? new StreamWriter(
            new FileStream(indexPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
            new UTF8Encoding(false))
        : null;
    using var collectorFrames = new StreamWriter(
        new FileStream(collectorFramesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024),
        new UTF8Encoding(false));
    using var parameterWriter = inThreadProcessMode == "field-decode-csv"
        ? new StreamWriter(new FileStream(parameterValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;
    using var previewWriter = inThreadProcessMode == "field-decode-csv" && writeValues
        ? new StreamWriter(new FileStream(previewValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;
    var payload = new byte[Oe1022dDefaults.RallFrameBytes];
    var fieldMeans = new double[Oe1022dProbeLayout.MeasurementFields.Length];
    var previewSeries = new double[Oe1022dProbeLayout.SamplesPerFrame];

    collectorFrames.WriteLine("{\"schema_version\":1,\"source\":\"oe1022d_probe\",\"frame_layout\":\"12288B_exact_read\",\"note\":\"probe-level frame index for direct-decode experiments\"}");

    if (parameterWriter is not null)
    {
        parameterWriter.Write("frame_seq,monotonic_ns,device_packet_counter,b_ref_source_code,b_ref_slope_code,b_ref_current_freq_hz,b_input_overload,b_gain_overload,b_pll_locked");
        foreach (var field in Oe1022dProbeLayout.MeasurementFields)
        {
            parameterWriter.Write(',');
            parameterWriter.Write(field.DisplayName);
        }
        parameterWriter.WriteLine();
    }

    if (previewWriter is not null)
    {
        previewWriter.WriteLine("frame_seq,sample_in_frame,global_sample_index,monotonic_ns,device_packet_counter,field_index,field_key,field_name,value");
    }

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

            if (inThreadProcessMode != "none")
            {
                var processingStart = Stopwatch.GetTimestamp();
                var processingSummary = ProcessOe1022dRawFrame(
                    payload,
                    inThreadProcessMode,
                    frameSeq,
                    monotonicNs,
                    payload[Oe1022dDefaults.DevicePacketCounterOffset],
                    previewFieldIndex,
                    fieldMeans,
                    previewSeries,
                    parameterWriter,
                    previewWriter,
                    ref globalSampleIndex,
                    out var statusSnapshot);
                processingTotalTicks += Stopwatch.GetTimestamp() - processingStart;
                processedFrames++;
                processedFiniteValues += processingSummary.FiniteValueCount;
                processedNonFiniteValues += processingSummary.NonFiniteValueCount;
                processedValueSum += processingSummary.ValueSum;
                lastBRefSourceCode = statusSnapshot?.BRefSourceCode;
                lastBRefSlopeCode = statusSnapshot?.BRefSlopeCode;
                lastBRefCurrentFreqHz = statusSnapshot?.BRefCurrentFreqHz;
                lastBInputOverload = statusSnapshot?.BInputOverload;
                lastBGainOverload = statusSnapshot?.BGainOverload;
                lastBPllLocked = statusSnapshot?.BPllLocked;
            }

            var ts = UtcNowString();
            raw?.Write(payload, 0, payload.Length);
            firstFrameTs ??= ts;
            firstFrameMonotonicNs ??= monotonicNs;
            lastFrameTs = ts;
            lastFrameMonotonicNs = monotonicNs;

            var counter = payload[Oe1022dDefaults.DevicePacketCounterOffset];
            packetAudit.Record(counter);
            if (index is not null)
            {
                RallArtifactWriter.WriteFrameIndexRecord(index, frameSeq, ts, monotonicNs, nextRawOffset, payload.Length, counter);
            }

            collectorFrames.Write("{\"frame_seq\":");
            collectorFrames.Write(frameSeq.ToString(CultureInfo.InvariantCulture));
            collectorFrames.Write(",\"ts\":\"");
            collectorFrames.Write(ts);
            collectorFrames.Write("\",\"monotonic_ns\":");
            collectorFrames.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
            collectorFrames.Write(",\"device_packet_counter\":");
            collectorFrames.Write(counter.ToString(CultureInfo.InvariantCulture));
            collectorFrames.Write(",\"in_thread_process_mode\":\"");
            collectorFrames.Write(inThreadProcessMode);
            collectorFrames.Write("\"}");
            collectorFrames.WriteLine();

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

    raw?.Flush(true);
    index?.Flush();
    collectorFrames.Flush();
    parameterWriter?.Flush();
    previewWriter?.Flush();

    var finishedAt = UtcNowString();
    if (writeRaw)
    {
        RallArtifactWriter.WriteWholeProbeSegment(
            segmentsPath,
            outDir,
            firstFrameTs ?? startedAt,
            lastFrameTs ?? finishedAt,
            firstFrameMonotonicNs ?? 0,
            lastFrameMonotonicNs ?? MonotonicNsSince(processStart),
            nextRawOffset,
            stats.FramesOk);
    }

    var elapsedMs = (long)(Stopwatch.GetElapsedTime(processStart).TotalMilliseconds);
    var rawBytesWritten = writeRaw && File.Exists(rawPath) ? new FileInfo(rawPath).Length : 0L;
    var processingTotalMs = processingTotalTicks * 1000.0 / Stopwatch.Frequency;
    var processingMeanUsPerFrame = processedFrames > 0
        ? processingTotalTicks * 1_000_000.0 / Stopwatch.Frequency / processedFrames
        : 0.0;
    var processedValueMean = processedFiniteValues > 0 ? processedValueSum / processedFiniteValues : 0.0;
    var summary = new
    {
        command = "oe-rall",
        resource = resourceName,
        baud_rate = baudRate,
        frame_bytes = Oe1022dDefaults.RallFrameBytes,
        post_write_delay_ms = Oe1022dDefaults.RallPostWriteDelayMs,
        visa_timeout_ms = Oe1022dDefaults.VisaTimeoutMs,
        duration_sec = durationSec,
        started_at = startedAt,
        finished_at = finishedAt,
        elapsed_ms = elapsedMs,
        read_attempts = stats.ReadAttempts,
        frames_ok = stats.FramesOk,
        read_errors = stats.ReadErrors,
        timeout_count = stats.TimeoutCount,
        raw_len_bad_count = stats.RawLenBadCount,
        raw_bytes_written = rawBytesWritten,
        raw_size_matches_frames_ok = writeRaw ? rawBytesWritten == stats.FramesOk * Oe1022dDefaults.RallFrameBytes : (bool?)null,
        write_raw = writeRaw,
        raw_path = writeRaw ? PathRelative(outDir, rawPath) : null,
        index_path = writeRaw ? PathRelative(outDir, indexPath) : null,
        segments_path = writeRaw ? PathRelative(outDir, segmentsPath) : null,
        collector_frames_path = PathRelative(outDir, collectorFramesPath),
        packet_counter = packetAudit.ToSummary(),
        in_thread_processing = new
        {
            mode = inThreadProcessMode,
            decode_mode = inThreadProcessMode == "field-decode-csv" ? Oe1022dProbeLayout.DecodeMode : null,
            write_values = writeValues,
            preview_field_index = inThreadProcessMode == "field-decode-csv" ? previewFieldIndex : (int?)null,
            preview_field_key = inThreadProcessMode == "field-decode-csv" ? previewField.Key : null,
            preview_field_name = inThreadProcessMode == "field-decode-csv" ? previewField.DisplayName : null,
            measurement_field_order = inThreadProcessMode == "field-decode-csv" ? Oe1022dProbeLayout.MeasurementFields.Select(static field => field.DisplayName).ToArray() : null,
            measurement_field_keys = inThreadProcessMode == "field-decode-csv" ? Oe1022dProbeLayout.MeasurementFields.Select(static field => field.Key).ToArray() : null,
            processed_frames = processedFrames,
            finite_value_count = processedFiniteValues,
            non_finite_value_count = processedNonFiniteValues,
            value_mean = processedFiniteValues > 0 && double.IsFinite(processedValueMean) ? processedValueMean : (double?)null,
            processing_total_ms = processingTotalMs,
            mean_processing_us_per_frame = processingMeanUsPerFrame,
            parameter_values_path = inThreadProcessMode == "field-decode-csv" ? PathRelative(outDir, parameterValuesPath) : null,
            preview_values_path = inThreadProcessMode == "field-decode-csv" && writeValues ? PathRelative(outDir, previewValuesPath) : null,
            last_b_ref_source_code = lastBRefSourceCode,
            last_b_ref_slope_code = lastBRefSlopeCode,
            last_b_ref_current_freq_hz = lastBRefCurrentFreqHz,
            last_b_input_overload = lastBInputOverload,
            last_b_gain_overload = lastBGainOverload,
            last_b_pll_locked = lastBPllLocked
        },
    };

    File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));

    Console.WriteLine($"oe-rall done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, delta_gt1={packetAudit.DeltaGt1Count}, process_mode={inThreadProcessMode}, out_dir={outDir}");
    return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && packetAudit.DeltaGt1Count == 0 ? 0 : 2;
}

static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dRawFrame(
    byte[] payload,
    string mode,
    long frameSeq,
    ulong monotonicNs,
    byte devicePacketCounter,
    int previewFieldIndex,
    double[] fieldMeans,
    double[] previewSeries,
    StreamWriter? parameterWriter,
    StreamWriter? previewWriter,
    ref long globalSampleIndex,
    out Oe1022dStatusSnapshot? statusSnapshot)
{
    statusSnapshot = null;

    return mode switch
    {
        "measurement-means" => ProcessOe1022dMeasurementMeans(payload),
        "field-decode-csv" => ProcessOe1022dFieldDecodeCsv(
            payload,
            frameSeq,
            monotonicNs,
            devicePacketCounter,
            previewFieldIndex,
            fieldMeans,
            previewSeries,
            parameterWriter,
            previewWriter,
            ref globalSampleIndex,
            out statusSnapshot),
        _ => (0, 0, 0.0)
    };
}

static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dMeasurementMeans(byte[] payload)
{
    const int measurementBytes = 8000;
    const int valueBytes = 8;

    long finiteValueCount = 0;
    long nonFiniteValueCount = 0;
    double valueSum = 0.0;
    var span = payload.AsSpan(0, measurementBytes);

    for (var offset = 0; offset < span.Length; offset += valueBytes)
    {
        var rawBits = BinaryPrimitives.ReadInt64LittleEndian(span.Slice(offset, valueBytes));
        var value = BitConverter.Int64BitsToDouble(rawBits);
        if (!double.IsFinite(value))
        {
            nonFiniteValueCount++;
            continue;
        }

        finiteValueCount++;
        valueSum += value;
    }

    return (finiteValueCount, nonFiniteValueCount, valueSum);
}

static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dFieldDecodeCsv(
    byte[] payload,
    long frameSeq,
    ulong monotonicNs,
    byte devicePacketCounter,
    int previewFieldIndex,
    double[] fieldMeans,
    double[] previewSeries,
    StreamWriter? parameterWriter,
    StreamWriter? previewWriter,
    ref long globalSampleIndex,
    out Oe1022dStatusSnapshot statusSnapshot)
{
    long finiteValueCount = 0;
    long nonFiniteValueCount = 0;
    double valueSum = 0.0;
    var span = payload.AsSpan();

    for (var fieldIndex = 0; fieldIndex < Oe1022dProbeLayout.MeasurementFields.Length; fieldIndex++)
    {
        var field = Oe1022dProbeLayout.MeasurementFields[fieldIndex];
        double fieldFiniteSum = 0.0;
        long fieldFiniteCount = 0;
        var fieldBaseOffset = field.Offset;

        for (var sampleIndex = 0; sampleIndex < Oe1022dProbeLayout.SamplesPerFrame; sampleIndex++)
        {
            var valueOffset = fieldBaseOffset + sampleIndex * Oe1022dProbeLayout.ValueBytes;
            var value = ReadDoubleBigEndian(span.Slice(valueOffset, Oe1022dProbeLayout.ValueBytes));

            if (fieldIndex == previewFieldIndex)
            {
                previewSeries[sampleIndex] = value;
            }

            if (!double.IsFinite(value))
            {
                nonFiniteValueCount++;
                continue;
            }

            finiteValueCount++;
            fieldFiniteCount++;
            fieldFiniteSum += value;
            valueSum += value;
        }

        fieldMeans[fieldIndex] = fieldFiniteCount > 0 ? fieldFiniteSum / fieldFiniteCount : double.NaN;
    }

    statusSnapshot = new Oe1022dStatusSnapshot(
        span[Oe1022dProbeLayout.BRefSourceCodeOffset],
        span[Oe1022dProbeLayout.BRefSlopeCodeOffset],
        ReadDoubleBigEndian(span.Slice(Oe1022dProbeLayout.BRefCurrentFreqHzOffset, Oe1022dProbeLayout.ValueBytes)),
        span[Oe1022dProbeLayout.BInputOverloadOffset],
        span[Oe1022dProbeLayout.BGainOverloadOffset],
        span[Oe1022dProbeLayout.BPllLockedOffset]);

    if (parameterWriter is not null)
    {
        parameterWriter.Write(frameSeq.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(devicePacketCounter.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BRefSourceCode.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BRefSlopeCode.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BRefCurrentFreqHz.ToString("R", CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BInputOverload.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BGainOverload.ToString(CultureInfo.InvariantCulture));
        parameterWriter.Write(',');
        parameterWriter.Write(statusSnapshot.BPllLocked.ToString(CultureInfo.InvariantCulture));
        for (var fieldIndex = 0; fieldIndex < fieldMeans.Length; fieldIndex++)
        {
            parameterWriter.Write(',');
            parameterWriter.Write(fieldMeans[fieldIndex].ToString("R", CultureInfo.InvariantCulture));
        }
        parameterWriter.WriteLine();
    }

    if (previewWriter is not null)
    {
        var previewField = Oe1022dProbeLayout.MeasurementFields[previewFieldIndex];
        for (var sampleIndex = 0; sampleIndex < previewSeries.Length; sampleIndex++)
        {
            previewWriter.Write(frameSeq.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(sampleIndex.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(globalSampleIndex.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(monotonicNs.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(devicePacketCounter.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(previewFieldIndex.ToString(CultureInfo.InvariantCulture));
            previewWriter.Write(',');
            previewWriter.Write(previewField.Key);
            previewWriter.Write(',');
            previewWriter.Write(previewField.DisplayName);
            previewWriter.Write(',');
            previewWriter.WriteLine(previewSeries[sampleIndex].ToString("R", CultureInfo.InvariantCulture));
            globalSampleIndex++;
        }
    }
    else
    {
        globalSampleIndex += previewSeries.Length;
    }

    return (finiteValueCount, nonFiniteValueCount, valueSum);
}

static double ReadDoubleBigEndian(ReadOnlySpan<byte> span)
{
    var rawBits = BinaryPrimitives.ReadInt64BigEndian(span);
    return BitConverter.Int64BitsToDouble(rawBits);
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
    var collectorBlocksPath = Path.Combine(outDir, "collector_blocks.jsonl");
    var parameterValuesPath = Path.Combine(outDir, "parameter_values.csv");
    var sampleValuesPath = Path.Combine(outDir, "sample_values.csv");
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
    string? lastStatusZoneSha256 = null;

    using var oe = Oe1300Tcp.Open(host, port);
    using var previewWriter = writeValues
        ? new StreamWriter(new FileStream(valuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false))
        : null;
    using var collectorBlocksWriter = new StreamWriter(new FileStream(collectorBlocksPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false));
    using var parameterWriter = new StreamWriter(new FileStream(parameterValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 256 * 1024), new UTF8Encoding(false));
    using var sampleWriter = new StreamWriter(new FileStream(sampleValuesPath, FileMode.Create, FileAccess.Write, FileShare.Read, bufferSize: 1024 * 1024), new UTF8Encoding(false));

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

    sampleWriter.Write("rall_index,sample_in_rall,global_sample_index,monotonic_ns,status_hex,status_byte,trig_count");
    foreach (var fieldName in Oe1300Defaults.SerialRallFieldNames)
    {
        sampleWriter.Write(',');
        sampleWriter.Write(fieldName);
    }
    sampleWriter.WriteLine();

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
            var statusZoneHex = Convert.ToHexString(payload, Oe1300Defaults.TcpRallPayloadBytes, payload.Length - Oe1300Defaults.TcpRallPayloadBytes).ToLowerInvariant();
            var statusZoneSha256 = Convert.ToHexString(SHA256.HashData(payload.AsSpan(Oe1300Defaults.TcpRallPayloadBytes, payload.Length - Oe1300Defaults.TcpRallPayloadBytes))).ToLowerInvariant();
            var payloadSha256 = Convert.ToHexString(SHA256.HashData(payload.AsSpan(0, payload.Length))).ToLowerInvariant();

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

            for (var sampleIndexInRall = 0; sampleIndexInRall < previewSeries.Length; sampleIndexInRall++)
            {
                if (previewWriter is not null)
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
                }

                sampleWriter.Write(decodedRallsOk.ToString(CultureInfo.InvariantCulture));
                sampleWriter.Write(',');
                sampleWriter.Write(sampleIndexInRall.ToString(CultureInfo.InvariantCulture));
                sampleWriter.Write(',');
                sampleWriter.Write(globalSampleIndex.ToString(CultureInfo.InvariantCulture));
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
                globalSampleIndex++;
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

            collectorBlocksWriter.WriteLine(JsonSerializer.Serialize(new
            {
                schema_version = 1,
                source = "oe1300_main",
                rall_index = decodedRallsOk,
                monotonic_ns = monotonicNs,
                sample_index_start = globalSampleIndex - previewSeries.Length,
                sample_index_end = globalSampleIndex,
                samples_per_parameter = previewSeries.Length,
                parameter_count = Oe1300Defaults.TcpRallLabviewParameterCount,
                status_hex = statusHex,
                status_byte = statusByte,
                trig_count = trigCount,
                payload_sha256 = payloadSha256,
                status_zone_sha256 = statusZoneSha256,
                status_zone_hex = statusZoneHex
            }, JsonOptions.Default));

            decodedSamplesPerParameter += previewSeries.Length;
            lastStatus = statusByte;
            lastStatusHex = statusHex;
            lastTrigCount = trigCount;
            lastStatusZoneSha256 = statusZoneSha256;
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
    collectorBlocksWriter.Flush();
    parameterWriter.Flush();
    sampleWriter.Flush();

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
        last_status_zone_sha256 = lastStatusZoneSha256,
        collector_blocks_path = PathRelative(outDir, collectorBlocksPath),
        parameter_values_path = PathRelative(outDir, parameterValuesPath),
        sample_values_path = PathRelative(outDir, sampleValuesPath),
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

static string ResolveSmbVisaResource()
{
    var candidates = new List<string>();
    AppendUnique(candidates, Smb100aDefaults.Resource);
    AppendUnique(candidates, Smb100aDefaults.FallbackResource);
    foreach (var candidate in Smb100aVisa.ListResources())
    {
        AppendUnique(candidates, candidate);
    }

    var failures = new List<string>();
    foreach (var candidate in candidates)
    {
        try
        {
            using var smb = Smb100aVisa.Open(candidate);
            var idn = smb.Query(Smb100aCommands.QueryIdn);
            if (idn.Contains(Smb100aDefaults.RequiredVendor, StringComparison.Ordinal) &&
                idn.Contains(Smb100aDefaults.RequiredModel, StringComparison.Ordinal))
            {
                return candidate;
            }

            failures.Add($"{candidate}: identity mismatch idn={idn}");
        }
        catch (Exception ex)
        {
            failures.Add($"{candidate}: {ex.Message}");
        }
    }

    throw new InvalidOperationException($"failed to resolve SMB100A VISA resource: {string.Join(" | ", failures)}");
}

static void AppendUnique(List<string> values, string? candidate)
{
    if (string.IsNullOrWhiteSpace(candidate))
    {
        return;
    }

    if (!values.Contains(candidate, StringComparer.OrdinalIgnoreCase))
    {
        values.Add(candidate);
    }
}

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
      Odmr.WinProbe oe-rall [--resource ASRL8::INSTR] [--baud 921600] [--in-thread-process-mode none|measurement-means|field-decode-csv] [--write-raw true|false] [--write-values true|false] [--preview-field-index 8] --duration-sec 300 --out-dir <dir>
      Odmr.WinProbe oe1300-idn --port <COMx> [--baud 115200]
      Odmr.WinProbe oe1300-rall --port <COMx> [--baud 115200] [--count 1] --out-dir <dir>
      Odmr.WinProbe oe1300-net-idn [--host 192.168.1.1] [--port 10001]
      Odmr.WinProbe oe1300-net-rall [--host 192.168.1.1] [--port 10001] [--count 1] --out-dir <dir>
      Odmr.WinProbe oe1300-net-labview-demo [--host 192.168.1.1] [--port 10001] [--post-write-delay-ms 5] [--drain-before-write true|false] [--preview-param-index 0] [--write-values true|false] --duration-sec 10 --out-dir <dir>
      Odmr.WinProbe smb-probe [--list-resources] [--resource USB::0x0AAD::0x0054::106789::INSTR]
      Odmr.WinProbe sweep-only-run [--resource ASRL8::INSTR] [--baud 921600] [--smb-resource USB::0x0AAD::0x0054::106789::INSTR] [--repeat 1] --out-dir <dir>
      Odmr.WinProbe minimal-3point-run [--resource ASRL8::INSTR] [--baud 921600] [--smb-resource USB::0x0AAD::0x0054::106789::INSTR] [--x COM4] [--y COM6] [--z COM3] [--cycles 1] [--laser-background] [--laser-port COM9] [--laser-power-mw 50] --out-dir <dir>
      Odmr.WinProbe run-resolve --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json>
      Odmr.WinProbe run-execute --station <json> --calibration <json> --plan <json> --smb-profile <json> --oe-profile <json> --laser-profile <json> --out-dir <dir> [--progress-jsonl <path>] [--stop-request-file <path>] [--emergency-stop-file <path>]
      Odmr.WinProbe resume-run --previous-run <dir> --out-dir <dir> [--progress-jsonl <path>] [--stop-request-file <path>] [--emergency-stop-file <path>]
      Odmr.WinProbe artifact-check --run <run-dir>
      Odmr.WinProbe audit-continuity --run <run-dir> --out <json>
      Odmr.WinProbe device-command-check
      Odmr.WinProbe live-replay --run <run-dir> [--tail-events 20]
      Odmr.WinProbe m8812-probe [--x COM4] [--y COM6] [--z COM3]
      Odmr.WinProbe laser-probe [--port COM9] --off-only
    """);
}

sealed record Oe1022dMeasurementField(string Key, string DisplayName, int Offset);

sealed record Oe1022dStatusSnapshot(
    byte BRefSourceCode,
    byte BRefSlopeCode,
    double BRefCurrentFreqHz,
    byte BInputOverload,
    byte BGainOverload,
    byte BPllLocked);

static class Oe1022dProbeLayout
{
    public const string DecodeMode = "measurement_fields_20x50_big_endian_v1";
    public const int SamplesPerFrame = 50;
    public const int ValueBytes = 8;
    public const int DefaultPreviewFieldIndex = 8;
    public const int BRefSourceCodeOffset = 8504;
    public const int BRefCurrentFreqHzOffset = 8505;
    public const int BRefSlopeCodeOffset = 8521;
    public const int BInputOverloadOffset = 8779;
    public const int BGainOverloadOffset = 8780;
    public const int BPllLockedOffset = 8781;

    public static readonly Oe1022dMeasurementField[] MeasurementFields =
    [
        new("a_x", "A-X", 0),
        new("a_y", "A-Y", 400),
        new("a_freq", "A-Freq", 800),
        new("a_noise", "A-Noise", 1200),
        new("a_xh1", "A-Xh1", 1600),
        new("a_yh1", "A-Yh1", 2000),
        new("a_xh2", "A-Xh2", 2400),
        new("a_yh2", "A-Yh2", 2800),
        new("b_x", "B-X", 3200),
        new("b_y", "B-Y", 3600),
        new("b_freq", "B-Freq", 4000),
        new("b_noise", "B-Noise", 4400),
        new("b_xh1", "B-Xh1", 4800),
        new("b_yh1", "B-Yh1", 5200),
        new("b_xh2", "B-Xh2", 5600),
        new("b_yh2", "B-Yh2", 6000),
        new("auxadc1", "AUXADC1", 6400),
        new("auxadc2", "AUXADC2", 6800),
        new("auxadc3", "AUXADC3", 7200),
        new("auxadc4", "AUXADC4", 7600)
    ];
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
                dwell_ms = value.DwellMs,
                lockin_model = value.LockinModel,
                collector_contract = value.CollectorContract,
                decode_failures = value.DecodeFailures,
                effective_sample_hz_per_parameter = value.EffectiveSampleHzPerParameter
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
