# Evidence before design-sync-R2

Датированный архив evidence из STATUS, перенесён 2026-09-21. Это описание
прежнего scope, не текущие статусы, gates или требования. Старые утверждения
о Draft/setting сохранены как исторические факты и не переопределяют новый канон.
Текущая очередь, паузы, готовность и применимость evidence — только в [STATUS](../STATUS.md).
Читать нужный IP, а не весь архив. Пути в inline code — от корня репозитория.

## IP-00

Historical scope status: Verified
Historical implementation evidence: commit `bb726e6`; `Assets/Game/Content/Model/ContentRegistry.cs`, `ContentId.cs` и связанные content contracts.
Historical verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; `ContentRegistryTests.cs` покрывает duplicate/missing/wrong-type/invalid IDs, invalid definitions/references, обязательный Build и загрузку configured fixture set.
Historical deviations: none recorded.
Historical documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

## IP-01

Historical scope status: Verified
Historical implementation evidence: commit `bb726e6`; `Assets/Game/Run/Model/RunModel.cs` и `RunController.cs`.
Historical verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; все 7 `RunModelTests.cs` passed.
Historical deviations: none recorded.
Historical documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

## IP-02

Historical scope status: Verified

Historical implementation evidence: commit `9223059`; `Assets/Game/Movement/Presenters/CameraFollowTarget.cs` и `Assets/Scenes/Gameplay.unity`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 30/30 tests passed on 2026-09-13; `Camera_FollowsPlayerAtViewportCenterAndPreservesDepth` проверяет wiring, центр viewport после смещения Player и сохранение camera depth.

Historical deviations: [DECISION-0001](../../decisions/0001-player-centered-camera.md) — утверждено правило центрирования камеры.

Historical documentation impact: Game Design и IP-02 синхронизированы; Content Design не затронут, так как изменение не содержит content entities или balance-data.

## IP-03

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Character/` — base/modifier stat model, death-to-run binding и `PlayerCharacterRuntime`; health model живёт в `Assets/Game/Combat/Health.cs` (см. DECISION-0006) и используется через `IHealthProfile`, который реализует `CharacterStats`; base stats больше не `[SerializeField]`-поля — грузятся через `FixtureCharacterCatalog.CreateDefault()` из `Resources/Content/Characters/FixtureCharacters.json` (см. DECISION-0009); `Assets/Scenes/Gameplay.unity` — runtime подключён к Player как movement speed source.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 43/43 tests passed on 2026-09-13; 13 IP-03 tests покрывают damage, capped heal, regeneration и pause, death-to-lost, base + keyed modifiers без double counting, stat clamps/removal и Gameplay scene wiring. Health model и JSON-конфиг base stats re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above); scene не содержала кастомных значений в удалённых полях — потери данных нет.

Historical deviations: [DECISION-0006](../../decisions/0006-shared-health-model.md) — health model вынесен из `Game.Character` в общий `Game.Combat` для переиспользования будущими не-enemy юнитами; [DECISION-0009](../../decisions/0009-json-content-config.md) — base stats вынесены из инспектора в JSON; CHAR-001…010 и PASSIVE-001…010 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design и IP-03 scope не изменились; execution status и readiness зависимых модулей синхронизированы; evidence path для health model и base stats обновлён.

## IP-04

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/` — content-compatible definition (теперь с опциональной `Visual`-ссылкой, см. DECISION-0007), seek movement, contact damage, lifecycle/factory и continuous fixture spawner; health переиспользует `Game.Combat.Health` через `FixedHealthProfile` (см. DECISION-0006); enemy definitions грузятся через `FixtureEnemyCatalog.Create()` из `Resources/Content/Enemies/FixtureEnemies.json` вместо C#-литералов (см. DECISION-0009); `EnemyRegistry.TryFindNearest` обёрнут в `PerfGuard` (см. DECISION-0008); `Assets/Scenes/Gameplay.unity` — configured `EnemySpawner`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 61/61 tests passed on 2026-09-13; 18 IP-04 tests покрывают definition/registry, health/death, seek/stop simulation, immediate/repeated contact damage, sustained-contact death, pause-safe contact/spawn timers, runtime spawn/despawn и Gameplay scene wiring. Health/visual/JSON-config/perf-log refactors re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: [DECISION-0002](../../decisions/0002-repeated-contact-damage.md) — утверждены повторные тики для длительного contact damage; [DECISION-0006](../../decisions/0006-shared-health-model.md) — `EnemyHealth` заменён на общий `Health`+`FixedHealthProfile`; [DECISION-0007](../../decisions/0007-content-referenced-visuals.md) — `EnemyDefinition` получил опциональную ссылку на визуал, резолвится и рендерится через `EnemyRuntime`; [DECISION-0008](../../decisions/0008-perf-logging.md) и [DECISION-0009](../../decisions/0009-json-content-config.md) — perf-логирование и JSON-конфиг; scene использует только `FIXTURE-ENEMY-SEEKER`, ENEMY-001…020 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design enemy schema, IP-04 scope/checks и DECISION-0002/0006/0007/0008/0009 синхронизированы; exact production intervals остаются TBD; DECISION-0007 и DECISION-0009 закладывают то, что потребует IP-20 (production enemies) для presentation asset и content config.

