# M8812 Operational Notes

## Physical interface

- rear connector is DB9 at TTL level
- the manual expects the dedicated level-shift cable, not a plain RS232 cable
- `LF` termination is required

## First-version serial stance

- baud rate: `9600`
- `8N1`
- no flow control
- no parity

Reverse material also mentions `DTR=true`. That is a useful implementation hint, but it is not promoted to protocol truth here.

## Minimal bring-up sequence

1. open serial transport
2. `*IDN?`
3. `SYST:REM`
4. `VOLT 75`
5. `CURR 0`
6. `OUTP 0`

This reflects current lab usage, while keeping command truth separate from policy.

## Minimal cleanup sequence

1. `CURR 0`
2. `OUTP 0`
3. `MEAS:CURR?`
4. `SYST:LOC`

The key idea is simple: zero current first, then remove output, then hand control back.

## Magnetic-model reminder

- the power supply speaks amperes
- the magnetic model speaks current-per-axis and magnetic field targets
- line-coil constants and calibration tables do not belong in the device command layer

## First-version non-goals

- no list-mode execution
- no broad measurement surface
- no remote-sense abstraction
- no attempt to replicate every front-panel feature

