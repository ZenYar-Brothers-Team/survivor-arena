# Implementation Status

Этот файл — единственный source of truth для состояния исполнения Implementation Plan. Спецификации в `modules/` не содержат оперативных статусов.

Last repository audit: 2026-09-15
Current next module: IP-13

Cross-cutting verification: Unity 6000.6.0f1 EditMode 141/141 and PlayMode 1/1 passed on 2026-09-15. The gameplay scene is composed from one fixture runtime catalog; character selection is restricted to unlocked definitions and supplies base stats, starting active skill and per-skill weighted draft values; draft RNG is seeded; pause ownership is reason-based; enemy targeting uses a live registry and reusable query buffers; gameplay UI follows a presenter/ViewState boundary; fulfilled set recipes participate probabilistically in the same draft while acquired sets remain outside 6+6 slots. Field bounds and ordinary obstacles follow [DECISION-0003](../decisions/0003-player-only-field-collision.md); stat composition follows [DECISION-0004](../decisions/0004-character-stat-composition.md); vertical UI delivery follows [DECISION-0005](../decisions/0005-vertical-ui-delivery.md); the shared `Game.Combat.Health` model follows [DECISION-0006](../decisions/0006-shared-health-model.md); content-referenced presentation assets follow [DECISION-0007](../decisions/0007-content-referenced-visuals.md); scale-sensitive operations use throttled perf warnings per [DECISION-0008](../decisions/0008-perf-logging.md); entity/content values are config-driven from JSON per [DECISION-0009](../decisions/0009-json-content-config.md); `GameplayCompositionRoot` rolls back already-initialized subsystems on partial init failure per [DECISION-0010](../decisions/0010-composition-root-rollback.md); frequently spawned/destroyed GameObjects (XP drops, enemies, skill mine markers) are pooled per [DECISION-0011](../decisions/0011-gameobject-pooling.md); numeric constructor-argument validation is centralized per [DECISION-0012](../decisions/0012-shared-numeric-validation.md). All production `.cs` files now hold exactly one type, named after the file (`AGENTS.md`'s "one type per file" rule); the pre-IP-08 single-skill prototype (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was dead code — never wired into `GameplayCompositionRoot` — and was deleted along with its tests, dropping the suite from 142 to 131; the also-dead `NearestEnemyTargetSelector` (superseded by `EnemyRegistry.TryFindNearest`, never used in production) was deleted along with its test, dropping the suite to 129. `PlayerCharacterRuntime.Initialize` now atomically wires `Health` to `RunController` (no window where damage could be taken before death routes to run-end).

Setting boundary audit: Game Design and Content Design define the player as a small goblin escaping an escalating pursuit by villagers, soldiers, knights, mages, clergy and angels. Current code remains setting-neutral: the scene and runtime use generic `Player`/`Enemy` contracts, `FIXTURE-*` IDs, configuration-only fixture display names and placeholder sprites; no production goblin, human, knight, mage or angel entity/art is implemented. Thematic runtime binding remains owned by production content IP-17…IP-24 and final presentation/player flow IP-26.

## Foundation and playable core

### IP-00 — Контракт контента, стабильные ID и конфигурация

Status: Verified  
Implementation evidence: commit `bb726e6`; `Assets/Game/Content/Model/ContentRegistry.cs`, `ContentId.cs` и связанные content contracts.  
Verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; `ContentRegistryTests.cs` покрывает duplicate/missing/wrong-type/invalid IDs, invalid definitions/references, обязательный Build и загрузку configured fixture set.  
Deviations: none recorded.  
Documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

### IP-01 — Run lifecycle, таймер, pause и завершение

Status: Verified  
Implementation evidence: commit `bb726e6`; `Assets/Game/Run/Model/RunModel.cs` и `RunController.cs`.  
Verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; все 7 `RunModelTests.cs` passed.  
Deviations: none recorded.  
Documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

### IP-02 — Перемещение игрока и базовая геометрия поля

Status: Verified

Implementation evidence: commit `9223059`; `Assets/Game/Movement/Presenters/CameraFollowTarget.cs` и `Assets/Scenes/Gameplay.unity`.

Verification evidence: Unity 6000.6.0f1 EditMode, 30/30 tests passed on 2026-09-13; `Camera_FollowsPlayerAtViewportCenterAndPreservesDepth` проверяет wiring, центр viewport после смещения Player и сохранение camera depth.

Deviations: [DECISION-0001](../decisions/0001-player-centered-camera.md) — утверждено правило центрирования камеры.

Documentation impact: Game Design и IP-02 синхронизированы; Content Design не затронут, так как изменение не содержит content entities или balance-data.

### IP-03 — Character stats, HP, damage, healing и regeneration

Status: Verified

Implementation evidence: `Assets/Game/Character/` — base/modifier stat model, death-to-run binding и `PlayerCharacterRuntime`; health model живёт в `Assets/Game/Combat/Health.cs` (см. DECISION-0006) и используется через `IHealthProfile`, который реализует `CharacterStats`; base stats больше не `[SerializeField]`-поля — грузятся через `FixtureCharacterCatalog.CreateDefault()` из `Resources/Content/Characters/FixtureCharacters.json` (см. DECISION-0009); `Assets/Scenes/Gameplay.unity` — runtime подключён к Player как movement speed source.

Verification evidence: Unity 6000.6.0f1 EditMode, 43/43 tests passed on 2026-09-13; 13 IP-03 tests покрывают damage, capped heal, regeneration и pause, death-to-lost, base + keyed modifiers без double counting, stat clamps/removal и Gameplay scene wiring. Health model и JSON-конфиг base stats re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above); scene не содержала кастомных значений в удалённых полях — потери данных нет.

