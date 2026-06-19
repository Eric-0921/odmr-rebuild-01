from __future__ import annotations

from dataclasses import asdict, dataclass
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time
from typing import Any, Iterator, Sequence


def find_repo_root(start: str | Path | None = None) -> Path:
    current = Path(start or __file__).resolve()
    for parent in [current, *current.parents]:
        if (parent / "configs").is_dir() and (parent / "tools" / "win-csharp").is_dir():
            return parent
    raise RuntimeError("could not find repository root containing configs and tools/win-csharp")


REPO_ROOT = find_repo_root()
CONFIG_GENERATOR_DIR = REPO_ROOT / "tools" / "config-generator"
if str(CONFIG_GENERATOR_DIR) not in sys.path:
    sys.path.insert(0, str(CONFIG_GENERATOR_DIR))

from odmr_config_core import AxisSpec, GeneratorRequest, ScanBlock, write_generated_bundle  # noqa: E402

TAG_PATTERN = re.compile(r"(?<!\S)#([^\s#]+)")


@dataclass(frozen=True)
class RunBundle:
    station_path: str
    calibration_path: str
    plan_path: str
    smb_profile_path: str
    oe_profile_path: str
    laser_profile_path: str


@dataclass(frozen=True)
class ControlPaths:
    control_dir: str
    progress_jsonl: str
    stop_request_file: str
    emergency_stop_file: str
    launch_metadata: str
    stdout_log: str
    stderr_log: str


@dataclass(frozen=True)
class LaunchHandle:
    pid: int
    command: list[str]
    cwd: str
    out_dir: str
    control_paths: ControlPaths
    metadata_path: str


@dataclass(frozen=True)
class ResumeHandle:
    previous_run_dir: str
    resume_out_dir: str
    resume_from_run_id: str | None


def parse_operator_tags(notes: str) -> list[str]:
    tags: list[str] = []
    seen: set[str] = set()
    for match in TAG_PATTERN.finditer(notes):
        tag = match.group(1).strip()
        if tag and tag not in seen:
            tags.append(tag)
            seen.add(tag)
    return tags


def build_operator_metadata(probe_id: str = "", notes: str = "") -> dict[str, Any] | None:
    normalized_probe_id = probe_id.strip()
    normalized_notes = notes.strip()
    tags = parse_operator_tags(normalized_notes)
    if not normalized_probe_id and not normalized_notes and not tags:
        return None
    return {
        "schema_version": 1,
        "probe_id": normalized_probe_id or None,
        "notes": normalized_notes or None,
        "tags": tags,
    }


