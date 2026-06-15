# OE1300 vs OE1022D 对比分析（开发视角）

> 对比目的：当前 ODMR 重建项目以 OE1022D 为实际采集设备（`truth/oe1022d/command-truth.md`），OE1300 手册已完成校对。本文件从开发/协议/运行角度对比两者差异，为设备选型、驱动抽象、迁移评估提供输入。

---

## 一、建议的对比维度

从开发角度，建议按以下维度进行对比：

1. **硬件规格**：通道数、频率范围、接口、供电、物理形态。
2. **协议表面**：命令命名、终止符、多命令、缓存区、查询语法。
3. **配置指令**：输入、参考、滤波器、灵敏度/时间常数、输出、扫频、PID。
4. **采集指令**：连续采集路径（`RALL?`）差异、返回格式、帧结构。
5. **枚举值映射**：同名功能的参数编码是否一致。
6. **运行链影响**：对现有 C# `Odmr.Devices` / runtime / collector 的兼容或改动面。

---

## 二、硬件规格对比

| 维度 | OE1022D | OE1300 | 开发影响 |
|------|---------|--------|----------|
| 形态 | 台式，11 kg | 模块式，400 g | OE1300 更适合嵌入式/机柜集成 |
| 供电 | 220–240 V AC，50/60 Hz，典型 50 W | 12 V DC ±5%，18–24 W | OE1300 需直流电源，OE1022D 直接市电 |
| 信号输入 | 双通道 A/B | 单通道，8 个解调器 | OE1300 解调器维度替代了 OE1022D 的通道维度 |
| 电压满量程 | 1 nV – 1 V（1-2-5） | 1 nV – 5 V（1-2-5） | OE1300 上限更高 |
| 电流输入增益 | $10^6$ / $10^8\ \mathrm{V/A}$ | $10^6\ \mathrm{V/A}$ | OE1022D 有 100 MΩ 电流输入档 |
| 参考频率 | 1 mHz – 102 kHz | 1 μHz – 100 kHz / 500 kHz | OE1300 低频/高频范围都更宽 |
| 输入阻抗（电压） | $10\ \mathrm{M\Omega} \parallel 25\ \mathrm{pF}$ | $10\ \mathrm{M\Omega} \parallel 25\ \mathrm{pF}$ | 一致 |
| 共模抑制比 | >100 dB @ 10 kHz | >70 dB @ 10 kHz | OE1022D 更好 |
| 谐波检测 | 2F…nF 至 102 kHz（n<32,767） | 2F…nF 至 100/500 kHz（n<65,535） | OE1300 谐波阶数上限更高 |
| 时间常数 | 10 μs – 3 ks（<200 Hz 时） | 1 μs – 3 ks | OE1300 下限更低，陡降阶数更多（6–48 dB/oct） |
| 通信接口 | RS-232、隔离 USB2.0 | UART（RS232/TTL）、隔离 RJ45 千兆网口 | OE1300 多了网口，少了 USB |
| 辅助输入 | 4 通道 AUX In | 2 通道 AUX In | OE1022D AUX 通道更多 |
| 辅助输出 | CH1/CH2 + Monitor + AUXOUT | CH1/CH2 + AUXOUT | 基本相当 |
| 显示 | 5.6 英寸 TFT 屏 | 无屏（PC Console） | OE1300 完全依赖上位机 |

---

## 三、协议层面对比

| 维度 | OE1022D | OE1300 | 开发影响 |
|------|---------|--------|----------|
| 命令风格 | 非标准 SCPI，命令以 `D` 结尾，如 `FMODD` | 非标准 SCPI，命令无 `D` 后缀，如 `FMOD` | **命令字符串完全不兼容，需独立命令表** |
| 终止符 | `<lf>` 或 `<cr>` | `<cr>` | OE1300 更严格，只能回车 |
| 多命令分隔 | `;` | `;` | 一致 |
| 命令缓存 | 256 字符 | 64 字节 | **OE1300 对批量命令长度更敏感** |
| 查询标记 | 命令后加 `?` | 命令后加 `?` | 一致 |
| 参数分隔 | `,` | `,` | 一致 |
| 通道/解调器参数 | `i=1/2` 代表 A/B 通道 | 解调器索引 `0~7` | **语义完全不同** |
| 波特率 | 未在 truth 中强调 | 9600 / 115200 / 921600 | OE1300 串口可配置 |
| 网口 | 无 | TCP / DHCP，IP/掩码/网关可设 | OE1300 支持网络采集 |

---

