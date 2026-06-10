### 6.16 TEST Subsystem

The self tests return a "0" if the test is performed successfully, otherwise a value other than "0" is returned. None of the commands of this system have an *RST value.

#### NOTICE

##### Improper use can destroy the assembly

The respective hardware assembly responds directly to the :TEST:DIRECT command; any safety mechanisms are bypassed. The command is intended for servicing purposes and should be used only by the Rohde & Schwarz service personnel.

:TEST<hw>:ALL:START.....457
:TEST<hw>:ALL:RESULT?.....457
:TEST<hw>:DIRect.....457

#### :TEST<hw>:ALL:START

Starts a self-test on all installed hardware options.
To query the result, use the command :TEST<hw>:ALL:RESult? on page 457.
Example: See :TEST<hw>:ALL:RESult? on page 457
Usage: Event
Manual operation: See "Start Selftest" on page 497
:TEST<hw>:ALL:RESult?
Queries the result of the performed self-test (command :TEST<hw>:ALL:STARt on page 457).
Return values:
<Result> 0 | 1 | RUNning | STOPped
0 Success
1 Fail
*RST: STOPped
Example: TEST:ALL:STAR Starts the self-test TEST:ALL:RES?
Usage: Query only
Manual operation: See "Start Selftest" on page 497

:TEST<hw>:DIRECT<HW_assembly>,<subaddress>,<hex data string>:TEST<hw>:DIRECT?<HW_assembly>,<subaddress>

The respective hardware assembly responds directly to the command; any safety mechanisms are bypassed. This function is only available via remote control.

Example: TEST:DIR 'SSYN', 0, #H12345678
TEST:DIR? 'SSYN', 0
Response: #H12345678

### 6.17 TRIGger Subsystem

The TRIGger system contains the commands for selecting the trigger source for the RF and LF sweep. The trigger input connectors are configured in the SOURce:INPUT subsystem.

The trigger system of the R&S SMB is a simplified implementation of the SCPI trigger system. The TRIGger system differs from the SCPI system as follows:

- No INITiate command; the instrument behaves as if INITiate:CONTINUOUS ON were set.

• Under TRIGger several sweep subsystems exist.

Other commands associated with the trigger system of the R&S SMB can be found in the modulation and RF signal subsystems.

• Suffix TRIGger<1|2> is not permitted

TRIGger<hw>

• TRIGger0 activates the LF output.

Table 6-2: Cross-reference between the manual and remote control

| R&S name | SCPI name | Command under manual control |
| --- | --- | --- |
| AUTO | IMMediate | "Auto" mode |
| SINGLE | BUS | "Single" mode. |
| EXTERNAL | EXTERNAL | "Ext Single" and "Ext Step" mode. Use command LFO: SWEep:MODE to select between the two sweep modes. |
| EAUTO | - | "Ext Start/Stop" mode. |

:TRIGger<hw>:FSweep:SOURce.....458

:TRIGger<hw>:FSweep[:IMMediate].....459

:TRIGger<hw>:LFFSweep.....459

:TRIGger<hw>:LFFSweep:SOURce.....460

:TRIGger<hw>:LFFSweep:IMMediate.....460

:TRIGger<hw>:PSweep:SOURce.....461

:TRIGger<hw>:PSweep[:IMMediate].....461

:TRIGger<hw>[:SWeep]:SOURce.....462

:TRIGger<hw>[:SWeep]:[:IMMediate].....463

:TRIGger<hw>[:IMMediate].....463

##### :TRIGger<hw>:FSWeep:SOURCE <Source>

Sets the trigger source for the RF frequency sweep.

The names of the parameters correspond directly to the various settings under manual control. SCPI uses other names for the parameters; these names are also accepted by the instrument. The SCPI names should be used if compatibility is an important consideration.

An overview of the various names is given in Table 6-2.

Parameters:

<Source>

AUTO | IMMediate | SINGle | BUS | EXternal | EAUTo

##### AUTO|IMMediate

The trigger is free-running, i.e. the trigger condition is fulfilled continuously. As soon as one sweep is finished, the next sweep is started.

##### SINGLEBUS

One complete sweep cycle is triggered by the GPIB commands [ : SOURce<hw>]:SWEep[:FREQUency]:EXECute, : TRIGger<hw>:FSweep[:IMMediate] or *TRG. The mode has to be set to AUTO ( : SOURce:SWEep:FREQUency:MODE AUTO).

##### EXTernal

The sweep is triggered externally via the [INST TRIG] connector.

EAUTO

The sweep is triggered externally via the [INST TRIG] connector. As soon as one sweep is finished, the next sweep is started. A second trigger event stops the sweep at the current frequency, a third trigger event starts the trigger at the start frequency, and so on.

*RST: AUTO

Example: TRIG:FSW:SOUR EXT

selects triggering with an external trigger.

Manual operation: See "Mode - RF Frequency Sweep" on page 180

#### :TRIGger<hw>:FSWeep[:IMMediate]

Immediately starts an RF frequency sweep cycle.

The command is only effective for sweep mode "Single" (SOUR: SWE: FREQ: MODE AUTO in combination with TRIG: FSW: SOUR SING).

The command corresponds to the manual control "Execute Single Sweep".

Example: SWE: FREQ:MODE AUTO

sets the triggered sweep mode, i.e. a trigger is required to start the sweep.

TRIG: FSW: SOUR SING

sets the "Single" trigger mode, i.e. a trigger starts a single sweep.

TRIG: FSW

starts a single RF frequency sweep.

Usage: Event

Manual operation: See "Execute Single Sweep - Frequency Sweep" on page 183

#### :TRIGger<hw>:LFFSweep

Usage: Event

Manual operation: See "Execute Single Sweep" on page 228

Immediately starts an LF frequency sweep.

The command is effective in sweep mode "Single" (LFO: SWE: MODE AUTO in combination with TRIG: LFFS: SOUR SING).

##### :TRIGger<hw>:LFFSweep:SOURCE <Source>

Sets the trigger source for the LF sweep. The trigger is triggered by the command : SOURce:LFOutput:SWEep[:FREQUency]EXECute.

The names of the parameters correspond directly to the various settings under manual control. SCPI uses other names for the parameters; these names are also accepted by the instrument. The SCPI names should be used if compatibility is an important consideration.

