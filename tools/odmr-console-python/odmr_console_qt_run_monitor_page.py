from __future__ import annotations

from dataclasses import asdict
import json
from pathlib import Path
import time
from typing import Any, Callable

from PySide6.QtCore import QTimer, Signal
from PySide6.QtWidgets import (
    QGridLayout,
    QGroupBox,
    QHBoxLayout,
    QLabel,
    QMessageBox,
    QPlainTextEdit,
    QProgressBar,
    QPushButton,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
    QWidget,
)

from odmr_console_core import (
    RunBundle,
    discard_run_dir,
    load_json,
    process_is_running,
    read_jsonl,
    read_progress_since,
    read_text_tail,
    request_emergency_stop,
    request_stop,
    start_resume,
    start_run,
)
from odmr_console_qt_shared import WorkerThread, add_display_row, format_duration, note_label, section_label


class RunMonitorPage(QWidget):
    active_out_dir_changed = Signal(str)

    def __init__(
        self,
        bundle_provider: Callable[[], RunBundle],
        out_dir_provider: Callable[[], str],
        validate_provider: Callable[[], bool] | None = None,
    ) -> None:
        super().__init__()
        self.bundle_provider = bundle_provider
        self.out_dir_provider = out_dir_provider
        self.validate_provider = validate_provider
        self.worker: WorkerThread | None = None
        self.handle: Any | None = None
        self.progress_offset = 0
        self.latest_progress: dict[str, Any] | None = None
        self.run_started_monotonic: float | None = None
        self.current_sweep_started_monotonic: float | None = None
        self.current_sweep_estimated_ms: int | None = None
        self.estimated_run_ms: int | None = None
        layout = QVBoxLayout(self)
        layout.addWidget(section_label("运行监控"))
        layout.addWidget(note_label("启动 C# run-execute / resume-run。进度来自 progress JSONL，stdout/stderr 只写入 control 日志，避免 pipe 阻塞。"))
        actions = QHBoxLayout()
        self.start_button = QPushButton("启动运行")
        self.start_button.clicked.connect(self.start_run)
        self.stop_button = QPushButton("当前点结束后暂停")
        self.stop_button.clicked.connect(self.stop_after_point)
        self.stop_button.setEnabled(False)
        self.resume_button = QPushButton("继续未完成 run")
        self.resume_button.clicked.connect(self.resume_run)
        self.resume_button.setEnabled(False)
        self.emergency_button = QPushButton("急停")
        self.emergency_button.clicked.connect(self.emergency_stop)
        self.emergency_button.setEnabled(False)
        self.emergency_button.setStyleSheet("QPushButton { color: white; background: #b00020; font-weight: 700; }")
        actions.addWidget(self.start_button)
        actions.addWidget(self.stop_button)
        actions.addWidget(self.resume_button)
        actions.addWidget(self.emergency_button)
        actions.addStretch(1)
        layout.addLayout(actions)
        status = QGroupBox("运行状态")
        status_layout = QGridLayout(status)
        self.state = QLabel("idle")
        self.station_id_label = QLabel("-")
        self.lockin_model_label = QLabel("-")
        self.collector_contract_label = QLabel("-")
        self.terminal_status_label = QLabel("-")
        self.point = QLabel("-")
        self.frames = QLabel("-")
        self.counts = QLabel("-")
        self.elapsed = QLabel("-")
        self.remaining = QLabel("-")
        add_display_row(status_layout, 0, "状态", self.state)
        add_display_row(status_layout, 1, "Station", self.station_id_label)
        add_display_row(status_layout, 2, "Lock-in", self.lockin_model_label)
        add_display_row(status_layout, 3, "Collector", self.collector_contract_label)
        add_display_row(status_layout, 4, "Terminal", self.terminal_status_label)
        add_display_row(status_layout, 5, "Point", self.point)
        add_display_row(status_layout, 6, "Frames", self.frames)
        add_display_row(status_layout, 7, "计数", self.counts)
        add_display_row(status_layout, 8, "已用时间", self.elapsed)
        add_display_row(status_layout, 9, "预计剩余", self.remaining)
        layout.addWidget(status)
        progress_group = QGroupBox("预计进度")
        progress_layout = QGridLayout(progress_group)
        self.total_progress = QProgressBar()
        self.total_progress.setRange(0, 1000)
        self.total_progress.setFormat("总进度 %p%")
        self.point_progress = QProgressBar()
        self.point_progress.setRange(0, 1000)
        self.point_progress.setFormat("当前 sweep %p%")
        progress_layout.addWidget(QLabel("总进度"), 0, 0)
        progress_layout.addWidget(self.total_progress, 0, 1)
        progress_layout.addWidget(QLabel("当前 sweep"), 1, 0)
        progress_layout.addWidget(self.point_progress, 1, 1)
        layout.addWidget(progress_group)
        self.table = QTableWidget(0, 8)
        self.table.setHorizontalHeaderLabels(["ts", "state", "event", "point", "idx", "frames", "delta_gt1", "quality"])
        self.table.setMinimumHeight(260)
        layout.addWidget(self.table, 1)
        self.log = QPlainTextEdit()
        self.log.setReadOnly(True)
        self.log.setMaximumHeight(150)
        layout.addWidget(self.log)
        self.timer = QTimer(self)
        self.timer.setInterval(500)
        self.timer.timeout.connect(self.refresh_progress)

    def start_run(self) -> None:
        if not self._validate_before_launch():
            return
        bundle = self.bundle_provider()
        out_dir = self.out_dir_provider()
        self.log.setPlainText(f"正在启动运行：{out_dir}...")
        self.start_button.setEnabled(False)
        self.worker = WorkerThread(lambda: start_run(bundle, out_dir))
        self.worker.completed.connect(self._started)
        self.worker.failed.connect(self._failed)
        self.worker.start()

    def _started(self, handle: Any) -> None:
        self.handle = handle
        self.active_out_dir_changed.emit(str(handle.out_dir))
        self.progress_offset = 0
        self.latest_progress = None
        self.run_started_monotonic = time.monotonic()
        self.current_sweep_started_monotonic = None
        self.current_sweep_estimated_ms = None
        self.estimated_run_ms = None
        self.total_progress.setValue(0)
        self.point_progress.setValue(0)
        self.table.setRowCount(0)
        self.station_id_label.setText("-")
        self.lockin_model_label.setText("-")
        self.collector_contract_label.setText("-")
        self.terminal_status_label.setText("-")
        self.stop_button.setEnabled(True)
        self.resume_button.setEnabled(False)
        self.emergency_button.setEnabled(True)
        self.log.setPlainText(json.dumps(asdict(handle), indent=2, ensure_ascii=False))
        self.timer.start()

    def _failed(self, message: str) -> None:
        self.start_button.setEnabled(True)
        self.stop_button.setEnabled(False)
        self.resume_button.setEnabled(False)
        self.emergency_button.setEnabled(False)
        self.timer.stop()
        self.log.setPlainText(message)

    def stop_after_point(self) -> None:
        if not self.handle:
            return
        request_stop(self.handle.control_paths.stop_request_file)
        self.log.appendPlainText(f"\npause requested: {self.handle.control_paths.stop_request_file}")

    def resume_run(self) -> None:
        if not self.handle:
            return
        if not self._validate_before_launch():
            return
        previous_out_dir = self.handle.out_dir
        self.log.appendPlainText(f"\n正在继续未完成 run：{previous_out_dir}")
        self.start_button.setEnabled(False)
        self.stop_button.setEnabled(False)
        self.resume_button.setEnabled(False)
        self.emergency_button.setEnabled(False)
        self.worker = WorkerThread(lambda: start_resume(previous_out_dir))
        self.worker.completed.connect(self._started)
        self.worker.failed.connect(self._failed)
        self.worker.start()

    def emergency_stop(self) -> None:
        if not self.handle:
            return
        reply = QMessageBox.warning(
            self,
            "确认急停",
            "急停会尽快关闭 SMB RF、Laser，并执行 M8812 cleanup。当前数据会先保留为 aborted run。",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No,
            QMessageBox.StandardButton.No,
        )
        if reply != QMessageBox.StandardButton.Yes:
            return
        request_emergency_stop(self.handle.control_paths.emergency_stop_file)
        self.emergency_button.setEnabled(False)
        self.state.setText("急停已请求，等待安全关闭")
        self.log.appendPlainText(f"\nemergency stop requested: {self.handle.control_paths.emergency_stop_file}")

    def refresh_progress(self) -> None:
        if not self.handle:
            return
        records, self.progress_offset = read_progress_since(
            self.handle.control_paths.progress_jsonl,
            self.progress_offset,
        )
        for record in records:
            self._append_record(record)
            self._observe_progress_record(record)
        if records:
            self.latest_progress = records[-1]
        if self.latest_progress:
            latest = self.latest_progress
            self.state.setText(str(latest.get("state", "-")))
            self.station_id_label.setText(self._load_station_id(self.handle.out_dir) if self.handle else "-")
            self.lockin_model_label.setText(str(latest.get("lockin_model") or self._load_run_field(self.handle.out_dir, "lockin_model") or "-"))
            self.collector_contract_label.setText(str(latest.get("collector_contract") or self._load_run_field(self.handle.out_dir, "collector_contract") or "-"))
            self.terminal_status_label.setText(self._load_run_status(self.handle.out_dir) or "-")
            self.point.setText(f"{latest.get('point_id') or '-'}  {latest.get('point_index') or '-'} / {latest.get('points_total') or '-'}")
            self.frames.setText(str(latest.get("frames_total") or "-"))
            self.counts.setText(
                f"timeout={latest.get('timeout_count')} raw_len_bad={latest.get('raw_len_bad_count')} delta_gt1={latest.get('delta_gt1_count')} decode={latest.get('decode_failures')}"
            )
            self._update_estimated_progress(latest)
            if latest.get("state") in {"Completed", "Failed", "Paused", "Aborted", "CleanupFailed"}:
                self.timer.stop()
                self.start_button.setEnabled(True)
                self.stop_button.setEnabled(False)
                self.resume_button.setEnabled(self._run_dir_is_resumable(self.handle.out_dir))
                self.emergency_button.setEnabled(False)
                if latest.get("state") in {"Failed", "CleanupFailed"}:
                    self._append_backend_log_tail()
                if latest.get("state") == "Completed":
                    self.total_progress.setValue(1000)
                    self.point_progress.setValue(1000)
                elif latest.get("state") == "Aborted":
                    self._ask_keep_or_discard()
                return
        else:
            self._update_estimated_progress(None)
        if not process_is_running(int(self.handle.pid)):
            self.timer.stop()
            self.start_button.setEnabled(True)
            self.stop_button.setEnabled(False)
            self.resume_button.setEnabled(self._run_dir_is_resumable(self.handle.out_dir))
            self.emergency_button.setEnabled(False)
            self.state.setText("process exited")
            self.terminal_status_label.setText(self._load_run_status(self.handle.out_dir) or "process_exited")
            stdout_tail = read_text_tail(self.handle.control_paths.stdout_log)
            stderr_tail = read_text_tail(self.handle.control_paths.stderr_log)
            self.log.appendPlainText(
                "\nC# run process exited before a terminal progress event was observed."
                "\n\nSTDOUT tail:\n"
                + (stdout_tail or "<empty>")
                + "\n\nSTDERR tail:\n"
                + (stderr_tail or "<empty>")
            )

    def _observe_progress_record(self, record: dict[str, Any]) -> None:
        if record.get("estimated_run_duration_ms"):
            self.estimated_run_ms = int(record["estimated_run_duration_ms"])
        if record.get("event_name") == "sweep_started":
            self.current_sweep_started_monotonic = time.monotonic()
            estimated = record.get("estimated_sweep_duration_ms")
            self.current_sweep_estimated_ms = int(estimated) if estimated else None
            self.point_progress.setValue(0)
        elif record.get("event_name") == "sweep_completed":
            self.point_progress.setValue(1000)

    def _update_estimated_progress(self, latest: dict[str, Any] | None) -> None:
        now = time.monotonic()
        if self.run_started_monotonic is not None:
            elapsed_sec = max(0.0, now - self.run_started_monotonic)
            self.elapsed.setText(format_duration(elapsed_sec))
            if self.estimated_run_ms:
                estimated_sec = self.estimated_run_ms / 1000.0
                ratio = min(0.99, elapsed_sec / estimated_sec) if estimated_sec > 0 else 0.0
                if latest and latest.get("state") == "Completed":
                    ratio = 1.0
                self.total_progress.setValue(int(ratio * 1000))
                remaining = estimated_sec - elapsed_sec
                self.remaining.setText(
                    "超过预计，等待设备完成/cleanup" if remaining < 0 else format_duration(remaining)
                )
            else:
                self.remaining.setText("-")

        if self.current_sweep_started_monotonic is not None and self.current_sweep_estimated_ms:
            elapsed_ms = (now - self.current_sweep_started_monotonic) * 1000.0
            ratio = min(0.99, elapsed_ms / max(1, self.current_sweep_estimated_ms))
            self.point_progress.setValue(int(ratio * 1000))

    def _ask_keep_or_discard(self) -> None:
        if not self.handle:
            return
        reply = QMessageBox.question(
            self,
            "急停已完成",
            "本次运行已安全急停。是否保留本次 partial artifact？选择 No 会移动到同级 _discarded，不会直接删除。",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No,
            QMessageBox.StandardButton.Yes,
        )
        if reply == QMessageBox.StandardButton.No:
            try:
                target = discard_run_dir(self.handle.out_dir)
                self.log.appendPlainText(f"\n已移动到：{target}")
            except Exception as exc:
                QMessageBox.warning(self, "丢弃失败", str(exc))

    def _run_dir_is_resumable(self, out_dir: str) -> bool:
        required = [
            Path(out_dir) / "station_snapshot.json",
            Path(out_dir) / "plan_snapshot.json",
            Path(out_dir) / "calibration_snapshot.json",
            Path(out_dir) / "smb_profile_snapshot.json",
            Path(out_dir) / "oe_profile_snapshot.json",
            Path(out_dir) / "laser_profile_snapshot.json",
            Path(out_dir) / "events.jsonl",
        ]
        if any(not path.exists() for path in required):
            return False

        status = self._load_run_status(out_dir)
        if status in {"completed", "aborted"}:
            return False
        if status in {"failed", "paused", "completed_with_failed_points"}:
            return self._has_remaining_points(out_dir)

        has_partial_facts = any(
            (Path(out_dir) / filename).exists()
            for filename in ["points.jsonl", "quality.jsonl", "device_state.jsonl"]
        )
        return has_partial_facts and self._has_remaining_points(out_dir)

    def _validate_before_launch(self) -> bool:
        if self.validate_provider is None:
            return True
        try:
            valid = self.validate_provider()
        except Exception as exc:
            QMessageBox.warning(self, "配置检查失败", str(exc))
            return False
        if not valid:
            QMessageBox.warning(self, "配置检查失败", "当前 station/profile/bundle 本地检查未通过，已阻止启动。")
            return False
        return True

    def _load_run_status(self, out_dir: str) -> str | None:
        for filename in ["summary.json", "run_manifest.json"]:
            path = Path(out_dir) / filename
            if not path.exists():
                continue
            try:
                status = load_json(path).get("status")
            except Exception:
                continue
            if isinstance(status, str) and status.strip():
                return status
        return None

    def _load_run_field(self, out_dir: str, field: str) -> str | None:
        for filename in ["summary.json", "run_manifest.json"]:
            path = Path(out_dir) / filename
            if not path.exists():
                continue
            try:
                value = load_json(path).get(field)
            except Exception:
                continue
            if isinstance(value, str) and value.strip():
                return value
        return None

    def _load_station_id(self, out_dir: str) -> str:
        station_snapshot = Path(out_dir) / "station_snapshot.json"
        if station_snapshot.exists():
            try:
                station_id = load_json(station_snapshot).get("station_id")
                if isinstance(station_id, str) and station_id.strip():
                    return station_id
            except Exception:
                pass
        return "-"

    def _has_remaining_points(self, out_dir: str) -> bool:
        total = self._points_total(out_dir)
        if total <= 0:
            return False
        return len(self._completed_point_ids(out_dir)) < total

    def _points_total(self, out_dir: str) -> int:
        plan_snapshot = Path(out_dir) / "plan_snapshot.json"
        if plan_snapshot.exists():
            try:
                resolved = load_json(plan_snapshot).get("resolved_point_count")
                if resolved is not None:
                    return int(resolved)
            except Exception:
                pass

        summary = Path(out_dir) / "summary.json"
        if summary.exists():
            try:
                points_total = load_json(summary).get("points_total")
                if points_total is not None:
                    return int(points_total)
            except Exception:
                pass

        return 0

    def _completed_point_ids(self, out_dir: str) -> set[str]:
        points_path = Path(out_dir) / "points.jsonl"
        quality_path = Path(out_dir) / "quality.jsonl"
        events_path = Path(out_dir) / "events.jsonl"

        point_ids = self._read_jsonl_ids(points_path, "point_id")
        quality_ids = {
            str(record.get("point_id"))
            for record in self._read_jsonl(quality_path)
            if record.get("quality_status") == "passed" and record.get("point_id")
        }
        completed_event_ids = {
            str(record.get("point_id"))
            for record in self._read_jsonl(events_path)
            if record.get("event") == "point_completed" and record.get("point_id")
        }
        return point_ids & quality_ids & completed_event_ids

    def _read_jsonl_ids(self, path: Path, field_name: str) -> set[str]:
        return {
            str(record.get(field_name))
            for record in self._read_jsonl(path)
            if record.get(field_name)
        }

    def _read_jsonl(self, path: Path) -> list[dict[str, Any]]:
        return read_jsonl(path)

    def _append_backend_log_tail(self) -> None:
        if not self.handle:
            return
        stdout_tail = read_text_tail(self.handle.control_paths.stdout_log)
        stderr_tail = read_text_tail(self.handle.control_paths.stderr_log)
        self.log.appendPlainText(
            "\n后端日志摘要:\nSTDOUT tail:\n"
            + (stdout_tail or "<empty>")
            + "\n\nSTDERR tail:\n"
            + (stderr_tail or "<empty>")
        )

    def _append_record(self, record: dict[str, Any]) -> None:
        row = self.table.rowCount()
        self.table.insertRow(row)
        values = [
            record.get("ts"),
            record.get("state"),
            record.get("event_name"),
            record.get("point_id"),
            record.get("point_index"),
            record.get("frames_total"),
            record.get("delta_gt1_count"),
            record.get("quality_status"),
        ]
        for column, value in enumerate(values):
            self.table.setItem(row, column, QTableWidgetItem("" if value is None else str(value)))
        self.table.scrollToBottom()
