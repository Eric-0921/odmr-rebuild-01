## 6. PC 软件安装使用说明

### 6.1 软件安装

我们提供的光盘包含有 PC 端控制软件和驱动。打开光盘后有如下文件，如 43 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_0/imgs/img_in_image_box_197_399_996_674.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A40Z%2F-1%2F%2F54b9e52faddbaf3f7ced5d56f4b4b615d33694078d48ea0d45c8cb8f7092a562" alt="Image" width="67%" />

图 43. 光盘内 PC 软件包

##### 第一步：软件驱动安装

如果 PC 本地没有安装 NI LabView 2011 或更新的版本，并且没有安装对应的 VISA 驱动的话，则需要安装图 43 中第 3 个文件夹内的驱动，打开 “OE1022D 软件驱动”，如图 44：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_0/imgs/img_in_image_box_181_908_1000_1110.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A40Z%2F-1%2F%2F30bf7cd197c70304930afa436c4bd6f3fd752a73d79ca429b392079560786109" alt="Image" width="68%" />

图 44. “OE1022D 软件驱动”文件夹

双击打开上图中红色方框内的 “setup.exe” 安装文件，开始安装 OE1022D PC 软件的使用环境驱动，一般情况下根据默认选项安装就可以了。

##### 注意：

1、安装成功后，需要重启电脑，以完成软件环境的配置。

2、如果 PC 机已安装有 NI LabView 2011 或以上版本，并且安装有对应 LabView2011 版本的 VISA 驱动，则只需要安装好串口驱动（第二步）后，再直接进行第三步就可以使用锁相放大器的配置软件进行相关参数设置了。

##### 第二步：驱动安装

说明：OE1022D 通信接口有 USB2.0 和 RS232 两种接口，用户根据自己需求安装对应的接口驱动

RS232 串口驱动安装方法：

打开图 43 中的第 6 个文件夹 “串口驱动”，如图 45 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_1/imgs/img_in_image_box_249_280_983_508.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F7632c39ad61c453084d99c289a2a319840daf128e438f6ea1b13e8bbfde7655b" alt="Image" width="61%" />

图 45. “串口驱动”文件夹

双击打开图 45 红色方框内的 “FT232 drive.exe” 文件，则会弹出如图 46 的软件窗口，表示此时正在安装串口驱动，只需要等待几分钟即可。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_1/imgs/img_in_image_box_234_656_955_1126.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F77cb0239e8ec08aa8639a2f5932be74dcd8c77e903d866900f4e0a58a588657e" alt="Image" width="60%" />

图 46. FT232 driver 安装界面

若 FT232 驱动安装成功后，会出现如图 47 的提示，此时只需要按照提示按下“回车”键即可完成该驱动的安装：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_2/imgs/img_in_image_box_230_154_961_627.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F7b81a0dad5397d6c896ba6aeb68123e4f5e9a756c1679d6b46482b2fdd196f91" alt="Image" width="61%" />

图 47. FT232 driver 安装完成提示

此时，使用串口线连接 PC 机和锁相放大器，则可自动识别连接成功。

##### 注意：

1. 如果 PC 机已经联网，当插上串口线连接 PC 与锁相放大器时，会自动联网搜索驱动并进行安装。

2. 如果 PC 机已安装有串口驱动，则可跳过该步。

3. 如果安装不成功，则可根据“串口驱动”目录下的说明文件“readme.txt”里面的解决方法，使用 inf 文件进行安装。

USB2.0 驱动安装方法：

打开图 43 中的第 4 个文件夹 “USB 驱动”，如图 48 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_2/imgs/img_in_image_box_235_1029_927_1223.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A41Z%2F-1%2F%2F5f55fc7a6ca36e7ca03c27ec5938a6a1211d37664daf7ab61f1d8dec67fc0585" alt="Image" width="58%" />

图 48. “USB 驱动”文件夹

USB2.0 驱动有多个版本选择，针对不同的系统，用户根据自己 PC 系统版本来选择安装，双击打开对应版本安装文件，开始安装 USB2.0 驱动，一般情况下根据默认选项安装就可以了。

注意：WIN10 系统是不用安装 USB2.0 驱动，直接插入 USB 线既可成功连接 OE1022D 上位机软件。

#### 第三步：运行软件

完成上述步骤后，用户则可根据自己用的通信接口选择对应的应用软件，使用 USB2.0 接口请打开 OE1022D_console_USB 文件夹，使用 RS232 串口请打开 OE1022D_console_RS232 文件夹，两个文件目录下都有以下文件：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_3/imgs/img_in_image_box_199_310_999_394.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2F26fb5ede9a62ddf4c2d636701895e76bcec230af19c80f5312c6b7976dfac65c" alt="Image" width="67%" />

图 49. “OE1022D”文件夹

双击打开图 49 中红色方框内的“OE1022D_Console_CH.exe”文件，如前面的安装无误，则会弹出以下软件窗口，此时可以在 PC 机上开始进行锁相放大器的参数配置：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_3/imgs/img_in_image_box_181_541_1012_1007.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A42Z%2F-1%2F%2Fbdd312c22aae714fb12cb011bbd8591eb00e8f6818b12d0baec35c6e39d2c58c" alt="Image" width="69%" />

图 50. OE1022D 软件界面

注意：如果 PC 机已安装有 NI LabView 2011 或以上版本，并且安装有对应 LabView2011 版本的 VISA 驱动，则只需要安装好串口驱动（第一步）后，再直接进行第三步就可以使用锁相放大器的配置软件进行相关参数设置了。

### 6.2 软件使用说明

前面所示的图 50 是锁相放大器 OE1022D（以下简称 OE1022D）当前版本的 PC 机配置软件的界面。

