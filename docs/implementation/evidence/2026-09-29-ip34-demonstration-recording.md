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

Unity 6000.6.0f1; repository safe runner with graphics. Recorder work did not
change the primary checkout or force-close its Editor. The user subsequently
closed the Editor for the separately authorized disk relocation described below.
The two original local URP settings diffs retain their pre-merge SHA-256 and are
excluded from this feature's commits.

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
- After relocation and saved-data rollback,
  `TestResults/checks/20260929T204659-582940Z/summary.json`: final full graphics
  **1043/1043 EditMode + 56/56 PlayMode PASS**, no failures/skips. Includes the
  final closed-session error guard and actual window-close callback assertion.
  Generation, audio integrity (28 files / 15 cues) and art provenance (269
  records) pass. The previously failing standard playtest export passes too.
- Python suite repeated after relocation: **25/25 PASS**.

At the resource-blocked checkpoint after the crashes, C: had approximately **219 MiB free**,
D: approximately **48,133 MiB free**. Fourteen old `TestResults/balance-build*`
directories occupied approximately **3.9 GiB** on C:. Disk pressure was an
observed build blocker and possible contributor to the native crashes, not a
proven root cause. No package disable, lock removal, forced Editor shutdown or
alternative test-channel bypass was attempted.

The user then explicitly authorized moving the worktree, primary project and
saved data where safe. The primary project was already on D:. The complete
worktree moved to `D:\GitHub\survivor-arena-automation`, retaining the old
Git/Codex path as a junction. A separate saved-data junction failed runtime
directory creation, so active saves were restored and verified at their standard
C: path, with the complete D: backup retained. The initial post-move run had
1043/1043 EditMode and 55/56 PlayMode, not a passing smoke; the final result above
supersedes that attempt. Full inventories, rollback and process-safety details:
[relocation evidence](2026-09-29-project-disk-relocation.md).

## Standalone build and technical pilot

- Safe `scripts/balance/build.py` produced
  `TestResults/balance-build-demonstration/balance.exe` at
  `2026-09-29T20:51:36Z`. Manifest commit: `4679609`, Unity `6000.6.0f1`,
  `Development+BALANCE_AUTOMATION`. `dirty=true` accurately includes the retained
  local URP settings and documentation edits, not an unrecorded gameplay delta.
- Ignored config `TestResults/demonstration-pilot-config.json`: fresh
  CHAR-001/FIELD-001, safePickup, 1×, one run, 20-second experiment wall budget,
  0.1-second samples, 256 MiB cap. Launch was **nonvisual and muted**.
- Output `TestResults/demonstration-bot-pilot-20260929`, run
  `80436e11056d4ec5822376f2b2f1ca09`: process exit 0; 24.371 seconds total runner
  wall time including startup/export. `Aborted` / `experimentWallBudget` is a
  censored, incomplete gameplay run, not a win or loss. The experiment/chain
  manifest correctly remains `partial`; that does not mean the recording failed.
- `demonstration.jsonl`: **191 samples, 556,247 bytes**, last sample at roughly
  19.0 running physics seconds; footer `recordingComplete=true`, 955 physics
  steps, zero rejected/truncated/incomplete-observation samples, no error.
  `automation.json` matches the file/footer. Build provenance is present and
  matches the new player manifest. Validator exits 0 with `controller=bot` and
  `humanDemonstration=false`; 84 samples have zero action. This is not evidence
  of expert movement or a measured throughput benchmark.
- All **17** regular profile/settings JSON and backup files kept identical
  names, lengths and SHA-256 before/after the controlled pilot. Receipt:
  `TestResults/demonstration-pilot-isolation-20260929.json`. No ordinary game or
  Editor was active when measurement started. The player log contains no
  exception/error/failure matches.

No real human demonstration or training happened. The next human session still
requires an explicit windowed launch with the user participating; recording
fixtures and this bot-labelled pilot must not be used as human examples.
