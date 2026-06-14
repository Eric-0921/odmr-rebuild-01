# ODMR 后处理参考流程

本文把 NV 色心 ODMR + 机器学习论文里的数据处理思路，对齐到当前 rebuild artifact 合同，并定义第一版离线后处理参考程序。

目标不是把分析逻辑塞回采集线程，而是从已经落盘的事实层构建三类离线样本：

- `trace_raw`: 每个 point 的原始 `B-X/b_x` 时间序列，用于审计和重新切分。
- `spectrum_fit`: 保留 contrast 语义的频率谱，用于 Lorentzian/Voigt/多峰物理拟合。
- `spectrum_ml`: 固定频率网格和单谱归一化后的输入，用于 GPR/CNN/其他监督模型。

## 1. 论文对照

| 论文 | 相关思路 | 对当前工程的约束 |
|---|---|---|
| Tsukamoto et al., Scientific Reports 2022, Accurate magnetic field imaging using nanodiamond quantum sensors enhanced by machine learning, https://www.nature.com/articles/s41598-022-18115-w | 直接把完整 ODMR 光谱向量映射到磁场，使用机器学习绕开复杂物理模型拟合 | 必须保留每个 point 的完整谱线向量，不能只保存峰位或拟合结果 |
| Homrighausen et al., Sensors 2023, Edge-Machine-Learning-Assisted Robust Magnetometer, https://www.mdpi.com/1424-8220/23/3/1119 | 每个频点多次采样平均，使用谱线边缘点估计 offset，然后做归一化后送入 CNN | 需要输出频率 bin 级均值、边缘 baseline、offset removed signal 和 ML normalization |
| Yao et al., arXiv 2026, A Deep-Learning-Boosted Framework for Quantum Sensing with NV Centers, https://arxiv.org/html/2603.14728v1 | 1D-CNN 消费固定长度 ODMR 序列；频率轴和信号做归一化以增强对功率、收光效率波动的鲁棒性 | 训练输入和物理拟合输入要分层保存，Z-score 不能覆盖原始 contrast 信息 |
| Yamamoto et al., Applied Physics Express 2025, Nanodiamond quantum thermometry assisted with machine learning, https://arxiv.org/html/2504.07582v1 | GPR 可以在较少频点下估计温度；随机频率顺序可降低系统性热漂移 | 频率采样策略和频率轴必须作为样本元信息保存，不能只保存信号数组 |
| Fujisaku et al., ACS Measurement Science Au 2021, Machine-Learning Optimization of Multiple Measurement Parameters, https://pubs.acs.org/doi/10.1021/acsmeasuresciau.1c00009 | 用 ML 优化激光功率、微波功率、曝光等测量参数，目标是提高 contrast 和 SNR | 微波功率、激光功率、OE 参数是实验条件或协变量，不应混入谱线强度归一化 |
| Dushenko et al., Physical Review Applied 2020, Sequential Bayesian Experiment Design for ODMR of NV Centers, https://www.nist.gov/publications/sequential-bayesian-experiment-design-optically-detected-magnetic-resonance-nitrogen | 用序贯贝叶斯实验设计选择下一批频点，减少固定扫频测量时间 | 当前固定扫频可以先做 v0，但后续要保留替换为非均匀频率 grid 的空间 |

## 2. 当前 artifact 和论文基础数据的对应关系

| 论文需要的数据 | 当前 artifact | 说明 |
|---|---|---|
| 原始 ODMR 信号 | `raw/oe1022d.rall`, `point_fields/*.npz` 里的 `b_x` | `raw` 是最终事实层，`npz` 是 point 级结构化 sidecar |
| 频率轴 | `points.jsonl.rf`, `smb_profile_snapshot.json` | 后处理阶段物化为 `frequency_hz[]` |
| 微波功率 | `points.jsonl.rf.power_dbm`, SMB snapshot | 作为 condition metadata |
| dwell/扫描步进 | `points.jsonl.rf.dwell_ms`, `start_hz`, `stop_hz`, `step_hz` | 用于理论频率网格和样本数审计 |
| 采样时间轴 | `raw/oe1022d.frames.idx.jsonl`, `point_fields.samples_total`, 设备语义 `1ms/sample` | host poll interval 不能反推采样间隔 |
| 磁场标签 | `points.jsonl.target_b_nt` | 当前三轴亥姆霍兹线圈已经校验，setpoint 作为标准实际磁场标签 |
| 设备质量 | `quality.jsonl`, `b_pll_locked_ratio`, overload ratio, frame duplicate/timeout | 作为筛选或训练权重，不参与谱线强度归一化 |
| OE 设置 | `oe_profile_snapshot.json` | `time_constant_index/filter_slope` 需要在后处理中映射为物理量 |
| 激光功率 | `laser_profile_snapshot.json` | 作为 condition metadata |

结论：当前采集设计没有严重偏离论文范式。它没有只保存拟合结果，而是保留了连续 raw、frame index、segments、point sidecar 和设备快照。真正需要补的是离线样本构建层。

## 3. 展示数据和训练数据是否会不同

会不同，而且应该不同。

论文图里展示的通常是便于人理解的谱线：

- 多次平均后的平滑曲线。
- 已经减掉 offset 或 baseline 的曲线。
- 只截取关键频段。
- 只展示少数代表性磁场或温度条件。
- 可能为了图示统一做了垂直平移或幅值缩放。

训练中间数据更像工程张量：

- 固定长度 `N_freq` 的频率网格。
- 每条谱线单独 Z-score 或 max normalization。
- 额外拼接功率、激光、time constant、SNR、contrast、quality flag 等协变量。
- 保留 label、sample weight、group id、run id，避免数据泄漏。
- 对 bad/warn/good 样本做筛选或降权。

