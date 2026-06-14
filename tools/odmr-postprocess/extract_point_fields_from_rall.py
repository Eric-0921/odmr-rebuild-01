#!/usr/bin/env python3
"""Extract per-point NPZ sidecars from raw/oe1022d.rall.

This is an offline rescue tool: when the C# runtime only writes the raw RALL
stream and frame index but does not produce point_fields/*.npz, this script
reconstructs them from the final truth layer (rall + frames.idx + segments).

It does not modify existing code or runtime behavior.
"""

from __future__ import annotations

import argparse
import json
import math
import struct
import sys
from pathlib import Path
from typing import Any


try:
    import numpy as np
except ImportError as exc:  # pragma: no cover
    raise RuntimeError("numpy is required: python3 -m pip install numpy") from exc


SCHEMA_VERSION = 1
RALL_FRAME_BYTES = 12288
SAMPLES_PER_FRAME = 50
MEASUREMENT_FIELDS = [
    ("a_x", 0),
    ("a_y", 400),
    ("a_freq", 800),
    ("a_noise", 1200),
    ("a_xh1", 1600),
    ("a_yh1", 2000),
    ("a_xh2", 2400),
    ("a_yh2", 2800),
    ("b_x", 3200),
    ("b_y", 3600),
    ("b_freq", 4000),
    ("b_noise", 4400),
    ("b_xh1", 4800),
    ("b_yh1", 5200),
    ("b_xh2", 5600),
    ("b_yh2", 6000),
    ("auxadc1", 6400),
    ("auxadc2", 6800),
    ("auxadc3", 7200),
    ("auxadc4", 7600),
]
FIELD_NAMES = [name for name, _ in MEASUREMENT_FIELDS]
STATUS_FIELDS = {
    "b_ref_source_code": (8504, 8505, "b"),
    "b_ref_slope_code": (8521, 8522, "b"),
    "b_ref_current_freq_hz": (8505, 8513, "d"),
    "b_input_overload": (8779, 8780, "b"),
    "b_gain_overload": (8780, 8781, "b"),
    "b_pll_locked": (8781, 8782, "b"),
}


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


def parse_frame(payload: bytes, endian: str) -> dict[str, Any]:
    if len(payload) != RALL_FRAME_BYTES:
        raise ValueError(f"expected {RALL_FRAME_BYTES} bytes, got {len(payload)}")

    prefix = ">" if endian == "big" else "<"
    result: dict[str, Any] = {}
    for name, offset in MEASUREMENT_FIELDS:
        values = struct.unpack(f"{prefix}50d", payload[offset : offset + 400])
        result[name] = np.asarray(values, dtype=np.float64)

    for name, (start, end, fmt) in STATUS_FIELDS.items():
        value = struct.unpack(f"{prefix}{fmt}", payload[start:end])[0]
        result[name] = value

    return result


def extract_segment(
    raw: bytes,
    frames: list[dict[str, Any]],
    segment: dict[str, Any],
    endian: str,
) -> tuple[dict[str, np.ndarray], dict[str, Any]]:
    seq_start = segment["frame_seq_start"]
    seq_end = segment["frame_seq_end"]
    if seq_start is None or seq_end is None:
        raise ValueError(f"segment {segment.get('segment_id')} has no frame range")

    selected = [f for f in frames if seq_start <= f["frame_seq"] <= seq_end]
    selected.sort(key=lambda f: f["frame_seq"])
    if not selected:
        raise ValueError(f"segment {segment.get('segment_id')}: no frames found")

    measurement_arrays: dict[str, list[np.ndarray]] = {name: [] for name in FIELD_NAMES}
    status_meta: dict[str, list[Any]] = {
        "frame_seq": [],
        "duplicate_hint": [],
        "b_ref_source_code": [],
        "b_ref_slope_code": [],
        "b_ref_current_freq_hz": [],
        "b_input_overload": [],
        "b_gain_overload": [],
        "b_pll_locked": [],
    }

    for frame in selected:
        offset = frame["raw_offset"]
        length = frame.get("raw_len", RALL_FRAME_BYTES)
        payload = raw[offset : offset + length]
        if len(payload) < RALL_FRAME_BYTES:
            # Pad incomplete trailing frame so parser can still proceed
            payload = payload + bytes(RALL_FRAME_BYTES - len(payload))
        parsed = parse_frame(payload, endian)
        for name in FIELD_NAMES:
            measurement_arrays[name].append(parsed[name])
        status_meta["frame_seq"].append(frame["frame_seq"])
        dup = frame.get("duplicate_of")
        status_meta["duplicate_hint"].append(dup if dup is not None else -1)
        status_meta["b_ref_source_code"].append(parsed["b_ref_source_code"])
        status_meta["b_ref_slope_code"].append(parsed["b_ref_slope_code"])
        status_meta["b_ref_current_freq_hz"].append(parsed["b_ref_current_freq_hz"])
        status_meta["b_input_overload"].append(parsed["b_input_overload"])
        status_meta["b_gain_overload"].append(parsed["b_gain_overload"])
        status_meta["b_pll_locked"].append(parsed["b_pll_locked"])

    npz_data: dict[str, np.ndarray] = {}
    for name in FIELD_NAMES:
        npz_data[name] = np.concatenate(measurement_arrays[name])
    for key, values in status_meta.items():
        dtype = {
            "frame_seq": np.uint64,
            "duplicate_hint": np.int64,
            "b_ref_source_code": np.int16,
            "b_ref_slope_code": np.int16,
            "b_ref_current_freq_hz": np.float64,
            "b_input_overload": np.int8,
            "b_gain_overload": np.int8,
            "b_pll_locked": np.int8,
        }[key]
        npz_data[key] = np.asarray(values, dtype=dtype)

    return npz_data, status_meta


