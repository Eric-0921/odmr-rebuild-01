from __future__ import annotations

import argparse
from dataclasses import asdict
import json
from pathlib import Path
import sys

from odmr_console_core import (
    RunBundle,
    default_bundle,
    demo_generator_request,
    generate_config_bundle,
    load_json,
    read_progress,
    request_stop,
    resolve_bundle,
    start_run,
)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="ODMR Python console core CLI")
    subparsers = parser.add_subparsers(dest="command", required=True)

    demo = subparsers.add_parser("generate-demo-bundle", help="generate a 3-point test bundle with current config-generator core")
    demo.add_argument("--out-dir", required=True)
    demo.add_argument("--run-id", default="python_console_demo_3point")

    resolve = subparsers.add_parser("resolve", help="call C# run-resolve for a bundle")
    add_bundle_args(resolve)
    resolve.add_argument("--dotnet", default="dotnet")

    start = subparsers.add_parser("start-run", help="start C# run-execute with progress JSONL and stop request file")
    add_bundle_args(start)
    start.add_argument("--out-dir", required=True)
    start.add_argument("--dotnet", default="dotnet")

    stop = subparsers.add_parser("stop", help="request stop-after-current-point")
    stop.add_argument("--metadata")
    stop.add_argument("--stop-request-file")

    progress = subparsers.add_parser("read-progress", help="print existing progress JSONL records as a JSON array")
    progress.add_argument("--progress-jsonl", required=True)

    args = parser.parse_args(argv)
    if args.command == "generate-demo-bundle":
        bundle = generate_config_bundle(demo_generator_request(args.run_id), args.out_dir)
        print(json.dumps(asdict(bundle), indent=2, ensure_ascii=False))
        return 0

    if args.command == "resolve":
        result = resolve_bundle(bundle_from_args(args), dotnet=args.dotnet)
        if result.stdout:
            print(result.stdout, end="")
        if result.stderr:
            print(result.stderr, file=sys.stderr, end="")
        return result.returncode

    if args.command == "start-run":
        handle = start_run(bundle_from_args(args), args.out_dir, dotnet=args.dotnet)
        print(json.dumps(asdict(handle), indent=2, ensure_ascii=False))
        return 0

    if args.command == "stop":
        stop_path = args.stop_request_file
        if args.metadata:
            metadata = load_json(args.metadata)
            stop_path = metadata["control_paths"]["stop_request_file"]
        if not stop_path:
            parser.error("stop requires --metadata or --stop-request-file")
        request_stop(stop_path)
        print(f"stop requested: {stop_path}")
        return 0

    if args.command == "read-progress":
        print(json.dumps(read_progress(args.progress_jsonl), indent=2, ensure_ascii=False))
        return 0

    parser.error(f"unknown command: {args.command}")
    return 1


def add_bundle_args(parser: argparse.ArgumentParser) -> None:
    defaults = default_bundle()
    parser.add_argument("--station", default=defaults.station_path)
    parser.add_argument("--calibration", default=defaults.calibration_path)
    parser.add_argument("--plan", default=defaults.plan_path)
    parser.add_argument("--smb-profile", default=defaults.smb_profile_path)
    parser.add_argument("--oe-profile", default=defaults.oe_profile_path)
    parser.add_argument("--laser-profile", default=defaults.laser_profile_path)


def bundle_from_args(args: argparse.Namespace) -> RunBundle:
    return RunBundle(
        station_path=str(Path(args.station)),
        calibration_path=str(Path(args.calibration)),
        plan_path=str(Path(args.plan)),
        smb_profile_path=str(Path(args.smb_profile)),
        oe_profile_path=str(Path(args.oe_profile)),
        laser_profile_path=str(Path(args.laser_profile)),
    )


if __name__ == "__main__":
    raise SystemExit(main())
