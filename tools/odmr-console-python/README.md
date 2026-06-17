# ODMR Python Console

This directory contains the Python control core and the PySide6 console.

It does not talk to VISA, serial, TCP, OE1022D, SMB100A, M8812, or CNI Laser
directly. Device control remains in the C# runtime and `Odmr.WinProbe` CLI.

## Boundary

- Python owns config composition, process launch, progress tailing, pause / resume control files, and emergency-stop request files.
- C# owns station resolution, device control, `RALL?` collection, artifacts, audit, and cleanup.
- `RALL?` hot path remains only in C# and is not touched by this console core.
- PySide6 is the main UI direction; the Tk generator remains a fallback.

## PySide6 UI

Install the UI dependency:

```bash
python3 -m pip install --user -r tools/odmr-console-python/requirements-pyside6.txt
```

Start the console:

```bash
python3 tools/odmr-console-python/odmr_console_qt.py
```

The UI pages are:

- `Run Bundle`: choose station, calibration, plan, SMB profile, OE profile, laser profile, and output.
- `Config Generator`: generate plan/profile JSON with the existing config-generator core.
- `Resolve / Estimate`: call C# `run-resolve`.
- `Run Monitor`: call C# `run-execute` / `resume-run` with `progress JSONL + stop/emergency request files`.
- `Artifact Review`: call C# `artifact-check` and `audit-continuity`.

## Commands

Generate a 3-point demo bundle with the current config generator core:

```bash
python3 tools/odmr-console-python/odmr_console.py generate-demo-bundle \
  --out-dir /tmp/odmr_console_demo \
  --run-id python_console_demo_3point
```

Resolve a bundle through C#:

```bash
python3 tools/odmr-console-python/odmr_console.py resolve \
  --plan /tmp/odmr_console_demo/python_console_demo_3point.plan.json \
  --smb-profile /tmp/odmr_console_demo/smb100a_python_console_demo.json \
  --oe-profile /tmp/odmr_console_demo/oe1022d_python_console_demo.json \
  --laser-profile /tmp/odmr_console_demo/cni_laser_python_console_demo.json
```

Start a run through C#:

```bash
python3 tools/odmr-console-python/odmr_console.py start-run \
  --plan /tmp/odmr_console_demo/python_console_demo_3point.plan.json \
  --smb-profile /tmp/odmr_console_demo/smb100a_python_console_demo.json \
  --oe-profile /tmp/odmr_console_demo/oe1022d_python_console_demo.json \
  --laser-profile /tmp/odmr_console_demo/cni_laser_python_console_demo.json \
  --out-dir runs/python_console_demo
```

This writes launch control files under:

```text
<out-dir>/control/
  progress.jsonl
  stop.request
  emergency_stop.request
  launch_metadata.json
  stdout.log
  stderr.log
```

If a run stops at a point boundary, C# writes terminal status `paused`. The
PySide6 `Resume` button then allocates a sibling output directory like
`<run-dir>__resume_01` and launches C# `resume-run --previous-run <run-dir>`.
Resume only supports the current direct-decode artifact contract; it does not
reopen historical raw-truth runs.

Request stop-after-current-point:

```bash
python3 tools/odmr-console-python/odmr_console.py stop \
  --metadata runs/python_console_demo/control/launch_metadata.json
```

Read progress:

```bash
python3 tools/odmr-console-python/odmr_console.py read-progress \
  --progress-jsonl runs/python_console_demo/control/progress.jsonl
```

## Progress JSONL

`Odmr.WinProbe run-execute` and `resume-run` write progress records only at run,
collector, point, and cleanup boundaries. They do not write per-frame progress
and do not enter the OE collector loop.

Each line includes:

- `schema_version`
- `ts`
- `pid`
- `run_id`
- `state`
- `event_name`
- `message`
- `point_id`
- `point_index`
- `points_total`
- `frames_total`
- `timeout_count`
- `raw_len_bad_count`
- `delta_gt1_count`
- `quality_status`

The Python side should treat progress as UI state only. Final data truth remains
the run artifact set and C# `artifact-check` / `audit-continuity`.

## Config Generator Integration

The console core imports `tools/config-generator/odmr_config_core.py` directly.
That preserves the current experiment-critical config surface:

- Magnetic Plan
- Plan Policy
- SMB100A profile
- OE1022D fixed profile and RALL lock checks
- CNI Laser profile

Generated JSON files are returned as a `RunBundle`, so the future UI can bind
them directly to run execution without asking the operator to browse individual
files from `configs/generated`.

## Tests

```bash
python3 -m py_compile tools/odmr-console-python/odmr_console_core.py tools/odmr-console-python/odmr_console.py tools/odmr-console-python/odmr_console_qt.py
python3 tools/odmr-console-python/tests/test_odmr_console_core.py
python3 tools/config-generator/tests/test_config_core.py
```
