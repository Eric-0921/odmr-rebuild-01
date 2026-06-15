# 1. 技术参数

## 1.1 信号通道

| 参数 | 规格 |
|------|------|
| 电压输入模式 | 单端或差分输入 |
| 满量程范围 | $1\ \mathrm{nV}$ 至 $5\ \mathrm{V}$，以 1-2-5 的倍数顺序步进 |
| 电流输入增益 | $10^6\ \mathrm{V/A}$ |
| 输入阻抗（电压） | $10\ \mathrm{M\Omega} \parallel 25\ \mathrm{pF}$，交流或直流耦合 |
| 输入阻抗（电流） | $1\ \mathrm{k\Omega}$ 到虚拟地 |
| 共模抑制比 | $>70\ \mathrm{dB}$ 至 $10\ \mathrm{kHz}$，以 $6\ \mathrm{dB/oct}$ 减小 |
| 动态储备 | $>120\ \mathrm{dB}$ |
| 增益精度 | 典型值 $0.2\%$，最大 $2\%$ |
| 电压噪声 | $997\ \mathrm{Hz}$ 时 $5\ \mathrm{nV/\sqrt{Hz}}$ |
| 电流噪声 | $97\ \mathrm{Hz}$ 时 $0.3\ \mathrm{pA/\sqrt{Hz}}$；$997\ \mathrm{Hz}$ 时 $0.3\ \mathrm{pA/\sqrt{Hz}}$ |

## 1.2 参考通道

### 输入

| 参数 | 规格 |
|------|------|
| 频率范围 | $1\ \mathrm{\mu Hz}$ 至 $100\ \mathrm{kHz}/500\ \mathrm{kHz}$ |
| 参考输入 | 方波或正弦波 |
| 输入阻抗 | $10\ \mathrm{M\Omega}$ |
| 方波参考电平 | $V_{IH} > 3\ \mathrm{V}$，$V_{IL} < 0.5\ \mathrm{V}$ |
| 正弦参考信号 | $>1\ \mathrm{Hz}$，$>300\ \mathrm{mV}_{pp}$ |

### 相位

| 参数 | 规格 |
|------|------|
| 分辨率 | $1\ \mathrm{\mu deg}$ |
| 绝对相位误差 | $<2\ \mathrm{deg}$ |
| 相对相位误差 | $<1\ \mathrm{deg}$ |
| 温漂（低于 $10\ \mathrm{kHz}$） | $<0.01\ \mathrm{°/°C}$ |
| 温漂（高于 $10\ \mathrm{kHz}$） | $<0.1\ \mathrm{°/°C}$ |

### 其他

| 参数 | 规格 |
|------|------|
| 谐波检测 | $2F, 3F, \dots, nF$ 至 $100\ \mathrm{kHz}/500\ \mathrm{kHz}$（$n < 65,535$） |
| 采集时间（内部参考） | 即时采集 |
| 采集时间（外部参考） | （3 个周期 + $5\ \mathrm{ms}$）或者 $40\ \mathrm{ms}$ |

## 1.3 解调器

| 参数 | 规格 |
|------|------|
| 数量 | 8 个 |
| 稳定性（数字输出） | 所有设置均无零点漂移 |
| 稳定性（显示） | 所有设置均无零点漂移 |
| 稳定性（模拟输出） | 所有动态储备设置小于 $5\ \mathrm{ppm/°C}$ |
| 谐波抑制 | $-75\ \mathrm{dB}$ |
| 时间常数 | $1\ \mathrm{\mu s}$ 至 $3\ \mathrm{ks}$；$6, 12, 18, 24, 30, 36, 42, 48\ \mathrm{dB/oct}$ 陡降 |
| 同步滤波器 | 低于 $1\ \mathrm{kHz}$ 且大于 $18\ \mathrm{dB/oct}$ 陡降方可开启 |

## 1.4 信号发生器

### 频率

| 参数 | 规格 |
|------|------|
| 范围 | $1\ \mathrm{mHz}$ 至 $100\ \mathrm{kHz}/500\ \mathrm{kHz}$ |
| 精度 | $2\ \mathrm{ppm} + 10\ \mathrm{\mu Hz}$ |
| 分辨率 | $1\ \mathrm{\mu Hz}$ |

### 失真

| 参数 | 规格 |
|------|------|
| 失真 | $-80\ \mathrm{dBc}$（$f < 10\ \mathrm{kHz}$），$-70\ \mathrm{dBc}$（$f > 10\ \mathrm{kHz}$） |

### 正弦幅值

| 参数 | 规格 |
|------|------|
| 范围 | $100\ \mathrm{\mu V}_{rms}$ 至 $5\ \mathrm{V}_{rms}$ |
| 分辨率 | 最低 $10\ \mathrm{\mu V}_{rms}$ |
| 误差 | 标准 $0.5\%$（$f < 10\ \mathrm{kHz}$），最大 $1\%$ |
| 温度稳定性 | $100\ \mathrm{ppm/°C}$ |

### 输出

| 参数 | 规格 |
|------|------|
| 正弦输出 | 正弦信号，输出阻抗 $50\ \mathrm{\Omega}$ |
| TTL 同步输出 | $5\ \mathrm{V}$ TTL/CMOS 电平，输出阻抗 $200\ \mathrm{\Omega}$ |

## 1.5 输出

### CH 1 和 CH 2

| 参数 | 规格 |
|------|------|
| 功能 | 输出 $X, Y, R, \theta$ 和谐波 |
| 幅值 | $\pm 10\ \mathrm{V}$ |
| 驱动电流 | $\pm 30\ \mathrm{mA}$ max |

### AUX Inputs

| 参数 | 规格 |
|------|------|
| 功能 | 2 通道输入 |
| 幅值 | $\pm 10\ \mathrm{V}$，$1\ \mathrm{mV}$ 分辨率 |
| 阻抗 | $1\ \mathrm{M\Omega}$ |

## 1.6 接口

| 参数 | 规格 |
|------|------|
| UART | 接口类型 RS232（可改为 XH2.54-4PIN 端子 TTL 电平） |
| 网口 | 隔离式 $1000\ \mathrm{Mbps}$ RJ45 接口 |

## 1.7 其他

| 参数 | 规格 |
|------|------|
| 电源要求（电压） | $12\ \mathrm{VDC} \pm 5\%$ |
| 电源要求（功率） | 标准 $18\ \mathrm{W}$，最大不超过 $24\ \mathrm{W}$ |
| 重量 | $400\ \mathrm{g}$ |
| 尺寸（长 × 宽 × 高） | $180\ \mathrm{mm} \times 106\ \mathrm{mm} \times 44\ \mathrm{mm}$ |

产品结构尺寸图：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//a67764a5-6f47-440b-9139-9d13e49746ba/markdown_1/imgs/img_in_image_box_150_509_1023_1019.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A05Z%2F-1%2F%2F58811c3a70f8e35965f2d5d8fadc9dd7a433efbaf144ba7ff974431ca07933b9" alt="Image" width="73%" /></div>

<div style="text-align: center;">图 1. 产品尺寸图</div>
