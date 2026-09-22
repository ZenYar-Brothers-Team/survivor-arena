# IP-00 — Контракт контента, стабильные ID и конфигурация

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить реализованный registry, typed references и validating catalogs. Меняется Context; новых acceptance requirements к registry этот пересмотр не добавляет.

## Зависимости

Готовый Unity/C# repository и действующие repository instructions.

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Статус и область документа»; Content «Статусы и каноничность», «Идентификаторы», «Числа и отдельная таблица баланса», «Связь с Implementation Plan и Code»; DECISION-0009/0012; JsonContentFile, ContentRegistry.

## Scope

Стабильные IDs и type-safe ссылки; duplicate/missing/wrong-type validation до gameplay; fixture/production definitions используют общий контракт; JSON authoring остаётся в одном Content-дереве. Новые schemas реализует владелец feature, не второй content registry.

## Out of Scope

Gameplay systems, production definitions, внешний balance Sheet, version field в content JSON без обоснования.

## Acceptance criteria

Проверенные duplicate/missing/wrong-type/invalid cases продолжают соответствовать текущему API. Новые approved IDs не обязывают generic registry знать их названия. Переименование display name не меняет identity.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Отдельный player-facing surface не нужен; bootstrap errors содержат ID, category и missing field.

## Проверки

Сверить существующие ContentRegistry/JsonContentFile tests с текущим контрактом. Одно изменение Context не требует запуска всего Unity suite; при API/schema change — affected tests владельца.

## Документационные изменения

Ссылки и approval каталога синхронизированы при M-01; сохранить историческое evidence при следующих изменениях. Semantic ID migrations описать отдельно.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-01](IP-01-run-lifecycle.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
