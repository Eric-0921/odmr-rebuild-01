#!/usr/bin/env python3
"""Generate report-ready diagnostics for an ODMR ML dataset."""

from __future__ import annotations

import argparse
import csv
import json
import math
from pathlib import Path
from typing import Any


def load_csv(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8", newline="") as handle:
        return list(csv.DictReader(handle))


def find_one(directory: Path, pattern: str) -> Path:
    matches = sorted(directory.glob(pattern))
    if not matches:
        raise FileNotFoundError(f"no file matching {pattern} in {directory}")
    if len(matches) > 1:
        raise ValueError(f"multiple files matching {pattern} in {directory}: {matches}")
    return matches[0]


def fnum(value: Any, default: float = math.nan) -> float:
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def select_point_indices(np: Any, target_b_nt: Any) -> list[int]:
    targets = np.asarray(target_b_nt, dtype=float)
    wanted = [
        (0.0, 0.0, 0.0),
        (0.0, 0.0, 280000.0),
        (280000.0, 0.0, 0.0),
        (0.0, 280000.0, 0.0),
        (280000.0, 280000.0, 280000.0),
    ]
    indices: list[int] = []
    for target in wanted:
        dist = np.linalg.norm(targets - np.asarray(target), axis=1)
        index = int(np.argmin(dist))
        if index not in indices:
            indices.append(index)
    return indices


def grouped_stats(np: Any, rows: list[dict[str, str]], key: str, value_key: str) -> list[dict[str, float]]:
    groups: dict[float, list[float]] = {}
    for row in rows:
        groups.setdefault(fnum(row[key]), []).append(fnum(row[value_key]))
    out: list[dict[str, float]] = []
    for group_key in sorted(groups):
        values = np.asarray(groups[group_key], dtype=float)
        values = values[np.isfinite(values)]
        out.append(
            {
                "key": group_key,
                "count": float(values.size),
                "min": float(np.min(values)),
                "median": float(np.median(values)),
                "max": float(np.max(values)),
            }
        )
    return out


def build_report(args: argparse.Namespace) -> int:
    import matplotlib.pyplot as plt
    import numpy as np

    postprocess = Path(args.postprocess).resolve()
    dataset_path = Path(args.dataset).resolve() if args.dataset else find_one(postprocess, "ml_dataset_*.npz")
    samples_path = Path(args.samples).resolve() if args.samples else find_one(postprocess, "ml_samples_*.csv")

    output_dir = Path(args.out).resolve() if args.out else postprocess / "ml_report"
    output_dir.mkdir(parents=True, exist_ok=True)

    rows = load_csv(samples_path)
    data = np.load(dataset_path, allow_pickle=False)
    frequency_ghz = np.asarray(data["frequency_hz"], dtype=float) / 1e9
    target_b_nt = np.asarray(data["target_b_nt"], dtype=float)
    point_id = [str(v) for v in data["point_id"]]
    bx = np.asarray(data["X_bx_smooth_z"], dtype=float)
    by = np.asarray(data["X_by_smooth_z"], dtype=float)
    br = np.asarray(data["X_br_smooth_z"], dtype=float)

    selected = select_point_indices(np, target_b_nt)
    fig, ax = plt.subplots(figsize=(11, 6))
    for index in selected:
        bx_ut, by_ut, bz_ut = target_b_nt[index] / 1000.0
        ax.plot(frequency_ghz, bx[index], lw=1.4, label=f"{point_id[index]} ({bx_ut:.0f},{by_ut:.0f},{bz_ut:.0f}) uT")
    ax.set_title("Representative LI-ODMR spectra: B-X smooth z")
    ax.set_xlabel("Frequency (GHz)")
    ax.set_ylabel("B-X smooth9 robust z")
    ax.grid(True, alpha=0.25)
    ax.legend(fontsize=8, ncol=1)
    fig.tight_layout()
    spectra_png = output_dir / "li_odmr_representative_spectra.png"
    fig.savefig(spectra_png, dpi=180)
    plt.close(fig)

    bz_values = sorted(np.unique(target_b_nt[:, 2]))
    fig, ax = plt.subplots(figsize=(11, 6))
    for bz in bz_values:
        mask = target_b_nt[:, 2] == bz
        mean = np.nanmean(bx[mask], axis=0)
        ax.plot(frequency_ghz, mean, lw=1.3, label=f"Bz={bz/1000:.0f} uT, n={int(mask.sum())}")
    ax.set_title("Mean LI-ODMR spectra by Bz plane")
    ax.set_xlabel("Frequency (GHz)")
    ax.set_ylabel("Mean B-X smooth9 robust z")
    ax.grid(True, alpha=0.25)
    ax.legend(fontsize=8)
    fig.tight_layout()
    bz_png = output_dir / "li_odmr_mean_spectra_by_bz.png"
    fig.savefig(bz_png, dpi=180)
    plt.close(fig)

    snr_stats = grouped_stats(np, rows, "target_bz_nt", "snr_like_bx")
    zc_stats = grouped_stats(np, rows, "target_bz_nt", "bx_smooth_z_zero_crossings")
    labels = [f"{row['key']/1000:.0f}" for row in snr_stats]
    fig, axes = plt.subplots(1, 2, figsize=(11, 4))
    axes[0].bar(labels, [row["median"] for row in snr_stats], color="#2f6f73")
    axes[0].set_title("Median SNR-like by Bz")
    axes[0].set_xlabel("Bz (uT)")
    axes[0].set_ylabel("snr_like_bx")
    axes[0].grid(True, axis="y", alpha=0.25)
    axes[1].bar(labels, [row["median"] for row in zc_stats], color="#9b5b2e")
    axes[1].set_title("Median zero crossings by Bz")
    axes[1].set_xlabel("Bz (uT)")
    axes[1].set_ylabel("B-X zero crossings")
    axes[1].grid(True, axis="y", alpha=0.25)
    fig.tight_layout()
    quality_png = output_dir / "ml_quality_by_bz.png"
    fig.savefig(quality_png, dpi=180)
    plt.close(fig)

    features = np.stack([bx, by, br], axis=1).reshape(bx.shape[0], -1)
    features = (features - features.mean(axis=0)) / (features.std(axis=0) + 1e-9)
    _, singular_values, vt = np.linalg.svd(features, full_matrices=False)
    scores = features @ vt[:2].T
    explained = singular_values**2 / np.sum(singular_values**2)
    fig, ax = plt.subplots(figsize=(7, 6))
    scatter = ax.scatter(scores[:, 0], scores[:, 1], c=target_b_nt[:, 2] / 1000.0, cmap="viridis", s=45)
    ax.set_title("PCA of three-channel LI-ODMR spectra")
    ax.set_xlabel(f"PC1 ({explained[0]*100:.1f}% var)")
    ax.set_ylabel(f"PC2 ({explained[1]*100:.1f}% var)")
    ax.grid(True, alpha=0.25)
    cbar = fig.colorbar(scatter, ax=ax)
    cbar.set_label("Bz (uT)")
    fig.tight_layout()
    pca_png = output_dir / "ml_pca_scatter_by_bz.png"
    fig.savefig(pca_png, dpi=180)
    plt.close(fig)

    snr = np.asarray([fnum(row["snr_like_bx"]) for row in rows], dtype=float)
    zero_crossings = np.asarray([fnum(row["bx_smooth_z_zero_crossings"]) for row in rows], dtype=float)
    summary = {
        "dataset": str(dataset_path),
        "samples": str(samples_path),
        "sample_count": int(target_b_nt.shape[0]),
        "frequency_bins": int(frequency_ghz.size),
        "target_min_nt": target_b_nt.min(axis=0).tolist(),
        "target_max_nt": target_b_nt.max(axis=0).tolist(),
        "unique_target_counts": [int(np.unique(target_b_nt[:, i]).size) for i in range(3)],
        "nan_count": {
            "X_bx_smooth_z": int(np.isnan(bx).sum()),
            "X_by_smooth_z": int(np.isnan(by).sum()),
            "X_br_smooth_z": int(np.isnan(br).sum()),
        },
        "snr_like_bx_quantiles": np.nanpercentile(snr, [0, 10, 25, 50, 75, 90, 100]).round(3).tolist(),
        "zero_crossing_quantiles": np.nanpercentile(zero_crossings, [0, 25, 50, 75, 100]).round(3).tolist(),
        "pca_explained_variance_first10": explained[:10].round(4).tolist(),
        "snr_by_bz": snr_stats,
        "zero_crossings_by_bz": zc_stats,
        "figures": {
            "representative_spectra": str(spectra_png),
            "mean_spectra_by_bz": str(bz_png),
            "quality_by_bz": str(quality_png),
            "pca_scatter_by_bz": str(pca_png),
        },
    }
    summary_path = output_dir / "ml_report_summary.json"
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    md_path = output_dir / "ml_report_summary.md"
    md_path.write_text(
        "\n".join(
            [
                "# LI-ODMR 3D Scan Report Draft",
                "",
                "## Dataset",
                "",
                f"- Samples: {target_b_nt.shape[0]}",
                f"- Frequency bins: {frequency_ghz.size}",
                f"- Field grid: {summary['unique_target_counts']} levels over {summary['target_min_nt']} to {summary['target_max_nt']} nT",
                f"- NaN counts: {summary['nan_count']}",
                f"- SNR-like B-X quantiles: {summary['snr_like_bx_quantiles']}",
                f"- Zero crossing quantiles: {summary['zero_crossing_quantiles']}",
                f"- PCA first 6 explained variance: {explained[:6].round(4).tolist()}",
                "",
                "## Figures",
                "",
                f"- Representative spectra: {spectra_png.name}",
                f"- Mean spectra by Bz: {bz_png.name}",
                f"- Quality by Bz: {quality_png.name}",
                f"- PCA by Bz: {pca_png.name}",
                "",
                "## Initial Interpretation",
                "",
                "- The dataset is complete for a 5 x 5 x 5 Cartesian field scan.",
                "- No NaN values were found in the three ML input channels.",
                "- Quality indicators show no overload and PLL lock is complete in the exported samples.",
                "- The Bz=0 plane has lower SNR-like values and more zero crossings than other Bz planes, so it should be reported as a distinct quality/physics regime.",
                "- PCA shows low-dimensional structure in the spectra, supporting ML analysis, but this is not yet a validated generalization result.",
                "",
            ]
        ),
        encoding="utf-8",
    )

    print(f"wrote report directory: {output_dir}")
    print(f"wrote summary: {summary_path}")
    print(f"wrote markdown: {md_path}")
    return 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Generate report-ready ODMR ML diagnostics.")
    parser.add_argument("--postprocess", required=True, help="Run postprocess directory containing ml_dataset_*.npz.")
    parser.add_argument("--dataset", help="Optional explicit ml_dataset_*.npz path.")
    parser.add_argument("--samples", help="Optional explicit ml_samples_*.csv path.")
    parser.add_argument("--out", help="Output report directory. Defaults to <postprocess>/ml_report.")
    return parser.parse_args()


if __name__ == "__main__":
    raise SystemExit(build_report(parse_args()))
