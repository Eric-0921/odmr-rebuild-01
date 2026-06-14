from __future__ import annotations

from dataclasses import asdict
from datetime import datetime
import json
from pathlib import Path
import subprocess
import sys
import time
from typing import Any, Callable

from PySide6.QtCore import QThread, QTimer, Qt, Signal
from PySide6.QtGui import QFont
from PySide6.QtWidgets import (
    QApplication,
    QCheckBox,
    QComboBox,
    QFileDialog,
    QFormLayout,
    QFrame,
    QGridLayout,
    QGroupBox,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QListWidget,
    QListWidgetItem,
    QMainWindow,
    QMessageBox,
    QProgressBar,
    QPushButton,
    QPlainTextEdit,
    QScrollArea,
    QSizePolicy,
    QStackedWidget,
    QTableWidget,
    QTableWidgetItem,
    QTabWidget,
    QVBoxLayout,
    QWidget,
)

from odmr_console_core import (
    REPO_ROOT,
    RunBundle,
    command_to_text,
    control_paths_for_out_dir,
    default_bundle,
    discard_run_dir,
    generate_config_bundle,
    load_json,
    process_is_running,
    read_progress,
    read_progress_since,
    read_text_tail,
    request_emergency_stop,
    request_stop,
    resolve_bundle,
    start_run,
    winprobe_project,
)

CONFIG_GENERATOR_DIR = REPO_ROOT / "tools" / "config-generator"
if str(CONFIG_GENERATOR_DIR) not in sys.path:
    sys.path.insert(0, str(CONFIG_GENERATOR_DIR))

from odmr_config_core import (  # noqa: E402
    AxisSpec,
    GeneratorRequest,
    ScanBlock,
    expand_block,
    from_canonical_unit,
    parse_values,
    to_canonical_unit,
)


CURRENT_UNITS = ["A", "mA"]
VOLTAGE_UNITS = ["V", "mV"]
TIME_UNITS = ["ms", "s"]
FREQUENCY_UNITS = ["Hz", "kHz", "MHz", "GHz"]
LOW_FREQUENCY_UNITS = ["Hz", "kHz", "MHz"]
LF_VOLTAGE_UNITS = ["mV", "V"]
LASER_POWER_UNITS = ["mW", "W"]
LASER_MODE_LABEL_BY_TOKEN = {
    "off_background": "Laser 关",
    "on_background": "Laser 开",
}
LASER_MODE_TOKEN_BY_LABEL = {label: token for token, label in LASER_MODE_LABEL_BY_TOKEN.items()}

SMB_FM_SOURCE_CHOICES = ["INT", "EXT", "INT,EXT"]
SMB_FM_MODE_CHOICES = ["NORM", "LNO", "HDEV"]
SMB_LF_SHAPE_CHOICES = ["SINE", "SQU", "TRI", "SAWT", "ISAW"]
SMB_LF_IMPEDANCE_CHOICES = ["LOW", "G600"]
SMB_SWEEP_MODE_CHOICES = ["AUTO", "MAN", "STEP"]
SMB_SWEEP_SPACING_CHOICES = ["LIN", "LOG"]
SMB_SWEEP_SHAPE_CHOICES = ["SAWT", "TRI"]
SMB_TRIGGER_SOURCE_CHOICES = ["AUTO", "SING", "EXT"]


def code_choice(code: int, label: str) -> str:
    return f"{code} - {label}"


def choice_code(value: object) -> int:
    return int(str(value).split("-", 1)[0].strip())


TOKEN_BY_LABEL: dict[str, str] = {}


def token_choice(token: str, label: str) -> str:
    TOKEN_BY_LABEL[label] = token
    return label


def choice_token(value: object) -> str:
    text = str(value).strip()
    return TOKEN_BY_LABEL.get(text, text.split(" - ", 1)[0].strip())


def find_choice_by_code(key: str, value: object) -> str:
    code = choice_code(value)
    for choice in OE_CHOICES[key]:
        if choice_code(choice) == code:
            return choice
    return str(code)


def format_duration(seconds: float) -> str:
    whole = max(0, int(round(seconds)))
    minutes, sec = divmod(whole, 60)
    hours, minutes = divmod(minutes, 60)
    if hours:
        return f"{hours}h {minutes:02d}m {sec:02d}s"
    return f"{minutes}m {sec:02d}s"


def find_choice_by_token(token: str, choices: list[str]) -> str:
    for choice in choices:
        if choice_token(choice) == token:
            return choice
    return token


OE_CHOICES = {
    "channel": [code_choice(1, "通道 A"), code_choice(2, "通道 B")],
    "input_source": [
        code_choice(0, "A 单端电压输入"),
        code_choice(1, "A-B 差分电压输入"),
        code_choice(2, "1 MΩ 电流输入"),
        code_choice(3, "100 MΩ 电流输入"),
    ],
    "input_grounding": [code_choice(0, "浮空"), code_choice(1, "接地")],
    "input_coupling": [code_choice(0, "交流耦合"), code_choice(1, "直流耦合")],
    "line_notch_filter": [
        code_choice(0, "关闭陷波器"),
        code_choice(1, "50 Hz 陷波器"),
        code_choice(2, "50 Hz + 100 Hz 陷波器"),
        code_choice(3, "100 Hz 陷波器"),
    ],
    "reference_source": [
        code_choice(0, "外部参考"),
        code_choice(1, "内部参考"),
        code_choice(2, "内部扫频参考"),
    ],
    "reference_slope": [
        code_choice(0, "TTL 上升沿触发"),
        code_choice(1, "正弦过零检测"),
        code_choice(2, "LabVIEW 锁定态观测读回"),
    ],
    "dynamic_reserve": [
        code_choice(0, "低噪声"),
        code_choice(1, "正常"),
        code_choice(2, "高储备"),
    ],
    "sensitivity_index": [
        code_choice(0, "1 nV/fA"),
        code_choice(1, "2 nV/fA"),
        code_choice(2, "5 nV/fA"),
        code_choice(3, "10 nV/fA"),
        code_choice(4, "20 nV/fA"),
        code_choice(5, "50 nV/fA"),
        code_choice(6, "100 nV/fA"),
        code_choice(7, "200 nV/fA"),
        code_choice(8, "500 nV/fA"),
        code_choice(9, "1 uV/pA"),
        code_choice(10, "2 uV/pA"),
        code_choice(11, "5 uV/pA"),
        code_choice(12, "10 uV/pA"),
        code_choice(13, "20 uV/pA"),
        code_choice(14, "50 uV/pA"),
        code_choice(15, "100 uV/pA"),
        code_choice(16, "200 uV/pA"),
        code_choice(17, "500 uV/pA"),
        code_choice(18, "1 mV/nA"),
        code_choice(19, "2 mV/nA"),
        code_choice(20, "5 mV/nA"),
        code_choice(21, "10 mV/nA"),
        code_choice(22, "20 mV/nA"),
        code_choice(23, "50 mV/nA"),
        code_choice(24, "100 mV/nA"),
        code_choice(25, "200 mV/nA"),
        code_choice(26, "500 mV/nA"),
        code_choice(27, "1 V/uA"),
    ],
    "time_constant_index": [
        code_choice(0, "10 us"),
        code_choice(1, "30 us"),
        code_choice(2, "100 us"),
        code_choice(3, "300 us"),
        code_choice(4, "1 ms"),
        code_choice(5, "3 ms"),
        code_choice(6, "10 ms"),
        code_choice(7, "30 ms"),
        code_choice(8, "100 ms"),
        code_choice(9, "300 ms"),
        code_choice(10, "1 s"),
        code_choice(11, "3 s"),
        code_choice(12, "10 s"),
        code_choice(13, "30 s"),
        code_choice(14, "100 s"),
        code_choice(15, "300 s"),
        code_choice(16, "1000 s"),
        code_choice(17, "3000 s"),
    ],
    "filter_slope": [
        code_choice(0, "6 dB/oct"),
        code_choice(1, "12 dB/oct"),
        code_choice(2, "18 dB/oct"),
        code_choice(3, "24 dB/oct"),
    ],
    "sync_filter": [code_choice(0, "关闭"), code_choice(1, "开启")],
    "sine_output_mode": [
        code_choice(0, "固定幅值输出"),
        code_choice(1, "线性扫幅输出"),
        code_choice(2, "对数扫幅输出"),
        code_choice(3, "直流输出"),
    ],
}
OE_CHOICE_KEYS = set(OE_CHOICES)

