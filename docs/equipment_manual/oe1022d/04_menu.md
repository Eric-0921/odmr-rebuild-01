## 4. 菜单

OE1022D 主菜单位于前面板控制部分的 MENU。MENU 主菜单共分为：[INPUT/FILTERS]、[REF/PHASE]、[GAIN/TC]、[DISPLAY]、[SAVE/LOAD]、[CHANNEL OUTPUT]、[SAMPLE]、[AUTO SET]、[SYSTEM]和九个子菜单。通过按每个按钮可切换到相应的子菜单界面。在不同的菜单中通过[CH A/B SWITCH]切换 A、B 两个通道的配置菜单，可分别配置两个通道。

### 4.1 [INPUT/FILTERS] 子菜单

在前面板菜单栏中选择[INPUT/FILTERS]进入。如图 13 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//d640ef89-ff10-4b31-b21f-1b1453f12ca3/markdown_1/imgs/img_in_image_box_319_547_871_954.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A19Z%2F-1%2F%2Fce5ba725bce7ec974c4afbeabe4200237e4607e9debbbef41b10878018764faf)

图 13. [INPUT/FILTERS] 子菜单

此子菜单中包括<Source>、<Current Gain>、<Grounding>、<Coupling>和<Line Notches>五种功能设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通过键盘以及旋钮来进行配置：

#### 4.1.1 <Source>：输入模式设置

<A>：单端电压信号输入模式，单端信号由 IN+ 输入。

<A-B>：差分电压信号输入模式。选择此模式时，将差分信号的一端由接口 IN+ 输入，另一端由接口 IN- 输入。

<I>：电流输入模式，电流信号由 IN+输入。

☆当使用电压模式时，输入最大不能超过 1 Vrms；电流模式时，输入最大不能超过 1uA。

#### 4.1.2 <Current Gain>: 电流增益设置

<1 M>：输入为微弱电流信号时，放大倍数为 $ 10^{6} $ V/A。

<100 M>：输入为微弱电流信号时，放大倍数为 $ 10^{8} $ V/A。

☆无论选择哪种增益，电流转换电压后的信号最大不能超过 1 Vrms。

#### 4.1.3 <Grounding>：接地设置

<Float>：Signal IN 输入接口外壳与仪器地通过 10 KΩ电阻隔离。

<Ground> : Signal IN 输入接口外壳与仪器地（仪器地已短接在大地---市电 GND 上）通过 10 Ω电阻短接。

一般认为信号灌入电流的能力不强（不致烧毁仪器接口芯片），或者确保信号地与仪器地处于同一地电平，可设置为<Ground>，让信号地与系统地短接在一起，防止信号地过于浮空带来的信号漂动。当信号地与仪器地绝对电势相差较大，且信号地灌入电流能力很强时，使用<Float>选项，浮空信号地，同时起限流保护作用。

#### 4.1.4 <Coupling>: 耦合设置

<AC>：交流耦合输入。其为一阶截止频率为 0.16Hz 的高通滤波器，可以衰减 0.16Hz 以下以及直流的信号的干扰。如果信号频率在 10 Hz 以上建议使用 <AC> 交流耦合。

<DC>：直流耦合输入。直流耦合不阻隔任何输入信号，如果信号频率低于10 Hz时建议使用<DC>直流耦合。但要注意输入信号的偏置量而导致的信号溢出。

#### 4.1.5 <Line Notches>: 陷波器设置

<None>：关闭所有陷波器。

<Line>：开启 50 Hz 陷波器，抑制工频干扰。

<2×Line>：开启 100 Hz 陷波器，抑制工频二次谐波干扰。

<Both>：同时开启 50 Hz 和 100 Hz 陷波器，抑制工频和工频二次谐波干扰。

在输入信号频率高于 200 Hz 时，可开启 50 Hz 和 100 Hz 陷波器抑制工频干扰；若输入信号频率低于 200 Hz，建议关闭两个陷波器，以避免陷波器的陡度不够锐利可能引起的幅值损失。

### 4.2 [REF/PHASE] 子菜单

在前面板的菜单栏选择[REF/PHASE]子菜单进入，选择不同的参考信号模式会有不同的界面，如图 14、图 15、图 16 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//d640ef89-ff10-4b31-b21f-1b1453f12ca3/markdown_3/imgs/img_in_image_box_347_145_843_512.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A21Z%2F-1%2F%2F04db7771576ec6e173077252bb23f5b299adb7df183e45682cce4e6377174f0d)

图 14. [REF/PHASE] 子菜单-<External>

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//d640ef89-ff10-4b31-b21f-1b1453f12ca3/markdown_3/imgs/img_in_image_box_351_585_840_946.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A21Z%2F-1%2F%2F9bfdcf72a444eae9e388fe65b36d981d726b1c75cf53cca3b8970cba84e28c99)

图 15. [REF/PHASE] 子菜单-<Internal>

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//d640ef89-ff10-4b31-b21f-1b1453f12ca3/markdown_3/imgs/img_in_image_box_347_1019_844_1386.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A21Z%2F-1%2F%2F231999bc53d7113c141ae482b68835c6a5c2f641c6360e06f5c88deb98cdf2fe)

图 16. [REF/PHASE] 子菜单 <Internal Sweep>

此子菜单中包括<Ref.phase>、<Ref.source>、<Ref.slope>、<Ref.frequency>、<Sweep>、<Harmonic>和<Sine Output>七种功能设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通

过键盘以及旋钮来进行配置：

#### 4.2.1 <Ref. phase>: 参考相位设置

通过数字键盘输入可设置 PSD 算法两路正交参考信号的相移角度，移相精度为  $ 0.01^{\circ} $，输入范围为  $ -180^{\circ} $ 至  $ +180^{\circ} $。

对于相位，必须有一个基准或者参考才有意义，系统中，我们默认以输入参考信号 REF IN 经过高精度锁相环锁定相位后的信号为相位基准，其余相位值都是相对于此而言的。

#### 4.2.2 <Ref. source>: 参考信号源设置

<External>：外部参考信号。OE1022D 将与 REF-IN BNC 输入的参考信号进行锁相。此时的界面如图 14 所示，可以对<Ref.slope>进行设置。

<Internal>：内部参考信号。此设置下参考信号将根据内部信号发生器的产生的信号作为参考信号。REF IN 端口输入信号将不起作用。此时的界面如图 15 所示，可以对<Ref.frequency>进行设置。

<Internal Sweep>：内部参考信号扫频。在此设置下，信号发生器根据用户设置的参数进行内部扫频。此时的界面如图 16 所示。可以对<Sweep>进行设置。