def compute_field_summary(array: np.ndarray) -> dict[str, Any]:
    finite = array[np.isfinite(array)]
    if finite.size == 0:
        return {"mean": None, "std": None, "min": None, "max": None}
    return {
        "mean": float(finite.mean()),
        "std": float(finite.std(ddof=1)) if finite.size > 1 else 0.0,
        "min": float(finite.min()),
        "max": float(finite.max()),
    }


def build_point_fields_row(
    run_id: str,
    point: dict[str, Any],
    segment: dict[str, Any],
    npz_data: dict[str, np.ndarray],
    sidecar_rel: str,
    manifest_rel: str,
) -> dict[str, Any]:
    pid = point["point_id"]
    seg_id = segment["segment_id"]
    n_frames = len(npz_data["frame_seq"])
    n_samples = npz_data["b_x"].size
    field_summaries = []
    for name in FIELD_NAMES:
        summary = compute_field_summary(npz_data[name])
        field_summaries.append(
            {
                "field_name": name.replace("_", "-").upper(),
                "npz_key": name,
                "mean": summary["mean"],
            }
        )

    b_pll = np.asarray(npz_data["b_pll_locked"], dtype=np.float64)
    b_input = np.asarray(npz_data["b_input_overload"], dtype=np.float64)
    b_gain = np.asarray(npz_data["b_gain_overload"], dtype=np.float64)

    return {
        "schema_version": SCHEMA_VERSION,
        "run_id": run_id,
        "point_id": pid,
        "segment_id": seg_id,
        "frames_parsed": n_frames,
        "samples_total": n_samples,
        "samples_per_frame": SAMPLES_PER_FRAME,
        "matrix_shape": [len(FIELD_NAMES), n_samples],
        "measurement_field_order": [n.replace("_", "-").upper() for n in FIELD_NAMES],
        "measurement_field_keys": FIELD_NAMES,
        "field_summaries": field_summaries,
        "b_pll_locked_ratio": float(np.mean(b_pll)) if b_pll.size else None,
        "b_input_overload_ratio": float(np.mean(b_input)) if b_input.size else None,
        "b_gain_overload_ratio": float(np.mean(b_gain)) if b_gain.size else None,
        "last_b_ref_source_code": int(npz_data["b_ref_source_code"][-1]) if n_frames else None,
        "last_b_ref_slope_code": int(npz_data["b_ref_slope_code"][-1]) if n_frames else None,
        "last_b_ref_current_freq_hz": float(npz_data["b_ref_current_freq_hz"][-1]) if n_frames else None,
        "sidecar": {
            "format": "npz",
            "schema_version": SCHEMA_VERSION,
            "relative_path": sidecar_rel,
            "manifest_relative_path": manifest_rel,
            "measurement_field_keys": FIELD_NAMES,
            "status_keys": [
                "frame_seq",
                "duplicate_hint",
                "b_ref_source_code",
                "b_ref_slope_code",
                "b_ref_current_freq_hz",
                "b_input_overload",
                "b_gain_overload",
                "b_pll_locked",
            ],
        },
    }