Deviations: [DECISION-0006](../decisions/0006-shared-health-model.md) — health model вынесен из `Game.Character` в общий `Game.Combat` для переиспользования будущими не-enemy юнитами; [DECISION-0009](../decisions/0009-json-content-config.md) — base stats вынесены из инспектора в JSON; CHAR-001…010 и PASSIVE-001…010 не реализовывались как production content.

Documentation impact: Game Design, Content Design и IP-03 scope не изменились; execution status и readiness зависимых модулей синхронизированы; evidence path для health model и base stats обновлён.

### IP-04 — Enemy core

Status: Verified

Implementation evidence: `Assets/Game/Enemy/` — content-compatible definition (теперь с опциональной `Visual`-ссылкой, см. DECISION-0007), seek movement, contact damage, lifecycle/factory и continuous fixture spawner; health переиспользует `Game.Combat.Health` через `FixedHealthProfile` (см. DECISION-0006); enemy definitions грузятся через `FixtureEnemyCatalog.Create()` из `Resources/Content/Enemies/FixtureEnemies.json` вместо C#-литералов (см. DECISION-0009); `EnemyRegistry.TryFindNearest` обёрнут в `PerfGuard` (см. DECISION-0008); `Assets/Scenes/Gameplay.unity` — configured `EnemySpawner`.

Verification evidence: Unity 6000.6.0f1 EditMode, 61/61 tests passed on 2026-09-13; 18 IP-04 tests покрывают definition/registry, health/death, seek/stop simulation, immediate/repeated contact damage, sustained-contact death, pause-safe contact/spawn timers, runtime spawn/despawn и Gameplay scene wiring. Health/visual/JSON-config/perf-log refactors re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Deviations: [DECISION-0002](../decisions/0002-repeated-contact-damage.md) — утверждены повторные тики для длительного contact damage; [DECISION-0006](../decisions/0006-shared-health-model.md) — `EnemyHealth` заменён на общий `Health`+`FixedHealthProfile`; [DECISION-0007](../decisions/0007-content-referenced-visuals.md) — `EnemyDefinition` получил опциональную ссылку на визуал, резолвится и рендерится через `EnemyRuntime`; [DECISION-0008](../decisions/0008-perf-logging.md) и [DECISION-0009](../decisions/0009-json-content-config.md) — perf-логирование и JSON-конфиг; scene использует только `FIXTURE-ENEMY-SEEKER`, ENEMY-001…020 не реализовывались как production content.

