# OE1300 网口 `RALL` 采样验证记录

本文档记录当前 `oe1300-net-labview-demo` 的实测结论，目标不是描述协议本身，而是回答三个问题：

1. 当前 collector/demo 的不可改变流程是什么？
2. 实际有效采样率是多少？
3. 解析后的样本是否会对真实外部输入产生响应？

## 1. 当前唯一保留的验证入口

当前仓库只保留一个 OE1300 网口采样验证入口：

- `tools/win-csharp/Odmr.WinProbe`
- command: `oe1300-net-labview-demo`

保留它的原因是：

- 已按原厂 LabVIEW 程序框图对齐
- 已完成 `60 s` 与 `15 min` 长跑验证
- 已完成配合本机 USB `SMB100A` 的受控输入验证

已删除的旧入口：

- `oe1300-net-collector-demo`
- `oe1300-net-raw-analyze`
- `oe1300-net-outp-demo`
- `oe1300-net-ascii-demo`

这些入口是中间探索路径，不再作为当前设备层事实。

## 2. 不可改变的 collector/demo 流程

当前固定流程如下：

```text
write RALL?\r
sleep 5 ms
read until 32768 B
decode current block
append parsed values
next round
```

这里明确禁止的改动：

- 默认路径加入 `drain-before-write`
- 加入额外 `poll interval`
- 改成 `producer + consumer` 双线程
- 先写本地 raw 二进制、再离线 parse
- 在 collector/demo 热路径里插入复杂 retry / deadline / replay 逻辑

当前阶段之所以这样冻结，是因为我们要验证的是：

- 设备层是否能稳定拿到新块
- 新块的解析后样本是否能达到约 `1 kHz`
- 解析结果是否响应真实输入

因此当前链路优先选择：

- 单线程
- 即时解码
- 直接保存解析后事实

而不是原始二进制归档优先。

## 3. 当前实现模型

当前实现位于：

- [Program.cs](/Users/erictseng/Documents/odmr-rebuild-01/tools/win-csharp/Odmr.WinProbe/Program.cs)

真实行为是：

- 单线程 loop
- 每次读到 `32768 B` 后立即调用 `Oe1300Parsers.DecodeTcpRallLabviewNamedSeries(payload)`
- 同步计算：
  - 预览参数的 `100` 个样本
  - `37` 个参数的块均值
  - `status_hex`
  - `status_byte`
  - `trig_count`
  - 整个 `3168 B` 状态区的 `status_zone_hex / status_zone_sha256`
- 直接写：
  - `collector_blocks.jsonl`
  - `parameter_values.csv`
  - `sample_values.csv`
  - `preview_values.csv`（仅 `--write-values true`）
  - `summary.json`

当前 **不保留独立 raw 二进制文件**。状态区原始事实保留在 `collector_blocks.jsonl` 的 `status_zone_hex` 中。

这符合当前阶段的工程选择：直接处理并保存“解析后原始事实”，而不是先堆本地 raw 再决定如何解释。

## 4. 60 秒验证结论

实测结果：

- `ralls_ok = 3808`
- `query_hz = 63.46`
- `timeout_count = 0`
- `raw_len_bad_count = 0`
- `decode_failures = 0`

若只按主机查询频率机械折算：

- `63.46 * 100 = 6345.82 samples/s/parameter`

但这不是最终真值，因为大量相邻块内容重复。

对 `parameter_values.csv` 做块级去重后：

- `adjacent_duplicate_blocks = 3211`
- `adjacent_duplicate_ratio = 84.34%`
- `adjacent_unique_runs = 597`
- `unique_blocks_total = 597`
- `effective_sample_hz_per_parameter ≈ 1000.89`

结论：

- 主机 `RALL?` 次数远高于真实新块到达频率
- 真正有效的新块频率约 `10 Hz`
- 每个新块含 `100` 个样本
- 所以最终有效采样率约 `1.00 kHz/parameter`

## 5. 15 分钟长跑结论

实测结果：

- `ralls_ok = 57500`
- `query_hz = 63.89`
- `timeout_count = 0`
- `raw_len_bad_count = 0`
- `decode_failures = 0`
- `last_status_hex = 0000`

块级去重结果：

- `adjacent_duplicate_blocks = 48504`
- `adjacent_duplicate_ratio = 84.36%`
- `adjacent_unique_runs = 8996`
- `unique_blocks_total = 8996`
- `run_length_mean = 6.39`
- `run_length_max = 7`
- `effective_unique_sample_hz ≈ 999.55`

结论：

- `60 s` 与 `15 min` 的重复块比例基本一致
- `15 min` 下的有效采样率仍稳定在约 `1.00 kHz/parameter`
- 当前流程在长跑中未出现退化

## 6. `SMB100A` 受控输入验证

本次验证不是只看采样率，还验证了解析结果会不会对真实输入变化产生响应。

### 6.1 设备事实

本机 USB 设备中：

