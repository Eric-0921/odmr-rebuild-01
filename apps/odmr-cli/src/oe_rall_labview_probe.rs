//! OE1022D RALL LabVIEW-style minimal probe.
//!
//! This diagnostic path intentionally avoids point runtime, SMB, laser, parsing,
//! first-byte deadlines, zero-byte retry, and hot-path input clearing.

use acquisition_runtime::{FrameIndexRecord, RALL_FRAME_BYTES};
use oe1022d_transport::{Oe1022dTransport, Oe1022dTransportConfig};
use serde::Serialize;
use serde_json::json;
use station_resolver::{resolve_station, DeviceKind, DeviceSpec, StationSpec, TransportHint};
use std::collections::BTreeMap;
use std::fs::{self, File, OpenOptions};
use std::io::Write;
use std::path::{Path, PathBuf};
use std::sync::mpsc::{channel, Receiver};
use std::thread;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};

const DEVICE_PACKET_COUNTER_OFFSET: usize = 12287;

#[derive(Debug, Clone)]
pub struct LabviewProbeOptions {
    pub station: PathBuf,
    pub out_dir: PathBuf,
    pub frames: usize,
    pub post_write_delay_ms: u64,
    pub read_timeout_ms: u64,
    pub max_read_errors: usize,
}

pub fn run_oe_rall_labview_probe(options: &LabviewProbeOptions) -> Result<ProbeSummary, String> {
    let station = read_station_spec(&options.station)?;
    let resolved = resolve_station(&station);
    let oe_device = find_first_device(&resolved.resolved_spec, DeviceKind::Oe1022d)?;
    let device_config = oe_config(
        oe_device,
        options.post_write_delay_ms,
        options.read_timeout_ms,
    )?;

    fs::create_dir_all(options.out_dir.join("raw")).map_err(|err| {
        format!(
            "无法创建 probe 输出目录 {}: {err}",
            options.out_dir.display()
        )
    })?;

    let run_id = format!("oe_rall_labview_probe_{}", now_ts_string());
    let started_at = now_ts_string();
    let run_start = Instant::now();
    let raw_path = options.out_dir.join("raw/oe1022d.rall");
    let index_path = options.out_dir.join("raw/oe1022d.frames.idx.jsonl");
    let events_path = options.out_dir.join("events.jsonl");
    let summary_path = options.out_dir.join("summary.json");

    let (frame_tx, frame_rx) = channel::<ProbeFrame>();
    let writer_raw_path = raw_path.clone();
    let writer_index_path = index_path.clone();
    let writer_join =
        thread::spawn(move || writer_loop(frame_rx, &writer_raw_path, &writer_index_path));

    let producer_result = (|| -> Result<ProducerSummary, String> {
        let mut events_file = OpenOptions::new()
            .create(true)
            .write(true)
            .truncate(true)
            .open(&events_path)
            .map_err(|err| format!("无法打开 events 文件 {}: {err}", events_path.display()))?;
        let mut transport = Oe1022dTransport::open(&device_config)
            .map_err(|err| format!("打开 OE1022D 失败: {err}"))?;
        transport
            .clear_all()
            .map_err(|err| format!("OE1022D 启动前清空 I/O buffer 失败: {err}"))?;
        let mut frames_ok = 0_usize;
        let mut read_errors = 0_usize;
        let mut timeout_count = 0_usize;

        let mut read_attempts = 0_usize;
        while frames_ok < options.frames {
            let read_started = Instant::now();
            read_attempts += 1;
            match transport.query_rall_frame_labview_exact(RALL_FRAME_BYTES) {
                Ok(payload) => {
                    frames_ok += 1;
                    frame_tx
                        .send(ProbeFrame {
                            frame_seq: (frames_ok - 1) as u64,
                            ts: now_ts_string(),
                            monotonic_ns: monotonic_ns(&run_start),
                            payload,
                        })
                        .map_err(|err| format!("probe writer channel 已断开: {err}"))?;
                }
                Err(err) => {
                    read_errors += 1;
                    if err.to_string().contains("超时") || err.to_string().contains("timeout") {
                        timeout_count += 1;
                    }
                    append_jsonl(
                        &mut events_file,
                        &ProbeEvent {
                            ts: now_ts_string(),
                            monotonic_ns: monotonic_ns(&run_start),
                            event: "rall_read_error".to_string(),
                            data: json!({
                                "read_attempt": read_attempts,
                                "frames_ok": frames_ok,
                                "elapsed_ms": read_started.elapsed().as_secs_f64() * 1000.0,
                                "error": err.to_string()
                            }),
                        },
                    )?;
                    if read_errors >= options.max_read_errors {
                        break;
                    }
                }
            }
        }

        drop(frame_tx);
        Ok(ProducerSummary {
            frames_requested: options.frames,
            frames_ok,
            read_attempts,
            read_errors,
            timeout_count,
        })
    })();

    let writer_summary = writer_join
        .join()
        .map_err(|_| "probe writer 线程 join 失败".to_string())??;
    let producer_summary = producer_result?;
    let ended_at = now_ts_string();

    let summary = ProbeSummary {
        run_id,
        station: options.station.display().to_string(),
        device_id: oe_device.device_id.clone(),
        port_path: device_config.port_path.clone(),
        baud_rate: device_config.baud_rate,
        command: "RALL?".to_string(),
        frame_bytes: RALL_FRAME_BYTES,
        post_write_delay_ms: options.post_write_delay_ms,
        read_timeout_ms: options.read_timeout_ms,
        max_read_errors: options.max_read_errors,
        started_at,
        ended_at,
        elapsed_ms: run_start.elapsed().as_secs_f64() * 1000.0,
        frames_requested: producer_summary.frames_requested,
        frames_ok: producer_summary.frames_ok,
        read_attempts: producer_summary.read_attempts,
        read_errors: producer_summary.read_errors,
        timeout_count: producer_summary.timeout_count,
        writer: writer_summary,
        raw_file: "raw/oe1022d.rall".to_string(),
        index_file: "raw/oe1022d.frames.idx.jsonl".to_string(),
    };

    write_pretty_json(&summary_path, &summary)?;
    println!(
        "OE RALL LabVIEW probe 完成: frames_ok={}, timeout_count={}, counter_delta_gt1={}, out_dir={}",
        summary.frames_ok,
        summary.timeout_count,
        summary.writer.packet_counter.delta_gt1_count,
        options.out_dir.display()
    );
    Ok(summary)
}

