//! Run-level OE1022D continuity audit.
//!
//! 这条命令只读取已落盘 artifact，不参与实时链路。
//! 当前目标很收敛：
//! - 基于 `segments.jsonl` 按 segment 审计连续性
//! - 优先读取 `raw/oe1022d.frames.parsed.jsonl`，否则从 `raw + frames.idx` 现算 RALL 字段
//! - 利用 B 通道 X/Y/Freq 字段和帧间时间戳判断是否存在疑似漏窗

use acquisition_runtime::{
    parse_rall_frame_full, FrameIndexRecord, ParsedFrameRecord, SegmentRecord,
};
use serde::Serialize;
use std::fs::{self, File};
use std::io::{BufRead, BufReader, Read, Seek, SeekFrom};
use std::path::{Path, PathBuf};

const DEFAULT_FRAME_GAP_MS: f64 = 50.0;
const GAP_OVERRUN_FLOOR_MS: f64 = 25.0;
const GAP_OVERRUN_RATIO: f64 = 0.8;
const XY_JUMP_SCORE_THRESHOLD: f64 = 8.0;
const B_X_INDEX: usize = 8;
const B_Y_INDEX: usize = 9;
const B_FREQ_INDEX: usize = 10;

pub fn run_audit_continuity(run_dir: &Path, out_path: Option<&Path>) -> Result<PathBuf, String> {
    let parsed_path = run_dir.join("raw/oe1022d.frames.parsed.jsonl");
    let raw_path = run_dir.join("raw/oe1022d.rall");
    let index_path = run_dir.join("raw/oe1022d.frames.idx.jsonl");
    let segments_path = run_dir.join("segments.jsonl");
    let out_path = out_path
        .map(Path::to_path_buf)
        .unwrap_or_else(|| run_dir.join("continuity_audit.json"));

    let (frames, parsed_frames_file) =
        load_parsed_frames(run_dir, &parsed_path, &raw_path, &index_path)?;
    let segments: Vec<SegmentRecord> = read_jsonl(&segments_path)?;

    let mut segment_reports = Vec::new();
    let mut suspected_missing_boundaries = 0_usize;
    let mut suspected_content_boundaries = 0_usize;
    let mut max_gap_ms = 0.0_f64;

    for segment in &segments {
        let Some(frame_seq_start) = segment.frame_seq_start else {
            continue;
        };
        let Some(frame_seq_end) = segment.frame_seq_end else {
            continue;
        };

        let report = audit_segment(segment, &frames, frame_seq_start, frame_seq_end)?;
        suspected_missing_boundaries += report.suspected_missing_boundaries;
        suspected_content_boundaries += report.suspected_content_boundaries;
        max_gap_ms = max_gap_ms.max(report.max_observed_gap_ms);
        segment_reports.push(report);
    }

    let frames_usable = frames
        .iter()
        .filter(|frame| frame.parse_status == "ok" && frame.measurement_matrix.is_some())
        .count();
    let frames_unique = frames
        .iter()
        .filter(|frame| {
            frame.parse_status == "ok"
                && frame.measurement_matrix.is_some()
                && frame.duplicate_hint.is_none()
        })
        .count();
    let verdict = if suspected_missing_boundaries > 0 {
        "suspected_missing_windows"
    } else if suspected_content_boundaries > 0 {
        "suspected_content_discontinuity"
    } else {
        "continuous"
    };

    let report = ContinuityAuditReport {
        schema_version: 1,
        run_dir: run_dir.display().to_string(),
        parsed_frames_file,
        segments_file: path_relative_to(run_dir, &segments_path),
        frames_total: frames.len(),
        frames_usable,
        frames_unique,
        segments_total: segments.len(),
        segments_audited: segment_reports.len(),
        suspected_missing_boundaries,
        suspected_content_boundaries,
        max_observed_gap_ms: max_gap_ms,
        verdict: verdict.to_string(),
        segment_reports,
    };

    if let Some(parent) = out_path.parent() {
        fs::create_dir_all(parent)
            .map_err(|err| format!("无法创建 continuity audit 目录 {}: {err}", parent.display()))?;
    }
    let text = serde_json::to_string_pretty(&report)
        .map_err(|err| format!("continuity audit JSON 序列化失败: {err}"))?;
    fs::write(&out_path, text)
        .map_err(|err| format!("无法写入 continuity audit {}: {err}", out_path.display()))?;

    println!(
        "continuity audit 完成: verdict={}, suspected_missing_boundaries={}, suspected_content_boundaries={}",
        report.verdict, report.suspected_missing_boundaries, report.suspected_content_boundaries
    );
    println!("continuity audit 已写入: {}", out_path.display());

    Ok(out_path)
}

