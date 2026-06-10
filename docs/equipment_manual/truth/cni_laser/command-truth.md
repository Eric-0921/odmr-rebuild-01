---
device: CNI_Laser_PSU_SR
source_priority:
  - CNI Laser psu-sr/DL21088_N5250-10.md
  - CNI Laser psu-sr/RS232语言协议_恒功率.md
  - CNI Laser psu-sr/激光器使用说明书.md
document_status: active_truth_v1
scope: first-version write-only APC power supply control
do_not_trust:
  - assumptions of SCPI compatibility
  - undocumented readback or ACK behavior
  - generic laser-manual power limits as this unit's protocol truth
naming_prefix: cni_laser_
---

# CNI Laser Command Truth

This device is not treated as a standard SCPI instrument. The first rebuild keeps only the raw write-frame truth.

## Frame rules

- serial: `9600`, `8N1`
- payloads are raw bytes, not ASCII command strings
- all frames start with header `55 AA`

## Power set

### Helper name
`cni_laser_power_set`

### Raw command
`55 AA 05 01 <hi> <lo> <checksum>`

### Arguments
- `<hi>`: power value high byte
- `<lo>`: power value low byte
- `<checksum>`: low 8 bits of the sum of `05 + 01 + <hi> + <lo>`

### Response
No reliable query-style response is documented.

### Units
Power argument is expressed in `mW`.

### Preconditions
Serial link established; caller sends raw bytes.

### Primary source
`CNI Laser psu-sr/RS232语言协议_恒功率.md`

### Notes / ambiguities
The first implementation should clamp by a station/profile-level configured max power, not by guesswork from a generic manual sentence.

## Laser off

### Helper name
`cni_laser_output_off`

### Raw command
`55 AA 03 00 03`

### Arguments
None.

### Response
No reliable query-style response is documented.

### Units
N/A

### Preconditions
Raw-byte serial path.

### Primary source
`CNI Laser psu-sr/RS232语言协议_恒功率.md`

### Notes / ambiguities
This is the most important command in the first build because it is the only documented hard-disable frame.

## Laser on

### Helper name
`cni_laser_output_on`

### Raw command
`55 AA 03 01 04`

### Arguments
None.

### Response
No reliable query-style response is documented.

### Units
N/A

### Preconditions
Raw-byte serial path.

### Primary source
`CNI Laser psu-sr/RS232语言协议_恒功率.md`

### Notes / ambiguities
Do not assume an ACK frame exists just because some serial devices echo writes.

