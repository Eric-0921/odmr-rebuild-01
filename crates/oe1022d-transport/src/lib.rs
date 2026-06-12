//! OE1022D 最小串口连接层。
//!
//! 当前职责：
//! - 打开 USB CDC 串口
//! - 发送 setup / query 命令
//! - 读取 ASCII 响应
//! - 读取定长 `RALL?` payload
//!
//! 注意：
//! - 这里只提供“怎么连”和“怎么收”的能力
//! - 不在这里实现 run 级 collector

use base64::prelude::*;
use device_transport::{
    query_ascii_line, write_command, LineTransportOptions, Result, TransportError,
};
use oe1022d_commands::{oe1022d_query_reference_source, oe1022d_rall_query};
use serde_json::{json, Value as JsonValue};
use serialport::{ClearBuffer, DataBits, FlowControl, Parity, SerialPort, StopBits};
use std::io::{BufRead, BufReader, Read, Write};
use std::process::{Child, ChildStdin, ChildStdout, Command, Stdio};
use std::thread;
use std::time::Duration;

const PYVISA_WORKER: &str = include_str!("pyvisa_worker.py");

#[derive(Debug, Clone)]
pub struct Oe1022dTransportConfig {
    pub port_path: String,
    pub baud_rate: u32,
    pub backend: Oe1022dBackendKind,
    pub visa_resource: Option<String>,
    pub python_command: Option<String>,
    pub timeout: Duration,
    pub rall_post_write_delay: Duration,
    pub rall_chunk_timeout: Duration,
    pub rall_first_byte_deadline: Duration,
    pub rall_frame_deadline: Duration,
    pub response_max_bytes: usize,
    pub command_terminator: Vec<u8>,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Oe1022dBackendKind {
    SerialPort,
    VisaPy,
}

impl Oe1022dTransportConfig {
    fn line_options(&self) -> LineTransportOptions {
        LineTransportOptions {
            command_terminator: self.command_terminator.clone(),
            max_response_bytes: self.response_max_bytes,
        }
    }
}

impl Default for Oe1022dTransportConfig {
    fn default() -> Self {
        Self {
            port_path: "/dev/cu.usbmodem".to_string(),
            baud_rate: 921600,
            backend: Oe1022dBackendKind::SerialPort,
            visa_resource: None,
            python_command: None,
            // RALL? 是 50ms 更新一次的定长快照。
            // ASCII query 可以容忍更保守的 timeout。
            timeout: Duration::from_millis(300),
            rall_post_write_delay: Duration::from_millis(30),
            // RALL? 热路径不能把一次空读放大成 300ms 级黑洞。
            // 单次 chunk 只等很短时间，整帧再用单独 deadline 控制。
            rall_chunk_timeout: Duration::from_millis(5),
            // 如果连第一个字节都迟迟不来，就尽快把这一轮判空，
            // 让上层有机会在同周期内重发一次。
            rall_first_byte_deadline: Duration::from_millis(30),
            rall_frame_deadline: Duration::from_millis(120),
            response_max_bytes: 4096,
            command_terminator: b"\r".to_vec(),
        }
    }
}

pub struct Oe1022dTransport {
    backend: Oe1022dBackend,
    options: LineTransportOptions,
    query_timeout: Duration,
    rall_post_write_delay: Duration,
    rall_chunk_timeout: Duration,
    rall_first_byte_deadline: Duration,
    rall_frame_deadline: Duration,
}

enum Oe1022dBackend {
    Serial(Box<dyn SerialPort>),
    VisaPy(VisaPyBackend),
}

impl Oe1022dTransport {
    pub fn open(config: &Oe1022dTransportConfig) -> Result<Self> {
        let backend = match config.backend {
            Oe1022dBackendKind::SerialPort => {
                let port = serialport::new(&config.port_path, config.baud_rate)
                    .data_bits(DataBits::Eight)
                    .parity(Parity::None)
                    .stop_bits(StopBits::One)
                    .flow_control(FlowControl::None)
                    .timeout(config.timeout)
                    .open()?;
                Oe1022dBackend::Serial(port)
            }
            Oe1022dBackendKind::VisaPy => {
                let resource = config
                    .visa_resource
                    .as_deref()
                    .unwrap_or(config.port_path.as_str());
                Oe1022dBackend::VisaPy(VisaPyBackend::open(
                    resource,
                    config.baud_rate,
                    config.timeout,
                    config.python_command.as_deref(),
                )?)
            }
        };

        Ok(Self {
            backend,
            options: config.line_options(),
            query_timeout: config.timeout,
            rall_post_write_delay: config.rall_post_write_delay,
            rall_chunk_timeout: config.rall_chunk_timeout,
            rall_first_byte_deadline: config.rall_first_byte_deadline,
            rall_frame_deadline: config.rall_frame_deadline,
        })
    }

