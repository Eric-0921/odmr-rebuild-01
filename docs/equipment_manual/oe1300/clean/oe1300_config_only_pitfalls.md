# OE1300 仅作为配置选项引入——潜在风险与逻辑改动点分析

> 前提：你希望在“配置”层面多一个 OE1300 选项，其他内容尽量不做改动。本文件分析这一目标下必须面对的潜在坑，以及哪些逻辑即使不想改也**不得不**改。

---

## 一、关键结论（先说结论）

**只加一个配置字段很容易，但让 OE1300 真正可用一定需要改代码。**

如果你只希望在配置文件里先出现 `"device_model": "oe1300"`，而不触发任何功能变化，那么可以做到完全不影响现有逻辑——但前提是运行时 still 只识别 `"oe1022d"`，OE1300 只是一个**占位符**。

一旦希望 OE1300 被选中后能够实际运行，下列地方**至少**需要新增分支，不能简单复用 OE1022D 逻辑。

---

## 二、最大的坑：`RALL?` 返回格式完全不同

这是导致项目逻辑被大改的核心原因。

| 项目 | OE1022D | OE1300（手册描述） |
|------|---------|-------------------|
| `RALL?` 返回 | 12288 字节二进制帧 | ASCII 浮点 CSV 字符串 |
| 解析方式 | 按字节偏移解析 f64 数组 | 按逗号 split |
| 连续性审计 | `payload[12287]` packet counter | 无 packet counter |
| 读取方式 | write → sleep 30ms → blocking exact read 12288B | write → read-until-terminator |
| 文件大小 | `frames * 12288` 可校验 | 变长，无法按固定大小校验 |

### 会导致大改的具体模块

1. **`Odmr.Runtime` 采集循环**
   - 当前 `ConfigDrivenRun` 写死 `frame_exact_bytes = 12288`。
   - OE1300 需要改成按终止符读取 ASCII 行，逻辑完全不同。
   - 影响文件：`ConfigDrivenRun.cs`、`SweepOnlyRun.cs`、`Minimal3PointRun.cs`。

2. **`Odmr.Artifacts.ContinuityAudit`**
   - 当前依赖 `payload[12287]` 作为 device packet counter。
   - OE1300 没有 packet counter，需要改用时间戳或 frame index 做连续性判断。
   - 这是“冻结热路径”中的关键审计逻辑，改动会影响验收指标。

3. **`Odmr.Artifacts.ArtifactCheck`**
   - 当前校验 `raw/oe1022d.rall` 大小 = `frames * 12288`。
   - OE1300 变长 ASCII 无法做同样校验，需要改为按行/帧校验或仅检查文件存在。

4. **`tools/odmr-postprocess/extract_point_fields_from_rall.py`**
   - 当前按 12288B 二进制帧、固定偏移、大端 f64 解析。
   - OE1300 需要全新的 ASCII 解析器。

5. **AGENTS.md 中的冻结条款**
   - 当前明确禁止修改 `RALL?` 热路径，除非重新跑 60s/15min/重复 15min 连续性验证。
   - 引入 OE1300 必然需要新热路径，这意味着要重新建立稳定性验收基线。

---

## 三、第二大坑：命令集完全不同

OE1022D 命令带 `D` 后缀（`FMODD`、`ISRCD`、`SENSD`），OE1300 不带（`FMOD`、`ISRC`、`IRNG`）。

| 功能 | OE1022D | OE1300 |
|------|---------|--------|
| 参考源 | `FMODD i,j` | `FMOD {i}` |
| 频率 | `FREQD i,f` | `FREQ {f}` |
| 输入源 | `ISRCD i,j` | `ISRC {i}` |
| 灵敏度 | `SENSD i,j`（索引） | `IRNG {i}`（索引，但表不同） |
| 时间常数 | `OFLTD i,j`（索引） | `OFLT {x}`（秒值） |
| 陡降 | `OFSLD i,j` | `OFSL {i}` |
| 通道输出源 | `FPOPD j,k` | `COUT i,j` |
| IDN | `*IDND?` | `*IDN?` |