An overview of the various names is given in the Table 6-2.

##### Parameters:

##### <Source>

##### AUTO|IMMediate

The trigger is free-running, i.e. the trigger condition is fulfilled continuously. As soon as one sweep is finished, the next sweep is started.

##### SINGLE|BUS

One complete sweep cycle is triggered by the GPIB commands [ : SOURce<hw> ] : LFOutput : SWEep [ : FREQuency ] : EXECute or *TRG.

The mode has to be set to AUTO([ : SOURce<hw> ] : LFOutput : SWEep[ : FREQuency] : MODE).

##### EXTernal

The sweep is triggered externally via the [INST TRIG] connector.

##### EAUTo

The sweep is triggered externally via the [INST TRIG] connector. As soon as one sweep is finished, the next sweep is started. A second trigger event stops the sweep at the current frequency, a third trigger event starts the trigger at the start frequency, and so on.

*RST: AUTO

Example: TRIG:LFFS:SOUR EXT

selects triggering with an external trigger.

#### :TRIGger<hw>:LFFSweep:IMMediate

Immediately starts an LF frequency sweep.

The command is effective in sweep mode "Single" (LFO: SWE: MODE AUTO in combination with TRIG: LFFS: SOUR SING).

Usage: Event

Manual operation: See "Execute Single Sweep" on page 228

:TRIGger<hw>:PSWeep:SOURCE <Source>

Sets the trigger source for the RF level sweep.

The names of the parameters correspond directly to the various settings under manual control. SCPI uses other names for the parameters; these names are also accepted by the instrument. The SCPI names should be used if compatibility is an important consideration.

An overview of the various names is given in Table 6-2.

##### Parameters:

AUTO | IMMediate | SINGle | BUS | EXternal | EAUTo

AUTO|IMMediate

The trigger is free-running, i.e. the trigger condition is fulfilled continuously. As soon as one sweep is finished, the next sweep is started.

SINGLE|BUS

One complete sweep cycle is triggered by the GPIB commands [:SOURce<hw>]:SWEep:POWER:EXECute, :TRIGger<hw>:PSweep[:IMMediate] or *TRG. The mode has to be set to AUTO(:SOURce:SWEep:LEVEL:MODE AUTO).

##### EXTernal

The sweep is triggered externally via the [INST TRIG] connector.

##### EAUTo

The sweep is triggered externally via the [INST TRIG] connector. As soon as one sweep is finished, the next sweep is started. A second trigger event stops the sweep at the current frequency, a third trigger event starts the trigger at the start frequency, and so on.

*RST: AUTO

Example: TRIG:PSW:SOUR EXT

selects triggering with an external trigger.

Manual operation: See "Mode - Level Sweep" on page 188

#### :TRIGger<hw>:PSWeep[:IMMediate]

Immediately starts an RF level sweep.

The command is only effective for sweep mode "Single"

(SOURCE: SWEep: POWER: MODE AUTO in combination with TRIG: PSW: SOUR SING).

The command corresponds to the manual control "Execute Single Sweep".

Example: SWE: POW:MODE AUTO

selects the triggered sweep mode, i.e. a trigger is required to start the sweep.

TRIG: PSW: SOUR SING

sets the single trigger mode, i.e. a trigger starts a single sweep.

TRIG: PSW

starts a single RF level sweep.

Usage: Event

Manual operation: See "Execute Single Sweep - Level Sweep" on page 190

##### :TRIGger<hw>[:SWEep]:SOURCE <Source>

Sets the trigger source for all sweeps.

The names of the parameters correspond directly to the various settings under manual control. SCPI uses other names for the parameters; these names are also accepted by the instrument. The SCPI names should be used if compatibility is an important consideration.

An overview of the various names is given in the Table 6-2.

##### Setting parameters:

<Source>

AUTO | IMMediate | SINGle | BUS | EXternal | EAUTo

AUTO|IMMediate

The trigger is free-running, i.e. the trigger condition is fulfilled continuously. As soon as one sweep is finished, the next sweep is started.

##### SINGLE|BUS

One complete sweep cycle is triggered by the GPIB commands : SOURce:SWEep:POWER|FREQUency:EXEC, TRIGger:PSWeeP|FSWeeP:IMMediate or *TRG.

If : SOURce:SWEep:POWER:MODE is set to STEP, one step is executed.

The mode has to be set to AUTO.

##### EXTernal

The sweep is triggered externally via the [INST TRIG] connector.

##### EAUTo

The sweep is triggered externally via the [INST TRIG] connector. As soon as one sweep is finished, the next sweep is started. A second trigger event stops the sweep at the current frequency, a third trigger event starts the trigger at the start frequency, and so on.

*RST: AUTO

Example: TRIG0: SOUR EXT

selects triggering with an external trigger. The trigger is input via the [INST TRIG] connector.

Usage: Setting only

Manual operation: See "Mode" on page 226

#### :TRIGger<hw>[:SWEep][:IMMediate]

Starts all sweeps which are activated for the respective path. The command starts all sweeps which are activated.

The sweep to be executed depends on the respective MODE setting

(: SOUR: SWEep: POW| FREQ: MODE and : SOUR: LFO: SWEep [ : FREQ ] : MODE).

The command corresponds to the manual-control command "Execute Trigger".

Example: TRIG

starts all active sweeps.

Usage: Event

Manual operation: See "Execute Single Sweep - Frequency Sweep" on page 183

#### :TRIGger<hw>[:IMMediate]

The command immediately starts the activated sweep.

The command performs a single sweep and therefore applies to sweep mode AUTO with sweep source SINGle. Use the commands

TRIG:FSW|LFFS|PSW|[:SWE]:SOUR SING, and SOUR:SWE:FREQ|POW:MODE, or SOUR:LFO:SWE:[FREQ:]MODE to set the respective single sweep. You can alternatively use an IMMediate command instead of the respective SWEep:[FREQ:]|POW:EXECute command.

Example: TRIG

starts all active sweeps.

Usage: Event

Manual operation: See "Execute Single Sweep" on page 228

### 6.18 UNIT subsystem

The UNIT subsystem contains the commands specifying which units are valid if no unit is indicated in a command. These settings are valid for the entire instrument.

#### :UNIT:ANGLE <Angle>

