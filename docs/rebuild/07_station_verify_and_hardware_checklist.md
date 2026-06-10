# Station Verify 与真机连通清单

本文只服务当前阶段目标：

- 用 `odmr-cli` 跑通 `station verify`
- 对 4 类设备做最小真机连通验证准备
- 用 `hardware smoke` 跑通 `RF + Mag + OE` 核心链路，`Laser` 只做 `OFF` 验证

当前已经真机验证过的命令和链路汇总见：

- `08_verified_command_and_runtime_baseline.md`

它不是 run launcher，也不是 runtime checklist。

## 当前命令

```bash
cargo run -p odmr-cli -- station verify --station configs/stations/lab_a.json

cargo run -p odmr-cli -- \
  station verify \
  --station configs/stations/lab_a.json \
  --out out/station_snapshot.json

cargo run -p odmr-cli -- \
  hardware smoke \
  --station configs/stations/lab_a.json \
  --out-dir out/hardware_smoke/manual

cargo run -p odmr-cli -- \
  run execute \
  --station configs/stations/lab_a.json \
  --calibration configs/calibrations/main.json \
  --plan configs/plans/minimal_3point_runtime.json \
  --smb-profile configs/profiles/smb100a_run_pll_default.json \
  --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json \
  --out-dir runs/manual
```

输出行为：

- 不带 `--out`：直接把 `station snapshot` 打到 stdout
- 带 `--out`：把 `station snapshot` 写到文件

串口路径规则：

- `station.json` 里的串口路径只当 hint，不当真值
- `station verify` 会先尝试 hint
- hint 失败后会自动枚举当前串口，并按设备类型逐个发送 probe 指令
- 命中后 snapshot 里会写出这次实际绑定到的端口
- 真机 run 应优先复用这次实际绑定结果，而不是继续相信旧的静态 hint

## station verify 当前做什么

### SMB100A

- 使用 `tcp_socket` hint
- 建连到 `host:port`
- 发送 `*IDN?`
- 在 smoke 中继续发送：
  - `SYST:ERR?`
  - `OUTP?`
  - `FREQ?`
  - `POW?`
  - `SWE:FREQ:STEP?`
  - `SWE:FREQ:DWEL?`
- 用 `identity.contains_all` 或 `identity.exact` 校验身份

### OE1022D

- 使用 `serial_port` hint
- 打开指定串口
- 失败时扫描当前串口候选
- 发送 `*IDN?`
- 在 smoke 中执行：
  - `port.clear(Input)`
  - `RALL?`
- 校验身份

### M8812

- 使用 `serial_port` hint
- 打开指定串口
- 失败时扫描当前串口候选
- 发送 `*IDN?`
- 在 smoke 中逐轴执行：
  - `SYST:REM`
  - `CURR 0.00000`
  - `OUTP 0`
  - `MEAS:CURR?`
  - `CURR 0.01000`
  - `OUTP 1`
  - `MEAS:CURR?`
  - `OUTP 0`
  - `CURR 0.00000`
  - `SYST:LOC`
- 严格校验 SN

### CNI Laser

- 使用 `serial_port` hint
- 打开指定串口
- 失败时扫描当前串口候选
- 发送 `Laser Off` 帧
- 读取固定长度 echo
- 用 echo 作为第一版最小验证

## station verify 当前不做什么

- 不做网络扫描
- 不做自动 claim
- 不做 preflight
- 不做 error queue drain
- 不做 output 状态校验
- 不做 cleanup 编排

这些是下一阶段工作，不属于当前最小设备连接链。

## hardware smoke 当前做什么

- 先执行一次 `station verify`
- `SMB100A` 只做身份、错误队列、输出关闭态和关键 readback
- `mag_x / mag_y / mag_z` 逐轴执行 `10mA` 微测并强制 cleanup
- `OE1022D` 只做单次 `RALL?`
- `CNI Laser` 只做 `off` 帧和 echo 验证
- point / step 不参与采集线程生命周期

固定目标不是“开始实验”，而是把连接参数、终止符、超时、回读和 cleanup 路径全部打实。

## 最小 runtime 当前做什么

- 先执行 `station verify`
- 加载 `calibration`
- 做一次 `Maynuo baseline lock`
- 启动 run 级单 reader `RALL?` collector
- 逐 point 执行：
  - `target_b_nt -> calibration -> target_current_a`
  - `SMB100A` sweep 配置
  - ring buffer 时间窗拉取
  - 最小 `RALL` 字段解析
- cleanup 后确认：
  - `OUTP? -> 0`
  - `FREQ:MODE? -> CW`
  - `SYST:ERR? -> 0,"No error"`

当前已经真机证明：

- collector 不是“设备一连上就启动”，而是 `run execute` 启动后才创建
- point 内 RF 输出保持开启可工作
- 程序结束时 RF output 与 RF frequency sweep state 可一起退出

## 实验室前准备

在去实验室前，先完成这些本地动作：

1. `cargo test --workspace`
2. 优先使用 `configs/stations/lab_a.json`
3. 确认其中的 `host / port_path / SN` 仍然匹配你当前实验室真实值
4. 明确哪些设备这次是 required，哪些是 optional

## 实验室最小核验顺序

建议顺序不要乱，先从最可观测、最简单的设备开始。

1. SMB100A
2. 单台 M8812
3. 其余两台 M8812
4. OE1022D
5. CNI Laser

原因：

- 先验证 TCP socket 和 SCPI query 路径
- 再验证串口 SCPI 路径
- 最后再碰二进制 laser 帧

## 各设备最小通过标准

### SMB100A

通过标准：

- TCP 可连接
- `*IDN?` 返回包含 `Rohde&Schwarz` 和 `SMB100A`
- `SYST:ERR?` 可读且无致命错误
- `OUTP? / FREQ? / POW? / SWE:FREQ:STEP? / SWE:FREQ:DWEL?` 可读

失败时先查：

- IP 是否变了
- 5025 端口是否可达
- 设备是否在 remote 可访问网络上

### M8812

通过标准：

- 串口可打开
- `*IDN?` 返回 `MAYNUO,M8812,<SN>,V2.7`
- SN 与配置一致
- `10mA` 微测后 `MEAS:CURR?` 有合理非零回读
- cleanup 可完成

失败时先查：

- 端口路径是否因重枚举变化
- 目标轴 SN 是否写错
- DTR 是否真的需要开启

### OE1022D

通过标准：

- 串口可打开
- `*IDN?` 返回 `SSI LIA-OE1022D`
- `RALL?` 单帧返回非零 payload

当前阶段不要求：

- 当场跑 collector
- 当场跑长时间 `RALL?` 稳定性

那是下一阶段。

### CNI Laser

通过标准：

- 串口可打开
- `Laser Off` 帧可写入
- 能读回预期 echo

当前阶段不要求：

- 开激光
- 设非零功率
- 验证光功率输出

## 真机 verify 后你应该拿到什么

至少要留下：

- 你实际使用的 `station.json`
- `station_snapshot.json`
- `hardware_smoke_events.jsonl`
- `hardware_smoke_command_audit.jsonl`
- 每台设备的观察到的身份字符串或 echo
- 如果失败，记录失败设备、hint、错误信息

## 下一阶段入口

当以下条件都满足时，再进入 runtime：

- 4 类设备的最小连接路径都被真机验证过
- `station verify` 能稳定输出 snapshot
- 站点配置里的 hint 和 identity 规则不再混乱

在这之前，不要急着写 point loop 或 collector runtime。
