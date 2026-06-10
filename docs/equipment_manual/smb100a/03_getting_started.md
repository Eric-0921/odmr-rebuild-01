## 3 Getting Started

### 3.1 Preparing for Use

Here, you can find basic information about setting up the product for the first time.

#### 3.1.1 Lifting and Carrying

See "Lifting and carrying the product" on page 13.

▶ Use the carrying handle at the side for lifting and carrying the R&S SMB.

For mounting the R&S SMB in a rack, see Chapter 3.1.4.2, "Mounting the R&S SMB in a Rack", on page 22.

#### 3.1.2 Unpacking and Checking

1. Unpack the R&S SMB carefully.

2. Retain the original packing material. Use it to protect the control elements and connectors when transporting or shipping the R&S SMB later.

See also "Lifting and carrying" on page 489.

3. Using the delivery notes, check the equipment for completeness.

4. Check the equipment for damage.

If the delivery is incomplete or equipment is damaged, contact Rohde & Schwarz.

#### 3.1.3 Choosing the Operating Site

Specific operating conditions ensure proper operation and avoid damage to the product and connected devices. For information on environmental conditions such as ambient temperature and humidity, see the data sheet.

See also "Choosing the operating site" on page 14.

##### Electromagnetic compatibility classes

The electromagnetic compatibility (EMC) class indicates where you can operate the product. The EMC class of the product is given in the data sheet.

• Class B equipment is suitable for use in:

– Residential environments

– Environments that are directly connected to a low-voltage supply network that supplies residential buildings

- Class A equipment is intended for use in industrial environments. It can cause radio disturbances in residential environments due to possible conducted and radiated disturbances. It is therefore not suitable for class B environments. If class A equipment causes radio disturbances, take appropriate measures to eliminate them.

#### 3.1.4 Setting Up the R&S SMB

See also:

• "Setting up the product" on page 14

"Intended use" on page 13

##### 3.1.4.1 Placing the R&S SMB on a Bench Top

##### To place the product on a bench top

1. Place the product on a stable, flat and level surface. Ensure that the surface can support the weight of the product. For information on the weight, see the data sheet.

2. CAUTION! Foldable feet can collapse. See "Setting up the product" on page 14. Always fold the feet completely in or out. With folded-out feet, do not place anything on top or underneath the product.

3. WARNING! A stack of products can fall over and cause injury. Never stack more than three products on top of each other. Instead, mount them in a rack.

Stack as follows:

• If the products have foldable feet, fold them in completely.

- It is best if all products have the same dimensions (width and length). If the products have different dimensions, stack according to size and place the smallest product on top.

- Do not exceed the permissible total load placed on the product at the bottom of the stack:

– 50 kg when stacking products of identical dimensions (left figure).

– 25 kg when stacking smaller products on top (middle figure).


Right = Stacked incorrectly, too many products

4. NOTICE! Overheating can damage the product.

Prevent overheating as follows:

- Keep a minimum distance of 10 cm between the fan openings of the product and any object in the vicinity.

- Do not place the product next to heat-generating equipment such as radiators or other products.

##### 3.1.4.2 Mounting the R&S SMB in a Rack

##### To prepare the rack

1. Observe the requirements and instructions in "Setting up the product" on page 14.

2. NOTICE! Insufficient airflow can cause overheating and damage the product. Design and implement an efficient ventilation concept for the rack.

##### To mount the R&S SMB in a rack

1. Use an adapter kit that fits the dimensions of the R&S SMB to prepare the instrument for rack mounting.

a) Order the rack adapter kit designed for the R&S SMB. For the order number, see the data sheet.

b) Mount the adapter kit. Follow the assembly instructions provided with the adapter kit.

2. Lift the R&S SMB to shelf height.

3. Push the R&S SMB onto the shelf until the rack brackets fit closely to the rack.

4. Tighten all screws at the rack brackets with a tightening torque of 1.2 Nm to secure the R&S SMB in the rack.

##### To unmount the R&S SMB from a rack

1. Loosen the screws at the rack brackets.

2. Remove the R&S SMB from the rack.

3. If placing the R&S SMB on a bench top again, unmount the adapter kit from the R&S SMB. Follow the instructions provided with the adapter kit.

#### 3.1.5 Considerations for Test Setup

##### Cable selection and electromagnetic interference (EMI)

Electromagnetic interference (EMI) can affect the measurement results.

To suppress electromagnetic radiation during operation:

• Use high-quality shielded cables, especially for the following connector types:

- BNC

Double-shielded BNC cables.

How to: "To connect to non-screwable connectors (BNC)" on page 26

- USB

Double-shielded USB cables.

How to: Chapter 3.1.9, "Connecting USB Devices", on page 25.

See Chapter 9.6, "Measuring USB Cable Quality", on page 506.

- LAN

At least CAT6 STP cables.

How to: Chapter 3.1.7, "Connecting to LAN", on page 24

• Always terminate open cable ends.

• Ensure that connected external devices comply with EMC regulations.

##### Signal input and output levels

Information on signal levels is provided in the data sheet. Keep the signal levels within the specified ranges to avoid damage to the R&S SMB and connected devices.

##### Preventing electrostatic discharge (ESD)

Electrostatic discharge is most likely to occur when you connect or disconnect a DUT.

NOTICE! Electrostatic discharge can damage the electronic components of the product and the device under test (DUT).

Ground yourself to prevent electrostatic discharge damage:

a) Use a wrist strap and cord to connect yourself to ground.

b) Use a conductive floor mat and heel strap combination.

#### 3.1.6 Connecting to Power

For safety information, see "Connecting to power" on page 14.


If there were any problems during power on, check the condition of the mains fuses as described under Chapter 8.2, "Changing Fuses", on page 490.

To connect the R&S SMB to power:

1. Plug the AC power cable into the AC power connector at the rear panel of the instrument. Only use the AC power cable delivered with the R&S SMB.

2. Plug the AC power cable into a power outlet with ground contact.

The required ratings are listed next to the AC power connector and in the data sheet.

#### 3.1.7 Connecting to LAN

##### Network environment

Before connecting the product to a local area network (LAN), consider the following:

• Install the latest firmware to reduce security risks.

- For internet or remote access, use secured connections if applicable.

- Ensure that the network settings comply with the security policies of your company. Contact your local system administrator or IT department before connecting your product to your company LAN.

- When connected to the LAN, the product may potentially be accessed from the internet, which may be a security risk. For example, attackers might misuse or damage the product.

##### To connect to LAN

The "LAN" connector is at the rear panel.

▶ Connect the LAN socket with an RJ-45 cable to the LAN.

By default, the R&S SMB is configured to use DHCP (dynamic host configuration protocol) and no static IP address is configured.

If switched on and connected to the LAN, you can find the address information in the network settings dialog.

See Chapter 4.2.3.8, "Network Settings", on page 104

#### 3.1.8 Connecting to IEC 625/IEEE 488 (GPIB)

##### To connect to the IEC 625/IEEE 488 interface

You can use the GPIB bus interface for controlling the instrument remotely. The "IEC 625/IEEE 488" connector is at the rear panel.

1. Connect the "IEC 625/IEEE 488" socket to the controller PC with a double-shielded GPIB bus interface cable.

2. Configure the settings for remote control over GPIB.

The controller must address the instrument with the GPIB bus address. For details, refer to Chapter 5.1.6, "GPIB Interface (IEC/IEEE Bus Interface)", on page 246 and Chapter A.1, "GPIB Bus Interface", on page 508.

#### 3.1.9 Connecting USB Devices

You can connect or disconnect USB devices, e.g., memory stick, a CD-ROM keyboard, etc, with the R&S SMB during operation.

To connect a USB device to the interface of the R&S SMB, always use the USB type A connector to the rear panel. Refer to the documentation of the USB device to find out which USB connector type you can connect to the USB device.

##### To connect USB storage devices

USB storage devices, such as memory sticks, allow easy data transfer from/to the R&S SMB. You can also use them for firmware updates.

▶ Connect the USB storage device to the USB type A connector at the rear panel.

##### To connect USB devices with external power supply

1. NOTICE! Connected devices with external power supply can feed back current into the 5 V power supply of the USB interface and thus damage the R&S SMB.

Ensure that there is no connection between the positive pole of the power supply and the +5 V power pin of the USB interface (VBUS).

2. Connect the USB storage device to the USB type A connector at the rear panel.

##### To connect a keyboard

▶ Connect the keyboard to the USB type A connector at the rear panel.

When connected, the R&S SMB detects the keyboard automatically. A detected keyboard has the default layout English – US.

Use the Chapter 4.2.3.9, "Display/Keyboard Settings", on page 108 dialog to configure the keyboard properties.

##### To connect a mouse

▶ Connect the keyboard to the USB type A connector at the rear panel.

When connected, the R&S SMB detects the mouse automatically.

##### To connect power sensors

You can also connect power sensors of the R&S NRP series either directly at the USB interface, or using an USB hub, e.g. R&S NRP-Z5 with several connected power sensors.

For sensors with network capability, you can use the LAN interface, see

▶ To connect a power sensor to the USB type A connector, you have several options:

• Connect the sensor to the USB type A connector

If necessary, use an adapter cable, e.g. R&S NRP-Z3 or R&S NRP-Z4

• Connect several sensors to an USB hub, and the hub to the R&S SMB.

See Chapter 4.3.6, "RF Measurement", on page 167.

#### 3.1.10 Connecting to RF

The "RF" connector is at the front panel.

##### To prepare for connecting to RF

1. NOTICE! Damaged or not clean connections can lead to RF insertion loss and mismatch, and even premature wear of the connectors.

Before connecting to the port, inspect the RF connector visually to check that it is clean, undamaged and mechanically compatible.

See the application note 1MA99 for information on how to handle and maintain the RF port, to minimize measurement deviations and ensure its longevity.

2. NOTICE! Risk of instrument damage. Excessive reverse power or DC voltage at the RF connector can damage the instrument.

Make sure that the values do not exceed the reverse power and DC limits as given in the data sheet.

3. If the R&S SMB is switched on, deactivate the RF output, before connecting an RF cable to the RF connector.

In the home screen, select the block "Level" > "RF ON > Off".

##### To connect to non-screwable connectors (BNC)

1. Use a high-quality RF cable that matches the RF connector type.

See "Cable selection and electromagnetic interference (EMI)" on page 23.

2. To connect the RF cable with the RF connector, proceed as follows:

a) Carefully align the connector of the cable and the RF connector along a common axis.

b) Mate the connectors along the common axis until the male pin of the connector of the cable engages with the female socket of the RF connector.

If your instrument is equipped with a test port adapter, see the application note 1MA100.

See "RF 50 Ohm" on page 54.

To prevent RF output switch-off

NOTICE! If you set a too high output level without a load connected to the instrument, the reverse power can exceed a limit forcing the R&S SMB to switch off the RF output.

Connect a load with sufficient return loss as given in the data sheet.

#### 3.1.11 Connecting to Ref In/Ref Out

The connector is at the rear panel.

To connect to Ref In/Ref Out

For connection, the R&S SMB provides BNC connectors.

▶ Follow the instructions in "To connect to non-screwable connectors (BNC)" on page 26.

#### 3.1.12 Switching On or Off

The following table provides an overview of power states, LEDs and power switch positions.

Table 3-1: Overview of power states


| State | LED | Position of power switch |
| --- | --- | --- |
| Off | ● gray | [0] |
| Standby | ● yellow | [I] |
| Ready | ● green | [I] |

##### To switch on the R&S SMB

The R&S SMB is off but connected to power. See Chapter 3.1.6, "Connecting to Power", on page 24.

1. Set the switch on the power supply to position [I].

The switch is at the rear panel.

The LED of the [ON/STANDBY] key is yellow.

2. Wait until the oven-controlled oscillator (OCXO) warms up. For the warm-up time, see data sheet.

3. Press the [ON/STANDBY] key at the front panel.

The LED changes to green. The R&S SMB boots.

When starting for the first time, the R&S SMB starts with the default settings. When restarting the instrument, the settings depend on the instrument configuration before shut-down.

See Chapter 4.2.8, "Storing and Loading Instrument Data - File Key", on page 127.

When the instrument is switched on, it automatically monitors main functions.

A detected fault is indicated by an "ERROR" message displayed in the info line together with a brief error description. For in-depth identification of the error, press the [INFO] key. In response, a description of the error(s) is displayed. In addition to automatic monitoring, you can perform maintenance tasks.

##### See:

Chapter 9, "Status Information, Error Messages and Troubleshooting", on page 499

Chapter 8.4, "Performing Maintenance Tasks", on page 491

##### To reboot the instrument

If it is necessary to restart the instrument, e.g. if the firmware stops unexpectedly:

##### ▶ Press the [STANDBY] key for approx. 5 s.

The R&S SMB reboots.

##### To shut down the product

The product is in the ready state.

##### ▶ Press the [ON/STANDBY] key.

The operating system shuts down. The LED changes to yellow.

In the standby state, the power switch circuits and the OCXO are active. To deactivate them, disconnect the instrument from the power supply.

##### To disconnect from power

The R&S SMB is in the standby state.

1. NOTICE! Risk of data loss. If you disconnect the product from power when it is in the ready state, you can lose settings and data. Shut it down first.

Set the toggle switch on the power supply to position [0].

The LED of the [ON/STANDBY] key is switched off.

2. Disconnect the R&S SMB from the power source.

#### 3.1.13 Default Settings

When the instrument is switched on, it is not the preset state that is active, but rather the instrument state that was set before the instrument was switched off. It is also recommended that you use the [PRESET] key to return the instrument to its defined preset state every time a new configuration is required or the current setup is not anymore relevant.

The R&S SMB offers a two-stage preset concept:

• Preset the instrument to a predefined state

The [PRESET] key calls up a defined instrument setup. All parameters and switching states are preset (also those of inactive operating modes). The default instrument settings provide a reproducible initial basis for all other settings. However, functions that concern the integration of the instrument into a measurement setup are not changed, e.g. GPIB bus address or reference oscillator source settings.

• Preset the instrument to its factory settings

The instrument can also be forced to load its default factory settings. To access the corresponding dialog box, press the [SETUP] key and select the "Factory Preset". For more information and an overview of the settings affected by the factory preset function, see Chapter 4.2.3.16, "Factory Preset", on page 122.

##### Overview of the Most Important Preset States

The following list gives an overview of the presets for the most important generator settings. The other presets can be found in the preset tables of the individual menus and the information accompanying the remote commands.

• "RF frequency" = 1 GHz

• "RF level" RF output switched off

● "Level" = 30 dBm for instruments including an attenuator

"Level" = -5 dBm for instruments with no attenuator

• "Offsets" = 0

