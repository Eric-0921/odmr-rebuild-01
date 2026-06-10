//! 最小 runtime 真机执行入口。
//!
//! 当前目标很收敛：
//! - 固定 profile 下发
//! - Maynuo 零偏电流一次性锁定
//! - Laser 固定背景开启/关闭
//! - OE1022D 单 collector 持续采集
//! - point 从 ring buffer 按时间窗拉 segment
//! - 写出第一版 run artifact

use acquisition_runtime::{
    build_point_field_record, compute_quality_record, AcquisitionRunPlan, BaselineAxisSnapshot,
    BaselineSnapshot, CalibrationProfile, CollectorConfig, CollectorCursor, CollectorFrame,
    EventRecord, FrameIndexRecord, FrameRingBuffer, LaserBackgroundMode, LaserRunProfile,
    Oe1022dRunProfile, PlanSnapshot, PointRecord, ResolvedRunPlan, ResolvedSmbSweep, RunManifest,
    SegmentRecord, SettleRecord, Smb100aRunProfile, SummaryRecord,
};
use chrono::{DateTime, Local};
use cni_laser_transport::{CniLaserTransport, CniLaserTransportConfig};
use m8812_commands::{m8812_set_voltage_protection_v, m8812_set_voltage_v};
use m8812_transport::{M8812Transport, M8812TransportConfig};
use oe1022d_commands::{
    oe1022d_set_dynamic_reserve, oe1022d_set_filter_slope, oe1022d_set_harmonic,
    oe1022d_set_input_coupling, oe1022d_set_input_grounding, oe1022d_set_input_source,
    oe1022d_set_line_notch_filter, oe1022d_set_phase_deg, oe1022d_set_reference_slope,
    oe1022d_set_reference_source, oe1022d_set_sensitivity_index, oe1022d_set_sine_output_mode,
    oe1022d_set_sine_output_voltage_vrms, oe1022d_set_sync_filter, oe1022d_set_time_constant_index,
};
use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::de::DeserializeOwned;
use serde::Serialize;
use serde_json::json;
use smb100a_commands::{
    smb100a_execute_frequency_sweep, smb100a_query_error_next, smb100a_set_fm_deviation_hz,
    smb100a_set_fm_mode, smb100a_set_fm_source, smb100a_set_fm_state, smb100a_set_frequency_mode,
    smb100a_set_lf_frequency_hz, smb100a_set_lf_output_state, smb100a_set_lf_shape,
    smb100a_set_lf_source_impedance, smb100a_set_lf_voltage_mv, smb100a_set_modulation_state,
    smb100a_set_output, smb100a_set_power_dbm, smb100a_set_sweep_dwell_ms, smb100a_set_sweep_mode,
    smb100a_set_sweep_output_voltage_start_v, smb100a_set_sweep_output_voltage_stop_v,
    smb100a_set_sweep_shape, smb100a_set_sweep_spacing, smb100a_set_sweep_start_hz,
    smb100a_set_sweep_step_hz, smb100a_set_sweep_stop_hz, smb100a_set_sweep_trigger_source,
};
use smb100a_transport::{Smb100aTransport, Smb100aTransportConfig};
use station_resolver::{
    resolve_station, DeviceKind, DeviceSpec, StationResolveResult, StationSpec, TransportHint,
};
use std::fs::{self, File, OpenOptions};
use std::io::{BufRead, BufReader, Read, Seek, SeekFrom, Write};
use std::path::{Path, PathBuf};
use std::sync::{
    atomic::{AtomicBool, Ordering},
    Arc, Mutex,
};
use std::thread::{self, JoinHandle};
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const RUNTIME_VERSION: &str = "0.1.0-mvp";
const SMB_CLEANUP_WAIT_MS: u64 = 300;
const MAG_CLEANUP_WAIT_MS: u64 = 500;
const SWEEP_COMPLETION_GUARD_MS: u64 = 100;

#[derive(Debug, Clone)]
struct RunEtaSummary {
    estimated_point_duration_ms: Option<u64>,
    estimated_total_duration_ms: Option<u64>,
    estimated_end_at: Option<SystemTime>,
}

#[derive(Debug, Clone)]
struct RingCapacityPlan {
    effective_capacity_frames: usize,
    estimated_frames_max: usize,
    guard_frames: usize,
}

