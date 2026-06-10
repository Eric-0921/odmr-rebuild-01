//! 手册命令真机验收入口。
//!
//! 目标不是做完整 runtime，而是把“手册里的固定配置命令”真打到设备上，
//! 并把 readback / error queue / PLL 锁定结果记录成可审计产物。

use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::Serialize;
use smb100a_transport::{Smb100aTransport, Smb100aTransportConfig};
use station_resolver::{resolve_station, DeviceKind, DeviceSpec, StationSpec, TransportHint};
use std::fs;
use std::path::{Path, PathBuf};
use std::thread;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const OE_PLL_QUERY_PRIMARY: &str = "*PLLD ? 2";
const OE_PLL_QUERY_FALLBACK: &str = "*PLLD? 2";
const OE_OVERLOAD_QUERY_PRIMARY: &str = "INOVD ? 2";
const OE_GAIN_OVERLOAD_QUERY_PRIMARY: &str = "GNOVD ? 2";
const SMB_COMMAND_SETTLE_MS: u64 = 300;
const OE_COMMAND_SETTLE_MS: u64 = 400;
const PLL_SETTLE_MS: u64 = 1200;
const SMB_EXPECTED_LF_FREQ_HZ: f64 = 500.0;
const SMB_EXPECTED_LF_VPEAK_V: f64 = 0.137;
const SMB_EXPECTED_LF_VPP_V: f64 = SMB_EXPECTED_LF_VPEAK_V * 2.0;
const OE_EXPECTED_REF_FREQ_TOLERANCE_HZ: f64 = 5.0;
const OE_SINE_REF_MIN_VPP_V: f64 = 0.4;
const OE_TTL_HIGH_MIN_V: f64 = 3.0;
const OE_TTL_LOW_MAX_V: f64 = 0.5;

