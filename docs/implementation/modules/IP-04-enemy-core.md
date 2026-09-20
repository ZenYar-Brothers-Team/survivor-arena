# IP-04 — Enemy lifecycle, contact damage и per-life identity

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить seek/contact/death/pooling/registry. Расширить authoritative lifecycle events без переноса drop/economy/export логики в EnemyRuntime.

## Зависимости

[IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Враги…», «Управление, бой и выживание»; Enemies schema; DECISION-0002/0003/0011; EnemyRuntime/Factory/Registry, Health; relevant TD-001/TD-003 только как проверка затронутых границ.

## Scope

Explicit per-spawn life identity, content ID, spawn/death/despawn reasons; immediate/repeating contact hits; reusable target/lifecycle adapter contract для IP-05. Death event несёт immutable position/source/life data для XP/drop/summary consumers. Ordinary category distinct from boss/Traveler. Жизненный цикл не зависит от конкретной реализации pickups.

## Out of Scope

Boss/Traveler behavior, production tuning, world-pickup policy, structured telemetry file writer.

## Acceptance criteria

Один life даёт максимум одно death event; cleanup/escape/despawn не kill. Pool rent даёт новую life identity, health/status/registry baseline. Контакт pause/end safe. Subscriber может забрать death payload после pool return без чтения изменившейся entity. Enemy passes ordinary field bounds.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HP effect виден через existing HUD, ID/life/reason в fixtures/DEV; необязательная постоянная полоса обычного enemy не вводится.

## Проверки

Spawn/register/death/unregister/reuse, sustained contact, duplicate damage/death, synchronous lethal callback и teardown; registry baseline restored. No reentrant mutation shared AoE buffers.

## Документационные изменения

Lifecycle/event contract и зависимости IP-05/06/13/28/31; mitigation для реально затронутого coupling, не blanket cleanup всего debt register.

### Lifecycle contract реализации

`EnemyLifeEvent` передаёт immutable life/run/content/category, position, nullable damage source и configured XP reward. `Spawned` различает Spawn/PoolReuse; `Died` публикуется один раз; `Despawned` различает Killed/Cleanup/Escaped/Reinitialized/Destroyed. XP и kill consumers обрабатывают только `Died`, не считают второй раз последующий `Despawned(Killed)`. Null source означает direct/unknown source, не выдуманный content ID.

`IEnemyLifeTarget` дополняет существующий damage receiver идентичностью жизни; delayed consumers обязаны сверять LifeId, а не только Unity object reference. Внутри lifecycle callbacks объект не возвращается в pool и не может быть переинициализирован; per-life subscriptions сбрасываются перед reuse. Shutdown очищает registry/contact/velocity/health binding без death event. Повторный Initialize создаёт новый Health и life identity. Control statuses добавляет и сбрасывает IP-05.

Composition подключает `EnemyExperienceDropSink` из Progression, передавая immutable death data вместо concrete enemy. Spawner предоставляет DEV ID/life/reason и `ordinary-enemy-kills` contribution; пока это не полная статистика future boss/Traveler. Потребители IP-06/28/31 подключаются к той же границе. Техническое обоснование и границы изменений Health/AoE — [DECISION-0016](../../decisions/0016-enemy-life-events.md), Proposed для архитектурного ревью.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-05](IP-05-active-skill-runtime.md), [IP-06](IP-06-xp-progression.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-13](IP-13-enemy-patterns.md), [IP-14](IP-14-wave-director.md), [IP-20](IP-20-production-enemies.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
