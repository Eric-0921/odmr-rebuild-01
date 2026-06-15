MODEL OE1300

Lock-In Amplifier

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d16a34a8-65ab-40d2-87cd-ed7ed7bea56c/markdown_0/imgs/img_in_image_box_395_1079_501_1197.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2Fa9b2a02065216b04300f8f57512df45150bcd24204840cbdf877cb070c6374ed" alt="Image" width="8%" /></div>


SINE SCIENTIFIC INSTRUMENTS

Copyright © 2022 by SSI.

All Rights Reserved.

Revision 1.0.4, 2024-04-03

Contact information

020-84133345

Address: 2906 Caizhi Building, No. 448 South Guangzhou Avenue, Haizhu District,

Guangzhou City, Guangdong Province

Email: support@ssi-instrument.com

## 目录

1. 技术参数 ..... 1    
1.1 信号通道 ..... 1    
1.2 参考通道 ..... 1    
1.3 解调器 ..... 2    
1.4 信号发生器 ..... 2    
1.5 输出 ..... 2    
1.6 接口 ..... 2    
1.7 其他 ..... 2    
2. 安全性和使用准备 ..... 4    
2.1 避免火灾或人身伤害 ..... 4    
2.2 锁相放大器使用注意事项 ..... 5    
3. 锁相放大器基础 ..... 6    
3.1 锁相放大器介绍 ..... 6    
3.2 OE1300 功能原理图 ..... 8    
3.3 参考通道 ..... 8    
3.4 相敏检波器 ..... 9    
3.5 时间常数和直流增益 ..... 10    
3.6 直流输出和增益 ..... 13    
3.7 动态储备 ..... 14    
3.8 信号输入放大和滤波 ..... 15    
3.9 输入端连接 ..... 16    
3.10 固有噪声 ..... 18    
3.11 外部噪声源 ..... 19    
3.12 噪声测量 ..... 21    
3.13 辅助模拟输入（AUXIN） ..... 21    
3.14 信号发生器的频率、幅值扫描 ..... 21    
3.15 示波器功能 ..... 21    
3.16 频谱分析功能 ..... 22    
3.17 PID 控制 ..... 23    
3.18 多解调器 ..... 25    
4. 产品概述 ..... 26    
4.1 接口 ..... 26    
4.2 信号接口 ..... 26    
4.3 通信接口 ..... 27    
4.3.1 UART 串口通信协议 ..... 27    
4.3.2 UART 协议配置 ..... 28    
5. 远程编程 ..... 29    
5.1 OE1300 命令语法 ..... 29    
5.2 详细的命令列表 ..... 29    
5.2.1 输入方式指令 ..... 30    
5.2.2 范围与时间常数指令 ..... 30    
5.2.3 参考与相位指令 ..... 31

5.2.4 正弦波输出指令 ..... 33    
5.2.5 CH 通道输出指令 ..... 33    
5.2.6 PID 设置 ..... 35    
5.2.7 波特率设置 ..... 37    
5.2.8 存读取设置指令 ..... 37    
5.2.9 数据和状态读取指令 ..... 39    
6. PC 软件安装使用说明 ..... 43    
6.1 软件驱动安装 ..... 43    
6.2 软件 Console 安装 ..... 45    
6.3 软件使用说明 ..... 50    
6.3.1 主界面功能介绍 ..... 52    
6.3.2 输入信号配置 ..... 53    
6.3.3 参考信号及扫频配置 ..... 55    
6.3.4 输入范围配置 ..... 57    
6.3.5 解调器谐波及任意频率配置 ..... 58    
6.3.6 滤波器配置 ..... 60    
6.3.7 输出通道配置 ..... 62    
6.3.8 正弦信号输出与 TTL 参考信号配置 ..... 64    
6.3.9 数据保存配置 ..... 65    
6.3.10 谐波波形显示 ..... 67    
6.3.11 关闭输出检测窗口 ..... 68    
6.3.12 示波器窗口 ..... 68    
6.3.13 FFT 窗口 ..... 71    
6.3.14 PID 控制窗口 ..... 74    
7. 操作实例 ..... 76    
7.1 串口通讯 ..... 76    
7.2 网口通讯 ..... 85

## 1. 技术参数

### 1.1 信号通道

- 电压输入模式 单端或差分输入

• 满量程范围 1 nV 至 5 V，以 1-2-5 的倍数顺序步进

 $$ 10^{6}\mathrm{~V/A} $$ 

- 输入阻抗

电压 10 MΩ//25 pF，交流或直流耦合电流 1 kΩ到虚拟地

比 >70 dB 至 10 kHz，以 6 dB/oct 减小

动态储备

增益精度

典型值0.2%，最大2%

997 Hz 时 5 nV/√Hz

电流噪声

97 Hz 时 0.3 pA/ √ Hz

997 Hz 时 0.3 pA/ √ Hz

### 1.2 参考通道

输入

频率范围 1 uHz 至 100 kHz/500 kHz

参考输入 方波或正弦波

输入阻抗 10 MΩ

方波参考电平  $ V_{IH} > 3V $,  $ V_{IL} < 0.5V $

正弦参考信号 >1 Hz

> 300 mVpp

##### 相位

分辨率 1 udeg

绝对相位误差 <2 deg

相对相位误差 <1 deg

温漂

低于  $ 10 \, kHz $ <0.01 / °C

高于  $ 10 \, kHz $ <0.1 / °C

谐波检测

2F, 3F, ...nF 至 100 kHz/500 kHz (n<65,535)

采集时间

内部参考 即时采集

外部参考 (3个周期+5 ms)或者40 ms

### 1.3 解调器

8个

数量

- 稳定性

  数字输出 所有设置均无零点漂移

  显示 所有设置均无零点漂移

  模拟输出 所有动态储备设置小于 5 ppm/℃

- 谐波抑制 -75 dB

- 时间常数 1 us 至 3 ks. 6, 12, 18, 24, 30, 36, 42, 48 dB/oct 陡降

- 同步滤波器 低于 1 kHz 且大于 18 dB/oct 陡降方可开启

### 1.4 信号发生器

● 频率

范围 1 mHz 至 100 kHz/500 kHz

精度 2 ppm + 10 uHz

分辨率 1 uHz

-80 dBc (f<10 kHz), -70 dBc (f>10 kHz)

失真

- 正弦幅值

范围 100 uVrms 至 5 Vrms

分辨率 最低 10 uVrms

误差 标准 0.5% (f<10 kHz)，最大 1%

温度稳定性 100 ppm/°C

正弦输出 正弦信号，输出阻抗 50 Ω

TTL 同步输出 5V TTL/CMOS 电平，输出阻抗 200 Ω

### 1.5 输出

- CH 1 和 CH 2

功能 输出 X,Y,R, $ \theta $ 和谐波

幅值  $ \pm10\ V $

驱动电流  $ \pm30\ mA\ max $

AUX Inputs

功能 2 通道输入

幅值  $ \pm10\ V $，1 mV 分辨率

阻抗 1 M $ \Omega $

### 1.6 接口

UART 接口类型 RS232（可改为 XH2.54-4PIN 端子 TTL 电平）

网口 隔离式 1000 Mbps RJ45 接口

1.7 其他

##### OE1300 Lock-In Amplifier

电源要求

电压

12 VDC $ \pm $5%

功率

标准 18 W，最大不超过 24 W

重量

400 g

尺寸

长

180 mm

宽

106 mm

高

44 mm

产品结构尺寸图：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//a67764a5-6f47-440b-9139-9d13e49746ba/markdown_1/imgs/img_in_image_box_150_509_1023_1019.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A05Z%2F-1%2F%2F58811c3a70f8e35965f2d5d8fadc9dd7a433efbaf144ba7ff974431ca07933b9" alt="Image" width="73%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图1. 产品尺寸图</div> </div>


## 2. 安全性和使用准备

详细阅读下列安全性预防措施，以避免人身伤害，并防止损坏本产品或与本产品连接的任何产品。为避免可能的危险，请务必按照规定使用本产品。只有合格人员才能执行维修过程。

### 2.1 避免火灾或人身伤害

使用合适的电源线。请只使用本产品专用并经所在国家/地区认证的电源线。

将产品接地。本产品通过电源线的接地导线接地。为避免电击，必须将接地导线与大地相连。在对本产品的输入端或输出端进行连接之前，请务必将本产品正确接地。

遵守所有终端额定值。为避免火灾或电击，请遵守产品上的所有额定值和标记。在对产品进行连接之前，请首先查阅产品手册，了解有关额定值的详细信息。只能将探头基准导线连接到大地。对任何终端（包括公共终端）施加的电压不要超过该终端的最大额定值。

断开电源。电源开关可以使产品断开电源。请参阅有关位置的说明。不要挡住电源开关；此电源开关必须能够随时供用户使用。

切勿开盖操作。请勿在外盖或面板打开时运行本产品。

怀疑产品出现故障时，请勿进行操作。如果怀疑本产品已损坏，请让合格的维修人员进行检查。

远离外露电路。电源接通后，请勿接触外露的线路和元件。

环境条件。温度范围:+10°C~+40°C；相对湿度:<90%不凝结

### 2.2 锁相放大器使用注意事项

以下情况可能会对锁相放大器造成损坏：

使用 A 或者 A-B 的输入模式时，对 A 或 B 端口输入大于 7V 峰值的电压信号；

使用 I 输入模式时，对 A 端口输入大于 7uA 峰值的电流信号；

使用Ⅰ输入模式时，对A端口输入大于7mV峰值的电压信号；

在使用过程中，Overload 值为 1，或者上位机界面显示 Overload 仍然继续使用；

对 REF-IN 端口输入大于 5V 峰值的电压信号；

对 AUX-ADC 的四个端口输入大于 10V 峰值的电压信号；

对 TRIG-IN 端口输入超出 0-5V 范围的电压信号；

对各个 output 端口输入电压信号；

在不使用锁相放大器的状态下，仍然把锁相放大器跟其他仪器连接起来；

工作电源接口输入市电范围外的电压；

强行塞入异物使风扇不转动；

搬运过程中砸到地上/撞到其他仪器/大力扔到桌上等外应力损伤；

注：如使用仪器过程中出现异常现象，请与本公司联系

联系电话：020-84133345

邮箱：support@ssi-instrument.com

单位：广州赛恩科学仪器有限公司

## 3. 锁相放大器基础

### 3.1 锁相放大器介绍

锁相放大器是用于微弱信号检测的装置，微弱信号常淹没在各种噪声中，锁相放大器可以将微弱信号从噪声中提取出来并对其进行准确测量。锁相放大器是基于互相干方法的微弱信号检测手段，其核心是相敏检测技术（Phase-Sensitive Detection），利用与待测信号有相同频率和固定相位关系的参考信号作为基准，提取出与参考信号有关的信号分量，过滤掉参考频率以外的噪声分量。

对微弱信号的最基本处理是放大，传统的放大处理在放大信号的同时，也放大了噪声，而且在不进行带宽限制或滤波处理的情况下，任何放大操作都将使得信号信噪比下降。因此，必须采用滤波手段提纯信号，提高信噪比，以实现对微弱信号的准确测量。但要实现中心频率可调而且稳定、高Q值的带通滤波器，往往十分困难。

相敏检测器（PSD）可以取代高 Q 值的带通滤波器，其基本模块包含一个将输入信号与参考信号相乘的乘法模块和一个对相乘结果进行低通滤波的滤波器模块。有时 PSD 也特指乘法模块，不包含滤波器模块。如 $ \underline{图2} $所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//a67764a5-6f47-440b-9139-9d13e49746ba/markdown_4/imgs/img_in_image_box_381_766_826_879.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A07Z%2F-1%2F%2Fbdf6fdae40a1a8ead26ef64d385bba9917269818a44ca0c55dc8f87ccf04482b" alt="Image" width="37%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图2. 相敏检测示意图</div> </div>


 $ S_{I}(t) $是掺杂了噪声的时域输入信号， $ S_{R}(t) $为与输入待测信号有相同频率关系的参考信号。PSD结合待测信号通道和参考信号通道，即可以形成一路完整的锁相放大器功能架构，称为单相型锁相放大器。其结构原理图如图3所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//a67764a5-6f47-440b-9139-9d13e49746ba/markdown_4/imgs/img_in_image_box_303_1076_885_1211.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A07Z%2F-1%2F%2F4facbbb5f3501f1469b123edf7945e83a3ac881bc9db4bcec542e924a1246df6" alt="Image" width="48%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 3. 单相型锁相放大器结构图</div> </div>


从信号通道进入 PSD 模块的信号可定义为：

 $$ S_{I}(t)=A_{I}sin\left(\omega t+\varphi\right)+\mathrm{B(t)} $$ 

其中ω是待测信号的频率， $ A_{I}\sin(\omega t+\varphi) $是待测信号，B(t)是掺杂的噪声。

参考信号通道输出的标准参考信号可定义为：

 $$ S_{R}(t)=A_{R}s i n\left(\omega t+\delta\right) $$ 

两路信号同时输入 PSD 模块进行乘法操作，得到的输出为：

 $$ \begin{aligned}S_{\mathrm{psd}}&=S_{I}(t)S_{R}(t)=A_{I}A_{R}\sin\left(\omega t+\varphi\right)\sin\left(\omega t+\delta\right)+\mathrm{B(t)}A_{R}\sin\left(\omega t+\delta\right)\\&=\frac{1}{2}A_{I}A_{R}\cos\left(\varphi-\delta\right)-\frac{1}{2}A_{I}A_{R}\cos\left(2\omega t+\varphi+\delta\right)+\mathrm{B(t)}A_{R}\sin\left(\omega t+\delta\right)\end{aligned} $$ 

上式结果有三部分，其中第一部分包含待测信号幅值 $ A_{I} $、参考信号幅值 $ A_{R} $以及输入信号相对于参考信号的相位差 $ (\varphi - \delta) $的余弦值，在输入信号有用部分与参考信号均稳定的情况下，可以认为该部分为一定值，即直流信号；同理，第二部分为原参考信号二倍频交流信号；而第三部分为噪声信号与参考信号的相乘，根据正弦信号的完备性可知，随机信号与其不具有相关性，其积分结果为零。

另一方面，从频谱来看，第一部分结果处于直流部分，第二部分在参考信号二倍频点，第三部分为原随机信号经过ω频谱搬移，以白噪声为例，搬移结果仍为白噪声。因此，将结果输入低通滤波器可以得到其直流部分如下：

 $$ S_{Output}=\frac{1}{2}A_{I}A_{R}\cos\left(\varphi-\delta\right) $$ 

虽然通过调整待测信号与参考信号的相位差 $ \left(\varphi-\delta\right) $就能确定待测信号的幅值，但是这个调整的精度是很难保证的。双相锁相放大器的产生很好的解决了这个问题。如图4所示是双相锁相放大器的原理架构图。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_0/imgs/img_in_image_box_196_751_968_964.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A56Z%2F-1%2F%2Fb92e1ad5e58c9488fe2ff79c3949c4cbe8489c059498ebcc75908e7b027d91aa" alt="Image" width="64%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图4. 双相锁相放大器结构图</div> </div>


令相位差 $ \theta=\varphi-\delta $，其中参考通道产生两个相差 $ 90^{\circ} $的正弦信号：

 $$ S_{R0}(t)=A_{R}s i n\left(\omega t+\delta\right),\ S_{R1}(t)=A_{R}\cos\left(\omega t+\delta\right), $$ 

可计算出输出结果为： $ S_{Output0} = \frac{1}{2} A_I A_R \cos \theta $， $ S_{Output1} = \frac{1}{2} A_I A_R \sin \theta $。

定义  $ X = A_{I}\cos\theta $， $ Y = A_{I}\sin\theta $，因此可计算出不依赖于相位差的输出幅值：

 $$ R=\sqrt{X^{2}+Y^{2}}=A_{I}=\frac{2\times\sqrt{S_{Output0}}^{2}+S_{Output1}^{2}}{A_{R}} $$ 

参考信号与待测信号之间的相位差可由下式得到：

 $$ \theta=tan^{-1}\left(Y/X\right) $$ 

### 3.20E1300 功能原理图

数字锁相放大器 OE1300 的原理框图如下所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_1/imgs/img_in_image_box_208_306_1022_710.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A57Z%2F-1%2F%2Fbb7d0e49472bad9b4a2b960bf332c915d8c43cf94bafc57b0a72e47be1f11e1e" alt="Image" width="68%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 5. OE1300 原理框图</div> </div>


总体来看，其功能模块大致分为信号调理通道、参考信号处理通道、算法实现模块、系统主控等部分。

### 3.3 参考通道

参考通道的功能是为相敏检测器提供与被检测信号相干的控制信号，OE1300 的参考信号可根据实际情况来选择正弦波或者方波信号，其输入阻抗为  $ 10 \, M\Omega $。

OE1300 锁相放大器有三种参考源模式，一为内部参考模式，二为外部输入模式，三为自参考模式。

内部参考模式时，仪器内部的高精度振荡器和合成算法能够产生用于与输入信号相乘的正弦波信号，此时不需用锁相环进行锁相，内部参考信号几乎不会受到相位噪声的影响。OE1300 的内部参考模式能够在 1 uHz 至 100 kHz/500 kHz 的频率范围内正常工作。由于内部振荡器与外部信号源的振荡器会有一定的频率偏差，而且没有锁相环跟踪锁定，因此内部产生的正弦信号与待测信号之间会有一定的频率差，并且不能保证两者间的相位稳定性。

外部参考模式时，正弦波信号和TTL逻辑电平可作为外部参考信号。锁相环在实际工作中会产生一定的相位抖动，这可能会造成测量的误差。相位抖动导致参考信号掺杂了不同频率的噪声，根据PSD相干原理，输出信号不仅包含有与参考信号频率相同的待测信号，还包含参考信号中其它频率的噪声。实际上，相位抖动一般比较小，不会造成测量问题。如果需要无抖动的测量，可以选用内部参考模式。由于该模式参考信号从内部晶振直接产生，没有使用锁相环，不需要调相，所以没有额外的相位抖动干扰。

