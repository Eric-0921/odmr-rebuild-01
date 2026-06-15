## 6.3 软件使用说明

打开软件后，进入启动界面，如 $\underline{图\ 31}$，点击左上角菜单栏的 `connect`，打开连接配置窗口（如 $\underline{图\ 32}$ 所示）。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_3/imgs/img_in_image_box_220_289_969_744.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F7a0874e7377f942c5810ea105756ca78545d88ac9276d78d187cb3df0e6d1a46" alt="Image" width="62%" /></div>

<div style="text-align: center;">图 32. 连接配置</div>

此时可以选择 `USB/RS232` 或 `TCP/IP` 方式进行连接。

`USB/RS232` 方式连接：选择对应测 COM 口后会显示对应连接机型，点击 `Connect` 连接。

`TCP/IP` 方式连接：打开“网络和 Internet 设置”，选择“选择更多适配器选项”进入网络连接页面，选择对应以太网右键“属性”，双击“Internert 协议版本 4（TCP/IPv4）”，选择手动输入填写 IP 地址，如 $\underline{图\ 33}$ 所示。输入好后确定保存，返回 Console，点击 `Connect` 连接。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_3/imgs/img_in_image_box_445_990_787_1415.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F1226a20b342bd4470e5f4fce85b60832c73d6ccbf94bfb0ca62bba7786299559" alt="Image" width="28%" /></div>

<div style="text-align: center;">图 33. 手动输入 IP 地址</div>

若连接成功，则进入主界面，如 $\underline{图\ 34}$。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_4/imgs/img_in_image_box_179_145_1010_612.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F9be9db1f44ea99ffb97db617de02aa73d0deb3bd9b53369e30886451d2afb8ad" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 34. 上位机主界面</div>

### 6.3.1 主界面功能介绍

主界面中，主要分为 3 部分：

**1) 图形化配置窗口**：以直观的方式配置锁放的参数，包括输入模式、耦合模式、幅度控制、参考模式，以及 `Sine Out` 输出的控制开关等。

**2) 参数化配置窗口**：以参数列表呈现，以更便捷的方式配置锁放的参数。控制功能与图形化配置窗口一致，但同时还有保存文件等上位机软件的配置选项。

**3) 数值显示窗**：用于显示解调结果等数值，可以选择波形方式显示（`Scope`），也可以选择数值方式显示（`Numeric`）。在数值方式显示情况下，可以选择多个数值同时显示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_0/imgs/img_in_image_box_214_547_1048_829.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2F845a64050643f8970a395034397fac6c6321770c4dd6b48e7e3a51bc66589c7b" alt="Image" width="70%" /></div>

<div style="text-align: center;">图 35. 波形方式显示</div>

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_0/imgs/img_in_image_box_216_906_1048_1191.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fcde405b4d0ddd3b63e3475b358320b00184aea8d7584022c4bad7d9567067386" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 36. 数值方式显示</div>

### 6.3.2 输入信号配置

输入信号的软件配置区域如 $\underline{图\ 37}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_1/imgs/img_in_image_box_180_269_1013_738.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fb0c922019d57b0dc66bd86306802b189679f8193401e0205414d04d43195d7e5" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 37. 输入信号的配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 3}$：

<div style="text-align: center;">表 3. 输入信号配置选项表</div>

| 参数 | 选项 |
|------|------|
| `Input Source`<br>输入信号源设置 | `Single-Ended Voltage`<br>单端电压信号（默认设置） |
| | `Differential Voltage`<br>差分电压信号 |
| | `Current`<br>电流信号 |
| `Input Coupling`<br>输入耦合设置 | `AC`<br>交流耦合（默认设置） |
| | `DC`<br>直流耦合 |

`<Input Source>` 输入信号源设置：

- `<A>`：单端电压信号输入模式。
- `<A-B>`：差分电压信号输入模式。选择此模式时，将双信号的一端由接口 A 输入，另一端由接口 B 输入。
- `<I>`：电流输入模式。

☆ 当使用电压模式时，输入最大不能超过 $5\ \mathrm{V}_{\mathrm{rms}}$。

