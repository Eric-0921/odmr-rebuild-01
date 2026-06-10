//! 最小真机核心链路 smoke 验证。
//!
//! 这层只做一件事：把现有 transport 和 station-resolver 串成一个可重复执行的真机验证入口。

use cni_laser_transport::{CniLaserTransport, CniLaserTransportConfig};
use m8812_commands::{m8812_query_idn, m8812_query_meas_current_a};
use m8812_transport::{M8812Transport, M8812TransportConfig};
use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::Serialize;
use smb100a_commands::{
    smb100a_query_error_next, smb100a_query_frequency, smb100a_query_idn, smb100a_query_output,
    smb100a_query_power, smb100a_query_sweep_dwell, smb100a_query_sweep_step,
};
use smb100a_transport::{Smb100aTransport, Smb100aTransportConfig};
use station_resolver::{
    resolve_station, DeviceKind, DeviceSpec, StationResolveResult, StationSnapshot, StationSpec,
    TransportHint,
};
use std::collections::HashMap;
use std::fs;
use std::path::{Path, PathBuf};
use std::thread;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const M8812_MICROTEST_CURRENT_A: f64 = 0.01;
const M8812_SETTLE_MS: u64 = 1000;
const OE_RALL_MAX_BYTES: usize = 16384;

pub fn run_hardware_smoke(station_path: &Path, out_dir: Option<&Path>) -> Result<PathBuf, String> {
    let spec = read_station_spec(station_path)?;
    let mut backend = RealHardwareSmokeBackend;
    let target_dir = resolve_output_dir(out_dir);
    fs::create_dir_all(&target_dir)
        .map_err(|err| format!("无法创建 smoke 输出目录 {}: {err}", target_dir.display()))?;

    let result = execute_hardware_smoke(&spec, &target_dir, &mut backend)?;
    if result.manifest.overall_status == "passed" {
        println!("hardware smoke 成功: {}", spec.station_id);
        println!("产物目录: {}", target_dir.display());
        Ok(target_dir)
    } else {
        Err(format!(
            "hardware smoke 失败: {}，产物目录: {}",
            spec.station_id,
            target_dir.display()
        ))
    }
}

pub fn execute_hardware_smoke<B: HardwareSmokeBackend>(
    spec: &StationSpec,
    out_dir: &Path,
    backend: &mut B,
) -> Result<HardwareSmokeResult, String> {
    let mut recorder = SmokeRecorder::default();
    let started_at = now_ts_string();
    recorder.record_event("station", "-", "station_verify_started", "running", None);

    let resolved = backend.verify_station(spec);
    let station_snapshot = resolved.snapshot;
    let resolved_spec = resolved.resolved_spec;
    write_pretty_json(&out_dir.join("station_snapshot.json"), &station_snapshot)?;

    if station_snapshot.has_required_failures() {
        recorder.record_event(
            "station",
            "-",
            "station_verified",
            "failed",
            Some(format!(
                "required_failures={}",
                station_snapshot.required_failures
            )),
        );
        let manifest = build_manifest(spec, &started_at, &station_snapshot, &HashMap::new());
        write_artifacts(out_dir, &manifest, &recorder)?;
        return Ok(HardwareSmokeResult { manifest });
    }

    recorder.record_event("station", "-", "station_verified", "passed", None);

    let mut phase_statuses: HashMap<String, PhaseStatus> = station_snapshot
        .devices
        .iter()
        .map(|device| {
            (
                device.device_id.clone(),
                if device.verification_status == "verified" {
                    PhaseStatus::NotRun
                } else {
                    PhaseStatus::Failed
                },
            )
        })
        .collect();

    let smb_device = find_first_device(&resolved_spec, DeviceKind::Smb100a)?;
    let smb_passed = backend.run_smb_phase(smb_device, &mut recorder);
    phase_statuses.insert(
        smb_device.device_id.clone(),
        if smb_passed {
            PhaseStatus::Passed
        } else {
            PhaseStatus::Failed
        },
    );

    if smb_passed {
        let axis_devices = find_all_devices(&resolved_spec, DeviceKind::M8812);
        for axis in axis_devices {
            let axis_passed = backend.run_m8812_axis_phase(
                axis,
                M8812_MICROTEST_CURRENT_A,
                M8812_SETTLE_MS,
                &mut recorder,
            );
            phase_statuses.insert(
                axis.device_id.clone(),
                if axis_passed {
                    PhaseStatus::Passed
                } else {
                    PhaseStatus::Failed
                },
            );
        }

        let oe_device = find_first_device(&resolved_spec, DeviceKind::Oe1022d)?;
        let oe_passed = backend.run_oe_phase(oe_device, &mut recorder);
        phase_statuses.insert(
            oe_device.device_id.clone(),
            if oe_passed {
                PhaseStatus::Passed
            } else {
                PhaseStatus::Failed
            },
        );

        if let Some(laser_device) = find_optional_first_device(&resolved_spec, DeviceKind::CniLaser)
        {
            let laser_passed = backend.run_laser_phase(laser_device, &mut recorder);
            phase_statuses.insert(
                laser_device.device_id.clone(),
                if laser_passed {
                    PhaseStatus::Passed
                } else {
                    PhaseStatus::Failed
                },
            );
        }
    } else {
        recorder.record_event(
            "hardware_smoke",
            smb_device.device_id.as_str(),
            "smoke_aborted_after_smb",
            "failed",
            Some("SMB100A 核心 query 失败，后续核心链路未执行".to_string()),
        );
    }

    let manifest = build_manifest(spec, &started_at, &station_snapshot, &phase_statuses);
    write_artifacts(out_dir, &manifest, &recorder)?;

    Ok(HardwareSmokeResult { manifest })
}

