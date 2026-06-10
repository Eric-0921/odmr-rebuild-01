# 真机验证命令与运行时基线

本文是 rebuild 第一轮真机 bring-up 与 `hardware smoke` 的单一事实入口。它只回答三件事：

- 这次 rebuild 已经真机验证过哪些命令
- 这次 rebuild 已经真机验证过哪些链路
- 下一阶段 runtime 应该直接继承哪些运行时边界

## 产物来源

本次基线来自以下真机产物：

- `out/station_snapshot_live.json`
- `out/hardware_verify_mag_lock/manual_live/mag_verify_summary.json`
- `out/hardware_verify_mag_lock/manual_live/mag_zero_lock_snapshot.json`
- `out/hardware_verify_mag_lock/manual_live/mag_verify_points.jsonl`
- `out/hardware_smoke/manual_scan/station_snapshot.json`
- `out/hardware_smoke/manual_scan/hardware_smoke_events.jsonl`
- `out/hardware_smoke/manual_scan/hardware_smoke_command_audit.jsonl`
- `runs/grid_2d_raster_small_live/summary.json`
- `runs/grid_2d_raster_small_live/points.jsonl`
- `runs/grid_2d_raster_small_live/segments.jsonl`
- `runs/grid_2d_raster_small_live/quality.jsonl`
- `runs/grid_2d_raster_small_live/point_fields.jsonl`
- `runs/grid_2d_raster_small_live/events.jsonl`
- `runs/manual_live_v4/summary.json`
- `runs/manual_live_v5/summary.json`
- `runs/manual_live_v5/points.jsonl`
- `runs/manual_live_v5/quality.jsonl`
- `runs/manual_live_v5/point_fields.jsonl`
- `runs/manual_live_v5/events.jsonl`
- `runs/x_axis_1d_bounce_15min_live/summary.json`
- `runs/x_axis_1d_bounce_15min_live/events.jsonl`
- `runs/x_axis_1d_bounce_15min_live/quality.jsonl`

文档整理日期：

- 真机 smoke 成功时间：`2026-06-10`
- 最小 runtime 真机完成时间：`2026-06-11`
- 零偏锁定单独验证时间：`2026-06-11`
- 2D 小网格真机完成时间：`2026-06-11`
- 15 分钟 1D 长跑完成时间：`2026-06-11`
- 文档收口时间：`2026-06-11`

## 新增真机校正事实（2026-06-11）

在厂商 LabVIEW 软件完成 OE1022D 通道 B PLL 配置并锁定之后，rebuild 侧只读快照确认：

- `FMODD? 2 = 0`
- `RSLPD? 2 = 2`
- `FREQD? 2 = 4.99999e+02`
- `*PLLD? 2 = 1`

对应产物：

- `out/hardware_state_snapshot/manual_after_labview/hardware_state_snapshot.json`

这条事实的含义是：

- 当前 `V1.5` 厂商 PDF 对 `RSLPD` 的 `0/1` 枚举说明不足以覆盖真实设备行为
- rebuild 中凡是把 `RSLPD` 固定写成 `0/1` 的地方，都只能视为旧假设，不能再当成真值
- 下一阶段实现必须允许“手册原文”与“真机观测”并存，直到完成受控 write-back 试验

## 验证状态规则

本文与命令规格文档统一使用以下状态：

- `rebuild_smoke_verified`
  - 已在本次 rebuild 真机 `station verify` 或 `hardware smoke` 中打通过
- `legacy_verified_not_rechecked`
  - 旧项目 / bring-up / 旧真机链路中验证过，但本次 rebuild 尚未重新真机打通
- `allowed_not_yet_verified`
  - 当前白名单允许进入后续实现，但本次 rebuild 尚未真机验证

## station verify 已验证命令

### SMB100A

- `*IDN?`
- 状态：`rebuild_smoke_verified`
- 观察到的响应示例：
  - `Rohde&Schwarz,SMB100A,1406.6000k02/101623,3.1.19.15-3.20.390.24`

### OE1022D

