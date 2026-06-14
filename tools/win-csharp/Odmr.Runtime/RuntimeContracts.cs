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
    Aborted,
    Completed,
    Failed,
    CleanupFailed
}

public static class RuntimeContracts
{
    public const string FrozenRallHotPath =
        "write RALL?, sleep 30ms, blocking exact read 12288B, append raw, append frame index";
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
    int? DwellMs = null);
