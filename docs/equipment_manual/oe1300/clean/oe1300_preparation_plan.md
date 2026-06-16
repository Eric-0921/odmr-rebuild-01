# 新增 OE1300 设备选项——前期准备计划（不改代码阶段）

> 目标：在不改动现有 C#/Python 代码的前提下，完成 OE1300 作为可选设备的所有文档、schema、真值表与迁移设计准备。

---

## 一、当前现状

OE1022D 是当前唯一支持的 OE 锁相放大器，且被深度硬编码：

- Station 中用 `"kind": "oe1022d"` + `"device_id": "oe1022d_main"` 识别。
- C# 中 `Oe1022dVisa`、`Oe1022dCommands`、`Oe1022dRunProfile` 直接写死。
- `RALL?` 热路径被冻结：write → sleep 30ms → blocking exact read 12288B → `payload[12287]` 作 packet counter。
- Python 后处理按 12288B 帧、固定偏移、大端 f64 解析。

因此新增 OE1300 不是“加一个配置项”，而是需要在代码层面引入 model-specific 路径。本阶段先完成所有不改代码的准备工作。

**网口状态更新**：复核手册第 7.2 节网口通讯截图后确认，OE1300/1351 Console 软件通过 **TCP Client 连接 `192.168.1.1:10001`**，PC 端应设为 `192.168.1.10/24`。拉取远端 LabVIEW 驱动后发现，原厂 `Configure Ethernet.vi` 使用串口发送 `NMOD;NIPA;NSMA;NGWA;NMAC` 序列，并等待 **25 s**。然而当前真机（固件 `Version:V1.3230310`）上，仅 `NMOD 0` 能在 6 s 后返回 `"Setting Ethernet Mode Success!"`，其余网络命令 10 s 内均无响应；`192.168.1.1:10001` 也无响应。因此**网口在当前固件上不可用**。

**串口状态更新**：早期探测误以为串口 ~1.5 s/命令，复测显示实际单条命令延迟仅 20–80 ms；100 次连续 `RALL?` 实测平均吞吐 **~18 Hz**。但进一步完整验证发现，当前固件 `V1.3230310` 的串口协议**基本只读**：可查询状态和读取 `RALL?`，但 `ISRC`/`FREQ`/`OFLT` 等 set 命令均无响应。因此第一版只能把 OE1300/1351 作为**固定配置的只读观察者**，实验前需通过前面板/Console 软件配置好设备（详见 `docs/equipment_manual/oe1300/clean/probe_reports/20260616_serial_probe.md`）。

---

## 二、前期准备的 7 个步骤

### Step 1：确认 OE1300 `RALL?` 的真实返回格式

这是所有技术决策的前提。OE1300 手册描述 `RALL?` 返回“逗号分隔的 ASCII 浮点字符串”，但需确认：

- [ ] 单条 `RALL?` 实际返回多少字节？是否固定长度？
- [ ] 返回字段顺序是否确实与手册表一致（0=X, 1=Y, 2=R, 3=θ, 4=XD1 … 36=AUX-IN2）？
- [ ] 是否有终止符（`\r` / `\n`）？
- [ ] 是否包含解调器 1–7 的数据？还是只返回当前使能的解调器？
- [ ] 连续两次 `RALL?` 之间是否需要延时？推荐延时是多少？
- [ ] 是否存在类似 OE1022D 的 packet counter 或其他连续性标识？

**行动**：
- 在真机上用串口/网口调试助手发送 `RALL?` 并抓取多帧原始数据。
- 或在 `tools/oe_rall_compare` 目录下新增 OE1300 探针脚本（仅探测，不改动主代码）。

**交付物**：
- `docs/equipment_manual/oe1300/oe1300_rall_frame_truth.md`
- 至少 10 帧原始捕获样本（脱敏后）

---

### Step 2：建立 OE1300 的 truth 文档

仿照 `truth/oe1022d/` 建立 `truth/oe1300/`：

- [ ] `truth/oe1300/command-truth.md`
  - 仅包含第一版所需的命令子集（参考、输入、滤波、输出、数据读取）。
  - 每条命令：helper name、raw command、arguments、response、units、preconditions、primary source、notes。
