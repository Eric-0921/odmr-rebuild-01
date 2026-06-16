# OE1300 远程控制提取摘要（基于仓库手册）

来源文件：
- `docs/equipment_manual/oe1300/oe1300sereies-lockin-manual-operation.pdf`
- 按 5.2 节（5.2.1～5.2.10）及 4.x 章节提取；
- 命令清单使用 OCR 识别 + `pdftotext` 校验。  

## 1. 设备参数（与控制相关）

型号/定位：
- OE1300（部分章节出现 OE1311/OE1351 为同系列说明）
- 多解调器能力：基波 + 7 路谐波解调（共 8 路）
- 电流输入范围：≤ 5 µA
- 电压输入范围：单端 / 差分输入总幅值 ≤ 5 V
- SINE OUT：最大 5 Vrms，50 Ω
- TTL OUT：5 V TTL/CMOS 方波，频率等于 SINE OUT

通信接口：
- RJ45 隔离网络接口（1000 Mbps）
- RS232（可选 UART TTL XH2.54-4PIN）
- 电源：12 V

UART 协议（文档说明）：
- ASCII 字符传输
- 数据位：8
- 奇偶校验：无
- 停止位：1
- 可选波特率：9600、115200（默认）、921600

网络参数（控制示例默认）：
- 默认 IP：`192.168.1.1`
- 默认网关：`192.168.1.10`
- 默认子网掩码：`255.255.255.0`

## 2. 远程编程总则（关键行为）

与运行链路约束相关的关键规则：
- 上位机与设备通信使用 ASCII 字符
- 命令长度：4 字符命令符 + 参数 + 回车（`<cr>`）作为终止符
- 一个命令行可发送多个命令，使用 `;` 分隔
- 命令输入缓冲区 64 byte，超限会覆盖旧命令
- 查询通过在命令尾加 `?` 完成；例如 `FREQ?`、`OUTP? 1`
- 查询返回按发送顺序回传，每个返回值以终结符结尾
- 文档示例与现网一致：`FMOD 1<cr>`、`FREQ 10E3<cr>`

## 3. OE1300 远程控制命令（5.2.x）

以下命令按手册分组摘录（未标注校验为疑似 OCR 误识，仍保留原命令）：

### 3.1 输入方式

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `ISRC` | set/query | 输入方式：`i=0` A、`i=1` A-B、`i=2` I |
| `ICPL` | set/query | 输入耦合方式：`i=0` AC、`i=1` DC |

### 3.2 范围与时间常数

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `IRNG` | set/query | 系统量程选择（i=0..29） |
| `OFLT` | set/query | 滤波器时间常数，范围 `1E-6~3000`，默认 `0.01` |
| `OFSL` | set/query | 低通滤波器斜率：`6/12/18/24/30/36/42/48 dB/oct` |
| `SYNC` | set/query | 参考频率低于 1000Hz 可开启；`i=0` 关闭，`i=1` 开启 |

### 3.3 参考与相位

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `PHAS` | set/query | 相位，参数 `i` 解调器（0 基波、1~7 谐波），`x` 为相位值 |
| `FMOD` | set/query | 参考源：`i=0` 外部、`i=1` 内部、`i=2` 输入自参考 |
| `FREQ` | set/query | 参考信号频率，`1 mHz ~ 100 kHz`，分辨率 `1 mHz` |
| `RSLP` | set/query | 参考触发：`i=0` TTL 上升沿、`i=1` TTL 下降沿、`i=2` Sine 过零；低于 2Hz 建议 TTL |
| `DMOD` | set/query | 解调器模式，`i=0..6`；`j=0`谐波模式，`j=1`任意频率模式 |
| `HARM` | set/query | 谐波阶数，`j=1..65535`，需满足 `j*f <= 100/500kHz` |
| `DARB` | set/query | 任意频率解调参考频率，`1µHz~100/500kHz`，分辨率 1µHz |

### 3.4 正弦波与 CH 输出

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `SWVT` | set/query | sine/ttl 输出开关：`0` 关闭，`1` 开启 |
| `SLVL` | set/query | 同步正弦波固定幅值 `100µVrms <= x <= 5Vrms`，步进 10µVrms |
| `COUT` | set/query | AUXOUT 输出数据源选择（j=0 FIXED,1 X,2 Y,3 R,4 θ,5 XD1,6 YD1,7 RD1,8 θD1 ... 37 AUX-IN2） |
| `CAUX` | set/query | CHOUT1/CHOUT2 DAC 输出电压，`-10 ~ 10 V` |
| `COFP` | set/query | CHOUT 偏置，`-100% ~ 100%` |
| `CEXP` | set/query | CHOUT 放大倍数，`0.001 <= x <= 10000` 的整数 |

