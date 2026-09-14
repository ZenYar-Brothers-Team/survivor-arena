# Implementation Status

Этот файл — единственный source of truth для состояния исполнения Implementation Plan. Спецификации в `modules/` не содержат оперативных статусов.

Last repository audit: 2026-09-14
Current next module: IP-11

Cross-cutting verification: Unity 6000.6.0f1 EditMode 134/134 and PlayMode 1/1 passed on 2026-09-14. The gameplay scene is composed from one fixture runtime catalog; draft RNG is seeded; pause ownership is reason-based; enemy targeting uses a live registry and reusable query buffers; gameplay UI follows a presenter/ViewState boundary. Field bounds and ordinary obstacles follow [DECISION-0003](../decisions/0003-player-only-field-collision.md); stat composition follows [DECISION-0004](../decisions/0004-character-stat-composition.md); vertical UI delivery follows [DECISION-0005](../decisions/0005-vertical-ui-delivery.md).

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

Implementation evidence: `Assets/Game/Character/` — base/modifier stat model, health model, death-to-run binding и `PlayerCharacterRuntime`; `Assets/Scenes/Gameplay.unity` — runtime подключён к Player как movement speed source.

Verification evidence: Unity 6000.6.0f1 EditMode, 43/43 tests passed on 2026-09-13; 13 IP-03 tests покрывают damage, capped heal, regeneration и pause, death-to-lost, base + keyed modifiers без double counting, stat clamps/removal и Gameplay scene wiring.

Deviations: none recorded; CHAR-001…010 и PASSIVE-001…010 не реализовывались как production content.

Documentation impact: Game Design, Content Design и IP-03 scope не изменились; execution status и readiness зависимых модулей синхронизированы.

### IP-04 — Enemy core

Status: Verified

Implementation evidence: `Assets/Game/Enemy/` — content-compatible definition, health, seek movement, contact damage, lifecycle/factory и continuous fixture spawner; `Assets/Scenes/Gameplay.unity` — configured `EnemySpawner`.

Verification evidence: Unity 6000.6.0f1 EditMode, 61/61 tests passed on 2026-09-13; 18 IP-04 tests покрывают definition/registry, health/death, seek/stop simulation, immediate/repeated contact damage, sustained-contact death, pause-safe contact/spawn timers, runtime spawn/despawn и Gameplay scene wiring.

Deviations: [DECISION-0002](../decisions/0002-repeated-contact-damage.md) — утверждены повторные тики для длительного contact damage; scene использует только `FIXTURE-ENEMY-SEEKER`, ENEMY-001…020 не реализовывались как production content.

Documentation impact: Game Design, Content Design enemy schema, IP-04 scope/checks и DECISION-0002 синхронизированы; exact production intervals остаются TBD.

### IP-05 — Active skill runtime и player damage pipeline

Status: Verified

Implementation evidence: `Assets/Game/ActiveSkill/` — content-compatible definition, automatic cooldown/target runtime, nearest-target hook, projectile and AoE primitives; `Assets/Game/Enemy/Model/IEnemyDamageReceiver.cs` and `EnemyDamageRequest.cs` — единый контракт урона врагам; `Assets/Scenes/Gameplay.unity` — configured `FIXTURE-SKILL-BOLT` на Player.

Verification evidence: Unity 6000.6.0f1 EditMode, 81/81 tests passed on 2026-09-14; 20 IP-05 tests покрывают definition/registry, cooldown и projectile lifetime, nearest-target hook, automatic trigger без attack input, character damage/cooldown multipliers, projectile hit/kill через enemy damage contract, AoE без повторного урона одной цели, pause/end и Gameplay scene wiring.

Deviations: none recorded; scene использует только `FIXTURE-SKILL-BOLT`, SKILL-001…015 не реализовывались как production content.

Documentation impact: Game Design, Content Design и IP-05 scope не изменились; execution status и readiness зависимых модулей синхронизированы.

### IP-06 — XP drops, pickup, expiry и level progression

Status: Verified

Implementation evidence: `Assets/Game/Progression/` — progression/bar model, configurable thresholds, pause-safe physical drop runtime, pickup/expiry/recovery pipeline and level-up event; `EnemyDefinition.ExperienceReward` and `EnemyRuntime` — XP spawn at death position; `CharacterStats` — recovery, pickup multiplier and drop-lifetime hooks; `Assets/Scenes/Gameplay.unity` — configured fixture runtime.

