# OE1300 系列（OE1301/OE1351）运行事实

> 来源：
> - `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md`
> - 真机探测报告 `docs/equipment_manual/oe1300/clean/probe_reports/20260616_serial_probe.md`（OE1351）
> - 真机探测报告 `docs/equipment_manual/oe1300/clean/probe_reports/20260616_serial_probe_oe1301.md`（OE1301 完整版）
> - 真机探测报告 `docs/equipment_manual/oe1300/clean/probe_reports/20260616_lan_probe.md`
>
> 已验证设备：
> - `OE1351`（SN:L2092228，固件 `V1.3230310`）
> - `OE1301`（SN:L2283261，固件 `V1.4h4-1.02-1.4.0.23B`）

---

## Session stance

- 命令通道为 ASCII，**终止符为 `\r`（0x0D）**。
- 多条命令可用 `;` 分隔，但第一版建议保持简短、单条发送。
- 默认串口波特率：`115200`（可改 `9600` / `921600`）。
- 校验位：无；数据位：8；停止位：1。
- **实际响应延迟约 20–80 ms/命令**；早期探测报告的 1.0–1.5 s 是读取策略等超时的假象。
- `*RST` 触发全系统重启，耗时约 3 s；`RSET` / `OUTX` 也会触发 Boot Loader 输出。**正常使用避免发送**。
- 串口接收缓冲区限制为 64 byte（手册 4.3.2 节），发送长命令或多条命令时需注意。

---

## Transport options

### 串口（当前唯一可用的 ODMR 采集方式）

#### OE1351（固件 V1.3230310）

- 已验证可用的查询命令（26 条）：`*IDN?`、`*PLL?`、`ISRC?`、`ICPL?`、`IRNG?`、`FMOD?`、`FREQ?`、`PHAS?`、`RSLP?`、`OFLT?`、`OFSL?`、`SYNC?`、`HARM?`、`DMOD?`、`DARB?`、`BAUD?`、`OVLD?`、`RALL?`、`CAUX?`、`SWVT?`、`SLVL?`、`OAUX?`、`COUT?`、`COFP?`、`CEXP?`、`TEMP?`。
- 单命令延迟约 20–80 ms；100 次连续 `RALL?` 实测平均吞吐 **~18 Hz**。
- **所有 set 命令均无响应且不生效**（`ISRC 0`、`FREQ 1000`、`OFLT 0.01` 等）。
- `IGND?`、`RMOD?`、`SNAP?`、`OUTP?`、全部 D 后缀命令、大量高级命令超时。
- **关键限制**：串口基本只读，无法通过串口修改运行参数。

#### OE1301（固件 V1.4h4...）

- 已验证可用的查询命令（49 条），包括：
  - 基础：`*IDN?`、`*PLL?`、`*PID? 0,1`、`BAUD?`、`TEMP?`、`OVLD?`
  - 输入：`ISRC?`、`ICPL?`、`IRNG?`、`INOV?`、`GNOV?`
  - 参考/相位：`FMOD?`、`FREQ?`、`PHAS?`/`PHAS? 0`、`RSLP?`、`HARM?`/`HARM? 0`
  - 解调器：`DMOD?`/`DMOD? 0`、`DARB?`/`DARB? 0`
  - 滤波：`OFLT?`、`OFSL?`、`SYNC?`、`SENS?`
  - 输出：`SWVT?`、`SLVL?`、`COUT? 0`、`CAUX?`/`CAUX? 0`、`COFP?`/`COFP? 0`、`CEXP?`/`CEXP? 0`、`OAUX?`/`OAUX? 0`、`OUTP? 0`
  - 数据：`RALL?`、`SNAP? 0,1`、`SNAP? 0,1,2,3`
  - 网络：`NMOD?`、`NIPA?`、`NSMA?`、`NGWA?`、`NMAC?`
  - 其他：`AGAN?`、`ARNG?`
- **set 命令大部分静默生效**（空响应，无 ACK），必须读回验证。
- 连续 `RALL?` 实测吞吐 **~16.6 Hz**。

### 网口（当前不适合 ODMR 主采集）

#### OE1301

- TCP Client 到 `192.168.1.1:10001` **可连接**；ping 可达，约 0.5 ms。
- 文本查询（`*IDN?`、`FREQ?`、`*PLL?`）返回正常 ASCII。
- **致命问题**：`RALL?` 和 `SNAP?` 通过 TCP 返回二进制数据（`RALL?` 可读出 32768 B，`SNAP?` 返回 4096 B），内容无意义/重复，无法解析为 37 字段 CSV。
- 对照 LabVIEW 驱动：`Ethernet__Query_Data.vi` 按 32768 B 读取并交给 `DATA Transmit.vi` 以 **400 B/帧、8 B 小端 double、固定偏移** 解析；OE1301 的 32768 B 缓冲区按该格式解析仍得到无意义数值，且尾部残留 `*IDN?` 字符串，说明当前固件网口数据路径不可用。
- **环境限制说明**：当前探测基于 Mac 裸 TCP socket。原厂 LabVIEW 驱动依赖 Windows NI-VISA，Mac 没有 NI-VISA 运行时；读终止符、缓冲策略可能与原厂驱动不同。要排除“Mac 环境导致”这一变量，需在 Windows + NI-VISA（C# 主栈）上复测。
- 因此网口**不能替代串口**作为 ODMR 采集路径。

#### OE1351

