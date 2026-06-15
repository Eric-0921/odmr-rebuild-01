# 7. 操作实例

## 7.1 串口通讯

本实例将演示 OE1300 远程控制串口环境搭建以及调试操作，你需要准备一条 RS232 转 USB 线，步骤如下：

1. 请用 USB 线连接 OE1300 的 USB 插口跟电脑上的任一 USB 插口。

2. 电脑会自动识别到 USB 设备，然后提示安装驱动程序。如果电脑操作系统为 WIN 7/8/8.1/10 系统，系统一般就会自动在网络上搜索驱动程序并自动安装，这个过程需要等待一段时间。如果安装失败（电脑没有连接网络会导致失败）就需要手动去安装 USB 的驱动，安装细节请参考第 5 章节。

3. 打开 U 盘文件中串口调试工具文件夹，双击 `UartAssist.exe` 文件，弹出软件界面如 $ \underline{图61} $：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//ce774750-7926-44b6-81ee-740c3e0330a2/markdown_4/imgs/img_in_image_box_181_606_1008_1393.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fde270e865b0db6f9526c68bc3dfd346358f7be0a7386d0b9e96969b6cacaa940" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 61. 打开的软件界面</div> </div>


该串口调试软件包含了通讯设置，接收区设置，发送区设置，接收区，以及发送区。OE1300 默认波特率为 115200，校验位无，数据位 8 位，停止位 1 位（OE1300 的波特率及校验位等可通过串口调试助手后续进行修改设置）。

串口号需要选择电脑为 OE1300 USB 接口自动分配的 COM 口，COM 端口编号可通过设备管理器中的端口（COM 和 LPT）选项来查看（计算机右键->属性->设备管理器->端口），如 $ \underline{\text{图62}} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_0/imgs/img_in_image_box_271_311_917_959.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2F2de81f38b1a0fb25e8c0f87bf45f60629594a42228355f4478e28493a1a9d96a" alt="Image" width="54%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 62. 端口号的查看</div> </div>


当配置好了端口号、波特率、校验位、数据位、停止位之后，如果连接按钮左边小圆圈为黑色熄灭状态（☑ 打开），需要点击一次改变按钮状态显示为红色点亮状态（☑ 断开），如果按钮为红色点亮状态就表明电脑跟当前串口号设备已连接成功，若多次点击连接不成功，请检查端口号是否选择合适，然后再尝试连接。连接成功如图63所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_1/imgs/img_in_image_box_181_154_1008_939.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F331b9e2872f6b240564fb48466055f5606287a927b19cff4c34728181fdeb460" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 63. 连接成功的状态</div> </div>


4. 完成以上操作之后，即可向 OE1300 发送指令来进行通讯：

OE1300 指令要求格式是四个大写字母助记符后加选项参数，例如指令 `ISRC 0` + 回车符（`0D`）或 `ISRC ?` + 回车符（`0D`），连续多条的指令可以用 `;` 号分隔开，指令结尾一定要附加上回车符或十六进制数 `0D`，更多详细指令请查看远程编程章节的介绍。

需要特别注意的是指令结尾一定要附加上回车符或十六进制数 `0D` 才会有效执行当前指令。发送指令时首先在发送区敲入指令，然后紧接着敲一下回车，最后点击发送按钮，指令就会发送出去。如 $ \underline{图64} $、 $ \underline{图65} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_2/imgs/img_in_image_box_181_154_1003_938.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F32268b2c9a0acf8bc9b9731ec15c711030dc8387edcd2f593763dd87be26db11" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 64. ASCII 码形式发送和接收指令</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_3/imgs/img_in_image_box_182_155_1008_940.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fdfe6c7adfed05a41bd4707fe6208091f5e39248fbd68c0b7b7da782fda4556b2" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 65. 十六进制格式发送和接收指令</div> </div>


同时的，串口调试助手可配置自动添加发送回车符 `0X0D`。勾选发送区设置的“自动发送附加位”选项，在弹出的附加位设置窗口选择固定位，附加值设置为十六进制值 `0D` 即可。配置如 $ \underline{66} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//3e44ec5c-a267-48ae-9872-a236ad32b643/markdown_4/imgs/img_in_image_box_182_248_1009_1034.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fcbfb2ca538ac535da7dde2a4c21a11d17ed8dee0157493c707eb37a1430b3826" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 66. 附加位的设置</div> </div>


多个指令的发送需要添加 `;` 号来分隔开，例如发送指令 `ISRC?;ISRC1;ISRC?;FMOD?;FREQ?` 效果如 $ \underline{图67} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_0/imgs/img_in_image_box_181_216_1009_1002.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A53Z%2F-1%2F%2Fc18ca60dd2ab189a7d522f0ab18adabce5acd5dac22aecbc2bcd86524bfa5367" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 67. 多重指令的执行</div> </div>


连续读取 OE1300 的 X、Y、R、$ \theta $ 和 Freq 值，可以设置串口调试助手软件的间隔发送，配置如 $ \underline{图68} $、 $ \underline{图69} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_1/imgs/img_in_image_box_182_216_1010_1003.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F218d29745e8bf6a3994ae494b7f99e97eb82a3cfb6eae2b1d42a1f2d51dfbafa" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 68. 连续读取单个 R 值</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_2/imgs/img_in_image_box_199_154_985_878.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2F1534cea4fbaac82a08978911fb23e188f73e16d1adf72d7ee6132ffcdca01a18" alt="Image" width="65%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 69. 连续读取 X、Y、R、 $ \theta $ 及 Freq 值</div> </div>


