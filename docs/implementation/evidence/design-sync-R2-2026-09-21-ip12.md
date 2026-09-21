# IP-12 — implementation and verification evidence

Date: 2026-09-21. Scope revision: design-sync-R2. Execution status is recorded only in [STATUS](../STATUS.md).

## IP-12

User request «закомить и делай следующий» authorized committing the completed packet and implementing the next Ready module. Previous IP-11/approved DECISION-0026 packet is commit `ee9945d`. Existing `Packages/manifest.json` modification was excluded and preserved. IP-07/08/09/10A prerequisites were checked against current code and the new full test runs.

### Implementation

- Independent validated CharacterComparisonBaseline resource, explicit ordered CharacterPresentation highlights, role and typed crop/icon references. Body and motion remain separate references. Registry detects missing/wrong-type baseline and sprite references; JSON rejects absent presentation/highlights and unknown fields. Complete CharacterBaseStats validation remains authoritative for all 17 supported channels.
- ICharacterAccessProvider supplies availability and visible lock reason. AllCharacters includes locked definitions; selection and launch recheck access. Immutable fixture provider has explicit unlocked IDs; no persistence or production unlock semantics were invented.
- Real pre-run selection fixture uses CharacterSelectPresenter → immutable ContentCard states → CharacterSelectScreen. Cards show role, starting-skill name, only declared comparisons, crop/icon placeholders and reason. Baseline is never inferred from roster order/selection; empty highlights stay empty even for large stat deviations.
- GameplayCompositionRoot holds dependent adapters before startup, initializes actual chosen character/skill/stats after confirmation and starts the run only after successful composition. Existing lower-level initialization/rollback remains. Shutdown → selection → fresh run resets stats, modifiers and active skill; telemetry captures the actual choice rather than setup's initial default.
- Selection document owns a separate top-level scene panel. Reopening under the HUD document originally triggered a UI Toolkit panel inheritance assertion; the new PlayMode smoke reproduces this path and protects the fix. HUD Shutdown detaches its visual tree and panel.

### Coverage and verification

Runner: `scripts/Test-Unity.ps1`, Unity **6000.6.0f1**, batchmode/nographics. A fresh Win32_Process check preceded every invocation; no interactive Unity Editor was running. Filter `^Game\.`; third-party tests selected: 0.

Final result: **439/439 EditMode, 6/6 PlayMode, 0 failed, 0 skipped**, 2026-09-21. Evidence files: `TestResults/EditMode.xml`, `TestResults/PlayMode.xml` and matching logs. Final EditMode followed the panel-lifecycle correction; final PlayMode used the same code/config revision. Later changes were documentation only.

New CharacterSelectPresenterTests cover visible lock conditions, resolved art/role/starting skill, ordered numeric comparisons, no autofill, roster reorder/selection baseline independence, access recheck at launch, failed launch retry, duplicate start exclusion, event detachment, invalid/missing baseline/fields and all-locked roster. Existing CharacterFrameworkTests retain deterministic weighted sampling, zero-weight exclusion, slot occupancy and modifier composition.

CharacterSelectionSmokeTests uses real NavigationSubmit events: locked Sturdy cannot start; explicit fake profile unlocks it; selecting its card starts RING L1 in 1/6 slots with configured HP/speed; telemetry records Sturdy; modifiers apply/remove; Shutdown → default Agile creates a new run ID, restores fresh stats and BOLT L1 without the previous ring/modifier. Existing gameplay, skill-pattern, UI-foundation, set and telemetry smokes also pass after explicit pre-run confirmation.

Critical-path regressions covered in passed suites: lifecycle/pause/end, combat/death, XP/draft, active/passive/set effects, wave/spawn/pooling, content registry/catalog, composition, UI and telemetry. `git diff --check`, content JSON parsing and modified-document local links checked separately.

### Limits and documentation impact

No production CHAR data, weight numbers, unlock prices, new PNG or approved portrait derivatives. Sturdy world body and both selection crop/icon roles resolve to explicit fixture placeholders through existing FixtureSpriteCatalog; Agile's existing approved world asset is untouched. No manual artistic acceptance or production visual completion is claimed. G-14/G-15 and G-17/G-18 remain with their owning production/art packets.

IP-12 schema/API, IP-22 authoring, IP-25 profile provider, IP-26 integration, STATUS/readiness and regression guard synchronized. [DECISION-0027](../../decisions/0027-character-selection-composition.md) records cross-layer implementation for architecture review (Proposed, not product approval). GDD/Content Design production values and species remain unchanged; existing approved DECISION-0026 is implemented. OBS-01 remains open, without tuning changes.