Defines the default unit for the phase modulation angle. It is not valid for commands which determine angle values, e.g. RF phase. It does not influence the manual control parameter unit and the display.

Parameters:
<Angle> DEGRee | RADian
*RST: RADian

Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

Example: UNIT: ANGL DEG

sets DEG as a default unit for all commands which determine angle values.

:UNIT:POWER <Power>

Defines the default unit for power parameters. This setting affects the GUI, as well as all remote control commands that determine power values.

Parameters:

<Power> V | DBUV | DBM

*RST: DBM

Example: UNIT: POW V

sets V as a default unit for all commands which determine power values.

### 6.19 Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

The direct command allow to access all functions of the stereo coder option.

Some of the functions are also available via SCPI commands. In this case, it is recommended to use the SCPI commands in order to keep the settings of the R&S SMB and the stereo coder synchronized. Direct command for which a SCPI command is available are marked with “for documentation reasons only” and the SCPI command is given.

The direct commands are sent to the Stereo/RDS Coder with [SOURCE:] STEREO:DIRECT "command string".

Information is queried with STEREo:DIRECT? "command string".

All parameters are string parameters, this is the reason why all of them have to be sent in quotation marks ("— characters are part of the full direct command !").

Prior to using the stereo coder, the stereo modulation of the R&S SMB has to be switched on with command SOURce:STEREO:STATE ON. The SCPI command SOURce:STEREO:AUDIO:FREQUency sets the LF-Generator frequency and command SOURce:STEREO:MMF limits the modulation frequency. These commands have no counterpart in the direct commands.

#### 6.19.1 Remote-Control Commands

STEReo:DIRECT "<FFG>=<RetrNumb>,<DataSeq#1>,<DataSeq#2>,..."

STEReo:DIRECT? "<FFG>"

Transmits data via free format groups (FFGs). A free format group can be filled with any desired data. (5 bits in block B and 16 bits each in blocks C and D of the group).

Note: The command described here only causes a queue to be filled with data for a specific group. The data will only be sent when the group in question is added to the group sequence with the command "GS", on page 473.

Setting parameters:

<RetrNumb> Number of retransmissions

<DataSeq> Max. 20 different data sequences can be defined.

10 characters must be specified each per <DataSeq>. Leading zeros, if any, must also be specified.

00: erases the data.

Range: 0000000000 to 1FFFFFFF (10 ASCII coded hexadecimal characters each)

Parameters for setting and query:

<FFG>

1A | 3A | 5A | 6A | 7A | 8A | 9A | 10A | 11A | 12A | 13A

Determines the free format group.

To transmit the FFGs of the B group, the same commands are used, only the A groups are replaced by the B groups in the group sequence. If B groups are transmitted, block C is overwritten with the PI code.

Example: STEREo:DIRect "1A=01,0123456789,1FFFFFFFFFF"

Fills a queue with the data "0123456789,1FFFFFFFFFF". The data is sent in consecutive order in group 1A after group 1A is added to the group sequence (see command "GS", on page 473).

Example: STEREo:DIRECT? "1A"

Reads the data of group 1A.

Response: "01,0123456789,1FFFFFFF"

STEReo:DIRECT "AF=<A>,<Freq#1>,<Freq#2>,..."

STEReo:DIRECT? "AF<z>"

Defines an alternative frequency list.

Note: A maximum of five AF lists with max. 25 frequencies per list can be created.

Parameters:

<Freq> xxx.x

Sets the alternative frequencies as ASCII coded decimal numbers.

If list <z> is not available, the response is ().

Range: 87.6 to 107.9

Setting parameters:

<A>

N

new AF list

+

AF list to be added

Query parameters:
<z> AF list to be read
Range: 1 to 5

Example:
STEREo:DIRECT "AF=N,97.4,98.3"
Defines an alternative frequency list, the alternative frequencies 97.4 and 98.3 are inserted.

Example:
STEREO:DIRECT? "AF1"
Reads the first alternative frequency list.
Response: "97.4,98.3"

Example:
STEREO:DIRECT "AF=N"
Deletes all frequency lists.

STEReo:DIRect "ARI=<State>"
STEReo:DIRect? "ARI"

(for documentation reasons only)
Activates ARI signal transmission.
Use SCPI command [ : SOURce ] : STEREo : ARI : STATE instead.
Setting parameters:
<State> 0 | 1
Example:
STEReo:DIRect "ARI=0"
Deactivates ARI signal transmission.
Example:
STEReo:DIRect? "ARI"
Response: "0"

STEReo:DIRECT "ARI-DEV=<Deviation>"

STEReo:DIRECT? "ARI-DEV"

(for documentation reasons only)

Sets the frequency deviation of the ARI signal (max. deviation).

Use SCPI command [ : SOURce ] : STEREO : ARI [ : DEviation ] instead.

Setting parameters:

<Deviation> Sets the frequency deviation.

Note: A four-digit value must always be set. Leading zeros, if any, must also be specified.

Range: 0000 to 1000 (ASCII coded decimal numbers), corresponding to 0 Hz to 10 kHz

Example: STEREO:DIRECT "ARI-DEV=1000"
Sets the ARI frequency deviation to 10 kHz.
Example: STEREO:DIRECT? "ARI-DEV
Response: "1000"

STEReo:DIRect "ARI-ID=<ld>"
STEReo:DIRect? "ARI-ID"

(for documentation reasons only)

Selects the ARI identification.

Use SCPI command [ : SOURce ] : STEREo : ARI : TYPE instead.

Parameters:
<ld>0|1|2|3
0
Off
1
DK (traffic announcement identification)
2
BK (area identification)
3
DK and BK (traffic announcement identification and area identification)

Example:
STEReo:DIRect "ARI-ID=0"
Deactivates the ARI identification.

Example:
STEReo:DIRect? "ARI-ID"
Response: "0"

##### STEReo:DIRECT "BIN=<x>"

Defines and sends, or queries, binary test patterns. The BIN command causes the Stereo/RDS Coder to send periodic binary bit patterns instead of RDS data.

##### Parameters:

0

binary mode OFF

1

00000000...,

2

11111111...,

3

01010101...,

4

11001100...

Example: STEREo:DIRECT "BIN=2"

The binary test pattern is set to "2" so that only "1s" are transmitted.

STEReo:DIRECT "BK=<Code>"

