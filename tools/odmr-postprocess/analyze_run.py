#!/usr/bin/env python3
"""Detailed offline analysis for a single ODMR run.

Produces diagnostic plots and text metrics for:
- B-X time series and spectrum
- time-frequency mapping quality
- modulation/drift signatures
- basic signal quality
"""

from __future__ import annotations

import argparse
import json
import math
import sys
from pathlib import Path
from typing import Any

import numpy as np


try:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
except ImportError as exc:  # pragma: no cover
    raise RuntimeError("matplotlib is required: python3 -m pip install matplotlib") from exc


try:
    from scipy.signal import find_peaks
except ImportError:  # pragma: no cover
    find_peaks = None


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


def build_frequency_axis(
    n_samples: int,
    rf: dict[str, Any],
) -> np.ndarray:
    start_hz = float(rf.get("start_hz", math.nan))
    stop_hz = float(rf.get("stop_hz", math.nan))
    step_hz = float(rf.get("step_hz", math.nan))
    dwell_ms = float(rf.get("dwell_ms", math.nan))

    sample_time_ms = np.arange(n_samples, dtype=np.float64)
    if not (math.isfinite(start_hz) and math.isfinite(step_hz) and step_hz != 0 and math.isfinite(dwell_ms) and dwell_ms > 0):
        return np.full(n_samples, math.nan)

    max_index = max(0, int(round((stop_hz - start_hz) / step_hz)))
    indices = np.floor(sample_time_ms / dwell_ms).astype(np.int64)
    indices = np.clip(indices, 0, max_index)
    return start_hz + indices * step_hz


def summarize_point(
    run_dir: Path,
    point: dict[str, Any],
    pf_row: dict[str, Any],
    channel_key: str,
) -> dict[str, Any]:
    npz_path = run_dir / pf_row["sidecar"]["relative_path"]
    with np.load(npz_path, allow_pickle=False) as data:
        b_x = np.asarray(data[channel_key], dtype=np.float64)
        frame_seq = np.asarray(data["frame_seq"], dtype=np.uint64)
        b_pll = np.asarray(data["b_pll_locked"], dtype=np.int8)

    rf = point.get("rf", {})
    frequency_hz = build_frequency_axis(b_x.size, rf)
    sample_time_ms = np.arange(b_x.size, dtype=np.float64)

    finite = b_x[np.isfinite(b_x)]
    stats = {
        "samples_total": int(b_x.size),
        "frames_total": int(frame_seq.size),
        "pll_locked_ratio": float(np.mean(b_pll)) if b_pll.size else None,
        "b_x_min": float(finite.min()) if finite.size else None,
        "b_x_max": float(finite.max()) if finite.size else None,
        "b_x_mean": float(finite.mean()) if finite.size else None,
        "b_x_std": float(finite.std(ddof=1)) if finite.size > 1 else None,
    }

    # Frequency-binned spectrum
    unique_freqs = np.unique(frequency_hz[np.isfinite(frequency_hz)])
    means = []
    stds = []
    counts = []
    for f in unique_freqs:
        mask = frequency_hz == f
        chunk = b_x[mask]
        means.append(float(chunk.mean()))
        stds.append(float(chunk.std(ddof=1)) if chunk.size > 1 else 0.0)
        counts.append(int(chunk.size))
    means = np.asarray(means)

    stats["frequency_bins"] = int(unique_freqs.size)
    stats["spectrum_min"] = float(means.min()) if means.size else None
    stats["spectrum_max"] = float(means.max()) if means.size else None
    stats["peak_to_peak"] = float(means.max() - means.min()) if means.size else None
    if stats["b_x_mean"] and stats["b_x_mean"] != 0:
        stats["relative_contrast_percent"] = float((means.max() - means.min()) / abs(stats["b_x_mean"]) * 100)

    return {
        "point": point,
        "rf": rf,
        "b_x": b_x,
        "frequency_hz": frequency_hz,
        "sample_time_ms": sample_time_ms,
        "unique_freqs": unique_freqs,
        "means": means,
        "stds": np.asarray(stds),
        "counts": np.asarray(counts),
        "stats": stats,
    }


