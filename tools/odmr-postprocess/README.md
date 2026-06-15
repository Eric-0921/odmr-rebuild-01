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

## 给网页版 GPT 判断 LI-ODMR 峰

加偏置小磁铁后，如果要把单次 run 整理给网页版 GPT 判断“偏置磁场过零点附近的共振峰/反对称结构”，使用：

```bash
python3 tools/odmr-postprocess/build_li_odmr_gpt_review.py \
  --run runs/<run_id> \
  --extract-missing-point-fields
```

输出：

```text
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>.csv
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>_metadata.csv
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>_metadata.jsonl
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>_summary.json
```

`li_odmr_gpt_review_<run_id>.csv` 是逐频点谱线明细。多 point run 会输出所有 point，例如 125 个 point、每条 641 个频点时，CSV 会有 `125 * 641 = 80125` 行。

`li_odmr_gpt_review_<run_id>_metadata.csv/jsonl` 是 point 级元数据，每行一个 point，包含 `target_b_nt`、RF sweep、激光、overload、PLL、sidecar 路径和折叠统计。

如果 Windows 端只有 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl`，没有 `point_fields.jsonl`，`--extract-missing-point-fields` 会先离线重建 point sidecar。

给网页版 GPT 时建议说明：

```text
这份数据包含多条 LI-ODMR 谱线。请按 point_id 分组，每个 point_id 是一条谱线；metadata 文件里有每条谱线对应的 target_bx_nt/target_by_nt/target_bz_nt。请重点看 frequency_ghz、b_x_smooth9_z、b_y_smooth9_z、b_r_smooth9_z 和 peak_hint_b_x_smooth9，判断每个 point 的 LI-ODMR 偏置磁场过零点附近共振峰/反对称结构。b_x_mean/b_y_mean 是 lock-in 原始输出，不是荧光强度。
```

注意：

- `B-X/B-Y` 是 lock-in 输出，不要按荧光强度做 `signal / baseline - 1`。
- 人眼和 GPT 优先看 `detrended`、`smooth9`、`smooth21` 和 `*_z` 列。
- `quality_status=passed` 只代表采集连续；必须同时看 `b_input_overload_ratio`、`b_gain_overload_ratio`、`b_pll_locked_ratio`。
- 如果 `b_input_overload_ratio` 或 `b_gain_overload_ratio` 非 0，不要当作正常谱线给训练或结论判断。

## 构建机器学习数据集

笛卡尔磁场扫描或多 point run 不应该直接使用 GPT review CSV 训练。GPT review CSV 面向“单条谱线人工判峰”；机器学习应使用一行一个 point 的样本表和定长矩阵：

```bash
python3 tools/odmr-postprocess/build_odmr_ml_dataset.py \
  --run runs/<run_id> \
  --extract-missing-point-fields
```

输出：

```text
runs/<run_id>/postprocess/ml_dataset_<run_id>.npz
runs/<run_id>/postprocess/ml_samples_<run_id>.csv
runs/<run_id>/postprocess/ml_samples_<run_id>.jsonl
runs/<run_id>/postprocess/ml_dataset_<run_id>_summary.json
```

`ml_dataset_<run_id>.npz` 是训练主输入：

```text
frequency_hz
point_id
target_b_nt
sample_weight
X_bx_smooth_z
X_by_smooth_z
X_br_smooth_z
X_bx_detrended
X_by_detrended
X_br_detrended
X_bx_mean
X_by_mean
X_br_mean
X_bnoise_mean
sample_count
```

`ml_samples_<run_id>.csv/jsonl` 是 point 级索引和质量表，包含 setpoint 标签、overload/PLL、SNR-like 指标、粗峰位、零交叉数量和对应 sidecar 路径。

训练建议：

- 优先使用 `X_bx_smooth_z`、`X_by_smooth_z`、`X_br_smooth_z` 作为第一版输入。
- 标签使用 `target_b_nt`，当前语义是 `helmholtz_setpoint_calibrated`。
- 用 `sample_weight` 过滤或降权 overload / PLL 不完整样本。
- train/test split 至少按 `source_run` 分组，后续再增加样品、日期、磁铁配置等 group 字段。
