use acquisition_runtime::{AcquisitionRunPlan, EventRecord, LaserBackgroundMode};
use serde::{Deserialize, Serialize};
use std::collections::{HashMap, VecDeque};
use std::sync::{Arc, Mutex};
use tokio::sync::broadcast;

const RECENT_EVENT_LIMIT: usize = 24;

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RecentRunRecord {
    pub run_id: String,
    pub status: String,
    pub started_at: String,
    pub ended_at: String,
    pub output_dir: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct ServiceStatus {
    pub service_state: String,
    #[serde(default)]
    pub active_run_id: Option<String>,
    pub workspace_root: String,
    pub config_root: String,
    #[serde(default)]
    pub last_run_dir: Option<String>,
    pub recent_runs: Vec<RecentRunRecord>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct VerifyStationResponse {
    pub station_path: String,
    pub station_id: String,
    pub output_dir: String,
    pub snapshot_path: String,
    pub devices_failed: usize,
    pub required_failures: usize,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct HardwareSnapshotResponse {
    pub station_path: String,
    pub output_dir: String,
    pub snapshot_path: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Default)]
pub struct SmbDefaultSweepOverride {
    #[serde(default)]
    pub start_hz: Option<f64>,
    #[serde(default)]
    pub stop_hz: Option<f64>,
    #[serde(default)]
    pub step_hz: Option<f64>,
    #[serde(default)]
    pub dwell_ms: Option<u64>,
    #[serde(default)]
    pub power_dbm: Option<f64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Default)]
pub struct LaserOverride {
    #[serde(default)]
    pub mode: Option<LaserBackgroundMode>,
    #[serde(default)]
    pub power_mw: Option<u16>,
    #[serde(default)]
    pub settle_ms: Option<u64>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunLaunchRequest {
    pub station_path: String,
    pub calibration_path: String,
    pub smb_profile_path: String,
    pub oe_profile_path: String,
    pub laser_profile_path: String,
    #[serde(default)]
    pub artifact_mode: Option<String>,
    pub draft_plan: AcquisitionRunPlan,
    #[serde(default)]
    pub smb_default_sweep_override: Option<SmbDefaultSweepOverride>,
    #[serde(default)]
    pub laser_override: Option<LaserOverride>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunLaunchAccepted {
    pub run_id: String,
    pub output_dir: String,
    pub accepted: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RunStateSnapshot {
    pub run_id: String,
    pub status: String,
    pub output_dir: String,
    pub started_at: String,
    #[serde(default)]
    pub ended_at: Option<String>,
    #[serde(default)]
    pub failure: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct PointProgressSnapshot {
    pub run_id: String,
    #[serde(default)]
    pub current_point_id: Option<String>,
    pub completed_points: usize,
    pub total_points: usize,
    pub progress_ratio: f64,
    #[serde(default)]
    pub eta_seconds: Option<f64>,
    #[serde(default)]
    pub target_b_nt: Option<[f64; 3]>,
    #[serde(default)]
    pub measured_current_a: Option<[f64; 3]>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct CollectorHealthSummary {
    pub run_id: String,
    pub health: String,
    pub timeout_count: usize,
    pub duplicate_count: usize,
    pub ring_retained_frames: usize,
    pub frame_rate_hz: f64,
    #[serde(default)]
    pub last_frame_ts: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct LiveTraceWindow {
    pub field: String,
    pub sample_dt_ms: f64,
    pub x: Vec<f64>,
    pub y: Vec<f64>,
    pub window_start_ts: String,
    pub window_end_ts: String,
}

#[derive(Debug, Clone, Serialize, Deserialize, PartialEq)]
pub struct RecentRuntimeEvent {
    pub ts: String,
    pub event: String,
    pub phase: String,
    #[serde(default)]
    pub point_id: Option<String>,
    #[serde(default)]
    pub device: Option<String>,
    pub data: serde_json::Value,
}

impl From<&EventRecord> for RecentRuntimeEvent {
    fn from(value: &EventRecord) -> Self {
        Self {
            ts: value.ts.clone(),
            event: value.event.clone(),
            phase: value.phase.clone(),
            point_id: value.point_id.clone(),
            device: value.device.clone(),
            data: value.data.clone(),
        }
    }
}

#[derive(Debug, Default)]
pub struct LiveSnapshotStore {
    pub run_state: Option<RunStateSnapshot>,
    pub point_progress: Option<PointProgressSnapshot>,
    pub collector_health: Option<CollectorHealthSummary>,
    pub trace_windows: HashMap<String, LiveTraceWindow>,
    pub recent_events: VecDeque<RecentRuntimeEvent>,
}

#[derive(Debug, Clone, Serialize)]
struct LiveEnvelope<'a, T: Serialize> {
    #[serde(rename = "type")]
    event_type: &'a str,
    payload: &'a T,
}

#[derive(Clone)]
pub struct LiveHub {
    tx: broadcast::Sender<String>,
    snapshots: Arc<Mutex<LiveSnapshotStore>>,
}

impl LiveHub {
    pub fn new(capacity: usize) -> Self {
        let (tx, _) = broadcast::channel(capacity.max(16));
        Self {
            tx,
            snapshots: Arc::new(Mutex::new(LiveSnapshotStore::default())),
        }
    }

    pub fn subscribe(&self) -> broadcast::Receiver<String> {
        self.tx.subscribe()
    }

    pub fn seed_messages(&self, service_status: &ServiceStatus) -> Result<Vec<String>, String> {
        let mut out = Vec::new();
        out.push(serialize_live("service_status", service_status)?);
        let snapshots = self
            .snapshots
            .lock()
            .map_err(|err| format!("live snapshot 锁失败: {err}"))?;
        if let Some(run_state) = &snapshots.run_state {
            out.push(serialize_live("run_state", run_state)?);
        }
        if let Some(point_progress) = &snapshots.point_progress {
            out.push(serialize_live("point_progress", point_progress)?);
        }
        if let Some(collector_health) = &snapshots.collector_health {
            out.push(serialize_live("collector_health", collector_health)?);
        }
        for trace in snapshots.trace_windows.values() {
            out.push(serialize_live("trace_window", trace)?);
        }
        for event in snapshots.recent_events.iter() {
            out.push(serialize_live("recent_event", event)?);
        }
        Ok(out)
    }

    pub fn publish_service_status(&self, status: &ServiceStatus) -> Result<(), String> {
        self.publish("service_status", status)
    }

    pub fn publish_run_state(&self, run_state: RunStateSnapshot) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("run_state 锁失败: {err}"))?;
            snapshots.run_state = Some(run_state.clone());
        }
        self.publish("run_state", &run_state)
    }

    pub fn publish_point_progress(
        &self,
        point_progress: PointProgressSnapshot,
    ) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("point_progress 锁失败: {err}"))?;
            snapshots.point_progress = Some(point_progress.clone());
        }
        self.publish("point_progress", &point_progress)
    }

    pub fn publish_collector_health(
        &self,
        collector_health: CollectorHealthSummary,
    ) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("collector_health 锁失败: {err}"))?;
            snapshots.collector_health = Some(collector_health.clone());
        }
        self.publish("collector_health", &collector_health)
    }

    pub fn publish_trace_window(&self, trace_window: LiveTraceWindow) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("trace_window 锁失败: {err}"))?;
            snapshots
                .trace_windows
                .insert(trace_window.field.clone(), trace_window.clone());
        }
        self.publish("trace_window", &trace_window)
    }

    pub fn publish_recent_event(&self, event: RecentRuntimeEvent) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("recent_event 锁失败: {err}"))?;
            snapshots.recent_events.push_back(event.clone());
            while snapshots.recent_events.len() > RECENT_EVENT_LIMIT {
                snapshots.recent_events.pop_front();
            }
        }
        self.publish("recent_event", &event)
    }

    pub fn publish_run_finished(&self, run_state: RunStateSnapshot) -> Result<(), String> {
        {
            let mut snapshots = self
                .snapshots
                .lock()
                .map_err(|err| format!("run_finished 锁失败: {err}"))?;
            snapshots.run_state = Some(run_state.clone());
        }
        self.publish("run_finished", &run_state)
    }

    fn publish<T: Serialize>(&self, event_type: &str, payload: &T) -> Result<(), String> {
        let serialized = serialize_live(event_type, payload)?;
        let _ = self.tx.send(serialized);
        Ok(())
    }
}

fn serialize_live<T: Serialize>(event_type: &str, payload: &T) -> Result<String, String> {
    serde_json::to_string(&LiveEnvelope {
        event_type,
        payload,
    })
    .map_err(|err| format!("live 事件序列化失败 `{event_type}`: {err}"))
}
