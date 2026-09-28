# UI layout R2 — Unity runtime evidence, 2026-09-28

Основание: пользователь принял HTML-композицию, попросил ещё немного увеличить
область персонажа и разрешил следующий шаг. Контракт — DECISION-0083/0085/0086,
UI/UX и IP-10A/IP-26. Текущий execution status — только [STATUS](../STATUS.md).

## Выполненная дельта

- HUD: HP привязан к верхней границе персонажа через gameplay camera, в том числе
  после смещения камеры; скрывается без валидного anchor. В центре только таймер.
  Подсказок управления и кнопки паузы нет. 6+6 слотов и полученные сеты:
  50 px в 1080p, 30 px в 720p; размер читаемого текста не уменьшен на 40%.
- Speed 1×/2×/3×/5× и волна перенесены в bounded DEV surface;
  presenter отвергает speed intent вне Editor/Development capability.
- Draft/Book: клик или Submit карточки только закрепляет просмотр; отдельная
  кнопка выбранной карточки подтверждает Выбрать/Исключить. Related count/list
  содержит только ещё достижимые, не полученные и meta-open сеты; достижимость
  вычисляет существующий owner. Regression покрывает 1 свободный/2 недостающих
  и 2 свободных/3 недостающих компонента.
- Related list прокручивается; рядом краткий эффект выбранного сета и уровни
  компонентов. Базовый урон скрыт, прибавки процентные; retention качественный.
  Краткие тексты используют canonical identities и не назначают значения TBD.
- Pause: область персонажа 100×96 px (720p), 140×120 px (1080p), текущие stats,
  все 12 слотов слева; скорость от общего CHARACTER-BASELINE-001.
  Справа один общий вертикальный scroll: полученные, прогресс, упущенные.
  Полученные/упущенные — компактные icon/name, прогресс — 2/3 колонки.
- Клик по сету любого статуса открывает краткую справку; Escape/ПКМ/вне/X,
  scroll, resize и смена экрана закрывают её без Resume. Focus возвращается,
  Space на сфокусированной видимой кнопке не утечёт в глобальную паузу.
- Фиксированный footer содержит Resume и существующие Settings/Quit из AppShell;
  владельцы действий/сохранений не дублируются. Повторный забег создаёт новый
  UI/anchor без старых подписок. Keyboard default и настройка mouse сохранены.

Основные owners: GameplayUiPresenter/GameplayUiCopy — projection/copy;
UiToolkitGameplayView/DraftCard/PauseBuildPanel — rendering/inspection;
GameplayUiRoot — responsive root/HP anchor; AppShellScreen и composition —
существующие navigation actions и input ownership. UXML/USS используют flex,
не web CSS grid/media queries. Новые raster assets не создавались и не менялись.

## Проверки и условия снимков

Unity 6000.6.0f1. Перед переносом: prerequisite EditMode 301/301 PASS,
`TestResults/checks/20260928T123723-761946Z/summary.json`.

Полный прогон: `python scripts/check_project.py --scope full --graphics --timeout 300`.
Опция graphics добавлена в безопасный runner: она убирает только `-nographics`,
сохраняя Editor/process/lock проверки. Graphics входит в receipt/cache key.
Python regression самого runner — 24/24 PASS.

Общий Unity результат: 877/877 EditMode + 33/33 PlayMode, 0 skipped;
`TestResults/checks/20260928T132601-145478Z/summary.json`.
Включена regression сохранения точности небольших чисел: регенерация 0.35/с
не округляется до 0.4/с в кратком описании.
Generation UP TO DATE; audio integrity 28 files/15 cues PASS;
art manifest 254 owner/role records PASS (не оценка пиксельного качества).
Browser mock повторно проверен после увеличения портрета: 38 captures PASS,
`TestResults/ui-layout-r2-set-info/summary.json`.

ProductionUiR2SmokeTests запускает настоящую ProductionSmokeScene, FIELD-001,
production catalog и registry sprites. Memory profile и полный билд 6+6
подготовлены тестом; это не прохождение, в котором такой билд получен естественно.
Book принимает выбор через inspect/confirm; Pause→Settings→Pause→Resume,
смещение персонажа/камеры и defeat→Retry проверены в той же composition.
Кадры сняты Unity с graphics через camera/UI RenderTexture, не HTML.

Снимки (локальные TestResults, не production assets), оба размера 1280/1920:

- `r2-production-hud-1280.png`, `r2-production-hud-1920.png`;
- `r2-production-book-1280.png`, `r2-production-book-1920.png`;
- `r2-production-pause-1280.png`, `r2-production-pause-1920.png`;
- `r2-production-set-info-1280.png`, `r2-production-set-info-1920.png`.

Отдельный synthetic stress UiLayoutR2SmokeTests: 10 связанных/20 общих рецептов,
длинные имена, 12 слотов, received/progress/missed, scroll до конца, popup у края,
bounded footer, inspect без commit, Banish confirmation, focus/input guard.
Captures `r2-density-popup-*`, `r2-density-bottom-*`, `r2-draft-density-*`
не выдаются за production каталог или естественный билд.

