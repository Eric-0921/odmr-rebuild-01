using System.Text.Json.Serialization;

namespace Odmr.Devices;

public sealed record DeviceCommandCatalogEntry(
    [property: JsonPropertyName("device")] string Device,
    [property: JsonPropertyName("command_id")] string CommandId,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("template")] string Template,
    [property: JsonPropertyName("rust_source")] string RustSource,
    [property: JsonPropertyName("csharp_source")] string CsharpSource,
    [property: JsonPropertyName("runtime_used")] bool RuntimeUsed);

public sealed record DeviceCommandCheckReport(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("total_commands")] int TotalCommands,
    [property: JsonPropertyName("runtime_commands")] int RuntimeCommands,
    [property: JsonPropertyName("devices")] IReadOnlyDictionary<string, int> Devices,
    [property: JsonPropertyName("commands")] IReadOnlyList<DeviceCommandCatalogEntry> Commands,
    [property: JsonPropertyName("passed")] bool Passed);

public static class DeviceCommandCatalog
{
    private const string OeRust = "crates/oe1022d-commands/src/lib.rs";
    private const string SmbRust = "crates/smb100a-commands/src/lib.rs";
    private const string M8812Rust = "crates/m8812-commands/src/lib.rs";
    private const string LaserRust = "crates/cni-laser-commands/src/lib.rs";

