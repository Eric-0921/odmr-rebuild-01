# M8812 Ambiguities And Validation

## Locked first-version decisions

- treat the lab device as `M8812`
- hard-limit current to `2 A`
- keep the command surface minimal

## Known conflicts

### Current ceiling: manual vs reverse

- manual: `M8812` current range is `0-2 A`
- reverse software: hardcoded `5000 mA`

Decision:

- first version trusts the manual and locks to `2 A`
- reverse-only `5 A` is recorded as historical behavior, not accepted truth

### Baud-rate claims

- manual exposes a limited serial setting set
- reverse UI suggests a wider range

Decision:

- trust the manual for supported capability
- keep first implementation at `9600`

### `SYST:SENS`

The command appears in the command reference, but first rebuild has no clear need for it and no resolved semantic contract.

Decision:

- keep it out of `command-truth.md`

### Reverse initialization sequence

Reverse analysis is useful for understanding old behavior:

- `SYST:REM`
- `VOLT 75`
- `CURR 0`
- `OUTP 0`

Decision:

- this sequence informs operational notes
- it does not outrank the manual as protocol truth

## Validation still worth doing later

- verify whether the specific lab cable path really needs DTR asserted
- capture one clean session transcript against each axis supply
- confirm whether any unit formatting edge cases appear near low-current setpoints

