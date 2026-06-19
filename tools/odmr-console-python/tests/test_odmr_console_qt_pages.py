from __future__ import annotations

import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


os.environ.setdefault("QT_QPA_PLATFORM", "offscreen")

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools" / "odmr-console-python"))

try:
    from PySide6.QtWidgets import QApplication, QGroupBox, QMessageBox

    from odmr_console_core import ControlPaths, LaunchHandle
    from odmr_console_qt_artifact_review_page import ArtifactReviewPage
    from odmr_console_qt_config_generator_page import ConfigGeneratorPage
    from odmr_console_qt_run_monitor_page import RunMonitorPage
except Exception as exc:  # pragma: no cover - exercised only when Qt is unavailable.
    QT_IMPORT_ERROR = exc
else:
    QT_IMPORT_ERROR = None


@unittest.skipIf(QT_IMPORT_ERROR is not None, f"PySide6 unavailable: {QT_IMPORT_ERROR}")
class OdmrConsoleQtPageTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        cls.app = QApplication.instance() or QApplication([])

    def test_start_run_is_blocked_by_validate_provider(self) -> None:
        page = RunMonitorPage(
            lambda: self.fail("bundle_provider should not be called"),
            lambda: self.fail("out_dir_provider should not be called"),
            lambda: False,
        )
        warnings: list[str] = []
        original_warning = QMessageBox.warning
        QMessageBox.warning = lambda *args, **kwargs: warnings.append(str(args[2])) or QMessageBox.StandardButton.Ok
        try:
            page.start_run()
        finally:
            QMessageBox.warning = original_warning

        self.assertIsNone(page.worker)
        self.assertTrue(warnings)

    def test_started_emits_active_out_dir(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            out_dir = Path(tmp) / "run_a__resume_01"
            control = ControlPaths(
                control_dir=str(out_dir / "control"),
                progress_jsonl=str(out_dir / "control" / "progress.jsonl"),
                stop_request_file=str(out_dir / "control" / "stop.request"),
                emergency_stop_file=str(out_dir / "control" / "emergency_stop.request"),
                launch_metadata=str(out_dir / "control" / "launch_metadata.json"),
                stdout_log=str(out_dir / "control" / "stdout.log"),
                stderr_log=str(out_dir / "control" / "stderr.log"),
            )
            handle = LaunchHandle(
                pid=1,
                command=["dotnet"],
                cwd=str(ROOT),
                out_dir=str(out_dir),
                control_paths=control,
                metadata_path=control.launch_metadata,
            )
            page = RunMonitorPage(lambda: self.fail("unused"), lambda: self.fail("unused"), lambda: True)
            observed: list[str] = []
            page.active_out_dir_changed.connect(observed.append)
            page._started(handle)
            page.timer.stop()

            self.assertEqual(observed, [str(out_dir)])

    def test_artifact_audit_failure_does_not_read_stale_summary(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            run_dir = Path(tmp)
            final_path = run_dir / "continuity_audit.json"
            pending_path = run_dir / "continuity_audit.pending.json"
            final_path.write_text(json.dumps({"verdict": "old_stale"}), encoding="utf-8")
            pending_path.write_text(json.dumps({"verdict": "new_failed"}), encoding="utf-8")
            page = ArtifactReviewPage(lambda: str(run_dir))
            result = subprocess.CompletedProcess(["audit"], 1, stdout="", stderr="boom")

            page._done(result, pending_path, final_path)

            output = page.output.toPlainText()
            self.assertIn("returncode=1", output)
            self.assertNotIn("old_stale", output)
            self.assertFalse(pending_path.exists())
            self.assertTrue(final_path.exists())

    def test_artifact_audit_success_promotes_pending_summary(self) -> None:
        with tempfile.TemporaryDirectory() as tmp:
            run_dir = Path(tmp)
            final_path = run_dir / "continuity_audit.json"
            pending_path = run_dir / "continuity_audit.pending.json"
            pending_path.write_text(json.dumps({"verdict": "continuous", "frames_total": 3}), encoding="utf-8")
            page = ArtifactReviewPage(lambda: str(run_dir))
            result = subprocess.CompletedProcess(["audit"], 0, stdout="", stderr="")

            page._done(result, pending_path, final_path)

            self.assertFalse(pending_path.exists())
            self.assertEqual(json.loads(final_path.read_text(encoding="utf-8"))["verdict"], "continuous")
            self.assertIn("continuous", page.output.toPlainText())

    def test_oe_common_widgets_stay_in_common_header(self) -> None:
        page = ConfigGeneratorPage()

        self.assertIsInstance(page.oe_profile_id.parentWidget(), QGroupBox)
        self.assertEqual(page.oe_profile_id.parentWidget().title(), "通用信息")
        self.assertIsInstance(page.oe_command_settle.parentWidget(), QGroupBox)
        self.assertEqual(page.oe_command_settle.parentWidget().title(), "通用信息")

    def test_block_refresh_preserves_selected_row(self) -> None:
        page = ConfigGeneratorPage()
        page.blocks.append(page.default_block("y_line", "y"))
        page._refresh_block_list(1)
        self.assertEqual(page.block_list.currentRow(), 1)

        page.block_prefix.setText("updated_y")
        self.assertTrue(page._add_or_update_block())
        self.assertEqual(page.block_list.currentRow(), 1)
        self.assertEqual(page.blocks[1].prefix, "updated_y")

        first_new_row = len(page.blocks)
        page._add_xyz_blocks()
        self.assertEqual(page.block_list.currentRow(), first_new_row)


if __name__ == "__main__":
    unittest.main()