STEReo:DIRECT? "BK"

(for documentation reasons only)

Sets the ARI area identification.

Use SCPI command [ : SOURce ] : STEREo : ARI : BK [ : CODE ] instead.

Parameters:

<Code> A | B | C | D | E | F

Example: STEREo:DIRECT "BK=E"

The ARI area identification is set to "E".

Example: STEREO:DIRECT? "BK"

Response: "E"

STEReo:DIRECT "CT=<Hour><Min><Sec>,<Day><Month><Year>"

STEReo:DIRECT? "CT"

Sets and activates transmission of the real-time clock.

Note: The CT data is transmitted in group 4A. Setting the real-time clock (CT command) automatically adds group 4A to the group sequence. Group 4A must not be manually added to, or removed from, the group sequence. To remove group 4A from the group sequence, use the command "CT=off".

##### Setting parameters:

<Hour><Min><Sec> Range: 00:00:00 to 23:59:59

<Day><Month><Year>

Range: 01.01.00 to 31.12.85

Example: STEREo:DIRECT "CT=20:30:59,01.08.03"

The real-time clock is set to 20:30:59 and 1 August 2003.

Example: STEREO:DIRECT? "CT"

Response: "20:31:06,01.08.03"

##### STEReo:DIRECT "CT=off"

Deactivates transmission of the real-time clock signal in the RDS signal.

Note: This command is used to remove group 4A from the group sequence. Group 4A must not be manually removed from the group sequence.

Example:

STEReo:DIRECT "CT=off"

The real-time clock signal is no longer transmitted in the RDS signal.

Usage: Setting only

STEReo:DIRect "DI=<x>"
STEReo:DIRect? "DI"

Sets or reads the decoder information (DI).

With this command, the current decoder operating mode (mono, stereo, etc) can be detected and, if necessary, changed.

##### Parameters:

Parameters:
<x>
Range: 0 to F (ASCII coded hexadecimal numbers)

Example:
STEReo:DIRect "DI=4"
The decoder information is set to "4".

Example:
STEReo:DIRect? "DI"
Response: "4"

STEReo:DIRect "DS=<x>"
STEReo:DIRect? "DS"

(for documentation reasons only)

Selects/activates a storage area in the Stereo/RDS Coder.

Upon activation, the settings stored in the selected area can be loaded.

Use SCPI command [ : SOURce ] : STEREo : RDS : DATaset instead.

Parameters:
<x>
Range: 1 to 5
Example:
STEReo:DIRECT "DS=2"
Storage area 2 is activated.
Example:
STEReo:DIRECT? "DS"
Response: "2"

STEReo:DIRECT "EON-AFA= <PI>,<A>,<Freq#1>,<Freq#2>,..."

STEReo:DIRECT? "EON-AFA,<PI>,<z>"

Enhanced Other Networks: defines type A alternative frequencies for the EON with the selected PI.

##### Parameters:

Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

<Freq> xxx.x

Sets the alternative frequencies as ASCII coded decimal numbers.

If list <z> is not available, the response is ().

Note: For each Enhanced Other Network (EON), a maximum of five type A alternative frequency lists can be created.

Range: 87.6 to 107.9

Setting parameters:

<A>

N

new AF list

+

AF list to be added

Query parameters:

<z>

AF list to be read

Range: 1 to 5

Example: STEREo:DIRECT "EON-AFA=1000,N,97.4,98.3"

Creates a new type A alternative frequency list for the EON with PI=1000.

The new list contains the alternative frequencies 97.4 MHz and 98.3 MHz.

Example: STEREo:DIRECT? "EON-AFA,1000,1"

Reads the first type A alternative frequency list of the EON with PI=1000.

Response: "97.4,98.3"

STEReo:DIRECT "EON-AFB=<PI>,<A>,<Freq#1>,<Freq#2>,..."

STEReo:DIRECT? "EON-AFB,<PI>,<z>"

Enhanced Other Networks: defines type B alternative frequencies for the EON with the selected PI.

Parameters:

<PI>

Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

<Freq> xxx.x

Sets the alternative frequencies as ASCII coded decimal numbers.

If list <z> is not available, the response is ().

Note: For each Enhanced Other Network (EON), a maximum of five type B alternative frequency lists can be created, each list containing max. five frequencies, where <Freq#1> is Tuned Frequency (TF) and <Freq#2..5> are the Mapped Frequencies (MF). A minimum of two frequencies per EON is required.

Range: 87.6 to 107.9

Setting parameters:

<A>

N

new AF list

+

AF list to be added

Query parameters:

<z> AF list to be read

Range: 1 to 5

Example: STEREo:DIRECT "EON-AFB=1000,N,97.4,98.3"

Creates a new type B alternative frequency list for the EON with PI=1000.

The list contains the alternative frequencies 97.4 MHz and 98.3 MHz.

Example: STEREo:DIRECT? "EON-AFB,1000,1"

Reads the first type B alternative frequency list of the EON with PI=1000.

Response: "97.4,98.3"

STEReo:DIRECT "EON-DEL=<PI>"

Enhanced Other Networks: deletes the complete EON with selected <PI>.

Parameters:

<PI> Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Example: STEREO:DIRECT "EON-DEL=1000"

Deletes the EON with PI=1000.

Usage: Setting only

STEReo:DIRECT "EON-PI=<PI>"

STEReo:DIRECT? "EON-PI"

Enhanced Other Networks: creates a new EON or reads the list of the program identification (PI) codes of all EONs created so far.

Note: A maximum of eight EONs can be created.

##### Parameters:

<PI>

Note: A four-digit value must always be set. Leading zeros, if any, must also be specified.

Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Example: STEREo:DIRECT "EON-PI=1000"

Creates a new EON with PI=1000.

Example: STEREO:DIRECT? "EON-PI"

Response: "1000"

STEReo:DIRECT "EON-PS=<PI>,<PS>"

STEReo:DIRECT? "EON-PS,<PI>"

Enhanced Other Networks: sets the program service (PS) name for the EON with the selected <PI>.

##### Parameters:

<PI> Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

##### Setting parameters:

Set.

<PS>

Note: An eight-digit value must always be set. Blank spaces, if any, must also be entered, otherwise the value will not be accepted.

Example: STEREo:DIRECT "EON-PS=1000, Test 123"

