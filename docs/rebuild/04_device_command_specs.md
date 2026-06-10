# 设备命令规格

第一版命令层采用白名单策略。命令 helper 只负责生成命令，不负责 transport、session、runtime、状态机或业务语义。

## 通用规则

- runtime 只能发送本文允许的命令或后续审查加入白名单的命令。
- 不允许业务层直接拼接裸命令字符串。
- set 命令必须尽量有 readback。
- 状态改变命令必须写入 `events.jsonl`。
- 设备错误队列非空时，不允许把 point 标为成功。
- 未真机确认的命令不得进入 runtime。

验证状态固定枚举：

- `rebuild_smoke_verified`
- `legacy_verified_not_rechecked`
- `allowed_not_yet_verified`

## SMB100A 第一版命令

用途：RF sweep 源。

允许命令：

| 动作 | Helper | 命令形态 | 类别 | 验证状态 |
| --- | --- | --- | --- | --- |
| 查询身份 | `smb100a_query_idn` | `*IDN?` | read_only | `rebuild_smoke_verified` |
| 查询错误 | `smb100a_query_error_next` | `SYST:ERR?` | read_only | `rebuild_smoke_verified` |
| 查询操作完成 | `smb100a_query_operation_complete` | `*OPC?` | read_only | `allowed_not_yet_verified` |
| 设置输出 | `smb100a_set_output` | `OUTP ON/OFF` | operational | `rebuild_smoke_verified` |
| 查询输出 | `smb100a_query_output` | `OUTP?` | read_only | `rebuild_smoke_verified` |
| 设置频率 | `smb100a_set_frequency_hz` | `FREQ <hz>` | operational | `legacy_verified_not_rechecked` |
| 设置频率模式 | `smb100a_set_frequency_mode` | `FREQ:MODE CW|FIX|SWE` | operational | `rebuild_smoke_verified` |
| 查询频率 | `smb100a_query_frequency` | `FREQ?` | read_only | `rebuild_smoke_verified` |
| 设置功率 | `smb100a_set_power_dbm` | `POW <dbm>dBm` | operational | `rebuild_smoke_verified` |
| 查询功率 | `smb100a_query_power` | `POW?` | read_only | `rebuild_smoke_verified` |
| 设置扫频起点 | `smb100a_set_sweep_start_hz` | `FREQ:STAR <hz>Hz` | operational | `rebuild_smoke_verified` |
| 查询扫频起点 | `smb100a_query_sweep_start` | `FREQ:STAR?` | read_only | `allowed_not_yet_verified` |
| 设置扫频终点 | `smb100a_set_sweep_stop_hz` | `FREQ:STOP <hz>Hz` | operational | `rebuild_smoke_verified` |
| 查询扫频终点 | `smb100a_query_sweep_stop` | `FREQ:STOP?` | read_only | `allowed_not_yet_verified` |
| 设置扫频步进 | `smb100a_set_sweep_step_hz` | `SWE:FREQ:STEP <hz>Hz` | operational | `rebuild_smoke_verified` |
| 查询扫频步进 | `smb100a_query_sweep_step` | `SWE:FREQ:STEP?` | read_only | `rebuild_smoke_verified` |
| 设置扫频驻留 | `smb100a_set_sweep_dwell_ms` | `SWE:FREQ:DWEL <ms>ms` | operational | `rebuild_smoke_verified` |
| 查询扫频驻留 | `smb100a_query_sweep_dwell` | `SWE:FREQ:DWEL?` | read_only | `rebuild_smoke_verified` |
| 设置扫频模式 | `smb100a_set_sweep_mode` | `SWE:MODE <mode>` | operational | `rebuild_smoke_verified` |
| 查询扫频模式 | `smb100a_query_sweep_mode` | `SWE:MODE?` | read_only | `allowed_not_yet_verified` |
| 设置扫频形状 | `smb100a_set_sweep_shape` | `SWE:SHAP <shape>` | operational | `rebuild_smoke_verified` |
| 设置扫频间隔 | `smb100a_set_sweep_spacing` | `SWE:SPAC <spacing>` | operational | `rebuild_smoke_verified` |
| 设置触发源 | `smb100a_set_sweep_trigger_source` | `TRIG:FSW:SOUR <source>` | operational | `rebuild_smoke_verified` |
| 查询触发源 | `smb100a_query_sweep_trigger_source` | `TRIG:FSW:SOUR?` | read_only | `allowed_not_yet_verified` |
| 设置扫频输出起点电压 | `smb100a_set_sweep_output_voltage_start_v` | `SWE:OVOL:STAR <value>` | operational | `rebuild_smoke_verified` |
| 设置扫频输出终点电压 | `smb100a_set_sweep_output_voltage_stop_v` | `SWE:OVOL:STOP <value>` | operational | `rebuild_smoke_verified` |
| 执行扫频 | `smb100a_execute_frequency_sweep` | `SWE:FREQ:EXEC` | operational | `rebuild_smoke_verified` |

运行前必须 readback：

- output off 或预期状态。
- power。
- sweep start/stop/step/dwell。
- trigger source。
- error queue。

禁止：

- 未列入白名单的 `SWE:OUTP:*`。
- 未验证的 list mode。
- 未验证的 modulation 组合。
- 运行时任意 raw SCPI passthrough。

本轮真机坑：

- `SWE:OVOL:STAR 0V` / `SWE:OVOL:STOP 3V` 会触发 `-103,"Invalid separator;V"`
- 当前实验室真机接受的是不带 `V` 后缀的 `SWE:OVOL:STAR 0` / `SWE:OVOL:STOP 3`
- point 内 `RF ON` 与 `frequency sweep active` 不是同一件事，runtime 必须显式管理 `OUTP` 与 `FREQ:MODE`

