### 6.15 SYSTEM Subsystem

The SYSTEM subsystem contains a series of commands for general functions which do not directly affect signal generation.

:SYSTEM:ERROR:ALL?.....438
:SYSTEM:ERROR:CODE:ALL?.....439
:SYSTEM:ERROR:CODE[:NEXT]?.....439
:SYSTEM:ERROR:COUNT?.....440
:SYSTEM:ERROR[:NEXT]?.....440
:SYSTEM:ERROR:HISTORY?.....440
:SYSTEM:ERROR:HISTORY:CLEAR.....441
:SYSTEM:ERROR:STATIC?.....441
:SYSTEM:HELP:EXPORT.....441
:SYSTEM:DLOCK.....442
:SYSTEM:KLOCK.....442
:SYSTEM:ULOCK.....442
:SYSTEM:RCL.....443
:SYSTEM:SAV.....443
:SYSTEM:SECURITY:VOLMode[:STATE].....443
:SYSTEM:COMMUnicate:GPIB:LTERminator.....444
:SYSTEM:COMMUnicate:GPIB[:SELF]:ADDRESS.....444
:SYSTEM:COMMUnicate:NETWORK[:COMMON]:DOMain.....444
:SYSTEM:COMMUnicate:NETWORK[:COMMON]:HOSTname.....445
:SYSTEM:COMMUnicate:NETWORK[:COMMON]:WORKgroup.....445
:SYSTEM:COMMUnicate:NETWORK[:IPAddress]:DNS.....445
:SYSTEM:COMMUnicate:NETWORK:IPAddress:MODE.....445
:SYSTEM:COMMUnicate:NETWORK:IPAddress.....446
:SYSTEM:COMMUnicate:NETWORK[:IPAddress]:GATEway.....446
:SYSTEM:COMMUnicate:NETWORK[:IPAddress]:SUBNet:MASK.....446
:SYSTEM:COMMUnicate:NETWORK:MACAddress.....446
:SYSTEM:COMMUnicate:NETWORK:STATUS?.....447
:SYSTEM:COMMUnicate:NETWORK:RESTart.....447
:SYSTEM:NINFormation?.....447
:SYSTEM:COMMUnicate:GPIB:RESOURCE?.....447
:SYSTEM:COMMUnicate:NETWORK:RESOURCE?.....448
:SYSTEM:COMMUnicate:HISLip:RESOURCE?.....448
:SYSTEM:COMMUnicate:USB:RESOURCE?.....448

:SYSTEM:COMMunicate:SERial:RESource?.....448
:SYSTEM:COMMunicate:SERial:BAUD.....449
:SYSTEM:COMMunicate:SERial:PARity.....449
:SYSTEM:COMMunicate:SERial:SBITs.....449
:SYSTEM:COMMunicate:SOCKET:RESOURCE.....450
:SYSTEM:IDENTIFICATION.....450
:SYSTEM:IDENTIFICATION:PRESET.....450
:SYSTEM:IRESPonse.....450
:SYSTEM:ORESPonse.....451
:SYSTEM:NTP:HOSTName.....451
:SYSTEM:NTP:STATE.....451
:SYSTEM:LANGUAGE.....452
:SYSTEM:SECURITY:SUPOLICY.....452
:SYSTEM:PROTECT<ch>[:STATE].....452
:SYSTEM:REBOOT.....453
:SYSTEM:RESTART.....453
:SYSTEM:SHUTDOWN.....453
:SYSTEM:STARTUP:COMPLETE?.....453
:SYSTEM:DISPLAY:UPDATE.....454
:SYSTEM:DATE.....454
:SYSTEM:TIME.....454
:SYSTEM:TIME:ZONE.....455
:SYSTEM:TIME:ZONE:CATALOG?.....455
:SYSTEM:VERSION?.....455
:SYSTEM:OSYStem?.....455
:SYSTEM:MMEMory:PATH:USER?.....456
:SYSTEM:WAIT.....456

##### :SYSTEM:ERROR:ALL?

Queries the error/event queue for all unread items and removes them from the queue.

##### Return values:

<All> string

Error/event_number, "<Error/event_description>[;Device-dependent info]"

