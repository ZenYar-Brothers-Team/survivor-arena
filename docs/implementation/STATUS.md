# Implementation Status

Единственный источник execution status, Execution order и evidence. Спецификации модулей не содержат оперативных статусов.

Last repository audit: 2026-09-21
Plan revision: design-sync-R2
Current active module: none (user-requested pause after IP-07)
Next Ready module: IP-08 (not started; user-requested pause after IP-07)

M-01: зарегистрирован принятый план и выполнена полная замена трёх design bodies без архивных копий старых документов; [DECISION-0015](../decisions/0015-design-sync-r2.md). Код не изменён. Исторические tests не подтверждают новые требования. Все пять источников/121 target card approved; реальные missing data/semantics/assets gates сохраняются.

## Проверка регистрации M-01

Documentation verification: три canonical bodies совпадают с нормализованными approved sources; все 121 target cards сохранены. Проверены 35 полных спецификаций, 160 dependency edges, обратные связи и соответствие Dependencies в STATUS. Очередь включает каждый IP ровно один раз, циклов нет; следующий Ready — IP-01. Ссылки и whitespace проверены. Runtime/config/assets/test files не изменены; Unity tests для этой документационной миграции не запускались. Отдельные копии старых трёх документов не созданы.

## Пауза по просьбе пользователя

IP-01, IP-03…IP-07 проверены для design-sync-R2; IP-00/IP-02 сохранены. Последняя проверка 2026-09-21: 343/343 Game.* EditMode, 1/1 PlayMode, 0 skipped (Unity 6000.6.0f1). G-01/G-03 draft semantics закрыты DECISION-0019/0020: uniform set backfill, общая очередь, только пустая при подборе Книга немедленно начисляет валюту. Production сумма и pickup content не объявлены готовыми.

Пользователь попросил завершить текущий IP-07 и сделать паузу. IP-07 завершён; IP-08 Ready, но не начат. Продолжение требует команды пользователя. IP-31 не начат; прежняя граница до IP-31 сохраняется для будущего продолжения, если пользователь её не изменит.

## Execution order

Выбирать первый Ready в этой таблице, если пользователь не назвал IP. Проверять prerequisites целевой ревизии и текущий packet. Таблица задаёт очередь; текущие статусы — в записях ниже.

| Приоритет | Модуль | Назначение |
|---:|---|---|
| 1 | [IP-00](modules/IP-00-content-contract.md) | Контракт контента, стабильные ID и конфигурация |
| 2 | [IP-01](modules/IP-01-run-lifecycle.md) | Run lifecycle, pause ownership и результат забега |
| 3 | [IP-02](modules/IP-02-player-movement.md) | Перемещение игрока, камера и базовая геометрия |
| 4 | [IP-03](modules/IP-03-character-stats.md) | Character stats, Health и новые stat channels |
| 5 | [IP-04](modules/IP-04-enemy-core.md) | Enemy lifecycle, contact damage и per-life identity |
| 6 | [IP-05](modules/IP-05-active-skill-runtime.md) | Общий combat pipeline, control effects и target contract |
| 7 | [IP-06](modules/IP-06-xp-progression.md) | XP lifecycle, effective pickup radius и progression |
| 8 | [IP-07](modules/IP-07-level-up-draft.md) | Трёхслотовый драфт, request queue и build progression |
| 9 | [IP-08](modules/IP-08-active-skill-framework.md) | Active-skill levels, targeting и effect families |
| 10 | [IP-09](modules/IP-09-passive-framework.md) | Passive modifiers и новые stat effects |
| 11 | [IP-10](modules/IP-10-reroll-banish.md) | Reroll/banish для обновлённого драфта |
| 12 | [IP-10A](modules/IP-10A-ui-foundation.md) | UI Foundation, reusable cards, HUD и test harness |
| 13 | [IP-31](modules/IP-31-manual-run-telemetry.md) | Локальная телеметрия ручных прогонов |
| 14 | [IP-32](modules/IP-32-manual-ai-balance.md) | Ручные прогоны и AI-assisted balance review |
| 15 | [IP-11](modules/IP-11-set-framework.md) | Set recipes, priority draft policy и effect families |
| 16 | [IP-12](modules/IP-12-character-framework.md) | Character definitions, weighted draft и selection presentation |
| 17 | [IP-12A](modules/IP-12A-visual-presentation-foundation.md) | Visual Presentation Foundation и asset production pipeline |
| 18 | [IP-13](modules/IP-13-enemy-patterns.md) | Enemy movement/attack patterns и control integration |
| 19 | [IP-14](modules/IP-14-wave-director.md) | Wave Director: continuous и burst timeline |
| 20 | [IP-15](modules/IP-15-boss-framework.md) | Boss/mid-boss encounter framework |
| 21 | [IP-16](modules/IP-16-field-framework.md) | Field definitions, selection и run configuration |
| 22 | [IP-28](modules/IP-28-world-pickups.md) | World pickup framework: зелье лечения и Book |
| 23 | [IP-29](modules/IP-29-traveler-framework.md) | Traveler encounter framework |
| 24 | [IP-25](modules/IP-25-meta-progression.md) | Persistent profile, meta currency, unlocks и permanent progression |
| 25 | [IP-26](modules/IP-26-functional-ui.md) | Functional UI и полный player flow |
| 26 | [IP-17](modules/IP-17-production-skills.md) | Production Active Skills SKILL-001…016 |
| 27 | [IP-18](modules/IP-18-production-passives.md) | Production Passive Items PASSIVE-001…014 |
| 28 | [IP-19](modules/IP-19-production-sets.md) | Production Sets SET-001…020 |
| 29 | [IP-20](modules/IP-20-production-enemies.md) | Production Enemies ENEMY-001…020 и зелье PICKUP-001 |
| 30 | [IP-21](modules/IP-21-production-bosses.md) | Production Final Bosses и Mid-bosses |
| 31 | [IP-22](modules/IP-22-production-characters.md) | Production Characters CHAR-001…010 |
| 32 | [IP-23](modules/IP-23-production-fields.md) | Production Fields FIELD-001…010 |
| 33 | [IP-30](modules/IP-30-production-travelers.md) | Production Travelers TRAVELER-001…010 и Book |
| 34 | [IP-24](modules/IP-24-production-waves.md) | Canonical Wave / Encounter Content и field bindings |
| 35 | [IP-27](modules/IP-27-integration.md) | End-to-end integration, regression и content validation |

## Scope revisions и готовность

IP-00/IP-02 сохраняют Verified: их behavioral acceptance не изменён, API текущего кода совместим; новые RunOutcome/control/presentation deltas проверяют их владельцы. При последующей несовместимой правке пересмотреть affected evidence.

IP-01 и изменённая основа IP-03…IP-14/IP-10A/IP-12A требуют новых дельт. Их прежнее Verified записано только как historical scope status. Все dependencies ниже относятся к `design-sync-R2`, если явно не указано иначе. IP-15 больше не Ready по старому IP-14 continuous evidence.

Для catalog packets выполненные ID и remaining scope ведутся здесь; pilot не переводит весь IP в Implemented/Verified. CG-01 approval получен; CG-02/03/04 и [G/W gaps](DESIGN_SYNC.md) учитываются только для зависящего packet. Не требуется повторно утверждать принятые designs.

## Модули

### IP-00 — Контракт контента, стабильные ID и конфигурация

Scope revision: design-sync-R2
Status: Verified
Dependencies: none
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: Существующая реализация сохранена; M-01 изменяет только ссылки/Context, без кода.
Target verification evidence: Сохранённое evidence ниже применимо к неизменному behavioral scope; в M-01 проверены только документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified
Historical implementation evidence: commit `bb726e6`; `Assets/Game/Content/Model/ContentRegistry.cs`, `ContentId.cs` и связанные content contracts.
Historical verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; `ContentRegistryTests.cs` покрывает duplicate/missing/wrong-type/invalid IDs, invalid definitions/references, обязательный Build и загрузку configured fixture set.
Historical deviations: none recorded.
Historical documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

