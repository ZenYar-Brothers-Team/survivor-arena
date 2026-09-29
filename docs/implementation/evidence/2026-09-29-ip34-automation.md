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
- Natural FIELD-001 completion and the sidecar are verified in [AB-08](#ab-08).

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
- The final 979/979 EditMode and 52/52 PlayMode smoke in [AB-08](#ab-08)
  includes bounded-event overflow, export-failure/incomplete-sidecar, duplicate
  finalization and normal `profile-after-purchases` integration. Natural run
  exports are listed there.

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
- Standalone chain reset, watchdog/partial behavior and natural completion are
  verified in [AB-08](#ab-08); the route advance remains fixture-only because
  the pilot produced no victory.

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
- Fresh standalone natural W/L, completed multi-chain series, full regression
  smoke and rebuilt final player are recorded in [AB-08](#ab-08).
- User requested that automation not appear on screen or play sound by default.
  Runner now uses Unity `-batchmode` by default and mutes the automation Player's
  listener before scene load. `--visual` opts one launch into a visible window;
  `--audio` additionally opts into sound (invalid without `--visual`). Python
  10/10 PASS. Rebuilt player at `TestResults/balance-build-headless/balance.exe`;
  real `TestResults/balance-headless-check/manifest.json` shows one bounded
  12.37 s `visual=false, audio=false` run, exit 0, planned partial timeout,
  with its own run ID and complete output packet. The runner recorded the mode;
  absence of a window/sound relies on Unity batch mode and the early mute path.

## AB-07

- `analyze.py` validates and includes every manifest/discovered run, recording
  missing/corrupt/incomplete reports with exclusion reasons. Natural W/L alone
  form the win-rate denominator. It emits typed JSON, runs/chains CSV and
  Markdown with grouped duration/level/phase distributions, phase observation
  counts, profile-chain milestones (including censoring), spend, errors,
  effective speed and completed runs per ten wall minutes.
- `compare.py` refuses mismatched template/initial preset, hero, route, bot
  policies, budgets or speed; build fingerprints remain visible as candidate
  differences. It makes no paired-seed, p-value or causal skill-damage claim.
- Python 14/14 PASS (`python -m unittest discover -s scripts/balance -p 'test_*.py' -v`),
  including hand-calculated 1 W / 1 L / 1 incomplete → 50% of 2, effective
  speed 3.5×, empty/all-incomplete, corrupt/missing reports and incompatible
  speed. The real `TestResults/balance-headless-check/summary.md` correctly
  reports no available win rate, one incomplete run and ~3.21× effective
  measured speed over its startup-including 12.37 s wall interval.
- Eight natural pilot runs, full regression smoke and real-series group tables
  are recorded in [AB-08](#ab-08).

<a id="ab-08"></a>
## AB-08 — production pilot and throughput

- All experiment configs, raw reports and manifests are local ignored files in
  `TestResults/pilot-{fresh,preset,1x,600s}-ip34/`; none were committed or
  filtered by outcome. All used CHAR-001/FIELD-001, random gameplay seeds,
  ordinary player loop, `visual=false`, `audio=false`. FIELD-002 route advance
  was proved only by the AB-05 victory fixture, not by these production runs.
- Fresh template: 2 independent chains × 2 naturally completed runs, 0 W / 4 L,
  no incomplete/error. 582.120 wall s, 2849.394 simulation s, effective 4.895×,
  4.123 completed runs per 10 wall minutes; mean completed run 712.349 simulation s.
  Both initial profile hashes are
  `8ed60823f37e83a546ffbcda808b32aeb42150f79cee8e24d6d386f8d6d6f185`.
- Preset template: 2 independent chains × 2 naturally completed runs, 0 W / 4 L,
  no incomplete/error. 567.444 wall s, 2786.660 simulation s, effective 4.911×,
  4.229 completed runs per 10 wall minutes; mean 696.665 simulation s. Both
  normalized initial profiles hash to
  `c6edad01af47fae0476bfb2db78ab84dcddcae5d24b7e97ca78bc70fe90017c26`
  and are semantically equal to the unchanged preset source. Each first run
  purchased META-008 0→1, META-004 1→2 and META-003 2→3 through the normal
  profile service; the second run began with those levels. Each second run
  bought META-004 2→3. No win, unlock or balance improvement is inferred.

| Template | Chain | Run 1 ID | Run 2 ID |
|---|---|---|---|
| fresh | 0001 | `6c89cb6f14c2475ca87d31dba09d3c20` | `34cd6c91c053461eb8556393966a94f1` |
| fresh | 0002 | `38d9e10d1d504c369a500e9ed5d2c8fd` | `0653a77d285f456ba8a3a692fb271f9b` |
| preset | 0001 | `8bfa9bba1a4440f4b0b0306918d2169b` | `791dcb2e504a4514b2baca8dddab25ea` |
| preset | 0002 | `b4bbb62d32f94bfcb71b837c5c0a217d` | `10d6f1425e4c4274ac5ac3fa099eb335` |

- The separate 1× natural run `9f22e930047049959fc49840cbfb5e5b`
  lost after 847.659 simulation s in 849.270 wall s (0.998×). Its reached
  phase was FIELD-001-P15. A single different-seed 1× run versus 5× series
  does not establish a speed-dependent balance effect.
- The dedicated 600 s configured window at 5× started six runs; five ended
  naturally (0 W / 5 L), and run
  `906fbce933b44ff38d4dfb8b2c7fc22f` was exported as
  `Aborted/incomplete`, `experimentWallBudget`, with 201.362 simulation s.
  It is included in 2992.053 total simulation s but excluded from the W/L
  denominator. The worker exited 0; manifest state is planned `partial`, not
  failed. Flush ended at 603.489 wall s: effective 4.958× and 4.971 completed
  runs per 10 actual wall minutes (exactly 5 completions in the 600 s window).
  Completed runs averaged 558.138 simulation s, so this throughput is not a
  full-length-run benchmark. Of 603.489 wall s, run intervals account for
  598.168 s; startup 2.890 s, between-run transitions 0.885 s in five gaps,
  shutdown/export 1.547 s. Telemetry pause total across six runs was 0.246 s.
- Host: AMD Ryzen 5 5600H, 6 cores / 12 logical processors, 15.4 GiB RAM,
  AMD Radeon Graphics, Windows. Pilot build: Unity 6000.6.0f1,
  `TestResults/balance-build-headless/build-manifest.json`, executable SHA-256
  `718444f6a718fa668da96eb0a58445b009673310e6129152a63dbf7a92b8de00`;
  its recorded commit was `de2d97c` with dirty source, so the fingerprint,
  not that commit alone, identifies the exact player. After the recorder tests,
  a fresh player was built in `TestResults/balance-build-final/` and passed a
  silent short standalone smoke (run `ab4929aea1ca4826bb672a7a525acbbd`,
  planned incomplete timeout, not a natural W/L).
- Full post-runtime graphics check: `TestResults/checks/20260929T120900-760368Z/summary.json`
  PASS, 979/979 EditMode, 52/52 PlayMode, 0 failed/skipped; content generation,
  audio integrity and art ownership checks PASS. Python balance tests 18/18
  PASS. The ordinary non-automation production composition/run regression is
  included in the PlayMode suite; no separate no-args standalone was launched
  against the live user profile.
- Sentinel caveat: settings remained at SHA-256
  `433210c20265b125359681906899e764f65e40b71a967b43a513d39dedf5f895`.
  The live profile changed near 11:30 UTC from pre-pilot
  `62e5e19126498b0d545c799743e7eac7c46ed71a6e05c7a78c1b45b93e52980e`
  to `152be417ae7975361a0ddd44f9cdcc096bd1d7756a36b8d5002da337669be81f`,
  then again at 12:04:31 UTC during the 600 s window to
  `ef0bd14e8808a4b18e1e66ea3cf965f9e7e06ddcd1ccfd002f9af11415e51b24`.
  None of the 15 pilot run IDs appeared in it; concurrent local activity means
  these writes cannot be conclusively attributed. A separate short silent
  final-player control run `b034e97fae6a48e1b7524b0de9592461` captured
  profile and settings hashes immediately before/after; both remained
  byte-identical. No user save was restored, deleted or edited to make the
  check pass.
- The visual launch path removes both Unity `-batchmode` and Windows hidden-window
  process flags; a Python regression test checks the launch arguments. A later
  explicitly requested visual run `3aa8bfa1120540218e610ac10107ffcc` completed
  naturally (Defeat, 636.040 simulation s, 134.469 wall s), with
  `visual=true`, `audio=false`; the user reported that it occupied the full
  screen. The cause was the empty in-memory Settings store: `SettingsService`
  applied its desktop borderless default after Unity's 640×360 command-line
  window setting. Visual workers now seed only their own memory store with the
  validated SafeWindow mode (1280×720 on this desktop), selected by an explicit
  `--balance-visual` marker; silent batch workers remain unchanged. Regression:
  `SettingsServiceTests.Load_SeededAutomationWindow_AppliesWindowedModeWithoutRewriting`
  and Python runner tests. Full graphics check
  `TestResults/checks/20260929T124850-830060Z/summary.json` PASS 980/980
  EditMode + 52/52 PlayMode, 0 failed/skipped; Python 18/18 PASS. A new player
  was built under `TestResults/balance-build-windowed/`. No additional visible
  standalone was opened after the fix; on-screen window size awaits the next
  user-requested visual run.
- Limits: bot `safePickup` + `randomLegal` is not a human skill model; one hero,
  one field, no reroll/banish use, no set-damage attribution, no deterministic
  replay. All 9 required natural pilot runs lost; these small dependent samples
  do not justify a balance edit.
