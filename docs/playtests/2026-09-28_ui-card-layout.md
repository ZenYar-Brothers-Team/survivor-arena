# UI review — карточки улучшений, 2026-09-28

Источник — сообщение пользователя при просмотре игры. Raw report не предоставлен;
reportId/runId, разрешение, время забега и конфигурация не уточнены. Телеметрия не
нужна для воспроизведённого расхождения разметки с утверждённым HTML-макетом.
Состояние IP и его приёмки — только [STATUS](../implementation/STATUS.md).

## OBS-01 — тип карточки мелкий и в овальной плашке

Цитата: «активное, пассивное и сет. Вот. И текст побольше сделать»;
«обводка мне это не нравится овальная».

Диагностика: DraftCard переводил Active/Passive как «Умение»/«Предмет»;
content-card-type сохранил 12 px, заливку и border-radius 99 px старого прохода.
Правка: «Активное»/«Пассивное»/«Сет», 18/20 px, цветная подпись без pill.
Regression: UiFoundationTests.DraftCard_TypeLabel_UsesSemanticElementAndVisualClass;
production PlayMode проверяет размер текста, отсутствие заливки и скругления.

## OBS-02 — пропущены иконки связанных сетов

Цитата: «почему снизу в наборе сетов в этом же окне нету иконки?»;
«иконку сета надо бы добавить. И на макете она была».

Диагностика: presenter уже заполнял RecipeProjectionViewState.Icon, но view
создавал только Button.text = recipe.Summary. Потеря произошла в rendering.
Правка: в строке Image с переданным Sprite, имя и отдельный числовой progress.
Regression: UiLayoutR2Tests.DraftRecipes_RenderProjectedIconTitleAndProgress;
production PlayMode проверяет ненулевые registry sprites и положение слева.

## OBS-03 — иконка карточки занимает отдельный ряд с пустотой по бокам

Цитата: «иконка в центре кучу места занимает, она самая небольшая,
справа-слева пустота»; «Были какие-то проблемы с тем, чтобы это ближе к макету сделать?».

Диагностика: DraftCard оставил вертикальную ContentCard-структуру и
align-self:center у icon; макет использовал горизонтальный card-top.
Технического ограничения UI Toolkit здесь нет: flex row поддерживает этот layout.
Это недочёт переноса, не намеренно выбранная альтернативная композиция.
Правка: локальный card-header объединяет icon слева и type/level справа;
название ниже по левому краю. Общая ContentCard для других экранов не перестраивалась.
Regression: structure assertions в UiFoundationTests и geometry assertions
в ProductionUiR2SmokeTests для 720p/1080p. Inspect/confirm остаются раздельными.

## Повторная проверка

