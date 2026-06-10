//! M8812 最小串口连接层。
//!
//! 当前职责：
//! - 打开串口
//! - 发送最小 SCPI 子集
//! - 读取 ASCII query 响应
//! - 暴露最小身份和电流回读接口

use device_transport::{query_ascii_line, write_command, LineTransportOptions, Result};
use m8812_commands::{
    m8812_query_error, m8812_query_idn, m8812_query_meas_current_a, m8812_set_current_a,
    m8812_set_local, m8812_set_output, m8812_set_remote,
};
use serialport::{ClearBuffer, DataBits, FlowControl, Parity, SerialPort, StopBits};
use std::time::Duration;

#[derive(Debug, Clone)]
pub struct M8812TransportConfig {
    pub port_path: String,
    pub baud_rate: u32,
    pub timeout: Duration,
    pub dtr_on_open: bool,
    pub response_max_bytes: usize,
}

impl M8812TransportConfig {
    fn line_options(&self) -> LineTransportOptions {
        LineTransportOptions {
            command_terminator: b"\n".to_vec(),
            max_response_bytes: self.response_max_bytes,
        }
    }
}

impl Default for M8812TransportConfig {
    fn default() -> Self {
        Self {
            port_path: "/dev/cu.PL2303G-USBtoUART".to_string(),
            baud_rate: 9600,
            timeout: Duration::from_millis(300),
            dtr_on_open: true,
            response_max_bytes: 4096,
        }
    }
}

pub struct M8812Transport {
    port: Box<dyn SerialPort>,
    options: LineTransportOptions,
}

impl M8812Transport {
    pub fn open(config: &M8812TransportConfig) -> Result<Self> {
        let mut port = serialport::new(&config.port_path, config.baud_rate)
            .data_bits(DataBits::Eight)
            .parity(Parity::None)
            .stop_bits(StopBits::One)
            .flow_control(FlowControl::None)
            .timeout(config.timeout)
            .open()?;

        if config.dtr_on_open {
            port.write_data_terminal_ready(true)?;
        }

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

    pub fn query(&mut self, command: &str) -> Result<String> {
        query_ascii_line(&mut self.port, command, &self.options)
    }

    pub fn query_idn(&mut self) -> Result<String> {
        self.query(m8812_query_idn())
    }

    pub fn enter_remote(&mut self) -> Result<()> {
        self.send(m8812_set_remote())
    }

    pub fn enter_local(&mut self) -> Result<()> {
        self.send(m8812_set_local())
    }

    pub fn query_error(&mut self) -> Result<String> {
        self.query(m8812_query_error())
    }

    pub fn set_current_a(&mut self, amps: f64) -> Result<()> {
        self.send(&m8812_set_current_a(amps))
    }

    pub fn set_output(&mut self, enabled: bool) -> Result<()> {
        self.send(m8812_set_output(enabled))
    }

    pub fn query_meas_current_a(&mut self) -> Result<String> {
        self.query(m8812_query_meas_current_a())
    }
}
