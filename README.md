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

当前可直接作为后续开发输入的配置示例：

- `configs/stations/lab_a.json`
- `configs/stations/lab_a.example.json`

其中 `lab_a.json` 直接保存当前实验室默认端口、地址和 SN，优先用它做真机验证；`lab_a.example.json` 只保留为更宽松的参考模板。

但要明确：

- 串口路径不是设备真值，只是最近一次成功时的 hint
- `station verify` 和 `hardware smoke` 会先尝试 hint
- hint 失败后会自动枚举当前串口并逐个发送设备 probe 指令完成重绑

当前已经打通的最小链路：

- `crates/*-commands`：设备命令 helper
- `crates/*-transport`：最小连接层
- `crates/station-resolver`：基于 hint + 串口自动扫描回退的 station verify 与 snapshot 生成
- `apps/odmr-cli`：CLI 入口

当前已经真机验证过的命令和链路见：

- `docs/rebuild/08_verified_command_and_runtime_baseline.md`

当前可执行命令：

```bash
cargo run -p odmr-cli -- station verify --station configs/stations/lab_a.json
cargo run -p odmr-cli -- station verify --station configs/stations/lab_a.json --out out/station_snapshot.json
cargo run -p odmr-cli -- hardware smoke --station configs/stations/lab_a.json --out-dir out/hardware_smoke/manual
cargo run -p odmr-cli -- run execute \
  --station configs/stations/lab_a.json \
  --calibration configs/calibrations/main.json \
  --plan configs/plans/minimal_3point_runtime.json \
  --smb-profile configs/profiles/smb100a_run_pll_default.json \
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json \
  --out-dir runs/manual
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
  - baseline lock policy
  - 3 个 `target_b_nt` point 的最小运行样例

真机 verify 准备见：

- `docs/rebuild/07_station_verify_and_hardware_checklist.md`

当前 smoke 的核心需求相关流程固定为：

- `RF + Mag + OE`
- `Laser` 只做固定 `OFF` 背景控制验证，不进入开启输出路径

下一阶段 runtime 的固定方向：

- 固定 profile + SMB100A 浮动参数
- `OE1022D` 采用 run 级单 reader `RALL?` collector
- collector 只在 `run execute` 打开后启动，不在“仅连接设备”阶段常驻拉取
- point 线程只按时间窗从 ring buffer 拉数据，并在 point 侧完成最小 `RALL` 字段解析
- 当前已经接入 `target_b_nt -> calibration -> target_current_a` 链路
- 当前已产出 `point_fields.jsonl`，每个 point 都有字段级 `B-X/B-Y/B-Freq/B-Noise/AUXADC1..4` 数据与 PLL 状态摘要
