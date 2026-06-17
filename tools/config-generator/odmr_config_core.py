from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
import copy
import json
from pathlib import Path
import re
from typing import Any


UNIT_FACTORS_TO_CANONICAL: dict[str, dict[str, float]] = {
    "field": {"nT": 1.0, "uT": 1000.0, "mT": 1000000.0},
    "frequency_hz": {"Hz": 1.0, "kHz": 1000.0, "MHz": 1000000.0, "GHz": 1000000000.0},
    "time_ms": {"ms": 1.0, "s": 1000.0},
    "current_a": {"A": 1.0, "mA": 0.001},
    "voltage_v": {"V": 1.0, "mV": 0.001},
    "voltage_mv": {"mV": 1.0, "V": 1000.0},
    "power_mw": {"mW": 1.0, "W": 1000.0},
}


@dataclass
class AxisSpec:
    enabled: bool
    mode: str = "range"
    fixed: float = 0.0
    start: float = 0.0
    stop: float = 0.0
    step: float = 10.0
    values_text: str = "0"


@dataclass
class ScanBlock:
    prefix: str
    traversal: str = "raster"
    total_points: int = 0
    axes: dict[str, AxisSpec] = field(default_factory=dict)


@dataclass
class GeneratorRequest:
    run_id: str
    operator: str
    acquisition_window_ms: int
    point_settle_ms: int
    blocks: list[ScanBlock]
    plan_kind: str = "magnetic_scan"
    acquisition_step_count: int = 1
    fixed_b_nt: list[float] = field(default_factory=lambda: [0.0, 0.0, 0.0])
    mag_baseline_policy: dict[str, Any] = field(default_factory=dict)
    quality_thresholds: dict[str, Any] = field(default_factory=dict)
    smb_profile_id: str = "smb100a_generated"
    smb_command_settle_ms: int = 500
    smb_error_check_after_write: bool = True
    smb_fixed: dict[str, Any] = field(default_factory=dict)
    smb_sweep: dict[str, Any] = field(default_factory=dict)
    oe_model: str = "oe1022d"
    oe_profile_id: str = "oe1022d_generated"
    oe_command_settle_ms: int = 500
    oe_fixed: dict[str, Any] = field(default_factory=dict)
    oe_collector: dict[str, Any] = field(default_factory=dict)
    laser_profile_id: str = "cni_laser_generated"
    laser_mode: str = "off_background"
    laser_power_mw: int = 0
    laser_settle_ms: int = 1000