- `*IDN?`
- 状态：`rebuild_smoke_verified`
- 观察到的响应示例：
  - `SSI LIA-OE1022D,SN:D6522078,Version:Ver6.3200831`

### M8812

- `*IDN?`
- 状态：`rebuild_smoke_verified`
- 观察到的响应示例：
  - `MAYNUO,M8812,080020960220402020,V2.7`
  - `MAYNUO,M8812,080020960220402022,V2.7`
  - `MAYNUO,M8812,080020960220402003,V2.7`

### CNI Laser

- `output_off`
- `echo readback`
- 状态：`rebuild_smoke_verified`
- 观察到的响应示例：
  - `55 AA 03 00 03`

## hardware smoke 已验证命令

### SMB100A

- `*IDN?`
- `SYST:ERR?`
- `OUTP?`
- `FREQ?`
- `POW?`
- `SWE:FREQ:STEP?`
- `SWE:FREQ:DWEL?`
- 状态：以上全部 `rebuild_smoke_verified`

### M8812

逐轴都已真机打通以下序列：

- `*IDN?`
- `SYST:REM`
- `CURR 0.00000`
- `OUTP 0`
- `MEAS:CURR?`
- `CURR 0.01000`
- `OUTP 1`
- `MEAS:CURR?`
- `OUTP 0`
- `CURR 0.00000`
- `SYST:LOC`
- 状态：以上全部 `rebuild_smoke_verified`

观察到的 10mA 微测回读：

- `mag_x -> 0.01003 A`
- `mag_y -> 0.01006 A`
- `mag_z -> 0.00998 A`

### OE1022D

- `*IDN?`
- `clear_input`
- `RALL?`
- 状态：
  - `*IDN?` -> `rebuild_smoke_verified`
  - `RALL?` -> `rebuild_smoke_verified`
  - `clear_input` 是 transport 行为，不属于命令白名单，但属于本次真机已验证连接步骤

观察到的单帧事实：

- `payload_len = 15168`
- `head = 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00`

这里必须加一句纠偏：

- `15168` 是早期 smoke 诊断路径里 `until_timeout` 风格读取观测到的长度
- 当前 runtime collector 和最小 parser 已经用定长 `12288 bytes` 真机打通
- 第一版 runtime 协议真值应以 `12288` 为准，不应把 `15168` 当作 `RALL?` 帧规范

### CNI Laser

- `55 AA 03 00 03`
- echo `55 AA 03 00 03`
- 状态：`rebuild_smoke_verified`

## 已验证链路

- `station verify`
- `hardware smoke`
- `hardware verify-mag-lock`
- `RF + Mag + OE` 核心链路
- `Laser OFF` 背景控制链路
- `Laser ON background` runtime 链路
- 串口 hint 失败后自动扫描、probe、认领链路
- `run execute` 最小 3-point runtime
- `run execute` 2D 小网格 runtime
- `Maynuo baseline lock once -> OE run 级 collector -> point window segmentation`
- `target_b_nt -> calibration -> target_current_a`
- point 级最小 `RALL` 字段解析与 `point_fields.jsonl` 落盘

## 零偏锁定单独验证结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- hardware verify-mag-lock --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/mag_zero_lock_verify.json --out-dir out/hardware_verify_mag_lock/manual_live`

运行结果：

- `summary.status = completed`
- `points_passed = 4 / 4`
- `baseline_current_a = [0.00007, 0.00009, 0.00010] A`
- `max_abs_error_a <= 0.00012 A`

这说明当前 rebuild 已经真机证明：

- “零场锁定”在第一版里的真实语义是零偏电流锁定
- 后续 point 电流采用 `locked_zero_offset_current_a + delta_current_a`
- 三轴非负目标电流在当前容差内可复现

## 最小 runtime 真机结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_pll_default.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/manual_live_v5`

运行结果：

- `summary.status = completed`
- `points_passed = 3 / 3`
- `frames_total = 677`
- point 级 frame 数：
  - `p0001 = 104`
  - `p0002 = 104`
  - `p0003 = 105`
