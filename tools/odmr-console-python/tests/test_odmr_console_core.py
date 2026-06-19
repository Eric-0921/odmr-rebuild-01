from __future__ import annotations

import tempfile
import os
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools" / "odmr-console-python"))

from odmr_console_core import (  # noqa: E402
    RunBundle,
    build_operator_metadata,
    control_paths_for_out_dir,
    demo_generator_request,
    generate_config_bundle,
    load_json,
    next_resume_out_dir,
    parse_operator_tags,
    process_is_running,
    read_progress,
    read_progress_since,
    read_jsonl_since,
    read_text_tail,
    request_emergency_stop,
    request_stop,
    resume_run_command,
    run_execute_command,
    start_run,
)
import odmr_console_core  # noqa: E402


class OdmrConsoleCoreTests(unittest.TestCase):
    def test_operator_metadata_parses_chinese_hash_tags(self) -> None:
        notes = "偏置加在样品左上角 #偏置 #低激光功率 #空气环境 #偏置"
        self.assertEqual(parse_operator_tags(notes), ["偏置", "低激光功率", "空气环境"])
        metadata = build_operator_metadata(" P-013 ", notes)
        self.assertEqual(metadata["probe_id"], "P-013")
        self.assertEqual(metadata["notes"], notes)
        self.assertEqual(metadata["tags"], ["偏置", "低激光功率", "空气环境"])
        self.assertIsNone(build_operator_metadata())

    def test_start_run_writes_operator_metadata(self) -> None:
        class FakeProcess:
            pid = 12345

        def fake_popen(*args, **kwargs):  # noqa: ANN001, ANN202
            return FakeProcess()

        original_popen = odmr_console_core.subprocess.Popen
        odmr_console_core.subprocess.Popen = fake_popen
        try:
            with tempfile.TemporaryDirectory() as tmp:
                bundle = RunBundle(
                    station_path="station.json",
                    calibration_path="calibration.json",
                    plan_path="plan.json",
                    smb_profile_path="smb.json",
                    oe_profile_path="oe.json",
                    laser_profile_path="laser.json",
                )
                out_dir = Path(tmp) / "run"
                operator_metadata = build_operator_metadata("P-013", "中文备注 #偏置")
                handle = start_run(bundle, out_dir, ROOT, "dotnet", operator_metadata)
                metadata = load_json(handle.metadata_path)
                self.assertEqual(metadata["operator_metadata"]["probe_id"], "P-013")
                self.assertEqual(metadata["operator_metadata"]["notes"], "中文备注 #偏置")
                self.assertEqual(metadata["operator_metadata"]["tags"], ["偏置"])
        finally:
            odmr_console_core.subprocess.Popen = original_popen

    def test_generate_config_bundle_reuses_config_generator_core(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            bundle = generate_config_bundle(demo_generator_request("console_core_test"), tmp, ROOT)
            self.assertTrue(Path(bundle.plan_path).exists())
            self.assertTrue(Path(bundle.smb_profile_path).exists())
            self.assertTrue(Path(bundle.oe_profile_path).exists())
            self.assertTrue(Path(bundle.laser_profile_path).exists())
            self.assertEqual(bundle.station_path, str(ROOT / "configs" / "stations" / "lab_a.json"))
            self.assertEqual(bundle.calibration_path, str(ROOT / "configs" / "calibrations" / "main.json"))

    def test_run_execute_command_uses_progress_and_stop_files(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            bundle = generate_config_bundle(demo_generator_request("console_command_test"), Path(tmp) / "generated", ROOT)
            out_dir = Path(tmp) / "run_out"
            control = control_paths_for_out_dir(out_dir)
            command = run_execute_command(bundle, out_dir, control, ROOT, "dotnet")
            self.assertIn("run-execute", command)
            self.assertIn("--progress-jsonl", command)
            self.assertIn(control.progress_jsonl, command)
            self.assertIn("--stop-request-file", command)
            self.assertIn(control.stop_request_file, command)
            self.assertIn("--emergency-stop-file", command)
            self.assertIn(control.emergency_stop_file, command)

    def test_resume_command_and_out_dir_allocation(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            previous_run = Path(tmp) / "run_a"
            previous_run.mkdir()
            allocated = next_resume_out_dir(previous_run)
            self.assertEqual(allocated.name, "run_a__resume_01")
            allocated.mkdir()
            next_allocated = next_resume_out_dir(previous_run)
            self.assertEqual(next_allocated.name, "run_a__resume_02")
            control = control_paths_for_out_dir(next_allocated)
            command = resume_run_command(previous_run, next_allocated, control, ROOT, "dotnet")
            self.assertIn("resume-run", command)
            self.assertIn("--previous-run", command)
            self.assertIn(str(previous_run), command)
            self.assertIn("--progress-jsonl", command)
            self.assertIn(control.progress_jsonl, command)

    def test_stop_request_and_progress_reading(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            stop_path = Path(tmp) / "control" / "stop.request"
            request_stop(stop_path)
            self.assertTrue(stop_path.exists())
            emergency_path = Path(tmp) / "control" / "emergency_stop.request"
            request_emergency_stop(emergency_path)
            self.assertTrue(emergency_path.exists())
            progress = Path(tmp) / "control" / "progress.jsonl"
            progress.parent.mkdir(parents=True, exist_ok=True)
            progress.write_text('{"event_name":"run_opened"}\n{"event_name":"collector_started"}\n', encoding="utf-8")
            self.assertEqual([record["event_name"] for record in read_progress(progress)], ["run_opened", "collector_started"])
            first_records, offset = read_progress_since(progress)
            self.assertEqual([record["event_name"] for record in first_records], ["run_opened", "collector_started"])
            with progress.open("a", encoding="utf-8") as handle:
                handle.write('{"event_name":"point_completed"}\n')
            next_records, next_offset = read_progress_since(progress, offset)
            self.assertEqual([record["event_name"] for record in next_records], ["point_completed"])
            self.assertGreater(next_offset, offset)

    def test_jsonl_reader_skips_bad_complete_lines_and_holds_incomplete_tail(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "progress.jsonl"
            path.write_text('{"event_name":"run_opened"}\nnot json\n{"event_name":"half"', encoding="utf-8")
            records, offset = read_jsonl_since(path)
            self.assertEqual([record["event_name"] for record in records], ["run_opened"])
            self.assertLess(offset, path.stat().st_size)

            with path.open("a", encoding="utf-8") as handle:
                handle.write('}\n{"event_name":"collector_started"}\n')
            next_records, next_offset = read_jsonl_since(path, offset)
            self.assertEqual(
                [record["event_name"] for record in next_records],
                ["half", "collector_started"],
            )
            self.assertEqual(next_offset, path.stat().st_size)

    def test_process_status_and_text_tail_helpers(self) -> None:
        self.assertTrue(process_is_running(os.getpid()))
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "log.txt"
            path.write_text("abcdef", encoding="utf-8")
            self.assertEqual(read_text_tail(path, 3), "def")
            self.assertEqual(read_text_tail(Path(tmp) / "missing.txt"), "")


if __name__ == "__main__":
    unittest.main()
