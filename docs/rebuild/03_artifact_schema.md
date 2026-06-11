# Artifact Schema

Artifact 是 run 的事实来源。第一版使用普通文件和 JSONL，便于 grep、replay、Python 分析和人工审查。

## Run 目录

```text
runs/<run_id>/
  run_manifest.json
  station_snapshot.json
  calibration_snapshot.json
  smb_profile_snapshot.json
  oe_profile_snapshot.json
  laser_profile_snapshot.json
  plan_snapshot.json
  events.jsonl
  points.jsonl
  point_fields.jsonl
  point_fields/
    seg_<point_id>_0000.npz
  segments.jsonl
  quality.jsonl
  raw/
    oe1022d.rall
    oe1022d.frames.idx.jsonl
    oe1022d.frames.parsed.jsonl   # 仅 debug artifact mode
  summary.json
```

所有 snapshot 都在 run 打开阶段落盘。`station_snapshot / calibration_snapshot / smb_profile_snapshot / oe_profile_snapshot / laser_profile_snapshot` 是输入冻结副本；`plan_snapshot` 则额外包含运行前展开后的 resolved points 与估算元信息。run 过程中不读取可变原始配置作为事实来源。

## run_manifest.json

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "created_at": "2026-06-10T10:00:00Z",
  "operator": "local",
  "station_id": "lab_a",
  "calibration_id": "cal_001",
  "runtime_version": "0.1.0",
  "status": "running",
  "laser_profile_id": "cni_laser_run_on_background",
  "plan_source_kind": "cartesian_grid",
  "resolved_point_count": 104,
  "estimated_run_duration_ms": 863200
}
```

终态允许值：

- `completed`
- `completed_with_failed_points`
- `failed`
- `aborted`
- `cleanup_failed`

## events.jsonl

每行一个事件：

```json
{
  "ts": "2026-06-10T10:00:01.123Z",
  "monotonic_ns": 123456789,
  "event": "point_stable",
  "run_id": "test_001",
  "point_id": "p0001",
  "device": "mag_x",
  "phase": "settle",
  "data": {
    "target_current_a": 0.0123,
    "measured_current_a": 0.0122
  }
}
```

必须记录的事件：

- `run_opened`
- `station_resolved`
- `preflight_passed`
- `collector_started`
- `point_prepare_started`
- `point_stable`
- `segment_started`
- `sweep_started`
- `sweep_completed`
- `segment_completed`
- `point_completed`
- `point_failed`
- `cleanup_started`
- `cleanup_completed`
- `collector_stopped`
- `run_completed`
- `run_failed`

## raw/oe1022d.frames.parsed.jsonl

这是 `raw/oe1022d.rall` 的运行时结构化 companion truth，不替代 raw。  
当前它不再是默认 artifact，而是 `run execute --artifact-mode debug` 才会额外写出的重型调试产物。

每行至少包含：

- `frame_seq`
- `ts`
- `monotonic_ns`
- `raw_offset`
- `raw_len`
- `transport_status`
- `parse_status`
- `padding_status`
- `duplicate_hint`
- `measurement_field_order`
- `measurement_matrix`
- `scalar_fields`
- `b_ref_source_code`
- `b_ref_slope_code`
- `b_ref_current_freq_hz`
- `b_input_overload`
- `b_gain_overload`
- `b_pll_locked`

当前工程结论：

- `raw + frames.idx + segments` 仍是 point 真值层
- `frames.parsed` 负责把每帧按手册表格直接整理成结构化 debug sidecar
- 默认轻量模式下，point 聚合和 continuity audit 直接从 `raw + frames.idx` 现算，不依赖 `frames.parsed`

## plan_snapshot.json

`plan_snapshot.json` 不再只是原始输入副本，它必须同时表达：

- 原始 plan 是显式 `points` 还是高层 `cartesian_grid`
- 展开后的 resolved point 总数
- 若是 `cartesian_grid`，当前 cycle mode / fixed total points / 估算 sweep 时长

示例：

```json
{
  "schema_version": 1,
  "run_id": "x_axis_1d_bounce_15min",
  "source_kind": "cartesian_grid",
  "declared_point_count": 8,
  "resolved_point_count": 104,
  "fixed_total_points": 104,
  "cycle_mode": "bounce_1d_x",
  "estimated_sweep": {
    "sweep_points": 26,
    "sweep_duration_ms": 1300
  },
  "estimated_point_duration_ms": 8300,
  "estimated_run_duration_ms": 863200
}
```

## points.jsonl

每个 point 一行，表达实验语义：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "point_id": "p0001",
  "index": 0,
  "target_b_nt": [10.0, 0.0, 0.0],
  "baseline_current_a": [0.0020, -0.0010, 0.0005],
  "calibrated_delta_current_a": [0.0103, 0.0001, -0.0002],
  "target_current_a": [0.0123, 0.0000, 0.0000],
  "rf": {
    "start_hz": 2800000000,
    "stop_hz": 2900000000,
    "step_hz": 1000000,
    "dwell_ms": 300,
    "power_dbm": -30.0
  },
  "settle": {
    "policy": "fixed_delay_with_readback",
    "started_at": "2026-06-10T10:00:05.000Z",
    "settled_at": "2026-06-10T10:00:08.000Z",
    "status": "passed"
  }
}
```