Sets the program service name for the EON with PI=1000 to "Test 123".

Example: STEREo:DIRECT? "EON-PS,1000"

Reads the program service name of the EON with PI=1000.

Response: "Test 123"

STEReo:DIRECT "EON-PTY=<PI>,<PTY>"

STEReo:DIRECT? "EON-PTY,<PI>"

Enhanced Other Networks: sets the program type (PTY) for the EON with the selected <PI>.

##### Parameters:

<PI> Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Setting parameters:

<PTY>

Range: 00 to 31 (ASCII coded decimal numbers)

Example:

STEReo:DIRECT "EON-PTY=1000,10"

Sets the program type for the EON with PI=1000 to "10".

Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

Example: STEREO:DIRECT? "EON-PTY,1000"

Reads the program type of the EON with PI=1000.

Response: "10"

STEReo:DIRECT "EON-TA=<PI>,<TA>"

STEReo:DIRECT? "EON-TA,<PI>"

Enhanced Other Networks: sets the TA flag for the EON with the selected <PI>.

Parameters:

<PI> Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Setting parameters:

<TA> 0 | 1

Example:

STEREO:DIRECT "EON-TA=1000,1"

Sets the TA flag for the EON with PI=1000 to "1".

Example:

STEREO:DIRECT? "EON-TA,1000"

Reads the TA flag of the EON with PI=1000.

Response: "1"

STEReo:DIRECT "EON-TP=<PI>,<TP>"

STEReo:DIRECT? "EON-TP,<PI>"

Enhanced Other Networks: sets the TP flag for the EON with the selected <PI>.

Parameters:

<PI> Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Setting parameters:

<TP> 0 | 1

Example:

STEREO:DIRECT "EON-TP=1000,1"

Sets the TP flag for the EON with PI=1000 to "1".

Example:

STEREO:DIRECT? "EON-TP,1000"

Reads the TP flag of the EON with PI=1000.

Response: "1"

STEReo:DIRECT "GS=<Group#1>,<Group#2>,...<Group#36>"

STEReo:DIRECT? "GS"

Sets or reads the group sequence.

Note: Only group A or group B data may be sent at a time. Only groups that contain data are transmitted. The groups 4A, 14B and 15B are automatically added to the group sequence and must not be added or removed manually.

Setting parameters:

<Group>0A,1A,2A, ... to 15B

##### Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

Example: STEREO:DIRECT "GS=0A,1B,10A,15A"
The groups 0A,1B,10A,15A are transmitted.
Example: STEREO:DIRECT? "GS"
Response: "0A,1B,10A,15A"

STEReo:DIRect "IMP=<x>"
STEReo:DIRect? "IMP"

(for documentation reasons only)
Sets external L, R impedances.
Use the SCPI command [ :SOURce] :STEReo:EXTERNAL:IMPedance instead.
Setting parameters:
<x>
    1 | 2
    1
    600 Ohm
    2
    100 kOhm
Example:
    STEReo:DIRect "IMP=1"
    The external impedance is set to 600 Ohm
Example:
    STEReo:DIRect? "IMP"
    Response: "1"

##### STEReo:DIRECT

"MASK=<NumbGroups>,<ErrFreeGroups>,<BitMaskBlcA>,<BitMaskBlcB>,<BitMaskBlcC>,<BitMaskBlcD>"

STEReo:DIRECT? "MASK"

Sets a bit mask to generate defined bit errors in the RDS data stream.

##### Setting parameters:

<NumbGroups> Number of groups to be masked.

If <NumbGroups> is set to zero, the RDS groups are continuously linked to the error mask.

If <NumbGroups> is set to a value other than zero, this value is decremented after each errored group transmitted. When zero count is reached, no further errored groups are transmitted, and MASK_STATE is set to "0".

Range: 00 to FF (hexadecimal values)

<ErrFreeGroups> Number of error-free groups to be inserted after each errored group.

Range: 00 to FF (hexadecimal values)

Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

<BitMaskBlc>

    <BitMaskBlcA>,<BitMaskBlcB>,<BitMaskBlcC>,<BitMaskBlcD>

    Hexadecimal bit mask for blocks A, B, C and D of the RDS groups. For each block, 26 bits (16 data bits and 10 CRC bits) have to be entered in hexadecimal code.

    Range: 0000000 to 3FFFFFFF

Example:

STEREO:DIRECT

"MASK=09,01,0000001,0000000,0000000,0000000"

In nine RDS groups, the least significant bit of the CRC code of block A is inverted, i.e. an errored bit is sent. After each errored group, one error-free group is inserted. After transmission of the complete sequence, MASK_STATE is set to "0".

With the command MASK_STATE=1, the above sequence (9 errored groups with one error-free group inserted after each errored group) is retransmitted once.

Then, MASK_STATE is again set to "0".

Example: STEREO:DIRECT? "MASK"

Response: "09,01,0000001,0000000,0000000,0000000"

STEReo:DIRECT "MASK_STATE=<State>"

STEReo:DIRECT? "MASK_STATE"

Switches on or off the transmission of defined bit errors in the RDS data stream.

Setting parameters:

<State> 0 | 1

<State> 0 | 1

Example:

STEReo:DIRect "MASK_STATE=1"

With the command MASK_STATE=1, a sequence of errored groups as defined by the MASK command is retransmitted once if the number of groups to be masked is other than zero. Then, MASK_STATE is automatically set to "0".

If the number of groups to be masked is equal to zero in the MASK command (which means continuous error transmission), the masking function can be switched off with MASK_STATE=0.

Example: STEREO:DIRECT? "MASK_STATE"

Response: "1"

The MASK_STATE query provides information as to whether the RDS data stream is linked to an error mask.

STEReo:DIRECT "MODE=<EMODE>"

STEReo:DIRECT? "MODE"

(for documentation reasons only)

Sets one of various transmit modes.

Use the SCPI command [ : SOURce ] : STEREO:AUDIO:MODE instead.

Setting parameters:
<EMODE> 1 | 2 | 3 | 4 | 5
1
L: signal in left channel only
2
R: signal in right channel only
3
signal of equal frequency and phase in left and right channel
4
signal of equal frequency and opposite phase in left and right channel
5
different, independent signals in left and right channel
(5 is not possible if the internal LF generator is selected as source (SRC = LFGen))
Example:
STEREO:DIRect "MODE=1"
Only the signal of the left channel is transmitted.
Example:
STEREO:DIRect? "MODE"
Response: "1"