• "Modulations State" = Off

• Uninterrupted level settings are switched off "Level Attenuator Mode" = AUTO

• Internal level control "Level ALC" = AUTO

• User correction "Level Ucor" = OFF

• "LF output State" = Off

"Sweep State" = Off

Settings that are not affected by the [PRESET] key

• Reference frequency settings ("Ref Oscillator" menu)

• Power on settings ("Level/EMF" menu)

• Network settings ("Setup" menu)

• GPIB address ("Setup" menu)

*IDN? Identification and emulation ("Setup" menu)

Password and settings protected by passwords ("Setup" menu)

• Start/Stop Display Update ("Setup" menu)

• Display and keyboard settings ("Setup" menu)


User-defined instrument states can be stored and called up in the "File" dialog.

#### 3.1.14 Working with Linux Operating System

The instrument uses an embedded Linux operating system, optimally adapted to the instrument.


##### Accessing the operating system

No access to the operating system is required for normal operation.

All necessary system settings can be made in the "Setup" dialog.

#### 3.1.15 Setting Up a Network (LAN) Connection

The R&S SMB is equipped with a network interface and can be connected to an Ethernet LAN (local area network). Provided the appropriate rights have been assigned by the network administrator and the Linux firewall configuration is adapted accordingly, the interface can be used, for example:

- To transfer data between a controller and the instrument, e.g. in order to run a remote control program.

See Chapter 5, "Remote Control Basics", on page 240.

- To access or control the measurement from a remote computer using the R&S VISA or Ultr@VNC programs (or similar tools, like another VNC client or any Web browser supporting Java)

- To transfer data from a remote computer and back, e.g. using network folders

This section describes how to configure the LAN interface. It includes the following topics:

Chapter 3.1.7, "Connecting to LAN", on page 24

Chapter 3.1.15.1, "Assigning the IP Address", on page 30

##### 3.1.15.1 Assigning the IP Address

Depending on the network capacities, the TCP/IP address information for the instrument can be obtained in different ways.

- If the network supports dynamic TCP/IP configuration using the Dynamic Host Configuration Protocol (DHCP), all address information can be assigned automatically.

- If the network does not support DHCP, the instrument tries to obtain the IP address via Zeroconf (APIPA) protocol. If this attempt does not succeed or if the instrument is set to use alternate TCP/IP configuration, the addresses must be set manually.

By default, the instrument is configured to use dynamic TCP/IP configuration and obtain all address information automatically. This means that it is safe to establish a physical connection to the LAN without any previous instrument configuration.

#### NOTICE

##### Risk of network errors!

Connection errors can affect the entire network.

If your network does not support DHCP, or if you choose to disable dynamic TCP/IP configuration, you must assign valid address information before connecting the instrument to the LAN.

Contact your network administrator to obtain a valid IP address.

##### Assigning the IP address on the instrument

1. Press the [Setup] key and select the "Network Settings" dialog.

2. Set the "Address Mode" to Static.

3. Select the "IP Address" and enter the IP address, for example 192.168.0.1..

The IP address consists of four number blocks separated by dots. Every block contains 3 numbers in maximum.

4. Select the "Subnet Mask" and enter the subnet mask, for example 255.255.255.0. The subnet mask consists of four number blocks separated by dots. Every block contains 3 numbers in maximum.


##### Use computer names to identify the instrument

In networks using a DHCP server, it is recommended that you address the instrument by its unambiguous computer name, see Chapter 3.1.15.2, "Using Computer Names", on page 31.

A computer name (hostname) is an unique dedicated identification of the instrument, that remains permanent as long as it is not explicitly changed. Hence, you can address an instrument by the same identification (computer name), irrespectively if a network or a point-to-point connection is used.

##### To assign the IP address manually on the remote computer

▶ Obtain the necessary information from your network administrator. If you use more than one LAN connector, you need separate address information for each connector.

For information on how to perform the configurations, refer to the documentation of the operating system the remote computer uses.

##### 3.1.15.2 Using Computer Names

In a LAN that uses a DNS server (Domain Name System server), each PC or instrument connected in the LAN can be accessed via an unambiguous computer name

instead of the IP address. The DNS server translates the host name to the IP address. This is especially useful when a DHCP server is used, as a new IP address may be assigned each time the instrument is restarted.

Each instrument is delivered with an assigned computer name, but this name can be changed.

The default instrument name is a non-case-sensitive string that follows the syntax

rs<serial number>.

The serial number can be found on the rear panel of the instrument. It is the third part of the device ID printed on the bar code sticker:


ID: 1419.0035K02 - 102030 - Zd Serial Number


##### Querying and changing a computer name

1. Press the "Setup" key and select "Network Settings". The computer name is displayed under "Hostname".

2. Press the "Setup" key, select "Protection" and enable the "Protection Level 1". The default password is 123456.

The parameter "Hostname" in the "Network Settings" dialog is now enabled for configuration.

3. Change the "Hostname".

#### 3.1.16 Remote Access via an External Controller

The R&S SMB can be remote accessed from a remote computer (external controller) via a network link. This allows convenient operation of the instrument from the desktop although the instrument is integrated in a rack somewhere else.


For an overview of the instrument's operating concept and the different ways to control and operate the instrument, see Chapter 3.4, "System Overview", on page 62.

There are different ways to establish a remote access connection to the signal generator but all of them require an established LAN connection between the instrument and the remote computer. The simplest way to remote access the instrument is to use a Web browser, such as Windows Internet Explorer or Mozilla Firefox for instance. Alternatively a remote access via a special application can be used.

For example, the free-of-charge program Ultr@VNC for PCs with Linux/Unix or Windows operating system is available for setting up the remote access connection. Using this application requires additional installation.

See the following table for an overview of the different ways to establish a remote access connection to the signal generator.

Table 3-2: Remote access via an external computer


| Remote access via | LAN connection | Installation of the additional application |  |
| --- | --- | --- | --- |
|  |  | on the instrument | on the remote computer |
| Any web browserfor example Windows Internet Explorer or Mozilla Firefox, see Chapter 3.1.16.1, "Using a Web Browser for Remote Access", on page 33 | required | no | Java Runtime must be installed and activated in the browser settings. |
| Web browser with HTML5for example LXI Browser, see "Web Control" on page 45 | required | no | Web sockets must be supported. |
| VNC Clientfor example Ultr@VNC or other dedicated client softwarefor PCs with Linux/Unix or Windows operating systemsee Chapter 3.1.16.2, "Remote Access via a VNC Client Software", on page 34 | required | required | VNC Viewer required |

When the connection is set up with a VNC client software (Ultr@VNC), direct control on the instrument is possible while remote access is established.

For return to direct operation on the instrument, the connection must be cut. After cutting the connection, it is still enabled and can be established again any time. The connection is disabled only after deactivation of the program.

This section gives an information on how to use the Web browser for remote access, how to install the applications for remote access and how to establish the connection between the instrument and an external computer with Windows operating system. Remote access via an external computer with Linux/Unix operating system is performed accordingly.


##### Default password

Remote-access and file access require the user "instrument" with default password "instrument".

#### NOTICE

##### Changing the default user and security passwords

It is highly recommended to change the default user and security passwords in the menu "Setup > Security" before connecting the instrument to the network (see section Chapter 4.2.3.14, "Security", on page 114).

##### 3.1.16.1 Using a Web Browser for Remote Access

The instrument can be remote-accessed via any web browser, as for example the Windows Internet Explorer or Mozilla Firefox.


Alternatively, you can also make use of the LXI browser, as described in "Web Control" on page 45.

To remote access the instrument via a web browser:

1. Connect the instrument and the remote computer to a LAN, see Chapter 3.1.7, "Connecting to LAN", on page 24.

2. Instal the Java Runtime Environment JRE on the remote computer.

3. Type the instruments' IP address in the address field of the Web browser on your PC, e.g. http://10.111.11.1

The "VNC Authentication" screen appears.


TightVNC applet - Windows Internet Explorer
http://10.111.11.1/
File Edit View Favorites Tools Help
TightVNC applet
Disconnect Options Clipboard Send Ctrl-Alt-Del Refresh
VNC Authentication
OK


VNC Authentication

Password:

4. Enter the password and select "OK". The default password is "instrument".

After the connection is established, the current signal generator screen with the block diagram is displayed and the instrument can be remote-accessed from the remote computer.

##### 3.1.16.2 Remote Access via a VNC Client Software

The following description explains how to establish the remote access, represented by means of the client software Ultr@VNC. The software is included in Linux/Unix operating system, but it is also available as a free-of-charge download on the Internet http://www.uvnc.com/download/index.html. Thus, it is also available for remote computers with Windows operating system.

A VNC client software is an application which can be used to access and control the instrument from a remote computer via LAN.

The GUI appearance of Ultr@VNC may vary if you use a later release. Also, similar programs may deviate in some details, but the basic procedure is the same.


#### NOTICE

##### Risk of unauthorized access

If the VNC service is enabled on the instrument, any user in the network who knows the computer name and password can access it.

Disable the VNC service on the instrument to prevent unauthorized access.

See "LAN Services" on page 117.

##### Setting up a VNC connection

1. Connect the instrument and the remote computer to a LAN, see Chapter 3.1.7, "Connecting to LAN", on page 24.

2. Install the Ultr@VNC application and enable it on the instrument.

3. In the ICF firewall, enable communication on the network via Ultr@VNC program.

4. Install the VNC Viewer on the remote computer with Windows operating system, see "Installing the VNC Viewer on a Windows PC" on page 37.

5. Set up the VNC connection between the instrument and:

a) the remote computer with Linux/Unix operating system, see "Setting up the VNC connection on the Linux/Unix remote computer" on page 37;

b) the remote computer with Windows operating system, see "Setting up the VNC connection on the Windows remote computer" on page 38.


##### Enabled Direct Control

The direct control of the instrument is not disabled and the instrument can be controlled from the front panel and via the remote computer alternately.

##### Installing the Ultr@VNC application

1. Download the program from the internet and copy it to a directory that can be accessed.

2. On the instrument, shut down firmware using the ALT+F4 key combination.

3. Double click on the setup file to start the installation.

The setup wizard leads through the installation. This description focus only on the relevant settings.

a) Select installation of all components.


Setup - Ultr@VNC Release 1.0.0 RC 18

Select Components
Which components should be installed?

Select the components you want to install: clear the components you do not want to install. Click Next when you are ready to continue.

Full installation
☑ UltraVNC Server
☑ UltraVNC Viewer
☑ UltraVNC Repeater

1.8 MB
1.0 MB
0.1 MB

Current selection requires at least 2.7 MB of disk space.

< Back Next > Cancel


b) In the "Additional Task Panel", enable all entries.


☑ Create a System Viewer with Ultr@VNC Viewer

### A successful installation is indicated by a message.

##### WinVNC


The WinVNC service was successfully registered  

The service may be started from the Control Panel, and will automatically be run the next time this machine is reset


OK


At the same time a warning is displayed stating that a password must be set.

##### WinVNC Error


WinVNC Error
WARNING: This machine has no default password set. WinVNC will present the Default Properties dialog now to allow one to be entered.
OK


4. Select "OK".

The "Default Local System Properties" panel opens.


WinVNC: Default Local System Properties
Incoming Connections
Accept Socket Connections
Password:
Display Number or Ports to use Auto
Display N
Ports Main: 5900
MS Logon
Enable Java Viewer (Http Connection)
Enable Xdmcp (X11 Connection)
When Last Client Disconnects
Do Nothing
Lock Workstation (W2K)
Logoff Workstation
Misc.
Enable Blank Monitor on Viewer Request
Enable File Transfer
Log debug infos to the WinVNC.log file
Allow Loopback Connections
Default Server Screen
1 / 1
Connection Settings
Force View Only (disable Viewers Inputs)
Disable Local Inputs (Keyboard & Mouse)
Remove Desktop Wallpaper
Update Handling
Poll Full Screen (Fast)
Poll Foreground Window -1
Poll Window Under Cursor -1
Polls HookDil
Video Hook Driver
Low Accuracy (Turbo Speed)
DSM Plugin
Use No Plugin detected...
Share only the Window Named :
Share
Query on incoming connection
Display Query Window
Apply
Timeout: 10
Cancel


5. Enter a password with a length of at least five digits.

This password is used on the remote computer to access the instrument. Other settings may be changed according to the user-specific security requirements.

After the installation the Ultr@VNC program is automatically started together with the operating system. On mouse over, the IP address of the instrument is indicated.

This IP address and the user-defined password are the prerequisites to enable remote access on the remote computer. Terminated connection is indicated by changed icon color.

##### Installing the VNC Viewer on a Windows PC

1. Download the Ultr@VNC program form internet and follow the installation instructions.

Only the program component VNC Viewer is required.

Note: The VNC Viewer program is included in the download for the installation of the Ultr@VNC program on the signal generator if "Full installation" was selected in the "Select Component" panel. In this case, the program ultr@vncviewer.exe can be copied to the Windows PC.

2. Install VNC Viewer program component on the remote computer.

##### Setting up the VNC connection on the Linux/Unix remote computer

The VNC program is available per default for Linux/Unix operating systems.

1. Start a Web browser on the remote computer and enter the IP address of the instrument.

2. Enter the following address:

vnc://<IP-address of the instrument>, e.g. vnc://192.168.1.1.

A dialog is opened and the password for the remote VNC connection is requested.

3. Enter the password as defined in the "Default Local System Properties" panel of the Ultr@VNC program and select "Log On".

The connection is established, the instrument is remote accessed and the current signal generator screen with the block diagram is displayed. The individual functions are operated using the mouse and keyboard.

In contrast to remote access via Remote Desktop, the direct control of the instrument is not disabled and the instrument can be controlled from the front panel and via the remote computer alternately.

##### Setting up the VNC connection on the Windows remote computer

1. Start VNC Viewer program component on the PC, select "VNC Server" and enter IP address of the instrument.


| Connection | ✗ |
| --- | --- |
| VNC Server: 192.168.1.1 (host:display or host::port) |  |
| Quick Options |  |
| AUTO | (Auto select best settings) |
| CULTRA | (>2Mbit/s) - Experimental |
| CLAN | (>1Mbit/s) - Max Colors |
| CMEDIUM | (>128Kbit/s) - Max Colors |
| CMODEM | (19 - 128Kbit/s) - 256 Colors |
| CSLOW | (<19Kbit/s) - 256 Colors, Fuzzy |
| View Only | Auto Scaling |
| Use DSMPlugin | No Plugin detected... |
| Proxy/Repeater |  |

2. To initialize the connection, select "Connect".

A message requesting the password appears.


YNC Authentication
Password: ___
Log On ___ Cancel ___


