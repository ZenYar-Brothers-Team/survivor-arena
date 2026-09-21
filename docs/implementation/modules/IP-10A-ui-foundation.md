# IP-10A — UI Foundation, reusable cards, HUD и test harness

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Расширить existing UI Toolkit/ViewState/presenter, не создать вторую foundation. Recipe/character semantic data поставляют IP-11/IP-12; foundation проверяется их fake snapshots.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-03](IP-03-character-stats.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-10](IP-10-reroll-banish.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

UI §§1,6–10,14,19–23; GDD только отображаемые run/HP/XP/build/draft rules; DECISION-0005; GameplayUiPresenter/ViewState/UXML/USS, semantic ID tests.

## Scope

Reusable DraftCard/ContentCard/details, icon references, current→next-level/effect text, recipe projection rendering contract, normal/hover/pressed/disabled/selected/locked states. Elapsed HUD 00:00→15:00, compact 6+6 icons/acquired sets, Pause/Build layout, nonblocking notifications. Fixture states including Book/sets/locked do not require full feature implementation. Preserve bounded collapsed DEV and lightweight changed-state rebuild.

## Out of Scope

Gameplay rule ownership, final image generation, settings services/full navigation, compendium/advanced stats/complex transitions.

## Acceptance criteria

View не вычисляет gameplay eligibility/recipes. Renderer корректен для 0/1/2/3 cards, empty/max/long-text states; projected≠current и fulfilled≠acquired различаются. Click не выдаёт два intent. HUD tooltip не перехватывает movement input; в Draft/Build действует обычная pause policy. Детали не скрывают обязательный выбор. Semantic IDs стабильны или мигрированы вместе с assets/tests. DEV gated и bounds≤25%×45% reference viewport.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Сам production HUD/draft/pause layout плюс fake-state gallery/harness. IP-11 реально наполняет recipe projection, IP-12 — selection, IP-15/16/25/29 — свой vertical UI; IP-26 связывает экраны.

## Проверки

Presenter fake model/view, UXML/USS IDs, PlayMode geometry/focus/queued drafts; manual 1920×1080 и минимальный поддерживаемый resolution/long labels. Projections не меняют build.

## Документационные изменения

Component/state/semantic contracts, approved UI section links, dependency consumers и tests; .claude UI rules remain.

### Контракт компонентов

- `ContentCardViewState` / `ContentCard`: title, summary, details, optional resolved Sprite, enabled/selected/locked. Без icon используется shape placeholder; production icons поставляют owning content IP.
- `DraftOptionViewState` / `DraftCard`: effect/current→next text, set marker/free slot и immutable ordered `RecipeProjectionViewState`. Producer задаёт current/projected/required, completes/acquired и готовые component/threshold строки с выделением текущего option. Renderer показывает первые два рецепта, `+N more` и полный список в details; сортировка и gameplay projection принадлежат IP-11.
- `BuildSlotViewState` / `SetBuildViewState`: compact HUD references и подробности Pause/Build. `SetRecipeProgressViewState.HasProgress` позволяет показывать owned component ниже threshold даже при `FulfilledComponents == 0`; IP-11 supplies semantics. Acquired list отделён от progressed unacquired recipes.
- `UiNotification`: один nonblocking slot с заменой сообщения и expiry по pause-aware delta. Event selection — producer; foundation связывает level-up/set acquisition и проверяет остальные тексты fake events.
- Build/character snapshots сохраняют элементы при неизменных данных. Draft revision остаётся authority для пересборки карточек; Banish mode обновляет их в рамках той же revision.
- HUD slots не focusable. Детали draft находятся под тремя позициями в отдельной scroll area; Pause/Build — grid 6+6 внутри scroll с отдельной Resume.
- Новые semantic IDs: `card-icon/title/summary/status/more`, `card-recipe-{index}` (локальны внутри card), `draft-details`, `pause-build`, `pause-character`, `hud-notification`. Прежние draft/slot/control IDs сохранены. USS resource — `UI/GameplayUiStyles`, чтобы не выбирать встроенный StyleSheet subasset `GameplayUi.uxml`.
- `UiFoundationTests` и `UiFoundationSmokeTests` — fake-state harness для 0/1/2/3, empty/max/long labels, Book/set/projection/locked/selected. Проверка layout выполняется при 1920×1080 и 1280×720; последний — lower test viewport, не новый product minimum.

## Gates и недостающие решения

G-01/G-03 short/book states определены DECISION-0019/0020 и поставляются владельцем IP-07; foundation сохраняет три позиции, origin, очередь и начисленную валюту. IP-10 поставляет banish mode/cancel, control hints и revision reset (DECISION-0022); reusable cards сохраняют эти presenter intents. Baseline-relative character filtering — IP-12. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
