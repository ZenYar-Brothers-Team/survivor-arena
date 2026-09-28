# Полевой фолиант — композиционный макет R2

Открыть [интерактивный макет](http://127.0.0.1:4179/docs/implementation/proposals/ui-layout-r2/index.html),
пока работает локальный preview server. Исходник — [index.html](index.html).

Это reviewable предложение layout, не игровая реализация и не новый источник
product rules. Approval стиля не означает approval этой геометрии. Текущий статус
и следующие работы — только [STATUS](../../STATUS.md).

## Что смотреть

- Бой: HP возле персонажа, 6+6 компактных иконок, XP, countdown, отдельный acquired set.
- Улучшение: upgrade / passive / set; краткий эффект и контекстные компоненты рецепта.
- Книга: отдельное раннее состояние, новое умение, origin и очередь 1 из 2.
- Пауза: персонаж, 12 слотов без общего scroll, acquired set; отдельный scroll рецептов.
- Управление: только фрагмент Settings с галочкой Off и клавишами паузы.
- Переключатели review: 1280×720 / 1920×1080, начальный / полный билд, плотный фон.

В Draft наведение с задержкой или keyboard focus раскрывает подробности; есть
также явная кнопка. Карточки и нижние действия не сдвигаются. Panel занимает
ограниченную область и прокручивается при необходимости; постоянной инструкции
или длинного описания до взаимодействия нет. Это один конкретный кандидат для
review, не окончательное решение о details.

## Происхождение и ограничения

Используются существующие production PNG из `Assets/Resources/Art`, текущие
Alegreya Sans Medium / PT Sans и названия из production catalogs. Изображения
не редактируются, не генерируются, не копируются в новые runtime assets;
подготовка нового art packet и `art_pipeline.py --apply` здесь неприменимы.

Фон собран в HTML из ground/props FIELD-001, Клёпки и enemy-001/002. Это
постановочная сцена на настоящих ассетах, **не capture Unity**, не проверка
реального масштаба камеры, density/VFX или actual-content runtime composition.
Сохранённые старые PNG оказались fixture-снимками; Editor параллельной сессии
не использовался. Перенос поверх живого боя остаётся обязательным отдельным шагом.

HP 84/116, время, XP, level и уровни билда — выбранный пример состояния, не
записанный забег. Draft показывает отдельный момент до получения SET-001,
Pause — после. Book — отдельный ранний пример со свободными слотами. Значения
нового умения в Book, кроме урона, базовые; итоговые character modifiers в макете
не вычисляются. По [DECISION-0085](../../../decisions/0085-ui-damage-percent-presentation.md)
абсолютный базовый урон не отображается; upgrade Камня показывает процентный
прирост из catalog с округлением до целого, а не пару абсолютных чисел.
Краткие тексты — UI-copy proposal на основании полных canonical cards, не изменение
механик. Имена и рецепты читаются из catalogs; summary намеренно не копирует их
технические поля. Проектные JSON не записываются.

Контент review: CHAR-001; SKILL-001/002/003/004/006/007; PASSIVE-001/002/004/008/009/011;
SET-001/004/006/010 и их компоненты. Только meta-открытые startup-рецепты;
TBD поздних сетов не выдаются за production rules.

Пока не моделируются: случайный reroll/banish pool, награды Book, persistence,
настоящий movement, world-to-screen HP-anchor, boss/Traveler overlays, notifications,
полный каталог и 0/1/2-option Draft. Кнопки игровых действий дают сообщение о
действии, не меняя игру. Нажатие слота Pause показывает только подпись, не полный
inspection state. Эти ограничения не скрыты за утверждением о готовности UI.

## Согласование с параллельной работой

[DECISION-0084](../../../decisions/0084-mouse-movement-and-pause-shortcuts.md)
владеет движением/сохранением preference и bindings. Клавиатура — default;
мышь включается галочкой, пауза — Escape / Space / ПКМ. Файлы этой реализации
не редактируются макетом.

В HTML Space/ПКМ переключают только бой ↔ ручную паузу. Они не снимают Draft
или Settings. Space на сфокусированном checkbox/button принадлежит этому control;
двойной pause/submit исключён в mock routing. Это UX regression case для интеграции,
не доказательство аналогичного поведения Unity. У HUD и HP нет pointer capture;
границы mouse movement над кнопкой Pause и menu focus проверить в actual composition.

## Повторный запуск

Из корня репозитория (Node.js):

```powershell
node docs/implementation/proposals/ui-layout-r2/serve-preview.cjs
```

Сервер слушает только `127.0.0.1:4179`, read-only GET/HEAD; раздаёт только preview,
необходимые resource folders и шрифты. Порт можно задать через `UI_PREVIEW_PORT`.
Для просмотра нужен браузер, но внешний интернет не требуется. Прямой `file://`
не рекомендуется из-за browser restrictions на чтение JSON.

Проверка (нужен Playwright; `PLAYWRIGHT_MODULE` может указывать его локальный путь):

```powershell
node docs/implementation/proposals/ui-layout-r2/verify-preview.cjs
```

По умолчанию проверка использует установленный Edge headless с временным чистым
профилем и закрывает его в `finally`. Можно выбрать `UI_PREVIEW_BROWSER=chrome`.
Личные профили, вкладки и настройки не используются. Вывод —
`TestResults/ui-layout-r2/`: 20 PNG и `summary.json`.

Проверяются загрузка assets, границы и text overflow, неподвижность карточек
при открытии details, отсутствие overlap с actions, default toggle, Space/ПКМ,
single selection и disabled reroll в Banish. Это browser-only checks.

Результаты проведённого review — [evidence](../../evidence/2026-09-28-ui-layout-r2-mockups.md).
