## 8. 操作实例

### 8.1 基本信号测量

本操作实例将简单演示如何使用 OE1022D 测量信号的 R、 $\theta$、X 以及 Y 值。你需要准备两条带 BNC 接头的信号线用于输入待测信号及参考信号。现在我们举例使用函数信号发生器产生一个幅值为 80 mVrms、频率为 1 kHz 的正弦波，并用 OE1022D 进行测量。步骤如下：

1. 断开所有与机箱连接的信号线，接入电源，打开电源开关，此时系统处于默认设置状态。

2. 用一条带 BNC 接头的信号线连接函数信号发生器的输出接口和 OE1022D 前面板通道 A 的 SIGNAL IN 的 IN+接口，用另一条带 BNC 接头的信号线连接函数信号发生器的参考信号接口和 OE1022D 前面板通道 A 的 REF IN 接口，如图 87 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//89c58790-056e-45bc-a3b5-a6c28a1320c7/markdown_3/imgs/img_in_image_box_184_687_1008_910.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A47Z%2F-1%2F%2F631fc1eca1ef6957acd41d85737045559f20200f379e0221acb12413841c4126)

图 87. 信号线连接图

3. 打开函数信号发生器电源，将参数设置为“波形：正弦波”、“幅值：80 mVrms”、“频率：1 kHz”、“输出阻抗：高阻”，待测信号的参数如图 88 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//89c58790-056e-45bc-a3b5-a6c28a1320c7/markdown_3/imgs/img_in_image_box_352_1050_836_1422.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A47Z%2F-1%2F%2Fa281d0185ec25efd8c4f509825f74e06cc41b33b3fda67eb7cdae562c9e582d3)

图 88. 待测信号参数图

4. 开启函数信号发生器的输出，观察主界面中监测栏的<Overload>是否提示溢出：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//89c58790-056e-45bc-a3b5-a6c28a1320c7/markdown_4/imgs/img_in_image_box_268_151_920_630.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A48Z%2F-1%2F%2F28c031d59cce04c202141eda5a6cf90290e1f521c448a758167b26810ef56946)

图 89. 主界面监测栏

若前级输入溢出，则显示 $ \underline{\text{Overload: INPUT NONE}} $；若放大溢出，则显示 $ \underline{\text{Overload: NONEGAIN}} $；若同时溢出，则显示 $ \underline{\text{Overload: INPUTGAIN}} $。

前级溢出时应立即减小数字信号发生器输出幅值，放大溢出应立即调节灵敏度值（OE1022D输入端峰值高于1.7 V或谷值低于-1.7 V时发生前级溢出，且默认灵敏度为100 mV，因此本例中数字信号发生器输出幅值为80 mVrms的正弦波时不会发生溢出，但是测量其他信号时要注意溢出情况）。调节灵敏度值的方法见下。

5. 调节灵敏度值。按下前面板[GAIN/TC]按键进入子菜单。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//89c58790-056e-45bc-a3b5-a6c28a1320c7/markdown_4/imgs/img_in_image_box_411_961_777_1383.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A48Z%2F-1%2F%2Fb63f7269e0a02aaf2d9b69399b19d40cae9a40935caae33220742c500c0a7be0)

图 90. [GAIN/TC] 菜单位置

[GAIN/TC]子菜单界面如下。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_0/imgs/img_in_image_box_267_184_927_566.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A16Z%2F-1%2F%2F2dfdb76c154806e2e50b8af112e44830f60cfbf076472a9145813916635a19de)

图 91. GAIN/TC 菜单界面

按下软键 1 以选中<Sensitivity>功能，选中区域会有高亮显示，通过旋转旋钮调节<Sensitivity>值，使测量信号值尽量满偏而不溢出。此处我们调节为<100 mV>即可。至此，我们即简单测出了从函数信号发生器输送过来的正弦波，如图 91 所示，测量出来的数据为： $ R=80.08\ mV $， $ \theta=-0.15^\circ $。

6. 主界面数据栏显示<R>、<θ>、<X>及<Y>值。按下前面板[DISPLAY]按键进入子菜单。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_0/imgs/img_in_image_box_412_837_778_1257.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A16Z%2F-1%2F%2F057181b49b14f867a1d0858433594c00d41beb1bd8334d8b982aaa8395369c92)

图 92. [DISPLAY]菜单位置

DISPLAY 子菜单界面如下。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_1/imgs/img_in_image_box_267_155_928_535.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A17Z%2F-1%2F%2F84892aa61a551d91aa9e34c0fb874fcefb8d350a6c34ced1c4142dd0d4316e1c)

