//! 最小 CLI 入口。
//!
//! 当前已经包含：
//! - `station verify`
//! - `hardware smoke`
//! - `hardware verify-mag-lock`
//! - `run execute`
//!
//! 这不是最终产品 CLI，但已经覆盖当前重建路线的核心 bring-up / runtime 链路。

mod hardware_profile_verify;
mod hardware_smoke;
mod hardware_state_snapshot;
mod hardware_verify_mag_lock;
mod run_execute;

use hardware_profile_verify::run_hardware_arm_pll_verify_state;
use hardware_profile_verify::run_hardware_profile_verify;
use hardware_smoke::run_hardware_smoke;
use hardware_state_snapshot::run_hardware_state_snapshot;
use hardware_verify_mag_lock::run_hardware_verify_mag_lock;
use run_execute::run_execute;
use station_resolver::{resolve_station, StationSpec};
use std::env;
use std::fs;
use std::path::{Path, PathBuf};

fn main() {
    if let Err(err) = run(env::args().collect()) {
        eprintln!("{err}");
        std::process::exit(1);
    }
}

fn run(args: Vec<String>) -> Result<(), String> {
    let command = parse_command(&args)?;
    match command {
        CliCommand::StationVerify { station, out } => run_station_verify(&station, out.as_deref()),
        CliCommand::HardwareSmoke { station, out_dir } => {
            run_hardware_smoke(&station, out_dir.as_deref()).map(|_| ())
        }
        CliCommand::HardwareVerifyProfile { station, out_dir } => {
            run_hardware_profile_verify(&station, out_dir.as_deref()).map(|_| ())
        }
        CliCommand::HardwareArmPllVerifyState { station, out_dir } => {
            run_hardware_arm_pll_verify_state(&station, out_dir.as_deref()).map(|_| ())
        }
        CliCommand::HardwareSnapshotPllState { station, out_dir } => {
            run_hardware_state_snapshot(&station, out_dir.as_deref()).map(|_| ())
        }
        CliCommand::HardwareVerifyMagLock {
            station,
            calibration,
            plan,
            out_dir,
        } => run_hardware_verify_mag_lock(&station, &calibration, &plan, out_dir.as_deref())
            .map(|_| ()),
        CliCommand::RunExecute {
            station,
            calibration,
            plan,
            smb_profile,
            oe_profile,
            laser_profile,
            out_dir,
        } => run_execute(
            &station,
            &calibration,
            &plan,
            &smb_profile,
            &oe_profile,
            &laser_profile,
            out_dir.as_deref(),
        )
        .map(|_| ()),
    }
}

fn run_station_verify(station_path: &Path, out_path: Option<&Path>) -> Result<(), String> {
    let spec = read_station_spec(station_path)?;
    let resolved = resolve_station(&spec);
    let snapshot = resolved.snapshot;
    let snapshot_json = serde_json::to_string_pretty(&snapshot)
        .map_err(|err| format!("snapshot 序列化失败: {err}"))?;

    if let Some(out_path) = out_path {
        if let Some(parent) = out_path.parent() {
            fs::create_dir_all(parent)
                .map_err(|err| format!("无法创建输出目录 {}: {err}", parent.display()))?;
        }
        fs::write(out_path, &snapshot_json)
            .map_err(|err| format!("无法写入 snapshot {}: {err}", out_path.display()))?;
        println!("station verify 成功: {}", spec.station_id);
        println!("snapshot 已写入: {}", out_path.display());
    } else {
        println!("{snapshot_json}");
    }

    if snapshot.has_required_failures() {
        Err(format!(
            "station verify 失败: required_failures={}",
            snapshot.required_failures
        ))
    } else {
        Ok(())
    }
}

fn read_station_spec(path: &Path) -> Result<StationSpec, String> {
    let text = fs::read_to_string(path)
        .map_err(|err| format!("无法读取 station 配置 {}: {err}", path.display()))?;
    serde_json::from_str(&text)
        .map_err(|err| format!("station 配置 JSON 解析失败 {}: {err}", path.display()))
}

