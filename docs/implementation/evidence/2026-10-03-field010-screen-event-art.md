# FIELD-010 — screen-event art evidence, 2026-10-03

Execution authority: [STATUS](../STATUS.md). Visual contract:
[DECISION-0158](../../decisions/0158-field010-screen-event-art.md),
[visual brief](../../art/briefs/field010-screen-events-visual-candidates-v1.md).

## Selection and preparation

The user approved the eight reviewed candidates: «хорошо, можешь встраивать в игру».
[Approved packet](../../../Art/Packets/field010-screen-events-v1.json) preserves the
original PNG hashes and verbatim built-in imagegen prompts. The standard pipeline
plan/apply prepared 35 files, including selected masters, provenance and eight runtime
derivatives. Technical fitting uses alpha crop, noise cutoff 8 and transparent padding;
no new artistic variant was substituted. Runtime PNGs use explicit import profiles,
512 maximum size (1024 for strip textures), 320 PPU, bilinear filtering, no mipmaps,
uncompressed RGBA, full-rect sprite and centered pivot. Unity generated the new metadata.

## Runtime integration

The authoring packet owns `screenEventPresentation`; `scripts/content/fields.py`
regenerates its production environment record. Eight typed visual references are
resolved and role-checked. Five sweep events bind spear, blade or cloud; the remaining
layouts use warning/strike strips, circles, warped ring strips and the safe-circle rim.

The shader clips the actual hazard geometry, repeats strip interiors and preserves
world border thickness. The ring gap uses constant width rather than a fixed angular
wedge. Judgment masks out the exact safe disk; narrow exterior ribbons leave the floor
visible. Sweep bodies retain their dimensions while entering/exiting a lane. Colors,
alpha, border/repeat/ribbon dimensions and sorting are authored in JSON. Ordering -4/-3
keeps event artwork beneath actor bodies. Existing pickup ordering is retained; the
mechanical brief's requested position above drops is not asserted by these checks.
This remains a hierarchy detail for the gameplay visual gate, not a change to game rules.

One driver owns a shared quad, eight materials and pooled four-layer views. The event
clock freezes outside Running. End/shutdown hides and returns views, clears property
blocks, unsubscribes callbacks and disposes shared materials/mesh. No colliders, damage,
layout, event schedule or other field artwork are changed by this integration.

## Fresh checks

Unity 6000.6.0f1; safe runner selected batch only after process/lock preflight.

- `art_pipeline.py Art/Packets/field010-screen-events-v1.json`: plan/apply, 35 files.
- `content/generate.py --check`: UP TO DATE.
- FIELD-010 + ScreenEvents graphics checks:
  `TestResults/checks/20261003T110922-733449Z/summary.json`, EditMode 54/54,
  PlayMode 2/2, zero failed/skipped; fresh PASS receipt.
- Final art scope: `TestResults/checks/20261003T111151-700027Z/summary.json`,
  EditMode 114/114, zero failed/skipped; manifest audit 342 owner/role records valid;
  fresh PASS receipt. Pixel quality is evaluated separately by captured-frame inspection.
- Final graphics capture run after repeat/capture timing adjustments:
  `TestResults/checks/20261003T111748-869822Z/summary.json`, PlayMode 2/2,
  zero failed/skipped, fresh PASS receipt; 26 updated warning/strike PNGs.
- Final manifest metadata audit after recording integration/evidence: 342 records PASS.

The PlayMode checks cover warning → one player hit → cleanup, pause, pooled reuse,
shutdown, and warning/strike capture for all 13 layouts. Captures are 1280×720;
the main/capture cameras share the same aspect ratio. The review page is
`TestResults/field010-art-review/index.html`, images `TestResults/field010-*.png`.

Wide burst rectangles repeat the artwork in both dimensions; narrow lanes keep the
cross-section without extra repeats. Sweep captures use the middle of flight so even
the slow cloud wave is visible inside the viewport.

Initial PlayMode tests caught a MaterialPropertyBlock created in a MonoBehaviour field
initializer; it is now created during Initialize. Later captured-frame review reduced
Judgment ribbon density and corrected capture-camera aspect. These corrections precede
the passing receipts above. A separate first art run had passing tests/audit but no
receipt because a parallel FIELD-004 prototype changed project inputs; it was rerun.

## Wider project check limitations

The full EditMode run (`20261003T105416-728306Z`) ran 1567 Game tests:
1563 passed, 4 failed, 0 skipped. Unrelated failures:

- `Game.Bootstrap.Tests.FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`:
  Vector2 array differs at index 1 despite displayed coordinates `(24.00, -2.75)` matching.
- `Game.Meta.Tests.MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`:
  after FIELD-001 expected `(11, 11, 6)`, actual `(11, 11, 11)`.
- `Game.Meta.Tests.MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`:
  expected 5 startup sets, actual 10, extra SET-023/024/026/030/034.
- `Game.UI.Tests.MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`:
  expected startup sets 001/004/006/010/017, actual also 023/024/026/030/034.

The broad graphics PlayMode run (`20261003T105713-228686Z`) ran 71 Game tests:
63 passed, 8 failed, 0 skipped. Two FIELD-010 failures were the initializer bug fixed
and verified above; six other failures remain outside this art task:

- `Game.Bootstrap.PlayModeTests.GameplaySmokeTests.HudSpeed_PauseAndEnd_RestoresUnityTimeScale`:
  expected 3, actual 1.
- `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`:
  expected 70, actual 85.
- `Game.Bootstrap.PlayModeTests.ProductionMonasterySmokeTests.DevUnlock_Field007Card_StartsThirtySixVisibleAltarsAndPausesCleanly`:
  expected Flex, actual None.
- `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`:
  FIELD-DEV-ZONES card bottom 1250 exceeds 1081.
- `Game.Bootstrap.PlayModeTests.UiFoundationSmokeTests.Foundation_ReferenceAndSmallViewport_CardsStayBoundedAndSubmitOnce`:
  expected positive value, actual 0.
- `Game.Bootstrap.PlayModeTests.UiLayoutR2SmokeTests.GameplaySpace_WhenDevelopmentButtonHasFocus_IsNotConsumedByPausePanel`:
  expected development-toggle focus, actual null.

Passing full EditMode coverage includes Run (21), Combat (16), Progression (184),
ActiveSkill (127), Enemy (269), Pooling (8), Bootstrap (133), UI (151) and Content (43).
The wider project has no full PASS; no IP is promoted on this evidence.

## Visual gates

Source preparation/import and target-scale technical inspection are recorded here;
artworks render with intact contours, transparent centers and no pink shader/missing
texture in inspected frames. Actor bodies and pickups remain readable, the ring corridor
and Judgment safe disk are visibly clear. Static captures do not substitute for motion
review or the user's gameplay acceptance. Gate E remains open for that review.
