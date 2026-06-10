# 磁场控制系统逆向成果目录

> 本目录包含对 `SimplePowerController.exe` + `CControls.dll` 的完整逆向分析成果，供其他 Agent 直接使用。

---

## 目录索引

### 1. 核心文档

| 文件 | 说明 |
|---|---|
| `逆向分析报告-协议与算法还原.md` | **必读**。完整逆向报告：程序结构、SCPI 协议、磁场算法、数据格式、动态验证结果、Rust 实现草案。 |
| `磁场控制系统软件说明-文字版.md` | 原软件使用说明书（文字版），操作流程参考。 |

### 2. 反编译源码（C#）

位于 `decompiled/` 下，由 `ilspycmd -p` 导出的完整工程。

#### SimplePowerController（主程序）

| 文件 | 行数 | 核心内容 |
|---|---|---|
| `SimplePowerController/FormMain.cs` | ~1835 | **最重要**。串口通信、SCPI 指令发送、磁场算法、Timer 轮询、数据保存 |
| `SimplePowerController/FormComm.cs` | ~304 | COM 口配置对话框 |
| `SimplePowerController/FormParamSet.cs` | ~198 | 线圈常数设置对话框 |
| `SimplePowerController/FormSaveDirSet.cs` | ~239 | 数据保存路径/文件名/采样间隔设置 |
| `SimplePowerController/LocalSettingAccessor.cs` | ~107 | `para.xml` 读写封装 |
| `SimplePowerController/ErrorLogger.cs` | ~26 | 错误日志记录器 |
| `SimplePowerController/Program.cs` | ~17 | 程序入口 |

#### CControls（自定义控件库）

| 文件 | 说明 |
|---|---|
| `CControls/CButton.cs` | 自定义按钮 |
| `CControls/CLed.cs` | LED 指示灯 |
| `CControls/CToggleButton.cs` | 切换开关 |
| `CControls/CRegulator*.cs` | 数值调节器（4位/6位，有符号/无符号） |
| `CControls/SevenSegment*.cs` | 七段数码管显示 |

### 3. 配置与数据

| 文件 | 说明 |
|---|---|
| `para.xml` | 当前运行配置。串口：X=COM4, Y=COM6, Z=COM3；线圈常数：X=143.26, Y=141.77, Z=156.15（nT/mA） |
| `Errors/` | 历史错误日志（2024-04 至今），主要错误为"操作已超时"（COM 口配置错误导致） |

### 4. 工具脚本

| 文件 | 说明 |
|---|---|
| `scan_devices.py` | 扫描本机串口并发送 `*IDN?` 识别 Maynuo 设备，自动匹配 X/Y/Z 轴 |
| `verify_protocol.py` | **动态验证脚本**。对三轴电源发送完整 SCPI 指令序列（使用 10mA 安全电流），验证协议正确性。 |

---

## 快速上手

### 如果要做协议开发（Rust/Python）

直接阅读 `逆向分析报告-协议与算法还原.md` 第 2~3 章，报告末尾已附有 **Rust 实现草案**。协议要点：

- 串口：9600/8/N/1，DTR=true
- 初始化：`SYST:REM` → `VOLT 75` → `CURR 0` → `OUTP 0`
- 设流：`CURR {mA/1000:.5f}`（单位 A，保留5位小数）
- 开关：`OUTP 1` / `OUTP 0`
- 回读：`MEAS:CURR?` → 返回 A，需 ×1000 转 mA
- 断开：`CURR 0` → `OUTP 0` → `SYST:LOC`

### 如果要修改/扩展原软件

阅读 `decompiled/SimplePowerController/FormMain.cs`，核心逻辑全部在此。注意：
- 反编译工程可能需要手动修复才能重新编译（资源文件、编译器生成类名等）
- `POWER_MAX_CURR = 5000.0`（mA）是硬编码上限

### 如果要排查硬件连接问题

运行 `scan_devices.py` 检查设备串口映射，或查看 `Errors/` 最新日志。

---

## 硬件信息备忘

| 轴 | 串口 | 设备型号 | SN 尾号 |
|---|---|---|---|
| X | COM4 | Maynuo M8812 | 2020 |
| Y | COM6 | Maynuo M8812 | 2022 |
| Z | COM3 | Maynuo M8812 | 2003 |
