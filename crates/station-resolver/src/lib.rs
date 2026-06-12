//! station-resolver：最小 station verify 与 snapshot 生成层。
//!
//! 第一版目标很收敛：
//! - 读取 station 规格
//! - 对串口池做 identity-first 认领
//! - 产出 station snapshot 与当次 resolved_spec
//!
//! 第一版不负责：
//! - 大范围网络扫描
//! - 运行时 preflight

use cni_laser_transport::{CniLaserTransport, CniLaserTransportConfig};
use device_transport::TransportError;
use m8812_transport::{M8812Transport, M8812TransportConfig};
use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::{Deserialize, Serialize};
use smb100a_transport::{Smb100aTransport, Smb100aTransportConfig};
use std::collections::HashSet;
use std::fmt;

#[derive(Debug)]
pub enum StationResolveError {
    Transport {
        device_id: String,
        source: TransportError,
    },
    IdentityMismatch {
        device_id: String,
        expected: IdentityRule,
        observed: String,
    },
    LaserEchoMismatch {
        device_id: String,
        expected_hex: String,
        observed_hex: String,
    },
}

impl fmt::Display for StationResolveError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Transport { device_id, source } => {
                write!(f, "设备 {device_id} 连接失败: {source}")
            }
            Self::IdentityMismatch {
                device_id,
                expected,
                observed,
            } => write!(
                f,
                "设备 {device_id} 身份不匹配，expected={expected:?}, observed={observed}"
            ),
            Self::LaserEchoMismatch {
                device_id,
                expected_hex,
                observed_hex,
            } => write!(
                f,
                "设备 {device_id} 激光 echo 不匹配，expected={expected_hex}, observed={observed_hex}"
            ),
        }
    }
}

impl std::error::Error for StationResolveError {}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "snake_case")]
pub enum DeviceKind {
    Smb100a,
    Oe1022d,
    M8812,
    CniLaser,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(tag = "transport", rename_all = "snake_case")]
