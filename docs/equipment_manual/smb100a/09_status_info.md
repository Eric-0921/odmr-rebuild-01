## 9 Status Information, Error Messages and Troubleshooting

The R&S SMB distinguishes between a variety of different messages such as status messages, error messages, warnings, or information that are displayed in the "Info" line on the screen, and also entered in the error/event queue of the status reporting system.

This section describes the information and status messages concerning the operating status of the instrument and the types of error messages and warnings. Some error messages require that the error must be eliminated before correct instrument operation can be ensured. The info window with a list of current messages and a detailed description of each message can be opened with the [INFO] key.

In the remote control mode, error messages are entered in the error/event queue of the status reporting system and can be queried with the command SYSTEM:ERROR?. If the error queue is empty, 0 ("No error") is returned. The status reporting system is described in detail in Chapter 5.5, "Status reporting system", on page 273.

Section Chapter 9.5, "Resolving network connection failures", on page 505 provides recommended solutions for network connection errors, and helps you to collect the information required for quick and efficient support.

### 9.1 Status Information

The status messages are displayed in the header section of the screen. The status information gives the user an overview of the main operating states and settings of the instrument. The states are indicated for information only and do not necessitate any action by the user. Status information is displayed between the frequency and level fields, at the left of the info line or in the info line itself.

#### 9.1.1 Status information displayed between the frequency and level fields

This section gives an overview of the status messages displayed between the frequency and level fields.

##### RF OFF

The RF output is switched off

##### MOD OFF

All modulations are switched off

##### FREQ OFFSET

A frequency offset is set.

The frequency entered and displayed in the "Frequency" field takes any set frequency offset into consideration, e.g. an offset set for a downstream instrument. This means that with a frequency offset the frequency displayed in the header does not correspond to the frequency at the RF output, but rather to the frequency at the output of the downstream instrument.

This allows the target frequency at the output of a downstream instrument to be entered in the frequency field. The signal generator changes the RF output frequency according to the entered offset.

However, the frequency entered and displayed in the "Frequency/Phase" dialog of the "RF" function block always corresponds to the RF output frequency. Any frequency offset is not taken into consideration.

The correlation is as follows:

Freq in header = RF output frequency (= Freq in dialog) + Freq offset (= Offset in dialog)

##### OVERLOAD

The power of the external signal applied to the RF output is too high. The overload protection is tripped and the connection between the RF output and attenuator is interrupted. The overload protection is reset by pressing the [RF ON/OFF] key. The RF input is activated when the overload protection is reset.

##### LEVEL OFFSET

A level offset is set.

The level entered and displayed in the "Level" field takes the offset of any downstream attenuators/amplifiers into consideration by way of calculation. This means that with a level offset the level displayed in the header does not correspond to the level at the RF output, but rather to the level at the output of the downstream instrument.

This allows the target level at the output of downstream instruments to be entered. The signal generator changes the RF output level according to the set offset.

However, the level entered and displayed in the "Level" dialog of the "RF" function block always corresponds to the RF output level. Any level offset is not taken into consideration.

The correlation is as follows:

Level in header = RF output level (= Level in dialog) + Level offset

##### EXT REF

An external reference is used.

The external signal with selectable frequency and defined level must be input at the [REF IN] connector. It is output at the [REF OUT] connector.

##### BUSY

A setting or calculation is executed.

#### 9.1.2 Status information displayed to the left of the Info line

This section gives an overview of the status messages displayed to the left of the Info line.

##### REMOTE

Indicates that the instrument is in remote control mode.

The keys on the front panel are usable, but all parameters are in read only mode.

To return to manual control, use the [local] key or the command >L. The current command must be fully processed before the mode is switched, otherwise the instrument switches immediately back to remote control.

##### REM-LLO

Indicates that the instrument is in remote control mode with local lockout enabled.

The [local] key is locked. To set the local lockout, use the command &LLO (local lockout).

The keys on the front panel are usable, but all parameters are in read only mode.

To return to manual operation or to "REMOTE" state, use one of the following commands:

• &LOCS

swichtes directly from "REM-LLO" to manual operation.

☑ &REMS

changes the remote control state from "REM-LLO" to "REMOTE".

- CALL IBLOC (generator%) (Visual Basic command) switches from remote control state to manual operation.

##### LOC-LLO

For the direct operation the state has been changed from remote control to manual operation (local state). The [LOCAL] key was disabled with the command LLO (local lockout).

With the next activating of the remote control mode, the instrument cannot be switched to manual operation by the operator. The status information changes to "REM-LLO".

The instrument can be switched to manual operation by means of remote control only (e.g. with the Visual Basic command CALL IBLOC (generator)).

#### 9.1.3 Status information displayed in the Info line

This section gives an overview of the status messages displayed in the Info line.

##### RFSweep / LevelSweep / LFSweep

The indicated sweep is enabled.

##### ALC On / Auto / S&H

The status of the automatic level control is indicated:

• ON

automatic level control permanently on

##### Auto

automatic level control is automatically adapted to the operating states

• S&H

automatic level control off, recalibration of the level whenever the level or frequency is set (sample and hold mode)

##### ListMode

List mode is active.

The values of the frequency/level pairs in the selected list are set for the chosen dwell time.

##### AttFixed

Attenuator fixed mode is active.

The uninterrupted level settings are made in a fixed range without attenuator switching. The variation range is set automatically when this mode is activated. The range is displayed under "Attenuator Fixed Range" in the "Level" dialog.

##### UCorr

User Correction is active.

The level is corrected by the given values in the selected user correction list. Correction is performed by the user-defined list values being added to the output level for the respective RF frequency. With frequencies which are not contained in the list, the level correction is determined by interpolation of the closest correction values.

##### OvenCold

The reference oscillator has not yet reached its nominal frequency.

When switching on from the STANDBY mode, the specified frequency accuracy is reached immediately. If the power switch was switched off, the reference oscillator needs some warm-up time to reach its nominal frequency. During this period of time, the output frequency does not yet reach its final value either.

### 9.2 Error Messages

Messages indicate errors in the instrument. They are displayed in the info line in different colors depending on their importance and display duration. Errors (e.g. no calibration data) are displayed in red, information (e.g. file not found) and warnings in black. Warnings indicate less significant errors (e.g. the instrument operates outside specified data).

#### 9.2.1 Volatile messages

Volatile messages report automatic settings in the instrument (e.g. switching off of incompatible types of modulation) or on illegal entries that are not accepted by the instrument (e.g. range violations). They are displayed in the info line on a yellow background. They are displayed on top of status information or permanent messages.

Volatile messages do not normally demand user actions and disappear automatically after a brief period of time. They are stored in the history, however.

SCPI command: :SYSTEM:ERROR:ALL? or :SYSTEM:ERROR[:NEXT] ?

Permanent messages are displayed if an error occurs that impairs further instrument operation, e.g. a hardware fault. The error signaled by a permanent message must be eliminated before correct instrument operation can be ensured.

#### 9.2.2 Permanent messages

The message is displayed until the error is eliminated. It covers the status display in the info line. After error elimination, the message automatically disappears and is also recorded in the history.

SCPI command::SYSTEM:ERROR:STATIC?

### 9.3 SCPI-Error Messages

The SCPI error messages are the same in all SCPI instruments. Detailed information and an overview of all error messages as defined in SCPI standard can be found in the corresponding documentation.

The errors are assigned negative numbers. The error text being entered into the error/event queue or being displayed is printed in bold face on the left together with the error code. Below the error text, there is an explanation as to the respective error.

### 9.4 Device-Specific Error Messages

The following table contains all error messages specific for the instrument in alphabetical order, as well as an explanation of the error situation. The positive error codes mark the errors specific of the instrument.

The device-specific error messages set bit 3 in the ESR register.


The index provides a list of the error messages sorted according to their error codes.


| Error Code | Error | Description | Remedy |
|---|---|---|---|
| 50 | External reference out of range or disconnected | External reference is selected but no external signal is applied or the signal is out of range. | · Check the selected reference signal source (internal or external) in the "Setup > Reference Oscillator" dialog. · Change setting to 'internal' if no appropriate external source is available. |
| 140 | This modulation forces other modulations off | A modulation has been switched on which cannot be used at the same time as an already active modulation. The previous modulation has been switched off. Example: Enabling FM modulation switches PM modulation off. |  |
| 180 | Adjustment failed | Adjustment could not be executed | The adjustment data have to be generated first by an internal or external adjustment or to be loaded into the device. See Chapter 8.4.3, "Internal Adjustments", on page 494. |
| 182 | Adjustment data missing | Adjustment data are missing. | The adjustment data have to be generated first by an internal or external adjustment or to be loaded into the instrument. |
| 183 | Adjustment data invalid | Adjustment data are invalid and must be restored. | The adjustment data have to be generated again by an internal or external adjustment or to be loaded into the instrument. |
| 200 | Cannot access hardware | The data transmission to a module was unsuccessful. | The module is not installed, not properly installed or missing. |
| 201 | Hardware revision out of date | A later version of certain parts of the instrument is necessary to execute the function selected. | The driver does not support the installed version of a module. |
| 202 | Cannot access the EEPROM | A error occurs when writing or reading a EEPROM. | The EEPROM might be defect and has to be replaced. |
| 203 | Invalid EEPROM data | Reading a EEPROM is possible, however the data are inconsistent. |  |
| 204 | Driver initialization failed | Initialization of a driver fails when booting the instrument firmware. | The driver is not compatible with the hardware or software configuration of the instrument. |
| 241 | No current list | There is no list selected. To execute the required operation, a list has to be selected in the related menu. | If no list is available, a new list must be created. |
| 242 | Unknown list type specified | The list type selected is not valid for the required operation. For instance, the file extension for waveform list files is *.wv. It is not possible to enter another file extension when selecting a list. | Check the selected list type. |
| 460 | Cannot open file | The selected file can not be opened. | Check the path and file name. |
| 461 | Cannot write file | The file can not be written. | Check if the file is read-only. |
| 462 | Cannot read file | The file can not be read. | Check if the file contents are compatible with the file type. |
| 463 | Filename missing | The required operation cannot be executed because the file name is not specified. | A file name has to be entered when creating a new list. |
| 464 | Invalid filename extension | The file extension is not valid for the required operation. | Check the file extension. For instance, the file extension for waveform list files is *.wv. It is not possible to enter another file extension when storing a list. |
| 465 | File contains invalid data | The selected file contains data that is not valid for the file type. The file extension determines the data that is valid for this file type. If the file extension is changed the lists are no longer recognized and the data are therefore invalid. Example: the extension of a waveform file (= *.wv) was changed to *.txt. | Check the file extension. |


##### Stereocoder error messages

