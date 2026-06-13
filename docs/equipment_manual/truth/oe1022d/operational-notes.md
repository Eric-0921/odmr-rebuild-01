# OE1022D Operational Notes

## Session stance

- command channel is ASCII
- terminator can be `LF` or `CR`
- multiple commands can be joined with `;`
- first rebuild should still prefer short, explicit command lines

## First-version role

`OE1022D` is treated as:

- a continuously running measurement source
- a fixed-configuration observer for the run

It is not treated as a per-point reconfigured mini-runtime.

## Continuous acquisition rule

- `RALL?` is the only acquisition path that matters in first version
- one collector owns the `RALL?` loop
- the current run collector loop is LabVIEW-like: write `RALL?`, wait `30ms`, blocking exact read `12288` bytes, then immediately start the next read
- first-byte deadlines, frame deadlines, zero-byte retry, and poll-interval sleep are not part of the current exact-read hot path
- other components consume the resulting stream

This is the key runtime direction that the old GUI integration violated.

## Configuration stance

The first rebuild only needs a stable fixed configuration surface:

- reference source / frequency
- input path
- reserve / sensitivity
- time constant / filter slope / sync filter

Anything beyond that stays out of scope until the continuous path is stable.

## Binary-frame handling stance

- do not collapse the full frame into a guessed packed struct
- preserve raw bytes
- build parsing around explicit offsets
- keep confirmed and inferred semantics separate

## Non-goals for first version

- no attempt to normalize the whole panel
- no attempt to expose every remote command
- no per-step collector restart
- no point-level thread lifecycle tied to acquisition
