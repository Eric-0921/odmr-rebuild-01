from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT / "tools" / "config-generator"))

from odmr_config_core import (  # noqa: E402
    AxisSpec,
    GeneratorRequest,
    ScanBlock,
    build_laser_profile,
    build_oe_profile,
    build_plan,
    build_smb_profile,
    expand_block,
    load_json,
    write_generated_bundle,
)


class ConfigCoreTests(unittest.TestCase):
    def request(self) -> GeneratorRequest:
        block = ScanBlock(
            prefix="x_line",
            traversal="bounce_1d_x",
            total_points=9,
            axes={
                "x": AxisSpec(enabled=True, start=0, stop=40, step=10),
                "y": AxisSpec(enabled=False, fixed=0),
                "z": AxisSpec(enabled=False, fixed=0),
            },
        )
        return GeneratorRequest(
            run_id="test generated",
            operator="local",
            acquisition_window_ms=0,
            point_settle_ms=500,
            blocks=[block],
            smb_profile_id="smb_generated",
            smb_start_hz=2830000000.0,
            smb_stop_hz=2890000000.0,
            smb_step_hz=500000.0,
            smb_dwell_ms=300,
            smb_power_dbm=-12.0,
            smb_rf_output_enabled=True,
            oe_profile_id="oe_generated",
            oe_time_constant_index=9,
            oe_filter_slope=1,
            laser_profile_id="laser_generated",
            laser_mode="on_background",
            laser_power_mw=50,
            laser_settle_ms=1000,
        )

    def test_bounce_block_expands_and_repeats(self) -> None:
        points = expand_block(self.request().blocks[0])
        self.assertEqual(len(points), 9)
        self.assertEqual([point["target_b_nt"][0] for point in points], [0, 10, 20, 30, 40, 30, 20, 10, 0])

    def test_plan_uses_explicit_points(self) -> None:
        template = load_json(ROOT / "configs" / "plans" / "x_axis_1d_bounce_15min.json")
        plan = build_plan(template, self.request())
        self.assertEqual(plan["run_id"], "test_generated")
        self.assertNotIn("point_source", plan)
        self.assertEqual(len(plan["points"]), 9)

    def test_profile_overrides_preserve_schema(self) -> None:
        request = self.request()
        smb = build_smb_profile(load_json(ROOT / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json"), request)
        oe = build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"), request)
        laser = build_laser_profile(load_json(ROOT / "configs" / "profiles" / "cni_laser_run_off_background.json"), request)
        self.assertEqual(smb["default_sweep"]["power_dbm"], -12.0)
        self.assertEqual(oe["collector"]["frame_exact_bytes"], 12288)
        self.assertEqual(oe["collector"]["rall_post_write_delay_ms"], 30)
        self.assertEqual(laser["power_mw"], 50)
        self.assertEqual(laser["mode"], "on_background")

    def test_write_generated_bundle(self) -> None:
        with tempfile.TemporaryDirectory() as temp_dir:
            paths = write_generated_bundle(
                ROOT,
                self.request(),
                ROOT / "configs" / "plans" / "x_axis_1d_bounce_15min.json",
                ROOT / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json",
                ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json",
                ROOT / "configs" / "profiles" / "cni_laser_run_off_background.json",
                temp_dir,
            )
            for path in paths.values():
                self.assertTrue(Path(path).exists())


if __name__ == "__main__":
    unittest.main()