## 四、核心配置指令对比

### 4.1 参考与相位

| 功能 | OE1022D | OE1300 | 备注 |
|------|---------|--------|------|
| 参考源 | `FMODD i,j`：0 外部 / 1 内部 / 2 内部扫频 | `FMOD {i}`：0 外部 / 1 内部 / 2 自参考 | OE1300 无通道参数；扫频由独立指令控制 |
| 频率 | `FREQD i,f` | `FREQ {f}` | OE1300 无通道参数 |
| 相位 | `PHASD i,x` | `PHAS i,x` | OE1300 中 `i` 是谐波通道 0–7 |
| 外部触发方式 | `RSLPD i,j`：0 TTL 上升沿 / 1 正弦过零 | `RSLP {i}`：0 TTL 上升 / 1 TTL 下降 / 2 Sine | OE1300 多了 TTL 下降沿 |
| 谐波 | `HARMD i,j,k` | `HARM i,j` | OE1022D 分谐波 1/2；OE1300 直接对 7 个解调器设阶数 |
| 扫频 | 专门一族：`SWTPD`、`SLLMD`、`SULMD`、`SSLLD`、`SSLGD`、`STLMD`、`SWRMD` | 未在命令表中出现 | OE1300 可能通过 `DMOD`/`DARB` 实现任意频率解调 |
| 任意频率解调 | 无独立命令 | `DMOD i,j` + `DARB i,f` | OE1300 特有的多解调器能力 |

### 4.2 输入与滤波器

| 功能 | OE1022D | OE1300 | 备注 |
|------|---------|--------|------|
| 输入源 | `ISRCD i,j`：0 A / 1 A-B / 2 1MΩ 电流 / 3 100MΩ 电流 | `ISRC {i}`：0 A / 1 A-B / 2 I | OE1022D 电流分两档；OE1300 仅一档 |
| 接地 | `IGNDD i,j`：0 Float / 1 Ground | — | OE1300 未见对应指令 |
| 耦合 | `ICPLD i,j`：0 AC / 1 DC | `ICPL {i}`：0 AC / 1 DC | 一致 |
| 陷波器 | `ILIND i,j`：0 关 / 1 50Hz / 2 50+100Hz / 3 100Hz | — | OE1022D 特有 |
| 动态储备 | `RMODD i,j`：0 Low Noise / 1 Normal / 2 High Reserve | — | OE1300 未见对应指令（可能由 `IRNG` 隐含） |
| 灵敏度 | `SENSD i,j`（索引表） | `IRNG {i}`（索引表） | 两者量程索引表不同 |
| 时间常数 | `OFLTD i,j`（索引表） | `OFLT {x}`（直接秒值） | **OE1300 用实际秒值，OE1022D 用索引** |
| 陡降 | `OFSLD i,j`：0–3（6/12/18/24 dB/oct） | `OFSL {i}`：0–7（6–48 dB/oct） | OE1300 陡降阶数更多 |
| 同步滤波 | `SYNCD i,j`：0 关 / 1 开 | `SYNC {i}`：0 关 / 1 开 | 一致 |

### 4.3 正弦输出

| 功能 | OE1022D | OE1300 | 备注 |
|------|---------|--------|------|
| 输出开关/类型 | `SWVTD i,j`：0 固定 / 1 Linear / 2 Log / 3 DC | `SWVT {i}`：0 关 / 1 开 | OE1022D 把扫幅/DC 模式做进一条指令；OE1300 仅开关 |
| 幅值 | `SLVLD i,x` | `SLVL {x}` | OE1300 无通道参数 |
| 扫幅参数 | `SVLLD`、`SVULD`、`SVSLD`、`SVSGD`、`SVTMD`、`SVRMD` | — | OE1300 未见扫幅指令 |
| 直流输出 | `SVDCD i,x` | — | OE1300 未见（但面板可能有） |

### 4.4 输出与公式

| 功能 | OE1022D | OE1300 | 备注 |
|------|---------|--------|------|
| 通道输出源 | `FPOPD j,k`：j=1/2 通道，k 选源 | `COUT i,j`：i=0/1 通道，j 选源 | 命令名与参数顺序都不同 |
| 偏置/放大 | `OEXPD j,k,x,l` | `COFP i,x`、`CEXP i,j` | OE1300 拆成两条指令 |
| AUXOUT | `CAUXD j,x` | `CAUX i,x` | 基本一致 |
| 公式 | `EQCDD`、`EQCSD` | — | OE1022D 特有 |

### 4.5 数据读取