☆ 当使用电流模式时，输入最大不能超过 $5\ \mathrm{\mu A}$。

`<Input Coupling>` 输入耦合设置：

- `<AC>`：交流耦合输入。交流耦合输入用于阻隔输入信号中的直流成分，如果信号频率在 $10\ \mathrm{Hz}$ 以上建议使用 `<AC>` 交流耦合。
- `<DC>`：直流耦合输入。直流耦合不阻隔任何输入信号，如果信号频率低于 $10\ \mathrm{Hz}$ 时建议使用 `<DC>` 直流耦合。但要注意输入信号的偏置量而导致的信号溢出。

### 6.3.3 参考信号及扫频配置

该项参数的软件配置区域如 $\underline{图\ 38}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//f0b330c1-eada-40b5-9476-79289556c483/markdown_3/imgs/img_in_image_box_179_253_1010_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F9fcf1bc3ba373a4976451596aa38669005e66cb86cfcb41f9c38cf14960d6d40" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 38. 参考信号配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 4}$：

<div style="text-align: center;">表 4. 参考信号配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Phase(°)`<br>参考相位设置 | 设置 PSD 算法两路正交参考信号的相移角度，移相精度为 $0.01\ \mathrm{^{\circ}}$，输入范围为 $-180\ \mathrm{^{\circ}}$ 至 $+180\ \mathrm{^{\circ}}$。 |
| `Reference Source`<br>参考信号源选择设置 | `External`<br>外部参考信号（默认设置） |
| | `Internal`<br>内部参考信号 |
| | `Self`<br>ADC 自参考模式 |
| `External Ref Trigger`<br>外部参考信号触发方式设置 | `TTL Rising Edge`<br>TTL 信号上升沿检测（默认设置） |
| | `TTL Falling Edge`<br>TTL 信号下降沿检测 |
| | `Sine`<br>正弦波信号检测 |
| `Int. Frequency`<br>内部参考频率设置 | 用户手动输入，频率范围为 $1\ \mathrm{mHz}$ 到 $100/500\ \mathrm{kHz}$，频率分辨率最小为 $1\ \mathrm{mHz}$。 |

`<Reference.Phase>`：参考相位设置。

通过 Console 输入可设置 PSD 算法两路正交参考信号的相移角度，移相精度为 $0.01\ \mathrm{^{\circ}}$，输入范围为 $-180\ \mathrm{^{\circ}}$ 至 $+180\ \mathrm{^{\circ}}$。

对于相位，必须有一个基准或者参考才有意义。系统中，我们默认以输入参考信号 `REF IN` 经过高精度锁相环锁定相位后的信号为相位基准，其余相位值都是相对于此而言的。

`<Reference.Source>`：参考信号源设置。

- `<External>`：外部参考信号。OE1300 将与 `REF-IN` SMB 输入的参考信号进行锁相。
- `<Internal>`：内部参考信号。此设置下参考信号将根据内部信号发生器产生的信号作为参考信号，`REF-IN` SMB 输入信号将不起作用。此时可以对 `<Reference.frequency>` 输入参数值进行设置。
- `<Self>`：自参考模式。在此设置下，OE1300 将以输入通道（A、A-B）的信号也作为参考信号来进行锁相，此时 `REF-IN` 接口无效。要注意的是，当输入信号幅值太小或者信噪比较低时，锁相环有可能不稳定，此时不建议用 `<Self>` 模式。

### 6.3.4 输入范围配置

该项参数的软件配置区域如 $\underline{图\ 39}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_0/imgs/img_in_image_box_180_255_1008_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fa34255317c44deb19d158cc7084a42aa02724390a9c056d1219a19dc1bc0d830" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 39. 范围配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 5}$：

<div style="text-align: center;">表 5. 范围配置选项表</div>

| `Range`<br>满偏范围设置 | | | | |
|------|------|------|------|------|
| $1\ \mathrm{nV/fA}$ | $100\ \mathrm{nV/fA}$ | $10\ \mathrm{\mu V/pA}$ | $1\ \mathrm{mV/nA}$ | $100\ \mathrm{mV/nA}$ |
| $2\ \mathrm{nV/fA}$ | $200\ \mathrm{nV/fA}$ | $20\ \mathrm{\mu V/pA}$ | $2\ \mathrm{mV/nA}$ | $200\ \mathrm{mV/nA}$ |
| $5\ \mathrm{nV/fA}$ | $500\ \mathrm{nV/fA}$ | $50\ \mathrm{\mu V/pA}$ | $5\ \mathrm{mV/nA}$ | $500\ \mathrm{mV/nA}$ |
| $10\ \mathrm{nV/fA}$ | $1\ \mathrm{\mu V/pA}$ | $100\ \mathrm{\mu V/pA}$ | $10\ \mathrm{mV/nA}$ | $1\ \mathrm{V/\mu A}$（默认设置） |
| $20\ \mathrm{nV/fA}$ | $2\ \mathrm{\mu V/pA}$ | $200\ \mathrm{\mu V/pA}$ | $20\ \mathrm{mV/nA}$ | $2\ \mathrm{V/\mu A}$ |
| $50\ \mathrm{nV/fA}$ | $5\ \mathrm{\mu V/pA}$ | $500\ \mathrm{\mu V/pA}$ | $50\ \mathrm{mV/nA}$ | $5\ \mathrm{V/\mu A}$ |

改变 `<Range>` 会改变系统的动态范围，同时也会影响到对 CH1、CH2 的输出。系统默认为 `<1 V/μA>`。

### 6.3.5 解调器谐波及任意频率配置

OE1300 除了参考频率的解调器之外，还有 7 个额外的解调器 D1-D7。通过 `<Demodulator>` 分别选中每个解调器，可以单独设置每个解调器的功能。

解调器谐波及任意频率的软件配置区域如 $\underline{图\ 40}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_1/imgs/img_in_image_box_180_314_1009_779.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fbcee67cc0980a62a80dd993f85841e644f6ca4e5323d87475aa68f82aa024a2c" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 40. 谐波及任意频率配置区域图</div>

解调器任意频率配置如 $\underline{图\ 41}$ 所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_1/imgs/img_in_image_box_416_885_773_1151.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fcca3f4a8290e3192910563ae02a2be4ea01e99e68517bc7a0d8f2a806a1cd9af" alt="Image" width="29%" /></div>

<div style="text-align: center;">图 41. 解调器任意频率配置图</div>

可供用户配置的选项如 $\underline{表\ 6}$：

<div style="text-align: center;">表 6. 谐波及任意频率配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Demodulator Channel`<br>解调器通道 | 8 路谐波解调通道选择 |
| `Demodulator Mode`<br>解调器模式 | `Harmonic` / `Arbitrary Frequency`<br>谐波 / 任意频率 |
| `Harmonic`<br>谐波 | 谐波阶数：$1 \sim 65535$<br>默认值：1、2、3、4、5、6、7、8（对应 8 路解调器） |
| `Arbitrary Frequency`<br>任意频率 | $0.001\ \mathrm{mHz} \sim 100/500\ \mathrm{kHz}$，默认 $1\ \mathrm{kHz}$ |