    public static readonly IReadOnlyList<DeviceCommandCatalogEntry> All =
    [
        Oe("query_idn", "query", "*IDN?", "Oe1022dVisa.QueryIdn", true),
        Oe("rall_query", "binary_query", "RALL?", "Oe1022dVisa.WriteRallQuery/ReadRallFrame", true),
        Oe("set_reference_source", "set", "FMODD {channel},{source}", "Oe1022dCommands.SetReferenceSource", true),
        Oe("query_reference_source", "query", "FMODD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_reference_frequency_hz", "set", "FREQD {channel},{hz}", "DeviceCommandCatalog", false),
        Oe("query_reference_frequency", "query", "FREQD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_input_source", "set", "ISRCD {channel},{source}", "Oe1022dCommands.SetInputSource", true),
        Oe("query_input_source", "query", "ISRCD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_input_grounding", "set", "IGNDD {channel},{grounding}", "Oe1022dCommands.SetInputGrounding", true),
        Oe("query_input_grounding", "query", "IGNDD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_input_coupling", "set", "ICPLD {channel},{coupling}", "Oe1022dCommands.SetInputCoupling", true),
        Oe("query_input_coupling", "query", "ICPLD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_line_notch_filter", "set", "ILIND {channel},{filter}", "Oe1022dCommands.SetLineNotchFilter", true),
        Oe("query_line_notch_filter", "query", "ILIND? {channel}", "DeviceCommandCatalog", false),
        Oe("set_reference_slope", "set", "RSLPD {channel},{slope}", "Oe1022dCommands.SetReferenceSlope", true),
        Oe("query_reference_slope", "query", "RSLPD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_phase_deg", "set", "PHASD {channel},{degree}", "Oe1022dCommands.SetPhaseDeg", true),
        Oe("query_phase", "query", "PHASD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_harmonic", "set", "HARMD {channel},{slot},{harmonic}", "Oe1022dCommands.SetHarmonic", true),
        Oe("query_harmonic", "query", "HARMD? {channel},{slot}", "DeviceCommandCatalog", false),
        Oe("set_dynamic_reserve", "set", "RMODD {channel},{mode}", "Oe1022dCommands.SetDynamicReserve", true),
        Oe("query_dynamic_reserve", "query", "RMODD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_sensitivity_index", "set", "SENSD {channel},{index}", "Oe1022dCommands.SetSensitivityIndex", true),
        Oe("query_sensitivity_index", "query", "SENSD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_time_constant_index", "set", "OFLTD {channel},{index}", "Oe1022dCommands.SetTimeConstantIndex", true),
        Oe("query_time_constant_index", "query", "OFLTD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_filter_slope", "set", "OFSLD {channel},{slope}", "Oe1022dCommands.SetFilterSlope", true),
        Oe("query_filter_slope", "query", "OFSLD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_sync_filter", "set", "SYNCD {channel},{enabled}", "Oe1022dCommands.SetSyncFilter", true),
        Oe("query_sync_filter", "query", "SYNCD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_sine_output_mode", "set", "SWVTD {channel},{mode}", "Oe1022dCommands.SetSineOutputMode", true),
        Oe("query_sine_output_mode", "query", "SWVTD? {channel}", "DeviceCommandCatalog", false),
        Oe("set_sine_output_voltage_vrms", "set", "SLVLD {channel},{vrms}", "Oe1022dCommands.SetSineOutputVoltageVrms", true),
        Oe("query_sine_output_voltage", "query", "SLVLD? {channel}", "DeviceCommandCatalog", false),

        Smb("query_idn", "query", "*IDN?", "Smb100aCommands.QueryIdn", true),
        Smb("query_error_next", "query", "SYST:ERR?", "Smb100aCommands.QuerySystemError", true),
        Smb("query_operation_complete", "query", "*OPC?", "Smb100aCommands.QueryOperationComplete", true),
        Smb("clear_status", "action", "*CLS", "DeviceCommandCatalog", false),
        Smb("set_output", "set", "OUTP {ON|OFF}", "Smb100aCommands.OutputOn/OutputOff", true),
        Smb("query_output", "query", "OUTP?", "Smb100aCommands.QueryOutput", true),
        Smb("set_frequency_hz", "set", "FREQ {hz}", "Smb100aCommands.SetFrequencyHz", true),
        Smb("set_frequency_mode", "set", "FREQ:MODE {mode}", "Smb100aCommands.FrequencyModeSweep/FrequencyModeCw", true),
        Smb("query_frequency", "query", "FREQ?", "DeviceCommandCatalog", false),
        Smb("query_frequency_mode", "query", "FREQ:MODE?", "Smb100aCommands.QueryFrequencyMode", true),
        Smb("set_power_dbm", "set", "POW {dbm}dBm", "SmbSweepSpec.ToCommands", true),
        Smb("query_power", "query", "POW?", "DeviceCommandCatalog", false),
        Smb("set_modulation_state", "set", "MOD:STAT {ON|OFF}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_fm_state", "set", "FM:STAT {ON|OFF}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_fm_source", "set", "FM:SOUR {source}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_fm_mode", "set", "FM:MODE {mode}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_fm_deviation_hz", "set", "FM:DEV {hz}Hz", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_lf_output_state", "set", "LFO {ON|OFF}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_lf_frequency_hz", "set", "LFO:FREQ {hz}Hz", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_lf_shape", "set", "LFO:SHAP {shape}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_lf_source_impedance", "set", "SOUR:LFO:SIMP {impedance}", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_lf_voltage_mv", "set", "LFO:VOLT {mv}mV", "ConfigDrivenRun.BuildSmbFixedCommands", true),
        Smb("set_sweep_start_hz", "set", "FREQ:STAR {hz}Hz", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_start", "query", "FREQ:STAR?", "DeviceCommandCatalog", false),
        Smb("set_sweep_stop_hz", "set", "FREQ:STOP {hz}Hz", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_stop", "query", "FREQ:STOP?", "DeviceCommandCatalog", false),
        Smb("set_sweep_step_hz", "set", "SWE:FREQ:STEP {hz}Hz", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_step", "query", "SWE:FREQ:STEP?", "DeviceCommandCatalog", false),
        Smb("set_sweep_dwell_ms", "set", "SWE:FREQ:DWEL {ms}ms", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_dwell", "query", "SWE:FREQ:DWEL?", "DeviceCommandCatalog", false),
        Smb("set_sweep_mode", "set", "SWE:MODE {mode}", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_mode", "query", "SWE:MODE?", "DeviceCommandCatalog", false),
        Smb("set_sweep_shape", "set", "SWE:SHAP {shape}", "SmbSweepSpec.ToCommands", true),
        Smb("set_sweep_spacing", "set", "SWE:SPAC {spacing}", "SmbSweepSpec.ToCommands", true),
        Smb("set_sweep_trigger_source", "set", "TRIG:FSW:SOUR {source}", "SmbSweepSpec.ToCommands", true),
        Smb("query_sweep_trigger_source", "query", "TRIG:FSW:SOUR?", "DeviceCommandCatalog", false),
        Smb("set_sweep_output_voltage_start_v", "set", "SWE:OVOL:STAR {v}", "SmbSweepSpec.ToCommands", true),
        Smb("set_sweep_output_voltage_stop_v", "set", "SWE:OVOL:STOP {v}", "SmbSweepSpec.ToCommands", true),
        Smb("execute_frequency_sweep", "action", "SWE:FREQ:EXEC", "Smb100aCommands.ExecuteFrequencySweep", true),

        M8812("query_idn", "query", "*IDN?", "M8812Commands.QueryIdn", true),
        M8812("set_remote", "action", "SYST:REM", "M8812Commands.SetRemote", true),
        M8812("set_local", "action", "SYST:LOC", "M8812Commands.SetLocal", true),
        M8812("query_error", "query", "SYST:ERR?", "DeviceCommandCatalog", false),
        M8812("set_voltage_v", "set", "VOLT {volts}", "M8812Commands.SetVoltage", true),
        M8812("set_voltage_protection_v", "set", "VOLT:PROT {volts}", "M8812Commands.SetVoltageProtection", true),
        M8812("set_current_a", "set", "CURR {amps}", "M8812Commands.SetCurrent", true),
        M8812("query_meas_current_a", "query", "MEAS:CURR?", "M8812Commands.QueryCurrent", true),
        M8812("set_output", "set", "OUTP {0|1}", "M8812Commands.SetOutputOn/SetOutputOff", true),

        Laser("set_power_mw", "binary_set", "55 AA 05 01 {hi} {lo} {checksum}", "CniLaserCommands.SetPowerMw", true),
        Laser("output_off", "binary_action", "55 AA 03 00 03", "CniLaserCommands.OutputOff", true),
        Laser("output_on", "binary_action", "55 AA 03 01 04", "CniLaserCommands.OutputOn", true)
    ];

    public static DeviceCommandCheckReport Check()
    {
        var devices = All
            .GroupBy(command => command.Device, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var duplicateIds = All
            .GroupBy(command => $"{command.Device}:{command.CommandId}", StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToArray();
        var passed = duplicateIds.Length == 0 && All.All(command =>
            !string.IsNullOrWhiteSpace(command.Template) &&
            !string.IsNullOrWhiteSpace(command.RustSource) &&
            !string.IsNullOrWhiteSpace(command.CsharpSource));

        return new DeviceCommandCheckReport(
            passed ? "passed" : "failed",
            All.Count,
            All.Count(command => command.RuntimeUsed),
            devices,
            All,
            passed);
    }

    private static DeviceCommandCatalogEntry Oe(string id, string kind, string template, string csharpSource, bool runtimeUsed) =>
        new("oe1022d", id, kind, template, OeRust, csharpSource, runtimeUsed);

    private static DeviceCommandCatalogEntry Smb(string id, string kind, string template, string csharpSource, bool runtimeUsed) =>
        new("smb100a", id, kind, template, SmbRust, csharpSource, runtimeUsed);

    private static DeviceCommandCatalogEntry M8812(string id, string kind, string template, string csharpSource, bool runtimeUsed) =>
        new("m8812", id, kind, template, M8812Rust, csharpSource, runtimeUsed);

    private static DeviceCommandCatalogEntry Laser(string id, string kind, string template, string csharpSource, bool runtimeUsed) =>
        new("cni_laser", id, kind, template, LaserRust, csharpSource, runtimeUsed);
}
