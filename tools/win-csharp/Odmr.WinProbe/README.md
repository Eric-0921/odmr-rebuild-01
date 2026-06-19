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
detect duplicate by packet counter
direct-decode unique frames in-thread
append collector_frames + unique-only parameter_values + unique-only sample_values
```

## Frozen RALL Hot Path

The `oe-rall` loop is a frozen LabVIEW-like contract. Do not change its order
or add work inside the loop unless the change is followed by a new 15-minute
continuity run with `delta_gt1_count = 0` and `audit-continuity` returning
`verdict = continuous`.

Allowed inside the loop:

- write `RALL?`
- sleep `30ms`
- blocking exact read `12288` bytes
- detect duplicate by `payload[12287]`
- decode `20 x 50` big-endian `double` for unique frames
- append `collector_frames.jsonl`
- append unique-only `parameter_values.csv`
- append unique-only `sample_values.csv`
- update minimal counters from `payload[12287]`

Forbidden inside the loop:

- extra poll sleep
- first-byte deadline
- frame deadline
- zero-byte retry
- timeout clear/retry
- per-frame console output
- GUI publish
- async or multi-reader behavior

It does not implement GUI bridging, live bridging, RALL parsing, retries, or
frame deadlines. `run-execute` restores the JSON-driven station/profile/plan
runtime path and applies the OE fixed profile once before starting the frozen
collector.

## Commands

### Formal runtime / artifact path

These are the stable entry points for normal runs, pause/resume, and offline
artifact checks.

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-resolve --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- run-execute --station configs/stations/lab_a.json --calibration configs/calibrations/main.json --plan configs/plans/minimal_3point_runtime.json --smb-profile configs/profiles/smb100a_run_monitor_2830_2890_-10dbm.json --oe-profile configs/profiles/oe1022d_run_ch_b_observed.json --laser-profile configs/profiles/cni_laser_run_off_background.json --out-dir runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- resume-run --previous-run runs/win_csharp_run_execute_minimal --out-dir runs/win_csharp_run_execute_minimal__resume_01
dotnet run --project tools/win-csharp/Odmr.WinProbe -- artifact-check --run runs/win_csharp_run_execute_minimal
dotnet run --project tools/win-csharp/Odmr.WinProbe -- audit-continuity --run runs/win_csharp_run_execute_minimal --out runs/win_csharp_run_execute_minimal/continuity_audit.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- device-command-check
dotnet run --project tools/win-csharp/Odmr.WinProbe -- live-replay --run runs/win_csharp_run_execute_minimal
```

### Diagnostics / probe / demo path

