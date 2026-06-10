# Equipment Manual Index

This rebuild branch keeps only the material needed for the first pure-code / CLI-first rebuild.

## Canonical truth docs

The only documents intended to drive the next implementation phase live under:

- `truth/README.md`
- `truth/smb100a/`
- `truth/m8812/`
- `truth/oe1022d/`
- `truth/cni_laser/`

These files define:

- the first-version command truth
- source-priority / trust rules
- known ambiguities
- operational notes worth keeping

## Frozen legacy source pool

The following directories are retained as source material only. They are frozen legacy references, not implementation truth by themselves:

- `original/`
- `smb100a/`
- `oe1022d/`
- `maynuo_dc-power-supply/`
- `CNI Laser psu-sr/`
- `../reverse_application/reverse_output/`

Rules:

- do not treat OCR split markdown as independent truth
- do not treat reverse behavior as protocol truth unless explicitly promoted in `truth/`
- do not copy command strings from frozen docs into code without first passing through `truth/`

## Current rebuild decisions

- command helpers must carry the device/model prefix, for example:
  - `smb100a_set_output`
  - `m8812_set_current`
  - `oe1022d_rall_query`
  - `cni_laser_power_set`
- helper naming should stay close to manual terminology
- first-version `SMB100A` transport is `Raw Socket`
- `RS-VISA` is optional diagnostic / comparison tooling, not the main runtime dependency
- `OE1022D` and `CNI Laser` are not forced into a generic SCPI abstraction

