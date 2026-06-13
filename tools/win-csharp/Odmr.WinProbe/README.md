# Odmr.WinProbe

Minimal Windows C# probe for the first VISA rebuild gate.

This tool intentionally keeps the OE1022D `RALL?` hot path small:

```text
open VISA ASRL
clear input once
loop:
write RALL?
sleep 30ms
blocking read 12288 bytes
append raw frame and frame index
```

## Frozen RALL Hot Path

The `oe-rall` loop is a frozen LabVIEW-like contract. Do not change its order
or add work inside the loop unless the change is followed by a new 15-minute
continuity run with `delta_gt1_count = 0` and `run audit-continuity` returning
`verdict = continuous`.

Allowed inside the loop:

- write `RALL?\r`
- sleep `30ms`
- blocking exact read `12288` bytes
- append raw frame
- append frame index
- update minimal counters from `payload[12287]`

Forbidden inside the loop:

- extra poll sleep
- first-byte deadline
- frame deadline
- zero-byte retry
- timeout clear/retry
- per-frame console output
- per-frame object serialization
- RALL parsing
- GUI publish
- async or multi-reader behavior

It does not implement station resolving, GUI bridging, RALL parsing, retries,
frame deadlines, or SMB100A control beyond the read-only TCP probe.

## Commands

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
```

`oe-rall` writes:

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`
- `summary.json`

`smb-probe` keeps the current Ethernet raw socket path and only sends:

- `*IDN?`
- `SYST:ERR?`
- `OUTP?`

`m8812-probe` only performs the safe minimum serial check:

- `*IDN?`
- `SYST:REM`
- `MEAS:CURR?`
- cleanup: `CURR 0.00000`, `OUTP 0`, `SYST:LOC`
