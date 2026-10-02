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

## Revision 2026-10-02 — widths, ring at the border, fallbacks, dev map

User request; numbers and rationale in
[DECISION-0137 revision](../../decisions/0137-field003-road-network-concept.md).
Profile `field003-roads-v1.json`: main road 10, dead end 8, end radius 7,
ringInset 7, fieldPadding 2 (2 units of grass outside the ring), interior
margins 27/23. Profile validation accepts ring edge exactly at fieldPadding.

Six fallbacks re-exported by the C# generator from the new profile (seeds
17/42/91/123/256/777 → 13/14/15/13/13/14 books, none used fallback); a test
now matches each stored fallback to the generator output for its seed.
[Schema image](../../prototypes/field003-roads/fallbacks-2026-10-02.png).

Generation measurement, EditMode batch, seeds 0–299 (temporary scratch test,
removed after export): fallback 0/300; layout attempts 1 for all 300; graph
attempts mean 1.93, median 1, p90 4, p99 6, max 8 (budget 24×4); books 10–19,
mean 14.8, three seeds at the minimum 10; time median 250 ms, p95 383 ms,
max 485 ms (PerfGuard warning threshold 500 ms). `FieldRoadLayout` exposes
`GraphAttempts`/`LayoutAttempts`.

Dev map (development launcher, Editor/Development Build only): roads, dead-end
corridors and round ends drawn at world widths in the profile colours, in the
surface painting order; summary adds the road piece count. Fields without roads
are unchanged.

Checks, Unity 6000.6.0f1 batch:
- EditMode `^Game\.(Presentation|Bootstrap|UI)\.` **286/286 PASS**:
  `TestResults/checks/20261002T081653-603580Z/summary.json`.
- PlayMode `ProductionField003SmokeTests` with graphics **1/1 PASS**:
  `TestResults/checks/20261002T081808-804572Z/summary.json`; overview capture
  shows the ring along the border.
- `generate.py --check` UP TO DATE.

Not run: full EditMode/PlayMode suite; visual check of the dev map overlay in
an interactive session.

## Revision 2026-10-02 — smooth road edge

User request: no snagging on the road edge, smooth round ends around books.
The 0.5-cell mask (staircase boundary, up to ~0.35 off the outline) is replaced
by a signed distance field on the same grid corners; marching squares with
linear interpolation produce both the player-only `EdgeCollider2D` loops and
the meshes (`FieldRoadDistanceField`, `FieldRoadMarchingSquares`).

Tests (six fallback layouts): contour vertices and edge midpoints within 0.05
of the analytic outline on smooth stretches, ≤0.3 chord at junction corners;
far half of every round end on its radius ±0.03; mesh triangles lie on the road
and the mesh area equals the contour-enclosed area within 0.2%; branch field
matches dead ends only; connectivity now checked on the field corners. Physics:
a 0.5-radius player body held diagonally into an oblique main road edge and
circling outward around a round end keeps ≥70% of its tangential speed.

Checks, Unity 6000.6.0f1 batch:
- EditMode `^Game\.(Presentation|Bootstrap|UI)\.` **291/291 PASS**:
  `TestResults/checks/20261002T091533-665246Z/summary.json`; no road PerfGuard warnings.
- PlayMode `ProductionField003SmokeTests` with graphics **1/1 PASS**:
  `TestResults/checks/20261002T091652-578642Z/summary.json`; gameplay capture
  shows smooth main road, bend and round end.

Not run: full suites; the slide test was not executed against the former
cell boundary; manual play check of the feel.

## Revision 2026-10-02 — main road keeps the dead-end mouth

User request: at a junction the main road surface dominates; the broken
dead-end surface may reach slightly onto it. New required profile field
`deadEndMouthOverlap` = 1 (DTO, validation 0 ≤ v < mainRoadWidth/2, authoring
and generated JSON). Branch visual field = max(flat-start corridor ∪ round end,
−(main + overlap)); walkable field, colliders and generation unchanged. Dev map
draws corridors under main roads. Corridors start flat at the entrance: a round
cap of radius 4 behind the entrance would reach exactly the 1-unit strip at the
far edge of a width-10 main road.

Tests: branch field equals the analytic clipped surface; main axis at every
mouth stays main surface while the strip inside the edge shows the dead end;
nothing behind any entrance across the main road on six fallbacks; dev map order.

Checks, Unity 6000.6.0f1 batch (parallel FIELD-007 work in the same tree):
- EditMode `^Game\.(Presentation|Bootstrap|UI)\.` 278/294: all 25 road/map
  tests and `Presentation_GeneratesSparseRoadsAndBookBranches_OnFirstMapGrass`
  PASS; 16 failures share one cause outside this scope — the in-progress
  FIELD-007 visual `FIELD-007-ALTAR-POSITIVE-VISUAL-PROP` has no art
  (`TestResults/checks/20261002T092845-996504Z`). Not a PASS for the scope.
- Before that work entered the tree, smooth-edge EditMode 291/291 PASS
  (`20261002T091533-665246Z`).
- PlayMode `ProductionField003SmokeTests` with graphics **1/1 PASS**
  (`20261002T093121-429832Z`); overview shows dead-end surface stopping at the
  main road edge.