pub fn run_hardware_profile_verify(
    station_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec = crate::read_station_spec(station_path)?;
    let target_dir = resolve_output_dir(out_dir);
    fs::create_dir_all(&target_dir).map_err(|err| {
        format!(
            "无法创建 profile verify 输出目录 {}: {err}",
            target_dir.display()
        )
    })?;

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

    let mut recorder = VerifyRecorder::default();
    let started_at = now_ts_string();
    let smb_device = find_first_device(&resolved.resolved_spec, DeviceKind::Smb100a)?;
    let oe_device = find_first_device(&resolved.resolved_spec, DeviceKind::Oe1022d)?;

    let smb_result = verify_smb100a_fixed_profile(smb_device, &mut recorder);
    let oe_result = verify_oe1022d_ch_b_profile(oe_device, &mut recorder);
    let _ = cleanup_smb100a_profile(smb_device, &mut recorder);

    let summary = HardwareProfileVerifySummary {
        station_id: spec.station_id.clone(),
        started_at,
        ended_at: now_ts_string(),
        smb100a_status: smb_result.status.clone(),
        oe1022d_status: oe_result.status.clone(),
        overall_status: if smb_result.status == "passed" && oe_result.status == "passed" {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        smb100a_checks: smb_result.checks,
        oe1022d_checks: oe_result.checks,
    };

    write_pretty_json(
        &target_dir.join("hardware_profile_verify_summary.json"),
        &summary,
    )?;
    write_jsonl(
        &target_dir.join("hardware_profile_verify_command_audit.jsonl"),
        &recorder.command_audit,
    )?;

    if summary.overall_status == "passed" {
        println!("hardware profile verify 成功: {}", summary.station_id);
        println!("产物目录: {}", target_dir.display());
        Ok(target_dir)
    } else {
        Err(format!(
            "hardware profile verify 失败: {}，产物目录: {}",
            summary.station_id,
            target_dir.display()
        ))
    }
}

pub fn run_hardware_arm_pll_verify_state(
    station_path: &Path,
    out_dir: Option<&Path>,
) -> Result<PathBuf, String> {
    let spec = crate::read_station_spec(station_path)?;
    let target_dir = resolve_arm_output_dir(out_dir);
    fs::create_dir_all(&target_dir).map_err(|err| {
        format!(
            "无法创建 arm pll verify 输出目录 {}: {err}",
            target_dir.display()
        )
    })?;

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

    let mut recorder = VerifyRecorder::default();
    let started_at = now_ts_string();
    let smb_device = find_first_device(&resolved.resolved_spec, DeviceKind::Smb100a)?;
    let oe_device = find_first_device(&resolved.resolved_spec, DeviceKind::Oe1022d)?;

    let smb_result = verify_smb100a_fixed_profile(smb_device, &mut recorder);
    let oe_result = arm_oe1022d_ch_b_pll_state(oe_device, &mut recorder);

    let summary = HardwareProfileVerifySummary {
        station_id: spec.station_id.clone(),
        started_at,
        ended_at: now_ts_string(),
        smb100a_status: smb_result.status.clone(),
        oe1022d_status: oe_result.status.clone(),
        overall_status: if smb_result.status == "passed" && oe_result.status == "passed" {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        smb100a_checks: smb_result.checks,
        oe1022d_checks: oe_result.checks,
    };

    write_pretty_json(&target_dir.join("hardware_pll_arm_summary.json"), &summary)?;
    write_jsonl(
        &target_dir.join("hardware_pll_arm_command_audit.jsonl"),
        &recorder.command_audit,
    )?;

    if summary.overall_status == "passed" {
        println!("hardware pll arm 成功: {}", summary.station_id);
        println!("产物目录: {}", target_dir.display());
        Ok(target_dir)
    } else {
        Err(format!(
            "hardware pll arm 失败: {}，产物目录: {}",
            summary.station_id,
            target_dir.display()
        ))
    }
}

fn verify_smb100a_fixed_profile(
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
) -> DeviceVerifyResult {
    let mut checks = Vec::new();

    let result = (|| -> Result<(), String> {
        let mut transport =
            Smb100aTransport::connect(&tcp_config(device)?).map_err(|err| err.to_string())?;

        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "*IDN?",
            None,
            Some("Rohde&Schwarz"),
            &mut checks,
        )?;

        send_smb_command(&mut transport, device, recorder, "MOD:STAT ON", &mut checks)?;
        observe_smb_query(&mut transport, device, recorder, "MOD:STAT?", &mut checks)?;

        send_smb_command(&mut transport, device, recorder, "FM:SOUR INT", &mut checks)?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "FM:SOUR?",
            None,
            Some("INT"),
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "FM:MODE HDEV",
            &mut checks,
        )?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "FM:MODE?",
            None,
            Some("HDEV"),
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "FM:DEV 4000000Hz",
            &mut checks,
        )?;
        verify_smb_numeric_query(
            &mut transport,
            device,
            recorder,
            "FM:DEV?",
            4_000_000.0,
            1.0,
            &mut checks,
        )?;

        send_smb_command(&mut transport, device, recorder, "LFO ON", &mut checks)?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "LFO?",
            Some("1"),
            None,
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "LFO:VOLT 137mV",
            &mut checks,
        )?;
        verify_smb_numeric_query(
            &mut transport,
            device,
            recorder,
            "LFO:VOLT?",
            0.137,
            0.001,
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "LFO:FREQ 500Hz",
            &mut checks,
        )?;
        verify_smb_numeric_query(
            &mut transport,
            device,
            recorder,
            "LFO:FREQ?",
            500.0,
            1.0,
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "LFO:SHAP SQU",
            &mut checks,
        )?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "LFO:SHAP?",
            None,
            Some("SQU"),
            &mut checks,
        )?;

        send_smb_command(
            &mut transport,
            device,
            recorder,
            "SOUR:LFO:SIMP LOW",
            &mut checks,
        )?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "SOUR:LFO:SIMP?",
            None,
            Some("LOW"),
            &mut checks,
        )?;

        send_smb_command(&mut transport, device, recorder, "FM:STAT ON", &mut checks)?;
        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "FM:STAT?",
            Some("1"),
            None,
            &mut checks,
        )?;

        verify_smb_query(
            &mut transport,
            device,
            recorder,
            "SYST:ERR?",
            Some("0,\"No error\""),
            None,
            &mut checks,
        )?;

        Ok(())
    })();

    match result {
        Ok(()) => DeviceVerifyResult {
            status: "passed".to_string(),
            checks,
        },
        Err(message) => {
            checks.push(CheckRecord {
                label: "smb100a_profile".to_string(),
                expected: None,
                observed: Some(message),
                status: "failed".to_string(),
            });
            DeviceVerifyResult {
                status: "failed".to_string(),
                checks,
            }
        }
    }
}

