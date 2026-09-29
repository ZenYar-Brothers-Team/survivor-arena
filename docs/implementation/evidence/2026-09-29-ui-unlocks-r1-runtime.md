# UI Unlocks R1 — перенос в Unity, 2026-09-29

Основание: [DECISION-0092](../../decisions/0092-unlocks-collection-ui.md).
Текущий статус и дальнейшая очередь — только [STATUS](../STATUS.md).

## Реализация

MetaPresenter выдаёт все 70 определений, включая начальные. Typed `Kind`/`Owned`
в MetaCardViewState позволяют фильтровать без разбора подписей. MetaUnlockPanel
показывает тип/имя/статус/цену, общую прокрутку, фильтры и счётчик. Карточки
3/2 колонки; шапка, фильтры и возврат вне списка. Пустая выборка объясняется.
Покупки остаются intents в ProfileService; условия, цены и сохранение не менялись.
Только закрытые персонажи остаются силуэтами с «?». Карты/сеты не скрыты.
Новых растровых ассетов и изменений арт-каталога нет.

## Проверки

`python scripts/check_project.py --scope full --graphics`

Свежий safe preflight выбрал batch при закрытом Editor. Unity 6000.6.0f1:
945/945 Game.* EditMode, 38/38 PlayMode, 0 failed/skipped, third-party 0.
Generated content актуален; audio integrity 28 файлов PASS; art audit 256 записей PASS.
Результаты: `TestResults/checks/20260929T055646-703339Z/summary.json`,
`EditMode.xml`, `PlayMode.xml` и соответствующие логи.

Новый EditMode test проверяет количество/типы/начальные открытия и маскировку
только персонажей. Новый production PlayMode test проверяет все фильтры, пустое
состояние, достижимость последней записи скроллом и bounds footer в двух размерах.
Полный набор также включает существующие tests покупок, ошибок записи, профиля,
run lifecycle, combat, XP/draft, active skills, waves/pooling, composition и content.

Снимки `TestResults/meta-unlocks-{1920x1080,1280x720}.png` и
`meta-unlocks-{character,field,ability,set}-{размер}.png` получены из Unity.
Визуально просмотрены общий экран 1080p и сеты 720p: сетка читаема, контент не
выходит по горизонтали, footer закреплён. Ручной пользовательский прогон нового
экрана не заявляется; screenshots и автоматические проверки его не заменяют.

Documentation impact: UI/UX §17, IP-26, DECISION-0092, proposal, PROJECT_MAP и
regression-map синхронизированы. Game/Content Design и баланс без изменений.
