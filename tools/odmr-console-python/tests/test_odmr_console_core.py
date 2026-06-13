from __future__ import annotations

import tempfile
from pathlib import Path
import sys
import unittest


ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools" / "odmr-console-python"))

from odmr_console_core import (  # noqa: E402
    control_paths_for_out_dir,
    demo_generator_request,
    generate_config_bundle,
    read_progress,
    request_stop,
    run_execute_command,
)


class OdmrConsoleCoreTests(unittest.TestCase):
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

    def test_stop_request_and_progress_reading(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            stop_path = Path(tmp) / "control" / "stop.request"
            request_stop(stop_path)
            self.assertTrue(stop_path.exists())
            progress = Path(tmp) / "control" / "progress.jsonl"
            progress.parent.mkdir(parents=True, exist_ok=True)
            progress.write_text('{"event_name":"run_opened"}\n{"event_name":"collector_started"}\n', encoding="utf-8")
            self.assertEqual([record["event_name"] for record in read_progress(progress)], ["run_opened", "collector_started"])


if __name__ == "__main__":
    unittest.main()
