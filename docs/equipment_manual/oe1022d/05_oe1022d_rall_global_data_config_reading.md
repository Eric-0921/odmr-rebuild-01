# OE1022D RALL? 全局数据配置读取指令

> 本文档把图片中的 `5.2.11 全局数据配置读取指令` 与 `RALL? 返回数据格式如下` 整理为 Markdown。
> 为方便 AI agent 读取，原始表格后附带了结构化解析提示。字节区间按图片中的 `数据位置` 原样保留，默认理解为从 0 开始、首尾都包含的 byte offset。

## 1. 指令说明表

| 指令 | 说明 |
|---|---|
| RALL? | RALL? 指令是 USB2.0 专用，在 RS232 接口没有该指令。RALL? 指令用于读取 OE1022D 的所有测量数据和当前配置信息。返回数据长度是 12288Bytes，数据中间没有逗号隔开，按照固定空间分配数据组合。RALL? 指令返回数据每 50ms 更新一次，返回之前 50ms 的测量数据，数据采样间隔是 1ms，也就是每次返回之前的 50 个测量数据。例如前 400 个 Bytes 数据为 50 个 X 值数据，每个 X 值是 64 位浮点数，占用 8 个 Bytes，按照时间先后顺序排列 50 个 X 值；然后到 Y 值……前 8000Bytes 是返回测量数据，然后的 1216Bytes 是当前配置信息，每个配置信息不像数据会返回 50 个值，而是只返回 1 次参数。最后的 3072Bytes 是空字符，合计 12288 Bytes。 |

## 2. RALL? 返回数据格式如下

