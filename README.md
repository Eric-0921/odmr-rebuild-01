# odmrctl-web-rebuild

这是一个从头开始的最小重建仓库，目标不是恢复旧 GUI，而是先做出一条可验证、可追溯、可长期运行的 CLI 实验主链。

## 当前主栈状态

`main` 分支的主运行栈是 Windows C#：

- 主入口：`tools/win-csharp/Odmr.WinProbe`
- 主命令：`run-resolve`、`run-execute`、`resume-run`、`artifact-check`、`audit-continuity`、`live-replay`、各设备 probe、`device-command-check`
- 主设备链路：OE1022D 走 NI-VISA ASRL，OE1300 走 TCP，SMB100A 走 C# VISA USB resource，M8812/CNI Laser 走 Windows Serial
- Rust 工程本体保留在 `win-csharp-rebuild` 分支作为归档参考，不在 `main` 日常主栈中保留

collector 按 `lockin_model` 分支，各自有冻结合同。OE1022D `RALL?` 热路径：

```text
write RALL?
sleep 30ms
blocking exact read 12288B
direct-decode
append collector_frames + unique-only parameter_values + unique-only sample_values
```

OE1300 `RALL?` 热路径：

```text
write RALL?\r
sleep 5ms
read until 32768B
direct-decode
append collector_blocks + unique-only parameter_values + unique-only sample_values
```

artifact 审查、连续性 audit、quality、GUI/live 都必须放在 collector 外侧。

当前 `main` 仓库包含三类内容：

- `docs/equipment_manual/`：设备手册真值文档、冻结参考资料、命令真值层
- `docs/rebuild/`：重建范围、架构、runtime 协议、artifact 设计、连接事实
- `tools/win-csharp/`：当前主运行栈

当前阶段约束：

- 已有 C# 主栈 runtime：固定 profile、baseline lock、run 级 `RALL?` collector、JSON plan、artifact 审查和连续性 audit
- helper 名称必须带设备型号前缀
- 注释和文档统一使用中文

语言边界已经更新：

- C# 负责设备命令、transport、runtime、artifact、离线审查和连续性 audit
- Rust 归档参考保留在 `win-csharp-rebuild` 分支，不再作为 `main` 运行依赖
- Python 负责 plan 生成、calibration 拟合、replay 后分析
- Python 不进入实时采集链路，不直接连接设备

下一阶段的直接输入文档：

- `docs/rebuild/13_csharp_primary_stack.md`
- `docs/rebuild/14_experiment_reliability_without_live_frontend.md`
- `docs/rebuild/15_rust_archive_exit.md`
- `docs/rebuild/01_architecture.md`
- `docs/rebuild/04_device_command_specs.md`
- `docs/rebuild/06_device_connection_facts.md`
- `docs/rebuild/09_运行与配置手册.md`
- `docs/rebuild/10_原标签契约.md`
- `docs/rebuild/11_变更与踩坑.md`

当前可直接作为后续开发输入的配置示例：

- `configs/stations/lab_a.json`
- `configs/stations/lab_a.example.json`

其中 `lab_a.json` 直接保存当前实验室默认端口、地址和 SN，优先用它做真机验证；`lab_a.example.json` 只保留为更宽松的参考模板。

但要明确：

- 串口路径不是设备真值，只是当前 Windows 实验机上已经验证过的连接事实
- 当前日常验证入口是 C# probe：`visa-list`、`oe-idn`、`smb-probe`、`m8812-probe`、`laser-probe`
- 当前实验入口是 C# `run-resolve` / `run-execute`
- 真实实验 provenance 来自 snapshots、`device_state.jsonl`、`segments.jsonl`、型号对应的 collector truth（OE1022D `collector_frames.jsonl` / OE1300 `collector_blocks.jsonl`）、`parameter_values.csv`、`sample_values.csv` 和离线审查工具

当前已经打通的 C# 主链路：

