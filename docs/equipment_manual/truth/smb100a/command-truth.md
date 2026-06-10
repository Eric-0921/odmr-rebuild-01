---
device: SMB100A
source_priority:
  - original/SMB100A_OperatingManual_en_23.pdf
  - smb100a/05_remote_control_basics.md
  - smb100a/06a_common_commands.md
  - smb100a/06j_output_subsystem.md
  - smb100a/06l_source_subsystem.md
  - smb100a/06n_system_subsystem.md
document_status: active_truth_v1
scope: first-version raw-socket control for identity, RF output, CW frequency/power, and RF frequency sweep
do_not_trust:
  - OCR-mutated tokens copied from indexes or examples without正文核对
  - split markdown as an independent truth source
  - RS-VISA-specific API shape as runtime design input
naming_prefix: smb100a_
---

# SMB100A Command Truth

This file defines the smallest command set allowed into the first rebuild.

## Session and diagnostics

### Helper name
`smb100a_query_idn`

### Raw command
`*IDN?`

### Arguments
None.

### Response
ASCII identification string for manufacturer, model, serial number, and firmware.

### Units
N/A

### Preconditions
Open remote session over raw socket or VISA-backed channel.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, chapter 6 common commands; mirrored in `smb100a/06a_common_commands.md`.

### Notes / ambiguities
Use this for first contact and identity confirmation. Do not match on firmware version alone.

---

### Helper name
`smb100a_query_error_next`

### Raw command
`SYST:ERR?`

### Arguments
None.

### Response
One error-queue entry, typically `0,"No error"` when clean.

### Units
N/A

### Preconditions
Instrument reachable; query consumes the oldest queued entry.

### Primary source
`original/smb100a_clean_operation.md` error-queue description; mirrored in `smb100a/06n_system_subsystem.md`.

### Notes / ambiguities
This is the preferred first-version queue check. `SYST:ERR:ALL?` exists, but is intentionally excluded from the minimal truth set.

---

### Helper name
`smb100a_query_operation_complete`

### Raw command
`*OPC?`

### Arguments
None.

### Response
ASCII `1` when all pending operations are complete.

### Units
N/A

### Preconditions
Use only after commands that can overlap internally.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, chapter 5.2 synchronization; mirrored in `smb100a/05_remote_control_basics.md`.

### Notes / ambiguities
Prefer `*OPC?` over blind sleeps when the instrument may still be processing an overlapped action.

---

### Helper name
`smb100a_clear_status`

### Raw command
`*CLS`

### Arguments
None.

### Response
None.

### Units
N/A

### Preconditions
Diagnostic-only in first version. Not part of the default run path.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, common commands; mirrored in `smb100a/06a_common_commands.md`.

### Notes / ambiguities
Allowed in the truth set so it can be implemented, but it must not be used casually in runtime startup. It clears status state and can hide evidence.

## RF output and CW state

### Helper name
`smb100a_set_output`

### Raw command
`OUTP ON|OFF`

### Arguments
`ON` enables RF output, `OFF` disables RF output.

### Response
None.

### Units
N/A

### Preconditions
Instrument already configured to a valid RF state.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, output subsystem; mirrored in `smb100a/06j_output_subsystem.md`.

### Notes / ambiguities
Use explicit `OFF` during cleanup. Do not infer state from cached UI assumptions.

---

### Helper name
`smb100a_query_output`

### Raw command
`OUTP?`

### Arguments
None.

### Response
Output state token, typically `0/1` or `OFF/ON` depending on transport formatting.

### Units
N/A

### Preconditions
None beyond remote reachability.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, output subsystem; mirrored in `smb100a/06j_output_subsystem.md`.

### Notes / ambiguities
First-version consumers should normalize both numeric and textual ON/OFF responses.

---

### Helper name
`smb100a_set_frequency_hz`

### Raw command
`FREQ <value>`

### Arguments
One numeric RF frequency value.

### Response
None.

### Units
Canonical unit is `Hz`.

### Preconditions
Use in CW mode or as the current RF frequency anchor before switching sweep mode.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, RF frequency; supported by existing lab usage.

### Notes / ambiguities
Keep internal software unit canonical in `Hz`, even if operator-facing formatting later uses MHz/GHz.

---

### Helper name
`smb100a_query_frequency`

### Raw command
`FREQ?`