#### 6.2.1 软件运行

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_4/imgs/img_in_image_box_814_286_921_365.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2F39c912b6cb968c609e4a5485285a67ef08bd676edd5074fc003fbcc29273548b" alt="Image" width="8%" />

在运行。此时若点击按钮“ $ \underline{\text{连接中...}} $”，则停止当前软件，该部分变为“ $ \underline{\text{⚪}} $”，表示当前软件没有运行，如图 51 右边部分所示。再次点击“ $ \underline{\text{连接}} $”按钮，此时软件又开始运行。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_4/imgs/img_in_image_box_432_437_791_566.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2F1053771f47e2868536a9f5541bc7c7dd92b9bcf37b73d1c91aa5684aa5fea35a" alt="Image" width="30%" />

图 51. 程序运行（左图）与程序停止（右图）

#### 6.2.2 PC 机与 OE1022D 连接

要使用该软件正确地配置 OE1022D，首先需要完成 PC 机和 OE1022D 的正确连接。图 52 所示为软件界面中显示当前连接状态的部分：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_4/imgs/img_in_image_box_304_829_887_973.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2Fc8037070b00101fbb66c71160d6c555fa34dd678b3b1933115287473739ce580" alt="Image" width="48%" />

图 52. 当前连接状态

如图 52 所示，点击左边的连接选项“连接”，软件会自动搜索串口资源，若 PC 机和 OE1022D 连接成功，此时下方窗口会显示 OE1022D 的版本号信息，如图 53 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//94e144ff-8230-4de5-b91d-b090bd24501f/markdown_4/imgs/img_in_image_box_305_1146_885_1284.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2F12fbe36524a4e1c0a0dd4913a19e257f3260cfc8be2efac8c0527d048cf00cb8" alt="Image" width="48%" />

图 53. 连接成功图

若等待几秒后，弹出如图 54 所示的提示框，则表示 PC 机与 OE1022D 连接异常。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_0/imgs/img_in_image_box_384_153_806_289.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A11Z%2F-1%2F%2F36b27cb5e06a40e67dfd15ffaa1e8972366468c0fabc083f56c3f6f7333773ae" alt="Image" width="35%" />

图 54. 搜索设备失败提示框

再次确认设备连接正确后，点击提示框中“确定”按钮，此时连接状态右方窗口会显示出错信息，如图 55 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_0/imgs/img_in_image_box_293_364_1012_577.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A11Z%2F-1%2F%2Fe1a35bafe0fdadd0d41825faf89095cdf3842afa446639e0fc0b3022ea42cafb" alt="Image" width="60%" />

图 55. 连接错误图

此时有两种方法可以重新连接：

（1）点击“串口配置信息”选项右边的“☑”，在弹出的下拉菜单中点击“刷新”则可以显示出当前 PC 机所连接的全部外部 COM 资源，用户需要选择正确的 COM 口来进行 PC 与 OE1022D 的连接。

注意: 当 PC 机使用 USB 线连接 OE1022D 时，可以在电脑的设备管理器里确定当前的连接使用的是哪个 COM 口，具体操作步骤是：选择“我的电脑”->右键->属性->设备管理器->查看“端口(COM 和 LPT)”。

若 COM 端口选择正确，等待几秒后，连接状态会更新为图 53 所示，连接成功。

（2）点击“ $ \underline{\text{连接中...}} $”按钮，使其变为“ $ \underline{\text{连接}} $”，此时可再次点击“ $ \underline{\text{连接}} $”按钮，重复上述连接操作，直到连接成功。

当连接成功后，如图 56 所示，红框内窗口内容将清空，并重新开始显示所有测量数值。同时，软件界面将显示 OE1022D 当前配置的各项参数。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_1/imgs/img_in_image_box_181_154_1010_620.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A12Z%2F-1%2F%2F3205e2a91d6efc14ff44a3f45392f1213da3c59add6341976e660ec08872377a" alt="Image" width="69%" />

图 56. OE1022D 软件连接成功

#### 6.2.3 输入与滤波器配置

输入与滤波器配置区域如下图所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_1/imgs/img_in_image_box_180_813_994_1268.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A13Z%2F-1%2F%2Fc02ba2f6424b71fba6cd34cb8872133e8e13c8e2e81ab6557b13ed12d43e639a" alt="Image" width="68%" />

图 57. 输入与滤波器配置区域图

可以根据待测信号的特点，对输入方式进行配置，如去除工频干扰，可以开启 50Hz 陷波器；对 OE1022D 的动态储备进行配置，如信号噪声相对比较大的情况可以选择高储备；对低通滤波器进行配置，如测量 200Hz 以下频率时，可以开启同步滤波器；对谐波次数进行配置，如测量基波以及 3 次谐波，可以配置谐波 1 为 1，配置谐波 2 为 3。以下为各个部分的详细介绍。

##### 6.2.3.1 输入信号的软件配置

输入信号的软件配置区域如图 58 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_2/imgs/img_in_image_box_187_235_1004_688.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A14Z%2F-1%2F%2F32d49aa1f87857e5f032acf8fc2a874f5059023c48f96fc39e13f1a96f5025af" alt="Image" width="68%" />

图 58. 输入信号的配置区域图

可供用户配置的选项如下表 6:

表 6. 输入信号配置选项表

