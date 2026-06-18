using Odmr.Devices;

namespace Odmr.Runtime;

public static partial class ConfigDrivenRun
{
    private static CniLaserSession? ApplyLaserProfile(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var mode = bundle.LaserProfile.Mode;
        if (mode == "off_background")
        {
            if (bundle.Connections.LaserPort is null)
            {
                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, null, new
                {
                    mode,
                    power_mw = bundle.LaserProfile.PowerMw,
                    status = "no_laser_device"
                });
                return null;
            }

            var laser = CniLaserSerial.Open(bundle.Connections.LaserPort);
            laser.OutputOff();
            Thread.Sleep(bundle.LaserProfile.SettleMs);
            AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, "cni_laser_main", new
            {
                mode,
                power_mw = bundle.LaserProfile.PowerMw
            });
            return laser;
        }

        if (mode != "on_background")
        {
            throw new InvalidOperationException($"unsupported laser profile mode: {mode}");
        }

        if (bundle.Connections.LaserPort is null)
        {
            throw new InvalidOperationException("laser profile requires on_background, but station has no laser port");
        }

        var session = CniLaserSerial.Open(bundle.Connections.LaserPort);
        session.SetPowerMw(bundle.LaserProfile.PowerMw);
        Thread.Sleep(bundle.LaserProfile.SettleMs);
        session.OutputOn();
        Thread.Sleep(bundle.LaserProfile.SettleMs);
        AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "laser_profile_applied", "laser", null, "cni_laser_main", new
        {
            mode,
            power_mw = bundle.LaserProfile.PowerMw
        });
        return session;
    }

    private static void ApplyOeFixedProfile(RunConfigBundle bundle, RuntimeResolvedConnections resolvedConnections, string eventsPath, long processStart)
    {
        switch (bundle.OeProfile.NormalizedModel)
        {
            case LockinModelNames.Oe1022d:
            {
                using var oe = Oe1022dVisa.Open(
                    resolvedConnections.Lockin.Resource ?? throw new InvalidOperationException("OE1022D resource missing"),
                    resolvedConnections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate);
                var fixedProfile = bundle.OeProfile.GetOe1022dFixed();
                var commands = BuildOeFixedCommands(fixedProfile);
                foreach (var command in commands)
                {
                    oe.SendAsciiCommand(command);
                    Thread.Sleep(bundle.OeProfile.CommandSettleMs);
                }

                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "oe_profile_applied", "profile", null, resolvedConnections.Lockin.DeviceId, new
                {
                    profile_id = bundle.OeProfile.ProfileId,
                    lockin_model = bundle.OeProfile.NormalizedModel,
                    fixed_commands_sent = true,
                    command_count = commands.Length,
                    channel = fixedProfile.Channel,
                    resource = resolvedConnections.Lockin.Resource
                });
                return;
            }
            case LockinModelNames.Oe1300:
            {
                using var oe = Oe1300Tcp.Open(
                    resolvedConnections.Lockin.Host ?? throw new InvalidOperationException("OE1300 host missing"),
                    resolvedConnections.Lockin.Port ?? Oe1300Defaults.TcpPort);
                var fixedProfile = bundle.OeProfile.GetOe1300Fixed();
                var commands = BuildOe1300FixedCommands(fixedProfile);
                foreach (var command in commands)
                {
                    oe.SendAsciiCommand(command);
                    Thread.Sleep(bundle.OeProfile.CommandSettleMs);
                }

                AppendEvent(eventsPath, processStart, bundle.Plan.RunId, "oe_profile_applied", "profile", null, resolvedConnections.Lockin.DeviceId, new
                {
                    profile_id = bundle.OeProfile.ProfileId,
                    lockin_model = bundle.OeProfile.NormalizedModel,
                    fixed_commands_sent = true,
                    command_count = commands.Length,
                    host = resolvedConnections.Lockin.Host,
                    port = resolvedConnections.Lockin.Port
                });
                return;
            }
            default:
                throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}");
        }
    }

    private static ILockinCollector CreateLockinCollector(
        RunConfigBundle bundle,
        RuntimeResolvedConnections resolvedConnections,
        string collectorPath,
        string parameterValuesPath,
        string sampleValuesPath,
        long processStart)
    {
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => new OeRallCollector(
                resolvedConnections.Lockin.Resource ?? throw new InvalidOperationException("OE1022D resource missing"),
                resolvedConnections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate,
                collectorPath,
                parameterValuesPath,
                sampleValuesPath,
                processStart),
            LockinModelNames.Oe1300 => new Oe1300TcpCollector(
                resolvedConnections.Lockin.Host ?? throw new InvalidOperationException("OE1300 host missing"),
                resolvedConnections.Lockin.Port ?? Oe1300Defaults.TcpPort,
                collectorPath,
                parameterValuesPath,
                sampleValuesPath,
                processStart,
                bundle.OeProfile.GetOe1300Collector()),
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static object BuildCollectorStartedData(RunConfigBundle bundle)
    {
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => new
            {
                frame_exact_bytes = bundle.OeProfile.GetOe1022dCollector().FrameExactBytes,
                rall_post_write_delay_ms = bundle.OeProfile.GetOe1022dCollector().RallPostWriteDelayMs,
                ring_capacity_frames = bundle.OeProfile.GetOe1022dCollector().RingCapacityFrames
            },
            LockinModelNames.Oe1300 => new
            {
                tcp_expected_bytes = bundle.OeProfile.GetOe1300Collector().TcpExpectedBytes,
                tcp_payload_bytes = bundle.OeProfile.GetOe1300Collector().TcpPayloadBytes,
                parameter_count = bundle.OeProfile.GetOe1300Collector().ParameterCount,
                samples_per_parameter = bundle.OeProfile.GetOe1300Collector().SamplesPerParameter,
                rall_post_write_delay_ms = bundle.OeProfile.GetOe1300Collector().RallPostWriteDelayMs,
                drain_before_write = bundle.OeProfile.GetOe1300Collector().DrainBeforeWrite
            },
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static string[] BuildOeFixedCommands(Oe1022dFixedProfile fixedProfile) =>
    [
        Oe1022dCommands.SetInputSource(fixedProfile.Channel, fixedProfile.InputSource),
        Oe1022dCommands.SetInputGrounding(fixedProfile.Channel, fixedProfile.InputGrounding),
        Oe1022dCommands.SetInputCoupling(fixedProfile.Channel, fixedProfile.InputCoupling),
        Oe1022dCommands.SetLineNotchFilter(fixedProfile.Channel, fixedProfile.LineNotchFilter),
        Oe1022dCommands.SetReferenceSource(fixedProfile.Channel, fixedProfile.ReferenceSource),
        Oe1022dCommands.SetReferenceSlope(fixedProfile.Channel, fixedProfile.ReferenceSlope),
        Oe1022dCommands.SetPhaseDeg(fixedProfile.Channel, fixedProfile.PhaseDeg),
        Oe1022dCommands.SetHarmonic(fixedProfile.Channel, 1, fixedProfile.Harmonic1),
        Oe1022dCommands.SetHarmonic(fixedProfile.Channel, 2, fixedProfile.Harmonic2),
        Oe1022dCommands.SetDynamicReserve(fixedProfile.Channel, fixedProfile.DynamicReserve),
        Oe1022dCommands.SetSensitivityIndex(fixedProfile.Channel, fixedProfile.SensitivityIndex),
        Oe1022dCommands.SetTimeConstantIndex(fixedProfile.Channel, fixedProfile.TimeConstantIndex),
        Oe1022dCommands.SetFilterSlope(fixedProfile.Channel, fixedProfile.FilterSlope),
        Oe1022dCommands.SetSyncFilter(fixedProfile.Channel, fixedProfile.SyncFilter),
        Oe1022dCommands.SetSineOutputMode(fixedProfile.Channel, fixedProfile.SineOutputMode),
        Oe1022dCommands.SetSineOutputVoltageVrms(fixedProfile.Channel, fixedProfile.SineOutputVoltageVrms)
    ];

    private static string[] BuildOe1300FixedCommands(Oe1300FixedProfile fixedProfile) =>
    [
        Oe1300Commands.SetInputSource(fixedProfile.InputSource),
        Oe1300Commands.SetInputCoupling(fixedProfile.InputCoupling),
        Oe1300Commands.SetInputRange(fixedProfile.InputRange),
        Oe1300Commands.SetReferenceSource(fixedProfile.ReferenceSource),
        Oe1300Commands.SetReferenceFrequency(fixedProfile.ReferenceFrequencyHz),
        Oe1300Commands.SetReferenceSlope(fixedProfile.ReferenceSlope),
        Oe1300Commands.SetSensitivity(fixedProfile.SensitivityIndex),
        Oe1300Commands.SetTimeConstant(fixedProfile.TimeConstantSeconds),
        Oe1300Commands.SetFilterSlope(fixedProfile.FilterSlope),
        Oe1300Commands.SetSync(fixedProfile.SyncEnabled),
        Oe1300Commands.SetSineOutputEnabled(fixedProfile.SineOutputEnabled),
        Oe1300Commands.SetSineOutputVoltageVrms(fixedProfile.SineOutputVoltageVrms)
    ];
}
