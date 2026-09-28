# IP-10A — UI Foundation, reusable cards, HUD и test harness

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Расширить existing UI Toolkit/ViewState/presenter, не создать вторую foundation. Recipe/character semantic data поставляют IP-11/IP-12; foundation проверяется их fake snapshots.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-03](IP-03-character-stats.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-10](IP-10-reroll-banish.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

UI §§1,6–10,14,19–23; GDD только отображаемые run/HP/XP/build/draft rules; DECISION-0005/0081/0083; GameplayUiPresenter/ViewState/UXML/USS, semantic ID tests. Для layout review — [предложение R2](../proposals/2026-09-28-ui-layout-r2.md), не источник новых утверждённых product rules.

## Scope

Reusable DraftCard/ContentCard/details, icon references, current→next-level/effect text, recipe projection rendering contract, normal/hover/focus/pressed/disabled/selected/locked states. HUD countdown 15:00→00:00 по DECISION-0069, player HP возле персонажа по DECISION-0083, compact 6+6 icons/acquired sets, Pause/Build layout, nonblocking notifications. Fixture states including Book/sets/locked do not require full feature implementation. Preserve bounded collapsed DEV and lightweight changed-state rebuild.

Скорость 1×/2×/3×/5× — только DEV drawer в Editor/Development Build, через
presenter intent и run owner. Выбор действует только во время Running,
сохраняется через pause, возвращается к 1× после завершения/выхода и при новом
забеге. Non-development UI не показывает controls и не принимает speed intents.

Layout и информационная иерархия проектируются отдельно от темы. Поддержать
краткую основную подачу и доступ к компонентам рецептов по
[DECISION-0086](../../decisions/0086-ui-review-density-and-inspection.md):
inspect отдельно от confirm, список связанных сетов и выбранный рецепт снизу,
компактный билд и расширенные рецепты Pause. Старые размеры не являются контрактом.

## Out of Scope

Gameplay rule ownership, final image generation, settings services/full navigation, compendium/advanced stats/complex transitions.

## Acceptance criteria

View не вычисляет gameplay eligibility/recipes. Renderer корректен для 0/1/2/3 cards, empty/max/long-text states; projected≠current и fulfilled≠acquired различаются. Click не выдаёт два intent. HUD tooltip не перехватывает movement input; в Draft/Build действует обычная pause policy. Детали не скрывают обязательный выбор. Semantic IDs стабильны или мигрированы вместе с assets/tests. DEV gated и bounds≤25%×45% reference viewport.

Player HP следует за персонажем при движении/camera follow/shake; нет отдельного
постоянного HP bar по краю экрана. Pause сохраняет current/max HP. Death/retry
не оставляют anchor старого персонажа. Release UI не вызывает speed changes.
Краткий текст содержит существенный эффект выбора; сокращение не теряет
значимые числа/единицы/изменения и доступ к полным рецептам.
Исключение presentation по DECISION-0085: абсолютный базовый damage не выводится
в карточках/details; прибавки урона — в процентах. Сохранённый процент урона
рикошета не превращается в bonus, его точное значение заменено качественным
текстом по DECISION-0086. Числа модели/баланс остаются прежними.

HUD без wave/инструкций и без кнопки Pause (Escape/Space/ПКМ сохраняются); размеры слотов и XP-блока уменьшены
примерно на 40% только в 720p, acquired slots того же размера. Скорость в Pause —
процент текущей скорости от общего CHARACTER-BASELINE-001, не от текущего героя.
Inspect/recipe scroll не отправляют selection; только фиксированная отдельная
кнопка в закреплённой карточке подтверждает один intent, включая Banish.
Проверить 10 связанных / 20 общих рецептов, 3 колонки Pause в 1080p / 2 в 720p
и общий правый scroll для «Получены» / рецептов / «Упущены»; полученные и
упущенные — только icon/name с разными разделителями. Увеличенная область
изображения персонажа не скрывает 6+6 слотов. Любой сет Pause открывает краткую
справку по click/keyboard; закрытие не отправляет gameplay intent/Resume,
возвращает фокус. Проверить bounds у края, scroll/resize cleanup и пустые группы.
Тот же краткий эффект в Draft / Book виден рядом с уровнями компонентов.
В Draft недостижимые/acquired/закрытые сеты исключены из списка и счётчика,
пустой результат скрывает inspector. Eligibility приходит от IP-11; готовый,
но не полученный рецепт остаётся видимым. Полный контракт — UI §§6–10.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Сам production HUD/draft/pause layout плюс fake-state gallery/harness. IP-11 реально наполняет recipe projection, IP-12 — selection, IP-15/16/25/29 — свой vertical UI; IP-26 связывает экраны.

## Проверки

Presenter fake model/view, UXML/USS IDs, PlayMode geometry/focus/queued drafts;
release visibility/DEV intent gating, player HP anchoring/lifecycle; manual
1920×1080 и 1280×720 с production-текстом, иконками и игровым фоном. Projections
не меняют build. Long-text/empty-icon fixtures остаются стресс-проверкой;
assertions не закрепляют старые координаты или обязательное расположение details снизу.

## Документационные изменения

IP-31 расширяет bounded DEV drawer вкладкой Playtest: `development-tab-playtest`, `development-pane-playtest`, `playtest-summary/note/marker/export`. Отдельные immutable PlaytestViewState/presenter intents принадлежат IP-31; foundation сохраняет collapsed/release gating и changed-state rendering.

Component/state/semantic contracts, approved UI section links, dependency consumers и tests; .claude UI rules remain.

### Контракт компонентов

- `ContentCardViewState` / `ContentCard`: title, summary, details, optional resolved Sprite, enabled/selected/locked. Без icon используется shape placeholder; production icons поставляют owning content IP.
- `DraftOptionViewState` / `DraftCard`: effect/current→next text, set marker/free slot и immutable ordered `RecipeProjectionViewState`. Producer задаёт current/projected/required, completes/acquired и готовые component/threshold строки с выделением текущего option. Карточка показывает число связанных сетов; закреплённый inspector — список всех связанных, краткий эффект и компоненты выбранного рецепта. Сортировка и gameplay projection принадлежат IP-11.
- Шапка DraftCard по DECISION-0086: иконка слева от типа/уровня, название ниже
  по левому краю. `card-header` / `card-level` — локальные semantic IDs;
  «Активное» / «Пассивное» / «Сет» — крупный цветной текст без pill.
  Каждый элемент recipe list использует переданный `RecipeProjectionViewState.Icon`,
  не теряя имя/числовой прогресс и поведение inspect-only.
- `RecipeProjectionViewState.OwnedComponents` задаёт numerator в списке; прежние
  Current/Projected остаются threshold counts для статусов. Typed
  `RecipeComponentViewState` задаёт presence/levels; view не выводит их из строк.
  `draft-details` — контейнер строк: ✓/○ по наличию, зелёный + «Уровень набран»
  только по текущему threshold. Заголовок draft не показывает уровни забега;
  очередь — только число оставшихся выборов. Проценты и reciprocal cooldown
  форматируются по UI §7 без изменения model/config.
- `BuildSlotViewState` / `SetBuildViewState`: compact HUD references и подробности Pause/Build. `SetRecipeProgressViewState.HasProgress` позволяет показывать owned component ниже threshold даже при `FulfilledComponents == 0`; IP-11 supplies semantics. Acquired list отделён от progressed unacquired recipes.
- `UiNotification`: один nonblocking slot с заменой сообщения и expiry по pause-aware delta. Event selection — producer; foundation связывает level-up/set acquisition и проверяет остальные тексты fake events.
- Build/character snapshots сохраняют элементы при неизменных данных. Draft revision остаётся authority для пересборки карточек; Banish mode обновляет их в рамках той же revision.
- HUD slots не focusable. Тело Draft-card по click/keyboard activation только
  закрепляет просмотр; hover/focus не меняют его. Отдельная кнопка подтверждает.
  Inspector не скрывает варианты/actions; точную геометрию задаёт UI/UX.
  Pause/Build отображает 6+6 и sets; его полная композиция включает Resume и
  feature-owned Settings/Quit IP-26, с действиями вне scroll.
- Новые semantic IDs: `card-icon/title/summary/status/more`, `card-recipe-{index}` (локальны внутри card), `draft-details`, `pause-build`, `pause-character`, `hud-notification`. Прежние draft/slot/control IDs сохранены. USS resource — `UI/GameplayUiStyles`, чтобы не выбирать встроенный StyleSheet subasset `GameplayUi.uxml`.
- R2: `draft-option-{index}` теперь inspect-button, соседний
  `draft-option-{index}-confirm` — единственный commit-action. `draft-recipe-list`
  и `draft-recipe-{index}` переключают краткий эффект/компоненты без gameplay intent.
  `pause-slots` отделён от общего `pause-build` ScrollView;
  `pause-received-sets`, `pause-recipes`, `pause-missed-sets` находятся внутри него.
  `set-popup` — overlay, `pause-footer` — фиксированный host feature-owned actions.
  Удалён `hud-pause`; прежние speed/wave IDs сохранены только внутри DEV.
- `PauseBuildPanel` отвечает за read-only группировку и popup/focus lifecycle;
  `GameplayUiCopy` — за короткий UI-текст и проценты из preview, без балансных значений.
  `GameplayUiRoot` проецирует HP по bounds текущего sprite и actual render camera;
  геометрический профиль `ui-compact` выбирается по ширине pixel panel.
- `UiFoundationTests` и `UiFoundationSmokeTests` — fake-state harness для 0/1/2/3, empty/max/long labels, Book/set/projection/locked/selected. Проверка layout выполняется при 1920×1080 и 1280×720; последний — lower test viewport, не новый product minimum.
- Production acceptance выполняется вместе с IP-26: реальные иконки и type labels,
  правдивый timer, обычный level-up отдельно от Book, заполненный билд и полный
  AppShell. Синтетические captures не подтверждают этот контракт.

## Gates и недостающие решения

G-01/G-03 short/book states определены DECISION-0019/0020 и поставляются владельцем IP-07; foundation сохраняет три позиции, origin, очередь и начисленную валюту. IP-10 поставляет banish mode/cancel, control hints и revision reset (DECISION-0022); reusable cards сохраняют эти presenter intents. Baseline-relative character filtering — IP-12. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
