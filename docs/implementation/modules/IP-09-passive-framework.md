# IP-09 — Passive modifiers и новые stat effects

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Доработать действующий keyed-modifier framework до полного набора 14 passives. Повторно не проектировать Health/stat composition.

## Зависимости

[IP-03](IP-03-character-stats.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD stats/XP/combat; PASSIVE-001…014 как compatibility matrix; CharacterStats/PlayerPassiveSetRuntime/FixturePassiveCatalog; DECISION-0004/0009.

## Scope

Mappings существующих ten channels и новых knockback, pickup radius, potion chance, size/range, low-HP modifier; per-level final values, keyed replace/remove/recompute. Перейти от старого PASSIVE-007 lifetime смысла к target pickup-radius в production mappings, сохранив generic lifetime capability без обязательного production item. Potion multiplier предоставляется потребителю, actual roll IP-28.

## Out of Scope

Production14 definitions/icons, реализация potion drop, дополнительная aura ради UI, автоматический rebalance.

## Acceptance criteria

Percent stacking/additive caps remain; action speed duration не интерпретируется как direct −%. Size/range направляются только в согласованные IP-08 parameters; low-HP modifier не snapshot навечно при acquisition. Повторный Apply/Initialize не накапливает эффект; remove/Shutdown возвращает baseline. XP radius изменяет уже существующие drops. Relative potion example5%×1.6=8%, не +60 percentage points.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Six passive slots и effective stat descriptions; feature-owned current/next-level deltas, dynamic snapshot после HP change; необязательные full internals только DEV.

## Проверки

Representative fixture каждого channel, 1→6, replacement/removal/rollback, cooldown/HP ratio/recovery/pickup/size/range interaction, low-HP threshold и neutral defaults; actual potion integration — IP-18/IP-28.

## Документационные изменения

Stat applicability/default ownership table, PASSIVE-007 semantic migration note; extend Content JSON/catalog tests; IP-18.

## Gates и недостающие решения

G-08/G-09 и potion cap G-10 блокируют только зависимые mappings. Утверждённые formulas и final level values не пересогласуются. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-18](IP-18-production-passives.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