<Channel-A>：仅通道 B 适用，调用通道 A 的参考信号设置。当使用通道 B 设置参考信号时，选择该设置可以使通道 B 与通道 A 使用同一参考信号。

#### 4.2.3 <Ref. Slope>: 外部参考信号类型设置

当<Ref.source>选择<External>时可进行此项设置，根据外部参考信号的类型选择对应的信号类型。

<TTL> ： 外部输入信号为方波时选择此项，低电平阈值为 0 到 0.5V，高电平阈值为 3 到 5V。

<Sine>：外部输入信号为正弦波时选择此项，此时为交流耦合输入，输入信号幅值大于 0.2Vp 时有效，最大输入电压幅值为 5Vp。

当输入参考信号为 TTL 逻辑电平时，建议选择 TTL 触发。应当注意，当输入 REF-IN 的参考信号虽然是方波，但电平值不满足 TTL 逻辑高低电平阈值条件时，可能得不到稳定的触发，此时可能得不到预期的测量结果，故此时推荐选用 SINE 触发。此外，对特别低的频率(<1 Hz)时，需使用 TTL 参考。

当输入 REF-IN 的参考信号为正弦信号时，建议选用该 SINE 触发。SINE 触发是在系统内部对 REF-IN 输入进行精密整形后再检测频率、相位信息。

> 校正说明（2026-06-11）：
> 本页内容按原始 PDF 页面图面重新核对后，`V1.5` 源文件在 `4.2.3 <Ref.Slope>` 仅展示 `TTL` 与 `Sine` 两类，不支持本 markdown 旧版本中写入的 `TTL Pos / TTL Neg` 三分法。
> 真机只读观测另外显示：在厂商 LabVIEW 软件配置并锁定 PLL 后，`RSLPD? 2 = 2`、`*PLLD? 2 = 1`。因此“PDF 图面”和“真机可观测枚举”当前并不一致，后续实现必须同时保留这两层事实。

另外，无论是<TTL>触发还是<SINE>触发，系统对其信号占空比（Duty Cycle）没有要求，但推荐使用常规50%占空比为宜。

#### 4.2.4 <Ref. frequency>: 内部参考信号频率设置

当<Ref.source>选择<Internal>时可进行此项设置，频率范围为 1 mHz 到 102 kHz，默认 1.000 kHz。频率设置可以通过键盘输入，频率分辨率最小为 1 mHz。

#### 4.2.5 <Sweep>：内部参考信号扫频设置

当<Ref.source>选择<InternalSweep>时可进行此项设置。选择界面上的<Sweep>功能进入二级子菜单。在二级子菜单中可以对<SweepType>、<Sweep Set>和<Sweep Run>功能进行设定。界面图如图 17 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f18b178b-12c8-4034-9f33-70dfa75fe9a0/markdown_0/imgs/img_in_image_box_339_323_850_704.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A22Z%2F-1%2F%2F143bd1ec6a904be891d41176e9645d5ee3e3be7c9355f70d3d95fafb7bb55ac0)

图 17. <Sweep>二级子菜单

##### 4.2.5.1 <Sweep Type>: 扫频类型设置

<Linear>：线性扫频类型。

<Log> : 对数扫频类型。

当选择<Linear>模式时，步进类型为频率，频率与设定步进频率进行累加。而在<Log>模式时，步进的类型为频率的百分比。

例如<Log>扫频模式下，扫频步进为 10%，开始频率为 1 kHz，截止频率为 2 kHz，扫描频率的过程如下：

1000.000 Hz

1100.000 Hz

1210.000 Hz

1331.000 Hz

1464.100 Hz

1610.510 Hz

1771.561 Hz

1948.717 Hz

2000.000 Hz

##### 4.2.5.2 <Sweep Set>: 扫频参数设置

<Start>：扫频的开始频率。

<Stop>：扫频的截止频率。

<Step>：当选<Linear>时为扫频的步进频率，当选<Log>时为百分比比例。

<Time>： 扫频的步进时间间隔。

通过数字键盘与软键配合对扫频的开始频率、截止频率、步进和时间设置，开始频率与

截止频率的范围为 1 mHz 到 102 kHz，默认<Start> 开始频率为 1.000 kHz，<Stop>截止频率为 5.000 kHz，频率分辨率最小为 1 mHz，<Log>时步进分辨率最小为 0.001%；步进时间分辨率最小为 1 ms，最大 100,000 ms。

##### 4.2.5.3 <Sweep Run>: 扫频运行模式设置

<Stop>：停止扫频。

<Single>：单次扫频。

<Loop>：循环扫频。

当<Single>单次扫频启动时，OE1022D 按照用户设置进行一次内部频率扫描，完成后，状态自动跳转回<Stop>停止扫频。

当<Loop>循环扫频启动时，OE1022D 根据用户设计不停的循环扫频，直到用户手动改变状态为<Stop>停止扫频。

#### 4.2.6 <Harmonic>: 谐波检测设置

OE1022D 可以进行双谐波的同时测量。通过在<Harmonic>二级子菜单中分别设置两个谐波的阶数<Harmonic1>和<Harmonic2>，即可实现双谐波的同时测量。界面图如图 18 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f18b178b-12c8-4034-9f33-70dfa75fe9a0/markdown_1/imgs/img_in_image_box_321_681_868_1088.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A23Z%2F-1%2F%2Ff707d5ae7f39b3bf8234ee5e59d6effc23322e6667e92bfa3eea07b7c9784ab2)

图 18. <Harmonic>二级子菜单

<Harmonic 1>: 谐波 1 设置，最小值 1，最大值 32767。

<Harmonic 2>：谐波 2 设置，最小值 1，最大值 32767。

通过数字键盘输入所需测量的谐波阶数，默认显示 1，表示检测 1 阶谐波(即基波)。<Harmonic>谐波阶数设置的限制是（Harmonic*Freq）<102 kHz，其中 Freq 表示参考信号频率。一旦超过限制时，系统会把谐波阶数自动往下调整直到满足条件。同时，当设置为 0 时，系统自动设置为 1。

例如输入信号是频率为 1kHz 的方波时，假定它的峰峰值为 A，设置<Harmonic>值分别为 1、2、3、4、5、6……时，将预期得到 R 值为 0.45A、0、0.15A、0、0.09A、0……，而这个序列正是方波信号傅立叶级数的系数序列的 A 倍。

☆注：双谐波测量的同时显示需在[DISPLAY]子菜单中的<FULL>选项中进行<LIST>的设置，详细可见[DISPLAY]子菜单。