def load_json(path: str | Path) -> dict[str, Any]:
    with Path(path).open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: str | Path, value: dict[str, Any]) -> None:
    target = Path(path)
    target.parent.mkdir(parents=True, exist_ok=True)
    with target.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(value, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def build_plan(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    plan = copy.deepcopy(template)
    plan["run_id"] = sanitize_id(request.run_id)
    plan["operator"] = request.operator.strip() or plan.get("operator", "local")
    plan["acquisition_window_ms"] = non_negative_int(request.acquisition_window_ms, "acquisition_window_ms")
    plan["point_settle_ms"] = non_negative_int(request.point_settle_ms, "point_settle_ms")
    if request.mag_baseline_policy:
        plan["mag_baseline_policy"] = normalize_mag_baseline(request.mag_baseline_policy)
    if request.quality_thresholds:
        plan["quality_thresholds"] = normalize_quality_thresholds(request.quality_thresholds)
    plan.pop("point_source", None)
    points: list[dict[str, Any]]
    if request.plan_kind == "no_magnetic_control":
        points = acquisition_only_points(request.acquisition_step_count)
    elif request.plan_kind == "constant_field":
        points = constant_field_points(request.fixed_b_nt)
    elif request.plan_kind == "magnetic_scan":
        points = []
        for block in request.blocks:
            points.extend(expand_block(block))
    else:
        raise ValueError(f"unsupported plan_kind: {request.plan_kind}")
    if not points:
        raise ValueError("generated plan has no points")
    plan["points"] = points
    return plan


def build_smb_profile(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    profile = copy.deepcopy(template)
    profile["profile_id"] = sanitize_id(request.smb_profile_id)
    profile["command_settle_ms"] = non_negative_int(request.smb_command_settle_ms, "smb_command_settle_ms")
    profile["error_check_after_write"] = bool(request.smb_error_check_after_write)
    fixed = profile.setdefault("fixed", {})
    fixed.update(normalize_smb_fixed(request.smb_fixed))
    sweep = profile.setdefault("default_sweep", {})
    sweep.update(normalize_smb_sweep(request.smb_sweep))
    return profile


def build_oe_profile(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    profile = copy.deepcopy(template)
    model = str(profile.get("model") or request.oe_model or "oe1022d").strip().lower()
    profile["model"] = model
    profile["profile_id"] = sanitize_id(request.oe_profile_id)
    profile["command_settle_ms"] = non_negative_int(request.oe_command_settle_ms, "oe_command_settle_ms")
    fixed = profile.setdefault("fixed", {})
    collector = profile.setdefault("collector", {})
    if model == "oe1300":
        fixed.update(normalize_oe1300_fixed(request.oe_fixed))
        collector.update(normalize_oe1300_collector(request.oe_collector or collector))
        if collector.get("tcp_expected_bytes") != 32768:
            raise ValueError("oe1300 collector tcp_expected_bytes must remain 32768")
        if collector.get("rall_post_write_delay_ms") != 5:
            raise ValueError("oe1300 collector rall_post_write_delay_ms must remain 5")
        return profile

    fixed.update(normalize_oe1022d_fixed(request.oe_fixed))
    collector.update(normalize_oe1022d_collector(request.oe_collector or collector))
    collector = profile.get("collector", {})
    if collector.get("frame_exact_bytes") != 12288:
        raise ValueError("oe collector frame_exact_bytes must remain 12288")
    if collector.get("rall_post_write_delay_ms") != 30:
        raise ValueError("oe collector rall_post_write_delay_ms must remain 30")
    return profile


def build_laser_profile(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    profile = copy.deepcopy(template)
    profile["profile_id"] = sanitize_id(request.laser_profile_id)
    profile["mode"] = request.laser_mode
    profile["power_mw"] = non_negative_int(request.laser_power_mw, "laser_power_mw")
    profile["settle_ms"] = non_negative_int(request.laser_settle_ms, "laser_settle_ms")
    return profile


def write_generated_bundle(
    repo_root: str | Path,
    request: GeneratorRequest,
    plan_template_path: str | Path,
    smb_template_path: str | Path,
    oe_template_path: str | Path,
    laser_template_path: str | Path,
    output_dir: str | Path | None = None,
) -> dict[str, str]:
    repo = Path(repo_root)
    target_dir = Path(output_dir) if output_dir else repo / "configs" / "generated" / datetime.now().strftime("%Y%m%d")
    run_id = sanitize_id(request.run_id)
    plan = build_plan(load_json(plan_template_path), request)
    smb = build_smb_profile(load_json(smb_template_path), request)
    oe = build_oe_profile(load_json(oe_template_path), request)
    laser = build_laser_profile(load_json(laser_template_path), request)

    paths = {
        "plan": target_dir / f"{run_id}.plan.json",
        "smb_profile": target_dir / f"{sanitize_id(request.smb_profile_id)}.json",
        "oe_profile": target_dir / f"{sanitize_id(request.oe_profile_id)}.json",
        "laser_profile": target_dir / f"{sanitize_id(request.laser_profile_id)}.json",
    }
    write_json(paths["plan"], plan)
    write_json(paths["smb_profile"], smb)
    write_json(paths["oe_profile"], oe)
    write_json(paths["laser_profile"], laser)
    return {key: str(path) for key, path in paths.items()}


def expand_block(block: ScanBlock) -> list[dict[str, Any]]:
    axes = effective_axes(block)
    x = axis_values(axes["x"], "x")
    y = axis_values(axes["y"], "y")
    z = axis_values(axes["z"], "z")
    active = [name for name in ("x", "y", "z") if axes[name].enabled]
    if block.traversal == "bounce_1d_x":
        base_targets = bounce_1d_x_targets(x, y, z, active)
    elif block.traversal == "raster":
        base_targets = raster_targets(x, y, z)
    else:
        raise ValueError(f"unsupported traversal: {block.traversal}")

    total = non_negative_int(block.total_points, "total_points")
    count = total if total > 0 else len(base_targets)
    prefix = sanitize_id(block.prefix)
    return [
        {
            "point_id": f"{prefix}_p{index + 1:06d}",
            "target_b_nt": base_targets[index % len(base_targets)],
            "magnetic_mode": "controlled",
        }
        for index in range(count)
    ]


def acquisition_only_points(count: int) -> list[dict[str, Any]]:
    total = max(1, non_negative_int(count, "acquisition_step_count"))
    return [
        {
            "point_id": f"acq_p{index + 1:06d}",
            "magnetic_mode": "none",
        }
        for index in range(total)
    ]


def constant_field_points(target_b_nt: list[float]) -> list[dict[str, Any]]:
    if len(target_b_nt) != 3:
        raise ValueError("fixed_b_nt must contain exactly 3 values")
    return [
        {
            "point_id": "field_p000001",
            "target_b_nt": [round9(float(value)) for value in target_b_nt],
            "magnetic_mode": "controlled",
        }
    ]


def effective_axes(block: ScanBlock) -> dict[str, AxisSpec]:
    defaults = {
        "x": AxisSpec(enabled=False),
        "y": AxisSpec(enabled=False),
        "z": AxisSpec(enabled=False),
    }
    defaults.update(block.axes)
    return defaults


def axis_values(axis: AxisSpec, name: str) -> list[float]:
    if not axis.enabled:
        return [round9(float(axis.fixed))]
    if axis.mode == "list":
        values = parse_values(axis.values_text)
        if not values:
            raise ValueError(f"{name} list is empty")
        return values
    if axis.mode != "range":
        raise ValueError(f"unsupported {name} axis mode: {axis.mode}")
    start = float(axis.start)
    stop = float(axis.stop)
    step = float(axis.step)
    if step == 0:
        raise ValueError(f"{name} step must not be zero")
    if abs(stop - start) < 1e-12:
        return [round9(start)]
    if (stop - start > 0) != (step > 0):
        raise ValueError(f"{name} step direction must move from start toward stop")
    values: list[float] = []
    value = start
    guard = 0
    while (step > 0 and value <= stop + 1e-9) or (step < 0 and value >= stop - 1e-9):
        values.append(round9(value))
        value += step
        guard += 1
        if guard > 100000:
            raise ValueError(f"{name} generated too many values")
    return values


def raster_targets(x: list[float], y: list[float], z: list[float]) -> list[list[float]]:
    return [[xv, yv, zv] for zv in z for yv in y for xv in x]


def bounce_1d_x_targets(
    x: list[float],
    y: list[float],
    z: list[float],
    active_axes: list[str],
) -> list[list[float]]:
    if active_axes != ["x"] or len(y) != 1 or len(z) != 1:
        raise ValueError("bounce_1d_x requires only X active with fixed Y and Z")
    sequence = list(x)
    if len(x) > 1:
        sequence.extend(x[index] for index in range(len(x) - 2, 0, -1))
    return [[xv, y[0], z[0]] for xv in sequence]


def parse_values(text: str) -> list[float]:
    parts = [part for part in re.split(r"[,\s;]+", text.strip()) if part]
    return [round9(float(part)) for part in parts]


def to_canonical_unit(value: Any, unit_kind: str, unit: str) -> float:
    return float(value) * unit_factor(unit_kind, unit)


def from_canonical_unit(value: Any, unit_kind: str, unit: str) -> float:
    return float(value) / unit_factor(unit_kind, unit)


def unit_factor(unit_kind: str, unit: str) -> float:
    try:
        return UNIT_FACTORS_TO_CANONICAL[unit_kind][unit]
    except KeyError as exc:
        raise ValueError(f"unsupported unit {unit!r} for {unit_kind}") from exc


def normalize_mag_baseline(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "baseline_current_a": [
            float(values.get("baseline_x_a", 0.0)),
            float(values.get("baseline_y_a", 0.0)),
            float(values.get("baseline_z_a", 0.0)),
        ],
        "settle_ms": non_negative_int(values.get("settle_ms", 1000), "baseline.settle_ms"),
        "readback_samples": non_negative_int(values.get("readback_samples", 3), "baseline.readback_samples"),
        "settle_tolerance_a": float(values.get("settle_tolerance_a", 0.002)),
        "voltage_v": nullable_float(values.get("voltage_v", 75.0)),
        "voltage_protection_v": nullable_float(values.get("voltage_protection_v", 75.0)),
        "output_enabled": bool(values.get("output_enabled", True)),
    }


def normalize_quality_thresholds(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "min_frames": non_negative_int(values.get("min_frames", 20), "quality.min_frames"),
        "max_timeout_count": non_negative_int(values.get("max_timeout_count", 2), "quality.max_timeout_count"),
        "max_duplicate_ratio": float(values.get("max_duplicate_ratio", 0.3)),
        "max_last_frame_age_ms": non_negative_int(values.get("max_last_frame_age_ms", 500), "quality.max_last_frame_age_ms"),
    }


def normalize_smb_fixed(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "modulation_enabled": bool(values.get("modulation_enabled", True)),
        "fm_enabled": bool(values.get("fm_enabled", True)),
        "fm_source": str(values.get("fm_source", "INT")),
        "fm_mode": str(values.get("fm_mode", "HDEV")),
        "fm_deviation_hz": float(values.get("fm_deviation_hz", 4000000.0)),
        "lf_output_enabled": bool(values.get("lf_output_enabled", True)),
        "lf_voltage_mv": float(values.get("lf_voltage_mv", 137.0)),
        "lf_frequency_hz": float(values.get("lf_frequency_hz", 500.0)),
        "lf_shape": str(values.get("lf_shape", "SQU")),
        "lf_source_impedance": str(values.get("lf_source_impedance", "LOW")),
    }


def normalize_smb_sweep(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "start_hz": float(values.get("start_hz", 2830000000.0)),
        "stop_hz": float(values.get("stop_hz", 2890000000.0)),
        "step_hz": float(values.get("step_hz", 500000.0)),
        "dwell_ms": non_negative_int(values.get("dwell_ms", 300), "smb.dwell_ms"),
        "power_dbm": float(values.get("power_dbm", -10.0)),
        "sweep_mode": str(values.get("sweep_mode", "AUTO")),
        "spacing": str(values.get("spacing", "LIN")),
        "shape": str(values.get("shape", "SAWT")),
        "trigger_source": str(values.get("trigger_source", "AUTO")),
        "output_voltage_start_v": float(values.get("output_voltage_start_v", 0.0)),
        "output_voltage_stop_v": float(values.get("output_voltage_stop_v", 3.0)),
        "rf_output_enabled": bool(values.get("rf_output_enabled", True)),
    }


def normalize_oe1022d_fixed(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "channel": non_negative_int(values.get("channel", 2), "oe.channel"),
        "input_source": non_negative_int(values.get("input_source", 0), "oe.input_source"),
        "input_grounding": non_negative_int(values.get("input_grounding", 0), "oe.input_grounding"),
        "input_coupling": non_negative_int(values.get("input_coupling", 0), "oe.input_coupling"),
        "line_notch_filter": non_negative_int(values.get("line_notch_filter", 0), "oe.line_notch_filter"),
        "reference_source": non_negative_int(values.get("reference_source", 0), "oe.reference_source"),
        "reference_slope": non_negative_int(values.get("reference_slope", 2), "oe.reference_slope"),
        "phase_deg": float(values.get("phase_deg", 0.0)),
        "harmonic_1": non_negative_int(values.get("harmonic_1", 1), "oe.harmonic_1"),
        "harmonic_2": non_negative_int(values.get("harmonic_2", 1), "oe.harmonic_2"),
        "dynamic_reserve": non_negative_int(values.get("dynamic_reserve", 1), "oe.dynamic_reserve"),
        "sensitivity_index": non_negative_int(values.get("sensitivity_index", 24), "oe.sensitivity_index"),
        "time_constant_index": non_negative_int(values.get("time_constant_index", 7), "oe.time_constant_index"),
        "filter_slope": non_negative_int(values.get("filter_slope", 1), "oe.filter_slope"),
        "sync_filter": non_negative_int(values.get("sync_filter", 0), "oe.sync_filter"),
        "sine_output_mode": non_negative_int(values.get("sine_output_mode", 0), "oe.sine_output_mode"),
        "sine_output_voltage_vrms": float(values.get("sine_output_voltage_vrms", 1.0)),
    }


def normalize_oe1022d_collector(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "poll_interval_ms": non_negative_int(values.get("poll_interval_ms", 48), "oe.poll_interval_ms"),
        "frame_exact_bytes": non_negative_int(values.get("frame_exact_bytes", 12288), "oe.frame_exact_bytes"),
        "frame_max_bytes": non_negative_int(values.get("frame_max_bytes", 16384), "oe.frame_max_bytes"),
        "ring_capacity_frames": non_negative_int(values.get("ring_capacity_frames", 512), "oe.ring_capacity_frames"),
        "guard_margin_ms": non_negative_int(values.get("guard_margin_ms", 3000), "oe.guard_margin_ms"),
        "rall_post_write_delay_ms": non_negative_int(values.get("rall_post_write_delay_ms", 30), "oe.rall_post_write_delay_ms"),
    }


def normalize_oe1300_fixed(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "input_source": non_negative_int(values.get("input_source", 0), "oe1300.input_source"),
        "input_coupling": non_negative_int(values.get("input_coupling", 0), "oe1300.input_coupling"),
        "input_range": non_negative_int(values.get("input_range", 0), "oe1300.input_range"),
        "reference_source": non_negative_int(values.get("reference_source", 0), "oe1300.reference_source"),
        "reference_frequency_hz": float(values.get("reference_frequency_hz", 1000.0)),
        "reference_slope": non_negative_int(values.get("reference_slope", 0), "oe1300.reference_slope"),
        "sensitivity_index": non_negative_int(values.get("sensitivity_index", 24), "oe1300.sensitivity_index"),
        "time_constant_seconds": float(values.get("time_constant_seconds", 0.1)),
        "filter_slope": non_negative_int(values.get("filter_slope", 1), "oe1300.filter_slope"),
        "sync_enabled": bool(values.get("sync_enabled", False)),
        "sine_output_enabled": bool(values.get("sine_output_enabled", False)),
        "sine_output_voltage_vrms": float(values.get("sine_output_voltage_vrms", 1.0)),
    }


def normalize_oe1300_collector(values: dict[str, Any]) -> dict[str, Any]:
    return {
        "tcp_expected_bytes": non_negative_int(values.get("tcp_expected_bytes", 32768), "oe1300.tcp_expected_bytes"),
        "tcp_payload_bytes": non_negative_int(values.get("tcp_payload_bytes", 29600), "oe1300.tcp_payload_bytes"),
        "parameter_count": non_negative_int(values.get("parameter_count", 37), "oe1300.parameter_count"),
        "samples_per_parameter": non_negative_int(values.get("samples_per_parameter", 100), "oe1300.samples_per_parameter"),
        "rall_post_write_delay_ms": non_negative_int(values.get("rall_post_write_delay_ms", 5), "oe1300.rall_post_write_delay_ms"),
        "drain_before_write": bool(values.get("drain_before_write", True)),
    }


def nullable_float(value: Any) -> float | None:
    if value is None:
        return None
    if isinstance(value, str) and not value.strip():
        return None
    return float(value)


def sanitize_id(value: str) -> str:
    cleaned = re.sub(r"[^A-Za-z0-9_-]+", "_", value.strip()).strip("_")
    return cleaned or "generated_config"


def non_negative_int(value: int | float | str, name: str) -> int:
    parsed = int(value)
    if parsed < 0:
        raise ValueError(f"{name} must be non-negative")
    return parsed


def round9(value: float) -> float:
    return round(float(value), 9)
