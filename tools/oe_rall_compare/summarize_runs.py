#!/usr/bin/env python3
"""Print comparable OE RALL probe metrics from one or more run directories."""

from __future__ import annotations

import argparse
import json
import pathlib


def read_summary(run_dir: pathlib.Path) -> dict:
    path = run_dir / "summary.json"
    return json.loads(path.read_text(encoding="utf-8"))


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("run_dirs", nargs="+")
    args = parser.parse_args()

    rows = []
    for item in args.run_dirs:
        run_dir = pathlib.Path(item)
        summary = read_summary(run_dir)
        counter = summary["writer"]["packet_counter"]
        rows.append(
            {
                "run": str(run_dir),
                "tool": summary.get("tool", "rust_odmr_cli"),
                "frames_ok": summary["frames_ok"],
                "read_errors": summary["read_errors"],
                "timeout_count": summary["timeout_count"],
                "raw_len_bad": summary["writer"]["raw_len_bad_count"],
                "delta_gt1": counter["delta_gt1_count"],
                "missing_windows": counter["estimated_missing_windows"],
                "gap_median_ms": counter["gap_median_ms"],
                "gap_max_ms": counter["gap_max_ms"],
                "elapsed_ms": summary["elapsed_ms"],
            }
        )

    print(
        "tool\trun\tframes_ok\tread_errors\ttimeout_count\traw_len_bad\t"
        "delta_gt1\tmissing_windows\tgap_median_ms\tgap_max_ms\telapsed_ms"
    )
    for row in rows:
        print(
            f"{row['tool']}\t{row['run']}\t{row['frames_ok']}\t{row['read_errors']}\t"
            f"{row['timeout_count']}\t{row['raw_len_bad']}\t{row['delta_gt1']}\t"
            f"{row['missing_windows']}\t{row['gap_median_ms']}\t{row['gap_max_ms']}\t"
            f"{row['elapsed_ms']}"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
