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
- `runs/verify_20260611_short_readchain/summary.json`
- `runs/verify_20260611_short_readchain/quality.jsonl`
- `runs/verify_20260611_short_readchain/raw/oe1022d.frames.idx.jsonl`
- `runs/verify_20260611_short_readchain/raw/oe1022d.frames.parsed.jsonl`
- `runs/verify_20260611_long_readchain/summary.json`
- `runs/verify_20260611_long_readchain/events.jsonl`
- `runs/verify_20260611_long_readchain/quality.jsonl`
- `runs/verify_20260611_long_readchain/raw/oe1022d.frames.idx.jsonl`
- `runs/verify_20260611_long_readchain/raw/oe1022d.frames.parsed.jsonl`
- `runs/grid_3d_raster_0_50_100ut_validation_live/summary.json`
- `runs/grid_3d_raster_0_50_100ut_validation_live/points.jsonl`
- `runs/grid_3d_raster_0_50_100ut_validation_live/segments.jsonl`
- `runs/grid_3d_raster_0_50_100ut_validation_live/quality.jsonl`
- `runs/grid_3d_raster_0_50_100ut_validation_live/point_fields.jsonl`
- `runs/grid_3d_raster_0_50_100ut_validation_live/point_fields/seg_p000001_0000.npz`
- `runs/grid_3d_raster_0_50_100ut_validation_live/continuity_audit.json`

文档整理日期：

- 真机 smoke 成功时间：`2026-06-10`
- 最小 runtime 真机完成时间：`2026-06-11`
- 零偏锁定单独验证时间：`2026-06-11`
- 2D 小网格真机完成时间：`2026-06-11`
- 15 分钟 1D 长跑完成时间：`2026-06-11`
- 文档收口时间：`2026-06-11`
- 读取链与表格解析短/长验证时间：`2026-06-11`
- 3D 单轮真机验证时间：`2026-06-12`

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

## 读取链短验证结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_tc100ms.json --laser-profile configs/profiles/cni_laser_run_on_background_180mw.json --out-dir runs/verify_20260611_short_readchain`

运行结果：

- `summary.status = completed`
- `points_passed = 3 / 3`
- `summary.frames_total = 2821`
- `points / segments / point_fields / quality = 3 / 3 / 3 / 3`
- `raw/oe1022d.frames.idx.jsonl` 与 `raw/oe1022d.frames.parsed.jsonl` 条数一致：
  - `2821 / 2821`
- `frames.parsed` 的 `parse_status` 全部为 `ok`
- collector 最终 `timeout_count = 1`
- 3 个 point 的 `frames_total = 758 / 759 / 758`

必须明确的观察：

- 这 1 次 timeout 发生在 point 窗口之外，所以 `quality.jsonl` 里的 point 级 `timeout_count` 仍然是 `0`
- 这说明当前 `quality.timeout_count` 只覆盖 point 窗口内 timeout，不等于 run 级 collector 总 timeout
- 当前短 run 已经真机证明：
  - `0-byte timeout` 仍会出现
  - 但新 collector 能恢复，不会立刻把整次 run 打崩
  - `raw / frames.idx / frames.parsed` 三层真值已经能保持对齐

## 15 分钟 1D 长跑结果（2026-06-11）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_tc100ms.json --laser-profile configs/profiles/cni_laser_run_on_background_180mw.json --out-dir runs/verify_20260611_long_readchain`

运行结果：

- `summary.status = completed`
- `points_passed = 21 / 21`
- `summary.frames_total = 19142`
- `points / segments / point_fields / quality = 21 / 21 / 21 / 21`
- `raw/oe1022d.frames.idx.jsonl` 与 `raw/oe1022d.frames.parsed.jsonl` 条数一致：
  - `19142 / 19142`
- `frames.parsed` 的 `parse_status` 全部为 `ok`
- collector 最终 `timeout_count = 13`
- point 窗口内累计 `timeout_count = 10`
- 最差 point：
  - `p000016 -> frames_total = 717`
  - `frame_coverage_ratio = 0.947`
  - `timeout_count = 2`
- 多个 point 在 timeout 后从常态 `758/759` 帧下降到：
  - `737 / 738 / 717`
- collector 稳态点速率约 `~1000 pts/s`

必须明确的校正说明：

- 这次长跑的 point 全部通过，不代表读链没有问题
- 当前真正的结论是：
  - collector 级 `0-byte timeout` 会低频重复出现
  - 新 collector 能恢复并继续跑完
  - 但 timeout 会真实吞掉 point 帧数
  - 当前 `quality` 门槛仍偏宽，尚不足以把这类“吞帧但未跌破 min_frames”的点判成失败