#### 4.2.7 <Sine Output>: 正弦信号输出设置

OE1022D 可通过后面板的“Sine Out”BNC 接头输出幅值由 0.001 Vrms 到 5 Vrms 的正弦波信号，通过二级子菜单设置可以实现<Fixed>固定幅值输出、<Linear>线性扫幅输出或者<Log>对数扫幅输出、以及<DC>固定直流输出 4 种不同的模式。二级子菜单中可以进行<Sweep Type>、<Voltage>、<Sweep Set>和<Sweep Run>的设置。选择不同的<Sweep Type>时，界面有所不同，如图 19 和图 20 所示。

当使用<External>外部参考时，<Sine Output> 提供一个与外部参考锁相的正弦信号；当使用<Internal>内部参考时，将由 OE1022D 自身的振荡器产生。同时后面板上“TTL OUT”的 BNC 接头将输出与<Sine Output>同步的 TTL 信号。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f18b178b-12c8-4034-9f33-70dfa75fe9a0/markdown_2/imgs/img_in_image_box_316_480_872_888.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A24Z%2F-1%2F%2Ffd2bebe51837a1be3bae6c3f76827784629a4cadaa7c2f20fe1d854ab59ae6bb)

图 19. <Sine Output> 二级子菜单 - <Fixed>

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f18b178b-12c8-4034-9f33-70dfa75fe9a0/markdown_2/imgs/img_in_image_box_316_977_871_1388.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A24Z%2F-1%2F%2Fe7980c420613f6aa02b28dce7a45a14de1e442243bc9c514e92ea567223d3268)

图 20. <Sine Output> 二级子菜单-<Linear&Log>

##### 4.2.7.1 <Sweep Type>: 扫幅类型设置

<Fixed>：固定幅值模式(默认)。

<Linear>：线性扫幅模式。

<Log>：对数扫幅模式。

<DC>：直流输出模式。

当选择<Fixed>固定幅值模式时，<Sine output> 根据用户设置的值输入的幅值进行输出。

当选择<Linear>或<Log>扫幅模式时，通过<Sweep Set>及<Sweep Run>来进行配置。

当选择<DC>直流模式时，<Sine output>将根据用户设置的值输出固定的直流信号。

##### 4.2.7.2 <Voltage>: 正弦信号输出幅值设置

固定幅值模式<Fixed>下的正弦信号幅值设置，通过数字键盘输入，范围为0.001Vrms~5Vrms，最小分辨率为0.001Vrms。

而在直流输出模式<DC>下，<Voltage>的范围变为-10 Vdc~10 Vdc，最小分辨率为 0.001V。

##### 4.2.7.3 <Sweep Set>: 扫幅参数设置

<Start>：扫幅的开始幅值。

<Stop>：扫幅的截止幅值。

<Step>：当<Linear>时为扫描的步进幅值，当<Log>时为百分比。

<Time>：扫幅的步进时间间隔。

通过数字键盘与软键配合对扫幅模式下的开始幅值、截止幅值，步进和时间设置。幅值范围为 0.001 Vrms 到 5 Vrms，默认<Start> 开始幅值为 1.000 Vrms，<Stop>截止幅值为 5.000 Vrms。幅值的分辨率最小为 0.001 Vrms，<Log>时步进分辨率最小为 0.001%，步进时间分辨率最小为 1 mS，最大为 100 s。

##### 4.2.7.4 <Sweep Run>: 扫幅运行模式设置

<Stop>：停止扫幅。

<Single>：单次扫幅。

<Loop> : 循环扫幅。

当<Single>单次扫幅启动时，OE1022D 按照用户设置进行一次幅值扫描，完成后状态自动跳转回<Stop>停止扫幅。

当<Loop>循环扫幅启动时，OE1022D 根据用户设计不停的循环扫幅，直到用户手动改变状态为<Stop>停止扫幅。

### 4.3 [GAIN/TC] 子菜单

在前面板的 MENU 菜单栏选择[GAIN/TC]子菜单进入，如图 21 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f18b178b-12c8-4034-9f33-70dfa75fe9a0/markdown_4/imgs/img_in_image_box_321_265_869_672.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A26Z%2F-1%2F%2Fabc57d3a2c8c1a77abe6f9731705fb2971b6cc7772af9d790e1bcd217f5e49dd)

图 21. [GAIN/TC] 子菜单

此子菜单中包括<Sensitivity>、<Reserve>、<Time Constant>、<Filter dB/oct>和<Synchronous>五种功能设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通过键盘以及旋钮来进行配置：

#### 4.3.1 <Sensitivity>: 满偏灵敏度设置

根据输入信号的大小选择合适的满偏灵敏度，通过重复按软键或旋钮调节数值。满偏灵敏度表如表3所示：

表 3. 满偏灵敏度表

| 1 nV/fA | 200 nV/fA | 50 uV/pA | 10 mV/nA |
|---|---|---|---|
| 2 nV/fA | 500 nV/fA | 100 uV/pA | 20 mV/nA |
| 5 nV/fA | 1 uV/pA | 200 uV/pA | 50 mV/nA |
| 10 nV/fA | 2 uV/pA | 500 uV/pA | 100 mV/nA |
| 20 nV/fA | 5 uV/pA | 1 mV/nA | 200 mV/nA |
| 50 nV/fA | 10 uV/pA | 2 mV/nA | 500 mV/nA |
| 100 nV/fA | 20 uV/pA | 5 mV/nA | 1 V/nA |

改变<Sensitivity>会改变系统的动态范围，同时也会影响到对 CH1 & CH2 的输出。当测量信号为电流信号时，满偏灵敏度为相应的电流单位。系统默认为<100 mV/nA>。

当使用自动设置灵敏度<AUTO GAIN>时，系统会根据输入信号的 R 值自动调整合适的<Sensitivity>。需注意的是，如果时间常数在 1s 以上，则不建议使用<AUTO GAIN>功能。

#### 4.3.2 <Reserve>: 动态储备设置

<Low>：动态储备设置为低。

<High>：动态储备设置为高。

<Normal>：动态储备设置为普通。

对于一般情况下的测量时，使用<Normal>即可。

当使用自动设置动态储备<AUTO RESERVE>时，系统会根据输入信号的 R 值自动调整测量所需要的最小动态范围。

<Sensitivity>和<Reserve>可以组合成不同的动态储备和输入增益，关系如表 4 所示：

表 4. 动态储备和增益关系表