pub trait HardwareSmokeBackend {
    fn verify_station(&mut self, spec: &StationSpec) -> StationResolveResult;
    fn run_smb_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool;
    fn run_m8812_axis_phase(
        &mut self,
        device: &DeviceSpec,
        microtest_current_a: f64,
        settle_ms: u64,
        recorder: &mut SmokeRecorder,
    ) -> bool;
    fn run_oe_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool;
    fn run_laser_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool;
}

#[derive(Default)]
pub struct RealHardwareSmokeBackend;

impl HardwareSmokeBackend for RealHardwareSmokeBackend {
    fn verify_station(&mut self, spec: &StationSpec) -> StationResolveResult {
        resolve_station(spec)
    }

    fn run_smb_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
        recorder.record_event(
            "smb100a",
            &device.device_id,
            "phase_started",
            "running",
            None,
        );

        let result = (|| -> Result<(), String> {
            let config = tcp_config(device)?;
            let mut transport =
                Smb100aTransport::connect(&config).map_err(|err| err.to_string())?;

            let idn = query_smb(&mut transport, device, recorder, smb100a_query_idn())?;
            if !identity_matches(device, &idn) {
                return Err(format!("SMB100A 身份不匹配: {idn}"));
            }

            let error = query_smb(&mut transport, device, recorder, smb100a_query_error_next())?;
            let output = query_smb(&mut transport, device, recorder, smb100a_query_output())?;
            let _freq = query_smb(&mut transport, device, recorder, smb100a_query_frequency())?;
            let _power = query_smb(&mut transport, device, recorder, smb100a_query_power())?;
            let _step = query_smb(&mut transport, device, recorder, smb100a_query_sweep_step())?;
            let _dwell = query_smb(
                &mut transport,
                device,
                recorder,
                smb100a_query_sweep_dwell(),
            )?;

            if !(output.trim() == "0" || output.trim().eq_ignore_ascii_case("off")) {
                return Err(format!("SMB100A 输出未关闭: {output}"));
            }

            recorder.record_event(
                "smb100a",
                &device.device_id,
                "phase_completed",
                "passed",
                Some(format!("SYST:ERR?={error}, OUTP?={output}")),
            );
            Ok(())
        })();

        if let Err(message) = result {
            recorder.record_event(
                "smb100a",
                &device.device_id,
                "phase_completed",
                "failed",
                Some(message),
            );
            return false;
        }

