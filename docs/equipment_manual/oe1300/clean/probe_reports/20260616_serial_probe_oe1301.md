# OE1301 串口命令完整验证报告

> 探测时间：2026-06-16  
> 探测接口：Mac USB-RS232 → `/dev/cu.usbserial-B0027SH3`  
> 波特率：`115200`（默认）  
> 终止符：`\r`  
> 设备身份：`SSI LIA-OE1301, SN:L2283261, Ver:V1.4h4-1.02-1.4.0.23B`  
> 命令来源：`reverse_application/oe1351-labview/` 原厂 LabVIEW VI 导出 + `docs/equipment_manual/oe1300/clean/chapter_05.md` 手册  
> 原始数据：`/tmp/oe1301_comprehensive/query_probe.jsonl`、`/tmp/oe1301_comprehensive/set_probe.jsonl`

---

## 1. 探测方法

从 LabVIEW `Configure Ethernet.vi` 的命令枚举中提取全部 139 条命令基码，再补充手册中的 `*PID`、`BAUD`、`ALRM`、`KCLK`、`OUTX`、`PORT`、`OVRM`，共 **146 条基码**。

对每条基码生成：

- 无参数查询：`CMD?`（`*IDN?`、`*RST?` 等特殊处理）
- `D` 后缀查询：`CMDD?`
- 多参数查询（带通道 0）：`PHAS? 0`、`DMOD? 0`、`HARM? 0`、`DARB? 0`、`COUT? 0`、`CAUX? 0`、`COFP? 0`、`CEXP? 0`、`OAUX? 0`、`OUTP? 0`、`*PID? 0,1`
- `SNAP? 0,1`、`SNAP? 0,1,2,3`

共 **156 条查询命令**，逐一发送并记录响应/超时。每次命令前清空串口接收缓冲区，超时 1.0 s。

危险命令 `*RST`、`RSET`、`OUTX` 不参与自动探测，避免触发重启或 Boot Loader 输出。

---

## 2. 查询探测结果

| 指标 | 数值 |
|------|------|
| 查询总数 | 156 |
| 正常响应 | 49 |
| 超时 | 107 |

### 2.1 真实存在的查询命令（49 条）

```text
*IDN?
*PID? 0,1
*PLL?
AGAN?
ARNG?
BAUD?
CAUX?      CAUX? 0
CEXP?      CEXP? 0
COFP?      COFP? 0
COUT? 0
DARB?      DARB? 0
DMOD?      DMOD? 0
FMOD?
FREQ?
GNOV?
HARM?      HARM? 0
ICPL?
INOV?
IRNG?
ISRC?
NGWA?
NIPA?
NMAC?
NMOD?
NSMA?
OAUX?      OAUX? 0
OFLT?
OFSL?
OUTP? 0
OVLD?
PHAS?      PHAS? 0
RALL?
RMOD?
RSLP?
SENS?
SLVL?
SNAP? 0,1
SNAP? 0,1,2,3
SWVT?
SYNC?
TEMP?
```

### 2.2 不存在的命令 / 幻觉（107 条超时）

**所有 `D` 后缀查询**均不存在：

```text
*IDND?  *PLLD?  *RSTD?
AGAND?  APHSD?  ARSVD?  ASCLD?
CAUXD?  FMODD?  FREQD?  GNOVD?  HARMD?  ICPLD?
IGND?   IGNDD?  ILIN?   ILIND?  INHZ?   INOVD?  ISRCD?
OEXPD?  OFLTD?  OFSLD?  OUTPD?
PHASD?  RALLD?  RESTD?  RMODD?  RSETD?  RSLPD?
SENSD?  SLEND?  SLLMD?  SLVLD?  SNAP?   SNAPD?
SPEDD?  SPRMD?  SPTSD?  SRATD?  SSET?   SSETD?
SSLED?  SSLGD?  SSLLD?  STLMD?  STRDD?  STRGD?
SULMD?  SVDCD?  SVLLD?  SVRMD?  SVSGD?  SVSLD?
SVTMD?  SVULD?  SWPT?   SWPTD?  SWRMD?  SWVTD?
SYNCD?  TRCA?   TRCAD?
```

**手册有但设备不支持的命令**：`IGND?`、`ILIN?`。

**LabVIEW 枚举中的高级/未公开命令全部不存在**：

```text
ALRM?   APHS?   ARSV?   ASCL?   CSPE?   DEQU?   DHCP?
EQCD?   EQCS?   FPOP?   OEXP?   PAUS?   SADD?
SLEN?   SLLM?   SPED?   SPRM?   SPTS?   SRAT?   SSET?
SSLE?   SSLG?   SSLL?   STLM?   STRD?   STRG?   SULM?
SVLL?   SVRM?   SVSG?   SVSL?   SVTM?   SVUL?   SWPT?
SWRM?   TRCA?   RSTU?   INHZ?   KCLK?   OVRM?   PORT?
```

