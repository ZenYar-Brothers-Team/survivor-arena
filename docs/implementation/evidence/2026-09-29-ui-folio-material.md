# UI Folio material — runtime evidence, 2026-09-29

Пользователь выбрал третий образец (мягкие складки), затем отдельно утвердил
полноразмерную текстуру словами «берем». Новое правило интерфейса или изменение
layout/flow не вводилось. Арт применён только к большим окнам Entry detail,
Settings, Meta/Results, Draft и Pause через общий `FolioPanelTexture`.

AI-generated master сохранён с дословным prompt и approval в
`Art/Source/UI/ui-folio-surface/`; runtime PNG —
`Assets/Resources/Art/UI/Folio/ui-folio-surface-background.png`.
`UI-FOLIO-SURFACE-VISUAL-BACKGROUND` зарегистрирован как Background;
подготовка выполнена `art_pipeline.py` (plan и apply). Одна фактура
растягивается по панели без повторения и ослаблена до 0.36 opacity, чтобы
не перекрывать текст и границу окна. Мелкие карточки остаются ровными.

Проверка `check_project.py --scope art`: 57/57 EditMode, manifest 269 PASS,
`TestResults/checks/20260929T103505-510136Z/summary.json`.
После настройки прозрачности выполнен `check_project.py --scope full --graphics`:
Unity 6000.6.0f1, safe batch runner, 965/965 EditMode + 39/39 PlayMode,
0 failed/skipped; generated content current, audio 28 PASS, art 269 PASS.
Итог: `TestResults/checks/20260929T103847-651304Z/summary.json`.
Просмотрены свежие `TestResults/meta-unlocks-set-1920x1080.png` и
`TestResults/settings-r1-1280x720.png`: складки видны, текст и границы читаемы.
Это внутренняя visual-проверка; пользовательская приёмка экранов с новым
материалом ещё не заявлена.

## Отзыв: усилить фактуру и показать её на паузе

Пользователь попросил сделать фактуру немного заметнее и добавить в игровые окна,
в частности Pause. Общее значение opacity повышено с 0.36 до 0.48. Pause уже имел
материал на внешней панели, но две непрозрачные колонки скрывали его большую часть;
теперь тот же слой присутствует внутри `pause-left` и `pause-right`. Карточки и
кнопки не текстурируются. Никакого нового raster asset или повторного тайла нет.

Просмотрены свежие `TestResults/ip10a-pause-1280x720.png` и
`TestResults/meta-unlocks-set-1920x1080.png`: складки заметнее и остаются под
текстом, рамки читаемы. Автоматическая проверка проверяет наличие материала на
обеих колонках Pause. Full graphics PASS:
`TestResults/checks/20260929T110557-960511Z/summary.json`, 969/969 EditMode,
39/39 PlayMode, 0 failed/skipped, art 269 PASS. Пользовательская визуальная
приёмка усиленного варианта остаётся открытой.