A comma separated list of error number and a short description of the error in FIFO order.

If the queue is empty, the response is 0, "No error"

Positive error numbers are instrument-dependent. Negative error numbers are reserved by the SCPI standard.

Volatile errors are reported once, at the time they appear. Identical errors are reported repeatedly only if the original error has already been retrieved from (and hence not any more present in) the error queue.

Example: SYST:ERR:ALL?

Queries all entries in the error queue.

Response: 0, 'no error'

No errors have occurred since the error queue was last read out.

Usage: Query only

##### :SYSTEM:ERROR:CODE:ALL?

Queries the error numbers of all entries in the error queue and then deletes them.

Return values:

<All> string

Returns the error numbers. To retrieve the entire error text, send the command :SYSTEM:ERROR:ALL?.

0

"No error", i.e. the error queue is empty

Positive value

Positive error numbers denote device-specific errors

Negative value

Negative error numbers denote error messages defined by SCPI.

Example: SYST:ERR:CODE:ALL

Queries all entries in the error queue.

Response: 0

No errors have occurred since the error queue was last read out.

Usage: Query only

#### :SYSTEM:ERROR:CODE[:NEXT]?

Queries the error number of the oldest entry in the error queue and then deletes it.

##### Return values:

<Next>

    string

    Returns the error number. To retrieve the entire error text, send the command :SYSTEM:ERROR:ALL?.

    0

    "No error", i.e. the error queue is empty

    Positive value

    Positive error numbers denote device-specific errors

    Negative value

    Negative error numbers denote error messages defined by SCPI.

Example: SYST:ERR:CODE

Queries the oldest entry in the error queue.

Response: 0

No errors have occurred since the error queue was last read out.

Usage: Query only

#### :SYSTEM:ERROR:COUNT?

Queries the number of entries in the error queue.

Return values:

<Count> integer

0

The error queue is empty.

Example: SYST:ERR:COUN

Queries the number of entries in the error queue.

Response: 1

One error has occurred since the error queue was last read out.

Usage: Query only

##### :SYSTEM:ERROR[:NEXT]?

Queries the error/event queue for the oldest item and removes it from the queue.

##### Return values:

<Next> string

Error/event_number, "<Error/event_description>[;Device-dependent info]"

Error number and a short description of the error.

If the queue is empty, the response is 0, "No error"

Positive error numbers are instrument-dependent. Negative error numbers are reserved by the SCPI standard.

Volatile errors are reported once, at the time they appear. Identical errors are reported repeatedly only if the original error has already been retrieved from (and hence not any more present in) the error queue.

Example: SYST:ERR?

Queries the oldest entry in the error queue.

Response: 0, 'no error'

No errors have occurred since the error queue was last read out.

Usage: Query only

Manual operation: See "History" on page 74

#### :SYSTEM:ERROR:HISTORY?

Queries the error history.

Note that the result can amount several kilobytes.

Return values:

<ErrorHistory> string

Example: SYSTEM:ERROR:HISTORY?
// 90,"Info;(*)Instrument startup... (Mar-13-2017/ 10:25:16-601 ms)",
90,"Info;(*)Information generated while processing license keys.,
Repaired Error!
COND: ( hr == false )
FILE: /home/sa_okbuildserver/jenkins/workspace/OK-Legacy-Distribution-30
ok_services_oklib/Src/CServiceExtension.cpp
LINE: 3554
ADDITIONAL INFO: Init ServiceExtension failed, 2877, -2147218613
HRESULT = 80001007
", 90,"[RF A] No frequency calibration data found.
Please run Adjust All!", ...
// returns all entries of the error queue

Usage: Query only

Manual operation: See "History" on page 74

#### :SYSTEM:ERROR:HISTORY:CLEAR

Clears the error history.

Example: SYSTEM:ERROR:HISTORY:ClEar // Deletes the history entries

Manual operation: See "Delete All" on page 74

##### :SYSTEM:ERROR:STATIC?

Returns a list of all errors existing at the time when the query is started. This list corresponds to the display on the info page under manual control.

##### Return values:

<StaticErrors> string
Example: SYSTEM:ERROR:STATIC?
    // -221,"Settings conflict", 153,"Input voltage out of range", ...
    // returns all static errors that are collected in the error queue