pub enum TransportHint {
    TcpSocket {
        host: String,
        port: u16,
        #[serde(default)]
        host_candidates: Vec<String>,
    },
    SerialPort { port_path: String, baud_rate: u32 },
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct IdentityRule {
    #[serde(default)]
    pub exact: Option<String>,
    #[serde(default)]
    pub exact_any: Vec<String>,
    #[serde(default)]
    pub contains_all: Vec<String>,
    #[serde(default)]
    pub contains_any: Vec<String>,
}

impl IdentityRule {
    pub fn matches(&self, observed: &str) -> bool {
        let observed = observed.trim();
        let exact_ok = self
            .exact
            .as_ref()
            .map(|exact| observed == exact.trim())
            .unwrap_or(true);
        let exact_any_ok = self.exact_any.is_empty()
            || self
                .exact_any
                .iter()
                .any(|exact| observed == exact.trim());
        let contains_all_ok = self
            .contains_all
            .iter()
            .all(|token| observed.contains(token));
        let contains_any_ok = self.contains_any.is_empty()
            || self
                .contains_any
                .iter()
                .any(|token| observed.contains(token));
        exact_ok && exact_any_ok && contains_all_ok && contains_any_ok
    }
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct DeviceSpec {
    pub device_id: String,
    pub kind: DeviceKind,
    pub required: bool,
    pub transport_hint: TransportHint,
    #[serde(default)]
    pub identity: Option<IdentityRule>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct StationSpec {
    pub station_id: String,
    pub devices: Vec<DeviceSpec>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct DeviceSnapshot {
    pub device_id: String,
    pub kind: DeviceKind,
    pub required: bool,
    pub transport_hint: TransportHint,
    #[serde(default)]
    pub resolved_transport: Option<TransportHint>,
    pub identity_observed: Option<String>,
    pub verification_method: String,
    pub verification_status: String,
    pub error_message: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
pub struct StationSnapshot {
    pub schema_version: u32,
    pub station_id: String,
    pub devices_total: usize,
    pub devices_verified: usize,
    pub devices_failed: usize,
    pub required_failures: usize,
    pub devices: Vec<DeviceSnapshot>,
}

impl StationSnapshot {
    pub fn has_required_failures(&self) -> bool {
        self.required_failures > 0
    }
}

#[derive(Debug, Clone)]
pub struct StationResolveResult {
    pub resolved_spec: StationSpec,
    pub snapshot: StationSnapshot,
}

pub fn verify_station(spec: &StationSpec) -> StationSnapshot {
    resolve_station(spec).snapshot
}

pub fn resolve_station(spec: &StationSpec) -> StationResolveResult {
    let mut resolved_spec = spec.clone();
    let mut devices = Vec::with_capacity(resolved_spec.devices.len());
    let mut devices_verified = 0_usize;
    let mut devices_failed = 0_usize;
    let mut required_failures = 0_usize;
    let mut claimed_serial_ports: HashSet<String> = HashSet::new();
    let available_serial_ports = list_available_serial_ports();

    for device in &mut resolved_spec.devices {
        let snapshot = match device.kind {
            DeviceKind::Smb100a => verify_smb100a(device),
            DeviceKind::Oe1022d | DeviceKind::M8812 | DeviceKind::CniLaser => {
                verify_serial_with_scan(device, &available_serial_ports, &mut claimed_serial_ports)
            }
        };

        if snapshot.verification_status == "verified" {
            devices_verified += 1;
            if let TransportHint::SerialPort { port_path, .. } = &device.transport_hint {
                claimed_serial_ports.insert(port_path.clone());
            }
        } else {
            devices_failed += 1;
            if snapshot.required {
                required_failures += 1;
            }
        }
        devices.push(snapshot);
    }

    StationResolveResult {
        resolved_spec,
        snapshot: StationSnapshot {
            schema_version: 2,
            station_id: spec.station_id.clone(),
            devices_total: devices.len(),
            devices_verified,
            devices_failed,
            required_failures,
            devices,
        },
    }
}

fn verify_smb100a(device: &DeviceSpec) -> DeviceSnapshot {
    let TransportHint::TcpSocket {
        host,
        port,
        host_candidates,
    } = &device.transport_hint
    else {
        unreachable!("SMB100A transport hint 必须是 tcp_socket");
    };
    let mut attempted_hosts = Vec::new();
    for candidate_host in tcp_candidate_hosts(host, host_candidates) {
        attempted_hosts.push(candidate_host.clone());
        let config = Smb100aTransportConfig {
            host: candidate_host.clone(),
            port: *port,
            ..Smb100aTransportConfig::default()
        };
        let transport = Smb100aTransport::connect(&config).map_err(|source| {
            StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            }
        });
        let Ok(mut transport) = transport else {
            continue;
        };
        let observed = transport
            .query_idn()
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            });
        let Ok(observed) = observed else {
            continue;
        };
        if verify_identity_rule(device, &observed).is_err() {
            continue;
        }

        let mut resolved_device = device.clone();
        if let TransportHint::TcpSocket { host, .. } = &mut resolved_device.transport_hint {
            *host = candidate_host;
        }
        return build_verified_snapshot(&resolved_device, observed, "tcp_idn_query");
    }

    build_failed_snapshot(
        device,
        "tcp_idn_query",
        format!(
            "TCP host 候选全部失败: {}",
            attempted_hosts.join(", ")
        ),
    )
}

fn verify_oe1022d(device: &DeviceSpec) -> DeviceSnapshot {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        unreachable!("OE1022D transport hint 必须是 serial_port");
    };
    let config = Oe1022dTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..Oe1022dTransportConfig::default()
    };
    let result = (|| {
        let mut transport =
            Oe1022dTransport::open(&config).map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        transport
            .clear_input()
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        let observed = transport
            .query_idn()
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        let observed = sanitize_ascii_observed(&observed);
        verify_identity_rule(device, &observed)?;
        Ok::<String, StationResolveError>(observed)
    })();

    match result {
        Ok(observed) => build_verified_snapshot(device, observed, "serial_idn_query"),
        Err(err) => build_failed_snapshot(device, "serial_idn_query", err.to_string()),
    }
}

fn verify_m8812(device: &DeviceSpec) -> DeviceSnapshot {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        unreachable!("M8812 transport hint 必须是 serial_port");
    };
    let config = M8812TransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..M8812TransportConfig::default()
    };
    let result = (|| {
        let mut transport =
            M8812Transport::open(&config).map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        let observed = transport
            .query_idn()
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        verify_identity_rule(device, &observed)?;
        Ok::<String, StationResolveError>(observed)
    })();

    match result {
        Ok(observed) => build_verified_snapshot(device, observed, "serial_idn_query"),
        Err(err) => build_failed_snapshot(device, "serial_idn_query", err.to_string()),
    }
}