pub fn run_execute(
    station_path: &Path,
    calibration_path: &Path,
    plan_path: &Path,
    smb_profile_path: &Path,
    oe_profile_path: &Path,
    laser_profile_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec: StationSpec = crate::read_station_spec(station_path)?;
    let calibration: CalibrationProfile = read_json_file(calibration_path)?;
    let plan: AcquisitionRunPlan = read_json_file(plan_path)?;
    let smb_profile: Smb100aRunProfile = read_json_file(smb_profile_path)?;
    let oe_profile: Oe1022dRunProfile = read_json_file(oe_profile_path)?;
    let laser_profile: LaserRunProfile = read_json_file(laser_profile_path)?;
    let resolved_plan = plan
        .resolve_points(&smb_profile)
        .map_err(|err| format!("plan 展开失败: {err}"))?;
    let eta_summary = build_run_eta_summary(&plan, &smb_profile, &resolved_plan)
        .map_err(|err| format!("run ETA 估算失败: {err}"))?;
    let ring_capacity_plan =
        build_ring_capacity_plan(&smb_profile, &oe_profile.collector, &resolved_plan)
            .map_err(|err| format!("collector ring capacity 估算失败: {err}"))?;
    let mut effective_collector_config = oe_profile.collector.clone();
    effective_collector_config.ring_capacity_frames = ring_capacity_plan.effective_capacity_frames;
    let target_dir = resolve_run_output_dir(&plan.run_id, out_dir);
    fs::create_dir_all(target_dir.join("raw"))
        .map_err(|err| format!("无法创建 run 输出目录 {}: {err}", target_dir.display()))?;
    print_stage(&format!(
        "打开 run: {} -> {}",
        plan.run_id,
        target_dir.display()
    ));

    let resolved = resolve_station(&spec);
    write_pretty_json(
        &target_dir.join("station_snapshot.json"),
        &resolved.snapshot,
    )?;
    if resolved.snapshot.has_required_failures() {
        return Err(format!(
            "station verify 失败，required_failures={}",
            resolved.snapshot.required_failures
        ));
    }
    write_pretty_json(
        &target_dir.join("plan_snapshot.json"),
        &build_plan_snapshot(&plan, &resolved_plan),
    )?;
    write_pretty_json(&target_dir.join("calibration_snapshot.json"), &calibration)?;
    write_pretty_json(&target_dir.join("smb_profile_snapshot.json"), &smb_profile)?;
    write_pretty_json(&target_dir.join("oe_profile_snapshot.json"), &oe_profile)?;
    write_pretty_json(
        &target_dir.join("laser_profile_snapshot.json"),
        &laser_profile,
    )?;

    let mut events_file = open_jsonl_writer(&target_dir.join("events.jsonl"))?;
    let mut points_file = open_jsonl_writer(&target_dir.join("points.jsonl"))?;
    let mut segments_file = open_jsonl_writer(&target_dir.join("segments.jsonl"))?;
    let mut quality_file = open_jsonl_writer(&target_dir.join("quality.jsonl"))?;
    let mut point_fields_file = open_jsonl_writer(&target_dir.join("point_fields.jsonl"))?;

    let created_at = now_ts_string();
    let mut manifest = RunManifest {
        schema_version: 1,
        run_id: plan.run_id.clone(),
        created_at: created_at.clone(),
        operator: plan.operator.clone(),
        station_id: spec.station_id.clone(),
        runtime_version: RUNTIME_VERSION.to_string(),
        calibration_id: calibration.calibration_id.clone(),
        status: "running".to_string(),
        smb_profile_id: smb_profile.profile_id.clone(),
        oe_profile_id: oe_profile.profile_id.clone(),
        laser_profile_id: laser_profile.profile_id.clone(),
        plan_source_kind: resolved_plan.source_kind.as_str().to_string(),
        resolved_point_count: resolved_plan.resolved_point_count,
        estimated_run_duration_ms: eta_summary
            .estimated_total_duration_ms
            .or(resolved_plan.estimated_run_duration_ms),
    };
    write_pretty_json(&target_dir.join("run_manifest.json"), &manifest)?;

    let start_instant = Instant::now();
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "run_opened",
        "run",
        None,
        None,
        json!({
            "plan_source_kind": manifest.plan_source_kind,
            "resolved_point_count": manifest.resolved_point_count
        }),
    )?;
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "station_resolved",
        "station",
        None,
        None,
        json!({"station_id": spec.station_id}),
    )?;
    print_stage(&format!(
        "station resolve 完成: station_id={}, points={}",
        spec.station_id, resolved_plan.resolved_point_count
    ));
    print_run_eta_summary(&eta_summary);

    let smb_device = find_first_device(&resolved, DeviceKind::Smb100a)?;
    let oe_device = find_first_device(&resolved, DeviceKind::Oe1022d)?;
    let laser_device = find_optional_device(&resolved, DeviceKind::CniLaser);
    let mut smb = Smb100aTransport::connect(&tcp_config(smb_device)?)
        .map_err(|err| format!("无法连接 SMB100A {}: {err}", smb_device.device_id))?;

    apply_smb_fixed_profile(
        &mut smb,
        smb_device,
        &smb_profile,
        &mut events_file,
        &start_instant,
        &plan.run_id,
    )?;
    print_stage(&format!("SMB profile 已下发: {}", smb_profile.profile_id));
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "preflight_passed",
        "preflight",
        None,
        None,
        json!({"smb_profile_id": smb_profile.profile_id, "oe_profile_id": oe_profile.profile_id}),
    )?;

    {
        let mut oe = Oe1022dTransport::open(&oe_config(oe_device)?)
            .map_err(|err| format!("无法连接 OE1022D {}: {err}", oe_device.device_id))?;
        apply_oe_fixed_profile(
            &mut oe,
            oe_device,
            &oe_profile,
            &mut events_file,
            &start_instant,
            &plan.run_id,
        )?;
    }
    print_stage(&format!("OE profile 已下发: {}", oe_profile.profile_id));

    let mut mag_axes = open_mag_axes(&resolved)?;
    let baseline_snapshot =
        lock_baseline_once(&mut mag_axes, &plan, &mut events_file, &start_instant)?;
    write_pretty_json(
        &target_dir.join("baseline_snapshot.json"),
        &baseline_snapshot,
    )?;
    print_stage(&format!(
        "零偏锁定完成: baseline_current_a={:?}",
        baseline_snapshot.baseline_current_a()
    ));

    let mut laser = open_optional_laser(laser_device, &laser_profile)?;
    apply_laser_background_policy(
        &mut laser,
        &laser_profile,
        &mut events_file,
        &start_instant,
        &plan.run_id,
    )?;
    print_stage(&format!(
        "Laser 背景策略已应用: profile={}, mode={}, power_mw={}",
        laser_profile.profile_id,
        laser_profile.mode.as_str(),
        laser_profile.power_mw
    ));

    let raw_dir = target_dir.join("raw");
    let mut collector = CollectorHandle::start(
        oe_device,
        &effective_collector_config,
        &raw_dir.join("oe1022d.rall"),
        &raw_dir.join("oe1022d.frames.idx.jsonl"),
        start_instant,
    )?;
    let mut raw_replay = RawReplayReader::open(
        &raw_dir.join("oe1022d.rall"),
        &raw_dir.join("oe1022d.frames.idx.jsonl"),
    )?;
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "collector_started",
        "collector",
        None,
        Some(oe_device.device_id.clone()),
        json!({
            "poll_interval_ms": effective_collector_config.poll_interval_ms,
            "ring_capacity_frames": effective_collector_config.ring_capacity_frames,
            "estimated_frames_max": ring_capacity_plan.estimated_frames_max,
            "guard_frames": ring_capacity_plan.guard_frames
        }),
    )?;
    print_stage(&format!(
        "collector 已启动: poll_interval_ms={}, ring_capacity_frames={}, estimated_frames_max={}, guard_frames={}",
        effective_collector_config.poll_interval_ms,
        effective_collector_config.ring_capacity_frames,
        ring_capacity_plan.estimated_frames_max,
        ring_capacity_plan.guard_frames
    ));

    let baseline_current_a = baseline_snapshot.baseline_current_a();
    let mut points_passed = 0_usize;
    let mut points_failed = 0_usize;
    let mut run_error: Option<String> = None;

    for (index, point) in resolved_plan.points.iter().enumerate() {
        print_stage(&format!(
            "point {}/{} {} 开始: target_b_nt={:?}",
            index + 1,
            resolved_plan.points.len(),
            point.point_id,
            point.target_b_nt
        ));
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "point_prepare_started",
            "point",
            Some(point.point_id.clone()),
            None,
            json!({"index": index, "target_b_nt": point.target_b_nt}),
        )?;

        let target_current_a = calibration.target_current_a(baseline_current_a, point.target_b_nt);
        let calibrated_delta_current_a = calibration.delta_current_a(point.target_b_nt);
        ensure_nonnegative_target_currents(&point.point_id, target_current_a)?;

        let point_result = execute_point(
            index,
            &plan,
            &smb_profile,
            &mut smb,
            &mut mag_axes,
            &mut collector,
            &mut raw_replay,
            &mut events_file,
            &mut points_file,
            &mut segments_file,
            &mut quality_file,
            &mut point_fields_file,
            &start_instant,
            &resolved_plan,
            &eta_summary,
            effective_collector_config.poll_interval_ms,
            baseline_current_a,
            calibrated_delta_current_a,
            target_current_a,
            point,
        );

        match point_result {
            Ok(outcome) if outcome.quality_status == "passed" => {
                points_passed += 1;
                print_point_progress(
                    &start_instant,
                    index,
                    resolved_plan.points.len(),
                    point,
                    &outcome,
                    &eta_summary,
                );
            }
            Ok(outcome) => {
                points_failed += 1;
                print_point_progress(
                    &start_instant,
                    index,
                    resolved_plan.points.len(),
                    point,
                    &outcome,
                    &eta_summary,
                );
                if plan.failure_policy == "abort_run" {
                    run_error = Some(format!("point {} 质量不达标，run 终止", point.point_id));
                    break;
                }
            }
            Err(err) => {
                points_failed += 1;
                append_event(
                    &mut events_file,
                    &start_instant,
                    &plan.run_id,
                    "point_failed",
                    "point",
                    Some(point.point_id.clone()),
                    None,
                    json!({"error": err}),
                )?;
                if plan.failure_policy == "abort_run" {
                    run_error = Some(err);
                    break;
                }
            }
        }
    }

    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "cleanup_started",
        "cleanup",
        None,
        None,
        json!({}),
    )?;
    print_stage("cleanup 开始");

    let mut cleanup_errors = Vec::new();
    if let Err(err) = cleanup_smb(&mut smb) {
        cleanup_errors.push(format!("SMB cleanup 失败: {err}"));
    }
    if let Err(err) = cleanup_laser(laser.as_mut()) {
        cleanup_errors.push(format!("Laser cleanup 失败: {err}"));
    }
    if let Err(err) = cleanup_mag_axes(&mut mag_axes) {
        cleanup_errors.push(format!("磁场轴 cleanup 失败: {err}"));
    }

    let collector_summary = collector.stop_join()?;
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "collector_stopped",
        "collector",
        None,
        Some(oe_device.device_id.clone()),
        json!({
            "frames_total": collector_summary.frames_total,
            "timeout_count": collector_summary.timeout_count
        }),
    )?;

    if cleanup_errors.is_empty() {
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "cleanup_completed",
            "cleanup",
            None,
            None,
            json!({}),
        )?;
        print_stage("cleanup 完成");
    }

    let ended_at = now_ts_string();
    let final_status = if !cleanup_errors.is_empty() {
        "cleanup_failed".to_string()
    } else if let Some(err) = &run_error {
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "run_failed",
            "run",
            None,
            None,
            json!({"error": err}),
        )?;
        "failed".to_string()
    } else if points_failed > 0 {
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "run_completed",
            "run",
            None,
            None,
            json!({"status": "completed_with_failed_points"}),
        )?;
        "completed_with_failed_points".to_string()
    } else {
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "run_completed",
            "run",
            None,
            None,
            json!({"status": "completed"}),
        )?;
        "completed".to_string()
    };

    let summary = SummaryRecord {
        run_id: plan.run_id.clone(),
        status: final_status.clone(),
        points_total: resolved_plan.points.len(),
        points_passed,
        points_failed,
        frames_total: collector_summary.frames_total,
        started_at: created_at,
        ended_at,
        failure: run_error.or_else(|| {
            if cleanup_errors.is_empty() {
                None
            } else {
                Some(cleanup_errors.join("; "))
            }
        }),
    };
    write_pretty_json(&target_dir.join("summary.json"), &summary)?;
    manifest.status = final_status.clone();
    write_pretty_json(&target_dir.join("run_manifest.json"), &manifest)?;

    if final_status == "completed" || final_status == "completed_with_failed_points" {
        println!("run execute 完成: {}", plan.run_id);
        println!("产物目录: {}", target_dir.display());
        Ok(target_dir)
    } else {
        Err(format!(
            "run execute 结束状态为 {final_status}，产物目录: {}",
            target_dir.display()
        ))
    }
}

