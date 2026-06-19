using System.Globalization;
using System.Text;
using System.Text.Json;
using Odmr.Artifacts;
using Odmr.Runtime;

namespace Odmr.WinProbe;

internal sealed class ProgressJsonlWriter : IProgress<RunProgressEvent>, IDisposable
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
                samples_total = value.SamplesTotal,
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