fn writer_loop(
    frame_rx: Receiver<ProbeFrame>,
    raw_path: &Path,
    index_path: &Path,
) -> Result<WriterSummary, String> {
    let mut raw_file = OpenOptions::new()
        .create(true)
        .write(true)
        .truncate(true)
        .open(raw_path)
        .map_err(|err| format!("无法打开 raw 文件 {}: {err}", raw_path.display()))?;
    let mut index_file = OpenOptions::new()
        .create(true)
        .write(true)
        .truncate(true)
        .open(index_path)
        .map_err(|err| format!("无法打开 frame index 文件 {}: {err}", index_path.display()))?;

    let mut frames_written = 0_usize;
    let mut raw_len_bad_count = 0_usize;
    let mut next_raw_offset = 0_u64;
    let mut counter_audit = PacketCounterAudit::default();

    while let Ok(frame) = frame_rx.recv() {
        let raw_len = frame.payload.len();
        if raw_len != RALL_FRAME_BYTES {
            raw_len_bad_count += 1;
        }
        raw_file
            .write_all(&frame.payload)
            .map_err(|err| format!("写入 raw 文件失败: {err}"))?;
        append_jsonl(
            &mut index_file,
            &FrameIndexRecord {
                frame_seq: frame.frame_seq,
                ts: frame.ts,
                monotonic_ns: frame.monotonic_ns,
                raw_offset: next_raw_offset,
                raw_len,
                parse_status: "not_parsed".to_string(),
                duplicate_of: None,
            },
        )?;
        if raw_len > DEVICE_PACKET_COUNTER_OFFSET {
            counter_audit.record(
                frame.frame_seq,
                frame.monotonic_ns,
                frame.payload[DEVICE_PACKET_COUNTER_OFFSET],
            );
        }
        next_raw_offset += raw_len as u64;
        frames_written += 1;
    }

    Ok(WriterSummary {
        frames_written,
        raw_len_bad_count,
        packet_counter: counter_audit.finish(),
    })
}

