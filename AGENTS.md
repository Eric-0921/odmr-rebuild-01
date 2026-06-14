# Agent Guide: odmr-rebuild-01

> 本文档面向 AI 编码助手，汇总项目架构、构建/测试命令、开发约定与运行时边界。
> 项目主要文档与注释使用中文，请保持中文。

## 1. 项目概述

这是一个 ODMR 实验系统的**最小重建仓库**，当前阶段目标不是恢复旧 GUI，而是在 Windows 上建立一条可验证、可追溯、可长期运行的 **CLI 实验主链**。

- **主运行栈**：C# / .NET 8（`tools/win-csharp/`）
- **离线辅助**：Python（`tools/config-generator`、`tools/odmr-console-python`、`tools/odmr-postprocess`）
- **归档参考**：Rust 工程本体保留在 `win-csharp-rebuild` Git 分支，`main` 分支不再依赖 Rust
- `target/debug/` 等目录是历史 Rust/C# 构建残留；`main` 分支根目录**没有 `Cargo.toml`**

核心工作流：

1. 用 Python 配置生成器或 PySide6 Console 生成/组合 6 个 JSON。
2. 用 C# `Odmr.WinProbe` 做设备探针、配置解析（`run-resolve`）、实验运行（`run-execute`）。
3. 用 C# 离线工具做 artifact 审查（`artifact-check`）与连续性审计（`audit-continuity`）。
4. 用 Python 做离线后处理（可选）。

## 2. 技术栈与运行时边界

### C# 主栈（当前唯一连接设备的栈）

| 项目 | 路径 | 职责 |
|------|------|------|
| `Odmr.Devices` | `tools/win-csharp/Odmr.Devices` | VISA/串口/TCP transport；设备命令 helper |
| `Odmr.Runtime` | `tools/win-csharp/Odmr.Runtime` | 配置解析、runtime、collector、point loop |
| `Odmr.Artifacts` | `tools/win-csharp/Odmr.Artifacts` | artifact 写入、合同检查、连续性审计、live replay |
| `Odmr.WinProbe` | `tools/win-csharp/Odmr.WinProbe` | CLI 入口 `Program.cs` |
| `Odmr.ControlPanel.WinForms` | `tools/win-csharp/Odmr.ControlPanel.WinForms` | legacy/fallback WinForms 控制面板 |

- 目标框架：`net8.0`（WinForms 项目为 `net8.0-windows`）
- 关键 NuGet 包：`NationalInstruments.Visa`（OE1022D VISA）、`System.IO.Ports`（M8812 / Laser 串口）
- 解决方案文件：`tools/win-csharp/Odmr.Win.sln`

### Python 边界

| 目录 | 作用 |
|------|------|
| `tools/config-generator` | 离线生成 `plan.json` + `smb_profile.json` + `oe_profile.json` + `laser_profile.json` |
| `tools/odmr-console-python` | PySide6 主 UI + 无 GUI core：组合配置、调用 C# CLI、tail progress、写 stop request |
| `tools/odmr-postprocess` | 离线后处理参考，读取 `point_fields/*.npz` 生成 CSV/JSONL |
| `python/odmr_replay` | 当前源码为空，仅有 `__pycache__` |
| `python/pyproject.toml` | 遗留配置，引用不存在的 `odmr_gui.app:main`，已不再使用 |

**Python 不进入实时采集链路，不直接连接设备。**

### 关键配置文件

一次 run 需要 6 个 JSON：

| 类型 | 关键文件 | 说明 |
|------|----------|------|
| Station | `configs/stations/lab_a.json` | 设备身份、transport hint、SN 认领规则 |
| Calibration | `configs/calibrations/main.json` | `target_b_nt → delta_current_a` 映射 |
| Plan | `configs/plans/minimal_3point_runtime.json` | run 元数据、point 列表或 `cartesian_grid` |
| SMB Profile | `configs/profiles/smb100a_run_pll_default.json` | SMB100A 固定 FM/LF 配置 + 默认 sweep |
| OE Profile | `configs/profiles/oe1022d_run_ch_b_observed.json` | OE1022D Channel-B 固定配置 + collector 参数 |
| Laser Profile | `configs/profiles/cni_laser_run_on_background.json` | Laser run 级背景模式与功率 |

## 3. 仓库目录结构

