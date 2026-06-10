## 6 Remote Control Commands

In the following, all remote-control commands will be presented in detail with their parameters and the ranges of numerical values.

For an introduction to remote control and the status registers, refer to Chapter 5, "Remote Control Basics", on page 240.

### 6.1 Conventions used in SCPI command descriptions

The following conventions are used in the remote command descriptions:

##### • Command usage

If not specified otherwise, commands can be used both for setting and for querying parameters.

If a command can be used for setting or querying only, or if it initiates an event, the usage is stated explicitly.

##### • Parameter usage

If not specified otherwise, a parameter can be used to set a value and it is the result of a query.

Parameters required only for setting are indicated as Setting parameters.

Parameters required only to refine a query are indicated as Query parameters. Parameters that are only returned as the result of a query are indicated as Return values.

##### • Conformity

Commands that are taken from the SCPI standard are indicated as SCPI confirmed. All commands used by the R&S SMB follow the SCPI syntax rules.

##### • Asynchronous commands

A command which does not automatically finish executing before the next command starts executing (overlapping command) is indicated as an Asynchronous command.

##### • Reset values (*RST)

Default parameter values that are used directly after resetting the instrument (*RST command) are indicated as *RST values, if available.

##### • Factory preset values

Default parameter values that are reset only by factory preset.

##### • Default unit

##### • Manual operation

The default unit is used for numeric values if no other unit is provided with the parameter.

If the result of a remote command can also be achieved in manual operation, a link to the description is inserted.

### 6.2 Common commands

Common commands are described in the IEEE 488.2 (IEC 625-2) standard. These commands have the same effect and are employed in the same way on different devices. The headers of these commands consist of "*" followed by three letters. Many common commands are related to the Status Reporting System.

Available common commands:

*CLS.....284    
*ESE.....284    
*ESR?.....284    
*IDN?.....285    
*IST?.....285    
*OPC.....285    
*OPT?.....285    
*PRE.....286    
*PSC.....286    
*RCL.....286    
*RST.....286    
*SAV.....287    
*SRE.....287    
*STB?.....287    
*TRG.....287    
*TST?.....288    
*WAI.....288

##### *CLS

##### Clear status

Sets the status byte (STB), the standard event register (ESR) and the EVENT part of the QUESTionable and the OPERation registers to zero. The command does not alter the mask and transition parts of the registers. It clears the output buffer.

Usage: Setting only

##### *ESE <Value>

Event status enable

Sets the event status enable register to the specified value. The query returns the contents of the event status enable register in decimal form.

Parameters:

<Value>

Range: 0 to 255

##### *ESR?

Event status read

Returns the contents of the event status register in decimal form and then sets the register to zero.

##### Return values:

<Contents> Range: 0 to 255

Usage: Query only

##### *IDN?

Identification

Returns the instrument identification.

##### Return values:

<ID> "Rohde&Schwarz,<device type>,<part number>/<serial number>,<firmware version>"

Example: Rohde&Schwarz, SMB, 1412.0000K02/000000, 03.01.158

Usage: Query only

Manual operation: See "Hardware Options / Software Options" on page 99

##### *IST?

Individual status query

Returns the contents of the IST flag in decimal form. The IST flag is the status bit which is sent during a parallel poll.

##### Return values:

<ISTflag> 0 | 1

Usage: Query only

##### *OPC

Operation complete

Sets bit 0 in the event status register when all preceding commands have been executed. This bit can be used to initiate a service request. The query writes a "1" into the output buffer when all preceding commands have been executed, which is useful for command synchronization.

##### *OPT?

Option identification query

Queries the options included in the instrument. For a list of all available options and their description, refer to the data sheet.

| Return values: | |
|---|---|
| `<Options>` | The query returns a list of options. The options are returned at fixed positions in a comma-separated string. A zero is returned for options that are not installed. |
| Usage: | Query only |
| Manual operation: | See "Hardware Options / Software Options" on page 99 |

##### *PRE <Value>

Parallel poll register enable

Sets parallel poll enable register to the indicated value. The query returns the contents of the parallel poll enable register in decimal form.

##### Parameters:

<Value> Range: 0 to 255

##### *PSC <Action>

Power on status clear

Determines whether the contents of the ENABLE registers are preserved or reset when the instrument is switched on. Thus a service request can be triggered when the instrument is switched on, if the status registers ESE and SRE are suitably configured. The query reads out the contents of the "power-on-status-clear" flag.

##### Parameters:

<Action> 0 | 1

0

The contents of the status registers are preserved.

1

Resets the status registers.

##### *RCL <Number>

##### Recall

Loads the instrument settings from an intermediate memory identified by the specified number. The instrument settings can be stored to this memory using the command *SAV with the associated number.

It also activates the instrument settings which are stored in a file and loaded using the MMEMORY:LOAD <number>, <file_name.extension> command.

Manual operation: See "Recall Immediate x" on page 131

##### *RST

Reset

Sets the instrument to a defined default status. The default settings are indicated in the description of commands.

The command is equivalent to SYSTEM:PRESet.

Usage: Setting only

Manual operation: See "Preset" on page 96

##### *SAV <Number>

##### Save

Stores the current instrument settings under the specified number in an intermediate memory. The settings can be recalled using the command *RCL with the associated number.

To transfer the stored instrument settings in a file, use the command :MMEMory:

STORE:STATE.

Manual operation: See "Save Immediate x" on page 129

##### *SRE <Contents>

Service request enable

Sets the service request enable register to the indicated value. This command determines under which conditions a service request is triggered.

##### Parameters:

<Contents>

Contents of the service request enable register in decimal form.

Bit 6 (MSS mask bit) is always 0.

Range: 0 to 255

##### *STB?

Status byte query

Reads the contents of the status byte in decimal form.

Usage: Query only

##### *TRG

##### Trigger

Triggers all actions waiting for a trigger event. In particular, *TRG generates a manual trigger signal. This common command complements the commands of the TRIGger subsystem.

Usage: Event

Manual operation: See "Execute Single Trigger" on page 234

##### *TST?

Self-test query

Initiates self-tests of the instrument and returns an error code.

##### Return values:

`<ErrorCode>`

| Value | Description |
|---|---|
| integer > 0 (in decimal format) | An error occurred. (For details, see the Service Manual supplied with the instrument). |
| 0 | No errors occurred. |

Usage: Query only

##### *WAI

Wait to continue

Prevents servicing of the subsequent commands until all preceding commands have been executed and all signals have settled (see also command synchronization and *OPC).

Usage: Event
