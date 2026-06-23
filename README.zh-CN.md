# CLI-ODMR：面向智能体的 ODMR 采集框架

[English](README.md)

CLI-ODMR 是一个面向 NV 色心 ODMR 实验的可复现命令行采集框架。  
它不再把一次实验看成“人工盯着仪器做一遍”的流程，而是把实验抽象成：

- `Point`：一个完整实验配置
- `Run`：一个可重复执行的采集任务

本项目当前目标不是恢复一个庞大的旧 GUI，而是先建立一条经过验证、可追溯、可长期运行的实验主链，并为每次运行留下可审计的 artifact。

## 项目要解决什么问题

传统 ODMR 采集往往依赖人工调参、临场等待和零散记录，这会直接带来：

- 实验复现困难
- 长时间运行成本高
- 数据集标准化不足
- 后续分析缺少可信 provenance

CLI-ODMR 的核心思路是把这件事收敛成：

- 标准化的 `Point + Run` 执行模型
- 统一的仪器命令行入口
- 可由 AI / Agent 辅助准备参数
- 面向回放、审查、分析的数据产物

## 工作流程转变

![从手动 ODMR 采集到面向智能体的 CLI-ODMR](docs/assets/readme/manual-vs-agent-cli-odmr.png)

## 技术架构

![CLI-ODMR 技术架构图](docs/assets/readme/cli-odmr-technical-architecture.png)

## 当前项目能力

- 用 6 个 JSON 输入组合一次完整实验：`station`、`calibration`、`plan`、`smb-profile`、`oe-profile`、`laser-profile`
- 在 Windows 真机上执行可重复的 ODMR run
- 把锁相采集保持为贯穿整个 run 的连续数据流
- 将连续采集结果重新归属到 point 级实验语义
- 输出结构化 artifact，包括谱线、参数、元数据、事件和审计结果
- 通过 `artifact-check` 与 `audit-continuity` 做离线质量核验

## 当前主栈

正式采集主栈是 Windows C# / .NET 8：

- `tools/win-csharp/Odmr.Devices`：设备 transport 与命令 helper
- `tools/win-csharp/Odmr.Runtime`：配置解析、runtime、point loop
- `tools/win-csharp/Odmr.Artifacts`：artifact 写入、合同检查、连续性审计
- `tools/win-csharp/Odmr.WinProbe`：CLI 主入口

Python 不进入实时采集热路径，主要负责：

- 配置生成
- 控制台 / UI 外壳
- 离线后处理

## 支持的设备角色

- 微波源：`SMB100A`
- 锁相放大器：`OE1022D` 或 `OE1300`
- 磁场 / 运动执行：`M8812`
- 激光控制：`CNI Laser PSU-SR`

## 核心 CLI 主流程

```text
run-resolve -> run-execute -> artifact-check -> audit-continuity
```

这条链路是正式实验入口。probe、诊断和 demo 命令与正式 runtime 合同分离。

## 快速开始

先构建 C# 解决方案：

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln
```

先做一次不碰设备的配置解析：

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve `
  --station configs/stations/lab_a.json `
  --calibration configs/calibrations/main.json `
  --plan configs/plans/minimal_3point_runtime.json `
  --smb-profile configs/profiles/smb100a_run_pll_default.json `
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json `
  --laser-profile configs/profiles/cni_laser_run_off_background.json
```

执行一次 run：

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute `
  --station configs/stations/lab_a.json `
  --calibration configs/calibrations/main.json `
  --plan configs/plans/minimal_3point_runtime.json `
  --smb-profile configs/profiles/smb100a_run_pll_default.json `
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json `
  --laser-profile configs/profiles/cni_laser_run_off_background.json `
  --out-dir runs/win_csharp_manual_minimal
```

做 artifact 与连续性检查：

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_manual_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_manual_minimal --out runs/win_csharp_manual_minimal/continuity_audit.json
```

## 仓库结构

```text
configs/              station、calibration、plan、profile JSON
docs/rebuild/         架构、runtime 合同、artifact schema、实验室事实
tools/win-csharp/     正式采集主栈
tools/config-generator/
tools/odmr-console-python/
tools/odmr-postprocess/
tools/plan-json-generator/
runs/                 run 输出目录，默认不进 git
```

## 可靠性边界

这个仓库把采集可靠性放在第一位：

- 连续采集热路径是冻结合同，不随意改动
- runtime 真相以 artifact 为准，不以 GUI 状态为准
- 失败必须留下足够事实，支持回放与审计
- Python 不直接进入真机实时采集闭环

对 `OE1022D`，当前经过验证的最小热路径是：

```text
write RALL?
sleep 30ms
blocking exact read 12288B
direct-decode
append collector truth and decoded CSV artifacts
```

## 文档入口

- 项目范围：`docs/rebuild/00_scope.md`
- 架构设计：`docs/rebuild/01_architecture.md`
- Artifact schema：`docs/rebuild/03_artifact_schema.md`
- 设备命令规则：`docs/rebuild/04_device_command_specs.md`
- 设备连接事实：`docs/rebuild/06_device_connection_facts.md`
- 已验证 runtime 基线：`docs/rebuild/08_verified_command_and_runtime_baseline.md`
- 运行与配置手册：`docs/rebuild/09_运行与配置手册.md`
- C# 主栈边界：`docs/rebuild/13_csharp_primary_stack.md`
- 无实时前端下的可信性：`docs/rebuild/14_experiment_reliability_without_live_frontend.md`

## 给暑期研学展示时建议关注

这个项目更适合作为“实验系统设计 + 自动化工程”来看，而不只是一个普通代码仓库。

建议重点看四件事：

- ODMR 实验怎样被抽象成 `Point` 和 `Run`
- 多种实验设备怎样被统一到一个 CLI 主链
- 长时间采集怎样做到可审计、可回放
- 标准化数据集怎样为后续物理分析或机器学习做准备

## License

当前仓库还没有单独声明许可证文件。