An error occurred concerning the stereo coder option R&S SMB-B5. Refer to the service manual, chapter 3, section "Troubleshooting" for error correction.

### 9.5 Resolving network connection failures

Several issues may cause failures in the network connection to the instrument. This section lists the most likely reasons and the recommended solutions.

##### Common reasons for network connection failures

• Network connecting cables and cable connectors of poor quality

- Incompatibility between the network interface of the R&S SMB and certain switches or routers available on the market

• An invalid IP address assigned to the instrument

##### Possible solutions to network connection failures

1. NOTICE! Connecting to the network can cause network failure. Errors can affect the entire network.

Consult your network administrator before performing the following tasks:

• Connecting the instrument to the network

• Configuring the network

• Changing IP addresses

2. Try out the following to resolve network connection failures:

- Check the network infrastructure. Exchange connecting cables, if obvious damage is visible.

- Observe the link status LED on the R&S SMB or the connected network device. The link status LED is located next to the LAN connector. If a link failure is detected, connect the instrument to a different device port or to a different network device.

- Check whether the LAN interface and the required LAN services are enabled. See "LAN Services" on page 117.

- If the IP address is set manually (no DHCP) or obtained via the Zeroconf (APIPA) protocol:

– Check whether the IP address of the instrument is within the network's address range.

- Check whether the IP address is valid.

See "IP Address" on page 106.

### 9.6 Measuring USB Cable Quality

To check the quality of the USB cable, see the service manual of the R&S SMB.

### 9.7 Obtaining Technical Support

If you encounter problems that you cannot solve yourself, contact your Rohde & Schwarz support center as listed at www.rohde-schwarz.com/support. Our support center staff is optimally trained to assist you in solving problems.

The support center finds solutions more quickly and efficiently if you provide them with information on the instrument and an error description.

- The following dialog boxes in the "Setup > System" menu provide useful information:

– Hardware Configuration: hardware assemblies

– Software/Options: the status of all software and hardware options installed on your instrument

- System Messages: displayed in the "Info" line and provide information on any errors that may have occurred.

See also the description of error messages Chapter 9, "Status Information, Error Messages and Troubleshooting", on page 499.

##### To collect error information

▶ Collect the error information and attach it to an email in which you describe the problem.

Send the email to the customer support address for your region as listed on the Internet (www.rohde-schwarz.com/support).

##### To remove sensitive data

For information on how to handle or remove the sensitive data from your instrument, refer to the description "Resolving Security Issues when working with R&S SMB".

##### Packing and transporting the instrument

If the instrument has to be transported or shipped, observe the notes described in Chapter 3.1.2, "Unpacking and Checking", on page 20, and Chapter 7, "Transporting", on page 489.

### 9.8 Contacting customer support

##### Technical support – where and when you need it

For quick, expert help with any Rohde & Schwarz product, contact our customer support center. A team of highly qualified engineers provides support and works with you to find a solution to your query on any aspect of the operation, programming or applications of Rohde & Schwarz products.

##### Contact information

Contact our customer support center at www.rohde-schwarz.com/support, or follow this QR code:


Figure 9-1: QR code to the Rohde & Schwarz support page


### Annex

### A Hardware Interfaces

This section covers hardware related topics, like pin assignment of the GPIB bus interface.

The remote control interfaces are described in details in Chapter 5, "Remote Control Basics", on page 240.

All other interfaces are described in sections "Legend of Front Planet" and "Legend of Rear Panel" in the Quick Start Guide.

For specifications refer to the data sheet.

### A.1 GPIB Bus Interface

Pin assignment


ATN IFC NRFD EOI D2 D0
GND SRO NDAC DAV D3 D1
GND(24) GND(22) GND(20) GND(18) D7 D5
GND(23) GND(21) GND(19) REN D6 D4


Figure A-1: Pin assignment of GPIB bus interface


##### Bus lines

• Data bus with 8 lines D0 to D7:

The transmission is bit-parallel and byte-serial in the ASCII/ISO code. D0 is the least significant bit, D7 the most significant bit.

• Control bus with five lines:

IFC (Interface Clear): active LOW resets the interfaces of the instruments connected to the default setting.

ATN (Attention): active LOW signals the transmission of interface messages, inactive HIGH signals the transmission of device messages.

SRQ (Service Request): active LOW enables the connected device to send a service request to the controller.

REN (Remote Enable): active LOW permits switchover to remote control.

EOI (End or Identify): has two functions in connection with ATN:

– ATN=HIGH active LOW marks the end of data transmission.

– ATN=LOW active LOW triggers a parallel poll.

• Handshake bus with three lines:

DAV (Data Valid): active LOW signals a valid data byte on the data bus.

NRFD (Not Ready For Data): active LOW signals that one of the connected devices is not ready for data transfer.

NDAC (Not Data Accepted): active LOW signals that the instrument connected is accepting the data on the data bus.

##### Interface Functions

Instruments which can be controlled via GPIB bus can be equipped with different interface functions. The interface function for the R&S SMB are listed in the following table.

Table A-1: GPIB bus interface functions


| Control character | Interface function |
|---|---|
| SH1 | Handshake source function (source handshake), full capability |
| AH1 | Handshake sink function (acceptor handshake), full capability |
| L4 | Listener function, full capability, de-addressed by MTA. |
| T6 | Talker function, full capability, ability to respond to serial poll, deadressed by MLA |
| SR1 | Service request function (Service Request), full capability |
| PP1 | Parallel poll function, full capability |
| RL1 | Remote/Local switch over function, full capability |
| DC1 | Reset function (Device Clear), full capability |
| DT1 | Trigger function (Device Trigger), full capability |

### List of commands

:CALibration:ALL[:MEASure]?..290
:CALibration:ROSCillator[:DATA]..292
:CALibration<hw>:FMOFFset[:MEASure]?..291
:CALibration<hw>:FREQUENCY[:MEASure]?..291
:CALibration<hw>:LEVEL:EXTern:DATA..292
:CALibration<hw>:LEVEL[:MEASure]?..291
:DEVICE:PRESet..288
:DIAGnostic:INFO:OTIMe?..294
:DIAGnostic:INFO:POCount?..294
:DIAGnostic<hw>:BGINfo:CATalog?..293
:DIAGnostic<hw>:BGINfo?..293
:DISPLAY:ANNotation:AMPLitude..295
:DISPLAY:ANNotation:FREQUency..295
:DISPLAY:ANNotation[:ALL]..295
:DISPLAY:DIALog:CLOSE..295
:DISPLAY:DIALog:CLOSE:ALL..296
:DISPLAY:DIALog:ID?..296
:DISPLAY:DIALog:OPEN..296
:DISPLAY:PSAVE:HOLDoff..296
:DISPLAY:PSAVE[:STATE]..296
:DISPLAY:UPDATE..297
:FORMAT:BORDER..297
:FORMAT:SREGister..298
:FORMAT[:DATA]..298
:HCOPY:DATA?..300
:HCOPY:DEVICE:LANGUAGE..300
:HCOPY:FILE[:NAME]..301
:HCOPY:FILE[:NAME]:AUTO:DIRectory..301
:HCOPY:FILE[:NAME]:AUTO:DIRectory:CLEAR..302
:HCOPY:FILE[:NAME]:AUTO:FILE?..302
:HCOPY:FILE[:NAME]:AUTO:STATE..303
:HCOPY:FILE[:NAME]:AUTO?..301
:HCOPY:FILE[:NAME]:AUTO:FILE]:DAY:STATE..302
:HCOPY:FILE[:NAME]:AUTO:FILE]:MONTH:STATE..302
:HCOPY:FILE[:NAME]:AUTO:FILE]:NUMBER?..302
:HCOPY:FILE[:NAME]:AUTO:FILE]:PREFix..303
:HCOPY:FILE[:NAME]:AUTO:FILE]:PREFix:STATE..303
:HCOPY:FILE[:NAME]:AUTO:FILE]:YEAR:STATE..302
:HCOPY:IMAGE:FORMAT..300
:HCOPY:REGION..303
:HCOPY[:EXECute]..301
:INITiate<hw>:POWER]:CONTINUOUS..322
:KBOARD:LANGUAGE..304
:KBOARD:LAYOUT..304
:MEMORY:HFRee?..311
:MMEMORY:CATalog:LENGTH?..308
:MMEMORY:CATalog?..308

:MMEMory:CDIRectory..309
:MMEMory:COPY..309
:MMEMory:DATA..310
:MMEMory:DCATalog:LENGTH?..311
:MMEMory:DCATalog?..310
:MMEMory:DELete..311
:MMEMory:LOAD:STATe..312
:MMEMory:MDIRectory..312
:MMEMory:MOVE..312
:MMEMory:MSIS..313
:MMEMory:RDIRectory..313
:MMEMory:STORE:STATE..313
:OUTPUT<hw>:AFIXed:RANGE:LOWER?..314
:OUTPUT<hw>:AFIXed:RANGE:UPPER?..314
:OUTPUT<hw>:ALC:SEARCH:MODE..315
:OUTPUT<hw>:AMODE..315
:OUTPUT<hw>:FILTer:AUTO..315
:OUTPUT<hw>:FILTer[:LPASs]:STATE..316
:OUTPUT<hw>:IMPedance?..316
:OUTPUT<hw>:PROTection:CLEAR..316
:OUTPUT<hw>:PROTection:TRIPped?..316
:OUTPUT<hw>[:STATE]..317
:OUTPUT<hw>[:STATE]:PON..317
:READ<ch>[:POWER]:..322
:SENSE<ch>:UNIT[:POWER]..330
:SENSE<ch>[:POWER]:APERture:DEFAULT:STATE..321
:SENSE<ch>[:POWER]:APERture:TIME..321
:SENSE<ch>[:POWER]:CORRection:SPDevice:LIST?..323
:SENSE<ch>[:POWER]:CORRection:SPDevice:SELECT..323
:SENSE<ch>[:POWER]:CORRection:SPDevice:STATE..323
:SENSE<ch>[:POWER]:DISPLAY:PERManent:PRIORITY..324
:SENSE<ch>[:POWER]:DISPLAY:PERManent:STATE..324
:SENSE<ch>[:POWER]:FILTer:LENGTH:AUTO?..324
:SENSE<ch>[:POWER]:FILTer:LENGTH:[:USER]..325
:SENSE<ch>[:POWER]:FILTer:NSRatio..325
:SENSE<ch>[:POWER]:FILTer:NSRatio:MTIME..325
:SENSE<ch>[:POWER]:FILTer:SONCe..326
:SENSE<ch>[:POWER]:FILTer:TYPE..326
:SENSE<ch>[:POWER]:FREQUency..327
:SENSE<ch>[:POWER]:LOGGing:STATE..327
:SENSE<ch>[:POWER]:OFFSET..327
:SENSE<ch>[:POWER]:OFFSET:STATE..328
:SENSE<ch>[:POWER]:SOURce..328
:SENSE<ch>[:POWER]:ZERO..330
:SLISt:CLEAR:LAN..319
:SLISt:CLEAR:USB..319
:SLISt:CLEAR:[ALL]..319
:SLISt:ELEMENT<ch>:MAPPING..319
:SLISt:SCAN:LSENsor..319
:SLISt:SCAN:USENsor..320