MAG_AXIS_MODE_CHOICES = [
    token_choice("range", "起点 / 终点 / 步进"),
    token_choice("list", "显式列表"),
]
MAG_TRAVERSAL_CHOICES = [
    token_choice("raster", "单向顺序扫描"),
    token_choice("bounce_1d_x", "X 轴往返扫描"),
]
PLAN_KIND_CHOICES = [
    token_choice("no_magnetic_control", "无磁场控制"),
    token_choice("constant_field", "零场 / 恒定磁场"),
    token_choice("magnetic_scan", "磁场扫描"),
]


def format_number(value: float) -> str:
    text = f"{float(value):.12g}"
    return "0" if text == "-0" else text


def timestamp_id() -> str:
    return datetime.now().strftime("%Y%m%d_%H%M%S")


def scroll_page(widget: QWidget) -> QScrollArea:
    area = QScrollArea()
    area.setWidgetResizable(True)
    area.setFrameShape(QFrame.Shape.NoFrame)
    area.setWidget(widget)
    return area


def section_label(text: str) -> QLabel:
    label = QLabel(text)
    font = label.font()
    font.setPointSize(font.pointSize() + 1)
    font.setBold(True)
    label.setFont(font)
    return label


def note_label(text: str) -> QLabel:
    label = QLabel(text)
    label.setWordWrap(True)
    label.setStyleSheet("color: #555;")
    return label


class WorkerThread(QThread):
    completed = Signal(object)
    failed = Signal(str)

    def __init__(self, fn: Callable[[], Any]) -> None:
        super().__init__()
        self._fn = fn

    def run(self) -> None:
        try:
            self.completed.emit(self._fn())
        except Exception as exc:  # pragma: no cover - UI thread reporting
            self.failed.emit(str(exc))


class NumberUnitInput(QWidget):
    def __init__(self, value: object, units: list[str] | None = None, unit: str | None = None, width: int = 120) -> None:
        super().__init__()
        layout = QHBoxLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setSpacing(6)
        self.value_edit = QLineEdit(str(value))
        self.value_edit.setMinimumWidth(width)
        layout.addWidget(self.value_edit, 1)
        self.unit_combo: QComboBox | None = None
        if units:
            self.unit_combo = QComboBox()
            self.unit_combo.addItems(units)
            self.unit_combo.setCurrentText(unit or units[0])
            self.unit_combo.setMinimumWidth(78)
            layout.addWidget(self.unit_combo, 0)
        elif unit:
            label = QLabel(unit)
            label.setMinimumWidth(42)
            layout.addWidget(label, 0)

    def text(self) -> str:
        return self.value_edit.text().strip()

    def set_text(self, value: object) -> None:
        self.value_edit.setText(format_number(float(value)) if isinstance(value, float) else str(value))

    def value(self) -> float:
        return float(self.text())

    def unit(self) -> str:
        return self.unit_combo.currentText() if self.unit_combo else ""

    def set_unit(self, unit: str) -> None:
        if self.unit_combo:
            self.unit_combo.setCurrentText(unit)

    def canonical(self, unit_kind: str) -> float:
        return to_canonical_unit(self.value(), unit_kind, self.unit())

    def set_canonical(self, value: object, unit_kind: str) -> None:
        self.set_text(from_canonical_unit(value, unit_kind, self.unit()))


class PathSelector(QWidget):
    changed = Signal()

    def __init__(self, title: str, presets: list[Path], is_dir: bool = False) -> None:
        super().__init__()
        self.title = title
        self.is_dir = is_dir
        layout = QGridLayout(self)
        layout.setContentsMargins(0, 0, 0, 0)
        layout.setColumnStretch(1, 1)
        label = QLabel(title)
        label.setMinimumWidth(145)
        layout.addWidget(label, 0, 0)
        self.combo = QComboBox()
        self.combo.setEditable(True)
        for path in presets:
            self.combo.addItem(str(path))
        self.combo.currentTextChanged.connect(lambda _text: self.changed.emit())
        layout.addWidget(self.combo, 0, 1)
        browse = QPushButton("浏览...")
        browse.clicked.connect(self._browse)
        layout.addWidget(browse, 0, 2)
        open_dir = QPushButton("打开目录")
        open_dir.clicked.connect(self._open_dir)
        layout.addWidget(open_dir, 0, 3)
        self.status = QLabel("未检查")
        self.status.setMinimumWidth(112)
        layout.addWidget(self.status, 0, 4)

    def path(self) -> str:
        return self.combo.currentText().strip()

    def set_path(self, path: str | Path) -> None:
        text = str(path)
        if self.combo.findText(text) < 0:
            self.combo.addItem(text)
        self.combo.setCurrentText(text)

    def set_status(self, text: str, ok: bool | None = None) -> None:
        color = "#166534" if ok else "#991b1b" if ok is False else "#555"
        self.status.setText(text)
        self.status.setStyleSheet(f"color: {color};")

    def _browse(self) -> None:
        current = self.path() or str(REPO_ROOT)
        if self.is_dir:
            selected = QFileDialog.getExistingDirectory(self, f"Select {self.title}", current)
        else:
            selected, _ = QFileDialog.getOpenFileName(self, f"Select {self.title}", current, "JSON files (*.json);;All files (*)")
        if selected:
            self.set_path(selected)

    def _open_dir(self) -> None:
        path = Path(self.path())
        target = path if self.is_dir else path.parent
        if not target.exists():
            QMessageBox.warning(self, "Open directory", f"Directory does not exist:\n{target}")
            return
        if sys.platform == "darwin":
            subprocess.Popen(["open", str(target)])
        elif sys.platform.startswith("win"):
            subprocess.Popen(["explorer", str(target)])
        else:
            subprocess.Popen(["xdg-open", str(target)])


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
        self.oe = self._selector("OE1022D 配置", "profiles", self.default.oe_profile_path)
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
                    rows.append(f"  station_id={value.get('station_id', '<missing>')}")
                elif title == "calibration":
                    rows.append(f"  calibration_id={value.get('calibration_id', '<missing>')}")
                elif title == "plan":
                    points = value.get("points")
                    point_source = value.get("point_source")
                    rows.append(f"  run_id={value.get('run_id', '<missing>')}")
                    rows.append(f"  points={len(points) if isinstance(points, list) else 'point_source' if point_source else '<missing>'}")
                else:
                    rows.append(f"  profile_id={value.get('profile_id', '<missing>')}")
            except Exception as exc:
                selector.set_status("invalid", False)
                rows.append(f"{title}: invalid  {exc}")
                ok = False
        out_dir = self.out_dir.text().strip()
        rows.append(f"out_dir: {out_dir}")
        self.summary.setPlainText("\n".join(rows))
        return ok


