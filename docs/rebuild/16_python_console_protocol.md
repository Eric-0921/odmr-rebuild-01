# Python Console Protocol Gate

本文记录 Python 前端进入 PySide6 之前的协议层 Gate 24。当前主 UI 已推进到 `docs/rebuild/17_pyside6_console.md` 记录的 PySide6 console；本文保留为协议历史和边界说明。

## 目标

- C# 继续作为唯一设备控制和采集 runtime。
- Python 只做配置组合、C# CLI 启动、progress tail、stop-after-current-point request 和 artifact 审查入口。
- 在 PySide6 UI 前先验证 CLI JSONL progress 不影响真实采集连续性。

## C# CLI 协议

`Odmr.WinProbe run-execute` 支持：

```text
--progress-jsonl <path>
--stop-request-file <path>
--emergency-stop-file <path>
```

另有恢复入口：

```text
resume-run --previous-run <dir> --out-dir <dir> --progress-jsonl <path> --stop-request-file <path> --emergency-stop-file <path>
```

`progress-jsonl` 每行一个 JSON record，来自现有 `RunProgressEvent`：

- `run_id`
- `state`
- `event_name`
- `message`
- `point_id`
- `point_index`
- `points_total`
- `frames_total`
- `timeout_count`
- `raw_len_bad_count`
- `delta_gt1_count`
- `quality_status`
- `estimated_run_duration_ms`
- `estimated_point_duration_ms`
- `estimated_sweep_duration_ms`
- `sweep_points`
- `start_hz`
- `stop_hz`
- `step_hz`
- `dwell_ms`

这些 progress record 只在 run、collector、point、cleanup 边界写出，不写逐帧数据。

`stop-request-file` 出现后触发现有 cancellation token。runtime 只在 point 边界暂停，不做强杀；terminal status 固定写为 `paused`，terminal event 固定写为 `run_paused`。

`emergency-stop-file` 出现后触发独立急停 token。runtime 会尽快中断当前 point/sweep，并进入安全停机路径：SMB RF OFF、Laser OFF、M8812 cleanup、collector stop。急停不是强杀进程；terminal status 写为 `aborted`，artifact 默认保留。

## Python Core

`tools/odmr-console-python` 的无 GUI core 是 PySide6 console 复用的控制核心：

- 复用 `tools/config-generator/odmr_config_core.py`
- 生成当前 C# runtime 可读的 plan/profile JSON
- 生成后直接返回 `RunBundle`
- 调用 C# `run-resolve`
- 调用 C# `run-execute --progress-jsonl --stop-request-file --emergency-stop-file`
- 对可恢复 run 调用 C# `resume-run --previous-run ...`
- 写 `control/launch_metadata.json`
- tail `control/progress.jsonl`
- 写 `control/stop.request` 做 point 边界暂停
- 写 `control/emergency_stop.request` 做立即安全停机

恢复时 Python core 负责：

- 从上一个 run 目录分配新的 sibling 输出目录，例如 `run_a__resume_01`
- 写新的 `control/launch_metadata.json`
- 在 metadata 中补 `resume.previous_run_dir` / `resume.resume_out_dir`
- 继续复用相同的 progress / stop / emergency-stop 文件协议

控制文件默认放在：

```text
<out-dir>/control/
```

这不是 runtime schema，C# runtime 不读取这些控制文件。

## 边界

- Python 不直接发 VISA、Serial、TCP 命令。
- Python 不解析 RALL raw，不进入 OE collector。
- resume 只支持当前 direct-decode truth run，不兼容历史 raw-truth run。
- PySide6 已在后续 gate 落地；当前文档只记录协议层验证。
- C# WinForms 保留为 legacy/fallback，不再作为主 UI 投入方向。

## 验证

基础检查：

```bash
python3 -m py_compile tools/odmr-console-python/odmr_console_core.py tools/odmr-console-python/odmr_console.py
python3 tools/odmr-console-python/tests/test_odmr_console_core.py
python3 tools/config-generator/tests/test_config_core.py
dotnet build tools/win-csharp/Odmr.Win.sln
```

协议检查：

```bash
python3 tools/odmr-console-python/odmr_console.py generate-demo-bundle --out-dir <tmp> --run-id python_console_demo_3point
python3 tools/odmr-console-python/odmr_console.py resolve --plan <tmp>/python_console_demo_3point.plan.json --smb-profile <tmp>/smb100a_python_console_demo.json --oe-profile <tmp>/oe1022d_python_console_demo.json --laser-profile <tmp>/cni_laser_python_console_demo.json
```

真机检查：

```bash
python3 tools/odmr-console-python/odmr_console.py start-run --plan <plan> --smb-profile <smb> --oe-profile <oe> --laser-profile <laser> --out-dir <run-dir>
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run <run-dir>
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run <run-dir> --out <run-dir>/continuity_audit.json
```

验收仍看：

- `timeout_count = 0`
- `raw_len_bad_count = 0`
- `delta_gt1_count = 0`
- `audit-continuity verdict = continuous`

## Gate 24 实测结果

2026-06-14 Windows 真机验证：

- Python console core 生成 3-point demo bundle，C# `run-resolve` 解析为 `explicit_points`，`resolved_point_count = 3`。
- Python console core 启动 demo 3-point run：
  - run dir: `runs/python_console_gate24_demo_20260614_072440`
  - `points=3/3`
  - `timeout=0`
  - `raw_len_bad=0`
  - `delta_gt1=0`
  - `artifact-check passed`
  - `audit-continuity verdict=continuous`
- `stop-request-file` 验证：
  - run dir: `runs/python_console_gate24_stop_20260614_072800`
  - 第 1 个 point 完成后停止
  - `stop_after_current_point_requested` 出现在 progress/events
  - cleanup 完整
  - `artifact-check passed`
- 15min laser background 第一次：
  - run dir: `runs/python_console_gate24_15min_20260614_072943`
  - `points=21/21`
  - `timeout=0`
  - `raw_len_bad=0`
  - `artifact-check passed`
  - `delta_gt1=1`
  - `audit-continuity verdict=device_counter_missing_windows`
  - audit 定位为 `prev_frame_seq=2007 -> next_frame_seq=2008`，`delta=2`，`gap_ms=33.8729`
- 15min laser background repeat：
  - run dir: `runs/python_console_gate24_15min_20260614_074757`
  - `points=21/21`
  - `timeout=0`
  - `raw_len_bad=0`
  - `delta_gt1=0`
  - `artifact-check passed`
  - `audit-continuity verdict=continuous`

结论：progress JSONL / pause-request 协议没有进入 collector 热路径；repeat 15min 满足连续性验收。第一次 15min 的单个 device counter gap 作为偶发采集异常保留记录，不在本 gate 内通过改 collector 处理。

## RALL 约束

本 gate 不修改 `OeRallCollector`。

冻结热路径仍是：

```text
write RALL?
sleep 30ms
blocking exact read 12288B
append raw
append frame index
```