fn cleanup_smb100a_profile(
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
) -> Result<(), String> {
    let mut transport =
        Smb100aTransport::connect(&tcp_config(device)?).map_err(|err| err.to_string())?;
    let mut checks = Vec::new();
    send_smb_command(&mut transport, device, recorder, "FM:STAT OFF", &mut checks)?;
    send_smb_command(
        &mut transport,
        device,
        recorder,
        "MOD:STAT OFF",
        &mut checks,
    )?;
    send_smb_command(&mut transport, device, recorder, "LFO OFF", &mut checks)?;
    Ok(())
}

fn verify_oe1022d_ch_b_profile(
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
) -> DeviceVerifyResult {
    let mut checks = Vec::new();

    let result = (|| -> Result<(), String> {
        let mut transport =
            Oe1022dTransport::open(&oe_config(device)?).map_err(|err| err.to_string())?;

        verify_oe_query(
            &mut transport,
            device,
            recorder,
            "*IDN?",
            None,
            Some("SSI LIA-OE1022D"),
            &mut checks,
        )?;

        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "FMODD 2,0",
            "FMODD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "RSLPD 2,1",
            "RSLPD? 2",
            "1",
            &mut checks,
        )?;
        apply_oe_numeric_pair(
            &mut transport,
            device,
            recorder,
            "PHASD 2,0",
            "PHASD? 2",
            0.0,
            0.01,
            &mut checks,
        )?;

        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ISRCD 2,0",
            "ISRCD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "IGNDD 2,0",
            "IGNDD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ICPLD 2,0",
            "ICPLD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ILIND 2,0",
            "ILIND? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "HARMD 2,1,1",
            "HARMD? 2,1",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "HARMD 2,2,1",
            "HARMD? 2,2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "RMODD 2,1",
            "RMODD? 2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "SENSD 2,24",
            "SENSD? 2",
            "24",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "OFLTD 2,9",
            "OFLTD? 2",
            "9",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "OFSLD 2,1",
            "OFSLD? 2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "SWVTD 2,0",
            "SWVTD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_numeric_pair(
            &mut transport,
            device,
            recorder,
            "SLVLD 2,1.000",
            "SLVLD? 2",
            1.0,
            0.001,
            &mut checks,
        )?;

        let observed_reference_frequency_hz =
            observe_oe_numeric_query(&mut transport, device, recorder, "FREQD? 2", &mut checks)?;
        let reference_chain =
            assess_oe_external_reference_chain(observed_reference_frequency_hz, &mut checks);
        let pll_state = verify_oe_pll_lock(&mut transport, device, recorder, &mut checks)?;
        if !reference_chain.ready {
            return Err(format!(
                "OE1022D 外部参考链未建立: {}；最终 PLL={pll_state}",
                reference_chain.message
            ));
        }
        if pll_state != "1" {
            return Err(format!(
                "OE1022D 外部参考频率已接近目标，但 PLL 仍未锁定: FREQD? 2={observed_reference_frequency_hz:.6} Hz, PLL={pll_state}"
            ));
        }

        verify_oe_query(
            &mut transport,
            device,
            recorder,
            OE_OVERLOAD_QUERY_PRIMARY,
            Some("0"),
            None,
            &mut checks,
        )?;
        verify_oe_query(
            &mut transport,
            device,
            recorder,
            OE_GAIN_OVERLOAD_QUERY_PRIMARY,
            Some("0"),
            None,
            &mut checks,
        )?;

        transport.clear_input().map_err(|err| err.to_string())?;
        let rall = query_oe_rall(&mut transport, device, recorder)?;
        checks.push(CheckRecord {
            label: "oe1022d_rall_nonempty".to_string(),
            expected: Some(">0 bytes".to_string()),
            observed: Some(rall.len().to_string()),
            status: if rall.is_empty() {
                "failed".to_string()
            } else {
                "passed".to_string()
            },
        });
        if rall.is_empty() {
            return Err("OE1022D RALL? 返回空 payload".to_string());
        }

        Ok(())
    })();

    match result {
        Ok(()) => DeviceVerifyResult {
            status: "passed".to_string(),
            checks,
        },
        Err(message) => {
            checks.push(CheckRecord {
                label: "oe1022d_profile".to_string(),
                expected: None,
                observed: Some(message),
                status: "failed".to_string(),
            });
            DeviceVerifyResult {
                status: "failed".to_string(),
                checks,
            }
        }
    }
}

