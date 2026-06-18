# Run 数据读取指南

本文给后续 agent 使用，说明如何读取 `runs/<run_id>` 里的实验数据，以及不同分析任务应该选哪个入口。

## 1. 先判断要读哪一层

优先级从高到低：

| 目标 | 推荐入口 | 说明 |
|---|---|---|
| 给人或 GPT 看 LI-ODMR 谱线 | `postprocess/li_odmr_gpt_review_<run_id>.csv` + `_metadata.csv` | 已经按频率 bin 聚合、去趋势、平滑、Z-score |
| 逐条看多 point 谱线和图片 | `postprocess/li_odmr_gpt_review_by_point/` | 每个 point 一个 CSV 和一张谱线-频率图 |
| 机器学习训练 | `postprocess/ml_dataset_<run_id>.npz` + `ml_samples_<run_id>.csv` | 定长矩阵 + point 级标签/质量 |
| 重新做后处理 | `sample_values.csv` + `segments.jsonl` + `points.jsonl` | 当前正式 decoded truth |
| 快速看运行质量 | `summary.json` + `quality.jsonl` | 不能替代样本级数据 |

不要把 `raw/oe1022d.rall`、`raw/oe1022d.frames.idx.jsonl` 或旧 `point_fields/*.npz` 当当前主入口。当前正式事实层是 direct-decode：

```text
sample_values.csv + segments.jsonl
```

## 2. Run 目录里每个文件的作用

关键文件：

```text
runs/<run_id>/
  run_manifest.json
  points.jsonl
  quality.jsonl
  segments.jsonl
  sample_values.csv
  parameter_values.csv
  collector_frames.jsonl      # OE1022D
  collector_blocks.jsonl      # OE1300
  smb_profile_snapshot.json
  oe_profile_snapshot.json
  laser_profile_snapshot.json
  calibration_snapshot.json
  summary.json
  postprocess/
```

`points.jsonl`：

- 一行一个 point。
- 保存 `point_id`、`target_b_nt`、RF sweep 参数、磁场电流设定等实验上下文。
- `target_b_nt` 是当前监督学习标签，语义为 `helmholtz_setpoint_calibrated`。

`segments.jsonl`：

- 一行一个 point 的数据窗口。
- 用 `sample_index_start` / `sample_index_end` 把 point 绑定到 `sample_values.csv` 的全局样本区间。

`sample_values.csv`：

- 样本级 decoded truth。
- OE1022D 每个唯一帧展开为 `1 ms/sample` 的 20 字段数据。
- 常用字段包括 `global_sample_index`、`b_x`、`b_y`、`b_noise`、`b_input_overload`、`b_gain_overload`、`b_pll_locked`。

`quality.jsonl`：

- 一行一个 point 的采集质量。
- `quality_status=passed` 只代表采集连续性，不代表谱线一定适合训练。
- 训练或判图还要看 overload 和 PLL 字段。

`postprocess/`：

- 后处理产物目录。
- 这里的 CSV/NPZ 通常是最适合给人、GPT 或 ML 直接使用的入口。

## 3. 最小读取示例：按 point 读取 Channel-B-X

如果需要从事实层重新读取某个 point 的原始 `Channel-B-X` 时间序列，使用：

```python
from pathlib import Path
import csv
import json

run = Path("runs/<run_id>")

segments = {}
with (run / "segments.jsonl").open("r", encoding="utf-8") as f:
    for line in f:
        row = json.loads(line)
        segments[row["point_id"]] = row

point_id = "x_line_p000001"
seg = segments[point_id]
start = int(seg["sample_index_start"])
end = int(seg["sample_index_end"])

bx = []
by = []
bnoise = []
with (run / "sample_values.csv").open("r", encoding="utf-8", newline="") as f:
    reader = csv.DictReader(f)
    for row in reader:
        index = int(row["global_sample_index"])
        if index < start:
            continue
        if index >= end:
            break
        bx.append(float(row["b_x"]))
        by.append(float(row["b_y"]))
        bnoise.append(float(row["b_noise"]))
```

也可以直接复用已有 helper：

```python
from pathlib import Path
from tools.odmr_postprocess.decoded_truth import load_point_series_map

run = Path("runs/<run_id>")
series = load_point_series_map(run, ["b_x", "b_y", "b_noise"])
bx = series["x_line_p000001"]["b_x"]
```