通过串口调试助手远程控制发送指令设置 OE1300 内部参数时，仅会保存在 OE1300 自身，当重新连接 Console 会重新根据 Console 设置进行修改。

OE1300 不只是单一兼容以上这款串口调试助手的远程控制，现在网络上许多的串口调试工具都能很好的兼容，操作步骤也基本类似。

## 7.2 网口通讯

本实例将演示 OE1300 远程控制网口 TCP 模式环境搭建以及调试操作，你需要准备一条网线，步骤如下：

请用网线一端连接 OE1300 的网口，网线另外一端连接电脑主机后面上的网口。

1. 电脑会自动识别到以太网，打开系统设置，点击网络和 Internet 连接选项，点击更改适配器选项。如 $ \underline{图70} $显示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_3/imgs/img_in_image_box_178_380_1013_842.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F57eb7cc1dc852d061c28022722cf676241b7b2b62d80e8c2e34567269983e50d" alt="Image" width="70%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 70. 打开的软件界面</div> </div>


2. 双击未识别的网络，点击属性，如 $ \underline{图71} $所示，然后双击 Internet 协议版本 4（TCP/IPv4），弹出 $ \underline{图72} $，选择使用下面的 IP 地址，手动输入 IP 地址，子网掩码，默认网关。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_4/imgs/img_in_image_box_179_151_986_720.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Facccf25e74bd461c2dccb6eccc7cc0e84c1e4223cc353ac22521296e434c3d10" alt="Image" width="67%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 71. 打开的软件界面</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//4382ecdd-ea5b-495b-ac24-78ddb1d702ce/markdown_4/imgs/img_in_image_box_334_789_856_1427.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fe6d9915ca5e66a7f4eca0fa778b9c912fc6302bacb0c9a083911ea21da54725b" alt="Image" width="43%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 72. 打开的软件界面</div> </div>


点击确定，在 $ \underline{图73} $中看到已接受的字节在变化，说明已经连接成功。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_0/imgs/img_in_image_box_322_226_862_968.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F587305d7ff02503fdae886c91a5cbb7f4192bde59ac2cdcf8b7d128b635b7e53" alt="Image" width="45%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 73. 打开的软件界面</div> </div>


3. 打开 U 盘文件中串口调试工具文件夹，双击 `NetAssist.exe` 文件，弹出软件界面如图 74：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_1/imgs/img_in_image_box_180_156_1009_874.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2F3adadb1485eaab7736349cc54098d889163fb53cee08bf16f661753c405eef6d" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 74. 连接成功的状态</div> </div>


该网络调试软件包含了通讯设置，接收区设置，发送区设置，接收区，以及发送区。

协议类型选择 TCP Client, OE1300 默认 IP 地址为 `192.168.1.1`，远程主机端口固定为 `10001`（OE1300 的 IP 地址等可通过串口调试助手后续进行修改设置）。

当配置好了协议类型、IP 地址、远程主机端口之后，如果连接按钮左边小圆圈为黑色熄灭状态（☑ 打开），需要点击一次改变按钮状态显示为红色点亮状态（ $ \uwave{\text{断开}} $），如果按钮为红色点亮状态就表明电脑跟当前串口号设备已连接成功，若多次点击连接不成功，请检查网络是否连接正确，然后再尝试连接。连接成功如 $ \underline{\text{图74}} $所示。

4. 完成以上操作之后，即可向 OE1300 发送指令来进行通讯：

OE1300 指令要求格式是四个大写字母助记符后加选项参数，例如指令 `ISRC 0` + 回车符（`0D`）或 `ISRC ?` + 回车符（`0D`），连续多条的指令可以用 `;` 号分隔开，指令结尾一定要附加上回车符或十六进制数 `0D`，更多详细指令请查看远程编程章节的介绍。

需要特别注意的是指令结尾一定要附加上回车符或十六进制数 `0D` 才会有效执行当前指令。发送指令时首先在发送区敲入指令，然后紧接着敲一下回车，最后点击发送按钮，指令就会发送出去。如 $ \underline{图75} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_2/imgs/img_in_image_box_180_154_1010_875.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A02Z%2F-1%2F%2Fd3f28d8403e3a4d97016a90917b03151ad2411dff1b003c509ce0b0c1ad2eeb6" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 75. ASCII 码形式发送和接收指令</div> </div>


<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_3/imgs/img_in_image_box_180_155_1010_876.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fa6d4711c264e13ce9005dd97e3ecca97ce48014c8f62e11536a4831fdc31ebc7" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 76. 十六进制格式发送和接收指令</div> </div>


多个指令的发送需要添加 `;` 号来分隔开，例如发送指令 `*IDN?;ISRC?;FREQ?` 效果如 $ \underline{图77} $所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//9506bbd8-2b99-4a34-8308-ce5aa04b77b9/markdown_4/imgs/img_in_image_box_180_216_1009_937.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A03Z%2F-1%2F%2Fdc8ae09e07c9c3ba31f657e1992a3481bcc1813438d6c60a91e07119a84bb9db" alt="Image" width="69%" /></div>


<div style="text-align: center;"><div style="text-align: center;">图 77. 多重指令的执行</div> </div>


通过网络调试助手远程控制发送指令设置 OE1300 内部参数时，仅会保存在 OE1300 自身，当重新连接 Console 会重新根据 Console 设置进行修改。