Verification evidence: Unity 6000.6.0f1 EditMode, 90/90 tests passed on 2026-09-14; 9 `Game.Progression.Tests` tests cover death drop position/reward, pickup, expiry with base 0% recovery, recovery without double award, configurable lifetime/thresholds, XP bar state, multiple levels, level-up pause/event and pause-safe drop behavior.

Deviations: none recorded; fixture thresholds and reward are non-production configuration, the final XP curve remains out of scope.

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

Implementation evidence: `Assets/Game/ActiveSkill/Progression/` — six-level definitions, activation-wave composition and typed projectile/beam/orbit/boomerang/chain/area/mine effects; `PlayerActiveSkillSetRuntime.cs` — concurrent build-synchronized skills; `SceneActiveSkillEffectExecutor.cs` and `FixtureProjectileRuntime.cs` — pause-safe execution, delayed/multi-wave scheduling, pierce and return passes; `Gameplay.unity` — fixture catalog runtime replaces the legacy single-skill fixture.

Verification evidence: Unity 6000.6.0f1 EditMode, 110/110 tests passed on 2026-09-14; 12 IP-08 tests cover exact levels 1→6, numeric and qualitative level changes, fan/ring/cross directions, pierce, boomerang return and per-pass hits, delayed AoE pause, chain retarget/falloff, mine lifetime/concurrent limit, rotated multi-wave execution, concurrent acquired skills, draft upgrades and scene wiring.

Deviations: none recorded; all catalog entries and numeric parameters are explicitly `FIXTURE-*`, while SKILL-001…015 remain Draft compatibility targets rather than production content.

Documentation impact: Game Design, Content Design and IP-08 scope did not change; IP-05 public projectile behavior remains backward compatible and dependant readiness was synchronized.

### IP-09 — Passive modifier framework

Status: Verified

Implementation evidence: `Assets/Game/Progression/Passive/` defines six-level passive progressions and fixture catalog; `PlayerPassiveSetRuntime.cs` applies keyed build-synchronized modifiers; `CharacterStats.cs` and `CharacterHealth.cs` implement additive source percentages, asymptotic cooldown reduction, 99% damage-reduction cap and proportional current-health rescaling.

Verification evidence: Unity 6000.6.0f1 EditMode, 121/121 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes keyed replacement, six levels, multiple stat categories, additive percentages, cooldown asymptote, damage-reduction cap, proportional health and scene-level passive selection.

Deviations: production PASSIVE-001…010 remain out of scope; fixture passives are non-production. Stat semantics are recorded in [DECISION-0004](../decisions/0004-character-stat-composition.md).

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

Status: Ready

### IP-12 — Character framework и weighted draft

Status: Ready

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

### IP-18 — Production Passive Items

Status: Blocked  
Blocked by: IP-09 and CG-01 approval of target PASSIVE IDs.

### IP-19 — Production Sets

Status: Blocked  
Blocked by: IP-11, IP-17, IP-18 and CG-01 approval of target SET/component IDs.

### IP-20 — Production Enemies

Status: Blocked  
Blocked by: IP-13 and CG-01 approval of target ENEMY IDs.

### IP-21 — Production Bosses и Mid-bosses

Status: Blocked  
Blocked by: IP-13, IP-15 and CG-01 approval of target BOSS/MIDBOSS IDs.

### IP-22 — Production Characters

Status: Blocked  
Blocked by: IP-12, IP-17 and CG-01 approval of target CHAR/starting SKILL IDs.

### IP-23 — Production Fields

Status: Blocked  
Blocked by: IP-16, required production content and CG-01 approval of target FIELD IDs.

### IP-24 — Canonical Wave / Encounter Content

Status: Blocked  
Blocked by: IP-14, IP-20, IP-21, IP-23 and CG-02 missing Approved Wave / Encounter Content.

## Meta, UI and integration

### IP-25 — Persistent profile и meta progression

Status: Blocked  
Blocked by: IP-12, IP-16; production economy remains CG-03 gated.

### IP-26 — Functional UI и полный player flow

Status: Blocked  
Blocked by: IP-16, IP-25.

### IP-27 — End-to-end integration

Status: Blocked  
Blocked by: all in-scope preceding system modules; content-complete verification also requires Approved production modules.

## Status maintenance rule

После изменения статуса нужно пересчитать готовность прямых dependants. Завершение модуля переводит его в `Implemented`; только успешное выполнение заявленных checks переводит его в `Verified`. Для каждого реализованного модуля обязательны implementation evidence, verification evidence, deviations и documentation impact.
