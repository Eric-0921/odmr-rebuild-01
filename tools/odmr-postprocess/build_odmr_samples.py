#!/usr/bin/env python3
"""Build reference ODMR postprocess samples from rebuild run artifacts."""

from __future__ import annotations

import argparse
import csv
import json
import math
import sys
from pathlib import Path
from typing import Any


SCHEMA_VERSION = 1
DEFAULT_SAMPLE_INTERVAL_MS = 1.0
DEFAULT_LABEL_SOURCE = "helmholtz_setpoint_calibrated"


def load_json(path: Path, default: Any = None) -> Any:
    if not path.exists():
        return default
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def load_jsonl(path: Path) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    if not path.exists():
        return rows
    with path.open("r", encoding="utf-8") as handle:
        for line_no, line in enumerate(handle, start=1):
            text = line.strip()
            if not text:
                continue
            try:
                rows.append(json.loads(text))
            except json.JSONDecodeError as exc:
                raise ValueError(f"{path}:{line_no}: invalid JSONL row") from exc
    return rows


def point_id(row: dict[str, Any]) -> str:
    value = row.get("point_id")
    if value is None:
        raise ValueError("row missing point_id")
    return str(value)


def index_by_point(rows: list[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {point_id(row): row for row in rows}


def numeric(value: Any, default: float = math.nan) -> float:
    if value is None:
        return default
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def finite_or_blank(value: float | int | None) -> float | int | str:
    if value is None:
        return ""
    if isinstance(value, float) and not math.isfinite(value):
        return ""
    return value


def finite_or_none(value: float | int | None) -> float | int | None:
    if value is None:
        return None
    if isinstance(value, float) and not math.isfinite(value):
        return None
    return value


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

    return json.dumps(clean(row), ensure_ascii=False, sort_keys=True)


def resolve_rf(point: dict[str, Any], smb_snapshot: dict[str, Any] | None) -> dict[str, Any]:
    rf = dict(point.get("rf") or {})
    default_sweep = {}
    if smb_snapshot:
        default_sweep = dict(smb_snapshot.get("default_sweep") or {})
    for key, value in default_sweep.items():
        rf.setdefault(key, value)
    return rf


def build_frequency_grid(start_hz: float, stop_hz: float, step_hz: float) -> list[float]:
    if not all(math.isfinite(v) for v in [start_hz, stop_hz, step_hz]):
        return []
    if step_hz == 0:
        return []
    direction = 1.0 if stop_hz >= start_hz else -1.0
    step = abs(step_hz) * direction
    span = stop_hz - start_hz
    count = int(math.floor(abs(span) / abs(step_hz) + 1e-9)) + 1
    grid = [start_hz + i * step for i in range(max(count, 0))]
    if not grid:
        return []
    last = grid[-1]
    if direction > 0 and last < stop_hz - abs(step_hz) * 1e-6:
        grid.append(stop_hz)
    if direction < 0 and last > stop_hz + abs(step_hz) * 1e-6:
        grid.append(stop_hz)
    return grid


def robust_baseline(values: list[float], edge_fraction: float) -> tuple[float, int]:
    finite = [v for v in values if math.isfinite(v)]
    if not finite:
        return math.nan, 0
    edge_count = max(1, int(math.ceil(len(values) * edge_fraction)))
    edge_count = min(edge_count, max(1, len(values) // 2))
    edges = values[:edge_count] + values[-edge_count:]
    edges = [v for v in edges if math.isfinite(v)]
    if not edges:
        return math.nan, edge_count
    edges_sorted = sorted(edges)
    mid = len(edges_sorted) // 2
    if len(edges_sorted) % 2:
        return edges_sorted[mid], edge_count
    return (edges_sorted[mid - 1] + edges_sorted[mid]) / 2.0, edge_count


def mad(values: list[float]) -> float:
    finite = [v for v in values if math.isfinite(v)]
    if not finite:
        return math.nan
    sorted_values = sorted(finite)
    mid = len(sorted_values) // 2
    median = sorted_values[mid] if len(sorted_values) % 2 else (sorted_values[mid - 1] + sorted_values[mid]) / 2.0
    deviations = sorted(abs(v - median) for v in finite)
    mid = len(deviations) // 2
    raw_mad = deviations[mid] if len(deviations) % 2 else (deviations[mid - 1] + deviations[mid]) / 2.0
    return raw_mad * 1.4826


def zscore(values: list[float]) -> list[float]:
    finite = [v for v in values if math.isfinite(v)]
    if not finite:
        return [math.nan for _ in values]
    mean = sum(finite) / len(finite)
    variance = sum((v - mean) ** 2 for v in finite) / max(1, len(finite) - 1)
    std = math.sqrt(variance)
    if std == 0 or not math.isfinite(std):
        return [math.nan for _ in values]
    return [(v - mean) / std if math.isfinite(v) else math.nan for v in values]


def target_b(point: dict[str, Any]) -> tuple[float, float, float]:
    value = point.get("target_b_nt")
    if not isinstance(value, list):
        return (math.nan, math.nan, math.nan)
    padded = list(value[:3]) + [math.nan, math.nan, math.nan]
    return (numeric(padded[0]), numeric(padded[1]), numeric(padded[2]))


def npz_path_for(run_dir: Path, field_row: dict[str, Any]) -> Path | None:
    sidecar = field_row.get("sidecar") or {}
    rel = sidecar.get("relative_path")
    if not rel:
        return None
    return run_dir / str(rel)


def load_signal(np: Any, npz_path: Path, key: str) -> list[float]:
    with np.load(npz_path, allow_pickle=False) as data:
        if key not in data:
            available = ", ".join(sorted(data.files))
            raise KeyError(f"{npz_path}: missing key {key!r}; available keys: {available}")
        arr = data[key]
        return [float(v) for v in np.ravel(arr)]


def bin_trace(
    signal: list[float],
    frequency_grid: list[float],
    dwell_ms: float,
    sample_interval_ms: float,
    sweep_offset_ms: float,
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    samples_per_frequency = max(1, int(round(dwell_ms / sample_interval_ms)))
    offset_samples = max(0, int(round(sweep_offset_ms / sample_interval_ms)))
    expected_samples = len(frequency_grid) * samples_per_frequency
    usable = signal[offset_samples : offset_samples + expected_samples]
    bins: list[dict[str, Any]] = []
    for index, frequency_hz in enumerate(frequency_grid):
        start = index * samples_per_frequency
        stop = start + samples_per_frequency
        chunk = usable[start:stop]
        chunk = [v for v in chunk if math.isfinite(v)]
        if not chunk:
            continue
        mean = sum(chunk) / len(chunk)
        if len(chunk) > 1:
            variance = sum((v - mean) ** 2 for v in chunk) / (len(chunk) - 1)
            std = math.sqrt(variance)
        else:
            std = math.nan
        bins.append(
            {
                "frequency_index": index,
                "frequency_hz": frequency_hz,
                "b_x_mean": mean,
                "b_x_std": std,
                "sample_count": len(chunk),
            }
        )
    meta = {
        "samples_per_frequency": samples_per_frequency,
        "offset_samples": offset_samples,
        "expected_sweep_samples": expected_samples,
        "usable_samples": len(usable),
        "raw_samples": len(signal),
    }
    return bins, meta


def add_normalized_signals(bins: list[dict[str, Any]], edge_fraction: float) -> dict[str, Any]:
    means = [numeric(row.get("b_x_mean")) for row in bins]
    baseline, edge_count = robust_baseline(means, edge_fraction)
    offset_removed = [v - baseline if math.isfinite(v) and math.isfinite(baseline) else math.nan for v in means]
    z_values = zscore(offset_removed)
    edge_values = means[:edge_count] + means[-edge_count:] if means else []
    edge_noise = mad(edge_values)
    finite_means = [v for v in means if math.isfinite(v)]
    if finite_means:
        span = max(finite_means) - min(finite_means)
        dip = baseline - min(finite_means) if math.isfinite(baseline) else math.nan
    else:
        span = math.nan
        dip = math.nan
    snr = span / edge_noise if math.isfinite(span) and math.isfinite(edge_noise) and edge_noise != 0 else math.nan
    for row, removed, z_value in zip(bins, offset_removed, z_values):
        mean = numeric(row.get("b_x_mean"))
        if math.isfinite(mean) and math.isfinite(baseline) and baseline != 0:
            row["signal_fit"] = mean / baseline - 1.0
        else:
            row["signal_fit"] = math.nan
        row["signal_offset_removed"] = removed
        row["signal_ml_z"] = z_value
    return {
        "baseline": baseline,
        "edge_count": edge_count,
        "edge_noise_mad": edge_noise,
        "contrast_span_est": span,
        "dip_contrast_est": dip / abs(baseline) if math.isfinite(dip) and math.isfinite(baseline) and baseline != 0 else math.nan,
        "snr_est": snr,
    }


def warning_list(
    signal_meta: dict[str, Any],
    bins: list[dict[str, Any]],
    frequency_grid: list[float],
    quality: dict[str, Any] | None,
) -> list[str]:
    warnings: list[str] = []
    if not frequency_grid:
        warnings.append("missing_frequency_grid")
    if signal_meta["raw_samples"] < signal_meta["expected_sweep_samples"]:
        warnings.append("raw_samples_less_than_expected_sweep_samples")
    if signal_meta["raw_samples"] > signal_meta["expected_sweep_samples"]:
        warnings.append("raw_samples_greater_than_expected_sweep_samples")
    if len(bins) < len(frequency_grid):
        warnings.append("partial_frequency_bins")
    if quality:
        status = str(quality.get("quality_status") or "").lower()
        if status and status not in {"ok", "good", "passed"}:
            warnings.append(f"quality_status_{status}")
        if numeric(quality.get("timeout_count"), 0.0) > 0:
            warnings.append("timeout_count_gt_zero")
        if numeric(quality.get("duplicate_count"), 0.0) > 0:
            warnings.append("duplicate_count_gt_zero")
    return warnings


def build(args: argparse.Namespace) -> int:
    try:
        import numpy as np
    except ImportError:
        print("numpy is required: python3 -m pip install numpy", file=sys.stderr)
        return 2

    run_dir = Path(args.run).resolve()
    out_dir = Path(args.out).resolve() if args.out else run_dir / "postprocess"
    out_dir.mkdir(parents=True, exist_ok=True)

    points = load_jsonl(run_dir / "points.jsonl")
    point_fields = load_jsonl(run_dir / "point_fields.jsonl")
    quality_rows = load_jsonl(run_dir / "quality.jsonl")
    smb_snapshot = load_json(run_dir / "smb_profile_snapshot.json", {})
    oe_snapshot = load_json(run_dir / "oe_profile_snapshot.json", {})
    laser_snapshot = load_json(run_dir / "laser_profile_snapshot.json", {})
    calibration_snapshot = load_json(run_dir / "calibration_snapshot.json", {})

    fields_by_point = index_by_point(point_fields)
    quality_by_point = index_by_point(quality_rows)

    spectra_path = out_dir / "odmr_spectra.csv"
    manifest_path = out_dir / "odmr_samples_manifest.jsonl"
    summary_path = out_dir / "odmr_dataset_summary.json"

    spectra_fields = [
        "schema_version",
        "run_id",
        "point_id",
        "frequency_index",
        "frequency_hz",
        "b_x_mean",
        "b_x_std",
        "sample_count",
        "signal_fit",
        "signal_offset_removed",
        "signal_ml_z",
        "label_bx_nt",
        "label_by_nt",
        "label_bz_nt",
        "label_source",
        "rf_power_dbm",
        "rf_dwell_ms",
        "quality_status",
    ]

    point_count = 0
    point_written = 0
    spectrum_rows = 0
    warning_counts: dict[str, int] = {}

    run_id = str(load_json(run_dir / "run_manifest.json", {}).get("run_id") or run_dir.name)

    with spectra_path.open("w", encoding="utf-8", newline="") as spectra_file, manifest_path.open(
        "w", encoding="utf-8"
    ) as manifest_file:
        writer = csv.DictWriter(spectra_file, fieldnames=spectra_fields)
        writer.writeheader()

        for point in points:
            point_count += 1
            pid = point_id(point)
            field_row = fields_by_point.get(pid)
            quality = quality_by_point.get(pid, {})
            rf = resolve_rf(point, smb_snapshot)
            start_hz = numeric(rf.get("start_hz"))
            stop_hz = numeric(rf.get("stop_hz"))
            step_hz = numeric(rf.get("step_hz"))
            dwell_ms = numeric(rf.get("dwell_ms"))
            frequency_grid = build_frequency_grid(start_hz, stop_hz, step_hz)
            labels = target_b(point)
            warnings: list[str] = []
            bins: list[dict[str, Any]] = []
            signal_meta = {
                "samples_per_frequency": None,
                "offset_samples": None,
                "expected_sweep_samples": 0,
                "usable_samples": 0,
                "raw_samples": 0,
            }
            norm_meta: dict[str, Any] = {}
            sidecar_path = npz_path_for(run_dir, field_row or {}) if field_row else None

            if not field_row:
                warnings.append("missing_point_fields")
            elif sidecar_path is None:
                warnings.append("missing_sidecar_path")
            elif not sidecar_path.exists():
                warnings.append("missing_sidecar_file")
            elif not math.isfinite(dwell_ms) or dwell_ms <= 0:
                warnings.append("invalid_dwell_ms")
            else:
                signal = load_signal(np, sidecar_path, args.channel_key)
                bins, signal_meta = bin_trace(
                    signal=signal,
                    frequency_grid=frequency_grid,
                    dwell_ms=dwell_ms,
                    sample_interval_ms=args.sample_interval_ms,
                    sweep_offset_ms=args.sweep_offset_ms,
                )
                norm_meta = add_normalized_signals(bins, args.edge_fraction)
                warnings.extend(warning_list(signal_meta, bins, frequency_grid, quality))

            for warning in warnings:
                warning_counts[warning] = warning_counts.get(warning, 0) + 1

            quality_status = str(quality.get("quality_status") or "")
            for row in bins:
                csv_row = {
                    "schema_version": SCHEMA_VERSION,
                    "run_id": run_id,
                    "point_id": pid,
                    "frequency_index": row.get("frequency_index"),
                    "frequency_hz": finite_or_blank(numeric(row.get("frequency_hz"))),
                    "b_x_mean": finite_or_blank(numeric(row.get("b_x_mean"))),
                    "b_x_std": finite_or_blank(numeric(row.get("b_x_std"))),
                    "sample_count": row.get("sample_count"),
                    "signal_fit": finite_or_blank(numeric(row.get("signal_fit"))),
                    "signal_offset_removed": finite_or_blank(numeric(row.get("signal_offset_removed"))),
                    "signal_ml_z": finite_or_blank(numeric(row.get("signal_ml_z"))),
                    "label_bx_nt": finite_or_blank(labels[0]),
                    "label_by_nt": finite_or_blank(labels[1]),
                    "label_bz_nt": finite_or_blank(labels[2]),
                    "label_source": args.label_source,
                    "rf_power_dbm": finite_or_blank(numeric(rf.get("power_dbm"))),
                    "rf_dwell_ms": finite_or_blank(dwell_ms),
                    "quality_status": quality_status,
                }
                writer.writerow(csv_row)
                spectrum_rows += 1

            manifest = {
                "schema_version": SCHEMA_VERSION,
                "run_id": run_id,
                "point_id": pid,
                "label_b_nt": [finite_or_none(v) for v in labels],
                "label_source": args.label_source,
                "rf": {
                    "start_hz": finite_or_none(start_hz),
                    "stop_hz": finite_or_none(stop_hz),
                    "step_hz": finite_or_none(step_hz),
                    "dwell_ms": finite_or_none(dwell_ms),
                    "power_dbm": finite_or_none(numeric(rf.get("power_dbm"))),
                },
                "sidecar": str(sidecar_path.relative_to(run_dir)) if sidecar_path and sidecar_path.exists() else None,
                "channel_key": args.channel_key,
                "frequency_count_expected": len(frequency_grid),
                "frequency_count_written": len(bins),
                "signal_meta": signal_meta,
                "normalization": {key: finite_or_none(numeric(value)) for key, value in norm_meta.items()},
                "quality_status": quality_status,
                "warnings": sorted(set(warnings)),
            }
            manifest_file.write(safe_json_dumps(manifest) + "\n")
            if bins:
                point_written += 1

    summary = {
        "schema_version": SCHEMA_VERSION,
        "run_id": run_id,
        "run_dir": str(run_dir),
        "out_dir": str(out_dir),
        "point_count": point_count,
        "point_written": point_written,
        "spectrum_rows": spectrum_rows,
        "sample_interval_ms": args.sample_interval_ms,
        "sweep_offset_ms": args.sweep_offset_ms,
        "label_source": args.label_source,
        "channel_key": args.channel_key,
        "warning_counts": warning_counts,
        "snapshots_present": {
            "smb_profile_snapshot": bool(smb_snapshot),
            "oe_profile_snapshot": bool(oe_snapshot),
            "laser_profile_snapshot": bool(laser_snapshot),
            "calibration_snapshot": bool(calibration_snapshot),
        },
        "outputs": {
            "spectra_csv": str(spectra_path),
            "manifest_jsonl": str(manifest_path),
        },
    }
    summary_path.write_text(safe_json_dumps(summary) + "\n", encoding="utf-8")
    print(f"wrote {spectrum_rows} spectrum rows for {point_written}/{point_count} points to {out_dir}")
    if warning_counts:
        print("warnings: " + ", ".join(f"{key}={value}" for key, value in sorted(warning_counts.items())))
    return 0


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build ODMR postprocess reference samples from a run directory.")
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument("--out", help="Output directory. Defaults to <run>/postprocess.")
    parser.add_argument("--channel-key", default="b_x", help="NPZ key for the ODMR signal trace.")
    parser.add_argument("--label-source", default=DEFAULT_LABEL_SOURCE, help="Magnetic field label source.")
    parser.add_argument("--sample-interval-ms", type=float, default=DEFAULT_SAMPLE_INTERVAL_MS)
    parser.add_argument("--sweep-offset-ms", type=float, default=0.0, help="Known delay before sweep samples start.")
    parser.add_argument("--edge-fraction", type=float, default=0.05, help="Fraction of frequency bins used on each edge.")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    if args.sample_interval_ms <= 0:
        print("--sample-interval-ms must be positive", file=sys.stderr)
        return 2
    if not (0 < args.edge_fraction <= 0.5):
        print("--edge-fraction must be in (0, 0.5]", file=sys.stderr)
        return 2
    return build(args)


if __name__ == "__main__":
    raise SystemExit(main())
