from __future__ import annotations

from datetime import datetime
from pathlib import Path
import subprocess
import sys
from typing import Any, Callable

from PySide6.QtCore import QThread, Signal
from PySide6.QtWidgets import (
    QComboBox,
    QFileDialog,
    QFrame,
    QGridLayout,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QMessageBox,
    QPushButton,
    QScrollArea,
    QWidget,
)

from odmr_console_core import REPO_ROOT

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
LOCKIN_MODEL_CHOICES = ["oe1022d", "oe1300"]


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


def detect_station_lockin_model(station: dict[str, Any]) -> str | None:
    devices = station.get("devices")
    if not isinstance(devices, list):
        return None
    models = [device.get("kind") for device in devices if isinstance(device, dict) and device.get("kind") in LOCKIN_MODEL_CHOICES]
    if len(models) != 1:
        return None
    return str(models[0])


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
        self.fixed_unit = unit or ""
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
        return self.unit_combo.currentText() if self.unit_combo else self.fixed_unit

    def set_unit(self, unit: str) -> None:
        if self.unit_combo:
            self.unit_combo.setCurrentText(unit)
        else:
            self.fixed_unit = unit

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
