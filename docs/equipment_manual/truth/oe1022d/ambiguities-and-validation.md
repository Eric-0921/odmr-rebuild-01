# OE1022D Ambiguities And Validation

## Locked first-version decisions

- original PDF outranks cleaned markdown
- `RALL?` is the canonical acquisition command
- the first implementation must preserve explicit offset-based parsing

## Known ambiguity buckets

### `oe1022d_clean_operation.md` is not pristine source

The cleaned markdown is useful for grep and long-form reading, but it already mixes in secondary interpretation and revision drift.

Decision:

- do not treat it as the top truth source

### `RALL` frame interpretation

The manual documents:

- frame length
- chunk order
- byte ranges
- update cadence

But it does not fully settle every semantic detail needed for a production parser.

Decision:

- split the future parser model into:
  - confirmed fields
  - inferred fields
  - reserved / trailing holes

### `RSLPD` and external-reference trigger semantics

The original `OE1022D用户使用文档V1.5.pdf` and real hardware observations now diverge in a concrete, reproducible way.

Decision:

- keep `RSLPD` out of first-version command truth until implementation explicitly models:
  - `manual-stated` values
  - `hardware-observed` values

Observed facts as of `2026-06-11`:

- PDF page 28 (`4.2.3 <Ref.Slope>`) shows only two UI categories:
  - `TTL`
  - `Sine`
- PDF page 57 (`5.2.1 RSLPD`) states only:
  - `j=0` => TTL rising-edge trigger
  - `j=1` => sine zero-crossing trigger
- PDF page 91 (`External Ref Trigger`) shows only:
  - `TTL Rising Edge`
  - `Sine Zero Crossing`
- real hardware readback after vendor LabVIEW configured and locked PLL on channel B:
  - `FMODD? 2 = 0`
  - `RSLPD? 2 = 2`
  - `FREQD? 2 = 4.99999e+02`
  - `*PLLD? 2 = 1`

Artifact source:

- `out/hardware_state_snapshot/manual_after_labview/hardware_state_snapshot.json`

Implication:

- the original PDF is not sufficient to describe the effective `RSLPD` enum space for this device / firmware / vendor-software path
- some local markdown files also introduced extra values and names that are not supported by the original PDF pages

### Unit and enum drift in secondary docs

Secondary markdown contains places where units or value interpretations drift.

Decision:

- trust original PDF chapter 4 and chapter 5 first

## Validation still worth doing later

- one clean parser-offset audit against real `RALL` frames
- one command transcript proving chosen fixed config values
- one explicit record of which sensitivity and time-constant indices are used in the first runtime profile
- one controlled write-back test for `RSLPD 2,2` after operator approval, to determine whether:
  - `2` is a writeable remote enum
  - `2` is a software-side alias that maps to another instrument-side mode