    pub fn list_visa_resources(python_command: Option<&str>) -> Result<Vec<String>> {
        VisaPyBackend::list_resources(python_command)
    }

    pub fn clear_input(&mut self) -> Result<()> {
        match &mut self.backend {
            Oe1022dBackend::Serial(port) => port.clear(ClearBuffer::Input)?,
            Oe1022dBackend::VisaPy(visa) => visa.clear()?,
        }
        Ok(())
    }

    pub fn clear_all(&mut self) -> Result<()> {
        match &mut self.backend {
            Oe1022dBackend::Serial(port) => port.clear(ClearBuffer::All)?,
            Oe1022dBackend::VisaPy(visa) => visa.clear()?,
        }
        Ok(())
    }

    pub fn send(&mut self, command: &str) -> Result<()> {
        match &mut self.backend {
            Oe1022dBackend::Serial(port) => {
                write_command(port, command, &self.options.command_terminator)
            }
            Oe1022dBackend::VisaPy(visa) => visa.send(command),
        }
    }

    pub fn query_text(&mut self, command: &str) -> Result<String> {
        match &mut self.backend {
            Oe1022dBackend::Serial(port) => {
                port.set_timeout(self.query_timeout)?;
                query_ascii_line(port, command, &self.options)
            }
            Oe1022dBackend::VisaPy(visa) => {
                visa.query_text(command, self.options.max_response_bytes)
            }
        }
    }

    pub fn query_idn(&mut self) -> Result<String> {
        self.query_text("*IDN?")
    }

    pub fn query_reference_source(&mut self, channel: u8) -> Result<String> {
        self.query_text(&oe1022d_query_reference_source(channel))
    }

    /// 发送一次 `RALL?` 并读取定长 payload。
    ///
    /// `expected_len` 暂时由上层显式传入，避免在 transport 层过早锁死帧长度假设。
    pub fn query_rall_frame(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        if matches!(self.backend, Oe1022dBackend::VisaPy(_)) {
            return self.query_rall_frame_labview_exact(expected_len);
        }
        self.send_rall_query()?;
        self.wait_after_rall_write();
        self.read_rall_frame_exact(expected_len)
    }

    pub fn send_rall_query(&mut self) -> Result<()> {
        self.send(oe1022d_rall_query())
    }

    pub fn read_rall_frame_exact(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        self.read_rall_frame_fast(expected_len)
    }

    pub fn query_rall_frame_labview_exact(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        let post_write_delay = self.rall_post_write_delay;
        match &mut self.backend {
            Oe1022dBackend::Serial(port) => {
                port.set_timeout(self.query_timeout)?;
                write_command(port, oe1022d_rall_query(), &self.options.command_terminator)?;
                if !post_write_delay.is_zero() {
                    thread::sleep(post_write_delay);
                }
                read_rall_frame_blocking_exact_with_reader(port, expected_len)
            }
            Oe1022dBackend::VisaPy(visa) => visa.query_rall_exact(expected_len, post_write_delay),
        }
    }

