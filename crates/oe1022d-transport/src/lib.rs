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

use device_transport::{
    query_ascii_line, write_command, LineTransportOptions, Result, TransportError,
};
use oe1022d_commands::{oe1022d_query_reference_source, oe1022d_rall_query};
use serialport::{ClearBuffer, DataBits, FlowControl, Parity, SerialPort, StopBits};
use std::io::Read;
use std::thread;
use std::time::Duration;

#[derive(Debug, Clone)]
pub struct Oe1022dTransportConfig {
    pub port_path: String,
    pub baud_rate: u32,
    pub timeout: Duration,
    pub rall_chunk_timeout: Duration,
    pub rall_first_byte_deadline: Duration,
    pub rall_frame_deadline: Duration,
    pub response_max_bytes: usize,
    pub command_terminator: Vec<u8>,
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
            // RALL? 是 50ms 更新一次的定长快照。
            // ASCII query 可以容忍更保守的 timeout。
            timeout: Duration::from_millis(300),
            // RALL? 热路径不能把一次空读放大成 300ms 级黑洞。
            // 单次 chunk 只等很短时间，整帧再用单独 deadline 控制。
            rall_chunk_timeout: Duration::from_millis(5),
            // 如果连第一个字节都迟迟不来，就尽快把这一轮判空，
            // 让上层有机会在同周期内重发一次。
            rall_first_byte_deadline: Duration::from_millis(20),
            rall_frame_deadline: Duration::from_millis(120),
            response_max_bytes: 4096,
            command_terminator: b"\r".to_vec(),
        }
    }
}

pub struct Oe1022dTransport {
    port: Box<dyn SerialPort>,
    options: LineTransportOptions,
    query_timeout: Duration,
    rall_chunk_timeout: Duration,
    rall_first_byte_deadline: Duration,
    rall_frame_deadline: Duration,
}

impl Oe1022dTransport {
    pub fn open(config: &Oe1022dTransportConfig) -> Result<Self> {
        let port = serialport::new(&config.port_path, config.baud_rate)
            .data_bits(DataBits::Eight)
            .parity(Parity::None)
            .stop_bits(StopBits::One)
            .flow_control(FlowControl::None)
            .timeout(config.timeout)
            .open()?;

        Ok(Self {
            port,
            options: config.line_options(),
            query_timeout: config.timeout,
            rall_chunk_timeout: config.rall_chunk_timeout,
            rall_first_byte_deadline: config.rall_first_byte_deadline,
            rall_frame_deadline: config.rall_frame_deadline,
        })
    }

    pub fn clear_input(&mut self) -> Result<()> {
        self.port.clear(ClearBuffer::Input)?;
        Ok(())
    }

    pub fn send(&mut self, command: &str) -> Result<()> {
        write_command(&mut self.port, command, &self.options.command_terminator)
    }

    pub fn query_text(&mut self, command: &str) -> Result<String> {
        self.set_port_timeout(self.query_timeout)?;
        query_ascii_line(&mut self.port, command, &self.options)
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
        self.send_rall_query()?;
        self.read_rall_frame_exact(expected_len)
    }

    pub fn send_rall_query(&mut self) -> Result<()> {
        write_command(
            &mut self.port,
            oe1022d_rall_query(),
            &self.options.command_terminator,
        )
    }

    pub fn read_rall_frame_exact(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        self.read_rall_frame_fast(expected_len)
    }

    pub fn query_rall_frame_exact_with_zero_retry(
        &mut self,
        expected_len: usize,
        max_zero_byte_retries: usize,
    ) -> Result<RallFrameReadOutcome> {
        let mut zero_byte_retry_count = 0;

        loop {
            self.send_rall_query()?;
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
        self.set_port_timeout(self.query_timeout)?;
        write_command(
            &mut self.port,
            oe1022d_rall_query(),
            &self.options.command_terminator,
        )?;

        let mut out = Vec::with_capacity(max_bytes.min(4096));
        let mut buf = [0_u8; 1024];

        loop {
            match self.port.read(&mut buf) {
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
        self.port.set_timeout(timeout)?;
        Ok(())
    }

    fn read_rall_frame_fast(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        self.set_port_timeout(self.rall_chunk_timeout)?;
        let started = std::time::Instant::now();
        let first_byte_deadline = started + self.rall_first_byte_deadline;
        let frame_deadline = started + self.rall_frame_deadline;
        let mut out = Vec::with_capacity(expected_len);
        let mut buf = [0_u8; 4096];

        while out.len() < expected_len && std::time::Instant::now() < frame_deadline {
            if out.is_empty() && std::time::Instant::now() >= first_byte_deadline {
                break;
            }
            let remaining = expected_len.saturating_sub(out.len());
            let read_len = remaining.min(buf.len());
            match self.port.read(&mut buf[..read_len]) {
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
}

pub struct RallFrameReadOutcome {
    pub payload: Vec<u8>,
    pub zero_byte_retry_count: usize,
}
