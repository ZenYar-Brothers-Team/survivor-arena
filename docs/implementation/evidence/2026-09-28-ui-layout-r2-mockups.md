# UI layout R2 — HTML-макеты, 2026-09-28

## Объём

Подготовлен [отдельный review-прототип](../proposals/ui-layout-r2/README.md)
HUD, Draft, Book, Pause / Build и фрагмента Controls. Это композиционный этап
предложения R2; UXML/USS/C# в этой работе не менялись. Сохранены чужие изменения
mouse movement / Settings / pause shortcuts; их evidence принадлежит отдельному
[пакету input](2026-09-28-mouse-movement-and-pause-shortcuts.md).

Настоящие иконки, имя персонажа, названия и компоненты рецептов используются
вместо fixture labels. Игровой фон составлен из production-арта FIELD-001,
но не захвачен из работающей игры. Каждый PNG имеет явную маркировку макета.

## Проверки

- Node syntax check: PASS.
- Playwright + установленный Edge headless, чистый временный профиль: PASS.
- 10 сочетаний экран × resolution: 1280×720 и 1920×1080, без broken images,
  ошибок browser JS, выхода основных panels/actions за stage и text overflow
  проверяемых карточек/слотов/панелей.
- Context details не двигают карточки и не перекрывают Reroll/Banish.
- Default Mouse movement Off; Space на checkbox переключает его без паузы.
- Draft Space не снимает паузу, selection даёт одно событие mock feedback.
- Banish отключает Reroll, отмена возвращает доступность.
- Space / ПКМ переключают HUD/manual pause в mock routing.
- После уточнения DECISION-0085 Book не показывает абсолютный базовый урон;
  upgrade Камня показывает +50% урона. Отдельные browser assertions проверяют
  наличие процента и отсутствие прежних абсолютных значений на карточках.
- Сохранены 20 кадров: пять экранов в двух размерах, details Draft/Book,
  начальные HUD/Pause и плотный HUD. Paths: `TestResults/ui-layout-r2/*.png`;
  receipt: `TestResults/ui-layout-r2/summary.json`.

Первый browser run выявил overflow build pane при 720p. Исправлено уменьшением
высоты rows и промежутков; шрифт не уменьшался. Повторный run прошёл.
Вручную осмотрены HUD, Draft, context recipe, Book, Controls и Pause 720p/1080p.
Кириллица/иконки читаются, все 12 слотов и footer Pause видимы; нижние рецепты
720p находятся в собственном scroll, не уводят controls за экран.

## Что эти проверки не подтверждают

Это не Unity verification, не gameplay capture и не пользовательская visual
приёмка. Не проверены world-to-screen anchor при движении/camera shake/retry,
release DEV gate, реальные pause reason ownership и routing ввода, boss/Traveler
overlays, live density/VFX, полный каталог, все display states и итоговые
ViewState values. Нужна production интеграция после выбора композиции.

Короткие формулировки и новая details-панель — конкретное предложение. Они
не становятся каноном автоматически. UI-профили/механики/баланс/сохранения
макет не изменяет. Текущий execution status — только [STATUS](../STATUS.md).
