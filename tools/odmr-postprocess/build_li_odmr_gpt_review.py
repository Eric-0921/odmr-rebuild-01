#!/usr/bin/env python3
"""Build a compact LI-ODMR CSV for GPT web review.

The output is intended for human/GPT inspection of resonance peaks and
zero-crossing-like structures after adding a small bias magnet. It consumes
existing run artifacts and writes only to the run's postprocess directory.
"""

from __future__ import annotations

import argparse
import csv
import json
import math
import subprocess
import sys
from pathlib import Path
from typing import Any


def load_json(path: Path, default: Any = None) -> Any:
    if not path.exists():
        return default
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def load_jsonl(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    rows: list[dict[str, Any]] = []
    with path.open("r", encoding="utf-8") as handle:
        for line in handle:
            text = line.strip()
            if text:
                rows.append(json.loads(text))
    return rows


def safe_json_dumps(row: dict[str, Any]) -> str:
    def clean(value: Any) -> Any:
        if isinstance(value, dict):
            return {str(k): clean(v) for k, v in value.items()}
        if isinstance(value, list):
            return [clean(v) for v in value]
        if isinstance(value, tuple):
            return [clean(v) for v in value]
        if isinstance(value, float) and not math.isfinite(value):
            return None
        return value

    return json.dumps(clean(row), ensure_ascii=False, indent=2)


def ensure_point_fields(run_dir: Path, allow_extract: bool) -> None:
    point_fields = run_dir / "point_fields.jsonl"
    if point_fields.exists() and any((run_dir / "point_fields").glob("*.npz")):
        return
    if not allow_extract:
        raise FileNotFoundError(f"{run_dir} has no point_fields sidecar; rerun with --extract-missing-point-fields")

    script = Path(__file__).with_name("extract_point_fields_from_rall.py")
    subprocess.run([sys.executable, str(script), "--run", str(run_dir)], check=True)


def build_frequency_grid(start_hz: float, stop_hz: float, step_hz: float) -> list[float]:
    if step_hz == 0:
        return []
    direction = 1.0 if stop_hz >= start_hz else -1.0
    step = abs(step_hz) * direction
    count = int(math.floor(abs(stop_hz - start_hz) / abs(step_hz) + 1e-9)) + 1
    return [start_hz + i * step for i in range(max(0, count))]


def moving_average(np: Any, values: Any, window: int) -> Any:
    arr = np.asarray(values, dtype=float)
    if window <= 1 or arr.size < window:
        return arr.copy()
    return np.convolve(arr, np.ones(window) / window, mode="same")


def robust_z(np: Any, values: Any) -> Any:
    arr = np.asarray(values, dtype=float)
    median = np.nanmedian(arr)
    scale = np.nanmedian(np.abs(arr - median)) * 1.4826
    if not math.isfinite(float(scale)) or scale == 0:
        std = np.nanstd(arr)
        return (arr - np.nanmean(arr)) / std if std else arr * 0
    return (arr - median) / scale


def linear_detrend(np: Any, freq_hz: Any, values: Any) -> tuple[Any, list[float | None]]:
    freq = np.asarray(freq_hz, dtype=float)
    arr = np.asarray(values, dtype=float)
    mask = np.isfinite(freq) & np.isfinite(arr)
    if mask.sum() < 3:
        return arr - np.nanmedian(arr), [None, None]
    coef = np.polyfit(freq[mask], arr[mask], 1)
    return arr - np.polyval(coef, freq), [float(coef[0]), float(coef[1])]


def fold_trace(np: Any, values: Any, freq_count: int, samples_per_freq: int) -> tuple[Any, Any, Any, int, int]:
    arr = np.asarray(values, dtype=float).ravel()
    expected = freq_count * samples_per_freq
    cycles = arr.size // expected if expected else 0
    if cycles > 0:
        cube = arr[: cycles * expected].reshape(cycles, freq_count, samples_per_freq)
        mean = cube.mean(axis=(0, 2))
        std = cube.reshape(cycles, freq_count, samples_per_freq).std(axis=(0, 2), ddof=1)
        count = np.full(freq_count, cycles * samples_per_freq, dtype=int)
        return mean, std, count, cycles, int(arr.size - cycles * expected)

    means = []
    stds = []
    counts = []
    for index in range(freq_count):
        chunk = arr[index * samples_per_freq : min((index + 1) * samples_per_freq, arr.size)]
        if chunk.size == 0:
            break
        means.append(float(np.mean(chunk)))
        stds.append(float(np.std(chunk, ddof=1)) if chunk.size > 1 else math.nan)
        counts.append(int(chunk.size))
    return np.asarray(means), np.asarray(stds), np.asarray(counts), 0, int(arr.size)


def peak_hints(values: list[float]) -> list[str]:
    hints: list[str] = []
    for index, value in enumerate(values):
        if index == 0 or index == len(values) - 1:
            hints.append("edge")
        elif value > values[index - 1] and value > values[index + 1]:
            hints.append("local_max")
        elif value < values[index - 1] and value < values[index + 1]:
            hints.append("local_min")
        else:
            hints.append("")
    return hints


def target_b(point: dict[str, Any]) -> list[Any]:
    values = list(point.get("target_b_nt") or [])
    values.extend([None, None, None])
    return values[:3]


def write_review_csv(args: argparse.Namespace) -> int:
    try:
        import numpy as np
    except ImportError:
        print("numpy is required: python3 -m pip install numpy", file=sys.stderr)
        return 2

    run_dir = Path(args.run).resolve()
    ensure_point_fields(run_dir, args.extract_missing_point_fields)

    points = load_jsonl(run_dir / "points.jsonl")
    point_fields = load_jsonl(run_dir / "point_fields.jsonl")
    qualities = load_jsonl(run_dir / "quality.jsonl")
    if not points:
        raise ValueError(f"missing points.jsonl rows in {run_dir}")
    if not point_fields:
        raise ValueError(f"missing point_fields.jsonl rows in {run_dir}")

    smb = load_json(run_dir / "smb_profile_snapshot.json", {})
    oe = load_json(run_dir / "oe_profile_snapshot.json", {})
    laser = load_json(run_dir / "laser_profile_snapshot.json", {})
    summary = load_json(run_dir / "summary.json", {})

    point = points[0]
    field_row = point_fields[0]
    quality = qualities[0] if qualities else {}
    rf = dict(smb.get("default_sweep") or {})
    rf.update(point.get("rf") or {})

    start_hz = float(rf["start_hz"])
    stop_hz = float(rf["stop_hz"])
    step_hz = float(rf["step_hz"])
    dwell_ms = float(rf["dwell_ms"])
    frequency_hz = build_frequency_grid(start_hz, stop_hz, step_hz)
    samples_per_freq = max(1, int(round(dwell_ms / args.sample_interval_ms)))

    sidecar_rel = (field_row.get("sidecar") or {}).get("relative_path")
    if not sidecar_rel:
        raise ValueError("point_fields row has no sidecar.relative_path")
    sidecar_path = run_dir / sidecar_rel

    with np.load(sidecar_path, allow_pickle=False) as data:
        b_x_mean, b_x_std, sample_count, cycles, leftover = fold_trace(
            np, data["b_x"], len(frequency_hz), samples_per_freq
        )
        b_y_mean, b_y_std, _, _, _ = fold_trace(np, data["b_y"], len(frequency_hz), samples_per_freq)
        b_noise_mean, _, _, _, _ = fold_trace(np, data["b_noise"], len(frequency_hz), samples_per_freq)

    frequency_hz_arr = np.asarray(frequency_hz[: b_x_mean.size], dtype=float)
    b_r_mean = np.sqrt(b_x_mean * b_x_mean + b_y_mean * b_y_mean)

    b_x_detrended, b_x_trend = linear_detrend(np, frequency_hz_arr, b_x_mean)
    b_y_detrended, b_y_trend = linear_detrend(np, frequency_hz_arr, b_y_mean)
    b_r_detrended, b_r_trend = linear_detrend(np, frequency_hz_arr, b_r_mean)

    b_x_smooth9 = moving_average(np, b_x_detrended, 9)
    b_y_smooth9 = moving_average(np, b_y_detrended, 9)
    b_r_smooth9 = moving_average(np, b_r_detrended, 9)
    b_x_smooth21 = moving_average(np, b_x_detrended, 21)
    b_y_smooth21 = moving_average(np, b_y_detrended, 21)
    b_r_smooth21 = moving_average(np, b_r_detrended, 21)

    b_x_z = robust_z(np, b_x_detrended)
    b_y_z = robust_z(np, b_y_detrended)
    b_r_z = robust_z(np, b_r_detrended)
    b_x_smooth9_z = robust_z(np, b_x_smooth9)
    b_y_smooth9_z = robust_z(np, b_y_smooth9)
    b_r_smooth9_z = robust_z(np, b_r_smooth9)
    hints = peak_hints([float(v) for v in b_x_smooth9])

    output_dir = Path(args.out).resolve() if args.out else run_dir / "postprocess"
    output_dir.mkdir(parents=True, exist_ok=True)
    suffix = args.name_suffix or run_dir.name
    csv_path = output_dir / f"li_odmr_gpt_review_{suffix}.csv"
    summary_path = output_dir / f"li_odmr_gpt_review_{suffix}_summary.json"

    labels = target_b(point)
    fieldnames = [
        "source_run",
        "point_id",
        "frequency_index",
        "frequency_hz",
        "frequency_ghz",
        "b_x_mean",
        "b_y_mean",
        "b_r_mean",
        "b_noise_mean",
        "b_x_std",
        "b_y_std",
        "sample_count",
        "b_x_detrended",
        "b_y_detrended",
        "b_r_detrended",
        "b_x_smooth9",
        "b_y_smooth9",
        "b_r_smooth9",
        "b_x_smooth21",
        "b_y_smooth21",
        "b_r_smooth21",
        "b_x_z",
        "b_y_z",
        "b_r_z",
        "b_x_smooth9_z",
        "b_y_smooth9_z",
        "b_r_smooth9_z",
        "peak_hint_b_x_smooth9",
        "target_bx_nt",
        "target_by_nt",
        "target_bz_nt",
        "rf_power_dbm",
        "rf_dwell_ms",
        "laser_mode",
        "laser_power_mw",
        "b_input_overload_ratio",
        "b_gain_overload_ratio",
        "b_pll_locked_ratio",
    ]

    with csv_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for index in range(frequency_hz_arr.size):
            writer.writerow(
                {
                    "source_run": run_dir.name,
                    "point_id": point.get("point_id"),
                    "frequency_index": index,
                    "frequency_hz": f"{frequency_hz_arr[index]:.0f}",
                    "frequency_ghz": f"{frequency_hz_arr[index] / 1e9:.9f}",
                    "b_x_mean": f"{b_x_mean[index]:.12e}",
                    "b_y_mean": f"{b_y_mean[index]:.12e}",
                    "b_r_mean": f"{b_r_mean[index]:.12e}",
                    "b_noise_mean": f"{b_noise_mean[index]:.12e}",
                    "b_x_std": f"{b_x_std[index]:.12e}",
                    "b_y_std": f"{b_y_std[index]:.12e}",
                    "sample_count": int(sample_count[index]),
                    "b_x_detrended": f"{b_x_detrended[index]:.12e}",
                    "b_y_detrended": f"{b_y_detrended[index]:.12e}",
                    "b_r_detrended": f"{b_r_detrended[index]:.12e}",
                    "b_x_smooth9": f"{b_x_smooth9[index]:.12e}",
                    "b_y_smooth9": f"{b_y_smooth9[index]:.12e}",
                    "b_r_smooth9": f"{b_r_smooth9[index]:.12e}",
                    "b_x_smooth21": f"{b_x_smooth21[index]:.12e}",
                    "b_y_smooth21": f"{b_y_smooth21[index]:.12e}",
                    "b_r_smooth21": f"{b_r_smooth21[index]:.12e}",
                    "b_x_z": f"{b_x_z[index]:.9f}",
                    "b_y_z": f"{b_y_z[index]:.9f}",
                    "b_r_z": f"{b_r_z[index]:.9f}",
                    "b_x_smooth9_z": f"{b_x_smooth9_z[index]:.9f}",
                    "b_y_smooth9_z": f"{b_y_smooth9_z[index]:.9f}",
                    "b_r_smooth9_z": f"{b_r_smooth9_z[index]:.9f}",
                    "peak_hint_b_x_smooth9": hints[index],
                    "target_bx_nt": labels[0],
                    "target_by_nt": labels[1],
                    "target_bz_nt": labels[2],
                    "rf_power_dbm": rf.get("power_dbm"),
                    "rf_dwell_ms": rf.get("dwell_ms"),
                    "laser_mode": laser.get("mode"),
                    "laser_power_mw": laser.get("power_mw"),
                    "b_input_overload_ratio": field_row.get("b_input_overload_ratio"),
                    "b_gain_overload_ratio": field_row.get("b_gain_overload_ratio"),
                    "b_pll_locked_ratio": field_row.get("b_pll_locked_ratio"),
                }
            )

    summary_row = {
        "source_run": run_dir.name,
        "purpose": "li_odmr_bias_magnet_zero_crossing_resonance_peak_review",
        "recommended_gpt_columns": [
            "frequency_ghz",
            "b_x_smooth9_z",
            "b_y_smooth9_z",
            "b_r_smooth9_z",
            "peak_hint_b_x_smooth9",
        ],
        "notes": [
            "B-X/B-Y are lock-in outputs, not fluorescence intensity.",
            "Use detrended and smoothed columns for visual/GPT peak review.",
            "Do not infer sample validity from quality_status alone; inspect overload ratios.",
        ],
        "rf": {key: rf.get(key) for key in ["start_hz", "stop_hz", "step_hz", "dwell_ms", "power_dbm"]},
        "laser": laser,
        "oe_fixed": oe.get("fixed"),
        "quality": quality,
        "point_fields_overload": {
            "b_input_overload_ratio": field_row.get("b_input_overload_ratio"),
            "b_gain_overload_ratio": field_row.get("b_gain_overload_ratio"),
            "b_pll_locked_ratio": field_row.get("b_pll_locked_ratio"),
        },
        "folding": {
            "frequency_bins": int(frequency_hz_arr.size),
            "samples_per_frequency": int(samples_per_freq),
            "complete_sweeps_used": int(cycles),
            "leftover_samples_ignored": int(leftover),
        },
        "trend_coefficients": {
            "b_x_linear": b_x_trend,
            "b_y_linear": b_y_trend,
            "b_r_linear": b_r_trend,
        },
        "summary": summary,
        "outputs": {
            "csv": str(csv_path),
            "summary_json": str(summary_path),
        },
    }
    summary_path.write_text(safe_json_dumps(summary_row) + "\n", encoding="utf-8")

    print(f"wrote {frequency_hz_arr.size} rows: {csv_path}")
    print(f"wrote summary: {summary_path}")
    return 0


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build LI-ODMR GPT review CSV from a run directory.")
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument("--out", help="Output directory. Defaults to <run>/postprocess.")
    parser.add_argument("--name-suffix", help="Output filename suffix. Defaults to the run directory name.")
    parser.add_argument("--sample-interval-ms", type=float, default=1.0)
    parser.add_argument(
        "--extract-missing-point-fields",
        action="store_true",
        help="If point_fields sidecars are missing, reconstruct them from raw/frames.idx/segments first.",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    if args.sample_interval_ms <= 0:
        print("--sample-interval-ms must be positive", file=sys.stderr)
        return 2
    return write_review_csv(args)


if __name__ == "__main__":
    raise SystemExit(main())
