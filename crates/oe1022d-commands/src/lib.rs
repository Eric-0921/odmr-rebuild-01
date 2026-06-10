//! OE1022D 第一版命令 helper。
//!
//! 设计原则：
//! - 只保留第一版需要的固定配置命令和 `RALL?`
//! - 命令名尽量贴手册
//! - 不在这里发明更高层的通道配置抽象

/// `FMODD i,j`：设置参考源。
pub fn oe1022d_set_reference_source(channel: u8, source: u8) -> String {
    format!("FMODD {channel},{source}")
}

/// `FMODD? i`：查询参考源。
pub fn oe1022d_query_reference_source(channel: u8) -> String {
    format!("FMODD? {channel}")
}

/// `FREQD i,f`：设置内部参考频率，单位 Hz。
pub fn oe1022d_set_reference_frequency_hz(channel: u8, hz: f64) -> String {
    format!("FREQD {channel},{hz}")
}

/// `FREQD? i`：查询参考频率。
pub fn oe1022d_query_reference_frequency(channel: u8) -> String {
    format!("FREQD? {channel}")
}

/// `ISRCD i,j`：设置输入方式。
pub fn oe1022d_set_input_source(channel: u8, source: u8) -> String {
    format!("ISRCD {channel},{source}")
}

/// `ISRCD? i`：查询输入方式。
pub fn oe1022d_query_input_source(channel: u8) -> String {
    format!("ISRCD? {channel}")
}

/// `IGNDD i,j`：设置输入接地方式。
pub fn oe1022d_set_input_grounding(channel: u8, grounding: u8) -> String {
    format!("IGNDD {channel},{grounding}")
}

/// `IGNDD? i`：查询输入接地方式。
pub fn oe1022d_query_input_grounding(channel: u8) -> String {
    format!("IGNDD? {channel}")
}

/// `ICPLD i,j`：设置输入耦合方式。
pub fn oe1022d_set_input_coupling(channel: u8, coupling: u8) -> String {
    format!("ICPLD {channel},{coupling}")
}

/// `ICPLD? i`：查询输入耦合方式。
pub fn oe1022d_query_input_coupling(channel: u8) -> String {
    format!("ICPLD? {channel}")
}

/// `ILIND i,j`：设置陷波器模式。
pub fn oe1022d_set_line_notch_filter(channel: u8, filter: u8) -> String {
    format!("ILIND {channel},{filter}")
}

/// `ILIND? i`：查询陷波器模式。
pub fn oe1022d_query_line_notch_filter(channel: u8) -> String {
    format!("ILIND? {channel}")
}

/// `RSLPD i,j`：设置外部参考触发方式。
///
/// 注意：
/// - V1.5 PDF 手册原文仅明确 `j=0` / `j=1`
/// - `2026-06-11` 真机只读观测到厂商 LabVIEW 配置并锁 PLL 后 `RSLPD? 2 = 2`
/// - 因此当前 helper 只负责拼接命令文本，不代表枚举空间已经被完全确认
pub fn oe1022d_set_reference_slope(channel: u8, slope: u8) -> String {
    format!("RSLPD {channel},{slope}")
}

/// `RSLPD? i`：查询外部参考触发方式。
///
/// 注意：查询结果当前不得假设只会返回 `0` / `1`。
pub fn oe1022d_query_reference_slope(channel: u8) -> String {
    format!("RSLPD? {channel}")
}

/// `PHASD i,x`：设置参考相位，单位 degree。
pub fn oe1022d_set_phase_deg(channel: u8, degree: f64) -> String {
    format!("PHASD {channel},{degree}")
}

/// `PHASD? i`：查询参考相位。
pub fn oe1022d_query_phase(channel: u8) -> String {
    format!("PHASD? {channel}")
}

/// `HARMD i,j,k`：设置某个谐波槽位的谐波阶数。
pub fn oe1022d_set_harmonic(channel: u8, slot: u8, harmonic: u16) -> String {
    format!("HARMD {channel},{slot},{harmonic}")
}

/// `HARMD? i,j`：查询某个谐波槽位的谐波阶数。
pub fn oe1022d_query_harmonic(channel: u8, slot: u8) -> String {
    format!("HARMD? {channel},{slot}")
}

/// `RMODD i,j`：设置动态储备模式。
pub fn oe1022d_set_dynamic_reserve(channel: u8, mode: u8) -> String {
    format!("RMODD {channel},{mode}")
}

/// `RMODD? i`：查询动态储备模式。
pub fn oe1022d_query_dynamic_reserve(channel: u8) -> String {
    format!("RMODD? {channel}")
}

/// `SENSD i,j`：设置灵敏度索引。
pub fn oe1022d_set_sensitivity_index(channel: u8, index: u8) -> String {
    format!("SENSD {channel},{index}")
}

/// `SENSD? i`：查询灵敏度索引。
pub fn oe1022d_query_sensitivity_index(channel: u8) -> String {
    format!("SENSD? {channel}")
}

/// `OFLTD i,j`：设置时间常数索引。
pub fn oe1022d_set_time_constant_index(channel: u8, index: u8) -> String {
    format!("OFLTD {channel},{index}")
}

/// `OFLTD? i`：查询时间常数索引。
pub fn oe1022d_query_time_constant_index(channel: u8) -> String {
    format!("OFLTD? {channel}")
}

