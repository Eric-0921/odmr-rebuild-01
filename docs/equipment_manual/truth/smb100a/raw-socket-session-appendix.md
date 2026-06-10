# SMB100A Raw Socket Session Appendix

This appendix is guidance for the first `SMB100A` transport implementation. It is not a cross-device abstraction guide.

## Why raw socket is the default

- the manual explicitly documents raw socket / socket communication
- port `5025` is the typical remote-control port
- VISA installation is optional for this path
- this keeps the first rebuild lighter and easier to audit

## Resource examples

- raw socket shape: `TCPIP::192.1.2.3::5025::SOCKET`
- direct socket runtime shape: `<host>:5025`

The rebuild runtime only needs the second shape internally.

## Session rules

- open one TCP connection per instrument session
- send ASCII SCPI lines
- terminate each command with `LF`
- read one response for each query before sending the next query
- avoid opportunistic command batching in first version

## Synchronization rules

- use `*OPC?` only for commands that can overlap internally
- do not replace every wait with `*OPC?`; use it intentionally
- do not issue `*CLS` immediately after an overlapped command sequence

## Minimal logging expectations

Each transport layer log entry should be able to answer:

- what exact command was sent
- whether it was a write or a query
- what raw response was received
- how long the round-trip took

This is one of the main reasons not to hide the protocol behind heavy abstraction.

## When to use RS-VISA anyway

Use `RS-VISA` or `RsInstrument` only when you need:

- comparison against a raw-socket anomaly
- a second implementation to confirm session behavior
- a quick one-off lab diagnostic
- guidance for logging / OPC / session-sharing patterns

Do not make first-version runtime correctness depend on VISA availability.

## Things not to cargo-cult from RsInstrument

- do not copy its full API surface
- do not infer that every device in the rebuild wants a VISA-like abstraction
- do not project SMB100A SCPI patterns onto `OE1022D` or `CNI Laser`