所以当前后处理必须同时产出至少两套信号。

对荧光强度型数据：

- `signal_fit = fluorescence / baseline - 1`: 保留 contrast，给物理拟合和可解释分析。
- `signal_ml_z = zscore(fluorescence - baseline)`: 去掉每条谱线的绝对幅值，给 ML baseline。

对当前 OE1022D `B-X/B-Y` lock-in 输出：

- 不使用 `b_x_mean / baseline - 1` 作为主视图，因为 lock-in X/Y 可以过零，baseline 可能接近 0。
- 使用 `b_x_detrended`、`b_y_detrended`、`b_r_detrended` 给人看。
- 使用 `b_x_smooth9_z`、`b_y_smooth9_z`、`b_r_smooth9_z` 给网页版 GPT 或机器判断峰型。

原始 `b_x` 不能被覆盖。

## 4. 标签定义

当前工程采用：

```text
label_b_nt = points.jsonl.target_b_nt
label_source = helmholtz_setpoint_calibrated
```

理由：

- 三轴亥姆霍兹线圈已经经过校验。
- 运行时已把 `target_b_nt -> calibration -> target_current_a -> MEAS:CURR?` 链路落盘。
- 对监督学习来说，当前 setpoint 可作为标准实际磁场标签。

边界：

- 这不是说每个 point 都有外部磁强计实时闭环读数。
- 如果未来接入外部磁强计，新增 `label_source = external_magnetometer`，不要破坏现有标签字段。

## 5. v0 后处理流程

输入 run 目录：

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

输出目录：

```text
runs/<run_id>/postprocess/
  odmr_spectra.csv
  odmr_samples_manifest.jsonl
  odmr_dataset_summary.json
```

处理步骤：

1. 读取 `points.jsonl`，拿到 `point_id`、`target_b_nt` 和 RF sweep 参数。
2. 读取 `quality.jsonl` 和 `point_fields.jsonl`，连接到 sidecar `npz`。
3. 从 `npz` 读取 `b_x` 数组，作为该 point 的原始 ODMR trace。
4. 按 `start_hz/stop_hz/step_hz` 构建理论 `frequency_hz[]`。
5. 按 `dwell_ms` 和 `sample_interval_ms=1` 把 trace 聚合成频率 bin 均值。
6. 用谱线两端频点估计 baseline。
7. 输出 `signal_fit` 和 `signal_ml_z`。
8. 输出每个 point 的质量和对齐 warning。

## 6. 频率轴对齐的保守策略

第一版程序只做理论重建，不做隐式相位校正：

```text
frequency_index = floor((sample_index - sweep_offset_ms) / dwell_ms)
frequency_hz = start_hz + frequency_index * step_hz
```

如果 `samples_total` 和 `N_freq * dwell_ms` 不一致，程序只输出 warning，不自动把谱线拉伸或重采样。

这点很重要：频率轴错位是后续拟合和训练最大的系统误差来源之一，不能在 v0 里静默修正。

## 7. 后续应该补的内容

- 建立 OE `time_constant_index -> time_constant_ms` 和 `filter_slope -> dB/oct` 映射表。
- 明确 `SWE:FREQ:EXEC` 到真实 RF sweep 起点的触发延迟审计方法。
- 生成固定长度 `.npz` 或 `.parquet` 训练集，避免长期依赖 CSV。
- 增加 group split 字段，例如 `run_id/date/sample_id`，防止同一 run 的相邻点同时进入 train/test。
- 在积累足够数据后比较三条路线：多峰拟合、GPR、小型 1D-CNN。

## 8. 加偏置小磁铁后的网页版 GPT 整理流程

用于快速让网页版 GPT 判断 LI-ODMR 偏置磁场过零点附近的共振峰或反对称结构：

```bash
python3 tools/odmr-postprocess/build_li_odmr_gpt_review.py \
  --run runs/<run_id> \
  --extract-missing-point-fields
```

该工具会：

1. 检查 `point_fields.jsonl` 和 `point_fields/*.npz` 是否存在。
2. 如果缺失，用 `raw/oe1022d.rall + raw/oe1022d.frames.idx.jsonl + segments.jsonl` 离线重建 sidecar。
3. 从 `points.jsonl` 和 SMB snapshot 构建 `frequency_hz/frequency_ghz`。
4. 按 `dwell_ms / 1ms` 把 `B-X/B-Y/B-Noise` 折叠成每个频点的均值。
5. 对 `B-X/B-Y/R=sqrt(X^2+Y^2)` 做线性去趋势。
6. 输出 `smooth9`、`smooth21` 和 robust Z-score 列。
7. 输出 `peak_hint_b_x_smooth9`，用于快速定位局部极大/极小。

输出文件：

```text
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>.csv
runs/<run_id>/postprocess/li_odmr_gpt_review_<run_id>_summary.json
```

给网页版 GPT 的提示词可以直接写：

```text
请重点看 frequency_ghz、b_x_smooth9_z、b_y_smooth9_z、b_r_smooth9_z 和 peak_hint_b_x_smooth9，判断 LI-ODMR 偏置磁场过零点附近的共振峰/反对称结构。b_x_mean/b_y_mean 是 lock-in 原始输出，不是荧光强度。quality_status 只代表采集连续，还要看 overload ratio。
```

有效性边界：

- `b_input_overload_ratio` 必须接近 `0`。
- `b_gain_overload_ratio` 必须接近 `0`。
- `b_pll_locked_ratio` 应接近 `1`。
- 如果这些条件不满足，CSV 仍可用于排查设备状态，但不应当给出峰位物理结论。
