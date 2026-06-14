# PySide6 Console Gate

本阶段把 Python 前端从无 GUI core 推进到 PySide6 控制台 v1。

## 开发原则

- C# 仍是唯一设备控制和采集 runtime。
- PySide6 只做配置生成、Run Bundle 组合、C# CLI 启动、progress JSONL 监控和 artifact 审查。
- 不新增 runtime schema；一次 run 仍由 station、calibration、plan、SMB profile、OE profile、laser profile 六个 JSON 组成。
- 配置生成复用 `tools/config-generator/odmr_config_core.py`，不复制 JSON 生成逻辑。
- 不做常驻 RALL live collector，不解析 RALL raw，不触碰 OE `RALL?` collector 热路径。

## UI 结构

入口：

```bash
python3 tools/odmr-console-python/odmr_console_qt.py
```

页面：

- `Run Bundle`：选择六个 JSON 和 run 输出目录，显示本地 JSON 摘要。
- `Config Generator`：生成 plan、SMB profile、OE profile、Laser profile，并自动绑定到 Run Bundle。
- `Resolve / Estimate`：调用 C# `run-resolve`。
- `Run Monitor`：调用 C# `run-execute --progress-jsonl --stop-request-file`，只 tail progress JSONL。
- `Artifact Review`：调用 C# `artifact-check` 和 `audit-continuity`。

## 边界

PySide6 不直接操作 VISA、Serial、TCP，也不发 SMB/OE/M8812/Laser 命令。Stop 语义仍是 stop-after-current-point，只通过 stop request file 触发 C# runtime 现有 cancellation。

配置生成器中的单位选择只影响输入显示；写入 JSON 前统一转换到 C# runtime 使用的 canonical unit。

## 验证

基础检查：

```bash
python3 -m py_compile tools/odmr-console-python/odmr_console_core.py tools/odmr-console-python/odmr_console.py tools/odmr-console-python/odmr_console_qt.py
python3 tools/odmr-console-python/tests/test_odmr_console_core.py
python3 tools/config-generator/tests/test_config_core.py
```

Windows 真机检查：

```bash
dotnet build tools/win-csharp/Odmr.Win.sln
python tools/odmr-console-python/odmr_console_qt.py
```

通过 UI 执行：

- 生成 3-point bundle
- `run-resolve`
- minimal 3-point run
- 15min laser background run
- `artifact-check`
- `audit-continuity`

验收仍看：

- `timeout=0`
- `raw_len_bad=0`
- `delta_gt1=0`
- `audit-continuity verdict=continuous`

## 2026-06-14 验证记录

Mac 本机：

- 已安装 `PySide6 6.11.1`。
- `py_compile` 通过。
- `tools/odmr-console-python/tests/test_odmr_console_core.py` 通过。
- `tools/config-generator/tests/test_config_core.py` 通过。
- PySide6 界面已实际启动并截图检查：Run Bundle、Config Generator、Magnetic Plan、SMB100A、OE1022D、Run Monitor、Artifact Review 页面没有明显文字挤压；单位控件位于数值输入右侧。

Windows 真机：

- 仓库同步到 `dadb4f1 Add PySide6 ODMR console`。
- 已安装 `PySide6 6.11.1`。
- `py_compile` 通过。
- Python console core 测试通过。
- config generator core 测试通过。
- `dotnet build tools/win-csharp/Odmr.Win.sln` 通过，`0 warnings / 0 errors`。
- PySide6 headless smoke 通过：UI 类生成临时 JSON、绑定 Run Bundle，并调用 C# `run-resolve`，`resolved_point_count = 8`。
- PySide6 Run Monitor 路径启动 minimal 3-point：
  - run dir: `runs/pyside6_ui_minimal_20260614_084932`
  - `points = 3/3`
  - `frames_total = 4733`
  - `timeout = 0`
  - `raw_len_bad = 0`
  - `delta_gt1 = 0`
  - `artifact-check passed`
  - `audit-continuity verdict = continuous`
- PySide6 Run Monitor 路径启动 15min laser background：
  - run dir: `runs/pyside6_ui_15min_laser_20260614_085303`
  - elapsed: `973.7s`
  - `points = 21/21`
  - `frames_total = 30482`
  - `timeout = 0`
  - `raw_len_bad = 0`
  - `delta_gt1 = 0`
  - `artifact-check passed`
  - `quality_status_counts.passed = 21`
  - `audit-continuity verdict = continuous`