Агент просмотрел обе production Pause/Book композиции, HUD и popup в 720p,
synthetic нижнюю часть scroll и Draft density. Проверены отсутствие пересечений,
видимость 12 слотов и footer, крупный портрет, короткие эффекты и полосы HP/XP.
Старые assertions мгновенного выбора/английских подписей обновлены под новый
контракт; поведенческие проверки выбора, паузы, settings и retry сохранены.

## Ограничения

Это автоматические проверки и осмотр агента, не пользовательская приёмка Unity
варианта. Ещё нужен живой просмотр читаемости при боевой плотности/VFX,
фактическом camera shake и физических mouse/keyboard shortcuts. Тест смещения
камеры и render callback ordering не заменяет такой visual review.
Release gate проверен через capability/intent tests; новый standalone release
build в этом проходе не заявляется. Остальные экраны и поздние каталоги не
переработаны, баланс/правила/контентные TBD не изменены.

## Корректировка карточек после просмотра игры

[Отзыв OBS-01…03](../../playtests/2026-09-28_ui-card-layout.md), 2026-09-28:
размер/подписи типа, пропущенные recipe icons, вертикальная вместо горизонтальной
шапки. Исправлено в DraftCard, UiToolkitGameplayView и scoped USS. Карточка
использует icon слева, type/level справа, название ниже слева. Подписи
«Активное»/«Пассивное»/«Сет» — 18/20 px и разные цвета без овальной плашки.
Recipe list теперь использует уже переданный Sprite и отдельные name/progress;
данные/правила выбора и базовая ContentCard других экранов не менялись.

Проверка дельты (не новый full smoke):
`python scripts/check_project.py --scope code --graphics --filter '^Game\.(UI\.Tests\.|Bootstrap\.PlayModeTests\.(UiFoundationSmokeTests|UiLayoutR2SmokeTests|ProductionUiR2SmokeTests))' --timeout 300`.
Unity 6000.6.0f1: 74/74 EditMode + 3/3 PlayMode, 0 failed/skipped,
`TestResults/checks/20260928T134607-896058Z/summary.json`.
Перед запуском пользователь закрыл Editor; runner выполнил свежий process/lock
preflight и использовал batch с graphics, не запускался поверх его игрового прогона.

Новые regression assertions проверяют type/header structure, Sprite identity,
пустой icon, production icons, 18+ px type без pill, относительные позиции
icon/type/level/title. Прежние inspect/confirm, dense recipe scrolling,
bounded geometry, Pause/settings/retry проверки включены в эти три PlayMode.
Повторно сняты и просмотрены `r2-production-book-1280.png` и
`r2-production-book-1920.png`: type, title и icons не пересекаются, исправления
видны на реальном каталоге. Условия arranged build и ограничения визуальной
приёмки выше сохраняются. HTML не переснимался: это проверка именно Unity.

## Компоненты рецепта и проценты после дополнительного review

[OBS-04…07](../../playtests/2026-09-28_ui-card-layout.md), 2026-09-28:
owned numerator независимо от прокачки; ✓/○ presence и зелёный текущий threshold
с «Уровень набран»; без номеров уровней забега в heading/queue, уровень карточки
сохранён. Icon/type/level увеличены до 88/21/18 px и 112/24/21 px (720p/1080p).
Числа процентов округляются до целых; cooldown переводится в относительную
частоту через обратный интервал. В SKILL-001 L2→L3 скрыта только техническая
компенсация lifetime при росте скорости и прежней дальности, показано +15%
скорости полёта. Gameplay/JSON/баланс не менялись.

Typed RecipeComponentViewState не заставляет renderer разбирать строку.
RecipeProjectionViewState.Current/Projected сохраняют threshold semantics;
новый OwnedComponents используется только для числового progress слева.
Legacy string-only fixtures поддерживаются, но production передаёт typed rows.

Scope/filter такой же, как в предыдущей корректировке. Новый результат:
83/83 EditMode + 4/4 PlayMode, 0 failed/skipped, graphics, Unity 6000.6.0f1;
`TestResults/checks/20260928T141619-446369Z/summary.json`.
Первый запуск не выполнил тесты из-за отсутствующей ActiveSkill dependency
в UI test assembly; catalog-specific assertion перенесён в существующую
Bootstrap PlayMode test assembly, без расширения production dependencies.
Указанный PASS получен после исправления компиляции.

Просмотрены новые `r2-production-book-1280.png` и `r2-production-book-1920.png`:
в 1080p «Полевой медик» 3/3 owned, но «В процессе», зелёный только Собира́тель
с выполненным текущим уровнем; в 720p готовый рецепт с тремя зелёными строками.
Более крупная шапка не перекрывает имя/описание/actions, scroll сохранён.
Условия arranged build и ограничения ручной приёмки по-прежнему действуют.

После стабилизации выполнен новый полный `--scope full --graphics`:
887/887 EditMode + 34/34 PlayMode, 0 failed/skipped,
`TestResults/checks/20260928T141812-824606Z/summary.json`.
Generation UP TO DATE; audio integrity 28 files/15 cues PASS;
art manifest 254 owner/role records PASS. Этот receipt включает все OBS-01…07.
