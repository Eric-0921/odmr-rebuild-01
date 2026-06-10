# CNI Laser Operational Notes

## Device stance

This unit is a low-observability write-only device in the first rebuild.

Meaning:

- we can send power / on / off frames
- we do not assume query, ACK, or state-readback support
- correctness must be established by external observation and controlled workflow

## What the device report tells us

From the retained device-specific report:

- model family corresponds to `532 nm` / `~300 mW`
- power-supply mode is `APC`
- modulation is reported as `No`

These are useful operating constraints, not protocol expansions.

## Practical first-version workflow

1. open serial link
2. send `laser off`
3. send power set
4. send `laser on` only when the run requires emission
5. always end by sending `laser off`

## Important implementation stance

- treat frames as bytes, not ASCII hex text
- checksum logic belongs in the command-helper layer
- later runtime should keep a dedicated emergency-off path

## Non-goals

- no generic SCPI wrapper
- no speculative state machine
- no fake readback API
- no modulation controls

