from __future__ import annotations

import json
import subprocess
from typing import Any, Callable

from PySide6.QtWidgets import (
    QHBoxLayout,
    QPlainTextEdit,
    QPushButton,
    QVBoxLayout,
    QWidget,
)

from odmr_console_core import RunBundle, resolve_bundle
from odmr_console_qt_shared import WorkerThread, note_label, section_label


class ResolvePage(QWidget):
    def __init__(self, bundle_provider: Callable[[], RunBundle]) -> None:
        super().__init__()
        self.bundle_provider = bundle_provider
        self.worker: WorkerThread | None = None
        layout = QVBoxLayout(self)
        layout.addWidget(section_label("预检查 / 预计用时"))
        layout.addWidget(note_label("调用 C# run-resolve 验证当前六个 JSON，UI 不自己推断 runtime 行为。"))
        button_row = QHBoxLayout()
        self.resolve_button = QPushButton("执行预检查")
        self.resolve_button.clicked.connect(self.resolve)
        button_row.addWidget(self.resolve_button)
        button_row.addStretch(1)
        layout.addLayout(button_row)
        self.output = QPlainTextEdit()
        self.output.setReadOnly(True)
        layout.addWidget(self.output, 1)

    def resolve(self) -> None:
        bundle = self.bundle_provider()
        self.output.setPlainText("Running C# run-resolve...")
        self.resolve_button.setEnabled(False)
        self.worker = WorkerThread(lambda: resolve_bundle(bundle))
        self.worker.completed.connect(self._done)
        self.worker.failed.connect(self._failed)
        self.worker.start()

    def _done(self, result: subprocess.CompletedProcess[str]) -> None:
        self.resolve_button.setEnabled(True)
        text = []
        text.append(f"returncode={result.returncode}")
        if result.stdout:
            text.append("\nSTDOUT:\n" + result.stdout)
            try:
                parsed = json.loads(result.stdout)
                text.insert(1, self._summary(parsed))
            except Exception:
                pass
        if result.stderr:
            text.append("\nSTDERR:\n" + result.stderr)
        self.output.setPlainText("\n".join(text))

    def _failed(self, message: str) -> None:
        self.resolve_button.setEnabled(True)
        self.output.setPlainText(message)

    @staticmethod
    def _summary(parsed: dict[str, Any]) -> str:
        sweep = parsed.get("estimated_sweep") or {}
        return "\n".join([
            "Summary:",
            f"  run_id={parsed.get('run_id')}",
            f"  source_kind={parsed.get('source_kind')}",
            f"  resolved_point_count={parsed.get('resolved_point_count')}",
            f"  sweep_points={sweep.get('sweep_points')}",
            f"  sweep_duration_ms={sweep.get('sweep_duration_ms')}",
            f"  estimated_point_duration_ms={parsed.get('estimated_point_duration_ms')}",
            f"  estimated_run_duration_ms={parsed.get('estimated_run_duration_ms')}",
        ])