#[derive(Debug, PartialEq, Eq)]
enum CliCommand {
    StationVerify {
        station: PathBuf,
        out: Option<PathBuf>,
    },
    HardwareSmoke {
        station: PathBuf,
        out_dir: Option<PathBuf>,
    },
    HardwareVerifyProfile {
        station: PathBuf,
        out_dir: Option<PathBuf>,
    },
    HardwareArmPllVerifyState {
        station: PathBuf,
        out_dir: Option<PathBuf>,
    },
    HardwareSnapshotPllState {
        station: PathBuf,
        out_dir: Option<PathBuf>,
    },
    HardwareVerifyMagLock {
        station: PathBuf,
        calibration: PathBuf,
        plan: PathBuf,
        out_dir: Option<PathBuf>,
    },
    RunExecute {
        station: PathBuf,
        calibration: PathBuf,
        plan: PathBuf,
        smb_profile: PathBuf,
        oe_profile: PathBuf,
        laser_profile: PathBuf,
        out_dir: Option<PathBuf>,
    },
}

fn parse_command(args: &[String]) -> Result<CliCommand, String> {
    if args.len() < 2 {
        return Err(usage());
    }

    match args.get(1).map(String::as_str) {
        Some("station") => parse_station_command(args),
        Some("hardware") => parse_hardware_command(args),
        Some("run") => parse_run_command(args),
        _ => Err(usage()),
    }
}

fn parse_station_command(args: &[String]) -> Result<CliCommand, String> {
    match args.get(2).map(String::as_str) {
        Some("verify") => parse_station_verify(args),
        _ => Err(usage()),
    }
}

fn parse_hardware_command(args: &[String]) -> Result<CliCommand, String> {
    match args.get(2).map(String::as_str) {
        Some("smoke") => parse_hardware_smoke(args),
        Some("verify-profile") => parse_hardware_verify_profile(args),
        Some("arm-pll-verify-state") => parse_hardware_arm_pll_verify_state(args),
        Some("snapshot-pll-state") => parse_hardware_snapshot_pll_state(args),
        Some("verify-mag-lock") => parse_hardware_verify_mag_lock(args),
        _ => Err(usage()),
    }
}

fn parse_run_command(args: &[String]) -> Result<CliCommand, String> {
    match args.get(2).map(String::as_str) {
        Some("execute") => parse_run_execute(args),
        _ => Err(usage()),
    }
}

fn parse_station_verify(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut out: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--out" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out 缺少路径参数".to_string());
                };
                out = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => {
                return Err(format!("未知参数: {other}\n\n{}", usage()));
            }
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };

    Ok(CliCommand::StationVerify { station, out })
}

fn parse_hardware_smoke(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };

    Ok(CliCommand::HardwareSmoke { station, out_dir })
}

fn parse_hardware_verify_profile(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };

    Ok(CliCommand::HardwareVerifyProfile { station, out_dir })
}

fn parse_hardware_snapshot_pll_state(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };

    Ok(CliCommand::HardwareSnapshotPllState { station, out_dir })
}

fn parse_hardware_arm_pll_verify_state(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };

    Ok(CliCommand::HardwareArmPllVerifyState { station, out_dir })
}

fn parse_run_execute(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut calibration: Option<PathBuf> = None;
    let mut plan: Option<PathBuf> = None;
    let mut smb_profile: Option<PathBuf> = None;
    let mut oe_profile: Option<PathBuf> = None;
    let mut laser_profile: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--plan" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--plan 缺少路径参数".to_string());
                };
                plan = Some(PathBuf::from(value));
                index += 2;
            }
            "--calibration" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--calibration 缺少路径参数".to_string());
                };
                calibration = Some(PathBuf::from(value));
                index += 2;
            }
            "--smb-profile" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--smb-profile 缺少路径参数".to_string());
                };
                smb_profile = Some(PathBuf::from(value));
                index += 2;
            }
            "--oe-profile" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--oe-profile 缺少路径参数".to_string());
                };
                oe_profile = Some(PathBuf::from(value));
                index += 2;
            }
            "--laser-profile" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--laser-profile 缺少路径参数".to_string());
                };
                laser_profile = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };
    let Some(calibration) = calibration else {
        return Err(format!("缺少 --calibration 参数\n\n{}", usage()));
    };
    let Some(plan) = plan else {
        return Err(format!("缺少 --plan 参数\n\n{}", usage()));
    };
    let Some(smb_profile) = smb_profile else {
        return Err(format!("缺少 --smb-profile 参数\n\n{}", usage()));
    };
    let Some(oe_profile) = oe_profile else {
        return Err(format!("缺少 --oe-profile 参数\n\n{}", usage()));
    };
    let Some(laser_profile) = laser_profile else {
        return Err(format!("缺少 --laser-profile 参数\n\n{}", usage()));
    };

    Ok(CliCommand::RunExecute {
        station,
        calibration,
        plan,
        smb_profile,
        oe_profile,
        laser_profile,
        out_dir,
    })
}

