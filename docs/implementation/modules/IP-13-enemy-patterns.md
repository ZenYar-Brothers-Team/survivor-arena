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

## Schema и runtime contract

`EnemyDefinitionData` требует явные `knockbackResistance` и `contactControls.knockbackDistance`, включая ноль. Для `TelegraphedDash` отдельно требуется `dashContactControls.knockbackDistance`; runtime выбирает его только в фазе Dashing. Ненулевой knockback требует positive `knockbackSeconds` через общий CombatControlProfile. Nullable поля DTO не скрывают missing production values.

Каждая ranged attack требует `projectileCount`, `projectileRadius`, `telegraphSeconds` и `controls.knockbackDistance`; остальные числовые базовые поля валидирует EnemyAttackProfile. Fixture packet задаёт wind-up 0.25 s. `telegraphSeconds` допускает явный zero для low-level immediate profiles; все ranged JSON fixtures имеют ненулевое предупреждение. Значения fixture не утверждают timings production-карточек.

| Attack family | Required per-kind fields / semantics | Fixture |
|---|---|---|
| Single | count=1, направление на цель | FIXTURE-ENEMY-SINGLE |
| Fan | count≥1, explicit spreadDegrees 0…360; равномерный сектор вокруг aim | FIXTURE-ENEMY-FAN |
| Burst | count≥1, positive burstIntervalSeconds; count — число последовательных одиночных выстрелов | FIXTURE-ENEMY-BURST-ORBIT |
| Ring | count≥1; равномерный полный круг | FIXTURE-ENEMY-RING-ZIGZAG |
| Cross | count=4; одна ось креста направлена на цель | FIXTURE-ENEMY-CROSS-RETREAT |
| Spiral | count≥1, explicit finite rotationStepDegrees; смещение каждого следующего залпа | FIXTURE-ENEMY-SPIRAL |
| Explosive | count=1, positive explosionRadius; radial damage/control при contact или expiry внутри radius | FIXTURE-ENEMY-DASH-EXPLOSIVE |

EnemyAttackController публикует Cooldown / Telegraphing / Bursting, оставшееся время фазы, aim и число оставшихся burst shots. После начала telegraph полный configured wind-up проходит без damage; aim продолжает следовать за целью. Cooldown отсчитывается от выпуска первого выстрела, включая время burst; после его завершения и истечения cooldown начинается следующий wind-up. Контракт fixture scheduler не определяет wave burst/cap/catch-up policy W-01. Slow и knockback не подменяют attack time. Pause не меняет aim, phase или timers.

EnemyMovementController сохраняет Seek, KeepDistance, Orbit, Zigzag, ApproachRetreat и TelegraphedDash. Dash direction фиксируется при входе в telegraph; pause сохраняет видимую фазу любых movement kinds. Runtime складывает `base speed × wave speed multiplier × slow multiplier × dash multiplier` с независимой knockback velocity. Например, base=0.7, wave=2, slow=0.5, dash=4 дают 2.8 wu/s по locked направлению; impulse 2 wu с resistance=25% за 2 s добавляет 0.75 wu/s по своему направлению. Dash/steering timers продолжаются по DECISION-0017.

Projectile хранит value CombatSource со source ID, owner LifeId/RunId/category и immutable profile. Shooter death/reuse не меняет этот snapshot. Hit/expiry возвращает снаряд до damage callbacks; сообщение в боевой pipeline использует сохранённые locals. Это позволяет callbacks повторно арендовать тот же component без его повторного despawn старым попаданием. Pause обнуляет velocity и замораживает lifetime; Won/Lost/Stopped немедленно освобождают снаряд без ожидания следующего physics tick. Reinit/return снимают run subscription, очищают source/profile/target/lifetime/velocity/renderer/trail. Callback старого run не действует на новый running life.

DEV остаётся в существующем gated/collapsed drawer: presenter получает movement/attack phase, phase timer, burst remainder, slow count/multiplier, knockback timer и last-shot source/life. Telegraph line использует тот же Sprite material; permanent ordinary-enemy HUD не добавляется. Проверки геометрии и lifecycle не означают художественное approval production VFX.

## Compatibility matrix

| Canonical targets | Framework mapping | Что остаётся owning packet |
|---|---|---|
| ENEMY-001/002/003/009/020 | Seek + contact/resistance | Production contact intervals, assets |
| ENEMY-004/005/012 | KeepDistance/Orbit + Single | Per-card targeting/hold behaviour, projectile geometry/data |
| ENEMY-006/019 | KeepDistance/Seek + Fan | Production parameters/art |
| ENEMY-007/016 | TelegraphedDash + separate dash contact controls | Production timings/duration |
| ENEMY-008 | Orbit + contact | Production steering data |
| ENEMY-010 | KeepDistance + Burst | Production ranges/art |
| ENEMY-011 | KeepDistance + Ring | Production ranges/art |
| ENEMY-013/017 | ApproachRetreat / Zigzag + contact | Production cycles/amplitudes |
| ENEMY-014 | Ranged + Explosive | Production speed/lifetime; explosion assets |
| ENEMY-015/018 | Cross / Spiral | Production spacing/rotation and other missing fields |
| BOSS-001/002/003/004; MIDBOSS-004/005/006/007 | Те же family profiles и category-neutral source/control pipeline | HP phases, ordering, post-dash triggers и encounter schedules — IP-15/IP-21 |
| Future Travelers | Те же controllers и CombatEntityCategory.Traveler | Role/support/escape и Book reward — IP-29/IP-30 |

IP-14/15/20/21/29 получают controllers/profiles без зависимости Enemy → draft или новых wave scheduling правил. WaveEnemyScaler сохраняет controls/telegraph при изменении damage/speed. Межслойная техническая запись: [DECISION-0028](../../decisions/0028-enemy-pattern-lifecycle.md). Текущий status и фактическое evidence — только STATUS.

## Traveler combat extension

IP-29 использует EnemyRuntime category Traveler и общий target registry.
IEnemyMovementDriver задаёт peaceful/support steering; encounter guard проверяется
до outgoing/incoming hit и FixedUpdate. EnemyProtection хранит source-owned aura/
shield, применяется в общем damage pipeline и сбрасывается при pool reuse.
Порядок reduction → shield → Health и cleanup описаны в DECISION-0035/0036.