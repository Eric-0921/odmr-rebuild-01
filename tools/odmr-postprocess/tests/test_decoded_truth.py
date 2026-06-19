from __future__ import annotations

import csv
import contextlib
import importlib.util
import io
import json
import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
POSTPROCESS = ROOT / "tools" / "odmr-postprocess"
sys.path.insert(0, str(POSTPROCESS))

from build_odmr_ml_dataset import main as build_ml_dataset  # noqa: E402
from decoded_truth import load_point_series_map  # noqa: E402


SAMPLE_COLUMNS = [
    "frame_seq",
    "global_sample_index",
    "b_x",
    "b_y",
    "b_noise",
    "b_input_overload",
    "b_gain_overload",
    "b_pll_locked",
]


def write_json(path: Path, value: object) -> None:
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_jsonl(path: Path, rows: list[dict[str, object]]) -> None:
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        for row in rows:
            handle.write(json.dumps(row, ensure_ascii=False) + "\n")


def write_sample_values(path: Path, columns: list[str] | None = None) -> None:
    fieldnames = columns or SAMPLE_COLUMNS
    rows = [
        {
            "frame_seq": 0,
            "global_sample_index": 0,
            "b_x": 1.0,
            "b_y": 2.0,
            "b_noise": 0.1,
            "b_input_overload": 0,
            "b_gain_overload": 0,
            "b_pll_locked": 1,
        },
        {
            "frame_seq": 1,
            "global_sample_index": 1,
            "b_x": 1.5,
            "b_y": 2.5,
            "b_noise": 0.2,
            "b_input_overload": 0,
            "b_gain_overload": 0,
            "b_pll_locked": 1,
        },
        {
            "frame_seq": 2,
            "global_sample_index": 2,
            "b_x": 2.0,
            "b_y": 3.0,
            "b_noise": 0.3,
            "b_input_overload": 0,
            "b_gain_overload": 0,
            "b_pll_locked": 1,
        },
        {
            "frame_seq": 3,
            "global_sample_index": 3,
            "b_x": 2.5,
            "b_y": 3.5,
            "b_noise": 0.4,
            "b_input_overload": 0,
            "b_gain_overload": 0,
            "b_pll_locked": 1,
        },
    ]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames, extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)


def make_run(run_dir: Path) -> None:
    write_jsonl(
        run_dir / "points.jsonl",
        [
            {
                "point_id": "p1",
                "target_b_nt": [1.0, 2.0, 3.0],
                "magnetic_mode": "controlled",
                "rf": {
                    "start_hz": 1000.0,
                    "stop_hz": 1001.0,
                    "step_hz": 1.0,
                    "dwell_ms": 2,
                    "power_dbm": -10.0,
                },
            }
        ],
    )
    write_jsonl(
        run_dir / "segments.jsonl",
        [{"point_id": "p1", "sample_index_start": 0, "sample_index_end": 4, "source_file": "sample_values.csv"}],
    )
    write_jsonl(run_dir / "quality.jsonl", [{"point_id": "p1", "quality_status": "passed"}])
    write_sample_values(run_dir / "sample_values.csv")
    write_json(run_dir / "smb_profile_snapshot.json", {"default_sweep": {}})
    write_json(run_dir / "oe_profile_snapshot.json", {"fixed": {}})
    write_json(run_dir / "laser_profile_snapshot.json", {"mode": "off_background", "power_mw": 0})


class DecodedTruthTests(unittest.TestCase):
    def test_load_point_series_map_reads_sample_values_by_segment(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            run_dir = Path(temp_dir)
            make_run(run_dir)
            series = load_point_series_map(run_dir, ["b_x", "b_y"])
            self.assertEqual(series["p1"]["global_sample_index"], [0.0, 1.0, 2.0, 3.0])
            self.assertEqual(series["p1"]["frame_seq"], [0.0, 1.0, 2.0, 3.0])
            self.assertEqual(series["p1"]["b_x"], [1.0, 1.5, 2.0, 2.5])
            self.assertEqual(series["p1"]["b_y"], [2.0, 2.5, 3.0, 3.5])

    def test_missing_sample_value_column_fails_fast(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            run_dir = Path(temp_dir)
            make_run(run_dir)
            write_sample_values(run_dir / "sample_values.csv", [column for column in SAMPLE_COLUMNS if column != "b_y"])
            with self.assertRaisesRegex(ValueError, "sample_values.csv missing columns: b_y"):
                load_point_series_map(run_dir, ["b_x", "b_y"])

    def test_missing_segment_field_fails_fast(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            run_dir = Path(temp_dir)
            make_run(run_dir)
            write_jsonl(run_dir / "segments.jsonl", [{"point_id": "p1", "sample_index_start": 0}])
            with self.assertRaisesRegex(ValueError, "segments.jsonl row 1 missing fields: sample_index_end"):
                load_point_series_map(run_dir, ["b_x"])

    @unittest.skipUnless(importlib.util.find_spec("numpy"), "numpy is required for ML dataset generation")
    def test_ml_dataset_sidecar_field_points_to_sample_values_csv(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            run_dir = Path(temp_dir) / "run"
            run_dir.mkdir()
            make_run(run_dir)
            out_dir = Path(temp_dir) / "out"
            with contextlib.redirect_stdout(io.StringIO()), contextlib.redirect_stderr(io.StringIO()):
                code = build_ml_dataset(
                    [
                        "--run",
                        str(run_dir),
                        "--out",
                        str(out_dir),
                        "--name-suffix",
                        "fixture",
                        "--smooth-window",
                        "1",
                    ]
                )
            self.assertEqual(code, 0)
            with (out_dir / "ml_samples_fixture.csv").open("r", encoding="utf-8", newline="") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(len(rows), 1)
            self.assertEqual(rows[0]["sidecar_npz"], "sample_values.csv")


if __name__ == "__main__":
    unittest.main()