- [ ] `truth/oe1300/operational-notes.md`
  - 会话 stance、终止符、多命令、缓存区限制（64 byte）。
  - `RALL?` 采集规则（与 OE1022D 区分）。
  - 哪些功能暂不在第一版范围（如 PID、网口参数配置是否进入第一版）。
- [ ] `truth/oe1300/ambiguities-and-validation.md`
  - 手册中描述不清或疑似笔误的地方。
  - 真机验证结果与手册不符的地方。

**行动**：对照已校对的 `oe1300/clean/chapter_05.md` 提炼 truth。

---

### Step 3：扩展设备命令规格文档

更新 `docs/rebuild/04_device_command_specs.md`：

- [ ] 新增“OE1300 第一版命令”章节。
- [ ] 明确白名单命令（无 `D` 后缀）。
- [ ] 说明 OE1300 与 OE1022D 命令形态差异（命名、参数、索引表）。
- [ ] 说明 `RALL?` 返回格式差异。
- [ ] 若第一版 OE1300 只支持部分命令，明确 scope。

---

### Step 4：设计 Station / Profile Schema 的 device_model 字段

当前 station 中没有 `device_model`，仅靠 `kind` 硬编码。新增 OE1300 需要：

**方案 A（推荐）：在 profile 中增加 `device_model`**

```json
{
  "profile_id": "oe1300_run_ch_b_observed",
  "device_model": "oe1300",
  "fixed": { },
  "collector": { }
}
```

Station 中保持 `kind: "oe1300"`：

```json
{
  "device_id": "oe_main",
  "kind": "oe1300",
  "identity": { "contains_all": ["SSI OE1300"] }
}
```

**方案 B：在 station 中增加 `device_model`，profile 保持通用**

```json
{
  "device_id": "oe_main",
  "kind": "oe",
  "device_model": "oe1300"
}
```

**需要决策的点**：
- `kind` 是否保留 `"oe1022d"` / `"oe1300"` 直接区分，还是统一 `"oe"` + `device_model`。
- `collector.frame_exact_bytes` 在 OE1300 中是否仍然有意义？若 `RALL?` 是变长 ASCII，则该字段应改为 `rall_read_terminator` 或 `rall_max_line_bytes`。
- `rall_post_write_delay_ms` 是否仍适用？

**交付物**：
- `docs/rebuild/oe_device_model_schema_proposal.md`
- 示例 station 和 profile JSON（仅作为 schema 草案，不替换现有文件）

---

### Step 5：设计 `ILockInAmplifier` 抽象接口

为将来代码改造做准备，先定义接口契约：

```csharp
public interface ILockInAmplifier : IDisposable
{
    string ModelName { get; }
    Task<string> QueryIdnAsync(CancellationToken ct);
    Task SetReferenceSourceAsync(int channelOrDemod, int source, CancellationToken ct);
    Task SetReferenceFrequencyAsync(int channelOrDemod, double hz, CancellationToken ct);
    Task SetInputSourceAsync(int channelOrDemod, int source, CancellationToken ct);
    Task SetInputCouplingAsync(int channelOrDemod, int coupling, CancellationToken ct);
    Task SetSensitivityAsync(int channelOrDemod, int index, CancellationToken ct);
    Task SetTimeConstantAsync(int channelOrDemod, double secondsOrIndex, CancellationToken ct);
    Task SetFilterSlopeAsync(int channelOrDemod, int slope, CancellationToken ct);
    Task SetSyncFilterAsync(int channelOrDemod, int onOff, CancellationToken ct);
    Task<RallFrame> ReadRallAsync(CancellationToken ct);
}

public record RallFrame(
    byte[] RawBytes,
    long FrameIndex,
    DateTimeOffset Timestamp,
    IReadOnlyDictionary<string, double> Fields
);
```

**需要决策的点**：
- 方法参数用 `int channelOrDemod` 还是抽象掉通道概念？
- `RallFrame` 是否统一，还是 OE1022D/OE1300 各自有实现？
- 命令生成器是否也接口化？

**交付物**：
- `docs/rebuild/oe_lockin_interface_proposal.md`
- 接口伪代码 + 两种实现的映射说明

---