| Sensitivity | Dynamic Reserves(dB) | Gain |  |  |  |  |
|---|---|---|---|---|---|---|
| Low Noise | Normal | High Reserve | Low Noise | Normal | High Reserve |  |
| 1 V | 0 | 0 | 0 | 1.4 | 1.4 | 1.4 |
| 500 mV | 6 | 6 | 6 | 1.4 | 1.4 | 1.4 |
| 200 mV | 4 | 14 | 14 | 4.4 | 1.4 | 1.4 |
| 100 mV | 0 | 10 | 20 | 14 | 4.4 | 1.4 |
| 50 mV | 6 | 16 | 26 | 14 | 4.4 | 1.4 |
| 20 mV | 4 | 24 | 34 | 44 | 4.4 | 1.4 |
| 10 mV | 0 | 20 | 40 | 140 | 14 | 1.4 |
| 5 mV | 6 | 26 | 46 | 140 | 14 | 1.4 |
| 2 mV | 4 | 34 | 54 | 440 | 14 | 1.4 |
| 1 mV | 10 | 40 | 60 | 440 | 14 | 1.4 |
| 500 \mu V | 16 | 46 | 66 | 440 | 14 | 1.4 |
| 200 \mu V | 24 | 54 | 74 | 440 | 14 | 1.4 |
| 100 \mu V | 30 | 60 | 80 | 440 | 14 | 1.4 |
| 50 \mu V | 36 | 66 | 86 | 440 | 14 | 1.4 |
| 20 \mu V | 44 | 74 | 94 | 440 | 14 | 1.4 |
| 10 \mu V | 50 | 80 | 100 | 440 | 14 | 1.4 |
| 5 \mu V | 56 | 86 | 106 | 440 | 14 | 1.4 |
| 2 \mu V | 64 | 94 | 114 | 440 | 14 | 1.4 |
| 1 \mu V | 70 | 100 | 120 | 440 | 14 | 1.4 |
| 500 nV | 76 | 106 | 126 | 440 | 14 | 1.4 |
| 200 nV | 84 | 114 | 134 | 440 | 14 | 1.4 |
| 100 nV | 90 | 120 | 140 | 440 | 14 | 1.4 |
| 50 nV | 96 | 126 | 146 | 440 | 14 | 1.4 |
| 20 nV | 104 | 134 | 154 | 440 | 14 | 1.4 |
| 10 nV | 110 | 140 | 160 | 440 | 14 | 1.4 |
| 5 nV | 116 | 146 | 166 | 440 | 14 | 1.4 |
| 2 nV | 124 | 154 | 174 | 440 | 14 | 1.4 |
| 1 nV | 130 | 160 | 180 | 440 | 14 | 1.4 |

#### 4.3.3 <Time Constant>: 时间常数设置

时间常数设置范围为 10 us 到 3000 s，通过重复按软键或旋钮调节数值，时间常数表如表 5 所示：

表 5. 时间常数表

| 10 us | 3 ms | 1 s | 300 s |
|---|---|---|---|
| 30 us | 10 ms | 3 s | 1000 s |
| 100 us | 30 ms | 10 s | 3000 s |
| 300 us | 100 ms | 30 s |  |
| 1 ms | 300 ms | 100 s |  |

时间常数越长，等效噪声带宽越小，系统测量响应的时间越长，测量的精度也越高。

#### 4.3.4 <Filter dB/oct>: 低通滤波器陡降设置

<6 dB/oct>：低通滤波器陡降 6 dB/oct。

<12 dB/oct>：低通滤波器陡降 12 dB/oct。

<18 dB/oct>：低通滤波器陡降 18 dB/oct。

<24 dB/oct>：低通滤波器陡降 24 dB/oct。

在同样的测量准确度下, 使用更高的滤波器陡降可以降低时间常数, 使得测量响应更快。具体的时间常数和滤波器陡降搭配, 必须根据实际情况来选择, 一个判定的准则是只要对测量结果的稳定度满意, 此时的时间常数和滤波器陡降就不需要设置太大, 以免等待时间过长。当然, 若想结果更加平稳, 可以适当增大时间常数和滤波器陡降。

#### 4.3.5 <Synchronous>：同步滤波器设置

<OFF>：关闭同步滤波器。

<<200 Hz> ：开启同步滤波器。当信号频率低于 200 Hz 时可以开启同步滤波器。低通滤波器在输入信号频率较低时无法稳定或需长时间才能得到稳定的结果，此时可借助于此同步滤波器改善效果。

同步滤波器可以有效去除参考频率及其倍频的信号，降低对低通滤波器的要求。

☆注：同步滤波器开启时，<Filter db/oct> 必须为<18 dB/oct>或<24 dB/oct>才能真正起作用！

### 4.4 [DISPLAY] 子菜单

在前面板的 MENU 菜单栏选择[DISPLAY]子菜单进入，如图 22 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//a02c1bf1-7cbe-4e35-a439-f1facf3efd4b/markdown_2/imgs/img_in_image_box_318_265_869_673.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A32Z%2F-1%2F%2F54bb5db67bfe70450bd51d759352eff6db549ce9084e7d525e266d36d42a9f2c)

图 22. [DISPLAY] 子菜单

[DISPLAY]子菜单主要包括<Display&Scale>、<Equation>和<Disp More> 3 个子菜单，可以通过子菜单旁边对应的软键进行选择、设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通过键盘以及旋钮来进行配置。

#### 4.4.1 <Display&Scale>: 动态区域显示设置

<Display&Scale>选项主要用于设置测量值的显示位置或显示类型。

<Display&Scale>选项其中又包括有<Area>区域显示设置和<Type/Trace>显示模式及类型设置两个子菜单。

注意 A、B 两个通道的数据栏是分开设置的。

##### 4.4.1.1 <Area>：区域显示设置

<Full>：为全栏显示模式，提供了<Bar>、<Chart>和<List>共3种显示模式，在显示界面的整个动态显示区域显示。

1)<Bar>：条形图，条形图的长度可根据实际的 R 值与满刻度的比例来改变显示的长度，对于当前的 R 值百分比情况的显示会十分直观。如图 23 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//a02c1bf1-7cbe-4e35-a439-f1facf3efd4b/markdown_3/imgs/img_in_image_box_319_157_871_565.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A33Z%2F-1%2F%2Fbd7ba927bad1a10a2954f2c5596ea5e8e9e91844cb872a7ee71f6c1ee2afbb4a)

图 23. <Full>-<Polar>显示

