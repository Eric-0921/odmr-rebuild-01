### 6.12 SENSE, READ and INITiate Subsystems

The SENSE subsystem contains the commands for configuring the power measurements with R&S NRP-Zxx power sensor(s) connected to the generator. The measurement is started and the measurement result retrieved with the READ command. The description of this commands is included in the following.

Up to four sensors can be connected to the signal generator. They are distinguished by means of the suffix under SENSE, that means SENSE [1] ... SENSE 4.

The suffixes <ch>

Furthermore the following suffixes denote:

• Sensor assignment READ<ch> and INITiate<hw>; range: [1] to 4

• Sensor mapping list: ELEMENT<ch>; range [1] to 25

:SLiSt:CLEAR:LAN.....319    
:SLiSt:CLEAR:USB.....319    
:SLiSt:CLEAR:[ALL].....319    
:SLiSt:ELEMENT<ch>:MAPping.....319    
:SLiSt:SCAN:LSENsor.....319    
:SLiSt:SCAN:USENsor.....320    
:SLiSt:SCAN:[STATE].....320    
:SLiSt:SENSOR:MAP.....320    
:SLiSt:[LIST]?.....321    
:SENSE<ch>[:POWER]:APERture:DEFAULT:STATE.....321    
:SENSE<ch>[:POWER]:APERture:TIME.....321    
:INITiate<hw>[:POWER]:CONTINUOUS.....322    
:READ<ch>[:POWER]?.....322    
:SENSE<ch>[:POWER]:CORRECTION:SPDevice:SELECT.....323    
:SENSE<ch>[:POWER]:CORRECTION:SPDevice:STATE.....323    
:SENSE<ch>[:POWER]:CORRECTION:SPDevice:LIST?.....323    
:SENSE<ch>[:POWER]:DISPLAY:PERMANENT:STATE.....324    
:SENSE<ch>[:POWER]:DISPLAY:PERMANENT:PRIORITY.....324    
:SENSE<ch>[:POWER]:FILTER:LENGTH:AUTO?.....324    
:SENSE<ch>[:POWER]:FILTER:LENGTH:[USER].....325    
:SENSE<ch>[:POWER]:FILTER:NSRATIO.....325    
:SENSE<ch>[:POWER]:FILTER:NSRATIO:MTIME.....325    
:SENSE<ch>[:POWER]:FILTER:SONCE.....326    
:SENSE<ch>[:POWER]:FILTER:TYPE.....326    
:SENSE<ch>[:POWER]:LOGGING:STATE.....327    
:SENSE<ch>[:POWER]:FREQUENCY.....327    
:SENSE<ch>[:POWER]:OFFSET.....327    
:SENSE<ch>[:POWER]:OFFSET:STATE.....328    
:SENSE<ch>[:POWER]:SNUMBER?.....328    
:SENSE<ch>[:POWER]:SOURCE.....328    
:SENSE<ch>[:POWER]:STATUS:[DEVICE]?.....329    
:SENSE<ch>[:POWER]:SVERSION?.....329    
:SENSE<ch>[:POWER]:TYPE?.....329    
:SENSE<ch>[:POWER]:ZERO.....330    
:SENSE<ch>[:POWER].....330

##### :SLiSt:CLEar:LAN

Removes all R&S NRP power sensors connected in the LAN from the list.

##### Example:

Usage: Event

Manual operation: See "Clear" on page 169

##### :SLiSt:CLEar:USB

Removes all R&S NRP power sensors connected over USB from the list.

Example: :SLISt:CLEar:LAN // remove sensors connected in the LAN from the list

Usage: Event

Manual operation: See "Clear" on page 169

##### :SLISt:CLEAR[:ALL]

Removes all R&S NRP power sensors from the list.

Example: :SLISt:CLEar[ALL] // remove all sensors from the list

Usage: Event

Manual operation: See "Clear" on page 169

##### :SLISt:ELEMENT<ch>:MAPping <Mapping>

Assigns an entry from the :LIST[:LIST] to one of the four sensor channels.

##### Parameters:

<Mapping>

    SENS1 | SENSor1 | SENS2 | SENSor2 | SENS3 | SENSor3 |

    SENS4 | SENSor4 | UNMapped

    *RST: UNMapped