fn verify_cni_laser(device: &DeviceSpec) -> DeviceSnapshot {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        unreachable!("CNI Laser transport hint 必须是 serial_port");
    };
    let config = CniLaserTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        ..CniLaserTransportConfig::default()
    };
    let result = (|| {
        let mut transport =
            CniLaserTransport::open(&config).map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;

        let expected = vec![0x55, 0xAA, 0x03, 0x00, 0x03];
        transport
            .output_off()
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        let observed = transport
            .read_echo_exact(expected.len())
            .map_err(|source| StationResolveError::Transport {
                device_id: device.device_id.clone(),
                source,
            })?;
        if observed != expected {
            return Err(StationResolveError::LaserEchoMismatch {
                device_id: device.device_id.clone(),
                expected_hex: hex_string(&expected),
                observed_hex: hex_string(&observed),
            });
        }
        Ok::<String, StationResolveError>(hex_string(&observed))
    })();

    match result {
        Ok(observed) => build_verified_snapshot(device, observed, "laser_off_echo"),
        Err(err) => build_failed_snapshot(device, "laser_off_echo", err.to_string()),
    }
}

fn verify_serial_with_scan(
    device: &mut DeviceSpec,
    available_serial_ports: &[String],
    claimed_serial_ports: &mut HashSet<String>,
) -> DeviceSnapshot {
    resolve_serial_device_with(
        device,
        available_serial_ports,
        claimed_serial_ports,
        verify_device_without_scan,
    )
}

fn verify_device_without_scan(device: &DeviceSpec) -> DeviceSnapshot {
    match device.kind {
        DeviceKind::Smb100a => verify_smb100a(device),
        DeviceKind::Oe1022d => verify_oe1022d(device),
        DeviceKind::M8812 => verify_m8812(device),
        DeviceKind::CniLaser => verify_cni_laser(device),
    }
}

fn list_available_serial_ports() -> Vec<String> {
    let Ok(ports) = serialport::available_ports() else {
        return Vec::new();
    };

    let mut out = ports
        .into_iter()
        .map(|port| port.port_name)
        .filter(|port| port.starts_with("/dev/cu.") || port.starts_with("/dev/tty."))
        .collect::<Vec<_>>();
    out.sort_by(|left, right| {
        serial_port_priority(left)
            .cmp(&serial_port_priority(right))
            .then_with(|| left.cmp(right))
    });
    out
}

fn resolve_serial_device_with<F>(
    device: &mut DeviceSpec,
    available_serial_ports: &[String],
    claimed_serial_ports: &mut HashSet<String>,
    verify_fn: F,
) -> DeviceSnapshot
where
    F: Fn(&DeviceSpec) -> DeviceSnapshot,
{
    let configured_port = serial_port_path(device).unwrap_or_default();
    let candidate_ports = order_candidate_serial_ports(available_serial_ports, &configured_port);
    let mut scan_failures = Vec::new();

    for candidate_port in candidate_ports {
        if claimed_serial_ports.contains(&candidate_port) {
            scan_failures.push(format!("{candidate_port}: already claimed"));
            continue;
        }

        let mut candidate_device = device.clone();
        set_serial_port_path(&mut candidate_device, candidate_port.clone());
        let candidate_snapshot = verify_fn(&candidate_device);
        if candidate_snapshot.verification_status == "verified" {
            *device = candidate_device;
            return candidate_snapshot;
        }

        if let Some(message) = candidate_snapshot.error_message {
            scan_failures.push(format!("{candidate_port}: {message}"));
        }
    }

    build_failed_snapshot(
        device,
        "serial_identity_scan",
        enrich_scan_failure_message(&configured_port, &scan_failures),
    )
}

fn order_candidate_serial_ports(
    available_serial_ports: &[String],
    configured_port: &str,
) -> Vec<String> {
    let mut ordered = available_serial_ports.to_vec();
    if configured_port.is_empty() {
        return ordered;
    }

    if let Some(position) = ordered.iter().position(|port| port == configured_port) {
        let hint_port = ordered.remove(position);
        ordered.insert(0, hint_port);
    }
    ordered
}

fn tcp_candidate_hosts(host: &str, host_candidates: &[String]) -> Vec<String> {
    let mut ordered = Vec::with_capacity(1 + host_candidates.len());
    ordered.push(host.to_string());
    for candidate in host_candidates {
        if !ordered.iter().any(|existing| existing == candidate) {
            ordered.push(candidate.clone());
        }
    }
    ordered
}

fn serial_port_priority(port: &str) -> u8 {
    if port.starts_with("/dev/cu.") {
        0
    } else {
        1
    }
}

fn serial_port_path(device: &DeviceSpec) -> Option<String> {
    match &device.transport_hint {
        TransportHint::SerialPort { port_path, .. } => Some(port_path.clone()),
        TransportHint::TcpSocket { .. } => None,
    }
}

fn set_serial_port_path(device: &mut DeviceSpec, port_path: String) {
    match &mut device.transport_hint {
        TransportHint::SerialPort {
            port_path: current, ..
        } => {
            *current = port_path;
        }
        TransportHint::TcpSocket { .. } => unreachable!("只能给 serial_port 设备改端口"),
    }
}

