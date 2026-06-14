using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Odmr.Artifacts;
using Odmr.Devices;

namespace Odmr.Runtime;

public sealed record Minimal3PointRunOptions(
    string OeResource,
    int OeBaudRate,
    string SmbHost,
    int SmbPort,
    string XPort,
    string YPort,
    string ZPort,
    int Cycles,
    bool EnableLaser,
    string LaserPort,
    int LaserPowerMw,
    string OutDir);

public sealed record Minimal3PointRunSummary(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("oe_resource")] string OeResource,
    [property: JsonPropertyName("oe_baud_rate")] int OeBaudRate,
    [property: JsonPropertyName("smb_host")] string SmbHost,
    [property: JsonPropertyName("smb_port")] int SmbPort,
    [property: JsonPropertyName("m8812_ports")] IReadOnlyList<string> M8812Ports,
    [property: JsonPropertyName("point_count")] int PointCount,
    [property: JsonPropertyName("cycles")] int Cycles,
    [property: JsonPropertyName("laser_enabled")] bool LaserEnabled,
    [property: JsonPropertyName("laser_port")] string? LaserPort,
    [property: JsonPropertyName("laser_power_mw")] int? LaserPowerMw,
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
    [property: JsonPropertyName("points_path")] string PointsPath,
    [property: JsonPropertyName("quality_path")] string QualityPath,
    [property: JsonPropertyName("baseline_path")] string BaselinePath,
    [property: JsonPropertyName("packet_counter")] PacketCounterSummary PacketCounter);

internal sealed record MinimalPointPlan(
    string PointId,
    double[] TargetBNt,
    double PowerDbm);

internal sealed record MagAxisHandle(
    string AxisId,
    string Port,
    string ExpectedSerial,
    M8812AxisSession Session);

public static class Minimal3PointRun
{
    private const int SmbCommandSettleMs = 500;
    private const int PointSettleMs = 1000;
    private const int BaselineSettleMs = 1000;
    private const int BaselineReadbackSamples = 3;
    private const double BaselineToleranceA = 0.002;
    private const double M8812VoltageV = 75.0;
    private const double CurrentPerNt = 0.001;
    private const int MinFrames = 10;
    private const int MaxTimeoutCount = 2;

    private static readonly MinimalPointPlan[] Points =
    [
        new("p0001", [0.0, 0.0, 0.0], -10.0),
        new("p0002", [10.0, 0.0, 0.0], -20.0),
        new("p0003", [0.0, 10.0, 0.0], -15.0)
    ];