2)<Chart>：XY 坐标图，显示测量值得曲线图形，可通过旋钮控制<Cursor>来查看采样点的数值大小，经常与[SAMPLE]子菜单和 CONTROL 控制区域配合做测量数据的采集。如图 24 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//a02c1bf1-7cbe-4e35-a439-f1facf3efd4b/markdown_3/imgs/img_in_image_box_320_747_870_1158.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A33Z%2F-1%2F%2F306d046a3c7010aa190bbca7a082ae3e7cac90087cfa2c604ef9afffcb37856e)

图 24. <Full>-<Chart>显示

3)<List> ：把输入信号的基波、两个谐波值同时显示。如图 25 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//a02c1bf1-7cbe-4e35-a439-f1facf3efd4b/markdown_4/imgs/img_in_image_box_320_158_868_564.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A34Z%2F-1%2F%2Faecc1c2f2bf068e085018f12ccca335ece5d44bf886467650f8ff2e9580b630a)

图 25. <Full>-<List>显示

<Top>：选定上区域的显示，当切换<Type>或<Trace>时候，该区域会随之变化，如图26和图27所示。<Type>提供<Value>、<List>两种选择。当<Type>为<Value>时，<Trace>提供了基波、谐波及Equation等值的选项。当<Type>为<List>时，<Trace>只能选择<Basic>(基波)、Harm1（谐波1）、Harm2（谐波2）和ADC（AUXIN）四个显示选项。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//a02c1bf1-7cbe-4e35-a439-f1facf3efd4b/markdown_4/imgs/img_in_image_box_320_780_869_1188.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A34Z%2F-1%2F%2Ff229b3ae033b92d92aec58622784561cb8ec4ecd5302f85781f9d5790bab41d3)

图 26. <Top>-<Bar>显示

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//dbc476ae-66eb-4c15-a9df-516ad8073e63/markdown_0/imgs/img_in_image_box_322_158_870_564.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A40Z%2F-1%2F%2Fadcff6aa4761194e130ad96b88e7a9bcc919f46334a276da81050494d4e7ddef)

图 27. <Top>-<List>显示

<Bottom>：选定下区域的显示，当切换<Type>或<Trace>时候，该区域会跟<Top>区域相同的规律随之变化。

##### 4.4.1.2 <Type/Trace>: 显示模式及类型设置

使用软键可以在<Type>和<Trace>之间进行切换，选中的会高亮显示。然后再使用旋钮对其进行模式切换。

<Type>：为显示的格式，选项有 <Value>(数值)、<List>(列表)，<Full>显示模式下还有<Bar>(数值百分比)及<Chart>(XY坐标)选项。

<Trace>：为显示的类型，选项有基波、谐波及 Equation 等值。如下：

<X> : 输入信号基波x值。

<Y> : 输入信号基波Y值。

<R> : 输入信号基波R值。

<θ> : 输入信号基波θ值。

<Xh1> : 输入信号第一路谐波x值。

<Yh1> : 输入信号第一路谐波Y值。

<Rh1> : 输入信号第一路谐波R值。

<θh1> : 输入信号第一路谐波θ值。

<Xh2> : 输入信号第二路谐波X值。

<Yh2> : 输入信号第二路谐波Y值。

<Rh2> : 输入信号第二路谐波R值。

<θh2> : 输入信号第二路谐波θ值。

<Noise>：输入信号单位噪声值。

<E1> : 第一个Equation值。

<E2> : 第二个Equation值。

<E3> : 第三个Equation值。

<E4>：第四个Equation值。

#### 4.4.2 <Equation>: 公式设置

OE1022D <Equation>功能使用户可以用仪器测量值进行基本的比例运算，并通过前面板CH1/CH2两个BNC进行输出。固定公式为

 $$  Equation=\frac{A\times B}{C} $$

其中 A、B、C 可以根据<Equation>二级子菜单进行设置,在[DISPLAY]子菜单共有四组<Equation>可用。如图 28 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//dbc476ae-66eb-4c15-a9df-516ad8073e63/markdown_1/imgs/img_in_image_box_320_419_869_824.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A40Z%2F-1%2F%2Fbaf6842282e5fb83287c069730e8f0418c464f6c27415d6b5e569b232e48535a)

图 28. <Equation> 二级子菜单

##### 4.4.2.1 <Equation>: 公式参数设置

选项公式 Equation1~4 = A*B/C 能通过软键选择需要改变的 A、B 或者 C 值，可供数值选项包括基波、谐波、ADC、频率及常数参数等。

例如，当 A 为 R=50 mV，B 为  $ \theta=130^\circ $，C 为 Freq=1 kHz，此时

 $$  Equation=\frac{A\times B}{C}=\frac{0.05\times130}{1000}=0.0065 $$

公式结果显示可以在[DISPLAY]子菜单中的<Trace>选项选择<E1>，代表显示 Equation1 结果。其中，在<Bar>显示模式下，显示范围为 0.000001 到 9999999，不带单位及进度条显示。

公式结果输出可以在[CHANNEL OUTPUT]子菜单中<Source>选择<E1>，代表输出Equation1 结果。输出遵循固定规则，具体参考[CHANNEL OUTPUT]子菜单。

##### 4.4.2.2 <C1>: 常数参数 1 的设置

常数参数<C1>设置，范围：-10～+10，可供<Equation>选择。

##### 4.4.2.3 <C2>: 常数参数 2 的设置

常数参数<C2>设置，范围：-10~+10，可供<Equation>选择。

#### 4.4.3 <Disp More>: 更多显示设置

<Disp More>二级子菜单增加<Range>与<Curve Point>设置，给用户在观看测量值时有更大的自由度。如图 29 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//dbc476ae-66eb-4c15-a9df-516ad8073e63/markdown_2/imgs/img_in_image_box_321_220_870_626.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F623ac72bdcd955a41404d9d053d535c34c34d006ee5f973bfbeff4a868526277)

图 29. <Disp More>二级子菜单

##### 4.4.3.1 <Range>: 显示范围设置

<Range> 用于设置<Bar>、<Chart>和<Polar>图形的最大刻度范围。

<Range>的输入范围为 1 nV 到 1 V，会随着<Sensitivity>的改变而同步改变，也可以通过数字键盘进行输入设置。如 1.00e-01 表示  $ 1.00 \times 10^{-1} $ V，即 100 mV。

在不修改<Sensitivity>的情况下，修改<Range>的值，如修改为 1.00e-03，即 1 mV，则显示的范围变为 -1 mV 到 +1 mV。

##### 4.4.3.2 <Curve Point>: 采样曲线位置设置

<Curve Point>提供了在<Chart>图模式下<Cursor>快速的位置定位功能，并查看该点测量值。其设置范围为 1~16384。