fn audit_segment(
    segment: &SegmentRecord,
    frames: &[ParsedFrameRecord],
    frame_seq_start: u64,
    frame_seq_end: u64,
) -> Result<SegmentContinuityReport, String> {
    let segment_frames = frames
        .iter()
        .filter(|frame| frame.frame_seq >= frame_seq_start && frame.frame_seq <= frame_seq_end)
        .collect::<Vec<_>>();
    let usable_frames = segment_frames
        .iter()
        .copied()
        .filter(|frame| frame.parse_status == "ok" && frame.measurement_matrix.is_some())
        .collect::<Vec<_>>();
    let unique_frames = usable_frames
        .iter()
        .copied()
        .filter(|frame| frame.duplicate_hint.is_none())
        .collect::<Vec<_>>();

    let median_frame_gap_ms = median_frame_gap_ms(&usable_frames).unwrap_or(DEFAULT_FRAME_GAP_MS);
    let mut suspect_boundaries = Vec::new();
    let mut max_observed_gap_ms = 0.0_f64;
    let mut max_x_jump_score = 0.0_f64;
    let mut max_y_jump_score = 0.0_f64;

    for pair in unique_frames.windows(2) {
        let prev = pair[0];
        let next = pair[1];
        let boundary = analyze_boundary(prev, next, median_frame_gap_ms)?;
        max_observed_gap_ms = max_observed_gap_ms.max(boundary.gap_ms);
        max_x_jump_score = max_x_jump_score.max(boundary.x_jump_score);
        max_y_jump_score = max_y_jump_score.max(boundary.y_jump_score);
        if boundary.is_suspected() {
            suspect_boundaries.push(boundary);
        }
    }

    let suspected_missing_boundaries = suspect_boundaries
        .iter()
        .filter(|boundary| {
            boundary
                .reasons
                .iter()
                .any(|reason| reason == "timing_gap_overrun")
        })
        .count();
    let suspected_content_boundaries = suspect_boundaries
        .iter()
        .filter(|boundary| {
            boundary
                .reasons
                .iter()
                .any(|reason| reason == "content_jump_xy")
        })
        .count();
    let verdict = if suspected_missing_boundaries > 0 {
        "suspected_missing_windows"
    } else if suspected_content_boundaries > 0 {
        "suspected_content_discontinuity"
    } else {
        "continuous"
    };

    Ok(SegmentContinuityReport {
        point_id: segment.point_id.clone(),
        segment_id: segment.segment_id.clone(),
        frames_total: segment_frames.len(),
        frames_usable: usable_frames.len(),
        frames_unique: unique_frames.len(),
        duplicate_frames: usable_frames.len().saturating_sub(unique_frames.len()),
        boundaries_evaluated: unique_frames.len().saturating_sub(1),
        suspected_missing_boundaries,
        suspected_content_boundaries,
        median_frame_gap_ms,
        max_observed_gap_ms,
        max_x_jump_score,
        max_y_jump_score,
        verdict: verdict.to_string(),
        suspect_boundaries,
    })
}

