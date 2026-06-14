# ODMR Config Generator

跨 Mac / Windows 的离线配置生成器，用来生成一次实验需要的可选 JSON：

- `plan.json`
- `smb100a profile.json`
- `oe1022d profile.json`
- `cni_laser profile.json`

它不连接设备，不调用 C# runtime，不改 OE `RALL?` collector。生成后的 JSON 优先由
PySide6 console 的 Run Bundle 页面选择并运行；Tk 生成器和 WinForms 控制面板只保留为 fallback。

## Run

```bash
python3 tools/config-generator/odmr_config_generator.py
```

Windows 上也可以用：

```powershell
python tools\config-generator\odmr_config_generator.py
```

## Tests

```bash
python3 tools/config-generator/tests/test_config_core.py
```

## Boundary

- 生成器负责配置编辑和 JSON 输出。
- PySide6 console 负责选择 JSON、校验 bundle、启动 run 和查看 artifact 审查入口。
- SMB/OE/Laser 参数继续写在 profile JSON 中。
- 实验 step 写在 C# runtime 已支持的 `points[]` plan 中；磁场只是 point 的可选上下文。
- Python GUI 是单线程 Tkinter 工具：只做离线表单编辑和本地 JSON 文件写入，不连接设备，不启动采集，不引入后台 worker。
- 页面切换使用单内容区 frame stack，不使用隐藏 Notebook tab；快速切换只切换已构建页面，不重新加载配置文件。

## UI Order

配置器按实验配置顺序分页：

1. Templates / Output：选择源 JSON 模板和生成目录。
2. 实验计划：定义无磁场控制、零场/恒定磁场，或磁场扫描 block。
3. 计划策略：定义 Maynuo baseline/output 和 point quality 阈值。
4. SMB100A：定义固定调制 profile 和默认 RF sweep。
5. OE1022D：定义 fixed profile；collector `12288B / 30ms` 只校验不编辑。
6. CNI Laser：定义 run 级背景模式和功率。
7. Generate：生成 JSON，交给 PySide6 console 的 Run Bundle 页面选择运行。

界面使用分页和滚动表单，避免把不同设备的大量字段挤在同一页。

## Plan / Point Semantics

当前 runtime 里 `point` 表示一次采集 step，而不再等同于“磁场点”。

- `magnetic_mode=none`：无磁场控制；不指挥 M8812，不做 baseline lock，`target_b_nt` 不存在。
- `magnetic_mode=controlled`：受控磁场；必须有 `target_b_nt`，运行时按 calibration 计算电流并执行 M8812 readback。
- 零场是 controlled single-point `[0,0,0]`，不是无磁场控制。
- 恒定磁场是 controlled single-point `[x,y,z]`。
- 1D/2D/3D 扫描是 controlled multi-point。

生成器不新增 run bundle schema，也不引入 `field_space.groups`；最终仍写现有 C# runtime 可读的 `plan.json` 和三个 profile JSON。

## Magnetic Field Flow

磁场页只负责生成目标磁场点，不直接写 M8812 电流，也不直接复刻原厂 exe 的控件状态。

原厂反编译资料位置：

- `reverse_application/reverse_output/逆向分析报告-协议与算法还原.md`
- `reverse_application/reverse_output/decompiled/SimplePowerController/SimplePowerController/FormMain.cs`
- `docs/rebuild/12_原厂锁零事实与边界.md`

从这些资料能确认的事实是：

- 原厂 `LockZero` 不是物理磁场闭环，而是零偏电流锁定。
- 原厂输出模型是 `zeroSetCurr + recurSetCurr`。
- 原厂磁场显示来自线圈常数换算：`Mag(nT)=Curr(mA)*CoilConstant(nT/mA)`，`Curr(mA)=Mag(nT)/CoilConstant(nT/mA)`。
- 原厂验证手段是 M8812 `MEAS:CURR?` 电流回读，不是外部磁场传感器回读。

这些值在 rebuild 里的对应位置：

- 目标磁场点：controlled point 生成到 plan JSON 的 `points[].target_b_nt`，单位固定为 `nT`。
- 磁场到电流的换算：`configs/calibrations/main.json` 的 `current_offset_a` 和 `current_per_nt`。
  默认主 calibration 应与 `reverse_application/reverse_output/para.xml` 的三轴 `CoilConstant(nT/mA)` 一致，即使用其倒数并换成 `A/nT`。