- `tools/win-csharp/Odmr.Devices`：设备命令与 transport
- `tools/win-csharp/Odmr.Runtime`：配置驱动 runtime
- `tools/win-csharp/Odmr.Artifacts`：artifact writer、contract check、continuity audit
- `tools/win-csharp/Odmr.WinProbe`：CLI 入口
- `tools/win-csharp/Odmr.ControlPanel.WinForms`：Windows Run Bundle legacy/fallback 控制面板
- `tools/config-generator`：跨 Mac/Windows 的 Python 配置生成器，输出 `plan + SMB/OE/Laser profile`
- `tools/odmr-console-python`：Python/PySide6 控制台，负责配置组合、配置生成、C# CLI 启动、progress JSONL tail、stop-after-current-point request 和 artifact 审查入口
- `tools/plan-json-generator`：独立浏览器版磁场扫描 `plan.json` 生成器，只作为轻量 plan-only 辅助工具

当前已经真机验证过的命令和链路见：

- `docs/rebuild/08_verified_command_and_runtime_baseline.md`

当前 `OE1022D` 读取链的最新收敛状态：

- Windows `OE1022D` 默认使用 NI-VISA/PyVISA backend，resolver 仍先枚举 VISA ASRL resource 并用 `*IDN?` + SN 认领设备。
- C# `run-execute` 的定长 `RALL?` 热路径已经收敛到 LabVIEW-like simple loop：
  - 写 `RALL?`
  - 等 `30ms`
  - blocking exact read `12288B`
  - 读完立即进入下一轮
- `poll_interval_ms` 不再参与定长 `RALL?` 热路径；旧的 first-byte deadline、frame deadline、zero-byte retry 旋钮已经从 runtime collector 配置中移除。
- `payload[12287]` 作为 `device_packet_counter` 写入 `collector_frames.jsonl`，并由 C# `audit-continuity` 做 `delta=1/0/>1` 连续性审计。
- `quality` 用 unique RALL windows 判断 `min_frames`，重复窗口只记录为诊断字段，不再因为 duplicate ratio 单独失败。
- `quality.jsonl` 现在额外输出：
  - `collector_health = clean | recovered_timeout | degraded_timeout`
  - `timeout_budget_remaining`
- `events.jsonl` 现在会显式记录：
  - `collector_timeout`
  - `collector_recovered`
  - `collector_stopped`
- `2026-06-13` Windows full stack long run 已验证：
  - `21/21 points passed`
  - `collector_timeout_total = 0`
  - `raw_len = 12288` 全部成立
  - `device_packet_counter delta_gt1_count = 0`
  - C# `audit-continuity` verdict = `continuous`

当前可执行主命令见：

- `tools/win-csharp/Odmr.WinProbe/README.md`
- `docs/rebuild/13_csharp_primary_stack.md`

## Agent / Harness Engineering 操作指南

推荐把 agent 操作固定成“只改 JSON、固定 CLI 流程、离线审查收口”。不要让 agent 临场改 C# runtime 或设备 helper；除非明确进入开发任务，否则实验任务只生成配置文件并执行现有 CLI。

参数分层规则：

- point 级参数可以放在同一个 `plan.json` 的 `points[]` 中逐点变化：`target_b_nt`、`magnetic_mode`、`smb_override.start_hz / stop_hz / step_hz / dwell_ms / power_dbm`。
- run/profile 级参数不要塞进 point；需要变化时拆成多个 run，每个 run 使用一套 profile JSON：OE fixed profile、OE collector、SMB fixed LF/FM profile、Laser profile、station、calibration、baseline policy。
- `point_source.cartesian_grid` 适合规则网格；非规则、agent 编排、混合 sweep 的任务优先生成显式 `points[]`。
- OE1022D / OE1300 collector 是 run 级单实例；不要在一个 run 内动态切 OE fixed 参数或 collector 参数。

固定 harness 流程：

```text
1. 读取现有 station / calibration / profile JSON
2. 按 run/profile 级参数分组；每组生成独立 run bundle
3. 每组内部只用 plan.points[] 表达 point 级磁场和 SMB sweep 变化
4. 先执行 run-resolve，保存/检查解析摘要
5. 再执行 run-execute，必须指定唯一 out-dir
6. 执行 artifact-check
7. 执行 audit-continuity
8. 汇总 summary.json、continuity_audit.json、operator_metadata 和关键事件
```

最小显式 point 示例：