| 分类 | 返回数据 | 数据位置 |
|---|---|---|
| 测量数据 | 50 个 A 通道 X 值（64 位浮点数） | 0~399 |
| 测量数据 | 50 个 A 通道 Y 值（64 位浮点数） | 400~799 |
| 测量数据 | 50 个 A 通道频率值（64 位浮点数） | 800~1199 |
| 测量数据 | 50 个 A 通道 Noise 值（64 位浮点数） | 1200~1599 |
| 测量数据 | 50 个 A 通道 Xh1 值（64 位浮点数） | 1600~1999 |
| 测量数据 | 50 个 A 通道 Yh1 值（64 位浮点数） | 2000~2399 |
| 测量数据 | 50 个 A 通道 Xh2 值（64 位浮点数） | 2400~2799 |
| 测量数据 | 50 个 A 通道 Yh2 值（64 位浮点数） | 2800~3199 |
| 测量数据 | 50 个 B 通道 X 值（64 位浮点数） | 3200~3599 |
| 测量数据 | 50 个 B 通道 Y 值（64 位浮点数） | 3600~3999 |
| 测量数据 | 50 个 B 通道频率值（64 位浮点数） | 4000~4399 |
| 测量数据 | 50 个 B 通道 Noise 值（64 位浮点数） | 4400~4799 |
| 测量数据 | 50 个 B 通道 Xh1 值（64 位浮点数） | 4800~5199 |
| 测量数据 | 50 个 B 通道 Yh1 值（64 位浮点数） | 5200~5599 |
| 测量数据 | 50 个 B 通道 Xh2 值（64 位浮点数） | 5600~5999 |
| 测量数据 | 50 个 B 通道 Yh2 值（64 位浮点数） | 6000~6399 |
| 测量数据 | 50 个 AUXADC1 值（64 位浮点数） | 6400~6799 |
| 测量数据 | 50 个 AUXADC2 值（64 位浮点数） | 6800~7199 |
| 测量数据 | 50 个 AUXADC3 值（64 位浮点数） | 7200~7599 |
| 测量数据 | 50 个 AUXADC4 值（64 位浮点数） | 7600~7999 |
| A 通道 Ref Phase 配置参数 | <Ref.Phase>设置（32 位浮点数） | 8200~8203 |
| A 通道 Ref Phase 配置参数 | <Ref.Source>设置（8 位整型数） | 8204 |
| A 通道 Ref Phase 配置参数 | 当前频率值（64 位浮点数） | 8205~8212 |
| A 通道 Ref Phase 配置参数 | 内部频率值（64 位浮点数） | 8213~8220 |
| A 通道 Ref Phase 配置参数 | <Ref.slope>设置（8 位整型数） | 8221 |
| A 通道 Ref Phase 配置参数 | <Harmonic 1>设置（64 位整型数） | 8222~8229 |
| A 通道 Ref Phase 配置参数 | <Harmonic 2>设置（64 位整型数） | 8230~8237 |
| A 通道 Ref Sweep 配置参数 | <Sweep Type>设置（8 位整型数） | 8246 |
| A 通道 Ref Sweep 配置参数 | <SweepStartFreq>设置（64 位浮点数） | 8247~8254 |
| A 通道 Ref Sweep 配置参数 | <SweepStopFreq>设置（64 位浮点数） | 8255~8262 |
| A 通道 Ref Sweep 配置参数 | <SweepStepFreq>设置（64 位浮点数） | 8263~8270 |
| A 通道 Ref Sweep 配置参数 | <SweepStepPerc>设置（32 位浮点数） | 8271~8274 |
| A 通道 Ref Sweep 配置参数 | <Sweep Time>设置（64 位整型数） | 8275~8282 |
| A 通道 Ref Sweep 配置参数 | <Sweep Run>设置（8 位整型数） | 8283 |
| A 通道 Sineout 配置参数 | <Sineout Voltage>设置（32 位浮点数） | 8292~8295 |
| A 通道 Sineout 配置参数 | <SweepMode>设置（8 位整型数） | 8296 |
| A 通道 Sineout 配置参数 | <SweepSartVolt>设置（32 位浮点数） | 8297~8300 |
| A 通道 Sineout 配置参数 | <SweepStopVolt>设置（32 位浮点数） | 8301~8304 |
| A 通道 Sineout 配置参数 | <SweepStepVolt>设置（32 位浮点数） | 8305~8308 |
| A 通道 Sineout 配置参数 | <SweepStepPrecent>设置（32 位浮点数） | 8309~8312 |
| A 通道 Sineout 配置参数 | <Sweep Time>设置（64 位整型数） | 8313~8320 |
| A 通道 Sineout 配置参数 | <Sineout Sweep Run>设置（8 位整型数） | 8321 |
| A 通道 Sineout 配置参数 | <Sineout DC Voltage>设置（32 位浮点数） | 8322~8325 |
| A 通道 Equation 配置参数 | <Equation C1>设置（64 位浮点数） | 8330~8337 |
| A 通道 Equation 配置参数 | <Equation C2>设置（64 位浮点数） | 8338~8345 |
| A 通道 Equation 配置参数 | <Equation1 A>源设置（8 位整型数） | 8346 |
| A 通道 Equation 配置参数 | <Equation2 A>源设置（8 位整型数） | 8347 |
| A 通道 Equation 配置参数 | <Equation3 A>源设置（8 位整型数） | 8348 |
| A 通道 Equation 配置参数 | <Equation4 A>源设置（8 位整型数） | 8349 |
| A 通道 Equation 配置参数 | <Equation1 B>源设置（8 位整型数） | 8350 |
| A 通道 Equation 配置参数 | <Equation2 B>源设置（8 位整型数） | 8351 |
| A 通道 Equation 配置参数 | <Equation3 B>源设置（8 位整型数） | 8352 |
| A 通道 Equation 配置参数 | <Equation4 B>源设置（8 位整型数） | 8353 |
| A 通道 Equation 配置参数 | <Equation1 C>源设置（8 位整型数） | 8354 |
| A 通道 Equation 配置参数 | <Equation2 C>源设置（8 位整型数） | 8355 |
| A 通道 Equation 配置参数 | <Equation3 C>源设置（8 位整型数） | 8356 |
| A 通道 Equation 配置参数 | <Equation4 C>源设置（8 位整型数） | 8357 |
| A 通道 Gain TC Input Filter 配置参数 | <Sensitivity>设置（8 位整型数） | 8390 |
| A 通道 Gain TC Input Filter 配置参数 | <Reserve>设置（8 位整型数） | 8391 |
| A 通道 Gain TC Input Filter 配置参数 | <Source>设置（8 位整型数） | 8392 |
| A 通道 Gain TC Input Filter 配置参数 | <Grounding>设置（8 位整型数） | 8393 |
| A 通道 Gain TC Input Filter 配置参数 | <Coupling>设置（8 位整型数） | 8394 |
| A 通道 Gain TC Input Filter 配置参数 | <Line Notch>设置（8 位整型数） | 8395 |
| A 通道 Gain TC Input Filter 配置参数 | <Time Constant>设置（8 位整型数） | 8404 |
| A 通道 Gain TC Input Filter 配置参数 | <Filter dB/oct>设置（8 位整型数） | 8405 |
| A 通道 Gain TC Input Filter 配置参数 | <Synchronous>设置（8 位整型数） | 8406 |
| CHOUT 配置参数 | <CH1 Output Source>设置（8 位整型数） | 8415 |
| CHOUT 配置参数 | <CH2 Output Source>设置（8 位整型数） | 8416 |
| CHOUT 配置参数 | <CH1 Offset>设置（32 位浮点数） | 8417~8420 |
| CHOUT 配置参数 | <CH2 Offset>设置（32 位浮点数） | 8421~8424 |
| CHOUT 配置参数 | <CH1 Expand>设置（16 位整型数） | 8425~8426 |
| CHOUT 配置参数 | <CH2 Expand>设置（16 位整型数） | 8427~8428 |
| CHOUT 配置参数 | <CH1 Output Speed>设置（8 位整型数） | 8429 |
| CHOUT 配置参数 | <CH2 Output Speed>设置（8 位整型数） | 8430 |
| CHOUT 配置参数 | <CH1 AUXOUT>值设置（32 位浮点数） | 8431~8434 |
| CHOUT 配置参数 | <CH2 AUXOUT>值设置（32 位浮点数） | 8435~8438 |
| A 通道 Sample 配置参数 | <Sample Time>设置（64 位浮点数） | 8441~8448 |
| A 通道 Sample 配置参数 | <Sample Length>设置（64 位整型数） | 8449~8456 |
| A 通道 Sample 配置参数 | <Sample Buffer1>设置（8 位整型数） | 8457 |
| A 通道 Sample 配置参数 | <Sample Buffer2>设置（8 位整型数） | 8458 |
| A 通道 Sample 配置参数 | <Sample Buffer3>设置（8 位整型数） | 8459 |
| A 通道 Sample 配置参数 | <Sample Buffer4>设置（8 位整型数） | 8460 |
| A 通道 Sample 配置参数 | <Sample Trigger Mode>设置（8 位整型数） | 8461 |
| A 通道 Sample 配置参数 | <Sample Mode>设置（8 位整型数） | 8462 |
| A 通道 Sample 配置参数 | <Sample Current Point>设置（64 位整型数） | 8463~8470 |
| A 通道状态参数 | Input Overload 状态（8 位整型数） | 8479 |
| A 通道状态参数 | Gain Overload 状态（8 位整型数） | 8480 |
| A 通道状态参数 | PLL LOCKED 状态（8 位整型数） | 8481 |
| B 通道 Ref Phase 配置参数 | <Ref.Phase>设置（32 位浮点数） | 8500~8503 |
| B 通道 Ref Phase 配置参数 | <Ref.Source>设置（8 位整型数） | 8504 |
| B 通道 Ref Phase 配置参数 | 当前频率值（64 位浮点数） | 8505~8512 |
| B 通道 Ref Phase 配置参数 | 内部频率值（64 位浮点数） | 8513~8520 |
| B 通道 Ref Phase 配置参数 | <Ref.slope>设置（8 位整型数） | 8521 |
| B 通道 Ref Phase 配置参数 | <Harmonic 1>设置（64 位整型数） | 8522~8529 |
| B 通道 Ref Phase 配置参数 | <Harmonic 2>设置（64 位整型数） | 8530~8537 |
| B 通道 Ref Sweep 配置参数 | <Sweep Type>设置（8 位整型数） | 8546 |
| B 通道 Ref Sweep 配置参数 | <SweepStartFreq>设置（64 位浮点数） | 8547~8554 |
| B 通道 Ref Sweep 配置参数 | <SweepStopFreq>设置（64 位浮点数） | 8555~8562 |
| B 通道 Ref Sweep 配置参数 | <SweepStepFreq>设置（64 位浮点数） | 8563~8570 |
| B 通道 Ref Sweep 配置参数 | <SweepStepPercent>设置（32 位浮点数） | 8571~8574 |
| B 通道 Ref Sweep 配置参数 | <Sweep Time>设置（64 位整型数） | 8575~8582 |
| B 通道 Ref Sweep 配置参数 | <Sweep Run>设置（8 位整型数） | 8583 |
| B 通道 Sineout 配置参数 | <Sineout Voltage>设置（32 位浮点数） | 8592~8595 |
| B 通道 Sineout 配置参数 | <SweepMode>设置（8 位整型数） | 8596 |
| B 通道 Sineout 配置参数 | <SweepSartVolt>设置（32 位浮点数） | 8597~8600 |
| B 通道 Sineout 配置参数 | <SweepStopVolt>设置（32 位浮点数） | 8601~8604 |
| B 通道 Sineout 配置参数 | <SweepStepVolt>设置（32 位浮点数） | 8605~8608 |
| B 通道 Sineout 配置参数 | <SweepStepPrec>设置（32 位浮点数） | 8609~8612 |
| B 通道 Sineout 配置参数 | <Sweep Time>设置（64 位整型数） | 8613~8620 |
| B 通道 Sineout 配置参数 | <Sineout Sweep Run>设置（8 位整型数） | 8621 |
| B 通道 Sineout 配置参数 | <Sineout DC Voltage>设置（32 位浮点数） | 8622~8625 |
| B 通道 Equation 配置参数 | <Equation C1>设置（64 位浮点数） | 8630~8637 |
| B 通道 Equation 配置参数 | <Equation C2>设置（64 位浮点数） | 8638~8645 |
| B 通道 Equation 配置参数 | <Equation1 A>源设置（8 位整型数） | 8646 |
| B 通道 Equation 配置参数 | <Equation2 A>源设置（8 位整型数） | 8647 |
| B 通道 Equation 配置参数 | <Equation3 A>源设置（8 位整型数） | 8648 |
| B 通道 Equation 配置参数 | <Equation4 A>源设置（8 位整型数） | 8649 |
| B 通道 Equation 配置参数 | <Equation1 B>源设置（8 位整型数） | 8650 |
| B 通道 Equation 配置参数 | <Equation2 B>源设置（8 位整型数） | 8651 |
| B 通道 Equation 配置参数 | <Equation3 B>源设置（8 位整型数） | 8652 |
| B 通道 Equation 配置参数 | <Equation4 B>源设置（8 位整型数） | 8653 |
| B 通道 Equation 配置参数 | <Equation1 C>源设置（8 位整型数） | 8654 |
| B 通道 Equation 配置参数 | <Equation2 C>源设置（8 位整型数） | 8655 |
| B 通道 Equation 配置参数 | <Equation3 C>源设置（8 位整型数） | 8656 |
| B 通道 Equation 配置参数 | <Equation4 C>源设置（8 位整型数） | 8657 |
| B 通道 Gain TC Input Filter 配置参数 | <Sensitivity>设置（8 位整型数） | 8690 |
| B 通道 Gain TC Input Filter 配置参数 | <Reserve>设置（8 位整型数） | 8691 |
| B 通道 Gain TC Input Filter 配置参数 | <Source>设置（8 位整型数） | 8692 |
| B 通道 Gain TC Input Filter 配置参数 | <Grounding>设置（8 位整型数） | 8693 |
| B 通道 Gain TC Input Filter 配置参数 | <Coupling>设置（8 位整型数） | 8694 |
| B 通道 Gain TC Input Filter 配置参数 | <Line Notch>设置（8 位整型数） | 8695 |
| B 通道 Gain TC Input Filter 配置参数 | <Time Constant>设置（8 位整型数） | 8704 |
| B 通道 Gain TC Input Filter 配置参数 | <Filter dB/oct>设置（8 位整型数） | 8705 |
| B 通道 Gain TC Input Filter 配置参数 | <Synchronous>设置（8 位整型数） | 8706 |
| B 通道 Sample 配置参数 | <Sample Time>设置（64 位浮点数） | 8741~8748 |
| B 通道 Sample 配置参数 | <Sample Length>设置（64 位整型数） | 8749~8756 |
| B 通道 Sample 配置参数 | <Sample Buffer1>设置（8 位整型数） | 8757 |
| B 通道 Sample 配置参数 | <Sample Buffer2>设置（8 位整型数） | 8758 |
| B 通道 Sample 配置参数 | <Sample Buffer3>设置（8 位整型数） | 8759 |
| B 通道 Sample 配置参数 | <Sample Buffer4>设置（8 位整型数） | 8760 |
| B 通道 Sample 配置参数 | <Sample Trigger Mode>设置（8 位整型数） | 8761 |
| B 通道 Sample 配置参数 | <Sample Mode>设置（8 位整型数） | 8762 |
| B 通道 Sample 配置参数 | <Sample Current Point>设置（64 位整型数） | 8763~8770 |
| B 通道状态参数 | Input Overload 状态（8 位整型数） | 8779 |
| B 通道状态参数 | Gain Overload 状态（8 位整型数） | 8780 |
| B 通道状态参数 | PLL LOCKED 状态（8 位整型数） | 8781 |
| IDN 序列号 | IDN 序列号返回值（40Bytes 长度） | 9170~9209 |


