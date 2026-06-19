#!/usr/bin/env python3
"""Read direct-decode runtime truth from sample_values.csv + segments.jsonl."""

from __future__ import annotations

import csv
import json
from pathlib import Path
from typing import Any


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
        for line in handle:
            text = line.strip()
            if text:
                rows.append(json.loads(text))
    return rows


def ensure_decoded_truth(run_dir: Path) -> None:
    missing = [
        name
        for name in ("sample_values.csv", "segments.jsonl", "points.jsonl")
        if not (run_dir / name).exists()
    ]
    if missing:
        raise FileNotFoundError(f"{run_dir} missing decoded truth files: {', '.join(missing)}")


def load_segments_by_point(run_dir: Path) -> dict[str, dict[str, Any]]:
    ensure_decoded_truth(run_dir)
    rows = load_jsonl(run_dir / "segments.jsonl")
    result: dict[str, dict[str, Any]] = {}
    required_fields = ["point_id", "sample_index_start", "sample_index_end"]
    for row_number, row in enumerate(rows, start=1):
        missing = [key for key in required_fields if key not in row]
        if missing:
            raise ValueError(f"segments.jsonl row {row_number} missing fields: {', '.join(missing)}")
        point_id = str(row.get("point_id") or "")
        if not point_id:
            raise ValueError(f"segments.jsonl row {row_number} has empty point_id")
        for key in ("sample_index_start", "sample_index_end"):
            try:
                int(row[key])
            except (TypeError, ValueError) as exc:
                raise ValueError(f"segments.jsonl row {row_number} field {key} must be an integer") from exc
        result[point_id] = row
    return result


def load_point_series_map(
    run_dir: Path,
    field_keys: list[str],
) -> dict[str, dict[str, list[float]]]:
    ensure_decoded_truth(run_dir)
    segments = load_segments_by_point(run_dir)
    ordered = sorted(
        (
            (
                point_id,
                int(segment["sample_index_start"]),
                int(segment["sample_index_end"]),
            )
            for point_id, segment in segments.items()
        ),
        key=lambda item: item[1],
    )
    requested_keys = list(dict.fromkeys(["frame_seq", "global_sample_index", *field_keys]))
    series_by_point: dict[str, dict[str, list[float]]] = {
        point_id: {key: [] for key in requested_keys}
        for point_id, _, _ in ordered
    }

    if not ordered:
        return series_by_point

    current_index = 0
    with (run_dir / "sample_values.csv").open("r", encoding="utf-8", newline="") as handle:
        reader = csv.DictReader(handle)
        missing_columns = [key for key in requested_keys if key not in (reader.fieldnames or [])]
        if missing_columns:
            raise ValueError(f"sample_values.csv missing columns: {', '.join(missing_columns)}")
        for row in reader:
            if current_index >= len(ordered):
                break

            global_sample_index = int(row["global_sample_index"])
            while current_index < len(ordered) and global_sample_index >= ordered[current_index][2]:
                current_index += 1
            if current_index >= len(ordered):
                break

            point_id, sample_start, sample_end = ordered[current_index]
            if global_sample_index < sample_start or global_sample_index >= sample_end:
                continue

            point_series = series_by_point[point_id]
            for key in requested_keys:
                point_series[key].append(parse_numeric(row[key]))

    return series_by_point


def build_point_field_summaries(
    series_by_point: dict[str, dict[str, list[float]]],
) -> dict[str, dict[str, Any]]:
    summaries: dict[str, dict[str, Any]] = {}
    for point_id, series in series_by_point.items():
        samples_total = len(series.get("global_sample_index", []))
        summaries[point_id] = {
            "point_id": point_id,
            "samples_total": samples_total,
            "b_input_overload_ratio": mean_of(series.get("b_input_overload", [])),
            "b_gain_overload_ratio": mean_of(series.get("b_gain_overload", [])),
            "b_pll_locked_ratio": mean_of(series.get("b_pll_locked", [])),
            "source_file": "sample_values.csv",
        }
    return summaries


def parse_numeric(value: str | None) -> float:
    if value is None or value == "":
        return float("nan")
    try:
        return float(value)
    except ValueError:
        return float("nan")


def mean_of(values: list[float]) -> float:
    finite = [value for value in values if value == value]
    if not finite:
        return float("nan")
    return sum(finite) / len(finite)