def build_manifest(
    run_id: str,
    point: dict[str, Any],
    segment: dict[str, Any],
    smb_snapshot: dict[str, Any],
    oe_snapshot: dict[str, Any],
    laser_snapshot: dict[str, Any],
    calibration_snapshot: dict[str, Any],
) -> dict[str, Any]:
    rf = dict(point.get("rf") or {})
    if smb_snapshot:
        default_sweep = dict(smb_snapshot.get("default_sweep") or {})
        for key, value in default_sweep.items():
            rf.setdefault(key, value)

    baseline = load_json(Path(__file__).parent.parent.parent / "baseline_snapshot.json", {})
    measured = point.get("settle", {}).get("measured_current_a", [None, None, None])

    return {
        "schema_version": SCHEMA_VERSION,
        "run_id": run_id,
        "point_id": point["point_id"],
        "segment_id": segment["segment_id"],
        "calibration_id": calibration_snapshot.get("calibration_id") if calibration_snapshot else None,
        "smb_profile_id": smb_snapshot.get("profile_id") if smb_snapshot else None,
        "oe_profile_id": oe_snapshot.get("profile_id") if oe_snapshot else None,
        "laser_profile_id": laser_snapshot.get("profile_id") if laser_snapshot else None,
        "target_b_nt": point.get("target_b_nt"),
        "baseline_current_a": point.get("baseline_current_a"),
        "calibrated_delta_current_a": point.get("calibrated_delta_current_a"),
        "target_current_a": point.get("target_current_a"),
        "measured_current_a": measured,
        "rf": rf,
        "oe_fixed": oe_snapshot.get("fixed") if oe_snapshot else None,
        "oe_collector": oe_snapshot.get("collector") if oe_snapshot else None,
        "laser_mode": laser_snapshot.get("mode") if laser_snapshot else None,
        "laser_power_mw": laser_snapshot.get("power_mw") if laser_snapshot else None,
        "laser_settle_ms": laser_snapshot.get("settle_ms") if laser_snapshot else None,
    }


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        description="Reconstruct point_fields/*.npz from raw/oe1022d.rall + frames.idx + segments."
    )
    parser.add_argument("--run", required=True, help="Run artifact directory.")
    parser.add_argument(
        "--endian",
        choices=["big", "little"],
        default="big",
        help="Byte order of f64 values in RALL frames (default: big).",
    )
    parser.add_argument(
        "--skip-existing",
        action="store_true",
        help="Do not overwrite existing point_fields.jsonl or NPZ files.")
    args = parser.parse_args(argv)

    run_dir = Path(args.run).resolve()
    raw_path = run_dir / "raw" / "oe1022d.rall"
    index_path = run_dir / "raw" / "oe1022d.frames.idx.jsonl"
    segments_path = run_dir / "segments.jsonl"
    points_path = run_dir / "points.jsonl"
    point_fields_path = run_dir / "point_fields.jsonl"
    fields_dir = run_dir / "point_fields"

    if not raw_path.exists():
        print(f"missing raw file: {raw_path}", file=sys.stderr)
        return 2
    if not index_path.exists():
        print(f"missing frame index: {index_path}", file=sys.stderr)
        return 2
    if not segments_path.exists():
        print(f"missing segments: {segments_path}", file=sys.stderr)
        return 2
    if not points_path.exists():
        print(f"missing points: {points_path}", file=sys.stderr)
        return 2

    fields_dir.mkdir(parents=True, exist_ok=True)

    raw = raw_path.read_bytes()
    frames = load_jsonl(index_path)
    segments = load_jsonl(segments_path)
    points = load_jsonl(points_path)
    points_by_id = {p["point_id"]: p for p in points}
    segments_by_point = {s["point_id"]: s for s in segments}

    smb_snapshot = load_json(run_dir / "smb_profile_snapshot.json", {})
    oe_snapshot = load_json(run_dir / "oe_profile_snapshot.json", {})
    laser_snapshot = load_json(run_dir / "laser_profile_snapshot.json", {})
    calibration_snapshot = load_json(run_dir / "calibration_snapshot.json", {})
    run_manifest = load_json(run_dir / "run_manifest.json", {})
    run_id = run_manifest.get("run_id") or run_dir.name

    if args.skip_existing and point_fields_path.exists():
        print(f"point_fields.jsonl already exists, skipping (--skip-existing): {point_fields_path}")
        return 0

    written = 0
    with point_fields_path.open("w", encoding="utf-8") as pf_file:
        for point in points:
            pid = point["point_id"]
            segment = segments_by_point.get(pid)
            if segment is None:
                print(f"warning: no segment for point {pid}", file=sys.stderr)
                continue

            seg_id = segment["segment_id"]
            sidecar_name = f"{seg_id}.npz"
            manifest_name = f"{seg_id}.manifest.json"
            sidecar_path = fields_dir / sidecar_name
            manifest_path = fields_dir / manifest_name

            if args.skip_existing and sidecar_path.exists():
                print(f"skip existing {sidecar_path}")
                continue

            try:
                npz_data, _ = extract_segment(raw, frames, segment, args.endian)
            except Exception as exc:
                print(f"failed to extract {pid}: {exc}", file=sys.stderr)
                continue

            np.savez_compressed(sidecar_path, **npz_data)
            manifest = build_manifest(
                run_id, point, segment, smb_snapshot, oe_snapshot, laser_snapshot, calibration_snapshot
            )
            manifest_path.write_text(safe_json_dumps(manifest) + "\n", encoding="utf-8")

            row = build_point_fields_row(
                run_id, point, segment, npz_data,
                f"point_fields/{sidecar_name}",
                f"point_fields/{manifest_name}",
            )
            pf_file.write(safe_json_dumps(row) + "\n")
            written += 1
            print(f"wrote {pid}: {npz_data['b_x'].size} samples from {npz_data['frame_seq'].size} frames")

    print(f"wrote {written} point sidecars to {fields_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