def plot_overview(ctx: dict[str, Any], out_path: Path) -> None:
    b_x = ctx["b_x"]
    frequency_hz = ctx["frequency_hz"]
    sample_time_ms = ctx["sample_time_ms"]
    unique_freqs = ctx["unique_freqs"]
    means = ctx["means"]
    stds = ctx["stds"]
    rf = ctx["rf"]
    pid = ctx["point"]["point_id"]

    fig, axes = plt.subplots(nrows=3, ncols=1, figsize=(12, 14))

    # 1. B-X time series colored by frequency
    ax0 = axes[0]
    sc = ax0.scatter(sample_time_ms / 1000.0, b_x, c=frequency_hz / 1e6, s=0.1, alpha=0.3, cmap="turbo")
    ax0.set_xlabel("Time from RF exposure start (s)")
    ax0.set_ylabel("B-X")
    ax0.set_title(f"{pid}: B-X time series (color = microwave frequency)")
    ax0.grid(True, alpha=0.3)
    fig.colorbar(sc, ax=ax0, label="Frequency (MHz)")

    # 2. Mean spectrum with error bars
    ax1 = axes[1]
    ax1.errorbar(unique_freqs / 1e6, means, yerr=stds, fmt=".-", markersize=2, linewidth=0.8, alpha=0.7)
    ax1.set_xlabel("Microwave frequency (MHz)")
    ax1.set_ylabel("B-X (mean ± std)")
    ax1.set_title(f"{pid}: frequency-binned B-X spectrum ({len(unique_freqs)} bins)")
    ax1.grid(True, alpha=0.3)
    if rf:
        ax1.axvline(rf.get("start_hz", 0) / 1e6, color="gray", linestyle="--", alpha=0.5)
        ax1.axvline(rf.get("stop_hz", 0) / 1e6, color="gray", linestyle="--", alpha=0.5)

    # 3. Histogram of B-X
    ax2 = axes[2]
    ax2.hist(b_x[np.isfinite(b_x)], bins=200, color="steelblue", edgecolor="none", alpha=0.7)
    ax2.set_xlabel("B-X")
    ax2.set_ylabel("Count")
    ax2.set_title(f"{pid}: B-X distribution")
    ax2.grid(True, alpha=0.3)

    fig.tight_layout()
    fig.savefig(out_path, dpi=150)
    plt.close(fig)


def plot_time_frequency_heatmap(ctx: dict[str, Any], out_path: Path) -> None:
    b_x = ctx["b_x"]
    frequency_hz = ctx["frequency_hz"]
    rf = ctx["rf"]
    pid = ctx["point"]["point_id"]

    start_hz = rf.get("start_hz", 0)
    stop_hz = rf.get("stop_hz", 0)
    step_hz = rf.get("step_hz", 1)
    dwell_ms = rf.get("dwell_ms", 1)

    # Build 2D array: rows = frequency bins, cols = samples within bin
    n_freq = int(round((stop_hz - start_hz) / step_hz)) + 1
    samples_per_bin = int(round(dwell_ms))

    # Reshape based on expected samples; truncate/pad to handle extra samples
    expected_total = n_freq * samples_per_bin
    usable = b_x[:expected_total]
    matrix = usable.reshape(n_freq, samples_per_bin)

    fig, ax = plt.subplots(figsize=(12, 8))
    freqs_mhz = (start_hz + np.arange(n_freq) * step_hz) / 1e6
    time_ms = np.arange(samples_per_bin)
    im = ax.imshow(
        matrix,
        aspect="auto",
        origin="lower",
        extent=[time_ms[0], time_ms[-1], freqs_mhz[0], freqs_mhz[-1]],
        cmap="RdBu_r",
        vmin=np.nanpercentile(matrix, 1),
        vmax=np.nanpercentile(matrix, 99),
    )
    ax.set_xlabel("Time within frequency bin (ms)")
    ax.set_ylabel("Microwave frequency (MHz)")
    ax.set_title(f"{pid}: B-X vs time-within-bin and frequency")
    fig.colorbar(im, ax=ax, label="B-X")
    fig.tight_layout()
    fig.savefig(out_path, dpi=150)
    plt.close(fig)


