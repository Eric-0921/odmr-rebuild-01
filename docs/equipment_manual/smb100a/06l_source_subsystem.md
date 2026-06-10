### 6.13 SOURCE Subsystem

The SOURCE subsystem contains the commands for configuring the digital and analog signals.

##### SOURCE<hw>

For one-path instruments, the keyword SOURCe is optional and can be omitted.

- SOURce:AM Subsystem.....331    
- SOURce:CORRection subsystem.....334    
- SOURce:FM Subsystem.....343    
- SOURce:FREQUency Subsystem.....346    
- SOURce:INPUT Subsystem.....353    
- SOURce:LFOutput Subsystem.....354    
- SOURce:LIST Subsystem.....364    
- SOURce:MODulation Subsystem.....376    
- SOURce:PGEN Subsystem.....377    
- SOURce:PHASE Subsystem.....378    
- SOURce:PM Subsystem.....378    
- SOURce:POWER Subsystem.....382    
- SOURce:PULM Subsystem.....392    
- SOURce:ROSCillator Subsystem.....409    
- SOURce:STEReo Subsystem.....412    
- SOURce:SWeep Subsystem.....421

#### 6.13.1 SOURCE:AM Subsystem

The AM subsystem contains the commands for setting the amplitude modulation.

An external modulation signal is input at the [MOD EXT] connector.

The settings for the internal modulation source (LF generator) are made in the SOURce:LFOutput subsystem.

[:SOURce<hw>]:AM:DEPTh:EXPonential.....331    
[:SOURce<hw>]:AM:DEPTh:LINear.....332    
[:SOURce<hw>]:AM:EXTERNAL:COUPling.....332    
[:SOURce<hw>]:AM:SENSitivity?.....332    
[:SOURce<hw>]:AM:SOURce.....333    
[:SOURce<hw>]:AM:STATE.....333    
[:SOURce<hw>]:AM:TYPE.....334

#### [:SOURCE<hw>]:AM:DEPTH:EXPonential <DepthExp>

Sets the overall modulation depth of the amplitude modulation in dB.

Note: The exponential AM mode applies to instruments with frequency option 12 GHz or higher. You can select this mode with command [ : SOURce<hw> ] :AM:TYPE. For more details, see also the GUI reference, Chapter 4.4.2, "Amplitude Modulation (AM)", on page 204.

##### Parameters:

<DepthExp>

float

Range: -40 to 40

Increment: 0.01


| Example: | AM:DEPT:LIN 15PCT sets the AM modulation depth to 15 percent. |
| Options: | (exponential): R&S SMB-B112/-B112L/-B120/-B120L/-B140/-B140L |
| Manual operation: | See "AM Depth" on page 205 |

#### [:SOURCE<hw>]:AM:DEPTH:LINear <DepthLin>

Sets the overall modulation depth of the amplitude modulation in percent.

Note: For high frequency instruments, you can alternatively select exponential amplitude modulation with command [ : SOURce<hw> ] :AM:TYPE. In this case, the generator sets modulation depth in dB (logarithmic).

For more details, see also the GUI reference, Chapter 4.4.2, "Amplitude Modulation (AM)", on page 204.

Parameters:
<DepthLin> float
Range: 0 to 100
Increment: 0.1
*RST: 30
Example: AM:DEPT:LIN 15 sets the AM modulation depth to 15 dB.
Manual operation: See "AM Depth" on page 205

#### [:SOURCE<hw>]:AM:EXTERNAL:COUPLING <Coupling>


| Selects the coupling mode for the external amplitude modulation signal. |  |
| Parameters: |  |
| <Coupling> | AC \| DC |
|  | AC |
|  | Uses only the AC signal component of the modulation signal. |
|  | DC |
|  | Uses the modulation signal as it is, with AC and DC. |
|  | *RST: AC |
| Example: | AM:EXT:COUP AC |
|  | selects the coupling mode AC for external amplitude modulation. |
| Manual operation: | See "Mod Ext Coupling" on page 206 |

#### [:SOURCE<hw>]:AM:SENSitivity?

Queries the input sensitivity of the externally applied signal for amplitude modulation.

The sensitivity depends on the set modulation depth, see [ : SOURce<hw>] :AM: DEPTh:LINear and [ : SOURce<hw>] :AM: DEPTh:EXPonential.

The returned value reports the sensitivity in \%/V. It is assigned to the voltage value for full modulation of the input.

##### Return values:

<Sensitivity> float

Range: 0 to 100

Example: AM: DEPT 50

sets a modulation depth of 50 %.

AM: SENS?

queries the input sensitivity at the external modulation input.

Response: 50

since the voltage value for full modulation is 1V, the resulting sensitivity is precisely 50 %/V.

Usage: Query only

Manual operation: See "AM Sensitivity" on page 206

#### [:SOURCE<hw>]:AM:SOURCE <Source>

Selects the modulation signal source for amplitude modulation.

With linear AM (see[:SOURCE<hw>]:AM:TYPE on page 334), you can use both, the internal and an external modulation signal at a time, for example to perform two-tone AM.

##### Parameters:

<Source>

    INTernal | EXternal | INT,EXT

    INTernal

    Uses the internally generated signal for modulation. To configure the frequency, use the commands of the Chapter 6.13.6, "SOURce:LFOutput Subsystem", on page 354 subsystem.

    EXTernal

    Uses an externally applied modulation signal.

    INT,EXT

    Uses both, the internal and external modulation signals.

    *RST: INT

Example:

    AM: SOUR INT

    selects the internal modulation source

Manual operation: See "AM Source" on page 205

#### [:SOURCE<hw>]:AM:STATE <State>

Activates amplitude modulation.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: AM:STAT ON

activates AM modulation.

Manual operation: See "State" on page 205

[:SOURCE<hw>]:AM:TYPE <AmType>

Selects exponential or linear amplitude modulation.

Exponential amplitude modulation is available for instruments, equipped with 12 GHz or higher frequency options. For more details, see also the GUI reference Chapter 4.4.2, "Amplitude Modulation (AM)", on page 204.

Parameters:

<AmType> LINear | EXPonential

*RST: LINear

Example: AM:TYPE EXP

activates the exponential amplitude modulation.

Options: (exponential): R&S SMB-B112/-B112L/-B120/-B120L/-B140/-B140L

Manual operation: See "AM Type" on page 205

#### 6.13.2 SOURCE: CORRECTION subsystem

The output level is corrected in the CORRection subsystem. Correction is performed by user-defined table values being added to the output level for the respective RF frequency. In the R&S SMB, this subsystem is used to select, transfer and activate user correction tables.

Each list is stored as a file. The name of the user correction file can be freely selected. The file extension *.uco is assigned automatically and cannot be changed.

The files can be stored in a freely selectable directory and opened from there. The default directory is set using command :MMEMory:CDIRectory on page 309. In the case of files which are stored in the default directory, only the file name has to be specified in commands. Otherwise, the complete absolute path has to be specified with every command. The extension can be omitted in any case.

In the following command examples, the files are stored in the default directory.

The amplitude can also be linearized automatically by means of a R&S NRP power sensor connected to the generator output signal. With the aid of the command [ : SOURce<hw>] : CORRection:CSET:DATA[ : SENSor<ch>] [ : POWer] : SONCe, a list with correction values for external test assemblies can be automatically determined, e.g. for compensating the frequency response of cables. The correction values can be acquired any time irrespective of the modulation settings of the generator.

[:SOURCE]:CORRection:CSET:CATalog?.....335    
[:SOURCE<hw>]:CORRection:CSET:DATA:FREQUency.....335    
[:SOURCE<hw>]:CORRection:CSET:DATA:FREQUency:POINts?.....336    
[:SOURCE<hw>]:CORRection:CSET:DATA:POWER.....336    
[:SOURCE<hw>]:CORRection:CSET:DATA:POWER:POINts?.....336    
[:SOURCE<hw>]:CORRection:CSET:DATA:SENSor<ch>][:POWER]:SONCe.....337    
[:SOURCE]:CORRection:CSET:DELETE.....337    
[:SOURCE<hw>]:CORRection:DEXChange:AFILe:CATalog?.....338    
[:SOURCE<hw>]:CORRection:DEXChange:AFILe:EXTension.....338    
[:SOURCE<hw>]:CORRection:DEXChange:AFILe:SELECT.....338    
[:SOURCE<hw>]:CORRection:DEXChange:AFILe:Separator:COLumn.....339    
[:SOURCE<hw>]:CORRection:DEXChange:AFILe:Separator:DECimal.....339    
[:SOURCE<hw>]:CORRection:DEXChange:EXECute.....340    
[:SOURCE<hw>]:CORRection:DEXChange:MODE.....340    
[:SOURCE<hw>]:CORRection:DEXChange:SELECT.....341    
[:SOURCE<hw>]:CORRection:CSET:SELECT.....341    
[:SOURCE<hw>]:CORRection:STATE.....342    
[:SOURCE<hw>]:CORRection:VALUE?.....342    
[:SOURCE<hw>]:CORRection:ZEROing:STATE.....342

##### [:SOURCE]:CORRection:CSET:CATalog?

Requests a list of user correction tables. The individual lists are separated by commas.

The lists are stored with the fixed file extensions *.uco in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

Return values:

<Catalog> string

Example: MMEM:CDIR '/var/user/ucor'

selects the directory for the user correction files.

CORR:CSET:CAT?

queries which correction tables are available.

Response:UCOR1, UCOR2, UCOR3

the correction tables UCOR1, UCOR2 and UCOR3 are available.

Usage: Query only

Manual operation: See "Directory, File List and File Name" on page 129

#### [:SOURCE<hw>]:CORRection:CSET:DATA:FREQUency <Frequency>

Transfers the frequency data to the table selected with :CORRection:CSET:SELECT.

The numerical suffix at SOURce must not be used for this command.

##### Parameters:

<Frequency>

Frequency#1[, Frequency#2, ...]

Range: 300 kHz to RFmax (depending on model)

Example: CORR:CSET '/var/user/ucor1'
selects the table ucor1.
CORR:CSET:DATA:FREQ 100MHz,102MHz,103MHz, ...
enters the frequency value in the table ucor1.
Manual operation: See "Edit User Cor. Data - User Correction" on page 161

##### [:SOURCE<hw>]:CORRection:CSET:DATA:FREQUency:POINts?

Queries the number of frequency values in the selected table.

The numerical suffix at SOURce must not be used for this command.

Return values:

    integer
    Range: 0 to 10000
    *RST: 0

Example:
    CORR:CSET '/var/user/'
    selects the table ucor1.
    CORR:CSET:DATA:FREQ:POIN?
    queries the number of frequency values in the table ucor1.
    Response: 440
    the table ucor1 contains 440 frequency values.

Usage:
Query only

##### [:SOURCE<hw>]:CORRection:CSET:DATA:POWER 

Transfers the level data to the table selected with [:SOURce<hw>]:CORRection:CSET[:SELECT].

*RST does not affect data lists. The numerical suffix at SOURce must not be used for this command.

##### Parameters:


    Power#1[, Power#2, ...]
    Example:
        CORR:CSET '/var/user/ucor1'
        selects the table ucor1.
        CORR:CSET:DATA:POW 1dB, 0.8dB, 0.75dB, ...
        enters the level values in the table ucor1.

Manual operation: See "Edit User Cor. Data - User Correction" on page 161

##### [:SOURCE<hw>]:CORRection:CSET:DATA:POWER:POINTs?

Queries the number of level values in the selected table.

The numerical suffix at SOURCE must not be used for this command.

Return values:

    integer
    Range: 0 to 10000
    *RST: 0

Example:
    CORR:CSET '/var/user/ucor1'
    selects the table ucor1.
    CORR:CSET:DATA:POW:POIN?
    queries the number of level values in the table ucor1.
    Response: 440
    the table ucor1 contains 440 level values.

Usage:
Query only

#### [:SOURCE<hw>]:CORRection:CSET:DATA[:SENSOR<ch>][:POWER]:SONCe

The command fills the selected user correction list with the level values measured by the power sensor for the given frequencies.

To select the used power sensor set the suffix in key word SENSE.


| Example: | CORR:CSET:DATA:SENS:POW:SONCfills the user correction list with level values acquired by the power sensor connector to the [SENSOR] connector. |
| Usage: | Event |
| Manual operation: | See "Fill User Correction Data with Sensor" on page 166 |

##### [:SOURCE]:CORRection:CSET:DELete <Filename>

Deletes the specified table.

The lists are stored with the fixed file extensions *.uco in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR. A path can also be specified in command SOUR:CORR:CSET:CAT?, in which case the file in the specified directory is deleted.

##### Setting parameters:

<Filename>

| LNOise |  |
| Provides phase noise and spurious characteristics close to CW. The range for modulation bandwidth and FM deviation is reduced (see data sheet). |  |
| HDEViation |  |
| Provides full setting range for FM deviation. The range of modulation bandwidth is reduced (see data sheet). |  |
| *RST: NORM |  |
| Example: FM:MODE NORM |  |
| Manual operation: See "FM Mode" on page 209 |  |

##### [:SOURCE<hw>]:FM:SENSitivity?

Queries the input sensitivity of the externally applied signal for frequency modulation. The returned value reports the sensitivity in Hz/V. It is assigned to the voltage value for full modulation of the input signal.

The sensitivity depends on the set [ : SOURce<hw>]:FM[:DEViation].

##### Return values:

<Sensitivity> float

Range: 0 to max

Increment: 0.01

Example: FM: DEV 5E3 sets a modulation deviation of 5 kHz. FM: SENS queries the input sensitivity at the external modulation input. Response: 5E3 since the voltage value for full modulation is 1V, the resulting sensitivity is precisely 5000 Hz/V.

Usage: Query only

Manual operation: See "FM Sensitivity" on page 210

##### [:SOURCE<hw>]:FM:SOURCE <Source>

Selects the modulation signal source for frequency modulation.

##### Parameters:

<Source> INTERNAL | EXTERNAL | INT,EXT

Uses the internally generated signal for modulation. To configure the frequency, use the commands of the Chapter 6.13.6, "SOURCE:LFOutput Subsystem", on page 354 subsystem.

##### EXT

INT,EXT

Uses both, the internal and external modulation signals.

*RST: INT

Example: FM: SOUR INT

selects the internal modulation source.

Manual operation: See "FM Source" on page 208

[:SOURCE<hw>]:FM:STATe <State>

Activates frequency modulation.

Note: Activation of FM deactivates phase modulation (PM).

Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: FM: STAT ON

Activates FM modulation.

Manual operation: See "State" on page 208

#### 6.13.4 SOURce:FREQUency Subsystem

This subsystem contains the commands used to define the frequency settings for the RF sources and sweeps.

[:SOURCE<hw>]:FREQUENCY:CENTer.....346

[:SOURCE<hw>]:FREQUENCY[:CW|FIXed].....347

[:SOURCE<hw>]:FREQUENCY[:CW|FIXed]:RCL.....348

[:SOURCE<hw>]:FREQUENCY:MANual.....348

[:SOURCE<hw>]:FREQUENCY:MODE.....349

[:SOURCE<hw>]:FREQUENCY:MULTIPlier.....350

[:SOURCE<hw>]:FREQUENCY:OFFSET.....350

[:SOURCE<hw>]:FREQUENCY:SPAN.....350

[:SOURCE<hw>]:FREQUENCY:START.....351

[:SOURCE<hw>]:FREQUENCY:STOP.....351

[:SOURCE<hw>]:FREQUENCY:STEP[:INCRement].....352

[:SOURCE<hw>]:FREQUENCY:STEP:MODE.....353

##### [:SOURCE<hw>]:FREQUENCY:CENTER 

Sets the center frequency of the RF sweep range.

