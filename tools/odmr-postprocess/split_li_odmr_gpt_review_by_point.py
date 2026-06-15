#!/usr/bin/env python3
"""Split a multi-point LI-ODMR GPT review CSV into per-point CSVs and plots."""

from __future__ import annotations

import argparse
import csv
import json
from collections import defaultdict
from pathlib import Path
from typing import Any


def find_one(directory: Path, pattern: str) -> Path:
    matches = sorted(directory.glob(pattern))
    if not matches:
        raise FileNotFoundError(f"no file matching {pattern} in {directory}")
    if len(matches) > 1:
        raise ValueError(f"multiple files matching {pattern} in {directory}: {matches}")
    return matches[0]


def find_spectrum_csv(directory: Path) -> Path:
    matches = sorted(
        path
        for path in directory.glob("li_odmr_gpt_review_*.csv")
        if not path.name.endswith("_metadata.csv")
    )
    if not matches:
        raise FileNotFoundError(f"no spectrum review CSV in {directory}")
    if len(matches) > 1:
        raise ValueError(f"multiple spectrum review CSV files in {directory}: {matches}")
    return matches[0]


def safe_name(value: str) -> str:
    return "".join(ch if ch.isalnum() or ch in "-_." else "_" for ch in value)


def load_metadata(path: Path) -> dict[str, dict[str, str]]:
    with path.open("r", encoding="utf-8", newline="") as handle:
        rows = list(csv.DictReader(handle))
    return {row["point_id"]: row for row in rows}


def load_spectra(path: Path) -> dict[str, list[dict[str, str]]]:
    groups: dict[str, list[dict[str, str]]] = defaultdict(list)
    with path.open("r", encoding="utf-8", newline="") as handle:
        for row in csv.DictReader(handle):
            groups[row["point_id"]].append(row)
    return dict(groups)


def fmt_ut(value: str) -> str:
    try:
        return f"{float(value) / 1000.0:.0f}"
    except (TypeError, ValueError):
        return value


def write_point_csv(path: Path, rows: list[dict[str, str]], metadata: dict[str, str]) -> None:
    metadata_columns = [f"meta_{key}" for key in metadata.keys()]
    fieldnames = list(rows[0].keys()) + [key for key in metadata_columns if key[5:] not in rows[0]]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in rows:
            out = dict(row)
            for key, value in metadata.items():
                if key not in out:
                    out[f"meta_{key}"] = value
            writer.writerow(out)


def plot_point(path: Path, rows: list[dict[str, str]], metadata: dict[str, str]) -> None:
    import matplotlib.pyplot as plt

    freq = [float(row["frequency_ghz"]) for row in rows]
    bx = [float(row["b_x_smooth9_z"]) for row in rows]
    by = [float(row["b_y_smooth9_z"]) for row in rows]
    br = [float(row["b_r_smooth9_z"]) for row in rows]

    bx_ut = fmt_ut(metadata.get("target_bx_nt", ""))
    by_ut = fmt_ut(metadata.get("target_by_nt", ""))
    bz_ut = fmt_ut(metadata.get("target_bz_nt", ""))
    title = f"{metadata.get('point_id', '')}  B=({bx_ut}, {by_ut}, {bz_ut}) uT"
    subtitle = (
        f"RF {metadata.get('rf_start_hz', '')}-{metadata.get('rf_stop_hz', '')} Hz, "
        f"step {metadata.get('rf_step_hz', '')} Hz, dwell {metadata.get('rf_dwell_ms', '')} ms, "
        f"power {metadata.get('rf_power_dbm', '')} dBm"
    )
    quality = (
        f"laser={metadata.get('laser_mode', '')} {metadata.get('laser_power_mw', '')} mW, "
        f"overload(input/gain)={metadata.get('b_input_overload_ratio', '')}/{metadata.get('b_gain_overload_ratio', '')}, "
        f"PLL={metadata.get('b_pll_locked_ratio', '')}"
    )

    fig, ax = plt.subplots(figsize=(11, 6))
    ax.plot(freq, bx, lw=1.8, color="#0b4f6c", label="Channel-B-X smooth9 z")
    ax.plot(freq, by, lw=0.9, color="#9aa3a8", alpha=0.45, label="B-Y auxiliary")
    ax.plot(freq, br, lw=0.9, color="#c8a24a", alpha=0.38, label="B-R auxiliary")
    ax.axhline(0, color="black", lw=0.8, alpha=0.35)
    ax.set_title(f"{title}\n{subtitle}\n{quality}", fontsize=10)
    ax.set_xlabel("Frequency (GHz)")
    ax.set_ylabel("Robust z-score; B-X is the primary trace")
    ax.grid(True, alpha=0.25)
    ax.legend(fontsize=8, loc="best")
    fig.tight_layout()
    fig.savefig(path, dpi=170)
    plt.close(fig)


