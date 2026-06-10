### 6.3 Preset Commands

The preset commands are not bundled in one subsystem. Therefore, they are listed separately in this section. In addition, a specific preset command is provided for each digital standard and for the fader. These specific commands are described in the associated subsystems.

Four presetting actions are available:

- Activating the default state of all internal instrument functions (`*RST` on page 286). Functions that concern the integration of the instrument into a measurement setup are not changed, e.g. GPIB address or reference oscillator source settings.

- Activating the preset state of the parameters related to the selected signal path (`SOURCE<hw>:PRESet` on page 289)

- Activating the preset state of all parameters that are not related to the signal path (`:DEVICE:PRESet` on page 288)

- Activating the original state of delivery (factory reset, `:SYSTEM:FPReset` on page 289). Only functions that are protected by a password remain unchanged as well as the passwords themselves.

##### :DEVICE:PRESet

Presets all parameters which are not related to the signal path, including the LF generator.

| Example: | Presets all instrument settings that are not related to the signal path |
| Usage:   | Event |

#### :SOURCE<hw>:PRESet

Presets all parameters which are related to the selected signal path.

The following functions are only preset by command `*RST`: Fading, transient recorder.

Example: `SOUR:PRES`

Presets all settings that are related to signal path

Usage: Event

##### :SYSTEM:PRESet

Triggers an instrument reset. It has the same effect as:

- The [Preset] key.

However, the command does not close open GUI dialogs like the key does.

- The `*RST` command

For an overview of the settings affected by the preset function, see Chapter 4.2.2, "Default Instrument Settings - Preset Key", on page 96.

Example: `SYST:PRES`

All instrument settings (also the settings that are not currently active) are reset to their default values.

Usage: Setting only

##### :SYSTEM:FPReset

Triggers an instrument reset to the original state of delivery.

Note: "Factory Preset" resets the "Remote Channel" and network settings to the default values.

Executing "Factory Preset" via remote control terminates the connection to the instrument, if these settings had been configured to values different to the default ones.

The factory preset function resets nearly all instrument settings. In addition to the regular preset by means of the [PRESET] key, a "Factory Preset" resets also the following values:

- Reference frequency settings ("Ref Oscillator" menu)

- Power on settings ("Level/EMF" menu)

- Network settings including hostname ("Setup" menu)

- Remote channel settings including GPIB address ("Setup" menu)

- Start/Stop display update ("Setup" menu)

- Display and keyboard settings ("Setup" menu).

To maintain security, password settings and all settings protected by these passwords like disabled USB and LAN connections are not changed.

Not affected by the "Factory Preset" are also user data, lists or instrument settings files, created for example by means of the Save/Recall function.

| Example:           | `SYST:FPR` |
|                    | All instrument settings (also the settings that are not currently active) are reset to the factory values. |
| Usage:             | Event |
| Manual operation:  | See "Factory Preset" on page 122 |