The range is defined by this center frequency and the specified[:

FREQUency:SPAN, according to the formula:

 $ f_{\text{CENTER}} - (f_{\text{SPAN}}/2) \ldots f_{\text{CENTER}} + (f_{\text{SPAN}}/2) $

with:

 $ f_{\text{SPAN}} = f_{\text{STOP}} - f_{\text{START}} $

The center frequency directly relates to the span, and the start and stop frequencies. If you change one of these parameters, the center frequency changes accordingly.

 $ f_{\text{CENTER}} = (f_{\text{STOP}} + f_{\text{STARt}})/2 $

Note: You can select any frequency within the setting range. The range is defined with the parameters [: SOURce<hw>] : FREQuency: STARt and [: SOURce<hw>] : FREQuency: STOP.

A defined offset and the multiplier factor affect the sweep frequency range and therefore all correlated parameters. The set frequencies are only absolute values, if the offset = 0 and the multiplication factor = 1. The multiplier multiplies the frequencies accordingly, and the offset ≠ 0 shifts the frequencies corresponding to the set value.

300 kHz * f_{MULTIPlier} + f_{OFFSET} ... f_{max} * f_{MULTIPlier} + f_{OFFSET}

Parameters:

    float
    Range: full frequency range
    Increment: see the data sheet: RF characteristics > Resolution of setting
    *RST: depends on model

Example:
    FREQ: CENT 400 MHz
    sets the center frequency for the frequency sweep to 400 MHz.
    FREQ: SPAN 200 MHz
    sets a span of 200 MHz. This sets the sweep range to 300 MHz to 500 MHz.

Manual operation: See "Center Freq - Frequency Sweep" on page 184

##### [:SOURCE<hw>]:FREQUENCY[:CW|FIXed] <Fixed>

Sets the frequency of the RF output signal.

In CW mode, see FREQ:MODE CW|FIXED, the instrument operates at a fixed frequency.

In sweep mode FREQ:MODE SWE, the value applies to the sweep frequency and the instrument processes the frequency settings in defined sweep steps.

You can enter either a numerical frequency value, of decrease or increase the current frequency step by step with FREQ UP and FREQ DOWN. The frequency is then increased or decreased by the value[:[：SOURce<hw>]:FREQUency:STEP[：INCREment] in FREQ:STEP:MODE USER.

##### Note:

A defined offset and the multiplier factor affect the sweep range and therefore all correlated parameters. The set frequencies are only absolute values, if the offset = 0 and the multiplication factor = 1. The multiplier multiplies the frequencies accordingly, and the offset  $ \neq $ 0 shifts the frequencies corresponding to the set value.

The actual frequency at the RF output does not change, but rather the value queried with [:SOUR] :FREQ?, according to the formula:

 $ f_{\text{FREQ}} = f_{\text{RFout}} * f_{\text{MULTIPlier}} + f_{\text{OFFSet}} $

Correlation: FREQ for FREQ:MODE SWE is linked to the sweep frequency.

##### Parameters:

<Fixed>

    float

    Range: full frequency range

    Increment: see the data sheet: RF characteristics > Resolution of setting

    *RST: 100 MHz

Example: FREQ 500kHz

sets the frequency of RF output signal A to 500 kHz.

Manual operation: See "RF Freq" on page 139

#### [:SOURCE<hw>]:FREQUENCY[:CW|FIXed]:RCL <Rcl>

Determines whether the RF frequency value is retained or taken from a loaded instrument configuration, when you recall instrument settings with the command *RCL.

Parameters:
<Rcl>
    INCLUDE | EXCLUDE
    INCLUDE
    Takes the frequency value of the loaded settings.
    EXCLUDE
    Retains the current frequency when an instrument configuration is loaded.
    *RST: | INCLUDE
Example:
    FREQ:RCL | INCL
    takes the frequency from the loaded instrument configuration.

Manual operation: See "Exclude Frequency" on page 131

#### [:SOURCE<hw>]:FREQUENCY:MANual <Manual>

Determines the frequency and triggers a sweep step manually in SWE:MODE MAN.

Note: You can select any frequency within the setting range. The range is defined with the parameters [: SOURce<hw>] : FREQuency: STARt and [: SOURce<hw>] :

FREQUENCY: STOP. A defined offset and the multiplier factor affect the sweep range and therefore all correlated parameters. The set frequencies are only absolute values, if the offset = 0 and the multiplication factor = 1. The multiplier multiplies the frequencies accordingly, and the offset  $ \neq $ 0 shifts the frequencies corresponding to the set value.

 $ f_{\text{START}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} \ldots f_{\text{STOP}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} $

##### Parameters:

<Manual> float

Range: full frequency range

Increment: see the data sheet: RF characteristics > Resolution of setting

Example: SWE:MODE MAN

sets the Step sweep mode.

Example: FREQ:MODE SWE

sets the frequency sweep mode. The sweep start frequency is output.

FREQ:MAN UP

triggers the next higher sweep step.

FREQ:MAN 500MHz

outputs 500 MHz RF frequency (must e within the sweep fre-

quency range).

FREQ:MAN DOWN

triggers the next lower sweep step relative to 500 MHz.

Manual operation: See "Current Freq - Frequency Sweep" on page 184

##### [:SOURce<hw>]:FREQUency:MODE <Mode>

Selects the frequency mode for the generating the RF output signal. The selected mode determines the parameters to be used for further frequency settings.

##### Parameters:

<Mode>

##### CW|FIXed

Sets the fixed frequency mode.

CW and FIXed are synonyms. The instrument operates at a defined frequency, set with command [ : SOURce<hw> ] : FREQuency [ : CW | FIXed].

##### SWEep

Sets the sweep mode. The instrument processes the frequency settings in defined sweep steps. To determine the corresponding frequency values, use the commands [ : SOURce<hw> ]:

FREQUency:STARt and [ : SOURce<hw> ]:FREQUency:STOP, or [ : SOURce<hw> ]:FREQUency:CENTer and [ : SOURce<hw> ]:FREQUency:SPAN and [ : SOURce<hw> ]:FREQUency:MANual.

##### LIST

Sets the list mode. The instrument processes the frequency and level settings by means of values loaded from a list.

To configure the list mode settings use the commands of the SOURce:LIST Subsystem

*RST: CW

*RST: CW

Example: FREQ:MODE SWE sets the SWEep mode.
Example: FREQ:MODE CW turns off the SWEep or LIST mode.
Manual operation: See "State - Frequency Sweep" on page 180

#### [:SOURCE<hw>]:FREQUENCY:MULTIPlier <Multiplier>

Sets the value for the multiplication factor of a subsequent downstream instrument.

Parameters:
<Multiplier> float
Range: 1 to dynamic
Increment: 0.001
*RST: 1
Example: FREQ:MULT 1
sets the multiplication factor to 1.
Manual operation: See "Multiplier" on page 141

#### [:SOURCE<hw>]:FREQUENCY:OFFSET<Offset>

Sets the frequency offset of a downstream instrument, for example a mixer.

If you have specified an OFFSet and / or a MULTiplier factor, the actual frequency at the RF output does not change, but rather the value queried with [:SOUR]:FREQ?, according to the following formula:

 $ f_{\text{FREQ}} = f_{\text{RFout}} * f_{\text{MULTIPlier}} + f_{\text{OFFSet}} $

Parameters:
<Offset> float
Increment: 0.01
*RST: 0
Example: FREQ:OFFS 500kHz sets the frequency offset to 500 kHz.
Manual operation: See "Offset" on page 140

##### [:SOURCE<hw>]:FREQUENCY:SPAN 

Determines the extent of the frequency sweep range. This setting in combination with the center frequency setting ([ : SOURce<hw>]: FREQuency: CENTER) defines the sweep range.

This parameter is related to the start and stop frequencies. If you change the frequency, the span changes accordingly.

 $ f_{\text{SPAN}} = f_{\text{STOP}} - f_{\text{START}} $

 $ f_{\text{START}} > f_{\text{STOP}} $ is permitted.

##### Parameters:

Parameters:

    float
    Range: full frequency range
    Increment: see the data sheet: RF characteristics > Resolution of setting
    *RST: 400E6

Example:
    FREQ: CENT 400 MHz
    sets the center frequency of the frequency sweep to 400 MHz.
    FREQ: SPAN 200 MHz
    sets a span of 200 MHz. This sets the sweep range to 300 MHz to 500 MHz.

Manual operation: See "Span - Frequency Sweep" on page 184

##### [:SOURCE<hw>]:FREQUENCY:START <Start>

Sets the start frequency for the RF sweep.

This parameter relates to the center frequency and span. If you change the frequency, these parameters change accordingly.

 $ f_{\text{START}} > f_{\text{STOP}} $ is permitted.

 $f_{START} = (f_{CENTER} - f_{SPAN}/2).$ 

Note: A defined offset and the multiplier factor affect the sweep range and therefore all correlated parameters. The set frequencies are only absolute values, if the offset = 0 and the multiplication factor = 1. The multiplier multiplies the frequencies accordingly, and the offset  $ \neq $ 0 shifts the frequencies corresponding to the set value.

 $ f_{\text{START}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} \ldots f_{\text{STOP}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} $

Parameters:
<Start> float
Range: full frequency range
Increment: see the data sheet: RF characteristics > Resolution of setting
*RST: 100 MHz
Example: FREQ:START 1 MHz
sets the start frequency for the frequency sweep to 1 MHz.
FREQ:STOP 2 GHz
sets the stop frequency for the frequency sweep to 2 GHz.
Manual operation: See "Start Freq - Frequency Sweep" on page 183

##### [:SOURCE<hw>]:FREQUENCY:STOP <Stop>

Sets the stop frequency for the RF sweep.

This parameter is related to the center frequency and span. If you change the frequency, these parameters change accordingly.

 $ f_{\text{START}} > f_{\text{STOP}} $ is permitted.

 $ f_{\text{STOP}} = (f_{\text{CENTER}} + f_{\text{SPAN}}/2). $

Note: A defined offset affects the sweep range and consequently all correlating parameters. The set frequencies are only absolute values, if the Offset = 0. Offset ≠ 0 shifts the frequencies according to the offset value.

 $ f_{\text{STARt}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} \ldots f_{\text{STOP}} \times f_{\text{MULTIPlier}} + f_{\text{OFFSET}} $

##### Parameters:

<Stop> float

Range: full frequency range

Increment: see the data sheet: RF characteristics > Resolution of setting

*RST: 500 MHz

Example: FREQ:STOP 2 GHz

sets the stop frequency for the frequency sweep to 2 GHz.

FREQ:STAR 1 MHz

sets the start frequency for the frequency sweep to 1 MHz.

Manual operation: See "Stop Freq - Frequency Sweep" on page 184

##### [:SOURCE<hw>]:FREQUENCY:STEP[:INCRement] 

Sets the step width for FREQ: STEP:MODE USER.

To adjust the frequency step by step with this step size, use the FREQ:UP and FREQ:DOWN commands.

Note: This value also applies to the step width of the rotary knob of the instrument and increases or decreases the frequency accordingly, when you work in user-defined step mode.

##### Parameters:

 float

Range: full frequency range

Increment: see the data sheet: RF characteristics > Resolution of setting

*RST: 1E6

Example: FREQ: STEP 50 kHz

sets the step width for the frequency setting to 50 kHz.

Manual operation: See "Variation Step" on page 141

#### [:SOURCE<hw>]:FREQUENCY:STEP:MODE <Mode>

Activates (USER) or deactivates (DECimal) the user-defined step width used when varying the frequency value with the frequency values UP/DOWN. The command is linked to the command "Variation Active" for manual control, i.e. the command also activates/deactivates the user-defined step width used when varying the frequency value with the rotary knob.

Parameters:

<Mode> DECimal | USER

*RST: DECimal

Example: FREQ: STEP 50 kHz

sets the step width for the frequency setting to 50 kHz.

FREQ: STEP: MODE USER

actives this step width for frequency variation with the rotary knob (manual control) and with frequency values UP/DOWN (remote control).

Manual operation: See "Variation Active" on page 141

#### 6.13.5 SOURCE:INPUT Subsystem

The SOURce: INPut subsystem contains the commands for configuring the inputs for external modulation signals. The instrument trigger setting influences all sweeps and is effective in the List mode (Instrument Trigger).

[:SOURce<hw>]:INPUT:MODext:IMPedance.....353

[:SOURce<hw>]:INPUT:MODext:WIGNore.....353

[:SOURce]:INPUT:TRIGger:SLOPe.....354

##### [:SOURCE<hw>]:INPUT:MODExt:IMPedance 

Sets the impedance for an externally applied modulation signal.

##### Parameters:

 HIGH | G600

HIGH

> 100 k0hm to ground

*RST: HIGH

Example: INP:MOD:IMP HIGH sets > 100 kOhm to ground.

Manual operation: See "Ext. Impedance" on page 207

#### [:SOURCE<hw>]:INPUT:MODExt:WIGNore <SuppressState>

Ignores warnings concerning an overload of the modulation signal input.

This setting is not affected by an instrument preset *rst or the and Save/Recall function. Only the factory preset resets this setting.

Parameters:

<SuppressState> 0 | 1 | OFF | ON

*RST: 0

Example: INP:MOD:WIGN ON suppresses the overvoltage warnings.

Manual operation: See "Ignore Overvoltage Warning" on page 207

##### [:SOURCE]:INPUT:TRIGger:SLOPe <Slope>

Sets the polarity of the active slope of an externally applied trigger signal at the trigger input (BNC connector at the rear of the instrument).

The setting is effective for both inputs at the same time.

##### Parameters:

<Slope> NEGative | POSitive

*RST: POSitive

Example: INP:TRIG:SLOP NEG

Activates the falling slope of the external trigger signal at the trigger input.

Manual operation: See "Ext. Trigger Input Slope" on page 187

#### 6.13.6 SOURce:LFOutput Subsystem

The SOURce: LFOutput subsystem contains the commands for setting the LF signal source in CW and Sweep mode and for analog modulation.

##### Example

The following example shows how to set an LF sweep.

1. Set the sweep range.

LFOutput: FREQuency: STARt 4 kHz

LFOutput: FREQuency: STOP 10 kHz

2. Select linear or logarithmic sweep spacing.

LFOutput: SWEep[:FREQUency]:SPACing LIN

3. Set the step width and dwell time.

LFOutput:SWEep[:FREQUency]:STEP[:LINEAR] 100 Hz

LFOutput:SWEep[:FREQUency]:DWELL 20 ms

4. Determine the sweep mode.

LFOutput: SWEep:MODE AUTO

5. Determine the trigger.

TRIGGER0:SOURCE SINGLE

6. Activate the sweep.

LFOutput: FREQuency:MODE SWEep

7. Trigger the sweep (depending on the mode).

LFOutput: SWEep: EXECute


| [:SOURCE]:LFOutput<ch>:FREQUency | 355 |
| [:SOURCE<hw>]:LFOutput:FREQUency:MANual | 356 |
| [:SOURCE<hw>]:LFOutput:FREQUency:MODE | 356 |
| [:SOURCE<hw>]:LFOutput:FREQUency:STARt | 357 |
| [:SOURCE<hw>]:LFOutput:FREQUency:STOP | 357 |
| [:SOURCE]:LFOutput[:STATE] | 357 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:DWELI | 358 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:EXECute | 358 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:MODE | 358 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:POINts | 359 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:RETRace | 360 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:RUNNING? | 360 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:SHAPe | 361 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:SPACing | 361 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:STEP[:LINear] | 361 |
| [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:STEP:LOGarithmic | 362 |
| [:SOURCE]:LFOutput:SHAPe | 363 |
| [:SOURCE]:LFOutput:SIMPedance | 363 |
| [:SOURCE]:LFOutput:VOLTAGE | 364 |

#### [:SOURCE]:LFOutput<ch>:FREQUency <Frequency>

Sets the frequency of the LF signal in LFO:FREQ:MODE CW|FIXED mode.

##### Note:

- If signal source "Internal" is set, the instrument performs the analog modulations (AM/FM/φM/PM) with this frequency.

- In sweep mode (LFO: FREQ: MODE SWEep), the frequency is coupled with the sweep frequency.

##### Parameters:

<Frequency> float

Range: full frequency range

Increment: see the data sheet: Modulation sources > Resolution of frequency setting

*RST: 1000

Example: LFO2: FREQ 5kHz

sets the frequency of the LF generator 2 signal to 5 kHz.

Manual operation: See "LF Gen Freq" on page 206

#### [:SOURCE<hw>]:LFOutput:FREQUency:MANual <Manual>

Determines the frequency and triggers the next sweep step manually in LFO:SWE[:FREQ]:MODE MAN, and LFO:SWE:[FREQ]:MODE STEP.

Note: You can select any frequency within the setting range. The range is defined with LFO:FREQ:START and LFO:FREQ:STOP.

Parameters:
<Manual> float
Range: full frequency range
Increment: see the data sheet: Modulation sources > Internal modulation generator > Resolution of frequency setting
*RST: 1000
Example: LFO: SWE:MODE MAN sets the "Step" sweep mode.
LFO: FREQ:MAN 5 kHz sets an LF frequency of 5 kHz for the next step in the "Step" sweep mode.
LFO: FREQ:MODE SWE sets the LF Sweep mode. An LF frequency of 5 kHz is output.
LFO: FREQ:MAN 5.1 kHz triggers the next sweep step with a frequency of 5.1 kHz.
Manual operation: See "Current Freq" on page 229

#### [:SOURCE<hw>]:LFOutput:FREQUency:MODE <Mode>

function

Example: LFO: FREQ: MODE SWE sets the sweep mode.

Manual operation: See "State" on page 226

[:SOURCE<hw>]:LFOutput:FREQUency:STARt <Start>

Sets the start frequency for the LF sweep.

Parameters:
<Start>
    float
    Range: full frequency range
    Increment: see the data sheet: Resolution of frequency setting
    *RST: 1 KHz

Example:
    RST*
    activates all presettings.
    LFO: SWE:MODE AUTO
    TRIGO: SOUR SING
    LFO: FREQ:STAR 1 kHz
    LFO: FREQ:STOP 10 kHz
    LFO: FREQ:MODE SWE
    LFO: SWE:EXEC
    the instrument generates a single sweep cycle from 1 kHz to 10 kHz automatically after a manual trigger event occurs
    (:LFOutput:SWEep:EXECute or *TRG). The step width is 1 kHz linear, with 15 ms dell time until the signal switches to the subsequent step.

Manual operation: See "Start Freq" on page 229

[:SOURce<hw>]:LFOutput:FREQUency:STOP <Stop>

Sets the stop frequency for the LF sweep.

Parameters:
<Stop> float
Range: full frequency range
Increment: see the data sheet: resolution of frequency setting
*RST: 100 KHz
Example: LFO: FREQ: STOP 10 kHz
sets the stop frequency for the LF sweep to 10 kHz.
Manual operation: See "Stop Freq" on page 229

[:SOURCE]:LFOutput[:STATE] <State>
Activates/deactivates the LF output.
Parameters:
<State> 0 | 1 | OFF | ON
*RST: 0

Example: LFO ON

activates the LF output. The settings under LFO: FREQ and LFO: SWE become effective.

Manual operation: See "LF Output State" on page 224

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:DWELI <Dwell>

Sets the dwell time for each frequency step of the sweep.

Tip: It is recommended to switch off the "Display Update" for optimum sweep performance especially with short dwell times (SYSTEM:DISPLAY:UPDATE OFF).

##### Parameters:

<Dwell> float

Range: see data sheet: Dwell time setting range

Increment: 100E-6

*RST: 15E-3

Example: LFO:SWE:DWEL 20 ms sets a dwell time of 20 ms.

Manual operation: See "Dwell Time - LF Sweep" on page 230

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:EXECute

Immediately starts an LF frequency sweep in LFO:SWE:MODE SINGle.

Example: LFO: SWE:MODE SING

sets the single cycle mode of the LF sweep.

LFO: SWE:EXEC

starts one cycle of the LF sweep.

Usage: Event

Manual operation: See "Execute Single Sweep" on page 228

##### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:MODE <Mode>

Sets the cycle mode of the LF sweep.

The assignment of the GPIB commands to the sweep modes is given in the description of the sweep dialogs.

##### Parameters:

<Mode>

  AUTO | MANual | STEP

  AUTO

  Performs a complete sweep cycle from the start to the end value when a trigger event occurs.

  The dwell time determines the time period for the signal to switch to the next step.

##### MANual

MANual
Performs a single sweep step when a manual trigger event occurs.
The trigger system is not active. You can trigger each frequency step of the sweep individually with the command [: SOURce<hw>] : LFOutput : FREQuency : MANual. In manual mode, use the rotary knob for switching to the next step.
With each step, the frequency increases by the value specified with the command [: SOURce<hw>] : LFOutput : SWEep[ : FREQuency] : STEP[ : LINear] or [ : SOURce<hw>] : LFOutput : SWEep[ : FREQuency] : STEP : LOGarithmic, respectively. A frequency value, entered with [ : SOURce<hw>] : LFOutput : FREQuency : MANual takes no effect.
With manual control, the frequency increases or decreases (depending on the direction of the rotary encoder) by the value specified under SOUR: LFO: SWE: FREQ: STEP: LIN (linear spacing) or ... : STEP: LOG (logarithmic spacing).
STEP
Each trigger triggers one sweep step only. The frequency increases by the value entered with [ : SOURce<hw>] : LFOutput : SWEep[ : FREQuency] : STEP[ : LINear] or [ : SOURce<hw>] : LFOutput : SWEep[ : FREQuency] : STEP : LOGarithmic.
*RST: AUTO
Example: LFO: SWE: MODE AUTO
selects Auto mode.
Manual operation: See "Mode" on page 226

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:POINTs 

Determines the number of steps for the LF frequency sweep within the sweep range.

This parameter always applies to the currently set sweep spacing and correlates with the step size as follows:

- for linear sweeps and  $ f_{\text{STARt}} < f_{\text{STOP}} $
    freq_points = ( $ f_{\text{SPAN}} / \text{step\_lin} $) + 1
    with  $ f_{\text{SPAN}} = f_{\text{STOP}} - f_{\text{STARt}} $
    To determine the step size, use the command SWE:STEP[:LIN].

- logarithmic sweeps and  $ f_{\text{STARt}} < f_{\text{STOP}} $

freq_points = ((log  $ f_{\text{STOP}} $ - log  $ f_{\text{STARt}} $) / log step_log) + 1

To determine the logarithmic step size, use the command SWE:STEP:LOG.

If you change the number of sweep points, the step size changes accordingly. The sweep range remains the same.

Each sweep spacing mode has assigned the POINTs setting separately. Thus, the command refers always to the particular set mode, see [ : SOURce<hw>]:LFOutput: SWEep[:FREQUency]:SPACing.

##### Parameters:

Example: LFO: FREQ: STAR

sets the start frequency to 2 kHz.

LFO: FREQ: STOP

sets the stop frequency to 20 kHz

LFO: SWE: SPAC LIN

sets linear sweep spacing.

LFO: SWE: POIN 11

sets 11 sweep steps for linear sweep spacing. The sweep step width (STEP) is automatically set to 2 kHz.

[:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:RETRace <State>

Activates that the signal changes to the start frequency value while it is waiting for the next trigger event.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single".

##### Parameters:

<State>

0 | 1 | OFF | ON

*RST: 0

Example: TRIGO: SWE: SOUR SING

LFO: SWE: MODE SWE

LFO: SWE: SHAP SAWT

LFO: SWE: RETR ON

activates retrace function, that menas the frequency changes to the value at start frequency while waiting for the next trigger event.

Manual operation: See "Retrace - LF Frequency Sweep" on page 230

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:RUNNING?

Queries the current status of the LF frequency sweep mode.

Return values:

<State> 0 | 1 | OFF | ON

Example: LFO: SWE: RUNN?

Response "1": the frequency sweep is running.

Usage: Query only

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:SHAPe <Shape>

Sets the cycle mode for a sweep sequence (shape).

Parameters:

<Shape> SAWTooth | TRIangle

SAWTooth

A sweep runs from the start to the stop frequency. A subsequent sweep starts at the start frequency, that menas the shape of the sweep sequence resembles a sawtooth.

TRIangle

A sweep runs from the start to the stop frequency and back, that menas the shape of the sweep resembles a triangle. A subsequent sweep starts at the start frequency.

*RST: SAWTooth

Example: SOUR:LFO:SWE:SHAP TRI

selects the sweep cycle with alternating ascending and descending sweep directions.

Manual operation: See "Shape" on page 229

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:SPACing <Spacing>

Selects the mode for the calculation of the frequency sweep intervals. The frequency increases or decreases by this value at each step.

##### Parameters:

<Spacing>

    LINear | LOGarithmic

    LINear

    With the linear sweep, the step width is a fixed frequency value which is added to the current frequency. The step width for linear sweep is entered in Hz (see [ : SOURce<hw>] : LFOutput : SWEep[ : FREQuency] : STEP[ : LINear] on page 361).

##### LOGarithmic

With the logarithmic sweep, the step width is a constant fraction of the current frequency. This fraction is added to the current frequency. The logarithmic step width is entered in % (see [ : SOURce<hw>]:LFOutput:SWEep[:FREQUency]:STEP:LOGarithmic on page 362).

Example: LFO:SWE:SPAC LIN

selects linear sweep spacing.

Manual operation: See "Spacing" on page 229

#### [:SOURCE<hw>]:LFOutput:SWEep[:FREQUency]:STEP[:LINEAR] <Linear>

Sets the step size for linear LF frequency sweep steps.

This parameter correlates with the number of steps [ : SOURce<hw>]: LFOutput : SWEep[:FREQUency]:POINts within the sweep range as follows:
 $ f_{STARt} < f_{STOP} $
freq_points = ((f_{STARt} - f_{STOP}) / step_lin) + 1
If you change the step size, the number of steps changes accordingly. The sweep range remains the same.

##### Parameters:

<Linear>
    float
    Range: full frequency range
    Increment: see the data sheet: Modulation sources > Resolution of frequency setting
    *RST: 1000

Example:
    LFO: FREQ: STAR
    sets the start frequency to 2 kHz.
    LFO: FREQ: STOP
    sets the stop frequency to 20 kHz.
    LFO: SWE: SPAC LIN
    sets linear sweep spacing.
    LFO: SWE: STEP 2 kHz
    sets the sweep step width to 2 kHz. The number of sweep steps for linear sweep spacing (POINTs) is automatically set to 11.

Manual operation: See "Step Lin/Log - LF Sweep" on page 230

### [:SOURce<hw>]:LFOutput:SWEep[:FREQUency]:STEP:LOGarithmic<Logarithmic>

Sets the logarithmically determined sweep step size for the LF frequency sweep. It is expressed in percent and you must enter the value and the unit PCT with the command.

The frequency is increased by a logarithmically calculated fraction of the current frequency according to:

 $$  step\_{l}og_{step+1}=f_{step}+step\_{l}og_{step}\times f_{step} $$ 

 $$ \mathsf{f}_{\mathsf{s t e p}+1}=\mathsf{f}_{\mathsf{s t e p}}+\mathsf{s t e p\_{l} o g}_{\mathsf{s t e p}+1} $$ 

with  $ f_{\text{STARt}} < f_{\text{STOP}} $ and step = the current number of the sweep steps

This parameter correlates with the number of steps LFO:SWE[:FREQ]:POIN within the sweep range as follows:

 $$ \mathrm{f r e q\_{p} o i n t s}=(\left(\log\mathrm{f}_{\mathrm{S T O P}}-\log\mathrm{f}_{\mathrm{S T A R t}}\right)/\log\mathrm{s t e p\_{l} o g})+1 $$ 

If you change the step size, the number of steps changes accordingly. The sweep range remains the same.

Parameters:

<Logarithmic> float

Range: 0.01 to 100

Increment: 0.01

*RST: 1

Example: LFO: FREQ: STAR

sets the start frequency to 1 kHz.

LFO: FREQ: STOP

sets the stop frequency to 100 kHz.

LFO: SWE: SPAC LOG

sets logarithmic sweep spacing.

LFO: SWE: STEP: LOG 10PCT

sets the step width for logarithmic sweep spacing to 10% of the previous frequency in each instance.

Manual operation: See "Step Lin/Log - LF Sweep" on page 230

#### [:SOURCE]:LFOutput:SHAPe <Shape>

Selects the shape of the LF signal.

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

##### Parameters:


| Parameters: |  |  |
| <Shape> | SINE \| SQUARE \| TRIANGLE \| SAWTOOTH \| ISAWTOOTH |  |
|  | *RST: | SINE |
| Example: | LFO: SHAP SQU |  |
|  | selects a rectangular shape for the signal of the LF generator. |  |
| Manual operation: | See "LF Gen Shape" on page 206 |  |

##### [:SOURCE]:LFOutput:SIMPedance <Slmpedance>

Selects the output impedance of the LF generator. Selection "LOW" and "600 Ohm" are available.

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.


| Parameters: |  |  |
| <Slmpedance> | LOW \| G600 |  |
|  | *RST: | LOW |
| Example: | SOUR:LFO:SIMP G600'sets the output impedance of the LF generator to 600 Ohms |  |
| Manual operation: | See "LF Source Impedance" on page 225 |  |

#### [:SOURCE]:LFOutput:VOLTAGE <Voltage>

Sets the voltage of the LF output signal.

Parameters:

<Voltage> float

Range: see the data sheet: Internal modulation generator >

Output voltage range

Increment: see the data sheet: resolution of output voltage setting

*RST: 1

Example: LFO: VOLT 2 V

sets the voltage of the LF output to 2 V.

Manual operation: See "I'E Output Voltage" on page 224

Manual operation: See "LF Output Voltage" on page 224

#### 6.13.7 SOURCE:LIST Subsystem

This subsystem contains the commands for the List mode of the instrument.

The following settings are required to operate the instrument in List mode:

## 1. Create a list.

If a list which does not exist is selected with the :LIST:SEL command, an empty list with the name of the selected list is created.

SOUR1:LIST:SEL "New_list"

## 2. Fill the list with values.

All list components must be of the same length. This does not apply to components of length 1. This is interpreted as if the component has the same length as the other components and as if all values are the same as the first value.

SOUR1:LIST:FREQ 100 MHz, 110 MHz, 120 MHz...

SOUR1:LIST:POW 2dBm, -1dBm, 0dBm...

## 3. Select a list.

If a new empty file has been created with the :LIST:SEL command, this file is selected, otherwise an existing list must be selected before the List mode is activated.

SOUR1:LIST:SEL "Old_list"

## 4. Set the dwell time.

The dwell time determines the duration of the individual list steps.

SOUR1: LIST: DWEL 3ms

## 5. Set the List mode.

The List mode determines the way in which the list is processed. In the example the list is processed once only or repeatedly depending on the trigger setting. SOUR1:LIST:MODE AUTO

## 6. Determine the trigger.

In the example each trigger causes the list to be processed once from beginning to end.

SOUR: LIST: TRIG: SOUR SING

7. Activate the List mode.

SOUR1:FREQ:MODE LIST

8. Trigger the list (depending on the mode).

SOUR1:LIST:TRIG:EXEC

9. Deactivate the List mode.

SOUR1:FREQ:MODE CW

SCPI refers to the individual lists as segments.

[ :SOURce<hw>]:LIST:CATalog?.....365

[ :SOURce<hw>]:LIST:DELETE.....366

[ :SOURce<hw>]:LIST:DELETE:ALL.....366

[ :SOURce<hw>]:LIST:DEXChange:AFILe:CATalog?.....367

[ :SOURce<hw>]:LIST:DEXChange:AFILe:EXTension.....367

[ :SOURce<hw>]:LIST:DEXChange:AFILe:SELECT.....368

[ :SOURce<hw>]:LIST:DEXChange:AFILe:SEParator:COLumn.....368

[ :SOURce<hw>]:LIST:DEXChange:AFILe:SEParator:DECimal.....369

[ :SOURce<hw>]:LIST:DEXChange:EXECute.....369

[ :SOURce<hw>]:LIST:DEXChange:MODE.....370

[ :SOURce<hw>]:LIST:DEXChange:SELECT.....370

[ :SOURce<hw>]:LIST:DWELI.....370

[ :SOURce<hw>]:LIST:FREE?.....371

[ :SOURce<hw>]:LIST:FREQUency.....371

[ :SOURce<hw>]:LIST:FREQUENCY:POINTs?.....372

[ :SOURce<hw>]:LIST:INDEX.....372

[ :SOURce<hw>]:LIST:INDEX:START.....372

[ :SOURce<hw>]:LIST:INDEX:STOP.....373

[ :SOURce<hw>]:LIST:LEARN.....373

[ :SOURce<hw>]:LIST:MODE.....374

[ :SOURce<hw>]:LIST:POWER.....374

[ :SOURce<hw>]:LIST:POWER:POINTs?.....374

[ :SOURce<hw>]:LIST:RESET.....375

[ :SOURce<hw>]:LIST:SELECT.....375

[ :SOURce<hw>]:LIST:TRIG:EXECUTE.....375

[ :SOURce<hw>]:LIST:TRIG:SOURCE.....376

##### [:SOURCE<hw>]:LIST:CATalog?

Requests a list of available lists. The individual lists are separated by commas.

The lists are stored with the fixed file extensions *.1sw in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

Return values:

<Catalog> string

Example: MMEM:CDIR '/var/Listmode'

selects the directory for the list mode files.

LIST:CAT?

queries the available lists.

Response: 'list1,list2'

the lists list1 and list2 are available.

Usage: Query only

#### [:SOURCE<hw>]:LIST:DELETE <Filename>

Deletes the specified list.

The files are stored with the fixed file extensions *.1sw in a directory of the user's choice. The directory applicable to the command is defined with the command MMEMORY:CDIR. To access the files in this directory, only the file name has to be given, without the path and the file extension. A path can also be specified in command :SOUR:LIST:CAT?, in which case the file in the specified directory is deleted.

*RST does not affect data lists.

##### Setting parameters:

<Filename> string

Example: MMEM:CDIR '/var/Listmode'

selects the directory for the list mode files.

LIST:DEL 'LIST1'

deletes the list list1.

Usage: Setting only

Manual operation: See "List Mode Data... - List Mode" on page 196

##### [:SOURCE<hw>]:LIST:DELETE:ALL

Deletes all lists in the selected directory.

Note: The list mode must be previously disabled to make sure that no records are selected when you set the frequency mode ([ : SOURce<hw>]: FREQuency:MODE).

The files are stored with the fixed file extensions *.lsw in a directory of the user's choice. You can select the directory with the commands :MMEMory:CDIRectory or [:SOURce<hw>]:LIST:CATalog?.

*RST does not affect data lists.

Example: MMEM:CDIR '/var/Listmode'
selects the directory for the list mode files.
FREQ:MODE SWE
deactivates the list mode for RF output and activates the sweep mode.
LIST:DEL:ALL
deletes all list mode files in the selected directory.
Usage: Event
Manual operation: See "List Mode Data... - List Mode" on page 196

#### [:SOURCE<hw>]:LIST:DEXChange:AFILe:CATalog?

Queries the available ASCII files for export or import of list mode data in the current or specified directory.

As response, you get a string containing the existing ASCII files *.txt or *.csv, separated by commas.

##### Return values:

<Catalog> string

Example: MMEM:CDIR '/var/import'

selects the directory for the ASCII files with frequency and level value pairs.

LIST:DEXC:AFIL:EXT TXT

determines the extension *.txt for the query.

LIST:DEXC:AFIL:CAT?

queries the available files with extension *.txt.

Response: 'list1,list2'

the ASCII files list1.txt and list2.txt are available.

Usage: Query only

Usage: Query only

#### [:SOURCE<hw>]:LIST:DEXChange:AFILe:EXTension <Extension>

Determines the extension of the ASCII file for import or export, or to query existing files.

##### Parameters:

<Extension> TXT | CSV

*RST: TXT

Example: MMEM:CDIR '/var/import'

selects the directory for the ASCII files with frequency and level value pairs.

LIST:DEXC:AFIL:EXT TXT

selects ASCII files with the extension *.txt for the query.

LIST:DEXC:AFIL:CAT?

queries the available files with extension *.txt.

Response: 'list1,list2'

the ASCII files list1.txt and list2.txt exist.

Manual operation: See "Extension - List Mode" on page 199

##### [:SOURCE<hw>]:LIST:DEXChange:AFILe:SELECT <Filename>

Selects the ASCII file to be imported or exported.

##### Parameters:

Parameters:

<Filename> <ascii_file_name>

Example:

LIST:DEXC:MODE IMP

determines that ASCII files with frequency and level value pairs

are imported into list mode lists.

LIST:DEXC:AFIL:EXT TXT

determines the extension *.txt for the query.

LIST:DEXC:AFIL:CAT?

queries the available files with extension *.txt.

Response: 'list1,list2'

the ASCII files list1.txt and list2.txt exist.

LIST:DEXC:AFIL:SEL '/var/list.csv'

selects list.csv for import.

LIST:DEXC:SEL '/var/list_imp'

determines the destination file list_imp.

LIST:DEXC:EXEC

imports the ASCII file data into the list file.

Manual operation: See "Select ASCII Source / Destination - List Mode"

on page 199

##### [:SOURCE<hw>]:LIST:DEXChange:AFILe:SEParator:COLumn <Column>

Selects the separator between the frequency and level column of the ASCII table.

##### Parameters:


| <Column> | TABulator \| SEMicolon \| COMMa \| SPACe |  |  |
|  | $ ^{*} $RST: | COMMa |  |

Example: LIST:DEXC:MODE EXP

    selects that the list is exported into an ASCII file.

    LIST:DEXC:AFIL:SEL '/var/list.csv'

    determines ASCII file list.csv as destination for the list mode list data.

    LIST:DEXC:AFIL:SEP:COL TAB

    defines a tabulator to separate the frequency and level values pairs.

    LIST:DEXC:AFIL:SEP:DEC DOT

    selects the decimal separator dot.

    LIST:DEXC:SEL '/var/list_imp'

    determines the source file list_imp for export into the ASCII file list.csv.

    LIST:DEXC:EXEC

    exports the list file data into the ASCII file.

Manual operation: See "Column Separator- List Mode" on page 199

#### [:SOURCE<hw>]:LIST:DEXChange:AFILe:SEParator:DECimal <Decimal>

Sets the decimal separator used in the ASCII data between '.' (decimal point) and ',' (comma) with floating-point numerals.

##### Parameters:

<Decimal> DOT | COMMa

*RST: DOT

Example: see[:SOURce<hw>]:LIST:DEXChange:AFILe:

SEParator:COLumn on page 368

Manual operation: See "Decimal Point - List Mode" on page 199

#### [:SOURCE<hw>]:LIST:DEXChange:EXECute

Executes the import or export of the selected list file, according to the previously set transfer direction with command [ : SOURce<hw> ] : LIST : DEXChange : MODE.

Example: LIST:DEXC:MODE IMP

    determines that ASCII files with frequency and level value pairs

    are imported into list mode lists.

    LIST:DEXC:AFIL:SEL '/var/list.csv'

    selects the ASCII file list.csv for import.

    LIST:DEXC:SEL '/var/list_imp'

    determines the destination file list_imp.

    LIST:DEXC:EXEC

    imports the ASCII file data into the list mode file.

Usage: Event

Manual operation: See "Import / Export - List Mode" on page 200

#### [:SOURCE<hw>]:LIST:DEXChange:MODE <Mode>

Selects if list mode lists should be imported or exported. Depending on the selection here, the file select command defines either the source or the destination for list mode lists and ASCII files.

Parameters:

<Mode> IMPort | EXPort

*RST: IMPort

Example: LIST:DEXC:MODE IMP

selects that ASCII files with frequency and level value pairs are

imported and transferred into list mode lists.

LIST:DEXC:AFIL:SEL '/var/list.csv'

selects that ASCII file list.csv is imported.

LIST:DEXC:SEL '/var/list_imp'

selects that the ASCII file list.csv is imported into list mode

list list_imp.

Manual operation: See "Mode - List Mode" on page 199

##### [:SOURCE<hw>]:LIST:DEXChange:SELECT <Filename>

Selects the list mode list to be imported or exported.

The list mode files are stored with the fixed file extensions *.1sw in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR. A path can also be specified in command SOUR:LIST:DEXC:SEL, in which case the files are stored or loaded in the specified directory.

##### Parameters:

<Filename>

    <list_name>

        LIST:DEXC:MODE IMP

        selects that ASCII files with frequency and level value pairs are

        imported and transferred into list mode lists.

        LIST:DEXC:AFIL:SEL '/var/list.csv'

        selects that ASCII file list.csv is imported.

        LIST:DEXC:SEL '/var/list_imp'

        selects that the ASCII file list.csv is imported into list mode

        list list_imp.

Manual operation: See "Select Destination / Source - List Mode" on page 200

##### [:SOURCE<hw>]:LIST:DWELI <Dwell>

Sets the dwell time. The R&S SMB generates the signal with the frequency / power value pairs of each list entry for that particular period.

##### Parameters:

<Dwell> float

Range: 7E-4 to 100

Increment: 1E-4

*RST: 15E-3

Example: LIST:DWELL 15

retains each setting in the list for 15 ms.

Manual operation: See "Dwell Time - List Mode" on page 195

##### [:SOURCE<hw>]:LIST:FREE?

Queries on the free storage space for list mode lists.

##### Return values:

<Free> integer

Range: 0 to INT_MAX

*RST: 0

Example: LIST: FREE?

Usage: Query only

Response: 2147483647; 1

##### [:SOURCE<hw>]:LIST:FREQUENCY <Frequency>

Fills the FREQuency column of the selected list with data.

*RST does not affect data lists.

##### Parameters:

<Frequency>

    <Frequency#1>{, <Frequency#2>, ...} | block data

    The data can be given either as a list of numbers (list can be of any length and list entries must be separated by commas) or as binary block data. When block data is transferred, 8 bytes are always interpreted as a floating-point number with double accuracy (see :FORMAT[:DATA] on page 298).

Range: 300 kHz to RFmax

##### Example:

LIST:SEL '/var/list3'

selects list3 for editing. The R&S SMB generates a new file automatically, if it does not exist yet.

SOUR:LIST:FREQ 1.4GHz, 1.3GHz, 1.2GHz, ...

specifies the frequency values in list3. If the list already contains data, it is overwritten.

Manual operation: See "Edit List Mode Data... - List Mode" on page 197

#### [:SOURCE<hw>]:LIST:FREQUENCY:POINTs?

The command queries the length (in points) of the FREQuency component of the selected list.

Return values:
 integer
Range: 0 to INT_MAX
*RST: 0

Example: LIST:SEL '/var/list3'

selects list3 for editing. The R&S SMB creates a new file automatically, if it does not exist yet.

LIST:FREQ:POIN?

queries the number of frequency values in the list

Response: 327

Usage: Query only

#### [:SOURCE<hw>]:LIST:INDEX 

Sets the list index in step mode (LIST:MODE STEP).

After the trigger signal the frequency and level settings of the selected index are processed in List mode.

*RST: 0

Example:

LIST:SEL '/var/list3'

selects list3 for use in List mode.

FREQ:MODE LIST

activates List mode. List3 is processed.

LIST:MODE STEP

selects manual, step-by-step processing of the list.

LIST:IND 5

the frequency/level value pair with index 5 is executed.

TRIG:LIST:SOUR SING

selects triggering by means of the single trigger. The list is executed once.

SOUR:LIST:TRIG:EXEC

triggers the processing of the selected list.

Manual operation: See "Current Index - List Mode" on page 196

##### [:SOURCE<hw>]:LIST:INDEX:START <Start>

Sets the start index of the index range which defines a subgroup of frequency/level value pairs in the current list. Only the values in the set index range (:LIST:INDEX:STARt ... :LIST:INDEX:STOP) are processed in List mode.

Parameters:
<Start> integer
Range: 0 to list length
*RST: 0
Example:
LIST:SEL '/var/list3'
selects list3 for use in List mode.
LIST:IND:STAR 25
sets 25 as start index of the index range.
LIST:IND:STOP 49
sets 49 as stop index of the index range.
FREQ:MODE LIST
activates List mode. The frequency/level value pairs from index 25 to index 49 in list3 are processed. All other entries of the list are ignored.

Manual operation: See "List Range In - List Mode" on page 198

#### [:SOURCE<hw>]:LIST:INDEX:STOP <Stop>

Sets the stop index of the index range which defines a subgroup of frequency/level value pairs in the current list. Only the values in the set index range (:LIST:INDEX:STARt ... :LIST:INDEX:STOP) are processed in list mode.

Parameters:
<Stop> integer
Range: 0 to list length
*RST: 0
Example: see [ :SOURce<hw>] :LIST:INDEX:STARt on page 372
Manual operation: See "List Range In - List Mode" on page 198

##### [:SOURCE<hw>]:LIST:LEARN

Learns the selected list to determine the hardware setting for all list entries. The results are saved with the list. When the list is activated the first time, these settings are calculated automatically.

Example: LIST:SEL '/var/list3'

selects list file. The file is created if it does not yet exist.

LIST:LEAR

starts learning of the hardware setting for list3 and stores the

setting.

Usage: Event

Manual operation: See "Learn List Mode Data... - List Mode" on page 196

Selects how the list is to be processed (similar to SOURce: SWEep:MODE).

Parameters:

<Mode>

    AUTO | STEP

    AUTO

    Each trigger event triggers a complete list cycle. Possible trigger settings for :LIST:TRIGger:SOURce are AUTO, SINGle and EXT.

    STEP

    Each trigger event triggers only one step in the list processing cycle. The list is processed in ascending order.

    *RST: AUTO

Example: LIST:MODE STEP

selects step-by-step processing of the list.

Manual operation: See "Mode - List Mode" on page 194

#### [:SOURCE<hw>]:LIST:POWER 

Fills the Level part of the selected list with data.

*RST does not affect data lists.

##### Parameters:


    {, , ...} | block data

    The data can be given either as a list of numbers (list can be of any length and list entries must be separated by commas) or as binary block data. When block data is transferred, 8 bytes are always interpreted as a floating-point number with double accuracy (see :FORMAT[:DATA] on page 298).

    Range: Minimum level to Maximum level

    Default unit: dBm

Example: LIST:SEL '/var/list3'

selects list3 for editing. The R&S SMB generates a new file automatically, if it does not exist yet.

LIST:POW 0dBm, 2dBm, 2dBm, 3dBm, ...

specifies the level values in list3. The number of level values must correspond to the number of frequency values. The previous data is overwritten.

Manual operation: See "Edit List Mode Data... - List Mode" on page 197

#### [:SOURCE<hw>]:LIST:POWER:POINTs?

Queries the length (in points) of the LEVel part of the selected list.

Return values:

 integer

Range: 0 to INT_MAX

*RST: 0

Example: LIST:SEL '/var/list3'

selects list3 for editing. The R&S SMB generates a new file automatically, if it does not exist yet.

LIST:POW:POIN?

queries the number of levels in the list file

Response: 327

Usage: Query only

#### [:SOURCE<hw>]:LIST:RESET

Resets the list to the starting point.

Example: LIST:RES

resets the list to the starting point.

Usage: Event

Manual operation: See "Reset - List Mode" on page 195

##### [:SOURCE<hw>]:LIST:SELECT <Filename>

Selects the specified list. If a new list is to be created, the name can be entered here. The list is created if it does not yet exist. The list selected here is available for the further processing steps (editing) and is used in the instrument when the list mode is activated.

The files are stored with the fixed file extensions *.lsw in a directory of the user's choice. The directory applicable to the command is defined with the command MMEMORY:CDIR. A path can also be specified in which case the list mode file in the specified directory is selected.

*RST does not affect data lists.

Parameters:

<Filename> '<list name>`

Example: LIST:SEL '/var/list3'

selects list3 for editing.

Manual operation: See "List Mode Data... - List Mode" on page 196

##### [:SOURCE<hw>]:LIST:TRIGger:EXECute

Starts the processing of a list in list mode. It corresponds to the manual-control command "Execute Single."


| Example: | SOUR: LIST: TRIG: EXEC\ntriggers the processing of the selected list. |
| Usage: | Event |
| Manual operation: | See "Execute Single - List Mode" on page 195 |

##### [:SOURCE<hw>]:LIST:TRIGger:SOURCE <Source>

Selects the trigger source processing lists.

The names of the parameters correspond to those under sweep mode. SCPI uses other names for the parameters; these names are also accepted by the instrument. The SCPI names should be used if compatibility is an important consideration. An overview of the various names is given in the following table:


| R&S name | SCPI name | Command under manual control |
| AUTO | IMMediate | MODE AUTO |
| SINGLE | BUS | MODE SINGLE or STEP |
| EXTERNAL | EXTERNAL | MODE EXT TRIG SINGLE or EXT TRIG STEP |

##### Parameters:

<Source> AUTO | IMMediate | SINGle | BUS | EXternal

##### AUTO|IMMediate

The trigger is free-running, i.e. the trigger condition is fulfilled continuously. The selected list in List mode is restarted as soon as it is finished.

##### SINGle|BUS

The list is triggered by the GPIB commands [ : SOURce<hw> ] :

LIST:TRIGger:EXECute. The list is executed once.

##### EXTernal

The list is triggered externally via the [INST TRIG] connector.

The list is executed once.

*RST: AUTO

Example: LIST:TRIG:SOUR EXT

selects triggering by means of the external trigger.

Manual operation: See "Mode - List Mode" on page 194

#### 6.13.8 SOURCE:MODulation Subsystem

This subsystem contains the command for switching on/off all modulations.

[:SOURCE<hw>]:MODulation[:ALL][:STATE] <State>

Activates/deactivates the modulations.

The command SOUR:MOD:ALL:STAT OFF switches all modulations off. A subsequent command SOUR:MOD:ALL:STAT ON restores the status that was active before the last switch-off. "MOD OFF" is displayed in the info line of the header next to the "Level" field.

##### Parameters:

<State>

0 | 1 | OFF | ON

*RST: 0

Example: MOD:STAT OFF

switches off all modulations.

Manual operation: See "[MOD ON/OFF]" on page 202

#### 6.13.9 SOURCE:PGEN Subsystem

This subsystem contains the commands for setting the pulse generator.

[:SOURCE<hw>]:PGENERator:STATE.....377

[:SOURCE<hw>]:PGENERator:STATE <State>

Activates/deactivates the output of the video/sync signal at the [PULSE VIDEO] connector at the rear of the instrument.

The signal output and the pulse generator are automatically switched on with activation of pulse modulation if pulse generator is selected as modulation source. The signal output can be switched off subsequently.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: PULM: SOUR INT

selects the internal pulse generator as source for pulse modulation

PULM: STAT ON

activates pulse modulation. The pulse generator and the output of the signals at the [PULSE VIDEO] connector are automatically activated in addition.

PGEN: STAT OFF

deactivates the output of the pulse signal by the pulse generator at the [PULSE VIDEO] connector. The pulse modulation of the RF carrier must be activated with command

SOURCE: PULM: STATE.

Manual operation: See "Video Sync Signal State - Pulse Generator" on page 232

#### 6.13.10 SOURCE:PHASE Subsystem

This subsystem contains the commands for adjusting the phase of the RF output signal relative to a reference signal of the same frequency.

[:SOURce<hw>]:PHASE.....378

[:SOURce<hw>]:PHASE:REFERENCE.....378

[:SOURCE<hw>]:PHASE 

Sets the phase variation relative to the current phase. The variation is specified in RADians.

##### Parameters:

 float

Range: -720 to 720

Increment: 0.1

*RST: 0

Example: PHAS 0.1 RAD

changes the phase by 0.1 RAD relative to the current phase.

PHAS:REF

adopts the set phase as the current phase.

Manual operation: See "Delta Phase" on page 142

#### [:SOURCE<hw>]:PHASE:REFERENCE

Adopts the phase set with SOURce:PHASE:ADJust as the current phase.

Example: PHAS 0.1RAD

changes the phase by 0.1 RAD relative to the current phase.

PHAS:REF

adopts the set phase as the current phase.

Usage: Event

Manual operation: See "Reset Delta Phase Display" on page 142

#### 6.13.11 SOURCE:PM Subsystem

The PM subsystem contains the commands for checking the phase modulation. The settings for the internal modulation source (LF generator) are made in the SOURce:LFOutput subsystem.

For information on the required options, see Chapter 4.4.4, "Phase Modulation (PhiM)", on page 211.

[ :SOURCE<hw>]:PM[:DEViation].....379

[ :SOURCE<hw>]:PM:EXTERNAL:COUPling.....379

[ :SOURCE<hw>]:PM:EXTERNAL:DEViation.....379

[ :SOURCE<hw>]:PM:INTERNAL:DEViation.....380

[ :SOURCE<hw>]:PM:MODE.....380

[:SOURce<hw>]:PM:SENSitivity?.....381

[:SOURce<hw>]:PM:SOURce.....381

[:SOURce<hw>]:PM:STATe.....381

#### [:SOURCE<hw>]:PM[:DEViation] <Deviation>

Sets the deviation of the phase modulation signals in RAD. The maximum deviation depends on the set RF frequency and the selected modulation mode (see data sheet).

##### Parameters:

<Deviation> float

Range: see data sheet

Increment: 1E-6

Example: PM 2 sets 2 RAD deviation to the phase modulation signal.

Manual operation: See "PhiM Deviation" on page 213

#### [:SOURCE<hw>]:PM:EXTERNAL:COUPLING <Coupling>

Selects the coupling mode for the external phase modulation signal.

Parameters:

<Coupling> AC | DC

AC

Uses only the AC signal component of the modulation signal.

DC

Uses the modulation signal as it is, with AC and DC.

*RST: AC

Example: PM:EXT:COUP AC

selects the coupling mode AC for the external phase modulation signal.

Manual operation: See "Mod Ext Coupling" on page 214

#### [:SOURCE<hw>]:PM:EXTERNAL:DEViation <Deviation>

Sets the modulation deviation of the external phase modulation signal in RAD. The maximum value depends on the set RF frequency and the selected modulation mode (see data sheet).

The sum of the deviations of all active frequency modulation signals may not exceed the total value set with command [ : SOURce<hw> ] :PM:EXTERNAL:DEViation.

##### Parameters:

<Deviation> float

Range: 0 to 20

*RST: 1

Example: PM 5 sets 5 RAD deviation for the external phase modulation signal.

Manual operation: See "PhiM Deviation" on page 213

#### [:SOURCE<hw>]:PM:INTernal:DEViation <Deviation>

Sets the deviation of the internal phase modulation signal in RAD.

The sum of the deviations of all active frequency modulation signals may not exceed the total value set with command [:SOURCE<hw>]:PM[:DEViation].

##### Parameters:

<Deviation> float

Range: see data sheet

Increment: 1E-6

*RST: 1

Example: PM:INT1:DEV 3RAD

sets 3 RAD deviation for the internal phase modulation signal.

Manual operation: See "PhiM Deviation" on page 213

#### [:SOURCE<hw>]:PM:MODE <Mode>

Selects the mode for the phase modulation.

##### Parameters:

<Mode>

    HDEViation | NORMAL | LNOise

    HDEViation

    Provides full setting range of PhiM deviation. The range of modulation frequency is limited (see data sheet). Recommended for low modulation frequencies and/or high PhiM deviation.

    NORMAL

    Provides full setting range of modulation bandwidth and PhiM deviation. Recommended for high modulation frequencies.

    LNOise

    Provides modulation with phase noise and spurious characteristics close to CW mode. The range for modulation bandwidth and PhiM deviation is limited (see data sheet)

    *RST: HBANDwidth

Example: PM:MODE LNO

selects Low Noise mode for external phase modulation.

Manual operation: See "PhiM Mode" on page 212

#### [:SOURCE<hw>]:PM:SENSitivity?

Queries the input sensitivity of the externally applied signal for phase modulation. The returned value reports the sensitivity in RAD/V. It is assigned to the voltage value for full modulation of the input.

Return values:

<Sensitivity> float

Example: PM:DEV 1 sets a modulation deviation of 1RAD. PM:SENS? queries the input sensitivity at the external modulation input. Response: 1 since the voltage value for full modulation is 1V, the resulting sensitivity is precisely 1RAD/V.

Usage: Query only

Manual operation: See "PhiM Sensitivity" on page 214

#### [:SOURCE<hw>]:PM:SOURCE <Source>

Selects the modulation signal source for phase modulation.

You can use both, the internal and an external modulation signal at a time.

Parameters:

<Source>

    INTernal | EXTernal | INT,EXT

    INTernal

    Uses the internally generated signal for modulation. To configure the LF signal, use the commands of the SOURce:LFOutput Subsystem subsystem.

    EXTernal

    Uses an externally applied modulation signal.

    INT,EXT

    Uses both, the internal and external modulation signals.

    *RST: INT

Example:

PM: SOUR INT

Example: PM: SOUR INT

selects the internal modulation source.

Manual operation: See "PhiM Source" on page 212

#### [:SOURCE<hw>]:PM:STATe <State>

Activates phase modulation.

Note: Activation of PM deactivates frequency modulation (FM).

Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: PM: STAT ON

activates PM.

Manual operation: See "State" on page 212

#### 6.13.12 SOURce:POWer Subsystem

This subsystem contains the commands for setting the output level, level control and level correction of the RF signal.

Other units can also be used instead of dBm:

• by entering the unit directly after the numerical value (example : POW 0.5V)

• by changing the DEFault unit in the UNIT system (see the command :UNIT:POWER).

[:SOURCE<hw>]:POWER:ALC:OMODe.....382

[:SOURCE<hw>]:POWER:ALC:SONCe.....383

[:SOURCE<hw>]:POWER:ALC[:STATe].....383

[:SOURCE<hw>]:POWER:ATTenuation:RFOF:MODE.....384

[:SOURCE<hw>]:POWER:EMF:STATE.....384

[:SOURCE<hw>]:POWER[:LEVEL][:IMMediate][:AMPLitude].....384

[:SOURCE<hw>]:POWER[:LEVEL][:IMMediate]:OFFSET.....385

[:SOURCE<hw>]:POWER[:LEVEL][:IMMediate]:RCL.....386

[:SOURCE<hw>]:POWER:LIMit[:AMPLitude].....386

[:SOURCE]:POWER:WIGNore.....386

[:SOURCE<hw>]:POWER:LMODE.....387

[:SOURCE<hw>]:POWER:MANual.....387

[:SOURCE<hw>]:POWER:MODE.....388

[:SOURCE<hw>]:POWER:POWER.....388

[:SOURCE<hw>]:POWER:SPC:CRAnge.....389

[:SOURCE<hw>]:POWER:SPC:DELAY.....389

[:SOURCE<hw>]:POWER:SPC:PEAK.....390

[:SOURCE<hw>]:POWER:SPC:SELECT.....390

[:SOURCE<hw>]:POWER:SPC:STATe.....390

[:SOURCE<hw>]:POWER:SPC:TARGET.....390

[:SOURCE<hw>]:POWER:START.....391

[:SOURCE<hw>]:POWER:STEP[:INCREment].....391

[:SOURCE<hw>]:POWER:STEP:MODE.....392

[:SOURCE<hw>]:POWER:STOP.....392

#### [:SOURCE<hw>]:POWER:ALC:OMODE <OffMode>

The command sets the level control mode which becomes active when automatic level control is deactivated (ALC Off).

Parameters:

<OffMode> SHOLd

SHOLd

Level control is activated briefly if the level or frequency changes ("ALC Off Sample & Hold").

*RST: SHOLd

Example:

POW:ALC OFF

deactivates automatic level control for RF output A.

POW:ALC:OMOD SHOL

level control is briefly activated if the frequency or level changes.

#### [:SOURCE<hw>]:POWER:ALC:SONCe

Temporarily activates level control for correction purposes.

Example: POW:ALC OFF

    deactivates automatic level control for RF output A.

    POW:ALC:SONC

    level control is performed once only.

Manual operation: See "Search Once - ALC" on page 155

#### [:SOURCE<hw>]:POWER:ALC[:STATE] <State>

Activates/deactivates automatic level control.

Parameters:

<State> ON | OFF | AUTO

ON

Internal level control is permanently activated.

OFF

Internal level control is deactivated; Sample & Hold mode is activated.

AUTO

Internal level control is activated/deactivated automatically depending on the operating state.

*RST: AUTO

Example: POW:ALC ON

activates automatic level control for RF output A.

Manual operation: See "State - ALC" on page 155

[:SOURCE<hw>]:POWER:ATTenuation:RFOFf:MODE <Mode>

Selects the attenuator mode, when the RF signal is switched off.

Parameters:

<Mode>

UNCHanged | FATTenuation

UNCHanged

Freezes the setting of the attenuator when RF is switched off.

The attenuator is only activated when RF is switched on.

This setting recommended if a constant VSWR (Voltage Standing Wave Ratio) is required.

Furthermore, on instruments equipped with a mechanical attenuator, it provides fast and wear-free operation.

FATTenuation

Sets attenuation to maximum when the RF signal is switched off. This setting is recommended for applications that require a high level of noise suppression.

*PST: n.a. (factory/preset: FATTenuation)

Example: SOUR: POW: ATT: RFOF: MODE FATT sets the RF OFF attenuator to maximum.

Manual operation: See "RF OFF Mode" on page 151

#### [:SOURCE<hw>]:POWER:EMF:STATE <State>

Displays the signal level as voltage of the EMF. The displayed value represents the voltage over a 50 Ohm load.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 0)

Example: POW:EMF:STAT 1

activates voltage level display.

Manual operation: See "Display Level as Voltage of EMF - RF Level" on page 153

#### [:SOURCE<hw>]:POWER[:LEVEL][:IMMediate][:AMPLitude] <Amplitude>

Sets the RF level applied to the DUT.

##### Notes:

If specified, a level offset [ : SOURce<hw>]:POWER[:LEVel][ : IMMediate]:OFFSet is included according to the formula:

Minimum level + OFFSET ... Maximum level + OFFSET

In addition to numerical values, you can increase or decrease the values step by step with the UP and DOWN according to the step width defined with[:

POWER: STEP[:INCREment].

The RF output is activated with :OUTPUT<hw>[:STATE] on page 317 (RF ON / RF OFF).

##### Parameters:

<Amplitude> Minimum level ... Maximum level

Determines the RF output level.

Range: Minimum level to Maximum level

*RST: -30

Example: The keywords of this command are largely optional. Therefore, both the long and short form of the command are shown.

SOUR: POW: LEV: IMM: AMPL 15

or

: POW 15

sets the RF level at output A to 15 dBm.

Manual operation: See "RF Level" on page 147

#### [:SOURCE<hw>]:POWER[:LEVEL][:IMMediate]:OFFSET<Offset>

Note: The level offset is also effective for level sweeps!

Specifies the constant level offset of a downstream attenuator/amplifier. If a level offset is entered, the level entered with :POWER no longer corresponds to the RF output level.

The following correlation applies:

POWER = RF output level + POWER:OFFSet.

Entering a level offset does not change the RF output level, but rather the query value of :POWER.

For more information, see "RF level vs. RF output level" on page 146.

Only dB is permitted as the unit here. The linear units (V, W, etc.) are not permitted.

The keywords of this command are largely optional. Therefore, both the long and short form of the command are shown in the example.

##### Parameters:

<Offset> float
Range: -100 to 100
Increment: 0.01
*RST: 0
Example: SOURce:POWER:LEVEL:IMMediate:OFFSET -10
or
POW:OFFS 10
sets the RF level offset to 10 dB
Manual operation: See "Offset (Level)" on page 150

Manual operation: See "Offset (Level)" on page 150

#### [:SOURCE<hw>]:POWER[:LEVEL][:IMMediate]:RCL <Rcl>

Determines whether the RF level is retained or taken from a loaded instrument configuration, when you recall instrument settings with the command *RCL.

Parameters:

<Rcl>

    INCLUDE | EXCLude

    INCLUDE

    Takes the level value of the loaded settings.

    EXCLude

    Retains the current level when an instrument configuration is loaded.

    *RST: | INCLUde

Example: POW:RCL INCL

takes the level value from an instrument configuration loaded with command *RCL.

Manual operation: See "Exclude Level" on page 131

#### [:SOURCE<hw>]:POWER:LIMIT[:AMPLitude] <Amplitude>

Limits the maximum RF output level in CW and SWEEP mode. It does not influence the "Level" display or the response to the POW? query command.

##### Parameters:

Parameters.

<Amplitude> float

Minimum level ... Maximum level

The value range for the level setting varies according to the instrument model.

The values are given in the data sheet.

Increment: 0.01

*RST: n.a. (factory preset: 30)

Example: SOURce:POWER:LIMit:AMPLitude 10 or :POW:LIM 10 limits the RF level to maximum +10 dBm.

Manual operation: See "Limit - RF Level" on page 149

##### [:SOURCE]:POWER:WIGNore <State>

Ignores level range warnings.

##### Parameters:

<State>

    0 | 1 | OFF | ON

    *RST: n.a. (factory preset: 0)


Example: POW:WIGN ON suppresses the level range warnings.

Manual operation: See "Ignore Level Range Warnings" on page 150

[:SOURCE<hw>]:POWER:LMODE <LevMode>

Sets the RF level mode.


| Parameters: |  |
| <LevMode> | NORMAL \| LOWNoise \| LOWDistortion |
|  | NORMAL |
|  | The RF signal is output in the standard values of the instrument. |
|  | LOWNoise |
|  | A very low noise sinewave signal is output. |
|  | LOWDistortion |
|  | A very pure sinewave signal is output. |
|  | *RST: Normal |

Example: POW:LMODE LOWD

sets the LOWDistortion mode. The instrument reduces distortions of the RF signal to a minimum.

Manual operation: See "RF Mode" on page 148

##### [:SOURCE<hw>]:POWER:MANual <Manual>

In Sweep mode ( : SOUR : POW : MODE SWE) the command sets the level for the next sweep step in the Step sweep mode ( : SOUR : SWE : POW : MODE MAN). Here only level values between the settings [ : SOUR ] : POW : STAR and [ : SOUR ] : POW : STOP are permitted. Each sweep step is triggered by a separate : SOUR : POW : MAN command.

As with the "Level" value entered in the "RF Level" menu, the OFFSET value is also taken into consideration with this command.

The specified value range is therefore only effective if : SOURce: POWER: OFFSet is set to 0. The value range for other OFFset values can be calculated using the following formula:

Minimum level + OFFSet ... Maximum level + OFFSet

##### Parameters:

<Manual>

float

Minimum level ... Maximum level

The value range for the level setting varies according to the instrument model

The values are given in the data sheet.

Increment: 0.01

*RST: -30

Example: POW: SWE: MODE MAN sets the Step sweep mode for RF output A. POW: MAN -5 dBm sets an RF level of -5 dBm for the next setting in the Step sweep mode for RF output A. POW: MODE SWE sets the Level Sweep mode for RF output A. POW: MAN -5.5 dBm triggers the next sweep step with a level of -5.5 dBm.
Manual operation: See "Current Level - Level Sweep" on page 191

##### [:SOURCE<hw>]:POWER:MODE <Mode>

Sets the instrument operating mode and therefore also the commands used to set the output level.

Parameters:
<Mode> CW | FIXed | SWEep
CW|FIXed
Operates at a constant level.
CW and FIXed are synonyms. To set the output level value, use the command [ : SOURce<hw>] : POWER[ : LEVel] [ : IMMediate] [ : AMPLitude].
SWEep
Operates in power sweep mode.
Set the range and current level with the commands [ : SOURce<hw>] : POWER: STARt, [ : SOURce<hw>] : POWER: STOP and [ : SOURce<hw>] : POWER: MANual.
*RST: CW
Example:
POW:MODE SWEep
selects the SWEep mode using the
POW:STAR; POW:STOP; POW:MAN settings.
Manual operation: See "State - Level Sweep" on page 187

##### [:SOURCE<hw>]:POWER:POWER 

Sets the RF level of the RF output connector.

The level entered with this command corresponds to the level at the RF output, i.e. any offset entry is not taken into consideration.

Note: The SCPI command [ : SOURce<hw> ] : POWER[ : LEVel ] [ : IMMediate ] [ : AMPLitude ] sets the level of the "Level" display, i.e. the level containing offset.

##### Parameters:


Minimum level ... Maximum level

The value range for the level setting varies according to the instrument model.

The values are given in the data sheet.

Increment: 0.01

*RST: -30

Example:

SOUR: POW: POW 15

sets the RF level at output to 15 dBm.

Manual operation: See "Amplitude" on page 149

#### [:SOURCE<hw>]:POWER:SPC:CRANge 

Defines the capture range of the power control system.

Within the range:

Target Level +/- Catch Range

the power control locks and tries to achieve the target level. Readings outside the range are not considered.

##### Parameters:

 float

Range: 0 to 50

Increment: 0.01

*RST: 30

Default unit: dB

Example: POW:SPC:CRAN 15 sets the capture range to +/- 15 dB.

Manual operation: See "Catch Range +/-" on page 159

#### [:SOURCE<hw>]:POWER:SPC:DELAY 

Defines a waiting period between the level adjustment of the generator and the next measurement of the power sensor.

##### Parameters:

 integer

Range: 0 to 1000

*RST: 0

Example: POW:SPC:DEL 2 ms

the sensor starts the next reading 2 ms after the level adjustment.

Manual operation: See "Delay Time" on page 159

#### [:SOURCE<hw>]:POWER:SPC:PEAK 

Activates power control by means of the peak power values, provided the power sensor supports this function.

##### Parameters:

 0 | 1 | OFF | ON

*RST: 0

Example: POW: SPC: PEAK ON

uses the measured peak power for power control.

Manual operation: See "Use Peak Power" on page 159

#### [:SOURCE<hw>]:POWER:SPC:SELECT 

Defines the currently selected sensor to be used for power control.

Parameters:

 SENS1 | SENS2 | SENS3 | SENS4

*RST: SENS1

Example: POW:SPC:SEL SENS2

selects the sensor connected to a second USB interface for power control.

Manual operation: See "Sensor" on page 157

#### [:SOURCE<hw>]:POWER:SPC:STATE 

Activates power control using the selected sensor. The control loop periodically adjusts the generator output. After switching off, the running loop is completed.

##### Parameters:

 0 | 1 | OFF | ON

*RST: 0

Example: POW: SPC: STAT ON

activates power control.

Manual operation: See "State" on page 157

##### [:SOURCE<hw>]:POWER:SPC:TARGET 

Sets the nominal level expected at the input of the sensor. To define the unit of the power value, use command :SENSE<ch>:UNIT[:POWER] on page 330.

##### Parameters:

 float

Range: -50 to 30

Increment: 0.01

*RST: -10

Example: SENS:UNIT dBm
selects unit dBm for setting the target level value.
POW:SPC:TARG -10
sets -10 dBm target level.

Manual operation: See "Target Level" on page 158

#### [:SOURCE<hw>]:POWER:START <Start>

Sets the start level for the RF sweep.

Note: You can select any level within the setting range. The range is defined by this start value and the [:SOURce<hw>] : POWER: STOP value.
A defined offset ([ : SOURce<hw>] : POWER[: LEVel][ : IMMediate]: OFFSET) affects the level values according to the formula:
Minimum level + OFFSET ... Maximum level + OFFSET
Parameters:
<Start> float
Determines the first level value of the sweep setting range.
Range: full specified level range
Increment: see the data sheet: Level sweep > Step size setting resolution
*RST: -30
Example: POW: STAR -20 dBm
sets the start level for the level sweep to -15 dBm for RF output A.
Manual operation: See "Start Level - Level Sweep" on page 190

##### [:SOURCE<hw>]:POWER:STEP[:INCRement] 

Sets the step width for POW: STEP:MODE USER.

To adjust the level step by step with this step size, use the POW:UP and POW:UP commands.

Note: This value also applies to the step width of the rotary knob of the instrument and increases or decreases the level accordingly, when you work in user-defined step mode.

##### Parameters:

 float
Range: full specified level range
Increment: see the data sheet: Level sweep > Step size setting resolution
*RST: 1
Example: POW:STEP 2 sets the step width for entering the RF level to 2 dB.

Manual operation: See "Variation Step" on page 152

#### [:SOURCE<hw>]:POWER:STEP:MODE <Mode>

Activates (USER) or deactivates (DECimal) the user-defined step width used when varying the level value with the level values UP/DOWN. The command is linked to setting "Variation Active" for manual control, i.e. the command also activates/deactivates the user-defined step width used when varying the level value with the rotary knob.

##### Parameters:

<Mode> DECimal | USER

*RST: DECimal

Example: POW: STEP 2

sets the step width for the level setting to 2 dB.

POW: STEP: MODE USER

actives this step width for level variation with the rotary knob (manual control) and with level values UP/DOWN (remote control).

Manual operation: See "Variation Active" on page 152

[:SOURCE<hw>]:POWER:STOP <Stop>

Sets the stop level for the RF sweep.

Note: You can select any level within the setting range. The range is defined by the [ : SOURce<hw>]:POWER:STARt value and this stop value.

A defined offset([:SOURce<hw>]:POWER[:LEVEL][:IMMediate]:OFFSET) affects the level values according to the formula:

Parameters:

<Stop> float

Determines the last level value of the sweep setting range.

Range: full specified level range

Increment: see the data sheet: Level sweep > Step size setting resolution

*RST: -10

Example: POW:STOP 3 sets the stop level for the level sweep to 3 dBm for RF output A.

Manual operation: See "Stop Level - Level Sweep" on page 191

#### 6.13.13 SOURCE:PULM Subsystem

This subsystem contains the commands for setting the pulse modulation.

The external signal is input at the [PULSE EXT] connector. The connector can be used as trigger input for internal pulse modulation. The polarity and input impedance of the connector can be selected. The pulse modulation signal is output at the [PULSE VIDEO] connector.

The LF generator is used as the internal modulation source. The pulse frequency of the internal rectangular signal is therefore set in the SOURce:LFOutput subsystem.

##### Programming Examples

##### Example: Performing pulse modulation

This example shows a command sequence to perform pulse modulation.

// *****
// Reset the instrument to start from an initial state
// *****
*RST; *CLS

// *****
// Set the RF signal frequency and level
// *****
SOURCE: FREQuency: CW 4000000000
SOURCE: POWER: LEVel: IMMediate: AMPLitude -25

// *****
// Configure the pulse modulation settings
// *****
// Select the internal modulation generator
SOURCE: PULM:SOURce INT
// Set trigger mode
SOURCE: PULM:TRIGer:MODE AUTO
// Select pulse mode
SOURCE: PULM:MODE DOUB

// Alternatively configure the pulse modulation settings for
// external modulation source
// *****
// Select the external modulation source
SOURCE:PULM:SOURce EXT
// Set the polarity of the externally applied modulation signal.
SOURCE:PULM:POLarity NORMAL
// Select the impedance for the external pulse modulation trigger input
SOURCE:PULM:TRIGger:EXTERNAL:IMPedance G10K

// *****
// Configure the pulse generator settings
// *****
// Set pulse period

SOURCE: PULM: PERiod 10 us
// Set pulse width
SOURCE: PULM: WIDTH 8 us
// Set double pulse width
SOURCE: PULM: DOUBLE: WIDTH 0.0000012
// Set double pulse delay
SOURCE: PULM: DOUBLE: DELay 0.0000045

// *****
// Activate the signal output
// *****
SOURCE: PGenerator: OUTPUT: STATE 1
SOURCE: PULM: STATE 1
OUTPUT1: STATE 1

#### Example: Generating a pulse train signal

This example shows a command sequence to create a pulse train signal.

// *****
// Reset the instrument to start from an initial state
// *****
*RST; *CLS

// *****
// Set the RF signal frequency and level
// *****
SOURCE: FREQuency: CW 400000000
SOURCE: POWER: LEVel: IMMediate: AMPLitude -25

// Create a pulse train data list
// *****
// Select the directory
MMEM:CDIR '/var/user/Lists/'
// Create and/or select the pulse train data file
SOURCE:PULM:TRAIN:SEL 'P_FIVE'
// Enter the pulse train data
SOURCE:PULM:TRAIN:ONTime 10ns,30ns,40ns,20ns,10ns
SOURCE:PULM:TRAIN:OFFTime 30ns,40ns,50ns,40ns,30ns
SOURCE:PULM:TRAIN:REPetition 10,1,3,10,6

// *****
// Select pulse train mode
// *****
// Select the internal modulation generator and the pulse mode
SOURCE: PULM: SOURce INTernal
SOURCE: PULM: MODE PTRain

// *****
//*****

// Activate the signal output
// *****
SOURCE:PGGenerator:OUTPUT:STATE 1
SOURCE:PULM:STATE 1
OUTPUT1:STATE 1

[:SOURCe<hw>]:PULM:DELAY 395    
[:SOURCe<hw>]:PULM:DOUBLE:DELAY 396    
[:SOURCe<hw>]:PULM:DOUBLE:STATE 396    
[:SOURCe<hw>]:PULM:DOUBLE:WIDTH 396    
[:SOURCe<hw>]:PULM:MODE 397    
[:SOURCe<hw>]:PULM:OUTPUT:SYNC[:STATE] 397    
[:SOURCe<hw>]:PULM:PERiod 397    
[:SOURCe<hw>]:PULM:POLarity 398    
[:SOURCe<hw>]:PULM:SOURCE 398    
[:SOURCe<hw>]:PULM:STATE 399    
[:SOURCe<hw>]:PULM:TRAIN:CATalog 399    
[:SOURCe<hw>]:PULM:TRAIN:DELete 399    
[:SOURCe<hw>]:PULM:TRAIN:OFFTime 400    
[:SOURCe<hw>]:PULM:TRAIN:OFFTime:POINts? 400    
[:SOURCe<hw>]:PULM:TRAIN:ONTime 401    
[:SOURCe<hw>]:PULM:TRAIN:ONTime:POINts? 401    
[:SOURCe<hw>]:PULM:TRAIN:REPetition 402    
[:SOURCe<hw>]:PULM:TRAIN:REPetition:POINts? 402    
[:SOURCe<hw>]:PULM:TRAIN:SELECT 403    
[:SOURCe<hw>]:PULM:TRIGger:EXTERNAL:GATE:POLarity 403    
[:SOURCe<hw>]:PULM:TRIGger:EXTERNAL:IMPedance 404    
[:SOURCe<hw>]:PULM:TRIGger:EXTERNAL:SLOPE 404    
[:SOURCe<hw>]:PULM:TRIGger:MODE 404    
[:SOURCe]:PULM[:INTernal]:TRAIN]:TRIGger:IMMediate 405    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:AFILe:CATalog? 405    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:AFILe:EXTension 406    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:AFILe:SELECT 406    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:AFILe:SEParator:COLumn 407    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:AFILe:SEParator:DECimal 407    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:EXECute 408    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:MODE 408    
[:SOURCe<hw>]:PULM:TRAIN:DEXChange:SELECT 409    
[:SOURCe<hw>]:PULM:WIDTH 409

#### [:SOURCE<hw>]:PULM:DELAY <Delay>

Sets the pulse delay.

Parameters:

<Delay>

float

Range: 0 to 100 s

Increment: 10 ns

*RST: 10 ns

Example: PULM:DEL 13 us
13 us elapse after a trigger before the first pulse is generated.
Options: R&S R&S SMB-K23 (Pulse Generator)
Manual operation: See "Pulse Delay - Pulse Generator" on page 233

#### [:SOURCE<hw>]:PULM:DOUBLE:DELAY <Delay>

Sets the delay from the start of the first pulse to the start of the second pulse.
Parameters:
<Delay> float
Range: 10 ns to 100 s
Increment: 5 ns
*RST: 3 us
Example:
PULM:DOUB:DEL 22 us
22 us elapse between the beginning of the first pulse and the
beginning of the second pulse in double-pulse mode.
Options:
R&S SMB-K23 (Pulse Generator)
Manual operation: See "Double Pulse Delay - Pulse Generator" on page 233

#### [:SOURCE<hw>]:PULM:DOUBLE:STATE <State>

Activates double pulse generation. The two pulses are generated in one pulse period.
Parameters:
<State> 0 | 1 | OFF | ON
*RST: 0
Example:
PULM: DOUB: STAT ON
double-pulse mode is enabled.
Options:
R&S SMB-K23 (Pulse Generator)

#### [:SOURCE<hw>]:PULM:DOUBLE:WIDTH <Width>

Sets the width of the second pulse in case of double pulse generation.

Parameters:
<Width> float
Range: 10 ns to 100 s
Increment: 10 ns
*RST: 3 us
Example:
PULM:DOUB:WIDT 33 us
sets a width of 33 us for the second pulse.
Options:
R&S SMB-K23 (Pulse Generator)
Manual operation: See "Double Pulse Width - Pulse Generator" on page 233

[:SOURce<hw>]:PULM:MODE <Mode>

Sets the mode of the pulse generator.

Parameters:

<Mode>          SINGLE | DOUBLE | PTRain

                          SINGLE

                          Enables single pulse generation.

                          DOUBLE

                          Enables double pulse generation. The two pulses are generated in one pulse period.

                          PTRain

                          A user-defined pulse train is generated The pulse train is defined by value pairs of on and off times that can be entered in a pulse train list.

                          *RST:          SINGLE

Example:

Options: R&S SMB-K23 (Pulse Generator), R&S SMB-K27 (Pulse Train)

Manual operation: See "Pulse Mode - Pulse Generator" on page 232

#### [:SOURCE<hw>]:PULM:OUTPUT:SYNC[:STATE] <Sync>

Configures the signal at the [SIGNAL VALID] connector.

Parameters:

<Sync>

    0 | 1 | OFF | ON

ON

Generates a single pulse at the beginning of a pulse sequence, e.g. to synchronize pulse modulation.

OFF

Returns the validity of the RF signal at the output:

1 (high), while the signal settles.

0 (low), when it is stable (valid).

*RST: OFF

Example:

PULM: OUTP: SYNC ON

uses the signal for synchronizing the pulse modulation.

Manual operation: See "Use SIGNAL VALID as Pulse Sync" on page 235

#### [:SOURCE<hw>]:PULM:PERiod 

Sets the period of the generated pulse. The period determines the repetition frequency of the internal signal.

##### Parameters:

 float

Range: 5 us | 20 ns to 100 s

Increment: 1 us | 5 ns

*RST: 10 us

Example: PULM: PER 220 us

the pulse period is 220 us.

Options: R&S SMB-K23 (Pulse Generator)

Manual operation: See "Pulse Period - Pulse Generator" on page 233

##### [:SOURCE<hw>]:PULM:POLarity 

Sets the polarity between modulating and modulated signal. This command is effective only for an external modulation signal.

##### Parameters:


    NORMAL | INVerted

    NORMAL

    The RF signal is suppressed during the pulse pause.

    INVerted

    The RF signal is suppressed during the pulse.

    *RST:    NORMAL

Example: PULM: SOUR EXT

selects the external modulation source.

Example: PULM: POL INV

selects inverted polarity.

Options: R&S SMB-K22 (Pulse Modulator)

Manual operation: See "Polarity" on page 216

##### [:SOURCE<hw>]:PULM:SOURCE <Source>

Selects the source for the pulse modulation signal.

##### Parameters:

<Source> INTERNAL | EXTERNAL

##### INTernal

The internally generated rectangular signal is used for the pulse modulation. The frequency of the internal signal can be set in the SOURce:LFOutput subsystem.

##### EXTernal

The signal applied externally via the EXT MOD connector is used for the pulse modulation.

*RST: INTernal

Example: PULM: SOUR INT

selects the internal modulation source.

PULM: STAT ON

activates the pulse modulation.

Usage: SCPI confirmed

Options: R&S SMB-K21 or R&S SMB-K22 (Pulse Modulator)

Manual operation: See "Source" on page 216

##### [:SOURCE<hw>]:PULM:STATe <State>

Activates the pulse modulation.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: PULM: STAT ON

activates pulse modulation.

Options: R&S SMB-K21 or R&S SMB-K22 (Pulse Modulator)

Manual operation: See "State" on page 216

##### [:SOURCE<hw>]:PULM:TRAin:CATalog?

Queries a list of available pulse train files. The individual pulse train files are separated by commas.

The files are stored with the fixed file extensions *.pulstrn in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

##### Return values:

<Catalog> string

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:CAT?

queries the available files.

Response: 'P_CONS', 'P_INCR', 'P_DECR'

the lists P_CONS, P_INCR and P_DECR are available.

Usage: Query only

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Pulse Train Data - Pulse Generator" on page 236

##### [:SOURCE<hw>]:PULM:TRAin:DELETE <Filename>

Deletes the specified pulse train file.

The files are stored with the fixed file extensions *.pulstrn in a directory of the user's choice. The directory applicable to the command is defined with the command MMEMORY:CDIR. To access the files in this directory, only the file name has to be given without the path and the file extension.

##### Setting parameters:

<Filename>

<list file name>

Example:

MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:DEL 'P_FIVE'

deletes the list P_FIVE

Usage: Setting only

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Pulse Train Data - Pulse Generator" on page 236

##### [:SOURCE<hw>]:PULM:TRAin:OFFTime <OffTime>

Fills the Off-time part of the selected file with data.

*RST does not affect data lists.

Parameters:

<OffTime>

    Offtime#1{, Offtime#2, ...} | binary block data

    The data can be given either as a list of numbers (list can be of any length and list entries must be separated by commas) or as binary block data.

    When block data is transferred, 8 (4) bytes are always interpreted as a floating-point number with double accuracy (see the command FORMAT: DATA).

    The maximum length is 2047 values.

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:OFFT 10ns, 30ns, 40ns, ...

specifies the off-time values in P_INCR. If the list already contains data, it is overwritten.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Edit Pulse Train Data" on page 237

[:SOURCE<hw>]:PULM:TRAin:OFFTime:POINTs?

Queries the length (in points) of the off-time component of the selected list.

##### Return values:


    integer

    Range: 0 to 2047

    *RST: 0


Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:OFFT:POIN?

queries the number of frequency values in P_INCR

Response: 7

P_INCR has 7 off-time entries.

Usage: Query only

Options: R&S SMB-K27 (Pulse Train)

[:SOURCE<hw>]:PULM:TRAin:ONTime <OnTime>

Fills the On-time part of the selected file with data.

##### Parameters:

<OnTime>

    Ontime#1{, Ontime#2, ...} | binary block data

    The data can be given either as a list of numbers (list can be of any length and list entries must be separated by commas) or as binary block data.

    When block data is transferred, 8 (4) bytes are always interpreted as a floating-point number with double accuracy (see the command FORMat:DATA).

    The maximum length is 2047 values.


Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:ONT 10ns, 30ns, 40ns, ...

specifies the on-time values in P_INCR. If the list already contains data, it is overwritten.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Edit Pulse Train Data" on page 237

##### [:SOURCE<hw>]:PULM:TRAin:ONTime:POINTs?

Queries the length (in points) of the ontime component of the selected list.

##### Return values:

 integer

Range: 0 to 2047

*RST: 0

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:ONT:POIN?

queries the number of frequency values in P_INCR

Response: 7

P_INCR has 7 ontime entries.

Usage: Query only

Options: R&S SMB-K27 (Pulse Train)

##### [:SOURCE<hw>]:PULM:TRAin:REPetition <Repetition>

Sets the number of repetitions for each ontime/offtime value pair.

*RST does not affect data lists.

Tip:"0" ignores the corresponding value pair in the pulse train. Thus, you can individually omit value pairs without deleting them from the table.

##### Parameters:

<Repetition> Repetition#1{, Repetition#2, ...}

Range: 0...65535

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:ONT 10ns, 30ns, 40ns, ...

specifies the ontime values in P_INCR. If the list already contains data, it is overwritten.

PULM:TRA:OFFT 10ns, 30ns, 40ns, ...

specifies the oftime values in P_INCR. If the list already contains data, it is overwritten.

PULM:TRA:REP 1, 8, 3, ...

specifies the number of repetitions for each value pair.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Edit Pulse Train Data" on page 237

#### [:SOURCE<hw>]:PULM:TRAIN:REPETITION:POINTS?

Queries the length (in points) of the repetition component of the selected list.

Return values:

 integer

Range: 0 to INT_MAX

*RST: 0

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

PULM:TRA:REP:POIN?

queries the number of repetition values in P_INCR

Response: 7

P_INCR has 7 repetition entries.

Usage: Query only

Options: R&S SMB-K27 (Pulse Train)

##### [:SOURCE<hw>]:PULM:TRAin:SELECT <Filename>

Selects the specified pulse train file. If a new file is to be created, the name can be entered here. The file is created if it does not yet exist. The file selected here is available for the further processing steps (editing) and is used in the instrument when the pulse train mode is activated.

The files are stored with the fixed file extensions *.pulstrn in a directory of the user's choice. The directory applicable to the command is defined with the command MMEMORY:CDIR.

*RST does not affect data lists.

##### Parameters:

<Filename> string

Example: MMEM:CDIR '/var/user/Lists'

selects the directory for the pulse train files.

PULM:TRA:SEL 'P_INCR'

selects P_INCR for editing. P_INCR is created if it does not yet exist.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Pulse Train Data - Pulse Generator" on page 236

#### [:SOURCE<hw>]:PULM:TRIGger:EXTERNAL:GATE:POLarity 

Selects the polarity of the Gate signal.

The signal is supplied via the [PULSE EXT] connector.

Parameters:

 NORMAL | INVerted

*RST: NORMAL


| Example: | PULM:TRIG:EXT:GATE:POL NORMThe pulse signal is generated while the gate signal is high. |
| Options: | R&S SMB-K23 (Pulse Generator) |
| Manual operation: | See "Gate Input Polarity - Pulse Generator" on page 234 |

#### [:SOURce<hw>]:PULM:TRIGger:EXTERNAL:IMPedance 

Selects the impedance for external pulse trigger.

Parameters:

 G50 | G10K

*RST: G50

Example:

SOUR: PULM: TRIG: EXT: IMP G50

selects 50 Ohm as the trigger impedance for the external pulse trigger.

Options:

R&S SMB-K21 or R&S SMB-K22 (Pulse Modulator)

Manual operation: See "External Impedance" on page 234

#### [:SOURCE<hw>]:PULM:TRIGger:EXTERNAL:SLOPe <Slope>

Sets the polarity of the active slope of an applied trigger at the [PULSE EXT] connector.

##### Parameters:

<Slope> NEGative | POSitive

*RST: POSitive

Example: PULM:TRIG:EXT:SLOP NEG

The pulse generator is triggered on the negative slope of the external trigger signal.

Options: R&S SMB-K23 (Pulse Generator)

Manual operation: See "External Trigger Input Slope - Pulse Generator" on page 234

#### [:SOURCE<hw>]:PULM:TRIGger:MODE <Mode>

Selects the trigger mode for pulse modulation.

Parameters:

<Mode> AUTO | EXternal | EGATe | SINGle

AUTO

The pulse modulation is generated continuously.

EXTERNAL

The pulse modulation is triggered by an external trigger event.

The trigger signal is supplied via the [PULSE EXT] connector.

##### EGATe

The pulse modulation is gated by an external gate signal. The signal is supplied via the [PULSE EXT] connector.

##### SINGLE

Pulse modulation is generated once.

Example: PULM:TRIG:MODE EXT

selects triggering by an external trigger event.

Options: R&S SMB-K23 (Pulse Generator)

Manual operation: See "Trigger Mode - Pulse Generator" on page 233

#### [:SOURCE]:PULM[:INTernal][:TRAin]:TRIGger:IMMediate

Initiates an internal single trigger signal for the pulse generator.

Example: PULM:TRIG:MODE SING

PULM:TRIG:IMM

Manual operation: See "Execute Single Trigger" on page 234

##### [:SOURCE<hw>]:PULM:TRAin:DEXChange:AFILe:CATalog?

Requests a list of available ASCII files for export/import of pulse train data. The individual files are separated by commas.

The ASCII files are stored with the fixed file extensions *.txt or *.csv in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

##### Return values:

<Catalog> string

Example: MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with ontime/offtime/repetition values.

PULM:TRA:DEXC:AFIL:EXT TXT

selects that ASCII files with extension *.txt are listed.

PULM:TRA:DEXC:AFIL:CAT?

queries the available files with extension *.txt.

Response: 'train1', 'train2'

the ASCII files train1.txt and train2.txt are available.

Usage: Query only

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Select ASCII Source / Destination - Import/Export Pulse Train Files" on page 239

#### [:SOURce<hw>]:PULM:TRAin:DEXChange:AFILe:EXTension <Extension>

Selects the file extension of the ASCII file to be imported or exported. Selection TXT (text file) or CSV (Excel file) is available.

Parameters:

<Extension> TXT | CSV

*RST: TXT

<Extension>

    *RST: TXT

    Example: MMEM:CDIR '/var/user/Lists/import'

    selects the directory for the ASCII files with ontime/oftime/repetition values.

    PULM:TRA:DEXC:AFIL:EXT TXT

    selects that ASCII files with extension *.txt are listed.

    PULM:TRA:DEXC:AFIL:CAT?

    queries the available files with extension *.txt.

    Response: 'train1', 'train2

    the ASCII files train1.txt and train2.txt are available.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Extension - ASCII File Settings" on page 238

##### [:SOURCE<hw>]:PULM:TRAin:DEXChange:AFILe:SELECT <Filename>

Selects the ASCII file to be imported or exported.

The ASCII files are stored with the fixed file extensions *.txt or *.csv in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

##### Parameters:

<Filename> string

Example: MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with ontime/offtime/repetition values.

PULM:TRA:DEXC:MODE IMP

selects that ASCII files with ontime/offtime/repetition values are imported and transferred into pulse train lists.

PULM:TRA:DEXC:AFIL:SEL 'train.csv'

selects that ASCII file train.csv is imported.

PULM:TRA:DEXC:SEL 'train_imp'

selects that the ASCII file train.csv is imported into pulse train list train imp.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Select ASCII Source / Destination - Import/Export Pulse Train Files" on page 239

#### [:SOURCE<hw>]:PULM:TRAin:DEXChange:AFILe:SEParator:COLumn <Column>

##### Parameters:

<Column> TABulator | SEMicolon | COMMa | SPACe

*RST: SEMicolon

Example:

PULM:TRA:DEXC:MODE EXP

selects that the pulse train list is exported into an ASCII file.

MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with ontime/offtime/repetition values.

PULM:TRA:DEXC:AFIL:SEL 'train.csv'

selects ASCII file train.csv as destination for the pulse train list data.

PULM:TRA:DEXC:AFIL:SEP:COL TAB

the ontime/offtime/repetition values are separated by a tabulator.

PULM:TRA:DEXC:AFIL:SEP:DEC DOT

selects the decimal separator dot.

PULM:TRA:DEXC:SEL 'train_imp'

selects that the pulse train list train_imp is imported into ASCII file train.csv.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Column Separator- ASCII File Settings" on page 239

[:SOURCE<hw>]:PULM:TRAIN:DEXChange:AFILe:SEPARator:DECimal <Decimal>

Select the decimal separator used in the ASCII data between '.' (decimal point) and ',' (comma) with floating-point numerals.

##### Parameters:

<Decimal> DOT | COMMa

*RST: DOT

Example: PULM:TRA:DEXC:MODE EXP

selects that the pulse train list is exported into an ASCII file.

MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with on-time/off-time/repetition values.

PULM:TRA:DEXC:AFIL:SEL 'train.csv'

selects ASCII file train.csv as destination for the pulse train list data.

PULM:TRA:DEXC:AFIL:SEP:COL TAB

the ontime/offtime/repetition values are separated by a tabulator.

PULM:TRA:DEXC:AFIL:SEP:DEC DOT

selects the decimal separator dot.

PULM:TRA:DEXC:SEL 'train_imp'

selects that the pulse train list train_imp is imported into ASCII file train.csv.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Decimal Point - ASCII File Settings" on page 239

#### [:SOURCE<hw>]:PULM:TRAin:DEXChange:EXECute

Starts the export or import of the selected file. When import is selected, the ASCII file is imported as pulse train list. When export is selected, the pulse train list is exported into the selected ASCII file.

Example: PULM:TRA:DEXC:MODE IMP

selects that ASCII files with ontime/offtime/repetition values are

imported and transferred into pulse train lists.

MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with on-time/off-time/

repetition values.

PULM:TRA:DEXC:AFIL:SEL 'train.csv'

selects that ASCII file train.csv is imported.

PULM:TRA:DEXC:SEL 'train_imp'

selects that the ASCII file train.csv is imported into pulse

train list train_imp.

PULM:TRA:DEXC:EXEC

starts the import of the ASCII file data into the pulse train file.

Usage: Event

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Import / Export - Import/Export Pulse Train Files" on page 239

#### [:SOURCE<hw>]:PULM:TRAin:DEXChange:MODE <Mode>

Selects if pulse train lists should be imported or exported. Depending on the selection, the file select command define either the source or the destination for pulse train lists and ASCII files.

##### Parameters:

<Mode>

    IMPORT | EXPort

    *RST:    IMPORT

Example: PULM:TRA:DEXC:MODE IMP

selects that ASCII files with ontime/offtime/repetition values are

imported and transferred into pulse train lists.

MMEM:CDIR '/var/user/Lists/import'

selects the directory for the ASCII files with ontime/offtime/repetition values.

PULM:TRA:DEXC:AFIL:SEL 'train.csv'

selects that ASCII file train.csv is imported.

PULM:TRA:DEXC:SEL 'train_imp'

selects that the ASCII file train.csv is imported into pulse train list train_imp.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Mode - Import/Export Pulse Train Files" on page 238

[:SOURCE<hw>]:PULM:TRAin:DEXChange:SELECT <Filename>

Selects the pulse train list to be imported or exported.

The pulse train files are stored with the fixed file extensions *.pulstrn in a directory of the user's choice. The directory applicable to the commands is defined with the command MMEMORY:CDIR.

##### Parameters:

<Filename> string

Example: PULM:TRA:DEXC:MODE IMP

    selects that ASCII files with ontime/oftime/repetition values are

    imported and transferred into pulse train lists.

    MMEM:CDIR '/var/user/Lists/import'

    selects the directory for the ASCII files with ontime/oftime/repetition values.

    PULM:TRA:DEXC:AFIL:SEL 'train.csv'

    selects that ASCII file train.csv is imported.

    PULM:TRA:DEXC:SEL 'train_imp'

    selects that the ASCII file train.csv is imported into pulse train list train_imp.

Options: R&S SMB-K27 (Pulse Train)

Manual operation: See "Select Destination / Source - Import/Export Pulse Train Files" on page 239

##### [:SOURCE<hw>]:PULM:WIDTH <Width>

Sets the width of the generated pulse. The width determines the pulse length. The pulse width must be at least 20ns less than the set pulse period.

##### Parameters:

<Width> float

Range: 10 ns to 100 s

Increment: 10 ns

*RST: 2 us

Example: PULM:WIDT 33 us

sets a width of 33 us for the pulse.

Options: R&S SMB-K23 (Pulse Generator)

Manual operation: See "Pulse Width - Pulse Generator" on page 233

#### 6.13.14 SOURce:ROSCillator Subsystem

This subsystem contains the commands for setting the external and internal reference frequency.

The settings of the reference oscillator are not affected by an instrument reset (*RST on page 286). They are only reset to factory state by the factory-preset (: SYSTEM: FPReset on page 289).

[:SOURce]:ROSCillator:EXTernal:FREQUency.....410

[:SOURce]:ROSCillator:EXTernal:RFOF[:STATE].....410

[:SOURce]:ROSCillator:EXTernal:SBANwidth.....411

[:SOURce]:ROSCillator[:INTernal]:ADJust:VALue.....411

[:SOURce]:ROSCillator[:INTernal]:ADJust[:STATE].....411

[:SOURce]:ROSCillator:SOURce.....412

#### [:SOURCE]:ROSCillator:EXTERNAL:FREQUENCY <Frequency>

Selects the external reference frequency.

Note: The installed hardware determines the available settings. Use the Hardware Config dialog to check the hardware the instrument is equipped with.

For information on the required hardware revision, refer to the release notes.

Parameters:

<Frequency> 5MHz | 10MHz

*RST: n.a. (factory preset: 10MHz)

Example: ROSC: SOUR EXT

Selects the external source. The reference must be input at the REF IN input.

ROSC:EXT:FREQ 10MHz

Selects 10 MHz external reference frequency.

Manual operation: See "External Reference Frequency" on page 145

##### [:SOURCE]:ROSCillator:EXTERNAL:RFOFf[:STATE] <State>

Activates that RF output is automatically switched off, when in external source mode no reference signal is supplied.

This setting ensures that no improper RF signal due to the missing external reference signal is output and used for measurements.

In addition to the error message "Ext Ref missing", the instrument generates the message "RF output deactivated".

##### Parameters:

<State>

    0 | 1 | OFF | ON

    *RST: n.a. (factory preset: 0)

Example: ROSC: SOUR EXT

Selects the external source. The reference must be input at the REF IN input.

Example: ROSC:EXT:RFOF:STAT ON

If the external signal is missing, no RF signal is output.

Manual operation: See "Deactivate RF Output (if external reference is missing)" on page 145

Sets the synchronization bandwidth for an external reference signal.
Parameters:
<SBandwidth>
    WIDE | NARROW
    NARROW
    The synchronization bandwidth is approx. 50 Hz.
    WIDE
    The synchronization bandwidth is approx. 350 Hz.
    *RST: n.a. (factory preset)
Example:
    ROSC: SOUR EXT
    Selects the external source.
    ROSC: EXT: FREQ 10 MHz
    Informs the instrument that the external reference has a frequency of 10 MHz.
    ROSC: EXT: SBAN WID
    Selects wideband setting for synchronization bandwidth.
Manual operation: See "Synchronization Bandwidth" on page 145

[:SOURCE]:ROSCillator[:INTernal]:ADJust:VALUE <Value>

Specifies the frequency correction value (adjustment value).

Parameters:
<Value> integer
Range: 0 to maximum value (see data sheet)
Increment: see data sheet
*RST: ---
Example: ROSC:ADJ:VAL 456
Sets the adjustment value to 456.
Manual operation: See "Adjustment DAC Value" on page 146

#### [:SOURCE]:ROSCillator[:INTernal]:ADJust[:STATE] <State>

Determines whether the calibrated (OFF) or a user-defined (ON) adjustment value is used for fine adjustment of the frequency.

If user-defined values are used, the instrument is no longer in the calibrated state. However, the calibration value is not changed and the instrument resumes the calibrated state after sending the com-

mand : SOURce: ROSCillator: INTernal: ADJust: STATe OFF.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 0)

Example: ROSC: SOUR INT

Selects the internal source.

ROSC: ADJ ON

Activates use of a user-defined adjustment value.

ROSC: ADJ: VAL 1400

Sets the adjustment value to 1400.

Manual operation: See "Adjustment Active" on page 145

[:SOURCE]:ROSCillator:SOURCE <Source>

Selects the reference frequency source.

Parameters:

<Source> INTernal | EXternal

INTernal

The internal reference oscillator is used.

EXTernal

An external reference signal is used. It must be input at the [REF IN] connector at the rear of the instrument.

The instrument is informed of the frequency of the external reference signal with the command [: SOURce]: ROSCillator:

EXTernal: FREQuency.

Example: ROSC: SOUR EXT

Selects the external source.

ROSC:EXT:FREQ 5 MHz

Informs the instrument that the external reference has a frequency of 5 MHz.

Manual operation: See "Source" on page 144

#### 6.13.15 SOURCE:STEReo Subsystem

This subsystem contains the SCPI commands for generating FM stereo multiplex signals, the radio traffic service ARI (Automotive Radio Information) and Radio Data System (RDS). Additional functions are available using the SOURce:STEREO:DIRECT commands (see Chapter 6.19, "Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5", on page 464).

[:SOURce]:STEReo:ARl:BK[:CODE].....413

[:SOURce]:STEReo:ARl:STATE.....413

[:SOURce]:STEReo:ARl:TYPE.....414

[:SOURce]:STEReo:ARl:TYPE:STATE.....414

[:SOURce]:STEReo:ARl[:DEViation].....414

[:SOURCE]:STEReo:AUDIO:MODE.....415

[:SOURCE]:STEReo:AUDIO:PREemphasis.....415

[:SOURCE]:STEReo:AUDIO:PREemphasis:STATE.....416

[:SOURCE]:STEReo:AUDIO:FREQUENCY].....416

[:SOURCE]:STEReo:DIRect.....417

[:SOURCE]:STEReo:EXTernal:IMPedance.....417

[:SOURCE]:STEReo:MMF.....417

[:SOURCE]:STEReo:PILot:PHASE.....418

[:SOURCE]:STEReo:PILot:STATE.....418

[:SOURCE]:STEReo:PILot[:DEViation]].....418

