# FIELD-010 — muted presentation evidence, 2026-10-03

Scope: [DECISION-0160](../../decisions/0160-field010-muted-presentation.md). User observations: [playtest record, OBS-01/02](../../playtests/2026-10-03_field010-visual-comfort.md). Execution authority: [STATUS](../STATUS.md).

## Correction

FIELD-010 had no obstacle layout, but the generic fallback unconditionally created a FIELD-001 stump and retained scene collision. Screen-event arenas without their own explicit/layout obstacles now disable the fixture renderer and collider instead. Own authored layouts retain their precedence. No new physical obstacle content was invented; the current approved FIELD-010 slice remains an open arena with 13 screen threats. Scene state is restored by the existing Dispose lifecycle.

Optional validated `groundTint` is neutral white for other fields and opaque `#A29AA8` for FIELD-010; CreateGround applies it to the ground renderer only. StrikeColor now also multiplies actual artwork, rather than only fill. The new strike tone is `#B5A9BF`, alpha .58, fill .12; red warning alpha .5→.8, fill .08→.14. Ink rotation is 45 degrees/s, strips 1.4 world units/s, ring .15 cycles/s, exterior .9 world units/s; pulse is disabled (Hz/depth 0). Reveal is .18 s. Damage, simulation timing, layouts, sound and original PNGs are unchanged.

Authoring input and generator were changed together; generated JSON was regenerated. GroundTint DTO/domain has validation; a test guards only FIELD-010 having a non-neutral tint. The real graphics smoke now guards no foreign Stump, no spawned blocking colliders, disabled fixture collider and the actual ground renderer tint. Regression map updated.

## Observed checks

- Art pipeline plan: changedFiles 0. No raster operation, GUID replacement or new provenance record.
- Safe closed-Editor art EditMode: 123/123 passed, 0 failed/skipped, Unity 6000.6.0f1, `TestResults/checks/20261003T194456-898916Z/summary.json`. Manifest audit: 347 records PASS (ownership/identity/paths, not pixel quality).
- Full EditMode: 1603/1607 passed, 4 failed, 0 skipped, `TestResults/checks/20261003T194645-258881Z/EditMode.xml`. The runner stops at failure, so full PlayMode is run separately. Failures are the same unrelated cases observed before this correction, listed below.
- Generation UP TO DATE; audio validator 55 files / 41 cues PASS. `git diff --check` for affected production files PASS.

### Full EditMode failures

- `Game.Bootstrap.Tests.FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`

```text
Expected and actual are both <UnityEngine.Vector2[4]>
  Values differ at index [1]
  Expected: (24.00, -2.75)
  But was:  (24.00, -2.75)
```

- `Game.Meta.Tests.MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`

```text
after FIELD-001
  Expected: (11, 11, 6)
  But was:  (11, 11, 11)
```

- `Game.Meta.Tests.MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`

```text
Expected is <System.String[5]>, actual is <System.String[10]>
  Values differ at index [5]
  Extra:    < "SET-023", "SET-024", "SET-026"... >
```

- `Game.UI.Tests.MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`

```text
Expected: equivalent to < "SET-001", "SET-004", "SET-006", "SET-010", "SET-017" >
  But was:  < "SET-001", "SET-004", "SET-006", "SET-010", "SET-017", "SET-023", "SET-024", "SET-026", "SET-030", "SET-034" >
```

## Broad PlayMode attempt and correction

The first broad graphics run `TestResults/checks/20261003T194934-564092Z/PlayMode.xml` reported 64/73 passed, 9 failed, 0 skipped. Three FIELD-010 tests exposed an error in the initial correction: disabling every environment obstacle name also disabled boundary walls, which pickup placement correctly rejected. The final code disables only the prototype obstacle collider; the four arena boundaries remain active. The final scoped repeat below determines the correction's result. The remaining six failures match earlier unrelated failures.

- `Game.Bootstrap.PlayModeTests.GameplaySmokeTests.HudSpeed_PauseAndEnd_RestoresUnityTimeScale`

```text
Expected: 3
  But was:  1.0f
```

- `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`

```text
Expected: 70
  But was:  85
```

- `Game.Bootstrap.PlayModeTests.ProductionField010ScreenEventsSmokeTests.Field010_DevelopmentEventsTab_StartsTheChosenEventTwoSecondsAfterThePress`

```text
System.InvalidOperationException : Field obstacle 'Wall_Top' requires active player-only collision.
```

- `Game.Bootstrap.PlayModeTests.ProductionField010ScreenEventsSmokeTests.Field010_PaintsEveryEventLayout_WarningAndStrike_ForReview`

```text
System.InvalidOperationException : Field obstacle 'Wall_Top' requires active player-only collision.
```

- `Game.Bootstrap.PlayModeTests.ProductionField010ScreenEventsSmokeTests.Field010_RunsScreenEvents_WarnsThenHurtsAPlayerOutsideTheSafeCircleOnce_AndRemovesThemOnShutdown`

```text
System.InvalidOperationException : Field obstacle 'Wall_Top' requires active player-only collision.
```

- `Game.Bootstrap.PlayModeTests.ProductionMonasterySmokeTests.DevUnlock_Field007Card_StartsThirtySixVisibleAltarsAndPausesCleanly`

```text
Expected: Flex
  But was:  None
```

- `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`

```text
field-select-FIELD-DEV-ZONES (x:72.00, y:945.00, width:332.00, height:305.00)
  Expected: less than or equal to 1081.0f
  But was:  1250.0f
```

- `Game.Bootstrap.PlayModeTests.UiFoundationSmokeTests.Foundation_ReferenceAndSmallViewport_CardsStayBoundedAndSubmitOnce`

```text
Expected: greater than 0.0f
  But was:  0.0f
```

- `Game.Bootstrap.PlayModeTests.UiLayoutR2SmokeTests.GameplaySpace_WhenDevelopmentButtonHasFocus_IsNotConsumedByPausePanel`

```text
Expected: same as <Button development-toggle (x:4.00, y:322.00, width:1272.00, height:38.00) world rect: (x:4.00, y:322.00, width:1272.00, height:38.00)>
  But was:  null
```

## Final scoped result after correction

Fresh safe graphics batch `TestResults/checks/20261003T195446-315437Z/summary.json`: EditMode 123/123 and FIELD-010 PlayMode 3/3 passed, 0 failed/skipped; manifest 347 PASS. Same final fingerprint for both platforms. Boundary validation and pickup placement now pass; the real scene assertions confirm absence of Stump, disabled prototype collider and configured ground tint. This resolves the three FIELD-010 failures from the first broad PlayMode attempt. Other broad failures remain outside scope; no whole-project PASS is claimed.

All 195 captures were regenerated (13 events × warning + 12 motion samples + strike + done). AI inspected current circle and half-screen strike images: foreign stump absent, actors/pickups retain color, floor and strike artwork visibly dimmer. Five review GIFs each have 13 frames; motion.html reports the muted variant and HTTP 200. Exact presentation-clock sampling/slower GIF timing remains as documented in the previous motion evidence. Comfort/readability acceptance is pending the user's new review.

Final Markdown/static checks PASS. No edits to Game_design.md or Content_design.md; the existing open-arena deviation and requested visual changes are documented in DECISION-0160, brief, project map, playtest record and STATUS.
