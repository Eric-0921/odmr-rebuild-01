# Artifact Schema

Artifact 是 run 的事实来源。第一版使用普通文件和 JSONL，便于 grep、replay、Python 分析和人工审查。

## Run 目录

```text
runs/<run_id>/
  run_manifest.json
  station_snapshot.json
  calibration_snapshot.json
  plan_snapshot.json
  events.jsonl
  points.jsonl
  point_fields.jsonl
  segments.jsonl
  quality.jsonl
  raw/
    oe1022d.rall
    oe1022d.frames.idx.jsonl
  summary.json
```

所有 snapshot 是执行前输入的冻结副本。run 过程中不读取可变原始配置作为事实来源。

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
  "status": "running"
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
- `segment_completed`
- `point_completed`
- `point_failed`
- `cleanup_started`
- `cleanup_completed`
- `collector_stopped`
- `run_completed`
- `run_failed`

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

## point_fields.jsonl

每个 point 一行，表达 point 窗口里已经解析出的最小字段级数据：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "point_id": "p0001",
  "segment_id": "seg_p0001_0000",
  "frames_parsed": 104,
  "samples_total": 5200,
  "matrix_shape": [20, 5200],
  "measurement_field_order": [
    "A-X", "A-Y", "A-Freq", "A-Noise", "A-Xh1", "A-Yh1", "A-Xh2", "A-Yh2",
    "B-X", "B-Y", "B-Freq", "B-Noise", "B-Xh1", "B-Yh1", "B-Xh2", "B-Yh2",
    "AUXADC1", "AUXADC2", "AUXADC3", "AUXADC4"
  ],
  "b_x_mv": [0.000000081, 0.000000079],
  "b_y_mv": [0.000000004, 0.000000005],
  "b_freq_hz": [499.9991, 499.9989],
  "b_noise_mv": [0.000000067, 0.000000068],
  "aux_adc1_v": [0.0026, 0.0027],
  "b_x_mean_mv": 0.000000017,
  "b_freq_mean_hz": 499.9990,
  "b_pll_locked_ratio": 1.0,
  "b_input_overload_ratio": 0.0,
  "b_gain_overload_ratio": 0.0,
  "last_b_ref_source_code": 0,
  "last_b_ref_slope_code": 2,
  "last_b_ref_current_freq_hz": 499.9990
}
```

这不是完整科学分析，只是 point 级最小字段产物。最终事实仍然是 `raw/oe1022d.rall + segments.jsonl`。

当前已观察到的现实问题：

- 直接把完整字段数组写进 `point_fields.jsonl`，体积会很快膨胀
- `manual_live_v5` 仅 `3` 个 point，`point_fields.jsonl` 已约 `2.2 MB`
- 因此这份 artifact 目前只能视为“字段级验证基线”，不能直接原样沿用到长时 run

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

## quality.jsonl

每个 point 至少一行质量摘要：

```json
{
  "schema_version": 1,
  "run_id": "test_001",
  "point_id": "p0001",
  "segment_id": "seg_p0001_0000",
  "frames_total": 626,
  "frames_valid": 620,
  "frames_unique": 620,
  "duplicate_count": 6,
  "duplicate_ratio": 0.0096,
  "timeout_count": 0,
  "parse_error_count": 0,
  "last_frame_age_ms": 42,
  "min_frames": 500,
  "quality_status": "passed"
}
```

允许的 `quality_status`：

- `passed`
- `failed_no_frames`
- `failed_min_frames`
- `failed_timeouts`
- `failed_parse_errors`
- `failed_settle`
- `failed_device_error`

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