[:SOURCE]:STEReo:RDS:DATaset.....418

[:SOURCE]:STEReo:RDS:STATE.....419

[:SOURCE]:STEReo:RDS:TRAFFic:ANNouncement[:STATE]].....419

[:SOURCE]:STEReo:RDS:TRAFFic:PROGRAM[:STATE]].....419

[:SOURCE]:STEReo:RDS[:DEViation]].....420

[:SOURCE]:STEReo:SOURCE.....420

[:SOURCE]:STEReo:STATE.....421

[:SOURCE]:STEReq[:DEViation]].....421

#### [:SOURCE]:STEReo:ARI:BK[:CODE] <Code>

Selects the area identification (BK) code of the ARI signal. The six letters (six different frequencies) identify a specific region in each country. The code is generated if the BK or DK+BK identifier of the ARI signal is activated.

Parameters:

<Code> A | B | C | D | E | F

*RST: A

Example: STER:ARI:TYPE BK

selects generation of area identification.

STER:ARI:BK A

selects the specific area identification code A to be generated.

Options: R&S SMB-B5

Manual operation: See "ARI BK - Stereo Modulation" on page 221

#### [:SOURCE]:STEReo:ARI:STATe <State>

Activates/deactivates the ARI signal generation. ARI signals can be generated simultaneously with MPX and RDS signals.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: OFF

