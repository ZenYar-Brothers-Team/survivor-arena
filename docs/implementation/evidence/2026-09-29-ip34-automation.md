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

## AB-05

- One `AutomationCampaignHost` owns a single isolated profile chain and starts
  successive ordinary `AutomationRunHost` runs only after the previous outcome,
  telemetry, reward save and intermission purchases finish. It writes
  `initial-profile.json` and `profile-after-purchases.json` under its chain and
  augments the sidecar with actual purchases/refusals/currency. The external
  runner will start a fresh process/store for each independent chain (AB-06).
- The purchase policy sorts affordable personal upgrades by next saved price,
  then ordinal ID, and re-reads level/price after every successful
  `ProfileService.PurchaseAsync`. The route policy repeats after defeat,
  advances after a real victory only if the next field is unlocked/playable,
  and emits explicit `routeBlocked`/`routeCleared` reasons.
- Automation EditMode 15/15 PASS,
  `TestResults/checks/20260929T104432-797166Z/summary.json`; campaign PlayMode
  2/2 PASS, `TestResults/checks/20260929T104728-381452Z/summary.json`.
  The campaign fixtures show two losses sharing one saved profile with one
  purchase, and a fixture victory unlocking/starting FIELD-002 through normal
  production navigation. The fixture victory uses a model time jump, not the
  natural pilot.
- Remaining verification: standalone chain reset, crash/watchdog and natural
  campaign completion; no production pilot result is claimed yet.

## AB-06

- Dedicated `BALANCE_AUTOMATION` Windows Development Build entry configures
  isolated in-memory profile/settings before the gameplay root's `Start`.
  Failed bootstrap disables that root before it can fall back to production
  stores. The builder records Unity/commit/dirty and executable/data hashes.
- `run.py` checks a new output path, source/build hashes, one child at a time,
  bounded wall time, cancellation and nonzero exits. Atomic manifest updates
  retain completed runs and the last started run ID even if its report is absent.
  The runner only stops its own child; no automatic retry or cleanup.
- Python 9/9 PASS (`python -m unittest discover -s scripts/balance -p 'test_*.py' -v`),
  including sequential chains, input/output rejection, nonzero child, crash
  with missing sidecar, hang deadline and keyboard cancellation. Unity
  automation EditMode 15/15 PASS at
  `TestResults/checks/20260929T105331-165017Z/summary.json`.
- Real dedicated Windows player built at `TestResults/balance-build/ip34-balance.exe`
  with adjacent build manifest. A fresh FIELD-001 standalone smoke ran from
  `TestResults/balance-smoke-ab06/manifest.json`: run
  `530e718e15b848dbb5eba6db04721496`, 449.43 simulation seconds in 98.45
  experiment wall seconds (5× setting), then explicit `runWallTimeout` at 90 s.
  It wrote telemetry, sidecar, profile snapshots and child log, with
  `Aborted/incomplete`, not defeat; chain/experiment state `partial`, exit 0.
  This is infrastructure evidence, **not** the natural-completion pilot.
- Pending for Verified: fresh standalone natural W/L, completed multi-chain
  series and full required regression smoke. The current player must be rebuilt
  after subsequent source changes before AB-08.
- User requested that automation not appear on screen or play sound by default.
  Runner now uses Unity `-batchmode` by default and mutes the automation Player's
  listener before scene load. `--visual` opts one launch into a visible window;
  `--audio` additionally opts into sound (invalid without `--visual`). Python
  10/10 PASS. Rebuilt player at `TestResults/balance-build-headless/balance.exe`;
  real `TestResults/balance-headless-check/manifest.json` shows one bounded
  12.37 s `visual=false, audio=false` run, exit 0, planned partial timeout,
  with its own run ID and complete output packet. The runner recorded the mode;
  absence of a window/sound relies on Unity batch mode and the early mute path.