| Input Source输入信号源设置 | Single-Ended Voltage单端电压信号 |
| --- | --- |
| Input Source输入信号源设置 | Differential Voltage差分电压信号 |
| Input Source输入信号源设置 | 1 MOhm Current Gain输入为微弱电流信号时，放大倍数 10E6 V/A |
| Input Source输入信号源设置 | 100 MOhm Current Gain输入为微弱电流信号时，放大倍数 10E8 V/A |
| Input Shield Grounding输入屏蔽接地设置 | Float输入接头地与仪器地（仪器地已短接在大地--市电 GND 上）通过 10 K \Omega 电阻隔离 |
| Input Shield Grounding输入屏蔽接地设置 | Ground输入接头地与仪器地通过 10 \Omega 电阻短接 |
| Input Coupling输入耦合设置 | AC交流耦合 |
| Input Coupling输入耦合设置 | DC直流耦合 |
| Input Notch Filter输入陷波器设置 | None不开启陷波器 |
| Input Notch Filter输入陷波器设置 | Line Notch开启 50 Hz 陷波器，抑制工频干扰 |
| Input Notch Filter输入陷波器设置 | Both Notch Filters |
|  | 同时开启 50 Hz 和 100 Hz 陷波器，抑制工频和工频二次谐波干扰 |
|  | 2x Line Notch |
|  | 开启 100Hz 陷波器，抑制工频干扰 |

##### 注意：

1 当选择好需要的设置后，需要点击“配置输入信号”，以完成输入信号的配置；全部设置都可同时更改，只需点击一次“配置输入信号”即可。

2. 每次重新修改其中的设置后，都需要点击该按钮以完成配置，否则设置无效。

##### 6.2.3.2 动态储备和灵敏度配置

该项参数的软件配置区域如图 59 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_3/imgs/img_in_image_box_181_628_1011_1093.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A15Z%2F-1%2F%2F757e3ee836178017b2f062947465d4681fc67f85d2ef13944a411ca519f222a3" alt="Image" width="69%" />

图 59. 动态储备和灵敏度配置区域图

可供用户配置的选项如下表 7:

表 7. 动态储备和灵敏度配置选项表

| Dynamic Reserve动态储备设置 | Low Noise 低噪声Normal 正常High Reserve 高储备 |  |  |
| --- | --- | --- | --- |
| Sensitivity满偏灵敏度设置 | 1 nV/fA2 nV/fA5 nV/fA10 nV/fA20 nV/fA50 nV/pA100 nV/pA1uV/pA200 uV/pA500 uV/pA1mV/nA200 mV/nA500 mV/nA100 nV/fA20 uV/pA5 mV/nA1V/uA |  |  |
| Auto Sensitivity自动灵敏度设置 | Disable/Enable关闭/开启 |  |  |
| Auto Reserve自动动态储备设置 | Disable/Enable关闭/开启 |  |  |

注意：

1. 当选择好需要的设置后，需要点击“配置动态储备与灵敏度”，以完成各项参数的配置；全部参数都可同时更改，只需点击一次“配置动态储备与灵敏度”即可。

2. 每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置动态储备与灵敏度}} $”按钮以完成配置，否则设置无效。

##### 6.2.3.3 滤波器配置

滤波器参数的软件配置区域如图 60 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//4f9b2886-20d6-45c3-8f02-1cf90d1990fb/markdown_4/imgs/img_in_image_box_182_926_1011_1389.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A16Z%2F-1%2F%2F83eacb0d8041cce63818b9f3305f425f249839512b8a696eb73726bd08743bc3" alt="Image" width="69%" />

图 60. 滤波器配置区域图

可供用户配置的选项如下表 8:

表 8. 滤波器配置选项表

| Time Constant | 10 us | 3 ms | 1 s | 300 s |
| --- | --- | --- | --- | --- |
| Time Constant | 30 us | 10 ms | 3 s | 1000 s |
| Time Constant | 100 us | 30 ms | 10 s | 3000 s |
| 滤波器时间常数设置 | 300 us | 100 ms | 30 s |  |
| 滤波器时间常数设置 | 1 ms | 300 ms | 100 s |  |
| Sync Filter同步滤波器设置 | Disable/Enable同步滤波器关/同步滤波器开 |  |  |  |
| Filter Slope滤波器陡降设置 | 6 dB/oct12 dB/oct18 dB/oct24 dB/oct |  |  |  |

##### 注意：

1. 当选择好需要的设置后，需要点击“配置滤波器”，以完成各项参数的配置；全部参数都可同时更改，只需点击一次“配置滤波器”即可。

2. 每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置滤波器}} $”按钮以完成配置，否则设置无效。

3.”Sync Filter”的配置相对独立，不是通过按钮”配置滤波器”，而是点击相应的”同步滤波器关”来选择是否开启同步滤波功能。当按钮被按下并显示”同步滤波器开”表示OE1022D已经开启了同步滤波器功能，当用户需要关闭同步滤波器时，再次点击该按钮会重新复位为初始状态” $ \underline{\text{同步滤波器关}} $”，表示OE1022D已经关闭了同步滤波器功能。

##### 6.2.3.4 谐波测量配置

谐波的软件配置区域如图 61 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//7e11dd39-d0c5-43e7-9307-9be399fef01c/markdown_1/imgs/img_in_image_box_182_272_1012_739.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A25Z%2F-1%2F%2Fbccebcd895095248126423ebd0bb4f4bec917f1283a84aec32910d3fe660be94" alt="Image" width="69%" />

图 61. 谐波及自动设置配置区域图

可供用户配置的选项如表 9:

表 9. 谐波及自动设置配置选项表

| Harmonic 1 谐波 1 检测设置 | 用户手动输入，范围是 1—32767 |
| --- | --- |
| Harmonic 2 谐波 2 检测设置 | 用户手动输入，范围是 1—32767 |
| Auto Scale 自动设置量程 | Disable/Enable 关闭/开启 |

##### 注意：

1. 当选择好需要的谐波设置后，需要点击“配置谐波检测”，以完成各项参数的配置；全部参数都可同时更改，只需点击一次“配置谐波检测”即可。