### 会导致的改动

1. **`Odmr.Devices.Oe1022dCommands`**
   - 全部命令字符串需要替换或新增 `Oe1300Commands`。
   - 不能通过字符串替换简单适配，因为参数数量和语义不同。

2. **`Odmr.Runtime.BuildOeFixedCommands`**
   - 当前把 OE1022D profile 字段一对一翻译成 `Oe1022dCommands`。
   - OE1300 需要完全不同的字段映射和命令生成逻辑。

3. **`DeviceCommandCatalog.cs` 白名单**
   - 当前只有 `oe1022d` 命令条目。
   - 需要新增 `oe1300` 白名单，否则 `device-command-check` 会失败。

4. **Python config-generator / console**
   - `normalize_oe_fixed` 全是 OE1022D 字段名。
   - UI 中“OE1022D 固定配置”字段与 OE1300 不同，需要动态渲染或拆分页面。

---

## 四、第三大坑：Profile Schema 不匹配

OE1022D profile 的 `fixed` 字段直接对应 OE1022D 命令参数：

```json
{
  "input_source": 0,
  "input_coupling": 0,
  "sensitivity_index": 18,
  "time_constant_index": 12,
  "filter_slope_index": 3,
  "sync_filter": 0
}
```

OE1300 的字段名和单位都不同：

- `time_constant` 不是索引，而是秒值（`1E-6 ~ 3000`）。
- 没有 `input_source` 的 100MΩ 电流档。
- 没有 `dynamic_reserve` 命令。
- 多了 `DMOD`、`HARM`、`DARB` 等多解调器配置。

### 最小影响的方案

如果只想加一个配置选项而不改现有 schema，可以：

1. 新建独立的 `oe1300_run_*.json` profile，字段按 OE1300 定义。
2. 在 `RunConfigBundle` 中把 `OeProfile` 从单一类型改为 `JsonElement` 或泛型，按 `device_model` 反序列化。
3. 这样 OE1022D 的 profile 完全不动，OE1300 走自己的解析路径。

**但这一步本身就属于“运行时 schema 改动”，不是纯配置改动。**

---

## 五、第四大坑：Station 识别方式

当前 station 靠 `"kind": "oe1022d"` 识别 OE 设备。如果新增 OE1300：

| 方案 | 配置改动 | 代码改动 | 风险 |
|------|---------|---------|------|
| A：`kind` 直接区分 `"oe1022d"` / `"oe1300"` | 小 | 中等 | 清晰，但所有 `kind == "oe1022d"` 判断要改为分支 |
| B：`kind = "oe"` + `device_model` | 中 | 较大 | 更抽象，但需要改 station schema 和 runtime 路由 |
| C：只加 `device_model`，`kind` 仍写 `"oe1022d"` | 最小 | 最小 | 最hack，但容易误导，且 identity 校验会失败 |

**推荐方案 A**：`kind` 直接写 `"oe1300"`，这样 `ResolveConnections` 可以按 `kind` 分支，逻辑最清晰。

---

## 六、第五大坑：Raw 文件名与 Artifact 结构

当前多处写死 `raw/oe1022d.rall` 和 `raw/oe1022d.frames.idx.jsonl`：

- `ConfigDrivenRun.cs`
- `SweepOnlyRun.cs`
- `Minimal3PointRun.cs`
- `ArtifactCheck.cs`
- `ContinuityAudit.cs`
- `LiveReplay.cs`
- `RallArtifacts.cs`

### 最小改动方案

把文件名改为按 `device_id` 生成：

```
raw/{device_id}.rall
raw/{device_id}.frames.idx.jsonl
```

这样 OE1022D 的设备 ID 仍可叫 `oe1022d_main`，OE1300 叫 `oe1300_main`，文件自然分开。

