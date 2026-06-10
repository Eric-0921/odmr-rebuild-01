### 6.14 STATus subsystem

This system contains the commands for the status reporting system. See also Chapter 5.5, "Status reporting system", on page 273 for detailed information.

*RST on page 286 has no effect on the status registers.

Value ranges

- Queries return the current value of the respective register, which permits a check of the device status.

Return values: A decimal value in the range 0 to 32767 ( $ =2^{15}-1 $)

- The configuration commands set the respective register thus determining which status changes of the R&S SMB cause the status registers to be changed. Setting values: A decimal value in the range 0 to 32767 (=2¹⁵-1)

:STATus:OPERATION:CONDITION?.....434

:STATUS:OPERATION:ENABLE.....434

:STATUS:OPERATION[:EVENT].....434

:STATUS:OPERATION:NTRansition.....434

:STATUS:OPERATION:PTRansition.....435

:STATUS:PRESet.....435

:STATUS:QUESTionable:CONDITION.....435

:STATUS:QUESTionable:ENABLE.....435

:STATUS:QUESTionable[:EVENT].....436

:STATUS:QUESTionable:NTRansition.....436

:STATUS:QUESTionable:PTRansition.....436

:STATUS:QUEue[:NEXT]?.....436

##### :STATus:OPERATION:CONDITION?

Quieries the content of the CONDITION part of the STATus: OPERation register.

This part contains information on the action currently being performed in the instrument. The content is not deleted after being read out because it indicates the current hardware status.

##### Return values:

<Condition> string

Example: :STATUS:OPERATION:CONDITION?

Usage: Query only

##### :STATUS:OPERATION:ENABLE <Enable>

Sets the bits of the ENABLE part of the STATus:OPERATION register. This setting determines which events of the Status-Event part are forwarded to the sum bit in the status byte. These events can be used for a service request.

##### Parameters:

<Enable> string

Example: :STAT:OPER:ENAB 32767

all events are forwarded to the sum bit of the status byte.

##### :STATUS:OPERATION[:EVENT] <Event>

Queries the content of the EVENT part of the STATus:OPERATION register. This part contains information on the actions performed in the instrument since the last readout. The content of the EVENT part is deleted after being read out.

##### Parameters:

<Event> string

Example: : STAT:OPER:EVEN?

queries the STATUS:OPERATION:EVENT register.

##### :STATus:OPERATION:NTRansition <Ntransition>

Sets the bits of the NTRansition part of the STATus:OPERATION register. If a bit is set, a transition from 1 to 0 in the condition part causes an entry to be made in the EVENT part of the register. The disappearance of an event in the hardware is thus registered, for example the end of an adjustment.

##### Parameters:

<Ntransition> string

Example: :STAT:OPER:NTR 0

a transition from 1 to 0 in the condition part of the Status:Operation register does not cause an entry to be made in the EVENT part.

##### :STATus:OPERATION:PTRansition <Ptransition>

Sets the bits of the PTRansition part of the STATus:OPERATION register. If a bit is set, a transition from 0 to 1 in the condition part causes an entry to be made in the EVENT part of the register. A new event in the hardware is thus registered, for example the start of an adjustment.

##### Parameters:

<Ptransition> string

Example: :STAT:OPER:PTR 32767

all transitions from 0 to 1 in the condition part of the Status:Operation register cause an entry to be made in the EVENT part.

##### :STATus:PRESet <Preset>

Resets the status registers. All PTRansition parts are set to FFFFh (32767), i.e. all transitions from 0 to 1 are detected. All NTRansition parts are set to 0, i.e. a transition from 1 to 0 in a CONDITION bit is not detected. The ENABLE parts of STATus:OPERATION and STATus:QUESTionable are set to 0, i.e. all events in these registers are not passed on.

##### Parameters:

<Preset> string

Example: STAT:PRES

resets the status registers.

##### :STATus:QUESTionable:CONDITION <Condition>

Queries the content of the CONDITION part of the STATus:QUESTionable register. This part contains information on the action currently being performed in the instrument. The content is not deleted after being read out since it indicates the current hardware status.

##### Parameters:

<Condition> string

Example: :STATUS:QUESTIONABLE:CONDITION? queries the Status:QUESTIONABLE:CONDITION register.

##### :STATus:QUESTionable:ENABLE <Enable>

Sets the bits of the ENABLE part of the STATus:QUESTionable register. The enable part determines which events of the STATus:EVENT part are enabled for the summary bit in the status byte. These events can be used for a service request.

If a bit in the ENABLE part is 1, and the corresponding EVENT bit is true, a positive transition occurs in the summary bit. This transition is reported to the next higher level.

##### Parameters:

<Enable> string

Example: STAT:QUES:ENAB 1

Problems when performing an adjustment cause an entry to be made in the sum bit.

#### :STATus:QUESTionable[:EVENT] <Event>

Queries the content of the EVENT part of the STATUS:QUESTionable register. This part contains information on the actions performed in the instrument since the last readout. The content of the EVENT part is deleted after being read out.

##### Parameters:

<Event> string

Example: STAT:QUES:EVEN? queries the Status:Questionable:Event register.

##### :STATus:QUESTionable:NTRansition <Ntransition>

Sets the bits of the NTRansition part of the STATus:QUESTionable register. If a bit is set, a transition from 1 to 0 in the condition part causes an entry to be made in the EVENT part of the register.

##### Parameters:

<Ntransition> string

Example: STAT:QUES:NTR 0

a transition from 1 to 0 in the condition part of the STA-Tus:QUESTionable register does not cause an entry to be made in the EVENT part

#### :STATus:QUESTionable:PTRansition <PTransition>

Sets the bits of the NTRansition part of the STATus:QUESTionable register. If a bit is set, a transition from 1 to 0 in the condition part causes an entry to be made in the EVENT part of the register.

##### Parameters:

<PTransition> string

Example: STAT:QUES:PTR 32767

all transitions from 0 to 1 in the condition part of the STA-Tus:QUEStionable register cause an entry to be made in the EVENT part

##### :STATus:QUEue[:NEXT]?

Queries the oldest entry in the error queue and then deletes it. Positive error numbers denote device-specific errors, and negative error numbers denote error messages defined by SCPI. If the error queue is empty, 0 ("No error") is returned.

The command is identical to :SYSTEM:ERROR[:NEXT] ? on page 440.

Return values:

<Next> string

Example: :STATUS:QUEue?

queries the oldest entry in the error queue.

Response: 0, 'no error'

no errors have occurred since the error que

Usage: Query only

Manual operation: See "History" on page 74