Usage: Query only

##### :SYSTEM:HELP:EXPORT

Saves the online help as zip archive in the user directory.

Example:

:SYSTEM:HELP:EXPORT
MMEM:CDIR?
// "/var/user"
MMEM:CAT?
// ...,"Log,DIR,4096","help.tgz,BIN,69836600"
// confirms that help zip archive is saved.

Usage: Event

:SYSTEM:DLOCK <DispLockStat>

Disables the display, or enables it again (OFF).

The command disables also the front panel keyboard of the instrument including the [LOCAL] key.

Parameters:

<DispLockStat> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 0)

Example: SYST:DLOC ON

Locks the display. To unlock the display SYST:DLOC OFF.

Manual operation: See "User Interface" on page 119

##### :SYSTEM:KLOCK <State>

Keyboard lock disables the front panel keyboard of the instrument including the [LOCAL] key, or enables it again (OFF).

The command disables also the front panel keyboard of the instrument including the [LOCAL] key.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 0)

Example: SYST:KLOC ON

Locks the front panel and external controls. To enable the controls, set SYST:KLOC OFF.

Manual operation: See "User Interface" on page 119

#### :SYSTEM:ULOCK <Mode>

Locks or unlocks the user interface of the instrument.

Parameters:

<Mode> ENABLEd | DONLy | Disabled

  ENABLEd

  Unlocks the display and all controls for the manual operation.

  DONLy

  Locks the controls for the manual operation of the instrument.

  The display shows the current settings.

  DISabled

  Locks the controls for the manual operation, and enables remote operation over VNC. The display shows the current settings.

*RST: n.a. (factory preset: ENABLEd)

Example: :SYST:ULOC DIS

Activates the user interface lock, including display and controls.

Manual operation: See "User Interface" on page 119

:SYSTEM:RCL <Pathname>

Loads a file with previously saved R&S SMB settings.

Loads the selected file with previously saved R&S SMB settings from the default or the specified directory. Loaded are files with extension *.savrcltxt.

Setting parameters:

<Pathname> string

Example: SYSTEM:RCL "/var/user/temp/Test"

  Loads the file Test.savrcltxt from the directory /var/user/temp/.

Usage: Setting only

Manual operation: See "Recall" on page 131

##### :SYSTEM:SAV <Pathname>

Saves the current R&S SMB settings into a file with defined filename and into a specified directory. The file extension (*.savrcltxt) is assigned automatically.

##### Setting parameters:

<Pathname> string

Example: SYSTEM:SAV "/var/user/temp/Test"

Saves the file Test.savrcltxt into the directory /var/user/temp/.

Usage: Setting only

Manual operation: See "Save" on page 129

##### :SYSTEM:SECURITY:VOLMode[:STATE] <SecPassWord>, <MmemProtState>

Activates volatile mode, so that no user data can be written to the internal memory permanently.

To enable volatile mode, reboot the instrument. Otherwise the change has no effect.

Parameters:

<MmemProtState> 0 | 1 | OFF | ON

*RST: 0

Setting parameters:

<SecPassWord> string

Current security password

The default password is 123456.

| Example: | SYSTEM:SECURITY:VOLMode:STATe "123456", 1 |
| --- | --- |
|  | SYSTEM:REBoot |
| Manual operation: | See "Volatile Mode" on page 119 |

#### :SYSTEM:COMMunicate:GPIB:LTERminator <LTerminator>

|  | Sets the terminator recognition for remote control via GPIB bus. |
| --- | --- |
|  | Parameters: |
| <LTerminator> | STANDARD \| EOI |
|  | EOI |
|  | The terminator must be sent together with the line message EOI (End of Line). This setting is recommended for binary block transmissions where a character could coincidentally have the value LF (Line Feed) but is not intended as the terminator. This setting must be selected for block data with undefined length. |
|  | STANDARD |
|  | An LF (Line Feed) is recognized as the terminator regardless of whether it is sent with or without EOI. |
|  | *RST: n.a. (factory preset: STANDard) |
| Example: | SYSTEM:COMMunicate:GPIB:LTERminator EOI Only a character which is sent simultaneously with the line message EOI is accepted as the terminator. |