#### 谐波解调阶数设置

当 `<Demodulator Mode>` 设置为 `<Harmonic>` 时，可以设置此项。其设置范围是 $1 \sim 65535$ 的整数。通过数字键盘输入所需测量的谐波阶数，默认显示 1，表示检测 1 阶谐波（即基波）。`<Harmonic>` 谐波阶数设置的限制是 $(\mathrm{Harmonic} \times \mathrm{Freq}) \le 100/500\ \mathrm{kHz}$，其中 $\mathrm{Freq}$ 表示参考信号频率。一旦超过限制时，系统会把谐波阶数自动往下调整直到满足条件。同时，当设置为 0 时，系统自动变化为 1。

例如输入信号是频率为 $1\ \mathrm{kHz}$ 的方波时，假定它的峰峰值为 A，设置 `<Harmonic>` 值分别为 1、2、3、4、5、6……时，将预期得到 R 值为 $0.45A$、$0$、$0.15A$、$0$、$0.09A$、$0$……，而这个序列正是方波信号傅立叶级数的系数序列的 A 倍。

☆ 注：多解调器测量的显示需在 `<Measure Selection>` 的 `<Base>` 选项卡中选择测量通道。

#### 解调器的参考频率设置

当 `<Demodulator Mode>` 设置为 `<Arbitrary Frequency>` 时，可以设置此项。`<Arbitrary Frequency>` 设置为某个频率时，解调器即以该频率为参考频率来解调信号。

