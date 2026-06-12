use crate::hardware_state_snapshot::run_hardware_state_snapshot;
use crate::live_bridge::{
    CollectorHealthSummary, HardwareSnapshotResponse, LaserOverride, LiveHub, LiveTraceWindow,
    PointProgressSnapshot, RecentRunRecord, RecentRuntimeEvent, RunLaunchAccepted,
    RunLaunchRequest, RunStateSnapshot, ServiceStatus, SmbDefaultSweepOverride,
    VerifyStationResponse,
};
use crate::run_execute::{request_run_interrupt, run_execute, RunArtifactMode};
use acquisition_runtime::{
    parse_rall_frame_minimal, CollectorConfig, EventRecord, FrameIndexRecord, LaserRunProfile,
    Oe1022dRunProfile, Smb100aRunProfile, SummaryRecord, RALL_SAMPLE_DT_MS,
};
use axum::extract::ws::{Message, WebSocket};
use axum::extract::{State, WebSocketUpgrade};
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use axum::routing::{get, post};
use axum::{Json, Router};
use chrono::{DateTime, Local, TimeZone, Utc};
use futures_util::StreamExt;
use serde::de::DeserializeOwned;
use serde::{Deserialize, Serialize};
use station_resolver::resolve_station;
use std::collections::{HashMap, VecDeque};
use std::fs::{self, File};
use std::io::{BufRead, BufReader, Read, Seek, SeekFrom, Write};
use std::net::SocketAddr;
use std::path::{Path, PathBuf};
use std::sync::{
    atomic::{AtomicBool, Ordering},
    Arc, Mutex,
};
use std::thread;
use std::time::{Duration, Instant, SystemTime, UNIX_EPOCH};
use tokio::net::TcpListener;

const RECENT_RUN_LIMIT: usize = 20;
const LIVE_TICK_MS: u64 = 100;
const LIVE_WINDOW_MS: f64 = 8_000.0;
const FRAME_RATE_WINDOW_MS: f64 = 2_000.0;
const TRACE_FIELDS: [(&str, usize); 4] = [("B-X", 8), ("B-Y", 9), ("B-Freq", 10), ("B-Noise", 11)];

#[derive(Debug)]
struct BridgeServiceState {
    active_run: Option<ActiveRunState>,
    last_run_dir: Option<PathBuf>,
}

#[derive(Debug, Clone)]
struct ActiveRunState {
    run_id: String,
    output_dir: PathBuf,
    finished_flag: Arc<AtomicBool>,
}

#[derive(Clone)]
struct BridgeState {
    workspace_root: PathBuf,
    config_root: PathBuf,
    runs_root: PathBuf,
    out_root: PathBuf,
    live: LiveHub,
    service: Arc<Mutex<BridgeServiceState>>,
}

#[derive(Debug, Deserialize)]
struct PathRequest {
    station_path: String,
}

#[derive(Debug, Serialize)]
struct StopRunResponse {
    stopped: bool,
    #[serde(default)]
    run_id: Option<String>,
}

#[derive(Debug, Serialize)]
struct ApiError {
    error: String,
}

#[derive(Debug, Clone)]
struct LaunchBundle {
    run_id: String,
    output_dir: PathBuf,
    plan_path: PathBuf,
    smb_profile_path: PathBuf,
    oe_profile_path: PathBuf,
    laser_profile_path: PathBuf,
    station_path: PathBuf,
    calibration_path: PathBuf,
    artifact_mode: RunArtifactMode,
    total_points: usize,
    collector: CollectorConfig,
}

pub fn run_gui_bridge_serve(bind: &str) -> Result<(), String> {
    let workspace_root =
        std::env::current_dir().map_err(|err| format!("无法获取 workspace_root: {err}"))?;
    let state = BridgeState {
        config_root: workspace_root.join("configs"),
        runs_root: workspace_root.join("runs"),
        out_root: workspace_root.join("out").join("gui_bridge"),
        workspace_root,
        live: LiveHub::new(256),
        service: Arc::new(Mutex::new(BridgeServiceState {
            active_run: None,
            last_run_dir: None,
        })),
    };

    let addr: SocketAddr = bind
        .parse()
        .map_err(|err| format!("--bind 地址解析失败 `{bind}`: {err}"))?;
    if !addr.ip().is_loopback() {
        return Err("gui-bridge 只允许监听 localhost/127.0.0.1".to_string());
    }

    fs::create_dir_all(&state.out_root).map_err(|err| {
        format!(
            "无法创建 gui-bridge 输出目录 {}: {err}",
            state.out_root.display()
        )
    })?;
    fs::create_dir_all(&state.runs_root)
        .map_err(|err| format!("无法创建 runs 目录 {}: {err}", state.runs_root.display()))?;

    let runtime = tokio::runtime::Builder::new_multi_thread()
        .enable_all()
        .build()
        .map_err(|err| format!("tokio runtime 创建失败: {err}"))?;

    runtime.block_on(async move {
        let shared = Arc::new(state);
        let app = Router::new()
            .route("/v1/status", get(handle_status))
            .route("/v1/verify-station", post(handle_verify_station))
            .route("/v1/hardware-snapshot", post(handle_hardware_snapshot))
            .route("/v1/run/start", post(handle_run_start))
            .route("/v1/run/stop", post(handle_run_stop))
            .route("/v1/runs/recent", get(handle_recent_runs))
            .route("/v1/live", get(handle_live_ws))
            .with_state(shared);
        let listener = TcpListener::bind(addr)
            .await
            .map_err(|err| format!("gui-bridge 绑定失败 {bind}: {err}"))?;
        println!("gui-bridge 已启动: http://{bind}");
        axum::serve(listener, app)
            .await
            .map_err(|err| format!("gui-bridge 服务异常退出: {err}"))
    })
}

