# IP-26 implementation and verification evidence — 2026-09-21

## IP-26

Revision: design-sync-R2. Unity 6000.6.0f1. Scope: functional fixture flow and
app settings by DECISION-0038; production content/art remains with catalog IPs.
Execution status/order is recorded only in [STATUS](../STATUS.md#ip-26--functional-ui-и-полный-player-flow).

### Implementation

- Main Menu Play/Meta/Settings/Exit, Character/Field selection and Back, manual pause
  actions, Quit → Results, immediate Retry with the same selection, Main Menu cleanup.
- Settings version 1, independent file/profile lifetime, finite/range validation,
  preserve-invalid before replacement, asynchronous serialized saves and retry.
- Master/Music/SFX 0–100% controls, actual input bindings, Shake toggle. Real sources
  implement Master×channel×gain; generated preview clips exercise both channels.
- Real window/borderless/resolution adapter, pending Apply/Keep/Revert and realtime
  timeout. Close during asynchronous Apply waits, rolls back, then saves audio.
- Actual-damage shake uses IP-12A gate; bounded render-only XY offset, stable baseline
  outside render callbacks, reset on pause/off/terminal/consumer disable/teardown.
- Notification queue for level/set/Traveler/boss/unlocks; special kills contribute
  to required Results independently of recorder. Results include acquired sets,
  separate level/Book/total rewards, and unlocks. Permanent bonuses appear on cards.
- Functional UI uses existing fixture art and ordinary UITK controls. No new raster
  assets, production soundtrack, remapping or analytics screen.

### Automated checks

Fresh process check before each Unity launch; no interactive project Editor was open.
Runner: `scripts/Test-Unity.ps1`, Game.* filter, batchmode/nographics.

- Full EditMode: **637 passed / 637 total, 0 failed, 0 skipped**.
- Full PlayMode: **21 passed / 21 total, 0 failed, 0 skipped**; repeated successfully
  after wiring the shared presentation gate.
- Local Windows release build: exit 0, `Builds/IP26/SurvivorArena.exe`.
- Local evidence copies: `TestResults/IP26/EditMode.xml`, `PlayMode.xml`, accompanying
  logs and `IP26-build.log`. These generated artifacts are not source-controlled.

Coverage: settings defaults/invalid/future document/preserve failure/latest revision,
video Keep/timeout/unsupported/close-during-apply, gain isolation/master mute,
shake bound/baseline/pause/off/death/disabled consumer, shell assets/presenter,
menu/settings/selection/run/nested pause/quit/results/retry, purchase/reinitialization.
Existing run lifecycle, damage/death, XP/draft, skills, spawning, content, composition
and UI suites passed. No third-party tests were counted as project evidence.

The initial menu incorrectly stopped a not-yet-started run; existing character/field
smokes exposed this and now pass. A disabled shake consumer could retain a new hit;
the extended presentation smoke checks that enabling it does not resume that effect.
See [regression map](../../regression-map.md#ip-26--regression-guards).

### Interactive observations and remaining review

Computer Use inspected the actual Windows player. Main Menu and Settings render;
first observation exposed low contrast on default controls. Revised build has dark
buttons/light readable text, readable percentage inputs and actual movement bindings.
Native desktop mode shown: **2560×1440 borderless**. A live confirmation countdown
was observed. The user also interacted with the player; this is not evidence of all
video/audio acceptance checks passing.

Remaining manual checklist on the final build:

1. In Settings choose Windowed → 1920×1080 → Apply → Keep; inspect menu, character,
   field, pause/build and Results for clipping/overlap, including long text.
2. Apply another supported resolution and let 10 seconds expire; verify return to
   confirmed mode. Repeat Apply → Revert/Escape. Check borderless and persistence
   after normal Exit/restart.
3. Test Music/Test SFX are audible; each channel slider affects only its preview,
   Master=0 mutes both. Check previews during pause and stop on Back.
4. In a run take damage with Shake On, then Off; check the effect is weak/short and
   disappears immediately on pause/end. Inspect draft/Book/set/Traveler/boss surfaces.

Unity Play is sufficient for gameplay/audio/UI review (`Assets/Scenes/Gameplay.unity`).
OS window mode changes need the standalone player. Synthetic tones validate routing;
production music/SFX assets are deliberately outside this packet. Required subjective
sound/shake and exact 1920×1080 review are not claimed from headless tests.

### Defeat Results regression — user follow-up

User reported that the other reviewed behavior was fine, but all actions after defeat
were inaccessible. Preserved quote/diagnosis: [OBS-01](../../playtests/2026-09-21_defeat-ui.md#obs-01--после-поражения-нельзя-перезапустить-забег).
The profile saved successfully and buttons were enabled. Independent panels all had
sortingOrder=0; UIDocument order did not order separate panels. The HUD terminal
overlay, created later, covered Results. Explicit panel ordering fixes render/input:
HUD 100 < selection 200 < Results/Meta 300 < shell/settings 400.

New `DefeatResultsSmokeTests` failed before the change (Results 0 was not above HUD 0)
and passes after it: lethal damage, saved result, enabled/pickable buttons, immediate
fresh Retry with original selection/full health, second defeat → Main Menu.
Full rerun: **637/637 EditMode, 22/22 PlayMode**, 0 failed/skipped.
Previous tests submitted directly to a button, bypassing cross-panel input order;
that limitation is now explicitly covered. User review did not specify exact resolution
or provide a raw report; no telemetry run identity is inferred.
