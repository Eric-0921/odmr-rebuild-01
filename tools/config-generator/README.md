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