图 93. [DISPLAY]菜单界面

系统默认设置中，数据栏上方显示<R>，下方显示<θ>值，通过以下介绍的方法可更改显示的数值。

例如将上方显示的<R>值更改为采用 XY 坐标来显示<θ>值的方法：首先按[软键 2]，使其选中<Full>；再按[软键 3]选中<Type>，<Type>区域此时高亮显示，通过调节旋钮可选择<Chart>(XY 坐标)或<Bar>(数字百分比)，我们选择<Chart>；再按[软键 3]选<Trace>，<Trace>区域此时高亮显示，通过调节旋钮可选择显示<R>、<θ>、<X>、<Y>，我们选择<R>。通过以上设置实现效果如下：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_1/imgs/img_in_image_box_267_841_929_1221.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A17Z%2F-1%2F%2F93f612f5cf40eb8254a4d06a6560e39cdecd12833ec2218c109ce137538c0a67)

图 94. XY 坐标显示< $\theta$>值效果

### 8.2 谐波测量

本实例将演示如何测量输入信号的谐波分量值。你需要准备两条带 BNC 接头的信号线用于输入待测信号及参考信号。我们举例使用函数信号发生器产生一个幅值为 160mVpp、频率为 1 kHz 的方波，并用 OE1022D 进行测量其 1 次和 3 次谐波。步骤如下：

1. 断开所有与机箱连接的信号线，接入电源，打开电源开关，此时系统处于默认设置状态。

2. 用一条带 BNC 接头的信号线连接函数信号发生器的输出接口和 OE1022D 前面板 A 通道 SIGNAL IN 的 IN+接口，用另一条带 BNC 接头的信号线连接函数信号发生器的参考信号接口和 OE1022D 前面板 A 通道的 REF IN 接口，如图 95 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_2/imgs/img_in_image_box_184_484_1008_704.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A18Z%2F-1%2F%2F10ccac51f28815d7ca20a3e03dac56e3407bc6bbe4dc27ad7a782dfdd17d0f4b)

图 95. 信号线连接图

3. 打开函数信号发生器电源，将参数设置为“波形：方波”、“幅值：100 mVpp”、“频率：1 kHz”，待测信号的参数如图 96 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_2/imgs/img_in_image_box_339_879_850_1278.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A18Z%2F-1%2F%2F5afa97d1727fd00efecb6df63bcbcb0da46d6271d07803e760fef4bfefb89d81)

图 96. 待测信号参数图

4. 按下前面板[REF PHASE]按键进入子菜单。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_3/imgs/img_in_image_box_411_150_778_570.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A19Z%2F-1%2F%2F5b41bd24364ae4ccc4c197104ad53aa48645c1d5cc775410e382eccdd79e22a3)

图 97. [REF PHASE] 子菜单位置

[REF PHASE]子菜单界面如下。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_3/imgs/img_in_image_box_187_714_1009_1188.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A19Z%2F-1%2F%2F982b8d6d4387f095119c0383f449cf795c1aaa7417ff93d8d295ead2d0f3099e)

图 98. REF PHASE 子菜单

其中<Harmonic>二级子菜单中设置测量的谐波次数，使用键盘输入可选择所需阶次。

同时测量输入方波的 3 次谐波和 5 次谐波的操作方法: 在[REF PHASE]子菜单中, 按下[软键 4]选择谐波功能, 进入<Harmonic>二级子菜单。如图 99 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_4/imgs/img_in_image_box_185_152_1009_627.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A20Z%2F-1%2F%2F212f147fdb05db4514d76c3a0dda23808bdabf46d14672a0056bc3ba76dba6a1)

图 99. Harmonic 二级子菜单

按下[软键1]，在数字键盘上按下数字键[1]，并按下[ENTER]键以确认；再按下[软键2]，在数字键盘上按下数字[3]，此时测量出来的值即为输入信号的3次和5次谐波。选择[DISPLAY]子菜单，在<Display&scale>选项中选择<Full>，<Type>选择<List>，即可查看测得的谐波（谐波1对应<Xh1>、<Yh1>、<Rh1>和<θh1>，谐波2对应<Xh2>、<Yh2>、<Rh2>和<θh2>，）。测量结果如图100所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b914d89-9887-441b-ae3c-9d3ace39bc44/markdown_4/imgs/img_in_image_box_186_874_1009_1346.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A21Z%2F-1%2F%2F917d25a782b83a03b8bf879e17a73f76eca43fefd40d25eebab310b7e98709e2)

