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
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-idn --port COM8 --baud 115200
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-rall --port COM8 --baud 115200 --count 1 --out-dir runs/oe1300_serial_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-idn --host 192.168.1.1 --port 10001
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-rall --host 192.168.1.1 --port 10001 --count 1 --out-dir runs/oe1300_net_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-collector-demo --host 192.168.1.1 --port 10001 --duration-sec 60 --out-dir runs/oe1300_net_collector_demo
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-collector-demo --host 192.168.1.1 --port 10001 --decode-in-loop true --duration-sec 60 --out-dir runs/oe1300_net_collector_demo_decode
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-collector-demo --host 192.168.1.1 --port 10001 --post-write-delay-ms 0 --write-artifacts false --duration-sec 10 --out-dir runs/oe1300_net_collector_benchmark
dotnet .\tools\win-csharp\Odmr.WinProbe\bin\Release\net8.0\Odmr.WinProbe.dll oe1300-net-collector-demo --host 192.168.1.1 --port 10001 --post-write-delay-ms 0 --drain-before-write true --duration-sec 60 --out-dir D:\temp\oe1300_net_collector_release
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-outp-demo --host 192.168.1.1 --port 10001 --param-index 0 --duration-sec 10 --out-dir runs/oe1300_net_outp_demo
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-raw-analyze --raw runs/oe1300_net_collector_demo/raw/oe1300_tcp.rall --max-frames 5000 --duration-sec 60
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_run_execute_minimal --out runs/win_csharp_run_execute_minimal/continuity_audit.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- device-command-check
dotnet run --project tools/win-csharp/Odmr.WinProbe -- live-replay --run runs/win_csharp_run_execute_minimal
```

`oe-rall` writes:

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`
- `summary.json`

`oe1300-net-collector-demo` is a standalone OE1300 TCP sampling demo. It only:

- writes `RALL?\r`
- waits `5ms` by default
- reads until `32768B`
- appends raw frame and frame index

With `--decode-in-loop true`, it additionally runs the current experimental
`Oe1300Parsers.DecodeTcpRall(...)` inside the collector loop so sampling-rate
impact can be compared directly against the raw-only path.

With `--write-artifacts false`, it skips raw/index/segment writes and acts as a
transport ceiling benchmark for the current `RALL?` path.

With `--drain-before-write true` (default), the collector clears any buffered
stale bytes before sending the next `RALL?\r`, so the measured rate is the fresh
query loop rate rather than a socket-backlog replay rate.

With `--drain-before-write false`, it skips that pre-drain step. This is only
useful for diagnosing socket backlog behavior and must not be treated as a fresh
sampling-rate measurement.

`oe1300-net-raw-analyze` is a post-run diagnostic helper. It scans adjacent
32768 B blocks in a captured raw file and reports how often the block content
actually changes, so query rate and new-block rate can be compared explicitly.

`oe1300-net-outp-demo` is the matching OE1300 TCP ASCII query benchmark for a
single parameter. It repeatedly sends `OUTP? <index>`, records one returned
ASCII float per query, and writes:

- `values.csv`
- `summary.json`

This path is intentionally separate from the `RALL?` collector so device-layer
fresh-query rate can be measured without the 32768 B binary block path.

It writes:

- `raw/oe1300_tcp.rall`
- `raw/oe1300_tcp.frames.idx.jsonl`
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

`device-command-check` lists the migrated C# command catalog and the archived
`win-csharp-rebuild` Rust command source each entry came from.

`live-replay` reduces existing `events.jsonl`, frame index, and summary files
into a current live snapshot. It does not connect to hardware.