async fn handle_status(
    State(state): State<Arc<BridgeState>>,
) -> Result<Json<ServiceStatus>, ApiResponse> {
    Ok(Json(state.service_status().map_err(ApiResponse::internal)?))
}

async fn handle_recent_runs(
    State(state): State<Arc<BridgeState>>,
) -> Result<Json<Vec<RecentRunRecord>>, ApiResponse> {
    Ok(Json(
        scan_recent_runs(&state.runs_root, RECENT_RUN_LIMIT).map_err(ApiResponse::internal)?,
    ))
}

async fn handle_verify_station(
    State(state): State<Arc<BridgeState>>,
    Json(request): Json<PathRequest>,
) -> Result<Json<VerifyStationResponse>, ApiResponse> {
    let station_path = PathBuf::from(&request.station_path);
    let spec = crate::read_station_spec(&station_path).map_err(ApiResponse::bad_request)?;
    let resolved = resolve_station(&spec);
    let target_dir = state.out_root.join("verify_station").join(timestamp_slug());
    fs::create_dir_all(&target_dir).map_err(ApiResponse::internal_from_io)?;
    let snapshot_path = target_dir.join("station_snapshot.json");
    write_pretty_json(&snapshot_path, &resolved.snapshot).map_err(ApiResponse::internal)?;

    Ok(Json(VerifyStationResponse {
        station_path: station_path.display().to_string(),
        station_id: spec.station_id,
        output_dir: target_dir.display().to_string(),
        snapshot_path: snapshot_path.display().to_string(),
        devices_failed: resolved.snapshot.devices_failed,
        required_failures: resolved.snapshot.required_failures,
    }))
}

async fn handle_hardware_snapshot(
    State(state): State<Arc<BridgeState>>,
    Json(request): Json<PathRequest>,
) -> Result<Json<HardwareSnapshotResponse>, ApiResponse> {
    if state.has_active_run().map_err(ApiResponse::internal)? {
        return Err(ApiResponse::conflict(
            "active run 期间禁止 hardware-snapshot".to_string(),
        ));
    }

    let station_path = PathBuf::from(&request.station_path);
    let target_dir = state
        .out_root
        .join("hardware_snapshot")
        .join(timestamp_slug());
    run_hardware_state_snapshot(&station_path, Some(&target_dir)).map_err(ApiResponse::internal)?;
    let snapshot_path = target_dir.join("hardware_state_snapshot.json");
    Ok(Json(HardwareSnapshotResponse {
        station_path: station_path.display().to_string(),
        output_dir: target_dir.display().to_string(),
        snapshot_path: snapshot_path.display().to_string(),
    }))
}