Documentation impact: Game Design, Content Design enemy schema, IP-04 scope/checks и DECISION-0002/0006/0007/0008/0009 синхронизированы; exact production intervals остаются TBD; DECISION-0007 и DECISION-0009 закладывают то, что потребует IP-20 (production enemies) для presentation asset и content config.

### IP-05 — Active skill runtime и player damage pipeline

Status: Verified

Implementation evidence: the original single-skill definition/runtime (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was superseded in place by IP-08's leveled `ActiveSkillProgressionDefinition`/`PlayerActiveSkillSetRuntime` and, once confirmed unused by `GameplayCompositionRoot`, deleted as dead code on 2026-09-15 (own tests included) rather than kept as a parallel implementation; `Assets/Game/Enemy/Model/IEnemyDamageReceiver.cs` and `EnemyDamageRequest.cs` — единый контракт урона врагам, unaffected by the cleanup; `Assets/Scenes/Gameplay.unity` — configured `FIXTURE-SKILL-BOLT` на Player.

Verification evidence: Unity 6000.6.0f1 EditMode, 81/81 tests passed on 2026-09-14, at the time this module's own single-skill implementation still existed; its acceptance criteria (automatic trigger without attack input, damage/cooldown multipliers, projectile hit/kill via the enemy damage contract, pause/end behavior) are now exercised by IP-08's implementation instead — see IP-08 evidence, and the full suite re-verified 131/131 on 2026-09-15 (see cross-cutting verification above) after the dead code's removal.

Deviations: none recorded; scene использует только `FIXTURE-SKILL-BOLT`, SKILL-001…015 не реализовывались как production content.

Documentation impact: Game Design, Content Design и IP-05 scope не изменились; execution status и readiness зависимых модулей синхронизированы; `ActiveSkillLevelDefinition` позже (IP-08 evidence) получил опциональную `Visual`-ссылку, см. [DECISION-0007](../decisions/0007-content-referenced-visuals.md).

### IP-06 — XP drops, pickup, expiry и level progression

Status: Verified

Implementation evidence: `Assets/Game/Progression/` — progression/bar model, configurable thresholds, pause-safe physical drop runtime, pickup/expiry/recovery pipeline and level-up event; `EnemyDefinition.ExperienceReward` and `EnemyRuntime` — XP spawn at death position; `CharacterStats` — recovery, pickup multiplier and drop-lifetime hooks; `Assets/Scenes/Gameplay.unity` — configured fixture runtime.

Verification evidence: Unity 6000.6.0f1 EditMode, 90/90 tests passed on 2026-09-14; 9 `Game.Progression.Tests` tests cover death drop position/reward, pickup, expiry with base 0% recovery, recovery without double award, configurable lifetime/thresholds, XP bar state, multiple levels, level-up pause/event and pause-safe drop behavior. `Initialize` rename re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Deviations: none recorded; fixture thresholds and reward are non-production configuration, the final XP curve remains out of scope. `PlayerExperienceRuntime.ConfigureForTests` was renamed to `Initialize` (guarded, throws if already initialized) to match every sibling runtime's composition-root wiring convention instead of exposing a test-only entry point; `GameplayCompositionRoot` now calls it directly and the 5 existing test call sites were updated in place — no behavior change.

Documentation impact: Game Design, Content Design and IP-06 scope did not change; execution status and direct dependant readiness synchronized.

### IP-07 — Level-up draft, 6+6 slots и base build progression

Status: Verified

Implementation evidence: `Assets/Game/Progression/Draft/` — stable-ID active/passive definitions, immutable 6+6 slot ownership, levels 1…6, unified eligibility pool and validated draft session; `LevelUpDraftRuntime.cs` — starting active slot, queued level-up drafts, minimal IMGUI chooser and resume-after-selection; `Assets/Scenes/Gameplay.unity` — configured non-production fixture pool.

Verification evidence: Unity 6000.6.0f1 EditMode, 98/98 tests passed on 2026-09-14; 8 IP-07 tests cover starting slot, filling 6 active + 6 passive slots, 1→6 upgrades, full-slot and max-level filtering, duplicate/invalid selection rejection, unified pool, pause/resume and queued drafts for multiple level-ups; scene integration verifies chooser wiring and fixture-only IDs.

Deviations: none recorded; offer count, pool entries and display labels are non-production fixture configuration, while reroll, banish, sets and weighted selection remain out of scope.

Documentation impact: Game Design, Content Design and IP-07 scope did not change; execution status and dependant readiness synchronized.

## Build systems

### IP-08 — Active-skill progression и pattern framework

Status: Verified

Implementation evidence: `Assets/Game/ActiveSkill/Progression/` — six-level definitions (`ActiveSkillLevelDefinition` теперь несёт опциональную `ContentRef<SpriteDefinition> Visual`, см. DECISION-0007), activation-wave composition and typed projectile/beam/orbit/boomerang/chain/area/mine effects; all 8 fixture skills × 6 levels now load from `Resources/Content/ActiveSkills/FixtureActiveSkills.json` via `FixtureActiveSkillCatalog.Create()`, with the 7 polymorphic effect types resolved by a `"kind"`-discriminated `ActiveSkillEffectJsonConverter` (see DECISION-0009) instead of hardcoded ternary logic; `PlayerActiveSkillSetRuntime.cs` — concurrent build-synchronized skills; `SceneActiveSkillEffectExecutor.cs` and `FixtureProjectileRuntime.cs` — pause-safe execution, delayed/multi-wave scheduling, pierce and return passes; `SceneActiveSkillEffectExecutor.TickMines` обёрнут в `PerfGuard` (see DECISION-0008); `Gameplay.unity` — fixture catalog runtime replaces the legacy single-skill fixture.

Verification evidence: Unity 6000.6.0f1 EditMode, 110/110 tests passed on 2026-09-14; 12 IP-08 tests cover exact levels 1→6, numeric and qualitative level changes, fan/ring/cross directions, pierce, boomerang return and per-pass hits, delayed AoE pause, chain retarget/falloff, mine lifetime/concurrent limit, rotated multi-wave execution, concurrent acquired skills, draft upgrades and scene wiring. Visual-reference, perf-log and JSON-config migration (all values hand-transcribed from the prior hardcoded logic) re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above) — the exact-level assertions above caught nothing wrong, confirming the transcription.