        true
    }

    fn run_m8812_axis_phase(
        &mut self,
        device: &DeviceSpec,
        microtest_current_a: f64,
        settle_ms: u64,
        recorder: &mut SmokeRecorder,
    ) -> bool {
        recorder.record_event(
            "m8812",
            &device.device_id,
            "axis_phase_started",
            "running",
            Some(format!("microtest_current_a={microtest_current_a}")),
        );

        let mut logic_ok = true;
        let mut cleanup_ok = true;
        let mut last_error: Option<String> = None;

        let config = match serial_config(device, 9600) {
            Ok(config) => config,
            Err(err) => {
                recorder.record_event(
                    "m8812",
                    &device.device_id,
                    "axis_phase_completed",
                    "failed",
                    Some(err),
                );
                return false;
            }
        };

        let mut transport = match M8812Transport::open(&config) {
            Ok(transport) => transport,
            Err(err) => {
                recorder.record_event(
                    "m8812",
                    &device.device_id,
                    "axis_phase_completed",
                    "failed",
                    Some(err.to_string()),
                );
                return false;
            }
        };

        if let Err(err) = run_m8812_sequence(
            &mut transport,
            device,
            microtest_current_a,
            settle_ms,
            recorder,
        ) {
            logic_ok = false;
            last_error = Some(err);
        }

        if let Err(err) = run_m8812_cleanup(&mut transport, device, recorder) {
            cleanup_ok = false;
            last_error = Some(err);
        }

        let passed = logic_ok && cleanup_ok;
        recorder.record_event(
            "m8812",
            &device.device_id,
            "axis_phase_completed",
            if passed { "passed" } else { "failed" },
            last_error,
        );
        passed
    }

    fn run_oe_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
        recorder.record_event(
            "oe1022d",
            &device.device_id,
            "phase_started",
            "running",
            None,
        );

        let result = (|| -> Result<(), String> {
            let config = oe_config(device)?;
            let mut transport = Oe1022dTransport::open(&config).map_err(|err| err.to_string())?;

            let idn = query_oe_text(&mut transport, device, recorder, "*IDN?")?;
            if !identity_matches(device, &idn) {
                return Err(format!("OE1022D 身份不匹配: {idn}"));
            }

            transport.clear_input().map_err(|err| err.to_string())?;
            recorder.record_event("oe1022d", &device.device_id, "clear_input", "passed", None);

            let started = Instant::now();
            let payload = transport
                .query_rall_frame_until_timeout(OE_RALL_MAX_BYTES)
                .map_err(|err| err.to_string())?;
            let duration_ms = started.elapsed().as_millis();
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "query_binary".to_string(),
                command_text: Some("RALL?".to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(format!(
                    "len={}, head={}",
                    payload.len(),
                    hex_preview(&payload, 32)
                )),
                status: if payload.is_empty() {
                    "failed".to_string()
                } else {
                    "passed".to_string()
                },
                duration_ms,
            });

            if payload.is_empty() {
                return Err("RALL? 返回空 payload".to_string());
            }

            recorder.record_event(
                "oe1022d",
                &device.device_id,
                "phase_completed",
                "passed",
                Some(format!("payload_len={}", payload.len())),
            );
            Ok(())
        })();

        if let Err(message) = result {
            recorder.record_event(
                "oe1022d",
                &device.device_id,
                "phase_completed",
                "failed",
                Some(message),
            );
            return false;
        }

        true
    }

    fn run_laser_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
        recorder.record_event(
            "cni_laser",
            &device.device_id,
            "phase_started",
            "running",
            None,
        );

        let result = (|| -> Result<(), String> {
            let config = laser_config(device)?;
            let mut transport = CniLaserTransport::open(&config).map_err(|err| err.to_string())?;
            let expected = vec![0x55, 0xAA, 0x03, 0x00, 0x03];

            let started = Instant::now();
            transport.output_off().map_err(|err| err.to_string())?;
            let echo = transport
                .read_echo_exact(expected.len())
                .map_err(|err| err.to_string())?;
            let duration_ms = started.elapsed().as_millis();

            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "command_hex".to_string(),
                command_text: None,
                command_hex: Some(hex_string(&expected)),
                expects_response: true,
                observed_response: Some(hex_string(&echo)),
                status: if echo == expected {
                    "passed".to_string()
                } else {
                    "failed".to_string()
                },
                duration_ms,
            });

            if echo != expected {
                return Err(format!(
                    "Laser echo 不匹配: expected={}, observed={}",
                    hex_string(&expected),
                    hex_string(&echo)
                ));
            }

            recorder.record_event(
                "cni_laser",
                &device.device_id,
                "phase_completed",
                "passed",
                None,
            );
            Ok(())
        })();

        if let Err(message) = result {
            recorder.record_event(
                "cni_laser",
                &device.device_id,
                "phase_completed",
                "failed",
                Some(message),
            );
            return false;
        }

        true
    }
}

