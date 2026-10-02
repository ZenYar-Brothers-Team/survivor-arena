# FIELD-003 — curb perspective and grass transition, 2026-10-02

User requested both corrections after reviewing the first road art:
«Да, давай, обе штуки сразу делай».

## Presentation changes

The approved curb master/runtime PNG is unchanged. Top-surface UV selects
only the upper stone face (`curbUvBounds.yMin` 0.46). Separate front-face
geometry projects 0.12 world units downward in the fixed 3/4 view, tinted
`#aaa296`. Back-facing edges have zero alpha; faces follow the viewer direction
instead of rotating the baked dark side toward the interior of every road.

A visual-only exterior dirt band, width 0.7 world units, reuses the broken-road
texture for subdued surface variation. Opacity 0.65, spatial noise scale 0.85
world units, noise strength 0.25, soil color `#8b795b`; configured in
[road authoring](../../balance/field003-roads-v1.json).
The shader fades to zero alpha at the outside edge, revealing the actual
FIELD-001 grass layer already beneath the roads. Soil color is converted for
the active color space, avoiding a bright halo in the project's linear mode.

No new raster, import exception, collider, walking-width change, generation
change, reward relocation or entity rule was introduced. All new geometry and
materials remain owned by the existing road runtime and its disposal path.
Art pipeline plan: 0 changed files; immutable masters/provenance are preserved.

## Fresh verification

- Road boundaries, motion/cleanup and new perspective/feather regression:
  EditMode 7/7, 0 failed/skipped,
  `TestResults/checks/20261002T105402-944526Z/summary.json`.
- Art/catalog/config validation: EditMode 103/103, 0 failed/skipped;
  manifest 314 owner/role records PASS,
  `TestResults/checks/20261002T105900-764050Z/summary.json`.
- Final FIELD-003 gameplay composition, books, player-only road boundaries and
  current enemy pool: graphics PlayMode 1/1, 0 failed/skipped,
  `TestResults/checks/20261002T110009-750270Z/summary.json`.
- Content generation check: UP TO DATE. AI inspected the final captures:
  [gameplay road](../proposals/2026-10-02-field003-road-art/edge-polish/gameplay.png),
  [gameplay book crop](../proposals/2026-10-02-field003-road-art/edge-polish/book-end.png),
  [whole circle / perspective review](../proposals/2026-10-02-field003-road-art/edge-polish/curb-circle.png).

The whole-circle image uses a wider review camera to show front and back
sides together; gameplay camera settings are unchanged. AI saw a consistent
front-face direction and a soft exterior grass transition without a pale halo.
User accepted the correction from the shown screenshots: «Выглядит хорошо,
закомить?» (2026-10-02). This visual acceptance is separate from the automated
checks; no full-duration run or unrelated project-wide PASS is claimed.