fn analyze_boundary(
    prev: &ParsedFrameRecord,
    next: &ParsedFrameRecord,
    median_frame_gap_ms: f64,
) -> Result<BoundarySuspicion, String> {
    let prev_matrix = prev
        .measurement_matrix
        .as_ref()
        .ok_or_else(|| format!("frame {} 缺少 measurement_matrix", prev.frame_seq))?;
    let next_matrix = next
        .measurement_matrix
        .as_ref()
        .ok_or_else(|| format!("frame {} 缺少 measurement_matrix", next.frame_seq))?;

    let duplicates_between = next
        .frame_seq
        .saturating_sub(prev.frame_seq)
        .saturating_sub(1);
    let gap_ms = (next.monotonic_ns.saturating_sub(prev.monotonic_ns)) as f64 / 1_000_000.0;
    let expected_gap_ms = median_frame_gap_ms * (duplicates_between + 1) as f64;
    let gap_overrun_ms = gap_ms - expected_gap_ms;

    let x_jump_score = boundary_jump_score(prev_matrix, next_matrix, B_X_INDEX, 1e-9)?;
    let y_jump_score = boundary_jump_score(prev_matrix, next_matrix, B_Y_INDEX, 1e-9)?;
    let b_freq_delta_hz = boundary_bridge_delta(prev_matrix, next_matrix, B_FREQ_INDEX)?;

    let gap_overrun_threshold = GAP_OVERRUN_FLOOR_MS.max(median_frame_gap_ms * GAP_OVERRUN_RATIO);
    let mut reasons = Vec::new();
    if gap_overrun_ms > gap_overrun_threshold {
        reasons.push("timing_gap_overrun".to_string());
    }
    if x_jump_score > XY_JUMP_SCORE_THRESHOLD && y_jump_score > XY_JUMP_SCORE_THRESHOLD {
        reasons.push("content_jump_xy".to_string());
    }

    Ok(BoundarySuspicion {
        prev_frame_seq: prev.frame_seq,
        next_frame_seq: next.frame_seq,
        duplicates_between,
        gap_ms,
        expected_gap_ms,
        gap_overrun_ms,
        x_jump_score,
        y_jump_score,
        b_freq_delta_hz,
        reasons,
    })
}

fn median_frame_gap_ms(frames: &[&ParsedFrameRecord]) -> Option<f64> {
    let mut gaps = frames
        .windows(2)
        .map(|pair| {
            (pair[1].monotonic_ns.saturating_sub(pair[0].monotonic_ns)) as f64 / 1_000_000.0
        })
        .filter(|gap| gap.is_finite() && *gap >= 0.0)
        .collect::<Vec<_>>();
    if gaps.is_empty() {
        return None;
    }
    gaps.sort_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal));
    Some(gaps[gaps.len() / 2])
}

fn boundary_jump_score(
    prev_matrix: &[Vec<f64>],
    next_matrix: &[Vec<f64>],
    field_index: usize,
    floor: f64,
) -> Result<f64, String> {
    let bridge = boundary_bridge_delta(prev_matrix, next_matrix, field_index)?;
    let prev_scale = edge_delta_scale(prev_matrix, field_index, true)?;
    let next_scale = edge_delta_scale(next_matrix, field_index, false)?;
    let scale = prev_scale.max(next_scale).max(floor);
    Ok(bridge / scale)
}

fn boundary_bridge_delta(
    prev_matrix: &[Vec<f64>],
    next_matrix: &[Vec<f64>],
    field_index: usize,
) -> Result<f64, String> {
    let prev = prev_matrix
        .get(field_index)
        .ok_or_else(|| format!("field_index 越界: {field_index}"))?;
    let next = next_matrix
        .get(field_index)
        .ok_or_else(|| format!("field_index 越界: {field_index}"))?;
    let prev_tail = prev
        .last()
        .copied()
        .ok_or_else(|| format!("field {field_index} 缺少 prev tail"))?;
    let next_head = next
        .first()
        .copied()
        .ok_or_else(|| format!("field {field_index} 缺少 next head"))?;
    Ok((next_head - prev_tail).abs())
}