STEReo:DIRECT "MS=<Flag>"

STEReo:DIRECT? "MS"

Sets or reads the music/speech flag.

The flag signals whether music or speech is being transmitted.

Setting parameters:

<Flag> M | S

Example:

STEREo:DIRect "MS=M"

The music/speech flag is set to "M". This signals that music is currently transmitted.

Example:

STEREo:DIRect? "MS"

Response: "M"

STEReo:DIRECT "MPX-DEV=<Deviation>"
STEReo:DIRECT? "MPX-DEV"
(for documentation reasons only)
Sets the MPX frequency deviation (max. deviation).
Use the SCPI command [:SOURCE]:STEReo[:DEViation] instead.

Setting parameters:
<Deviation> A five-digit value must always be set. Leading zeros, if any, must also be specified.
Range: 00000 to 10000 (ASCII coded decimal numbers), corresponding to 0 Hz to 100 kHz
Example: STEREo:DIRECT "MPX-DEV=00201" Sets the MPX frequency deviation to 2.01 kHz.
Example: STEREo:DIRECT? "MPX-DEV" Response: "00201"

STEReo:DIRECT "PI=<PI>"
STEReo:DIRECT? "PI"

Sets or reads the RDS program identification (PI) code.

Setting parameters:

<PI>

Note: A four-digit value must always be set. Leading zeros, if any, must also be specified, otherwise the value will not be accepted.

Range: 0000 to FFFF (ASCII coded hexadecimal numbers)

Example:

STEREO:DIRECT "PI=1234"

The program identification code to be transmitted is set to "1234".

Example: STEREO:DIRECT? "PI"

Response: "1234"

STEReo:DIRECT "PIL=<State>"
STEReo:DIRECT? "PIL"

(for documentation reasons only)
Activates/deactivates the pilot tone.
Use the SCPI command [ : SOURce ] : STEREO : PILot : STATE instead.

Setting parameters:
<State> 0 | 1
Example:
STEREO:DIRECT "PIL=1"
The pilot tone is activated.
Example:
STEREO:DIRECT "PIL"
Response: "1"

STEReo:DIRECT "PIL-DEV=<Deviation>"
STEReo:DIRECT? "PIL-DEV"
(for documentation reasons only)

Sets the pilot tone frequency deviation (max. deviation).
Use the SCPI command [ : SOURce ] : STEREo : PILot [ : DEviation ] instead.
Setting parameters:
<Deviation> Note: A four-digit value must always be set. Leading zeros, if any, must also be specified.
Range: 0000 to 1000 (ASCII coded decimal numbers), corresponding to 0 Hz to 10 kHz
Example: STEREo : DIRect "PIL-DEV=1000"
Sets the frequency deviation of the pilot tone to 10 kHz.
Example: STEREo : DIRect? "PIL-DEV"
Response: "1000"

STEReo:DIRect "PIL-PH=<Phase>"
STEReo:DIRect? "PIL-PH"

(for documentation reasons only)
Sets the pilot tone phase.
Use the SCPI command [:SOURce]:STEReo:PILot:PHASE instead.
Setting parameters:
<Phase>
    Note: A two-digit value must always be set with a sign ("+" or "-") in front of it. Leading zeros, if any, must also be specified.
    Range: -5.0 to +5.0 (ASCII coded decimal numbers), corresponding to ±5.0
Example:
    STEReo:DIRect "PIL-PH=-33"
    The pilot tone phase is set to 3.3
Example:
    STEReo:DIRect? "PIL-PH"
    Response: "-33"

STEReo:DIRECT "PRE=<Preemphasis>"

STEReo:DIRECT? "PRE"

(for documentation reasons only)

Sets one of various preemphasis options.

Use the SCPI commands [ : SOURce ] : STEREo : AUDIO : PREemphasis : STATE and [ : SOURce ] : STEREo : AUDIO : PREemphasis instead.

Setting parameters:
<Preemphasis>0|1|2
0
Off
1
50 us

Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

|  | 2 |
| --- | --- |
|  | 75 us |
| Example: | STEReo:DIRECT "PRE=1" |
|  | The preemphasis is set to 50 us. |
| Example: | STEReo:DIRECT? "PRE" |
|  | Response: "1" |

##### STEReo:DIRECT "PRESET"

Sets the default settings in accordance with specifications.

Example: STEREO:DIRECT "PRESET"

Usage: Event

STEReo:DIRECT "PS=<PS>"

STEReo:DIRECT? "PS"

Sets or reads the RDS program service (PS) name.

Setting parameters:

<PS> 8 ASCII characters

Note: An eight-digit value must always be set. Blank spaces, if any, must also be entered, otherwise the value will not be accepted.

Special characters in the program service name are entered with a leading back slash (\) followed by the decimal code of the special character according to table E1 of CENELEC.

Example: STER:DIR "RT=02,0,test text with \217"

217 denotes the German ü.

Example: STEREo:DIRECT "PS=RDS Test"

Sets the program service name to be transmitted to "RDS Test".

Example: STEREO:DIRECT? "PS"

Response: "RDS Test"

STEReo:DIRECT "PS-TABLE=<Table>"

STEReo:DIRECT? "PS-TABLE"

Selects the character set table tobe used for the display of the RDS program service (PS) name in the receiver.

The information concerning the character set is transmitted in segment 0 of the PS. Segment 0 is repeatedly transmitted if the value for PS-TABLE > 0. For PS-TABLE=0 no information concerning the character set is transmitted.

Setting parameters:

0 | 1 | 2 | 3

0

no information concerning the character set table in the PS

1

2

3

Example: STEREo:DIRect "PS-TABLE=2"

The information concerning the character set is transmitted in segment 0 of the PS in group 0A. To this end, segment 0 is transmitted repeatedly. At the first transmission segment 0 contains the information about the character set, at the second transmission segment 0 contains the first two characters of the PS.

STEReo:DIRect "PTY=<PTY>"

STEReo:DIRect? "PTY"

Sets or reads the program type (PTY).

##### Setting parameters:

<PTY>

Note: A two-digit value must always be set. A leading zero, if any, must also be specified.