在输入信号包含多个频率信息，而用户需要分别提取出来的时候，这个模式尤为有用。

### 6.3.6 滤波器配置

在同样的测量准确度下，使用更高的滤波器陡降可以降低时间常数，使得测量响应更快。具体的时间常数和滤波器陡降搭配，必须根据实际情况来选择。一个判定的准则是：只要对测量结果的稳定度满意，此时的时间常数和滤波器陡降就不需要设置太大，以免等待时间过长。当然，若想结果更加平稳，可以适当增大时间常数和滤波器陡降。

滤波器参数的软件配置区域如 $\underline{图\ 42}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//2ce32b16-2e35-4f48-8cdf-11321560de1e/markdown_3/imgs/img_in_image_box_179_379_1011_844.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fc2fb1b0e0020025488bbed08938dddf113b321ca4d474d9d64f92accf317e6ac" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 42. 滤波器配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 7}$：

<div style="text-align: center;">表 7. 滤波器配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Time Constant`<br>滤波器时间常数设置 | $1\ \mathrm{\mu s} \sim 3\ \mathrm{ks}$，默认 $100\ \mathrm{ms}$ |
| `Filter Slope`<br>滤波器陡降设置 | $6\ \mathrm{dB/oct}$、$12\ \mathrm{dB/oct}$（默认设置）、$18\ \mathrm{dB/oct}$、$24\ \mathrm{dB/oct}$、$30\ \mathrm{dB/oct}$、$36\ \mathrm{dB/oct}$、$42\ \mathrm{dB/oct}$、$48\ \mathrm{dB/oct}$ |
| `Sync Filter`<br>同步滤波器设置 | `OFF` / `ON`<br>关闭 / 开启，默认关闭 |

当信号频率低于 $1\ \mathrm{kHz}$ 时可以开启同步滤波器。低通滤波器在输入信号频率较低时无法或需长时间才能得到稳定的结果，此时可借助于此同步滤波器改善效果。

同步滤波器可以有效去除参考频率及其倍频的信号，降低对低通滤波器的要求。

☆ 注：同步滤波器开启时，`<Filter dB/oct>` 必须为 $18\ \mathrm{dB/oct}$ 以上才能真正起作用！

### 6.3.7 输出通道配置

该项参数的软件配置区域如 $\underline{图\ 43}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_0/imgs/img_in_image_box_179_251_1011_718.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F1b9d71b2021be3305fe00121e7de30fc2f56a79a3be6c7dd204efa465c348e01" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 43. 输出通道的配置区域图</div>

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_0/imgs/img_in_image_box_415_773_813_1073.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F34ba406e11f75a49b25f63bfc21dd3fe671b94413a99e917a9c39c6329e425d3" alt="Image" width="33%" /></div>

<div style="text-align: center;">图 44. 辅助输出配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 8}$：

<div style="text-align: center;">表 8. 输出通道配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Output Channel`<br>输出通道选择 | `CH1` / `CH2` |
| `Output Source`<br>输出源 | 可以控制上位机输出通道 CH1/CH2 控制两路辅助 `AUX_DAC` 输出直流电压，由用户手动输入，电压范围为 $-10\ \mathrm{V}$ 至 $+10\ \mathrm{V}$，默认输出为 $1\ \mathrm{V}$，最小分辨率为 $1\ \mathrm{mV}$；<br>也可以输出用户需要的数值，数值类型包括信号的 X/Y/R/θ 值、信号谐波的 X/Y/R/θ 值、频率、噪声值以及辅助输入值；默认设置 `AUX_DAC` 输出直流电压。 |
| `Offset(%)`<br>偏置设置 | 可调范围是 $-100\% \sim +100\%$，最小步进为 $0.01\%$，默认 $0.00\%$。只能对 R/X/Y/θ 值进行设置，默认值为 0。 |
| `Expand`<br>放大设置 | 可调范围是 $0.001 \sim 10000$，默认值为 1。 |