3. Enter the password as defined in the "Default Local System Properties" panel of the Ultr@VNC program and select "Log On".

The connection is established, the instrument is remote accessed and the current signal generator screen with the block diagram is displayed. The individual functions are operated using the mouse and keyboard.


The direct control of the instrument is not disabled and the instrument can be controlled from the front panel and via the remote computer alternately.

##### Enabled Direct Control

##### Terminating VNC Connection

The remote access via VNC connection can be terminated either on the R&S SMB or on the external PC. Terminating the connection does not disable it. It can be established again any time. See the notice above concerning unauthorized access due to VNC connection!

1. Terminate the connection on the R&S SMB

a) Press the "Windows" key to access the operating system.

b) Right-click on the VNC icon on the task bar and select "Kill all clients".

2. To terminate the connection on the external Linux/Unix PC, close the internet browser or close the signal generator window.

3. To terminate the connection on the external Windows PC, close the VNC Viewer program.

The connection is terminated. The color of the VNC icon in the status bar of the instrument changes.

A message on the external PC indicates the disconnection.

##### Disabling Remote Access via Ultr@VNC

The VNC connection can be disabled by removing the program on the instrument or by deactivating the VNC Server service.

1. Remove the VNC program

a) Press the "Windows" key to access the operating system and open the "Add or Remove Programs" by selecting "Start > Settings > Control Panel > Add or Remove Programs".

b) Remove the VNC program.

2. Deactivate the VNC Server service

a) Press the "Windows" key to access the operating system and open the "Services" by selecting "Start > Settings > Control Panel > Services".

b) Deactivate the VNC Server service.

The connection is disabled, the VNC icon disappears from the task bar of the instrument.

#### 3.1.17 LXI Configuration