#[derive(Debug, Clone, Serialize)]
pub struct HardwareSmokeManifest {
    pub station_id: String,
    pub started_at: String,
    pub ended_at: String,
    pub overall_status: String,
    pub devices_total: usize,
    pub devices_passed: usize,
    pub devices_failed: usize,
    pub axes_passed: usize,
    pub axes_failed: usize,
}

#[derive(Debug, Clone, Serialize)]
pub struct HardwareSmokeEvent {
    pub ts: String,
    pub phase: String,
    pub device_id: String,
    pub action: String,
    pub status: String,
    pub detail: Option<String>,
}

#[derive(Debug, Clone, Serialize)]
pub struct CommandAuditRecord {
    pub ts: String,
    pub device_id: String,
    pub transport: String,
    pub command_kind: String,
    pub command_text: Option<String>,
    pub command_hex: Option<String>,
    pub expects_response: bool,
    pub observed_response: Option<String>,
    pub status: String,
    pub duration_ms: u128,
}

pub struct HardwareSmokeResult {
    pub manifest: HardwareSmokeManifest,
}

#[derive(Default)]
pub struct SmokeRecorder {
    events: Vec<HardwareSmokeEvent>,
    command_audit: Vec<CommandAuditRecord>,
}

impl SmokeRecorder {
    pub fn record_event(
        &mut self,
        phase: &str,
        device_id: &str,
        action: &str,
        status: &str,
        detail: Option<String>,
    ) {
        self.events.push(HardwareSmokeEvent {
            ts: now_ts_string(),
            phase: phase.to_string(),
            device_id: device_id.to_string(),
            action: action.to_string(),
            status: status.to_string(),
            detail,
        });
    }

    pub fn record_command(&mut self, record: CommandAuditRecord) {
        self.command_audit.push(record);
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum PhaseStatus {
    Passed,
    Failed,
    NotRun,
}

fn run_m8812_sequence(
    transport: &mut M8812Transport,
    device: &DeviceSpec,
    microtest_current_a: f64,
    settle_ms: u64,
    recorder: &mut SmokeRecorder,
) -> Result<(), String> {
    let idn = query_m8812(transport, device, recorder, m8812_query_idn())?;
    if !identity_matches(device, &idn) {
        return Err(format!("M8812 身份不匹配: {idn}"));
    }

    send_m8812(transport, device, recorder, "SYST:REM")?;
    send_m8812(transport, device, recorder, "CURR 0.00000")?;
    send_m8812(transport, device, recorder, "OUTP 0")?;
    let _pre_current = query_m8812(transport, device, recorder, m8812_query_meas_current_a())?;
    send_m8812(
        transport,
        device,
        recorder,
        format!("CURR {microtest_current_a:.5}").as_str(),
    )?;
    send_m8812(transport, device, recorder, "OUTP 1")?;
    thread::sleep(Duration::from_millis(settle_ms));
    let post_current = query_m8812(transport, device, recorder, m8812_query_meas_current_a())?;
    let post_current_a: f64 = post_current
        .trim()
        .parse()
        .map_err(|err| format!("MEAS:CURR? 解析失败: {err}, raw={post_current}"))?;
    if post_current_a.abs() < 0.001 {
        return Err(format!("MEAS:CURR? 非零回读不合理: {post_current_a:.6} A"));
    }

    Ok(())
}

fn run_m8812_cleanup(
    transport: &mut M8812Transport,
    device: &DeviceSpec,
    recorder: &mut SmokeRecorder,
) -> Result<(), String> {
    let mut errors = Vec::new();

    if let Err(err) = send_m8812(transport, device, recorder, "OUTP 0") {
        errors.push(format!("OUTP 0: {err}"));
    }
    if let Err(err) = send_m8812(transport, device, recorder, "CURR 0.00000") {
        errors.push(format!("CURR 0.00000: {err}"));
    }
    if let Err(err) = send_m8812(transport, device, recorder, "SYST:LOC") {
        errors.push(format!("SYST:LOC: {err}"));
    }

    if errors.is_empty() {
        recorder.record_event(
            "m8812",
            &device.device_id,
            "cleanup_completed",
            "passed",
            None,
        );
        Ok(())
    } else {
        let detail = errors.join("; ");
        recorder.record_event(
            "m8812",
            &device.device_id,
            "cleanup_completed",
            "failed",
            Some(detail.clone()),
        );
        Err(detail)
    }
}

fn query_smb(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut SmokeRecorder,
    command: &str,
) -> Result<String, String> {
    let started = Instant::now();
    let result = transport.query(command);
    let duration_ms = started.elapsed().as_millis();
    match result {
        Ok(response) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "tcp_socket".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(response.clone()),
                status: "passed".to_string(),
                duration_ms,
            });
            Ok(response)
        }
        Err(err) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "tcp_socket".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(err.to_string()),
                status: "failed".to_string(),
                duration_ms,
            });
            Err(err.to_string())
        }
    }
}

