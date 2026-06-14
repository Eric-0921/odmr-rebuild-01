using System.Text.Json;
using System.Text.Json.Serialization;
using Odmr.Devices;

namespace Odmr.Runtime;

public sealed record StationSpec(
    [property: JsonPropertyName("station_id")] string StationId,
    [property: JsonPropertyName("devices")] IReadOnlyList<StationDeviceSpec> Devices);

public sealed record StationDeviceSpec(
    [property: JsonPropertyName("device_id")] string DeviceId,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("required")] bool Required,
    [property: JsonPropertyName("transport_hint")] StationTransportHint TransportHint,
    [property: JsonPropertyName("identity")] StationIdentity? Identity);

public sealed record StationTransportHint(
    [property: JsonPropertyName("transport")] string Transport,
    [property: JsonPropertyName("host")] string? Host,
    [property: JsonPropertyName("port")] int? Port,
    [property: JsonPropertyName("resource")] string? Resource,
    [property: JsonPropertyName("baud_rate")] int? BaudRate,
    [property: JsonPropertyName("port_path")] string? PortPath,
    [property: JsonPropertyName("host_candidates")] IReadOnlyList<string>? HostCandidates,
    [property: JsonPropertyName("resource_candidates")] IReadOnlyList<string>? ResourceCandidates,
    [property: JsonPropertyName("port_candidates")] IReadOnlyList<string>? PortCandidates);

public sealed record StationIdentity(
    [property: JsonPropertyName("contains_all")] IReadOnlyList<string>? ContainsAll,
    [property: JsonPropertyName("contains_any")] IReadOnlyList<string>? ContainsAny);

public sealed record StationConnectionFacts(
    string StationId,
    string SmbHost,
    int SmbPort,
    string OeResource,
    int OeBaudRate,
    string? XPort,
    string? YPort,
    string? ZPort,
    string? LaserPort);

public sealed record CalibrationProfile(
    [property: JsonPropertyName("calibration_id")] string CalibrationId,
    [property: JsonPropertyName("current_offset_a")] double[] CurrentOffsetA,
    [property: JsonPropertyName("current_per_nt")] double[][] CurrentPerNt)
{
    public double[] DeltaCurrentA(IReadOnlyList<double> targetBNt)
    {
        ValidateTriplet(targetBNt, "target_b_nt");
        if (CurrentOffsetA.Length != 3 || CurrentPerNt.Length != 3 || CurrentPerNt.Any(row => row.Length != 3))
        {
            throw new InvalidOperationException("calibration must contain 3 current offsets and a 3x3 current_per_nt matrix");
        }

        var outValues = new[] { CurrentOffsetA[0], CurrentOffsetA[1], CurrentOffsetA[2] };
        for (var axis = 0; axis < 3; axis++)
        {
            outValues[axis] += CurrentPerNt[axis][0] * targetBNt[0];
            outValues[axis] += CurrentPerNt[axis][1] * targetBNt[1];
            outValues[axis] += CurrentPerNt[axis][2] * targetBNt[2];
        }

        return outValues;
    }

    public double[] TargetCurrentA(IReadOnlyList<double> baselineCurrentA, IReadOnlyList<double> targetBNt)
    {
        ValidateTriplet(baselineCurrentA, "baseline_current_a");
        var delta = DeltaCurrentA(targetBNt);
        return [baselineCurrentA[0] + delta[0], baselineCurrentA[1] + delta[1], baselineCurrentA[2] + delta[2]];
    }

    private static void ValidateTriplet(IReadOnlyList<double> values, string name)
    {
        if (values.Count != 3)
        {
            throw new InvalidOperationException($"{name} must contain exactly 3 values");
        }
    }
}

public sealed record Smb100aRunProfile(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("command_settle_ms")] int CommandSettleMs,
    [property: JsonPropertyName("error_check_after_write")] bool ErrorCheckAfterWrite,
    [property: JsonPropertyName("fixed")] SmbFixedProfile Fixed,
    [property: JsonPropertyName("default_sweep")] SmbSweepDefaults DefaultSweep)
{
    public long EstimatedPointConfigurationMs => 13L * CommandSettleMs;
}

