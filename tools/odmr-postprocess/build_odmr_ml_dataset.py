#!/usr/bin/env python3
"""Build ML-ready ODMR datasets from rebuild run artifacts."""

from __future__ import annotations

import argparse
import csv
import json
import math
import sys
from pathlib import Path
from typing import Any

from build_li_odmr_gpt_review import (
    build_frequency_grid,
    ensure_point_fields,
    fold_trace,
    linear_detrend,
    load_json,
    load_jsonl,
    moving_average,
    robust_z,
    safe_json_dumps,
    target_b,
)


SCHEMA_VERSION = 1
DEFAULT_SAMPLE_INTERVAL_MS = 1.0


def point_id(row: dict[str, Any]) -> str:
    value = row.get("point_id")
    if value is None:
        raise ValueError("row missing point_id")
    return str(value)


def index_by_point(rows: list[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {point_id(row): row for row in rows}


def resolve_rf(point: dict[str, Any], smb: dict[str, Any]) -> dict[str, Any]:
    rf = dict(smb.get("default_sweep") or {})
    rf.update(point.get("rf") or {})
    return rf


def numeric(value: Any, default: float = math.nan) -> float:
    if value is None:
        return default
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def finite_float(value: float) -> float | str:
    return value if math.isfinite(value) else ""


def edge_noise(np: Any, values: Any, edge_fraction: float = 0.05) -> float:
    arr = np.asarray(values, dtype=float)
    if arr.size == 0:
        return math.nan
    edge_count = max(1, int(math.ceil(arr.size * edge_fraction)))
    edge_count = min(edge_count, max(1, arr.size // 2))
    edges = np.concatenate([arr[:edge_count], arr[-edge_count:]])
    median = np.nanmedian(edges)
    mad = np.nanmedian(np.abs(edges - median)) * 1.4826
    return float(mad)


def zero_crossing_count(np: Any, values: Any) -> int:
    arr = np.asarray(values, dtype=float)
    arr = arr[np.isfinite(arr)]
    if arr.size < 2:
        return 0
    signs = np.sign(arr)
    signs = signs[signs != 0]
    if signs.size < 2:
        return 0
    return int(np.sum(signs[1:] != signs[:-1]))


def strongest_slope_freq(np: Any, frequency_hz: Any, values: Any) -> tuple[float, float]:
    freq = np.asarray(frequency_hz, dtype=float)
    arr = np.asarray(values, dtype=float)
    if arr.size < 2 or freq.size < 2:
        return math.nan, math.nan
    slope = np.gradient(arr, freq)
    if not np.isfinite(slope).any():
        return math.nan, math.nan
    index = int(np.nanargmax(np.abs(slope)))
    return float(freq[index]), float(slope[index])


def peak_features(np: Any, frequency_hz: Any, values: Any, prefix: str) -> dict[str, float | int | str]:
    freq = np.asarray(frequency_hz, dtype=float)
    arr = np.asarray(values, dtype=float)
    if arr.size == 0 or freq.size == 0 or not np.isfinite(arr).any():
        return {
            f"{prefix}_max_freq_hz": "",
            f"{prefix}_max_value": "",
            f"{prefix}_min_freq_hz": "",
            f"{prefix}_min_value": "",
            f"{prefix}_max_abs_freq_hz": "",
            f"{prefix}_max_abs_value": "",
            f"{prefix}_zero_crossings": 0,
            f"{prefix}_strongest_slope_freq_hz": "",
            f"{prefix}_strongest_slope": "",
        }
    max_index = int(np.nanargmax(arr))
    min_index = int(np.nanargmin(arr))
    abs_index = int(np.nanargmax(np.abs(arr)))
    slope_freq, slope = strongest_slope_freq(np, freq, arr)
    return {
        f"{prefix}_max_freq_hz": float(freq[max_index]),
        f"{prefix}_max_value": float(arr[max_index]),
        f"{prefix}_min_freq_hz": float(freq[min_index]),
        f"{prefix}_min_value": float(arr[min_index]),
        f"{prefix}_max_abs_freq_hz": float(freq[abs_index]),
        f"{prefix}_max_abs_value": float(arr[abs_index]),
        f"{prefix}_zero_crossings": zero_crossing_count(np, arr),
        f"{prefix}_strongest_slope_freq_hz": finite_float(slope_freq),
        f"{prefix}_strongest_slope": finite_float(slope),
    }


def quality_weight(field_row: dict[str, Any]) -> float:
    input_overload = numeric(field_row.get("b_input_overload_ratio"), 0.0)
    gain_overload = numeric(field_row.get("b_gain_overload_ratio"), 0.0)
    pll_locked = numeric(field_row.get("b_pll_locked_ratio"), 1.0)
    if input_overload > 0 or gain_overload > 0:
        return 0.0
    if pll_locked < 1.0:
        return max(0.0, pll_locked)
    return 1.0


def build_dataset(args: argparse.Namespace) -> int:
    try:
        import numpy as np
    except ImportError:
        print("numpy is required: python -m pip install numpy", file=sys.stderr)
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

    fields_by_point = index_by_point(point_fields)
    qualities_by_point = index_by_point(qualities) if qualities else {}
    smb = load_json(run_dir / "smb_profile_snapshot.json", {})
    oe = load_json(run_dir / "oe_profile_snapshot.json", {})
    laser = load_json(run_dir / "laser_profile_snapshot.json", {})

    output_dir = Path(args.out).resolve() if args.out else run_dir / "postprocess"
    output_dir.mkdir(parents=True, exist_ok=True)
    suffix = args.name_suffix or run_dir.name

    rows: list[dict[str, Any]] = []
    arrays: dict[str, list[Any]] = {
        "X_bx_mean": [],
        "X_by_mean": [],
        "X_br_mean": [],
        "X_bnoise_mean": [],
        "X_bx_detrended": [],
        "X_by_detrended": [],
        "X_br_detrended": [],
        "X_bx_smooth_z": [],
        "X_by_smooth_z": [],
        "X_br_smooth_z": [],
        "sample_count": [],
    }
    point_ids: list[str] = []
    target_b_nt: list[list[float]] = []
    sample_weight: list[float] = []
    frequency_ref: Any = None
    skipped: list[dict[str, Any]] = []

    for point in points:
        pid = point_id(point)
        field_row = fields_by_point.get(pid)
        if field_row is None:
            skipped.append({"point_id": pid, "reason": "missing_point_fields"})
            continue

        rf = resolve_rf(point, smb)
        try:
            frequency_hz = build_frequency_grid(float(rf["start_hz"]), float(rf["stop_hz"]), float(rf["step_hz"]))
            samples_per_freq = max(1, int(round(float(rf["dwell_ms"]) / args.sample_interval_ms)))
        except (KeyError, TypeError, ValueError) as exc:
            skipped.append({"point_id": pid, "reason": f"bad_rf:{exc}"})
            continue
        if not frequency_hz:
            skipped.append({"point_id": pid, "reason": "empty_frequency_grid"})
            continue

        sidecar_rel = (field_row.get("sidecar") or {}).get("relative_path")
        if not sidecar_rel:
            skipped.append({"point_id": pid, "reason": "missing_sidecar_path"})
            continue
        sidecar_path = run_dir / str(sidecar_rel)
        if not sidecar_path.exists():
            skipped.append({"point_id": pid, "reason": "sidecar_not_found"})
            continue

        with np.load(sidecar_path, allow_pickle=False) as data:
            bx_mean, bx_std, count, cycles, leftover = fold_trace(np, data["b_x"], len(frequency_hz), samples_per_freq)
            by_mean, by_std, _, _, _ = fold_trace(np, data["b_y"], len(frequency_hz), samples_per_freq)
            bnoise_mean, _, _, _, _ = fold_trace(np, data["b_noise"], len(frequency_hz), samples_per_freq)

        freq = np.asarray(frequency_hz[: bx_mean.size], dtype=float)
        if frequency_ref is None:
            frequency_ref = freq
        elif freq.size != frequency_ref.size or not np.allclose(freq, frequency_ref, rtol=0.0, atol=0.5):
            skipped.append({"point_id": pid, "reason": "frequency_grid_mismatch"})
            continue

        br_mean = np.sqrt(bx_mean * bx_mean + by_mean * by_mean)
        bx_detrended, _ = linear_detrend(np, freq, bx_mean)
        by_detrended, _ = linear_detrend(np, freq, by_mean)
        br_detrended, _ = linear_detrend(np, freq, br_mean)
        bx_smooth = moving_average(np, bx_detrended, args.smooth_window)
        by_smooth = moving_average(np, by_detrended, args.smooth_window)
        br_smooth = moving_average(np, br_detrended, args.smooth_window)
        bx_smooth_z = robust_z(np, bx_smooth)
        by_smooth_z = robust_z(np, by_smooth)
        br_smooth_z = robust_z(np, br_smooth)

        labels = [numeric(v) for v in target_b(point)]
        weight = quality_weight(field_row)
        noise = edge_noise(np, bx_detrended)
        span = float(np.nanmax(bx_detrended) - np.nanmin(bx_detrended)) if bx_detrended.size else math.nan
        snr = span / noise if math.isfinite(span) and math.isfinite(noise) and noise != 0 else math.nan

        row: dict[str, Any] = {
            "source_run": run_dir.name,
            "point_id": pid,
            "point_index": len(point_ids),
            "target_bx_nt": labels[0],
            "target_by_nt": labels[1],
            "target_bz_nt": labels[2],
            "rf_start_hz": rf.get("start_hz"),
            "rf_stop_hz": rf.get("stop_hz"),
            "rf_step_hz": rf.get("step_hz"),
            "rf_dwell_ms": rf.get("dwell_ms"),
            "rf_power_dbm": rf.get("power_dbm"),
            "laser_mode": laser.get("mode"),
            "laser_power_mw": laser.get("power_mw"),
            "b_input_overload_ratio": field_row.get("b_input_overload_ratio"),
            "b_gain_overload_ratio": field_row.get("b_gain_overload_ratio"),
            "b_pll_locked_ratio": field_row.get("b_pll_locked_ratio"),
            "quality_status": qualities_by_point.get(pid, {}).get("quality_status"),
            "sample_weight": weight,
            "frequency_bins": int(freq.size),
            "samples_per_frequency": int(samples_per_freq),
            "complete_sweeps_used": int(cycles),
            "leftover_samples_ignored": int(leftover),
            "edge_noise_bx": finite_float(noise),
            "span_bx_detrended": finite_float(span),
            "snr_like_bx": finite_float(snr),
            "sidecar_npz": str(sidecar_rel),
        }
        row.update(peak_features(np, freq, bx_smooth_z, "bx_smooth_z"))
        row.update(peak_features(np, freq, by_smooth_z, "by_smooth_z"))
        row.update(peak_features(np, freq, br_smooth_z, "br_smooth_z"))

        rows.append(row)
        point_ids.append(pid)
        target_b_nt.append(labels)
        sample_weight.append(weight)
        arrays["X_bx_mean"].append(bx_mean)
        arrays["X_by_mean"].append(by_mean)
        arrays["X_br_mean"].append(br_mean)
        arrays["X_bnoise_mean"].append(bnoise_mean)
        arrays["X_bx_detrended"].append(bx_detrended)
        arrays["X_by_detrended"].append(by_detrended)
        arrays["X_br_detrended"].append(br_detrended)
        arrays["X_bx_smooth_z"].append(bx_smooth_z)
        arrays["X_by_smooth_z"].append(by_smooth_z)
        arrays["X_br_smooth_z"].append(br_smooth_z)
        arrays["sample_count"].append(count)

    if frequency_ref is None or not rows:
        raise ValueError("no ML samples were built")

    csv_path = output_dir / f"ml_samples_{suffix}.csv"
    jsonl_path = output_dir / f"ml_samples_{suffix}.jsonl"
    npz_path = output_dir / f"ml_dataset_{suffix}.npz"
    summary_path = output_dir / f"ml_dataset_{suffix}_summary.json"

    fieldnames = list(rows[0].keys())
    with csv_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)
    with jsonl_path.open("w", encoding="utf-8") as handle:
        for row in rows:
            handle.write(safe_json_dumps(row) + "\n")

    np.savez_compressed(
        npz_path,
        schema_version=np.asarray([SCHEMA_VERSION], dtype=np.int32),
        source_run=np.asarray([run_dir.name]),
        point_id=np.asarray(point_ids),
        frequency_hz=np.asarray(frequency_ref, dtype=np.float64),
        target_b_nt=np.asarray(target_b_nt, dtype=np.float64),
        sample_weight=np.asarray(sample_weight, dtype=np.float32),
        **{key: np.asarray(value) for key, value in arrays.items()},
    )

    summary = {
        "schema_version": SCHEMA_VERSION,
        "source_run": run_dir.name,
        "purpose": "ml_ready_odmr_cartesian_scan_dataset",
        "sample_count": len(rows),
        "skipped_count": len(skipped),
        "frequency_bins": int(frequency_ref.size),
        "recommended_input_arrays": ["X_bx_smooth_z", "X_by_smooth_z", "X_br_smooth_z"],
        "label_array": "target_b_nt",
        "sample_weight_array": "sample_weight",
        "notes": [
            "Each row/sample is one point in the field scan.",
            "B-X/B-Y are lock-in outputs; smooth_z arrays are detrended, smoothed, and robust-z normalized per point.",
            "Use source_run or future group fields for train/test split to avoid leakage across neighboring points.",
        ],
        "rf_reference": rows[0],
        "laser": laser,
        "oe_fixed": oe.get("fixed"),
        "skipped": skipped,
        "outputs": {
            "npz": str(npz_path),
            "samples_csv": str(csv_path),
            "samples_jsonl": str(jsonl_path),
            "summary_json": str(summary_path),
        },
    }
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print(f"wrote {len(rows)} ML samples: {csv_path}")
    print(f"wrote arrays: {npz_path}")
    print(f"wrote summary: {summary_path}")
    if skipped:
        print(f"skipped {len(skipped)} samples", file=sys.stderr)
    return 0


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build ML-ready ODMR dataset from a run directory.")
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument("--out", help="Output directory. Defaults to <run>/postprocess.")
    parser.add_argument("--name-suffix", help="Output filename suffix. Defaults to the run directory name.")
    parser.add_argument("--sample-interval-ms", type=float, default=DEFAULT_SAMPLE_INTERVAL_MS)
    parser.add_argument("--smooth-window", type=int, default=9)
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
    if args.smooth_window <= 0:
        print("--smooth-window must be positive", file=sys.stderr)
        return 2
    return build_dataset(args)


if __name__ == "__main__":
    raise SystemExit(main())
