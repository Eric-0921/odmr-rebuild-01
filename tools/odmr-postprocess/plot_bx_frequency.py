#!/usr/bin/env python3
"""Plot B-X ODMR spectrum with time-mapped microwave frequency axis.

For each point, the RF sweep is assumed to be a linear stair from start_hz to
stop_hz with step_hz and dwell_ms. Each OE1022D sample is 1 ms apart, so the
sample index (counted from rf_exposure_started) maps directly to a frequency
bin:

    frequency_index = floor(sample_index / dwell_ms)
    frequency_hz    = start_hz + frequency_index * step_hz

This is an offline plotting tool; it does not modify runtime code.
"""

from __future__ import annotations

import argparse
import json
import math
import sys
from pathlib import Path
from typing import Any

import numpy as np
from decoded_truth import ensure_decoded_truth, load_json, load_jsonl, load_point_series_map

try:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
except ImportError as exc:  # pragma: no cover
    raise RuntimeError("matplotlib is required: python3 -m pip install matplotlib") from exc
def parse_iso_ts(ts: str) -> float:
    """Return seconds since epoch for an ISO-8601 timestamp string."""
    # Python 3.11+ has datetime.fromisoformat with Z support; keep simple here.
    from datetime import datetime, timezone
    ts = ts.replace("Z", "+00:00")
    return datetime.fromisoformat(ts).replace(tzinfo=timezone.utc).timestamp()


def find_rf_exposure_start(events: list[dict[str, Any]], point_id: str) -> float | None:
    """Return rf_exposure_started monotonic_ns (seconds) for the given point."""
    for event in events:
        if event.get("point_id") != point_id:
            continue
        if event.get("event") == "rf_exposure_started":
            return event["monotonic_ns"] / 1e9
        # Fallback to sweep_started if rf_exposure_started is absent
        if event.get("event") == "sweep_started":
            return event["monotonic_ns"] / 1e9
    return None


def resolve_rf(point: dict[str, Any], smb_snapshot: dict[str, Any] | None) -> dict[str, Any]:
    rf = dict(point.get("rf") or {})
    if smb_snapshot:
        default_sweep = dict(smb_snapshot.get("default_sweep") or {})
        for key, value in default_sweep.items():
            rf.setdefault(key, value)
    return rf


def build_frequency_axis(
    b_x: np.ndarray,
    rf: dict[str, Any],
    exposure_start_s: float,
    frame_t0_s: float | None = None,
) -> tuple[np.ndarray, np.ndarray, dict[str, Any]]:
    """Return (frequency_hz_per_sample, sample_time_ms, meta) for b_x."""
    start_hz = float(rf.get("start_hz", math.nan))
    stop_hz = float(rf.get("stop_hz", math.nan))
    step_hz = float(rf.get("step_hz", math.nan))
    dwell_ms = float(rf.get("dwell_ms", math.nan))

    n_samples = b_x.size
    # Each sample is 1 ms. Time origin is the RF exposure start.
    sample_time_ms = np.arange(n_samples, dtype=np.float64)
    if frame_t0_s is not None and math.isfinite(exposure_start_s):
        sample_time_ms = sample_time_ms + (frame_t0_s - exposure_start_s) * 1000.0

    if not (math.isfinite(start_hz) and math.isfinite(step_hz) and step_hz != 0 and math.isfinite(dwell_ms) and dwell_ms > 0):
        return np.full(n_samples, math.nan), sample_time_ms, {"warning": "invalid_rf_parameters"}

    max_index = max(0, int(round((stop_hz - start_hz) / step_hz)))
    indices = np.floor(sample_time_ms / dwell_ms).astype(np.int64)
    indices = np.clip(indices, 0, max_index)
    frequency_hz = start_hz + indices * step_hz

    meta = {
        "start_hz": start_hz,
        "stop_hz": stop_hz,
        "step_hz": step_hz,
        "dwell_ms": dwell_ms,
        "max_index": max_index,
        "expected_samples": (max_index + 1) * int(round(dwell_ms)),
    }
    return frequency_hz, sample_time_ms, meta


def bin_by_frequency(
    frequency_hz: np.ndarray,
    b_x: np.ndarray,
) -> tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
    """Return unique frequencies, means, stds, counts."""
    finite = np.isfinite(frequency_hz) & np.isfinite(b_x)
    freq = frequency_hz[finite]
    signal = b_x[finite]
    unique_freqs = np.unique(freq)
    means = np.empty_like(unique_freqs)
    stds = np.empty_like(unique_freqs)
    counts = np.empty(unique_freqs.shape, dtype=np.int64)
    for i, f in enumerate(unique_freqs):
        mask = freq == f
        chunk = signal[mask]
        means[i] = chunk.mean()
        stds[i] = chunk.std(ddof=1) if chunk.size > 1 else 0.0
        counts[i] = chunk.size
    return unique_freqs, means, stds, counts