public sealed record SmbFixedProfile(
    [property: JsonPropertyName("modulation_enabled")] bool ModulationEnabled,
    [property: JsonPropertyName("fm_enabled")] bool FmEnabled,
    [property: JsonPropertyName("fm_source")] string FmSource,
    [property: JsonPropertyName("fm_mode")] string FmMode,
    [property: JsonPropertyName("fm_deviation_hz")] double FmDeviationHz,
    [property: JsonPropertyName("lf_output_enabled")] bool LfOutputEnabled,
    [property: JsonPropertyName("lf_voltage_mv")] double LfVoltageMv,
    [property: JsonPropertyName("lf_frequency_hz")] double LfFrequencyHz,
    [property: JsonPropertyName("lf_shape")] string LfShape,
    [property: JsonPropertyName("lf_source_impedance")] string LfSourceImpedance);

public sealed record SmbSweepDefaults(
    [property: JsonPropertyName("start_hz")] double StartHz,
    [property: JsonPropertyName("stop_hz")] double StopHz,
    [property: JsonPropertyName("step_hz")] double StepHz,
    [property: JsonPropertyName("dwell_ms")] int DwellMs,
    [property: JsonPropertyName("power_dbm")] double PowerDbm,
    [property: JsonPropertyName("sweep_mode")] string SweepMode,
    [property: JsonPropertyName("spacing")] string Spacing,
    [property: JsonPropertyName("shape")] string Shape,
    [property: JsonPropertyName("trigger_source")] string TriggerSource,
    [property: JsonPropertyName("output_voltage_start_v")] double OutputVoltageStartV,
    [property: JsonPropertyName("output_voltage_stop_v")] double OutputVoltageStopV,
    [property: JsonPropertyName("rf_output_enabled")] bool RfOutputEnabled)
{
    public SmbSweepSpec ApplyOverride(SmbSweepOverride? overrideSpec)
    {
        return new SmbSweepSpec(
            ToLong(overrideSpec?.StartHz ?? StartHz, "start_hz"),
            ToLong(overrideSpec?.StopHz ?? StopHz, "stop_hz"),
            ToLong(overrideSpec?.StepHz ?? StepHz, "step_hz"),
            overrideSpec?.DwellMs ?? DwellMs,
            overrideSpec?.PowerDbm ?? PowerDbm,
            overrideSpec?.SweepMode ?? SweepMode,
            overrideSpec?.Spacing ?? Spacing,
            overrideSpec?.Shape ?? Shape,
            overrideSpec?.TriggerSource ?? TriggerSource,
            overrideSpec?.OutputVoltageStartV ?? OutputVoltageStartV,
            overrideSpec?.OutputVoltageStopV ?? OutputVoltageStopV,
            overrideSpec?.RfOutputEnabled ?? RfOutputEnabled);
    }

    private static long ToLong(double value, string name)
    {
        var rounded = Math.Round(value);
        if (Math.Abs(value - rounded) > 0.000001)
        {
            throw new InvalidOperationException($"SMB sweep {name} must be an integer Hz value");
        }

        return (long)rounded;
    }
}

public sealed record SmbSweepOverride(
    [property: JsonPropertyName("start_hz")] double? StartHz,
    [property: JsonPropertyName("stop_hz")] double? StopHz,
    [property: JsonPropertyName("step_hz")] double? StepHz,
    [property: JsonPropertyName("dwell_ms")] int? DwellMs,
    [property: JsonPropertyName("power_dbm")] double? PowerDbm,
    [property: JsonPropertyName("sweep_mode")] string? SweepMode,
    [property: JsonPropertyName("spacing")] string? Spacing,
    [property: JsonPropertyName("shape")] string? Shape,
    [property: JsonPropertyName("trigger_source")] string? TriggerSource,
    [property: JsonPropertyName("output_voltage_start_v")] double? OutputVoltageStartV,
    [property: JsonPropertyName("output_voltage_stop_v")] double? OutputVoltageStopV,
    [property: JsonPropertyName("rf_output_enabled")] bool? RfOutputEnabled);

