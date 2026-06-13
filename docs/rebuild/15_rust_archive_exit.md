# Rust 归档退出边界

本文定义当前仓库从 Rust 主栈退出后的边界。

## 当前结论

C# 是唯一日常实验主栈：

- 设备 probe
- JSON 配置解析
- runtime 执行
- artifact 写出
- artifact-check
- audit-continuity
- device command catalog

Rust 保留为归档参考，不再作为以下流程的必要依赖：

- 日常实验采集
- 真机验收
- 连续性审计
- artifact 合同审查
- 当前 GUI/live 工作

## 当前 C# 主入口

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_manual_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_manual_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_manual_minimal --out runs/win_csharp_manual_minimal/continuity_audit.json
```

设备 probe:

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only
```

## Rust 目录状态

以下目录保留但归档：

- `crates/`
- `apps/odmr-cli/`
- `Cargo.toml`
- `Cargo.lock`

允许用途：

- 查历史实现
- 查旧命令白名单来源
- 做回归定位时的人读参考

禁止用途：

- 作为日常实验入口
- 作为真机验收必跑项
- 作为 artifact 审查必需项
- 作为 GUI/live 默认后端

## Python GUI 状态

`python/odmr_gui` 是旧 GUI/live bridge 参考。当前阶段不做前端，不自动启动 Rust `gui-bridge`。

需要恢复 GUI/live 时，应优先基于 C# artifact/events/live reducer 重新设计，不能把旧 Cargo bridge 重新变成默认依赖。

## 验收定义

一次完整的“脱离 Rust”验收至少包括：

- Windows `dotnet build tools/win-csharp/Odmr.Win.sln` 通过
- C# minimal 3-point run 完成
- C# 15min laser background run 完成
- C# `artifact-check` 通过
- C# `audit-continuity` 返回 `continuous`
- README 和当前运行手册不再把 Cargo/Rust CLI 作为日常命令

历史文档可以保留 Rust/Cargo 记录，但必须从语境上是历史记录或归档参考。