2.“自动灵敏度”、“自动动态储备”、“自动相移”和“自动量程”的配置相对独立，不是通过按钮“配置动态储备与灵敏度”，而是点击相应的“关闭”来选择是否开启自动功能。“关闭”表示不开启，当点击“关闭”时，该按钮会变为“处理中”，表示OE1022D正在执行自动功能，当执行完毕后，按钮会重新复位为初始状态“关闭”，此时的“动态储备”和“灵敏度”一项会更新为OE1022D返回的值。

#### 6.2.4 参考信号及扫频配置

该项参数的软件配置区域如图 62 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//7e11dd39-d0c5-43e7-9307-9be399fef01c/markdown_2/imgs/img_in_image_box_180_264_1011_725.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A26Z%2F-1%2F%2Ff0d802a3c5a607e5b439170392da64c559fe890d614336b769e91ebb18264ba7" alt="Image" width="69%" />

图 62. 参考信号配置区域图

可供用户配置的选项如下表 10:

表 10. 参考信号配置选项表

| Reference Source | External外部参考信号 |
| --- | --- |
| Reference Source | Internal内部参考信号 |
| 参考信号源选择设置 | Int. Sweep内部扫频参考 |
| External Ref Trigger外部参考信号触发方式设置 | TTL Rising Edge TTL信号上升沿检测 |
| External Ref Trigger外部参考信号触发方式设置 | Sine Zero Crossing Sine信号过零检测 |

> 校正说明（2026-06-11）：
> 按原始 PDF 页面图面重新核对，本页 `V1.5` 源文件表格只明确列出 `TTL Rising Edge` 与 `Sine Zero Crossing` 两项。
> 旧整理稿中的 `TTL Falling Edge` 不应继续被当作该 PDF 页面的直接真值。
> 但真机只读观测显示：厂商 LabVIEW 软件将设备配置到 PLL 锁定后，`RSLPD? 2 = 2`、`FREQD? 2 ≈ 500 Hz`、`*PLLD? 2 = 1`。因此本页只能说明“PDF 显示项”，不能穷尽真实可观测枚举。
| Int.Frequency内部参考频率设置 | 用户手动输入，频率范围为1 mHz到102 kHz，频率分辨率最小为1 mHz |
| Phase(°)参考相位设置 | 设置PSD算法两路正交参考信号的相移角度，移相精度为0.01°，输入范围为-180°至+180° |
| Auto Phase自动相移设置 | Disable/Enable关闭/开启 |

##### 注意：

1. 当选择好需要的设置后，需要点击“配置参考信号”，以完成各项参数的配置；全部参数都可同时更改，只需点击一次“配置参考信号”即可。

2. 每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置参考信号}} $”按钮以完成配置，否则设置无效。

在参考信号配置中的“ $ \underline{\text{参考信号源}} $”里选择“ $ \underline{\text{内部扫频参考}} $”时，可对此项进行配置，其软件配置区域如图 63 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//7e11dd39-d0c5-43e7-9307-9be399fef01c/markdown_3/imgs/img_in_image_box_179_438_1011_902.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A27Z%2F-1%2F%2F04f0788ff6d8f83d8a25876295c994734c46cf67c86e1782b6d8a24ea96014e4" alt="Image" width="69%" />

图 63. 内部扫频参考配置区域图

可供用户配置的选项如下表 11:

表 11. 内部扫频参考配置选项表

| Int.Sweep Type内部扫描类型设置 | Linear线性扫频类型 |
| --- | --- |
| Int.Sweep Type内部扫描类型设置 | Log对数扫描类型 |
| Int.Sweep Run Mode扫频运行模式设置 | Single单次扫频 |
| Int.Sweep Run Mode扫频运行模式设置 | Loop循环扫频 |
| Int.Sweep Run Mode扫频运行模式设置 | Stop停止扫频 |
| Int.Sweep Start(Hz)内部扫频开始频率设置 | 用于设定扫频的开始频率值，由用户手动输入，频率范围为1 mHz到102 kHz，频率分辨率最小为1 mHz |
| Int.Sweep Step(Hz) | 用于设置线性扫频或者对数扫频每次步进的频率值，用 |
| 线性步进频率设置 | 户手动输入。线性扫频：步长范围为 1 mHz 到 102 kHz，频率分辨率最小为 1 mHz；对数扫频：步长范围为 0.001%到 100 % |
| Int.Sweep Stop(Hz) | 用于设定扫频的截止频率值，由用户手动输入，频率范围内部扫频截止频率设置 |
| Int.Sweep Stop(Hz) | 围为 1 mHz 到 102 kHz，频率分辨率最小为 1 mHz |
| Linear Step Time(ms) | 用于设置每次步进的时间，用户手动输入，步长范围为扫频步进时间设置 |
| Linear Step Time(ms) | 1 ms 到 100 s，频率分辨率最小为 1 ms |

##### 注意:

当选择好需要的设置后，需要点击“配置参考信号”，以完成各项参数的配置；全部参数都可同时更改，只需点击一次“配置参考信号”即可。

#### 6.2.5 通道输出与数据采样配置

通道输出与采样配置区域如下图 64 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_0/imgs/img_in_image_box_178_263_1012_725.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A47Z%2F-1%2F%2Fa742de278304d4c782448189edea8983f572e25cd345f23d02f263ae94158a36" alt="Image" width="70%" />

图 64. 数据缓存区配置区域图

通道输出与采样配置区域可以对数据缓存区配置、仪器的设置配置以及 Channel Out 通道输出进行配置，以下为各个部分的详细介绍。

##### 6.2.5.1 数据缓存区配置

