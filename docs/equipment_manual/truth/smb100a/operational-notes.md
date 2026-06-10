# SMB100A Operational Notes

## Scope

These notes are guidance for the first rebuild. They are not protocol truth by themselves.

## Transport stance

- main transport: raw TCP socket
- expected port: `5025`
- `RS-VISA` / `RsInstrument` remains optional for:
  - comparison
  - troubleshooting
  - session-behavior reference

## Session shape

- connect by IP or hostname
- confirm identity with `*IDN?`
- use one request / one response sequencing
- prefer explicit readback after state-changing commands that matter to the run
- use `*OPC?` only when the instrument may still be processing an overlapped action

## Minimal first-version RF workflow

For fixed-frequency use:

1. confirm identity
2. check `SYST:ERR?`
3. set `FREQ`
4. set `POW`
5. verify `OUTP` is still off
6. enable `OUTP` only when the runtime actually begins emission

For frequency sweep use:

1. set `FREQ:STAR`
2. set `FREQ:STOP`
3. set `SWE:FREQ:STEP`
4. set `SWE:FREQ:DWEL`
5. set `SWE:MODE`
6. set `TRIG:FSW:SOUR`
7. execute one sweep path consistently

## Things intentionally not expanded yet

- no HiSLIP-first path
- no VXI-11-first path
- no generic VISA abstraction
- no wide modulation surface
- no attempt to normalize the entire SMB100A command tree

## Error-queue discipline

- `SYST:ERR?` returns and consumes the oldest queue entry
- do not spam it in the middle of a sequence unless you accept losing queue history
- do not use `*CLS` as a casual cleanup shortcut in the default runtime path

## OCR handling reminder

The split markdown is useful for grep and navigation, but command tokens in indexes/examples can be corrupted. When in doubt, verify against the original PDF正文.

