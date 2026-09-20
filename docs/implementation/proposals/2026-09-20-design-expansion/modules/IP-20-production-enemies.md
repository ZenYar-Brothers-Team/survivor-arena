# IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-20-production-enemies.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-04](IP-04-enemy-core.md), [IP-13](IP-13-enemy-patterns.md), [IP-28](IP-28-world-pickups.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новые GDD enemies/combat; полные выбранные ENEMY-001…020; Art Production §2 и generic VFX; DECISION-0003/0011/0013. Полная карточка PICKUP-001, GDD правило выпадения зелья с обычных врагов, Art Production §9; pickup schema IP-28.

## Scope

двадцать definitions с movement/attack/HP/size/speed/contact/XP/resistance и role metadata; отдельно authoring visual size и gameplay geometry. Body per enemy, общие projectile families, telegraphs где они нужны механике; смерть предоставляет authoritative drop/telemetry signal. Здесь же production definition и регистрация PICKUP-001, ordinary-enemy drop bindings и world/UI art зелья через готовый IP-28. Healing/drop/radius/lifetime values должны быть заполнены по принятым правилам; passive/set modifiers применяются через общий reward contract.

## Out of Scope

final schedules, Travelers как переименованные обычные враги, самостоятельный rebalancing.

## Acceptance criteria

каждый ID создаётся по данным и демонстрирует карточку; death фиксируется один раз, despawn не выдаётся за kill/drop; enemy field pass-through сохраняется; pooled reuse очищает status, registry, visual state. Body motion не деформирует root/collider. PICKUP-001 загружается по stable ID, выдаёт указанное лечение, использует согласованные drop tables и approved runtime art. Production значения не подменяются fixture tuning; где в карточке нет чисел, packet остаётся с явно указанным gap.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

читаемая attack preparation и отличия role/silhouette; DEV показывает ID/pattern/effects; без ненужного отдельного HUD на каждого обычного врага.

## Проверки

per-ID smoke, melee/ranged/dash, hit/miss/lifetime, drop source, pause/end/pool; density/performance measurement и manual silhouette при толпе. Potion per-ID load/roll/heal, modifier integration и kill-versus-despawn reward check.

## Документационные изменения

numeric card completeness, body/projectile family reuse mapping, telemetry origin и test evidence. PICKUP-001 data completeness, drop eligibility/bindings и image provenance входят в evidence этого IP.

## Gates и недостающие решения

G-10/G-14: contact intervals, недостающие attack/drop/healing values и pickup lifecycle; AG-01 для конкретных картинок. Approved design не означает complete JSON. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
