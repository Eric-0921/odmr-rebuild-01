---
device: OE1022D
source_priority:
  - original/OE1022D用户使用文档V1.5.pdf
  - oe1022d/04_menu.md
  - oe1022d/05_oe1022d_remote_programming_commands_55_74.md
  - oe1022d/05_oe1022d_rall_global_data_config_reading.md
  - oe1022d/校对后的oe1022d面板基础设置/*.json
document_status: active_truth_v1
scope: first-version fixed configuration plus continuous RALL acquisition
do_not_trust:
  - original/oe1022d_clean_operation.md as the top truth source
  - supplemental markdown that introduces unsupported enum values without PDF backing
  - guessed packed-struct interpretations of the RALL frame
naming_prefix: oe1022d_
---

# OE1022D Command Truth

This file only promotes the command subset needed for the first continuous-acquisition rebuild.

## Channel convention

- `i = 1` means channel A
- `i = 2` means channel B

## Reference and basic acquisition context

### Helper name
`oe1022d_set_reference_source`

### Raw command
`FMODD i,j`

### Arguments
- `i`: channel selector
- `j`: reference source
  - `0` external
  - `1` internal
  - `2` internal sweep

### Response
None.

### Units
N/A

### Preconditions
ASCII command session, one line terminated with `LF` or `CR`.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.1; mirrored in `oe1022d/05_oe1022d_remote_programming_commands_55_74.md`.

### Notes / ambiguities
Keep enum values exactly as the manual table states.

---

### Helper name
`oe1022d_query_reference_source`

### Raw command
`FMODD? i`

### Arguments
One channel selector.

### Response
Current reference-source code for that channel.

### Units
N/A

### Preconditions
None.

### Primary source
Same as above.

### Notes / ambiguities
Treat returned value as the canonical source of truth, not GUI panel assumptions.

---

### Helper name
`oe1022d_set_reference_frequency_hz`

### Raw command
`FREQD i,f`

### Arguments
- `i`: channel selector
- `f`: frequency value

### Response
None.

### Units
Canonical unit is `Hz`.

### Preconditions
Use when the selected reference source allows an internal reference frequency.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.1.

### Notes / ambiguities
Store internally in `Hz`, not kHz.

## Input and filter setup

### Helper name
`oe1022d_set_input_source`

### Raw command
`ISRCD i,j`

### Arguments
- `i`: channel selector
- `j`: input mode
  - `0` A
  - `1` A-B
  - `2` 1 MOhm current input
  - `3` 100 MOhm current input

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.3.

### Notes / ambiguities
The first rebuild should keep these integer enums explicit, not hide them behind guessed descriptive aliases.

---

### Helper name
`oe1022d_set_input_grounding`

### Raw command
`IGNDD i,j`

### Arguments
- `i`: channel selector
- `j`: grounding mode
  - `0` float
  - `1` ground

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.3.

### Notes / ambiguities
None beyond keeping to the manual table.

---

### Helper name
`oe1022d_set_input_coupling`

### Raw command
`ICPLD i,j`

### Arguments
- `i`: channel selector
- `j`: coupling mode
  - `0` AC
  - `1` DC

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.3.

### Notes / ambiguities
None beyond keeping to the manual table.

---

### Helper name
`oe1022d_set_line_notch_filter`

### Raw command
`ILIND i,j`

### Arguments
- `i`: channel selector
- `j`: notch-filter mode
  - `0` off
  - `1` 50 Hz
  - `2` 50 Hz + 100 Hz
  - `3` 100 Hz

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.3.

### Notes / ambiguities
Keep the integer table exactly as printed.

---

### Helper name
`oe1022d_set_dynamic_reserve`

### Raw command
`RMODD i,j`

### Arguments
- `i`: channel selector
- `j`: dynamic reserve
  - `0` Low Noise
  - `1` Normal
  - `2` High Reserve

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.4.

### Notes / ambiguities
None beyond keeping to the manual table.

---

### Helper name
`oe1022d_set_sensitivity_index`

### Raw command
`SENSD i,j`

### Arguments
- `i`: channel selector
- `j`: sensitivity index

### Response
None.

### Units
The index maps to a manual lookup table. The helper should keep the index explicit.

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.4.

### Notes / ambiguities
First rebuild should not invent a new normalized enum table until the command layer is implemented and tested.

---

### Helper name
`oe1022d_set_time_constant_index`

### Raw command
`OFLTD i,j`

### Arguments
- `i`: channel selector
- `j`: time-constant index

### Response
None.

### Units
Index maps to the manual table.

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.4.

### Notes / ambiguities
The cleaned markdown includes unit mistakes in places. Trust the original PDF table first.

---

### Helper name
`oe1022d_set_filter_slope`

### Raw command
`OFSLD i,j`

### Arguments
- `i`: channel selector
- `j`: slope selector
  - `0` 6 dB/oct
  - `1` 12 dB/oct
  - `2` 18 dB/oct
  - `3` 24 dB/oct

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.4.

### Notes / ambiguities
None beyond keeping to the manual table.

---

### Helper name
`oe1022d_set_sync_filter`

### Raw command
`SYNCD i,j`

### Arguments
- `i`: channel selector
- `j`: sync-filter state
  - `0` off
  - `1` on

### Response
None.

### Units
N/A

### Preconditions
Channel exists.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.4.

### Notes / ambiguities
The UI can gray this control in some contexts, but the remote command itself remains part of the protocol surface.

## Continuous acquisition

### Helper name
`oe1022d_rall_query`

### Raw command
`RALL?`

### Arguments
None.

### Response
Binary frame of `12288` bytes.

### Units
Mixed binary payload:
- measurement arrays
- one-shot configuration values
- trailing unused bytes

### Preconditions
Treat as the dedicated continuous-acquisition path. Use a single producer loop.

### Primary source
`original/OE1022D用户使用文档V1.5.pdf`, chapter 5.2.11; mirrored in `oe1022d/05_oe1022d_rall_global_data_config_reading.md`.

### Notes / ambiguities
The manual gives field order and byte ranges, but does not fully settle parser details such as all inferred semantics. First-version parser work must preserve:

- confirmed fields
- inferred fields
- trailing holes

as separate categories.