> 结论：这些高级命令在当前 OE1301 固件中**未实现**，属于 LabVIEW 驱动枚举与固件实际能力不一致的部分。第一版应全部排除。

---

## 3. set 命令验证

对存在对应查询的命令，采用“查询 → set → 再查询”三段式验证。OE1301 的 set 命令**全部静默执行**（空响应，无 ACK），需要靠读回判断。

### 3.1 经验证有效的 set 命令

| 命令 | 读回验证 | 备注 |
|------|----------|------|
| `ISRC 1` | `ISRC?` 0→1 | |
| `ICPL 0` | `ICPL?` 1→0 | |
| `IRNG 10` | `IRNG?` 20→10 | |
| `FMOD 1` / `FMOD 2` | `FMOD?` 变化 | |
| `PHAS 0,45.0` | `PHAS? 0` 0→45 | |
| `RSLP 0` | `RSLP?` 变化 | |
| `HARM 0,2` | `HARM? 0` 变化 | |
| `DMOD 0,1` | `DMOD? 0` 变化 | |
| `DARB 0,2000` | `DARB? 0` 变化 | |
| `SENS 15` | `SENS?` 变化 | |
| `OFLT 0.1` | `OFLT?` 变化 | |
| `OFSL 0` | `OFSL?` 变化 | |
| `SWVT 1` | `SWVT?` 变化 | |
| `SLVL 0.1` | `SLVL?` 变化 | |
| `COUT 0,1` | `COUT? 0` 变化 | |
| `CAUX 0,1.0` | `CAUX? 0` 变化 | |
| `COFP 0,50` | `COFP? 0` 变化 | |
| `CEXP 0,10` | `CEXP? 0` 变化 | |

### 3.2 有条件生效的 set 命令

| 命令 | 验证结果 | 说明 |
|------|----------|------|
| `FREQ 1000` | 仅在 `FMOD=1`（内部参考）时生效 | 在 `FMOD=2`（自参考）时 `FREQ?` 返回实测信号频率，不受 `FREQ` set 影响 |
| `SYNC 1` | 未生效（测试时 `OFSL=0`） | 手册要求低通陡降 ≥ 18 dB/oct 且参考频率 < 1 kHz；需在 `OFSL≥3` 时重测 |

### 3.3 不生效的 set 命令

| 命令 | 现象 | 说明 |
|------|------|------|
| `RMOD 1` | `RMOD?` 不变 | 当前固件不支持或该参数被锁定 |
| `AGAN 0` | `AGAN?` 不变；该命令本身可能不是有效 set | 疑似枚举冗余 |
| `ARNG 0` | `ARNG?` 不稳定/不变 | 疑似枚举冗余 |

### 3.4 未验证/危险 set

```text
*RST      # 触发 Zynq 全系统重启，约 3 s
RSET      # 同样触发重启 / Boot Loader 输出
OUTX      # 触发 Boot Loader 输出
BAUD      # 改变波特率会断开当前串口会话，未验证
NMOD/NIPA/NSMA/NGWA/NMAC  # 改变网络参数，未在此轮验证
```

---

## 4. 命令存在性判定标准

本次探测对“命令是否存在”给出明确结论：

- **存在**：至少有一种查询形式能稳定返回有效数据。
- **不存在（幻觉）**：所有查询形式均超时，且多次复测一致。
- **存疑**：`AGAN?`/`ARNG?` 偶尔返回数值但不稳定，结合对应 set 不生效，判定为**枚举冗余或行为异常**，不建议第一版使用。

---

## 5. 关键结论

1. **OE1301 串口远不是“只读”**：大量参数可通过 set 命令修改，但所有 set 均无 ACK，必须读回验证。
2. **LabVIEW 枚举 ≠ 固件能力**：大量高级命令（SWPT/SLLM/SULM/SSLL/SSLG/STLM/SWRM/SVxx/SPED/SRAT/SLEN/SSLE/STRG/SPRM/STRD/PAUS/SPTS/TRCA/RSTU/INHZ/DEQU/SADD/CSPE/EQCD/EQCS/FPOP/OEXP）在当前固件上均未实现。
3. **D 后缀查询全部不存在**。
4. **IGND/ILIN 手册有但设备不支持**。
5. **可用命令白名单**（第一版建议）见 `docs/equipment_manual/truth/oe1300/operational-notes.md`。