fn arm_oe1022d_ch_b_pll_state(
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
) -> DeviceVerifyResult {
    let mut checks = Vec::new();

    let result = (|| -> Result<(), String> {
        let mut config = oe_config(device)?;
        config.timeout = Duration::from_millis(1500);
        let mut transport = Oe1022dTransport::open(&config).map_err(|err| err.to_string())?;

        verify_oe_query(
            &mut transport,
            device,
            recorder,
            "*IDN?",
            None,
            Some("SSI LIA-OE1022D"),
            &mut checks,
        )?;

        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "FMODD 2,0",
            "FMODD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "RSLPD 2,0",
            "RSLPD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_numeric_pair(
            &mut transport,
            device,
            recorder,
            "PHASD 2,0",
            "PHASD? 2",
            0.0,
            0.01,
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ISRCD 2,0",
            "ISRCD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "IGNDD 2,0",
            "IGNDD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ICPLD 2,0",
            "ICPLD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "ILIND 2,0",
            "ILIND? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "HARMD 2,1,1",
            "HARMD? 2,1",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "HARMD 2,2,1",
            "HARMD? 2,2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "RMODD 2,1",
            "RMODD? 2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "SENSD 2,24",
            "SENSD? 2",
            "24",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "OFLTD 2,9",
            "OFLTD? 2",
            "9",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "OFSLD 2,1",
            "OFSLD? 2",
            "1",
            &mut checks,
        )?;
        apply_oe_pair(
            &mut transport,
            device,
            recorder,
            "SWVTD 2,0",
            "SWVTD? 2",
            "0",
            &mut checks,
        )?;
        apply_oe_numeric_pair(
            &mut transport,
            device,
            recorder,
            "SLVLD 2,1.000",
            "SLVLD? 2",
            1.0,
            0.001,
            &mut checks,
        )?;

        // 给 OE 留足时间进入稳定态，再读参考频率和 PLL。
        thread::sleep(Duration::from_millis(PLL_SETTLE_MS));

        let observed_frequency_hz =
            observe_oe_numeric_query(&mut transport, device, recorder, "FREQD? 2", &mut checks)?;
        checks.push(CheckRecord {
            label: "oe1022d_pll_arm_mode_note".to_string(),
            expected: Some("保持在外部参考 + TTL 上升沿激活态，不 cleanup".to_string()),
            observed: Some(format!(
                "FREQD? 2={observed_frequency_hz:.6} Hz；当前状态将保留给人工复核"
            )),
            status: "passed".to_string(),
        });

        let pll_observed = query_oe_text_with_fallback(
            &mut transport,
            device,
            recorder,
            OE_PLL_QUERY_PRIMARY,
            OE_PLL_QUERY_FALLBACK,
        )?;
        let pll_normalized = normalize_oe_response(&pll_observed);
        checks.push(CheckRecord {
            label: "*PLLD? 2".to_string(),
            expected: None,
            observed: Some(pll_normalized),
            status: "passed".to_string(),
        });

        Ok(())
    })();

    match result {
        Ok(()) => DeviceVerifyResult {
            status: "passed".to_string(),
            checks,
        },
        Err(message) => {
            checks.push(CheckRecord {
                label: "oe1022d_pll_arm".to_string(),
                expected: None,
                observed: Some(message),
                status: "failed".to_string(),
            });
            DeviceVerifyResult {
                status: "failed".to_string(),
                checks,
            }
        }
    }
}

fn apply_oe_pair(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    command: &str,
    query: &str,
    expected: &str,
    checks: &mut Vec<CheckRecord>,
) -> Result<(), String> {
    send_oe_command(transport, device, recorder, command, checks)?;
    verify_oe_query(
        transport,
        device,
        recorder,
        query,
        Some(expected),
        None,
        checks,
    )?;
    Ok(())
}

