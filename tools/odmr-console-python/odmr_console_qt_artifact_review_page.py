from __future__ import annotations

import json
from pathlib import Path
import subprocess
from typing import Callable

from PySide6.QtWidgets import (
    QFileDialog,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QPlainTextEdit,
    QPushButton,
    QVBoxLayout,
    QWidget,
)

from odmr_console_core import command_to_text, load_json, winprobe_project, REPO_ROOT
from odmr_console_qt_shared import WorkerThread, note_label, section_label


class ArtifactReviewPage(QWidget):
    def __init__(self, out_dir_provider: Callable[[], str]) -> None:
        super().__init__()
        self.out_dir_provider = out_dir_provider
        self.worker: WorkerThread | None = None
        self.review_buttons: list[QPushButton] = []
        layout = QVBoxLayout(self)
        layout.addWidget(section_label("数据审查"))
        layout.addWidget(note_label("只读 run artifact。artifact-check 和 audit-continuity 都由 C# CLI 执行，不碰设备。"))
        input_row = QHBoxLayout()
        input_row.addWidget(QLabel("Run dir"))
        self.run_dir = QLineEdit()
        input_row.addWidget(self.run_dir, 1)
        use_current = QPushButton("Use Current Out-dir")
        use_current.clicked.connect(lambda: self.run_dir.setText(self.out_dir_provider()))
        browse = QPushButton("Browse...")
        browse.clicked.connect(self._browse)
        input_row.addWidget(use_current)
        input_row.addWidget(browse)
        layout.addLayout(input_row)
        actions = QHBoxLayout()
        self.check_button = QPushButton("检查 artifact")
        self.check_button.clicked.connect(self.artifact_check)
        self.audit_button = QPushButton("连续性审计")
        self.audit_button.clicked.connect(self.audit)
        self.review_buttons = [self.check_button, self.audit_button]
        actions.addWidget(self.check_button)
        actions.addWidget(self.audit_button)
        actions.addStretch(1)
        layout.addLayout(actions)
        self.output = QPlainTextEdit()
        self.output.setReadOnly(True)
        layout.addWidget(self.output, 1)

    def _browse(self) -> None:
        selected = QFileDialog.getExistingDirectory(self, "Select run directory", self.run_dir.text() or str(REPO_ROOT / "runs"))
        if selected:
            self.run_dir.setText(selected)

    def artifact_check(self) -> None:
        run_dir = self._run_dir()
        command = ["dotnet", "run", "--project", winprobe_project(), "--", "artifact-check", "--run", str(run_dir)]
        self._run_command(command)

    def audit(self) -> None:
        run_dir = self._run_dir()
        out_path = run_dir / "continuity_audit.json"
        command = ["dotnet", "run", "--project", winprobe_project(), "--", "audit-continuity", "--run", str(run_dir), "--out", str(out_path)]
        self._run_command(command, out_path)

    def _run_dir(self) -> Path:
        text = self.run_dir.text().strip() or self.out_dir_provider()
        self.run_dir.setText(text)
        return Path(text)

    def _run_command(self, command: list[str], json_out: Path | None = None) -> None:
        self.output.setPlainText(command_to_text(command))
        self._set_running(True)
        self.worker = WorkerThread(lambda: subprocess.run(command, cwd=REPO_ROOT, text=True, capture_output=True, check=False))
        self.worker.completed.connect(lambda result: self._done(result, json_out))
        self.worker.failed.connect(self._failed)
        self.worker.start()

    def _done(self, result: subprocess.CompletedProcess[str], json_out: Path | None) -> None:
        self._set_running(False)
        text = [f"returncode={result.returncode}"]
        if result.stdout:
            text.append("\nSTDOUT:\n" + result.stdout)
        if result.stderr:
            text.append("\nSTDERR:\n" + result.stderr)
        if json_out and json_out.exists():
            try:
                audit = load_json(json_out)
                text.insert(1, "\nAudit summary:\n" + json.dumps({
                    "verdict": audit.get("verdict"),
                    "frames_total": audit.get("frames_total"),
                    "delta0_count": audit.get("delta0_count"),
                    "delta1_count": audit.get("delta1_count"),
                    "delta_gt1_count": audit.get("delta_gt1_count"),
                    "estimated_missing_windows": audit.get("estimated_missing_windows"),
                }, indent=2, ensure_ascii=False))
            except Exception:
                pass
        self.output.setPlainText("\n".join(text))

    def _failed(self, message: str) -> None:
        self._set_running(False)
        self.output.setPlainText(message)

    def _set_running(self, running: bool) -> None:
        for button in self.review_buttons:
            button.setEnabled(not running)