#[allow(clippy::too_many_arguments)]
fn execute_point(
    index: usize,
    plan: &AcquisitionRunPlan,
    smb_profile: &Smb100aRunProfile,
    smb: &mut Smb100aTransport,
    mag_axes: &mut [MagAxisHandle],
    collector: &mut CollectorHandle,
    raw_replay: &mut RawReplayReader,
    events_file: &mut File,
    points_file: &mut File,
    segments_file: &mut File,
    quality_file: &mut File,
    point_fields_file: &mut File,
    start_instant: &Instant,
    resolved_plan: &ResolvedRunPlan,
    _eta_summary: &RunEtaSummary,
    collector_poll_interval_ms: u64,
    baseline_current_a: [f64; 3],
    calibrated_delta_current_a: [f64; 3],
    target_current_a: [f64; 3],
    point: &acquisition_runtime::RunPointPlan,
) -> Result<PointExecutionOutcome, String> {
    set_mag_target_currents(mag_axes, target_current_a)?;
    thread::sleep(Duration::from_millis(plan.point_settle_ms));
    let measured_current_a = read_mag_currents(mag_axes)?;

    let sweep = smb_profile
        .default_sweep
        .apply_override(point.smb_override.as_ref());
    configure_smb_for_point(smb, smb_profile, &sweep)?;

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "point_stable",
        "settle",
        Some(point.point_id.clone()),
        None,
        json!({
            "target_current_a": target_current_a,
            "measured_current_a": measured_current_a
        }),
    )?;

    let segment_id = format!("seg_{}_0000", point.point_id);
    let start_ts = now_ts_string();
    let start_monotonic_ns = monotonic_ns(start_instant);
    let start_cursor = collector.committed_cursor()?;
    let timeouts_before = collector.timeout_count()?;

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "segment_started",
        "segment",
        Some(point.point_id.clone()),
        None,
        json!({"segment_id": segment_id}),
    )?;

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "sweep_started",
        "sweep",
        Some(point.point_id.clone()),
        None,
        json!({
            "segment_id": segment_id,
            "resolved_point_count": resolved_plan.resolved_point_count,
            "estimated_sweep_duration_ms": sweep.estimate().ok().map(|estimate| estimate.sweep_duration_ms)
        }),
    )?;

    smb.send(smb100a_execute_frequency_sweep())
        .map_err(|err| format!("SMB 执行扫频失败: {err}"))?;
    let sweep_completion = wait_for_sweep_complete(smb, &sweep)?;

    if smb_profile.error_check_after_write {
        ensure_smb_no_error(smb)?;
    }

    let end_ts = now_ts_string();
    let end_monotonic_ns = monotonic_ns(start_instant);
    let end_cursor = collector.committed_cursor()?;
    let timeouts_after = collector.timeout_count()?;
    let segment = SegmentRecord {
        schema_version: 1,
        run_id: plan.run_id.clone(),
        segment_id: segment_id.clone(),
        point_id: point.point_id.clone(),
        source: "oe1022d_main".to_string(),
        start_ts,
        end_ts,
        start_monotonic_ns,
        end_monotonic_ns,
        raw_file: "raw/oe1022d.rall".to_string(),
        raw_offset_start: start_cursor.next_raw_offset,
        raw_offset_end: end_cursor.next_raw_offset,
        frame_seq_start: if start_cursor.next_frame_seq < end_cursor.next_frame_seq {
            Some(start_cursor.next_frame_seq)
        } else {
            None
        },
        frame_seq_end: if start_cursor.next_frame_seq < end_cursor.next_frame_seq {
            Some(end_cursor.next_frame_seq - 1)
        } else {
            None
        },
    };
    append_jsonl(segments_file, &segment)?;
    let frames = raw_replay.replay_segment(&segment)?;
    let estimated_frames_expected = sweep.estimate().ok().map(|estimate| {
        estimate_frames_for_quality(estimate.sweep_duration_ms, collector_poll_interval_ms)
    });

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "sweep_completed",
        "sweep",
        Some(point.point_id.clone()),
        None,
        json!({
            "segment_id": segment_id,
            "estimated_sweep_duration_ms": sweep_completion.estimated_sweep_duration_ms,
            "opc_wait_ms": sweep_completion.opc_wait_ms,
            "fallback_used": sweep_completion.fallback_used
        }),
    )?;

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "segment_completed",
        "segment",
        Some(point.point_id.clone()),
        None,
        json!({
            "segment_id": segment_id,
            "frames_total": frames.len(),
            "raw_offset_start": segment.raw_offset_start,
            "raw_offset_end": segment.raw_offset_end
        }),
    )?;

    // 这里先把 point 窗口解析成最小字段级数据，collector 线程本身仍只负责 raw/ring buffer。
    let point_fields =
        build_point_field_record(&plan.run_id, &point.point_id, &segment_id, &frames)
            .map_err(|err| format!("RALL point 字段解析失败: {err}"))?;
    append_jsonl(point_fields_file, &point_fields)?;

    let quality = compute_quality_record(
        &plan.run_id,
        &point.point_id,
        &segment_id,
        end_monotonic_ns,
        &frames,
        &plan.quality_thresholds,
        timeouts_after.saturating_sub(timeouts_before),
        estimated_frames_expected,
    );
    append_jsonl(quality_file, &quality)?;

    let point_record = PointRecord {
        schema_version: 1,
        run_id: plan.run_id.clone(),
        point_id: point.point_id.clone(),
        index,
        target_b_nt: point.target_b_nt,
        baseline_current_a,
        calibrated_delta_current_a,
        target_current_a,
        rf: sweep,
        settle: SettleRecord {
            policy: "fixed_delay_with_readback".to_string(),
            started_at: now_ts_string(),
            settled_at: now_ts_string(),
            status: "passed".to_string(),
            measured_current_a,
        },
    };
    append_jsonl(points_file, &point_record)?;

    if quality.quality_status == "passed" {
        append_event(
            events_file,
            start_instant,
            &plan.run_id,
            "point_completed",
            "point",
            Some(point.point_id.clone()),
            None,
            json!({"quality_status": quality.quality_status}),
        )?;
    }

    Ok(PointExecutionOutcome {
        quality_status: quality.quality_status,
        frames_total: frames.len(),
        collector_frames_total: end_cursor.next_frame_seq,
        timeout_count: timeouts_after,
        ring_retained_frames: collector.retained_ring_frames()?,
    })
}

fn apply_smb_fixed_profile(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    profile: &Smb100aRunProfile,
    events_file: &mut File,
    start_instant: &Instant,
    run_id: &str,
) -> Result<(), String> {
    let commands = vec![
        smb100a_set_modulation_state(profile.fixed.modulation_enabled).to_string(),
        smb100a_set_fm_state(profile.fixed.fm_enabled).to_string(),
        smb100a_set_fm_source(&profile.fixed.fm_source),
        smb100a_set_fm_mode(&profile.fixed.fm_mode),
        smb100a_set_fm_deviation_hz(profile.fixed.fm_deviation_hz),
        smb100a_set_lf_output_state(profile.fixed.lf_output_enabled).to_string(),
        smb100a_set_lf_voltage_mv(profile.fixed.lf_voltage_mv),
        smb100a_set_lf_frequency_hz(profile.fixed.lf_frequency_hz),
        smb100a_set_lf_shape(&profile.fixed.lf_shape),
        smb100a_set_lf_source_impedance(&profile.fixed.lf_source_impedance),
    ];

    for command in commands {
        transport
            .send(&command)
            .map_err(|err| format!("SMB100A 固定配置失败 `{command}`: {err}"))?;
        thread::sleep(Duration::from_millis(profile.command_settle_ms));
        if profile.error_check_after_write {
            ensure_smb_no_error(transport)?;
        }
    }

    append_event(
        events_file,
        start_instant,
        run_id,
        "smb_profile_applied",
        "profile",
        None,
        Some(device.device_id.clone()),
        json!({"profile_id": profile.profile_id}),
    )?;
    Ok(())
}