注意：上面第二段示例在脚本内运行时更推荐直接把 `tools/odmr-postprocess` 加到 `sys.path`，因为目录名里有连字符，不能按普通 Python package 名直接 import。

## 4. 从原始时间序列变成频率谱

频率轴来自 RF 参数：

```text
points.jsonl.rf 或 smb_profile_snapshot.json.default_sweep
start_hz / stop_hz / step_hz / dwell_ms
```

处理逻辑：

```text
freq_count = floor(abs(stop_hz - start_hz) / abs(step_hz)) + 1
samples_per_freq = dwell_ms / 1 ms
每个 point 的 b_x 时间序列按 freq_count * samples_per_freq 折叠
对每个频点取均值和标准差
```

如果 point 里有多个完整 sweep，当前后处理会把完整 sweep 一起平均；尾部不足一个完整 sweep 的样本只记录为 `leftover_samples_ignored`，不强行拉伸频率轴。

## 5. 给 GPT 或导师看的入口

先生成：

```bash
python3 tools/odmr-postprocess/build_li_odmr_gpt_review.py \
  --run runs/<run_id>
```

输出：

```text
postprocess/li_odmr_gpt_review_<run_id>.csv
postprocess/li_odmr_gpt_review_<run_id>_metadata.csv
postprocess/li_odmr_gpt_review_<run_id>_metadata.jsonl
postprocess/li_odmr_gpt_review_<run_id>_summary.json
```

多 point run 中：

- 主 CSV 是逐频点明细。
- 必须按 `point_id` 分组。
- metadata CSV 是一行一个 point，记录标签和质量。

如果要拆成每个 point 一个 CSV 和图片：

```bash
python3 tools/odmr-postprocess/split_li_odmr_gpt_review_by_point.py \
  --postprocess runs/<run_id>/postprocess
```

输出：

```text
postprocess/li_odmr_gpt_review_by_point/
  csv/
  plots/
  manifest.csv
  summary.json
```

## 6. 给机器学习用的入口

先生成：

```bash
python3 tools/odmr-postprocess/build_odmr_ml_dataset.py \
  --run runs/<run_id>
```

输出：

```text
postprocess/ml_dataset_<run_id>.npz
postprocess/ml_samples_<run_id>.csv
postprocess/ml_samples_<run_id>.jsonl
postprocess/ml_dataset_<run_id>_summary.json
```

推荐第一版输入：

```text
X_bx_smooth_z
X_by_smooth_z
X_br_smooth_z
```

标签：

```text
target_b_nt
```

质量/权重：

```text
sample_weight
ml_samples_<run_id>.csv 里的 b_input_overload_ratio / b_gain_overload_ratio / b_pll_locked_ratio
```

加载示例：

```python
import numpy as np

data = np.load("runs/<run_id>/postprocess/ml_dataset_<run_id>.npz")

X = np.stack(
    [
        data["X_bx_smooth_z"],
        data["X_by_smooth_z"],
        data["X_br_smooth_z"],
    ],
    axis=1,
)
y = data["target_b_nt"]
frequency_hz = data["frequency_hz"]
sample_weight = data["sample_weight"]
```

`X` 的 shape 是：

```text
[n_points, 3, n_freq]
```

## 7. 常见坑

1. 不要把整个多 point CSV 当成一条连续谱线。

`li_odmr_gpt_review_<run_id>.csv` 必须按 `point_id` 分组。

2. 不要只看 `quality_status=passed`。

还要看：

```text
b_input_overload_ratio
b_gain_overload_ratio
b_pll_locked_ratio
```

3. 不要把 `B-R` 当主 LI-ODMR 过零信号。

当前主分析信号是：

```text
Channel-B-X / b_x
```

`B-Y` 和 `B-R` 是辅助通道。`B-R = sqrt(B-X^2 + B-Y^2)` 会丢掉正负号，不适合作为过零点主信号。

4. 不要在训练时随机打散同一个 run 的邻近点后宣称泛化。

3D 笛卡尔扫描中的邻近 point 很相似。验证时至少要尝试：

```text
leave-one-bz-plane-out
leave-one-bx-level-out
leave-one-by-level-out
```

5. 不要回退到 raw-truth 旧合同。

除非明确在处理历史 run，否则当前 agent 应该基于：

```text
sample_values.csv
segments.jsonl
points.jsonl
quality.jsonl
```

以及 `postprocess/` 中已经生成的衍生产物。
