# PySide6 Console Gate

本阶段把 Python 前端从无 GUI core 推进到 PySide6 控制台 v1。

## 开发原则

- C# 仍是唯一设备控制和采集 runtime。
- PySide6 只做配置生成、Run Bundle 组合、C# CLI 启动、progress JSONL 监控和 artifact 审查。
- 不新增 runtime schema；一次 run 仍由 station、calibration、plan、SMB profile、OE profile、laser profile 六个 JSON 组成。
- 配置生成复用 `tools/config-generator/odmr_config_core.py`，不复制 JSON 生成逻辑。
- 不做常驻 RALL live collector，不解析 RALL raw，不触碰 OE `RALL?` collector 热路径。
- `point` 的运行语义是一次采集 step；磁场只是 point 的可选上下文。

## UI 结构

入口：

```bash
python3 tools/odmr-console-python/odmr_console_qt.py
```

页面：

- `本次实验配置`：选择六个 JSON 和 run 输出目录，显示本地 JSON 摘要。
- `配置生成`：生成 plan、SMB profile、OE profile、Laser profile，并自动绑定到本次实验配置。
- `预检查 / 预计用时`：调用 C# `run-resolve`。
- `运行监控`：调用 C# `run-execute --progress-jsonl --stop-request-file --emergency-stop-file`，只 tail progress JSONL。
- `数据审查`：调用 C# `artifact-check` 和 `audit-continuity`。

Plan 类型：

- `无磁场控制`：生成 `magnetic_mode=none` acquisition step，不指挥 M8812，不做 baseline lock，也不把它伪装成 `[0,0,0]`。
- `零场 / 恒定磁场`：生成一个 `magnetic_mode=controlled` point，默认 `[0,0,0]`，按现有 baseline/current/readback 链路执行。
- `磁场扫描`：生成多个 `magnetic_mode=controlled` point；默认扫描方向为单向 `raster`，往返仅作为 X 轴 1D 可选模式。

## 边界

PySide6 不直接操作 VISA、Serial、TCP，也不发 SMB/OE/M8812/Laser 命令。普通 Stop 语义仍是 stop-after-current-point，只通过 stop request file 触发 C# runtime 在 point 边界退出。

急停按钮只创建 `control/emergency_stop.request`。C# runtime 收到后尽快中断当前 point/sweep，执行 SMB RF OFF、Laser OFF、M8812 cleanup、collector stop，并把 summary/manifest 写为 `aborted`。UI 收到 `run_aborted` 后询问是否保留本次数据；选择丢弃时移动到同级 `_discarded`，不直接删除。

预计进度是 UI 本地估算，不是实验真值。`run-resolve` 输出 `estimated_sweep`、`estimated_point_duration_ms`、`estimated_run_duration_ms`；运行中 progress JSONL 低频输出 `sweep_started/sweep_completed` 和 sweep 参数。PySide6 用 500 ms timer 插值显示当前 sweep 和总进度，超过预计时停在 99% 等待设备完成或 cleanup。这个机制不读取 RALL raw，不按 frame 推进，也不进入 collector。

配置生成器中的单位选择只影响输入显示；写入 JSON 前统一转换到 C# runtime 使用的 canonical unit。

Windows 日常工作区以 `D:\git-zbw\odmr-rebuild-01` 为准。曾经存在的 `C:\Users\Piwei Tseng\odmr-rebuild-01` 只作为旧 clone，不作为 Visual Studio / PySide6 日常入口。2026-06-14 已修复 D 盘 clone 的 `remote.origin.fetch`，并强制同步到 `origin/main`。

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

## 长时间运行 UI 外壳校对

- Run Monitor 只按文件 offset 增量 tail `progress.jsonl`，不读 RALL raw，不进入 collector。
- stdout/stderr 继续写入 `<out-dir>/control/stdout.log` 和 `stderr.log`，避免 pipe 阻塞。
- 预计进度只依赖 SMB sweep 参数和 UI 本地时钟，不作为 artifact、quality 或审计依据。
- 急停路径由 C# runtime 做安全停机；Python/PySide6 不直接向设备发命令。
- 如果 C# `dotnet run` 进程在 terminal progress event 前退出，Run Monitor 会停止计时器并显示 stdout/stderr 尾部，避免 UI 一直停留在 running。
- Config Generator 当前扫描块校验失败时会阻止 Generate，避免用户界面显示的新扫描参数未写入 JSON 而实际运行旧 block。
- Artifact Review 运行 `artifact-check` / `audit-continuity` 时会禁用审查按钮，避免重复点击造成输出混乱。
## 完整程序启动（Windows 日常入口）

Windows 实验机的日常工作区固定使用：

```powershell
cd D:\git-zbw\odmr-rebuild-01
```

每次从 Mac 侧同步新 commit 后，Windows 侧先更新仓库：

```powershell
git fetch origin
git reset --hard origin/main
```

启动完整程序的推荐入口是 PySide6 console。它负责配置组合、生成 plan/profile、调用 C# `run-execute`、tail progress JSONL、发 stop-after-current-point request，并提供 artifact 审查入口：

```powershell
python tools\odmr-console-python\odmr_console_qt.py
```

如果 Python 环境缺少 PySide6，先安装 UI 依赖：

```powershell
python -m pip install -r tools\odmr-console-python\requirements-pyside6.txt
```

GUI 里完整实验路径：

1. 在 `本次实验配置` 页选择 `station`、`calibration`、`plan`、`smb-profile`、`oe-profile`、`laser-profile`。
2. 如需新点表或 profile，在 `配置生成` 页生成并保存 JSON。
3. 在 `预检查 / 预计用时` 页调用 `run-resolve`。
4. 在 `运行监控` 页选择输出目录并启动；该入口实际调用 C# `Odmr.WinProbe run-execute`。
5. 实验结束后，在 `数据审查` 页运行 `artifact-check` 和 `audit-continuity`。

不走 GUI 时，等价的最小 CLI 启动链路是：

```powershell
dotnet build tools\win-csharp\Odmr.Win.sln
dotnet run --project tools\win-csharp\Odmr.WinProbe -- run-resolve --station configs\stations\lab_a.json --calibration configs\calibrations\main.json --plan configs\plans\minimal_3point_runtime.json --smb-profile configs\profiles\smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs\profiles\oe1022d_run_ch_b_observed.json --laser-profile configs\profiles\cni_laser_run_off_background.json
dotnet run --project tools\win-csharp\Odmr.WinProbe -- run-execute --station configs\stations\lab_a.json --calibration configs\calibrations\main.json --plan configs\plans\minimal_3point_runtime.json --smb-profile configs\profiles\smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs\profiles\oe1022d_run_ch_b_observed.json --laser-profile configs\profiles\cni_laser_run_off_background.json --out-dir runs\win_csharp_manual_minimal
dotnet run --project tools\win-csharp\Odmr.WinProbe -- artifact-check --run runs\win_csharp_manual_minimal
dotnet run --project tools\win-csharp\Odmr.WinProbe -- audit-continuity --run runs\win_csharp_manual_minimal --out runs\win_csharp_manual_minimal\continuity_audit.json
```

边界：

- Python/PySide6 console 是当前完整程序入口，但不直接控制设备。
- 设备控制仍只由 Windows C# `Odmr.WinProbe` 执行。
- `artifact-check` 和 `audit-continuity` 是只读审查，不碰设备。
