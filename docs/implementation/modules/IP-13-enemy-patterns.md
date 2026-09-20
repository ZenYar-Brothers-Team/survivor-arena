# IP-13 — Enemy movement/attack patterns и control integration

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить families и pooled enemy projectiles; подключить новые combat/control/source contracts в существующие controllers.

## Зависимости

[IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD combat/enemies; ENEMY-001…020 schemas/cards как compatibility, BOSS/MIDBOSS relevant patterns; EnemyMovementController/AttackController/projectile runtime.

## Scope

Seek/keep-distance/orbit/zigzag/retreat/dash и single/fan/burst/ring/cross/spiral/explosive attacks; source ID/life in projectiles survives shooter death. Movement combines base/scaled speed and approved slow/knockback/dash priority; explicit per-kind knockback/resistance. Category-neutral composition для future boss/Traveler; projectile burst ≠ wave burst.

## Out of Scope

Production entity numbers/art, wave burst schedule, general pathfinding/formation AI.

## Acceptance criteria

Every fixture pattern hits/misses/lifetimes as configured; slow меняет movement, не cooldown; immunity/knockback boundaries follow IP-05. Dash telegraph/direction lock не переписываются visual motion. Pause/end clean all projectiles; pooling clears trails/effects/owner ID. New combos не требуют changes в draft/wave models.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Telegraph и projectile читаемы; dev phase/attack/effect/source snapshot через presenter; no permanent HUD для ordinary enemies.

## Проверки

Geometry/burst/spiral/explosive cases, dash+slow+knockback approved ordering, pause/lifetime/hit/source-after-shooter-death, repeated pool reuse; PlayMode representative patterns.

## Документационные изменения

Extend enemy JSON schema required per-kind fields, per-pattern compatibility matrix; IP-15/20/21/29 consume same controllers.

## Gates и недостающие решения

G-07 закрыт DECISION-0017: knockback не приостанавливает dash/steering. Missing attack fields G-14 остаются для production cards; synthetic required values — только fixture. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-14](IP-14-wave-director.md), [IP-15](IP-15-boss-framework.md), [IP-20](IP-20-production-enemies.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