- 无论 Mac 配置 `192.168.0.0/24` 还是 `192.168.1.0/24`，设备均无 ARP/MAC 响应。
- 串口网络配置命令中仅 `NMOD 0` 返回 `"Setting Ethernet Mode Success!"`，其余超时。
- 第一版放弃网口。

---

## `RALL?` acquisition rule

- **串口**：返回 **37 个 ASCII 浮点数的 CSV 字符串**，以 `\r` 结束，长度约 520–560 B。
- **网口（OE1301）**：返回固定长度二进制数据，当前固件不可解析。
- 字段顺序：0=X, 1=Y, 2=R, 3=θ, 4=XD1, 5=YD1, 6=RD1, 7=θD1, ..., 34=Frequency, 35=AUX-IN1, 36=AUX-IN2。
- 与 OE1022D 的 12288 B 二进制帧完全不同；**不可复用 OE1022D 的采集路径**。
- 目前未发现类似 OE1022D `payload[12287]` 的 packet counter。若需连续性审计，需通过时间戳或序列号在应用层实现。
- 连续 `RALL?` 实测吞吐约 **16–18 Hz**。

---

## First-version role

### OE1351

- **固定配置的只读观察者**。
- 实验前通过前面板或 Console 软件配置参数。
- C# runtime 只发查询命令（主要是 `RALL?`）。

### OE1301

- **可配置的串口采集源**。
- Runtime 可在 point 边界下发允许的 set 命令，并读回验证。
- 无法串口修改的参数（`RMOD`）仍需人工预配置。

与 OE1022D 的关键差异：

- OE1022D：二进制 `RALL?` 帧，可高速连续采集，含 packet counter。
- OE1300/1351：ASCII CSV `RALL?`（串口），采集速度受串口吞吐限制，但 16–18 Hz 已可满足多数 ODMR 场景。

---

## Configuration stance

### OE1351

当前固件串口基本只读，因此第一版**不通过 runtime 下发配置命令**。设备参数应通过前面板或 Console 软件在实验前设置好。

### OE1301

允许 runtime 下发的参数（set 后读回验证）：

```text
ISRC n
ICPL n
IRNG n
FMOD n
PHAS i,x
RSLP n
HARM i,j
DMOD i,j
DARB i,f
SENS n
OFLT x
OFSL n
SWVT n
SLVL x
COUT i,j
CAUX i,x
COFP i,x
CEXP i,j
FREQ f        # 仅在 FMOD=1 内部参考模式时生效
```

必须在实验前人工配置或当前固件不支持的参数：

```text
RMOD n        # 串口 set 不生效
SYNC n        # 受 OFSL/频率限制，可能不生效
FREQ f        # 在 FMOD≠1 时不生效
```

---

## First-version command whitelist

### 查询（OE1301）

```text
*IDN?   *PLL?   *PID? i,j
ISRC?   ICPL?   IRNG?   INOV?   GNOV?
FMOD?   FREQ?   PHAS?   PHAS? i   RSLP?   HARM?   HARM? i
DMOD?   DMOD? i   DARB?   DARB? i
SENS?   OFLT?   OFSL?   SYNC?
SWVT?   SLVL?   COUT? i   CAUX?   CAUX? i   COFP?   COFP? i
CEXP?   CEXP? i   OAUX?   OAUX? i   OUTP? i
RALL?   SNAP? i,j   SNAP? i,j,k,l
OVLD?   TEMP?   BAUD?
NMOD?   NIPA?   NSMA?   NGWA?   NMAC?
```

### 禁用 / 不存在的命令

```text
*RST        # 会触发 3 s 重启
RSET        # 同样触发重启 / Boot Loader 输出
OUTX        # 触发 Boot Loader 输出
BAUD set    # 改变波特率，风险高
所有 D-后缀查询（ISRCD? / FREQD? / OFLTD? ...）
IGND? / ILIN?              # 手册有但设备不支持
SWPT? / SLLM? / SULM? / SSLL? / SSLG? / STLM? / SWRM?
SVLL? / SVUL? / SVSL? / SVSG? / SVTM? / SVRM?
SPED? / SRAT? / SLEN? / SSLE? / STRG? / SPRM? / STRD? / PAUS? / SPTS? / TRCA?
RSTU? / INHZ? / DEQU? / SADD? / CSPE?
EQCD? / EQCS? / FPOP? / OEXP?
ARSV? / ASCL? / APHS?
ALRM? / KCLK? / OVRM? / PORT? / DHCP?
```

---

## Non-goals for first version

- 不通过网口做 `RALL?` / `SNAP?` 采集（OE1301 TCP 返回二进制乱码）。
- 不实现全部 D 后缀命令、扫频、示波器/FFT 相关高级命令。
- OE1351 不通过串口修改任何设备参数。
- 不在热路径中加入 parser、retry、deadline 等额外逻辑（保持与串口响应特性匹配即可）。

---

## Open questions

1. OE1301 的 `RMOD` 是否被前面板锁定？是否存在解锁命令？
2. OE1301 网口 `RALL?` 二进制格式含义是什么？是否与某个 LabVIEW VI 解析表对应？
   - 已对照 `OE1311&OE1351_DATA Transmit.vi`：LabVIEW 期望 **400 B/帧、50×8 B 小端 double**，固定偏移提取 37 字段；OE1301 实际 32768 B 缓冲区不符合该格式，故不是同一协议，当前固件网口数据路径不可用。
3. OE1351 是否可通过升级固件获得与 OE1301 类似的 set 命令支持？
