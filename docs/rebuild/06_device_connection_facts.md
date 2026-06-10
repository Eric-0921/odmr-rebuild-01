# 设备连接事实

本文只记录已经从旧项目、真机快照、bring-up 审计中提炼出的最小连接事实。它的用途不是定义 runtime，而是给下一阶段的 transport 和 station verify 提供可信输入。

原则：

- 只复用旧项目里已经被真实设备验证过的连接事实。
- 不复用旧项目的 step 生命周期、GUI 工作流和 runtime 抽象。
- 端口路径只能是 hint，不能作为永久身份。
- 新项目中，设备连接统一由 Rust 负责；Python 不直接连接任何设备。

本次 rebuild 真机 smoke 已经证明：

- 串口设备可以通过“先枚举当前串口池，再按身份 probe 认领”的方式稳定绑定
- `station verify` 和 `hardware smoke` 已经能覆盖 `RF + Mag + OE + Laser OFF`
- `run execute` 也已经再次证明：实际运行应信当次 probe/identity 认领结果，不应信旧 hint 端口

## 总体结论

### Rust / Python 边界

- Rust 负责：
  - socket / serial transport
  - 设备身份探测
  - station verify
  - 运行时 collector 和 cleanup
- Python 负责：
  - plan 生成
  - calibration 拟合
  - artifact 读取和分析

这意味着“连接设备”这件事第一版必须落在 Rust，不应由 Python 承担。

### 设备身份原则

- SMB100A：协议层 `*IDN?` + TCP `5025`
- OE1022D：协议层 `*IDN?`，串口路径只作 hint
- M8812：协议层 `*IDN?` 第 3 字段 SN 严格匹配
- CNI Laser：无稳定 `*IDN?`，依赖协议帧回显、USB 信息和人工 claim

当前代码基线已经进一步收紧为：

- `station verify` / `run execute` 每次都先枚举当前串口池
- hint 只影响候选排序
- `resolved_spec` 和 `station_snapshot.json` 必须写出本轮实际认领端口

### 本次 rebuild 真机已验证结果

- 验证产物：
  - `out/hardware_smoke/manual_scan/station_snapshot.json`
  - `out/hardware_smoke/manual_scan/hardware_smoke_events.jsonl`
  - `out/hardware_smoke/manual_scan/hardware_smoke_command_audit.jsonl`
- 当前真实逻辑轴到 SN：
  - `mag_x -> 080020960220402020`
  - `mag_y -> 080020960220402022`
  - `mag_z -> 080020960220402003`
- 当前 smoke 中 `OE1022D RALL?` 单帧 payload：
  - `15168 bytes`
- 当前最小 runtime `run execute` 中，定长 `RALL?` 真值帧：
  - `12288 bytes`
- 当前 station snapshot 新字段：
  - `transport_hint`：原始配置 hint
  - `resolved_transport`：本轮真实认领到的端口

这两个数字并不矛盾，原因必须写清楚：

- `15168` 来自早期 smoke 的 `query_rall_frame_until_timeout(max_bytes)` 路径
- `12288` 来自当前 runtime 的 `query_rall_frame(expected_len=12288)` 定长读取路径
- 对第一版 runtime 来说，`12288 bytes` 才是 collector 和 parser 应锁定的协议真值
- `until_timeout` 路径只保留给 smoke / 诊断，不能当作 runtime 帧协议

## SMB100A

### 已验证连接事实

- transport：TCP raw socket
- 默认端口：`5025`
- 最小身份命令：`*IDN?`
- 最小健康查询：
  - `SYST:ERR?`
  - `OUTP?`
- 真实快照中的身份响应：
  - `Rohde&Schwarz,SMB100A,1406.6000k02/101623,3.1.19.15-3.20.390.24`

### 已验证最小探测顺序

1. `*IDN?`
2. `SYST:ERR?`，必要时清空 error queue
3. `OUTP?`
4. 需要时再查：
   - `FREQ?`
   - `POW?`
   - `SWE:FREQ:STEP?`
   - `SWE:FREQ:DWEL?`

### 本次 rebuild smoke 已验证最小命令序列

1. `*IDN?`
2. `SYST:ERR?`
3. `OUTP?`
4. `FREQ?`
5. `POW?`
6. `SWE:FREQ:STEP?`
7. `SWE:FREQ:DWEL?`

### 连接约束

- 第一版主 transport 固定为 Raw Socket，不以 VISA 作为必需依赖。
- auto-discovery 可以做有限 TCP 探测，但 runtime 不应依赖全网扫描。
- query interrupted 会污染 error queue，因此连接层必须提供“清空错误队列直到 `0,\"No error\"`”的能力。

### 新项目应直接继承的事实

- `smb100a` transport 用 Rust 实现 TCP socket。
- 最小 verify 以 query-only 为主，不在 verify 阶段发 `OUTP ON`、`SWE:EXEC` 等状态改变命令。

## OE1022D

### 已验证连接事实

- transport：USB CDC 串口
- 旧项目稳定工作参数：`921600` baud
- 最小身份命令：`*IDN?`
- run 级核心读取命令：`RALL?`
- 真机快照中的身份响应示例：
  - `SSI LIA-OE1022D,SN:D6522078,Version:Ver6.3200831`

### 已验证最小探测顺序

