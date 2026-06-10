#!/usr/bin/env python3
"""
扫描串口并识别 Maynuo 电源设备
通过发送 *IDN? 命令读取设备身份信息
"""
import serial
import serial.tools.list_ports
import time
import sys

def scan_maynuo_devices():
    ports = list(serial.tools.list_ports.comports())
    print("=" * 60)
    print("本机可用串口列表:")
    print("=" * 60)
    for p in ports:
        sn = p.serial_number if p.serial_number else "N/A"
        print(f"  {p.device}: {p.description}")
        print(f"    USB VID:PID = {p.vid:04X}:{p.pid:04X}, USB SN = {sn}")
    print()
    
    print("=" * 60)
    print("尝试识别 Maynuo 设备 (发送 *IDN? 查询)...")
    print("=" * 60)
    
    devices = []
    for p in ports:
        try:
            with serial.Serial(p.device, baudrate=9600, timeout=1) as s:
                s.write(b'*IDN?\n')
                time.sleep(0.3)
                response = s.read_all().decode('ascii', errors='ignore').strip()
                if response:
                    print(f"  {p.device}: {response}")
                    devices.append({
                        'port': p.device,
                        'idn': response,
                        'description': p.description
                    })
                else:
                    print(f"  {p.device}: 无响应")
        except Exception as e:
            print(f"  {p.device}: 打开失败 - {e}")
    
    print()
    return devices

def match_devices(devices):
    """根据用户提供的SN尾号匹配设备到轴"""
    print("=" * 60)
    print("设备匹配结果 (根据 *IDN? 中的 SN 尾号):")
    print("=" * 60)
    
    axis_map = {}
    for dev in devices:
        idn = dev['idn']
        # Maynuo IDN格式通常类似: MAYNUO,ELECTRONIC,MODEL,S/N:xxxx
        sn = None
        if 'S/N:' in idn:
            sn_part = idn.split('S/N:')[-1].strip()
            sn = sn_part
        elif 'SN' in idn.upper():
            # 尝试其他格式
            parts = idn.split(',')
            for part in parts:
                if 'SN' in part.upper() or 'S/N' in part.upper():
                    sn = part.strip()
        
        if sn:
            sn_tail = sn[-4:] if len(sn) >= 4 else sn
            if sn_tail.endswith('2020'):
                axis = 'X轴'
                axis_map['X'] = dev['port']
            elif sn_tail.endswith('2022'):
                axis = 'Y轴'
                axis_map['Y'] = dev['port']
            elif sn_tail.endswith('2003'):
                axis = 'Z轴'
                axis_map['Z'] = dev['port']
            else:
                axis = '未知轴'
            
            print(f"  {dev['port']} -> {axis} (SN尾号: {sn_tail}, IDN: {idn})")
        else:
            print(f"  {dev['port']} -> 无法解析SN (IDN: {idn})")
    
    print()
    return axis_map

if __name__ == '__main__':
    devices = scan_maynuo_devices()
    if devices:
        axis_map = match_devices(devices)
        print("=" * 60)
        print("建议的 para.xml 串口配置:")
        print("=" * 60)
        if 'X' in axis_map:
            print(f'  PortX="{axis_map["X"]}"  (SN尾号2020)')
        if 'Y' in axis_map:
            print(f'  PortY="{axis_map["Y"]}"  (SN尾号2022)')
        if 'Z' in axis_map:
            print(f'  PortZ="{axis_map["Z"]}"  (SN尾号2003)')
        print()
    else:
        print("未识别到任何响应的串口设备。")
        print("可能原因:")
        print("  1. 设备未上电")
        print("  2. 串口线未连接")
        print("  3. 设备不支持 SCPI *IDN? 命令")
