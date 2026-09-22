# IP-18 — Production Passive Items PASSIVE-001…014

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-28](IP-28-world-pickups.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD stats/XP/skills; полные выбранные PASSIVE-001…014 cards; UI §§6–10; Art Production §7. Effects/stacking rules берутся из IP-03/IP-05/IP-09/IP-13 и существующей stat composition.

## Scope

четырнадцать six-level definitions, новые supported channels, icon/label/effect delta. Постоянные world auras и world pickup sprites для passive предмета по умолчанию не производятся.

## Out of Scope

собственная скрытая stat formula в пассивке, новые постоянные world effects, самостоятельный balance redesign.

## Acceptance criteria

каждый уровень даёт указанное изменение; same-key replacement/recompute не накапливает bonus. Action speed UI соответствует формуле, Max HP сохраняет ratio, damage-reduction cap и остальные сохранённые правила проверены. Missing level/число/channel выявляются валидатором.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

6 passive slots, icons/levels и readable changes; соответствующие current stats видны в Pause/Build, dynamic effects — через актуальные snapshots.

## Проверки

per-ID L1…6, stacking/replacement/removal, threshold boundaries, heal/XP/action-speed integration, pause/reset; presenter deltas и manual icon slot readability.

## Документационные изменения

range до 014, stat/effect mapping, numeric completeness и asset/test references.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017; G-10 semantics — DECISION-0033, implementation consumer — IP-28. Полные значения 14 passives остаются gate; отсутствие конкретного runtime parameter не заполняется hidden default. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт framework для production mappings

Использовать [таблицу IP-09](IP-09-passive-framework.md#stat-applicability-и-владельцы-defaults). PASSIVE-007 задаёт только `pickupRadiusMultiplierBonus`; generic lifetime остаётся технической возможностью, без production item. PASSIVE-002 одновременно задаёт regeneration и относительный potion multiplier; cap/roll проверяет IP-28. Все L1…L6 — итоговые значения, заменяющие keyed modifier. Feature preview и slot descriptions уже читают эти каналы; production icons/definitions и per-ID verification поставляются здесь.

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-02](../milestones/FIELD-001-start.md#f1-02):
PASSIVE-001…005/007…009/011/012. Packet prerequisites: F1-00/01; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).
