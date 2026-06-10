# Maynuo M8812 Remote Control Reference

> Extracted from `m88_manual_cleaned.md` Chapter 5 (Remote Operation Mode) and Chapter 6 (SCPI Communication Protocol).
> Original manual covers M8811/M8812/M8813/M8851/M8852/M8853/M8871/M8872/M8873/M8874.
> **Our lab equipment is M8812** (0-75V, 0-2A).

---

## 1. Communication Interface

### 1.1 Physical Connection

- **Rear-panel connector**: DB9 (TTL voltage level)
- **Required cable**: M-131 voltage level shift cable (TTL → RS232)
- **Warning**: Standard RS232 cable will **not** work. The DB9 output is TTL level, not RS232 level.

### 1.2 Serial Parameters

| Parameter | Value | Notes |
|-----------|-------|-------|
| Baud rate | 9600 | Front-panel selectable: 4800 / 9600 / 19200 / 38400 |
| Data bits | 8 | — |
| Stop bits | 1 | — |
| Parity | none | Front-panel selectable: none / even / odd |
| Flow control | none | — |
| EOI terminator | `<LF>` (0x0A) | Every command must end with line feed |

### 1.3 Case Sensitivity

SCPI commands are **case-insensitive**. Both `SYST:REM` and `syst:rem` are valid.

---

## 2. Basic Commands (IEEE-488.2)

### `*IDN?` — Identity Query

Reads manufacturer, model, serial number, and firmware version.

**Response format**: `MAYNUO,M8812,<SN>,<FW>`

**Example**:
```
→ *IDN?
← MAYNUO,M8812,881201096006118000,V1.0
```

---

## 3. System Commands

### `SYST:REM` — Enter Remote Mode

- **Response**: None
- **Note**: "REM" appears on front-panel VFD when successful. Required before any set command.

### `SYST:LOC` — Return to Local Mode

- **Response**: None
- **Note**: "REM" disappears from VFD. Send **last** after all operations complete.

### `SYST:ERR?` — Read Error Queue

- **Response**: `"<code>, '<message>"`
- **Common codes**:
  - `0, 'No Error'`
  - `50, 'Error Para Count'`
  - `70, 'Invalid Command'`

### `SYST:SENS <bool>` — Remote Sense Control

- **Parameter**: `0` = OFF, `1` = ON
- **Response**: None
- **Note**: Enables/disables remote voltage sensing (four-wire measurement).

---

## 4. Measurement Commands

### `MEAS:CURR?` — Read Output Current

- **Response**: Numeric (unit: A)
- **Example**: `← 0.01000` means 10.00 mA

### `MEAS:VOLT?` — Read Output Voltage

- **Response**: Numeric (unit: V)
- **Example**: `← 12.560` means 12.560 V

### `MEAS:DVM?` — Read Voltmeter Input

- **Response**: Numeric (unit: V)
- **Note**: Uses the built-in 5½-digit voltmeter, independent of output terminals.

---

## 5. Setting Commands

### `VOLT <Value>` — Set Voltage

- **Parameter**: numeric | `MAX` | `MIN`
- **Unit**: V
- **Response**: None
- **M8812 range**: 0 – 75 V

### `VOLT:PROT <Value>` — Set Over-Voltage Protection

- **Parameter**: numeric | `MAX` | `MIN`
- **Unit**: V
- **Response**: None
- **M8812 max**: 76 V (hardware ceiling)

### `CURR <Value>` — Set Current

- **Parameter**: numeric | `MAX` | `MIN`
- **Unit**: A
- **Response**: None
- **M8812 range**: 0 – 2 A

### `OUTP <bool>` — Output On/Off

- **Parameter**: `0` = OFF, `1` = ON
- **Response**: None

### `MODE <mode>` — Operating Mode

- **Parameters**:
  - `FIX` — Command set mode (normal SCPI operation)
  - `LIST` — Sequential list mode (see §6)
  - `DRM` — Milliohmmeter mode
- **Response**: None

---

