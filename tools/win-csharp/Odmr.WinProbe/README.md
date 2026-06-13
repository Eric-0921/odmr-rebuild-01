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
frame deadlines, or SMB100A control beyond the read-only TCP probe.

## Commands

```powershell
dotnet run --project tools/win-csharp/Odmr.WinProbe -- visa-list
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-idn --resource ASRL8::INSTR --baud 921600
dotnet run --project tools/win-csharp/Odmr.WinProbe -- oe-rall --resource ASRL8::INSTR --baud 921600 --duration-sec 300 --out-dir runs/win_csharp_oe_rall_5min
dotnet run --project tools/win-csharp/Odmr.WinProbe -- smb-probe --host 169.254.2.20 --port 5025
```

`oe-rall` writes:

- `raw/oe1022d.rall`
- `raw/oe1022d.frames.idx.jsonl`
- `segments.jsonl`
- `summary.json`

`smb-probe` keeps the current Ethernet raw socket path and only sends:

- `*IDN?`
- `SYST:ERR?`
- `OUTP?`
