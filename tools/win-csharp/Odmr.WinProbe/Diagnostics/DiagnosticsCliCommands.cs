using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ivi.Visa;
using Odmr.Artifacts;
using Odmr.Devices;
using Odmr.Runtime;

namespace Odmr.WinProbe;

internal static class DiagnosticsCliCommands
{
    private const int Oe1022dDefaultPreviewFieldIndex = 8;

    internal static bool TryExecute(string command, IReadOnlyDictionary<string, string> options, out int exitCode)
    {
        switch (command)
        {
            case "visa-list":
                exitCode = VisaList();
                return true;
            case "oe-idn":
                exitCode = OeIdn(options);
                return true;
            case "oe-rall":
                exitCode = OeRall(options);
                return true;
            case "oe1300-idn":
                exitCode = Oe1300Idn(options);
                return true;
            case "oe1300-rall":
                exitCode = Oe1300Rall(options);
                return true;
            case "oe1300-net-idn":
                exitCode = Oe1300NetIdn(options);
                return true;
            case "oe1300-net-rall":
                exitCode = Oe1300NetRall(options);
                return true;
            case "oe1300-net-labview-demo":
                exitCode = Oe1300NetLabviewDemo(options);
                return true;
            case "smb-probe":
                exitCode = SmbProbe(options);
                return true;
            case "sweep-only-run":
                exitCode = SweepOnlyRunCommand(options);
                return true;
            case "minimal-3point-run":
                exitCode = Minimal3PointRunCommand(options);
                return true;
            case "m8812-probe":
                exitCode = M8812Probe(options);
                return true;
            case "laser-probe":
                exitCode = LaserProbe(options);
                return true;
            default:
                exitCode = 0;
                return false;
        }
    }

    private static int LaserProbe(IReadOnlyDictionary<string, string> options)
    {
        var port = CliSupport.GetOption(options, "port", CniLaserDefaults.Port);
        if (!CliSupport.GetBoolOption(options, "off-only", defaultValue: false))
        {
            return CliSupport.Fail("laser-probe requires --off-only");
        }

        var result = CniLaserSerial.ProbeOffOnly(port);
        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

        return result.EchoMatched ? 0 : CliSupport.Fail("CNI laser output_off echo mismatch");
    }

    private static int M8812Probe(IReadOnlyDictionary<string, string> options)
    {
        var xPort = CliSupport.GetOption(options, "x", M8812Defaults.XPort);
        var yPort = CliSupport.GetOption(options, "y", M8812Defaults.YPort);
        var zPort = CliSupport.GetOption(options, "z", M8812Defaults.ZPort);
        var result = M8812Serial.Probe(xPort, yPort, zPort);

        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

        if (!result.AllIdentityMatched)
        {
            return CliSupport.Fail("M8812 identity mismatch");
        }

        return result.CleanupOk ? 0 : CliSupport.Fail("M8812 cleanup failed");
    }

    private static int VisaList()
    {
        foreach (var resource in Oe1022dVisa.ListResources())
        {
            Console.WriteLine(resource);
        }

        return 0;
    }

    private static int OeIdn(IReadOnlyDictionary<string, string> options)
    {
        var resourceName = CliSupport.GetOption(options, "resource", Oe1022dDefaults.Resource);
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);

        using var oe = Oe1022dVisa.Open(resourceName, baudRate);
        var idn = oe.QueryIdn();

        Console.WriteLine(idn);

        if (!idn.Contains(Oe1022dDefaults.RequiredIdnPrefix, StringComparison.Ordinal) ||
            !idn.Contains(Oe1022dDefaults.RequiredSerial, StringComparison.Ordinal))
        {
            return CliSupport.Fail($"OE1022D identity mismatch: expected `{Oe1022dDefaults.RequiredIdnPrefix}` and `{Oe1022dDefaults.RequiredSerial}`");
        }

