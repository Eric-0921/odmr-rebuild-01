# ODMR Plan JSON Generator

Standalone browser tool for generating C# runtime-compatible `AcquisitionRunPlan`
JSON. It writes `points[]` plans only and does not introduce a new runtime schema.

For normal experiment setup, use `tools/config-generator/odmr_config_generator.py`
instead. The Python generator covers magnetic plan, SMB100A sweep profile,
OE1022D fixed profile, and CNI laser profile. This browser tool is plan-only.

Open `index.html` directly in a browser, or serve the directory with any static
file server.

## Checks

```bash
node tools/plan-json-generator/tests/plan-core.test.mjs
```

The fixture plans under `tests/fixtures/` are intentionally small and can be
passed to `Odmr.WinProbe run-resolve` with the normal station/profile JSONs.