## 6. LIST Mode (Sequential List Operation)

> **Discovery note**: M8812 supports pre-programming up to **200 steps** into device memory, then executing them with minimal communication overhead. This is a potential future optimization for dense magnetic field scans.
> **Current stance**: Not implemented in M5B-B. See `docs/lab-bringup/maynuo_m8812_lab_notes.md` §7 for discussion.

### 6.1 Memory Organization

| `LIST:AREA` | Files | Steps per file |
|-------------|-------|----------------|
| 1 | 1 | 200 |
| 2 | 2 | 100 |
| 4 | 4 | 50 |
| 8 | 8 | 25 |

### 6.2 List Programming Commands

```
LIST:AREA <1|2|4|8>      # Select memory division
LIST:COUN <1~200>        # Set total step count
LIST:MODE <CONT|STEP|LOOP>  # Execution mode
LIST:VOLT <step>,<value> # Set voltage for step N
LIST:CURR <step>,<value> # Set current for step N
LIST:WIDTH <step>,<ms>   # Set dwell time for step N
LIST:RCL <1~8>           # Recall saved file
```

### 6.3 List Execution Commands

```
TRIG:SOUR <IMM|EXT|BUS>  # Trigger source
TRIG                     # Fire trigger (BUS mode)
```

**Execution modes**:
- `CONT` — Continuous: steps execute back-to-back automatically
- `STEP` — Single-step: one trigger advances one step
- `LOOP` — Loop: repeat entire sequence

### 6.4 Verification Commands

```
LIST:VOLT? <step>        # Query programmed voltage
LIST:CURR? <step>        # Query programmed current
LIST:WIDTH? <step>       # Query programmed dwell
LIST:AREA?               # Query area setting
LIST:COUN?               # Query step count
LIST:MODE?               # Query execution mode
```

---

## 7. M8812-Specific Specifications

| Spec | M8812 Value |
|------|-------------|
| Voltage range | 0 – 75 V |
| Current range | **0 – 2 A** |
| Voltage setting resolution | 1 mV |
| Current setting resolution | 0.05 mA |
| Current readback resolution | 0.01 mA |
| Current setting accuracy | 0.05% + 0.5 mA |
| Current readback accuracy | 0.05% + 2 mA |
| Ripple (current) | 1 mA rms |
| LIST max steps | 200 |

---

## 8. Command Quick Reference Table

| Command | Type | Response | Implemented in `odmr-maynuo-m8812` |
|---------|------|----------|-----------------------------------|
| `*IDN?` | Query | `MAYNUO,M8812,<SN>,<FW>` | ✅ Mag-M2A |
| `SYST:REM` | Set | None | ✅ Mag-M2B |
| `SYST:LOC` | Set | None | ✅ Mag-M2B |
| `SYST:ERR?` | Query | `"<code>, '<msg>"` | ✅ Mag-M2C |
| `SYST:SENS <bool>` | Set | None | — |
| `MEAS:CURR?` | Query | Float (A) | ✅ Mag-M2B |
| `MEAS:VOLT?` | Query | Float (V) | — |
| `MEAS:DVM?` | Query | Float (V) | — |
| `VOLT <val>` | Set | None | ✅ Mag-M2B |
| `VOLT:PROT <val>` | Set | None | ✅ Mag-M2C |
| `CURR <val>` | Set | None | ✅ Mag-M2B |
| `OUTP <bool>` | Set | None | ✅ Mag-M2B |
| `MODE <FIX/LIST/DRM>` | Set | None | — |
| `LIST:*` | Set/Query | Various | — |
| `TRIG` | Set | None | — |
| `TRIG:SOUR <mode>` | Set | None | — |

---

## 9. Related Documents

- [Maynuo M8812 Lab Notes](../../lab-bringup/maynuo_m8812_lab_notes.md) — operational procedures, safety sequences, noise characterization
- [Original Full Manual](m88_manual_cleaned.md) — complete scanned manual with front-panel operation, milliohmmeter, voltmeter functions