fn configure_smb_for_point(
    transport: &mut Smb100aTransport,
    profile: &Smb100aRunProfile,
    sweep: &ResolvedSmbSweep,
) -> Result<(), String> {
    let commands = vec![
        smb100a_set_frequency_mode("SWE"),
        smb100a_set_power_dbm(sweep.power_dbm),
        smb100a_set_sweep_start_hz(sweep.start_hz),
        smb100a_set_sweep_stop_hz(sweep.stop_hz),
        smb100a_set_sweep_step_hz(sweep.step_hz),
        smb100a_set_sweep_dwell_ms(sweep.dwell_ms),
        smb100a_set_sweep_mode(&sweep.sweep_mode),
        smb100a_set_sweep_spacing(&sweep.spacing),
        smb100a_set_sweep_shape(&sweep.shape),
        smb100a_set_sweep_trigger_source(&sweep.trigger_source),
        smb100a_set_sweep_output_voltage_start_v(sweep.output_voltage_start_v),
        smb100a_set_sweep_output_voltage_stop_v(sweep.output_voltage_stop_v),
        smb100a_set_output(sweep.rf_output_enabled).to_string(),
    ];

    for command in commands {
        transport
            .send(&command)
            .map_err(|err| format!("SMB100A point 配置失败 `{command}`: {err}"))?;
        thread::sleep(Duration::from_millis(profile.command_settle_ms));
        if profile.error_check_after_write {
            ensure_smb_no_error(transport)?;
        }
    }

    let observed_output = transport
        .query_output()
        .map_err(|err| format!("SMB100A OUTP? 查询失败: {err}"))?;
    let rf_enabled = observed_output.trim() == "1";
    if rf_enabled != sweep.rf_output_enabled {
        return Err(format!(
            "SMB100A RF 输出状态不符: expected={}, observed={observed_output}",
            if sweep.rf_output_enabled { "ON" } else { "OFF" }
        ));
    }

    let observed_frequency_mode = transport
        .query_frequency_mode()
        .map_err(|err| format!("SMB100A FREQ:MODE? 查询失败: {err}"))?;
    if observed_frequency_mode.trim() != "SWE" {
        return Err(format!(
            "SMB100A 频率模式不符: expected=SWE, observed={observed_frequency_mode}"
        ));
    }

    Ok(())
}

fn apply_oe_fixed_profile(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    profile: &Oe1022dRunProfile,
    events_file: &mut File,
    start_instant: &Instant,
    run_id: &str,
) -> Result<(), String> {
    let fixed = &profile.fixed;
    let commands = vec![
        oe1022d_set_input_source(fixed.channel, fixed.input_source),
        oe1022d_set_input_grounding(fixed.channel, fixed.input_grounding),
        oe1022d_set_input_coupling(fixed.channel, fixed.input_coupling),
        oe1022d_set_line_notch_filter(fixed.channel, fixed.line_notch_filter),
        oe1022d_set_reference_source(fixed.channel, fixed.reference_source),
        oe1022d_set_reference_slope(fixed.channel, fixed.reference_slope),
        oe1022d_set_phase_deg(fixed.channel, fixed.phase_deg),
        oe1022d_set_harmonic(fixed.channel, 1, fixed.harmonic_1),
        oe1022d_set_harmonic(fixed.channel, 2, fixed.harmonic_2),
        oe1022d_set_dynamic_reserve(fixed.channel, fixed.dynamic_reserve),
        oe1022d_set_sensitivity_index(fixed.channel, fixed.sensitivity_index),
        oe1022d_set_time_constant_index(fixed.channel, fixed.time_constant_index),
        oe1022d_set_filter_slope(fixed.channel, fixed.filter_slope),
        oe1022d_set_sync_filter(fixed.channel, fixed.sync_filter),
        oe1022d_set_sine_output_mode(fixed.channel, fixed.sine_output_mode),
        oe1022d_set_sine_output_voltage_vrms(fixed.channel, fixed.sine_output_voltage_vrms),
    ];

    for command in commands {
        transport
            .send(&command)
            .map_err(|err| format!("OE1022D 固定配置失败 `{command}`: {err}"))?;
        thread::sleep(Duration::from_millis(profile.command_settle_ms));
    }

    append_event(
        events_file,
        start_instant,
        run_id,
        "oe_profile_applied",
        "profile",
        None,
        Some(device.device_id.clone()),
        json!({"profile_id": profile.profile_id}),
    )?;
    Ok(())
}

fn open_mag_axes(resolved: &StationResolveResult) -> Result<Vec<MagAxisHandle>, String> {
    let mut out = Vec::new();
    for axis_id in ["mag_x", "mag_y", "mag_z"] {
        let device = find_device_by_id(resolved, axis_id)?;
        let mut transport = M8812Transport::open(&serial_m8812_config(device)?)
            .map_err(|err| format!("无法连接磁场轴 {axis_id}: {err}"))?;
        transport
            .enter_remote()
            .map_err(|err| format!("{axis_id} 进入 remote 失败: {err}"))?;
        out.push(MagAxisHandle {
            axis_id: axis_id.to_string(),
            transport,
        });
    }
    Ok(out)
}

fn lock_baseline_once(
    mag_axes: &mut [MagAxisHandle],
    plan: &AcquisitionRunPlan,
    events_file: &mut File,
    start_instant: &Instant,
) -> Result<BaselineSnapshot, String> {
    let policy = &plan.mag_baseline_policy;
    let mut axes = Vec::with_capacity(mag_axes.len());

    for (index, current_a) in policy.baseline_current_a.iter().enumerate() {
        if *current_a < 0.0 {
            return Err(format!(
                "零偏锁定不支持负电流: axis_index={}, baseline_current_a={}A",
                index, current_a
            ));
        }
    }

    for (index, axis) in mag_axes.iter_mut().enumerate() {
        if let Some(voltage_v) = policy.voltage_v {
            axis.transport
                .send(&m8812_set_voltage_v(voltage_v))
                .map_err(|err| format!("{} 设置电压失败: {err}", axis.axis_id))?;
        }
        if let Some(voltage_protection_v) = policy.voltage_protection_v {
            axis.transport
                .send(&m8812_set_voltage_protection_v(voltage_protection_v))
                .map_err(|err| format!("{} 设置过压保护失败: {err}", axis.axis_id))?;
        }

        axis.transport
            .set_current_a(policy.baseline_current_a[index])
            .map_err(|err| format!("{} 设置 baseline 电流失败: {err}", axis.axis_id))?;
        axis.transport
            .set_output(policy.output_enabled)
            .map_err(|err| format!("{} 设置 baseline 输出失败: {err}", axis.axis_id))?;
        thread::sleep(Duration::from_millis(policy.settle_ms));

        let mut readbacks = Vec::new();
        for _ in 0..policy.readback_samples {
            readbacks.push(query_meas_current_a(&mut axis.transport)?);
            thread::sleep(Duration::from_millis(100));
        }
        let locked_zero_offset_current_a = mean_current(&readbacks);
        if (locked_zero_offset_current_a - policy.baseline_current_a[index]).abs()
            > policy.settle_tolerance_a
        {
            return Err(format!(
                "{} 零偏锁定超出容差: setpoint={}A, locked={}A, tolerance={}A",
                axis.axis_id,
                policy.baseline_current_a[index],
                locked_zero_offset_current_a,
                policy.settle_tolerance_a
            ));
        }

        axes.push(BaselineAxisSnapshot {
            axis: axis.axis_id.clone(),
            zero_offset_setpoint_a: policy.baseline_current_a[index],
            zero_offset_measured_samples_a: readbacks,
            locked_zero_offset_current_a: Some(locked_zero_offset_current_a),
        });
    }

    append_event(
        events_file,
        start_instant,
        &plan.run_id,
        "baseline_locked",
        "baseline",
        None,
        None,
        json!({"axes": axes.len(), "mode": "legacy_zero_offset_lock"}),
    )?;

    Ok(BaselineSnapshot {
        schema_version: 1,
        mode: "legacy_zero_offset_lock".to_string(),
        baseline_locked_at: now_ts_string(),
        settle_ms: policy.settle_ms,
        readback_samples: policy.readback_samples,
        settle_tolerance_a: policy.settle_tolerance_a,
        axes,
    })
}

fn set_mag_target_currents(
    mag_axes: &mut [MagAxisHandle],
    target_current_a: [f64; 3],
) -> Result<(), String> {
    for (index, axis) in mag_axes.iter_mut().enumerate() {
        axis.transport
            .set_current_a(target_current_a[index])
            .map_err(|err| format!("{} 设置 point 电流失败: {err}", axis.axis_id))?;
        axis.transport
            .set_output(true)
            .map_err(|err| format!("{} 打开输出失败: {err}", axis.axis_id))?;
    }
    Ok(())
}

fn read_mag_currents(mag_axes: &mut [MagAxisHandle]) -> Result<[f64; 3], String> {
    let mut out = [0.0_f64; 3];
    for (index, axis) in mag_axes.iter_mut().enumerate() {
        out[index] = query_meas_current_a(&mut axis.transport)?;
    }
    Ok(out)
}

fn query_meas_current_a(transport: &mut M8812Transport) -> Result<f64, String> {
    let response = transport
        .query_meas_current_a()
        .map_err(|err| format!("查询 M8812 电流失败: {err}"))?;
    response
        .trim()
        .parse::<f64>()
        .map_err(|err| format!("M8812 电流回读解析失败 `{response}`: {err}"))
}

fn mean_current(values: &[f64]) -> f64 {
    if values.is_empty() {
        return 0.0;
    }
    values.iter().sum::<f64>() / values.len() as f64
}

