# 重建范围

本文定义 ODMR 重建第一版的边界。第一版目标不是通用平台，也不是 GUI 产品，而是一条可验证、可追溯、可长期运行的 CLI 实验 runtime 主链。

## 一句话目标

构建一个以磁场点标定和点级数据采集为核心的最小实验 runtime：

- 系统能确认设备身份。
- 系统能加载 station、calibration、plan。
- 系统能启动 run 级连续 OE1022D 采集流。
- 系统能按 point 切换磁场和 RF sweep 条件。
- 系统能把连续数据流按 segment 归属到 point。
- 系统能生成可回放、可审计的 artifact。
- 系统能在失败时留下足够事实，而不是只报告流程失败。

## 第一版支持能力

- 一个 OE1022D 作为固定观测器。
- 一个 SMB100A 作为 RF sweep 源。
- 三台 M8812 作为 X/Y/Z 磁场执行器。
- 一个 CNI Laser PSU-SR 作为固定或少量可控的激光源。
- 一种点扫描：读取 plan 中的 point 列表，逐点执行。
- 一个 run 级 OE1022D collector，贯穿整个 run。
- 连续 raw 落盘，加 point/segment 索引。
- CLI 执行、CLI 监控、CLI replay 的最小接口。

## 第一版明确不做

- 不做 GUI。
- 不做通用实验平台。
- 不做复杂 plan 模板系统。
- 不做动态设备参数全集编辑器。
- 不做多 OE、多 SMB、多 station 编排。
- 不做 Python 实时采集主循环。
- 不把每个 point 实现成一次独立 collector 生命周期。
- 不把手册中所有命令都封装进 runtime。

## 核心原则

### Run 是生命周期对象

整个实验 run 负责打开设备、启动采集、执行点表、cleanup 和关闭采集。point 数量再多，也不改变 run 级生命周期。

### Point 是实验语义对象

point 只表达本次科学问题真正变化的变量。第一版 point 中允许的变量是：

- `point_id`
- `target_b_nt` 或三轴目标电流
- SMB100A sweep 核心参数
- point 采集窗口和质量阈值的少量覆盖

### Segment 是运行时归属对象

segment 表示连续 OE1022D 数据流中的一个时间窗。每个 segment 归属到一个 point，并记录 raw offset、时间戳、帧数、错误统计和质量状态。

### Artifact 是事实来源

GUI、后处理、分析脚本都只能消费 artifact。artifact 不为界面服务，而为审计、回放、分析服务。

## 语言边界

### C# 负责

- 设备命令 helper。
- transport 和 session。
- device probes。
- JSON config resolve。
- run 级 collector。
- point 执行与 segment 标注。
- raw 和 jsonl artifact 写入。
- cleanup、stop、failure 记录。
- artifact-check 和 audit-continuity。

### Python 负责

- plan 生成。
- 磁场矩阵展开。
- calibration 拟合。
- 离线 replay 和分析。
- 图表、报告、导出。

### Python 不负责

- 实时采集主循环。
- 串口 collector。
- 设备 stop/cleanup 的最后保障。
- 多设备运行时一致性协调。

## 第一阶段交付

第一阶段结束时，仓库至少应具备：

- `dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list`
- `dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve ...`
- `dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute ...`
- `dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run <run_dir>`
- `dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run <run_dir> --out <json>`

第一阶段成功标准不是“流程跑完”，而是 artifact 能证明每个 point 是否采到有效数据，以及失败发生在哪里。
