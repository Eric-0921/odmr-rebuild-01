from __future__ import annotations

import copy
import json
import sys
from pathlib import Path
from typing import Any

import pyqtgraph as pg
from PySide6.QtCore import QProcess, QStandardPaths, QTimer, Qt, QUrl, QObject, Signal
from PySide6.QtGui import QAction, QDesktopServices
from PySide6.QtNetwork import QNetworkAccessManager, QNetworkReply, QNetworkRequest
from PySide6.QtWebSockets import QWebSocket
from PySide6.QtWidgets import (
    QApplication,
    QComboBox,
    QFileDialog,
    QFormLayout,
    QGridLayout,
    QGroupBox,
    QHBoxLayout,
    QHeaderView,
    QLabel,
    QLineEdit,
    QListWidget,
    QListWidgetItem,
    QMainWindow,
    QMessageBox,
    QPushButton,
    QPlainTextEdit,
    QSpinBox,
    QDoubleSpinBox,
    QStackedWidget,
    QStatusBar,
    QTableWidget,
    QTableWidgetItem,
    QTabWidget,
    QTextEdit,
    QVBoxLayout,
    QWidget,
)

WORKSPACE_ROOT = Path(__file__).resolve().parents[2]
CONFIG_ROOT = WORKSPACE_ROOT / "configs"
SERVICE_HTTP = "http://127.0.0.1:8787"
SERVICE_WS = "ws://127.0.0.1:8787/v1/live"
DEFAULT_PLAN = {
    "run_id": "gui_run",
    "operator": "",
    "acquisition_window_ms": 0,
    "point_settle_ms": 500,
    "failure_policy": "abort_run",
    "mag_baseline_policy": {
        "baseline_current_a": [0.0, 0.0, 0.0],
        "settle_ms": 1000,
        "readback_samples": 3,
        "settle_tolerance_a": 0.001,
        "output_enabled": True,
    },
    "quality_thresholds": {
        "min_frames": 1,
        "max_timeout_count": 5,
        "max_duplicate_ratio": 0.5,
        "max_last_frame_age_ms": 1000,
    },
    "points": [{"point_id": "p000001", "target_b_nt": [0.0, 0.0, 0.0]}],
}
TRACE_FIELDS = ["B-X", "B-Y", "B-Freq", "B-Noise"]


def list_json_files(root: Path) -> list[Path]:
    if not root.exists():
        return []
    return sorted(root.glob("*.json"))


def parse_float_list(text: str) -> list[float]:
    parts = [chunk.strip() for chunk in text.replace("\n", ",").split(",")]
    values = [part for part in parts if part]
    if not values:
        raise ValueError("列表不能为空")
    return [float(value) for value in values]


def maybe_float(text: str) -> float | None:
    stripped = text.strip()
    return None if not stripped else float(stripped)


def maybe_int(text: str) -> int | None:
    stripped = text.strip()
    return None if not stripped else int(stripped)


def pretty_json(value: Any) -> str:
    return json.dumps(value, ensure_ascii=False, indent=2)


class DraftStore:
    def __init__(self) -> None:
        base = Path(QStandardPaths.writableLocation(QStandardPaths.StandardLocation.AppDataLocation))
        self.path = base / "draft.json"

    def load(self) -> dict[str, Any]:
        if not self.path.exists():
            return {}
        try:
            return json.loads(self.path.read_text(encoding="utf-8"))
        except Exception:
            return {}

    def save(self, payload: dict[str, Any]) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        self.path.write_text(pretty_json(payload), encoding="utf-8")


class RuntimeServiceClient(QObject):
    status_received = Signal(dict)
    recent_runs_received = Signal(list)
    verify_finished = Signal(dict)
    snapshot_finished = Signal(dict)
    run_started = Signal(dict)
    run_stopped = Signal(dict)
    request_failed = Signal(str, str)
    live_message = Signal(dict)
    socket_connected = Signal()
    socket_disconnected = Signal()

    def __init__(self, parent: QObject | None = None) -> None:
        super().__init__(parent)
        self.manager = QNetworkAccessManager(self)
        self.socket = QWebSocket()
        self.socket.connected.connect(self.socket_connected)
        self.socket.disconnected.connect(self.socket_disconnected)
        self.socket.textMessageReceived.connect(self._on_socket_text)

    def fetch_status(self) -> None:
        self._get("/v1/status", "status", self.status_received.emit)

    def fetch_recent_runs(self) -> None:
        self._get("/v1/runs/recent", "recent_runs", self.recent_runs_received.emit)

    def verify_station(self, station_path: str) -> None:
        self._post("/v1/verify-station", "verify_station", {"station_path": station_path}, self.verify_finished.emit)

    def hardware_snapshot(self, station_path: str) -> None:
        self._post("/v1/hardware-snapshot", "hardware_snapshot", {"station_path": station_path}, self.snapshot_finished.emit)

    def run_start(self, payload: dict[str, Any]) -> None:
        self._post("/v1/run/start", "run_start", payload, self.run_started.emit)

    def run_stop(self) -> None:
        self._post("/v1/run/stop", "run_stop", {}, self.run_stopped.emit)

    def connect_live(self) -> None:
        if self.socket.state() == QWebSocket.SocketState.ConnectedState:
            return
        self.socket.open(QUrl(SERVICE_WS))

    def disconnect_live(self) -> None:
        self.socket.close()

    def _get(self, path: str, label: str, on_ok) -> None:
        reply = self.manager.get(QNetworkRequest(QUrl(f"{SERVICE_HTTP}{path}")))
        reply.finished.connect(lambda: self._finish_json(reply, label, on_ok))

    def _post(self, path: str, label: str, payload: dict[str, Any], on_ok) -> None:
        request = QNetworkRequest(QUrl(f"{SERVICE_HTTP}{path}"))
        request.setHeader(QNetworkRequest.KnownHeaders.ContentTypeHeader, "application/json")
        data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        reply = self.manager.post(request, data)
        reply.finished.connect(lambda: self._finish_json(reply, label, on_ok))

    def _finish_json(self, reply: QNetworkReply, label: str, on_ok) -> None:
        body = bytes(reply.readAll()).decode("utf-8", errors="replace")
        if reply.error() != QNetworkReply.NetworkError.NoError:
            message = body
            try:
                parsed = json.loads(body)
                message = parsed.get("error", body)
            except Exception:
                message = body or reply.errorString()
            self.request_failed.emit(label, message)
            reply.deleteLater()
            return
        try:
            parsed = json.loads(body or "{}")
        except Exception as exc:
            self.request_failed.emit(label, f"JSON 解析失败: {exc}")
            reply.deleteLater()
            return
        on_ok(parsed)
        reply.deleteLater()

    def _on_socket_text(self, message: str) -> None:
        try:
            payload = json.loads(message)
        except Exception:
            return
        self.live_message.emit(payload)


