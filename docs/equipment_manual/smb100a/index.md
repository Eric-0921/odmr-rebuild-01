# R&S® SMB100A RF and Microwave Signal Generator Operating Manual

ROHDE&SCHWARZ SMB 100A - SIGNAL GENERATOR 100kHz, 12.75GHz

1407080632

Version 23

ROHDE & SCHWARZ

Make ideas real

RS

This document describes the R&S® SMB100A, stock no. 1406.6000K02/K03 and its options.

R&S® SMB-B1/-B1H

R&S® SMB-B5

R&S® SMB-B25/-B26

R&S® SMB-B30/-B31/-B32

R&S® SMB-B101/-B102/-B103/-B106

R&S® SMB-B112/-B112L/-B120/-B120L

R&S® SMB-B131

R&S® SMB-B140/-B140L/-B140N

R&S® SMB-K21/-K22

R&S® SMB-K23/-K27

This manual describes firmware version FW 5.00.116.xx and later of the R&S® SMB100A.

## Contents

1 Safety and Regulatory Information ..... 13    
1.1 Safety instructions ..... 13    
1.2 Labels on R&S SMB ..... 15    
1.3 Warning Messages in the Documentation ..... 16    
1.4 Korea certification class B ..... 16    
2 Welcome to the R&S SMB ..... 17    
2.1 Key Features ..... 17    
2.2 Documentation overview ..... 17    
2.2.1 User manual and help ..... 17    
2.2.2 Data sheets and brochures ..... 17    
2.2.3 Release notes and open source acknowledgment (OSA) ..... 18    
2.2.4 Application notes, application cards, white papers, etc ..... 18    
2.3 Typographical conventions ..... 18    
2.4 Notes on screenshots ..... 18    
3 Getting Started ..... 20    
3.1 Preparing for Use ..... 20    
3.1.1 Lifting and Carrying ..... 20    
3.1.2 Unpacking and Checking ..... 20    
3.1.3 Choosing the Operating Site ..... 20    
3.1.4 Setting Up the R&S SMB ..... 21    
3.1.4.1 Placing the R&S SMB on a Bench Top ..... 21    
3.1.4.2 Mounting the R&S SMB in a Rack ..... 22    
3.1.5 Considerations for Test Setup ..... 23    
3.1.6 Connecting to Power ..... 24    
3.1.7 Connecting to LAN ..... 24    
3.1.8 Connecting to IEC 625/IEEE 488 (GPIB) ..... 25    
3.1.9 Connecting USB Devices ..... 25    
3.1.10 Connecting to RF ..... 26    
3.1.11 Connecting to Ref In/Ref Out ..... 27    
3.1.12 Switching On or Off ..... 27    
3.1.13 Default Settings ..... 29

3.1.14 Working with Linux Operating System ..... 30    
3.1.15 Setting Up a Network (LAN) Connection ..... 30    
3.1.15.1 Assigning the IP Address ..... 30    
3.1.15.2 Using Computer Names ..... 31    
3.1.16 Remote Access via an External Controller ..... 32    
3.1.16.1 Using a Web Browser for Remote Access ..... 33    
3.1.16.2 Remote Access via a VNC Client Software ..... 34    
3.1.17 LXI Configuration ..... 39    
3.1.17.1 LXI Browser Settings ..... 40    
3.1.17.2 LAN Configuration ..... 42    
3.2 Instrument Tour ..... 48    
3.2.1 Front Panel Tour ..... 48    
3.2.1.1 Utility Keys ..... 49    
3.2.1.2 Standby LEDs and Standby Key ..... 49    
3.2.1.3 Display ..... 50    
3.2.1.4 Setup Keys ..... 50    
3.2.1.5 Keypad for data entry ..... 51    
3.2.1.6 Rotary Knob and Navigation Keys ..... 52    
3.2.1.7 Front Panel Connectors ..... 53    
3.2.2 Rear Panel Tour ..... 54    
3.2.2.1 Description of the Connectors ..... 55    
3.3 Trying out the Instrument ..... 57    
3.4 System Overview ..... 62    
3.4.1 Brief Introduction to the Instrument's Concept ..... 62    
3.4.2 Signal Flow at a Glance ..... 62    
3.4.3 Application Field of the Instrument ..... 63    
3.4.4 Description of Individual Diagram Blocks ..... 63    
3.5 Instrument Control ..... 66    
3.5.1 Manual Operation ..... 66    
3.5.1.1 Legend of Manual Controls ..... 66    
3.5.1.2 Key Elements ..... 69    
3.5.1.3 Display ..... 71    
3.5.1.4 Accessing Dialogs ..... 76

