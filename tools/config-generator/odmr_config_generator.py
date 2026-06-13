from __future__ import annotations

import sys
from pathlib import Path
import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from odmr_config_core import (
    AxisSpec,
    GeneratorRequest,
    ScanBlock,
    from_canonical_unit,
    load_json,
    parse_values,
    to_canonical_unit,
    write_generated_bundle,
)


STEP_TITLES = [
    "1 Templates / Output",
    "2 Magnetic Plan",
    "3 Plan Policy",
    "4 SMB100A",
    "5 OE1022D",
    "6 CNI Laser",
    "7 Generate",
]


def find_repo_root() -> Path:
    current = Path(__file__).resolve()
    for parent in [current.parent, *current.parents]:
        if (parent / "configs").is_dir() and (parent / "tools" / "win-csharp").is_dir():
            return parent
    raise RuntimeError("could not find repository root")


def format_number(value: float) -> str:
    text = f"{float(value):.12g}"
    return "0" if text == "-0" else text


def code_choice(code: int, label: str) -> str:
    return f"{code} - {label}"


def choice_code(value: object) -> int:
    text = str(value).strip()
    return int(text.split("-", 1)[0].strip())


SMB_FM_SOURCE_CHOICES = ["INT", "EXT", "INT,EXT"]
SMB_FM_MODE_CHOICES = ["NORM", "LNO", "HDEV"]
SMB_LF_SHAPE_CHOICES = ["SINE", "SQU", "TRI", "SAWT", "ISAW"]
SMB_LF_IMPEDANCE_CHOICES = ["LOW", "G600"]
SMB_SWEEP_MODE_CHOICES = ["AUTO", "MAN", "STEP"]
SMB_SWEEP_SPACING_CHOICES = ["LIN", "LOG"]
SMB_SWEEP_SHAPE_CHOICES = ["SAWT", "TRI"]
SMB_TRIGGER_SOURCE_CHOICES = ["AUTO", "SING", "EXT"]
FIELD_UNIT_CHOICES = ["nT", "uT", "mT"]
CURRENT_UNIT_CHOICES = ["A", "mA"]
VOLTAGE_UNIT_CHOICES = ["V", "mV"]
TIME_UNIT_CHOICES = ["ms", "s"]
FREQUENCY_UNIT_CHOICES = ["Hz", "kHz", "MHz", "GHz"]
LOW_FREQUENCY_UNIT_CHOICES = ["Hz", "kHz", "MHz"]
LF_VOLTAGE_UNIT_CHOICES = ["mV", "V"]
LASER_POWER_UNIT_CHOICES = ["mW", "W"]

OE_CHOICES = {
    "channel": [code_choice(1, "Ch-A"), code_choice(2, "Ch-B")],
    "input_source": [
        code_choice(0, "single-ended voltage A"),
        code_choice(1, "differential voltage A-B"),
        code_choice(2, "1 MOhm current gain"),
        code_choice(3, "100 MOhm current gain"),
    ],
    "input_grounding": [code_choice(0, "float"), code_choice(1, "ground")],
    "input_coupling": [code_choice(0, "AC"), code_choice(1, "DC")],
    "line_notch_filter": [
        code_choice(0, "off"),
        code_choice(1, "50 Hz"),
        code_choice(2, "50 Hz + 100 Hz"),
        code_choice(3, "100 Hz"),
    ],
    "reference_source": [
        code_choice(0, "external"),
        code_choice(1, "internal"),
        code_choice(2, "internal sweep"),
    ],
    "reference_slope": [
        code_choice(0, "TTL rising edge"),
        code_choice(1, "sine zero crossing"),
        code_choice(2, "observed LabVIEW locked readback"),
    ],
    "dynamic_reserve": [
        code_choice(0, "low noise"),
        code_choice(1, "normal"),
        code_choice(2, "high reserve"),
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
    "sync_filter": [code_choice(0, "off"), code_choice(1, "on")],
    "sine_output_mode": [
        code_choice(0, "fixed amplitude"),
        code_choice(1, "linear sweep amplitude"),
        code_choice(2, "log sweep amplitude"),
        code_choice(3, "DC output"),
    ],
}
OE_CHOICE_KEYS = set(OE_CHOICES)


def oe_choice_for_code(key: str, value: object) -> str:
    code = choice_code(value)
    for choice in OE_CHOICES[key]:
        if choice_code(choice) == code:
            return choice
    return str(code)


class ScrollPage(ttk.Frame):
    def __init__(self, parent: tk.Widget) -> None:
        super().__init__(parent)
        self.canvas = tk.Canvas(self, highlightthickness=0)
        self.scrollbar = ttk.Scrollbar(self, orient=tk.VERTICAL, command=self.canvas.yview)
        self.body = ttk.Frame(self.canvas, padding=12)
        self.window_id = self.canvas.create_window((0, 0), window=self.body, anchor="nw")
        self.canvas.configure(yscrollcommand=self.scrollbar.set)
        self.canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        self.scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.body.bind("<Configure>", self._update_scrollregion)
        self.canvas.bind("<Configure>", self._resize_window)

    def _update_scrollregion(self, _event: tk.Event) -> None:
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))

    def _resize_window(self, event: tk.Event) -> None:
        self.canvas.itemconfigure(self.window_id, width=event.width)