class RuntimeProcess(QObject):
    log_message = Signal(str)

    def __init__(self, parent: QObject | None = None) -> None:
        super().__init__(parent)
        self.process = QProcess(self)
        self.process.setWorkingDirectory(str(WORKSPACE_ROOT))
        self.process.readyReadStandardError.connect(self._drain_stderr)
        self.process.readyReadStandardOutput.connect(self._drain_stdout)

    def ensure_running(self) -> None:
        if self.process.state() != QProcess.ProcessState.NotRunning:
            return
        binary = WORKSPACE_ROOT / "target" / "debug" / "odmr"
        if binary.exists():
            self.process.setProgram(str(binary))
            self.process.setArguments(["gui-bridge", "serve"])
        else:
            self.process.setProgram("cargo")
            self.process.setArguments(["run", "-p", "odmr-cli", "--", "gui-bridge", "serve"])
        self.process.start()
        self.log_message.emit("Starting local runtime service")

    def _drain_stdout(self) -> None:
        text = bytes(self.process.readAllStandardOutput()).decode("utf-8", errors="replace").strip()
        if text:
            self.log_message.emit(text)

    def _drain_stderr(self) -> None:
        text = bytes(self.process.readAllStandardError()).decode("utf-8", errors="replace").strip()
        if text:
            self.log_message.emit(text)