外部参考模式下（TTL 和 sine）两种参考波形都可以使用，TTL 参考时，要求高电平>3V，低电平<0.5V；正弦参考为交流耦合，正弦信号幅值大于 0.4Vpp 有效。但当频率低于 1 Hz 时，必须使用 TTL 电平信号模式。由于正弦波信号在输出的幅值较小时信噪比较低，而且幅值会有抖动，而很多函数发生器都可以产生稳定的 TTL 同步信号，所以更推荐使用方波信号作为参考信号。

自参考模式时，将锁相放大器输入通道（A、A-B）的信号也作为参考信号来进行锁相，此时 REF-IN 接口无效。要注意的是，当输入信号幅值太小或者信噪比较低时，锁相环有可能不稳定，不建议用参考模式。

### 3.4 相敏检波器

OE1300 的相敏检波器（PSD）由一个数字乘法器来实现。输入信号放大滤波后由 24 bit A/D 转换器变为数字信号输入到相敏检测器。又因为内部信号发生器产生的参考信号也是位宽为 26 bit 的数字量，所以本产品的相敏检波模块的分辨率为 48 bit。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_2/imgs/img_in_image_box_450_622_760_750.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A57Z%2F-1%2F%2F1fd78874ac757210b7c8124e93a82ff60398cafb9d4a67ed7023098ba86b54c0" alt="Image" width="26%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 6. 相干检测核心部分</div> </div>


锁相放大器的相敏检波模块主要实现输入信号与参考信号的相干调制，传统的锁相放大器通过一个模拟乘法器来实现以上功能。但这种以模拟技术实现相干调制的方法存在诸多缺陷，它不仅会极大地限制相敏检波器的精度还会引入很多背景噪声，这些对于微弱信号的测量都是极为不利的。

基于以上考虑，本产品采用数字技术来实现信号的相干调制。因为内部信号发生器产生的参考信号是位宽为 24 bit 的数字信号，所以它能极大地避免谐波分量对相干调制的影响。实际上，谐波分量的抑制可达 -75 dB，这就意味着在相干调制的过程中谐波分量几乎没有影响。

另外由于模拟技术实现的相敏检波器存在温度漂移、直流偏置，所以其输出结果往往与实际结果间存在一定的偏差（即系统误差，并且这一系统误差往往带有不确定性），而以数字技术实现的相敏检波器就可以避免这一问题的产生。在系统正常工作的情况下，几乎不会产生相应的系统误差。考虑到模拟乘法器的输入量均是模拟量，所以其参考信号也会受到温度漂移效应的影响。这就会使得参考信号也会存在偏差，进而使得相干调制的结果存在更大的系统误差。

以模拟技术实现的相敏检波器的动态储备基本被限制在 60 dB 以下，这是因为在模拟系统中往往存在很多背景噪声。由于锁相放大器主要用于微弱信号的检测，所以当背景噪声的幅值与信号相接近或是比信号更大时相干调制的结果就会出错。而采用数字技术实现的相敏检波器就不存在此类问题，它的动态储备主要受 A/D 转换的质量限制。一旦输入信号完成数字化后，就不会在相干调制的过程中引入额外的误差。实际上，OE1300 的动态储备能达到 120 dB 以上。

综上可以看出，以数字技术实现的相敏检波器在各方面性能上均优于以模拟技术实现的相敏检波器，并且以数字技术实现的相敏检波器还拥有易于调试等优点，因而成为本产品的

最优选择。

### 3.5 时间常数和直流增益

相敏检波器的输出包含多种频率成分的信号，其中既有输入信号与参考信号的和频成分也有两者的差频成分以及噪声信号。并且仅当输入信号与参考信号同频时，两者的差频信号才为一直流信号。相敏检波器后端的低通滤波器能将除直流分量外的噪声信号以及和频信号滤除，以便让锁相放大器具备一个高品质带通滤波器的功能。

##### 时间常数和陡降

前面讲锁相放大器原理时讲到，信号经相敏检波后，需经低通滤波后才能输出被放大的直流信号。显然，这个低通的带宽越窄，噪声含量就越小，即信噪比越高。典型的低通滤波器如图7所示的一阶RC滤波器。降低其带宽的简单办法就是增加其数量并串联起来，我称之为级联，一阶RC滤波器的个数称为级联阶数。增加滤波器的级联阶数虽然可提高输出的信噪比，但其代价是增加了时间常数。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_3/imgs/img_in_image_box_175_627_983_815.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A58Z%2F-1%2F%2F852b201d8e50dc075076887fe5f52c2949e9a7f87e85760da5a8b4d159501664" alt="Image" width="67%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图7. 左图为一阶 RC 滤波器及其传输方程；右图为多阶级联 RC 滤波器及其传输方程</div> </div>


对于一阶低通 RC 滤波器，定义时间常数  $ \tau = RC $，其截止频率为  $ f_c = 1/(2\pi\tau) $，时间常数越大，带宽就越窄。同时，其阶跃响应如图 8 所示，可知滤波器时间常数越大，则信号阶跃响应响应越慢，稳定时间越长。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_3/imgs/img_in_chart_box_226_988_936_1367.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A58Z%2F-1%2F%2F7a990f85c837bdbb4cc08711881e563101c8db75708e54d7d8ce6b4d050b7781" alt="Image" width="59%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图8. 时间常数 $ \tau $的阶跃响应表示</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_4/imgs/img_in_chart_box_197_167_547_447.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2F6a144e1d9f1e2bfba236dae528a76b99a17993afb778a9eee14c9b12d3102b25" alt="Image" width="29%" /></div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//d17dbe9b-bd37-450c-8121-349198eb3400/markdown_4/imgs/img_in_chart_box_592_168_949_447.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2F5329c5d0c1c6d632926353c5f6f06cf2c56ab226d18e01e919e6b3ef630f03bd" alt="Image" width="29%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 9. 阶数 n=1、2、4、8 时的阶跃响应和陡降</div> </div>


如 $ \underline{图9} $所示为不同阶数RC低通滤波器的阶跃响应和幅频响应。当滤波器的时间常数t一定，阶数n越大，则系统输出从最小值逐渐增加并稳定到最大值的99%所用的时间越长，且带宽越窄。

表 1 给出了不同时间常数和级联阶数对应的系统带宽和响应时间。

因为锁相放大器中相敏检测器后端的低通滤波器设计采用多个低通滤波器级联的方式实现，所以此时稳定时间是由当前设置的时间常数大小和滤波器阶数共同决定的。

<div style="text-align: center;"><div style="text-align: center;">表1. 不同低通滤波器阶数对应的系统带宽和稳定时间</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Order</td><td style='text-align: center; word-wrap: break-word;'>Time</td><td colspan="2">Roll-off</td><td colspan="3">Bandwidth in units of 1/t</td><td colspan="4">Settling time in units of  $ \tau $</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>n</td><td style='text-align: center; word-wrap: break-word;'>constant  $ \tau $</td><td style='text-align: center; word-wrap: break-word;'>dB/oct</td><td style='text-align: center; word-wrap: break-word;'>dB/dec</td><td style='text-align: center; word-wrap: break-word;'>$ f_{3dB} $</td><td style='text-align: center; word-wrap: break-word;'>$ f_{NEP} $</td><td style='text-align: center; word-wrap: break-word;'>$ f_{NEP}/f_{3dB} $</td><td style='text-align: center; word-wrap: break-word;'>63.2%</td><td style='text-align: center; word-wrap: break-word;'>90%</td><td style='text-align: center; word-wrap: break-word;'>99%</td><td style='text-align: center; word-wrap: break-word;'>99.9%</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>20</td><td style='text-align: center; word-wrap: break-word;'>0.159</td><td style='text-align: center; word-wrap: break-word;'>0.250</td><td style='text-align: center; word-wrap: break-word;'>1.57</td><td style='text-align: center; word-wrap: break-word;'>1.00</td><td style='text-align: center; word-wrap: break-word;'>2.30</td><td style='text-align: center; word-wrap: break-word;'>4.61</td><td style='text-align: center; word-wrap: break-word;'>6.91</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>12</td><td style='text-align: center; word-wrap: break-word;'>40</td><td style='text-align: center; word-wrap: break-word;'>0.102</td><td style='text-align: center; word-wrap: break-word;'>0.125</td><td style='text-align: center; word-wrap: break-word;'>1.23</td><td style='text-align: center; word-wrap: break-word;'>2.15</td><td style='text-align: center; word-wrap: break-word;'>3.89</td><td style='text-align: center; word-wrap: break-word;'>6.64</td><td style='text-align: center; word-wrap: break-word;'>9.23</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>18</td><td style='text-align: center; word-wrap: break-word;'>60</td><td style='text-align: center; word-wrap: break-word;'>0.081</td><td style='text-align: center; word-wrap: break-word;'>0.094</td><td style='text-align: center; word-wrap: break-word;'>1.16</td><td style='text-align: center; word-wrap: break-word;'>3.26</td><td style='text-align: center; word-wrap: break-word;'>5.32</td><td style='text-align: center; word-wrap: break-word;'>8.41</td><td style='text-align: center; word-wrap: break-word;'>11.23</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>24</td><td style='text-align: center; word-wrap: break-word;'>80</td><td style='text-align: center; word-wrap: break-word;'>0.069</td><td style='text-align: center; word-wrap: break-word;'>0.078</td><td style='text-align: center; word-wrap: break-word;'>1.13</td><td style='text-align: center; word-wrap: break-word;'>4.35</td><td style='text-align: center; word-wrap: break-word;'>6.68</td><td style='text-align: center; word-wrap: break-word;'>10.05</td><td style='text-align: center; word-wrap: break-word;'>13.06</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>30</td><td style='text-align: center; word-wrap: break-word;'>100</td><td style='text-align: center; word-wrap: break-word;'>0.061</td><td style='text-align: center; word-wrap: break-word;'>0.069</td><td style='text-align: center; word-wrap: break-word;'>1.12</td><td style='text-align: center; word-wrap: break-word;'>5.43</td><td style='text-align: center; word-wrap: break-word;'>7.99</td><td style='text-align: center; word-wrap: break-word;'>11.60</td><td style='text-align: center; word-wrap: break-word;'>14.79</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>36</td><td style='text-align: center; word-wrap: break-word;'>120</td><td style='text-align: center; word-wrap: break-word;'>0.056</td><td style='text-align: center; word-wrap: break-word;'>0.062</td><td style='text-align: center; word-wrap: break-word;'>1.11</td><td style='text-align: center; word-wrap: break-word;'>6.51</td><td style='text-align: center; word-wrap: break-word;'>9.27</td><td style='text-align: center; word-wrap: break-word;'>13.11</td><td style='text-align: center; word-wrap: break-word;'>16.45</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>42</td><td style='text-align: center; word-wrap: break-word;'>140</td><td style='text-align: center; word-wrap: break-word;'>0.051</td><td style='text-align: center; word-wrap: break-word;'>0.057</td><td style='text-align: center; word-wrap: break-word;'>1.11</td><td style='text-align: center; word-wrap: break-word;'>7.58</td><td style='text-align: center; word-wrap: break-word;'>10.53</td><td style='text-align: center; word-wrap: break-word;'>14.57</td><td style='text-align: center; word-wrap: break-word;'>18.06</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>48</td><td style='text-align: center; word-wrap: break-word;'>160</td><td style='text-align: center; word-wrap: break-word;'>0.048</td><td style='text-align: center; word-wrap: break-word;'>0.053</td><td style='text-align: center; word-wrap: break-word;'>1.10</td><td style='text-align: center; word-wrap: break-word;'>8.64</td><td style='text-align: center; word-wrap: break-word;'>11.77</td><td style='text-align: center; word-wrap: break-word;'>16.00</td><td style='text-align: center; word-wrap: break-word;'>19.62</td></tr></table>

用模拟技术实现的滤波器很难实现大于 100 s 的时间常数，这是因为此时所需的电容不仅在数值上还是在规格上都过大。但为什么需要如此大的时间常数？因为在某些情况下是别无选择的。例如当参考信号的频率低于 1 Hz 并且存在很多低频噪声干扰时，相敏检波器的输出就会包含很多低频成分的干扰。同步滤波器仅能滤除其中参考信号的谐波分量，其余的噪声信号需要由其后置的滤波器来滤除。

可以简单地认为，时间常数越大，阶数越高，输出的带宽就越低，显示的测量幅度、相位等值就越稳定。然而，过大的时间常数会抹平输入信号（随时间）的变化，从而失去有用的信息。因此，在实际应用中，需要根据输入信号随时间变化的情况，协调时间常数与信噪比之间的平衡。

OE1300 滤波器时间常数能  $ 1 \, us $ 到  $ 3 \, ks $ 调节，这能满足大多数测量的需求。

##### 数字滤波器与模拟滤波器对比

为了尽量提升 OE1300 的性能，我们采用数字滤波器来实现对相干调制结果的低通滤波处理。与大多数模拟系统与数字系统的对比一样，数字系统拥有很多模拟系统所不具备的优

势。首先模拟器件固有的温度漂移和非线性将极大的限制滤波器的滚降程度。其次，要通过模拟器件搭建一个时间常数大、高品质的低通滤波器需要占据相当大的电路板面积，这不仅会使得仪器的成本上升，而且大量的模拟器件也会为今后的调试带来很大的难度。

本产品采用数字技术实现的低通滤波器是一个 48 bit 位宽，直流增益为 0 dB，等效 Q 值最高达 145 dB 以上的窄带滤波器。

##### 同步滤波器

数字滤波器的另一个优势是可以轻松搭建同步滤波器。即使输入信号没有噪声，相敏检波器的输出仍会包含输入信号与参考信号的和频分量（二倍频分量），并且这一和频分量幅值可能会大于所需的差频分量幅值。在频率较低的情况下，要过滤掉二倍频分量所需要的时间常数会很大。例如输入信号是1 Hz频率的波形，那么二倍频分量即为2 Hz，即使是10秒时间常数的二阶RC滤波器，对于2 Hz频率位置的衰减也只有40多dB。

同步滤波器是把参考频率的一个完整周期时间内的所有数据作平均算法，可以有效过滤参考频率的所有倍频分量。在上述的例子中，如果用了同步滤波器，只需要1秒的等待时间，即可以实现比10秒时间常数的RC滤波器更好的效果。

在 OE1300 中，同步滤波器被设置为当检测频率低于 1 kHz 时有效。因为频率更高时，和频分量能够在时间常数较小的情况下被移除，所以此时没必要使用同步滤波器。在同步滤波器的后端我们还设计了八阶滤波器，这样的滤波器组合不仅能够滤除参考信号的谐波分量，还能滤除其余的噪声信号。

##### 直流输出增益

相敏检测器的直流输出能力，它取决于动态储备的大小。当动态储备为 60 dB 时，允许噪声信号比待测信号大 1000 倍。在相敏检测器中，噪声信号不能超过相敏检测器的输入范围。在一个模拟锁相放大器中，假设相敏检测器的最大输入幅值为 5 V。在它的动态储备为 60 dB 时，相敏检测器输入端的信号将只有 5 mV。而相敏检测器是不会放大信号的，所以其输出仅有几毫伏。即使相敏检测器的直流输出没有误差，后端的放大器直接将其放大 1000 倍到 5 V，也很容易使信号失真。如果 PSD 有 1 mV 的偏移量，则将在输出端变为 1 V 的输出。这就是为什么基于模拟技术的相敏检测器不能达到太高的动态储备的原因。

因为基于数字技术的锁相放大器没有采用模拟直流放大器，所以数字锁相放大器不存在直流输出的偏置。数字直流放大器也不存在输入偏移量。数字直流放大器只需将接受到的数据与预先设定好的增益相乘，再将结果输出即可。这就是 OE1300 在动态储备能达到 120 dB 时仍能不受偏置影响的原因。

### 3.6 直流输出和增益

OE1300 有 CH1 和 CH2 两个输出通道。CH1 和 CH2 的接口采用标准 SMB 接头，集成在 OE1300 面板。

##### CH_{1} 和 CH_{2} 的输出与显示

CH1 和 CH2 的输出范围为 -10 V 到 +10 V，最小分辨率达 1.2 mV。根据当前对信号的测量结果与当前设置输入范围按比例输出。此外，OE1300 还可以通过指令集配置 Channel Out 控件中选择 CH1 和 CH2 的数据源（包括被测信号的 X 值、Y 值、R 值、 $ \theta $ 值等数据）以及偏移与增益。

##### X，Y 和 R 的输出偏移与增益

OE1300 能够通过设置偏移量以抵消测量时的误差。这对于测量值在某些标称值附近存在误差的情况下是极其有用的。因为偏移量可以在范围内任意设置，所以输出的偏移量可以说几乎为零。输出的变化可以直接从显示屏或面板的输出端读出。偏移量以满刻度输出的百分比形式表示，并且这一比值不会因为范围的变化而改变。偏移量最多可以设置为满刻度输出的±100%。

另外还能对 X、Y 和 R 的输出值放大。这一功能是通过给输出数据乘以一个增益系数来实现的。因此，一个仅有满偏刻度十分之一的信号经过放大后能提供 10 V 的输出而不是 1 V 的输出。输出信号增益的作用一般是在某些非零值的附近增加测量分辨率。

在不超过满偏刻度的情况下，OE1300 能够提供增益系数为+0.001 ～ +10000 的多个档位的输出增益。其输出的计算公式为：

 $$ Output=\left(\frac{Signal}{Range}+Offset\right)\times Expand\times10(V) $$ 