class ConfigGeneratorPage(QWidget):
    bundle_generated = Signal(object)

    def __init__(self) -> None:
        super().__init__()
        self.blocks: list[ScanBlock] = [self.default_block("x_line", "x")]
        self._building_block = False
        root = QVBoxLayout(self)
        root.addWidget(section_label("配置生成"))
        root.addWidget(note_label("生成器输出现有 C# runtime 的 plan/profile JSON。生成后自动绑定到 Run Bundle。"))
        self.tabs = QTabWidget()
        root.addWidget(self.tabs, 1)

        self.template_page = QWidget()
        self.mag_page = QWidget()
        self.policy_page = QWidget()
        self.smb_page = QWidget()
        self.oe_page = QWidget()
        self.laser_page = QWidget()
        self.generate_page = QWidget()
        for title, page in [
            ("模板 / 输出", self.template_page),
            ("实验计划", self.mag_page),
            ("计划策略", self.policy_page),
            ("SMB100A", self.smb_page),
            ("OE1022D", self.oe_page),
            ("CNI Laser", self.laser_page),
            ("生成", self.generate_page),
        ]:
            self.tabs.addTab(scroll_page(page), title)

        self._build_templates()
        self._build_magnetic()
        self._build_policy()
        self._build_smb()
        self._build_oe()
        self._build_laser()
        self._build_generate()
        self._load_default_paths()
        self._load_templates()
        self._refresh_block_list()

    def _build_templates(self) -> None:
        layout = QVBoxLayout(self.template_page)
        layout.addWidget(section_label("模板和生成目录"))
        self.plan_template = PathSelector("Plan 模板", sorted((REPO_ROOT / "configs" / "plans").glob("*.json")))
        self.smb_template = PathSelector("SMB 模板", sorted((REPO_ROOT / "configs" / "profiles").glob("smb*.json")))
        self.oe_template = PathSelector("OE 模板", sorted((REPO_ROOT / "configs" / "profiles").glob("oe*.json")))
        self.laser_template = PathSelector("Laser 模板", sorted((REPO_ROOT / "configs" / "profiles").glob("cni*.json")))
        self.output_dir = PathSelector("生成文件目录", [REPO_ROOT / "configs" / "generated"], is_dir=True)
        for widget in [self.plan_template, self.smb_template, self.oe_template, self.laser_template, self.output_dir]:
            layout.addWidget(widget)
        load = QPushButton("读取模板默认值")
        load.clicked.connect(self._load_templates)
        layout.addWidget(load, 0, Qt.AlignmentFlag.AlignLeft)
        layout.addWidget(note_label("模板只提供默认值；最终写出的仍然是独立 plan/profile JSON。"))
        layout.addStretch(1)

    def _build_magnetic(self) -> None:
        layout = QVBoxLayout(self.mag_page)
        identity = QGroupBox("实验身份")
        form = QGridLayout(identity)
        self.run_id = QLineEdit("generated_plan")
        self.operator = QLineEdit("local")
        self.acquisition_window = NumberUnitInput(0, TIME_UNITS, "ms")
        self.point_settle = NumberUnitInput(500, TIME_UNITS, "ms")
        add_form_row(form, 0, 0, "运行 ID", self.run_id)
        add_form_row(form, 1, 0, "操作人", self.operator)
        add_form_row(form, 0, 1, "采集窗口", self.acquisition_window)
        add_form_row(form, 1, 1, "点位稳定等待", self.point_settle)
        layout.addWidget(identity)

        mode_group = QGroupBox("实验步骤类型")
        mode_grid = QGridLayout(mode_group)
        self.plan_kind = QComboBox()
        self.plan_kind.addItems(PLAN_KIND_CHOICES)
        self.plan_kind.setCurrentText(find_choice_by_token("no_magnetic_control", PLAN_KIND_CHOICES))
        self.acquisition_step_count = QLineEdit("1")
        self.fixed_x = NumberUnitInput(0, unit="nT")
        self.fixed_y = NumberUnitInput(0, unit="nT")
        self.fixed_z = NumberUnitInput(0, unit="nT")
        add_form_row(mode_grid, 0, 0, "计划类型", self.plan_kind)
        add_form_row(mode_grid, 0, 1, "无磁场采集步数", self.acquisition_step_count)
        add_form_row(mode_grid, 1, 0, "恒定 X 磁场", self.fixed_x)
        add_form_row(mode_grid, 1, 1, "恒定 Y 磁场", self.fixed_y)
        add_form_row(mode_grid, 2, 0, "恒定 Z 磁场", self.fixed_z)
        layout.addWidget(mode_group)
        layout.addWidget(note_label(
            "point 表示一次采集 step。无磁场控制不会伪装成 0,0,0，也不会指挥 M8812；零场/恒定磁场和磁场扫描才会生成 target_b_nt 并走 M8812 baseline/readback。"
        ))

        blocks_group = QGroupBox("磁场扫描块")
        blocks_layout = QGridLayout(blocks_group)
        blocks_layout.setColumnStretch(1, 1)
        self.block_list = QListWidget()
        self.block_list.setMinimumWidth(260)
        self.block_list.currentRowChanged.connect(self._load_block)
        blocks_layout.addWidget(self.block_list, 0, 0, 6, 1)
        self.block_prefix = QLineEdit()
        self.block_traversal = QComboBox()
        self.block_traversal.addItems(MAG_TRAVERSAL_CHOICES)
        self.block_total_points = QLineEdit("0")
        add_form_row(blocks_layout, 0, 1, "扫描块前缀", self.block_prefix)
        add_form_row(blocks_layout, 1, 1, "扫描方向", self.block_traversal)
        add_form_row(blocks_layout, 2, 1, "总点数（0=按网格一次）", self.block_total_points)
        action_row = QHBoxLayout()
        update = QPushButton("添加 / 更新扫描块")
        update.clicked.connect(self._add_or_update_block)
        delete = QPushButton("删除选中")
        delete.clicked.connect(self._remove_block)
        xyz = QPushButton("添加 X/Y/Z 三个单轴扫描")
        xyz.clicked.connect(self._add_xyz_blocks)
        action_row.addWidget(update)
        action_row.addWidget(delete)
        action_row.addWidget(xyz)
        action_row.addStretch(1)
        blocks_layout.addLayout(action_row, 3, 1, 1, 2)
        layout.addWidget(blocks_group)

        axes = QGroupBox("坐标轴定义")
        axes_layout = QHBoxLayout(axes)
        self.axis_widgets: dict[str, dict[str, Any]] = {}
        for axis in ["x", "y", "z"]:
            axes_layout.addWidget(self._axis_group(axis))
        layout.addWidget(axes)
        layout.addStretch(1)

    def _axis_group(self, axis: str) -> QGroupBox:
        group = QGroupBox(f"{axis.upper()} 轴")
        form = QFormLayout(group)
        enabled = QCheckBox("启用")
        mode = QComboBox()
        mode.addItems(MAG_AXIS_MODE_CHOICES)
        fixed = NumberUnitInput(0, unit="nT")
        start = NumberUnitInput(0, unit="nT")
        stop = NumberUnitInput(40 if axis == "x" else 0, unit="nT")
        step = NumberUnitInput(10, unit="nT")
        values_text = QLineEdit("0, 10, 20, 30, 40")
        values_row = QWidget()
        values_layout = QHBoxLayout(values_row)
        values_layout.setContentsMargins(0, 0, 0, 0)
        values_layout.setSpacing(6)
        values_layout.addWidget(values_text, 1)
        values_layout.addWidget(QLabel("nT"), 0)
        form.addRow("启用扫描", enabled)
        form.addRow("输入方式", mode)
        form.addRow("固定磁场", fixed)
        form.addRow("起始磁场", start)
        form.addRow("结束磁场", stop)
        form.addRow("步进磁场", step)
        form.addRow("显式点列表", values_row)
        self.axis_widgets[axis] = {
            "enabled": enabled,
            "mode": mode,
            "fixed": fixed,
            "start": start,
            "stop": stop,
            "step": step,
            "values_text": values_text,
        }
        return group

    def _build_policy(self) -> None:
        layout = QVBoxLayout(self.policy_page)
        baseline = QGroupBox("Maynuo 零场偏置 / 输出策略")
        grid = QGridLayout(baseline)
        self.baseline_x = NumberUnitInput(0, CURRENT_UNITS, "A")
        self.baseline_y = NumberUnitInput(0, CURRENT_UNITS, "A")
        self.baseline_z = NumberUnitInput(0, CURRENT_UNITS, "A")
        self.baseline_settle = NumberUnitInput(1000, TIME_UNITS, "ms")
        self.readback_samples = QLineEdit("3")
        self.settle_tolerance = NumberUnitInput(0.002, CURRENT_UNITS, "A")
        self.voltage = NumberUnitInput(75, VOLTAGE_UNITS, "V")
        self.voltage_protection = NumberUnitInput(75, VOLTAGE_UNITS, "V")
        self.output_enabled = QCheckBox("enabled")
        self.output_enabled.setChecked(True)
        rows = [
            ("X 轴零场电流", self.baseline_x),
            ("Y 轴零场电流", self.baseline_y),
            ("Z 轴零场电流", self.baseline_z),
            ("零场稳定等待", self.baseline_settle),
            ("回读采样次数", self.readback_samples),
            ("电流容差", self.settle_tolerance),
            ("输出电压", self.voltage),
            ("电压保护", self.voltage_protection),
            ("输出使能", self.output_enabled),
        ]
        for index, (label, widget) in enumerate(rows):
            add_form_row(grid, index // 2, index % 2, label, widget)
        layout.addWidget(baseline)

        quality = QGroupBox("采集质量标记阈值")
        qgrid = QGridLayout(quality)
        self.min_frames = QLineEdit("20")
        self.max_timeout = QLineEdit("2")
        self.max_duplicate = QLineEdit("0.3")
        self.max_last_age = NumberUnitInput(500, TIME_UNITS, "ms")
        for index, (label, widget) in enumerate([
            ("最少帧数", self.min_frames),
            ("最大 timeout 数", self.max_timeout),
            ("最大重复比例", self.max_duplicate),
            ("最后一帧最大延迟", self.max_last_age),
        ]):
            add_form_row(qgrid, index // 2, index % 2, label, widget)
        layout.addWidget(quality)
        layout.addStretch(1)

    def _build_smb(self) -> None:
        layout = QVBoxLayout(self.smb_page)
        identity = QGroupBox("SMB100A profile identity")
        grid = QGridLayout(identity)
        self.smb_profile_id = QLineEdit()
        self.smb_command_settle = NumberUnitInput(500, TIME_UNITS, "ms")
        self.smb_error_check = QCheckBox("Check SYST:ERR? after batch")
        self.smb_error_check.setChecked(True)
        add_form_row(grid, 0, 0, "Profile ID", self.smb_profile_id)
        add_form_row(grid, 0, 1, "Command settle", self.smb_command_settle)
        add_form_row(grid, 1, 0, "Batch error check", self.smb_error_check)
        layout.addWidget(identity)

        fixed = QGroupBox("SMB fixed modulation profile")
        fgrid = QGridLayout(fixed)
        self.smb_mod_enabled = QCheckBox("enabled")
        self.smb_mod_enabled.setChecked(True)
        self.smb_fm_enabled = QCheckBox("enabled")
        self.smb_fm_enabled.setChecked(True)
        self.smb_fm_source = combo(SMB_FM_SOURCE_CHOICES, "INT")
        self.smb_fm_mode = combo(SMB_FM_MODE_CHOICES, "HDEV")
        self.smb_fm_deviation = NumberUnitInput(4, FREQUENCY_UNITS, "MHz")
        self.smb_lf_enabled = QCheckBox("enabled")
        self.smb_lf_enabled.setChecked(True)
        self.smb_lf_voltage = NumberUnitInput(137, LF_VOLTAGE_UNITS, "mV")
        self.smb_lf_frequency = NumberUnitInput(500, LOW_FREQUENCY_UNITS, "Hz")
        self.smb_lf_shape = combo(SMB_LF_SHAPE_CHOICES, "SQU")
        self.smb_lf_impedance = combo(SMB_LF_IMPEDANCE_CHOICES, "LOW")
        for index, (label, widget) in enumerate([
            ("Modulation enabled", self.smb_mod_enabled),
            ("FM enabled", self.smb_fm_enabled),
            ("FM source", self.smb_fm_source),
            ("FM mode", self.smb_fm_mode),
            ("FM deviation", self.smb_fm_deviation),
            ("LF output enabled", self.smb_lf_enabled),
            ("LF voltage", self.smb_lf_voltage),
            ("LF frequency", self.smb_lf_frequency),
            ("LF shape", self.smb_lf_shape),
            ("LF source impedance", self.smb_lf_impedance),
        ]):
            add_form_row(fgrid, index // 2, index % 2, label, widget)
        layout.addWidget(fixed)

        sweep = QGroupBox("SMB default RF sweep")
        sgrid = QGridLayout(sweep)
        self.smb_start = NumberUnitInput(2.83, FREQUENCY_UNITS, "GHz")
        self.smb_stop = NumberUnitInput(2.89, FREQUENCY_UNITS, "GHz")
        self.smb_step = NumberUnitInput(0.5, FREQUENCY_UNITS, "MHz")
        self.smb_dwell = NumberUnitInput(300, TIME_UNITS, "ms")
        self.smb_power = NumberUnitInput(-10, None, "dBm")
        self.smb_sweep_mode = combo(SMB_SWEEP_MODE_CHOICES, "AUTO")
        self.smb_spacing = combo(SMB_SWEEP_SPACING_CHOICES, "LIN")
        self.smb_shape = combo(SMB_SWEEP_SHAPE_CHOICES, "SAWT")
        self.smb_trigger = combo(SMB_TRIGGER_SOURCE_CHOICES, "AUTO")
        self.smb_voltage_start = NumberUnitInput(0, None, "V")
        self.smb_voltage_stop = NumberUnitInput(3, None, "V")
        self.smb_rf_output = QCheckBox("enabled")
        self.smb_rf_output.setChecked(True)
        for index, (label, widget) in enumerate([
            ("Start", self.smb_start),
            ("Stop", self.smb_stop),
            ("Step", self.smb_step),
            ("Dwell", self.smb_dwell),
            ("Power", self.smb_power),
            ("Sweep mode", self.smb_sweep_mode),
            ("Spacing", self.smb_spacing),
            ("Shape", self.smb_shape),
            ("Trigger source", self.smb_trigger),
            ("Output voltage start", self.smb_voltage_start),
            ("Output voltage stop", self.smb_voltage_stop),
            ("RF output enabled", self.smb_rf_output),
        ]):
            add_form_row(sgrid, index // 2, index % 2, label, widget)
        layout.addWidget(sweep)
        layout.addStretch(1)

    def _build_oe(self) -> None:
        layout = QVBoxLayout(self.oe_page)
        layout.addWidget(note_label("OE1022D 固定配置只写入 profile snapshot。RALL collector 锁定为 12288B + 30ms，不由此页面修改。"))
        group = QGroupBox("OE1022D 固定配置")
        grid = QGridLayout(group)
        self.oe_profile_id = QLineEdit()
        self.oe_command_settle = NumberUnitInput(500, TIME_UNITS, "ms")
        self.oe_fields: dict[str, QWidget] = {
            "channel": combo(OE_CHOICES["channel"], find_choice_by_code("channel", 2)),
            "input_source": combo(OE_CHOICES["input_source"], find_choice_by_code("input_source", 0)),
            "input_grounding": combo(OE_CHOICES["input_grounding"], find_choice_by_code("input_grounding", 0)),
            "input_coupling": combo(OE_CHOICES["input_coupling"], find_choice_by_code("input_coupling", 0)),
            "line_notch_filter": combo(OE_CHOICES["line_notch_filter"], find_choice_by_code("line_notch_filter", 0)),
            "reference_source": combo(OE_CHOICES["reference_source"], find_choice_by_code("reference_source", 0)),
            "reference_slope": combo(OE_CHOICES["reference_slope"], find_choice_by_code("reference_slope", 2)),
            "phase_deg": NumberUnitInput(0, None, "deg"),
            "harmonic_1": QLineEdit("1"),
            "harmonic_2": QLineEdit("1"),
            "dynamic_reserve": combo(OE_CHOICES["dynamic_reserve"], find_choice_by_code("dynamic_reserve", 1)),
            "sensitivity_index": combo(OE_CHOICES["sensitivity_index"], find_choice_by_code("sensitivity_index", 24)),
            "time_constant_index": combo(OE_CHOICES["time_constant_index"], find_choice_by_code("time_constant_index", 7)),
            "filter_slope": combo(OE_CHOICES["filter_slope"], find_choice_by_code("filter_slope", 1)),
            "sync_filter": combo(OE_CHOICES["sync_filter"], find_choice_by_code("sync_filter", 0)),
            "sine_output_mode": combo(OE_CHOICES["sine_output_mode"], find_choice_by_code("sine_output_mode", 0)),
            "sine_output_voltage_vrms": NumberUnitInput(1, None, "Vrms"),
        }
        rows: list[tuple[str, QWidget]] = [
            ("配置 ID", self.oe_profile_id),
            ("命令等待时间", self.oe_command_settle),
            ("通道", self.oe_fields["channel"]),
            ("输入信号源", self.oe_fields["input_source"]),
            ("输入屏蔽接地", self.oe_fields["input_grounding"]),
            ("输入耦合", self.oe_fields["input_coupling"]),
            ("输入陷波器", self.oe_fields["line_notch_filter"]),
            ("参考信号源", self.oe_fields["reference_source"]),
            ("外部参考触发方式", self.oe_fields["reference_slope"]),
            ("参考相位", self.oe_fields["phase_deg"]),
            ("谐波 1 检测", self.oe_fields["harmonic_1"]),
            ("谐波 2 检测", self.oe_fields["harmonic_2"]),
            ("动态储备", self.oe_fields["dynamic_reserve"]),
            ("满偏灵敏度", self.oe_fields["sensitivity_index"]),
            ("滤波器时间常数", self.oe_fields["time_constant_index"]),
            ("滤波器陡降", self.oe_fields["filter_slope"]),
            ("同步滤波器", self.oe_fields["sync_filter"]),
            ("正弦信号输出模式", self.oe_fields["sine_output_mode"]),
            ("正弦输出幅值", self.oe_fields["sine_output_voltage_vrms"]),
        ]
        for index, (label, widget) in enumerate(rows):
            add_form_row(grid, index // 2, index % 2, label, widget)
        layout.addWidget(group)
        layout.addStretch(1)

    def _build_laser(self) -> None:
        layout = QVBoxLayout(self.laser_page)
        group = QGroupBox("CNI Laser 运行级开关")
        grid = QGridLayout(group)
        self.laser_profile_id = QLineEdit()
        self.laser_mode = combo(list(LASER_MODE_LABEL_BY_TOKEN.values()), LASER_MODE_LABEL_BY_TOKEN["off_background"])
        self.laser_power = NumberUnitInput(0, LASER_POWER_UNITS, "mW")
        self.laser_settle = NumberUnitInput(1000, TIME_UNITS, "ms")
        for index, (label, widget) in enumerate([
            ("配置 ID", self.laser_profile_id),
            ("Laser 状态", self.laser_mode),
            ("功率", self.laser_power),
            ("切换等待", self.laser_settle),
        ]):
            add_form_row(grid, index // 2, index % 2, label, widget)
        layout.addWidget(group)
        layout.addWidget(note_label("Laser 是 run 级开关：运行开始时按这里设置开/关，运行结束或急停时关闭；不会在每个 point 反复开关。"))
        layout.addStretch(1)

    def _build_generate(self) -> None:
        layout = QVBoxLayout(self.generate_page)
        button = QPushButton("Generate plan + profiles and bind Run Bundle")
        button.clicked.connect(self._generate)
        layout.addWidget(button, 0, Qt.AlignmentFlag.AlignLeft)
        self.generate_output = QPlainTextEdit()
        self.generate_output.setReadOnly(True)
        self.generate_output.setMinimumHeight(420)
        layout.addWidget(self.generate_output, 1)

    def _load_default_paths(self) -> None:
        self.plan_template.set_path(REPO_ROOT / "configs" / "plans" / "x_axis_1d_bounce_15min.json")
        self.smb_template.set_path(REPO_ROOT / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json")
        self.oe_template.set_path(REPO_ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json")
        self.laser_template.set_path(REPO_ROOT / "configs" / "profiles" / "cni_laser_run_off_background.json")
        self.output_dir.set_path(REPO_ROOT / "configs" / "generated")

    def _load_templates(self) -> None:
        try:
            plan = load_json(self.plan_template.path())
            smb = load_json(self.smb_template.path())
            oe = load_json(self.oe_template.path())
            laser = load_json(self.laser_template.path())
            self._set_plan_values(plan)
            self._set_smb_values(smb)
            self._set_oe_values(oe)
            self._set_laser_values(laser)
            for selector in [self.plan_template, self.smb_template, self.oe_template, self.laser_template, self.output_dir]:
                selector.set_status("ok", True)
        except Exception as exc:
            QMessageBox.critical(self, "Load templates failed", str(exc))

    def _set_plan_values(self, plan: dict[str, Any]) -> None:
        self.run_id.setText(f"{plan.get('run_id', 'generated_plan')}_generated")
        self.operator.setText(plan.get("operator", "local"))
        self.acquisition_window.set_canonical(plan.get("acquisition_window_ms", 0), "time_ms")
        self.point_settle.set_canonical(plan.get("point_settle_ms", 500), "time_ms")
        points = plan.get("points") if isinstance(plan.get("points"), list) else []
        if points and all(point.get("magnetic_mode") == "none" for point in points if isinstance(point, dict)):
            self.plan_kind.setCurrentText(find_choice_by_token("no_magnetic_control", PLAN_KIND_CHOICES))
            self.acquisition_step_count.setText(str(len(points)))
        elif len(points) == 1 and isinstance(points[0], dict) and points[0].get("target_b_nt") is not None:
            self.plan_kind.setCurrentText(find_choice_by_token("constant_field", PLAN_KIND_CHOICES))
            target = points[0].get("target_b_nt", [0, 0, 0])
            self.fixed_x.set_canonical(target[0] if len(target) > 0 else 0, "field")
            self.fixed_y.set_canonical(target[1] if len(target) > 1 else 0, "field")
            self.fixed_z.set_canonical(target[2] if len(target) > 2 else 0, "field")
        else:
            self.plan_kind.setCurrentText(find_choice_by_token("magnetic_scan", PLAN_KIND_CHOICES))
        baseline = plan.get("mag_baseline_policy", {})
        currents = baseline.get("baseline_current_a", [0, 0, 0])
        self.baseline_x.set_canonical(currents[0] if len(currents) > 0 else 0, "current_a")
        self.baseline_y.set_canonical(currents[1] if len(currents) > 1 else 0, "current_a")
        self.baseline_z.set_canonical(currents[2] if len(currents) > 2 else 0, "current_a")
        self.baseline_settle.set_canonical(baseline.get("settle_ms", 1000), "time_ms")
        self.readback_samples.setText(str(baseline.get("readback_samples", 3)))
        self.settle_tolerance.set_canonical(baseline.get("settle_tolerance_a", 0.002), "current_a")
        self.voltage.set_canonical(baseline.get("voltage_v", 75), "voltage_v")
        self.voltage_protection.set_canonical(baseline.get("voltage_protection_v", 75), "voltage_v")
        self.output_enabled.setChecked(bool(baseline.get("output_enabled", True)))
        quality = plan.get("quality_thresholds", {})
        self.min_frames.setText(str(quality.get("min_frames", 20)))
        self.max_timeout.setText(str(quality.get("max_timeout_count", 2)))
        self.max_duplicate.setText(str(quality.get("max_duplicate_ratio", 0.3)))
        self.max_last_age.set_canonical(quality.get("max_last_frame_age_ms", 500), "time_ms")

    def _set_smb_values(self, smb: dict[str, Any]) -> None:
        fixed = smb.get("fixed", {})
        sweep = smb.get("default_sweep", {})
        self.smb_profile_id.setText(f"{smb.get('profile_id', 'smb100a')}_generated")
        self.smb_command_settle.set_canonical(smb.get("command_settle_ms", 500), "time_ms")
        self.smb_error_check.setChecked(bool(smb.get("error_check_after_write", True)))
        self.smb_mod_enabled.setChecked(bool(fixed.get("modulation_enabled", True)))
        self.smb_fm_enabled.setChecked(bool(fixed.get("fm_enabled", True)))
        self.smb_fm_source.setCurrentText(fixed.get("fm_source", "INT"))
        self.smb_fm_mode.setCurrentText(fixed.get("fm_mode", "HDEV"))
        self.smb_fm_deviation.set_canonical(fixed.get("fm_deviation_hz", 4000000), "frequency_hz")
        self.smb_lf_enabled.setChecked(bool(fixed.get("lf_output_enabled", True)))
        self.smb_lf_voltage.set_canonical(fixed.get("lf_voltage_mv", 137), "voltage_mv")
        self.smb_lf_frequency.set_canonical(fixed.get("lf_frequency_hz", 500), "frequency_hz")
        self.smb_lf_shape.setCurrentText(fixed.get("lf_shape", "SQU"))
        self.smb_lf_impedance.setCurrentText(fixed.get("lf_source_impedance", "LOW"))
        self.smb_start.set_canonical(sweep.get("start_hz", 2830000000), "frequency_hz")
        self.smb_stop.set_canonical(sweep.get("stop_hz", 2890000000), "frequency_hz")
        self.smb_step.set_canonical(sweep.get("step_hz", 500000), "frequency_hz")
        self.smb_dwell.set_canonical(sweep.get("dwell_ms", 300), "time_ms")
        self.smb_power.set_text(sweep.get("power_dbm", -10))
        self.smb_sweep_mode.setCurrentText(sweep.get("sweep_mode", "AUTO"))
        self.smb_spacing.setCurrentText(sweep.get("spacing", "LIN"))
        self.smb_shape.setCurrentText(sweep.get("shape", "SAWT"))
        self.smb_trigger.setCurrentText(sweep.get("trigger_source", "AUTO"))
        self.smb_voltage_start.set_text(sweep.get("output_voltage_start_v", 0))
        self.smb_voltage_stop.set_text(sweep.get("output_voltage_stop_v", 3))
        self.smb_rf_output.setChecked(bool(sweep.get("rf_output_enabled", True)))

    def _set_oe_values(self, oe: dict[str, Any]) -> None:
        fixed = oe.get("fixed", {})
        self.oe_profile_id.setText(f"{oe.get('profile_id', 'oe1022d')}_generated")
        self.oe_command_settle.set_canonical(oe.get("command_settle_ms", 500), "time_ms")
        for key, widget in self.oe_fields.items():
            value = fixed.get(key)
            if value is None:
                continue
            if key in OE_CHOICE_KEYS and isinstance(widget, QComboBox):
                widget.setCurrentText(find_choice_by_code(key, value))
            elif isinstance(widget, NumberUnitInput):
                widget.set_text(value)
            elif isinstance(widget, QLineEdit):
                widget.setText(str(value))

    def _set_laser_values(self, laser: dict[str, Any]) -> None:
        self.laser_profile_id.setText(f"{laser.get('profile_id', 'cni_laser')}_generated")
        mode = laser.get("mode", "off_background")
        self.laser_mode.setCurrentText(LASER_MODE_LABEL_BY_TOKEN.get(mode, LASER_MODE_LABEL_BY_TOKEN["off_background"]))
        self.laser_power.set_canonical(laser.get("power_mw", 0), "power_mw")
        self.laser_settle.set_canonical(laser.get("settle_ms", 1000), "time_ms")

    def _refresh_block_list(self) -> None:
        self.block_list.blockSignals(True)
        self.block_list.clear()
        for block in self.blocks:
            active = "".join(axis.upper() for axis, spec in block.axes.items() if spec.enabled) or "fixed"
            try:
                count = len(expand_block(block))
            except Exception:
                count = -1
            item = QListWidgetItem(f"{block.prefix} [{active}] points={count if count >= 0 else '?'}")
            self.block_list.addItem(item)
        self.block_list.blockSignals(False)
        if self.blocks:
            self.block_list.setCurrentRow(0)
            self._load_block(0)

    def _load_block(self, index: int) -> None:
        if index < 0 or index >= len(self.blocks) or self._building_block:
            return
        block = self.blocks[index]
        self.block_prefix.setText(block.prefix)
        self.block_traversal.setCurrentText(find_choice_by_token(block.traversal, MAG_TRAVERSAL_CHOICES))
        self.block_total_points.setText(str(block.total_points))
        for axis, spec in block.axes.items():
            widgets = self.axis_widgets[axis]
            widgets["enabled"].setChecked(spec.enabled)
            widgets["mode"].setCurrentText(find_choice_by_token(spec.mode, MAG_AXIS_MODE_CHOICES))
            widgets["fixed"].set_text(spec.fixed)
            widgets["start"].set_text(spec.start)
            widgets["stop"].set_text(spec.stop)
            widgets["step"].set_text(spec.step)
            widgets["values_text"].setText(spec.values_text)

    def _read_axis(self, axis: str) -> AxisSpec:
        widgets = self.axis_widgets[axis]
        values_text = widgets["values_text"].text().strip()
        if choice_token(widgets["mode"].currentText()) == "list":
            values_text = ", ".join(
                format_number(to_canonical_unit(value, "field", "nT"))
                for value in parse_values(values_text)
            )
        return AxisSpec(
            enabled=widgets["enabled"].isChecked(),
            mode=choice_token(widgets["mode"].currentText()),
            fixed=widgets["fixed"].canonical("field"),
            start=widgets["start"].canonical("field"),
            stop=widgets["stop"].canonical("field"),
            step=widgets["step"].canonical("field"),
            values_text=values_text,
        )

    def _read_block(self) -> ScanBlock:
        return ScanBlock(
            prefix=self.block_prefix.text().strip() or "scan",
            traversal=choice_token(self.block_traversal.currentText()),
            total_points=int(self.block_total_points.text() or "0"),
            axes={axis: self._read_axis(axis) for axis in ("x", "y", "z")},
        )

    def _add_or_update_block(self, show_error: bool = True) -> bool:
        try:
            block = self._read_block()
            expand_block(block)
            row = self.block_list.currentRow()
            if row < 0:
                self.blocks.append(block)
            else:
                self.blocks[row] = block
            self._refresh_block_list()
            return True
        except Exception as exc:
            if show_error:
                QMessageBox.warning(self, "Invalid scan block", str(exc))
            return False

    def _remove_block(self) -> None:
        row = self.block_list.currentRow()
        if row >= 0:
            del self.blocks[row]
        if not self.blocks:
            self.blocks.append(self.default_block("x_line", "x"))
        self._refresh_block_list()

    def _add_xyz_blocks(self) -> None:
        self.blocks.extend([
            self.default_block("x_line", "x"),
            self.default_block("y_line", "y"),
            self.default_block("z_line", "z"),
        ])
        self._refresh_block_list()

    def _request(self) -> GeneratorRequest:
        plan_kind = choice_token(self.plan_kind.currentText())
        if plan_kind == "magnetic_scan" and not self._add_or_update_block(show_error=False):
            raise ValueError("current magnetic scan block is invalid; fix it before generating JSON")
        return GeneratorRequest(
            run_id=self.run_id.text().strip(),
            operator=self.operator.text().strip(),
            acquisition_window_ms=int(round(self.acquisition_window.canonical("time_ms"))),
            point_settle_ms=int(round(self.point_settle.canonical("time_ms"))),
            blocks=self.blocks if plan_kind == "magnetic_scan" else [],
            plan_kind=plan_kind,
            acquisition_step_count=int(self.acquisition_step_count.text() or "1"),
            fixed_b_nt=[
                self.fixed_x.canonical("field"),
                self.fixed_y.canonical("field"),
                self.fixed_z.canonical("field"),
            ],
            mag_baseline_policy={
                "baseline_x_a": self.baseline_x.canonical("current_a"),
                "baseline_y_a": self.baseline_y.canonical("current_a"),
                "baseline_z_a": self.baseline_z.canonical("current_a"),
                "settle_ms": int(round(self.baseline_settle.canonical("time_ms"))),
                "readback_samples": int(self.readback_samples.text()),
                "settle_tolerance_a": self.settle_tolerance.canonical("current_a"),
                "voltage_v": self.voltage.canonical("voltage_v"),
                "voltage_protection_v": self.voltage_protection.canonical("voltage_v"),
                "output_enabled": self.output_enabled.isChecked(),
            },
            quality_thresholds={
                "min_frames": int(self.min_frames.text()),
                "max_timeout_count": int(self.max_timeout.text()),
                "max_duplicate_ratio": float(self.max_duplicate.text()),
                "max_last_frame_age_ms": int(round(self.max_last_age.canonical("time_ms"))),
            },
            smb_profile_id=self.smb_profile_id.text().strip(),
            smb_command_settle_ms=int(round(self.smb_command_settle.canonical("time_ms"))),
            smb_error_check_after_write=self.smb_error_check.isChecked(),
            smb_fixed={
                "modulation_enabled": self.smb_mod_enabled.isChecked(),
                "fm_enabled": self.smb_fm_enabled.isChecked(),
                "fm_source": self.smb_fm_source.currentText(),
                "fm_mode": self.smb_fm_mode.currentText(),
                "fm_deviation_hz": self.smb_fm_deviation.canonical("frequency_hz"),
                "lf_output_enabled": self.smb_lf_enabled.isChecked(),
                "lf_voltage_mv": self.smb_lf_voltage.canonical("voltage_mv"),
                "lf_frequency_hz": self.smb_lf_frequency.canonical("frequency_hz"),
                "lf_shape": self.smb_lf_shape.currentText(),
                "lf_source_impedance": self.smb_lf_impedance.currentText(),
            },
            smb_sweep={
                "start_hz": self.smb_start.canonical("frequency_hz"),
                "stop_hz": self.smb_stop.canonical("frequency_hz"),
                "step_hz": self.smb_step.canonical("frequency_hz"),
                "dwell_ms": int(round(self.smb_dwell.canonical("time_ms"))),
                "power_dbm": self.smb_power.value(),
                "sweep_mode": self.smb_sweep_mode.currentText(),
                "spacing": self.smb_spacing.currentText(),
                "shape": self.smb_shape.currentText(),
                "trigger_source": self.smb_trigger.currentText(),
                "output_voltage_start_v": self.smb_voltage_start.value(),
                "output_voltage_stop_v": self.smb_voltage_stop.value(),
                "rf_output_enabled": self.smb_rf_output.isChecked(),
            },
            oe_profile_id=self.oe_profile_id.text().strip(),
            oe_command_settle_ms=int(round(self.oe_command_settle.canonical("time_ms"))),
            oe_fixed={
                key: choice_code(widget.currentText()) if key in OE_CHOICE_KEYS and isinstance(widget, QComboBox)
                else widget.value() if isinstance(widget, NumberUnitInput)
                else int(widget.text()) if isinstance(widget, QLineEdit) and key.startswith("harmonic")
                else widget.text()
                for key, widget in self.oe_fields.items()
            },
            laser_profile_id=self.laser_profile_id.text().strip(),
            laser_mode=LASER_MODE_TOKEN_BY_LABEL.get(self.laser_mode.currentText(), "off_background"),
            laser_power_mw=int(round(self.laser_power.canonical("power_mw"))),
            laser_settle_ms=int(round(self.laser_settle.canonical("time_ms"))),
        )

    def _generate(self) -> None:
        try:
            request = self._request()
            bundle = generate_config_bundle(
                request,
                self.output_dir.path(),
                plan_template_path=self.plan_template.path(),
                smb_template_path=self.smb_template.path(),
                oe_template_path=self.oe_template.path(),
                laser_template_path=self.laser_template.path(),
            )
            output = ["已生成 JSON 并绑定到本次实验配置：", ""]
            output.extend(f"{key}: {value}" for key, value in asdict(bundle).items())
            if request.plan_kind == "no_magnetic_control":
                preview_points = max(1, request.acquisition_step_count)
            elif request.plan_kind == "constant_field":
                preview_points = 1
            else:
                preview_points = sum(len(expand_block(block)) for block in request.blocks)
            output.extend([
                "",
                f"plan_kind={request.plan_kind}",
                f"scan_blocks={len(request.blocks)}",
                f"resolved_preview_points={preview_points}",
                "下一步：打开“预检查 / 预计用时”，通过后到“运行监控”启动。",
            ])
            self.generate_output.setPlainText("\n".join(output))
            self.bundle_generated.emit(bundle)
        except Exception as exc:
            QMessageBox.critical(self, "Generate failed", str(exc))

    @staticmethod
    def default_block(prefix: str, active_axis: str) -> ScanBlock:
        return ScanBlock(
            prefix=prefix,
            traversal="raster",
            axes={
                "x": AxisSpec(enabled=active_axis == "x", start=0, stop=40 if active_axis == "x" else 0, step=10, fixed=0, values_text="0, 10, 20, 30, 40"),
                "y": AxisSpec(enabled=active_axis == "y", start=0, stop=40 if active_axis == "y" else 0, step=10, fixed=0, values_text="0, 10, 20, 30, 40"),
                "z": AxisSpec(enabled=active_axis == "z", start=0, stop=40 if active_axis == "z" else 0, step=10, fixed=0, values_text="0, 10, 20, 30, 40"),
            },
        )


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


class RunMonitorPage(QWidget):
    def __init__(self, bundle_provider: Callable[[], RunBundle], out_dir_provider: Callable[[], str]) -> None:
        super().__init__()
        self.bundle_provider = bundle_provider
        self.out_dir_provider = out_dir_provider
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
        layout.addWidget(note_label("启动 C# run-execute。进度来自 progress JSONL，stdout/stderr 只写入 control 日志，避免 pipe 阻塞。"))
        actions = QHBoxLayout()
        self.start_button = QPushButton("启动运行")
        self.start_button.clicked.connect(self.start_run)
        self.stop_button = QPushButton("当前点结束后停止")
        self.stop_button.clicked.connect(self.stop_after_point)
        self.stop_button.setEnabled(False)
        self.emergency_button = QPushButton("急停")
        self.emergency_button.clicked.connect(self.emergency_stop)
        self.emergency_button.setEnabled(False)
        self.emergency_button.setStyleSheet("QPushButton { color: white; background: #b00020; font-weight: 700; }")
        actions.addWidget(self.start_button)
        actions.addWidget(self.stop_button)
        actions.addWidget(self.emergency_button)
        actions.addStretch(1)
        layout.addLayout(actions)
        status = QGroupBox("运行状态")
        status_layout = QGridLayout(status)
        self.state = QLabel("idle")
        self.point = QLabel("-")
        self.frames = QLabel("-")
        self.counts = QLabel("-")
        self.elapsed = QLabel("-")
        self.remaining = QLabel("-")
        add_display_row(status_layout, 0, "状态", self.state)
        add_display_row(status_layout, 1, "Point", self.point)
        add_display_row(status_layout, 2, "Frames", self.frames)
        add_display_row(status_layout, 3, "计数", self.counts)
        add_display_row(status_layout, 4, "已用时间", self.elapsed)
        add_display_row(status_layout, 5, "预计剩余", self.remaining)
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
        self.progress_offset = 0
        self.latest_progress = None
        self.run_started_monotonic = time.monotonic()
        self.current_sweep_started_monotonic = None
        self.current_sweep_estimated_ms = None
        self.estimated_run_ms = None
        self.total_progress.setValue(0)
        self.point_progress.setValue(0)
        self.table.setRowCount(0)
        self.stop_button.setEnabled(True)
        self.emergency_button.setEnabled(True)
        self.log.setPlainText(json.dumps(asdict(handle), indent=2, ensure_ascii=False))
        self.timer.start()

    def _failed(self, message: str) -> None:
        self.start_button.setEnabled(True)
        self.stop_button.setEnabled(False)
        self.emergency_button.setEnabled(False)
        self.timer.stop()
        self.log.setPlainText(message)

    def stop_after_point(self) -> None:
        if not self.handle:
            return
        request_stop(self.handle.control_paths.stop_request_file)
        self.log.appendPlainText(f"\nstop requested: {self.handle.control_paths.stop_request_file}")

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
            self.point.setText(f"{latest.get('point_id') or '-'}  {latest.get('point_index') or '-'} / {latest.get('points_total') or '-'}")
            self.frames.setText(str(latest.get("frames_total") or "-"))
            self.counts.setText(
                f"timeout={latest.get('timeout_count')} raw_len_bad={latest.get('raw_len_bad_count')} delta_gt1={latest.get('delta_gt1_count')}"
            )
            self._update_estimated_progress(latest)
            if latest.get("state") in {"Completed", "Failed", "Aborted", "CleanupFailed"}:
                self.timer.stop()
                self.start_button.setEnabled(True)
                self.stop_button.setEnabled(False)
                self.emergency_button.setEnabled(False)
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
            self.emergency_button.setEnabled(False)
            self.state.setText("process exited")
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


class MainWindow(QMainWindow):
    def __init__(self) -> None:
        super().__init__()
        self.setWindowTitle("ODMR PySide6 Console")
        self.resize(1380, 900)
        self.setMinimumSize(1180, 760)
        central = QWidget()
        self.setCentralWidget(central)
        layout = QHBoxLayout(central)
        layout.setContentsMargins(12, 12, 12, 12)
        self.nav = QListWidget()
        self.nav.setFixedWidth(210)
        self.nav.addItems(["本次实验配置", "配置生成", "预检查 / 预计用时", "运行监控", "数据审查"])
        self.nav.setCurrentRow(0)
        layout.addWidget(self.nav)
        self.stack = QStackedWidget()
        layout.addWidget(self.stack, 1)

        self.run_bundle = RunBundlePage()
        self.config_generator = ConfigGeneratorPage()
        self.resolve_page = ResolvePage(self.run_bundle.bundle)
        self.monitor_page = RunMonitorPage(self.run_bundle.bundle, self.current_out_dir)
        self.review_page = ArtifactReviewPage(self.current_out_dir)
        for page in [self.run_bundle, self.config_generator, self.resolve_page, self.monitor_page, self.review_page]:
            self.stack.addWidget(page)
        self.nav.currentRowChanged.connect(self.stack.setCurrentIndex)
        self.config_generator.bundle_generated.connect(self._bind_generated_bundle)

    def current_out_dir(self) -> str:
        return self.run_bundle.out_dir.text().strip()

    def _bind_generated_bundle(self, bundle: RunBundle) -> None:
        self.run_bundle.set_bundle(bundle)
        self.run_bundle.make_new_out_dir()
        self.nav.setCurrentRow(0)


def combo(values: list[str], current: str) -> QComboBox:
    widget = QComboBox()
    widget.addItems(values)
    widget.setCurrentText(current)
    widget.setMinimumWidth(140)
    return widget


def add_form_row(layout: QGridLayout, row: int, column_pair: int, label: str, widget: QWidget) -> None:
    label_widget = QLabel(label)
    label_widget.setWordWrap(True)
    label_widget.setMinimumWidth(150)
    value_column = column_pair * 2 + 1
    label_column = column_pair * 2
    layout.addWidget(label_widget, row, label_column)
    layout.addWidget(widget, row, value_column)
    layout.setColumnStretch(value_column, 1)


def add_display_row(layout: QGridLayout, row: int, label: str, widget: QWidget) -> None:
    label_widget = QLabel(label)
    label_widget.setMinimumWidth(100)
    layout.addWidget(label_widget, row, 0)
    layout.addWidget(widget, row, 1)
    layout.setColumnStretch(1, 1)


def main() -> int:
    app = QApplication(sys.argv)
    app.setApplicationName("ODMR PySide6 Console")
    font = QFont()
    font.setPointSize(12)
    app.setFont(font)
    app.setStyleSheet(
        """
        QGroupBox { font-weight: 600; margin-top: 14px; }
        QGroupBox::title { subcontrol-origin: margin; left: 8px; padding: 0 3px; }
        QLineEdit, QComboBox, QPlainTextEdit, QTableWidget { font-weight: 400; }
        QPushButton { min-height: 28px; padding-left: 10px; padding-right: 10px; }
        """
    )
    window = MainWindow()
    window.show()
    return app.exec()


if __name__ == "__main__":
    raise SystemExit(main())