fn edge_delta_scale(matrix: &[Vec<f64>], field_index: usize, tail: bool) -> Result<f64, String> {
    let values = matrix
        .get(field_index)
        .ok_or_else(|| format!("field_index 越界: {field_index}"))?;
    if values.len() < 2 {
        return Ok(0.0);
    }

    let mut diffs = values
        .windows(2)
        .map(|pair| (pair[1] - pair[0]).abs())
        .filter(|delta| delta.is_finite())
        .collect::<Vec<_>>();
    if diffs.is_empty() {
        return Ok(0.0);
    }

    if diffs.len() > 5 {
        diffs = if tail {
            diffs[diffs.len() - 5..].to_vec()
        } else {
            diffs[..5].to_vec()
        };
    }
    diffs.sort_by(|a, b| a.partial_cmp(b).unwrap_or(std::cmp::Ordering::Equal));
    Ok(diffs[diffs.len() / 2])
}

fn path_relative_to(run_dir: &Path, path: &Path) -> String {
    path.strip_prefix(run_dir)
        .unwrap_or(path)
        .display()
        .to_string()
}

fn read_jsonl<T: serde::de::DeserializeOwned>(path: &Path) -> Result<Vec<T>, String> {
    let file =
        File::open(path).map_err(|err| format!("无法打开 JSONL 文件 {}: {err}", path.display()))?;
    let reader = BufReader::new(file);
    let mut records = Vec::new();
    for (index, line) in reader.lines().enumerate() {
        let line = line.map_err(|err| {
            format!(
                "读取 JSONL 文件 {} 第 {} 行失败: {err}",
                path.display(),
                index + 1
            )
        })?;
        if line.trim().is_empty() {
            continue;
        }
        let record = serde_json::from_str(&line).map_err(|err| {
            format!(
                "解析 JSONL 文件 {} 第 {} 行失败: {err}",
                path.display(),
                index + 1
            )
        })?;
        records.push(record);
    }
    Ok(records)
}

fn load_parsed_frames(
    run_dir: &Path,
    parsed_path: &Path,
    raw_path: &Path,
    index_path: &Path,
) -> Result<(Vec<ParsedFrameRecord>, String), String> {
    if parsed_path.exists() {
        return Ok((
            read_jsonl(parsed_path)?,
            path_relative_to(run_dir, parsed_path),
        ));
    }

    let index: Vec<FrameIndexRecord> = read_jsonl(index_path)?;
    let mut raw_file = File::open(raw_path)
        .map_err(|err| format!("无法打开 raw 文件 {}: {err}", raw_path.display()))?;
    let mut frames = Vec::with_capacity(index.len());

    for record in index {
        let mut payload = vec![0_u8; record.raw_len];
        raw_file
            .seek(SeekFrom::Start(record.raw_offset))
            .map_err(|err| format!("raw seek 失败 offset={}: {err}", record.raw_offset))?;
        raw_file
            .read_exact(&mut payload)
            .map_err(|err| format!("raw 读取失败 frame_seq={}: {err}", record.frame_seq))?;
        let parsed = parse_rall_frame_full(&payload).map_err(|err| {
            format!(
                "从 raw 重建 parsed frame 失败 frame_seq={}: {err}",
                record.frame_seq
            )
        })?;
        frames.push(ParsedFrameRecord {
            frame_seq: record.frame_seq,
            ts: record.ts,
            monotonic_ns: record.monotonic_ns,
            raw_offset: record.raw_offset,
            raw_len: record.raw_len,
            transport_status: "payload_committed".to_string(),
            parse_status: record.parse_status,
            duplicate_hint: record.duplicate_of,
            packet_counter_candidate_u8: record.packet_counter_candidate_u8,
            packet_counter_gap: record.packet_counter_gap,
            ..parsed
        });
    }

    Ok((
        frames,
        format!(
            "{} (derived_from_raw + {})",
            path_relative_to(run_dir, raw_path),
            path_relative_to(run_dir, index_path)
        ),
    ))
}

#[derive(Debug, Serialize)]
struct ContinuityAuditReport {
    schema_version: u32,
    run_dir: String,
    parsed_frames_file: String,
    segments_file: String,
    frames_total: usize,
    frames_usable: usize,
    frames_unique: usize,
    segments_total: usize,
    segments_audited: usize,
    suspected_missing_boundaries: usize,
    suspected_content_boundaries: usize,
    max_observed_gap_ms: f64,
    verdict: String,
    segment_reports: Vec<SegmentContinuityReport>,
}