Example: SLIST:ELEMENT3:MAPping SENS1

maps the third sensor from the list to the first sensor channel

Manual operation: See "Sensor Mapping List" on page 168

##### :SLISt:SCAN:LSENsor <|P>

Scans for R&S NRP power sensors connected in the LAN.

Setting parameters:

Example: :SLIST:SCAN:LSENsor 'NRQ6',101624 //sensor name, serial number

:SLIST:SCAN:LSENsor 'NRQ6',11.123.1.123, 101624 //IP address, serial number

Usage: Setting only

Manual operation: See "Add LAN Sensor settings" on page 169

:SLISt:SCAN:USENsor <DeviceID>, <Serial>

Scans for R&S NRP power sensors connected over a USB interface.

Parameters:
<Serial> integer
Range: 0 to 999999

Setting parameters:
<DeviceID> String or Integer
Range: 0 to 999999
*RST: 0

Example:
:SLiSt:SCAN:USENsor 'NRQ6',101624 //sensor name, serial number
:SLiSt:SCAN:USENsor #H15b,101624 //device ID (hexadecimal), serial number
:SLiSt:SCAN:USENsor 347,101624 //device ID (decimal), serial number

Usage: Setting only

Manual operation: See "Add USB Sensor settings" on page 169

#### :SLISt:SCAN[:STATE] <State>

Starts the search for R&S NRP power sensors, connected in the LAN or via the USBTMC protocol.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: SLIST:SCAN:STATE 1

searches for sensors connected in the LAN or via the USBTMC protocol

Manual operation: See "Scan" on page 169

##### :SLISt:SENSOR:MAP <SensorId>, <Mapping>

Assigns a sensor directly to one of the sensor channels, using the sensor name and serial number.

To find out the sensor name and ID, you can get it from the label of the R&S NRP, or using the command :SLISt:SCAN[:STATE]. This command detects all R&S NRP power sensors connected in the LAN or via USBTMC protocol.

Setting parameters:
<SensorId> string

<Mapping>

    enum

    Example:

        SLISt:SENSOR:MAP "NRPS18S-100654-USB Legacy",

        SENS4

        maps the sensor directly to channel 4

Usage: Setting only

Manual operation: See "Sensor Mapping List" on page 168

#### :SLISt[:LIST]?

Returns a list of all detected sensors in a comma-separated string.

Return values:

<SensorList> Each entry contains information on the sensor type, serial number and interface. The order of the entries does not correspond to the order the sensors are displayed in the "NRP Sensor Mapping" dialog.

Example: :SLiSt:LIST?
// Response:
// "NRP33SN-V-900007-USB Legacy", "NRP-Z211-900001-USB Legacy",
// "NRP33SN-V-900005-USB TMC", "NRP33SN-V-900011-LAN"
// list of automatically detected sensors
// the list can contain more entries
list of automatically detected sensors; the list can contain more entries
Usage: Query only
Manual operation: See "Sensor Mapping List" on page 168

#### :SENSE<ch>[:POWER]:APERture:DEFAULT:STATE <UseDefAp>

Deactivates the default aperture time of the respective sensor.

To specify a user-defined value, use the command :SENSE<ch>[:POWER]:APERTURE:TIME on page 321.

Parameters:

<UseDefAp> 0 | 1 | OFF | ON

*RST: 1

Example: SENS:POW:APER:DEF:STAT 0

  deactivates the default aperture time of the sensor.

Manual operation: See "Use Default Aperture Time" on page 177

#### :SENSE<ch>[:POWER]:APERture:TIME <ApTime>

Defines the aperture time (size of the acquisition interval) for the corresponding sensor.

##### Parameters:

<ApTime> float

Range: depends on connected power sensor

Increment: 1E-9

*RST: depends on connected power sensor

Example: SENS:POW:APER:TIM 23ms sets 23 ms aperture time.

Manual operation: See "Aperture Time" on page 177

##### :INITiate<hw>[:POWER]:CONTINUOUS <Continuous>

The command switches the local state of the continuous power measurement by the R&S NRP-Zxx power sensors on and off. Switching off the local state enhances the measurement performance during remote control

