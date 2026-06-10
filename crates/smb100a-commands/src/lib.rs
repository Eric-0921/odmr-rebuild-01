//! SMB100A 第一版命令 helper。
//!
//! 设计原则：
//! - 只负责生成手册级命令
//! - 不包含任何 I/O、session、transport、runtime 逻辑
//! - helper 名称保留设备型号前缀，便于全局 grep 和审计

/// `*IDN?`：查询设备身份字符串。
pub fn smb100a_query_idn() -> &'static str {
    "*IDN?"
}

/// `SYST:ERR?`：读取并弹出最旧的一条错误队列记录。
pub fn smb100a_query_error_next() -> &'static str {
    "SYST:ERR?"
}

/// `*OPC?`：等待当前挂起操作完成后返回 `1`。
pub fn smb100a_query_operation_complete() -> &'static str {
    "*OPC?"
}

/// `*CLS`：清除状态寄存器与事件队列。
///
/// 注意：这个命令在第一版运行链路里只应被视为诊断工具。
pub fn smb100a_clear_status() -> &'static str {
    "*CLS"
}

/// `OUTP ON|OFF`：设置 RF 输出开关。
pub fn smb100a_set_output(enabled: bool) -> &'static str {
    if enabled {
        "OUTP ON"
    } else {
        "OUTP OFF"
    }
}

/// `OUTP?`：查询 RF 输出开关状态。
pub fn smb100a_query_output() -> &'static str {
    "OUTP?"
}

/// `FREQ <value>`：设置 CW 频率，内部统一用 Hz。
pub fn smb100a_set_frequency_hz(hz: f64) -> String {
    format!("FREQ {hz}")
}

/// `FREQ:MODE CW|FIX|SWE`：设置 RF 频率工作模式。
///
/// 真机语义补充：
/// - `FREQ:MODE SWE` 进入 RF frequency sweep 路径
/// - `FREQ:MODE CW` 退出 sweep / list，回到固定频率模式
pub fn smb100a_set_frequency_mode(mode: &str) -> String {
    format!("FREQ:MODE {mode}")
}

/// `FREQ?`：查询当前频率。
pub fn smb100a_query_frequency() -> &'static str {
    "FREQ?"
}

/// `POW <value>dBm`：设置 RF 功率，内部统一用 dBm。
pub fn smb100a_set_power_dbm(dbm: f64) -> String {
    format!("POW {dbm}dBm")
}

/// `POW?`：查询当前 RF 功率。
pub fn smb100a_query_power() -> &'static str {
    "POW?"
}

/// `MOD:STAT ON|OFF`：统一开关全部模拟调制。
pub fn smb100a_set_modulation_state(enabled: bool) -> &'static str {
    if enabled {
        "MOD:STAT ON"
    } else {
        "MOD:STAT OFF"
    }
}

/// `FM:STAT ON|OFF`：设置 FM 调制开关。
pub fn smb100a_set_fm_state(enabled: bool) -> &'static str {
    if enabled {
        "FM:STAT ON"
    } else {
        "FM:STAT OFF"
    }
}

/// `FM:SOUR <source>`：设置 FM 调制源。
pub fn smb100a_set_fm_source(source: &str) -> String {
    format!("FM:SOUR {source}")
}

/// `FM:MODE <mode>`：设置 FM 调制模式。
pub fn smb100a_set_fm_mode(mode: &str) -> String {
    format!("FM:MODE {mode}")
}

/// `FM:DEV <value>Hz`：设置 FM 调制偏移量。
pub fn smb100a_set_fm_deviation_hz(hz: f64) -> String {
    format!("FM:DEV {hz}Hz")
}

/// `LFO ON|OFF`：设置 LF 输出开关。
pub fn smb100a_set_lf_output_state(enabled: bool) -> &'static str {
    if enabled {
        "LFO ON"
    } else {
        "LFO OFF"
    }
}

/// `LFO:FREQ <value>Hz`：设置 LF 发生器频率。
pub fn smb100a_set_lf_frequency_hz(hz: f64) -> String {
    format!("LFO:FREQ {hz}Hz")
}