#[allow(clippy::too_many_arguments)]
fn apply_oe_numeric_pair(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    command: &str,
    query: &str,
    expected: f64,
    tolerance: f64,
    checks: &mut Vec<CheckRecord>,
) -> Result<(), String> {
    send_oe_command(transport, device, recorder, command, checks)?;
    let observed = query_oe_text(transport, device, recorder, query)?;
    let numeric = parse_numeric_response(&observed)?;
    let passed = (numeric - expected).abs() <= tolerance;
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: Some(expected.to_string()),
        observed: Some(observed.clone()),
        status: if passed {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    if passed {
        Ok(())
    } else {
        Err(format!(
            "OE1022D 数值不匹配: command={query}, expected={expected}, observed={observed}"
        ))
    }
}

fn verify_smb_query(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    query: &str,
    expected_exact: Option<&str>,
    expected_contains: Option<&str>,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    let observed = query_smb_text(transport, device, recorder, query)?;
    let trimmed = observed.trim();
    let passed = match (expected_exact, expected_contains) {
        (Some(exact), _) => trimmed == exact,
        (_, Some(fragment)) => trimmed.contains(fragment),
        (None, None) => true,
    };
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: expected_exact
            .map(str::to_string)
            .or_else(|| expected_contains.map(str::to_string)),
        observed: Some(trimmed.to_string()),
        status: if passed {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    if passed {
        Ok(observed)
    } else {
        Err(format!("SMB100A readback 不匹配: {query} -> {trimmed}"))
    }
}

fn verify_smb_numeric_query(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    query: &str,
    expected: f64,
    tolerance: f64,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    let observed = query_smb_text(transport, device, recorder, query)?;
    let numeric = parse_numeric_response(&observed)?;
    let passed = (numeric - expected).abs() <= tolerance;
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: Some(expected.to_string()),
        observed: Some(observed.clone()),
        status: if passed {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    if passed {
        Ok(observed)
    } else {
        Err(format!(
            "SMB100A 数值不匹配: command={query}, expected={expected}, observed={observed}"
        ))
    }
}

fn observe_smb_query(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    query: &str,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    let observed = query_smb_text(transport, device, recorder, query)?;
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: None,
        observed: Some(observed.trim().to_string()),
        status: "passed".to_string(),
    });
    Ok(observed)
}

fn verify_oe_query(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    query: &str,
    expected_exact: Option<&str>,
    expected_contains: Option<&str>,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    let observed = query_oe_text(transport, device, recorder, query)?;
    let normalized = normalize_oe_response(&observed);
    let passed = match (expected_exact, expected_contains) {
        (Some(exact), _) => normalized == exact,
        (_, Some(fragment)) => normalized.contains(fragment),
        (None, None) => true,
    };
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: expected_exact
            .map(str::to_string)
            .or_else(|| expected_contains.map(str::to_string)),
        observed: Some(normalized.clone()),
        status: if passed {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    if passed {
        Ok(observed)
    } else {
        Err(format!("OE1022D readback 不匹配: {query} -> {normalized}"))
    }
}

fn observe_oe_numeric_query(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    query: &str,
    checks: &mut Vec<CheckRecord>,
) -> Result<f64, String> {
    let observed = query_oe_text(transport, device, recorder, query)?;
    let normalized = normalize_oe_response(&observed);
    let numeric = parse_numeric_response(&normalized)?;
    checks.push(CheckRecord {
        label: query.to_string(),
        expected: None,
        observed: Some(normalized),
        status: "passed".to_string(),
    });
    Ok(numeric)
}

fn verify_oe_pll_lock(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    let primary = pll_query_with_retry(transport, device, recorder, "RSLPD 2,1", 1, checks)?;
    if primary == "1" {
        return Ok(primary);
    }

    let fallback = pll_query_with_retry(transport, device, recorder, "RSLPD 2,0", 0, checks)?;
    if fallback == "1" {
        return Ok(fallback);
    }

    Ok(format!("RSLPD=1 -> {primary}, RSLPD=0 -> {fallback}"))
}

fn assess_oe_external_reference_chain(
    observed_reference_frequency_hz: f64,
    checks: &mut Vec<CheckRecord>,
) -> ReferenceChainAssessment {
    let freq_close = (observed_reference_frequency_hz - SMB_EXPECTED_LF_FREQ_HZ).abs()
        <= OE_EXPECTED_REF_FREQ_TOLERANCE_HZ;
    checks.push(CheckRecord {
        label: "oe1022d_reference_frequency_plausibility".to_string(),
        expected: Some(format!(
            "{} +/- {} Hz",
            SMB_EXPECTED_LF_FREQ_HZ, OE_EXPECTED_REF_FREQ_TOLERANCE_HZ
        )),
        observed: Some(format!("{observed_reference_frequency_hz:.6} Hz")),
        status: if freq_close {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });

    let sine_ready = SMB_EXPECTED_LF_VPP_V >= OE_SINE_REF_MIN_VPP_V;
    let ttl_ready = SMB_EXPECTED_LF_VPEAK_V >= OE_TTL_HIGH_MIN_V;
    let amplitude_message = format!(
        "SMB LF 设定为 {:.3} Vpeak / {:.3} Vpp；OE 正弦参考门限 > {:.3} Vpp；TTL 门限高电平 > {:.1} V、低电平 < {:.1} V",
        SMB_EXPECTED_LF_VPEAK_V,
        SMB_EXPECTED_LF_VPP_V,
        OE_SINE_REF_MIN_VPP_V,
        OE_TTL_HIGH_MIN_V,
        OE_TTL_LOW_MAX_V
    );
    checks.push(CheckRecord {
        label: "oe1022d_reference_amplitude_viability".to_string(),
        expected: Some("满足正弦或 TTL 外部参考门限".to_string()),
        observed: Some(amplitude_message.clone()),
        status: if sine_ready || ttl_ready {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });

    let ready = freq_close && (sine_ready || ttl_ready);
    let message = if !freq_close {
        format!(
            "FREQD? 2={observed_reference_frequency_hz:.6} Hz，未接近 SMB 目标 {} Hz；{}",
            SMB_EXPECTED_LF_FREQ_HZ, amplitude_message
        )
    } else if !(sine_ready || ttl_ready) {
        format!(
            "参考频率已接近目标，但 SMB LF 输出幅值仍不满足 OE 外部参考门限；{}",
            amplitude_message
        )
    } else {
        format!(
            "参考频率与幅值条件均满足，等待 PLL 锁定；{}",
            amplitude_message
        )
    };

    ReferenceChainAssessment { ready, message }
}

fn pll_query_with_retry(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    slope_command: &str,
    slope_expected: u8,
    checks: &mut Vec<CheckRecord>,
) -> Result<String, String> {
    apply_oe_pair(
        transport,
        device,
        recorder,
        slope_command,
        "RSLPD? 2",
        &slope_expected.to_string(),
        checks,
    )?;

    let mut last_value = String::new();
    for attempt in 0..5_u8 {
        thread::sleep(Duration::from_millis(800));
        let observed = query_oe_text_with_fallback(
            transport,
            device,
            recorder,
            OE_PLL_QUERY_PRIMARY,
            OE_PLL_QUERY_FALLBACK,
        )?;
        let normalized = normalize_oe_response(&observed);
        checks.push(CheckRecord {
            label: format!("*PLLD? 2 / slope={slope_expected} / attempt={attempt}"),
            expected: Some("1".to_string()),
            observed: Some(normalized.clone()),
            status: if normalized == "1" {
                "passed".to_string()
            } else {
                "failed".to_string()
            },
        });
        last_value = normalized;
        if last_value == "1" {
            break;
        }
    }

    Ok(last_value)
}

fn send_smb_command(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    command: &str,
    checks: &mut Vec<CheckRecord>,
) -> Result<(), String> {
    let started = Instant::now();
    let result = transport.send(command).map_err(|err| err.to_string());
    recorder.command_audit.push(CommandAuditRecord {
        ts: now_ts_string(),
        device_id: device.device_id.clone(),
        transport: "tcp_socket".to_string(),
        command_kind: "command_ascii".to_string(),
        command_text: command.to_string(),
        observed_response: None,
        status: if result.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        duration_ms: started.elapsed().as_millis(),
    });
    checks.push(CheckRecord {
        label: command.to_string(),
        expected: None,
        observed: None,
        status: if result.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    result?;
    thread::sleep(Duration::from_millis(SMB_COMMAND_SETTLE_MS));
    verify_smb_query(
        transport,
        device,
        recorder,
        "SYST:ERR?",
        Some("0,\"No error\""),
        None,
        checks,
    )?;
    Ok(())
}

fn send_oe_command(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
    command: &str,
    checks: &mut Vec<CheckRecord>,
) -> Result<(), String> {
    transport.clear_input().map_err(|err| err.to_string())?;
    let started = Instant::now();
    let result = transport.send(command).map_err(|err| err.to_string());
    recorder.command_audit.push(CommandAuditRecord {
        ts: now_ts_string(),
        device_id: device.device_id.clone(),
        transport: "serial_port".to_string(),
        command_kind: "command_ascii".to_string(),
        command_text: command.to_string(),
        observed_response: None,
        status: if result.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        duration_ms: started.elapsed().as_millis(),
    });
    checks.push(CheckRecord {
        label: command.to_string(),
        expected: None,
        observed: None,
        status: if result.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
    });
    result?;
    thread::sleep(Duration::from_millis(OE_COMMAND_SETTLE_MS));
    Ok(())
}

fn query_smb_text(
    transport: &mut Smb100aTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
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
    recorder: &mut VerifyRecorder,
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
    recorder: &mut VerifyRecorder,
    primary: &str,
    fallback: &str,
) -> Result<String, String> {
    match query_oe_text(transport, device, recorder, primary) {
        Ok(value) => Ok(value),
        Err(_) => query_oe_text(transport, device, recorder, fallback),
    }
}

fn query_oe_rall(
    transport: &mut Oe1022dTransport,
    device: &DeviceSpec,
    recorder: &mut VerifyRecorder,
) -> Result<Vec<u8>, String> {
    let started = Instant::now();
    let observed = transport
        .query_rall_frame_until_timeout(16384)
        .map_err(|err| err.to_string());
    recorder.command_audit.push(CommandAuditRecord {
        ts: now_ts_string(),
        device_id: device.device_id.clone(),
        transport: "serial_port".to_string(),
        command_kind: "query_binary".to_string(),
        command_text: "RALL?".to_string(),
        observed_response: observed
            .as_ref()
            .ok()
            .map(|payload| format!("len={}, head={}", payload.len(), hex_preview(payload, 32))),
        status: if observed.is_ok() {
            "passed".to_string()
        } else {
            "failed".to_string()
        },
        duration_ms: started.elapsed().as_millis(),
    });
    observed
}

fn normalize_oe_response(text: &str) -> String {
    text.chars()
        .filter(|ch| *ch != '\0')
        .collect::<String>()
        .trim()
        .to_string()
}

fn parse_numeric_response(text: &str) -> Result<f64, String> {
    let normalized = text.chars().filter(|ch| *ch != '\0').collect::<String>();
    let trimmed = normalized.trim();
    trimmed
        .parse::<f64>()
        .map_err(|err| format!("数值解析失败: '{trimmed}', {err}"))
}

fn resolve_output_dir(out_dir: Option<&Path>) -> PathBuf {
    if let Some(path) = out_dir {
        return path.to_path_buf();
    }
    let stamp = now_ts_string();
    PathBuf::from(format!("out/hardware_profile_verify/{stamp}"))
}

fn resolve_arm_output_dir(out_dir: Option<&Path>) -> PathBuf {
    if let Some(path) = out_dir {
        return path.to_path_buf();
    }
    let stamp = now_ts_string();
    PathBuf::from(format!("out/hardware_pll_arm/{stamp}"))
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

fn hex_preview(bytes: &[u8], limit: usize) -> String {
    bytes
        .iter()
        .take(limit)
        .map(|byte| format!("{byte:02X}"))
        .collect::<Vec<_>>()
        .join(" ")
}

#[derive(Debug, Clone, Serialize)]
struct HardwareProfileVerifySummary {
    station_id: String,
    started_at: String,
    ended_at: String,
    smb100a_status: String,
    oe1022d_status: String,
    overall_status: String,
    smb100a_checks: Vec<CheckRecord>,
    oe1022d_checks: Vec<CheckRecord>,
}

#[derive(Debug, Clone, Serialize)]
struct CheckRecord {
    label: String,
    expected: Option<String>,
    observed: Option<String>,
    status: String,
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
struct VerifyRecorder {
    command_audit: Vec<CommandAuditRecord>,
}

struct DeviceVerifyResult {
    status: String,
    checks: Vec<CheckRecord>,
}

struct ReferenceChainAssessment {
    ready: bool,
    message: String,
}
