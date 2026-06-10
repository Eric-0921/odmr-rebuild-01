//! CNI Laser PSU-SR 第一版命令 helper。
//!
//! 这里不返回 ASCII 字符串，而是直接生成原始字节帧。
//! 这是因为该设备不是标准 SCPI 设备。

/// 固定帧头。
pub const CNI_LASER_HEADER: [u8; 2] = [0x55, 0xAA];

const CNI_LASER_CMD_SET_POWER: u8 = 0x05;
const CNI_LASER_CMD_OUTPUT: u8 = 0x03;
const CNI_LASER_SUBCMD_SET_POWER: u8 = 0x01;
const CNI_LASER_SUBCMD_OFF: u8 = 0x00;
const CNI_LASER_SUBCMD_ON: u8 = 0x01;

/// `55 AA 05 01 <hi> <lo> <checksum>`：设置功率，单位 mW。
pub fn cni_laser_power_set(power_mw: u16) -> Vec<u8> {
    let hi = ((power_mw >> 8) & 0xFF) as u8;
    let lo = (power_mw & 0xFF) as u8;
    let data = [CNI_LASER_SUBCMD_SET_POWER, hi, lo];
    build_frame(CNI_LASER_CMD_SET_POWER, &data)
}

/// `55 AA 03 00 03`：关闭激光输出。
pub fn cni_laser_output_off() -> Vec<u8> {
    build_frame(CNI_LASER_CMD_OUTPUT, &[CNI_LASER_SUBCMD_OFF])
}

/// `55 AA 03 01 04`：开启激光输出。
pub fn cni_laser_output_on() -> Vec<u8> {
    build_frame(CNI_LASER_CMD_OUTPUT, &[CNI_LASER_SUBCMD_ON])
}

fn build_frame(command: u8, data: &[u8]) -> Vec<u8> {
    let checksum = cni_laser_checksum(command, data);
    let mut out = Vec::with_capacity(2 + 1 + data.len() + 1);
    out.extend_from_slice(&CNI_LASER_HEADER);
    out.push(command);
    out.extend_from_slice(data);
    out.push(checksum);
    out
}

/// 校验和规则：取 `command + data bytes` 的低 8 位。
pub fn cni_laser_checksum(command: u8, data: &[u8]) -> u8 {
    let sum = command as u16 + data.iter().map(|value| *value as u16).sum::<u16>();
    (sum & 0xFF) as u8
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn power_frame_matches_manual_example() {
        assert_eq!(
            cni_laser_power_set(100),
            vec![0x55, 0xAA, 0x05, 0x01, 0x00, 0x64, 0x6A]
        );
    }

    #[test]
    fn output_frames_match_manual_examples() {
        assert_eq!(cni_laser_output_off(), vec![0x55, 0xAA, 0x03, 0x00, 0x03]);
        assert_eq!(cni_laser_output_on(), vec![0x55, 0xAA, 0x03, 0x01, 0x04]);
    }

    #[test]
    fn checksum_rule_is_low_byte_sum() {
        assert_eq!(cni_laser_checksum(0x05, &[0x01, 0x00, 0x64]), 0x6A);
    }
}