fn ensure_nonnegative_target_currents(
    point_id: &str,
    target_current_a: [f64; 3],
) -> Result<(), String> {
    for (axis_index, current_a) in target_current_a.into_iter().enumerate() {
        if current_a < 0.0 {
            return Err(format!(
                "point {} 目标电流出现负值，当前磁场电源第一版不支持负输出: axis_index={}, target_current_a={}A",
                point_id, axis_index, current_a
            ));
        }
    }
    Ok(())
}

fn cleanup_smb(transport: &mut Smb100aTransport) -> Result<(), String> {
    transport
        .send(smb100a_set_output(false))
        .map_err(|err| format!("SMB100A OUTP OFF 失败: {err}"))?;
    transport
        .send(&smb100a_set_frequency_mode("CW"))
        .map_err(|err| format!("SMB100A FREQ:MODE CW 失败: {err}"))?;
    thread::sleep(Duration::from_millis(SMB_CLEANUP_WAIT_MS));
    ensure_smb_no_error(transport)
}

fn cleanup_mag_axes(mag_axes: &mut [MagAxisHandle]) -> Result<(), String> {
    for axis in mag_axes.iter_mut() {
        axis.transport
            .set_current_a(0.0)
            .map_err(|err| format!("{} CURR 0 失败: {err}", axis.axis_id))?;
        axis.transport
            .set_output(false)
            .map_err(|err| format!("{} OUTP 0 失败: {err}", axis.axis_id))?;
        thread::sleep(Duration::from_millis(MAG_CLEANUP_WAIT_MS));
        axis.transport
            .enter_local()
            .map_err(|err| format!("{} SYST:LOC 失败: {err}", axis.axis_id))?;
    }
    Ok(())
}

fn open_optional_laser(
    device: Option<&DeviceSpec>,
    profile: &LaserRunProfile,
) -> Result<Option<LaserHandle>, String> {
    let Some(device) = device else {
        return match profile.mode {
            LaserBackgroundMode::OnBackground => Err(
                "laser_profile 要求 on_background，但 station 中没有可用 laser 设备".to_string(),
            ),
            LaserBackgroundMode::OffBackground => Ok(None),
        };
    };

    let transport = CniLaserTransport::open(&laser_config(device)?)
        .map_err(|err| format!("无法连接 Laser {}: {err}", device.device_id))?;
    Ok(Some(LaserHandle {
        device_id: device.device_id.clone(),
        transport,
    }))
}

fn apply_laser_background_policy(
    laser: &mut Option<LaserHandle>,
    profile: &LaserRunProfile,
    events_file: &mut File,
    start_instant: &Instant,
    run_id: &str,
) -> Result<(), String> {
    match (laser.as_mut(), &profile.mode) {
        (Some(laser), LaserBackgroundMode::OnBackground) => {
            laser
                .transport
                .set_power_mw(profile.power_mw)
                .map_err(|err| format!("Laser 设置功率失败: {err}"))?;
            thread::sleep(Duration::from_millis(profile.settle_ms));
            laser
                .transport
                .output_on()
                .map_err(|err| format!("Laser 开启输出失败: {err}"))?;
            thread::sleep(Duration::from_millis(profile.settle_ms));
            append_event(
                events_file,
                start_instant,
                run_id,
                "laser_profile_applied",
                "laser",
                None,
                Some(laser.device_id.clone()),
                json!({
                    "profile_id": profile.profile_id,
                    "mode": profile.mode.as_str(),
                    "power_mw": profile.power_mw
                }),
            )
        }
        (Some(laser), LaserBackgroundMode::OffBackground) => {
            laser
                .transport
                .output_off()
                .map_err(|err| format!("Laser 关闭输出失败: {err}"))?;
            thread::sleep(Duration::from_millis(profile.settle_ms));
            append_event(
                events_file,
                start_instant,
                run_id,
                "laser_profile_applied",
                "laser",
                None,
                Some(laser.device_id.clone()),
                json!({
                    "profile_id": profile.profile_id,
                    "mode": profile.mode.as_str(),
                    "power_mw": profile.power_mw
                }),
            )
        }
        (None, LaserBackgroundMode::OffBackground) => append_event(
            events_file,
            start_instant,
            run_id,
            "laser_profile_applied",
            "laser",
            None,
            None,
            json!({
                "profile_id": profile.profile_id,
                "mode": profile.mode.as_str(),
                "power_mw": profile.power_mw,
                "status": "no_laser_device"
            }),
        ),
        (None, LaserBackgroundMode::OnBackground) => {
            Err("laser_profile 要求 on_background，但没有打开 laser transport".to_string())
        }
    }
}

fn cleanup_laser(laser: Option<&mut LaserHandle>) -> Result<(), String> {
    let Some(laser) = laser else {
        return Ok(());
    };
    laser
        .transport
        .output_off()
        .map_err(|err| format!("Laser output_off 失败: {err}"))?;
    Ok(())
}

fn ensure_smb_no_error(transport: &mut Smb100aTransport) -> Result<(), String> {
    let response = transport
        .query(smb100a_query_error_next())
        .map_err(|err| format!("SMB100A 查询错误队列失败: {err}"))?;
    if response.trim().starts_with('0') {
        Ok(())
    } else {
        Err(format!("SMB100A error queue 非空: {response}"))
    }
}

struct SweepCompletionObservation {
    estimated_sweep_duration_ms: u64,
    opc_wait_ms: u64,
    fallback_used: bool,
}

struct LaserHandle {
    device_id: String,
    transport: CniLaserTransport,
}

struct PointExecutionOutcome {
    quality_status: String,
    frames_total: usize,
    collector_frames_total: u64,
    timeout_count: usize,
    ring_retained_frames: usize,
}

fn wait_for_sweep_complete(
    transport: &mut Smb100aTransport,
    sweep: &ResolvedSmbSweep,
) -> Result<SweepCompletionObservation, String> {
    let estimate = sweep
        .estimate()
        .map_err(|err| format!("SMB sweep 时长估算失败: {err}"))?;
    let opc_started = Instant::now();
    let response = transport
        .query_operation_complete()
        .map_err(|err| format!("SMB100A *OPC? 查询失败: {err}"))?;
    if response.trim() != "1" {
        return Err(format!("SMB100A *OPC? 返回异常: {response}"));
    }

    let opc_wait_ms = opc_started.elapsed().as_millis() as u64;
    let fallback_used =
        opc_wait_ms.saturating_add(SWEEP_COMPLETION_GUARD_MS) < estimate.sweep_duration_ms;
    if fallback_used {
        let remaining_ms = estimate
            .sweep_duration_ms
            .saturating_sub(opc_wait_ms)
            .saturating_add(SWEEP_COMPLETION_GUARD_MS);
        thread::sleep(Duration::from_millis(remaining_ms));
    }

    Ok(SweepCompletionObservation {
        estimated_sweep_duration_ms: estimate.sweep_duration_ms,
        opc_wait_ms,
        fallback_used,
    })
}

fn build_plan_snapshot(plan: &AcquisitionRunPlan, resolved_plan: &ResolvedRunPlan) -> PlanSnapshot {
    PlanSnapshot {
        schema_version: 1,
        run_id: plan.run_id.clone(),
        source_kind: resolved_plan.source_kind.as_str().to_string(),
        declared_point_count: resolved_plan.declared_point_count,
        resolved_point_count: resolved_plan.resolved_point_count,
        fixed_total_points: resolved_plan.fixed_total_points,
        cycle_mode: resolved_plan.cycle_mode.clone(),
        estimated_sweep: resolved_plan.estimated_sweep.clone(),
        estimated_point_duration_ms: resolved_plan.estimated_point_duration_ms,
        estimated_run_duration_ms: resolved_plan.estimated_run_duration_ms,
        source_plan: plan.clone(),
        resolved_points: resolved_plan.points.clone(),
    }
}

fn build_run_eta_summary(
    plan: &AcquisitionRunPlan,
    smb_profile: &Smb100aRunProfile,
    resolved_plan: &ResolvedRunPlan,
) -> Result<RunEtaSummary, String> {
    if resolved_plan.points.is_empty() {
        return Ok(RunEtaSummary {
            estimated_point_duration_ms: None,
            estimated_total_duration_ms: Some(0),
            estimated_end_at: Some(SystemTime::now()),
        });
    }

    let mut point_durations_ms = Vec::with_capacity(resolved_plan.points.len());
    for point in &resolved_plan.points {
        let sweep = smb_profile
            .default_sweep
            .apply_override(point.smb_override.as_ref());
        let estimate = sweep
            .estimate()
            .map_err(|err| format!("point {} sweep 估算失败: {err}", point.point_id))?;
        point_durations_ms.push(estimated_point_duration_ms(
            plan.point_settle_ms,
            smb_profile.estimated_point_configuration_ms(),
            estimate.sweep_duration_ms,
        ));
    }

    let estimated_total_duration_ms = point_durations_ms
        .iter()
        .copied()
        .fold(0_u64, u64::saturating_add);
    let estimated_point_duration_ms = if point_durations_ms
        .iter()
        .all(|duration| *duration == point_durations_ms[0])
    {
        Some(point_durations_ms[0])
    } else {
        None
    };

    Ok(RunEtaSummary {
        estimated_point_duration_ms,
        estimated_total_duration_ms: Some(estimated_total_duration_ms),
        estimated_end_at: Some(
            SystemTime::now() + Duration::from_millis(estimated_total_duration_ms),
        ),
    })
}

