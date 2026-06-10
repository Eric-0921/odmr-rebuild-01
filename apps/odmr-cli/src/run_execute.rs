//! 最小 runtime 真机执行入口。
//!
//! 当前目标很收敛：
//! - 固定 profile 下发
//! - Maynuo baseline 一次性锁定
//! - OE1022D 单 collector 持续采集
//! - point 从 ring buffer 按时间窗拉 segment
//! - 写出第一版 run artifact

use acquisition_runtime::{
    build_point_field_record, compute_quality_record, AcquisitionRunPlan, BaselineAxisSnapshot,
    BaselineSnapshot, CalibrationProfile, CollectorConfig, CollectorCursor, CollectorFrame,
    EventRecord, FrameRingBuffer, Oe1022dRunProfile, PointRecord, ResolvedSmbSweep, RunManifest,
    SegmentRecord, SettleRecord, Smb100aRunProfile, SummaryRecord,
};
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
use std::io::Write;
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

pub fn run_execute(
    station_path: &Path,
    calibration_path: &Path,
    plan_path: &Path,
    smb_profile_path: &Path,
    oe_profile_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec: StationSpec = crate::read_station_spec(station_path)?;
    let calibration: CalibrationProfile = read_json_file(calibration_path)?;
    let plan: AcquisitionRunPlan = read_json_file(plan_path)?;
    let smb_profile: Smb100aRunProfile = read_json_file(smb_profile_path)?;
    let oe_profile: Oe1022dRunProfile = read_json_file(oe_profile_path)?;
    let target_dir = resolve_run_output_dir(&plan.run_id, out_dir);
    fs::create_dir_all(target_dir.join("raw"))
        .map_err(|err| format!("无法创建 run 输出目录 {}: {err}", target_dir.display()))?;

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
    write_pretty_json(&target_dir.join("plan_snapshot.json"), &plan)?;
    write_pretty_json(&target_dir.join("calibration_snapshot.json"), &calibration)?;
    write_pretty_json(&target_dir.join("smb_profile_snapshot.json"), &smb_profile)?;
    write_pretty_json(&target_dir.join("oe_profile_snapshot.json"), &oe_profile)?;

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
    };
    write_pretty_json(&target_dir.join("run_manifest.json"), &manifest)?;

    let start_instant = Instant::now();
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

    let mut mag_axes = open_mag_axes(&resolved)?;
    let baseline_snapshot =
        lock_baseline_once(&mut mag_axes, &plan, &mut events_file, &start_instant)?;
    write_pretty_json(
        &target_dir.join("baseline_snapshot.json"),
        &baseline_snapshot,
    )?;

    if let Some(device) = laser_device {
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "laser_background_policy",
            "laser",
            None,
            Some(device.device_id.clone()),
            json!({"mode": "off_background"}),
        )?;
    }

    let raw_dir = target_dir.join("raw");
    let mut collector = CollectorHandle::start(
        oe_device,
        &oe_profile.collector,
        &raw_dir.join("oe1022d.rall"),
        &raw_dir.join("oe1022d.frames.idx.jsonl"),
        start_instant,
    )?;
    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "collector_started",
        "collector",
        None,
        Some(oe_device.device_id.clone()),
        json!({"poll_interval_ms": oe_profile.collector.poll_interval_ms}),
    )?;

    let baseline_current_a = baseline_snapshot.baseline_current_a();
    let mut points_passed = 0_usize;
    let mut points_failed = 0_usize;
    let mut run_error: Option<String> = None;

    for (index, point) in plan.points.iter().enumerate() {
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

        let point_result = execute_point(
            index,
            &plan,
            &smb_profile,
            &mut smb,
            &mut mag_axes,
            &mut collector,
            &mut events_file,
            &mut points_file,
            &mut segments_file,
            &mut quality_file,
            &mut point_fields_file,
            &start_instant,
            baseline_current_a,
            calibrated_delta_current_a,
            target_current_a,
            point,
        );

        match point_result {
            Ok(quality_status) if quality_status == "passed" => {
                points_passed += 1;
            }
            Ok(_) => {
                points_failed += 1;
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

    let mut cleanup_errors = Vec::new();
    if let Err(err) = cleanup_smb(&mut smb) {
        cleanup_errors.push(format!("SMB cleanup 失败: {err}"));
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
        points_total: plan.points.len(),
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
    events_file: &mut File,
    points_file: &mut File,
    segments_file: &mut File,
    quality_file: &mut File,
    point_fields_file: &mut File,
    start_instant: &Instant,
    baseline_current_a: [f64; 3],
    calibrated_delta_current_a: [f64; 3],
    target_current_a: [f64; 3],
    point: &acquisition_runtime::RunPointPlan,
) -> Result<String, String> {
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
    let start_cursor = collector.snapshot_cursor()?;
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

    if sweep.rf_output_enabled {
        smb.send(smb100a_execute_frequency_sweep())
            .map_err(|err| format!("SMB 执行扫频失败: {err}"))?;
        if smb_profile.error_check_after_write {
            ensure_smb_no_error(smb)?;
        }
    }

    thread::sleep(Duration::from_millis(plan.acquisition_window_ms));

    let end_ts = now_ts_string();
    let end_monotonic_ns = monotonic_ns(start_instant);
    let end_cursor = collector.snapshot_cursor()?;
    let timeouts_after = collector.timeout_count()?;
    let frames = collector.pull_window(start_monotonic_ns, end_monotonic_ns)?;

    let raw_offset_start = frames
        .first()
        .map(|frame| frame.raw_offset)
        .unwrap_or(start_cursor.next_raw_offset);
    let raw_offset_end = frames
        .last()
        .map(|frame| frame.raw_offset + frame.raw_len() as u64)
        .unwrap_or(end_cursor.next_raw_offset);
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
        raw_offset_start,
        raw_offset_end,
        frame_seq_start: frames.first().map(|frame| frame.frame_seq),
        frame_seq_end: frames.last().map(|frame| frame.frame_seq),
    };
    append_jsonl(segments_file, &segment)?;

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
            "frames_total": frames.len()
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

    Ok(quality.quality_status)
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

        axes.push(BaselineAxisSnapshot {
            axis: axis.axis_id.clone(),
            baseline_setpoint_a: policy.baseline_current_a[index],
            measured_current_a: readbacks,
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
        json!({"axes": axes.len()}),
    )?;

    Ok(BaselineSnapshot {
        schema_version: 1,
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
    OpenOptions::new()
        .create(true)
        .append(true)
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

struct MagAxisHandle {
    axis_id: String,
    transport: M8812Transport,
}

struct CollectorSharedState {
    ring: FrameRingBuffer,
    timeout_count: usize,
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
                .append(true)
                .open(&raw_path)
                .map_err(|err| format!("无法打开 raw 文件 {}: {err}", raw_path.display()))?;
            let mut index_file = OpenOptions::new()
                .create(true)
                .append(true)
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
                            guard.ring.push(ts, monotonic_ns, payload)
                        };

                        raw_file
                            .write_all(&frame.payload)
                            .map_err(|err| format!("写入 raw 文件失败: {err}"))?;
                        append_jsonl(&mut index_file, &frame.index_record())?;
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
                frames_total: guard.ring.cursor().next_frame_seq,
                timeout_count: guard.timeout_count,
            })
        });

        Ok(Self {
            stop_flag,
            shared,
            join: Some(join),
        })
    }

    fn snapshot_cursor(&self) -> Result<CollectorCursor, String> {
        let guard = self
            .shared
            .lock()
            .map_err(|_| "collector state mutex poisoned".to_string())?;
        Ok(guard.ring.cursor())
    }

    fn pull_window(
        &self,
        start_monotonic_ns: u64,
        end_monotonic_ns: u64,
    ) -> Result<Vec<CollectorFrame>, String> {
        let guard = self
            .shared
            .lock()
            .map_err(|_| "collector state mutex poisoned".to_string())?;
        Ok(guard.ring.pull_window(start_monotonic_ns, end_monotonic_ns))
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