1. 打开串口
2. `*IDN?`
3. `port.clear(Input)` 清输入缓存
4. 仅在只读核验中查询少量 setup 状态
5. 真正采集时只保留 `RALL?`

### 本次 rebuild smoke 已验证最小命令序列

1. `*IDN?`
2. `port.clear(Input)`
3. `RALL?`

### 已验证采集约束

- `RALL?` 是唯一核心数据路径。
- 同一串口只能有一个 reader。
- 不支持 pipeline：连续排队多个 `RALL?` 会返回垃圾或空数据。
- 旧项目稳定性验证基于：
  - run 级单 collector
  - 独立 producer
  - 持续 consumer drain
- 旧项目 collector 的关键经验：
  - 采集前 `clear(Input)`
  - 读取节拍约 `48ms`
  - 去重可基于 `X[0]`

### 本次 rebuild 真机已验证事实

- `*IDN?` 已在 rebuild smoke 中重验
- `RALL?` 单帧已在 rebuild smoke 中重验
- 当前单帧 `payload_len = 15168`

### 新项目应直接继承的事实

- OE 连接、collector、cleanup 必须用 Rust 实现。
- Python 不直接碰 OE 串口。
- point 切换不允许重启 collector。

## M8812

### 已验证连接事实

- transport：USB-to-serial
- 当前旧项目稳定参数：`9600`, `8N1`, no flow control
- DTR：`true`
- 典型 read timeout：`100ms` 到 `300ms`
- 最小身份命令：`*IDN?`
- 身份响应形态：
  - `MAYNUO,M8812,<SN>,V2.7`

### 已验证 SN 到逻辑轴映射

- X：`080020960220402020`
- Y：`080020960220402022`
- Z：`080020960220402003`

### 本轮运行重新认领到的真实动态端口

`2026-06-11` 的 recheck / runtime 中，当前真实可用端口为：

- `mag_x -> /dev/cu.PL2303G-USBtoUART11220`
- `mag_y -> /dev/cu.PL2303G-USBtoUART11210`
- `mag_z -> /dev/cu.PL2303G-USBtoUART11230`

这再次证明：

- `station.json` 里的 `/dev/cu.PL2303G-*` 只能是 hint
- 之前讨论过的 `1310/1320/1330` 已经失效
- runtime 必须始终以当次 `station verify` 的 probe 认领结果为准

### 已验证最小初始化顺序

1. `*IDN?`
2. `SYST:REM`
3. `VOLT 75`
4. `CURR 0`
5. `OUTP 0`

### 本次 rebuild smoke 已验证最小命令序列

1. `*IDN?`
2. `SYST:REM`
3. `CURR 0.00000`
4. `OUTP 0`
5. `MEAS:CURR?`
6. `CURR 0.01000`
7. `OUTP 1`
8. `MEAS:CURR?`
9. `OUTP 0`
10. `CURR 0.00000`
11. `SYST:LOC`

### 已验证运行期最小命令

- `CURR {:.5f}`
- `OUTP 1`
- `OUTP 0`
- `MEAS:CURR?`
- `SYST:LOC`

### 已验证 cleanup 倾向

旧项目里同时出现过两种顺序：

- `CURR 0 -> OUTP 0 -> SYST:LOC`
- `OUTP 0 -> CURR 0 -> SYST:LOC`

这说明“最终要归零、关输出、回本地”是确定的，但顺序在不同 bring-up 阶段曾有变化。新项目应把 cleanup 顺序当成一个待最终敲定的 transport/runtime 设计点，而不是盲目照抄某一个旧实现。

### 新项目应直接继承的事实

- M8812 连接必须用 Rust serial transport。
- station verify 依赖 `*IDN?` 严格匹配 SN，不依赖 `/dev/cu.PL2303G-*` 这类动态路径。
- 第一版 point 运行期只需要：
  - `CURR`
  - `OUTP`
  - `MEAS:CURR?`
- `VOLT 75` 和保护阈值属于 station/profile setup，不属于 point 变量。

## CNI Laser

### 已验证连接事实

- transport：RS232 / serial
- 参数：`9600`, `8N1`, no parity
- 协议：二进制帧，不是 SCPI
- 最小帧：
  - Laser Off：`55 AA 03 00 03`
  - Laser On：`55 AA 03 01 04`
  - Set Power：`55 AA 05 01 [hi] [lo] [checksum]`

### 已验证协议事实

- 校验和规则：低 8 位求和
- 功率 100mW 示例：
  - `55 AA 05 01 00 64 6A`
- 旧项目把它当作低可观测写入型设备处理，而不是可靠可读回的 SCPI 设备。

### 本次 rebuild smoke 已验证最小命令序列

1. `55 AA 03 00 03`
2. echo `55 AA 03 00 03`

### 新项目应直接继承的事实

- Laser 连接也由 Rust 负责。
- 第一版只需要最小控制：
  - 设功率
  - 开
  - 关
- 不把它当作有完整状态 readback 的设备。

## 下一阶段落地建议

下一阶段不要直接写 runtime，先做最小 transport：

1. `smb100a` Raw Socket transport
2. `m8812` serial transport
3. `oe1022d` serial transport
4. `cni_laser` serial transport

每个 transport 第一版只要支持：

- `open`
- `close`
- `send`
- `query`
- timeout / terminator 基础配置
- 最小身份验证

等这层稳定，再做 station resolver 和 runtime。
