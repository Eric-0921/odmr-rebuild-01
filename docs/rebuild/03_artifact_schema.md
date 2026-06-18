# Artifact Schema

Artifact 是当前 runtime 的正式事实层。当前合同已经固定为 direct-decode，不再把 `raw/oe1022d.rall`、`frames.idx` 或任何 `raw-truth` 文件当正式主路径。

## 一、正式事实层

两台锁相共用同一套 run 外壳，但 collector truth 按型号分开：

- `OE1022D`
  - `collector_frames.jsonl`
  - `parameter_values.csv`
  - `sample_values.csv`
  - `segments.jsonl`
- `OE1300`
  - `collector_blocks.jsonl`
  - `parameter_values.csv`
  - `sample_values.csv`
  - `segments.jsonl`

point / quality / resume / artifact-check / audit-continuity 都必须建立在这些 decoded truth 文件之上，不能回退到 raw 文件。

## 二、Run 目录

```text
runs/<run_id>/
  run_manifest.json
  station_snapshot.json
  calibration_snapshot.json
  smb_profile_snapshot.json
  oe_profile_snapshot.json
  laser_profile_snapshot.json
  plan_snapshot.json
  baseline_snapshot.json
  events.jsonl
  points.jsonl
  collector_frames.jsonl          # oe1022d
  collector_blocks.jsonl          # oe1300
  parameter_values.csv
  sample_values.csv
  segments.jsonl
  quality.jsonl
  device_state.jsonl
  summary.json
  continuity_audit.json           # 按需生成
  control/                        # PySide6/console 启动时存在
    progress.jsonl
    stop.request
    emergency_stop.request
    launch_metadata.json
    stdout.log
    stderr.log
```

运行时只会写当前型号对应的 collector 文件：

- `oe1022d` 只写 `collector_frames.jsonl`
- `oe1300` 只写 `collector_blocks.jsonl`

## 三、run_manifest.json

`run_manifest.json` 是本次 run 的静态索引，至少包含：

```json
{
  "schema_version": 1,
  "run_id": "x_axis_1d_bounce_15min",
  "created_at": "2026-06-18T02:33:51.0000000Z",
  "operator": "local",
  "station_id": "lab_a",
  "lockin_model": "oe1022d",
  "collector_contract": "write RALL? -> sleep 30ms -> exact read 12288B -> direct-decode -> append collector_frames + parameter_values + sample_values",
  "runtime_version": "0.1.0",
  "calibration_id": "lab_a_para_xml_inverse_coil_constant",
  "status": "running",
  "smb_profile_id": "smb100a_run_monitor_2830_2890_-10dbm",
  "oe_profile_id": "oe1022d_run_ch_b_observed",
  "laser_profile_id": "cni_laser_run_off_background",
  "plan_source_kind": "cartesian_grid",
  "resolved_point_count": 104,
  "estimated_run_duration_ms": 863200
}
```

当前允许的终态：

- `completed`
- `completed_with_failed_points`
- `failed`
- `paused`
- `aborted`
- `cleanup_failed`

`resume-run` 只接受 direct-decode 合同下的 run，不兼容历史 raw-truth run。

## 四、Snapshot 文件

以下文件都是 run 打开时冻结的输入副本：

- `station_snapshot.json`
- `calibration_snapshot.json`
- `smb_profile_snapshot.json`
- `oe_profile_snapshot.json`
- `laser_profile_snapshot.json`
- `plan_snapshot.json`

关键约束：

- `oe_profile_snapshot.json` 必须带 `model`
- `run_manifest.json`、`summary.json`、`control/progress.jsonl` 也必须带 `lockin_model`
- station 与 oe_profile 的型号必须一致；不允许 `lab_a.json + oe1300 profile` 这种交叉组合

## 五、events.jsonl

每行一个 runtime 事件。当前双型号共用同一套 point 级状态机，至少会出现：

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
- `run_paused`
- `run_aborted`

`ResumeRun` 判定一个 point 已完成时，固定看三重交集：

- `points.jsonl` 中存在该 `point_id`
- `quality.jsonl` 中该点 `quality_status == passed`
- `events.jsonl` 中存在 `point_completed`

## 六、points.jsonl

`points.jsonl` 记录 point 的实验上下文，不保存原始样本数组。每行至少包含：

- `point_id`
- `index`
- `point_kind`
- `magnetic_mode`
- `m8812_commanded`
- `target_b_nt`
- `baseline_current_a`
- `calibrated_delta_current_a`
- `target_current_a`
- `rf`
- `settle`

point 是否真正成功，不看 `points.jsonl` 单文件，而看 `points + quality + events` 的交叉结果。

## 七、Collector Truth

### 1. OE1022D: collector_frames.jsonl

每行对应一个 `12288 B` 设备帧，至少包含：

- `frame_seq`
- `ts`
- `monotonic_ns`
- `sample_index_start/end`
- `samples_per_frame`
- `device_packet_counter`
- `b_ref_source_code`
- `b_ref_slope_code`
- `b_ref_current_freq_hz`
- `b_input_overload`
- `b_gain_overload`
- `b_pll_locked`

冻结热路径：

```text
write RALL?
sleep 30ms
exact read 12288B
direct-decode
append collector_frames + parameter_values + sample_values
```

连续性审计仍以 `device_packet_counter` 为核心：

