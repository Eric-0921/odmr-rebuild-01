# 6. PC 软件安装使用说明

## 6.1 软件驱动安装

我们一般都是以 U 盘的形式把 PC 机软件提供给用户的，均可在 Android、Linux、Mac、Windows 系统上运行。打开 U 盘后有如下文件，如图 17 所示：

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_285_415_893_604.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fdba5ea9bf3543641c0ae615d1fd1a0cb8106dea0e0e213b5105a498ba0bc9595" alt="Image" width="51%" /></div>

<div style="text-align: center;">图 17. U 盘内 PC 软件包</div>

驱动安装步骤如下：

1. 打开 U 盘中的第 3 个文件夹 `串口驱动`，如图 18 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_262_775_856_937.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fc4f0c73c00d2e14464e92384b0e79bcb1d31ff1008b039b0b701dbc6ac8ff1a5" alt="Image" width="49%" /></div>

   <div style="text-align: center;">图 18. “串口驱动”文件夹</div>

2. 选择并打开对应系统的文件夹（以 Windows 为例），进入底层文件夹，如图 19 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_1/imgs/img_in_image_box_239_1067_953_1184.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A54Z%2F-1%2F%2Fea6466a13834ff600971f81cd23307b48ec06a3b1c5daf9cb8e77b1537245c41" alt="Image" width="59%" /></div>

   <div style="text-align: center;">图 19. 串口驱动-Windows 系统文件的底层文件</div>

3. 右键 `以管理员身份运行` 图 19 红色方框内的 `PL2300-M_LogoDriver_Setup.exe` 文件，等待进度条加载完成后会弹出如图 20 的软件窗口，点击 `Next`，等待几分钟后出现图 21 所示界面，按下 `Finish` 完成安装。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_2/imgs/img_in_image_box_276_153_914_628.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F52cbc5ddb8e6f16e25cc942227cb4bcc59525cf4616703dd30b82dba91899591" alt="Image" width="53%" /></div>

   <div style="text-align: center;">图 20. PL2300-M_LogoDriver 安装界面</div>

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_2/imgs/img_in_image_box_276_698_913_1177.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F59133e79ae36db8082a1f7e806342e6ae2ff26494ea4f33687eaf567343027b6" alt="Image" width="53%" /></div>

   <div style="text-align: center;">图 21. PL2300-M_LogoDriver 安装完成提示</div>

4. 此时，使用 USB 线连接 PC 机和锁相放大器，即可自动识别并连接成功。

> **注意：**
> 1. 如果 PC 机已经联网，当插上 USB 连接 PC 与锁相放大器时，会自动联网搜索驱动并进行安装。
> 2. 如果 PC 机已安装有串口转 USB 的驱动，则可跳过该步骤。

## 6.2 软件 Console 安装

若前面的安装步骤都确定没有问题后，用户则可打开图 13 中的第 1 个文件夹 `Console`，有如下文件，如图 22 所示。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_262_292_855_454.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fc7fd599265a38d20590a07770c4b305f20ada38bb8226d92f610b1b55598eae1" alt="Image" width="49%" /></div>

<div style="text-align: center;">图 22. Console-Windows 系统文件的底层文件夹</div>

Console 安装步骤如下：

1. 选择并打开对应系统的文件夹（以 Windows 为例），进入底层文件夹，如图 23 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_211_586_910_671.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2Fa97becdb4d7827f495beda06b14121f18ebaf72ba705dbb4a7fd1c368f77cf77" alt="Image" width="58%" /></div>

   <div style="text-align: center;">图 23. Consol-Windows 文件夹的底层文件</div>

2. 右键 `以管理员身份运行` 图 23 红色方框内的 `LIA_Console_V1.3.12.221025.exe` 文件，会弹出如图 24 的软件安装窗口，点击 `下一步（Next）` 按钮。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_3/imgs/img_in_image_box_179_874_1011_1248.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A55Z%2F-1%2F%2F623b8c2f1fb0d6c16725e122b722bb82ca17ad339fbfdffc497783aa18ff8daf" alt="Image" width="69%" /></div>

   <div style="text-align: center;">图 24. LIA_Console 安装界面</div>

3. 在选择安装目录界面中单击 `下一步（Next）` 按钮，如图 25 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_4/imgs/img_in_image_box_179_159_1012_527.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A56Z%2F-1%2F%2F27de429500ca2fff6ef23d0b73d542e0f4296e74fda278fa076f46d80f877f85" alt="Image" width="69%" /></div>

   <div style="text-align: center;">图 25. LIA_Console 安装程序界面</div>

4. 在安装组件界面中单击 `下一步（Next）` 按钮，如图 26 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//8ad222ac-d03f-4247-b7d0-ddda3dc392ba/markdown_4/imgs/img_in_image_box_178_671_1011_1044.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A58%3A56Z%2F-1%2F%2F1fc5efbb05800ed4f14822899d5608cd9064a6623ac1fe2a76dab300754956e6" alt="Image" width="69%" /></div>

   <div style="text-align: center;">图 26. LIA_Console 组件安装界面</div>

5. 在开始菜单快捷方式界面中单击 `下一步（Next）` 按钮，进入准备安装界面，然后单击 `安装（Install）` 按钮，如图 28 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_0/imgs/img_in_image_box_176_204_1013_631.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Ff62b9d9476c7ca5168681ada06030c1fc21197425c170f6e93adab307ab538df" alt="Image" width="70%" /></div>

   <div style="text-align: center;">图 27. LIA_Console 开始菜单快捷方式界面</div>

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_0/imgs/img_in_image_box_180_808_1011_1184.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A00Z%2F-1%2F%2Fc9e708b4669850b5deb431d3d9397553710c81b555d51d3608e7d49609ed0c5d" alt="Image" width="69%" /></div>

   <div style="text-align: center;">图 28. 准备安装界面</div>

6. 此时会弹出如图 28 所示的软件窗口，见到图 29 所示界面时表示正在安装 LIA_Console 上位机软件，只需等待几分钟即可。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_1/imgs/img_in_image_box_178_280_1012_654.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2F1f8422c89e08eb673ade140df442a3e119e7d2b292adf0555f5be187ecea4f43" alt="Image" width="70%" /></div>

   <div style="text-align: center;">图 29. 正在安装 LIA_Console</div>

7. 在已完成界面中单击 `完成（finish）` 按钮，如图 30 所示。

   <div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_1/imgs/img_in_image_box_176_782_1012_1219.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fba9f2022577675f3bd400c25be29f6423a7370681450e3a8a25ba724ddfd5bfd" alt="Image" width="70%" /></div>

   <div style="text-align: center;">图 30. 完成安装</div>

安装上位机程序时会在以下路径中创建一个 Windows 开始菜单项：`开始菜单 → LIA_Console → LIA_Console`。此链接将打开图 31 中显示的 LIA_Console 上位机程序。

<div style="text-align: center;"><img src="https://pplines-online.bj.bcebos.com/deploy/official/paddleocr/pp-ocr-vl-16-online//c53b1136-6684-4e58-b74a-f9c7a289209f/markdown_2/imgs/img_in_image_box_180_287_1011_753.jpg?authorization=bce-auth-v1%2FALTAKDN8mY5KlNI7zaRpLmOqrw%2F2026-06-15T13%3A59%3A01Z%2F-1%2F%2Fd61806ca6c4735a148202b41d2b958bc8fac9ce9abd50d0a196cd889370059a9" alt="Image" width="69%" /></div>

<div style="text-align: center;">图 31. LIA_Console</div>