fn parse_hardware_verify_mag_lock(args: &[String]) -> Result<CliCommand, String> {
    let mut station: Option<PathBuf> = None;
    let mut calibration: Option<PathBuf> = None;
    let mut plan: Option<PathBuf> = None;
    let mut out_dir: Option<PathBuf> = None;
    let mut index = 3_usize;

    while index < args.len() {
        match args[index].as_str() {
            "--station" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--station 缺少路径参数".to_string());
                };
                station = Some(PathBuf::from(value));
                index += 2;
            }
            "--calibration" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--calibration 缺少路径参数".to_string());
                };
                calibration = Some(PathBuf::from(value));
                index += 2;
            }
            "--plan" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--plan 缺少路径参数".to_string());
                };
                plan = Some(PathBuf::from(value));
                index += 2;
            }
            "--out-dir" => {
                let Some(value) = args.get(index + 1) else {
                    return Err("--out-dir 缺少路径参数".to_string());
                };
                out_dir = Some(PathBuf::from(value));
                index += 2;
            }
            "--help" | "-h" => return Err(usage()),
            other => return Err(format!("未知参数: {other}\n\n{}", usage())),
        }
    }

    let Some(station) = station else {
        return Err(format!("缺少 --station 参数\n\n{}", usage()));
    };
    let Some(calibration) = calibration else {
        return Err(format!("缺少 --calibration 参数\n\n{}", usage()));
    };
    let Some(plan) = plan else {
        return Err(format!("缺少 --plan 参数\n\n{}", usage()));
    };

    Ok(CliCommand::HardwareVerifyMagLock {
        station,
        calibration,
        plan,
        out_dir,
    })
}

