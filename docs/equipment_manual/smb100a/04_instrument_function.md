## 4 Instrument Function

### 4.1 Overview of Instrument Functions

This chapter explains the functions of the R&S SMB and the options available in the setting menus. The associated SCPI command is specified for each parameter (where applicable).

The description starts with the general instrument settings which do not directly affect signal generation. The majority of these settings can be accessed by means of front-panel softkey menus and not by means of function block menus.

The signal generation functions are then described, beginning with the functions which affect the RF signal ("RF" block) and the analog modulations ("Mod" block). The configuration of the modulation generators (LF generators and pulse generator) and of the LF sweep is offered in the "Mod Gen" block. The clock synthesis signal is set in the "Clock Synthesis" block.

The general instrument settings include various functions, such as:

• Setting a defined basic setup using the [PRESET] key

see Chapter 4.2.2, "Default Instrument Settings - Preset Key", on page 96

• Switching from remote control to manual control using the [LOCAL] key see Chapter 4.2.4, "Switching to Manual Control - Local Key", on page 123

- Configuring the generator and its interfaces in the "Setup" dialog - e.g. setting the GPIB address, starting an adjustment, querying instrument data see Chapter 4.2.3, "General Configuration of Instrument - Setup Key", on page 97

• Calling up the online help using the [HELP] key

see Chapter 4.2.7, "Help System - Help Key", on page 126

• Querying messages using the [INFO] key

see Chapter 4.2.6, "Messages - Info Key", on page 126

• Loading and storing complete instrument settings in the "File" menu

see Chapter 4.2.8, "Storing and Loading Instrument Data - File Key", on page 127

The RF signal and the reference oscillator are configured in the "RF" function block:

##### CW mode

see Chapter 4.3.1, "Overview of RF Signal", on page 136

##### • List mode

see Chapter 4.3.7.4, "List Mode", on page 192

##### • Frequency and Level Sweep mode

see Chapter 4.3.7.1, "Overview", on page 178

##### • Reference Oscillator

see Chapter 4.3.4, "Reference Oscillator", on page 142

##### • RF Level

see Chapter 4.3.5.1, "Overview of RF Level", on page 146

• RF Level Sweep

see Chapter 4.3.7.3, "RF Level Sweep", on page 187

##### • ALC

see Chapter 4.3.5.4, "Automatic Level Control - ALC", on page 153

##### • Power Sensors

see Chapter 4.3.6.2, "NRP Power Viewer", on page 170

- User Correction

  see Chapter 4.3.5.6, "User Correction", on page 159

The analog and external digital modulations are activated in the "Modulation" function block:

##### • Amplitude Modulation

see Chapter 4.4.2, "Amplitude Modulation (AM)", on page 204

Frequency Modulation

see Chapter 4.4.3, "Frequency Modulation (FM)", on page 207

• Phase Modulation

see Chapter 4.4.4, "Phase Modulation (PhiM)", on page 211

##### • Pulse Modulation

see Chapter 4.4.5, "Pulse Modulation (PM)", on page 214

##### • Stereo Modulation

see Chapter 4.4.6, "Stereo Modulation", on page 217

The internal LF generators, the LF frequency sweep and the pulse generator are configured in the "Mod Gen" function block:

##### • LF Frequency Sweep

see Chapter 4.5.3, "LF Frequency Sweep", on page 225

• LF output

see Chapter 4.5.2, "LF Output", on page 224

### 4.2 General Instrument Settings

#### 4.2.1 Overview of General Instrument Settings

This section describes the settings which do not directly affect signal generation. Most of these settings can only be accessed by means of menus which are opened using keys or key combinations on the external keyboard or keys on the front panel key emulation.

The general instrument settings therefore affect various functions, such as storing instrument settings using the [FILE] key or setting the GPIB address in the menu of the [SETUP] key. The order in which the descriptions are given corresponds to the layout of the keys on the front panel of the R&S SMB (from top left to bottom right).

#### 4.2.2 Default Instrument Settings - Preset Key

The [PRESET] key performs a defined instrument setup. All parameters and switching states are preset (also those of inactive operating modes). The default instrument settings provide a reproducible initial basis for further settings.

However, functions concerning the integration of the instrument in a measurement setup are not changed, for example the GPIB address or reference oscillator settings.

When the instrument is switched on, it is not the preset state that is active, but rather the instrument state that was set before the instrument was switched on.

User-defined instrument states can be accessed and stored in the "File" menu.

?

</div>

</div>

Resetting the instrument to the factory state is possible with the Factory Preset function.

##### Preset

Presets all parameters and switching states.

The following list gives an overview of the presets for the most important generator settings. The other presets can be found in the preset tables of the individual menus and the information accompanying the remote commands.

• "RF frequency" = 1 GHz

• "RF level" RF output switched off

"Level" = 30 dBm for instruments including an attenuator

"Level" = -5 dBm for instruments with no attenuator

• "Offsets" = 0

• "Modulations State" = Off

• Uninterrupted level settings are switched off "Level Attenuator Mode" = AUTO

• Internal level control "Level ALC" = AUTO

• User correction "Level Ucor" = OFF

• "LF output State" = Off

• "Sweep State" = Off

Settings that are not affected by the [PRESET] key

• Reference frequency settings ("Ref Oscillator" menu)

• Power on settings ("Level/EMF" menu)

• Network settings ("Setup" menu)

• GPIB address ("Setup" menu)

*IDN? Identification and emulation ("Setup" menu)

Password and settings protected by passwords ("Setup" menu)

• Start/Stop Display Update ("Setup" menu)

• Display and keyboard settings ("Setup" menu)

Remote command:

 $ ^{*} $RST on page 286

#### 4.2.3 General Configuration of Instrument - Setup Key

Setup

System
Reference Oscillator...
Internal Adjustments...
Hardware Config...
Software / Options...

The "Setup" menu provides access to basic instrument settings, regardless of the currently set operating mode or measurement. It contains information on the instrument's equipment, and comprises all settings for the general configuration of the instrument and its interfaces.

To access the "Setup" menu, press the [SETUP] key.

The "Setup" menu is divided into functional sections as follows:

"System": covers general instrument parameters.

• "Test": used to perform function tests.

● "Environment": used to configure the controller interfaces.

● "Remote": used to configure the remote control interfaces.

● "Protection": used to set the protection level for service functions and security settings.

- "Settings": used to save or recall instrument settings or to preset the instrument to factory settings.

Most submenus of this key can be accessed only via the [SETUP] key or the menu tree ([MENU] key), with the following exceptions:

- The "Reference Oscillator" dialog can also be accessed in the "RF" block and is therefore described in the section on this block (see Chapter 4.3.4, "Reference Oscillator", on page 142).

