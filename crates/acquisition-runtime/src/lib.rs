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
use serde_json::{Map as JsonMap, Value as JsonValue};
use std::collections::VecDeque;
use std::fmt;
use std::sync::OnceLock;

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

    pub fn estimate(&self) -> Result<SweepEstimate, SweepEstimateError> {
        self.apply_override(None).estimate()
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

impl ResolvedSmbSweep {
    pub fn estimate(&self) -> Result<SweepEstimate, SweepEstimateError> {
        if self.step_hz <= 0.0 {
            return Err(SweepEstimateError::InvalidStepHz(self.step_hz));
        }

        let span_hz = (self.stop_hz - self.start_hz).abs();
        let step_count = (span_hz / self.step_hz).round() as u64;
        let sweep_points = step_count.saturating_add(1);
        Ok(SweepEstimate {
            sweep_points,
            sweep_duration_ms: sweep_points.saturating_mul(self.dwell_ms),
        })
    }
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct SweepEstimate {
    pub sweep_points: u64,
    pub sweep_duration_ms: u64,
}

#[derive(Debug, Clone, PartialEq)]
pub enum SweepEstimateError {
    InvalidStepHz(f64),
}

impl fmt::Display for SweepEstimateError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::InvalidStepHz(step_hz) => {
                write!(f, "SMB sweep step_hz 必须大于 0，当前为 {step_hz}")
            }
        }
    }
}

impl std::error::Error for SweepEstimateError {}

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

impl Smb100aRunProfile {
    pub fn estimated_point_configuration_ms(&self) -> u64 {
        13_u64.saturating_mul(self.command_settle_ms)
    }
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
    #[serde(default = "default_rall_post_write_delay_ms")]
    pub rall_post_write_delay_ms: u64,
    #[serde(default = "default_rall_chunk_timeout_ms")]
    pub rall_chunk_timeout_ms: u64,
    #[serde(default = "default_rall_first_byte_deadline_ms")]
    pub rall_first_byte_deadline_ms: u64,
    #[serde(default = "default_rall_frame_deadline_ms")]
    pub rall_frame_deadline_ms: u64,
    #[serde(default = "default_zero_byte_retry_limit")]
    pub zero_byte_retry_limit: usize,
}

const fn default_rall_chunk_timeout_ms() -> u64 {
    5
}

const fn default_rall_post_write_delay_ms() -> u64 {
    30
}

const fn default_rall_first_byte_deadline_ms() -> u64 {
    20
}

const fn default_rall_frame_deadline_ms() -> u64 {
    120
}

const fn default_zero_byte_retry_limit() -> usize {
    1
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct Oe1022dRunProfile {
    pub profile_id: String,
    pub command_settle_ms: u64,
    pub fixed: Oe1022dFixedProfile,
    pub collector: CollectorConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "snake_case")]
pub enum LaserBackgroundMode {
    OnBackground,
    OffBackground,
}