    pub fn query_rall_frame_exact_with_zero_retry(
        &mut self,
        expected_len: usize,
        max_zero_byte_retries: usize,
    ) -> Result<RallFrameReadOutcome> {
        if let Oe1022dBackend::VisaPy(visa) = &mut self.backend {
            return visa
                .query_rall_exact(expected_len, self.rall_post_write_delay)
                .map(|payload| RallFrameReadOutcome {
                    payload,
                    zero_byte_retry_count: 0,
                });
        }

        let mut zero_byte_retry_count = 0;

        loop {
            self.send_rall_query()?;
            self.wait_after_rall_write();
            match self.read_rall_frame_exact(expected_len) {
                Ok(payload) => {
                    return Ok(RallFrameReadOutcome {
                        payload,
                        zero_byte_retry_count,
                    });
                }
                Err(TransportError::Timeout { partial_len: 0, .. })
                    if zero_byte_retry_count < max_zero_byte_retries =>
                {
                    zero_byte_retry_count += 1;
                    self.clear_input()?;
                }
                Err(err) => return Err(err),
            }
        }
    }

    /// 发送一次 `RALL?`，并持续读取直到串口超时。
    ///
    /// 这条路径专供单帧 smoke 验证使用：
    /// - 只要求拿到非零 payload
    /// - 不在 transport 层强制假设固定帧长度
    pub fn query_rall_frame_until_timeout(&mut self, max_bytes: usize) -> Result<Vec<u8>> {
        if let Oe1022dBackend::VisaPy(visa) = &mut self.backend {
            let expected_len = max_bytes.min(12_288);
            return visa.query_rall_exact(expected_len, self.rall_post_write_delay);
        }

        self.set_port_timeout(self.query_timeout)?;
        self.send_rall_query()?;
        self.wait_after_rall_write();

        let mut out = Vec::with_capacity(max_bytes.min(4096));
        let mut buf = [0_u8; 1024];
        let Oe1022dBackend::Serial(port) = &mut self.backend else {
            unreachable!("VISA backend returned above");
        };

        loop {
            match port.read(&mut buf) {
                Ok(0) => break,
                Ok(read_len) => {
                    let remaining = max_bytes.saturating_sub(out.len());
                    if remaining == 0 {
                        break;
                    }
                    let copy_len = read_len.min(remaining);
                    out.extend_from_slice(&buf[..copy_len]);
                    if out.len() >= max_bytes {
                        break;
                    }
                }
                Err(err)
                    if matches!(
                        err.kind(),
                        std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
                    ) =>
                {
                    if out.is_empty() {
                        return Err(err.into());
                    }
                    break;
                }
                Err(err) if err.kind() == std::io::ErrorKind::Interrupted => continue,
                Err(err) => return Err(err.into()),
            }
        }

        Ok(out)
    }
}

impl Oe1022dTransport {
    fn set_port_timeout(&mut self, timeout: Duration) -> Result<()> {
        if let Oe1022dBackend::Serial(port) = &mut self.backend {
            port.set_timeout(timeout)?;
        }
        Ok(())
    }

    fn wait_after_rall_write(&self) {
        if !self.rall_post_write_delay.is_zero() {
            thread::sleep(self.rall_post_write_delay);
        }
    }

    fn read_rall_frame_fast(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        self.set_port_timeout(self.rall_chunk_timeout)?;
        let Oe1022dBackend::Serial(port) = &mut self.backend else {
            return Err(transport_io_error(
                "VISA backend does not support detached RALL reads",
            ));
        };
        read_rall_frame_fast_with_reader(
            port,
            expected_len,
            self.rall_first_byte_deadline,
            self.rall_frame_deadline,
        )
    }
}

struct VisaPyBackend {
    _child: Child,
    stdin: ChildStdin,
    stdout: BufReader<ChildStdout>,
    next_id: u64,
}

impl VisaPyBackend {
    fn open(
        resource: &str,
        baud_rate: u32,
        timeout: Duration,
        python_command: Option<&str>,
    ) -> Result<Self> {
        let mut backend = Self::spawn(python_command)?;
        backend.request(json!({
            "op": "open",
            "resource": resource,
            "baud_rate": baud_rate,
            "timeout_ms": duration_ms_u64(timeout)
        }))?;
        Ok(backend)
    }

    fn list_resources(python_command: Option<&str>) -> Result<Vec<String>> {
        let mut backend = Self::spawn(python_command)?;
        let result = backend.request(json!({"op": "list_resources"}))?;
        let resources = result
            .as_array()
            .ok_or_else(|| transport_io_error("PyVISA list_resources returned non-array"))?
            .iter()
            .map(|value| {
                value
                    .as_str()
                    .map(str::to_string)
                    .ok_or_else(|| transport_io_error("PyVISA resource is not a string"))
            })
            .collect::<Result<Vec<_>>>()?;
        Ok(resources)
    }