通过 Console 输入，`Expand` 可调范围是 $0.001 \sim 10000$，默认值为 1。但 `Expand` 的设置使得计算超出了 $\pm 10\ \mathrm{V}$ 的时候，输出值将会维持在 $\pm 10\ \mathrm{V}$。

输出信号的计算公式如下：

1. 当选择信号为 `<R>`、`<X>`、`<Y>`、`<谐波的 X>`、`<谐波的 Y>`、`<谐波的 R>`、`<X-Noise>`、`<Y-Noise>` 时：

$$ \text{输出} = \left( \frac{\text{Signal}(\text{选择信号})}{\text{Range}} + \text{Offset} \right) \times \text{Expand} \times 10\ \mathrm{V} $$

2. 当选择信号为 `<θ>`、`<θD1>`、`<谐波的 θ>` 时：

$$ \text{输出} = \frac{\text{Signal}(\text{选择信号})}{180^{\circ}} \times 10\ \mathrm{V} $$

3. 除了上面两种情况，还有下面选项：

   a) `AUXOUT`：按照用户设定的电压值输出。  
   b) `ADC1` ~ `ADC2`：输出等于 `AUX-IN` 的输入电压。  
   c) 频率 `Freq`：

频率每个阶梯分 $5\ \mathrm{V} \sim 10\ \mathrm{V}$，例如：

| 频率 | 输出电压 |
|------|---------|
| $1000\ \mathrm{Hz}$ | $5\ \mathrm{V}$ |
| $1200\ \mathrm{Hz}$ | $6\ \mathrm{V}$ |
| $1600\ \mathrm{Hz}$ | $8\ \mathrm{V}$ |
| $1800\ \mathrm{Hz}$ | $9\ \mathrm{V}$ |
| $1990\ \mathrm{Hz}$ | $9.95\ \mathrm{V}$ |
| $2000\ \mathrm{Hz}$ | $5\ \mathrm{V}$（下一阶梯） |

阶梯定义为：

- $62.5\ \mathrm{Hz} \sim 125\ \mathrm{Hz}$
- $125\ \mathrm{Hz} \sim 250\ \mathrm{Hz}$
- $250\ \mathrm{Hz} \sim 500\ \mathrm{Hz}$
- $1\ \mathrm{kHz} \sim 2\ \mathrm{kHz}$
- $500\ \mathrm{Hz} \sim 1000\ \mathrm{Hz}$
- $4\ \mathrm{kHz} \sim 8\ \mathrm{kHz}$
- $8\ \mathrm{kHz} \sim 16\ \mathrm{kHz}$

☆ 注：每一个 CH 通道有一个独立的偏置值和放大值。假如设置了 CH1 的 `<Offset>` 是 $50\%$ 和 `<Expand>` 是 3，那只有 CH1 通道输出会受影响，CH2 的输出不变。

☆ 注：`<Offset>` 与 `<Expand>` 的设置不会影响动态区域数据框内的数据显示。

### 6.3.8 正弦信号输出与 TTL 参考信号配置

该项参数的软件配置区域如 $\underline{图\ 45}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_2/imgs/img_in_image_box_180_283_1010_750.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2Ffd411ceb2be3a3297239cafea75b18f3e8d1308269df3ce716458613f5257557" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 45. 正弦信号输出配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 9}$：

<div style="text-align: center;">表 9. 正弦信号输出配置选项表</div>