3.5.1.5 Setting Parameters.....77    
3.5.1.6 Editors.....83    
3.5.1.7 How to Use the Help System.....85    
3.5.1.8 File Management.....86    
3.5.1.9 Legend of Front-Panel Controls.....90    
3.5.2 Remote Control.....92    
3.5.3 Remote Access.....92    
4 Instrument Function.....94    
4.1 Overview of Instrument Functions.....94    
4.2 General Instrument Settings.....95    
4.2.1 Overview of General Instrument Settings.....95    
4.2.2 Default Instrument Settings - Preset Key.....96    
4.2.3 General Configuration of Instrument - Setup Key.....97    
4.2.3.1 Hardware Config.....97    
4.2.3.2 Software / Options.....98    
4.2.3.3 Manage License Keys.....100    
4.2.3.4 NRP Info/Update.....101    
4.2.3.5 SMZ Info Update.....103    
4.2.3.6 Display Update.....103    
4.2.3.7 Shutting Down and Rebooting the Instrument.....104    
4.2.3.8 Network Settings.....104    
4.2.3.9 Display/Keyboard Settings.....108    
4.2.3.10 Remote Channel Settings.....109    
4.2.3.11 Instrument Emulations.....110    
4.2.3.12 LXI Status.....112    
4.2.3.13 Protection.....113    
4.2.3.14 Security.....114    
4.2.3.15 Save/Recall.....122    
4.2.3.16 Factory Preset.....122    
4.2.3.17 Help.....122    
4.2.4 Switching to Manual Control - Local Key.....123    
4.2.5 Generating a Hard Copy of the Display.....123    
4.2.5.1 Hard Copy Settings.....123

4.2.5.2 Hard Copy Options ..... 124    
4.2.6 Messages - Info Key ..... 126    
4.2.7 Help System - Help Key ..... 126    
4.2.8 Storing and Loading Instrument Data - File Key ..... 127    
4.2.8.1 Save/Recall Menu ..... 128    
4.2.8.2 Saving Instrument Settings ..... 128    
4.2.8.3 Loading Instrument Settings ..... 130    
4.2.8.4 File Manager ..... 132    
4.2.8.5 Accessing the File System of the Instrument and Transferring Files from and to the Instrument ..... 133    
4.3 RF Block ..... 136    
4.3.1 Overview of RF Signal ..... 136    
4.3.1.1 RF Output ..... 136    
4.3.1.2 RF Signal Modes and Characteristics ..... 137    
4.3.2 RF Frequency ..... 138    
4.3.2.1 RF Frequency vs. RF Output Frequency ..... 138    
4.3.2.2 Setting the RF Frequency ..... 139    
4.3.2.3 RF Frequency Dialog ..... 139    
4.3.2.4 Frequency Settings ..... 140    
4.3.2.5 User Variation Settings ..... 141    
4.3.3 Phase ..... 142    
4.3.3.1 Phase Settings ..... 142    
4.3.4 Reference Oscillator ..... 142    
4.3.4.1 Reference Oscillator Settings ..... 144    
4.3.5 RF Level ..... 146    
4.3.5.1 Overview of RF Level ..... 146    
4.3.5.2 RF Level Dialog ..... 148    
4.3.5.3 Power-On/EMF Settings ..... 153    
4.3.5.4 Automatic Level Control - ALC ..... 153    
4.3.5.5 NRP Level Control ..... 155    
4.3.5.6 User Correction ..... 159    
4.3.5.7 Reverse Power Protection ..... 167    
4.3.6 RF Measurement ..... 167    
4.3.6.1 NRP Sensor Mapping ..... 167