```json
{
  "run_id": "agent_custom_run",
  "operator": "agent",
  "acquisition_window_ms": 0,
  "point_settle_ms": 1000,
  "failure_policy": "continue",
  "mag_baseline_policy": {
    "baseline_current_a": [0.0, 0.0, 0.0],
    "settle_ms": 1000,
    "readback_samples": 3,
    "settle_tolerance_a": 0.002,
    "voltage_v": 75.0,
    "voltage_protection_v": 75.0,
    "output_enabled": true
  },
  "quality_thresholds": {
    "min_frames": 10,
    "max_timeout_count": 2,
    "max_duplicate_ratio": 0.3,
    "max_last_frame_age_ms": 500
  },
  "points": [
    {
      "point_id": "custom_a",
      "target_b_nt": [0.0, 0.0, 0.0],
      "smb_override": {
        "start_hz": 2800000000.0,
        "stop_hz": 2810000000.0,
        "step_hz": 1000000.0,
        "dwell_ms": 20,
        "power_dbm": -20.0
      }
    },
    {
      "point_id": "no_mag",
      "magnetic_mode": "none",
      "smb_override": {
        "start_hz": 2700000000.0,
        "stop_hz": 2705000000.0,
        "step_hz": 500000.0,
        "dwell_ms": 10,
        "power_dbm": -30.0
      }
    }
  ]
}
```

推荐 agent 提示词模板：

```text
你是 ODMR 实验 harness agent。你的任务是编排实验配置和调用既有 CLI，不是开发 runtime。

仓库和执行路径：
- macOS 仓库路径：/Users/erictseng/Documents/odmr-rebuild-01
- Windows 真机仓库路径：D:\git-zbw\odmr-rebuild-01
- 真机执行必须在 Windows 仓库中运行：Set-Location 'D:\git-zbw\odmr-rebuild-01'
- C# CLI 项目固定为：tools\win-csharp\Odmr.WinProbe
- 生成的实验 JSON 默认写入：configs\generated\
- 运行输出默认写入：runs\<run_id>_<timestamp>\

允许修改：
- 只允许新增或修改 JSON 配置文件，优先放在 configs/generated/。
- 可生成 plan JSON。
- 可复制现有 profile JSON 到 configs/generated/ 后修改 run/profile 级参数。
- 可写独立 notes/metadata JSON，但不要用文件名承载实验备注。

禁止修改：
- 不要修改 tools/win-csharp/**、tools/odmr-console-python/**、docs/**、README.md。
- 不要修改 station/calibration 原始文件，除非用户明确要求。
- 不要直接调用 VISA、Serial、TCP 或设备 SDK；设备访问只能通过 Odmr.WinProbe CLI。
- 不要修改 collector 热路径，不要新增后台 reader，不要绕过 artifact-check / audit-continuity。

参数分层规则：
- point 级允许写入 plan.points[]：target_b_nt、magnetic_mode、smb_override.start_hz、smb_override.stop_hz、smb_override.step_hz、smb_override.dwell_ms、smb_override.power_dbm。
- run/profile 级必须拆成多个 run：OE sensitivity/time constant/filter/reference/collector、SMB fixed LF/FM、Laser、station、calibration、mag_baseline_policy。
- 如果实验同时要求两个 OE time constant，就生成两个 oe_profile JSON 和两个 run；不要试图在同一个 run 的 point 内切 OE 参数。
- 磁场电源当前只支持非负目标电流；不要生成会导致负电流的 target_b_nt。

固定输入文件优先级：
- 默认 OE1022D station：configs\stations\lab_a.json
- 默认 OE1300 station：configs\stations\lab_a_oe1300_tcp.json
- 默认 calibration：configs\calibrations\main.json
- 默认 SMB profile：configs\profiles\smb100a_run_pll_default.json
- 默认 OE1022D profile：configs\profiles\oe1022d_run_ch_b_observed.json
- 默认 OE1300 profile：configs\profiles\oe1300_run_tcp_default.json
- 默认 Laser off profile：configs\profiles\cni_laser_run_off_background.json

执行顺序：
1. 生成 JSON 后，先输出本次会使用的六个路径：station、calibration、plan、smb-profile、oe-profile、laser-profile。
2. 必须先运行 run-resolve；失败则停止，不准继续 run-execute。
3. run-resolve 成功后运行 run-execute，并使用唯一 out-dir。
4. run-execute 完成后运行 artifact-check。
5. artifact-check 完成后运行 audit-continuity。
6. 最后汇总 run 目录、summary.json、artifact-check 输出、continuity_audit.json。

命令模板：
dotnet run --project tools\win-csharp\Odmr.WinProbe -- run-resolve --station <station.json> --calibration <calibration.json> --plan <plan.json> --smb-profile <smb.json> --oe-profile <oe.json> --laser-profile <laser.json>

dotnet run --project tools\win-csharp\Odmr.WinProbe -- run-execute --station <station.json> --calibration <calibration.json> --plan <plan.json> --smb-profile <smb.json> --oe-profile <oe.json> --laser-profile <laser.json> --out-dir <run_dir> --progress-jsonl <run_dir>\control\progress.jsonl --stop-request-file <run_dir>\control\stop.request --emergency-stop-file <run_dir>\control\emergency_stop.request

dotnet run --project tools\win-csharp\Odmr.WinProbe -- artifact-check --run <run_dir>

dotnet run --project tools\win-csharp\Odmr.WinProbe -- audit-continuity --run <run_dir> --out <run_dir>\continuity_audit.json

失败处理：
- run-resolve 失败：报告 JSON 路径和错误，不运行设备。
- run-execute 失败或 aborted：仍保留 run_dir，继续尝试 artifact-check；如果 artifact-check 无法运行，报告缺失文件。
- artifact-check 未 passed：不要声称实验成功。
- audit-continuity verdict 不是 continuous：不要声称连续性通过。

最终报告必须包含：
- 使用的 git commit
- 六个输入 JSON 路径
- run_dir
- lockin_model
- points_passed / points_total
- timeout_count
- raw_len_bad_count
- OE1022D: delta_gt1_count
- OE1300: decode_failures 和 effective_sample_hz_per_parameter
- artifact-check status
- continuity verdict
- 任何 operator_metadata / notes 文件路径
```