| 参数 | 说明 |
|------|------|
| `SineOut / TTL Out Type`<br>正弦信号与 TTL 参考信号输出模式设置 | `OFF` / `Fixed`<br>关闭 / 定值正弦信号，默认关闭 |
| | `TTL OUT` 输出接口提供 $5\ \mathrm{V}$ TTL/CMOS 兼容的方波信号，输出阻抗为 $200\ \mathrm{\Omega}$，其频率与 `SINE OUT` 相同。 |
| `Sine Out Voltage(Vrms)`<br>定值信号幅值设置 | 当正弦信号输出模式选择定值正弦信号时可操作此项，由用户手动输入，电压值范围为 $100\ \mathrm{\mu V}_{\mathrm{rms}}$ 至 $5\ \mathrm{V}_{\mathrm{rms}}$，默认输出为 $1\ \mathrm{V}_{\mathrm{rms}}$，最小分辨率为 $1\ \mathrm{\mu V}_{\mathrm{rms}}$。 |

OE1300 可通过前面板的 `Sine Out` SMB 接头输出幅值由 $100\ \mathrm{\mu V}_{\mathrm{rms}}$ 到 $5\ \mathrm{V}_{\mathrm{rms}}$ 的正弦波信号。

当使用 `<External>` 外部参考时，`<Sine Out>` 提供一个与外部参考锁相的正弦信号；当使用 `<Internal>` 内部参考时，将由 OE1300 自身的振荡器产生信号。同时前面板上 `TTL OUT` 的 SMB 头将输出与 `<Sine Out>` 同频的 TTL 信号。

### 6.3.9 数据保存配置

该项参数的软件配置区域如 $\underline{图\ 46}$ 红框内所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_3/imgs/img_in_image_box_211_255_980_686.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fe5522850d1fd1de6a512664257be652c00c789b69f3df3413b86cf973f670996" alt="Image" width="64%" /></div>

<div style="text-align: center;">图 46. Sample 数据保存配置区域图</div>

可供用户配置的选项如下 $\underline{表\ 10}$：

<div style="text-align: center;">表 10. Sample 数据保存配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Sample Rate(s)` | 用于设定采样的时间间隔，用户手动输入，时间间隔范围为 $1\ \mathrm{ms}$ 到 $100\ \mathrm{s}$，分辨率最小为 $1\ \mathrm{ms}$，默认为 $100\ \mathrm{ms}$。 |
| `Save Path` | 数据以 csv 表格的形式保存文件，保存在程序目录下。 |

软件有数据记录保存的功能，可根据用户需要选择是否保存一段时间内的 OE1300 采集到的数据。

保存的数据包括测量信号的 R、X、Y、θ、频率和噪声的值；测量的七路谐波的 R、X、Y 和 θ 的值；以及两路辅助输入的信号值。

选择是否存储数据的具体步骤如下：

1. 数据以 csv 表格的形式保存文件，保存在程序目录下。
2. 点击 `Save Data` 弹出 $\underline{图\ 47}$ 文件保存窗口，此时可修改文件名称和保存路径，点击“保存”按钮保存文件。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_4/imgs/img_in_image_box_252_145_1020_571.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2F3e2461bd253c3a679014ea81c9bafb74b5d7ecb014084e79b12a7fc5873a0b6c" alt="Image" width="64%" /></div>

<div style="text-align: center;">图 47. 数据文件保存窗口</div>

3. 当数据保存时，Sample 界面会显示 `saving`，如 $\underline{图\ 48}$ 所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//b204d4b9-628b-48b0-a2aa-4f9072dba92d/markdown_4/imgs/img_in_image_box_454_666_818_1088.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A04Z%2F-1%2F%2F13a2e8e2bbc483bd50137e0c4aaa2633667a83a3e9b91fdfedb3b96a41dab967" alt="Image" width="30%" /></div>

<div style="text-align: center;">图 48. 数据保存配置区域图</div>

4. 再次按下 `Saving` 按钮，按钮状态由 `Saving` 重新变为 `Save Date`，表示停止保存采集的数据。
5. 在 `Sample Rate(S)` 可以修改当前显示和保存数据的采样率，输入范围为 $0.1\ \mathrm{s} \sim 100\ \mathrm{s}$。

### 6.3.10 谐波波形显示

在软件左窗口选择“谐波波形显示”选项页，$\underline{图\ 49}$ 如下：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_0/imgs/img_in_image_box_174_252_1009_721.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A58Z%2F-1%2F%2Fb033f8053fd73a270dfab6cee15195ad34f426384afb2a896c217f6a05859acb" alt="Image" width="70%" /></div>

<div style="text-align: center;">图 49. 谐波波形显示图</div>

此时左窗口中可以分别显示两个谐波的 XY 坐标图。对每一个谐波可设置显示 R、X、Y 和 θ 值。

### 6.3.11 关闭输出检测窗口

在软件内可以选择打开/关闭输出检测窗口，如 $\underline{图\ 50}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_1/imgs/img_in_image_box_180_253_1009_719.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Fd7edc1521bbe042caa188e2f568b0c8c4f4af9d31154965a298e3608e53bca76" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 50. 关闭输出检测窗口</div>

### 6.3.12 示波器窗口

在软件使用 TCP/IP 连接时，左上目录能够选择 `Function` → `Oscilloscope` 示波器功能，如 $\underline{图\ 51}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_1/imgs/img_in_image_box_179_906_1010_1376.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2F6f039f8c7924880dd6c311b0a32f71208950f136c420a84a3f7aef75b3df05ab" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 51. 软件示波器功能</div>

此时可以打开左下角 `NEW` 调出测量选择菜单，调出对应显示窗口；同时右边示波器功能窗口可以调节示波器基础设置，如 $\underline{图\ 52}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_2/imgs/img_in_image_box_179_143_1011_605.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fdc925b5d438233cda227fa16e60b44bb0ad274309020250c0600c90f307ee297" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 52. 示波器 Trigger 及测量功能菜单设置</div>

<div style="text-align: center;">表 11. 示波器 Trigger 功能配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Sampling Rate`<br>采样率设置 | $15.26\ \mathrm{Hz} \sim 4\ \mathrm{MHz}$<br>值等于基本采样率除以 $2^n$，其中 $n$ 是整数。 |
| `Length(pts)`<br>显示长度或持续时间 | `8192` / `4096` |
| `Edge`<br>触发设置 | `Rising` / `Falling`<br>上升沿触发 / 下降沿触发 |
| `Level`<br>触发信号范围设置 | 设置触发电平值（允许负值） |

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_3/imgs/img_in_chart_box_221_142_1055_603.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F55eb89a0fdefb6b5f8942c11e4b496f1fd4ddc44ec52e5084c65c8b028d5d386" alt="Image" width="70%" /></div>