Example: STER:ARI:STAT ON

activates generation of an ARI signal.

Options: R&S SMB-B5

Manual operation: See "ARI State - Stereo Modulation" on page 221

[:SOURCE]:STEReo:ARI:TYPE <Type>

Selects the generated identifiers of the ARI signal.

Parameters:

<Type> OFF | DK | BK | BKDK

OFF

Only the 57 kHz subcarrier is generated (Senderkennung). It marks the stations which broadcast traffic programs and enables the receiver to recognize the frequency as being ARI-capable.

DK

The message identification (Durchsagekennung) is generated in addition (low-frequency 30% AM). It signalizes that a traffic message is currently broadcasted.

BK

The area identification (Bereichskennung) is generated in addition (60% AM). This code is used to identify the geographical region covered by the radio station. The specific code is selected below.

BKDK

The area and message identification are generated in addition.

*BST: DK

Example: STER:ARI:TYPE BKDK

A complete ARI signal with all identifiers is generated.

Options: R&S SMB-B5

Manual operation: See "ARI Identification - Stereo Modulation" on page 221

##### [:SOURCE]:STEReo:ARI:TYPE:STATE <State>

Activates/deactivates the Stereo ARI Identifier.

##### Parameters:

<State>

0 | 1 | OFF | ON

*RST: 0