async fn handle_run_start(
    State(state): State<Arc<BridgeState>>,
    Json(request): Json<RunLaunchRequest>,
) -> Result<Json<RunLaunchAccepted>, ApiResponse> {
    if state.has_active_run().map_err(ApiResponse::internal)? {
        return Err(ApiResponse::conflict(
            "已有 active run，当前服务只允许单 run".to_string(),
        ));
    }

    let launch = build_launch_bundle(&state, request).map_err(ApiResponse::bad_request)?;
    let finished_flag = Arc::new(AtomicBool::new(false));

    {
        let mut guard = state
            .service
            .lock()
            .map_err(|err| ApiResponse::internal(format!("service 状态锁失败: {err}")))?;
        guard.active_run = Some(ActiveRunState {
            run_id: launch.run_id.clone(),
            output_dir: launch.output_dir.clone(),
            finished_flag: Arc::clone(&finished_flag),
        });
    }

    let started_at = now_ts_string();
    state
        .live
        .publish_run_state(RunStateSnapshot {
            run_id: launch.run_id.clone(),
            status: "running".to_string(),
            output_dir: launch.output_dir.display().to_string(),
            started_at: started_at.clone(),
            ended_at: None,
            failure: None,
        })
        .map_err(ApiResponse::internal)?;
    state
        .live
        .publish_point_progress(PointProgressSnapshot {
            run_id: launch.run_id.clone(),
            current_point_id: None,
            completed_points: 0,
            total_points: launch.total_points,
            progress_ratio: 0.0,
            eta_seconds: None,
            target_b_nt: None,
            measured_current_a: None,
        })
        .map_err(ApiResponse::internal)?;
    state
        .live
        .publish_service_status(&state.service_status().map_err(ApiResponse::internal)?)
        .map_err(ApiResponse::internal)?;

    spawn_live_reducer(
        state.live.clone(),
        launch.run_id.clone(),
        launch.output_dir.clone(),
        launch.collector.clone(),
        launch.total_points,
        Arc::clone(&finished_flag),
    );
    spawn_run_thread(Arc::clone(&state), launch, started_at, finished_flag);

    Ok(Json(RunLaunchAccepted {
        run_id: state
            .active_run_id()
            .map_err(ApiResponse::internal)?
            .unwrap_or_default(),
        output_dir: state
            .last_active_output_dir()
            .map_err(ApiResponse::internal)?
            .unwrap_or_default(),
        accepted: true,
    }))
}

async fn handle_run_stop(
    State(state): State<Arc<BridgeState>>,
) -> Result<Json<StopRunResponse>, ApiResponse> {
    let active = state.active_run_snapshot().map_err(ApiResponse::internal)?;
    let Some(active) = active else {
        return Ok(Json(StopRunResponse {
            stopped: false,
            run_id: None,
        }));
    };

    request_run_interrupt().map_err(ApiResponse::internal)?;
    loop {
        if active.finished_flag.load(Ordering::SeqCst) {
            break;
        }
        tokio::time::sleep(Duration::from_millis(100)).await;
    }

    Ok(Json(StopRunResponse {
        stopped: true,
        run_id: Some(active.run_id),
    }))
}

async fn handle_live_ws(
    ws: WebSocketUpgrade,
    State(state): State<Arc<BridgeState>>,
) -> impl IntoResponse {
    ws.on_upgrade(move |socket| handle_live_socket(socket, state))
}

async fn handle_live_socket(mut socket: WebSocket, state: Arc<BridgeState>) {
    if let Ok(messages) = state
        .live
        .seed_messages(&state.service_status().unwrap_or_else(|_| ServiceStatus {
            service_state: "disconnected".to_string(),
            active_run_id: None,
            workspace_root: state.workspace_root.display().to_string(),
            config_root: state.config_root.display().to_string(),
            last_run_dir: None,
            recent_runs: Vec::new(),
        }))
    {
        for message in messages {
            if socket.send(Message::Text(message)).await.is_err() {
                return;
            }
        }
    }

    let mut receiver = state.live.subscribe();
    loop {
        tokio::select! {
            maybe_out = receiver.recv() => {
                match maybe_out {
                    Ok(message) => {
                        if socket.send(Message::Text(message)).await.is_err() {
                            return;
                        }
                    }
                    Err(_) => return,
                }
            }
            maybe_in = socket.next() => {
                match maybe_in {
                    Some(Ok(Message::Close(_))) | None => return,
                    Some(Ok(_)) => {}
                    Some(Err(_)) => return,
                }
            }
        }
    }
}

impl BridgeState {
    fn service_status(&self) -> Result<ServiceStatus, String> {
        let guard = self
            .service
            .lock()
            .map_err(|err| format!("service 状态锁失败: {err}"))?;
        Ok(ServiceStatus {
            service_state: if guard.active_run.is_some() {
                "active_run".to_string()
            } else {
                "idle".to_string()
            },
            active_run_id: guard.active_run.as_ref().map(|run| run.run_id.clone()),
            workspace_root: self.workspace_root.display().to_string(),
            config_root: self.config_root.display().to_string(),
            last_run_dir: guard
                .last_run_dir
                .as_ref()
                .map(|path| path.display().to_string()),
            recent_runs: scan_recent_runs(&self.runs_root, RECENT_RUN_LIMIT)?,
        })
    }

    fn has_active_run(&self) -> Result<bool, String> {
        Ok(self
            .service
            .lock()
            .map_err(|err| format!("service 状态锁失败: {err}"))?
            .active_run
            .is_some())
    }

    fn active_run_snapshot(&self) -> Result<Option<ActiveRunState>, String> {
        Ok(self
            .service
            .lock()
            .map_err(|err| format!("service 状态锁失败: {err}"))?
            .active_run
            .clone())
    }

    fn active_run_id(&self) -> Result<Option<String>, String> {
        Ok(self
            .service
            .lock()
            .map_err(|err| format!("service 状态锁失败: {err}"))?
            .active_run
            .as_ref()
            .map(|run| run.run_id.clone()))
    }