当前 UI 边界：

- PySide6 console 是当前主 UI，负责组合一次 run 所需的六个 JSON：`station`、`calibration`、`plan`、`smb-profile`、`oe-profile`、`laser-profile`，并显示解析摘要、启动 run、tail progress、触发 artifact 审查。
- PySide6 Run Monitor 支持两种停止：普通停止是当前 point 后停止；急停会请求 C# runtime 尽快关闭 SMB RF、Laser，并执行 M8812 cleanup 与 collector stop，run 状态写为 `aborted`。
- PySide6 的预计进度只基于 C# `run-resolve` 的 sweep/run 估算和 progress JSONL 低频事件，本地 500ms 插值显示；它不读取 RALL raw、不按 frame 推进、不参与 quality/audit。
- 日常配置编辑优先使用 PySide6 console 内置的 Config Generator；Tk 版 `tools/config-generator/odmr_config_generator.py` 保留为 fallback。生成器输出实验 `plan.json`、SMB sweep profile、OE fixed profile 和 Laser profile。
- plan 中的 `point` 表示一次采集 step：`magnetic_mode=none` 是无磁场控制，`magnetic_mode=controlled` 才使用 `target_b_nt` 走 M8812 baseline/current/readback 链路。
- Python/PySide6 控制台只调用 C# `Odmr.WinProbe`，不直接操作 VISA/串口/TCP；PySide6 UI 复用 console core，不重新实现设备控制。
- Python 配置生成器是单线程离线 GUI；UI 可选择输入单位，但写入 JSON 时统一回到现有 C# runtime units：磁场 `nT`、频率 `Hz`、时间 `ms`、电流 `A`、laser `mW`。
- 配置生成器中的设备枚举来自仓库说明书/命令真值，SMB/OE 的可选 token 和 OE `j` 编码使用下拉框；只有连续数值和实验身份字段允许手动输入。
- `tools/plan-json-generator/index.html` 只保留为 plan-only 的浏览器辅助工具；它不编辑 SMB/OE/Laser。
- 配置生成器输出的 JSON 仍然是现有 C# runtime schema，不引入 `field_space/groups` 或新的 run bundle schema。

第一版 runtime 默认配置：

- `configs/profiles/smb100a_run_pll_default.json`
  - 固定 LF/FM profile
  - point 只覆盖 sweep/power
