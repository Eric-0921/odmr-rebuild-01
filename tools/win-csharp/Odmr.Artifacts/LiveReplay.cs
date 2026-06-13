using System.Text.Json;
using System.Text.Json.Serialization;

namespace Odmr.Artifacts;

public sealed record LiveReplaySnapshot(
    [property: JsonPropertyName("run_dir")] string RunDir,
    [property: JsonPropertyName("run_id")] string? RunId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("points_total")] long PointsTotal,
    [property: JsonPropertyName("points_completed")] long PointsCompleted,
    [property: JsonPropertyName("points_passed")] long PointsPassed,
    [property: JsonPropertyName("points_failed")] long PointsFailed,
    [property: JsonPropertyName("frames_total")] long FramesTotal,
    [property: JsonPropertyName("last_frame_seq")] long? LastFrameSeq,
    [property: JsonPropertyName("last_frame_ts")] string? LastFrameTs,
    [property: JsonPropertyName("timeout_count")] long TimeoutCount,
    [property: JsonPropertyName("raw_len_bad_count")] long RawLenBadCount,
    [property: JsonPropertyName("delta_gt1_count")] long DeltaGt1Count,
    [property: JsonPropertyName("collector_health")] string CollectorHealth,
    [property: JsonPropertyName("event_counts")] IReadOnlyDictionary<string, long> EventCounts,
    [property: JsonPropertyName("recent_events")] IReadOnlyList<LiveReplayEvent> RecentEvents);

public sealed record LiveReplayEvent(
    [property: JsonPropertyName("ts")] string? Ts,
    [property: JsonPropertyName("event")] string? Event,
    [property: JsonPropertyName("point_id")] string? PointId,
    [property: JsonPropertyName("device")] string? Device,
    [property: JsonPropertyName("phase")] string? Phase);

public static class LiveReplay
{
    public static LiveReplaySnapshot Replay(string runDir, int tailEvents)
    {
        if (tailEvents < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tailEvents), "tail event count must be nonnegative");
        }

        var summaryPath = Path.Combine(runDir, "summary.json");
        var eventsPath = Path.Combine(runDir, "events.jsonl");
        var indexPath = Path.Combine(runDir, "raw", "oe1022d.frames.idx.jsonl");
        if (!File.Exists(summaryPath))
        {
            throw new FileNotFoundException("summary missing", summaryPath);
        }

        if (!File.Exists(eventsPath))
        {
            throw new FileNotFoundException("events missing", eventsPath);
        }

        using var summaryDocument = JsonDocument.Parse(File.ReadAllText(summaryPath));
        var summary = summaryDocument.RootElement;
        var eventCounts = new Dictionary<string, long>(StringComparer.Ordinal);
        var recentEvents = new Queue<LiveReplayEvent>();
        long pointsCompletedFromEvents = 0;

        foreach (var line in File.ReadLines(eventsPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var eventName = GetString(root, "event");
            if (eventName is not null)
            {
                eventCounts[eventName] = eventCounts.TryGetValue(eventName, out var count) ? count + 1 : 1;
                if (eventName == "point_completed")
                {
                    pointsCompletedFromEvents++;
                }
            }

            if (tailEvents > 0)
            {
                recentEvents.Enqueue(new LiveReplayEvent(
                    GetString(root, "ts"),
                    eventName,
                    GetString(root, "point_id"),
                    GetString(root, "device"),
                    GetString(root, "phase")));
                while (recentEvents.Count > tailEvents)
                {
                    recentEvents.Dequeue();
                }
            }
        }

        var lastFrame = File.Exists(indexPath) ? ReadLastFrame(indexPath) : null;
        var timeoutCount = GetInt64(summary, "timeout_count") ?? 0;
        var rawLenBadCount = GetInt64(summary, "raw_len_bad_count") ?? 0;
        var deltaGt1Count = GetNestedInt64(summary, "packet_counter", "delta_gt1_count") ?? 0;
        var collectorHealth = timeoutCount == 0 && rawLenBadCount == 0 && deltaGt1Count == 0
            ? "clean"
            : "degraded";

        return new LiveReplaySnapshot(
            runDir,
            GetString(summary, "run_id"),
            GetString(summary, "status"),
            GetInt64(summary, "points_total") ?? 0,
            pointsCompletedFromEvents,
            GetInt64(summary, "points_passed") ?? 0,
            GetInt64(summary, "points_failed") ?? 0,
            GetInt64(summary, "frames_total") ?? GetInt64(summary, "frames_ok") ?? 0,
            lastFrame?.FrameSeq,
            lastFrame?.Ts,
            timeoutCount,
            rawLenBadCount,
            deltaGt1Count,
            collectorHealth,
            eventCounts,
            recentEvents.ToArray());
    }

    private static FrameIndexAuditRecord? ReadLastFrame(string indexPath)
    {
        string? lastLine = null;
        foreach (var line in File.ReadLines(indexPath))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lastLine = line;
            }
        }

        return lastLine is null
            ? null
            : JsonSerializer.Deserialize<FrameIndexAuditRecord>(lastLine, JsonOptions.Default);
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static long? GetInt64(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) &&
        property.ValueKind == JsonValueKind.Number &&
        property.TryGetInt64(out var value)
            ? value
            : null;

    private static long? GetNestedInt64(JsonElement element, string objectName, string propertyName)
    {
        if (!element.TryGetProperty(objectName, out var nested) || nested.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return GetInt64(nested, propertyName);
    }
}