public sealed record Oe1022dRunProfile(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("command_settle_ms")] int CommandSettleMs,
    [property: JsonPropertyName("fixed")] Oe1022dFixedProfile Fixed,
    [property: JsonPropertyName("collector")] OeCollectorConfig Collector);

public sealed record Oe1022dFixedProfile(
    [property: JsonPropertyName("channel")] int Channel,
    [property: JsonPropertyName("input_source")] int InputSource,
    [property: JsonPropertyName("input_grounding")] int InputGrounding,
    [property: JsonPropertyName("input_coupling")] int InputCoupling,
    [property: JsonPropertyName("line_notch_filter")] int LineNotchFilter,
    [property: JsonPropertyName("reference_source")] int ReferenceSource,
    [property: JsonPropertyName("reference_slope")] int ReferenceSlope,
    [property: JsonPropertyName("phase_deg")] double PhaseDeg,
    [property: JsonPropertyName("harmonic_1")] int Harmonic1,
    [property: JsonPropertyName("harmonic_2")] int Harmonic2,
    [property: JsonPropertyName("dynamic_reserve")] int DynamicReserve,
    [property: JsonPropertyName("sensitivity_index")] int SensitivityIndex,
    [property: JsonPropertyName("time_constant_index")] int TimeConstantIndex,
    [property: JsonPropertyName("filter_slope")] int FilterSlope,
    [property: JsonPropertyName("sync_filter")] int SyncFilter,
    [property: JsonPropertyName("sine_output_mode")] int SineOutputMode,
    [property: JsonPropertyName("sine_output_voltage_vrms")] double SineOutputVoltageVrms);

public sealed record OeCollectorConfig(
    [property: JsonPropertyName("poll_interval_ms")] int PollIntervalMs,
    [property: JsonPropertyName("frame_exact_bytes")] int FrameExactBytes,
    [property: JsonPropertyName("frame_max_bytes")] int FrameMaxBytes,
    [property: JsonPropertyName("ring_capacity_frames")] int RingCapacityFrames,
    [property: JsonPropertyName("guard_margin_ms")] int GuardMarginMs,
    [property: JsonPropertyName("rall_post_write_delay_ms")] int RallPostWriteDelayMs);

public sealed record LaserRunProfile(
    [property: JsonPropertyName("profile_id")] string ProfileId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("power_mw")] int PowerMw,
    [property: JsonPropertyName("settle_ms")] int SettleMs);

public sealed record MagBaselinePolicy(
    [property: JsonPropertyName("baseline_current_a")] double[] BaselineCurrentA,
    [property: JsonPropertyName("settle_ms")] int SettleMs,
    [property: JsonPropertyName("readback_samples")] int ReadbackSamples,
    [property: JsonPropertyName("settle_tolerance_a")] double SettleToleranceA,
    [property: JsonPropertyName("voltage_v")] double? VoltageV,
    [property: JsonPropertyName("voltage_protection_v")] double? VoltageProtectionV,
    [property: JsonPropertyName("output_enabled")] bool OutputEnabled);

public sealed record RunQualityThresholds(
    [property: JsonPropertyName("min_frames")] int MinFrames,
    [property: JsonPropertyName("max_timeout_count")] int MaxTimeoutCount,
    [property: JsonPropertyName("max_duplicate_ratio")] double MaxDuplicateRatio,
    [property: JsonPropertyName("max_last_frame_age_ms")] long MaxLastFrameAgeMs);

public sealed record AcquisitionRunPlan(
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("operator")] string Operator,
    [property: JsonPropertyName("acquisition_window_ms")] int AcquisitionWindowMs,
    [property: JsonPropertyName("point_settle_ms")] int PointSettleMs,
    [property: JsonPropertyName("failure_policy")] string FailurePolicy,
    [property: JsonPropertyName("mag_baseline_policy")] MagBaselinePolicy MagBaselinePolicy,
    [property: JsonPropertyName("quality_thresholds")] RunQualityThresholds QualityThresholds,
    [property: JsonPropertyName("point_source")] PointSourceConfig? PointSource,
    [property: JsonPropertyName("points")] IReadOnlyList<RunPointPlan>? Points);