当<Chart>图为复位状态（没有进行数据存储），该值设置不起作用，一直保持为 0。数据采集进行中该选项会隐藏起来(暂不可用)，在暂停后会再显示出来。

☆注意：在使用<Curve Point>前请在 CONTROL 控制区域中的[CURSOR]按键调出<Cursor>。

### 4.5 [SAVE/RECALL] 子菜单

在前面板的 MENU 菜单栏选择[SAVE/RECALL]子菜单进入，如图 30 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//dbc476ae-66eb-4c15-a9df-516ad8073e63/markdown_3/imgs/img_in_image_box_319_266_872_674.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2Ff2489cffc34f7557220eee5beadeb70fe35a0845e38a49e78c7b863dde1140ae)

图 30. [SAVE/RECALL] 子菜单

[SAVE&RECALL] 菜单主要用来保存（Save）和读取（Recall）已经设定的参数和状态。根据用户个人的喜好，可以在项中保存为 <S1>，<S2>，<S3> 和 <S4> 四个存档。如图 30 所示，选择<Save>，<Channel>选择<S1>，再选择<Execute>项中的<YES>，即可将当前设定的参数和状态保存在<S1>存档。需要读取该存档，只需选择<Recall>，<Channel>选择<S1>，再选择<Execute>项中的<YES>即可。

另外在 <Recall> 状态时，<Channel>还有<Default>选项，可以把整机配置还原成默认值。

☆注意：[SAVE&RECALL]菜单跟当前选择是 CHA 或 CHB 通道无关，OE1022D 只有 4 个存档区域。

### 4.6 [CHANNEL OUTPUT] 子菜单

在前面板的 MENU 菜单栏选择[CHANNEL OUTPUT]子菜单进入，如图 31 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//dbc476ae-66eb-4c15-a9df-516ad8073e63/markdown_4/imgs/img_in_image_box_319_268_870_674.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2Fb3438450f39fe43d869b6009bb2b8c6e202e4e5d4bcf95473f9a3a7cef0ba25d)

图 31. [CHANNEL OUTPUT] 子菜单

[CHANNEL OUTPUT]子菜单控制后面板上的两个 BNC 通道 CH1 和 CH2 输出用户需要的 R、X、Y 和  $ \theta $ 值等，以及通过 <Offset&Expand> 设置输出的偏置与放大倍数。同时在 <Speed> 选项栏设置通道输出数据速率。

##### 输出信号的计算公式如下

1、当选择信号为<R>，<X>，<Y>，<Rh1>，<Xh1>，<Yh1>，<Rh2>，<Xh2>，<Yh2>，<Noise>时：

 $$  输出 =\left(\frac{Signal( 选择信号 )}{Sens}+Offset\right)\times Expand\times10V $$

2、当选择信号为<θ>，<θh1>，<θh2>时：

 $$  输出 =\frac{Signal( 选择信号 )}{180^{\circ}}\times10V $$

3、当选择信号为<E1>~<E4>(Equation)时，除了上面两种情况，还有下面选项：

a) AUX_IN1~AUX_IN4：输出等于输入电压

b) 常数系数：直接对应，如 10 对应 10 V，-5 对应 -5 V

c) 频率Freq :

频率每个阶梯分5 V-10 V，例如；

 $$ 1000Hz=5V $$

 $$ 1200Hz=6V $$

 $$ 1600Hz=8V $$

 $$ 1800\mathrm{H z}=9\mathrm{V} $$

 $$ 1990Hz=9.95V $$

 $$ 2000Hz=5V( 下一阶梯 ) $$

阶梯定义为：

62.5 Hz – 125 Hz
125 Hz – 250 Hz
250 Hz – 500 Hz
500 Hz – 1000 Hz
1 kHz – 2 kHz
4 kHz – 8 kHz
8 kHz – 16 kHz

Equation 计算时，按照用户所选，以上诉公式进行换算，再根据 Equation 公式本身计算，得出最后结果，结果范围为  $ \pm 10 \, V $

4、当选择信号为<AUXOUT>时，输出等于设定的电压值，范围为-10V~10V，兼容原来AUXOUT接口功能。

#### 4.6.1 <Output.Source>: 输出源通道设置

以下部分选项为A、B通道均有：

<X>：通道输出X值对应的模拟电平。

<Y>：通道输出Y值对应的模拟电平。

<R>：通道输出R值对应的模拟电平。

<θ>：通道输出θ值对应的模拟电平。

<Xh1>：通道输出Xh1值对应的模拟电平。

<Yh1>：通道输出Yh1值对应的模拟电平。

<Rh1>：通道输出Rh1值对应的模拟电平。

<θh1>：通道输出θh1值对应的模拟电平。

<Xh2>：通道输出Xh2值对应的模拟电平。

<Yh2>：通道输出Yh2值对应的模拟电平。

<Rh2>：通道输出Rh2值对应的模拟电平。

<θh2>：通道输出θh2值对应的模拟电平。

<Noise>：通道输出Noise值对应的模拟电平。

<E1>：通道输出E1值对应的模拟电平。

<E2>：通道输出E2值对应的模拟电平。

<E3>：通道输出E3值对应的模拟电平。

<E4>：通道输出E4值对应的模拟电平。

以下<AUXOUT>选项为[Channel Output]独有部分，与A、B通道无关：

<AUXOUT>：通道输出Voltage中设定的电压值。

#### 4.6.2 <0offset & Expand>: 偏置与放大设置

通过选择<Offset & Expand>选项进入二级子菜单，界面如图 32 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b0d5128-3332-4681-a5a4-ffdf80aa4a00/markdown_1/imgs/img_in_image_box_323_158_868_562.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A40Z%2F-1%2F%2F7e67331f3f7bad3789466c41677b84610c241a29de660df30421e0a7557ecaa5)

图 32. <Offset&Expand> 二级子菜单

##### 4.6.2.1 <Offset & Expand>: 设置对应参数值的偏置及放大

<Offset>偏置设置：

通过数字键盘输入，可调范围是 $ -100\% \sim +100\% $，其中最小步进为0.01%，默认0.00%。

<Expand>放大设置：

通过数字键盘输入，可调范围是 1~256，默认值为 1。但 Expand 的设置使得计算超出了  $ \pm 10V $ 的时候，则输出结果会限制在  $ \pm 10V $。

☆注：每一个<R>、<X>、<Y>、<Rh1>等选项值都对应了一个独立的偏置值和放大值。假如设置了<X>的偏置和放大值，必须同时设置 CH1 或者 CH2 的通道输出源也是<X>，才能产生效果。

