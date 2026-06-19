# Station Verify 与真机连通清单

本文只服务当前 Windows C# 主栈目标：

- 用 C# probe 跑通 4 类设备最小真机连通验证
- 用 C# `run-resolve` 检查 JSON 配置
- 用 C# `run-execute` 跑通 `RF + Mag + OE + Laser background` 核心链路
- 用 C# `artifact-check` 和 `audit-continuity` 做离线审查

当前已经真机验证过的命令和链路汇总见：

- `08_verified_command_and_runtime_baseline.md`
- `13_csharp_primary_stack.md`
- `14_experiment_reliability_without_live_frontend.md`

它不是 run launcher，也不是 runtime checklist。

## 当前命令

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln

dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --resource USB0::0x0AAD::0x0054::106789::INSTR
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only

dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json

dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_manual_minimal

dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_manual_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_manual_minimal --out runs/win_csharp_manual_minimal/continuity_audit.json
```

输出行为：

- probes 直接输出设备身份、连接和最小状态
- `run-execute` 写出 snapshots、manifest、events、raw、frames.idx、segments、points、quality、device_state、summary
- `artifact-check` 和 `audit-continuity` 只读 artifact，不碰设备

串口路径规则：

- 当前 Windows 实验机固定事实是：OE `ASRL8::INSTR`，SMB USB VISA resource，M8812 `COM4/COM6/COM3`，Laser `COM9`
- SMB100A resource 在 NI-VISA 环境中可能显示为 `USB0::...`，在文档/示例中也常见 `USB::...`；C# resolver 两种前缀都接受，最终以 `*IDN?` 身份匹配为准。
- `station.json` 保存这些事实和 identity 条件
- 真机 run 的 provenance 来自 snapshots、`device_state.jsonl`、segments/raw/frame index 和离线审查
- 端口变化时先跑 C# probes，不回到旧 Rust `station verify`

## C# probes 当前做什么

### SMB100A

- 使用 VISA USB resource 建连到 `USB::...::INSTR`
- 发送 `*IDN?`
- 发送 `SYST:ERR?`
- 发送 `OUTP?`

### OE1022D

- 通过 NI-VISA 打开 `ASRL8::INSTR`
- `oe-idn` 发送 `*IDN?` 并校验身份
- `oe-rall` 用冻结热路径做定长稳定性采集

### M8812

- 使用 `COM4/COM6/COM3`
- 每轴执行：
  - `*IDN?`
  - `SYST:REM`
  - `CURR 0.00000`
  - `OUTP 0`
  - `MEAS:CURR?`
  - `SYST:LOC`
- 严格校验 SN
- probe 不设置非零电流，不开启输出

### CNI Laser

- 使用 `COM9`
- `laser-probe --off-only` 只发送 `Laser Off` 帧
- 读取固定长度 echo
- 用 echo 作为第一版最小验证

## C# probes 当前不做什么

- 不做网络扫描
- 不做 preflight
- 不做 error queue drain
- 不做推断式端口发现
- 不开启 laser
- 不启动 OE 常驻 RALL

注意：

- probes 是连接事实验证，不是完整实验。
- 完整实验链路由 `run-execute` 验证。

## C# runtime 当前做什么

- 加载 `calibration`
- 做一次 `Maynuo baseline lock`
- 启动 run 级单 reader `RALL?` collector
- 逐 point 执行：
  - `target_b_nt -> calibration -> target_current_a`
  - `SMB100A` 在 CW 状态下配置 sweep
  - `segment_start` 后才切 `FREQ:MODE SWE` 并执行 `SWE:FREQ:EXEC`
  - `SWE:FREQ:EXEC + *OPC?`
  - sweep 完成后回到 `FREQ:MODE CW + FREQ start_hz`
  - segment 覆盖真实 RF exposure window
  - 再按 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl` 回切 point 窗口
  - 写 `device_state.jsonl` 绑定 intended/measured/current/sweep/RF exposure/segment
- cleanup 后确认：
  - `OUTP? -> 0`
  - `FREQ:MODE? -> CW`
  - `SYST:ERR? -> 0,"No error"`

当前代码已经切到的新基线：

- collector 不是“设备一连上就启动”，而是 C# `run-execute` 启动后才创建
- point 语义已经改成 `1 point = 1 sweep`
- `*OPC?` 已在 rebuild 真机短 run 中被证伪：它会过早返回，不能单独当 point 结束信号
- runtime 现已改成 `*OPC?` + sweep duration fallback
- 程序结束时 RF output 与 RF frequency sweep state 会退出
- 15 分钟 C# laser background run 已通过：`21/21 points`，`timeouts=0`，`raw_len_bad=0`，`delta_gt1=0`，C# audit `continuous`

## 实验室前准备

在去实验室前，先完成这些本地动作：

1. `dotnet build tools/win-csharp/Odmr.Win.sln`
2. 优先使用 `configs/stations/lab_a.json`
3. 确认其中的 `host / port_path / SN` 仍然匹配你当前实验室真实值
4. 明确哪些设备这次是 required，哪些是 optional

## 实验室最小核验顺序

建议顺序不要乱，先从最可观测、最简单的设备开始。

1. SMB100A
2. 单台 M8812
3. 其余两台 M8812
4. OE1022D
5. CNI Laser

原因：

- 先验证 TCP socket 和 SCPI query 路径
- 再验证串口 SCPI 路径
- 最后再碰二进制 laser 帧

## 各设备最小通过标准

### SMB100A

通过标准：

- TCP 可连接
- `*IDN?` 返回包含 `Rohde&Schwarz` 和 `SMB100A`
- `SYST:ERR?` 可读且无致命错误
- `OUTP? / FREQ? / POW? / SWE:FREQ:STEP? / SWE:FREQ:DWEL?` 可读

失败时先查：

- IP 是否变了
- 5025 端口是否可达
- 设备是否在 remote 可访问网络上

### M8812

通过标准：

- 串口可打开
- `*IDN?` 返回 `MAYNUO,M8812,<SN>,V2.7`
- SN 与配置一致
- `10mA` 微测后 `MEAS:CURR?` 有合理非零回读
- cleanup 可完成

失败时先查：

- 端口路径是否因重枚举变化
- 目标轴 SN 是否写错
- DTR 是否真的需要开启

### OE1022D

通过标准：

- 串口可打开
- `*IDN?` 返回 `SSI LIA-OE1022D`
- `RALL?` 单帧返回非零 payload

当前阶段不要求：

- 当场跑 collector
- 当场跑长时间 `RALL?` 稳定性

那是下一阶段。

### CNI Laser

通过标准：

- 串口可打开
- `Laser Off` 帧可写入
- 能读回预期 echo

当前阶段不要求：

- 开激光
- 设非零功率
- 验证光功率输出

## 真机 verify 后你应该拿到什么

至少要留下：

- 你实际使用的 `station.json`
- C# probe stdout 或保存的日志
- `run-execute` 输出目录
- snapshots
- `device_state.jsonl`
- `artifact-check` 输出
- `continuity_audit.json`
- 每台设备的观察到的身份字符串或 echo
- 如果失败，记录失败设备、连接事实、错误信息

## 进入实验采集的条件

当以下条件都满足时，再进入 runtime：

- 4 类设备的最小连接路径都被真机验证过
- `run-resolve` 能解析目标 plan
- `run-execute` minimal 3-point 通过
- `artifact-check` 通过
- `audit-continuity` 返回 `continuous`

在这之前，不要急着改 point loop 或 collector runtime。