- `configs/profiles/oe1022d_run_ch_b_observed.json`
  - 固定 Channel-B profile
  - collector 参数和 ring buffer 容量一并固化
- `configs/calibrations/main.json`
  - 第一版最小磁场映射样例
  - 当前默认值按 `reverse_application/reverse_output/para.xml` 的三轴线圈常数反推
  - 对角线近似为 `X=6.980315e-6`、`Y=7.053679e-6`、`Z=6.404099e-6 A/nT`
- `configs/plans/minimal_3point_runtime.json`
  - 零偏锁定策略
  - 3 个显式 `target_b_nt` point 的最小运行样例
- `configs/plans/no_magnetic_control_single_step.json`
  - 1 个无磁场控制采集 step
  - 不指挥 M8812，不把无磁场伪装成 `[0,0,0]`
- `configs/plans/zero_baseline_single_point.json`
  - 1 个 controlled 零场 point `[0,0,0]`
- `configs/plans/constant_field_single_point.json`
  - 1 个 controlled 恒定磁场 point
- `configs/plans/mag_zero_lock_verify.json`
  - 三轴磁场单独验证样例
- `configs/plans/grid_2d_raster_small.json`
  - 2D 小网格真实运行样例
- `configs/plans/grid_3d_example.json`
  - 3D 配置展开样例
- `configs/plans/x_axis_1d_bounce_15min.json`
  - 高层 `cartesian_grid` 1D X 轴往返样例
  - 当前配置是 `fixed_total_points=21`
  - 是否接近“15 分钟”取决于搭配的 SMB sweep profile；配 `smb100a_run_pll_default.json` 实测更接近 `36min`
  - 估时已包含每 point 的 SMB 重配置 settle 开销
- `configs/profiles/smb100a_run_short_sweep_15min.json`
  - 专用短 sweep profile
  - 用于 1D 往返长跑基线
- `configs/profiles/cni_laser_run_on_background.json`
  - run 固定背景开启样例
  - 默认功率 `50mW`，可在 JSON 中直接调整

真机 verify 准备见：

- `docs/rebuild/07_station_verify_and_hardware_checklist.md`

当前 smoke 的核心需求相关流程固定为：

- `RF + Mag + OE`
- `Laser` 只做固定 `OFF` 背景控制验证，不进入开启输出路径

下一阶段 runtime 的固定方向：

- 固定 profile + SMB100A 浮动参数
- `Laser` 作为 run 固定背景条件开启，不进入 point 变量
- `OE1022D` / `OE1300` 都采用 run 级单 reader `RALL?` collector，按 `lockin_model` 选择不同 collector 合同
- collector 只在 C# `run-execute` 打开后启动，不在“仅连接设备”阶段常驻拉取
- point 真值已切到 decoded truth：OE1022D `collector_frames.jsonl`，OE1300 `collector_blocks.jsonl`，两者共同使用 `parameter_values.csv + sample_values.csv + segments.jsonl`
- 当前 C# `run-execute` 默认 artifact：型号对应 collector truth + `parameter_values.csv + sample_values.csv + segments + points + quality + device_state + events + snapshots + manifest + summary`
- 旧 Rust `raw/oe1022d.frames.parsed.jsonl` 和早期 `raw/frames.idx` 产物不属于当前 C# 默认 artifact 合同
- 当前实验室真机已证明：`*OPC?` 不能单独作为 sweep 结束信号，runtime 已改为 `*OPC?` + sweep 时长估算 fallback
- 当前已经接入 `target_b_nt -> calibration -> target_current_a -> measured_current_a` 链路
- 当前零场锁定语义是“零偏电流锁定 + 复现电流叠加”，不是物理零磁场已证明
- 当前磁场电源第一版只支持非负目标电流；默认 1D/2D/3D 示例网格已全部改成非负值
- 当前后处理直接读取 `sample_values.csv + segments.jsonl`，不再要求 `point_fields.jsonl` 或每 point `NPZ` sidecar
- 当前 `runs/grid_3d_raster_0_50_100ut_validation_live` 已完成 `3D 0/50/100 nT` 单轮真机验证：`27/27 points passed`、`collector_timeout_total=0`
- continuity audit 现在直接读取 decoded collector truth：OE1022D 用 `collector_frames.jsonl`，OE1300 用 `collector_blocks.jsonl`
