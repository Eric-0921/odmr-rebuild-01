# SMB100A Ambiguities And Validation

## Accepted first-version decisions

- main transport is `Raw Socket`
- `RS-VISA` is optional diagnostic tooling
- first-version command scope is limited to:
  - identity
  - error queue
  - output on/off
  - CW frequency / power
  - RF frequency sweep core parameters

## Known ambiguity buckets

### OCR corruption in split markdown

Observed examples include malformed tokens in indexes and examples. Typical risk patterns:

- `DWELI`
- `TRIGO`
- `OMODe`
- `SONCe`

Policy:

- never promote OCR-only tokens into `command-truth.md`
- prefer正文命令标题 over contents pages, examples, and OCR-generated lists

### `SYST:ERR?` vs richer error queries

The manual exposes richer queue access such as `SYST:ERR:ALL?`, but the first rebuild keeps only `SYST:ERR?` in scope.

Reason:

- enough for minimal preflight and post-action checks
- avoids premature API growth

### `*CLS` usage

The manual defines `*CLS`, but first-version runtime must not treat it as default startup hygiene.

Reason:

- it can erase diagnostic evidence
- existing lab practice already treats it as a controlled diagnostic action

### Query readback semantics

Commands like `FREQ?` and `POW?` can be influenced by broader instrument context.

Policy:

- use them as readback
- do not use them as a substitute for an explicit plan model

## Validation to keep for later

- verify exact line termination behavior over the chosen raw-socket client implementation
- verify whether all targeted commands behave identically over raw socket and VISA
- confirm response normalization rules for `OUTP?`
- keep one small socket-only smoke transcript in a later implementation phase

