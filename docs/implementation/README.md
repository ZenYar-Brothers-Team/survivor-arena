# Implementation Plan

Implementation Plan задаёт порядок, границы и критерии реализации. Он построен по двум каноническим design-артефактам и не пересказывает их полностью:

- [`../Game_design.md`](../Game_design.md) — системные правила игры;
- [`../Content_design.md`](../Content_design.md) — конкретные сущности и параметры;
- [`modules/`](modules/) — спецификации модулей реализации;
- [`STATUS.md`](STATUS.md) — единственный источник текущих статусов и evidence;
- [`WORKFLOW.md`](WORKFLOW.md) — обязательный процесс превращения IP в код.

Repository/code показывает фактическое состояние реализации, но не переопределяет продуктовые документы автоматически.

## Модель исполнения

Каждый IP-модуль является отдельной единицей scope. Coding AI читает его спецификацию, указанные в ней секции Game Design и Content Design, правила workflow, актуальный статус и только релевантную часть репозитория.

Если пользователь просит «следующую часть», выбирается первый по номеру модуль со статусом `Ready` в [`STATUS.md`](STATUS.md). Модуль, явно названный пользователем, имеет приоритет, но до реализации всё равно проверяются зависимости и content gates.

Приоритет источников:

1. Game Design определяет системные правила.
2. Content Design определяет конкретные сущности и их параметры.
3. IP-модуль определяет scope, зависимости и acceptance criteria текущей работы.
4. Repository/code определяет фактическое техническое состояние.

Если источники конфликтуют, применяется процесс из [`WORKFLOW.md`](WORKFLOW.md), а не молчаливый выбор coding AI.

## Техническая предпосылка

Проект использует Unity/C#. Core systems должны поддерживать стабильные Content Design ID и data/config-driven параметры. Display name не является идентификатором. Неопределённые балансные значения не должны превращаться в hard-coded системные правила.

## Майлстоуны

### M0 — Foundation

IP-00…IP-01. Загрузка определений по стабильным ID и жизненный цикл забега.

### M1 — Playable Core

IP-02…IP-07. Fixture-персонаж на fixture-поле перемещается, получает урон, автоматически атакует, убивает врагов, собирает XP и выбирает развитие.

### M2 — Build Systems

IP-08…IP-12. Active/passive frameworks, reroll/banish, sets и characters.

### M3 — Encounter Systems

IP-13…IP-16. Enemy patterns, Wave Director, bosses/mid-bosses и fields.

### M4 — Canonical Content Integration

IP-17…IP-24. После approval подключается production content по стабильным ID.

### M5 — Meta & Player Flow

IP-25…IP-26. Сохранение, разблокировки, мета-прогресс и функциональный UI-flow.

### M6 — Integration

IP-27. Полный end-to-end run и meta loop проверены совместно.

## Content gates

### CG-01 — Approval

AI-generated catalog сейчас имеет статус Draft. Это не блокирует framework IP-00…IP-16 и framework-части IP-25/IP-26, но production IP-17…IP-23 не могут превращать Draft в канон без явного approval пользователя.

### CG-02 — Wave / Encounter Content

Раздел Content Design пока пуст, поэтому IP-24 не может создавать production schedules самостоятельно.

### CG-03 — Meta economy

Permanent upgrades, цены, награды и часть unlock conditions остаются TBD. IP-25 может реализовать framework с явно помеченными fixtures/placeholders, но не финальную экономику.

### CG-04 — Balance knobs

Draft offer count, reroll/banish counts, XP thresholds/lifetime, set probabilities и точный final-boss spawn time остаются конфигурируемыми TBD.

## Каталог модулей

- [IP-00 — Контракт контента, стабильные ID и конфигурация](modules/IP-00-content-contract.md)
- [IP-01 — Run lifecycle, таймер, pause и завершение](modules/IP-01-run-lifecycle.md)
- [IP-02 — Перемещение игрока и базовая геометрия поля](modules/IP-02-player-movement.md)
- [IP-03 — Character stats, HP, damage, healing и regeneration](modules/IP-03-character-stats.md)
- [IP-04 — Enemy core](modules/IP-04-enemy-core.md)
- [IP-05 — Active skill runtime и player damage pipeline](modules/IP-05-active-skill-runtime.md)
- [IP-06 — XP drops, pickup, expiry и level progression](modules/IP-06-xp-progression.md)
- [IP-07 — Level-up draft, 6+6 slots и base build progression](modules/IP-07-level-up-draft.md)
- [IP-08 — Active-skill progression и pattern framework](modules/IP-08-active-skill-framework.md)
- [IP-09 — Passive modifier framework](modules/IP-09-passive-framework.md)
- [IP-10 — Reroll и banish](modules/IP-10-reroll-banish.md)
- [IP-10A — UI Foundation and test harness](modules/IP-10A-ui-foundation.md)
- [IP-11 — Set framework](modules/IP-11-set-framework.md)
- [IP-12 — Character framework и weighted draft](modules/IP-12-character-framework.md)
- [IP-13 — Enemy movement и attack patterns](modules/IP-13-enemy-patterns.md)
- [IP-14 — Wave Director](modules/IP-14-wave-director.md)
- [IP-15 — Boss/mid-boss framework](modules/IP-15-boss-framework.md)
- [IP-16 — Field definitions и run configuration](modules/IP-16-field-framework.md)
- [IP-17 — Production Active Skills](modules/IP-17-production-skills.md)
- [IP-18 — Production Passive Items](modules/IP-18-production-passives.md)
- [IP-19 — Production Sets](modules/IP-19-production-sets.md)
- [IP-20 — Production Enemies](modules/IP-20-production-enemies.md)
- [IP-21 — Production Bosses и Mid-bosses](modules/IP-21-production-bosses.md)
- [IP-22 — Production Characters](modules/IP-22-production-characters.md)
- [IP-23 — Production Fields](modules/IP-23-production-fields.md)
- [IP-24 — Canonical Wave / Encounter Content](modules/IP-24-production-waves.md)
- [IP-25 — Persistent profile и meta progression](modules/IP-25-meta-progression.md)
- [IP-26 — Functional UI и полный player flow](modules/IP-26-functional-ui.md)
- [IP-27 — End-to-end integration](modules/IP-27-integration.md)

## Готовность плана

План покрывает текущие системные правила и отделяет framework от production content. Изменение параметров сущности не требует переписывать IP, пока не меняются её system contract, зависимости или module ownership.