图 100. 方波三次谐波测量结果

方波的谐波理论值计算：设方波的峰峰值为 E，角频率为 $\omega$，则经过傅里叶展开之后得到：

$$ \mathrm{f}(t)=\frac{2E}{\pi}\Big(\sin\left(\omega t\right)+\frac{1}{3}\sin\left(3\omega t\right)+\frac{1}{5}\sin\left(5\omega t\right)\cdots+\frac{1}{n}\sin\left(n\omega t\right)\Big) $$

其 n 次谐波即为正弦波：

$$ \mathrm{f}(t)=\frac{2E}{n\pi}\sin\left(n\omega t\right) $$

因此得到 n 次谐波的有效值为：

$$ \mathrm{R}=\frac{\sqrt{2}E}{n\pi} $$

因此我们可以根据此公式来比较测量结果与理论结果是否接近。在本实例中，方波峰峰值 E 为 160mV，则 1 次谐波计算值为：

$$  R=\frac{\sqrt{2}\times160}{1\times\pi}mV\approx72.025mV $$

3 次谐波计算值为：

$$ \mathrm{R}=\frac{\sqrt{2}\times160}{3\times\pi}\mathrm{mV}\approx24.008\mathrm{mV} $$

5 次谐波计算值为：

$$  R=\frac{\sqrt{2}\times160}{5\times\pi}mV\approx14.410mV $$

根据以上算法，即可将测量值与理论计算值进行对比。

### 8.3 某任意光源光谱测量

本实例将演示如何测量一个任意光源的光谱。你需要准备好光谱测量的有关仪器，包括光学斩波器(OE3001)、光栅单色仪(WDG15-Z)及其控制系统、光电探头(电探头采用日本Hamamatsu公司S2386系列的Si光敏二极管)、数据采集平台(NI cDAQ-9172数据采集平台)和PC等，控制单色仪在其光谱测量范围内自动扫描，并用OE1022D对光电流进行测量。步骤如下：

1. 断开所有与机箱连接的信号线，接入电源，打开电源开关，此时系统处于默认设置状态。

2. 设置 OE1022D 的相关参数：

   (1) 在前面板菜单栏中选择[INPUT/FILTERS]键进入子菜单，选择<I>电流输入模式，电流增益设置为<1 M>，其它设置为默认状态；

   (2) 选择[REF/PHASE]键进入子菜单，参考信号源选择<External>，信号源类型选择<TTL>；

   (3) 选择[GAIN/TC]键进入子菜单，在第一次测量时满偏灵敏度设置为最大值<1 uA>，在随后的测量中可以根据实际适当调小；动态储备设置为<Normal>；时间常数设置为<300 ms>；低通滤波器陡降设置为<12 dB/oct>；关闭同步滤波器；

   (4) 选择[OUTPUT/OFFSET]进入子菜单，选择输出通道一<CH1>，信号源选择<R>，输出速度设置为<Fast>；

3. 搭建起光谱测量平台，并用一条带 BNC 接头的信号线连接光电探头的输出接口和 OE1022D 前面板 SIGNAL IN 的 IN+接口；用另一条带 BNC 接头的信号线连接光学斩波器的同步频率信号输出接口 OUTPUT 和 OE1022D 前面板的 REF IN 接口；用一条带 BNC 接头的信号线连接 OE1022D 后面板的 CH1 接口和数据采集平台，示意图如图 101 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//02de6cee-4176-4410-9f86-e9b0da6969eb/markdown_1/imgs/img_in_image_box_184_949_991_1255.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A36Z%2F-1%2F%2F3d4ee9c0fd3009ef6ab89b7de9b3daab25928540281b220078af629fbd29eecf)

图 101. 光谱测量平台示意图

该平台的实际连接图如下图 102 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//02de6cee-4176-4410-9f86-e9b0da6969eb/markdown_2/imgs/img_in_image_box_213_149_974_570.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A38Z%2F-1%2F%2F9136ae07744ecb0e4d610572e6d7ae264679574f676cd03fb538585382f3d786)

图 102. 光谱测量平台实物连接图

4. 开始光谱测量实验，同时利用数据采集平台采集光谱数据，可以得到如下光谱曲线图103（未定标）：