### IP-01 — Run lifecycle, pause ownership и результат забега

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-00
Current packet: Run identity, terminal snapshot/RunOutcome, reset/teardown contract; выполнить новый Scope без переписывания готового таймера/pause.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: none.
Target implementation evidence: RunModel RunId/immutable RunOutcome, contributor capture/unavailable/failure contract, Stop reasons; RunController Shutdown/reinit; elapsed HUD. Контракт и границы Retry записаны в IP-01.
Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 257/257 и PlayMode 1/1 passed (TestResults/EditMode.xml, PlayMode.xml); 9 новых lifecycle/outcome tests, elapsed presenter assertion и terminal HUD smoke. Сторонние пакеты исключены фильтром.
Documentation impact: IP-01 terminal/time/teardown contract; GDD/CD rules unchanged.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified
Historical implementation evidence: commit `bb726e6`; `Assets/Game/Run/Model/RunModel.cs` и `RunController.cs`.
Historical verification evidence: Unity 6000.6.0f1 EditMode, 29/29 tests passed on 2026-09-13; все 7 `RunModelTests.cs` passed.
Historical deviations: none recorded.
Historical documentation impact: Game Design и Content Design не затронуты; verification status synchronized.

### IP-02 — Перемещение игрока, камера и базовая геометрия

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-01
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: Существующая реализация сохранена; M-01 изменяет только ссылки/Context, без кода.
Target verification evidence: Сохранённое evidence ниже применимо к неизменному behavioral scope; в M-01 проверены только документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: commit `9223059`; `Assets/Game/Movement/Presenters/CameraFollowTarget.cs` и `Assets/Scenes/Gameplay.unity`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 30/30 tests passed on 2026-09-13; `Camera_FollowsPlayerAtViewportCenterAndPreservesDepth` проверяет wiring, центр viewport после смещения Player и сохранение camera depth.

Historical deviations: [DECISION-0001](../decisions/0001-player-centered-camera.md) — утверждено правило центрирования камеры.

Historical documentation impact: Game Design и IP-02 синхронизированы; Content Design не затронут, так как изменение не содержит content entities или balance-data.

### IP-03 — Character stats, Health и новые stat channels

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-01, IP-02
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Remaining gates: G-08/G-09 закрыты DECISION-0017. IP-05 фиксирует damage при активации; parameter mapping реализует IP-08.
Remaining acceptance / IDs: none; G-08/G-09 remain gates of consuming IPs.
Target implementation evidence: CharacterStats/Modifier/BaseStats новые каналы, CharacterHealthStatBinding, required CharacterBaseStatsMapper, actionSpeed JSON/API migration, immutable HUD stats + DEV scroll observation.
Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 283/283 и PlayMode 1/1 passed; stacking/replacement/removal, low-HP 100/55/10/5%, heal/max-HP/no recursion, overflow rollback, 17 missing-field cases, mapping и HUD smoke.
Documentation impact: stat/units/JSON dictionary в IP-03, terminology в DECISION-0004; GDD/CD formulas unchanged; applicability оставлена G-08/G-09.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Character/` — base/modifier stat model, death-to-run binding и `PlayerCharacterRuntime`; health model живёт в `Assets/Game/Combat/Health.cs` (см. DECISION-0006) и используется через `IHealthProfile`, который реализует `CharacterStats`; base stats больше не `[SerializeField]`-поля — грузятся через `FixtureCharacterCatalog.CreateDefault()` из `Resources/Content/Characters/FixtureCharacters.json` (см. DECISION-0009); `Assets/Scenes/Gameplay.unity` — runtime подключён к Player как movement speed source.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 43/43 tests passed on 2026-09-13; 13 IP-03 tests покрывают damage, capped heal, regeneration и pause, death-to-lost, base + keyed modifiers без double counting, stat clamps/removal и Gameplay scene wiring. Health model и JSON-конфиг base stats re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above); scene не содержала кастомных значений в удалённых полях — потери данных нет.

Historical deviations: [DECISION-0006](../decisions/0006-shared-health-model.md) — health model вынесен из `Game.Character` в общий `Game.Combat` для переиспользования будущими не-enemy юнитами; [DECISION-0009](../decisions/0009-json-content-config.md) — base stats вынесены из инспектора в JSON; CHAR-001…010 и PASSIVE-001…010 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design и IP-03 scope не изменились; execution status и readiness зависимых модулей синхронизированы; evidence path для health model и base stats обновлён.

### IP-04 — Enemy lifecycle, contact damage и per-life identity

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-02, IP-03
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: EnemyLifeEvent/LifeId/category/reasons/source/position, IEnemyLifeTarget/IEnemyLifecycleSink; pool-safe teardown/reinit, EnemyExperienceDropSink composition, ordinary-enemy RunOutcome contribution, reentrant Health/AoE fixes.
Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 289/289 и PlayMode 1/1 passed; pool identity/payload/source retention, cleanup/escape, live reinit, synchronous lethal callbacks, nested AoE, XP regression и terminal HUD/outcome. PlayMode повторён после исправления smoke timing и test assembly references.
Documentation impact: IP-04 lifecycle contract; DECISION-0016 Proposed (architecture review), GDD/CD rules unchanged; TD-001/003 mitigation documented without rewriting debt register.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/` — content-compatible definition (теперь с опциональной `Visual`-ссылкой, см. DECISION-0007), seek movement, contact damage, lifecycle/factory и continuous fixture spawner; health переиспользует `Game.Combat.Health` через `FixedHealthProfile` (см. DECISION-0006); enemy definitions грузятся через `FixtureEnemyCatalog.Create()` из `Resources/Content/Enemies/FixtureEnemies.json` вместо C#-литералов (см. DECISION-0009); `EnemyRegistry.TryFindNearest` обёрнут в `PerfGuard` (см. DECISION-0008); `Assets/Scenes/Gameplay.unity` — configured `EnemySpawner`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 61/61 tests passed on 2026-09-13; 18 IP-04 tests покрывают definition/registry, health/death, seek/stop simulation, immediate/repeated contact damage, sustained-contact death, pause-safe contact/spawn timers, runtime spawn/despawn и Gameplay scene wiring. Health/visual/JSON-config/perf-log refactors re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: [DECISION-0002](../decisions/0002-repeated-contact-damage.md) — утверждены повторные тики для длительного contact damage; [DECISION-0006](../decisions/0006-shared-health-model.md) — `EnemyHealth` заменён на общий `Health`+`FixedHealthProfile`; [DECISION-0007](../decisions/0007-content-referenced-visuals.md) — `EnemyDefinition` получил опциональную ссылку на визуал, резолвится и рендерится через `EnemyRuntime`; [DECISION-0008](../decisions/0008-perf-logging.md) и [DECISION-0009](../decisions/0009-json-content-config.md) — perf-логирование и JSON-конфиг; scene использует только `FIXTURE-ENEMY-SEEKER`, ENEMY-001…020 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design enemy schema, IP-04 scope/checks и DECISION-0002/0006/0007/0008/0009 синхронизированы; exact production intervals остаются TBD; DECISION-0007 и DECISION-0009 закладывают то, что потребует IP-20 (production enemies) для presentation asset и content config.

