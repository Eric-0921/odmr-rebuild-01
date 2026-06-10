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
    query_ascii_line, read_exact_payload, write_command, LineTransportOptions, Result,
};
use oe1022d_commands::{oe1022d_query_reference_source, oe1022d_rall_query};
use serialport::{ClearBuffer, DataBits, FlowControl, Parity, SerialPort, StopBits};
use std::io::Read;
use std::time::Duration;

#[derive(Debug, Clone)]
pub struct Oe1022dTransportConfig {
    pub port_path: String,
    pub baud_rate: u32,
    pub timeout: Duration,
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
            timeout: Duration::from_millis(1000),
            response_max_bytes: 4096,
            command_terminator: b"\r".to_vec(),
        }
    }
}

pub struct Oe1022dTransport {
    port: Box<dyn SerialPort>,
    options: LineTransportOptions,
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
        write_command(
            &mut self.port,
            oe1022d_rall_query(),
            &self.options.command_terminator,
        )?;
        read_exact_payload(&mut self.port, expected_len)
    }

    /// 发送一次 `RALL?`，并持续读取直到串口超时。
    ///
    /// 这条路径专供单帧 smoke 验证使用：
    /// - 只要求拿到非零 payload
    /// - 不在 transport 层强制假设固定帧长度
    pub fn query_rall_frame_until_timeout(&mut self, max_bytes: usize) -> Result<Vec<u8>> {
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