#### :SYSTEM:COMMunicate:GPIB[:SELF]:ADDRESS <Address>

Sets the GPIB address.

|  | Parameters: |
| --- | --- |
| <Address> | integer |
|  | Range: 0 to 30 |
|  | *RST: n.a. (factory preset: 28) |
| Example: | SYSTEM:COMMunicate:GPIB:SELF:ADDRESS 28 Sets GPIB address. |
| Manual operation: | See "GPIB channel address" on page 109 |

#### :SYSTEM:COMMunicate:NETWORK[:COMMon]:Domain <Domain>

Sets the primary suffix, that is the DNS name without the host name part.

Parameters:

<Domain> string

Example: SYSTEM:COMMunicate:NETWORK:COMMON:DOMAIN 'ABC.DE' sets the domain of the network.

Manual operation: See "DNS Suffix" on page 107

##### :SYSTEM:COMMunicate:NETWORK[:COMMON]:HOSTname <Hostname>

Sets the individual host name of the R&S SMB.

Note: it is recommended that you do not change the host name in order to avoid problems with the network connection. However, if you change the host name be sure to use an unique name.

The host name is a protected parameter, To change it, first disable protection level 1 with command :SYSTEM:PROTECT<ch>[:STATE] on page 452.

##### Parameters:

<Hostname> string

Example: SYSTEM:PROTect1:STATe OFF,123456

SYSTEM:COMMunicate:NETWORK:HOSTname 'SIGGEN'

sets the individual computer name of the R&S SMB.

Manual operation: See "Hostname" on page 105

#### :SYSTEM:COMMunicate:NETWORK[:COMMON]:WORKgroup <Workgroup>

Sets the individual workgroup name of the instrument.

##### Parameters:

| <Workgroup> | string |
| --- | --- |
| Example: | SYSTEM:COMMunicate:NETWORK:COMMON:WORKgroup 'TEST_09' sets the workgroup name |

Manual operation: See "Workgroup" on page 106

#### :SYSTEM:COMMunicate:NETWORK[:IPAddress]:DNS <DNS>

Determines the net DNS server to resolve the name.

|  | Parameters: |
| --- | --- |
| <DNS> | string |
| Example: | SYST:COMM:NETW:IPAD:DNS 123.456.0.1 |
| Manual operation: | See "DNS Server" on page 107 |

##### :SYSTEM:COMMunicate:NETWORK:IPAddress:MODE <Mode>

Selects manual or automatic setting of the IP address.

##### Parameters:

| <Mode> | AUTO \| STATic *RST: n.a. (factory preset: AUTO) |
| --- | --- |
| Example: | SYSTEM:COMMunicate:NETWORK:IPADdress:MODE AUTO The IP address is assigned automatically (DHCP) |

Manual operation: See "Address Mode" on page 106

:SYSTEM:COMMunicate:NETWORK:IPADdress <IpAddress>
Sets the IP address.
Parameters:
<IpAddress> string
Range: 0.0.0.0. to ff.ff.ff.ff
Example:
SYSTEM:COMMunicate:NETWORK:IPADdress "7.8.9.10"
sets the IP address of the instrument.
Manual operation: See "IP Address" on page 106

:SYSTEM:COMMunicate:NETWORK[:IPAddress]:GATEway <Gateway>

Sets the IP address of the default gateway.

Parameters:

<Gateway> string

Range: 0.0.0.0 to ff.ff.ff.ff

Example: SYSTEM:COMMunicate:NETWORK:IPADdress:GATeway '1.2.3.4' sets the IP address of the default gateway.

Manual operation: See "Default Gateway" on page 107

##### :SYSTEM:COMMunicate:NETWORK[:IPAddress]:SUBNet:MASK <Mask>

Sets the subnet mask.

Parameters:

<Mask> string

Example: SYSTEM:COMMunicate:NETWORK:IPADdress:SUBNet:MASK '255.255.0.0' determines the subnet mask.

Manual operation: See "Subnet Mask" on page 106

#### :SYSTEM:COMMunicate:NETWORK:MACaddress <MacAddress>

Queries the MAC address of the network adapter.

