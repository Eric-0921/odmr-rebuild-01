# 当前实验主链路可信性阶段

本文记录 `win-csharp-rebuild` 当前阶段的工程边界：先把真实实验采集链路做可信，不做 OE 常驻 `RALL?` reader，也不做前端 live monitor。

## 当前范围

本阶段只处理三件事：

- RF sweep 窗口必须和 segment 对齐。
- 每个 point 必须有设备上下文和 provenance。
- artifact-check 必须能离线审查 point、segment、quality、device_state 的合同。

以下内容暂缓，不阻塞当前实验采集：

- OE `RALL?` 常驻 live collector。
- recording gate。
- 前端 live monitor / GUI 显示。

暂缓不是取消。后续要做 LabVIEW-style live monitor 时，仍按“RALL reader 常驻、UI 读 latest、run/save 只打开 recorder”的方向做，但不能先让它影响当前可用的实验链路。

## RF Sweep Window 规则

SMB100A 的 RF output 和 frequency sweep state 分开管理：

- `OUTP ON` 是 run 级状态，run 开始后打开，point 之间不反复开关。
- point 间隙保持 `FREQ:MODE CW`。
- point 间隙频率设为当前 point 的 `start_hz`。
- 每个 point 先完成 M8812 target current 和 measured current 记录。
- SMB sweep 参数在 CW 状态下配置。
- 一批 SMB 配置命令之后只做一次 `SYST:ERR?`。
- `segment_start` 之后才切 `FREQ:MODE SWE` 并执行 `SWE:FREQ:EXEC`。
- `*OPC?` 用于观察 sweep 完成；如果设备过早返回，runtime 使用 sweep duration 估算 fallback。
- sweep 完成后切回 `FREQ:MODE CW` 并设置 `FREQ start_hz`。
- 然后才写 `segment_end`。

因此 artifact 里的 segment 覆盖的是实际 RF exposure 窗口，而不是“先把 sweep 打开，再开始采集”的错位窗口。

## Point Device Context Contract

每个 run 必须输出：

```text
device_state.jsonl
```

每个 point 一行，记录：

- `run_id`
- `point_id`
- `point_index`
- `target_b_nt`
- `target_current_a`
- `measured_current_a`
- `smb_profile_id`
- `smb_sweep`
- `smb_configure_error`
- `smb_sweep_execution`
- `rf_exposure`
- `segment`
- `laser_profile_id`
- `laser_mode`
- `laser_power_mw`
- `oe_profile_id`

`rf_exposure` 必须包含：

- `started_ts`
- `ended_ts`
- `started_monotonic_ns`
- `ended_monotonic_ns`
- `segment_start_monotonic_ns`
- `segment_end_monotonic_ns`

`segment` 必须绑定：

- `segment_id`
- `frame_seq_start`
- `frame_seq_end`
- `raw_offset_start`
- `raw_offset_end`

本阶段不做 SMB 全量逐参数 readback。point 的可信性来自 intended config、批次 `SYST:ERR?`、`SWE:FREQ:EXEC` / `*OPC?` 结果、M8812 measured current、RF exposure 时间戳和 segment/raw/frame 绑定。

OE1022D 运行状态仍以 `RALL?` artifact 为主，不把 OE parser 或 fixed readback 插入 collector 热路径。

## Artifact Check Contract

`artifact-check` 必须检查：

- `summary.json`
- `run_manifest.json`
- snapshots
- `events.jsonl`
- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`
- `points.jsonl`
- `quality.jsonl`
- `device_state.jsonl`

核心条件：

- `raw size == frames_total * 12288`
- `idx lines == frames_total`
- `segments/points/quality/device_state` 行数一致
- 每个 `point_id` 都有 matching segment 和 device_state
- `device_state.segment` 与 `segments.jsonl` 的 segment id、frame range、raw offset range 一致
- `rf_exposure.started_monotonic_ns >= segment.start_monotonic_ns`
- `rf_exposure.ended_monotonic_ns <= segment.end_monotonic_ns`
- `summary.status == run_manifest.status`
- required events 覆盖 run、collector、profile、laser、point、sweep、cleanup

`audit-continuity` 继续是离线工具，只读 artifact，不进入 collector。

## RALL Collector 边界

当前阶段不改变 OE `RALL?` collector 热路径，也不改变 run-scoped 生命周期：

```text
write RALL?
sleep 30ms
blocking exact read 12288B
append raw
append frame index
```

禁止放入热路径：

- parser
- GUI/live publish
- quality/audit
- retry/deadline
- 空窗补采
- 多 reader

如果未来要把 `RALL?` 改成 station-scoped 常驻线程，必须作为单独 gate，并重新跑 60s、15min、repeat 15min continuity。

## 当前验收命令

Windows:

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_gate19_device_state_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_gate19_device_state_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_gate19_device_state_minimal --out runs/win_csharp_gate19_device_state_minimal/continuity_audit.json
```

15min laser background run 继续作为长测验收：

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/win_csharp_gate20_rf_context_laser_15min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_gate20_rf_context_laser_15min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_gate20_rf_context_laser_15min --out runs/win_csharp_gate20_rf_context_laser_15min/continuity_audit.json
```