    fn last_active_output_dir(&self) -> Result<Option<String>, String> {
        Ok(self
            .service
            .lock()
            .map_err(|err| format!("service 状态锁失败: {err}"))?
            .active_run
            .as_ref()
            .map(|run| run.output_dir.display().to_string()))
    }
}

fn build_launch_bundle(
    state: &BridgeState,
    request: RunLaunchRequest,
) -> Result<LaunchBundle, String> {
    let station_path = normalize_path(&state.workspace_root, &request.station_path);
    let calibration_path = normalize_path(&state.workspace_root, &request.calibration_path);
    let smb_profile_path = normalize_path(&state.workspace_root, &request.smb_profile_path);
    let oe_profile_path = normalize_path(&state.workspace_root, &request.oe_profile_path);
    let laser_profile_path = normalize_path(&state.workspace_root, &request.laser_profile_path);

    let mut smb_profile: Smb100aRunProfile = read_json_file(&smb_profile_path)?;
    apply_smb_override(
        &mut smb_profile,
        request.smb_default_sweep_override.as_ref(),
    );
    let mut laser_profile: LaserRunProfile = read_json_file(&laser_profile_path)?;
    apply_laser_override(&mut laser_profile, request.laser_override.as_ref());
    let oe_profile: Oe1022dRunProfile = read_json_file(&oe_profile_path)?;
    let resolved_plan = request
        .draft_plan
        .resolve_points(&smb_profile)
        .map_err(|err| format!("draft_plan 展开失败: {err}"))?;

    let artifact_mode = match request.artifact_mode.as_deref().unwrap_or("lightweight") {
        "lightweight" => RunArtifactMode::Lightweight,
        "debug" => RunArtifactMode::Debug,
        other => {
            return Err(format!(
                "artifact_mode 仅支持 lightweight 或 debug，当前为 {other}"
            ))
        }
    };

    let run_id = request.draft_plan.run_id.trim().to_string();
    if run_id.is_empty() {
        return Err("draft_plan.run_id 不能为空".to_string());
    }

    let launch_dir = state.out_root.join("launches").join(format!(
        "{}_{}",
        timestamp_slug(),
        sanitize_slug(&run_id)
    ));
    fs::create_dir_all(&launch_dir)
        .map_err(|err| format!("无法创建 launch 目录 {}: {err}", launch_dir.display()))?;

    let plan_path = launch_dir.join("plan.json");
    let merged_smb_path = launch_dir.join("smb_profile.json");
    let merged_laser_path = launch_dir.join("laser_profile.json");
    write_pretty_json(&plan_path, &request.draft_plan)?;
    write_pretty_json(&merged_smb_path, &smb_profile)?;
    write_pretty_json(&merged_laser_path, &laser_profile)?;
    let request_audit_path = launch_dir.join("run_launch_request.json");
    write_pretty_json(&request_audit_path, &request)?;

    let output_dir = unique_run_output_dir(&state.runs_root, &run_id);
    Ok(LaunchBundle {
        run_id,
        output_dir,
        plan_path,
        smb_profile_path: merged_smb_path,
        oe_profile_path,
        laser_profile_path: merged_laser_path,
        station_path,
        calibration_path,
        artifact_mode,
        total_points: resolved_plan.points.len(),
        collector: oe_profile.collector,
    })
}

fn spawn_run_thread(
    state: Arc<BridgeState>,
    launch: LaunchBundle,
    started_at: String,
    finished_flag: Arc<AtomicBool>,
) {
    thread::spawn(move || {
        let output_dir = launch.output_dir.clone();
        let run_id = launch.run_id.clone();
        let result = run_execute(
            &launch.station_path,
            &launch.calibration_path,
            &launch.plan_path,
            &launch.smb_profile_path,
            false,
            &launch.oe_profile_path,
            Some(&launch.laser_profile_path),
            false,
            Some(&output_dir),
            launch.artifact_mode,
        );
        finished_flag.store(true, Ordering::SeqCst);

        let summary = read_json_file::<SummaryRecord>(&output_dir.join("summary.json")).ok();
        let run_state = summary
            .map(|summary| RunStateSnapshot {
                run_id: summary.run_id,
                status: summary.status,
                output_dir: output_dir.display().to_string(),
                started_at: summary.started_at,
                ended_at: Some(summary.ended_at),
                failure: summary.failure,
            })
            .unwrap_or_else(|| RunStateSnapshot {
                run_id: run_id.clone(),
                status: if result.is_ok() {
                    "completed".to_string()
                } else {
                    "failed".to_string()
                },
                output_dir: output_dir.display().to_string(),
                started_at,
                ended_at: Some(now_ts_string()),
                failure: result.err(),
            });

        if let Ok(mut guard) = state.service.lock() {
            guard.last_run_dir = Some(output_dir.clone());
            if guard
                .active_run
                .as_ref()
                .map(|active| active.run_id.as_str())
                == Some(run_id.as_str())
            {
                guard.active_run = None;
            }
        }

        let _ = state.live.publish_run_finished(run_state);
        if let Ok(service_status) = state.service_status() {
            let _ = state.live.publish_service_status(&service_status);
        }
    });
}