fn query_m8812(
    transport: &mut M8812Transport,
    device: &DeviceSpec,
    recorder: &mut SmokeRecorder,
    command: &str,
) -> Result<String, String> {
    let started = Instant::now();
    let result = transport.query(command);
    let duration_ms = started.elapsed().as_millis();
    match result {
        Ok(response) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(response.clone()),
                status: "passed".to_string(),
                duration_ms,
            });
            Ok(response)
        }
        Err(err) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(err.to_string()),
                status: "failed".to_string(),
                duration_ms,
            });
            Err(err.to_string())
        }
    }
}

fn send_m8812(
    transport: &mut M8812Transport,
    device: &DeviceSpec,
    recorder: &mut SmokeRecorder,
    command: &str,
) -> Result<(), String> {
    let started = Instant::now();
    let result = transport.send(command);
    let duration_ms = started.elapsed().as_millis();
    match result {
        Ok(()) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "command_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: false,
                observed_response: None,
                status: "passed".to_string(),
                duration_ms,
            });
            Ok(())
        }
        Err(err) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "command_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: false,
                observed_response: Some(err.to_string()),
                status: "failed".to_string(),
                duration_ms,
            });
            Err(err.to_string())
        }
    }
}

fn query_oe_text(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut SmokeRecorder,
    command: &str,
) -> Result<String, String> {
    let started = Instant::now();
    let result = transport.query_text(command);
    let duration_ms = started.elapsed().as_millis();
    match result {
        Ok(response) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(response.clone()),
                status: "passed".to_string(),
                duration_ms,
            });
            Ok(response)
        }
        Err(err) => {
            recorder.record_command(CommandAuditRecord {
                ts: now_ts_string(),
                device_id: device.device_id.clone(),
                transport: "serial_port".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some(command.to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some(err.to_string()),
                status: "failed".to_string(),
                duration_ms,
            });
            Err(err.to_string())
        }
    }
}

fn build_manifest(
    spec: &StationSpec,
    started_at: &str,
    station_snapshot: &StationSnapshot,
    phase_statuses: &HashMap<String, PhaseStatus>,
) -> HardwareSmokeManifest {
    let ended_at = now_ts_string();
    let mut devices_passed = 0_usize;
    let mut axes_passed = 0_usize;
    let mut axes_failed = 0_usize;

    for device in &spec.devices {
        match phase_statuses
            .get(&device.device_id)
            .copied()
            .unwrap_or(PhaseStatus::Failed)
        {
            PhaseStatus::Passed => {
                devices_passed += 1;
                if device.kind == DeviceKind::M8812 {
                    axes_passed += 1;
                }
            }
            PhaseStatus::Failed | PhaseStatus::NotRun => {
                if device.kind == DeviceKind::M8812 {
                    axes_failed += 1;
                }
            }
        }
    }

    let devices_failed = spec.devices.len().saturating_sub(devices_passed);
    let overall_status = if station_snapshot.has_required_failures() || devices_failed > 0 {
        "failed".to_string()
    } else {
        "passed".to_string()
    };

    HardwareSmokeManifest {
        station_id: spec.station_id.clone(),
        started_at: started_at.to_string(),
        ended_at,
        overall_status,
        devices_total: spec.devices.len(),
        devices_passed,
        devices_failed,
        axes_passed,
        axes_failed,
    }
}