The remote measurement is triggered by the READ query (command :READ<ch>[: POWER] ? on page 322) which also provides the measurement results. The local state is not influenced by this command, measurements results can be retrieved with local state on or off.

##### Parameters:


<Continuous> 0 | 1 | OFF | ON

*RST: OFF

Example: INIT:CONT ON

switches local state of continuous power measurement on.

Manual operation: See "State" on page 173

##### :READ<ch>[:POWER]?

The command triggers the measurement with power sensors and provides the power measurement result of the selected power sensor. The value is provided with the unit set with command SENSE:UNIT[:POWER].

For certain power sensors, e.g. R&S NRP-Z81, two values are returned, first the value for the average level and - separated by a comma - the peak level

Note: The local state is not influenced by this command, measurements results can be retrieved with local state on or off. For long measurement times it is recommended to use a SRQ (MAV bit) for command synchronization.

Suffix:

<ch>1..3

##### Return values:

<Power> string

Example: SENS:UNIT DBM

selects unit dBm for presentation of measurement result.

READ1?

queries the measurement result of the sensor connected to the

SENSOR interface.

Response: -45.6246576745440230

-45.6 dBm were measured at the given frequency.

or e.g. for R&S NRP-Z81

Response:

-55.62403263352178, -22.419472478812476

-55.6 dBm is the measured average level, -22.4 dBm is the

measured peak level at the given frequency

Usage: Query only

Manual operation: See "Level (Peak)" on page 173

##### :SENSE<ch>[:POWER]:CORRection:SPDevice:SELECT <Select>

Several S-parameter tables can be stored in a sensor. The command selects a loaded data set for S-parameter correction for the corresponding sensor.

##### Parameters:



| <Select> | float |  |
|---|---|---|
|  | *RST: | 0 |

##### :SENSE<ch>[:POWER]:CORRection:SPDevice:STATE <State>

The command activates the use of the s-parameters correction data of the selected power sensor.

Note: For power sensor with attenuator this command is automatically set to ON.

##### Parameters:

<State> 0 | 1 | OFF | ON
*RST: OFF
Example: SENS:POW:CORR:SPD:STAT ON
activates the use of the s-parameters correction data of power sensor 1.
Manual operation: See "Use SParameter - Power Sensors" on page 159

##### :SENSE<ch>[:POWER]:CORRection:SPDevice:LIST?

Queries the list of the S-parameter data sets that have been loaded to the power sensor.

Return values:
<List> string list
*RST: 0

Usage: Query only

#### :SENSE<ch>[:POWER]:DISPLAY:PERManent:STATE <State>

The command switches on and off the permanent indication of the power measurement result in the upper right corner of the block diagram. For each sensor, the type of sensor, the connector, the measurement source and - if set - the offset is indicated.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: ON

Example: SENS1:POW:DISP:PERM:STAT ON the permanent viewer is switched on.

Manual operation: See "Permanent Display State" on page 174

#### :SENSe<ch>[:POWER]:DISPLAY:PERManent:PRIority <Priority>

The command selects which power measurement result (average or peak power) is indicated when permanent display is active.

<Priority> AVERage | PEAK

*RST: PEAK

Example: SENS1:DISP:PERM:STAT ON the permanent viewer is switched on. SENS1:DISP:PERM:PRI AVER the measured average power is indicated.

Manual operation: See "Display Priority" on page 174

#### :SENSE<ch>[:POWER]:FILTER:LENGTH:AUTO?

The command queries the current filter length for auto filter mode ( :SENSE<[1] ...3>:POWER:FILTER:TYPE AUTO)

Return values:

<Auto>

    float

    Range: 1 to 65536

Example:

    SENS1:FILT:TYPE AUTO

    selects auto filter mode for the power sensor connected to the

    SENSOR connector.

    SENS1:FILT:LENG:AUTO?

    queries the automatically set filter length.

    Response: 1024

Usage:

Manual operation: See "Filter Length" on page 176

#### :SENSE<ch>[:POWER]:FILTER:LENGTH[:USER] <User>