:SLiSt:SCAN[:STATe]..320
:SLiSt:SENSOR:MAP..320
:SLiSt[:LIST]?..321
:SOURCE<hw>:PRESet..289
:STATUS:OPERATION:CONDITION?..434
:STATUS:OPERATION:ENABLE..434
:STATUS:OPERATION:NTRansition..434
:STATUS:OPERATION:PTRansition..435
:STATUS:OPERATION[:EVENT]..434
:STATUS:PRESet..435
:STATUS:QUESTionable:CONDITION..435
:STATUS:QUESTionable:ENABLE..435
:STATUS:QUESTionable:NTRansition..436
:STATUS:QUESTionable:PTRansition..436
:STATUS:QUESTionable[:EVENT]..436
:STATUS:QUEue[:NEXT]?..436
:SYSTEM:COMMunicate:GPIB:LTERminator..444
:SYSTEM:COMMunicate:GPIB:RESOURCE?..447
:SYSTEM:COMMunicate:GPIB[:SELF]:ADDRess..444
:SYSTEM:COMMunicate:HISLip:RESOURCE?..448
:SYSTEM:COMMunicate:NETWORK:IPADDRESS..446
:SYSTEM:COMMunicate:NETWORK:MACADDRESS..446
:SYSTEM:COMMunicate:NETWORK:RESOURCE?..448
:SYSTEM:COMMunicate:NETWORK:RESTART..447
:SYSTEM:COMMunicate:NETWORK:STATUS?..447
:SYSTEM:COMMunicate:NETWORK[:COMMON]:DOMAIN..444
:SYSTEM:COMMunicate:NETWORK[:COMMON]:HOSTName..445
:SYSTEM:COMMunicate:NETWORK[:COMMON]:WORKGROUP..445
:SYSTEM:COMMunicate:NETWORK[:IPADDRESS]:DNS..445
:SYSTEM:COMMunicate:NETWORK[:IPADDRESS]:GATeway..446
:SYSTEM:COMMunicate:NETWORK[:IPADDRESS]:SUBNet:MASK..446
:SYSTEM:COMMunicate:SERIAL:BAUD..449
:SYSTEM:COMMunicate:SERIAL:PARity..449
:SYSTEM:COMMunicate:SERIAL:RESOURCE?..448
:SYSTEM:COMMunicate:SERIAL:SBITs..449
:SYSTEM:COMMunicate:SOCKET:RESOURCE?..450
:SYSTEM:COMMunicate:USB:RESOURCE?..448
:SYSTEM:DATE..454
:SYSTEM:DISPLAY:UPDATE..454
:SYSTEM:CLOCK..442
:SYSTEM:ERROR:ALL?..438
:SYSTEM:ERROR:CODE:ALL?..439
:SYSTEM:ERROR:CODE[:NEXT]?..439
:SYSTEM:ERROR:COUNT?..440
:SYSTEM:ERROR:HISTORY:CLEAR..441
:SYSTEM:ERROR:HISTORY?..440
:SYSTEM:ERROR:STATIC?..441
:SYSTEM:ERROR[:NEXT]?..440
:SYSTEM:FPReset..289

:SYSTEM:HELP:EXPORT.....441
:SYSTEM:IDENTification.....450
:SYSTEM:IDENTification:PRESet.....450
:SYSTEM:IRESponse.....450
:SYSTEM:KLOCK.....442
:SYSTEM:LANGUAGE.....452
:SYSTEM:MMEMory:PATH:USER?.....456
:SYSTEM:NINFormation?.....447
:SYSTEM:NTP:HOSTname.....451
:SYSTEM:NTP:STATE.....451
:SYSTEM:ORESponse.....451
:SYSTEM:OSYStem?.....455
:SYSTEM:PRESet.....289
:SYSTEM:PROTECT<ch>[:STATE].....452
:SYSTEM:RCL.....443
:SYSTEM:REBoot.....453
:SYSTEM:RESTart.....453
:SYSTEM:SAV.....443
:SYSTEM:SECURITY:SUPolicy.....452
:SYSTEM:SECURITY:VOLMode[:STATE].....443
:SYSTEM:SHUTDOWN.....453
:SYSTEM:STARUp:COMPLETE?.....453
:SYSTEM:TIME.....454
:SYSTEM:TIME:ZONE.....455
:SYSTEM:TIME:ZONE:CATalog?.....455
:SYSTEM:ULOCK.....442
:SYSTEM:VERSION?.....455
:SYSTEM:WAIT.....456
:TEST<hw>:ALL:RESult?.....457
:TEST<hw>:ALL:START.....457
:TEST<hw>:DIRect.....457
:TRIGger<hw>:FSweep:SOURce.....458
:TRIGger<hw>:FSweep[:IMMediate].....459
:TRIGger<hw>:LFFSweep.....459
:TRIGger<hw>:LFFSweep:IMMediate.....460
:TRIGger<hw>:LFFSweep:SOURce.....460
:TRIGger<hw>:PSweep:SOURce.....461
:TRIGger<hw>:PSweep[:IMMediate].....461
:TRIGger<hw>[:IMMediate].....463
:TRIGger<hw>[:SWEep]:SOURce.....462
:TRIGger<hw>[:SWEep]:[:IMMediate].....463
:UNIT:ANGLE.....463
:UNIT:POWER.....464
[:SOURce]:CALIBration:STEReo:ANALog[:MEAS]?.....292
[:SOURce]:CORRection:CSET:CATalog?.....335
[:SOURce]:CORRection:CSET:DELete.....337
[:SOURce]:INPUT:TRIGger:SLOPe.....354
[:SOURce]:LFOutput:SHAPe.....363
[:SOURce]:LFOutput:SIMPedance.....363
[:SOURce]:LFOutput:VOLTAGE.....364