OBS-01…03 исправлены и проверены автоматической regression 2026-09-28:
74/74 UI EditMode + 3/3 UI PlayMode, 720p/1080p graphics. Новые production
captures просмотрены агентом; [runtime evidence](../implementation/evidence/2026-09-28-ui-layout-r2-runtime.md#корректировка-карточек-после-просмотра-игры).
Пользовательская визуальная приёмка исправленного варианта ещё не получена.

## OBS-04 — счётчик компонентов в Draft считает прокачку вместо наличия

Цитата: «не количество уже полностью прогрейженных до нужных уровней,
а количество полученных элементов этого сета».

Диагностика: RecipeProjectionViewState.Progress показывал Current/Projected —
число выполненных thresholds. Исправление: отдельный OwnedComponents для
numerator, без добавления ещё не принятого выбора. Старые threshold counts
остаются у ready/completes статусов: 3/3 owned не означает готовый рецепт.
Regression: GameplayUiPresenterTests.RecipeProjection_PartialThresholdCompletesAndAlreadyEnoughAreDistinct.

## OBS-05 — наличие и выполненный уровень должны различаться

Цитата: «галочка действительно пусть отмечает, что есть предмет»;
«когда он набран, пусть становится зелёным»; уточнение: «Не крестик, а круг».

Исправление: typed RecipeComponentViewState, ✓/○ по CurrentLevel > 0;
зелёная строка с «Уровень набран» только при текущем уровне ≥ required.
ProjectedLevel сохраняется для preview, но не окрашивает ещё не выполненный
threshold. View не извлекает семантику из текста. Pause rules не меняются.
Regression: UiLayoutR2Tests.DraftComponents_PresenceAndCurrentLevelMet_AreIndependent;
ProductionUiR2SmokeTests проверяет зелёный цвет и подпись в обеих геометриях.

## OBS-06 — краткая очередь и более крупная шапка карточки

Цитаты: «про уровни: какой был, какой стал — не надо» (шапка draft);
«Увеличь размер того, что идет выше названия. То есть иконку побольше сделай,
надписи: активное, новый уровень один».

Исправление: «Выбери улучшение» / «Книга странника», только «Ещё улучшений: N»
при очереди; без номера earned/next уровня. Уровень самой карточки остаётся.
Icon/type/level: 88/21/18 px в 720p, 112/24/21 px в 1080p.
Regression: GameplayUiPresenterTests.LevelDraftHeading_OmitsEarnedAndNextLevels_KeepingOnlyQueueCount
и production geometry assertions.

## OBS-07 — отрицательное время, пересчёт скорости и целые проценты

Цитаты: «не просто минус на плюс поменять, а, ну, пересчитать»;
«в процентах указываются, округляй до целых процентов».
На уточняющий вопрос о названии умения пользователь ответил: «у броска камня».

Диагностика SKILL-001 L2→L3: скорость 10→11.5 при прежней дальности 5;
lifetime сокращается обратно пропорционально скорости. Это полёт, не cooldown.
Исправление: «Скорость полёта +15%», техническое компенсирующее уменьшение
lifetime не выводится. Настоящая длительность, меняющая дальность/эффект,
не скрывается. Для cooldown: reciprocal frequency, например 2→1.5 s даёт +33%,
не +25%; action-speed bonus учитывает прежнюю скорость. Проценты целые,
non-percent 0.35 HP/s сохраняет точность. Model/config/balance не менялись.
Regression: UiLayoutR2Tests.CooldownCopy_UsesReciprocalFrequency,
PercentCopy_RoundsToWholeWithoutNegativeZero и production
StoneCopy_HidesDerivedFlightLifetime_ButPreservesTrueLifetimeChanges.

Повторная проверка OBS-04…07: scoped 83/83 UI EditMode + 4/4 UI PlayMode,
graphics 720p/1080p (`TestResults/checks/20260928T141619-446369Z/summary.json`).
Снимки просмотрены агентом; повторной пользовательской приёмки ещё нет.
Итоговый общий smoke после правок: 887/887 EditMode + 34/34 PlayMode, graphics,
0 failed/skipped (`TestResults/checks/20260928T141812-824606Z/summary.json`).

## OBS-08 — достижимые сеты без взятых компонентов скрыты

Цитата: «сет еще не собран, он при этом не упущен, но он и не начал собираться»;
«По идее надо, как бы с нулями»; «он может нацелиться на него, хотя даже у него
ничего не собрано».

Диагностика: presenter уже передавал все meta-открытые сеты, но PauseBuildPanel
отбрасывал `!HasProgress`. Это соответствовало прежнему ограничению UI §10;
пользователь уточнил контракт. Фильтр снят: все достижимые unacquired видны,
без компонентов — `0/N · Не начат`. Состав, уровни и просмотр эффекта сохранены.
Meta-closed скрыты прежним producer; acquired/missed остаются отдельно.
Draft/Book filtering, thresholds, slot attainability и баланс не меняются.

Regression: `UiFoundationTests.PauseRecipes_ZeroOwnedAttainable_ShowsZeroAndKeepsAcquiredAndMissedSeparate`;
`UiLayoutR2SmokeTests.DenseRecipes_OnePauseScroll_PopupAndDraftInspectionNeverCommit`
проверяет неначатый рецепт, общий scroll и справку в 720p/1080p.
Результаты проверки — [runtime evidence](../implementation/evidence/2026-09-28-ui-layout-r2-runtime.md#неначатые-рецепты-на-паузе).