### IP-05 — Общий combat pipeline, control effects и target contract

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-03, IP-04
Current packet: Unified combat attribution/results, movement-only slow, additive knockback и общий target-query contract.
Remaining gates: G-06…G-09 resolved by DECISION-0017; production control tuning belongs to later content packets.
Remaining acceptance / IDs: none.
Target implementation evidence: CombatDamageRequest/CombatResult/HealthChange; source/target life+run+content snapshots до lethal callbacks; activation-time low-HP damage; CombatControlState и JSON profiles; PlayerMover/EnemyRuntime добавляют knockback к movement/dash. ICombatTargetQuery/SceneCombatTargetQuery работает с IEnemyLifeTarget, category filters и fake Boss/Traveler; EnemyTargetLife защищает delayed target от pool reuse. Enemy/projectile/contact и 7 active-skill effect families используют общий boundary. DEV показывает controls.
Target verification evidence: Unity 6000.6.0f1, 2026-09-20: Game.* EditMode 311/311, PlayMode 1/1, skipped 0; без third-party suites. Overkill/actual healing, zero-damage controls, resistance, slow refresh/expiry/overlap, movement/dash clocks, attack cadence, pause/stop, walls/no stored distance, lethal→pool identity, delayed source/low-HP snapshot, all 7 effect paths, generic category query и composed PlayerMover smoke.
Documentation impact: DECISION-0017 approved; GDD combat, PASSIVE-014, DESIGN_SYNC, proposal и affected IP gates синхронизированы. Size/range mapping остаётся реализацией IP-08, set propagation — IP-11.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: the original single-skill definition/runtime (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was superseded in place by IP-08's leveled `ActiveSkillProgressionDefinition`/`PlayerActiveSkillSetRuntime` and, once confirmed unused by `GameplayCompositionRoot`, deleted as dead code on 2026-09-15 (own tests included) rather than kept as a parallel implementation; `Assets/Game/Enemy/Model/IEnemyDamageReceiver.cs` and `EnemyDamageRequest.cs` — единый контракт урона врагам, unaffected by the cleanup; `Assets/Scenes/Gameplay.unity` — configured `FIXTURE-SKILL-BOLT` на Player.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 81/81 tests passed on 2026-09-14, at the time this module's own single-skill implementation still existed; its acceptance criteria (automatic trigger without attack input, damage/cooldown multipliers, projectile hit/kill via the enemy damage contract, pause/end behavior) are now exercised by IP-08's implementation instead — see IP-08 evidence, and the full suite re-verified 131/131 on 2026-09-15 (see cross-cutting verification above) after the dead code's removal.

Historical deviations: none recorded; scene использует только `FIXTURE-SKILL-BOLT`, SKILL-001…015 не реализовывались как production content.

Historical documentation impact: Game Design, Content Design и IP-05 scope не изменились; execution status и readiness зависимых модулей синхронизированы; `ActiveSkillLevelDefinition` позже (IP-08 evidence) получил опциональную `Visual`-ссылку, см. [DECISION-0007](../decisions/0007-content-referenced-visuals.md).

### IP-06 — XP lifecycle, effective pickup radius и progression

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-04, IP-05
Current packet: Effective XP radius, source/drop identities, producer events и separate base/awarded lifetime totals.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: Existing/new drops читают effective radius; sprite/collider size не меняет world-unit pickup distance. ExperienceDropIdentity и ExperienceAwardEvent сохраняют drop/run/source life/content, base/awarded и Collected/Expired/DevelopmentIntervention origin; death sink deduplicates life. PlayerExperienceRuntime contributor `experience` пишет immutable RunExperienceSnapshot отдельно от current-level remainder; atomic XP advance, pool/producer Shutdown и DEV observation. Curve/lifetime JSON сохранены.
Target verification evidence: Unity 6000.6.0f1, 2026-09-20: Game.* EditMode 320/320, PlayMode 1/1, skipped 0, без third-party suites. Radius before/after spawn и removal, world scale, death source/dedup, pool life reset, pickup/recovery multipliers и recovery0, no double award, multiple thresholds/terminal snapshot, shutdown/reinit/unregister, terminal/pause freeze, intervention origin и real-scene pickup/HUD. Повторно проверена IP-05 regression suite.
Documentation impact: IP-06 units/producer contract и fixture rationale; DECISION-0018 Proposed для архитектурного ревью реализации принятого scope. Product formulas PASSIVE-006/007/010 не изменены; G-01/G-03 позднее закрыты DECISION-0019/0020 в IP-07. IP-07 добавил atomic LevelsEarned range перед legacy per-level events, чтобы одна XP награда ставила requests подряд.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/` — progression/bar model, configurable thresholds, pause-safe physical drop runtime, pickup/expiry/recovery pipeline and level-up event; `EnemyDefinition.ExperienceReward` and `EnemyRuntime` — XP spawn at death position; `CharacterStats` — recovery, pickup multiplier and drop-lifetime hooks; `Assets/Scenes/Gameplay.unity` — configured fixture runtime.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 90/90 tests passed on 2026-09-14; 9 `Game.Progression.Tests` tests cover death drop position/reward, pickup, expiry with base 0% recovery, recovery without double award, configurable lifetime/thresholds, XP bar state, multiple levels, level-up pause/event and pause-safe drop behavior. `Initialize` rename re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: none recorded; fixture thresholds and reward are non-production configuration, the final XP curve remains out of scope. `PlayerExperienceRuntime.ConfigureForTests` was renamed to `Initialize` (guarded, throws if already initialized) to match every sibling runtime's composition-root wiring convention instead of exposing a test-only entry point; `GameplayCompositionRoot` now calls it directly and the 5 existing test call sites were updated in place — no behavior change.

Historical documentation impact: Game Design, Content Design and IP-06 scope did not change; execution status and direct dependant readiness synchronized.

### IP-07 — Трёхслотовый драфт, request queue и build progression

Scope revision: design-sync-R2
Status: Verified
Dependencies: IP-01, IP-06
Current packet: Целевой fixture framework завершён; общая очередь/preview/revisions и immediate empty-Book currency по DECISION-0019/0020.
Remaining gates: Нет для IP-07. Production Book ID/сумма/lifetime — IP-28/IP-30/IP-25; global set chance/order/reroll policy G-02 — IP-10/IP-11.
Remaining acceptance / IDs: Нет для принятого scope IP-07.
Target implementation evidence: `Progression/Draft` — immutable request/origin/revision/preview, eligibility + set provider + uniform backfill; `LevelUpDraftRuntime` — FIFO, atomic earned-level batch, stale/double intent rejection, pause ownership, terminal/shutdown cancellation, pickup dedup и immediate currency. `RunDraftSnapshot`/contributor `draft` сохраняет выбранный build, counters и BookCurrency независимо от IP-31. Config `draft.emptyBookCurrency` обязателен в fixture JSON. UI — три позиции/disabled short slots, current→next values, Book heading/accent, next request, DEV fixture Book и HUD currency.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1, `scripts/Test-Unity.ps1` после проверки отсутствия открытого Editor: 343/343 Game.* EditMode, 1/1 PlayMode, 0 skipped. Новые checks покрывают FIFO/batch/Book ordering, old revision/session, short/empty/only-set failed roll, равные интервалы RNG и отсутствие повторов, banish/acquired filters, immediate/dedup/old-run Book currency, ordinary empty и поздний empty без валюты, terminal callbacks/outcome, manual pause/shutdown, pure active/passive previews, presenter/реальный short UXML и PlayMode Book→LevelUp sequence. Результаты: `TestResults/EditMode.xml`, `TestResults/PlayMode.xml`.
Documentation impact: GDD XP/Book/meta rules, UI §§7/12, approved DECISION-0019/0020, DESIGN_SYNC, proposal и consumer IP-10/IP-10A/IP-11/IP-25/IP-28 синхронизированы. Fixture currency = 1 не утверждает production баланс. Legacy per-set fixture chance изолирован адаптером до IP-11; production provider policy не объявлена реализованной.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Draft/` — stable-ID active/passive definitions, immutable 6+6 slot ownership, levels 1…6, unified eligibility pool and validated draft session; `LevelUpDraftRuntime.cs` — starting active slot, queued level-up drafts, UI-owned chooser state, resume-after-selection and iterative consumption of pending drafts when no eligible acquisition/upgrade remains; `Assets/Scenes/Gameplay.unity` — configured non-production fixture pool.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 98/98 tests passed on 2026-09-14; 8 IP-07 tests cover starting slot, filling 6 active + 6 passive slots, 1→6 upgrades, full-slot and max-level filtering, duplicate/invalid selection rejection, unified pool, pause/resume and queued drafts for multiple level-ups; scene integration verifies chooser wiring and fixture-only IDs. Exhausted full/maxed build and repeated no-option level-ups were re-verified in the full Unity 6000.6.0f1 suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: none recorded; offer count, pool entries and display labels are non-production fixture configuration, while reroll, banish, sets and weighted selection remain out of scope.

Historical documentation impact: Game Design and IP-07 now explicitly record the user-approved no-options rule; Content Design is unaffected because no entity or balance data changed.

### IP-08 — Active-skill levels, targeting и effect families

Scope revision: design-sync-R2
Status: Ready
Dependencies: IP-05, IP-07
Current packet: Framework по принятому map G-08/G-09; prerequisites IP-05/IP-07 проверены. G-04 разрешается в affected production/set packet без выдуманного return у SKILL-008. Не начат: пользователь попросил паузу после IP-07.
Remaining gates: G-08/G-09 resolved by DECISION-0017. G-04 return disc уточняется только если выбранное решение меняет SKILL-008; не выдумывать return внутри framework.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-08-active-skill-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/ActiveSkill/Progression/` — six-level definitions (`ActiveSkillLevelDefinition` теперь несёт опциональную `ContentRef<SpriteDefinition> Visual`, см. DECISION-0007), activation-wave composition and typed projectile/beam/orbit/boomerang/chain/area/mine effects; `ProjectileDirectionGenerator` defines `Cross` as exactly four diagonal rays, distinct from arbitrary-count `Ring`; all 8 fixture skills × 6 levels now load from `Resources/Content/ActiveSkills/FixtureActiveSkills.json` via `FixtureActiveSkillCatalog.Create()`, with the 7 polymorphic effect types resolved by a `"kind"`-discriminated `ActiveSkillEffectJsonConverter` (see DECISION-0009) instead of hardcoded ternary logic; `PlayerActiveSkillSetRuntime.cs` — concurrent build-synchronized skills; `SceneActiveSkillEffectExecutor.cs` and `FixtureProjectileRuntime.cs` — pause-safe execution, delayed/multi-wave scheduling, pierce and return passes; `SceneActiveSkillEffectExecutor.TickMines` обёрнут в `PerfGuard` (see DECISION-0008); `Gameplay.unity` — fixture catalog runtime replaces the legacy single-skill fixture.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 110/110 tests passed on 2026-09-14; 12 IP-08 tests cover exact levels 1→6, numeric and qualitative level changes, fan/ring/cross directions, pierce, boomerang return and per-pass hits, delayed AoE pause, chain retarget/falloff, mine lifetime/concurrent limit, rotated multi-wave execution, concurrent acquired skills, draft upgrades and scene wiring. Visual-reference, perf-log and JSON-config migration (all values hand-transcribed from the prior hardcoded logic) re-verified against the full suite (131/131, 2026-09-15). The diagonal four-ray Cross, its distinction from Ring and rejection of invalid Cross counts were verified in the full Unity suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: [DECISION-0007](../decisions/0007-content-referenced-visuals.md) — `ActiveSkillLevelDefinition`/`ActiveSkillProgressionDefinition` gained an optional per-level `Visual` reference validated by `ContentRegistry`; the fixture Ring/Cross skill declares two distinct visual ids per tier. [DECISION-0008](../decisions/0008-perf-logging.md) and [DECISION-0009](../decisions/0009-json-content-config.md) cover perf-logging and JSON-driven content, including the polymorphic-effect converter pattern. All catalog entries and numeric parameters are explicitly `FIXTURE-*`, while SKILL-001…015 remain Draft compatibility targets rather than production content.

Historical documentation impact: IP-08 now defines the diagonal four-ray Cross contract; Game Design and Content Design are unchanged because this distinguishes a framework layout and fixture content without changing production entities.

### IP-09 — Passive modifiers и новые stat effects

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-03, IP-06, IP-07, IP-08
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope).
Remaining gates: G-08/G-09 resolved by DECISION-0017; potion cap G-10 относится к actual roll IP-28. Утверждённые formulas и final level values не пересогласуются.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-09-passive-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Passive/` defines six-level passive progressions, now loaded via `FixturePassiveCatalog.Create()` from `Resources/Content/Passives/FixturePassives.json` — the former `level * X` formulas are expanded into explicit per-level values in config (see DECISION-0009); `PlayerPassiveSetRuntime.cs` applies keyed build-synchronized modifiers; `CharacterStats.cs` implements additive source percentages, asymptotic cooldown reduction and 99% damage-reduction cap; proportional current-health rescaling lives in `Assets/Game/Combat/Health.cs` (see DECISION-0006) via the `IHealthProfile` `Changed` event.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 121/121 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes keyed replacement, six levels, multiple stat categories, additive percentages, cooldown asymptote, damage-reduction cap, proportional health and scene-level passive selection. Health file-path change and JSON-config migration re-verified against the full suite (131/131, 2026-09-15, see cross-cutting verification above).

Historical deviations: production PASSIVE-001…010 remain out of scope; fixture passives are non-production. Stat semantics are recorded in [DECISION-0004](../decisions/0004-character-stat-composition.md); the health file path was superseded by [DECISION-0006](../decisions/0006-shared-health-model.md); passive values moved to JSON per [DECISION-0009](../decisions/0009-json-content-config.md).

Historical documentation impact: Game Design, Content Design, IP-09 and direct dependant readiness synchronized.

### IP-10 — Reroll/banish для обновлённого драфта

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-07
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Remaining gates: G-02: reroll заново бросает set checks или сохраняет; set banish semantics. G-03 Book ordinary pool/shared counters/immediate empty currency закрыт DECISION-0020. Численные counters остаются CG-04.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-10-reroll-banish.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `DraftRunControls.cs` owns configurable run-local counters and banished stable IDs; `DraftPool.cs` filters banished entries and guarantees a changed reroll offer set when an alternative exists; `LevelUpDraftRuntime.cs` validates actions, rebuilds the open draft, resolves successful reroll/banish exhaustion without constructing an empty `DraftSession`, and drains consecutive unavailable pending drafts iteratively; `GameplayCompositionRoot.cs` supplies serialized fixture counts.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 128/128 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes repeated rerolls, alternative offers, invalid and exhausted actions, persistent banish filtering, reset, last-option exhaustion, scene configuration and the live level-up flow. Reroll-to-empty, banish-last followed by another level-up, full/maxed 6+6 exhaustion and pause release were re-verified in the full Unity suite, EditMode 169/169 and PlayMode 1/1 passed on 2026-09-17.

Historical deviations: exact counts and recovery remain CG-04 balance TBD; the scene values of 2 rerolls and 2 banishes are explicitly non-production fixture configuration.

Historical documentation impact: Game Design, IP-07 and IP-10 now explicitly define no-options resolution; Content Design is unaffected; IP-26 dependency readiness remains unchanged.

### IP-10A — UI Foundation, reusable cards, HUD и test harness

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-01, IP-03, IP-06, IP-07, IP-10
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-10 (Blocked, target scope).
Remaining gates: G-01/G-03 short/book states определены DECISION-0019/0020 и проверены IP-07. Foundation сохраняет существующий contract. Baseline-relative character filtering — IP-12.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-10A-ui-foundation.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/UI/` contains immutable HUD/draft/build/run-overlay states, model/view contracts, a pure presenter, runtime adapter, stable semantic element IDs and UI Toolkit UXML/USS/runtime theme; visible HP/XP bars and 6+6 live build slots surface IP-03/IP-05/IP-06/IP-08/IP-09 state; `GameplayUiRoot.cs` owns the thin Unity lifecycle adapter; `GameplayCompositionRoot.cs` injects gameplay dependencies; gameplay-owned IMGUI was removed from `LevelUpDraftRuntime.cs`.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 134/134 tests passed on 2026-09-14; PlayMode gameplay smoke, 1/1 passed. Coverage includes presenter HUD/draft/build snapshots and intents, development-build gating, pause/result state, required UXML semantic elements and runtime theme import, scene wiring, non-zero resolved HP-bar geometry, 6+6 rendered build slots and live HUD/draft/build updates through two level-ups with reroll/banish.

Historical deviations: development commands use fixed fixture amounts and are visible only in Editor/Development Build; production presentation, navigation and content screens remain IP-26 scope.

Historical documentation impact: [DECISION-0005](../decisions/0005-vertical-ui-delivery.md) records the user-approved AI-first vertical UI pipeline; `WORKFLOW.md` and every IP specification now define a feature-owned UI/observability contract; IP-26 integrates and completes the UI rather than introducing it for the first time; Game Design and Content Design behavior are unchanged.

### IP-31 — Локальная телеметрия ручных прогонов

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-01, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-10, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-10 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: Нет зависимости от production approval, полного art/UI/meta или новых encounters. Capabilities явно ограничены поставленными producers; diagnostic config budgets фиксируются до implementation.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-31-manual-run-telemetry.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Новый модуль; прежнего implementation/verification evidence нет.

### IP-32 — Ручные прогоны и AI-assisted balance review

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-31
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-31 (Blocked, target scope).
Remaining gates: BG-01 для применения конкретных чисел/механик; отсутствие product target отмечается, а не заполняется AI.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-32-manual-ai-balance.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Новый модуль; прежнего implementation/verification evidence нет.

### IP-11 — Set recipes, priority draft policy и effect families

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-07, IP-08, IP-09, IP-10, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-09 (Blocked, target scope), IP-10 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: G-08 закрыт DECISION-0017. G-02/G-04/G-05/G-13: processing order/reroll-banish policies (Book pool уже определён DECISION-0020), disc-return и trash-explosion conflicts, modifier applicability, exact thresholds/effect values. Framework fixtures не назначают production значения.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-11-set-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Sets/` defines validated active/passive recipe components with minimum levels, probability-gated `SetDefinition`s, unlimited slot-free acquisition and one independently ticked/disposed extra-ability instance per acquired set; `DraftPool` and `PlayerBuild` integrate sets into the unified level-up flow while blocking duplicates and levels; `FixtureSetCatalog` loads two non-production shared-component recipes from `Resources/Content/Sets/FixtureSets.json`; `GameplayUiPresenter`/ViewState/UXML expose acquired sets separately from 6+6 slots and fixture recipe progress with stable semantic IDs; `FixtureRuntimeContentCatalog` validates recipe content references and composes the fixture set runtime.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 136/136 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Seven new EditMode checks cover unmet and threshold-complete recipes, probability miss/hit, slot-free level-less acquisition, duplicate rejection, shared components across multiple sets, independent extra-ability tick/disposal, missing recipe references and live `LevelUpDraftRuntime` acquisition; UI presenter/assets and gameplay smoke verify separate sets state and fixture recipe-progress observability.

Historical deviations: `FIXTURE-SET-*` recipes, names and 50% draft chances are explicitly non-production test configuration; fixture extra abilities provide lifecycle/tick observability without claiming a production gameplay effect. Production SET-001…008 recipes, weights and concrete effects remain IP-19 scope and content-gated.

Historical documentation impact: Game Design, Content Design and IP-11 scope remain unchanged; execution evidence and IP-19 dependency state were synchronized.

### IP-12 — Character definitions, weighted draft и selection presentation

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-07, IP-08, IP-09, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-09 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: G-14/G-15: numeric weights, unlock completion semantics и baseline display metadata при незаполненности; renderer не придумывает их.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-12-character-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Progression/Character/` defines validated character definitions, typed starting-build references, per-active-skill draft weights with a default of 1, and a roster that exposes only explicitly unlocked choices; `DraftPool` performs seeded weighted sampling without replacement and removes zero-weight active skills before random selection; `LevelUpDraftRuntime` resolves the selected character's starting active skill through the shared content registry. `FixtureCharacterDefinitionCatalog` loads two non-production profiles from `Resources/Content/Characters/FixtureCharacters.json`; `GameplayCompositionRoot` selects the configured unlocked fixture, applies its base stats and starting loadout, and UI Toolkit exposes unlocked character cards with starting skill and compact stat snapshots.

Historical verification evidence: Unity 6000.6.0f1 EditMode, 141/141 tests passed on 2026-09-15; PlayMode gameplay smoke, 1/1 passed. Five new EditMode checks cover two distinct fixture profiles, unlocked/locked selection, starting active-slot resolution, reproducible seeded weighted sampling, zero-weight exclusion, character-dependent deterministic outcomes, and passive modifiers layered on the selected base stats. Content-registry, UI presenter/assets, scene wiring and PlayMode smoke additionally verify referenced skills, the configured selected character and unlocked-character observability.

Historical deviations: `FIXTURE-CHARACTER-AGILE` and `FIXTURE-CHARACTER-STURDY`, their stats, unlock flags and weights are explicitly non-production test configuration. Production CHAR-001…010, production starting SKILL references and persistent unlock state remain IP-22/IP-25 scope and content-gated.

Historical documentation impact: Game Design and Content Design already define the required character rules and remain unchanged; DECISION-0009 was synchronized with the expanded JSON character shape. IP-22 no longer depends on unfinished framework work (IP-17 and CG-01 still block production characters), and IP-25 now remains blocked only by IP-16 plus its production-economy gate.

### IP-12A — Visual Presentation Foundation и asset production pipeline

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-00, IP-02, IP-03, IP-04, IP-05, IP-08, IP-12
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-12 (Blocked, target scope).
Remaining gates: G-17/G-18: сопоставить approved CHAR-001 concept с master и исправить always-green vs nongreen family conflict; per-image replacement/approval gates остаются.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-12A-visual-presentation-foundation.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: module registered at `docs/implementation/modules/IP-12A-visual-presentation-foundation.md`; user-approved visual language and generation contract are recorded in `docs/art/ART_DIRECTION.md`; user-approved source/runtime layout, naming/version lifecycle, provenance, raster/import contract and replacement gates are recorded in `docs/art/ASSET_PIPELINE.md`; gameplay/presentation isolation, single-writer pose composition, data-driven motion profiles and authoritative signal/lifecycle contracts are approved in `docs/decisions/0013-procedural-sprite-presentation.md` and enforced for future work by `AGENTS.md`. `Assets/Game/Presentation/` provides a validating JSON-backed motion-profile catalog, pure pose compositor, isolated rig, pause-aware/reinitializable runtime and Editor importer enforcing the sprite contract; its zero-velocity idle cycle combines profile-driven breathing squash/stretch, vertical bob and slight anxious sway, then blends out as locomotion reaches reference speed. `Health.Damaged` supplies authoritative hit reactions. `CharacterDefinition` resolves both visual and motion-profile content references, `FixtureRuntimeContentCatalog` registers those definitions, and `GameplayCompositionRoot` initializes and rolls back the presentation subsystem. `Gameplay.unity` uses `Player/VisualRoot/{BodyRoot,ShadowRenderer}` and no longer renders or deforms the gameplay root. Its Editor/Development-Build UI exposes the real presentation runtime through reproducible `Live`, `Idle`, `Left`, `Right` and `Reset` controls; damage and pause continue to use authoritative gameplay signals. Development tooling is now collapsed by default behind a compact `DEV` launcher and uses bounded `Run`, `Build` and `Presentation` tabs with scrolling for long content, per the user-approved extension to DECISION-0005 and the project workflow. The user-approved goblin iterations, selected master and verbatim prompt/provenance are stored under `Art/Source/Characters/fixture-character-agile/`; the normalized runtime body is `Assets/Resources/Art/Sprites/Characters/fixture-character-agile/fixture-character-agile-body.png` and is loaded through `FixtureSprites.json` instead of placeholder fallback.

Historical verification evidence: final Unity 6000.6.0f1 regression passed on 2026-09-16: EditMode 159/159 and PlayMode 1/1. Checks cover motion-profile validation, visible zero-velocity idle pose, diagonal motion calibration, equivalent pose at simulated 30/120 FPS, presentation safety bounds, pause/win/loss freeze, pose composition, preview override without authoritative-body mutation, pause/reset and repeated initialization, damage signaling, character presentation references, runtime catalog resolution, showcase UI intents/assets, composition-root wiring and the scene hierarchy boundary. The imported body is validated as 512×512, 320 PPU, Full Rect, custom ground-contact pivot, Bilinear, Clamp, no mipmaps, uncompressed and Resources-loadable; alpha analysis found a 73.4% visible-height silhouette with a fully transparent outer border. PlayMode verifies the real goblin sprite, facing, hit flash, pause freeze and reset in Gameplay. All referenced JSON documents parse successfully, required source/master/runtime/`.meta` artifacts exist, and `git diff --check` passes. The user approved the integrated gameplay-scale motion on 2026-09-16 after the idle frequency was reduced from `1.35` to `0.675`; the reusable recipe, provenance template and five category checklists satisfy the handoff criterion for adding the next asset without redesigning the pipeline.

Historical deviations: user explicitly prioritized this new cross-cutting fixture vertical slice before IP-13. It uses `FIXTURE-CHARACTER-AGILE`; Draft `CHAR-001…010` remain unimplemented and CG-01 is not bypassed. DECISION-0013 moves Player visual ownership out of `PlayerMover`/root `SpriteRenderer` into a child presentation rig without changing movement or collision rules.

Historical documentation impact: module scope and execution order registered; `docs/art/ART_DIRECTION.md` and `docs/art/ASSET_PIPELINE.md` approved by the user on 2026-09-16 as the visual and asset-delivery sources of truth; DECISION-0013 and `AGENTS.md` define the implementation boundary for procedural sprite work. `ASSET_PIPELINE.md` now includes the complete 13-stage reuse recipe and separate checklists for characters, enemies, projectiles, pickups and UI portraits/icons; `Art/Templates/asset-record.template.json` provides a copyable provenance record. DECISION-0005, `WORKFLOW.md` and `AGENTS.md` require compact, collapsed, tabbed and bounded development tooling. Game Design and Content Design are unchanged because gameplay rules and production entities are unchanged.

### IP-13 — Enemy movement/attack patterns и control integration

Scope revision: design-sync-R2
Status: Ready
Dependencies: IP-03, IP-04, IP-05
Current packet: Fixture pattern/control integration; prerequisites и G-07 готовы. Модуль расположен после IP-31 и не входит в текущий разрешённый пользователем отрезок исполнения.
Remaining gates: G-07 resolved by DECISION-0017; missing attack fields G-14 для production cards; synthetic required values остаются fixture.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-13-enemy-patterns.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/Model/` adds validated, composable movement and attack profiles/controllers for seek, keep-distance, orbit, zigzag, approach-retreat and telegraphed dash plus single/fan/burst/ring/cross/spiral/explosive projectile patterns; `Assets/Game/Enemy/Runtime/EnemyProjectileRuntime.cs` and `EnemyProjectileFactory.cs` provide pooled, pause-safe projectile lifecycle, player hit/explosion damage and end-of-run cleanup; `EnemyRuntime.cs` composes profiles and renders dash telegraphs. `FixtureEnemies.json` contains seven non-production combinations, and `ContinuousFixtureEnemySpawner` cycles them without wave-system changes while sharing enemy/projectile pools. The compact development Run pane exposes current fixture ID, movement phase and attack pattern through the immutable UI state/presenter boundary.

Historical verification evidence: Unity 6000.6.0f1 EditMode 169/169 and PlayMode gameplay smoke 1/1 passed on 2026-09-17. Eight IP-13 EditMode checks cover movement-family behavior, dash direction lock/telegraph/pause, fan/ring/cross geometry, burst/spiral sequencing, direct hit/explosion miss/run-state damage, pause-safe projectile lifetime, end-of-run projectile cleanup and fixture coverage of melee/dash/required projectile families. Full-suite scene/content/UI checks verify catalog registration, composition-root wiring and semantic UI assets; `git diff --check` passed.

Historical deviations: all new definitions and numeric values are explicitly `FIXTURE-*` compatibility content; Draft `ENEMY-001…020`, bosses and canonical encounter schedules remain unimplemented. No product-rule deviation recorded.

Historical documentation impact: Game Design and Content Design remain unchanged because the module implements their existing framework contract without promoting Draft entities. Execution evidence and direct dependant readiness were synchronized.

### IP-14 — Wave Director: continuous и burst timeline

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-04, IP-13
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-13 (Ready, target scope).
Remaining gates: G-11/G-14 только в части shared event boundary/data; отдельный W-01: burst cap/drop-vs-defer, catch-up и whether bosses/Travelers count toward cap. Эти pressure rules до реализации не выбираются молча.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-14-wave-director.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Verified

Historical implementation evidence: `Assets/Game/Enemy/Model/Wave/` adds validated `WaveTimelineDefinition`/`WavePhaseDefinition` (tag, duration, spawn interval, alive cap, weighted composition, independent per-phase `WaveEnemyModifiers`) and `WaveHookDefinition` mid-boss/final-boss markers, the pure `WaveDirector` (phase selection from run time, spawn-count/capacity decisions, seeded weighted enemy choice, one-shot hook events) and `WaveEnemyScaler`; `FixtureWaveTimelineCatalog` loads the non-production `Resources/Content/Waves/FixtureWaveTimeline.json` (13 phases covering ordinary/pressure/elite/rest rhythm across three escalating cycles plus a final rush, with a faster-but-frailer later pressure phase). `ContinuousFixtureEnemySpawner` now only executes director decisions and keeps the shared enemy/projectile pools (release unregisters from `EnemyRegistry`, reuse re-registers); `GameplayCompositionRoot` builds the director from the registry-validated timeline; the RNG `seed` and the `spawnRadius` live in the timeline JSON (no serialized/hardcoded copies), and enemy movement/attack/modifier DTOs carry no tuning defaults — per-kind fields are required in JSON and rejected by name when missing (see the AGENTS.md config rule). UI: HUD `hud-wave` label with per-tag style classes and a dev Run-pane wave observation (timeline time, phase, interval/cap/alive, mix, multipliers, next hook) flow through `WaveViewState`/`WaveObservabilityViewState` and the presenter. See [DECISION-0014](../decisions/0014-wave-director-timeline.md).

Historical verification evidence: Unity 6000.6.0f1 EditMode 195/195 project tests passed on 2026-09-20 (run through the open Editor via UnitySkills, after the DTO-default/seed/spawnRadius-to-JSON refactor; the same run's only failure is in the third-party `UnitySkills` package's own tests, not in `Game.*`). PlayMode gameplay smoke 1/1 (`GameplayScene_ComposesLevelsUpAndResumes`) also passed after that refactor (the second PlayMode test in the run belongs to the UnitySkills package). 26 new EditMode checks cover definition/config validation (phases, composition, modifiers, hooks, duplicate ids), phase transitions (ordered, skipped, last-phase hold), pause/not-running behavior (no spawns, no transitions, no hooks), intensity/capacity clamping, phase-scoped weighted composition with per-phase modifiers, seed determinism and weight ratios, one-shot hook ordering, fixture rhythm (all four tags, elite pressure > respite, later phase faster-but-frailer, full 900s walk with every phase reached and no missing enemy refs), registry-validated timeline references, and pooled spawner cycles (six refill/release cycles reuse ≤ cap instances, `EnemyRegistry` returns to baseline, released enemies are inactive and never returned as targets, killed enemy is reused with fresh health, `Shutdown` clears everything). Presenter test verifies the HUD wave state and observability rebuild on transition; the asset test verifies the new semantic UXML IDs; the PlayMode smoke verifies `WAVE 1/…· ORDINARY` and the dev wave observation in the composed scene. Manual check: open the DEV panel → Run tab and watch `WAVE DIRECTOR` and the HUD wave badge change colour at ~0:45 (Pressure), ~1:15 (Elite) and ~2:00 (Respite).

Historical deviations: all phases, compositions and multipliers are explicitly `FIXTURE-*` compatibility content; canonical 15-minute schedules stay CG-02 gated (IP-24) and Draft `ENEMY-*` entities remain unimplemented. Hooks are only announced; boss spawning belongs to IP-15. No product-rule deviation recorded.

Historical documentation impact: Game Design and Content Design unchanged (framework implements the existing rhythm contract without promoting Draft content); DECISION-0014 added.

### IP-15 — Boss/mid-boss encounter framework

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-01, IP-05, IP-08, IP-13, IP-14, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-13 (Ready, target scope), IP-14 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: G-07 закрыт DECISION-0017. G-14: используемые phase/attack fields; final timing configurable, production values не обязательны для synthetic framework.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-15-boss-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Ready
Dependencies IP-01, IP-13 and IP-14 are Verified; consume `WaveDirector.HookTriggered` (MidBoss/FinalBoss) per [DECISION-0014](../decisions/0014-wave-director-timeline.md).

### IP-16 — Field definitions, selection и run configuration

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-02, IP-12, IP-14, IP-15, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-12 (Blocked, target scope), IP-14 (Blocked, target scope), IP-15 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: G-14/G-15: конкретные geometry/difficulty/unlock values; fixture metadata отдельно.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-16-field-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-15 (IP-14 is Verified).

### IP-28 — World pickup framework: зелье лечения и Book

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-05, IP-06, IP-07, IP-09, IP-10, IP-11, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-09 (Blocked, target scope), IP-10 (Blocked, target scope), IP-11 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-01/G-03 draft pool/consume/queue/empty-Book currency закрыты DECISION-0019/0020. G-02/G-10 — set controls и potion/drop edge cases. Недостающие production числа и Book ID/card блокируют соответствующие production packets IP-20/IP-30, а не этот fixture framework.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-28-world-pickups.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Новый модуль; прежнего implementation/verification evidence нет.

### IP-29 — Traveler encounter framework

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-08, IP-13, IP-15, IP-16, IP-28
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-13 (Ready, target scope), IP-15 (Blocked, target scope), IP-16 (Blocked, target scope), IP-28 (Blocked, target scope).
Remaining gates: G-11/G-12/G-14: temporal/spatial/type/scaling/support semantics и required values.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-29-traveler-framework.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Новый модуль; прежнего implementation/verification evidence нет.

### IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-01, IP-03, IP-12, IP-16, IP-10A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-12 (Blocked, target scope), IP-16 (Blocked, target scope), IP-10A (Blocked, target scope).
Remaining gates: CG-03/G-15: prices/rewards/upgrades/achievement conditions и Quit reward semantics. Framework fixtures отдельно от production economy.
Remaining acceptance / IDs: Persistent profile/flow и production economy, rewards/prices/upgrades/unlocks; CG-03. Fixture framework не закрывает весь production scope.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-16; production economy remains CG-03 gated. IP-12's `CharacterRoster` accepts an explicit unlocked-ID set ready to be supplied by this profile layer.

### IP-26 — Functional UI и полный player flow

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-01, IP-10A, IP-11, IP-12, IP-15, IP-16, IP-25, IP-28, IP-29, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-10A (Blocked, target scope), IP-11 (Blocked, target scope), IP-12 (Blocked, target scope), IP-15 (Blocked, target scope), IP-16 (Blocked, target scope), IP-25 (Blocked, target scope), IP-28 (Blocked, target scope), IP-29 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-15/G-16: Quit/reward/failure ordering, real audio/shake/settings contract. Retry same character/field immediate уже утверждён.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-26-functional-ui.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-16, IP-25.

### IP-17 — Production Active Skills SKILL-001…016

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-08, IP-10A, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-10A (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-08/G-09 закрыты DECISION-0017; нужны полные параметры 16 skills; G-04 только если решение меняет SKILL-008; images проходят asset gates.
Remaining acceptance / IDs: SKILL-001…016, полные уровни и per-ID assets/checks.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): CG-01 approval of target SKILL IDs.
Groundwork: [DECISION-0007](../decisions/0007-content-referenced-visuals.md) already implements the "missing presentation asset обнаруживается validator" acceptance criterion via `ContentRegistry`/`IReferencesContent`; this IP registers real `SpriteDefinition`s and resolves per-level visuals into projectile/mine rendering instead of designing the mechanism from scratch.

### IP-18 — Production Passive Items PASSIVE-001…014

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-09, IP-10A, IP-12A, IP-28
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-09 (Blocked, target scope), IP-10A (Blocked, target scope), IP-12A (Blocked, target scope), IP-28 (Blocked, target scope).
Remaining gates: G-08/G-09 закрыты DECISION-0017; G-10 и полные значения 14 passives остаются; отсутствие конкретного runtime parameter не заполняется hidden default.
Remaining acceptance / IDs: PASSIVE-001…014, production data/icons и связанные integration checks.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-09 and CG-01 approval of target PASSIVE IDs.
Groundwork: [DECISION-0009](../decisions/0009-json-content-config.md) already loads fixture passives from `Resources/Content/Passives/FixturePassives.json` via `FixturePassiveCatalog`; this IP registers real `PASSIVE-*` entries in that same JSON convention instead of hardcoding them.

### IP-19 — Production Sets SET-001…020

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-11, IP-17, IP-18, IP-28, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-11 (Blocked, target scope), IP-17 (Blocked, target scope), IP-18 (Blocked, target scope), IP-28 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-08 закрыт DECISION-0017. G-02/G-04/G-05/G-13: recipes/effects approved, но thresholds/proc payload и два внутренних конфликта требуют закрытия.
Remaining acceptance / IDs: SET-001…020, полные thresholds/effect values, icons/VFX и integration.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-17, IP-18 and CG-01 approval of target SET/component IDs.
Groundwork: IP-11 provides JSON-configured recipes, probabilistic unified-draft eligibility, slot-free acquisition, independent extra-ability lifecycle and set UI/recipe observability; this IP supplies approved production definitions and concrete effects through those extension points.

### IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-04, IP-13, IP-28, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-13 (Ready, target scope), IP-28 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-10/G-14: contact intervals, недостающие attack/drop/healing values и pickup lifecycle; AG-01 для конкретных картинок. Approved design не означает complete JSON.
Remaining acceptance / IDs: ENEMY-001…020, PICKUP-001, drop data и production art.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): CG-01 approval of target ENEMY IDs.
Groundwork: `EnemyDefinition.Visual` and the `Game.Presentation` module ([DECISION-0007](../decisions/0007-content-referenced-visuals.md)) already resolve end-to-end into `EnemyRuntime` rendering for the fixture enemy; `FixtureEnemyCatalog`/JSON ([DECISION-0009](../decisions/0009-json-content-config.md)) already loads enemy definitions from config instead of code; this IP only needs to register real `SpriteDefinition`s and `ENEMY-*` entries in those same conventions.