These commands are intentionally kept under diagnostics. They are for device
identification, collector experiments, and LabVIEW-style decode validation. Do
not treat them as the formal runtime contract.

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --in-thread-process-mode measurement-means --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min_processed
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --in-thread-process-mode field-decode-csv --write-raw false --preview-field-index 8 --write-values true --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min_decoded
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-idn --port COM8 --baud 115200
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-rall --port COM8 --baud 115200 --count 1 --out-dir runs/oe1300_serial_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-idn --host 192.168.1.1 --port 10001
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-rall --host 192.168.1.1 --port 10001 --count 1 --out-dir runs/oe1300_net_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-labview-demo --host 192.168.1.1 --port 10001 --post-write-delay-ms 5 --preview-param-index 0 --csv-write-mode all --duration-sec 10 --out-dir runs/oe1300_net_labview_demo
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe1300-net-labview-demo --host 192.168.1.1 --port 10001 --post-write-delay-ms 5 --preview-param-index 0 --csv-write-mode unique-only --duration-sec 10 --out-dir runs/oe1300_net_labview_demo_unique
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --list-resources
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --resource USB0::0x0AAD::0x0054::106789::INSTR
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --station configs/stations/lab_a.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-validate --smb-profile configs/profiles/smb100a_run_pll_default.json --station configs/stations/lab_a.json
dotnet run --project tools/win-csharp/Odmr.WinProbe -- sweep-only-run --resource ASRL8::INSTR --baud 921600 --smb-resource USB0::0x0AAD::0x0054::106789::INSTR --repeat 1 --out-dir runs/sweep_only_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- minimal-3point-run --resource ASRL8::INSTR --baud 921600 --smb-resource USB0::0x0AAD::0x0054::106789::INSTR --x COM4 --y COM6 --z COM3 --cycles 1 --out-dir runs/minimal_3point_probe
dotnet run --project tools/win-csharp/Odmr.WinProbe -- m8812-probe --x COM4 --y COM6 --z COM3
dotnet run --project tools/win-csharp/Odmr.WinProbe -- laser-probe --port COM9 --off-only
```

`oe-rall --in-thread-process-mode field-decode-csv --write-raw false` reflects the
same direct-decode direction now used by runtime. It writes:

- `collector_frames.jsonl`
- `parameter_values.csv`
- `preview_values.csv`
- `summary.json`

`oe-rall --in-thread-process-mode measurement-means` is a controlled collector
overhead experiment for OE1022D. It keeps the same single-thread `RALL?` hot
path, but after each `12288 B` exact read it scans the first `8000 B`
measurement area as `1000` contiguous `f64` slots and records processing timing
in `summary.json`.

This mode exists only to compare collector-thread overhead against the frozen
baseline. It is not the default runtime contract, and it must not be treated as
permission to move general parsing into the OE1022D run-time collector.

`oe-rall --in-thread-process-mode field-decode-csv` is the second-stage direct
decode experiment for OE1022D. It still keeps the same single-thread hot path,
but after each `12288 B` exact read it immediately:

- decodes the first `8000 B` as `20 x 50` big-endian `double`
- extracts `B`-channel status bytes from fixed offsets
- writes one frame-level row into `parameter_values.csv`
- optionally writes one selected field into `preview_values.csv`

This mode established the direct-decode contract now used by runtime: do not
keep `raw/oe1022d.rall` as formal truth; persist decoded CSV/JSON directly.

Current verified OE1022D direct-decode layout is:

- frame bytes: `12288`
- measurement payload: first `8000 B`
- field count: `20`
- samples per field per frame: `50`
- byte order: big-endian `double`
- field order:
  - `A-X`, `A-Y`, `A-Freq`, `A-Noise`, `A-Xh1`, `A-Yh1`, `A-Xh2`, `A-Yh2`
  - `B-X`, `B-Y`, `B-Freq`, `B-Noise`, `B-Xh1`, `B-Yh1`, `B-Xh2`, `B-Yh2`
  - `AUXADC1`, `AUXADC2`, `AUXADC3`, `AUXADC4`
- fixed-offset status:
  - `b_ref_source_code @ 8504`
  - `b_ref_current_freq_hz @ 8505..8512`
  - `b_ref_slope_code @ 8521`
  - `b_input_overload @ 8779`
  - `b_gain_overload @ 8780`
  - `b_pll_locked @ 8781`

`oe1300-net-labview-demo` is the LabVIEW-style `RALL?` decode benchmark. It
keeps the same TCP binary hot path:

- write `RALL?\r`
- wait `5 ms` by default
- read until `32768 B`

Then it decodes the first `29600 B` as:

- `37` parameters
- `100` big-endian `double` samples per parameter
- plus fixed-offset `status` and `Trig_Count`

The summary reports both:

- `query_hz` for host-side `RALL?` loops
- `unique_block_hz` and `effective_sample_hz_per_parameter` after duplicate-block elimination

`--csv-write-mode` controls how CSV payload rows are written:

- `all` keeps the current behavior: every queried block expands into `parameter_values.csv` and `sample_values.csv`
- `unique-only` still queries and records every block in `collector_blocks.jsonl`, but only writes CSV rows for `unique_block = true`

It writes:

- `summary.json`
- `collector_blocks.jsonl`
- `parameter_values.csv`
- `sample_values.csv`
- `preview_values.csv` when `--write-values true`

Current verified interpretation is:

- first `29600 B` contains the parameter payload
- payload order follows the serial/PDF `RALL?` table exactly
- `37` parameters map to `X .. Aux-IN2`
- each parameter contains `100` samples
- each sample is one big-endian `double`
- fixed offsets also expose `status` and `Trig_Count`

For storage-pressure validation, the summary now also records:

- `unique_blocks` / `duplicate_blocks`
- `written_ralls`
- `written_samples_per_parameter_total`
- `collector_blocks_bytes`
- `parameter_values_bytes`
- `sample_values_bytes`

For strict LabVIEW-style behavior, `--drain-before-write` now defaults to
`false`. The pre-drain path should only be used as a socket-backlog diagnostic,
not as the default collector behavior.

`smb-probe` accepts exactly one connection source: `--resource` for VISA,
`--host [--port]` for TCP, or `--station` for station transport hints. Each
path only sends:

SMB100A USB VISA resources may be reported as either `USB::...` or `USB0::...`;
the resolver accepts both forms and validates the device with `*IDN?`.

- `*IDN?`
- `SYST:ERR?`
- `OUTP?`

`smb-validate` is kept as a compatibility command for profile smoke checks. It
uses the same connection source rules, applies the selected SMB profile, runs
one configured sweep, then verifies cleanup returns RF output to off and
frequency mode to CW/FIX.

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
- `collector_frames.jsonl` for OE1022D or `collector_blocks.jsonl` for OE1300
- `parameter_values.csv`
- `sample_values.csv`
- `segments.jsonl`
- `points.jsonl`
- `quality.jsonl`
- `device_state.jsonl`
- `summary.json`

`run-execute` now supports point-boundary pause through `--stop-request-file`.
When the stop request is observed before the next point, runtime finishes
cleanup, writes terminal status `paused`, and emits `run_paused`.

`resume-run` reuses the previous run snapshots and starts from the first point
that does not satisfy all three completed-point facts:

- `points.jsonl` contains the `point_id`
- `quality.jsonl` contains the same `point_id` with `quality_status = passed`
- `events.jsonl` contains `point_completed`

`resume-run` always writes into a new output directory and adds
`resume_manifest.json`. It only supports the current direct-decode truth
contract and does not read `raw/oe1022d.rall` or frame-index files.

`artifact-check` is an offline run directory contract check. It reads existing
artifacts only and does not open instruments or touch the collector. A terminal
`paused` run is treated as a valid partial artifact as long as the executed
point/segment/device-state facts are self-consistent.

`audit-continuity` is an offline device-packet-counter audit over
`collector_frames.jsonl` for OE1022D. For OE1300 it audits `collector_blocks.jsonl`,
block sequence, duplicate handling, decode failures, and effective sample rate.
It does not reopen raw binary files.

`device-command-check` lists the migrated C# command catalog and the archived
`win-csharp-rebuild` Rust command source each entry came from.

`live-replay` reduces existing `events.jsonl`, OE1022D `collector_frames.jsonl`
or OE1300 `collector_blocks.jsonl`, and summary files into a current live
snapshot. It does not connect to hardware.
