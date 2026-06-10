//! 设备 transport 共用基础。
//!
//! 这层只负责非常薄的 I/O 辅助：
//! - 写入一条命令
//! - 读取一行 ASCII 响应
//! - 读取定长二进制 payload
//! - 统一错误类型
//!
//! 不负责：
//! - 设备级命令语义
//! - 业务状态机
//! - runtime 生命周期

use std::fmt;
use std::io::{Read, Write};

/// transport 层统一错误。
#[derive(Debug)]
pub enum TransportError {
    Io(std::io::Error),
    Serial(serialport::Error),
    Utf8(std::string::FromUtf8Error),
    Timeout {
        context: &'static str,
        partial_len: usize,
    },
    ResponseTooLong {
        max_len: usize,
    },
}

impl fmt::Display for TransportError {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Io(err) => write!(f, "I/O 错误: {err}"),
            Self::Serial(err) => write!(f, "串口错误: {err}"),
            Self::Utf8(err) => write!(f, "UTF-8 解析错误: {err}"),
            Self::Timeout {
                context,
                partial_len,
            } => write!(f, "{context} 超时，已读取 {partial_len} 字节"),
            Self::ResponseTooLong { max_len } => {
                write!(f, "响应超过上限 {max_len} 字节")
            }
        }
    }
}

impl std::error::Error for TransportError {}

impl From<std::io::Error> for TransportError {
    fn from(value: std::io::Error) -> Self {
        Self::Io(value)
    }
}

impl From<serialport::Error> for TransportError {
    fn from(value: serialport::Error) -> Self {
        Self::Serial(value)
    }
}

impl From<std::string::FromUtf8Error> for TransportError {
    fn from(value: std::string::FromUtf8Error) -> Self {
        Self::Utf8(value)
    }
}

pub type Result<T> = std::result::Result<T, TransportError>;

/// 一行命令交互的基础参数。
#[derive(Debug, Clone)]
pub struct LineTransportOptions {
    pub command_terminator: Vec<u8>,
    pub max_response_bytes: usize,
}

impl Default for LineTransportOptions {
    fn default() -> Self {
        Self {
            command_terminator: b"\n".to_vec(),
            max_response_bytes: 4096,
        }
    }
}

/// 写入一条 ASCII 命令，并在末尾补 terminator。
pub fn write_command<W: Write>(writer: &mut W, command: &str, terminator: &[u8]) -> Result<()> {
    writer.write_all(command.as_bytes())?;
    writer.write_all(terminator)?;
    writer.flush()?;
    Ok(())
}

/// 读取一行 ASCII 响应。
///
/// 这里把 `\\n` 或 `\\r` 都视为行结束符，避免不同设备终止符差异。
pub fn read_ascii_line<R: Read>(reader: &mut R, max_response_bytes: usize) -> Result<String> {
    let mut out = Vec::new();
    let mut buf = [0_u8; 1];

    loop {
        match reader.read(&mut buf) {
            Ok(0) => {
                if out.is_empty() {
                    return Err(TransportError::Timeout {
                        context: "读取 ASCII 响应",
                        partial_len: 0,
                    });
                }
                break;
            }
            Ok(_) => {
                let byte = buf[0];
                if byte == b'\n' || byte == b'\r' {
                    break;
                }
                out.push(byte);
                if out.len() > max_response_bytes {
                    return Err(TransportError::ResponseTooLong {
                        max_len: max_response_bytes,
                    });
                }
            }
            Err(err)
                if matches!(
                    err.kind(),
                    std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
                ) =>
            {
                if out.is_empty() {
                    return Err(TransportError::Timeout {
                        context: "读取 ASCII 响应",
                        partial_len: 0,
                    });
                }
                break;
            }
            Err(err) if err.kind() == std::io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(TransportError::Io(err)),
        }
    }

    Ok(String::from_utf8(out)?.trim().to_string())
}

/// 发送命令并读取一行 ASCII 响应。
pub fn query_ascii_line<T: Read + Write>(
    io: &mut T,
    command: &str,
    options: &LineTransportOptions,
) -> Result<String> {
    write_command(io, command, &options.command_terminator)?;
    read_ascii_line(io, options.max_response_bytes)
}

/// 读取定长 payload。
pub fn read_exact_payload<R: Read>(reader: &mut R, payload_len: usize) -> Result<Vec<u8>> {
    let mut out = vec![0_u8; payload_len];
    let mut offset = 0_usize;

    while offset < payload_len {
        match reader.read(&mut out[offset..]) {
            Ok(0) => {
                return Err(TransportError::Timeout {
                    context: "读取二进制 payload",
                    partial_len: offset,
                });
            }
            Ok(read_len) => {
                offset += read_len;
            }
            Err(err)
                if matches!(
                    err.kind(),
                    std::io::ErrorKind::TimedOut | std::io::ErrorKind::WouldBlock
                ) =>
            {
                return Err(TransportError::Timeout {
                    context: "读取二进制 payload",
                    partial_len: offset,
                });
            }
            Err(err) if err.kind() == std::io::ErrorKind::Interrupted => continue,
            Err(err) => return Err(TransportError::Io(err)),
        }
    }

    Ok(out)
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::VecDeque;
    use std::io;

    struct FakeIo {
        reads: VecDeque<io::Result<Vec<u8>>>,
        writes: Vec<u8>,
        pending: VecDeque<u8>,
    }

    impl FakeIo {
        fn new(chunks: Vec<io::Result<Vec<u8>>>) -> Self {
            Self {
                reads: chunks.into(),
                writes: Vec::new(),
                pending: VecDeque::new(),
            }
        }
    }

    impl Read for FakeIo {
        fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> {
            if self.pending.is_empty() {
                let Some(chunk) = self.reads.pop_front() else {
                    return Ok(0);
                };
                let chunk = chunk?;
                self.pending.extend(chunk);
            }

            let len = self.pending.len().min(buf.len());
            for slot in buf.iter_mut().take(len) {
                *slot = self.pending.pop_front().expect("pending 不应为空");
            }
            Ok(len)
        }
    }

    impl Write for FakeIo {
        fn write(&mut self, buf: &[u8]) -> io::Result<usize> {
            self.writes.extend_from_slice(buf);
            Ok(buf.len())
        }

        fn flush(&mut self) -> io::Result<()> {
            Ok(())
        }
    }

    #[test]
    fn query_ascii_line_writes_command_and_reads_trimmed_response() {
        let mut io = FakeIo::new(vec![Ok(b"OK\r".to_vec())]);
        let options = LineTransportOptions::default();
        let response = query_ascii_line(&mut io, "*IDN?", &options).unwrap();
        assert_eq!(response, "OK");
        assert_eq!(io.writes, b"*IDN?\n");
    }

    #[test]
    fn read_exact_payload_collects_multiple_chunks() {
        let mut io = FakeIo::new(vec![Ok(vec![1, 2]), Ok(vec![3]), Ok(vec![4, 5])]);
        let payload = read_exact_payload(&mut io, 5).unwrap();
        assert_eq!(payload, vec![1, 2, 3, 4, 5]);
    }
}