#[derive(Debug)]
struct ProbeFrame {
    frame_seq: u64,
    ts: String,
    monotonic_ns: u64,
    payload: Vec<u8>,
}

#[derive(Debug, Serialize)]
struct ProbeEvent {
    ts: String,
    monotonic_ns: u64,
    event: String,
    data: serde_json::Value,
}

#[derive(Debug)]
struct ProducerSummary {
    frames_requested: usize,
    frames_ok: usize,
    read_attempts: usize,
    read_errors: usize,
    timeout_count: usize,
}

#[derive(Debug, Serialize)]
pub struct ProbeSummary {
    run_id: String,
    station: String,
    device_id: String,
    port_path: String,
    baud_rate: u32,
    command: String,
    frame_bytes: usize,
    post_write_delay_ms: u64,
    read_timeout_ms: u64,
    max_read_errors: usize,
    started_at: String,
    ended_at: String,
    elapsed_ms: f64,
    frames_requested: usize,
    frames_ok: usize,
    read_attempts: usize,
    read_errors: usize,
    timeout_count: usize,
    writer: WriterSummary,
    raw_file: String,
    index_file: String,
}

#[derive(Debug, Serialize)]
struct WriterSummary {
    frames_written: usize,
    raw_len_bad_count: usize,
    packet_counter: PacketCounterSummary,
}

#[derive(Debug, Default)]
struct PacketCounterAudit {
    previous: Option<PacketCounterSample>,
    frames_audited: usize,
    delta_1_count: usize,
    delta_0_count: usize,
    delta_gt1_count: usize,
    estimated_missing_windows: usize,
    delta_counts: BTreeMap<u8, usize>,
    first_counter: Option<u8>,
    last_counter: Option<u8>,
    gaps_ms: Vec<f64>,
}

impl PacketCounterAudit {
    fn record(&mut self, _frame_seq: u64, monotonic_ns: u64, counter: u8) {
        if self.first_counter.is_none() {
            self.first_counter = Some(counter);
        }
        if let Some(previous) = self.previous {
            let delta = counter.wrapping_sub(previous.counter);
            *self.delta_counts.entry(delta).or_default() += 1;
            self.gaps_ms
                .push(monotonic_ns.saturating_sub(previous.monotonic_ns) as f64 / 1_000_000.0);
            match delta {
                0 => self.delta_0_count += 1,
                1 => self.delta_1_count += 1,
                other => {
                    self.delta_gt1_count += 1;
                    self.estimated_missing_windows += other.saturating_sub(1) as usize;
                }
            }
        }
        self.previous = Some(PacketCounterSample {
            monotonic_ns,
            counter,
        });
        self.last_counter = Some(counter);
        self.frames_audited += 1;
    }

    fn finish(mut self) -> PacketCounterSummary {
        self.gaps_ms.sort_by(f64::total_cmp);
        let delta_counts = self
            .delta_counts
            .into_iter()
            .map(|(delta, count)| PacketCounterDeltaCount { delta, count })
            .collect();
        PacketCounterSummary {
            offset: DEVICE_PACKET_COUNTER_OFFSET,
            frames_audited: self.frames_audited,
            boundaries_evaluated: self.frames_audited.saturating_sub(1),
            first_counter: self.first_counter,
            last_counter: self.last_counter,
            delta_1_count: self.delta_1_count,
            delta_0_count: self.delta_0_count,
            delta_gt1_count: self.delta_gt1_count,
            estimated_missing_windows: self.estimated_missing_windows,
            delta_counts,
            gap_median_ms: percentile(&self.gaps_ms, 0.5),
            gap_p95_ms: percentile(&self.gaps_ms, 0.95),
            gap_p99_ms: percentile(&self.gaps_ms, 0.99),
            gap_max_ms: self.gaps_ms.last().copied(),
        }
    }
}