**这个改动相对独立，风险低，但涉及文件多。**

---

## 七、如果坚持“只做配置选项、其他不改”的最小方案

如果你真的只想在配置里多一个选项，而**暂时不让 OE1300 真正跑起来**，可以这样做：

1. 在 `configs/profiles/` 中新增 `oe1300_run_placeholder.json`。
2. 在 `configs/stations/` 中新增 `lab_a_oe1300_placeholder.json`，`kind = "oe1300"`。
3. **不做任何代码改动**。
4. 运行现有 `run-resolve` / `run-execute` 时，如果 station 中 `kind == "oe1300"`，会因为在 `ResolveConnections` 中无法匹配而直接报错。

这不是真正的“设备选项”，只是配置文件多了一个不会被识别的值。

---

## 八、真正“加一个设备选项”所需的最小代码改动集

如果你希望 OE1300 可选且可运行，但尽量不动 OE1022D 逻辑，下面是**最小改动清单**：

### 必须改的（ unavoidable ）

| 模块 | 改动内容 | 改动大小 |
|------|---------|---------|
| `Odmr.Devices` | 新增 `Oe1300Visa.cs`、`Oe1300Commands.cs` | 中 |
| `Odmr.Devices/DeviceCommandCatalog.cs` | 增加 `oe1300` 白名单条目 | 小 |
| `Odmr.Runtime/RunConfig.cs` | `OeProfile` 改为按 `device_model` 反序列化 | 中 |
| `Odmr.Runtime/RunConfig.cs` | `ResolveConnections` 按 `kind` 路由 | 小 |
| `Odmr.Runtime/ConfigDrivenRun.cs` | `BuildOeFixedCommands` 按 model 分支 | 中 |
| `Odmr.Runtime/*Run.cs` | raw/index 文件名参数化 | 小 |
| `Odmr.Runtime/*Run.cs` | `RALL?` reader 按 model 分支 | **大** |
| `Odmr.Artifacts/ContinuityAudit.cs` | 按 model 选择 packet counter 策略 | 中 |
| `Odmr.Artifacts/ArtifactCheck.cs` | 按 model 校验 raw 文件 | 小 |
| `tools/odmr-postprocess` | 新增 OE1300 ASCII 解析器 | 中 |

### 可以暂时不改的

- PySide6 Console 的 UI：可以先不支持 OE1300 图形配置，只用手写 JSON。
- config-generator：可以先用示例 JSON 手动编辑。
- WinForms ControlPanel：legacy UI，可暂不扩展。

---

## 九、给你的建议

如果你的真实需求是“**先让 OE1300 在配置里出现，但功能上先不支持**”，那么：

1. 只加 `configs/profiles/oe1300_*.json` 和 `configs/stations/lab_a_oe1300.json` 作为草案。
2. 在文档中明确说明：OE1300 尚未接入 runtime，选择它会报错。
3. 不改任何代码。

如果你希望“**配置里出现，并且选中后真的能跑**”，那么必须接受：

- 新增 `Oe1300Visa` + `Oe1300Commands`；
- 新增 `RALL?` ASCII reader；
- 修改连续性审计逻辑；
- 修改 Python 后处理。

这些改动中，**`RALL?` 采集循环和连续性审计是最大的两个逻辑改动点**，会直接触及当前项目最敏感的“冻结热路径”。

---

## 十、下一步建议

在决定是否进入代码改造之前，建议先做一件成本低但信息量大的事：

> **在真机上用串口/网口向 OE1300 发送 `RALL?`，捕获 10 帧原始返回。**

根据实际返回格式，再判断：
- 如果 OE1300 也支持某种固定长度二进制帧（与手册描述不符但存在隐藏协议），改动可能大幅缩小。
- 如果确实是变长 ASCII，那么“其他内容不做改动”的目标基本不可行。
