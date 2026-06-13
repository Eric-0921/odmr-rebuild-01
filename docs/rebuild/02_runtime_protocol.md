# Runtime 协议

本文定义第一版 runtime 的生命周期和状态约束。重点是保证采集流连续、point 语义清楚、失败可追溯。

第一版 runtime 不是“所有设备都可调”，而是“磁场点 + SMB sweep 变量，其余设备按 profile 固定”。
当前“零场锁定”语义是旧系统兼容的零偏电流锁定，不是物理零磁场已证明。

## Runtime 状态

```text
Created
StationResolved
PreflightPassed
RunOpened
CollectorRunning
PointRunning
Stopping
Completed
Failed
CleanupFailed
```

合法迁移：

- `Created -> StationResolved`
- `StationResolved -> PreflightPassed`
- `PreflightPassed -> RunOpened`
- `RunOpened -> CollectorRunning`
- `CollectorRunning -> PointRunning`
- `PointRunning -> CollectorRunning`
- `CollectorRunning -> Stopping`
- `Stopping -> Completed`
- 任意非终态 -> `Failed`
- `Failed -> Stopping`
- `Stopping -> CleanupFailed`

非法行为：

- 未完成 station resolve 就打开设备。
- 未通过 preflight 就启动 run。
- 未启动 collector 就开始 point。
- point 切换时重启 OE1022D collector。
- cleanup 前直接丢弃设备 session。

## 参数归属矩阵

### station / profile fixed

- `SMB100A`
  - 连接参数
  - 默认安全状态
  - modulation / list mode 禁用策略
  - trigger 默认策略
- `OE1022D`
  - 全部 setup 配置
- `M8812`
  - 电压
  - 保护阈值
  - `remote/local`
  - `DTR`
  - cleanup 顺序
- `Laser`
  - 默认功率
  - 默认开关策略
  - emergency off 规则

### run fixed

- settle policy
- failure policy
- `SMB100A` 默认 sweep mode / trigger mode

### point / run variable

- `target_b_nt`
- `SMB100A start/stop/step/dwell/power`

### 第一版明确不允许 point 级变化

- point 级 `OE1022D` 参数
- point 级 `Laser` 参数
- point 级 `M8812` 电压 / 保护参数

## Collector 协议

OE1022D collector 是 run 级单实例。

它只在 `run execute` 进入 run 生命周期后启动，不在“station verify / 仅连接设备”阶段提前常驻。

职责：

- 按 LabVIEW-like tight loop 持续执行定长 `RALL?`：写 `RALL?`，等待 `30ms`，blocking exact read `12288B`，读完立即进入下一轮。
- 读取完整 frame。
- 打 monotonic timestamp 和 wall timestamp。
- 分配连续 frame sequence。
- 写入 raw log。
- 写入 `raw/oe1022d.frames.idx.jsonl`。
- 更新 ring buffer。
- 报告 timeout、parse error、duplicate、last frame age。
- 提供 committed cursor，作为 point durable 边界记录基础。

约束：

- 同一 OE1022D 串口只能有一个 reader。
- OE1022D collector 只在打开串口后清一次输入缓冲区；热循环内不逐帧清输入。
- `RALL` 设备采样间隔按 `1ms/sample` 处理；host poll interval 不反推出采样点间隔。
- 定长 `RALL?` 热路径不使用 `poll_interval_ms` 做额外 sleep，不使用 first-byte deadline、frame deadline、zero-byte retry 或 timeout 后 clear/retry。
- `payload[12287]` 当前作为 `device_packet_counter` 进入 frame index 和 continuity audit：`delta=1` 是新窗口，`delta=0` 是重复窗口，`delta>1` 是疑似漏 50ms 窗口。
- producer 不使用 `try_send` 静默丢帧作为主链策略。
- raw writer / health consumer 必须持续 drain。
- stop 必须包含 request、observed、port close、thread joined 四个阶段。
- `Drop` 不等于线程已经退出。
- 最小 `RALL` 字段解析不在 collector 线程执行，而在 point 从 `raw + frames.idx + segments` 回切之后执行。
- ring buffer 只是即时消费层，不是最终事实层。
- ring buffer 容量仍按估算值加 guard 动态规划，但只服务观察体验，不再承担 point 保真。
- point 真值边界必须取 committed cursor，不能取 ring cursor。

## Point 执行协议

每个 point 的执行顺序：

