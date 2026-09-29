# IP-34 AB-14 — movement demonstration recorder, 2026-09-29

Execution status and next gate belong only to [STATUS](../STATUS.md#automated-runs-execution).
Contract: [AB-14](../modules/IP-34-automated-balance-runs.md#ab-14).

## Scope and implementation

After merging local `develop-evg` at `69ea7d1` in merge `b01eddc`, prepared
`human/v1` movement recording, not a learned policy. Existing bot IDs remain
separate. Native keyboard/mouse input is not replaced; human mode requires one
chain, 1× and an explicit visual launch. The example starts each run manually
paused, pauses on focus loss, auto-selects legal drafts, and makes no meta
purchases. Profile/settings isolation uses the existing standalone bootstrap.

`PlayerMover.MovementIntentApplied` publishes clamped analog intent immediately
before applying running physics velocity. `DemonstrationRecordingSession`
captures current observations with a separate running-physics step/time clock.
Samples contain previous-step/current action, HP/XP/stats, skills/cooldowns,
enemy current phases, threats/pickups/obstacles/beams, build and bounds. Sampling
is sparse: action duration is `stepSeconds`, not the sample interval; this is
not video or a replay. Existing imperfect sensor coverage and bounded entity
truncation are explicit quality flags, not hidden completeness assumptions.

`DemonstrationWriter` sends immutable serialized lines to one bounded background
queue, flushes off the gameplay thread, reserves footer space and accounts for
UTF-8 bytes. Sample/byte/queue/I/O errors stop the run and retain `.partial`.
Only clean non-empty streams are promoted after close; no overwrite. Host finish
waits for drain, including graceful window-close veto. Repeated stop callbacks
cannot reset the asynchronous save wait. A later unrelated export failure cannot
rewrite the already-closed recording result.

`scripts/balance/record.py` prepares a unique config/output folder and explicitly
launches a windowed silent player. `validate_demonstration.py` checks the stream,
clocks, numeric data, action bounds, footer counts and quality flags. Bot recordings
remain labeled `bot`; fixtures with injected movement are not expert examples.
No learner/package installation, gameplay tuning, new HUD or production-profile
mutation is included. Architecture/script/testability guidance informed separating
pure clock/writer logic from read-only Unity adapters.

## Observed verification

Unity 6000.6.0f1; repository safe runner with graphics. The primary checkout/Editor
was not changed or closed. Original three local Unity-settings files still match
their pre-merge SHA-256; the two remaining settings diffs are excluded from this
feature's commit.

- `python -m unittest discover -s scripts/balance -p 'test_*.py'`: **25/25 PASS**.
- `TestResults/checks/20260929T194853-772585Z/summary.json`: targeted
  **57/57 EditMode + 11/11 PlayMode**, no failures/skips. Includes config, clock,
  bounded writer, native input ownership, paused sampling, analog pre-physics
  capture, overflow and stop/drain tests.
- `TestResults/checks/20260929T195032-068250Z/summary.json`: full graphics
  **1043/1043 EditMode + 56/56 PlayMode**, no failures/skips. Generation,
  audio integrity (28 files / 15 cues) and art provenance (269 records) pass.
  This run includes the idempotent repeated-stop fix, but precedes the final
  closed-session error guard and the expanded window-close callback assertion.
- After that final delta, `TestResults/checks/20260929T195348-049120Z/EditMode.xml`:
  **1043/1043 PASS**. PlayMode produced **no result**, native Editor exit
  `3758096385`. Log shows assembly-reload startup crash with Mono/Regex and
  `UnitySkills.RegistryService` initialization frames, plus low-disk warning.
- Focused retry `TestResults/checks/20260929T195549-576625Z/PlayMode.log`:
  **NOT RUN / INCOMPLETE**, no XML, exit `2147483651`; crash immediately after
  Test Runner initialization, Mono memory-pool/shortcut-attribute initialization
  in native stack. These are not passed tests or recorder assertion failures.

Read-only resource check after the crashes: C: approximately **219 MiB free**,
D: approximately **48,133 MiB free**. Fourteen old `TestResults/balance-build*`
directories occupy approximately **3.9 GiB** on C:. No directory was deleted or
moved. Disk pressure is an observed build blocker and possible contributor to
the native crashes, not a proven root cause of those crashes. No package disable,
lock removal, Editor shutdown or alternative test-channel bypass was attempted.

## Not executed / continuation

- Final full PlayMode verification after the last lifecycle delta is not complete.
- New `TestResults/balance-build-demonstration/balance.exe` was **not built**.
  README commands describe the intended build location, not an existing player.
- The hidden bot-labelled recorder pilot was **not run**. Its prepared ignored
  config is `TestResults/demonstration-pilot-config.json`: fresh CHAR-001/FIELD-001,
  safePickup, 1×, one run, 20-second experiment wall budget, 0.1-second samples,
  256 MiB file cap. No outcome, throughput or isolation measurement is claimed.
- No real human demonstration and no training have happened.

After resource recovery, rerun the final safe smoke; build a separate player;
run the hidden pilot; validate completed JSONL and profile/settings isolation.
Only then hand off an explicit human recording launch. Moving the old build
artifacts to a new directory on D: can recover space without deleting them, but
requires the user's approval and checking that no running process needs them.
Do not move the worktree, source files or ordinary game saves as part of that.