[:SOURCE]:LFOutput[:STATE].....357
[:SOURCE]:LFOutput<ch>:FREQUency.....355
[:SOURCE]:POWER:WIGNore.....386
[:SOURCE]:PULM[:INTernal][:TRAIN]:TRIGger:IMMediate.....405
[:SOURCE]:ROSCillator:EXTERNAL:FREQUENCY.....410
[:SOURCE]:ROSCillator:EXTERNAL:RFOFF[:STATE].....410
[:SOURCE]:ROSCillator:EXTERNAL:SBANDwidth.....411
[:SOURCE]:ROSCillator:SOURCE.....412
[:SOURCE]:ROSCillator[:INTernal]:ADJust:VALue.....411
[:SOURCE]:ROSCillator[:INTernal]:ADJust[:STATE].....411
[:SOURCE]:STEReO:ARI:BK[:CODE].....413
[:SOURCE]:STEReO:ARI:STATE.....413
[:SOURCE]:STEReO:ARI:TYPE.....414
[:SOURCE]:STEReO:ARI:TYPE:STATE.....414
[:SOURCE]:STEReO:ARI[:DEViation].....414
[:SOURCE]:STEReO:AUDIO:MODE.....415
[:SOURCE]:STEReO:AUDIO:PREemphasis.....415
[:SOURCE]:STEReO:AUDIO:PREemphasis:STATE.....416
[:SOURCE]:STEReO:AUDIO[:FREQUENCY].....416
[:SOURCE]:STEReO:DIRect.....417
[:SOURCE]:STEReO:EXTERNAL:IMPedance.....417
[:SOURCE]:STEReO:MMF.....417
[:SOURCE]:STEReO:PILot:PHASE.....418
[:SOURCE]:STEReO:PILot:STATE.....418
[:SOURCE]:STEReO:PILot[:DEViation].....418
[:SOURCE]:STEReO:RDS:DATaset.....418
[:SOURCE]:STEReO:RDS:STATE.....419
[:SOURCE]:STEReO:RDS:TRAFfic:ANNouncement[:STATE].....419
[:SOURCE]:STEReO:RDS:TRAFfic:PROGRAM[:STATE].....419
[:SOURCE]:STEReO:RDS[:DEViation].....420
[:SOURCE]:STEReO:SOURCE.....420
[:SOURCE]:STEReO:STATE.....421
[:SOURCE]:STEReO[:DEViation].....421
[:SOURCE<hw>:AM:DEPTH:EXPonential.....331
[:SOURCE<hw>:AM:DEPTH:LINear.....332
[:SOURCE<hw>:AM:EXTERNAL:COUPLING.....332
[:SOURCE<hw>:AM:SENSitivity?.....332
[:SOURCE<hw>:AM:SOURCE.....333
[:SOURCE<hw>:AM:STATE.....333
[:SOURCE<hw>:AM:TYPE.....334
[:SOURCE<hw>:CORRection:CSET:DATA:FREQUENCY.....335
[:SOURCE<hw>:CORRection:CSET:DATA:FREQUENCY:POINts?.....336
[:SOURCE<hw>:CORRection:CSET:DATA:POWER.....336
[:SOURCE<hw>:CORRection:CSET:DATA:POWER:POINts?.....336
[:SOURCE<hw>:CORRection:CSET:DATA[:SENSOR<ch>][:POWER]:SONCe.....337
[:SOURCE<hw>:CORRection:CSET[:SELECT].....341
[:SOURCE<hw>:CORRection:DEXChange:AFILe:CATalog?.....338
[:SOURCE<hw>:CORRection:DEXChange:AFILe:EXTension.....338
[:SOURCE<hw>:CORRection:DEXChange:AFILe:SELECT.....338
[:SOURCE<hw>:CORRection:DEXChange:AFILe:SEPARator:COLumn.....339

[:SOURCE<hw>]:CORRection:DEXChange:AFILe:SEParator:DECimal..339
[:SOURCE<hw>]:CORRection:DEXChange:EXECute..340
[:SOURCE<hw>]:CORRection:DEXChange:MODE..340
[:SOURCE<hw>]:CORRection:DEXChange:SELECT..341
[:SOURCE<hw>]:CORRection:VALUE?..342
[:SOURCE<hw>]:CORRection:ZEROing:STATE..342
[:SOURCE<hw>]:CORRection:[:STATE]..342
[:SOURCE<hw>]:FM:EXTERNAL:COUPLING..343
[:SOURCE<hw>]:FM:EXTERNAL:DEVIATION..344
[:SOURCE<hw>]:FM:INTERNAL:DEVIATION..344
[:SOURCE<hw>]:FM:MODE..344
[:SOURCE<hw>]:FM:SENSITIVITY?..345
[:SOURCE<hw>]:FM:SOURCE..345
[:SOURCE<hw>]:FM:STATE..346
[:SOURCE<hw>]:FM[:DEVIATION]..343
[:SOURCE<hw>]:FREQUENCY:CENTER..346
[:SOURCE<hw>]:FREQUENCY:MANUAL..348
[:SOURCE<hw>]:FREQUENCY:MODE..349
[:SOURCE<hw>]:FREQUENCY:MULTIPLIER..350
[:SOURCE<hw>]:FREQUENCY:OFFSET..350
[:SOURCE<hw>]:FREQUENCY:SPAN..350
[:SOURCE<hw>]:FREQUENCY:START..351
[:SOURCE<hw>]:FREQUENCY:STEP:MODE..353
[:SOURCE<hw>]:FREQUENCY:STEP[:INCREMENT]..352
[:SOURCE<hw>]:FREQUENCY:STOP..351
[:SOURCE<hw>]:FREQUENCY:CW|FIXed]..347
[:SOURCE<hw>]:FREQUENCY:CW|FIXed]:RCL..348
[:SOURCE<hw>]:INPUT:MODExt:IMPedance..353
[:SOURCE<hw>]:INPUT:MODExt:WIGNORE..353
[:SOURCE<hw>]:LFOutput:FREQUENCY:MANUAL..356
[:SOURCE<hw>]:LFOutput:FREQUENCY:MODE..356
[:SOURCE<hw>]:LFOutput:FREQUENCY:START..357
[:SOURCE<hw>]:LFOutput:FREQUENCY:STOP..357
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:DWELI..358
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:EXECUTE..358
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:MODE..358
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:POINTS..359
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:RETRACE..360
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:RUNNING?..360
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:SHAPES..361
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:SPACING..361
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:STEP:LOGARITHMIC..362
[:SOURCE<hw>]:LFOutput:SWEep[:FREQUENCY]:STEP[:LINear]..361
[:SOURCE<hw>]:LIST:CATALOG?..365
[:SOURCE<hw>]:LIST:DELETE..366
[:SOURCE<hw>]:LIST:DELETE:ALL..366
[:SOURCE<hw>]:LIST:DEXChange:AFILE:CATALOG?..367
[:SOURCE<hw>]:LIST:DEXChange:AFILE:EXTENSION..367
[:SOURCE<hw>]:LIST:DEXChange:AFILE:SELECT..368
[:SOURCE<hw>]:LIST:DEXChange:AFILE:SEPARATOR:COLUMN..368

[:SOURce<hw>]:LIST:DEXChange:AFILe:SEParator:DECimal..369
[:SOURce<hw>]:LIST:DEXChange:EXECute..369
[:SOURce<hw>]:LIST:DEXChange:MODE..370
[:SOURce<hw>]:LIST:DEXChange:SELECT..370
[:SOURce<hw>]:LIST:DWELI..370
[:SOURce<hw>]:LIST:FREE?..371
[:SOURce<hw>]:LIST:FREQUency..371
[:SOURce<hw>]:LIST:FREQUency:POINts?..372
[:SOURce<hw>]:LIST:INDEX..372
[:SOURce<hw>]:LIST:INDEX:START..372
[:SOURce<hw>]:LIST:INDEX:STOP..373
[:SOURce<hw>]:LIST:LEARN..373
[:SOURce<hw>]:LIST:MODE..374
[:SOURce<hw>]:LIST:POWER..374
[:SOURce<hw>]:LIST:POWER:POINts?..374
[:SOURce<hw>]:LIST:RESET..375
[:SOURce<hw>]:LIST:SELECT..375
[:SOURce<hw>]:LIST:TRIGger:EXECute..375
[:SOURce<hw>]:LIST:TRIGger:SOURce..376
[:SOURce<hw>]:MODulation[:ALL][:STATE]..376
[:SOURce<hw>]:PGenerator:STATE..377
[:SOURce<hw>]:PHASE..378
[:SOURce<hw>]:PHASE:REFERENCE..378
[:SOURce<hw>]:PM:External:COUpling..379
[:SOURce<hw>]:PM:External:DEViation..379
[:SOURce<hw>]:PM:Internal:DEViation..380
[:SOURce<hw>]:PM:MODE..380
[:SOURce<hw>]:PM:Sensitivity?..381
[:SOURce<hw>]:PM:SOURCE..381
[:SOURce<hw>]:PM:STATE..381
[:SOURce<hw>]:PM[:DEViation]..379
[:SOURce<hw>]:POWER:ALC:OMODe..382
[:SOURce<hw>]:POWER:ALC:SONCe..383
[:SOURce<hw>]:POWER:ALC[:STATE]..383
[:SOURce<hw>]:POWER:ATTenuation:RFOFF:MODE..384
[:SOURce<hw>]:POWER:EMF:STATE..384
[:SOURce<hw>]:POWER:LIMIT[:AMPLitude]..386
[:SOURce<hw>]:POWER:LMODE..387
[:SOURce<hw>]:POWER:MANUAL..387
[:SOURce<hw>]:POWER:MODE..388
[:SOURce<hw>]:POWER:POWER..388
[:SOURce<hw>]:POWER:SPC:CRANGe..389
[:SOURce<hw>]:POWER:SPC:DELAY..389
[:SOURce<hw>]:POWER:SPC:PEAK..390
[:SOURce<hw>]:POWER:SPC:SELECT..390
[:SOURce<hw>]:POWER:SPC:STATE..390
[:SOURce<hw>]:POWER:SPC:TARGET..390
[:SOURce<hw>]:POWER:START..391
[:SOURce<hw>]:POWER:STEP:MODE..392
[:SOURce<hw>]:POWER:STEP[:INCRement]..391

[:SOURce<hw>]:POWER:STOP.....392
[:SOURce<hw>]:POWER[:LEVEL][:IMMediate]:OFFSET.....385
[:SOURce<hw>]:POWER[:LEVEL][:IMMediate]:RCL.....386
[:SOURce<hw>]:POWER[:LEVEL][:IMMediate][:AMPLitude].....384
[:SOURce<hw>]:PULM:DELAY.....395
[:SOURce<hw>]:PULM:DOUBLE:DELAY.....396
[:SOURce<hw>]:PULM:DOUBLE:STATE.....396
[:SOURce<hw>]:PULM:DOUBLE:WIDTH.....396
[:SOURce<hw>]:PULM:MODE.....397
[:SOURce<hw>]:PULM:OUTPUT:SYNC[:STATE].....397
[:SOURce<hw>]:PULM:PERIOD.....397
[:SOURce<hw>]:PULM:POLarity.....398
[:SOURce<hw>]:PULM:SOURCe.....398
[:SOURce<hw>]:PULM:STATE.....399
[:SOURce<hw>]:PULM:TRAIN:CATalog?.....399
[:SOURce<hw>]:PULM:TRAIN:DELETE.....399
[:SOURce<hw>]:PULM:TRAIN:DEXChange:AFILe:CATalog?.....405
[:SOURce<hw>]:PULM:TRAIN:DEXChange:AFILe:EXTension.....406
[:SOURce<hw>]:PULM:TRAIN:DEXChange:AFILe:SELECT.....406
[:SOURce<hw>]:PULM:TRAIN:DEXChange:AFILe:SEPARator:COLumn.....407
[:SOURce<hw>]:PULM:TRAIN:DEXChange:AFILe:SEPARator:DECimal.....407
[:SOURce<hw>]:PULM:TRAIN:DEXChange:EXECute.....408
[:SOURce<hw>]:PULM:TRAIN:DEXChange:MODE.....408
[:SOURce<hw>]:PULM:TRAIN:DEXChange:SELECT.....409
[:SOURce<hw>]:PULM:TRAIN:OFFTime.....400
[:SOURce<hw>]:PULM:TRAIN:OFFTime:POINts?.....400
[:SOURce<hw>]:PULM:TRAIN:ONTime.....401
[:SOURce<hw>]:PULM:TRAIN:ONTime:POINts?.....401
[:SOURce<hw>]:PULM:TRAIN:REPetition.....402
[:SOURce<hw>]:PULM:TRAIN:REPetition:POINts?.....402
[:SOURce<hw>]:PULM:TRAIN:SELECT.....403
[:SOURce<hw>]:PULM:TRIGger:EXTERNAL:GATE:POLarity.....403
[:SOURce<hw>]:PULM:TRIGger:EXTERNAL:IMPedance.....404
[:SOURce<hw>]:PULM:TRIGger:EXTERNAL:SLOPe.....404
[:SOURce<hw>]:PULM:TRIGger:MODE.....404
[:SOURce<hw>]:PULM:WIDTH.....409
[:SOURce<hw>]:SWEep:POWER:DWEL.....429
[:SOURce<hw>]:SWEep:POWER:EXECute.....429
[:SOURce<hw>]:SWEep:POWER:MODE.....430
[:SOURce<hw>]:SWEep:POWER:POINTs.....430
[:SOURce<hw>]:SWEep:POWER:RETRace.....431
[:SOURce<hw>]:SWEep:POWER:RUNNING?.....431
[:SOURce<hw>]:SWEep:POWER:SHAPe.....431
[:SOURce<hw>]:SWEep:POWER:SPACing:MODE?.....432
[:SOURce<hw>]:SWEep:POWER:STEP[:LOGarithmic].....432
[:SOURce<hw>]:SWEep:RESET[:ALL].....433
[:SOURce<hw>]:SWEep[:FREQUency]:DWEL.....422
[:SOURce<hw>]:SWEep[:FREQUency]:EXECute.....423
[:SOURce<hw>]:SWEep[:FREQUency]:LFConnector.....423
[:SOURce<hw>]:SWEep[:FREQUency]:MODE.....424

[:SOURce<hw>]:SWEep[:FREQUency]:OVOLtage:STARt..424
[:SOURce<hw>]:SWEep[:FREQUency]:OVOLtage:STOP..425
[:SOURce<hw>]:SWEep[:FREQUency]:POINts..425
[:SOURce<hw>]:SWEep[:FREQUency]:RETRace..426
[:SOURce<hw>]:SWEep[:FREQUency]:RUNNING?..426
[:SOURce<hw>]:SWEep[:FREQUency]:SHAPe..426
[:SOURce<hw>]:SWEep[:FREQUency]:SPACing..427
[:SOURce<hw>]:SWEep[:FREQUency]:STEP:LOGarithmic..428
[:SOURce<hw>]:SWEep[:FREQUency]:STEP[:LINear]..427
*CLS..284
*ESE..284
*ESR?..284
*IDN?..285
*IST?..285
*OPC..285
*OPT?..285
*PRE..286
*PSC..286
*RCL..286
*RST..286
*SAV..287
*SRE..287
*STB?..287
*TRG..287
*TST?..288
*WAI..288
SENSE<ch>[:POWER]:SNUMBER?..328
SENSE<ch>[:POWER]:STATUS[:DEVICE]?..329
SENSE<ch>[:POWER]:SVERSION?..329
SENSE<ch>[:POWER]:TYPE?..329
STEReo:DIRect..464
STEReo:DIRect..465
STEReo:DIRect..466
STEReo:DIRect..466
STEReo:DIRect..467
STEReo:DIRect..467
STEReo:DIRect..468
STEReo:DIRect..468
STEReo:DIRect..468
STEReo:DIRect..469
STEReo:DIRect..469
STEReo:DIRect..470
STEReo:DIRect..471
STEReo:DIRect..471
STEReo:DIRect..472
STEReo:DIRect..472
STEReo:DIRect..473
STEReo:DIRect..473
STEReo:DIRect..473

STEReo:DIRect.....474
STEReo:DIRect.....474
STEReo:DIRect.....475
STEReo:DIRect.....475
STEReo:DIRect.....476
STEReo:DIRect.....476
STEReo:DIRect.....477
STEReo:DIRect.....477
STEReo:DIRect.....477
STEReo:DIRect.....478
STEReo:DIRect.....478
STEReo:DIRect.....479
STEReo:DIRect.....479
STEReo:DIRect.....479
STEReo:DIRect.....480
STEReo:DIRect.....480
STEReo:DIRect.....481
STEReo:DIRect.....481
STEReo:DIRect.....481
STEReo:DIRect.....482
STEReo:DIRect.....482
STEReo:DIRect.....482
STEReo:DIRect.....483
STEReo:DIRect.....484
STEReo:DIRect.....484
STEReo:DIRect.....484
STEReo:DIRect.....485
STEReo:DIRect?.....484

### Index

#### Symbols

*OPC ..... 272
*OPC? ..... 272
*RST ..... 281
*WAI ..... 272
/var directory ..... 305
>L ..... 250
>R ..... 250
&LLO ..... 250
9.91E37
Remote control ..... 271
10 - Hardware failure ..... 505
11 - DSP failure ..... 505
12 - IIC bus error ..... 505
13 - Real time clock error ..... 505
50 - Extern reference out of range or disconnected ..... 504
100 - Software failure ..... 505
101 - DSP mailbox timeout ..... 505
102 - DSP overflow ..... 505
120 - RDS error ..... 505
121 - PI error ..... 505
122 - PSN error ..... 505
123 - Buffer overflow ..... 505
124 - Dataset empty ..... 505
140 - This modulation forces other modulations off ..... 504
180 - Adjustment failed ..... 504
182 - Adjustment data missing ..... 504
183 - Adjustment data invalid ..... 504
200 - Cannot access hardware ..... 504
201 - Hardware revision out of date ..... 504
202 - Cannot access the EEPROM ..... 504
203 - Invalid EEPROM data ..... 504
204 - river initialization failed ..... 504
241 - No current list ..... 504
242 - Unknown list type specified ..... 504
460 - Cannot open file ..... 505
461 - Cannot write file ..... 505
462 - Cannot read file ..... 505
463 - Filename missing ..... 505
464 - Invalid filename extension ..... 505
465 - File contains invalid data ..... 505

##### A

Abort button ..... 83
Abort calculation ..... 83
About
Internal adjustments ..... 491
Selftests ..... 491
AC supply ..... 55
Accept
Security Settings ..... 121
Access denied ..... 76
Add LAN sensor
NRP sensor mapping ..... 169
Add sensor
NRP sensor mapping ..... 169
Add USBTMC sensor
NRP sensor mapping ..... 169
Address mode
Network settings ..... 106
Adjust All ..... 495

Adjust Analog Channels ..... 292, 496
Adjust Level ..... 495
Adjust Synthesis ..... 495
Adjust Synthesis Extension ..... 495
Adjustment active
Reference oscillator ..... 145
Adjustment Data ..... 152
Adjustment frequency
Reference oscillator ..... 146
Adjustments
All ..... 495
Settings ..... 495
ALC (automatic level control) ..... 153, 382
ALC Auto ..... 501
ALC On ..... 153, 501
ALC S&H ..... 501
ALC Table ..... 501
All packages
Secure update policy ..... 121
AM Depth ..... 205
AM Sensitivity ..... 206
AM Source ..... 205
AM State ..... 205
AM Type
amplitude modulation ..... 205
Amplitude
Annotation (setup security) ..... 119
Amplitude modulation
AM type ..... 205
Analog modulation
Ignore overvoltage warning ..... 207, 210, 214
Annotation
Amplitude (security) ..... 119
Frequency (security) ..... 119
Aperture time
Power sensors ..... 177
Application cards ..... 18
Application notes ..... 18
Applications of the R&S SMB ..... 63
ARI BK Code ..... 221, 413
ARI Deviation ..... 221
ARI Identification ..... 221, 414
ARI State ..... 221, 413
Arrow keys ..... 52
ASCII file import/export
List Mode ..... 192
Assemblies display ..... 97
ATN ..... 508
Attention ..... 508
Attenuator Mode ..... 151
Attenuator Settings
Fixed Range (PEP) in ..... 151
mode ..... 151
RF OFF Mode ..... 151
AttFixed ..... 502
AUDIO Identification ..... 415
Audio Source ..... 420
Stereo modulation ..... 218
Auto once
Power Sensors ..... 177
Auto Once - Power Sensors ..... 326
Autonegotiation
Failed ..... 505

Avahi
LAN Services ..... 118

##### B

Baudrate
RS232 interface ..... 110
Bench top, placing ..... 21
Block data ..... 268
Bluetooth
RS232 Interface ..... 109
Bluetooth Pin ..... 121
Boolean parameters ..... 267
Brackets ..... 268
Brochures ..... 17
Browser settings
LXI ..... 40
Buffer overflow (123) ..... 505
BUSY ..... 500
##### C
Carrying the instrument ..... 20
Case-sensitivity
SCPI ..... 264
Catch range
NRP level control ..... 159
Center Freq ..... 184
Change password
Setup (security) ..... 117
Check front panel ..... 492
Performing ..... 493
Settings ..... 493
Checking the instrument ..... 20
Cleaning ..... 490
Clear
NRP sensor mapping ..... 169
Clear status
Remote ..... 284
Closed loop power control
see NRP level control ..... 155
CLPC
see NRP level control ..... 155
Colon ..... 268
Column Separator Import/Export
Pulse Train data ..... 239
Column Separator Import/Export - User Correction data ..... 163
Comma ..... 268
Command sequence
Recommendation ..... 281
Remote ..... 288
Commands ..... 242
Brackets ..... 268
Colon ..... 268
Comma ..... 268
Command line structure ..... 269
Common ..... 242
Double dagger ..... 268
GBIP, addressed ..... 248
GBIP, universal ..... 247
Instrument control ..... 242
Overlapping ..... 271
Pipe ..... 268
Question mark ..... 268
Quotation mark ..... 268
SCPI confirmed ..... 242
Sequential ..... 271

Syntax elements ..... 268
White space ..... 268
Common commands
Syntax ..... 263
Computer name
Changing ..... 31
see hostname ..... 105
use instead of IP address ..... 31
CONDITION ..... 276
Confirm unsigned
Secure update policy ..... 121
Confirm user password
Setup (security) ..... 117
Connecting
Keyboard ..... 25
LAN ..... 24, 25
Memory stick ..... 25
Mouse ..... 25
Power ..... 24
To Ref In/Ref Out ..... 27
To RF ..... 26
USB devices ..... 25
Connection fault
169.254.*.* ..... 505
Connection to an external controller ..... 32
Connector
GPIB ..... 56
IEC/IEEE ..... 56
INSTR TRIG ..... 56
LAN ..... 55
LF ..... 53
MOD EXT ..... 53
PULSE EXT ..... 56
PULSE VIDEO ..... 56
REF IN ..... 57
REF OUT ..... 57
RF ..... 54
S/P DIF ..... 56
SIGNAL VALID ..... 56
Stereo R/L ..... 56
USB type A ..... 55
USB type B ..... 508
Control bus ..... 32
Controller, external
Copy
Several files ..... 133
Copy instrument settings ..... 133
Counter ..... 97, 294
Coupling mode ..... 210
Coupling mode - EXT MOD (AM) ..... 206
Create New Directory ..... 133
Current Freq (LF Sweep) ..... 229
Current Freq (RF Sweep) ..... 184
Current Index
List Mode ..... 196
Current Level ..... 191
Current Sensors ..... 101
Cursor keys ..... 52
Customer support ..... 507
Cut ..... 133

##### D

Data sheets ..... 17
Data Valid ..... 508
Dataset empty (124) ..... 505

Date
Setup ..... 492
DAV ..... 508
DCL ..... 247
Deactivate RF output ..... 145
Decimal Point Import/Export
Pulse Train data ..... 239
Decimal Point Import/Export - List Mode data ..... 199
Decimal Point Import/Export - User Correction data ..... 163
DEF ..... 266
Default gateway
Network settings ..... 107
Default instrument settings ..... 29, 96
Default values
Remote ..... 286
Delay
Double pulse ..... 233
Pulse generator ..... 233
Delay time
NRP level control ..... 159
Delete instrument settings ..... 133, 311
Delta Phase ..... 142
Destination Import/Export
Pulse Train data ..... 239
User Correction data ..... 164
Destination Import/Export - List Mode data ..... 200
Deviation
Stereo modulation ..... 218
Device ID ..... 100
NRP sensor mapping ..... 169
Device-specific error messages ..... 503
DHCP ..... 30
Direct programming ..... 417
Display ..... 71
Lock (security) ..... 119
Display priority
Power sensors ..... 174
DISPLAY subsystem ..... 294
Disposal ..... 490
DNS server
Network settings ..... 107
DNS suffix
Network settings ..... 107
Documentation overview ..... 17
Double dagger ..... 268
Double Pulse ..... 232
Double pulse delay
Pulse generator ..... 233
Double pulse width
Pulse generator ..... 233
DOWN ..... 266
DSP failure (11) ..... 505
DSP mailbox timeout (101) ..... 505
DSP overflow (102) ..... 505
Dwell time
RF sweep ..... 186
Dwell Time ..... 192, 195, 230
List Mode ..... 195


##### E

Pulse Train ..... 237
Edit Pulse Train Data ..... 236
Edit User Correction Data ..... 161
EFC mode (Electronic Frequency Control) ..... 144
EMF - Level display ..... 153

Emulation
IDN string ..... 112
Language ..... 111
Mode ..... 111
Set IDN and OPT to default ..... 112
Enable
LAN interface ..... 118
ENABLE ..... 276
Enable registers
Remote ..... 286
Enter License Key ..... 100
EOI ..... 508
Error log ..... 506
Error messages ..... 73, 499
Adjustment data invalid (183) ..... 504
Adjustment data missing (182) ..... 504
Adjustment failed (180) ..... 504
Cannot access hardware (200) ..... 504
Cannot access the EEPROM (202) ..... 504
Cannot open file (460) ..... 505
Cannot read file (462) ..... 505
Cannot write file (461) ..... 505
Driver initialization failed (204) ..... 504
External reference out of range or disconnected (50) ..... 504
File contains invalid data (465) ..... 505
Filename missing (463) ..... 505
Hardware revision out of date (201) ..... 504
Invalid EEPROM data (203) ..... 504
Invalid filename extension (464) ..... 505
No current list (241) ..... 504
This modulation forces other modulations off (140) ..... 504
Unknown list type specified (242) ..... 504
Error Messages ..... 502
device-specific ..... 503
SCPI ..... 503
Error queue ..... 277
Error queue query ..... 439, 440
Error queues
Recommendations ..... 282
ESE (event status enable register) ..... 278
ESR ..... 274
ESR (event status register) ..... 278
EVENT ..... 276
Event status enable register (ESE) ..... 278
Remote ..... 284
Event status register (ESR) ..... 278
Remote ..... 284
Exclude Frequency ..... 131
Exclude Level ..... 131
Execute
Single trigger ..... 234
Execute Single
List Mode ..... 195
Execute single sweep
Frequency sweep ..... 183
Execute Single Sweep
Level Sweep ..... 190
Execute Single Sweep - LF Sweep ..... 228
Expiration date of option ..... 99
Export Deactivation Response ..... 101
EXT REF ..... 500
Extension Import/Export
Pulse Train data ..... 238
Extension Import/Export - List Mode data ..... 199
Extension Import/Export - User Correction data ..... 163
External frequency
Reference oscillator ..... 145

External impedance
Pulse generator ..... 234
External Impedance
Stereo modulation ..... 219
External input impedance ..... 234
External modulation signal ..... 204
FM ..... 207
PhiM ..... 211
External modulation signal - voltage ..... 203

##### F

Factory preset ..... 122
Features of R&S SMB ..... 63
File
Hard copy ..... 124
File info
Hard copy ..... 124
File list ..... 308
File menu
Save/Recall ..... 128
File name
Hard copy ..... 124
File setup
Hard copy ..... 125
File type selection ..... 132
Fill Table
List Mode ..... 197
Fill Table - List mode ..... 200
Fill Table From ..... 200
Fill Table Range ..... 164, 200
Filter
Low harmonic ..... 150
mode (low harmonic) ..... 150
Power Sensors ..... 175
State (low harmonic) ..... 151
Filter Length
Power Sensors ..... 175
Filter Length - Power Sensors ..... 324, 325, 326
Firmware version ..... 99
Floating licenses ..... 99
FM deviation ..... 209
FM Deviation
Stereo modulation ..... 218
FM mode ..... 209
FM sensitivity ..... 210
FM source ..... 208
FM state ..... 208
FM-DC mode ..... 142
FREQ OFFSET ..... 499
Frequency
Annotation (setup security) ..... 119
LF generator ..... 206, 209, 213, 219, 224
List Mode ..... 197
Multiplier ..... 141
PM ..... 233
Power sensors ..... 175
RF signal ..... 138
Set Frequency ..... 139
Stereo modulation ..... 219
Frequency - RF sweep ..... 183, 184
Frequency (status bar)
RF signal ..... 139
Frequency dialog
RF signal ..... 139
Frequency display ..... 72
Front panel key emulation ..... 68, 91

Front panel test
Performing ..... 493
FTP
How to ..... 133
LAN Services ..... 118
Fuses
How to replace the line fuse ..... 490

##### G

Gate input polarity ..... 234
Gated
Pulse trigger input ..... 234
Gated signal
Pulse modulation ..... 233
Gateway ..... 107
GET ..... 248
Goto local
Remote channel settings ..... 110
GPIB ..... 110
bus address ..... 109
Characteristics ..... 246
Connecting ..... 25
interface messages ..... 247
Remote control interface ..... 240
GPIB address ..... 248
GTL ..... 248
GUI Language ..... 108

##### H

Hard copy 124
File 124
File info 124
File name 124
File setup 125
Hardware Config 97
Hardware failure (10) 505
Hardware options 98
Help 17
HiSLIP 244
Protocol 243
Resource string 243
Host name 169
NRP sensor mapping 169
see Computer name 31
Hostname 105
Network settings 105
see Computer name 31
How to: 490
Replace the line fuse 490
HTTP 118
LAN Services 118

#### i

Identification
Emulation mode ..... 111
Remote ..... 285
Identification mode ..... 111
IDN string
Emulation ..... 112
Emulation mode ..... 112
IEC 625/IEEE 488
Connecting ..... 25
IFC ..... 247, 508

Ignore level warning
Level ..... 150
Ignore overvoltage warning
Analog modulation ..... 207, 210, 214
IIC bus error (12) ..... 505
Impedance
External input ..... 207, 210, 214
Impedance Audio Signal ..... 219
Import License Key ..... 100
Import/Export
List Mode data ..... 200
Pulse Train data ..... 238, 239
User Correction data ..... 162, 164
Import/Export - List Mode data ..... 198
INF ..... 266
INFO key ..... 73
Input
Stereo Modulation ..... 217
Input connector ..... 53, 56, 57
Installed Assembly ..... 98
Instrument
Carrying ..... 20
Checking ..... 20
Lifting ..... 20
Operating site ..... 20, 21
Unpacking ..... 20
Instrument Config ..... 97
Instrument Emulations ..... 110
Instrument help ..... 17
Instrument messages ..... 241
Instrument settings
Recall ..... 130, 286, 312
Save ..... 128, 287, 313
Instrument Trigger ..... 187, 192, 198, 231
Instrument-specific commands ..... 242
Interface
functions (GPIB bus) ..... 509
Interface Clear ..... 508
Interface messages ..... 241, 245
Interfaces
GPIB ..... 246
USB ..... 245
Internal adjustments
About ..... 491
Interrupt ..... 279
Invalid results
Remote control ..... 271
IP address ..... 243
Changing ..... 30
Network settings ..... 106
Not recognized ..... 505
NRP sensor mapping ..... 169
IP address mode ..... 106
IST ..... 274
IST flag
Remote ..... 285

##### K

Key
Arrow key ..... 53
BACKSPACE ..... 51
Cursor key ..... 53
DIAGRAM ..... 51
ENTER ..... 52
ESC ..... 51
FILE ..... 49, 127

FREQ ..... 50
HELP ..... 49
INFO ..... 49
LEVEL ..... 50
LOCAL ..... 49
MOD ON/OFF ..... 51, 202
ON/OFF TOGGLE ..... 50
PRESET ..... 29, 49, 96
RF ON/OFF ..... 51
SETUP ..... 49, 97
WINBAR ..... 49
Key - HELP ..... 126
Key - LOCAL ..... 123
Key combinations ..... 66, 90
Keyboard
Layout ..... 108
Keyboard emulation ..... 68
Keyboard Language ..... 304
Keypad ..... 51
Keywords
Mnemonics ..... 263

##### L

LAN
Configuration ..... 30
Connecting ..... 24, 25
Environment ..... 24
Interface ..... 242
IP address ..... 243
Remote control interface ..... 240
VISA ..... 243
VXI protocol ..... 244
LAN interface
Avahi ..... 118
enable ..... 118
FTP ..... 118
HTTP ..... 118
LAN over SCPI ..... 118
SMB (Samba) ..... 118
SSH ..... 118
VNC ..... 118
LAN Interface
Services ..... 117
Setup ..... 117
LAN Status - LXI ..... 112
Language
Emulation ..... 111
Language Keyboard ..... 304
Layout
Keyboard ..... 108
Learn List Mode Data ..... 196
Level ..... 147
Ignore level warning ..... 150
limit ..... 149, 158
Power sensors ..... 173
RF output ..... 149
uninterrupted setting ..... 151
Level - Step width ..... 152
Level (Avg.)
Power sensors ..... 173
Level control ..... 153, 382
Level display ..... 72
Level display EMF ..... 153
Level limit
RF level ..... 149, 158

Level offset ..... 150
Power Sensors ..... 175
State (power sensors) ..... 175
LEVEL OFFSET ..... 500
Level Sweep ..... 187
Level user correction ..... 159
LevelSweep ..... 501
LF frequency sweep
    Retrace ..... 230
LF gen frequency ..... 206, 209, 213, 224
LF gen shape ..... 206, 209, 213, 225
LF Gen Voltage ..... 224
LF generator ..... 223
    Frequency ..... 206, 209, 213, 224
    Shape ..... 206, 209, 213, 225
LF output ..... 224
LF Output
    State ..... 224
    Sweep voltage ..... 186, 187
LF output - Source Impedance ..... 225
LF Source Impedance ..... 225
LF sweep
    Step lin ..... 230
    Step log ..... 230
    Step width ..... 230
LF Sweep ..... 229
LF Gen Frequency ..... 219
LF Sweep ..... 501
License for software option ..... 99
License Key ..... 100
Lifting the instrument ..... 20
Limit
    Level ..... 149, 158
Linux ..... 30
Linux controller ..... 34
List mode ..... 192
    manual processing of lists ..... 194
List Mode Data ..... 196
List Range In ..... 198
ListMode ..... 502
LLO ..... 247
Load instrument settings ..... 130, 286, 312
LOC-LLO ..... 501
Local to remote switchover ..... 249, 250
Lock
    Display (setup security) ..... 119
Low harmonic
    Filter ..... 150
    Filter mode ..... 150
    Filter state ..... 151
LXl ..... 112
    Browser interface ..... 40
    Browser settings ..... 40
    Configuration ..... 39
    LAN configuration ..... 42
    Ping ..... 44
    Remote trace (SCPI) ..... 46
    Web Control ..... 45

##### M

Mac address ..... 107
Maintenance ..... 490, 491
Malfunctions
Reacting ..... 282
Manual Remote Control ..... 32
MAX ..... 266

Max Modulation Freq ..... 220
Maximum deviation ..... 213
FM ..... 209
Measured level
NRP level control ..... 158
Menu structure
Access denied ..... 76
Header ..... 76
Menu area ..... 76
Messages
Commands ..... 242
Instrument ..... 241
Instrument responses ..... 242
Interface ..... 241
MIN ..... 266
Mnemonics ..... 263
Optional ..... 265
Mod external coupling
AM ..... 206
FM ..... 210
PhiM ..... 214
Mod Gen block ..... 64
MOD OFF ..... 499
MOD ON/OFF Key ..... 202
Mode
Emulation ..... 111
FM ..... 209
Level Sweep ..... 188
LF Gen ..... 226
LF sweep ..... 226
List Mode ..... 194
Low harmonic filter ..... 150
PhiM ..... 212
Pulse generator ..... 232
RF frequency sweep ..... 180
RF level ..... 148
Mode Import/Export
Pulse Train data ..... 238
Mode Import/Export - List Mode data ..... 199
Mode Import/Export - User Correction data ..... 163
Modulation
AM ..... 204
FM ..... 207
PhiM ..... 211
PM ..... 214
Pulse modulation ..... 214
Stereo modulation ..... 217
Modulation depth - AM ..... 205
Modulation deviation
FM ..... 209
PhiM ..... 213
Mounting, in a rack ..... 22
MPX Deviation ..... 218
Multi transmitter measurements ..... 153
Multiple files
Copy ..... 133
Multiplier
Frequency ..... 141

##### N

NAN ..... 266
NAN (not a number)
Remote control ..... 271
Navigation keys ..... 52
NDAC ..... 508

Network
Connection fails ..... 505
Environment ..... 24
Network settings ..... 104
Address mode ..... 106
Default gateway ..... 107
DNS server ..... 107
DNS suffix ..... 107
Hostname ..... 105
IP address ..... 106
Status ..... 105
Subnet mask ..... 106
Workgroup ..... 106
Network status ..... 105
New password
Setup (security) ..... 117
New user password
Setup (security) ..... 116
NINF ..... 266
No-load voltage (EMF) ..... 153
NRFD ..... 508
NRP Info ..... 101
NRP level control ..... 155
catch range ..... 159
delay time ..... 159
measured level ..... 158
sensor ..... 157
state ..... 157
target level ..... 158
use peak power ..... 159
NRP power control
RF level limit ..... 149, 158
use SParameter ..... 159, 177
NRP power viewer
use SParameter ..... 159, 177
NRP sensor mapping
Add LAN sensor ..... 169
Add sensor ..... 169
Add USB TMC sensor ..... 169
Clear ..... 169
Connector ..... 168
Device ID ..... 169
Host name ..... 169
IP address ..... 169
Protocol ..... 168
Scan ..... 169
Sensor name ..... 168, 169
Serial number ..... 169
Settings ..... 168
NTP
Address ..... 492
NTRansition ..... 276
Number of licenses ..... 99
Numeric parameters ..... 266
Numeric values
Special ..... 266

##### O

OCXO ..... 57
Off time
Pulse Train ..... 237
Offset
RF signal ..... 140
Offset - Level ..... 150
Old password
Setup (security) ..... 117

Setup (security) ..... 116
On time
Pulse Train ..... 237
Online help
Working with ..... 85
Open source acknowledgment ..... 18
Open source acknowledgments ..... 99
Operating concept ..... 66
Operating site
Choosing ..... 20
Setting up the instrument ..... 21
Operating system ..... 30
Operation complete
Remote ..... 285
Operation hours ..... 98, 294
Operation mode
File menu ..... 128
OPT String ..... 112
Option
Hardware ..... 98
Software ..... 98
Option: expiration date ..... 99
Options
Hard copy ..... 124
Identification (remote) ..... 285
OSA ..... 18
Oscillator
Reference ..... 142
Source (reference) ..... 144
Output
LF ..... 224
Output connector ..... 53, 54, 56, 57
Output queue ..... 274
Output Voltage - LF output ..... 224
Output voltage start ..... 186
Output voltage stop ..... 187
OvenCold ..... 502
Overlapped commands
Preventing ..... 272
Overlapping commands ..... 271
Overload ..... 167
OVERLOAD ..... 500

##### P

Parallel poll register enable
Remote ..... 286
Parameters
Block data ..... 268
Boolean ..... 267
Numeric values ..... 266
SCPI ..... 265
Special numeric values ..... 266
String ..... 268
Text ..... 267
Parity
RS232 interface ..... 110
Part numbers ..... 97
Password
Change (security password) ..... 117
Confirm (security password) ..... 117
Confirm (user password (security)) ..... 117
New (security password) ..... 117
New (user password (security)) ..... 116
Old (security password) ..... 117
Paste ..... 133

PCI bus ..... 98
Period
Pulse generator ..... 233
Permanent display
Power sensors ..... 174
Phase
RF signal ..... 142
PhiM
Mode ..... 212
Modulation deviation ..... 213
Source ..... 212
PhiM Sensitivity ..... 214
PhiM State ..... 212
PI error (121) ..... 505
Pilot Deviation ..... 220
Pilot Phase ..... 220
Pilot State ..... 220, 418
Ping
LXI ..... 44
Pipe ..... 268
Placing, on a bench top ..... 21
Polarity
Pulse Modulation ..... 216
Power
Connecting the instrument ..... 24
List Mode ..... 197
Power sensors
Aperture time ..... 177
Auto once ..... 177
Display priority ..... 174
Filter ..... 175
Frequency ..... 175
Level ..... 173
Level (Avg.) ..... 173
Level offset ..... 175
NRP level control ..... 155
Permanent display ..... 174
Power viewer ..... 172
Source ..... 175
State ..... 173
State (level offset) ..... 175
Type ..... 173
Unit ..... 174
Use default aperture time ..... 177
zero ..... 158, 174
Power-On Count ..... 97, 98
Power-On Counter ..... 294
Power-On State ..... 153
PPC ..... 248
PPE ..... 274
PPU ..... 247
Preemphasis ..... 219, 416
Preparing for use ..... 20
Preset ..... 29
Preset - instrument settings ..... 96
Preset instrument settings ..... 29
Protection ..... 113
Protocol
VXI ..... 244
PSN error (122) ..... 505
PTRansition ..... 276
Pulse delay
Pulse generator ..... 233
Pulse external impedance ..... 234
Pulse generator ..... 231
Delay ..... 233
Double pulse delay ..... 233

Double pulse width ..... 233
External impedance ..... 234
Mode ..... 232
Period ..... 233
Settings ..... 231
Signal valid as pulse sync ..... 235
Trigger mode ..... 233
Video-Sync signal state ..... 232
Width ..... 233
Pulse Generator
Pulse Train ..... 235
Pulse generator state ..... 232
Pulse mode
Pulse generator ..... 232
Pulse Mode ..... 232
Pulse modulation ..... 214
repetition frequency ..... 233
Signal valid as pulse sync ..... 235
Source ..... 216
State ..... 216
Trigger mode ..... 233
Pulse period
Pulse generator ..... 233
Pulse Train ..... 232
Pulse Generator ..... 235
Pulse Train Data ..... 236
Pulse trigger input slope ..... 234
Pulse width
Pulse generator ..... 233
Q
Queries ..... 242, 270
Status ..... 280
Question mark ..... 268, 270
Questionable status register ..... 278, 279
Quotation mark ..... 268
R
R&S signed packages
Secure update policy ..... 121
Rack, mounting ..... 22
RDS Data Set ..... 222, 418
RDS Deviation ..... 222
RDS error (120) ..... 505
RDS Program Identification ..... 222
RDS Program Service Name ..... 222
RDS State ..... 221, 419
RDS Traffic Announcement ..... 223, 419
RDS Traffic Program ..... 222, 419
Real time clock error (13) ..... 505
Recall instrument settings ..... 130, 131, 286, 312
Recall intermediate ..... 286
Recall Intermediate ..... 131
Recommendations
Remote control programming ..... 281
Reference
Source (oscillator) ..... 144
Reference frequency
External ..... 145
Reference oscillator ..... 142
Adjustment active ..... 145
Adjustment frequency ..... 146
External frequency ..... 145
RF off state ..... 145
Settings ..... 144

Source ..... 144
synchronization bandwidth ..... 145
Registers ..... 274
Release notes ..... 18
REM-LLO ..... 501
Remote
Bluetooth ..... 109
RS232 ..... 109
REMOTE ..... 501
Remote access ..... 32
Remote channel
GPIB address ..... 109
Remote channel settings ..... 109
Goto local ..... 110
GPIB resource (VISA resource string) ..... 110
HISLIP resource (VISA resource string) ..... 110
Network resource (Ethernet resource string) ..... 110
Serial ..... 110
Socket resource (VISA resource string) ..... 110
USB VISA resource string ..... 110
Remote control
Basics ..... 240
GPIB address ..... 248
Interfaces ..... 240
Protocols ..... 240
Remote Control
GPIB ..... 251
Remote control switchover ..... 249
Remote trace
LXI ..... 46
Removing sensitive data ..... 506
REN ..... 508
Rename ..... 133
File ..... 312
Repetition
Pulse Train ..... 237
Replace the line fuse
How to: ..... 490
Reset ..... 183
List Mode ..... 195
Reset - RF sweep ..... 183
Reset Delta Phase Display ..... 142
Reset instrument settings ..... 29, 96
Reset values
Remote ..... 286
Resolving network problems ..... 107
Restart network
Setting ..... 107
Retrace
LF frequency sweep ..... 230
RF frequency sweep ..... 185
RF level sweep ..... 191
Reverse power protection ..... 167
Revisions ..... 97
RF block ..... 65
RF During Power Search - ALC ..... 155
RF frequency
Multiplier ..... 141
Settings ..... 140
RF Frequency
Set frequency (dialog) ..... 140
RF frequency sweep
Retrace ..... 185
RF frequency vs. RF output frequency ..... 138
RF level
Mode ..... 148

RF Level
limit ..... 149, 158
RF level limit
NRP power control ..... 149, 158
RF Level Sweep
Retrace ..... 191
RF OFF ..... 136, 499
RF off state
Reference oscillator ..... 145
RF ON ..... 136
RF ON/OFF key ..... 136
RF output
Power-on state ..... 153
RF Output ..... 136
RF output level ..... 149
RF output state ..... 137
RF signal
Frequency ..... 138
Frequency dialog ..... 139
Frequency offset ..... 140
Multiplier ..... 141
Offset ..... 140
Phase ..... 142
Phase adjustment ..... 142
Set frequency (status bar) ..... 139
User variation ..... 141
RF State ..... 137
RF sweep
Step lin ..... 185
Step log ..... 185
Step width ..... 185
RFSweep ..... 501
Rotary knob ..... 52
RS232
Baud rate ..... 110
Parity ..... 110
Serial interface ..... 109
Stop bits ..... 110
Run Update ..... 101
S
Safety instructions ..... 13
SAMBA/SMB
How to ..... 133
Sample-and-Hold mode ..... 382
Sample&Hold mode ..... 153
Save immediate ..... 128
Save instrument settings ..... 128, 287, 313
Save intermediate ..... 287
Scan
NRP sensor mapping ..... 169
SCPI
LAN Services ..... 118
Parameters ..... 265
Syntax ..... 263
Version ..... 241
SCPI - error messages ..... 503
SCPI confirmed commands ..... 242
SCPI remote trace
LXI ..... 46
Screen Saver ..... 108
SDC ..... 248
Search Once - ALC ..... 155

Secure update policy
All packages ..... 121
Confirm unsigned ..... 121
R&S signed packages ..... 121
Security
Accept settings ..... 121
Annotation amplitude (setup) ..... 119
Annotation frequency (setup) ..... 119
Change password (setup) ..... 117
Confirm password (setup) ..... 117
Confirm user password (setup) ..... 117
Display lock (setup) ..... 119
New password (setup) ..... 117
New user password (setup) ..... 116
Old password (setup) ..... 117
Secure update policy ..... 121
Security password ..... 117
User interface ..... 119
User password ..... 116
Security password
Security ..... 117
Setup ..... 117
Security Password
Setup ..... 121
Security settings
LAN services ..... 117
Security Settings
Security Password ..... 121
USB storage ..... 119
User Name ..... 116
Volatile Mode ..... 119
Select ASCII Destination
Pulse Train data ..... 239
User Correction data ..... 163
Select ASCII Destination - List Mode data ..... 199
Select ASCII Source
Pulse Train data ..... 239
User Correction data ..... 163
Select List ..... 196
Select NRP File ..... 101
Select Pulse Train Data ..... 236
Select sensor
Power viewer ..... 172
Self-test
Remote ..... 288
Selftest ..... 497
Selftests
About ..... 491
Sensor
NRP level control ..... 157
Power viewer ..... 172
Sensor name
NRP sensor mapping ..... 169
Sequential commands ..... 271
Serial bus ..... 98
Serial interface
RS232 ..... 109
Serial number
NRP sensor mapping ..... 169
Serial numbers ..... 97
Service request (SRQ) ..... 277, 279
Service request enable register (SRE) ..... 277
Remote ..... 287
Set frequency
RF signal ..... 140
Set IDN and OPT to default
Emulation mode ..... 112

Setting commands ..... 242
Setting not possible ..... 76
Setting parameters ..... 77
Settings

Check front panel ..... 493
Internal adjustments ..... 495
NRP sensor mapping ..... 168
Pulse generator ..... 231
Reference oscillator ..... 144
Setup

Accept security settings ..... 121
Annotation amplitude (security) ..... 119
Annotation frequency (security) ..... 119
Change password (security) ..... 117
Confirm password (security) ..... 117
Confirm password (user) ..... 117
Date ..... 492
Display lock (security) ..... 119
LAN services ..... 117
New password (security) ..... 117
New password (user) ..... 116
Old password (security) ..... 117
Old password (user) ..... 116
Performing the front panel test ..... 493
Secure update policy ..... 121
Security password ..... 117
Time ..... 492
Timezone ..... 492
User interface (security) ..... 119
User password ..... 116
Setup key ..... 97
Shape

LF generator ..... 206, 209, 213, 219, 225
RF Level Sweep ..... 191
Shape - LF Frequency Sweep ..... 229
Shape - RF Sweep ..... 184
Show level permanent - Power Sensors ..... 324
Signal valid as pulse sync

pulse generator ..... 235
pulse modulation ..... 235
Single Pulse ..... 232
Single trigger

Execute ..... 234
Slope

Pulse trigger input ..... 234
SMB (Samba)

LAN Services ..... 118
SMZ Info

FW-Version ..... 103
MAX Freq ..... 103
Min Freq ..... 103
Revision ..... 103
Serial number ..... 103
Type ..... 103
Software failure (100) ..... 505
Software options ..... 98
Source

FM ..... 208
Level Sweep Trigger ..... 188
LF sweep trigger ..... 226
List Mode ..... 194
PhiM ..... 212
Power sensors ..... 175
Pulse modulation ..... 216
Reference oscillator ..... 144
RF frequency sweep trigger ..... 180
Source - AM ..... 205

Source - Power Sensors ..... 328
Source impedance - LF output ..... 225
Source Import/Export
Pulse Train data ..... 239
User Correction data ..... 164
Spacing
Level Sweep ..... 192
Spacing - LF Sweep ..... 229
Spacing - RF Sweep ..... 184
Span (RF sweep) ..... 184
SPD ..... 247
SPE ..... 247
SRE ..... 274
SRE (service request enable register) ..... 277
SRQ ..... 508
SRQ (service request) ..... 277, 279
SSH
LAN Services ..... 118
Standby mode ..... 49
Start
Power Sensors ..... 322
Start Freq ..... 183
Start Freq - LF sweep ..... 229
Start Level ..... 190
Start/Stop Display Update ..... 103
State
Adjustment (reference oscillator) ..... 145
ALC ..... 155
LF Output ..... 224
List mode ..... 194
Low harmonic filter ..... 151
NRP level control ..... 157
PhiM ..... 212
Power sensors ..... 173
Pulse modulation ..... 216
Stereo modulation ..... 218, 421
Video-Sync signal ..... 232
State - User Correction ..... 160
Status
Network settings ..... 105
Queries ..... 280
Status byte
Remote ..... 284, 287
Status byte (STB) ..... 277
Status information ..... 499
Status Information ..... 101
status messages
FREQ OFFSET ..... 499
MOD OFF ..... 499
OVERLOAD ..... 500
RF OFF ..... 499
Status messages
ALC On ..... 501
AttFixed ..... 502
Auto ..... 501
BUSY ..... 500
EXT REF ..... 500
LEVEL OFFSET ..... 500
LevelSweep ..... 501
LFSweep ..... 501
ListMode ..... 502
LOC-LLO ..... 501
OvenCold ..... 502
REM-LLO ..... 501
REMOTE ..... 501
RFSweep ..... 501
S&H ..... 501

Table ..... 501
UCorr ..... 502
Status registers ..... 274
CONDITION ..... 276
ENABLE ..... 276
EVENT ..... 276
Model ..... 275
NTRansition ..... 276
Parts ..... 275
PTRansition ..... 276
Status reporting system ..... 273
Application ..... 279
Common commands ..... 284
STB ..... 274
Step
Level Sweep ..... 192
Step lin
LF sweep ..... 230
RF sweep ..... 185
Step log
LF sweep ..... 230
RF sweep ..... 185
Step width
Frequency ..... 141
Level Sweep ..... 192
LF sweep ..... 230
RF sweep ..... 185
Stereo modulation ..... 217
Stop bits
RS232 interface ..... 110
Stop Freq - LF Sweep ..... 229
Stop Freq - RF Sweep ..... 184
Stop Level ..... 191
Storage ..... 490, 491
String in remote commands ..... 268
Subnet mask
Network settings ..... 106
Suffixes ..... 264
Support ..... 506
Sweep
Retrace (LF frequency) ..... 230
Retrace (RF frequency) ..... 185
Retrace (RF level) ..... 191
RF frequency sweep ..... 180
Start Level ..... 190
Stop Level ..... 191
Trigger ..... 183, 190
Sweep - Center frequency ..... 184
Sweep - LF start frequency ..... 229
Sweep - LF stop frequency ..... 229
Sweep - Trigger ..... 228
Sweep output voltage ..... 186, 187
Sweep shape
RF Level Sweep ..... 191
Sweep shape - LF Frequency Sweep ..... 229
Sweep shape - RF Sweep ..... 184
Sweep spacing
Level Sweep ..... 192
Sweep spacing - RF Sweep ..... 184
Switching
On or off ..... 27
Synchronization bandwidth
reference oscillator ..... 145
Syntax elements
SCPI ..... 268

System
Identification ..... 111
IDN string ..... 112
Language ..... 111
Network Mac address ..... 107
Set IDN and OPT to default ..... 112
System directory ..... 305

##### T

Target level
NRP level control ..... 158
Test
Check front panel ..... 493
Test setup
Considerations ..... 23
Text parameters in remote commands ..... 267
Time
Setup ..... 492
UTC ..... 492
Timezone
Setup ..... 492
UTC ..... 492
Toggle Summary Screen ..... 108
Trigger
Event (remote) ..... 287
Trigger Input Slope ..... 187, 192, 198, 231
Trigger mode
Pulse modulation ..... 233
Trigger source
LF sweep ..... 226
RF frequency sweep ..... 180
Trigger Source
Level Sweep ..... 188
List Mode ..... 194
Triggering sweep manually ..... 190
Troubleshooting ..... 499
TS-USB1
USB adapter ..... 109
Type
Power sensors ..... 173
Type - Power Sensors ..... 329, 330

##### u

UCorr ..... 502
Ultr@VNC ..... 34
Uninterrupted level setting ..... 151
Unit
Power sensors ..... 174
Units
Input ..... 51
UNIX controller ..... 34
Unpacking the instrument ..... 20
UP ..... 266
Update sensor ..... 101
USB
Adapter TS-USB1 ..... 109
Interfaces ..... 245
Remote control interface ..... 240
USB Storage - Setup ..... 119
Use default aperture time
Power sensors ..... 177
Use LF connector to output sweep voltage ..... 186
Use peak power
NRP level control ..... 159

Use SParameter
NRP power control ..... 159, 177
NRP power viewer ..... 159, 177
Use SParameters - Power Sensors ..... 323
User Correction ..... 159
User Correction Data ..... 161
User interface
Lock (security) ..... 119
User manual ..... 17
User Name - Setup ..... 116
User password
Security ..... 116
Setup ..... 116
User variation
RF frequency settings ..... 141
Variation active ..... 141
Variation step ..... 141

#### V

Value - User Correction ..... 160
Variation Active ..... 141
Variation step ..... 141
Variation Step ..... 141, 152
Vertical bar ..... 268
Video-Sync signal state
    Pulse generator ..... 232
VISA ..... 240, 243
    Libraries ..... 241
    Resource string ..... 241, 243
VISA resource string
    Ethernet ..... 110
    GPIB ..... 110
    HISLIP ..... 110
    Serial ..... 110
    Socket ..... 110
    USB ..... 110
VNC
    LAN Services ..... 118
VNC connection ..... 34
Volatile mode
    Setup ..... 119
Voltage ramp
    RF sweep ..... 186
VXI protocol ..... 244

##### W

Wait
Remote ..... 288
Wait Time ..... 108
Warnings ..... 73, 502
Web Control
LXI ..... 45
White papers ..... 18
White space ..... 268
Width
Double pulse ..... 233
Pulse generator ..... 233
Workgroup
Network settings ..... 106

##### Z

Zero
Power sensors ..... 158, 174
Zero - Power Sensors ..... 330

Zeroconf
IP address ..... 505
Zoom
Pulse Train ..... 236
Zoom Position
Pulse Train ..... 236
