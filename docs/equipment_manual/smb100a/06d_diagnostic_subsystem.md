### 6.5 DIAGnostic Subsystem

The DIAGnostic system contains the commands used for instrument diagnosis and servicing. SCPI does not define any DIAGnostic commands; the commands listed here

are all device-specific. All DIAGnostic commands are query commands which are not influenced by *RST.

:DIAGnostic<hw>:BGInfo?.....293

:DIAGnostic<hw>:BGInfo:CATalog?.....293

:DIAGnostic:INFO:OTIMe?.....294

:DIAGnostic:INFO:POCount?.....294

##### :DIAGnostic<hw>:BGINfo? [<Board>]

Checks the modules available in the instrument using the variant and revision state.

If the command is sent without parameters being specified, a complete list of all modules is returned (the various entries are separated by commas). The length of the list is variable and depends on the instrument equipment configuration.

If the command is sent with parameters, a list of the specified modules is returned (the various entries are separated by commas). A list of modules names can be called up using the command :DIAGnostic<hw>:BGINfo:CATalog? on page 293.

Query parameters:

<Board> string

Return values:

<BglInfo>

    <Module name>Module stock number incl. variant><Module revision><Module serial number>

    Each entry for one module consists of four parts which are separated by space characters.

Example: DIAG:BGIN

Queries the instrument configuration.

Returns the data of all available modules.

DIAG:BGIN? 'MBRD'

Queries the configuration of the motherboard.

Response: MBRD 1141.3501.02 1.5.3 100023

Module motherboard with part number 1141.3501.01 has revision 1.5.3 and serial number 100023.

Usage: Query only

Manual operation: See "Assembly" on page 98

##### :DIAGnostic<hw>:BGINfo:CATalog?

Queries the names of the assemblies available in the instrument.

Return values:

<Catalog> string

A complete list of all assemblies is returned (the various entries are separated by commas). The length of the list is variable and depends on the instrument equipment configuration.

Example: DIAG:BGIN:CAT

Queries the names of the assemblies.

Usage: Query only

:DIAGnostic:INFO:OTIMe?

The command queries the number of operation hours.

Return values:

<OTIMe> float

Example: DIAG: INFO:OTIM

queries the operation hours.

Response: 100023

The instrument was operated for 100023 hours up to now.

Usage: Query only

Manual operation: See "Operation Time / h"

#### :DIAGnostic:INFO:POCount?

The command queries the number of power-on events.

Return values:

<Pocount> float

Example: DIAG: INFO: POC

queries the number of power on events.

Response: 123

The instrument was switched on for 123 times up to now.

Usage: Query only

Manual operation: See "Power On Count" on page 98