<div style="text-align: center;">图 53. 示波器 Cursor 功能菜单设置</div>

<div style="text-align: center;">表 12. Cursor 功能配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Type`<br>光标设置 | `Off` / `Voltage` / `Frequency` / `Both`<br>关闭 / 电压轴（Y）/ 时间轴（X）/ 同时开启 X 和 Y 光标 |
| `Cursor Line`<br>光标移动设置 | `Single` / `Both`<br>单个移动光标 / 同时移动 X 和 Y 光标 |

### 6.3.13 FFT 窗口

在软件使用 TCP/IP 连接时，左上目录能够选择 `Function` → `FFT` 测量功能，如 $\underline{图\ 54}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_4/imgs/img_in_chart_box_180_281_1010_754.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F5cc8391b1480abbe9ef9e91ea9fb9b334aabb3ed7880eea1d82e2e65ec4f8bd5" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 54. 频谱分析功能</div>

红框中 FFT 功能窗口可以调节 FFT 基础设置，如 $\underline{图\ 55}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4e6af67c-8934-471f-9356-a2397a081d8f/markdown_4/imgs/img_in_chart_box_179_935_1010_1408.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fca2a2d2a51317c9623ee6ae847e4a52aac750f8f0315c2cee3ad61390f0b66c7" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 55. 频谱分析 Trigger 功能菜单设置</div>

<div style="text-align: center;">表 13. 频谱分析 Trigger 功能配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Sampling Rate`<br>采样率设置 | $15.26\ \mathrm{Hz} \sim 4\ \mathrm{MHz}$<br>值等于基本采样率除以 $2^n$，其中 $n$ 是整数。 |
| `Length(pts)`<br>显示长度或持续时间 | `8192` / `4096` |

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_0/imgs/img_in_chart_box_180_380_1010_853.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Ff8c9fa7b76408902a8beb1d231e045f53c76e12d60ccf429da21d14acd2e12be" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 56. 频谱分析 Cursor 功能菜单设置</div>

