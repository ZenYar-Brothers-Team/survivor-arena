# FIELD-010 — strike motion and audio evidence, 2026-10-03

Execution status belongs to [STATUS](../STATUS.md). Scope and parameters: [DECISION-0159](../../decisions/0159-field010-strike-motion-and-audio.md).

## Implementation and provenance

The eight approved runtime PNGs are retained. Art pipeline plan returned `changedFiles: 0`; no new raster or artistic replacement was introduced. Shader motion uses the hazard simulation clock: radial reveal and rotation for circles, interior scrolling for strips/large rectangles, angular ink movement for the expanding ring and moving exterior ribbons around Judgment's safe disk. Pulse/reveal parameters are validated in the typed profile and authored in the existing generator input. A separate fixed boundary remains visible during reveal; all analytic damage masks and safe cutouts retain their geometry. Pool return clears material properties and clock; gameplay pause freezes event time.

`HazardStrikeStarted` emits once per telegraph-to-strike boundary, including boundaries crossed by a large Tick. Bootstrap injects the runtime into Audio; Audio selects one of four families for all 13 production event IDs. Sources are existing CC0 OGGs with unchanged hashes/provenance in SOURCES.json. No added AudioSource pool or gameplay dependency on Audio. Important pool/cooldown policies, settings, silent automated tests and pause/resume are retained. Audio validator: 55 files, 41 cues, hashes/licenses/references PASS. Generation --check UP TO DATE.

## Review capture method

Graphics PlayMode renders 12 exact presentation-clock samples per event plus warning, strike and done captures: 195 PNGs for 13 events. The simulation is paused during PNG encoding because disk latency can outlast a brief strike; the production view/shader renders samples of the unchanged hazard at the chosen event times. This is a controlled presentation sequence, not a continuous live gameplay recording. A separate live circle strike assertion verifies configured sound playback, silence on speakers during automation, paused event clock/audio and resume of the same voice.

The local gallery `TestResults/field010-art-review/motion.html` combines five GIF sequences (strips, circles, ring, half screen and Judgment) with click-to-play sound, plus four audio family auditions. GIF timing is slowed for inspection; gameplay timing is unchanged. Source OGG samples are copied byte-for-byte. User motion/listening approval remains open; passing tests do not imply artistic approval.

## Fresh final scoped checks

- Graphics FIELD-010 PlayMode: 3/3 passed, 0 failed/skipped, `TestResults/checks/20261003T192831-935974Z/summary.json`, Unity 6000.6.0f1, closed-Editor safe batch. Live sound/pause assertions passed; 195 fresh captures generated with corrected frame sampling.
- Combined art/audio/presentation/ScreenEvents/FIELD-010 EditMode: 181/181 passed, 0 failed/skipped, `TestResults/checks/20261003T193047-455189Z/summary.json`, safe batch. Manifest audit: 347 owner/role records PASS (identity/path validation, not pixel quality).
- Both runs recorded fresh PASS receipts; generation and audio validation passed. AI inspected actual early/late circle frames and half-screen strike; each of the five review GIFs contains 13 frames (12 strike samples plus done). All four review OGGs match source files byte-for-byte; local page returned HTTP 200.

## Broader regression attempts

Full EditMode attempt `TestResults/checks/20261003T191326-173979Z/EditMode.xml`: 1601/1605 passed, 4 failed, 0 skipped. Affected subsets inside this run passed: Audio 6/6, ScreenEvents 53/53, FIELD-010 integration 8/8, Presentation 111/111.

All Game.* graphics PlayMode attempt `TestResults/checks/20261003T192323-517055Z/PlayMode.xml`: 67/73 passed, 6 failed, 0 skipped; FIELD-010 3/3 passed including live audio pause/resume. This attempt preceded the capture-only timing correction described above. The unrelated failures below prevent a whole-project PASS.

### EditMode failures

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

### PlayMode failures

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