### 3.5 PID 设置

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `*PID` | set/query | `i=0/1` 通道，`j` 为参数位 |

`j` 关键项（按文档）：
- `1`：PID 开关（0 关闭，1 开启）
- `2`：输入源选择（范围 `0~31` 与 `34~36`，映射见 SNAP 参数）
- `3`：输出目的地（0 Aux Out1, 1 Aux Out2, 2 Sine Out, 3 Int Frequency）
- `4`：采样间隔索引（`0~31`，`SampleRate = 4MHz/2^x`）
- `6/7/8`：PID 系数 `Kp/Ki/Kd`
- `9`：微分项滤波带宽，需 `< SampleRate/π`
- `10/11`：输出限幅 Max/Min
- `12`：Set Point
- `13`：Output Offset
- `14/15`：积分项限幅 Max/Min
- `16`：PID 输出（只读）
- `17`：PID 输入值（只读）

### 3.6 串口与网口参数

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `BAUD` | set/query | 波特率：9600/115200/921600 |
| `NMOD` | set/query | 网口模式：`i=0` TCP、`i=1` DHCP |
| `NIPA` | set/query | IP 地址设置 |
| `NSMA` | set/query | 子网掩码设置 |
| `NGWA` | set/query | 网关设置 |

### 3.7 存读与状态命令

| 命令 | 方向 | 说明 |
| --- | --- | --- |
| `*RST` | set | 全量复位：状态/参数默认化，缓存区清空 |
| `*IDN?` | query | 返回字符串：`OE1300, SNxxxxxx, Version: Vxxx` |
| `RALL?` | query | 全帧读取（X/Y/R/θ/Dk 通道、噪声、频率、AUX-IN） |
| `OAUX? i` | query | Aux 输入读取，`i=0` AUX-IN1，`i=1` AUX-IN2 |
| `OVLD?` | query | ADC overload 状态：0 无溢出，1 有溢出 |
| `*PLL?` | query | PLL 锁定状态：0 未锁/内部参考，1 已锁 |

## 4. 已知 caveat（OCR 与手册核对）

- 命令文本来自 PDF OCR，局部字符可能有误（例如 OCR 对部分希腊字母、表格内竖线的识别误差）
- `COUT`、`SNAP` 的全部通道映射与参数细表建议优先以 PDF 版式原表复核
- 目前仅汇总到可直接用于命令白名单建设的层级，未下沉到 C# Helper 实现

## 5. 追加修订（补全遗漏项）

### 5.1 RALL/SNAP/OUTP 系列（同参数映射）

- `RALL?`、`SNAP`、`OUTP` 三者共享同一组参数索引（0~36）：
  - `0` X
  - `1` Y
  - `2` R
  - `3` θ
  - `4` XD1
  - `5` YD1
  - `6` RD1
  - `7` θD1
  - `8` XD2
  - `9` YD2
  - `10` RD2
  - `11` θD2
  - `12` XD3
  - `13` YD3
  - `14` RD3
  - `15` θD3
  - `16` XD4
  - `17` YD4
  - `18` RD4
  - `19` θD4
  - `20` XD5
  - `21` YD5
  - `22` RD5
  - `23` θD5
  - `24` XD6
  - `25` YD6
  - `26` RD6
  - `27` θD6
  - `28` XD7
  - `29` YD7
  - `30` RD7
  - `31` θD7
  - `32` X-Noise
  - `33` Y-Noise
  - `34` Frequency
  - `35` AUX-IN1
  - `36` AUX-IN2
- `RALL?`：返回一个逗号分隔的单行字符串，当前可用作采集查询；文档强调仅查询用途。
- `SNAP ? i,j{,k...}`：同一时刻读取最多 10 个参数，返回顺序按发送参数顺序，不保证与 `OUTP` 连续查询同一时刻。
- `OUTP ? i`：单参数读取。参数表与上面一致。

### 5.2 控制命令细项（补充）

- 通讯示例：`ISRC ?`、`FREQ?`、`OUTP? 1` 均为合法，末尾必须加回车（`<cr>` / `0D`）。
- `SNAP?` 例子：`SNAP?0,1,34,3;` 返回 `"<X>,<Y>,<Frequency>,<θ>"` 形式同一时刻序列。
- `OAUX ? i`：`i=0` AUX-IN1，`i=1` AUX-IN2，返回电压值（V）。
- `OVLD ?`：`0` 无 ADC overload，`1` 有 overflow，建议伴随信号幅值/输入量程调整。
- `*PLL ?`：`0` 未锁或内部参考，`1` 锁定。

## 6. OE1311/OE1351 网口 `RALL` 二进制包结论（基于原厂 LabVIEW 导出）

本节不是手册原文，而是基于以下材料交叉确认后的实现结论：

- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/OE1311&OE1351__Ethernet__Query_Data/*`
- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/OE1311&OE1351__DATA_Transmit/*`
- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/OE1311&OE1351__Data_Index/*`
- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/OE1311&OE1351__Query_PLL&Overload/*`
- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/OE1311&OE1351__RALL_Adressctl/*`
- `reverse_application/oe1351-labview/OUTPUT-ssi_oe1351/Example__OE1311&OE1351_Trig_in/*`
- `Downloads/Convert to Big-Endian.txt`

### 6.1 网口查询热路径

原厂 LabVIEW 的网口查询路径可概括为：

- 发送 `RALL?\r`
- 等待约 `5 ms`
- 循环 `TCP Read 32768`
- 缓冲长度达到 `>= 32767` 后停止
- 把整块 buffer 交给 `OE1311&OE1351_DATA Transmit.vi`

这说明：

- 网口 `RALL` 不是串口那种逗号分隔 ASCII 返回；
- 它是固定上限长度的大块二进制响应；
- 采样热路径本身非常简单，不依赖复杂重试或逐字段查询。

当前在仓库中已经把这条路径收敛为 **不可改变的 collector/demo 基线**：

```text
write RALL?\r
sleep 5 ms
read until 32768 B
decode current block
append parsed values
next round
```

这里的“不可改变”指的是：

- 不加入额外的 `poll sleep`
- 不在默认路径里做 `drain-before-write`
- 不引入 first-byte deadline / frame deadline / zero-byte retry
- 不拆成 `producer(raw pull) + consumer(parse)` 双线程
- 不在默认验证路径里保留本地二进制 raw 文件

原因不是代码风格，而是已经有过对比：一旦把这条路径复杂化，就会重新引入“主机查询速率高，但有效块率不清楚”的歧义，且会破坏与原厂 LabVIEW 行为的一一对应。

### 6.2 主数据区与附加状态区

`OE1311&OE1351_DATA Transmit.vi` 直接显示：

- 前 `29600 B` 被当作主采样数据区；
- 该区域按 `74` 组切分；
- 每组 `400 B`；
- 主数据区之外，LabVIEW 还从固定偏移读取附加状态：
  - `29990` 附近 `2 B` -> `PLL&Overload`
  - `29997` 附近 `1 B` -> `Trig_Count`

当前已确认：

- `37` 个主参数全部在主数据区内；
- `PLL/Overload` 与 `Trig_Count` 不属于这 `37` 个主参数序列，而是附加状态字段；
- `29600 B` 之后到包尾的其余保留区暂未完全表格化，但这不阻碍主采样数据解码。

### 6.3 主参数顺序与串口/手册完全对齐

`OE1311&OE1351_Data Index.vi` 给出的参数顺序为：

`X, Y, R, θ, XD1, YD1, RD1, θD1, XD2, YD2, RD2, θD2, XD3, YD3, RD3, θD3, XD4, YD4, RD4, θD4, XD5, YD5, RD5, θD5, XD6, YD6, RD6, θD6, XD7, YD7, RD7, θD7, X-Noise, Y-Noise, Frequency, AUX-IN1, AUX-IN2`

这与手册中的串口 `RALL?` 参数表一一对应。

LabVIEW 图中的命名风格略有不同，但语义相同：

- `θ` = `Xita`
- `XDn` = `Xhn`
- `YDn` = `Yhn`
- `RDn` = `Rhn`
- `θDn` = `Xita hn`
- `AUX-IN1` = `AUXADC1`
- `AUX-IN2` = `AUXADC2`

因此，对 OE1311/OE1351 来说可以认为：

- 串口 `RALL?` 与网口 `RALL` 的主字段集合相同；
- 顺序相同；
- 区别在于封装形式不同：串口是 ASCII 单值序列，网口是二进制批量采样块；
- 网口额外携带状态字段 `PLL&Overload` 与 `Trig_Count`。

### 6.4 字节序结论

`Convert to Big-Endian.vi` 的作用不是“单纯声明类型为 big-endian”，而是：

- 输入 `U8[]`
- 按标量字宽分块（`U8/U16/U32/U64`）
- 对每个块执行字节倒序
- 再拼回 `U8[]`

结合真实抓包结果，当前可以把网口 `RALL` 采样区的字节序语义固定为：

- 设备输出的 8 字节采样标量不能直接按主机 little-endian `double` 解释；
- LabVIEW 侧会先做按 8 字节块的字节翻转；
- 翻转后再按 `double` 解释，才能得到物理合理的采样值；
- 在 C# 中，等价实现是直接按 big-endian `double` 读取，或先做 8 字节翻转再按 little-endian 读取。

### 6.5 当前固定的解码口径

当前仓库里对网口 `RALL` 的固定解码口径是：

- 每块总读取长度：`32768 B`
- 主采样区：前 `29600 B`
- 主采样区切分：`74 x 400 B`
- 主参数数目：`37`
- 每参数样本数：`100`
- 标量类型：`8 B double`
- 字节序：按 big-endian `double` 解释
- 附加状态：
  - `status_hex` / `status_byte` 取自 `29990` 开始的 `2 B`
  - `Trig_Count` 取自 `29997` 的 `1 B`

当前默认落盘的不是 raw 二进制，而是两类解析后事实：

- `parameter_values.csv`
  - 每个 `RALL` 一行
  - 保存 `status_hex`、`status_byte`、`trig_count`
  - 以及 `37` 个参数在该块内的均值
- `preview_values.csv`
  - 仅在 `--write-values true` 时写出
  - 保存选定参数在每个 `RALL` 内展开后的 `100` 个样本

因此，当前验证链路是“直接处理并保存解析后原始事实”，而不是“先保留本地 raw 二进制再离线重放”。

## 7. 采样率语义：`RALL?` 查询频率不等于解析后样本频率

这是 OE1300 与 OE1022D 最需要分开的地方。

### 7.1 OE1022D 的“采样点”定义

当前仓库中，OE1022D 的事实语义已经冻结为：

- collector 热路径：`write RALL? -> sleep 30ms -> exact read 12288B -> 下一轮`
- `payload[12287]` 是 `device_packet_counter`
- 设备内部结果窗口按约 `50 ms` 更新

因此 OE1022D 里要区分：

- `RALL?` 主机查询频率
- 设备内部新窗口到达频率

如果主机查询得比设备窗口更新更快，会读到重复窗口：

- `device_packet_counter delta = 0`：重复窗口，不是丢包
- `delta = 1`：新窗口连续到达
- `delta > 1`：疑似漏设备窗口

所以在 OE1022D 中，一个 point 的有效数据更接近：

- `segment` 时间窗内的 `unique device windows`

而不是：

- 单纯的主机 `RALL?` 次数

### 7.2 OE1300 的“采样点”定义

OE1311/OE1351 网口 `RALL` 不同。  
一次网口 `RALL` 返回的不是一个时刻的单值，而是一整块批量采样数据。

当前基于 LabVIEW `DATA Transmit` 与实测 decode，可先固定这一点：

- 一次 `RALL` 查询会返回 `37` 个参数的批量样本；
- 因而 `RALL?` 查询频率只是“主机多久拿一块数据”；
- 真正的每参数有效采样率应按“每块解出多少点”乘以查询频率计算。

现阶段调试结论可写成：

- `effective_per_parameter_sample_hz = rall_query_hz * samples_per_parameter_per_rall`

在当前 LabVIEW 对齐 demo 中，虽然主机 `RALL` 查询频率约为 `64 Hz`，但真实抓到的大量相邻块是重复块，因此不能直接用：

- `64 Hz * 100 samples_per_rall = 6400 Hz`

来当作最终采样率真值。

正确口径应改为：

- `effective_per_parameter_sample_hz = unique_rall_block_hz * 100`

其中 `unique_rall_block_hz` 指的是“内容真正变化的新块频率”。

因此，OE1300 的 point 定义不应照搬 OE1022D 的“一个 `RALL` = 一个设备窗口”思路，而应改为：

- `1 point = point segment 时间窗内的已解码参数样本序列`

也就是说，对 OE1300 来说，后续 collector/demo 的重点指标应是：

- 每次 `RALL` 能稳定解出多少每参数样本；
- point 窗口内累计得到多少已解码样本；
- 解码后样本是否连续、是否存在状态异常；
- 主机 `RALL` 频率只作为吞吐指标，不应直接当作最终采样率。

## 8. 当前实现边界：单线程、即时解码、不保留 raw

当前 `tools/win-csharp/Odmr.WinProbe` 中保留的 `oe1300-net-labview-demo`，其真实实现边界已经固定如下：

- 单线程 loop
- 同一线程内完成：
  - `RALL` 拉取
  - 当前块解码
  - 统计更新
  - CSV 落盘
- 不做 raw 二进制持久化
- 不做双线程解耦
- 不做 raw replay 路径

也就是说，它和 OE1022D 当前 run-time collector 不同：

- OE1022D：collector 热路径冻结为“只拉取 raw，不在 collector 线程里做字段解析”
- OE1300 当前 demo：为了先把设备层采样事实跑通，采用“拉取后立即解析并保存解析后事实”的更直接路径

这不是说双线程一定错误，而是当前阶段没有证据表明双线程能带来更高的有效采样率；相反，单线程、无 raw 持久化、直接保存解析后事实，已经能稳定复现约 `1 kHz/parameter` 的有效采样率，并且最接近原厂 LabVIEW 的设备层实现。