fn usage() -> String {
    [
        "用法:",
        "  odmr station verify --station <path> [--out <path>]",
        "  odmr hardware smoke --station <path> [--out-dir <path>]",
        "  odmr hardware verify-profile --station <path> [--out-dir <path>]",
        "  odmr hardware arm-pll-verify-state --station <path> [--out-dir <path>]",
        "  odmr hardware snapshot-pll-state --station <path> [--out-dir <path>]",
        "  odmr hardware verify-mag-lock --station <path> --calibration <path> --plan <path> [--out-dir <path>]",
        "  odmr run execute --station <path> --calibration <path> --plan <path> --smb-profile <path> --oe-profile <path> --laser-profile <path> [--out-dir <path>]",
        "",
        "示例:",
        "  odmr station verify --station configs/stations/lab_a.json",
        "  odmr station verify --station configs/stations/lab_a.json --out out/station_snapshot.json",
        "  odmr hardware smoke --station configs/stations/lab_a.json --out-dir out/hardware_smoke/manual",
        "  odmr hardware verify-profile --station configs/stations/lab_a.json --out-dir out/hardware_profile_verify/manual",
        "  odmr hardware arm-pll-verify-state --station configs/stations/lab_a.json --out-dir out/hardware_pll_arm/manual",
        "  odmr hardware snapshot-pll-state --station configs/stations/lab_a.json --out-dir out/hardware_state_snapshot/manual",
        "  odmr hardware verify-mag-lock --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/mag_zero_lock_verify.json --out-dir out/hardware_verify_mag_lock/manual",
        "  odmr run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_pll_default.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/manual",
        "  odmr run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_short_sweep_15min.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/x_axis_1d_bounce_15min",
    ]
    .join("\n")
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_station_verify_command() {
        let args = vec![
            "odmr".to_string(),
            "station".to_string(),
            "verify".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--out".to_string(),
            "out/station_snapshot.json".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::StationVerify {
                station: PathBuf::from("configs/stations/lab_a.json"),
                out: Some(PathBuf::from("out/station_snapshot.json")),
            }
        );
    }

    #[test]
    fn parse_requires_station_argument() {
        let args = vec![
            "odmr".to_string(),
            "station".to_string(),
            "verify".to_string(),
        ];
        let err = parse_command(&args).unwrap_err();
        assert!(err.contains("--station"));
    }

    #[test]
    fn parse_hardware_smoke_command() {
        let args = vec![
            "odmr".to_string(),
            "hardware".to_string(),
            "smoke".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--out-dir".to_string(),
            "out/hardware_smoke/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::HardwareSmoke {
                station: PathBuf::from("configs/stations/lab_a.json"),
                out_dir: Some(PathBuf::from("out/hardware_smoke/manual")),
            }
        );
    }

    #[test]
    fn parse_hardware_verify_profile_command() {
        let args = vec![
            "odmr".to_string(),
            "hardware".to_string(),
            "verify-profile".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--out-dir".to_string(),
            "out/hardware_profile_verify/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::HardwareVerifyProfile {
                station: PathBuf::from("configs/stations/lab_a.json"),
                out_dir: Some(PathBuf::from("out/hardware_profile_verify/manual")),
            }
        );
    }

    #[test]
    fn parse_hardware_snapshot_pll_state_command() {
        let args = vec![
            "odmr".to_string(),
            "hardware".to_string(),
            "snapshot-pll-state".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--out-dir".to_string(),
            "out/hardware_state_snapshot/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::HardwareSnapshotPllState {
                station: PathBuf::from("configs/stations/lab_a.json"),
                out_dir: Some(PathBuf::from("out/hardware_state_snapshot/manual")),
            }
        );
    }

    #[test]
    fn parse_hardware_arm_pll_verify_state_command() {
        let args = vec![
            "odmr".to_string(),
            "hardware".to_string(),
            "arm-pll-verify-state".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--out-dir".to_string(),
            "out/hardware_pll_arm/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::HardwareArmPllVerifyState {
                station: PathBuf::from("configs/stations/lab_a.json"),
                out_dir: Some(PathBuf::from("out/hardware_pll_arm/manual")),
            }
        );
    }

    #[test]
    fn parse_hardware_verify_mag_lock_command() {
        let args = vec![
            "odmr".to_string(),
            "hardware".to_string(),
            "verify-mag-lock".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--calibration".to_string(),
            "configs/calibrations/main.json".to_string(),
            "--plan".to_string(),
            "configs/plans/mag_zero_lock_verify.json".to_string(),
            "--out-dir".to_string(),
            "out/hardware_verify_mag_lock/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::HardwareVerifyMagLock {
                station: PathBuf::from("configs/stations/lab_a.json"),
                calibration: PathBuf::from("configs/calibrations/main.json"),
                plan: PathBuf::from("configs/plans/mag_zero_lock_verify.json"),
                out_dir: Some(PathBuf::from("out/hardware_verify_mag_lock/manual")),
            }
        );
    }

    #[test]
    fn parse_run_execute_command() {
        let args = vec![
            "odmr".to_string(),
            "run".to_string(),
            "execute".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--calibration".to_string(),
            "configs/calibrations/main.json".to_string(),
            "--plan".to_string(),
            "configs/plans/minimal_3point_runtime.json".to_string(),
            "--smb-profile".to_string(),
            "configs/profiles/smb100a_run_pll_default.json".to_string(),
            "--oe-profile".to_string(),
            "configs/profiles/oe1022d_run_ch_b_observed.json".to_string(),
            "--laser-profile".to_string(),
            "configs/profiles/cni_laser_run_on_background.json".to_string(),
            "--out-dir".to_string(),
            "runs/manual".to_string(),
        ];

        let command = parse_command(&args).unwrap();
        assert_eq!(
            command,
            CliCommand::RunExecute {
                station: PathBuf::from("configs/stations/lab_a.json"),
                calibration: PathBuf::from("configs/calibrations/main.json"),
                plan: PathBuf::from("configs/plans/minimal_3point_runtime.json"),
                smb_profile: PathBuf::from("configs/profiles/smb100a_run_pll_default.json"),
                oe_profile: PathBuf::from("configs/profiles/oe1022d_run_ch_b_observed.json"),
                laser_profile: PathBuf::from("configs/profiles/cni_laser_run_on_background.json"),
                out_dir: Some(PathBuf::from("runs/manual")),
            }
        );
    }

    #[test]
    fn parse_run_execute_requires_laser_profile() {
        let args = vec![
            "odmr".to_string(),
            "run".to_string(),
            "execute".to_string(),
            "--station".to_string(),
            "configs/stations/lab_a.json".to_string(),
            "--calibration".to_string(),
            "configs/calibrations/main.json".to_string(),
            "--plan".to_string(),
            "configs/plans/minimal_3point_runtime.json".to_string(),
            "--smb-profile".to_string(),
            "configs/profiles/smb100a_run_pll_default.json".to_string(),
            "--oe-profile".to_string(),
            "configs/profiles/oe1022d_run_ch_b_observed.json".to_string(),
        ];

        let err = parse_command(&args).unwrap_err();
        assert!(err.contains("--laser-profile"));
    }
}