Example: STER:ARI:TYPE:STAT ON

Options: R&S SMB-B5

#### [:SOURCE]:STEReo:ARI[:DEViation] <Deviation>

Sets the frequency deviation of the ARI subcarrier signal.

Parameters:

<Deviation> integer

Range: 0 to 10000

*RST: 3500

Example: STER:ARI:DEV 3.5kHz

sets the frequency deviation of the 57 kHz subcarrier to 3.5kHz.

Options: R&S SMB-B5

Manual operation: See "ARI Deviation - Stereo Modulation" on page 221

[:SOURCE]:STEReo:AUDIO:MODE <Mode>

Selects the generated identifiers of the AUDIO signal.

##### Parameters:


| <Mode> | LEFT \| RIGHT \| RELeft \| RELeft \| RNELeft |
|  | LEFT |
|  | A mono signal containing the left channel is generated/fed in. |
|  | RIGHT |
|  | A mono signal containing the right channel is generated/fed in. |
|  | RELeft |
|  | A stereo signal with right and left channel is generated/fed in. |
|  | The channels have the same frequency and phase. |
|  | REMLeft |
|  | The signal on the left external audio input is used for both channels, left and right. The right channel is inverted. |
|  | RNELeft |
|  | (External source only) |
|  | A stereo signal containing different, independent right and left channels is feed in. It is possible, for example, to feed a fixed audio frequency to the first channel while a frequency sweep is being performed in the second channel. |
|  | *RST: RIGHT |
| Example: | STER: SOUR LFG |
|  | The internal LF generator is used as modulation source for the audio signal. |
|  | STER: AUD: MODE RIGH |
|  | A mono signal containing the left channel is generated. |

