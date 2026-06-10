## 3. 界面介绍

### 3.1 前面板

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//8fc1066a-f44c-4f4c-8dce-837813f334e6/markdown_1/imgs/img_in_image_box_215_393_967_613.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A53Z%2F-1%2F%2F0a5b9e6ffc40ad68fca2bcf3a8331eed92c142a67e803103a619551ceac38773" alt="Image" width="63%" />

图 8. OE1022D 前面板

#### 3.1.1 显示屏

OE1022D 使用 5.6 英寸 TFT 显示屏作为用户的数据显示与交互控制。显示屏分辨率为 640*480，有 8 级的背景亮度可供用户选择，可以在[SYSTEM]子菜单设置。

屏幕的左边大幅区域用于显示输入信号的测量结果，支持单区域或双区域显示。此外，每个区域显示支持数字，XY坐标显示。

屏幕的右边区域用于测量控制条件的选择与修改。

#### 3.1.2 软键

显示屏的右边有 5 个软键。软键根据不同的当前目录有着不同的功能。总体来说，软键有着两个主要功能，一是在不同的设置选项进行选择，二是高亮某些参数，然后使用旋钮或键盘进行输入。不管哪种功能，软键只对屏幕右方与软键相邻的参数起作用。

#### 3.1.3 旋钮

旋钮可以调整那些被软键高亮的参数。大部分的参数均可以使用旋钮进行调整。沿顺时针方向旋转是增大参数，沿逆时针方向旋转是减小参数。

#### 3.1.4 键盘

键盘由 3 组键组成。ENTRY 区域主要对被软键高亮的参数进行数据形式的输入。MENU 区域改变屏幕右方的参数列表，并提供 10 个不同的功能菜单。CONTROL 区域提供 XY 显示测量时的辅助功能，如[CURSOR]等。

#### 3.1.5 BNC 连接器

##### REF IN

参考信号输入可以使用正弦波或 TTL 驱动。参考通道输入阻抗为 1 MΩ，正弦参考输入时为交流耦合。对于低频应用的情况(<1 Hz)，推荐使用 TTL 驱动的参考信号。

##### SIGNAL IN

输入信号分为单端输入模式 A、差分输入模式 A-B 以及电流输入模式 I。单端输入时 IN+ 作为信号输入端。差分输入时，可以测量 IN+ 以及 IN- 之间差分信号。单端电压信号的 IN+ 以及 IN- 两个输入的输入阻抗均 10 MΩ/25 pF；电流信号输入时，通过 IN+ 输入，内部连接 1 kΩ 电阻到地。

### 3.2 后面板

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//8fc1066a-f44c-4f4c-8dce-837813f334e6/markdown_2/imgs/img_in_image_box_188_657_1017_906.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A54Z%2F-1%2F%2F66614b202f749c5f232838c83cb67a33dd65698c2e4031e3c01d3af44f38123c" alt="Image" width="69%" />

图 9. 后面板

OE1022D 后面板如图 9 所示，包括散热风扇、电源接口、电源开关、USB 接口、RS232 接口以及扩展功能接口。其中，扩展功能接口包括 SINE OUT、TTL OUT、AUX IN、TRIG IN、CH1&CH2 OUTPUT 和 MONITOR。

#### 3.2.1 电源接口

电源接口用于整台仪器供电输入，接受 220 V、50 Hz 交流市电，内置保险丝，同时兼备滤除高频噪声干扰的滤波器功能。

#### 3.2.2 USB

USB 接口允许 OE1022D 锁相放大器与 PC 机进行通信。可以通过 PC 机对 OE1022D 进行控制和读取数据。

#### 3.2.3 RS232 接口

RS232 公接口允许 OE1022D 与其他上位机之间进行通讯，最高波特率为 921600bps。接口定义为 DB9 公接口标准定义。

#### 3.2.4 SINE OUT

两路信号发生器提供最大 5 Vrms 的幅值可编程输出，输出阻抗为 50 Ω。当外部参考信号使用时，信号发生器通过锁相环与输入信号进行锁相。

另外 Sineout 还能输出设定的直流信号。