4.3.6.2 NRP Power Viewer ..... 170    
4.3.7 RF Sweep and List Mode ..... 178    
4.3.7.1 Overview ..... 178    
4.3.7.2 RF Frequency Sweep ..... 179    
4.3.7.3 RF Level Sweep ..... 187    
4.3.7.4 List Mode ..... 192    
4.4 Modulation ..... 201    
4.4.1 Overview of Modulation ..... 201    
4.4.1.1 Enabling/Disabling Analog Modulations using the MOD On/Off Key ..... 202    
4.4.1.2 Modulation Sources ..... 202    
4.4.2 Amplitude Modulation (AM) ..... 204    
4.4.2.1 Amplitude Modulation Settings ..... 204    
4.4.3 Frequency Modulation (FM) ..... 207    
4.4.3.1 Frequency Modulation Settings ..... 208    
4.4.4 Phase Modulation (PhiM) ..... 211    
4.4.4.1 Phase Modulation Dialog ..... 211    
4.4.5 Pulse Modulation (PM) ..... 214    
4.4.5.1 Pulse Modulation Settings ..... 215    
4.4.6 Stereo Modulation ..... 217    
4.4.6.1 Stereo Modulation Dialog ..... 217    
4.5 Modulation Generator and LF Output ..... 223    
4.5.1 Overview of LF Generator ..... 223    
4.5.2 LF Output ..... 224    
4.5.2.1 LF Output Dialog ..... 224    
4.5.3 LF Frequency Sweep ..... 225    
4.5.4 Pulse Generator ..... 231    
4.5.4.1 Pulse Generator Settings ..... 231    
4.5.4.2 Pulse Train Generation ..... 235    
5 Remote Control Basics ..... 240    
5.1 Remote Control Interfaces and Protocols ..... 240    
5.1.1 VISA Libraries ..... 241    
5.1.2 Messages ..... 241    
5.1.3 LAN Interface ..... 242

5.1.3.1 HiSLIP protocol ..... 244    
5.1.3.2 VXI-11 protocol ..... 244    
5.1.3.3 Socket communication ..... 244    
5.1.3.4 LAN interface messages ..... 245    
5.1.4 USB Interface ..... 245    
5.1.5 Serial Interface ..... 246    
5.1.6 GPIB Interface (IEC/IEEE Bus Interface) ..... 246    
5.1.6.1 GPIB interface messages ..... 247    
5.1.6.2 GPIB Instrument Address ..... 248    
5.1.7 LXI Browser Interface ..... 248    
5.2 Starting a Remote Control Session ..... 249    
5.2.1 Switching to Remote Control ..... 249    
5.2.2 Returning to Manual Operation ..... 250    
5.2.3 Examples ..... 250    
5.2.3.1 Remote Control over GPIB ..... 251    
5.2.3.2 Remote Control over LAN using VXI-11 Protocol ..... 251    
5.2.3.3 Remote Control over LAN using Socket Communication ..... 256    
5.3 SCPI command structure ..... 263    
5.3.1 Syntax for common commands ..... 263    
5.3.2 Syntax for instrument-specific commands ..... 264    
5.3.2.1 Long and short form ..... 264    
5.3.2.2 Numeric suffixes ..... 264    
5.3.2.3 Optional mnemonics ..... 265    
5.3.3 SCPI parameters ..... 265    
5.3.3.1 Numeric values ..... 266    
5.3.3.2 Special numeric values ..... 266    
5.3.3.3 Boolean parameters ..... 267    
5.3.3.4 Text parameters ..... 267    
5.3.3.5 Character strings ..... 268    
5.3.3.6 Block data ..... 268    
5.3.4 Overview of syntax elements ..... 268    
5.3.5 Structure of a command line ..... 269    
5.3.6 Responses to queries ..... 270

