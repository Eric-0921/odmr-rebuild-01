# Odmr.WinProbe

Minimal Windows C# probe for the first VISA rebuild gate.

This tool intentionally keeps the OE1022D `RALL?` hot path small:

```text
write RALL?
sleep 30ms
blocking read 12288 bytes
append raw frame and frame index
```

It does not implement station resolving, GUI bridging, RALL parsing, retries,
frame deadlines, or SMB100A control.

## Commands

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min
```

`oe-rall` writes:

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `summary.json`
