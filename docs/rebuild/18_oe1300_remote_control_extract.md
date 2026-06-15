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