|  | Parameters: |
| --- | --- |
| <MacAddress> | string |
| Example: | SYST:COMM:NETW:MAC queries the MAC address. |
| Manual operation: | See "MAC Address" on page 107 |

#### :SYSTEM:COMMunicate:NETWORK:STATus?

Queries the network configuration state.

<State> 0 | 1 | OFF | ON

Usage: Query only

Manual operation: See "Network Status" on page 105

##### :SYSTEM:COMMunicate:NETWORK:REStart

|  | Restarts the network connection to the instrument, terminates the connection and sets it up again. |
| --- | --- |
| Example: | SYSTEM:COMMunicate:NETWORK:RESTART |
| Usage: | Event |
| Manual operation: | See "Restart Network" on page 107 |

##### :SYSTEM:NINFormation?

Queries the oldest information message ("Error History > Level > Info") in the error/event queue.

##### Return values:

<NextInfo> string

Example: :SYSTEM:NINFormation?

Queries the oldest entry in the info message queue.

Response: 90,"Info;=== Instrument startup...

==="

Information message containing error number 90, that states,

that the instrument startup is complete.

Usage: Query only

##### :SYSTEM:COMMunicate:GPIB:RESOURCE?

Queries the VISA resource string for remote control via the GPIB interface.

To change the GPIB address, use the command :SYSTEM:COMMunicate:GPIB[:SELF]:ADDRESS.

##### Return values:

<Resource> string

Example: SYSTEM:COMMunicate:GPIB:RESOURCE? queries the VISA resource string. Response: "GPIB::28::INSTR"

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

##### :SYSTEM:COMMunicate:NETWORK:RESOURCE?

Queries the VISA resource string, used for remote control of the instrument with VXI-11 protocol.

##### Return values:

<Resource> string

Example: SYSTEM:COMMunicate:NETWORK:RESOURCE:Response:TCPIP::192.1.2.3::INSTR

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

##### :SYSTEM:COMMunicate:HISLip:RESOURCE?

Queries the VISA resource string, used for remote control of the instrument with HiSLIP protocol.

##### Return values:

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

##### :SYSTEM:COMMunicate:USB:RESOURCE?

Queries the VISA resource string for remote control via the USB interface.

##### Return values:

<Resource> string

Example: SYSTEM:COMMunicate:USB:RESOURCE? queries the VISA resource string for remote control via the USB interface.

Response: "USB::72::000000::INSTR"

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

##### :SYSTEM:COMMunicate:SERial:RESOURCE?

Queries the VISA resource string for the serial remote control interface. This string is used for remote control of the instrument.

Example: SYSTEM:COMMunicate:SERIAL:RESOURCE:queries the VISA resource string. Response: "ASRL1::INSTR"

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

##### :SYSTEM:COMMunicate:SERial:BAUD <Baud>

Sets the baudrate for the serial remote control interface.

Parameters:

<Baud> 2400 | 4800 | 9600 | 19200 | 38400 | 57600 | 115200

*RST: n.a. (factory preset: 115200)

Example: SYSTEM:COMMunicate:SERial:BAUD 115200 Sets 115200 baudrate.

Manual operation: See "Baud Rate" on page 110

##### :SYSTEM:COMMunicate:SERial:PARity <Parity>

Sets the parity for the serial remote control interface.

Parameters:

<Parity> NONE | ODD | EVEN

*RST: n.a. (factory preset: NONE)

Example: SYST:COMM:SER:PAR NONE

Selects parity NONE.

Manual operation: See "Parity" on page 110

##### :SYSTEM:COMMunicate:SERial:SBITs <SBits>

Sets the number of stop bits for the serial remote control interface.

Parameters:

<SBits> 1 | 2

*RST: n.a. (factory preset: 1)

Example: SYST:COMM:SER:SBIT 2

Selects 2 stop bits.

Manual operation: See "Stop Bits" on page 110

##### :SYSTEM:COMMunicate:SOCKET:RESOURCE?

Queries the VISA resource string for remote control via LAN interface, using TCP/IP socket protocol.

##### Return values:

<Resource> string

Example: SYSTEM:COMMunicate:SOCKET:RESOURCE?

Response: "TCPIP::10.113.1.150::5025::SOCKET"

