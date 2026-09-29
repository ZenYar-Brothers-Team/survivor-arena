# Meta backdrop and complete field collection art

User feedback: [OBS-06/07](../../playtests/2026-09-29_meta-feedback.md).

## Changes

MetaScreen has an opaque full-panel dark plum backdrop, shown only while its
view state is visible. It covers the scene outside the inset shop/results panel;
it does not change the camera, pause or draft overlays.

The production sprite registry includes backgrounds for all ten field unlock
entries. RunResultsProjection (also used by the unlock collection) can resolve
that approved background without a playable FieldDefinition. Previously only
FIELD-001…003 had thumbnails through their gameplay definitions. No gameplay
fields, unlock rules or saves were changed. Existing approved raster assets and
manifest entries were reused, with no new generation/import/replacement packet.

## Verification

Full graphics PASS: `TestResults/checks/20260929T075813-309574Z/summary.json`.
Unity 6000.6.0f1, safe batch runner with closed Editor: 949/949 EditMode and
39/39 PlayMode, zero failed/skipped. Generated content current, audio 28 PASS,
art provenance 268 PASS.
New EditMode coverage resolves all ten field illustrations through the collection
projection. PlayMode checks all 70 collection images, ten field images, opaque
full-screen backdrop bounds at 1920×1080 / 1280×720 and hiding after retry.
User acceptance is not claimed by automated checks.

Fresh screenshots reviewed: `TestResults/meta-personal-1920x1080.png`,
`meta-unlocks-field-1280x720.png`, `results-production-1280x720.png`.
The backdrop covers the outer margins, field illustrations are present,
and the existing layout remains intact. `git diff --check` PASS.
