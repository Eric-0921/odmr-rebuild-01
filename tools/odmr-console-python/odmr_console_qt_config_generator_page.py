from __future__ import annotations

from dataclasses import asdict
import json

from PySide6.QtCore import Qt, Signal
from PySide6.QtWidgets import (
    QCheckBox,
    QComboBox,
    QFormLayout,
    QGridLayout,
    QGroupBox,
    QHBoxLayout,
    QLabel,
    QLineEdit,
    QListWidget,
    QListWidgetItem,
    QMessageBox,
    QPlainTextEdit,
    QPushButton,
    QStackedWidget,
    QTabWidget,
    QVBoxLayout,
    QWidget,
)

from odmr_console_core import generate_config_bundle, load_json
from odmr_console_qt_shared import (
    AxisSpec,
    CURRENT_UNITS,
    FREQUENCY_UNITS,
    LASER_MODE_LABEL_BY_TOKEN,
    LASER_MODE_TOKEN_BY_LABEL,
    LASER_POWER_UNITS,
    LF_VOLTAGE_UNITS,
    LOCKIN_MODEL_CHOICES,
    LOW_FREQUENCY_UNITS,
    MAG_AXIS_MODE_CHOICES,
    MAG_TRAVERSAL_CHOICES,
    OE_CHOICES,
    OE_CHOICE_KEYS,
    PLAN_KIND_CHOICES,
    REPO_ROOT,
    SMB_FM_MODE_CHOICES,
    SMB_FM_SOURCE_CHOICES,
    SMB_LF_IMPEDANCE_CHOICES,
    SMB_LF_SHAPE_CHOICES,
    SMB_SWEEP_MODE_CHOICES,
    SMB_SWEEP_SHAPE_CHOICES,
    SMB_SWEEP_SPACING_CHOICES,
    SMB_TRIGGER_SOURCE_CHOICES,
    ScanBlock,
    TIME_UNITS,
    VOLTAGE_UNITS,
    add_form_row,
    choice_code,
    choice_token,
    combo,
    expand_block,
    find_choice_by_code,
    find_choice_by_token,
    format_number,
    note_label,
    parse_values,
    scroll_page,
    section_label,
    token_choice,
    to_canonical_unit,
    GeneratorRequest,
    NumberUnitInput,
    PathSelector,
)


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
            ("锁相放大器", self.oe_page),
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
        layout.addWidget(note_label("锁相配置沿用同一个 oe_profile 文件名，但内容必须显式带 model，并和 station 内 active lock-in 一致。"))
        header = QGroupBox("通用信息")
        header_grid = QGridLayout(header)
        self.oe_model = combo(LOCKIN_MODEL_CHOICES, "oe1022d")
        self.oe_profile_id = QLineEdit()
        self.oe_command_settle = NumberUnitInput(500, TIME_UNITS, "ms")
        add_form_row(header_grid, 0, 0, "型号", self.oe_model)
        add_form_row(header_grid, 0, 1, "配置 ID", self.oe_profile_id)
        add_form_row(header_grid, 1, 0, "命令等待时间", self.oe_command_settle)
        layout.addWidget(header)

        self.oe_stack = QStackedWidget()

        oe1022d_page = QWidget()
        oe1022d_layout = QVBoxLayout(oe1022d_page)
        oe1022d_layout.addWidget(note_label("OE1022D collector 固定为 12288B exact read + 30ms delay。"))
        group = QGroupBox("OE1022D 固定配置")
        grid = QGridLayout(group)
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
        oe1022d_layout.addWidget(group)

        oe1300_page = QWidget()
        oe1300_layout = QVBoxLayout(oe1300_page)
        oe1300_layout.addWidget(note_label("OE1300 collector 固定为 TCP `RALL?\\r` -> 32768B -> 37 x 100 big-endian double。"))
        fixed_group = QGroupBox("OE1300 固定配置")
        fixed_grid = QGridLayout(fixed_group)
        self.oe1300_fixed_fields: dict[str, QWidget] = {
            "input_source": QLineEdit("0"),
            "input_coupling": QLineEdit("0"),
            "input_range": QLineEdit("0"),
            "reference_source": QLineEdit("0"),
            "reference_frequency_hz": NumberUnitInput(1000, LOW_FREQUENCY_UNITS, "Hz"),
            "reference_slope": QLineEdit("0"),
            "sensitivity_index": QLineEdit("24"),
            "time_constant_seconds": NumberUnitInput(0.1, TIME_UNITS, "s"),
            "filter_slope": QLineEdit("1"),
            "sync_enabled": QCheckBox("enabled"),
            "sine_output_enabled": QCheckBox("enabled"),
            "sine_output_voltage_vrms": NumberUnitInput(1.0, None, "Vrms"),
        }
        oe1300_rows = [
            ("输入源", self.oe1300_fixed_fields["input_source"]),
            ("输入耦合", self.oe1300_fixed_fields["input_coupling"]),
            ("输入量程", self.oe1300_fixed_fields["input_range"]),
            ("参考源", self.oe1300_fixed_fields["reference_source"]),
            ("参考频率", self.oe1300_fixed_fields["reference_frequency_hz"]),
            ("参考斜率", self.oe1300_fixed_fields["reference_slope"]),
            ("灵敏度索引", self.oe1300_fixed_fields["sensitivity_index"]),
            ("时间常数", self.oe1300_fixed_fields["time_constant_seconds"]),
            ("滤波器陡降", self.oe1300_fixed_fields["filter_slope"]),
            ("同步滤波", self.oe1300_fixed_fields["sync_enabled"]),
            ("正弦输出使能", self.oe1300_fixed_fields["sine_output_enabled"]),
            ("正弦输出幅值", self.oe1300_fixed_fields["sine_output_voltage_vrms"]),
        ]
        for index, (label, widget) in enumerate(oe1300_rows):
            add_form_row(fixed_grid, index // 2, index % 2, label, widget)
        oe1300_layout.addWidget(fixed_group)

        collector_group = QGroupBox("OE1300 Collector 合同")
        collector_grid = QGridLayout(collector_group)
        self.oe1300_collector_fields: dict[str, QWidget] = {
            "tcp_expected_bytes": QLineEdit("32768"),
            "tcp_payload_bytes": QLineEdit("29600"),
            "parameter_count": QLineEdit("37"),
            "samples_per_parameter": QLineEdit("100"),
            "rall_post_write_delay_ms": NumberUnitInput(5, TIME_UNITS, "ms"),
            "drain_before_write": QCheckBox("enabled"),
        }
        self.oe1300_collector_fields["drain_before_write"].setChecked(True)
        oe1300_collector_rows = [
            ("TCP 总字节数", self.oe1300_collector_fields["tcp_expected_bytes"]),
            ("主采样区字节数", self.oe1300_collector_fields["tcp_payload_bytes"]),
            ("参数数", self.oe1300_collector_fields["parameter_count"]),
            ("每参数样本数", self.oe1300_collector_fields["samples_per_parameter"]),
            ("RALL 写后等待", self.oe1300_collector_fields["rall_post_write_delay_ms"]),
            ("写前 drain socket", self.oe1300_collector_fields["drain_before_write"]),
        ]
        for index, (label, widget) in enumerate(oe1300_collector_rows):
            add_form_row(collector_grid, index // 2, index % 2, label, widget)
        oe1300_layout.addWidget(collector_group)

        self.oe_stack.addWidget(oe1022d_page)
        self.oe_stack.addWidget(oe1300_page)
        layout.addWidget(self.oe_stack)
        self.oe_model.currentTextChanged.connect(self._sync_oe_model_view)
        self._sync_oe_model_view(self.oe_model.currentText())
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

    def _sync_oe_model_view(self, model: str) -> None:
        self.oe_stack.setCurrentIndex(0 if model == "oe1022d" else 1)

    def _set_oe_values(self, oe: dict[str, Any]) -> None:
        model = str(oe.get("model", "oe1022d")).strip().lower()
        self.oe_model.setCurrentText(model)
        self._sync_oe_model_view(model)
        fixed = oe.get("fixed", {})
        collector = oe.get("collector", {})
        self.oe_profile_id.setText(f"{oe.get('profile_id', 'oe1022d')}_generated")
        self.oe_command_settle.set_canonical(oe.get("command_settle_ms", 500), "time_ms")
        if model == "oe1300":
            for key, widget in self.oe1300_fixed_fields.items():
                value = fixed.get(key)
                if value is None:
                    continue
                if isinstance(widget, QCheckBox):
                    widget.setChecked(bool(value))
                elif isinstance(widget, NumberUnitInput):
                    widget.set_text(value)
                elif isinstance(widget, QLineEdit):
                    widget.setText(str(value))
            for key, widget in self.oe1300_collector_fields.items():
                value = collector.get(key)
                if value is None:
                    continue
                if isinstance(widget, QCheckBox):
                    widget.setChecked(bool(value))
                elif isinstance(widget, NumberUnitInput):
                    widget.set_text(value)
                elif isinstance(widget, QLineEdit):
                    widget.setText(str(value))
            return

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
            oe_model=self.oe_model.currentText(),
            oe_profile_id=self.oe_profile_id.text().strip(),
            oe_command_settle_ms=int(round(self.oe_command_settle.canonical("time_ms"))),
            oe_fixed=(
                {
                    key: choice_code(widget.currentText()) if key in OE_CHOICE_KEYS and isinstance(widget, QComboBox)
                    else widget.value() if isinstance(widget, NumberUnitInput)
                    else int(widget.text()) if isinstance(widget, QLineEdit) and (key.startswith("harmonic") or key.endswith("_index") or key == "channel")
                    else widget.text()
                    for key, widget in self.oe_fields.items()
                }
                if self.oe_model.currentText() == "oe1022d"
                else {
                    key: widget.isChecked() if isinstance(widget, QCheckBox)
                    else widget.canonical("frequency_hz") if key == "reference_frequency_hz" and isinstance(widget, NumberUnitInput)
                    else widget.canonical("time_ms") / 1000.0 if key == "time_constant_seconds" and isinstance(widget, NumberUnitInput)
                    else widget.value() if isinstance(widget, NumberUnitInput)
                    else int(widget.text())
                    for key, widget in self.oe1300_fixed_fields.items()
                }
            ),
            oe_collector=(
                {}
                if self.oe_model.currentText() == "oe1022d"
                else {
                    key: widget.isChecked() if isinstance(widget, QCheckBox)
                    else int(round(widget.canonical("time_ms"))) if key == "rall_post_write_delay_ms" and isinstance(widget, NumberUnitInput)
                    else int(widget.text())
                    for key, widget in self.oe1300_collector_fields.items()
                }
            ),
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
