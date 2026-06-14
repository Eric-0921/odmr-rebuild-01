# ODMR postprocess reference

这个目录放离线后处理参考程序。它只读取 run artifact，不修改采集数据，不进入 runtime 热路径。

## 输入

程序默认读取：

```text
runs/<run_id>/
  points.jsonl
  quality.jsonl
  point_fields.jsonl
  point_fields/*.npz
  smb_profile_snapshot.json
  oe_profile_snapshot.json
  laser_profile_snapshot.json
  calibration_snapshot.json
```

`points.jsonl.target_b_nt` 默认作为标准磁场标签：

```text
label_source = helmholtz_setpoint_calibrated
```

这是因为当前三轴亥姆霍兹线圈已经校验，setpoint 可以作为实际磁场标准。

## 输出

默认输出到 `<run>/postprocess/`：

```text
odmr_spectra.csv
odmr_samples_manifest.jsonl
odmr_dataset_summary.json
```

`odmr_spectra.csv` 是频率 bin 级数据：

```text
point_id, frequency_hz, b_x_mean, b_x_std, signal_fit, signal_ml_z, label_bx_nt, label_by_nt, label_bz_nt
```

`odmr_samples_manifest.jsonl` 是 point 级质量、对齐 warning 和 provenance。

## 用法

```bash
python3 tools/odmr-postprocess/build_odmr_samples.py --run runs/<run_id>
```

如果已知 sweep 触发后有固定延迟，可以显式指定：

```bash
python3 tools/odmr-postprocess/build_odmr_samples.py \
  --run runs/<run_id> \
  --sweep-offset-ms 0
```

如果要换标签来源：

```bash
python3 tools/odmr-postprocess/build_odmr_samples.py \
  --run runs/<run_id> \
  --label-source external_magnetometer
```

## 设计边界

- 不解析裸 `raw/oe1022d.rall`，优先消费当前 runtime 已生成的 point sidecar。
- 不静默校正频率轴错位，只输出 warning。
- 不把 `quality` 和设备状态混进谱线归一化。
- 同时输出 `signal_fit` 和 `signal_ml_z`，避免训练归一化覆盖物理 contrast。