- `/dev/cu.usbserial-B0027SH3` 实际是 `OE1301`
- 本机 `SMB100A` 的真实控制资源是：
  - `USB0::2733::84::106789::0::INSTR`

`*IDN?` 返回：

- `Rohde&Schwarz,SMB100A,1406.6000k02/106789,3.0.13.0-2.20.382.70`

### 6.2 受控窗口

对 `SMB100A` 施加了以下三段式 LF 输出：

- `OFF`：`2026-06-16T23:31:02Z` 到 `23:33:02Z`
- `ON`：`2026-06-16T23:33:03Z` 到 `23:39:03Z`
  - `LFO ON`
  - `500 Hz`
  - `137 mV`
  - `SQU`
- `OFF`：`2026-06-16T23:39:03Z` 到 `23:43:03Z`

所有 `SMB100A` 命令均返回：

- `0,"No error"`

### 6.3 解析结果响应

按 `parameter_values.csv` 的块均值统计：

- `YNoise`
  - `OFF1`: `8.52e-06`
  - `ON`: `3.02e-03`
  - `OFF2`: `3.52e-05`
- `R`
  - `OFF1`: `5.0399e-04`
  - `ON`: `5.1706e-04`
  - `OFF2`: `5.0557e-04`

按 `preview_values.csv` 的样本级 `X` 统计：

- `X RMS`
  - `OFF1`: `3.5592e-04`
  - `ON`: `3.7900e-04`
  - `OFF2`: `3.5784e-04`
- `X peak abs`
  - `OFF`: 约 `8.35e-04`
  - `ON`: 约 `1.64e-03`

折算关系：

- `X RMS on/off_avg ≈ 1.062x`
- `X peak on/off ≈ 1.97x`
- `YNoise on/off_avg ≈ 138x`
- `R on/off_avg ≈ 1.024x`

结论：

- `LFO ON` 窗口里，`oe1300` 解析结果有明确响应
- 切回 `OFF` 后，响应又回落
- 因而当前网口采样链路不是“只会解析出一堆静态数字”
- 它确实在响应真实外部输入

## 7. 当前阶段的工程结论

到目前为止，OE1300 网口 `RALL` 的设备层结论可固定为：

- 协议细节已足够支持稳定解码
- 不需要再保留多套试验 collector
- 当前最可靠路径是：
  - 单线程
  - 立即解码
  - 只保留解析后事实
- 实际有效采样率约 `1 kHz/parameter`
- 已完成 `15 min` 长跑和 `SMB100A` 受控输入验证

后续若进入 runtime 集成，必须单独回答两个问题，而不能直接照搬：

1. runtime 是否也坚持“不保留 raw、只保留解析后事实”？
2. runtime point/segment 的事实层到底定义为：
   - 主机查询块
   - 去重后新块
   - 还是展开后的每参数样本序列

在这两个问题未定之前，当前文档和 demo 的结论只代表：

- **OE1300 设备层采样事实已经跑通**

而不是 runtime 设计已经最终冻结。

## 8. 2026-06-17：Windows `180 s` 无外部输入复测

这轮复测的目标不是验证波形响应，而是把 `OE1300` 当前现成的 direct-decode 路径，与 `OE1022D field-decode-csv` 的 collector 同线程 direct-decode 路径放到同一个 Windows 真机环境里做对照。

命令：

- `dotnet ... Odmr.WinProbe.dll oe1300-net-labview-demo --host 192.168.1.1 --port 10001 --post-write-delay-ms 5 --preview-param-index 0 --write-values true --duration-sec 180 --out-dir oe1300_net_labview_180s`

链路确认：

- Windows `以太网` 口地址：`192.168.1.10/24`
- `ping -S 192.168.1.10 192.168.1.1` 成功
- `Test-NetConnection 192.168.1.1 -Port 10001` 返回 `TcpTestSucceeded = True`
- `oe1300-net-idn` 返回：
  - `SSI LIA-OE1301,SN:L2283261,Ver:V1.4h4-1.02-1.4.0.23B`

结果：

- `ralls_ok = 11559`
- `timeout_count = 0`
- `raw_len_bad_count = 0`
- `decode_failures = 0`
- `query_hz = 64.216`
- 机械折算 `effective_sample_hz_per_parameter = 6421.60`

但按 `parameter_values.csv` 相邻块去重后：

- `adjacent_duplicate_blocks = 9758`
- `unique_blocks = 1801`
- 重复块比例 = `84.42%`
- 唯一有效块率 = `1801 / 180.002 s = 10.01 Hz`
- 实际有效采样率 = `1801 * 100 / 180.002 s = 1000.54 Hz/parameter`

因此，这轮 `180 s` Windows 复测继续支持此前的结论：

- 主机 `RALL?` 查询频率约 `64 Hz`
- 但真正的新块频率仍然约 `10 Hz`
- 每个新块 `100` 样本
- 最终解析后的有效采样率仍然约 `1.00 kHz/parameter`
