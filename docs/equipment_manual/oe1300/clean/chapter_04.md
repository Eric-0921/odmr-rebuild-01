# 4. 产品概述

OE1311/OE1351 是一款模块化锁放。作为一款模块化的锁放，它仍然具有齐全的功能，包括：电压信号单端输入，电压信号差分输入，电流信号输入；TTL 参考信号，正弦参考信号输入；TTL 参考信号输出，Sine 信号输出，以及 2 路辅助信号输出和 2 路辅助信号输入。

得益于其多级程控放大器以及较大的动态储备配置，输入信号幅度可以设置为 $1\ \mathrm{nV}$ 至 $5\ \mathrm{V}$ 或 $1\ \mathrm{fA} \sim 5\ \mathrm{\mu A}$。

同时，其使用了高端的 Zynq 系列 SOC，应用其强大的运算能力，可以同时实现 1 路基波及 7 路谐波解调通道的测量。且 7 路谐波还可以设置为任意频率解调模式。

## 4.1 接口

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_4/imgs/img_in_image_box_262_607_928_906.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2Ffc415d2024ad9c922d621e8128f259f952a9ad413b8ef1b9fba5dfe34cd9ef87" alt="Image" width="55%" /></div>

<div style="text-align: center;">图 16. OE1311/OE1351 接口指示</div>

信号接口分别是：I，+V，-V Diff，REF IN，AUX In1，AUX In2，OUTPUT1，OUTPUT2，SINE OUT，TTL OUT。

通信接口分别是：RJ45 隔离网络接口，RS232 串口（可选 UART TTL 串口）。

电源接口：$12\ \mathrm{V}$ 电源输入。

## 4.2 信号接口

| 接口 | 说明 |
|------|------|
| I | 电流信号输入，输入阻抗 $1\ \mathrm{k\Omega}$，输入电流幅度 $\leq 5\ \mathrm{\mu A}$。 |
| +V | 电压信号输入正端，输入阻抗 $10\ \mathrm{M\Omega} \parallel 10\ \mathrm{pF}$，输入电压幅度 $\leq 5\ \mathrm{V}$。 |
| -V Diff | 电压信号输入负端，输入阻抗 $10\ \mathrm{M\Omega} \parallel 10\ \mathrm{pF}$，输入电压幅度 $\leq 5\ \mathrm{V}$，仅在电压信号差分输入模式下有用。 |
| REF IN | 参考输入，可以使用正弦波或 TTL 方波驱动。正弦波输入时输入阻抗为 $1\ \mathrm{M\Omega}$，交流耦合。对于低频应用的情况（$< 1\ \mathrm{Hz}$），推荐使用 TTL 方波的参考信号。<br>最大输入电压：$\leq 5\ \mathrm{V}$（TTL 模式）；$\leq 10\ \mathrm{V}_{pp}$（Sine 模式）。 |
| AUX In1 | 辅助输入 1，输入 DC 范围 $\pm 10\ \mathrm{V}$，最小分辨率为 $0.1\ \mathrm{mV}$，输入阻抗 $1\ \mathrm{M\Omega}$。 |
| AUX In2 | 辅助输入 2，输入 DC 范围 $\pm 10\ \mathrm{V}$，最小分辨率为 $0.1\ \mathrm{mV}$，输入阻抗 $1\ \mathrm{M\Omega}$。 |
| OUTPUT1 | 辅助输出 1：输出 DC 范围 $\pm 10\ \mathrm{V}$，最小分辨率为 $1.2\ \mathrm{mV}$。 |
| OUTPUT2 | 辅助输出 2：输出 DC 范围 $\pm 10\ \mathrm{V}$，最小分辨率为 $1.2\ \mathrm{mV}$。 |
| SINE OUT | 信号发生器：提供最大 $5\ \mathrm{V}_{rms}$ 幅值的可编程正弦波输出，输出阻抗为 $50\ \mathrm{\Omega}$。当外部参考信号使用时，信号发生器通过锁相环与参考信号进行锁相。 |
| TTL OUT | TTL OUT 输出接口提供 $5\ \mathrm{V}$ TTL/CMOS 兼容的方波信号，输出阻抗为 $200\ \mathrm{\Omega}$，其频率与 SINE OUT 相同。 |

## 4.3 通信接口

| 接口 | 说明 |
|------|------|
| RJ45 网络接口 | 隔离式 $1000\ \mathrm{Mbps}$。 |
| 串口 | 接口端子：RS232（可选端子 XH2.54-4PIN TTL 电平）。 |

### 4.3.1 UART 串口通信协议

UART 作为异步串口通信协议的一种，工作原理是将传输数据的每个字符一位接一位地传输。

通信协议说明如下：

- **起始位**：先发出一个逻辑“0”的信号，表示传输字符的开始。
- **资料位**：紧接着起始位之后。数据位的个数可以是 4、5、6、7、8 等，构成一个字符。通常采用 ASCII 码。从最低位开始传送，靠时钟定位。
- **奇偶校验位**：资料位加上这一位后，使得“1”的位数应为偶数（偶校验）或奇数（奇校验），以此来校验资料传送的正确性。
- **停止位**：它是一个字符数据的结束标志。可以是 1 位、1.5 位、2 位的高电平。由于数据是在传输线上定时的，并且每一个设备有其自己的时钟，很可能在通信中两台设备间出现了小小的不同步。因此停止位不仅仅是表示传输的结束，并且提供计算机校正时钟同步的机会。适用于停止位的位数越多，不同时钟同步的容忍程度越大，但是数据传输率同时也越慢。
- **空闲位**：处于逻辑“1”状态，表示当前线路上没有资料传送。
- **波特率**：是衡量资料传送速率的指标。表示每秒钟传送的二进制位数。例如资料传送速率为 120 字符/秒，而每一个字符为 10 位，则其传送的波特率为 $10 \times 120 = 1200$ 位/秒 $= 1200$ 波特。

### 4.3.2 UART 协议配置

- 可选波特率：9600，115200（default），921600
- 校验位：无
- 数据位：8
- 停止位：1
