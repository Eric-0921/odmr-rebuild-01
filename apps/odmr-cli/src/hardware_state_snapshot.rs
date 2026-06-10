//! PLL 相关状态只读快照入口。
//!
//! 目标：
//! - 不改设备配置
//! - 慢速读取 SMB100A / OE1022D 的当前状态
//! - 把实际 readback 落盘，便于和 Windows 原厂软件配置后的状态做对比

use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::Serialize;
use smb100a_transport::{Smb100aTransport, Smb100aTransportConfig};
use station_resolver::{resolve_station, DeviceKind, DeviceSpec, StationSpec, TransportHint};
use std::fs;
use std::path::{Path, PathBuf};
use std::thread;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const SMB_QUERY_SPACING_MS: u64 = 300;
const OE_QUERY_SPACING_MS: u64 = 500;
const OE_PLL_QUERY_PRIMARY: &str = "*PLLD ? 2";
const OE_PLL_QUERY_FALLBACK: &str = "*PLLD? 2";

pub fn run_hardware_state_snapshot(
    station_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec = crate::read_station_spec(station_path)?;
    let target_dir = resolve_output_dir(out_dir);
    fs::create_dir_all(&target_dir)
        .map_err(|err| format!("无法创建状态快照输出目录 {}: {err}", target_dir.display()))?;

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

    let smb_device = find_first_device(&resolved.resolved_spec, DeviceKind::Smb100a)?;
    let oe_device = find_first_device(&resolved.resolved_spec, DeviceKind::Oe1022d)?;
    let mut recorder = SnapshotRecorder::default();
    let started_at = now_ts_string();

    let smb100a_state = snapshot_smb100a_state(smb_device, &mut recorder)?;
    let oe1022d_state = snapshot_oe1022d_state(oe_device, &mut recorder)?;
    let derived_findings = build_derived_findings(&smb100a_state, &oe1022d_state);

    let snapshot = HardwareStateSnapshot {
        station_id: spec.station_id,
        started_at,
        ended_at: now_ts_string(),
        smb100a_query_spacing_ms: SMB_QUERY_SPACING_MS,
        oe1022d_query_spacing_ms: OE_QUERY_SPACING_MS,
        smb100a_state,
        oe1022d_state,
        derived_findings,
    };

    write_pretty_json(&target_dir.join("hardware_state_snapshot.json"), &snapshot)?;
    write_jsonl(
        &target_dir.join("hardware_state_snapshot_command_audit.jsonl"),
        &recorder.command_audit,
    )?;

    println!("hardware state snapshot 成功: {}", snapshot.station_id);
    println!("产物目录: {}", target_dir.display());
    Ok(target_dir)
}

fn snapshot_smb100a_state(
    device: &DeviceSpec,
    recorder: &mut SnapshotRecorder,
) -> Result<Vec<QueryStateRecord>, String> {
    let mut transport =
        Smb100aTransport::connect(&tcp_config(device)?).map_err(|err| err.to_string())?;
    let mut states = Vec::new();

    for command in [
        "*IDN?",
        "OUTP?",
        "FREQ?",
        "POW?",
        "MOD:STAT?",
        "FM:STAT?",
        "FM:SOUR?",
        "FM:MODE?",
        "FM:DEV?",
        "LFO?",
        "LFO:VOLT?",
        "LFO:FREQ?",
        "LFO:SHAP?",
        "SOUR:LFO:SIMP?",
        "SYST:ERR?",
    ] {
        let observed = query_smb_text(&mut transport, device, recorder, command)?;
        states.push(QueryStateRecord {
            command: command.to_string(),
            observed: observed.trim().to_string(),
        });
        sleep_spacing(SMB_QUERY_SPACING_MS);
    }

    Ok(states)
}