/// `LFO:SHAP <shape>`：设置 LF 波形。
pub fn smb100a_set_lf_shape(shape: &str) -> String {
    format!("LFO:SHAP {shape}")
}

/// `SOUR:LFO:SIMP <impedance>`：设置 LF 输出阻抗。
pub fn smb100a_set_lf_source_impedance(impedance: &str) -> String {
    format!("SOUR:LFO:SIMP {impedance}")
}

/// `LFO:VOLT <value>mV`：设置 LF 输出幅值。
pub fn smb100a_set_lf_voltage_mv(mv: f64) -> String {
    format!("LFO:VOLT {mv}mV")
}

/// `FREQ:STAR <value>Hz`：设置扫频起点。
pub fn smb100a_set_sweep_start_hz(hz: f64) -> String {
    format!("FREQ:STAR {hz}Hz")
}

/// `FREQ:STAR?`：查询扫频起点。
pub fn smb100a_query_sweep_start() -> &'static str {
    "FREQ:STAR?"
}

/// `FREQ:STOP <value>Hz`：设置扫频终点。
pub fn smb100a_set_sweep_stop_hz(hz: f64) -> String {
    format!("FREQ:STOP {hz}Hz")
}

/// `FREQ:STOP?`：查询扫频终点。
pub fn smb100a_query_sweep_stop() -> &'static str {
    "FREQ:STOP?"
}

/// `SWE:FREQ:STEP <value>Hz`：设置扫频步进。
pub fn smb100a_set_sweep_step_hz(hz: f64) -> String {
    format!("SWE:FREQ:STEP {hz}Hz")
}

/// `SWE:FREQ:STEP?`：查询扫频步进。
pub fn smb100a_query_sweep_step() -> &'static str {
    "SWE:FREQ:STEP?"
}

/// `SWE:FREQ:DWEL <value>ms`：设置扫频驻留时间。
pub fn smb100a_set_sweep_dwell_ms(ms: u64) -> String {
    format!("SWE:FREQ:DWEL {ms}ms")
}

/// `SWE:FREQ:DWEL?`：查询扫频驻留时间。
pub fn smb100a_query_sweep_dwell() -> &'static str {
    "SWE:FREQ:DWEL?"
}

/// `SWE:MODE AUTO|MAN|STEP`：设置扫频模式。
pub fn smb100a_set_sweep_mode(mode: &str) -> String {
    format!("SWE:MODE {mode}")
}

/// `SWE:MODE?`：查询扫频模式。
pub fn smb100a_query_sweep_mode() -> &'static str {
    "SWE:MODE?"
}

/// `SWE:SHAP <shape>`：设置扫频形状。
pub fn smb100a_set_sweep_shape(shape: &str) -> String {
    format!("SWE:SHAP {shape}")
}

/// `SWE:SPAC <spacing>`：设置扫频间隔模式。
pub fn smb100a_set_sweep_spacing(spacing: &str) -> String {
    format!("SWE:SPAC {spacing}")
}

/// `TRIG:FSW:SOUR <token>`：设置频率扫频触发源。
pub fn smb100a_set_sweep_trigger_source(source: &str) -> String {
    format!("TRIG:FSW:SOUR {source}")
}

/// `TRIG:FSW:SOUR?`：查询频率扫频触发源。
pub fn smb100a_query_sweep_trigger_source() -> &'static str {
    "TRIG:FSW:SOUR?"
}

/// `SWE:OVOL:STAR <value>`：设置扫频起点对应的 LF 输出电压。
///
/// 真机验证补充：
/// - `2026-06-11` 在当前实验室 SMB100A 上，`SWE:OVOL:STAR 0V` 会报
///   `-103,"Invalid separator;V"`
/// - 同一设备接受 `SWE:OVOL:STAR 0` / `SWE:OVOL:STAR 0.0`
pub fn smb100a_set_sweep_output_voltage_start_v(v: f64) -> String {
    format!("SWE:OVOL:STAR {v}")
}

