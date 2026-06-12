#!/usr/bin/env python3
"""Mac-side OE1022D RALL? LabVIEW-style probe using pyserial."""

from __future__ import annotations

import argparse
import json
import pathlib
import statistics
import time

import serial

FRAME_BYTES = 12288
COUNTER_OFFSET = 12287


def now_ts() -> str:
    millis = int(time.time() * 1000)
    return f"{millis // 1000}.{millis % 1000:03d}Z"


def monotonic_ns(start: int) -> int:
    return time.monotonic_ns() - start


def percentile(sorted_values: list[float], p: float) -> float | None:
    if not sorted_values:
        return None
    index = round((len(sorted_values) - 1) * p)
    return sorted_values[index]


class CounterAudit:
    def __init__(self) -> None:
        self.previous: tuple[int, int] | None = None
        self.frames_audited = 0
        self.first_counter: int | None = None
        self.last_counter: int | None = None
        self.delta_1_count = 0
        self.delta_0_count = 0
        self.delta_gt1_count = 0
        self.estimated_missing_windows = 0
        self.delta_counts: dict[int, int] = {}
        self.gaps_ms: list[float] = []

    def record(self, monotonic: int, counter: int) -> None:
        if self.first_counter is None:
            self.first_counter = counter
        if self.previous is not None:
            previous_ns, previous_counter = self.previous
            delta = (counter - previous_counter) & 0xFF
            self.delta_counts[delta] = self.delta_counts.get(delta, 0) + 1
            self.gaps_ms.append((monotonic - previous_ns) / 1_000_000.0)
            if delta == 0:
                self.delta_0_count += 1
            elif delta == 1:
                self.delta_1_count += 1
            else:
                self.delta_gt1_count += 1
                self.estimated_missing_windows += delta - 1
        self.previous = (monotonic, counter)
        self.last_counter = counter
        self.frames_audited += 1

    def finish(self) -> dict:
        gaps = sorted(self.gaps_ms)
        return {
            "offset": COUNTER_OFFSET,
            "frames_audited": self.frames_audited,
            "boundaries_evaluated": max(0, self.frames_audited - 1),
            "first_counter": self.first_counter,
            "last_counter": self.last_counter,
            "delta_1_count": self.delta_1_count,
            "delta_0_count": self.delta_0_count,
            "delta_gt1_count": self.delta_gt1_count,
            "estimated_missing_windows": self.estimated_missing_windows,
            "delta_counts": [
                {"delta": delta, "count": count}
                for delta, count in sorted(self.delta_counts.items())
            ],
            "gap_median_ms": statistics.median(gaps) if gaps else None,
            "gap_p95_ms": percentile(gaps, 0.95),
            "gap_p99_ms": percentile(gaps, 0.99),
            "gap_max_ms": gaps[-1] if gaps else None,
        }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--frames", type=int, default=1200)
    parser.add_argument("--post-write-delay-ms", type=int, default=30)
    parser.add_argument("--read-timeout-ms", type=int, default=10000)
    parser.add_argument("--max-read-errors", type=int, default=1)
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if args.frames <= 0:
        raise SystemExit("--frames must be > 0")
    if args.read_timeout_ms <= 0:
        raise SystemExit("--read-timeout-ms must be > 0")
    if args.max_read_errors <= 0:
        raise SystemExit("--max-read-errors must be > 0")

    out_dir = pathlib.Path(args.out_dir)
    raw_dir = out_dir / "raw"
    raw_dir.mkdir(parents=True, exist_ok=True)
    raw_path = raw_dir / "oe1022d.rall"
    index_path = raw_dir / "oe1022d.frames.idx.jsonl"
    events_path = out_dir / "events.jsonl"
    summary_path = out_dir / "summary.json"

    started_at = now_ts()
    start_ns = time.monotonic_ns()
    counter_audit = CounterAudit()
    frames_ok = 0
    read_attempts = 0
    read_errors = 0
    timeout_count = 0
    raw_len_bad_count = 0
    raw_offset = 0

    with serial.Serial(
        port=args.port,
        baudrate=921600,
        bytesize=8,
        parity="N",
        stopbits=1,
        timeout=args.read_timeout_ms / 1000.0,
        write_timeout=args.read_timeout_ms / 1000.0,
        xonxoff=False,
        rtscts=False,
        dsrdtr=False,
    ) as ser:
        ser.reset_input_buffer()
        ser.reset_output_buffer()
        with raw_path.open("wb") as raw_file, index_path.open("w") as index_file, events_path.open(
            "w"
        ) as events_file:
            while frames_ok < args.frames:
                read_attempts += 1
                read_start = time.monotonic_ns()
                ser.write(b"RALL?\r")
                ser.flush()
                time.sleep(args.post_write_delay_ms / 1000.0)
                payload = ser.read(FRAME_BYTES)
                read_elapsed_ms = (time.monotonic_ns() - read_start) / 1_000_000.0

                if len(payload) != FRAME_BYTES:
                    read_errors += 1
                    if len(payload) == 0:
                        timeout_count += 1
                    else:
                        raw_len_bad_count += 1
                    events_file.write(
                        json.dumps(
                            {
                                "ts": now_ts(),
                                "monotonic_ns": monotonic_ns(start_ns),
                                "event": "rall_read_error",
                                "data": {
                                    "read_attempt": read_attempts,
                                    "frames_ok": frames_ok,
                                    "raw_len": len(payload),
                                    "elapsed_ms": read_elapsed_ms,
                                },
                            },
                            separators=(",", ":"),
                        )
                        + "\n"
                    )
                    events_file.flush()
                    if read_errors >= args.max_read_errors:
                        break
                    continue

                frame_seq = frames_ok
                frame_ns = monotonic_ns(start_ns)
                raw_file.write(payload)
                raw_file.flush()
                index_file.write(
                    json.dumps(
                        {
                            "frame_seq": frame_seq,
                            "ts": now_ts(),
                            "monotonic_ns": frame_ns,
                            "raw_offset": raw_offset,
                            "raw_len": len(payload),
                            "parse_status": "not_parsed",
                            "duplicate_of": None,
                        },
                        separators=(",", ":"),
                    )
                    + "\n"
                )
                index_file.flush()
                counter_audit.record(frame_ns, payload[COUNTER_OFFSET])
                raw_offset += len(payload)
                frames_ok += 1

    summary = {
        "tool": "pyserial_probe",
        "port_path": args.port,
        "baud_rate": 921600,
        "command": "RALL?",
        "frame_bytes": FRAME_BYTES,
        "post_write_delay_ms": args.post_write_delay_ms,
        "read_timeout_ms": args.read_timeout_ms,
        "max_read_errors": args.max_read_errors,
        "started_at": started_at,
        "ended_at": now_ts(),
        "elapsed_ms": (time.monotonic_ns() - start_ns) / 1_000_000.0,
        "frames_requested": args.frames,
        "frames_ok": frames_ok,
        "read_attempts": read_attempts,
        "read_errors": read_errors,
        "timeout_count": timeout_count,
        "writer": {
            "frames_written": frames_ok,
            "raw_len_bad_count": raw_len_bad_count,
            "packet_counter": counter_audit.finish(),
        },
        "raw_file": "raw/oe1022d.rall",
        "index_file": "raw/oe1022d.frames.idx.jsonl",
    }
    summary_path.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(
        "pyserial OE RALL probe complete: "
        f"frames_ok={frames_ok}, timeout_count={timeout_count}, "
        f"counter_delta_gt1={summary['writer']['packet_counter']['delta_gt1_count']}, "
        f"out_dir={out_dir}"
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