fn write_artifacts(
    out_dir: &Path,
    manifest: &HardwareSmokeManifest,
    recorder: &SmokeRecorder,
) -> Result<(), String> {
    write_pretty_json(&out_dir.join("hardware_smoke_manifest.json"), manifest)?;
    write_jsonl(
        &out_dir.join("hardware_smoke_events.jsonl"),
        &recorder.events,
    )?;
    write_jsonl(
        &out_dir.join("hardware_smoke_command_audit.jsonl"),
        &recorder.command_audit,
    )?;
    Ok(())
}

fn write_pretty_json<T: Serialize>(path: &Path, value: &T) -> Result<(), String> {
    let text = serde_json::to_string_pretty(value)
        .map_err(|err| format!("序列化 JSON 失败 {}: {err}", path.display()))?;
    fs::write(path, text).map_err(|err| format!("写入 JSON 文件失败 {}: {err}", path.display()))
}

fn write_jsonl<T: Serialize>(path: &Path, values: &[T]) -> Result<(), String> {
    let mut out = String::new();
    for value in values {
        let line = serde_json::to_string(value)
            .map_err(|err| format!("序列化 JSONL 失败 {}: {err}", path.display()))?;
        out.push_str(&line);
        out.push('\n');
    }
    fs::write(path, out).map_err(|err| format!("写入 JSONL 文件失败 {}: {err}", path.display()))
}

fn read_station_spec(path: &Path) -> Result<StationSpec, String> {
    let text = fs::read_to_string(path)
        .map_err(|err| format!("无法读取 station 配置 {}: {err}", path.display()))?;
    serde_json::from_str(&text)
        .map_err(|err| format!("station 配置 JSON 解析失败 {}: {err}", path.display()))
}

fn resolve_output_dir(out_dir: Option<&Path>) -> PathBuf {
    out_dir.map(PathBuf::from).unwrap_or_else(|| {
        PathBuf::from("out")
            .join("hardware_smoke")
            .join(now_unix_secs_string())
    })
}

fn now_unix_secs_string() -> String {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("系统时钟不应早于 UNIX_EPOCH")
        .as_secs()
        .to_string()
}

fn now_ts_string() -> String {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("系统时钟不应早于 UNIX_EPOCH")
        .as_millis()
        .to_string()
}

fn find_first_device(spec: &StationSpec, kind: DeviceKind) -> Result<&DeviceSpec, String> {
    spec.devices
        .iter()
        .find(|device| device.kind == kind)
        .ok_or_else(|| format!("station 配置缺少必需设备: {kind:?}"))
}

fn find_optional_first_device(spec: &StationSpec, kind: DeviceKind) -> Option<&DeviceSpec> {
    spec.devices.iter().find(|device| device.kind == kind)
}

fn find_all_devices(spec: &StationSpec, kind: DeviceKind) -> Vec<&DeviceSpec> {
    spec.devices
        .iter()
        .filter(|device| device.kind == kind)
        .collect()
}

fn identity_matches(device: &DeviceSpec, observed: &str) -> bool {
    device
        .identity
        .as_ref()
        .map(|rule| rule.matches(observed))
        .unwrap_or(true)
}

fn tcp_config(device: &DeviceSpec) -> Result<Smb100aTransportConfig, String> {
    let TransportHint::TcpSocket { host, port } = &device.transport_hint else {
        return Err(format!(
            "设备 {} transport 不是 tcp_socket",
            device.device_id
        ));
    };
    Ok(Smb100aTransportConfig {
        host: host.clone(),
        port: *port,
        ..Smb100aTransportConfig::default()
    })
}

fn serial_config(device: &DeviceSpec, default_baud: u32) -> Result<M8812TransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!(
            "设备 {} transport 不是 serial_port",
            device.device_id
        ));
    };
    Ok(M8812TransportConfig {
        port_path: port_path.clone(),
        baud_rate: if *baud_rate == 0 {
            default_baud
        } else {
            *baud_rate
        },
        ..M8812TransportConfig::default()
    })
}

fn oe_config(device: &DeviceSpec) -> Result<Oe1022dTransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!(
            "设备 {} transport 不是 serial_port",
            device.device_id
        ));
    };
    Ok(Oe1022dTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..Oe1022dTransportConfig::default()
    })
}

