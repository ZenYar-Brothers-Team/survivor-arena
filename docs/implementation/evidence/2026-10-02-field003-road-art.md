# FIELD-003 — road art, 2026-10-02

User selected the grey paving/stone-curb concept («да, берем первую») and
approved main paving, broken paving and curb candidates («да, берем»).
Packet: [field003-roads-art-v1](../../../Art/Packets/field003-roads-art-v1.json).
Immutable masters, exact prompts and hashes are retained under
`Art/Source/Fields/field-003-road-{main,broken,curb}/`.

## Rendering contract

The current marching-squares road meshes and player-only boundary contours
are unchanged. Main/branch meshes use world XY divided by `art.surfaceRepeat`
in [authoring profile](../../balance/field003-roads-v1.json), currently 8 world units.
The road shader mirrors sampling at tile edges (full mirrored period 16 units)
without changing or cloning the imported non-readable textures.
Grass continues to use FIELD-001. Existing branch-mouth overlap is preserved.

One visual-only curb mesh follows the same closed contours, including holes
and circular book ends. Width 0.38 world units, row repeat 4 units; normalized
UV bounds select the solid strip within the approved transparent canvas.
These values affect presentation only. Miter joins are bounded to prevent spikes.
No curb collider, new obstacle, reward relocation or enemy rule was added.
Owned meshes and materials are removed with the road runtime; imported textures
remain shared. New references participate in registry validation.

## Checks and captures

- Art import/catalog: 100/100 passed, 0 failed/skipped, in the fresh shared
  EditMode run (1359/1359 overall),
  `TestResults/checks/20261002T102159-847116Z/EditMode.xml`.
  Manifest validator: 314 owner/role records PASS.
  The standalone final art runs passed 100/100 but did not produce reusable
  receipts because other project inputs changed during them. The fresh shared
  EditMode run supplies current evidence for the same 100 art/catalog cases;
  its still-running general PlayMode stage is not claimed as passed here.
- Road physics, world UV, shader sampler and cleanup: 6/6 EditMode, 0 failed/skipped;
  `TestResults/checks/20261002T101446-159408Z/summary.json`.
- Actual FIELD-003 composition and current enemy pool: graphics PlayMode 1/1,
  0 failed/skipped; `TestResults/checks/20261002T101602-598216Z/summary.json`.
- `generate.py --check`: UP TO DATE; art pipeline plan/apply completed.
- Captures: `TestResults/field003-roads-gameplay.png`,
  `field003-roads-overview.png`, `field003-roads-book-art.png`.
  Preserved review copies: [road](../proposals/2026-10-02-field003-road-art/gameplay.png),
  [book end](../proposals/2026-10-02-field003-road-art/book-end.png),
  [overview](../proposals/2026-10-02-field003-road-art/overview.png).
  AI inspected the gameplay and circular book-end captures. Player/book remain
  readable; curb follows the visible edge. Mirrored surface repetition can still
  be noticed on broad open paving; dense combat and artistic scale need user review.

The first road-art attempt failed because Unity forbids cloning a non-readable
imported texture. Sampling was moved into a resource-loaded road shader and
the road tests then passed. No import readability exception was introduced.
No full-duration run or general full-suite result is claimed here.
