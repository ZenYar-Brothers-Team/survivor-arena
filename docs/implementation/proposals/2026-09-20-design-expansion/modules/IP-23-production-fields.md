# IP-23 — Production Fields FIELD-001…010

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-23-production-fields.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-16](IP-16-field-framework.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новые GDD/CD fields, полные выбранные FIELD-001…010 и referenced entities; UI §5; Art Production §§11–13; DECISION-0003.

## Scope

десять geometry/environment definitions, approved enemy/boss/midboss mapping и unlock/difficulty metadata; ground treatment, нужный decor/obstacle pack и derived thumbnail. Число obstacles определяется gameplay geometry; декоративные props не получают collider автоматически.

## Out of Scope

самостоятельные wave schedules, заранее фиксированное число препятствий на поле, сложный tileset без необходимости.

## Acceptance criteria

selected field загружает correct geometry/environment; ordinary boundaries/obstacles блокируют только игрока; references валидны. Difficulty 1–5 задана явно; thumbnail отражает поле; безопасное направление движения читается, props не маскируют опасности. Неполная geometry/size/card data отмечена per-field.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

name/description/difficulty/lock condition/thumbnail + loading state; технические wave timings/boss stats не перегружают selection.

## Проверки

per-field load/selection/ref validation и collision, restart/reload cleanup; manual density/contrast на реальном camera scale и thumbnail review.

## Документационные изменения

field→kit→roles mapping, geometry constraints, metadata completeness, bindings владельцев IP-24/IP-30.

## Gates и недостающие решения

G-14/G-15: geometry/enemy pools/difficulty и unlock conditions. Весь approved mapping переносится, numeric schedules отдельно. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
