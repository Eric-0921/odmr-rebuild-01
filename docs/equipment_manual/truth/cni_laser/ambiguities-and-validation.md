# CNI Laser Ambiguities And Validation

## Locked first-version decisions

- protocol truth comes from the RS232 frame sheet and the device-specific report
- the device is treated as write-only
- the first rebuild does not invent a readback API

## Known ambiguity buckets

### Power limit source

- protocol page shows how to encode power
- generic user material can talk loosely about power/current behavior
- device-specific report shows this unit was tested around `300 mW`

Decision:

- actual max power belongs in station/profile configuration
- do not hardwire one guessed universal ceiling into the protocol truth

### ACK / echo behavior

The protocol sheet does not define a formal query/response or ACK frame.

Decision:

- first version assumes no reliable ACK
- any observed echo remains transport behavior, not device truth

### "Power" vs "current" wording

Some material uses wording that can tempt implementers to expose a current-setting API.

Decision:

- first version exposes only power-oriented helper names

### Modulation assumptions

The retained report says `调制: No`.

Decision:

- modulation is out of scope

## Validation still worth doing later

- capture one clean power-set / on / off byte transcript
- verify whether the serial path echoes bytes on this exact controller/cable path
- confirm the station-level maximum power chosen for this lab unit