## OE1022D 第一版命令

用途：固定观测器和连续 `RALL?` 数据源。

允许命令：

| 动作 | Helper | 命令形态 | 类别 | 验证状态 |
| --- | --- | --- | --- | --- |
| 查询身份 | transport 内 `*IDN?` | `*IDN?` | read_only | `rebuild_smoke_verified` |
| 设置参考源 | `oe1022d_set_reference_source` | `FMODD i,j` | setup | `legacy_verified_not_rechecked` |
| 查询参考源 | `oe1022d_query_reference_source` | `FMODD? i` | read_only | `allowed_not_yet_verified` |
| 设置外部参考触发方式 | `oe1022d_set_reference_slope` | `RSLPD i,j` | setup | `allowed_not_yet_verified` |
| 查询外部参考触发方式 | `oe1022d_query_reference_slope` | `RSLPD? i` | read_only | `allowed_not_yet_verified` |
| 设置参考频率 | `oe1022d_set_reference_frequency_hz` | `FREQD i,f` | setup | `legacy_verified_not_rechecked` |
| 设置输入方式 | `oe1022d_set_input_source` | `ISRCD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置输入接地 | `oe1022d_set_input_grounding` | `IGNDD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置输入耦合 | `oe1022d_set_input_coupling` | `ICPLD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置陷波器 | `oe1022d_set_line_notch_filter` | `ILIND i,j` | setup | `legacy_verified_not_rechecked` |
| 设置动态储备 | `oe1022d_set_dynamic_reserve` | `RMODD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置灵敏度 | `oe1022d_set_sensitivity_index` | `SENSD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置时间常数 | `oe1022d_set_time_constant_index` | `OFLTD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置滤波斜率 | `oe1022d_set_filter_slope` | `OFSLD i,j` | setup | `legacy_verified_not_rechecked` |
| 设置同步滤波 | `oe1022d_set_sync_filter` | `SYNCD i,j` | setup | `legacy_verified_not_rechecked` |
| 读取全局帧 | `oe1022d_rall_query` | `RALL?` | acquisition | `rebuild_smoke_verified` |

运行约束：

- setup 只在 run 前执行。
- run 中只允许 collector 周期性执行 `RALL?`。
- point 不允许修改 OE 配置。
- 任何 viewer 不允许直接查询 OE 串口。
- `RSLPD` 当前必须分开理解：
  - `manual-stated`：V1.5 PDF 仅写 `j=0` / `j=1`
  - `hardware-observed`：`2026-06-11` 真机在厂商 LabVIEW 配置并锁 PLL 后，观测到 `RSLPD? 2 = 2`
- 在完成受控 write-back 验证前，任何实现都不得把 `RSLPD` 枚举空间写死为只有 `0/1`。
- `RALL?` 当前必须分开理解：
  - `smoke_timeout_path`：早期诊断路径观测到过 `15168 bytes`
  - `runtime_exact_frame`：run 级 collector 已用定长 `12288 bytes` 真机打通
- 第一版 runtime parser、raw offset 和 segment 逻辑一律以 `12288` 为准

## M8812 第一版命令

用途：三轴磁场执行器。

允许命令：

| 动作 | Helper | 命令形态 | 类别 | 验证状态 |
| --- | --- | --- | --- | --- |
| 查询身份 | `m8812_query_idn` | `*IDN?` | read_only | `rebuild_smoke_verified` |
| 远程模式 | `m8812_set_remote` | `SYST:REM` | setup | `rebuild_smoke_verified` |
| 本地模式 | `m8812_set_local` | `SYST:LOC` | cleanup | `rebuild_smoke_verified` |
| 查询错误 | `m8812_query_error` | `SYST:ERR?` | read_only | `allowed_not_yet_verified` |
| 设置电压 | `m8812_set_voltage_v` | `VOLT <v>` | setup | `legacy_verified_not_rechecked` |
| 设置过压保护 | `m8812_set_voltage_protection_v` | `VOLT:PROT <v>` | setup | `legacy_verified_not_rechecked` |
| 设置电流 | `m8812_set_current_a` | `CURR <a>` | operational | `rebuild_smoke_verified` |
| 查询测量电流 | `m8812_query_meas_current_a` | `MEAS:CURR?` | read_only | `rebuild_smoke_verified` |
| 设置输出 | `m8812_set_output` | `OUTP 0/1` | operational | `rebuild_smoke_verified` |

运行约束：

- point 只表达目标磁场或目标电流。
- 电压和保护阈值属于 station/profile，不属于 point。
- 每个 point 必须记录目标电流和 readback 电流。

## CNI Laser 第一版命令

用途：激光背景条件控制。

允许命令：

| 动作 | Helper | 命令形态 | 类别 | 验证状态 |
| --- | --- | --- | --- | --- |
| 设置功率 | `cni_laser_power_set` | raw frame `55 AA 05 ...` | setup | `legacy_verified_not_rechecked` |
| 关闭输出 | `cni_laser_output_off` | raw frame `55 AA 03 00 03` | cleanup | `rebuild_smoke_verified` |
| 开启输出 | `cni_laser_output_on` | raw frame `55 AA 03 01 04` | operational | `allowed_not_yet_verified` |
| 计算校验和 | `cni_laser_checksum` | low byte sum | helper | `legacy_verified_not_rechecked` |

运行约束：

- 第一版 laser 默认由 run profile 控制。
- point 级 laser 扫描后置。
- cleanup 必须能关闭输出。

## 命令测试要求

每个命令 crate 至少保持：

- golden string 或 golden bytes 单元测试。
- 禁止命令不出现在 helper 中。
- 参数单位在函数名中明确体现。

后续增加命令时，必须同时更新本文、helper 和测试。