<Offset>可在 -100% ～ +100% 之间进行设置，可通过数字键盘直接输入，最小步进为 0.01%；<Expand>值可在 +0.001 ～ +10000 之间进行设置，可通过数字键盘直接输入，最小步进为 1。例如：

 $$ Output=\left(\frac{0.1mV}{1mV}+0.2\right)\times2\times10(V)=6(V) $$ 

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ac32d628-de64-470d-be1c-25c08c21962e/markdown_2/imgs/img_in_image_box_221_146_993_578.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F9359f8bedd6c0c64d074db8a7fd83ac58450a50e3e86e62ad74e423df1666045" alt="Image" width="64%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 10. 输出偏移与增益设置</div> </div>


### 3.7 动态储备

动态储备的定义是最大可容纳的噪声信号和满量程信号的比值。动态储备表示锁相放大器对噪声容忍程度的大小，通常以 dB 表示。

 $$  动态储备 =20lg\frac{OVL}{FS}(dB) $$ 

其中 OVL 表示输入总动态范围，FS 是最大量程，表示输出动态范围。若动态储备为 100dB，表示系统能容忍的噪声可以比有用信号高出  $ 10^{5} $ 倍。

实际上动态储备设置应该保证整个实验过程中不发生过载，过载还可能出现在前置放大器的输入端和DC放大器的信号输出端。系统的输入增益与动态储备成反比，因为噪声也会随着输入增益而放大，因此可以通过减少输入增益来实现高动态储备。前级放大倍数设置为较合理范围，以防止噪声过载，经过PSD和低通滤波器滤掉了大部分噪声后，直流放大倍数设置为较大值，将信号放大到满量程。

锁相放大器的输入信号在 PSD 处理之前需要交流放大，而在 PSD 处理之后进行直流放大即可。在总增益不变的情况下，如果调整交流增益增加，直流增益减小，则输入噪声经交流放大很容易使 PSD 过载，动态储备减小，同时输出的直流漂移减小。反之，如果增加直流增益，降低交流增益，则动态储备提高，使锁相放大器具有良好的抗干扰能力，但以输出稳定性为代价，降低了测量精度。

直流放大输出精度受噪声的频率和幅值影响。幅值较大且与信号频率相同的噪声经过PSD后同样变成直流信号，这样经过低通滤波器时直接叠加到输出，对输出结果造成影响。

动态储备与噪声频率有关。在参考频率处的动态储备为0，远离参考频率时动态储备增加，离参考频率足够远时，动态储备可达到最大值。参考频率附近的动态储备对仪器噪声容限极其重要，增加低通滤波器的级数可以提高滤波效果，从而增加参考频率附近的动态储备。远离参考频率的动态储备一般比较大，但一般对测量影响不大。

OE1300 动态储备可达 120 dB 以上，高的动态储备会产生输出噪声和漂移。当动态储备较高时，由于模数转换器的噪声存在导致输出误差增加。所有的信号源都存在本底噪声，因

此在 PSD 提取信号过程中就会掺杂着噪声，如果噪声很大，在高动态储备测量中就会产生较大的输出误差。如果外部噪声较小，则其输出主要是受 OE1300 自身噪声影响。这时可以通过降低动态储备和直流增益来降低输出误差。因此，在实际应用中应尽量使用较低动态储备，即较高的输入增益。

### 3.8 信号输入放大和滤波

锁相放大器可以测量低至纳伏级的微弱信号。模数转化器可以将模拟信号数字化，但信号必须达到能被识别的强度。因此低噪声信号放大器必须有足够大的增益，将信号放大到可直接被模数转化器转化的程度，而无需降低信号的信噪比。OE1300 的模拟放大倍数增益大约在 0.3 到 1300 倍。

直流信号和交流信号的总增益由范围确定，两者各自的增益则由动态储备设定。

##### 输入噪声

OE1300 信号放大器的输入噪声约为 5nVrms/ $ \sqrt{Hz} $。如果放大器的输入噪声为 5 nVrms/ $ \sqrt{Hz} $，增益为 1000 倍，那么将输出 5 $ \mu $Vrms/ $ \sqrt{Hz} $ 噪音。假设放大器的输出为一阶 RC 低通滤波器（6 dB/oct 的滚降），RC 过滤器的时间常数为 100 ms。放大器的输入噪声和电阻的约翰逊噪声具有高斯噪声性质，其噪声的量正比于该噪声带宽的平方根。单级 RC 滤波器的等效噪声带宽（ENBW）为  $ 1/(4 \times TC) $。这意味着，对滤波器输入的高斯噪声进行滤波，其有效带宽等于 ENBW。在这个例子中，滤波器输入端有 5 $ \mu $Vrms/ $ \sqrt{Hz} $ 噪声，其等效噪声带宽为 2.5 Hz，滤波器的输出电压噪声为 5 $ \mu $Vrms/ $ \sqrt{Hz} \times \sqrt{2.5Hz} = 7.9\mu $Vrms。对于高斯噪声，噪声峰值是噪声有效值的 6.6 倍左右。因此，输出有大约 52 $ \mu $V 峰值噪声。

锁相放大器的输入噪声同理。在 500uV 及以下的量程内，输入增益达到最大，输入噪声的大小将决定输出噪声。而低通滤波器的等效噪声带宽又影响输出的噪声量。

等效噪声带宽取决于时间常数和滤波器滚降（参考 $ \underline{\text{3.5章}} $）。例如，将OE1300设定到<5μV>量程，设置时间常数为<100 ms>以及<6 dB/oct>的滚降，则其等效噪声带宽为2.5 Hz。这个设置下，等效到输入端的噪声为7.9 nVrms，输出为量程的0.16%（即7.9nV/5μV），噪声峰峰值则是满量程的1%左右。

假定信号是由一个低阻抗信号源发出的。其中电阻约翰逊噪声为  $ 0.13 \times \sqrt{R} $，以  $ 100\ \Omega $ 电阻为例，常温下其约翰逊噪声为  $ 1.3\ nV_{rms}/\sqrt{Hz} $。而一个阻抗为  $ 2\ k\Omega $ 的信号源的约翰逊噪声  $ 5.8nV_{rms}/\sqrt{Hz} $ 都大于 OE1300 的自身输入噪声。系统总噪声大小由各个噪声源的平方之和后开根号计算出来。例如，一个  $ 2\ k\Omega $ 阻抗的信号源接入到 OE1300，它自身的约翰逊噪声和 OE1300 的输入噪声叠加起来，总噪声大小为  $ \sqrt{5^2 + 5.8^2} = 7.7nV_{rms}/\sqrt{Hz} $。

在增益较低时，经过放大后的噪声信号仍然低于模数转化器的自身噪声，此时系统的输出噪声主要是模数转换器噪声，但这种情况下的滤波器之后的直流增益很低，输出的噪声相对于有用信号可忽略不计。

##### 抗混叠滤波器

输入信号经过放大电路之后，会通过抗混叠滤波器，这是信号的数字化处理前必须要完成的。根据奈奎斯特定理，采样频率至少是信号频率的两倍。比如信号频率是 100 kHz，那至少需要 200 kHz 采样频率才能进行采样。OE1300 的 A/D 转换器采样频率是 4MHz，A/D 转换器无法转换高于 2 MHz 频率的信号，高于 2 MHz 的信号会违反奈奎斯特定理，导致欠采样。欠采样的结果是 A/D 转换器输出的数字流中，高频的信号将出现在低频部分，即信号发生混叠，造成测量错误。

为了避免欠采样这种情况，先将模拟信号进行低通滤波处理，消除信号超过 2 MHz 的高频部分。OE1300 的低通滤波器具有平坦的通带（0-500 kHz），在这个频率范围内的信号不会受影响。高于 102 kHz 的高频部分信号会被逐渐衰减，从 500 kHz 至 2 MHz 是过渡阶段，对高于 2 MHz 频率的信号和噪声产生 100 dB 的衰减。

##### 输入阻抗

OE1300 的输入阻抗是 10 MΩ。如果需要更高的输入阻抗，可以使用前置放大器 OE400X 系列。OE400X 系列前置放大器的输入阻抗可达 100 MΩ 或更高，满足用户的各种使用场景。

### 3.9 输入端连接

噪声存在于所有的电路中。即使在信号幅值较大的情况下，噪声也会降低测量的精度。为了得到最佳测量精度，必须注意减少实验环境中可以避免的噪声源。除了系统固有噪声之外，其他噪声源（如市电噪声、信号发生器的噪声、在空间分布的电磁场等）的影响和不同仪器之间的地电平差、地环路问题，可以在输入连线的环节降低影响。

我们的仪器有两种输入连接的方式，单端连接和差分连接。单端连接非常的方便，而差分连接则能有效消除噪声的影响。

##### 单端连接模式（A 或 I）

单端连接模式中，使用 A 或 I 输入端。锁相放大器检测 A 或 I 输入接口的中心导体和外壳导体之间的电压差。

单端连接模式对噪声抵抗能力较弱。单根信号线就像天线，会被环境的电磁噪声所影响，屏蔽层会吸收这些噪声，因为单端连接模式是检测中心信号线和屏蔽层的电压差，因此这些噪声会被带入锁相放大器内部。

##### 差分连接模式（A-B）

差分连接模式有两根信号线连接到信号源，每一根接到对应的输入端（A、B）中。这个模式下检测 A 和 B 接口的中心导体之间的电压差，两个接口的外壳屏蔽层吸收的噪声不会被锁相放大器获取。

使用差分连接模式有一个需要注意的地方，两个输入端的电缆应该紧密缠绕，不应形成环路，以免产生电磁感应，从而给测量带来误差。

##### 电流输入模式（1）

电流输入模式同样使用 A/I 输入端。这个模式下输入阻抗为 1 kΩ，电流增益为  $ 10^6 $。测量量程是 1 fA 至 5 μA。这个模式适用于源阻抗大于 1 MΩ（对应增益）的小电流测量。另外信号线的分布电容应该尽量较小，以免影响电流模式的测量带宽。

电流增益的带宽，如 $ \underline{\text{表2}} $所示：

<div style="text-align: center;"><div style="text-align: center;">表2. 电流增益和带宽关系</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>增益</td><td style='text-align: center; word-wrap: break-word;'>带宽</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>$ x 10^{6} V/A $</td><td style='text-align: center; word-wrap: break-word;'>70kHz</td></tr></table>

##### 交流耦合和直流耦合模式

OE1300 对输入的信号有交流耦合和直流耦合两种模式。交流耦合通过一阶 RC 高通滤波器（-3 dB 频率是 0.16 Hz）来滤除直流和较低频率信号，交流耦合应该在信号频率大于 10 Hz （保证通带平坦度）的情况下使用。对于低于 10 Hz 频率的信号，应该使用直流耦合模式。直流耦合模式不对输入信号有任何阻碍。

如果输入信号含有直流成分，若没有被去除，那会带来几个隐患：在放大电路中直流分量也会被放大，如果被放大到超过 A/D 转换器的输入范围，那么就会使测量结果产生误差，也有可能损坏 A/D 转换器。另外直流量被 A/D 转换器量化成数字量后，在 PSD 里会和参考正弦信号相乘，那么需要更强大的低通滤波器才能滤除，会导致需要更久的测量时间。

当待测信号的频率大于  $ 10 \, Hz $ 时，建议使用交流耦合模式。

### 3.10 固有噪声

##### 噪声

从主观的角度出发，可认为凡是不希望得到或者有碍于准确测量的输入或影响均可称之为噪声。噪声具有瞬时性和不可预知性的随机性。几乎所有测量领域，探测弱信号的最终限制因素都在于噪声。即使要测量的信号并非很弱，噪声的存在也会降低测量精度。某些形式的噪声是无法避免的（例如待测信号的抖动），只有通过信号平均和缩小带宽等技术来克服。而另一些形式的噪声（例如射频干扰和接地回路）可以由很多技术来消除或降低，包括滤波和良好的线路结构和元件布局。同时，放大器本身在工作时也会产生噪声，可以通过低噪声放大器设计技术解决这一问题。

电子系统中存在各种各样的固有噪声源，这些噪声有它们的物理含义。

##### 约翰逊噪声 (Johnson Noise)

任何一种无源器件，其导体中的电子始终在做随机运动，其两端会因此产生一个噪声电压，这就是约翰逊噪声，也称为白噪声或热噪声。它存在于所有电子器件和传输介质中。它受温度变化的影响，但与频率变化无关。从频域上看，热噪声在整个频段具有均匀的功率谱密度，即类似于白色光谱，它不能够被消除的，因此是电子系统性能的上限的影响因素之一。在温度为T时，由一个电阻R产生的实际噪声电压由下式计算出来：

 $$ \mathrm{V}=\sqrt{4\mathrm{kTRB}} $$ 

其中 k 为玻尔兹曼常数， $ k=1.38\times10^{-23} $ J/K，T 是以开尔文为单位的热力学温度(热力学温度与摄氏度的转换关系为： $ ^{0} $K= $ ^{0} $C+273.16)，B 是以赫兹为单位的带宽。

随后，Nyquist 利用热力学推理以数学方式描述了热噪声的统计特性，并证明了热噪声功率谱函数为：

 $$ \mathrm{S t(f)=4K T R(V^{2}/H z)} $$ 

例如，室温下，将一个 10K 电阻接入高保真放大器的输入端，输出端接伏特表，用带宽为 10 kHz 的滤波器来测量它的开路有效电压，结果为 1.3 uV。

热噪声电压的瞬间幅度在任何情况下一般来说都是不可预见的，但是它遵循高斯分布。其意义就在于它是任何检波器、信号源或者放大器的噪声电压的下限。源内阻的阻抗部分会产生热噪声，放大器的偏置和负载电阻也同样如此。

##### 散射噪声（Shot Noise）

电流其实是一股离散的电荷流，而不是一种真正的流体。电荷量的有限性导致了电流的统计性起伏。如果电荷之间互不影响，那么电流的波动就由下式给定：

 $$ I_{\mathrm{n o i s e}}=\sqrt{2\mathrm{q I B}} $$ 

其中 q 为电子电荷(1.6 × 10⁻¹⁹ C)，I 为电路中 RMS 电流值，B 为测量带宽。例如，一个稳定的 1 A 电流，在 10 kHz 范围内测量，其有效值波动为 57 nA，也就是在 0.000,006% 上下波动。对于更小的电流，其波动更大：一个稳定的 1 uA 的电流在 10 kHz 范围内测量值的均方电流波动为 57 pA，也就是 0.006% 的波动。对于 1 pA 的电流，均方电流噪声波动为 56 fA(在同样

带宽测量)，也就是5.6%的波动！

随后证明了散弹噪声电流也是一种白噪声，其功率谱密度函数为：

 $$ \mathrm{S_{s l}(f)=2q I(A^{2}/H z)} $$ 

前面给出的散射噪声公式是假设电流中的载流子互不影响而得出的。当电荷通过一个势垒时，这种假设确实是存在的，例如面接触型二极管中的电流是以电荷的扩散形式传播的。但是对于最常见的金属导体来说就不是这样，其载流子之间有着很密切的联系。

## 1/f 噪声（Flicker Noise）

1925 年，Johnson 在电子管电流中首次发现 1/f 噪声，其突出特点在于该噪声的功率谱函数正比于 1/f。频率越低，噪声越严重，因为又称为低频噪声。其微观机理在于当两种导体接触不理想时，其接触电阻将发生随机涨落，从而引起噪声。

尽管对 1/f 噪声研究已达数十年，然后其适用的情形不一从而有许多的描述模型。其电流幅度满足高斯分布，功率谱密度正比于工作频率的倒数，起功率谱密度函数表示为：

 $$ \mathrm{S}(\mathrm{f})=\frac{K I_{d}^{2}}{f}(V^{2}/Hz) $$ 

1/f 噪声也叫闪烁噪声（flicker noise），是有源器件中载波密度的随机波动而产生的，它会对中心频率信号进行调制，并在中心频率上形成两个边带，降低了振荡器的 Q 值。由于 1/f 噪声是在中心频率附近的主要噪声，因此在设计器件模型时必须考虑到它的影响。

散射噪声和热噪声都是由于物理特性而产生的不可避免的噪声。对于相同阻值的电阻，制作精良的电阻和便宜的炭阻所产生的热噪声完全一样。另外，实际设备都会有各种各样的过量噪声源。实际中的电阻都存在阻值的波动，其结果是产生一个附加的噪声电压(与永久存在的热噪声叠加在一起)，其值与流经它的直流电流成正比。这一噪声和很多与电阻构造相关的因素有关，其中包括电阻的材料，特别是封装技术。以纯炭阻，碳膜电阻，金属膜电阻和绕线电阻为例，绕线电阻的噪声最小，金属膜电阻次之，炭膜电阻再次之，纯炭阻最大。

### 3.11 外部噪声源

内部固有噪声是难以避免的，只有尽可能减少这种噪声的大小。相对于固有噪声而言，外部噪声的形式各种各样，而且绝大多数的噪声源都是异步的。外部噪声源主要通过增加动态储备和时间常数的要求，进而影响了测量的时间。少数的噪声源和参考信号联系紧密，与实际测量信号相加或相减，造成测量结果的错误。幸好，外部噪声源可以通过多种途径尽可能减少。

##### 电容耦合

由于布线之间总是有互容，互容如同寄生在布线之间的一样，所以叫寄生电容，又称为杂散电容。极板与周围体（各种元件甚至人体）也产生电容联系。而在锁相放大器附近的交流电压信号可以用过这些寄生电容耦合到设备上。虽然寄生电容可能很小，但耦合来的电压信号仍然有可能比待测微弱信号要大。

寄生电容的影响可由以下公式计算出来：

 $$ \mathrm{I}=\omega\mathrm{C}_{\mathrm{stray}}\mathrm{V}_{\mathrm{n o i s e}} $$ 

其中，ω是噪声频率的  $ 2\pi $ 倍， $ C_{stray} $ 为寄生电容容值， $ V_{noise} $ 是噪声的振幅。