Range: 00 to 31 (ASCII coded decimal numbers)

Example: STEREo:DIRECT "PTY=08"

Sets the program type to be transmitted to "08".

Example: STEREO:DIRECT? "PTY"

Response: "08"

##### STEReo:DIRECT "PTYN=<PTYN>"

##### STEReo:DIRECT? "PTYN"

Sets or reads the RDS program type (PTY) name.

##### Setting parameters:

<PTYN> 8 ASCII characters

Note: An eight-digit value must always be set. Blank spaces, if any, must also be entered, otherwise the value will not be accepted.

Example:

STEREO:DIRECT "PTYN=Football"

Sets the program type name to be transmitted to "Football".

STEREO:DIRECT "GS=0A, 10A"

Group 10A is activated in addition to group 0A. The program

type name "Football" is now transmitted.

Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

Example: STEREO:DIRECT? "PTYN"
Response: "Football"
Example: STEREO:DIRECT "PTYN="
Transmission of PTYN in group 10A is stopped, even if group 10A is contained in the group sequence.

STEReo:DIRect "RDS=<State>"
STEReo:DIRect? "RDS"
(for documentation reasons only)
Switches RDS on or off.
Use the SCPI command [ : SOURce ] : STEREo : RDS : STATE instead.
Setting parameters:
<State> 0 | 1
Example:
STEReo:DIRect "RDS=1"
RDS is switched on.
Example:
STEReo:DIRect? "RDS"
Response: "1"

STEReo:DIRECT "RDS-PH=<Phase>"

STEReo:DIRECT? "PDS-PH"

Sets the RDS phase.

Setting parameters:
<Phase>
Range: 000 to 359 (ASCII coded decimal numbers)
Example:
STEReo:DIRECT "RDS-PH=100"
The RDS phase is set to 100.
Example:
STEReo:DIRECT? "RDS-PH"
Response: "100"

(for documentation reasons only)

Sets the RDS frequency deviation (max. deviation).

Use the SCPI command [ : SOURce ] : STEREo : RDS [ : DEviation ] instead.

Setting parameters:
<Deviation> Note: A four-digit value must always be set. Leading zeros, if any, must also be specified.
Range: 0000 to 1000 (ASCII coded decimal numbers), corresponding to 0 Hz to 10.00 kHz)

##### Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5

| Example: | STEReo:DIRECT "RDS-DEV=0201" The RDS frequency deviation is set to 2.01 kHz. |
| --- | --- |
| Example: | STEReo:DIRECT? "RDS-DEV" Response: "0201" |

##### STEReo:DIRECT "RDS-PRESET"

All RDS specific parameters are deleted or set to a default values.

| Example: | STEREO:DIRECT "RDS-PRESET" |
| --- | --- |
| Sets all RDS parameter to their preset values |  |
| Usage: | Event |

STEReo:DIRECT "RT=<RetranNumber>,<A/BFlag>,<RadioTextMsg#1>,<RadioTextMsg#2>"

STEReo:DIRECT? "RT"

##### Radio text

##### Setting parameters:

<RetranNumber> Range: 00 to 15 (ASCII coded decimal numbers), number of retransmissions of radio text message

<A/BFlag> 0 | 1

If the A/B flag is set, the A/B bit in group 2A is toggled to signal that a new radio text message will be transmitted.)

<RadioTextMsg> max. 64 characters

Two texts of 64 characters each can be transmitted in a radio text message

Note: For group B, the length of a radio text is limited to 32 characters. Special characters in the radio text are entered with a leading back slash (\) followed by the decimal code of the special character according to table E1 of CENELEC.

Example: STER:DIR "RT=02,0,test text with \217"

217 denotes the German ü.

Example: STEREO:DIRECT "RT=02,1,Test message 123"

The radio text message "Test message 123" is transmitted.

Example: STEREO:DIRECT? "RT"

Response: "02,1,Test message 123"

STEReo:DIRECT "SPS=<Time>,<PSN#1>,<PSN#2>,...<PSN#20>"

STEReo:DIRECT? "SPS"

Switching program service names (PSN). The program name automatically changed after the set time interval

Parameters:

<PSN>

8 ASCII characters

Max. 20 program service names of eight characters each can be entered.

Note: The program service names have to be entered as 8-digit texts. Blank spaces, if any, must also be entered, otherwise the value will not be accepted.

STEReo:DIRECT "SPS=0" stops the transmission of the scrolling PS and starts the transmission of the standard PS.

Setting parameters:
<Time> Time interval in seconds
Range: 00 to 59 s
Example:
STEREo:DIRect "SPS=05,TEST0123,TEST4567"
The program service names "TEST0123" and "TEST4567" are
alternately transmitted at an interval of 5 seconds.
Example:
STEREo:DIRect? "SPS"
Queries the program service names
Response: "05,TEST0123,TEST4567"

(for documentation reasons only)

Selects the signal source.

Use the SCPI command [ : SOURce ] : STEREo : SOURce instead.

Setting parameters:
<SigSource> 0 | 1 | 2 | 3
0
Off
1
external analog (via L and R inputs)
2
external digital
3
internal with LF generator

Example:
The external analog L and R inputs are selected as source.

Example:
The external analog L and R inputs are selected as source.

STEReo:DIRECT? "STATUS"

Status request as to whether the encoder or the update loader program is being executed.

##### Return values:

Return values:

<Status> ENC encoder program is running

UPL update loader program is running

Example: STEREo:DIRECT? "STATUS"

Response: "ENC"

Usage: Query only

##### STEReo:DIRECT "STORE=<DataSet>"

Stores data in the flash memory. All RDS-specific settings are stored in data set <DataSet#> of the flash memory.

##### Setting parameters:

<DataSet>

Range: 1 to 5

Example:

    STEREO:DIRECT "STORE=1"

    The current settings are stored in data set "1"

Usage:

    Setting only

##### STEReo:DIRECT "TA=<State>"

##### STEReo:DIRECT? "TA"

(for documentation reasons only)

Sets or reads the traffic announcement flag.

This flag signals whether traffic information is currently being broadcast.

Use the SCPI command [ : SOURce ] : STEREO : RDS : TRAFFIC : ANNOUNCEMENT [ : STATE ] instead.

Setting parameters:

<State> 0 | 1