该项参数的软件配置区域如图 65 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_0/imgs/img_in_image_box_213_972_977_1393.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A47Z%2F-1%2F%2F0009dd376ff237fcbd1f365886c1b3f8d699928b20df657b35994eda28a830c6" alt="Image" width="64%" />

图 65. 数据缓存区配置区域图

可供用户配置的选项如下表 12:

表 12. 数据缓存区配置选项表

| Buffer 1 缓存区 1 内容 | 可选择信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、四路辅助输入的信号值 ADC1/ADC2/ADC3/ADC4、噪声值、频率值、公式值 E1/E2/E3/E4 |
| --- | --- |
| Buffer 2 缓存区 2 内容 | 可选择信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、四路辅助输入的信号值 ADC1/ADC2/ADC3/ADC4、噪声值、频率值、公式值 E1/E2/E3/E4 |
| Buffer 3 缓存区 3 内容 | 可选择信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、四路辅助输入的信号值 ADC1/ADC2/ADC3/ADC4、噪声值、频率值、公式值 E1/E2/E3/E4 |
| Buffer 4 缓存区 4 内容 | 可选择信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、四路辅助输入的信号值 ADC1/ADC2/ADC3/ADC4、噪声值、频率值、公式值 E1/E2/E3/E4 |
| Step Time(ms) 采样间隔设置 | 用于设定采样的时间间隔，用户手动输入，时间间隔范围为 1 ms 到 100 s，分辨率最小为 1 ms |
| Length 采样长度设置 | 用于设定采样的数据长度，用户手动输入，长度范围为 1 到 16384，分辨率最小为 1 |
| Int/Ext 采样触发模式 | Int 内部采样触发 |
| Int/Ext 采样触发模式 | Ext 外部采样触发 |
| Single/Loop 采样运行模式 | Single 单次采样 |
| Single/Loop 采样运行模式 | Loop 循环采样 |

注意：

1. 当选择好需要的设置后，需要点击“ $ \underline{\text{配置采样}} $”，此时才能对右边的“ $ \underline{\text{开始采样}} $”进行设置。每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置采样}} $”按钮以完成配置并启动采样，否则设置无效。

开始采样

2.点击“开始采样”按钮，其下方出现进度条“▌”时，表示OE1022D已经启动数据采样，当进度条跑到100%后会消失不显示，表示数据采样已完成；在采样过程中可以通过点击“暂停采样”和“停止采样”按钮对采样过程进行控制。全部参数都可同时更改，只需点击一次“配置采样”即可。

3.采样完成之后才能对上方的“ $ \underline{\text{保存数据}} $”和“ $ \underline{\text{清除数据}} $”进行操作。这两个按钮用于保存和清除缓存区内的数据。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_1/imgs/img_in_image_box_624_1217_838_1328.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A48Z%2F-1%2F%2F2a6194056bb44d96e33492aa9ef8880fb9f6bf61edaa5cb439e2121e42402c4f" alt="Image" width="17%" />

4.点击“保存数据”按钮后，界面会弹出窗口“☐”

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_1/imgs/img_in_image_box_582_1345_707_1389.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A48Z%2F-1%2F%2F7a1367302ac14891a93dbb44cd5298dca7a5469fc7f1dc22afbf06d3f453cb29" alt="Image" width="10%" />

开始保存数据，按钮下方会显示进度条“___”，表示正在进行数据保存，由于数据传输量较大，保存过程中软件和机器均不能进行其他操作，需等待片刻；当进度条跑到100%后会消失不显示，表示已经保存好buffer采集的数据，此时会在当前软件运行目录下生成一个excel表格，文件名为Buffer_Data.xls。点击“清除数据”按钮后，会清空四个数据缓存区，并清空当前软件运行目录下的 excel 表格的数据。

##### 6.2.5.2 系统设置读取与存储

该项参数的软件配置区域如图 65 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_2/imgs/img_in_image_box_179_299_1011_763.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A49Z%2F-1%2F%2Fb1ab564850594868acf0e66ad7cc5ed00bf6d11581e9be04f564a286786fec85" alt="Image" width="69%" />

图 66. 数据缓存区配置区域图

可供用户配置的选项如下表 13:

表 13. 数据缓存区配置选项表

| System Setting系统设置 | 可选默认设置以及用户自定义的设置 1、设置 2、设置 3 和设置 4 |
| --- | --- |
| Save Setting保存设置 | 把当前设置保存到当前用户选定的系统设置中, 可保存在设置 1、设置 2、设置 3 或设置 4 |
| Recall Setting读取设置 | 恢复为用户选定的系统设置。恢复为默认设置为仪器的出厂设置，设置 1、设置 2、设置 3 或设置 4 为用户自定义的设置 |

##### 注意：

当选择好需要的设置后，需要点击“读取采样”或“保存采样”才会生效。

##### 6.2.5.3 Channel Out 通道输出配置

该项参数的软件配置区域如图 67 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_3/imgs/img_in_image_box_179_155_1011_621.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A49Z%2F-1%2F%2Fdc6652b9e433f6366ab9a8ddb582bcb6d8133fbe9c6d9c4ec43d2219d157e196" alt="Image" width="69%" />

图 67. 输出通道的配置区域图

可供用户配置的选项如下表 14:

表 14. 输出通道配置选项表

| Channel 1 输出通道 1 | 可以控制后面板上输出通道 CH1 输出用户需要的数值，数值类型包括两个通道信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、噪声值以及计算公式 E1/E2/E3/E4 值 |
| --- | --- |
| Channel 2 输出通道 2 | 可以控制后面板上输出通道 CH2 输出用户需要的数值，数值类型包括两个通道信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、噪声值以及计算公式 E1/E2/E3/E4 值 |
| Offset(%) 偏置设置 | 可调范围是 -100% -- +100%，其中最小步进为 0.01%，默认 0.00%，只能对 R/X/Y 值进行设置 |
| Expand 放大设置 | 可调范围是 1~256，默认值为 1 |
| Speed 输出速度设置 | 可选择“快速/Fast”或者“慢速/Slow”。当选择“快速”时，输出通道 1 和 2 只能输出 R、X、Y 值。 |