当噪声源频率变大时，耦合噪声将会变大。如果噪声源和参考频率一致，对测量结果的影响会很大。因为锁相放大器会滤除其他频率的噪声，但是会把与参考频率一致的噪声当作信号进行测量。

减少电容耦合的方法：

移除噪声源，或者尽量把噪声源远离仪器和信号线。

设计低阻抗的实验装置，这样耦合的噪声电流就只会产生很小的噪声电压。

· 容性屏蔽，例如将整套实验装置放入金属盒中。

##### 电感耦合

交流电附近会感应出一个磁场，如果放置器件在交流电附近，感应的磁场会耦合到电路中进而影响电路。变化的交流电会产生变化的磁场，变化的磁场感应产生电动势，感应电动势会影响电路的电流电压，进而使实验的测量发生偏差。电动势的大小和磁场变化的频率有关，频率越快，电动势越大，对实验的测量影响就越大。

减少感性耦合的方法：

• 尽可能移除仪器附近的噪声源。

使用双绞线或者紧密缠绕的两根同轴电缆线以减小环路效应。

对仪器进行磁性屏蔽，防止磁场进入并穿透测量的区域。

##### 颤动噪声效应

大部分噪声源都是以电气的形式影响电路，然而机械振动的噪声也可通过颤噪效应转化成为电气形式。因微振动而使传输电缆或者待测信号产生的机械振动，会产生频率变化的电形式噪声。

消除颤噪效应的方法：

在测量时，尽可能地减少机械的振动。

传输微弱信号的传输线应绑紧固定以减少它们的颤动。

用低噪声的电缆来替代普通电缆以减少颤噪效应。

##### 热电偶效应

热电偶效应，指的是两种不同的金属相互接触时在它们之间产生的电势差。产生接触电势差的原因是：（1）两种金属电子的逸出功不同。（2）两种金属的电子浓度不同。若 A、B 两种金属的逸出功分别为  $ V_{a} $ 和  $ V_{b} $，电子浓度分别为  $ N_{a} $ 和  $ N_{b} $，则它们之间的接触电势差为

 $$ \mathrm{V}_{a b}=\mathrm{V}_{a}-\mathrm{V}_{b}+\frac{\mathrm{kT}}{q}\times\ln\left(\frac{\mathrm{Na}}{N_{b}}\right) $$ 