LXI ("LAN eXtensions for Instrumentation" is an instrumentation platform for measuring instruments and test systems that is based on standard Ethernet technology. LXI is intended to be the LAN-based successor to GPIB, combining the advantages of Ethernet with the simplicity and familiarity of GPIB.

Like GPIB, LXI determines and standardizes the way the instrument behaves in a LAN. The LXI implementation by the Rohde&Schwarz signal generators allows you to change certain LAN settings, to reset the LAN connection as well as to identify the instrument easily.

For information about the LXI standard, refer to the LXI website at http://www.lxistan-dard.org. See also "News from Rohde & Schwarz, article 2006/II - 190".

The R&S SMB provides an integrated "LXI Status" dialog for LXI status indication and reset of the LAN configuration ("LAN Configuration Initialize", LCI). To access the LXI status dialog, press the [Setup] key and select "Remote > LXI Status".

For further information, see Chapter 4.2.3.12, "LXI Status", on page 112.


##### Firmware update

After a firmware update, shut down and restart the instrument in order to enable the full LXI functionality.

##### Default state of the network settings

According to the LXI standard, an LCI must set the following parameters to a default state.


| Parameter | Value |
| --- | --- |
| TCP/IP Mode | DHCP + Auto IP Address |
| Dynamic DNS | Enabled |
| ICMP Ping | Enabled |
| Password for LAN configuration | LxiWebIfc |

The LCI for the R&S SMB also resets the following parameters:


| Parameter | Value |
| --- | --- |
| Hostname | <Instrument-specific host name> |
| Description | Vector Signal Generator |
| Negotiation | Auto Detect |
| VXI-11 Discovery | Enabled |

The LAN settings are configured using the instrument's "LXI Browser Interface".

##### 3.1.17.1 LXI Browser Settings

To access the instrument via the web browser:

▶ Type in the instrument's host name or IP address in the address field of the browser on your PC, for example "http://10.113.1.205".

Note: Do not add the missing zeros in the IP address, while opening the instrument home page.

The instrument home page (welcome page) opens.


ROHDE&SCHWARZ LXI


Home
Lan Configuration
Status
Utilities
Instrument Model
Manufacturer
Serial Number
Description
LXI Version
LXI Extended Features
DNS Host Name(s)
MAC Address
IP Address
Firmware Revision
Current Time
Current Time source
VISA resource string
Glossary
Device Indicator
www.rohde-schwarz.com
Status
No error

R&S SMB
Rohde & Schwarz GmbH & Co. KG
100011
Rohde & Schwarz Signal Generator SMB100A 100011
1.4 LXI Core 2011
instrument
00:25:64:C3:31:82
10.113.1.150
03.20.390.12
Wednesday, 2015/12/02, 11:54:06
Operating System
TCPIP::10.113.1.150::inst0::INSTR
TCPIP::10.113.1.150::hislip0::INSTR
ACTIVE (press to toggle)
© 2015 ROHDE&SCHWARZ. All rights reserved.


© 2015 ROHDE&SCHWARZ. All rights reserved.

The navigation pane of the browser interface contains the following elements:

##### "LXI"

– "Home" opens the instrument home page.

The home page displays the device information required by the LXI standard, including the VISA resource string in read-only format.

– "Device Indicator" activates or deactivates the LXI status indication. When activated, the LXI LEDs flash, both in the browser dialog and in the LXI dialog of the connected instrument. A green LXI status symbol indicates that a LAN connection has been established; a red symbol indicates that no LAN cable is connected.

– "Lan Configuration" allows you to configure LAN parameters and to initiate a ping, see "Ping Client" on page 44.

– "Status" displays information about the LXI status of the instrument.

##### • "Instrument Control"

– "Utilities" provides access to the LXI event log functionality required by the LXI standard.

– "Web Control" provides remote access to the instrument, see "Web Control" on page 45.

##### • "Diagnostics"

– "SCPI Remote Trace" records messages exchanged via the remote control interface, see "SCPI Remote Trace" on page 46.

##### ● "Help"

– "Glossary" explains terms related to the LXI standard.

www.rohde-schwarz.com opens the Rohde & Schwarz home page.

▶ Press the "INACTIVE (press to toggle)" button to activate the connection.

ACTIVE (press to toggle)


●


A green flashing status LED indicates the active connection. If the LAN connection fails, the LED turns red.

The status of the LAN connection is also indicated by the "LAN Status indicator" in the "LXI Status" dialog.

For further information, see section "LXI Status" in the Operating Manual.

##### 3.1.17.2 LAN Configuration

The "LAN Configuration" web page provides access to the parameters required for identifying the R&S SMB in the network, and allows modification.


##### Password protection

Changing the LAN configuration is password-protected. The default password is Lxi-Weblfc (notice upper and lower case characters). This password cannot be changed in the current firmware version.

It comprises the following navigation entries.

IP Configuration ___ 42

● Advanced LAN Configuration.....43

☑ Ping Client.....44

Web Control.....45

● SCPI Remote Trace.....4

##### IP Configuration

The "IP Configuration" page displays all mandatory LAN parameters.


ROHDE&SCHWARZ

LX1
LAN Parameters
Home
Lan Configuration
IP Configuration
Advanced Config
Ping Client
DNS Hostname(s)
Domain
rssmb100a100020.rsint.net
rsint.net

Attention! Changing the hostname reboots the device!

Status
Utilities
Description
SMB (3.20.x.x) 000000

Instrument Control
IP Address Mode
DHCP + Auto IP Address

Web Control
IP Address
10.113.1.150

File Download
Subnet Mask
10.111.1.32

File Upload
Default Gateway
10.111.0.1

Diagnostics
Obtain DNS Server Address automatically

Device Screenshot
DNS Server(s)
10.0.2.166

SCPI Remote Trace
Register Device at DNS Server dynamically

SCPI Command Shell
HiSLIP Port
4880

Help
Glossary
www.rohde-schwarz.com

Status
No error

Submit
(Password required!)


The "IP Address Mode" selects a configuration mode for the IP address of the R&S SMB. With static configuration, the entered IP address, subnet mask, and default gateway are used. With dynamic configuration, DHCP or dynamic link local addressing (automatic IP) are used to obtain the instrument IP address.

See Chapter 3.1.15.1, "Assigning the IP Address", on page 30.

##### Advanced LAN Configuration

The "Advanced Config" page provides LAN settings that are not declared mandatory by the LXI standard.


ROHDE&SCHWARZ

LX1
LAN Parameters
Home
Lan Configuration
IP Configuration
Advanced Config
Ping Client
Status
Utilities
mDNS and DNS-SD
ICMP Ping enabled
VXI-11 Discovery
Submit
(Password required!)

Web Control
Diagnostics
SCPI Remote Trace
Help
Glossary
www.rohde-schwarz.com

Status
No error
© 2015 ROHDE&SCHWARZ. All rights reserved.


The following advanced parameters are available:

"mDNS and DNS-SD": The additional protocols "multicast DNS" and "DNS service discovery" are used for device communication in zero configuration networks, working without DNS and DHCP.

● "ICMP Ping enabled": Must be enabled to use the ping utility.

If you disable this setting, the instrument does not answer ping requests. The setting does not affect the LXI ping client. You can ping other hosts from the instrument, even if the setting is disabled.

"VXI-11 Discovery": Must be enabled to detect the instrument in the LAN. If you disable this setting, the instrument cannot be detected by the VXI-11 discovery protocol mechanism. The setting does not affect other detection mechanisms. Setting up a VXI-11 connection via the IP address or the host name is independent of this setting.

#### Ping Client

The "Ping Client" page provides the ping utility to verify the connection between the LXI-compliant instrument and another device.

The ping is initiated from the instrument. Using the ICMP echo request and echo reply packets, the function checks whether the communication with a device via LAN works. Ping is useful for the diagnosis of IP network or router failures.

The ping utility is not password-protected.

To initiate a ping from the instrument to the device:

1. Enable "ICMP Ping" on the "Advanced LAN Configuration" page.

2. Select the "Ping Client" page.

3. In the "Destination Address" field, enter the IP address of the device you want to ping (without the ping command and without any further parameters), e.g. 10.113.1.203.

4. Select "Submit".


ROHDE&SCHWARZ

LX1
☑ Ping Parameter

Home
☑ Lan Configuration
Status
☑ Utilities
Destination Address

Instrument Control
Web Control

Clear
☑ Submit
PING 10.111.1.32 (10.111.1.32): 56 data bytes
64 bytes from 10.111.1.32: seq=0 ttl=64 time=0.280 ms

Diagnostics
SCPI Remote Trace
Result
--- 10.111.1.32 ping statistics ---
1 packets transmitted, 1 packets received, 0% packet loss
round-trip min/avg/max = 0.280/0.280/0.280 ms

Help
Glossary
www.rohde-schwarz.com

Status
No error
© 2015 ROHDE&SCHWARZ. All rights reserved.


##### Web Control

The web control functionality provides remote operation via VNC using a Web browser (with HTML5). This mode does not require additional installation or activation. The VNC protocol allows simultaneous operation from several remote devices. The instrument remains locally operable.

The GUI of the R&S SMB is visible. To perform the settings, you can operate the instrument as with the manual control. The instrument controls are available via the front panel simulation. File upload and download between the instrument and the remote PC is also available.

##### Starting a Remote Control via the LXI web browser

This section assumes that the instrument and the controller PC are connected in the LAN.

1. Start a web browser that supports html5 (W3C compliant).

2. Enter the IP address of the R&S SMB in the browser's address bar. The R&S SMB's welcome page is displayed.

3. In the navigation pane, select "Instrument Control" > "Web Control".

Remote access to the instrument requires the password. The default password is instrument.

4. Enter the password.

5. Confirm with the [Enter] key.

After the connection is established, the current screen of the R&S SMB is displayed in the browser window.


File Edit View Favorites Tools Help
Page Safety Tools
Connected (unencrypted) to: rssmb100a100020:0
Freq RF ON MOD ON Level
311.000 000 000 MHz -20.00 dBm
RfSweep, ALC-Auto Info
LF NRP-Z81 -59.59 dBm Avg
Mod Gen Modulation RF
config... config... config... RF
On On On On
LF AM Sweep


6. Use the mouse cursor and keyboard to access the functionality of the instrument as you would directly perform the settings on the instruments front panel.

##### SCPI Remote Trace

The remote trace functionality allows you to trace input and output strings at the remote control interface of the R&S SMB.

A recorded trace (message log) can be evaluated directly in the dialog. Use the highlighting and navigation functions provided in the lower toolbar to locate error messages and messages containing arbitrary search strings. You can also export the message log to a *.csv file and evaluate the file using a suitable program.

To trace and display messages:

1. In the navigation pane, select "Diagnostics" > "SCPI Remote Trace".

2. In the toolbar bar of the "SCPI Remote Trace" page, select "live mode" > "on" and "logging" > "on".

"live mode > on" displays all commands and responses, and "logging > on" also traces messages.

3. If you now control the R&S SMB with SCPI commands, using an appropriate tool, the SCPI remote trace records the information sent and received.


ROHDE&SCHWARZ LXI


Web Control

Diagnostics

SCPI Remote Trace

##### Help


Status
live mode: on off logging: on off filter log file: refresh download clear details
Utilities
rec MT | message
Instrument Control
0 > 1 "idn?
0 < 1 Rohde&Schwarz, 
Web Control
0 > 1 "opt?
0 < 1 0
Diagnostics
0 > 1 Freq?
0 < 1 1000000000
SCPI Remote Trace
0 > 1 LEV-20
0 > 1 LEV?
Help
Glossary
www.rohde-schwarz.com
Status
20 live records received
© 2015 ROHDE&SCHWARZ All rights reserved.


The function records all sent commands, received responses and messages, and stores them in an internal database. If "live mode" is disabled, you can display the recent traces upon request, using the "refresh" button. You can also store the log in a file.

Note: The diagnostics functionality is extended in later releases, e.g. to download or upload SCPI command files from / to the instrument.

##### Toolbars

The toolbar at the top of the dialog provides basic settings and functions.

live mode: ☉ on ☐ off logging: ☉ on ☐ off ☎ filter ☎ log file: ☎ refresh ☎ download ☎ clear

● "Live mode" / "logging": If logging is switched on, messages are traced. They are stored in an internal database and can be displayed upon request, using the refresh button (live mode off) or they can be displayed automatically (live mode on).

● "Filter": applies a filter to columns and/or rows when working (live mode off)

● "Refresh": reads the message log from the internal database and displays it

"Download": stores the SCPI trace log to a *.csv file

● "Clear": deletes all message log entries in the database and on the screen

- "Details": displays details of the selected message, for example an SCPI command in hex format (also possible by double\-\clicking a message)

##### Columns

The following columns are available if no column filter is applied:

● "Rec": record number of the message within the message log

• I: number of the subinstrument

"MT": indicates the type of the message. Possible values and related message contents are:

– > = incoming command

– < = outgoing response to a query

– E = error message, highlighted by red color

– T = execution time, i.e. time required by the instrument to process the command internally

- "message": indicates the type of the message. Possible values and related message contents are:

– > = incoming command

– < = outgoing response to a query

– E = error message, denoted in red

– T = execution time, i.e. time required by the instrument to process the command internally

### 3.2 Instrument Tour

#### 3.2.1 Front Panel Tour

The front panel of the R&S SMB consists of the VGA display, some utility keys (left side) and the hardkey area with connectors and control interfaces (right side). The subsequent sections provide brief explanations on the controls and connectors, the hard-key area and the front panel.


M/p


Figure 3-1: Front panel view


##### 3.2.1.1 Utility Keys


PRESET LOCAL
HELP SETUP
FILE INFO
WINBAR


The keys to the left of the display cause the R&S SMB to return to a definite instrument state and provide information on the instrument and assistance.

For more information refer to chapter "Instrument Settings".

##### [PRESET]

Sets the instrument to a defined state (see Chapter 3.1.13, "Default Settings", on page 29).

##### [LOCAL]

Switches from remote control to local (manual) control.

##### [SETUP]

Opens the "Setup" dialog for configuring presettings.

For more information, see Chapter 4.2.3, "General Configuration of Instrument - Setup Key", on page 97.

##### [File]

Activates the menu for storing or loading files (see Chapter 3.5.1.8, "File Management", on page 86).

##### INFO

Displays status messages, error messages and warnings.

##### [WINBAR]

Toggles between the diagram and active menus.

##### [HELP]

Displays context-sensitive help text.

##### 3.2.1.2 Standby LEDs and Standby Key


ON STANDBY
①


The standby LEDs and the [ON/STANDBY] key are located in the bottom left corner of the front panel.

The [ON/STANDBY] key toggles the instrument between standby and ready state (indicated by the standby LEDs).

The standby LEDs indicate the instrument states as follow:

- the green LED (left) is on when the instrument is ready for operation.

• the yellow LED (right) is on in the standby mode.

See Table 3-1.

##### 3.2.1.3 Display

The display clearly shows all main settings and signal generator states.

The display is divided into the following sections:

• Frequency and level display with info line

– Frequency and level settings containing offset.

– Status messages

Brief error messages.

To access an window with detailed information for a message, use the [INFO] key.

##### • Block diagram

The block diagram shows the current configuration and the signal flow in the generator with the aid of function blocks containing an on/off switch. Selecting a function block opens a list of associated setting menus. Active menus, info windows and graphs are displayed on top of the block diagram. The block diagram can be displayed in the foreground anytime with the [DIAGRAM] key.

For detailed information, see Chapter 3.5.1.3, "Display", on page 71.

##### 3.2.1.4 Setup Keys

The keys to the right of the display set parameters, select views and control the windows.

##### Keys for Setting Parameters


FREQ
LEVEL
ON/OFF TOGGLE
BACK-SPACE


These keys provide direct access to the settings in the header of the instrument for fast setting the RF signal.

For more information refer to chapter "Instrument Functions".

##### [FREQ]

Activates frequency entry.

##### [LEVEL]

Activates level entry.

##### [ ON/OFF TOGGLE]

• Switches highlighted elements or a function block on and off.

- Switches between two or more settings, e.g. items of selection lists. At the end of a list, the cursor is set on the first entry again.

##### [BACKSPACE]

Deletes the character to the left of the cursor.

##### Display Keys


ESC CLOSE
DIAGR
MOD
ONIOFF
RF
ONIOFF


The keys assigned right to rotary knob arrange different windows on the display, and turn the RF signal and the modulation on and off.

##### [DIAGRAM]

Brings the block diagram to the foreground. Active menus are minimized.

##### [ESC]

The function of this key depends on the current cursor position.

• Calls the next higher selection level.

- Closes the open window without accepting new entries; the old value or parameter is retained.

• In dialog boxes that contain a "Cancel" button it activates that button.

• Closes all kinds of dialog boxes, if the edit mode is not active.

• Quits the edit mode, if the edit mode is active.

• Switches between different entry fields of a menu.

- Shifts the entry cursor from the header display to the previously active menu, or to the previously highlighted block in the block diagram if no menu is active.

##### [MOD ON/OFF]

Switches the modulations on and off.

"MOD OFF" is displayed in the info line of the header next to the "Level" field.

##### [RF ON/OFF]

Switches the RF signal on and off.

"RF OFF" is displayed in the header next to the "Frequency" field.

##### 3.2.1.5 Keypad for data entry

The keys in the data entry keypad are used to enter alphanumeric data and units.

Data entry keys are only enabled while the cursor is placed on a data input field in a dialog. Their function depends on the data type of the input field.


7 8 9 G/n
abc def dBμV
4 5 6 M/μ
ghi jkl mno μV
1 2 3 k/m
pqrs tuv wxyz mV
0 1 x1
bI dB(m)


| Keys | Description |
| --- | --- |
| 0...9/abc | Enters the corresponding numbers (in numeric input fields) or characters (character input fields). |
| . | Inserts a decimal point (numeric input fields) or dot (character input fields) at the cursor position. Multiple decimal points are not allowed. |
| Unit keys | Selects a unit and thus determine the absolute value, or changes the unit, i.e. trigger a recalculation without changing the absolute value. The function depends on the time at which the [Unit] key is used during parameter entry (see "Working with Units" on page 81). For unit-free values, the [x1] key is equivalent to [ENTER]. It confirms the previous entry and deactivates the input field. |
| _ | Adds a blank in a character input field. |
| *...# | Enters special characters. Toggles through the available characters if the key is pressed several times in a row. |
| A <-> a | Toggles between uppercase and lowercase characters. |
| A, B, C, D, E, F | Enters hexadecimal values. The letters assigned to the keys are automatically active when an entry field with a hexadecimal value is active. |

##### [ENTER]

Pressing the rotary knob has the same effect.

- Concludes the entry of dimensionless entries. For other entries, this key can be used instead of the default unit key. The new value is accepted.

• Confirms ("OK") and closes open input windows.

• In a dialog box, selects the default or focused button.

• In a dialog box, activates the edit mode for the focused area, if available.

- In a dialog box, activates or deactivates the selected option of the focused area, if the edit mode is active.

• Calls the next menu level.

##### 3.2.1.6 Rotary Knob and Navigation Keys

The rotary knob and the arrow keys are alternative control elements for data variation and navigation in the graphical user interface.


##### [Rotary Knob]

The rotary knob has several functions:

- Increases (clockwise direction) or decreases (counter-clockwise direction) numeric values at a defined step width in editing mode

• Moves the cursor, e.g. to a function block in the block diagram

• Scrolls within lists, tables or tree views

• Acts like the [ENTER] key, when it is pressed.

- Shifts the selection bar within focused areas (e.g. lists), if the edit mode is activated.

Note: Turning or pressing the rotary knob is equivalent to pressing the [Up] and [Down] keys or the [ENTER] key in the keypad.

##### [Navigation Keys]


The navigation keys consist of 4 arrow keys which are used for navigation, alternatively to the rotary knob.

[UP/ DOWN The up and down arrow keys do the following:

- In a numeric edit dialog box, increase or decrease the instrument parameter.

• In a list, table, window or dialog box, scroll vertically.

[LEFT/ RIGHT The left and right arrow keys do the following:

- In an alphanumeric edit dialog box, move the cursor forward and back.

• In a list, table, window or dialog box, scroll horizontally.

##### 3.2.1.7 Front Panel Connectors

The RF and LF output connectors and a connector for external modulation signals are located on the front panel.

##### MOD EXT


→ MOD EXT


Input for external modulation signals.

[LF]


LF


Output for internal LF modulation generator signal.

See also data sheet and Chapter 4.5.1, "Overview of LF Generator", on page 223, Chapter 4.5.2, "LF Output", on page 224.

##### RF 50 Ohm

Output for RF signal.

NOTICE! Maximum Input Levels. Do not overload the RF output.

The instrument is equipped with a reverse power protection that prevents the RF output against back feed, see Chapter 4.3.5.7, "Reverse Power Protection", on page 167. Nevertheless, the maximum permissible reverse power is specified in the data sheet.

Depending on the equipped frequency option, the RF output connectors vary.


| RF Option |  | Microwave Option |  |
| --- | --- | --- | --- |
| RF 50 Ω |  | RF 50.52 |  |
| Frequency options | Connector type | Frequency options | Connector type |
| R&S SMB-B101 | N female | R&S SMB-B112/-112L | test port adapter, PC 3.5 mm female |
| R&S SMB-B102 |  | R&S SMB-B120/-B120L |  |
| R&S SMB-B103 |  | R&S SMB-B131 | test port adapter, PC 2.92 mm female |
| R&S SMB-B106 |  |  |  |

NOTICE! Risk of RF connector and cable damage. If you tighten the connectors too strongly, you can damage the cables and connectors. If you do not tighten the connectors enough, the measurement results can be inaccurate.

Always use an appropriate torque wrench suitable for this type of connector and apply the torque specified in the application note 1MA99.

The application notes are available on the Internet and provide additional information on care and handling of RF connectors.

Rohde & Schwarz offers appropriate torque wrenches for various connectors. For ordering information, see the R&S SMB data sheet or product brochure.

#### 3.2.2 Rear Panel Tour

This section gives an overview of connectors on the rear panel of the instrument. Each connector is briefly described and a reference is given to the chapters containing detailed information. For technical data of the connectors refer to the data sheet.


IEEE 488

USB IN
USB
LAN

STEREO

100 - 380 V AC / 10 - 600 M/14 - 6A

REF IN
REF OUT
EXT
PULSE
VIDEO
PULSE
TRIG
INST
SIGNAL
VALID
S/P DIF


Figure 3-2: Rear panel view


##### 3.2.2.1 Description of the Connectors


##### [Fuses]

The R&S SMB is fully fused by two fuses IEC60127-T3.15H/250 V.

The fuses are accommodated in the fuse holders next to the power connector. Use only fuses of the mentioned type.


##### [AC supply and power switch]

When the R&S SMB is connected to the AC supply, it automatically sets itself to the correct range for the applied voltage (range: see type label). There is no need to set the voltage manually or change fuses.

The power switch can be set to two positions:

##### • 0

The instrument is disconnected from the mains.

##### [I]

The instrument is power-supplied. It is either ready for operation (STANDBY) or in operating mode, depending on the position of the [ON/STANDBY] switch on the instrument front.

See also data sheet and Chapter 3.1.6, "Connecting to Power", on page 24.


USB IN


##### [USB IN]

USB (universal serial bus) interface of type B (device USB).

This interface can be used for remote control of the instrument.


USB
☐


##### [USB Connectors type A]

USB (universal serial bus) interfaces of type A (host USB).

• Connection of peripherals such as mouse, keyboard, etc.

• Connection of memory stick for file transmission

• Firmware update

See Chapter 3.1.9, "Connecting USB Devices", on page 25.


LAN


##### [LAN Connector]

##### Ethernet interface

• For integrating signal generators in a network

• Remote control of signal generator

• Remote access to the signal generator

• Firmware update

##### See also:

Chapter 3.1.7, "Connecting to LAN", on page 24

Chapter 5.1.3, "LAN Interface", on page 242


IEEE 488 IEC625


##### [IEC 625/IEEE 488]

IEC-bus (IEEE 488) interface for remote control of the instrument.

See also Chapter A.1, "GPIB Bus Interface", on page 508 and Chapter 5.1.6, "GPIB Interface (IEC/IEEE Bus Interface)", on page 246.

Note: In order to avoid electromagnetic interference (EMI) caused by open lines, always terminate any connected IEC-bus cable with an instrument or a controller.


STEREO
R
L


##### [Stereo R/L]

Inputs for analog stereo modulation signals. External modulation sources or the internal LF generator can be used (stereo modulation is available with option R&S SMB-B5).

See also Chapter 4.4.6, "Stereo Modulation", on page 217.


S/P DIF


##### [S/P DIF]

Input for digital stereo signals (stereo modulation is available with option R&S SMB-B5).

See also Chapter 4.4.6, "Stereo Modulation", on page 217.


SIGNAL VALID


##### [SIGNAL VALID]

Output of valid signal. This signal marks the valid signal times (valid level and frequency indication). The signal is generated automatically.


INSTR TRIG


##### [INSTR TRIG]

Input for external trigger for sweeps and list mode.

See also Chapter 4.3.7.4, "List Mode", on page 192 and Chapter 4.3.7.1, "Overview", on page 178.


PULSE
VIDEO


##### [PULSE VIDEO]

Output of internal pulse generator signal or external pulse signal fed in via the [PULSE EXT] connector (video signal).

See also Chapter 4.4.5, "Pulse Modulation (PM)", on page 214.


PULSE
EXT


##### [PULSE EXT]

Input of external pulse signal or input of external trigger/gate signal for internal pulse generator.

See also Chapter 4.4.5, "Pulse Modulation (PM)", on page 214.


REF OUT


##### [REF OUT]

Output of internal reference signal.

See also Chapter 4.3.4, "Reference Oscillator", on page 142.


REF IN


##### [REF IN]

Input for external reference signal.

See also Chapter 4.3.4, "Reference Oscillator", on page 142.

##### [OCXO]

• Oven-controlled reference oscillator (option R&S SMB-B1)

• high performance oven-controlled reference oscillator (option R&S SMB-B1H)


SMB-B1
OCXO


The OCXO generates a very precise 10 MHz reference signal. It needs some minutes of warm-up time to reach its nominal frequency.

Refer also to the data sheet for detailed information.

### 3.3 Trying out the Instrument

This section provides an example on how to configure the instrument for generating of an amplitude modulated signal.

##### Generation of an Amplitude-Modulated Signal

To generate a simple AM-modulated signal, proceed as follows:

1. Activate default (preset) state

Press the [PRESET] key to set a defined instrument state.


| Freq | RF OFF | MOD OFF | Level |
| --- | --- | --- | --- |
| 1.000 000 000 0 | GHz | -30.00 | dBm |
| ALC-S&H |  |  | Info |
| Mod Gen Modulation RF config... config... □ On □ On □ On |  |  |  |

2. Select and activate AM modulation

a) Turn the rotary knob and select the "Modulation" block.


| Modulation |
| --- |
| config... |
| ☐ On |

b) Press the rotary knob to open the dialog where the modulation can be selected.

Note: Different modulation modes are available depending on the options installed.


Modulation
Amplitude Modulation...
Frequency Modulation...
Phase Modulation...
Pulse Modulation...


The "Amplitude Mod..." menu is the first menu and is highlighted per default.

I've "Amplitude Mod..." menu is the first menu and is highlighted per default.

c) Turn the rotary knob and highlight "Amplitude Mod...".

