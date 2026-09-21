# IP-19 — Production Sets SET-001…020

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-11](IP-11-set-framework.md), [IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-28](IP-28-world-pickups.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD «Сеты»/draft; полные выбранные SET-001…020 и компоненты рецептов; UI §§8–10; Art Direction §12.1, Art Production §8.

## Scope

двадцать точных recipes/thresholds, effects и source/proc rules, icons и только необходимые дополнительные visual roles. Skill transformations, buffs и отдельные set-attacks подключаются к соответствующим IP-11 contracts, а не все трактуются как независимый projectile.

## Out of Scope

новые рецепты/эффекты вне утверждённых документов, отдельный set progression/rarity, постоянный VFX spam ради отличия.

## Acceptance criteria

recipe fulfilled ≠ acquired; общий setDraftChance/priority принадлежат IP-07/IP-10/IP-11 и не дублируются per-card weight. Set не занимает active/passive slot и не получает уровень; shared components и несколько sets работают вместе. Non-recursion/source inheritance соблюдаются. Неполный proc payload или внутреннее противоречие выбранной карточки блокирует её конкретный effect, а не весь каталог.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

progress/threshold detail, completes-recipe projection, distinct set card, acquired effect summary; source/proc counters только DEV/telemetry. Внешний вид set effect остаётся вторичным по отношению к active skills.

## Проверки

per-recipe truth table, acquisition/duplicate, per-effect smoke, same components/multiple sets, proc boundaries и no recursion; manual сочетание 3–4 sets на реальном масштабе.

## Документационные изменения

range до 020, уточнение «extra abilities» до полного approved набора effects, resolved card gaps и effect/asset/test mapping.

## Gates и недостающие решения

G-02 закрыт DECISION-0022. G-04/G-05/G-08/G-13: recipes/effects approved, но thresholds/proc payload и два внутренних конфликта требуют закрытия. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт потребления IP-11

Использовать `SetDefinition.Effects`, keyed `SetEffectAbility`/`ISetEffectHost` и единый `draft.setDraftChance`; per-set probability не возвращать. JSON schema/compatibility matrix — [IP-11](IP-11-set-framework.md#реализованный-framework-contract). Source/ownership — [DECISION-0025](../../decisions/0025-set-effect-source-and-ownership.md).

Fixtures доказывают семейства, а не точные двадцать production payloads. При переносе каждого ID подключить его специальные параметры/условия к reusable effect executor, проверить no recursion, modifier applicability, single-entity caps и реальный масштаб; не считать generic damage/size bonus реализацией chain targets, return phase, slowed-target aura или projectile replacement. Real potion event связывается с IP-28 после принятого pickup, не с любым heal callback.