#[derive(Debug, Clone, Copy)]
struct PacketCounterSample {
    monotonic_ns: u64,
    counter: u8,
}

#[derive(Debug, Serialize)]
struct PacketCounterSummary {
    offset: usize,
    frames_audited: usize,
    boundaries_evaluated: usize,
    first_counter: Option<u8>,
    last_counter: Option<u8>,
    delta_1_count: usize,
    delta_0_count: usize,
    delta_gt1_count: usize,
    estimated_missing_windows: usize,
    delta_counts: Vec<PacketCounterDeltaCount>,
    gap_median_ms: Option<f64>,
    gap_p95_ms: Option<f64>,
    gap_p99_ms: Option<f64>,
    gap_max_ms: Option<f64>,
}

#[derive(Debug, Serialize)]
struct PacketCounterDeltaCount {
    delta: u8,
    count: usize,
}

fn percentile(sorted: &[f64], p: f64) -> Option<f64> {
    if sorted.is_empty() {
        return None;
    }
    let index = ((sorted.len() - 1) as f64 * p).round() as usize;
    sorted.get(index).copied()
}

fn read_station_spec(path: &Path) -> Result<StationSpec, String> {
    let text = fs::read_to_string(path)
        .map_err(|err| format!("无法读取 station 配置 {}: {err}", path.display()))?;
    serde_json::from_str(&text)
        .map_err(|err| format!("station 配置 JSON 解析失败 {}: {err}", path.display()))
}

fn find_first_device(spec: &StationSpec, kind: DeviceKind) -> Result<&DeviceSpec, String> {
    spec.devices
        .iter()
        .find(|device| device.kind == kind)
        .ok_or_else(|| format!("station 中缺少设备 kind={kind:?}"))
}

fn oe_config(
    device: &DeviceSpec,
    post_write_delay_ms: u64,
    read_timeout_ms: u64,
) -> Result<Oe1022dTransportConfig, String> {
    let TransportHint::SerialPort {
        port_path,
        baud_rate,
    } = &device.transport_hint
    else {
        return Err(format!("设备 {} 不是 serial_port", device.device_id));
    };
    Ok(Oe1022dTransportConfig {
        port_path: port_path.clone(),
        baud_rate: *baud_rate,
        timeout: Duration::from_millis(read_timeout_ms),
        rall_post_write_delay: Duration::from_millis(post_write_delay_ms),
        ..Oe1022dTransportConfig::default()
    })
}

fn append_jsonl<T: Serialize>(file: &mut File, value: &T) -> Result<(), String> {
    let line = serde_json::to_string(value).map_err(|err| format!("JSON 序列化失败: {err}"))?;
    writeln!(file, "{line}").map_err(|err| format!("写入 JSONL 失败: {err}"))
}

fn write_pretty_json<T: Serialize>(path: &Path, value: &T) -> Result<(), String> {
    let text =
        serde_json::to_string_pretty(value).map_err(|err| format!("JSON 序列化失败: {err}"))?;
    fs::write(path, text).map_err(|err| format!("无法写入 JSON {}: {err}", path.display()))
}

fn now_ts_string() -> String {
    let now = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("system time before unix epoch");
    let secs = now.as_secs();
    let millis = now.subsec_millis();
    format!("{secs}.{millis:03}Z")
}

fn monotonic_ns(start_instant: &Instant) -> u64 {
    start_instant.elapsed().as_nanos() as u64
}