fn snapshot_oe1022d_state(
    device: &DeviceSpec,
    recorder: &mut SnapshotRecorder,
) -> Result<Vec<QueryStateRecord>, String> {
    let mut config = oe_config(device)?;
    config.timeout = Duration::from_millis(1500);
    let mut transport = Oe1022dTransport::open(&config).map_err(|err| err.to_string())?;
    let mut states = Vec::new();

    for command in [
        "*IDN?",
        "FMODD? 2",
        "RSLPD? 2",
        "PHASD? 2",
        "ISRCD? 2",
        "IGNDD? 2",
        "ICPLD? 2",
        "ILIND? 2",
        "HARMD? 2,1",
        "HARMD? 2,2",
        "RMODD? 2",
        "SENSD? 2",
        "OFLTD? 2",
        "OFSLD? 2",
        "SWVTD? 2",
        "SLVLD? 2",
        "FREQD? 2",
        "INOVD ? 2",
        "GNOVD ? 2",
    ] {
        let observed = query_oe_text(&mut transport, device, recorder, command)?;
        states.push(QueryStateRecord {
            command: command.to_string(),
            observed: normalize_oe_response(&observed),
        });
        sleep_spacing(OE_QUERY_SPACING_MS);
    }

    let pll_observed = query_oe_text_with_fallback(
        &mut transport,
        device,
        recorder,
        OE_PLL_QUERY_PRIMARY,
        OE_PLL_QUERY_FALLBACK,
    )?;
    states.push(QueryStateRecord {
        command: "*PLLD? 2".to_string(),
        observed: normalize_oe_response(&pll_observed),
    });

    Ok(states)
}

fn build_derived_findings(
    smb100a_state: &[QueryStateRecord],
    oe1022d_state: &[QueryStateRecord],
) -> Vec<String> {
    let mut findings = Vec::new();

    if let Some(value) = query_value(smb100a_state, "LFO:VOLT?") {
        if let Ok(vpeak) = value.parse::<f64>() {
            findings.push(format!(
                "SMB100A LF 输出幅值 readback: {vpeak:.6} Vpeak / {:.6} Vpp",
                vpeak * 2.0
            ));
        }
    }

    if let Some(value) = query_value(smb100a_state, "LFO:FREQ?") {
        findings.push(format!("SMB100A LF 输出频率 readback: {value} Hz"));
    }

    if let Some(value) = query_value(oe1022d_state, "FREQD? 2") {
        findings.push(format!("OE1022D Ch-B 参考频率 readback: {value} Hz"));
    }

    if let Some(value) = query_value(oe1022d_state, "*PLLD? 2") {
        findings.push(format!("OE1022D Ch-B PLL 状态 readback: {value}"));
    }

    findings.push(format!(
        "本次快照使用慢速查询节奏：SMB 间隔 {} ms，OE 间隔 {} ms",
        SMB_QUERY_SPACING_MS, OE_QUERY_SPACING_MS
    ));
    findings
}

fn query_value<'a>(states: &'a [QueryStateRecord], command: &str) -> Option<&'a str> {
    states
        .iter()
        .find(|record| record.command == command)
        .map(|record| record.observed.as_str())
}

fn sleep_spacing(ms: u64) {
    thread::sleep(Duration::from_millis(ms));
}

fn query_smb_text(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut SnapshotRecorder,
    query: &str,
) -> Result<String, String> {
    let started = Instant::now();
    let observed = transport.query(query).map_err(|err| err.to_string());
    recorder.command_audit.push(CommandAuditRecord {
        ts: now_ts_string(),
        device_id: device.device_id.clone(),
        transport: "tcp_socket".to_string(),
        command_kind: "query_ascii".to_string(),
        command_text: query.to_string(),
        observed_response: observed.clone().ok(),
        status: if observed.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        duration_ms: started.elapsed().as_millis(),
    });
    observed
}

fn query_oe_text(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut SnapshotRecorder,
    query: &str,
) -> Result<String, String> {
    transport.clear_input().map_err(|err| err.to_string())?;
    let started = Instant::now();
    let observed = transport.query_text(query).map_err(|err| err.to_string());
    recorder.command_audit.push(CommandAuditRecord {
        ts: now_ts_string(),
        device_id: device.device_id.clone(),
        transport: "serial_port".to_string(),
        command_kind: "query_ascii".to_string(),
        command_text: query.to_string(),
        observed_response: observed
            .clone()
            .ok()
            .map(|value| normalize_oe_response(&value)),
        status: if observed.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        duration_ms: started.elapsed().as_millis(),
    });
    observed
}