def plot_point(
    run_id: str,
    point: dict[str, Any],
    rf: dict[str, Any],
    b_x: np.ndarray,
    events: list[dict[str, Any]],
    out_path: Path,
) -> dict[str, Any]:
    pid = point["point_id"]
    exposure_start_s = find_rf_exposure_start(events, pid)
    if exposure_start_s is None:
        print(f"warning: no rf_exposure_started event for {pid}, using segment start time fallback")
        # Fallback: treat first sample as t=0
        exposure_start_s = 0.0

    frequency_hz, sample_time_ms, meta = build_frequency_axis(b_x, rf, exposure_start_s)
    unique_freqs, means, stds, counts = bin_by_frequency(frequency_hz, b_x)

    target_b = point.get("target_b_nt", [math.nan, math.nan, math.nan])
    target_b_str = ", ".join(f"{v:.1f}" if isinstance(v, (int, float)) and math.isfinite(v) else "nan" for v in target_b)

    fig, axes = plt.subplots(
        nrows=2,
        ncols=1,
        figsize=(10, 10),
        gridspec_kw={"height_ratios": [1, 1]},
        sharex=True,
    )

    # Top: binned mean ± std
    ax0 = axes[0]
    ax0.errorbar(
        unique_freqs / 1e6,
        means,
        yerr=stds,
        fmt="o-",
        markersize=3,
        linewidth=1,
        capsize=2,
        label=f"B-X mean ± std (n={counts.sum()} samples)",
    )
    ax0.set_ylabel("B-X")
    ax0.set_title(f"{run_id} / {pid}\nB-X vs microwave frequency (target B = [{target_b_str}] nT)")
    ax0.grid(True, alpha=0.3)
    ax0.legend(loc="best")

    # Bottom: raw sample scatter colored by sample time
    ax1 = axes[1]
    scatter = ax1.scatter(
        frequency_hz / 1e6,
        b_x,
        c=sample_time_ms,
        s=1,
        alpha=0.4,
        cmap="viridis",
    )
    ax1.set_xlabel("Microwave frequency (MHz)")
    ax1.set_ylabel("B-X (raw sample)")
    ax1.grid(True, alpha=0.3)
    cbar = fig.colorbar(scatter, ax=ax1)
    cbar.set_label("Sample time from RF start (ms)")

    fig.tight_layout()
    fig.savefig(out_path, dpi=150)
    plt.close(fig)

    return {
        "point_id": pid,
        "samples_total": int(b_x.size),
        "frequency_bins": int(unique_freqs.size),
        "frequency_range_mhz": [float(unique_freqs.min() / 1e6), float(unique_freqs.max() / 1e6)] if unique_freqs.size else [None, None],
        "b_x_range": [float(b_x[np.isfinite(b_x)].min()), float(b_x[np.isfinite(b_x)].max())] if np.any(np.isfinite(b_x)) else [None, None],
        "plot": str(out_path),
        **meta,
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Plot B-X vs microwave frequency for each point in a run."
    )
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument("--out-dir", help="Output directory for plots. Defaults to <run>/postprocess/plots.")
    parser.add_argument("--channel-key", default="b_x", help="NPZ key for the ODMR signal trace.")
    args = parser.parse_args(argv)

    run_dir = Path(args.run).resolve()
    ensure_decoded_truth(run_dir)
    out_dir = Path(args.out_dir).resolve() if args.out_dir else run_dir / "postprocess" / "plots"
    out_dir.mkdir(parents=True, exist_ok=True)

    points = load_jsonl(run_dir / "points.jsonl")
    events = load_jsonl(run_dir / "events.jsonl")
    smb_snapshot = load_json(run_dir / "smb_profile_snapshot.json", {})
    run_manifest = load_json(run_dir / "run_manifest.json", {})
    run_id = run_manifest.get("run_id") or run_dir.name
    traces_by_point = load_point_series_map(run_dir, [args.channel_key])

    points_by_id = {p["point_id"]: p for p in points}
    summaries: list[dict[str, Any]] = []

    for pid, trace in traces_by_point.items():
        point = points_by_id.get(pid)
        if point is None:
            print(f"warning: no point record for {pid}", file=sys.stderr)
            continue
        b_x = np.asarray(trace[args.channel_key], dtype=np.float64)

        rf = resolve_rf(point, smb_snapshot)
        plot_path = out_dir / f"bx_vs_frequency_{pid}.png"
        summary = plot_point(run_id, point, rf, b_x, events, plot_path)
        summaries.append(summary)
        print(f"plotted {pid}: {summary['frequency_bins']} frequency bins -> {plot_path}")

    summary_path = out_dir / "plot_summaries.json"
    summary_path.write_text(json.dumps(summaries, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"wrote {len(summaries)} plots to {out_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