其中 k 为玻尔兹曼常数， $ k = 1.38 \times 10^{-23} $J/K。T 是以开尔文为单位的热力学温度(热力学温度与摄氏度的转换关系为： $ ^0\text{K} = ^0\text{C} + 273.16 $，其中 q 为电子电荷( $ 1.60 \times 10^{-19} $ C)。由上式可得知接触电势数值决定于金属的性质和接触面的温度，因不同金属的功函数(电子逸出金属表面所需的功)不同而产生。

当两种金属接触时，在接触点产生的电动势会在原电平的基础上增加了一个缓慢变化的毫伏级的电平。这种噪声与温度密切相关，由于温度变化缓慢，因为这种噪声频率也很低。热电偶效应会随着检测器输出变大而增长，在低频率时影响较大，尤其是 mHz 级别的测量时，影响更大。

消除热电偶效应的方法：

测量仪器尽可能保持在恒温状态。

使用补偿特性的节点。

### 3.12 噪声测量

OE1300 提供噪声测量功能，可以测量输入信号在参考频率下的噪声。部分噪声源对频率有相关性，锁相放大器可以对这些噪声源进行测量。

根据用户设定的 RC 滤波器的带宽，锁相放大器可以理解为以参考频率为中心频率，通带带宽为 RC 滤波器带宽两倍的带通滤波器。因此参考频率附近的噪声会保留在输出端。当输入信号就是一个噪声源，那么锁相放大器就可以测量其在设定频率上的噪声值。若把频率按照扫频的方式测量，还能得到噪声源的噪声功率谱图。

OE1300 测量噪声的方法是首先计算一段时间内 X 值的均方差，其含义是参考频率附近一定带宽内的总噪声。而这个带宽就是 PSD 之后的数字滤波器的带宽，因为不同带宽得到的噪声是不同的，所以接下来要进行归一化处理。把计算得到的均方差除以数字滤波器的等效噪声带宽的平方根（ $ \sqrt{ENBW} $），得到的噪声谱密度就是需要的测量值，其单位是 V/√Hz。

### 3.13 辅助模拟输入（AUX IN）

OE1300 包含了两路 16 位的高精度辅助 AUX-ADC 输入，输入电压范围为 ±10 V，最小分辨率达 0.3mV，采样率为 500ksps。这两路 ADC 提供输入信号钳位保护和内部差分放大功能，输入阻抗达 1MΩ，可同时进行信号采集，用来测量低速模拟信号，或者测量从某个实验得到的直流信号（例如来自温度传感器或者压力传感器），以便于进行比例运算和传送给控制计算机。

### 3.14 信号发生器的频率、幅值扫描

OE1300 新增频率、幅值扫描功能。内部信号发生器可以在输出幅值和频率上进行扫描。频率的扫描可以实现对用户感兴趣频段的扫描，更好的分析信号的特点。扫幅功能通过 Sine output 输出大大增加了在实际中的应用范围。

频率和幅值的扫描基本方式都是通过设定起点值和终点值，并在这两个值之间以步进的方式增加数值，完成扫描。详细见频率扫描，幅值扫描相关章节。

### 3.15 示波器功能

示波器模拟带宽 1 MHz。垂直分辨率 24 bits，最高实时采样率 4 MSPS，超低本底噪声。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_0/imgs/img_in_chart_box_180_140_1009_602.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F893d017b6864e3bae1645cc992f236125cc42313a3d5502cecfec33ca546fd6d" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 11. 示波器功能窗口</div> </div>


### 3.16 频谱分析功能

FFT（Fast Fourier Transformation），即快速傅里叶变换，是一种将信号的频域信息提取出来的算法。很多时候，信号在时域上很难看出特征，但是如果变换到频域之后，就很容易看出该信号在各个频率上的分量，以便于对信号进行全方位的评估。

FFT 是在 DFT（Discrete Fourier Transformation）基础上进行改进获得的。直接计算 DFT 的计算量与变换区间长度 N 的平方呈正比。当 N 较大时，例如 N=1024，则需要 1048576 次运算，因此直接用 DFT 算法进行频谱分析和信号实时处理是很耗费资源的。FFT 的算法思想即是不断把长序列的 DFT 分解成多个短序列的 DFT，并利用周期性和对称性来减少 DFT 运算次数。例如把 N 序项分为两个 N/2 项的序列，每个 N/2 点 DFT 变换需要(N/2)^2 次运算，再用 N 次运算把两个 N/2 点的 DFT 变换组合成一个 N 点的 DFT 变换。这样变换以后，总的运算次数就变成  $ N+2*(N/2)^2=N+N^2/2 $。继续上面的例子，N=1024 时，总的运算次数就变成了 525312 次，节省了大约 50%的运算量。

在 OE1300 中开发并集成了 FFT 高精度频谱分析功能，可以进行在 DC 至 2 MHz 的范围内实现频域分析，并实时显示信号的频谱分布。如 $ \underline{图12} $所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_1/imgs/img_in_chart_box_180_145_961_516.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F5bcd336be253b7c5e79293a0a62c5596072a5c88ff96e4ce3ad0df8e4ff53752" alt="Image" width="65%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 12. FFT 功能窗口</div> </div>


注意：该频谱分析、示波器功能只有在使用以太网接口连接 Console 时才能使用。

## 3. 17 PID 控制

PID 控制器是指比例-积分-微分控制器，由比例单元（Proportional），积分单元（Integral）和微分单元（Derivative）组成。PID 控制器是一个在工业控制应用中常见的反馈回路部件。这个控制器把收集到的数据和一个参考值进行比较，然后把这个差别用于计算新的输入值，这个新的输入值的目的是可以让系统的数据达到或者保持在参考值。PID 控制器可以根据历史数据和差别的出现率来调整输入值，使系统更加准确而稳定。

PID 控制器的比例单元（P）、积分单元（I）和微分单元（D）分别对应目前误差、过去累计误差及未来误差。

在连续时间域中，PID算法可以表示为：

 $$ \mathrm{u}(\mathrm{t})=\mathrm{K}_{\mathrm{p}}e(t)+K_{i}\int_{0}^{t}e(\tau)d\tau+K_{d}\frac{d}{d t}e(t) $$ 

其中

K_{p}：比例增益

 $ K_{i} $：积分增益

 $ K_{d} $：微分增益

e：误差 = 设定值（Set Point）- 反馈值（process variable）

t: 目前时间

 $ \tau $：积分变数，数值从0到目前时间t

PID 控制器可以视为是频域系统的滤波器。在计算控制器最终是否会达到稳定结果时，此性质很有用。PID 控制器的传递函数为：

 $$ H(s)=K_{p}+\frac{K_{i}}{s}+K_{d}s $$ 

为了增强系统的稳定性，可在微分项中添加一阶低通滤波器，得到传递函数为：

 $$ H(s)=K_{p}+\frac{K_{i}}{s}+\frac{K_{d}s}{1+T_{f}s} $$ 

其中  $ T_{f} $ 为滤波器时间常数

 $$ T_{f}=\frac{1}{\omega}=\frac{1}{2\pi f_{BW}} $$ 

其中：

ω：滤波器特征角频率

f_{BW}：滤波器带宽（截止频率）

OE1300 中集成了 2 路独立的 PID 控制器，每路都具有亚微秒延迟。PID 原理框图如图 $ \underline{13} $:

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_2/imgs/img_in_image_box_206_402_1003_657.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2F4e1cca00250ed3ebf2304944aacd519c0a872461c0387a2ebdd2f5e4c1d50ed4" alt="Image" width="66%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 13. PID 原理框图</div> </div>


使用时，可根据外部 DUT 的传递函数推导出 PID 控制器的传递函数及其系数  $ K_{p} $， $ K_{i} $， $ K_{d} $，将系数写入 OE1300 的 PID 模块并启动，使得系统可以快速稳定到设定值。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_2/imgs/img_in_image_box_180_770_1049_1297.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2F7825a7cbdb02c8ad6882bbedbadf21a630fe1d41c97af46b816e7a0997306d1e" alt="Image" width="72%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 14. PID 控制界面</div> </div>


除了  $ K_{p} $， $ K_{i} $， $ K_{d} $，这三个系数，PID 模块还需要设置其他参数使其可以正常工作：

A）输入源：既可以选择锁放功能8个通道的解调结果，X，Y，R，Xita等，也可以选辅助输入Aux IN；

B）采样频率：也就是 PID 模块的运行速率。

C）设定值（Set Point）：预设的输入源的目标值。

D）积分饱和限幅：如果负载突变，引起误差的跳变，那么很容易出现饱和效应，此时，误差e(k)将会在很长一段时间内保持正值，而且值比较大，会使得积分项非常大，当到达误差为0的情况时，积分项不能很快归0，而出现严重的超调现象。这是由于积分项引起的，所以叫积分饱和，需要设置其最高及最低限幅，降低超调。

E）微分项滤波器截止频率  $ f_{BW} $： $ f_{BW} $ 需要满足： $ f_{BW} < f_s / \pi $， $ f_s $ 为采样频率，否者滤波器不起作用。

F）输出偏置值：PID 输出的初始值

G）输出限幅：可以设置输出最大值和最小值

以上这些参数都可以根据实际需要进行设置。

### 3.18 多解调器

谐波是指周期函数或者周期性波形中能用常数、与原函数的最小周期相同的正弦函数和余弦函数的线性组合表达的部分。根据傅立叶级数的原理，周期函数都可以展开为常数与一组具有共同周期的正弦函数和余弦函数之和。其展开式中，常数表达的部分称为直流分量，最小正周期等于原函数的周期的部分称为基波或一次谐波，最小正周期的若干倍等于原函数的周期的部分称为高次谐波。

传统的锁相放大器中，同一时间只能测量基频信号或者某个谐波信号分量。在很多的实际应用上，往往需要对多个谐波的同时测量和记录。这时，目前的锁相放大器就很难满足要求了。

OE1300 突破性的开发了多谐波同时测量功能，可以最多同时进行 8 个通道谐波分量的测量。原来需 8 台锁相放大器完成的工作，现在 OE1300 一台即可完成。

对多谐波的测量设置在软件[Frequency]窗口中<Demodulator>中进行；

在软件[Measure Selection]窗口中<Demodulator>中选择对应数值显示窗口。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_3/imgs/img_in_image_box_180_908_1012_1375.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2F2bdcf69e3f74d4d01a40d90b25771dc3d227d06e47b13781fde1eaf5c72f40c9" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 15. 多谐波数值方式显示</div> </div>


## 4. 产品概述

OE1311/OE1351 是一款模块化锁放。作为一款模块化的锁放，它仍然具有齐全的功能，包括：电压信号单端输入，电压信号差分输入，电流信号输入；TLL 参考信号，正弦参考信号输入；TTL 参考信号输出，Sine 信号输出，以及 2 路辅助信号输出和 2 路辅助信号输入。

得益于其多级程控放大器以及较大的动态储备配置，输入信号幅度可以设置为 1nV 至 5V 或 1fA~5uA。

同时，其使用了高端的 Zynq 系列 SOC，应用其强大的运算能力，可以同时实现 1 路基波及 7 路谐波解调通道的测量。且 7 路谐波还可以设置为任意频率解调模式。

### 4.1 接口

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//7453d22b-7561-44ec-afb3-38e06ab61458/markdown_4/imgs/img_in_image_box_262_607_928_906.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2Ffc415d2024ad9c922d621e8128f259f952a9ad413b8ef1b9fba5dfe34cd9ef87" alt="Image" width="55%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 16. OE1311/OE1351 接口指示</div> </div>


信号接口分别是：I，+V,-V Diff，REF IN，AUX In1，AUX In2，OUTPUT1，OUTPUT2，SINE OUT，TTL OUT.

通信接口分别是：RJ45隔离网络接口，RS232串口（可选UART TTL串口）。

电源接口：12V 电源输入

### 4.2 信号接口

电流信号输入，输入阻抗  $ 1\mathrm{k}\Omega $，输入电流幅度 $ \leqslant5\mathrm{uA} $。

+V

电压信号输入正端，输入阻抗  $ 10 \, M\Omega / 10 \, pF $，输入电压幅度  $ \leq 5 \, V $。

##### -V Diff

电压信号输入负端，输入阻抗 10 MΩ//10 pF，输入电压幅度≤5 V，仅在电压信号差分输入模式下有用。

REF IN

参考输入，可以使用正弦波或 TTL 方波驱动。正弦波输入时输入阻抗为  $ 1 \, M\Omega $，交流耦合。对于低频应用的情况(<1 Hz)，推荐使用 TTL 方波的参考信号。

最大输入电压： $ \leq5\ V $（TTL 模式）； $ \leq10\ V_{pp} $（Sine 模式）

##### AUX In1

辅助输入 1，输入 DC 范围±10 V，最小分辨率为 0.1 mV，输入阻抗 1 MΩ AUX In2

辅助输入2，输入 DC 范围±10 V，最小分辨率为 0.1 mV，输入阻抗 1 MΩ

##### OUTPUT1

辅助输出 1：输出 DC 范围±10 V，最小分辨率为 1.2 mV。

##### OUTPUT2

辅助输出2：输出DC范围 $ \pm10V $，最小分辨率为1.2mV。

##### SINEOUT

信号发生器：提供最大 5 Vrms 幅值的可编程正弦波输出，输出阻抗为  $ 50\ \Omega $。当外部参考信号使用时，信号发生器通过锁相环与参考信号进行锁相。

TTL OUT

TTL OUT 输出接口提供 5 V TTL/CMOS 兼容的方波信号，输出阻抗为 200 Ω，其频率与 SINE OUT 相同。

### 4.3 通信接口

## 1、RJ45 网络接口

隔离式 1000 Mbps

2、串口

接口端子：RS232 （可选端子 XH2.54-4PIN TTL 电平）

#### 4.3.1 UART 串口通信协议

UART 作为异步串口通信协议的一种，工作原理是将传输数据的每个字符一位接一位地传输。

通信协议说明如下：

起始位：先发出一个逻辑 “0” 的信号，表示传输字符的开始。

资料位：紧接着起始位之后。数据位的个数可以是4、5、6、7、8等，构成一个字符。通常采用ASCII码。从最低位开始传送，靠时钟定位。

奇偶校验位：资料位加上这一位后，使得“1”的位数应为偶数（偶校验）或奇数（奇校验），以此来校验资料传送的正确性。

停止位：它是一个字符数据的结束标志。可以是 1 位、1.5 位、2 位的高电平。由于数据是在传输线上定时的，并且每一个设备有其自己的时钟，很可能在通信中两台设备间出现了小小的不同步。因此停止位不仅仅是表示传输的结束，并且提供计算机校正时钟同步的机会。适用于停止位的位数越多，不同时钟同步的容忍程度越大，但是数据传输率同时也越慢。

空闲位：处于逻辑 “1” 状态，表示当前线路上没有资料传送。

波特率：是衡量资料传送速率的指标。表示每秒钟传送的二进制位数。例如资料传送速率为120字符/秒，而每一个字符为10位，则其传送的波特率为 $ 10 \times 120 = 1200 $位/秒=1200波特。

#### 4.3.2 UART 协议配置

可选波特率：9600，115200（default），921600

校验位：无

数据位：8

停止位：1

## 5. 远程编程

### 5.10E1300 命令语法

上位机与 OE1300 的通信使用 ASCII 字符来进行。命令符使用大写，所有命令均由四个命令字符（如有必要可带上参数）和一个命令终结符组成。终结字符必须是一个回车符<cr>。OE1300 只有在收到命令终结符时，才会执行用户输入的命令。命令可能需要一个或多个参数，多个参数之间用逗号分隔(，)。

多个命令可以在同一命令行发送，但命令之间需要添加分号(;)。

OE1300 有一个容量为 64byte 字符的输入缓存区，并根据接收命令的顺序来处理命令。当缓存区写满时，最新命令将会把最旧并已执行命令覆盖。建议一次输入多个命令时不要超过 64byte 字符。

OE1300 允许用户通过命令查询内部参数的当前值。查询命令的格式为由当前命令后加上一个问号 “?” 并省略原命令所需的一个或多个参数。OE1300 以 ASCII 字符串的形式返回用户所查询的参数，如果一个命令行中发送多个查询(用分号隔开)的话，应答将会按顺序一个一个地返回，每个返回值后尾都跟着一个终结符。

命令格式举例：



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>FMOD 1 &lt;cr&gt;</td><td style='text-align: center; word-wrap: break-word;'>设置参考源为内部参考</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>FREQ 10E3 &lt;cr&gt;</td><td style='text-align: center; word-wrap: break-word;'>设置内部参考信号频率为 10 kHz</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>OUTP? 1 &lt;cr&gt;</td><td style='text-align: center; word-wrap: break-word;'>查询通道 1 的输出值</td></tr></table>

### 5.2 详细的命令列表

每一个命令所指定的参数是有严格顺序的，不同参数之间用逗号(,)分隔。在大括号{}里面的参数是可选的，不需要每个都填写。只有在命令后面加上(?)的助记符时，才会启动查询命令，没有(?)是不会查询的。大部分情况下，查询命令不需要发送大括号{}内的内容。注意：在发送命令时()和{}都不需要发送。

变量定义如下：



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>i,j,k,l,m,n,o,p,q,r,s,t,u</td><td style='text-align: center; word-wrap: break-word;'>整数</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>x</td><td style='text-align: center; word-wrap: break-word;'>实数</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>f</td><td style='text-align: center; word-wrap: break-word;'>频率值</td></tr></table>

以上所有的数值变量均可以被表示为整数、浮点数或指数格式（例如，数字5可以表示为5，5.0，0.5E1）。而字符串则被作为一个ASCII字符序列的形式发送。

#### 5.2.1 输入方式指令



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>ISRC (?) {i}</td><td style='text-align: center; word-wrap: break-word;'>ISRC 指令用于设置或查询输入信号的方式。\ni=0 时选择 A(单端电压信号输入);\ni=1 时选择&lt;A-B&gt;(差分电压输入);\ni=2 时选择&lt;I&gt;(电流信号输入)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>ICPL (?) {i}</td><td style='text-align: center; word-wrap: break-word;'>ICPL 指令用于设置或查询输入耦合方式。\ni=0 时选择&lt;AC&gt;(交流耦合输入);\ni=1 时选择&lt;DC&gt;(交流耦合输入)。</td></tr></table>

#### 5.2.2 范围与时间常数指令



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="17">IRNG (?) {i}</td><td colspan="2">IRNG 指令用于设置或查询系统量程&lt;input range&gt;。参数 i 用于选择不同的量程。\n具体如下：</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>i/range</td><td style='text-align: center; word-wrap: break-word;'>i/range</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0: 5V/uA</td><td style='text-align: center; word-wrap: break-word;'>15: 50uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1: 2V/uA</td><td style='text-align: center; word-wrap: break-word;'>16: 20uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2: 1V/uA (default)</td><td style='text-align: center; word-wrap: break-word;'>17: 10uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3: 500mV/nA</td><td style='text-align: center; word-wrap: break-word;'>18: 5uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4: 200mV/nA</td><td style='text-align: center; word-wrap: break-word;'>19: 2uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5: 100mV/nA</td><td style='text-align: center; word-wrap: break-word;'>20: 1uV/pA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6: 50mV/nA</td><td style='text-align: center; word-wrap: break-word;'>21: 500nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7: 20mV/nA</td><td style='text-align: center; word-wrap: break-word;'>22: 200nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8: 10mV/nA</td><td style='text-align: center; word-wrap: break-word;'>23: 100nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>9: 5mV/nA</td><td style='text-align: center; word-wrap: break-word;'>24: 50nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10: 2mV/nA</td><td style='text-align: center; word-wrap: break-word;'>25: 20nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>11: 1mV/nA</td><td style='text-align: center; word-wrap: break-word;'>26: 10nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>12: 500uV/pA</td><td style='text-align: center; word-wrap: break-word;'>27: 5nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>13: 200uV/pA</td><td style='text-align: center; word-wrap: break-word;'>28: 2nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>14: 100uV/pA</td><td style='text-align: center; word-wrap: break-word;'>29: 1nV/fA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>OFLT (?) {x}</td><td colspan="2">OFLT 指令用于设置或查询滤波器的时间常数。\n参数 x 代表时间常数, 单位 s, 取值范围 1E-6 ~ 3000, 0.01(default)。</td></tr><tr><td rowspan="10">OFSL (?) {i}</td><td colspan="2">OFSL 指令用于设置或查询低通滤波器的陡降。\n参数 i 用于选择不同陡降。\n具体如下：</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>i</td><td style='text-align: center; word-wrap: break-word;'>Filter dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>6 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>12 dB/oct(default)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>18 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>24 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>30 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>36 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>42 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>48 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>SYNC (?) {i}</td><td colspan="2">SYNC 指令用于设置或查询同步滤波器的开关状态。一般当参考频率低于 1000 Hz 时需要开启同步滤波器。\n同步滤波器只有低通滤波器陡降≥18 dB/oct 且参考频率低于 1 kHz 时有效。\ni=0: 关闭同步滤波器; (default)\ni=1: 时开启同步滤波器。</td></tr></table>





#### 5.2.3 参考与相位指令



<table border="1" style="margin: auto; word-wrap: break-word;"><tr><td style="text-align: center; word-wrap: break-word;">PHAS (?) i, {x}</td><td style="text-align: center; word-wrap: break-word;">PHAS 指令用于设置或查询解调通道相移。\n参数 i 是解调通道数，取值：0~7，0 代表基波，1~7 代表谐波通道；\n参数 x 是指相位值（实数值，单位：°）。\n相位限制在 $ \pm 180° $。</td></tr><tr><td style="text-align: center; word-wrap: break-word;">FMOD (?) {i}</td><td style="text-align: center; word-wrap: break-word;">FMOD 指令用于设置或查询参考信号源。\ni=0 时选择外部参考；\ni=1 时选择内部参考。\ni=2 时选择输入自参考信号</td></tr><tr><td style="text-align: center; word-wrap: break-word;">FREQ (?) {f}</td><td style="text-align: center; word-wrap: break-word;">FREQ 指令用于设置或查询内部参考信号的频率，f 值可设置范围 1 mHz-100 kHz，最小分辨率为 1 mHz。指令 FREQ?会返回当前的参考信号频率（内部或外部）</td></tr><tr><td style="text-align: center; word-wrap: break-word;">RSLP (?) {i}</td><td style="text-align: center; word-wrap: break-word;">当使用外部参考源时，RSLP 指令用于设置或查询参考信号当前的触发方式。\ni=0 时设置&lt;TTL&gt;上升沿触发；\ni=1 时设置&lt;TTL&gt;下降沿触发；\ni=2 时设置正弦波过零检测&lt;Sine&gt;。\n当频率低于 2 Hz 时，须使用&lt;TTL&gt;触发方式。</td></tr><tr><td style="text-align: center; word-wrap: break-word;">DMOD (?) i {j}</td><td style="text-align: center; word-wrap: break-word;">DMOD 指令用于设置或查询 7 个解调器的模式。</td></tr><tr><td style="text-align: center; word-wrap: break-word;"></td><td style="text-align: center; word-wrap: break-word;">参数 i 必须设置. 取值 0~6\ni=0 时选择解调器 1,\ni=1 时选择解调器 2,\n......\ni=6 时选择解调器 7;\n参数 j 是用于设置解调器的模式\nj=0 时选择谐波模式,\nj=1 时选择任意频率解调模式\n指令 DMOD?i 会返回当前查询的解调器的解调模式。</td></tr><tr><td style="text-align: center; word-wrap: break-word;">HARM (?) i { j }</td><td style="text-align: center; word-wrap: break-word;">HARM 指令用于设置或查询谐波阶数。\n参数 i 必须设置. 取值 0~6 之间的整数\ni=0 时选择解调器 1,\ni=1 时选择解调器 2,\n......\ni=6 时选择解调器 7;\n参数 j 可以设置为 1 到 65535 之间的整数，表示谐波阶数。\nHARM i, j 指令将会设置检测输入参考频率的 j 次谐波。参数 j 必须满足  $ j \times f \leq 100/500\text{ kHz} $。如果 j 次谐波的值大于 100/500 kHz，那么谐波次数 j 会被自动设置为满足条件  $ j \times f \leq 100/500\text{ kHz} $ 的 j 的最大值。</td></tr><tr><td style="text-align: center; word-wrap: break-word;">DARB (?) i { f }</td><td style="text-align: center; word-wrap: break-word;">DARB 指令用于设置或查询解调器的任意频率模式时的参考频率。\n参数 i 必须设置. 取值 0~6 之间的整数\ni=0 时选择解调器 1,\ni=1 时选择解调器 2,\n......\ni=6 时选择解调器 7;\n参数 f 值可设置范围 1uHz-100/500 kHz，最小分辨率为 1 uHz。</td></tr></table>




#### 5.2.4 正弦波输出指令



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>SWVT (?) {i}</td><td style='text-align: center; word-wrap: break-word;'>SWVT 指令用于设置或查询&lt;Sine Output&gt;的输出类型\ni=0 时选择关闭 Sineout 和 TTL out 输出；\ni=1 时选择开启 Sineout 和 TTL out 输出</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>SLVL (?) {x}</td><td style='text-align: center; word-wrap: break-word;'>SLVL 指令用于设置或查询输出的同步正弦波固定幅值模式的幅度。\n参数 x 指幅度电压（实数值，单位：V）。参数 x 必须满足 100 uVrms ≤ x ≤ 5 Vrms，最小分辨率为 10 uVrms。</td></tr></table>

#### 5.2.5 CH 通道输出指令



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="25">COUT(?) i {, j}</td><td colspan="2">COUT 指令用于设置或查询 AUXOUT 输出通道。\n发送该指令时参数 i 必须设置\ni=0 时选择 CH1；\ni=1 时选择 CH2；\n参数 j 用于选择输出值的类型。\n具体如下：</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>j</td><td style='text-align: center; word-wrap: break-word;'>CH 通道源</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>FIXED</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>X</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>Y</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>R</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>\theta</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>XD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>YD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>RD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>\theta D1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>9</td><td style='text-align: center; word-wrap: break-word;'>XD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10</td><td style='text-align: center; word-wrap: break-word;'>YD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>11</td><td style='text-align: center; word-wrap: break-word;'>RD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>12</td><td style='text-align: center; word-wrap: break-word;'>\theta D2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>13</td><td style='text-align: center; word-wrap: break-word;'>XD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>14</td><td style='text-align: center; word-wrap: break-word;'>YD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>15</td><td style='text-align: center; word-wrap: break-word;'>RD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>16</td><td style='text-align: center; word-wrap: break-word;'>\theta D3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>17</td><td style='text-align: center; word-wrap: break-word;'>XD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>18</td><td style='text-align: center; word-wrap: break-word;'>YD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>19</td><td style='text-align: center; word-wrap: break-word;'>RD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>20</td><td style='text-align: center; word-wrap: break-word;'>\theta D4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>21</td><td style='text-align: center; word-wrap: break-word;'>XD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>22</td><td style='text-align: center; word-wrap: break-word;'>YD5</td></tr></table>



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="15"></td><td rowspan="15"></td><td style='text-align: center; word-wrap: break-word;'>23</td><td style='text-align: center; word-wrap: break-word;'>RD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>24</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>25</td><td style='text-align: center; word-wrap: break-word;'>XD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>26</td><td style='text-align: center; word-wrap: break-word;'>YD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>27</td><td style='text-align: center; word-wrap: break-word;'>RD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>28</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>29</td><td style='text-align: center; word-wrap: break-word;'>XD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>30</td><td style='text-align: center; word-wrap: break-word;'>YD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>31</td><td style='text-align: center; word-wrap: break-word;'>RD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>32</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>33</td><td style='text-align: center; word-wrap: break-word;'>X-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>34</td><td style='text-align: center; word-wrap: break-word;'>Y-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>35</td><td style='text-align: center; word-wrap: break-word;'>Frequency</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>36</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>37</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>CAUX(?)i{x}</td><td colspan="3">CAUX指令用于设置或查询AUXOUT输出的电压值。\n参数i对应CH的通道\ni=0时对应CHOUT1;\ni=1时对应CHOUT2。\n参数x用于设置DAC的电压值，范围是-10.000V≤x≤10.000V。\n例如发送指令CAUX 1,5.00，会设置&lt;CHOUT2&gt;的AUXOUT模式的输出值为5.00V。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>COFP(?)i{x}</td><td colspan="3">COFP指令用于设置或查询CHOUT通道输出的偏置值。\n参数i对应CH的通道\ni=0时对应CHOUT1;\ni=1时对应CHOUT2。\n参数x用于设置偏置值，范围是(-100.00≤x≤100.00)%，最小分辨率为0.01。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>CEXP(?)i{j}</td><td colspan="3">CEXP指令用于设置或查询CHOUT通道输出的放大倍数值。\n参数i对应CH的通道\ni=0时对应CHOUT1;\ni=1时对应CHOUT2。\n参数j用于设置输出放大倍数，范围是(0.001≤x≤10000)的整数。</td></tr></table>

#### 5.2.6 PID 设置



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="11">*PID(?)i,j {,x}</td><td colspan="2">其中 i 代表配置那个通道，j 代表 PID 的某个功能的编码，x 是具体的设置参数。\ni: PID 通道\ni = 0 对应通道 1；\ni = 1 对应通道 2；\nj (PID 功能选项地址) 和 x (PID 参数) 的取值及其含义见下表：</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>j</td><td style='text-align: center; word-wrap: break-word;'>功能说明及 x 取值</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>保留/Reserve</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>使能/关闭 PID 模块：\nx = 0: 关闭 PID 模块\nx = 1: 使能 PID 模块</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>输入源选择/Input Source Select，可以选择所有 8 个锁放解调通道的输出 X, Y, R, Xita 或者 Aux IN1, Aux IN2 等。\nx 取值范围为：0~31 以及 34~36，对应参数见 SNAP 指令的参数。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>输出目的地选择，\nx 取值范围：0~3\nx = 0: Aux Out1\nx = 1: Aux Out2\nx = 2: Sine Out\nx = 3: Int Frequency</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>采样间隔（通过采样间隔设置采样率）\nx 取值范围：0~31（整数）\nSample Rate = 4MHz/2 $ ^{{x}} $；\n举例：\nx = 0 时，Sample Rate = 4MHz\nx = 1 时，Sample Rate = 2MHz\n......\nx = 31 时，Sample Rate = 1.86MHz</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>保留/Reserve</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>系数  $ K_p $，x 取值范围：实数</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>系数  $ K_i $，x 取值范围：实数</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>系数  $ K_d $，x 取值范围：实数</td></tr><tr><td style='text-align: center; word-wrap: break-word;'></td><td style='text-align: center; word-wrap: break-word;'>9</td><td style='text-align: center; word-wrap: break-word;'>微分项滤波器带宽 Filter Bandwidth\n(Hz) 带宽设置需要满足：\n\nFilter Bandwidth &lt; Sample Rate\n/  $ \pi $\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\</td></tr></table>





#### 5.2.7 波特率设置



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>BAUD(?) {}}</td><td style='text-align: center; word-wrap: break-word;'>BAUD 指令用于设置或查询串口波特率。\n参数 j 对应波特率值\n参数 j 值可设置：9600、115200(default)、921600。</td></tr></table>

#### 5.2.8 网口参数设置



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>NMOD(?) {i}</td><td style='text-align: center; word-wrap: break-word;'>NMOD 指令用于设置或查询网口连接模式。参数 i 值可设置：\ni = 0 时选择 TCP 模式\ni = 1 时选择 DHCP 模式</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>NIPA(?) {s}</td><td style='text-align: center; word-wrap: break-word;'>NIPA 指令用于设置或查询网口的 IP 地址。\n参数 s 值格式为：x:x:x:xx 取值范围：0~255。\n默认值为 192.168.1.1\n例如发送指令 NIPA 192.168.1.5，会设置网口的 IP 地址为 192.168.1.5。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>NSMA(?) {s}</td><td style='text-align: center; word-wrap: break-word;'>NSMA 指令用于设置或查询网口的子网掩码。\n参数 s 值格式为：x:x:x:xx 取值范围：0~255。\n默认值为 255.255.255.0</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>NGWA(?) {s}</td><td style='text-align: center; word-wrap: break-word;'>NSMA 指令用于设置或查询网口的网关地址。\n参数 s 值格式为：x:x:x:xx 取值范围：0~255。\n默认值为 192.168.1.10</td></tr></table>

#### 5.2.9 存读取设置指令



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>*RST</td><td style='text-align: center; word-wrap: break-word;'>RST 指令用于复位。设备内部的所有状态与参数都会重置为默认值，数据缓存区内的数据也会丢失。*号是为了补齐 4 位命令字符。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>*IDN ?</td><td style='text-align: center; word-wrap: break-word;'>IDN ?指令用于查询模块锁放的 ID，格式为“SSI OE1300, SNXXXXXX, Version: VXXX”。其中第一个为型号，如 OE1300；第二个为序列号，如 SN00001；第三个为硬件版本号，如</td></tr></table>

Version: V1.00.*号是为了补齐4位命令字符。

#### 5.2.10 数据和状态读取指令

RALL?指令用于读取所有的解调结果数据以及Aux IN数据。