Press the rotary knob to open the "Amplitude Modulation" dialog.


| Amplitude Modulation | ☐× |
| --- | --- |
| State | Off |
| AM Source | Int |
| AM Depth | 30.0 % |
| AM Source = INTernal |  |
| LFGen Freq | 1.000 00 kHz |

d) Turn the rotary knob to select parameter "AM Depth", press the rotary knob to allow editing and enter the preferred AM depth with the aid of the numeric keypad and the unit keys.


| State | Off |
| --- | --- |
| AM Source | Int |
| AM Depth | 20 % |
| AM Source = INTernal |  |
| LFGen Freq | 1.000 00 kHz |

e) Finally, select "State" and press the rotary knob to switch on the AM modulation.


| Amplitude Modulation | ☐☒ |
| --- | --- |
| State | On |
| AM Source | Int |
| AM Depth | 20.0 % |
| AM Source = INTernal |  |
| LFGen Freq | 1.000 00 kHz |

To indicate the active state, the "Modulation" block is displayed in blue. The "RF" is not yet active, which means that no RF signal is output.

f) Press the [DIAGRAM] key to display the complete block diagram.

3. Set frequency and level and activate RF signal

a) Press the [FREQ] key to activate the editing mode for frequency entry. The "Frequency" entry field in the header section of the display is highlighted.


| Freq | RF OFF | MOD ON |
| --- | --- | --- |
| 1.000 000 000 00 | GHz |  |

Enter the frequency using the numeric keypad and terminate the entry by pressing a unit key.

b) Press the [LEVEL] key and enter the level settings in the same way.


Level
-30.00 dBm


c) Press the [DIAGRAM] key to display the complete block diagram.

d) Turn the rotary knob to select the "RF" block.

Press the [RF ON/OFF] key to activate the "RF" block.

The AM modulation signal is now present at the RF output.


Freq Level
363.000 000 00 MHz -11.00 dBm
LfSweep, ALC-S&H Info
Mod Gen Modulation RF
config... config... config... RF
On On On
Swp AM


##### Generation of an RF Frequency Sweep Signal

In the example, an RF frequency sweep is configured. Proceed as follow:

1. Activate default (preset) state

Press the [PRESET] key to set a defined instrument state.


| Freq | RF OFF | MOD OFF | Level |
| --- | --- | --- | --- |
| 1.000 000 000 0 | GHz | -30.00 dBm |  |
| ALC-S&H |  |  | Info |


| Mod Gen | Modulation | RF |
| --- | --- | --- |
| config... | config... | config... |
| ∫ On | ∫ On | ∫ On |

2. Configure and activate RF frequency sweep.

a) Turn the rotary knob and select the "RF" block.


| RF |
| --- |
| config... |
| ☐ On |

b) Press the rotary knob to open the dialog where the RF frequency sweep can be selected.

RF Sweep / List
Frequency Sweep...
Level Sweep...
List Mode...

c) Turn the rotary knob and highlight "RF Frequency Sweep…". Press the rotary knob to open the "RF Frequency Sweep" dialog.


| RF Frequency Sweep |  |  |  |  |
| --- | --- | --- | --- | --- |
| State |  | Off |  |  |
| Mode |  | Auto |  |  |
| Start Freq |  | 100.000 000 000 | MHz |  |
| Stop Freq |  | 500.000 000 000 | MHz |  |
| Center Freq |  | 300.000 000 000 | MHz |  |
| Span |  | 400.000 000 000 | MHz |  |

All sweep parameters are default except for the sweep mode. The default settings are not changed.

d) Turn the rotary knob to select parameter "Mode", press the rotary knob to open the selection list and select "Single".


| RF Frequency Sweep |  |
| --- | --- |

e) Press the rotary knob to apply the selection.

For triggering, the "Execute Single Sweep" and "Reset Sweep" buttons are displayed.

f) Finally, select "State" and press the rotary knob to switch on the RF frequency sweep.


| RF Frequency Sweep |  |
| --- | --- |

g) Press the [DIAGRAM] key to display the complete block diagram. The "RF" is not yet active, which means that no RF signal is output.

3. Activate RF signal.

a) Turn the rotary knob to select the "RF" block.

b) Press the [RF ON/OFF] key to activate the "RF" signal output.


| RF |
| --- |
| config... |
| On |
| Sweep |

To indicate the active state, the RF block is displayed in blue. An RF signal with the default frequency and level settings is output, i.e. 1 GHz and -30 dBm. The sweep is not yet active, it must be triggered in the sweep dialog.

4. Trigger RF frequency sweep

a) Press the [Winbar] key to switch to the "RF Frequency Sweep" dialog. Turn the rotary knob to select the "Execute Single Sweep" button.


| RF Frequency Sweep |  |  |  |
| --- | --- | --- | --- |
| State | On |  |  |
| Mode | Single |  |  |
| Execute Single Sweep |  |  |  |
| Reset Sweep |  |  |  |
| Start Freq | 100.000 000 000 | MHz |  |

Press the rotary knob to trigger (start) the frequency sweep.

A linear single sweep signal is now present at the RF output, starting at 100 MHz. The sweep is processed in 1 MHz steps with dwell time of 10 ms per step up to the stop frequency of 500 MHz.

The sweep starts at 100 MHz, stops at 500 MHz in 1 MHz steps is output with a dwell time of 10 ms per step.

### 3.4 System Overview

This section helps you to get familiar with the R&S SMB. It provides an introduction to the general concept of the instrument with a sample of the possible application fields. It also describes the main blocks in the signal generation flow.

For information on how to access functions and interact with the R&S SMB, refer to Chapter 3.5, "Instrument Control", on page 66.

#### 3.4.1 Brief Introduction to the Instrument's Concept

The R&S SMB is a high-performance signal generator developed to meet demanding customer requirements. Offering excellent signal characteristic and straightforward and intuitive operation, the signal generator makes signal generation fast and easy.

#### 3.4.2 Signal Flow at a Glance

The R&S SMB is equipped with an intuitive user interface. The central element of the display is the block diagram that shows the signal flow and processing from the left on the display to most right, i.e. the generated signal can be seen at a glance.

Each block represents a functional unit of the instrument. Thus you always know the position at which a parameter affects the signal flow. The main settings of a block are indicated in the block. The interconnection of employed inputs and outputs is also shown. The user is thus always informed about the connection of inputs and outputs in the signal flow and where they can be configured. A window is opened for each menu where parameters can be set. When the window is opened, an entry is made in the "Winbar" below the display. All open menus are of equal priority (not modal) and can be accessed any time.

The block diagram in the figure below shows a fully equipped instrument.


Freq Level
363.000 000 00 MHz -11.00 dBm
LfSweep, ALC-S&H Info
Mod Gen Modulation RF
config... config... config... RF
On On On On
Swp AM


Figure 3-3: Block diagram of a fully equipped R&S SMB


With the rotary knob, you can navigate in the block diagram and the dialogs, and operate the instrument with one hand. The cursor is moved line by line through the block diagram or dialog. Turning the button clockwise advances the cursor. The selected

block can be activated or deactivated with the [TOGGLE] key. Active blocks are highlighted by a colored background.

The instrument comprises a comprehensive info and help system. You can access the context-sensitive help with the [HELP] ([F1]) key at any time. The help system indicates the currently selected parameter and offers additional services such as cross references, index and contents. The content of the help system corresponds to the operating manual of the instrument.

Warning and conflict messages caused by incorrect operation as well as further information are displayed in the "Info" line. A complete list of existing conflicts is displayed when the [INFO] ([CTRL+I]) key is pressed. Additional information on entries can be requested from the help system. The history function permits display of all messages.

Assistants simplify the completion of tables. After data entry in the assistant, the table is modified only after the "Accept" button has been pressed. Pressing the "Accept" button also stores the assistant data.

See Chapter 3.5, "Instrument Control", on page 66 for an overview on how to work with the instrument.

For an in-depth description of the dialog boxes and the instrument functions, refer to section Chapter 4.1, "Overview of Instrument Functions", on page 94.

#### 3.4.3 Application Field of the Instrument

The main field of application of the R&S SMB is the generation of sine wave signals with very high spectral purity. These signals are needed e.g. for adjacent channel or phase noise measurements. In addition, the RF signal can be modulated with the internal modulations waveforms sine waves and rectangular signals.

#### 3.4.4 Description of Individual Diagram Blocks

The signal path of the instrument is configured by installing a frequency option that comprises all required modules.


One of the following options must be installed.

• R&S SMB-B101 (up to 1.1 GHz)

• R&S SMB-B102 (up to 2.2 GHz)

R&S SMB-B103 (up to 3.2 GHz)

• R&S SMB-B106 (up to 6 GHz)

R&S SMB-B112 (up to 12.75 GHz)

R&S SMB-B112L (up to 12.75 GHz without attenuator)

• R&S SMB-B120 (up to 20 GHz)

R&S SMB-B120L (up to 20 GHz without attenuator)

R&S SMB-B131 (up to 31.8 GHz)

R&S SMB-B140/-B140N (up to 40 GHz)

R&S SMB-B140L (up to 40 GHz without attenuator)

Instruments without step attenuator provide a restricted level range at the RF output. Refer to the data sheet for detailed information.

You can additionally get the following options for microwave instruments:

• up to 20 GHz (R&S SMB-B120/-B120L)

– high output power option R&S SMB-B31

— low harmonic filter option R&S SMB-B25

• up to 40 GHz (R&S SMB-B140/-B140L/-B140N)

– high output power option R&S SMB-B32

– low harmonic filter option R&S SMB-B26

See data sheet for detailed information.

Up-to-date information is available at R&S SMB homepage on the internet http://www.rohde-schwarz.com/product/smb100a.html.


Mod Gen config... On


##### Mod Gen block

The internal modulation sources are configured in this block. Also, the "LF frequency sweep" can be activated here.

The internal modulation signals are provided at the LF output at the front of the instrument. The LF output signal and the modulations sources for the analog modulations AM, FM and PhiM can be selected independently from each other.

An internal LF generator is available as the internal source for the analog modulations AM, FM and PHiM. Available modulation shapes are sine and rectangle.

A pulse generator provides single and double pulse modulation with selectable pulse widths and periods. Additionally, an option is available to generate pulse train signals.

The R&S SMB offers three different sweep types (frequency sweep, level sweep and LF sweep) to be activated alternatively. Each type has 6 modes which differ with respect to the sweep cycle mode (continuous, individual and step-by-step) and triggering mode (automatic, internal and external). In the "Mod Gen" block, the LF sweep is configured. Frequency and level sweep settings are accessed via the "RF" block.

The status display in the block shows whether LF generator and/or a sweep are active. The selected internal LF generator and/or noise source are switched on or off with the [TOGGLE ON/OFF] key.


Modulation config... On


##### Modulation block

The internal and external analog modulations are configured and activated in this block. The [MOD ON/OFF] key switches the active modulation(s) on/off.

The internal modulation sources are configured in the "Mod Gen" block. External amplitude, frequency or phase modulation signals can be fed in at the input connector [MOD EXT] at the front of the instrument. An external pulse signal is fed in via the BNC connector [PULSE EXT] at the rear of the instrument. AC or DC coupling for external feed is possible.

Modulation signals of up to two sources (internal and external source) can be combined for AM/FM and PhiM modulation.

Available internal and external analog modulation modes are:

• Amplitude modulation (AM)

• Frequency modulation (FM)

• Phase modulation (PhiM)

• Pulse modulation (Pulse)

• Stereo modulation (Stereo)

Note: For modulation modes that can be simultaneously used, refer to the R&S SMB data sheet.

The status display in the block shows the active modulation(s). Use the [TOGGLE ON/OFF] key to switch the active modulation of the block on or off.


RF
config...
□ On


##### RF block

In this block, the RF parameters and frequency/level sweep settings are set.

The active sweep is displayed in the block. The [RF ON/OFF] key switches the RF signal on and off. When the signal is switched off, the switch before the RF output symbol is open.

RF settings include:

• Frequency and reference frequency

• Level settings: if required.

• NRP Power Viewer using power sensors

• Frequency and level sweep

- List Mode settings. In this mode, extremely fast frequency and level settings can be made.

The [RF 50 Ohm] output connector at the front of the instrument provides the RF signal. An external trigger/gate signal for sweeps is input via the [INST TRIG] connector at the rear of the instrument

Note: Frequency and level are set fast with the aid of the [FREQ] and [LEVEL] keys.

Use the [TOGGLE ON/OFF] key to switch the RF output on or off.

### 3.5 Instrument Control

The operating concept of the R&S SMB employs the following three ways of instrument control:

• Manual operation

• Remote control

• Remote access

#### 3.5.1 Manual Operation

The R&S SMB can be operated intuitively either via the interactive block diagram or via a menu tree. All menus are in the form of windows that can be operated in the same way. Rotary knob, keys and softkeys, or alternatively a mouse, allow direct and therefore convenient access to entries and settings.

The display shows the current signal generator state. The embedded online help functions supports you in signal configuration.

This section describes the concept of manual operation of the signal generator. It includes a description of the general structure of a dialog box, working with dialog boxes and the block diagram and the setting of parameters.

For an in-depth description of the instrument functions and dialog boxes, refer to Chapter 4, "Instrument Function", on page 94.

##### 3.5.1.1 Legend of Manual Controls

The following table lists all available key combinations used on the PC keyboard to trigger functions on the instrument. Keyboard labels are described in alphabetical order.

In addition, a front panel key emulation and an on-screen keyboard can be used for manual operation by mouse only.

Table 3-3: Overview of the keyboard key combinations and their function