fn enrich_scan_failure_message(configured_port: &str, scan_failures: &[String]) -> String {
    let hint = if configured_port.is_empty() {
        "无 hint".to_string()
    } else {
        format!("hint={configured_port}")
    };
    let scan_summary = if scan_failures.is_empty() {
        "未找到可用串口候选".to_string()
    } else {
        format!("候选探测失败: {}", scan_failures.join(" | "))
    };
    format!("identity-first 串口认领失败: {hint}; {scan_summary}")
}

fn sanitize_ascii_observed(observed: &str) -> String {
    observed.trim_matches('\0').trim().to_string()
}

fn verify_identity_rule(
    device: &DeviceSpec,
    observed: &str,
) -> std::result::Result<(), StationResolveError> {
    if let Some(rule) = &device.identity {
        if !rule.matches(observed) {
            return Err(StationResolveError::IdentityMismatch {
                device_id: device.device_id.clone(),
                expected: rule.clone(),
                observed: observed.to_string(),
            });
        }
    }
    Ok(())
}

fn build_verified_snapshot(
    device: &DeviceSpec,
    observed: String,
    verification_method: &str,
) -> DeviceSnapshot {
    DeviceSnapshot {
        device_id: device.device_id.clone(),
        kind: device.kind.clone(),
        required: device.required,
        transport_hint: device.transport_hint.clone(),
        resolved_transport: Some(device.transport_hint.clone()),
        identity_observed: Some(observed),
        verification_method: verification_method.to_string(),
        verification_status: "verified".to_string(),
        error_message: None,
    }
}

fn build_failed_snapshot(
    device: &DeviceSpec,
    verification_method: &str,
    error_message: String,
) -> DeviceSnapshot {
    DeviceSnapshot {
        device_id: device.device_id.clone(),
        kind: device.kind.clone(),
        required: device.required,
        transport_hint: device.transport_hint.clone(),
        resolved_transport: None,
        identity_observed: None,
        verification_method: verification_method.to_string(),
        verification_status: "failed".to_string(),
        error_message: Some(error_message),
    }
}

