from __future__ import annotations

import sys
from pathlib import Path
import tkinter as tk
from tkinter import filedialog, messagebox, ttk

from odmr_config_core import (
    AxisSpec,
    GeneratorRequest,
    ScanBlock,
    load_json,
    write_generated_bundle,
)


def find_repo_root() -> Path:
    current = Path(__file__).resolve()
    for parent in [current.parent, *current.parents]:
        if (parent / "configs").is_dir() and (parent / "tools" / "win-csharp").is_dir():
            return parent
    raise RuntimeError("could not find repository root")


class ConfigGeneratorApp(tk.Tk):
    def __init__(self) -> None:
        super().__init__()
        self.repo_root = find_repo_root()
        self.title("ODMR Config Generator")
        self.geometry("1180x780")
        self.minsize(980, 680)
        self.blocks: list[ScanBlock] = [self.default_block("x_line", "x")]
        self.template_vars: dict[str, tk.StringVar] = {}
        self.axis_vars: dict[str, dict[str, tk.Variable]] = {}
        self._build_ui()
        self._load_defaults()
        self._refresh_block_list()
        self._load_block(0)
        self._load_profile_defaults()

    def _build_ui(self) -> None:
        root = ttk.Frame(self, padding=10)
        root.pack(fill=tk.BOTH, expand=True)
        root.columnconfigure(0, weight=1)
        root.columnconfigure(1, weight=1)
        root.rowconfigure(1, weight=1)

        self._build_template_frame(root)
        self._build_run_frame(root)
        self._build_scan_frame(root)
        self._build_profile_frame(root)
        self._build_output_frame(root)

    def _build_template_frame(self, parent: ttk.Frame) -> None:
        frame = ttk.LabelFrame(parent, text="Templates")
        frame.grid(row=0, column=0, columnspan=2, sticky="ew", pady=(0, 8))
        frame.columnconfigure(1, weight=1)
        frame.columnconfigure(4, weight=1)
        rows = [
            ("plan", "Plan template", 0, 0),
            ("smb", "SMB profile", 0, 3),
            ("oe", "OE profile", 1, 0),
            ("laser", "Laser profile", 1, 3),
        ]
        for key, label, row, col in rows:
            var = tk.StringVar()
            self.template_vars[key] = var
            ttk.Label(frame, text=label).grid(row=row, column=col, sticky="w", padx=(6, 4), pady=4)
            ttk.Entry(frame, textvariable=var).grid(row=row, column=col + 1, sticky="ew", pady=4)
            ttk.Button(frame, text="Browse", command=lambda k=key: self._browse_template(k)).grid(row=row, column=col + 2, padx=4, pady=4)
        ttk.Button(frame, text="Load template values", command=self._load_profile_defaults).grid(row=2, column=0, padx=6, pady=6, sticky="w")

    def _build_run_frame(self, parent: ttk.Frame) -> None:
        frame = ttk.LabelFrame(parent, text="Run / Plan")
        frame.grid(row=1, column=0, sticky="nsew", padx=(0, 8), pady=(0, 8))
        frame.columnconfigure(1, weight=1)
        self.run_id = tk.StringVar(value="generated_plan")
        self.operator = tk.StringVar(value="local")
        self.point_settle = tk.IntVar(value=500)
        self.acq_window = tk.IntVar(value=0)
        self._entry(frame, "Run ID", self.run_id, 0)
        self._entry(frame, "Operator", self.operator, 1)
        self._entry(frame, "Point settle ms", self.point_settle, 2)
        self._entry(frame, "Acq window ms", self.acq_window, 3)

    def _build_scan_frame(self, parent: ttk.Frame) -> None:
        frame = ttk.LabelFrame(parent, text="Magnetic Scan Blocks")
        frame.grid(row=2, column=0, sticky="nsew", padx=(0, 8))
        frame.columnconfigure(1, weight=1)
        parent.rowconfigure(2, weight=1)

        self.block_list = tk.Listbox(frame, height=7, exportselection=False)
        self.block_list.grid(row=0, column=0, rowspan=9, sticky="nsew", padx=6, pady=6)
        self.block_list.bind("<<ListboxSelect>>", self._on_block_select)

        self.block_prefix = tk.StringVar()
        self.block_traversal = tk.StringVar(value="raster")
        self.block_total_points = tk.IntVar(value=0)
        self._entry(frame, "Prefix", self.block_prefix, 0, column=1)
        ttk.Label(frame, text="Traversal").grid(row=1, column=1, sticky="w", padx=6, pady=3)
        ttk.Combobox(frame, textvariable=self.block_traversal, values=["raster", "bounce_1d_x"], state="readonly").grid(row=1, column=2, sticky="ew", padx=6, pady=3)
        self._entry(frame, "Total points", self.block_total_points, 2, column=1)

        axis_header = ttk.Frame(frame)
        axis_header.grid(row=3, column=1, columnspan=2, sticky="ew", padx=6, pady=(8, 2))
        for index, title in enumerate(["Axis", "Active", "Mode", "Fixed", "Start", "Stop", "Step", "List"]):
            ttk.Label(axis_header, text=title).grid(row=0, column=index, sticky="w", padx=2)

        for row, axis in enumerate(["x", "y", "z"], start=4):
            self._axis_row(frame, axis, row)

        button_row = ttk.Frame(frame)
        button_row.grid(row=8, column=1, columnspan=2, sticky="ew", padx=6, pady=6)
        ttk.Button(button_row, text="Add / Update block", command=self._add_or_update_block).pack(side=tk.LEFT, padx=(0, 6))
        ttk.Button(button_row, text="Remove", command=self._remove_block).pack(side=tk.LEFT, padx=(0, 6))
        ttk.Button(button_row, text="Add X/Y/Z single-axis", command=self._add_xyz_blocks).pack(side=tk.LEFT)

    def _build_profile_frame(self, parent: ttk.Frame) -> None:
        frame = ttk.LabelFrame(parent, text="Spectrum / Device Profiles")
        frame.grid(row=1, column=1, rowspan=2, sticky="nsew", pady=(0, 8))
        frame.columnconfigure(1, weight=1)
        frame.columnconfigure(3, weight=1)

        self.smb_profile_id = tk.StringVar()
        self.smb_start = tk.DoubleVar()
        self.smb_stop = tk.DoubleVar()
        self.smb_step = tk.DoubleVar()
        self.smb_dwell = tk.IntVar()
        self.smb_power = tk.DoubleVar()
        self.smb_rf_output = tk.BooleanVar(value=True)
        self.oe_profile_id = tk.StringVar()
        self.oe_time_constant = tk.IntVar()
        self.oe_filter_slope = tk.IntVar()
        self.laser_profile_id = tk.StringVar()
        self.laser_mode = tk.StringVar(value="off_background")
        self.laser_power = tk.IntVar(value=0)
        self.laser_settle = tk.IntVar(value=1000)

        ttk.Label(frame, text="SMB100A").grid(row=0, column=0, sticky="w", padx=6, pady=(6, 2))
        self._entry(frame, "SMB profile id", self.smb_profile_id, 1)
        self._entry(frame, "Start Hz", self.smb_start, 2)
        self._entry(frame, "Stop Hz", self.smb_stop, 3)
        self._entry(frame, "Step Hz", self.smb_step, 4)
        self._entry(frame, "Dwell ms", self.smb_dwell, 5)
        self._entry(frame, "Power dBm", self.smb_power, 6)
        ttk.Checkbutton(frame, text="RF output enabled", variable=self.smb_rf_output).grid(row=7, column=1, sticky="w", padx=6, pady=3)

        ttk.Label(frame, text="OE1022D").grid(row=0, column=2, sticky="w", padx=6, pady=(6, 2))
        self._entry(frame, "OE profile id", self.oe_profile_id, 1, column=2)
        self._entry(frame, "Time constant index", self.oe_time_constant, 2, column=2)
        self._entry(frame, "Filter slope", self.oe_filter_slope, 3, column=2)
        ttk.Label(frame, text="RALL 12288B / 30ms locked").grid(row=4, column=3, sticky="w", padx=6, pady=3)

        ttk.Label(frame, text="CNI Laser").grid(row=8, column=0, sticky="w", padx=6, pady=(14, 2))
        self._entry(frame, "Laser profile id", self.laser_profile_id, 9)
        ttk.Label(frame, text="Laser mode").grid(row=10, column=0, sticky="w", padx=6, pady=3)
        ttk.Combobox(frame, textvariable=self.laser_mode, values=["off_background", "on_background"], state="readonly").grid(row=10, column=1, sticky="ew", padx=6, pady=3)
        self._entry(frame, "Power mW", self.laser_power, 11)
        self._entry(frame, "Settle ms", self.laser_settle, 12)

    def _build_output_frame(self, parent: ttk.Frame) -> None:
        frame = ttk.LabelFrame(parent, text="Output")
        frame.grid(row=3, column=0, columnspan=2, sticky="nsew")
        frame.columnconfigure(1, weight=1)
        self.output_dir = tk.StringVar()
        ttk.Label(frame, text="Generated config dir").grid(row=0, column=0, sticky="w", padx=6, pady=6)
        ttk.Entry(frame, textvariable=self.output_dir).grid(row=0, column=1, sticky="ew", padx=6, pady=6)
        ttk.Button(frame, text="Browse", command=self._browse_output_dir).grid(row=0, column=2, padx=6, pady=6)
        ttk.Button(frame, text="Generate JSON", command=self._generate).grid(row=0, column=3, padx=6, pady=6)
        self.summary = tk.Text(frame, height=8, wrap="word")
        self.summary.grid(row=1, column=0, columnspan=4, sticky="nsew", padx=6, pady=(0, 6))

    def _entry(self, parent: ttk.Frame, label: str, variable: tk.Variable, row: int, column: int = 0) -> None:
        ttk.Label(parent, text=label).grid(row=row, column=column, sticky="w", padx=6, pady=3)
        ttk.Entry(parent, textvariable=variable).grid(row=row, column=column + 1, sticky="ew", padx=6, pady=3)

    def _axis_row(self, parent: ttk.Frame, axis: str, row: int) -> None:
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
        row_frame = ttk.Frame(parent)
        row_frame.grid(row=row, column=1, columnspan=2, sticky="ew", padx=6, pady=2)
        ttk.Label(row_frame, text=axis.upper(), width=4).grid(row=0, column=0, sticky="w")
        ttk.Checkbutton(row_frame, variable=vars_for_axis["enabled"]).grid(row=0, column=1, sticky="w")
        ttk.Combobox(row_frame, textvariable=vars_for_axis["mode"], values=["range", "list"], state="readonly", width=8).grid(row=0, column=2, padx=2)
        for col, key in enumerate(["fixed", "start", "stop", "step", "values_text"], start=3):
            ttk.Entry(row_frame, textvariable=vars_for_axis[key], width=12 if key != "values_text" else 22).grid(row=0, column=col, padx=2)

    def _load_defaults(self) -> None:
        self.template_vars["plan"].set(str(self.repo_root / "configs" / "plans" / "x_axis_1d_bounce_15min.json"))
        self.template_vars["smb"].set(str(self.repo_root / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json"))
        self.template_vars["oe"].set(str(self.repo_root / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"))
        self.template_vars["laser"].set(str(self.repo_root / "configs" / "profiles" / "cni_laser_run_off_background.json"))
        self.output_dir.set(str(self.repo_root / "configs" / "generated"))

    def _load_profile_defaults(self) -> None:
        try:
            plan = load_json(self.template_vars["plan"].get())
            smb = load_json(self.template_vars["smb"].get())
            oe = load_json(self.template_vars["oe"].get())
            laser = load_json(self.template_vars["laser"].get())
            self.run_id.set(f"{plan.get('run_id', 'generated_plan')}_generated")
            self.operator.set(plan.get("operator", "local"))
            self.point_settle.set(plan.get("point_settle_ms", 500))
            self.acq_window.set(plan.get("acquisition_window_ms", 0))
            sweep = smb["default_sweep"]
            self.smb_profile_id.set(f"{smb.get('profile_id', 'smb100a')}_generated")
            self.smb_start.set(sweep.get("start_hz", 2830000000.0))
            self.smb_stop.set(sweep.get("stop_hz", 2890000000.0))
            self.smb_step.set(sweep.get("step_hz", 500000.0))
            self.smb_dwell.set(sweep.get("dwell_ms", 300))
            self.smb_power.set(sweep.get("power_dbm", -10.0))
            self.smb_rf_output.set(bool(sweep.get("rf_output_enabled", True)))
            self.oe_profile_id.set(f"{oe.get('profile_id', 'oe1022d')}_generated")
            self.oe_time_constant.set(oe["fixed"].get("time_constant_index", 9))
            self.oe_filter_slope.set(oe["fixed"].get("filter_slope", 1))
            self.laser_profile_id.set(f"{laser.get('profile_id', 'cni_laser')}_generated")
            self.laser_mode.set(laser.get("mode", "off_background"))
            self.laser_power.set(laser.get("power_mw", 0))
            self.laser_settle.set(laser.get("settle_ms", 1000))
        except Exception as exc:
            messagebox.showerror("Load templates failed", str(exc), parent=self)

    def _browse_template(self, key: str) -> None:
        path = filedialog.askopenfilename(
            parent=self,
            title=f"Select {key} template",
            initialdir=str(self.repo_root / "configs"),
            filetypes=[("JSON files", "*.json"), ("All files", "*.*")],
        )
        if path:
            self.template_vars[key].set(path)

    def _browse_output_dir(self) -> None:
        path = filedialog.askdirectory(parent=self, title="Select generated config directory", initialdir=self.output_dir.get())
        if path:
            self.output_dir.set(path)

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
            vars_for_axis = self.axis_vars[axis]
            vars_for_axis["enabled"].set(spec.enabled)
            vars_for_axis["mode"].set(spec.mode)
            vars_for_axis["fixed"].set(spec.fixed)
            vars_for_axis["start"].set(spec.start)
            vars_for_axis["stop"].set(spec.stop)
            vars_for_axis["step"].set(spec.step)
            vars_for_axis["values_text"].set(spec.values_text)

    def _read_block(self) -> ScanBlock:
        return ScanBlock(
            prefix=self.block_prefix.get(),
            traversal=self.block_traversal.get(),
            total_points=int(self.block_total_points.get()),
            axes={axis: self._read_axis(axis) for axis in ("x", "y", "z")},
        )

    def _read_axis(self, axis: str) -> AxisSpec:
        vars_for_axis = self.axis_vars[axis]
        return AxisSpec(
            enabled=bool(vars_for_axis["enabled"].get()),
            mode=str(vars_for_axis["mode"].get()),
            fixed=float(vars_for_axis["fixed"].get()),
            start=float(vars_for_axis["start"].get()),
            stop=float(vars_for_axis["stop"].get()),
            step=float(vars_for_axis["step"].get()),
            values_text=str(vars_for_axis["values_text"].get()),
        )

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
            run_id=self.run_id.get(),
            operator=self.operator.get(),
            acquisition_window_ms=int(self.acq_window.get()),
            point_settle_ms=int(self.point_settle.get()),
            blocks=list(self.blocks),
            smb_profile_id=self.smb_profile_id.get(),
            smb_start_hz=float(self.smb_start.get()),
            smb_stop_hz=float(self.smb_stop.get()),
            smb_step_hz=float(self.smb_step.get()),
            smb_dwell_ms=int(self.smb_dwell.get()),
            smb_power_dbm=float(self.smb_power.get()),
            smb_rf_output_enabled=bool(self.smb_rf_output.get()),
            oe_profile_id=self.oe_profile_id.get(),
            oe_time_constant_index=int(self.oe_time_constant.get()),
            oe_filter_slope=int(self.oe_filter_slope.get()),
            laser_profile_id=self.laser_profile_id.get(),
            laser_mode=self.laser_mode.get(),
            laser_power_mw=int(self.laser_power.get()),
            laser_settle_ms=int(self.laser_settle.get()),
        )

    def _generate(self) -> None:
        try:
            self._add_or_update_block()
            paths = write_generated_bundle(
                self.repo_root,
                self._request(),
                self.template_vars["plan"].get(),
                self.template_vars["smb"].get(),
                self.template_vars["oe"].get(),
                self.template_vars["laser"].get(),
                self.output_dir.get(),
            )
            self.summary.delete("1.0", tk.END)
            self.summary.insert(tk.END, "Generated JSON files:\n")
            for key, path in paths.items():
                self.summary.insert(tk.END, f"{key}: {path}\n")
        except Exception as exc:
            messagebox.showerror("Generate failed", str(exc), parent=self)

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