Options: R&S SMB-B5

Manual operation: See "Mode - Stereo Modulation" on page 219

#### [:SOURCE]:STEReo:AUDIO:PREemphasis 

Sets the preemphasis used for signal generation.

Parameters:

 float

Range: 50 us to 75 us

Example: STER: SOUR LFG
The internal LF generator is used as modulation source for the audio signal.
STER: AUD: PRE 50µs
sets preemphasis to 50µs.
STER: AUD: PRE: STAT ON
activates preemphasis.
Options: R&S SMB-B5
Manual operation: See "Preemphasis - Stereo Modulation" on page 219

##### [:SOURCE]:STEReo:AUDIO:PREemphasis:STATE <State>

Activates the use of preemphasis for signal generation.

Parameters:
<State> 0 | 1 | OFF | ON
*RST: OFF

Example:
STER: SOUR LFG
The internal LF generator is used as modulation source for the audio signal.
STER: AUD: PRE 50µs
sets preemphasis to 50µs.
STER: AUD: PRE: STAT ON
activates preemphasis.

Options:
R&S SMB-B5

Manual operation: See "Preemphasis - Stereo Modulation" on page 219

#### [:SOURce]:STEReo:AUDIO[:FREQUency] <Frequency>

Sets the frequency of the LF generator signal. The command is an alias to command SOURce:LFOutput:FREQUency.