## 3. AI-agent 解析辅助

### 3.1 固定事实

| 项目 | 值 |
|---|---|
| 指令 | RALL? |
| 接口限制 | USB2.0 专用；RS232 接口没有该指令 |
| 返回总长度 | 12288 Bytes |
| 测量数据区 | 前 8000 Bytes |
| 配置信息区 | 后续 1216 Bytes |
| 空字符区 | 最后的 3072 Bytes |
| 数据刷新周期 | 每 50ms 更新一次 |
| 测量数据采样间隔 | 1ms |
| 每次返回测量点数 | 50 个测量数据 |
| 分隔符 | 数据中间没有逗号隔开 |
| 排列方式 | 按照固定空间分配数据组合；每个测量值按照时间先后顺序排列 |

### 3.2 结构化字段索引

> `byte_start` 和 `byte_end` 从 `数据位置` 拆分而来，均为闭区间。`inferred_type` 和 `repeat_count` 是为了方便 parser / schema 生成，不是原表新增含义。

| field_id | 分类 | 返回数据 | byte_start | byte_end | inferred_type | repeat_count |
|---:|---|---|---:|---:|---|---:|
| 1 | 测量数据 | 50 个 A 通道 X 值（64 位浮点数） | 0 | 399 | f64 | 50 |
| 2 | 测量数据 | 50 个 A 通道 Y 值（64 位浮点数） | 400 | 799 | f64 | 50 |
| 3 | 测量数据 | 50 个 A 通道频率值（64 位浮点数） | 800 | 1199 | f64 | 50 |
| 4 | 测量数据 | 50 个 A 通道 Noise 值（64 位浮点数） | 1200 | 1599 | f64 | 50 |
| 5 | 测量数据 | 50 个 A 通道 Xh1 值（64 位浮点数） | 1600 | 1999 | f64 | 50 |
| 6 | 测量数据 | 50 个 A 通道 Yh1 值（64 位浮点数） | 2000 | 2399 | f64 | 50 |
| 7 | 测量数据 | 50 个 A 通道 Xh2 值（64 位浮点数） | 2400 | 2799 | f64 | 50 |
| 8 | 测量数据 | 50 个 A 通道 Yh2 值（64 位浮点数） | 2800 | 3199 | f64 | 50 |
| 9 | 测量数据 | 50 个 B 通道 X 值（64 位浮点数） | 3200 | 3599 | f64 | 50 |
| 10 | 测量数据 | 50 个 B 通道 Y 值（64 位浮点数） | 3600 | 3999 | f64 | 50 |
| 11 | 测量数据 | 50 个 B 通道频率值（64 位浮点数） | 4000 | 4399 | f64 | 50 |
| 12 | 测量数据 | 50 个 B 通道 Noise 值（64 位浮点数） | 4400 | 4799 | f64 | 50 |
| 13 | 测量数据 | 50 个 B 通道 Xh1 值（64 位浮点数） | 4800 | 5199 | f64 | 50 |
| 14 | 测量数据 | 50 个 B 通道 Yh1 值（64 位浮点数） | 5200 | 5599 | f64 | 50 |
| 15 | 测量数据 | 50 个 B 通道 Xh2 值（64 位浮点数） | 5600 | 5999 | f64 | 50 |
| 16 | 测量数据 | 50 个 B 通道 Yh2 值（64 位浮点数） | 6000 | 6399 | f64 | 50 |
| 17 | 测量数据 | 50 个 AUXADC1 值（64 位浮点数） | 6400 | 6799 | f64 | 50 |
| 18 | 测量数据 | 50 个 AUXADC2 值（64 位浮点数） | 6800 | 7199 | f64 | 50 |
| 19 | 测量数据 | 50 个 AUXADC3 值（64 位浮点数） | 7200 | 7599 | f64 | 50 |
| 20 | 测量数据 | 50 个 AUXADC4 值（64 位浮点数） | 7600 | 7999 | f64 | 50 |
| 21 | A 通道 Ref Phase 配置参数 | <Ref.Phase>设置（32 位浮点数） | 8200 | 8203 | f32 | 1 |
| 22 | A 通道 Ref Phase 配置参数 | <Ref.Source>设置（8 位整型数） | 8204 | 8204 | i8 | 1 |
| 23 | A 通道 Ref Phase 配置参数 | 当前频率值（64 位浮点数） | 8205 | 8212 | f64 | 1 |
| 24 | A 通道 Ref Phase 配置参数 | 内部频率值（64 位浮点数） | 8213 | 8220 | f64 | 1 |
| 25 | A 通道 Ref Phase 配置参数 | <Ref.slope>设置（8 位整型数） | 8221 | 8221 | i8 | 1 |
| 26 | A 通道 Ref Phase 配置参数 | <Harmonic 1>设置（64 位整型数） | 8222 | 8229 | i64 | 1 |
| 27 | A 通道 Ref Phase 配置参数 | <Harmonic 2>设置（64 位整型数） | 8230 | 8237 | i64 | 1 |
| 28 | A 通道 Ref Sweep 配置参数 | <Sweep Type>设置（8 位整型数） | 8246 | 8246 | i8 | 1 |
| 29 | A 通道 Ref Sweep 配置参数 | <SweepStartFreq>设置（64 位浮点数） | 8247 | 8254 | f64 | 1 |
| 30 | A 通道 Ref Sweep 配置参数 | <SweepStopFreq>设置（64 位浮点数） | 8255 | 8262 | f64 | 1 |
| 31 | A 通道 Ref Sweep 配置参数 | <SweepStepFreq>设置（64 位浮点数） | 8263 | 8270 | f64 | 1 |
| 32 | A 通道 Ref Sweep 配置参数 | <SweepStepPerc>设置（32 位浮点数） | 8271 | 8274 | f32 | 1 |
| 33 | A 通道 Ref Sweep 配置参数 | <Sweep Time>设置（64 位整型数） | 8275 | 8282 | i64 | 1 |
| 34 | A 通道 Ref Sweep 配置参数 | <Sweep Run>设置（8 位整型数） | 8283 | 8283 | i8 | 1 |
| 35 | A 通道 Sineout 配置参数 | <Sineout Voltage>设置（32 位浮点数） | 8292 | 8295 | f32 | 1 |
| 36 | A 通道 Sineout 配置参数 | <SweepMode>设置（8 位整型数） | 8296 | 8296 | i8 | 1 |
| 37 | A 通道 Sineout 配置参数 | <SweepSartVolt>设置（32 位浮点数） | 8297 | 8300 | f32 | 1 |
| 38 | A 通道 Sineout 配置参数 | <SweepStopVolt>设置（32 位浮点数） | 8301 | 8304 | f32 | 1 |
| 39 | A 通道 Sineout 配置参数 | <SweepStepVolt>设置（32 位浮点数） | 8305 | 8308 | f32 | 1 |
| 40 | A 通道 Sineout 配置参数 | <SweepStepPrecent>设置（32 位浮点数） | 8309 | 8312 | f32 | 1 |
| 41 | A 通道 Sineout 配置参数 | <Sweep Time>设置（64 位整型数） | 8313 | 8320 | i64 | 1 |
| 42 | A 通道 Sineout 配置参数 | <Sineout Sweep Run>设置（8 位整型数） | 8321 | 8321 | i8 | 1 |
| 43 | A 通道 Sineout 配置参数 | <Sineout DC Voltage>设置（32 位浮点数） | 8322 | 8325 | f32 | 1 |
| 44 | A 通道 Equation 配置参数 | <Equation C1>设置（64 位浮点数） | 8330 | 8337 | f64 | 1 |
| 45 | A 通道 Equation 配置参数 | <Equation C2>设置（64 位浮点数） | 8338 | 8345 | f64 | 1 |
| 46 | A 通道 Equation 配置参数 | <Equation1 A>源设置（8 位整型数） | 8346 | 8346 | i8 | 1 |
| 47 | A 通道 Equation 配置参数 | <Equation2 A>源设置（8 位整型数） | 8347 | 8347 | i8 | 1 |
| 48 | A 通道 Equation 配置参数 | <Equation3 A>源设置（8 位整型数） | 8348 | 8348 | i8 | 1 |
| 49 | A 通道 Equation 配置参数 | <Equation4 A>源设置（8 位整型数） | 8349 | 8349 | i8 | 1 |
| 50 | A 通道 Equation 配置参数 | <Equation1 B>源设置（8 位整型数） | 8350 | 8350 | i8 | 1 |
| 51 | A 通道 Equation 配置参数 | <Equation2 B>源设置（8 位整型数） | 8351 | 8351 | i8 | 1 |
| 52 | A 通道 Equation 配置参数 | <Equation3 B>源设置（8 位整型数） | 8352 | 8352 | i8 | 1 |
| 53 | A 通道 Equation 配置参数 | <Equation4 B>源设置（8 位整型数） | 8353 | 8353 | i8 | 1 |
| 54 | A 通道 Equation 配置参数 | <Equation1 C>源设置（8 位整型数） | 8354 | 8354 | i8 | 1 |
| 55 | A 通道 Equation 配置参数 | <Equation2 C>源设置（8 位整型数） | 8355 | 8355 | i8 | 1 |
| 56 | A 通道 Equation 配置参数 | <Equation3 C>源设置（8 位整型数） | 8356 | 8356 | i8 | 1 |
| 57 | A 通道 Equation 配置参数 | <Equation4 C>源设置（8 位整型数） | 8357 | 8357 | i8 | 1 |
| 58 | A 通道 Gain TC Input Filter 配置参数 | <Sensitivity>设置（8 位整型数） | 8390 | 8390 | i8 | 1 |
| 59 | A 通道 Gain TC Input Filter 配置参数 | <Reserve>设置（8 位整型数） | 8391 | 8391 | i8 | 1 |
| 60 | A 通道 Gain TC Input Filter 配置参数 | <Source>设置（8 位整型数） | 8392 | 8392 | i8 | 1 |
| 61 | A 通道 Gain TC Input Filter 配置参数 | <Grounding>设置（8 位整型数） | 8393 | 8393 | i8 | 1 |
| 62 | A 通道 Gain TC Input Filter 配置参数 | <Coupling>设置（8 位整型数） | 8394 | 8394 | i8 | 1 |
| 63 | A 通道 Gain TC Input Filter 配置参数 | <Line Notch>设置（8 位整型数） | 8395 | 8395 | i8 | 1 |
| 64 | A 通道 Gain TC Input Filter 配置参数 | <Time Constant>设置（8 位整型数） | 8404 | 8404 | i8 | 1 |
| 65 | A 通道 Gain TC Input Filter 配置参数 | <Filter dB/oct>设置（8 位整型数） | 8405 | 8405 | i8 | 1 |
| 66 | A 通道 Gain TC Input Filter 配置参数 | <Synchronous>设置（8 位整型数） | 8406 | 8406 | i8 | 1 |
| 67 | CHOUT 配置参数 | <CH1 Output Source>设置（8 位整型数） | 8415 | 8415 | i8 | 1 |
| 68 | CHOUT 配置参数 | <CH2 Output Source>设置（8 位整型数） | 8416 | 8416 | i8 | 1 |
| 69 | CHOUT 配置参数 | <CH1 Offset>设置（32 位浮点数） | 8417 | 8420 | f32 | 1 |
| 70 | CHOUT 配置参数 | <CH2 Offset>设置（32 位浮点数） | 8421 | 8424 | f32 | 1 |
| 71 | CHOUT 配置参数 | <CH1 Expand>设置（16 位整型数） | 8425 | 8426 | i16 | 1 |
| 72 | CHOUT 配置参数 | <CH2 Expand>设置（16 位整型数） | 8427 | 8428 | i16 | 1 |
| 73 | CHOUT 配置参数 | <CH1 Output Speed>设置（8 位整型数） | 8429 | 8429 | i8 | 1 |
| 74 | CHOUT 配置参数 | <CH2 Output Speed>设置（8 位整型数） | 8430 | 8430 | i8 | 1 |
| 75 | CHOUT 配置参数 | <CH1 AUXOUT>值设置（32 位浮点数） | 8431 | 8434 | f32 | 1 |
| 76 | CHOUT 配置参数 | <CH2 AUXOUT>值设置（32 位浮点数） | 8435 | 8438 | f32 | 1 |
| 77 | A 通道 Sample 配置参数 | <Sample Time>设置（64 位浮点数） | 8441 | 8448 | f64 | 1 |
| 78 | A 通道 Sample 配置参数 | <Sample Length>设置（64 位整型数） | 8449 | 8456 | i64 | 1 |
| 79 | A 通道 Sample 配置参数 | <Sample Buffer1>设置（8 位整型数） | 8457 | 8457 | i8 | 1 |
| 80 | A 通道 Sample 配置参数 | <Sample Buffer2>设置（8 位整型数） | 8458 | 8458 | i8 | 1 |
| 81 | A 通道 Sample 配置参数 | <Sample Buffer3>设置（8 位整型数） | 8459 | 8459 | i8 | 1 |
| 82 | A 通道 Sample 配置参数 | <Sample Buffer4>设置（8 位整型数） | 8460 | 8460 | i8 | 1 |
| 83 | A 通道 Sample 配置参数 | <Sample Trigger Mode>设置（8 位整型数） | 8461 | 8461 | i8 | 1 |
| 84 | A 通道 Sample 配置参数 | <Sample Mode>设置（8 位整型数） | 8462 | 8462 | i8 | 1 |
| 85 | A 通道 Sample 配置参数 | <Sample Current Point>设置（64 位整型数） | 8463 | 8470 | i64 | 1 |
| 86 | A 通道状态参数 | Input Overload 状态（8 位整型数） | 8479 | 8479 | i8 | 1 |
| 87 | A 通道状态参数 | Gain Overload 状态（8 位整型数） | 8480 | 8480 | i8 | 1 |
| 88 | A 通道状态参数 | PLL LOCKED 状态（8 位整型数） | 8481 | 8481 | i8 | 1 |
| 89 | B 通道 Ref Phase 配置参数 | <Ref.Phase>设置（32 位浮点数） | 8500 | 8503 | f32 | 1 |
| 90 | B 通道 Ref Phase 配置参数 | <Ref.Source>设置（8 位整型数） | 8504 | 8504 | i8 | 1 |
| 91 | B 通道 Ref Phase 配置参数 | 当前频率值（64 位浮点数） | 8505 | 8512 | f64 | 1 |
| 92 | B 通道 Ref Phase 配置参数 | 内部频率值（64 位浮点数） | 8513 | 8520 | f64 | 1 |
| 93 | B 通道 Ref Phase 配置参数 | <Ref.slope>设置（8 位整型数） | 8521 | 8521 | i8 | 1 |
| 94 | B 通道 Ref Phase 配置参数 | <Harmonic 1>设置（64 位整型数） | 8522 | 8529 | i64 | 1 |
| 95 | B 通道 Ref Phase 配置参数 | <Harmonic 2>设置（64 位整型数） | 8530 | 8537 | i64 | 1 |
| 96 | B 通道 Ref Sweep 配置参数 | <Sweep Type>设置（8 位整型数） | 8546 | 8546 | i8 | 1 |
| 97 | B 通道 Ref Sweep 配置参数 | <SweepStartFreq>设置（64 位浮点数） | 8547 | 8554 | f64 | 1 |
| 98 | B 通道 Ref Sweep 配置参数 | <SweepStopFreq>设置（64 位浮点数） | 8555 | 8562 | f64 | 1 |
| 99 | B 通道 Ref Sweep 配置参数 | <SweepStepFreq>设置（64 位浮点数） | 8563 | 8570 | f64 | 1 |
| 100 | B 通道 Ref Sweep 配置参数 | <SweepStepPercent>设置（32 位浮点数） | 8571 | 8574 | f32 | 1 |
| 101 | B 通道 Ref Sweep 配置参数 | <Sweep Time>设置（64 位整型数） | 8575 | 8582 | i64 | 1 |
| 102 | B 通道 Ref Sweep 配置参数 | <Sweep Run>设置（8 位整型数） | 8583 | 8583 | i8 | 1 |
| 103 | B 通道 Sineout 配置参数 | <Sineout Voltage>设置（32 位浮点数） | 8592 | 8595 | f32 | 1 |
| 104 | B 通道 Sineout 配置参数 | <SweepMode>设置（8 位整型数） | 8596 | 8596 | i8 | 1 |
| 105 | B 通道 Sineout 配置参数 | <SweepSartVolt>设置（32 位浮点数） | 8597 | 8600 | f32 | 1 |
| 106 | B 通道 Sineout 配置参数 | <SweepStopVolt>设置（32 位浮点数） | 8601 | 8604 | f32 | 1 |
| 107 | B 通道 Sineout 配置参数 | <SweepStepVolt>设置（32 位浮点数） | 8605 | 8608 | f32 | 1 |
| 108 | B 通道 Sineout 配置参数 | <SweepStepPrec>设置（32 位浮点数） | 8609 | 8612 | f32 | 1 |
| 109 | B 通道 Sineout 配置参数 | <Sweep Time>设置（64 位整型数） | 8613 | 8620 | i64 | 1 |
| 110 | B 通道 Sineout 配置参数 | <Sineout Sweep Run>设置（8 位整型数） | 8621 | 8621 | i8 | 1 |
| 111 | B 通道 Sineout 配置参数 | <Sineout DC Voltage>设置（32 位浮点数） | 8622 | 8625 | f32 | 1 |
| 112 | B 通道 Equation 配置参数 | <Equation C1>设置（64 位浮点数） | 8630 | 8637 | f64 | 1 |
| 113 | B 通道 Equation 配置参数 | <Equation C2>设置（64 位浮点数） | 8638 | 8645 | f64 | 1 |
| 114 | B 通道 Equation 配置参数 | <Equation1 A>源设置（8 位整型数） | 8646 | 8646 | i8 | 1 |
| 115 | B 通道 Equation 配置参数 | <Equation2 A>源设置（8 位整型数） | 8647 | 8647 | i8 | 1 |
| 116 | B 通道 Equation 配置参数 | <Equation3 A>源设置（8 位整型数） | 8648 | 8648 | i8 | 1 |
| 117 | B 通道 Equation 配置参数 | <Equation4 A>源设置（8 位整型数） | 8649 | 8649 | i8 | 1 |
| 118 | B 通道 Equation 配置参数 | <Equation1 B>源设置（8 位整型数） | 8650 | 8650 | i8 | 1 |
| 119 | B 通道 Equation 配置参数 | <Equation2 B>源设置（8 位整型数） | 8651 | 8651 | i8 | 1 |
| 120 | B 通道 Equation 配置参数 | <Equation3 B>源设置（8 位整型数） | 8652 | 8652 | i8 | 1 |
| 121 | B 通道 Equation 配置参数 | <Equation4 B>源设置（8 位整型数） | 8653 | 8653 | i8 | 1 |
| 122 | B 通道 Equation 配置参数 | <Equation1 C>源设置（8 位整型数） | 8654 | 8654 | i8 | 1 |
| 123 | B 通道 Equation 配置参数 | <Equation2 C>源设置（8 位整型数） | 8655 | 8655 | i8 | 1 |
| 124 | B 通道 Equation 配置参数 | <Equation3 C>源设置（8 位整型数） | 8656 | 8656 | i8 | 1 |
| 125 | B 通道 Equation 配置参数 | <Equation4 C>源设置（8 位整型数） | 8657 | 8657 | i8 | 1 |
| 126 | B 通道 Gain TC Input Filter 配置参数 | <Sensitivity>设置（8 位整型数） | 8690 | 8690 | i8 | 1 |
| 127 | B 通道 Gain TC Input Filter 配置参数 | <Reserve>设置（8 位整型数） | 8691 | 8691 | i8 | 1 |
| 128 | B 通道 Gain TC Input Filter 配置参数 | <Source>设置（8 位整型数） | 8692 | 8692 | i8 | 1 |
| 129 | B 通道 Gain TC Input Filter 配置参数 | <Grounding>设置（8 位整型数） | 8693 | 8693 | i8 | 1 |
| 130 | B 通道 Gain TC Input Filter 配置参数 | <Coupling>设置（8 位整型数） | 8694 | 8694 | i8 | 1 |
| 131 | B 通道 Gain TC Input Filter 配置参数 | <Line Notch>设置（8 位整型数） | 8695 | 8695 | i8 | 1 |
| 132 | B 通道 Gain TC Input Filter 配置参数 | <Time Constant>设置（8 位整型数） | 8704 | 8704 | i8 | 1 |
| 133 | B 通道 Gain TC Input Filter 配置参数 | <Filter dB/oct>设置（8 位整型数） | 8705 | 8705 | i8 | 1 |
| 134 | B 通道 Gain TC Input Filter 配置参数 | <Synchronous>设置（8 位整型数） | 8706 | 8706 | i8 | 1 |
| 135 | B 通道 Sample 配置参数 | <Sample Time>设置（64 位浮点数） | 8741 | 8748 | f64 | 1 |
| 136 | B 通道 Sample 配置参数 | <Sample Length>设置（64 位整型数） | 8749 | 8756 | i64 | 1 |
| 137 | B 通道 Sample 配置参数 | <Sample Buffer1>设置（8 位整型数） | 8757 | 8757 | i8 | 1 |
| 138 | B 通道 Sample 配置参数 | <Sample Buffer2>设置（8 位整型数） | 8758 | 8758 | i8 | 1 |
| 139 | B 通道 Sample 配置参数 | <Sample Buffer3>设置（8 位整型数） | 8759 | 8759 | i8 | 1 |
| 140 | B 通道 Sample 配置参数 | <Sample Buffer4>设置（8 位整型数） | 8760 | 8760 | i8 | 1 |
| 141 | B 通道 Sample 配置参数 | <Sample Trigger Mode>设置（8 位整型数） | 8761 | 8761 | i8 | 1 |
| 142 | B 通道 Sample 配置参数 | <Sample Mode>设置（8 位整型数） | 8762 | 8762 | i8 | 1 |
| 143 | B 通道 Sample 配置参数 | <Sample Current Point>设置（64 位整型数） | 8763 | 8770 | i64 | 1 |
| 144 | B 通道状态参数 | Input Overload 状态（8 位整型数） | 8779 | 8779 | i8 | 1 |
| 145 | B 通道状态参数 | Gain Overload 状态（8 位整型数） | 8780 | 8780 | i8 | 1 |
| 146 | B 通道状态参数 | PLL LOCKED 状态（8 位整型数） | 8781 | 8781 | i8 | 1 |
| 147 | IDN 序列号 | IDN 序列号返回值（40Bytes 长度） | 9170 | 9209 | bytes[40] | 1 |


### 3.3 Parser 实现注意事项

1. 图片只给出字段顺序、字节位置、数据类型和更新节奏；没有给出浮点数/整型数的端序。实际 parser 需要用真实 RALL? 返回帧验证端序。
2. `数据位置` 采用闭区间理解，例如 `0~399` 表示 400 Bytes。
3. 50 个测量值字段均为连续数组；每个 64 位浮点数占 8 Bytes，因此 `50 × 8 = 400 Bytes`。
4. 配置参数不是 50 个值，而是每个配置项只返回 1 次参数。
5. 表中存在未列出的间隔区间，例如 `8000~8199`、`8238~8245`、`8284~8291` 等；实现时应按保留/未定义字节处理，不要擅自写入业务含义。
6. 表格写明最后的 3072Bytes 是空字符，且总长度是 12288 Bytes；若按 3072 Bytes 反推，空字符区起点为 `9216`，因此 `9210~9215` 也应视为未定义/保留区，除非真实设备返回验证另有含义。
