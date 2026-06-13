# ODMR Config Generator

跨 Mac / Windows 的离线配置生成器，用来生成一次实验需要的可选 JSON：

- `plan.json`
- `smb100a profile.json`
- `oe1022d profile.json`
- `cni_laser profile.json`

它不连接设备，不调用 C# runtime，不改 OE `RALL?` collector。生成后的 JSON 由
Windows C# Run Bundle 控制面板选择并运行。

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
- C# WinForms 负责选择 JSON、校验 bundle、启动 run。
- SMB/OE/Laser 参数继续写在 profile JSON 中。
- 磁场扫描点写在 C# runtime 已支持的 `points[]` plan 中。
- Python GUI 是单线程 Tkinter 工具：只做离线表单编辑和本地 JSON 文件写入，不连接设备，不启动采集，不引入后台 worker。

## UI Order

配置器按实验配置顺序分页：

1. Templates / Output：选择源 JSON 模板和生成目录。
2. Magnetic Plan：定义磁场扫描 block。
3. Plan Policy：定义 Maynuo baseline/output 和 point quality 阈值。
4. SMB100A：定义固定调制 profile 和默认 RF sweep。
5. OE1022D：定义 fixed profile；collector `12288B / 30ms` 只校验不编辑。
6. CNI Laser：定义 run 级背景模式和功率。
7. Generate：生成 JSON，交给 C# Run Bundle 控制面板选择运行。

界面使用分页和滚动表单，避免把不同设备的大量字段挤在同一页。

## Units

界面允许选择输入单位，但写入 JSON 时统一回到 C# runtime 的 canonical units：

- Magnetic field: UI 可选 `nT/uT/mT`，JSON 写 `target_b_nt`，单位固定为 `nT`。
- M8812 current: UI 可选 `A/mA`，JSON 写 `baseline_current_a` 和 `settle_tolerance_a`，单位固定为 `A`。
- Voltage: UI 可选 `V/mV`，plan JSON 写 `voltage_v` / `voltage_protection_v`，单位固定为 `V`。
- Time: UI 可选 `ms/s`，plan/profile JSON 中的 settle、dwell、quality age 等字段固定写 `ms`。
- SMB RF start/stop: UI 可选 `Hz/kHz/MHz/GHz`，profile JSON 写 `start_hz` / `stop_hz`，单位固定为 `Hz`。
- SMB RF step: UI 可选 `Hz/kHz/MHz/GHz`，profile JSON 写 `step_hz`，单位固定为 `Hz`。
- SMB FM deviation and LF frequency: UI 可选频率单位，profile JSON 固定写 `Hz`。
- SMB LF voltage: UI 可选 `mV/V`，profile JSON 写 `lf_voltage_mv`，单位固定为 `mV`。
- Laser power: UI 可选 `mW/W`，profile JSON 写 `power_mw`，单位固定为 `mW`。

原则是：UI 方便输入，JSON 保持现有 schema 和单位，不让 C# runtime 或 artifact contract 发生变化。