<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="37">RALL?</td><td style='text-align: center; word-wrap: break-word;'>数据顺序编码</td><td style='text-align: center; word-wrap: break-word;'>Parameter</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>X</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>Y</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>R</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>XD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>YD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>RD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>XD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>9</td><td style='text-align: center; word-wrap: break-word;'>YD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10</td><td style='text-align: center; word-wrap: break-word;'>RD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>11</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>12</td><td style='text-align: center; word-wrap: break-word;'>XD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>13</td><td style='text-align: center; word-wrap: break-word;'>YD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>14</td><td style='text-align: center; word-wrap: break-word;'>RD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>15</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>16</td><td style='text-align: center; word-wrap: break-word;'>XD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>17</td><td style='text-align: center; word-wrap: break-word;'>YD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>18</td><td style='text-align: center; word-wrap: break-word;'>RD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>19</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>20</td><td style='text-align: center; word-wrap: break-word;'>XD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>21</td><td style='text-align: center; word-wrap: break-word;'>YD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>22</td><td style='text-align: center; word-wrap: break-word;'>RD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>23</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>24</td><td style='text-align: center; word-wrap: break-word;'>XD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>25</td><td style='text-align: center; word-wrap: break-word;'>YD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>26</td><td style='text-align: center; word-wrap: break-word;'>RD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>27</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>28</td><td style='text-align: center; word-wrap: break-word;'>XD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>29</td><td style='text-align: center; word-wrap: break-word;'>YD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>30</td><td style='text-align: center; word-wrap: break-word;'>RD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>31</td><td style='text-align: center; word-wrap: break-word;'>$ \theta $D7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>32</td><td style='text-align: center; word-wrap: break-word;'>X-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>33</td><td style='text-align: center; word-wrap: break-word;'>Y-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>34</td><td style='text-align: center; word-wrap: break-word;'>Frequency</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>35</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'></td><td style='text-align: center; word-wrap: break-word;'>36</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN2</td></tr><tr><td rowspan="10"></td><td style='text-align: center; word-wrap: break-word;'>SNAP ? i,j 指令用于在同一个时间点记录最多 10 个不同的参数值。例如，指令 SNAP? 用于在同一时刻查询&lt;X&gt;、&lt;Y&gt;、&lt;R&gt;、&lt; \theta &gt;或&lt;F&gt;等值，该功能在时间常数很短的时候非常实用。因为如果使用 OUTP?指令来连续读取两个不同的参数值，两个参数返回值之间会有一定的延时，使得读取的两个数据不是在同一时刻下测量所得，当数据变化速度较快时，就可能导致一定误差。
SNAP?i,j 指令需要至少 2 个参数，最多可以同时读取 10 个。参数 i,j,k,l,m,n,o,p,q,r 的选择具体如下：</td><td style='text-align: center; word-wrap: break-word;'>SNAP ? i,j {k,l,m,n,o,p,q,r}</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>i,j,k,l,m,n,o,p,q,r</td><td style='text-align: center; word-wrap: break-word;'>Parameter</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>X</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>Y</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>R</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>0</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>XD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>YD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>RD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>\theta D1</td></tr><tr><td rowspan="23">SNAP ? i,j {k,l,m,n,o,p,q,r}</td><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>XD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>9</td><td style='text-align: center; word-wrap: break-word;'>YD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10</td><td style='text-align: center; word-wrap: break-word;'>RD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>11</td><td style='text-align: center; word-wrap: break-word;'>\theta D2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>12</td><td style='text-align: center; word-wrap: break-word;'>XD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>13</td><td style='text-align: center; word-wrap: break-word;'>YD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>14</td><td style='text-align: center; word-wrap: break-word;'>RD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>15</td><td style='text-align: center; word-wrap: break-word;'>\theta D3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>16</td><td style='text-align: center; word-wrap: break-word;'>XD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>17</td><td style='text-align: center; word-wrap: break-word;'>YD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>18</td><td style='text-align: center; word-wrap: break-word;'>RD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>19</td><td style='text-align: center; word-wrap: break-word;'>\theta D4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>20</td><td style='text-align: center; word-wrap: break-word;'>XD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>21</td><td style='text-align: center; word-wrap: break-word;'>YD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>22</td><td style='text-align: center; word-wrap: break-word;'>RD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>23</td><td style='text-align: center; word-wrap: break-word;'>\theta D5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>24</td><td style='text-align: center; word-wrap: break-word;'>XD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>25</td><td style='text-align: center; word-wrap: break-word;'>YD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>26</td><td style='text-align: center; word-wrap: break-word;'>RD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>27</td><td style='text-align: center; word-wrap: break-word;'>\theta D6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>28</td><td style='text-align: center; word-wrap: break-word;'>XD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>29</td><td style='text-align: center; word-wrap: break-word;'>YD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>30</td><td style='text-align: center; word-wrap: break-word;'>RD7</td></tr><tr><td rowspan="6"></td><td style='text-align: center; word-wrap: break-word;'>31</td><td style='text-align: center; word-wrap: break-word;'>0D7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>32</td><td style='text-align: center; word-wrap: break-word;'>X-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>33</td><td style='text-align: center; word-wrap: break-word;'>Y-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>34</td><td style='text-align: center; word-wrap: break-word;'>Frequency</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>35</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>36</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>OUTP ? i</td><td colspan="2">返回的值是一个单一的字符串，该字符串内不同值之间用逗号(,)分隔，并且顺序是按照发送指令时i,j,k,l,m的顺序返回。例如，发送SNAP?0,1,34,3；会依次返回&lt;X&gt;、&lt;Y&gt;、&lt;Frequency&gt;和&lt;0&gt;的值。这些值均放在同一个字符串中，例如：
"0.951359,0.0253297,1000.00,1.234"。第一个是&lt;X&gt;值，第二个是&lt;Y&gt;值，第三个是频率值，第四个是&lt;0&gt;值。
该指令对比OUTP指令好处是可以同时获取多个数据，这些数据都是同一时刻的，不会存在延时。
OUTP ? i指令用于读取单个参数值。参数i对应于下表：
参数i的选择具体如下：</td></tr><tr><td rowspan="28">OUTP ? i</td><td style='text-align: center; word-wrap: break-word;'>i</td><td style='text-align: center; word-wrap: break-word;'>Parameter</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>0</td><td style='text-align: center; word-wrap: break-word;'>X</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>1</td><td style='text-align: center; word-wrap: break-word;'>Y</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2</td><td style='text-align: center; word-wrap: break-word;'>R</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>3</td><td style='text-align: center; word-wrap: break-word;'>0</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>4</td><td style='text-align: center; word-wrap: break-word;'>XD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5</td><td style='text-align: center; word-wrap: break-word;'>YD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>6</td><td style='text-align: center; word-wrap: break-word;'>RD1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>7</td><td style='text-align: center; word-wrap: break-word;'>0D1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>8</td><td style='text-align: center; word-wrap: break-word;'>XD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>9</td><td style='text-align: center; word-wrap: break-word;'>YD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10</td><td style='text-align: center; word-wrap: break-word;'>RD2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>11</td><td style='text-align: center; word-wrap: break-word;'>0D2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>12</td><td style='text-align: center; word-wrap: break-word;'>XD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>13</td><td style='text-align: center; word-wrap: break-word;'>YD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>14</td><td style='text-align: center; word-wrap: break-word;'>RD3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>15</td><td style='text-align: center; word-wrap: break-word;'>0D3</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>16</td><td style='text-align: center; word-wrap: break-word;'>XD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>17</td><td style='text-align: center; word-wrap: break-word;'>YD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>18</td><td style='text-align: center; word-wrap: break-word;'>RD4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>19</td><td style='text-align: center; word-wrap: break-word;'>0D4</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>20</td><td style='text-align: center; word-wrap: break-word;'>XD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>21</td><td style='text-align: center; word-wrap: break-word;'>YD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>22</td><td style='text-align: center; word-wrap: break-word;'>RD5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>23</td><td style='text-align: center; word-wrap: break-word;'>0D5</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>24</td><td style='text-align: center; word-wrap: break-word;'>XD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>25</td><td style='text-align: center; word-wrap: break-word;'>YD6</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>26</td><td style='text-align: center; word-wrap: break-word;'>RD6</td></tr></table>










<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="10"></td><td rowspan="10"></td><td style='text-align: center; word-wrap: break-word;'>27</td><td style='text-align: center; word-wrap: break-word;'>$ \theta D6 $</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>28</td><td style='text-align: center; word-wrap: break-word;'>XD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>29</td><td style='text-align: center; word-wrap: break-word;'>YD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>30</td><td style='text-align: center; word-wrap: break-word;'>RD7</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>31</td><td style='text-align: center; word-wrap: break-word;'>$ \theta D7 $</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>32</td><td style='text-align: center; word-wrap: break-word;'>X-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>33</td><td style='text-align: center; word-wrap: break-word;'>Y-Noise</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>34</td><td style='text-align: center; word-wrap: break-word;'>Frequency</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>35</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN1</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>36</td><td style='text-align: center; word-wrap: break-word;'>AUX-IN2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>OAUX ? i</td><td colspan="3">OAUX 指令用于查询 AUX-IN 接口的输入电压值。\n参数 i 必须设置\ni=0 时读取 AUX-IN1；\ni=1 时读取 AUX-IN2；\n查询返回结果以伏特(V)为单位，但单位不会输出。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>OVLD ?</td><td colspan="3">OVLD?; 指令用于查询 ADC Overload 的状态。\n查询返回结果是 0 或者 1。\n0 表示 ADC 没有溢出；\n1 表示 ADC 发生溢出。此时需要把输入信号减小或者调整 input range 到更大量程。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>*PLL ?</td><td colspan="3">*PLL?; 指令用于查询锁相环的状态。查询返回结果是 0 或者 1。\n0 表示现在锁相环没有锁定或者是处于内部参考模式；\n1 表示锁相环已经锁定。</td></tr></table>

## 6. PC 软件安装使用说明

### 6.1 软件驱动安装

我们一般都是以 U 盘的形式把 PC 机软件提供给用户的，均可在 Android,Linux mac Windows 系统上运行。打开 U 盘后有如下文件，如 $ \underline{图17} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_285_415_893_604.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fdba5ea9bf3543641c0ae615d1fd1a0cb8106dea0e0e213b5105a498ba0bc9595" alt="Image" width="51%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 17. U 盘内 PC 软件包</div> </div>


首先要安装串口转 USB 驱动，打开 $ \underline{图17} $中的第3个文件夹"串口驱动"，如 $ \underline{图18} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_262_775_856_937.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fc4f0c73c00d2e14464e92384b0e79bcb1d31ff1008b039b0b701dbc6ac8ff1a5" alt="Image" width="49%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 18. “串口驱动”文件夹</div> </div>


选择打开对应系统的文件夹，如 Windows 系统举例，选择打开文件夹进入底层文件下，如 $ \underline{图19} $所示

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_239_1067_953_1184.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fea6466a13834ff600971f81cd23307b48ec06a3b1c5daf9cb8e77b1537245c41" alt="Image" width="59%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 19. 串口驱动-Windows 系统文件的底层文件</div> </div>


右键“以管理员身份运行” $ \underline{图19} $红色方框内的“PL2300-M_LogoDriver_Setup.exe”文件，等待进度条加载完成后则会弹出如 $ \underline{图20} $的软件窗口，点击 Next 只需要等待几分钟，见到以下界面 $ \underline{图21} $时按下 Finish 后完成安装。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_2/imgs/img_in_image_box_276_153_914_628.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F52cbc5ddb8e6f16e25cc942227cb4bcc59525cf4616703dd30b82dba91899591" alt="Image" width="53%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 20. PL2300-M_LogoDriver 安装界面</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_2/imgs/img_in_image_box_276_698_913_1177.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F59133e79ae36db8082a1f7e806342e6ae2ff26494ea4f33687eaf567343027b6" alt="Image" width="53%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 21. PL2300-M_LogoDriver 安装完成提示</div> </div>


此时，使用 USB 线连接 PC 机和锁相放大器，则可自动识别连接成功。

1. 如果 PC 机已经联网，当插上 USB 连接 PC 与锁相放大器时，会自动联网搜索驱动并进行安装。

2. 如果 PC 机已安装有串口转 USB 的驱动，则可跳过该步。

### 6.2 软件 Console 安装

若前面的安装步骤都确定没有问题后，用户则可打开图13中的第1个文件夹"Console"，有如下文件，如 $ \underline{图22} $所示

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_262_292_855_454.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fc7fd599265a38d20590a07770c4b305f20ada38bb8226d92f610b1b55598eae1" alt="Image" width="49%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 22. Console-Windows 系统文件的底层文件夹</div> </div>


选择打开对应系统的文件夹，如 Windows 系统举例，选择打开文件夹进入底层文件下，如 $ \underline{图23} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_211_586_910_671.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fa97becdb4d7827f495beda06b14121f18ebaf72ba705dbb4a7fd1c368f77cf77" alt="Image" width="58%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 23. Consol-Windows 文件夹的底层文件</div> </div>


如 Windows 系统举例，右键“以管理员身份运行” $ \underline{\text{图23}} $红色方框内的“LIA_Console_V1.3.12.221025.exe”文件，则会弹出如 $ \underline{\text{图24}} $的软件安装窗口，点击“下一步（Next）”按钮。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_179_874_1011_1248.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F623b8c2f1fb0d6c16725e122b722bb82ca17ad339fbfdffc497783aa18ff8daf" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 24. LIA\_Console 安装界面</div> </div>


在选择安装 LIA_Console 的目录界面中单击 “下一步（Next）” 按钮。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_4/imgs/img_in_image_box_179_159_1012_527.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A56Z%2F-1%2F%2F27de429500ca2fff6ef23d0b73d542e0f4296e74fda278fa076f46d80f877f85" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 25. LIA_Console 安装程序界面</div> </div>


在安装组件界面中单击“下一步（Next）”按钮。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_4/imgs/img_in_image_box_178_671_1011_1044.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A56Z%2F-1%2F%2F1fc5efbb05800ed4f14822899d5608cd9064a6623ac1fe2a76dab300754956e6" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 26. LIA\textsubscript{Console} 组件安装界面</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_0/imgs/img_in_image_box_176_204_1013_631.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Ff62b9d9476c7ca5168681ada06030c1fc21197425c170f6e93adab307ab538df" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 27. LIA\_Console 开始菜单快捷方式界面</div> </div>


##### 在开始菜单快捷方式界面中单击 “安装（Install）” 按钮

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_0/imgs/img_in_image_box_180_808_1011_1184.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fc9e708b4669850b5deb431d3d9397553710c81b555d51d3608e7d49609ed0c5d" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 28. 准备安装界面</div> </div>


则会弹出如图 28 所示的软件窗口，见到以下界面时表示正在安装 LIA_Console 上位机软件，只需要等待几分钟即可。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_1/imgs/img_in_image_box_178_280_1012_654.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F1f8422c89e08eb673ade140df442a3e119e7d2b292adf0555f5be187ecea4f43" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 29. 正在安装 LIA_Console</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_1/imgs/img_in_image_box_176_782_1012_1219.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fba9f2022577675f3bd400c25be29f6423a7370681450e3a8a25ba724ddfd5bfd" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 30. 完成安装</div> </div>


安装上位机程序时会在以下路径中创建一个 Windows 开始菜单项：开始菜单 → LIA_Console → LIA_Console。此链接将打开 $ \underline{\text{图31}} $中显示的 LIA_Console 上位机程序。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_2/imgs/img_in_image_box_180_287_1011_753.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fd61806ca6c4735a148202b41d2b958bc8fac9ce9abd50d0a196cd889370059a9" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 31. LIA\_Console</div> </div>


### 6.3 软件使用说明

打开软件后，进入进入启动界面，如 $ \underline{\text{图31}} $，点击左上角菜单栏的 connect，打开连接配置窗口（如 $ \underline{\text{图32}} $所示）

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_3/imgs/img_in_image_box_220_289_969_744.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F7a0874e7377f942c5810ea105756ca78545d88ac9276d78d187cb3df0e6d1a46" alt="Image" width="62%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 32. 连接配置</div> </div>


此时可以选择 USB/RS232 或 TCP/IP 方式进行连接。

USB/RS232方式连接：选择对应测 COM 口后会显示对应连接机型，点击 Connect 连接。

TCP/IP 方式连接：打开“网络和 Internet 设置”选择“选择更多适配器选项”进入网络连接页面，选择对应以太网右键“属性”双击“Internert 协议版本 4（TCP/IPv4）”选择手动输入填写 IP 地址，如 $ \underline{图33} $所示，输入好后确定保存，返回 Console，点击 Connect 连接。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_3/imgs/img_in_image_box_445_990_787_1415.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F1226a20b342bd4470e5f4fce85b60832c73d6ccbf94bfb0ca62bba7786299559" alt="Image" width="28%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 33. 手动输入 IP 地址</div> </div>


若连接成功，则进入主界面，如 $ \underline{图34} $，

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_4/imgs/img_in_image_box_179_145_1010_612.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F9be9db1f44ea99ffb97db617de02aa73d0deb3bd9b53369e30886451d2afb8ad" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 34. 上位机主界面</div> </div>


#### 6.3.1 主界面功能介绍

主界面中，主要分为3部分：

##### 1) 图形化配置窗口：

以直观的方式配置锁放的参数，包括输入模式，耦合模式，幅度控制，参考模式，以及Sine Out输出的控制开关等；

##### 2) 参数化配置窗口

以参数列表呈现，以更便捷的方式配置锁放的参数。控制功能与图形化配置窗口一致，但同时还有保存文件等上位机软件的配置选项。

##### 3) 数值显示窗

用于显示解调结果等数值，可以选择波形方式显示（Scope），也可以选择数值方式显示（Numeric），在数值方式显示情况下，可以选择多个数值同时显示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_0/imgs/img_in_image_box_214_547_1048_829.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2F845a64050643f8970a395034397fac6c6321770c4dd6b48e7e3a51bc66589c7b" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 35. 波形方式显示</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_0/imgs/img_in_image_box_216_906_1048_1191.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fcde405b4d0ddd3b63e3475b358320b00184aea8d7584022c4bad7d9567067386" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 36. 数值方式显示</div> </div>


