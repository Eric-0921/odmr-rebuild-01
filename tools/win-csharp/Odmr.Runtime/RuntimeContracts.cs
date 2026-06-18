namespace Odmr.Runtime;

public enum RuntimeState
{
    Created,
    StationResolved,
    PreflightPassed,
    RunOpened,
    CollectorRunning,
    PointRunning,
    Stopping,
    Paused,
    Aborted,
    Completed,
    Failed,
    CleanupFailed
}

public static class RuntimeContracts
{
    public const string Oe1022dFrozenRallHotPath =
        "write RALL?, sleep 30ms, blocking exact read 12288B, direct-decode, append collector_frames + parameter_values + sample_values";

    public const string Oe1300FrozenRallHotPath =
        "write RALL?\\r, sleep 5ms, read until 32768B, detect unique block, decode 37 x 100 big-endian double for unique blocks only, append collector_blocks + unique-only parameter_values + unique-only sample_values";
}

public sealed record RunProgressEvent(
    RuntimeState State,
    string EventName,
    string Message,
    string? PointId,
    int? PointIndex,
    int? PointsTotal,
    long? FramesTotal,
    long? TimeoutCount,
    long? RawLenBadCount,
    long? DeltaGt1Count,
    string? QualityStatus,
    long? EstimatedRunDurationMs = null,
    long? EstimatedPointDurationMs = null,
    long? EstimatedSweepDurationMs = null,
    long? SweepPoints = null,
    long? StartHz = null,
    long? StopHz = null,
    long? StepHz = null,
    int? DwellMs = null,
    string? LockinModel = null,
    string? CollectorContract = null,
    long? DecodeFailures = null,
    double? EffectiveSampleHzPerParameter = null);