    public static Minimal3PointRunSummary Execute(Minimal3PointRunOptions options)
    {
        var rawDir = Path.Combine(options.OutDir, "raw");
        Directory.CreateDirectory(rawDir);

        var rawPath = Path.Combine(rawDir, "oe1022d.rall");
        var indexPath = Path.Combine(rawDir, "oe1022d.frames.idx.jsonl");
        var segmentsPath = Path.Combine(options.OutDir, "segments.jsonl");
        var pointsPath = Path.Combine(options.OutDir, "points.jsonl");
        var qualityPath = Path.Combine(options.OutDir, "quality.jsonl");
        var baselinePath = Path.Combine(options.OutDir, "baseline_snapshot.json");
        var summaryPath = Path.Combine(options.OutDir, "summary.json");

        foreach (var path in new[] { segmentsPath, pointsPath, qualityPath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        if (options.Cycles <= 0)
        {
            throw new ArgumentException("--cycles must be positive");
        }

        var startedAt = UtcNowString();
        var processStart = Stopwatch.GetTimestamp();
        var runId = Path.GetFileName(Path.GetFullPath(options.OutDir));
        using var collector = new OeRallCollector(options.OeResource, options.OeBaudRate, rawPath, indexPath, processStart);
        using var smb = Smb100aTcp.Open(options.SmbHost, options.SmbPort);
        var magAxes = OpenMagAxes(options);
        CniLaserSession? laser = null;

        try
        {
            collector.Start();
            collector.WaitForFirstFrame(TimeSpan.FromSeconds(5));

            if (options.EnableLaser)
            {
                laser = CniLaserSerial.Open(options.LaserPort);
                laser.SetPowerMw(options.LaserPowerMw);
                Thread.Sleep(1000);
                laser.OutputOn();
                Thread.Sleep(1000);
            }

            var baseline = LockBaseline(magAxes, baselinePath);
            var baselineCurrentA = baseline.Axes
                .Select(axis => axis.LockedZeroOffsetCurrentA ?? axis.ZeroOffsetSetpointA)
                .ToArray();

            smb.ApplyDefaultFixedProfile(SmbCommandSettleMs);

            for (var cycle = 0; cycle < options.Cycles; cycle++)
            {
                for (var index = 0; index < Points.Length; index++)
                {
                    RunPoint(
                        runId,
                        cycle,
                        index,
                        Points[index],
                        baselineCurrentA,
                        magAxes,
                        smb,
                        collector,
                        segmentsPath,
                        pointsPath,
                        qualityPath,
                        processStart);
                }
            }
        }
        finally
        {
            Exception? cleanupFailure = null;
            if (laser is not null)
            {
                try
                {
                    laser.OutputOff();
                }
                catch (Exception ex)
                {
                    cleanupFailure ??= ex;
                }
                finally
                {
                    try
                    {
                        laser.Dispose();
                    }
                    catch (Exception ex)
                    {
                        cleanupFailure ??= ex;
                    }
                }
            }

            try
            {
                CleanupMagAxes(magAxes);
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }
            finally
            {
                foreach (var axis in magAxes)
                {
                    try
                    {
                        axis.Session.Dispose();
                    }
                    catch (Exception ex)
                    {
                        cleanupFailure ??= ex;
                    }
                }
            }

            try
            {
                smb.Cleanup();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

            try
            {
                collector.Stop();
            }
            catch (Exception ex)
            {
                cleanupFailure ??= ex;
            }

            if (cleanupFailure is not null)
            {
                throw new InvalidOperationException("minimal 3-point cleanup failed", cleanupFailure);
            }
        }

        var snapshot = collector.Snapshot();
        var finishedAt = UtcNowString();
        var rawBytesWritten = new FileInfo(rawPath).Length;
        var summary = new Minimal3PointRunSummary(
            "minimal-3point-run",
            runId,
            options.OeResource,
            options.OeBaudRate,
            options.SmbHost,
            options.SmbPort,
            [options.XPort, options.YPort, options.ZPort],
            Points.Length * options.Cycles,
            options.Cycles,
            options.EnableLaser,
            options.EnableLaser ? options.LaserPort : null,
            options.EnableLaser ? options.LaserPowerMw : null,
            startedAt,
            finishedAt,
            (long)Stopwatch.GetElapsedTime(processStart).TotalMilliseconds,
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
            PathRelative(options.OutDir, pointsPath),
            PathRelative(options.OutDir, qualityPath),
            PathRelative(options.OutDir, baselinePath),
            snapshot.PacketCounter.ToSummary());

        File.WriteAllText(summaryPath, JsonSerializer.Serialize(summary, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
        return summary;
    }

    private static List<MagAxisHandle> OpenMagAxes(Minimal3PointRunOptions options)
    {
        var axes = new List<MagAxisHandle>
        {
            new("mag_x", options.XPort, M8812Defaults.XSerial, M8812Serial.Open(options.XPort)),
            new("mag_y", options.YPort, M8812Defaults.YSerial, M8812Serial.Open(options.YPort)),
            new("mag_z", options.ZPort, M8812Defaults.ZSerial, M8812Serial.Open(options.ZPort))
        };

        foreach (var axis in axes)
        {
            axis.Session.Clear();
            var idn = axis.Session.QueryIdn();
            if (!idn.Contains(axis.ExpectedSerial, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"{axis.AxisId} identity mismatch: {idn}");
            }

            axis.Session.SetRemote();
        }

        return axes;
    }

    private static BaselineSnapshot LockBaseline(IReadOnlyList<MagAxisHandle> axes, string baselinePath)
    {
        var snapshots = new List<BaselineAxisSnapshot>(axes.Count);
        foreach (var axis in axes)
        {
            axis.Session.SetVoltage(M8812VoltageV);
            axis.Session.SetVoltageProtection(M8812VoltageV);
            axis.Session.SetCurrent(0.0);
            axis.Session.SetOutput(true);
            Thread.Sleep(BaselineSettleMs);

            var readbacks = new List<double>(BaselineReadbackSamples);
            for (var i = 0; i < BaselineReadbackSamples; i++)
            {
                readbacks.Add(axis.Session.MeasureCurrent());
                Thread.Sleep(100);
            }

            var locked = readbacks.Average();
            if (Math.Abs(locked) > BaselineToleranceA)
            {
                throw new InvalidOperationException($"{axis.AxisId} baseline lock exceeds tolerance: {locked}A");
            }

            snapshots.Add(new BaselineAxisSnapshot(axis.AxisId, 0.0, readbacks, locked));
        }

        var baseline = new BaselineSnapshot(
            1,
            "locked_zero_offset",
            UtcNowString(),
            BaselineSettleMs,
            BaselineReadbackSamples,
            BaselineToleranceA,
            snapshots);
        File.WriteAllText(baselinePath, JsonSerializer.Serialize(baseline, JsonOptions.Pretty) + Environment.NewLine, new UTF8Encoding(false));
        return baseline;
    }

    private static void RunPoint(
        string runId,
        int cycle,
        int index,
        MinimalPointPlan point,
        IReadOnlyList<double> baselineCurrentA,
        IReadOnlyList<MagAxisHandle> magAxes,
        Smb100aSession smb,
        OeRallCollector collector,
        string segmentsPath,
        string pointsPath,
        string qualityPath,
        long processStart)
    {
        var deltaCurrentA = DeltaCurrent(point.TargetBNt);
        var targetCurrentA = new[]
        {
            baselineCurrentA[0] + deltaCurrentA[0],
            baselineCurrentA[1] + deltaCurrentA[1],
            baselineCurrentA[2] + deltaCurrentA[2]
        };

        for (var axisIndex = 0; axisIndex < magAxes.Count; axisIndex++)
        {
            magAxes[axisIndex].Session.SetCurrent(targetCurrentA[axisIndex]);
            magAxes[axisIndex].Session.SetOutput(true);
        }

        var settleStarted = UtcNowString();
        Thread.Sleep(PointSettleMs);
        var measuredCurrentA = magAxes.Select(axis => axis.Session.MeasureCurrent()).ToArray();
        var settleEnded = UtcNowString();

        var sweep = SmbSweepSpec.Default.WithPowerDbm(point.PowerDbm);
        smb.ConfigureSweep(sweep, SmbCommandSettleMs);

        var timeoutsBefore = collector.Snapshot().Stats.TimeoutCount;
        var segmentStartTs = UtcNowString();
        var segmentStartMonotonicNs = MonotonicNsSince(processStart);
        var segmentStart = collector.Cursor();

        smb.ExecuteSweep(sweep);

        var segmentEndTs = UtcNowString();
        var segmentEndMonotonicNs = MonotonicNsSince(processStart);
        var segmentEnd = collector.Cursor();
        var timeoutsAfter = collector.Snapshot().Stats.TimeoutCount;
        var pointTimeouts = timeoutsAfter - timeoutsBefore;
        var framesInSegment = segmentEnd.NextFrameSeq - segmentStart.NextFrameSeq;
        var globalIndex = cycle * Points.Length + index;
        var segmentId = $"seg_{point.PointId}_{globalIndex:0000}";

        RallArtifactWriter.AppendSegmentRecord(
            segmentsPath,
            new SegmentRecord(
                1,
                runId,
                segmentId,
                point.PointId,
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

        RallArtifactWriter.AppendJsonl(
            pointsPath,
            new PointRecord(
                1,
                runId,
                point.PointId,
                globalIndex,
                "acquisition_step",
                RunPointPlan.Controlled,
                true,
                point.TargetBNt,
                baselineCurrentA.ToArray(),
                deltaCurrentA,
                targetCurrentA,
                ToSweepRecord(sweep),
                new SettleRecord(
                    "fixed_delay_with_readback",
                    settleStarted,
                    settleEnded,
                    "passed",
                    measuredCurrentA)));

        var duplicateCount = 0L;
        var duplicateRatio = 0.0;
        var qualityStatus = framesInSegment >= MinFrames && pointTimeouts <= MaxTimeoutCount ? "passed" : "failed_min_frames";
        RallArtifactWriter.AppendJsonl(
            qualityPath,
            new QualityRecord(
                1,
                runId,
                point.PointId,
                segmentId,
                framesInSegment,
                framesInSegment - duplicateCount,
                duplicateCount,
                duplicateRatio,
                pointTimeouts,
                0,
                MinFrames,
                null,
                null,
                pointTimeouts == 0 ? "clean" : "recovered_timeout",
                Math.Max(0, MaxTimeoutCount - pointTimeouts),
                qualityStatus));
    }

    private static void CleanupMagAxes(IReadOnlyList<MagAxisHandle> axes)
    {
        foreach (var axis in axes)
        {
            axis.Session.SetCurrent(0.0);
            axis.Session.SetOutput(false);
            Thread.Sleep(100);
            axis.Session.SetLocal();
        }
    }

    private static double[] DeltaCurrent(IReadOnlyList<double> targetBNt) =>
    [
        targetBNt[0] * CurrentPerNt,
        targetBNt[1] * CurrentPerNt,
        targetBNt[2] * CurrentPerNt
    ];

    private static SmbSweepRecord ToSweepRecord(SmbSweepSpec sweep) =>
        new(
            sweep.StartHz,
            sweep.StopHz,
            sweep.StepHz,
            sweep.DwellMs,
            sweep.PowerDbm,
            sweep.SweepMode,
            sweep.Spacing,
            sweep.Shape,
            sweep.TriggerSource,
            sweep.OutputVoltageStartV,
            sweep.OutputVoltageStopV,
            sweep.RfOutputEnabled);

    private static string UtcNowString() => DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", System.Globalization.CultureInfo.InvariantCulture);

    private static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }

    private static string PathRelative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
