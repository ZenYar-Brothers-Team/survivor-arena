# DECISION-0036 — Traveler encounter и source-owned protection

Status: Proposed
Date: 2026-09-21
Related IP: IP-08, IP-13, IP-16, IP-24, IP-26, IP-27, IP-28, IP-29, IP-30, IP-31

## Context

Approved product policy: [DECISION-0035](0035-traveler-encounter-rules.md).
Нужны temporary actors трёх ролей, независимое расписание, обычные combat targets,
source-owned support, Book и вертикальный UI/telemetry slice.

## Implementation record

`Game.Traveler` владеет catalog, schedule draw, placement, role steering и encounter
lifetime. `TravelerScheduleDefinition` расширяет field-owned token без зависимости
Field → Traveler; ContentRegistry проверяет реальные typed references. Bootstrap
связывает конкретный schedule payload поля, общий pickup runtime и XP sink.

Actors используют pooled `EnemyRuntime` с категорией Traveler. Existing EnemyRegistry
и active-skill target adapters видят их без отдельного target pipeline. Offensive
профили используют Enemy movement/attack controllers; мирные роли подключаются через
`IEnemyMovementDriver`. Encounter guard проверяется перед movement/attack/contact и
incoming damage: frame order не разрешает атаку или награду после deadline.

`EnemyProtection` в Game.Enemy хранит source-keyed aura maxima и один shield.
Damage pipeline: обновление expiry → controls с effective resistance → reduction →
shield → Health. CombatResult сохраняет original requested damage и actual Health
loss. Game.Enemy не зависит от Traveler. Owner снимает support при death/escape/
cleanup; source deadline также ограничивает shield и aura независимо от update order.

Placement использует player-reachable axis-aligned fixture geometry с clearance не
меньше максимального Traveler radius. Rejection sampling выбирает случайное направление
на окружности фиксированного радиуса, без clamp distance; configured attempts bounded.
Role movement и внешнее смещение проектируются в доступную область. Это fixture
adapter, не новая общая navigation/collision система.

UI читает immutable snapshots через presenter, показывает HP и off-screen arrows,
отводя разные внутренние линии для одновременных pointers. Dev intent находится в
Build tab существующего drawer. Telemetry подписывается на life/combat events и
экспортирует schedule, фактический scale/HP и plain x/y вместо рекурсивного Unity Vector2.

## Consequences

Восемь synthetic fixtures покрывают contact/dash/cross offensive, wander/avoid/rest,
guard aura, resistance aura и shield. Цветовые placeholders и текстовые метки не
являются production art. Production TRAVELER-001…010, Book ID/art и balance-data
остаются IP-30/IP-24. Это Proposed technical record, не новое product approval.
