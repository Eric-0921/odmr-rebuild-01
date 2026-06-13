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

It does not implement GUI bridging, live bridging, RALL parsing, retries, or
frame deadlines. `run-execute` restores the JSON-driven station/profile/plan
runtime path and applies the OE fixed profile once before starting the frozen
collector.

## Commands

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_run_execute_minimal --out runs/win_csharp_run_execute_minimal/continuity_audit.json
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

`laser-probe --off-only` only sends the output-off frame and verifies echo:

- `55 AA 03 00 03`

`run-execute` writes the config-driven runtime artifact set:

- snapshots: station, plan, calibration, SMB, OE, laser
- `run_manifest.json`
- `events.jsonl`
- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`
- `points.jsonl`
- `quality.jsonl`
- `summary.json`

`artifact-check` is an offline run directory contract check. It reads existing
artifacts only and does not open instruments or touch the collector.

`audit-continuity` is an offline device-packet-counter audit. It does not parse
RALL payloads in the collector thread.
