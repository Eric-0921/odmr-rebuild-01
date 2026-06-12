//! SMB100A 最小连接层。
//!
//! 当前只负责：
//! - TCP socket 建连
//! - 发送 SCPI 命令
//! - 读取一行 ASCII 响应
//! - 最小身份/健康查询
//!
//! 不负责：
//! - sweep 业务编排
//! - station resolve
//! - runtime 生命周期

use device_transport::{query_ascii_line, write_command, LineTransportOptions, Result};
use smb100a_commands::{
    smb100a_query_error_next, smb100a_query_frequency_mode, smb100a_query_idn,
    smb100a_query_operation_complete, smb100a_query_output,
};
use std::net::{TcpStream, ToSocketAddrs};
use std::time::Duration;

#[derive(Debug, Clone)]
pub struct Smb100aTransportConfig {
    pub host: String,
    pub port: u16,
    pub connect_timeout: Duration,
    pub read_timeout: Duration,
    pub write_timeout: Duration,
    pub response_max_bytes: usize,
}

impl Smb100aTransportConfig {
    fn line_options(&self) -> LineTransportOptions {
        LineTransportOptions {
            command_terminator: b"\n".to_vec(),
            max_response_bytes: self.response_max_bytes,
        }
    }
}

impl Default for Smb100aTransportConfig {
    fn default() -> Self {
        Self {
            host: "127.0.0.1".to_string(),
            port: 5025,
            connect_timeout: Duration::from_millis(500),
            read_timeout: Duration::from_millis(500),
            write_timeout: Duration::from_millis(500),
            response_max_bytes: 4096,
        }
    }
}

pub struct Smb100aTransport {
    stream: TcpStream,
    options: LineTransportOptions,
}

impl Smb100aTransport {
    /// 建立到 SMB100A 的 TCP 连接。
    pub fn connect(config: &Smb100aTransportConfig) -> Result<Self> {
        let address = (config.host.as_str(), config.port)
            .to_socket_addrs()?
            .next()
            .expect("SMB100A 地址解析结果不能为空");
        let stream = TcpStream::connect_timeout(&address, config.connect_timeout)?;
        stream.set_read_timeout(Some(config.read_timeout))?;
        stream.set_write_timeout(Some(config.write_timeout))?;
        stream.set_nodelay(true)?;

        Ok(Self {
            stream,
            options: config.line_options(),
        })
    }

    /// 发送一条不期待响应的命令。
    pub fn send(&mut self, command: &str) -> Result<()> {
        write_command(&mut self.stream, command, &self.options.command_terminator)
    }

    /// 发送一条 query 并读取一行响应。
    pub fn query(&mut self, command: &str) -> Result<String> {
        query_ascii_line(&mut self.stream, command, &self.options)
    }

    pub fn query_idn(&mut self) -> Result<String> {
        self.query(smb100a_query_idn())
    }

    pub fn query_error_next(&mut self) -> Result<String> {
        self.query(smb100a_query_error_next())
    }

    pub fn query_output(&mut self) -> Result<String> {
        self.query(smb100a_query_output())
    }

    pub fn query_frequency_mode(&mut self) -> Result<String> {
        self.query(smb100a_query_frequency_mode())
    }

    pub fn query_operation_complete(&mut self) -> Result<String> {
        self.query(smb100a_query_operation_complete())
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::{Read, Write};
    use std::net::TcpListener;
    use std::thread;

    #[test]
    fn connect_and_query_idn_roundtrip() {
        let listener = TcpListener::bind("127.0.0.1:0").unwrap();
        let addr = listener.local_addr().unwrap();

        let handle = thread::spawn(move || {
            let (mut socket, _) = listener.accept().unwrap();
            let mut buf = [0_u8; 64];
            let read_len = socket.read(&mut buf).unwrap();
            let observed = std::str::from_utf8(&buf[..read_len]).unwrap();
            assert!(observed.starts_with("*IDN?"));
            socket.write_all(b"Rohde&Schwarz,SMB100A\r\n").unwrap();
        });

        let config = Smb100aTransportConfig {
            host: addr.ip().to_string(),
            port: addr.port(),
            ..Smb100aTransportConfig::default()
        };

        let mut transport = Smb100aTransport::connect(&config).unwrap();
        let idn = transport.query_idn().unwrap();
        assert_eq!(idn, "Rohde&Schwarz,SMB100A");
        handle.join().unwrap();
    }
}