point record 不保存 raw 数据本体，只保存该点的上下文。

## baseline_snapshot.json

当前 baseline snapshot 实际表达的是旧系统兼容的“零偏电流锁定”：

```json
{
  "schema_version": 1,
  "mode": "legacy_zero_offset_lock",
  "baseline_locked_at": "2026-06-11T10:00:00Z",
  "settle_ms": 1000,
  "readback_samples": 3,
  "settle_tolerance_a": 0.002,
  "axes": [
    {
      "axis": "mag_x",
      "zero_offset_setpoint_a": 0.0,
      "zero_offset_measured_samples_a": [0.00007, 0.00007, 0.00006],
      "locked_zero_offset_current_a": 0.0000667
    }
  ]
}
```

这里的 `locked_zero_offset_current_a` 是后续 point 叠加 `delta_current_a` 的基线，而不是物理零磁场已经被证明。

## point_fields.jsonl

每个 point 一行，表达 point 窗口里已经解析出的轻量 metadata：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "point_id": "p0001",
  "segment_id": "seg_p0001_0000",
  "frames_parsed": 104,
  "samples_total": 5200,
  "samples_per_frame": 50,
  "matrix_shape": [20, 5200],
  "measurement_field_order": [
    "A-X", "A-Y", "A-Freq", "A-Noise", "A-Xh1", "A-Yh1", "A-Xh2", "A-Yh2",
    "B-X", "B-Y", "B-Freq", "B-Noise", "B-Xh1", "B-Yh1", "B-Xh2", "B-Yh2",
    "AUXADC1", "AUXADC2", "AUXADC3", "AUXADC4"
  ],
  "measurement_field_keys": [
    "a_x", "a_y", "a_freq", "a_noise", "a_xh1", "a_yh1", "a_xh2", "a_yh2",
    "b_x", "b_y", "b_freq", "b_noise", "b_xh1", "b_yh1", "b_xh2", "b_yh2",
    "auxadc1", "auxadc2", "auxadc3", "auxadc4"
  ],
  "field_summaries": [
    { "field_name": "B-X", "npz_key": "b_x", "mean": 0.000000017 },
    { "field_name": "B-Freq", "npz_key": "b_freq", "mean": 499.9990 }
  ],
  "b_pll_locked_ratio": 1.0,
  "b_input_overload_ratio": 0.0,
  "b_gain_overload_ratio": 0.0,
  "last_b_ref_source_code": 0,
  "last_b_ref_slope_code": 2,
  "last_b_ref_current_freq_hz": 499.9990,
  "sidecar": {
    "format": "npz",
    "schema_version": 1,
    "relative_path": "point_fields/seg_p0001_0000.npz",
    "measurement_field_keys": ["a_x", "a_y", "a_freq", "a_noise"],
    "status_keys": [
      "frame_seq",
      "duplicate_hint",
      "b_ref_source_code",
      "b_ref_slope_code",
      "b_ref_current_freq_hz",
      "b_input_overload",
      "b_gain_overload",
      "b_pll_locked"
    ]
  }
}
```

这不是完整科学分析，只是 point 级轻量 metadata。  
完整 20 字段数组和必要状态数组默认进入每个 point 的 `NPZ` sidecar。

最终事实层是：

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`