| Key of PC keyboard | Short description | Function |
| --- | --- | --- |
| ./*...# | Special characters | Enters a special character, e.g. period/decimal point. |
| +/- | Sign | Enters the sign. |
| - / (shift+) a—z | A<->a | Switches between upper-case and lower-case letters. |
| ALT + F9 | Units G/n / dBuV / dBu | Selects the unit Giga/Nano, dBuV for the RF level and dBu for the LF level. |
| ALT + F10 | Units M/u / uV | Selects the units Mega/Micro and uV for RF levels. |
| ALT + F11 | Units k/m / uV | Selects the units Kilo/Milli and uV for RF levels. |
| ALT + F12 | Enter function Unit dBm | Confirms entries in the base unit and values without a unit. Selects dBm for the RF level and dB for level offset and level step width. |
| Arrow keys | Cursor | Moves the cursor. |
| Backspace | Clears entry | Clears the last entry (number, sign or decimal point) |
| CTRL + A | Rearrange | Arranges open dialogs automatically |
| CTRL + C | Recalculate | Starts recalculation of the signal. |
| CTRL + D | Block diagram | Sets the cursor on the block diagram and hides all menus. |
| CTRL + E | Setup menu | Opens the setup menu for general instrument settings. |
| CTRL + F1 - F8 | Softkey 1 - 8 | Triggers the function assigned to the soft-key. |
| CTRL + F | Frequency | Activates the frequency entry. |
| CTRL + G | Closes menus | Closes an active menu. |
| CTRL + H | Hide | Minimizes the active menu. Pressing the respective button in the Winbar opens the menu again. |
| CTRL + I | Info | Opens/closes the info window |
| CTRL + L | Level | Activates the level entry. |
| CTRL + M | Menu | Calls the menu selection list. |
| CTRL + O | Modulation on/off | Switches modulation on/off. "MOD OFF" is indicated in the status line. |
| CTRL + P | Preset | Restores a defined basic instrument setup. |
| CTRL + Q | Local | Switches the instrument from remote control to manual control. |
| CTRL + R | RF on/off | Switches the RF output signal on/off. "RF OFF" is indicated in the status line. Both RF output signals are always deactivated in the case of two-path instruments. |
| CTRL + S | Storage under Windows | Activates the menu for storing instrument settings. |
| CTRL + T | Toggle on/off | Switches a block or parameter on/off. Toggles between the different possibilities of setting a selection parameter. |
| CTRL + Y | Hardcopy | Opens the menu for configuring and starting a hardcopy. |
| CTRL + W | Winbar | Displays the Winbar in the foreground/background. |
| Enter | Enter | Terminates an entry. |
| ESC | ESC | Selects the next higher menu/selection level. When the editing mode is exited with ESC, the previous value is restored. |
| F1 | Help | Opens/closes context-sensitive help. |
| Ins | Insert | Activates the insert mode. |


##### Front Panel Key Emulation

The R&S SMB provides a front panel key emulation to enable execution of the front panel key functions by mouse e.g. for remote access. The emulation is called on the external monitor by a right mouse click. The front panel key functions are executed by a mouse click on the associated button.


| Freq | Diagram |
| --- | --- |
| Level | Menu |
| File | Rearr. |
| RF On/Off | WinBar |
| Mod On/Off | Help |
| Preset |  |
| Local |  |
| Setup |  |
| Hardcopy |  |
| Info |  |

##### On-screen Keyboard

In addition, the Windows XP operating system provides a keyboard emulation that can be used for system settings if no external keyboard but a mouse is available.

To access the on-Screen Keyboard, select "Start > Programs > Accessories > Accessibility > On-Screen Keyboard".


| File | Keyboard | Settings | Help |  |
| --- | --- | --- | --- | --- |
| esc | F1 | F2 | F3 | F4 |
| 1 | 2 | 3 | 4 | 5 |
| tab | q | w | e | r |
| lock | a | s | d | f |
| shift | y | x | c | y |
| ctrl | alt |  |  |  |

##### 3.5.1.2 Key Elements

The manual operating concept of the R&S SMB enables the user to make settings as intuitively as possible and at the same time gives a permanent overview of characteristics of the generated signal and of the current instrument state. Numerous online help functions support user settings.

##### Block diagram

The block diagram is the core of the operating concept.

A graphics display shows the current configuration and the signal flow in the form of a block diagram. All graphical elements can be accessed for operation. An element is selected by means of the arrow keys and the associated setting function is called by pressing Enter. Required menus and graphs are displayed on the block diagram which is displayed again in the foreground whenever the [DIAGRAM] ([CTRL+D]) key is pressed.

The main characteristics of the RF signal, frequency and level, are permanently displayed in the header section of the screen and can be directly set in the display fields after the [FREQ] (CTRL+F) or [LEVEL] (CTRL+L) key is pressed. Status messages for the output signal are displayed in addition to frequency and level.


Freq RF ON MOD ON Level
1.000 000 000 000 GHz -30.00 dBm


##### Operation via Graphical User Interface

##### • Functional blocks

Menus are assigned to the specific function blocks in the block diagram. The function blocks represent elements of signal generation. Function blocks displayed with a blue frame can be directly switched on and off by means of the [TOGGLE ON/OFF] (CTRL+T) key. The menus of the highlighted function blocks can be called by pressing the [ENTER] key.

##### Example:

The "Modulation" block contains all menus required for modulation signal configuration.


Modulation
config...
On
PuM


In this block all modulations can be selected.


Modulation
Amplitude Modulation
Frequency Modulation...
Phase Modulation...
Pulse Modulation...


##### Signal flow

The signal flow between the function blocks and the employed inputs and outputs are also shown.


LF MOD
Mod Gen Modulation RF
config... config... config... RF
On On On On
LF AM Sweep


##### • Menu tree

The menu tree can be opened and closed with the [MENU] (CTRL+M) key. The menu tree is organized in the same way as the directories under Windows. The function blocks correspond to the first directory level, the menus to subdirectories.

Mod Gen
Modulation Generator -
LF Generator / Output...
LF Frequency Sweep...
Pulse Generator...

##### Operation corresponds to the Windows concept

To offer the user a familiar environment, operation is very similar to operation of Windows user interfaces. All menus and tables are made up of known elements, such as selection lists, check boxes and entry fields.

A blue frame indicates that the selected item is active. In the highlighted element, entries can be made.


##### Rotary knob

Operation is possible via front-panel keys, an external keyboard and the mouse. However, most of the settings can be easily made with the rotary knob:

- Turning the rotary knob shifts the entry focus to the target element.


- Pressing the rotary knob activates the selected entry field.

Depending on the parameter, the submenu is called, the numeric value varied, the list entry selected or the check box activated or deactivated.

- If a value is entered, the entry is stored by another click on the rotary knob and the editing mode is exited.

##### Settings in subdialogs

A separate window is opened for each dialog and subdialog. The dialogs can be operated independently of each other, i.e. none of the dialogs requires that settings in other dialogs be completed before it can be closed. This ensures flexible operation at all times.

##### Keys with assigned simple functions

Most keys on the front panel of the R&S SMB directly perform a simple function.

Since a great number of settings can thus be made by a keystroke, operation is easy. For instance, the [CLOSE] (ESC) key closes the active menu; with the [RF ON/OFF] (CTRL+R) key the RF output signal can be switched on or off.

An exception are keys that call a menu such as the [MENU] (CTRL+M) key which opens the complete menu tree of the instrument, the [SETUP] (CTRL+E) key which opens the menus for general instrument settings or the [FILE] (CTRL+S) key which opens the menu for file management.

##### Help functions for user support

Numerous help functions support the user in signal configuration.

##### • Value ranges

The valid setting range is displayed for each numeric parameter. This requires a short wait after activation of the entry field. The range is then displayed automatically after a few seconds.


| Phase Settings |  |  |
| --- | --- | --- |
| Delta Phase | 0.0 | deg |
| Reset Delta Phase | Min = -720.0 degMax = 720.0 deg | 0.0 |
|  | insert mode |  |

If the entered value is outside the permissible range, the next permissible value is automatically set and a message is output.

##### • Context-sensitive help

Context-sensitive help for each parameter can be called with the [HELP] or [F1] key.

##### • Comprehensive online help

Each help page is part of a comprehensive online help function which can be called by means of an index, a content tree or the "Previous/Next" buttons.

##### Info line with messages for indication of the current instrument state

A great variety of different messages such as status messages, error messages, warnings or information are displayed in the header field of the screen. With the aid of the [INFO] (CTRL+I) key, help pages can be called for most of the messages. They provide background information on the message and indicate operating steps that may be required. All messages are explained in the online help which can be called with the [HELP] (F1) key.


Freq RF ON MOD OFF Level
1.000 000 000 000 GHz -30.00 dBm
ALC-Auto Info


##### 3.5.1.3 Display

The display shows the current signal generator state and offers graphical elements for direct operation. It is divided into three sections:

- The frequency and level display with info line indicates the main output signal parameters and reports the current state with status, error and warning messages.

- The block diagram shows the instrument configuration, the signal characteristic as well as the inputs and outputs used and permits interactive operation via graphics elements. Active menus and graphs are displayed on top of the block diagram.

Winbar with labeled softkeys for menu display.

The block diagram in the figure below shows a fully equipped instrument.


Freq Level
1.000 000 000 0 GHz -30.00 dBm
ALC-S&H Info
Mod Gen Modulation RF
config... config... config... RF
On On On On
A/F/PuM


Figure 3-4: Block diagram of a fully equipped R&S SMB


##### Settings Displayed in the Header Section

Frequency/level settings and a few status messages (see "Status Information and Messages" on page 72) are displayed in the header field of the screen. The display may vary depending on the instrument's operating mode:

- In the sweep mode, the current frequency or level of the output signal is displayed. The status message "SweepMode" is displayed in the info line.

- In the list mode, neither the current frequency nor level is displayed, the indication is dimmed.

• If user correction is active, the status message "UCorr" is displayed in the info line.


The values displayed in the "Freq" and "Level" fields include a set offset or multiplier factor.

For more See also Chapter 4.3.2, "RF Frequency", on page 138 and Chapter 4.3.5.1, "Overview of RF Level", on page 146.

The frequency and level indication can be enlarged so that it covers the complete display of the R&S SMB by using the [DIAGR] key. This key toggles between block diagram, magnified frequency and level indication and the display of the active dialog. This requires the "Summary Screen Toggle" to be enabled.

See also "Toggle Summary Screen" on page 108.

##### Status Information and Messages

The instrument indicates status information and messages in the header section of the screen. The messages differ with respect to their importance (errors, warnings, info) and the time of their appearance (brief and permanent messages), and require different treatment

For additional information refer to the info window (see "Info Window" on page 73).

Refer to Chapter 9, "Status Information, Error Messages and Troubleshooting", on page 499 for an overview of all status information and messages and corrective actions.

##### Status Information

The status information gives the user an overview of the main operating states and settings of the instrument. The states are indicated for information only and do not necessitate any action by the user.

Status information is displayed between the frequency and level fields, at the left of the info line or in the info line itself.

##### Messages

Messages indicate errors in the instrument. They are displayed in the info line in different colors depending on their importance and display duration. Errors (e.g. no calibration data) are displayed in red, information (e.g. file not found) and warnings in black. Warnings indicate less significant errors (e.g. the instrument operates outside specified data).

##### Volatile messages

Brief messages report automatic settings in the instrument (e.g. switching off of incompatible types of modulation) or on illegal entries that are not accepted by the instrument (e.g. range violations). They are displayed in the info line on a yellow background. They are displayed on top of status information or permanent messages.

Volatile messages do not normally demand user actions and disappear automatically after a brief period of time. They are stored in the history, however.

These messages can be read from remote using the commands :SYSTEM:ERROR[:NEXT]? and :SYSTEM:ERROR:ALL?.

##### Permanent Messages

Permanent messages are displayed if an error occurs that impairs further instrument operation, e.g. a hardware fault. The error signalled by a permanent message must be eliminated before correct instrument operation can be ensured.

The message is displayed until the error is eliminated. It covers the status display in the info line. After error elimination, the message automatically disappears and is also recorded in the history.

These messages can be read from remote using the command :SYSTEM:ERROR:STATIC?.

##### Info Window

A few operating states and the current message are displayed in the info line.

The [Info] window with a list of current permanent messages and a detailed description of each message can be opened with the [INFO] (CTRL+I) key.


A: UCorr, ALC-Auto | B: ALC-Auto

- No pending errors.

Delete
Delete all
Del. volatile

Info
Info line

List of current messages with short message text.

Detailed description for highlighted message

Show Static


The upper section of the info window contains a list of all current permanent messages in the order of their occurrence, i.e. the most recent message is displayed first. In the lower section of the window, additional information on the highlighted message is displayed. A history of all messages that have occurred since instrument switch-on can be called with the "History" key. The most recent message is displayed first.

The messages are color-coded according to their level. Device-specific messages are red, info and remote control error are black. The level is also indicated in the "Lev" column (Err, Sys or Info). Column "SCPI" indicates the SCPI error code.

With the aid of the softkey buttons, error messages can be cleared and a history of all messages called.

##### Delete

Clears the highlighted message.

This button is available only if the history of the messages is displayed.

##### Delete All

Clears all messages.

This button is available only if the history of the messages is displayed.

##### Remote command:

:SYSTEM:ERROR:HISTORY:CLEAR on page 441

##### History

Calls the list of all messages that have occurred since instrument switch-on. The most recent messages are displayed at the top of the list. When the button is pressed again, the list of current messages is displayed.

##### Remote command:

:SYSTEM:ERROR[:NEXT] ? on page 440 or :STATUS:QUEue[:NEXT] ? on page 436

:SYSTEM:ERROR:HISTory? on page 440

Each time a SYST:ERR? or STAT:QUE? query is sent, the oldest entry in the error queue is returned and at the same time cleared in the list.

##### Block Diagram

The block diagram shows provided options, signal configuration and the currently selected signal flow of the generator with inputs and outputs used. Signal generation can be completely operated from the block diagram. The highlighted function block can be directly switched on and off with the [TOGGLE ON/OFF] (CTRL+T) key. Pressing the Enter opens the associated setting menu.


LF MOD

Mod Gen Modulation RF
config... config... config... RF
On On On
LF AM Sweep


Figure 3-5: Block diagram of the R&S SMB


##### Function Blocks in the Block Diagram

Each block represents a function of signal generation. The function is indicated in the headline of the block. In the check box, the respective function can be quickly activated/ deactivated with the [TOGGLE ON/OFF] (CTRL+T) key. After activation, the block is displayed in blue. Status information is displayed below the check box. It is different for the different blocks.

Pressing the rotary knob (front panel) or the "Config..." button (mouse) opens the associated setting menu.

##### Example: Modulation block


Modulation

config...

On

PuM


In this block, the modulation signals are set. The status information of the "Modulation" block indicates the selected modulation.

##### Signal Flow and Input/Output Symbols in the Block Diagram

The input/output symbols in the block diagram show the currently used inputs and outputs of the signal generator. Unused inputs and outputs are not shown. The lines indicate the signal flow.

Symbols and labels refer to the corresponding inputs and outputs on the front and rear panel of the signal generator. The direction - input or output - is indicated by an arrow.

##### Example:


PULSE
Modulation
config...
On
PuM FM


The symbols indicate the input for the external pulse signal on the instrument rear panel.

##### Structure of the Dialogs

The parameters are set in the menus. Menus are accessed either via the function blocks in the diagram or by means of the [MENU] (CTRL+M) key. The menus are displayed on top of the block diagram.

The [WINBAR] key toggles between the active menus. The [REARR] key toggles between the enlarged and normal sized menus.

Working with menus and dialog boxes is described in Chapter 3.5.1.4, "Accessing Di-logs", on page 76; the setting of parameters in Chapter 3.5.1.5, "Setting Parameters", on page 77.

The menus are in Windows format. The menus differ in details depending on their function but they consist of the same main elements. Each menu consists of a menu header and one or more menu areas with various fields for setting parameters.

The header line contains the name of the menu and the buttons for minimizing and closing the menu. To operate the buttons, use the mouse or the front key [CLOSE] (ESC).

Several fields of associated but separately set parameters are organized in menu areas. A menu area is framed and labelled with the function common to all parameters.

Example: Pulse Generator


| Pulse Generator |  |  |
| --- | --- | --- |
| Pulse Mode | Single | ▼ |
| Pulse Period | 1.000 0 ms | ▼ |
| Pulse Width | 500.0 $ \mu $s | ▼ |

Each of the setting fields is assigned a parameter name. The kind of setting varies depending on the parameter to be set. Some settings can only be made in a specific configuration. If setting is not permitted with the specific configuration selected, the respective item is disabled and displayed in gray and the entry or selection field cannot be accessed.

##### 3.5.1.4 Accessing Dialogs

The [MENU] (CTRL+M) key opens the complete menu tree. Selecting a functional block and pressing the [Enter] key opens the menu associated with this block.

An alternatively way to access a dialog is to use the [CLOSE] (ESC), [DIAGRAM] (CTRL+D) and [REARR] (CTRL+A) keys on the front panel.

For a quick access to the dialogs, use one of the following alternative methods.

Displaying the block diagram or a dialog in the foreground

▶ Press the [DIAGRAM] (CTRL+D) key to move the cursor to the block diagram. All active menus are minimized.

##### Enlarging the indication of the header section

▶ Press the [DIAGRAM] key twice to enlarge the indication of the header section.

Tip: Use the [REARR] key to toggle between the enlarged and normal sized dialogs.

##### Accessing the menu tree

▶ Press the [MENU] (CTRL+M) key to open the complete menu tree.

##### Calling the File or Setup dialog

▶ Use the [FILE] (CTRL+S) or [SETUP] (CTRL+E) keys to open the respective dialog.

##### Closing an active menu

▶ Press the [CLOSE] key to close an active menu.

Tip: If the cursor is at the highest menu level, you can also use the [ESC] key to close the active menu.

##### Accessing the header area

▶ Press the [FREQ] (CTRL+F) and [LEVEL] (CTRL+L) keys to activate the "Frequency" or "Level" entry fields in the header area.


##### Keyboard Shortcuts

Keyboard shortcuts (e.g. "Ctrl + D" for the displaying the block diagram in the foreground) provide direct access to all utility dialogs of the instrument (see Chapter 3.5.1.9, "Legend of Front-Panel Controls", on page 90).

##### 3.5.1.5 Setting Parameters

The R&S SMB offers several and sometimes alternative possibilities for setting parameters. Operation is possible from the front panel, with the aid of a mouse and/or from a PC keyboard.


The examples whitin this description focus on the operation from the front panel.


For more information, refer to:

Chapter 3.5.1.9, "Legend of Front-Panel Controls", on page 90 for an overview of key functions and a cross-reference between the front panel keys and the keyboard shortcuts

• section "Instrument Functions" for a detailed description of key functions.

Most of the parameters are set in the different menus. The R&S SMB provides alternative ways for accessing the dialogs. Turn the rotary knob and navigate to the corre-

sponding block in the block diagram and press the knob to open the dialog or perform a mouse click on the "Config..." button.

An exception are the "Setup" and "File" dialogs. In the "Setup" dialog, general settings are made which are not directly concerned with signal generation, e.g. setting of the GPIB-bus address. In the "File" dialog, files and lists are managed.

These menus can only be called with the [SETUP] (CTRL+E) and [FILE] (CTRL+S) keys.

Frequency and level are directly set in the header area of the display using the [FREQ] and [LEVEL] keys.

Specific settings can also be made directly in the block diagram, e.g. activating a function block by means of the [TOGGLE ON/OFF] (CTRL+T) key or switching the RF output on and off with the aid of the [RF ON/OFF] (CTRL+R) key. Changes affecting the signal flow are immediately visible in the graphics display.

This section provides an information about the parameter handling that comprises of the following main steps:

• "Working with the Cursor" on page 78

"Selecting a Control Element" on page 79

"Switching Parameters On/Off" on page 79

"Entering a Value" on page 79

• "Working with Units" on page 81

"Selecting a Value from a List" on page 81

• "Terminating Entries with Confirmation" on page 82

● "Restoring the Previous Value" on page 82

##### Working with the Cursor

After the instrument is switched on, the cursor is always on the first function block of the diagram (default setting).

##### Moving the cursor on the display

▶ To move the cursor, use one of the following alternative methods:

a) Use the rotary knob or the arrow keys.

b) Use the [Winbar] key to toggle between the active dialogs.

c) Use the [ESC] key.

Tip: Be aware that the function of the [ESC] key depends on the current cursor position.

The function of this key depends on the current cursor position.

• Calls the next higher selection level.

- Closes the open window without accepting new entries; the old value or parameter is retained.

• In dialog boxes that contain a "Cancel" button it activates that button.

- Closes all kinds of dialog boxes, if the edit mode is not active.

• Quits the edit mode, if the edit mode is active.

• Switches between different entry fields of a menu.

- Shifts the entry cursor from the header display to the previously active menu, or to the previously highlighted block in the block diagram if no menu is active.

##### Moving the courstor to the heading area

▶ Press the [FREQ] (CTRL+F) or [LEVEL] (CTRL+L) key to move the cursor to the header area.

##### Selecting a Control Element

Control elements are always selected in the same way no matter whether a function block in the diagram, a menu in the menu tree, a parameter in the menu or an entry in a list or table is concerned.

▶ To activate an element, put the cursor on it.


| Source | Internal |
| --- | --- |

An active element is highlighted by a blue frame.

##### Switching Parameters On/Off

A parameter can be activated and deactivated using a button or a check box.

1. Select the parameter.

2. To change the state of a parameter, use the "Enter" function of the different control media:

• Press the rotary knob

• Press [ENTER]

• Press the [TOGGLE ON OFF] (CTRL+T) key.

Colour and label of a button change, the check box is ticked or the tick is removed.

##### Entering a Value

Numeric and alphanumeric values can be edited in the entry fields. In the editing mode, cursors of different colour are used. A blue cursor indicates the overwrite mode, a green cursor the insert mode.

Numeric and alphanumeric values can either be newly entered or the existing value can be changed. Incorrect entries are cleared with the [BACKSPACE] key.

##### Entering a new numerical value

1. Select the parameter.

2. Press a numeric key to activate the editing mode.

The previous value is cleared and the new value can be entered.

##### Editing a value in the insert mode (default setting)

1. Press the rotary knob (= Enter) to activate the editing mode.

If the cursor is placed at the right of the total value, the insert mode is always active.

2. Set the cursor to the left of the number to be changed using the [Left/Right] arrow keys.

The cursor is displayed in green.

3. Click on a numeric key to insert a new value.


| Dwell Time | 10.000 ms | Dwell Time | 6 ms |
| --- | --- | --- | --- |

##### Editing a value in the overwrite mode

1. Activate the editing mode.

2. Set the cursor on the numeric digit to be changed using the [Left/Right] arrow keys. The cursor is displayed in blue and the number to be replaced is highlighted.

3. Click on a numeric key to overwrite the highlighted value.


| Dwell Time | 10.000 ms | Dwell Time | 10.000 ms |
| --- | --- | --- | --- |

##### Varying a value

1. Activate the editing mode.

2. Set the cursor to the left of the number to be changed using the [Left/Right] arrow keys.

The value at the cursor position is varied.

3. To vary the selected value, use the [Up/Down] arrow key or turn the rotary knob. The value is increased or decreased.

##### Entering a new alphanumerical value

1. Select the parameter.

2. Press an alphanumeric key to start the editing mode.

The new value is entered.

##### Editing an alphanumerical value

An existing value, e.g. a file name, can be changed in the insert mode (see example) or in the overwrite mode.

1. Select the parameter and activate the editing mode.

2. Set the cursor to the left of the alphanumerical value using the [Left/Right] arrow keys.

Tip: If hexadecimal values are to be entered, the numeric front-panel keys are automatically changed to hexadecimal values.

3. Click on an alphanumeric key to insert a new alphanumeric value.

##### Terminating the entry of a numeric value

To terminate the entry of a numeric value:

1. Press the rotary knob (= Enter).

2. Press a [UNIT] key on the front panel.

3. Select a "Unit" in the selection field next to the parameter value.

##### Working with Units

The unit of a parameter is displayed next to the value. When the parameter is edited, the unit is selected either from the list or by means of the front-panel keys. When the entry is completed, the unit can be changed. In this case the value remains unchanged but is automatically adapted to the new unit.

While operating the instrument by means of a mouse, assign the unit to the selected parameter before entering its value.

##### Assigning a unit

To assign a unit to a value, use one of the following alternatives:

1. Press a [UNIT] key on the front panel.


Frequency 10 GHz + M/μ = Frequency 10.000 000 000 MHz


2. Select a "Unit" in the selection field next to the parameter value. Press the [ENTER] key.

The unit displayed in the entry field next to the value is assigned.

##### Changing a unit

To subsequently change a unit, i.e. after the entry has been terminated and when the editing mode is not active, use one of the following alternatives:

1. Press a [UNIT] key on the front panel.

2. Select"Unit" in the selection field next to the parameter value.

The value remains unchanged but the display is automatically adapted to the new unit, i.e. the value is recalculated to suit the new unit.


Frequency 10 000.000 000 kHz + M/μ = Frequency 10.000 000 000 MHz


The new unit is indicated in the value field of the menu.

Selecting a Value from a List

Selection lists provide a list of predefined values for the selected parameter.


| Source | Internal |
| --- | --- |
| Output | Internal External |

To select a item from a list, proceed as follows:

1. Press [ENTER] key to open the list.

2. Use one of the following alternatives to navigate through the list:

a) Turn the rotary knob or use the [Up/Down] arrow keys. The selected item is highlighted.

b) Press [TOGGLE ON/OFF] key several times until the preferred entry is displayed in the selection field.

3. To confirm the selection, press the [ENTER] key.

##### Terminating Entries with Confirmation

The instrument behaves different by the termination of entries depending on the parameter type and the way this parameter is set.

##### Confirming settings

▶ To confirm the settings, press the rotary knob or one of the [Unit] keys (see also "Working with Units" on page 81).

Note: Variations by means of the rotary knob are immediately set.

##### Confirming multiple values

In some cases, like for instance when editing data in a user correction table, it is useful first to enter few values and to confirm them together. Such settings require additional confirmation. Not yet confirmed settings are displayed on a yellow background as an indication that the currently displayed values do not represent the target signal.

▶ To confirm these settings, select the "Save" or "Accept" button, respectively.

##### Confirming parameters with On/Off state

Most of the instrument functions with enabled and disabled states are calculated and effective only after this function has been enabled. However, there are functions like the frequency variation of the reference oscillator for instance, that are immediately set after confirmation.

▶ To confirm a parameter with On/Off state, enable the parameter.

##### Restoring the Previous Value

Parameter variations with the rotary knob are immediately set and therefore not reversible.

Normally, values cannot be restored in the case of mouse control because no explicit confirmation is required in this case and entries are automatically confirmed when the entry or selection field is exited.

##### Restoring values

In the case of front-panel control or operation from the keyboard, previous values can be restored as long as the new value is not confirmed, i.e. the entry is not completed.

▶ To restore the values, press the [ESC] key.

##### Restoring values that require confirmation

All settings that are not confirmed with the "Accept" button but require this additional confirmation can be restored.

1. Press the [ESC] key.

A confirmation query is displayed.


Manning
You changed some parameter(s) which require a new calculation of the signal.
Do you really want to abort your changes without ACCEPT?
OK
Cancel


2. Confirm with "OK" to abort the changes.

Select "Cancel" to return to the dialog. The previous selected settings are displayed.

##### Restoring values after an extended calculation has been started

Calculation and setting might require different period of time. Many settings are made without noticeable calculation times; such operations are indicated by a "BUSY" message displayed in the status field of the header section.

A window with a progress indicates that the instrument performs an extended calculation that requires longer calculation time. The termination of such a calculation restores the previous values.

▶ Press the [Abort] button to terminate the calculation.

All previous values are restored.

##### 3.5.1.6 Editors

The R&S SMB provides user-friendly editors for defining lists. Lists containing frequency and level value pairs are used for the list mode and the user-defined level correction.

The lists are saved to files and may thus have any length. The file name of the lists and the directory to which the files are saved are user-selectable. The file prefix is different for each list type and is permanently assigned by the system.

For information about file handling and overview of the automatically assigned file pre-fixes, refer to Chapter 3.5.1.8, "File Management", on page 86.

##### Working with List Editor

The "User Correction" and "List Mode" dialogs provide a list editor for defining the frequency/level value pairs.

##### Editing list mode data lists

1. To access a list editor and open an existing data list for editing, use the cursor keys to select the associated button "Edit User Correction Data..." or "Edit List Mode Data..." (if available) in the individual menu.

The selected list is displayed and the cursor marks the first row of the "Frequency/Hz" column.


|  | Frequency/Hz | Power/dBm |
| --- | --- | --- |
| 1 | 100 000.000 | -140.00 |
| 2 | 100 010.000 | -140.00 |
| 3 | 100 020.000 | -140.00 |
| 4 | 100 030.000 | -140.00 |
| 5 | 100 040.000 | -140.00 |
| 6 | 100 050.000 | -140.00 |
| 7 | 100 060.000 | -140.00 |
| 8 | 100 070.000 | -140.00 |
| 9 | 100 080.000 | -140.00 |
| Goto | Edit | Save |

If no list has been selected, a blank list of only one row is displayed.

2. Press the [Left/Right] arrow keys to change between the columns. Use the [Up/Down] arrow keys to mark a row.

3. Use the numeric keys to enter the value for the value pairs in the "Frequency/Hz" and "Power/dBm" table columns. A blank row is inserted at the end of the list. Terminate the entry by pressing a [Unit] key.

4. To select a row, select the "GoTo" button and press the [ENTER] key. Use the numeric keys to enter the row index in the entry field and press the [ENTER] key to confirm the entry.

The cursor moves to the selected row.

5. To insert a new row in the table, select the row above which the new row is to be inserted and select "Insert Row(s)".

A row is inserted above the currently marked row.

If no row has been selected, a row is inserted at the beginning of the list.

6. Use the "Save" function to save the edited list under its current name.

Enter the file name in the "File Select" dialog and select the directory (see "File Select Dialog" on page 88).

Only complete value pairs are taken into consideration; rows containing an entry in only one column are ignored.

##### Creating a new list mode data list

A new list can be created under a new name either by generating a blank file in the "File Select" menu (see section Chapter 3.5.1.8, "File Management", on page 86) or by changing an existing list which will then be saved under a new name.

1. To create an empty data list, select "RF > List Mode > List Mode Data... > New List" or respectively "RF > User Correction > User Cor. Data... > New User Correction Data" and enter the file name of the new data list.

2. To open the data list for editing, select the associated button "Edit User Correction Data..." or "Edit List Mode Data..." in the individual menu.

Edit the list and save it under a new name.

##### 3.5.1.7 How to Use the Help System

The R&S SMB is equipped with a context-sensitive help function. A help page is available for each parameter and can be called any time during instrument operation.

##### Calling context-sensitive and general help

▶ To display the general help dialog box, press the [HELP] (F1) key.

The help dialog is displayed. A topic containing information about the current menu or the currently opened dialog box and its function is displayed.

Help - State

Contents Index Back
Instrument Function > Modulations > Amplitude Modulation (AM) > Amplitude Modulation Menu > State

State
Activates/deactivates AM modulation.
SCPI command:
[ :SOURce<hw>]:AM:STATE

On top, the help dialog box contains a navigation bar with:

• "Contents" - accesses a table of help contents

• "Index" - switches to an index table

● "Back /"Previous/ "Next" for navigation to further help topics

##### Navigating in the table of contents

1. To navigate within the table of contents entries, use the [Up/Down] keys. Entries that contain further entries are marked with a plus sign.

2. To display a help topic, press the "ENTER" key.

The corresponding help topic is displayed.

##### Navigating in the help topics

1. To scroll through a page, use the [Up/Down] arrow keys.

2. To follow a cross-reference, select the link text.

3. To return to the previous page, select "Back".

This function scrolls back all steps that you have performed before.

4. Use the "Scroll Right" or "Scroll Left" buttons to shift the indicated area of the navigation window to the left or right.

##### Using the Index

1. Select "Index".

2. Enter the first characters of the topic you are interested in. The entries starting with these characters are displayed.

3. Press the [ENTER] key to change the focus.

4. Use the [Up/Down] keys to navigate and select the suitable keyword.

5. Press the [ENTER] key to display the help topic.

The corresponding help topic is displayed.

##### Closing the help window

▶ Press the [HELP] (F1) key.

##### 3.5.1.8 File Management

The R&S SMB uses files to save all instrument data, i.e. system and user data.

The user data includes saved instrument settings and lists and the user correction.

The files are stored in the internal memory of the instrument or on a USB memory stick. The /var directory can be used to save user-defined data; any subdirectory structure can be created on /var. Some default subdirectories are predefined, but can be changed at any time.

The /opt directory is a protected system drive and therefore unaccessible system directory. The files on this directory contain data that must not be changed. Therefore, this drive should not be accessed, since reconstruction of the system partition will lead to data loss. To prevent inadvertent deletion or overwriting of system files, this drive is not specified in the file menus.

Files can be exchanged either via a memory stick or a connected network. A memory stick is connected to the USB interface and is assigned the var/usb/ drive. In the case of a connected network, all network drives that can be accessed are available. The files are accessed in a "Save/Recall" dialog in the individual menus.

The files are differentiated according to their extensions; each type of file is assigned a specific file content. The extension is usually of no consequence to the user since access to the files occurs in the individual menus where only the relevant type of file is

available. See "Extensions for User Files" on page 89 for an overview of the supported file extensions.

The user data can be roughly divided into the following data types:

##### • Settings

Instrument settings can be saved and loaded. In case of saving, the current setting is saved to the specified file.

##### Lists

Lists, e.g. user correction lists, can be loaded. They can be generated either externally or internally. For internal generation, a new list must be created in the "File Select" dialog which will then be edited in the list editor of the individual menu.


##### For more information, refer to:

Chapter 3.5.1.9, "Legend of Front-Panel Controls", on page 90 for an overview of key functions and a cross-reference between the front panel keys and the keyboard shortcuts

- to section "Instrument Functions" for a detailed description of key functions.

##### Accessing files with user data

1. To access an editable user data file, select the "Save/Recall" or "File Manager" function in the individual dialog.


| Recall Settings |
| --- |
| Save Settings |
| File Manager... |

2. To access a loadable data file, select the "Select/New" or "File Manager" function in the individual dialog.


| Select User Correction Data |
| --- |
| New User Correction Data |
| File Manager... |

3. To access the "File Manager" function, press the [SETUP] (CTRL+E) key and select "Save/Recall > File Manager".

A "File Select" window for loading, saving or creating a file or the "File Manager" dialog for managing all files is displayed.


##### Saving and loading of all instrument settings

All instrument settings are saved and loaded in the "File" menu.

To access the "File" menu, press the [FILE] (CTRL+S) key.

For more information, see Chapter 4.2.8, "Storing and Loading Instrument Data - File Key", on page 127.

##### File Select Dialog

The "Save/Recall" dialaog displays the available drives and directories. In the upper part, "Recent Data Sets", the files last used are listed.


| Save/Recall | ✗ |  |
| --- | --- | --- |
| Operation Mode | Save | Recent Files |
| /var/user/SavRcl |  |  |
| ☑ Lists☑ Pulse Trains☑ SavRcl☑ UCS2010☑ /var/volatile☑ |  |  |
| File Name: |  |  |
| presetsettings |  |  |
| Save | Save | File |
|  | Imm 1 | Imm 2 |


| Save/Recall |  |  |  |
| --- | --- | --- | --- |
| Operation Mode |  | Recall | Recent files |
| /var/user/Lists/SavRcl |  |  |  |
| + | DME |  |  |
| - | LevelCalData |  |  |
| + | LfGenCalData |  |  |
| + | Rf |  |  |
| - | SavRcl |  |  |
| + | presetsettings |  |  |
| + | SvnCalibData |  |  |
| Exclude Frequency |  | Exclude Level |  |
| Recall | Recall | Recall | File |
|  | Imm 1 | Imm 2 | Mgr... |

The available drives and directories and the files of the selected directory are displayed. The currently selected path is displayed above the window. Only the relevant files without file extensions are displayed. If the area is opened several times, the path last selected is displayed. When a file is saved or created, its name is user-selectable; the extension is assigned automatically and cannot be entered. The file is saved to the selected path.

In addition to the files saved by the user, some menus also offer files containing predefined contents. These files are saved to a specific directory on system drive; for this reason, this directory cannot be chosen from the "File Select" menu.

##### Working with the File Select dialog

1. Access the "File Select" dialog (see "Accessing files with user data" on page 87).

2. Navigate in the "File Select" dialog.

3. Load an existing file.

In the "Recall Settings" dialog, mark a file and press the "Select" button.

4. Save a file.

In the "Save Settings" dialog, enter file name in the "File Name:" field.

Select the directory to which the file is to be saved and then select the "Save" button.

5. Create a new file.

To create a new file, use the "Save Settings" functionality, i.e. specify file name and directory and save the file.

The created file is empty; it must be filled with the necessary values in the individual editor.

##### File Manager

The "File Manager" allows general file management such as copying, shifting, renaming and deleting files as well as generating new directories.


| File Type | Instrument State (.savrclxt) |
| --- | --- |
| /var/user/Lists/SavRd | All Files (*.) |
| + | Instrument State (.savrclxt) |
| + | DME\nARP Waveform (.w) |
| + | LevelCalD\nNRP Setting (.nrp) |
| + | LfGenCal\nList Mode List (.lsw) |
| + | Rf\nAll Files (*) |
| + | SavRcl\npresetsettings.savrclxt |
| + | SynCalibData |
| Cut | Copy\nPaste |
| Rename | Delete\nCreate New Dir |

Use the "File Type" to select a file type from the list. This can be used to process either all files (all files (*) selection) or a specific selection of files. See "Extensions for User Files" on page 89 for an overview of the supported file extensions. The available drives and directories and the files of the selected directory are displayed. The currently selected path is displayed above the windows. If the area is opened several times, the path last selected is displayed. Unlike the "File Select" window, the "File Manager" displays the full file names including extensions.

##### Working with the File Manager dialog

1. Accessing the "File Manager" dialog (see "Accessing files with user data" on page 87).

2. Navigating in the "File Manager" dialog.

Operation is very similar to the operation of a standard Windows explorer.

3. Moving, duplicating, deleting or renaming files

To move a file, select the file and press the "Cut" button. Mark the directory to which the file is to be moved and select the "Paste" button. If the target directory already contains a file with the same name, a confirmation query is displayed to confirm overwriting of this file.

Perform the similar steps and cut/copy/rename/delete the file.

Tip: The operation corresponds to the Windows concept.

4. Creating a new directory

Mark drive or directory level where the new directory is to be created, select the "Create New Directory" button and enter the name of the new directory in the entry window that opens. Confirm with [ENTER].

##### Extensions for User Files

The following table lists all available file extensions for user files. The currently available files on the instrument depend on the installed options.

Table 3-4: List of the automatically assigned file extensions in the instrument


| Function | List type | Contents | File suffix |
| --- | --- | --- | --- |
| Instrument State | Settings | Instrument settings | *.savrcltxt |
| "User Correction" | List | User-defined level correction values | *.uco |
|  |  | Export Data | *.txt or *.csv |
| "List Mode" | List | User-defined frequency/level value pairs | *.lsw |
|  |  | Export Data | *.txt or *.csv |
| "Pulse Train List" |  | User-defined offtime/ontime/repetition values | *.pulstrn |
| SMZ Settings | Settings | Data (firmware) of a connected SMZ frequency multiplier | *.efmfirm |
| NRP Settings | Settings | NRP Settings | *.nrp |


##### 3.5.1.9 Legend of Front-Panel Controls

The following table lists all key functions available on the front panel. Key combinations used on the PC keyboard to trigger key functions on the instrument front panel are also described. Keyboard labels are described in alphabetical order.

In addition, a front panel key emulation and an on-screen keyboard can be used for manual operation by mouse only.

Table 3-5: Cross-reference between the front panel keys and keyboard shortcuts


| Front-panel key | Key of PC keyboard | Function |
| --- | --- | --- |
| Turning the rotary knob | Tab key (towards the right) Shift + Tab (towards the left) | Sets the cursor with the rotary knob. |
| Pressing the rotary knob | Enter | Pressing the rotary knob confirms an entry; it has the same function as the ENTER key. |
| Arrow keys | Arrow keys | Moves the cursor. |
| ENTER / *1 / dB(m) | Enter | Terminates an entry. |
|  | ALT + F12 | Confirms entries in the base unit and values with-out a unit. |
|  |  | Selects dBm for the RF level and dB for level offset and level step width. |
| ./*...# | ./*...# | Enters a period/decimal point. Enters a special character. |
| -/A<->a | -/ (shift+) a—z | Enters the sign. |
|  |  | Switches between upper-case and lower-case letters. |
| 0-9 / a...z | CTRL+ 0-9 / a...z CTRL | Enters the number/letter. |
| [BACKSPACE] | Backspace | Clears the last entry (number, sign or decimal point) |
| [ESC / CLOSE] | ESC / CTRL + G | Selects the next higher menu/selection level. When the editing mode is exited with ESC, the previous value is restored. |
|  |  | Closes an active menu. |
| [DIAGR] | CTRL+D | Sets the cursor on the block diagram and hides all menus. |
| [FILE] | CTRL+S | Activates the menu for storing instrument settings. |
| [FREQ] | CTRL+F | Activates the frequency entry. |
| G/n / dBuV | ALT+F9 | Selects the unit Giga/Nano, dBuV for the RF level and dBu for the LF level. |
| [HELP] | F1 | Opens/closes context-sensitive help. |
| [INFO] | CTRL+I | Opens/closes the info window |
| k/m / mV | ALT+F11 | Selects the units kilo/milli and mV for RF levels. |
| [LEVEL] | CTRL+L | Activates the level entry. |
| [LOCAL] | CTRL+Q | Switches the instrument from remote control to manual control. |
| M/u / uV | ALT+F10 | Selects the units Mega/Micro and uV for RF levels. |
| [MOD ON/OFF] | CTRL+O | Switches modulation on/off. "MOD OFF" is indica-ted in the status line. |
| [TOGGLE] | CTRL+T | Switches a block or parameter on/off. Toggles between the different possibilities of set-ting a selection parameter. |
| [PRESET] | CTRL+P | Restores a defined basic instrument setup. |
| [RF ON/OFF] | CTRL+R | Switches the RF output signal on/off. "RF OFF" is indicated in the status line. |
| [SETUP] | CTRL+E | Opens the setup menu for general instrument set-tings. |
| [WINBAR] | CTRL+W | Toggles between the active menus. |


##### Front Panel Key Emulation

The R&S SMB provides a front panel key emulation to enable execution of the front panel key functions by mouse e.g. for remote access. The emulation is called by a right mouse click. The front panel key functions are executed by a mouse click on the associated button.


| Freq | Diagram |
| --- | --- |
| Level | Menu |
| File | Rearr. |
| RF On/Off | WinBar |
| Mod On/Off | Help |
| Preset |  |
| Local |  |
| Setup |  |
| Hardcopy |  |
| Info |  |

#### 3.5.2 Remote Control

Remote control is an operation of the instrument by means of remote control commands or programs that automatize repeating settings. The instrument is connected to a computer running the program.

The R&S SMB supports various remote control connections:

• Connecting the instrument to a (LAN) network

• Using the LXI browser interface in a LAN network

• Connecting a PC via the IEC-bus (IEEE 488) interface

• Remote control via the USB interface


Tip: For remote control over LAN or USB, you can use the R&S VISA (Virtual Instrument Software Architecture) library provided for download at the Rohde & Schwarz website http://www.rohde-schwarz.com/rsvisa.

This way of operation and the instructions how to set up a connection for remote control are described in the Chapter 5, "Remote Control Basics", on page 240. The description of the remote control commands is provided in Chapter 6, "Remote Control Commands", on page 283.

#### 3.5.3 Remote Access

Remote access is the operating of the instrument from a remote computer. Both the R&S SMB and the computer are connected in a LAN.

Remote access in contrast to remote control does not use remote-control commands but a separate software which is installed on the remote computer. After its start, the software simulates the user interface of the instrument. The instrument can thus be operated from the remote computer as on the unit itself. The individual functions are operated using the mouse and keyboard. Specific instrument functions can be execu-

ted using specific key combinations on the keyboard or a front panel key emulation that can be operated with the mouse.

This way of operation and the instructions how to set up a connection for remote access are described in Chapter 3.1.16, "Remote Access via an External Controller", on page 32.
