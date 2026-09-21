# IP-23 — Production Fields FIELD-001…010

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-16](IP-16-field-framework.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD/CD fields, полные выбранные FIELD-001…010 и referenced entities; UI §5; Art Production §§11–13; DECISION-0003.

Field/run configuration API — [IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema).
Production environment adapter заменяет fixture scene-name binding; metadata/refs
проходят тот же pre-run validation. G-20: до переноса сложности решить расхождение
шкалы UI/IP 1–5 и CD 1–10; автоматического mapping по ID нет.

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

G-14/G-15: geometry/enemy pools/difficulty и unlock conditions. Весь approved mapping переносится, numeric schedules отдельно. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
