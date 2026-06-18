from __future__ import annotations

from pathlib import Path
from typing import Any

from PySide6.QtCore import Signal
from PySide6.QtWidgets import (
    QGroupBox,
    QGridLayout,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QPlainTextEdit,
    QPushButton,
    QVBoxLayout,
    QWidget,
    QFileDialog,
)

from odmr_console_core import RunBundle, default_bundle, load_json
from odmr_console_qt_shared import (
    REPO_ROOT,
    PathSelector,
    detect_station_lockin_model,
    note_label,
    section_label,
    timestamp_id,
)


class RunBundlePage(QWidget):
    bundle_changed = Signal()

    def __init__(self) -> None:
        super().__init__()
        self.default = default_bundle()
        root = QVBoxLayout(self)
        root.setSpacing(12)
        root.addWidget(section_label("本次实验配置"))
        root.addWidget(note_label("一次 run 只组合现有 C# runtime 可读的六个 JSON 和输出目录；这里不创建新的 bundle schema。"))

        selectors = QGroupBox("配置文件输入")
        selectors_layout = QVBoxLayout(selectors)
        self.station = self._selector("硬件站配置", "stations", self.default.station_path)
        self.calibration = self._selector("磁场校准", "calibrations", self.default.calibration_path)
        self.plan = self._selector("实验计划", "plans", self.default.plan_path)
        self.smb = self._selector("SMB100A 配置", "profiles", self.default.smb_profile_path)
        self.oe = self._selector("OE 配置（oe_profile）", "profiles", self.default.oe_profile_path)
        self.laser = self._selector("Laser 配置", "profiles", self.default.laser_profile_path)
        for selector in [self.station, self.calibration, self.plan, self.smb, self.oe, self.laser]:
            selector.changed.connect(self.bundle_changed.emit)
            selectors_layout.addWidget(selector)
        root.addWidget(selectors)

        output = QGroupBox("数据输出")
        output_layout = QGridLayout(output)
        output_layout.setColumnStretch(1, 1)
        output_layout.addWidget(QLabel("数据保存根目录"), 0, 0)
        self.output_root = QLineEdit(str(REPO_ROOT / "runs"))
        output_layout.addWidget(self.output_root, 0, 1)
        browse_root = QPushButton("浏览...")
        browse_root.clicked.connect(self._browse_output_root)
        output_layout.addWidget(browse_root, 0, 2)
        output_layout.addWidget(QLabel("本次运行目录"), 1, 0)
        self.out_dir = QLineEdit(str(REPO_ROOT / "runs" / f"pyside6_run_{timestamp_id()}"))
        output_layout.addWidget(self.out_dir, 1, 1)
        new_dir = QPushButton("新建")
        new_dir.clicked.connect(self.make_new_out_dir)
        output_layout.addWidget(new_dir, 1, 2)
        self.output_root.textChanged.connect(lambda _text: self.bundle_changed.emit())
        self.out_dir.textChanged.connect(lambda _text: self.bundle_changed.emit())
        root.addWidget(output)

        actions = QHBoxLayout()
        validate = QPushButton("检查配置")
        validate.clicked.connect(self.validate_local)
        actions.addWidget(validate)
        actions.addStretch(1)
        root.addLayout(actions)

        summary_group = QGroupBox("本次实验配置摘要")
        summary_layout = QVBoxLayout(summary_group)
        self.summary = QPlainTextEdit()
        self.summary.setReadOnly(True)
        self.summary.setMinimumHeight(210)
        self.summary.setPlainText("Validate the bundle to preview station, plan, profiles, and output.")
        summary_layout.addWidget(self.summary)
        root.addWidget(summary_group, 1)

    def _selector(self, label: str, subdir: str, default_path: str) -> PathSelector:
        presets = sorted((REPO_ROOT / "configs" / subdir).glob("*.json"))
        if Path(default_path) not in presets:
            presets.insert(0, Path(default_path))
        selector = PathSelector(label, presets)
        selector.set_path(default_path)
        return selector

    def bundle(self) -> RunBundle:
        return RunBundle(
            station_path=self.station.path(),
            calibration_path=self.calibration.path(),
            plan_path=self.plan.path(),
            smb_profile_path=self.smb.path(),
            oe_profile_path=self.oe.path(),
            laser_profile_path=self.laser.path(),
        )

    def set_bundle(self, bundle: RunBundle) -> None:
        self.station.set_path(bundle.station_path)
        self.calibration.set_path(bundle.calibration_path)
        self.plan.set_path(bundle.plan_path)
        self.smb.set_path(bundle.smb_profile_path)
        self.oe.set_path(bundle.oe_profile_path)
        self.laser.set_path(bundle.laser_profile_path)
        self.validate_local()
        self.bundle_changed.emit()

    def make_new_out_dir(self) -> None:
        run_id = "pyside6_run"
        try:
            run_id = load_json(self.plan.path()).get("run_id", run_id)
        except Exception:
            pass
        self.out_dir.setText(str(Path(self.output_root.text().strip()) / f"{run_id}_{timestamp_id()}"))

    def _browse_output_root(self) -> None:
        selected = QFileDialog.getExistingDirectory(self, "Select output root", self.output_root.text() or str(REPO_ROOT / "runs"))
        if selected:
            self.output_root.setText(selected)
            self.make_new_out_dir()

    def validate_local(self) -> bool:
        rows = []
        ok = True
        station_data: dict[str, Any] | None = None
        oe_data: dict[str, Any] | None = None
        for selector, title in [
            (self.station, "station"),
            (self.calibration, "calibration"),
            (self.plan, "plan"),
            (self.smb, "smb_profile"),
            (self.oe, "oe_profile"),
            (self.laser, "laser_profile"),
        ]:
            try:
                value = load_json(selector.path())
                selector.set_status("ok", True)
                rows.append(f"{title}: ok  {selector.path()}")
                if title == "station":
                    station_data = value
                    rows.append(f"  station_id={value.get('station_id', '<missing>')}")
                    rows.append(f"  station_lockin_model={detect_station_lockin_model(value) or '<invalid>'}")
                elif title == "calibration":
                    rows.append(f"  calibration_id={value.get('calibration_id', '<missing>')}")
                elif title == "plan":
                    points = value.get("points")
                    point_source = value.get("point_source")
                    rows.append(f"  run_id={value.get('run_id', '<missing>')}")
                    rows.append(f"  points={len(points) if isinstance(points, list) else 'point_source' if point_source else '<missing>'}")
                else:
                    rows.append(f"  profile_id={value.get('profile_id', '<missing>')}")
                    if title == "oe_profile":
                        oe_data = value
                        rows.append(f"  lockin_model={value.get('model', '<missing>')}")
            except Exception as exc:
                selector.set_status("invalid", False)
                rows.append(f"{title}: invalid  {exc}")
                ok = False
        if station_data is not None and oe_data is not None:
            station_model = detect_station_lockin_model(station_data)
            profile_model = str(oe_data.get("model", "")).strip().lower()
            if station_model and profile_model and station_model == profile_model:
                rows.append(f"station/profile match: ok  {station_model}")
            else:
                rows.append(f"station/profile match: invalid  station={station_model or '<missing>'} profile={profile_model or '<missing>'}")
                self.station.set_status("mismatch", False)
                self.oe.set_status("mismatch", False)
                ok = False
        out_dir = self.out_dir.text().strip()
        rows.append(f"out_dir: {out_dir}")
        self.summary.setPlainText("\n".join(rows))
        return ok
