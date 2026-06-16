# OE1301 本机联机复核记录（2026-06-17）

> 目的：
> - 复核 PDF 翻译到 Markdown 的可靠性
> - 复核串口 / 网口连接
> - 复核命令行为
> - 复核 `RALL?` 采集逻辑，并和 LabVIEW VI 导出图交叉验证
>
> 本次复核机器：
> - 仓库工作区：`/Users/erictseng/Documents/odmr-rebuild-01`
> - 串口：`/dev/cu.usbserial-B0027SH3`
> - 网卡：`en5 = 192.168.1.10/24`
>
> 原始探测输出：
> - `runs/oe1301_live_verify_20260617/serial_query_probe.json`
> - `runs/oe1301_live_verify_20260617/tcp_probe.json`

---

## 1. 先说结论

- 用户口述是 “`oe1351` 已接入本机”，但 **2026-06-17 当场实机读回身份不是 OE1351，而是 `OE1301`**：

```text
*IDN? -> SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B
```

- **PDF → 清洗版 Markdown** 在“命令语法、网口默认地址、串口协议、`RALL?` 字段表”这几块上是可用的。
- **PDF → 原始 OCR Markdown** 不能直接当真值，表格和符号位有明显 OCR 污染。
- **串口链路可用**，`RALL?` 返回 **37 字段 ASCII CSV**，当前实测连续吞吐约 **16.26 Hz**。
- **网口链路可用**，`192.168.1.1:10001` 可连，文本查询正常。
- **网口 `RALL?` 不可直接用**：返回 **32768 B 二进制**，且与 LabVIEW `DATA Transmit.vi` 假定的 400 B frame 结构对不上。
- 因而当前这台设备的工程结论不变：**第一版 runtime 只能用串口 ASCII `RALL?` 做采集，不能用 TCP `RALL?` 做主链。**

---

## 2. PDF 翻译到 Markdown 的可靠性

### 2.1 可认为可靠的部分

我直接看了 PDF 原页渲染图，和 `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md` 对照，以下事实一致：

- 通信接口：RJ45 + RS232/UART，见手册页 26~28。
- 串口配置：`9600/115200/921600`、`8N1`，见手册页 27~28。
- 网口默认：`192.168.1.1`，远程端口 `10001`，见第 7.2 节。
- 命令语法：四位大写助记符 + 参数 + `\r`；多命令可用 `;` 分隔。
- `RALL?`：手册文本描述为单行 ASCII CSV，字段顺序 0~36。

对应的清洗版 Markdown 落点：

- `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md:968`
- `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md:982`
- `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md:1824`
- `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md:1830`
- `docs/equipment_manual/oe1300/clean/oe1300_manual_clean.md:1846`

### 2.2 不能直接信 OCR 原稿的部分

`docs/equipment_manual/oe1300/oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md` 有足够多的 OCR 污染，不能直接作为命令白名单真值：

- `5.1 OE1300 命令语法` 被识别成 `5.10E1300 命令语法`
- `BAUD (?) {j}` 被识别成 `BAUD(?) {}}`
- `θD7` / `θD1` / `θD2` 等字段被识别成 `0D7` / `0D1` / `0D2`

对应行：

- `docs/equipment_manual/oe1300/oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md:996`
- `docs/equipment_manual/oe1300/oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md:1079`
- `docs/equipment_manual/oe1300/oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md:1101`

结论很直接：

- **清洗版 Markdown 可用**
- **原始 OCR Markdown 只能做草稿，必须和 PDF 图页或人工清洗版交叉核对**

---

## 3. 连接与命令复核

### 3.1 串口

本机串口设备存在：

```text
/dev/cu.usbserial-B0027SH3
```

查询命令实测全部正常返回：

```text
*IDN?   -> SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B
BAUD?   -> 115200
*PLL?   -> 0
ISRC?   -> 0
FREQ?   -> 1.023181539e-06
NMOD?   -> 1
NIPA?   -> 192.168.1.1
NSMA?   -> 255.255.255.0
NGWA?   -> 192.168.1.1
```

多命令链也正常：

```text
*IDN?;ISRC?;FREQ?
-> SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B\r0\r1.023181539e-06
```

这和手册“多条命令可用 `;` 分隔”的说法一致，也和手册图 77 一致。

### 3.2 网口

本机 `en5` 已经在 `192.168.1.10/24`，与手册第 7.2 节默认网段一致。

TCP 到 `192.168.1.1:10001` 实测可连，文本查询正常：