fn spawn_live_reducer(
    live: LiveHub,
    run_id: String,
    output_dir: PathBuf,
    collector: CollectorConfig,
    total_points: usize,
    finished_flag: Arc<AtomicBool>,
) {
    thread::spawn(move || {
        let mut reducer = LiveReducer::new(live, run_id, output_dir, collector, total_points);
        reducer.run(finished_flag);
    });
}

struct LiveReducer {
    live: LiveHub,
    run_id: String,
    output_dir: PathBuf,
    sample_dt_ms: f64,
    ring_capacity_frames: usize,
    total_points: usize,
    started_at: Instant,
    completed_points: usize,
    current_point_id: Option<String>,
    current_target_b_nt: Option<[f64; 3]>,
    measured_current_a: Option<[f64; 3]>,
    timeout_count: usize,
    duplicate_count: usize,
    current_health: String,
    total_frames_seen: usize,
    last_frame_ts: Option<String>,
    frame_marks_ms: VecDeque<f64>,
    traces: HashMap<String, VecDeque<(f64, f64)>>,
}

impl LiveReducer {
    fn new(
        live: LiveHub,
        run_id: String,
        output_dir: PathBuf,
        collector: CollectorConfig,
        total_points: usize,
    ) -> Self {
        let mut traces = HashMap::new();
        for (field, _) in TRACE_FIELDS {
            traces.insert(field.to_string(), VecDeque::new());
        }
        Self {
            live,
            run_id,
            output_dir,
            sample_dt_ms: RALL_SAMPLE_DT_MS,
            ring_capacity_frames: collector.ring_capacity_frames,
            total_points,
            started_at: Instant::now(),
            completed_points: 0,
            current_point_id: None,
            current_target_b_nt: None,
            measured_current_a: None,
            timeout_count: 0,
            duplicate_count: 0,
            current_health: "clean".to_string(),
            total_frames_seen: 0,
            last_frame_ts: None,
            frame_marks_ms: VecDeque::new(),
            traces,
        }
    }

    fn run(&mut self, finished_flag: Arc<AtomicBool>) {
        let mut event_tailer = JsonlTail::new(self.output_dir.join("events.jsonl"));
        let mut index_tailer =
            JsonlTail::new(self.output_dir.join("raw").join("oe1022d.frames.idx.jsonl"));
        let raw_path = self.output_dir.join("raw").join("oe1022d.rall");
        let mut idle_ticks_after_finish = 0_usize;

        loop {
            let mut progressed = false;

            if let Ok(lines) = event_tailer.read_new_lines() {
                for line in lines {
                    progressed = true;
                    if let Ok(event) = serde_json::from_str::<EventRecord>(&line) {
                        self.handle_event(event);
                    }
                }
            }

            if let Ok(lines) = index_tailer.read_new_lines() {
                for line in lines {
                    progressed = true;
                    if let Ok(index) = serde_json::from_str::<FrameIndexRecord>(&line) {
                        let _ = self.handle_index_record(&raw_path, index);
                    }
                }
            }

            self.publish_health();
            self.publish_traces();

            if finished_flag.load(Ordering::SeqCst) {
                if progressed {
                    idle_ticks_after_finish = 0;
                } else {
                    idle_ticks_after_finish += 1;
                    if idle_ticks_after_finish >= 5 {
                        return;
                    }
                }
            }

            thread::sleep(Duration::from_millis(LIVE_TICK_MS));
        }
    }

    fn handle_event(&mut self, event: EventRecord) {
        let _ = self
            .live
            .publish_recent_event(RecentRuntimeEvent::from(&event));
        match event.event.as_str() {
            "point_prepare_started" => {
                self.current_point_id = event.point_id.clone();
                self.current_target_b_nt = extract_triplet(&event.data, "target_b_nt");
                self.publish_point_progress();
            }
            "point_stable" => {
                self.measured_current_a = extract_triplet(&event.data, "measured_current_a");
                self.publish_point_progress();
            }
            "point_completed" => {
                self.completed_points = self.completed_points.saturating_add(1);
                self.publish_point_progress();
            }
            "collector_timeout" => {
                self.timeout_count = self.timeout_count.saturating_add(1);
                self.current_health = "degraded_timeout".to_string();
            }
            "collector_recovered" => {
                self.current_health = "recovered_timeout".to_string();
            }
            _ => {}
        }
    }