## IP-05

Historical scope status: Verified

Historical implementation evidence: the original single-skill definition/runtime (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was superseded in place by IP-08's leveled `ActiveSkillProgressionDefinition`/`PlayerActiveSkillSetRuntime` and, once confirmed unused by `GameplayCompositionRoot`, deleted as dead code on 2026-09-15 (own tests included) rather than kept as a parallel implementation; `Assets/Game/Enemy/Model/IEnemyDamageReceiver.cs` and `EnemyDamageRequest.cs` — единый контракт урона врагам, unaffected by the cleanup; `Assets/Scenes/Gameplay.unity` — configured `FIXTURE-SKILL-BOLT` на Player.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 81/81 tests passed on 2026-09-14, at the time this module's own single-skill implementation still existed; its acceptance criteria (automatic trigger without attack input, damage/cooldown multipliers, projectile hit/kill via the enemy damage contract, pause/end behavior) are now exercised by IP-08's implementation instead — see IP-08 evidence, and the full suite re-verified 131/131 on 2026-09-15 (see cross-cutting verification above) after the dead code's removal.

Historical deviations: none recorded; scene использует только `FIXTURE-SKILL-BOLT`, SKILL-001…015 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design и IP-05 scope не изменились; execution status и readiness зависимых модулей синхронизированы; `ActiveSkillLevelDefinition` позже (IP-08 evidence) получил опциональную `Visual`-ссылку, см. [DECISION-0007](../../decisions/0007-content-referenced-visuals.md).

## IP-06

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/` — progression/bar model, configurable thresholds, pause-safe physical drop runtime, pickup/expiry/recovery pipeline and level-up event; `EnemyDefinition.ExperienceReward` and `EnemyRuntime` — XP spawn at death position; `CharacterStats` — recovery, pickup multiplier and drop-lifetime hooks; `Assets/Scenes/Gameplay.unity` — configured fixture runtime.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 90/90 tests passed on 2026-09-14; 9 `Game.Progression.Tests` tests cover death drop position/reward, pickup, expiry with base 0% recovery, recovery without double award, configurable lifetime/thresholds, XP bar state, multiple levels, level-up pause/event and pause-safe drop behavior. `Initialize` rename re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: none recorded; fixture thresholds and reward are non-production configuration, the final XP curve remains out of scope. `PlayerExperienceRuntime.ConfigureForTests` was renamed to `Initialize` (guarded, throws if already initialized) to match every sibling runtime's composition-root wiring convention instead of exposing a test-only entry point; `GameplayCompositionRoot` now calls it directly and the 5 existing test call sites were updated in place — no behavior change.

Historical documentation impact: Game Design, Content Design and IP-06 scope did not change; execution status and direct dependant readiness synchronized.

## IP-07

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Draft/` — stable-ID active/passive definitions, immutable 6+6 slot ownership, levels 1…6, unified eligibility pool and validated draft session; `LevelUpDraftRuntime.cs` — starting active slot, queued level-up drafts, UI-owned chooser state, resume-after-selection and iterative consumption of pending drafts when no eligible acquisition/upgrade remains; `Assets/Scenes/Gameplay.unity` — configured non-production fixture pool.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 98/98 tests passed on 2026-09-14; 8 IP-07 tests cover starting slot, filling 6 active + 6 passive slots, 1→6 upgrades, full-slot and max-level filtering, duplicate/invalid selection rejection, unified pool, pause/resume and queued drafts for multiple level-ups; scene integration verifies chooser wiring and fixture-only IDs. Exhausted full/maxed build and repeated no-option level-ups were re-verified in the full Unity 6000.6.0f1 suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: none recorded; offer count, pool entries and display labels are non-production fixture configuration, while reroll, banish, sets and weighted selection remain out of scope.

Historical documentation impact: Game Design and IP-07 now explicitly record the user-approved no-options rule; Content Design is unaffected because no entity or balance data changed.

## IP-08

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/ActiveSkill/Progression/` — six-level definitions (`ActiveSkillLevelDefinition` теперь несёт опциональную `ContentRef<SpriteDefinition> Visual`, см. DECISION-0007), activation-wave composition and typed projectile/beam/orbit/boomerang/chain/area/mine effects; `ProjectileDirectionGenerator` defines `Cross` as exactly four diagonal rays, distinct from arbitrary-count `Ring`; all 8 fixture skills × 6 levels now load from `Resources/Content/ActiveSkills/FixtureActiveSkills.json` via `FixtureActiveSkillCatalog.Create()`, with the 7 polymorphic effect types resolved by a `"kind"`-discriminated `ActiveSkillEffectJsonConverter` (see DECISION-0009) instead of hardcoded ternary logic; `PlayerActiveSkillSetRuntime.cs` — concurrent build-synchronized skills; `SceneActiveSkillEffectExecutor.cs` and `FixtureProjectileRuntime.cs` — pause-safe execution, delayed/multi-wave scheduling, pierce and return passes; `SceneActiveSkillEffectExecutor.TickMines` обёрнут в `PerfGuard` (see DECISION-0008); `Gameplay.unity` — fixture catalog runtime replaces the legacy single-skill fixture.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 110/110 tests passed on 2026-09-14; 12 IP-08 tests cover exact levels 1→6, numeric and qualitative level changes, fan/ring/cross directions, pierce, boomerang return and per-pass hits, delayed AoE pause, chain retarget/falloff, mine lifetime/concurrent limit, rotated multi-wave execution, concurrent acquired skills, draft upgrades and scene wiring. Visual-reference, perf-log and JSON-config migration (all values hand-transcribed from the prior hardcoded logic) re-verified against the full suite (131/131, 2026-09-15). The diagonal four-ray Cross, its distinction from Ring and rejection of invalid Cross counts were verified in the full Unity suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: [DECISION-0007](../../decisions/0007-content-referenced-visuals.md) — `ActiveSkillLevelDefinition`/`ActiveSkillProgressionDefinition` gained an optional per-level `Visual` reference validated by `ContentRegistry`; the fixture Ring/Cross skill declares two distinct visual ids per tier. [DECISION-0008](../../decisions/0008-perf-logging.md) and [DECISION-0009](../../decisions/0009-json-content-config.md) cover perf-logging and JSON-driven content, including the polymorphic-effect converter pattern. All catalog entries and numeric parameters are explicitly `FIXTURE-*`, while SKILL-001…015 remain Draft compatibility targets rather than production content.

Historical documentation impact: IP-08 now defines the diagonal four-ray Cross contract; Game Design and Content Design are unchanged because this distinguishes a framework layout and fixture content without changing production entities.

## IP-09

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Passive/` defines six-level passive progressions, now loaded via `FixturePassiveCatalog.Create()` from `Resources/Content/Passives/FixturePassives.json` — the former `level * X` formulas are expanded into explicit per-level values in config (see DECISION-0009); `PlayerPassiveSetRuntime.cs` applies keyed build-synchronized modifiers; `CharacterStats.cs` implements additive source percentages, asymptotic cooldown reduction and 99% damage-reduction cap; proportional current-health rescaling lives in `Assets/Game/Combat/Health.cs` (see DECISION-0006) via the `IHealthProfile` `Changed` event.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 121/121 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes keyed replacement, six levels, multiple stat categories, additive percentages, cooldown asymptote, damage-reduction cap, proportional health and scene-level passive selection. Health file-path change and JSON-config migration re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: production PASSIVE-001…010 remain out of scope; fixture passives are non-production. Stat semantics are recorded in [DECISION-0004](../../decisions/0004-character-stat-composition.md); the health file path was superseded by [DECISION-0006](../../decisions/0006-shared-health-model.md); passive values moved to JSON per [DECISION-0009](../../decisions/0009-json-content-config.md).

Historical documentation impact: Game Design, Content Design, IP-09 and direct dependant readiness synchronized.

## IP-10

Historical scope status: Verified

Historical implementation evidence: `DraftRunControls.cs` owns configurable run-local counters and banished stable IDs; `DraftPool.cs` filters banished entries and guarantees a changed reroll offer set when an alternative exists; `LevelUpDraftRuntime.cs` validates actions, rebuilds the open draft, resolves successful reroll/banish exhaustion without constructing an empty `DraftSession`, and drains consecutive unavailable pending drafts iteratively; `GameplayCompositionRoot.cs` supplies serialized fixture counts.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 128/128 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes repeated rerolls, alternative offers, invalid and exhausted actions, persistent banish filtering, reset, last-option exhaustion, scene configuration and the live level-up flow. Reroll-to-empty, banish-last followed by another level-up, full/maxed 6+6 exhaustion and pause release were re-verified in the full Unity suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: exact counts and recovery remain CG-04 balance TBD; the scene values of 2 rerolls and 2 banishes are explicitly non-production fixture configuration.

Historical documentation impact: Game Design, IP-07 and IP-10 now explicitly define no-options resolution; Content Design is unaffected; IP-26 dependency readiness remains unchanged.

## IP-10A

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/UI/` contains immutable HUD/draft/build/run-overlay states, model/view contracts, a pure presenter, runtime adapter, stable semantic element IDs and UI Toolkit UXML/USS/runtime theme; visible HP/XP bars and 6+6 live build slots surface IP-03/IP-05/IP-06/IP-08/IP-09 state; `GameplayUiRoot.cs` owns the thin Unity lifecycle adapter; `GameplayCompositionRoot.cs` injects gameplay dependencies; gameplay-owned IMGUI was removed from `LevelUpDraftRuntime.cs`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 134/134 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes presenter HUD/draft/build snapshots and intents, development-build gating, pause/result state, required UXML semantic elements and runtime theme import, scene wiring, non-zero resolved HP-bar geometry, 6+6 rendered build slots and live HUD/draft/build updates through two level-ups with reroll/banish.

Historical deviations: development commands use fixed fixture amounts and are visible only in Editor/Development Build; production presentation, navigation and content screens remain IP-26 scope.

Historical documentation impact: [DECISION-0005](../../decisions/0005-vertical-ui-delivery.md) records the user-approved AI-first vertical UI pipeline; `WORKFLOW.md` and every IP specification now define a feature-owned UI/observability contract; IP-26 integrates and completes the UI rather than introducing it for the first time; Game Design and Content Design behavior are unchanged.

## IP-31

Новый модуль; прежнего implementation/verification evidence нет.

## IP-32

Новый модуль; прежнего implementation/verification evidence нет.

## IP-11

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Sets/` defines validated active/passive recipe components with minimum levels, probability-gated `SetDefinition`s, unlimited slot-free acquisition and one independently ticked/disposed extra-ability instance per acquired set; `DraftPool` and `PlayerBuild` integrate sets into the unified level-up flow while blocking duplicates and levels; `FixtureSetCatalog` loads two non-production shared-component recipes from `Resources/Content/Sets/FixtureSets.json`; `GameplayUiPresenter`/ViewState/UXML expose acquired sets separately from 6+6 slots and fixture recipe progress with stable semantic IDs; `FixtureRuntimeContentCatalog` validates recipe content references and composes the fixture set runtime.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 136/136 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Seven new EditMode checks cover unmet and threshold-complete recipes, probability miss/hit, slot-free level-less acquisition, duplicate rejection, shared components across multiple sets, independent extra-ability tick/disposal, missing recipe references and live `LevelUpDraftRuntime` acquisition; UI presenter/assets and gameplay smoke verify separate sets state and fixture recipe-progress observability.

Historical deviations: `FIXTURE-SET-*` recipes, names and 50% draft chances are explicitly non-production test configuration; fixture extra abilities provide lifecycle/tick observability without claiming a production gameplay effect. Production SET-001…008 recipes, weights and concrete effects remain IP-19 scope and content-gated.

Historical documentation impact: Game Design, Content Design and IP-11 scope remain unchanged; execution evidence and IP-19 dependency state were synchronized.

## IP-12

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Character/` defines validated character definitions, typed starting-build references, per-active-skill draft weights with a default of 1, and a roster that exposes only explicitly unlocked choices; `DraftPool` performs seeded weighted sampling without replacement and removes zero-weight active skills before random selection; `LevelUpDraftRuntime` resolves the selected character's starting active skill through the shared content registry. `FixtureCharacterDefinitionCatalog` loads two non-production profiles from `Resources/Content/Characters/FixtureCharacters.json`; `GameplayCompositionRoot` selects the configured unlocked fixture, applies its base stats and starting loadout, and UI Toolkit exposes unlocked character cards with starting skill and compact stat snapshots.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 141/141 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Five new EditMode checks cover two distinct fixture profiles, unlocked/locked selection, starting active-slot resolution, reproducible seeded weighted sampling, zero-weight exclusion, character-dependent deterministic outcomes, and passive modifiers layered on the selected base stats. Content-registry, UI presenter/assets, scene wiring and PlayMode smoke additionally verify referenced skills, the configured selected character and unlocked-character observability.

Historical deviations: `FIXTURE-CHARACTER-AGILE` and `FIXTURE-CHARACTER-STURDY`, their stats, unlock flags and weights are explicitly non-production test configuration. Production CHAR-001…010, production starting SKILL references and persistent unlock state remain IP-22/IP-25 scope and content-gated.

Historical documentation impact: Game Design and Content Design already define the required character rules and remain unchanged; DECISION-0009 was synchronized with the expanded JSON character shape. IP-22 no longer depends on unfinished framework work (IP-17 and CG-01 still block production characters), and IP-25 now remains blocked only by IP-16 plus its production-economy gate.

## IP-12A

Historical scope status: Verified

Historical implementation evidence: module registered at `docs/implementation/modules/IP-12A-visual-presentation-foundation.md`; user-approved visual language and generation contract are recorded in `docs/art/ART_DIRECTION.md`; user-approved source/runtime layout, naming/version lifecycle, provenance, raster/import contract and replacement gates are recorded in `docs/art/ASSET_PIPELINE.md`; gameplay/presentation isolation, single-writer pose composition, data-driven motion profiles and authoritative signal/lifecycle contracts are approved in `docs/decisions/0013-procedural-sprite-presentation.md` and enforced for future work by `AGENTS.md`. `Assets/Game/Presentation/` provides a validating JSON-backed motion-profile catalog, pure pose compositor, isolated rig, pause-aware/reinitializable runtime and Editor importer enforcing the sprite contract; its zero-velocity idle cycle combines profile-driven breathing squash/stretch, vertical bob and slight anxious sway, then blends out as locomotion reaches reference speed. `Health.Damaged` supplies authoritative hit reactions. `CharacterDefinition` resolves both visual and motion-profile content references, `FixtureRuntimeContentCatalog` registers those definitions, and `GameplayCompositionRoot` initializes and rolls back the presentation subsystem. `Gameplay.unity` uses `Player/VisualRoot/{BodyRoot,ShadowRenderer}` and no longer renders or deforms the gameplay root. Its Editor/Development-Build UI exposes the real presentation runtime through reproducible `Live`, `Idle`, `Left`, `Right` and `Reset` controls; damage and pause continue to use authoritative gameplay signals. Development tooling is now collapsed by default behind a compact `DEV` launcher and uses bounded `Run`, `Build` and `Presentation` tabs with scrolling for long content, per the user-approved extension to DECISION-0005 and the project workflow. The user-approved goblin iterations, selected master and verbatim prompt/provenance are stored under `Art/Source/Characters/fixture-character-agile/`; the normalized runtime body is `Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png` and is loaded through `FixtureSprites.json` instead of placeholder fallback.

Historical verification evidence: final Unity 6000.6.0f1 regression passed on 2026-09-16: EditMode 159/159 and PlayMode 1/1. Checks cover motion-profile validation, visible zero-velocity idle pose, diagonal motion calibration, equivalent pose at simulated 30/120 FPS, presentation safety bounds, pause/win/loss freeze, pose composition, preview override without authoritative-body mutation, pause/reset and repeated initialization, damage signaling, character presentation references, runtime catalog resolution, showcase UI intents/assets, composition-root wiring and the scene hierarchy boundary. The imported body is validated as 512×512, 320 PPU, Full Rect, custom ground-contact pivot, Bilinear, Clamp, no mipmaps, uncompressed and Resources-loadable; alpha analysis found a 73.4% visible-height silhouette with a fully transparent outer border. PlayMode verifies the real goblin sprite, facing, hit flash, pause freeze and reset in Gameplay. All referenced JSON documents parse successfully, required source/master/runtime/`.meta` artifacts exist, and `git diff --check` passes. The user approved the integrated gameplay-scale motion on 2026-09-16 after the idle frequency was reduced from `1.35` to `0.675`; the reusable recipe, provenance template and five category checklists satisfy the handoff criterion for adding the next asset without redesigning the pipeline.

Historical deviations: user explicitly prioritized this new cross-cutting fixture vertical slice before IP-13. It uses `FIXTURE-CHARACTER-AGILE`; Draft `CHAR-001…010` remain unimplemented and CG-01 is not bypassed. DECISION-0013 moves Player visual ownership out of `PlayerMover`/root `SpriteRenderer` into a child presentation rig without changing movement or collision rules.

Historical documentation impact: module scope and execution order registered; `docs/art/ART_DIRECTION.md` and `docs/art/ASSET_PIPELINE.md` approved by the user on 2026-09-16 as the visual and asset-delivery sources of truth; DECISION-0013 and `AGENTS.md` define the implementation boundary for procedural sprite work. `ASSET_PIPELINE.md` now includes the complete 13-stage reuse recipe and separate checklists for characters, enemies, projectiles, pickups and UI portraits/icons; `Art/Templates/asset-record.template.json` provides a copyable provenance record. DECISION-0005, `WORKFLOW.md` and `AGENTS.md` require compact, collapsed, tabbed and bounded development tooling. Game Design and Content Design are unchanged because gameplay rules and production entities are unchanged.

## IP-13

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/Model/` adds validated, composable movement and attack profiles/controllers for seek, keep-distance, orbit, zigzag, approach-retreat and telegraphed dash plus single/fan/burst/ring/cross/spiral/explosive projectile patterns; `Assets/Game/Enemy/Runtime/EnemyProjectileRuntime.cs` and `EnemyProjectileFactory.cs` provide pooled, pause-safe projectile lifecycle, player hit/explosion damage and end-of-run cleanup; `EnemyRuntime.cs` composes profiles and renders dash telegraphs. `FixtureEnemies.json` contains seven non-production combinations, and `ContinuousFixtureEnemySpawner` cycles them without wave-system changes while sharing enemy/projectile pools. The compact development Run pane exposes current fixture ID, movement phase and attack pattern through the immutable UI state/presenter boundary.

Historical verification evidence: Unity 6000.6.0f1 EditMode 169/169 and PlayMode gameplay smoke 1/1 passed on 2026-09-17. Eight IP-13 EditMode checks cover movement-family behavior, dash direction lock/telegraph/pause, fan/ring/cross geometry, burst/spiral sequencing, direct hit/explosion miss/run-state damage, pause-safe projectile lifetime, end-of-run projectile cleanup and fixture coverage of melee/dash/required projectile families. Full-suite scene/content/UI checks verify catalog registration, composition-root wiring and semantic UI assets; `git diff --check` passed.

Historical deviations: all new definitions and numeric values are explicitly `FIXTURE-*` compatibility content; Draft `ENEMY-001…020`, bosses and canonical encounter schedules remain unimplemented. No product-rule deviation recorded.

Historical documentation impact: Game Design and Content Design remain unchanged because the module implements their existing framework contract without promoting Draft entities. Execution evidence and direct dependant readiness were synchronized.

## IP-14

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/Model/Wave/` adds validated `WaveTimelineDefinition`/`WavePhaseDefinition` (tag, duration, spawn interval, alive cap, weighted composition, independent per-phase `WaveEnemyModifiers`) and `WaveHookDefinition` mid-boss/final-boss markers, the pure `WaveDirector` (phase selection from run time, spawn-count/capacity decisions, seeded weighted enemy choice, one-shot hook events) and `WaveEnemyScaler`; `FixtureWaveTimelineCatalog` loads the non-production `Resources/Content/Waves/FixtureWaveTimeline.json` (13 phases covering ordinary/pressure/elite/rest rhythm across three escalating cycles plus a final rush, with a faster-but-frailer later pressure phase). `ContinuousFixtureEnemySpawner` now only executes director decisions and keeps the shared enemy/projectile pools (release unregisters from `EnemyRegistry`, reuse re-registers); `GameplayCompositionRoot` builds the director from the registry-validated timeline; the RNG `seed` and the `spawnRadius` live in the timeline JSON (no serialized/hardcoded copies), and enemy movement/attack/modifier DTOs carry no tuning defaults — per-kind fields are required in JSON and rejected by name when missing (see the AGENTS.md config rule). UI: HUD `hud-wave` label with per-tag style classes and a dev Run-pane wave observation (timeline time, phase, interval/cap/alive, mix, multipliers, next hook) flow through `WaveViewState`/`WaveObservabilityViewState` and the presenter. See [DECISION-0014](../../decisions/0014-wave-director-timeline.md).

Historical verification evidence: Unity 6000.6.0f1 EditMode 195/195 project tests passed on 2026-09-20 (run through the open Editor via UnitySkills, after the DTO-default/seed/spawnRadius-to-JSON refactor; the same run's only failure is in the third-party `UnitySkills` package's own tests, not in `Game.*`). PlayMode gameplay smoke 1/1 (`GameplayScene_ComposesLevelsUpAndResumes`) also passed after that refactor (the second PlayMode test in the run belongs to the UnitySkills package). 26 new EditMode checks cover definition/config validation (phases, composition, modifiers, hooks, duplicate ids), phase transitions (ordered, skipped, last-phase hold), pause/not-running behavior (no spawns, no transitions, no hooks), intensity/capacity clamping, phase-scoped weighted composition with per-phase modifiers, seed determinism and weight ratios, one-shot hook ordering, fixture rhythm (all four tags, elite pressure > respite, later phase faster-but-frailer, full 900s walk with every phase reached and no missing enemy refs), registry-validated timeline references, and pooled spawner cycles (six refill/release cycles reuse ≤ cap instances, `EnemyRegistry` returns to baseline, released enemies are inactive and never returned as targets, killed enemy is reused with fresh health, `Shutdown` clears everything). Presenter test verifies the HUD wave state and observability rebuild on transition; the asset test verifies the new semantic UXML IDs; the PlayMode smoke verifies `WAVE 1/…· ORDINARY` and the dev wave observation in the composed scene. Manual check: open the DEV panel → Run tab and watch `WAVE DIRECTOR` and the HUD wave badge change colour at ~0:45 (Pressure), ~1:15 (Elite) and ~2:00 (Respite).

Historical deviations: all phases, compositions and multipliers are explicitly `FIXTURE-*` compatibility content; canonical 15-minute schedules stay CG-02 gated (IP-24) and Draft `ENEMY-*` entities remain unimplemented. Hooks are only announced; boss spawning belongs to IP-15. No product-rule deviation recorded.

Historical documentation impact: Game Design and Content Design unchanged (framework implements the existing rhythm contract without promoting Draft content); DECISION-0014 added.

## IP-15

Historical scope status: Ready
Dependencies IP-01, IP-13 and IP-14 are Verified; consume `WaveDirector.HookTriggered` (MidBoss/FinalBoss) per [DECISION-0014](../../decisions/0014-wave-director-timeline.md).

## IP-16

Historical scope status: Blocked
Historical blockers (до миграции): IP-15 (IP-14 is Verified).

## IP-28

Новый модуль; прежнего implementation/verification evidence нет.

## IP-29

Новый модуль; прежнего implementation/verification evidence нет.

## IP-25

Historical scope status: Blocked
Historical blockers (до миграции): IP-16; production economy remains CG-03 gated. IP-12's `CharacterRoster` accepts an explicit unlocked-ID set ready to be supplied by this profile layer.

## IP-26

Historical scope status: Blocked
Historical blockers (до миграции): IP-16, IP-25.

## IP-17

Historical scope status: Blocked
Historical blockers (до миграции): CG-01 approval of target SKILL IDs.
Groundwork: [DECISION-0007](../../decisions/0007-content-referenced-visuals.md) already implements the "missing presentation asset обнаруживается validator" acceptance criterion via `ContentRegistry`/`IReferencesContent`; this IP registers real `SpriteDefinition`s and resolves per-level visuals into projectile/mine rendering instead of designing the mechanism from scratch.

## IP-18

Historical scope status: Blocked
Historical blockers (до миграции): IP-09 and CG-01 approval of target PASSIVE IDs.
Groundwork: [DECISION-0009](../../decisions/0009-json-content-config.md) already loads fixture passives from `Resources/Content/Passives/FixturePassives.json` via `FixturePassiveCatalog`; this IP registers real `PASSIVE-*` entries in that same JSON convention instead of hardcoding them.

## IP-19

Historical scope status: Blocked
Historical blockers (до миграции): IP-17, IP-18 and CG-01 approval of target SET/component IDs.
Groundwork: IP-11 provides JSON-configured recipes, probabilistic unified-draft eligibility, slot-free acquisition, independent extra-ability lifecycle and set UI/recipe observability; this IP supplies approved production definitions and concrete effects through those extension points.

## IP-20

Historical scope status: Blocked
Historical blockers (до миграции): CG-01 approval of target ENEMY IDs.
Groundwork: `EnemyDefinition.Visual` and the `Game.Presentation` module ([DECISION-0007](../../decisions/0007-content-referenced-visuals.md)) already resolve end-to-end into `EnemyRuntime` rendering for the fixture enemy; `FixtureEnemyCatalog`/JSON ([DECISION-0009](../../decisions/0009-json-content-config.md)) already loads enemy definitions from config instead of code; this IP only needs to register real `SpriteDefinition`s and `ENEMY-*` entries in those same conventions.

## IP-21

Historical scope status: Blocked
Historical blockers (до миграции): IP-15 and CG-01 approval of target BOSS/MIDBOSS IDs.

## IP-22

Historical scope status: Blocked
Historical blockers (до миграции): IP-17 and CG-01 approval of target CHAR/starting SKILL IDs.
Groundwork: IP-12 provides JSON-driven character definitions, validated starting-skill references, base stats, disappearing-XP recovery, weighted draft values including zero, unlocked-only selection and fixture UI observability. [DECISION-0006](../../decisions/0006-shared-health-model.md) keeps `PlayerCharacterRuntime` health independent of a single stat profile, while [DECISION-0007](../../decisions/0007-content-referenced-visuals.md) remains the presentation-reference convention for production character art.

## IP-23

Historical scope status: Blocked
Historical blockers (до миграции): IP-16, required production content and CG-01 approval of target FIELD IDs.

## IP-30

Новый модуль; прежнего implementation/verification evidence нет.

## IP-24

Historical scope status: Blocked
Historical blockers (до миграции): IP-14, IP-20, IP-21, IP-23 and CG-02 missing Approved Wave / Encounter Content.

## IP-27

Historical scope status: Blocked
Historical blockers (до миграции): all in-scope preceding system modules; content-complete verification also requires Approved production modules.

## Cross-cutting evidence

Ниже сохранена исходная запись базы `d9a1970`. Её описание старого goblin-only setting и Draft gates относится к прежней документации и не переопределяет новый канон. Runtime по-прежнему fixture-only; новые production IDs не реализованы этой миграцией.

Cross-cutting verification: Unity 6000.6.0f1 EditMode 195/195 passed on 2026-09-20 and PlayMode 1/1 passed after the DTO-defaults refactor; continuous spawning is driven by a data-driven wave timeline per [DECISION-0014](../../decisions/0014-wave-director-timeline.md). The gameplay scene is composed from one fixture runtime catalog; character selection is restricted to unlocked definitions and supplies base stats, starting active skill, presentation references and per-skill weighted draft values; draft RNG is seeded; pause ownership is reason-based; enemy targeting uses a live registry and reusable query buffers; gameplay UI follows a presenter/ViewState boundary; fulfilled set recipes participate probabilistically in the same draft while acquired sets remain outside 6+6 slots. Field bounds and ordinary obstacles follow [DECISION-0003](../../decisions/0003-player-only-field-collision.md); stat composition follows [DECISION-0004](../../decisions/0004-character-stat-composition.md); vertical UI delivery follows [DECISION-0005](../../decisions/0005-vertical-ui-delivery.md); the shared `Game.Combat.Health` model follows [DECISION-0006](../../decisions/0006-shared-health-model.md); content-referenced presentation assets follow [DECISION-0007](../../decisions/0007-content-referenced-visuals.md); scale-sensitive operations use throttled perf warnings per [DECISION-0008](../../decisions/0008-perf-logging.md); entity/content values are config-driven from JSON per [DECISION-0009](../../decisions/0009-json-content-config.md); `GameplayCompositionRoot` rolls back already-initialized subsystems on partial init failure per [DECISION-0010](../../decisions/0010-composition-root-rollback.md); frequently spawned/destroyed GameObjects (XP drops, enemies, skill mine markers and enemy projectiles) are pooled per [DECISION-0011](../../decisions/0011-gameobject-pooling.md); numeric constructor-argument validation is centralized per [DECISION-0012](../../decisions/0012-shared-numeric-validation.md); procedural sprite presentation is isolated from gameplay transforms and composed by a single writer per [DECISION-0013](../../decisions/0013-procedural-sprite-presentation.md). All production `.cs` files now hold exactly one type, named after the file (`AGENTS.md`'s "one type per file" rule); the pre-IP-08 single-skill prototype (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was dead code — never wired into `GameplayCompositionRoot` — and was deleted along with its tests, dropping the suite from 142 to 131; the also-dead `NearestEnemyTargetSelector` (superseded by `EnemyRegistry.TryFindNearest`, never used in production) was deleted along with its test, dropping the suite to 129. `PlayerCharacterRuntime.Initialize` now atomically wires `Health` to `RunController` (no window where damage could be taken before death routes to run-end).

Setting boundary audit: Game Design and Content Design define the player as a small goblin escaping an escalating pursuit by villagers, soldiers, knights, mages, clergy and angels. Runtime contracts remain setting-neutral and production IDs remain unimplemented; IP-12A now deliberately includes one user-approved goblin body as `FIXTURE-CHARACTER-AGILE` pipeline evidence. Enemies, skills and other fixture visuals still use placeholders, and no production `CHAR-*`, human, knight, mage or angel entity/art is implemented. Thematic production binding remains owned by IP-17…IP-24 and final presentation/player flow IP-26.
