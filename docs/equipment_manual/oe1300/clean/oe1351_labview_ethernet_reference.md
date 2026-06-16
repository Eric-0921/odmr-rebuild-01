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
- 当前真机（固件 `V1.3230310`）两个默认 IP 均不可达，故第一版不实现网口。

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

1. VISA Write 命令
2. 等待 **5 ms**
3. VISA Read（缓冲区 32768 B）
4. 调用 `DATA Transmit.vi` 解析

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

## 4. `RALL?` 数据解析

### 4.1 返回格式

- ASCII CSV，37 个浮点数
- 以 `\r` 结尾
- 示例长度约 522 B

### 4.2 字段索引

与手册一致：

| 索引 | 参数 |
|---:|---|
| 0 | X |
| 1 | Y |
| 2 | R |
| 3 | θ |
| 4–7 | XD1, YD1, RD1, θD1 |
| 8–11 | XD2, YD2, RD2, θD2 |
| 12–15 | XD3, YD3, RD3, θD3 |
| 16–19 | XD4, YD4, RD4, θD4 |
| 20–23 | XD5, YD5, RD5, θD5 |
| 24–27 | XD6, YD6, RD6, θD6 |
| 28–31 | XD7, YD7, RD7, θD7 |
| 32 | X-Noise |
| 33 | Y-Noise |
| 34 | Frequency |
| 35 | AUX-IN1 |
| 36 | AUX-IN2 |

### 4.3 LabVIEW 解析方式

`RS232_Query Data.vi` 使用 C-style `sscanf` 格式串：

```c
"%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g,%g"
```

共 37 个 `%g`。

---

## 5. 若未来网口可用时的实现要点

1. **建立 TCP 连接**：`socket.connect((device_ip, 10001))`，超时 3–5 s。
2. **发送命令**：`command + "\r"`，使用同一个 socket（保持连接）。
3. **读取响应**：
   - 写完后 sleep 5 ms（数据查询）或 50 ms（设置查询）。
   - 读取到 `\r` 为止，或设置 32768 B 缓冲区上限。
4. **解析 `RALL?`**：按 37 字段 CSV 用浮点解析。
5. **连续性审计**：RALL? 帧内无 packet counter；若需审计，可在应用层用时间戳或发送序号。
6. **不要通过串口配置网口**：当前固件该路径不可靠；应假设网口 IP 已预先配好。

---

## 6. 当前固件限制

- 网口默认 IP 不可达（`192.168.0.1` 和 `192.168.1.1` 均无 ARP 响应）。
- 串口网络配置命令中仅 `NMOD` 有响应。
- **串口基本只读**：`ISRC`/`FREQ`/`OFLT`/`ICPL` 等 set 命令均无响应，无法通过串口修改运行参数。
- 因此第一版实现应：**仅通过串口读取 `RALL?` 和状态查询，设备配置在实验前由人工/Console 完成，网口留作后续升级**。