    fn handle_index_record(
        &mut self,
        raw_path: &Path,
        index: FrameIndexRecord,
    ) -> Result<(), String> {
        let payload = read_frame_payload(raw_path, index.raw_offset, index.raw_len)?;
        let parsed = parse_rall_frame_minimal(&payload)
            .map_err(|err| format!("RALL minimal 解析失败: {err}"))?;
        let frame_end_ms = parse_ts_millis(&index.ts).unwrap_or_else(now_epoch_millis_f64);
        self.total_frames_seen = self.total_frames_seen.saturating_add(1);
        if index.duplicate_of.is_some() {
            self.duplicate_count = self.duplicate_count.saturating_add(1);
        }
        self.last_frame_ts = Some(index.ts.clone());
        self.frame_marks_ms.push_back(frame_end_ms);
        while let Some(front) = self.frame_marks_ms.front().copied() {
            if frame_end_ms - front > FRAME_RATE_WINDOW_MS {
                self.frame_marks_ms.pop_front();
            } else {
                break;
            }
        }

        for (field_name, field_index) in TRACE_FIELDS {
            let Some(series) = parsed.measurement_matrix.get(field_index) else {
                continue;
            };
            let start_ms =
                frame_end_ms - self.sample_dt_ms * (series.len().saturating_sub(1) as f64);
            let buffer = self.traces.entry(field_name.to_string()).or_default();
            for (sample_index, value) in series.iter().enumerate() {
                buffer.push_back((start_ms + self.sample_dt_ms * sample_index as f64, *value));
            }
            while let Some((ts_ms, _)) = buffer.front().copied() {
                if frame_end_ms - ts_ms > LIVE_WINDOW_MS {
                    buffer.pop_front();
                } else {
                    break;
                }
            }
        }

        Ok(())
    }

    fn publish_point_progress(&self) {
        let progress_ratio = if self.total_points == 0 {
            0.0
        } else {
            self.completed_points as f64 / self.total_points as f64
        };
        let eta_seconds = if self.completed_points == 0 {
            None
        } else {
            let elapsed = self.started_at.elapsed().as_secs_f64();
            let mean_per_point = elapsed / self.completed_points as f64;
            Some(mean_per_point * self.total_points.saturating_sub(self.completed_points) as f64)
        };
        let _ = self.live.publish_point_progress(PointProgressSnapshot {
            run_id: self.run_id.clone(),
            current_point_id: self.current_point_id.clone(),
            completed_points: self.completed_points,
            total_points: self.total_points,
            progress_ratio,
            eta_seconds,
            target_b_nt: self.current_target_b_nt,
            measured_current_a: self.measured_current_a,
        });
    }

    fn publish_health(&self) {
        let now_ms = now_epoch_millis_f64();
        let frame_window_span_s = if let (Some(first), Some(last)) = (
            self.frame_marks_ms.front().copied(),
            self.frame_marks_ms.back().copied(),
        ) {
            ((last - first).max(1.0)) / 1_000.0
        } else {
            1.0
        };
        let frame_rate_hz = if self.frame_marks_ms.is_empty() {
            0.0
        } else {
            self.frame_marks_ms.len() as f64 / frame_window_span_s
        };
        let _ = self.live.publish_collector_health(CollectorHealthSummary {
            run_id: self.run_id.clone(),
            health: self.current_health.clone(),
            timeout_count: self.timeout_count,
            duplicate_count: self.duplicate_count,
            ring_retained_frames: self.total_frames_seen.min(self.ring_capacity_frames),
            frame_rate_hz,
            last_frame_ts: self
                .last_frame_ts
                .clone()
                .or_else(|| Some(ts_from_epoch_ms(now_ms))),
        });
    }

    fn publish_traces(&self) {
        for (field_name, _) in TRACE_FIELDS {
            let Some(buffer) = self.traces.get(field_name) else {
                continue;
            };
            let x = buffer.iter().map(|(ts_ms, _)| *ts_ms).collect::<Vec<_>>();
            let y = buffer.iter().map(|(_, value)| *value).collect::<Vec<_>>();
            let window_start_ts = x.first().copied().map(ts_from_epoch_ms).unwrap_or_default();
            let window_end_ts = x.last().copied().map(ts_from_epoch_ms).unwrap_or_default();
            let _ = self.live.publish_trace_window(LiveTraceWindow {
                field: field_name.to_string(),
                sample_dt_ms: self.sample_dt_ms,
                x,
                y,
                window_start_ts,
                window_end_ts,
            });
        }
    }
}

struct JsonlTail {
    path: PathBuf,
    offset: u64,
}

impl JsonlTail {
    fn new(path: PathBuf) -> Self {
        Self { path, offset: 0 }
    }

