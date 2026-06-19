# 重建架构

第一版架构按作业生命周期组织，不按 GUI 页面或设备面板组织。

## 模块分层

```text
configs/
  stations/
  calibrations/
  plans/

tools/win-csharp/
  Odmr.Devices/
  Odmr.Runtime/
  Odmr.Artifacts/
  Odmr.WinProbe/

python/
  odmr_plan/
  odmr_calibration/
  odmr_analysis/

runs/
```

当前仓库主栈是 `tools/win-csharp`。runtime 只能通过 C# command catalog、设备 session 或后续审查过的命令模块生成设备命令，不允许业务层拼接未审查的裸命令字符串。

语言边界：

- C# 负责设备连接、transport、config resolve、runtime、artifact、离线审查。
- Python 负责 plan 生成、calibration 拟合、replay 后分析。
- Python 不直接连接设备，不参与实时采集主循环。

设备连接的一手事实见 [06_device_connection_facts.md](./06_device_connection_facts.md)。

## 核心对象

### Station

Station 描述“谁是谁”和“现在如何连接”。

职责：

- 记录设备永久身份规则。
- 记录 transport hint。
- 记录设备固定配置。
- 记录 cleanup policy。
- 为每次 run 生成 `station_snapshot.json`。

端口路径只能是 hint，不是身份。设备身份优先来自协议身份、USB serial、VID/PID、物理拓扑或人工 claim。

### Calibration

Calibration 描述目标磁场如何转换为设备设定值。

职责：

- 三轴 coil mapping。
- zero baseline。
- current limit。
- valid range。
- calibration version。
- last verified timestamp。

runtime 只使用 calibration snapshot，不在 run 中临时发明换算规则。

### Plan

Plan 描述本次 run 要测哪些 point。

职责：

- run metadata。
- RF sweep 默认参数。
- laser 默认参数。
- point list 或高层 `cartesian_grid`。
- quality thresholds。

第一版 plan 可以手写。高层 `cartesian_grid` 只负责在运行前展开成显式 points；runtime 本体不直接理解网格语义。plan generator 后置。

## 参数归属矩阵

第一版 runtime 不是“所有设备都可调”，而是“磁场点 + SMB sweep 变量，其余设备按 profile 固定”。

### station / profile fixed

- `SMB100A`
  - 连接参数
  - 默认安全状态
  - 禁用 modulation / list mode
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

### 第一版不允许 point 级变化

- point 级 `OE1022D` 参数
- point 级 `Laser` 参数
- point 级 `M8812` 电压 / 保护参数

### Runtime

Runtime 是核心执行层。

职责：

- resolve station。
- preflight。
- 打开 run directory。
- 按 `lockin_model` 启动 run 级 collector（OE1022D `collector_frames`，OE1300 `collector_blocks`）。
- 执行 point list。
- 等待磁场稳定。
- 创建 segment 边界。
- 写 artifact。
- 执行 cleanup。
- 关闭 collector 并 join 线程。

Runtime 不应该关心 GUI，也不应该把 viewer 作为主消费者。

### Artifact

Artifact 是 run 的事实记录。

职责：

- 保存输入 snapshot。
- 保存事件流。
- 保存 point/segment/quality。
- 保存 decoded collector truth。
- 支持 replay 和分析。

## 数据流

```text
lock-in collector
  -> OE1022D collector_frames.jsonl 或 OE1300 collector_blocks.jsonl
  -> unique-only parameter_values.csv
  -> unique-only sample_values.csv
  -> segment indexer
point decoded truth window
  -> segments.jsonl + sample_values.csv
  -> point quality evaluator
  -> postprocess / ML dataset builder
```

关键约束：

- 每个 lock-in 只有一个 producer 读取。
- collector 不知道当前 point，只持续执行本型号冻结的 `RALL?` 热路径。
- collector 直接写 decoded truth，不再把 raw/frames.idx 当正式事实层。
- segmenter 负责把 decoded collector 序号、sample index 和时间窗归属给 point。
- point 真值窗口来自 `segments.jsonl + sample_values.csv`，必要时结合型号对应 collector truth 审计。
- viewer 只能订阅 artifact tail 或 progress JSONL。
- decoded truth 是最终事实来源，不依赖 viewer 存活。

## 控制流

```text
station verify
  -> calibration load
  -> plan validate
  -> run open
  -> device preflight
  -> collector start
  -> point loop
  -> cleanup
  -> collector stop/join
  -> run summary
```

point loop 内部顺序：

1. 计算 point 对应的三轴电流。
2. 设置 M8812 输出目标。
3. 等待 settle/readback 达标。
4. 配置 SMB100A sweep。
5. 确认 `OUTP ON` 与 `FREQ:MODE SWE`。
6. 记录 segment 起点。
7. 发送 `SWE:FREQ:EXEC` 并用 `*OPC?` 等待单次 sweep 完成。
8. 记录 segment 终点。
9. 先写 `segments.jsonl`，再按 decoded collector 序号和 sample index 绑定 point 时间窗。
10. 计算 point quality 和 point 摘要。
11. 记录 `point_completed` 或 `point_failed`。

## CLI 边界

```bash
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --station configs/stations/lab_a.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/test.json --smb-profile configs/profiles/smb100a_run_pll_default.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/test.json --smb-profile configs/profiles/smb100a_run_pll_default.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/<run_id>
dotnet run --project tools/win-csharp/Odmr.WinProbe -- live-replay --run runs/<run_id>
```

`run-execute` 必须依赖当次 station/profile resolve 结果。设备解析不唯一时必须失败，不允许猜。串口/VISA/TCP 设备都按 station hint 排序，再用协议身份认领。

## 失败模型

失败不是单一字符串。每个失败至少要记录：

- `phase`
- `device`
- `operation`
- `command` 或 action id
- `observed_response`
- `timestamp`
- `run_state`
- `cleanup_attempted`
- `cleanup_result`

失败后不允许把 run 标成成功。run 可以是 `completed_with_failed_points`，但不能把空采 point 混入成功数据。