#### 3.2.5 TTL OUT

TTL 输出，与 Sine Output 同步，输出阻抗为  $ 200\ \Omega $ 。当正弦波输出幅值很小时，TTL OUT 可用于其他仪器的同步锁定。

#### 3.2.6 AUX IN

四路辅助 AUX-ADC 输入，输入范围±10 V，最小分辨率为 1mV。

#### 3.2.7 TRIG IN

TRIG IN 接收一个 TTL 信号输入，用于触发测量数据采样和开始数据获取。TRIG IN 上升沿有效，最大采样速率为 1kHz。

#### 3.2.8 CH1&CH2 OUTPUT

CH1&CH2 OUTPUT 输出范围为 -10 V 到 +10 V。根据当前测量信号的测量结果与设置的测量范围的比值，正比到输出。

另外 CHOUT 还能输出设定的直流信号。

#### 3.2.9 MONITOR

MONITOR 提供了一个模拟放大、滤波之后得到的输出信号，即输入到 ADC 之前的信号。MONITOR 输出的幅值固定为 ADC 前信号的 5 倍。由于模拟放大也会放大噪声，MONITOR 输出并不适合观察到原信号幅值十分小的信号。

### 3.3 主界面

OE1022D 主界面中可以分为三个部分。

#### 3.3.1 数据栏

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//8fc1066a-f44c-4f4c-8dce-837813f334e6/markdown_4/imgs/img_in_image_box_369_342_820_677.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A56Z%2F-1%2F%2F0613427dd8dcd80408277b0afbaf8e5f016f3163c3732b0cdb46a12e07a4262d" alt="Image" width="37%" />

图 10. 主界面-数据栏

如图 10 所示，可在[DISPLAY]子菜单中选择显示<X>、<Y>、<R>、< $ \theta $>等值，显示方式可选择数字图、条形图、XY 坐标图。设置方式详见[DISPLAY]子菜单。

#### 3.3.2 监测栏

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//8fc1066a-f44c-4f4c-8dce-837813f334e6/markdown_4/imgs/img_in_image_box_375_896_825_1230.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A56Z%2F-1%2F%2F4defc2c3a3f72c9aad82bad8f964bf7ba49c015122d1eaaf08c30bcf75e691b3" alt="Image" width="37%" />

图 11. 主界面-监测栏

监测栏一共显示伍项内容，包括：

<Sens>：当前选择灵敏度大小，可以检测当前输入信号是否过大。当输入信号超出灵敏度的时候，显示OVLD提示。此时应该调整灵敏度到适合的量程，如果输入信号过大，需尽快断开信号输入，防止过压损坏仪器。

<Overload>：溢出提示。能够提示前级输入和放大是否溢出。若未发生溢出，则显示：Overload: NONE NONE；若前级输入溢出，则显示 Overload: INPUT NONE；若放大溢出，则显示 Overload: NONE GAIN；若同时溢出，则显示 Overload: INPUT GAIN。此时需尽快把输入信号减少以防止对机器造成过压损坏仪器。

<Freq>：参考信号频率。显示输入参考信号的频率或内部参考的频率。

<Ref.Source>：参考信号。显示选用的参考信号是内部参考还是外部参考。

<PLL>：锁相环的锁相提示，提示相位是否锁定。当锁相环已经锁定，则显示 $ \underline{\text{PLL: LOCKED}} $；当没有参考信号，或者锁相环未锁定，则显示 $ \underline{\text{PLL: UNLOCKED}} $；当使用内部参考时，一直显示 $ \underline{\text{PLL: NONE}} $。

#### 3.3.3 功能栏

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//d640ef89-ff10-4b31-b21f-1b1453f12ca3/markdown_0/imgs/img_in_image_box_369_421_820_754.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A18Z%2F-1%2F%2F2dcae2c25e35ee2badb616ceeeabfdbca03d7e725f4454d010aac5d3f970222e" alt="Image" width="37%" />

图 12. 主界面-功能栏

如图 12 所示，功能设置框内有多种功能选择，与前面板的 5 个软键一一对应，在不同的子菜单中有不同作用，是控制系统的主要方式。