    fn read_new_lines(&mut self) -> Result<Vec<String>, String> {
        if !self.path.exists() {
            return Ok(Vec::new());
        }
        let mut file = File::open(&self.path)
            .map_err(|err| format!("无法打开 {}: {err}", self.path.display()))?;
        file.seek(SeekFrom::Start(self.offset))
            .map_err(|err| format!("无法 seek {}: {err}", self.path.display()))?;
        let mut reader = BufReader::new(file);
        let mut out = Vec::new();

        loop {
            let mut line = String::new();
            let read = reader
                .read_line(&mut line)
                .map_err(|err| format!("读取 {} 失败: {err}", self.path.display()))?;
            if read == 0 {
                break;
            }
            if !line.ends_with('\n') {
                break;
            }
            self.offset += read as u64;
            out.push(line.trim_end().to_string());
        }
        Ok(out)
    }
}

fn apply_smb_override(
    profile: &mut Smb100aRunProfile,
    override_spec: Option<&SmbDefaultSweepOverride>,
) {
    let Some(override_spec) = override_spec else {
        return;
    };
    profile.default_sweep.start_hz = override_spec
        .start_hz
        .unwrap_or(profile.default_sweep.start_hz);
    profile.default_sweep.stop_hz = override_spec
        .stop_hz
        .unwrap_or(profile.default_sweep.stop_hz);
    profile.default_sweep.step_hz = override_spec
        .step_hz
        .unwrap_or(profile.default_sweep.step_hz);
    profile.default_sweep.dwell_ms = override_spec
        .dwell_ms
        .unwrap_or(profile.default_sweep.dwell_ms);
    profile.default_sweep.power_dbm = override_spec
        .power_dbm
        .unwrap_or(profile.default_sweep.power_dbm);
}

fn apply_laser_override(profile: &mut LaserRunProfile, override_spec: Option<&LaserOverride>) {
    let Some(override_spec) = override_spec else {
        return;
    };
    if let Some(mode) = &override_spec.mode {
        profile.mode = mode.clone();
    }
    profile.power_mw = override_spec.power_mw.unwrap_or(profile.power_mw);
    profile.settle_ms = override_spec.settle_ms.unwrap_or(profile.settle_ms);
}

fn scan_recent_runs(runs_root: &Path, limit: usize) -> Result<Vec<RecentRunRecord>, String> {
    let mut out = Vec::new();
    if !runs_root.exists() {
        return Ok(out);
    }

    for entry in fs::read_dir(runs_root)
        .map_err(|err| format!("无法读取 runs 目录 {}: {err}", runs_root.display()))?
    {
        let entry = entry.map_err(|err| format!("读取 runs 目录项失败: {err}"))?;
        let path = entry.path();
        let summary_path = path.join("summary.json");
        if !summary_path.is_file() {
            continue;
        }
        let summary: SummaryRecord = match read_json_file(&summary_path) {
            Ok(summary) => summary,
            Err(_) => continue,
        };
        out.push(RecentRunRecord {
            run_id: summary.run_id,
            status: summary.status,
            started_at: summary.started_at,
            ended_at: summary.ended_at,
            output_dir: path.display().to_string(),
        });
    }

    out.sort_by(|left, right| right.started_at.cmp(&left.started_at));
    out.truncate(limit);
    Ok(out)
}

fn normalize_path(workspace_root: &Path, raw: &str) -> PathBuf {
    let path = PathBuf::from(raw);
    if path.is_absolute() {
        path
    } else {
        workspace_root.join(path)
    }
}

fn unique_run_output_dir(runs_root: &Path, run_id: &str) -> PathBuf {
    let base = format!("{}_{}", sanitize_slug(run_id), timestamp_slug());
    for attempt in 0..1000_usize {
        let candidate = if attempt == 0 {
            runs_root.join(&base)
        } else {
            runs_root.join(format!("{base}_r{attempt}"))
        };
        if !candidate.exists() {
            return candidate;
        }
    }
    runs_root.join(format!("{}_{}", sanitize_slug(run_id), now_unix_ms()))
}

fn sanitize_slug(value: &str) -> String {
    value
        .chars()
        .map(|ch| {
            if ch.is_ascii_alphanumeric() || ch == '-' || ch == '_' {
                ch
            } else {
                '_'
            }
        })
        .collect()
}

fn extract_triplet(value: &serde_json::Value, key: &str) -> Option<[f64; 3]> {
    serde_json::from_value::<[f64; 3]>(value.get(key)?.clone()).ok()
}

fn read_frame_payload(path: &Path, raw_offset: u64, raw_len: usize) -> Result<Vec<u8>, String> {
    let mut file =
        File::open(path).map_err(|err| format!("无法打开 raw 文件 {}: {err}", path.display()))?;
    file.seek(SeekFrom::Start(raw_offset))
        .map_err(|err| format!("无法 seek raw 文件 {}: {err}", path.display()))?;
    let mut payload = vec![0_u8; raw_len];
    file.read_exact(&mut payload)
        .map_err(|err| format!("读取 raw frame 失败 {}: {err}", path.display()))?;
    Ok(payload)
}

