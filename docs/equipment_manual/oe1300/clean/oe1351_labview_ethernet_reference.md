# OE1311/OE1351 LabVIEW 驱动网口/串口协议参考

> 来源：`reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/` 中的原厂 LabVIEW VI 导出文件。
> 用途：记录若未来网口调通后，C#/Python 实现应遵循的协议细节。

---

## 1. 传输层

### 1.1 串口

- 波特率：`115200`（前面板默认值）
- 数据位：8，无校验，1 停止位
- 终止符：`\r`（0x0D）
- 命令与网口共用同一套 ASCII 指令集

### 1.2 网口

- **协议**：TCP Client
- **端口**：`10001`（手册与 LabVIEW 驱动一致）
- **设备 IP**：
  - 手册默认：`192.168.1.1`
  - LabVIEW 驱动默认网关：`192.168.0.1`（暗示 PC 应设为 `192.168.0.10/24`）
- OE1301（固件 `V1.4h4-1.02-1.4.0.23B`）在 `192.168.1.1:10001` 可连接，文本查询正常，但 `RALL?`/`SNAP?` 返回不可解析的二进制数据。
- OE1351（固件 `V1.3230310`）两个默认 IP 均不可达。
- **环境限制**：原厂 LabVIEW 驱动基于 Windows NI-VISA 构建，当前 Mac 环境没有 NI-VISA 运行时；Mac 上使用的裸 TCP socket 可能在终止符处理、读取策略上与 NI-VISA 存在差异。要完全复现原厂驱动行为，需要在 Windows + NI-VISA / C# 主栈上重新验证。
- 因此第一版不实现网口采集。

---

## 2. 命令格式

### 2.1 基本形态

所有命令均为 4 字符大写助记符（`*IDN`、`*RST` 用 `*` 补齐），后接选项参数，以 `\r` 结尾。

示例：

```text
RALL?\r
ISRC 0\r
FREQ 1000\r
```

### 2.2 Set / Query 模式

LabVIEW 的 `SSI_Command.vi` 根据 "Set/Query" 枚举决定附加空格参数还是 `?`：

- Set：`CMD value`（如 `ISRC 0`）
- Query：`CMD?`（如 `ISRC?`）

### 2.3 多命令连接

手册和 LabVIEW 均支持用 `;` 分隔多条命令：

```text
ISRC?;FREQ?;RALL?\r
```

但当前真机对网络配置命令的串接响应异常（只解析第一条），普通查询命令的串接待验证。

### 2.4 LabVIEW 驱动命令枚举

除手册列出的命令外，LabVIEW 还暴露了大量带 `D` 后缀的命令（如 `ISRCD`、`IGNDD`、`ICPLD`…），可能表示“查询默认值”或“显示值”。第一版可暂不使用。

完整枚举包含：

```text
ISRC, IGND, ICPL, ILIN, PHAS, FMOD, RSLP, FREQ, SWPT, SLLM, SULM,
SSLL, SSLG, STLM, SWRM, IRNG, SENS, RMOD, OFLT, OFSL, SYNC, DMOD,
HARM, DARB, SWVT, SLVL, SVLL, SVUL, SVSL, SVSG, SVTM, SVRM,
RALL, SNAP, OUTP, OAUX, COUT, CAUX, COFP, CEXP, FPOP, OEXP,
*IDN, *RST, OVLD, INOV, GNOV, *PLL, SSET, RSET, AGAN, APHS,
ARSV, ASCL, EQCD, EQCS, SPED, SRAT, SLEN, SSLE, STRG, SPRM,
STRD, PAUS, SPTS, TRCA, TEMP, RSTU, INHZ, DEQU, SADD, ARNG, CSPE,
以及对应的 D 后缀版本
```

网络配置专用命令：

```text
NMOD, NIPA, NSMA, NGWA, NMAC
```

---

## 3. 读写时序

### 3.1 串口查询数据

`OE1311&OE1351_RS232_Query Data.vi`：

1. VISA Write 命令
2. 等待 **70 ms**
3. VISA Read（缓冲区 32768 B）
4. 用 `sscanf` 格式字符串 `%g,%g,%g,...` 解析 37 个浮点

### 3.2 网口查询数据

`OE1311&OE1351_Ethernet__Query Data.vi`：

1. VISA Write `RALL?\r`
2. 等待 **5 ms**
3. 进入读取循环：每次 `TCP Read` 最多读 **32768 B**，追加到 `buffer`
4. 当 `buffer` 长度 **≥ 32767 B** 时退出循环
5. 调用 `DATA Transmit.vi` 按二进制 400 B 帧解析

### 3.3 网口查询设置

`OE1311&OE1351_Ethernet__Query Setting.vi`：

1. VISA Write 命令
2. 等待 **50 ms**
3. VISA Read（缓冲区 32768 B）
4. 拆分到 Input Filter、Reference Phase、Demodulator 等簇

### 3.4 串口网络配置

`OE1311&OE1351_Configure Ethernet.vi`：

1. 通过 **RS232** 发送：`NMOD;NIPA;NSMA;NGWA;NMAC` 串接命令
2. 等待 **25000 ms（25 s）**
3. VISA Read 返回

当前真机仅 `NMOD 0` 能在约 6 s 返回 `"Setting Ethernet Mode Success!"`，其余网络命令超时。

