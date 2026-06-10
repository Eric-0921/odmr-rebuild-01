//! CNI Laser 最小串口连接层。
//!
//! 当前职责：
//! - 打开串口
//! - 发送二进制控制帧
//! - 可选读取固定长度 echo
//!
//! 不负责：
//! - 激光安全流程
//! - 高层状态机
//! - runtime 编排

use cni_laser_commands::{cni_laser_output_off, cni_laser_output_on, cni_laser_power_set};
use device_transport::{read_exact_payload, Result};
use serialport::{DataBits, FlowControl, Parity, SerialPort, StopBits};
use std::io::Write;
use std::time::Duration;

#[derive(Debug, Clone)]
pub struct CniLaserTransportConfig {
    pub port_path: String,
    pub baud_rate: u32,
    pub timeout: Duration,
}

impl Default for CniLaserTransportConfig {
    fn default() -> Self {
        Self {
            port_path: "/dev/cu.usbserial".to_string(),
            baud_rate: 9600,
            timeout: Duration::from_millis(500),
        }
    }
}

pub struct CniLaserTransport {
    port: Box<dyn SerialPort>,
}

impl CniLaserTransport {
    pub fn open(config: &CniLaserTransportConfig) -> Result<Self> {
        let port = serialport::new(&config.port_path, config.baud_rate)
            .data_bits(DataBits::Eight)
            .parity(Parity::None)
            .stop_bits(StopBits::One)
            .flow_control(FlowControl::None)
            .timeout(config.timeout)
            .open()?;

        Ok(Self { port })
    }

    pub fn write_frame(&mut self, frame: &[u8]) -> Result<()> {
        self.port.write_all(frame)?;
        self.port.flush()?;
        Ok(())
    }

    pub fn read_echo_exact(&mut self, expected_len: usize) -> Result<Vec<u8>> {
        read_exact_payload(&mut self.port, expected_len)
    }

    pub fn set_power_mw(&mut self, power_mw: u16) -> Result<()> {
        self.write_frame(&cni_laser_power_set(power_mw))
    }

    pub fn output_off(&mut self) -> Result<()> {
        self.write_frame(&cni_laser_output_off())
    }

    pub fn output_on(&mut self) -> Result<()> {
        self.write_frame(&cni_laser_output_on())
    }
}
