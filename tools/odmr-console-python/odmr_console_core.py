from __future__ import annotations

from dataclasses import asdict, dataclass
from datetime import datetime, timezone
import json
import os
from pathlib import Path
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
    ])
    return command


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
) -> LaunchHandle:
    repo = Path(repo_root) if repo_root else REPO_ROOT
    out_path = Path(out_dir)
    control_paths = control_paths_for_out_dir(out_path)
    Path(control_paths.control_dir).mkdir(parents=True, exist_ok=True)
    stop_path = Path(control_paths.stop_request_file)
    if stop_path.exists():
        stop_path.unlink()

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
    write_json(
        control_paths.launch_metadata,
        {
            "schema_version": 1,
            "started_at": utc_now(),
            "pid": handle.pid,
            "command": handle.command,
            "cwd": handle.cwd,
            "out_dir": handle.out_dir,
            "bundle": asdict(bundle),
            "control_paths": asdict(control_paths),
        },
    )
    return handle


def request_stop(stop_request_file: str | Path) -> None:
    Path(stop_request_file).parent.mkdir(parents=True, exist_ok=True)
    Path(stop_request_file).write_text(f"stop requested at {utc_now()}\n", encoding="utf-8")


def read_progress(path: str | Path) -> list[dict[str, Any]]:
    progress_path = Path(path)
    if not progress_path.exists():
        return []
    records = []
    with progress_path.open("r", encoding="utf-8") as handle:
        for line in handle:
            stripped = line.strip()
            if stripped:
                records.append(json.loads(stripped))
    return records


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
        if progress_path.exists():
            with progress_path.open("r", encoding="utf-8") as handle:
                handle.seek(offset)
                for line in handle:
                    stripped = line.strip()
                    if stripped:
                        yield json.loads(stripped)
                offset = handle.tell()
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