Deviations: [DECISION-0007](../decisions/0007-content-referenced-visuals.md) — `ActiveSkillLevelDefinition`/`ActiveSkillProgressionDefinition` gained an optional per-level `Visual` reference validated by `ContentRegistry`; the fixture Ring/Cross skill now declares two distinct visual ids per tier instead of an implicit level ternary (the underlying `Cross`/`Ring` projectile-pattern bug noted in PR review is unrelated and not fixed by this); [DECISION-0008](../decisions/0008-perf-logging.md) and [DECISION-0009](../decisions/0009-json-content-config.md) — perf-logging and JSON-driven content, including the polymorphic-effect converter pattern. All catalog entries and numeric parameters are explicitly `FIXTURE-*`, while SKILL-001…015 remain Draft compatibility targets rather than production content.

Documentation impact: Game Design, Content Design and IP-08 scope did not change; IP-05 public projectile behavior remains backward compatible and dependant readiness was synchronized; DECISION-0007 gives IP-17 a ready presentation-asset extension point, and DECISION-0009 gives it a ready JSON-config + polymorphic-effect pattern, instead of requiring either to be designed from scratch.

### IP-09 — Passive modifier framework

Status: Verified

Implementation evidence: `Assets/Game/Progression/Passive/` defines six-level passive progressions, now loaded via `FixturePassiveCatalog.Create()` from `Resources/Content/Passives/FixturePassives.json` — the former `level * X` formulas are expanded into explicit per-level values in config (see DECISION-0009); `PlayerPassiveSetRuntime.cs` applies keyed build-synchronized modifiers; `CharacterStats.cs` implements additive source percentages, asymptotic cooldown reduction and 99% damage-reduction cap; proportional current-health rescaling lives in `Assets/Game/Combat/Health.cs` (see DECISION-0006) via the `IHealthProfile` `Changed` event.

