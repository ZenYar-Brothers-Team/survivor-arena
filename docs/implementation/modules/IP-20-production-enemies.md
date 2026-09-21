# IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-04](IP-04-enemy-core.md), [IP-13](IP-13-enemy-patterns.md), [IP-28](IP-28-world-pickups.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD enemies/combat; полные выбранные ENEMY-001…020; Art Production §2 и generic VFX; DECISION-0003/0011/0013. Полная карточка PICKUP-001, GDD правило выпадения зелья с обычных врагов, Art Production §9; pickup schema IP-28.

## Scope

двадцать definitions с movement/attack/HP/size/speed/contact/XP/resistance и role metadata; отдельно authoring visual size и gameplay geometry. Body per enemy, общие projectile families, telegraphs где они нужны механике; смерть предоставляет authoritative drop/telemetry signal. Здесь же production definition и регистрация PICKUP-001, ordinary-enemy drop bindings и world/UI art зелья через готовый IP-28. Healing/drop/radius/lifetime values должны быть заполнены по принятым правилам; passive/set modifiers применяются через общий reward contract.

## Out of Scope

final schedules, Travelers как переименованные обычные враги, самостоятельный rebalancing.

## Acceptance criteria

каждый ID создаётся по данным и демонстрирует карточку; death фиксируется один раз, despawn не выдаётся за kill/drop; enemy field pass-through сохраняется; pooled reuse очищает status, registry, visual state. Body motion не деформирует root/collider. PICKUP-001 загружается по stable ID, выдаёт указанное лечение, использует согласованные drop tables и approved runtime art. Production значения не подменяются fixture tuning; где в карточке нет чисел, packet остаётся с явно указанным gap.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

читаемая attack preparation и отличия role/silhouette; DEV показывает ID/pattern/effects; без ненужного отдельного HUD на каждого обычного врага.

## Проверки

per-ID smoke, melee/ranged/dash, hit/miss/lifetime, drop source, pause/end/pool; density/performance measurement и manual silhouette при толпе. Potion per-ID load/roll/heal, modifier integration и kill-versus-despawn reward check.

## Документационные изменения

numeric card completeness, body/projectile family reuse mapping, telemetry origin и test evidence. PICKUP-001 data completeness, drop eligibility/bindings и image provenance входят в evidence этого IP.

## Gates и недостающие решения

G-10 pickup semantics утверждены DECISION-0033, lifecycle реализует IP-28. G-14 и production data: contact intervals, недостающие attack/drop/healing values; AG-01 для конкретных картинок. Approved design не означает complete JSON. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Enemy pattern integration

Использовать [IP-13 schema/runtime contract](IP-13-enemy-patterns.md#schema-и-runtime-contract): explicit per-attack controls/wind-up, отдельный dash contact, immutable source/life snapshots и reset-safe projectile pool. Encounter owner задаёт category; новые комбинации profiles не меняют draft/wave models. Phase ordering/support/escape mechanics остаются scope этого owning packet, а fixture numbers не являются production balance.

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.