public sealed record RunPointPlan(
    [property: JsonPropertyName("point_id")] string PointId,
    [property: JsonPropertyName("target_b_nt")] double[]? TargetBNt,
    [property: JsonPropertyName("smb_override")] SmbSweepOverride? SmbOverride,
    [property: JsonPropertyName("magnetic_mode")] string? MagneticMode = null)
{
    public const string Controlled = "controlled";
    public const string None = "none";

    [JsonIgnore]
    public string EffectiveMagneticMode => NormalizeMagneticMode(MagneticMode);

    [JsonIgnore]
    public bool UsesMagneticControl => EffectiveMagneticMode == Controlled;

    public static string NormalizeMagneticMode(string? mode) =>
        string.IsNullOrWhiteSpace(mode) ? Controlled : mode.Trim().ToLowerInvariant();
}

public sealed record PointSourceConfig(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("axes_nt")] CartesianGridAxesNt AxesNt,
    [property: JsonPropertyName("order")] IReadOnlyList<string>? Order,
    [property: JsonPropertyName("cycle_mode")] string CycleMode,
    [property: JsonPropertyName("stop_condition")] CartesianGridStopCondition StopCondition);

public sealed record CartesianGridAxesNt(
    [property: JsonPropertyName("x")] IReadOnlyList<double> X,
    [property: JsonPropertyName("y")] IReadOnlyList<double> Y,
    [property: JsonPropertyName("z")] IReadOnlyList<double> Z);

public sealed record CartesianGridStopCondition(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("total_points")] int TotalPoints);

public sealed record SweepEstimate(
    [property: JsonPropertyName("sweep_points")] long SweepPoints,
    [property: JsonPropertyName("sweep_duration_ms")] long SweepDurationMs);

public sealed record ResolvedRunPlan(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("declared_point_count")] int DeclaredPointCount,
    [property: JsonPropertyName("resolved_point_count")] int ResolvedPointCount,
    [property: JsonPropertyName("fixed_total_points")] int? FixedTotalPoints,
    [property: JsonPropertyName("cycle_mode")] string? CycleMode,
    [property: JsonPropertyName("estimated_sweep")] SweepEstimate? EstimatedSweep,
    [property: JsonPropertyName("estimated_point_duration_ms")] long? EstimatedPointDurationMs,
    [property: JsonPropertyName("estimated_run_duration_ms")] long? EstimatedRunDurationMs,
    [property: JsonPropertyName("points")] IReadOnlyList<RunPointPlan> Points)
{
    [JsonIgnore]
    public bool RequiresMagneticControl => Points.Any(point => point.UsesMagneticControl);
}

public sealed record PlanSnapshot(
    [property: JsonPropertyName("schema_version")] int SchemaVersion,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("declared_point_count")] int DeclaredPointCount,
    [property: JsonPropertyName("resolved_point_count")] int ResolvedPointCount,
    [property: JsonPropertyName("fixed_total_points")] int? FixedTotalPoints,
    [property: JsonPropertyName("cycle_mode")] string? CycleMode,
    [property: JsonPropertyName("estimated_sweep")] SweepEstimate? EstimatedSweep,
    [property: JsonPropertyName("estimated_point_duration_ms")] long? EstimatedPointDurationMs,
    [property: JsonPropertyName("estimated_run_duration_ms")] long? EstimatedRunDurationMs,
    [property: JsonPropertyName("source_plan")] AcquisitionRunPlan SourcePlan,
    [property: JsonPropertyName("resolved_points")] IReadOnlyList<RunPointPlan> ResolvedPoints);

