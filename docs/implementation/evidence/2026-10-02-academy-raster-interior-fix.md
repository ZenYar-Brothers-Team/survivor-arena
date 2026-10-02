# Academy — approved raster interior overlay fix

User reported that the approved book and portal symbol periodically looked
replaced by purple procedural ink. `ZoneSealPresentationRuntime` kept the
rotating Motion mesh enabled above the raster glyph (sortingOrder +2 vs +1);
Burst preparation also scales those arcs up from the center, crossing the glyph.
The fallback Glyph mesh was already hidden, but the second procedural interior
layer was still visible. With an approved raster glyph both interior meshes now
stay disabled. This also protects the approved knockback arrows.

The PNGs, sprite identities, import settings and art packets are unchanged.
No new raster generation/import is involved. The rim animation, raster orientation,
light/fade, ground flattening and gameplay geometry/timing remain unchanged.
Contract clarification: [DECISION-0153](../../decisions/0153-academy-single-burst-portal.md).

Regression: `RasterGlyphs_ApprovedSymbols_StayStillWithoutProceduralInterior_ThroughWholeOccurrence`
samples each of the three kinds at 161 times from 0 to 40 seconds. It verifies
sprite identity, visibility, absence of both procedural interior renderers,
orientation/flattening, window exit and reinitialization. Its Motion-disabled
assertion fails with the previous renderer gating.

Initial runner attempts could not execute with an open Editor and unavailable
REST; after the user closed Editor the safe graphics runner was started.
Fresh results are recorded below; execution status belongs only to STATUS.md.

Safe graphics runner PASS, Unity 6000.6.0f1: 13/13 EditMode and 1/1 PlayMode,
zero failures/skips, `TestResults/checks/20261002T194427-132908Z/summary.json`.
Fresh waiting/preparing/active captures were produced; preparing and active
images were inspected and show the book/spiral/arrows without inner arc overlays.
Manifest 334 records PASS; scoped diff whitespace check PASS.