- 所有 point 的 `b_pll_locked_ratio = 1.0`
- 所有 point 的 `last_b_ref_source_code = 0`
- 所有 point 的 `last_b_ref_slope_code = 2`
- `B-Freq` point 均值约 `499.999 Hz`

run 结束后额外只读核验：

- `OUTP? -> 0`
- `FREQ:MODE? -> CW`
- `SYST:ERR? -> 0,"No error"`

这说明当前 runtime 已经真机证明：

- point 内 RF 输出保持开启可工作
- cleanup 后 RF output 已关闭
- cleanup 后 RF frequency sweep 状态已退出到 `CW`
- 最小 `RALL` parser 能在真实 point 窗口上稳定产出字段级数据

## 2D 小网格真机结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/grid_2d_raster_small.json --smb-profile configs/profiles/smb100a_run_short_sweep_15min.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/grid_2d_raster_small_live`

运行结果：

- `summary.status = completed`
- `points_passed = 9 / 9`
- `points / segments / point_fields / quality = 9 / 9 / 9 / 9`
- `summary.frames_total = 1666`
- `quality.frames_total_sum = 264`
- `RALL` 实测点速率约 `1030.5 ~ 1065.9 pts/s`，平均 `1042.5 pts/s`
- `Laser` 在整个 run 期间保持 `on_background`

这说明当前 runtime 已经真机证明：

- 正向 2D 小网格 point 规划可展开并执行
- collector 单实例和 point segmentation 在小网格下稳定
- 复跑同一个 `--out-dir` 后，artifact 会被正确重建，不再静默 append 旧内容

## 15 分钟 1D 长跑结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_short_sweep_15min.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/x_axis_1d_bounce_15min_live`

运行结果：

- `summary.status = completed_with_failed_points`
- `points_passed = 103 / 104`
- `points_failed = 1 / 104`
- `summary.frames_total = 18725`
- `quality_status` 统计：
  - `passed = 129`
  - `failed_min_frames = 1`
- 明确坏点：
  - `p000060 -> frames_total = 12`
- collector 最终 `timeout_count = 4`
- collector 稳态点速率约 `~1015 pts/s`

必须明确的校正说明：

- 这次长跑是在发现“当前磁场电源第一版不支持负输出”之前启动的
- 因此它主要用于验证 `collector / sweep / segmentation / cleanup` 的长时稳定性
- 不应用它来宣称“负磁场点的电流映射已经有效”
- 同时，这个目录里的 JSONL 曾受旧版 append 语义污染；当前代码已修复，但该目录不再作为“文件条数真值”来源

## 明确未验证内容

以下内容当前不得写成 `rebuild_smoke_verified`：

- “非负网格条件下”的 15 分钟以上 run 级 collector
- `OE1022D` 30 分钟以上 run 级 collector
- 更长时 point segmentation 稳定性
- point 级 `Laser` 变量
- point 级 `OE1022D` 配置变更
- 3D 真机网格运行

## 运行时基线

下一阶段 runtime 必须直接继承以下结论：

- 第一版 runtime 不是“所有设备都可调”，而是“磁场点 + SMB sweep 变量，其余设备按 profile 固定”
- `OE1022D` 不是 point 级设备，而是 run 级固定观测器
- `RALL?` 必须由单一 run 级 reader 线程持续执行
- point 线程不直接碰 OE 串口；point 完整帧序列按 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl` 回切恢复
- ring buffer 只保留最近窗口观察和 collector 健康摘要，不再决定 point 完整性
- `continuous raw + frame index + segment boundary` 才是最终事实来源

## 当前参数归属结论

### point / run 允许变化

- `target_b_nt`
- `SMB100A start/stop/step/dwell/power`

### station / profile 固定

- `OE1022D` 全部 setup 配置
- `M8812` 电压、保护阈值、DTR、cleanup 顺序
- `Laser` 默认功率、默认开关策略、emergency off

### run 固定

- acquisition window 默认值
- settle policy
- failure policy
- `SMB100A` 默认 sweep mode / trigger mode