```text
configs/              # station / calibration / plan / profile JSON
docs/
  equipment_manual/   # 设备手册真值与冻结参考
  rebuild/            # 架构、runtime 协议、artifact 设计、连接事实
python/               # 遗留/空源码包与 pyproject.toml
reverse_application/  # 原厂逆向分析资料
runs/                 # 实验输出目录（被 .gitignore 忽略）
target/               # 历史构建残留（被 .gitignore 忽略）
tools/
  config-generator/         # Tkinter 离线配置生成器 + core
  odmr-console-python/      # PySide6 控制台 + core + CLI
  odmr-postprocess/         # 离线后处理参考
  oe_rall_compare/          # 跨平台 RALL 探针/诊断脚本
  plan-json-generator/      # 浏览器版 plan-only 生成器
  win-csharp/               # 当前主运行栈（C#）
```

## 4. 构建与运行命令

### C# 主栈

```powershell
# 构建整个解决方案
dotnet build tools/win-csharp/Odmr.Win.sln

# 设备探针
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only

# 配置解析（不连接设备）
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve `
  --station configs/stations/lab_a.json `
  --calibration configs/calibrations/main.json `
  --plan configs/plans/minimal_3point_runtime.json `
  --smb-profile configs/profiles/smb100a_run_pll_default.json `
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json `
  --laser-profile configs/profiles/cni_laser_run_off_background.json

# 执行一次 run
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute `
  --station configs/stations/lab_a.json `
  --calibration configs/calibrations/main.json `
  --plan configs/plans/minimal_3point_runtime.json `
  --smb-profile configs/profiles/smb100a_run_pll_default.json `
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json `
  --laser-profile configs/profiles/cni_laser_run_off_background.json `
  --out-dir runs/win_csharp_manual_minimal `
  --progress-jsonl runs/win_csharp_manual_minimal/control/progress.jsonl `
  --stop-request-file runs/win_csharp_manual_minimal/control/stop.request

# 离线审查与审计
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_manual_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_manual_minimal --out runs/win_csharp_manual_minimal/continuity_audit.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- device-command-check
dotnet run --project tools/win-csharp/Odmr.WinProbe -- live-replay --run runs/win_csharp_manual_minimal
```

> 入口命令完整列表见 `tools/win-csharp/Odmr.WinProbe/Program.cs` 的 `PrintUsage()`。

### Python 工具

```bash
# Tk 配置生成器
python tools/config-generator/odmr_config_generator.py

# PySide6 控制台（当前主 UI）
python -m pip install -r tools/odmr-console-python/requirements-pyside6.txt
python tools/odmr-console-python/odmr_console_qt.py

# Python console CLI（不连接设备）
python tools/odmr-console-python/odmr_console.py generate-demo-bundle --out-dir /tmp/demo --run-id demo
python tools/odmr-console-python/odmr_console.py resolve --plan ...
python tools/odmr-console-python/odmr_console.py start-run --plan ... --out-dir runs/demo
python tools/odmr-console-python/odmr_console.py stop --metadata runs/demo/control/launch_metadata.json
python tools/odmr-console-python/odmr_console.py read-progress --progress-jsonl runs/demo/control/progress.jsonl