当前默认策略：

- 默认不再把完整数组内联进 `point_fields.jsonl`
- 默认不再写 `frames.parsed`
- 长时 run 目录大小主要由 `raw/oe1022d.rall` 决定，而不是 JSON sidecar

## segments.jsonl

每个 segment 一行，表达连续流中的归属窗口：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "segment_id": "seg_p0001_0000",
  "point_id": "p0001",
  "source": "oe1022d_main",
  "start_ts": "2026-06-10T10:00:08.100Z",
  "end_ts": "2026-06-10T10:00:38.100Z",
  "start_monotonic_ns": 1000000000,
  "end_monotonic_ns": 31000000000,
  "raw_file": "raw/oe1022d.rall",
  "raw_offset_start": 1048576,
  "raw_offset_end": 2097152,
  "frame_seq_start": 120,
  "frame_seq_end": 745
}
```

segment 是 point 和 raw 的连接层。没有 segment 的 point 不能进入有效数据集。

在 sweep-driven runtime 中，segment 的 `start_ts/end_ts` 必须对应这次 point 的 `sweep_started/sweep_completed` 窗口，而不是固定 sleep。

## quality.jsonl

每个 point 至少一行质量摘要：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "point_id": "p0001",
  "segment_id": "seg_p0001_0000",
  "frames_total": 626,
  "frames_unique": 620,
  "duplicate_count": 6,
  "duplicate_ratio": 0.0096,
  "timeout_count": 0,
  "last_frame_age_ms": 42,
  "min_frames": 500,
  "estimated_frames_expected": 757,
  "frame_coverage_ratio": 0.8269,
  "collector_health": "clean",
  "timeout_budget_remaining": 2,
  "quality_status": "passed"
}
```

这里的 `estimated_frames_expected / frame_coverage_ratio` 只用于诊断：

- 它们帮助区分“真实采样空洞”和“缓存截断”
- 当前不会直接改变 `quality_status` 规则

允许的 `quality_status`：

- `passed`
- `failed_no_frames`
- `failed_min_frames`
- `failed_timeout`

`collector_health` 用来区分“point 可用性”和“collector 健康状态”：

- `clean`
- `recovered_timeout`
- `degraded_timeout`

## raw/oe1022d.rall

连续 raw 文件是 OE1022D 原始帧事实来源。第一版不要求格式复杂，但必须满足：

- append-only。
- frame 顺序和 collector sequence 一致。
- 不按 point 切碎。
- 可通过 index 定位 frame 范围。

## raw/oe1022d.frames.idx.jsonl

每个 frame 一行 index：

```json
{
  "frame_seq": 120,
  "ts": "2026-06-10T10:00:08.112Z",
  "monotonic_ns": 1012000000,
  "raw_offset": 1048576,
  "raw_len": 12288,
  "parse_status": "ok",
  "duplicate_of": null
}
```

## summary.json

```json
{
  "run_id": "test_001",
  "status": "completed_with_failed_points",
  "points_total": 15,
  "points_passed": 14,
  "points_failed": 1,
  "frames_total": 9400,
  "started_at": "2026-06-10T10:00:00Z",
  "ended_at": "2026-06-10T10:12:00Z",
  "failure": null
}
```

`summary.json` 是索引和概览，不是事实替代。分析应以 JSONL 和 raw 为准。
