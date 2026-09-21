# IP-12 — Character definitions, weighted draft и selection presentation

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить roster/starting skill/unlocked selection/seeded ordinary weights; расширить definitions/stat/UI mapping нового roster.

## Зависимости

[IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Персонажи», slots; Content CHAR-001…010 + starting SKILL references, UI §§4,23; CharacterDefinition/Roster/FixtureCharacterDefinitionCatalog.

## Scope

Validated base stats/new channels, starting slot reference, per-skill weights including zero; unlocked roster API и lock reason presentation supplied from profile provider. Body/crop/icon metadata and concise baseline-relative modifiers. Character species не ограничивается goblin, no unique passive system. Profile persistence — IP-25, framework использует explicit fake unlocked set.

## Out of Scope

Production10 characters/art, финальные prices/unlocks/weights если отсутствуют, новая character passive system.

## Acceptance criteria

Два fixtures различаются stats/start skill/weights; zero-weight никогда не выпадает ordinary sampling. Starting skill занимает 1 из 6. Locked невозможно запустить, но UI может показать locked card. Baseline и критерий significant modifier заданы, не выводятся произвольно из первого персонажа; no full internal stat dump in selection. Reinit не удерживает previous character stats.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Player-facing selection fixture со starting skill/role/crop/modifiers и lock condition, без ожидания full navigation IP-26.

Переиспользовать `ContentCardViewState` / `ContentCard` IP-10A: resolved icon, concise summary/details, locked/selected/enabled. Baseline-relative modifier summary и lock condition поставляет IP-12; renderer не выбирает baseline.

## Проверки

Loadout/weights deterministic, locked selection, stat overlay/removal, new-field validation; presenter locked/unlocked/baseline summary и PlayMode selection→correct initial loadout.

## Документационные изменения

Character schema/selection contract; changed names/CHAR-006 species в Content; API link IP-22/IP-25/IP-26.

## Gates и недостающие решения

G-14/G-15: numeric weights, unlock completion semantics и baseline display metadata при незаполненности; renderer не придумывает их. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-12A](IP-12A-visual-presentation-foundation.md), [IP-16](IP-16-field-framework.md), [IP-22](IP-22-production-characters.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
