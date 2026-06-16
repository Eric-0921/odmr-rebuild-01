#!/usr/bin/env python3
"""Verify OE1300/OE1351 serial commands against the manual.

Reads commands from a JSONL test plan, sends each over serial, records the
raw response, round-trip latency, and a pass/fail heuristic.
"""

from __future__ import annotations

import argparse
import json
import pathlib
import sys
import time
from dataclasses import asdict, dataclass
from typing import Any

import serial


DEFAULT_PORT = "/dev/cu.usbserial-B0027SH3"
DEFAULT_BAUD = 115200
DEFAULT_TIMEOUT = 3.0


@dataclass
class ProbeResult:
    idx: int
    command: str
    raw_sent: str
    raw_response: str
    response_hex: str
    latency_ms: float
    status: str  # ok | empty | timeout | error
    note: str = ""


def now_ts() -> str:
    millis = int(time.time() * 1000)
    return f"{millis // 1000}.{millis % 1000:03d}Z"


def send_command(ser: serial.Serial, command: str, timeout: float) -> tuple[str, float, str]:
    """Send a command and return (raw_response, latency_ms, status)."""
    raw = (command + "\r").encode("ascii")
    ser.reset_input_buffer()
    ser.reset_output_buffer()

    start = time.perf_counter()
    ser.write(raw)
    ser.flush()

    deadline = time.perf_counter() + timeout
    chunks: list[bytes] = []
    while time.perf_counter() < deadline:
        pending = ser.in_waiting
        if pending:
            chunk = ser.read(pending)
            chunks.append(chunk)
            # OE1300 responses end with \r; stop early if we see it.
            if chunk.endswith(b"\r"):
                break
        else:
            time.sleep(0.02)

    latency_ms = (time.perf_counter() - start) * 1000.0
    raw_bytes = b"".join(chunks)
    raw_response = raw_bytes.decode("ascii", errors="replace").rstrip("\r\n")

    if not chunks:
        status = "timeout"
    elif raw_response == "":
        status = "empty"
    else:
        status = "ok"

    return raw_response, latency_ms, status


def make_test_plan() -> list[dict[str, Any]]:
    """Default command verification plan based on the OE1300 manual."""
    commands: list[dict[str, Any]] = [
        # Basic identification / reset
        {"cmd": "*IDN?", "note": "identification"},
        {"cmd": "*RST", "note": "reset to defaults", "expect_empty": True},
        {"cmd": "*IDN?", "note": "identification after reset"},

        # Network parameters (read, then write, then read)
        {"cmd": "NMOD?", "note": "network mode query"},
        {"cmd": "NIPA?", "note": "IP address query"},
        {"cmd": "NSMA?", "note": "subnet mask query"},
        {"cmd": "NGWA?", "note": "gateway query"},
        {"cmd": "NMOD 0", "note": "set TCP mode"},
        {"cmd": "NIPA 192.168.1.5", "note": "set IP address"},
        {"cmd": "NSMA 255.255.255.0", "note": "set subnet mask"},
        {"cmd": "NGWA 192.168.1.1", "note": "set gateway to device IP"},
        {"cmd": "NMOD?", "note": "network mode query after set"},
        {"cmd": "NIPA?", "note": "IP address query after set"},
        {"cmd": "NSMA?", "note": "subnet mask query after set"},
        {"cmd": "NGWA?", "note": "gateway query after set"},

        # Input configuration
        {"cmd": "ISRC?", "note": "input source query"},
        {"cmd": "ICPL?", "note": "input coupling query"},
        {"cmd": "IGND?", "note": "input grounding query"},
        {"cmd": "IRNG?", "note": "input range query"},

        # Reference / phase
        {"cmd": "FMOD?", "note": "reference source query"},
        {"cmd": "FREQ?", "note": "reference frequency query"},
        {"cmd": "PHAS?", "note": "reference phase query"},
        {"cmd": "RMOD?", "note": "reference mode query (if supported)"},
        {"cmd": "RSLP?", "note": "reference slope query (if supported)"},

        # Filter
        {"cmd": "OFLT?", "note": "time constant query"},
        {"cmd": "OFSL?", "note": "filter slope query"},
        {"cmd": "SYNC?", "note": "sync filter query"},

        # Output
        {"cmd": "OUTP?", "note": "output enable query"},
        {"cmd": "OEUT? 0", "note": "CH1 output source query"},
        {"cmd": "OEUT? 1", "note": "CH2 output source query"},

        # Harmonic
        {"cmd": "HARM? 0", "note": "harmonic 1 query"},

        # Utility
        {"cmd": "BAUD?", "note": "baud rate query"},
        {"cmd": "OVLD?", "note": "overload query"},
        {"cmd": "*PLL?", "note": "PLL status query"},

        # Data read commands
        {"cmd": "SNAP? 0", "note": "snap parameter 0 (X)"},
        {"cmd": "SNAP? 2", "note": "snap parameter 2 (R)"},
        {"cmd": "SNAP? 34", "note": "snap parameter 34 (Frequency)"},
        {"cmd": "RALL?", "note": "read all demodulator values (37 CSV fields)"},
    ]
    return commands