Usage: Query only

Manual operation: See "VISA Resource Strings" on page 110

#### :SYSTEM:IDENTification <Identification>

Selects the mode the instrument identification is performed.

Parameters:

<Identification> AUTO | USER

AUTO

The *IDN string and the *OPT string are set automatically.

USER

Enables the selection of user definable *IDN and *OPT strings.

*RST: n.a. (factory preset: AUTO)

Example:

SYST:IDN USER

Selects the user-defined identification string.

Manual operation: See "Mode" on page 111

##### :SYSTEM:IDENTification:PRESet

Sets the *IDN and *OPT strings in user defined mode to default values.

Example: SYSTEM:IDENTification USER SYSTEM:IDENTification:PRESet

Usage: Event

Manual operation: See "Set to default" on page 112

##### :SYSTEM:IRESponse <IdnResponse>

Defines the user defined identification string for *IDN.

Note: While working in an emulation mode, the instrument's specific command set is disabled, i.e. the SCPI command SYST:IRES is discarded.

##### Parameters:

<IdnResponse> string

Example:
    SYST:IDEN USER
    // Selects a user-defined identification
    SYST:IRES "Test Device"
    // Defines identification string 'test device'
    *IDN?
    // Response: 'test device'

Manual operation: See "IDN String" on page 112

##### :SYSTEM:ORESponse <OResponse>

Defines the user defined response string for *OPT.

Note: While working in an emulation mode, the instrument's specific command set is disabled, i.e. the SCPI command SYST:ORES is discarded.

##### Parameters:

<OResponse> string
Example:
    // Selects a user-defined identification
    SYST:ORES "Test Option"
    // Defines the OPT string 'test option'
    *OPT?
    // Response: 'test option'

Manual operation: See "OPT String" on page 112

##### :SYSTEM:NTP:HOSTname <NTPName>

Sets the address of the NTP server. You can enter the IP address, or the hostname.

|  | Parameters: |
| --- | --- |
| <NTPName> | string |
| Example: | SYSTEM:NTP:HOSTname "pool.ntp.org" |
| Manual operation: | See "NTP Address" on page 492 |

#### :SYSTEM:NTP:STATE <UseNtpState>

Activates clock synchronization via NTP.

Parameters:
<UseNtpState> 0 | 1 | OFF | ON
*RST: n.a. (factory preset: 0)
Example: SYSTEM:NTP:STATE 1
Manual operation: See "Use Time from NTP Server" on page 492

##### :SYSTEM:LANGUAGE <Language>

Sets the remote control command set.

The instrument can also be remote controlled via the command set of several other generators, for example HP generator. See the Application Note 1GP71 at the download area of the product site on the Internet.

Note: While working in an emulation mode, the instrument's specific command set is disabled, i.e. the SCPI command SYSTEM: LANGuage will be discarded.

The return to the SCPI command set of the R&S SMB can only be performed by using the appropriate command of the selected command set. For example, the HP command EX returns to the instrument-specific GPIB command set (selection SYST:LANG 'HPxxxx').

##### Parameters:

Parameters:

<Language> string

Example: SYSTEM:LANGUAGE "SCPI" sets the SCPI command set.

Manual operation: See "Language" on page 111

##### :SYSTEM:SECURITY:SUPolicy <SecPassWord>, <UpdatePolicy>

Configures the automatic signature verification for firmware installation.

Parameters:

<UpdatePolicy> STRict | CONFirm | IGNote

*RST: n.a. (factory preset: CONFirm)

Setting parameters:

<SecPassWord> string

Manual operation: See "Secure Update Policy" on page 121

#### :SYSTEM:PROTECT<ch>[:STATE] <State>[, <Key>]

Activates and deactivates the specified protection level.

##### Suffix:

<ch> Indicates the protection level.

See also Chapter 4.2.3.13, "Protection", on page 113.

##### Parameters:

<State> select

*RST: n.a. (factory preset: 1)

##### Setting parameters:

<Key>

    integer

    The respective functions are disabled when the protection level is activated. No password is required for activation of a level. A password must be entered to deactivate the protection level. The password for the first level is 123456.

