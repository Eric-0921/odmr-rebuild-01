---
device: M8812
source_priority:
  - original/maynuoPseries_operation.pdf
  - maynuo_dc-power-supply/m88_manual_cleaned.md
  - maynuo_dc-power-supply/m8812_remote_control_reference.md
  - ../reverse_application/reverse_output/逆向分析报告-协议与算法还原.md
document_status: active_truth_v1
scope: first-version current-source control for magnetic-axis execution
do_not_trust:
  - reverse-only hardcoded 5A ceiling
  - reverse-only serial capability claims that exceed the manual
  - unexplained commands such as SYST:SENS without first-version need
naming_prefix: m8812_
---

# M8812 Command Truth

This file is limited to the minimal command set needed for magnetic-axis control.

## Identity and mode control

### Helper name
`m8812_query_idn`

### Raw command
`*IDN?`

### Arguments
None.

### Response
Manufacturer, model, serial number, firmware string, e.g. `MAYNUO,M8812,<SN>,<FW>`.

### Units
N/A

### Preconditions
Correct serial transport and terminator in place.

### Primary source
`original/maynuoPseries_operation.pdf`, remote-operation chapter; mirrored in `m88_manual_cleaned.md`.

### Notes / ambiguities
Match manufacturer + model + serial identity. Do not depend on one firmware string.

---

### Helper name
`m8812_set_remote`

### Raw command
`SYST:REM`

### Arguments
None.

### Response
None.

### Units
N/A

### Preconditions
Remote session active.

### Primary source
`original/maynuoPseries_operation.pdf`, remote-operation chapter; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
First-version set commands should only occur after entering remote mode.

---

### Helper name
`m8812_set_local`

### Raw command
`SYST:LOC`

### Arguments
None.

### Response
None.

### Units
N/A

### Preconditions
Send last, after output is verified off.

### Primary source
`original/maynuoPseries_operation.pdf`, remote-operation chapter; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
This belongs in cleanup, not mid-run.

---

### Helper name
`m8812_query_error`

### Raw command
`SYST:ERR?`

### Arguments
None.

### Response
Error code and message, for example `0, 'No Error'`.

### Units
N/A

### Preconditions
None.

### Primary source
`original/maynuoPseries_operation.pdf`, SCPI chapter; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
Keep this in the first-version truth set because it is cheap and useful for preflight and post-cleanup checks.

## Output configuration and readback

### Helper name
`m8812_set_voltage_v`

### Raw command
`VOLT <value>`

### Arguments
One numeric voltage value.

### Response
None.

### Units
Canonical unit is `V`.

### Preconditions
Remote mode active.

### Primary source
`original/maynuoPseries_operation.pdf`; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
For the current lab chain, `75 V` is the expected configured value. The command remains generic because the helper should still expose the manual command.

---

### Helper name
`m8812_set_voltage_protection_v`

### Raw command
`VOLT:PROT <value>`

### Arguments
One numeric over-voltage protection setting.

### Response
None.

### Units
`V`

### Preconditions
Remote mode active.

### Primary source
`original/maynuoPseries_operation.pdf`; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
Keep this in scope because it is part of the minimal protected bring-up path.

---

### Helper name
`m8812_set_current_a`

### Raw command
`CURR <value>`

### Arguments
One numeric current value.

### Response
None.

### Units
Canonical unit is `A`.

### Preconditions
Remote mode active; first-version software hard limit is `0 <= current <= 2 A`.

### Primary source
`original/maynuoPseries_operation.pdf`; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
Internal magnetic logic may operate in `mA` or `nT`, but this helper must remain manual-faithful and accept amperes.

---

### Helper name
`m8812_query_meas_current_a`

### Raw command
`MEAS:CURR?`

### Arguments
None.

### Response
Measured current value.

### Units
Treat response as `A`.

### Preconditions
Output may be on or off; readback is still meaningful.

### Primary source
`original/maynuoPseries_operation.pdf`; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
If later layers want `mA`, they must convert explicitly.

---

### Helper name
`m8812_set_output`

### Raw command
`OUTP 0|1`

### Arguments
`0` disables output, `1` enables output.

### Response
None.

### Units
N/A

### Preconditions
Voltage/current already configured.

### Primary source
`original/maynuoPseries_operation.pdf`; mirrored in `m8812_remote_control_reference.md`.

### Notes / ambiguities
Do not treat this command as equivalent to entering/leaving remote mode.