5.4 Command Sequence and Synchronization ..... 271    
5.4.1 Preventing overlapping execution ..... 272    
5.5 Status reporting system ..... 273    
5.5.1 Hierarchy of the status registers ..... 273    
5.5.2 Structure of a SCPI status register ..... 275    
5.5.3 Status byte (STB) and service request enable register (SRE) ..... 277    
5.5.4 Event status register (ESR) and event status enable register (ESE) ..... 278    
5.5.5 Questionable status register (STATUS:QUEStionable) ..... 278    
5.5.6 Operation status register (STATUS:OPERATION) ..... 279    
5.5.7 Application of the status reporting system ..... 279    
5.5.7.1 Service request ..... 279    
5.5.7.2 Serial poll ..... 280    
5.5.7.3 Query of an instrument status ..... 280    
5.5.7.4 Error queue ..... 280    
5.5.8 Reset values of the status reporting system ..... 281    
5.6 General programming recommendations ..... 281    
6 Remote Control Commands ..... 283    
6.1 Conventions used in SCPI command descriptions ..... 283    
6.2 Common commands ..... 284    
6.3 Preset Commands ..... 288    
6.4 CALibration Subsystem ..... 290    
6.5 DIAGnostic Subsystem ..... 292    
6.6 DISPlay Subsystem ..... 294    
6.7 FORMAT Subsystem ..... 297    
6.8 HCOPy Subsystem ..... 299    
6.9 KBOard Subsystem ..... 304    
6.10 MMEMory subsystem ..... 305    
6.10.1 File naming conventions ..... 305    
6.10.2 Extensions for user files ..... 306    
6.10.3 Examples ..... 306    
6.10.4 Remote control commands ..... 308    
6.11 OUTPUT Subsystem ..... 314    
6.12 SENSE, READ and INITiate Subsystems ..... 318

6.13 SOURce Subsystem.....330    
6.13.1 SOURce:AM Subsystem.....331    
6.13.2 SOURce:CORRection subsystem.....334    
6.13.3 SOURce:FM Subsystem.....343    
6.13.4 SOURce:FREQUency Subsystem.....346    
6.13.5 SOURce:INPUT Subsystem.....353    
6.13.6 SOURce:LFOutput Subsystem.....354    
6.13.7 SOURce:LIST Subsystem.....364    
6.13.8 SOURce:MODulation Subsystem.....376    
6.13.9 SOURce:PGEN Subsystem.....377    
6.13.10 SOURce:PHASE Subsystem.....378    
6.13.11 SOURce:PM Subsystem.....378    
6.13.12 SOURce:POWER Subsystem.....382    
6.13.13 SOURce:PULM Subsystem.....392    
6.13.14 SOURce:ROSCillator Subsystem.....409    
6.13.15 SOURce:STEReo Subsystem.....412    
6.13.16 SOURce:SWEep Subsystem.....421    
6.14 STATus subsystem.....433    
6.15 SYSTEM Subsystem.....437    
6.16 TEST Subsystem.....456    
6.17 TRIGger Subsystem.....457    
6.18 UNIT subsystem.....463    
6.19 Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5.....464    
6.19.1 Remote-Control Commands.....464    
6.19.2 Examples.....485    
6.19.2.1 Alternative Frequency Lists.....485    
6.19.2.2 Enhanced Other Networks.....486    
6.19.2.3 Free Format Groups (FFGs).....487    
6.19.2.4 Transparent-Mode.....488    
7 Transporting.....489    
8 Maintenance, Storage and Disposal.....490    
8.1 Cleaning.....490    
8.2 Changing Fuses.....490