class MainWindow(QMainWindow):
    def __init__(self) -> None:
        super().__init__()
        self.client = RuntimeServiceClient(self)
        self.runtime_process = RuntimeProcess(self)
        self.draft_store = DraftStore()
        self.base_plan: dict[str, Any] = copy.deepcopy(DEFAULT_PLAN)
        self.service_status: dict[str, Any] = {}
        self.recent_runs: list[dict[str, Any]] = []
        self.live_traces: dict[str, tuple[list[float], list[float]]] = {}
        self.curves: dict[str, Any] = {}
        self.service_boot_attempted = False

        self.setWindowTitle("ODMR GUI")
        self.resize(1600, 980)
        self._build_ui()
        self._wire_signals()
        self.reload_config_lists()
        self.restore_draft()
        QTimer.singleShot(0, self.bootstrap_service)

    def _build_ui(self) -> None:
        root = QWidget()
        layout = QVBoxLayout(root)
        self.tabs = QTabWidget()
        self.tabs.addTab(self._build_setup_tab(), "Run Setup")
        self.tabs.addTab(self._build_live_tab(), "Live Monitor")
        self.tabs.addTab(self._build_recent_runs_tab(), "Recent Runs")
        layout.addWidget(self.tabs)
        self.setCentralWidget(root)

        self.status_bar = QStatusBar()
        self.setStatusBar(self.status_bar)
        open_action = QAction("Open Latest Artifacts", self)
        open_action.triggered.connect(self.open_latest_artifacts)
        self.menuBar().addAction(open_action)

    def _build_setup_tab(self) -> QWidget:
        page = QWidget()
        layout = QVBoxLayout(page)

        config_group = QGroupBox("Config Selection")
        config_form = QFormLayout(config_group)
        self.station_combo = QComboBox()
        self.calibration_combo = QComboBox()
        self.plan_combo = QComboBox()
        self.smb_combo = QComboBox()
        self.oe_combo = QComboBox()
        self.laser_combo = QComboBox()
        config_form.addRow("Station", self.station_combo)
        config_form.addRow("Calibration", self.calibration_combo)
        config_form.addRow("Plan Template", self.plan_combo)
        config_form.addRow("SMB Profile", self.smb_combo)
        config_form.addRow("OE Profile", self.oe_combo)
        config_form.addRow("Laser Profile", self.laser_combo)
        layout.addWidget(config_group)

        runtime_group = QGroupBox("Common Run Fields")
        runtime_form = QFormLayout(runtime_group)
        self.run_id_edit = QLineEdit()
        self.operator_edit = QLineEdit()
        self.point_settle_spin = QSpinBox()
        self.point_settle_spin.setRange(0, 3_600_000)
        self.failure_policy_combo = QComboBox()
        self.failure_policy_combo.addItem("abort_run")
        self.failure_policy_combo.addItem("continue")
        runtime_form.addRow("Run ID", self.run_id_edit)
        runtime_form.addRow("Operator", self.operator_edit)
        runtime_form.addRow("Point Settle (ms)", self.point_settle_spin)
        runtime_form.addRow("Failure Policy", self.failure_policy_combo)
        layout.addWidget(runtime_group)

        quality_group = QGroupBox("Quality Thresholds")
        quality_form = QFormLayout(quality_group)
        self.min_frames_spin = QSpinBox()
        self.min_frames_spin.setRange(0, 1_000_000)
        self.max_timeout_spin = QSpinBox()
        self.max_timeout_spin.setRange(0, 1_000_000)
        self.max_duplicate_spin = QDoubleSpinBox()
        self.max_duplicate_spin.setRange(0.0, 1.0)
        self.max_duplicate_spin.setSingleStep(0.01)
        self.max_last_age_spin = QSpinBox()
        self.max_last_age_spin.setRange(0, 1_000_000)
        quality_form.addRow("Min Frames", self.min_frames_spin)
        quality_form.addRow("Max Timeout Count", self.max_timeout_spin)
        quality_form.addRow("Max Duplicate Ratio", self.max_duplicate_spin)
        quality_form.addRow("Max Last Frame Age (ms)", self.max_last_age_spin)
        layout.addWidget(quality_group)

        point_group = QGroupBox("Point Source")
        point_layout = QVBoxLayout(point_group)
        self.point_mode_combo = QComboBox()
        self.point_mode_combo.addItem("cartesian_grid", "cartesian_grid")
        self.point_mode_combo.addItem("explicit_points", "explicit_points")
        point_layout.addWidget(self.point_mode_combo)
        self.point_stack = QStackedWidget()

        grid_page = QWidget()
        grid_form = QFormLayout(grid_page)
        self.grid_x_edit = QLineEdit("0")
        self.grid_y_edit = QLineEdit("0")
        self.grid_z_edit = QLineEdit("0")
        self.grid_cycle_combo = QComboBox()
        self.grid_cycle_combo.addItem("raster", "raster")
        self.grid_cycle_combo.addItem("bounce_1d_x", "bounce_1d_x")
        self.grid_total_points_spin = QSpinBox()
        self.grid_total_points_spin.setRange(1, 10_000_000)
        grid_form.addRow("X (comma-separated)", self.grid_x_edit)
        grid_form.addRow("Y", self.grid_y_edit)
        grid_form.addRow("Z", self.grid_z_edit)
        grid_form.addRow("Cycle Mode", self.grid_cycle_combo)
        grid_form.addRow("Fixed Total Points", self.grid_total_points_spin)
        self.point_stack.addWidget(grid_page)

        explicit_page = QWidget()
        explicit_layout = QVBoxLayout(explicit_page)
        self.explicit_points_edit = QPlainTextEdit(pretty_json(DEFAULT_PLAN["points"]))
        explicit_layout.addWidget(self.explicit_points_edit)
        self.point_stack.addWidget(explicit_page)

        point_layout.addWidget(self.point_stack)
        layout.addWidget(point_group)

        override_group = QGroupBox("High-Frequency Overrides")
        override_grid = QGridLayout(override_group)
        self.smb_start_edit = QLineEdit()
        self.smb_stop_edit = QLineEdit()
        self.smb_step_edit = QLineEdit()
        self.smb_dwell_edit = QLineEdit()
        self.smb_power_edit = QLineEdit()
        self.laser_mode_combo = QComboBox()
        self.laser_mode_combo.addItem("default", "")
        self.laser_mode_combo.addItem("on_background", "on_background")
        self.laser_mode_combo.addItem("off_background", "off_background")
        self.laser_power_edit = QLineEdit()
        self.laser_settle_edit = QLineEdit()

        override_grid.addWidget(QLabel("SMB Start Hz"), 0, 0)
        override_grid.addWidget(self.smb_start_edit, 0, 1)
        override_grid.addWidget(QLabel("SMB Stop Hz"), 0, 2)
        override_grid.addWidget(self.smb_stop_edit, 0, 3)
        override_grid.addWidget(QLabel("SMB Step Hz"), 1, 0)
        override_grid.addWidget(self.smb_step_edit, 1, 1)
        override_grid.addWidget(QLabel("SMB Dwell ms"), 1, 2)
        override_grid.addWidget(self.smb_dwell_edit, 1, 3)
        override_grid.addWidget(QLabel("SMB Power dBm"), 2, 0)
        override_grid.addWidget(self.smb_power_edit, 2, 1)
        override_grid.addWidget(QLabel("Laser Mode"), 2, 2)
        override_grid.addWidget(self.laser_mode_combo, 2, 3)
        override_grid.addWidget(QLabel("Laser Power mW"), 3, 0)
        override_grid.addWidget(self.laser_power_edit, 3, 1)
        override_grid.addWidget(QLabel("Laser Settle ms"), 3, 2)
        override_grid.addWidget(self.laser_settle_edit, 3, 3)
        layout.addWidget(override_group)

        buttons = QHBoxLayout()
        self.verify_button = QPushButton("Verify Station")
        self.snapshot_button = QPushButton("Snapshot")
        self.start_button = QPushButton("Start Run")
        self.stop_button = QPushButton("Stop Run")
        self.open_button = QPushButton("Open Latest Artifacts")
        self.reload_button = QPushButton("Reload Configs")
        buttons.addWidget(self.verify_button)
        buttons.addWidget(self.snapshot_button)
        buttons.addWidget(self.start_button)
        buttons.addWidget(self.stop_button)
        buttons.addWidget(self.open_button)
        buttons.addWidget(self.reload_button)
        layout.addLayout(buttons)
        layout.addStretch(1)
        return page

    def _build_live_tab(self) -> QWidget:
        page = QWidget()
        layout = QVBoxLayout(page)
        status_group = QGroupBox("Runtime State")
        status_grid = QGridLayout(status_group)
        self.connection_label = QLabel("Disconnected")
        self.run_state_label = QLabel("-")
        self.point_label = QLabel("-")
        self.progress_label = QLabel("-")
        self.eta_label = QLabel("-")
        self.target_label = QLabel("-")
        self.measured_label = QLabel("-")
        self.collector_label = QLabel("-")
        self.timeout_label = QLabel("-")
        self.duplicate_label = QLabel("-")
        self.frame_rate_label = QLabel("-")
        fields = [
            ("Connection", self.connection_label),
            ("Run State", self.run_state_label),
            ("Current Point", self.point_label),
            ("Progress", self.progress_label),
            ("ETA", self.eta_label),
            ("Target B (nT)", self.target_label),
            ("Measured Current (A)", self.measured_label),
            ("Collector", self.collector_label),
            ("Timeouts", self.timeout_label),
            ("Duplicates", self.duplicate_label),
            ("Frame Rate", self.frame_rate_label),
        ]
        for index, (label, widget) in enumerate(fields):
            status_grid.addWidget(QLabel(label), index // 2, (index % 2) * 2)
            status_grid.addWidget(widget, index // 2, (index % 2) * 2 + 1)
        layout.addWidget(status_group)

        plots_group = QGroupBox("Live Traces")
        plots_layout = QGridLayout(plots_group)
        pg.setConfigOptions(antialias=False)
        for index, field in enumerate(TRACE_FIELDS):
            plot = pg.PlotWidget()
            plot.setBackground("w")
            plot.showGrid(x=True, y=True, alpha=0.2)
            plot.setLabel("bottom", "Seconds")
            plot.setLabel("left", field)
            curve = plot.plot(pen=pg.mkPen(width=1.5, color=(20, 80 + index * 30, 160)))
            self.curves[field] = curve
            plots_layout.addWidget(plot, index // 2, index % 2)
        layout.addWidget(plots_group, 1)

        events_group = QGroupBox("Recent Events")
        events_layout = QVBoxLayout(events_group)
        self.event_list = QListWidget()
        events_layout.addWidget(self.event_list)
        layout.addWidget(events_group, 1)
        return page

    def _build_recent_runs_tab(self) -> QWidget:
        page = QWidget()
        layout = QVBoxLayout(page)
        top = QHBoxLayout()
        self.refresh_recent_button = QPushButton("Refresh")
        self.open_selected_run_button = QPushButton("Open Selected")
        top.addWidget(self.refresh_recent_button)
        top.addWidget(self.open_selected_run_button)
        top.addStretch(1)
        layout.addLayout(top)

        self.recent_runs_table = QTableWidget(0, 4)
        self.recent_runs_table.setHorizontalHeaderLabels(["Run ID", "Status", "Started", "Output Dir"])
        header = self.recent_runs_table.horizontalHeader()
        header.setSectionResizeMode(0, QHeaderView.ResizeMode.ResizeToContents)
        header.setSectionResizeMode(1, QHeaderView.ResizeMode.ResizeToContents)
        header.setSectionResizeMode(2, QHeaderView.ResizeMode.ResizeToContents)
        header.setSectionResizeMode(3, QHeaderView.ResizeMode.Stretch)
        layout.addWidget(self.recent_runs_table)
        return page

    def _wire_signals(self) -> None:
        self.client.status_received.connect(self.on_service_status)
        self.client.recent_runs_received.connect(self.on_recent_runs)
        self.client.verify_finished.connect(self.on_verify_finished)
        self.client.snapshot_finished.connect(self.on_snapshot_finished)
        self.client.run_started.connect(self.on_run_started)
        self.client.run_stopped.connect(self.on_run_stopped)
        self.client.request_failed.connect(self.on_request_failed)
        self.client.live_message.connect(self.on_live_message)
        self.client.socket_connected.connect(lambda: self.connection_label.setText("Connected"))
        self.client.socket_connected.connect(lambda: self.status_bar.showMessage("Live connection attached"))
        self.client.socket_disconnected.connect(self.on_live_disconnected)
        self.runtime_process.log_message.connect(self.status_bar.showMessage)

        self.reload_button.clicked.connect(self.reload_config_lists)
        self.plan_combo.currentIndexChanged.connect(self.load_plan_template)
        self.point_mode_combo.currentIndexChanged.connect(self.on_point_mode_changed)
        self.verify_button.clicked.connect(self.on_verify_clicked)
        self.snapshot_button.clicked.connect(self.on_snapshot_clicked)
        self.start_button.clicked.connect(self.on_start_clicked)
        self.stop_button.clicked.connect(self.client.run_stop)
        self.open_button.clicked.connect(self.open_latest_artifacts)
        self.refresh_recent_button.clicked.connect(self.client.fetch_recent_runs)
        self.open_selected_run_button.clicked.connect(self.open_selected_recent_run)

        watched = [
            self.station_combo,
            self.calibration_combo,
            self.plan_combo,
            self.smb_combo,
            self.oe_combo,
            self.laser_combo,
            self.run_id_edit,
            self.operator_edit,
            self.point_settle_spin,
            self.failure_policy_combo,
            self.min_frames_spin,
            self.max_timeout_spin,
            self.max_duplicate_spin,
            self.max_last_age_spin,
            self.point_mode_combo,
            self.grid_x_edit,
            self.grid_y_edit,
            self.grid_z_edit,
            self.grid_cycle_combo,
            self.grid_total_points_spin,
            self.explicit_points_edit,
            self.smb_start_edit,
            self.smb_stop_edit,
            self.smb_step_edit,
            self.smb_dwell_edit,
            self.smb_power_edit,
            self.laser_mode_combo,
            self.laser_power_edit,
            self.laser_settle_edit,
        ]
        for widget in watched:
            if isinstance(widget, QComboBox):
                widget.currentIndexChanged.connect(self.save_draft)
            elif isinstance(widget, (QLineEdit, QTextEdit, QPlainTextEdit)):
                widget.textChanged.connect(self.save_draft)
            elif isinstance(widget, (QSpinBox, QDoubleSpinBox)):
                widget.valueChanged.connect(self.save_draft)

    def bootstrap_service(self) -> None:
        self.client.fetch_status()

    def reload_config_lists(self) -> None:
        selections = self.snapshot_path_selections()
        self.populate_combo(self.station_combo, list_json_files(CONFIG_ROOT / "stations"))
        self.populate_combo(self.calibration_combo, list_json_files(CONFIG_ROOT / "calibrations"))
        self.populate_combo(self.plan_combo, list_json_files(CONFIG_ROOT / "plans"))
        self.populate_combo(self.smb_combo, [path for path in list_json_files(CONFIG_ROOT / "profiles") if "smb100a" in path.name])
        self.populate_combo(self.oe_combo, [path for path in list_json_files(CONFIG_ROOT / "profiles") if "oe1022d" in path.name])
        self.populate_combo(self.laser_combo, [path for path in list_json_files(CONFIG_ROOT / "profiles") if "laser" in path.name])
        self.restore_path_selections(selections)
        if self.plan_combo.count() and not self.base_plan:
            self.load_plan_template()

    def populate_combo(self, combo: QComboBox, paths: list[Path]) -> None:
        current = combo.currentData()
        combo.blockSignals(True)
        combo.clear()
        for path in paths:
            combo.addItem(path.relative_to(WORKSPACE_ROOT).as_posix(), str(path))
        combo.blockSignals(False)
        if current:
            self.select_combo_by_data(combo, current)

    def snapshot_path_selections(self) -> dict[str, str]:
        return {
            "station": self.station_combo.currentData() or "",
            "calibration": self.calibration_combo.currentData() or "",
            "plan": self.plan_combo.currentData() or "",
            "smb": self.smb_combo.currentData() or "",
            "oe": self.oe_combo.currentData() or "",
            "laser": self.laser_combo.currentData() or "",
        }

    def restore_path_selections(self, selections: dict[str, str]) -> None:
        mapping = {
            self.station_combo: selections.get("station"),
            self.calibration_combo: selections.get("calibration"),
            self.plan_combo: selections.get("plan"),
            self.smb_combo: selections.get("smb"),
            self.oe_combo: selections.get("oe"),
            self.laser_combo: selections.get("laser"),
        }
        for combo, value in mapping.items():
            if value:
                self.select_combo_by_data(combo, value)
        if self.plan_combo.currentData():
            self.load_plan_template()

    def select_combo_by_data(self, combo: QComboBox, data: str) -> None:
        for index in range(combo.count()):
            if combo.itemData(index) == data:
                combo.setCurrentIndex(index)
                return

    def restore_draft(self) -> None:
        draft = self.draft_store.load()
        if not draft:
            return
        self.restore_path_selections(draft.get("paths", {}))
        self.run_id_edit.setText(draft.get("run_id", ""))
        self.operator_edit.setText(draft.get("operator", ""))
        self.point_settle_spin.setValue(int(draft.get("point_settle_ms", self.point_settle_spin.value())))
        self.failure_policy_combo.setCurrentText(draft.get("failure_policy", "abort_run"))
        quality = draft.get("quality_thresholds", {})
        self.min_frames_spin.setValue(int(quality.get("min_frames", self.min_frames_spin.value())))
        self.max_timeout_spin.setValue(int(quality.get("max_timeout_count", self.max_timeout_spin.value())))
        self.max_duplicate_spin.setValue(float(quality.get("max_duplicate_ratio", self.max_duplicate_spin.value())))
        self.max_last_age_spin.setValue(int(quality.get("max_last_frame_age_ms", self.max_last_age_spin.value())))

        point_mode = draft.get("point_mode", "cartesian_grid")
        self.point_mode_combo.setCurrentIndex(0 if point_mode == "cartesian_grid" else 1)
        self.grid_x_edit.setText(draft.get("grid_x", "0"))
        self.grid_y_edit.setText(draft.get("grid_y", "0"))
        self.grid_z_edit.setText(draft.get("grid_z", "0"))
        self.grid_cycle_combo.setCurrentText(draft.get("grid_cycle_mode", "raster"))
        self.grid_total_points_spin.setValue(int(draft.get("grid_total_points", self.grid_total_points_spin.value())))
        self.explicit_points_edit.setPlainText(draft.get("explicit_points_json", self.explicit_points_edit.toPlainText()))

        smb_override = draft.get("smb_override", {})
        self.smb_start_edit.setText(smb_override.get("start_hz", ""))
        self.smb_stop_edit.setText(smb_override.get("stop_hz", ""))
        self.smb_step_edit.setText(smb_override.get("step_hz", ""))
        self.smb_dwell_edit.setText(smb_override.get("dwell_ms", ""))
        self.smb_power_edit.setText(smb_override.get("power_dbm", ""))
        self.laser_mode_combo.setCurrentIndex(max(0, self.laser_mode_combo.findData(draft.get("laser_mode", ""))))
        self.laser_power_edit.setText(draft.get("laser_power_mw", ""))
        self.laser_settle_edit.setText(draft.get("laser_settle_ms", ""))

    def save_draft(self) -> None:
        payload = {
            "paths": self.snapshot_path_selections(),
            "run_id": self.run_id_edit.text(),
            "operator": self.operator_edit.text(),
            "point_settle_ms": self.point_settle_spin.value(),
            "failure_policy": self.failure_policy_combo.currentText(),
            "quality_thresholds": {
                "min_frames": self.min_frames_spin.value(),
                "max_timeout_count": self.max_timeout_spin.value(),
                "max_duplicate_ratio": self.max_duplicate_spin.value(),
                "max_last_frame_age_ms": self.max_last_age_spin.value(),
            },
            "point_mode": self.point_mode_combo.currentData(),
            "grid_x": self.grid_x_edit.text(),
            "grid_y": self.grid_y_edit.text(),
            "grid_z": self.grid_z_edit.text(),
            "grid_cycle_mode": self.grid_cycle_combo.currentData(),
            "grid_total_points": self.grid_total_points_spin.value(),
            "explicit_points_json": self.explicit_points_edit.toPlainText(),
            "smb_override": {
                "start_hz": self.smb_start_edit.text(),
                "stop_hz": self.smb_stop_edit.text(),
                "step_hz": self.smb_step_edit.text(),
                "dwell_ms": self.smb_dwell_edit.text(),
                "power_dbm": self.smb_power_edit.text(),
            },
            "laser_mode": self.laser_mode_combo.currentData(),
            "laser_power_mw": self.laser_power_edit.text(),
            "laser_settle_ms": self.laser_settle_edit.text(),
        }
        self.draft_store.save(payload)

    def load_plan_template(self) -> None:
        path = self.plan_combo.currentData()
        if not path:
            return
        try:
            self.base_plan = json.loads(Path(path).read_text(encoding="utf-8"))
        except Exception as exc:
            self.show_error(f"Plan template load failed: {exc}")
            return

        self.run_id_edit.setText(self.base_plan.get("run_id", ""))
        self.operator_edit.setText(self.base_plan.get("operator", ""))
        self.point_settle_spin.setValue(int(self.base_plan.get("point_settle_ms", 0)))
        self.failure_policy_combo.setCurrentText(self.base_plan.get("failure_policy", "abort_run"))
        quality = self.base_plan.get("quality_thresholds", {})
        self.min_frames_spin.setValue(int(quality.get("min_frames", 0)))
        self.max_timeout_spin.setValue(int(quality.get("max_timeout_count", 0)))
        self.max_duplicate_spin.setValue(float(quality.get("max_duplicate_ratio", 0.0)))
        self.max_last_age_spin.setValue(int(quality.get("max_last_frame_age_ms", 0)))

        if self.base_plan.get("point_source"):
            point_source = self.base_plan["point_source"]
            self.point_mode_combo.setCurrentIndex(0)
            axes = point_source.get("axes_nt", {})
            self.grid_x_edit.setText(", ".join(str(value) for value in axes.get("x", [0])))
            self.grid_y_edit.setText(", ".join(str(value) for value in axes.get("y", [0])))
            self.grid_z_edit.setText(", ".join(str(value) for value in axes.get("z", [0])))
            self.grid_cycle_combo.setCurrentIndex(max(0, self.grid_cycle_combo.findData(point_source.get("cycle_mode", "raster"))))
            total_points = point_source.get("stop_condition", {}).get("total_points", 1)
            self.grid_total_points_spin.setValue(int(total_points))
        else:
            self.point_mode_combo.setCurrentIndex(1)
            self.explicit_points_edit.setPlainText(pretty_json(self.base_plan.get("points", [])))
        self.save_draft()

    def on_point_mode_changed(self) -> None:
        self.point_stack.setCurrentIndex(0 if self.point_mode_combo.currentData() == "cartesian_grid" else 1)
        self.save_draft()

    def on_verify_clicked(self) -> None:
        station = self.station_combo.currentData()
        if not station:
            self.show_error("Station path is required")
            return
        self.client.verify_station(station)

    def on_snapshot_clicked(self) -> None:
        station = self.station_combo.currentData()
        if not station:
            self.show_error("Station path is required")
            return
        self.client.hardware_snapshot(station)

    def on_start_clicked(self) -> None:
        try:
            payload = self.build_run_start_payload()
        except Exception as exc:
            self.show_error(str(exc))
            return
        self.client.run_start(payload)

    def build_run_start_payload(self) -> dict[str, Any]:
        station_path = self.station_combo.currentData()
        calibration_path = self.calibration_combo.currentData()
        smb_profile_path = self.smb_combo.currentData()
        oe_profile_path = self.oe_combo.currentData()
        laser_profile_path = self.laser_combo.currentData()
        missing = [
            label
            for label, value in {
                "station": station_path,
                "calibration": calibration_path,
                "smb_profile": smb_profile_path,
                "oe_profile": oe_profile_path,
                "laser_profile": laser_profile_path,
            }.items()
            if not value
        ]
        if missing:
            raise ValueError(f"Missing required config selection: {', '.join(missing)}")

        plan = copy.deepcopy(self.base_plan or DEFAULT_PLAN)
        plan["run_id"] = self.run_id_edit.text().strip() or plan.get("run_id", "gui_run")
        plan["operator"] = self.operator_edit.text().strip()
        plan["point_settle_ms"] = self.point_settle_spin.value()
        plan["failure_policy"] = self.failure_policy_combo.currentText()
        plan["quality_thresholds"] = {
            "min_frames": self.min_frames_spin.value(),
            "max_timeout_count": self.max_timeout_spin.value(),
            "max_duplicate_ratio": self.max_duplicate_spin.value(),
            "max_last_frame_age_ms": self.max_last_age_spin.value(),
        }

        if self.point_mode_combo.currentData() == "cartesian_grid":
            plan["points"] = []
            plan["point_source"] = {
                "kind": "cartesian_grid",
                "axes_nt": {
                    "x": parse_float_list(self.grid_x_edit.text()),
                    "y": parse_float_list(self.grid_y_edit.text()),
                    "z": parse_float_list(self.grid_z_edit.text()),
                },
                "order": ["x", "y", "z"],
                "cycle_mode": self.grid_cycle_combo.currentData(),
                "stop_condition": {
                    "kind": "fixed_total_points",
                    "total_points": self.grid_total_points_spin.value(),
                },
            }
        else:
            plan["point_source"] = None
            plan["points"] = json.loads(self.explicit_points_edit.toPlainText())

        smb_override = {
            "start_hz": maybe_float(self.smb_start_edit.text()),
            "stop_hz": maybe_float(self.smb_stop_edit.text()),
            "step_hz": maybe_float(self.smb_step_edit.text()),
            "dwell_ms": maybe_int(self.smb_dwell_edit.text()),
            "power_dbm": maybe_float(self.smb_power_edit.text()),
        }
        smb_override = {key: value for key, value in smb_override.items() if value is not None}

        laser_override = {
            "mode": self.laser_mode_combo.currentData() or None,
            "power_mw": maybe_int(self.laser_power_edit.text()),
            "settle_ms": maybe_int(self.laser_settle_edit.text()),
        }
        laser_override = {key: value for key, value in laser_override.items() if value is not None}

        return {
            "station_path": station_path,
            "calibration_path": calibration_path,
            "smb_profile_path": smb_profile_path,
            "oe_profile_path": oe_profile_path,
            "laser_profile_path": laser_profile_path,
            "artifact_mode": "lightweight",
            "draft_plan": plan,
            "smb_default_sweep_override": smb_override or None,
            "laser_override": laser_override or None,
        }

    def on_service_status(self, payload: dict[str, Any]) -> None:
        self.service_status = payload
        self.connection_label.setText("Connected")
        active_run_id = payload.get("active_run_id")
        self.run_state_label.setText(payload.get("service_state", "-"))
        self.status_bar.showMessage(f"Service ready: {payload.get('workspace_root', '')}")
        self.client.connect_live()
        self.update_button_state(active_run_id is not None)
        self.on_recent_runs(payload.get("recent_runs", []))

    def on_recent_runs(self, runs: list[dict[str, Any]]) -> None:
        self.recent_runs = runs
        self.recent_runs_table.setRowCount(len(runs))
        for row, run in enumerate(runs):
            self.recent_runs_table.setItem(row, 0, QTableWidgetItem(run.get("run_id", "")))
            self.recent_runs_table.setItem(row, 1, QTableWidgetItem(run.get("status", "")))
            self.recent_runs_table.setItem(row, 2, QTableWidgetItem(run.get("started_at", "")))
            self.recent_runs_table.setItem(row, 3, QTableWidgetItem(run.get("output_dir", "")))

    def on_verify_finished(self, payload: dict[str, Any]) -> None:
        self.status_bar.showMessage(f"Station verify saved to {payload.get('snapshot_path', '')}")
        QMessageBox.information(
            self,
            "Verify Station",
            pretty_json(payload),
        )

    def on_snapshot_finished(self, payload: dict[str, Any]) -> None:
        self.status_bar.showMessage(f"Hardware snapshot saved to {payload.get('snapshot_path', '')}")
        QMessageBox.information(
            self,
            "Hardware Snapshot",
            pretty_json(payload),
        )

    def on_run_started(self, payload: dict[str, Any]) -> None:
        self.status_bar.showMessage(f"Run accepted: {payload.get('run_id', '')}")
        self.update_button_state(True)
        self.tabs.setCurrentIndex(1)
        self.client.fetch_status()

    def on_run_stopped(self, payload: dict[str, Any]) -> None:
        self.status_bar.showMessage(f"Run stop finished: {payload.get('run_id', '')}")
        self.client.fetch_status()

    def on_request_failed(self, op: str, message: str) -> None:
        if op == "status" and not self.service_boot_attempted:
            self.service_boot_attempted = True
            self.connection_label.setText("Starting service")
            self.runtime_process.ensure_running()
            QTimer.singleShot(1500, self.client.fetch_status)
            return
        self.connection_label.setText("Disconnected")
        self.status_bar.showMessage(f"{op} failed: {message}")
        if op != "status":
            self.show_error(message)
        self.update_button_state(False)

    def on_live_message(self, payload: dict[str, Any]) -> None:
        event_type = payload.get("type")
        data = payload.get("payload", {})
        if event_type == "service_status":
            self.service_status = data
            self.update_button_state(bool(data.get("active_run_id")))
        elif event_type in {"run_state", "run_finished"}:
            self.run_state_label.setText(data.get("status", "-"))
            if event_type == "run_finished":
                self.update_button_state(False)
                self.client.fetch_status()
                self.client.fetch_recent_runs()
        elif event_type == "point_progress":
            current = data.get("current_point_id") or "-"
            self.point_label.setText(current)
            progress = float(data.get("progress_ratio", 0.0)) * 100.0
            completed = data.get("completed_points", 0)
            total = data.get("total_points", 0)
            self.progress_label.setText(f"{completed}/{total} ({progress:.1f}%)")
            eta = data.get("eta_seconds")
            self.eta_label.setText(f"{eta:.1f}s" if eta is not None else "-")
            self.target_label.setText(self.format_triplet(data.get("target_b_nt")))
            self.measured_label.setText(self.format_triplet(data.get("measured_current_a")))
        elif event_type == "collector_health":
            self.collector_label.setText(data.get("health", "-"))
            self.timeout_label.setText(str(data.get("timeout_count", "-")))
            self.duplicate_label.setText(str(data.get("duplicate_count", "-")))
            self.frame_rate_label.setText(f"{float(data.get('frame_rate_hz', 0.0)):.1f} Hz")
        elif event_type == "trace_window":
            field = data.get("field")
            if field in self.curves:
                x_values = data.get("x", [])
                y_values = data.get("y", [])
                if x_values:
                    last = x_values[-1]
                    shifted = [float(value - last) / 1000.0 for value in x_values]
                    self.curves[field].setData(shifted, y_values)
        elif event_type == "recent_event":
            item = QListWidgetItem(self.format_event_line(data))
            self.event_list.insertItem(0, item)
            while self.event_list.count() > 200:
                self.event_list.takeItem(self.event_list.count() - 1)

    def on_live_disconnected(self) -> None:
        self.connection_label.setText("Disconnected")
        QTimer.singleShot(1000, self.client.connect_live)

    def update_button_state(self, active_run: bool) -> None:
        connected = bool(self.service_status)
        self.verify_button.setEnabled(connected and not active_run)
        self.snapshot_button.setEnabled(connected and not active_run)
        self.start_button.setEnabled(connected and not active_run)
        self.stop_button.setEnabled(connected and active_run)
        self.open_button.setEnabled(bool(self.latest_output_dir()))

    def open_latest_artifacts(self) -> None:
        path = self.latest_output_dir()
        if not path:
            self.show_error("No recent artifacts found")
            return
        QDesktopServices.openUrl(QUrl.fromLocalFile(path))

    def latest_output_dir(self) -> str | None:
        if self.service_status.get("last_run_dir"):
            return self.service_status["last_run_dir"]
        if self.recent_runs:
            return self.recent_runs[0].get("output_dir")
        return None

    def open_selected_recent_run(self) -> None:
        row = self.recent_runs_table.currentRow()
        if row < 0:
            self.show_error("Select a run first")
            return
        path_item = self.recent_runs_table.item(row, 3)
        if not path_item:
            return
        QDesktopServices.openUrl(QUrl.fromLocalFile(path_item.text()))

    def format_triplet(self, values: Any) -> str:
        if not isinstance(values, list):
            return "-"
        return ", ".join(f"{float(value):.6g}" for value in values)

    def format_event_line(self, event: dict[str, Any]) -> str:
        ts = event.get("ts", "")
        phase = event.get("phase", "")
        name = event.get("event", "")
        point_id = event.get("point_id")
        suffix = f" [{point_id}]" if point_id else ""
        return f"{ts} {phase} {name}{suffix}"

    def show_error(self, message: str) -> None:
        QMessageBox.critical(self, "ODMR GUI", message)


def main() -> None:
    app = QApplication(sys.argv)
    window = MainWindow()
    window.show()
    sys.exit(app.exec())