### Step 6：创建 OE1300 配置模板（草案）

不改现有配置文件，但在 `configs/profiles/` 旁新建草案目录：

```
configs/profiles/drafts/
  oe1300_run_ch_b_observed.json
configs/stations/drafts/
  lab_a_oe1300.json
```

模板中应包含：

- [ ] OE1300 fixed 配置字段：`ISRC`、`ICPL`、`IRNG`、`OFLT`、`OFSL`、`SYNC`、`FMOD`、`FREQ`、`RSLP`、`SWVT`、`SLVL`。
- [ ] Collector 配置：根据 Step 1 真机结果填写 `frame_exact_bytes` 或新的 `rall_read_mode` / `rall_max_line_bytes`。
- [ ] Station identity：`contains_all: ["SSI OE1300"]`。

**行动**：根据 `chapter_05.md` 和 truth 文件编写模板。

---

### Step 7：制定代码迁移路线图

把探索 agent 输出的“改动入口清单”整理为可执行的迁移计划：

- [ ] Phase 1：C# 设备层抽象
  - 新增 `ILockInAmplifier`、`Oe1022dLockIn`、`Oe1300LockIn`。
  - 新增 `Oe1300Visa.cs` 与 `Oe1300Commands.cs`。
  - `DeviceCommandCatalog` 增加 `oe1300` 白名单。
- [ ] Phase 2：Runtime 配置与路由
  - `RunConfigBundle.OeProfile` 改为按 `device_model` 反序列化。
  - `ResolveConnections` 按 `kind`/`device_model` 路由。
  - `BuildOeFixedCommands` 按 model 分支。
  - raw/index 文件名参数化。
- [ ] Phase 3：采集循环
  - OE1022D 保持 frozen binary exact-read。
  - OE1300 实现 ASCII line reader。
- [ ] Phase 4：Artifact / 审计
  - `ContinuityAudit` 按 model 选择 packet counter 策略。
  - `ArtifactCheck` 按 model 校验 raw 文件。
- [ ] Phase 5：Python 离线工具
  - config-generator 支持 model 选择。
  - odmr-console 动态渲染 OE 字段。
  - postprocess 增加 OE1300 解析器。
- [ ] Phase 6：文档与测试
  - 更新 AGENTS.md、04_device_command_specs.md、13_csharp_primary_stack.md。
  - 真机 smoke：OE1300 idn、RALL? 单帧、长跑 + artifact-check + audit-continuity。

**交付物**：
- `docs/rebuild/oe1300_migration_roadmap.md`

---

## 三、本阶段不改代码的交付物清单

| 交付物 | 位置建议 | 优先级 |
|--------|----------|--------|
| OE1300 `RALL?` 帧真值 | `docs/equipment_manual/oe1300/oe1300_rall_frame_truth.md` | P0 |
| OE1300 command truth | `truth/oe1300/command-truth.md` | P0 |
| OE1300 operational notes | `truth/oe1300/operational-notes.md` | P0 |
| OE1300 ambiguities | `truth/oe1300/ambiguities-and-validation.md` | P1 |
| 命令规格扩展 | `docs/rebuild/04_device_command_specs.md` 新增章节 | P0 |
| Station/Profile schema 提案 | `docs/rebuild/oe_device_model_schema_proposal.md` | P0 |
| `ILockInAmplifier` 接口提案 | `docs/rebuild/oe_lockin_interface_proposal.md` | P1 |
| OE1300 profile/station 草案 | `configs/profiles/drafts/oe1300_*.json`、`configs/stations/drafts/lab_a_oe1300.json` | P1 |
| 代码迁移路线图 | `docs/rebuild/oe1300_migration_roadmap.md` | P1 |

---

## 四、立即可以开始的 3 件事

1. **真机抓 `RALL?` 数据**：用串口助手或扩展 `tools/oe_rall_compare` 脚本，获取 OE1300 实际返回格式。
2. **写 command-truth 与 operational-notes**：基于已校对的 `chapter_05.md` 提炼。
3. **设计 device_model schema**：决定 `kind` 与 `device_model` 的组合方式，产出 schema 提案。

等以上 3 件事完成后，再进入代码改造阶段。