#[derive(Debug, Serialize)]
struct SegmentContinuityReport {
    point_id: String,
    segment_id: String,
    frames_total: usize,
    frames_usable: usize,
    frames_unique: usize,
    duplicate_frames: usize,
    boundaries_evaluated: usize,
    suspected_missing_boundaries: usize,
    suspected_content_boundaries: usize,
    median_frame_gap_ms: f64,
    max_observed_gap_ms: f64,
    max_x_jump_score: f64,
    max_y_jump_score: f64,
    verdict: String,
    suspect_boundaries: Vec<BoundarySuspicion>,
}

#[derive(Debug, Serialize)]
struct BoundarySuspicion {
    prev_frame_seq: u64,
    next_frame_seq: u64,
    duplicates_between: u64,
    gap_ms: f64,
    expected_gap_ms: f64,
    gap_overrun_ms: f64,
    x_jump_score: f64,
    y_jump_score: f64,
    b_freq_delta_hz: f64,
    reasons: Vec<String>,
}

impl BoundarySuspicion {
    fn is_suspected(&self) -> bool {
        !self.reasons.is_empty()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn parsed_frame(
        frame_seq: u64,
        monotonic_ns: u64,
        duplicate_hint: Option<u64>,
        x_start: f64,
        x_step: f64,
        y_start: f64,
        y_step: f64,
    ) -> ParsedFrameRecord {
        fn series(start: f64, step: f64) -> Vec<f64> {
            (0..50).map(|index| start + step * index as f64).collect()
        }

        let mut matrix = vec![vec![0.0; 50]; 20];
        matrix[B_X_INDEX] = series(x_start, x_step);
        matrix[B_Y_INDEX] = series(y_start, y_step);
        matrix[B_FREQ_INDEX] = vec![500.0; 50];

        ParsedFrameRecord {
            schema_version: 1,
            layout_version: "test".to_string(),
            frame_seq,
            ts: format!("frame_{frame_seq}"),
            monotonic_ns,
            raw_offset: frame_seq * 12288,
            raw_len: 12288,
            transport_status: "payload_committed".to_string(),
            parse_status: "ok".to_string(),
            padding_status: "zero".to_string(),
            duplicate_hint,
            packet_counter_candidate_u8: Some(frame_seq as u8),
            packet_counter_gap: None,
            parse_error: None,
            measurement_field_order: Vec::new(),
            measurement_matrix: Some(matrix),
            scalar_fields: Vec::new(),
            b_ref_source_code: Some(0),
            b_ref_slope_code: Some(2),
            b_ref_current_freq_hz: Some(500.0),
            b_input_overload: Some(false),
            b_gain_overload: Some(false),
            b_pll_locked: Some(true),
        }
    }

    #[test]
    fn duplicate_boundary_gap_is_not_suspected() {
        let prev = parsed_frame(10, 0, None, 0.0, 1.0, 10.0, 1.0);
        let next = parsed_frame(12, 100_000_000, None, 50.0, 1.0, 60.0, 1.0);

        let boundary = analyze_boundary(&prev, &next, 50.0).unwrap();
        assert_eq!(boundary.duplicates_between, 1);
        assert!(boundary.reasons.is_empty());
    }

    #[test]
    fn missing_window_gap_is_suspected() {
        let prev = parsed_frame(10, 0, None, 0.0, 1.0, 10.0, 1.0);
        let next = parsed_frame(11, 100_000_000, None, 50.0, 1.0, 60.0, 1.0);

        let boundary = analyze_boundary(&prev, &next, 50.0).unwrap();
        assert!(boundary
            .reasons
            .iter()
            .any(|reason| reason == "timing_gap_overrun"));
    }

    #[test]
    fn content_jump_is_suspected() {
        let prev = parsed_frame(10, 0, None, 0.0, 1.0, 10.0, 1.0);
        let next = parsed_frame(11, 50_000_000, None, 500.0, 1.0, 1000.0, 1.0);

        let boundary = analyze_boundary(&prev, &next, 50.0).unwrap();
        assert!(boundary
            .reasons
            .iter()
            .any(|reason| reason == "content_jump_xy"));
    }
}