fn laser_config(device: &DeviceSpec) -> Result<CniLaserTransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!(
            "设备 {} transport 不是 serial_port",
            device.device_id
        ));
    };
    Ok(CniLaserTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..CniLaserTransportConfig::default()
    })
}

fn hex_string(bytes: &[u8]) -> String {
    bytes
        .iter()
        .map(|b| format!("{b:02X}"))
        .collect::<Vec<_>>()
        .join(" ")
}

fn hex_preview(bytes: &[u8], max_len: usize) -> String {
    hex_string(&bytes[..bytes.len().min(max_len)])
}

#[cfg(test)]
mod tests {
    use super::*;
    use station_resolver::{IdentityRule, StationSnapshot};
    use std::collections::VecDeque;

    #[derive(Default)]
    struct FakeBackend {
        station_result: Option<StationResolveResult>,
        smb: bool,
        oe: bool,
        laser: bool,
        axis_results: VecDeque<bool>,
    }

    impl HardwareSmokeBackend for FakeBackend {
        fn verify_station(&mut self, _spec: &StationSpec) -> StationResolveResult {
            self.station_result.clone().unwrap()
        }

        fn run_smb_phase(&mut self, device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
            recorder.record_command(CommandAuditRecord {
                ts: "0".to_string(),
                device_id: device.device_id.clone(),
                transport: "fake".to_string(),
                command_kind: "query_ascii".to_string(),
                command_text: Some("*IDN?".to_string()),
                command_hex: None,
                expects_response: true,
                observed_response: Some("ok".to_string()),
                status: if self.smb { "passed" } else { "failed" }.to_string(),
                duration_ms: 0,
            });
            self.smb
        }

        fn run_m8812_axis_phase(
            &mut self,
            _device: &DeviceSpec,
            _microtest_current_a: f64,
            _settle_ms: u64,
            recorder: &mut SmokeRecorder,
        ) -> bool {
            let result = self.axis_results.pop_front().unwrap_or(false);
            recorder.record_event(
                "m8812",
                "axis",
                "axis_phase_completed",
                if result { "passed" } else { "failed" },
                None,
            );
            result
        }

        fn run_oe_phase(&mut self, _device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
            recorder.record_event(
                "oe1022d",
                "oe",
                "phase_completed",
                if self.oe { "passed" } else { "failed" },
                None,
            );
            self.oe
        }

        fn run_laser_phase(&mut self, _device: &DeviceSpec, recorder: &mut SmokeRecorder) -> bool {
            recorder.record_event(
                "cni_laser",
                "laser",
                "phase_completed",
                if self.laser { "passed" } else { "failed" },
                None,
            );
            self.laser
        }
    }

    #[test]
    fn smoke_fails_when_required_station_verify_fails() {
        let spec = sample_station_spec();
        let snapshot = StationSnapshot {
            schema_version: 1,
            station_id: "lab_a".to_string(),
            devices_total: 1,
            devices_verified: 0,
            devices_failed: 1,
            required_failures: 1,
            devices: vec![],
        };
        let mut backend = FakeBackend {
            station_result: Some(StationResolveResult {
                resolved_spec: spec.clone(),
                snapshot,
            }),
            ..FakeBackend::default()
        };
        let temp = tempdir_path("required_fail");

        let result = execute_hardware_smoke(&spec, &temp, &mut backend).unwrap();
        assert_eq!(result.manifest.overall_status, "failed");
        assert!(temp.join("station_snapshot.json").exists());
        assert!(temp.join("hardware_smoke_manifest.json").exists());
    }

    #[test]
    fn smoke_continues_other_axes_when_one_axis_fails() {
        let spec = sample_station_spec();
        let snapshot = successful_station_snapshot(&spec);
        let mut backend = FakeBackend {
            station_result: Some(StationResolveResult {
                resolved_spec: spec.clone(),
                snapshot,
            }),
            smb: true,
            oe: true,
            laser: true,
            axis_results: VecDeque::from(vec![true, false, true]),
        };
        let temp = tempdir_path("axis_continue");

        let result = execute_hardware_smoke(&spec, &temp, &mut backend).unwrap();
        assert_eq!(result.manifest.axes_passed, 2);
        assert_eq!(result.manifest.axes_failed, 1);
        assert_eq!(result.manifest.overall_status, "failed");
    }

