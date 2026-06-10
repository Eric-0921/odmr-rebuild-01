# Truth Docs Index

This directory is the first implementation-facing document layer for the rebuild.

## Status

- scope: first-version command truth only
- audience: command-helper authors, transport-shell authors, runtime authors
- non-goal: full manual rewrite
- non-goal: direct code generation

## Directory layout

- `smb100a/`
  - `command-truth.md`
  - `operational-notes.md`
  - `ambiguities-and-validation.md`
  - `raw-socket-session-appendix.md`
- `m8812/`
  - `command-truth.md`
  - `operational-notes.md`
  - `ambiguities-and-validation.md`
- `oe1022d/`
  - `command-truth.md`
  - `operational-notes.md`
  - `ambiguities-and-validation.md`
- `cni_laser/`
  - `command-truth.md`
  - `operational-notes.md`
  - `ambiguities-and-validation.md`

## Global trust rules

1. `truth/` is the only implementation input layer.
2. `original/` PDF material outranks cleaned markdown when the two differ.
3. Split markdown under `smb100a/` and `oe1022d/` is mainly a navigation / grep layer.
4. Reverse-engineered material can explain observed behavior, but it does not become protocol truth automatically.
5. If a command is missing from `command-truth.md`, it is out of scope for first-version implementation.

## Device-specific high-risk reminders

- `SMB100A`
  - main transport is `Raw Socket` on port `5025`
  - `RS-VISA` is optional, not required
- `M8812`
  - first-version hard current ceiling is `2 A`
  - do not promote reverse-only `5 A` behavior
- `OE1022D`
  - `RALL?` is the canonical acquisition path
  - `RALL` field map contains confirmed fields, inferred fields, and trailing holes; do not collapse them into one guessed struct
- `CNI Laser`
  - no reliable readback / ACK is documented
  - first version is write-only and low-observability by design