### IP-21 — Production Final Bosses и Mid-bosses

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-15, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-15 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-14: точные attack timings/phase payload, rewards и required fields каждой карточки.
Remaining acceptance / IDs: BOSS-001…010 и MIDBOSS-001…010, phase/attack data и art.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-15 and CG-01 approval of target BOSS/MIDBOSS IDs.

### IP-22 — Production Characters CHAR-001…010

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-12, IP-17, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-12 (Blocked, target scope), IP-17 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-14/G-15/G-17: weights/unlock metadata и связь concept/master/runtime. CHAR-006 огр и прочие approved roster choices не переутверждаются.
Remaining acceptance / IDs: CHAR-001…010, complete stats/loadouts/weights и body/selection art.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-17 and CG-01 approval of target CHAR/starting SKILL IDs.
Groundwork: IP-12 provides JSON-driven character definitions, validated starting-skill references, base stats, disappearing-XP recovery, weighted draft values including zero, unlocked-only selection and fixture UI observability. [DECISION-0006](../decisions/0006-shared-health-model.md) keeps `PlayerCharacterRuntime` health independent of a single stat profile, while [DECISION-0007](../decisions/0007-content-referenced-visuals.md) remains the presentation-reference convention for production character art.

### IP-23 — Production Fields FIELD-001…010

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-16, IP-20, IP-21, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-16 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-14/G-15: geometry/enemy pools/difficulty и unlock conditions. Весь approved mapping переносится, numeric schedules отдельно.
Remaining acceptance / IDs: FIELD-001…010, geometry/metadata/kits/thumbnails.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-16, required production content and CG-01 approval of target FIELD IDs.