impl LaserBackgroundMode {
    pub fn as_str(&self) -> &'static str {
        match self {
            Self::OnBackground => "on_background",
            Self::OffBackground => "off_background",
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct LaserRunProfile {
    pub profile_id: String,
    pub mode: LaserBackgroundMode,
    pub power_mw: u16,
    pub settle_ms: u64,
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

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "snake_case")]
pub enum PlanSourceKind {
    ExplicitPoints,
    CartesianGrid,
}

impl PlanSourceKind {
    pub fn as_str(&self) -> &'static str {
        match self {
            Self::ExplicitPoints => "explicit_points",
            Self::CartesianGrid => "cartesian_grid",
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct CartesianGridAxesNt {
    pub x: Vec<f64>,
    pub y: Vec<f64>,
    pub z: Vec<f64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "snake_case")]
pub enum CartesianGridCycleMode {
    Raster,
    #[serde(rename = "bounce_1d_x")]
    Bounce1dX,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(tag = "kind", rename_all = "snake_case")]
pub enum CartesianGridStopCondition {
    FixedTotalPoints { total_points: usize },
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct CartesianGridPointSource {
    pub axes_nt: CartesianGridAxesNt,
    #[serde(default = "default_cartesian_order")]
    pub order: Vec<String>,
    pub cycle_mode: CartesianGridCycleMode,
    pub stop_condition: CartesianGridStopCondition,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
#[serde(tag = "kind", rename_all = "snake_case")]
pub enum PointSource {
    CartesianGrid {
        axes_nt: CartesianGridAxesNt,
        #[serde(default = "default_cartesian_order")]
        order: Vec<String>,
        cycle_mode: CartesianGridCycleMode,
        stop_condition: CartesianGridStopCondition,
    },
}

impl PointSource {
    fn into_cartesian_grid(self) -> CartesianGridPointSource {
        match self {
            Self::CartesianGrid {
                axes_nt,
                order,
                cycle_mode,
                stop_condition,
            } => CartesianGridPointSource {
                axes_nt,
                order,
                cycle_mode,
                stop_condition,
            },
        }
    }
}

#[derive(Debug, Clone, PartialEq)]
pub enum RunPlanResolveError {
    EmptyPointPlan,
    InvalidCartesianOrder(Vec<String>),
    MissingAxisValues(&'static str),
    Bounce1dXRequiresSingletonYZ { y_len: usize, z_len: usize },
    FixedTotalPointsMustBePositive,
    SweepEstimate(SweepEstimateError),
}

impl fmt::Display for RunPlanResolveError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::EmptyPointPlan => write!(f, "plan 没有可执行 point"),
            Self::InvalidCartesianOrder(order) => {
                write!(
                    f,
                    "cartesian_grid.order 目前只支持 [\"x\", \"y\", \"z\"]，当前为 {order:?}"
                )
            }
            Self::MissingAxisValues(axis) => {
                write!(f, "cartesian_grid 轴 {axis} 没有任何取值")
            }
            Self::Bounce1dXRequiresSingletonYZ { y_len, z_len } => write!(
                f,
                "bounce_1d_x 只允许 y/z 单元素，当前 y_len={y_len}, z_len={z_len}"
            ),
            Self::FixedTotalPointsMustBePositive => {
                write!(f, "fixed_total_points 必须大于 0")
            }
            Self::SweepEstimate(err) => err.fmt(f),
        }
    }
}

impl std::error::Error for RunPlanResolveError {}

impl From<SweepEstimateError> for RunPlanResolveError {
    fn from(value: SweepEstimateError) -> Self {
        Self::SweepEstimate(value)
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ResolvedRunPlan {
    pub source_kind: PlanSourceKind,
    pub declared_point_count: usize,
    pub resolved_point_count: usize,
    #[serde(default)]
    pub fixed_total_points: Option<usize>,
    #[serde(default)]
    pub cycle_mode: Option<CartesianGridCycleMode>,
    #[serde(default)]
    pub estimated_sweep: Option<SweepEstimate>,
    #[serde(default)]
    pub estimated_point_duration_ms: Option<u64>,
    #[serde(default)]
    pub estimated_run_duration_ms: Option<u64>,
    pub points: Vec<RunPointPlan>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct AcquisitionRunPlan {
    pub run_id: String,
    pub operator: String,
    #[serde(default)]
    pub acquisition_window_ms: u64,
    pub point_settle_ms: u64,
    pub failure_policy: String,
    pub mag_baseline_policy: MagBaselinePolicy,
    pub quality_thresholds: RunQualityThresholds,
    #[serde(default)]
    pub point_source: Option<PointSource>,
    #[serde(default)]
    pub points: Vec<RunPointPlan>,
}

impl AcquisitionRunPlan {
    pub fn resolve_points(
        &self,
        smb_profile: &Smb100aRunProfile,
    ) -> Result<ResolvedRunPlan, RunPlanResolveError> {
        if let Some(point_source) = self.point_source.clone() {
            let point_source = point_source.into_cartesian_grid();
            validate_cartesian_order(&point_source.order)?;
            validate_axis_values(&point_source.axes_nt)?;

            let base_points = match point_source.cycle_mode {
                CartesianGridCycleMode::Raster => build_raster_points(&point_source.axes_nt),
                CartesianGridCycleMode::Bounce1dX => {
                    build_bounce_1d_x_points(&point_source.axes_nt)?
                }
            };

            let fixed_total_points = match point_source.stop_condition {
                CartesianGridStopCondition::FixedTotalPoints { total_points } => {
                    if total_points == 0 {
                        return Err(RunPlanResolveError::FixedTotalPointsMustBePositive);
                    }
                    total_points
                }
            };

            let resolved_points = repeat_points_to_total(&base_points, fixed_total_points);
            let estimated_sweep = smb_profile.default_sweep.estimate()?;
            let estimated_point_duration_ms = estimated_sweep
                .sweep_duration_ms
                .saturating_add(self.point_settle_ms)
                .saturating_add(smb_profile.estimated_point_configuration_ms());
            let estimated_run_duration_ms =
                estimated_point_duration_ms.saturating_mul(fixed_total_points as u64);

            return Ok(ResolvedRunPlan {
                source_kind: PlanSourceKind::CartesianGrid,
                declared_point_count: base_points.len(),
                resolved_point_count: resolved_points.len(),
                fixed_total_points: Some(fixed_total_points),
                cycle_mode: Some(point_source.cycle_mode),
                estimated_sweep: Some(estimated_sweep),
                estimated_point_duration_ms: Some(estimated_point_duration_ms),
                estimated_run_duration_ms: Some(estimated_run_duration_ms),
                points: resolved_points,
            });
        }

        if self.points.is_empty() {
            return Err(RunPlanResolveError::EmptyPointPlan);
        }

        Ok(ResolvedRunPlan {
            source_kind: PlanSourceKind::ExplicitPoints,
            declared_point_count: self.points.len(),
            resolved_point_count: self.points.len(),
            fixed_total_points: None,
            cycle_mode: None,
            estimated_sweep: None,
            estimated_point_duration_ms: None,
            estimated_run_duration_ms: None,
            points: self.points.clone(),
        })
    }

    pub fn resolve_points_without_smb_profile(
        &self,
    ) -> Result<ResolvedRunPlan, RunPlanResolveError> {
        if let Some(point_source) = self.point_source.clone() {
            let point_source = point_source.into_cartesian_grid();
            validate_cartesian_order(&point_source.order)?;
            validate_axis_values(&point_source.axes_nt)?;

            let base_points = match point_source.cycle_mode {
                CartesianGridCycleMode::Raster => build_raster_points(&point_source.axes_nt),
                CartesianGridCycleMode::Bounce1dX => {
                    build_bounce_1d_x_points(&point_source.axes_nt)?
                }
            };

            let fixed_total_points = match point_source.stop_condition {
                CartesianGridStopCondition::FixedTotalPoints { total_points } => {
                    if total_points == 0 {
                        return Err(RunPlanResolveError::FixedTotalPointsMustBePositive);
                    }
                    total_points
                }
            };

            let resolved_points = repeat_points_to_total(&base_points, fixed_total_points);
            return Ok(ResolvedRunPlan {
                source_kind: PlanSourceKind::CartesianGrid,
                declared_point_count: base_points.len(),
                resolved_point_count: resolved_points.len(),
                fixed_total_points: Some(fixed_total_points),
                cycle_mode: Some(point_source.cycle_mode),
                estimated_sweep: None,
                estimated_point_duration_ms: None,
                estimated_run_duration_ms: None,
                points: resolved_points,
            });
        }

        if self.points.is_empty() {
            return Err(RunPlanResolveError::EmptyPointPlan);
        }

        Ok(ResolvedRunPlan {
            source_kind: PlanSourceKind::ExplicitPoints,
            declared_point_count: self.points.len(),
            resolved_point_count: self.points.len(),
            fixed_total_points: None,
            cycle_mode: None,
            estimated_sweep: None,
            estimated_point_duration_ms: None,
            estimated_run_duration_ms: None,
            points: self.points.clone(),
        })
    }
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
    pub laser_profile_id: String,
    pub plan_source_kind: String,
    pub resolved_point_count: usize,
    #[serde(default)]
    pub estimated_run_duration_ms: Option<u64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PlanSnapshot {
    pub schema_version: u32,
    pub run_id: String,
    pub source_kind: String,
    pub declared_point_count: usize,
    pub resolved_point_count: usize,
    #[serde(default)]
    pub fixed_total_points: Option<usize>,
    #[serde(default)]
    pub cycle_mode: Option<CartesianGridCycleMode>,
    #[serde(default)]
    pub estimated_sweep: Option<SweepEstimate>,
    #[serde(default)]
    pub estimated_point_duration_ms: Option<u64>,
    #[serde(default)]
    pub estimated_run_duration_ms: Option<u64>,
    pub source_plan: AcquisitionRunPlan,
    pub resolved_points: Vec<RunPointPlan>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct BaselineAxisSnapshot {
    pub axis: String,
    #[serde(alias = "baseline_setpoint_a")]
    pub zero_offset_setpoint_a: f64,
    #[serde(alias = "measured_current_a")]
    pub zero_offset_measured_samples_a: Vec<f64>,
    #[serde(default)]
    pub locked_zero_offset_current_a: Option<f64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct BaselineSnapshot {
    pub schema_version: u32,
    #[serde(default = "default_baseline_mode")]
    pub mode: String,
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
            out[index] = axis
                .locked_zero_offset_current_a
                .unwrap_or(axis.zero_offset_setpoint_a);
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
    #[serde(default)]
    pub estimated_frames_expected: Option<usize>,
    #[serde(default)]
    pub frame_coverage_ratio: Option<f64>,
    #[serde(default = "default_collector_health_clean")]
    pub collector_health: String,
    #[serde(default)]
    pub timeout_budget_remaining: usize,
    pub quality_status: String,
}

fn default_collector_health_clean() -> String {
    "clean".to_string()
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
pub const RALL_MEASUREMENT_BYTES: usize = 8000;
pub const RALL_CONFIG_BYTES: usize = 1216;
pub const RALL_PADDING_BYTES: usize = 3072;
pub const RALL_PADDING_START: usize = RALL_MEASUREMENT_BYTES + RALL_CONFIG_BYTES;
const RALL_PARAM_BLOCK_BYTES: usize = RALL_SAMPLE_COUNT * 8;

pub const RALL_FIELD_ORDER: [&str; RALL_PARAM_COUNT] = [
    "A-X", "A-Y", "A-Freq", "A-Noise", "A-Xh1", "A-Yh1", "A-Xh2", "A-Yh2", "B-X", "B-Y", "B-Freq",
    "B-Noise", "B-Xh1", "B-Yh1", "B-Xh2", "B-Yh2", "AUXADC1", "AUXADC2", "AUXADC3", "AUXADC4",
];

const RALL_LAYOUT_MARKDOWN: &str = include_str!(
    "../../../docs/equipment_manual/oe1022d/05_oe1022d_rall_global_data_config_reading.md"
);

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RallMeasurementFieldSpec {
    pub index: usize,
    pub name: String,
    pub start: usize,
    pub end: usize,
    pub sample_count: usize,
    pub encoding: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RallScalarFieldSpec {
    pub index: usize,
    pub category: String,
    pub name: String,
    pub start: usize,
    pub end: usize,
    pub encoding: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RallReservedRangeSpec {
    pub start: usize,
    pub end: usize,
    pub reason: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RallLayoutSpec {
    pub schema_version: u32,
    pub source_document: String,
    pub measurement_fields: Vec<RallMeasurementFieldSpec>,
    pub scalar_fields: Vec<RallScalarFieldSpec>,
    pub reserved_ranges: Vec<RallReservedRangeSpec>,
    pub padding_start: usize,
    pub padding_end: usize,
    pub padding_must_be_zero: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ParsedScalarFieldRecord {
    pub index: usize,
    pub category: String,
    pub name: String,
    pub start: usize,
    pub end: usize,
    pub encoding: String,
    pub value: JsonValue,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ParsedFrameRecord {
    pub schema_version: u32,
    pub layout_version: String,
    pub frame_seq: u64,
    pub ts: String,
    pub monotonic_ns: u64,
    pub raw_offset: u64,
    pub raw_len: usize,
    pub transport_status: String,
    pub parse_status: String,
    pub padding_status: String,
    #[serde(default)]
    pub duplicate_hint: Option<u64>,
    #[serde(default)]
    pub parse_error: Option<String>,
    pub measurement_field_order: Vec<String>,
    #[serde(default)]
    pub measurement_matrix: Option<Vec<Vec<f64>>>,
    #[serde(default)]
    pub scalar_fields: Vec<ParsedScalarFieldRecord>,
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

#[derive(Debug, Clone, PartialEq)]
pub enum MinimalRallParseError {
    WrongLength { expected: usize, actual: usize },
    NonFiniteValue { field: String, sample_index: usize },
    LayoutSpec(String),
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
            Self::LayoutSpec(message) => write!(f, "RALL 布局规格错误: {message}"),
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
    pub samples_per_frame: usize,
    pub matrix_shape: [usize; 2],
    pub measurement_field_order: Vec<String>,
    pub measurement_field_keys: Vec<String>,
    pub field_summaries: Vec<PointFieldSummaryRecord>,
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
    pub sidecar: PointFieldSidecarRef,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldSummaryRecord {
    pub field_name: String,
    pub npz_key: String,
    pub mean: f64,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldSidecarRef {
    pub format: String,
    pub schema_version: u32,
    pub relative_path: String,
    pub manifest_relative_path: String,
    pub measurement_field_keys: Vec<String>,
    pub status_keys: Vec<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldSidecarData {
    pub schema_version: u32,
    pub matrix_shape: [usize; 2],
    pub samples_per_frame: usize,
    pub measurement_field_order: Vec<String>,
    pub measurement_field_keys: Vec<String>,
    pub measurement_fields: Vec<Vec<f64>>,
    pub frame_seq: Vec<u64>,
    pub duplicate_hint: Vec<i64>,
    pub b_ref_source_code: Vec<i16>,
    pub b_ref_slope_code: Vec<i16>,
    pub b_ref_current_freq_hz: Vec<f64>,
    pub b_input_overload: Vec<i8>,
    pub b_gain_overload: Vec<i8>,
    pub b_pll_locked: Vec<i8>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldBundle {
    pub metadata: PointFieldRecord,
    pub sidecar: PointFieldSidecarData,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointFieldSidecarManifest {
    pub schema_version: u32,
    pub run_id: String,
    pub point_id: String,
    pub segment_id: String,
    pub calibration_id: String,
    pub smb_profile_id: String,
    pub smb_command_settle_ms: u64,
    pub smb_error_check_after_write: bool,
    pub smb_fixed: Smb100aFixedProfile,
    pub oe_profile_id: String,
    pub laser_profile_id: String,
    pub target_b_nt: [f64; 3],
    pub baseline_current_a: [f64; 3],
    pub calibrated_delta_current_a: [f64; 3],
    pub target_current_a: [f64; 3],
    pub measured_current_a: [f64; 3],
    pub rf: ResolvedSmbSweep,
    pub oe_fixed: Oe1022dFixedProfile,
    pub oe_collector: CollectorConfig,
    pub laser_mode: String,
    pub laser_power_mw: u16,
    pub laser_settle_ms: u64,
}

impl CollectorFrame {
    pub fn raw_len(&self) -> usize {
        self.payload.len()
    }

    pub fn index_record(&self, parse_status: impl Into<String>) -> FrameIndexRecord {
        FrameIndexRecord {
            frame_seq: self.frame_seq,
            ts: self.ts.clone(),
            monotonic_ns: self.monotonic_ns,
            raw_offset: self.raw_offset,
            raw_len: self.raw_len(),
            parse_status: parse_status.into(),
            duplicate_of: self.duplicate_of,
        }
    }
}

pub fn parse_rall_frame_minimal(bytes: &[u8]) -> Result<MinimalRallFrame, MinimalRallParseError> {
    let parsed = parse_rall_frame_full(bytes)?;
    let measurement_matrix = parsed
        .measurement_matrix
        .ok_or_else(|| MinimalRallParseError::LayoutSpec("measurement_matrix 缺失".to_string()))?;

    Ok(MinimalRallFrame {
        measurement_matrix,
        status: MinimalRallStatus {
            b_ref_source_code: parsed.b_ref_source_code,
            b_ref_slope_code: parsed.b_ref_slope_code,
            b_ref_current_freq_hz: parsed.b_ref_current_freq_hz,
            b_input_overload: parsed.b_input_overload,
            b_gain_overload: parsed.b_gain_overload,
            b_pll_locked: parsed.b_pll_locked,
        },
    })
}

pub fn parse_rall_frame_full(bytes: &[u8]) -> Result<ParsedFrameRecord, MinimalRallParseError> {
    if bytes.len() != RALL_FRAME_BYTES {
        return Err(MinimalRallParseError::WrongLength {
            expected: RALL_FRAME_BYTES,
            actual: bytes.len(),
        });
    }

    let layout = rall_layout_spec()?;
    let mut measurement_matrix = Vec::with_capacity(layout.measurement_fields.len());
    for field in &layout.measurement_fields {
        let mut samples = Vec::with_capacity(field.sample_count);
        for sample_index in 0..field.sample_count {
            let sample_start = field.start + sample_index * 8;
            let sample_end = sample_start + 8;
            let chunk: [u8; 8] = bytes[sample_start..sample_end].try_into().unwrap();
            let value = f64::from_bits(u64::from_be_bytes(chunk));
            if !value.is_finite() {
                return Err(MinimalRallParseError::NonFiniteValue {
                    field: field.name.clone(),
                    sample_index,
                });
            }
            samples.push(value);
        }
        debug_assert_eq!(field.end + 1 - field.start, RALL_PARAM_BLOCK_BYTES);
        measurement_matrix.push(samples);
    }

    let scalar_fields = layout
        .scalar_fields
        .iter()
        .map(|spec| parse_scalar_field(bytes, spec))
        .collect::<Result<Vec<_>, _>>()?;
    let padding_status = if bytes[RALL_PADDING_START..].iter().all(|byte| *byte == 0) {
        "all_zero".to_string()
    } else {
        "non_zero".to_string()
    };

    Ok(ParsedFrameRecord {
        schema_version: 1,
        layout_version: "oe1022d_rall_v1_table_driven".to_string(),
        frame_seq: 0,
        ts: String::new(),
        monotonic_ns: 0,
        raw_offset: 0,
        raw_len: bytes.len(),
        transport_status: "payload_committed".to_string(),
        parse_status: "ok".to_string(),
        padding_status,
        duplicate_hint: None,
        parse_error: None,
        measurement_field_order: layout
            .measurement_fields
            .iter()
            .map(|field| field.name.clone())
            .collect(),
        measurement_matrix: Some(measurement_matrix),
        scalar_fields,
        b_ref_source_code: read_rall_u8(bytes, 8504),
        b_ref_slope_code: read_rall_u8(bytes, 8521),
        b_ref_current_freq_hz: read_rall_f64(bytes, 8505),
        b_input_overload: read_rall_u8(bytes, 8779).map(|value| value != 0),
        b_gain_overload: read_rall_u8(bytes, 8780).map(|value| value != 0),
        b_pll_locked: read_rall_u8(bytes, 8781).map(|value| value != 0),
    })
}

pub fn build_parsed_frame_record(
    frame: &CollectorFrame,
    transport_status: &str,
    parse_status: &str,
    parse_result: Result<ParsedFrameRecord, MinimalRallParseError>,
) -> ParsedFrameRecord {
    match parse_result {
        Ok(mut parsed) => {
            parsed.frame_seq = frame.frame_seq;
            parsed.ts = frame.ts.clone();
            parsed.monotonic_ns = frame.monotonic_ns;
            parsed.raw_offset = frame.raw_offset;
            parsed.raw_len = frame.raw_len();
            parsed.transport_status = transport_status.to_string();
            parsed.parse_status = parse_status.to_string();
            parsed.duplicate_hint = frame.duplicate_of;
            parsed
        }
        Err(err) => ParsedFrameRecord {
            schema_version: 1,
            layout_version: "oe1022d_rall_v1_table_driven".to_string(),
            frame_seq: frame.frame_seq,
            ts: frame.ts.clone(),
            monotonic_ns: frame.monotonic_ns,
            raw_offset: frame.raw_offset,
            raw_len: frame.raw_len(),
            transport_status: transport_status.to_string(),
            parse_status: parse_status.to_string(),
            padding_status: "not_checked".to_string(),
            duplicate_hint: frame.duplicate_of,
            parse_error: Some(err.to_string()),
            measurement_field_order: RALL_FIELD_ORDER
                .iter()
                .map(|name| (*name).to_string())
                .collect(),
            measurement_matrix: None,
            scalar_fields: Vec::new(),
            b_ref_source_code: None,
            b_ref_slope_code: None,
            b_ref_current_freq_hz: None,
            b_input_overload: None,
            b_gain_overload: None,
            b_pll_locked: None,
        },
    }
}

pub fn build_point_field_bundle(
    run_id: &str,
    point_id: &str,
    segment_id: &str,
    sidecar_relative_path: &str,
    sidecar_manifest_relative_path: &str,
    frames: &[CollectorFrame],
) -> Result<PointFieldBundle, MinimalRallParseError> {
    let parsed_frames = frames
        .iter()
        .map(|frame| {
            let mut parsed = parse_rall_frame_full(&frame.payload)?;
            parsed.frame_seq = frame.frame_seq;
            parsed.ts = frame.ts.clone();
            parsed.monotonic_ns = frame.monotonic_ns;
            parsed.raw_offset = frame.raw_offset;
            parsed.raw_len = frame.raw_len();
            parsed.duplicate_hint = frame.duplicate_of;
            Ok(parsed)
        })
        .collect::<Result<Vec<_>, MinimalRallParseError>>()?;
    build_point_field_bundle_from_parsed(
        run_id,
        point_id,
        segment_id,
        sidecar_relative_path,
        sidecar_manifest_relative_path,
        &parsed_frames,
    )
}

pub fn build_point_field_bundle_from_parsed(
    run_id: &str,
    point_id: &str,
    segment_id: &str,
    sidecar_relative_path: &str,
    sidecar_manifest_relative_path: &str,
    frames: &[ParsedFrameRecord],
) -> Result<PointFieldBundle, MinimalRallParseError> {
    let measurement_field_order = RALL_FIELD_ORDER
        .iter()
        .map(|name| (*name).to_string())
        .collect::<Vec<_>>();
    let measurement_field_keys = measurement_field_order
        .iter()
        .map(|name| point_field_npz_key(name))
        .collect::<Vec<_>>();
    let mut measurement_fields = vec![Vec::new(); RALL_PARAM_COUNT];
    let mut pll_locked_frames = 0_usize;
    let mut input_overload_frames = 0_usize;
    let mut gain_overload_frames = 0_usize;
    let mut frame_seq = Vec::with_capacity(frames.len());
    let mut duplicate_hint = Vec::with_capacity(frames.len());
    let mut b_ref_source_code = Vec::with_capacity(frames.len());
    let mut b_ref_slope_code = Vec::with_capacity(frames.len());
    let mut b_ref_current_freq_hz = Vec::with_capacity(frames.len());
    let mut b_input_overload = Vec::with_capacity(frames.len());
    let mut b_gain_overload = Vec::with_capacity(frames.len());
    let mut b_pll_locked = Vec::with_capacity(frames.len());
    let mut last_status = MinimalRallStatus {
        b_ref_source_code: None,
        b_ref_slope_code: None,
        b_ref_current_freq_hz: None,
        b_input_overload: None,
        b_gain_overload: None,
        b_pll_locked: None,
    };

    for frame in frames {
        let matrix = frame.measurement_matrix.as_ref().ok_or_else(|| {
            MinimalRallParseError::LayoutSpec("parsed frame 缺少 measurement_matrix".to_string())
        })?;
        if matrix.len() != RALL_PARAM_COUNT {
            return Err(MinimalRallParseError::LayoutSpec(format!(
                "parsed frame measurement_matrix 行数错误: expected={}, actual={}",
                RALL_PARAM_COUNT,
                matrix.len()
            )));
        }
        last_status = MinimalRallStatus {
            b_ref_source_code: frame.b_ref_source_code,
            b_ref_slope_code: frame.b_ref_slope_code,
            b_ref_current_freq_hz: frame.b_ref_current_freq_hz,
            b_input_overload: frame.b_input_overload,
            b_gain_overload: frame.b_gain_overload,
            b_pll_locked: frame.b_pll_locked,
        };
        for (field_index, samples) in matrix.iter().enumerate() {
            measurement_fields[field_index].extend_from_slice(samples);
        }
        frame_seq.push(frame.frame_seq);
        duplicate_hint.push(frame.duplicate_hint.map(|value| value as i64).unwrap_or(-1));
        b_ref_source_code.push(frame.b_ref_source_code.map(i16::from).unwrap_or(-1));
        b_ref_slope_code.push(frame.b_ref_slope_code.map(i16::from).unwrap_or(-1));
        b_ref_current_freq_hz.push(frame.b_ref_current_freq_hz.unwrap_or(f64::NAN));
        b_input_overload.push(option_bool_to_i8(frame.b_input_overload));
        b_gain_overload.push(option_bool_to_i8(frame.b_gain_overload));
        b_pll_locked.push(option_bool_to_i8(frame.b_pll_locked));
        if frame.b_pll_locked.unwrap_or(false) {
            pll_locked_frames += 1;
        }
        if frame.b_input_overload.unwrap_or(false) {
            input_overload_frames += 1;
        }
        if frame.b_gain_overload.unwrap_or(false) {
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

    let field_summaries = measurement_field_order
        .iter()
        .zip(measurement_field_keys.iter())
        .zip(measurement_fields.iter())
        .map(|((field_name, npz_key), values)| PointFieldSummaryRecord {
            field_name: field_name.clone(),
            npz_key: npz_key.clone(),
            mean: mean(values),
        })
        .collect::<Vec<_>>();
    let sidecar = PointFieldSidecarRef {
        format: "npz".to_string(),
        schema_version: 1,
        relative_path: sidecar_relative_path.to_string(),
        manifest_relative_path: sidecar_manifest_relative_path.to_string(),
        measurement_field_keys: measurement_field_keys.clone(),
        status_keys: vec![
            "frame_seq".to_string(),
            "duplicate_hint".to_string(),
            "b_ref_source_code".to_string(),
            "b_ref_slope_code".to_string(),
            "b_ref_current_freq_hz".to_string(),
            "b_input_overload".to_string(),
            "b_gain_overload".to_string(),
            "b_pll_locked".to_string(),
        ],
    };

    Ok(PointFieldBundle {
        metadata: PointFieldRecord {
            schema_version: 1,
            run_id: run_id.to_string(),
            point_id: point_id.to_string(),
            segment_id: segment_id.to_string(),
            frames_parsed,
            samples_total,
            samples_per_frame: RALL_SAMPLE_COUNT,
            matrix_shape: [RALL_PARAM_COUNT, samples_total],
            measurement_field_order: measurement_field_order.clone(),
            measurement_field_keys: measurement_field_keys.clone(),
            field_summaries,
            b_pll_locked_frames: pll_locked_frames,
            b_pll_locked_ratio: ratio(pll_locked_frames),
            b_input_overload_frames: input_overload_frames,
            b_input_overload_ratio: ratio(input_overload_frames),
            b_gain_overload_frames: gain_overload_frames,
            b_gain_overload_ratio: ratio(gain_overload_frames),
            last_b_ref_source_code: last_status.b_ref_source_code,
            last_b_ref_slope_code: last_status.b_ref_slope_code,
            last_b_ref_current_freq_hz: last_status.b_ref_current_freq_hz,
            sidecar: sidecar.clone(),
        },
        sidecar: PointFieldSidecarData {
            schema_version: 1,
            matrix_shape: [RALL_PARAM_COUNT, samples_total],
            samples_per_frame: RALL_SAMPLE_COUNT,
            measurement_field_order,
            measurement_field_keys,
            measurement_fields,
            frame_seq,
            duplicate_hint,
            b_ref_source_code,
            b_ref_slope_code,
            b_ref_current_freq_hz,
            b_input_overload,
            b_gain_overload,
            b_pll_locked,
        },
    })
}

fn read_rall_u8(bytes: &[u8], offset: usize) -> Option<u8> {
    bytes.get(offset).copied()
}

fn read_rall_f64(bytes: &[u8], offset: usize) -> Option<f64> {
    read_f64_be(bytes, offset).ok()
}

fn mean(values: &[f64]) -> f64 {
    if values.is_empty() {
        return 0.0;
    }
    values.iter().sum::<f64>() / values.len() as f64
}

fn point_field_npz_key(field_name: &str) -> String {
    field_name
        .chars()
        .map(|ch| match ch {
            '-' => '_',
            other => other.to_ascii_lowercase(),
        })
        .collect()
}

fn option_bool_to_i8(value: Option<bool>) -> i8 {
    match value {
        Some(true) => 1,
        Some(false) => 0,
        None => -1,
    }
}

pub fn rall_layout_spec() -> Result<&'static RallLayoutSpec, MinimalRallParseError> {
    static SPEC: OnceLock<Result<RallLayoutSpec, String>> = OnceLock::new();
    SPEC.get_or_init(parse_rall_layout_spec_from_markdown)
        .as_ref()
        .map_err(|message| MinimalRallParseError::LayoutSpec(message.clone()))
}

fn parse_rall_layout_spec_from_markdown() -> Result<RallLayoutSpec, String> {
    let mut measurement_fields = Vec::new();
    let mut scalar_fields = Vec::new();

    for line in RALL_LAYOUT_MARKDOWN.lines() {
        if !line.starts_with('|') {
            continue;
        }
        let columns = line.split('|').map(str::trim).collect::<Vec<_>>();
        if columns.len() < 8 {
            continue;
        }
        let Ok(index) = columns[1].parse::<usize>() else {
            continue;
        };
        let category = columns[2];
        let name = columns[3];
        let start = columns[4]
            .parse::<usize>()
            .map_err(|err| format!("RALL layout start 解析失败 `{}`: {err}", columns[4]))?;
        let end = columns[5]
            .parse::<usize>()
            .map_err(|err| format!("RALL layout end 解析失败 `{}`: {err}", columns[5]))?;
        let encoding = columns[6];
        let sample_count = columns[7]
            .parse::<usize>()
            .map_err(|err| format!("RALL layout sample_count 解析失败 `{}`: {err}", columns[7]))?;

        if sample_count == RALL_SAMPLE_COUNT && encoding == "f64" {
            measurement_fields.push(RallMeasurementFieldSpec {
                index,
                name: name.to_string(),
                start,
                end,
                sample_count,
                encoding: encoding.to_string(),
            });
        } else {
            scalar_fields.push(RallScalarFieldSpec {
                index,
                category: category.to_string(),
                name: name.to_string(),
                start,
                end,
                encoding: encoding.to_string(),
            });
        }
    }

    if measurement_fields.len() != RALL_PARAM_COUNT {
        return Err(format!(
            "RALL measurement field 数量错误: expected={}, actual={}",
            RALL_PARAM_COUNT,
            measurement_fields.len()
        ));
    }

    let mut reserved_ranges = Vec::new();
    let mut occupied_ranges = measurement_fields
        .iter()
        .map(|field| (field.start, field.end))
        .chain(scalar_fields.iter().map(|field| (field.start, field.end)))
        .collect::<Vec<_>>();
    occupied_ranges.sort_by_key(|(start, _)| *start);

    let mut cursor = 0_usize;
    for (start, end) in occupied_ranges {
        if start > cursor {
            reserved_ranges.push(RallReservedRangeSpec {
                start: cursor,
                end: start - 1,
                reason: "manual_reserved_gap".to_string(),
            });
        }
        cursor = end.saturating_add(1);
    }
    if cursor < RALL_PADDING_START {
        reserved_ranges.push(RallReservedRangeSpec {
            start: cursor,
            end: RALL_PADDING_START - 1,
            reason: "manual_reserved_gap".to_string(),
        });
    }

    Ok(RallLayoutSpec {
        schema_version: 1,
        source_document:
            "docs/equipment_manual/oe1022d/05_oe1022d_rall_global_data_config_reading.md"
                .to_string(),
        measurement_fields,
        scalar_fields,
        reserved_ranges,
        padding_start: RALL_PADDING_START,
        padding_end: RALL_FRAME_BYTES - 1,
        padding_must_be_zero: true,
    })
}

fn parse_scalar_field(
    bytes: &[u8],
    spec: &RallScalarFieldSpec,
) -> Result<ParsedScalarFieldRecord, MinimalRallParseError> {
    let value = match spec.encoding.as_str() {
        "i8" => JsonValue::from(read_i8(bytes, spec.start)?),
        "i16" => JsonValue::from(read_i16_be(bytes, spec.start)?),
        "i64" => JsonValue::from(read_i64_be(bytes, spec.start)?),
        "f32" => {
            let value = read_f32_be(bytes, spec.start)?;
            JsonValue::from(value)
        }
        "f64" => {
            let value = read_f64_be(bytes, spec.start)?;
            JsonValue::from(value)
        }
        encoding if encoding.starts_with("bytes[") => {
            let raw = bytes.get(spec.start..=spec.end).ok_or_else(|| {
                MinimalRallParseError::LayoutSpec(format!("bytes 字段越界: {}", spec.name))
            })?;
            let mut value = JsonMap::new();
            value.insert("hex".to_string(), JsonValue::from(hex_encode(raw)));
            let text = String::from_utf8_lossy(raw)
                .trim_matches(char::from(0))
                .trim()
                .to_string();
            value.insert("text".to_string(), JsonValue::from(text));
            JsonValue::Object(value)
        }
        other => {
            return Err(MinimalRallParseError::LayoutSpec(format!(
                "不支持的 RALL 标量编码: {other}"
            )))
        }
    };

    Ok(ParsedScalarFieldRecord {
        index: spec.index,
        category: spec.category.clone(),
        name: spec.name.clone(),
        start: spec.start,
        end: spec.end,
        encoding: spec.encoding.clone(),
        value,
    })
}

fn read_i8(bytes: &[u8], offset: usize) -> Result<i8, MinimalRallParseError> {
    bytes
        .get(offset)
        .copied()
        .map(|value| value as i8)
        .ok_or_else(|| MinimalRallParseError::LayoutSpec(format!("i8 字段越界: offset={offset}")))
}

fn read_i16_be(bytes: &[u8], offset: usize) -> Result<i16, MinimalRallParseError> {
    let chunk: [u8; 2] = bytes
        .get(offset..offset + 2)
        .ok_or_else(|| MinimalRallParseError::LayoutSpec(format!("i16 字段越界: offset={offset}")))?
        .try_into()
        .unwrap();
    Ok(i16::from_be_bytes(chunk))
}

fn read_i64_be(bytes: &[u8], offset: usize) -> Result<i64, MinimalRallParseError> {
    let chunk: [u8; 8] = bytes
        .get(offset..offset + 8)
        .ok_or_else(|| MinimalRallParseError::LayoutSpec(format!("i64 字段越界: offset={offset}")))?
        .try_into()
        .unwrap();
    Ok(i64::from_be_bytes(chunk))
}

fn read_f32_be(bytes: &[u8], offset: usize) -> Result<f32, MinimalRallParseError> {
    let chunk: [u8; 4] = bytes
        .get(offset..offset + 4)
        .ok_or_else(|| MinimalRallParseError::LayoutSpec(format!("f32 字段越界: offset={offset}")))?
        .try_into()
        .unwrap();
    Ok(f32::from_bits(u32::from_be_bytes(chunk)))
}

fn read_f64_be(bytes: &[u8], offset: usize) -> Result<f64, MinimalRallParseError> {
    let chunk: [u8; 8] = bytes
        .get(offset..offset + 8)
        .ok_or_else(|| MinimalRallParseError::LayoutSpec(format!("f64 字段越界: offset={offset}")))?
        .try_into()
        .unwrap();
    Ok(f64::from_bits(u64::from_be_bytes(chunk)))
}

fn hex_encode(bytes: &[u8]) -> String {
    let mut out = String::with_capacity(bytes.len() * 2);
    for byte in bytes {
        use std::fmt::Write as _;
        let _ = write!(&mut out, "{byte:02X}");
    }
    out
}

fn default_cartesian_order() -> Vec<String> {
    vec!["x".to_string(), "y".to_string(), "z".to_string()]
}

fn default_baseline_mode() -> String {
    "legacy_zero_offset_lock".to_string()
}

fn validate_cartesian_order(order: &[String]) -> Result<(), RunPlanResolveError> {
    let expected = default_cartesian_order();
    if order == expected {
        Ok(())
    } else {
        Err(RunPlanResolveError::InvalidCartesianOrder(order.to_vec()))
    }
}

fn validate_axis_values(axes: &CartesianGridAxesNt) -> Result<(), RunPlanResolveError> {
    if axes.x.is_empty() {
        return Err(RunPlanResolveError::MissingAxisValues("x"));
    }
    if axes.y.is_empty() {
        return Err(RunPlanResolveError::MissingAxisValues("y"));
    }
    if axes.z.is_empty() {
        return Err(RunPlanResolveError::MissingAxisValues("z"));
    }
    Ok(())
}

fn build_raster_points(axes: &CartesianGridAxesNt) -> Vec<RunPointPlan> {
    let mut points = Vec::with_capacity(axes.x.len() * axes.y.len() * axes.z.len());
    let mut next_index = 1_usize;

    for z in &axes.z {
        for y in &axes.y {
            for x in &axes.x {
                points.push(RunPointPlan {
                    point_id: format!("p{next_index:06}"),
                    target_b_nt: [*x, *y, *z],
                    smb_override: None,
                });
                next_index += 1;
            }
        }
    }

    points
}

fn build_bounce_1d_x_points(
    axes: &CartesianGridAxesNt,
) -> Result<Vec<RunPointPlan>, RunPlanResolveError> {
    if axes.y.len() != 1 || axes.z.len() != 1 {
        return Err(RunPlanResolveError::Bounce1dXRequiresSingletonYZ {
            y_len: axes.y.len(),
            z_len: axes.z.len(),
        });
    }

    let mut x_values = axes.x.clone();
    if axes.x.len() > 1 {
        x_values.extend(axes.x[1..axes.x.len() - 1].iter().rev().copied());
    }

    let y = axes.y[0];
    let z = axes.z[0];
    Ok(x_values
        .into_iter()
        .enumerate()
        .map(|(index, x)| RunPointPlan {
            point_id: format!("p{:06}", index + 1),
            target_b_nt: [x, y, z],
            smb_override: None,
        })
        .collect())
}

fn repeat_points_to_total(base_points: &[RunPointPlan], total_points: usize) -> Vec<RunPointPlan> {
    let mut resolved = Vec::with_capacity(total_points);
    for index in 0..total_points {
        let mut point = base_points[index % base_points.len()].clone();
        point.point_id = format!("p{:06}", index + 1);
        resolved.push(point);
    }
    resolved
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

    pub fn push(
        &mut self,
        ts: String,
        monotonic_ns: u64,
        payload: Vec<u8>,
        duplicate_of: Option<u64>,
    ) -> CollectorFrame {
        let frame = CollectorFrame {
            frame_seq: self.next_frame_seq,
            ts,
            monotonic_ns,
            raw_offset: self.next_raw_offset,
            duplicate_of,
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
    estimated_frames_expected: Option<usize>,
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
    let frame_coverage_ratio = estimated_frames_expected.map(|expected| {
        if expected == 0 {
            0.0
        } else {
            frames_total as f64 / expected as f64
        }
    });
    let collector_health = if timeout_count == 0 {
        "clean"
    } else if timeout_count <= thresholds.max_timeout_count {
        "recovered_timeout"
    } else {
        "degraded_timeout"
    };
    let timeout_budget_remaining = thresholds.max_timeout_count.saturating_sub(timeout_count);

    let quality_status = if frames_total == 0 {
        "failed_no_frames".to_string()
    } else if frames_total < thresholds.min_frames {
        "failed_min_frames".to_string()
    } else if timeout_count > thresholds.max_timeout_count {
        "failed_timeout".to_string()
    } else if duplicate_ratio > thresholds.max_duplicate_ratio
        || last_frame_age_ms > thresholds.max_last_frame_age_ms
    {
        "failed_quality".to_string()
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
        estimated_frames_expected,
        frame_coverage_ratio,
        collector_health: collector_health.to_string(),
        timeout_budget_remaining,
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

    fn sample_profile() -> Smb100aRunProfile {
        Smb100aRunProfile {
            profile_id: "test".to_string(),
            command_settle_ms: 500,
            error_check_after_write: true,
            fixed: Smb100aFixedProfile {
                modulation_enabled: true,
                fm_enabled: true,
                fm_source: "INT".to_string(),
                fm_mode: "HDEV".to_string(),
                fm_deviation_hz: 4.0e6,
                lf_output_enabled: true,
                lf_voltage_mv: 137.0,
                lf_frequency_hz: 500.0,
                lf_shape: "SQU".to_string(),
                lf_source_impedance: "LOW".to_string(),
            },
            default_sweep: sample_defaults(),
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
    fn sweep_estimate_counts_inclusive_points() {
        let estimate = sample_defaults().estimate().unwrap();
        assert_eq!(estimate.sweep_points, 201);
        assert_eq!(estimate.sweep_duration_ms, 100500);
    }

    #[test]
    fn explicit_plan_resolves_without_grid_expansion() {
        let plan = AcquisitionRunPlan {
            run_id: "run_explicit".to_string(),
            operator: "local".to_string(),
            acquisition_window_ms: 5000,
            point_settle_ms: 1000,
            failure_policy: "continue".to_string(),
            mag_baseline_policy: MagBaselinePolicy {
                baseline_current_a: [0.0, 0.0, 0.0],
                settle_ms: 1000,
                readback_samples: 3,
                settle_tolerance_a: 0.001,
                voltage_v: Some(75.0),
                voltage_protection_v: Some(75.0),
                output_enabled: true,
            },
            quality_thresholds: RunQualityThresholds {
                min_frames: 1,
                max_timeout_count: 0,
                max_duplicate_ratio: 1.0,
                max_last_frame_age_ms: 1000,
            },
            point_source: None,
            points: vec![RunPointPlan {
                point_id: "p0001".to_string(),
                target_b_nt: [1.0, 2.0, 3.0],
                smb_override: None,
            }],
        };

        let resolved = plan.resolve_points(&sample_profile()).unwrap();
        assert_eq!(resolved.source_kind, PlanSourceKind::ExplicitPoints);
        assert_eq!(resolved.resolved_point_count, 1);
        assert_eq!(resolved.points[0].target_b_nt, [1.0, 2.0, 3.0]);
    }

    #[test]
    fn cartesian_grid_can_resolve_without_smb_profile() {
        let plan = AcquisitionRunPlan {
            run_id: "run_grid_no_smb".to_string(),
            operator: "local".to_string(),
            acquisition_window_ms: 0,
            point_settle_ms: 1000,
            failure_policy: "continue".to_string(),
            mag_baseline_policy: sample_cartesian_plan(
                CartesianGridCycleMode::Bounce1dX,
                CartesianGridStopCondition::FixedTotalPoints { total_points: 1 },
            )
            .mag_baseline_policy,
            quality_thresholds: RunQualityThresholds {
                min_frames: 1,
                max_timeout_count: 0,
                max_duplicate_ratio: 1.0,
                max_last_frame_age_ms: 1000,
            },
            point_source: Some(PointSource::CartesianGrid {
                axes_nt: CartesianGridAxesNt {
                    x: vec![-10.0, 0.0, 10.0],
                    y: vec![-10.0, 0.0, 10.0],
                    z: vec![0.0],
                },
                order: default_cartesian_order(),
                cycle_mode: CartesianGridCycleMode::Raster,
                stop_condition: CartesianGridStopCondition::FixedTotalPoints { total_points: 9 },
            }),
            points: Vec::new(),
        };

        let resolved = plan.resolve_points_without_smb_profile().unwrap();
        assert_eq!(resolved.source_kind, PlanSourceKind::CartesianGrid);
        assert_eq!(resolved.declared_point_count, 9);
        assert_eq!(resolved.resolved_point_count, 9);
        assert_eq!(resolved.estimated_sweep, None);
        assert_eq!(
            resolved.points.first().unwrap().target_b_nt,
            [-10.0, -10.0, 0.0]
        );
        assert_eq!(
            resolved.points.last().unwrap().target_b_nt,
            [10.0, 10.0, 0.0]
        );
    }

    #[test]
    fn bounce_1d_x_expansion_repeats_forward_and_backward() {
        let plan = sample_cartesian_plan(
            CartesianGridCycleMode::Bounce1dX,
            CartesianGridStopCondition::FixedTotalPoints { total_points: 6 },
        );
        let resolved = plan.resolve_points(&sample_profile()).unwrap();
        let values = resolved
            .points
            .iter()
            .map(|point| point.target_b_nt[0])
            .collect::<Vec<_>>();
        assert_eq!(resolved.source_kind, PlanSourceKind::CartesianGrid);
        assert_eq!(values, vec![-10.0, 0.0, 10.0, 0.0, -10.0, 0.0]);
        assert_eq!(resolved.estimated_point_duration_ms, Some(107500));
    }

    #[test]
    fn raster_cartesian_expansion_keeps_x_fastest() {
        let plan = AcquisitionRunPlan {
            run_id: "run_raster".to_string(),
            operator: "local".to_string(),
            acquisition_window_ms: 0,
            point_settle_ms: 1000,
            failure_policy: "continue".to_string(),
            mag_baseline_policy: sample_cartesian_plan(
                CartesianGridCycleMode::Bounce1dX,
                CartesianGridStopCondition::FixedTotalPoints { total_points: 1 },
            )
            .mag_baseline_policy,
            quality_thresholds: RunQualityThresholds {
                min_frames: 1,
                max_timeout_count: 0,
                max_duplicate_ratio: 1.0,
                max_last_frame_age_ms: 1000,
            },
            point_source: Some(PointSource::CartesianGrid {
                axes_nt: CartesianGridAxesNt {
                    x: vec![1.0, 2.0],
                    y: vec![10.0, 20.0],
                    z: vec![100.0],
                },
                order: default_cartesian_order(),
                cycle_mode: CartesianGridCycleMode::Raster,
                stop_condition: CartesianGridStopCondition::FixedTotalPoints { total_points: 4 },
            }),
            points: Vec::new(),
        };

        let resolved = plan.resolve_points(&sample_profile()).unwrap();
        let targets = resolved
            .points
            .iter()
            .map(|point| point.target_b_nt)
            .collect::<Vec<_>>();
        assert_eq!(
            targets,
            vec![
                [1.0, 10.0, 100.0],
                [2.0, 10.0, 100.0],
                [1.0, 20.0, 100.0],
                [2.0, 20.0, 100.0]
            ]
        );
    }

    #[test]
    fn bounce_1d_x_rejects_multi_value_y_or_z() {
        let mut plan = sample_cartesian_plan(
            CartesianGridCycleMode::Bounce1dX,
            CartesianGridStopCondition::FixedTotalPoints { total_points: 4 },
        );
        plan.point_source = Some(PointSource::CartesianGrid {
            axes_nt: CartesianGridAxesNt {
                x: vec![-10.0, 0.0, 10.0],
                y: vec![0.0, 10.0],
                z: vec![0.0],
            },
            order: default_cartesian_order(),
            cycle_mode: CartesianGridCycleMode::Bounce1dX,
            stop_condition: CartesianGridStopCondition::FixedTotalPoints { total_points: 4 },
        });

        let err = plan.resolve_points(&sample_profile()).unwrap_err();
        assert!(matches!(
            err,
            RunPlanResolveError::Bounce1dXRequiresSingletonYZ { .. }
        ));
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
    fn baseline_snapshot_prefers_locked_zero_offset_current() {
        let snapshot = BaselineSnapshot {
            schema_version: 1,
            mode: "legacy_zero_offset_lock".to_string(),
            baseline_locked_at: "1.000Z".to_string(),
            settle_ms: 1000,
            readback_samples: 3,
            settle_tolerance_a: 0.002,
            axes: vec![
                BaselineAxisSnapshot {
                    axis: "mag_x".to_string(),
                    zero_offset_setpoint_a: 0.0,
                    zero_offset_measured_samples_a: vec![0.001, 0.0011, 0.0009],
                    locked_zero_offset_current_a: Some(0.001),
                },
                BaselineAxisSnapshot {
                    axis: "mag_y".to_string(),
                    zero_offset_setpoint_a: 0.0,
                    zero_offset_measured_samples_a: vec![0.002, 0.0021, 0.0019],
                    locked_zero_offset_current_a: Some(0.002),
                },
                BaselineAxisSnapshot {
                    axis: "mag_z".to_string(),
                    zero_offset_setpoint_a: 0.0,
                    zero_offset_measured_samples_a: vec![0.003, 0.0031, 0.0029],
                    locked_zero_offset_current_a: Some(0.003),
                },
            ],
        };

        assert_eq!(snapshot.baseline_current_a(), [0.001, 0.002, 0.003]);
    }

    #[test]
    fn laser_run_profile_parses_from_json() {
        let profile: LaserRunProfile = serde_json::from_str(
            r#"{
                "profile_id": "laser_on",
                "mode": "on_background",
                "power_mw": 50,
                "settle_ms": 1000
            }"#,
        )
        .unwrap();

        assert_eq!(profile.profile_id, "laser_on");
        assert_eq!(profile.mode, LaserBackgroundMode::OnBackground);
        assert_eq!(profile.power_mw, 50);
        assert_eq!(profile.settle_ms, 1000);
    }

    #[test]
    fn ring_buffer_retains_latest_frames_only() {
        let mut ring = FrameRingBuffer::new(2);
        ring.push("t1".to_string(), 10, vec![1, 2, 3], None);
        ring.push("t2".to_string(), 20, vec![4, 5], None);
        ring.push("t3".to_string(), 30, vec![6], None);

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

        let quality = compute_quality_record("run_1", "p1", "seg1", 10, &[], &thresholds, 0, None);
        assert_eq!(quality.quality_status, "failed_no_frames");
        assert_eq!(quality.collector_health, "clean");
        assert_eq!(quality.timeout_budget_remaining, 0);
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

        let quality = compute_quality_record(
            "run_1",
            "p1",
            "seg1",
            20_000_000,
            &frames,
            &thresholds,
            0,
            Some(4),
        );
        assert_eq!(quality.quality_status, "passed");
        assert_eq!(quality.frames_total, 2);
        assert_eq!(quality.frames_unique, 2);
        assert_eq!(quality.estimated_frames_expected, Some(4));
        assert_eq!(quality.frame_coverage_ratio, Some(0.5));
        assert_eq!(quality.collector_health, "clean");
        assert_eq!(quality.timeout_budget_remaining, 0);
    }

    #[test]
    fn quality_fails_when_timeout_occurs() {
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

        let quality = compute_quality_record(
            "run_1",
            "p1",
            "seg1",
            20_000_000,
            &frames,
            &thresholds,
            1,
            Some(4),
        );
        assert_eq!(quality.quality_status, "failed_timeout");
        assert_eq!(quality.frame_coverage_ratio, Some(0.5));
        assert_eq!(quality.collector_health, "degraded_timeout");
        assert_eq!(quality.timeout_budget_remaining, 0);
    }

    #[test]
    fn quality_allows_timeout_within_threshold() {
        let thresholds = RunQualityThresholds {
            min_frames: 2,
            max_timeout_count: 2,
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

        let quality = compute_quality_record(
            "run_1",
            "p1",
            "seg1",
            20_000_000,
            &frames,
            &thresholds,
            1,
            Some(4),
        );
        assert_eq!(quality.quality_status, "passed");
        assert_eq!(quality.frame_coverage_ratio, Some(0.5));
        assert_eq!(quality.collector_health, "recovered_timeout");
        assert_eq!(quality.timeout_budget_remaining, 1);
    }

    #[test]
    fn quality_allows_timeout_at_threshold_boundary() {
        let thresholds = RunQualityThresholds {
            min_frames: 2,
            max_timeout_count: 2,
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

        let quality = compute_quality_record(
            "run_1",
            "p1",
            "seg1",
            20_000_000,
            &frames,
            &thresholds,
            2,
            Some(4),
        );
        assert_eq!(quality.quality_status, "passed");
        assert_eq!(quality.collector_health, "recovered_timeout");
        assert_eq!(quality.timeout_budget_remaining, 0);
    }

    #[test]
    fn quality_fails_when_timeout_exceeds_threshold() {
        let thresholds = RunQualityThresholds {
            min_frames: 2,
            max_timeout_count: 2,
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

        let quality = compute_quality_record(
            "run_1",
            "p1",
            "seg1",
            20_000_000,
            &frames,
            &thresholds,
            3,
            Some(4),
        );
        assert_eq!(quality.quality_status, "failed_timeout");
        assert_eq!(quality.collector_health, "degraded_timeout");
        assert_eq!(quality.timeout_budget_remaining, 0);
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
    fn point_field_bundle_aggregates_all_fields_and_sidecar_contract() {
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

        let bundle = build_point_field_bundle(
            "run_1",
            "p1",
            "seg1",
            "point_fields/seg1.npz",
            "point_fields/seg1.manifest.json",
            &frames,
        )
        .unwrap();
        let record = bundle.metadata;

        assert_eq!(record.frames_parsed, 1);
        assert_eq!(record.samples_total, 50);
        assert_eq!(record.samples_per_frame, 50);
        assert_eq!(record.matrix_shape, [20, 50]);
        assert_eq!(record.sidecar.relative_path, "point_fields/seg1.npz");
        assert_eq!(
            record.sidecar.manifest_relative_path,
            "point_fields/seg1.manifest.json"
        );
        assert_eq!(record.field_summaries.len(), 20);
        assert_eq!(record.field_summaries[8].field_name, "B-X");
        assert_eq!(record.field_summaries[8].npz_key, "b_x");
        assert_eq!(record.field_summaries[8].mean, 0.03);
        assert_eq!(record.field_summaries[10].mean, 10.0);
        assert_eq!(record.b_pll_locked_ratio, 1.0);
        assert_eq!(bundle.sidecar.measurement_field_keys[8], "b_x");
        assert_eq!(
            bundle.sidecar.measurement_fields[8].first().copied(),
            Some(1.5)
        );
        assert_eq!(
            bundle.sidecar.measurement_fields[9].first().copied(),
            Some(2.5)
        );
        assert_eq!(
            bundle.sidecar.measurement_fields[10].first().copied(),
            Some(500.0)
        );
        assert_eq!(
            bundle.sidecar.measurement_fields[16].first().copied(),
            Some(3.1)
        );
        assert_eq!(bundle.sidecar.b_pll_locked, vec![1]);
    }

    fn sample_cartesian_plan(
        cycle_mode: CartesianGridCycleMode,
        stop_condition: CartesianGridStopCondition,
    ) -> AcquisitionRunPlan {
        AcquisitionRunPlan {
            run_id: "run_grid".to_string(),
            operator: "local".to_string(),
            acquisition_window_ms: 0,
            point_settle_ms: 500,
            failure_policy: "continue".to_string(),
            mag_baseline_policy: MagBaselinePolicy {
                baseline_current_a: [0.0, 0.0, 0.0],
                settle_ms: 1000,
                readback_samples: 3,
                settle_tolerance_a: 0.001,
                voltage_v: Some(75.0),
                voltage_protection_v: Some(75.0),
                output_enabled: true,
            },
            quality_thresholds: RunQualityThresholds {
                min_frames: 1,
                max_timeout_count: 0,
                max_duplicate_ratio: 1.0,
                max_last_frame_age_ms: 1000,
            },
            point_source: Some(PointSource::CartesianGrid {
                axes_nt: CartesianGridAxesNt {
                    x: vec![-10.0, 0.0, 10.0],
                    y: vec![0.0],
                    z: vec![0.0],
                },
                order: default_cartesian_order(),
                cycle_mode,
                stop_condition,
            }),
            points: Vec::new(),
        }
    }

    fn write_f64(bytes: &mut [u8], offset: usize, value: f64) {
        let end = offset + 8;
        bytes[offset..end].copy_from_slice(&value.to_bits().to_be_bytes());
    }
}