Verification evidence: Unity 6000.6.0f1 EditMode, 121/121 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes keyed replacement, six levels, multiple stat categories, additive percentages, cooldown asymptote, damage-reduction cap, proportional health and scene-level passive selection. Health file-path change and JSON-config migration re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Deviations: production PASSIVE-001…010 remain out of scope; fixture passives are non-production. Stat semantics are recorded in [DECISION-0004](../decisions/0004-character-stat-composition.md); the health file path was superseded by [DECISION-0006](../decisions/0006-shared-health-model.md); passive values moved to JSON per [DECISION-0009](../decisions/0009-json-content-config.md).

Documentation impact: Game Design, Content Design, IP-09 and direct dependant readiness synchronized.

### IP-10 — Reroll и banish

Status: Verified

Implementation evidence: `DraftRunControls.cs` owns configurable run-local counters and banished stable IDs; `DraftPool.cs` filters banished entries and guarantees a changed reroll offer set when an alternative exists; `LevelUpDraftRuntime.cs` validates actions, rebuilds the open draft and exposes fixture IMGUI controls; `GameplayCompositionRoot.cs` supplies serialized fixture counts.

Verification evidence: Unity 6000.6.0f1 EditMode, 128/128 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes repeated rerolls, alternative offers, invalid and exhausted actions, persistent banish filtering, reset, last-option exhaustion, scene configuration and the live level-up flow.

Deviations: exact counts and recovery remain CG-04 balance TBD; the scene values of 2 rerolls and 2 banishes are explicitly non-production fixture configuration.

Documentation impact: Game Design already defined reroll/banish behavior and remains unchanged; IP-10 edge behavior and checks were clarified; Content Design is unaffected; IP-26 dependency readiness was synchronized.

### IP-10A — UI Foundation and test harness

Status: Verified

Implementation evidence: `Assets/Game/UI/` contains immutable HUD/draft/build/run-overlay states, model/view contracts, a pure presenter, runtime adapter, stable semantic element IDs and UI Toolkit UXML/USS/runtime theme; visible HP/XP bars and 6+6 live build slots surface IP-03/IP-05/IP-06/IP-08/IP-09 state; `GameplayUiRoot.cs` owns the thin Unity lifecycle adapter; `GameplayCompositionRoot.cs` injects gameplay dependencies; gameplay-owned IMGUI was removed from `LevelUpDraftRuntime.cs`.

Verification evidence: Unity 6000.6.0f1 EditMode, 134/134 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes presenter HUD/draft/build snapshots and intents, development-build gating, pause/result state, required UXML semantic elements and runtime theme import, scene wiring, non-zero resolved HP-bar geometry, 6+6 rendered build slots and live HUD/draft/build updates through two level-ups with reroll/banish.

Deviations: development commands use fixed fixture amounts and are visible only in Editor/Development Build; production presentation, navigation and content screens remain IP-26 scope.

Documentation impact: [DECISION-0005](../decisions/0005-vertical-ui-delivery.md) records the user-approved AI-first vertical UI pipeline; `WORKFLOW.md` and every IP specification now define a feature-owned UI/observability contract; IP-26 integrates and completes the UI rather than introducing it for the first time; Game Design and Content Design behavior are unchanged.

