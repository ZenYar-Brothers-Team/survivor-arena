# IP-24 — Canonical Wave / Encounter Content и field bindings

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-14](IP-14-wave-director.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-23](IP-23-production-fields.md), [IP-29](IP-29-traveler-framework.md), [IP-30](IP-30-production-travelers.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
24 фазы на 900 секунд, 6-type composition, caps/bursts и hooks.
Это Proposed packet по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
использовать как production data только после approval, затем выполнить проверки этого IP.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD run/waves/fields/Travelers; Content Wave / Encounter Content и selected FIELD/ENEMY/BOSS/MIDBOSS/TRAVELER cards; IP-14 burst и IP-29 Traveler contracts; DECISION-0014.

Использовать [IP-16 field configuration](IP-16-field-framework.md#framework-api-и-fixture-schema):
timeline enemy refs входят в field pool, final/mid hooks совпадают с definitions.
Смена поля создаёт fresh director/encounter state; global default timeline не подменяет
выбранный field. Optional Traveler token требует consumer IP-29 и полного schedule packet.

## Scope

15-minute field schedules: continuous/burst composition, pressure/rest phases, rates/counts/caps, final/midboss timings и Traveler schedule/scaling. Regular-wave и Traveler policies остаются раздельно видимыми. Балансировочное предложение создаёт IP-32; в production попадает его конкретно принятый вариант.

## Out of Scope

молчаливое заполнение пустых schedules, adaptive difficulty/автоматический AI rebalance, фиксация FPS-target без измерения.

## Acceptance criteria

каждый in-scope field имеет законченный, валидируемый encounter config; невозможно объявить каталог завершённым по одному пилотному полю. Pressure/rest, burst и Traveler events исполняются по данным; final boss появляется к заданному времени; пауза не сдвигает run-time schedule; capped/omitted spawns диагностируются.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

phase/timer HUD, boss/Traveler vertical slices, DEV timeline со scheduled и actual events; IP-31 получает поле/phase/source и фактическую нагрузку.

## Проверки

accelerated boundary timeline, same-time events/caps/catch-up, reference/number validation; реальный 15-minute run representative fields и per-field targeted smoke; repeated seed comparison без обещания deterministic physics replay.

## Документационные изменения

Использовать [IP-14 runtime/schema](IP-14-wave-director.md#runtime-и-fixture-schema):
явные seed/mode, count и полуоткрытое burst window; expired/suppressed не превращаются
в deferred очередь. Producer `SpawnResolved` отражает actual и причины пропусков.
Fixture groups 18/26/34 и spawn-only load bound не задают production числа/FPS budget.

versioned encounter data и rationale принятого баланса, field bindings, run evidence; отделить proposal чисел от утверждённого config.

## Gates и недостающие решения

CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Traveler policy update

[DECISION-0035](../../decisions/0035-traveler-encounter-rules.md) заменяет полный
spawn interval на [0,T−120], определяет выбор без повторов, две высоты экрана от
игрока и HP/damage-only scaling. Простые support interactions заданы тем же решением.
Production presence/XP/support/attack numbers, field pools и per-ID art остаются
content gates; эти данные не подменяются synthetic framework fixtures.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-08](../milestones/FIELD-001-start.md#f1-08):
FIELD-001 900-second schedule и startup bindings. Packet prerequisites: F1-00…07; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).

Поправка [DECISION-0052](../../decisions/0052-field001-six-ordinary-enemies.md):
ordinary pool FIELD-001 — ENEMY-001…005 и ENEMY-007, ровно шесть типов.
F1-04 поставляет их definitions/art; F1-08 связывает все шесть с timeline,
F1-09 проверяет совместную читаемость и давление. Боссы/Путники считаются отдельно.
