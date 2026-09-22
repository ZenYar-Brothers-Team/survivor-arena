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


Контракт потребления skill stats и JSON L1…L6: [IP-08 parameter mapping](IP-08-active-skill-framework.md#контракт-параметров-для-потребителей), [DECISION-0021](../../decisions/0021-additive-skill-level-bonuses.md). Size/range применяются один раз executor-ом из activation snapshot; passive definitions не переписывают skill level data. Повторные проценты skill upgrades складываются к базе.

## Документационные изменения

Stat applicability/default ownership table, PASSIVE-007 semantic migration note; extend Content JSON/catalog tests; IP-18.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017. Potion cap G-10 утверждён DECISION-0033: actual drop roll IP-28 применяет multiplier к выбранной базе и ограничивает итог 100%; multiplier channel не выполняет roll. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-18](IP-18-production-passives.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Stat applicability и владельцы defaults

JSON уровня хранит **итоговый bonus**, не сумму уровней. Catalog → `CharacterStatModifier` → keyed `CharacterStats` → существующий consumer. Отсутствующее поле означает neutral bonus 0, определённый `CharacterStatModifier`; базовый multiplier 1 принадлежит `CharacterBaseStats`. Параметры fixture не объявляют production ID или баланс.

| Compatibility target | Поля JSON modifier | Consumer / fixture |
|---|---|---|
| PASSIVE-001 | `maxHealthMultiplierBonus` | Health с сохранением HP ratio; VITALITY |
| PASSIVE-002 | `healthRegenerationPerSecondBonus`, `potionDropMultiplierBonus` | Health regeneration; multiplier для IP-28; COLLECTOR |
| PASSIVE-003 | `movementSpeedMultiplierBonus` | Player movement; HASTE |
| PASSIVE-004 | `activeSkillDamageMultiplierBonus` | Attack snapshot; HASTE |
| PASSIVE-005 | `actionSpeedBonus` | Cooldown duration = base / (1 + bonus); HASTE |
| PASSIVE-006 | `disappearingXpRecoveryBonus` | Expiry recovery, cap 1; MEMORY |
| PASSIVE-007 | `pickupRadiusMultiplierBonus` | Existing/new XP drops, world units; PICKUP-RADIUS |
| PASSIVE-008 | `incomingDamageReductionBonus` | Incoming damage, cap 0.99; MEMORY |
| PASSIVE-009 | `healthRestorationMultiplierBonus` | Healing и regeneration; MEMORY |
| PASSIVE-010 | `pickedUpXpMultiplierBonus` | Physical pickup only; MEMORY |
| PASSIVE-011 | `knockbackResistanceBonus`, `outgoingKnockbackBonus` | Incoming resistance cap 1, outgoing source before target resistance; KNOCKBACK |
| PASSIVE-012 | `effectSizeMultiplierBonus` | IP-08 radius/width/hit size only; SIZE |
| PASSIVE-013 | `effectRangeMultiplierBonus` | IP-08 targeting/travel/lifetime mapping only; RANGE |
| PASSIVE-014 | `lowHealthDamageMaxBonus` | Dynamic HP curve → next attack snapshot; LOW-HEALTH |

Все fixture ID имеют префикс `FIXTURE-PASSIVE-`; девять definitions покрывают все каналы, а не регистрируют четырнадцать production items. MEMORY сохраняет generic `xpDropLifetimeBonusSeconds` для совместимости старого fixture; это **не** mapping нового PASSIVE-007. Его единственный production mapping — pickup radius; исторические lifetime данные не переинтерпретируются как новый предмет. Production definitions/icons принадлежат IP-18.

Проценты задаются долями (0.6 = +60%), regeneration — HP/s, lifetime — секунды. Pickup radius = base radius × (1 + сумма bonus): база 2 и +60% дают 3.2 world units. Potion chance consumer получает multiplier: 0.05 × 1.6 = 0.08; actual roll/cap поставляет IP-28 по DECISION-0033. Low-HP multiplier = 1 + max bonus × min(1, (1 − HP/maxHP) / 0.9): max bonus 0.7 при 55% HP даёт 1.35, при 10% и ниже — 1.7.

`PlayerPassiveSetRuntime.Initialize` освобождает предыдущие modifiers/subscription/catalog перед повторной сборкой. Shutdown и rollback удаляют только собственные ключи; build slots остаются неизменны. Удаление технического modifier не разрешает игроку освобождать слот.

UI: feature-owned current/next preview сохраняется в draft; шесть существующих semantic passive slot IDs показывают итоговые значения через tooltip. LOW-HEALTH дополнительно показывает текущий агрегированный коэффициент из immutable `CharacterStatsViewState`, обновляемый по HP/stat events и доступный также без DEV. Новых UXML/USS элементов не требуется; internals остаются в существующей DEV панели.