public sealed record ConfigResolutionSummary(
    [property: JsonPropertyName("station_id")] string StationId,
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("source_kind")] string SourceKind,
    [property: JsonPropertyName("declared_point_count")] int DeclaredPointCount,
    [property: JsonPropertyName("resolved_point_count")] int ResolvedPointCount,
    [property: JsonPropertyName("fixed_total_points")] int? FixedTotalPoints,
    [property: JsonPropertyName("cycle_mode")] string? CycleMode,
    [property: JsonPropertyName("first_point")] RunPointPlan? FirstPoint,
    [property: JsonPropertyName("last_point")] RunPointPlan? LastPoint,
    [property: JsonPropertyName("smb_profile_id")] string SmbProfileId,
    [property: JsonPropertyName("oe_profile_id")] string OeProfileId,
    [property: JsonPropertyName("laser_profile_id")] string LaserProfileId);

public sealed record RunConfigBundle(
    StationSpec Station,
    CalibrationProfile Calibration,
    AcquisitionRunPlan Plan,
    Smb100aRunProfile SmbProfile,
    Oe1022dRunProfile OeProfile,
    LaserRunProfile LaserProfile,
    StationConnectionFacts Connections,
    ResolvedRunPlan ResolvedPlan)
{
    public PlanSnapshot BuildPlanSnapshot() =>
        new(
            1,
            Plan.RunId,
            ResolvedPlan.SourceKind,
            ResolvedPlan.DeclaredPointCount,
            ResolvedPlan.ResolvedPointCount,
            ResolvedPlan.FixedTotalPoints,
            ResolvedPlan.CycleMode,
            ResolvedPlan.EstimatedSweep,
            ResolvedPlan.EstimatedPointDurationMs,
            ResolvedPlan.EstimatedRunDurationMs,
            Plan,
            ResolvedPlan.Points);

    public ConfigResolutionSummary ToSummary() =>
        new(
            Station.StationId,
            Plan.RunId,
            ResolvedPlan.SourceKind,
            ResolvedPlan.DeclaredPointCount,
            ResolvedPlan.ResolvedPointCount,
            ResolvedPlan.FixedTotalPoints,
            ResolvedPlan.CycleMode,
            ResolvedPlan.Points.FirstOrDefault(),
            ResolvedPlan.Points.LastOrDefault(),
            SmbProfile.ProfileId,
            OeProfile.ProfileId,
            LaserProfile.ProfileId);
}

