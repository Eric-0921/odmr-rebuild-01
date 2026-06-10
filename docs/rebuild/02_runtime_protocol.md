# Runtime 协议

本文定义第一版 runtime 的生命周期和状态约束。重点是保证采集流连续、point 语义清楚、失败可追溯。

第一版 runtime 不是“所有设备都可调”，而是“磁场点 + SMB sweep 变量，其余设备按 profile 固定”。

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

- acquisition window 默认值
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

- 固定节拍发送 `RALL?`。
- 读取完整 frame。
- 打 monotonic timestamp 和 wall timestamp。
- 分配连续 frame sequence。
- 写入 raw log。
- 更新 ring buffer。
- 报告 timeout、parse error、duplicate、last frame age。
- 提供基于时间戳和 raw offset 的 point 窗口拉取基础。

约束：

- 同一 OE1022D 串口只能有一个 reader。
- producer 不使用 `try_send` 静默丢帧作为主链策略。
- raw writer / health consumer 必须持续 drain。
- stop 必须包含 request、observed、port close、thread joined 四个阶段。
- `Drop` 不等于线程已经退出。
- 最小 `RALL` 字段解析不在 collector 线程执行，而在 point 从 ring buffer 拉窗口之后执行。
- ring buffer 只是即时消费层，不是最终事实层。
- ring buffer 容量必须覆盖最大 point acquisition window 再加一段 guard margin。
- 如果做不到上述容量约束，就不能声称支持“point 主动拉窗口”。

## Point 执行协议

每个 point 的执行顺序：

1. 写 `point_prepare_started`。
2. 将 `target_b_nt` 通过 calibration 转换为三轴电流。
3. 设置三台 M8812 目标电流。
4. 等待 M8812 readback 或 settle policy 达标。
5. 配置 SMB100A sweep 和功率。
6. readback 校验 SMB100A 关键参数。
7. 写 `point_stable`。
8. 记录 segment start timestamp 和 raw offset。
9. 执行 acquisition window 或 sweep。
10. 记录 segment end timestamp 和 raw offset。
11. 从 ring buffer 按 `[start_ts, end_ts]` 主动拉取 point 时间窗。
12. 把窗口内每帧解析成 `20 x 50` double matrix，并抽出当前关心字段。
13. 计算 quality 和即时摘要。
14. 写 `point_completed` 或 `point_failed`。

point 失败不必默认终止整个 run。是否继续由 plan 中的 failure policy 决定。但 point 失败必须显式进入 artifact。

旧讨论中的 “step” 在第一版 runtime 中只作为 point 执行阶段的兼容说法，不是独立采集生命周期对象。

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
- run 中持续执行 `RALL?`。
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