fn build_ring_capacity_plan(
    smb_profile: &Smb100aRunProfile,
    collector: &CollectorConfig,
    resolved_plan: &ResolvedRunPlan,
) -> Result<RingCapacityPlan, String> {
    let mut estimated_frames_max = 1_usize;
    for point in &resolved_plan.points {
        let sweep = smb_profile
            .default_sweep
            .apply_override(point.smb_override.as_ref());
        let estimate = sweep
            .estimate()
            .map_err(|err| format!("point {} sweep 估算失败: {err}", point.point_id))?;
        estimated_frames_max = estimated_frames_max.max(estimate_frames_for_quality(
            estimate.sweep_duration_ms,
            collector.poll_interval_ms,
        ));
    }

    let guard_frames =
        estimate_frames_for_quality(collector.guard_margin_ms, collector.poll_interval_ms);
    Ok(RingCapacityPlan {
        effective_capacity_frames: estimated_frames_max
            .saturating_add(guard_frames)
            .max(collector.ring_capacity_frames.max(1)),
        estimated_frames_max,
        guard_frames,
    })
}

fn estimated_point_duration_ms(
    point_settle_ms: u64,
    point_configuration_ms: u64,
    estimated_sweep_duration_ms: u64,
) -> u64 {
    point_settle_ms
        .saturating_add(point_configuration_ms)
        .saturating_add(estimated_sweep_duration_ms)
        .saturating_add(SWEEP_COMPLETION_GUARD_MS)
}

fn estimate_frames_for_quality(duration_ms: u64, poll_interval_ms: u64) -> usize {
    if duration_ms == 0 || poll_interval_ms == 0 {
        return 0;
    }
    duration_ms
        .saturating_add(poll_interval_ms - 1)
        .saturating_div(poll_interval_ms) as usize
}

fn print_stage(message: &str) {
    println!("[odmr] {message}");
}

fn print_run_eta_summary(eta_summary: &RunEtaSummary) {
    let point_duration = eta_summary
        .estimated_point_duration_ms
        .map(format_duration_ms)
        .unwrap_or_else(|| "按 point 覆盖动态估算".to_string());
    let total_duration = eta_summary
        .estimated_total_duration_ms
        .map(format_duration_ms)
        .unwrap_or_else(|| "未知".to_string());
    let end_time = eta_summary
        .estimated_end_at
        .map(format_local_time)
        .unwrap_or_else(|| "未知".to_string());
    println!(
        "[odmr] 预计单点时长={}, 预计总时长={}, 预计结束时间={}",
        point_duration, total_duration, end_time
    );
}

fn print_point_progress(
    start_instant: &Instant,
    index: usize,
    total_points: usize,
    point: &acquisition_runtime::RunPointPlan,
    outcome: &PointExecutionOutcome,
    eta_summary: &RunEtaSummary,
) {
    let elapsed_s = start_instant.elapsed().as_secs_f64();
    let estimated_sample_rate = if elapsed_s > 0.0 {
        outcome.collector_frames_total as f64 * 50.0 / elapsed_s
    } else {
        0.0
    };
    let completed_points = index + 1;
    let progress_percent = if total_points == 0 {
        0.0
    } else {
        (completed_points as f64 / total_points as f64) * 100.0
    };
    let elapsed_ms = start_instant.elapsed().as_millis() as u64;
    let remaining_ms = eta_summary
        .estimated_total_duration_ms
        .map(|estimated| estimated.saturating_sub(elapsed_ms));
    let eta_end =
        remaining_ms.map(|remaining| SystemTime::now() + Duration::from_millis(remaining));
    println!(
        "[odmr] point {}/{} {} 完成: target_b_nt={:?}, frames_total={}, quality_status={}, collector_frames_total={}, ring_retained_frames={}, timeout_count={}, 估算点速率={:.1} pts/s, progress={:.1}%, 已耗时={}, 剩余预计时长={}, 预计结束时间={}",
        index + 1,
        total_points,
        point.point_id,
        point.target_b_nt,
        outcome.frames_total,
        outcome.quality_status,
        outcome.collector_frames_total,
        outcome.ring_retained_frames,
        outcome.timeout_count,
        estimated_sample_rate,
        progress_percent,
        format_duration_ms(elapsed_ms),
        remaining_ms
            .map(format_duration_ms)
            .unwrap_or_else(|| "未知".to_string()),
        eta_end
            .map(format_local_time)
            .unwrap_or_else(|| "未知".to_string())
    );
}

fn format_duration_ms(duration_ms: u64) -> String {
    let total_seconds = duration_ms / 1000;
    let hours = total_seconds / 3600;
    let minutes = (total_seconds % 3600) / 60;
    let seconds = total_seconds % 60;
    if hours > 0 {
        format!("{hours:02}:{minutes:02}:{seconds:02}")
    } else {
        format!("{minutes:02}:{seconds:02}")
    }
}

fn format_local_time(system_time: SystemTime) -> String {
    let date_time: DateTime<Local> = DateTime::<Local>::from(system_time);
    date_time.format("%Y-%m-%d %H:%M:%S").to_string()
}

struct RawReplayReader {
    raw_path: PathBuf,
    index_path: PathBuf,
    index_bytes_read: u64,
    cached_index: Vec<FrameIndexRecord>,
}

impl RawReplayReader {
    fn open(raw_path: &Path, index_path: &Path) -> Result<Self, String> {
        Ok(Self {
            raw_path: raw_path.to_path_buf(),
            index_path: index_path.to_path_buf(),
            index_bytes_read: 0,
            cached_index: Vec::new(),
        })
    }

    fn refresh_index(&mut self) -> Result<(), String> {
        let mut reader = BufReader::new(
            OpenOptions::new()
                .read(true)
                .open(&self.index_path)
                .map_err(|err| {
                    format!(
                        "无法打开 frame index 文件 {}: {err}",
                        self.index_path.display()
                    )
                })?,
        );
        reader
            .seek(SeekFrom::Start(self.index_bytes_read))
            .map_err(|err| format!("frame index seek 失败: {err}"))?;

        let mut line = String::new();
        loop {
            line.clear();
            let bytes = reader
                .read_line(&mut line)
                .map_err(|err| format!("frame index 读取失败: {err}"))?;
            if bytes == 0 {
                break;
            }
            self.index_bytes_read = self.index_bytes_read.saturating_add(bytes as u64);
            if line.trim().is_empty() {
                continue;
            }
            let record: FrameIndexRecord =
                serde_json::from_str(line.trim_end()).map_err(|err| {
                    format!(
                        "frame index JSON 解析失败 {}: {err}",
                        self.index_path.display()
                    )
                })?;
            self.cached_index.push(record);
        }

        Ok(())
    }

    fn replay_segment(&mut self, segment: &SegmentRecord) -> Result<Vec<CollectorFrame>, String> {
        self.refresh_index()?;
        let Some(frame_seq_start) = segment.frame_seq_start else {
            return Ok(Vec::new());
        };
        let Some(frame_seq_end) = segment.frame_seq_end else {
            return Ok(Vec::new());
        };
        if frame_seq_end < frame_seq_start {
            return Err(format!(
                "segment {} frame_seq 边界非法: start={}, end={}",
                segment.segment_id, frame_seq_start, frame_seq_end
            ));
        }
        if self.cached_index.len() <= frame_seq_end as usize {
            return Err(format!(
                "frame index 未覆盖 segment {}: need_end_seq={}, cached_len={}",
                segment.segment_id,
                frame_seq_end,
                self.cached_index.len()
            ));
        }

        let mut raw_file = OpenOptions::new()
            .read(true)
            .open(&self.raw_path)
            .map_err(|err| format!("无法打开 raw 文件 {}: {err}", self.raw_path.display()))?;
        let mut frames = Vec::with_capacity((frame_seq_end - frame_seq_start + 1) as usize);

        for frame_seq in frame_seq_start..=frame_seq_end {
            let record = self
                .cached_index
                .get(frame_seq as usize)
                .ok_or_else(|| format!("frame index 缺少 frame_seq={frame_seq}"))?;
            if record.frame_seq != frame_seq {
                return Err(format!(
                    "frame index frame_seq 不连续: expected={}, actual={}",
                    frame_seq, record.frame_seq
                ));
            }
            let mut payload = vec![0_u8; record.raw_len];
            raw_file
                .seek(SeekFrom::Start(record.raw_offset))
                .map_err(|err| format!("raw seek 失败 offset={}: {err}", record.raw_offset))?;
            raw_file
                .read_exact(&mut payload)
                .map_err(|err| format!("raw 读取失败 frame_seq={frame_seq}: {err}"))?;
            frames.push(CollectorFrame {
                frame_seq: record.frame_seq,
                ts: record.ts.clone(),
                monotonic_ns: record.monotonic_ns,
                raw_offset: record.raw_offset,
                payload,
                duplicate_of: record.duplicate_of,
            });
        }

        Ok(frames)
    }
}