fn query_oe_text_with_fallback(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut SnapshotRecorder,
    primary: &str,
    fallback: &str,
) -> Result<String, String> {
    match query_oe_text(transport, device, recorder, primary) {
        Ok(value) => Ok(value),
        Err(_) => query_oe_text(transport, device, recorder, fallback),
    }
}

fn normalize_oe_response(text: &str) -> String {
    text.chars()
        .filter(|ch| *ch != '\0')
        .collect::<String>()
        .trim()
        .to_string()
}

fn resolve_output_dir(out_dir: Option<&Path>) -> PathBuf {
    if let Some(path) = out_dir {
        return path.to_path_buf();
    }
    let stamp = now_ts_string();
    PathBuf::from(format!("out/hardware_state_snapshot/{stamp}"))
}

fn find_first_device(spec: &StationSpec, kind: DeviceKind) -> Result<&DeviceSpec, String> {
    spec.devices
        .iter()
        .find(|device| device.kind == kind)
        .ok_or_else(|| format!("station 配置中缺少设备种类: {:?}", kind))
}

fn tcp_config(device: &DeviceSpec) -> Result<Smb100aTransportConfig, String> {
    match &device.transport_hint {
        TransportHint::TcpSocket { host, port } => Ok(Smb100aTransportConfig {
            host: host.clone(),
            port: *port,
            read_timeout: Duration::from_millis(1000),
            write_timeout: Duration::from_millis(1000),
            ..Smb100aTransportConfig::default()
        }),
        other => Err(format!(
            "设备 {} 缺少 TCP transport hint: {other:?}",
            device.device_id
        )),
    }
}

fn oe_config(device: &DeviceSpec) -> Result<Oe1022dTransportConfig, String> {
    match &device.transport_hint {
        TransportHint::SerialPort {
            port_path,
            baud_rate,
        } => Ok(Oe1022dTransportConfig {
            port_path: port_path.clone(),
            baud_rate: *baud_rate,
            ..Oe1022dTransportConfig::default()
        }),
        other => Err(format!(
            "设备 {} 缺少串口 transport hint: {other:?}",
            device.device_id
        )),
    }
}

fn write_pretty_json<T: Serialize>(path: &Path, value: &T) -> Result<(), String> {
    let text = serde_json::to_string_pretty(value)
        .map_err(|err| format!("JSON 序列化失败 {}: {err}", path.display()))?;
    fs::write(path, text).map_err(|err| format!("无法写入 {}: {err}", path.display()))
}

fn write_jsonl<T: Serialize>(path: &Path, values: &[T]) -> Result<(), String> {
    let mut out = String::new();
    for value in values {
        let line = serde_json::to_string(value)
            .map_err(|err| format!("JSONL 序列化失败 {}: {err}", path.display()))?;
        out.push_str(&line);
        out.push('\n');
    }
    fs::write(path, out).map_err(|err| format!("无法写入 {}: {err}", path.display()))
}

fn now_ts_string() -> String {
    let millis = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("系统时间必须晚于 UNIX_EPOCH")
        .as_millis();
    millis.to_string()
}

#[derive(Debug, Clone, Serialize)]
struct HardwareStateSnapshot {
    station_id: String,
    started_at: String,
    ended_at: String,
    smb100a_query_spacing_ms: u64,
    oe1022d_query_spacing_ms: u64,
    smb100a_state: Vec<QueryStateRecord>,
    oe1022d_state: Vec<QueryStateRecord>,
    derived_findings: Vec<String>,
}

#[derive(Debug, Clone, Serialize)]
struct QueryStateRecord {
    command: String,
    observed: String,
}

#[derive(Debug, Clone, Serialize)]
struct CommandAuditRecord {
    ts: String,
    device_id: String,
    transport: String,
    command_kind: String,
    command_text: String,
    observed_response: Option<String>,
    status: String,
    duration_ms: u128,
}

#[derive(Default)]
struct SnapshotRecorder {
    command_audit: Vec<CommandAuditRecord>,
}
