# odmrctl-web-rebuild

这是一个从头开始的最小重建仓库，目标不是恢复旧 GUI，而是先做出一条可验证、可追溯、可长期运行的 CLI 实验主链。

当前仓库包含三类内容：

- `docs/equipment_manual/`：设备手册真值文档、冻结参考资料、命令真值层
- `docs/rebuild/`：重建范围、架构、runtime 协议、artifact 设计、连接事实
- `crates/*-commands`：第一版设备命令 helper

当前阶段约束：

- 已有最小 transport、station verify、hardware smoke 入口
- 已有第一版 `run execute` 骨架：固定 profile、baseline lock、run 级 `RALL?` collector、3-point plan
- helper 名称必须带设备型号前缀
- 注释和文档统一使用中文

语言边界已经固定：

- Rust 负责设备命令、transport、station verify、runtime、artifact
- Python 负责 plan 生成、calibration 拟合、replay 后分析
- Python 不进入实时采集链路，不直接连接设备

下一阶段的直接输入文档：

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

- 串口路径不是设备真值，只是最近一次成功时的 hint
- `station verify`、`hardware smoke`、`run execute` 会先枚举当前串口池
- hint 只用于候选排序，不再作为“先直连再回退”的真值
- 真实绑定结果来自 probe / identity / echo，并会写入 `station_snapshot.json`

当前已经打通的最小链路：

- `crates/*-commands`：设备命令 helper
- `crates/*-transport`：最小连接层
- `crates/station-resolver`：基于 identity-first 串口认领的 station verify 与 snapshot 生成
- `apps/odmr-cli`：CLI 入口

当前已经真机验证过的命令和链路见：

- `docs/rebuild/08_verified_command_and_runtime_baseline.md`

当前 `OE1022D` 读取链的最新收敛状态：

- 默认 `observed` profile 已固定为：
  - ASCII query timeout = `300ms`
  - `rall_chunk_timeout = 5ms`
  - `rall_first_byte_deadline = 30ms`
  - `rall_frame_deadline = 120ms`
  - `zero_byte_retry_limit = 1`
- `quality` 现在真正按 `max_timeout_count` 阈值判定，不再把 `timeout_count > 0` 直接视为失败
- `quality.jsonl` 现在额外输出：
  - `collector_health = clean | recovered_timeout | degraded_timeout`
  - `timeout_budget_remaining`
- `events.jsonl` 现在会显式记录：
  - `collector_timeout`
  - `collector_recovered`
  - `collector_stopped`
- `minimal_3point_runtime + smb100a_run_pll_default + oe1022d_run_ch_b_observed + cni_laser_run_on_background`
  已在 `2026-06-12` 真机连续跑通 3 次：
  - 3/3 points passed
  - `collector_timeout_total = 0`
  - `max(frame_gap_ms) ≈ 54.3 ~ 54.6ms`

GUI 本地环境与启动约束：

- GUI 首版依赖本机 Python 虚拟环境，不走系统 Python 直接运行
- GUI 技术栈固定为 `PySide6 + pyqtgraph`
- GUI 只连接本机 `odmr gui-bridge serve`，不直接碰硬件
- 第一次启动前必须先建虚拟环境并安装 `python/` 包

GUI 首次安装：

```bash
python3 -m venv .venv
source .venv/bin/activate
python3 -m pip install -U pip
python3 -m pip install -e ./python
```

GUI 启动方式：

```bash
cargo run -p odmr-cli -- gui-bridge serve
```

新开一个终端：

```bash
source .venv/bin/activate
odmr-gui
```

当前可执行命令：

```bash
cargo run -p odmr-cli -- station verify --station configs/stations/lab_a.json
cargo run -p odmr-cli -- station verify --station configs/stations/lab_a.json --out out/station_snapshot.json
cargo run -p odmr-cli -- hardware smoke --station configs/stations/lab_a.json --out-dir out/hardware_smoke/manual
cargo run -p odmr-cli -- hardware verify-mag-lock \
  --station configs/stations/lab_a.json \
  --calibration configs/calibrations/main.json \
  --plan configs/plans/mag_zero_lock_verify.json \
  --out-dir out/hardware_verify_mag_lock/manual

cargo run -p odmr-cli -- run execute \
  --station configs/stations/lab_a.json \
  --calibration configs/calibrations/main.json \
  --plan configs/plans/minimal_3point_runtime.json \
  --smb-profile configs/profiles/smb100a_run_pll_default.json \
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json \
  --laser-profile configs/profiles/cni_laser_run_on_background.json \
  --out-dir runs/manual

cargo run -p odmr-cli -- run execute \
  --station configs/stations/lab_a.json \
  --calibration configs/calibrations/main.json \
  --plan configs/plans/x_axis_1d_bounce_15min.json \
  --smb-profile configs/profiles/smb100a_run_short_sweep_15min.json \
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json \
  --laser-profile configs/profiles/cni_laser_run_on_background.json \
  --out-dir runs/x_axis_1d_bounce_15min

cargo run -p odmr-cli -- gui-bridge serve
```

第一版 runtime 默认配置：

- `configs/profiles/smb100a_run_pll_default.json`
  - 固定 LF/FM profile
  - point 只覆盖 sweep/power
- `configs/profiles/oe1022d_run_ch_b_observed.json`
  - 固定 Channel-B profile
  - collector 参数和 ring buffer 容量一并固化
- `configs/calibrations/main.json`
  - 第一版最小磁场映射样例
  - 当前示例是对角线 `1 mA / nT`
- `configs/plans/minimal_3point_runtime.json`
  - 零偏锁定策略
  - 3 个显式 `target_b_nt` point 的最小运行样例
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
- `OE1022D` 采用 run 级单 reader `RALL?` collector
- collector 只在 `run execute` 打开后启动，不在“仅连接设备”阶段常驻拉取
- point 真值已切到 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl` 回切；ring buffer 只做实时观察
- 当前 `run execute` 默认进入轻量 artifact 模式：保留 `raw + frames.idx + segments + events + quality + point_fields.jsonl metadata + point_fields/*.npz`
- `raw/oe1022d.frames.parsed.jsonl` 已改成 `--artifact-mode debug` 才落盘的重型调试产物
- 当前实验室真机已证明：`*OPC?` 不能单独作为 sweep 结束信号，runtime 已改为 `*OPC?` + sweep 时长估算 fallback
- 当前已经接入 `target_b_nt -> calibration -> target_current_a -> measured_current_a` 链路
- 当前零场锁定语义是“零偏电流锁定 + 复现电流叠加”，不是物理零磁场已证明
- 当前磁场电源第一版只支持非负目标电流；默认 1D/2D/3D 示例网格已全部改成非负值
- 当前 `point_fields.jsonl` 已改成轻量 metadata；完整 20 字段数组和必要状态数组进入每个 point 的 `NPZ` sidecar
- 当前 `runs/grid_3d_raster_0_50_100ut_validation_live` 已完成 `3D 0/50/100 uT` 单轮真机验证：`27/27 points passed`、`collector_timeout_total=0`
- continuity audit 现在即使没有 `frames.parsed`，也能直接从 `raw + frames.idx` 重建逐帧结构化数据做审计