#[allow(clippy::too_many_arguments)]
fn append_event(
    file: &mut File,
    start_instant: &Instant,
    run_id: &str,
    event: &str,
    phase: &str,
    point_id: Option<String>,
    device: Option<String>,
    data: serde_json::Value,
) -> Result<(), String> {
    let record = EventRecord {
        ts: now_ts_string(),
        monotonic_ns: monotonic_ns(start_instant),
        event: event.to_string(),
        run_id: run_id.to_string(),
        point_id,
        device,
        phase: phase.to_string(),
        data,
    };
    append_jsonl(file, &record)
}

fn open_jsonl_writer(path: &Path) -> Result<File, String> {
    if let Some(parent) = path.parent() {
        fs::create_dir_all(parent)
            .map_err(|err| format!("无法创建目录 {}: {err}", parent.display()))?;
    }
    OpenOptions::new()
        .create(true)
        .write(true)
        .truncate(true)
        .open(path)
        .map_err(|err| format!("无法打开 JSONL 文件 {}: {err}", path.display()))
}

fn append_jsonl<T: Serialize>(file: &mut File, value: &T) -> Result<(), String> {
    let line = serde_json::to_string(value).map_err(|err| format!("JSONL 序列化失败: {err}"))?;
    writeln!(file, "{line}").map_err(|err| format!("JSONL 写入失败: {err}"))
}

fn write_pretty_json<T: Serialize>(path: &Path, value: &T) -> Result<(), String> {
    let text =
        serde_json::to_string_pretty(value).map_err(|err| format!("JSON 序列化失败: {err}"))?;
    fs::write(path, text).map_err(|err| format!("无法写入 {}: {err}", path.display()))
}

fn read_json_file<T: DeserializeOwned>(path: &Path) -> Result<T, String> {
    let text = fs::read_to_string(path)
        .map_err(|err| format!("无法读取配置 {}: {err}", path.display()))?;
    serde_json::from_str(&text).map_err(|err| format!("JSON 解析失败 {}: {err}", path.display()))
}

fn resolve_run_output_dir(run_id: &str, out_dir: Option<&Path>) -> PathBuf {
    if let Some(out_dir) = out_dir {
        return out_dir.to_path_buf();
    }
    let stamp = now_ts_string().replace([':', '-'], "");
    PathBuf::from("runs").join(format!("{run_id}_{stamp}"))
}

fn now_ts_string() -> String {
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("system time before unix epoch");
    let secs = now.as_secs();
    let millis = now.subsec_millis();
    format!("{secs}.{millis:03}Z")
}

fn monotonic_ns(start_instant: &Instant) -> u64 {
    start_instant.elapsed().as_nanos() as u64
}

fn find_first_device(
    resolved: &StationResolveResult,
    kind: DeviceKind,
) -> Result<&DeviceSpec, String> {
    resolved
        .resolved_spec
        .devices
        .iter()
        .find(|device| device.kind == kind)
        .ok_or_else(|| format!("station 中缺少设备 kind={kind:?}"))
}

fn find_optional_device(resolved: &StationResolveResult, kind: DeviceKind) -> Option<&DeviceSpec> {
    resolved
        .resolved_spec
        .devices
        .iter()
        .find(|device| device.kind == kind)
}

fn find_device_by_id<'a>(
    resolved: &'a StationResolveResult,
    device_id: &str,
) -> Result<&'a DeviceSpec, String> {
    resolved
        .resolved_spec
        .devices
        .iter()
        .find(|device| device.device_id == device_id)
        .ok_or_else(|| format!("station 中缺少设备 {device_id}"))
}

fn tcp_config(device: &DeviceSpec) -> Result<Smb100aTransportConfig, String> {
    let TransportHint::TcpSocket { host, port } = &device.transport_hint else {
        return Err(format!("设备 {} 不是 tcp_socket", device.device_id));
    };
    Ok(Smb100aTransportConfig {
        host: host.clone(),
        port: *port,
        ..Smb100aTransportConfig::default()
    })
}

fn oe_config(device: &DeviceSpec) -> Result<Oe1022dTransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!("设备 {} 不是 serial_port", device.device_id));
    };
    Ok(Oe1022dTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..Oe1022dTransportConfig::default()
    })
}

fn serial_m8812_config(device: &DeviceSpec) -> Result<M8812TransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!("设备 {} 不是 serial_port", device.device_id));
    };
    Ok(M8812TransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..M8812TransportConfig::default()
    })
}

fn laser_config(device: &DeviceSpec) -> Result<CniLaserTransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!("设备 {} 不是 serial_port", device.device_id));
    };
    Ok(CniLaserTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..CniLaserTransportConfig::default()
    })
}

struct MagAxisHandle {
    axis_id: String,
    transport: M8812Transport,
}

struct CollectorSharedState {
    ring: FrameRingBuffer,
    timeout_count: usize,
    committed_cursor: CollectorCursor,
}

impl CollectorSharedState {
    fn record_polled_frame(
        &mut self,
        ts: String,
        monotonic_ns: u64,
        payload: Vec<u8>,
    ) -> CollectorFrame {
        self.ring.push(ts, monotonic_ns, payload)
    }

    fn mark_committed(&mut self, frame: &CollectorFrame) {
        self.committed_cursor = CollectorCursor {
            next_frame_seq: frame.frame_seq + 1,
            next_raw_offset: frame.raw_offset + frame.raw_len() as u64,
        };
    }
}

struct CollectorStopSummary {
    frames_total: u64,
    timeout_count: usize,
}

struct CollectorHandle {
    stop_flag: Arc<AtomicBool>,
    shared: Arc<Mutex<CollectorSharedState>>,
    join: Option<JoinHandle<Result<CollectorStopSummary, String>>>,
}

impl CollectorHandle {
    fn start(
        device: &DeviceSpec,
        config: &CollectorConfig,
        raw_path: &Path,
        index_path: &Path,
        run_start_instant: Instant,
    ) -> Result<Self, String> {
        let stop_flag = Arc::new(AtomicBool::new(false));
        let shared = Arc::new(Mutex::new(CollectorSharedState {
            ring: FrameRingBuffer::new(config.ring_capacity_frames),
            timeout_count: 0,
            committed_cursor: CollectorCursor {
                next_frame_seq: 0,
                next_raw_offset: 0,
            },
        }));
        let device_config = oe_config(device)?;
        let poll_interval = config.poll_interval_ms;
        let frame_exact_bytes = config.frame_exact_bytes;
        let frame_max_bytes = config.frame_max_bytes;
        let stop_clone = Arc::clone(&stop_flag);
        let shared_clone = Arc::clone(&shared);
        let raw_path = raw_path.to_path_buf();
        let index_path = index_path.to_path_buf();

        let join = thread::spawn(move || {
            let mut transport = Oe1022dTransport::open(&device_config)
                .map_err(|err| format!("collector 打开 OE1022D 失败: {err}"))?;
            let mut raw_file = OpenOptions::new()
                .create(true)
                .write(true)
                .truncate(true)
                .open(&raw_path)
                .map_err(|err| format!("无法打开 raw 文件 {}: {err}", raw_path.display()))?;
            let mut index_file = OpenOptions::new()
                .create(true)
                .write(true)
                .truncate(true)
                .open(&index_path)
                .map_err(|err| {
                    format!("无法打开 frame index 文件 {}: {err}", index_path.display())
                })?;
            let mut next_poll_deadline = run_start_instant;

            loop {
                if stop_clone.load(Ordering::SeqCst) {
                    break;
                }

                let now = Instant::now();
                if now < next_poll_deadline {
                    thread::sleep(next_poll_deadline.saturating_duration_since(now));
                }

                let _ = transport.clear_input();
                match if frame_exact_bytes > 0 {
                    transport.query_rall_frame(frame_exact_bytes)
                } else {
                    transport.query_rall_frame_until_timeout(frame_max_bytes)
                } {
                    Ok(payload) if !payload.is_empty() => {
                        let ts = now_ts_string();
                        let monotonic_ns = monotonic_ns(&run_start_instant);

                        let frame = {
                            let mut guard = shared_clone
                                .lock()
                                .map_err(|_| "collector ring buffer mutex poisoned".to_string())?;
                            guard.record_polled_frame(ts, monotonic_ns, payload)
                        };

                        raw_file
                            .write_all(&frame.payload)
                            .map_err(|err| format!("写入 raw 文件失败: {err}"))?;
                        append_jsonl(&mut index_file, &frame.index_record())?;
                        let mut guard = shared_clone
                            .lock()
                            .map_err(|_| "collector ring buffer mutex poisoned".to_string())?;
                        guard.mark_committed(&frame);
                    }
                    Ok(_) => {
                        let mut guard = shared_clone
                            .lock()
                            .map_err(|_| "collector ring buffer mutex poisoned".to_string())?;
                        guard.timeout_count += 1;
                    }
                    Err(_) => {
                        let mut guard = shared_clone
                            .lock()
                            .map_err(|_| "collector ring buffer mutex poisoned".to_string())?;
                        guard.timeout_count += 1;
                    }
                }

                next_poll_deadline += Duration::from_millis(poll_interval);
                let now = Instant::now();
                if next_poll_deadline <= now {
                    next_poll_deadline = now;
                }
            }

            let guard = shared_clone
                .lock()
                .map_err(|_| "collector ring buffer mutex poisoned".to_string())?;
            Ok(CollectorStopSummary {
                frames_total: guard.committed_cursor.next_frame_seq,
                timeout_count: guard.timeout_count,
            })
        });

        Ok(Self {
            stop_flag,
            shared,
            join: Some(join),
        })
    }