- The "Save/Recall" dialog can also be accessed with the [FILE] key and is therefore described in the section on this key (see Chapter 4.2.8, "Storing and Loading Instrument Data - File Key", on page 127.

##### 4.2.3.1 Hardware Config

In the "Hardware Config" dialog, the installed assemblies together with their variants and revision states can be displayed for servicing purposes.

To open the "Hardware Config" dialog, select "System" and press the [SETUP] or [MENU] key.

| Counter | Counter | Counter | Counter |
| --- | --- | --- | --- |
| Operation Time / h | Operation Time / h | Operation Time / h | 6 |
| Power On Count | Power On Count | Power On Count | 0 |
| Common Assembly | Common Assembly | Common Assembly | Common Assembly |
| Assembly | Part Number | Revision |  |
| SMB100A | 1406.6000k02 | --- |  |
| Basis Board | 1406.6600.00 | 00.00 |  |
| COM-FPGA |  | 00.00.00 |  |
| RF Assembly | RF Assembly | RF Assembly | RF Assembly |
| Assembly | Part Number | Revision |  |
| RF Board | 1406.7220.06 | 01.01 |  |
| MOD-FPGA |  | 20.20.00 |  |
| Baseband Assembly | Baseband Assembly | Baseband Assembly | Baseband Assembly |
| Assembly | Part Number | Revision |  |
| Stereo Coder | 1407.3240.00 | 00.00 |  |

Section "Counter" in the upper part of the menu shows the "Operation Time" (in hours) and the number of power-on ("Power On Counter").

The second part of the menu is a table that lists the installed assemblies. It is divided into the sections:

• "Common Assembly"

"RF Assembly"

☑ "Baseband Assembly"

##### Operation Time / h

Displays the operation time in hours.

##### Remote command:

:DIAGnostic:INFO:OTIMe? on page 294

##### Power On Count

Displays the number of power-on.

##### Remote command:

:DIAGnostic:INFO:POCount? on page 294

##### Assembly

The tables list the installed assemblies.

"Assembly" Assembly name

"Part Number" Part Number of assembly

"Revision" Revision state of assembly

Remote command:

:DIAGnostic<hw>:BGINfo? on page 293

##### 4.2.3.2 Software / Options

The "Software/Options" dialog shows the firmware version of the instrument software and all installed hardware and software options.

i

</div>

</div>

Software options purchased at a later stage can be activated with a keycode. The activation code is supplied with the software option.

How to install options is described in chapter 4 "Software Update / Installing Options" of the service manual.

The installation of hardware options purchased at a later stage is also described in the service manual. Most hardware options need to be installed at an authorized Rohde&Schwarz service shop.

To access the "Software/Options" dialog, select "System" and press the [SETUP] or [MENU] key.

The menu is divided into the following sections:

"Firmware"

"Hardware Options"

"Software Options"

| Package | More... |
| --- | --- |
| SMB100A FW | 02.05.19 beta (Release.. |
| R&S COMPASS | 2.1.59.0 (Release) |
| Hardware Options | Hardware Options |
| Option | More... |
| SMB-B106 | 9 kHz to 6 GHz |
| Software Options (Internal) | Software Options (Internal) |
| Option | More... |
| SMB-K22 | Pulse Modulator (Desi.. |
| SMB-K23 | Pulse Generator (Desi.. |

##### Firmware

The firmware section of the menu shows the firmware version and the version of the software platform.

Note: Your instrument is delivered with the latest firmware version available. Firmware updates and the "Release Notes" describing the improvements and modifications are provided on the Internet at the download site of the Rohde & Schwarz signal generator home page. This home page always offers the latest information on your signal generator, e.g. also on changes of the firmware update procedure.

Remote command:

n.a.

##### Hardware Options / Software Options

The tables in the sections "Hardware" and "Software" list the installed hardware and software options.

"Option" Short name of option

"Designation" Name of option

Remote command:

*OPT? on page 285

 $ ^{*} $IDN? on page 285

##### Versions

The "Versions" tab shows the versions of the technical specification of the R&S SMB and of the software components that comprise the firmware.

"Package" Name of the component.

"Version" Current issue of the component.

Remote command:

n.a.

##### Show Open Source Acknowledgments

Accesses the list of the used open source software packages and the corresponding verbatim license texts.

##### 4.2.3.3 Manage License Keys

This dialog is the central dialog for managing licenses, like enabling newly purchased and/or newly registered options or performing the required instrument related steps during the process of unregistration of licenses.

| Device ID | 1406.6000K02-000000-EC |
| --- | --- |
| Enter License Key | Enter License Key |
| Enter License Key | Enter License Key |
| Import License Key From File | Import License Key From File |
| Export Deactivation Response To File | Export Deactivation Response To File |

An option is ready to operate after it is enabled by means of a license key code supplied with the option. The license key is delivered as a file or on paper. Unregistered licenses must be registered for a particular instrument prior to the corresponding option can be enabled for operation.

i

</div>

</div>

##### License Registration

If your purchased license is delivered unregistered, you must register it before you can activate the option.

For detailed information about the license registration, refer to the installation instructions provided with the option (Supplement A) and the documentation of the online tool "Manage Licenses" (https://extranet.rohde-schwarz.com/service).

i

</div>

</div>

Only if the R&S SMB is equipped with an older firmware version, a firmware update prior to enabling the software option may be required. The information on the valid firmware versions for the purchased software option is provided together with the option.

The firmware update is described in the service manual, chapter 4.

##### Device ID

Displays the instrument specific identification number. The device ID is an unique string with the following structure:

<stock number>-<serial number>-<checksum>

##### Enter License Key

Type here the license key provided with the option.

For license keys delivered as a file, use Import License Key from File...

##### Import License Key from File...

Opens a dialog for selecting the file with the license key.

##### Export Deactivation Response to File...

Exports the generated deactivation response key to a file and opens a file management dialog to save the file. This key is required during the unregistration process.

##### Status Information

Displays status information.

##### 4.2.3.4 NRP Info/Update

The "NRP Info/Update..." dialog covers information on connected power sensors, like serial number, revision state and features of the particular sensor. You can directly update the sensor firmware.

Access:

▶ Select "Setup > System > NRP Info/Update…".

| NRP Info / Update | NRP Info / Update | NRP Info / Update | NRP Info / Update |
| --- | --- | --- | --- |
| Current Sensors | Current Sensors | Current Sensors | Rev: 17.11.27.03
Peak: Yes |
| Sensor Type | Serial Number | More... | Interface: LAN |
| NRP18SN | 101748 | 17.11.27.03 |  |
| NRP18AN | 100992 | 17.11.27.03 |  |

The "NRP Info / Update" dialog indicates the connected R&S NRP power sensors with specific information and contains the functions to update the firmware of a connected sensor.

The remote commands required to remotely configure the power sensor settings are described in Chapter 6.12, "SENSe, READ and INITiate Subsystems", on page 318.

##### How to update an R&S NRP sensor

To perform an R&S NRP sensor update proceed as follows:

1. Open the R&S website http://www.rohde-schwarz.com in section "Power Meters & Voltmeters > R&S NRP Sensors".

2. Select the respective sensor, e.g. R&S NRP18SN.

3. Select "Downloads > Firmware" and the offered firmware suitable for your sensor.

4. Transfer and save the firmware on the instruments, for example in the /var/user/ directory.

5. Connect the sensor to the R&S SMB and select "Setup > System > NRP Info Update" to open the dialog.

6. Select the sensor in the left sensor selection field.

7. Select the update file with "Select NRP File".

8. Start the update procedure with "Run Update".

The update starts and a bar indicates the progress.

##### How to restart an interrupted update of an R&S NRP sensor

An accidental removal of the sensor during the update process interrupts the update. If no other sensor is connected to the instrument, proceed as follows to restart the update process:

1. Do not reconnect the sensor but keep it ready to be connected.

2. In the "Setup > System > NRP Info Update" dialog, select "Rescue" in the left sensor selection field

3. Activate "Run Update".

4. Confirm query in message box

5. Connect sensor within 4 seconds

The update starts, a bar informs about the progress.

##### Current Sensors

Shows the sensors that are connected to the generator with information on serial number, the revision state and some features.

Tip: Click on a sensor to get quick information about the firmware version and whether this sensor measures the peak of the signal.

Remote command:

SENSE<ch>[:POWER]:TYPE? on page 329

SENSE<ch>[:POWER]:SVERsion? on page 329

SENSE<ch>[:POWER]:SNUMBER? on page 328

##### Update

Section "Update" provides access to the file system in order to select a file for an R&S NRP sensor update (Button "Select NRP File"), the selected file is indicated to the left of the button. On the left side, the sensor to be updated is selected.

Button "Run Update" starts the update.

Note: If the update is interrupted for example by accidental removal of the sensor during the process, the button "Rescue" appears. Thus, you can restart the update process.

Prerequisite is that no other sensor is connected to the instrument.

Refer to "How to update an R&S NRP sensor" on page 101 and "How to restart an interrupted update of an R&S NRP sensor" on page 102 for detailed instructions.

Remote command:

n.a.

##### 4.2.3.5 SMZ Info Update

The "SMZ Info/Update" dialog covers information on a connected frequency multiplier, like type, serial number revision state, frequency range and firmware version. You can directly perform an update of the multiplier firmware.

Menu
- Setup
- System
- Reference Oscillator...
- Internal Adjustments...
- Hardware Config...
- Software/Options...
- Manage License Keys...
- Start/Stop Display Update...
- NRP-Z Info/Update...
- SMZ Info/Update...
- Update...

To access the "SMZ Info/Update" dialog, perform one of the following

• On the front panel, press the [SETUP] key and select "SMZ > SMZ Info Update".

• In the block diagram, select "RF Block > Configure > SMZ Info Update"

| SMZ Info / Update |
| --- |

i

</div>

</div>

• Where you can find the description...

Since this section is relevant when an R&S SMZxxx frequency multiplier is connected, you can find a detailed description in the user manual of the frequency multiplier. It is included in the online help of the R&S SMB, but can also be found in pdf format on the CD, or on the R&S website http://www.rohde-schwarz.com/product/SMZ.html.

##### 4.2.3.6 Display Update

The "Start/Stop Display Update" dialog provides the possibility to switch off update of the displayed parameters to increase speed for certain settings.

The indicated values are not updated and may therefore differ from the intern, used values.

Start / Stop Display Update

### Display Update is

On

Please, be aware:

If "Display Update is OFF", the values of displayed parameters could differ from their internal, used values. This mode is mainly provided to support unjittered signal output, e.g. in fast sweep or list mode.

Remote state changes switch the "Display Update is" to ON.

##### Display Update is On/Off

Switches on/off the update of the displayed parameters.

Switching off the update of the displayed parameters increases the speed for certain settings.

Note: For optimum sweep performance with short dwell times and for fast settling times, it is recommended to switch off the display update.

Remote command:

:SYSTEM:DISPLAY:UPDATE on page 454

##### 4.2.3.7 Shutting Down and Rebooting the Instrument

The [Power On/Standby] front panel key switches the instrument from the standby to the ready state or vice versa. In remote operation form a remote computer or in manual control, the R&S SMB provides you with another possibility to shut the instrument down or to reboot the system.

▶ To access the required settings, select "Setup > Environment > Shut Down".

| Shut down | ✗ |
| --- | --- |
| Shut down | Shut down |
| Reboot | Reboot |

Remote control commands:

: SYSTEM: SHUTdown on page 453

: SYSTEM: REBoot on page 453

• see also :SYSTEM:REStart on page 453

##### 4.2.3.8 Network Settings

The "Network Settings" dialog shows the parameters relevant for identifying the instrument in a network. The R&S SMB is equipped with a network interface and can be connected to an Ethernet LAN (local area network).

How to connect the signal generator to the network is described in Chapter 3.1.7, "Connecting to LAN", on page 24.

#### NOTICE

##### Risk of network errors!

Connection errors can affect the entire network.

If your network does not support DHCP, or if you choose to disable dynamic TCP/IP configuration, you must assign valid address information before connecting the instrument to the LAN.

Contact your network administrator to obtain a valid IP address.

To access this dialog, press the [setup] or [menu] key and select "Environment > Network Settings".

| Network Settings | Network Settings | Network Settings |
| --- | --- | --- |
| Network Status | Network Status | Connected |
| Hostname | Hostname | rssma100a100021 |
| Workgroup | Workgroup | INSTRUMENT |
| Address Mode | Address Mode | Auto (DHCP) |
| IP Address | IP Address | 10.111.10.136 |
| Subnet Mask | Subnet Mask | 255.255.0.0 |
| Default Gateway | Default Gateway | 10.111.0.1 |
| DNS Suffix | DNS Suffix | rsint.net |
| DNS Server | DNS Server | 10.0.23.159 |
| MAC Address | MAC Address | 00 25 64 C3 31 82 |
| Restart Network | Restart Network | Restart Network |

In the "Network Settings" dialog, you can configure the settings of the general network environment and specific identification parameters of the instrument in the network.

The remote commands required to remotely configure the network are described in Chapter 6.15, "SYSTEM Subsystem", on page 437.

##### Network Status

Indicates that the instrument is connected to the network.

Remote command:

:SYSTEM:COMMunicate:NETWORK:STATUS? on page 447

##### Hostname

Displays the host name.

Each instrument is delivered with an assigned host name, a logical name which can be used instead of the IP address. With the default network settings, the IP address is allocated by the DHCP server. This address may change each time the instrument is reconnected. Unlike the IP address, the host name does not change.

Note: Since the host name of the instrument is a protected parameter, you must first unlock protection level 1 to enable the entry (see Chapter 4.2.3.13, "Protection", on page 113).

It is recommended that you neither change the default network settings nor the host name in order to avoid problems with the network connection.

However, if you change the host name be sure to use an unique name.

Remote command:

:SYSTEM:COMMunicate:NETWORK[:COMMON]:HOSTname on page 445

##### Workgroup

Sets the individual windows workgroup name of the R&S SMB. This parameter is required in case the instrument is integrated in a windows network.

Note: Since the workgroup name of the instrument is a protected parameter, you must first unlock protection level 1 to enable the entry (see Chapter 4.2.3.13, "Protection", on page 113).

Remote command:

:SYSTEM:COMMunicate:NETWORK[:COMMON]:WORKgroup on page 445

##### Address Mode

Selects the mode for assigning the IP address.

"Auto (DHCP)" Assigns the IP address automatically, provided the network supports DHCP (Dynamic Host Configuration Protocol)

The network used must support automatic assignment of the IP address via DHCP or APIPA (Zeroconf) in order to use this function.

"Static" Enables you to assign the IP address manually.

Remote command:

:SYSTEM:COMMunicate:NETWORK:IPAddress:MODE on page 445

##### IP Address

Displays the IP address.

By default, the R&S SMB is configured to use dynamic TCP/IP configuration and to obtain the whole address information automatically.

If the network does not support DHCP or the attempt does not succeed, the instrument tries to obtain the IP address via Zeroconf (APIPA) protocol. IP addresses assigned via Zeroconf start with the number blocks 169.254.*.*.

Note: An IP address that is assigned via the Zeroconf protocol while the network requires an IP address assigned via the DHCP server may cause network connection failures.

See Chapter 9.5, "Resolving network connection failures", on page 505.

To assign the IP address manually, select Address Mode "Static".

Remote command:

:SYSTEM:COMMunicate:NETWORK:IPADdress on page 446

##### Subnet Mask

Displays the bit group of the subnet in the host identifier.

To assign the subnet mask manually, select Address Mode "Static".

##### Remote command:

:SYSTEM:COMMunicate:NETWORK[:IPAddress]:SUBNet:MASK on page 446

##### Default Gateway

Displays the gateway address.

To assign the gateway address manually, select Address Mode "Static".

This address identifies the router on the same network as the instrument that is used to forward traffic to destinations beyond the local network.

##### Remote command:

:SYSTEM:COMMunicate:NETWORK[:IPAddress]:GATEway on page 446

##### DNS Suffix

Displays the primary DNS (Domain Name System) suffix, that means the DNS name without the host name part.

The DNS system uses the suffix for registration and name resolution to uniquely identify the instrument in the entire network.

To assign the DNS suffix manually, select Address Mode "Static".

##### Remote command:

:SYSTEM:COMMunicate:NETWORK[:COMMON]:DOMain on page 444

##### DNS Server

Determines the preferred server for name resolution. The DNS server contains the underlying numerical values that are required for name resolution of the host name as part of the IP address.

To select the DNS server manually, select Address Mode "Static".

##### Remote command:

:SYSTEM:COMMunicate:NETWORK[:IPAddress]:DNS on page 445

##### MAC Address

Indicates the MAC (Media Access Control) address, a unique identifier of the network adapter in the R&S SMB.

##### Remote command:

:SYSTEM:COMMunicate:NETWORK:MACAddress on page 446

##### Restart Network

Terminates the network connection to the instrument and subsequently re-establishes it.

Used this function to resolve network problems.

Note: Only the connection of the instrument to the network restarts, the network itself is not affected.

##### Remote command:

:SYSTEM:COMMunicate:NETWORK:REStart on page 447

##### 4.2.3.9 Display/Keyboard Settings

In the "Display/Keyboard Settings" dialog the power-save mode and external keyboard settings are made. It is opened using the [SETUP] or [MENU] key under "Environment".

| Display/Keyboard | X |
| --- | --- |
| Display Settings | Display Settings |
| Screen Saver | Active |
| Wait time | 10 Min |
| Toggle Summary Screen | Active |
| Gui Language | English (US) |
| USB Keyboard Settings | USB Keyboard Settings |
| Layout | English (US) |

##### Screen Saver Active

Activates/deactivates the screen-save mode of the display.

If activated, the display including backlight is completely switched off after the elapse of the "Wait Time" when no entries via front panel, external mouse or external keyboard are made.

This mode is recommended for preserving the display especially if the instrument is exclusively operated via remote control.

##### Remote command:

:DISPLAY:PSAVE[:STATE] on page 296

##### Wait Time

Enters the idle time that must elapse before the display lamp is shut off when no entries are made.

##### Remote command:

:DISPLAY:PSAVE:HOLDoff on page 296

##### Toggle Summary Screen

Activates/deactivates the magnified frequency and level indication. If activated, the frequency and level indication covers the complete display.

Remote command:

n.a.

##### GUI Language

Selects the language of the graphical user interface.

Remote command:

n.a.

##### Layout (USB Keyboard Settings)

Selects the keyboard layout for the selected keyboard language.

The assignment of some keys depends on the selected layout and language.

##### Remote command:

:KBOard:LAYout on page 304

:KBOard: LANGuage on page 304

##### 4.2.3.10 Remote Channel Settings

The "Remote Channel Settings" dialog provides access to the settings for remote control. The dialog is opened using the [SETUP] or [MENU] key under "Remote".

▶ To access this dialog, press the [setup] or [menu] key and select "Remote > GPIB,...".

| Remote Channel Settings | Remote Channel Settings | Remote Channel Settings |
| --- | --- | --- |

The "Remote Channel Settings" dialog contains the GPIB address and displays the VISA resource strings provided for remote control via the various interfaces.

##### GPIB channel address

Sets the address of the GPIB channel the instrument is connected to.

##### Remote command:

:SYSTEM:COMMunicate:GPIB[:SELF]:ADDRESS on page 444

##### RS232 using USB adapter

Remote control via a serial interface is possible via a USB. The controller and the instrument must be connected with the external USB/serial-adapter R&S TS1-USB (see recommended extras in the data sheet) and a serial crossover (null modem) cable. A USB connection requires the VISA library to be installed on the controller. VISA will detect and configure the R&S SMB automatically when the USB connection is established.

In addition, you can also use a Bluetooth connection for remote control via the serial interface. The settings are effective for both interfaces (see also Chapter 4.2.3.14, "Security", on page 114).

##### Baud Rate ← RS232 using USB adapter

Sets the baudrate for the serial remote control interface.

##### Remote command:

:SYSTEM:COMMunicate:SERIAL:BAUD on page 449

##### Parity ← RS232 using USB adapter

Sets the parity for the serial remote control interface.

##### Remote command:

:SYSTEM:COMMunicate:SERIAL:PARity on page 449

##### Stop Bits ← RS232 using USB adapter

Sets the number of stop bits for the serial remote control interface.

##### Remote command:

:SYSTEM:COMMunicate:SERIAL:SBITS on page 449

##### VISA Resource Strings

Displays the VISA resource strings, used for remote control of the instrument. Each interface requires an individual unique address, to identify the instrument for remote control.

##### Remote command:

:SYSTEM:COMMunicate:HISLip:RESOURCE? on page 448

:SYSTEM:COMMunicate:NETWORK:RESOURCE? on page 448

:SYSTEM:COMMunicate:SOCKET:RESOURCE? on page 450

:SYSTEM:COMMunicate:GPIB:RESOURCE? on page 447

:SYSTEM:COMMunicate:USB:RESOURCE? on page 448

:SYSTEM:COMMunicate:SERIAL:RESOURCE? on page 448

##### Goto Local

Switches the instrument to operate in local control mode.

Switching from remote to local control mode can be also done with one of the following actions:

• manually with the [LOCAL] key on the front panel

• with the interface command &GTL via the remote control interface

• with the key combination [CTRL + Q].

Remote command:

&GTL

##### 4.2.3.11 Instrument Emulations

It is also possible to remotely control the R&S SMB via the command set of another signal generator, as for example of an HP generator. With this function you can, for example, replace a signal generator with an R&S SMB in an automated test setup, without adjusting the command scripts used. You find all the remote control command sets supported by the R&S SMB in a selection list.

For more information on this topic, the application note 1GP89: Remote Emulation with the R&S SMB100A RF and Microwave Signal Generator describes in detail how to use this feature.

The selected instrument also defines the identification string that is retrieved with query *IDN?. In addition to the preset values, you can enter a user-defined identification string, for example to provide individual identification for each generator, like 'MY_R&S SMB' (see Mode and IDN String).

As any other parameter, you can additionally change the remote control command set to be emulated via the Language command. However, once you have switched to an emulation, the R&S SMB specific command set is disabled, that means this command is no longer effective. To return, you need to know the corresponding remote control command of the simulated instrument. If you emulate an HP generator for example, the HP command EX returns to the SCPI command set.

To access this dialog, press the [setup] or [menu] key and select "Remote > Instrument Emulations".

| Instrument Emulations | X |
| --- | --- |
| Language | SCPI |
| Mode | *IDN?/*OPT? Identification - AF2023 |
| Auto AF2024 |  |
| IDN String | AF2030 |
| Rohde&Schwarz,SMB100A,1406.6000k02/00000 0,3.1.17.1-03.01.113 beta (Debug) | Rohde&Schwarz,SMB100A,1406.6000k02/00000 0,3.1.17.1-03.01.113 beta (Debug) |
| OPT String | OPT String |
| SMB-B5,SMB-B106,SMB-K22,SMB-K23 | SMB-B5,SMB-B106,SMB-K22,SMB-K23 |

The "Instrument Emulations" dialog enables you to emulate a remote control command set of several other signal generators.

The remote commands required to remotely configure the emulation settings are described in Chapter 6.15, "SYSTEM Subsystem", on page 437.

##### Language

Selects the instrument whose remote command set is emulated by the R&S SMB.

Remote command:

: SYSTEM: LANGuage on page 452

##### Mode

Selects the way the instrument identification is performed.

"Automatic" Sets the "IDN String" and the "OPT String" automatically for the instrument selected with the parameter Language.

"User Defined" Enables you to define the "IDN String" and the "OPT String" for the instrument selected with the parameter Language.

Remote command:

:SYSTEM:IDENTification on page 450

##### Set to default

Enables you to reset the *IDN and *OPT strings in user defined mode, see "Mode" on page 111.

The default strings vary depending on the selected emulation mode (Language)

##### Remote command:

:SYSTEM:IDENTification:PRESet on page 450

##### IDN String

Indicates the identification string of the instrument when queried with the common command *IDN?.

To assign a user defined identification string, select Mode "User defined".

Remote command:

 $ ^{*} $IDN? on page 285

:SYSTEM:IRESponse on page 450 (user defined mode)

##### OPT String

Indicates the option string of the instrument as queried with the common command *OPT?.

In "User defined" (see Mode) IDN String, you can create a user defined option string in addition to the automatically created one.

##### Remote command:

*OPT? on page 285

: SYSTEM: ORESponse on page 451

##### 4.2.3.12 LXI Status

The "LXI - LAN eXtensions for Instruments - Status..." dialog displays the settings and status of the LAN and allows to reset the LAN connection.

For more information on LXI, see Chapter 3.1.17, "LXI Configuration", on page 39.

LXI - LAN eXten...ruments - Status X
LAN Status:

</div>

</div>

| Hostname | rssmx100a100020 |
| --- | --- |
| MAC Address | 00 90 b8 1a 11 cf |
| IP Address | 10.111.1.32 |
| LAN Reset | LAN Reset |

##### LAN Status

The LED indicates the LXI status.

"green"    normal operation

"green (flashing)"

device identification

"red"    LAN fault

Remote command:

n.a.

##### LAN Reset

Initiates the network configuration reset mechanism for the instrument and resets the hostname, MAC address, and IP address.

According to the LXI standard, a LAN Reset must place the following network settings to a default state:

| Parameter | Value |
| --- | --- |
| TCP/IP Mode | DHCP + Auto IP Address |
| Dynamic DNS | Enabled |
| ICMP Ping | Enabled |
| Password for LAN configuration | LxiWebIfc |

The LAN Reset for the R&S SMB also resets the following parameters:

| Parameter |  |
| --- | --- |
| Hostname | Instrument-specific host name |
| Description | RF and microwave signal generator |
| Negotiation | Auto Detect |
| VXI-11 Discovery | Enabled |

The LAN settings are configured using the instrument's LXI Browser Interface described in Chapter 3.1.17, "LXI Configuration", on page 39.

To open the "Instrument Home Page" (welcome page), type the instrument's computer name (host name) or IP address in the address field of the browser on your PC, for example http://10.111.10.175.

Note: Do not add the missing zeros in the IP address, while opening the Instrument Home Page.

Remote command:

n.a.

##### 4.2.3.13 Protection

This "Protection" dialog provides access to the unlocking of different protection levels.

Access:

▶ Select "Setup > Protection"

After power on the instrument, all protection levels are locked. To unlock the protection, the correct password must be entered, see "To unlock or lock a protection level..." on page 114.

| Protection | ☐☒ |
| --- | --- |
| Protection Level 1 | ☑ On |
| Protection Level 2 | ☑ On |
| Protection Level 3 | ☑ On |
| Protection Level 4 | ☑ On |
| Protection Level 5 | ☑ On |

The following functions are protected in the respective levels:

• Protection Level 1

Protects against accidental changes to certain settings, e.g. clock and date, network settings or instrument names. You can access this protection level with the password 123456.

• Protection Level 2

Provides access to the unlocking of protected service functions. It is accessible for authorized personnel of Rohde & Schwarz service departments.

• Protection Level 3-5

Are reserved for factory internal use.

##### To unlock or lock a protection level...

1. In the "Password" entry field, enter the password for the corresponding protection level.

2. Confirm with the [Enter] key.

The checkbox of the protection level is disabled, i.e. the protection is unlocked.

3. To lock a protection level again, select the checkbox.

##### Protection Level/Password

Locks or unlocks the corresponding protection level.

E.g. protection level 1 expands the functionality of the internal adjustment and to access the selftests.

The password is 123456.

For access to service functions of protection level 2, see the service manual of your R&S SMB.

Remote command:

:SYSTEM:PROTECT<ch>[:STATE] on page 452

##### 4.2.3.14 Security

The security concept of the R&S SMB helps you to protect your instrument against uncontrolled access and changes. All provided security services require that you enter the security password.

Provided security services are:

Password management secures controlled user access to the instrument

With the two-step password concept, you can assign a user-defined password for the operating system, as well as a security password for accessing the mass storage of the instrument.

For more information concerning the security password, see the description Resolving Security Issues when Working with an R&S SMB. You can find this document on the R&S SMB product page at "Downloads" > "Manuals".

• LAN Services secures controlled network access.

You can individually lock and unlock the supported LAN interface services, see "LAN Services" on page 117.

Remote control via LAN interface requires that the interface is activated, but you can enable the required services specifically.

• General security parameters as:

USB Storage secures controlled access to the mass memory of the instrument.

– Volatile mode protects against modification or deletion of data in the file system.

- Annotation frequency and amplitude prevents reading the display.

User Interface prevents front panel operation and/or reading the display

- Secure Update Policy check that verifies the integrity and origin of the firmware package to be installed.

– Bluetooth enables operation of the instrument via Bluetooth.

i

</div>

</div>

Changing the password for the operating system or the security password requires that you enter the old password, the new password and that you confirm the new password.

To assign the password, press the "Accept" button. This action can not be undone! Keep also in mind, that security settings are never reset, even if you perform a factory preset.

▶ To access this dialog, press the [SETUP] or [MENU] key and select "Protection" > "Security".

Security
Change User Password
Valid for VNC, FTP and SMB (Samba) access
User Name
Instrument
Old Password
New Password
Confirm Password
Change Password
Change Security Password
Old Password
New Password
Confirm Password
Change Password
Security Settings
LAN Services...
USB Storage
Enabled
Volatile Mode
Enabled
Annotation Frequency
Enabled
Annotation Amplitude
Enabled
User Interface
Confirm
Secure Update Policy
Security Password
Accept
Bluetooth

</div>

</div>

The "Security" dialog comprises the parameters for configuring the passwords, as well as the security settings of the mass storage and the LAN services.

i

</div>

</div>

The settings in this dialog will not be assigned until you enter the Security Password and confirm with the Accept button.

##### User Name

Indicates the user name used for access to the Linux operating system.

The user name and password are required for remote access to the instrument via VNC, FTP or SAMBA.

##### Change User Password

Allows you to change and confirm the user password.

##### Old Password ← Change User Password

Enters the current user password. The default password is "instrument".

Note: It is highly recommended to change the default user password before connecting the instrument to the network.

##### New Password ← Change User Password

Enters the new user password.

##### Confirm Password ← Change User Password

Confirms the new user password by reperating.

Note: The new password will not be assigned until you select the Change Password button.

##### Change Password ← Change User Password

Changes the user password accordingly.

Note: Keep in mind, that a changed password is never reset, even if you perform a factory preset.

##### Change Security Password

Enables you to change and confirm the security password.

##### Old Password ← Change Security Password

Enters the currently used security password. The default password is '123456'.

Note: It is highly recommended to change the default security password before connecting the instrument to the network.

The security password is required when changing the status of the USB and LAN interface.

##### New Password ← Change Security Password

Enters the new security password.

The security password may contain decimal characters only.

##### Confirm Password ← Change Security Password

Confirms the new password by repeating.

Note: The new password will not be assigned until you select the Change Password button.

##### Change Password ← Change Security Password

Changes the password accordingly.

Note: Keep in mind, that a changed password is never reset, even if you perform a factory preset.

##### LAN Services

Opens the "LAN Services" dialog for individually enabling or disabling the available LAN interface services.

| LAN Services | LAN Services | LAN Services | LAN Services |
| --- | --- | --- | --- |

##### Enable LAN Interface ← LAN Services

Enables the LAN interface in general, and thus provides remote access via all unlocked services.

Note: The activated LAN services will not be assigned until you enter the Security Password and confirm with Accept.

##### Enable LAN Services individually ← LAN Services

Enables or disables the following interface services individually.

##### "SCPI over LAN"

activates access over LAN to remotely control the instrument using SCPI (Standard Commands for Programmable Instruments) commands.

"VNC" activates access via VNC (Virtual Network Computing) interface, a graphical desktop sharing system that uses RFB protocol to remotely control the instrument.

"SSH" activates access via SSH (Secure Shell), a network protocol for secure data communication.

"HTTP" activates access via HTTP (Hyper Text Transfer Protocol), the application protocol for hypermedia information systems.

"FTP" activates access via FTP (File Transfer Protocol), used to transfer files from a host to the instrument and vice versa.

##### "SMB (Samba)"

activates access to SMB (Server Message Block), used for providing shared access to files, printers and serial ports of a network.

##### "Avahi (Zeroconf)"

Avahi (Zerocont)"

activates Avahi, a service for automatic configuration of the instrument in a network environment.

##### "Software Update"

allows updating the instrument firmware via the LAN interface. For more information on this topic see the release notes of the instrument, provided on the Internet at the download site or the Rohde & Schwarz Signal Generator home page.

##### USB Storage

Activates the access to external USB storage media.

This setting has no effect on a mouse or a keyboard, connected via USB.

Note: The setting will not be assigned until you enter the Security Password and confirm with Accept.

##### Volatile Mode

Activates write protection on the file system to prevent modification or erasure of valuable data.

Note: The setting will not be assigned until you enter the Security Password, confirm with Accept, and reboot the instrument.

##### Remote command:

:SYSTEM:SECURITY:VOLMode[:STATE] on page 443

##### Annotation Frequency

Enables/disables the display of the currently used frequency in the header of the instrument.

Note: The setting will not be assigned until you enter the Security Password and confirm with Accept.

##### Remote command:

:DISPLAY:ANNotation:FREQUency on page 295

##### Annotation Amplitude

Enables/disables the display of the currently selected level in the header of the instrument.

Note: The setting will not be assigned until you enter the Security Password and confirm with Accept.

##### Remote command:

:DISPLAY:ANNotation:AMPLitude on page 295

##### User Interface

Allows you to lock the manual of the controls of the instrument, and to hide even the entire display.

The setting requires the entry of the security password 123456 and is only accepted after the "Accept" button is pressed.

Tip: Section "Enabling a locked user interface for manual operation" on page 120 describes how you can unlock the control elements and the user interface.

"Enabled" Enables the display and all controls for the manual operation of the instrument.

"VNC Only" Locks the keys at the front panel and externally connected keyboard and mouse.

The display on the screen remains and shows the current settings and changes.

Unlocking is possible via VNC or turning off and on again.

"Display only"

Locks the manual operation of the instrument. The display on the screen remains and shows the current settings and changes.

This security feature protects the instrument against unauthorized access, but still shows the current settings and processes, for example when you operate the instrument via remote control.

The function disables:

- the keys at the front panel of the instrument

• the external mouse and keyboard

The instrument indicates the locked controls by a padlock 📐 softkey in the taskbar.

##### "Disabled"

Locks the display and all controls for the manual operation of the instrument.

This security feature protects the instrument against unauthorized reading and access, for example when you operate the instrument via remote control.

The function disables:

• the display

- the keys at the front panel of the instrument

• the external mouse and keyboard

The screen shuts off and shows a padlock instead.

♡

</div>

</div>

##### Remote command:

: SYSTEM: ULOCK on page 442

: SYSTEM: DLOCK on page 442

: SYSTEM: KLOCK on page 442

##### Enabling a locked user interface for manual operation

To unlock the user interface for manual operation you have the following options:

- On the instrument's keypad or external keyboard, enter the security password 123456.

Even if you press any key, the instrument prompts you to enter the security password for unlocking.

Security password to unlock

Note The character of the first key you pressed is immediately added in the input field. Prior to inserting the password delete this entry.

- In remote control mode, send the command SYST:ULOC ENABLEd to release all locks at once.

Alternatively, you can use the command SYST:KLOC OFF to unlock the keyboard, or SYST:DLOC OFF to release the display.

Via remote control, there is no password required.

##### Remote command:

: SYSTEM: ULOCK on page 442

: SYSTEM: DLOCK on page 442

: SYSTEM: KLOCK on page 442

##### Secure Update Policy

Allows you to configure the automatic signature verification for firmware installation.

To apply the change: enter the security password and confirm with "Accept". Otherwise the change has no effect.

##### See also:

Chapter 4.2.3.14, "Security", on page 114 for more information on the security concept.

- The release notes for details on signature verification when installing new or former firmware versions, available at www.rohde-schwarz.com/firmware/smb100a.

##### "Confirm Unsigned"

Performs the signature verification.

If the check detects any discrepancies, the instrument issues a warning message. You can still update the firmware or reject updating. This setting also enables you to downgrade the firmware version.

"All Packages" Accepts all packages without signature verification.

##### "R&S Signed Packages"

Performs the signature check.

If the check detects any discrepancies, the instrument issues a warning message and locks the update to this firmware.

##### Remote command:

:SYSTEM:SECURITY:SUPolicy on page 452

##### Security Password

Enters the password that is required to enable or to disable the settings protected by a security password. Default is '123456'.

Note: It is highly recommended that you to change the default security password before connecting the instrument to the network.

All settings are only accepted after the "Accept" button is pressed.

##### Accept

Applies the modified settings, provided the security password is entered correctly.

Note: This action can not be undone. Keep in mind, that a changed password is never reset, even if you perform a factory preset.

##### Bluetooth Pin

Sets the Bluetooth pin of an external Bluetooth device. The pin is required to enable remote control via an external Bluetooth device.

Requires a Bluetooth adapter (recommended extra, see data sheet).

##### 4.2.3.15 Save/Recall

The "Save/Recall" submenu can also be called up with the [FILE] key and is therefore described in the section of this key (see Chapter 4.2.8, "Storing and Loading Instrument Data - File Key", on page 127).

##### 4.2.3.16 Factory Preset

The "Factory Preset" dialog provides a function to reset the instrument's settings to their factory state. This function is activated by pressing the "Execute Factory Preset" button.

| Factory Preset | ☐ |
| --- | --- |
| Execute Factory Preset |  |
| Resets instrument to factory configuration. Security settings and user data are not effected. | Resets instrument to factory configuration. Security settings and user data are not effected. |

##### Factory Preset

Reset the instrument's settings to their factory state.

Note: "Factory Preset" resets the "Remote Channel" and network settings to the default values.

Executing "Factory Preset" via remote control terminates the connection to the instrument, if these settings had been configured to values different to the default ones.

The factory preset function resets nearly all instrument settings. In addition to the regular preset by means of the [PRESET] key, a "Factory Preset" resets also the following values:

• Reference frequency settings ("Ref Oscillator" menu)

• Power on settings ("Level/EMF" menu)

• Network settings including hostname ("Setup" menu)

- Remote channel settings including GPIB address ("Setup" menu)

• Start/Stop display update ("Setup" menu)

• Display and keyboard settings ("Setup" menu).

To maintain security, password settings and all settings protected by these passwords like disabled USB and LAN connections are not changed.

Not affected by the "Factory Preset" are also user data, lists or instrument settings files, created for example by means of the Save/Recall function.

Remote command:

:SYSTEM:FPReset on page 289

##### 4.2.3.17 Help

The "Help" dialog offers comprehensive online help for the R&S SMB. A desired topic can be selected via the table of contents (select "Manual") or the index (select "Index").

For context-sensitive information about a marked parameter, press the [HELP] key. For a description of the "Help" menu, refer to the section covering to the [HELP] key (see Chapter 4.2.7, "Help System - Help Key", on page 126).

#### 4.2.4 Switching to Manual Control - Local Key

The local key switches from remote control to manual control (local state).

In remote control mode the instrument indicates the remote state in the display header. The rest of the display remains unchanged and shows the current instrument status, that means the status which exists under the remote control settings. The instrument can be operated (for example dialogs can be opened). However, it is not possible to enter or change values.

The status message additionally indicates whether the [LOCAL] key is disabled or enabled.

The following states are indicated:

"REMOTE"

The [LOCAL] key switches the instrument from remote control to manual control. The current command must be fully processed before the mode is switched, otherwise the instrument switches immediately back to remote control.

"REM-LLO"

The [LOCAL] key is locked, initiated by the &LLO (local lockout) command. The instrument can be switched from remote state to local state only via remote control, for example with &GTR or the Visual Basic command CALL IBLOC (generator%). The [LOCAL] key has previously been locked by the remote command &LLO.

When switching from remote to manual control, the display update function is automatically deactivated ("SETUP" > "Start/Stop Display Update" > "Off").

#### 4.2.5 Generating a Hard Copy of the Display

The save/recall function enables you to store the settings in a file. In addition, you can create a hard copy of the current display to save the most important settings of a performed signal generation in an image file.

##### 4.2.5.1 Hard Copy Settings

Creating a hard copy of the display requires that you have an external keyboard connected to the instrument.

▶ To access the dialog, use the key combination [CTRL+Z], or [CTRL+Y] depending on the used keyboard settings.

Hardcopy
/var/user/SMB20180924000.png
Automatic Naming On
Format PNG Options...
Save

</div>

</div>

The dialog contains the parameters for configuring the output format and location of a hard copy.

The remote commands required to define the hard copy settings are described in Chapter 6.8, "HCOPy Subsystem", on page 299.

##### Options

Opens the "Hard Copy Options" dialog for configuring the corresponding parameters (see "File Options" on page 125.

Remote command:

n.a.

##### File

Some configuration parameters are already offered in the "Hard Copy" dialog. All configuration parameters are available in "File Options" on page 125.

##### Automatic Naming

Activates automatic generation of the file name. Automatic naming is configured in the "Options..." sub dialog, see "File Options" on page 125.

Remote command:

:HCOPy:FILE[:NAME]:AUTO:STATE on page 303

##### File Info

Indicates the file name. The file name can be entered either manually via the file manager (button "File...") or generated automatically (Automatic naming checkbox). Automatic naming is configured in the "Options..." submenu.

Remote command:

:HCOPy:FILE[:NAME] on page 301

:HCOPy:FILE[:NAME]:AUTO:FILE? on page 302

##### 4.2.5.2 Hard Copy Options

This section describes the "Hard Copy Options" dialog.

| Hard Copy Options | Hard Copy Options | Hard Copy Options |
| --- | --- | --- |

##### File Options

Dialog for setting the file parameters.

"Format" Selects the output file format, for example *.bmp, *.jpg*.xpm*.png.

##### Remote command:

:HCOPy:IMAGE:FORMAT on page 300

:HCOPy:DEVICE:LANGUAGE on page 300

"Region" Selects the snapshot area, either the entire screen or the currently active dialog.

##### Remote command:

:HCOPY:REGION on page 303

"Automatic    If enabled, creates the output filenames automatically according to Naming" rules following the activated components.

"Path..." Selects the directory.

Note: To select the destination path, you have to specify a file name as well. Otherwise an error message is displayed and the selection is canceled.

Directory, path and file name are displayed in the infoline right to the "Path" button.

##### Remote command:

:HCOPy:FILE[:NAME]:AUTO:DIRECTORY on page 301

:HCOPy:FILE[:NAME]:AUTO? on page 301

"Clear Path" Deletes all image files with extensions bmp, img, png, xpm and csv in the directory set for automatic naming. Before deleting the image files a warning message is displayed requiring the confirmation.

##### Remote command:

:HCOPy:FILE[:NAME]:AUTO:DIRECTORY:CLEAR on page 302

"Prefix, Year, Month, Day"

Determines the rules for "Automatic Naming".

Per default, the automatically generated file name is composed of:

<Path>/<Prefix><YYYY><MM><DD><Number>.<Format>, where

Y, M and D mean Year, Month, Day; Number is the "Current Auto Number".

You can deactivate/activate each component separately.

The "Resulting File Name" indicates the current file name syntax.

##### Remote command:

:HCOPY:FILE[:NAME]:AUTO[:FILE]:PREFix:STATE on page 303

:HCOPY:FILE[:NAME]:AUTO[:FILE]:PREFix on page 303

:HCOPY:FILE[:NAME]:AUTO[:FILE]:YEAR:STATE on page 302

:HCOPY:FILE[:NAME]:AUTO[:FILE]:MONTH:STATE on page 302

:HCOPY:FILE[:NAME]:AUTO[:FILE]:DAY:STATE on page 302

"Current Auto

Number"

Indicates the number which is used in the automatically generated file name.

Note: When initially switching on the instrument the number is reset to the lowest possible value. Starting with number 0 the output directory is scanned for already existing files. As long as files with the same name are existing the number is increased by 1. The number is automatically set so that the resulting file name will be unique within the selected path. The current number is not in the save/recall file but will be temporarily stored within the database. At following save operations the number is increased.

Remote command:

:HCOPy:FILE[:NAME]:AUTO[:FILE]:NUMBER? on page 302

"Resulting File    Indicates the automatically generated file name."

Name"

Remote command:
:HCOPy:FILE[:NAME]:AUTO:FILE? on page 302

##### Save

Saves the hard copy.

Remote command:

:HCOPY[:EXECute] on page 301

#### 4.2.6 Messages - Info Key

The [INFO] key opens a window containing a detailed description of every message displayed in the info bar, see "Info Window" on page 73 and Chapter 9, "Status Information, Error Messages and Troubleshooting", on page 499.

#### 4.2.7 Help System - Help Key

The [HELP] key opens a browser window containing a context-sensitive description of the highlighted parameter.

Help - State
Contents Index Back
Instrument Function > Modulations > Amplitude Modulation (AM) > Amplitude Modulation Menu > State
State
Activates/deactivates AM modulation.
SCPI command:
[ : SOURce<hw>]: AM: STATE

The context-sensitive page which is opened with the [HELP] key is part of a comprehensive help system. It is possible to move from this context-sensitive page to any page of the help system. The following navigation aids are available:

##### • Internal links in the text

They open pages which are directly linked to the described function. In this way it is possible, for example, to call up the description of the GPIB command for any particular function.

##### • Back

The "Back" button calls up the page last viewed.

• Contents in the navigation panel

The contents list is used to open the individual help pages. It has a hierarchical structure. The highlighted line indicates where the currently displayed page is within the contents list.

• Index in the navigation panel

The index is used to call up all pages which contain the selected entry. The index has an alphabetical structure and also contains all GPIB commands.

##### • Find

The find function allows you to look for freely selectable terms in all help pages. A list of the pages containing the entered term is displayed as the search result. The search can be limited to words in the page title to increase the number of hits.

#### 4.2.8 Storing and Loading Instrument Data - File Key

The R&S SMB allows complete instrument settings to be saved in files either on the internal flash memory or on external USB memory devices.

Defined and complex instrument settings can then be reproduced at any time by loading this data. If required, these settings can be loaded to various signal generators.

The corresponding menu is available under "Save/Recall" in the "Setup" menu. The instrument settings are saved in files which can be saved in data directories.

Additionally there are intermediate memories in which the current instrument setting can be saved and then called up again by just pressing a key. This provides fast switching between different instrument settings.

Only settings which differ from the preset values and configuration data for the operating elements (e.g. window positions) are saved. As a result the files remain relatively small. Furthermore, instrument settings can easily be transferred between different equipped signal generators since the files contain only relevant information. When loaded, the referenced settings are implemented and all non-referenced parameters are set to the associated preset values.

If list data is part of the instrument settings, e.g. a list of user correction data, a reference to this list is saved, not the list itself. The list is reactivated when the associated settings are loaded, but the list may have been modified or deleted in the meantime or may not be available on a different instrument. If the list has been modified, the new entries will be used. An error message appears if an attempt is made to access a non-existing list or to activate settings which are not supported by the instrument.

i

</div>

</div>

• Network settings and remote settings are not saved and restored.

- Lists are saved and loaded in the appropriate menus. For example, the user correction data list is created and saved in the "User Correction" menu.

When loading an instrument setting, it is possible to select whether the current frequency and level setting is to be retained or whether the saved settings are to be activated. It is possible to delete saved instrument settings. A file can be copied by loading it with "Recall" and then storing it under a new name.

Settings can be transferred easily between instruments with different equipment options and/or firmware versions because only the settings which differ from the preset values are affected. When settings are loaded, only those which are possible on the instrument are implemented. Error messages indicate the settings which cannot be implemented.

The saved file is transferred from one instrument to another using the memory stick.

General file management functions such as copying and moving data are available in the "File Manager" dialog.

##### 4.2.8.1 Save/Recall Menu

The settings available in the File menu "Save/Recall" depend on the selected "Operation Mode".

Operation Mode Save ▼

For more information, see "File Select Dialog" on page 88.

##### Operation Mode

Selects the file function.

Accesses the settings for storing ("Save") and loading ("Recall") of the instrument settings.

"Save..."

Calls the menu for storing the current instrument setting (see Chapter 4.2.8.2, "Saving Instrument Settings", on page 128).

"Recall..."

Calls the menu for calling up a saved instrument setting (see Chapter 4.2.8.3, "Loading Instrument Settings", on page 130).

##### 4.2.8.2 Saving Instrument Settings

In "Operation Mode > Save", you can save the current instrument setting in a file.

| Save/Recall | Save/Recall | Save/Recall |
| --- | --- | --- |

##### Recent files

Displays the files last used.

##### Directory, File List and File Name

Note:

You access this generic standard function each time you perform one of the following:

• store or load (settings) files

• define a folder these files are to be stored in or

• navigate through the file system.

The name of the dialog is context sensitive but the provided functions are self-explanatory and very similar.

With the provided settings, you can perform the following:

- to navigate through the file system, use the directory tree

- to load and store files, use the dedicated functions "Select", "Save" and Recent files

- to perform standard file management functions, like create new directories, move, copy, delete files and/or directories, use the standard "File Manager" function (see "File Manager" on page 130).

##### Remote command:

to list all files in a directory:

:MMEMory:CDIRectory on page 309

:MMEMory:CATalog? on page 308

[:SOURCE]:CORRection:CSET:CATalog? on page 335

##### Save

Saves the current instrument settings in the specified file and path.

Remote command:

: SYSTEM: SAV on page 443

:MMEMory:STORE:STATE on page 313

##### Save Immediate x

Saves the current instrument setting in one of the three intermediate memories.

These instrument settings are retained until a different instrument setting is saved in the intermediate memory. When the instrument is switched off, the contents of the intermediate memories are retained.

Remote command:

* SAV on page 287

##### File Manager

Accesses the "File Manager" dialog, see Chapter 4.2.8.4, "File Manager",

on page 132.

Remote command:

n.a.

##### 4.2.8.3 Loading Instrument Settings

In "Operation Mode > Recall", you can load complete instrument settings, and select whether the current or saved frequency and level settings are to be used.

| Save/Recall | Save/Recall | Save/Recall | Save/Recall |
| --- | --- | --- | --- |
| Operation Mode | Recall | Recent files | Recent files |
| /var/user/Lists/SavRcl | /var/user/Lists/SavRcl | /var/user/Lists/SavRcl | /var/user/Lists/SavRcl |
| DME | DME | DME | DME |
| LevelCalData | LevelCalData | LevelCalData | LevelCalData |
| LfGenCalData | LfGenCalData | LfGenCalData | LfGenCalData |
| Rf | Rf | Rf | Rf |
| SavRcl | SavRcl | SavRcl | SavRcl |
| presetsettings | presetsettings | presetsettings | presetsettings |
| SvnCalihData | SvnCalihData | SvnCalihData | SvnCalihData |
| Exclude Frequency | Exclude Frequency | Exclude Level | Exclude Level |
| Recall | Recall | Recall | File |
| Imm 1 | Imm 2 | Mgr... |  |

##### Recent files

Displays the files last used.

##### Directory, File List and File Name

Note:

You access this generic standard function each time you perform one of the following:

• store or load (settings) files

• define a folder these files are to be stored in or

• navigate through the file system.

The name of the dialog is context sensitive but the provided functions are self-explanatory and very similar.

With the provided settings, you can perform the following:

- to navigate through the file system, use the directory tree

- to load and store files, use the dedicated functions "Select", "Save" and Recent files

- to perform standard file management functions, like create new directories, move, copy, delete files and/or directories, use the standard "File Manager" function (see "File Manager" on page 130).

Remote command:

to list all files in a directory:

:MMEMory:CDIRectory on page 309

:MMEMory:CATalog? on page 308

[:SOURCE]:CORRection:CSET:CATalog? on page 335

##### Exclude Frequency

The current frequency is retained when a saved instrument setting is loaded.

##### Remote command:

[:SOURce<hw>] :FREQUency[:CW|FIXED]:RCL on page 348

##### Exclude Level

The current level is retained when a saved instrument setting is loaded.

##### Remote command:

[:SOURCE<hw>] :POWER[:LEVEL][:IMMediate]:RCL on page 386

##### Recall

Loads the selected configuration.

If an instrument setting in which a sweep was activated is saved, the sweep starts when the recall command is called.

If an instrument setting which accesses lists is saved, this list is also loaded.

If the list has been deleted in the meantime, an error message appears when the instrument setting is loaded. If the list has been overwritten in the meantime, the new entries will be used.

##### Remote command:

:MMEMory:LOAD:STATE on page 312

: SYSTEM: RCL on page 443

##### Recall Immediate x

Loads the selected configuration from one of the three intermediate memories.

If an instrument setting in which a sweep was activated is saved, the sweep is started when the recall command is called.

If an instrument setting which accesses lists is saved, this list is also loaded.

If the list has been deleted in the meantime, an error message appears when the instrument setting is loaded. If the list has been overwritten in the meantime, the new entries will be used.

A message appears if no instrument configuration is saved in this memory.

Remote command:

 $ ^{*} $RCL on page 286

##### File Manager

Accesses the "File Manager" dialog, see Chapter 4.2.8.4, "File Manager", on page 132.

Remote command:

n.a.

##### 4.2.8.4 File Manager

The "File Manager" is a tool very similar to a standard Windows Explorer and helps you manage mass storage media and files saved on the R&S SMB.

You can perform the following tasks:

- Copying multiple files from disk to other media and vice versa, see Chapter 4.2.8.5, "Accessing the File System of the Instrument and Transferring Files from and to the Instrument", on page 133

• Copying files into another directory, see Copy and Paste

• Renaming and deleting files, see Rename and Delete

• Creating new directories on the following drives:

hard disk

– internal flash memory

- memory stick

See Create New Directory

File Manager
File Type
/var/user/Lists/SavRc
DME
LevelCalD
LfGenCal
Rf
SavRcl
presetsettings.savrcltxt
SynCalibData
Cut
Rename

Instrument State (.savrcltxt)
All Files (*.)
Instrument State (.savrcltxt)
ARB Waveform (.w)
NRP Setting (.nrp)
List Mode List (.lsw)
All Files (*)
Copy
Delete
Create New Dir

</div>

</div>

For more information, see "File Manager" on page 88.

##### File Type

Selects the file type to be listed. If you select a file type with a specific file extension, only files with this extension are listed in the directory.

Remote command:

n.a.

##### Directory and File Name

Selects the directory in which the file to be deleted or copied is located. The dialog lists all files in this directory. Selected files are highlighted. The path is indicated above the directory window.

Unlike the "Save/Recall" and "File Select" dialogs, the "File Manager" displays the full file names including extensions.

Remote command:

:MMEMory:CDIRectory on page 309

##### Cut

Cuts the selected file. It can be pasted into a different directory using the "Paste" button.

##### Remote command:

:MMEMory:DELete on page 311

##### Copy

Copies the selected file. It can be pasted into a different or the same directory using the "Paste" button. When pasting the file into the same directory file name Copy of <file name> is given automatically. When pasting the file into a different directory, the original file name is kept.

##### Remote command:

:MMEMory:COPY on page 309

##### Paste

Pastes the file that has been copied or cut before.

Remote command:

n.a.

##### Rename

Renames the selected file or directory. The new name can be entered in the "New File-name" dialog.

##### Remote command:

:MMEMORY:MOVE on page 312

##### Delete

Deletes the selected file. Before the file is deleted, a message appears prompting the user to confirm deletion of the file.

##### Remote command:

:MMEMory:DELete on page 311

##### Create New Directory

Creates a new directory. The name of the new directory can be entered in the "New Directory" dialog.

Note: When the subdirectory is entered, it is possible to enter an absolute path name (e.g. /var/MEAS) or the path relative to the current directory (e.g. .../MEAS).

The directory is created as a subdirectory in the selected level.

Remote command:

:MMEMory:MDIRectory on page 312

##### 4.2.8.5 Accessing the File System of the Instrument and Transferring Files from and to the Instrument

To access files and the file system of the instrument or to use the general file management functions such as copying and moving data, use the standard "File Manager" dialog.

To transfer files from and to the instruments or to exchange files, use one of the following alternatives:

• Connect a memory stick to one of the USB interfaces.

The instrument recognizes automatically a connected memory stick.

• Connect the instrument to a LAN.

For information on how to set up a LAN connection, refer to Chapter 3.1.7, "Connecting to LAN", on page 24.

An instrument connected to a LAN supports the standard file transfer methods from a remote client:

– FTP (file transfer protocol)

see "To access the file system of the R&S SMB via ftp" on page 134

– File sharing according to the SAMBA/SMB (server message block) protocol see "To access the file system of the R&S SMB via SMB (Samba)" on page 135

Both file transfer methods access the folder /var/user/share.

This section provides an introduction to this topic. For comprehensive information, refer to the Application Note 1GP72 "Connectivity of Rohde&Schwarz Signal Generators".

##### To access the file system of the R&S SMB via ftp

If the R&S SMB is connected to a LAN and the required configurations are completed, you can use File Transfer Protocol (ftp) to access the file system and to transfer files from and to the instrument.

1. Connect the instrument and the remote PC to a LAN.

2. Find out the "IP Address" of the instrument:

a) Select "Setup > Environment > Network Settings".

b) Write down the "IP Address" of the instrument, e.g. 10.113.10.105.

3. On the remote PC, start the Windows Explorer.

4. In the address field, enter ftp://<IP Address" of the Instrument>, e.g. ftp://10.113.10.105

A log on dialog opens and requests a password.

Tip: Default password. The FTP file access use the user instrument with default password instrument.

It is highly recommended that you change the user password in the "Security" dialog before connecting the instrument to the network!

See Chapter 4.2.3.14, "Security", on page 114.

5. Enter the password to access the /var/user/share directory.

| File | Edit | View | Favorites | Tools | Help |  |
| --- | --- | --- | --- | --- | --- | --- |
| Back | Back | Back | Back | Back | Back | Search |
| Address | Address | Address | Address | Address | Address | Go |
| Name | Name | Name | Name | Name | Name | Size Type |
| bin | bin | bin | bin | bin | bin | File Folder |
| share | share | share | share | share | share | File Folder |
| User: instrument | User: instrument | User: instrument | User: instrument | User: instrument | User: instrument | Local intranet |

You can access the files in the /var/user/ directory, perform standard function like creating directory, etc.

6. Open the /var/user/share directory and create a new directory, e.g. testftp.

7. On the instrument, press the [File] key and open the /var/user/share directory. The dialog displays the testftp directory.

| Save/Recall | Save/Recall | Save/Recall | Save/Recall | Save/Recall |
| --- | --- | --- | --- | --- |
| Select Operation | Recall | Recent files |  |  |
| d:/var/user/share | d:/var/user/share | d:/var/user/share | d:/var/user/share | d:/var/user/share |
| var/user/share | var/user/share | var/user/share | var/user/share | var/user/share |
| testftp | testftp | testftp | testftp | testftp |
| Exclude Frequency | Exclude Frequency | Exclude Level | Exclude Level | Exclude Level |
| Recall | Recall | Recall | Recall | File |
| Imm 1 | Imm 2 | Imm 3 | Manager... |  |

##### To access the file system of the R&S SMB via SMB (Samba)

The SMB (Samba) protocol is an alternative way to access the file system of the instrument form a remote PC, if both the instrument and the PC are connected to a LAN.

1. Connect the instrument and the remote PC to a LAN.

2. Find out the "IP Address" of the instrument:

a) Select "Setup > Environment > Network Settings".

b) Write down the "IP Address" of the instrument, e.g. 10.113.10.105.

3. On the remote PC, start the Windows Explorer and open the "Map Network Drive" dialog.

a) Select a valid "Drive", e.g. W.

b) In the "Folder" field, enter:

//<"IP Address" of the Instrument>/share or

//<"Hostname" of the Instrument>/share, e.g. //10.113.10.105/share

c) Select "Finish".

A log on dialog opens and requests an user name and a password.

4. Enter the user name and the password of your instrument.

The default user name and password is instrument.

Tip: Default password. The SAMBA/SMB file access use the user instrument with default password instrument.

It is highly recommended that you change the user password in the "Security" dialog before connecting the instrument to the network!

See Chapter 4.2.3.14, "Security", on page 114.

The /var/user/share directory of the instrument is mapped to and displayed as a network drive of the remote PC.

You can access the files in this directory, perform standard function like creating directory, storing files, etc.

### 4.3 RF Block

#### 4.3.1 Overview of RF Signal

Settings for the RF output signal and analog modulation are made under "RF Signal". These settings can be accessed in the block diagram by way of the "RF" function block, or by means of the menu with the same name which is opened using the [MENU] key.

RF
config...
On

</div>

</div>

The function block is available for the basic unit (R&S SMB + frequency option) without additional equipment options.

##### 4.3.1.1 RF Output

Basically, the RF output signal is deactivated. The previous state is restored, when the signal is reactivated.

##### Activating RF Signal Output

If the settings for the RF signal are done, you can activate RF signal output via:

• the [RF ON/OFF] key (the current entry focus is irrelevant)

• the checkbox in the "RF" block (see "RF On" on page 137)

- the "RF Frequency > RF ON" checkbox in the RF block (see "RF Output State" on page 137).

To open the menu, select the "Configure" button in the RF block.

The current state of the RF output (activated and deactivated) is indicated in the block diagram by means of the different block color and the status of the "On" checkbox.

An active sweep is also indicated in the block.

RF
config... RF
On

</div>

</div>

RF
config...
On

</div>

</div>

To query the impedance of the RF outputs, use the command :OUTPUT<hw>:

IMPedance? on page 316.

##### RF On

Activates RF signal output.

This function corresponds to the [RF ON /OFF] key.

See also Chapter 4.3.1.1, "RF Output", on page 136.

Remote command:

:OUTPUT<hw>[:STATE] on page 317

##### RF Output State

Activates the RF output signal by selecting the RF checkbox in the "Configure" dialog of the "RF" block.

RF Frequency
✓ RF ON
Frequency / Phase...

</div>

</div>

##### Remote command:

:OUTPUT<hw>[:STATE] on page 317

##### 4.3.1.2 RF Signal Modes and Characteristics

##### • CW

The RF signal is generated with the set frequency and level. This is the default mode.

• Sweep

The RF signal is generated as a sweep with the set parameters. It is not possible to activate frequency, level and LF sweep simultaneously.

##### • List Mode

The RF signal is generated on the basis of a list of predefined frequency and level values. The duration of the individual steps can be predefined.

Instruments connected downstream can be taken into consideration when setting the frequency and level by entering a frequency and/or level offset.

Automatic level control ("ALC") ensures maximum level accuracy.

User-specific lists which contain level correction values for any frequency range ("User Correction") can be created to, for example, compensate the cable attenuation in a test assembly setup.

The R&S SMB generates the RF signal in unmodulated or analog form. The signal generator is equipped therefore with the following sources for analog modulations:

• an internal LF generator

• an internal pulse generator

• the external modulation inputs [MOD EXT] and [PULSE EXT].

An external trigger signal for the sweeps and the LIST mode can be provided at the [INST TRIG] input.

The input [REF IN] is used to input an external instrument reference, and the output [REF OUT] serves as the output of the reference frequency (internal or external).

#### 4.3.2 RF Frequency

The value of the RF frequency is displayed in the header of the display ("Freq"). This field provides the direct input of the RF frequency. Alternatively, you can enter the RF frequency in the "Frequency/Phase" dialog.

Freq RF ON MOD ON Level
1.000 000 000 000 GHz -30.00 dBm

</div>

</div>

Note that the displayed RF frequency in the header, and the RF output frequency, entered in the "Frequency/Phase" dialog can be different, as explained in the following section.

##### 4.3.2.1 RF Frequency vs. RF Output Frequency

If you are working with a downstream instrument, e.g. a mixer or a frequency multiplier, you can enter the related parameter value in the frequency settings dialog ("Offset", "Multiplier").

The generator includes these parameters and displays the result in the "Freq" field in the status bar, as if the downstream instrument and the generator were one unit. This displayed frequency corresponds to the value at the RF output of the downstream instrument. However, the frequency provided at the RF output of the signal generator corresponds to the frequency value set in the "Frequency/Phase" dialog.

The instrument activates the "Freq Offset" icon in the status bar, when a frequency offset or multiplication factor is set.

The correlation between the RF frequency, the RF output frequency and the frequency offset is as follows:

"Freq" (in header) = "RF output frequency" (Frequency in dialog) * "Multiplier" factor (Multiplier in dialog) + "Freq offset" (Offset in dialog)

Downstream instrument:
- frequency offset / multiplication factor
- "Offset" / "Multiplier" in the settings dialog
RF output (downstream instrument)
Mixer/Multiplier
connector frequency
"Freq" in the display of the signal generator (status bar)
RF output:
- connector frequency
- "Frequency" in the settings dialog

</div>

</div>

i

</div>

</div>

If you have the R&S SMB equipped with one of the microwave frequency options R&S SMB-B112, -B120, -B131 or -B140, you can, in addition, operate an R&S SMZxx frequency multiplier.

xx represents the multiplier type that you can use according to the target frequency range.

Note: Instruments with option R&S SMB-B112 only support the R&S SMZ75(M/E) frequency multiplier models.

##### 4.3.2.2 Setting the RF Frequency

To change the RF frequency, press the [FREQ] key and enter the desired frequency. Changes to the RF frequency have an immediate effect (without confirmation with the [Enter] key) on the output signal.

##### RF Freq

Enters the RF frequency, considering the frequency offset.

Note: The SCPI command sets the level of the "Freq" display, that means an entered frequency offset and multiplier factor are considered in the frequency value.

##### Remote command:

[:SOURCE<hw>] :FREQUENCY[:CW|FIXED] on page 347

##### 4.3.2.3 RF Frequency Dialog

The combined "RF Frequency / Phase..." dialog contains the parameters required for configuring the frequency and settings like a frequency offset, or a multiplier factor of an externally connected multiplier, see Chapter 4.3.2.4, "Frequency Settings", on page 140.

| RF Frequency / Phase | X |
| --- | --- |
| Frequency Settings | Frequency Settings |
| Frequency | 403.000 000 000 MHz |
| Offset | 0.000 Hz |
| Multiplier | 1.000 |
| User Variation | User Variation |
| Variation Active | Variation Active |
| Variation Step | 1.000 000 000 MHz |
| Phase Settings | Phase Settings |
| Delta Phase | 0.0 deg |
| Reset Delta Phase Display | Reset Delta Phase Display |

Furthermore, the dialog provides additional settings parameters which are described in:

Chapter 4.3.3, "Phase", on page 142

##### 4.3.2.4 Frequency Settings

RF Frequency
√ RF ON
Frequency / Phase...

Access:

▶ Select "RF > config... > RF Frequency > Frequency/Phase".

| Frequency Settings | Frequency Settings | Frequency Settings |
| --- | --- | --- |
| Frequency | 1.000 000 000 000 | GHz |
| Offset | 0.000 | Hz |
| Multiplier | 1.000 | 1.000 |

In the upper section of the combined "RF Frequency / Phase ..." settings dialog, you can configure the frequency of the RF signal.

The remote commands required to define the settings are described in Chapter 6.13.4, "SOURCE:FREQUENCY Subsystem", on page 346.

##### Frequency

Sets the RF frequency of the RF output connector. The frequency entered and displayed here corresponds to the frequency at the RF output, that means any offset entry is not considered.

Note: Suppressed values in the status bar

For security concerns or certain operating modes, you can hide the frequency and level display in the status bar.

##### • *****

The display has been disabled for security reasons.

See:

- Annotation Frequency

- Annotation Amplitude

•
---
---

</div>

</div>

The display is disabled when list mode is running, see "State - List Mode" on page 194.

##### Remote command:

[:SOURCE<hw>] :FREQUENCY[:CW|FIXED] on page 347

Note: This command sets the frequency of the "FREQ" display, that is the frequency containing offset and multiplier.

##### Offset

Sets the frequency offset relative to the RF frequency. The frequency offset of a downstream instrument (for example a mixer) is entered.

The entry does not change the value of the RF frequency at the RF output. It only changes the RF frequency displayed in the display header. The value of the RF frequency in the header corresponds to the frequency at the output of the downstream instrument, see also Chapter 4.3.2.1, "RF Frequency vs. RF Output Frequency", on page 138.

Remote command:
[:SOURCE<hw>] :FREQUENCY:OFFSET on page 350

##### Multiplier

Sets the multiplication factor for the RF frequency.

In the frequency field of the status bar, the instrument adjusts its frequency display according to the set multiplication factor. This frequency value shows the frequency at the output of the downstream multiplier. The entry does not change the RF frequency at the RF output of the R&S SMB, see also Chapter 4.3.2.1, "RF Frequency vs. RF Output Frequency", on page 138.

Remote command:

[:SOURCE<hw>] :FREQUENCY:MULTIPlier on page 350

##### 4.3.2.5 User Variation Settings

##### Access:

▶ Select "RF > config... > RF Frequency > Frequency/Phase".

| User Variation | User Variation | User Variation |
| --- | --- | --- |
| Variation Active | Variation Active | Variation Active |
| Variation Step | 1.000 000 000 | MHz |

The combined "RF Frequency / Phase ..." settings dialog contains the parameters determine the step size for adjusting the frequency with the rotary knob.

##### Variation Active

Activates the user-defined step width used when varying the frequency value with the rotary knob.

The frequency value set with the rotary knob is varied using the user-defined step width which is entered under "Variation Step".

The frequency value set with the rotary knob is varied in steps of one unit at the cursor position (standard operating mode).

##### Remote command:

[:SOURCE<hw>] :FREQUENCY:STEP:MODE on page 353

##### Variation Step

Sets the user-defined step width. This step width is used when entering the RF frequency using the rotary knob. Frequency variation with this step width must also be activated with "Variation Active".

##### Remote command:

[:SOURCE<hw>] :FREQUENCY:STEP[:INCREMENT] on page 352

#### 4.3.3 Phase

The phase of the RF output signal can be changed in the "Phase Settings" section of the "RF Frequency/Phase" dialog.

##### 4.3.3.1 Phase Settings

RF ON
Frequency/Phase

</div>

</div>

▶ To access the dialog for configuring the phase settings, perform one of the following:

● Select "RF > config... > RF Frequency > Frequency/Phase".

• Press the [menu] key and select "RF > RF Frequency > Frequency/Phase".

| Phase Settings | Phase Settings | Phase Settings |
| --- | --- | --- |
| Delta Phase | 0.0 | deg |
| Reset Delta Phase Display |  |  |

The combined "RF Frequency / Phase ..." settings dialog contains the parameters to configure the phase settings of the RF signal.

The remote commands required to define the settings are described in Chapter 6.13.10, "SOURCE:PHASE Subsystem", on page 378.

##### Delta Phase

Sets the phase of the RF signal. The current phase of the signal is used as the reference. This function allows, for example, the phase of the output signal to be synchronized with the phase of a signal from a second signal generator.

Remote command:

[ :SOURCE<hw> ] :PHASE on page 378

##### Reset Delta Phase Display

Resets delta phase value. The set phase is adopted as the new current phase, i.e. the delta phase value is reset to 0.

Remote command:

[:SOURce<hw>] :PHASE:REFERENCE on page 378

#### 4.3.4 Reference Oscillator

The R&S SMB is equipped with an internal reference oscillator that generates a reference frequency of 10 MHz. It is used as internal reference source for the synthesizer and the local oscillator. Alternatively, you can apply an external reference signal.

Regardless of the used reference source (internal or external), the R&S SMB always provides the configured reference frequency at the output. You can use it, for example to synchronize several interconnected instruments.

i

</div>

</div>

The settings of the reference oscillator are not affected by an instrument preset ("PRE-SET" key).

The following examples briefly explain the possible test setups and the settings to be considered.

• Internal  $ f_{ref} = 10 $ MHz (10 MHz [REF OUT])

Rohe & Schwarz Signal Generator
10 MHz Internal Reference
10 MHz REF OUT
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10 MHz REF OUT
10 MHz REF IN
10

</div>

</div>

Figure 4-1: Synchronizing a subsequent instrument using the internal 10 MHz reference signal of the R&S SMB

The internal reference oscillator supplies the reference frequency.

Settings:

Source: "Internal"

• External  $ f_{ref} = 10 $ MHz (10 MHz [REF OUT])

If you have a clean external reference signal with 10 MHz frequency, you can directly pass it to the output. The signal quality remains the same.

10 MHz

</div>

</div>

Figure 4-2: Synchronizing instruments by means of an externally applied reference signal having 10 MHz

Settings:

Source: "External"

External Reference Frequency: "10 MHz"

Set the additionally provided parameters, as for example the synchronization bandwidth according to the requirements of the application.

• External  $ f_{ref} = 5/10 $ MHz (5/10 MHz [REF OUT])

If you have an external reference signal with 5 or 10 MHz, you can directly pass it to the output. The signal quality remains the same.

5.10 MHz

</div>

</div>

Figure 4-3: Synchronizing a subsequent instrument an externally applied reference frequency of 5 or 10 MHz

Settings:

Source: "External"

External Reference Frequency: "5 or 10 MHz"

##### Input and output connectors of the reference frequency

The appropriate connectors are located at the rear panel, see "[REF IN]" on page 57 and "[REF OUT]" on page 57.

##### 4.3.4.1 Reference Oscillator Settings

RF Frequency

✓ RF ON

Frequency / Phase...

Reference Oscillator...

To access the settings dialog for configuring the reference signal, perform one of the following:

• In the block diagram, select "RF > config... > RF Frequency > Reference Oscillator"

• Press the [menu] key and select "RF > RF Frequency > Reference Oscillator"

Press the [setup] key and select "Setup > System > Reference Oscillator"

| Reference Oscillator | Reference Oscillator | Reference Oscillator |
| --- | --- | --- |
| Source | Internal | Internal |
| Adjustment | Adjustment | Adjustment |
| Adjustment Active | Adjustment Active | Adjustment Active |
| Adjustment DAC Value | Adjustment DAC Value | 476 |

| Reference Oscillator | ✗ |
| --- | --- |
| Source | External |
| Deactivate RF Output (if external reference is missing) | ☐ On |
| External Reference Frequency | 10 MHz |
| Synchronisation Bandwidth | Wide |

In the "Reference Oscillator Settings" dialog, you can select the signal source and frequency to be used as the reference frequency, and determine a user-defined adjustment value.

The remote commands required to define the reference oscillator settings are described in Chapter 6.13.14, "SOURCE:ROSCillator Subsystem", on page 409.

##### Source

Selects the source of the reference frequency.

See Chapter 4.3.4, "Reference Oscillator", on page 142, which provides an overview of the different test scenarios for configuring the reference frequency.

"Internal" Uses the internal 10 MHz reference signal, either with the calibrated or a user-defined adjustment value.

"External" Uses an external reference signal. The frequency of the external reference signal must be selected under "External Reference Frequency" on page 145.

##### Remote command:

[:SOURce]:ROSCillator:SOURce on page 412

##### Deactivate RF Output (if external reference is missing)

Turns the RF output off when the external reference signal is selected, but no signal is supplied.

This function prevents that no improper RF signal due to the missing external reference signal is used for measurements. A message indicates that the external signal is missing and the RF output is deactivated.

This setting is not affected by a reset.

##### Remote command:

[:SOURCE]:ROSCillator:EXTERNAL:RFOFF[:STATE] on page 410

##### External Reference Frequency

Determines the frequency of the external reference signal.

You can select an external reference signal having a frequency of 5 MHz or 10 MHz.

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

##### Remote command:

[:SOURCE]:ROSCillator:EXTERNAL:FREQUENCY on page 410

##### Synchronization Bandwidth

Selects the synchronization bandwidth for an external reference signal.

"Narrow" Synchronization bandwidth is 50 Hz.

"Wide" The synchronization bandwidth is approximately 350 Hz. This mode is useful for very precise reference sources of high spectral purity.

##### Remote command:

[:SOURCE]:ROSCillator:EXTERNAL:SBANDwidth on page 411

##### Adjustment Active

Selects the adjustment mode.

"OFF" Uses the calibrated internal reference frequency.

"ON" Allows you to apply a deviation to the internal reference frequency, according to your requirements. To enter the value, use Adjustment DAC Value.

Remote command:
[:SOURCE]:ROSCillator[:INTernal]:ADJust[:STATE] on page 411

##### Adjustment DAC Value

Sets a user-defined deviation for the internal reference frequency. This value takes effect when it is activated with Adjustment Active. "0" represents the calibrated state. The setting range depends on the reference oscillator type and its factory calibration value.

Note: A factory preset resets this setting to the calibration value of the instrument.

Remote command:

[:SOURCE]:ROSCillator[:INTernal]:ADJust:VALUE on page 411

#### 4.3.5 RF Level

##### 4.3.5.1 Overview of RF Level

i

</div>

</div>

##### Message "Level overrange/underrange"

If this message appears in the status line, the set level ("Level") is out of range (see data sheet).

In this case, the signal level at the output can deviate from the set value.

Level
-30.00 dBm

</div>

</div>

The value of the RF level is displayed in the level field in the header of the display ("Level"). This field provides the direct input of the RF level value. Alternatively, you can enter the level in the "Level/EMF/..." dialog.

Note that the displayed RF level in the header, and the RF output level, set in the "Level/EMF" dialog can be different, as explained in the following section.

##### RF level vs. RF output level

If you are working with a downstream instrument, e.g. an attenuator or amplifier, you can enter the related parameter value in the level settings dialog ("Offset").

The generator includes these parameters and displays the result in the "Level" field in the status bar, as if the downstream instrument and the generator were one unit. This displayed level value corresponds to the value at the RF output of the downstream instrument. However, the level provided at the RF output of the signal generator corresponds to the level value set in the "Level/EMF/..." dialog.

The instrument activates the "Level Offset" icon in the status bar, when a level offset is set.

The correlation is as follows:

"Level" (in header) = "RF output level" (Level in menu) + "Level offset" (Offset in menu)

Downstream instrument:
- level offset
- "Offset" in the settings dialog
RF output:
- level at the connector
- "Amplitude" in the settings dialog
RF output (downstream instrument)
- output level
- "Lev" in R&S signal generator's display (status bar)

</div>

</div>

The RF output is protected against overloading by an external signal applied to the RF output (see Chapter 4.3.5.7, "Reverse Power Protection", on page 167).

##### Setting the RF level

To change the RF level, press the [LEVEL] key and enter the desired level. Changes to the RF level have an immediate effect (without confirmation with the Enter key) on the output signal.

##### RF Level

Enters the RF level, considering the level offset (see "RF level vs. RF output level" on page 146).

dBm, dBμV, mV and μV can be used as the level units. The four unit keys are labeled with these units.

Note: The SCPI command sets the level of the "Level" display, i.e. an entered level offset is considered in the level value.

##### Remote command:

[:SOURCE<hw>] :POWER[:LEVEL][:IMMediate][:AMPLitude] on page 384

##### 4.3.5.2 RF Level Dialog

RF Level
Level / Attenuator...

</div>

</div>

Access:

▶ Select "RF > config... > RF Level > Level/Attenuator".

| RF Level / EMF |
| --- |

The offset-free level, level offset and level limit are set in the top section of the dialog. The attenuator mode is set in the "Attenuator Settings" section.

In section "User Variation", you can determine the step size for adjusting the level with the rotary knob (with "Variation Active On").

The power-on behavior of the instrument and the level display in the display header are set in the "Power-On / EMF Settings" section (see Chapter 4.3.5.3, "Power-On/EMF Settings", on page 153).

The remote commands required to define the settings are described in Chapter 6.13.12, "SOURCE:POWER Subsystem", on page 382.

##### Level Settings

The offset-free level, attenuation mode, level offset and level limit are set in the top section of the dialog.

If you have the instrument equipped with a harmonic filter, you can also configure the filter.

##### RF Mode

Selects the level mode for signal output.

This function allows you, to optimize the RF output signal for applications, where improved harmonic suppression or a low Signal-to-Noise ratio is required.

Note: The modes "Low Distortion" and "Low Noise", for improving harmonic suppression or the S/N ratio require that an attenuator is fitted.

"Normal" The generator provides an RF output signal with high signal to noise ratio as well as low distortion, according to the data sheet.

"LOW Noise" This setting forces the generator to optimize the signal to noise ratio.

"LOW Distortion"

The generator reduces distortion (harmonics) of the RF signal.

##### Remote command:

[:SOURCE<hw>] :POWER:LMODE on page 387

##### Amplitude

Sets the RF level of the RF output connector.

The level entered and displayed here corresponds to the level at the RF output, that means any offset entry is not considered.

##### Note: Suppressed values in the status bar

##### • *****

For security concerns or certain operating modes, you can hide the frequency and level display in the status bar.

The display has been disabled for security reasons.

See:

- Annotation Frequency

- Annotation Amplitude

---

</div>

</div>

The display is disabled when list mode is running, see "State - List Mode" on page 194.

##### Remote command:

[:SOURCE<hw>] :POWER:POWER on page 388

Note: The SCPI command [ : SOURce<hw> ] : POWER[ : LEVel ] [ : IMMediate ] [ : AMPLitude ] sets the level of the "Level" display, that is the level containing offset.

##### Limit - RF Level

Sets an upper limit for the RF output power.

You can use it to protect your DUT from damage due to high input power. If you enter an RF level above this value, the instrument limits the output power to this specified value, and generates a warning message.

However, the level indication in the status bar is not affected.

Note: The limit value is always effective, regardless of whether you work with "NRP Power Control" or not.

The value is not affected by an instrument preset ([PRESET] key), *RST and the "Save/Recall" function. It is influenced only by the Factory Preset and the factory value is equal to maximum level.

##### Remote command:

[:SOURCE<hw>] :POWER:LIMIT[:AMPLitude] on page 386

##### Offset (Level)

Sets the level offset relative to the RF level.

The level offset of a downstream instrument (for example an attenuator or amplifier) is entered.

The entry does not change the value of the RF level at the RF output. It only changes the RF level displayed in the display header. The value of the RF level in the header corresponds to the level at the output of the downstream instrument.

Remote command:

[:SOURCE<hw>] :POWER[:LEVEL][:IMMediate]:OFFSET on page 385

##### Ignore Level Range Warnings

Suppresses warnings the instrument generates when either the level, or the PEP value are out of range. This function prevents automated measurements from being stopped due to a level warning.

The following warnings are suppressed in both, the history and in the error queue:

• Level overrange / level underrange

- PEP value greater than defined upper bound / PEP value less than defined lower bound (fix range)

Remote command:

[:SOURCE]:POWER:WIGNore on page 386

##### Low Harmonic Filter Settings

In 20 GHz or 40 GHz instruments (option R&S SMB-B120(L) /-B140(L)), you can install a low harmonic filter (option R&S SMB-B25 /-B26), to improve the harmonic performance.

See the website http://www.rohde-schwarz.com/product/smb100a.html or the data-sheet for more information on the options available according to the features of your instrument.

For a fitted low harmonic filter, you can define its operating mode in section "Low Harmonic Filter" of the "RF Level / EMF" dialog. Otherwise, the section is hidden.

RF Level
Level / Attenuator...
Low Harmonic Filter...

</div>

</div>

▶ To access the filter settings, select "RF > Config > RF Level > Low Harmonic Filter".

| Mode | Low Harmonic Filter | Low Harmonic Filter |
| --- | --- | --- |
| Manual | ▶ |  |
| State |  | On ▶ |

##### Mode

Selects between automatic or manual switching of the filter.

"Auto"

The filter switches automatically on and off, according to the given operating conditions. It is active within a certain frequency and level range and automatically turns off, if the frequency falls below the lower limit, or the level exceeds the upper limit.

The corresponding limit values are given in the data sheet.

Note: The State field shows the current state of the filter.

"Manual" In this mode, you can switch the filter individually.

##### Remote command:

:OUTPUT<hw>:FILTER:AUTO on page 315

##### State

Switches the filter in manual mode.

Note: If you work in the "Auto" Mode, this parameter shows the current state of the filter. If you then change the state, the operation mode automatically turns to manual operation.

##### Remote command:

:OUTPUT<hw>:FILTER[:LPASs]:STATE on page 316

##### Attenuator Settings

The R&S SMB can be configured to provide level settings without interruption. It is possible for instruments with or without step attenuator. The attenuator mode is set in the "Attenuator Settings" section of the "RF level / EMF" dialog.

##### Attenuator Mode

Sets the attenuator mode at the RF output.

"Auto" Standard mode.

"Fixed" When this operating mode is switched on, the attenuator, relays and amplifier stages are fixed in their current positions to provide level settings without interruption. The resulting variation range is defined and displayed under "Attenuator Fixed Range".

Note: The function is effective when automatic level control is activated ("ALC State = On").

If the normal variation range is overranged or underranged, level errors increase considerably and the warning "Level under/over-range" appears in the info line. The spectral purity of the output signal decreases with high attenuation.

##### Remote command:

:OUTPUT<hw>:AMODE on page 315

##### Fixed Range (PEP) In

Displays the level range in which the level is set without interruption for the "Attenuator Mode fixed" setting.

##### Remote command:

:OUTPUT<hw>:AFIXed:RANGE:UPPER? on page 314

:OUTPUT<hw>:AFIXed:RANGE:LOWER? on page 314

##### RF OFF Mode

Selects the attenuator mode, when the RF signal is switched off.

The setting of the RF OFF mode is not affected by an instrument preset ([PRESET] key), *RST and the "Save/Recall" function. This parameter is influenced only by the Factory Preset.

"Unchanged" Freezes the setting of the attenuator when RF is switched off. The attenuator is only activated when RF is switched on. This setting is recommended if a constant VSWR (Voltage Standing Wave Ratio) is required. Furthermore, on instruments equipped with a mechanical attenuator, it provides fast and wear-free operation.

##### "Full Attenuation"

Sets attenuation to maximum when the RF signal is switched off. This setting is recommended for applications that require a high level of noise suppression.

##### Remote command:

[:SOURCE<hw>] :POWER:ATTenuation:RFOFF:MODE on page 384

##### User Variation

If the level is set using the rotary knob, the step width is defined in the "User Variation" section.

##### Variation Active

Activates the user-defined step width used when varying the level value with the rotary knob.

"ON" The level value set with the rotary knob is varied using the user-defined step width which is entered under "Variation Step".

"OFF" The level value set with the rotary knob is varied in steps of one unit at the cursor position (standard operating mode).

##### Remote command:

[:SOURCE<hw>] :POWER:STEP:MODE on page 392

##### Variation Step

Sets the user-defined step width for entering the RF level using the rotary knob. Level variation with this step width must also be activated with "Variation Active".

##### Remote command:

[:SOURCE<hw>] :POWER:STEP[:INCRement] on page 391

##### External Level Adjustment

The external level adjustment provides information about the data that has been used for calibrating the RF level.

By default the instrument uses correction data obtained in the factory before delivery. In exceptional cases, you can determine the calibration values with an R&S NRP power sensor, and use these values for the external level correction. This feature is a protected function (see Service Manual, chapter 2, "Adjustment").

##### Adjustment Data

Indicates what data has been used for level calibration.

##### Remote command:

:CALibration<hw>:LEVel:EXTern:DATA on page 292

##### 4.3.5.3 Power-On/EMF Settings

RF Level
Level / Attenuator...

</div>

</div>

The power-on behavior of the R&S SMB and the level display in the display header are set in the "Power-On / EMF Settings" section of the "RF Level/EMF" dialog.

To open the "RF Level/EMF" dialog, select "RF > Configure > EMF" or use the [MENU] key under "RF".

Power-On / EMF Settings
Power-On State Previous Setting Display Level as Voltage of EMF

##### Power-On State - RF Signal

Selects the state which the RF output is to assume after the instrument is switched on.

The output is deactivated when the instrument is switched on.

"Previous Set- When the instrument is switched on, the output assumes the same thing" state as it had when the instrument was switched off.

Remote command:

:OUTPUT<hw>[:STATE]:PON on page 317

##### Display Level as Voltage of EMF - RF Level

Activates display of the signal level as voltage of the EMF (no-load voltage). If this setting is deactivated, the level is displayed as a voltage over a 50 Ohm load.

Note: This setting is not affected by an instrument preset ([Preset] key), *RST) or the "Save/Recall" function. Only the Chapter 4.2.3.16, "Factory Preset", on page 122 resets the setting.

Remote command:

[:SOURCE<hw>] :POWER:EMF:STATE on page 384

##### 4.3.5.4 Automatic Level Control - ALC

Your signal generator is equipped with an automatic level control unit to obtain best RF level accuracy.

Automatic Level Control (ALC) is an adaptive control system to stabilize the RF output level. It continuously monitors the current level and adjusts it to keep a steady state over temperature and time.

i

</div>

</div>

ALC is active in almost all applications by default. However, the Pulse Modulation mode excludes ALC, as the control loop would detect incorrect values and result in level deviations.

Also note that ALC may detect incorrect values in multi-transmitter test setups. If multiple generators are coupled, reverse power may affect the ALC readings. Based on incorrect values, ALC would have an impact on the signal to intermodulation ratio.

##### ALC States

The following description basically explains the ALC states and their principle of operation. In particular ALC OFF (Sample & Hold) gives an overview on the function in terms of the equipment of the generator.

The R&S SMB offers the ALC states:

##### • AUTO

automatically adjusts the output level to the operating conditions.

##### • On

enables ALC permanently, regardless of the currently selected mode.

##### • Off

deactivates ALC.

The instrument switches to Sample & Hold (S&H) state, which still allows to maintain a constant output level.

The following section explains the functionality of "Sample & Hold", to provide an overview and to indicate what is to be considered. "On" and "Auto" require no additional explanation. Furthermore, find the ALC state settings described in detail in State - ALC.

##### ALC OFF (Sample & Hold)

In "S&H" mode, the signal generator switches for a short period of time into CW mode and activates ALC. ALC adjusts the level to the set value and the generator holds the value (freeze). Then, the generator switches ALC off again and back to the operating mode.

RF output behavior during Sample & Hold depends on the configuration of your instrument. Instruments equipped with...:

##### • an electronic step attenuator

The level is decreased by 30 dB.

##### • a mechanical step attenuator

By default, the mechanical step attenuator is not switched during S&H cycles to optimize the settling time. The instrument provides the output power for 3 ... 5 ms. However, you can affect the attenuation at the output by the setting "RF during Power Search" to "Minimum", see RF During Power Search - ALC. Then the generator decreases the level by 30 dB with the mechanical attenuator. Note that this may take a certain period of time.

High frequency instruments, such as the R&S SMB with one of the high frequency options R&S SMB-B120 or R&S SMB-B140, are equipped with a mechanical step attenuator.

##### • no step attenuator

The signal generator outputs the set level for 3 to 5 ms after level or frequency setting during a Sample & hold measurement.

Instruments equipped with one of the options R&S SMB-B112L, R&S SMB-B120L or R&S SMB-B140L come without step attenuator.

The level control status is permanently displayed as a status message in the info line.

|  | ALC-Auto | Info |
| --- | --- | --- |

##### Automatic Level Control Settings

RF Level ___
Level / Attenuator...
Automatic Level Control...

</div>

</div>

To open the "Automatic Level Control" dialog, select "RF" > "Configure" > "Automatic Level Control" or use the [MENU] key under "RF".

The combined dialog "ALC / UCOR" is divided into the several sections and provides access to the "Automatic Level Control" settings and to function "User Correction", see Chapter 4.3.5.6, "User Correction", on page 159).

| Automatic Level Control | Automatic Level Control | Automatic Level Control |
| --- | --- | --- |
| State | On | ✓ |
| Rf During Power Search | Normal | ✓ |
| Search Once | Search Once |  |

##### State - ALC

Activates/deactivates internal level control.

"Auto" The instrument selects the most appropriate ALC mode automatically. ALC is on in most operating conditions. Default state.

"On" Activates ALC, regardless of the operating conditions.

##### "Off (Sample & Hold)"

Hold)" Deactivates internal level control.

Sample & hold closes the level control loop at every frequency and level change for a short period of time. The level control voltage is sampled and then clamped.

##### Remote command:

[ : SOURce<hw> ] : POWER: ALC [ : STATE ] on page 383

##### RF During Power Search - ALC

Activates the mode for the mechanical step attenuator and for output during ALC power search.

"Normal" The RF output is active during power search.

"Minimum" The RF output is inactive during power search.

##### Remote command:

:OUTPUT<hw>:ALC:SEARCH:MODE on page 315

##### Search Once - ALC

Forces the generator to execute level adjustment once, although the "Sample & Hold" mode is active.

##### Remote command:

[:SOURCE<hw>] :POWER:ALC:SONCe on page 383

##### 4.3.5.5 NRP Level Control

With the NRP Level Control function, you can achieve a very stable and accurate RF power supplied to your DUT. With the aid of a downstream control circuit, a CLPC (Closed Loop Power Control), you can detect frequency response characteristics of the used components, such as losses due to cables, modules or components like power amplifiers, and compensate these effects accordingly.

Example: How to set up a closed loop power control

R&S SMB100A Signal Generator

Amplifier
x dB
DUT
input level
Coupler

R&S NRPxx
Power Sensor

power control

target level
S Parameter (optional)

</div>

</div>

Figure 4-4: Example of a test setup with NRP Level Control

As shown in the example, the sensor measures a proportional power in defined time intervals, derivated from a coupler. It considers optionally given S-parameters and returns the results to the generator. The signal generator compares the measured level with the set value and adjusts its output level accordingly.

This allows you to control the external signal level continuously and reliably reach a constant input level at the DUT in real time.

i

</div>

</div>

##### Impact of the NRP Level Control and the Operating Modes

Since the frequency and level of the RF output signal are continuously adjusted during "NRP Level Control", this operating mode interferes those with varying frequency and level values.

The reason is, that the generator regularly transmits the output frequency to the connected R&S NRPxx power sensor, which in turn requests the signal generator to adjust the output level according to its measurement. In contrast to this real time control loop, for example the list operating mode already generates the RF output signal on previously optimized frequency and level value pairs. In this case, the "NRP Level Control" as a second control loop would impact the already determined RF signal values and also considerably slow down the measurement. Similar impacts occur in sweep mode, and also the "NRP Power Viewer" and "NRP Level Control" affect each other's functionality.

Hence, the operating modes exclude each other as follows:

• "NRP Level Control" automatically disables NRP Power Viewer, and vice versa.

- Activating the RF frequency sweep, RF level sweep or the list mode instantly deactivates a running "NRP Level Control".

- A running list or RF sweep mode blocks "NRP Level Control". It can not be activated.

Also keep in mind that modulated signals may differ from CW signals regarding mean power and peak power. This affects the operation of "NRP Level Control".

RF Level

Level / Attenuator...

Automatic Level Control...

NRP-Z Level Control...

User Correction...

##### NRP Level Control Settings

▶ To access the dialog for configuring the level control settings, perform one of the following:

● Select "RF > config... > RF Level > NRP Level Control".

• Press the [menu] key and select "RF > RF Level > NRP Level Control".

NRP Level Control (RF)
1 - NRP18SN S/N 101748
State On
Measured Level -54.53 dBm
Target Level -10.00 dBm
Rf Level Limit 30.00 dBm
Catch Range +/- 30.00 dB
Delay Time 0 ms
Use Peak Power On
Use SParameter On

</div>

</div>

The dialog contains all parameters for configuring the settings for level control test setup.

The remote commands required to define these settings are described in Chapter 6.13.12, "SOURCE:POWER Subsystem", on page 382

##### Sensor

Selects the R&S NRP power sensor for power control.

Note: In remote control, the sensors are set up using the SENSE commands. The remote measurement is triggered by the READ query which also provides the measurement results.

The software version of the connected power sensor can be retrieved by means of the remote control command :SENS:POW:TYPE?.

Use the "Setup >" Chapter 4.2.3.4, "NRP Info/Update", on page 101 dialog to update the sensor software.

##### Remote command:

[:SOURce<hw>] :POWER:SPC:SELECT on page 390

##### State

Activates power control using the selected sensor.

The control loop periodically adjusts the generator output. After switching off, the running loop is completed.

##### Remote command:

[:SOURCE<hw>] :POWER:SPC:STATE on page 390

##### Measured Level

Indicates the current reading of the sensor.

##### Zero - Power Sensors

Activates the auto zero function.

Zeroing calibrates the external power sensor by adjusting its reading at zero signal power. For this purpose, the RF power source must be switched off or disconnected from the sensor. If a Rohde & Schwarz power sensor receives an input power during the zeroing process, it aborts zeroing and generates an error message. Zeroing takes a few seconds, depending on the sensor model. Refer to the documentation of your power sensor for more information.

##### Tips for zeroing

When to perform zeroing:

• During warm up after switching on or connecting the instrument

• After a substantial change of the ambient temperature

• After fastening the power sensor module to an RF connector at high temperature

• After several hours of operation

- When low-power signals are to be measured, e.g. less than 10 dB above the lower measurement limit.

- Switch off the RF power source for zeroing, but do not disconnect it from the power sensor. This proceeding keeps the thermal equilibrium, and the zeroing process also compensates the noise that superimposes the measured signal (e.g. from a broadband amplifier).

Remote command:

:SENSE<ch>[:POWER]:ZERO on page 330

##### Target Level

Specifies the nominal level expected at the input of the sensor. The signal generator adjusts the output power accordingly, in order to meet the target value at the sensor input, and thus the power required at the DUT.

Remote command:

[:SOURCE<hw>] :POWER:SPC:TARGET on page 390

##### Limit - RF Level

Sets an upper limit for the RF output power.

You can use it to protect your DUT from damage due to high input power. If you enter an RF level above this value, the instrument limits the output power to this specified value, and generates a warning message.

However, the level indication in the status bar is not affected.

Note: The limit value is always effective, regardless of whether you work with "NRP Power Control" or not.

The value is not affected by an instrument preset ([PRESET] key), *RST and the "Save/Recall" function. It is influenced only by the Factory Preset and the factory value is equal to maximum level.

Remote command:

[:SOURCE<hw>] :POWER:LIMIT[:AMPLitude] on page 386

##### Catch Range +/-

Sets the capture range of the control system.

Within the range:

Target Level +/- Catch Range

the power control locks and tries to achieve the target level. Readings outside the range are not considered.

##### Remote command:

[:SOURCE<hw>] :POWER:SPC:CRANge on page 389

##### Delay Time

Defines a waiting period between the level adjustment of the generator and the next measurement of the power sensor.

With this parameter, you compensate any dead times in the controlled system.

##### Remote command:

[:SOURCE<hw>] :POWER:SPC:DELAY on page 389

##### Use Peak Power

Activates control by means of the peak power values, provided the power sensor supports this function. Otherwise, the dialog does not show this parameter.

##### Remote command:

[ : SOURce<hw> ] : POWER : SPC : PEAK on page 390

##### Use SParameter - Power Sensors

Activates the use of the S-Parameter correction data of the connected power sensor. For sensors with attenuator this checkbox is automatically checked.

Refer to the manual of the connected R&S NRP power sensor for a description on how to use the SParameter table.

##### Remote command:

:SENSE<ch>[:POWER]:CORRection:SPDevice:STATE on page 323

##### 4.3.5.6 User Correction

The "User Correction" function is used to create and activate lists in which level correction values predefined by the user are freely assigned to RF frequencies. Correction is performed by the user-defined table values being added to the output level for the respective RF frequency.

With frequencies which are not contained in the list, the level correction is determined by interpolation of the closest correction values.

The lists are created in the "List Editor". Each list is stored in its own file with the predefined file extension *. uco. The name of the User Correction file can be freely selected. The files are loaded from the "Lists..." file manager. Externally created tables with pairs of frequency and level values can be converted into User Correction files using the import function. The external files must have the file extension *.txt or *.csv. These file formats are provided e.g. by the Microsoft Excel program. The separators for table columns and for decimal floating-point numerals can be set. In addition,

internally created User Correction data can be exported into ASCII files using the export function.

The amplitude can also be linearized automatically by means of an R&S NRP power sensor connected to one of the generator output signals. With the aid of the "Fill with Sensor" function, a table with correction values for external test assemblies can be automatically determined, e.g. for compensating the frequency response of cables. The User Correction list with the correction values acquired by the sensor is generated in the "Edit User Correction List" menu. The correction values can be acquired any time irrespective of the modulation settings of the generator.

If user correction is activated, the "UCOR" display (User Correction) is shown in the header together with the "Level" display. The RF output level is the sum of both values.

"Level" + "UCOR" = Output level

If activated, user correction is effective in all operating modes.

RF Level

Level / Attenuator...

Automatic Level Control...

NRP-Z Level Control...

User Correction...

##### User Correction Menu

To open the "User Correction" menu, select "RF > Configure > User Correction" or use the [MENU] key under "RF".

The combined menu "ALC/UCOR" is divided into the several sections.

##### User Correction Settings

The "User Correction" settings are set in the most lower section of the combined dialog; this section is used to activate/deactivate user correction, and to create, select and activate the lists.

The upper section provides access to the automatic level control settings, see Chapter 4.3.5.4, "Automatic Level Control - ALC", on page 153.

| User Correction Settings | User Correction Settings |
| --- | --- |
| State | On |
| User Correction | 0.00 dB |
| User Cor. Data... | ucor |
| Edit User Cor. Data... |  |
| Import/Export >>> |  |

##### State - User Correction

Activates/deactivates user correction.

The "UCOR" status message appears in the frequency and level display.

##### Remote command:

[:SOURce<hw>] : CORRection[:STATE] on page 342

##### User Correction Value - User Correction

Indicates the current value for level correction.

##### Remote command:

[:SOURce<hw>] : CORRection : VALUE? on page 342

##### User Cor. Data - User Correction

Calls the "File Select" menu for selecting and creating a list or the "File Manager".

| Select User Correction Data |
| --- |
| New User Correction Data |
| File Manager... |

##### Remote command:

[:SOURCE]:CORRection:CSET:CATalog? on page 335

[:SOURce<hw>] :CORRection:CSET[:SELECT] on page 341

[:SOURCE]:CORRection:CSET:DELete on page 337

##### Edit User Cor. Data - User Correction

Calls the editor for editing the selected user correction list.

A list consists of any number of frequency/level value pairs. The currently selected list is displayed.

Each list is saved as a separate file with extension *.uco. The file name and the directory to which the file is saved are user-selectable.

Note: Save list only after filling both columns (frequency and level), otherwise the entries are lost.

| Edit User Correction Data ucor_list1 | Edit User Correction Data ucor_list1 | Edit User Correction Data ucor_list1 | Edit User Correction Data ucor_list1 |
| --- | --- | --- | --- |
|  | Frequency/Hz | Power/dB | Power/dB |
| 6 | 5 300 000.00 | 5.42 | 5.42 |
| 7 | 6 300 000.00 | 4.73 | 4.73 |
| 8 | 7 300 000.00 | 4.15 | 4.15 |
| 9 | 8 300 000.00 | 3.72 | 3.72 |
| 10 | 9 300 000.00 | 3.28 | 3.28 |
| Goto | Edit | Save | Save |

### "Frequency /Hz"

Enters the frequency to which the level correction value applies.

Note: The "Fill..." function allows to automatically enter any number of frequencies with freely selectable range and increment. Using the "Fill With Sensor" function of the "Edit" sub menu requires only the entry of the frequency values. The level values are automatically acquired by the connected power sensor.

##### "Power/dB"

Enters the level correction value to which the specified frequency applies. The values can be entered manually or automatically with the "Fill With Sensor" function (available in the "Edit" sub menu).

Selects row for editing.

"Goto"

| Goto First |
| --- |
| Goto Last |
| Goto Row... |

If Goto row is selected, a window opens for entering the requested row.

goto line:
OK    Cancel

</div>

</div>

"Edit" Calls a selection of possible actions described below.

| Insert Row |
| --- |
| Insert Range... |
| Fill... |
| Fill With Sensor... |
| Delete Row |
| Delete Range... |

"Insert Row" Insert a new row before the marked row.

"Insert Range" Insert new rows before the marked row. The number of rows to be inserted can be defined in an entry window.

Rows to Insert
OK | Cancel

</div>

</div>

"Fill...." Opens a sub menu for defining a set of list values to be automatically entered in the ucor list (see "Filling the Correction List automatically" on page 164).

"Fill With Sensor" Calls the menu to activate the filling of the user correction list with level values acquired by the selected power sensor (see "Filling the Correction List with Power Sensor Measurement Data" on page 165).

"Delete Row" Deletes the marked row.

"Delete  Allows to delete any number of rows starting with the marked row.

Range..." The number of rows to be deleted can be defined in an entry window.

Rows to Delete
OK | Cancel

</div>

</div>

"Save" The list is saved under its current name.

##### Remote command:

[:SOURce<hw>] :CORRection:CSET[:SELECT] on page 341

[:SOURce<hw>] :CORRection:CSET:DATA:FREQUency on page 335

[:SOURce<hw>] :CORRection:CSET:DATA:POWER on page 336

##### Import/Export

User correction list can be imported from externally created files or exported into text or CSV-files. The import/export settings are available after clicking the "Import/Export" button.

##### Import/Export >>>

Expands the menu with the area for import and export of user correction files.

Externally edited Excel tables with any number of frequency/level value pairs can be imported as text or CSV-files and used for user correction.

Conversely, you can also export internally created user correction lists as text or CSV-files.

| << Hide Import/Export | << Hide Import/Export |
| --- | --- |
| Import / Export UCOR Files | Import / Export UCOR Files |
| Mode | Import |
| ASCII File Settings | ASCII File Settings |
| Extension | TXT |
| Decimal Point | Point |
| Column Separator | Comma |
| Select ASCII Source | Select ASCII Source |
| None | None |
| Select Destination | Select Destination |
| None | None |
| Import | Import |

| << Hide Import/Export | << Hide Import/Export |
| --- | --- |
| Import / Export UCOR Files | Import / Export UCOR Files |
| Mode | Export |
| ASCII File Settings | ASCII File Settings |
| Extension | TXT |
| Decimal Point | Point |
| Column Separator | Comma |
| Select ASCII Destination | Select ASCII Destination |
| None | None |
| Select Source | Select Source |
| None | None |
| Export | Export |

##### Mode - User Correction

Selects if user correction lists should be imported or exported. The settings offered depend on the selected mode.

##### Remote command:

[:SOURCE<hw>] :CORRection:DEXChange:MODE on page 340

##### Extension - User Correction

Selects the file extension of the ASCII file to be imported or exported. Selection "TXT" (text file) or "CSV" (Excel file) is available.

##### Remote command:

[:SOURce<hw>] :CORRection:DEXChange:AFILe:EXTension on page 338

##### Decimal Point - User Correction

Selects the decimal separator used in the ASCII data between '.' (decimal point) and ',' (comma) with floating-point numerals.

##### Remote command:

[:SOURce<hw>] :CORRection:DEXChange:AFILe:SEParator:DECimal

on page 339

on page 339

##### Column Separator- User Correction

Selects the separator between the frequency and level column of the ASCII table the user correction list is exported to or imported from.

##### Remote command:

[:SOURCE<hw>] :CORRection:DEXChange:AFILe:SEParator:COLumn

on page 339

##### Select ASCII Source / Destination - User Correction

Calls the "File Manager" for selecting the ASCII file to be imported into a user correction list (source) or the ASCII file the user correction list is exported (destination) in.

##### Remote command:

[:SOURce<hw>] :CORRection:DEXChange:AFILe:SELECT on page 338

##### Destination / Source - User Correction

Calls the "File Manager" for selecting the user correction list to be exported (source) into an ASCII file or the destination for the ASCII file to be imported (destination) in.

##### Remote command:

[:SOURce<hw>] :CORRection:DEXChange:SELECT on page 341

##### Import / Export - User Correction

Starts the export or import of the selected file.

When import is selected, the ASCII file is imported as user correction list.

When export is selected, the user correction list is exported into the selected ASCII file.

##### Remote command:

[:SOURce<hw>] :CORRection:DEXChange:EXECute on page 340

##### Filling the Correction List automatically

The "Fill Table" menu enables you to automatically set the level correction values.

Fill User Correction Data
From 1
Range 3
Select column to fill
Start Value 0.00 Hz
End Value 0.000 000 000 000 Hz
Increment Value 0.000 000 000 000 Hz
Fill

</div>

</div>

The start line and the number of rows to be filled are defined under "From" and "Range."

The column to be filled is selected under "Select column to fill". Depending on the selection here, the default for start, end, and increment value are set. As the settings are interdependent, a change of one parameter may result in the automatic change of one or more of the other parameters. The filling of the column with the selected value settings is started with button "Fill".

i

</div>

</div>

The correction list entries are only computed when the "Fill" button is pressed.

##### From

Sets the start value of the index range.

Remote command:

n.a.

##### Range

Sets the range for filling the table.

Remote command:

n.a.

##### Select column to fill

Selects either the frequency or the level column to be filled with the value defined below.

Remote command:

n.a.

##### Start value

Sets the start value for the frequency or the level entries.

Remote command:

n.a.

##### End value

Displays the end value for the frequency or the level entries.

Remote command:

n.a.

##### Increment value

Sets the increment for the frequency or the level entries.

Remote command:

n.a.

##### Fill

Fills the selected column in the set range with values, starting with the start value and using the set increment.

Remote command:

n.a.

##### Filling the Correction List with Power Sensor Measurement Data

The level correction values for the user correction list can be acquired by means of R&S NRP power sensors. The R&S NRP sensors are connected to either the [SENSOR] connector or to one of the [USB] interfaces. Configuration of the connection is performed in the "Power Sensor" menu (see Chapter 4.3.6.2, "NRP Power Viewer", on page 170). The filling of the user correction list with measurement data is performed in the ucor list editor (see "Edit User Cor. Data - User Correction" on page 161).

In the editor, the frequencies for which the correction values are to be acquired are entered in the frequency column (either manually or by means of the "Fill…" menu).

i

</div>

</div>

Do not save the list at this point, because the frequency entries are lost as long as there are no entries for the level column also. In the following these entries are automatically acquired by the connected power sensor.

All level correction values for the given frequency values are measured using the Power Sensor and automatically filled in the selected list after the "Execute" button is pressed. The list is automatically stored and recalled again after filling.

##### Fill User Correction Data with Sensor Settings

The "Fill with Sensor" button of the "Edit User Correction Data" menu opens the associated menu.

Sensor 1 - NRP-Z11 S/N 900001
List To Fill d/UserCorrectionData
Execute
Used SMB Settings For Measurement
Modulation Off (CW)
Amplitude -30.00 dBm
Use SParameter Off
Attenuator Mode Auto
Fixed Range (PEP) In:
5980.0 ... 6000.0 dBm

This dialog describes all parameters for filling a table automatically with sensor readings.

i

</div>

</div>

To select the sensor and determine its parameters, refer to Chapter 4.3.6.2, "NRP Power Viewer", on page 170.

To fill the table, press the "Execute" button.

##### Fill User Correction Data with Sensor

Enables you to fill the table with correction data acquired by a connected power sensor from Rohde & Schwarz.

"Sensor" Displays connected sensors for selection.

"List To Fill" Indicates the used list file.

"Include Zeroing"

Performs a zeroing procedure before acquiring the user correction data to improve precision. Since during zeroing no signal may be applied to the sensor, RF is temporarily switched off at the generator. When unchecked, the zeroing procedure is skipped. The RF signal level might be blanked shortly. This setting is recommended if blanking of RF is undesirable or the absence of power at the sensor can not be guaranteed.

"Execute" Performs automatic filling of the list, provided a sensor is detected and the user correction list contains at least one frequency value.

##### Remote command:

[:SOURce<hw>] :CORRection:ZEROing:STATE on page 342

[:SOURce<hw>] :CORRection:CSET:DATA[:SENSOR<ch>] :POWER]:SONCe

on page 337

##### Used SMB Settings for Measurement

Displays the settings relevant for the measurement.

"RF Source" Shows the path for which the correction menu settings are made.

"Modulation" Indicates the modulation state

"Amplitude" Shows the currently set level.

"Use SParameter"
    Indicates whether SParameter correction is used
"Attenuator Mode"
    Displays the selected mode of the attenuator.
"Fixed Range (PEP) In:"
    Shows the level range.

Remote command:
n.a.

##### 4.3.5.7 Reverse Power Protection

The reverse power protection prevents against overload by an external signal applied to the RF output of the R&S SMB.

i

</div>

</div>

The R&S SMB equipped with frequency options up to 6 GHz includes a reverse power protection as standard. For instruments equipped with frequency option R&S SMB-B112 or R&S SMB-B112L a reverse power protection option R&S SMB-B30 is available. Refer to the data sheet for additional information and the respective option.

The reverse power protection is tripped when the power of the external signal becomes too high. A relay opens and interrupts the internal connection to the RF output. This condition is indicated in the display header by the "OVERLOAD" status message.

##### Overload

If an "Overload" status message is indicated in the display header, reset the overload protection by pressing the [RF ON/OFF] key.

The RF input is activated when the overload protection is reset.

Remote command:

:OUTPUT<hw>:PROTECTION:TRIPped? on page 316

:OUTPUT<hw>:PROTECTION:CLEar on page 316

:OUTPUT<hw>[:STATE] on page 317

#### 4.3.6 RF Measurement

##### 4.3.6.1 NRP Sensor Mapping

The "NRP Sensor Mapping" lists all R&S NRP sensors detected by the instrument.

Any R&S NRP sensor that supports the USB legacy protocol and is connected to one of the USB interfaces, is detected automatically and added to the list. Vice versa, the R&S SMB removes a sensor from the list, when it is disconnected.

R&S NRP sensors that are connected via LAN or use the USBTMC protocol are not automatically detected. They are detected by the scan search function.

##### Access:

▶ Select "RF > config... > RF Measurement > NRP Sensor Mapping"

| RF OFF | MODE OFF | Level | Level |
| --- | --- | --- | --- |
| 1.000 000 000 000 GHz | 1.000 000 000 000 GHz | -30.00 dBm | -30.00 dBm |
| A: ALC-Auto | A: ALC-Auto | A: ALC-Auto | Info |
| NRP Sensor Mapping | NRP Sensor Mapping | NRP Sensor Mapping | ✗ |
| Sensor Name | Protocol | Conn. | Map. |
| 1 NRP18SN-900101 | Visa |  | 2 |
| 2 NRP33SN-V-900011 | Visa |  |  |
| Start Scan | Clear | << Hide | << Hide |
| Add Sensor | Add Sensor | Add Sensor | Add Sensor |
| IP or Host Name | NRQ6-101624 | NRQ6-101624 | NRQ6-101624 |
| Add LAN Sensor | Add LAN Sensor | Add LAN Sensor | Add LAN Sensor |
| Device ID or Name | NRQ6 | NRQ6 | NRQ6 |
| Serial Number | Serial Number | 101624 | 101624 |
| Add USB TMC Sensor | Add USB TMC Sensor | Add USB TMC Sensor | Add USB TMC Sensor |

The dialog lists all detected R&S NRP sensors for selection and mapping. You can also browse the network for sensors.

The detected sensors are characterized by the used protocol and the corresponding connector icon. In the "Mapping" column, you can assign the sensor to one of the available sensor channels. The list can contain several entries but the R&S SMB can only use up to four sensors simultaneously.

The remote commands required to define these settings are described in Chapter 6.12, "SENSE, READ and INITiate Subsystems", on page 318.

##### Settings

Sensor Mapping List.....168

Scan.....169

Clear.....169

Add Sensor/Hide 'Add Sensor'.....169

Add Sensor settings.....169

L Add LAN Sensor settings.....169

L Add USB Sensor settings.....169

##### Sensor Mapping List

Displays a list of all sensor entries with information on the sensor name, the used protocol, the connector and the assigned mapping.

If a sensor is connected via LAN or uses the USBTMC protocol, its protocol is indicated as "Visa".

Remote command:

:SLiSt[:LIST]? on page 321

:SLISt:ELEMENT<ch>:MAPping on page 319

:SLiSt:SENSOR:MAP on page 320

##### Scan

Scans the network and the USB connections for sensors connected via the VISA communication protocol, i.e. sensors that are addressed via LAN or USBTMC.

Sensors communicating via the USB legacy protocol are detected automatically.

##### Remote command:

:SLiSt:SCAN[:STATE] on page 320

##### Clear

Removes the selected sensor from the sensor mapping list.

##### Remote command:

: SLIST: CLEar: LAN on page 319

: SLIST: CLEar: USB on page 319

: SLIST: CLEar[:ALL] on page 319

##### Add Sensor/Hide 'Add Sensor'

Shows or hides the "Add Sensor" settings.

##### Add Sensor settings

Configures settings to add sensors connected to the R&S SMB over USB or LAN.

##### Add LAN Sensor settings ← Add Sensor settings

Configures settings to add sensors connected to the R&S SMB over LAN.

Displays the host name or the IP address of a R&S NRP power sensor.

If the R&S SMB does not detect a connected R&S NRP sensor, you can assign the address information manually.

##### "Add LAN Sensor"

Adds a detected R&S NRP sensor connected in the LAN to the list of sensors, including its device ID or name and its serial number.

##### Remote command:

:SLISt:SCAN:LSENsor on page 319

##### Add USB Sensor settings ← Add Sensor settings

Configures settings to add sensors connected to the R&S SMB via USB.

"Device ID or Sensor Name"

Displays the device identifier or the name of the R&S NRP power sensor.

If the R&S SMB does not detect a connected R&S NRP sensor, you can assign the ID or name manually.

##### "Serial Number"

Displays the serial number of the R&S NRP power sensor. If the R&S SMB does not detect a connected R&S NRP sensor, you can assign the serial number manually.

"Add USBTMC Sensor"

Add USB IMC Sensor

Adds a detected R&S NRP sensor connected at the USB interface to the list of sensors, including its device ID or name and its serial number.

Remote command:

:SLiSt:SCAN:USENsor on page 320

##### 4.3.6.2 NRP Power Viewer

The R&S SMB features the power viewer function for measuring or monitoring either the RF output power, or a freely selectable signal source with R&S NRP power sensors.

The instrument can perform up to 4 power measurements simultaneously.

To connect the sensors you have the following options:

• connect the sensor directly at a [USB] inte

Requires the following cables, depending on the used sensor type:

– R&S NRP-ZKU (USB interface cable) for R&S NRPxx power sensors

– R&S NRP-Z3 or R&S NRP-Z4 (USB adapter cables) for sensors of the R&S NRP-Zxx family

- connect the sensor indirectly via [USB] using the R&S NRP-Z5 USB sensor hub. The R&S NRP-Z5 USB sensor hub (high-speed USB 2.0) can host up to 4 R&S NRP sensors. It provides simultaneous internal and external triggering of all connected sensors.

Requires additional cables, depending on the used output connector of the hub. Choose one of the following:

– Short extension cable R&S NRP-Z2 for connection to the sensor connector. This six-pole connection provides the external trigger capability.

– Standard USB cable (USB type A to USB type B) to any USB type A connector of the R&S SMB. This connection does not support external triggering.

- connection the sensor indirectly via USB hub with external power supply unit

Requires the following cables, depending on the used sensor type:

– R&S NRP-ZKU (USB interface cable) for R&S NRPxx power sensors

– R&S NRP-Z3

or R&S NRP-Z4 (USB adapter cables) for sensors of the R&S NRP-Zxx family

- connect an R&S NRPxxN power sensors via LAN

Using the Ethernet interface requires PoE (Power over Ethernet) to provide the electrical power.

To establish the connection, you can use:

– A PoE Ethernet switch, e.g. R&S NRP-ZAP1 and an RJ-45 Ethernet cable.

– A PoE injector and an RJ-45 Ethernet cable.

See also:

Chapter 3.2, "Instrument Tour", on page 48 for the assignment to the available connectors

• Getting Started manual of the R&S NRPSeries Power Sensors

- The Rohde & Schwarz website http://www.rohde-schwarz.com, section "Power Meters & Voltmeters" for information on the sensor hub and the available accessories.

##### Detection and mapping

The R&S SMB automatically detects a connected R&S NRP power sensor and indicates it in the dilaogs "NRP Power Viewer" NRP Power Viewer Settings and NRP Sensor Mapping dialogs. By default, sensors 1 to 4 are assigned to the sensors at the USB connectors, according to their sequence of connection. In the "Sensor Mapping dialog", you can change the mapping.

For device specific information on the connected sensor, see Chapter 4.2.3.4, "NRP Info/Update", on page 101. For information on the scope of your power sensor refer to the manual of your R&S NRP power sensor.

i

</div>

</div>

On connection, the R&S SMB immediately starts the measurement of a detected R&S NRP power sensor. If you perform an instrument preset ([Preset] key or *RST), the R&S SMB stops the measurements. The connection and the mapping of the power sensors remain, the measurements must be restarted.

A sensor continuously measures the average signal power of the selected source, such as an external signal, or the output signal of the signal generator with the RF level used as reference value. The R&S SMB shows the result in the NRP Power Viewer Settings settings dialog, but you can also permanently display the readings in the block diagram.

♡

</div>

</div>

##### Further functions of the R&S SMB related to R&S NRP power sensors are:

• Acquisition of level correction data, see Chapter 4.3.5.6, "User Correction", on page 159.

The acquired level correction data is used to create and activate lists in which level correction values predefined by the user are freely assigned to RF frequencies. Correction is performed by the user-defined table values being added to the output level for the respective RF frequency.

- NRP Level Control, see Chapter 4.3.5.5, "NRP Level Control", on page 155.

Note that "NRP Power Viewer" automatically disables "NRP Level Control", and vice versa.

- The software version of the connected power sensor can be retrieved by means of the remote control command SENSE<ch>[:POWER]:TYPE? on page 329.

Use the Chapter 4.2.3.4, "NRP Info/Update", on page 101 dialog to update the sensor software.

i

</div>

</div>

"NRP Power Viewer" automatically disables NRP Level Control, and vice versa.

##### NRP Power Viewer Settings

##### Access:

▶ Select one of the following:

● Select "RF > config... > RF Measurement > NRP Power Viewer".

- Press the [menu] key and select "RF > RF Measurement > NRP Power Viewer".

| NRP Power Viewer |
| --- |

The dialog shows the settings and measurement values of the sensor selected in the field next to the connector symbol. For indicating the parameters of another sensor, switch to the respective sensor in the selection list.

i

</div>

</div>

When you connect your power sensor(s) via the R&S NRP-Z5 USB sensor hub, each channel of the hub is firmly assigned to the associated sensor channel in the generator.

The remote commands required to define the settings are described in Chapter 6.12, "SENSE, READ and INITiate Subsystems", on page 318.

##### Sensor

Selects the R&S NRP power sensor for display.

In remote control, the sensors are set up using the SENSE commands. The remote measurement is triggered by the READ query which also provides the measurement results.

The sensor is selected by suffix 1, 2, 3 or 4 in key word SENSE or READ of the command header.

Suffix 1 denotes the sensor connected at the first [USB] interface, and suffix 2, 3 and 4 are assigned to further sensors connected via USB. The suffix is identical to the index which is assigned automatically to each sensor upon connection.

Note: The software version of the connected power sensor can be retrieved by means of the remote control command SENS:POW:TYPE?.

Use the "Setup >" Chapter 4.2.3.4, "NRP Info/Update", on page 101 dialog to update the sensor software.

Remote command:

SENSE<ch>[:POWER]:STATUS[:DEVICE]?on page 329

##### Type

Indicates the type and the serial number of the connected R&S NRP power sensor. The sensor type is automatically detected.

##### Remote command:

SENSe<ch>[:POWER]:TYPE? on page 329

SENSe<ch>[:POWER]:SNUMBER? on page 328

##### State

Activates/deactivates level measurement by the power sensor.

The local state is set with the INIT command. Switching the local state off enhances the measurement performance.

In remote control, the sensors are set up using the SENSE commands. The remote measurement is triggered by the READ query which also provides the measurement results. The state is not influenced by these commands, measurements results can be retrieved with local State on or off.

The sensor is selected by suffix 1, 2, 3 or 4 in key word SENSE or READ of the command header.

Suffix 1 denotes the sensor connected at the first [USB] interface, and suffix 2, 3 and 4 are assigned to further sensors connected via USB. The suffix is identical to the index which is assigned automatically to each sensor upon connection.

To query the availability of a sensor at a given connector, use the command SENSE<ch>[:POWER]:STATUS[:DEVICE]? on page 329.

##### Remote command:

:INITiate<hw>[:POWER]:CONTINUOUS ON PAGE 322

##### Level (Peak)

With certain power sensors only, for example R&S NRP-Z81.

Indicates the measured peak level value with the selected unit.

##### Remote command:

:READ<ch>[:POWER] ? on page 322

##### Level (Avg.)

Indicates the measured level value with the selected unit.

##### Remote command:

:READ<ch>[:POWER] ? on page 322

##### Unit

Selects the unit used for result display.

The power sensor provides the measured value in Watt.

In which unit the measured value is indicated is selected here and might be Watt, dBm or dBuV.

##### Remote command:

:SENSE<ch>:UNIT[:POWER] on page 330

##### Permanent Display State

Activates the permanent indication of the power measurement result in the upper right corner of the block diagram. The instrument shows the type of sensor, the corresponding connector, the measurement source and - if set - the offset.

1 NRP18SN -73.50 dBm Avg

It is possible to switch the permanent display active for several sensors. In this case, the instrument indicates the values of the sensor with the lowest port number in the display.

##### Remote command:

:SENSE<ch>[:POWER]:DISPLAY:PERManent:STATE on page 324

##### Display Priority

Determines whether the instrument displays the measured average or the peak power permanently on the screen.

To select the peak power display, it is required that the R&S NRP power sensor supports this feature. On power-on or connecting a sensor the average power value is set by default.

To enable the permanent display in the block diagram, select Permanent Display State.

##### Remote command:

:SENSE<ch>[:POWER]:DISPLAY:PERManent:PRIORITY on page 324

##### Zero - Power Sensors

Activates the auto zero function.

Zeroing calibrates the external power sensor by adjusting its reading at zero signal power. For this purpose, the RF power source must be switched off or disconnected from the sensor. If a Rohde & Schwarz power sensor receives an input power during the zeroing process, it aborts zeroing and generates an error message. Zeroing takes a few seconds, depending on the sensor model. Refer to the documentation of your power sensor for more information.

##### Tips for zeroing

When to perform zeroing:

• During warm up after switching on or connecting the instrument

• After a substantial change of the ambient temperature

• After fastening the power sensor module to an RF connector at high temperature

• After several hours of operation

- When low-power signals are to be measured, e.g. less than 10 dB above the lower measurement limit.

- Switch off the RF power source for zeroing, but do not disconnect it from the power sensor. This proceeding keeps the thermal equilibrium, and the zeroing process also compensates the noise that superimposes the measured signal (e.g. from a broadband amplifier).

Remote command:

:SENSE<ch>[:POWER]:ZERO on page 330

##### Source

Selects the source for measurement.

"RF" Measurement source is the RF signal of the generator. The RF frequency is used as the measurement frequency of the sensor and the corresponding correction factor is used. In this mode the RF frequency of the generator is sent to the sensor automatically if changed.

"User" Measurements source is any freely selectable source. The frequency is entered manually under frequency (e.g. for measurement of ampli-fier gain with 2 sensors).

##### Remote command:

:SENSE<ch>[:POWER]:SOURCE on page 328

##### Frequency

Source User only

Enters the frequency for measurement source "User".

##### Remote command:

:SENSE<ch>[:POWER]:FREQUENCY on page 327

##### Level Offset

Activates and defines a level offset which is added to the measured value. This allows e.g. an attenuator in the signal path to be considered. The offset is always entered in dB, irrespective of the selected unit for result display.

Remote command:

:SENSE<ch>[:POWER]:OFFSET:STATE on page 328

:SENSE<ch>[:POWER]:OFFSET on page 327

##### Filter

Determines the length of the filter used for the measurement. The filter length affects the measurement time directly.

The averaging filter is used to reduce fluctuations in the measured result to the extent desired. Such fluctuations can be caused by inherent noise of the measuring instrument, modulation of the measurement signal or beats from the superposition of adjacent carriers. A more stable display has to be traded off against longer measurements. The measurement result is obtained from a two-stage averaging process.

Note: Longer measurements do not mean that it takes longer to display a new result, but rather that it takes longer for the result to settle when the power changes.

Measurements are continuously repeated in a predefined time window. The measurement result is obtained by averaging the measured values for the last 2N time windows. The number N is the filter length, the factor of 2 arises because the output signals from the microwave detector to suppress low-frequency noise are chopped at the same rate as the time windows, which means that an independent measured value can only be obtained from two consecutive values. As the filter length is the multiplier for the time window it directly influences the measurement time.

The filter length can be selected automatically or can be manually set to a fixed value. As a preliminary, you should always check if the auto mode is giving satisfactory results because you will always have to adjust an optimal, manual filter-length setting if the power is not constant.

Selection "Fixed Noise" is offered for achieving defined measurement accuracy.

"Auto" The filter length is automatically selected and adapted to the currently measured value. With very high signals the filter length and therefore the measurement time can be short. With very low signal levels the filter length and therefore the measurement time is increased in order to reduce noise. The used filter length is indicated in the field to the right, see Filter Length.

"User" The filter length is set manually.

The filter length is entered in the entry window to the right. As the filter length works as a multiplier for the time window, this results in a constant measurement time.

Note: The time window varies depending on the used sensor. For most sensors it is fixed to 20 ms. For the R&S NRP-Z81 sensor it is 10 us. Therefore, the user filter length for the R&S NRP-Z81 has be about 1000 times larger than the filter length for other sensors in order to achieve the same filtering result.

The Auto Once button can be used to search for the optimum filter length for the current measurement conditions. The found filter length is indicated in the field to the right, see Filter Length.

"Fixed Noise" The averaging factor is selected so that the sensors intrinsic noise (2 standard deviations) does not exceed the specified noise content. The desired noise content is entered in the entry field to the right, see Noise Content.

To avoid very long settling times when the power is low, the averaging factor can be limited with the Timeout parameter.

##### Remote command:

:SENSE<ch>[:POWER]:FILTER:TYPE on page 326

##### Filter Length ← Filter

Indicates the used filter length for filter type "Auto" or "User".

##### Remote command:

:SENSE<ch>[:POWER]:FILTER:LENGTH:AUTO? on page 324

:SENSE<ch>[:POWER]:FILTER:LENGTH[:USER] on page 325

##### Noise Content ← Filter

Sets the noise content for filter type "Fixed Noise".

##### Remote command:

:SENSE<ch>[:POWER]:FILTER:NSRatio on page 325

##### Timeout ← Filter

Sets a time limit for the averaging process.

##### Remote command:

:SENSE<ch>[:POWER]:FILTER:NSRatio:MTIME on page 325

##### Auto Once ← Filter

Calculates the optimum filter length for the current measurement conditions and indicates the value in the Filter Length.

##### Remote command:

:SENSE<ch>[:POWER]:FILTER:SONCe on page 326

##### Use Default Aperture Time

Enables you to specify a user-defined aperture time for the respective sensor.

The sensor default setting is usually sufficient. If however, the readings vary, it is recommended that you adjust the aperture time exactly to one modulation period, in order to obtain stable readings. To specify the aperture time, see Aperture Time.

##### Remote command:

:SENSE<ch>[:POWER]:APERTURE:DEFAULT:STATE on page 321

##### Aperture Time

Defines the acquisition time for the respective sensor, provided the entry field is enabled, see Use Default Aperture Time.

For example you can adjust the aperture time exactly to one signal period, in order to obtain a sufficient low average value.

##### Remote command:

:SENSE<ch>[:POWER]:APERTURE:TIME on page 321

##### Use SParameter - Power Sensors

Activates the use of the S-Parameter correction data of the connected power sensor. For sensors with attenuator this checkbox is automatically checked.

Refer to the manual of the connected R&S NRP power sensor for a description on how to use the SParameter table.

##### Remote command:

:SENSE<ch>[:POWER]:CORRection:SPDevice:STATE on page 323

##### Enable Logging

Activates recording of R&S NRP power sensor readings.

If enabled, every value measured by a connected power sensor and indicated in the user interface, is written to a log file. Per measurement the function logs the measured value (2 readings when you work with peak sensors), the sensor type and the measurement time (time stamp).

The function automatically creates the file name SensLog<n>.txt and stores the file in *txt format under /var/user/SensorLogging on the hard disk. You can enable logging for each connected sensor separately. If enabled, one file per sensor is written.

Note: This specific function is intended for measurements with long time intervals, or if there is a risk that the connection to the sensor can be interrupted and you need the data for reconstruction.

The simplified recording function continuously writes the values in the file of the corresponding sensor number, like Sens1Log.txt. When you start a new measurement, the existing data will not be overwritten, but added to the file.

If you use this function, it is recommended that you regularly remove the files from the hard disk, since they require storage capacity.

Remote command:

:SENSE<ch>[:POWER]:LOGGing:STATE on page 327

#### 4.3.7 RF Sweep and List Mode

##### 4.3.7.1 Overview

The R&S SMB offers three different sweep types (frequency sweep, level sweep and LF sweep) to be activated alternatively. Each type has 6 modes which differ with respect to the sweep cycle mode (continuous, individual and step-by-step) and triggering mode (automatic, internal and external).

i

</div>

</div>

- Sweeps and list mode can not be activated simultaneously, they deactivate each other.

- Activating a sweep mode immediately disables NRP Level Control.

Vice versa, a running sweep mode blocks "NRP Level Control". It can not be activated.

- If you want to remain at a specific frequency or level value during a sweep, enter the value directly in the status bar. The sweep stops immediately.

##### Setting a sweep

A sweep is set in five basic steps which are shown below taking a frequency sweep as an example.

?

</div>

</div>

The LF sweep is activated and configured in the "Mod Gen" block.

1. Set the sweep range ("Start Freq" and "Stop Freq" or "Center Freq" and "Span").

2. Select linear or logarithmic sweep spacing ("Spacing").

3. Set the step width ("Step Lin/Log") and dwell time ("Dwell Time").

4. Activate the sweep ("Mode" to Auto, Single, Step or External Single, External Step).

5. Trigger the sweep, except for Auto mode ("Execute Single Sweep", Current Frequency or External Trigger Signal).

i

</div>

</div>

It is recommended to switch off the display update for optimum sweep performance especially with short dwell times (see Chapter 4.2.3.6, "Display Update", on page 103).

##### 4.3.7.2 RF Frequency Sweep

The dialog enables you to activate and configure a sweep for the RF frequency.

To open the "RF Frequency Sweep" dialog, select "RF > Configure > RF Frequency Sweep" or use the [MENU] key under "RF".

In the top section of the dialog, the RF sweep is activated and the sweep mode is selected.

The buttons are used to reset the RF sweep (all sweep modes) or to execute the RF sweep ("Single" mode).

The sweep range, sweep spacing and dwell time are set in the bottom section.

For the frequency sweep, an output signal at the [LF] connector can be activated. It provides a linear voltage ramp from start to stop of the sweep. The output voltage can be used for example to control an oscilloscope.

You can configure the sweep range of the RF sweep in two ways, either by entering the "Start" and "Stop" values or by entering the "Center" frequency and the "Span".

The two sets of parameters correlate as follows:

● "Start Freq" = "Center Freq" - "Span"/2

● "Stop Freq" = "Center Freq" + "Span"/2

"Center Freq" = ("Start Freq" + [Stop Freq])/2

● "Span" = "Stop Freq" - "Start Freq"

| RF Frequency Sweep | RF Frequency Sweep |
| --- | --- |

##### RF Frequency Sweep Settings

▶ To access the sweep dialog, select "RF > configure > Sweep/List > RF Frequency Sweep".

In these dialogs you can configure the corresponding sweep signal.

##### State - Frequency Sweep

Activates RF sweep mode.

##### Note:

Activating a sweep mode automatically deactivates other sweeps and the list mode.

##### Remote command:

[:SOURce<hw>] :FREQUency:MODE on page 349

##### Mode - RF Frequency Sweep

Selects the RF frequency sweep mode.

If you change the sweep mode during the execution, the signal generator stops the sweep and starts with the next trigger event at the initial value.

The "Reset Sweep" button sets the sweep to the start value.

##### "Auto"

Generates a continuously repeating sweep signal immediately after activating the sweep mode.

The sweep steps are performed atomatically, controlled by the dwell time, see "Dwell Time - Frequency Sweep" on page 186.

##### Example:

SOUR: SWE: FREQ:MODE AUTO

TRIG:FSW:SOUR AUTO

SOUR: FREQ:MODE SWE

"Single" Generates a single sweep cycle after a trigger event. The sweep steps within the cycle are performed automatically, controlled by the dwell time. If one cycle is completed, the instrument waits for the next trigger event. To trigger the sweep, use "Execute Single Sweep" button, or the corresponding remote control commands, for example *TRG.

##### Example:

SOUR: SWE: FREQ:MODE AUTO

TRIG: FSW: SOUR SING

SOUR: FREQ:MODE SWE

SOUR: SWE: FREQ: EXEC

"Step"

Generates the sweep signal step-by-step, manually triggered.

To perform the sweep steps, enter the frequency value under Current Freq - Frequency Sweep. You can directly enter the value, but also use the [up] and [down] navigation keys or the [rotary knob]. You can determine the step width below in the entry field "Step Lin" or "Step Log", see Step Lin/Log - Frequency Sweep.

If a step is out of the sweep range ("Start Freq" or "Stop Freq"), it is ignored.

Note: To step through the sweep frequencies in remote control mode, use the FREQ:MAN command with the UP or DOWN parameter.

##### Example:

SOUR: FREQ: CENT 300MHz

SOUR: FREQ: SPAN 400MHz

SOUR: SWE: FREQ: SPAC LIN

SOUR: SWE: FREQ: STEP: LIN 100MHz

SOUR: FREQ:MODE MAN

TRIG:FSW:SOUR SING

set sweep mode "Step".

SOUR: FREQ:MODE SWE

activate sweep mode, the frequency is set to "Start Freq".

SOUR: FREQ: MAN UP

set the frequency to the next higher sweep frequency.

SOUR: FREQ: MAN DOWN

##### "Extern Single"

set the frequency to the next lower sweep frequency.

Generates a single sweep cycle when an a external trigger event occurs.

occurs.

The sweep steps within the cycle are performed automatically, controlled by the dwell time. If one cycle is completed, the instrument waits for the next trigger event.

To trigger the sweep, apply an external trigger signal.

Refer to the description of the rear panel for information on the connectors for external trigger signal input (see Chapter 3.2.2, "Rear Panel Tour", on page 54).

##### Example:

SOUR: SWE: FREQ:MODE AUTO

TRIG:FSW:SOUR EXT

SOUR: FREQ:MODE SWE (External trigger)

##### "Extern Step"

"ern Step" Generates the sweep signal step-by-step, manually triggered.

To trigger a sweep step, apply an external trigger signal. The step width corresponds to the step width set for the rotary knob.

##### Example:

SOUR: SWE: FREQ:MODE STEP

SOUR: SWE: FREQ: SPAC LIN

SOUR: SWE: FREQ: STEP: LIN 1MHz

TRIG:FSW:SOUR EXT

SOUR: FREQ:MODE SWE (External trigger)

##### "Extern Start/Stop"

Generates a continuously repeating sweep signal that is started, stopped and restarted by subsequent external trigger events. The sweep steps are performed automatically, controlled by the dwell time.

Refer to the description of the rear panel for information on the connectors for external trigger signal input (see Chapter 3.2.2, "Rear Panel Tour", on page 54).

##### Example:

SOUR: SWE: FREQ:MODE AUTO

TRIG:FSW:SOUR EAUT

SOUR: FREQ:MODE SWE (External trigger)

##### Remote command:

[ : SOURce<hw> ] : SWEep [ : FREQuency ] : MODE on page 424

:TRIGger<hw>:FSweep:SOURce on page 458

[:SOURce<hw>] :FREQUency:MODE on page 349

##### Execute Single Sweep - Frequency Sweep

Starts a sweep manually. This trigger button is displayed in "Single" mode.

##### Remote command:

[ : SOURce<hw> ] : SWEep [ : FREQuency ] : EXECute on page 423

:TRIGger<hw>:FSweep[:IMMediate] on page 459

:TRIGger<hw>[:SWEep][:IMMediate] on page 463

##### Reset Sweep - Frequency Sweep

Resets the sweep.

With the next trigger event, the sweep starts with at the initial value.

##### Remote command:

[:SOURCE<hw>] : SWEep:RESET[:ALL] on page 433

##### Start Freq - Frequency Sweep

Sets the start frequency.

##### Remote command:

[:SOURCE<hw>] :FREQUency:STARt on page 351

##### Stop Freq - Frequency Sweep

Sets the stop frequency.

Remote command:

[:SOURCE<hw>] :FREQUENCY:STOP on page 351

##### Center Freq - Frequency Sweep

Sets the center frequency.

Remote command:

[: SOURce<hw>] : FREQuency: CENTER on page 346

##### Span - Frequency Sweep

Sets the span.

##### Remote command:

[:SOURce<hw>] :FREQUency:SPAN on page 350

##### Current Freq - Frequency Sweep

Displays the current frequency.

In sweep "Step" mode, the parameter is editable and you can enter frequency for the next step.

##### Remote command:

[:SOURCE<hw>] :FREQUency:MANual on page 348

##### Spacing - Frequency Sweep

Selects the mode for the calculation of the frequency sweep intervals.

"Linear" Takes the frequency value entered as an absolute value in Hz.

"Logarithmic" Takes the value entered as a logarithmic value, that means as a constant fraction of teh current frequency in %.

##### Remote command:

[:SOURCE<hw>] :SWEep[:FREQUency]:SPACing on page 427

##### Shape - RF Frequency Sweep

Selects the waveform shape of the sweep signal.

| --- | --- | --- |
| Feature | Frequency | Frequency |
| Feature | Frequency | Frequency |
| Start | 0 | 0 |
| Start | 0 | 0 |
| Stop | 0 | 0 |
| Stop | 0 | 0 |
| Triangle shape | 0 | 0 |
| Triangle shape | 0 | 0 |
| Sawtooth shape | 0 | 0 |
| Sawtooth shape | 0 | 0 |

</td></tr>
  </tbody>
</table>

One sweep runs from start to stop frequency. Each subsequent sweep starts at the start frequency, that means the shape of the sweep sequence resembles a sawtooth.

"Triangle"

The sweep runs from the start to the stop frequency and back, that means the shape of the sweep resembles a triangle. Each subsequent sweep starts at the start frequency.

##### Remote command:

[:SOURCE<hw>] :SWEep[:FREQUency] :SHAPe on page 426

##### Retrace - RF Frequency Sweep

Activates that the signal changes to the start frequency value while it is waiting for the next trigger event.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single", see Mode - RF Frequency Sweep.

| --- | --- | --- | --- |
| Stop Frequency | 0 | 0 | 0 |
| Stop Frequency | 0 | 0 | 0 |

</td></tr>
  </tbody>
</table>

##### Remote command:

[ : SOURce<hw> ] : SWEep [ : FREQuency ] : RETRace on page 426

##### Step Lin/Log - Frequency Sweep

Sets the step width for the individual frequency sweep steps.

At each step this value is added to the current frequency.

Depending on the Spacing - Frequency Sweep mode you have set, the corresponding parameter is displayed.

"Step Lin" The step width is a constant value in Hz.

Remote command:

[ : SOURce<hw> ] : SWEep [ : FREQuency ] : STEP [ : LINear ] on page 427

"Step Log"

Log" The step width is determined logarithmically in %, that means as a constant fraction of the current frequency.

Successive frequencies are calculated as follows:

• start_f < stop_f

If f2 > stop_f: f2 is set to stop_f.

• start_f > stop_f

If f2 < stop_f: f2 is set to stop_f.

When the shape "Triangle" is set, the frequency values on the slope from stop_f back to start_f are the same as on the slope from start_f to stop_f.

##### Remote command:

[:SOURCE<hw>] :SWEep[:FREQUency]:STEP:LOGarithmic on page 428

##### Dwell Time - Frequency Sweep

Sets the dwell time. The dwell time determines the duration of the individual sweep steps.

| --- | --- | --- |
| Time | Signal Level | Signal Level |
| Time | Signal Level | Signal Level |
| 0 | 0 | 0 |
| 0 | 0 | 0 |
| 1 | 0 | 0 |
| 1 | 0 | 0 |
| 2 | 0 | 0 |
| 2 | 0 | 0 |
| 3 | 0 | 0 |
| 3 | 0 | 0 |
| 4 | 0 | 0 |
| 4 | 0 | 0 |
| 5 | 0 | 0 |
| 5 | 0 | 0 |
| 6 | 0 | 0 |
| 6 | 0 | 0 |
| 7 | 0 | 0 |
| 7 | 0 | 0 |
| 8 | 0 | 0 |
| 8 | 0 | 0 |
| 9 | 0 | 0 |
| 9 | 0 | 0 |
| 10 | 0 | 0 |
| 10 | 0 | 0 |
| 11 | 0 | 0 |
| 11 | 0 | 0 |
| 12 | 0 | 0 |
| 12 | 0 | 0 |
| 13 | 0 | 0 |
| 13 | 0 | 0 |
| 14 | 0 | 0 |
| 14 | 0 | 0 |
| 15 | 0 | 0 |
| 15 | 0 | 0 |
| 16 | 0 | 0 |
| 16 | 0 | 0 |
| 17 | 0 | 0 |
| 17 | 0 | 0 |
| 18 | 0 | 0 |
| 18 | 0 | 0 |
| 19 | 0 | 0 |
| 19 | 0 | 0 |
| 20 | 0 | 0 |
| 20 | 0 | 0 |
| 21 | 0 | 0 |
| 21 | 0 | 0 |
| 22 | 0 | 0 |
| 22 | 0 | 0 |
| 23 | 0 | 0 |
| 23 | 0 | 0 |
| 24 | 0 | 0 |
| 24 | 0 | 0 |
| 25 | 0 | 0 |
| 25 | 0 | 0 |
| 26 | 0 | 0 |
| 26 | 0 | 0 |
| 27 | 0 | 0 |
| 27 | 0 | 0 |
| 28 | 0 | 0 |
| 28 | 0 | 0 |
| 29 | 0 | 0 |
| 29 | 0 | 0 |
| 30 | 0 | 0 |
| 30 | 0 | 0 |
| 31 | 0 | 0 |
| 31 | 0 | 0 |
| 32 | 0 | 0 |
| 32 | 0 | 0 |
| 33 | 0 | 0 |
| 33 | 0 | 0 |
| 34 | 0 | 0 |
| 34 | 0 | 0 |
| 35 | 0 | 0 |
| 35 | 0 | 0 |
| 36 | 0 | 0 |
| 36 | 0 | 0 |
| 37 | 0 | 0 |
| 37 | 0 | 0 |
| 38 | 0 | 0 |
| 38 | 0 | 0 |
| 39 | 0 | 0 |
| 39 | 0 | 0 |
| 40 | 0 | 0 |
| 40 | 0 | 0 |
| 41 | 0 | 0 |
| 41 | 0 | 0 |
| 42 | 0 | 0 |
| 42 | 0 | 0 |
| 43 | 0 | 0 |
| 43 | 0 | 0 |
| 44 | 0 | 0 |
| 44 | 0 | 0 |
| 45 | 0 | 0 |
| 45 | 0 | 0 |
| 46 | 0 | 0 |
| 46 | 0 | 0 |
| 47 | 0 | 0 |
| 47 | 0 | 0 |
| 48 | 0 | 0 |
| 48 | 0 | 0 |
| 49 | 0 | 0 |
| 49 | 0 | 0 |
| 50 | 0 | 0 |
| 50 | 0 | 0 |
| 51 | 0 | 0 |
| 51 | 0 | 0 |
| 52 | 0 | 0 |
| 52 | 0 | 0 |
| 53 | 0 | 0 |
| 53 | 0 | 0 |
| 54 | 0 | 0 |
| 54 | 0 | 0 |
| 55 | 0 | 0 |
| 55 | 0 | 0 |
| 56 | 0 | 0 |
| 56 | 0 | 0 |
| 57 | 0 | 0 |
| 57 | 0 | 0 |
| 58 | 0 | 0 |
| 58 | 0 | 0 |
| 59 | 0 | 0 |
| 59 | 0 | 0 |
| 60 | 0 | 0 |
| 60 | 0 | 0 |
| 61 | 0 | 0 |
| 61 | 0 | 0 |
| 62 | 0 | 0 |
| 62 | 0 | 0 |
| 63 | 0 | 0 |
| 63 | 0 | 0 |
| 64 | 0 | 0 |
| 64 | 0 | 0 |
| 65 | 0 | 0 |
| 65 | 0 | 0 |
| 66 | 0 | 0 |
| 66 | 0 | 0 |
| 67 | 0 | 0 |
| 67 | 0 | 0 |
| 68 | 0 | 0 |
| 68 | 0 | 0 |
| 69 | 0 | 0 |
| 69 | 0 | 0 |
| 70 | 0 | 0 |
| 70 | 0 | 0 |
| 71 | 0 | 0 |
| 71 | 0 | 0 |
| 72 | 0 | 0 |
| 72 | 0 | 0 |
| 73 | 0 | 0 |
| 73 | 0 | 0 |
| 74 | 0 | 0 |
| 74 | 0 | 0 |
| 75 | 0 | 0 |
| 75 | 0 | 0 |
| 76 | 0 | 0 |
| 76 | 0 | 0 |
| 77 | 0 | 0 |
| 77 | 0 | 0 |
| 78 | 0 | 0 |
| 78 | 0 | 0 |
| 79 | 0 | 0 |
| 79 | 0 | 0 |
| 80 | 0 | 0 |
| 80 | 0 | 0 |
| 81 | 0 | 0 |
| 81 | 0 | 0 |
| 82 | 0 | 0 |
| 82 | 0 | 0 |
| 83 | 0 | 0 |
| 83 | 0 | 0 |
| 84 | 0 | 0 |
| 84 | 0 | 0 |
| 85 | 0 | 0 |
| 85 | 0 | 0 |
| 86 | 0 | 0 |
| 86 | 0 | 0 |
| 87 | 0 | 0 |
| 87 | 0 | 0 |
| 88 | 0 | 0 |
| 88 | 0 | 0 |
| 89 | 0 | 0 |
| 89 | 0 | 0 |
| 90 | 0 | 0 |
| 90 | 0 | 0 |
| 91 | 0 | 0 |
| 91 | 0 | 0 |
| 92 | 0 | 0 |
| 92 | 0 | 0 |
| 93 | 0 | 0 |
| 93 | 0 | 0 |
| 94 | 0 | 0 |
| 94 | 0 | 0 |
| 95 | 0 | 0 |
| 95 | 0 | 0 |
| 96 | 0 | 0 |
| 96 | 0 | 0 |
| 97 | 0 | 0 |
| 97 | 0 | 0 |
| 98 | 0 | 0 |
| 98 | 0 | 0 |
| 99 | 0 | 0 |
| 99 | 0 | 0 |
| 100 | 0 | 0 |
| 100 | 0 | 0 |

</td></tr>
  </tbody>
</table>

The "Dwell Time" set by the user is used as the step time of the sweep. The effective net dwell time is shorter, reduced by the setting time. This setting time may be greater than the time specified in the data sheet.

##### Note:

It is recommended to switch off the display update for optimum sweep performance especially with short dwell times (see Chapter 4.2.3.6, "Display Update", on page 103).

Remote command:

[ : SOURce<hw> ] : SWEep [ : FREQuency ] : DWELL on page 422

##### Use LF connector to output sweep voltage - RF Frequency Sweep

Activates the output of a linear voltage ramp from sweep start to sweep stop at the LF connector. This signal can be used for the X-deflection of an oscilloscope. The voltage range is determined below.

Remote command:

[:SOURce<hw>] :SWEep[:FREQUency]:LFConnector on page 423

##### Output Voltage Start Freq - RF Frequency Sweep

Sets the voltage at the sweep start frequency.

##### Remote command:

[:SOURCE<hw>] :SWEep[:FREQUency]:OVOLtage:START on page 424

##### Output Voltage Stop - RF Frequency Sweep

Sets the voltage at the sweep stop frequency.

Remote command:

[:SOURCE<hw>] :SWEep[:FREQUency]:OVOLtage:STOP on page 425

##### Ext. Trigger Input Slope

Sets the polarity of the active slope of an externally applied instrument trigger.

This setting affects the INST TRIG input (BNC connector at the rear of the instrument).

"Positive" activates the rising edge of the trigger signal.

"Negative" activates the falling edge of the trigger signal.

Remote command:

[:SOURCE]:INPUT:TRIGger:SLOPe on page 354

##### 4.3.7.3 RF Level Sweep

To open the "Level Sweep" menu, select "RF > Configure > Level Sweep" or use the [MENU] key under "RF".

##### RF Level Sweep Settings

In the top section, the RF level sweep is activated and the sweep mode is selected. The buttons are used to reset the level sweep (all sweep modes) or to execute the level sweep ("Single" mode).

The sweep range, sweep spacing and dwell time are set in the bottom section.

| RF Level Sweep | RF Level Sweep |
| --- | --- |

##### State - Level Sweep

Activates Level Sweep mode.

##### Note:

Activating a sweep mode automatically deactivates other sweeps and the list mode.

##### Remote command:

[:SOURCE<hw>] :POWER:MODE on page 388

##### Mode - Level Sweep

Selects the level sweep instrument operating mode and the sweep mode.

If you change the sweep mode during the execution, the signal generator stops the sweep and starts with the next trigger event at the initial value.

The "Reset Sweep" button sets the sweep to the start value.

"Auto" Sets an automatically repeated sweep cycle.

##### Example:

SOUR: SWE: POW: MODE AUTO

TRIG: PSW: SOUR AUTO

SOUR: POW: MODE SWE

"Single"

Sets a single sweep cycle. The sweep is triggered by the "Execute Single Sweep" button, or by means remote trigger commands, e.g. *TRG.

##### Example:

SOUR: SWE: POW: MODE AUTO

TRIG: PSW: SOUR SING

SOUR: POW: MODE SWE

SOUR: SWE: POW: EXEC

##### "Step"

Sets a step-by-step sweep cycle.

If this mode is activated, the cursor moves to the value displayed for "Current Level". Each sweep step is triggered by a variation of the value in the "Current Level" entry window. The step width is set below at entry field "Step".

If this mode is activated, the cursor moves to the value displayed for "Current Level". If a different sweep mode was activated prior to the "Step" mode, the current sweep is stopped. The step sweep starts at the current level value.

##### Example:

SOUR: SWE: POW: MODE MAN

TRI: PSW: SOUR SING

SOUR: SWE: POW: STEP 0.5

SOUR: POW: MODE SWE

SOUR: POW: MAN -16

The value entered with command SOUR: SWE: POW: STEP sets the step width.

The value entered with command SOUR: POW:MAN has no effect, the command only triggers the next sweep step. However, the value has to be in the currently set sweep range (start to stop). In remote control only a step-by-step sweep from start to stop frequency is possible.

#### "Extern Single"

Sets a single sweep cycle. The sweep is triggered by an external trigger signal.

Refer to the description of the rear panel for information about the connectors for external trigger signal input (see Chapter 3.2.2, "Rear Panel Tour", on page 54).

##### Example:

SOUR: SWE: POW: MODE AUTO

TRIG: PSW: SOUR EXT

SOUR: POW: MODE SWE (External trigger)

##### "Extern Step"

Sets a step-by-step sweep cycle. Each sweep step is triggered by an external trigger signal (trigger source as described under "External Single"). The step width corresponds to the step width of the rotary knob.

##### Example:

SOUR: SWE: POW: MODE STEP

SOUR: SWE: POW: STEP 0.5

TRIG: PSW: SOUR EXT

SOUR: POW: MODE SWE (External trigger)

##### "Extern Start/Stop"

Sets an automatically repeated sweep cycle that is started, stopped and restart by subsequent external trigger events.

The first external trigger signal starts the sweep (Start).

The next external trigger signal stops the sweep at the current frequency (Stop).

The third external trigger signal starts the sweep at the start frequency (Start).

Refer to the description of the rear panel for information about the connectors for external trigger signal input (see section "Legend for Rear Panel View").

##### Example:

SOUR: SWE: POW: MODE AUTO

TRIG: PSW: SOUR EAUT

SOUR: POW: MODE SWE (External trigger)

##### Remote command:

[:SOURCE<hw>] :SWEep:POWER:MODE on page 430

:TRIGger<hw>:PSWEEP:SOURCE on page 461.

[:SOURCE<hw>] :POWER:MODE on page 388

##### Reset Sweep - Level Sweep

Resets the sweep. The start level is set and the next sweep starts from there.

##### Remote command:

[:SOURCE<hw>] :SWEep:RESET[:ALL] on page 433

##### Execute Single Sweep - Level Sweep

Triggers the sweep manually. A manual sweep can only be triggered if "Mode Single" is selected.

##### Example:

SOUR: SWE: POW: MODE AUTO

TRIG: PSW: SOUR SING

SOUR: POW: MODE SWE

SOUR: SWE: EXEC

##### Remote command:

[:SOURCE<hw>] :SWEep:POWER:EXECute on page 429

:TRIGger<hw>:PSWeep[:IMMediate] on page 461

:TRIGger<hw>[:SWEep][:IMMediate] on page 463

##### Start Level - Level Sweep

Sets the start level.

##### Remote command:

[:SOURCE<hw>] :POWER:START on page 391

##### Stop Level - Level Sweep

Sets the stop level.

Remote command:

[:SOURCE<hw>] :POWER:STOP on page 392

##### Current Level - Level Sweep

Displays the current level.

If "Step" is set, the level for the next level step of the sweep is entered here.

##### Remote command:

[:SOURCE<hw>] :POWER:MANual on page 387

##### Shape - RF Level Sweep

Selects the cycle mode for a sweep sequence (shape).

"Sawtooth" One sweep runs from the start level to the stop level. The subsequent sweep starts at the start level again, i.e. the shape of sweep sequence resembles a sawtooth.

"Triangle" One sweep runs from start to stop level and back, i.e. the shape of the sweep resembles a triangle. Each subsequent sweep starts at the start level again.

##### Remote command:

[:SOURCE<hw>] :SWEep:POWER:SHAPe on page 431

##### Retrace - RF Level Sweep

Activates that the signal changes to the start level value while it is waiting for the next trigger event. It allows you to shift down the power during the waiting period.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single", see Mode - Level Sweep.

| --- | --- | --- | --- | --- | --- |
| Signal Level | Retrace On | Retrace Off | Retrace On | Retrace Off | Retrace Off |
| Signal Level | Retrace On | Retrace Off | Retrace On | Retrace Off | Retrace Off |
| start level | -1 | 1 | -1 | 1 | 1 |
| start level | -1 | 1 | -1 | 1 | 1 |
| stopped | -1 | 1 | -1 | 1 | 1 |
| stopped | -1 | 1 | -1 | 1 | 1 |
| running | -1 | 1 | -1 | 1 | 1 |
| running | -1 | 1 | -1 | 1 | 1 |
| stopped | -1 | 1 | -1 | 1 | 1 |
| stopped | -1 | 1 | -1 | 1 | 1 |

</td></tr>
  </tbody>
</table>

Remote command:
[: SOURce<hw>] : SWEep: POWER: RETRace on page 431

##### Step - Level Sweep

Sets the step width for the individual sweep steps. This entry is effective for all sweep modes.

With the level sweep, the logarithmic step width is a constant fraction of the current level. This fraction is added to the current level. The logarithmic step width is entered in dB.

##### Remote command:

[:SOURCE<hw>] :SWEep:POWER:STEP[:LOGarithmic] on page 432

##### Dwell Time - Level Sweep

Enters the dwell time and determines the duration of the individual sweep steps.

| --- | --- | --- |
| Time | Signal Level | Signal Level |
| Time | Signal Level | Signal Level |
| 0 | 0 | 0 |
| 0 | 0 | 0 |
| 1 | 0 | 0 |
| 1 | 0 | 0 |
| 2 | 0 | 0 |
| 2 | 0 | 0 |
| 3 | 0 | 0 |
| 3 | 0 | 0 |
| 4 | 0 | 0 |
| 4 | 0 | 0 |
| 5 | 0 | 0 |
| 5 | 0 | 0 |
| 6 | 0 | 0 |
| 6 | 0 | 0 |
| 7 | 0 | 0 |
| 7 | 0 | 0 |
| 8 | 0 | 0 |
| 8 | 0 | 0 |
| 9 | 0 | 0 |
| 9 | 0 | 0 |
| 10 | 0 | 0 |
| 10 | 0 | 0 |
| 11 | 0 | 0 |
| 11 | 0 | 0 |
| 12 | 0 | 0 |
| 12 | 0 | 0 |
| 13 | 0 | 0 |
| 13 | 0 | 0 |
| 14 | 0 | 0 |
| 14 | 0 | 0 |
| 15 | 0 | 0 |
| 15 | 0 | 0 |
| 16 | 0 | 0 |
| 16 | 0 | 0 |
| 17 | 0 | 0 |
| 17 | 0 | 0 |
| 18 | 0 | 0 |
| 18 | 0 | 0 |
| 19 | 0 | 0 |
| 19 | 0 | 0 |
| 20 | 0 | 0 |
| 20 | 0 | 0 |
| 21 | 0 | 0 |
| 21 | 0 | 0 |
| 22 | 0 | 0 |
| 22 | 0 | 0 |
| 23 | 0 | 0 |
| 23 | 0 | 0 |
| 24 | 0 | 0 |
| 24 | 0 | 0 |
| 25 | 0 | 0 |
| 25 | 0 | 0 |
| 26 | 0 | 0 |
| 26 | 0 | 0 |
| 27 | 0 | 0 |
| 27 | 0 | 0 |
| 28 | 0 | 0 |
| 28 | 0 | 0 |
| 29 | 0 | 0 |
| 29 | 0 | 0 |
| 30 | 0 | 0 |
| 30 | 0 | 0 |
| 31 | 0 | 0 |
| 31 | 0 | 0 |
| 32 | 0 | 0 |
| 32 | 0 | 0 |
| 33 | 0 | 0 |
| 33 | 0 | 0 |
| 34 | 0 | 0 |
| 34 | 0 | 0 |
| 35 | 0 | 0 |
| 35 | 0 | 0 |
| 36 | 0 | 0 |
| 36 | 0 | 0 |
| 37 | 0 | 0 |
| 37 | 0 | 0 |
| 38 | 0 | 0 |
| 38 | 0 | 0 |
| 39 | 0 | 0 |
| 39 | 0 | 0 |
| 40 | 0 | 0 |
| 40 | 0 | 0 |
| 41 | 0 | 0 |
| 41 | 0 | 0 |
| 42 | 0 | 0 |
| 42 | 0 | 0 |
| 43 | 0 | 0 |
| 43 | 0 | 0 |
| 44 | 0 | 0 |
| 44 | 0 | 0 |
| 45 | 0 | 0 |
| 45 | 0 | 0 |
| 46 | 0 | 0 |
| 46 | 0 | 0 |
| 47 | 0 | 0 |
| 47 | 0 | 0 |
| 48 | 0 | 0 |
| 48 | 0 | 0 |
| 49 | 0 | 0 |
| 49 | 0 | 0 |
| 50 | 0 | 0 |
| 50 | 0 | 0 |
| 51 | 0 | 0 |
| 51 | 0 | 0 |
| 52 | 0 | 0 |
| 52 | 0 | 0 |
| 53 | 0 | 0 |
| 53 | 0 | 0 |
| 54 | 0 | 0 |
| 54 | 0 | 0 |
| 55 | 0 | 0 |
| 55 | 0 | 0 |
| 56 | 0 | 0 |
| 56 | 0 | 0 |
| 57 | 0 | 0 |
| 57 | 0 | 0 |
| 58 | 0 | 0 |
| 58 | 0 | 0 |
| 59 | 0 | 0 |
| 59 | 0 | 0 |
| 60 | 0 | 0 |
| 60 | 0 | 0 |
| 61 | 0 | 0 |
| 61 | 0 | 0 |
| 62 | 0 | 0 |
| 62 | 0 | 0 |
| 63 | 0 | 0 |
| 63 | 0 | 0 |
| 64 | 0 | 0 |
| 64 | 0 | 0 |
| 65 | 0 | 0 |
| 65 | 0 | 0 |
| 66 | 0 | 0 |
| 66 | 0 | 0 |
| 67 | 0 | 0 |
| 67 | 0 | 0 |
| 68 | 0 | 0 |
| 68 | 0 | 0 |
| 69 | 0 | 0 |
| 69 | 0 | 0 |
| 70 | 0 | 0 |
| 70 | 0 | 0 |
| 71 | 0 | 0 |
| 71 | 0 | 0 |
| 72 | 0 | 0 |
| 72 | 0 | 0 |
| 73 | 0 | 0 |
| 73 | 0 | 0 |
| 74 | 0 | 0 |
| 74 | 0 | 0 |
| 75 | 0 | 0 |
| 75 | 0 | 0 |
| 76 | 0 | 0 |
| 76 | 0 | 0 |
| 77 | 0 | 0 |
| 77 | 0 | 0 |
| 78 | 0 | 0 |
| 78 | 0 | 0 |
| 79 | 0 | 0 |
| 79 | 0 | 0 |
| 80 | 0 | 0 |
| 80 | 0 | 0 |
| 81 | 0 | 0 |
| 81 | 0 | 0 |
| 82 | 0 | 0 |
| 82 | 0 | 0 |
| 83 | 0 | 0 |
| 83 | 0 | 0 |
| 84 | 0 | 0 |
| 84 | 0 | 0 |
| 85 | 0 | 0 |
| 85 | 0 | 0 |
| 86 | 0 | 0 |
| 86 | 0 | 0 |
| 87 | 0 | 0 |
| 87 | 0 | 0 |
| 88 | 0 | 0 |
| 88 | 0 | 0 |
| 89 | 0 | 0 |
| 89 | 0 | 0 |
| 90 | 0 | 0 |
| 90 | 0 | 0 |
| 91 | 0 | 0 |
| 91 | 0 | 0 |
| 92 | 0 | 0 |
| 92 | 0 | 0 |
| 93 | 0 | 0 |
| 93 | 0 | 0 |
| 94 | 0 | 0 |
| 94 | 0 | 0 |
| 95 | 0 | 0 |
| 95 | 0 | 0 |
| 96 | 0 | 0 |
| 96 | 0 | 0 |
| 97 | 0 | 0 |
| 97 | 0 | 0 |
| 98 | 0 | 0 |
| 98 | 0 | 0 |
| 99 | 0 | 0 |
| 99 | 0 | 0 |
| 100 | 0 | 0 |
| 100 | 0 | 0 |

</td></tr>
  </tbody>
</table>

The "Dwell Time" set by the user is used as the step time of the sweep. The effective net dwell time is shorter, reduced by the setting time. This setting time may be greater than the time specified in the data sheet.

##### Note:

It is recommended to switch off the display update for optimum sweep performance especially with short dwell times (see Chapter 4.2.3.6, "Display Update", on page 103).

Remote command:

[:SOURCE<hw>] :SWEep:POWER:DWELL on page 429

##### Ext. Trigger Input Slope

Sets the polarity of the active slope of an externally applied instrument trigger.

This setting affects the INST TRIG input (BNC connector at the rear of the instrument).

"Positive" activates the rising edge of the trigger signal.

"Negative" activates the falling edge of the trigger signal.

Remote command:

[ : SOURce ] : INPUT : TRIGger : SLOPe on page 354

##### 4.3.7.4 List Mode

Similar to a sweep, a series of previously defined frequency and level points is processed in List mode. In contrast to a sweep, however, a list with freely selectable value

pairs (frequency and level) can be created. The value range for frequency and level covers the entire configurable value range of the instrument.

i

</div>

</div>

Interactions between List mode and other operating modes or settings

- List mode and sweeps can not be activated simultaneously, they deactivate each other.

- Activating the list mode instantly disables NRP Level Control.

A running list mode blocks "NRP Level Control". It can not be activated

The lists can be created in the "List Editor". Each list is stored in its own file with the predefined file extension *.1sw. The name of the list file can be freely selected. The files are loaded from the "Lists..." file manager. Externally created tables with pairs of frequency and level values can be converted into List files using the import function. The external files must have the file extension *.txt or *.csv. These file formats are provided e.g. by the Microsoft® Excel program. The separators for table columns and for decimal floating-point numerals can be set. In addition, internally created List data can be exported into ASCII files using the export function.

The necessary hardware settings are calculated the first time a list is processed. With long dwell times, this calculation can be performed while the list is being processed; the entered dwell times are observed. With very short dwell times, calculation of the hardware settings increases the dwell time for the initial processing cycle; the entered value is only observed from the second processing cycle onwards. In this case a message appears to inform the user that there is a deviation between the current and set dwell times. No further calculations are required after the first run through a list. The current dwell times will definitely no longer deviate from the set dwell times.

The list is processed from the beginning to the end of the list (modes "Auto", ("External") "Single", ("External") "Step").

RF Sweep / List
Frequency Sweep...
Level Sweep...
List Mode...

</div>

</div>

##### List Mode Dialog

To open the "List Mode" menu, select "RF > Configure > List Mode" or use the [MENU] key under "RF".

The menu is used to activate/deactivate the operating mode List, to create, select and activate the lists, and to select the trigger mode and the dwell time.

| List Mode | ☐☒ |
| --- | --- |
| State | Off |
| Mode | Extern Single |
| Dwell Time | 10.000 ms |
| Current Index | 0 |
| Learn List Mode Data | Learn List Mode Data |
| List Mode Data... | None |
| Edit List Mode Data... | Edit List Mode Data... |
| List Range In: [ | 0 ; 9 ] |
| Import/Export >>> | Import/Export >>> |

##### General Settings

##### State - List Mode

Activates/deactivates the List mode. The currently selected list is processed.

In case of a new or modified list, the necessary hardware settings are automatically determined on activation of the list mode. The data determined in this way is stored along with the list and is available whenever the list is used again.

This means that when activating the list mode, the system checks whether any hardware settings are present. If so, the list is started immediately, but if not they are automatically determined (the list is learnt).

A "Learn List Mode Data" button is available for deliberately activating list learning.

Note: Activating the list mode automatically deactivates all sweeps. During list mode the frequency and level indications do not display the currently set values.

Remote command:

[:SOURCE<hw>] :FREQUENCY:MODE on page 349

##### Mode - List Mode

Selects the cycle mode of the List mode.

"Auto" Cycle from the beginning to the end of the list with automatic restart at the beginning. If a different mode was activated prior to the Auto mode, the cycle continues from the beginning of the list. The duration of a list step is determined by the set dwell time. Button "Reset" restarts the list at the starting point.

"Single" Single cycle from the beginning to the end of the list. If "Single" is selected, the cycle is not started immediately. The "Execute Single" button appears under the "Mode" line. The cycle is started with this button. The duration of a list step is determined by the set dwell time. Button "Reset" restarts the list at the starting point.

| Mode | Single |
| --- | --- |
| Execute Single |  |

"Step"

Manual, step-by-step processing of the list. Activating "Step" stops the current list and the cursor moves to the value displayed for "Current Index". It is now possible to scroll up and down in the list in discrete steps by varying the index. The duration of a list step is determined by the time between two index entries.

Button "Reset" restarts the list at the starting point.

Current Index 0

</div>

</div>

"Extern Single" Single cycle from the beginning to the end of the list as with "Single", but started by an external trigger.

The external trigger signal is input at the BNC connector [INST TRIG].

Button "Reset" restarts the list at the starting point.

"Extern Step" Step-by-step cycle using the external trigger signal. Each trigger event starts a single step. The duration of a list step is determined by the time between two trigger events. The external trigger signal is input at the BNC connector [INST TRIG]. Button "Reset" restarts the list at the starting point.

##### Remote command:

[ : SOURce<hw> ] : LIST : MODE on page 374

[ : SOURce<hw> ] : LIST : TRIGer : SOURce on page 376

##### Execute Single - List Mode

Triggers the list manually. This button is available only if mode "Single" is selected.

##### Remote command:

[:SOURce<hw>] :LIST:TRIGger:EXECute on page 375

##### Reset - List Mode

Resets the list to the starting point.

##### Remote command:

[:SOURCE<hw>] :LIST:RESET on page 375

##### Dwell Time - List Mode

Enters the dwell time. The dwell time determines the duration of a list step in list operating modes "Auto", "Single" and "External Single". In these modes a complete list is processed either once or continuously.

In list operating modes "Step" and "Extern Step", the set dwell time does not affect signal generation. In this case, the duration of a list step is determined by the time between two (internal or external) trigger events.

| --- | --- | --- |
| Time | Signal Level | Signal Level |
| Time | Signal Level | Signal Level |
| 0 | 0 | 0 |
| 0 | 0 | 0 |
| 1 | 0 | 0 |
| 1 | 0 | 0 |
| 2 | 0 | 0 |
| 2 | 0 | 0 |
| 3 | 0 | 0 |
| 3 | 0 | 0 |
| 4 | 0 | 0 |
| 4 | 0 | 0 |
| 5 | 0 | 0 |
| 5 | 0 | 0 |
| 6 | 0 | 0 |
| 6 | 0 | 0 |
| 7 | 0 | 0 |
| 7 | 0 | 0 |
| 8 | 0 | 0 |
| 8 | 0 | 0 |
| 9 | 0 | 0 |
| 9 | 0 | 0 |
| 10 | 0 | 0 |
| 10 | 0 | 0 |
| 11 | 0 | 0 |
| 11 | 0 | 0 |
| 12 | 0 | 0 |
| 12 | 0 | 0 |
| 13 | 0 | 0 |
| 13 | 0 | 0 |
| 14 | 0 | 0 |
| 14 | 0 | 0 |
| 15 | 0 | 0 |
| 15 | 0 | 0 |
| 16 | 0 | 0 |
| 16 | 0 | 0 |
| 17 | 0 | 0 |
| 17 | 0 | 0 |
| 18 | 0 | 0 |
| 18 | 0 | 0 |
| 19 | 0 | 0 |
| 19 | 0 | 0 |
| 20 | 0 | 0 |
| 20 | 0 | 0 |
| 21 | 0 | 0 |
| 21 | 0 | 0 |
| 22 | 0 | 0 |
| 22 | 0 | 0 |
| 23 | 0 | 0 |
| 23 | 0 | 0 |
| 24 | 0 | 0 |
| 24 | 0 | 0 |
| 25 | 0 | 0 |
| 25 | 0 | 0 |
| 26 | 0 | 0 |
| 26 | 0 | 0 |
| 27 | 0 | 0 |
| 27 | 0 | 0 |
| 28 | 0 | 0 |
| 28 | 0 | 0 |
| 29 | 0 | 0 |
| 29 | 0 | 0 |
| 30 | 0 | 0 |
| 30 | 0 | 0 |
| 31 | 0 | 0 |
| 31 | 0 | 0 |
| 32 | 0 | 0 |
| 32 | 0 | 0 |
| 33 | 0 | 0 |
| 33 | 0 | 0 |
| 34 | 0 | 0 |
| 34 | 0 | 0 |
| 35 | 0 | 0 |
| 35 | 0 | 0 |
| 36 | 0 | 0 |
| 36 | 0 | 0 |
| 37 | 0 | 0 |
| 37 | 0 | 0 |
| 38 | 0 | 0 |
| 38 | 0 | 0 |
| 39 | 0 | 0 |
| 39 | 0 | 0 |
| 40 | 0 | 0 |
| 40 | 0 | 0 |
| 41 | 0 | 0 |
| 41 | 0 | 0 |
| 42 | 0 | 0 |
| 42 | 0 | 0 |
| 43 | 0 | 0 |
| 43 | 0 | 0 |
| 44 | 0 | 0 |
| 44 | 0 | 0 |
| 45 | 0 | 0 |
| 45 | 0 | 0 |
| 46 | 0 | 0 |
| 46 | 0 | 0 |
| 47 | 0 | 0 |
| 47 | 0 | 0 |
| 48 | 0 | 0 |
| 48 | 0 | 0 |
| 49 | 0 | 0 |
| 49 | 0 | 0 |
| 50 | 0 | 0 |
| 50 | 0 | 0 |
| 51 | 0 | 0 |
| 51 | 0 | 0 |
| 52 | 0 | 0 |
| 52 | 0 | 0 |
| 53 | 0 | 0 |
| 53 | 0 | 0 |
| 54 | 0 | 0 |
| 54 | 0 | 0 |
| 55 | 0 | 0 |
| 55 | 0 | 0 |
| 56 | 0 | 0 |
| 56 | 0 | 0 |
| 57 | 0 | 0 |
| 57 | 0 | 0 |
| 58 | 0 | 0 |
| 58 | 0 | 0 |
| 59 | 0 | 0 |
| 59 | 0 | 0 |
| 60 | 0 | 0 |
| 60 | 0 | 0 |
| 61 | 0 | 0 |
| 61 | 0 | 0 |
| 62 | 0 | 0 |
| 62 | 0 | 0 |
| 63 | 0 | 0 |
| 63 | 0 | 0 |
| 64 | 0 | 0 |
| 64 | 0 | 0 |
| 65 | 0 | 0 |
| 65 | 0 | 0 |
| 66 | 0 | 0 |
| 66 | 0 | 0 |
| 67 | 0 | 0 |
| 67 | 0 | 0 |
| 68 | 0 | 0 |
| 68 | 0 | 0 |
| 69 | 0 | 0 |
| 69 | 0 | 0 |
| 70 | 0 | 0 |
| 70 | 0 | 0 |
| 71 | 0 | 0 |
| 71 | 0 | 0 |
| 72 | 0 | 0 |
| 72 | 0 | 0 |
| 73 | 0 | 0 |
| 73 | 0 | 0 |
| 74 | 0 | 0 |
| 74 | 0 | 0 |
| 75 | 0 | 0 |
| 75 | 0 | 0 |
| 76 | 0 | 0 |
| 76 | 0 | 0 |
| 77 | 0 | 0 |
| 77 | 0 | 0 |
| 78 | 0 | 0 |
| 78 | 0 | 0 |
| 79 | 0 | 0 |
| 79 | 0 | 0 |
| 80 | 0 | 0 |
| 80 | 0 | 0 |
| 81 | 0 | 0 |
| 81 | 0 | 0 |
| 82 | 0 | 0 |
| 82 | 0 | 0 |
| 83 | 0 | 0 |
| 83 | 0 | 0 |
| 84 | 0 | 0 |
| 84 | 0 | 0 |
| 85 | 0 | 0 |
| 85 | 0 | 0 |
| 86 | 0 | 0 |
| 86 | 0 | 0 |
| 87 | 0 | 0 |
| 87 | 0 | 0 |
| 88 | 0 | 0 |
| 88 | 0 | 0 |
| 89 | 0 | 0 |
| 89 | 0 | 0 |
| 90 | 0 | 0 |
| 90 | 0 | 0 |
| 91 | 0 | 0 |
| 91 | 0 | 0 |
| 92 | 0 | 0 |
| 92 | 0 | 0 |
| 93 | 0 | 0 |
| 93 | 0 | 0 |
| 94 | 0 | 0 |
| 94 | 0 | 0 |
| 95 | 0 | 0 |
| 95 | 0 | 0 |
| 96 | 0 | 0 |
| 96 | 0 | 0 |
| 97 | 0 | 0 |
| 97 | 0 | 0 |
| 98 | 0 | 0 |
| 98 | 0 | 0 |
| 99 | 0 | 0 |
| 99 | 0 | 0 |
| 100 | 0 | 0 |
| 100 | 0 | 0 |

</td></tr>
  </tbody>
</table>

The "Dwell Time" set by the user is used as the step time of the list mode. The effective net dwell time is shorter, reduced by the setting time. This setting time may be greater than the time specified in the data sheet.

##### Remote command:

[:SOURCE<hw>] :LIST:DWELL on page 370

##### Current Index - List Mode

Sets the list index in "Step" mode.

##### Remote command:

[:SOURCE<hw>] :LIST:INDEX on page 372

##### Learn List Mode Data... - List Mode

Starts the determination of the hardware setting for the selected list. The data determined in this way is stored along with the list.

It may be necessary to deliberately activate list learning in the event of greatly altered environmental conditions that require new hardware settings.

If this is not done, a previously learned hardware setting will continue to be used when list mode is switched on ("State = On"). If no setting is available, e.g. when the list is used for the first time, learning is automatically activated.

Remote command:

[:SOURCE<hw>] :LIST:LEARN on page 373

##### List Mode Data... - List Mode

Calls the "File Select" menu for selecting and creating a list or the "File Manager".

Select List
New List
File Manager...

##### Remote command:

[:SOURce<hw>]:LIST:SELECT on page 375
[:SOURce<hw>]:LIST:DELETE on page 366
[:SOURce<hw>]:LIST:DELETE:ALL on page 366

##### Edit List Mode Data... - List Mode

Calls the editor for editing the selected list. A list consists of any number of frequency/level value pairs. The currently selected list is displayed.

| Edit List Mode Data data_list | Edit List Mode Data data_list | Edit List Mode Data data_list | Edit List Mode Data data_list |
| --- | --- | --- | --- |

"Frequency /H Enter the frequency of the frequency/power value pair. z"

##### Remote command:

[:SOURCE<hw>] :LIST:FREQUency on page 371

"Power /dBm" Enter the level of the frequency/power value pair.

##### Remote command:

[:SOURCE<hw>] :LIST:POWER on page 374

"Goto"

Selects row for editing.

Goto first
Goto last
Goto row...

</div>

</div>

If "Goto row" is selected, a window opens for entering the requested row.

goto line:
OK    Cancel

</div>

</div>

"Edit"

Calls a selection of possible actions described below.

| Insert Row |
| --- |
| Insert Range... |
| Fill... |
| Delete Row |
| Delete Range... |

"Insert Row" Inserts a new row before the marked row.

"Insert Range" Inserts new rows before the marked row. The number of rows to be inserted can be defined in an entry window.

Rows to Insert
OK | Cancel

</div>

</div>

"Fill...." Opens a sub menu for defining a set of list values to be automatically entered in the List Mode table (see "Filling the List Mode Data automatically" on page 200).

"Delete Row" Deletes the marked row.

"Delete  Allows to delete any number of rows starting with the marked row.

Range..." The number of rows to be deleted can be defined in an entry window.

Rows to Delete
OK
Cancel

</div>

</div>

"Save" The list is saved under its current name.

##### List Range In - List Mode

Defines an index range in the current list by setting the start and stop index. Only the values in the selected index range are processed in List mode, all other list entries are ignored.

##### Remote command:

[:SOURCE<hw>] :LIST:INDEX:START on page 372

[:SOURCE<hw>] :LIST:INDEX:STOP on page 373

##### Ext. Trigger Input Slope

Sets the polarity of the active slope of an externally applied instrument trigger.

This setting affects the INST TRIG input (BNC connector at the rear of the instrument).

"Positive" activates the rising edge of the trigger signal.

"Negative" activates the falling edge of the trigger signal.

##### Remote command:

[ : SOURce ] : INPUT : TRIGger : SLOPe on page 354

##### Import/Export

Lists can be imported from externally created files or exported into text or CSV-files. The import/export settings are available after clicking the "Import/Export" button.

##### Import/Export - List Mode

Expands the menu with the area for import and export of list mode files.

| <<Hide Import/Export | <<Hide Import/Export |
| --- | --- |
| -Import / Export List Files | -Import / Export List Files |
| Mode | Import |
| -ASCII File Settings | -ASCII File Settings |
| Extension | TXT |
| Decimal Point | Point |
| Column Separator | Semicolon |
| Select ASCII Source | Select ASCII Source |
| None | None |
| Select Destination | Select Destination |
| None | None |
| Import | Import |

Externally edited Excel tables with frequency/level pairs can be imported as text or CSV-files and used for list mode.

On the other hand, internally created list mode lists can be exported as text or CSV-files.

##### Mode - List Mode

Selects if list mode lists should be imported or exported. The settings offered below depend on the selected mode.

##### Remote command:

[:SOURCE<hw>] :LIST:DEXChange:MODE on page 370

##### Extension - List Mode

Selects the file extension of the ASCII file to be imported or exported. Selection TXT (text file) or CSV (Excel file) is available.

##### Remote command:

[:SOURCE<hw>] :LIST:DEXChange:AFILe:EXTension on page 367

##### Decimal Point - List Mode

Selects the decimal separator used in the ASCII data between '.' (decimal point) and ',' (comma) with floating-point numerals.

##### Remote command:

[:SOURce<hw>] :LIST:DEXChange:AFILE:SEParator:DECimal on page 369

##### Column Separator- List Mode

Selects the separator between the frequency and level column of the ASCII table.

##### Remote command:

[:SOURCE<hw>] :LIST:DEXChange:AFILE:SEParator:COLumn on page 368

##### Select ASCII Source / Destination - List Mode

Calls the "File Manager" for selecting the ASCII file to be imported into a list mode list (source) or the ASCII file the list mode list is exported (destination) in.

##### Remote command:

[:SOURce<hw>] :LIST:DEXChange:AFILE:SELECT on page 368

##### Select Destination / Source - List Mode

Calls the "File Manager" for selecting the list mode list to be exported (source) into an ASCII file or the destination for the ASCII file to be imported (destination) in.

##### Remote command:

[:SOURce<hw>] :LIST:DEXChange:SELECT on page 370

##### Import / Export - List Mode

Starts the export or import of the selected file.

When import is selected, the ASCII file is imported as list mode list.

When export is selected, the list mode list is exported into the selected ASCII file.

##### Remote command:

[:SOURCE<hw>] :LIST:DEXChange:EXECute on page 369

##### Filling the List Mode Data automatically

The "Fill List Mode Data" menu enables you to automatically set the values in the List Mode table.

| Fill List Mode Data | Fill List Mode Data | Fill List Mode Data |
| --- | --- | --- |
| From | 1 |  |
| Range | 3 |  |
| Select column to fill | Frequency/Hz |  |
| Start Value | 9 000.000 | Hz |
| End Value | 9 200.000 | Hz |
| Increment Value | 100.000 | Hz |

The start line and the number of rows to be filled are defined under "From" and "Range".

The column to be filled is selected under "Select column to fill". Depending on the selection here, the default for start, end, and increment value are set. As the settings are interdependent, a change of one parameter may result in the automatic change of one or more of the other parameters.

The filling of the column with the selected value settings is started with button "Fill".

i

</div>

</div>

The list entries are only computed when the "Fill" button is pressed.

##### From

Sets the start value of the index range.

Remote command:

n.a.

##### Range

Sets the range for filling the table.

Remote command:

n.a.

Select column to fill

Selects either the frequency or the level column to be filled with the value defined below.

Remote command:

n.a.

Start value

Sets the start value for the frequency or the level entries.

Remote command:

n.a.

End value

Sets the end value for the frequency or the level entries.

Remote command:

n.a.

Increment value

Sets the increment for the frequency or the level entries.

Remote command:

n.a.

Fill

Fills the selected column in the set range with values, starting with the start value and using the set increment.

Remote command:

n.a.

### 4.4 Modulation

#### 4.4.1 Overview of Modulation

Analog modulation is a method used to transmit information of an LF (Low Frequency) signal in accordance with a second signal, typically one of a higher frequency. This is done by varying one or more properties of a high frequency waveform, called the modulation or carrier signal, with the modulating signal that contains the information to be transmitted.

The three key parameters of the modulation signal are the amplitude, phase and frequency. These parameters are modified in accordance with the low frequency signal to obtain the modulated RF signal.

The R&S SMB provides the following types of modulation:

• AM (Amplitude Modulation)

• FM (Frequency Modulation)

• PhiM (Phase Modulation)

• PULM (Pulse Modulation)

• Stereo Modulation

In addition, the RF signal can be modulated with various internally generated modulation waveforms, like sine or rectangular signal waves. The basic unit (R&S SMB + frequency option) provides amplitude, frequency and phase modulation without additional equipment options, as well as a standard LF generator provided for internal modulation. Further available options are:

R&S SMB-B5, Stereo/RDS Coder for performing stereo modulation

• R&S SMB-K21, Pulse Modulation for instruments with high frequency options

- R&S SMB-K22, Pulse Modulation for instruments equipped with frequency-options up to 6 GHz

R&S SMB-K23, Pulse Generator for pulse signals

- R&S SMB-K27 Pulse Train for generating pulse train signals (only for instruments with serial number higher than 102400)

Settings for the modulation are made in separate modulation menus. These menus can be accessed in the block diagram by way of the "Modulation" function block, or by means of the menu with the same name which is opened using the [MENU] key.

##### 4.4.1.1 Enabling/Disabling Analog Modulations using the MOD On/Off Key

The [MOD ON/OFF] key switches the modulations on and off.

##### [MOD ON/OFF]

Press the [MOD ON/OFF] key to enable/disable analog modulations.

Pressing the key again restores the status that was active before the last switch-off. "MOD OFF" is displayed in the info line of the header next to the "Level" field.

Remote command:

[:SOURCE<hw>] :MODulation[:ALL][:STATE] on page 376

##### 4.4.1.2 Modulation Sources

The following modulations use internal and external modulation sources:

• Amplitude modulation

• Pulse modulation

• Frequency modulation

• Phase modulation

##### Internal Modulation Sources

An LF generator and a pulse generator are available as internal modulation sources for a fully equipped instrument. The LF generator supplies sinusoidal or rectangular signals.

The optional pulse generator (option R&S SMB-K27) provides single and double pulse modulation with selectable pulse widths and periods or a user-definable pulse train.

See also Chapter 4.5.1, "Overview of LF Generator", on page 223.

##### External Modulation Sources

The modulation input [MOD EXT] at the instrument front provides the external modulation sources for amplitude, frequency and phase modulation.

The external audio signal for stereo modulation is input via the analog [L] and [R] inputs or via the digital [S/P DIF] interface at the rear of the instrument.

The external modulation signal for AM, FM and PM at the input must have a voltage of  $ U_S = 1 \, V $ ( $ U_{EFF} = 0.707 \, V $) in order to achieve the displayed modulation depth and range. The input voltage should not exceed 1 V, otherwise modulation distortions might occur.

i

</div>

</div>

##### Considerations to AM when using an external modulation signal:

With Mod Ext Coupling > DC, the RF output signal behaves according to:

- input signal = 0 V: the RF output amplitude corresponds to the level value set in the R&S SMB

- input signal = +1 V: the output level increases up to the maximum value given by the set modulation sensitivity

- input signal = -1 V: the output level decreases down to the minimum value given by the set modulation sensitivity

With Mod Ext Coupling > AC, the modulation input signal is internally highpass filtered. Therefore, the DC content of the input signal is removed before it reaches the amplitude modulator.

The [PULSE EXT] connector at the rear of the instrument controls the external pulse modulation. The input shows some hysteresis with threshold levels of 0.5 V/1.5 V. The voltage must not exceed 5 V.

##### Simultaneous Operation of Several Modulations or Other Operating Modes

The table shows the modulations and operating modes which can be activated simultaneously (+) or which deactivate each other (-).

|  | AM | FM | PhiM | Pulse |
| --- | --- | --- | --- | --- |
| Amplitude modulation (AM) | / | + | + | (+) |
| Frequency modulation (FM) | + | / | - | + |
| Phase modulation (PhiM) | + | - | / | + |
| Pulse modulation (Pulse) | (+) | + | + | / |

#### 4.4.2 Amplitude Modulation (AM)

An internal and/or external source can be selected for amplitude modulation. The LF modulation generator is available as the internal source.

Two-tone AM is possible by simultaneously switching on the external and internal source.

The [MOD EXT] input connector for external feed of analog modulation signals is at the front of the instrument. The coupling mode of the input (AC or DC) can be selected.

The AM modulation depth is limited by the maximum peak envelope power (PEP).

##### Exponential AM (Instruments with high frequency options)

Besides the linear amplitude modulation, whereby the signal voltage is proportional to the modulation signal, instruments equipped with the frequency options (R&S SMB-B112(L)/-B120(L)/-B140(L)) provide a level-proportional power or amplitude modulation.

In this case, the R&S SMB exponentially distorts the modulation signal, before it is output at the [LF connector] - regardless of the AM Source Int, or Ext. The AM Depth is then indicated in dB.

i

</div>

</div>

##### Signal Sources for Exponential AM

You can perform exponential AM using either the internal, or an external modulation signal. However, in contrast to linear AM, the signal at the LF output connector is distorted in any operating mode. AM Source Int+Ext is not available.

##### 4.4.2.1 Amplitude Modulation Settings

Modulation

Amplitude Modulation.

Frequency Modulation...

Phase Modulation...

Pulse Modulation...

To open the "Amplitude Modulation" dialog, select "Modulation > Configure > Amplitude Modulation" or use the [MENU] key under "Modulation".

In the upper section of the dialog, the modulation source is selected and the modulation switched on. The modulation source can be selected independently for the different modulation types and the LF output.

The configuration of the selected external and/or internal modulation source is performed in the lower section of the dialog or in the "LF Output" dialog (internal source only).

These settings affect all modulations which use the same modulation source.

| Amplitude Modulation |
| --- |

##### State

Activates amplitude modulation.

Remote command:

[:SOURCE<hw>] :AM:STATE on page 333

##### AM Source

Selects the source for the AM modulation signal.

"Internal" Uses the internal LF generator as modulation signal source for AM.

"External" Uses an externally applied modulation signal.

The external signal is input via the [MOD EXT] connector.

"Intern + Extern"

Uses both, the internal and externally applied modulation signal, for example to perform two-tone AM.

Note: This setting applies to linear AM, see "Exponential AM (Instruments with high frequency options)" on page 204.

Remote command:

[:SOURCE<hw>] :AM:SOURCE on page 333

##### AM Type

Selects between linear or exponential (logarithmic) amplitude modulation, if you work with an instrument that is equipped with a 12 GHz, or higher frequency option.

Remote command:

[:SOURCE<hw>] :AM:TYPE on page 334

##### AM Depth

Sets the modulation depth in percent.

Note: With two-tone modulation, observe that the set modulation depth applies to both signals and the sum modulation depth is determined by doubling the set modulation depth. This results in overmodulation if the maximal value for modulation depth is exceeded (see data sheet).

For instruments with frequency option 12 GHz or higher, you can additionally select AM Type Exponential. In this case, the generator sets modulation depth in dB (logarithmic).

Modulation is possible both, upwards and downwards. Accordingly, the dynamic range extends for instruments without attenuator from minimum to maximum level. For instruments with attenuator, the dynamic range corresponds to the Fixed Range (PEP) In; these are downwards about 20 dB, and upwards about 5 dB, that means in total about 25 dB around the set level.

##### Effects of positive/negative modulation depth:

##### AM Source Int

– positive depth -> downwards modulation

– negative depth -> upwards modulation

##### • AM Source Ext

– positive depth and negative external voltage -> downwards modulation

– positive depth and positive external voltage -> upwards modulation

– negative depth and negative external voltage -> upwards modulation

– negative depth and positive external voltage -> downwards modulation

##### Remote command:

[:SOURCE<hw>] :AM:DEPTH:LINEAR on page 332

[:SOURCE<hw>] :AM:DEPTH:EXPonential on page 331

##### LF Gen Freq

Sets the frequency of the LF generator.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

[ :SOURCE] :LFOutput<ch>:FREQUency on page 355

##### LF Gen Shape

Selects the waveform shape of the LF signal.

| LFGen Shape | Sine |
| --- | --- |
| Source Impedance | Sine Square Triangle Sawtooth Inv. Sawtooth |

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

##### Remote command:

[:SOURCE]:LFOutput:SHAPe on page 363

##### AM Sensitivity

Displays the input sensitivity of the externally applied modulation signal at the [MOD EXT] input in %/V in AM Type Linear mode, and dB/V in AM Type Exponential mode.

The modulation depth entered under AM Depth is achieved with 1 Volt modulation of the input.

##### Remote command:

[:SOURCE<hw>] :AM:SENSitivity? on page 332

##### Mod Ext Coupling

Selects the coupling mode (AC or DC) for external feed.

Note: Coupling for external feed via input [MOD EXT] can be set independently for all modulations using the external modulation signal.

"AC" Disconnects the DC voltage component and uses only the AC component of the modulation signal.

"DC" Uses the modulation signal with both components, AC and DC.

Remote command:

[:SOURce<hw>] :AM:EXTERNAL:COUPling on page 332

##### Ext. Impedance

(Source "External" only)

Sets the impedance for the external modulation signal, applied at the [MOD EXT] connector.

You can select 600 Ohm or high (>100 kOhm).

This setting affects all analog modulations which use the external modulation signal.

##### Remote command:

[ : SOURce<hw> ] : INPUT : MODExt : IMPedance on page 353

##### Ignore Overvoltage Warning

Suppresses warnings the instrument generates when the modulation signal input is overloaded.

This function prevents a warning caused by signals, that generally comply with the specification, but temporarily overload the input, for example due to spikes. The warning is suppressed in the history, and in the error queue.

Note: This setting is not affected by an instrument preset ([preset] key), *rst or the Save/Recall function. Only the factory preset resets (enables) this setting.

##### Remote command:

[:SOURce<hw>] :INPUT:MODExt:WIGNore on page 353

#### 4.4.3 Frequency Modulation (FM)

An internal and/or external source can be selected for frequency modulation. The LF GEN modulation generator is available as the internal source. Two-tone FM is possible by simultaneously switching on the external and internal source.

The [MOD EXT] input connectors for external feed of analog modulation signals are at the front of the instrument. The coupling mode of the input (AC or DC) can be selected.

Selection between three modulation modes is possible:

• "Normal" mode with full setting range for modulation bandwidth and FM deviation.

- "Low Noise" mode with better signal/noise ratio, but reduced setting range for modulation bandwidth

- "High Deviation" mode with full setting range for FM deviation and a reduced setting range for modulation bandwidth (see data sheet).

i

</div>

</div>

It is not possible to use frequency modulation simultaneously with phase modulation. See "Simultaneous Operation of Several Modulations or Other Operating Modes" on page 203 for an overview in detail.

##### 4.4.3.1 Frequency Modulation Settings

Modulation Amplitude Modulation Frequency Modulation Phase Modulation Pulse Modulation

</div>

</div>

To access the "Frequency Modulation" dialog, select "Modulation > Configure > Frequency Modulation" or use the [MENU] key under "Modulation".

In the upper section of the dialog, you can select the modulation source and activate modulation. The modulation source can be selected independently for the different modulation types and the LF output.

The configuration of the selected external and/or internal modulation source is performed in the lower section of the menu (internal source only).

These settings affect all modulations which use the same modulation sources.

| Frequency Modulation | Frequency Modulation | Frequency Modulation |
| --- | --- | --- |

##### State

Activates frequency modulation.

Activation of FM deactivates phase modulation.

Remote command:

[ : SOURCe<hw> ] : FM: STATE on page 346

##### FM Source

Selects the source for the FM signal.

"Internal" Uses the internal LF generator as modulation signal source for FM.

"External" Uses an externally applied modulation signal.

The external signal is input via the [MOD EXT] connector.

"Internal + External"

Uses both, the internal and externally applied modulation signal, for example to perform two-tone FM.

##### Remote command:

[ : SOURce<hw> ] : FM: SOURce on page 345

##### FM Mode

Selects the mode for the frequency modulation.

"Normal" The maximum range for modulation bandwidth and FM deviation is available.

"Low Noise" Frequency modulation with phase noise and spurious characteristics close to CW mode. The ranges of modulation bandwidth and FM deviation are reduced (see data sheet).

"High Deviation"

Frequency modulation with full setting range for FM deviation. The range of modulation bandwidth is reduced (see data sheet).

##### Remote command:

[ : SOURce<hw> ] : FM:MODE on page 344

##### FM Deviation

Sets the modulation deviation in Hz.

The maximum deviation depends on the RF frequency and the modulation mode (see data sheet).

Note that you can set a deviation that is too high for a specific RF frequency, or set an RF frequency outside of the adjustable range of the deviation. In both cases, the instrument sets the maximum deviation and displays an error message.

In "Int + Ext" modulation source mode, the instrument devides the deviation into half for each source.

##### Remote command:

[:SOURCE<hw>]:FM[:DEViation] on page 343

[:SOURCE<hw>] :FM:INTERNAL:DEViation on page 344

[:SOURce<hw>] :FM:EXTERNAL:DEViation on page 344

##### LF Gen Freq

Sets the frequency of the LF generator.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

[:SOURce]:LFOutput<ch>:FREQUency on page 355

##### LF Gen Shape

Selects the waveform shape of the LF signal.

| LFGen Shape | Sine |
| --- | --- |
| Source Impedance | Sine Square Triangle Sawtooth Inv. Sawtooth |

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

##### Remote command:

[:SOURCE]:LFOutput:SHAPe on page 363

##### FM Sensitivity

Displays the input sensitivity of the externally applied modulation signal at the [MOD EXT] input in Hz/V.

The modulation deviation entered with FM Deviation is achieved with 1 Volt (=  $ U_{peak} $) of the input signal.

Note: The input voltage must not exceed 1.1 V $ _{p} $ otherwise modulation distortions occur.

##### Remote command:

[:SOURCE<hw>] :FM:SENSitivity? on page 345

##### Adjust FM Offset

Starts the adjustment for the FM/PhiM modulator. The option is adjusted concerning DC-offset.

##### Remote command:

:CALibration<hw>:FMOFfset[:MEASure]? on page 291

##### Mod Ext Coupling

(Source "External" only)

Selects the coupling mode (AC or DC) for the externally applied frequency modulation signal.

Note: Coupling for external feed via input [MOD EXT] can be set independently for all modulations using the external modulation signal.

"AC" Disconnects the DC voltage component and uses only the AC component of the modulation signal.

"DC" Uses the modulation signal with both components, AC and DC.

##### Remote command:

[:SOURCE<hw>] :FM:EXTERNAL:COUPLING on page 343

##### Ext. Impedance

(Source "External" only)

Sets the impedance for the external modulation signal, applied at the [MOD EXT] connector.

You can select 600 Ohm or high (>100 kOhm).

This setting affects all analog modulations which use the external modulation signal.

##### Remote command:

[:SOURCE<hw>] : INPUT : MODExt : IMPedance on page 353

##### Ignore Overvoltage Warning

Suppresses warnings the instrument generates when the modulation signal input is overloaded.

This function prevents a warning caused by signals, that generally comply with the specification, but temporarily overload the input, for example due to spikes. The warning is suppressed in the history, and in the error queue.

Note: This setting is not affected by an instrument preset ([preset] key), *rst or the Save/Recall function. Only the factory preset resets (enables) this setting.

Remote command:

[:SOURce<hw>] :INPUT:MODExt:WIGNore on page 353

#### 4.4.4 Phase Modulation (PhiM)

i

</div>

</div>

It is not possible to use phase modulation simultaneously with frequency modulation. See "Simultaneous Operation of Several Modulations or Other Operating Modes" on page 203 for an overview in detail.

An internal and/or external source can be selected for phase modulation. The [LF GEN] modulation generator is available as the internal source.

The [MOD EXT] input connector for external feed of analog modulation signals is at the front of the instrument. The coupling mode of the input (AC or DC) and the impedance can be selected.

Selection between the following modulation modes is possible:

- "Normal" mode with full setting range for modulation bandwidth and PhiM deviation.

- "High Deviation" mode with full setting range for PhiM deviation and a reduced setting range for modulation bandwidth. Phase noise is reduced in the lower modulation frequency range compared to the default mode.

- "Low Noise" mode with better signal/noise ratio, but reduced setting range for modulation bandwidth and deviation (see data sheet)

##### 4.4.4.1 Phase Modulation Dialog

Modulation Amplitude Modulation. Frequency Modulation. Phase Modulation. Pulse Modulation.

To open the "Phase Modulation" dialog, select "Modulation > Configure > Phase Modulation" or use the [MENU] key under "Modulation".

| Phase Modulation | Phase Modulation | Phase Modulation |
| --- | --- | --- |

In the upper section of the dialog, the modulation source is selected and the modulation switched on. The modulation source can be selected independently for the different modulation types and the LF output.

The configuration of the selected external and/or internal modulation source is performed in the lower section of the dialog (internal source only).

These settings affect all modulations which use the same modulation sources.

An LF generator and a pulse generator are available as internal sources.

##### State

Activates PhiM modulation.

Activation of PhiM deactivates frequency modulation.

##### Remote command:

[ : SOURce<hw> ] : PM: STATE on page 381

##### PhiM Source

Selects the source for the PhiM signal.

"Internal" Uses the internal LF generator as the modulation signal source for PhiM.

"External" Uses an externally applied modulation signal. The external signal is input via the [MOD EXT] connector.

"Internal + External"

Uses both, the internal and externally applied modulation signal.

##### Remote command:

[ : SOURce<hw> ] : PM: SOURce on page 381

##### PhiM Mode

Selects the mode for the phase modulation.

"Normal"

The full range for modulation bandwidth and PM deviation is available.

##### "High Deviation"

The maximum range for  $ \phi $M deviation is available. Phase noise is improved for low frequencies compared to the default mode. The range of modulation frequency is limited (see data sheet). This mode is recommended for low modulation frequencies and/or high PhiM deviation.

"Low Noise" Phase modulation with phase noise and spurious characteristics close to CW mode. The range for modulation bandwidth and PM deviation is limited (see data sheet).

##### Remote command:

[ : SOURce<hw> ] : PM:MODE on page 380

##### PhiM Deviation

Sets the modulation deviation in RAD.

The maximum deviation depends on the set RF frequency and the selected modulation mode (see data sheet).

If the entered deviation is too high for the set RF frequency, the instrument provides the maximum value and displays an error message. The same applies, if the RF frequency is set to a value, at which the deviation cannot be determined.

The deviation of the internal source must not exceed the deviation of the external source in case of modulation source "Int+Ext".

##### Remote command:

[:SOURCE<hw>]:PM[:DEViation] on page 379

[:SOURCE<hw>] :PM:INTERNAL:DEViation on page 380

[:SOURCE<hw>] :PM:EXTERNAL:DEViation on page 379

##### LF Gen Freq

Sets the frequency of the LF generator.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

[:SOURCE]:LFOutput<ch>:FREQUency on page 355

##### LF Gen Shape

Selects the waveform shape of the LF signal.

| LFGen Shape | Sine |
| --- | --- |
| Source Impedance | Sine Square Triangle Sawtooth Inv. Sawtooth |

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

Remote command:

[:SOURCE]:LFOutput:SHAPe on page 363

##### PhiM Sensitivity

Displays the input sensitivity of the externally applied modulation signal at the [MOD EXT] input in RAD/V.

The modulation deviation entered with PhiM Deviation"PhiM Deviation" is achieved with 1 Volt (=U_{peak}) of the input signal.

Note: The input voltage must not exceed 1.1 V $ _{p} $ otherwise modulation distortions occur.

Remote command:

[:SOURCE<hw>] :PM:SENSitivity? on page 381

##### Mod Ext Coupling

Selects the coupling mode ("AC" or "DC") for the external modulation signal.

Note: Coupling for external feed via input [MOD EXT] can be set independently for all modulations using the external modulation signal.

"AC" Disconnects the DC voltage component and uses only the AC component of the modulation signal.

DC" Uses the modulation signal with both components, AC and DC.

##### Remote command:

[:SOURce<hw>] :PM:EXTERNAL:COUPling on page 379

##### Ext. Impedance

(Source "External" only)

Sets the impedance for the external modulation signal, applied at the [MOD EXT] connector.

You can select 600 Ohm or high (>100 kOhm).

This setting affects all analog modulations which use the external modulation signal.

##### Remote command:

[:SOURce<hw>] :INPUT:MODExt:IMPedance on page 353

##### Ignore Overvoltage Warning

Suppresses warnings the instrument generates when the modulation signal input is overloaded.

This function prevents a warning caused by signals, that generally comply with the specification, but temporarily overload the input, for example due to spikes. The warning is suppressed in the history, and in the error queue.

Note: This setting is not affected by an instrument preset ([preset] key), *rst or the Save/Recall function. Only the factory preset resets (enables) this setting.

Remote command:

[:SOURCE<hw>] :INPUT:MODExt:WIGNore on page 353

#### 4.4.5 Pulse Modulation (PM)

• option Pulse Modulator (R&S SMB-K22)

The available options for performing pulse modulation include:

- option Pulse Generator (R&S SMB-K23), comprises "Single" and "Double" pulse generation

• option Pulse Train (R&S SMB-K27), enables generation of pulse trains.

As modulation signal, you can either use the signal of the internal pulse generator or an externally supplied signal. In case of external source, the external signal is input via the [PULSE EXT] connector at the rear of the instrument. In case of internal source, this connector can be used as external trigger or gate signal input for internal pulse modulation. The polarity and input impedance of the connector can be selected.

The pulse signal is output at the [PULSE VIDEO] connector at the rear of the instrument.

i

</div>

</div>

##### Automatic Level Control is deactivated with pulse modulation!

When pulse modulation is activated, the R&S SMB deactivates ALC automatically ("ALC OFF", i.e. switches to "Sample & Hold" state).

The "Sample & Hold" state opens the ALC loop, and disables the automatic control of the output level. The level modulator is set directly.

However, to correct the output level, the R&S SMB executes a "Sample & Hold" measurement after each change of frequency or level settings.

The level is decreased by 30 dB during "Sample & Hold" measurement.

##### 4.4.5.1 Pulse Modulation Settings

Modulation

Amplitude Modulation...

Frequency Modulation...

Phase Modulation...

Pulse Modulation...

To access the "Pulse Modulation" settings, select "Modulation > config... > Pulse Modulation".

| Pulse Modulation | Pulse Modulation | Pulse Modulation |
| --- | --- | --- |

| Pulse Modulation | X |
| --- | --- |
| State | On |
| Source | External |
| Polarity | Normal |
| Connector/Trigger Settings | Connector/Trigger Settings |
| External Impedance | 50  $ \Omega $ |

The dialog contains all parameters for configuring a pulse modulation signal, comprising the signal source, pulse generator and trigger settings.

"Source Pulse Generator"

Depending on the selected modulation source, the provided parameters vary:

Displays the parameters for configuring the pulse generator signal, which in turn vary according to the selected "Mode > Single / Double ...".

Note: Extended features as the generation of double pulse signals or selectable trigger mode require option R&S SMB-K23.

##### ☑ "External"

Enables you to configure the polarity of an externally supplied pulse modulation signal.

Additionally, you can use the internally generated Valid Signal for synchronization of the pulse modulation, and assign this signal to the [VALID SIGNAL] connector, see Chapter 4.5.4.1, "Pulse Generator Settings", on page 231 for description.

Note: The pulse generator settings in this dialog are mirrored from the actual "Pulse Generator" dialog of the "Mod Gen" block. Therefore find the description on the access and the corresponding parameters under in Chapter 4.5.4.1, "Pulse Generator Settings", on page 231.

Option R&S SMB-K27 enables the generation of pulse trains. For description of the pulse train dialog, see Chapter 4.5.4.2, "Pulse Train Generation", on page 235.

##### State

Activates pulse modulation.

When the internal modulation source (pulse generator) is selected, the pulse generator is switched on automatically and the video/sync signal is output at the [PULSE VIDEO] output at the rear of the instrument. Signal output can be switched off in the "Pulse Generator" dialog (see Chapter 4.5.4, "Pulse Generator", on page 231).

Remote command:

[:SOURCE<hw>] :PULM:STATE on page 399

##### Source

Selects the modulation signal source for pulse modulation.

"Pulse Generator"

Uses the pulse generator as modulation signal source.

Uses the internally generated rectangular signal pulse modulation.

"External" Uses an externally applied modulation signal. The external modulation signal is input via the [PULSE EXT] connector.

Remote command:

[: SOURce<hw>] : PULM: SOURce on page 398

##### Polarity

(External Source only)

Selects the polarity of the modulation signal.

"Normal" The RF signal is On while the level is high at the modulation input.

"Inverse" The RF level is Off if the level is high at the modulation input.

Remote command:

[:SOURCE<hw>] :PULM:POLarity on page 398

#### 4.4.6 Stereo Modulation

Options R&S SMB-B5, Stereo/RDS Coder enables generation of stereo-modulated RF signals according to standard. Beside the MPX (FM stereo multiplex) signal, also the radio traffic service ARI (Automotive Radio Information) and Radio Data System (RDS) are supported by the option.

An internal or external source can be selected for the audio signal of the stereo modulation. In case of external source, the external signal is input via the analog [L] and [R] inputs or via the digital [S/P DIF] interface at the rear of the instrument. In case of internal source, the LF generator is used. Measurements can be performed in the operating modes L and R, and L = R, L = -R and R! = L (ext. signals only).

A typical setup with the R&S SMB with the option Stereo/RDS Coder in connection with the Audio Analyzer UPV is shown in the following graph.

R&S SMB100A Signal Generator
incl. Stereo/RDS Coder (R&S SMB-B5)
RF modulated test signal
including ARI and RDS
FM stereo tuner
Signal generation and analysis
Analog audio signals

</div>

</div>

##### 4.4.6.1 Stereo Modulation Dialog

To open the "Stereo Modulation" dialog, select "Modulation > Configure > Stereo Modulation" or use the [MENU] key under "Modulation".

Modulation
Amplitude Mod...
Frequency Mod...
Phase Mod...
Pulse Mod...
Stereo Mod...

</div>

</div>

In the upper section of the menu, the stereo modulation is configured and switched on. The configuration and activation of the additional pilot tone, ARI and RDS settings is performed in the lower section of the dialog.

State On

FM Deviation 40,000 kHz
Audio Source EdtLR
Mode Mono Left

Ext R.L Impedance 100 kOhm
Preemphasis 50 us
Max Modulation Freq 1,000 kHz
Stereo Pilot Tone Settings
Pilot State On
Pilot Deviation 6,750 kHz
Pilot Phase 0.0 deg
ARI Settings
ARI State Off
ARI Deviation 3,500 kHz
ARI Identification Off
ARI BK A
- RDS Settings
- RDS State On
- RDS Deviation 2,000 kHz
- RDS Data Set DS1
- Program Service Name
- Program Identification
- Traffic Program On
- Traffic Announcement On
- Stereo Coder
- Adjustment Data Factory

</div>

</div>

##### General Settings

##### State - Stereo Modulation

Activates/deactivates stereo modulation.

##### Remote command:

[:SOURCE]:STEREO:STATE on page 421

##### FM Deviation - Stereo Modulation

Sets the MPX (Multiplex stereo signal) deviation.

##### Remote command:

[:SOURCE]:STEReo[:DEViation] on page 421

##### Audio Source - Stereo Modulation

Selects the source for the audio signal.

"Off" No audio signal is provided, ARI and RDS signal can be generated separately.

"Ext L,R" The external audio stereo signal is feed in via the analog [L] and [R] inputs.

"Ext S/P Diff" The external audio signal is feed in via the digital [S/P DIF] input.

"LF Gen" The audio stereo signal is internally generated by the LF generator.

[ : SOURce ] : STEREo : SOURce on page 420

##### Mode - Stereo Modulation

Selects the mode for the audio signal. If the internal LF generator is selected as audio source, the signal is generated according to the selection here. For external signals, the signal type has to be entered.

"Mono Left" A mono signal containing the left channel is generated/fed in.

"Mono Right" A mono signal containing the right channel is generated/fed in.

"Stereo R=L" A stereo signal with right and left channel is generated/fed in. The channels have the same frequency and phase.

"Stereo R=-L" The signal on the left external audio input is used for both channels, left and right. The right channel is inverted.

"Stereo R!=L" (External source only)

A stereo signal containing different, independent right and left channels is fed in. It is possible, for example, to feed a fixed audio frequency to the first channel while a frequency sweep is being performed in the second channel.

##### Remote command:

[:SOURCE]:STEREO:AUDIO:MODE on page 415

##### LF Gen Freq - Stereo Modulation

(Audio source "LF Gen" only)

Sets the frequency of the LF generator signal.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

##### (two alias commands are available)

[:SOURCE]:STEREO:AUDIO[:FREQUENCY] on page 416

[ : SOURce ] : LFOutput<ch>: FREQuency on page 355

##### LF Gen Shape - Stereo Modulation

(Audio source "LF Gen" only)

Selects the shape of the LF generator signal.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

[:SOURCE]:LFOutput:SHAPe on page 363

##### External R/L Impedance - Stereo Modulation

(External analog audio signal input only)

Selects the input impedance for the external analog audio signal inputs [L] and [R].

##### Remote command:

[ : SOURce ] : STEREo : EXTernal : IMPedance on page 417

##### Preemphasis - Stereo Modulation

Activates and sets the pre-emphasis used for signal generation.

##### Remote command:

[:SOURCE]:STEReo:AUDIO:PREemphasis:STATE on page 416

[ :SOURCE] :STEREO:AUDIO:PREemphasis on page 415

##### Max Modulation Freq- Stereo Modulation

Sets the maximum modulation frequency that may be used.

This parameter is valid/required only when pre-emphasis has been activated and an external modulation source is used.

Pre-emphasis increases the high-frequency portions of the signal in the level before the FM modulator is reached. This can lead to internal overload of the modulator in the case of sinewave signals with full modulation. The MMF parameter is used to reduce the internal full modulation to such an extent that sinewave signals with nominal voltage can be transmitted with low distortion at the stereo input even when pre-emphasis up to the set frequency has been activated. However, this reduces the S/N ratio on the basis of the increase in level by the pre-emphasis (at the MMF that has been set).

In the case of normal modulation signals such as voice or music, this parameter can be left at its default value because the amplitude of the high-frequency portions of these signals normally decreases substantially.

##### Remote command:

[:SOURCE]:STEREO:MMF on page 417

##### Stereo Pilot Tone Settings

The 19 kHz pilot tone is configured in the "Stereo Pilot Tone Settings" section.

##### Pilot State - Stereo Modulation

Activates/deactivates the pilot tone generation.

##### Remote command:

[ :SOURCE]:STEREO:PILot:STATE on page 418

##### Pilot Deviation - Stereo Modulation

Sets the deviation of the pilot tone.

##### Remote command:

[:SOURCE]:STEReo:PILot[:DEViation] on page 418

##### Pilot Phase - Stereo Modulation

Sets the phase of the pilot tone in relation to the 38 kHz carrier signal of the receiver. For a correct demodulation, the pilot tone must be in phase with the 38 kHz carrier.

##### Remote command:

[ : SOURce ] : STEREo : PI Lot : PHASE on page 418

##### ARI Settings

The radio traffic service ARI (Automotive Radio Information) is configured in the "ARI Settings" section.

##### ARI State - Stereo Modulation

Activates/deactivates the ARI signal generation. ARI signals can be generated simultaneously with MPX and RDS signals.

##### Remote command:

[ : SOURCE ] : STEREO : ARI : STATE on page 413

##### ARI Deviation - Stereo Modulation

Sets the frequency deviation of the ARI subcarrier signal.

##### Remote command:

[:SOURCE]:STEReo:ARI[:DEViation] on page 414

##### ARI Identification - Stereo Modulation

Selects the generated identifiers of the ARI signal.

"Off" Only the 57 kHz subcarrier is generated (Senderkennung). It marks the stations which broadcast traffic programs and enables the receiver to recognize the frequency as being ARI-capable.

"DK" The message identification (Durchsagekennung) is generated in addition (low-frequency 30% AM). It signalizes that a traffic message is currently broadcasted.

"BK" The area identification (Bereichskennung) is generated in addition (60% AM). This code is used to identify the geographical region covered by the radio station. The specific code is selected below.

"DK+BK" The area and message identification are generated in addition.

##### Remote command:

[ : SOURCE ] : STEREO : ARI : TYPE on page 414

##### ARI BK - Stereo Modulation

Selects the specific area identification (BK) code of the ARI signal. The six letters (six different frequencies) identify a specific region in each country.

##### Remote command:

[:SOURCE]:STEREO:ARI:BK[:CODE] on page 413

##### RDS Settings

The RDS (Radio Data System) is configured in the RDS Settings section, RDS is a communications protocol standard from the European Broadcasting Union for sending digital information embedded in conventional FM radio broadcasts. The RDS system standardises several types of transmitted information, including time, track/artist info and station identification.

##### RDS State - Stereo Modulation

Activates/deactivates the RDS signal generation. RDS signals can be generated simultaneously with MPX and ARI signals.

Remote command:

[ : SOURCE ] : STEREO : RDS : STATE on page 419

##### RDS Deviation - Stereo Modulation

Sets the deviation of the RDS subcarrier.

Remote command:

[:SOURCE]:STEReo:RDS[:DEViation] on page 420

##### RDS Data Set - Stereo Modulation

Selects the data set used in the RDS signal. Five data sets are provided on the instrument. The values of the data sets can be defined via remote control (command SOURce: STEREO: DIRECT)

Each of these data sets contains predefined values for:

• PI (program identification, identifies the broadcast station)

• PS or scrolling PS (program service name, represents the station identity name)

• TP (traffic program, mark stations with regular traffic programs)

• TA (traffic announcement, marks the start of a traffic program)

• PTY (program type, predefined genres of broadcasting programs, e.g. news)

• PTYN (program type name)

• DI (decoder information)

• MS (music /speech)

• CT (clock time, used for synchronization)

The following values are empty:

- RT (radio text, two text blocks with 64 symbols each)

- AF (alternative frequencies, maximum of five lists with 25 frequencies each, enables the receiver to re-tune to a different frequency providing the same station when the first signal becomes too weak)

• TMC (traffic message channel)

- EON (enhanced other networks, eight PS with five EON AF lists each, enables the receiver to automatically tune into these stations if a traffic program is broadcasted)

The program identification and the program service name of the selected data set are indicated in the menu.

Remote command:

[:SOURCE]:STEREO:RDS:DATaset on page 418

##### RDS Program Service Name - Stereo Modulation

Indicates the RDS program service name.

##### Remote command:

[:SOURCE]:STEREO:DIRECT on page 417

##### RDS Program Identification - Stereo Modulation

Indicates the RDS program identification.

Remote command:

[:SOURCE]:STEREO:DIRECT on page 417

##### RDS Traffic Program State - Stereo Modulation

Activates the RDS traffic program (TP function). The receiver can recognize a frequency as being capable of traffic information only if the TP function is active.

Remote command:
[:SOURCE]:STEReo:RDS:TRAFFic:PROGRAM[:STATE] on page 419

##### RDS Traffic Announcement State - Stereo Modulation

Activates the RDS traffic announcement (TA function). If activated, the receiver switches from the current status, e.g. playing a CD, to the receive mode and enables the broadcast of a traffic announcement. The TP state has to be on.

Remote command:

[ :SOURCE]:STEReo:RDS:TRAFFic:ANNouncement[:STATE] on page 419

##### Adjustment Data

Indicates the adjustment state of the analog channels of the stereo coder. For the adjustment of the S/P DIF see service manual, chapter 2, "Adjustment".

See "Adjust Stereo Coder" on page 496.

### 4.5 Modulation Generator and LF Output

#### 4.5.1 Overview of LF Generator

The internal modulation generator of the instrument provides a sinusoidal or rectangular LF modulation signal without additional equipment options. The corresponding key data, as for example the frequency range, is specified under "Modulation sources" in the data sheet.

You can use the internal LF signal as modulation signal source for the analog modulations, as for example the amplitude modulation. The signal applies to all modulations which are using the internal modulation signal. Therefore, any modification of the LF signal impacts all currently active modulations immediately.

To configure the LF generator signal, see Chapter 4.5.2.1, "LF Output Dialog", on page 224. However, you can also configure the LF signal directly in the settings dialogs of the analog modulations.

- Pulse Generator (option R&S SMB-K23) for generating single and double pulse signals, see Chapter 4.5.4, "Pulse Generator", on page 231.

Optionally, the instrument provides the following modulation sources:

- High-performance pulse generator (option R&S SMB-K27) for generating pulse train signals.

The R&S SMB also provides the configured LF signal at the corresponding output connector, for example as modulation signal source for interconnected instruments.

#### 4.5.2 LF Output

—Modulation Generator—LF Generator / Output.

</div>

</div>

▶ To open the "LF Generator / Output" dialog, select "Mod Gen > Configure > LF Generator / Output" or use the [MENU] key under "Mod Gen".

| LF Generator / Output | LF Generator / Output |
| --- | --- |

The dialog provides access to the configuration of the internal modulation generators, and you can activate the output of the LF signal

The available settings depend on the source selected and on the installed options. Alternatively, you can perform the settings also in the corresponding dialogs of the analog modulations, like "Amplitude Modulation". The configured LF signal applies to all modulations which use the internal modulation sources, and to the LF output.

i

</div>

</div>

##### AM Exponential (Instruments with 12, 20, or 40 GHz frequency options)

If you perform exponential AM (AM Type Exponential) with the internal signal of the LF generator, the [LF] output provides the exponential modulation signal.

Using AM Source External, the instrument supplies the distorted external signal. The signal of the internal modulation generator is not available, as well as setting the parameters LF Gen Freq and "LF Gen Shape" on page 206.

The remote commands required to define these settings are described in Chapter 6.13.6, "SOURCE:LFOutput Subsystem", on page 354.

##### 4.5.2.1 LF Output Dialog

##### LF Output State

Activates the LF output. This setting has no effect on the modulations.

The modulation signal is output at the [LF output] connector of the instrument.

Remote command:

[:SOURce]:LFOutput[:STATE] on page 357

##### LF Output Voltage

Sets the voltage (peak) of the LF output signal.

Remote command:

[ : SOURce ] : LFOutput : VOLTage on page 364

##### LF Gen Freq

Sets the frequency of the LF generator.

This setting affects all analog modulations which use the LF generator as the internal modulation source.

##### Remote command:

[:SOURCE]:LFOutput<ch>:FREQUency on page 355

##### LF Gen Shape

Selects the waveform shape of the LF signal.

| LFGen Shape | Sine |
| --- | --- |
| Source Impedance | Sine Square Triangle Sawtooth Inv. Sawtooth |

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

Remote command:

[:SOURCE]:LFOutput:SHAPe on page 363

##### LF Source Impedance

Selects the output impedance of the LF generator. Selection LOW and 600 Ohm are available.

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

Remote command:

[:SOURce]:LFOutput:SIMPedance on page 363

#### 4.5.3 LF Frequency Sweep

— Modulation Generator —
LF Generator / Output...
LF Frequency Sweep...

</div>

</div>

The "LF Frequency Sweep" dialog is used to configure and activate an LF frequency sweep signal.

To open the "LF Frequency Sweep" dialog, select "Mod Gen > Configure > LF Frequency Sweep" or use the [MENU] key under "Mod Gen".

The LF sweep mode is activated and the sweep mode is selected. The buttons are used to reset the LF sweep (all sweep modes) or to execute the LF sweep ("Single" mode).

The sweep range, sweep spacing and dwell time are set in the bottom of the section.

| LF Frequency Sweep | LF Frequency Sweep |
| --- | --- |

##### State

Activates the LF frequency sweep signal generation.

##### Note:

Activating a sweep mode automatically deactivates other sweeps and the list mode.

Remote command:

[:SOURCE<hw>] :LFOutput:FREQUency:MODE on page 356

##### Mode

Selects the LF frequency sweep mode.

If you change the sweep mode during the execution, the signal generator stops the sweep and starts with the next trigger event at the initial value.

The "Reset Sweep" button sets the sweep to the start value.

"Auto"

Generates a continuously repeating sweep signal immediately after

activating the sweep mode.

The sweep steps are performed atomatically, controlled by the dwell time, see "Dwell Time - LF Sweep" on page 230.

##### Example:

SOUR:LFO:SWE:FREQ:MODE AUTO

TRIG0:SWE:SOUR AUTO

SOUR: LFO: FREQ: MODE SWE

#### "Single"

Generates a single sweep cycle after a trigger event. The sweep steps within the cycle are performed automatically, controlled by the dwell time. If one cycle is completed, the instrument waits for the next trigger event. To trigger the sweep, use "Execute Single Sweep" button, or the corresponding remote control commands, for example *TRG.

##### Example:

SOUR:LFO:SWE:FREQ:MODE AUTO

TRIG0:SWE:SOUR SING

SOUR:LFO:FREQ:MODE SWE

SOUR:LFO:SWE:FREQ:EXEC

#### "Step"

Generates the sweep signal step-by-step, manually triggered.

To perform the sweep steps, enter the frequency value under Current Freq.

##### Example:

SOUR: LFO: SWE: FREQ: MODE MAN

SOUR:LFO:FREQ:MODE SWE

SOUR: LFO: SWE: FREQ: SPAC LIN

SOUR: LFO: SWE: FREQ: STEP: LIN 1E34

SOUR:LFO:FREQ:MAN 12 kHz

The value entered with command

SOUR: LFO: SWE: FREQ: STEP: LIN | LOG sets the step width.

The value entered with command SOUR: LFO: FREQ: MAN has no effect, the command only sets the next sweep step. In remote control only a step-by-step sweep from start to stop frequency is possible.

#### "Extern Single"

Generates a single sweep cycle when an a external trigger event occurs.

The sweep steps within the cycle are performed automatically, controlled by the dwell time. If one cycle is completed, the instrument waits for the next trigger event.

To trigger the sweep, apply an external trigger signal.

Refer to the description of the rear panel for information on the connectors for external trigger signal input (see Chapter 3.2.2, "Rear Panel Tour", on page 54).

##### Example:

SOUR:LFO:SWE:FREQ:MODE AUTO

TRIG0:SWE:SOUR EXT

SOUR:LFO:FREQ:MODE SWE (External trigger)

"External Step" Generates the sweep signal step-by-step, manually triggered. To trigger a sweep step, apply an external trigger signal. The step width corresponds to the step width set for the rotary knob.

#### Example:

Example:

SOUR:LFO:SWE:FREQ:MODE AUTO

TRIG0:SWE:SOUR EXT

SOUR:LFO:FREQ:MODE SWE (External trigger)

"Extern Start/ Stop" Generates a continuously repeating sweep signal that is started, stopped and restarted by subsequent external trigger events. The sweep steps are performed automatically, controlled by the dwell time.

Refer to the description of the rear panel for information on the connectors for the external trigger signal input (see Chapter 3.2.2, "Rear Panel Tour", on page 54).

##### Example:

SOUR:LFO:SWE:FREQ:MODE AUTO

TRIG0:SWE:SOUR EAUT

SOUR:LFO:FREQ:MODE SWE (External trigger)

##### Remote command:

[:SOURce<hw>] :LFOutput :SWEep[:FREQUency] :MODE on page 358

:TRIGger<hw>[:SWEep]:SOURce on page 462

[:SOURce<hw>] :LFOutput :FREQUency:MODE on page 356

##### Execute Single Sweep

Starts a sweep manually. This trigger button is displayed in "Single" mode.

##### Example:

SOUR: LFO: SWE: FREQ: MODE AUTO

TRIG:LFFS:SWE:SOUR SING

TRIG: LFFS

##### Remote command:

[:SOURce<hw>] :LFOutput :SWEep[:FREQUency]:EXECute on page 358

:TRIGger<hw>:LFFSweep:IMMediate on page 460

:TRIGger<hw>:LFFSweep on page 459

:TRIGger<hw>[:IMMediate] on page 463

##### Reset Sweep

Resets a sweep.

With the next trigger event, the sweep starts with at the initial value.

Remote command:

[:SOURce<hw>] :SWEep:RESET[:ALL] on page 433

##### Start Freq

Sets the start frequency.

##### Remote command:

[:SOURce<hw>] :LFOutput:FREQUency:STARt on page 357

##### Stop Freq

Sets the stop frequency.

##### Remote command:

[ : SOURce<hw> ] : LFOutput : FREQuency : STOP on page 357

##### Current Freq

Displays the current frequency.

In sweep "Step" mode, the parameter is editable and you can enter frequency for the next step.

##### Remote command:

[:SOURce<hw>] :LFOutput:FREQUency:MANual on page 356

##### Spacing

Selects the mode for the calculation of the frequency sweep intervals.

"Linear" Takes the frequency value entered as an absolute value in Hz

"Logarithmic" Takes the value entered as a logarithmic value, that means as a constant fraction of teh current frequency in %.

##### Remote command:

[:SOURCE<hw>] :LFOutput :SWEep[:FREQUency] :SPACing on page 361

##### Shape

Selects the waveform shape of the sweep signal.

| --- | --- | --- |
| Feature | Frequency | Frequency |
| Feature | Frequency | Frequency |
| stop | 0 | 0 |
| stop | 0 | 0 |
| start | 0 | 0 |
| start | 0 | 0 |
| stop | 0 | 0 |
| stop | 0 | 0 |
| triangle shape | 0 | 0 |
| triangle shape | 0 | 0 |
| triangle shape | 1 | 1 |
| triangle shape | 1 | 1 |
| triangle shape | 2 | 2 |
| triangle shape | 2 | 2 |
| triangle shape | 3 | 3 |
| triangle shape | 3 | 3 |
| triangle shape | 4 | 4 |
| triangle shape | 4 | 4 |
| triangle shape | 5 | 5 |
| triangle shape | 5 | 5 |
| triangle shape | 6 | 6 |
| triangle shape | 6 | 6 |
| triangle shape | 7 | 7 |
| triangle shape | 7 | 7 |
| triangle shape | 8 | 8 |
| triangle shape | 8 | 8 |
| triangle shape | 9 | 9 |
| triangle shape | 9 | 9 |
| triangle shape | 10 | 10 |
| triangle shape | 10 | 10 |
| triangle shape | 11 | 11 |
| triangle shape | 11 | 11 |
| triangle shape | 12 | 12 |
| triangle shape | 12 | 12 |
| triangle shape | 13 | 13 |
| triangle shape | 13 | 13 |
| triangle shape | 14 | 14 |
| triangle shape | 14 | 14 |
| triangle shape | 15 | 15 |
| triangle shape | 15 | 15 |
| triangle shape | 16 | 16 |
| triangle shape | 16 | 16 |
| triangle shape | 17 | 17 |
| triangle shape | 17 | 17 |
| triangle shape | 18 | 18 |
| triangle shape | 18 | 18 |
| triangle shape | 19 | 19 |
| triangle shape | 19 | 19 |
| triangle shape | 20 | 20 |
| triangle shape | 20 | 20 |
| triangle shape | 21 | 21 |
| triangle shape | 21 | 21 |
| triangle shape | 22 | 22 |
| triangle shape | 22 | 22 |
| triangle shape | 23 | 23 |
| triangle shape | 23 | 23 |
| triangle shape | 24 | 24 |
| triangle shape | 24 | 24 |
| triangle shape | 25 | 25 |
| triangle shape | 25 | 25 |
| triangle shape | 26 | 26 |
| triangle shape | 26 | 26 |
| triangle shape | 27 | 27 |
| triangle shape | 27 | 27 |
| triangle shape | 28 | 28 |
| triangle shape | 28 | 28 |
| triangle shape | 29 | 29 |
| triangle shape | 29 | 29 |
| triangle shape | 30 | 30 |
| triangle shape | 30 | 30 |
| triangle shape | 31 | 31 |
| triangle shape | 31 | 31 |
| triangle shape | 32 | 32 |
| triangle shape | 32 | 32 |
| triangle shape | 33 | 33 |
| triangle shape | 33 | 33 |
| triangle shape | 34 | 34 |
| triangle shape | 34 | 34 |
| triangle shape | 35 | 35 |
| triangle shape | 35 | 35 |
| triangle shape | 36 | 36 |
| triangle shape | 36 | 36 |
| triangle shape | 37 | 37 |
| triangle shape | 37 | 37 |
| triangle shape | 38 | 38 |
| triangle shape | 38 | 38 |
| triangle shape | 39 | 39 |
| triangle shape | 39 | 39 |
| triangle shape | 40 | 40 |
| triangle shape | 40 | 40 |
| triangle shape | 41 | 41 |
| triangle shape | 41 | 41 |
| triangle shape | 42 | 42 |
| triangle shape | 42 | 42 |
| triangle shape | 43 | 43 |
| triangle shape | 43 | 43 |
| triangle shape | 44 | 44 |
| triangle shape | 44 | 44 |
| triangle shape | 45 | 45 |
| triangle shape | 45 | 45 |
| triangle shape | 46 | 46 |
| triangle shape | 46 | 46 |
| triangle shape | 47 | 47 |
| triangle shape | 47 | 47 |
| triangle shape | 48 | 48 |
| triangle shape | 48 | 48 |
| triangle shape | 49 | 49 |
| triangle shape | 49 | 49 |
| triangle shape | 50 | 50 |
| triangle shape | 50 | 50 |
| triangle shape | 51 | 51 |
| triangle shape | 51 | 51 |
| triangle shape | 52 | 52 |
| triangle shape | 52 | 52 |
| triangle shape | 53 | 53 |
| triangle shape | 53 | 53 |
| triangle shape | 54 | 54 |
| triangle shape | 54 | 54 |
| triangle shape | 55 | 55 |
| triangle shape | 55 | 55 |
| triangle shape | 56 | 56 |
| triangle shape | 56 | 56 |
| triangle shape | 57 | 57 |
| triangle shape | 57 | 57 |
| triangle shape | 58 | 58 |
| triangle shape | 58 | 58 |
| triangle shape | 59 | 59 |
| triangle shape | 59 | 59 |
| triangle shape | 60 | 60 |
| triangle shape | 60 | 60 |
| triangle shape | 61 | 61 |
| triangle shape | 61 | 61 |
| triangle shape | 62 | 62 |
| triangle shape | 62 | 62 |
| triangle shape | 63 | 63 |
| triangle shape | 63 | 63 |
| triangle shape | 64 | 64 |
| triangle shape | 64 | 64 |
| triangle shape | 65 | 65 |
| triangle shape | 65 | 65 |
| triangle shape | 66 | 66 |
| triangle shape | 66 | 66 |
| triangle shape | 67 | 67 |
| triangle shape | 67 | 67 |
| triangle shape | 68 | 68 |
| triangle shape | 68 | 68 |
| triangle shape | 69 | 69 |
| triangle shape | 69 | 69 |
| triangle shape | 70 | 70 |
| triangle shape | 70 | 70 |
| triangle shape | 71 | 71 |
| triangle shape | 71 | 71 |
| triangle shape | 72 | 72 |
| triangle shape | 72 | 72 |
| triangle shape | 73 | 73 |
| triangle shape | 73 | 73 |
| triangle shape | 74 | 74 |
| triangle shape | 74 | 74 |
| triangle shape | 75 | 75 |
| triangle shape | 75 | 75 |
| triangle shape | 76 | 76 |
| triangle shape | 76 | 76 |
| triangle shape | 77 | 77 |
| triangle shape | 77 | 77 |
| triangle shape | 78 | 78 |
| triangle shape | 78 | 78 |
| triangle shape | 79 | 79 |
| triangle shape | 79 | 79 |
| triangle shape | 80 | 80 |
| triangle shape | 80 | 80 |
| triangle shape | 81 | 81 |
| triangle shape | 81 | 81 |
| triangle shape | 82 | 82 |
| triangle shape | 82 | 82 |
| triangle shape | 83 | 83 |
| triangle shape | 83 | 83 |
| triangle shape | 84 | 84 |
| triangle shape | 84 | 84 |
| triangle shape | 85 | 85 |
| triangle shape | 85 | 85 |
| triangle shape | 86 | 86 |
| triangle shape | 86 | 86 |
| triangle shape | 87 | 87 |
| triangle shape | 87 | 87 |
| triangle shape | 88 | 88 |
| triangle shape | 88 | 88 |
| triangle shape | 89 | 89 |
| triangle shape | 89 | 89 |
| triangle shape | 90 | 90 |
| triangle shape | 90 | 90 |
| triangle shape | 91 | 91 |
| triangle shape | 91 | 91 |
| triangle shape | 92 | 92 |
| triangle shape | 92 | 92 |
| triangle shape | 93 | 93 |
| triangle shape | 93 | 93 |
| triangle shape | 94 | 94 |
| triangle shape | 94 | 94 |
| triangle shape | 95 | 95 |
| triangle shape | 95 | 95 |
| triangle shape | 96 | 96 |
| triangle shape | 96 | 96 |
| triangle shape | 97 | 97 |
| triangle shape | 97 | 97 |
| triangle shape | 98 | 98 |
| triangle shape | 98 | 98 |
| triangle shape | 99 | 99 |
| triangle shape | 99 | 99 |
| triangle shape | 100 | 100 |
| triangle shape | 100 | 100 |
| triangle shape | 101 | 101 |
| triangle shape | 101 | 101 |
| triangle shape | 102 | 102 |
| triangle shape | 102 | 102 |
| triangle shape | 103 | 103 |
| triangle shape | 103 | 103 |
| triangle shape | 104 | 104 |
| triangle shape | 104 | 104 |
| triangle shape | 105 | 105 |
| triangle shape | 105 | 105 |
| triangle shape | 106 | 106 |
| triangle shape | 106 | 106 |
| triangle shape | 107 | 107 |
| triangle shape | 107 | 107 |
| triangle shape | 108 | 108 |
| triangle shape | 108 | 108 |
| triangle shape | 109 | 109 |
| triangle shape | 109 | 109 |
| triangle shape | 110 | 110 |
| triangle shape | 110 | 110 |
| triangle shape | 111 | 111 |
| triangle shape | 111 | 111 |
| triangle shape | 112 | 112 |
| triangle shape | 112 | 112 |
| triangle shape | 113 | 113 |
| triangle shape | 113 | 113 |
| triangle shape | 114 | 114 |
| triangle shape | 114 | 114 |
| triangle shape | 115 | 115 |
| triangle shape | 115 | 115 |
| triangle shape | 116 | 116 |
| triangle shape | 116 | 116 |
| triangle shape | 117 | 117 |
| triangle shape | 117 | 117 |
| triangle shape | 118 | 118 |
| triangle shape | 118 | 118 |
| triangle shape | 119 | 119 |
| triangle shape | 119 | 119 |
| triangle shape | 120 | 120 |
| triangle shape | 120 | 120 |
| triangle shape | 121 | 121 |
| triangle shape | 121 | 121 |
| triangle shape | 122 | 122 |
| triangle shape | 122 | 122 |
| triangle shape | 123 | 123 |
| triangle shape | 123 | 123 |
| triangle shape | 124 | 124 |
| triangle shape | 124 | 124 |
| triangle shape | 125 | 125 |
| triangle shape | 125 | 125 |
| triangle shape | 126 | 126 |
| triangle shape | 126 | 126 |
| triangle shape | 127 | 127 |
| triangle shape | 127 | 127 |
| triangle shape | 128 | 128 |
| triangle shape | 128 | 128 |
| triangle shape | 129 | 129 |
| triangle shape | 129 | 129 |
| triangle shape | 130 | 130 |
| triangle shape | 130 | 130 |
| triangle shape | 131 | 131 |
| triangle shape | 131 | 131 |
| triangle shape | 132 | 132 |
| triangle shape | 132 | 132 |
| triangle shape | 133 | 133 |
| triangle shape | 133 | 133 |
| triangle shape | 134 | 134 |
| triangle shape | 134 | 134 |
| triangle shape | 135 | 135 |
| triangle shape | 135 | 135 |
| triangle shape | 136 | 136 |
| triangle shape | 136 | 136 |
| triangle shape | 137 | 137 |
| triangle shape | 137 | 137 |
| triangle shape | 138 | 138 |
| triangle shape | 138 | 138 |
| triangle shape | 139 | 139 |
| triangle shape | 139 | 139 |
| triangle shape | 140 | 140 |
| triangle shape | 140 | 140 |
| triangle shape | 141 | 141 |
| triangle shape | 141 | 141 |
| triangle shape | 142 | 142 |
| triangle shape | 142 | 142 |
| triangle shape | 143 | 143 |
| triangle shape | 143 | 143 |
| triangle shape | 144 | 144 |
| triangle shape | 144 | 144 |
| triangle shape | 145 | 145 |
| triangle shape | 145 | 145 |
| triangle shape | 146 | 146 |
| triangle shape | 146 | 146 |
| triangle shape | 147 | 147 |
| triangle shape | 147 | 147 |
| triangle shape | 148 | 148 |
| triangle shape | 148 | 148 |
| triangle shape | 149 | 149 |
| triangle shape | 149 | 149 |
| triangle shape | 150 | 150 |
| triangle shape | 150 | 150 |
| triangle shape | 151 | 151 |
| triangle shape | 151 | 151 |
| triangle shape | 152 | 152 |
| triangle shape | 152 | 152 |
| triangle shape | 153 | 153 |
| triangle shape | 153 | 153 |
| triangle shape | 154 | 154 |
| triangle shape | 154 | 154 |
| triangle shape | 155 | 155 |
| triangle shape | 155 | 155 |
| triangle shape | 156 | 156 |
| triangle shape | 156 | 156 |
| triangle shape | 157 | 157 |
| triangle shape | 157 | 157 |
| triangle shape | 158 | 158 |
| triangle shape | 158 | 158 |
| triangle shape | 159 | 159 |
| triangle shape | 159 | 159 |
| triangle shape | 160 | 160 |
| triangle shape | 160 | 160 |
| triangle shape | 161 | 161 |
| triangle shape | 161 | 161 |
| triangle shape | 162 | 162 |
| triangle shape | 162 | 162 |
| triangle shape | 163 | 163 |
| triangle shape | 163 | 163 |
| triangle shape | 164 | 164 |
| triangle shape | 164 | 164 |
| triangle shape | 165 | 165 |
| triangle shape | 165 | 165 |
| triangle shape | 166 | 166 |
| triangle shape | 166 | 166 |
| triangle shape | 167 | 167 |
| triangle shape | 167 | 167 |
| triangle shape | 168 | 168 |
| triangle shape | 168 | 168 |
| triangle shape | 169 | 169 |
| triangle shape | 169 | 169 |
| triangle shape | 170 | 170 |
| triangle shape | 170 | 170 |
| triangle shape | 171 | 171 |
| triangle shape | 171 | 171 |
| triangle shape | 172 | 172 |
| triangle shape | 172 | 172 |
| triangle shape | 173 | 173 |
| triangle shape | 173 | 173 |
| triangle shape | 174 | 174 |
| triangle shape | 174 | 174 |
| triangle shape | 175 | 175 |
| triangle shape | 175 | 175 |
| triangle shape | 176 | 176 |
| triangle shape | 176 | 176 |
| triangle shape | 177 | 177 |
| triangle shape | 177 | 177 |
| triangle shape | 178 | 178 |
| triangle shape | 178 | 178 |
| triangle shape | 179 | 179 |
| triangle shape | 179 | 179 |
| triangle shape | 180 | 180 |
| triangle shape | 180 | 180 |
| triangle shape | 181 | 181 |
| triangle shape | 181 | 181 |
| triangle shape | 182 | 182 |
| triangle shape | 182 | 182 |
| triangle shape | 183 | 183 |
| triangle shape | 183 | 183 |
| triangle shape | 184 | 184 |
| triangle shape | 184 | 184 |
| triangle shape | 185 | 185 |
| triangle shape | 185 | 185 |
| triangle shape | 186 | 186 |
| triangle shape | 186 | 186 |
| triangle shape | 187 | 187 |
| triangle shape | 187 | 187 |
| triangle shape | 188 | 188 |
| triangle shape | 188 | 188 |
| triangle shape | 189 | 189 |
| triangle shape | 189 | 189 |
| triangle shape | 190 | 190 |
| triangle shape | 190 | 190 |
| triangle shape | 191 | 191 |
| triangle shape | 191 | 191 |
| triangle shape | 192 | 192 |
| triangle shape | 192 | 192 |
| triangle shape | 193 | 193 |
| triangle shape | 193 | 193 |
| triangle shape | 194 | 194 |
| triangle shape | 194 | 194 |
| triangle shape | 195 | 195 |
| triangle shape | 195 | 195 |
| triangle shape | 196 | 196 |
| triangle shape | 196 | 196 |
| triangle shape | 197 | 197 |
| triangle shape | 197 | 197 |
| triangle shape | 198 | 198 |
| triangle shape | 198 | 198 |
| triangle shape | 199 | 199 |
| triangle shape | 199 | 199 |
| triangle shape | 200 | 200 |
| triangle shape | 200 | 200 |
| triangle shape | 201 | 201 |
| triangle shape | 201 | 201 |
| triangle shape | 202 | 202 |
| triangle shape | 202 | 202 |
| triangle shape | 203 | 203 |
| triangle shape | 203 | 203 |
| triangle shape | 204 | 204 |
| triangle shape | 204 | 204 |
| triangle shape | 205 | 205 |
| triangle shape | 205 | 205 |
| triangle shape | 206 | 206 |
| triangle shape | 206 | 206 |
| triangle shape | 207 | 207 |
| triangle shape | 207 | 207 |
| triangle shape | 208 | 208 |
| triangle shape | 208 | 208 |
| triangle shape | 209 | 209 |
| triangle shape | 209 | 209 |
| triangle shape | 210 | 210 |
| triangle shape | 210 | 210 |
| triangle shape | 211 | 211 |
| triangle shape | 211 | 211 |
| triangle shape | 212 | 212 |
| triangle shape | 212 | 212 |
| triangle shape | 213 | 213 |
| triangle shape | 213 | 213 |
| triangle shape | 214 | 214 |
| triangle shape | 214 | 214 |
| triangle shape | 215 | 215 |
| triangle shape | 215 | 215 |
| triangle shape | 216 | 216 |
| triangle shape | 216 | 216 |
| triangle shape | 217 | 217 |
| triangle shape | 217 | 217 |
| triangle shape | 218 | 218 |
| triangle shape | 218 | 218 |
| triangle shape | 219 | 219 |
| triangle shape | 219 | 219 |
| triangle shape | 220 | 220 |
| triangle shape | 220 | 220 |
| triangle shape | 221 | 221 |
| triangle shape | 221 | 221 |
| triangle shape | 222 | 222 |
| triangle shape | 222 | 222 |
| triangle shape | 223 | 223 |
| triangle shape | 223 | 223 |
| triangle shape | 224 | 224 |
| triangle shape | 224 | 224 |
| triangle shape | 225 | 225 |
| triangle shape | 225 | 225 |
| triangle shape | 226 | 226 |
| triangle shape | 226 | 226 |
| triangle shape | 227 | 227 |
| triangle shape | 227 | 227 |
| triangle shape | 228 | 228 |
| triangle shape | 228 | 228 |
| triangle shape | 229 | 229 |
| triangle shape | 229 | 229 |
| triangle shape | 230 | 230 |
| triangle shape | 230 | 230 |
| triangle shape | 231 | 231 |
| triangle shape | 231 | 231 |
| triangle shape | 232 | 232 |
| triangle shape | 232 | 232 |
| triangle shape | 233 | 233 |
| triangle shape | 233 | 233 |
| triangle shape | 234 | 234 |
| triangle shape | 234 | 234 |
| triangle shape | 235 | 235 |
| triangle shape | 235 | 235 |
| triangle shape | 236 | 236 |
| triangle shape | 236 | 236 |
| triangle shape | 237 | 237 |
| triangle shape | 237 | 237 |
| triangle shape | 238 | 238 |
| triangle shape | 238 | 238 |
| triangle shape | 239 | 239 |
| triangle shape | 239 | 239 |
| triangle shape | 240 | 240 |
| triangle shape | 240 | 240 |
| triangle shape | 241 | 241 |
| triangle shape | 241 | 241 |
| triangle shape | 242 | 242 |
| triangle shape | 242 | 242 |
| triangle shape | 243 | 243 |
| triangle shape | 243 | 243 |
| triangle shape | 244 | 244 |
| triangle shape | 244 | 244 |
| triangle shape | 245 | 245 |
| triangle shape | 245 | 245 |
| triangle shape | 246 | 246 |
| triangle shape | 246 | 246 |
| triangle shape | 247 | 247 |
| triangle shape | 247 | 247 |
| triangle shape | 248 | 248 |
| triangle shape | 248 | 248 |
| triangle shape | 249 | 249 |
| triangle shape | 249 | 249 |
| triangle shape | 250 | 250 |
| triangle shape | 250 | 250 |
| triangle shape | 251 | 251 |
| triangle shape | 251 | 251 |
| triangle shape | 252 | 252 |
| triangle shape | 252 | 252 |
| triangle shape | 253 | 253 |
| triangle shape | 253 | 253 |
| triangle shape | 254 | 254 |
| triangle shape | 254 | 254 |
| triangle shape | 255 | 255 |
| triangle shape | 255 | 255 |
| triangle shape | 256 | 256 |
| triangle shape | 256 | 256 |
| triangle shape | 257 | 257 |
| triangle shape | 257 | 257 |
| triangle shape | 258 | 258 |
| triangle shape | 258 | 258 |
| triangle shape | 259 | 259 |
| triangle shape | 259 | 259 |
| triangle shape | 260 | 260 |
| triangle shape | 260 | 260 |
| triangle shape | 261 | 261 |
| triangle shape | 261 | 261 |
| triangle shape | 262 | 262 |
| triangle shape | 262 | 262 |
| triangle shape | 263 | 263 |
| triangle shape | 263 | 263 |
| triangle shape | 264 | 264 |
| triangle shape | 264 | 264 |
| triangle shape | 265 | 265 |
| triangle shape | 265 | 265 |
| triangle shape | 266 | 266 |
| triangle shape | 266 | 266 |
| triangle shape | 267 | 267 |
| triangle shape | 267 | 267 |
| triangle shape | 268 | 268 |
| triangle shape | 268 | 268 |
| triangle shape | 269 | 269 |
| triangle shape | 269 | 269 |
| triangle shape | 270 | 270 |
| triangle shape | 270 | 270 |
| triangle shape | 271 | 271 |
| triangle shape | 271 | 271 |
| triangle shape | 272 | 272 |
| triangle shape | 272 | 272 |
| triangle shape | 273 | 273 |
| triangle shape | 273 | 273 |
| triangle shape | 274 | 274 |
| triangle shape | 274 | 274 |
| triangle shape | 275 | 275 |
| triangle shape | 275 | 275 |
| triangle shape | 276 | 276 |
| triangle shape | 276 | 276 |
| triangle shape | 277 | 277 |
| triangle shape | 277 | 277 |
| triangle shape | 278 | 278 |
| triangle shape | 278 | 278 |
| triangle shape | 279 | 279 |
| triangle shape | 279 | 279 |
| triangle shape | 280 | 280 |
| triangle shape | 280 | 280 |
| triangle shape | 281 | 281 |
| triangle shape | 281 | 281 |
| triangle shape | 282 | 282 |
| triangle shape | 282 | 282 |
| triangle shape | 283 | 283 |
| triangle shape | 283 | 283 |
| triangle shape | 284 | 284 |
| triangle shape | 284 | 284 |
| triangle shape | 285 | 285 |
| triangle shape | 285 | 285 |
| triangle shape | 286 | 286 |
| triangle shape | 286 | 286 |
| triangle shape | 287 | 287 |
| triangle shape | 287 | 287 |
| triangle shape | 288 | 288 |
| triangle shape | 288 | 288 |
| triangle shape | 289 | 289 |
| triangle shape | 289 | 289 |
| triangle shape | 290 | 290 |
| triangle shape | 290 | 290 |
| triangle shape | 291 | 291 |
| triangle shape | 291 | 291 |
| triangle shape | 292 | 292 |
| triangle shape | 292 | 292 |
| triangle shape | 293 | 293 |
| triangle shape | 293 | 293 |
| triangle shape | 294 | 294 |
| triangle shape | 294 | 294 |
| triangle shape | 295 | 295 |
| triangle shape | 295 | 295 |
| triangle shape | 296 | 296 |
| triangle shape | 296 | 296 |
| triangle shape | 297 | 297 |
| triangle shape | 297 | 297 |
| triangle shape | 298 | 298 |
| triangle shape | 298 | 298 |
| triangle shape | 299 | 299 |
| triangle shape | 299 | 299 |
| triangle shape | 300 | 300 |
| triangle shape | 300 | 300 |
| triangle shape | 301 | 301 |
| triangle shape | 301 | 301 |
| triangle shape | 302 | 302 |
| triangle shape | 302 | 302 |
| triangle shape | 303 | 303 |
| triangle shape | 303 | 303 |
| triangle shape | 304 | 304 |
| triangle shape | 304 | 304 |
| triangle shape | 305 | 305 |
| triangle shape | 305 | 305 |
| triangle shape | 306 | 306 |
| triangle shape | 306 | 306 |
| triangle shape | 307 | 307 |
| triangle shape | 307 | 307 |
| triangle shape | 308 | 308 |
| triangle shape | 308 | 308 |
| triangle shape | 309 | 309 |
| triangle shape | 309 | 309 |
| triangle shape | 310 | 310 |
| triangle shape | 310 | 310 |
| triangle shape | 311 | 311 |
| triangle shape | 311 | 311 |
| triangle shape | 312 | 312 |
| triangle shape | 312 | 312 |
| triangle shape | 313 | 313 |
| triangle shape | 313 | 313 |
| triangle shape | 314 | 314 |
| triangle shape | 314 | 314 |
| triangle shape | 315 | 315 |
| triangle shape | 315 | 315 |
| triangle shape | 316 | 316 |
| triangle shape | 316 | 316 |
| triangle shape | 317 | 317 |
| triangle shape | 317 | 317 |
| triangle shape | 318 | 318 |
| triangle shape | 318 | 318 |
| triangle shape | 319 | 319 |
| triangle shape | 319 | 319 |
| triangle shape | 320 | 320 |
| triangle shape | 320 | 320 |
| triangle shape | 321 | 321 |
| triangle shape | 321 | 321 |
| triangle shape | 322 | 322 |
| triangle shape | 322 | 322 |
| triangle shape | 323 | 323 |
| triangle shape | 323 | 323 |
| triangle shape | 324 | 324 |
| triangle shape | 324 | 324 |
| triangle shape | 325 | 325 |
| triangle shape | 325 | 325 |
| triangle shape | 326 | 326 |
| triangle shape | 326 | 326 |
| triangle shape | 327 | 327 |
| triangle shape | 327 | 327 |
| triangle shape | 328 | 328 |
| triangle shape | 328 | 328 |
| triangle shape | 329 | 329 |
| triangle shape | 329 | 329 |
| triangle shape | 330 | 330 |
| triangle shape | 330 | 330 |
| triangle shape | 331 | 331 |
| triangle shape | 331 | 331 |
| triangle shape | 332 | 332 |
| triangle shape | 332 | 332 |
| triangle shape | 333 | 333 |
| triangle shape | 333 | 333 |
| triangle shape | 334 | 334 |
| triangle shape | 334 | 334 |
| triangle shape | 335 | 335 |
| triangle shape | 335 | 335 |
| triangle shape | 336 | 336 |
| triangle shape | 336 | 336 |
| triangle shape | 337 | 337 |
| triangle shape | 337 | 337 |
| triangle shape | 338 | 338 |
| triangle shape | 338 | 338 |
| triangle shape | 339 | 339 |
| triangle shape | 339 | 339 |
| triangle shape | 340 | 340 |
| triangle shape | 340 | 340 |
| triangle shape | 341 | 341 |
| triangle shape | 341 | 341 |
| triangle shape | 342 | 342 |
| triangle shape | 342 | 342 |
| triangle shape | 343 | 343 |
| triangle shape | 343 | 343 |
| triangle shape | 344 | 344 |
| triangle shape | 344 | 344 |
| triangle shape | 345 | 345 |
| triangle shape | 345 | 345 |
| triangle shape | 346 | 346 |
| triangle shape | 346 | 346 |
| triangle shape | 347 | 347 |
| triangle shape | 347 | 347 |
| triangle shape | 348 | 348 |
| triangle shape | 348 | 348 |
| triangle shape | 349 | 349 |
| triangle shape | 349 | 349 |
| triangle shape | 350 | 350 |
| triangle shape | 350 | 350 |
| triangle shape | 351 | 351 |
| triangle shape | 351 | 351 |
| triangle shape | 352 | 352 |
| triangle shape | 352 | 352 |
| triangle shape | 353 | 353 |
| triangle shape | 353 | 353 |
| triangle shape | 354 | 354 |
| triangle shape | 354 | 354 |
| triangle shape | 355 | 355 |
| triangle shape | 355 | 355 |
| triangle shape | 356 | 356 |
| triangle shape | 356 | 356 |
| triangle shape | 357 | 357 |
| triangle shape | 357 | 357 |
| triangle shape | 358 | 358 |
| triangle shape | 358 | 358 |
| triangle shape | 359 | 359 |
| triangle shape | 359 | 359 |
| triangle shape | 360 | 360 |
| triangle shape | 360 | 360 |
| triangle shape | 361 | 361 |
| triangle shape | 361 | 361 |
| triangle shape | 362 | 362 |
| triangle shape | 362 | 362 |
| triangle shape | 363 | 363 |
| triangle shape | 363 | 363 |
| triangle shape | 364 | 364 |
| triangle shape | 364 | 364 |
| triangle shape | 365 | 365 |
| triangle shape | 365 | 365 |
| triangle shape | 366 | 366 |
| triangle shape | 366 | 366 |
| triangle shape | 367 | 367 |
| triangle shape | 367 | 367 |
| triangle shape | 368 | 368 |
| triangle shape | 368 | 368 |
| triangle shape | 369 | 369 |
| triangle shape | 369 | 369 |
| triangle shape | 370 | 370 |
| triangle shape | 370 | 370 |
| triangle shape | 371 | 371 |
| triangle shape | 371 | 371 |
| triangle shape | 372 | 372 |
| triangle shape | 372 | 372 |
| triangle shape | 373 | 373 |
| triangle shape | 373 | 373 |
| triangle shape | 374 | 374 |
| triangle shape | 374 | 374 |
| triangle shape | 375 | 375 |
| triangle shape | 375 | 375 |
| triangle shape | 376 | 376 |
| triangle shape | 376 | 376 |
| triangle shape | 377 | 377 |
| triangle shape | 377 | 377 |
| triangle shape | 378 | 378 |
| triangle shape | 378 | 378 |
| triangle shape | 379 | 379 |
| triangle shape | 379 | 379 |
| triangle shape | 380 | 380 |
| triangle shape | 380 | 380 |
| triangle shape | 381 | 381 |
| triangle shape | 381 | 381 |
| triangle shape | 382 | 382 |
| triangle shape | 382 | 382 |
| triangle shape | 383 | 383 |
| triangle shape | 383 | 383 |
| triangle shape | 384 | 384 |
| triangle shape | 384 | 384 |
| triangle shape | 385 | 385 |
| triangle shape | 385 | 385 |
| triangle shape | 386 | 386 |
| triangle shape | 386 | 386 |
| triangle shape | 387 | 387 |
| triangle shape | 387 | 387 |
| triangle shape | 388 | 388 |
| triangle shape | 388 | 388 |
| triangle shape | 389 | 389 |
| triangle shape | 389 | 389 |
| triangle shape | 390 | 390 |
| triangle shape | 390 | 390 |
| triangle shape | 391 | 391 |
| triangle shape | 391 | 391 |
| triangle shape | 392 | 392 |
| triangle shape | 392 | 392 |
| triangle shape | 393 | 393 |
| triangle shape | 393 | 393 |
| triangle shape | 394 | 394 |
| triangle shape | 394 | 394 |
| triangle shape | 395 | 395 |
| triangle shape | 395 | 395 |
| triangle shape | 396 | 396 |
| triangle shape | 396 | 396 |
| triangle shape | 397 | 397 |
| triangle shape | 397 | 397 |
| triangle shape | 398 | 398 |
| triangle shape | 398 | 398 |
| triangle shape | 399 | 399 |
| triangle shape | 399 | 399 |
| triangle shape | 400 | 400 |
| triangle shape | 400 | 400 |
| triangle shape | 401 | 401 |
| triangle shape | 401 | 401 |
| triangle shape | 402 | 402 |
| triangle shape | 402 | 402 |
| triangle shape | 403 | 403 |
| triangle shape | 403 | 403 |
| triangle shape | 404 | 404 |
| triangle shape | 404 | 404 |
| triangle shape | 405 | 405 |
| triangle shape | 405 | 405 |
| triangle shape | 406 | 406 |
| triangle shape | 406 | 406 |
| triangle shape | 407 | 407 |
| triangle shape | 407 | 407 |
| triangle shape | 408 | 408 |
| triangle shape | 408 | 408 |
| triangle shape | 409 | 409 |
| triangle shape | 409 | 409 |
| triangle shape | 410 | 410 |
| triangle shape | 410 | 410 |
| triangle shape | 411 | 411 |
| triangle shape | 411 | 411 |
| triangle shape | 412 | 412 |
| triangle shape | 412 | 412 |
| triangle shape | 413 | 413 |
| triangle shape | 413 | 413 |
| triangle shape | 414 | 414 |
| triangle shape | 414 | 414 |
| triangle shape | 415 | 415 |
| triangle shape | 415 | 415 |
| triangle shape | 416 | 416 |
| triangle shape | 416 | 416 |
| triangle shape | 417 | 417 |
| triangle shape | 417 | 417 |
| triangle shape | 418 | 418 |
| triangle shape | 418 | 418 |
| triangle shape | 419 | 419 |
| triangle shape | 419 | 419 |
| triangle shape | 420 | 420 |
| triangle shape | 420 | 420 |
| triangle shape | 421 | 421 |
| triangle shape | 421 | 421 |
| triangle shape | 422 | 422 |
| triangle shape | 422 | 422 |
| triangle shape | 423 | 423 |
| triangle shape | 423 | 423 |
| triangle shape | 424 | 424 |
| triangle shape | 424 | 424 |
| triangle shape | 425 | 425 |
| triangle shape | 425 | 425 |
| triangle shape | 426 | 426 |
| triangle shape | 426 | 426 |
| triangle shape | 427 | 427 |
| triangle shape | 427 | 427 |
| triangle shape | 428 | 428 |
| triangle shape | 428 | 428 |
| triangle shape | 429 | 429 |
| triangle shape | 429 | 429 |
| triangle shape | 430 | 430 |
| triangle shape | 430 | 430 |
| triangle shape | 431 | 431 |
| triangle shape | 431 | 431 |
| triangle shape | 432 | 432 |
| triangle shape | 432 | 432 |
| triangle shape | 433 | 433 |
| triangle shape | 433 | 433 |
| triangle shape | 434 | 434 |
| triangle shape | 434 | 434 |
| triangle shape | 435 | 435 |
| triangle shape | 435 | 435 |
| triangle shape | 436 | 436 |
| triangle shape | 436 | 436 |
| triangle shape | 437 | 437 |
| triangle shape | 437 | 437 |
| triangle shape | 438 | 438 |
| triangle shape | 438 | 438 |
| triangle shape | 439 | 439 |
| triangle shape | 439 | 439 |
| triangle shape | 440 | 440 |
| triangle shape | 440 | 440 |
| triangle shape | 441 | 441 |
| triangle shape | 441 | 441 |
| triangle shape | 442 | 442 |
| triangle shape | 442 | 442 |
| triangle shape | 443 | 443 |
| triangle shape | 443 | 443 |
| triangle shape | 444 | 444 |
| triangle shape | 444 | 444 |
| triangle shape | 445 | 445 |
| triangle shape | 445 | 445 |
| triangle shape | 446 | 446 |
| triangle shape | 446 | 446 |
| triangle shape | 447 | 447 |
| triangle shape | 447 | 447 |
| triangle shape | 448 | 448 |
| triangle shape | 448 | 448 |
| triangle shape | 449 | 449 |
| triangle shape | 449 | 449 |
| triangle shape | 450 | 450 |
| triangle shape | 450 | 450 |
| triangle shape | 451 | 451 |
| triangle shape | 451 | 451 |
| triangle shape | 452 | 452 |
| triangle shape | 452 | 452 |
| triangle shape | 453 | 453 |
| triangle shape | 453 | 453 |
| triangle shape | 454 | 454 |
| triangle shape | 454 | 454 |
| triangle shape | 455 | 455 |
| triangle shape | 455 | 455 |
| triangle shape | 456 | 456 |
| triangle shape | 456 | 456 |
| triangle shape | 457 | 457 |
| triangle shape | 457 | 457 |
| triangle shape | 458 | 458 |
| triangle shape | 458 | 458 |
| triangle shape | 459 | 459 |
| triangle shape | 459 | 459 |
| triangle shape | 460 | 460 |
| triangle shape | 460 | 460 |
| triangle shape | 461 | 461 |
| triangle shape | 461 | 461 |
| triangle shape | 462 | 462 |
| triangle shape | 462 | 462 |
| triangle shape | 463 | 463 |
| triangle shape | 463 | 463 |
| triangle shape | 464 | 464 |
| triangle shape | 464 | 464 |
| triangle shape | 465 | 465 |
| triangle shape | 465 | 465 |
| triangle shape | 466 | 466 |
| triangle shape | 466 | 466 |
| triangle shape | 467 | 467 |
| triangle shape | 467 | 467 |
| triangle shape | 468 | 468 |
| triangle shape | 468 | 468 |
| triangle shape | 469 | 469 |
| triangle shape | 469 | 469 |
| triangle shape | 470 | 470 |
| triangle shape | 470 | 470 |
| triangle shape | 471 | 471 |
| triangle shape | 471 | 471 |
| triangle shape | 472 | 472 |
| triangle shape | 472 | 472 |
| triangle shape | 473 | 473 |
| triangle shape | 473 | 473 |
| triangle shape | 474 | 474 |
| triangle shape | 474 | 474 |
| triangle shape | 475 | 475 |
| triangle shape | 475 | 475 |
| triangle shape | 476 | 476 |
| triangle shape | 476 | 476 |
| triangle shape | 477 | 477 |
| triangle shape | 477 | 477 |
| triangle shape | 478 | 478 |
| triangle shape | 478 | 478 |
| triangle shape | 479 | 479 |
| triangle shape | 479 | 479 |
| triangle shape | 480 | 480 |
| triangle shape | 480 | 480 |
| triangle shape | 481 | 481 |
| triangle shape | 481 | 481 |
| triangle shape | 482 | 482 |
| triangle shape | 482 | 482 |
| triangle shape | 483 | 483 |
| triangle shape | 483 | 483 |
| triangle shape | 484 | 484 |
| triangle shape | 484 | 484 |
| triangle shape | 485 | 485 |
| triangle shape | 485 | 485 |
| triangle shape | 486 | 486 |
| triangle shape | 486 | 486 |
| triangle shape | 487 | 487 |
| triangle shape | 487 | 487 |
| triangle shape | 488 | 488 |
| triangle shape | 488 | 488 |
| triangle shape | 489 | 489 |
| triangle shape | 489 | 489 |
| triangle shape | 490 | 490 |
| triangle shape | 490 | 490 |
| triangle shape | 491 | 491 |
| triangle shape | 491 | 491 |
| triangle shape | 492 | 492 |
| triangle shape | 492 | 492 |
| triangle shape | 493 | 493 |
| triangle shape | 493 | 493 |
| triangle shape | 494 | 494 |
| triangle shape | 494 | 494 |
| triangle shape | 495 | 495 |
| triangle shape | 495 | 495 |
| triangle shape | 496 | 496 |
| triangle shape | 496 | 496 |
| triangle shape | 497 | 497 |
| triangle shape | 497 | 497 |
| triangle shape | 498 | 498 |
| triangle shape | 498 | 498 |
| triangle shape | 499 | 499 |
| triangle shape | 499 | 499 |
| triangle shape | 500 | 500 |
| triangle shape | 500 | 500 |
| triangle shape | 501 | 501 |
| triangle shape | 501 | 501 |
| triangle shape | 502 | 502 |
| triangle shape | 502 | 502 |
| triangle shape | 503 | 503 |
| triangle shape | 503 | 503 |
| triangle shape | 504 | 504 |
| triangle shape | 504 | 504 |
| triangle shape | 505 | 505 |
| triangle shape | 505 | 505 |
| triangle shape | 506 | 506 |
| triangle shape | 506 | 506 |
| triangle shape | 507 | 507 |
| triangle shape | 507 | 507 |
| triangle shape | 508 | 508 |
| triangle shape | 508 | 508 |
| triangle shape | 509 | 509 |
| triangle shape | 509 | 509 |
| triangle shape | 510 | 510 |
| triangle shape | 510 | 510 |
| triangle shape | 511 | 511 |
| triangle shape | 511 | 511 |
| triangle shape | 512 | 512 |
| triangle shape | 512 | 512 |
| triangle shape | 513 | 513 |
| triangle shape | 513 | 513 |
| triangle shape | 514 | 514 |
| triangle shape | 514 | 514 |
| triangle shape | 515 | 515 |
| triangle shape | 515 | 515 |
| triangle shape | 516 | 516 |
| triangle shape | 516 | 516 |
| triangle shape | 517 | 517 |
| triangle shape | 517 | 517 |
| triangle shape | 518 | 518 |
| triangle shape | 518 | 518 |
| triangle shape | 519 | 519 |
| triangle shape | 519 | 519 |
| triangle shape | 520 | 520 |
| triangle shape | 520 | 520 |
| triangle shape | 521 | 521 |
| triangle shape | 521 | 521 |
| triangle shape | 522 | 522 |
| triangle shape | 522 | 522 |
| triangle shape |  |  |
| triangle shape |  |  |

</td></tr>
  </tbody>
</table>

"Sawtooth"

The sweep runs from the start to the stop frequency. Each subsequent sweep starts at the start frequency, that means the shape of the sweep sequence resembles a sawtooth.

"Triangle"

The sweep runs from start to stop frequency and back, that means the shape of the sweep resembles a triangle. A subsequent sweep starts at the start frequency.

##### Remote command:

[:SOURCE<hw>] :LFOutput :SWEep[:FREQUency] :SHAPe on page 361

##### Retrace - LF Frequency Sweep

Activates that the signal changes to the start frequency value while it is waiting for the next trigger event.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single", see Mode.

| --- | --- | --- | --- |
| Stop Frequency | 0 | 0 | 0 |
| Stop Frequency | 0 | 0 | 0 |

</td></tr>
  </tbody>
</table>

##### Remote command:

[:SOURCE<hw>] :LFOutput :SWEep[:FREQUency] :RETRace on page 360

##### Step Lin/Log - LF Sweep

Sets the step width for the individual frequency sweep steps.

At each step this value is added to the current frequency.

Depending on the Spacing mode you have set, the corresponding parameter is displayed.

"Step Lin" The step width is a constant value in Hz.

##### Remote command:

[:SOURCE<hw>] :LFOutput :SWEep[:FREQUency] :STEP[:LINEAR] on page 361

"Step Log" The step width is determined logarithmically in %, that means as a constant fraction of the current frequency.

##### Remote command:

[:SOURce<hw>] :LFOutput :SWEep[:FREQUency]:STEP:LOGarithmic on page 362

##### Dwell Time - LF Sweep

Defines the duration of the individual sweep steps.

| --- | --- | --- |
| Time | Signal Level | Signal Level |
| Time | Signal Level | Signal Level |
| 0 | 0 | 0 |
| 0 | 0 | 0 |
| 1 | 0 | 0 |
| 1 | 0 | 0 |
| 2 | 0 | 0 |
| 2 | 0 | 0 |
| 3 | 0 | 0 |
| 3 | 0 | 0 |
| 4 | 0 | 0 |
| 4 | 0 | 0 |
| 5 | 0 | 0 |
| 5 | 0 | 0 |
| 6 | 0 | 0 |
| 6 | 0 | 0 |
| 7 | 0 | 0 |
| 7 | 0 | 0 |
| 8 | 0 | 0 |
| 8 | 0 | 0 |
| 9 | 0 | 0 |
| 9 | 0 | 0 |
| 10 | 0 | 0 |
| 10 | 0 | 0 |
| 11 | 0 | 0 |
| 11 | 0 | 0 |
| 12 | 0 | 0 |
| 12 | 0 | 0 |
| 13 | 0 | 0 |
| 13 | 0 | 0 |
| 14 | 0 | 0 |
| 14 | 0 | 0 |
| 15 | 0 | 0 |
| 15 | 0 | 0 |
| 16 | 0 | 0 |
| 16 | 0 | 0 |
| 17 | 0 | 0 |
| 17 | 0 | 0 |
| 18 | 0 | 0 |
| 18 | 0 | 0 |
| 19 | 0 | 0 |
| 19 | 0 | 0 |
| 20 | 0 | 0 |
| 20 | 0 | 0 |
| 21 | 0 | 0 |
| 21 | 0 | 0 |
| 22 | 0 | 0 |
| 22 | 0 | 0 |
| 23 | 0 | 0 |
| 23 | 0 | 0 |
| 24 | 0 | 0 |
| 24 | 0 | 0 |
| 25 | 0 | 0 |
| 25 | 0 | 0 |
| 26 | 0 | 0 |
| 26 | 0 | 0 |
| 27 | 0 | 0 |
| 27 | 0 | 0 |
| 28 | 0 | 0 |
| 28 | 0 | 0 |
| 29 | 0 | 0 |
| 29 | 0 | 0 |
| 30 | 0 | 0 |
| 30 | 0 | 0 |
| 31 | 0 | 0 |
| 31 | 0 | 0 |
| 32 | 0 | 0 |
| 32 | 0 | 0 |
| 33 | 0 | 0 |
| 33 | 0 | 0 |
| 34 | 0 | 0 |
| 34 | 0 | 0 |
| 35 | 0 | 0 |
| 35 | 0 | 0 |
| 36 | 0 | 0 |
| 36 | 0 | 0 |
| 37 | 0 | 0 |
| 37 | 0 | 0 |
| 38 | 0 | 0 |
| 38 | 0 | 0 |
| 39 | 0 | 0 |
| 39 | 0 | 0 |
| 40 | 0 | 0 |
| 40 | 0 | 0 |
| 41 | 0 | 0 |
| 41 | 0 | 0 |
| 42 | 0 | 0 |
| 42 | 0 | 0 |
| 43 | 0 | 0 |
| 43 | 0 | 0 |
| 44 | 0 | 0 |
| 44 | 0 | 0 |
| 45 | 0 | 0 |
| 45 | 0 | 0 |
| 46 | 0 | 0 |
| 46 | 0 | 0 |
| 47 | 0 | 0 |
| 47 | 0 | 0 |
| 48 | 0 | 0 |
| 48 | 0 | 0 |
| 49 | 0 | 0 |
| 49 | 0 | 0 |
| 50 | 0 | 0 |
| 50 | 0 | 0 |
| 51 | 0 | 0 |
| 51 | 0 | 0 |
| 52 | 0 | 0 |
| 52 | 0 | 0 |
| 53 | 0 | 0 |
| 53 | 0 | 0 |
| 54 | 0 | 0 |
| 54 | 0 | 0 |
| 55 | 0 | 0 |
| 55 | 0 | 0 |
| 56 | 0 | 0 |
| 56 | 0 | 0 |
| 57 | 0 | 0 |
| 57 | 0 | 0 |
| 58 | 0 | 0 |
| 58 | 0 | 0 |
| 59 | 0 | 0 |
| 59 | 0 | 0 |
| 60 | 0 | 0 |
| 60 | 0 | 0 |
| 61 | 0 | 0 |
| 61 | 0 | 0 |
| 62 | 0 | 0 |
| 62 | 0 | 0 |
| 63 | 0 | 0 |
| 63 | 0 | 0 |
| 64 | 0 | 0 |
| 64 | 0 | 0 |
| 65 | 0 | 0 |
| 65 | 0 | 0 |
| 66 | 0 | 0 |
| 66 | 0 | 0 |
| 67 | 0 | 0 |
| 67 | 0 | 0 |
| 68 | 0 | 0 |
| 68 | 0 | 0 |
| 69 | 0 | 0 |
| 69 | 0 | 0 |
| 70 | 0 | 0 |
| 70 | 0 | 0 |
| 71 | 0 | 0 |
| 71 | 0 | 0 |
| 72 | 0 | 0 |
| 72 | 0 | 0 |
| 73 | 0 | 0 |
| 73 | 0 | 0 |
| 74 | 0 | 0 |
| 74 | 0 | 0 |
| 75 | 0 | 0 |
| 75 | 0 | 0 |
| 76 | 0 | 0 |
| 76 | 0 | 0 |
| 77 | 0 | 0 |
| 77 | 0 | 0 |
| 78 | 0 | 0 |
| 78 | 0 | 0 |
| 79 | 0 | 0 |
| 79 | 0 | 0 |
| 80 | 0 | 0 |
| 80 | 0 | 0 |
| 81 | 0 | 0 |
| 81 | 0 | 0 |
| 82 | 0 | 0 |
| 82 | 0 | 0 |
| 83 | 0 | 0 |
| 83 | 0 | 0 |
| 84 | 0 | 0 |
| 84 | 0 | 0 |
| 85 | 0 | 0 |
| 85 | 0 | 0 |
| 86 | 0 | 0 |
| 86 | 0 | 0 |
| 87 | 0 | 0 |
| 87 | 0 | 0 |
| 88 | 0 | 0 |
| 88 | 0 | 0 |
| 89 | 0 | 0 |
| 89 | 0 | 0 |
| 90 | 0 | 0 |
| 90 | 0 | 0 |
| 91 | 0 | 0 |
| 91 | 0 | 0 |
| 92 | 0 | 0 |
| 92 | 0 | 0 |
| 93 | 0 | 0 |
| 93 | 0 | 0 |
| 94 | 0 | 0 |
| 94 | 0 | 0 |
| 95 | 0 | 0 |
| 95 | 0 | 0 |
| 96 | 0 | 0 |
| 96 | 0 | 0 |
| 97 | 0 | 0 |
| 97 | 0 | 0 |
| 98 | 0 | 0 |
| 98 | 0 | 0 |
| 99 | 0 | 0 |
| 99 | 0 | 0 |
| 100 | 0 | 0 |
| 100 | 0 | 0 |

</td></tr>
  </tbody>
</table>

The "Dwell Time" set by the user is used as the step time of the sweep. The effective net dwell time is shorter, reduced by the setting time. This setting time may be greater than the time specified in the data sheet.

##### Note:

It is recommended to switch off the display update for optimum sweep performance especially with short dwell times (see Chapter 4.2.3.6, "Display Update", on page 103).

Remote command:

[:SOURCE<hw>] :LFOutput :SWEep[:FREQUency] :DWELL on page 358

##### Ext. Trigger Input Slope

Sets the polarity of the active slope of an externally applied instrument trigger.

This setting affects the INST TRIG input (BNC connector at the rear of the instrument).

"Positive" activates the rising edge of the trigger signal.

"Negative" activates the falling edge of the trigger signal.

Remote command:

[:SOURCE]:INPUT:TRIGGER:SLOPe on page 354

#### 4.5.4 Pulse Generator

The "Pulse Generator" dialog is used to configure and activate a pulse modulation signal.

##### 4.5.4.1 Pulse Generator Settings

—Modulation Generator—
LF Generator / Output...
LF Frequency Sweep...
Pulse Generator...

</div>

</div>

To access the pulse generator settings ...

▶ Select "Mod Gen > config... > Pulse Generator" or use the [MENU] key under "Mod Gen".

Alternatively, the R&S SMB provides the pulse generator parameters in the "Pulse Modulation" dialog accessed via the "Modulation" block.

| Pulse Generator | X |
| --- | --- |
| Video/Sync SignalState | On |
| Pulse Generator | Pulse Generator |
| Pulse Mode | Single |
| Pulse Period | 10.00  $ \mu s $ |
| Pulse Width | 2.00  $ \mu s $ |
| Pulse Delay | 0.01  $ \mu s $ |
| Connector/Trigger Settings | Connector/Trigger Settings |
| Trigger Mode | External Single |
| Ext Trigger Input Slope | Positive |
| External Impedance | 50  $ \Omega $ |
| Use SIGNAL VALID as Pulse Sync | On |

The dialog provides the settings for the pulse characteristics and trigger mode. Depending on the selected modulation source and pulse mode the provided parameters vary.

Note: Extended features as the generation of double pulse signals with selectable pulse widths and periods, or selectable trigger mode require option R&S SMB-K23.

##### Video Sync Signal State - Pulse Generator

Switches on/off the output of the video/sync signal at the [PULSE VIDEO] connector. The signal output and the pulse generator are automatically switched on with activation of pulse modulation if pulse generator is selected as modulation source. The signal output can be switched off subsequently.

Pulse modulation of the RF carrier is activated in the "Pulse modulation" menu of the "Modulation" block.

Remote command:

[:SOURce<hw>] :PGENERator:STATE on page 377

##### Pulse Mode - Pulse Generator

Sets the mode of the pulse generator.

A single pulse is generated in one pulse period.

"Double" Two pulses are generated in one pulse period. Additional settings for the double pulse are available in the menu.

Train" Requires option R&S SMB-K27.

A user-defined pulse train is generated. Additional settings for the pulse train are available in the menu after selection of the pulse train mode (see Chapter 4.5.4.2, "Pulse Train Generation", on page 235). A pulse train is a sequence of pulses with user-defined on and off times. The on-time/off-time value pairs are defined in a pulse train list. The currently used pulse train file is displayed in the sub menu.

##### Remote command:

[ : SOURce<hw> ] : PULM:MODE on page 397

##### Pulse Period - Pulse Generator

Sets the period of the generated pulse. The period determines the repetition frequency of the internal signal.

##### Remote command:

[:SOURCE<hw>] : PULM: PERiod on page 397

##### Pulse Width - Pulse Generator

Sets the width of the generated pulse. The width determines the pulse length. The pulse width must be at least 20 ns less than the set pulse period.

##### Remote command:

[:SOURCE<hw>] : PULM:WIDTH on page 409

##### Pulse Delay - Pulse Generator

(External trigger only)

Sets the pulse delay. The pulse delay determines the time that elapses after a trigger event before pulse modulation starts. The pulse delay is not effective for double pulse generation.

##### Remote command:

[ : SOURce<hw> ] : PULM: DELay on page 395

##### Double Pulse Width - Pulse Generator

(Double Pulse only)

Sets the width of the second pulse.

##### Remote command:

[:SOURce<hw>] :PULM:DOUBLE:WIDTH on page 396

##### Double Pulse Delay - Pulse Generator

(Double Pulse only)

Sets the delay from the start of the first pulse to the start of the second pulse.

##### Remote command:

[:SOURce<hw>] :PULM:DOUBLE:DELAY on page 396

##### Trigger Mode - Pulse Generator

Selects the trigger mode for pulse modulation.

Note: An external trigger signal is supplied via the [PULSE EXT] connector.

| Auto | The pulse generator signal is generated continuously. |
| --- | --- |
| "Single" | The pulse generator signal is triggered by an internal trigger event, initiated with the "Execute Single Trigger" on page 234. |
| "External Single" | The pulse modulation is triggered by an external trigger event. |
| "External Gated" | The pulse generator signal is gated by an external gate signal. |
| Remote command: | Remote command: |
| [ :SOURCE<hw>] :PULM:TRIGGER:MODE on page 404 | [ :SOURCE<hw>] :PULM:TRIGGER:MODE on page 404 |

##### Execute Single Trigger

Initiates a single pulse sequence manually.

This function is enabled in "Single Trigger", see Trigger Mode - Pulse Generator

Remote command:

[:SOURce]:PULM[:INTernal][:TRAin]:TRIGger:IMMediate on page 405

*TRG on page 287

##### External Trigger Input Slope - Pulse Generator

(External Trigger only)

Sets the polarity of the active slope of an applied trigger signal.

"Positive" The pulse generator is triggered on the positive slope of the external trigger signal.

"Negative" The pulse generator is triggered on the negative slope of the external trigger signal.

Remote command:

[: SOURce<hw>] : PULM: TRIGger: EXternal: SLOPe on page 404

##### Gate Input Polarity - Pulse Generator

(Trigger Mode External Gated only)

Selects the polarity of the Gate signal.

The signal is supplied via the [PULSE EXT] connector.

"Normal" The pulse signal is generated while the gate signal is high.

"Inverse" The pulse signal is generated while the gate signal is low.

Remote command:

[ : SOURce<hw>] : PULM: TRIGger: EXTernal: GATE: POLarity on page 40

##### External Impedance

Selects the input impedance (10 kOhm or 50 Ohm) for the external trigger and gate signal input ([PULSE EXT]).

##### Remote command:

[:SOURCE<hw>] :PULM:TRIGger:EXTERNAL:IMPedance on page 404

##### Use SIGNAL VALID as Pulse Sync

Configures the signal at the [SIGNAL VALID] connector (rear panel):

"selected" Indicates the validity of the RF signal at the output:

• high: while the signal settles.

• low: when it is stable (valid).

"cleared"

Generates a single pulse at the beginning of a pulse sequence, e.g. to synchronize pulse modulation.

Remote command:
[: SOURce<hw>] : PULM: OUTPUT: SYNC[: STATE] on page 397

##### 4.5.4.2 Pulse Train Generation

In "Pulse Train" mode, the instrument provides the associated parameters for configuring a user-defined pulse train signal.

A pulse train is a sequence of pulses with user-defined on and off times. The "ON Time / OFF Time" value pairs are defined in a pulse train table and can be stored in a file. The currently loaded file is displayed in the dialog. You can export an internally created pulse train list as well as import an externally created one.

i

</div>

</div>

In remote control mode, you must first create a data file, before you switch to pulse train mode. Otherwise you get the error message "No current list" in the "Info" line.

##### How to configure a pulse train signal

To perform pulse train generation, perform the following steps:

1. In the block diagram, select "Modulation > config... > Pulse Modulation".

2. Select "Source > Pulse Generator".

3. Select "Pulse Mode > Train".

| Pulse Modulation | X |
| --- | --- |
| State | On |
| Source | Pulse Generator |
| Pulse Generator | Pulse Generator |
| Pulse Mode | Train |
| Pulse Train Data... | adg |
| Edit Pulse Train Data... | Edit Pulse Train Data... |
| Import/Export >>> | Import/Export >>> |
| Connector/Trigger Settings | Connector/Trigger Settings |
| Trigger Mode | External Single |
| Ext Trigger Input Slope | Positive |
| External Impedance | 50  $ \Omega $ |
| Use SIGNAL VALID as Pulse Sync | On |

The instrument displays the parameters required for configuring pulse train data.

4. Select "Pulse Train Data... > New List / Select List or File Manager".

5. Navigate to the target directory and select an existing file, or create a new file by assigning the "File Name".

6. According to your selection, confirm with "Save" or "Select".

The R&S SMB automatically uses the new file for further editing. Pulse train data files have the fixed file extension *.pulstrn.

7. In the "Pulse Modulation" dialog, select "Edit Pulse Train Data... > Edit" to define the on and off time value pairs and the repetition factor for each value pair.

8. When completed, save the file.

9. Starting in the block diagram, perform the following steps to activate signal generation:

a) Select "Mod Gen > config... > Pulse Generator > Video/Sync Signal State > On".

b) Select "Modulation > config... > Pulse Modulation > State > On".

c) Activate RF signal generation in the "RF" block.

The R&S SMB generates an RF pulse sequence signal according to the values specified in the file.

##### Pulse Train Data - Pulse Generator

Opens the "File Select" dialog for selecting and creating a pulse train file, and provides access to the "File Manager".

##### Remote command:

[:SOURce<hw>] :PULM:TRAin:CATalog? on page 399

[:SOURCE<hw>] :PULM:TRAin:SELECT on page 403

[:SOURce<hw>] :PULM:TRAin:DELete on page 399

##### Edit Pulse Train Data - Pulse Generator

Opens the Pulse Train Dialog.

##### Pulse Train Dialog

Pulse Train Dialog - jgb Edit
Edit Pulse Train Data Edit
2.0 6.0 10.0 14.0 18.0 
Zoom Position 12.000 
Zoom In Zoom Out

</div>

</div>

Displays the pulse sequence as defined in the file.

"Edit"

Opens the pulse train dialog, see Edit Pulse Train Data. The dialog graphically represents the pulse train signal and provides access to the data editor.

"Zoom Position"

Sets the blue marker in the pulse train graph. The marker defines the center of any zoom in or zoom out action.

"Zoom In / Zoom Out"

Enlarges the diagram by factor 2 per "Zoom In", or scales it down accordingly when you select "Zoom Out".

##### Edit Pulse Train Data

Opens an editor allowing you to enter the "On-Time / OFF-Time" value pairs in a table. In addition, you can assign a repetition rate to each pair. Based on these values, the instrument then generates the pulse train signal. You can enter any number of value pairs and save your list in a file. The file name is displayed in the header of the dialog.

| Edit Pulse Train Data job | Edit Pulse Train Data job | Edit Pulse Train Data job | Edit Pulse Train Data job | Edit Pulse Train Data job |
| --- | --- | --- | --- | --- |
|  | ON-Time/ $ \mu $s | OFF-Time/ $ \mu $s | Count |  |
| 1 | 2.000 | 6.000 | 1 |  |
| 2 | 2.400 | 6.000 | 2 |  |
| 3 |  |  |  |  |
| Goto | Edit | Save |  |  |

"ON-Time/ $ \mu $s"  Determines the length of the respective pulse (signal is high).

##### Remote command:

[:SOURCE<hw>] : PULM: TRAIN: ON Time on page 401

"OFF-Time/ $ \mu $s"  Determines the time length, the signal level of the pulse remains low.

##### Remote command:

[:SOURCE<hw>] :PULM:TRAIN:OFFTime on page 400

"Count"

Sets the number of repetitions for each pulse ("ON-Time/μs"/"OFF-Time/μs" value pair).

Tip: If you set "Count = 0", the corresponding value pair is ignored in the pulse sequence. With this function you can skip value pairs individually, without deleting them from the table. This allows re-enabling a value pair by entering a number unequal to zero.

##### Remote command:

[:SOURCE<hw>] :PULM:TRAIN:REPetition on page 402

"Goto"

Selects row for editing.

Goto first
Goto last

</div>

</div>

If Goto row is selected, a window opens for entering the requested row.

Goto row:
OK    Cancel

</div>

</div>

(it is not possible to change individual positions of the list)

"Edit"

Opens a menu containing editing functions.

| Insert RowInsert Range... |
| --- |
| Fill... |
| Delete RowDelete Range... |

"Insert Row" Inserts a new row before the marked row.

"Insert Range" Inserts new rows before the marked row. The number of rows to be inserted can be defined in an entry window.

Rows to Insert
OK | Cancel

</div>

</div>

"Fill...."

Opens a dialog for defining a set of list values to be automatically entered in the list.

The start line and the number of rows to be filled are defined under "From" and "Range".

The column to be filled is selected under "Select column to fill". Depending on the selection here, the default for start, end, and increment value are set. As the settings are interdependent, a change of one parameter may result in the automatic change of one or more of the other parameters.

The filling of the column with the selected value settings is started with button "Fill".

"Delete Row" Deletes the marked row.

Deletes the selected number of rows including the marked row. The number of rows to be inserted can be defined in an entry window.

"Save As" Opens the file dialog to save the list under a new name. Each list is saved to the CompactFlash™ card as a separate file with the file prefix *.pulstrn. The file name and the directory to which the file is saved are user-selectable.

"Save" The list is saved under its current name.

##### Import/Export - Pulse Train Mode

Expands the menu with the area for import and export of pulse train files.

Externally edited Excel tables with on/off time and repetition triplets can be imported as text files or CSV files and used for pulse train mode.

On the other hand, internally created pulse train lists can be exported as text files or CSV files.

##### Mode - Import/Export Pulse Train Files

Selects if pulse train lists should be imported or exported. The settings offered below depend on the selected mode.

##### Remote command:

[:SOURCE<hw>] : PULM:TRAIN:DEXChange:MODE on page 408

##### Extension - ASCII File Settings

Selects the file extension of the ASCII file to be imported or exported. Selection TXT (text file) or CSV (Excel file) is available.

Remote command:

[:SOURCE<hw>] : PULM:TRAIN:DEXChange:AFILe:EXTENSION on page 406

##### Decimal Point - ASCII File Settings

Selects the decimal separator used in the ASCII data between '.' (decimal point) and ',' (comma) with floating-point numerals.

##### Remote command:

[:SOURce<hw>] :PULM:TRAin:DEXChange:AFILe:SEParator:DECimal

on page 407

##### Column Separator- ASCII File Settings

Selects the separator between the frequency and level column of the ASCII table.

##### Remote command:

[ :SOURce<hw> ] :PULM:TRAin:DEXChange:AFILe:SEParator:COLumn on page 407

##### Select ASCII Source / Destination - Import/Export Pulse Train Files

Opens the "File Manager" for selecting the ASCII file to be imported into a pulse train list (source) or the ASCII file the pulse train list is exported (destination) in.

##### Remote command:

[:SOURce<hw>] :PULM:TRAin:DEXChange:AFILe:CATalog? on page 405

[:SOURce<hw>] :PULM:TRAin:DEXChange:AFILe:SELECT on page 406

##### Select Destination / Source - Import/Export Pulse Train Files

Opens the "File Manager" for selecting the pulse train list to be exported (source) into an ASCII file, or the destination for the ASCII file to be imported (destination) in.

##### Remote command:

[:SOURCE<hw>] :PULM:TRAin:DEXChange:SELECT on page 409

##### Import / Export - Import/Export Pulse Train Files

Starts the export or import of the selected file.

If import is selected, the ASCII file is imported as pulse train list.

If export is selected, the pulse train list is exported into the selected ASCII file.

##### Remote command:

[:SOURCE<hw>] :PULM:TRAin:DEXChange:EXECute on page 408