class ConfigGeneratorApp(tk.Tk):
    def __init__(self) -> None:
        super().__init__()
        self.repo_root = find_repo_root()
        self.title("ODMR Config Generator")
        self.geometry("1280x860")
        self.minsize(1100, 760)

        self.template_vars: dict[str, tk.StringVar] = {}
        self.run_vars: dict[str, tk.Variable] = {}
        self.baseline_vars: dict[str, tk.Variable] = {}
        self.quality_vars: dict[str, tk.Variable] = {}
        self.smb_vars: dict[str, tk.Variable] = {}
        self.oe_vars: dict[str, tk.Variable] = {}
        self.laser_vars: dict[str, tk.Variable] = {}
        self.unit_vars: dict[str, tk.StringVar] = {
            "field": tk.StringVar(value="nT"),
            "baseline_current": tk.StringVar(value="A"),
            "voltage": tk.StringVar(value="V"),
            "time": tk.StringVar(value="ms"),
            "rf_frequency": tk.StringVar(value="GHz"),
            "rf_step": tk.StringVar(value="MHz"),
            "fm_deviation": tk.StringVar(value="MHz"),
            "lf_frequency": tk.StringVar(value="Hz"),
            "lf_voltage": tk.StringVar(value="mV"),
            "laser_power": tk.StringVar(value="mW"),
        }
        self.axis_vars: dict[str, dict[str, tk.Variable]] = {}
        self.blocks: list[ScanBlock] = [self.default_block("x_line", "x")]
        self.current_step = 0
        self._unit_last_values = {key: variable.get() for key, variable in self.unit_vars.items()}

        self._build_ui()
        self._load_default_paths()
        self._refresh_block_list()
        self._load_block(0)
        self._load_template_values()
        self._install_unit_traces()

    def _build_ui(self) -> None:
        root = ttk.Frame(self, padding=10)
        root.pack(fill=tk.BOTH, expand=True)
        root.columnconfigure(1, weight=1)
        root.rowconfigure(0, weight=1)

        side = ttk.Frame(root, width=230)
        side.grid(row=0, column=0, sticky="ns", padx=(0, 10))
        side.grid_propagate(False)
        ttk.Label(side, text="Configuration Order", font=("", 11, "bold")).pack(anchor="w")
        ttk.Label(
            side,
            text="先选模板和输出目录，再定义磁场扫描；随后确认 Plan policy、SMB、OE、Laser，最后生成 JSON 给 C# Run Bundle 使用。",
            wraplength=205,
            justify=tk.LEFT,
        ).pack(anchor="w", pady=(6, 10))
        self.step_list = tk.Listbox(side, height=len(STEP_TITLES), exportselection=False)
        self.step_list.pack(fill=tk.X)
        for title in STEP_TITLES:
            self.step_list.insert(tk.END, title)
        self.step_list.selection_set(0)
        self.step_list.bind("<<ListboxSelect>>", self._select_step_from_list)
        ttk.Button(side, text="Previous", command=lambda: self._move_step(-1)).pack(fill=tk.X, pady=(16, 4))
        ttk.Button(side, text="Next", command=lambda: self._move_step(1)).pack(fill=tk.X)

        self.content = ttk.Frame(root)
        self.content.grid(row=0, column=1, sticky="nsew")
        self.content.rowconfigure(0, weight=1)
        self.content.columnconfigure(0, weight=1)
        self.pages = [ScrollPage(self.content) for _ in STEP_TITLES]
        for page in self.pages:
            page.grid(row=0, column=0, sticky="nsew")

        self._build_templates_page(self.pages[0].body)
        self._build_magnetic_page(self.pages[1].body)
        self._build_policy_page(self.pages[2].body)
        self._build_smb_page(self.pages[3].body)
        self._build_oe_page(self.pages[4].body)
        self._build_laser_page(self.pages[5].body)
        self._build_generate_page(self.pages[6].body)
        self._show_step(0)

    def _build_templates_page(self, parent: ttk.Frame) -> None:
        self._section_title(parent, "Templates and output paths", 0)
        for row, (key, label) in enumerate([
            ("plan", "Plan template"),
            ("smb", "SMB100A profile template"),
            ("oe", "OE1022D profile template"),
            ("laser", "CNI laser profile template"),
            ("output", "Generated config directory"),
        ], start=1):
            var = tk.StringVar()
            self.template_vars[key] = var
            self._path_row(parent, label, var, row, is_directory=key == "output")
        ttk.Button(parent, text="Load template values into forms", command=self._load_template_values).grid(row=7, column=1, sticky="w", pady=14)
        ttk.Label(
            parent,
            text="输出结果仍然是现有 C# runtime 的 plan/profile JSON。这里不创建新的 run bundle schema。",
            wraplength=820,
        ).grid(row=8, column=0, columnspan=3, sticky="w", pady=(10, 0))

    def _build_magnetic_page(self, parent: ttk.Frame) -> None:
        self._section_title(parent, "Run identity", 0)
        self.run_vars = {
            "run_id": tk.StringVar(value="generated_plan"),
            "operator": tk.StringVar(value="local"),
            "acquisition_window_ms": tk.DoubleVar(value=0),
            "point_settle_ms": tk.DoubleVar(value=500),
        }
        self._form_field(parent, "Run ID", self.run_vars["run_id"], 1, 0)
        self._form_field(parent, "Operator", self.run_vars["operator"], 2, 0)
        self._form_field(parent, "Acquisition window", self.run_vars["acquisition_window_ms"], 1, 2, unit=(self.unit_vars["time"], TIME_UNIT_CHOICES))
        self._form_field(parent, "Point settle", self.run_vars["point_settle_ms"], 2, 2, unit=(self.unit_vars["time"], TIME_UNIT_CHOICES))

        self._section_title(parent, "Scan blocks", 4)
        block_frame = ttk.Frame(parent)
        block_frame.grid(row=5, column=0, columnspan=4, sticky="nsew")
        block_frame.columnconfigure(1, weight=1)
        self.block_list = tk.Listbox(block_frame, height=9, exportselection=False)
        self.block_list.grid(row=0, column=0, rowspan=6, sticky="nsw", padx=(0, 12))
        self.block_list.bind("<<ListboxSelect>>", self._on_block_select)

        self.block_prefix = tk.StringVar()
        self.block_traversal = tk.StringVar(value="raster")
        self.block_total_points = tk.IntVar(value=0)
        self._form_field(block_frame, "Block prefix", self.block_prefix, 0, 1)
        self._form_field(block_frame, "Traversal", self.block_traversal, 1, 1, choices=["raster", "bounce_1d_x"])
        self._form_field(block_frame, "Total points (0 = once)", self.block_total_points, 2, 1)

        axis_area = ttk.Frame(parent)
        axis_area.grid(row=6, column=0, columnspan=4, sticky="ew", pady=(12, 0))
        axis_area.columnconfigure(0, weight=1)
        axis_area.columnconfigure(1, weight=1)
        axis_area.columnconfigure(2, weight=1)
        for index, axis in enumerate(["x", "y", "z"]):
            self._axis_panel(axis_area, axis, index)

        actions = ttk.Frame(parent)
        actions.grid(row=7, column=0, columnspan=4, sticky="w", pady=14)
        ttk.Button(actions, text="Add / Update selected block", command=self._add_or_update_block).pack(side=tk.LEFT, padx=(0, 8))
        ttk.Button(actions, text="Remove selected block", command=self._remove_block).pack(side=tk.LEFT, padx=(0, 8))
        ttk.Button(actions, text="Add X/Y/Z single-axis blocks", command=self._add_xyz_blocks).pack(side=tk.LEFT)

    def _build_policy_page(self, parent: ttk.Frame) -> None:
        self._section_title(parent, "Maynuo baseline / output policy", 0)
        self.baseline_vars = {
            "baseline_x_a": tk.DoubleVar(value=0.0),
            "baseline_y_a": tk.DoubleVar(value=0.0),
            "baseline_z_a": tk.DoubleVar(value=0.0),
            "settle_ms": tk.DoubleVar(value=1000),
            "readback_samples": tk.IntVar(value=3),
            "settle_tolerance_a": tk.DoubleVar(value=0.002),
            "voltage_v": tk.DoubleVar(value=75.0),
            "voltage_protection_v": tk.DoubleVar(value=75.0),
            "output_enabled": tk.BooleanVar(value=True),
        }
        self._form_grid(parent, self.baseline_vars, [
            ("baseline_x_a", "Baseline X current"),
            ("baseline_y_a", "Baseline Y current"),
            ("baseline_z_a", "Baseline Z current"),
            ("settle_ms", "Settle"),
            ("readback_samples", "Readback samples"),
            ("settle_tolerance_a", "Settle tolerance A"),
            ("voltage_v", "Voltage"),
            ("voltage_protection_v", "Voltage protection"),
            ("output_enabled", "Output enabled"),
        ], start_row=1, units={
            "baseline_x_a": (self.unit_vars["baseline_current"], CURRENT_UNIT_CHOICES),
            "baseline_y_a": (self.unit_vars["baseline_current"], CURRENT_UNIT_CHOICES),
            "baseline_z_a": (self.unit_vars["baseline_current"], CURRENT_UNIT_CHOICES),
            "settle_ms": (self.unit_vars["time"], TIME_UNIT_CHOICES),
            "settle_tolerance_a": (self.unit_vars["baseline_current"], CURRENT_UNIT_CHOICES),
            "voltage_v": (self.unit_vars["voltage"], VOLTAGE_UNIT_CHOICES),
            "voltage_protection_v": (self.unit_vars["voltage"], VOLTAGE_UNIT_CHOICES),
        })

        self._section_title(parent, "Point quality thresholds", 7)
        self.quality_vars = {
            "min_frames": tk.IntVar(value=20),
            "max_timeout_count": tk.IntVar(value=2),
            "max_duplicate_ratio": tk.DoubleVar(value=0.3),
            "max_last_frame_age_ms": tk.DoubleVar(value=500),
        }
        self._form_grid(parent, self.quality_vars, [
            ("min_frames", "Min frames"),
            ("max_timeout_count", "Max timeout count"),
            ("max_duplicate_ratio", "Max duplicate ratio"),
            ("max_last_frame_age_ms", "Max last frame age"),
        ], start_row=8, units={
            "max_last_frame_age_ms": (self.unit_vars["time"], TIME_UNIT_CHOICES),
        })

    def _build_smb_page(self, parent: ttk.Frame) -> None:
        self.smb_vars = {
            "profile_id": tk.StringVar(),
            "command_settle_ms": tk.DoubleVar(value=500),
            "error_check_after_write": tk.BooleanVar(value=True),
            "modulation_enabled": tk.BooleanVar(value=True),
            "fm_enabled": tk.BooleanVar(value=True),
            "fm_source": tk.StringVar(value="INT"),
            "fm_mode": tk.StringVar(value="HDEV"),
            "fm_deviation_hz": tk.DoubleVar(value=4000000.0),
            "lf_output_enabled": tk.BooleanVar(value=True),
            "lf_voltage_mv": tk.DoubleVar(value=137.0),
            "lf_frequency_hz": tk.DoubleVar(value=500.0),
            "lf_shape": tk.StringVar(value="SQU"),
            "lf_source_impedance": tk.StringVar(value="LOW"),
            "start_hz": tk.DoubleVar(value=2830000000.0),
            "stop_hz": tk.DoubleVar(value=2890000000.0),
            "step_hz": tk.DoubleVar(value=500000.0),
            "dwell_ms": tk.DoubleVar(value=300),
            "power_dbm": tk.DoubleVar(value=-10.0),
            "sweep_mode": tk.StringVar(value="AUTO"),
            "spacing": tk.StringVar(value="LIN"),
            "shape": tk.StringVar(value="SAWT"),
            "trigger_source": tk.StringVar(value="AUTO"),
            "output_voltage_start_v": tk.DoubleVar(value=0.0),
            "output_voltage_stop_v": tk.DoubleVar(value=3.0),
            "rf_output_enabled": tk.BooleanVar(value=True),
        }
        self._section_title(parent, "SMB100A profile identity", 0)
        self._form_grid(parent, self.smb_vars, [
            ("profile_id", "Profile ID"),
            ("command_settle_ms", "Command settle"),
            ("error_check_after_write", "Check SYST:ERR? after batch"),
        ], start_row=1, units={
            "command_settle_ms": (self.unit_vars["time"], TIME_UNIT_CHOICES),
        })
        self._section_title(parent, "SMB fixed modulation profile", 4)
        self._form_grid(parent, self.smb_vars, [
            ("modulation_enabled", "Modulation enabled"),
            ("fm_enabled", "FM enabled"),
            ("fm_source", "FM source", SMB_FM_SOURCE_CHOICES),
            ("fm_mode", "FM mode", SMB_FM_MODE_CHOICES),
            ("fm_deviation_hz", "FM deviation"),
            ("lf_output_enabled", "LF output enabled"),
            ("lf_voltage_mv", "LF voltage"),
            ("lf_frequency_hz", "LF frequency"),
            ("lf_shape", "LF shape", SMB_LF_SHAPE_CHOICES),
            ("lf_source_impedance", "LF source impedance", SMB_LF_IMPEDANCE_CHOICES),
        ], start_row=5, units={
            "fm_deviation_hz": (self.unit_vars["fm_deviation"], FREQUENCY_UNIT_CHOICES),
            "lf_voltage_mv": (self.unit_vars["lf_voltage"], LF_VOLTAGE_UNIT_CHOICES),
            "lf_frequency_hz": (self.unit_vars["lf_frequency"], LOW_FREQUENCY_UNIT_CHOICES),
        })
        self._section_title(parent, "SMB default RF sweep", 11)
        self._form_grid(parent, self.smb_vars, [
            ("start_hz", "Start"),
            ("stop_hz", "Stop"),
            ("step_hz", "Step"),
            ("dwell_ms", "Dwell"),
            ("power_dbm", "Power"),
            ("sweep_mode", "Sweep mode", SMB_SWEEP_MODE_CHOICES),
            ("spacing", "Spacing", SMB_SWEEP_SPACING_CHOICES),
            ("shape", "Shape", SMB_SWEEP_SHAPE_CHOICES),
            ("trigger_source", "Trigger source", SMB_TRIGGER_SOURCE_CHOICES),
            ("output_voltage_start_v", "Output voltage start"),
            ("output_voltage_stop_v", "Output voltage stop"),
            ("rf_output_enabled", "RF output enabled"),
        ], start_row=12, units={
            "start_hz": (self.unit_vars["rf_frequency"], FREQUENCY_UNIT_CHOICES),
            "stop_hz": (self.unit_vars["rf_frequency"], FREQUENCY_UNIT_CHOICES),
            "step_hz": (self.unit_vars["rf_step"], FREQUENCY_UNIT_CHOICES),
            "dwell_ms": (self.unit_vars["time"], TIME_UNIT_CHOICES),
            "power_dbm": "dBm",
            "output_voltage_start_v": "V",
            "output_voltage_stop_v": "V",
        })

    def _build_oe_page(self, parent: ttk.Frame) -> None:
        self.oe_vars = {
            "profile_id": tk.StringVar(),
            "command_settle_ms": tk.DoubleVar(value=500),
            "channel": tk.StringVar(value=oe_choice_for_code("channel", 2)),
            "input_source": tk.StringVar(value=oe_choice_for_code("input_source", 0)),
            "input_grounding": tk.StringVar(value=oe_choice_for_code("input_grounding", 0)),
            "input_coupling": tk.StringVar(value=oe_choice_for_code("input_coupling", 1)),
            "line_notch_filter": tk.StringVar(value=oe_choice_for_code("line_notch_filter", 0)),
            "reference_source": tk.StringVar(value=oe_choice_for_code("reference_source", 0)),
            "reference_slope": tk.StringVar(value=oe_choice_for_code("reference_slope", 2)),
            "phase_deg": tk.DoubleVar(value=0.0),
            "harmonic_1": tk.IntVar(value=1),
            "harmonic_2": tk.IntVar(value=1),
            "dynamic_reserve": tk.StringVar(value=oe_choice_for_code("dynamic_reserve", 1)),
            "sensitivity_index": tk.StringVar(value=oe_choice_for_code("sensitivity_index", 24)),
            "time_constant_index": tk.StringVar(value=oe_choice_for_code("time_constant_index", 9)),
            "filter_slope": tk.StringVar(value=oe_choice_for_code("filter_slope", 1)),
            "sync_filter": tk.StringVar(value=oe_choice_for_code("sync_filter", 0)),
            "sine_output_mode": tk.StringVar(value=oe_choice_for_code("sine_output_mode", 0)),
            "sine_output_voltage_vrms": tk.DoubleVar(value=1.0),
        }
        self._section_title(parent, "OE1022D fixed profile", 0)
        oe_fields: list[tuple[str, str] | tuple[str, str, list[str]]] = [
            ("profile_id", "Profile ID"),
            ("command_settle_ms", "Command settle"),
            ("channel", "Channel", OE_CHOICES["channel"]),
            ("input_source", "Input source", OE_CHOICES["input_source"]),
            ("input_grounding", "Input grounding", OE_CHOICES["input_grounding"]),
            ("input_coupling", "Input coupling", OE_CHOICES["input_coupling"]),
            ("line_notch_filter", "Line notch filter", OE_CHOICES["line_notch_filter"]),
            ("reference_source", "Reference source", OE_CHOICES["reference_source"]),
            ("reference_slope", "Reference slope", OE_CHOICES["reference_slope"]),
            ("phase_deg", "Phase deg"),
            ("harmonic_1", "Harmonic 1"),
            ("harmonic_2", "Harmonic 2"),
            ("dynamic_reserve", "Dynamic reserve", OE_CHOICES["dynamic_reserve"]),
            ("sensitivity_index", "Sensitivity index", OE_CHOICES["sensitivity_index"]),
            ("time_constant_index", "Time constant index", OE_CHOICES["time_constant_index"]),
            ("filter_slope", "Filter slope", OE_CHOICES["filter_slope"]),
            ("sync_filter", "Sync filter", OE_CHOICES["sync_filter"]),
            ("sine_output_mode", "Sine output mode", OE_CHOICES["sine_output_mode"]),
            ("sine_output_voltage_vrms", "Sine output voltage Vrms"),
        ]
        self._form_grid(parent, self.oe_vars, oe_fields, start_row=1, columns=1)
        ttk.Label(
            parent,
            text="Collector is locked: frame_exact_bytes=12288 and rall_post_write_delay_ms=30. This GUI does not edit the RALL hot path.",
            wraplength=840,
            foreground="#555555",
        ).grid(row=len(oe_fields) + 2, column=0, columnspan=2, sticky="w", pady=14)

    def _build_laser_page(self, parent: ttk.Frame) -> None:
        self.laser_vars = {
            "profile_id": tk.StringVar(),
            "mode": tk.StringVar(value="off_background"),
            "power_mw": tk.DoubleVar(value=0),
            "settle_ms": tk.DoubleVar(value=1000),
        }
        self._section_title(parent, "CNI laser run background profile", 0)
        self._form_field(parent, "Profile ID", self.laser_vars["profile_id"], 1, 0)
        self._form_field(parent, "Mode", self.laser_vars["mode"], 2, 0, choices=["off_background", "on_background"])
        self._form_field(parent, "Power", self.laser_vars["power_mw"], 3, 0, unit=(self.unit_vars["laser_power"], LASER_POWER_UNIT_CHOICES))
        self._form_field(parent, "Settle", self.laser_vars["settle_ms"], 4, 0, unit=(self.unit_vars["time"], TIME_UNIT_CHOICES))
        ttk.Label(
            parent,
            text="Laser is generated as a run-level background profile. The C# runtime opens/closes it at run boundaries, not per point.",
            wraplength=840,
        ).grid(row=5, column=0, columnspan=4, sticky="w", pady=14)

    def _build_generate_page(self, parent: ttk.Frame) -> None:
        self._section_title(parent, "Generate JSON files", 0)
        ttk.Button(parent, text="Generate plan + profiles", command=self._generate).grid(row=1, column=0, sticky="w", pady=(0, 12))
        self.summary = tk.Text(parent, height=22, wrap="word", font=("Menlo", 11))
        self.summary.grid(row=2, column=0, columnspan=4, sticky="nsew")
        parent.rowconfigure(2, weight=1)
        parent.columnconfigure(0, weight=1)

    def _section_title(self, parent: ttk.Frame, text: str, row: int) -> None:
        ttk.Label(parent, text=text, font=("", 12, "bold")).grid(row=row, column=0, columnspan=4, sticky="w", pady=(0, 10))

    def _path_row(self, parent: ttk.Frame, label: str, variable: tk.StringVar, row: int, is_directory: bool) -> None:
        ttk.Label(parent, text=label, width=28).grid(row=row, column=0, sticky="w", pady=5)
        ttk.Entry(parent, textvariable=variable).grid(row=row, column=1, sticky="ew", pady=5)
        ttk.Button(parent, text="Browse", command=lambda: self._browse_path(variable, is_directory)).grid(row=row, column=2, padx=(8, 0), pady=5)
        parent.columnconfigure(1, weight=1)

    def _form_grid(
        self,
        parent: ttk.Frame,
        values: dict[str, tk.Variable],
        fields: list[tuple[str, str] | tuple[str, str, list[str]]],
        start_row: int,
        columns: int = 2,
        units: dict[str, tuple[tk.StringVar, list[str]] | str] | None = None,
    ) -> None:
        for index, field in enumerate(fields):
            key = field[0]
            label = field[1]
            choices = field[2] if len(field) > 2 else None
            row = start_row + index // columns
            col = (index % columns) * 2
            self._form_field(parent, label, values[key], row, col, choices=choices, unit=units.get(key) if units else None)

    def _form_field(
        self,
        parent: ttk.Frame,
        label: str,
        variable: tk.Variable,
        row: int,
        column: int,
        choices: list[str] | None = None,
        unit: tuple[tk.StringVar, list[str]] | str | None = None,
    ) -> None:
        ttk.Label(parent, text=label, wraplength=180).grid(row=row, column=column, sticky="w", padx=(0, 8), pady=4)
        if isinstance(variable, tk.BooleanVar):
            ttk.Checkbutton(parent, variable=variable).grid(row=row, column=column + 1, sticky="w", pady=4)
        elif choices:
            ttk.Combobox(parent, textvariable=variable, values=choices, state="readonly").grid(row=row, column=column + 1, sticky="ew", pady=4)
        elif unit:
            value_frame = ttk.Frame(parent)
            value_frame.grid(row=row, column=column + 1, sticky="ew", pady=4)
            value_frame.columnconfigure(0, weight=1)
            ttk.Entry(value_frame, textvariable=variable).grid(row=0, column=0, sticky="ew")
            if isinstance(unit, str):
                ttk.Label(value_frame, text=unit, width=max(4, len(unit))).grid(row=0, column=1, sticky="e", padx=(6, 0))
            else:
                unit_var, unit_choices = unit
                ttk.Combobox(value_frame, textvariable=unit_var, values=unit_choices, state="readonly", width=7).grid(row=0, column=1, sticky="e", padx=(6, 0))
        else:
            ttk.Entry(parent, textvariable=variable).grid(row=row, column=column + 1, sticky="ew", pady=4)
        parent.columnconfigure(column + 1, weight=1)

    def _axis_panel(self, parent: ttk.Frame, axis: str, column: int) -> None:
        group = ttk.LabelFrame(parent, text=f"{axis.upper()} axis")
        group.grid(row=0, column=column, sticky="new", padx=4)
        group.columnconfigure(1, weight=1)
        vars_for_axis: dict[str, tk.Variable] = {
            "enabled": tk.BooleanVar(),
            "mode": tk.StringVar(value="range"),
            "fixed": tk.DoubleVar(value=0.0),
            "start": tk.DoubleVar(value=0.0),
            "stop": tk.DoubleVar(value=0.0),
            "step": tk.DoubleVar(value=10.0),
            "values_text": tk.StringVar(value="0"),
        }
        self.axis_vars[axis] = vars_for_axis
        self._form_field(group, "Active", vars_for_axis["enabled"], 0, 0)
        self._form_field(group, "Mode", vars_for_axis["mode"], 1, 0, choices=["range", "list"])
        field_unit = (self.unit_vars["field"], FIELD_UNIT_CHOICES)
        self._form_field(group, "Fixed", vars_for_axis["fixed"], 2, 0, unit=field_unit)
        self._form_field(group, "Start", vars_for_axis["start"], 3, 0, unit=field_unit)
        self._form_field(group, "Stop", vars_for_axis["stop"], 4, 0, unit=field_unit)
        self._form_field(group, "Step", vars_for_axis["step"], 5, 0, unit=field_unit)
        self._form_field(group, "Explicit list", vars_for_axis["values_text"], 6, 0, unit=field_unit)

    def _load_default_paths(self) -> None:
        self.template_vars["plan"].set(str(self.repo_root / "configs" / "plans" / "x_axis_1d_bounce_15min.json"))
        self.template_vars["smb"].set(str(self.repo_root / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json"))
        self.template_vars["oe"].set(str(self.repo_root / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"))
        self.template_vars["laser"].set(str(self.repo_root / "configs" / "profiles" / "cni_laser_run_off_background.json"))
        self.template_vars["output"].set(str(self.repo_root / "configs" / "generated"))

    def _load_template_values(self) -> None:
        try:
            plan = load_json(self.template_vars["plan"].get())
            smb = load_json(self.template_vars["smb"].get())
            oe = load_json(self.template_vars["oe"].get())
            laser = load_json(self.template_vars["laser"].get())
            self._set_run_values(plan)
            self._set_plan_policy_values(plan)
            self._set_smb_values(smb)
            self._set_oe_values(oe)
            self._set_laser_values(laser)
        except Exception as exc:
            messagebox.showerror("Load templates failed", str(exc), parent=self)

    def _install_unit_traces(self) -> None:
        for key, variable in self.unit_vars.items():
            variable.trace_add("write", lambda *_args, unit_key=key: self._on_unit_changed(unit_key))

    def _on_unit_changed(self, key: str) -> None:
        old_unit = self._unit_last_values.get(key)
        new_unit = self.unit_vars[key].get()
        if not old_unit or old_unit == new_unit:
            return
        try:
            if key == "field":
                self._convert_field_unit(old_unit, new_unit)
            elif key == "baseline_current":
                self._convert_variables([
                    self.baseline_vars["baseline_x_a"],
                    self.baseline_vars["baseline_y_a"],
                    self.baseline_vars["baseline_z_a"],
                    self.baseline_vars["settle_tolerance_a"],
                ], "current_a", old_unit, new_unit)
            elif key == "voltage":
                self._convert_variables([
                    self.baseline_vars["voltage_v"],
                    self.baseline_vars["voltage_protection_v"],
                ], "voltage_v", old_unit, new_unit)
            elif key == "time":
                self._convert_variables([
                    self.run_vars["acquisition_window_ms"],
                    self.run_vars["point_settle_ms"],
                    self.baseline_vars["settle_ms"],
                    self.quality_vars["max_last_frame_age_ms"],
                    self.smb_vars["command_settle_ms"],
                    self.smb_vars["dwell_ms"],
                    self.oe_vars["command_settle_ms"],
                    self.laser_vars["settle_ms"],
                ], "time_ms", old_unit, new_unit)
            elif key == "rf_frequency":
                self._convert_variables([self.smb_vars["start_hz"], self.smb_vars["stop_hz"]], "frequency_hz", old_unit, new_unit)
            elif key == "rf_step":
                self._convert_variables([self.smb_vars["step_hz"]], "frequency_hz", old_unit, new_unit)
            elif key == "fm_deviation":
                self._convert_variables([self.smb_vars["fm_deviation_hz"]], "frequency_hz", old_unit, new_unit)
            elif key == "lf_frequency":
                self._convert_variables([self.smb_vars["lf_frequency_hz"]], "frequency_hz", old_unit, new_unit)
            elif key == "lf_voltage":
                self._convert_variables([self.smb_vars["lf_voltage_mv"]], "voltage_mv", old_unit, new_unit)
            elif key == "laser_power":
                self._convert_variables([self.laser_vars["power_mw"]], "power_mw", old_unit, new_unit)
        finally:
            self._unit_last_values[key] = new_unit

    def _convert_variables(self, variables: list[tk.Variable], unit_kind: str, old_unit: str, new_unit: str) -> None:
        for variable in variables:
            try:
                variable.set(from_canonical_unit(to_canonical_unit(variable.get(), unit_kind, old_unit), unit_kind, new_unit))
            except Exception:
                continue

    def _convert_field_unit(self, old_unit: str, new_unit: str) -> None:
        selected = self._selected_block_index()
        if selected is not None:
            try:
                self.blocks[selected] = self._read_block()
            except Exception:
                selected = None
        for block in self.blocks:
            for spec in block.axes.values():
                spec.fixed = self._convert_number(spec.fixed, "field", old_unit, new_unit)
                spec.start = self._convert_number(spec.start, "field", old_unit, new_unit)
                spec.stop = self._convert_number(spec.stop, "field", old_unit, new_unit)
                spec.step = self._convert_number(spec.step, "field", old_unit, new_unit)
                try:
                    spec.values_text = ", ".join(
                        format_number(self._convert_number(value, "field", old_unit, new_unit))
                        for value in parse_values(spec.values_text)
                    )
                except Exception:
                    pass
        if selected is not None:
            self._load_block(selected)

    def _convert_number(self, value: object, unit_kind: str, old_unit: str, new_unit: str) -> float:
        return from_canonical_unit(to_canonical_unit(value, unit_kind, old_unit), unit_kind, new_unit)

    def _set_run_values(self, plan: dict) -> None:
        time_unit = self.unit_vars["time"].get()
        self.run_vars["run_id"].set(f"{plan.get('run_id', 'generated_plan')}_generated")
        self.run_vars["operator"].set(plan.get("operator", "local"))
        self.run_vars["acquisition_window_ms"].set(from_canonical_unit(plan.get("acquisition_window_ms", 0), "time_ms", time_unit))
        self.run_vars["point_settle_ms"].set(from_canonical_unit(plan.get("point_settle_ms", 500), "time_ms", time_unit))

    def _set_plan_policy_values(self, plan: dict) -> None:
        baseline = plan.get("mag_baseline_policy", {})
        currents = baseline.get("baseline_current_a", [0.0, 0.0, 0.0])
        current_unit = self.unit_vars["baseline_current"].get()
        voltage_unit = self.unit_vars["voltage"].get()
        time_unit = self.unit_vars["time"].get()
        for key, value in {
            "baseline_x_a": from_canonical_unit(currents[0] if len(currents) > 0 else 0.0, "current_a", current_unit),
            "baseline_y_a": from_canonical_unit(currents[1] if len(currents) > 1 else 0.0, "current_a", current_unit),
            "baseline_z_a": from_canonical_unit(currents[2] if len(currents) > 2 else 0.0, "current_a", current_unit),
            "settle_ms": from_canonical_unit(baseline.get("settle_ms", 1000), "time_ms", time_unit),
            "readback_samples": baseline.get("readback_samples", 3),
            "settle_tolerance_a": from_canonical_unit(baseline.get("settle_tolerance_a", 0.002), "current_a", current_unit),
            "voltage_v": from_canonical_unit(baseline.get("voltage_v", 75.0), "voltage_v", voltage_unit),
            "voltage_protection_v": from_canonical_unit(baseline.get("voltage_protection_v", 75.0), "voltage_v", voltage_unit),
            "output_enabled": baseline.get("output_enabled", True),
        }.items():
            self.baseline_vars[key].set(value)
        quality = plan.get("quality_thresholds", {})
        for key, value in {
            "min_frames": quality.get("min_frames", 20),
            "max_timeout_count": quality.get("max_timeout_count", 2),
            "max_duplicate_ratio": quality.get("max_duplicate_ratio", 0.3),
            "max_last_frame_age_ms": from_canonical_unit(quality.get("max_last_frame_age_ms", 500), "time_ms", time_unit),
        }.items():
            self.quality_vars[key].set(value)

    def _set_smb_values(self, smb: dict) -> None:
        fixed = smb.get("fixed", {})
        sweep = smb.get("default_sweep", {})
        rf_unit = self.unit_vars["rf_frequency"].get()
        rf_step_unit = self.unit_vars["rf_step"].get()
        fm_unit = self.unit_vars["fm_deviation"].get()
        lf_frequency_unit = self.unit_vars["lf_frequency"].get()
        lf_voltage_unit = self.unit_vars["lf_voltage"].get()
        time_unit = self.unit_vars["time"].get()
        self.smb_vars["profile_id"].set(f"{smb.get('profile_id', 'smb100a')}_generated")
        self.smb_vars["command_settle_ms"].set(from_canonical_unit(smb.get("command_settle_ms", 500), "time_ms", time_unit))
        self.smb_vars["error_check_after_write"].set(smb.get("error_check_after_write", True))
        for key in ["modulation_enabled", "fm_enabled", "fm_source", "fm_mode", "lf_output_enabled", "lf_shape", "lf_source_impedance"]:
            self.smb_vars[key].set(fixed.get(key, self.smb_vars[key].get()))
        self.smb_vars["fm_deviation_hz"].set(from_canonical_unit(fixed.get("fm_deviation_hz", 4000000.0), "frequency_hz", fm_unit))
        self.smb_vars["lf_voltage_mv"].set(from_canonical_unit(fixed.get("lf_voltage_mv", 137.0), "voltage_mv", lf_voltage_unit))
        self.smb_vars["lf_frequency_hz"].set(from_canonical_unit(fixed.get("lf_frequency_hz", 500.0), "frequency_hz", lf_frequency_unit))
        for key in ["power_dbm", "sweep_mode", "spacing", "shape", "trigger_source", "output_voltage_start_v", "output_voltage_stop_v", "rf_output_enabled"]:
            self.smb_vars[key].set(sweep.get(key, self.smb_vars[key].get()))
        self.smb_vars["dwell_ms"].set(from_canonical_unit(sweep.get("dwell_ms", 300), "time_ms", time_unit))
        self.smb_vars["start_hz"].set(from_canonical_unit(sweep.get("start_hz", 2830000000.0), "frequency_hz", rf_unit))
        self.smb_vars["stop_hz"].set(from_canonical_unit(sweep.get("stop_hz", 2890000000.0), "frequency_hz", rf_unit))
        self.smb_vars["step_hz"].set(from_canonical_unit(sweep.get("step_hz", 500000.0), "frequency_hz", rf_step_unit))

    def _set_oe_values(self, oe: dict) -> None:
        fixed = oe.get("fixed", {})
        self.oe_vars["profile_id"].set(f"{oe.get('profile_id', 'oe1022d')}_generated")
        self.oe_vars["command_settle_ms"].set(from_canonical_unit(oe.get("command_settle_ms", 500), "time_ms", self.unit_vars["time"].get()))
        for key in [key for key in self.oe_vars if key not in {"profile_id", "command_settle_ms"}]:
            value = fixed.get(key, self.oe_vars[key].get())
            self.oe_vars[key].set(oe_choice_for_code(key, value) if key in OE_CHOICE_KEYS else value)

    def _set_laser_values(self, laser: dict) -> None:
        self.laser_vars["profile_id"].set(f"{laser.get('profile_id', 'cni_laser')}_generated")
        self.laser_vars["mode"].set(laser.get("mode", "off_background"))
        self.laser_vars["power_mw"].set(from_canonical_unit(laser.get("power_mw", 0), "power_mw", self.unit_vars["laser_power"].get()))
        self.laser_vars["settle_ms"].set(from_canonical_unit(laser.get("settle_ms", 1000), "time_ms", self.unit_vars["time"].get()))

    def _browse_path(self, variable: tk.StringVar, is_directory: bool) -> None:
        if is_directory:
            path = filedialog.askdirectory(parent=self, initialdir=variable.get() or str(self.repo_root))
        else:
            path = filedialog.askopenfilename(
                parent=self,
                initialdir=str(self.repo_root / "configs"),
                filetypes=[("JSON files", "*.json"), ("All files", "*.*")],
            )
        if path:
            variable.set(path)

    def _refresh_block_list(self) -> None:
        self.block_list.delete(0, tk.END)
        for block in self.blocks:
            active = "".join(axis.upper() for axis, spec in block.axes.items() if spec.enabled) or "fixed"
            self.block_list.insert(tk.END, f"{block.prefix} [{active}] {block.traversal}")
        if self.blocks:
            self.block_list.selection_set(0)

    def _on_block_select(self, _event: tk.Event) -> None:
        index = self._selected_block_index()
        if index is not None:
            self._load_block(index)

    def _load_block(self, index: int) -> None:
        block = self.blocks[index]
        self.block_prefix.set(block.prefix)
        self.block_traversal.set(block.traversal)
        self.block_total_points.set(block.total_points)
        for axis, spec in block.axes.items():
            values = self.axis_vars[axis]
            values["enabled"].set(spec.enabled)
            values["mode"].set(spec.mode)
            values["fixed"].set(spec.fixed)
            values["start"].set(spec.start)
            values["stop"].set(spec.stop)
            values["step"].set(spec.step)
            values["values_text"].set(spec.values_text)

    def _read_block(self) -> ScanBlock:
        return ScanBlock(
            prefix=self.block_prefix.get(),
            traversal=self.block_traversal.get(),
            total_points=int(self.block_total_points.get()),
            axes={axis: self._read_axis(axis) for axis in ("x", "y", "z")},
        )

    def _read_axis(self, axis: str) -> AxisSpec:
        values = self.axis_vars[axis]
        return AxisSpec(
            enabled=bool(values["enabled"].get()),
            mode=str(values["mode"].get()),
            fixed=float(values["fixed"].get()),
            start=float(values["start"].get()),
            stop=float(values["stop"].get()),
            step=float(values["step"].get()),
            values_text=str(values["values_text"].get()),
        )

    def _canonical_blocks(self) -> list[ScanBlock]:
        field_unit = self.unit_vars["field"].get()
        blocks: list[ScanBlock] = []
        for block in self.blocks:
            blocks.append(ScanBlock(
                prefix=block.prefix,
                traversal=block.traversal,
                total_points=block.total_points,
                axes={axis: self._canonical_axis(spec, field_unit) for axis, spec in block.axes.items()},
            ))
        return blocks

    def _canonical_axis(self, spec: AxisSpec, field_unit: str) -> AxisSpec:
        values_text = spec.values_text
        if spec.mode == "list":
            values_text = ", ".join(format_number(to_canonical_unit(value, "field", field_unit)) for value in parse_values(spec.values_text))
        return AxisSpec(
            enabled=spec.enabled,
            mode=spec.mode,
            fixed=to_canonical_unit(spec.fixed, "field", field_unit),
            start=to_canonical_unit(spec.start, "field", field_unit),
            stop=to_canonical_unit(spec.stop, "field", field_unit),
            step=to_canonical_unit(spec.step, "field", field_unit),
            values_text=values_text,
        )

    def _canonical_baseline_policy(self) -> dict[str, object]:
        current_unit = self.unit_vars["baseline_current"].get()
        voltage_unit = self.unit_vars["voltage"].get()
        return {
            "baseline_x_a": to_canonical_unit(self.baseline_vars["baseline_x_a"].get(), "current_a", current_unit),
            "baseline_y_a": to_canonical_unit(self.baseline_vars["baseline_y_a"].get(), "current_a", current_unit),
            "baseline_z_a": to_canonical_unit(self.baseline_vars["baseline_z_a"].get(), "current_a", current_unit),
            "settle_ms": self._time_ms(self.baseline_vars["settle_ms"].get()),
            "readback_samples": self.baseline_vars["readback_samples"].get(),
            "settle_tolerance_a": to_canonical_unit(self.baseline_vars["settle_tolerance_a"].get(), "current_a", current_unit),
            "voltage_v": to_canonical_unit(self.baseline_vars["voltage_v"].get(), "voltage_v", voltage_unit),
            "voltage_protection_v": to_canonical_unit(self.baseline_vars["voltage_protection_v"].get(), "voltage_v", voltage_unit),
            "output_enabled": self.baseline_vars["output_enabled"].get(),
        }

    def _canonical_smb_fixed(self) -> dict[str, object]:
        return {
            "modulation_enabled": self.smb_vars["modulation_enabled"].get(),
            "fm_enabled": self.smb_vars["fm_enabled"].get(),
            "fm_source": self.smb_vars["fm_source"].get(),
            "fm_mode": self.smb_vars["fm_mode"].get(),
            "fm_deviation_hz": to_canonical_unit(self.smb_vars["fm_deviation_hz"].get(), "frequency_hz", self.unit_vars["fm_deviation"].get()),
            "lf_output_enabled": self.smb_vars["lf_output_enabled"].get(),
            "lf_voltage_mv": to_canonical_unit(self.smb_vars["lf_voltage_mv"].get(), "voltage_mv", self.unit_vars["lf_voltage"].get()),
            "lf_frequency_hz": to_canonical_unit(self.smb_vars["lf_frequency_hz"].get(), "frequency_hz", self.unit_vars["lf_frequency"].get()),
            "lf_shape": self.smb_vars["lf_shape"].get(),
            "lf_source_impedance": self.smb_vars["lf_source_impedance"].get(),
        }

    def _canonical_smb_sweep(self) -> dict[str, object]:
        return {
            "start_hz": to_canonical_unit(self.smb_vars["start_hz"].get(), "frequency_hz", self.unit_vars["rf_frequency"].get()),
            "stop_hz": to_canonical_unit(self.smb_vars["stop_hz"].get(), "frequency_hz", self.unit_vars["rf_frequency"].get()),
            "step_hz": to_canonical_unit(self.smb_vars["step_hz"].get(), "frequency_hz", self.unit_vars["rf_step"].get()),
            "dwell_ms": self._time_ms(self.smb_vars["dwell_ms"].get()),
            "power_dbm": self.smb_vars["power_dbm"].get(),
            "sweep_mode": self.smb_vars["sweep_mode"].get(),
            "spacing": self.smb_vars["spacing"].get(),
            "shape": self.smb_vars["shape"].get(),
            "trigger_source": self.smb_vars["trigger_source"].get(),
            "output_voltage_start_v": self.smb_vars["output_voltage_start_v"].get(),
            "output_voltage_stop_v": self.smb_vars["output_voltage_stop_v"].get(),
            "rf_output_enabled": self.smb_vars["rf_output_enabled"].get(),
        }

    def _time_ms(self, value: object) -> int:
        return int(round(to_canonical_unit(value, "time_ms", self.unit_vars["time"].get())))

    def _add_or_update_block(self) -> None:
        block = self._read_block()
        index = self._selected_block_index()
        if index is None:
            self.blocks.append(block)
        else:
            self.blocks[index] = block
        self._refresh_block_list()

    def _remove_block(self) -> None:
        index = self._selected_block_index()
        if index is None:
            return
        del self.blocks[index]
        if not self.blocks:
            self.blocks.append(self.default_block("x_line", "x"))
        self._refresh_block_list()
        self._load_block(min(index, len(self.blocks) - 1))

    def _add_xyz_blocks(self) -> None:
        self.blocks.extend([
            self.default_block("x_line", "x"),
            self.default_block("y_line", "y"),
            self.default_block("z_line", "z"),
        ])
        self._refresh_block_list()

    def _selected_block_index(self) -> int | None:
        selection = self.block_list.curselection()
        return int(selection[0]) if selection else None

    def _request(self) -> GeneratorRequest:
        return GeneratorRequest(
            run_id=str(self.run_vars["run_id"].get()),
            operator=str(self.run_vars["operator"].get()),
            acquisition_window_ms=self._time_ms(self.run_vars["acquisition_window_ms"].get()),
            point_settle_ms=self._time_ms(self.run_vars["point_settle_ms"].get()),
            blocks=self._canonical_blocks(),
            mag_baseline_policy=self._canonical_baseline_policy(),
            quality_thresholds={
                "min_frames": self.quality_vars["min_frames"].get(),
                "max_timeout_count": self.quality_vars["max_timeout_count"].get(),
                "max_duplicate_ratio": self.quality_vars["max_duplicate_ratio"].get(),
                "max_last_frame_age_ms": self._time_ms(self.quality_vars["max_last_frame_age_ms"].get()),
            },
            smb_profile_id=str(self.smb_vars["profile_id"].get()),
            smb_command_settle_ms=self._time_ms(self.smb_vars["command_settle_ms"].get()),
            smb_error_check_after_write=bool(self.smb_vars["error_check_after_write"].get()),
            smb_fixed=self._canonical_smb_fixed(),
            smb_sweep=self._canonical_smb_sweep(),
            oe_profile_id=str(self.oe_vars["profile_id"].get()),
            oe_command_settle_ms=self._time_ms(self.oe_vars["command_settle_ms"].get()),
            oe_fixed={
                key: choice_code(self.oe_vars[key].get()) if key in OE_CHOICE_KEYS else self.oe_vars[key].get()
                for key in self.oe_vars
                if key not in {"profile_id", "command_settle_ms"}
            },
            laser_profile_id=str(self.laser_vars["profile_id"].get()),
            laser_mode=str(self.laser_vars["mode"].get()),
            laser_power_mw=int(round(to_canonical_unit(self.laser_vars["power_mw"].get(), "power_mw", self.unit_vars["laser_power"].get()))),
            laser_settle_ms=self._time_ms(self.laser_vars["settle_ms"].get()),
        )

    def _generate(self) -> None:
        try:
            self._add_or_update_block()
            request = self._request()
            paths = write_generated_bundle(
                self.repo_root,
                request,
                self.template_vars["plan"].get(),
                self.template_vars["smb"].get(),
                self.template_vars["oe"].get(),
                self.template_vars["laser"].get(),
                self.template_vars["output"].get(),
            )
            self.summary.delete("1.0", tk.END)
            self.summary.insert(tk.END, "Generated JSON files for C# Run Bundle:\n\n")
            for key, path in paths.items():
                self.summary.insert(tk.END, f"{key}: {path}\n")
            self.summary.insert(tk.END, "\nNext step:\nOpen the Windows C# Control Panel, select these generated JSON files in Run Bundle, validate, then run.\n")
            self._show_step(6)
        except Exception as exc:
            messagebox.showerror("Generate failed", str(exc), parent=self)

    def _select_step_from_list(self, _event: tk.Event) -> None:
        selection = self.step_list.curselection()
        if selection:
            self._show_step(int(selection[0]))

    def _move_step(self, delta: int) -> None:
        next_index = max(0, min(len(STEP_TITLES) - 1, self.current_step + delta))
        self._show_step(next_index)

    def _show_step(self, index: int) -> None:
        self.current_step = max(0, min(len(STEP_TITLES) - 1, index))
        self.pages[self.current_step].tkraise()
        self.step_list.selection_clear(0, tk.END)
        self.step_list.selection_set(self.current_step)
        self.step_list.see(self.current_step)
        self.after_idle(self._refresh_selected_page)
        self.after(30, self._refresh_selected_page)

    def _refresh_selected_page(self) -> None:
        page = self.pages[self.current_step]
        page.update_idletasks()
        page.canvas.itemconfigure(page.window_id, width=max(1, page.canvas.winfo_width()))
        page.canvas.configure(scrollregion=page.canvas.bbox("all"))
        page.canvas.yview_moveto(0)

    @staticmethod
    def default_block(prefix: str, active_axis: str) -> ScanBlock:
        return ScanBlock(
            prefix=prefix,
            traversal="bounce_1d_x" if active_axis == "x" else "raster",
            axes={
                "x": AxisSpec(enabled=active_axis == "x", start=0.0, stop=40.0 if active_axis == "x" else 0.0, step=10.0, fixed=0.0, values_text="0, 10, 20, 30, 40"),
                "y": AxisSpec(enabled=active_axis == "y", start=0.0, stop=40.0 if active_axis == "y" else 0.0, step=10.0, fixed=0.0, values_text="0, 10, 20, 30, 40"),
                "z": AxisSpec(enabled=active_axis == "z", start=0.0, stop=40.0 if active_axis == "z" else 0.0, step=10.0, fixed=0.0, values_text="0, 10, 20, 30, 40"),
            },
        )


def main() -> int:
    app = ConfigGeneratorApp()
    app.mainloop()
    return 0


if __name__ == "__main__":
    sys.exit(main())
