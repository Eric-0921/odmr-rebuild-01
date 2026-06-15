# OE1300 手册 Markdown 校对 Roadmap

## 一、源文件清单

| 文件 | 说明 |
|------|------|
| `oe1300sereies-lockin-manual-operation.pdf` | 原始 PDF，95 页，作为校对真值 |
| `oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md` | PaddleOCR 生成的 Markdown（约 1945 行） |
| `oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.json` | PaddleOCR 布局 JSON，95 个 page 对象，每个 page 含 `prunedResult.parsing_res_list`（block 级标签、文字、bbox） |

输出目录：`clean/`

---

## 二、章节结构（以 PDF 为准）

共 **7 个顶层章节**。章节映射表已写入 `clean/chapter_map.json`：

| 章节 | 标题 | 文档页 | PDF 页 | Markdown 行号 |
|------|------|--------|--------|---------------|
| 1 | 技术参数 | 1–3 | 5–7 | 99–280 |
| 2 | 安全性和使用准备 | 4–5 | 8–9 | 281–338 |
| 3 | 锁相放大器基础 | 6–25 | 10–29 | 339–893 |
| 4 | 产品概述 | 26–28 | 30–32 | 894–993 |
| 5 | 远程编程 | 29–42 | 33–46 | 994–1118 |
| 6 | PC 软件安装使用说明 | 43–75 | 47–79 | 1119–1775 |
| 7 | 操作实例 | 76–91 | 80–95 | 1776–1945 |

---

## 三、OCR / 真值策略

- **主真值**：PaddleOCR JSON（`prunedResult.parsing_res_list` 中的 `block_content`），字符识别质量最好。
- **辅助参考**：`pdftotext -layout` 提取的 PDF 文本层（`clean/chapter_XX_pdf.txt`），用于快速核对行序和段落边界。
- **结构参考**：原 Markdown（`oe1300sereies-lockin-manual-operation.pdf_by_PaddleOCR-VL-1.6.md`）。
- **本地 tesseract 效果差**，不采用。

---

## 四、校对风格（以 `clean/chapter_01_sample.md` 为准）

1. **标题层级**：顶层 `# 1. 技术参数`，二级 `## 1.1 信号通道`，三级 `### 输入/相位` 等。
2. **参数表格**：把原 PaddleOCR 的零散项目符号整理成“参数 / 规格”标准 Markdown 表格。
3. **公式**：使用 LaTeX，如 `$10^6\ \mathrm{V/A}$`、`$V_{IH} > 3\ \mathrm{V}$`、`$\pm 10\ \mathrm{V}$`。
4. **图片**：保留原 Markdown 中的 `<img>` 外链和图注，不得删除。
5. **单位**：用 `\mathrm{}` 包裹单位，如 `\mathrm{nV/\sqrt{Hz}}`。
6. **OCR 纠错**：
   - 修正编号错误：`3.20E1300` → `3.2 OE1300`、`3. 17` → `3.17`、`5.10E1300` → `5.1 OE1300`。
   - 修正孤立标题：`## 1、RJ45 网络接口` 归到 4.3 下；`## 4. 完成以上操作之后...` 归到第 7 章正文。
   - 删除被误识别的页眉 `OE1300 Lock-In Amplifier` 标题。

---

## 五、Subagent 并行执行计划

每轮最多启动 4 个 `coder` subagent，每轮结束后检查输出并启动下一轮。

### Round 1：Ch2、Ch3、Ch4、Ch5
- 输入：`clean/chapter_map.json` 中对应章节的 `md_line_range`、`pdf_pages`。
- 输出：`clean/chapter_02.md`、`clean/chapter_03.md`、`clean/chapter_04.md`、`clean/chapter_05.md`。

### Round 2：Ch6、Ch7
- Ch6 内容较长（33 PDF 页），但仍作为单章处理。
- 输出：`clean/chapter_06.md`、`clean/chapter_07.md`。

### Round 3：合并与终审
- 合并 `chapter_01_sample.md`（或重命名为 `chapter_01.md`）、`chapter_02.md` … `chapter_07.md`。
- 生成 `clean/oe1300_manual_clean.md`。
- 输出 `clean/diff_summary.md`，列明主要修改点。

---

## 六、参考文件（已生成）

| 文件 | 说明 |
|------|------|
| `clean/chapter_map.json` | 7 章映射表 |
| `clean/chapter_01_sample.md` | 第 1 章校对样例 |
| `clean/chapter_XX_pdf.txt` | 每章 PDF 文本（XX=01..07） |
| `clean/chapter_XX_json_summary.json` | 每章 JSON block 摘要 |
| `clean/pdf_extracted.txt` | 完整 PDF 文本 |

---

## 七、当前进度

- [x] 目录勘察与章节映射
- [x] 第 1 章样例生成
- [x] 参考文件预提取
- [ ] Round 1：Ch2–Ch5 校对
- [ ] Round 2：Ch6–Ch7 校对
- [ ] Round 3：合并、终审、diff 总结