Parameters:
<Frequency> float
Range: 0.1 to 1E6
Increment: 0.01
*RST: 1000

Example:
STER: SOUR LFG
The internal LF generator is used as modulation source for the audio signal.
STER: AUD: FREQ 1100
sets the frequency of the audio signal to 1.1 kHz

Options:
R&S SMB-B5

Manual operation: See "LF Gen Freq - Stereo Modulation" on page 219

#### [:SOURCE]:STEReo:DIRECT <Direct>

Sends a R&S SMB command string to the stereo coder. The direct commands offer extended settings possibilities for the stereo coder (see Chapter 6.19, "Direct Commands for the Stereo/RDS Coder Option R&S SMB-B5", on page 464).

##### Parameters:

<Direct> string

Example: STER:DIR 'ARI-ID=0'

deactivates the ARI identification.

Options: R&S SMB-B5

Manual operation: See "RDS Program Service Name - Stereo Modulation" on page 222

#### [:SOURCE]:STEReo:EXTERNAL:IMPedance 

Selects the input impedance for the external analog audio signal inputs L and R.

##### Parameters:

 600 | 600Ohm | 100000 | 100kOhm | 100000Ohm

*RST: 100000

Example: SOUR:STER:EXT:IMP 6000hm

selects 600 OHM as the impedance for the external analog audio signals.

Options: R&S SMB-B5

Manual operation: See "External R/L Impedance - Stereo Modulation" on page 219

#### [:SOURCE]:STEReo:MMF <Mmf>

Sets the maximum possibly used modulation frequency. This setting is only effective for external modulation source and activated preemphasis. It prevents over modulation but result in a decreased s/n ratio.

##### Parameters:

<Mmf> integer

Range: 1000 to 18000

*RST: 1000

Example: SOUR:STER:MMF 2000

sets a maximum modulation frequency of 2 kHz.

Options: R&S SMB-B5

Manual operation: See "Max Modulation Freq- Stereo Modulation" on page 220

#### [:SOURCE]:STEReo:PILot:PHASE 

Sets the phase of the pilot tone in degrees, in relation to the 38 kHz carrier signal of the receiver. For a correct demodulation, the pilot tone must be in phase with the 38 kHz carrier.

Parameters:
 float
Range: -5 to 5
Increment: 0.1
*RST: 0

Example: SOUR:STER:PIL:PHAS .2DEG
decreases pilot tone quality by adding a phase difference of 0.2 degrees between pilot signal and receiver carrier signal.

Options: R&S SMB-B5

Manual operation: See "Pilot Phase - Stereo Modulation" on page 220

#### [:SOURCE]:STEReo:PILot:STATE <State>

Activates/deactivates the pilot tone generation.

Parameters:
<State> 0 | 1 | OFF | ON
*RST: OFF

Example:
STER:PIL:STAT ON
activates generation of the pilot tone.

Options:
R&S SMB-B5

Manual operation: See "Pilot State - Stereo Modulation" on page 220

##### [:SOURCE]:STEReo:PILot[:DEViation] <Deviation>

Sets the deviation of the pilot tone.

<Deviation> integer

Range: 0 to 10 kHz

*RST: 6.75 kHz

Example: SOUR:STER:PIL:DEV 6.75kHz sets the pilot tone deviation according to standard.

Options: R&S SMB-B5

Manual operation: See "Pilot Deviation - Stereo Modulation" on page 220

#### [:SOURCE]:STEReo:RDS:DATaset <Dataset>

Selects one of the five data sets provided on the instrument for use in the RDS signal.

##### Parameters:

<Dataset> DS1 | DS2 | DS3 | DS4 | DS5

*RST: DS1

Example: STER:RDS:DAT DS5

activates use of data set 5 for generation of the RDS signal.

Options: R&S SMB-B5

Manual operation: See "RDS Data Set - Stereo Modulation" on page 222

##### [:SOURCE]:STEReo:RDS:STATE <State>

Activates/deactivates the RDS signal generation. RDS signals can be generated simultaneously with MPX and ARI signals.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: OFF

Example: STER:RDS:STAT ON

activates generation of RDS signal.

Options: R&S SMB-B5

Manual operation: See "RDS State - Stereo Modulation" on page 221

#### [:SOURCE]:STEReo:RDS:TRAFfic:ANNouncement[:STATE] <State>

Activates the RDS traffic announcement. If activated, the receiver switches from the current status, e.g. playing a CD, to the receive mode and enables the broadcast of a traffic announcement. The TP state has to be on.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: OFF

Example: STER:RDS:TRAF:PROG:STAT ON activates RDS traffic program.

STER:RDS:TRAF:ANN:STAT ON activates RDS traffic announcement.

Options: R&S SMB-B5

Manual operation: See "RDS Traffic Announcement State - Stereo Modulation" on page 223

##### [:SOURCE]:STEReo:RDS:TRAFfic:PROGRAM[:STATE] <State>

Activates the RDS traffic program. The receiver can recognize a frequency as being capable of traffic information only if the TP function is active.

##### Parameters:

<State>

0 | 1 | OFF | ON

*RST: OFF

Example: STER:RDS:TRAF:PROG:STAT ON activates the RDS traffic program.

Options: R&S SMB-B5

Manual operation: See "RDS Traffic Program State - Stereo Modulation" on page 222

##### [:SOURCE]:STEReo:RDS[:DEViation] <Deviation>

Sets the deviation of the RDS subcarrier.

##### Parameters:

<Deviation> integer

Range: 0 to 10 kHz

*RST: 2 kHz

Example: SOUR:STER:RDS:DEV 2kHz

sets the RDS signal deviation according to standard.

Manual operation: See "RDS Deviation - Stereo Modulation" on page 222

##### [:SOURCE]:STEReo:SOURCE <Source>

Selects the source for the audio signal.

##### Parameters:

<Source>

OFF | LREXt | SPEXt | LFGen

OFF

No audio signal is provided, ARI and RDS signal can be generated separately.

LREX

The external audio signal is feed in via the analog L and R inputs.

SPEX

The external audio signal is feed in via the digital S/P DIF interface.

LFG

The audio stereo signal is internally generated by the LF generator.

*PST* LPEYt

Example: STER: SOUR LFG

The internal LF generator is used as modulation source for the audio signal.

Options: R&S SMB-B5

Manual operation: See "Audio Source - Stereo Modulation" on page 218

[:SOURCE]:STEReo:STATE <State>

Activates/deactivates stereo modulation.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: OFF

Example: STER:STAT ON

activates generation of stereo signal.

Options: R&S SMB-B5

Manual operation: See "State - Stereo Modulation" on page 218

[:SOURCE]:STEReo[:DEViation] <Deviation>

Sets the MPX (Multiplex stereo signal) deviation.

<Deviation> integer

Range: 0 to depends on instrument hardware

*RST: 40 kHz

Example: STER 40kHz

sets the stereo deviation according to standard.

Options: R&S SMB-B5

Manual operation: See "FM Deviation - Stereo Modulation" on page 218

#### 6.13.16 SOURce:SWEep Subsystem

The SOURCE: subsystem contains the commands for configuring RF sweep signals.

- The keyword [ :FREQUENCY] can be omitted, then the commands are SCPI-compliant.

- To activate a RF sweep mode, use the following commands:

– RF frequency sweep: SOURCE: FREQUENCY: MODE SWEEP (SOURCE: FREQUENCY: MODE CW (off))

– RF level sweep: SOURCE: POWER: MODE SWEep (SOURCE: POWER: MODE CW (off))

- All sweeps, including the LF sweep, can be set independently from each other.

This example shows how to set up a frequency sweep.

1. Set the sweep range.

SOUR: FREQ: CENT 200 MHz

SOUR: FREQ: SPAN 300 MHz

2. Select linear or logarithmic spacing.

SOUR: SWE: FREQ: SPAC LIN

3. Set the step width and dwell time.

SOUR: SWE: FREQ: STEP: LIN 20 MHz