def utc_now() -> str:
    return datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def default_bundle(repo_root: str | Path | None = None) -> RunBundle:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    return RunBundle(
        station_path=str(repo / "configs" / "stations" / "lab_a.json"),
        calibration_path=str(repo / "configs" / "calibrations" / "main.json"),
        plan_path=str(repo / "configs" / "plans" / "x_axis_1d_bounce_15min.json"),
        smb_profile_path=str(repo / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json"),
        oe_profile_path=str(repo / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"),
        laser_profile_path=str(repo / "configs" / "profiles" / "cni_laser_run_off_background.json"),
    )


def default_template_paths(repo_root: str | Path | None = None) -> dict[str, str]:
    bundle = default_bundle(repo_root)
    return {
        "plan_template": bundle.plan_path,
        "smb_template": bundle.smb_profile_path,
        "oe_template": bundle.oe_profile_path,
        "laser_template": bundle.laser_profile_path,
    }


def demo_generator_request(run_id: str = "python_console_demo_3point") -> GeneratorRequest:
    return GeneratorRequest(
        run_id=run_id,
        operator="python_console",
        acquisition_window_ms=0,
        point_settle_ms=500,
        blocks=[
            ScanBlock(
                prefix="demo_x",
                traversal="raster",
                total_points=0,
                axes={
                    "x": AxisSpec(True, "list", 0.0, 0.0, 0.0, 10.0, "0, 10, 20"),
                    "y": AxisSpec(False, "range", 0.0, 0.0, 0.0, 10.0, "0"),
                    "z": AxisSpec(False, "range", 0.0, 0.0, 0.0, 10.0, "0"),
                },
            )
        ],
        smb_profile_id="smb100a_python_console_demo",
        oe_profile_id="oe1022d_python_console_demo",
        laser_profile_id="cni_laser_python_console_demo",
    )


def generate_config_bundle(
    request: GeneratorRequest,
    output_dir: str | Path,
    repo_root: str | Path | None = None,
    station_path: str | Path | None = None,
    calibration_path: str | Path | None = None,
    plan_template_path: str | Path | None = None,
    smb_template_path: str | Path | None = None,
    oe_template_path: str | Path | None = None,
    laser_template_path: str | Path | None = None,
) -> RunBundle:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    defaults = default_bundle(repo)
    templates = default_template_paths(repo)
    paths = write_generated_bundle(
        repo,
        request,
        plan_template_path or templates["plan_template"],
        smb_template_path or templates["smb_template"],
        oe_template_path or templates["oe_template"],
        laser_template_path or templates["laser_template"],
        output_dir,
    )
    return RunBundle(
        station_path=str(station_path or defaults.station_path),
        calibration_path=str(calibration_path or defaults.calibration_path),
        plan_path=paths["plan"],
        smb_profile_path=paths["smb_profile"],
        oe_profile_path=paths["oe_profile"],
        laser_profile_path=paths["laser_profile"],
    )


def control_paths_for_out_dir(out_dir: str | Path) -> ControlPaths:
    control_dir = Path(out_dir) / "control"
    return ControlPaths(
        control_dir=str(control_dir),
        progress_jsonl=str(control_dir / "progress.jsonl"),
        stop_request_file=str(control_dir / "stop.request"),
        emergency_stop_file=str(control_dir / "emergency_stop.request"),
        launch_metadata=str(control_dir / "launch_metadata.json"),
        stdout_log=str(control_dir / "stdout.log"),
        stderr_log=str(control_dir / "stderr.log"),
    )


def winprobe_project(repo_root: str | Path | None = None) -> str:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    return str(repo / "tools" / "win-csharp" / "Odmr.WinProbe")


def run_resolve_command(bundle: RunBundle, repo_root: str | Path | None = None, dotnet: str = "dotnet") -> list[str]:
    return [
        dotnet,
        "run",
        "--project",
        winprobe_project(repo_root),
        "--",
        "run-resolve",
        "--station",
        bundle.station_path,
        "--calibration",
        bundle.calibration_path,
        "--plan",
        bundle.plan_path,
        "--smb-profile",
        bundle.smb_profile_path,
        "--oe-profile",
        bundle.oe_profile_path,
        "--laser-profile",
        bundle.laser_profile_path,
    ]


def run_execute_command(
    bundle: RunBundle,
    out_dir: str | Path,
    control_paths: ControlPaths,
    repo_root: str | Path | None = None,
    dotnet: str = "dotnet",
) -> list[str]:
    command = run_resolve_command(bundle, repo_root, dotnet)
    command[5] = "run-execute"
    command.extend([
        "--out-dir",
        str(out_dir),
        "--progress-jsonl",
        control_paths.progress_jsonl,
        "--stop-request-file",
        control_paths.stop_request_file,
        "--emergency-stop-file",
        control_paths.emergency_stop_file,
    ])
    return command


def resume_run_command(
    previous_run: str | Path,
    out_dir: str | Path,
    control_paths: ControlPaths,
    repo_root: str | Path | None = None,
    dotnet: str = "dotnet",
) -> list[str]:
    return [
        dotnet,
        "run",
        "--project",
        winprobe_project(repo_root),
        "--",
        "resume-run",
        "--previous-run",
        str(previous_run),
        "--out-dir",
        str(out_dir),
        "--progress-jsonl",
        control_paths.progress_jsonl,
        "--stop-request-file",
        control_paths.stop_request_file,
        "--emergency-stop-file",
        control_paths.emergency_stop_file,
    ]


def resolve_bundle(bundle: RunBundle, repo_root: str | Path | None = None, dotnet: str = "dotnet") -> subprocess.CompletedProcess[str]:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    return subprocess.run(
        run_resolve_command(bundle, repo, dotnet),
        cwd=repo,
        text=True,
        capture_output=True,
        check=False,
    )


def start_run(
    bundle: RunBundle,
    out_dir: str | Path,
    repo_root: str | Path | None = None,
    dotnet: str = "dotnet",
    operator_metadata: dict[str, Any] | None = None,
) -> LaunchHandle:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    out_path = Path(out_dir)
    control_paths = control_paths_for_out_dir(out_path)
    Path(control_paths.control_dir).mkdir(parents=True, exist_ok=True)
    stop_path = Path(control_paths.stop_request_file)
    if stop_path.exists():
        stop_path.unlink()
    emergency_path = Path(control_paths.emergency_stop_file)
    if emergency_path.exists():
        emergency_path.unlink()

    command = run_execute_command(bundle, out_path, control_paths, repo, dotnet)
    stdout = open(control_paths.stdout_log, "w", encoding="utf-8", newline="\n")
    stderr = open(control_paths.stderr_log, "w", encoding="utf-8", newline="\n")
    try:
        process = subprocess.Popen(command, cwd=repo, stdout=stdout, stderr=stderr, text=True)
    finally:
        stdout.close()
        stderr.close()

    handle = LaunchHandle(
        pid=process.pid,
        command=command,
        cwd=str(repo),
        out_dir=str(out_path),
        control_paths=control_paths,
        metadata_path=control_paths.launch_metadata,
    )
    metadata = {
        "schema_version": 1,
        "started_at": utc_now(),
        "pid": handle.pid,
        "command": handle.command,
        "cwd": handle.cwd,
        "out_dir": handle.out_dir,
        "bundle": asdict(bundle),
        "control_paths": asdict(control_paths),
    }
    if operator_metadata:
        metadata["operator_metadata"] = operator_metadata
    write_json(control_paths.launch_metadata, metadata)
    return handle


def next_resume_out_dir(previous_run_dir: str | Path) -> Path:
    source = Path(previous_run_dir)
    parent = source.parent
    stem = source.name
    for index in range(1, 100):
        candidate = parent / f"{stem}__resume_{index:02d}"
        if not candidate.exists():
            return candidate
    raise RuntimeError(f"failed to allocate resume directory for {source}")


def start_resume(
    previous_run_dir: str | Path,
    repo_root: str | Path | None = None,
    dotnet: str = "dotnet",
) -> LaunchHandle:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    previous_path = Path(previous_run_dir)
    out_path = next_resume_out_dir(previous_path)
    control_paths = control_paths_for_out_dir(out_path)
    Path(control_paths.control_dir).mkdir(parents=True, exist_ok=True)
    for request_file in [control_paths.stop_request_file, control_paths.emergency_stop_file]:
        request_path = Path(request_file)
        if request_path.exists():
            request_path.unlink()

    command = resume_run_command(previous_path, out_path, control_paths, repo, dotnet)
    stdout = open(control_paths.stdout_log, "w", encoding="utf-8", newline="\n")
    stderr = open(control_paths.stderr_log, "w", encoding="utf-8", newline="\n")
    try:
        process = subprocess.Popen(command, cwd=repo, stdout=stdout, stderr=stderr, text=True)
    finally:
        stdout.close()
        stderr.close()

    run_id = None
    plan_snapshot = previous_path / "plan_snapshot.json"
    if plan_snapshot.exists():
        try:
            run_id = load_json(plan_snapshot).get("run_id")
        except Exception:
            run_id = None

    handle = LaunchHandle(
        pid=process.pid,
        command=command,
        cwd=str(repo),
        out_dir=str(out_path),
        control_paths=control_paths,
        metadata_path=control_paths.launch_metadata,
    )
    write_json(
        control_paths.launch_metadata,
        {
            "schema_version": 1,
            "started_at": utc_now(),
            "pid": handle.pid,
            "command": handle.command,
            "cwd": handle.cwd,
            "out_dir": handle.out_dir,
            "resume": asdict(ResumeHandle(str(previous_path), str(out_path), run_id)),
            "control_paths": asdict(control_paths),
        },
    )
    return handle


def request_stop(stop_request_file: str | Path) -> None:
    Path(stop_request_file).parent.mkdir(parents=True, exist_ok=True)
    Path(stop_request_file).write_text(f"stop requested at {utc_now()}\n", encoding="utf-8")


def request_emergency_stop(emergency_stop_file: str | Path) -> None:
    Path(emergency_stop_file).parent.mkdir(parents=True, exist_ok=True)
    Path(emergency_stop_file).write_text(f"emergency stop requested at {utc_now()}\n", encoding="utf-8")


def discard_run_dir(run_dir: str | Path) -> Path:
    source = Path(run_dir)
    if not source.exists():
        raise FileNotFoundError(str(source))
    discarded_root = source.parent / "_discarded"
    discarded_root.mkdir(parents=True, exist_ok=True)
    target = discarded_root / f"{source.name}_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
    source.replace(target)
    return target


def read_progress(path: str | Path) -> list[dict[str, Any]]:
    return read_jsonl(path)


def read_jsonl(path: str | Path) -> list[dict[str, Any]]:
    records, _offset = read_jsonl_since(path)
    return records


def read_jsonl_since(path: str | Path, offset: int = 0) -> tuple[list[dict[str, Any]], int]:
    target = Path(path)
    if not target.exists():
        return [], offset
    if offset > target.stat().st_size:
        offset = 0
    records: list[dict[str, Any]] = []
    with target.open("r", encoding="utf-8") as handle:
        handle.seek(offset)
        while True:
            line_start = handle.tell()
            line = handle.readline()
            if line == "":
                return records, handle.tell()
            stripped = line.strip()
            if not stripped:
                continue
            try:
                records.append(json.loads(stripped))
            except json.JSONDecodeError:
                if not line.endswith("\n"):
                    return records, line_start
                continue


def read_progress_since(path: str | Path, offset: int = 0) -> tuple[list[dict[str, Any]], int]:
    return read_jsonl_since(path, offset)


def process_is_running(pid: int) -> bool:
    if pid <= 0:
        return False
    if sys.platform.startswith("win"):
        result = subprocess.run(
            ["tasklist", "/FI", f"PID eq {pid}", "/FO", "CSV", "/NH"],
            text=True,
            capture_output=True,
            check=False,
        )
        return result.returncode == 0 and str(pid) in result.stdout and "INFO:" not in result.stdout
    try:
        os.kill(pid, 0)
        return True
    except ProcessLookupError:
        return False
    except PermissionError:
        return True


def read_text_tail(path: str | Path, max_chars: int = 6000) -> str:
    target = Path(path)
    if not target.exists():
        return ""
    text = target.read_text(encoding="utf-8", errors="replace")
    return text[-max_chars:]


def tail_progress(path: str | Path, poll_sec: float = 0.2) -> Iterator[dict[str, Any]]:
    progress_path = Path(path)
    offset = 0
    while True:
        records, offset = read_jsonl_since(progress_path, offset)
        yield from records
        time.sleep(poll_sec)


def write_json(path: str | Path, value: Any) -> None:
    target = Path(path)
    target.parent.mkdir(parents=True, exist_ok=True)
    with target.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(value, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def load_json(path: str | Path) -> Any:
    with Path(path).open("r", encoding="utf-8") as handle:
        return json.load(handle)


def bundle_from_mapping(value: dict[str, Any]) -> RunBundle:
    return RunBundle(
        station_path=value["station_path"],
        calibration_path=value["calibration_path"],
        plan_path=value["plan_path"],
        smb_profile_path=value["smb_profile_path"],
        oe_profile_path=value["oe_profile_path"],
        laser_profile_path=value["laser_profile_path"],
    )


def command_to_text(command: Sequence[str]) -> str:
    return " ".join(command)
