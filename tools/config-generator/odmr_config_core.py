from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
import copy
import json
from pathlib import Path
import re
from typing import Any


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
    smb_profile_id: str
    smb_start_hz: float
    smb_stop_hz: float
    smb_step_hz: float
    smb_dwell_ms: int
    smb_power_dbm: float
    smb_rf_output_enabled: bool
    oe_profile_id: str
    oe_time_constant_index: int
    oe_filter_slope: int
    laser_profile_id: str
    laser_mode: str
    laser_power_mw: int
    laser_settle_ms: int


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
    plan.pop("point_source", None)
    points: list[dict[str, Any]] = []
    for block in request.blocks:
        points.extend(expand_block(block))
    if not points:
        raise ValueError("generated plan has no points")
    plan["points"] = points
    return plan


def build_smb_profile(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    profile = copy.deepcopy(template)
    profile["profile_id"] = sanitize_id(request.smb_profile_id)
    sweep = profile.setdefault("default_sweep", {})
    sweep["start_hz"] = float(request.smb_start_hz)
    sweep["stop_hz"] = float(request.smb_stop_hz)
    sweep["step_hz"] = float(request.smb_step_hz)
    sweep["dwell_ms"] = non_negative_int(request.smb_dwell_ms, "smb_dwell_ms")
    sweep["power_dbm"] = float(request.smb_power_dbm)
    sweep["rf_output_enabled"] = bool(request.smb_rf_output_enabled)
    return profile


def build_oe_profile(template: dict[str, Any], request: GeneratorRequest) -> dict[str, Any]:
    profile = copy.deepcopy(template)
    profile["profile_id"] = sanitize_id(request.oe_profile_id)
    fixed = profile.setdefault("fixed", {})
    fixed["time_constant_index"] = non_negative_int(request.oe_time_constant_index, "oe_time_constant_index")
    fixed["filter_slope"] = non_negative_int(request.oe_filter_slope, "oe_filter_slope")
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
        }
        for index in range(count)
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