#### 6.3.2 输入信号配置

输入信号的软件配置区域如 $ \underline{图37} $.红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_1/imgs/img_in_image_box_180_269_1013_738.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fb0c922019d57b0dc66bd86306802b189679f8193401e0205414d04d43195d7e5" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 37. 输入信号的配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{\text{表3}} $：

<div style="text-align: center;"><div style="text-align: center;">表3. 输入信号配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="3">Input Source\n输入信号源设置</td><td style='text-align: center; word-wrap: break-word;'>Single-Ended Voltage\n单端电压信号(默认设置)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Differential Voltage\n差分电压信号</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Current\n电流信号</td></tr><tr><td rowspan="2">Input Coupling\n输入耦合设置</td><td style='text-align: center; word-wrap: break-word;'>AC\n交流耦合(默认设置)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>DC\n直流耦合</td></tr></table>

<Input Source>输入信号源设置

<A>：单端电压信号输入模式。

<A-B>：差分电压信号输入模式。选择此模式时，将双信号的一端由接口A输入，另一端由接口B输入。

<1>：电流输入模式。

☆当使用电压模式时，输入最大不能超过 5 Vrms。

☆当使用电流模式时，输入最大不能超过5 uA。

<input Coupling>输入耦合设置

<AC>：交流耦合输入。交流耦合输入用于阻隔输入信号中的直流成分，如果信号频率在 10 Hz 以上建议使用<AC>交流耦合。

<DC>：直流耦合输入。直流耦合不阻隔任何输入信号，如果信号频率低于10 Hz时建议使用<DC>直流耦合。但要注意输入信号的偏置量而导致的信号溢出。

#### 6.3.3 参考信号及扫频配置

该项参数的软件配置区域如 $ \underline{\text{图38}} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_3/imgs/img_in_image_box_179_253_1010_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F9fcf1bc3ba373a4976451596aa38669005e66cb86cfcb41f9c38cf14960d6d40" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 38. 参考信号配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{\text{表4}} $：

<div style="text-align: center;"><div style="text-align: center;">表4. 参考信号配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Phase(°)
参考相位设置</td><td style='text-align: center; word-wrap: break-word;'>设置 PSD 算法两路正交参考信号的相移角度，移相精度为 0.01°，输入范围为-180° 至+180°</td></tr><tr><td rowspan="3">Reference Source
参考信号源选择设置</td><td style='text-align: center; word-wrap: break-word;'>External
外部参考信号（默认设置）</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Internal
内部参考信号</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Self
ADC 自参考模式</td></tr><tr><td rowspan="3">External Ref Trigger
外部参考信号触发方式设置</td><td style='text-align: center; word-wrap: break-word;'>TTL Rising Edge
TTL 信号上升沿检测（默认设置）</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>TTL Falling Edge
TTL 信号下降沿检测</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Sine
正弦波信号检测</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Int. Frequency
内部参考频率设置</td><td style='text-align: center; word-wrap: break-word;'>用户手动输入，频率范围为 1 mHz 到 100/500 kHz，频率分辨率最小为 1 mHz，</td></tr></table>

<Reference.Phase>：参考相位设置

通过 Console 输入可设置 PSD 算法两路正交参考信号的相移角度，移相精度为  $ 0.01^{\circ} $，输入范围为  $ -180^{\circ} $ 至  $ +180^{\circ} $。

对于相位，必须有一个基准或者参考才有意义，系统中，我们默认以输入参考信号 REF IN 经过高精度锁相环锁定相位后的信号为相位基准，其余相位值都是相对于此而言的。

<Reference.Source>：参考信号源设置

〈External〉：外部参考信号。OE1300 将与 REF-IN SMB 输入的参考信号进行锁相。

<Internal>：内部参考信号。此设置下参考信号将根据内部信号发生器产生的信号作为参考信号。REF-IN SMB 输入信号将不起作用。此时可以对<Reference.frequency>输入参数值进行设置。

<Self>：自参考模式。在此设置下，OE1300将以输入通道（A、A-B）的信号也作为参考信号来进行锁相，此时REF-IN接口无效。要注意的是，当输入信号幅值太小或者信噪比较低时，锁相环有可能不稳定，此时不建议用<Self>模式。

#### 6.3.4 输入范围配置

该项参数的软件配置区域如图39红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_0/imgs/img_in_image_box_180_255_1008_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fa34255317c44deb19d158cc7084a42aa02724390a9c056d1219a19dc1bc0d830" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 39. 范围配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{\text{表5}} $：

<div style="text-align: center;"><div style="text-align: center;">表5. 范围配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="6">Range\n满偏范围\n设置</td><td style='text-align: center; word-wrap: break-word;'>1 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>100 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>10 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>1 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>100mV/nA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>2 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>200 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>20 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>2 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>200mV/nA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>5 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>500 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>50 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>5 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>500mV/nA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>10 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>1 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>100 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>10 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>1 V/uA(默认设置)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>20 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>2 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>200 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>20 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>2 V/uA</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>50 nV/fA</td><td style='text-align: center; word-wrap: break-word;'>5 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>500 uV/pA</td><td style='text-align: center; word-wrap: break-word;'>50 mV/nA</td><td style='text-align: center; word-wrap: break-word;'>5 V/uA</td></tr></table>

改变<Range>会改变系统的动态范围，同时也会影响到对 CH1、CH2 的输出。系统默认为<1V>。

#### 6.3.5 解调器谐波及任意频率配置

OE1300 除了参考频率的解调器之外，还有 7 个额外的解调器 D1-D7，通过<Demodulator>分别选中每个解调器，可以单独设置每个解调器的功能。

解调器谐波及任意频率的软件配置区域如图 $ \underline{40} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_1/imgs/img_in_image_box_180_314_1009_779.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fbcee67cc0980a62a80dd993f85841e644f6ca4e5323d87475aa68f82aa024a2c" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 40. 谐波及任意频率配置区域图</div> </div>


解调器任意频率配置如 $ \underline{\text{图41}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_1/imgs/img_in_image_box_416_885_773_1151.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fcca3f4a8290e3192910563ae02a2be4ea01e99e68517bc7a0d8f2a806a1cd9af" alt="Image" width="29%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 41. 解调器任意频率配置图</div> </div>


可供用户配置的选项如 $ \underline{表6} $:

<div style="text-align: center;"><div style="text-align: center;">表6. 谐波及任意频率配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Demodulator Channel\n解调器通道</td><td style='text-align: center; word-wrap: break-word;'>8 路谐波解调通道选择</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Demodulator Mode\n解调器模式</td><td style='text-align: center; word-wrap: break-word;'>Harmonic/Arbitrary Frequency\n谐波/任意频率</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Harmonic\n谐波</td><td style='text-align: center; word-wrap: break-word;'>谐波阶数: 1~65535\nDefault: 1、2、3、4、5、6、7、8(对应 8 解调器)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Arbitrary Frequency</td><td style='text-align: center; word-wrap: break-word;'>0.001 mHz—100/500 kHz</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>任意频率</td><td style='text-align: center; word-wrap: break-word;'>1 kHz（默认设置）</td></tr></table>





##### 谐波解调阶数设置

当<Demodulator Mode>设置为<Harmonic>时，可以设置此项。其设置范围是 1~65535 的整数。通过数字键盘输入所需测量的谐波阶数，默认显示 1，表示检测 1 阶谐波(即基波)。<Harmonic >谐波阶数设置的限制是（Harmonic*Freq）≤100/500kHz，其中 Freq 表示参考信号频率。一旦超过限制时，系统会把谐波阶数自动往下调整直到满足条件。同时，当设置为 0 时，系统自动变化为 1。

例如输入信号是频率为 1kHz 的方波时，假定它的峰峰值为 A，设置<Harmonic>值分别为 1、2、3、4、5、6……时，将预期得到 R 值为 0.45A、0、0.15A、0、0.09A、0……，而这个序列正是方波信号傅立叶级数的系数序列的 A 倍。

☆注：多解调器测量的显示需在<Measure Selection>的<Base>选项卡中选择测量通道。

解调器的参考频率设置

当 <<Demodulator Mode> 设置为 <Arbitrary Frequency> 时，可以设置此项。<Arbitrary Frequency> 设置为某个频率时，解调器即以该频率为参考频率来解调信号。

在输入信号包含多个频率信息，而用户需要分别提取出来的时候，这个模式尤为有用。

#### 6.3.6 滤波器配置

在同样的测量准确度下，使用更高的滤波器陡降可以降低时间常数，使得测量响应更快。具体的时间常数和滤波器陡降搭配，必须根据实际情况来选择，一个判定的准则是只要对测量结果的稳定度满意，此时的时间常数和滤波器陡降就不需要设置太大，以免等待时间过长。当然，若想结果更加平稳，可以适当增大时间常数和滤波器陡降。

滤波器参数的软件配置区域如 $ \underline{\text{图42}} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_3/imgs/img_in_image_box_179_379_1011_844.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fc2fb1b0e0020025488bbed08938dddf113b321ca4d474d9d64f92accf317e6ac" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 42. 滤波器配置区域图</div> </div>


可供用户配置的选项如下表7：

<div style="text-align: center;"><div style="text-align: center;">表7. 滤波器配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Time Constant\n滤波器时间常数设置</td><td style='text-align: center; word-wrap: break-word;'>1us~3ks\n100ms(默认设置)</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Filter Slope\n滤波器陡降设置</td><td style='text-align: center; word-wrap: break-word;'>6 dB/oct\n12 dB/oct(默认设置)\n18 dB/oct\n24 dB/oct\n30 dB/oct\n36 dB/oct\n42 dB/oct\n48 dB/oct</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Sync Filter\n同步滤波器设置</td><td style='text-align: center; word-wrap: break-word;'>OFF/ON\n关闭/开启\n默认关闭</td></tr></table>

当信号频率低于  $ 1 \, kHz $ 时可以开启同步滤波器。低通滤波器在输入信号频率较低时无法或需长时间才能得到稳定的结果，此时可借助于此同步滤波器改善效果。

同步滤波器可以有效去除参考频率及其倍频的信号，降低对低通滤波器的要求。☆注：同步滤波器开启时，<Filter db/oct> 必须为<18 dB/oct>以上才能真正起作用！

#### 6.3.7 输出通道配置

该项参数的软件配置区域如 $ \underline{\text{图43}} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_0/imgs/img_in_image_box_179_251_1011_718.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F1b9d71b2021be3305fe00121e7de30fc2f56a79a3be6c7dd204efa465c348e01" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 43. 输出通道的配置区域图</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_0/imgs/img_in_image_box_415_773_813_1073.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F34ba406e11f75a49b25f63bfc21dd3fe671b94413a99e917a9c39c6329e425d3" alt="Image" width="33%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 44. 辅助输出配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{表8} $:

<div style="text-align: center;"><div style="text-align: center;">表8. 输出通道配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Output Channel\n输出通道选择</td><td style='text-align: center; word-wrap: break-word;'>CH1/CH2</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Output Source\n输出源</td><td style='text-align: center; word-wrap: break-word;'>可以控制上位机输出通道 CH1/CH2 控制两路辅助 AUX_DAC 输出直流电压，由用户手动输入，电压范围为 -10 V 至 +10 V，默认值输出为 1V，最小分辨率为 1 mV。；\n也可以输出用户需要的数值，数值类型包括信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、频率、噪声值以及辅助输入值；默认设置 AUX_DAC 输出直流电压</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Offset(%)\n偏置设置</td><td style='text-align: center; word-wrap: break-word;'>可调范围是 -100% -- +100%，其中最小步进为 0.01%，默认 0.00%，只能对 R/X/Y/ \theta 值进行设置，默认值为 0</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Expand\n放大设置</td><td style='text-align: center; word-wrap: break-word;'>可调范围是 0.001~10000，默认值为 1</td></tr></table>





通过 Console 输入，可调范围是 0.001~10000，默认值为 1。但 Expand 的设置使得计算超出了 ±10V 的时候，输出值将会维持在 ±10 V。

输出信号的计算公式如下

1、当选择信号为<R>，<X>，<Y>，<谐波的 X>，<谐波的 Y/值>，<谐波的 R 值>，<X-Noise>，<Y-Noise>时：

 $$  输出 =\left(\frac{Signal( 选择信号 )}{Range}+Offset\right)\times Expand\times10V $$ 

2、当选择信号为<0>，<0D1>，<谐波的0值>时：

 $$  输出 =\frac{Signal( 选择信号 )}{180^{\circ}}\times10V $$ 

3、除了上面两种情况，还有下面选项：

a) AUXOUT：按照用户设定的电压值输出。

b) ADC1~ADC2 ：输出等于 AUX-IN 的输入电压。

c) 频率Freq :

频率每个阶梯分5 V-10 V，例如：

1000Hz = 5 V
1200Hz = 6 V
1600Hz = 8 V
1800Hz = 9 V
1990Hz = 9.95 V
2000Hz = 5 V（下一阶梯）

阶梯定义为：

 $$ 62.5~\mathrm{H z}-125~\mathrm{H z} $$ 

 $$ 125~\mathrm{H z}-250~\mathrm{H z} $$ 

250 Hz – 500 Hz

1 kHz - 2 kHz

500 Hz – 1000 Hz

4 kHz - 8 kHz

8 kHz – 16 kHz

☆注：每一个 CH 通道有一个独立的偏置值和放大值。假如设置了 CH1 的 <Offset> 是 50% 和 <Expand> 是 3，那只有 CH1 通道输出会受影响，CH2 的输出不变。

☆注：<Offset>与<Expand>的设置不会影响动态区域数据框内的数据显示。

#### 6.3.8 正弦信号输出与 TTL 参考信号配置

该项参数的软件配置区域如 $ \underline{\text{图45}} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_2/imgs/img_in_image_box_180_283_1010_750.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2Ffd411ceb2be3a3297239cafea75b18f3e8d1308269df3ce716458613f5257557" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 45. 正弦信号输出配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{\text{表9}} $:

<div style="text-align: center;"><div style="text-align: center;">表9. 正弦信号输出配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td rowspan="2">SineOut /TTL Out Type\n正弦信号与 TTL 参考信号\n输出模式设置</td><td style='text-align: center; word-wrap: break-word;'>OFF/Fixed\n关闭/定值正弦信号，默认关闭</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>TTL OUT 输出接口提供 5 V TTL/CMOS 兼容的方波信号，\n输出阻抗为 200 Ω，其频率与 SINE OUT 相同。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Sine Out Voltage(Vrms)\n定值信号幅值设置</td><td style='text-align: center; word-wrap: break-word;'>当正弦信号输出模式选择定值正弦信号时可操作此项，由用户手动输入，电压值范围为 100uVrms 至 5 Vrms，默认值输出为 1 Vrms，最小分辨率为 1 uVrms。</td></tr></table>

OE1300 可通过前面板的 “Sine Out” SMB 接头输出幅值由 100 uVrms 到 5 Vrms 的正弦波信号.

当使用<External>外部参考时，<Sine Out>提供一个与外部参考锁相的正弦信号；当使用<Internal>内部参考时，将由 OE1300 自身的振荡器产生信号。同时前面板上“TTL OUT”的 SMB 头将输出与<Sine Out>同频的 TTL 信号。

#### 6.3.9 数据保存配置

该项参数的软件配置区域如 $ \underline{\text{图46}} $红框内所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_3/imgs/img_in_image_box_211_255_980_686.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fe5522850d1fd1de6a512664257be652c00c789b69f3df3413b86cf973f670996" alt="Image" width="64%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 46. Sample 数据保存配置区域图</div> </div>


可供用户配置的选项如下 $ \underline{\text{表 10}} $：

<div style="text-align: center;"><div style="text-align: center;">表 10. Sample 数据保存配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Sample Rate(s)</td><td style='text-align: center; word-wrap: break-word;'>用于设定采样的时间间隔，用户手动输入，时间间隔范围为 1 ms 到 100 s，分辨率最小为 1 ms，默认为 100 ms</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Save Path:</td><td style='text-align: center; word-wrap: break-word;'>数据以 csv 表格的形式保存文件，保存在程序目录下。</td></tr></table>

软件有数据记录保存的功能，可根据用户需要选择是否保存一段时间内的 OE1300 采集到的数据。

保存的数据包括测量信号的 R、X、Y、 $ \theta $、频率和噪声的值；测量的七路谐波的 R、X、Y 和  $ \theta $ 的值；以及两路辅助输入的信号值。

选择是否存储数据的具体步骤如下：

1. 数据以 csv 表格的形式保存文件，保存在程序目录下。

2. 点击“Save Data”弹出 $ \underline{\text{图47}} $文件保存窗口，此时可修改文件名称和保存路径，点击

“保存”按钮保存文件。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_4/imgs/img_in_image_box_252_145_1020_571.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2F3e2461bd253c3a679014ea81c9bafb74b5d7ecb014084e79b12a7fc5873a0b6c" alt="Image" width="64%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 47. 数据文件保存窗口</div> </div>


3. 当数据保存时，Sample 界面会显示 “saving”，如 $ \underline{图48} $所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_4/imgs/img_in_image_box_454_666_818_1088.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2F13a2e8e2bbc483bd50137e0c4aaa2633667a83a3e9b91fdfedb3b96a41dab967" alt="Image" width="30%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 48. 数据保存配置区域图</div> </div>


4. 再次按下 “Saving” 按钮，按钮状态由 “Saving” 重新变为 “Save Date”，表示停止保存采集的数据。

5. 在 “Sample Rate(S)” 可以修改当前显示和保存数据的采样率，输入范围为 0.1 s 100s。

#### 6.3.10 谐波波形显示