def run_probe(args: argparse.Namespace) -> int:
    out_dir = pathlib.Path(args.out_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    results_path = out_dir / "oe1300_command_probe_results.jsonl"
    summary_path = out_dir / "oe1300_command_probe_summary.json"

    plan = make_test_plan()
    if args.plan:
        with open(args.plan, "r", encoding="utf-8") as f:
            plan = [json.loads(line) for line in f if line.strip()]

    results: list[ProbeResult] = []

    with serial.Serial(args.port, args.baud, timeout=args.timeout) as ser:
        print(f"Opened {args.port} @ {args.baud} baud, timeout={args.timeout}s")
        print(f"Running {len(plan)} commands...")

        for idx, item in enumerate(plan, start=1):
            cmd = item["cmd"]
            note = item.get("note", "")
            expect_empty = item.get("expect_empty", False)

            raw_response, latency_ms, status = send_command(ser, cmd, args.timeout)

            # Heuristic: if command expects empty response, treat empty as ok.
            if expect_empty and status == "empty":
                status = "ok"

            result = ProbeResult(
                idx=idx,
                command=cmd,
                raw_sent=cmd + "\\r",
                raw_response=raw_response,
                response_hex=raw_response.encode("ascii", errors="replace").hex(),
                latency_ms=round(latency_ms, 3),
                status=status,
                note=note,
            )
            results.append(result)

            with open(results_path, "a", encoding="utf-8") as f:
                f.write(json.dumps(asdict(result), ensure_ascii=False) + "\n")

            print(f"[{idx:03d}/{len(plan):03d}] {cmd:30s} -> {status:8s} ({latency_ms:7.2f} ms) {note}")

            # *RST may trigger a device restart; give it more time before next command.
            if cmd.upper().startswith("*RST"):
                time.sleep(2.5)
            else:
                time.sleep(0.1)

    ok_count = sum(1 for r in results if r.status == "ok")
    empty_count = sum(1 for r in results if r.status == "empty")
    timeout_count = sum(1 for r in results if r.status == "timeout")
    error_count = sum(1 for r in results if r.status == "error")

    summary = {
        "started_at": now_ts(),
        "port": args.port,
        "baud": args.baud,
        "timeout_s": args.timeout,
        "commands_total": len(results),
        "ok": ok_count,
        "empty": empty_count,
        "timeout": timeout_count,
        "error": error_count,
        "results_file": str(results_path),
    }
    with open(summary_path, "w", encoding="utf-8") as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)

    print("\nSummary:")
    print(f"  total={len(results)} ok={ok_count} empty={empty_count} timeout={timeout_count} error={error_count}")
    print(f"  results: {results_path}")
    print(f"  summary: {summary_path}")
    return 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Verify OE1300/OE1351 serial commands")
    parser.add_argument("--port", default=DEFAULT_PORT, help="serial port")
    parser.add_argument("--baud", type=int, default=DEFAULT_BAUD, help="baud rate")
    parser.add_argument("--timeout", type=float, default=DEFAULT_TIMEOUT, help="read timeout per command (seconds)")
    parser.add_argument("--out-dir", default="/tmp/oe1300_probe", help="output directory")
    parser.add_argument("--plan", help="JSONL test plan file (default: built-in manual commands)")
    return parser.parse_args()


if __name__ == "__main__":
    sys.exit(run_probe(parse_args()))