☆注意：<Offset>与<Expand>的设置不会影响动态区域数据框内的数据显示。

#### 4.6.3 <Speed>：输出速率设置

<Slow>：通道输出 CH1 / CH2 的数据速率为 10 Hz。

<Fast>：通道输出 CH1 / CH2 的数据速率为 312.5 kHz。

☆注意：CH1 和 CH2 的<Speed>设置是各自独立的。

#### 4.6.4 <Voltage>: AUXOUT 输出幅值设置

输入范围为 $ -10V\sim10V $，分辨率为 $ 0.001V $。

此选项仅在<Source>选择<AUXOUT>时出现，此模式下后面板 CHOUT 接口输出<Voltage>设定的值。

### 4.7 [SAMPLE] 子菜单

[SAMPLE]子菜单用于对测量对象进行采样存储控制。包括了<Step Time>、<Length>、<Select>、<Trigger Mode>和<Sample Mode>的设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通过键盘以及旋钮来进行配置。如图 33 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b0d5128-3332-4681-a5a4-ffdf80aa4a00/markdown_2/imgs/img_in_image_box_319_328_871_737.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F1c8cc71ce8743fda0990cc8a035a0eea0737e9109b6f6aa5261614d3d3a0b008)

图 33. [SAMPLE] 子菜单

[SAMPLE]菜单的设置要配合 CONTROL 区域中的按键来进行采样的开始，暂停，复位等控制。具体见 CONTROL 区域按键介绍。

#### 4.7.1 <Step Time>: 采样间隔设置

采样时间间隔设置，范围：1 ms~100s，每一个时间间隔采集一个数据。

#### 4.7.2 <Length>: 采样长度设置

采样数据长度设置，范围：1~16384。

当数据采样执行中，如果该选项被设置为小于当前采样点的长度的数值，在单次采样模式中，采样会立即停止，而在循环采样模式中，采样会重头开始采样。

#### 4.7.3 <Select>: 采样对象设置

<Select>二级子菜单界面如图 34 所示。

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b0d5128-3332-4681-a5a4-ffdf80aa4a00/markdown_3/imgs/img_in_image_box_322_159_868_562.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2Fd386b7cb96dc479ff0e43e8bea4603f03b3908429a844e74c1eac042fc338598)

图 34. <Select>二级子菜单

##### 4.7.3.1 <Buffer1-4>: 对四个采样缓存区的选择设置

OE1022D 内部开辟了 8 个存储<Buffer>（每个测量通道各 4 个），每个 Buffer 可以存储 16384 个 32 位数。每个<Buffer>可以记录下面测量中任意一个：

<R>,<X>,<Y>,<θ>,<Rh1>,<Xh1>,<Yh1>,<θh1>,<Rh2>,<Xh2>,<Yh2>,<θh2>,<Noise>,<E1>,<E2>,<E3>,<E4>,<A1>,<A2>,<A3>,<A4>,<Freq>。

只有当采样重新开始时，<Buffer>设置才会生效。同时<Buffer>的选择会影响到<Chart>显示模式下<Trace>的选项。在<Chart>显示模式下，<Trace>只可以选择<Buffer>设置里的测量项。

#### 4.7.4 <Trigger Mode>: 触发模式设置

<INT>：内部触发模式

<EXT>：外部触发模式

当内部触发模式时，采样时间间隔由内部产生，通过<Sample Time>中设置。

当外部触发模式时，通过后面板 BNC-TRIG IN 输入采样脉冲，每个采样脉冲采样一次，采样速率不超过 1 kHz。

内部触发模式与外部触发模式都需通过 CONTROL 区域中的按钮来控制开启、停止或复位采样。

#### 4.7.5 <Sample Mode>: 采样模式设置

<Single>：单次采样模式

<Loop>：循环采样模式

单次采样模式下，系统采集完一次数据会自动停止，等待下一次开始。循环采集模式下采样自动循环并覆盖原来的采样。

单次采样模式和循环采样模式都需通过 CONTROL 区域中的按钮来控制开启、停止或复位采样。

### 4.8 [SYSTEM] 子菜单

[SYSTEM]子菜单包括OE1022D中的系统资讯与系统设置。如仪器资讯，屏幕亮度，Remote通讯设置等。如图35所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b0d5128-3332-4681-a5a4-ffdf80aa4a00/markdown_4/imgs/img_in_image_box_317_298_872_706.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2F9509890708dd2d28315c0b7c7d008960938cc4242be237194d94e2f8192a6fab)

图 35. [SYSTEM] 子菜单

#### 4.8.1 <Info>二级子菜单

选择<Info>二级子菜单进入，界面显示研发单位等信息，如图 36 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//3b0d5128-3332-4681-a5a4-ffdf80aa4a00/markdown_4/imgs/img_in_image_box_319_902_871_1311.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2Fb1d5fbc83f6202a7e835088459fedc292586a8169a250d77a12c2da6ae1646d5)

图 36. <INFO>-研发单位

按对应的操作按钮，界面显示 OE1022D 版本，如图 37 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_0/imgs/img_in_image_box_318_186_871_596.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A28Z%2F-1%2F%2F32656ca453a57d228787361606f0046d5919514d890c270fcc15e755b98d6951)

图 37. <INFO>-版本

再次按下按钮，界面显示联系方式，如图 38 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_0/imgs/img_in_image_box_317_762_873_1175.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A28Z%2F-1%2F%2F13c2f03326ab980fe0a3ea2cdec6f67ac27df9ae28639471d9c1bfa65ac76e40)

图 38. <INFO>- 联系方式

#### 4.8.2 <Screen>二级子菜单

选择<Screen>子菜单进入，如图 39、图 40 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_1/imgs/img_in_image_box_320_157_870_564.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A29Z%2F-1%2F%2F2473038b68f90569fa4d7f7db1c68272e16b8ff01caa6e8aa473b1b8f5dd682f)

图 39. <Screen>子菜单-<Style1>

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_1/imgs/img_in_image_box_319_655_871_1064.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A29Z%2F-1%2F%2F1e1a83e99d429c616f9d350716f8c0327e96eab18348683bbf7007a6a5a8d7f1)

图 40. <Screen>子菜单-<Style2>

此子菜单中包括<Window Color>和<Backlight>两种功能设置：

<Window Color>: 界面色调设置

<Style1>：界面主色调为黄色。

<Style2>：界面主色调为绿色。

<Backlight>：背光亮度设置

通过旋钮调节，亮度等级可从<Level1>调节至<Level8>。

#### 4.8.3 <Remote>二级子菜单

