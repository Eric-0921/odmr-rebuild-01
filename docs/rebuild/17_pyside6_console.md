# PySide6 Console Gate

当前 PySide6 console 是 Windows C# runtime 的前端外壳，不直接连接设备。它现在必须同时服务两种锁相：

- `oe1022d`
- `oe1300`

两者共用同一套页面、同一套按钮语义、同一套 pause/resume 主链。

## 一、边界

- C# 仍是唯一设备控制和采集 runtime。
- PySide6 只做配置生成、bundle 组合、CLI 启动、progress 监控和 artifact 审查。
- 不引入新的 bundle schema；输入仍然固定为六个 JSON。
- 不直接读取 raw，不进入 collector 线程，不做设备恢复逻辑。

## 二、页面结构

入口：

```bash
python3 tools/odmr-console-python/odmr_console_qt.py
```

页面：

- `本次实验配置`
- `配置生成`
- `预检查 / 预计用时`
- `运行监控`
- `数据审查`

## 三、本次实验配置

`Run Bundle` 页保持六个输入选择器：

- `硬件站配置`
- `磁场校准`
- `实验计划`
- `SMB100A 配置`
- `OE 配置（oe_profile）`
- `Laser 配置`

新增本地校验：

- 读取 `oe_profile.model`
- 从 `station.devices[].kind` 推断当前锁相型号
- 在摘要中同时显示：
  - `station_id`
  - `station_lockin_model`
  - `lockin_model`
- 如果 station/profile 型号不一致，直接标成 `mismatch`，禁止继续启动 run

## 四、配置生成

配置生成器仍然只输出 C# runtime 能直接读取的 `plan/profile JSON`。

当前锁相页已经改成“锁相放大器”，内部按 `model` 切换：

- `oe1022d`
  - 显示原有固定配置表单
  - 生成 `model = "oe1022d"` 的 profile
- `oe1300`
  - 显示 TCP `RALL` collector/fixed 参数表单
  - 生成 `model = "oe1300"` 的 profile

生成后的文件名约定：

- `oe1022d_*_generated.json`
- `oe1300_*_generated.json`

但回填路径仍然是同一个 `oe_profile_path`。

## 五、运行监控

Run Monitor 当前必须显示：

- `station_id`
- `lockin_model`
- `collector_contract`
- `terminal_status`

同时继续显示运行计数：

- `points_done / total`
- `frames_total`
- `samples_total`
- `timeout_count`
- `raw_len_bad_count`
- `decode_failures`

当前按钮语义：

- `Start`
- `Pause`
- `Emergency Stop`
- `Resume`

其中：

- `Pause` 通过 `stop.request` 触发 point 边界暂停
- `Emergency Stop` 通过 `emergency_stop.request` 触发中断
- `Resume` 调用 `resume-run`

## 六、Resume 启用规则

`Resume` 按钮只在真正可恢复的 terminal run 上启用：

- `paused`
- `failed`
- `completed_with_failed_points` 且仍有剩余 point
- 无 terminal status 但已有部分 artifact 的 `process exited`

以下状态禁用：

- `completed`
- `aborted`

恢复输出目录必须是新目录，例如：

```text
runs/demo_run__resume_01
```

不覆盖旧 run。

## 七、Artifact Review

审查页继续只调用：

- `artifact-check`
- `audit-continuity`

两者都按 `lockin_model` 自动分支：

- `oe1022d` 审 `collector_frames.jsonl`
- `oe1300` 审 `collector_blocks.jsonl`

PySide6 不自行解释 collector 细节，只展示 CLI 输出和 JSON 摘要。

## 八、截图验收口径

当前 UI 验收不靠口头描述，靠截图。

至少要覆盖：

- `Run Bundle`：`OE1022D`
- `Run Bundle`：`OE1300`
- `Config Generator`：`OE1022D`
- `Config Generator`：`OE1300`
- `Run Monitor`：运行中
- `Run Monitor`：`paused`
- `Run Monitor`：`resumed/completed`
- `Artifact Review`

出现以下任一情况，都算未通过：

- 控件重叠
- 文本截断
- 按钮被裁切
- 状态标签与真实 run 不一致
- `Resume` 启用逻辑错误

## 九、当前实现原则

本轮 UI 明确不做：

- 不做复杂前端抽象
- 不做单 station 内双锁相切换
- 不做自动资源调度
- 不做直接设备控制
- 不为 `oe1300` 单独再造一套 run 入口

实现重点只有两个：

- 把双型号真实接入同一条主链
- 把运行状态和合同信息直观暴露给操作者

## 十、最低验证

本地：

```bash
python3 -m py_compile tools/odmr-console-python/odmr_console_core.py tools/odmr-console-python/odmr_console_qt.py
python3 tools/odmr-console-python/tests/test_odmr_console_core.py
python3 tools/config-generator/tests/test_config_core.py
```

Windows：

```powershell
dotnet build tools/win-csharp/Odmr.Win.sln -c Release
python tools\odmr-console-python\odmr_console_qt.py
```

通过 UI 至少验证：

- `run-resolve`
- `run-execute`
- `pause`
- `resume`
- `artifact-check`
- `audit-continuity`

而且这条链路必须对 `oe1022d` 和 `oe1300` 都成立。
