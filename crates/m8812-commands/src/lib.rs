//! M8812 第一版命令 helper。
//!
//! 这层只负责把手册里的串口命令集中表达出来。
//! 不负责电流上限策略、磁场换算、transport 或清理流程。

/// `*IDN?`：查询设备身份。
pub fn m8812_query_idn() -> &'static str {
    "*IDN?"
}

/// `SYST:REM`：进入远程控制模式。
pub fn m8812_set_remote() -> &'static str {
    "SYST:REM"
}

/// `SYST:LOC`：回到本地模式。
pub fn m8812_set_local() -> &'static str {
    "SYST:LOC"
}

/// `SYST:ERR?`：查询错误队列。
pub fn m8812_query_error() -> &'static str {
    "SYST:ERR?"
}

/// `VOLT <value>`：设置输出电压，单位 V。
pub fn m8812_set_voltage_v(volts: f64) -> String {
    format!("VOLT {volts}")
}

/// `VOLT:PROT <value>`：设置过压保护，单位 V。
pub fn m8812_set_voltage_protection_v(volts: f64) -> String {
    format!("VOLT:PROT {volts}")
}

/// `CURR <value>`：设置输出电流，单位 A。
pub fn m8812_set_current_a(amps: f64) -> String {
    format!("CURR {amps:.5}")
}

/// `MEAS:CURR?`：查询回读电流，单位 A。
pub fn m8812_query_meas_current_a() -> &'static str {
    "MEAS:CURR?"
}

/// `OUTP 0|1`：设置输出开关。
pub fn m8812_set_output(enabled: bool) -> &'static str {
    if enabled {
        "OUTP 1"
    } else {
        "OUTP 0"
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn identity_and_mode_commands_are_exact() {
        assert_eq!(m8812_query_idn(), "*IDN?");
        assert_eq!(m8812_set_remote(), "SYST:REM");
        assert_eq!(m8812_set_local(), "SYST:LOC");
        assert_eq!(m8812_query_error(), "SYST:ERR?");
    }

    #[test]
    fn current_helper_keeps_manual_unit_and_precision() {
        assert_eq!(m8812_set_current_a(0.01), "CURR 0.01000");
        assert_eq!(m8812_set_current_a(0.0), "CURR 0.00000");
    }

    #[test]
    fn voltage_and_output_helpers_format_expected_strings() {
        assert_eq!(m8812_set_voltage_v(75.0), "VOLT 75");
        assert_eq!(m8812_set_voltage_protection_v(75.0), "VOLT:PROT 75");
        assert_eq!(m8812_set_output(true), "OUTP 1");
        assert_eq!(m8812_set_output(false), "OUTP 0");
    }
}