    fn spawn(python_command: Option<&str>) -> Result<Self> {
        let (program, mut args) = python_invocation(python_command);
        args.push("-u".to_string());
        args.push("-c".to_string());
        args.push(PYVISA_WORKER.to_string());
        let mut child = Command::new(program)
            .args(args)
            .stdin(Stdio::piped())
            .stdout(Stdio::piped())
            .stderr(Stdio::inherit())
            .spawn()?;
        let stdin = child
            .stdin
            .take()
            .ok_or_else(|| transport_io_error("failed to open PyVISA worker stdin"))?;
        let stdout = child
            .stdout
            .take()
            .ok_or_else(|| transport_io_error("failed to open PyVISA worker stdout"))?;
        Ok(Self {
            _child: child,
            stdin,
            stdout: BufReader::new(stdout),
            next_id: 1,
        })
    }

    fn clear(&mut self) -> Result<()> {
        self.request(json!({"op": "clear"})).map(|_| ())
    }

    fn send(&mut self, command: &str) -> Result<()> {
        self.request(json!({"op": "send", "command": command}))
            .map(|_| ())
    }

    fn query_text(&mut self, command: &str, max_response_bytes: usize) -> Result<String> {
        let result = self.request(json!({
            "op": "query_text",
            "command": command,
            "max_response_bytes": max_response_bytes
        }))?;
        result
            .as_str()
            .map(str::to_string)
            .ok_or_else(|| transport_io_error("PyVISA query_text returned non-string"))
    }

    fn query_rall_exact(
        &mut self,
        expected_len: usize,
        post_write_delay: Duration,
    ) -> Result<Vec<u8>> {
        let result = self.request(json!({
            "op": "query_rall_exact",
            "expected_len": expected_len,
            "post_write_delay_ms": duration_ms_u64(post_write_delay)
        }))?;
        let encoded = result
            .as_str()
            .ok_or_else(|| transport_io_error("PyVISA query_rall_exact returned non-string"))?;
        BASE64_STANDARD.decode(encoded).map_err(|err| {
            transport_io_error(format!("PyVISA payload base64 decode failed: {err}"))
        })
    }