public static class RunConfigLoader
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    public static RunConfigBundle Load(
        string stationPath,
        string calibrationPath,
        string planPath,
        string smbProfilePath,
        string oeProfilePath,
        string laserProfilePath)
    {
        var station = ReadJson<StationSpec>(stationPath);
        var calibration = ReadJson<CalibrationProfile>(calibrationPath);
        var plan = ReadJson<AcquisitionRunPlan>(planPath);
        var smbProfile = ReadJson<Smb100aRunProfile>(smbProfilePath);
        var oeProfile = ReadJson<Oe1022dRunProfile>(oeProfilePath);
        var laserProfile = ReadJson<LaserRunProfile>(laserProfilePath);

        var resolvedPlan = ResolvePlan(plan, smbProfile);
        ValidateOeCollector(oeProfile);
        var connections = ResolveConnections(station, resolvedPlan.RequiresMagneticControl);
        return new RunConfigBundle(station, calibration, plan, smbProfile, oeProfile, laserProfile, connections, resolvedPlan);
    }

    public static T ReadJson<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, ReadOptions) ??
            throw new InvalidOperationException($"failed to parse JSON: {path}");
    }

    public static ResolvedRunPlan ResolvePlan(AcquisitionRunPlan plan, Smb100aRunProfile smbProfile)
    {
        if (plan.PointSource is not null)
        {
            var source = plan.PointSource;
            if (source.Kind != "cartesian_grid")
            {
                throw new InvalidOperationException($"unsupported point_source.kind: {source.Kind}");
            }

            ValidateCartesianOrder(source.Order);
            ValidateAxisValues(source.AxesNt);

            var basePoints = source.CycleMode switch
            {
                "raster" => BuildRasterPoints(source.AxesNt),
                "bounce_1d_x" => BuildBounce1dXPoints(source.AxesNt),
                _ => throw new InvalidOperationException($"unsupported cartesian cycle_mode: {source.CycleMode}")
            };

            if (source.StopCondition.Kind != "fixed_total_points")
            {
                throw new InvalidOperationException($"unsupported stop_condition.kind: {source.StopCondition.Kind}");
            }

            if (source.StopCondition.TotalPoints <= 0)
            {
                throw new InvalidOperationException("fixed_total_points must be positive");
            }

            var points = RepeatPointsToTotal(basePoints, source.StopCondition.TotalPoints);
            var defaultSweep = smbProfile.DefaultSweep.ApplyOverride(null);
            var estimate = new SweepEstimate(defaultSweep.SweepPoints, defaultSweep.EstimatedSweepDurationMs);
            var estimatedPointMs = estimate.SweepDurationMs + plan.PointSettleMs + smbProfile.EstimatedPointConfigurationMs;
            return new ResolvedRunPlan(
                1,
                plan.RunId,
                "cartesian_grid",
                basePoints.Count,
                points.Count,
                source.StopCondition.TotalPoints,
                source.CycleMode,
                estimate,
                estimatedPointMs,
                estimatedPointMs * points.Count,
                points);
        }

        var explicitPoints = plan.Points ?? Array.Empty<RunPointPlan>();
        if (explicitPoints.Count == 0)
        {
            throw new InvalidOperationException("plan has no executable points");
        }

        var normalizedExplicitPoints = new List<RunPointPlan>(explicitPoints.Count);
        foreach (var point in explicitPoints)
        {
            normalizedExplicitPoints.Add(NormalizeAndValidatePoint(point));
        }

        return new ResolvedRunPlan(
            1,
            plan.RunId,
            "explicit_points",
            explicitPoints.Count,
            normalizedExplicitPoints.Count,
            null,
            null,
            null,
            null,
            null,
            normalizedExplicitPoints);
    }

    public static StationConnectionFacts ResolveConnections(StationSpec station, bool requireMagAxes)
    {
        var smb = RequiredDevice(station, "smb100a", "smb100a_main");
        var oe = RequiredDevice(station, "oe1022d", "oe1022d_main");
        var x = requireMagAxes ? RequiredDevice(station, "m8812", "mag_x") : OptionalDevice(station, "m8812", "mag_x");
        var y = requireMagAxes ? RequiredDevice(station, "m8812", "mag_y") : OptionalDevice(station, "m8812", "mag_y");
        var z = requireMagAxes ? RequiredDevice(station, "m8812", "mag_z") : OptionalDevice(station, "m8812", "mag_z");
        var laser = station.Devices.FirstOrDefault(device => device.Kind == "cni_laser");

        return new StationConnectionFacts(
            station.StationId,
            Required(smb.TransportHint.Host, "SMB host"),
            smb.TransportHint.Port ?? throw new InvalidOperationException("SMB port missing"),
            Required(oe.TransportHint.Resource, "OE resource"),
            oe.TransportHint.BaudRate ?? Oe1022dDefaults.BaudRate,
            x is null ? null : Required(x.TransportHint.PortPath, "mag_x port"),
            y is null ? null : Required(y.TransportHint.PortPath, "mag_y port"),
            z is null ? null : Required(z.TransportHint.PortPath, "mag_z port"),
            laser?.TransportHint.PortPath);
    }

    public static void ValidateOeCollector(Oe1022dRunProfile profile)
    {
        if (profile.Collector.FrameExactBytes != Oe1022dDefaults.RallFrameBytes)
        {
            throw new InvalidOperationException($"oe_profile.collector.frame_exact_bytes must be {Oe1022dDefaults.RallFrameBytes}");
        }

        if (profile.Collector.RallPostWriteDelayMs != Oe1022dDefaults.RallPostWriteDelayMs)
        {
            throw new InvalidOperationException($"oe_profile.collector.rall_post_write_delay_ms must be {Oe1022dDefaults.RallPostWriteDelayMs}");
        }
    }

    private static StationDeviceSpec RequiredDevice(StationSpec station, string kind, string deviceId)
    {
        return station.Devices.FirstOrDefault(device => device.DeviceId == deviceId && device.Kind == kind) ??
            throw new InvalidOperationException($"station missing required {kind} device `{deviceId}`");
    }

    private static StationDeviceSpec? OptionalDevice(StationSpec station, string kind, string deviceId) =>
        station.Devices.FirstOrDefault(device => device.DeviceId == deviceId && device.Kind == kind);

    private static string Required(string? value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"{name} missing") : value;

    private static void ValidateCartesianOrder(IReadOnlyList<string>? order)
    {
        IReadOnlyList<string> effective = order is null || order.Count == 0 ? ["x", "y", "z"] : order;
        if (effective.Count != 3 || effective[0] != "x" || effective[1] != "y" || effective[2] != "z")
        {
            throw new InvalidOperationException($"cartesian_grid.order only supports [\"x\", \"y\", \"z\"], current=[{string.Join(",", effective)}]");
        }
    }

    private static void ValidateAxisValues(CartesianGridAxesNt axes)
    {
        if (axes.X.Count == 0)
        {
            throw new InvalidOperationException("cartesian_grid axis x has no values");
        }
        if (axes.Y.Count == 0)
        {
            throw new InvalidOperationException("cartesian_grid axis y has no values");
        }
        if (axes.Z.Count == 0)
        {
            throw new InvalidOperationException("cartesian_grid axis z has no values");
        }
    }

    private static List<RunPointPlan> BuildRasterPoints(CartesianGridAxesNt axes)
    {
        var points = new List<RunPointPlan>(axes.X.Count * axes.Y.Count * axes.Z.Count);
        var next = 1;
        foreach (var z in axes.Z)
        {
            foreach (var y in axes.Y)
            {
                foreach (var x in axes.X)
                {
                    points.Add(new RunPointPlan($"p{next:000000}", [x, y, z], null, RunPointPlan.Controlled));
                    next++;
                }
            }
        }

        return points;
    }

    private static List<RunPointPlan> BuildBounce1dXPoints(CartesianGridAxesNt axes)
    {
        if (axes.Y.Count != 1 || axes.Z.Count != 1)
        {
            throw new InvalidOperationException($"bounce_1d_x requires single y/z values, y_len={axes.Y.Count}, z_len={axes.Z.Count}");
        }

        var xValues = axes.X.ToList();
        if (axes.X.Count > 1)
        {
            for (var i = axes.X.Count - 2; i >= 1; i--)
            {
                xValues.Add(axes.X[i]);
            }
        }

        return xValues.Select((x, index) => new RunPointPlan($"p{index + 1:000000}", [x, axes.Y[0], axes.Z[0]], null, RunPointPlan.Controlled)).ToList();
    }

    private static List<RunPointPlan> RepeatPointsToTotal(IReadOnlyList<RunPointPlan> basePoints, int totalPoints)
    {
        var resolved = new List<RunPointPlan>(totalPoints);
        for (var index = 0; index < totalPoints; index++)
        {
            var point = basePoints[index % basePoints.Count];
            resolved.Add(point with { PointId = $"p{index + 1:000000}" });
        }

        return resolved;
    }

    private static RunPointPlan NormalizeAndValidatePoint(RunPointPlan point)
    {
        var mode = RunPointPlan.NormalizeMagneticMode(point.MagneticMode);
        if (mode is not RunPointPlan.Controlled and not RunPointPlan.None)
        {
            throw new InvalidOperationException($"point {point.PointId} unsupported magnetic_mode: {point.MagneticMode}");
        }

        if (mode == RunPointPlan.None)
        {
            if (point.TargetBNt is not null)
            {
                throw new InvalidOperationException($"point {point.PointId} magnetic_mode=none must not define target_b_nt");
            }

            return point with { MagneticMode = RunPointPlan.None };
        }

        if (point.TargetBNt is null || point.TargetBNt.Length != 3)
        {
            throw new InvalidOperationException($"point {point.PointId} target_b_nt must contain exactly 3 values");
        }

        return point with { MagneticMode = RunPointPlan.Controlled };
    }
}