### IP-30 — Production Travelers TRAVELER-001…010 и Book

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-29, IP-12A
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-29 (Blocked, target scope), IP-12A (Blocked, target scope).
Remaining gates: G-03/G-10/G-11/G-12/G-14/G-17: Book card/ID/параметры, complete Traveler/scaling/support data и конкретные images. Designs TRAVELER-001…010 уже approved.
Remaining acceptance / IDs: TRAVELER-001…010 и отсутствующая production Book card/ID/data/art.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Новый модуль; прежнего implementation/verification evidence нет.

### IP-24 — Canonical Wave / Encounter Content и field bindings

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-14, IP-20, IP-21, IP-23, IP-29, IP-30
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-14 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-23 (Blocked, target scope), IP-29 (Blocked, target scope), IP-30 (Blocked, target scope).
Remaining gates: CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон.
Remaining acceptance / IDs: Полные production encounter schedules и bindings всех 10 полей; CG-02/CG-04.
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): IP-14, IP-20, IP-21, IP-23 and CG-02 missing Approved Wave / Encounter Content.

### IP-27 — End-to-end integration, regression и content validation

Scope revision: design-sync-R2
Status: Blocked
Dependencies: IP-00, IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-09, IP-10, IP-10A, IP-11, IP-12, IP-12A, IP-13, IP-14, IP-15, IP-16, IP-17, IP-18, IP-19, IP-20, IP-21, IP-22, IP-23, IP-24, IP-25, IP-26, IP-28, IP-29, IP-30, IP-31, IP-32
Current packet: Целевая спецификация целиком либо конкретный согласованный catalog packet после выполнения prerequisites/gates; реализация ещё не начата.
Blocked by: IP-08 (Ready, target scope), IP-09 (Blocked, target scope), IP-10 (Blocked, target scope), IP-10A (Blocked, target scope), IP-11 (Blocked, target scope), IP-12 (Blocked, target scope), IP-12A (Blocked, target scope), IP-13 (Ready, target scope), IP-14 (Blocked, target scope), IP-15 (Blocked, target scope), IP-16 (Blocked, target scope), IP-17 (Blocked, target scope), IP-18 (Blocked, target scope), IP-19 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-22 (Blocked, target scope), IP-23 (Blocked, target scope), IP-24 (Blocked, target scope), IP-25 (Blocked, target scope), IP-26 (Blocked, target scope), IP-28 (Blocked, target scope), IP-29 (Blocked, target scope), IP-30 (Blocked, target scope), IP-31 (Blocked, target scope), IP-32 (Blocked, target scope).
Remaining gates: Только реальные missing required contracts/data/asset checks полного scope этого плана. Уменьшение каталога возможно лишь как отдельное явное изменение плана; один smoke не закрывает content-complete verification.
Remaining acceptance / IDs: Новые acceptance criteria и checks из [спецификации](modules/IP-27-integration.md).
Target implementation evidence: Отсутствует для новых требований; регистрация спецификации не означает реализации.
Target verification evidence: Новые checks не запускались; historical test counts не перенесены в target verification.
Documentation impact: Зарегистрирован целевой scope, prerequisites, gates и consumer links.