# 离线后处理
python tools/odmr-postprocess/build_odmr_samples.py --run runs/<run_id>
```

### 浏览器版 Plan 生成器

直接用浏览器打开 `tools/plan-json-generator/index.html`，或用任意静态文件服务器托管该目录。

## 5. 测试策略

- **C#：当前没有单元测试项目**。验证依赖真机 smoke / 最小 run / 长跑 + `artifact-check` + `audit-continuity`。
  - 关键验收指标：`timeout_count == 0`、`raw_len_bad_count == 0`、`delta_gt1_count == 0`、`artifact-check passed`、`audit-continuity verdict == continuous`。
- **Python `unittest`**：
  - `python tools/config-generator/tests/test_config_core.py`
  - `python tools/odmr-console-python/tests/test_odmr_console_core.py`
- **浏览器 plan-core**：
  - `node tools/plan-json-generator/tests/plan-core.test.mjs`（需要 Node.js）
- **基础语法/编译检查**：
  - `python -m py_compile tools/odmr-console-python/*.py tools/config-generator/*.py`
  - `dotnet build tools/win-csharp/Odmr.Win.sln`

## 6. 代码与开发约定

- **注释与文档统一使用中文**；代码标识符使用英文。
- **Helper/类命名必须带设备型号前缀**，例如：
  - `Oe1022dVisa`、`Smb100aTcp`、`M8812Serial`、`CniLaserSerial`
  - `Oe1022dCommands`、`Smb100aCommands` 等
- **命令白名单**：runtime 只能发送 `docs/rebuild/04_device_command_specs.md` 中允许的命令；业务层不得直接拼接裸 SCPI。
- **Helper 职责**：只生成命令字符串/字节，不做 transport、session 或状态机。
- **状态改变必须写 `events.jsonl`**；设备错误队列非空时不能把 point 标为成功。
- **参数归属**：
  - point/run 可变：`target_b_nt`、SMB sweep `start/stop/step/dwell/power`
  - station/profile 固定：OE1022D 全部 setup、M8812 电压/保护、Laser 背景
- **零偏锁定语义**：当前是“零偏电流锁定 + 复现电流叠加”，**不是物理零磁场已证明**。
- **磁场电源第一版只支持非负目标电流**；负电流会在 `run-execute` 中直接报错。
- **新增功能默认进入 C# 主栈**；只有查历史行为或做回归对照时才读取 `win-csharp-rebuild` 分支。

### 冻结的 OE1022D `RALL?` 热路径

```text
write RALL?
sleep 30ms
blocking exact read 12288B
append raw
append frame index
```

禁止在热路径中加入 parser、retry、deadline、GUI publish、quality/audit、多 reader。若必须修改，需重新跑 60s / 15min / repeat 15min 连续性验证。

## 7. Artifact / Run 目录约定

一次 run 的默认输出目录结构（`runs/<run_id>/`）：

```text
run_manifest.json
station_snapshot.json
calibration_snapshot.json
smb_profile_snapshot.json
oe_profile_snapshot.json
laser_profile_snapshot.json
plan_snapshot.json
baseline_snapshot.json
events.jsonl
points.jsonl
point_fields.jsonl
point_fields/
  seg_<point_id>_0000.npz
  seg_<point_id>_0000.manifest.json
segments.jsonl
quality.jsonl
device_state.jsonl
raw/
  oe1022d.rall
  oe1022d.frames.idx.jsonl
  oe1022d.frames.parsed.jsonl   # 仅 --artifact-mode debug，默认不写
summary.json
continuity_audit.json
control/                         # PySide6 console 生成
  progress.jsonl
  stop.request
  launch_metadata.json
  stdout.log
  stderr.log
```

- **最终事实层**：`raw/oe1022d.rall` + `raw/oe1022d.frames.idx.jsonl` + `segments.jsonl`。
- 每帧固定 **12288 B**；`payload[12287]` 作为 `device_packet_counter`，用于连续性审计。
- `point_fields.jsonl` 只存轻量 metadata；完整 20 字段数组默认进 `point_fields/*.npz`。
- 同 `--out-dir` 复跑时，runtime 会覆盖/重建本次生成的 jsonl/raw/index，避免静默 append。
- PySide6 console 的 stop 语义是 **stop-after-current-point**：写 `control/stop.request`，C# runtime 在 point 边界取消。

## 8. 安全与真机注意事项

- **激光器**：当前 smoke 流程中 Laser 只做固定 `OFF` 背景控制验证，不进入开启输出路径；只有经过明确验证的 run 才使用 `cni_laser_run_on_background.json`。
- **RF 输出**：SMB100A 在 point 中通过 `OUTP ON` + `FREQ:MODE SWE` + `SWE:FREQ:EXEC` 打开，cleanup 必须关闭输出。
- **磁场电源**：M8812 cleanup 固定归零、关闭输出、切回 local；负电流不被当前 runtime 支持。
- **串口路径只是 hint**：真机身份优先用 `*IDN?` + SN 认领，不能依赖固定 COM 口号作为设备真值。
- **不要修改 `RALL?` 热路径**除非你已经准备好重新跑连续性验证；它是当前唯一被证明可长期稳定的采集链路。
- **不要从 Python 调用 VISA/串口/TCP 进入实时采集**；Python 只处理离线配置、后处理和 UI。

## 9. 延伸阅读 / 关键文档

核心开发输入：

- `README.md`
- `docs/rebuild/01_architecture.md`
- `docs/rebuild/03_artifact_schema.md`
- `docs/rebuild/04_device_command_specs.md`
- `docs/rebuild/09_运行与配置手册.md`
- `docs/rebuild/13_csharp_primary_stack.md`
- `docs/rebuild/14_experiment_reliability_without_live_frontend.md`
- `docs/rebuild/15_rust_archive_exit.md`
- `docs/rebuild/16_python_console_protocol.md`
- `docs/rebuild/17_pyside6_console.md`
- `docs/rebuild/17_odmr_postprocessing_reference.md`

工具入口文档：

- `tools/win-csharp/Odmr.WinProbe/README.md`
- `tools/config-generator/README.md`
- `tools/odmr-console-python/README.md`
- `tools/odmr-postprocess/README.md`
- `tools/plan-json-generator/README.md`