SOUR: SWE: FREQ: DWEL 12 ms

4. Select the trigger mode.

TRIG:FSW:SOUR SING

5. Select the sweep mode and activate the sweep.

SOUR: SWE: FREQ: MODE AUTO

SOUR: FREQ: MODE SWE

6. Trigger the sweep.

SOURCE: SWE: FREQ: EXEC

It is recommended that you switch off the "Start/Stop Display Update" for optimum sweep performance, especially with short dwell times (SYST:DISP:UPD OFF).

[:SOURCE<hw>]:SWEep[:FREQUency]:DWELI.....422

[:SOURCE<hw>]:SWEep[:FREQUency]:EXECute.....423

[:SOURCE<hw>]:SWEep[:FREQUency]:LConnector.....423

[:SOURCE<hw>]:SWEep[:FREQUency]:MODE.....424

[:SOURCE<hw>]:SWEep[:FREQUency]:OVOLtage:START.....424

[:SOURCE<hw>]:SWEep[:FREQUency]:OVOLtage:STOP.....425

[:SOURCE<hw>]:SWEep[:FREQUency]:POINts.....425

[:SOURCE<hw>]:SWEep[:FREQUency]:RETRace.....426

[:SOURCE<hw>]:SWEep[:FREQUency]:RUNNING?.....426

[:SOURCE<hw>]:SWEep[:FREQUency]:SHAP.....426

[:SOURCE<hw>]:SWEep[:FREQUency]:SPACing.....427

[:SOURCE<hw>]:SWEep[:FREQUency]:STEP[:LINear].....427

[:SOURCE<hw>]:SWEep[:FREQUency]:STEP:LOGarithmic.....428

[:SOURCE<hw>]:SWEep:POWER:DWELI.....429

[:SOURCE<hw>]:SWEep:POWER:EXECute.....429

[:SOURCE<hw>]:SWEep:POWER:MODE.....430

[:SOURCE<hw>]:SWEep:POWER:POINts.....430

[:SOURCE<hw>]:SWEep:POWER:RETRace.....431

[:SOURCE<hw>]:SWEep:POWER:RUNNING?.....431

[:SOURCE<hw>]:SWEep:POWER:SHAPE.....431

[:SOURCE<hw>]:SWEep:POWER:SPACing:MODE?.....432

[:SOURCE<hw>]:SWEep:POWER:STEP[:LOGarithmic].....432

[:SOURCE<hw>]:SWEep:RESET:ALL].....433

[:SOURCE<hw>]:SWEep[:FREQUency]:DWELI <Dwell>

Sets the time taken for each frequency step of the sweep.

The keyword [ : FREQuency] can be omitted (see example). The command is then SCPI-compliant.

Tip: It is recommended to switch off the "Display Update" for optimum sweep performance especially with short dwell times (SYSTEM:DISPLAY:UPDATE OFF).

Parameters:
<Dwell> float
Range: 2E-3 to 100
Increment: 100E-6
*RST: 15E-3

Example:
SWE:DWEL 12 ms
sets a dwell time of 12 ms for a frequency sweep at the RF output.

Manual operation: See "Dwell Time - Frequency Sweep" on page 186

#### [:SOURCE<hw>]:SWEep[:FREQUency]:EXECute

Starts an RF frequency sweep cycle manually.
The command is only effective in single mode.
Example:
TRIG:FSW:SOUR SING
SOUR:SWE:FREQ:MODE AUT
SWE:FREQ:EXEC
triggers a frequency sweep at the RF output.
Usage:
Event
Manual operation: See "Execute Single Sweep - Frequency Sweep" on page 183

#### [:SOURce<hw>]:SWEep[:FREQUency]:LFConnector <LfConnector>

Activates the output of a sweep voltage ramp at the LF connector.

The voltage range is set with commands
SOURCE: SWEep: FREQuency: OVOLtage: STARt and ...: STOP
Parameters:
<LfConnector> 0 | 1 | OFF | ON
*RST: 0
Example: SWE:LFC ON
activates the output of a linear voltage ramp from sweep start to
sweep stop at the LF connector.
SWE:OVOL:STAR 0V
SWE:OVOL:STOP 3V
'the voltage at sweep start is 0 Volt and at sweep stop 3 V.
Manual operation: See "Use LF connector to output sweep voltage - RF Frequency
Sweep" on page 186

#### [:SOURCE<hw>]:SWEep[:FREQUency]:MODE <Mode>

Sets the sweep mode.

The keyword [ : FREQuency ] can be omitted (see example). The command is then SCPI-compliant.

##### Parameters:

<Mode> AUTO | MANual | STEP

AUTO

Each trigger triggers exactly one complete sweep.

##### MANual

The trigger system is not active. Each frequency step of the sweep is triggered individually, either by varying the "Current Frequency" value using the rotary knob under manual control or by means of a FREQ:MAN command under remote control. With manual control, the frequency increases or decreases (depending on the direction of the rotary encoder) by the value specified under FREQ:STEP:INCREment. With remote control, the frequency is set directly with the command : FREQ:MAN.

##### STEP

Each trigger triggers one sweep step only (Mode Single Step). The frequency increases by the value entered under SOUR:SWE:FREQ:STEP:LIN (linear spacing) or ... : STEP:LOG (logarithmic spacing).

Example: SWE:MODE AUTO

selects Mode Auto for a frequency sweep at the RF output.

Manual operation: See "Mode - RF Frequency Sweep" on page 180

##### [:SOURCE<hw>]:SWEep[:FREQUency]:OVOLtage:START <Start>

Sets the voltage at the sweep stop frequency. The linear voltage ramp from sweep start to stop is output at the LF connector.

##### Parameters:

<Start> float

Range: -3 to 3

Increment: 1E-3

*RST: 0

Example: SWE:LFC ON

    activates the output of a linear voltage ramp from sweep start to

    sweep stop at the LF connector.

    SWE:OVOL:STAR 0V

    SWE:OVOL:STOP 3V

    the voltage at the sweep start frequency is 0 V and at the stop

    frequency 3 V.

Manual operation: See "Output Voltage Start Freq - RF Frequency Sweep" on page 186

#### [:SOURCE<hw>]:SWEep[:FREQUency]:OVOLtage:STOP <Stop>

Sets the voltage at the sweep stop frequency. The linear voltage ramp from sweep start to stop is output at the LF connector.

Parameters:
<Stop> float
Range: -3 to 3
Increment: 1E-3
*RST: 3

Example: SWE:LFC ON
activates the output of a linear voltage ramp from sweep start to sweep stop at the LF connector.
SWE:OVOL:STAR 0V
SWE:OVOL:STOP 3V
the voltage at the sweep start frequency is 0 V and at the stop frequency 3 V.

Manual operation: See "Output Voltage Stop - RF Frequency Sweep" on page 187

#### [:SOURCE<hw>]:SWEep[:FREQUency]:POINTs 

Determines the number of steps for the RF frequency sweep within the sweep range.

This parameter always applies to the currently set sweep spacing and correlates with the step size as follows:

- for linear sweeps
    freq_points = (f_SPAN / step_lin) + 1
    To determine the step size, use the command SWE: STEP[:LIN].

- logarithmic sweeps and  $ f_{\text{STARt}} < f_{\text{STOP}} $
freqq_points = ((log f_{\text{STOP}} - log f_{\text{STARt}}) / log step_log) + 1
To determine the logarithmic step size, use the command SWE:STEP:LOG.

If you change the number of sweep points, the step size changes accordingly. The sweep range remains the same.

##### Parameters:


integer
Range: 2..max

Example: FREQ: STAR
sets the start frequency to 100 MHz.
FREQ: STOP
sets the stop frequency to 500 MHz.
SWE: SPAC LIN
sets linear sweep spacing.
SWE: POIN 401
sets 401 sweep steps for linear sweep spacing. The sweep step width (STEP) is automatically set to 1 MHz.

#### [:SOURCE<hw>]:SWEep[:FREQUency]:RETRace <State>

Activates that the signal changes to the start frequency value while it is waiting for the next trigger event.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single".

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: 0

Example: TRIGO: SWE: SOUR SING

FREQ:MODE SWE

SWE:SHAP SAWT

SWE:RETR ON

activates retrace function, i.e. the frequency changes to the value at start frequency while waiting for the next trigger event.

Manual operation: See "Retrace - RF Frequency Sweep" on page 185

#### [:SOURCE<hw>]:SWEep[:FREQUency]:RUNNING?

Queries the current state of the frequency sweep mode.

##### Return values:

<State> 0 | 1 | OFF | ON

Example: SWE:RUNN?

Response "1": signal generation in level sweep active.

Usage: Query only

#### [:SOURCE<hw>]:SWEep[:FREQUency]:SHAPe <Shape>

Sets the cycle mode for a sweep sequence (shape).

Parameters:

<Shape> SAWTooth | TRIangle

One sweep runs from start to stop frequency. Each subsequent sweep starts at the start frequency, i.e. the shape of the sweep sequence resembles a sawtooth.

One sweep runs from start to stop frequency and back, i.e. the shape of the sweep resembles a triangle. Each subsequent sweep starts at the start frequency.

Example: SOUR: SWE: SHAP TRI

selects the sweep cycle with alternating ascending and descending sweep directions.

Manual operation: See "Shape - RF Frequency Sweep" on page 184

#### [:SOURCE<hw>]:SWEep[:FREQUency]:SPACing <Spacing>

Selects the mode for the calculation of the frequency sweep intervals. The frequency increases or decreases by this value at each step.

The keyword [ : FREQuency ] can be omitted. Then the command is SCPI-compliant.

Parameters:

<Spacing>

    LINear | LOGarithmic

    LINear

    With the linear sweep, the step width is a fixed frequency value which is added to the current frequency. The step width for linear sweep is entered in Hz (see [ : SOURce<hw>] : SWEep[ : FREQuency] : STEP[ : LINear] on page 427).

    LOGarithmic

    With the logarithmic sweep, the step width is a constant fraction of the current frequency. This fraction is added to the current frequency. The logarithmic step width is entered in % (see [ : SOURce<hw>] : SWEep[ : FREQuency] : STEP : LOGarithmic on page 428).

    *RST: LINear

    Example: SWE:SPAC LIN

    selects linear sweep spacing for a frequency sweep at the RF output

Manual operation: See "Spacing - Frequency Sweep" on page 184

#### [:SOURCE<hw>]:SWEep[:FREQUency]:STEP[:LINear] <Linear>

Sets the step size for linear RF frequency sweep steps.

This parameter is related to the number of steps ([ : SOURce<hw>]: SWEep[: FREQuency]: POINts) within the sweep range as follows:

f_STARt < f_STOP

freq_points = (f_SPAN / step_lin) + 1

If you change the step size, the number of steps changes accordingly. The sweep range remains the same.

The keywords [ : FREQuency] and [ : LINear] can be omitted. The command is then SCPI-compliant.

Parameters:
<Linear> float
Range: full frequency range
Increment: see the data sheet: RF characteristics > Resolution of setting

Example: FREQ:STAR 1GHz
sets the start frequency to 1 GHz.
FREQ:STOP 5GHz
sets the stop frequency to 5 GHz.
SWE:SPAC LIN
sets linear sweep spacing.
SWE:STEP 2 MHz
sets the step width for linear sweep spacing to 2 MHz (RF sweep) at the RF output. The number of sweep steps for linear sweep spacing (POINTs) is automatically set to 2001.

Manual operation: See "Step Lin/Log - Frequency Sweep" on page 185

##### [:SOURCE<hw>]:SWEep[:FREQUency]:STEP:LOGarithmic <Logarithmic>

Sets a logarithmically determined sweep step size for the RF frequency sweep. It is expressed in percent and you must enter the value and the unit PCT with the command.

The frequency is increased by a logarithmically calculated fraction of the current frequency according to:

step_log $ _{n+1} $ = f $ _{n} $ + step_log $ _{n} $ x f $ _{n} $

 $$ f_{n+1}=f_{n}+step\_{l}og_{n+1} $$ 

with  $ f_{\text{START}} < f_{\text{STOP}} $ and n = number of sweep steps

This parameter correlates with the number of steps SWE: FREQ: POIN within the sweep range as follows:

 $$ \mathrm{f r e q\_{p} o i n t s}=(\left(\log\mathrm{f}_{\mathrm{S T O P}}-\log\mathrm{f}_{\mathrm{S T A R t}}\right)/\log\mathrm{s t e p\_{l} o g})+1 $$ 

If you change the step size, the number of steps changes accordingly. The sweep range remains the same.

Parameters:
<Logarithmic> float
Range: 0.01 to 100
Increment: 1E-3
*RST: 1
Example: FREQ:STAR 1GHz
sets the start frequency to 1 GHz.
FREQ:STOP 5GHz
sets the stop frequency to 5 GHz.
SWE:SPAC LOG
sets logarithmic sweep spacing.
SWE:STEP:LOG 10PCT
sets the step width for logarithmic sweep spacing to 10% of the previous frequency in each instance (for a frequency sweep).
Manual operation: See "Step Lin/Log - Frequency Sweep" on page 185

##### [:SOURCE<hw>]:SWEep:POWER:DWELI <Dwell>

Sets the time taken for each level step of the sweep.

Tip: It is recommended to switch off the "Display Update" for optimum sweep performance especially with short dwell times (SYSTEM:DISPLAY:UPDATE OFF).

##### Parameters:

<Dwell> float
Range: 1E-3 to 100
Increment: 100E-6
*RST: 15E-3

Example: SWE: POW: DWEL 12 ms

sets a dwell time of 12 ms for a level sweep at the RF output.

Manual operation: See "Dwell Time - Level Sweep" on page 192

##### [:SOURCE<hw>]:SWEep:POWER:EXECute

Triggers a sweep.

The command is only valid for sweep mode Single (SOURCE: SWEep: POWER: MODE SINGLE). The command corresponds to the manual-control command "Execute Single Sweep".

Example: SOURce:SWEep:POWER:MODE SINGle sets the single cycle mode of the level sweep. SWE:POW:EXEC triggers a level sweep at the RF output.

Usage: Event

Manual operation: See "Execute Single Sweep - Level Sweep" on page 190

[:SOURCE<hw>]:SWEep:POWER:MODE <Mode>

Sets the cycle mode of the level sweep.

Parameters:

<Mode> AUTO | MANual | STEP

AUTO

Each trigger triggers exactly one complete sweep.

MANual

The trigger system is not active. Each level step of the sweep is triggered individually, either by varying the "Current Level" value using the rotary knob under manual control or by means of a POW:MAN command under remote control.

With manual control, the level increases or decreases (depending on the direction of the rotary encoder) by the value specified under SOUR:SWE:POW:STEP. With remote control, the level increases by the value specified under SWEep:POW:STEP which each sent:POW:MAN command, irrespective the value entered there.

STEP

Each trigger triggers one sweep step only. The level increases

Each trigger triggers one sweep step only. The level increases by the value entered under :SWEep:POWER:STEP.

*RST: AUTO

Example: SWE: POW: MODE AUTO

selects Mode Auto for a level sweep at RF output.

Manual operation: See "Mode - Level Sweep" on page 188

##### [:SOURCE<hw>]:SWEep:POWER:POINTs 

Determines the number of steps for the RF level sweep within the sweep range.

This parameter always applies to the currently set sweep spacing and correlates with the step size as follows:

pow_points = (f_{STOP} - f_{START} / step_log) + 1

To determine the step size use the command SWE:POW:STEP[:LOG].

If you change the number of sweep points, the step size changes accordingly. The sweep range remains the same.

##### Parameters:


integer

Range: 2...max

Example: POW:STAR - 30 dBm
sets the start frequency to -30 dBm.
POW:STOP - 10 dBm
sets the stop frequency to -10 dBm.
SWE:POW:POIN 20
sets 20 sweep steps. The sweep step width (STEP) is automatically set to 1 dB.

#### [:SOURCE<hw>]:SWEep:POWER:RETRace <State>

Activates that the signal changes to the start level value while it is waiting for the next trigger event.

You can enable this feature, when you are working with sawtooth shapes in sweep mode "Single" or "External Single".

Parameters:
<State> 0 | 1 | OFF | ON
*RST: 0
Example:
TRIG0:SWE:SOUR SING
POW:MODE SWE
SWE:POW:SHAP SAWT
SWE:POW:RETR ON
activates retrace function, i.e. the level changes to the value at start level while waiting for the next trigger event.

Manual operation: See "Retrace - RF Level Sweep" on page 191

#### [:SOURCE<hw>]:SWEep:POWER:RUNNING?

Queries the current state of the level sweep mode.

Return values:
<State>
0 | 1 | OFF | ON
Example:
SWE: POW: RUNN?
Response "1": signal generation in level sweep active.
Usage:
Query only

#### [:SOURCE<hw>]:SWEep:POWER:SHAPe <Shape>

Sets the cycle mode for a sweep sequence (shape).

Parameters:
<Shape> SAWTooth | TRangle
SAWTooth
One sweep runs from the start level to the stop level. The subsequent sweep starts at the start level again, i.e. the shape of sweep sequence resembles a sawtooth.

##### TRIangle

One sweep runs from start to stop level and back, i.e. the shape of the sweep resembles a triangle. Each subsequent sweep starts at the start level again.

Example: SOUR: SWE: POW: SHAP TRI

selects the sweep cycle with alternating ascending and descending sweep directions.

Manual operation: See "Shape - RF Level Sweep" on page 191

#### [:SOURCE<hw>]:SWEep:POWER:SPACing:MODE?

Queries the sweep spacing mode. The sweep spacing for level sweeps is always linear.

##### Return values:

Return values:

<Mode>

    LINear

    *RST:

    LINear

Example: SWE: POW: SPAC: MODE?

queries the sweep spacing for a level sweep at RF output.

Result: LIN

linear spacing

Usage: Query only

#### [:SOURCE<hw>]:SWEep:POWER:STEP[:LOGarithmic] <Logarithmic>

Sets a logarithmically determined sweep step size for the RF level sweep. It is expressed in decibels and you must enter the value and the unit dB with the command.

The level is increased by a logarithmically calculated fraction of the current level according to:

step_size $ _{n+1} $ = Level $ _{n} $ + step_size $ _{n} $ x Level $ _{n} $

 $$  Level_{n+1}=Level_{n}+step\_{s}ize_{n+1} $$ 

with  $ Level_{START} < level_{STOP} $, step_size = SWE:POW:STEP[:LOG] and n = number of sweep steps

This parameter correlates with the number of steps SWE: POW: POIN within the sweep range as follows:

 $$  level_{points}=((Level_{STOP}-Level_{STARt})/step_{size})+1) $$ 

If you change the step size, the number of steps changes accordingly. The sweep range remains the same.

Parameters:

<Logarithmic> float

Increment: 0.01

*RST: 1

Example: SWE:POW:STEP 10dB

sets the step width for logarithmic sweep spacing to 10 dB of the previous level in each instance (for a level sweep).

Manual operation: See "Step - Level Sweep" on page 192

[:SOURCE<hw>]:SWEep:RESET[:ALL]

Resets all active sweeps to the starting point.

Example: SWE:RES

resets all active sweeps to the starting point.

Usage: Event

Manual operation: See "Reset Sweep - Frequency Sweep" on page 183
