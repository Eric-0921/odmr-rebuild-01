//! 三轴磁场零偏锁定与点级电流验证。
//!
//! 目标很收敛：
//! - 只认领并操作三台 M8812
//! - 执行一次旧系统兼容的零偏电流锁定
//! - 按 plan 的 resolved points 验证 `target_b_nt -> target_current_a -> measured_current_a`
//! - 强制 cleanup

use acquisition_runtime::{
    AcquisitionRunPlan, BaselineAxisSnapshot, BaselineSnapshot, CalibrationProfile, EventRecord,
};
use m8812_commands::{m8812_set_voltage_protection_v, m8812_set_voltage_v};
use m8812_transport::{M8812Transport, M8812TransportConfig};
use serde::de::DeserializeOwned;
use serde::Serialize;
use serde_json::json;
use station_resolver::{
    resolve_station, DeviceSpec, StationResolveResult, StationSpec, TransportHint,
};
use std::fs::{self, File, OpenOptions};
use std::io::Write;
use std::path::{Path, PathBuf};
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const MAG_CLEANUP_WAIT_MS: u64 = 500;

pub fn run_hardware_verify_mag_lock(
    station_path: &Path,
    calibration_path: &Path,
    plan_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec: StationSpec = crate::read_station_spec(station_path)?;
    let calibration: CalibrationProfile = read_json_file(calibration_path)?;
    let plan: AcquisitionRunPlan = read_json_file(plan_path)?;
    let resolved_plan = plan
        .resolve_points_without_smb_profile()
        .map_err(|err| format!("mag verify plan 展开失败: {err}"))?;
    let target_dir = resolve_output_dir(&plan.run_id, out_dir);
    fs::create_dir_all(&target_dir)
        .map_err(|err| format!("无法创建磁场验证输出目录 {}: {err}", target_dir.display()))?;
    print_stage(&format!(
        "打开磁场验证: {} -> {}",
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

    let mut events_file = open_jsonl_writer(&target_dir.join("mag_verify_events.jsonl"))?;
    let mut points_file = open_jsonl_writer(&target_dir.join("mag_verify_points.jsonl"))?;
    let start_instant = Instant::now();
    let started_at = now_ts_string();

    append_event(
        &mut events_file,
        &start_instant,
        &plan.run_id,
        "mag_verify_opened",
        "run",
        None,
        None,
        json!({"resolved_point_count": resolved_plan.resolved_point_count}),
    )?;

    let mut mag_axes = open_mag_axes(&resolved)?;
    let baseline_snapshot =
        lock_zero_offset_once(&mut mag_axes, &plan, &mut events_file, &start_instant)?;
    write_pretty_json(
        &target_dir.join("mag_zero_lock_snapshot.json"),
        &baseline_snapshot,
    )?;
    let baseline_current_a = baseline_snapshot.baseline_current_a();
    print_stage(&format!(
        "零偏锁定完成: baseline_current_a={:?}",
        baseline_current_a
    ));

    let mut points_passed = 0_usize;
    let mut points_failed = 0_usize;
    let mut run_error: Option<String> = None;

    for (index, point) in resolved_plan.points.iter().enumerate() {
        let delta_current_a = calibration.delta_current_a(point.target_b_nt);
        let target_current_a = calibration.target_current_a(baseline_current_a, point.target_b_nt);
        ensure_nonnegative_target_currents(&point.point_id, target_current_a)?;
        print_stage(&format!(
            "磁场验证 point {}/{} {}: target_b_nt={:?}",
            index + 1,
            resolved_plan.points.len(),
            point.point_id,
            point.target_b_nt
        ));

        set_mag_target_currents(&mut mag_axes, target_current_a)?;
        std::thread::sleep(Duration::from_millis(plan.point_settle_ms));
        let measured_current_a = read_mag_currents(&mut mag_axes)?;
        let max_abs_error_a = max_abs_axis_error(target_current_a, measured_current_a);
        let status = if max_abs_error_a <= plan.mag_baseline_policy.settle_tolerance_a {
            points_passed += 1;
            "passed"
        } else {
            points_failed += 1;
            "failed"
        };

        append_jsonl(
            &mut points_file,
            &MagVerifyPointRecord {
                schema_version: 1,
                run_id: plan.run_id.clone(),
                point_id: point.point_id.clone(),
                index,
                target_b_nt: point.target_b_nt,
                baseline_current_a,
                calibrated_delta_current_a: delta_current_a,
                target_current_a,
                measured_current_a,
                max_abs_error_a,
                settle_tolerance_a: plan.mag_baseline_policy.settle_tolerance_a,
                status: status.to_string(),
            },
        )?;
        append_event(
            &mut events_file,
            &start_instant,
            &plan.run_id,
            "mag_point_verified",
            "point",
            Some(point.point_id.clone()),
            None,
            json!({
                "target_b_nt": point.target_b_nt,
                "target_current_a": target_current_a,
                "measured_current_a": measured_current_a,
                "max_abs_error_a": max_abs_error_a,
                "status": status
            }),
        )?;

        print_stage(&format!(
            "磁场验证 point {}/{} {} 完成: measured_current_a={:?}, max_abs_error_a={}, status={}",
            index + 1,
            resolved_plan.points.len(),
            point.point_id,
            measured_current_a,
            max_abs_error_a,
            status
        ));

        if status == "failed" && plan.failure_policy == "abort_run" {
            run_error = Some(format!(
                "磁场验证 point {} 超出容差 {}A",
                point.point_id, plan.mag_baseline_policy.settle_tolerance_a
            ));
            break;
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
    print_stage("磁场验证 cleanup 开始");
    let cleanup_result = cleanup_mag_axes(&mut mag_axes);
    if cleanup_result.is_ok() {
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
        print_stage("磁场验证 cleanup 完成");
    }

    let ended_at = now_ts_string();
    let status = if cleanup_result.is_err() {
        "cleanup_failed".to_string()
    } else if run_error.is_some() {
        "failed".to_string()
    } else if points_failed > 0 {
        "completed_with_failed_points".to_string()
    } else {
        "completed".to_string()
    };

    let summary = MagVerifySummary {
        run_id: plan.run_id.clone(),
        status: status.clone(),
        points_total: resolved_plan.points.len(),
        points_passed,
        points_failed,
        baseline_current_a,
        started_at,
        ended_at,
        failure: run_error.or_else(|| cleanup_result.err()),
    };
    write_pretty_json(&target_dir.join("mag_verify_summary.json"), &summary)?;

    if status == "completed" || status == "completed_with_failed_points" {
        println!("磁场验证完成: {}", plan.run_id);
        println!("产物目录: {}", target_dir.display());
        Ok(target_dir)
    } else {
        Err(format!(
            "磁场验证结束状态为 {status}，产物目录: {}",
            target_dir.display()
        ))
    }
}

#[derive(Debug, Serialize)]
struct MagVerifyPointRecord {
    schema_version: u32,
    run_id: String,
    point_id: String,
    index: usize,
    target_b_nt: [f64; 3],
    baseline_current_a: [f64; 3],
    calibrated_delta_current_a: [f64; 3],
    target_current_a: [f64; 3],
    measured_current_a: [f64; 3],
    max_abs_error_a: f64,
    settle_tolerance_a: f64,
    status: String,
}

#[derive(Debug, Serialize)]
struct MagVerifySummary {
    run_id: String,
    status: String,
    points_total: usize,
    points_passed: usize,
    points_failed: usize,
    baseline_current_a: [f64; 3],
    started_at: String,
    ended_at: String,
    failure: Option<String>,
}

struct MagAxisHandle {
    axis_id: String,
    transport: M8812Transport,
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

fn lock_zero_offset_once(
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
            .map_err(|err| format!("{} 设置零偏电流失败: {err}", axis.axis_id))?;
        axis.transport
            .set_output(policy.output_enabled)
            .map_err(|err| format!("{} 打开输出失败: {err}", axis.axis_id))?;
        std::thread::sleep(Duration::from_millis(policy.settle_ms));

        let mut readbacks = Vec::new();
        for _ in 0..policy.readback_samples {
            readbacks.push(query_meas_current_a(&mut axis.transport)?);
            std::thread::sleep(Duration::from_millis(100));
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

fn cleanup_mag_axes(mag_axes: &mut [MagAxisHandle]) -> Result<(), String> {
    for axis in mag_axes.iter_mut() {
        axis.transport
            .set_current_a(0.0)
            .map_err(|err| format!("{} CURR 0 失败: {err}", axis.axis_id))?;
        axis.transport
            .set_output(false)
            .map_err(|err| format!("{} OUTP 0 失败: {err}", axis.axis_id))?;
        std::thread::sleep(Duration::from_millis(MAG_CLEANUP_WAIT_MS));
        axis.transport
            .enter_local()
            .map_err(|err| format!("{} SYST:LOC 失败: {err}", axis.axis_id))?;
    }
    Ok(())
}

fn max_abs_axis_error(target: [f64; 3], observed: [f64; 3]) -> f64 {
    target
        .into_iter()
        .zip(observed)
        .map(|(lhs, rhs)| (lhs - rhs).abs())
        .fold(0.0_f64, f64::max)
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

fn print_stage(message: &str) {
    println!("[mag-verify] {message}");
}

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
    append_jsonl(
        file,
        &EventRecord {
            ts: now_ts_string(),
            monotonic_ns: monotonic_ns(start_instant),
            event: event.to_string(),
            run_id: run_id.to_string(),
            point_id,
            device,
            phase: phase.to_string(),
            data,
        },
    )
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

fn resolve_output_dir(run_id: &str, out_dir: Option<&Path>) -> PathBuf {
    if let Some(out_dir) = out_dir {
        return out_dir.to_path_buf();
    }
    let stamp = now_ts_string().replace([':', '-'], "");
    PathBuf::from("out")
        .join("hardware_verify_mag_lock")
        .join(format!("{run_id}_{stamp}"))
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

fn serial_m8812_config(device: &DeviceSpec) -> Result<M8812TransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
        ..
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

#[cfg(test)]
mod tests {
    use super::{ensure_nonnegative_target_currents, open_jsonl_writer};
    use std::fs;
    use std::io::Write;
    use std::path::PathBuf;
    use std::time::{SystemTime, UNIX_EPOCH};

    #[test]
    fn open_jsonl_writer_truncates_existing_file() {
        let path = unique_temp_path("odmr_mag_verify_jsonl_truncate");
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
        let err = ensure_nonnegative_target_currents("p0001", [0.0, -0.001, 0.0]).unwrap_err();
        assert!(err.contains("不支持负输出"));
    }

    fn unique_temp_path(prefix: &str) -> PathBuf {
        let nanos = SystemTime::now()
            .duration_since(UNIX_EPOCH)
            .unwrap()
            .as_nanos();
        std::env::temp_dir().join(format!("{prefix}_{nanos}.jsonl"))
    }
}