在软件左窗口选择“谐波波形显示”选项页， $ \underline{图49} $如下：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_0/imgs/img_in_image_box_174_252_1009_721.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A58Z%2F-1%2F%2Fb033f8053fd73a270dfab6cee15195ad34f426384afb2a896c217f6a05859acb" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 49. 谐波波形显示图</div> </div>


此时左窗口中可以分别显示两个谐波的 XY 坐标图。对每一个谐波可设置显示 R、X、Y 和  $ \theta $ 值。

#### 6.3.11 关闭输出检测窗口

在软件内可以选择打开/关闭输出检测窗口，如 $ \underline{\text{图50}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_1/imgs/img_in_image_box_180_253_1009_719.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Fd7edc1521bbe042caa188e2f568b0c8c4f4af9d31154965a298e3608e53bca76" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 50. 关闭输出检测窗口</div> </div>


#### 6.3.12 示波器窗口

在软件在使用 TCP/IP 连接时左上目录能够选择“Function”选择“Oscilloscope”示波器功能，如 $ \underline{图51} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_1/imgs/img_in_image_box_179_906_1010_1376.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2F6f039f8c7924880dd6c311b0a32f71208950f136c420a84a3f7aef75b3df05ab" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 51. 软件示波器功能</div> </div>


此时可以打开左下角"NEW"调出测量选择菜单，调出对应显示窗口；同时右边示波器功能窗口可以调节的示波器基础设置。如 $ \underline{图52} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_2/imgs/img_in_image_box_179_143_1011_605.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fdc925b5d438233cda227fa16e60b44bb0ad274309020250c0600c90f307ee297" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 52. 示波器 Trigger 及测量功能菜单设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 11. 示波器 Trigger 功能配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Sampling Rate\n采样率设置</td><td style='text-align: center; word-wrap: break-word;'>15.26 Hz ~ 4 MHz\n值等于基本采样率除以 2^n，其中 n 是整数。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Length(pts)\n显示长度或持续时间</td><td style='text-align: center; word-wrap: break-word;'>8192/4096</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Edge\n触发设置</td><td style='text-align: center; word-wrap: break-word;'>Rising/Falling\n上升沿触发/下降沿触发</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Level\n触发信号范围设置</td><td style='text-align: center; word-wrap: break-word;'>设置触发电平值（允许负值）</td></tr></table>

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_3/imgs/img_in_chart_box_221_142_1055_603.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F55eb89a0fdefb6b5f8942c11e4b496f1fd4ddc44ec52e5084c65c8b028d5d386" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 53. 示波器 Cursor 功能菜单设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 12. Cursor 功能配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Type 光标设置</td><td style='text-align: center; word-wrap: break-word;'>Off/Voltage/Frequency/Both 关闭/电压轴（Y）/时间轴（X）/同时开启 X 和 Y 光标</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Cursor Line 光标移动设置</td><td style='text-align: center; word-wrap: break-word;'>Single/Both 单个移动光标/同时移动 X 和 Y 光标</td></tr></table>

#### 6.3.13 FFT 窗口

在软件在使用 TCP/IP 连接时左上目录能够选择“Function”选择“FFT”测量功能，如 $ \underline{图54} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_4/imgs/img_in_chart_box_180_281_1010_754.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F5cc8391b1480abbe9ef9e91ea9fb9b334aabb3ed7880eea1d82e2e65ec4f8bd5" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 54. 频谱分析功能</div> </div>


红框中 FFT 功能窗口可以调节的 FFT 基础设置。如 $ \underline{\text{图 55}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_4/imgs/img_in_chart_box_179_935_1010_1408.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fca2a2d2a51317c9623ee6ae847e4a52aac750f8f0315c2cee3ad61390f0b66c7" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 55. 频谱分析 Trigger 功能菜单设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 13. 频谱分析 Trigger 功能配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Sampling Rate\n采样率设置</td><td style='text-align: center; word-wrap: break-word;'>15.26 Hz ~ 4 MHz\n值等于基本采样率除以 2^n，其中 n 是整数。</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Length(pts)\n显示长度或持续时间</td><td style='text-align: center; word-wrap: break-word;'>8192/4096</td></tr></table>

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_0/imgs/img_in_chart_box_180_380_1010_853.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Ff8c9fa7b76408902a8beb1d231e045f53c76e12d60ccf429da21d14acd2e12be" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 56. 频谱分析 Cursor 功能菜单设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 14. 频谱分析 Cursor 功能配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Type 光标设置</td><td style='text-align: center; word-wrap: break-word;'>Off/Voltage/Frequency/Both 关闭/电压轴（Y）/时间轴（X）/同时开启 X 和 Y 光标</td></tr><tr><td style='text-align: center; word-wrap: break-word;'>Cursor Line 光标移动设置</td><td style='text-align: center; word-wrap: break-word;'>Single/Both 单个移动光标/同时移动 X 和 Y 光标</td></tr></table>

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_1/imgs/img_in_chart_box_180_141_1010_613.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Fada94a37d2ac57c3b843d1929d8e14836e23115e5fd0c9757a25ff8c0a9af15b" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 57. 频谱分析 Advanced 功能菜单设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 15. 频谱分析 Advanced 功能配置选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>FFT Window\nFFT 窗函数设置</td><td style='text-align: center; word-wrap: break-word;'>Rectangular/Hanning/Blackman/Hamming\n四种不同的 FFT 窗函数可供选择。每个窗函数在幅值精度和频谱泄漏之间会有不同程度的折衷。请查看相关文献，以便找到最符合您需求的窗函数。</td></tr></table>

#### 6.3.14 PID 控制窗口

在软件控制主界面左上目录能够选择“Function”->“PID”测量功能，如图58所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_2/imgs/img_in_image_box_180_255_1010_648.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fed90718213aef56bfd07a88799eef7e5f3259e9c65491b56c97d31e9b69d9390" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 58. PID 控制功能</div> </div>


红框中 PID 功能窗口可以调节的 PID 基础设置，有两个独立、可分别配置的 PID 可选择。如 $ \underline{\text{图59}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_2/imgs/img_in_image_box_179_769_950_1233.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F4c78b284fafc027f09854f0a6787e67d6d3488bafc6a32ac09dfc9eddcf37772" alt="Image" width="64%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 59. PID 控制菜单设置</div> </div>


使用 PID 功能时，可以先将锁放接入系统，再配置好所有 PID 参数，开启 PID 即可。

如果被测设备（DUT）的传递函数未知，并且只有少量噪声从环境中耦合到系统中，那么通常手动操作是最快的方法。手动配置新的控制环时，建议首先采用较小的 P 值，并将其他参数（I、D、Offset）设置为零。通过启用控制器，能够立即看到 P 的方向是否正确以及反馈是否作用于正确的输出参数，可以通过检查显示在 PID 选项卡中的<Output Select>的值来确认输出参数。积分增益 I 的逐步提高有助于将 PID 误差信号完全归零。启用微分增益 D 可提高反馈回路的速度，但也会导致反馈回路行为不稳定。用户可根据实际环路响

应的速度和效果，调整 P、I、D 的参数。过程中也可调整滤波器带宽，限幅器的限幅范围等参数。

在 PID 参数调整过程中，建议用户使用示波器查看 PID 的输入输出接口，OE1300 的上位机通讯接口速率最高是 1 ksps，而 PID 模块的实际运行速率最高为 4 Msps，上位机存在欠采样的问题，有可能无法捕捉一些高频的震荡现象。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_3/imgs/img_in_image_box_179_335_1014_791.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F2eff0824a2c20100572962cdab75dd89319b8721373fa2cad8b9028a4bca69fe" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 60. PID 采样保存设置</div> </div>


<div style="text-align: center;"><div style="text-align: center;">表 16. PID 采样保存选项表</div> </div>




<table border=1 style='margin: auto; word-wrap: break-word;'><tr><td style='text-align: center; word-wrap: break-word;'>Sample PID 控制器采样保存速率设置</td><td style='text-align: center; word-wrap: break-word;'>可调范围是 0.100~10000.000 s/sample，默认值为 0.100</td></tr></table>

## 7. 操作实例

### 7.1 串口通讯

本实例将演示 OE1300 远程控制串口环境搭建以及调试操作，你需要准备一条 RS232 转 USB 线，步骤如下：

1. 请用 USB 线连接 OE1300 的 USB 插口跟电脑上的任一 USB 插口。

2. 电脑会自动识别到 USB 设备，然后提示安装驱动程序。如果电脑操作系统为 WIN 7/8/8.1/10 系统，系统一般就会自动在网络上搜索驱动程序并自动安装，这个过程需要等待一段时间。如果安装失败（电脑没有连接网络会导致失败）就需要手动去安装 USB 的驱动，安装细节请参考第5章节。

3. 打开 U 盘文件中串口调试工具文件夹，双击 UartAssist.exe 文件，弹出软件界面如 $ \underline{图61} $：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_4/imgs/img_in_image_box_181_606_1008_1393.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fde270e865b0db6f9526c68bc3dfd346358f7be0a7386d0b9e96969b6cacaa940" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 61. 打开的软件界面</div> </div>


该串口调试软件包含了通讯设置，接收区设置，发送区设置，接收区，以及发送区。OE1300 默认波特率为 115200，校验位无，数据位 8 位，停止位 1 位（OE1300 的波特率

及校验位等可通过串口调试助手后续进行修改设置）。

串口号需要选择电脑为 OE1300 USB 接口自动分配的 COM 口，COM 端口编号可通过设备管理器中的端口（COM 和 LPT）选项来查看（计算机右键->属性->设备管理器->端口），如 $ \underline{\text{图62}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_0/imgs/img_in_image_box_271_311_917_959.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2F2de81f38b1a0fb25e8c0f87bf45f60629594a42228355f4478e28493a1a9d96a" alt="Image" width="54%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 62. 端口号的查看</div> </div>


当配置好了端口号、波特率、校验位、数据位、停止位之后，如果连接按钮左边小圆圈为黑色熄灭状态（☑ 打开），需要点击一次改变按钮状态显示为红色点亮状态（☑ 断开），如果按钮为红色点亮状态就表明电脑跟当前串口号设备已连接成功，若多次点击连接不成功，请检查端口号是否选择合适，然后再尝试连接。连接成功如图63所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_1/imgs/img_in_image_box_181_154_1008_939.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F331b9e2872f6b240564fb48466055f5606287a927b19cff4c34728181fdeb460" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 63. 连接成功的状态</div> </div>


## 4. 完成以上操作之后，即可向 OE1300 发送指令来进行通讯：

OE1300 指令要求格式是四个大写字母助记符后加选项参数，例如指令“ISRC O+回车符（0D）”或“ISRC ?+回车符（0D）”，连续多条的指令可以用“;”号分隔开，指令结尾一定要附加上回车符或十六进制数 0D，更多详细指令请查看远程编程章节的介绍。

需要特别注意的是指令结尾一定要附加上回车符或十六进制数 OD 才会有效执行当前指令。发送指令时首先在发送区敲入指令，然后紧接着敲一下回车，最后点击发送按钮，指令就会发送出去。如 $ \underline{图64} $、 $ \underline{图65} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_2/imgs/img_in_image_box_181_154_1003_938.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F32268b2c9a0acf8bc9b9731ec15c711030dc8387edcd2f593763dd87be26db11" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 64. ASCII 码形式发送和接收指令</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_3/imgs/img_in_image_box_182_155_1008_940.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fdfe6c7adfed05a41bd4707fe6208091f5e39248fbd68c0b7b7da782fda4556b2" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 65. 十六进制格式发送和接收指令</div> </div>


同时的，串口调试助手可配置自动添加发送回车符 OXOD。勾选发送区设置的“自动发送附加位”选项，在弹出的附加位设置窗口选择固定位，附加值设置为十六进制值"OD"即可。配置如 $ \underline{66} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_4/imgs/img_in_image_box_182_248_1009_1034.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fcbfb2ca538ac535da7dde2a4c21a11d17ed8dee0157493c707eb37a1430b3826" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 66. 附加位的设置</div> </div>


多个指令的发送需要添加“；”号来分隔开，例如发送指令“ISRC?;ISRC1;ISRC?;FMOD?;FREQ?”效果如 $ \underline{图67} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_0/imgs/img_in_image_box_181_216_1009_1002.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2Fc18ca60dd2ab189a7d522f0ab18adabce5acd5dac22aecbc2bcd86524bfa5367" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 67. 多重指令的执行</div> </div>


连续读取 OE1300 的 X、Y、R、 $ \theta $ 和 Freq 值，可以设置串口调试助手软件的间隔发送，配置如 $ \underline{图68} $、 $ \underline{图69} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_1/imgs/img_in_image_box_182_216_1010_1003.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F218d29745e8bf6a3994ae494b7f99e97eb82a3cfb6eae2b1d42a1f2d51dfbafa" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 68. 连续读取单个 R 值</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_2/imgs/img_in_image_box_199_154_985_878.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F1534cea4fbaac82a08978911fb23e188f73e16d1adf72d7ee6132ffcdca01a18" alt="Image" width="65%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 69. 连续读取 X、Y、R、 $ \theta $ 及 Freq 值</div> </div>


通过串口调试助手远程控制发送指令设置 OE1300 内部参数时，仅会保存在 OE1300 自身，当重新连接 Console 会重新根据 Console 设置进行修改。

OE1300 不只是单一兼容以上这款串口调试助手的远程控制，现在网络上许多的串口调试工具都能很好的兼容，操作步骤也基本类似。

### 7.2 网口通讯

本实例将演示 OE1300 远程控制网口 TCP 模式环境搭建以及调试操作，你需要准备一条网线，步骤如下：

请用网线一端连接 OE1300 的网口，网线另外一端连接电脑主机后面上的网口。

1. 电脑会自动识别到以太网，打开系统设置，点击网络和 Internet 连接选项，点击更改适配器选项。如 $ \underline{图70} $显示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_3/imgs/img_in_image_box_178_380_1013_842.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F57eb7cc1dc852d061c28022722cf676241b7b2b62d80e8c2e34567269983e50d" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 70. 打开的软件界面</div> </div>


2. 双击未识别的网络，点击属性，如 $ \underline{图71} $所示，然后双击 Internet 协议版本 4(TCP/IPv4)，弹出 $ \underline{图72} $，选择使用下面的 IP 地址，手动输入 IP 地址，子网掩码，默认网关。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_4/imgs/img_in_image_box_179_151_986_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Facccf25e74bd461c2dccb6eccc7cc0e84c1e4223cc353ac22521296e434c3d10" alt="Image" width="67%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 71. 打开的软件界面</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_4/imgs/img_in_image_box_334_789_856_1427.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fe6d9915ca5e66a7f4eca0fa778b9c912fc6302bacb0c9a083911ea21da54725b" alt="Image" width="43%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 72. 打开的软件界面</div> </div>


点击确定，在 $ \underline{图73} $中看到已接受的字节在变化，说明已经连接成功。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_0/imgs/img_in_image_box_322_226_862_968.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F587305d7ff02503fdae886c91a5cbb7f4192bde59ac2cdcf8b7d128b635b7e53" alt="Image" width="45%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 73. 打开的软件界面</div> </div>


3. 打开 U 盘文件中串口调试工具文件夹，双击 NetAssist.exe 文件，弹出软件界面如图 74:

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_1/imgs/img_in_image_box_180_156_1009_874.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F3adadb1485eaab7736349cc54098d889163fb53cee08bf16f661753c405eef6d" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 74. 连接成功的状态</div> </div>


该网络调试软件包含了通讯设置，接收区设置，发送区设置，接收区，以及发送区。

协议类型选择 TCP Client, OE1300 默认 IP 地址为 192.168.1.1，远程主机端口固定为 10001（OE1300 的 IP 地址等可通过串口调试助手后续进行修改设置）。

当配置好了协议类型、IP 地址、远程主机端口之后，如果连接按钮左边小圆圈为黑色熄灭状态（☑ 打开），需要点击一次改变按钮状态显示为红色点亮状态

（ $ \uwave{\text{断开}} $），如果按钮为红色点亮状态就表明电脑跟当前串口号设备已连接成功，若多次点击连接不成功，请检查网络是否连接正确，然后再尝试连接。连接成功如 $ \underline{\text{图74}} $所示。

4. 完成以上操作之后，即可向 OE1300 发送指令来进行通讯：

OE1300 指令要求格式是四个大写字母助记符后加选项参数，例如指令“ISRC O+回车符（0D）”或“ISRC ?+回车符（0D）”，连续多条的指令可以用“;”号分隔开，指令结尾一定要附加上回车符或十六进制数 0D，更多详细指令请查看远程编程章节的介绍。

需要特别注意的是指令结尾一定要附加上回车符或十六进制数 OD 才会有效执行当前指令。发送指令时首先在发送区敲入指令，然后紧接着敲一下回车，最后点击发送按钮，指令就会发送出去。如 $ \underline{图75} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_2/imgs/img_in_image_box_180_154_1010_875.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2Fd3f28d8403e3a4d97016a90917b03151ad2411dff1b003c509ce0b0c1ad2eeb6" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 75. ASCII 码形式发送和接收指令</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_3/imgs/img_in_image_box_180_155_1010_876.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fa6d4711c264e13ce9005dd97e3ecca97ce48014c8f62e11536a4831fdc31ebc7" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 76. 十六进制格式发送和接收指令</div> </div>


多个指令的发送需要添加“；”号来分隔开，例如发送指令“*IDN?;ISRC?;FREQ?”效果如 $ \underline{图77} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_4/imgs/img_in_image_box_180_216_1009_937.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fdc8ae09e07c9c3ba31f657e1992a3481bcc1813438d6c60a91e07119a84bb9db" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 77. 多重指令的执行</div> </div>


通过网络调试助手远程控制发送指令设置 OE1300 内部参数时，仅会保存在 OE1300 自身，当重新连接 Console 会重新根据 Console 设置进行修改。