fn hex_string(bytes: &[u8]) -> String {
    bytes
        .iter()
        .map(|b| format!("{b:02X}"))
        .collect::<Vec<_>>()
        .join(" ")
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn identity_rule_exact_match_works() {
        let rule = IdentityRule {
            exact: Some("MAYNUO,M8812,SN,V2.7".to_string()),
            exact_any: Vec::new(),
            contains_all: Vec::new(),
            contains_any: Vec::new(),
        };
        assert!(rule.matches("MAYNUO,M8812,SN,V2.7"));
        assert!(!rule.matches("MAYNUO,M8812,OTHER,V2.7"));
    }

    #[test]
    fn identity_rule_contains_all_works() {
        let rule = IdentityRule {
            exact: None,
            exact_any: Vec::new(),
            contains_all: vec!["Rohde&Schwarz".to_string(), "SMB100A".to_string()],
            contains_any: Vec::new(),
        };
        assert!(rule.matches("Rohde&Schwarz,SMB100A,123"));
        assert!(!rule.matches("Rohde&Schwarz,XYZ"));
    }

    #[test]
    fn identity_rule_contains_any_works() {
        let rule = IdentityRule {
            exact: None,
            exact_any: Vec::new(),
            contains_all: vec!["SSI LIA-OE1022D".to_string()],
            contains_any: vec!["SN:D6522078".to_string(), "SN:D6130220".to_string()],
        };
        assert!(rule.matches("SSI LIA-OE1022D,SN:D6522078,Version:Ver6.3"));
        assert!(rule.matches("SSI LIA-OE1022D,SN:D6130220,Version:Ver6.3"));
        assert!(!rule.matches("SSI LIA-OE1022D,SN:D0000000,Version:Ver6.3"));
    }

    #[test]
    fn station_spec_roundtrip_json_works() {
        let spec = StationSpec {
            station_id: "lab_a".to_string(),
            devices: vec![DeviceSpec {
                device_id: "smb100a_main".to_string(),
                kind: DeviceKind::Smb100a,
                required: true,
                transport_hint: TransportHint::TcpSocket {
                    host: "169.254.2.20".to_string(),
                    port: 5025,
                    host_candidates: vec!["169.254.1.20".to_string()],
                },
                identity: Some(IdentityRule {
                    exact: None,
                    exact_any: Vec::new(),
                    contains_all: vec!["SMB100A".to_string()],
                    contains_any: Vec::new(),
                }),
            }],
        };

        let json = serde_json::to_string_pretty(&spec).unwrap();
        let restored: StationSpec = serde_json::from_str(&json).unwrap();
        assert_eq!(restored, spec);
    }

    #[test]
    fn failed_snapshot_reports_required_flag() {
        let spec = DeviceSpec {
            device_id: "laser".to_string(),
            kind: DeviceKind::CniLaser,
            required: false,
            transport_hint: TransportHint::SerialPort {
                port_path: "/dev/null".to_string(),
                baud_rate: 9600,
            },
            identity: None,
        };
        let snapshot = build_failed_snapshot(&spec, "laser_off_echo", "boom".to_string());
        assert_eq!(snapshot.verification_status, "failed");
        assert!(!snapshot.required);
        assert_eq!(snapshot.error_message.as_deref(), Some("boom"));
    }

    #[test]
    fn candidate_order_uses_hint_only_for_sorting() {
        let ports = vec![
            "/dev/cu.usbserial-b".to_string(),
            "/dev/cu.usbserial-a".to_string(),
        ];
        let ordered = order_candidate_serial_ports(&ports, "/dev/cu.usbserial-a");
        assert_eq!(
            ordered,
            vec![
                "/dev/cu.usbserial-a".to_string(),
                "/dev/cu.usbserial-b".to_string()
            ]
        );
    }

    #[test]
    fn tcp_candidate_hosts_keeps_primary_and_deduplicates() {
        let ordered = tcp_candidate_hosts(
            "169.254.2.20",
            &[
                "169.254.1.20".to_string(),
                "169.254.2.20".to_string(),
                "169.254.3.20".to_string(),
            ],
        );
        assert_eq!(
            ordered,
            vec![
                "169.254.2.20".to_string(),
                "169.254.1.20".to_string(),
                "169.254.3.20".to_string(),
            ]
        );
    }

    #[test]
    fn identity_scan_can_rebind_when_hint_is_wrong() {
        let mut device = DeviceSpec {
            device_id: "mag_x".to_string(),
            kind: DeviceKind::M8812,
            required: true,
            transport_hint: TransportHint::SerialPort {
                port_path: "/dev/cu.wrong".to_string(),
                baud_rate: 9600,
            },
            identity: None,
        };
        let ports = vec!["/dev/cu.wrong".to_string(), "/dev/cu.correct".to_string()];
        let mut claimed = HashSet::new();

        let snapshot = resolve_serial_device_with(&mut device, &ports, &mut claimed, |candidate| {
            match serial_port_path(candidate).as_deref() {
                Some("/dev/cu.correct") => build_verified_snapshot(
                    candidate,
                    "MAYNUO,M8812,080020960220402020,V2.7".to_string(),
                    "serial_idn_query",
                ),
                _ => build_failed_snapshot(
                    candidate,
                    "serial_idn_query",
                    "mock mismatch".to_string(),
                ),
            }
        });

        assert_eq!(snapshot.verification_status, "verified");
        assert_eq!(
            serial_port_path(&device).as_deref(),
            Some("/dev/cu.correct")
        );
        assert_eq!(
            snapshot.resolved_transport,
            Some(TransportHint::SerialPort {
                port_path: "/dev/cu.correct".to_string(),
                baud_rate: 9600,
            })
        );
    }

    #[test]
    fn claimed_port_is_not_reused_for_second_device() {
        let ports = vec!["/dev/cu.shared".to_string(), "/dev/cu.other".to_string()];
        let mut claimed = vec!["/dev/cu.shared".to_string()]
            .into_iter()
            .collect::<HashSet<_>>();
        let mut device = DeviceSpec {
            device_id: "mag_y".to_string(),
            kind: DeviceKind::M8812,
            required: true,
            transport_hint: TransportHint::SerialPort {
                port_path: "/dev/cu.shared".to_string(),
                baud_rate: 9600,
            },
            identity: None,
        };

        let snapshot = resolve_serial_device_with(&mut device, &ports, &mut claimed, |candidate| {
            match serial_port_path(candidate).as_deref() {
                Some("/dev/cu.other") => build_verified_snapshot(
                    candidate,
                    "MAYNUO,M8812,080020960220402022,V2.7".to_string(),
                    "serial_idn_query",
                ),
                _ => build_failed_snapshot(
                    candidate,
                    "serial_idn_query",
                    "mock mismatch".to_string(),
                ),
            }
        });

        assert_eq!(serial_port_path(&device).as_deref(), Some("/dev/cu.other"));
        assert_eq!(snapshot.verification_status, "verified");
    }
}