### Arguments
None.

### Response
Current RF frequency.

### Units
Treat returned value as `Hz`.

### Preconditions
None.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, RF frequency query behavior.

### Notes / ambiguities
Returned value can be influenced by offset/unit context in broader SMB state. First-version runtime should treat it as readback, not as planning input.

---

### Helper name
`smb100a_set_power_dbm`

### Raw command
`POW <value>dBm`

### Arguments
One numeric RF level.

### Response
None.

### Units
Canonical unit is `dBm`.

### Preconditions
Valid level range for the current frequency and hardware option set.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, RF level section and source subsystem; supported by existing lab usage.

### Notes / ambiguities
First version keeps power in `dBm` only. No unit abstraction layer is needed yet.

---

### Helper name
`smb100a_query_power`

### Raw command
`POW?`

### Arguments
None.

### Response
Current RF level.

### Units
Treat response as `dBm` in the first version.

### Preconditions
None.

### Primary source
`original/SMB100A_OperatingManual_en_23.pdf`, RF level query behavior.

### Notes / ambiguities
Do not silently reinterpret this as volts or EMF mode unless a later version explicitly opens that scope.

## RF frequency sweep

### Helper name
`smb100a_set_sweep_start_hz`

### Raw command
`FREQ:STAR <value>Hz`

### Arguments
One numeric sweep start frequency.

### Response
None.

### Units
`Hz`

### Preconditions
Sweep plan chosen by runtime.

### Primary source
`smb100a/06l_source_subsystem.md`

### Notes / ambiguities
Start/stop belong to the frequency subsystem, not the separate trigger command family.

---

### Helper name
`smb100a_set_sweep_stop_hz`

### Raw command
`FREQ:STOP <value>Hz`

### Arguments
One numeric sweep stop frequency.

### Response
None.

### Units
`Hz`

### Preconditions
Sweep plan chosen by runtime.

### Primary source
`smb100a/06l_source_subsystem.md`

### Notes / ambiguities
Keep start/stop validation in the future plan/runtime layer, not in this truth file.

---

### Helper name
`smb100a_set_sweep_step_hz`

### Raw command
`SWE:FREQ:STEP <value>Hz`

### Arguments
One numeric step size.

### Response
None.

### Units
`Hz`

### Preconditions
Frequency sweep path selected.

### Primary source
Existing lab command usage plus `smb100a/06l_source_subsystem.md`.

### Notes / ambiguities
OCR can mutate related sweep tokens in examples; prefer正文命令标题.

---

### Helper name
`smb100a_set_sweep_dwell_ms`

### Raw command
`SWE:FREQ:DWEL <value>ms`

### Arguments
One numeric dwell time.

### Response
None.

### Units
Canonical unit is `ms`.

### Preconditions
Frequency sweep path selected.

### Primary source
Existing lab command usage plus `smb100a/06l_source_subsystem.md`.

### Notes / ambiguities
Store dwell internally as milliseconds for first version; no broader time-unit layer is needed yet.

---

### Helper name
`smb100a_set_sweep_mode`

### Raw command
`SWE:MODE AUTO|MAN|STEP`

### Arguments
One sweep execution mode token.

### Response
None.

### Units
N/A

### Preconditions
Sweep path selected.

### Primary source
`smb100a/06l_source_subsystem.md`

### Notes / ambiguities
For the first rebuild, treat this as an explicit runtime choice, not an implicit side effect of other commands.

---

### Helper name
`smb100a_set_sweep_trigger_source`

### Raw command
`TRIG:FSW:SOUR SING|AUTO|EXT`

### Arguments
One trigger-source token.

### Response
None.

### Units
N/A

### Preconditions
Frequency sweep configured.

### Primary source
`smb100a/06o_test_subsystem.md`

### Notes / ambiguities
The first rebuild only needs the command truth. Policy for which token to prefer belongs to runtime design.

---

### Helper name
`smb100a_execute_frequency_sweep`

### Raw command
`SWE:FREQ:EXEC`

### Arguments
None.

### Response
None.

### Units
N/A

### Preconditions
Sweep mode and trigger path already configured.

### Primary source
`smb100a/06o_test_subsystem.md`

### Notes / ambiguities
`TRIG:FSW:IMM` is an alternative trigger-style path. First version should pick one execution route and stay consistent.