8.3 Storage.....491    
8.4 Performing Maintenance Tasks.....491    
8.4.1 Date and Time.....491    
8.4.2 Check Front Panel.....492    
8.4.2.1 Check Front Panel Settings.....493    
8.4.2.2 Performing the Front Panel Tests.....493    
8.4.3 Internal Adjustments.....494    
8.4.3.1 Internal Adjustments Settings.....495    
8.4.3.2 Performing Internal Adjustments.....496    
8.4.4 Selftest.....497    
8.5 Disposal.....498    
9 Status Information, Error Messages and Troubleshooting.....499    
9.1 Status Information.....499    
9.1.1 Status information displayed between the frequency and level fields.....499    
9.1.2 Status information displayed to the left of the Info line.....501    
9.1.3 Status information displayed in the Info line.....501    
9.2 Error Messages.....502    
9.2.1 Volatile messages.....502    
9.2.2 Permanent messages.....503    
9.3 SCPI-Error Messages.....503    
9.4 Device-Specific Error Messages.....503    
9.5 Resolving network connection failures.....505    
9.6 Measuring USB Cable Quality.....506    
9.7 Obtaining Technical Support.....506    
9.8 Contacting customer support.....507    
Annex.....508    
A Hardware Interfaces.....508    
A.1 GPIB Bus Interface.....508    
List of commands.....510    
Index.....520


---

## Split File Navigation

> Files are split by top-level chapters; Chapter 6 is further split by SCPI subsystem. Content is unmodified.

| 文件 | 章节 | 行数 | 内容概述 |
|------|------|------|----------|
| `01_safety.md` | 1 | ~146 | Safety and Regulatory Information：安全说明、标签、警告信息、认证 |
| `02_welcome.md` | 2 | ~70 | Welcome to the R&S SMB：关键特性、文档概览、排版约定 |
| `03_getting_started.md` | 3 | ~4,070 | Getting Started：准备工作、搬运/安装/连接、开关机、默认设置、LAN/VNC/LXI 配置、仪器导览、试用、系统概述、仪器控制 |
| `04_instrument_function.md` | 4 | ~9,560 | Instrument Function：通用设置、RF 模块（频率/相位/参考振荡器/电平）、调制、调制发生器与 LF 输出 |
| `05_remote_control_basics.md` | 5 | ~2,002 | Remote Control Basics：远程控制接口与协议、建立会话、SCPI 命令结构、命令序列与同步、状态报告系统、编程建议 |
| `06a_common_commands.md` | 6.1–6.2 | ~254 | Remote Control Commands — 前言 + Common commands：SCPI 约定、通用命令 |
| `06b_preset_commands.md` | 6.3 | ~84 | Preset Commands |
| `06c_calibration_subsystem.md` | 6.4 | ~146 | CALibration Subsystem |
| `06d_diagnostic_subsystem.md` | 6.5 | ~108 | DIAGnostic Subsystem |
| `06e_display_subsystem.md` | 6.6 | ~167 | DISPlay Subsystem |
| `06f_format_subsystem.md` | 6.7 | ~86 | FORMat Subsystem |
| `06g_hcopy_subsystem.md` | 6.8 | ~240 | HCOPy Subsystem |
| `06h_keyboard_subsystem.md` | 6.9 | ~46 | KBOard Subsystem |
| `06i_memory_subsystem.md` | 6.10 | ~437 | MMEMory subsystem |
| `06j_output_subsystem.md` | 6.11 | ~214 | OUTPUT Subsystem |
| `06k_sense_read_init_subsystems.md` | 6.12 | ~701 | SENSE, READ and INITiate Subsystems |
| `06l_source_subsystem.md` | 6.13 | ~5,716 | SOURCE Subsystem：RF 频率/电平/调制源、扫描、LIST 模式等（本章最大） |
| `06m_status_subsystem.md` | 6.14 | ~192 | STATus subsystem |
| `06n_system_subsystem.md` | 6.15 | ~1,075 | SYSTEM Subsystem |
| `06o_test_subsystem.md` | 6.16 | ~1,730 | TEST Subsystem |
| `07_transporting.md` | 7 | ~24 | Transporting：搬运注意事项 |
| `08_maintenance.md` | 8 | ~439 | Maintenance, Storage and Disposal：维护、存储、报废、内部校准 |
| `09_status_info.md` | 9 | ~2,508 | Status Information, Error Messages and Troubleshooting：状态信息、错误消息、故障排除 |