/// `SWE:OVOL:STOP <value>`：设置扫频终点对应的 LF 输出电压。
///
/// 真机验证补充：
/// - `2026-06-11` 在当前实验室 SMB100A 上，`SWE:OVOL:STOP 3V` 会报
///   `-103,"Invalid separator;V"`
/// - 同一设备接受 `SWE:OVOL:STOP 3` / `SWE:OVOL:STOP 3.0`
pub fn smb100a_set_sweep_output_voltage_stop_v(v: f64) -> String {
    format!("SWE:OVOL:STOP {v}")
}

/// `SWE:FREQ:EXEC`：执行一次频率扫频。
pub fn smb100a_execute_frequency_sweep() -> &'static str {
    "SWE:FREQ:EXEC"
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn idn_query_is_exact() {
        assert_eq!(smb100a_query_idn(), "*IDN?");
    }

    #[test]
    fn output_helper_formats_expected_tokens() {
        assert_eq!(smb100a_set_output(true), "OUTP ON");
        assert_eq!(smb100a_set_output(false), "OUTP OFF");
    }

    #[test]
    fn frequency_and_power_keep_units_explicit() {
        assert_eq!(smb100a_set_frequency_hz(2.87e9), "FREQ 2870000000");
        assert_eq!(smb100a_set_frequency_mode("CW"), "FREQ:MODE CW");
        assert_eq!(smb100a_set_frequency_mode("SWE"), "FREQ:MODE SWE");
        assert_eq!(smb100a_set_power_dbm(-30.0), "POW -30dBm");
    }

    #[test]
    fn modulation_and_fm_helpers_keep_manual_shape() {
        assert_eq!(smb100a_set_modulation_state(true), "MOD:STAT ON");
        assert_eq!(smb100a_set_modulation_state(false), "MOD:STAT OFF");
        assert_eq!(smb100a_set_fm_state(true), "FM:STAT ON");
        assert_eq!(smb100a_set_fm_state(false), "FM:STAT OFF");
        assert_eq!(smb100a_set_fm_source("INT"), "FM:SOUR INT");
        assert_eq!(smb100a_set_fm_mode("HDEV"), "FM:MODE HDEV");
        assert_eq!(smb100a_set_fm_deviation_hz(4.0e6), "FM:DEV 4000000Hz");
    }

    #[test]
    fn lf_helpers_keep_manual_shape() {
        assert_eq!(smb100a_set_lf_output_state(true), "LFO ON");
        assert_eq!(smb100a_set_lf_output_state(false), "LFO OFF");
        assert_eq!(smb100a_set_lf_frequency_hz(500.0), "LFO:FREQ 500Hz");
        assert_eq!(smb100a_set_lf_shape("SQU"), "LFO:SHAP SQU");
        assert_eq!(smb100a_set_lf_source_impedance("LOW"), "SOUR:LFO:SIMP LOW");
        assert_eq!(smb100a_set_lf_voltage_mv(137.0), "LFO:VOLT 137mV");
    }

    #[test]
    fn sweep_helpers_keep_manual_shape() {
        assert_eq!(smb100a_set_sweep_start_hz(100.0), "FREQ:STAR 100Hz");
        assert_eq!(smb100a_set_sweep_stop_hz(200.0), "FREQ:STOP 200Hz");
        assert_eq!(smb100a_set_sweep_step_hz(5.0), "SWE:FREQ:STEP 5Hz");
        assert_eq!(smb100a_set_sweep_dwell_ms(300), "SWE:FREQ:DWEL 300ms");
        assert_eq!(smb100a_set_sweep_mode("AUTO"), "SWE:MODE AUTO");
        assert_eq!(smb100a_set_sweep_shape("SAWT"), "SWE:SHAP SAWT");
        assert_eq!(smb100a_set_sweep_spacing("LIN"), "SWE:SPAC LIN");
        assert_eq!(
            smb100a_set_sweep_trigger_source("SING"),
            "TRIG:FSW:SOUR SING"
        );
        assert_eq!(
            smb100a_set_sweep_output_voltage_start_v(0.0),
            "SWE:OVOL:STAR 0"
        );
        assert_eq!(
            smb100a_set_sweep_output_voltage_stop_v(3.0),
            "SWE:OVOL:STOP 3"
        );
        assert_eq!(smb100a_execute_frequency_sweep(), "SWE:FREQ:EXEC");
    }
}
