# IP-23 — Production Fields FIELD-001…010

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-16](IP-16-field-framework.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
200×200 geometry, 64 obstacle rects и metadata.
Packet Approved 2026-09-24 по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
используется как production data; проверки этого IP сохраняются.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD/CD fields, полные выбранные FIELD-001…010 и referenced entities; UI §5; Art Production §§11–13; DECISION-0003.

Field/run configuration API — [IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema).
Production environment adapter заменяет fixture scene-name binding; metadata/refs
проходят тот же pre-run validation. G-20 resolved по DECISION-0038: переносить
explicit difficulty 1–5 из CD; автоматического mapping по ID нет.

## Scope

десять geometry/environment definitions, approved enemy/boss/midboss mapping и unlock/difficulty metadata; ground treatment, нужный decor/obstacle pack и derived thumbnail. Число obstacles определяется gameplay geometry; декоративные props не получают collider автоматически.

## Out of Scope

самостоятельные wave schedules, заранее фиксированное число препятствий на поле, сложный tileset без необходимости.

## Acceptance criteria

selected field загружает correct geometry/environment; ordinary boundaries/obstacles блокируют только игрока; references валидны. Difficulty 1–5 задана явно; thumbnail отражает поле; безопасное направление движения читается, props не маскируют опасности. Неполная geometry/size/card data отмечена per-field.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

name/description/difficulty/lock condition/thumbnail + loading state; технические wave timings/boss stats не перегружают selection.

## Проверки

per-field load/selection/ref validation и collision, restart/reload cleanup; manual density/contrast на реальном camera scale и thumbnail review.

## Документационные изменения

field→kit→roles mapping, geometry constraints, metadata completeness, bindings владельцев IP-24/IP-30.

## Gates и недостающие решения

G-14: geometry/enemy pools; G-20 resolved по DECISION-0038 (difficulty 1–5). G-15 resolved по DECISION-0037; unlock conditions берутся из CD. Весь approved mapping переносится, numeric schedules отдельно. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-08](../milestones/FIELD-001-start.md#f1-08):
FIELD-001 geometry/environment/metadata/thumbnail. Packet prerequisites: F1-00…07; framework prerequisites из раздела
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
