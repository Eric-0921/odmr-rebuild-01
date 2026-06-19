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
    acquisition_only_points,
    build_laser_profile,
    build_oe_profile,
    build_plan,
    build_smb_profile,
    constant_field_points,
    expand_block,
    from_canonical_unit,
    load_json,
    to_canonical_unit,
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
            mag_baseline_policy={
                "baseline_x_a": 0.0,
                "baseline_y_a": 0.0,
                "baseline_z_a": 0.0,
                "settle_ms": 1000,
                "readback_samples": 3,
                "settle_tolerance_a": 0.002,
                "voltage_v": 75.0,
                "voltage_protection_v": 75.0,
                "output_enabled": True,
            },
            quality_thresholds={
                "min_frames": 20,
                "max_timeout_count": 2,
                "max_duplicate_ratio": 0.3,
                "max_last_frame_age_ms": 500,
            },
            smb_profile_id="smb_generated",
            smb_command_settle_ms=500,
            smb_error_check_after_write=True,
            smb_fixed={
                "modulation_enabled": True,
                "fm_enabled": True,
                "fm_source": "INT",
                "fm_mode": "HDEV",
                "fm_deviation_hz": 4000000.0,
                "lf_output_enabled": True,
                "lf_voltage_mv": 137.0,
                "lf_frequency_hz": 500.0,
                "lf_shape": "SQU",
                "lf_source_impedance": "LOW",
            },
            smb_sweep={
                "start_hz": 2830000000.0,
                "stop_hz": 2890000000.0,
                "step_hz": 500000.0,
                "dwell_ms": 300,
                "power_dbm": -12.0,
                "sweep_mode": "AUTO",
                "spacing": "LIN",
                "shape": "SAWT",
                "trigger_source": "AUTO",
                "output_voltage_start_v": 0.0,
                "output_voltage_stop_v": 3.0,
                "rf_output_enabled": True,
            },
            oe_profile_id="oe_generated",
            oe_command_settle_ms=500,
            oe_fixed={
                "channel": 2,
                "input_source": 0,
                "input_grounding": 0,
                "input_coupling": 0,
                "line_notch_filter": 0,
                "reference_source": 0,
                "reference_slope": 2,
                "phase_deg": 0.0,
                "harmonic_1": 1,
                "harmonic_2": 1,
                "dynamic_reserve": 1,
                "sensitivity_index": 24,
                "time_constant_index": 7,
                "filter_slope": 1,
                "sync_filter": 0,
                "sine_output_mode": 0,
                "sine_output_voltage_vrms": 1.0,
            },
            laser_profile_id="laser_generated",
            laser_mode="on_background",
            laser_power_mw=50,
            laser_settle_ms=1000,
        )

    def test_bounce_block_expands_and_repeats(self) -> None:
        points = expand_block(self.request().blocks[0])
        self.assertEqual(len(points), 9)
        self.assertEqual([point["target_b_nt"][0] for point in points], [0, 10, 20, 30, 40, 30, 20, 10, 0])
        self.assertTrue(all(point["magnetic_mode"] == "controlled" for point in points))

    def test_acquisition_only_points_are_not_fake_zero_field(self) -> None:
        points = acquisition_only_points(1)
        self.assertEqual(points, [{"point_id": "acq_p000001", "magnetic_mode": "none"}])

    def test_constant_field_point_is_controlled(self) -> None:
        points = constant_field_points([0, 10, 20])
        self.assertEqual(points[0]["target_b_nt"], [0.0, 10.0, 20.0])
        self.assertEqual(points[0]["magnetic_mode"], "controlled")

    def test_plan_uses_explicit_points(self) -> None:
        template = load_json(ROOT / "configs" / "plans" / "x_axis_1d_bounce_15min.json")
        plan = build_plan(template, self.request())
        self.assertEqual(plan["run_id"], "test_generated")
        self.assertNotIn("point_source", plan)
        self.assertEqual(len(plan["points"]), 9)
        self.assertEqual(plan["points"][0]["magnetic_mode"], "controlled")
        self.assertEqual(plan["mag_baseline_policy"]["baseline_current_a"], [0.0, 0.0, 0.0])
        self.assertEqual(plan["quality_thresholds"]["min_frames"], 20)

    def test_plan_can_be_no_magnetic_control(self) -> None:
        template = load_json(ROOT / "configs" / "plans" / "minimal_3point_runtime.json")
        request = self.request()
        request.plan_kind = "no_magnetic_control"
        request.acquisition_step_count = 1
        plan = build_plan(template, request)
        self.assertEqual(plan["points"], [{"point_id": "acq_p000001", "magnetic_mode": "none"}])

    def test_plan_can_be_constant_field(self) -> None:
        template = load_json(ROOT / "configs" / "plans" / "minimal_3point_runtime.json")
        request = self.request()
        request.plan_kind = "constant_field"
        request.fixed_b_nt = [0, 0, 0]
        plan = build_plan(template, request)
        self.assertEqual(plan["points"][0]["target_b_nt"], [0.0, 0.0, 0.0])
        self.assertEqual(plan["points"][0]["magnetic_mode"], "controlled")

    def test_profile_overrides_preserve_schema(self) -> None:
        request = self.request()
        smb = build_smb_profile(load_json(ROOT / "configs" / "profiles" / "smb100a_run_monitor_2830_2890_-10dbm.json"), request)
        oe = build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"), request)
        laser = build_laser_profile(load_json(ROOT / "configs" / "profiles" / "cni_laser_run_off_background.json"), request)
        self.assertEqual(smb["default_sweep"]["power_dbm"], -12.0)
        self.assertEqual(smb["fixed"]["fm_deviation_hz"], 4000000.0)
        self.assertTrue(smb["error_check_after_write"])
        self.assertEqual(oe["collector"]["frame_exact_bytes"], 12288)
        self.assertEqual(oe["collector"]["rall_post_write_delay_ms"], 30)
        self.assertEqual(oe["fixed"]["sensitivity_index"], 24)
        self.assertEqual(laser["power_mw"], 50)
        self.assertEqual(laser["mode"], "on_background")

    def test_explicit_oe1300_model_overrides_oe1022d_template(self) -> None:
        request = self.request()
        request.oe_model = "oe1300"
        request.oe_fixed = {
            "input_source": 0,
            "input_coupling": 0,
            "input_range": 1,
            "reference_source": 0,
            "reference_frequency_hz": 1000.0,
            "reference_slope": 0,
            "sensitivity_index": 24,
            "time_constant_seconds": 0.1,
            "filter_slope": 1,
            "sync_enabled": False,
            "sine_output_enabled": False,
            "sine_output_voltage_vrms": 1.0,
        }
        request.oe_collector = {
            "tcp_expected_bytes": 32768,
            "tcp_payload_bytes": 29600,
            "parameter_count": 37,
            "samples_per_parameter": 100,
            "rall_post_write_delay_ms": 5,
            "drain_before_write": True,
        }
        oe = build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"), request)
        self.assertEqual(oe["model"], "oe1300")
        self.assertEqual(oe["collector"]["tcp_expected_bytes"], 32768)
        self.assertEqual(oe["collector"]["rall_post_write_delay_ms"], 5)
        self.assertEqual(oe["fixed"]["input_range"], 1)
        self.assertNotIn("frame_exact_bytes", oe["collector"])
        self.assertNotIn("channel", oe["fixed"])

    def test_oe_template_model_applies_when_request_model_is_not_explicit(self) -> None:
        request = self.request()
        request.oe_model = None
        request.oe_fixed = {}
        request.oe_collector = {}
        oe = build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1300_run_tcp_default.json"), request)
        self.assertEqual(oe["model"], "oe1300")
        self.assertEqual(oe["collector"]["tcp_expected_bytes"], 32768)
        self.assertEqual(oe["collector"]["rall_post_write_delay_ms"], 5)
        self.assertNotIn("frame_exact_bytes", oe["collector"])

    def test_explicit_oe1022d_model_overrides_oe1300_template(self) -> None:
        request = self.request()
        request.oe_model = "oe1022d"
        request.oe_fixed = {}
        request.oe_collector = {}
        oe = build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1300_run_tcp_default.json"), request)
        self.assertEqual(oe["model"], "oe1022d")
        self.assertEqual(oe["collector"]["frame_exact_bytes"], 12288)
        self.assertEqual(oe["collector"]["rall_post_write_delay_ms"], 30)
        self.assertNotIn("tcp_expected_bytes", oe["collector"])

    def test_unknown_oe_model_fails(self) -> None:
        request = self.request()
        request.oe_model = "oe1400"
        with self.assertRaisesRegex(ValueError, "unsupported oe_model"):
            build_oe_profile(load_json(ROOT / "configs" / "profiles" / "oe1022d_run_ch_b_observed.json"), request)

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

    def test_unit_conversion_helpers(self) -> None:
        self.assertEqual(to_canonical_unit(2.83, "frequency_hz", "GHz"), 2830000000.0)
        self.assertEqual(to_canonical_unit(0.5, "frequency_hz", "MHz"), 500000.0)
        self.assertEqual(to_canonical_unit(0.05, "power_mw", "W"), 50.0)
        self.assertEqual(to_canonical_unit(10, "field", "uT"), 10000.0)
        self.assertEqual(to_canonical_unit(2, "current_a", "mA"), 0.002)
        self.assertEqual(to_canonical_unit(0.5, "time_ms", "s"), 500.0)
        self.assertEqual(from_canonical_unit(2830000000.0, "frequency_hz", "GHz"), 2.83)


if __name__ == "__main__":
    unittest.main()