def split_review(args: argparse.Namespace) -> int:
    postprocess = Path(args.postprocess).resolve()
    spectrum_csv = Path(args.spectrum_csv).resolve() if args.spectrum_csv else find_spectrum_csv(postprocess)
    metadata_csv = Path(args.metadata_csv).resolve() if args.metadata_csv else find_one(
        postprocess, "li_odmr_gpt_review_*_metadata.csv"
    )
    out_dir = Path(args.out).resolve() if args.out else postprocess / "li_odmr_gpt_review_by_point"
    csv_dir = out_dir / "csv"
    plot_dir = out_dir / "plots"
    csv_dir.mkdir(parents=True, exist_ok=True)
    plot_dir.mkdir(parents=True, exist_ok=True)

    metadata_by_point = load_metadata(metadata_csv)
    spectra_by_point = load_spectra(spectrum_csv)
    manifest: list[dict[str, Any]] = []

    for point_id in sorted(spectra_by_point, key=lambda key: int(spectra_by_point[key][0].get("point_index", 0))):
        rows = spectra_by_point[point_id]
        metadata = metadata_by_point.get(point_id, {"point_id": point_id})
        point_index = int(rows[0].get("point_index", len(manifest)))
        stem = f"{point_index + 1:03d}_{safe_name(point_id)}"
        csv_path = csv_dir / f"{stem}.csv"
        plot_path = plot_dir / f"{stem}.png"
        write_point_csv(csv_path, rows, metadata)
        plot_point(plot_path, rows, metadata)
        manifest.append(
            {
                "point_index": point_index,
                "point_id": point_id,
                "target_bx_nt": metadata.get("target_bx_nt"),
                "target_by_nt": metadata.get("target_by_nt"),
                "target_bz_nt": metadata.get("target_bz_nt"),
                "frequency_bins": len(rows),
                "csv": str(csv_path),
                "plot": str(plot_path),
            }
        )

    manifest_csv = out_dir / "manifest.csv"
    with manifest_csv.open("w", encoding="utf-8", newline="") as handle:
        fieldnames = ["point_index", "point_id", "target_bx_nt", "target_by_nt", "target_bz_nt", "frequency_bins", "csv", "plot"]
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(manifest)

    summary_path = out_dir / "summary.json"
    summary_path.write_text(
        json.dumps(
            {
                "source_spectrum_csv": str(spectrum_csv),
                "source_metadata_csv": str(metadata_csv),
                "point_count": len(manifest),
                "output_dir": str(out_dir),
                "csv_dir": str(csv_dir),
                "plot_dir": str(plot_dir),
                "manifest_csv": str(manifest_csv),
            },
            ensure_ascii=False,
            indent=2,
        )
        + "\n",
        encoding="utf-8",
    )

    print(f"wrote {len(manifest)} per-point CSVs: {csv_dir}")
    print(f"wrote {len(manifest)} per-point plots: {plot_dir}")
    print(f"wrote manifest: {manifest_csv}")
    return 0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Split LI-ODMR GPT review data by point and plot each spectrum.")
    parser.add_argument("--postprocess", required=True, help="Run postprocess directory.")
    parser.add_argument("--spectrum-csv", help="Explicit li_odmr_gpt_review_<run>.csv path.")
    parser.add_argument("--metadata-csv", help="Explicit li_odmr_gpt_review_<run>_metadata.csv path.")
    parser.add_argument("--out", help="Output directory. Defaults to <postprocess>/li_odmr_gpt_review_by_point.")
    return parser.parse_args()


if __name__ == "__main__":
    raise SystemExit(split_review(parse_args()))
