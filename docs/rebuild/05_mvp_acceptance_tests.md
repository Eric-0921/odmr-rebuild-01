# MVP 验收测试

第一版 MVP 的验收重点是 runtime 事实正确，不是界面完整或功能丰富。

## 验收层级

### 1. 命令 helper 单元测试

目标：命令字符串或字节帧稳定可审计。

必须覆盖：

- SMB100A identity、output、frequency、power、sweep helper。
- OE1022D 固定配置和 `RALL?` helper。
- M8812 identity、remote/local、current、output helper。
- CNI Laser power/output frame 和 checksum。

失败条件：

- helper 输出和 golden 不一致。
- helper 输出未明确单位。
- 新增 helper 未更新设备命令规格。

### 2. Fake transport 测试

目标：验证 runtime 协议，而不依赖真机。

必须覆盖：

- station resolve 成功后才能 preflight。
- preflight 失败时不启动 collector。
- collector 是 run 级单实例。
- point 切换不会重启 OE collector。
- `RALL?` frame 能持续进入 raw writer。
- segment 起止 offset 正确。
- SMB100A readback 不符时 point 失败。
- M8812 settle 不达标时 point 失败。
- OE timeout 超阈值时 point quality 失败。
- cleanup 在失败后仍被调用。
- stop 会等待 collector thread joined。

### 3. Artifact schema 测试

目标：每次 run 都能生成完整事实记录。

必须覆盖：

- `run_manifest.json` 存在且有终态。
- `station_snapshot.json`、`calibration_snapshot.json`、`plan_snapshot.json` 存在。
- `events.jsonl` 至少包含 run、collector、point、cleanup 事件。
- 每个 point 有 `points.jsonl` 记录。
- 每个有效 point 有 `segments.jsonl` 记录。
- 每个 point 有 `quality.jsonl` 记录。
- raw file 和 frame index offset 能对应。
- `summary.json` 的 point 统计和 quality 统计一致。

### 4. Replay 测试

目标：离线读取 artifact 能重建 run 事实。

必须覆盖：

- replay 能列出 point 顺序。
- replay 能按 point 找到 segment。
- replay 能按 segment 找到 raw frame 范围。
- replay 能区分 passed point 和 failed point。
- replay 不需要连接任何设备。

### 5. 最小真机 smoke test

目标：证明最小链路能在真实设备上跑通。

建议第一版真机计划：

- 1 个 OE1022D。
- 1 个 SMB100A。
- 3 个 M8812。
- laser 使用固定状态或关闭。
- 3 个 point。
- 每个 point acquisition window 5 到 10 秒。
- RF sweep 使用固定 start/stop/step/dwell/power。

通过条件：

- station verify 能唯一绑定设备。
- run 中只有一个 OE collector。
- 三个 point 都有 settle 记录。
- 三个 point 都有 segment。
- 每个 segment 有非零 frame。
- quality 至少能判断 passed 或明确 failed reason。
- cleanup 后 RF output 关闭。
- collector stop 后线程已 joined。
- replay 能离线读取 run。

## 最低质量阈值

第一版默认阈值建议：

```json
{
  "min_frames": 50,
  "max_timeout_count": 3,
  "max_parse_error_count": 0,
  "max_duplicate_ratio": 0.05,
  "max_last_frame_age_ms": 500
}
```

这些数值只是 MVP 默认值，后续应由真实 OE poll interval 和 acquisition window 调整。

## 禁止通过的情况

以下情况即使流程走完，也不能算 MVP 通过：

- 任意 point 没有 quality 记录。
- 任意有效 point 没有 segment。
- raw 是按 point 切碎生成，而不是 run 级连续流。
- point 切换时重启 OE collector。
- collector stop 没有 join 语义。
- station resolve 有歧义但继续执行。
- cleanup 失败没有写 artifact。
- run summary 只写 completed，但实际存在空采 point。

## MVP 完成定义

MVP 完成的最低标准：

- CLI 能执行一个 3 point 真机 run。
- artifact 能证明每个 point 的设备上下文、segment、raw 范围和质量。
- replay 能在无设备状态下重建 point 结果。
- 失败能被显式记录，而不是被流程完成掩盖。

达到这个标准后，才考虑更复杂的 plan generator、TUI、GUI 或更高层工作流。