def plot_fft_analysis(ctx: dict[str, Any], out_path: Path) -> None:
    b_x = ctx["b_x"]
    pid = ctx["point"]["point_id"]

    fs = 1000.0  # 1 ms sample interval
    clean = b_x[np.isfinite(b_x)]
    n = len(clean)
    freqs = np.fft.rfftfreq(n, d=1.0 / fs)
    power = np.abs(np.fft.rfft(clean)) ** 2

    fig, axes = plt.subplots(nrows=2, ncols=1, figsize=(12, 10))

    # Full spectrum
    ax0 = axes[0]
    ax0.semilogy(freqs, power, linewidth=0.5)
    ax0.set_xlabel("Frequency (Hz)")
    ax0.set_ylabel("Power")
    ax0.set_title(f"{pid}: FFT of full B-X time series (sample rate 1 kHz)")
    ax0.grid(True, alpha=0.3, which="both")
    ax0.set_xlim(0, 100)

    # Zoom 0-600 Hz with annotations
    ax1 = axes[1]
    mask = freqs <= 600
    ax1.semilogy(freqs[mask], power[mask], linewidth=0.8)
    ax1.axvline(500, color="red", linestyle="--", alpha=0.7, label="SMB LF modulation 500 Hz")
    ax1.axvline(50, color="green", linestyle="--", alpha=0.5, label="50 Hz line")
    ax1.axvline(100, color="green", linestyle="--", alpha=0.5, label="100 Hz line")
    ax1.set_xlabel("Frequency (Hz)")
    ax1.set_ylabel("Power")
    ax1.set_title(f"{pid}: FFT detail (0-600 Hz)")
    ax1.grid(True, alpha=0.3, which="both")
    ax1.legend(loc="upper right")

    fig.tight_layout()
    fig.savefig(out_path, dpi=150)
    plt.close(fig)


def plot_bin_traces(ctx: dict[str, Any], out_path: Path, n_trace_bins: int = 10) -> None:
    b_x = ctx["b_x"]
    frequency_hz = ctx["frequency_hz"]
    unique_freqs = ctx["unique_freqs"]
    pid = ctx["point"]["point_id"]

    # Pick evenly spaced frequency bins
    selected = np.linspace(0, len(unique_freqs) - 1, n_trace_bins, dtype=int)

    fig, axes = plt.subplots(nrows=n_trace_bins, ncols=1, figsize=(12, 2.5 * n_trace_bins), sharex=True)
    if n_trace_bins == 1:
        axes = [axes]

    for ax, idx in zip(axes, selected):
        f = unique_freqs[idx]
        mask = frequency_hz == f
        chunk = b_x[mask]
        t = np.arange(chunk.size)
        ax.plot(t, chunk, linewidth=0.5)
        ax.set_ylabel("B-X")
        ax.set_title(f"{f / 1e6:.3f} MHz: {chunk.size} samples")
        ax.grid(True, alpha=0.3)
    axes[-1].set_xlabel("Sample index within frequency bin")
    fig.suptitle(f"{pid}: B-X trace within selected frequency bins", y=1.00)
    fig.tight_layout()
    fig.savefig(out_path, dpi=150)
    plt.close(fig)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Detailed offline analysis of one ODMR run.")
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument("--out-dir", help="Output directory for plots. Defaults to <run>/analysis.")
    parser.add_argument("--channel-key", default="b_x", help="NPZ key for the signal trace.")
    args = parser.parse_args(argv)

    run_dir = Path(args.run).resolve()
    out_dir = Path(args.out_dir).resolve() if args.out_dir else run_dir / "analysis"
    out_dir.mkdir(parents=True, exist_ok=True)

    point_fields = load_jsonl(run_dir / "point_fields.jsonl")
    points = load_jsonl(run_dir / "points.jsonl")
    points_by_id = {p["point_id"]: p for p in points}

    if not point_fields:
        print(f"missing point_fields.jsonl in {run_dir}; run extract_point_fields_from_rall.py first", file=sys.stderr)
        return 2

    summaries: list[dict[str, Any]] = []
    for pf in point_fields:
        pid = pf["point_id"]
        point = points_by_id.get(pid)
        if point is None:
            print(f"warning: no point record for {pid}", file=sys.stderr)
            continue

        ctx = summarize_point(run_dir, point, pf, args.channel_key)
        summaries.append({"point_id": pid, "stats": ctx["stats"]})

        print(f"\n=== {pid} ===")
        for key, value in ctx["stats"].items():
            if isinstance(value, float):
                print(f"  {key}: {value:.3e}")
            else:
                print(f"  {key}: {value}")

        plot_overview(ctx, out_dir / f"overview_{pid}.png")
        plot_time_frequency_heatmap(ctx, out_dir / f"timefreq_heatmap_{pid}.png")
        plot_fft_analysis(ctx, out_dir / f"fft_{pid}.png")
        plot_bin_traces(ctx, out_dir / f"bin_traces_{pid}.png", n_trace_bins=10)

    summary_path = out_dir / "analysis_summary.json"
    summary_path.write_text(json.dumps(summaries, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"\nwrote analysis plots and summary to {out_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