/// `OFSLD i,j`：设置滤波器斜率索引。
pub fn oe1022d_set_filter_slope(channel: u8, slope: u8) -> String {
    format!("OFSLD {channel},{slope}")
}

/// `OFSLD? i`：查询滤波器斜率索引。
pub fn oe1022d_query_filter_slope(channel: u8) -> String {
    format!("OFSLD? {channel}")
}

/// `SYNCD i,j`：设置同步滤波器开关。
pub fn oe1022d_set_sync_filter(channel: u8, enabled: u8) -> String {
    format!("SYNCD {channel},{enabled}")
}

/// `SYNCD? i`：查询同步滤波器开关。
pub fn oe1022d_query_sync_filter(channel: u8) -> String {
    format!("SYNCD? {channel}")
}

/// `SWVTD i,j`：设置正弦输出模式。
pub fn oe1022d_set_sine_output_mode(channel: u8, mode: u8) -> String {
    format!("SWVTD {channel},{mode}")
}

/// `SWVTD? i`：查询正弦输出模式。
pub fn oe1022d_query_sine_output_mode(channel: u8) -> String {
    format!("SWVTD? {channel}")
}

/// `SLVLD i,x`：设置固定幅值模式下的正弦输出幅值，单位 Vrms。
pub fn oe1022d_set_sine_output_voltage_vrms(channel: u8, vrms: f64) -> String {
    format!("SLVLD {channel},{vrms}")
}

/// `SLVLD? i`：查询固定幅值模式下的正弦输出幅值。
pub fn oe1022d_query_sine_output_voltage(channel: u8) -> String {
    format!("SLVLD? {channel}")
}

/// `RALL?`：读取一帧全局测量和配置二进制数据。
pub fn oe1022d_rall_query() -> &'static str {
    "RALL?"
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn reference_helpers_keep_manual_shape() {
        assert_eq!(oe1022d_set_reference_source(2, 0), "FMODD 2,0");
        assert_eq!(oe1022d_query_reference_source(1), "FMODD? 1");
        assert_eq!(
            oe1022d_set_reference_frequency_hz(1, 2048.0),
            "FREQD 1,2048"
        );
        assert_eq!(oe1022d_query_reference_frequency(2), "FREQD? 2");
        assert_eq!(oe1022d_set_reference_slope(2, 1), "RSLPD 2,1");
        assert_eq!(oe1022d_query_reference_slope(2), "RSLPD? 2");
        assert_eq!(oe1022d_set_phase_deg(2, 0.0), "PHASD 2,0");
        assert_eq!(oe1022d_query_phase(2), "PHASD? 2");
    }

    #[test]
    fn input_and_filter_helpers_keep_manual_shape() {
        assert_eq!(oe1022d_set_input_source(2, 0), "ISRCD 2,0");
        assert_eq!(oe1022d_query_input_source(2), "ISRCD? 2");
        assert_eq!(oe1022d_set_input_grounding(2, 0), "IGNDD 2,0");
        assert_eq!(oe1022d_query_input_grounding(2), "IGNDD? 2");
        assert_eq!(oe1022d_set_input_coupling(2, 1), "ICPLD 2,1");
        assert_eq!(oe1022d_query_input_coupling(2), "ICPLD? 2");
        assert_eq!(oe1022d_set_line_notch_filter(2, 0), "ILIND 2,0");
        assert_eq!(oe1022d_query_line_notch_filter(2), "ILIND? 2");
        assert_eq!(oe1022d_set_dynamic_reserve(2, 1), "RMODD 2,1");
        assert_eq!(oe1022d_query_dynamic_reserve(2), "RMODD? 2");
        assert_eq!(oe1022d_set_sensitivity_index(2, 24), "SENSD 2,24");
        assert_eq!(oe1022d_query_sensitivity_index(2), "SENSD? 2");
        assert_eq!(oe1022d_set_time_constant_index(2, 9), "OFLTD 2,9");
        assert_eq!(oe1022d_query_time_constant_index(2), "OFLTD? 2");
        assert_eq!(oe1022d_set_filter_slope(2, 1), "OFSLD 2,1");
        assert_eq!(oe1022d_query_filter_slope(2), "OFSLD? 2");
        assert_eq!(oe1022d_set_sync_filter(2, 0), "SYNCD 2,0");
        assert_eq!(oe1022d_query_sync_filter(2), "SYNCD? 2");
        assert_eq!(oe1022d_set_harmonic(2, 1, 1), "HARMD 2,1,1");
        assert_eq!(oe1022d_query_harmonic(2, 1), "HARMD? 2,1");
        assert_eq!(oe1022d_set_harmonic(2, 2, 1), "HARMD 2,2,1");
        assert_eq!(oe1022d_query_harmonic(2, 2), "HARMD? 2,2");
    }

    #[test]
    fn sine_output_helpers_keep_manual_shape() {
        assert_eq!(oe1022d_set_sine_output_mode(2, 0), "SWVTD 2,0");
        assert_eq!(oe1022d_query_sine_output_mode(2), "SWVTD? 2");
        assert_eq!(oe1022d_set_sine_output_voltage_vrms(2, 1.0), "SLVLD 2,1");
        assert_eq!(oe1022d_query_sine_output_voltage(2), "SLVLD? 2");
    }

    #[test]
    fn rall_query_is_exact() {
        assert_eq!(oe1022d_rall_query(), "RALL?");
    }
}
