### 6.6 DISPlay Subsystem

The Display subsystem contains the commands to set the power-save mode of the instrument.

:DISPLAY:ANNotation:AMPLitude ..... 295    
:DISPLAY:ANNotation:FREQUency ..... 295    
:DISPLAY:ANNotation[:ALL] ..... 295    
:DISPLAY:DIALog:CLOSE ..... 295    
:DISPLAY:DIALog:CLOSE:ALL ..... 296    
:DISPLAY:DIALog:ID? ..... 296    
:DISPLAY:DIALog:OPEN ..... 296    
:DISPLAY:PSAVE:HOLDoff ..... 296    
:DISPLAY:PSAVE[:STATE] ..... 296    
:DISPLAY:UPDATE ..... 297

#### :DISPLAY:ANNotation:AMPLitude <State>

Indicates asterisks instead of the level values in the status bar.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 1)

Example: DISP:ANN:AMPL ON Suppresses the level display.

Manual operation: See "Annotation Amplitude" on page 119

#### :DISPLAY:ANNotation:FREQUency <State>

Indicates asterisks instead of the frequency values in the status bar.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 1)

Example: DISP:ANN:FREQ ON Supresses the frequency display.

Manual operation: See "Annotation Frequency" on page 119

#### :DISPLAY:ANNotation[:ALL] <State>

Displays asterisks instead of the level and frequency values in the status bar of the instrument. This setting is useful when you remotely control the instrument.

##### Parameters:

<State> 0 | 1 | OFF | ON

*RST: 1

Example: DISP:ANN:AMPL ON

Shows asterisks instead of frequency and level values.

#### :DISPLAY:DIALOG:CLOSE <DialogId>

Closes the specified dialog. To determine the dialog identifier, use command :

DISPLAY:DIALOG:ID?.

##### Setting parameters:

Setting parameters:

<DialogId> string

Example: DISP:DIAL:CLOS "<dialog ID>"

    Closes the dialog, determined with the "<dialog ID>".

Usage: Setting only

#### :DISPLAY:DIALog:CLOSE:ALL

Closes all open dialogs.

Example: DISP:DIAL:CLOS:ALL

Usage: Event

#### :DISPLAY:DIALog:ID?

Returns the dialog identifiers of the open dialogs in a string separated by blanks.

##### Return values:

<DialogIdList> string

Example: DISP:DIAL:ID?

Response: "<dialog ID(1)> <dialog ID(2)> ... <dialog ID(n)>".

Returns the dialog identifiers of all opened dialogs.

Usage: Query only

#### :DISPLAY:DIALOG:OPEN <DialogId>

Opens the specified dialog. To determine the dialog identifier, use command :

DISPLAY:DIALog:ID?.

##### Setting parameters:

<DialogId> string

Example: DISP:DIAL:OPEN "<dialog ID>"

    Opens the dialog, determined with the "<dialog ID>".

Usage: Setting only

#### :DISPLAY:PSAVE:HOLDoff <HoldoffTimeMin>

Sets the waiting time for the screen-save mode of the display.

##### Parameters:

<HoldoffTimeMin> integer

Range: 1 to 60

*RST: n.a. (factory preset: 10)

Default unit: minute

Example: DISP:PSAV:HOLD 8

Sets the timeout of the screen saver to 8 minutes.

Manual operation: See "Wait Time" on page 108

#### :DISPLAY:PSAVE[:STATE] <State>

Activates the screen-save mode of the display.

If activated, the display including backlight is switched off after the wait time elapses and if no entries via front panel, external mouse or external keyboard are made. To set the wait time, use command :DISPLAY:PSAVE:HOLDoff.

This mode is recommended for protecting the display, especially if you operate the instrument via remote control.

Parameters:

<State> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 0)

Example: DISP:PSAV ON Activates screen saver mode.

Manual operation: See "Screen Saver Active" on page 108

:DISPLAY:UPDATE <Update>

Activates the refresh mode of the display.

Parameters:

<Update> 0 | 1 | OFF | ON

*RST: n.a. (factory preset: 1)

Example: DISP:UPD ON

Activates automatic update of the display at defined time intervals.