- 零偏电流策略：plan JSON 的 `mag_baseline_policy.baseline_current_a`、`settle_ms`、`readback_samples`、`settle_tolerance_a`。
- 运行时换算代码：`tools/win-csharp/Odmr.Runtime/RunConfig.cs` 的 `CalibrationProfile.TargetCurrentA(...)`。
- 运行时执行链：`tools/win-csharp/Odmr.Runtime/ConfigDrivenRun.cs` 中 point loop 的 `target_b_nt -> target_current_a -> M8812 SetCurrent -> MEAS:CURR?`。
- 运行产物审查：`baseline_snapshot.json` 记录锁定后的 `locked_zero_offset_current_a`，`device_state.jsonl` 记录每个 point 的 intended target/current、measured current、segment 和 RF exposure。

因此磁场页的流程是：

```text
UI 扫描块
  -> 展开为 plan.points[].target_b_nt + magnetic_mode=controlled
  -> C# run-execute 读取 station + calibration + plan + profiles
  -> LockBaseline 读取/锁定零偏电流
  -> target_b_nt 通过 calibration 计算 delta/target current
  -> M8812 下发目标电流并用 MEAS:CURR? 回读
  -> device_state.jsonl / baseline_snapshot.json / points.jsonl 落盘审查
```

这条链路表达的是“目标磁场设定通过校准换算成电流并被设备回读确认”，不是“物理零磁场已经闭环证明”。

无磁场控制模式会跳过上述 M8812 链路，但仍保留 point/segment/quality/device_state artifact，用于绑定 RF sweep 和 OE RALL 采集窗口。

## Device Options

设备命令枚举不允许随手输入字符串：

- SMB100A 的 token 下拉来自仓库中的 R&S 命令真值和手册摘录：
  - `FM:SOUR`: `INT` / `EXT` / `INT,EXT`
  - `FM:MODE`: `NORM` / `LNO` / `HDEV`
  - `LFO:SHAP`: `SINE` / `SQU` / `TRI` / `SAWT` / `ISAW`
  - `SOUR:LFO:SIMP`: `LOW` / `G600`
  - `SWE:MODE`: `AUTO` / `MAN` / `STEP`
  - `SWE:SPAC`: `LIN` / `LOG`
  - `SWE:SHAP`: `SAWT` / `TRI`
  - `TRIG:FSW:SOUR`: `AUTO` / `SING` / `EXT`
- OE1022D 的 `j` 编码下拉来自 `docs/equipment_manual/oe1022d/校对后的oe1022d面板基础设置/` 和当前已验证 profile。
  UI 显示为 `code - meaning`，生成 JSON 时仍写回整数 code，例如 `24 - 100 mV/nA` 会写成 `"sensitivity_index": 24`。
- OE `reference_slope=2` 是当前 LabVIEW 锁定态 profile 中的已观测读回值；手册 V1.5 只列出 `0/1`，所以 UI 明确标注为 observed LabVIEW locked readback。
- 仍然保留自由输入的字段只限连续数值或实验身份字段，例如频率、功率、电流、settle 时间、run id、operator。

## Units

界面把单位选择放在对应数值输入框右侧，不再集中放在页面开头。改变单位时，当前数值会按物理量自动换算，例如 `2.83 GHz` 切到 `MHz` 会显示为 `2830`。写入 JSON 时仍统一回到 C# runtime 的 canonical units：

- Magnetic field: UI 统一使用 `nT`，JSON 写 `target_b_nt`，单位固定为 `nT`。
- M8812 current: UI 可选 `A/mA`，JSON 写 `baseline_current_a` 和 `settle_tolerance_a`，单位固定为 `A`。
- Voltage: UI 可选 `V/mV`，plan JSON 写 `voltage_v` / `voltage_protection_v`，单位固定为 `V`。
- Time: UI 可选 `ms/s`，plan/profile JSON 中的 settle、dwell、quality age 等字段固定写 `ms`。
- SMB RF start/stop: UI 可选 `Hz/kHz/MHz/GHz`，profile JSON 写 `start_hz` / `stop_hz`，单位固定为 `Hz`。
- SMB RF step: UI 可选 `Hz/kHz/MHz/GHz`，profile JSON 写 `step_hz`，单位固定为 `Hz`。
- SMB FM deviation and LF frequency: UI 可选频率单位，profile JSON 固定写 `Hz`。
- SMB LF voltage: UI 可选 `mV/V`，profile JSON 写 `lf_voltage_mv`，单位固定为 `mV`。
- Laser power: UI 可选 `mW/W`，profile JSON 写 `power_mw`，单位固定为 `mW`。

原则是：UI 方便输入，JSON 保持现有 schema 和单位，不让 C# runtime 或 artifact contract 发生变化。