- 因此现在不能再把“全部 passed”误读成“采集链完全稳定”
- 同时，这次长跑已经足够排除一个旧怀疑：
  - 当前 point 级掉帧主要不是 ring buffer 截断
  - 因为 `raw / frames.idx / frames.parsed` 已保持条数对齐
- 坏点根因已收敛到真实 collector / transport 抖动

## 3D `0/50/100 uT` 单轮验证结果（2026-06-12）

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/grid_3d_raster_0_50_100ut_validation.json --smb-profile configs/profiles/smb100a_run_3d_validation_fast.json --oe-profile configs/profiles/oe1022d_run_3d_validation_fast.json --laser-profile configs/profiles/cni_laser_run_on_background.json --out-dir runs/grid_3d_raster_0_50_100ut_validation_live`

运行结果：

- `summary.status = completed`
- `points_passed = 27 / 27`
- `points / segments / point_fields / quality = 27 / 27 / 27 / 27`
- `summary.frames_total = 3290`
- `quality.timeout_count = 0`（全部 point）
- `quality.collector_health = clean`（全部 point）
- `collector_stopped.timeout_count = 0`
- continuity audit 结果：
  - `verdict = continuous`
  - `suspected_missing_boundaries = 0`
  - `max_observed_gap_ms = 99.19`
- 默认轻量 artifact 已真机验证：
  - `raw/oe1022d.frames.parsed.jsonl` 默认不落盘
  - `point_fields.jsonl` 改为 metadata
  - `point_fields/*.npz` 可作为完整 20 字段数组 sidecar

这说明当前 runtime 已经真机证明：

- 3D `x/y/z = [0, 50, 100] uT` 的 27 点笛卡尔积能正确展开并单轮执行
- `target_b_nt -> calibration -> target_current_a -> measured_current_a` 三轴电流链已在真实 run 中完整落盘
- 3D 快配 profile 下，run 级 collector 可在 27 点单轮中保持 `0-byte timeout = 0`
- 默认轻量 artifact 策略不会破坏 replay / continuity audit / point 级字段 sidecar

## 明确未验证内容

以下内容当前不得写成 `rebuild_smoke_verified`：

- “timeout 继续低频出现时”的更严格 quality 判定规则
- `OE1022D` 30 分钟以上 run 级 collector
- 更长时 point segmentation 稳定性
- point 级 `Laser` 变量
- point 级 `OE1022D` 配置变更
- 更长时的 3D 多轮稳定性

## 运行时基线

下一阶段 runtime 必须直接继承以下结论：

- 第一版 runtime 不是“所有设备都可调”，而是“磁场点 + SMB sweep 变量，其余设备按 profile 固定”
- `OE1022D` 不是 point 级设备，而是 run 级固定观测器
- `RALL?` 必须由单一 run 级 reader 线程持续执行
- point 线程不直接碰 OE 串口；point 完整帧序列按 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl` 回切恢复
- 默认不再同步落 `raw/oe1022d.frames.parsed.jsonl`；只有 debug 模式才写
- point 级完整 20 字段数组默认进入 `point_fields/*.npz`
- `point_fields.jsonl` 只保留 metadata、摘要统计和 sidecar 路径
- ring buffer 只保留最近窗口观察和 collector 健康摘要，不再决定 point 完整性
- `continuous raw + frame index + segment boundary` 才是最终事实来源
- `frames.parsed` 是 companion truth，不替代 raw

## 当前默认 observed profile 基线（2026-06-12）

当前默认真机基线是这组配置：

- `plan = configs/plans/minimal_3point_runtime.json`
- `smb-profile = configs/profiles/smb100a_run_pll_default.json`
- `oe-profile = configs/profiles/oe1022d_run_ch_b_observed.json`
- `laser-profile = configs/profiles/cni_laser_run_on_background.json`

当前默认 `OE1022D` 热路径参数：

- `ASCII query timeout = 300ms`
- `rall_post_write_delay = 30ms`
- `rall_chunk_timeout = 5ms`
- `rall_first_byte_deadline = 30ms`
- `rall_frame_deadline = 120ms`
- `zero_byte_retry_limit = 1`

原厂 LabVIEW `OE1022D_USB_Query Data.vi` 的 RALL 热路径是：

- 写 `RALL?`
- 等 `30ms`
- 固定读 `12288B`
- 转 `U8[]` 后交给 `OE1022D_DATA Transmit.vi`

因此 rebuild collector 只在打开串口后清一次输入缓冲区，不在每次 `RALL?` 读前清输入，避免丢弃已经到达的连续采集帧。

当前默认 `quality` / 诊断语义：

- `timeout_count > max_timeout_count` 才会判成 `failed_timeout`
- `quality.jsonl` 额外输出：
  - `collector_health = clean | recovered_timeout | degraded_timeout`
  - `timeout_budget_remaining`
- `events.jsonl` 额外记录：
  - `collector_timeout`
  - `collector_recovered`
  - `collector_stopped`

### 3-run 真机验收

真机运行目录：

- `runs/manual_live_recheck_20260612_d30_r1`
- `runs/manual_live_recheck_20260612_d30_r2`
- `runs/manual_live_recheck_20260612_d30_r3`

验收结果：

- 三轮都是 `points_passed = 3 / 3`
- 三轮都是 `collector_stopped.data.timeout_count = 0`
- 三轮 `quality.jsonl` 全 point：
  - `timeout_count = 0`
  - `collector_health = clean`
  - `quality_status = passed`
- `raw/oe1022d.frames.idx.jsonl`：
  - `max(frame_gap_ms)` 分别约为 `54.35 / 54.34 / 54.55`
  - 没有 `>=100ms` gap

结论：

- 当前默认 `observed` profile 已经满足最小 3-point runtime 的主线真机基线
- 旧的 `tc100ms / monitor` 组合问题仍保留为历史记录，但不能再当作当前默认配置的现状

## collector split + timeout300 历史复验（2026-06-12，旧 tc100ms / monitor 组合）

这轮改动固定为：

- `OE1022D serial timeout = 300ms`
- collector 改成 `producer(只负责 RALL?) + consumer(负责 parse/write/ring/commit)` 双线程
- point 级质量规则改成：
  - 旧实现阶段一度按 `point 内任一 timeout -> failed_timeout` 收紧；该结论已被后续 `max_timeout_count` 语义取代

### 3-point 短验证

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_tc100ms.json --laser-profile configs/profiles/cni_laser_run_on_background_180mw.json --out-dir runs/verify_20260612_collector_split_short_v3`

运行结果：

- 无 timeout point：
  - `frames_total ≈ 725 / 726`
  - `quality_status = passed`
- timeout point：
  - `frames_total ≈ 714 / 720`
  - `quality_status = failed_timeout`

必须明确的新校正：

- 当前 point 的“正常帧数”更接近 `725/726`，而不是先前用 `48ms` 直接估出来的 `757`
- 这和手册里 `RALL?` 每 `50ms` 更新一次的设备语义是一致的
- 因此 `frame_coverage_ratio` 目前仍只能作为诊断字段，不能直接拿现有 `757` 估值做硬 fail

### Ctrl-C cleanup 复验

真机运行命令：

- `cargo run -p odmr-cli -- run execute ... --out-dir runs/verify_20260612_interrupt_cleanup`
- 中途人工 `Ctrl-C`

运行结果：

- CLI 不再像之前那样直接退出
- 会进入 `cleanup 开始 -> cleanup 完成`
- 后续 `hardware smoke` 通过：
  - `runs/verify_20260612_interrupt_cleanup_post_smoke`

这说明当前“人工中断 -> cleanup”主路径已经比之前可靠。

### 15 分钟长跑复验

真机运行命令：

- `cargo run -p odmr-cli -- run execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/x_axis_1d_bounce_15min.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_tc100ms.json --laser-profile configs/profiles/cni_laser_run_on_background_180mw.json --out-dir runs/verify_20260612_collector_split_long`

运行中观察到：

- 前几个无 timeout point：
  - `frames_total = 725 / 726`
  - `quality_status = passed`
- timeout point：
  - `frames_total = 714 / 715`
  - `quality_status = failed_timeout`
- 但在后续 point 上出现新的更深层故障：
  - 连续 timeout streak 后 `collector_frames_total` 停止增长
  - 后续 point 直接出现 `frames_total = 0`
  - 人工中断后 cleanup 进入挂起态，需手动杀进程

因此这轮复验的结论不是“collector split 已完全修复”，而是：

- timeout 已经能被 point 级质量规则正确识别
- 真实无 timeout point 的帧数基线已重新校正
- 但长跑下仍存在“连续 timeout -> collector 卡死 -> cleanup 挂起”的未解决故障

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