##### 注意：

1. 当设置好 CH1 和 CH2 输出配置之后，点击“配置通道输出”，以完成各项参数的配置；全部参数都可同时更改。

2. 每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置通道输出}} $”按钮以完成配置，否则设置无效。

#### 6.2.6 正弦输出及公式配置

##### 正弦输出及公式配置如下图所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//f080f1ec-48ca-400c-8586-07c5be78abc0/markdown_4/imgs/img_in_image_box_179_254_1012_712.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A50Z%2F-1%2F%2Fd77bf489b2dfbd7ccc76a91c113903f57e587fc2fd334b1dbc02bf31abec2551" alt="Image" width="69%" />

图 68. 正弦输出及公式配置区域图

正弦输出及公式配置区域可以对正弦信号输出的幅值大小、输出模式以及系统内自定义计算公式进行配置，以下为各个部分的详细介绍。

##### 6.2.6.1 正弦信号输出配置

该项参数的软件配置区域如图 69 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//e77ffc91-6547-4dd0-99f8-fe69dedde323/markdown_0/imgs/img_in_image_box_180_229_1012_694.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A43Z%2F-1%2F%2Fcfacd667e171145081964518273a73c54027aa6dbba843197bc47ce9444d1e6b" alt="Image" width="69%" />

图 69. 正弦信号输出配置区域图

可供用户配置的选项如下表 15:

表 15. 正弦信号输出配置选项表

| SineOut Mode | Fixed定值正弦信号 |
| --- | --- |
| SineOut Mode | Linear线性扫幅模式 |
| SineOut Mode | Log对数扫幅模式 |
| Sine Out Voltage(Vrms)正弦幅值设置 | 当正弦信号输出模式选择定值正弦信号时可操作此项，由用户手动输入，电压值范围为 1 mVrms 至 5 Vrms，最小分辨率为 1 mVrms。 |
| SineOut Start Voltage(Vrms)扫描开始幅值设置 | 用于设定扫幅的开始值，由用户手动输入，电压值范围为 1 mVrms 至 5 Vrms，最小分辨率为 1 mVrms。 |
| SineOut Stop Voltage(Vrms)扫描截止幅值设置 | 用于设定扫幅的截止值，由用户手动输入，电压值范围为 1 mVrms 至 5 Vrms，最小分辨率为 1 mVrms。 |
| SineOut Sweep Step(Vrms)扫描步进幅值设置 | 用于设置每次步进的频率值，用户手动输入。线性扫幅：电压值范围为 1 mVrms 至 5 Vrms，最小分辨率为 1 mVrms；对数扫幅：电压范围为 0.001% 到 100%，最小分辨率为 0.001。 |
| SineOut Run Mode扫描运行模式设置 | Single单次扫频 |
| SineOut Run Mode扫描运行模式设置 | Loop |
|  | 循环扫频 |
|  | Stop 停止扫频 |
| Step Time(ms) | 用于设置每次步进的时间,用户手动输入,范围为1ms |
| 扫描步进时间设置 | 到100s，频率分辨率最小为1ms |

##### 注意：

1. 当设置好正弦信号的输出配置之后，点击“配置正弦输出”，以完成各项参数的配置；全部参数都可同时更改。

2. 每次重新修改其中的设置后，都需要点击“ $ \underline{\text{配置正弦输出}} $”按钮以完成配置，否则设置无效。

##### 6.2.6.2 自定义公式配置

该项参数的软件配置区域如图 70 红框内所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//e77ffc91-6547-4dd0-99f8-fe69dedde323/markdown_2/imgs/img_in_image_box_182_232_1011_693.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2F517386e86d4d5fafe70304e77824e8c820636dd19cff0ae4644c84f2010674cc" alt="Image" width="69%" />

图 70. 公式配置区域图

可供用户配置的选项如下表 16:

表 16. 公式配置选项表

| Equation 公式设置 | 公式的输出值可选择 E1/E2/E3/E4，输入的三个值可选择两个通道信号的 X/Y/R/ \theta 值、信号谐波的 X/Y/R/ \theta 值、四路辅助输入的信号值 ADC1/ADC2/ADC3/ADC4、噪声值、频率值以及参数 C1/C2 值 |
| --- | --- |
| C1 C1 参数设置 | 用于设定公式的可选输入，由用户手动输入，范围为 -10 至 +10，最小分辨率为 0.001 |
| C2 C2 参数设置 | 用于设定公式的可选输入，由用户手动输入，范围为 -10 至 +10，最小分辨率为 0.001 |

##### 注意：

1. 当设置好公式的输出配置之后，点击“ $ \underline{\text{配置公式}} $”，以完成各项参数的配置；全部参数都可同时更改。

2. 每次重新修改其中的设置后，都需要点击“配置公式”按钮以完成配置，否则设置无效。

#### 6.2.7 数据显示与保存

##### 6.2.7.1 测量值实时显示

打开软件默认左窗口为“数据显示”选项页，视图如下：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//e77ffc91-6547-4dd0-99f8-fe69dedde323/markdown_3/imgs/img_in_image_box_184_305_1009_761.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A45Z%2F-1%2F%2F720d9fac8f99208b27ef7f715023ab6ab75fd68610c6c01751866b12c133de3f" alt="Image" width="69%" />

图 71. 谐波波形显示图