### IP-11 — Set framework

Status: Verified

Implementation evidence: `Assets/Game/Progression/Sets/` defines validated active/passive recipe components with minimum levels, probability-gated `SetDefinition`s, unlimited slot-free acquisition and one independently ticked/disposed extra-ability instance per acquired set; `DraftPool` and `PlayerBuild` integrate sets into the unified level-up flow while blocking duplicates and levels; `FixtureSetCatalog` loads two non-production shared-component recipes from `Resources/Content/Sets/FixtureSets.json`; `GameplayUiPresenter`/ViewState/UXML expose acquired sets separately from 6+6 slots and fixture recipe progress with stable semantic IDs; `FixtureRuntimeContentCatalog` validates recipe content references and composes the fixture set runtime.

Verification evidence: Unity 6000.6.0f1 EditMode, 136/136 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Seven new EditMode checks cover unmet and threshold-complete recipes, probability miss/hit, slot-free level-less acquisition, duplicate rejection, shared components across multiple sets, independent extra-ability tick/disposal, missing recipe references and live `LevelUpDraftRuntime` acquisition; UI presenter/assets and gameplay smoke verify separate sets state and fixture recipe-progress observability.

Deviations: `FIXTURE-SET-*` recipes, names and 50% draft chances are explicitly non-production test configuration; fixture extra abilities provide lifecycle/tick observability without claiming a production gameplay effect. Production SET-001…008 recipes, weights and concrete effects remain IP-19 scope and content-gated.

Documentation impact: Game Design, Content Design and IP-11 scope remain unchanged; execution evidence and IP-19 dependency state were synchronized.

### IP-12 — Character framework и weighted draft

Status: Verified

Implementation evidence: `Assets/Game/Progression/Character/` defines validated character definitions, typed starting-build references, per-active-skill draft weights with a default of 1, and a roster that exposes only explicitly unlocked choices; `DraftPool` performs seeded weighted sampling without replacement and removes zero-weight active skills before random selection; `LevelUpDraftRuntime` resolves the selected character's starting active skill through the shared content registry. `FixtureCharacterDefinitionCatalog` loads two non-production profiles from `Resources/Content/Characters/FixtureCharacters.json`; `GameplayCompositionRoot` selects the configured unlocked fixture, applies its base stats and starting loadout, and UI Toolkit exposes unlocked character cards with starting skill and compact stat snapshots.

Verification evidence: Unity 6000.6.0f1 EditMode, 141/141 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Five new EditMode checks cover two distinct fixture profiles, unlocked/locked selection, starting active-slot resolution, reproducible seeded weighted sampling, zero-weight exclusion, character-dependent deterministic outcomes, and passive modifiers layered on the selected base stats. Content-registry, UI presenter/assets, scene wiring and PlayMode smoke additionally verify referenced skills, the configured selected character and unlocked-character observability.

Deviations: `FIXTURE-CHARACTER-AGILE` and `FIXTURE-CHARACTER-STURDY`, their stats, unlock flags and weights are explicitly non-production test configuration. Production CHAR-001…010, production starting SKILL references and persistent unlock state remain IP-22/IP-25 scope and content-gated.

Documentation impact: Game Design and Content Design already define the required character rules and remain unchanged; DECISION-0009 was synchronized with the expanded JSON character shape. IP-22 no longer depends on unfinished framework work (IP-17 and CG-01 still block production characters), and IP-25 now remains blocked only by IP-16 plus its production-economy gate.

## Encounter systems

### IP-13 — Enemy movement и attack patterns

Status: Ready

Implementation evidence: —

Verification evidence: —

### IP-14 — Wave Director

Status: Blocked  
Blocked by: IP-13; production schedules remain CG-02 gated.

### IP-15 — Boss/mid-boss framework

Status: Blocked  
Blocked by: IP-13, IP-14.

### IP-16 — Field definitions и run configuration