| × 10⁴ | OE1022D |
|------|---------|
| 0.0  | 0       |
| 0.25 | 0.05    |
| 0.5  | 0.05    |
| 0.75 | 0.05    |
| 1.0  | 0.05    |
| 1.25 | 0.05    |
| 1.5  | 0.05    |
| 1.6  | 0.05    |
| 1.7  | 0.05    |
| 1.75 | 0.7     |
| 1.8  | 0.05    |
| 1.85 | 1.35    |
| 1.9  | 0.05    |
| 2.0  | 0.05    |
| 2.1  | 0.05    |
| 2.2  | 0.05    |
| 2.3  | 0.05    |
| 2.4  | 0.05    |
| 2.5  | 0.05    |
| 2.6  | 0.05    |
| 2.7  | 0.05    |
| 2.8  | 0.05    |
| 2.9  | 0.05    |
| 3.0  | 0.05    |
| 3.1  | 0.05    |
| 3.2  | 0.05    |
| 3.3  | 0.05    |
| 3.4  | 0.05    |
| 3.5  | 0.05    |
| 3.6  | 2.2     |
| 3.7  | 0.05    |
| 3.8  | 0.05    |

图 103. OE1022D 测得的光谱曲线图

5. 将 OE1022D 替换为 SR830，在相同的参数设置下进行同样的光谱测量实验，得到的光谱曲线对照图 104 如下：

| × 10⁴ | SR830 | OE1022D |
|------|-------|---------|
| 0.0  | 0.0   | 0.0     |
| 0.25 | 0.05  | 0.05    |
| 0.5  | 0.05  | 0.05    |
| 0.75 | 0.05  | 0.05    |
| 1.0  | 0.05  | 0.05    |
| 1.25 | 0.05  | 0.05    |
| 1.5  | 0.05  | 0.05    |
| 1.6  | 0.05  | 0.05    |
| 1.7  | 0.05  | 0.05    |
| 1.8  | 0.05  | 0.05    |
| 1.9  | 0.05  | 0.05    |
| 2.0  | 0.05  | 0.05    |
| 2.1  | 0.05  | 0.05    |
| 2.2  | 0.05  | 0.05    |
| 2.3  | 0.05  | 0.05    |
| 2.4  | 0.05  | 0.05    |
| 2.5  | 0.05  | 0.05    |
| 2.75 | 0.05  | 0.05    |
| 3.0  | 0.05  | 0.05    |
| 3.25 | 0.05  | 0.05    |
| 3.5  | 0.05  | 0.05    |
| 3.6  | 0.05  | 0.05    |
| 3.7  | 0.05  | 0.05    |
| 3.8  | 0.05  | 0.05    |
| 3.9  | 0.05  | 0.05    |

图 104. 两台锁放测得的光谱曲线对照图

观察波形图可得，两条曲线基本重合。

### 8.4 串口通讯

本实例将演示 OE1022D 远程控制串口环境搭建以及调试操作,你需要准备一条 USB 线,步骤如下:

1. 请用 USB 线连接 OE1022D 的 USB 插口跟电脑上的任一 USB 插口。

2. 电脑会自动识别到 USB 设备，然后提示安装驱动程序。如果电脑操作系统为 WIN 7 系统，系统一般就会自动在网络上搜索驱动程序并自动安装，这个过程需要等待一段时间。如果安装失败（电脑没有连接网络会导致失败）就需要手动去安装 USB 的驱动，安装细节请参考 6.2 章节。

3. 打开光碟中 uart 文件夹，双击 UartAssist.exe 文件，弹出软件界面如图 105:

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//02de6cee-4176-4410-9f86-e9b0da6969eb/markdown_4/imgs/img_in_image_box_180_478_1010_1273.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A39Z%2F-1%2F%2F58dec8ea4f1fb92c1b3d84cbb72d162f6bcc2ead64209be31d509797c77e9c90)

图 105. 打开的软件界面

该串口调试软件包含了通讯设置，接收区设置，发送区设置，接收区，以及发送区。

OE1022D 默认波特率为 921600，校验位无，数据位 8 位，停止位 1 位（OE1022D 的波特率及校验位等可通过前面板键盘上的 RS232 菜单选项来设置）。

串口号需要选择电脑为 OE1022D USB 接口自动分配的 COM 口，COM 端口编号可通过设备管理器中的端口（COM 和 LPT）选项来查看（计算机右键->属性->设备管理器->端口），如图 106 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f7cf829f-8c6f-439b-bb3e-4ba73fef2f3e/markdown_0/imgs/img_in_image_box_196_185_998_940.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2F13f5badbddad4cf3c7dcc5993610dba065f2580468abad6df369bed40fa87a4d)

图 106. 端口号的查看