### 3.5 复位

`OE1311&OE1351_Configure Reset.vi`：

1. 发送 `*RST\r`
2. 等待 **3000 ms**
3. 设备触发 Xilinx Zynq 全系统重启

---

## 4. 网口 `RALL?` 数据解析（LabVIEW 实际方案）

> 2026-06-16 更新：OE1301 实测表明，网口 `RALL?` 返回的是**二进制数据**，与手册声称的 ASCII CSV 不同。LabVIEW 驱动中的 `Ethernet__Query_Data.vi` 也正是按二进制帧解析，而非 CSV。

### 4.1 整体流程

`OE1311&OE1351_Ethernet__Query_Data.vi`：

1. 通过已建立的 TCP 连接 VISA Write `RALL?\r`。
2. 等待 **5 ms**。
3. 进入读取循环：每次 `TCP Read` 最多读 **32768 B**，把读取结果追加到 `buffer`。
4. 当 `buffer` 长度 **≥ 32767 B** 时退出循环。
5. 将 `buffer` 送入 `OE1311&OE1351_DATA Transmit.vi` 解析为 37 路参数数组，并输出 `PLL&Overload` / `Trig_Count`。

### 4.2 二进制帧格式

`DATA Transmit.vi` 把输入字节流按 **400 B / 帧** 切分，每帧内部为 **50 个 8 B 的 IEEE-754 double（小端）**。LabVIEW 只取其中 37 个，固定偏移如下：

| 偏移（B） | 参数 |
|---:|:---|
| 0 | X |
| 8 | Y |
| 16 | R |
| 24 | θ（Xita） |
| 32 | X-Noise |
| 40 | Y-Noise |
| 48 | Frequency |
| 56 | Xh1 |
| 64 | Yh1 |
| 72 | Rh1 |
| 80 | θh1 |
| 88 | Xh2 |
| 96 | Yh2 |
| 104 | Rh2 |
| 112 | θh2 |
| 120 | Xh3 |
| 128 | Yh3 |
| 136 | Rh3 |
| 144 | θh3 |
| 152 | Xh4 |
| 160 | Yh4 |
| 168 | Rh4 |
| 176 | θh4 |
| 184 | Xh5 |
| 192 | Yh5 |
| 200 | Rh5 |
| 208 | θh5 |
| 216 | Xh6 |
| 224 | Yh6 |
| 232 | Rh6 |
| 240 | θh6 |
| 248 | Xh7 |
| 256 | Yh7 |
| 264 | Rh7 |
| 272 | θh7 |
| 280 | AUX-IN1 |
| 288 | AUX-IN2 |

每帧剩余 112 B（偏移 296–399）未在 VI 中解析，可能为保留/内部数据。

### 4.3 状态字节

在 **30000 B 固定缓冲区** 的尾部：

- 偏移 **29990**：状态字节，`PLL&Overload` 簇从中取两位（分别对应 `PLL Lock` 与 `Input Overload`）。
- 偏移 **29997**：`Trig_Count`（U8）。

### 4.4 与 OE1301 实测结果的差异

OE1301（固件 `V1.4h4-1.02-1.4.0.23B`）通过 TCP 返回的二进制数据**不遵循上述 400 B 帧格式**：

- `RALL?` 单次读出约 **32768 B**，但按 400 B/帧解析后得到的是无意义数值。
- `SNAP? 0,1,2,3` 返回 **4096 B**，呈现重复模式，同样无法解析。
- 缓冲区尾部甚至出现之前 `*IDN?` 的字符串残留（`SSI LIA-OE1301,...`），说明当前固件的网口数据路径可能返回的是未初始化/内存回显，而非真正的采样数据。

因此，**OE1301 的网口数据查询当前不可用**；即便使用原厂 LabVIEW 驱动，也很可能得到同样结果。

---

## 5. 若未来网口调通时的实现要点

1. **TCP 连接**：`socket.connect((device_ip, 10001))`，超时 3–5 s，保持长连接。
2. **发送命令**：`command + "\r"`，单条发送。
3. **读取 `RALL?`**：写完后 sleep 5 ms，然后循环读取直到累积到 32767 B（或自行定义帧数 × 400 B）。
4. **解析**：按 **400 B 帧、8 B 小端 double、固定偏移** 解析，不要当 CSV 处理。
5. **状态/触发**：从偏移 29990/29997 取状态位和触发计数。
6. **连续性审计**：帧内没有类似 OE1022D 的 packet counter；若需审计，在应用层用时间戳/发送序号。

---

## 6. 当前固件限制

- **OE1351（固件 `V1.3230310`）**：网口默认 IP 不可达，串口网络配置命令基本无效，第一版放弃网口。
- **OE1301（固件 `V1.4h4-1.02-1.4.0.23B`）**：网口 TCP/10001 可达，文本命令正常，但 `RALL?`/`SNAP?` 返回的二进制数据无法解析，且与 LabVIEW 驱动的 400 B 帧格式不匹配。
- **串口方面**：OE1301 的 set 命令大部分静默生效，OE1351 的 set 命令不生效。
- **工程决策**：第一版仅使用串口 ASCII `RALL?` 作为 ODMR 采集路径；网口留作后续固件/协议澄清后再启用。