- `delta0 = 重复窗口`
- `delta1 = 正常新窗口`
- `delta_gt1 = 疑似漏窗口`

### 2. OE1300: collector_blocks.jsonl

每行对应一个 `32768 B` 网口 `RALL` 块，至少包含：

- `rall_index`
- `ts`
- `monotonic_ns`
- `sample_index_start/end`
- `unique_block`
- `unique_block_index`

冻结热路径：

```text
write RALL?\r
sleep 5ms
read until 32768B
detect unique block
decode 37 x 100 big-endian double for unique blocks only
append collector_blocks + unique-only parameter_values + unique-only sample_values
```

当前 OE1300 连续性不依赖 packet counter，而依赖：

- `rall_index` 连续
- `unique_block` 去重
- `timeout_count == 0`
- `raw_len_bad_count == 0`
- `decode_failures == 0`
- `effective_sample_hz_per_parameter >= 900`

## 八、parameter_values.csv

这是块级 / 帧级摘要表：

- `OE1022D`：每帧一行，主字段均值 + 关键状态
- `OE1300`：每个 `unique_block` 一行，`37` 个参数均值 + 状态区结构化字段

它是快速审阅表，不替代样本级真值。

## 九、sample_values.csv

这是样本级 decoded truth：

- `OE1022D`：每 `1 ms` 样本一行，`20` 个主字段展开
- `OE1300`：每个 `unique_block` 内按 `1 ms` 展开，`37` 个参数展开

`collector_blocks.jsonl` 仍保留所有 query 到的块，并通过 `unique_block` / `unique_block_index` 负责去重事实层；CSV 只保存去重后的有效样本。当前不再把状态区原始十六进制字节冗余写入 `collector_blocks.jsonl`。

point 级离线后处理默认直接读取：

- `sample_values.csv`
- `segments.jsonl`

不再要求“先保存 raw 才能分析”。

## 十、segments.jsonl

`segments.jsonl` 负责把 point 绑定到 decoded truth 窗口。每行至少包含：

- `segment_id`
- `point_id`
- `source`
- `start_ts/end_ts`
- `start_monotonic_ns/end_monotonic_ns`
- `source_file`
- `block_seq_start/end`
- `sample_index_start/end`

关键变化：

- 不再保存 `raw_file/raw_offset_start/raw_offset_end`
- 统一按 decoded collector 的序号和 sample index 绑定
- `source_file` 对 `OE1022D` 是 `collector_frames.jsonl`，对 `OE1300` 是 `collector_blocks.jsonl`

## 十一、quality.jsonl

每个 point 至少一行质量摘要。字段对两型号尽量同构，但判定逻辑按 collector 语义分支。

共同字段包括：

- `point_id`
- `segment_id`
- `frames_total`
- `frames_unique`
- `duplicate_count`
- `duplicate_ratio`
- `timeout_count`
- `min_frames`
- `collector_health`
- `quality_status`

当前正式 `quality_status`：

- `passed`
- `failed_no_frames`
- `failed_min_frames`
- `failed_timeout`
- `failed_decode`
- `failed_duplicate_only`

解释：

- `OE1022D` 的 `frames_total/unique` 是帧语义
- `OE1300` 的 `frames_total/unique` 实际是块语义，但字段名暂时保持不变，避免打散 point/quality 主链

## 十二、device_state.jsonl

每个 point 一行，钉住当次实验设备背景和 segment 绑定，至少包含：

- point 基本信息
- `target_b_nt / target_current_a / measured_current_a`
- `smb_profile_id`
- `smb_sweep`
- `smb_sweep_execution`
- `rf_exposure`
- `segment`
- `laser_profile_id`
- `laser_mode`
- `laser_power_mw`
- `oe_profile_id`

这里的 `segment` 只引用 decoded truth 的窗口范围，不引用 raw 偏移。

## 十三、summary.json

`summary.json` 是 run 级概览，不替代事实层，但必须把 collector 健康指标显式带出来。当前至少包含：

- `run_id`
- `status`
- `lockin_model`
- `collector_contract`
- `points_total`
- `points_passed`
- `points_failed`
- `frames_total`
- `samples_total`
- `started_at`
- `ended_at`
- `failure`
- `read_attempts`
- `timeout_count`
- `raw_len_bad_count`
- `decode_failures`
- `collector_frames_path`
- `collector_blocks_path`
- `parameter_values_path`
- `sample_values_path`
- `packet_counter`
- `query_hz`
- `unique_block_hz`
- `effective_sample_hz_per_parameter`

解释：

- `packet_counter` 只对 `OE1022D` 有意义
- `unique_block_hz`、`effective_sample_hz_per_parameter` 只对 `OE1300` 有意义
- `collector_frames_path` 与 `collector_blocks_path` 二选一

## 十四、Artifact Check 与 Audit

`artifact-check` 当前只承认 decoded truth 合同：

- `oe1022d` 要求 `collector_frames.jsonl`
- `oe1300` 要求 `collector_blocks.jsonl`

它不再要求：

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- 任何 raw replay 伴生产物

`audit-continuity` 也按型号分支：

- `oe1022d`：packet counter 连续性
- `oe1300`：块序连续性 + 去重后有效采样率

## 十五、历史说明

仓库里仍可能保留一些早期文档，描述 `raw/oe1022d.rall + frames.idx` 为正式真值层。那是旧合同，当前一律作历史参考，不再作为实现依据或验收口径。