<div style="text-align: center;">表 14. 频谱分析 Cursor 功能配置选项表</div>

| 参数 | 说明 |
|------|------|
| `Type`<br>光标设置 | `Off` / `Voltage` / `Frequency` / `Both`<br>关闭 / 电压轴（Y）/ 时间轴（X）/ 同时开启 X 和 Y 光标 |
| `Cursor Line`<br>光标移动设置 | `Single` / `Both`<br>单个移动光标 / 同时移动 X 和 Y 光标 |

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_1/imgs/img_in_chart_box_180_141_1010_613.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A59Z%2F-1%2F%2Fada94a37d2ac57c3b843d1929d8e14836e23115e5fd0c9757a25ff8c0a9af15b" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 57. 频谱分析 Advanced 功能菜单设置</div>

<div style="text-align: center;">表 15. 频谱分析 Advanced 功能配置选项表</div>

| 参数 | 说明 |
|------|------|
| `FFT Window`<br>FFT 窗函数设置 | `Rectangular` / `Hanning` / `Blackman` / `Hamming`<br>四种不同的 FFT 窗函数可供选择。每个窗函数在幅值精度和频谱泄漏之间会有不同程度的折衷。请查看相关文献，以便找到最符合您需求的窗函数。 |

### 6.3.14 PID 控制窗口

在软件控制主界面左上目录能够选择 `Function` → `PID` 测量功能，如 $\underline{图\ 58}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_2/imgs/img_in_image_box_180_255_1010_648.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fed90718213aef56bfd07a88799eef7e5f3259e9c65491b56c97d31e9b69d9390" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 58. PID 控制功能</div>

红框中 PID 功能窗口可以调节 PID 基础设置，有两个独立、可分别配置的 PID 可选择，如 $\underline{图\ 59}$ 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_2/imgs/img_in_image_box_179_769_950_1233.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F4c78b284fafc027f09854f0a6787e67d6d3488bafc6a32ac09dfc9eddcf37772" alt="Image" width="64%" /></div>

<div style="text-align: center;">图 59. PID 控制菜单设置</div>

使用 PID 功能时，可以先将锁放接入系统，再配置好所有 PID 参数，开启 PID 即可。

如果被测设备（DUT）的传递函数未知，并且只有少量噪声从环境中耦合到系统中，那么通常手动操作是最快的方法。手动配置新的控制环时，建议首先采用较小的 P 值，并将其他参数（I、D、Offset）设置为零。通过启用控制器，能够立即看到 P 的方向是否正确以及反馈是否作用于正确的输出参数，可以通过检查显示在 PID 选项卡中的 `<Output Select>` 的值来确认输出参数。积分增益 I 的逐步提高有助于将 PID 误差信号完全归零。启用微分增益 D 可提高反馈回路的速度，但也会导致反馈回路行为不稳定。用户可根据实际环路响应的速度和效果，调整 P、I、D 的参数。过程中也可调整滤波器带宽、限幅器的限幅范围等参数。

在 PID 参数调整过程中，建议用户使用示波器查看 PID 的输入输出接口。OE1300 的上位机通讯接口速率最高是 $1\ \mathrm{ksps}$，而 PID 模块的实际运行速率最高为 $4\ \mathrm{Msps}$，上位机存在欠采样的问题，有可能无法捕捉一些高频的震荡现象。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_3/imgs/img_in_image_box_179_335_1014_791.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2F2eff0824a2c20100572962cdab75dd89319b8721373fa2cad8b9028a4bca69fe" alt="Image" width="70%" /></div>

<div style="text-align: center;">图 60. PID 采样保存设置</div>

<div style="text-align: center;">表 16. PID 采样保存选项表</div>

| 参数 | 说明 |
|------|------|
| `Sample`<br>PID 控制器采样保存速率设置 | 可调范围是 $0.100 \sim 10000.000\ \mathrm{s/sample}$，默认值为 $0.100$ |