当配置好了端口号、波特率、校验位、数据位、停止位之后，如果连接按钮左边小圆圈为黑色熄灭状态（☑ 连接），需要点击一次改变按钮状态显示为红色点亮状态( ☑ 断开 )，如果按钮为红色点亮状态就表明电脑跟当前串口号设备已连接成功，若多次点击连接不成功，请检查端口号是否选择合适，然后再尝试连接。连接成功如图 107 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f7cf829f-8c6f-439b-bb3e-4ba73fef2f3e/markdown_1/imgs/img_in_image_box_181_153_1010_944.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2F8a49997bd0e55743f77d7b28ab574db38390c970a4c3a22007f2013b797294eb)

图 107. 连接成功的状态

4. 完成以上操作之后，即可向 OE1022D 发送指令来进行通讯：

OE1022D 指令要求格式为:

$$ 大写字母助记符后 + 通道选择 1/2 + 选项参数 $$

例如指令“SENSD,1,24+回车符（0D）”或“SENSD?,1+回车符（0D）”，连续多条的指令可以用“；”号分隔开，指令结尾一定要附加上回车符或十六进制数 0D，更多详细指令请查看远程编程章节的介绍。

需要特别注意的是指令结尾一定要附加上回车符或十六进制数 0D 才会有效执行当前指令。发送指令时首先在发送区键入指令，然后紧接着敲一下回车，最后点击发送按钮，指令就会发送出去。如图 108、图 109 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f7cf829f-8c6f-439b-bb3e-4ba73fef2f3e/markdown_2/imgs/img_in_image_box_181_152_1011_945.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2F47938f892b103b1fb0eb92533b70eb8d769851dc6b4d31b92e194faba01f7539)

图 108. ASCII 码形式发送和接收指令

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f7cf829f-8c6f-439b-bb3e-4ba73fef2f3e/markdown_3/imgs/img_in_image_box_182_152_1011_945.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A45Z%2F-1%2F%2F7d81397a2b066abf9d7cd8fc5f5f3cd020be45957e83a88e51da187e44c40128)

图 109. 十六进制格式发送和接收指令

同时的，串口调试助手可配置自动添加发送回车符 0X0D。勾选发送区设置的“自动发送附加位”选项，在弹出的附加位设置窗口选择固定位，附加值设置为十六进制值“0D”即可。配置如图 110 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f7cf829f-8c6f-439b-bb3e-4ba73fef2f3e/markdown_4/imgs/img_in_image_box_182_152_1010_947.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A45Z%2F-1%2F%2Ff42f021ba2f6b237f7a4c04e8fd3541d793851c171042c29e191912750b804c7)

图 110. 附加位的设置

多个指令的发送需要添加“;”号来分隔开，例如发送指令“SENSD 1,25;FMODD 1,1;FREQD 1,2048;SENSD 1,1;FMODD 1,1;FREQD 1,1”效果如图 111 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//0e73a573-edbe-4b56-ba50-6d4dc562831f/markdown_0/imgs/img_in_image_box_180_153_1011_944.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2Fab627e7901337d3a443c110999c8a4478f675a6ca73df853bb9b8b915ac4f74f)

图 111. 多重指令的执行

连续读取 OE1022D 的 X、Y、R、 $\theta$ 和 Freq 值，可以设置串口调试助手软件的间隔发送，配置如图 112、图 113 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//0e73a573-edbe-4b56-ba50-6d4dc562831f/markdown_1/imgs/img_in_image_box_180_152_1009_945.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2F018c007a7d2001204bed79aab451fd4fd57a9676ce1f49c466a4101eeca8ce06)

图 112. 连续读取单个 R 值

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//0e73a573-edbe-4b56-ba50-6d4dc562831f/markdown_2/imgs/img_in_image_box_192_154_998_1002.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2F12b51a956bd823ab23a4fc21b2b9c470a2c7d60b0ca85c5609350e311c3cd0c9)

图 113. 连续读取 X、Y、R、 $\theta$ 及 Freq 值

通过串口调试助手远程控制发送指令设置 OE1022D 内部参数时，会同时更新 LCD 显示屏上状态的显示。例如 OE1022D 当前状态栏的<sens>值为<100 mV>，对应指令标识号为 25。当发送了指令“SENS 24”之后，OE1022D 状态栏<sens>值会改变为指令标识码 24 所对应的值为<50 mV>。

OE1022D 不只是单一兼容以上这款串口调试助手（精装版 V3.8.3）的远程控制，现在网络上许多的串口调试工具都能很好的兼容，操作步骤也基本类似。