Example:

    STEReo:DIRECT "TA=1"

    The traffic announcement flag is set to "1".

Example:

    STEReo:DIRECT? "TA"

    Response: "1"

STEReo:DIRECT "TP=<State>"
STEReo:DIRECT? "TP"
(for documentation reasons only)

Sets or reads the traffic program flag. This flag signals whether traffic information is generally transmitted.

Use the SCPI command [ : SOURce ] : STEREo : RDS : TRAFFic : ANNouncement [ : STATE ] instead.

##### Setting parameters:

<State> 0 | 1

Example: STEREO:DIRECT "TP=1"

The traffic program flag is set to "1".

Example: STEREO:DIRECT? "TP"

Response: "1"

STEReo:DIRECT "TRANS<DataNumber>=<DataStream>"

STEReo:DIRECT? "TRANS<DataNumber>"

##### Transparent mode.

An RDS data stream of binary data is generated. If transparent data is selected, all other RDS data is ignored.

##### Parameters:

<DataNumber> Max. 20 different data sequences can be defined.

Range: 0 to 13

<DataStream> 16 ASCII coded hexadecimal characters (blocks A to D of the RDS groups)

TRANS=0 deletes all transparent data and switches back to normal RDS data transmission.

Note: 16 characters must be specified for each data sequence.

Leading zeros, if any, must also be specified. The data will be transmitted even if it constitutes no meaningful RDS data.

Example: STEREo:DIRECT "TRANS1=0123456789ABCDEF"

The data "0123456789ABCDEF" is sent instead of the RDS data.

Example: STEREO:DIRECT? "TRANS1"

Reads the transparent data.

Response: "0123456789ABCDEF"

#### 6.19.2 Examples

##### 6.19.2.1 Alternative Frequency Lists

Alternative frequency lists can be transmitted in two ways:

• Method A:

The frequencies of an AF list are entered one after the other; the frequency currently transmitted has to be specified as the first frequency.

##### • Method B:

The frequencies of an AF list are entered in pairs, each pair containing the frequency currently transmitted and an alternative frequency. The frequency pairs should normally be entered in ascending order. Descending order should be chosen only if the alternative frequencies belong to different regions or are used to broadcast different programs at different times.

Do not combine methods A and B!

##### Method A:

1. Generate a new alternative frequency list with STEREO:DIRECT "AF=N,87.6,87.7,87.8".

Set the group sequence, e.g.

STEReo:DIRECT "GS=0A,14A".

The group sequence must contain group 0A.

The alternative frequencies are now transmitted in group 0A.

3. Add another alternative frequency list with

STEReo:DIRECT "AF=+,88.6,88.7,88.8"

##### Method B:

1. Generate a new alternative frequency list with

    STEREO:DIRECT "AF=N,87.6,90.2,87.6,90.2".

2. Set the group sequence, e.g.

STEREO:DIRECT "GS=0A,14A".

The group sequence must contain group 0A

The alternative frequencies are now transmitted in group 0A.

3. Add another alternative frequency list with

STEReo:DIRECT "AF=+,88.6,91.2,88.6,91.2"

The frequency lists are not checked for correctness. For this reason, make sure that the syntax is correct.

A maximum of five AF lists can be generated. For type A lists, max. 25 frequencies per list can be specified, for type B lists, max. 12 frequencies per list.

##### 6.19.2.2 Enhanced Other Networks

##### Creating an EON data set

1. Read the list of existing EON data sets with

STEREO:DIRECT? "EON-PI"

The list shows the EON PI codes already used and those remaining for new data sets.

2. Create an EON data set with

STEREO:DIRECT "EON-PI=1234"

3. Set the program service (PS) name for the EON data set with

STEREO:DIRECT "EON-PS=1234, TEST EON"

4. Set the group sequence, e.g.:

STEREO:DIRECT "GS=0A,14A"

Group 14A with variants 0 to 3 is now transmitted.

5. Create a new AF list for the EON:

a) Using method A

STEReo:DIRECT "EON-AFA=1234,N,87.6,87.7,87.8"
b) Create further AF lists for the EON, using method A:

STEReo:DIRECT "EON-AFA=1234,+88.6,88.7,88.8"
c) Read the first AF list of the EON with

STEReo:DIRECT? "EON-AFA,1234,1"

6. Create a new AF list for the EON, using method B:

STEReo:DIRECT "EON-AFB=1234, N, 87.6, 87.7, 87.8"

where 87.6 = tuned frequency,

87.7 = mapped frequency 1(variant 5),

87.8 = mapped frequency 2 variant 6)

Note: Do not combine methods A and B for generating EON alternative frequency lists.

A maximum of five AF lists can be generated. For type A lists, max. 25 frequencies per list can be specified, for type B lists, max. five frequencies per list.

##### 6.19.2.3 Free Format Groups (FFGs)

In the user-definable groups 1A, 3A, 5A, 6A, 7A, 8A, 9A, 10A, 11A, 12A and 13A, any desired data can be transmitted. Five bits of this data are transmitted in block B and 16 bits each in blocks C and D of the specified group.

1. Define the data to be transmitted in group 1A:

STEReo:DIRECT "1A=05,0000000000,1FFFFFFF"

Group 1A is now transmitted first with "0000000000" and then with "1FFFFFFF. Each of the two data sequences is retransmitted five times, which is indicated by the information "05".

2. Set the group sequence, e.g.:

STEREO:DIRECT "GS=0A,1A"

The defined data is now transmitted in group 1A.

Max. 20 different data sequences can be defined.

##### 6.19.2.4 Transparent-Mode

The transparent mode allows the user to transmit freely definable binary data instead of the standard RDS data. Blocks A to D of the RDS groups are used. This means that standard RDS data will no longer be transmitted when transparent data is set. The binary data will be sent even if it constitutes no valid or meaningful RDS data. The transmission of standard RDS data will not be resumed until the transparent data is deleted.

1. Transmit the alternating sequences '0123456789ABCDEF' and 'ABC-DEF0123456789' instead of the RDS data:

STEReo:DIRECT "TRANS=0123456789ABCDEF,ABCDEF0123456789"

2. Delete the transparent data and switch back to standard RDS data transmission with:

STEREO:DIRECT "TRANS=0"

Max. 20 different data sequences can be defined.