Example: // to activate protection level

    SYSTEM:PROTECT1:STATE 1

    // internal adjustments or hostname cannot be changed

    // to unlock protection level 1

    SYSTEM:PROTECT1:STATE 0,123456

    // internal adjustments are accessible

Manual operation: See "Protection Level/Password" on page 114

##### :SYSTEM:REBoot

Restarts the firmware and the operating system.

Usage: Event

##### :SYSTEM:RESTart

Restarts the firmware. The operating system remains active.

Usage: Event

##### :SYSTEM:SHUTdown

Shuts down the instrument.

Usage: Event

##### :SYSTEM:STARtup:COMPLETE?

Queries if the startup of the instrument is completed.

##### Return values:

<Complete>

    0 | 1 | OFF | ON

    *RST: 0

Example:

// 1

// the startup of the instrument is completed

Usage: Query only

##### :SYSTEM:DISPLAY:UPDATE <Update>

Switches the update of the display on/off. A switchover from remote control to manual control always sets the status of the update of the display to ON.

##### Parameters:

<Update>

0 | 1 | OFF | ON

*RST: ON

Example: SYST:DISP:UPD OFF

switches update of displayed parameter values off.

Manual operation: See "Display Update is On/Off" on page 104

:SYSTEM:DATE <Year>, <Month>, <Day>

Queries or sets the date for the instrument-internal calendar.

This parameter is protected, in order to prevent accidental changes.

It can be accessed with protection level 1, see :SYSTEM:PROTECT<ch>[:STATE] on page 452.

##### Parameters:

<Year>

    <year>,<month>,<day>

    <Month>

        integer

        Range: 1 to 12

    <Day>

        integer

        Range: 1 to 31

    Example:

        SYST:DATE?

        Response: "2011, 05, 01"

        it is the 1st of May, 2011.



Manual operation: See "Date" on page 492

##### :SYSTEM:TIME <Hour>, <Minute>, <Second>

Queries or sets the time for the instrument-internal clock.

The parameter is protected, in order to prevent accidental changes.

It can be accessed with protection level 1, see :SYSTEM:PROTECT<ch>[:STATE] on page 452.

##### Parameters:

<Hour> 0...23,0...59,0...59

Range: 0 to 23

<Minute> integer

Range: 0 to 59

<Second> integer

Range: 0 to 59

Example: SYSTEM:TIME?

Response: "12,0,0" it is precisely 12 pm.

Manual operation: See "Time" on page 492

##### :SYSTEM:TIME:ZONE <TimeZone>

Sets the time zone. You can query the list of the available time zones with :SYSTEM:TIME:ZONE:CATalog?.

##### Parameters:

<TimeZone> string

Manual operation: See "Time Zone" on page 492

##### :SYSTEM:TIME:ZONE:CATalog?

Queries the list of available time zones.

##### Return values:

<Catalog>

Usage: Query only

Manual operation: See "Time Zone" on page 492

##### :SYSTEM:VERSION?

Queries the SCPI version the instrument's command set complies with.

##### Return values:

<Version> string

Example: SYST:VERS

queries the SCPI version.

Response: "1996"

The instrument complies with the SCPI version from 1996.

Usage: Query only

##### :SYSTEM:OSYStem?

Queries the operating system of the instrument.

##### Return values:

<operSystem> string

Example: SYSTEM:OSYStem?

Response: "Linux"

Usage: Query only

#### :SYSTEM:MMEMory:PATH:USER?

Queries the user directory, that means the directory the instrument stores user files on.

Return values:

<PathUser> string

Example: SYSTEM:MMEMORY:PATH:USER?

Response: "/var/user/"

Usage: Query only

#### :SYSTEM:WAIT <TimeMs>

Delays the execution of the subsequent remote command by the specified time.

This function is useful, for example to execute an SCPI sequence automatically but with a defined time delay between some commands.

Setting parameters:

<TimeMs> integer

Wait time in ms

Range: 0 to 10000

*RST: 0

Example: :SYSTEM:WAIT 10000

// waits 10s before resetting the instrument

*RST

Usage: Setting only

The TEST system contains the commands for performing selftest routines, and for direct adjustment of the hardware assemblies (:TEST:DIRECT).

