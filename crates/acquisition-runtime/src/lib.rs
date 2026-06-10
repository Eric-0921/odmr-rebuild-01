//! 最小 runtime 原型 crate。
//!
//! 这层先把第一版 runtime 的核心事实模型钉死：
//! - 固定 profile 与 point 变量分离
//! - baseline / point / segment / quality artifact 结构固定
//! - `RALL?` run 级 collector 的 ring buffer 行为固定
//!
//! 这层暂时不直接负责真实硬件 I/O。真实设备连接和命令发送仍由 CLI 侧驱动，
//! 这里提供的是运行时协议、数据结构和可测试的窗口/质量逻辑。

use serde::{Deserialize, Serialize};
use std::collections::VecDeque;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct Smb100aFixedProfile {
    pub modulation_enabled: bool,
    pub fm_enabled: bool,
    pub fm_source: String,
    pub fm_mode: String,
    pub fm_deviation_hz: f64,
    pub lf_output_enabled: bool,
    pub lf_voltage_mv: f64,
    pub lf_frequency_hz: f64,
    pub lf_shape: String,
    pub lf_source_impedance: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct SmbSweepDefaults {
    pub start_hz: f64,
    pub stop_hz: f64,
    pub step_hz: f64,
    pub dwell_ms: u64,
    pub power_dbm: f64,
    pub sweep_mode: String,
    pub spacing: String,
    pub shape: String,
    pub trigger_source: String,
    pub output_voltage_start_v: f64,
    pub output_voltage_stop_v: f64,
    pub rf_output_enabled: bool,
}

impl SmbSweepDefaults {
    pub fn apply_override(&self, override_spec: Option<&SmbSweepOverride>) -> ResolvedSmbSweep {
        let Some(override_spec) = override_spec else {
            return ResolvedSmbSweep {
                start_hz: self.start_hz,
                stop_hz: self.stop_hz,
                step_hz: self.step_hz,
                dwell_ms: self.dwell_ms,
                power_dbm: self.power_dbm,
                sweep_mode: self.sweep_mode.clone(),
                spacing: self.spacing.clone(),
                shape: self.shape.clone(),
                trigger_source: self.trigger_source.clone(),
                output_voltage_start_v: self.output_voltage_start_v,
                output_voltage_stop_v: self.output_voltage_stop_v,
                rf_output_enabled: self.rf_output_enabled,
            };
        };

        ResolvedSmbSweep {
            start_hz: override_spec.start_hz.unwrap_or(self.start_hz),
            stop_hz: override_spec.stop_hz.unwrap_or(self.stop_hz),
            step_hz: override_spec.step_hz.unwrap_or(self.step_hz),
            dwell_ms: override_spec.dwell_ms.unwrap_or(self.dwell_ms),
            power_dbm: override_spec.power_dbm.unwrap_or(self.power_dbm),
            sweep_mode: override_spec
                .sweep_mode
                .clone()
                .unwrap_or_else(|| self.sweep_mode.clone()),
            spacing: override_spec
                .spacing
                .clone()
                .unwrap_or_else(|| self.spacing.clone()),
            shape: override_spec
                .shape
                .clone()
                .unwrap_or_else(|| self.shape.clone()),
            trigger_source: override_spec
                .trigger_source
                .clone()
                .unwrap_or_else(|| self.trigger_source.clone()),
            output_voltage_start_v: override_spec
                .output_voltage_start_v
                .unwrap_or(self.output_voltage_start_v),
            output_voltage_stop_v: override_spec
                .output_voltage_stop_v
                .unwrap_or(self.output_voltage_stop_v),
            rf_output_enabled: override_spec
                .rf_output_enabled
                .unwrap_or(self.rf_output_enabled),
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ResolvedSmbSweep {
    pub start_hz: f64,
    pub stop_hz: f64,
    pub step_hz: f64,
    pub dwell_ms: u64,
    pub power_dbm: f64,
    pub sweep_mode: String,
    pub spacing: String,
    pub shape: String,
    pub trigger_source: String,
    pub output_voltage_start_v: f64,
    pub output_voltage_stop_v: f64,
    pub rf_output_enabled: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Default)]
pub struct SmbSweepOverride {
    #[serde(default)]
    pub start_hz: Option<f64>,
    #[serde(default)]
    pub stop_hz: Option<f64>,
    #[serde(default)]
    pub step_hz: Option<f64>,
    #[serde(default)]
    pub dwell_ms: Option<u64>,
    #[serde(default)]
    pub power_dbm: Option<f64>,
    #[serde(default)]
    pub sweep_mode: Option<String>,
    #[serde(default)]
    pub spacing: Option<String>,
    #[serde(default)]
    pub shape: Option<String>,
    #[serde(default)]
    pub trigger_source: Option<String>,
    #[serde(default)]
    pub output_voltage_start_v: Option<f64>,
    #[serde(default)]
    pub output_voltage_stop_v: Option<f64>,
    #[serde(default)]
    pub rf_output_enabled: Option<bool>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct Smb100aRunProfile {
    pub profile_id: String,
    pub command_settle_ms: u64,
    pub error_check_after_write: bool,
    pub fixed: Smb100aFixedProfile,
    pub default_sweep: SmbSweepDefaults,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct Oe1022dFixedProfile {
    pub channel: u8,
    pub input_source: u8,
    pub input_grounding: u8,
    pub input_coupling: u8,
    pub line_notch_filter: u8,
    pub reference_source: u8,
    pub reference_slope: u8,
    pub phase_deg: f64,
    pub harmonic_1: u16,
    pub harmonic_2: u16,
    pub dynamic_reserve: u8,
    pub sensitivity_index: u8,
    pub time_constant_index: u8,
    pub filter_slope: u8,
    pub sync_filter: u8,
    pub sine_output_mode: u8,
    pub sine_output_voltage_vrms: f64,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct CollectorConfig {
    pub poll_interval_ms: u64,
    pub frame_exact_bytes: usize,
    pub frame_max_bytes: usize,
    pub ring_capacity_frames: usize,
    pub guard_margin_ms: u64,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct Oe1022dRunProfile {
    pub profile_id: String,
    pub command_settle_ms: u64,
    pub fixed: Oe1022dFixedProfile,
    pub collector: CollectorConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct MagBaselinePolicy {
    pub baseline_current_a: [f64; 3],
    pub settle_ms: u64,
    pub readback_samples: u32,
    pub settle_tolerance_a: f64,
    #[serde(default)]
    pub voltage_v: Option<f64>,
    #[serde(default)]
    pub voltage_protection_v: Option<f64>,
    pub output_enabled: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunQualityThresholds {
    pub min_frames: usize,
    pub max_timeout_count: usize,
    pub max_duplicate_ratio: f64,
    pub max_last_frame_age_ms: u64,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct CalibrationProfile {
    pub calibration_id: String,
    pub current_offset_a: [f64; 3],
    pub current_per_nt: [[f64; 3]; 3],
}

impl CalibrationProfile {
    pub fn delta_current_a(&self, target_b_nt: [f64; 3]) -> [f64; 3] {
        let mut out = self.current_offset_a;
        for (axis_index, row) in self.current_per_nt.iter().enumerate() {
            out[axis_index] += row[0] * target_b_nt[0];
            out[axis_index] += row[1] * target_b_nt[1];
            out[axis_index] += row[2] * target_b_nt[2];
        }
        out
    }

    pub fn target_current_a(
        &self,
        baseline_current_a: [f64; 3],
        target_b_nt: [f64; 3],
    ) -> [f64; 3] {
        let delta = self.delta_current_a(target_b_nt);
        [
            baseline_current_a[0] + delta[0],
            baseline_current_a[1] + delta[1],
            baseline_current_a[2] + delta[2],
        ]
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunPointPlan {
    pub point_id: String,
    pub target_b_nt: [f64; 3],
    #[serde(default)]
    pub smb_override: Option<SmbSweepOverride>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct AcquisitionRunPlan {
    pub run_id: String,
    pub operator: String,
    pub acquisition_window_ms: u64,
    pub point_settle_ms: u64,
    pub failure_policy: String,
    pub mag_baseline_policy: MagBaselinePolicy,
    pub quality_thresholds: RunQualityThresholds,
    pub points: Vec<RunPointPlan>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunManifest {
    pub schema_version: u32,
    pub run_id: String,
    pub created_at: String,
    pub operator: String,
    pub station_id: String,
    pub runtime_version: String,
    pub calibration_id: String,
    pub status: String,
    pub smb_profile_id: String,
    pub oe_profile_id: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct BaselineAxisSnapshot {
    pub axis: String,
    pub baseline_setpoint_a: f64,
    pub measured_current_a: Vec<f64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct BaselineSnapshot {
    pub schema_version: u32,
    pub baseline_locked_at: String,
    pub settle_ms: u64,
    pub readback_samples: u32,
    pub settle_tolerance_a: f64,
    pub axes: Vec<BaselineAxisSnapshot>,
}

impl BaselineSnapshot {
    pub fn baseline_current_a(&self) -> [f64; 3] {
        let mut out = [0.0_f64; 3];
        for (index, axis) in self.axes.iter().take(3).enumerate() {
            out[index] = axis.baseline_setpoint_a;
        }
        out
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct SettleRecord {
    pub policy: String,
    pub started_at: String,
    pub settled_at: String,
    pub status: String,
    pub measured_current_a: [f64; 3],
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointRecord {
    pub schema_version: u32,
    pub run_id: String,
    pub point_id: String,
    pub index: usize,
    pub target_b_nt: [f64; 3],
    pub baseline_current_a: [f64; 3],
    pub calibrated_delta_current_a: [f64; 3],
    pub target_current_a: [f64; 3],
    pub rf: ResolvedSmbSweep,
    pub settle: SettleRecord,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct SegmentRecord {
    pub schema_version: u32,
    pub run_id: String,
    pub segment_id: String,
    pub point_id: String,
    pub source: String,
    pub start_ts: String,
    pub end_ts: String,
    pub start_monotonic_ns: u64,
    pub end_monotonic_ns: u64,
    pub raw_file: String,
    pub raw_offset_start: u64,
    pub raw_offset_end: u64,
    pub frame_seq_start: Option<u64>,
    pub frame_seq_end: Option<u64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct QualityRecord {
    pub schema_version: u32,
    pub run_id: String,
    pub point_id: String,
    pub segment_id: String,
    pub frames_total: usize,
    pub frames_unique: usize,
    pub duplicate_count: usize,
    pub duplicate_ratio: f64,
    pub timeout_count: usize,
    pub last_frame_age_ms: u64,
    pub min_frames: usize,
    pub quality_status: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct SummaryRecord {
    pub run_id: String,
    pub status: String,
    pub points_total: usize,
    pub points_passed: usize,
    pub points_failed: usize,
    pub frames_total: u64,
    pub started_at: String,
    pub ended_at: String,
    #[serde(default)]
    pub failure: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct EventRecord {
    pub ts: String,
    pub monotonic_ns: u64,
    pub event: String,
    pub run_id: String,
    #[serde(default)]
    pub point_id: Option<String>,
    #[serde(default)]
    pub device: Option<String>,
    pub phase: String,
    #[serde(default)]
    pub data: serde_json::Value,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct FrameIndexRecord {
    pub frame_seq: u64,
    pub ts: String,
    pub monotonic_ns: u64,
    pub raw_offset: u64,
    pub raw_len: usize,
    pub parse_status: String,
    #[serde(default)]
    pub duplicate_of: Option<u64>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct CollectorFrame {
    pub frame_seq: u64,
    pub ts: String,
    pub monotonic_ns: u64,
    pub raw_offset: u64,
    pub payload: Vec<u8>,
    pub duplicate_of: Option<u64>,
}

pub const RALL_FRAME_BYTES: usize = 12288;
pub const RALL_PARAM_COUNT: usize = 20;
pub const RALL_SAMPLE_COUNT: usize = 50;
const RALL_PARAM_BLOCK_BYTES: usize = RALL_SAMPLE_COUNT * 8;

pub const RALL_FIELD_ORDER: [&str; RALL_PARAM_COUNT] = [
    "A-X", "A-Y", "A-Freq", "A-Noise", "A-Xh1", "A-Yh1", "A-Xh2", "A-Yh2", "B-X", "B-Y", "B-Freq",
    "B-Noise", "B-Xh1", "B-Yh1", "B-Xh2", "B-Yh2", "AUXADC1", "AUXADC2", "AUXADC3", "AUXADC4",
];

#[derive(Debug, Clone, PartialEq)]
pub enum MinimalRallParseError {
    WrongLength {
        expected: usize,
        actual: usize,
    },
    NonFiniteValue {
        field: &'static str,
        sample_index: usize,
    },
}

impl std::fmt::Display for MinimalRallParseError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::WrongLength { expected, actual } => {
                write!(f, "RALL 帧长度错误: expected={expected}, actual={actual}")
            }
            Self::NonFiniteValue {
                field,
                sample_index,
            } => {
                write!(f, "RALL 字段 {field} 在 sample {sample_index} 出现非有限值")
            }
        }
    }
}

impl std::error::Error for MinimalRallParseError {}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct MinimalRallStatus {
    #[serde(default)]
    pub b_ref_source_code: Option<u8>,
    #[serde(default)]
    pub b_ref_slope_code: Option<u8>,
    #[serde(default)]
    pub b_ref_current_freq_hz: Option<f64>,
    #[serde(default)]
    pub b_input_overload: Option<bool>,
    #[serde(default)]
    pub b_gain_overload: Option<bool>,
    #[serde(default)]
    pub b_pll_locked: Option<bool>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct MinimalRallFrame {
    pub measurement_matrix: Vec<Vec<f64>>,
    pub status: MinimalRallStatus,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldRecord {
    pub schema_version: u32,
    pub run_id: String,
    pub point_id: String,
    pub segment_id: String,
    pub frames_parsed: usize,
    pub samples_total: usize,
    pub matrix_shape: [usize; 2],
    pub measurement_field_order: Vec<String>,
    pub b_x_mv: Vec<f64>,
    pub b_y_mv: Vec<f64>,
    pub b_freq_hz: Vec<f64>,
    pub b_noise_mv: Vec<f64>,
    pub aux_adc1_v: Vec<f64>,
    pub aux_adc2_v: Vec<f64>,
    pub aux_adc3_v: Vec<f64>,
    pub aux_adc4_v: Vec<f64>,
    pub b_x_mean_mv: f64,
    pub b_y_mean_mv: f64,
    pub b_freq_mean_hz: f64,
    pub b_noise_mean_mv: f64,
    pub aux_adc1_mean_v: f64,
    pub aux_adc2_mean_v: f64,
    pub aux_adc3_mean_v: f64,
    pub aux_adc4_mean_v: f64,
    pub b_pll_locked_frames: usize,
    pub b_pll_locked_ratio: f64,
    pub b_input_overload_frames: usize,
    pub b_input_overload_ratio: f64,
    pub b_gain_overload_frames: usize,
    pub b_gain_overload_ratio: f64,
    #[serde(default)]
    pub last_b_ref_source_code: Option<u8>,
    #[serde(default)]
    pub last_b_ref_slope_code: Option<u8>,
    #[serde(default)]
    pub last_b_ref_current_freq_hz: Option<f64>,
}

impl CollectorFrame {
    pub fn raw_len(&self) -> usize {
        self.payload.len()
    }

    pub fn index_record(&self) -> FrameIndexRecord {
        FrameIndexRecord {
            frame_seq: self.frame_seq,
            ts: self.ts.clone(),
            monotonic_ns: self.monotonic_ns,
            raw_offset: self.raw_offset,
            raw_len: self.raw_len(),
            parse_status: "ok".to_string(),
            duplicate_of: self.duplicate_of,
        }
    }
}

pub fn parse_rall_frame_minimal(bytes: &[u8]) -> Result<MinimalRallFrame, MinimalRallParseError> {
    if bytes.len() != RALL_FRAME_BYTES {
        return Err(MinimalRallParseError::WrongLength {
            expected: RALL_FRAME_BYTES,
            actual: bytes.len(),
        });
    }

    let mut measurement_matrix = Vec::with_capacity(RALL_PARAM_COUNT);
    for (field_index, field_name) in RALL_FIELD_ORDER.iter().enumerate() {
        let start = field_index * RALL_PARAM_BLOCK_BYTES;
        let end = start + RALL_PARAM_BLOCK_BYTES;
        let mut samples = Vec::with_capacity(RALL_SAMPLE_COUNT);
        for sample_index in 0..RALL_SAMPLE_COUNT {
            let sample_start = start + sample_index * 8;
            let sample_end = sample_start + 8;
            let chunk: [u8; 8] = bytes[sample_start..sample_end].try_into().unwrap();
            let value = f64::from_bits(u64::from_be_bytes(chunk));
            if !value.is_finite() {
                return Err(MinimalRallParseError::NonFiniteValue {
                    field: field_name,
                    sample_index,
                });
            }
            samples.push(value);
        }
        debug_assert_eq!(end - start, RALL_PARAM_BLOCK_BYTES);
        measurement_matrix.push(samples);
    }

    Ok(MinimalRallFrame {
        measurement_matrix,
        status: MinimalRallStatus {
            b_ref_source_code: read_rall_u8(bytes, 8504),
            b_ref_slope_code: read_rall_u8(bytes, 8521),
            b_ref_current_freq_hz: read_rall_f64(bytes, 8505),
            b_input_overload: read_rall_u8(bytes, 8779).map(|value| value != 0),
            b_gain_overload: read_rall_u8(bytes, 8780).map(|value| value != 0),
            b_pll_locked: read_rall_u8(bytes, 8781).map(|value| value != 0),
        },
    })
}

pub fn build_point_field_record(
    run_id: &str,
    point_id: &str,
    segment_id: &str,
    frames: &[CollectorFrame],
) -> Result<PointFieldRecord, MinimalRallParseError> {
    let mut b_x_mv = Vec::new();
    let mut b_y_mv = Vec::new();
    let mut b_freq_hz = Vec::new();
    let mut b_noise_mv = Vec::new();
    let mut aux_adc1_v = Vec::new();
    let mut aux_adc2_v = Vec::new();
    let mut aux_adc3_v = Vec::new();
    let mut aux_adc4_v = Vec::new();
    let mut pll_locked_frames = 0_usize;
    let mut input_overload_frames = 0_usize;
    let mut gain_overload_frames = 0_usize;
    let mut last_status = MinimalRallStatus {
        b_ref_source_code: None,
        b_ref_slope_code: None,
        b_ref_current_freq_hz: None,
        b_input_overload: None,
        b_gain_overload: None,
        b_pll_locked: None,
    };

    for frame in frames {
        let parsed = parse_rall_frame_minimal(&frame.payload)?;
        last_status = parsed.status.clone();
        b_x_mv.extend_from_slice(&parsed.measurement_matrix[8]);
        b_y_mv.extend_from_slice(&parsed.measurement_matrix[9]);
        b_freq_hz.extend_from_slice(&parsed.measurement_matrix[10]);
        b_noise_mv.extend_from_slice(&parsed.measurement_matrix[11]);
        aux_adc1_v.extend_from_slice(&parsed.measurement_matrix[16]);
        aux_adc2_v.extend_from_slice(&parsed.measurement_matrix[17]);
        aux_adc3_v.extend_from_slice(&parsed.measurement_matrix[18]);
        aux_adc4_v.extend_from_slice(&parsed.measurement_matrix[19]);
        if parsed.status.b_pll_locked.unwrap_or(false) {
            pll_locked_frames += 1;
        }
        if parsed.status.b_input_overload.unwrap_or(false) {
            input_overload_frames += 1;
        }
        if parsed.status.b_gain_overload.unwrap_or(false) {
            gain_overload_frames += 1;
        }
    }

    let frames_parsed = frames.len();
    let samples_total = frames_parsed * RALL_SAMPLE_COUNT;
    let ratio = |count: usize| -> f64 {
        if frames_parsed == 0 {
            0.0
        } else {
            count as f64 / frames_parsed as f64
        }
    };

    Ok(PointFieldRecord {
        schema_version: 1,
        run_id: run_id.to_string(),
        point_id: point_id.to_string(),
        segment_id: segment_id.to_string(),
        frames_parsed,
        samples_total,
        matrix_shape: [RALL_PARAM_COUNT, samples_total],
        measurement_field_order: RALL_FIELD_ORDER
            .iter()
            .map(|name| (*name).to_string())
            .collect(),
        b_x_mv: b_x_mv.clone(),
        b_y_mv: b_y_mv.clone(),
        b_freq_hz: b_freq_hz.clone(),
        b_noise_mv: b_noise_mv.clone(),
        aux_adc1_v: aux_adc1_v.clone(),
        aux_adc2_v: aux_adc2_v.clone(),
        aux_adc3_v: aux_adc3_v.clone(),
        aux_adc4_v: aux_adc4_v.clone(),
        b_x_mean_mv: mean(&b_x_mv),
        b_y_mean_mv: mean(&b_y_mv),
        b_freq_mean_hz: mean(&b_freq_hz),
        b_noise_mean_mv: mean(&b_noise_mv),
        aux_adc1_mean_v: mean(&aux_adc1_v),
        aux_adc2_mean_v: mean(&aux_adc2_v),
        aux_adc3_mean_v: mean(&aux_adc3_v),
        aux_adc4_mean_v: mean(&aux_adc4_v),
        b_pll_locked_frames: pll_locked_frames,
        b_pll_locked_ratio: ratio(pll_locked_frames),
        b_input_overload_frames: input_overload_frames,
        b_input_overload_ratio: ratio(input_overload_frames),
        b_gain_overload_frames: gain_overload_frames,
        b_gain_overload_ratio: ratio(gain_overload_frames),
        last_b_ref_source_code: last_status.b_ref_source_code,
        last_b_ref_slope_code: last_status.b_ref_slope_code,
        last_b_ref_current_freq_hz: last_status.b_ref_current_freq_hz,
    })
}

fn read_rall_u8(bytes: &[u8], offset: usize) -> Option<u8> {
    bytes.get(offset).copied()
}

fn read_rall_f64(bytes: &[u8], offset: usize) -> Option<f64> {
    let chunk: [u8; 8] = bytes.get(offset..offset + 8)?.try_into().ok()?;
    Some(f64::from_bits(u64::from_be_bytes(chunk)))
}

fn mean(values: &[f64]) -> f64 {
    if values.is_empty() {
        return 0.0;
    }
    values.iter().sum::<f64>() / values.len() as f64
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub struct CollectorCursor {
    pub next_frame_seq: u64,
    pub next_raw_offset: u64,
}

#[derive(Debug, Clone, PartialEq)]
pub struct FrameRingBuffer {
    capacity_frames: usize,
    frames: VecDeque<CollectorFrame>,
    next_frame_seq: u64,
    next_raw_offset: u64,
}

impl FrameRingBuffer {
    pub fn new(capacity_frames: usize) -> Self {
        Self {
            capacity_frames,
            frames: VecDeque::with_capacity(capacity_frames.max(1)),
            next_frame_seq: 0,
            next_raw_offset: 0,
        }
    }

    pub fn push(&mut self, ts: String, monotonic_ns: u64, payload: Vec<u8>) -> CollectorFrame {
        let frame = CollectorFrame {
            frame_seq: self.next_frame_seq,
            ts,
            monotonic_ns,
            raw_offset: self.next_raw_offset,
            duplicate_of: None,
            payload,
        };
        self.next_frame_seq += 1;
        self.next_raw_offset += frame.raw_len() as u64;

        if self.frames.len() == self.capacity_frames {
            self.frames.pop_front();
        }
        self.frames.push_back(frame.clone());
        frame
    }

    pub fn cursor(&self) -> CollectorCursor {
        CollectorCursor {
            next_frame_seq: self.next_frame_seq,
            next_raw_offset: self.next_raw_offset,
        }
    }

    pub fn pull_window(
        &self,
        start_monotonic_ns: u64,
        end_monotonic_ns: u64,
    ) -> Vec<CollectorFrame> {
        self.frames
            .iter()
            .filter(|frame| {
                frame.monotonic_ns >= start_monotonic_ns && frame.monotonic_ns <= end_monotonic_ns
            })
            .cloned()
            .collect()
    }

    pub fn total_retained_frames(&self) -> usize {
        self.frames.len()
    }
}

pub fn compute_quality_record(
    run_id: &str,
    point_id: &str,
    segment_id: &str,
    segment_end_monotonic_ns: u64,
    frames: &[CollectorFrame],
    thresholds: &RunQualityThresholds,
    timeout_count: usize,
) -> QualityRecord {
    let frames_total = frames.len();
    let duplicate_count = frames
        .iter()
        .filter(|frame| frame.duplicate_of.is_some())
        .count();
    let frames_unique = frames_total.saturating_sub(duplicate_count);
    let duplicate_ratio = if frames_total == 0 {
        0.0
    } else {
        duplicate_count as f64 / frames_total as f64
    };
    let last_frame_age_ms = frames
        .last()
        .map(|frame| segment_end_monotonic_ns.saturating_sub(frame.monotonic_ns) / 1_000_000)
        .unwrap_or(u64::MAX);

    let quality_status = if frames_total == 0 {
        "failed_no_frames".to_string()
    } else if frames_total < thresholds.min_frames {
        "failed_min_frames".to_string()
    } else if timeout_count > thresholds.max_timeout_count
        || duplicate_ratio > thresholds.max_duplicate_ratio
        || last_frame_age_ms > thresholds.max_last_frame_age_ms
    {
        "failed_timeouts".to_string()
    } else {
        "passed".to_string()
    };

    QualityRecord {
        schema_version: 1,
        run_id: run_id.to_string(),
        point_id: point_id.to_string(),
        segment_id: segment_id.to_string(),
        frames_total,
        frames_unique,
        duplicate_count,
        duplicate_ratio,
        timeout_count,
        last_frame_age_ms,
        min_frames: thresholds.min_frames,
        quality_status,
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn sample_defaults() -> SmbSweepDefaults {
        SmbSweepDefaults {
            start_hz: 2.80e9,
            stop_hz: 2.90e9,
            step_hz: 5.0e5,
            dwell_ms: 500,
            power_dbm: -20.0,
            sweep_mode: "AUTO".to_string(),
            spacing: "LIN".to_string(),
            shape: "SAWT".to_string(),
            trigger_source: "AUTO".to_string(),
            output_voltage_start_v: 0.0,
            output_voltage_stop_v: 3.0,
            rf_output_enabled: true,
        }
    }

    #[test]
    fn smb_override_only_changes_whitelisted_fields() {
        let defaults = sample_defaults();
        let resolved = defaults.apply_override(Some(&SmbSweepOverride {
            stop_hz: Some(2.91e9),
            power_dbm: Some(-10.0),
            ..SmbSweepOverride::default()
        }));

        assert_eq!(resolved.start_hz, defaults.start_hz);
        assert_eq!(resolved.stop_hz, 2.91e9);
        assert_eq!(resolved.power_dbm, -10.0);
        assert_eq!(resolved.step_hz, defaults.step_hz);
        assert_eq!(resolved.shape, defaults.shape);
    }

    #[test]
    fn calibration_maps_target_b_to_target_current() {
        let calibration = CalibrationProfile {
            calibration_id: "diag_1ma_per_nt".to_string(),
            current_offset_a: [0.0, 0.0, 0.0],
            current_per_nt: [[0.001, 0.0, 0.0], [0.0, 0.001, 0.0], [0.0, 0.0, 0.001]],
        };

        let delta = calibration.delta_current_a([10.0, 20.0, 30.0]);
        let target = calibration.target_current_a([0.1, 0.2, 0.3], [10.0, 20.0, 30.0]);

        assert_eq!(delta, [0.01, 0.02, 0.03]);
        assert_eq!(target, [0.11, 0.22, 0.32999999999999996]);
    }

    #[test]
    fn ring_buffer_retains_latest_frames_only() {
        let mut ring = FrameRingBuffer::new(2);
        ring.push("t1".to_string(), 10, vec![1, 2, 3]);
        ring.push("t2".to_string(), 20, vec![4, 5]);
        ring.push("t3".to_string(), 30, vec![6]);

        let window = ring.pull_window(0, 100);
        assert_eq!(window.len(), 2);
        assert_eq!(window[0].frame_seq, 1);
        assert_eq!(window[1].frame_seq, 2);
        assert_eq!(ring.cursor().next_raw_offset, 6);
    }

    #[test]
    fn quality_fails_when_window_has_no_frames() {
        let thresholds = RunQualityThresholds {
            min_frames: 2,
            max_timeout_count: 0,
            max_duplicate_ratio: 0.1,
            max_last_frame_age_ms: 100,
        };

        let quality = compute_quality_record("run_1", "p1", "seg1", 10, &[], &thresholds, 0);
        assert_eq!(quality.quality_status, "failed_no_frames");
    }

    #[test]
    fn quality_passes_when_thresholds_are_met() {
        let thresholds = RunQualityThresholds {
            min_frames: 2,
            max_timeout_count: 0,
            max_duplicate_ratio: 0.5,
            max_last_frame_age_ms: 100,
        };
        let frames = vec![
            CollectorFrame {
                frame_seq: 0,
                ts: "t1".to_string(),
                monotonic_ns: 1_000_000,
                raw_offset: 0,
                payload: vec![1; 16],
                duplicate_of: None,
            },
            CollectorFrame {
                frame_seq: 1,
                ts: "t2".to_string(),
                monotonic_ns: 5_000_000,
                raw_offset: 16,
                payload: vec![2; 16],
                duplicate_of: None,
            },
        ];

        let quality =
            compute_quality_record("run_1", "p1", "seg1", 20_000_000, &frames, &thresholds, 0);
        assert_eq!(quality.quality_status, "passed");
        assert_eq!(quality.frames_total, 2);
        assert_eq!(quality.frames_unique, 2);
    }

    #[test]
    fn minimal_rall_parser_extracts_matrix_and_b_status() {
        let mut frame = vec![0_u8; RALL_FRAME_BYTES];
        write_f64(&mut frame, 0, 1.0);
        write_f64(&mut frame, 8, 2.0);
        write_f64(&mut frame, 8 * RALL_SAMPLE_COUNT * 8, 8.0);
        write_f64(&mut frame, 8 * RALL_SAMPLE_COUNT * 8 + 8, 9.0);
        write_f64(&mut frame, 10 * RALL_SAMPLE_COUNT * 8, 1234.0);
        write_f64(&mut frame, 11 * RALL_SAMPLE_COUNT * 8, 0.5);
        write_f64(&mut frame, 16 * RALL_SAMPLE_COUNT * 8, 3.3);
        write_f64(&mut frame, 8505, 500.0);
        frame[8504] = 2;
        frame[8521] = 1;
        frame[8779] = 0;
        frame[8780] = 1;
        frame[8781] = 1;

        let parsed = parse_rall_frame_minimal(&frame).unwrap();

        assert_eq!(parsed.measurement_matrix.len(), RALL_PARAM_COUNT);
        assert_eq!(parsed.measurement_matrix[0][0], 1.0);
        assert_eq!(parsed.measurement_matrix[0][1], 2.0);
        assert_eq!(parsed.measurement_matrix[8][0], 8.0);
        assert_eq!(parsed.measurement_matrix[8][1], 9.0);
        assert_eq!(parsed.status.b_ref_source_code, Some(2));
        assert_eq!(parsed.status.b_ref_slope_code, Some(1));
        assert_eq!(parsed.status.b_ref_current_freq_hz, Some(500.0));
        assert_eq!(parsed.status.b_input_overload, Some(false));
        assert_eq!(parsed.status.b_gain_overload, Some(true));
        assert_eq!(parsed.status.b_pll_locked, Some(true));
    }

    #[test]
    fn point_field_record_aggregates_selected_fields() {
        let mut payload = vec![0_u8; RALL_FRAME_BYTES];
        write_f64(&mut payload, 8 * RALL_PARAM_BLOCK_BYTES, 1.5);
        write_f64(&mut payload, 9 * RALL_PARAM_BLOCK_BYTES, 2.5);
        write_f64(&mut payload, 10 * RALL_PARAM_BLOCK_BYTES, 500.0);
        write_f64(&mut payload, 11 * RALL_PARAM_BLOCK_BYTES, 0.25);
        write_f64(&mut payload, 16 * RALL_PARAM_BLOCK_BYTES, 3.1);
        payload[8781] = 1;

        let frames = vec![CollectorFrame {
            frame_seq: 0,
            ts: "t1".to_string(),
            monotonic_ns: 1_000_000,
            raw_offset: 0,
            payload,
            duplicate_of: None,
        }];

        let record = build_point_field_record("run_1", "p1", "seg1", &frames).unwrap();

        assert_eq!(record.frames_parsed, 1);
        assert_eq!(record.samples_total, 50);
        assert_eq!(record.matrix_shape, [20, 50]);
        assert_eq!(record.b_x_mv.first().copied(), Some(1.5));
        assert_eq!(record.b_y_mv.first().copied(), Some(2.5));
        assert_eq!(record.b_freq_hz.first().copied(), Some(500.0));
        assert_eq!(record.b_noise_mv.first().copied(), Some(0.25));
        assert_eq!(record.aux_adc1_v.first().copied(), Some(3.1));
        assert_eq!(record.b_pll_locked_ratio, 1.0);
    }

    fn write_f64(bytes: &mut [u8], offset: usize, value: f64) {
        let end = offset + 8;
        bytes[offset..end].copy_from_slice(&value.to_bits().to_be_bytes());
    }
}