fn parse_ts_millis(ts: &str) -> Option<f64> {
    DateTime::parse_from_rfc3339(ts)
        .ok()
        .map(|dt| dt.timestamp_millis() as f64)
}

fn ts_from_epoch_ms(epoch_ms: f64) -> String {
    let millis = epoch_ms.round() as i64;
    Utc.timestamp_millis_opt(millis)
        .single()
        .map(|dt| dt.to_rfc3339())
        .unwrap_or_default()
}

fn write_pretty_json<T: Serialize>(path: &Path, value: &T) -> Result<(), String> {
    if let Some(parent) = path.parent() {
        fs::create_dir_all(parent)
            .map_err(|err| format!("无法创建目录 {}: {err}", parent.display()))?;
    }
    let text = serde_json::to_string_pretty(value)
        .map_err(|err| format!("JSON 序列化失败 {}: {err}", path.display()))?;
    let mut file =
        File::create(path).map_err(|err| format!("无法创建文件 {}: {err}", path.display()))?;
    file.write_all(text.as_bytes())
        .map_err(|err| format!("无法写入文件 {}: {err}", path.display()))
}

fn read_json_file<T: DeserializeOwned>(path: &Path) -> Result<T, String> {
    let text =
        fs::read_to_string(path).map_err(|err| format!("无法读取 {}: {err}", path.display()))?;
    serde_json::from_str(&text).map_err(|err| format!("JSON 解析失败 {}: {err}", path.display()))
}

fn timestamp_slug() -> String {
    Local::now().format("%Y%m%d_%H%M%S").to_string()
}

fn now_ts_string() -> String {
    Local::now().to_rfc3339()
}

fn now_unix_ms() -> u128 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|duration| duration.as_millis())
        .unwrap_or_default()
}

fn now_epoch_millis_f64() -> f64 {
    now_unix_ms() as f64
}

struct ApiResponse {
    status: StatusCode,
    error: String,
}

impl ApiResponse {
    fn bad_request(error: String) -> Self {
        Self {
            status: StatusCode::BAD_REQUEST,
            error,
        }
    }

    fn conflict(error: String) -> Self {
        Self {
            status: StatusCode::CONFLICT,
            error,
        }
    }

    fn internal(error: String) -> Self {
        Self {
            status: StatusCode::INTERNAL_SERVER_ERROR,
            error,
        }
    }

    fn internal_from_io(err: std::io::Error) -> Self {
        Self::internal(err.to_string())
    }
}

impl IntoResponse for ApiResponse {
    fn into_response(self) -> Response {
        (self.status, Json(ApiError { error: self.error })).into_response()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn smb_override_only_touches_allowed_fields() {
        let mut profile = Smb100aRunProfile {
            profile_id: "smb".to_string(),
            command_settle_ms: 10,
            error_check_after_write: true,
            fixed: acquisition_runtime::Smb100aFixedProfile {
                modulation_enabled: false,
                fm_enabled: false,
                fm_source: "INT".to_string(),
                fm_mode: "NORM".to_string(),
                fm_deviation_hz: 1.0,
                lf_output_enabled: false,
                lf_voltage_mv: 1.0,
                lf_frequency_hz: 2.0,
                lf_shape: "SIN".to_string(),
                lf_source_impedance: "LOW".to_string(),
            },
            default_sweep: acquisition_runtime::SmbSweepDefaults {
                start_hz: 1.0,
                stop_hz: 2.0,
                step_hz: 3.0,
                dwell_ms: 4,
                power_dbm: 5.0,
                sweep_mode: "AUTO".to_string(),
                spacing: "LIN".to_string(),
                shape: "SAWT".to_string(),
                trigger_source: "AUTO".to_string(),
                output_voltage_start_v: 6.0,
                output_voltage_stop_v: 7.0,
                rf_output_enabled: true,
            },
        };

        apply_smb_override(
            &mut profile,
            Some(&SmbDefaultSweepOverride {
                start_hz: Some(10.0),
                stop_hz: None,
                step_hz: Some(30.0),
                dwell_ms: Some(40),
                power_dbm: Some(-10.0),
            }),
        );

        assert_eq!(profile.default_sweep.start_hz, 10.0);
        assert_eq!(profile.default_sweep.stop_hz, 2.0);
        assert_eq!(profile.default_sweep.step_hz, 30.0);
        assert_eq!(profile.default_sweep.dwell_ms, 40);
        assert_eq!(profile.default_sweep.power_dbm, -10.0);
        assert_eq!(profile.default_sweep.sweep_mode, "AUTO");
    }
}