此时左窗口实时地显示两个测量通道的基波、谐波 1、谐波 2、频率、噪声以及辅助数据采集的四通道 ADC 的测量值。

注：显示数据 m、u、n、f、p 分别表示  $ 10^{-3} $、 $ 10^{-6} $、 $ 10^{-9} $、 $ 10^{-12} $。

##### 6.2.7.2 数据保存

软件有数据记录保存的功能，可根据用户需要选择是否保存一段时间内的 OE1022D 采集到的数据。

保存的数据包括测量两个通道信号的 R、X、Y、 $ \theta $、频率和噪声的值；测量的两路谐波的 R、X、Y 和  $ \theta $ 的值；以及四路辅助输入的信号值。

选择是否存储数据的具体步骤如下：

1. 数据以 Excel 表格的形式保存，文件名为“Data_recorded_excel.xls”，保存在程序目录下。

2. 当软件运行时，点击图 72 红框内“保存数据”按钮，当按钮被按下并显示“保存中...”，表示正在保存当前采集的数据。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//e77ffc91-6547-4dd0-99f8-fe69dedde323/markdown_4/imgs/img_in_image_box_182_145_1011_610.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A46Z%2F-1%2F%2F41160d7ec5e0ff2cdf2b80cb3cb7dc9be6958e06d6321b0d786caacd0dd7321a" alt="Image" width="69%" />

图 72. 数据保存配置区域图

3. 再次按下“保存中...”按钮，按钮状态由“保存中...”重新变为“保存数据”，表示停止保存采集的数据。

4. 在“___”前显示和保存数据的采样率，输入范围为 0.1 s~100 s。

##### 6.2.7.3 波形图显示

在软件左窗口选择“波形图”选项页，视图如下：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_0/imgs/img_in_image_box_183_244_1010_695.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A57Z%2F-1%2F%2F4287f3aa241fb612b14de1b980467054612e04a7dcaafa61c5368fd0cf46d6b0" alt="Image" width="69%" />

图 73. 谐波波形显示图

此时左窗口中可以分别设置显示基波、谐波 1、谐波 2 的 XY 坐标图。对每一个选项可设置显示对应的 R、X、Y 和  $ \theta $ 值。

### 6.3 软件使用实例

本使用实例将简单演示如何使用 OE1022D PC 软件进行锁相放大器的参数配置以及 R、X、Y 和  $ \theta $ 值的观察和记录。

首先需要按照前面 6.2 的软件使用说明，成功连接 OE1022D 与 PC 机，然后就可以开始进行配置了。

假设用户需要进行以下锁相放大器设置，并进行数据采集与保存：

表 17. 实例配置表

| 输入信号类型 | 单端电压输入 |
| --- | --- |
| 输入信号大小 | 40mV |
| 输入耦合方式 | AC |
| 开启陷波器 | 不开启 |
| 动态储备 | Normal Noise |
| 满偏灵敏度 | 使用自动功能 Auto Sensitivity |
| 参考信号输入 | 使用外部参考，1000 Hz |
| 参考信号触发方式 | TTL 上升沿触发 |
| 移相角度 | 0 |
| 低通滤波器的时间常数 | 300 ms |
| 滤波器陡降 | 12 dB/oct |
| 数据采样率 | 125 Hz |
| 数据缓存区 1(buffer1) | X |
| 数据缓存区 2(buffer2) | Y |

要完成以上设置，具体操作步骤如下：

①首先根据表 17，在输入信号配置中选择输入信号类型、输入耦合方式和是否开启陷波器，其它选项默认，如下图 74 所示，最后需要点击“配置输入信号”，以完成输入信号的配置：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_2/imgs/img_in_image_box_181_157_1011_620.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A59Z%2F-1%2F%2F75b63dd8f5d65079080b2f47f32ac303654ed29f70cd2c11e185649ccba64ba2" alt="Image" width="69%" />

图 74. 输入信号配置图

②根据表 17，在参考信号配置区域选择参考信号源类型、外部参考和参考相位值，其它选项默认，如下图 75 所示，最后需要点击“配置参考信号”，以完成配置：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_2/imgs/img_in_image_box_186_775_1001_1225.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A02%3A59Z%2F-1%2F%2F06eda511ea6a9345ef94f607e86463835b874ae6dc58fc952a94603afb8c00dd" alt="Image" width="68%" />

图 75. 参考信号配置图

若不使用“自动相移”和“自动量程”来自动设置移相和显示刻度范围，则不需要点击相应的“关闭”按钮，保持不变即可。

若使用“自动相移”来自动设置移相角度，则点击相应的“关闭”按钮，当按钮被按下并显示“处理中...”，表示 OE1022D 正在执行自动移相功能，当执行完毕后，按钮会重新复位为初始状态“关闭”，此时的“Phase”一项会更新为 OE1022D 返回的移相角度值。

③根据表 17，在滤波器配置区域选择好时间常数和滤波器陡降，其它选项默认，如下

图 76 所示，最后需要点击“配置滤波器”，以完成配置：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_3/imgs/img_in_image_box_190_179_1001_630.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A00Z%2F-1%2F%2Ff09e0441f6a7e9f4b93aa01038c492eacc33646f0595ea33fdf5067bdbc87a31" alt="Image" width="68%" />

图 76. 滤波器配置图

若不使用同步滤波器，则不需要点击相应的“关闭”按钮，保持不变即可。

若使用同步滤波器，则点击相应的“同步滤波器关”按钮，当按钮被按下并显示

“同步滤波器开”，表示 OE1022D 已经开启了同步滤波器功能，当用户需要关闭同步滤波器

时，再次点击该按钮会重新复位为初始状态“同步滤波器关”，表示 OE1022D 已经关闭了同步滤波器功能。

