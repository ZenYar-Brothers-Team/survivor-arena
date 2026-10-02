# FIELD-003 road runtime — evidence 2026-10-02

Scope: user-authorized implementation directly on current FIELD-003, no new
dev field. [Decision](../../decisions/0137-field003-road-network-concept.md),
[authoring profile](../../balance/field003-roads-v1.json).

## Implementation

- Seeded C# sparse graph generator: MST, optional long-cycle links, mandatory
  ring connections, smoothed degree-two bends, independent book branches.
  24 graph attempts × up to 4 layouts; approved six-layout fallback after
  budget exhaustion, logged with the original seed. No per-frame generation.
- Shared 0.5-unit cell surface drives two colored meshes and player-only
  closed EdgeCollider2D contours. Reuses FIELD-001 grass, skips former FIELD-003
  rock/column patterns and decor. Continuous player physics restores on cleanup.
- Player starts on the closest main axis to center; contact size is checked
  using the selected character's actual collider after its presentation binding.
- One PICKUP-002 at each round end before run start, exact position without
  scatter. Existing contact, pause, cleanup, lifetime=null and currency rules;
  explicit choice-count override=1. Traveler Books retain the weighted 1–3.
- Ordinary waves, enemy pool, bosses, Travelers, spawn/teleports and drops
  unchanged. Grass boundary excluded from Traveler placement clearance.

## Verification

Unity 6000.6.0f1, safe `check_project.py` batch routing after a fresh process
probe (initial sandbox probe required escalation). `--graphics`.

Final full run: `TestResults/checks/20261001T214720-209808Z/`:
EditMode **1285/1285 PASS**, PlayMode **61/63**, no skipped tests.
FIELD-003 smoke passed (startup, book positions/count override, player-only
boundaries, existing monster pool). Full result is **FAIL**, not a full PASS:

- `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`:
  expected 70 entries, actual 85. Current working tree includes 15 newly added
  low-tier sets outside the road task; the test uses the earlier count.
- `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`:
  `field-select-FIELD-DEV-ZONES` bottom 1250 > 1081 at the tested resolution.
  Field selection UI/layout was not changed by this task.

Separate FIELD-003 PlayMode receipt after inspecting these failures:
`TestResults/checks/20261001T215137-756834Z/summary.json`, **1/1 PASS**,
0 failed/skipped, graphics. No runtime edits between full and scoped runs.

Scoped physics evidence: `TestResults/checks/20261001T214537-925565Z/summary.json`,
**4/4 PASS**, no skipped. Walking and 500-unit/s forced velocity stay on road;
non-Player body passes; all 11 reference branch mouths can be crossed in both
directions with radius 1; cleanup removes boundaries. Boundary build has a
1-second regression bound in `FieldRoadSurfaceTests`.

Final EditMode includes six reference layouts with preserved 11/14/15/18/13/15
book counts, seed replay and 32 newly generated layouts. New layouts:
**13–19 branches**, **0 fallback uses**, all walkable cells/book centers connected.
The first series took **0.18–0.39 s/layout**, including surface rasterization,
flood validation and assertions (not just generator CPU time).

Content generation `--check`: UP TO DATE; audio integrity: 28 files / 15 cues
PASS; art manifest audit: 293 owner/role records PASS. No art packet apply was
needed: existing grass/pickup bindings are reused, no new raster was authored.

Initial attempts are retained as diagnostics: scoped run
`20261001T213717-609009Z` had an obsolete rock-layout assertion, replaced by
the new road contract; `20261001T214014-611641Z` timed out at 300 s because
multi-path grass PolygonCollider2D rebuilt after every SetPath. No PASS is
claimed for these attempts. Contour construction replaced that implementation.

## Visual evidence and limits

Actual Unity screenshots inspected:
[gameplay scale](../../../TestResults/field003-roads-gameplay.png),
[whole-map overview](../../../TestResults/field003-roads-overview.png).
Main road/branches and shared boundaries are visible; stairs at the 0.5-unit
cell edge are expected in this blockout. Captures are world-only, without HUD.
Colored meshes are temporary presentation; no new raster assets were imported.

This automated scope does not establish full-run gameplay balance, all-hero
manual movement acceptance, final road textures/edge art or short-corridor
distribution on an unlimited seed population. Those gates remain in STATUS.

## Follow-up: start on the nearest road centerline

User explicitly confirmed the desired spawn rule: road centerline nearest to
arena center, even if the center itself is grass. Inspection confirmed this is
already implemented by `FieldRoadLayout.SpawnPosition` and applied to both
player Transform and Rigidbody2D by composition. No runtime algorithm change
was needed; public contract documentation and focused tests were added.

Three fixtures cover a nearest axis on either side of center (center on grass)
and a road through center. Expected points lie inside long road segments,
not at vertices/intersections or at the road edge.

Targeted EditMode **3/3 PASS**, 0 failed/skipped:
`TestResults/checks/20261002T064143-440087Z/summary.json`.
Earlier class-wide run observed 6/6 passing tests, but the runner reported
INCOMPLETE because concurrent working-tree inputs changed during execution
(`20261002T064011-815577Z`); the subsequent focused run has a confirmed receipt.