#### Historical evidence — до design-sync-R2

Historical scope status: Blocked
Historical blockers (до миграции): all in-scope preceding system modules; content-complete verification also requires Approved production modules.

## Historical cross-cutting evidence — до design-sync-R2

Ниже сохранена исходная запись базы `d9a1970`. Её описание старого goblin-only setting и Draft gates относится к прежней документации и не переопределяет новый канон. Runtime по-прежнему fixture-only; новые production IDs не реализованы этой миграцией.

Cross-cutting verification: Unity 6000.6.0f1 EditMode 195/195 passed on 2026-09-20 and PlayMode 1/1 passed after the DTO-defaults refactor; continuous spawning is driven by a data-driven wave timeline per [DECISION-0014](../decisions/0014-wave-director-timeline.md). The gameplay scene is composed from one fixture runtime catalog; character selection is restricted to unlocked definitions and supplies base stats, starting active skill, presentation references and per-skill weighted draft values; draft RNG is seeded; pause ownership is reason-based; enemy targeting uses a live registry and reusable query buffers; gameplay UI follows a presenter/ViewState boundary; fulfilled set recipes participate probabilistically in the same draft while acquired sets remain outside 6+6 slots. Field bounds and ordinary obstacles follow [DECISION-0003](../decisions/0003-player-only-field-collision.md); stat composition follows [DECISION-0004](../decisions/0004-character-stat-composition.md); vertical UI delivery follows [DECISION-0005](../decisions/0005-vertical-ui-delivery.md); the shared `Game.Combat.Health` model follows [DECISION-0006](../decisions/0006-shared-health-model.md); content-referenced presentation assets follow [DECISION-0007](../decisions/0007-content-referenced-visuals.md); scale-sensitive operations use throttled perf warnings per [DECISION-0008](../decisions/0008-perf-logging.md); entity/content values are config-driven from JSON per [DECISION-0009](../decisions/0009-json-content-config.md); `GameplayCompositionRoot` rolls back already-initialized subsystems on partial init failure per [DECISION-0010](../decisions/0010-composition-root-rollback.md); frequently spawned/destroyed GameObjects (XP drops, enemies, skill mine markers and enemy projectiles) are pooled per [DECISION-0011](../decisions/0011-gameobject-pooling.md); numeric constructor-argument validation is centralized per [DECISION-0012](../decisions/0012-shared-numeric-validation.md); procedural sprite presentation is isolated from gameplay transforms and composed by a single writer per [DECISION-0013](../decisions/0013-procedural-sprite-presentation.md). All production `.cs` files now hold exactly one type, named after the file (`AGENTS.md`'s "one type per file" rule); the pre-IP-08 single-skill prototype (`ActiveSkillDefinition`, `PlayerActiveSkillRuntime`) was dead code — never wired into `GameplayCompositionRoot` — and was deleted along with its tests, dropping the suite from 142 to 131; the also-dead `NearestEnemyTargetSelector` (superseded by `EnemyRegistry.TryFindNearest`, never used in production) was deleted along with its test, dropping the suite to 129. `PlayerCharacterRuntime.Initialize` now atomically wires `Health` to `RunController` (no window where damage could be taken before death routes to run-end).

Setting boundary audit: Game Design and Content Design define the player as a small goblin escaping an escalating pursuit by villagers, soldiers, knights, mages, clergy and angels. Runtime contracts remain setting-neutral and production IDs remain unimplemented; IP-12A now deliberately includes one user-approved goblin body as `FIXTURE-CHARACTER-AGILE` pipeline evidence. Enemies, skills and other fixture visuals still use placeholders, and no production `CHAR-*`, human, knight, mage or angel entity/art is implemented. Thematic production binding remains owned by IP-17…IP-24 and final presentation/player flow IP-26.

## Status maintenance rule

После изменения статуса/API/acceptance пересчитать готовность потребителей и Next Ready по Execution order. Implemented означает выполненный полный обязательный scope; Verified — фактически пройденные проверки с evidence. Исторический test count не переносится автоматически. Каталоги ведут completed/remaining IDs здесь; если ни один оставшийся packet не готов, указывать конкретный Blocked gate. Подробности — [WORKFLOW](WORKFLOW.md).