选择<Remote>二级子菜单进入，如图 41 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_2/imgs/img_in_image_box_318_262_869_671.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A30Z%2F-1%2F%2F8df726cad42b1fc0be4ea02278e39eb66224cc779a31581ed9ff2858108ae21a)

图 41. <Remote>子菜单

此子菜单中包括<Remote>、<Baud Rate>和<Parity>三种功能设置：

<Remote>: 通信接口设置

<USB> : 通信接口为方口 USB。

<RS232>：通信接口为九针公串口。

<Baud Rate>：波特率设置

通过重复按对应软键或旋钮调节波特率，数值可设置为：<600>、<1200>、<2400>、<4800>、<9600>、<19200>、<38400>、<43000>、<56000>、<57600>、<115200>、<230400>、<380400>、<460800>、<921600>。

<Parity>：奇偶校验设置

<Even>：偶校验

<ODD>：奇校验

<NONE>：无校验

注意：通信接口设置为 USB 时<Baud Rate >和<Parity>不需要设置，此设置只针对串口通信

#### 4.8.4 <Reset ?>: 锁相放大器复位

按下对应软键会软重启系统。重启后不改变当前设置。

### 4.9 [AUTO SET] 子菜单

[AUTO SET]子菜单包括 OE1022D 中的 4 种自动设置，每个菜单可以通过[CH A/B SWITCH]切换通道，通过键盘以及旋钮来进行配置。如图 42 所示：

![Image](https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//63c9978e-93a5-48ab-a1d0-56daa2adf149/markdown_3/imgs/img_in_image_box_319_297_870_705.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A31Z%2F-1%2F%2F498b5823b9c9e1777a8df18d809681a070b76b710d3a8f830f25ae99ad6a61fc)

图 42. AUTO SET 子菜单

#### 4.9.1 <Auto Reserve>: 自动设置动态储备功能

当按下<Auto Reserve>按钮时，系统会根据当前的信号而自动改变<Reserve>的设置。其原则是选取测量当前信号的最小动态储备的设置。

#### 4.9.2 <Auto Gain>: 自动设置灵敏度功能

当按下<Auto Gain>按钮时，系统会根据当前的 R 值自动改变<Sensitivity>设置，其原则就是在不超过最大灵敏度下选择合适的<Sensitivity>。OE1022D 的<Auto Gain>功能需要一定的响应时间（一般小于 5 秒），如果当前的 R 值波动较大时，<Auto Gain>设置有可能不成功。此时请切换至[GAIN/TC]子菜单进行手动设置。

#### 4.9.3 <Auto Phase>: 自动移相功能

当按下<Auto Phase>按钮时，OE1022D会调整参考信号的相移使得测得的输入信号相位为0°。该功能需要一定的响应时间（一般小于5秒），如果当前的θ值波动较大时，<Auto Phase>设置有可能不成功。此时请切换至[REF/PHASE]子菜单手动设置<Phase>值。

#### 4.9.4 <Auto Scale>: 自动刻度功能

当按<Auto Scale>按钮时，OE1022D 会根据输入信号 R 值来自动调整相应的 Scale 刻度值，使显示的数值尽量在较大量程里面。该功能只影响显示，输入输出不会受到影响。同时，也可以通过[DISPLAY]子菜单里的<Range>手动设置。

### 4.10 其他功能键说明

#### 4.10.1 [CH A/B SWITCH] 键设置

[CH A/B SWITCH]为双通道的切换键。根据当前功能栏选择的通道切换显示为另一通道的功能栏，从而对另一通道参数进行配置。每个通道的配置需要单独设置。

#### 4.10.2 [CONTROL] 菜单

控制区域包含四个按钮，分别为[START CONT]、[PAUSE CLEAR]、[CURSOR]、[ACTIVE CHART]。配合[SAMPLE]菜单与<Chart>图，控制当前数据采集的开启、暂停和复位，选择[CURSOR]标尺和选择<Chart>与显示等。

##### 4.10.2.1 [START/CONT]：开启或继续数据采样

[START/CONT]按键功能有两个，一是开始新的采样[START],二是继续被暂停的采样[CONTINUE]。

采样开始使测量数据按照[SAMPLE]菜单中设定的采样率保存到内部 Buffer 上。如果当前是<Chart>图状态，采样的数据会同步显示在<Chart>图上。否则，系统会在后台保存好数据。

单次采样模式下，当采样点数到达了采样长度就会自动停止采样。当系统采样在停止状态，只要再次按开启采样按键，系统又会从头开始采样直到结束点处停止。

当系统在采样期间被暂停时[PAUSE]，可以通过该按键继续末完成的采样。

## 4. 10. 2. 2 [PAUSE/CLEAR]：暂停采样或清空采样数据

[PAUSE/CLEAR]按键功能有两个，一是暂停采样[PAUSE],二是复位采样[CLEAR]。

数据采样进行中可通过该按键来暂停数据采样，如果在<Chart>图状态，<Chart>图会跟着暂停。在暂停状态下再次按一次该按键会清空采样数据并转为停止状态。如果<Chart>图状态，所绘制的采样曲线也会同时清除。

## 4. 10. 2. 3 [CURSOR]: <Chart>图中标尺的显示和隐藏

[CURSOR]按键功能有两个：一是在<Chart>图调出或隐藏<Cursor>，二是在其他状态下重新选中<Cursor>的作用。

当<Chart>图中没有<Cursor>时，该按键能调出并选中<Cursor>，接下来就可以通过转动旋钮来移动<Cursor>读取测量数值。当<Cursor>已调出但是没有被选中（例如<Sensitivity>选项参数设置在高亮状态，旋钮只作用于该参数设定），[CURSOR]按键可以重新选中<Cursor>。在选中<Cursor>的状态下再按一下[CURSOR]按键就可以把<Cursor>隐藏起来。

通过旋钮来调整<Cursor>时，当<Cursor>超出到<Chart>图的两个边沿时，<Chart>图会执行换页操作，也可以通过<Disp More>菜单的<Curve Point>直接定位。数据采样进行中或暂停中都可以使用[CURSOR]功能。

## 4. 10. 2. 4 [ACTIVE CHART]：选择<Chart>图为活动区域

[ACTIVE CHART]按键用于选择<Top>或<Bottom>区域的<Chart>图为当前活动区，只有被选为活动区的<Chart>图，[CURSOR]才会对该区域作用，被选中的活动区<Chart>图左上角会高亮显示。

<Full>区域的<Chart>图一直是活动区，该按键对<Full>区不起任何作用。