    fn committed_cursor(&self) -> Result<CollectorCursor, String> {
        let guard = self
            .shared
            .lock()
            .map_err(|_| "collector state mutex poisoned".to_string())?;
        Ok(guard.committed_cursor)
    }

    fn retained_ring_frames(&self) -> Result<usize, String> {
        let guard = self
            .shared
            .lock()
            .map_err(|_| "collector state mutex poisoned".to_string())?;
        Ok(guard.ring.total_retained_frames())
    }

    fn timeout_count(&self) -> Result<usize, String> {
        let guard = self
            .shared
            .lock()
            .map_err(|_| "collector state mutex poisoned".to_string())?;
        Ok(guard.timeout_count)
    }

    fn stop_join(mut self) -> Result<CollectorStopSummary, String> {
        self.stop_flag.store(true, Ordering::SeqCst);
        let Some(join) = self.join.take() else {
            return Err("collector join handle 不存在".to_string());
        };
        join.join()
            .map_err(|_| "collector 线程 join 失败".to_string())?
    }
}

#[cfg(test)]
mod tests {
    use super::{
        build_ring_capacity_plan, ensure_nonnegative_target_currents, open_jsonl_writer,
        CollectorSharedState, RawReplayReader,
    };
    use acquisition_runtime::{
        CollectorConfig, CollectorCursor, FrameIndexRecord, FrameRingBuffer, SegmentRecord,
        Smb100aFixedProfile, Smb100aRunProfile, SmbSweepDefaults,
    };
    use std::fs;
    use std::io::Write;
    use std::path::PathBuf;
    use std::time::{SystemTime, UNIX_EPOCH};

    #[test]
    fn open_jsonl_writer_truncates_existing_file() {
        let path = unique_temp_path("odmr_run_execute_jsonl_truncate");
        fs::write(&path, "stale\n").unwrap();

        let mut file = open_jsonl_writer(&path).unwrap();
        writeln!(file, "fresh").unwrap();
        drop(file);

        let content = fs::read_to_string(&path).unwrap();
        assert_eq!(content, "fresh\n");
        let _ = fs::remove_file(path);
    }

    #[test]
    fn negative_target_current_is_rejected() {
        let err = ensure_nonnegative_target_currents("p0001", [-0.001, 0.0, 0.0]).unwrap_err();
        assert!(err.contains("不支持负输出"));
    }

    #[test]
    fn committed_cursor_advances_only_after_mark_committed() {
        let mut shared = CollectorSharedState {
            ring: FrameRingBuffer::new(2),
            timeout_count: 0,
            committed_cursor: CollectorCursor {
                next_frame_seq: 0,
                next_raw_offset: 0,
            },
        };
        let frame = shared.record_polled_frame("t1".to_string(), 10, vec![1, 2, 3, 4]);
        assert_eq!(shared.committed_cursor.next_frame_seq, 0);
        assert_eq!(shared.committed_cursor.next_raw_offset, 0);

        shared.mark_committed(&frame);
        assert_eq!(shared.committed_cursor.next_frame_seq, 1);
        assert_eq!(shared.committed_cursor.next_raw_offset, 4);
    }

    #[test]
    fn raw_replay_reader_reconstructs_frames_from_raw_and_index() {
        let raw_path = unique_temp_path("odmr_raw_replay").with_extension("rall");
        let index_path = unique_temp_path("odmr_raw_replay").with_extension("jsonl");
        let frame_a = vec![1_u8, 2, 3, 4];
        let frame_b = vec![5_u8, 6, 7, 8];

        fs::write(&raw_path, [&frame_a[..], &frame_b[..]].concat()).unwrap();
        let records = vec![
            FrameIndexRecord {
                frame_seq: 0,
                ts: "t1".to_string(),
                monotonic_ns: 100,
                raw_offset: 0,
                raw_len: frame_a.len(),
                parse_status: "ok".to_string(),
                duplicate_of: None,
            },
            FrameIndexRecord {
                frame_seq: 1,
                ts: "t2".to_string(),
                monotonic_ns: 200,
                raw_offset: frame_a.len() as u64,
                raw_len: frame_b.len(),
                parse_status: "ok".to_string(),
                duplicate_of: None,
            },
        ];
        let index_text = records
            .iter()
            .map(|record| serde_json::to_string(record).unwrap())
            .collect::<Vec<_>>()
            .join("\n")
            + "\n";
        fs::write(&index_path, index_text).unwrap();

        let segment = SegmentRecord {
            schema_version: 1,
            run_id: "run".to_string(),
            segment_id: "seg".to_string(),
            point_id: "p1".to_string(),
            source: "oe".to_string(),
            start_ts: "s".to_string(),
            end_ts: "e".to_string(),
            start_monotonic_ns: 0,
            end_monotonic_ns: 300,
            raw_file: "raw/oe1022d.rall".to_string(),
            raw_offset_start: 0,
            raw_offset_end: 8,
            frame_seq_start: Some(0),
            frame_seq_end: Some(1),
        };

        let mut reader = RawReplayReader::open(&raw_path, &index_path).unwrap();
        let frames = reader.replay_segment(&segment).unwrap();
        assert_eq!(frames.len(), 2);
        assert_eq!(frames[0].payload, frame_a);
        assert_eq!(frames[1].payload, frame_b);

        let _ = fs::remove_file(raw_path);
        let _ = fs::remove_file(index_path);
    }

    #[test]
    fn ring_capacity_plan_expands_for_long_sweep() {
        let collector = CollectorConfig {
            poll_interval_ms: 48,
            frame_exact_bytes: 12288,
            frame_max_bytes: 16384,
            ring_capacity_frames: 64,
            guard_margin_ms: 3000,
        };
        let smb_profile = Smb100aRunProfile {
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
            default_sweep: SmbSweepDefaults {
                start_hz: 2.83e9,
                stop_hz: 2.89e9,
                step_hz: 5.0e5,
                dwell_ms: 300,
                power_dbm: -10.0,
                sweep_mode: "AUTO".to_string(),
                spacing: "LIN".to_string(),
                shape: "SAWT".to_string(),
                trigger_source: "AUTO".to_string(),
                output_voltage_start_v: 0.0,
                output_voltage_stop_v: 3.0,
                rf_output_enabled: true,
            },
        };
        let resolved_plan = acquisition_runtime::ResolvedRunPlan {
            source_kind: acquisition_runtime::PlanSourceKind::ExplicitPoints,
            declared_point_count: 1,
            resolved_point_count: 1,
            fixed_total_points: None,
            cycle_mode: None,
            estimated_sweep: None,
            estimated_point_duration_ms: None,
            estimated_run_duration_ms: None,
            points: vec![acquisition_runtime::RunPointPlan {
                point_id: "p000001".to_string(),
                target_b_nt: [0.0, 0.0, 0.0],
                smb_override: None,
            }],
        };

        let plan = build_ring_capacity_plan(&smb_profile, &collector, &resolved_plan).unwrap();
        assert!(plan.effective_capacity_frames > 512);
        assert!(plan.estimated_frames_max > 700);
        assert!(plan.guard_frames > 0);
    }

    fn unique_temp_path(prefix: &str) -> PathBuf {
        let nanos = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap()
            .as_nanos();
        std::env::temp_dir().join(format!("{prefix}_{nanos}.jsonl"))
    }
}
