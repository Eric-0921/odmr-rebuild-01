# C# 主栈与 Rust 归档边界

本文定义 `win-csharp-rebuild` 分支当前的运行入口和归档边界。

## 主栈

日常实验、审查和设备验证以 Windows C# 为主栈：

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/win_csharp_primary_15min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_primary_15min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_primary_15min --out runs/win_csharp_primary_15min/continuity_audit.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- device-command-check
```

设备 probe 入口：

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only
```

## Rust 归档边界

Rust 代码保留在仓库中作为归档参考：

- 历史 runtime 和 GUI/live bridge 的实现参考。
- 历史 artifact/audit 行为的对照参考。
- 不再作为日常实验运行、审查或设备验证的必要依赖。

新增功能默认进入 C# 主栈。只有在查历史行为或回归定位时才读取 Rust 实现。

## Collector 边界

OE1022D `RALL?` collector 热路径保持：

```text
write RALL?
sleep 30ms
blocking exact read 12288B
append raw
append frame index
```

以下能力只允许在 collector 外侧实现：

- artifact-check
- audit-continuity
- quality
- parser
- GUI/live reducer
- retry/deadline 实验

如果未来确实要改变 collector 热路径，必须作为单独 gate，并重新跑 60s、15min、repeat 15min 连续性验证。

## 当前验收状态

- C# `run-execute` 已完成 minimal 3-point 和 15min laser background 真机验证。
- C# `artifact-check` 已通过 15min run artifact 合同检查。
- C# `audit-continuity` 已在关键字段上对齐 Rust 归档 audit。
- C# `device-command-check` 已覆盖 OE1022D、SMB100A、M8812、CNI Laser 当前 Rust command crates 的命令面。