        return 0;
    }

    private static int Oe1300Idn(IReadOnlyDictionary<string, string> options)
    {
        var portName = CliSupport.GetRequiredOption(options, "port");
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1300Defaults.SerialBaudRate);

        using var oe = Oe1300Serial.Open(portName, baudRate);
        var idn = oe.QueryIdn();
        Console.WriteLine(idn);

        return idn.Contains(Oe1300Defaults.RequiredIdnPrefix, StringComparison.Ordinal)
            ? 0
            : CliSupport.Fail($"OE1300 identity mismatch: expected prefix `{Oe1300Defaults.RequiredIdnPrefix}`");
    }

    private static int Oe1300NetIdn(IReadOnlyDictionary<string, string> options)
    {
        var host = CliSupport.GetOption(options, "host", Oe1300Defaults.Host);
        var port = CliSupport.GetIntOption(options, "port", Oe1300Defaults.TcpPort);

        using var oe = Oe1300Tcp.Open(host, port);
        var idn = oe.QueryIdn();
        Console.WriteLine(idn);

        return idn.Contains(Oe1300Defaults.RequiredIdnPrefix, StringComparison.Ordinal)
            ? 0
            : CliSupport.Fail($"OE1300 TCP identity mismatch: expected prefix `{Oe1300Defaults.RequiredIdnPrefix}`");
    }

    private static int SmbProbe(IReadOnlyDictionary<string, string> options)
    {
        if (CliSupport.GetBoolOption(options, "list-resources", false))
        {
            foreach (var visaResource in Smb100aVisa.ListResources())
            {
                Console.WriteLine(visaResource);
            }

            return 0;
        }

        var resource = CliSupport.GetOptionalOption(options, "resource") ?? CliSupport.ResolveSmbVisaResource();
        var result = Smb100aVisa.Probe(resource);

        Console.WriteLine(JsonSerializer.Serialize(result, JsonOptions.Pretty));

        if (!result.IdentityMatched)
        {
            return CliSupport.Fail($"SMB100A identity mismatch: expected `{Smb100aDefaults.RequiredVendor}` and `{Smb100aDefaults.RequiredModel}`");
        }

        return result.ErrorQueueClean ? 0 : CliSupport.Fail($"SMB100A error queue is not clean: {result.SystemError}");
    }

    private static int SweepOnlyRunCommand(IReadOnlyDictionary<string, string> options)
    {
        var resourceName = CliSupport.GetOption(options, "resource", Oe1022dDefaults.Resource);
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
        var smbResource = CliSupport.GetOptionalOption(options, "smb-resource") ?? CliSupport.ResolveSmbVisaResource();
        var repeat = CliSupport.GetIntOption(options, "repeat", 1);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");

        var summary = SweepOnlyRun.Execute(new SweepOnlyRunOptions(resourceName, baudRate, smbResource, repeat, outDir));
        Console.WriteLine($"sweep-only-run done: repeat={summary.RepeatCount}, frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
        return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
    }

    private static int Minimal3PointRunCommand(IReadOnlyDictionary<string, string> options)
    {
        var resourceName = CliSupport.GetOption(options, "resource", Oe1022dDefaults.Resource);
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
        var smbResource = CliSupport.GetOptionalOption(options, "smb-resource") ?? CliSupport.ResolveSmbVisaResource();
        var xPort = CliSupport.GetOption(options, "x", M8812Defaults.XPort);
        var yPort = CliSupport.GetOption(options, "y", M8812Defaults.YPort);
        var zPort = CliSupport.GetOption(options, "z", M8812Defaults.ZPort);
        var cycles = CliSupport.GetIntOption(options, "cycles", 1);
        var enableLaser = CliSupport.GetBoolOption(options, "laser-background", false);
        var laserPort = CliSupport.GetOption(options, "laser-port", CniLaserDefaults.Port);
        var laserPowerMw = CliSupport.GetIntOption(options, "laser-power-mw", 50);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");

        var summary = Minimal3PointRun.Execute(new Minimal3PointRunOptions(resourceName, baudRate, smbResource, xPort, yPort, zPort, cycles, enableLaser, laserPort, laserPowerMw, outDir));
        Console.WriteLine($"minimal-3point-run done: points={summary.PointCount}, cycles={summary.Cycles}, laser={summary.LaserEnabled}, frames_ok={summary.FramesOk}, timeouts={summary.TimeoutCount}, raw_len_bad={summary.RawLenBadCount}, delta_gt1={summary.PacketCounter.DeltaGt1Count}, out_dir={outDir}");
        return summary.TimeoutCount == 0 && summary.RawLenBadCount == 0 && summary.PacketCounter.DeltaGt1Count == 0 ? 0 : 2;
    }

    private static int OeRall(IReadOnlyDictionary<string, string> options)
    {
        var resourceName = CliSupport.GetOption(options, "resource", Oe1022dDefaults.Resource);
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1022dDefaults.BaudRate);
        var durationSec = CliSupport.GetIntOption(options, "duration-sec", 300);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");
        var inThreadProcessMode = CliSupport.GetOption(options, "in-thread-process-mode", "none").Trim().ToLowerInvariant();
        var writeRaw = CliSupport.GetBoolOption(options, "write-raw", true);
        var writeValues = CliSupport.GetBoolOption(options, "write-values", false);
        var previewFieldIndex = CliSupport.GetIntOption(options, "preview-field-index", Oe1022dDefaultPreviewFieldIndex);

        if (durationSec <= 0)
        {
            return CliSupport.Fail("--duration-sec must be positive");
        }

        if (inThreadProcessMode is not ("none" or "measurement-means" or "field-decode-csv"))
        {
            return CliSupport.Fail("--in-thread-process-mode must be `none`, `measurement-means`, or `field-decode-csv`");
        }

        if (previewFieldIndex < 0 || previewFieldIndex >= Oe1022dDirectDecode.MeasurementFields.Length)
        {
            return CliSupport.Fail($"--preview-field-index must be between 0 and {Oe1022dDirectDecode.MeasurementFields.Length - 1}");
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
        var previewField = Oe1022dDirectDecode.MeasurementFields[previewFieldIndex];

        var startedAt = CliSupport.UtcNowString();
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
        var fieldMeans = new double[Oe1022dDirectDecode.MeasurementFields.Length];
        var previewSeries = new double[Oe1022dDirectDecode.SamplesPerFrame];

        collectorFrames.WriteLine("{\"schema_version\":1,\"source\":\"oe1022d_probe\",\"frame_layout\":\"12288B_exact_read\",\"note\":\"probe-level frame index for direct-decode experiments\"}");

        if (parameterWriter is not null)
        {
            parameterWriter.Write("frame_seq,monotonic_ns,device_packet_counter,b_ref_source_code,b_ref_slope_code,b_ref_current_freq_hz,b_input_overload,b_gain_overload,b_pll_locked");
            foreach (var field in Oe1022dDirectDecode.MeasurementFields)
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

                var monotonicNs = CliSupport.MonotonicNsSince(processStart);

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

                var ts = CliSupport.UtcNowString();
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

        var finishedAt = CliSupport.UtcNowString();
        if (writeRaw)
        {
            RallArtifactWriter.WriteWholeProbeSegment(
                segmentsPath,
                outDir,
                firstFrameTs ?? startedAt,
                lastFrameTs ?? finishedAt,
                firstFrameMonotonicNs ?? 0,
                lastFrameMonotonicNs ?? CliSupport.MonotonicNsSince(processStart),
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
            raw_path = writeRaw ? CliSupport.PathRelative(outDir, rawPath) : null,
            index_path = writeRaw ? CliSupport.PathRelative(outDir, indexPath) : null,
            segments_path = writeRaw ? CliSupport.PathRelative(outDir, segmentsPath) : null,
            collector_frames_path = CliSupport.PathRelative(outDir, collectorFramesPath),
            packet_counter = packetAudit.ToSummary(),
            in_thread_processing = new
            {
                mode = inThreadProcessMode,
                decode_mode = inThreadProcessMode == "field-decode-csv" ? Oe1022dDirectDecode.DecodeMode : null,
                write_values = writeValues,
                preview_field_index = inThreadProcessMode == "field-decode-csv" ? previewFieldIndex : (int?)null,
                preview_field_key = inThreadProcessMode == "field-decode-csv" ? previewField.Key : null,
                preview_field_name = inThreadProcessMode == "field-decode-csv" ? previewField.DisplayName : null,
                measurement_field_order = inThreadProcessMode == "field-decode-csv" ? Oe1022dDirectDecode.MeasurementFields.Select(static field => field.DisplayName).ToArray() : null,
                measurement_field_keys = inThreadProcessMode == "field-decode-csv" ? Oe1022dDirectDecode.MeasurementFields.Select(static field => field.Key).ToArray() : null,
                processed_frames = processedFrames,
                finite_value_count = processedFiniteValues,
                non_finite_value_count = processedNonFiniteValues,
                value_mean = processedFiniteValues > 0 && double.IsFinite(processedValueMean) ? processedValueMean : (double?)null,
                processing_total_ms = processingTotalMs,
                mean_processing_us_per_frame = processingMeanUsPerFrame,
                parameter_values_path = inThreadProcessMode == "field-decode-csv" ? CliSupport.PathRelative(outDir, parameterValuesPath) : null,
                preview_values_path = inThreadProcessMode == "field-decode-csv" && writeValues ? CliSupport.PathRelative(outDir, previewValuesPath) : null,
                last_b_ref_source_code = lastBRefSourceCode,
                last_b_ref_slope_code = lastBRefSlopeCode,
                last_b_ref_current_freq_hz = lastBRefCurrentFreqHz,
                last_b_input_overload = lastBInputOverload,
                last_b_gain_overload = lastBGainOverload,
                last_b_pll_locked = lastBPllLocked
            }
        };

        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));

        Console.WriteLine($"oe-rall done: frames_ok={stats.FramesOk}, timeouts={stats.TimeoutCount}, raw_len_bad={stats.RawLenBadCount}, delta_gt1={packetAudit.DeltaGt1Count}, process_mode={inThreadProcessMode}, out_dir={outDir}");
        return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && packetAudit.DeltaGt1Count == 0 ? 0 : 2;
    }

    private static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dRawFrame(
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

    private static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dMeasurementMeans(byte[] payload)
    {
        var measurementBytes = Oe1022dDirectDecode.MeasurementFields.Length *
                               Oe1022dDirectDecode.SamplesPerFrame *
                               Oe1022dDirectDecode.ValueBytes;

        long finiteValueCount = 0;
        long nonFiniteValueCount = 0;
        double valueSum = 0.0;
        var span = payload.AsSpan(0, measurementBytes);

        for (var offset = 0; offset < span.Length; offset += Oe1022dDirectDecode.ValueBytes)
        {
            var rawBits = BinaryPrimitives.ReadInt64LittleEndian(span.Slice(offset, Oe1022dDirectDecode.ValueBytes));
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

    private static (long FiniteValueCount, long NonFiniteValueCount, double ValueSum) ProcessOe1022dFieldDecodeCsv(
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

        for (var fieldIndex = 0; fieldIndex < Oe1022dDirectDecode.MeasurementFields.Length; fieldIndex++)
        {
            var fieldFiniteSum = 0.0;
            long fieldFiniteCount = 0;

            for (var sampleIndex = 0; sampleIndex < Oe1022dDirectDecode.SamplesPerFrame; sampleIndex++)
            {
                var value = Oe1022dDirectDecode.ReadMeasurementValue(payload, fieldIndex, sampleIndex);

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

        statusSnapshot = Oe1022dDirectDecode.ReadStatus(payload);

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
            var previewField = Oe1022dDirectDecode.MeasurementFields[previewFieldIndex];
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

    private static int Oe1300Rall(IReadOnlyDictionary<string, string> options)
    {
        var portName = CliSupport.GetRequiredOption(options, "port");
        var baudRate = CliSupport.GetIntOption(options, "baud", Oe1300Defaults.SerialBaudRate);
        var count = CliSupport.GetIntOption(options, "count", 1);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");

        if (count <= 0)
        {
            return CliSupport.Fail("--count must be positive");
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
                raw_path = CliSupport.PathRelative(outDir, rawPath),
                json_path = CliSupport.PathRelative(outDir, jsonPath),
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
            started_at = CliSupport.UtcNowString(),
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

    private static int Oe1300NetRall(IReadOnlyDictionary<string, string> options)
    {
        var host = CliSupport.GetOption(options, "host", Oe1300Defaults.Host);
        var port = CliSupport.GetIntOption(options, "port", Oe1300Defaults.TcpPort);
        var count = CliSupport.GetIntOption(options, "count", 1);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");

        if (count <= 0)
        {
            return CliSupport.Fail("--count must be positive");
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
                raw_path = CliSupport.PathRelative(outDir, rawPath),
                json_path = jsonPath is null ? null : CliSupport.PathRelative(outDir, jsonPath),
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
            started_at = CliSupport.UtcNowString(),
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

    private static int Oe1300NetLabviewDemo(IReadOnlyDictionary<string, string> options)
    {
        var host = CliSupport.GetOption(options, "host", Oe1300Defaults.Host);
        var port = CliSupport.GetIntOption(options, "port", Oe1300Defaults.TcpPort);
        var durationSec = CliSupport.GetIntOption(options, "duration-sec", 0);
        var outDir = CliSupport.GetRequiredOption(options, "out-dir");
        var postWriteDelayMs = CliSupport.GetIntOption(options, "post-write-delay-ms", 5);
        var drainBeforeWrite = CliSupport.GetBoolOption(options, "drain-before-write", false);
        var writeValues = CliSupport.GetBoolOption(options, "write-values", false);
        var previewParamIndex = CliSupport.GetIntOption(options, "preview-param-index", 0);
        var csvWriteMode = CliSupport.GetOption(options, "csv-write-mode", "all").Trim().ToLowerInvariant();

        if (durationSec <= 0)
        {
            return CliSupport.Fail("--duration-sec must be positive");
        }

        if (previewParamIndex < 0 || previewParamIndex >= Oe1300Defaults.TcpRallLabviewParameterCount)
        {
            return CliSupport.Fail($"--preview-param-index must be between 0 and {Oe1300Defaults.TcpRallLabviewParameterCount - 1}");
        }

        if (csvWriteMode is not ("all" or "unique-only"))
        {
            return CliSupport.Fail("--csv-write-mode must be one of: all, unique-only");
        }

        Directory.CreateDirectory(outDir);
        var valuesPath = Path.Combine(outDir, "preview_values.csv");
        var collectorBlocksPath = Path.Combine(outDir, "collector_blocks.jsonl");
        var parameterValuesPath = Path.Combine(outDir, "parameter_values.csv");
        var sampleValuesPath = Path.Combine(outDir, "sample_values.csv");
        var summaryPath = Path.Combine(outDir, "summary.json");
        var startedAt = CliSupport.UtcNowString();
        var processStart = Stopwatch.GetTimestamp();
        var deadline = processStart + durationSec * Stopwatch.Frequency;
        var payload = new byte[Oe1300Defaults.TcpRallExpectedBytes];
        var previewParamName = Oe1300Defaults.SerialRallFieldNames[previewParamIndex];

        var stats = new ProbeStats();
        long decodedRallsOk = 0;
        long decodeFailures = 0;
        long decodedSamplesPerParameter = 0;
        long globalSampleIndex = 0;
        long uniqueBlocks = 0;
        long duplicateBlocks = 0;
        long writtenRalls = 0;
        long writtenSamplesPerParameter = 0;
        long previewFiniteCount = 0;
        long previewNonFiniteCount = 0;
        double previewMin = double.PositiveInfinity;
        double previewMax = double.NegativeInfinity;
        double previewSum = 0.0;
        byte? lastStatus = null;
        string? lastStatusHex = null;
        byte? lastTrigCount = null;
        string? previousPayloadSha256 = null;
        var writeUniqueOnly = string.Equals(csvWriteMode, "unique-only", StringComparison.Ordinal);

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

                var monotonicNs = CliSupport.MonotonicNsSince(processStart);
                var statusHex = Convert.ToHexString(payload, Oe1300Defaults.TcpRallStatusOffset, Oe1300Defaults.TcpRallStatusByteCount).ToLowerInvariant();
                var statusByte = payload[Oe1300Defaults.TcpRallStatusOffset];
                var trigCount = payload[Oe1300Defaults.TcpRallTrigCountOffset];
                var payloadSha256 = Convert.ToHexString(SHA256.HashData(payload.AsSpan(0, payload.Length))).ToLowerInvariant();
                var uniqueBlock = !string.Equals(previousPayloadSha256, payloadSha256, StringComparison.Ordinal);
                if (uniqueBlock)
                {
                    uniqueBlocks++;
                }
                else
                {
                    duplicateBlocks++;
                }

                var sampleIndexStart = globalSampleIndex;
                var writeCurrentBlock = !writeUniqueOnly || uniqueBlock;
                if (writeCurrentBlock)
                {
                    var namedSeries = Oe1300Parsers.DecodeTcpRallLabviewNamedSeries(payload);
                    var previewSeries = namedSeries[previewParamName];

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

                    decodedSamplesPerParameter += previewSeries.Length;
                    writtenSamplesPerParameter += previewSeries.Length;
                    writtenRalls++;
                }

                collectorBlocksWriter.WriteLine(JsonSerializer.Serialize(new
                {
                    schema_version = 1,
                    source = "oe1300_main",
                    rall_index = decodedRallsOk,
                    ts = CliSupport.UtcNowString(),
                    monotonic_ns = monotonicNs,
                    sample_index_start = sampleIndexStart,
                    sample_index_end = globalSampleIndex,
                    unique_block = uniqueBlock,
                    unique_block_index = uniqueBlock ? uniqueBlocks - 1 : Math.Max(0, uniqueBlocks - 1)
                }, JsonOptions.Default));

                lastStatus = statusByte;
                lastStatusHex = statusHex;
                lastTrigCount = trigCount;
                previousPayloadSha256 = payloadSha256;
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
        var uniqueBlockHz = elapsedMs > 0 ? uniqueBlocks / (elapsedMs / 1000.0) : 0.0;
        var writtenBlockHz = elapsedMs > 0 ? writtenRalls / (elapsedMs / 1000.0) : 0.0;
        var queryBasedPerParameterHz = queryHz * Oe1300Defaults.TcpRallLabviewSamplesPerParameter;
        var effectivePerParameterHz = uniqueBlockHz * Oe1300Defaults.TcpRallLabviewSamplesPerParameter;
        var writtenPerParameterHz = writtenBlockHz * Oe1300Defaults.TcpRallLabviewSamplesPerParameter;
        var effectiveTotalScalarHz = effectivePerParameterHz * Oe1300Defaults.TcpRallLabviewParameterCount;
        var previewMean = previewFiniteCount > 0 ? previewSum / previewFiniteCount : 0.0;
        var collectorBlocksBytes = new FileInfo(collectorBlocksPath).Length;
        var parameterValuesBytes = new FileInfo(parameterValuesPath).Length;
        var sampleValuesBytes = new FileInfo(sampleValuesPath).Length;
        var previewValuesBytes = writeValues && File.Exists(valuesPath) ? new FileInfo(valuesPath).Length : 0L;

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
            csv_write_mode = csvWriteMode,
            preview_param_index = previewParamIndex,
            preview_param_name = previewParamName,
            decode_mode = "labview_37_parameters_x_100_samples_big_endian_v2",
            rall_payload_bytes = Oe1300Defaults.TcpRallPayloadBytes,
            rall_frame_bytes = Oe1300Defaults.TcpRallFrameBytes,
            labview_parameter_count = Oe1300Defaults.TcpRallLabviewParameterCount,
            labview_frames_per_parameter = Oe1300Defaults.TcpRallLabviewFramesPerParameter,
            labview_samples_per_parameter = Oe1300Defaults.TcpRallLabviewSamplesPerParameter,
            started_at = startedAt,
            finished_at = CliSupport.UtcNowString(),
            elapsed_ms = elapsedMs,
            read_attempts = stats.ReadAttempts,
            ralls_ok = decodedRallsOk,
            read_errors = stats.ReadErrors,
            timeout_count = stats.TimeoutCount,
            raw_len_bad_count = stats.RawLenBadCount,
            decode_failures = decodeFailures,
            query_hz = queryHz,
            unique_block_hz = uniqueBlockHz,
            written_block_hz = writtenBlockHz,
            unique_blocks = uniqueBlocks,
            duplicate_blocks = duplicateBlocks,
            written_ralls = writtenRalls,
            decoded_samples_per_parameter_total = decodedSamplesPerParameter,
            written_samples_per_parameter_total = writtenSamplesPerParameter,
            query_based_sample_hz_per_parameter = queryBasedPerParameterHz,
            effective_sample_hz_per_parameter = effectivePerParameterHz,
            written_sample_hz_per_parameter = writtenPerParameterHz,
            effective_total_scalar_hz = effectiveTotalScalarHz,
            preview_finite_count = previewFiniteCount,
            preview_non_finite_count = previewNonFiniteCount,
            preview_value_min = previewFiniteCount > 0 && double.IsFinite(previewMin) ? previewMin : (double?)null,
            preview_value_max = previewFiniteCount > 0 && double.IsFinite(previewMax) ? previewMax : (double?)null,
            preview_value_mean = previewFiniteCount > 0 && double.IsFinite(previewMean) ? previewMean : (double?)null,
            last_status_hex = lastStatusHex,
            last_status_byte = lastStatus,
            last_trig_count = lastTrigCount,
            collector_blocks_bytes = collectorBlocksBytes,
            parameter_values_bytes = parameterValuesBytes,
            sample_values_bytes = sampleValuesBytes,
            preview_values_bytes = writeValues ? previewValuesBytes : (long?)null,
            collector_blocks_path = CliSupport.PathRelative(outDir, collectorBlocksPath),
            parameter_values_path = CliSupport.PathRelative(outDir, parameterValuesPath),
            sample_values_path = CliSupport.PathRelative(outDir, sampleValuesPath),
            values_path = writeValues ? CliSupport.PathRelative(outDir, valuesPath) : null
        };

        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
        Console.WriteLine($"oe1300-net-labview-demo done: ralls_ok={decodedRallsOk}, unique_blocks={uniqueBlocks}, query_hz={queryHz:0.###}, effective_sample_hz_per_parameter={effectivePerParameterHz:0.###}, csv_write_mode={csvWriteMode}, out_dir={outDir}");
        return stats.TimeoutCount == 0 && stats.RawLenBadCount == 0 && decodeFailures == 0 ? 0 : 2;
    }
}