1. 写 `point_prepare_started`。
2. 将 `target_b_nt` 通过 calibration 转换为 `delta_current_a`。
3. 用 `locked_zero_offset_current_a + delta_current_a` 计算三台 M8812 目标电流。
4. 等待 M8812 readback 或 settle policy 达标。
5. 配置 SMB100A sweep 和功率。
6. readback 校验 `OUTP ON` 与 `FREQ:MODE SWE` 等关键参数。
7. 写 `point_stable`。
8. 记录 segment start timestamp 和 raw offset。
9. 发送 `SWE:FREQ:EXEC`。
10. 先观察 `*OPC?` 的实际等待时长；若明显早于基于 `start/stop/step/dwell` 的 sweep 估算时长，则退回到“估算时长 + guard”。
11. 记录 segment end timestamp 和 raw offset。
12. 先把 `segment_start/end + frame_seq/raw_offset` 写入 `segments.jsonl`。
13. 再按 committed `frame_seq/raw_offset` 从 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl` 回切 point 帧序列。
14. 把窗口内每帧解析成 `20 x 50` double matrix，并抽出当前关心字段。
15. 计算 quality 和即时摘要。
16. 写 `point_completed` 或 `point_failed`。

point 失败不必默认终止整个 run。是否继续由 plan 中的 failure policy 决定。但 point 失败必须显式进入 artifact。

旧讨论中的 “step” 在第一版 runtime 中只作为 point 执行阶段的兼容说法，不是独立采集生命周期对象。

硬规则：

- `1 point = 1 SMB100A frequency sweep`
- `acquisition_window_ms` 只保留给旧样例兼容，不再作为 sweep-driven point 的真值边界
- point 真实边界必须由 `sweep_started -> sweep_completed` 定义
- 当前实验室真机上，`*OPC?` 可能在数毫秒内返回，不能单独当作 sweep 结束信号
- `fixed_total_points` 的估时必须包含每 point 的 SMB 重配置 settle 开销，不能只算 sweep 本体时长
- 当前磁场电源第一版只支持非负目标电流；如果 `locked_zero_offset_current_a + delta_current_a` 算出负值，runtime 必须直接报错

## Settle 协议

磁场 settle 不是固定 sleep 的别名。

第一版允许两种策略：

- `readback_window`：连续 N 次 readback 落在容差内。
- `fixed_delay_with_readback`：等待固定时间后至少做一次 readback 记录。

每个 point 至少记录：

- `settle_started_at`
- `settled_at`
- `target_b_nt`
- `target_current_a`
- `measured_current_a`
- `settle_policy`
- `settle_status`

没有 settle 记录的 point 不允许标记为有效。

## SMB100A 协议

第一版只允许 RF sweep 核心路径：

- 查询身份。
- 查询错误。
- 设置功率。
- 设置 sweep start/stop/step/dwell。
- 设置 sweep trigger source。
- 打开/关闭 RF output。
- 执行 frequency sweep。
- 必要 readback。

其中 point / run 变量只允许：

- `start`
- `stop`
- `step`
- `dwell`
- `power`

禁止：

- 运行时直接发送未列入命令白名单的 SCPI。
- 为了“手册可能支持”而发送未经验证命令。
- 在 error queue 非空时继续把 point 标为成功。
- 在 point 内关闭 RF output 但继续把同一个 sweep 视为有效采集窗口。

## M8812 协议

第一版 M8812 在 runtime 中只作为磁场执行器。

允许：

- 查询身份。
- remote/local。
- 设置电流。
- 设置输出开关。
- 查询测量电流。
- 查询错误。

不允许 point 直接表达通用电源参数全集。电压、保护阈值、输出策略属于 station/profile。

## OE1022D 协议

第一版 OE1022D 是固定观测器。

允许：

- run 开始前执行固定配置。
- run 中由单一 collector 按 LabVIEW-like exact-read 循环持续执行 `RALL?`。
- 记录 collector health。
- 为 point 提供按时间窗拉取的 ring buffer 只读接口。

不允许：

- point 级修改 OE 配置。
- point 级重启 OE collector。
- viewer 或 analysis 直接读取 OE 串口。

## CNI Laser 协议

第一版激光器默认作为 run profile 固定背景条件。

允许：

- 设置功率。
- 开启输出。
- 关闭输出。
- emergency off。

point 级 laser 变量后置，除非当前科学目标明确要求扫描 laser。

当前推荐主路径是 `on_background`：

- run 打开阶段设功率并开启输出
- point 期间保持不变
- run 结束统一关闭

## Run 结束协议

正常结束：

1. 停 SMB100A RF output。
2. 将 M8812 输出降到 cleanup policy 规定状态。
3. 关闭或保持 laser 到 profile 规定状态。
4. request collector stop。
5. 等待 collector observed stop。
6. close port。
7. join collector thread。
8. flush artifact。
9. 写 run summary。

异常结束：

- 写 failure event。
- 尽力执行 cleanup。
- cleanup 的每一步都要记录结果。
- artifact flush 必须发生在进程退出前。
