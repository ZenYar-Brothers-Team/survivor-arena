# design-sync-R2 evidence through 2026-09-21

Подробные записи выполненной реализации и проверок, перенесённые из STATUS
без нового запуска тестов. Это evidence на указанную дату, не текущая таблица
статусов. Актуальность после последующих изменений определяет [STATUS](../STATUS.md).
Читать только нужный раздел. Пути в inline code — от корня репозитория;
`TestResults` — локальные результаты, не постоянные артефакты Git.

## M-01

Documentation verification: три canonical bodies совпадают с нормализованными approved sources; все 121 target cards сохранены. Проверены 35 полных спецификаций, 160 dependency edges, обратные связи и соответствие Dependencies в STATUS. Очередь включает каждый IP ровно один раз, циклов нет; следующий Ready — IP-01. Ссылки и whitespace проверены. Runtime/config/assets/test files не изменены; Unity tests для этой документационной миграции не запускались. Отдельные копии старых трёх документов не созданы.

## IP-00

Scope revision: design-sync-R2

Target implementation evidence: Существующая реализация сохранена; M-01 изменяет только ссылки/Context, без кода.

Target verification evidence: Сохранённое evidence [прежнего scope](pre-design-sync-R2.md#ip-00) применимо к неизменному behavioral scope; в M-01 проверены только документы/совместимость, Unity заново не запускался.

Documentation impact: Обновлены Context/источники/consumer links.

## IP-01

Scope revision: design-sync-R2

Target implementation evidence: RunModel RunId/immutable RunOutcome, contributor capture/unavailable/failure contract, Stop reasons; RunController Shutdown/reinit; elapsed HUD. Контракт и границы Retry записаны в IP-01.

Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 257/257 и PlayMode 1/1 passed (TestResults/EditMode.xml, PlayMode.xml); 9 новых lifecycle/outcome tests, elapsed presenter assertion и terminal HUD smoke. Сторонние пакеты исключены фильтром.

Documentation impact: IP-01 terminal/time/teardown contract; GDD/CD rules unchanged.

## IP-02

Scope revision: design-sync-R2

Target implementation evidence: Существующая реализация сохранена; M-01 изменяет только ссылки/Context, без кода.

Target verification evidence: Сохранённое evidence [прежнего scope](pre-design-sync-R2.md#ip-02) применимо к неизменному behavioral scope; в M-01 проверены только документы/совместимость, Unity заново не запускался.

Documentation impact: Обновлены Context/источники/consumer links.

## IP-03

Scope revision: design-sync-R2

Target implementation evidence: CharacterStats/Modifier/BaseStats новые каналы, CharacterHealthStatBinding, required CharacterBaseStatsMapper, actionSpeed JSON/API migration, immutable HUD stats + DEV scroll observation.

Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 283/283 и PlayMode 1/1 passed; stacking/replacement/removal, low-HP 100/55/10/5%, heal/max-HP/no recursion, overflow rollback, 17 missing-field cases, mapping и HUD smoke.

Documentation impact: stat/units/JSON dictionary в IP-03, terminology в DECISION-0004; GDD/CD formulas unchanged; applicability оставлена G-08/G-09.

## IP-04

Scope revision: design-sync-R2

Target implementation evidence: EnemyLifeEvent/LifeId/category/reasons/source/position, IEnemyLifeTarget/IEnemyLifecycleSink; pool-safe teardown/reinit, EnemyExperienceDropSink composition, ordinary-enemy RunOutcome contribution, reentrant Health/AoE fixes.

Target verification evidence: 2026-09-20, Unity 6000.6.0f1, Game.* EditMode 289/289 и PlayMode 1/1 passed; pool identity/payload/source retention, cleanup/escape, live reinit, synchronous lethal callbacks, nested AoE, XP regression и terminal HUD/outcome. PlayMode повторён после исправления smoke timing и test assembly references.

Documentation impact: IP-04 lifecycle contract; DECISION-0016 Proposed (architecture review), GDD/CD rules unchanged; TD-001/003 mitigation documented without rewriting debt register.

## IP-05

Scope revision: design-sync-R2

Target implementation evidence: CombatDamageRequest/CombatResult/HealthChange; source/target life+run+content snapshots до lethal callbacks; activation-time low-HP damage; CombatControlState и JSON profiles; PlayerMover/EnemyRuntime добавляют knockback к movement/dash. ICombatTargetQuery/SceneCombatTargetQuery работает с IEnemyLifeTarget, category filters и fake Boss/Traveler; EnemyTargetLife защищает delayed target от pool reuse. Enemy/projectile/contact и 7 active-skill effect families используют общий boundary. DEV показывает controls.

Target verification evidence: Unity 6000.6.0f1, 2026-09-20: Game.* EditMode 311/311, PlayMode 1/1, skipped 0; без third-party suites. Overkill/actual healing, zero-damage controls, resistance, slow refresh/expiry/overlap, movement/dash clocks, attack cadence, pause/stop, walls/no stored distance, lethal→pool identity, delayed source/low-HP snapshot, all 7 effect paths, generic category query и composed PlayerMover smoke.

Documentation impact: DECISION-0017 approved; GDD combat, PASSIVE-014, DESIGN_SYNC, proposal и affected IP gates синхронизированы. Size/range mapping остаётся реализацией IP-08, set propagation — IP-11.

## IP-06

Scope revision: design-sync-R2

Target implementation evidence: Existing/new drops читают effective radius; sprite/collider size не меняет world-unit pickup distance. ExperienceDropIdentity и ExperienceAwardEvent сохраняют drop/run/source life/content, base/awarded и Collected/Expired/DevelopmentIntervention origin; death sink deduplicates life. PlayerExperienceRuntime contributor `experience` пишет immutable RunExperienceSnapshot отдельно от current-level remainder; atomic XP advance, pool/producer Shutdown и DEV observation. Curve/lifetime JSON сохранены.

Target verification evidence: Unity 6000.6.0f1, 2026-09-20: Game.* EditMode 320/320, PlayMode 1/1, skipped 0, без third-party suites. Radius before/after spawn и removal, world scale, death source/dedup, pool life reset, pickup/recovery multipliers и recovery0, no double award, multiple thresholds/terminal snapshot, shutdown/reinit/unregister, terminal/pause freeze, intervention origin и real-scene pickup/HUD. Повторно проверена IP-05 regression suite.

Documentation impact: IP-06 units/producer contract и fixture rationale; DECISION-0018 Proposed для архитектурного ревью реализации принятого scope. Product formulas PASSIVE-006/007/010 не изменены; G-01/G-03 позднее закрыты DECISION-0019/0020 в IP-07. IP-07 добавил atomic LevelsEarned range перед legacy per-level events, чтобы одна XP награда ставила requests подряд.

## IP-07

Scope revision: design-sync-R2

Target implementation evidence: `Progression/Draft` — immutable request/origin/revision/preview, eligibility + set provider + uniform backfill; `LevelUpDraftRuntime` — FIFO, atomic earned-level batch, stale/double intent rejection, pause ownership, terminal/shutdown cancellation, pickup dedup и immediate currency. `RunDraftSnapshot`/contributor `draft` сохраняет выбранный build, counters и BookCurrency независимо от IP-31. Config `draft.emptyBookCurrency` обязателен в fixture JSON. UI — три позиции/disabled short slots, current→next values, Book heading/accent, next request, DEV fixture Book и HUD currency.

Target verification evidence: 2026-09-21, Unity 6000.6.0f1, `scripts/Test-Unity.ps1` после проверки отсутствия открытого Editor: 343/343 Game.* EditMode, 1/1 PlayMode, 0 skipped. Новые checks покрывают FIFO/batch/Book ordering, old revision/session, short/empty/only-set failed roll, равные интервалы RNG и отсутствие повторов, banish/acquired filters, immediate/dedup/old-run Book currency, ordinary empty и поздний empty без валюты, terminal callbacks/outcome, manual pause/shutdown, pure active/passive previews, presenter/реальный short UXML и PlayMode Book→LevelUp sequence. Результаты: `TestResults/EditMode.xml`, `TestResults/PlayMode.xml`.

Documentation impact: GDD XP/Book/meta rules, UI §§7/12, approved DECISION-0019/0020, DESIGN_SYNC, proposal и consumer IP-10/IP-10A/IP-11/IP-25/IP-28 синхронизированы. Fixture currency = 1 не утверждает production баланс. Legacy per-set fixture chance изолирован адаптером до IP-11; production provider policy не объявлена реализованной.