④根据表 17，在动态储备和满偏灵敏度配置区域选择储备类型、灵敏度类型，其它选项默认，如下图 77 所示，最后需要点击“配置动态储备与灵敏度”，以完成配置；

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_4/imgs/img_in_image_box_181_157_1011_620.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A01Z%2F-1%2F%2F32fbc71fd212b0e81ad12326f9060952e24589f52f414d3ca19508cdb392dd50" alt="Image" width="69%" />

图 77. 动态储备和灵敏度配置图

另外，由于选择使用“自动灵敏度”来自动设置满偏灵敏度，只需要点击“自动灵敏度”下的“关闭”按钮，“该按钮会变为“ $ \underline{\text{处理中...}} $”，表示 OE1022D 正在执行自动功能，当执行完毕后，按钮会重新复位为初始状态“ $ \underline{\text{关闭}} $”，此时的“动态储备”和“灵敏度”一项会更新为 OE1022D 返回的值。

此时，再看主界面上的“Sensitivity”一项，对比图 77，发现 Sensitivity 已经改变为合适的灵敏度了（由 100 mV 变为 50 mV），如图 78 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//5ee7ece9-ce6c-443f-870d-6f64045813f6/markdown_4/imgs/img_in_image_box_182_907_1011_1371.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A03%3A01Z%2F-1%2F%2Facd35bdbab7de48f40cd70bdfadd594d7b9772e3a51f6b1be94be30442dfb738" alt="Image" width="69%" />

图 78. 自动灵敏度设置

把配置前后的 OE1022D 界面对比图如下两图所示，可见配置成功：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_0/imgs/img_in_image_box_274_155_914_625.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2Fda334a2ebd1ee23def0a06f551b5edf8a5c9f5b14a8510d54992df7b68cac4ef" alt="Image" width="53%" />

图 79. 配置前 OE1022D 界面-SENS 为 100 mV

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_0/imgs/img_in_image_box_275_717_914_1187.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A44Z%2F-1%2F%2F8a6387f572318bb1262cedc739374c36ea76855ef085571a3dcdd4de52a3c9c9" alt="Image" width="53%" />

图 80. 配置后 OE1022D 界面-SENS 为 50 mV

⑤以上步骤 1-4 已经根据要求配置完了 OE1022D，其它选项默认，此时可以开始进行数据的采集和保存了。

数据以 Excel 表格的形式保存在选定目录下，文件名为“Data_recorded_excel.xls”，共有 32 列数据。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_1/imgs/img_in_image_box_181_156_1011_621.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A45Z%2F-1%2F%2F63b15eff250ef45aee8965655eceb74f8566ad4e93fe89df576801d01dd6f18e" alt="Image" width="69%" />

图 81. 数据保存暂停中

如图 81 所示，点击红框内按钮“保存数据”，当按钮被按下并显示“保存中...”，表示正在保存当前采集的数据，如图 82 所示：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_1/imgs/img_in_image_box_181_811_1011_1277.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A45Z%2F-1%2F%2F1ac61e2a463f16d035735659248289724826969ce865544d5748524474117825" alt="Image" width="69%" />

图 82. 数据保存执行中

⑥除了步骤 5 中的保存数据方法, 还有一种是可以根据用户的需要设置不同的数据采样率, 把数据保存在 OE1022D 的四个数据缓存区内, 每个数据缓存区可以采集一种数据, 数据类型参见 6.2.5.1 章节。

数据以 Excel 表格的形式保存在当前软件运行的目录下, 文件名为 Data_recorded_excel.xls, 如图 83 所示:

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_2/imgs/img_in_image_box_186_151_1004_321.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A46Z%2F-1%2F%2Faa6ee342bdd7188c79c5b3e575dda54892983b03b57a3cde44511ab65ceed0ac" alt="Image" width="68%" />

图 83. 保存数据表格

具体操作如下（假设用户选择数据采样间隔为 50 ms，采样长度为 1000，缓存区 1~4 分别采集 R、X、Y 和  $ \theta $ 值）：

1. 在数据缓存区配置区域选择好每个缓存区的采集数据类型、采样间隔和采样长度，其它选项默认，然后点击“ $ \underline{\text{配置采样}} $”按钮，再点击右方的“ $ \underline{\text{开始采样}} $”按钮。当在按钮下方出现进度条时，表示开始进行数据采集，如图 84 红框内所示。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_2/imgs/img_in_image_box_178_594_1011_1060.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A46Z%2F-1%2F%2F6d050ebc0832ba43ad2296316206b92348f4633ead68dbe308eb1c107bdd8519" alt="Image" width="69%" />

图 84. 数据采集

2. 当进度条时跑到 100% 时，进度条会消失不显示，表示 OE1022D 已经完成数据采集，此时若想保存数据，则点击图 85 红框的“保存数据”按钮：

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_3/imgs/img_in_image_box_178_155_1011_620.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A46Z%2F-1%2F%2Fb68eb9a8b78ffc503c005bd528c04fba94797841aec3653c0daba6a5d220cb3a" alt="Image" width="69%" />

图 85. 保存采集数据

如下图所示，当点击“保存数据”后，按钮下方会显示进度条，表示正在进行数据保存；当进度条跑到100%后会消失不显示，表示已经保存好缓存区采集的数据，此时在软件运行的当前文件夹会生成上面讲过的四个excel文档。

<img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-15//b1fc91d8-db5a-4dfb-8d46-8c379743749e/markdown_3/imgs/img_in_image_box_179_811_1012_1276.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-04-30T20%3A01%3A46Z%2F-1%2F%2F69a8bcfba92da42d4f574298cce2cb4c25cee13054b4c39189a97dc9f23eedd1" alt="Image" width="69%" />

图 86. 采集数据保存中