    #[test]
    fn smoke_writes_command_audit_and_events() {
        let spec = sample_station_spec();
        let snapshot = successful_station_snapshot(&spec);
        let mut backend = FakeBackend {
            station_result: Some(StationResolveResult {
                resolved_spec: spec.clone(),
                snapshot,
            }),
            smb: true,
            oe: true,
            laser: true,
            axis_results: VecDeque::from(vec![true, true, true]),
        };
        let temp = tempdir_path("artifact_write");

        execute_hardware_smoke(&spec, &temp, &mut backend).unwrap();
        let events = fs::read_to_string(temp.join("hardware_smoke_events.jsonl")).unwrap();
        let audit = fs::read_to_string(temp.join("hardware_smoke_command_audit.jsonl")).unwrap();
        assert!(events.contains("station_verified"));
        assert!(audit.contains("*IDN?"));
    }

    fn sample_station_spec() -> StationSpec {
        StationSpec {
            station_id: "lab_a".to_string(),
            devices: vec![
                DeviceSpec {
                    device_id: "smb100a_main".to_string(),
                    kind: DeviceKind::Smb100a,
                    required: true,
                    transport_hint: TransportHint::TcpSocket {
                        host: "127.0.0.1".to_string(),
                        port: 5025,
                    },
                    identity: Some(IdentityRule {
                        exact: None,
                        contains_all: vec!["SMB100A".to_string()],
                    }),
                },
                DeviceSpec {
                    device_id: "mag_x".to_string(),
                    kind: DeviceKind::M8812,
                    required: true,
                    transport_hint: TransportHint::SerialPort {
                        port_path: "/dev/null".to_string(),
                        baud_rate: 9600,
                    },
                    identity: None,
                },
                DeviceSpec {
                    device_id: "mag_y".to_string(),
                    kind: DeviceKind::M8812,
                    required: true,
                    transport_hint: TransportHint::SerialPort {
                        port_path: "/dev/null".to_string(),
                        baud_rate: 9600,
                    },
                    identity: None,
                },
                DeviceSpec {
                    device_id: "mag_z".to_string(),
                    kind: DeviceKind::M8812,
                    required: true,
                    transport_hint: TransportHint::SerialPort {
                        port_path: "/dev/null".to_string(),
                        baud_rate: 9600,
                    },
                    identity: None,
                },
                DeviceSpec {
                    device_id: "oe1022d_main".to_string(),
                    kind: DeviceKind::Oe1022d,
                    required: true,
                    transport_hint: TransportHint::SerialPort {
                        port_path: "/dev/null".to_string(),
                        baud_rate: 921600,
                    },
                    identity: None,
                },
                DeviceSpec {
                    device_id: "cni_laser_main".to_string(),
                    kind: DeviceKind::CniLaser,
                    required: false,
                    transport_hint: TransportHint::SerialPort {
                        port_path: "/dev/null".to_string(),
                        baud_rate: 9600,
                    },
                    identity: None,
                },
            ],
        }
    }

    fn successful_station_snapshot(spec: &StationSpec) -> StationSnapshot {
        StationSnapshot {
            schema_version: 1,
            station_id: spec.station_id.clone(),
            devices_total: spec.devices.len(),
            devices_verified: spec.devices.len(),
            devices_failed: 0,
            required_failures: 0,
            devices: spec
                .devices
                .iter()
                .map(|device| station_resolver::DeviceSnapshot {
                    device_id: device.device_id.clone(),
                    kind: device.kind.clone(),
                    required: device.required,
                    transport_hint: device.transport_hint.clone(),
                    identity_observed: Some("ok".to_string()),
                    verification_method: "fake".to_string(),
                    verification_status: "verified".to_string(),
                    error_message: None,
                })
                .collect(),
        }
    }

    fn tempdir_path(name: &str) -> PathBuf {
        let path = std::env::temp_dir().join(format!(
            "odmr_hardware_smoke_test_{}_{}",
            name,
            now_unix_secs_string()
        ));
        let _ = fs::remove_dir_all(&path);
        fs::create_dir_all(&path).unwrap();
        path
    }
}
