#!/usr/bin/env python3
"""
协议验证脚本 - 严格按说明书流程，逐条发送 SCPI 指令并记录设备响应
验证对象：Maynuo M8812 电源（X/Y/Z 三轴）
安全原则：
  1. 电流始终从 0 开始
  2. 任何异常立即断开
  3. 不与其他程序同时占用串口
"""
import serial
import time
import sys

# 设备配置（与 para.xml 一致）
DEVICES = {
    'X': {'port': 'COM4', 'sn_tail': '2020', 'coil': 143.26},
    'Y': {'port': 'COM6', 'sn_tail': '2022', 'coil': 141.77},
    'Z': {'port': 'COM3', 'sn_tail': '2003', 'coil': 156.15},
}
BAUDRATE = 9600
READ_TIMEOUT = 0.3


def log(msg):
    print(f"[{time.strftime('%H:%M:%S')}] {msg}")


def open_port(port_name):
    sp = serial.Serial(
        port=port_name,
        baudrate=BAUDRATE,
        bytesize=serial.EIGHTBITS,
        parity=serial.PARITY_NONE,
        stopbits=serial.STOPBITS_ONE,
        timeout=READ_TIMEOUT,
    )
    sp.dtr = True
    time.sleep(0.05)
    return sp


def send_cmd(sp, cmd, expect_response=False, delay=0.1):
    """发送 SCPI 指令，可选等待并返回响应"""
    full_cmd = cmd if cmd.endswith('\n') else cmd + '\n'
    sp.write(full_cmd.encode('ascii'))
    sp.flush()
    log(f"  TX -> {repr(full_cmd)}")
    if delay:
        time.sleep(delay)
    if expect_response:
        raw = sp.read_all()
        resp = raw.decode('ascii', errors='replace').strip()
        log(f"  RX <- {repr(resp) if resp else '(no response)'}")
        return resp
    return None


def verify_device(sp, axis):
    """步骤1：验证设备身份 (*IDN?)"""
    log(f"[{axis}] 验证设备身份...")
    sp.reset_input_buffer()
    resp = send_cmd(sp, '*IDN?', expect_response=True, delay=0.2)
    if not resp:
        raise RuntimeError(f"{axis}轴设备无响应")
    parts = resp.split(',')
    if len(parts) < 4 or 'MAYNUO' not in parts[0].upper():
        raise RuntimeError(f"{axis}轴设备身份异常: {resp}")
    sn = parts[2] if len(parts) >= 3 else 'unknown'
    log(f"  OK: {resp} (SN: {sn})")
    return resp


def init_sequence(sp, axis):
    """步骤2：连接初始化（对应代码中的 toolStripButton1_Click）"""
    log(f"[{axis}] 连接初始化...")
    send_cmd(sp, 'SYST:REM', delay=0.05)
    send_cmd(sp, 'VOLT 75', delay=0.05)
    send_cmd(sp, 'CURR 0', delay=0.05)
    send_cmd(sp, 'OUTP 0', delay=0.05)
    log(f"  OK: 远程模式, 电压75V, 电流0A, 输出关闭")


def read_measured_current(sp, axis):
    """步骤3：查询实际电流（对应 timer1_Tick + DataReceived）"""
    log(f"[{axis}] 查询实际输出电流...")
    sp.reset_input_buffer()
    resp = send_cmd(sp, 'MEAS:CURR?', expect_response=True, delay=0.2)
    if resp:
        try:
            curr_A = float(resp)
            curr_mA = curr_A * 1000.0
            log(f"  OK: {curr_A:.6f} A = {curr_mA:.2f} mA")
            return curr_mA
        except ValueError:
            log(f"  WARN: 无法解析响应: {resp}")
    return None


def set_current_safe(sp, axis, curr_mA):
    """步骤4：设置电流（严格限制在安全范围）"""
    if curr_mA < 0 or curr_mA > 5000:
        raise ValueError(f"电流 {curr_mA} mA 超出安全范围 [0, 5000]")
    curr_A = abs(curr_mA) / 1000.0
    log(f"[{axis}] 设置电流 {curr_mA:.2f} mA ({curr_A:.5f} A)...")
    send_cmd(sp, f'CURR {curr_A:.5f}', delay=0.05)
    log(f"  OK")


def set_output(sp, axis, on):
    """步骤5：开关输出"""
    state = 1 if on else 0
    log(f"[{axis}] 设置输出 {'ON' if on else 'OFF'}...")
    send_cmd(sp, f'OUTP {state}', delay=0.05)
    log(f"  OK")


def shutdown_sequence(sp, axis):
    """步骤6：安全断开（对应代码中的断开连接）"""
    log(f"[{axis}] 安全断开...")
    set_current_safe(sp, axis, 0)
    set_output(sp, axis, False)
    send_cmd(sp, 'SYST:LOC', delay=0.05)
    log(f"  OK: 电流归零, 输出关闭, 本地模式")


def verify_axis(axis, cfg):
    """对单轴进行完整验证"""
    port = cfg['port']
    log(f"\n{'='*60}")
    log(f"开始验证 {axis}轴 ({port}, SN尾号 {cfg['sn_tail']})")
    log(f"{'='*60}")

    sp = open_port(port)
    try:
        # 步骤1: 身份验证
        verify_device(sp, axis)

        # 步骤2: 初始化
        init_sequence(sp, axis)

        # 步骤3: 查询电流（应接近0）
        read_measured_current(sp, axis)

        # 步骤4: 设置一个极小测试电流并开启输出
        test_curr_mA = 10.0  # 10mA，极小值，安全
        set_current_safe(sp, axis, test_curr_mA)
        set_output(sp, axis, True)
        time.sleep(0.2)

        # 步骤5: 再次查询电流，验证设置生效
        actual = read_measured_current(sp, axis)
        if actual is not None:
            if abs(actual - test_curr_mA) < 5.0:
                log(f"  PASS: 实际电流 {actual:.2f} mA 接近设定值 {test_curr_mA:.2f} mA")
            else:
                log(f"  WARN: 实际电流 {actual:.2f} mA 与设定值 {test_curr_mA:.2f} mA 偏差较大")

        # 步骤6: 安全关闭
        shutdown_sequence(sp, axis)

    except Exception as e:
        log(f"  ERROR: {e}")
        # 紧急关闭
        try:
            send_cmd(sp, 'OUTP 0')
            send_cmd(sp, 'SYST:LOC')
        except:
            pass
        raise
    finally:
        sp.close()
        log(f"[{axis}] 串口已关闭")


def main():
    log("磁场控制系统 - 协议验证脚本")
    log("安全警告：本脚本只使用 0~10mA 极小电流进行验证")
    log("请确保设备已上电、串口连接正确，且原程序未运行\n")

    # 验证所有轴
    for axis, cfg in DEVICES.items():
        try:
            verify_axis(axis, cfg)
        except Exception as e:
            log(f"[{axis}] 验证失败: {e}")

    log(f"\n{'='*60}")
    log("验证完成")
    log(f"{'='*60}")


if __name__ == '__main__':
    main()
