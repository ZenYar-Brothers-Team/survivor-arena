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

## AB-02

- `PlayerMover` accepts an optional direction source before its existing speed,
  knockback and Rigidbody2D calculation. Null retains keyboard/mouse.
- Owner-backed read-only snapshots include current enemies, projectiles, XP,
  world pickups, authored obstacle colliders and visible boss hazard shapes.
  The adapter never scans all scene objects per frame or draws future RNG.
  `DangerWash` inverse safe-zone geometry is explicitly `coverageIncomplete`;
  the bot stops rather than treating an unmodeled hazard as safe.
- `safePickup/v1` evaluates eight directions plus stop, predicts visible threats,
  gives local obstacle detours and bounded stuck recovery. Formula, units,
  ranges and worked example are in the IP-34 module §5; these are bot settings,
  not gameplay tuning or a claim about human play.
- Checks: Unity 6000.6.0f1 full graphics safe runner 975/975 EditMode +
  40/40 PlayMode, 0 failed/skipped, audio/generation/art 268 PASS at
  `TestResults/checks/20260929T101309-221659Z/summary.json`. After adding
  the prepared reachable XP scene fixture, targeted PlayMode 2/2 PASS at
  `TestResults/checks/20260929T101741-482847Z/summary.json`. Pure policy
  tests cover XP, obstacle, projectile, incomplete coverage and stuck; the
  production-scene fixtures cover Rigidbody movement, pause/end and XP pickup.
- First PlayMode attempt could not load production audio because a new worktree
  held Git LFS pointers. Cached runtime audio/art LFS objects were checked out
  into this worktree only; no production content changed. No completed natural
  balance run is claimed for AB-02.

## AB-03

- `AutomationRunHost` is attached explicitly in development, chooses an unlocked
  hero/field through normal launchers, sets an existing run speed, observes
  actual offered draft options with captured revision and a separate policy RNG,
  and waits for `ProfileSaveTask` plus matching receipt. It never grants a
  build/XP, moves a transform, locks HP or synthesizes an outcome.
- Explicit states include profile wait, selection, running/draft, result save,
  completed/stopped/failed. Manual pause, draft, wall and save timeouts are
  bounded. Stop remains incomplete even if the normal profile binding saves a
  legitimate administrative-stop reward.
- Checks: Automation EditMode 13/13 PASS,
  `TestResults/checks/20260929T102500-805369Z/summary.json`; fixture
  PlayMode 6/6 PASS, `TestResults/checks/20260929T102809-096738Z/summary.json`.
  Fixtures cover authoritative victory/defeat/stop, queued Books and stale
  revision, manual pause timeout, save failure and teardown. The fixture win
  advances the model clock only in a test; it is not a natural production run.
- Remaining verification: one naturally completed FIELD-001 production run
  after AB-04 sidecar exists; this is not yet counted as done.

## AB-04

- A development-only export sink writes the existing typed IP-31 telemetry to
  the isolated run folder, not the ordinary player-data Playtests folder.
  `automation.json` is published after the telemetry export and matching saved
  reward receipt. Administrative stops are incomplete even when their telemetry
  export and legitimate reward are complete.
- The bounded automation recorder subscribes to actual draft offers, selections
  and wave phase changes. Its counters survive dropped history; it records
  offered IDs/levels, selected IDs/levels, phase entry HP/level/XP, reached phase
  and final build. The sidecar carries config/initial-profile hashes and known
  gameplay seeds. Set-damage attribution and deterministic replay are explicitly
  unsupported; the existing run.json remains the source for damage/healing/XP.
- Composed PlayMode 6/6 PASS at
  `TestResults/checks/20260929T103928-315357Z/summary.json`, including artifact
  shape, saved profile, offers versus selections, phase, and incomplete stop.
- Verification still pending: naturally elapsed production run, recorder overflow,
  export failure, and `profile-after-purchases` (AB-05). This is implemented,
  not yet Verified.