Status: Blocked  
Blocked by: IP-14, IP-15.

## Production content

### IP-17 — Production Active Skills

Status: Blocked  
Blocked by: CG-01 approval of target SKILL IDs.  
Groundwork: [DECISION-0007](../decisions/0007-content-referenced-visuals.md) already implements the "missing presentation asset обнаруживается validator" acceptance criterion via `ContentRegistry`/`IReferencesContent`; this IP registers real `SpriteDefinition`s and resolves per-level visuals into projectile/mine rendering instead of designing the mechanism from scratch.

### IP-18 — Production Passive Items

Status: Blocked  
Blocked by: IP-09 and CG-01 approval of target PASSIVE IDs.  
Groundwork: [DECISION-0009](../decisions/0009-json-content-config.md) already loads fixture passives from `Resources/Content/Passives/FixturePassives.json` via `FixturePassiveCatalog`; this IP registers real `PASSIVE-*` entries in that same JSON convention instead of hardcoding them.

### IP-19 — Production Sets

Status: Blocked  
Blocked by: IP-17, IP-18 and CG-01 approval of target SET/component IDs.
Groundwork: IP-11 provides JSON-configured recipes, probabilistic unified-draft eligibility, slot-free acquisition, independent extra-ability lifecycle and set UI/recipe observability; this IP supplies approved production definitions and concrete effects through those extension points.

### IP-20 — Production Enemies

Status: Blocked  
Blocked by: IP-13 and CG-01 approval of target ENEMY IDs.  
Groundwork: `EnemyDefinition.Visual` and the `Game.Presentation` module ([DECISION-0007](../decisions/0007-content-referenced-visuals.md)) already resolve end-to-end into `EnemyRuntime` rendering for the fixture enemy; `FixtureEnemyCatalog`/JSON ([DECISION-0009](../decisions/0009-json-content-config.md)) already loads enemy definitions from config instead of code; this IP only needs to register real `SpriteDefinition`s and `ENEMY-*` entries in those same conventions.

### IP-21 — Production Bosses и Mid-bosses

Status: Blocked  
Blocked by: IP-13, IP-15 and CG-01 approval of target BOSS/MIDBOSS IDs.

### IP-22 — Production Characters

Status: Blocked  
Blocked by: IP-17 and CG-01 approval of target CHAR/starting SKILL IDs.
Groundwork: IP-12 provides JSON-driven character definitions, validated starting-skill references, base stats, disappearing-XP recovery, weighted draft values including zero, unlocked-only selection and fixture UI observability. [DECISION-0006](../decisions/0006-shared-health-model.md) keeps `PlayerCharacterRuntime` health independent of a single stat profile, while [DECISION-0007](../decisions/0007-content-referenced-visuals.md) remains the presentation-reference convention for production character art.

### IP-23 — Production Fields

Status: Blocked  
Blocked by: IP-16, required production content and CG-01 approval of target FIELD IDs.

### IP-24 — Canonical Wave / Encounter Content

Status: Blocked  
Blocked by: IP-14, IP-20, IP-21, IP-23 and CG-02 missing Approved Wave / Encounter Content.

## Meta, UI and integration

### IP-25 — Persistent profile и meta progression

Status: Blocked  
Blocked by: IP-16; production economy remains CG-03 gated. IP-12's `CharacterRoster` accepts an explicit unlocked-ID set ready to be supplied by this profile layer.

### IP-26 — Functional UI и полный player flow

Status: Blocked  
Blocked by: IP-16, IP-25.

### IP-27 — End-to-end integration

Status: Blocked  
Blocked by: all in-scope preceding system modules; content-complete verification also requires Approved production modules.

## Status maintenance rule

После изменения статуса нужно пересчитать готовность прямых dependants. Завершение модуля переводит его в `Implemented`; только успешное выполнение заявленных checks переводит его в `Verified`. Для каждого реализованного модуля обязательны implementation evidence, verification evidence, deviations и documentation impact.
