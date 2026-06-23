# Probe2 3D Cartesian Chunk Resolve Summary

- Probe: `probe2`
- Sweep: `2.74-3.00 GHz`, `500 kHz`, `500 ms`, `-10 dBm`
- OE profile: `configs/profiles/oe1022d_run_ch_b_observed_tc100ms.json`
- Laser profile: `configs/profiles/cni_laser_run_on_background.json`
- Chunk count: `45`
- Points per chunk: `75`
- Total points: `3375`
- Resolve status: `45/45` passed
- Estimated run hours per chunk: `5.5187`

## Chunking rule

- X: `0..280 uT`, step `20 uT` (15 values)
- Y: split into 3 groups per Z level: `0..80`, `100..180`, `200..280 uT`
- Z: `0..280 uT`, step `20 uT` (15 values)
- Each run: `15 x 5 x 1 = 75 points`

## Key files

- Manifest: `configs/generated/probe2_xyz20_full_2740_3000_500khz_500ms_chunk_manifest.csv`
- Summary: `configs/generated/probe2_xyz20_full_2740_3000_500khz_500ms_chunk_summary.json`
- Resolve CSV: `reports/resolve_probe2_xyz20_full_2740_3000_500khz_500ms/run_resolve_results.csv`
- Resolve JSONL: `reports/resolve_probe2_xyz20_full_2740_3000_500khz_500ms/run_resolve_results.jsonl`

## First three chunks

- `1` `probe2_xyz20_z000_y000_080_x000_280_full_2740_3000_500khz_500ms_m75` -> `5.5187 h`
- `2` `probe2_xyz20_z000_y100_180_x000_280_full_2740_3000_500khz_500ms_m75` -> `5.5187 h`
- `3` `probe2_xyz20_z000_y200_280_x000_280_full_2740_3000_500khz_500ms_m75` -> `5.5187 h`