The command selects the filter length for user filter mode (SENSE:POWER:FILTER:TYPE USER). As the filter length works as a multiplier for the time window, a constant filter length results in a constant measurement time. Values 1 and 2^n are settable.

The time window is fixed to 20 ms.

Parameters:
<User>
    float
    Range: 1 to 65536
    *RST: 1

Example:
    SENS:FILT:TYPE USER
    selects user filter mode.
    SENS:FILT:LENG 16
    sets a filter length of 16. The resulting measurement time is 640 ms (2x16x20 ms).

Manual operation: See "Filter Length" on page 176

##### :SENSe<ch>[:POWER]:FILTER:NSRatio <NSRatio>

The command defines the noise content for fixed noise filter mode

(:SENSE<[1] ...3>:POWER:FILTER:TYPE NSRatio). This value determines the proportion of intrinsic noise in the measured result.

Parameters:
<NSRatio> float
Range: 0.001 to 1
Increment: 0.001
*RST: 0.01
Example: SENS1:FILT:TYPE NSR
selects fixed noise filter mode for the power sensor connected to the SENSOR connector.
SENS1:FILT:NSR 0.2
sets a noise content of 0.2.
Manual operation: See "Noise Content" on page 176

#### :SENSE<ch>[:POWER]:FILTER:NSRatio:MTIME <MTime>

The command defines the timeout for fixed noise filter mode

(:SENSE<[1] ...3>:POWER:FILTER:TYPE NSRatio). This value ensures limited settling times.

Parameters:
<MTime> float
Range: 1 to 999.99
Increment: 0.01
*RST: 4
Default unit: s
Example: SENS1:FILT:TYPE NSR
selects fixed noise filter mode for the power sensor connected to the SENSOR connector.
SENS1:FILT:NSR .2
sets a noise content of 0.2.
SENS1:FILT:NSR:MTIM 5
limits the settling time to 5 seconds
Manual operation: See "Timeout" on page 177

Manual operation: See "Timeout" on page 177

##### :SENSe<ch>[:POWER]:FILTER:SONCe

The command activates the search for the optimum filter length for the current measurement conditions. The found filter length can be retrieved with com-

mand :SENSE:POWER:FILTER:LENGTH:USER?. This command is only available for user filter mode ( :SENSE:POWER:FILTER:TYPE USER).

Example: SENS:FILT:TYPE USER

selects user filter mode.

SENS:FILT:SONC

activates the search for the optimum filter length.

SENS:FILT:LENG?

returns the found optimum filter length.

Response: 128

##### Usage: Event

Manual operation: See "Auto Once" on page 177

##### :SENSE<ch>[:POWER]:FILTER:TYPE <Type>

The command selects the filter mode. The filter length is the multiplier for the time window and thus directly influences the measurement time.

##### Parameters:

<Type>

##### AUTO | USER | NSRatio

##### AUTO

The filter length is automatically selected depending on the measured value. For high values, a short filter length is selected and for low values a long filter length is selected.

##### USER

The filter length is set manually. As the filter length works as a multiplier for the measurement time, this results in a constant measurement time.

##### NSRatio

The filter length (averaging factor) is selected so that the sensor's intrinsic noise (2 standard deviations) does not exceed the specified noise content. The desired noise content is entered with command SENSE: FILTer:NSRatio.

To avoid very long settling times when the power is low, the averaging factor can be limited with the Timeout parameter (command SENSE: FILTer:NSRatio:MTIME).

Example: SENS:FILT:TYPE AUTO

selects automatic filter selection.

Manual operation: See "Filter" on page 175

#### :SENSE<ch>[:POWER]:LOGGing:STATE <State>

Activates the recording of the power values, measured by a connected R&S NRP power sensor.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: SENS:LOGG:STAT ON

activates recording of the power measurement of the first sensor.

Manual operation: See "Enable Logging" on page 177

#### :SENSe<ch>[:POWer]:FREQUency <Frequency>

The command sets the RF frequency of the source if the user source is selected (SENSE[:POWER]:SOURCE USER).

##### Parameters:

<Frequency> float

*RST: 1 GHz

Example: SENS:SOUR USER

selects user-defined source.

SENS:FREQ 2.44 GHz

