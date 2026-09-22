# IP-05 — Общий combat pipeline, control effects и target contract

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Переиспользовать живой IP-08 runtime/executor и общий Health. Удалённый single-skill prototype не восстанавливать. IP-05 остаётся владельцем общего damage/control boundary.

## Зависимости

[IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Управление, бой и выживание»; Content Active Skills/Enemies/Bosses/Travelers schemas, SKILL-004/013 и PASSIVE-011/014 для effect semantics; DECISION-0003/0006/0011/0012/0013; damage request/result, target provider, Health.

## Scope

Unified source/target/content/life/skill-level result и actual HP loss/heal; snapshot identity до lethal callbacks. Generic target-query adapter без concrete-enemy-only limitation. Authoritative knockback distance/direction/bonus/resistance и enemy movement-only slow state/lifetime; interfaces control application/expiry and category filters. No UI/export dependency. Source origin различает ordinary skill, set и secondary proc; правила set propagation реализует IP-11.

## Out of Scope

Возврат dead prototype, production effect numbers, burn/stun/freeze, wave spawn, telemetry storage/AI.

## Acceptance criteria

100 requested damage в 10 HP без mitigation записывает 10 actual; overkill только при измеренном post-mitigation request. При 2 wu base, +25% outgoing и 40% resistance итог 1.5 wu, immunity 100%→0. Slow 20%→80% movement, attack cadence unchanged. Pause/end, reapply/overlap/expiry следуют решённой таблице; pool reset очищает effects. Player knockback respects player-only bounds, enemy не получает новых стен. Query adapter пригоден для future categories через fakes; полную boss/Traveler integration проверяют IP-15/IP-29.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Authoritative effect state доступен fixtures/DEV; visual recoil/flash только VisualRoot, forced movement — gameplay. Semantic source IDs предоставляются будущей IP-31 без обязательного recorder.

## Проверки

Actual vs requested damage/heal/max-HP rescale; zero-damage control; simultaneous slow/knockback, resistance boundaries, dash priority, zero-direction, collision/pause/end; lethal hit→death→pool return; delayed projectile source snapshots. Query tests fake ordinary/boss/Traveler targets, no missing pattern path.

## Документационные изменения

Combat/control/source API и approved gap deltas; обновить callers/tests в одном change, cross-layer DECISION. IP-08/11/13/15/28/29/31 потребляют один contract.

## Gates и недостающие решения

G-06…G-09 закрыты [DECISION-0017](../../decisions/0017-combat-control-semantics.md). Общие edge cases утверждены; production duration/magnitude остаются required content data. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-06](IP-06-xp-progression.md), [IP-08](IP-08-active-skill-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-13](IP-13-enemy-patterns.md), [IP-15](IP-15-boss-framework.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт реализации для потребителей

`Game.Combat` — pure C# request/source/identity/result и HealthChange. `Requested` — величина до mitigation/restoration; `AfterMitigation` — вычисленное масштабированное значение, `Actual` — фактическая потеря/восстановление HP до callbacks; `Overkill` имеет смысл только для damage. Max-HP rescale не является damage/heal. `ResolvedKnockbackDistance` — дистанция рассчитанного impulse до столкновения/смерти, а не измеренный путь тела.

`CombatSource` сохраняет owner life/run/content/category, effect content ID, origin (ActiveSkill/Set/SecondaryProc/EnemyContact/EnemyProjectile) и доступный skill level. Незаполненная attribution остаётся Unknown/null, не выдумывается из последнего активного объекта. `EnemyDamageRequest` — совместимый adapter общего request; `WithAmount`/`WithDirection` сохраняют attribution и controls. Runtime публикует immutable `CombatResolved` даже для lethal hit; подписки относятся к жизни объекта и снимаются при её завершении.

`CombatControlProfile` принадлежит wave/contact/projectile config. Положительная distance или slow fraction требует положительного explicit duration. `CombatControlState` хранит один knockback vector/timer и таймеры slow по source; конечный неполный physics tick сохраняет указанную дистанцию. Player получает только knockback, enemy — knockback и slow. Normal/dash velocity складывается с control velocity одним writer; zero movement speed от slow не замораживает dash phase или attack cadence.

`ICombatTargetQuery` задаёт nearest/copy-alive/category filters через `IEnemyDamageReceiver`; scene registry принимает `IEnemyLifeTarget`, включая adapters будущих bosses/Travelers. `EnemyTargetLife` — reference + captured life для delayed tracking и projectile deduplication. Конкретные target components будущих категорий остаются у IP-15/IP-29.
