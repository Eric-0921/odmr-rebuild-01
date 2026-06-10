### 6.4 CALibration Subsystem

The CALibration system contains the commands for performing internal adjustment. Adjustment is triggered by the query commands.

Understanding the query response

0: error-free execution of the adjustments

- 1: indicates that an error occurred, the process has been canceled.

##### Suffix <hw>

| Suffix | Value range | Description |
|--------|-------------|-------------|
| CALibration<hw> | [1] | Optional suffix |

| Command | Page |
|---------|------|
| :CALibration:ALL[:MEASure]? | 290 |
| :CALibration<hw>:FMOFfset[:MEASure]? | 291 |
| :CALibration<hw>:FREQUency[:MEASure]? | 291 |
| :CALibration<hw>:LEVel[:MEASure]? | 291 |
| :CALibration<hw>:LEVel:EXTern:DATA | 292 |
| :CALibration:ROSCillator[:DATA] | 292 |
| [:SOURCE]:CALibration:STEReo:ANALog[:MEAS]? | 292 |

#### :CALibration:ALL[:MEASure]? [<Force>]

Starts all internal adjustments that do not require external measurement equipment.

| Property | Value |
|----------|-------|
| Query parameters: <Force> | string |
| Return values: <Measure> | 0 \| 1 \| OFF \| ON |
| Example: | CAL:ALL:MEAS? // Response "0" // Adjustment has been performed successfully |
| Usage: | Query only |

Manual operation: See "Adjust All" on page 495

#### :CALibration<hw>:FMOFfset[:MEASure]?

Starts all adjustment for the FM/PhiM modulator.

Return values:

<Measure> 0 | 1

Example: CAL:FMOF? starts the adjustments for the FM/PhiM modulator. Response: "0" the adjustments have been performed successfully

Usage: Query only

Manual operation: See "Adjust FM Offset" on page 210

#### :CALibration<hw>:FREQUency[:MEASure]?

Starts all adjustments which affect the frequency.

Return values:

<Measure> 0 | 1

Example:

CAL: FREQ: MEAS?

starts the adjustments for maximum frequency accuracy.

Response: "0"

the adjustments have been performed successfully.

Usage: Query only

Manual operation: See "Adjust Synthesis" on page 495

#### :CALibration<hw>:LEVel[:MEASure]? [<Force>]

Starts all adjustments which affect the level.

The acquired correction values improve the settling time and the signal quality.

Query parameters:

<Force> string

*RST: force

Return values:

<Measure> 0 | 1

Example:

CAL: LEV: MEAS?

starts adjustments for maximum level accuracy.

Response: "0"

adjustment has been performed successfully.

Usage: Query only

Manual operation: See "Adjust Level" on page 495

##### :CALibration<hw>:LEVel:EXTern:DATA <Data>

Queries what data has been used for the level calibration.

By default the instrument uses correction data obtained in the factory before delivery. In addition, customer data can be used for external level correction. The customer data is obtained using a R&S NRP power sensor. External level correction is a protected function (see service manual, chapter 2, "Adjustment").

Parameters:

Example: CAL:LEV:EXT:DATA FACT

selects the use of the data acquired at the factory for external level correction.

Manual operation: See "Adjustment Data" on page 152

##### :CALibration:ROSCillator[:DATA] <Data>

Sets the calibration value for the custom defined external adjustment.

Parameters:

<Data>

    integer

    Range: 0 to INT_MAX

    *RST: 0

#### [:SOURCE]:CALibration:STEReo:ANALog[:MEAS]?

The command starts all adjustments which affect the analog channels of the stereo coder option.

<Meas> 0 | 1

Example: CAL:STER:ANAL?

starts the adjustments for analog channels of the stereo coder.

Response: 0

the adjustments have been performed successfully.

Usage: Query only

Options: R&S SMB-B5

Manual operation: See "Adjust Stereo Coder" on page 496