    fn request(&mut self, mut request: JsonValue) -> Result<JsonValue> {
        let id = self.next_id;
        self.next_id += 1;
        request["id"] = json!(id);
        serde_json::to_writer(&mut self.stdin, &request)
            .map_err(|err| transport_io_error(format!("PyVISA request serialize failed: {err}")))?;
        self.stdin.write_all(b"\n")?;
        self.stdin.flush()?;

        let mut line = String::new();
        let read_len = self.stdout.read_line(&mut line)?;
        if read_len == 0 {
            return Err(transport_io_error("PyVISA worker exited"));
        }
        let response: JsonValue = serde_json::from_str(&line).map_err(|err| {
            transport_io_error(format!("PyVISA response parse failed: {err}: {line}"))
        })?;
        if response.get("id").and_then(JsonValue::as_u64) != Some(id) {
            return Err(transport_io_error(format!(
                "PyVISA response id mismatch: expected={id}, observed={response}"
            )));
        }
        if response
            .get("ok")
            .and_then(JsonValue::as_bool)
            .unwrap_or(false)
        {
            Ok(response.get("result").cloned().unwrap_or(JsonValue::Null))
        } else if response.get("error_kind").and_then(JsonValue::as_str) == Some("timeout") {
            Err(TransportError::Timeout {
                context: "PyVISA OE1022D 操作",
                partial_len: response
                    .get("partial_len")
                    .and_then(JsonValue::as_u64)
                    .unwrap_or(0) as usize,
            })
        } else {
            Err(transport_io_error(format!(
                "PyVISA worker error: {}",
                response
                    .get("error")
                    .and_then(JsonValue::as_str)
                    .unwrap_or("unknown error")
            )))
        }
    }
}

fn read_rall_frame_blocking_exact_with_reader<R: Read>(
    reader: &mut R,
    expected_len: usize,
) -> Result<Vec<u8>> {
    let mut out = Vec::with_capacity(expected_len);
    let mut buf = [0_u8; 4096];

    while out.len() < expected_len {
        let remaining = expected_len.saturating_sub(out.len());
        let read_len = remaining.min(buf.len());
        match reader.read(&mut buf[..read_len]) {
            Ok(0) => thread::sleep(Duration::from_millis(1)),
            Ok(actual) => out.extend_from_slice(&buf[..actual]),
            Err(err)
                if matches!(
                    err.kind(),
                    std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
                ) =>
            {
                return Err(TransportError::Timeout {
                    context: "LabVIEW-style 读取 OE1022D RALL 定长帧",
                    partial_len: out.len(),
                });
            }
            Err(err) if err.kind() == std::io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(err.into()),
        }
    }

    Ok(out)
}

fn python_invocation(python_command: Option<&str>) -> (String, Vec<String>) {
    let env_command = std::env::var("ODMR_PYTHON").ok();
    let command = python_command.or(env_command.as_deref());
    if let Some(command) = command.filter(|value| !value.trim().is_empty()) {
        let mut parts = command
            .split_whitespace()
            .map(str::to_string)
            .collect::<Vec<_>>();
        let program = parts.remove(0);
        return (program, parts);
    }

    #[cfg(windows)]
    {
        ("py".to_string(), vec!["-3".to_string()])
    }
    #[cfg(not(windows))]
    {
        ("python3".to_string(), Vec::new())
    }
}

fn duration_ms_u64(duration: Duration) -> u64 {
    duration.as_millis().try_into().unwrap_or(u64::MAX)
}

fn transport_io_error(message: impl Into<String>) -> TransportError {
    TransportError::Io(std::io::Error::new(
        std::io::ErrorKind::Other,
        message.into(),
    ))
}

#[derive(Debug)]
pub struct RallFrameReadOutcome {
    pub payload: Vec<u8>,
    pub zero_byte_retry_count: usize,
}

#[cfg(test)]
fn read_rall_frame_exact_with_zero_retry_impl<FSend, FRead, FClear>(
    expected_len: usize,
    max_zero_byte_retries: usize,
    mut send_rall_query: FSend,
    mut read_rall_frame_exact: FRead,
    mut clear_input: FClear,
) -> Result<RallFrameReadOutcome>
where
    FSend: FnMut() -> Result<()>,
    FRead: FnMut() -> Result<Vec<u8>>,
    FClear: FnMut() -> Result<()>,
{
    let mut zero_byte_retry_count = 0;

    loop {
        send_rall_query()?;
        match read_rall_frame_exact() {
            Ok(payload) => {
                debug_assert_eq!(payload.len(), expected_len);
                return Ok(RallFrameReadOutcome {
                    payload,
                    zero_byte_retry_count,
                });
            }
            Err(TransportError::Timeout { partial_len: 0, .. })
                if zero_byte_retry_count < max_zero_byte_retries =>
            {
                zero_byte_retry_count += 1;
                clear_input()?;
            }
            Err(err) => return Err(err),
        }
    }
}

fn read_rall_frame_fast_with_reader<R: Read>(
    reader: &mut R,
    expected_len: usize,
    first_byte_deadline: Duration,
    frame_deadline: Duration,
) -> Result<Vec<u8>> {
    let started = std::time::Instant::now();
    let first_byte_deadline = started + first_byte_deadline;
    let frame_deadline = started + frame_deadline;
    let mut out = Vec::with_capacity(expected_len);
    let mut buf = [0_u8; 4096];

    while out.len() < expected_len && std::time::Instant::now() < frame_deadline {
        if out.is_empty() && std::time::Instant::now() >= first_byte_deadline {
            break;
        }
        let remaining = expected_len.saturating_sub(out.len());
        let read_len = remaining.min(buf.len());
        match reader.read(&mut buf[..read_len]) {
            Ok(0) => {
                thread::sleep(Duration::from_millis(1));
            }
            Ok(actual) => {
                out.extend_from_slice(&buf[..actual]);
            }
            Err(err)
                if matches!(
                    err.kind(),
                    std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
                ) =>
            {
                thread::sleep(Duration::from_millis(1));
            }
            Err(err) if err.kind() == std::io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(err.into()),
        }
    }

    if out.len() == expected_len {
        Ok(out)
    } else {
        Err(TransportError::Timeout {
            context: "读取 OE1022D RALL 定长帧",
            partial_len: out.len(),
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::VecDeque;
    use std::io;

    struct FakeReader {
        reads: VecDeque<io::Result<Vec<u8>>>,
        pending: VecDeque<u8>,
    }

    impl FakeReader {
        fn new(reads: Vec<io::Result<Vec<u8>>>) -> Self {
            Self {
                reads: reads.into(),
                pending: VecDeque::new(),
            }
        }
    }

    impl Read for FakeReader {
        fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
            if self.pending.is_empty() {
                let Some(next) = self.reads.pop_front() else {
                    return Err(io::Error::new(io::ErrorKind::TimedOut, "timeout"));
                };
                self.pending.extend(next?);
            }

            let len = self.pending.len().min(buf.len());
            for slot in buf.iter_mut().take(len) {
                *slot = self.pending.pop_front().expect("pending 不应为空");
            }
            Ok(len)
        }
    }

    #[test]
    fn rall_first_byte_timeout_fails_fast() {
        let mut reader = FakeReader::new(vec![]);
        let err = read_rall_frame_fast_with_reader(
            &mut reader,
            4,
            Duration::from_millis(2),
            Duration::from_millis(20),
        )
        .expect_err("首字节超时应直接失败");
        match err {
            TransportError::Timeout { partial_len, .. } => assert_eq!(partial_len, 0),
            other => panic!("unexpected error: {other}"),
        }
    }

    #[test]
    fn rall_frame_can_be_assembled_within_deadline() {
        let mut reader = FakeReader::new(vec![Ok(vec![1, 2]), Ok(vec![3, 4])]);
        let payload = read_rall_frame_fast_with_reader(
            &mut reader,
            4,
            Duration::from_millis(5),
            Duration::from_millis(20),
        )
        .expect("应在 frame deadline 内拼满");
        assert_eq!(payload, vec![1, 2, 3, 4]);
    }

    #[test]
    fn zero_byte_timeout_triggers_single_controlled_resend() {
        let mut send_calls = 0_usize;
        let mut clear_calls = 0_usize;
        let mut reads = VecDeque::from([
            Err(TransportError::Timeout {
                context: "读取 OE1022D RALL 定长帧",
                partial_len: 0,
            }),
            Ok(vec![1, 2, 3, 4]),
        ]);

        let outcome = read_rall_frame_exact_with_zero_retry_impl(
            4,
            1,
            || {
                send_calls += 1;
                Ok(())
            },
            || reads.pop_front().expect("应存在预置 read 结果"),
            || {
                clear_calls += 1;
                Ok(())
            },
        )
        .expect("零字节 timeout 后应允许同周期重试一次");

        assert_eq!(send_calls, 2);
        assert_eq!(clear_calls, 1);
        assert_eq!(outcome.zero_byte_retry_count, 1);
        assert_eq!(outcome.payload, vec![1, 2, 3, 4]);
    }

    #[test]
    fn partial_frame_timeout_does_not_resend() {
        let mut send_calls = 0_usize;
        let mut clear_calls = 0_usize;

        let err = read_rall_frame_exact_with_zero_retry_impl(
            4,
            1,
            || {
                send_calls += 1;
                Ok(())
            },
            || {
                Err(TransportError::Timeout {
                    context: "读取 OE1022D RALL 定长帧",
                    partial_len: 2,
                })
            },
            || {
                clear_calls += 1;
                Ok(())
            },
        )
        .expect_err("半帧超时不应触发 resend");

        assert_eq!(send_calls, 1);
        assert_eq!(clear_calls, 0);
        match err {
            TransportError::Timeout { partial_len, .. } => assert_eq!(partial_len, 2),
            other => panic!("unexpected error: {other}"),
        }
    }
}