```text
*IDN? -> SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B
*PLL? -> 0
FREQ? -> 1.023181539e-06
```

所以今天这台设备的结论不是“网口坏了”，而是：

- **链路层 / TCP 端口正常**
- **文本协议正常**
- **坏的是 TCP `RALL?` 数据路径**

---

## 4. `RALL?` 采集逻辑复核

### 4.1 串口 `RALL?`

串口单次 `RALL?` 实测：

- 返回长度：`520 B`
- 返回格式：**ASCII CSV**
- 字段数：**37**
- 示例前 6 项：

```text
1.8769159e-04,5.6513395e-03,5.6544554e-03,8.8097799e+01,4.0772929e-04,5.6397360e-03
```

- 示例后 3 项：

```text
1.023181539e-06,6.2500000e-04,6.2500000e-04
```

20 次连续 `RALL?` burst：

- 平均延迟：`61.485 ms`
- 平均吞吐：`16.264 Hz`
- 最短 / 最长：`59.385 ms / 62.773 ms`

这和手册“ASCII 浮点 CSV”的描述一致，也和 `OE1311&OE1351_RS232_Query Data.vi` 的逻辑一致：

- VISA Write
- wait `70 ms`
- VISA Read
- `sscanf("%g,%g,%g,...")` 解析 37 个浮点

对应整理文档：

- `docs/equipment_manual/oe1300/clean/oe1351_labview_ethernet_reference.md:91`

### 4.2 网口 `RALL?`

TCP 发送 `RALL?\r` 后，今天这台 OE1301 实测返回：

- 长度：`32768 B`
- 头 32B：不是 ASCII CSV
- 尾部可见上一条 `*IDN?` 身份字符串残留

本次抓到的尾部 ASCII 片段：

```text
...SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.
```

把前 296 B 按 LabVIEW `DATA Transmit.vi` 的“小端 double”方式解出来，得到的是明显无物理意义的大数/极小数。

也就是说，**今天这台设备返回的 32768 B 并不满足 LabVIEW `DATA Transmit.vi` 假设的 400 B frame 协议**。

### 4.3 和 LabVIEW VI 图的关系

我直接看了导出的 VI 图，结论如下：

- `OE1311&OE1351_Ethernet__Query_Data.vi`
  - Write `RALL?\r`
  - wait `5 ms`
  - 反复 `TCP Read 32768`
  - buffer 长度 `>= 32767` 才退出
  - 再把 buffer 交给 `DATA Transmit.vi`

- `OE1311&OE1351_DATA Transmit.vi`
  - 把数据按 **400 B 一帧**
  - 每帧按 **50 个 8B little-endian double**
  - 只取其中 37 个字段
  - 额外从 `29990` / `29997` 位置取状态与 `Trig_Count`

这说明：

- **LabVIEW 网口路径本来就不是按 ASCII `RALL?` 在读**
- 但 **今天这台 OE1301 的 TCP 二进制返回，又不符合 LabVIEW 这套二进制解析**

所以当前最稳的判断是：

- 手册第 5.2.10 对串口 `RALL?` 的描述是成立的
- LabVIEW 对网口 `RALL?` 的实现另走了一套二进制路径
- 但今天这台实机的 TCP 二进制路径仍然不可直接用

### 4.4 Windows 固定 IP 后的复核补充（2026-06-17）

后续把 Windows 实验机的设备网卡按手册第 7.2 节改成固定地址后：

- Windows 设备网卡：`192.168.1.10`
- 子网掩码：`255.255.255.0`
- 默认网关：`192.168.1.1`

再在 Windows 临时目录 `D:\temp\odmr_oe1300_verify_20260617\src` 中运行 `Odmr.WinProbe` 新增的 `oe1300-net-idn` / `oe1300-net-rall` 命令，结果如下：

- `oe1300-net-idn --host 192.168.1.1 --port 10001`

```text
SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B
```

- `oe1300-net-rall --host 192.168.1.1 --port 10001 --count 5`
  - 5/5 次全部成功
  - 每次都返回 `32768 B`
  - 主机侧命令平均耗时 `14.889 ms`
  - 首包 `head_hex`：

```text
3f30fa832f238ecb3f30f922f8b2e5a9
```

对应 `summary.json` 关键字段：

```json
{
  "transport": "tcp_binary",
  "host": "192.168.1.1",
  "port": 10001,
  "count": 5,
  "mean_latency_ms": 14.889420000000001,
  "mean_bytes": 32768,
  "min_bytes": 32768,
  "max_bytes": 32768
}
```

