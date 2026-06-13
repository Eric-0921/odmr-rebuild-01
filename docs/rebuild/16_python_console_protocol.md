# Python Console Protocol Gate

本文记录 Python 前端进入 PySide6 之前的协议层 gate。

## 目标

- C# 继续作为唯一设备控制和采集 runtime。
- Python 只做配置组合、C# CLI 启动、progress tail、stop-after-current-point request 和 artifact 审查入口。
- 在 PySide6 UI 前先验证 CLI JSONL progress 不影响真实采集连续性。

## C# CLI 协议

`Odmr.WinProbe run-execute` 支持：

```text
--progress-jsonl <path>
--stop-request-file <path>
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

这些 progress record 只在 run、collector、point、cleanup 边界写出，不写逐帧数据。

`stop-request-file` 出现后触发现有 cancellation token。runtime 只在 point 边界停止，不做强杀。

## Python Core

`tools/odmr-console-python` 是后续 PySide6 的无 GUI core：

- 复用 `tools/config-generator/odmr_config_core.py`
- 生成当前 C# runtime 可读的 plan/profile JSON
- 生成后直接返回 `RunBundle`
- 调用 C# `run-resolve`
- 调用 C# `run-execute --progress-jsonl --stop-request-file`
- 写 `control/launch_metadata.json`
- tail `control/progress.jsonl`

控制文件默认放在：

```text
<out-dir>/control/
```

这不是 runtime schema，C# runtime 不读取这些控制文件。

## 边界

- Python 不直接发 VISA、Serial、TCP 命令。
- Python 不解析 RALL raw，不进入 OE collector。
- PySide6 后置；当前 gate 只验证协议和稳定性。
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
