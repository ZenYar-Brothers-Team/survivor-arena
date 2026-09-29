# IP-34 automation implementation evidence — 2026-09-29

Runtime status and packet order live only in [STATUS](../STATUS.md).

## AB-01

- Strict version-1 experiment config in `Game.Automation`: required fields, IDs,
  production meta references, first-field runtime binding, policy version/ranges,
  path confinement and output collision are checked before launch.
- `PresetProfileBuilder` and `scripts/balance/prepare_preset.py` create a declared
  lab starting profile through production Meta definitions. Personal spending is
  reconstructed by level; no fake run receipts. The input profile is captured at
  validation, then each chain receives its own memory profile store. Settings
  use `MemorySettingsStore`, never the player settings file.
- Examples and usage: `scripts/balance/examples/`, `scripts/balance/README.md`.
- Checks: `python -m unittest discover -s scripts/balance -p 'test_*.py' -v`
  3/3 PASS; `python scripts/check_project.py --scope code --platforms EditMode
  --filter '^Game\.Automation\.'` Unity 6000.6.0f1 6/6 EditMode PASS,
  0 failed/skipped, batch with safe Editor preflight,
  `TestResults/checks/20260929T095720-373237Z/summary.json`;
  docs selected-path STATIC PASS. A prior 5/5 run was not accepted because Unity
  imported `.meta` files during the check and the runner marked inputs changed.
- Coverage: malformed/unknown config, locked starting hero/field, policy ranges,
  invalid personal owner/cap, immutable config snapshot, independent chain
  purchase state, preset source change after validation, player save/settings
  sentinels, no invented receipts. No production run is claimed for AB-01.