同时，按当前 `LabVIEW 400 B/frame + little-endian double` 假设做实验性解码时，首帧仍然会出现大量明显不合理的数值，例如：

```text
X      -> -9.237099641050116E+55
Y      -> -7.391511288208573E-107
R      -> -8.038316553270679E+197
Theta  -> 5.40492371713744E+279
```

所以这里的工程事实需要再收紧一层：

- **Windows 网口配置正确后，TCP 文本查询稳定可用**
- **Windows 网口配置正确后，TCP `RALL?` 原始 32768 B 抓取也稳定可用**
- **当前不成立的只有“把这 32768 B 按现有 LabVIEW 偏移直接解释”**

也就是说，当前阻塞点已经不是“设备网口不通”，而是：

- `TCP RALL?` 的真实二进制帧结构还没有被正确复原
- 现有 `DATA Transmit.vi` 导出的偏移假设，至少对今天这台 `OE1301 SN:L2283261` 仍然不成立

---

### 4.5 TCP `RALL?` 的阶段性解码结论（2026-06-17）

后续继续对同一参数做了 **串口 120 点** 与 **网口 120 包** 的对照，并直接检查了单包 `32768 B` 里 `74 x 400 B` 的 frame 结构，得到下面这条新的工程事实：

- **网口 `RALL?` 不是另一套语义字段。**
- **网口 `RALL?` 也不是“串口 37 字段按顺序直接平铺成 37 个 double”。**
- **当前最符合实测的是：`big-endian double + frame block mean`。**

目前已经稳定复原出的 block 结构如下：

- `frame 0-1` -> `X`
- `frame 2-3` -> `Y`
- `frame 4-5` -> `R`
- `frame 6-7` -> `Theta`
- `frame 8-9` -> `XNoise`
- `frame 10-11` -> `YNoise`
- `frame 12-13` -> `Frequency`
- `frame 14-21` -> `D1` 四组：`XD1 / YD1 / RD1 / ThetaD1`
- `frame 22-29` -> `D2` 四组：`XD2 / YD2 / RD2 / ThetaD2`
- `frame 30-37` -> `D3` 四组：`XD3 / YD3 / RD3 / ThetaD3`
- `frame 38-45` -> `D4` 四组：`XD4 / YD4 / RD4 / ThetaD4`
- `frame 46-53` -> `D5` 四组：`XD5 / YD5 / RD5 / ThetaD5`
- `frame 54-61` -> `D6` 四组：`XD6 / YD6 / RD6 / ThetaD6`
- `frame 62-69` -> `D7` 四组：`XD7 / YD7 / RD7 / ThetaD7`
- `AuxIn1 / AuxIn2` 当前用固定偏移 `28656 / 28432` 做阶段性恢复

对应地，`Odmr.WinProbe oe1300-net-rall` 的实验性解码已经改成输出 **与串口同名的 37 个字段**，不再输出原先那版错误的“每帧线性字段表”。

需要明确的是，这里解出来的是 **网口字段语义**，不是“串口数值逐点严格相等”的意思。当前实测可确认：

- `Theta / ThetaD1..ThetaD7 / Frequency` 与串口高度一致
- `X / Y / R / XDn / YDn / RDn / Noise` 在字段归属上已经对上，但数值相对串口存在稳定比例偏差
- `AuxIn1 / AuxIn2` 已能恢复出可用数值，但偏移仍应继续压实

所以当前的阻塞点已经从“网口字段完全不明”收敛为：

- 主字段语义已经复原
- 数值标定关系仍需继续验证
- Aux 偏移仍是阶段性结论

## 5. 当前工程决策

基于 **2026-06-17 本机实测**，当前设备应按下面的规则处理：

1. **把这台在线设备识别为 `OE1301`，不是 `OE1351`。**
2. **串口 `RALL?` 仍然是当前最直接的事实基准。**
3. **TCP `RALL?` 的字段语义已经可以在设备层解出来，但当前仍属于实验性解码，不应直接并入 runtime 主采集。**
4. **`oe1300_manual_clean.md` 可以继续作为开发输入。**
5. **`pdf_by_PaddleOCR-VL-1.6.md` 不能直接作为命令白名单真值。**

如果后面要继续做 C# runtime 适配，第一步不是改 `OE1022D` 那套 frozen hot path，而是单独为 `OE1301` 新建并继续收紧：

- 串口 ASCII line reader
- TCP `RALL?` 37 字段实验性 decoder
- 串口 / 网口之间的数值标定关系
- 无 packet counter 的连续性审计策略
