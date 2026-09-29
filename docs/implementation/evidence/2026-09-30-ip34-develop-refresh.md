# Automation worktree: develop-evg refresh — 2026-09-30

Execution status and gates belong to [STATUS](../STATUS.md).

## Integration scope

The user requested another `develop-evg` integration into the isolated automation
worktree, physically `D:\GitHub\survivor-arena-automation`. Starting automation
HEAD: `a7b110183f2f609fec5fa63a08e18e64f9026ae7`. Selected local `develop-evg`:
`2ea7a8390f1137e2eb08cc99013d56525229b089`; fresh fetch showed
`origin/develop-evg` at `744b2d8b57122de037f50159853d6a3bf8fb5515`, already included
in the local branch. The primary checkout and its interactive Editor were left
alone. No ordinary game profiles or saved-data paths were moved in this refresh.

The incoming commit changes spawn bias to 1.0, adds the approved FIELD-001
continuous-spawn opening multiplier (0.6 for 30 seconds), removes the visible
perimeter fence while retaining player boundary colliders, and fixes Space
pause handling when a DEV/HUD button holds focus. These are incoming approved
trial changes, not new balancing decisions made during integration. Their
manual gameplay/visual acceptance boundaries remain unchanged.

## Conflict resolution and local changes

- No C# or content-file merge conflicts. Generated JSON is taken with its
  incoming authoring sources; it is not hand-patched.
- The text conflict in `docs/regression-map.md` is resolved by retaining both
  automation/recorder coverage and incoming spawn/perimeter regression entries.
- The automation presentation ADR is renumbered from 0102 to
  [0104](../../decisions/0104-balance-runner-presentation.md), because the incoming
  branch owns 0102 for invisible field boundaries and 0103 for opening spawn
  cadence. Current links are updated; historical evidence keeps its original
  chronology. The older, unrelated duplicate 0054 already existed in starting
  HEAD and is not changed by this merge.
- The two pre-existing local URP settings remain unstaged and byte-identical:
  `UniversalRP.asset` SHA-256
  `105C850A8996289ABBB82E404CCD00D855FD16CDC59055392F623849F9B4C71B`;
  `UniversalRenderPipelineGlobalSettings.asset` SHA-256
  `ADCF683780138E1C4987ADD26CE706020423AC21725CA7FB391FFB096AFDCFEA`.

## Verification

- Python balance suite: **25/25 PASS** after integration.
- Fresh full Unity graphics smoke uses the repository `smoke-check` safety
  procedure and safe `check_project.py` batch runner against the isolated
  worktree: **1044/1044 EditMode + 57/57 PlayMode PASS**, zero failed/skipped,
  no third-party tests counted. Unity `6000.6.0f1`;
  `TestResults/checks/20260929T210159-952480Z/summary.json`
  (2026-09-30 local date / 2026-09-29 UTC).
- Generation `UP TO DATE`; audio integrity **28 files / 15 cues PASS**; art
  provenance **269 records PASS**. This does not confer visual approval.
- Passed coverage includes run lifecycle, damage/death, XP/level-up/draft,
  active skills, wave/spawn/pool, composition root, UI presenters, content
  loading, GameplaySmokeTests, and recorder integration. Earlier recorder
  builds/pilots are not counted as a new player run of this gameplay revision.

Existing standalone player builds and earlier bot/human-recorder pilot data
retain their original build provenance. This source refresh does not rebuild a
player, run a new bot pilot, record a human session or train a model. A new player
build is required before running the updated gameplay in the automation player.
Documentation impact: incoming approved design changes are retained; integration
adds this evidence, synchronizes STATUS, and disambiguates ADR links only.