enters the RF frequency of the source which is 2.44 GHz.

Manual operation: See "Frequency" on page 175

##### :SENSE<ch>[:POWER]:OFFSET <Offset>

The command enters a level offset which is added to the measured level value after activation with command SENSE[:POWER]:OFFSET:STATE ON. This allows e.g. an attenuator in the signal path to be considered.

##### Parameters:

`<Offset>` float

Range: -100.0 to 100.0

*RST: 0

Default unit: dB

Example: SENS:POW:OFFS 10.0 sets a level offset of 10 dB

Manual operation: See "Level Offset" on page 175

##### :SENSE<ch>[:POWER]:OFFSET:STATE <State>

The command activates the addition of the level offset to the measured value. The level offset value is set with command SENSE[:POWER]:OFFSET.

##### Parameters:

<State>

0 | 1 | OFF | ON

*RST: OFF

Example: SENS1:POW:OFFS 0.4dB

sets a level offset of 0.4 dB

SENS1:POW:OFFS:STAT ON

a level offset of 0.4 dB is added to the measured value.

Manual operation: See "Level Offset" on page 175

#### :SENSe<ch>[:POWER]:SNUMBER?

The command queries the serial number of the sensor.

##### Return values:

<Snumber> string

Example: SENS:SNUM?

queries the serial number.

Usage: Query only

Manual operation: See "Current Sensors" on page 102

##### :SENSe<ch>[:POWER]:SOURCE <Source>

The command selects the signal source for the measurement.

Parameters:

<Source> A | B | USER | RF

*RST: A

Example: SENS:SOUR A

selects the RF signal as measurement source. The RF frequency is used as the measurement frequency of the sensor and the corresponding correction factor is used. The level setting of the instrument serves as reference level of the measurement.

Manual operation: See "Source" on page 175

#### :SENSe<ch>[:POWER]:STATUS[:DEVICE]?

The command queries if a sensor is connected to the signal generator.

The sensor is selected by suffix in the keyword SENSE or READ of the command header. Suffix 1 denotes the sensor connected to the SENSOR connector, suffix 2 the sensor connected first to one of the USB interfaces and suffix 3 the sensor connected second to one of the USB interfaces.

##### Return values:

<DEVICE> 0 | 1 | OFF | ON

Example: SENS:STAT?

queries if a sensor is connected to the instrument.

Response: 1

a sensor is connected to the POWER SENSOR interface.

Usage: Query only

Manual operation: See "Sensor" on page 172

#### :SENSe<ch>[:POWER]:SVERsion?

The command queries the software version of the connected R&S NRP power sensor.

##### Return values:

<Sversion> string

Example: SENS:POW:SVER?

queries the software version of the R&S NRP power sensor.

Usage: Query only

Manual operation: See "Current Sensors" on page 102

##### :SENSe<ch>[:POWER]:TYPE?

The command queries the type of sensor. The type is automatically detected.

##### Return values:

<Type> string

Example: SENS:TYPE?

queries the type of sensor connected to the POWER SENSOR connector.

Response: NRP-Z21

the R&S NRP-Z21 sensor is used.

Usage: Query only

Manual operation: See "Current Sensors" on page 102

##### :SENSE<ch>[:POWER]:ZERO

The command activates the autozero function. Zeroing is required in regular interval (at least once a day) and if the temperature has varied more than about 5 °C, if the sensor has been replaced or if measurements of signals with very low power are to be performed. The RF power source must be switched off or disconnected from the sensor before starting the autozero function.

Example: SENS:ZERO

activates autozero function.

Usage: Event

Manual operation: See "Zero - Power Sensors" on page 158

#### :SENSe<ch>:UNIT[:POWER] <Power>

The command selects the unit used for result query with command READ. The power sensor provides the measured value in Watt. In which unit the measured value is returned is selected here and might be either Watt, dBm or dBuV.

##### Parameters:

<Power> DBM | DBUV | WATT

*RST: DBM

Example: SENS2:UNIT DBM

selects unit dBm for the measured value returned by command

READ.

READ2?

Response: 7.34

7.34 dBm are measured by sensor 2.

Manual operation: See "Unit" on page 174