| 功能 | OE1022D | OE1300 | 备注 |
|------|---------|--------|------|
| 单参数 | `OUTPD? i` | `OUTP? i` | 命令名只差一个 `D` |
| 多参数快照 | `SNAPD? i,j,...` | `SNAP? i,j,...` | 参数编码表几乎一致（0–36） |
| AUX 输入 | `OAUXD? i` | `OAUX? i` | 一致 |
| 全局数据 | `RALL?` → **12288 字节二进制帧** | `RALL?` → **ASCII 浮点字符串** | **最大差异：帧格式完全不同** |
| 过载 | `INOVD?` / `GNOVD?` | `OVLD?` | 命令名不同 |
| PLL 锁定 | `*PLLD?` | `*PLL?` | 一致 |
| IDN | `*IDND?` | `*IDN?` | OE1022D 多一个 `D` |
| 复位 | `*RSTD` | `*RST` | OE1022D 多一个 `D` |

---

## 五、`RALL?` 采集路径差异（关键）

| 维度 | OE1022D | OE1300 | 开发影响 |
|------|---------|--------|----------|
| 触发方式 | 发送 `RALL?` 后读取 | 发送 `RALL?` 后读取 | 一致 |
| 返回格式 | **二进制，固定 12288 B** | **ASCII 字符串，逗号分隔浮点数** | OE1300 无需二进制解析 |
| 帧结构 | 需按偏移量解析数组和配置 | 直接按顺序拆分 CSV | OE1300 解析更简单 |
| 性能 | USB2.0 专用高速路径 | UART/网口 | OE1300 网口可能更稳定 |
| 当前代码依赖 | `Odmr.Artifacts` 连续性审计依赖 `payload[12287]` 作为 packet counter | 无 packet counter | **连续性审计逻辑需重写** |
| 死线策略 | 当前冻结：write → sleep 30ms → blocking exact read 12288B | 可改为 read-until-terminator | OE1300 采集循环更简单 |

---

## 六、开发迁移评估

### 6.1 若继续用 OE1022D

- 保持现有 `truth/oe1022d/command-truth.md` 和 C# 命令白名单。
- 继续冻结 `RALL?` 热路径：30ms + exact 12288B read。
- 不需要读 OE1300 手册。

### 6.2 若要迁移到 OE1300

需要新建或扩展以下部分：

1. **命令白名单**：在 `docs/rebuild/04_device_command_specs.md` 中新增 OE1300 命令表。
2. **Helper 层**：新增 `Oe1300Commands`（无 `D` 后缀、64B 缓存、解调器索引 0–7）。
3. **Transport**：新增网口 TCP 会话（当前 OE1022D 用 USB/RS232）。
4. **采集循环**：重写 `RALL?` reader：从 binary exact-read 改为 ASCII line reader。
5. **Artifact 连续性审计**：OE1300 无 packet counter，需用时间戳/帧序号替代。
6. **配置映射**：把 OE1022D 的 `SENSD`/`OFLTD` 索引转换为 OE1300 的 `IRNG` 索引 / `OFLT` 秒值。
7. **Runtime**：解调器索引 0–7 替代通道 A/B；8 个解调器的数据字段更多。
8. **PySide6 Console**：设备选择、配置 UI、命令生成器需区分 OE1022D/OE1300。

### 6.3 若同时支持两者

建议做一层抽象：

- `ILockInAmplifier` 接口：reference source、input path、filter、output、acquisition。
- `Oe1022dLockIn` 与 `Oe1300LockIn` 两个实现。
- 配置文件中用 `device_model: oe1022d | oe1300` 区分。
- 命令白名单按型号隔离。

---

## 七、快速决策建议

| 场景 | 建议 |
|------|------|
| 当前主链只需稳定跑 ODMR | **继续用 OE1022D**，不迁移；OE1300 校对稿作为存档 |
| 需要更宽频率/更小体积/网口 | 评估 OE1300；迁移成本主要在采集循环和命令层 |
| 未来可能换设备 | 现在就在 `Odmr.Devices` 中引入 `ILockInAmplifier` 抽象 |
| 只做文档归档 | OE1300 校对稿已完成，可直接入库 |

---

## 八、参考文件

- OE1022D：`docs/equipment_manual/oe1022d/01_specifications.md`、`05_oe1022d_remote_programming_commands_55_74.md`、`truth/oe1022d/command-truth.md`
- OE1300：`docs/equipment_manual/oe1300/clean/chapter_01.md`、`chapter_05.md`
