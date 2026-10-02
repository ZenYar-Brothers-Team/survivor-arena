# Implementation Status

Единственный источник execution status и Execution order. Навигация по коду/данным: [PROJECT_MAP](../PROJECT_MAP.md).

Plan revision: design-sync-R2; startup packets: field-001-start-R1.
UI entry R1 feedback закрыт пользователем 2026-09-28: «Отлично», «Все, идем дальше». Запрет тестов на время фидбека завершён. Финальные правки: мечи сложности, закрытые герои только силуэт/«?», медленная диагональная пыль и замедленный свет.
Worktree integration 2026-09-28: `develop-evg-wt` (`3d557ea`, `9e0c51f`) перенесена в текущую линию с сохранением UI entry R1; [merge evidence](evidence/2026-09-28-worktree-integration.md). Свежий полный smoke Unity 6000.6.0f1 с graphics: 921/921 EditMode + 34/34 PlayMode, 0 failed/skipped, generation/audio/art 254 PASS (`TestResults/checks/20260928T184120-318706Z/summary.json`). Персонажи/путники остаются Implemented до ручной приёмки; текущий UI packet и его порядок ниже сохраняются.
Current UI checkpoint: UI entry R1 — Verified в границах трёх стартовых экранов, с финальным feedback пользователя и fresh checks ниже. Приёмка не распространяется на Results/Meta/Settings или новые gameplay-каталоги.
Results R1 feedback 2026-09-28: «Новые открытия» расширены в макете до смешанной коллекции сетов, умений, персонажей и карт; изображение + название + тип. Новые открытия расположены выше собранных сетов в общей прокручиваемой области. Состав и условия открытия не меняются; перенос ниже.
UI Results R1 runtime packet: Verified — пользователь принял результат словами «Отлично, идем дальше». Композиция и +20 за успешный Book upgrade перенесены в Unity (DECISION-0090). Full graphics PASS перед приёмкой: 936/936 EditMode + 36/36 PlayMode, 0 failed/skipped; generation/audio/art 256 PASS. [Evidence](evidence/2026-09-28-ui-results-r1-runtime.md), `TestResults/checks/20260928T205143-284843Z/summary.json`. Нового прогона при фиксации приёмки не было.
Settings R1 — Verified: пользователь подтвердил просмотр в игре 2026-09-29 словами
«там всё принимается». Full graphics 946/946 EditMode + 39/39 PlayMode, затем
targeted Settings 3/3 после USS-коррекции, 0 failed/skipped. [Settings evidence](evidence/2026-09-29-ui-settings-r1-runtime.md).
Active execution: Meta R1 и полная вкладка «Открытия»; поставка и проверки сохранены:
[Unlocks evidence](evidence/2026-09-29-ui-unlocks-r1-runtime.md), [personal Meta evidence](evidence/2026-09-29-ui-meta-r1-runtime.md).
Автоматического перехода к gameplay-IP нет.
Исторический общий Unity smoke до entry R1: 2026-09-28, 887/887 EditMode + 34/34 PlayMode с graphics, 0 skipped; generation/audio integrity и provenance 254 PASS (`TestResults/checks/20260928T141812-824606Z/summary.json`); [UI runtime evidence](evidence/2026-09-28-ui-layout-r2-runtime.md). Ручную приёмку эти проверки не заменяют.
После игрового отзыва исправлены [OBS-01…07](../playtests/2026-09-28_ui-card-layout.md): более крупный icon/type/level header без pill, recipe icons, owned count отдельно от thresholds, зелёный текущий уровень с ✓/○ presence, короткая очередь и целые проценты с корректным пересчётом скорости. Полный smoke выше включает эти правки; пользовательская приёмка закрыта 2026-09-28.
Последующая дельта [OBS-08](../playtests/2026-09-28_ui-card-layout.md): Pause показывает все meta-открытые достижимые рецепты, включая `0/N · Не начат`. Новый scoped UI graphics PASS: 84/84 EditMode + 4/4 PlayMode, 0 failed/skipped (`TestResults/checks/20260928T144429-015728Z/summary.json`); [условия](evidence/2026-09-28-ui-layout-r2-runtime.md#неначатые-рецепты-на-паузе). Общий smoke выше предшествует этой дельте.

## Действующие границы

FIELD-003 road runtime 2026-10-02 — Implemented (явный scope пользователя):
первый игровой перенос R03-01/02/04 непосредственно на текущем FIELD-003, без dev-поля.
Монстры, расписания, Путники, дропы и lifetime сохраняются; награды на траве
могут оставаться недоступными. Книги ответвлений появляются при старте и используют
обычный Book lifecycle/валюту, но дают ровно один выбор. 32 seed: 13–19 книг,
все связны, 0 fallback. Цветные mesh поверх травы FIELD-001; кромка player-only.
Unity 6000.6 graphics: EditMode 1285/1285 PASS; общий PlayMode 61/63 (два UI
падения вне road scope), отдельный FIELD-003 smoke 1/1 PASS, physics 4/4 PASS.
Generation/audio/art integrity PASS. [Evidence](evidence/2026-10-02-field003-road-runtime.md).
Уточнение старта: ближайшая к центру точка оси основной дороги; центр на траве
и центр на дороге — targeted EditMode 3/3 PASS (`20261002T064143-440087Z`).
Remaining: R03-05 — игровой отзыв на текущих волнах/героях, числовые метрики
коротких участков; R03-06 — текстуры/кромка; R03-07 — полный ручной забег.
Общий IP-23 не Verified; общая очередь не возобновлена.

Исправления по ревью 2026-10-02 — Implemented: конус SET-022 без врага в радиусе бьёт в случайную сторону (seed атаки; [дополнение DECISION-0138](../decisions/0138-low-tier-sets.md)); выданные замедления двух сетов не смешивают силу и длительность; provenance плейтестов пишет `contentKind` каталога и актуальный `rngUncovered`; решение о скорости забега перенумеровано [DECISION-0054 → DECISION-0140](../decisions/0140-run-speed-controls.md) (дубликат номера); мёртвая зона `<Joystick>/stick`; все 13 `docs/balance/validate_*.py` PASS (правило телепорта по DECISION-0128, учёт переносов DECISION-0136, UTF-8 вывод). Unity 6000.6 EditMode `^Game\.(ActiveSkill|Progression|Telemetry|Enemy)\.` 563/563 и `^Game\.(Bootstrap|UI|Meta|Movement)\.` 257/257 PASS (`TestResults/checks/20261001T220858-324051Z`, `20261001T221028-308642Z`); PlayMode и ручная проверка не запускались.

Усиление сетов R1 2026-10-02 — Implemented по [DECISION-0139](../decisions/0139-set-buffs-r1.md): SET-002 (+25% range, +15% damage/action speed Бумерангу и Диску), SET-004 без Льда (Волна сама замедляет 30%), SET-005 лечение 10%, SET-010 slow 50% и уязвимость +20% в радиусе орбиты. EditMode 1276/1276 PASS; PlayMode и ручная проверка не запускались.

Low-tier сеты SET-021…035 2026-10-02 — Implemented по [DECISION-0138](../decisions/0138-low-tier-sets.md) ([пакет](../balance/sets-low-v1.md)): 15 ранних слабых сетов (2–4 компонента, сумма уровней 5–6), покупка за золото 150/100, 14 из 15 доступны с первого профиля. Новые механики: конус SET-022, `ExtraProjectiles`, `SlowStrengthBonus`. EditMode 1271/1271 PASS; PlayMode и ручной прогон сборки сетов не запускались, цены и числа без плейтеста. Иконки поставлены отдельным проходом ниже.

Иконки low-tier 2026-10-02: пользователь утвердил все 15 SET-021…035; art pipeline plan/apply,
masters/provenance, runtime 256×256, Unity import и production bindings выполнены.
Свежие проверки Unity 6000.6: art 80/80 EditMode + ProductionSetCatalogTests 6/6 PASS,
0 failed/skipped; generation и manifest integrity 308 PASS.
[Evidence](evidence/2026-10-02-low-tier-set-icons.md). Remaining: игровой цикл всех новых сетов
и совместный review, баланс чисел/цен. Ручная приёмка иконок в реальном UI не заменяется source approval;
полный low-tier packet не Verified. Общая очередь не возобновлена.

World visuals SET-021/022 2026-10-02: камешек использует approved stone projectile,
Хлопушка показывает короткий pooled `ConeArc` по направлению/углу/радиусу damage.
Свежие Unity 6000.6 проверки: ActiveSkill EditMode 122/122, art EditMode 80/80,
production visual smoke PlayMode с graphics 2/2 PASS, 0 failed/skipped; generation/manifest PASS.
[Evidence](evidence/2026-10-02-low-tier-set-world-visuals.md). Оба выявленных world visual gaps закрыты;
ручной обзор в плотном бою и общая low-tier приёмка остаются.

SET-022 visual feedback 2026-10-02: по [OBS-01](../playtests/2026-10-02_set-022-cone-motion.md)
статичный контур заменён расходящейся дугой без прямых боков (OBS-02): travel 0.14 s + fade 0.10 s,
damage остаётся мгновенным. Свежие EditMode 122/122 + graphics PlayMode 3/3 PASS,
0 failed/skipped; [Unity animation](proposals/2026-10-02-low-tier-set-icons/set-022-arc-only.gif).
Визуал показан пользователю; финальная художественная приёмка этой коррекции ожидается.

FIELD-003 дорожная сеть 2026-10-01 — концепт и геометрическое исследование по
[DECISION-0137](../decisions/0137-field003-road-network-concept.md): road-only
поле 200×200, основные дороги 0.8H, ответвления 2–4H шириной 0.7H, книга с
одним улучшением на каждый тупик, минимум 10 / гибкая цель около 15 без лимита сверху.
По отзыву пользователя прямоугольный каркас заменён случайными связями и
разным числом перекрёстков/выходов на кольцо. Шесть схем v2 вместили
21/22/23/22/21/19 тупиков (история). Последующее исследование v3: 18/18/12/16/12/14
тупиков, среднее 15; максимум пути между основными развилками 5.38–5.88H,
около 1.4 с Python-геометрии без полных перегенераций.
[Расчёт и ограничения v3](../prototypes/field003-roads/README-v3.md).
Отзыв 2026-10-02: плотная структура v3 отвергнута («соты»); приоритет —
разнообразие связей и редкость участков <2H, число 15/максимум 6H гибкие.
Проба v4: основные дороги 28–29% поля, 11/14/15/18/13/15 тупиков,
0/217 коротких участков с учётом книжных входов на шести seed, 1.35–2.29 с
Python-геометрии, 0–2 отклонённых графа. Длинные периметральные участки остаются.
[Проба и ограничения v4](../prototypes/field003-roads/README-v4.md).
Пользователь принял вариант v4 2026-10-02 («Мне нравится. …сохрани схему»).
[Фиксированный профиль/код/эталонные раскладки](../prototypes/field003-roads/approved-v4/README.md)
сохранены; повторное воспроизведение шести seed совпало по количеству книг и
длинам дорог/тупиков (Python replay PASS). Карточка FIELD-003 синхронизирована.
[План R03-01…07](milestones/FIELD-003-road-network.md) подготовлен; последующий
runtime перенос и его текущие gates — отдельная запись FIELD-003 road runtime
выше. Общая execution queue сохраняется.

Перенос карт 2026-10-01 — Implemented по [DECISION-0136](../decisions/0136-map-transfer-fields-002-003.md): тест «Тест 02» удалён, его кляксы стали геометрией FIELD-002 («Пограничные руины»), прежняя FIELD-002 (тракт) стала FIELD-003 («Королевский тракт»); фон, миниатюры, состав волн FIELD-002 (ENEMY-008→010) и Content Design синхронизированы. EditMode 1259/1259, PlayMode FIELD-002…004 smoke PASS; полный PlayMode без графики вылетает в рендере, графический прогон: 1 UI-падение вне scope. Ручной прогон и gameplay-scale review карт открыты.

FIELD-DEV-BLOBS контакт 2026-10-01 — Implemented по DECISION-0133: двадцать контуров подогнаны по плотному ядру установленных PNG вокруг импортированных pivot/PPU; generation STATIC PASS и Python 2/2 PASS. Unity art check NOT RUN: процесс Editor без определимого project path. После команды «подключай новый арт» утверждённый набор перенесён через art_pipeline: заменены 17 PNG, ещё 3 уже имели мягкий край; все GUID сохранены. Контуры повторно подогнаны по новому арту; Python 2/2 PASS, generation и manifest 293 PASS. Контакт в движении ожидает игрового просмотра. [Evidence](evidence/2026-10-01-dev-obstacle-soft-edges-and-contact.md).

FIELD-DEV-BLOBS палитра грунта 2026-10-01 — Implemented: по «Подключай и комить»
заменены 16 PNG под фактический FIELD-003 пол; 4 колючки сохранены.
Все 20 контуров и sprite GUID/PPU/pivot неизменны. Python 2/2 PASS;
Unity art check после замены NOT RUN: процесс Editor без определимого project path.
[Пакет и проверки](../../Art/Concepts/field-002-ruins-blob-art/ground-match-review.md).

Открытия за обычные убийства 2026-10-01 — восемь порогов `ordinaryKills`
увеличены ×6 по [DECISION-0131](../decisions/0131-ordinary-kill-unlock-thresholds.md).
Каталог, Content Design, подсказки FIELD-002/Meta и тесты синхронизированы;
ранее открытые ID не отзываются. Generation/content STATIC PASS, EditMode
1169/1169 и целевые PlayMode 2/2 PASS, 0 failed/skipped. Полный PlayMode
не завершился из-за падения Unity в render loop; его PASS не заявлен.
[Изменение и проверки](evidence/2026-10-01-ordinary-kill-thresholds.md).

Прыжок финальных боссов 2026-09-30 — Implemented по
[DECISION-0128](../decisions/0128-boss-teleport-impact-radius.md): радиус удара
при приземлении BOSS-001…010 уменьшен с 3.5 до 2.8 world units. Урон, тайминг,
точка приземления и trigger distance не менялись. Unity EditMode проверка
ожидает доступного test runner; generation check PASS.

Путники 2026-09-30 — Implemented по [DECISION-0127](../decisions/0127-traveler-initial-health-scaling.md):
Путник, появившийся в 0:00, получает треть прежнего здоровья. Его HP плавно
достигает прежней шкалы к последнему допустимому появлению; урон сохраняет K.
Проверки этого изменения перечислены в следующей записи evidence.

Открытия 2026-09-30 — Implemented по [DECISION-0125](../decisions/0125-fields-and-achievement-unlocks-proposal.md):
70 MetaEconomy IDs используют утверждённые условия по полям, достижениям и
монетам; профиль v3 сохраняет накопительный прогресс, UI показывает пороги,
а герой выдаёт своё стартовое умение. FIELD-001 остаётся принятым; ручная
оценка FIELD-002/003 открыта; FIELD-004 реализован, его полный ручной прогон открыт;
FIELD-005…010 пока без production geometry и полных волн. Форма обратной связи для игроков — отдельный открытый UI/сервисный
packet: точка входа, поля и доставка ещё не утверждены. По сообщению игрока
и [DECISION-0126](../decisions/0126-obstacle-transparent-padding-contact.md)
подогнаны горизонтальные коллайдеры player-only препятствий FIELD-001…003 к
видимой форме prop sprites; ручная оценка контакта в движении открыта.
Full graphics 1146/1146 EditMode + 60/60 PlayMode PASS, 0 failed/skipped,
generation/audio и art provenance 273/273 PASS;
`TestResults/checks/20260930T163407-043713Z/summary.json`.
[Изменения и проверки](evidence/2026-09-30-achievement-unlocks-and-obstacle-contact.md).

Дельта 2026-09-30 по [DECISION-0117](../decisions/0117-skill-reach-and-xp-crystal-scale.md)
— Implemented: базовая дальность всех active skills через характеристики
CHAR-001…010 ×0.8; визуальный масштаб XP-кристалла и базовый радиус его
подбора ×0.85. Генератор синхронизирован; после согласования параллельных UI
и balance-правок полный graphics smoke 1101/1101 EditMode + 59/59 PlayMode,
art manifest 270/270 PASS (`TestResults/checks/20260930T115751-305090Z`).
[Изменения и проверки](evidence/2026-09-30-skill-range-and-xp-crystals.md).

Дельта UI по [DECISION-0118](../decisions/0118-closed-maps-and-ui-scale.md)
— Implemented: закрытые карты размыты и скрывают название; Field Select
показывает 10 карт двумя рядами, настройка масштаба интерфейса Авто/100/125/150%
сохраняется, драфт выбирается мышью. Согласование тестов и документов после
трёх параллельных сессий завершено; full graphics 1101/1101 + 59/59 PASS,
Python tools 26/26 и балансные валидаторы PASS. Ручной просмотр нового UI остаётся
открытым. [Evidence](evidence/2026-09-30-parallel-session-reconciliation.md).


Снаряды 2026-09-30: по [DECISION-0116](../decisions/0116-projectile-scale-and-disk-circle.md)
камень −10%, клинок −15%, сфера −10%, бумеранг без изменений; базовый
рикошетный диск имеет новый вид строго сверху, видимый диаметр 0.324 и
совпадающий hit radius 0.162 (L6 0.1944). v002 подключён с сохранённым GUID;
art graphics 63/63 и manifest 270/270 PASS; стабильный full graphics
1086/1086 EditMode + 59/59 PlayMode PASS, 0 failed/skipped
(`TestResults/checks/20260930T100714-126686Z`). Игровой visual review
вращения и рикошетов открыт. [Сравнение и проверки](evidence/2026-09-30-projectile-size-and-disk-preview.md).


Размеры тел 2026-09-30 — Implemented, игровая визуальная приёмка открыта:
[DECISION-0114](../decisions/0114-body-scale-and-boomerang.md) уменьшает
CHAR-001…010 на 25%, обычных ENEMY-001…020 на 20%, увеличивает видимый
SKILL-006 с 1.4 до 1.6. Боссы, mini-bosses и Путники сохраняют масштаб.
[Общая картинка и проверки](evidence/2026-09-30-body-scale-review.md).
Full graphics: 1085/1085 EditMode + 59/59 PlayMode PASS, art 269/269;
`TestResults/checks/20260930T091916-526918Z/summary.json`.


Дельта 2026-09-30 по [DECISION-0115](../decisions/0115-shared-300-enemy-cap.md) — Implemented: общий технический предел обычных врагов FIELD-001/002/003 повышен с 200 до 300; generator проверяет равенство исходных пакетов. Targeted EditMode 252/252 и graphics PlayMode 59/59 PASS; полный check и ручная оценка плотности открыты ([evidence](evidence/2026-09-30-shared-300-enemy-cap.md)). Исторические записи ниже сохраняют значения своих проверенных ревизий.

Историческая дельта 2026-09-30 по поручению пользователя: [DECISION-0105](../decisions/0105-continuous-cap-replacement.md)
сохраняла тогдашний cap 200, но продолжает спавн за счёт бесшумного удаления самых дальних
обычных врагов; boss/mid-boss/Traveler не входят в cap. Human recorder получил
`activeFirst15/v1` и возврат скорости на 1× после нажатия speed-кнопки.
Первый human сеанс завершился incomplete (`humanSpeedChanged`) и не является
данными для обучения. [Evidence](evidence/2026-09-30-cap-replacement-and-human-draft.md).
Player `f5fd816` собран и сверен по хешам; ручной плейтест выполнен на этой версии.
Human session 2026-09-30: три завершённых забега/14 016 валидных samples,
четвёртый пустой `.partial` исключён; пользователь не заметил проблем со
спавном/исчезновением. [Запись](evidence/2026-09-30-human-demonstration-session.md),
[выбранный report и отзыв](../playtests/2026-09-29_8dde1f80.md).
Offline imitation candidate обучен на трёх human runs: на каждом held-out run
хуже повторения предыдущей команды, в игру не подключён. [Evidence](evidence/2026-09-30-human-imitation-candidate.md).
Recorder теперь фиксирует каждый шаг смены движения между плановыми samples;
полный graphics smoke 1051/1051 + 58/58 PASS. Новый player `e094afc` собран,
bot-labelled pilot 1141 samples validator PASS.
[Evidence](evidence/2026-09-30-action-change-recorder.md).
Новый human сеанс на `develop-evg` `a568e9d`: 4 завершённых забега,
11 687 валидных samples, один административно прерванный забег исключён.
Offline кандидат на четырёх held-out забегах уступил repeat-previous baseline
по общей точности; в игру не выбран. Пользователь проблем в игре не заметил.
[Запись и оценка](evidence/2026-09-30-human-action-change-session.md).
Двухэтапная offline-проба `change/keep → direction` почти не распознаёт момент
смены: 37/3 870 поворотов, gate AUC 0.536…0.563; модель не выбрана для игры.
[Диагностика](evidence/2026-09-30-two-stage-imitation-probe.md).
История наблюдений 0,4/1,2 s и 1/3 s не дала существенного выигрыша:
44 и 35 угаданных смен из 3 870; policy не выбрана.
[Temporal evidence](evidence/2026-09-30-temporal-imitation-probe.md).

Автоматические прогоны 2026-09-29 — [IP-34](modules/IP-34-automated-balance-runs.md),
`automated-runs-v7`: AB-01…14 реализованы и проверены в пределах scoped приёмки;
AB-14 — recorder Verified: full graphics 1043/1043 + 56/56, Python 25/25,
новый player и тихий bot-labelled pilot (191 samples), 17 обычных profile/settings файлов неизменны;
worktree перенесён на D:; активные сохранения безопасно возвращены на C: после
проверки несовместимости LocalLow junction, полная копия на D: сохранена.
Реальная человеческая запись проведена 2026-09-30; offline кандидат обучен,
но в игру не выбран.
AB-13 проверен как ограниченный эксперимент с поиском траекторий, но качество
бота для балансных прогонов не достигнуто (три ранних поражения)
([DECISION-0097](../decisions/0097-automated-balance-runs-v1.md),
[DECISION-0104](../decisions/0104-balance-runner-presentation.md));
[scoped очередь](#automated-runs-execution) содержит фактические проверки.
Интеграция `develop-evg` до `69ea7d1` в `ip34-automation` 2026-09-29:
full graphics 1034/1034 EditMode + 52/52 PlayMode, generation/audio/art 269 PASS;
[evidence](evidence/2026-09-29-ip34-develop-integration.md). Старые bot-пилоты
относятся к прежнему gameplay build, не к новой anti-blob реализации.
Повторная интеграция `develop-evg` до `2ea7a83` 2026-09-30: новый full graphics
1044/1044 EditMode + 57/57 PlayMode, Python 25/25, generation/audio/art 269 PASS;
[evidence](evidence/2026-09-30-ip34-develop-refresh.md). Вошли opening spawn ×0.6,
bias 1.0, невидимый периметр и Space focus fix; ручные gates исходной ветки сохранены.
Новый player для записи/плейтеста собран по `f5fd816` и сверен по хешам;
ручной запуск остаётся открытым.
Остальные UI/gameplay поручения и паузы сохраняются.

UI Folio polish 2026-09-29 — Implemented: по явному поручению пользователя
выполняется отложенная чистовая отделка кнопок и фоновых поверхностей из
DECISION-0081. Общий процедурный фон и полная button state matrix применены к
Entry, Settings, Meta и Results без изменения layout/flow. Новых raster assets нет.
Full graphics PASS: 949/949 EditMode + 39/39 PlayMode, 0 failed/skipped,
art 268 PASS. [Evidence](evidence/2026-09-29-ui-folio-polish.md).
Пользователь принял visual review 2026-09-29: «Хорошо, что дальше?».
UI follow-up 2026-09-29 — Implemented, ожидает visual review пользователя:
[OBS-01…06](../playtests/2026-09-29_ui-folio-followup.md). Угловые рамки и
линейные «бумажные» штрихи удалены; сохранены только тональные градиенты.
Стартовый синий кадр/оранжевый fixture закрыты folio-цветом, production default
управления мышью подтверждён выключенным, Meta purchase reason стабилен во время
сохранения, а Unlocks использует сегментированные состояния и автоматически
показывает только закрытые умения/сеты. Новая материальная фактура отложена до
следующей визуальной итерации. Full graphics PASS: 964/964 EditMode + 39/39
PlayMode, 0 failed/skipped, art 268 PASS
(`TestResults/checks/20260929T094640-646705Z/summary.json`).

UI Folio material 2026-09-29 — Implemented, ожидает visual review в Unity:
пользователь выбрал третий образец (мягкие складки) и утвердил отдельный raster.
Он подготовлен по art pipeline и подключён к крупным окнам с пониженной
непрозрачностью; мелкие карточки не текстурируются. Full graphics PASS:
965/965 EditMode + 39/39 PlayMode, 0 failed/skipped, art 269 PASS
(`TestResults/checks/20260929T103847-651304Z/summary.json`).
[Evidence](evidence/2026-09-29-ui-folio-material.md).

Отзыв 2026-09-29 [OBS-01…03](../playtests/2026-09-29_ui-material-and-reward.md):
материал больших панелей усилен до 0.48 opacity и добавлен в обе колонки Pause;
стартовый L1 исключён из награды по [DECISION-0098](../decisions/0098-earned-level-run-reward.md).
Изменения Implemented; visual review усиленной фактуры в игре и ручной повтор
раннего выхода остаются за пользователем. Full graphics PASS: 969/969 EditMode +
39/39 PlayMode, 0 failed/skipped, art 269 PASS
(`TestResults/checks/20260929T110557-960511Z/summary.json`).
[Reward evidence](evidence/2026-09-29-earned-level-reward.md),
[UI evidence](evidence/2026-09-29-ui-folio-material.md).

Meta backdrop / field collection feedback 2026-09-29 — Implemented: полноэкранная
тёмная подложка Meta/results и все десять готовых иллюстраций карт в коллекции.
Gameplay definitions FIELD-004…010 и доступность запуска не меняются.
Full graphics PASS: 949/949 EditMode + 39/39 PlayMode, 0 failed/skipped,
art 268 PASS. [Evidence](evidence/2026-09-29-meta-backdrop-field-art.md).
Ожидается пользовательский просмотр.

Delta 2026-09-29 — Verified (автоматические проверки): начальные controls 1/1 по DECISION-0094,
без изменения персональных покупок; full graphics 947/947 EditMode + 39/39 PlayMode,
0 failed/skipped. [Evidence](evidence/2026-09-29-starting-draft-controls.md).
Meta stat icons — Implemented: 12 финальных кандидатов утверждены пользователем
«отлично» 2026-09-29 и подключены в Unity; slot 32 px перед названием без роста
высоты. Full graphics 948/948 EditMode + 39/39 PlayMode, 0 failed/skipped,
art 268 PASS; [evidence](evidence/2026-09-29-meta-stat-icons.md).
Ожидается пользовательский просмотр интегрированного экрана.

Meta feedback 2026-09-29 — Implemented, ожидает ручного просмотра: [OBS-01…05](../playtests/2026-09-29_meta-feedback.md),
first-entry icons, стабильные строки, смысловой порядок и ясные счётчики unlocks.
Стартовый состав и сохранение не меняются. Свежий full graphics PASS:
946/946 EditMode + 39/39 PlayMode, 0 failed/skipped; generation/audio/art PASS.
[Evidence](evidence/2026-09-29-meta-feedback.md), `TestResults/checks/20260929T071014-911383Z/summary.json`.

Settings R1: 2026-09-29 пользователь поручил «Следующий шаг». Разрешён
композиционный проход [Настроек](proposals/2026-09-29-ui-settings-r1.md);
HTML-макет принят пользователем («Принимаю, переноси в юнити»); перенос
Unity Settings — Verified, без новых настроек и без изменения service ownership.
Full graphics 946/946 EditMode + 39/39 PlayMode PASS; после USS-коррекции controls
targeted Settings PlayMode 3/3 PASS, 0 failed/skipped.
[Runtime evidence](evidence/2026-09-29-ui-settings-r1-runtime.md). Пользователь
подтвердил ручной просмотр в игре 2026-09-29: «там всё принимается».
Новых gameplay-IP автоматически не начинать.
Это не утверждение о ручном прогоне Meta и не разрешение на gameplay-IP.

UI Unlocks review 2026-09-29: по поручению пользователя подготовлен полный
[браузерный макет](proposals/ui-meta-r1/README.md#полная-коллекция-открытий--2026-09-29)
на 70 определениях. Фильтры, 1080p/720p, покупка/ошибка и общая прокрутка проверены
в браузере. 2026-09-29 пользователь согласовал продолжение; перенос «Открытий»
в Unity — Implemented; full graphics 945/945 + 38/38 PASS, [evidence](evidence/2026-09-29-ui-unlocks-r1-runtime.md). Замечание о силуэтах карт/сетов отменено: скрыты только
неоткрытые персонажи. Это не приёмка Unity Meta; последующее поручение по Settings выше.

- Дизайн `design-sync-R2` и 121 исходная карточка утверждены (DECISION-0015); оставшиеся TBD и новые proposals не получают approval автоматически.
- FIELD-001: F1-00…09 Verified; полный ручной прогон и пользовательская приёмка закрыты 2026-09-28, exact stress-performance и restart audit PASS. Автоматического перехода к следующему полю нет.
- FIELD-002: F2-01…05 поставлены; F2-06 ждёт ручного прогона. IP-12A gameplay density review остаётся отдельным открытым gate.
- 2026-09-29: ребаланс волн FIELD-002/003 на ритм FIELD-001 (16 фаз, передышки 15 s) и множитель поля HP ×(1+0.2·(N−1)), урон ×(1+0.1·(N−1)) — [field-rhythm-v2](../balance/field-rhythm-v2.md), Approved ([DECISION-0095](../decisions/0095-field002-003-rhythm-v2.md)), Implemented: генератор выпускает `ProductionWaveTimelineField002/003.json` из v2, регрессия ритма в `ProductionField002ContentTests`. Full graphics PASS 946/946 EditMode + 37/37 PlayMode, 0 failed/skipped; generation/audio/art 256 PASS (`TestResults/checks/20260929T071126-528915Z/summary.json`). Ощущение сложности ждёт ручных прогонов F2-06 и FIELD-003.
- 2026-09-29, [DECISION-0096](../decisions/0096-field-curve-meta-bonus-xp-book.md), Implemented: множитель поля смягчён до HP ×(1+0.15·(N−1)), урон ×(1+0.08·(N−1)) (FIELD-002 ×1.15/×1.08, FIELD-003 ×1.30/×1.16); мета-бонусы META-003…014 ×1.5 при прежних ценах; кривая опыта по полям для пакетов FIELD-004+ (полный билд ≈L75 к 12:00 FIELD-010); Книга Путника даёт 1/2/3 выбора с весами 50/35/15% (`BookUpgradeCount`). Full graphics PASS 958/958 EditMode + 37/37 PlayMode, 0 failed/skipped; generation/audio/art 256 PASS (`TestResults/checks/20260929T075518-536090Z/summary.json`). Ручная проверка Книги и меты в игре открыта.
- Поздние каталоги и поля сохраняют свои prerequisites/остатки в записях IP. Ни approval арта, ни пройденные автоматические тесты не заменяют gameplay-scale review.
- IP-33 разрешён отдельным поручением вне F1-09; прослушивание остаётся открытым. REPO-01 разрешает только предложенный структурный рефакторинг и его проверки, без изменения баланса и без запуска следующего IP.
- UI layout R2: стиль DECISION-0081 подтверждён, temporary layout не принят.
  [DECISION-0083](../decisions/0083-player-ui-layout-and-dev-boundary.md) фиксирует
  HP возле персонажа и DEV-only speed; [DECISION-0086](../decisions/0086-ui-review-density-and-inspection.md)
  фиксирует принятые комментарии к [композиции R2](proposals/2026-09-28-ui-layout-r2.md):
  компактный 720p без Pause button, inspect/confirm, reachable recipe list,
  увеличенные compact missed, 3/2 колонки Pause и baseline speed.
  Пользователь принял HTML-композицию и разрешил следующий шаг: runtime-срез
  HUD → Draft/Book → Pause реализован, область персонажа 100×96 / 140×120 px.
  Есть Unity captures и regression; 2026-09-28 пользователь явно принял
  игровой вариант: «Приемка, считай сделана, я уже посмотрел, интерфейс нормальный».
  Разрешён следующий композиционный шаг Main Menu/Character Select/Field Select.
  Его HTML требует отдельного approval; остальные экраны и каталоги не входят.
- История поручений и оснований: [датированный архив](evidence/2026-09-27-execution-history.md). При выборе работы читать эту шапку, очередь и нужные записи; архив — только при необходимости.

## Execution order

<a id="automated-runs-execution"></a>
### IP-34 — автоматические прогоны, scoped очередь

Применяется только после поручения на реализацию IP-34. Ревизия `automated-runs-v7`;
спецификация и критерии — [план](modules/IP-34-automated-balance-runs.md).
Поручение пользователя охватывает AB-01…08 последовательно, AB-09…13 отдельно; общий backlog не
возобновляется.

| Порядок | Packet | Status | Prerequisite / следующий шаг |
|---:|---|---|---|
| 1 | [AB-01 — конфигурация и изоляция профилей](modules/IP-34-automated-balance-runs.md#ab-01) | Verified | 6/6 EditMode + 3/3 Python, [evidence](evidence/2026-09-29-ip34-automation.md#ab-01) |
| 2 | [AB-02 — наблюдение и движение](modules/IP-34-automated-balance-runs.md#ab-02) | Verified | Full graphics 975/975 + 40/40, XP PlayMode 2/2; [evidence](evidence/2026-09-29-ip34-automation.md#ab-02) |
| 3 | [AB-03 — один автономный забег](modules/IP-34-automated-balance-runs.md#ab-03) | Verified | 9 natural completions plus fixture lifecycle tests; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 4 | [AB-04 — отчёт и история развития](modules/IP-34-automated-balance-runs.md#ab-04) | Verified | Natural sidecars/profile snapshots; overflow/export/duplicate tests in final 52/52 PlayMode; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 5 | [AB-05 — campaign и межзабеговая прогрессия](modules/IP-34-automated-balance-runs.md#ab-05) | Verified | Two independent chains per template, purchases in preset, fixture route advance; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 6 | [AB-06 — standalone и локальный runner](modules/IP-34-automated-balance-runs.md#ab-06) | Verified | Silent player, full multi-chain series, 600 s partial; visual SafeWindow fix and rebuilt player, next on-screen check pending; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 7 | [AB-07 — статистика и сравнение](modules/IP-34-automated-balance-runs.md#ab-07) | Verified | 18/18 Python tests and real-series groups/censoring; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 8 | [AB-08 — пилот и измерение скорости](modules/IP-34-automated-balance-runs.md#ab-08) | Verified | 8 natural template runs, separate 1× and 600 s window, full graphics 979/979 + 52/52; [evidence](evidence/2026-09-29-ip34-automation.md#ab-08) |
| 9 | [AB-09 — XP-focused bot profile](modules/IP-34-automated-balance-runs.md#ab-09) | Verified | Отдельный ID и пример; Unity 983/983 + 52/52, Python 18/18, тихий production pilot; [evidence](evidence/2026-09-29-ip34-xp-bot.md). |
| 10 | [AB-10 — широкий обход за XP](modules/IP-34-automated-balance-runs.md#ab-10) | Verified | Дуга 6 world units и выбор безопасной XP-цели; Unity 990/990 + 52/52, Python 18/18, тихий pilot; [evidence](evidence/2026-09-29-ip34-orbit-bot.md). |
| 11 | [AB-11 — заманивание кучи и возврат за XP](modules/IP-34-automated-balance-runs.md#ab-11) | Verified | Режимы и telemetry, Unity 996/996 + 52/52, Python 18/18, два тихих пилота с низким XP; [evidence](evidence/2026-09-29-ip34-herd-bot.md). |
| 12 | [AB-12 — диагностический трек и адаптивный обход](modules/IP-34-automated-balance-runs.md#ab-12) | Verified | Отдельный ID, ограниченный трек; Unity 999/999 + 52/52, Python 18/18, две тихие серии по три забега; [evidence](evidence/2026-09-29-ip34-adaptive-herd-bot.md). |
| 13 | [AB-13 — поиск траекторий с моделью преследования](modules/IP-34-automated-balance-runs.md#ab-13) | Verified | Эксперимент: closed-loop сценарии, Unity 1013/1013 + 52/52, Python 18/18; production 3/3 ранних поражения, пригодность бота не установлена; [evidence](evidence/2026-09-29-ip34-trajectory-bot.md). |
| 14 | [AB-14 — запись демонстраций управления](modules/IP-34-automated-balance-runs.md#ab-14) | Verified | Python 25/25; final full graphics 1043/1043 + 56/56; отдельный player, тихий bot-labelled pilot 191 samples, validator PASS, 17 обычных profile/settings файлов неизменны. Human session: 3 завершённых забега/14 016 samples; offline кандидат обучен позднее, в игру не выбран. [Implementation evidence](evidence/2026-09-29-ip34-demonstration-recording.md), [human evidence](evidence/2026-09-30-human-demonstration-session.md), [training evidence](evidence/2026-09-30-human-imitation-candidate.md). |

Строгий replay, новый fast simulation loop, автоподбор чисел и vision не входят
в эту очередь. Имеющиеся полные/ручные проверки других IP не считаются evidence IP-34.

При разрешении на исполнение выбирать первый Ready packet активного этапа ниже,
если пользователь не назвал другой scope. Пока этап активен, поздний backlog
автоматически не выбирать. Порядок IP после этапа сохранён во второй таблице.

<a id="field001-execution"></a>
### FIELD-001 initial slice — приоритетная очередь

Все packets относятся к `field-001-start-R1`. Status ниже относится к packet,
а не к полному каталожному IP. Успех стартового поднабора не закрывает весь каталог.

| Приоритет | Packet / владельцы | Status | Prerequisites / конкретный gate |
|---:|---|---|---|
| 1 | [F1-00 — полные данные](milestones/FIELD-001-start.md#f1-00); IP-17…26/30/32 | Verified | 2026-09-24: baseline v1 Approved (DECISION-0053), canon синхронизирован; static validator PASS; [evidence](evidence/field001-baseline-v1-2026-09-23.md#approval-2026-09-24) |
| 2 | [F1-01 — 10 skills](milestones/FIELD-001-start.md#f1-01); IP-17 | Verified | SKILL-001…007/010/013/014 (L1–6, art/VFX); Unity 709/709 + 26/26, 2026-09-24. [Evidence](evidence/field001-f1-01-2026-09-24.md), [общий прогон](evidence/field001-f1-08-2026-09-24.md#unity-full-pass) |
| 3 | [F1-02 — 10 passives](milestones/FIELD-001-start.md#f1-02); IP-18 | Verified | 2026-09-24: completed IDs PASSIVE-001…005/007…009/011/012 (L1–6, icons); Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-02-2026-09-24.md) |
| 4 | [F1-03 — Клёпка/profile/UI](milestones/FIELD-001-start.md#f1-03); IP-22/25/26 | Verified | 2026-09-24: CHAR-001 production definition/visual binding, MetaEconomy по DECISION-0050, миграция при загрузке; Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-03-2026-09-24.md) |
| 5 | [F1-04 — enemies/potion](milestones/FIELD-001-start.md#f1-04); IP-20 | Verified | ENEMY-001…005/007 + PICKUP-001; тела ENEMY-003/004/005/007 подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-04-2026-09-24.md) |
| 6 | [F1-05 — 5 sets](milestones/FIELD-001-start.md#f1-05); IP-19 | Verified | 2026-09-24: SET-001/004/006/010/017 (пороги, эффекты, SET-017 attack/telegraph); Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-05-2026-09-24.md) |
| 7 | [F1-06 — boss/mid-boss](milestones/FIELD-001-start.md#f1-06); IP-21 | Verified | BOSS-001/MIDBOSS-001; тела и общий снаряд веера/кольца подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-06-2026-09-24.md) |
| 8 | [F1-07 — 3 Travelers/Book](milestones/FIELD-001-start.md#f1-07); IP-30 | Verified | TRAVELER-001/002/005 + FIELD-001 schedule, PICKUP-002; три тела подключены, Unity 709/709 + 26/26; текущий вид принят пользователем 2026-09-24. [Art review](../playtests/2026-09-24_field001-art-acceptance.md), [art evidence](evidence/field001-art-integration-2026-09-24.md), [packet evidence](evidence/field001-f1-07-2026-09-24.md) |
| 9 | [F1-08 — production field/run](milestones/FIELD-001-start.md#f1-08); IP-23/24/25/26 | Verified | 2026-09-24: FIELD-001 (поле, 900-s timeline, 64 authored player-only obstacles), production composition без fixture fallback, production профиль `profile-v1.json`; Unity full PASS 2026-09-24 ([Unity 709/709 + 26/26](evidence/field001-f1-08-2026-09-24.md#unity-full-pass)); [evidence](evidence/field001-f1-08-2026-09-24.md), [DECISION-0054 §9](../decisions/0054-field001-autonomous-execution.md#9-конкретизации-f1-08) |
| 10 | [F1-09 — доведение/приёмка](milestones/FIELD-001-start.md#f1-09); IP-27/12A/31/32 | Verified | Полный ручной прогон и ощущение карты приняты пользователем 2026-09-28. Performance после DECISION-0082: load, minute-5/minute-10, exact final stress `250 + 2 bosses + 3 Travelers` (`p95 16.673 ms`, `p99 16.680 ms`) и 10 restarts PASS; [performance evidence](evidence/2026-09-28-field001-performance.md), [матрица](evidence/field001-f1-09-2026-09-24.md) |

### Пользовательские правки FIELD-001 — 2026-09-27

По [DECISION-0069](../decisions/0069-field001-feedback-tuning.md) заменён и приглушён level-up cue, ослаблены damage/range/size SKILL-006, смягчена оранжевая вспышка SKILL-014, поле уплотнено до 288 препятствий четырёх типов по миниатюре, HUD показывает обратный отсчёт 15:00 → 00:00. Runtime, исходные данные генератора, арт-пакет и документы синхронизированы. EditMode 855/855 и PlayMode 30/30 PASS; art scope 50/50 и 151 provenance record PASS; audio integrity 28/28, layouts 200 сидов PASS. Художественный review и игровой баланс после изменений ждут пользовательского прогона. [Evidence](evidence/2026-09-27-field001-feedback-tuning.md).

Дополнительный отзыв 2026-09-27: на стартовом экране FIELD-001 гарантированы два видимых объекта вне свободного круга радиуса 6 ([DECISION-0070](../decisions/0070-field001-opening-screen-obstacles.md)); SKILL-007 L1–L3 слегка ослаблен до 22/22/27.5 damage ([DECISION-0071](../decisions/0071-early-chain-lightning-damage.md)). Safe full check: EditMode 856/856, PlayMode 30/30, art provenance 151/151 PASS; layout validator 200 сидов PASS. Игровой баланс ждёт ручного прогона. [Evidence](evidence/2026-09-27-opening-screen-and-lightning.md).
Плейтест 2026-09-27 (вечер), [DECISION-0073](../decisions/0073-playtest-2026-09-27-sets-boss-fields-balance.md): в списке сетов только meta-открытые, упущенные сеты внизу; порог прыжка боссов 6.25 units (+25%); все пропсы FIELD-002/003 появляются в каждом забеге как варианты кусков паттернов; урон умений выровнен и снижен (медиана ×0.7). EditMode 866/866, PlayMode 30/30 PASS. Сложность ждёт ручного прогона. [Evidence](evidence/2026-09-27-playtest-sets-boss-fields-balance.md).

Подлаг перед первым level-up ([DECISION-0074](../decisions/0074-pickup-hitch-and-fresh-drop-seed.md)): первый дроп зелья строил visual и полный поиск точки внутри смерти врага (88 ms в PlayMode, 372 ms в Editor); пул прогревается при сборке забега, размещение сначала проверяет прямой отрезок от игрока — 1.2 ms. Броски и разброс дропа получают свежий seed на каждый забег.

Ребаланс прогрессии ([DECISION-0075](../decisions/0075-progression-specialization-and-survivability.md)): первые уровни втрое дороже при прежней сумме XP к L40; стартовое умение персонажа получает специализацию ≈×2 (CHAR-001: +60% damage, +25% action speed); урон врагов по игроку ×0.7; зелье 30 HP; регенерация вдвое слабее; дроп пикапов без поиска пути. EditMode 870/870, PlayMode 30/30 PASS (`TestResults/checks/20260927T172233-150157Z`). Темп и выживаемость ждут ручного прогона.

Ритм волн ([DECISION-0076](../decisions/0076-wave-cap-and-field001-rhythm.md)): `maxAliveEnemies=200` перенесён на timeline и стал единым техническим пределом continuous/burst; FIELD-001 сокращён с 24 до 16 фаз, после быстрого старта combat-фазы 70–90 s, передышки 20 s, составы 2–4 типа. Номинальный поток сохранён: 1736 requests и expected XP 2886.6 против прежних ≈1735/≈2883. Unity full PASS 870/870 + 30/30; ручная оценка темпа/производительности открыта. [Evidence](evidence/2026-09-27-wave-cap-and-field001-rhythm.md).

Ранние вееры и первый враг ([DECISION-0077](../decisions/0077-early-projectile-growth-and-enemy001-speed.md)): projectile counts SKILL-002 теперь 3/4/5/7/9/11, SKILL-013 — 4/5/6/7/9/13; прежние финальные 11/13 сохранены. ENEMY-001 «Селянин с вилами» замедлен ровно на 20%, 1.20 → 0.96. Static validators PASS; Unity full PASS 870/870 EditMode + 30/30 PlayMode, 0 skipped (`TestResults/checks/20260927T191226-569482Z`). Ручная оценка ранней силы и давления открыта. [Evidence](evidence/2026-09-27-early-projectile-growth-and-enemy001-speed.md).

Общая ранняя прогрессия active skills ([DECISION-0078](../decisions/0078-active-skill-early-progression.md)): принцип слабого L1 и возврата прежнего L3 распространён на SKILL-001…016; L4–L6 и финальные значения сохранены. Камень ослаблен мягче, 14 → 9.33 damage (÷1.5); у остальных уменьшены основной count/radius/range/duration/targets с ростом через L2–L3. Static validators PASS; targeted production catalogs 16/16 PASS; Unity full PASS 870/870 EditMode + 30/30 PlayMode, 0 skipped (`TestResults/checks/20260927T193313-833849Z`). Ручная оценка L1–L3 открыта. [Evidence](evidence/2026-09-27-active-skill-early-progression.md).

Плейтест `091d834e` и tuning ([DECISION-0079](../decisions/0079-playtest-sky-strike-radius-and-xp-curve.md)): radius SKILL-010 L1–L6 теперь `0.8/1.3/1.8/1.8/1.8/1.8`, третий удар L6 `×1.35`; первые десять XP thresholds дешевле ровно на 20%, сумма до L40 сохранена на 1257 XP. Generation/static validators и Unity full PASS 870/870 EditMode + 30/30 PlayMode, 0 skipped (`TestResults/checks/20260927T204517-509733Z`). Ручная оценка темпа/радиуса открыта; лаги около 5-й/10-й минут диагностированы отдельно и не считаются исправленными. [Evidence](evidence/2026-09-27-sky-strike-radius-and-xp-curve.md), [playtest review](../playtests/2026-09-27_091d834e.md).

Урон прыжка/телепорт-удара BOSS-001…010 уменьшен ровно в 1.5 раза ([DECISION-0080](../decisions/0080-boss-teleport-damage-reduction.md)): диапазон теперь 13.333333…24 вместо 20…36. Авторинговые balance-данные, Content Design, production catalog и ожидания catalog tests синхронизированы; Unity full PASS 870/870 + 30/30 (`TestResults/checks/20260928T074750-421217Z`). Ручная оценка урона закрыта общей пользовательской приёмкой FIELD-001 2026-09-28.

Пользовательский прогон 2026-09-28: FIELD-001 полностью принят. После CPU-профилирования и [DECISION-0082](../decisions/0082-simple-traveler-protector-targeting.md) standalone benchmark полностью PASS: scene/run load, minute-5, minute-10, 10 restart и exact final stress `250 ordinary + 2 bosses + 3 Travelers` (`p95 16.673 ms`, `p99 16.680 ms`, max `37.367 ms`, GPU p95 `2.262 ms`). F1-09 Verified. [Evidence](evidence/2026-09-28-field001-performance.md).

При завершении добавлять сюда completed IDs, дату/revision и evidence ссылку,
пересчитывать downstream. Успех стартового packet не закрывает весь IP; его
оставшиеся ID перечислены в записи владельца. Принятые baseline frameworks —
зависимости по именам в спецификации packet и записям ниже, не повторные работы.

<a id="field002-execution"></a>
### FIELD-002 slice — очередь (DECISION-0063)

Данные: [field002-v1](../balance/field002-v1.md). Status ниже относится к packet, не к полному IP.

| Приоритет | Packet / владельцы | Status | Prerequisites / gate |
|---:|---|---|---|
| 1 | F2-01 — атака в конце рывка и повторный залп (IP-15/IP-21 framework) | Implemented | 2026-09-26: `EnemyDashVolleyProfile/Controller`, JSON `dashEndAttack`/`dashEndRepeat`, масштаб волн; EditMode 211/211 (Enemy/Traveler/Bootstrap) |
| 2 | F2-02 — BOSS-002/MIDBOSS-002 production encounters (IP-21) | Implemented | 2026-09-26; оба body v001 утверждены, импортированы и подключены; gameplay-scale review открыт. [Encounter evidence](evidence/2026-09-26-field002-slice.md), [art evidence](evidence/2026-09-26-field002-art.md) |
| 3 | F2-03 — поле FIELD-002: геометрия, окружение, выбор поля (IP-23) | Implemented | 2026-09-26; ground, boulder, column, shrine и thumbnail v001 подключены; boundary использует прежний плетень, gameplay-scale review открыт. [Field evidence](evidence/2026-09-26-field002-slice.md), [art evidence](evidence/2026-09-26-field002-art.md) |
| 4 | F2-04 — волны FIELD-002 и модификаторы поля (IP-24) | Implemented | 2026-09-26; [evidence](evidence/2026-09-26-field002-slice.md) |
| 5 | F2-05 — общий пул Путников без повторов ролей (IP-29/IP-30) | Implemented | 2026-09-26; [evidence](evidence/2026-09-26-field002-slice.md) |
| 6 | F2-06 — приёмка: прогоны FIELD-002, сложность, производительность | Blocked | F2-01…05 Implemented (Unity 802/802 + 28/28); нужен ручной прогон пользователя |

### Общий IP backlog после этапа

Порядок сохраняется для оставшегося scope. Возобновлять после команды пользователя;
текущие IP statuses и revision exceptions — в записях ниже.

| Приоритет | Модуль |
|---:|---|
| 1 | [IP-00](modules/IP-00-content-contract.md) |
| 2 | [IP-01](modules/IP-01-run-lifecycle.md) |
| 3 | [IP-02](modules/IP-02-player-movement.md) |
| 4 | [IP-03](modules/IP-03-character-stats.md) |
| 5 | [IP-04](modules/IP-04-enemy-core.md) |
| 6 | [IP-05](modules/IP-05-active-skill-runtime.md) |
| 7 | [IP-06](modules/IP-06-xp-progression.md) |
| 8 | [IP-07](modules/IP-07-level-up-draft.md) |
| 9 | [IP-08](modules/IP-08-active-skill-framework.md) |
| 10 | [IP-09](modules/IP-09-passive-framework.md) |
| 11 | [IP-10](modules/IP-10-reroll-banish.md) |
| 12 | [IP-10A](modules/IP-10A-ui-foundation.md) |
| 13 | [IP-31](modules/IP-31-manual-run-telemetry.md) |
| 14 | [IP-32](modules/IP-32-manual-ai-balance.md) |
| 15 | [IP-11](modules/IP-11-set-framework.md) |
| 16 | [IP-12](modules/IP-12-character-framework.md) |
| 17 | [IP-12A](modules/IP-12A-visual-presentation-foundation.md) |
| 18 | [IP-13](modules/IP-13-enemy-patterns.md) |
| 19 | [IP-14](modules/IP-14-wave-director.md) |
| 20 | [IP-15](modules/IP-15-boss-framework.md) |
| 21 | [IP-16](modules/IP-16-field-framework.md) |
| 22 | [IP-28](modules/IP-28-world-pickups.md) |
| 23 | [IP-29](modules/IP-29-traveler-framework.md) |
| 24 | [IP-25](modules/IP-25-meta-progression.md) |
| 25 | [IP-26](modules/IP-26-functional-ui.md) |
| 26 | [IP-17](modules/IP-17-production-skills.md) |
| 27 | [IP-18](modules/IP-18-production-passives.md) |
| 28 | [IP-19](modules/IP-19-production-sets.md) |
| 29 | [IP-20](modules/IP-20-production-enemies.md) |
| 30 | [IP-21](modules/IP-21-production-bosses.md) |
| 31 | [IP-22](modules/IP-22-production-characters.md) |
| 32 | [IP-23](modules/IP-23-production-fields.md) |
| 33 | [IP-30](modules/IP-30-production-travelers.md) |
| 34 | [IP-24](modules/IP-24-production-waves.md) |
| 35 | [IP-27](modules/IP-27-integration.md) |
| 36 | [IP-33](modules/IP-33-production-audio.md) |

## REPO-01 — Структура, навигация и единые проверки

Status: Verified
Scope: PROJECT_MAP, сокращение истории в STATUS, синхронизация entry rules, отдельный Game.Audio, структура генератора, нейтральное имя RuntimeContentCatalog и включение data checks/fingerprints.
Authorization: пользователь 2026-09-27 поручил реализовать предложенный рефакторинг; [DECISION-0072](../decisions/0072-project-structure-and-audio-ownership.md).
Acceptance: те же gameplay данные и аудиоклипы; сохранённые GUID; ссылки/карта актуальны; Python tooling tests, generation/audio integrity и полный безопасный Unity smoke.
Evidence: 2026-09-27, Python 23/23, Unity 6000.6.0f1 EditMode 857/857 + PlayMode 30/30, 0 skipped; generation/audio/manifest PASS. 39 перенесённых GUID сохранены; статусы всех 36 IP сохранены. [Рефакторинг](evidence/2026-09-27-project-structure.md).

## Scope revisions и готовность

IP-00/IP-02 сохраняют Verified: их behavioral acceptance не изменён, API текущего кода совместим; новые RunOutcome/control/presentation deltas проверяют их владельцы. При последующей несовместимой правке пересмотреть affected evidence.

IP-01 и изменённая основа IP-03…IP-14/IP-10A/IP-12A требуют новых дельт. Их прежнее Verified записано только в [архиве прежнего scope](evidence/pre-design-sync-R2.md). Все dependencies ниже относятся к `design-sync-R2`, если явно не указано иначе. IP-15 больше не Ready по старому IP-14 continuous evidence.

Для catalog packets выполненные ID и remaining scope ведутся здесь; pilot не переводит весь IP в Implemented/Verified. CG-01 approval получен; CG-02/03/04 и [G/W gaps](DESIGN_SYNC.md) учитываются только для зависящего packet. Не требуется повторно утверждать принятые designs.

Общие поля записей: Scope revision = `design-sync-R2` для всех IP, пока явно
не указано иное. Для ещё не начатых IP текущий packet — полная спецификация
либо явно согласованный catalog packet после выполнения prerequisites/gates;
Documentation impact регистрации — scope, prerequisites, gates и consumer links.
Это не implementation/verification evidence. Изменённый packet, выполненные ID,
отклонения и новый documentation impact записываются в конкретный IP.

## Модули

### IP-00 — Контракт контента, стабильные ID и конфигурация

Status: Verified
Dependencies: none
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-00).
Target verification evidence: Сохранённые проверки [неизменного scope](evidence/pre-design-sync-R2.md#ip-00); M-01 проверяет документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-00).

### IP-01 — Run lifecycle, pause ownership и результат забега

Status: Verified
Dependencies: IP-00
Current packet: Run identity, terminal snapshot/RunOutcome, reset/teardown contract; целевой Scope завершён с сохранением готового таймера/pause.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-01).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 257/257, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
2026-09-28 manual-pause input delta DECISION-0084: Escape/Space/right mouse переключают только manual ownership; full PASS 870/870 + 31/31, 0 skipped — [evidence](evidence/2026-09-28-mouse-movement-and-pause-shortcuts.md).
Follow-up 2026-09-29 — Implemented, ожидает игрового просмотра: `Space` больше не подавляется фокусом DEV/HUD-кнопки; перехват сфокусированного control ограничен экраном паузы и popup сета. Новый PlayMode regression **1/1 PASS**, GameplaySmokeTests **3/3 PASS**, 0 failed/skipped. [Evidence](evidence/2026-09-29-space-pause-focus.md).
Documentation impact: IP-01 terminal/time/teardown contract; GDD и UI manual-pause shortcuts синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-01).

### IP-02 — Перемещение игрока, камера и базовая геометрия

Status: Verified
Dependencies: IP-01
Current packet: DECISION-0084 mouse movement delta реализована; keyboard остаётся default, pointer deadzone = 1 world unit.
Follow-up 2026-09-29 — Implemented, ожидает игрового просмотра: камера останавливается так, чтобы видимая область не выходила за поле; игрок упирается в прежнюю физическую границу, follow возобновляется после отхода, Screen Shake тоже ограничен ([DECISION-0101](../decisions/0101-camera-field-edge.md), [OBS-01](../playtests/2026-09-29_camera-field-edge.md#obs-01--край-поля-остаётся-в-кадре)). Targeted Movement/Bootstrap EditMode **53/53 PASS**, 0 failed/skipped; PlayMode не дал result XML из-за падения Unity в render loop; [evidence](evidence/2026-09-29-camera-field-edge.md). Игровой визуальный результат ещё не принят.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Автоматизированный scope закрыт; ощущение deadzone в standalone остаётся ручной проверкой, не блокирует функциональный contract.
Target implementation evidence: [DECISION-0084 delta](evidence/2026-09-28-mouse-movement-and-pause-shortcuts.md); прежняя база — [design-sync-R2](evidence/design-sync-R2-2026-09-21.md#ip-02).
Target verification evidence: 2026-09-28 Unity 6000.6.0f1: full PASS 870/870 EditMode + 31/31 PlayMode, 0 skipped; targeted 126/126 + composed 1/1 — [evidence](evidence/2026-09-28-mouse-movement-and-pause-shortcuts.md).
Documentation impact: GDD/UI, IP-01/02/26 и DECISION-0084 синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-02).

### IP-03 — Character stats, Health и новые stat channels

Status: Verified
Dependencies: IP-01, IP-02
Current packet: Целевой scope завершён; состав реализации — в evidence.
Remaining gates: G-08/G-09 закрыты DECISION-0017. IP-05 фиксирует damage при активации; parameter mapping реализует IP-08.
Remaining acceptance / IDs: none; G-08/G-09 remain gates of consuming IPs.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-03).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 283/283, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: stat/units/JSON dictionary в IP-03, terminology в DECISION-0004; GDD/CD formulas unchanged; applicability оставлена G-08/G-09.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-03).

### IP-04 — Enemy lifecycle, contact damage и per-life identity

Status: Verified
Dependencies: IP-02, IP-03
Current packet: Целевой scope завершён; состав реализации — в evidence.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-04).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 289/289, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: IP-04 lifecycle contract; DECISION-0016 Proposed (architecture review), GDD/CD rules unchanged; TD-001/003 mitigation documented without rewriting debt register.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-04).

### IP-05 — Общий combat pipeline, control effects и target contract

Status: Verified
Dependencies: IP-03, IP-04
Current packet: Unified combat attribution/results, movement-only slow, additive knockback и общий target-query contract.
Remaining gates: G-06…G-09 resolved by DECISION-0017; production control tuning belongs to later content packets.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-05).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 311/311, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: DECISION-0017 approved; GDD combat, PASSIVE-014, DESIGN_SYNC, proposal и affected IP gates синхронизированы. Size/range mapping остаётся реализацией IP-08, set propagation — IP-11.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-05).

### IP-06 — XP lifecycle, effective pickup radius и progression

Status: Verified
Dependencies: IP-04, IP-05
Current packet: Effective XP radius, source/drop identities, producer events и separate base/awarded lifetime totals.
Remaining gates: Нет дополнительных product gaps для указанного scope.
Remaining acceptance / IDs: none.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-06).
Target verification evidence: 2026-09-20, Unity 6000.6.0f1: Game.* EditMode 320/320, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Production tuning 2026-09-27: XP thresholds обновлены по DECISION-0079; первые 10 стоят 160 вместо 200 XP, сумма до L40 остаётся 1257. Generation/static validators и Unity full PASS 870/870 + 30/30; [evidence](evidence/2026-09-27-sky-strike-radius-and-xp-curve.md).
Documentation impact: IP-06 units/producer contract и fixture rationale; DECISION-0018 Proposed для архитектурного ревью реализации принятого scope. Product formulas PASSIVE-006/007/010 не изменены; G-01/G-03 позднее закрыты DECISION-0019/0020 в IP-07. IP-07 добавил atomic LevelsEarned range перед legacy per-level events, чтобы одна XP награда ставила requests подряд.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-06).

### IP-07 — Трёхслотовый драфт, request queue и build progression

Status: Verified
Dependencies: IP-01, IP-06
Current packet: Целевой fixture framework завершён; общая очередь/preview/revisions и immediate empty-Book currency по DECISION-0019/0020.
Remaining gates: Нет для IP-07. Production Book ID/сумма/lifetime — IP-28/IP-30/IP-25; G-02 закрыт DECISION-0022; controls snapshot — IP-10, global set chance поставлен IP-11; production значение остаётся balance-data.
Remaining acceptance / IDs: Нет для принятого scope IP-07.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-07).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 343/343, PlayMode 1/1 passed; coverage/условия — по ссылке выше.
Documentation impact: GDD XP/Book/meta rules, UI §§7/12, approved DECISION-0019/0020, DESIGN_SYNC, proposal и consumer IP-10/IP-10A/IP-11/IP-25/IP-28 синхронизированы. Fixture currency = 1 не утверждает production баланс. Legacy per-set fixture chance позднее заменён единым provider в IP-11; production значение не назначено.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-07).

### IP-08 — Active-skill levels, targeting и effect families

Status: Verified
Dependencies: IP-05, IP-07
Current packet: Framework design-sync-R2, 13 fixture definitions L1…L6; production IDs/art остаются IP-17.
Remaining gates: Для framework нет. G-08/G-09 закрыты DECISION-0017, additive level bonuses — DECISION-0021. G-04 остаётся только affected production/set gate; return для SKILL-008 не выдуман.
Remaining acceptance / IDs: Нет в обязательном framework scope; production SKILL-001…016 не зарегистрированы.
Target implementation evidence: Targeting, cumulative JSON resolver, spatial mapping, shared boomerang ledger, deceleration/pool, diagnostics и compatibility matrix — [IP-08 evidence](evidence/design-sync-R2-2026-09-21-ip08.md#ip-08).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1, Game.* EditMode 364/364, PlayMode 2/2 passed, 0 skipped. Условия и coverage — по ссылке выше.
Documentation impact: Content Design additive upgrades, approved DECISION-0021, IP-08 parameter/code/test matrix и consumer IP-03/IP-09/IP-17 синхронизированы. Fixture numbers не утверждают production balance.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-08).

### IP-09 — Passive modifiers и новые stat effects

Status: Verified
Dependencies: IP-03, IP-06, IP-07, IP-08
Current packet: Framework compatibility matrix PASSIVE-001…014; 9 non-production fixture definitions L1…L6, keyed lifecycle и slot descriptions.
Remaining gates: Нет для framework; actual potion roll/cap поставлен IP-28 по approved DECISION-0033, production definitions/icons — IP-18.
Remaining acceptance / IDs: Нет для обязательного framework scope; production PASSIVE-001…014 не зарегистрированы.
Target implementation evidence: Channels/migration/default ownership, reinitialize cleanup и dynamic UI — [IP-09 evidence](evidence/design-sync-R2-2026-09-21-ip09.md#ip-09).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 369/369, PlayMode 2/2 passed, 0 skipped. Условия и coverage — по ссылке выше.
Documentation impact: IP-09 mapping/defaults и PASSIVE-007 migration, IP-18 consumer contract, regression map и готовность потребителей синхронизированы. GDD/CD и production balance не изменены.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-09).

### IP-10 — Reroll/banish для обновлённого драфта

Status: Verified
Dependencies: IP-07
Current packet: Request-local set checks snapshot, reroll/banish policy, shared Book controls и UI Banish mode/cancel/revision reset.
Remaining gates: Нет для fixture framework. G-02 закрыт approved DECISION-0022; G-03 — DECISION-0020. Production-база 1/1 принята DECISION-0094; global set chance provider поставлен IP-11; production значение остаётся balance-data.
Remaining acceptance / IDs: Нет для обязательного scope IP-10.
Target implementation evidence: Snapshot всех checks, ordinal ID, сохранение при banish, mode/cancel/control hints — [IP-10 evidence](evidence/design-sync-R2-2026-09-21-ip10.md#ip-10).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 374/374, PlayMode 2/2 passed, 0 skipped. Условия, coverage и XML/log paths — по ссылке выше.
Documentation impact: Approved DECISION-0022, GDD/UI, DESIGN_SYNC/proposal, IP-07/IP-10/IP-10A/IP-11/IP-19/IP-28 и readiness consumers синхронизированы. Production balance не изменён.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-10).

### IP-10A — UI Foundation, reusable cards, HUD и test harness

Status: Verified
Scope revision: design-sync-R2 + ui-layout-R2. Прежний foundation scope Verified; новая visual delta отдельно.
Dependencies: IP-01, IP-03, IP-06, IP-07, IP-10
Remaining gates: Product contract выбран по DECISION-0086; G-01/G-03 foundation закрыты. Свежие prerequisite checks перед переносом: 301/301 EditMode PASS. Recipe/character semantics остаются у IP-11/IP-12.
Remaining acceptance / IDs: Нет для UI layout R2. Пользователь явно принял игровой интерфейс 2026-09-28; точные условия его ручного просмотра не домысливаются. Автоматическая geometry/input/anchor/retry regression и scoped OBS-08 проверки указаны ниже.
Target implementation evidence: [UI layout R2 runtime](evidence/2026-09-28-ui-layout-r2-runtime.md): HP-anchor, DEV-only speed, HUD density, inspect/confirm, reachable recipe inspector, один общий Pause scroll, compact acquired/missed с popup, увеличенный персонаж, baseline speed и существующие Settings/Quit в фиксированном footer.
Target verification evidence: Unity production scene captures в 720p/1080p; synthetic 10/20 recipe stress отдельно от production. Полный graphics smoke и browser matrix 38 captures — [runtime evidence](evidence/2026-09-28-ui-layout-r2-runtime.md). Ручная приёмка закрыта прямым подтверждением пользователя, а не HTML/tests.
Latest review delta: [OBS-01…07](../playtests/2026-09-28_ui-card-layout.md) — увеличенный header, recipe icons, owned/threshold semantics, queue copy и whole-percent/speed formatter. Scoped PASS 83/83 + 4/4, затем full PASS 887/887 + 34/34; fresh captures 720p/1080p просмотрены агентом. Пользовательская приёмка получена 2026-09-28.
Follow-up OBS-08: zero-owned достижимые рецепты больше не скрываются; acquired/missed отдельно. Новый scoped PASS 84/84 + 4/4 и просмотренные synthetic captures 720p/1080p — [evidence](evidence/2026-09-28-ui-layout-r2-runtime.md#неначатые-рецепты-на-паузе). UI/UX §10/23, DECISION-0086, IP-10A/IP-11 и regression-map синхронизированы; gameplay gates не менялись.
Prior foundation evidence: Reusable cards/projections/HUD/notifications — [IP-10A](evidence/design-sync-R2-2026-09-21-ip10a.md#ip-10a); 2026-09-21 Unity 6000.6.0f1: 383/383 EditMode + 3/3 PlayMode, 0 skipped. Это evidence прежнего принятого scope, не новой visual delta.
2026-09-24 HUD speed extension по прямому запросу пользователя: 1×/2×/3×/5× через RunModel/RunController и UI presenter, выбор сохраняется через паузу, `Time.timeScale` сбрасывается при завершении/выходе. [DECISION-0140](../decisions/0140-run-speed-controls.md); Unity 6000.6.0f1: 711/711 Game.* EditMode, 27/27 PlayMode, 0 skipped, geometry 1920×1080/1280×720; [summary](../../TestResults/checks/20260924T180701-450922Z/summary.json). Визуальная проверка подтверждена пользователем 2026-09-24: «всё хорошо, проверено».
Documentation impact: 2026-09-28 UI/UX, GDD speed classification, DECISION-0140/0081/0083, IP-10A/26/27 и art guidance синхронизированы. Прежняя player-HUD трактовка speed заменена DEV-only; gameplay balance не менялся. Foundation consumers сохраняют проверенные API, новая visual acceptance требуется только для изменённого scope.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-10a).

### IP-31 — Локальная телеметрия ручных прогонов

Status: Verified
Dependencies: IP-01, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-10, IP-10A
Current packet: Bounded local recorder, immutable JSON/summary/feedback export, provenance/capabilities, Playtest UI, feature-owned producers и ordered composition teardown.
Remaining gates: Нет product gates для реализации; отсутствующие boss/Traveler/meta/set-effect/character-detail adapters явно unsupported.
Remaining acceptance / IDs: Нет для telemetry scope. Реальный marker и companion feedback связаны; точное expected поведение по наблюдению «опыт стреляет» не уточнено, причина требует отдельной диагностики.
Target implementation evidence: [IP-31 evidence](evidence/design-sync-R2-2026-09-21-ip31.md#ip-31), [schema/metric dictionary](PLAYTEST_REPORT.md).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 407/407 Game.* EditMode, 4/4 PlayMode, 0 skipped; snapshots 1920×1080/1280×720. Ручной aborted run 108ff5b3e8ed457e84704dcbfa25e0f8: linked feedback/marker, pause, hashes, counters и final export проверены; [manual evidence](evidence/design-sync-R2-2026-09-21-ip31.md#manual-run-2026-09-21).
Documentation impact: Schema/retention/capabilities, BALANCE_WORKFLOW, IP-01/IP-04/IP-06/IP-07/IP-10A/IP-31 contracts, regression-map и readiness. DECISION-0023 Proposed: technical ownership/teardown; GDD/CD и баланс не менялись.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-31).

### IP-32 — Ручные прогоны и AI-assisted balance review

Status: Verified
Dependencies: IP-31
Current packet: Workflow verified; latest applied cycle — плейтест 091d834e и DECISION-0079.
Remaining gates: Нет для workflow scope; BG-01 и explicit approval сохраняются для будущего применения конкретных чисел/механик.
Remaining acceptance / IDs: Нет для workflow scope. Для latest cycle OBS-01 performance и ручная перепроверка OBS-02/03 остаются открытыми; реализация workflow от этого не становится незавершённой.
Target implementation evidence: [IP-32 evidence](evidence/design-sync-R2-2026-09-21-ip32.md#ip-32), [latest review](../playtests/2026-09-27_091d834e.md), [latest change evidence](evidence/2026-09-27-sky-strike-radius-and-xp-curve.md), [checklist](../playtests/CHECKLIST.md).
Target verification evidence: latest cycle — source report сохранён byte-identical, 23/23 config snapshots verified; generation/static validators PASS; Unity 6000.6.0f1 full PASS 870/870 EditMode + 30/30 PlayMode, 0 skipped. Ручной повторный прогон открыт.
Documentation impact: baseline/CD, DECISION-0079, playtest review/OBS, IP-06/IP-17/IP-32 и generated production catalogs синхронизированы; GDD не менялся.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-32).

### IP-11 — Set recipes, priority draft policy и effect families

Status: Verified
Dependencies: IP-07, IP-08, IP-09, IP-10, IP-10A
Current packet: Global chance/order/backfill, 3–6-component recipes, six reusable effect families in four real JSON fixtures, source/non-recursion, keyed cleanup, recipe projection/acquisition feedback and DEV counters.
Remaining gates: Нет для fixture framework. G-04/G-05/G-13, exact production payloads/thresholds/art остаются у IP-19; real potion event binding поставлен IP-28; production SET-001…020 не зарегистрированы.
Remaining acceptance / IDs: Нет для обязательного framework scope. Per-ID production correctness и manual art review не заявлены.
Target implementation evidence: [IP-11 evidence](evidence/design-sync-R2-2026-09-21-ip11.md#ip-11), [schema/compatibility matrix](modules/IP-11-set-framework.md#реализованный-framework-contract).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 426/426 Game.* EditMode, 5/5 PlayMode, 0 skipped. Four simultaneous sets via queued choices и deterministic producer-first teardown проверены; XML/log paths — в evidence.
Documentation impact: IP-03/IP-08/IP-10/IP-11/IP-19/IP-28 contracts, regression guard и readiness; DECISION-0025 Proposed для source/ownership architecture review. GDD/CD и production balance без изменений; OBS-01 открыт.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-11).

### IP-12 — Character definitions, weighted draft и selection presentation

Status: Verified
Dependencies: IP-07, IP-08, IP-09, IP-10A
Current packet: Fixture character framework с отдельным baseline, ordered highlights и pre-run selection по approved DECISION-0026.
Remaining gates: Нет для framework packet. Production G-14 остаётся IP-22; G-15 resolved по DECISION-0037, profile поставляет IP-25; G-17/G-18 и image approval — IP-12A/IP-22. Fixture numbers/placeholders не являются production balance/art.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-12 evidence](evidence/design-sync-R2-2026-09-21-ip12.md#ip-12), [schema/API](modules/IP-12-character-framework.md#framework-api-и-fixture-schema).
Target verification evidence: 2026-09-21 — Unity 6000.6.0f1, **439/439 Game.* EditMode, 6/6 PlayMode, 0 skipped**. Реальный selection→Sturdy loadout→Shutdown→Agile, locked rejection, baseline independence/highlights, weights и telemetry; [details](evidence/design-sync-R2-2026-09-21-ip12.md#coverage-and-verification).
Documentation impact: IP-12/IP-22/IP-25/IP-26 contracts, DECISION-0027 technical record (Proposed), regression guard и consumer readiness. GDD/CD production values без изменений.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-12).

### IP-12A — Visual Presentation Foundation и asset production pipeline

Status: Implemented
Dependencies: IP-00, IP-02, IP-03, IP-04, IP-05, IP-08, IP-12
Current packet: Category import/role validation, provenance/inventory reconciliation, generic presentation adapters и synthetic fixture kit.
Slow ice follow-up 2026-09-30: пользователь утвердил общий raster; mask import и runtime bar+ice подключены по [DECISION-0108](../decisions/0108-slow-status-look-preview.md). Art 63/63 + 270 records, финальные graphics EditMode 432/432 и PlayMode 59/59 PASS; игровой visual review открыт ([evidence](evidence/2026-09-30-slow-ice-runtime.md)).
После игрового отзыва: v001 заменена первой показанной текстурой v002; оверлей проверен на уменьшенном body sprite, полоска перенесена ниже его границы. Art 64/64 + 270 records, full EditMode 1089/1089 и slow PlayMode 1/1 PASS; общий graphics PlayMode 58/59, единственный FAIL в `MetaShopSmokeTests` (70 против 61). Gameplay-scale review v002 остаётся открытым ([evidence](evidence/2026-09-30-slow-ice-runtime.md)).
Remaining gates: G-17 concept mapping и G-18 закрыты DECISION-0029; per-image/replacement gates сохраняются для новых assets. Пользователь принял Presentation Fixture Review; остаётся gameplay density часть gate E.
Remaining acceptance / IDs: Реальный gameplay density review с 3–4 сетами. UI body reuse и idle/flip/hit/proc/death/collect/pause/reset в Presentation Fixture Review приняты пользователем 2026-09-21 («всё хорошо»). Четыре synthetic copies не являются этим прогоном; production enemy/pickup/VFX art не заявлен.
Target implementation evidence: [IP-12A evidence](evidence/design-sync-R2-2026-09-21-ip12a.md#ip-12a), [pipeline/API](../art/ASSET_PIPELINE.md#21-category-profiles-и-reusable-adapters-ip-12a), [manifest](../../Art/asset-manifest.json).
Workflow tooling: пакетная подготовка approved art, numeric-only preview и безопасный scoped runner; [DECISION-0049](../decisions/0049-art-workflow-automation-and-preview.md), [tooling evidence](evidence/2026-09-22-workflow-tools.md). Density/per-image gates сохраняются.
Art follow-up: approved ENEMY-001 body подключён к FIXTURE-ENEMY-SEEKER; импорт, отдельный child motion и pool reset проверены. 641/641 EditMode, 23/23 PlayMode; [evidence](evidence/2026-09-21-enemy001-art.md). Текущий gameplay-визуал принят пользователем 2026-09-22; документированный density-прогон с 3–4 сетами остаётся отдельной проверкой.
Contact follow-up (IP-02/IP-04/IP-12A): по поручению пользователя от 2026-09-22 выполнен опыт с меньшими кругами внутри двух текущих body; [DECISION-0039](../decisions/0039-conservative-body-contact-circles.md), [evidence](evidence/2026-09-22-body-contact-circles.md). Повторный пользовательский плейтест ощущения открыт; этот опыт не закрывает production/density gates и не начинает следующий IP.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **481/481 Game.* EditMode, 8/8 PlayMode, 0 skipped**. Body/UI/VFX import/reimport, role/resource negatives, alpha border, child-root invariance, pool/disable/reinitialize и preferences; manifest audit 9 records. Diagnostic capture 1920×1080 просмотрен; пользователь отдельно принял интерактивный стенд («всё хорошо»). Это не подтверждает плотный gameplay с 3–4 сетами. [Details](evidence/design-sync-R2-2026-09-21-ip12a.md#checks).
Documentation impact: Approved DECISION-0029, Proposed technical DECISION-0030, GDD/Art Direction, pipeline/inventory/provenance/manifest, IP-12A/IP-14/IP-22/IP-26 и readiness. W-01 runtime burst поставлен отдельным IP-14; его spawn-only checks не закрывают gameplay density review.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-12a).

### IP-13 — Enemy movement/attack patterns и control integration

Status: Implemented
Dependencies: IP-03, IP-04, IP-05
Current packet: Пользовательская дельта 2026-09-29 — пять лёгких anti-blob movement kinds, per-spawn weighted movement variants и ENEMY-001 rollout; прежний fixture movement/attack/control scope сохраняется.
Movement overlay follow-up 2026-09-30 — Implemented для пробной разбивки blob: выбранные ordinary жизни на короткое время получают fan waypoint без замены базового movement kind; pause, dash priority и pool reuse сохранены. Enemy EditMode 231/231 PASS; игровой просмотр открыт. [DECISION-0130](../decisions/0130-periodic-blob-breakup-trial.md), [evidence](evidence/2026-09-30-periodic-blob-breakup.md).
Remaining gates: Нет для fixture framework. G-07 закрыт DECISION-0017. G-14 остаётся для production cards; новые wind-up/control values — synthetic fixtures.
Remaining acceptance / IDs: пользователь сравнил двухтипную смесь с прежней шеститипной и выбрал прежнюю как более интересную в игре. ENEMY-001 вновь использует 75% усиленный BlockedSidestep и по 5% пяти других шаблонов. Автоматический перебор остаётся экспериментом; production catalog после возврата 8/8 EditMode PASS. Прежний fixture framework packet закрыт.
Target implementation evidence: [Fixed-75 sweep и rollout](evidence/2026-09-30-anti-blob-fixed75-sweep.md), [Anti-blob delta](evidence/2026-09-29-anti-blob-enemy-movement.md), [IP-13 evidence](evidence/design-sync-R2-2026-09-21-ip13.md#ip-13), [schema/compatibility matrix](modules/IP-13-enemy-patterns.md#schema-и-runtime-contract).
Target verification evidence: шеститипная настройка возвращена по игровому отзыву 2026-09-30; production catalog **8/8** EditMode PASS (`TestResults/checks/20260930T180212-400941Z/summary.json`), generation `UP TO DATE`. Итоговый fixed-75 sweep **1/1** относится к двухтипной пробе; её пользователь отклонил по ощущению игры. [Эксперимент и возврат](evidence/2026-09-30-anti-blob-fixed75-sweep.md). Настройка 2026-09-29 — Enemy EditMode **207/207**; PlayMode прежней итерации дважды завершился в Unity RenderPipelineManager до result XML. [История](evidence/2026-09-29-anti-blob-enemy-movement.md#automated-checks).
Documentation impact: GDD/CD, IP-13/IP-20, DECISION-0099, authoring baseline, generated catalog и regression map синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-13).

### IP-14 — Wave Director: continuous и burst timeline

Status: Verified
Dependencies: IP-04, IP-13
Current packet: timeline-level technical cap 300 для continuous/burst, skipped-window expiry, seeded composition/geometry, deterministic hooks, actual spawn outcomes и existing HUD/DEV projection (DECISION-0076/0115).
Anti-blob trial 2026-09-30 — Implemented по [DECISION-0130](../decisions/0130-periodic-blob-breakup-trial.md): phase-level `blobBreakup` включён во всех текущих production-фазах FIELD-001…004 для всех ordinary типов. Раз в 10 s обрабатываются плотные непересекающиеся квадраты 3.5×3.5 wu на сетке 7×7 (охват 24.5×24.5 wu), кроме центрального квадрата с игроком; порог 11 ordinary врагов, выбор 70 %, cap 60 на квадрат, манёвр до 6 s. Сторона веера определяется положением врага относительно линии от группы к игроку. `enemyIds` ограничивает только получателей манёвра, но все ordinary типы учитываются в плотности; выключение на фазе поддержано. Targeted Enemy EditMode 26/26 PASS относится к предыдущей настройке (`TestResults/checks/20260930T194540-114798Z/summary.json`); тест новой настройки NOT RUN: открытый Editor не отвечает через UnitySkills REST. [Evidence](evidence/2026-09-30-periodic-blob-breakup.md).
Текущая дельта: общий cap 300 для всех production-полей по [DECISION-0115](../decisions/0115-shared-300-enemy-cap.md); код читает значение из timeline, генератор отклоняет расхождение пакетов. [Evidence](evidence/2026-09-30-shared-300-enemy-cap.md).
Follow-up 2026-09-30 — Implemented: cap replacement по [DECISION-0105](../decisions/0105-continuous-cap-replacement.md) удаляет самого дальнего ordinary без событий, kills и дропа; расписания и cap 200 не менялись. Затронутые EditMode 44/44; full graphics 1051/1051 + 57/57, generation/audio/art 269 PASS. Ручной плейтест открыт. [Evidence](evidence/2026-09-30-cap-replacement-and-human-draft.md).
Follow-up 2026-09-29 — Implemented, ожидает игрового просмотра: по [DECISION-0100](../decisions/0100-opposite-centroid-spawn.md) production FIELD-001/002/003 используют `spawnOppositeBias=1.0` (повышено с 0.8 через 0.9) и обычный `spawnRadius=10` вместо 12; первые 20 секунд спавн остаётся у края экрана. На tick спавна оцениваются не более 16 живых обычных врагов, выбор угла охватывает всё кольцо. Исходный алгоритм: Enemy/Bootstrap EditMode **247/247 PASS**; тюнинг 1.0: production field EditMode **17/17 PASS**, content STATIC PASS, 0 failed/skipped. [Тюнинг/evidence](evidence/2026-09-29-opposite-spawn-bias-tuning.md), [исходный алгоритм](evidence/2026-09-29-opposite-centroid-spawn.md), [OBS-09](../playtests/2026-09-29_anti-blob-movement.md#obs-09--проба-противоположного-спавна-и-более-близкого-кольца).
Follow-up 2026-09-29 — Implemented, ожидает игрового просмотра: по [DECISION-0103](../decisions/0103-field001-opening-spawn-rate.md) только FIELD-001 получает 0.6 обычной continuous частоты в первые 30 s; generated content STATIC PASS, Enemy/Bootstrap EditMode **248/248 PASS**, GameplaySmokeTests PlayMode **3/3 PASS**. [Evidence](evidence/2026-09-29-perimeter-and-opening-rate.md), [OBS-01](../playtests/2026-09-29_field001-opening-rate.md#obs-01--снизить-спавн-в-первые-30-секунд).
Remaining gates: Нет для synthetic framework. W-01 обновлён по DECISION-0076; G-11/G-14 production schedules/Traveler timing остаются у catalog packets. IP-12A density review отдельно.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [DECISION-0076 follow-up](evidence/2026-09-27-wave-cap-and-field001-rhythm.md), [runtime/schema](modules/IP-14-wave-director.md#runtime-и-fixture-schema).
Target verification evidence: 2026-09-27, Unity 6000.6.0f1: **870/870 Game.* EditMode, 30/30 PlayMode, 0 skipped**; targeted wave/bootstrap/UI EditMode **293/293**. [Details](evidence/2026-09-27-wave-cap-and-field001-rhythm.md#проверки).
Documentation impact: GDD/CD, DECISION-0014/0029/0045/0076, IP-14/IP-24, balance packets и STATUS синхронизированы. Production FPS guarantee не заявляется; manual FIELD-001 review открыт.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-14).

### IP-15 — Boss/mid-boss encounter framework

Status: Verified
Dependencies: IP-01, IP-05, IP-08, IP-13, IP-14, IP-10A
Current packet: Synthetic final/mid encounters, ordered attack sequences и HP thresholds, one-shot uncapped hooks, lifecycle/source events и final HUD через producer Changed.
Remaining gates: Нет для synthetic framework. G-07 закрыт DECISION-0017; G-14 production attack payload/rewards/timings/assets остаются IP-21/IP-24.
Remaining acceptance / IDs: none for fixture framework; BOSS-/MIDBOSS- production IDs не поставлялись.
Target implementation evidence: [IP-15 evidence](evidence/design-sync-R2-2026-09-21-ip15.md#ip-15), [schema/runtime](modules/IP-15-boss-framework.md#fixture-schema-и-phase-contract).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **516/516 Game.* EditMode, 10/10 PlayMode, 0 skipped**. Все 13 fixture skills повреждают boss; real scene bar/telegraph/pause/terminal cleanup. [Conditions/results](evidence/design-sync-R2-2026-09-21-ip15.md#checks).
Documentation impact: IP-15 schema/ownership/missing-rule list, IP-16/IP-21 bindings, DECISION-0031 Proposed technical record, regression guards и readiness. GDD/CD/art без изменений; IP-12A density review отдельно.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-15).

### IP-16 — Field definitions, selection и run configuration

Status: Verified
Dependencies: IP-02, IP-12, IP-14, IP-15, IP-10A
Current packet: Два synthetic поля, typed refs/validation, profile access, Character→Field→Run/Back, immutable run identity и telemetry, fresh director/encounters/spawn reset.
Remaining gates: Нет для fixture framework. G-14 production geometry/schedules относятся к IP-23; G-20 resolved по DECISION-0038; G-15 resolved по DECISION-0037, profile поставляет IP-25; placeholder thumbnail без image approval.
Remaining acceptance / IDs: none for fixture framework; FIELD-001…010 не поставлялись.
Target implementation evidence: [IP-16 evidence](evidence/design-sync-R2-2026-09-21-ip16.md#ip-16), [schema/API](modules/IP-16-field-framework.md#framework-api-и-fixture-schema).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1 — **538/538 Game.* EditMode, 12/12 PlayMode, 0 skipped**. Selection/locked/Back, typed refs/hooks, two-field reinit, release-safe snapshot, actual telemetry, player-only geometry и cancellation; [checks](evidence/design-sync-R2-2026-09-21-ip16.md#checks).
Documentation impact: IP-16/IP-23/IP-24/IP-25/IP-26/IP-29 contracts, Proposed DECISION-0032, G-20 и consumer readiness. GDD/CD/art без изменений.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-16).

### IP-28 — World pickup framework: зелье лечения и Book

Status: Verified
Dependencies: IP-05, IP-06, IP-07, IP-09, IP-10, IP-11, IP-12A
Current packet: Пользователь разрешил реализацию запросом «работаем дальше». Synthetic potion/Book framework по approved DECISION-0033 завершён; разрешение ограничено IP-28.
Remaining gates: Нет для fixture framework. Production числа и Book ID/card/art остаются у IP-20/IP-30; IP-12A density review отдельно.
Remaining acceptance / IDs: none для fixture framework; production IDs не зарегистрированы.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21-ip28.md): pooled lifecycle, reachable death drops, Health/Book/set rewards, UI/telemetry.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 571/571 Game.* EditMode, 14/14 PlayMode, 0 failed, 0 skipped; [coverage/results](evidence/design-sync-R2-2026-09-21-ip28.md#checks).
Documentation impact: Approved DECISION-0033/GDD/CD/DESIGN_SYNC; Proposed technical DECISION-0034, IP-28 schema/ownership, consumer contracts/readiness синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-28).

### IP-29 — Traveler encounter framework

Status: Verified
Dependencies: IP-08, IP-13, IP-15, IP-16, IP-28
Current packet: По запросу «реализуй» выполнен synthetic encounter framework DECISION-0035: восемь fixtures/три роли, field schedules, support, Book, UI/telemetry. Дальнейшие IP автоматически не начинать.
Remaining gates: Нет для fixture framework. G-14 production presence/XP/support/attack values, field pools и art остаются IP-24/IP-30. G-20 UI difficulty не используется как scaling rank; IP-12A density review отдельно.
Remaining acceptance / IDs: none для fixture framework; production TRAVELER-001…010 не зарегистрированы.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21-ip29.md): runtime/schedules/placement, Enemy protection/target integration, HUD/dev/telemetry.
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 598/598 Game.* EditMode, 15/15 PlayMode, 0 failed, 0 skipped; [coverage/results](evidence/design-sync-R2-2026-09-21-ip29.md#verification).
Documentation impact: Approved DECISION-0035/GDD/CD/DESIGN_SYNC; Proposed technical DECISION-0036; IP-13/IP-16/IP-24/IP-26/IP-27/IP-29/IP-30, regression-map и consumer readiness синхронизированы.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-29).

### IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — новый production profile 10/10/5 и DECISION-0050 unlock metadata; terminal integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-03, IP-12, IP-16, IP-10A
Current packet: Meta R1 — персональные улучшения и возврат по DECISION-0091. F1-03/F1-08/F1-09 приняты и заново не открываются.
Remaining gates: ручная приёмка Meta R1. Автоматические проверки пройдены; миграция отменена пользователем.
Remaining acceptance / IDs: пользовательская приёмка игрового экрана персональной прокачки. F1-03/F1-08/F1-09 остаются принятыми; исторические проверки — в startup queue и evidence.
Prior implementation evidence (design-sync-R2): [IP-25 evidence](evidence/design-sync-R2-2026-09-21-ip25.md#implementation), [runtime/schema](modules/IP-25-meta-progression.md#runtime-api--schema--reset).
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1: **624/624 Game.* EditMode, 18/18 PlayMode, 0 skipped**; [coverage/results](evidence/design-sync-R2-2026-09-21-ip25.md#checks).
Documentation impact: IP-25 API/schema/save/reset и IP-26 consumers, regression map; GDD/CD правила DECISION-0037 сохранены. Fixture Book=50, новые raster assets не создавались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-25).

Target implementation evidence: [Meta R1 runtime](evidence/2026-09-29-ui-meta-r1-runtime.md); принятые прежние UI/startup packets сохраняются.
Target verification evidence: full graphics 944/944 EditMode + 37/37 PlayMode PASS; последующая правка галочки — targeted PlayMode 1/1 PASS. Результаты, пути, screenshots и ограничения в Meta R1 evidence; ручной приёмки нового экрана нет.

Reward delta DECISION-0098: стартовый L1 больше не оплачивается, новые receipts
получают `5 × (L−1)` при неизменной Book-награде; старые receipts сохранены.
Full graphics 969/969 EditMode + 39/39 PlayMode PASS; [evidence](evidence/2026-09-29-earned-level-reward.md).

### IP-26 — Functional UI и полный player flow

Status: In progress
Scope revision: design-sync-R2 + field-001-start-R1 + ui-layout-R2 + ui-entry-R1.
Startup packet: F1-03 — startup/locks/recipe UI; Results и actual-content integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-10A, IP-11, IP-12, IP-15, IP-16, IP-25, IP-28, IP-29, IP-12A
Current packet: Meta R1 и «Открытия». Settings R1 по DECISION-0093 перенесён в Unity, автоматически проверен и принят пользователем 2026-09-29 («там всё принимается»).
Results reward delta DECISION-0098 отображает сохранённую награду только за уровни
после L1; новые Pause material surfaces подключены и проверены автоматически,
ручной visual review усиленной фактуры остаётся открытым.
Pause shortcut focus fix 2026-09-29: DEV/HUD-фокус больше не подавляет `Space`;
новый PlayMode regression 1/1 PASS, игровой просмотр ожидается
([evidence](evidence/2026-09-29-space-pause-focus.md)).
Delta 2026-09-30 [DECISION-0107](../decisions/0107-hostile-damage-notifications-midboss-coins.md) — Implemented:
коэффициент урона врагов вынесен из защиты героя (IP-03), полоска мини-босса (IP-15/21), единый канал
уведомлений, валюта «монеты» (IP-25/26). Full graphics 1071/1071 + 58/58 PASS; визуальная приёмка
плашки и полоски ожидается ([evidence](evidence/2026-09-30-decision-0107.md)).
Затем счётчик сета на паузе переведён на взятые компоненты, как в драфте: UI 114/114 + 4/4 PASS.
DEV-превью эффекта замедления (4 варианта + «Все») и скорость 0.5× по
[DECISION-0108](../decisions/0108-slow-status-look-preview.md) — Implemented; full graphics 1081/1081 + 59/59
PASS; на момент превью выбор варианта ожидал пользователя ([evidence](evidence/2026-09-30-slow-status-preview.md)).
Пользователь выбрал «Полоска» + «Лёд», затем по игровому отзыву заменил raster на первый показанный вариант v002 2026-09-30. Production binding выполнен, итоговый gameplay-scale gate открыт ([evidence](evidence/2026-09-30-slow-ice-runtime.md)).
[DECISION-0112](../decisions/0112-draft-one-click-selection.md) — выбор Draft/Book
одним нажатием, справка сетов по hover/focus с сохранением последней; Implemented.
Первый EditMode 1085/1085 PASS и две неудачные PlayMode попытки зафиксированы в
[evidence](evidence/2026-09-30-boss-distance-and-draft-selection.md). Последующий full graphics
1085/1085 + 59/59 PASS (`TestResults/checks/20260930T091916-526918Z`);
ручной просмотр выбора в игре открыт.
Указатель на Путников по [DECISION-0109](../decisions/0109-traveler-offscreen-pointer.md) и скрытие секунд
перезарядки в описаниях умений — Implemented; full graphics 1083/1083 + 59/59 PASS
(`TestResults/checks/20260930T064602-849800Z`); визуальная приёмка ожидается.
Затем: исправлены цвет/разворот обводки и видимость полоски замедления (материалы вместо свойств
SpriteRenderer); полоски HP без чисел, HP мини-босса над головой
([DECISION-0110](../decisions/0110-health-bars-no-numbers-midboss-overhead.md)) — Implemented, Unity-проверка ожидается.
Proposal verification: [HTML evidence](evidence/2026-09-28-ui-entry-r1-mockups.md) — 34 captures с A/B/C/D/E, 720p/1080p: geometry/input/lock/scroll/motion/reduced-motion, силуэты, десять полей без scroll, E alpha/pointer/layers/light PASS. Не новая Unity verification и не approval арт-кандидатов.
Latest menu approval: [выбранная пара SHA256](proposals/ui-entry-r1/menu-shepotka-review.md#visual-approval--2026-09-28) — backplate v001 + Shepotka foreground v002. Взрослый образ и свитки только для иллюстрации; canonical CHAR-003 не меняется. Средняя пыль перед обоими героями и усиленное движение лучей приняты. Предыдущий арт сохранён; выбранные слои подключены в Unity через approved packet.
Damage presentation: [DECISION-0085](../decisions/0085-ui-damage-percent-presentation.md) реализована в runtime: базовый урон скрыт, прибавки в процентах; numeric regression включена в новые checks.
Remaining gates: незафиксированные ручные отзывы Meta. Новые production characters/fields не входят в UI packet.
Remaining acceptance / IDs: макет персональных улучшений принят 2026-09-29. Осталась Unity-приёмка Meta. Миграция глобальных покупок отменена пользователем. Results R1, Entry R1, UI layout R2 и F1-09 остаются принятыми.
Prior implementation evidence (design-sync-R2): Main Menu/full navigation, settings persistence/video rollback/audio routing/shake, notifications, result sets/special kills и permanent modifier display; [IP-26 evidence](evidence/design-sync-R2-2026-09-21-ip26.md#ip-26).
Documentation impact: 2026-09-28 стиль «Полевой фолиант» подтверждён, первый проход — стилевой прототип: fixture UI 65/65 + PlayMode 1/1 не доказывают production layout. DECISION-0086 уточняет UI/UX и IP-10A/26/27: длинное описание заменено recipe inspector, клик не подтверждает, missed компактны, скорость от общего baseline. [Первый проход](evidence/2026-09-28-field-folio-ui-vertical-slice.md), [layout R2](proposals/2026-09-28-ui-layout-r2.md).
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1, **637/637 Game.* EditMode, 22/22 PlayMode, 0 skipped**, Windows release build exit 0. Interactive menu/settings/contrast checked at native 2560×1440; Пользователь сообщил «всё в порядке», кроме недоступного Retry после поражения; [OBS-01](../playtests/2026-09-21_defeat-ui.md#obs-01--после-поражения-нельзя-перезапустить-забег) воспроизведён и исправлен с failing-before/passing-after regression. После отчёта об исправлении пользователь явно поручил «ставь верифайд и комить»: оставшиеся manual acceptance gates закрыты его приёмкой. Новые измерения 1920×1080 или повторный ручной прогон не заявляются; см. evidence/DECISION-0038.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-26).

Target implementation evidence: [Meta R1 runtime](evidence/2026-09-29-ui-meta-r1-runtime.md); принятые прежние UI/startup packets сохраняются.
Target verification evidence: full graphics 944/944 EditMode + 37/37 PlayMode PASS; последующая правка галочки — targeted PlayMode 1/1 PASS. Результаты, пути, screenshots и ограничения в Meta R1 evidence; ручной приёмки нового экрана нет.

### IP-17 — Production Active Skills SKILL-001…016

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-01 — SKILL-001…007/010/013/014. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-08, IP-10A, IP-12A
Blocked by: нет для реализации; данные, поведение и world art всех 16 ID подключены.
Remaining gates: ручная visual acceptance на реальной скорости; G-08/G-09 закрыты DECISION-0017, параметры 16 skills Approved по DECISION-0053/0060.
Remaining acceptance / IDs: ручная проверка читаемости луча SKILL-012, мины и снарядов SKILL-009/011/015/016 на реальной скорости.
Late IDs 2026-09-26: SKILL-008/009/011/012/015/016 Implemented (production JSON, per-ID тесты L1…L6), луч SKILL-012 — процедурный; Unity full PASS 766/766 + 27/27 — [evidence](evidence/2026-09-26-late-skills-passives.md).
Data packet 2026-09-26: [late-skills-passives-v1](../balance/late-skills-passives-v1.md) для SKILL-008/009/011/012/015/016 — Approved 2026-09-26 ([DECISION-0060](../decisions/0060-late-skills-passives-data-v1.md)); static validator PASS. SKILL-012 использует процедурный луч; world art остальных четырёх подключён ниже.
World art 2026-09-26: пользователь утвердил SKILL-009/011/015/016; immutable masters, provenance, runtime PNG и typed references подключены. Unity full PASS 769/769 EditMode + 27/27 PlayMode, manifest 108/108; gameplay-scale review остаётся открытым. [Evidence](evidence/2026-09-26-late-skill-world-art.md).
Startup subset F1-01: SKILL-001…007/010/013/014 Implemented 2026-09-24 — [evidence](evidence/field001-f1-01-2026-09-24.md).
Balance follow-up 2026-09-27: SKILL-002/013 получили постепенный projectile-count growth с прежними финальными caps; production data и all-level assertions синхронизированы, Unity full PASS 870/870 + 30/30. [Evidence](evidence/2026-09-27-early-projectile-growth-and-enemy001-speed.md).
Catalog-wide follow-up 2026-09-27: SKILL-001…016 получили общий L1→L3 ramp без изменения L4–L6; all-level primary curves проверены targeted 16/16 и full 870/870 + 30/30. [Evidence](evidence/2026-09-27-active-skill-early-progression.md).
SKILL-010 follow-up 2026-09-27: radius curve уменьшена до `0.8/1.3/1.8/1.8/1.8/1.8`, L6 third strike `×1.35`; production data и assertions синхронизированы, Unity full PASS 870/870 + 30/30. [Evidence](evidence/2026-09-27-sky-strike-radius-and-xp-curve.md).
Target implementation evidence: F1-01 subset и поздние SKILL-008/009/011/012/015/016; см. evidence выше.
Target verification evidence: автоматические проверки PASS 2026-09-26; ручная visual acceptance не проведена.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-17).

### IP-18 — Production Passive Items PASSIVE-001…014

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-02 — PASSIVE-001…005/007…009/011/012. Required packets: F1-00/01; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-09, IP-10A, IP-12A, IP-28
Blocked by: нет.
Remaining gates: G-08/G-09 закрыты DECISION-0017; G-10 закрыт DECISION-0033/IP-28; полные значения 14 passives остаются; отсутствие конкретного runtime parameter не заполняется hidden default.
Remaining acceptance / IDs: все 14 ID реализованы; осталась ручная проверка читаемости иконок PASSIVE-006/010/013/014 в draft/build slots.
Late IDs 2026-09-26: PASSIVE-006/010/013/014 Implemented, Unity full PASS 766/766 + 27/27 — [evidence](evidence/2026-09-26-late-skills-passives.md).
Data packet 2026-09-26: [late-skills-passives-v1](../balance/late-skills-passives-v1.md) для PASSIVE-006/010/013/014 — значения карточек и каналы, Approved 2026-09-26 ([DECISION-0060](../decisions/0060-late-skills-passives-data-v1.md)); static validator PASS.
Startup subset F1-02: PASSIVE-001…005/007…009/011/012 Implemented 2026-09-24 — [evidence](evidence/field001-f1-02-2026-09-24.md).
Target implementation evidence: F1-02 subset only.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-18).

### IP-19 — Production Sets SET-001…020

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-05 — SET-001/004/006/010/017. Required packets: F1-00/01/02/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-11, IP-17, IP-18, IP-28, IP-12A
Blocked by: нет для реализации; IP-17/IP-18 Implemented, IP-11/IP-28 Verified, IP-12A Implemented.
Remaining gates: G-08 закрыт DECISION-0017, G-02 закрыт DECISION-0022; G-04/G-05/G-13 закрыты DECISION-0061 для sets-v1. Финальная проверка и visual acceptance остаются после реализации.
Remaining acceptance / IDs: все 20 SET ID реализованы; остаются реальный прогон и ручное сочетание 3–4 сетов на реальном масштабе.
Data packet 2026-09-26: [sets-v1](../balance/sets-v1.md) — пороги и числа 15 сетов, решения G-04/G-05 и орбита SET-014; Approved 2026-09-26 ([DECISION-0061](../decisions/0061-sets-data-v1.md), мусор +50%, орбита +35% вращения); static validator PASS. G-04/G-05/G-13 для этих ID закрыты; IP-17 world art подключён 2026-09-26.
World art 2026-09-26: пользователь утвердил отдельные projectile-спрайты SET-008/016/018/019/020; masters, provenance, runtime PNG, typed references и специальные маршруты SET-008/set-attacks подключены. Art PASS 44/44, full PASS 792/792 EditMode + 27/27 PlayMode, manifest 117/117. Gameplay-scale и ручной обзор сочетания сетов остаются открытыми. [Evidence](evidence/2026-09-26-set-world-art.md).
Startup subset F1-05: SET-001/004/006/010/017 Implemented 2026-09-24 — [evidence](evidence/field001-f1-05-2026-09-24.md).
Target implementation evidence: 2026-09-26 — SET-002/003/005/007/008/009/011/012/013/014/015/016/018/019/020 и новый вид `SkillMechanics`; [evidence](evidence/2026-09-26-sets-v1.md).
Target verification evidence: Unity full PASS 2026-09-26, EditMode 784/784 + PlayMode 27/27 (автоматические); ручная проверка сочетаний не выполнена.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-19).

### IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-04 — ENEMY-001…005, ENEMY-007 и PICKUP-001. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-04, IP-13, IP-28, IP-12A
Blocked by: gameplay-scale review новых body и projectile visuals. ENEMY-006 переиспользует ENEMY-005 projectile. Данные и поведение всех 20 врагов реализованы 2026-09-26; per-ID projectile art gate поздних ranged IDs закрыт 2026-09-27.
Remaining gates: G-10 semantics/lifecycle закрыты DECISION-0033/IP-28; G-14 для поздних ID закрыт DECISION-0062; AG-01 сохраняется только для игрового визуального review.
Remaining acceptance / IDs: ENEMY-006/008/009 и ENEMY-010…020 gameplay-scale body review; ENEMY-010/011/012/014/015/018/019 projectile gameplay-scale review; startup body art принят 2026-09-24.
Data packet 2026-09-26: [enemies-v1](../balance/enemies-v1.md) — недостающие параметры 14 врагов, скорость ×1.3 к карточной по образцу FIELD-001, прочие карточные числа без изменений; Approved 2026-09-26 ([DECISION-0062](../decisions/0062-enemies-data-v1.md)); static validator PASS; G-14 для этих ID закрыт.
Late IDs 2026-09-26: ENEMY-006, 008…020 Implemented (production JSON, per-ID тесты); ENEMY-006/008/009 body art — [FIELD-002 evidence](evidence/2026-09-26-field002-enemy-art.md), ENEMY-010…020 body art — [late-art evidence](evidence/2026-09-26-late-enemy-body-art.md). Projectile v001 для ENEMY-010/011/012/014/015/018/019 утверждены и подключены 2026-09-27; полный PASS 857/857 EditMode + 30/30 PlayMode, manifest 188/188. Открыт только ручной gameplay-scale review. [Projectile art evidence](evidence/2026-09-27-enemy-projectile-art.md).
Startup subset F1-04: ENEMY-001…005/007 + PICKUP-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-04-2026-09-24.md).
Balance follow-up 2026-09-27: ENEMY-001 speed 1.20 → 0.96 (−20%); прочие параметры и Seek-поведение сохранены, Unity full PASS 870/870 + 30/30. [Evidence](evidence/2026-09-27-early-projectile-growth-and-enemy001-speed.md).
Anti-blob follow-up 2026-09-29: ENEMY-001 speed 0.96 → 1.056 (+10%); прежняя смесь 75% усиленный BlockedSidestep и по 5% остальных пяти шаблонов. Её исходное сравнение 534 Physics2D-прогонов и Enemy EditMode 207/207 сохранены как история. Две альтернативы зафиксированы в [OBS-07](../playtests/2026-09-29_anti-blob-movement.md#obs-07--две-гипотезы-для-следующего-anti-blob-эксперимента). [Движение](evidence/2026-09-29-anti-blob-enemy-movement.md), [первая проба](evidence/2026-09-29-anti-blob-sweep.md).
Anti-blob follow-up 2026-09-30: при фиксированных 75% BlockedSidestep перебраны 106 смесей. Двухтипная проба 75% BlockedSidestep / 25% InertialPursuit дала на 20 seed плотность 0.850 против 0.897 шеститипной смеси, крупнейшую группу 0.941 против 0.975, но увеличила среднюю дистанцию до игрока. Итоговый sweep 1/1 и production catalog 8/8 EditMode PASS относятся к этой пробе. Пользователь оценил прежнюю шеститипную смесь как более интересную в игре и поручил её вернуть; текущая конфигурация — 75% BlockedSidestep / по 5% остальных пяти шаблонов. После возврата production catalog 8/8 EditMode PASS, generation `UP TO DATE`. [Evidence](evidence/2026-09-30-anti-blob-fixed75-sweep.md).
Balance follow-up 2026-09-30 — Implemented: ENEMY-005 стреляет двумя стрелами вместо трёх на всех полях по [DECISION-0106](../decisions/0106-archer-two-arrow-volley.md). Generated content UP TO DATE, scoped content STATIC PASS, Enemy/Bootstrap EditMode 14/14 PASS; повторный ручной забег на новой сборке открыт. [Evidence](evidence/2026-09-30-archer-two-arrow.md).
ENEMY-007 body contact refit to its approved half-size v002 sprite: radius 0.266696, centerY 0.299833; global contact fit PASS, Unity full smoke 784/784 EditMode и 27/27 PlayMode, zero skipped — [evidence](evidence/2026-09-26-enemy007-contact-refit.md). Остальные gates и статус IP-20 не изменились.
Target implementation evidence: ENEMY-001 v002 принят пользователем; runtime 256×256 импортирован и подключён как body существующего FIXTURE-ENEMY-SEEKER с отдельным motion profile/child rig. Fixture ID, баланс и collider сохранены. Production ENEMY-001 binding не выполнен; G-14 и пользовательский gameplay/density review остаются. [Art integration evidence](evidence/2026-09-21-enemy001-art.md).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 641/641 Game.* EditMode и 23/23 PlayMode, 0 skipped. Import/reimport GUID, registry refs, child-only motion, hit/pause, death/mixed-pool reuse и Gameplay spawner. [Условия и ограничения](evidence/2026-09-21-enemy001-art.md#verification).
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-20).

### IP-21 — Production Final Bosses и Mid-bosses

Status: Blocked
2026-09-30 [DECISION-0111](../decisions/0111-boss-teleport-trigger-distance.md):
дистанция запуска телепорта BOSS-001…010 увеличена 6.25 → 7.1875 units (+15%).
Генерация и 16/16 data validation PASS. После исходного PlayMode crash
последующий full graphics прошёл 1085/1085 EditMode + 59/59 PlayMode;
ручной просмотр дистанции прыжка открыт
([evidence](evidence/2026-09-30-boss-distance-and-draft-selection.md)).
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-06 — BOSS-001 и MIDBOSS-001. Required packets: F1-00/01/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-15, IP-12A
Blocked by: поля FIELD-004…010 и их волны (IP-23/IP-24) ещё не созданы. FIELD-003 уже есть, но полный живой прогон поздних боссов на своих полях пока невозможен. Данные/поведение всех 16 реализованы (bosses-v1), body и projectile art подключены.
Remaining gates: gameplay-scale body/projectile review и живая проверка зон/лучей/призыва на своих полях.
Remaining acceptance / IDs: BOSS-002/MIDBOSS-002 gameplay-scale review; BOSS-003…010, MIDBOSS-003…010 — gameplay-scale review утверждённых тел/снарядов и ручная проверка в забеге; startup body/projectile art принят 2026-09-24. [FIELD-002 art](evidence/2026-09-26-field002-art.md), [late boss body art](evidence/2026-09-27-boss-body-art.md), [projectile art](evidence/2026-09-27-boss-projectile-art.md).
Startup subset F1-06: BOSS-001, MIDBOSS-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-06-2026-09-24.md).
Data packet 2026-09-27: [bosses-v1](../balance/bosses-v1.md) — недостающие параметры BOSS-003…010/MIDBOSS-003…010 (урон как доля контакта, XP, тайминги, телепорт финальных), фирменные атаки каждому боссу на трёх новых семействах (зона, луч, призыв; редакция 2 по просьбе пользователя) и расширения схемы E1…E6; карточные числа без изменений; **Approved 2026-09-27** ([DECISION-0066](../decisions/0066-bosses-data-v1.md)); GDD (правило зон/лучей/призыва) и 16 карточек CD синхронизированы; static validator PASS. Следующая работа IP-21 — реализация F1…F3, E1…E6 и 16 encounters по пакету; автоматически не начинать.
Packet bosses-v1 Implemented 2026-09-27: семейства зона/луч/призыв, расширения E1…E6 и 16 encounters из утверждённой таблицы; Unity 6000.6.0f1 full PASS — **EditMode 843/843, PlayMode 29/29, 0 skipped**, `TestResults/checks/20260927T080300-187440Z/summary.json`; [evidence](evidence/2026-09-27-ip21-bosses-v1.md).
Body art packet 2026-09-27: пользователь утвердил 16 поз после правок разнообразия рук; BOSS-003…010/MIDBOSS-003…010 body v001 импортированы и подключены к production data. [Art evidence](evidence/2026-09-27-boss-body-art.md).
Projectile art packet 2026-09-27: пользователь утвердил девять shared families FIELD-002…010; `BOSS-002…010-VISUAL-PROJECTILE` импортированы, соответствующие boss/midboss attacks переведены с fallback BOSS-001, зоны/лучи остаются procedural. [Art evidence](evidence/2026-09-27-boss-projectile-art.md).
Target implementation evidence: F1-06 (BOSS-001/MIDBOSS-001), F2-02 (BOSS-002/MIDBOSS-002), bosses-v1 (остальные 16) — [encounters](evidence/2026-09-27-ip21-bosses-v1.md), [body art](evidence/2026-09-27-boss-body-art.md), [projectile art](evidence/2026-09-27-boss-projectile-art.md).
Target verification evidence: boss art — Unity full PASS 2026-09-27 (**857/857 EditMode + 30/30 PlayMode**, 0 skipped), `TestResults/checks/20260927T125843-334689Z/summary.json`; art scope 50/50, manifest 176/176, `TestResults/checks/20260927T125557-368084Z/summary.json`; gameplay-scale review и живой прогон на своих полях открыты.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-21).

### IP-22 — Production Characters CHAR-001…010

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — CHAR-001; поздние character IDs только unlock metadata. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-12, IP-17, IP-12A
Blocked by: нет для реализации; данные, веса и visual bindings всех 10 ID подключены.
Remaining gates: G-14 weights закрыт DECISION-0089; G-15 resolved по DECISION-0037, unlock metadata определены; concept/master identity подтверждена DECISION-0029, production runtime binding/art review остаются per-ID. CHAR-006 огр и прочие approved roster choices не переутверждаются.
Remaining acceptance / IDs: ручной прогон CHAR-002…010 (ощущение характеристик и блокировок), gameplay-scale body review и чистовые portrait/icon при недостаточной читаемости crop; CHAR-005/006/007/008/009/010 честно открываются только после FIELD-004…009. Утверждённые body CHAR-002…005 подготовлены как visual assets 2026-09-26 — [evidence](evidence/2026-09-26-character-body-art.md). Body CHAR-006…010 подготовлены единым art packet 2026-09-27: runtime imports и contact profiles зарегистрированы, Unity art scope 44/44 EditMode, manifest 143/143, global contact fit PASS; production binding и gameplay-scale review открыты — [evidence](evidence/2026-09-27-character-body-art.md).
Startup subset F1-03: CHAR-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-03-2026-09-24.md).
Data packet 2026-09-28: [characters-v1](../balance/characters-v1.md) — CHAR-002…010: заметные характеристики (±30…50%) со слабыми сторонами, веса драфта ×1.35/0 для активных и пассивок: по три повышенных и по два заблокированных каждого типа (6–8 недоступных сетов у каждого, разные наборы); веса начинают действовать на пассивки. **Approved 2026-09-28** ([DECISION-0089](../decisions/0089-characters-v1.md)); static validator PASS.
CHAR-002…010 Implemented 2026-09-28: generator из characters-v1, `passiveDraftWeights` в JSON/DTO, `DraftPool` применяет вес персонажа к пассивкам, portrait/icon — body crop как у CHAR-001, motion — общий `CHAR-001-MOTION`; GDD и карточки синхронизированы. Unity 6000.6.0f1: EditMode 911/911 (`TestResults/checks/20260928T153347-522756Z`), PlayMode 34/34 с graphics (`TestResults/checks/20260928T153929-023270Z/summary.json`), 0 skipped. Первый PlayMode без graphics упал в рендере batch-процесса (crash handler) и не считается результатом. В игре не проверено.
Target implementation evidence: F1-03 + characters-v1 / DECISION-0089; перенесён коммит `3d557ea`, [integration evidence](evidence/2026-09-28-worktree-integration.md).
Target verification evidence: свежий полный smoke объединённой версии — 921/921 EditMode + 34/34 PlayMode с graphics, 0 failed/skipped (`TestResults/checks/20260928T184120-318706Z/summary.json`); включает 24 ProductionCharacterCatalogTests. Gameplay-scale/manual gates выше остаются открытыми.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-22).

### IP-23 — Production Fields FIELD-001…010

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 geometry/environment/metadata/thumbnail. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Follow-up 2026-09-29 — Implemented, ожидает игрового просмотра: по [DECISION-0102](../decisions/0102-invisible-field-perimeter.md) сплошной видимый забор по периметру FIELD-001/002/003 убран; player-only сцена-коллайдеры и внутренние препятствия сохранены. Generated content STATIC PASS; Enemy/Bootstrap EditMode **248/248 PASS**, GameplaySmokeTests PlayMode **3/3 PASS**. [Evidence](evidence/2026-09-29-perimeter-and-opening-rate.md), [OBS-02](../playtests/2026-09-29_camera-field-edge.md#obs-02--убрать-видимые-преграды-с-границ-карт).
Dependencies: IP-16, IP-20, IP-21, IP-12A
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope).
Remaining gates: G-14: geometry/enemy pools; G-20 resolved по DECISION-0038; G-15 resolved по DECISION-0037. Весь approved mapping переносится, numeric schedules отдельно.
Remaining acceptance / IDs: FIELD-002 gameplay-scale review; FIELD-003 ручной прогон и gameplay-scale review утверждённых руин/воды/миниатюры; FIELD-004 Implemented по DECISION-0129: geometry, environment, 16-phase timeline, BOSS-004/MIDBOSS-004 и Traveler rank 4 прошли targeted EditMode 4/4 и PlayMode 1/1; полный ручной забег и gameplay-scale review открыты ([evidence](evidence/2026-09-30-field004-production-packet.md)). Dev-only поле FIELD-DEV-BLOBS (per-run кляксы, арена 120, спавн FIELD-001) — [DECISION-0132](../decisions/0132-dev-blob-test-field.md), не входит в десять полей. FIELD-005…010 geometry/metadata/environment kits, production bindings и target-scale review подготовленных thumbnails; FIELD-001 thumbnail image и Unity verification. [FIELD-002 art](evidence/2026-09-26-field002-art.md), [FIELD-003 art](evidence/2026-09-27-field003-art-and-projectile-halo.md), [FIELD-004…010 thumbnails](evidence/2026-09-27-field004-010-thumbnails.md).
Startup subset F1-08: FIELD-001 geometry/obstacles/metadata/environment Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
Data packet 2026-09-27: [field003-v1](../balance/field003-v1.md) — FIELD-003 «Пограничные руины»: волны на каркасе FIELD-001 (HP ×1.24, урон ×1.16, частота +20%, стрелки 28%, лимит ≤220), 16 кластеров руин (107 кусков), ENEMY-010 с первой волны (карточка FIELD-003…008 по выбору пользователя); Proposed ([DECISION-0067](../decisions/0067-field003-v1.md)); static validator PASS.
FIELD-003 Implemented 2026-09-27: поле, окружение (107 авторских препятствий, вертикальные стены повёрнуты), FIELD-003-TIMELINE и FIELD-003-TRAVELERS из утверждённого пакета; арт руин, воды и миниатюры — per-ID gates, до них плетень/валун прежних полей; Unity full PASS 2026-09-27 (EditMode 847/847, PlayMode 30/30, `TestResults/checks/20260927T084545-984347Z/summary.json`); [evidence](evidence/2026-09-27-field003.md).
FIELD-003 art packet 2026-09-27: ground, ruined wall, rubble, visual-only water и thumbnail v001 утверждены и подключены; hostile projectile halo приглушён без изменения размера и остаётся под sprite. Art scope 50/50, manifest 181/181, FIELD-003 tests 4/4 PASS; gameplay-scale review открыт. [Evidence](evidence/2026-09-27-field003-art-and-projectile-halo.md).
FIELD-004…010 thumbnail packet 2026-09-27: семь v001 изображений утверждены пользователем, включая отдельную закатную палитру FIELD-010; source/runtime PNG, import profiles и `FIELD-004…010-VISUAL-BACKGROUND` зарегистрированы. Art scope 50/50 и manifest 195/195 PASS; production field definitions/bindings и target-scale UI review остаются открытыми. [Evidence](evidence/2026-09-27-field004-010-thumbnails.md).
Obstacle art packet FIELD-001…010 2026-09-27: 52 утверждённых prop v001 подготовлены и зарегистрированы (2/4/4 дополнения для FIELD-001/002/003, по 6 для FIELD-004…010). FIELD-001…003 используют optional per-piece visual overrides поверх прежней player-only geometry; FIELD-004…010 ждут production layouts. Art scope 50/50, manifest 247/247 PASS; Unity full PASS — EditMode 859/859, PlayMode 30/30, 0 skipped (`TestResults/checks/20260927T153748-635944Z/summary.json`); gameplay-scale review открыт. [Evidence](evidence/2026-09-27-field-obstacle-art.md).
Ground texture packet FIELD-004…010 2026-09-27: семь утверждённых tile v001 подготовлены и зарегистрированы как `FIELD-004…010-VISUAL-GROUND`; FIELD-010 использует отдельную закатную палитру. Production bindings ждут definitions полей. Art scope 51/51, manifest 254/254 PASS (`TestResults/checks/20260927T160506-865622Z/summary.json`); repeat seams и gameplay-scale readability остаются открыты. [Evidence](evidence/2026-09-27-field004-010-ground-textures.md).
Per-run obstacle layouts 2026-09-27 ([DECISION-0068](../decisions/0068-per-run-obstacle-layouts.md)): FIELD-001…003 расставляют препятствия заново каждый забег из фиксированных паттернов по сетке ячеек (равномерная плотность, свободный старт, проходы между ячейками); Unity EditMode 854/854 + PlayMode 30/30 (раннер без итогового PASS из-за параллельных правок входных файлов) — [evidence](evidence/2026-09-27-per-run-obstacle-layouts.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-23).

### IP-30 — Production Travelers TRAVELER-001…010 и Book

Status: Implemented
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-07 — TRAVELER-001/002/005 и production Book. Required packets: F1-00/01/02/04/05; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-29, IP-12A
Blocked by: нет для реализации; данные, поведение и body art всех 10 ID подключены (production Book PICKUP-002 — с F1-07).
Remaining gates: G-03/G-10 semantics закрыты DECISION-0020/0033 и IP-28; G-11/G-12/scaling semantics — DECISION-0035. G-14/G-17, production Book card/ID/параметры, complete Traveler/support data, production bindings и gameplay-scale review. Designs TRAVELER-001…010 уже approved; body images для всех десяти подготовлены.
Remaining acceptance / IDs: ручной прогон новых путников (читаемость рывка 008, креста 010, всплеска скорости 007, лечения 009, кольца ауры 005, зигзага/рывков 002/003 и орбиты с телепортом 006) и gameplay-scale review тел и размеров collision 1.1…1.45; startup body art принят 2026-09-24, body остальных семи утверждены 2026-09-27 — [evidence](evidence/2026-09-27-traveler-body-art.md).
Startup subset F1-07: TRAVELER-001/002/005, FIELD-001 schedule, PICKUP-002 Implemented 2026-09-24 — [evidence](evidence/field001-f1-07-2026-09-24.md).
Data packet 2026-09-28: [travelers-v1](../balance/travelers-v1.md) — TRAVELER-003/004/006…010 выравниваются под один уровень прогрессии со стартовыми путниками своей роли (HP/контакт ±25%), XP/присутствие по роли, поддержка 007/009 и атаки 004/008/010; общий пул и масштабирование K без изменений. **Approved 2026-09-28** ([DECISION-0088](../decisions/0088-travelers-v1.md)); static validator PASS.
TRAVELER-003/004/006…010 Implemented 2026-09-28: генератор из travelers-v1, расписания FIELD-001…003 — все 10 путников; `TravelerDefinition.Scale` теперь сохраняет весь профиль атаки (cadence, follow-ups, windup) — раньше терял, не проявлялось без атакующих production-путников. Content Design синхронизирован. Unity 6000.6.0f1 с graphics: EditMode 921/921, PlayMode 34/34, 0 skipped; art provenance 254 PASS (`TestResults/checks/20260928T161222-756791Z/summary.json`). В игре не проверено.
Рывок TRAVELER-008 2026-09-30 ([DECISION-0119](../decisions/0119-knight-shoving-dash.md)): `TelegraphedDash` получил необязательные `dashDistance`/`dashTelegraphWidth`/`dashShove*` (длина 4.5, широкая полоса 1.6, толчок обычных врагов вбок, проход сквозь игрока с одним ударом); старые рывки без изменений. Targeted EditMode Enemy+Traveler+Bootstrap 295/295, 0 failed/skipped (`TestResults/checks/20260930T121445-204762Z/summary.json`); полный PlayMode не запускался, в игре не проверено.
Небоевые Путники и поддержка 2026-09-30 ([DECISION-0120](../decisions/0120-travelers-peaceful-movement-and-support.md), [travelers-v2](../balance/travelers-v2.json)): 002 уходит зигзагом, 003 рывками, 006 кружит по овалу вокруг игрока с телепортом; 005 аура −40 %, 007 всплеск +50 % скорости на 5 с, 009 лечение +20 HP раз в 3 с; зоны — эллипсы 0.8, у каждой умения процедурный эффект; `UpdateSupport` без LINQ. Targeted EditMode Traveler+Enemy+UI+Bootstrap+Telemetry 459/459, 0 failed/skipped (`TestResults/checks/20260930T123453-933690Z/summary.json`); полный smoke и PlayMode не запускались, в игре не проверено.
[DECISION-0121](../decisions/0121-wanderer-flee-distance-and-knight-dash-range.md) 2026-09-30: странствующие уходят с 0.8 ширины экрана (14.2), рывок Рыцаря 9 ед., полоса 0.53, линия 4 (`dashTelegraphLength`). Targeted EditMode Traveler+Enemy+UI+Bootstrap 435/435, 0 failed/skipped (`TestResults/checks/20260930T125748-621632Z/summary.json`); в игре не проверено.
[DECISION-0122](../decisions/0122-traveler-count-and-type-mix.md) 2026-09-30: 1–5 Путников на карту (`minCount`, распределение 10/25/30/25/10 %), выбор группами по одному на роль с мешком без повторов внутри роли, любой count. Targeted EditMode Traveler+Enemy+UI+Bootstrap+Field+Content 487/487, 0 failed/skipped (`TestResults/checks/20260930T131046-533881Z/summary.json`); PlayMode, performance с 5 Путниками и в игре не проверено.
[DECISION-0123](../decisions/0123-traveler-aura-shapes.md) 2026-09-30: контуры эффектов — 005 шестиугольный барьер, 009 расходящиеся круги, 007 орбитальные огоньки, 006 кольцо; `effectShape` в данных. Проверки форм выполнены в следующей дельте DECISION-0124; в игре не проверено.
[DECISION-0124](../decisions/0124-traveler-aura-ornaments-and-haste-status.md) 2026-09-30: процедурные орнаменты 005/007/009 и двухслойное движение, жёлтая полоса времени ускорения и мерцающая молния на ускоренных врагах; при замедлении обе полосы стоят одна под другой. Цифры поддержки не менялись. Full graphics PASS: 1137/1137 EditMode + 59/59 PlayMode, 0 failed/skipped; art 270/270, generation/audio PASS (`TestResults/checks/20260930T140205-890210Z/summary.json`). Игровой просмотр нового рисунка и плотности двух статусов открыт — [evidence](evidence/2026-09-30-traveler-aura-ornaments-and-haste-status.md).
Approved aura art follow-up 2026-09-30: пользователь выбрал и поручил подключить три рисунка; исходники 005/007/009 круглые, Unity сплющивает их до 0.8 после вращения в общем корне. Растровый орнамент заменил основной процедурный слой при прежнем pooled движении. Full graphics 1140/1140 EditMode + 59/59 PlayMode, 0 failed/skipped; art 273/273, generation/audio PASS (`TestResults/checks/20260930T145947-128613Z/summary.json`). Gameplay-scale review при нескольких Путниках открыт — [evidence](evidence/2026-09-30-traveler-aura-ornaments-and-haste-status.md).
Healing aura follow-up 2026-09-30: по игровому отзыву пользователя TRAVELER-009 затемнён (alpha 0.85 → 0.55), число волн 3 → 2, длительность 0.5 → 0.8 s; лечение и cooldown прежние. Targeted Traveler EditMode 42/42 PASS (`TestResults/checks/20260930T185017-213808Z/summary.json`); gameplay-scale review нового темпа открыт — [DECISION-0124](../decisions/0124-traveler-aura-ornaments-and-haste-status.md), [evidence](evidence/2026-09-30-traveler-aura-ornaments-and-haste-status.md).
Target implementation evidence: travelers-v1 (DECISION-0088), 2026-09-28.
Target verification evidence: свежий полный smoke объединённой версии — 921/921 EditMode + 34/34 PlayMode с graphics, 0 failed/skipped (`TestResults/checks/20260928T184120-318706Z/summary.json`); включает 13 ProductionTravelerCatalogTests. [Integration evidence](evidence/2026-09-28-worktree-integration.md); ручная приёмка выше остаётся открытой.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-30).

### IP-24 — Canonical Wave / Encounter Content и field bindings

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 900-second schedule и startup bindings. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-14, IP-20, IP-21, IP-23, IP-29, IP-30
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-23 (Blocked, target scope).
Remaining gates: CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон.
Remaining acceptance / IDs: Полные production encounter schedules и bindings полей 004…010 (002/003 реализованы, ручные прогоны открыты); CG-02/CG-04; Unity verification FIELD-001.
Anti-blob trial 2026-09-30: единый типаж `BLOB-FAN-001` (раз в 10 s, все плотные квадраты 3.5×3.5 wu с минимум 11 ordinary врагами, кроме центрального квадрата с игроком; 70 %, максимум 60 на квадрат, манёвр до 6 s) хранится отдельно и включён для FIELD-001…004 через ссылки в фазах; в расписаниях волн нет чисел разбивки, есть только ссылка и optional enemy IDs для получателей манёвра. Состав/частота спавна и wave modifiers не менялись. [DECISION-0130](../decisions/0130-periodic-blob-breakup-trial.md), [evidence](evidence/2026-09-30-periodic-blob-breakup.md).
Startup subset F1-08: FIELD-001-TIMELINE (900 s, hooks 450/810) и startup bindings Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
FIELD-001 opening-rate follow-up 2026-09-29 — Implemented по [DECISION-0103](../decisions/0103-field001-opening-spawn-rate.md): первые 30 s continuous cadence ×0.6, FIELD-002/003 без изменения; generated content STATIC PASS, Enemy/Bootstrap EditMode **248/248 PASS**, GameplaySmokeTests PlayMode **3/3 PASS**. [Evidence](evidence/2026-09-29-perimeter-and-opening-rate.md).
DECISION-0076 follow-up Verified 2026-09-27: FIELD-001 — 16 фаз с 2–4 типами, combat-фазы 70–90 s и передышки 20 s; cap 200 перенесён на timeline и применяется также к burst. Unity full PASS 870/870 + 30/30; [evidence](evidence/2026-09-27-wave-cap-and-field001-rhythm.md). Ручной плейтест открыт.
Текущая дельта: FIELD-001/002/003 используют общий cap 300 по [DECISION-0115](../decisions/0115-shared-300-enemy-cap.md). [Evidence](evidence/2026-09-30-shared-300-enemy-cap.md).
FIELD-003-TIMELINE Implemented 2026-09-27 по field003-v1 (DECISION-0067): 24 фазы, HP ×1.24, урон ×1.16, ENEMY-010 с первой волны; после DECISION-0076 использует общий технический cap 200. Исторический Unity full PASS 2026-09-27 (EditMode 847/847, PlayMode 30/30, `TestResults/checks/20260927T084545-984347Z/summary.json`); [evidence](evidence/2026-09-27-field003.md). Ручной прогон не выполнен.
Target implementation evidence: FIELD-001 rhythm и общий cap — [evidence](evidence/2026-09-27-wave-cap-and-field001-rhythm.md); остальные поля 004…010 остаются gated.
Target verification evidence: DECISION-0076 delta — Unity 6000.6.0f1, 870/870 EditMode + 30/30 PlayMode, 0 skipped; ручной плейтест не выполнен.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-24).

### IP-27 — End-to-end integration, regression и content validation

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-09 — полный стартовый run и приёмка FIELD-001 только initial content. Required packets: F1-00…08; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-00, IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-09, IP-10, IP-10A, IP-11, IP-12, IP-12A, IP-13, IP-14, IP-15, IP-16, IP-17, IP-18, IP-19, IP-20, IP-21, IP-22, IP-23, IP-24, IP-25, IP-26, IP-28, IP-29, IP-30, IP-31, IP-32
Blocked by: IP-17 (Blocked, target scope), IP-18 (Blocked, target scope), IP-19 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-23 (Blocked, target scope), IP-24 (Blocked, target scope), IP-25 (Blocked, field-001-start-R1 delta), IP-26 (Blocked, ui-entry-R1 и остальные экраны).
Remaining gates: Только реальные missing required contracts/data/asset checks полного scope этого плана. Уменьшение каталога возможно лишь как отдельное явное изменение плана; один smoke не закрывает content-complete verification.
Remaining acceptance / IDs: Все criteria/IDs из [спецификации](modules/IP-27-integration.md).
UI review delta 2026-09-28: Production composition первого ui-layout-R2 среза поставлена, проверена и принята пользователем — [runtime evidence](evidence/2026-09-28-ui-layout-r2-runtime.md#пользовательская-приёмка-игрового-интерфейса). IP-10A больше не блокирует integration; ui-entry-R1 и остальные экраны остаются в IP-26. Принятый F1-09 не отменён.
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-27).

### IP-33 — Production audio, общий звуковой язык

Status: Implemented
Scope revision: audio-first-pass-R1, явное поручение пользователя 2026-09-27 вне заблокированной F1-09 очереди.
Dependencies: F1-03 settings subset (DECISION-0038), F1-01/04/06/07/08 event and composition subsets — выполнены для FIELD-001.
Remaining acceptance: прослушивание обычной/плотной волны, босса, паузы и 5×; возможная корректировка громкости/тембров. Специальный low-HP и attack-telegraph contract остаются за пределами первого среза.
Target implementation evidence: 28 CC0-клипов, один общий boss track, 15 семейств событий, ограниченные голоса и real-time cooldown; [подробности](evidence/2026-09-27-production-audio.md).
Target verification evidence: audio integrity 28/28; после REPO-01 Unity 6000.6.0f1 EditMode 857/857 (включая 3 Audio tests) + PlayMode 30/30, 0 skipped; [evidence](evidence/2026-09-27-project-structure.md). Автоматические PlayMode-прогоны без вывода звука на динамики; художественный review остаётся открытым. Предыдущие прогоны — в исходном audio evidence.

### IP-34 — Автоматические прогоны баланса и прогрессии

Status: Verified
Scope revision: automated-runs-v7, расширение по поручению 2026-09-29.
Dependencies: IP-01, IP-02, IP-07, IP-16, IP-25, IP-31; F1-09 subset IP-27.
Current packet: AB-01…14 закрыты в пределах scoped приёмки выше; качество бота
для реальной балансировки и обучение новой модели этим не приняты.
Follow-up 2026-09-30 — Implemented: human template использует `activeFirst15/v1`,
скорость записи возвращается на 1×. Затронутые EditMode 44/44, full graphics
1051/1051 + 57/57 PASS; player по `f5fd816` собран, хеши совпали;
человеческая запись выполнена: 3 завершённых забега/14 016 samples.
[Evidence](evidence/2026-09-30-cap-replacement-and-human-draft.md).
Current gate: human session на player `f5fd816` завершена, три записи валидны,
четвёртая пустая `.partial` исключена. Перед сборкой full graphics 1051/1051 + 57/57,
Python 25/25 PASS ([build evidence](evidence/2026-09-30-cap-replacement-and-human-draft.md),
[session evidence](evidence/2026-09-30-human-demonstration-session.md)).
Offline обучение кандидата проведено, но он не выбран новой policy: held-out
accuracy ниже baseline на всех трёх забегах; closed-loop проверки нет
([evidence](evidence/2026-09-30-human-imitation-candidate.md)).
Recorder follow-up: снимки при смене направления между periodic samples,
старые JSONL совместимы; full graphics 1051/1051 + 58/58 PASS, новый player
и bot-labelled pilot 1141 samples validator PASS.
[Evidence](evidence/2026-09-30-action-change-recorder.md).
Human follow-up на `a568e9d`: 4 завершённых забега/11 687 samples validator
PASS, один короткий administrative abort исключён. Offline MLP уступил baseline
на каждом held-out забеге; closed-loop проверка и выбор policy открыты.
[Evidence](evidence/2026-09-30-human-action-change-session.md).
Research follow-up: двухэтапная модель с отдельным change gate и direction head
не дала практически полезного улучшения (66.929% против 66.886% baseline,
37/3 870 смен); policy не выбрана, игровой pilot для этого кандидата не запускался.
Python 31/31 PASS. [Evidence](evidence/2026-09-30-two-stage-imitation-probe.md).
Temporal follow-up: оба масштаба истории остались около repeat-previous
baseline, а change gate не стал пригодным для игры. Python 33/33 PASS;
runtime и content не менялись. Цель игрока не наблюдается в human JSONL;
следующий research scope — closed-loop оценка автономной policy по выживанию,
XP и уровню, без заявления о восстановлении человеческого намерения.
[Evidence](evidence/2026-09-30-temporal-imitation-probe.md).
Closed-loop bot pilot на закреплённом `develop-evg` `4eac2a4`: три значения
`trajectorySearch.contactPenalty` (20/60/120) дали по 3/3 natural Defeat;
контрольный `herdLoopAdaptive` тоже 3/3 Defeat, но два забега длились 455/562 s.
Новая policy не выбрана, пригодность для балансировки не установлена;
следующий research вопрос — связь выживания с убийствами и сбором XP.
Runtime, content и баланс не менялись.
[Evidence](evidence/2026-09-30-closed-loop-bot-pilot.md).
Herd XP follow-up: в двух долгих забегах 81/69 убийств, 15/26 собранного XP,
62/39 истёкшего базового XP. Проверен `sweepSeconds` 5→1 на том же player:
3/3 natural Defeat, среднее выживание 233.56 s против 375.08 s у контроля;
параметр не принят. Следующий candidate должен проверять выбор достижимого XP
или путь к нему; пригодная policy ещё не установлена.
[Evidence](evidence/2026-09-30-herd-xp-conversion-probe.md).
Ресурсный блокер снят: worktree целиком на D:, старый Git/Codex путь
сохранён junction-ссылкой. Активные сохранения возвращены в обычную папку C: после
ошибки записи через LocalLow junction; полная копия на D: сохранена, восстановленные
15992 файла сверены SHA-256; [relocation evidence](evidence/2026-09-29-project-disk-relocation.md).
Authorization: AB-01…08, bot-профили AB-09…13 и AB-14 recorder по поручениям пользователя 2026-09-29.
Acceptance: заманивание толпы, обход и возврат к XP при большой куче;
локальный сбор без кучи, HP-aware уклонение, mode telemetry, smoke и pilot
проверены как механика. AB-13 дополнительно проверяет поиск маршрутов с
моделью преследования: локальные сценарии проходят, три production забега
закончились ранним поражением. Улучшение XP/выживаемости и пригодность для
балансировки не установлены. Новый исследовательский подход/расширение модели
и сопоставимые многосидовые серии — отдельный scope, не закрытый этим Verified.
Целевой win rate, автоподбор и
визуальный reviewer остаются будущими отдельными решениями; `--visual` доступен
по явному запросу, но не запускался на экране в финальном пилоте.
Prerequisite audit 2026-09-29: проверены текущие `ProfileCodec/MemoryProfileStore`,
`IProfileService.PurchaseAsync`, launchers/ProfileSaveTask composition root,
revision-aware draft commands, PlayerMover и telemetry. Для полного v1 нужны
новые input/observation/report adapters, а не повторная реализация gameplay.
Content boundary: базовый пилот использует FIELD-001; дополнительные поля только
с готовыми production bindings. FIELD-004…010 не становятся доступными этим планом.
Implementation/verification evidence: [AB-01…08](evidence/2026-09-29-ip34-automation.md),
финальный Unity 6000.6.0f1 full graphics 980/980 EditMode + 52/52 PlayMode,
0 failed/skipped, Python 18/18; 8 natural template runs, one natural 1× run,
one 600 s window with 5 natural runs plus one censored. Live profile changed
during overlapping local activity without matching automation run IDs; a
separate controlled final-player run left profile/settings hashes unchanged.
Без правки баланса.
Documentation impact: IP-34 schema/examples, policy formula/limits,
[DECISION-0104](../decisions/0104-balance-runner-presentation.md) и инструкция
синхронизированы; GDD/CD и production balance без изменений.
AB-09 evidence: [XP-focused profile](evidence/2026-09-29-ip34-xp-bot.md);
пилот 1/1 natural loss (84.66 simulation s, 10 XP), без вывода об улучшении
относительно safePickup. GDD/CD и gameplay balance не менялись.
AB-10 evidence: [wide-arc XP profile](evidence/2026-09-29-ip34-orbit-bot.md);
финальный пилот 1/1 natural loss (855.39 simulation s, 13 XP, 174 expired),
без вывода о преимуществе. GDD/CD и gameplay balance не менялись.
AB-11 evidence: [herd-and-return profile](evidence/2026-09-29-ip34-herd-bot.md);
Unity 996/996 EditMode + 52/52 PlayMode, Python 18/18; финальный тихий пилот
1/1 natural loss (49.34 simulation s, 4 XP). Режимы работают, но выигрыш
по XP и выживанию не установлен. GDD/CD и gameplay balance не менялись.
AB-12 evidence: [adaptive herd profile](evidence/2026-09-29-ip34-adaptive-herd-bot.md);
Unity 999/999 EditMode + 52/52 PlayMode, Python 18/18; финальные три тихих
пилота 3/3 natural losses (7/15/31 XP), один дошёл до 812.6 simulation s.
Трек работает; преимущество не установлено. GDD/CD и gameplay balance не менялись.
AB-13 evidence: [trajectory-search experiment](evidence/2026-09-29-ip34-trajectory-bot.md);
Unity 1013/1013 EditMode + 52/52 PlayMode, Python 18/18; финальные три тихих
пилота 3/3 natural losses: 84.1/91.7/112.0 simulation s, 14/7/11 XP.
Модель решает контролируемый обход, но не обеспечивает полноценный забег.
Профиль остаётся экспериментальным; GDD/CD и gameplay balance не менялись.

AB-14 evidence: [demonstration recording](evidence/2026-09-29-ip34-demonstration-recording.md).
Native human input/паузы, bounded async JSONL, validator и оконный launcher проверены:
final full graphics 1043/1043 + 56/56 PASS после последней lifecycle delta,
Python 25/25. Новый player собран; тихий bot-labelled pilot дал чистую запись
191 samples, остановку по wall budget без зачёта поражения и неизменность
17 обычных profile/settings файлов. Это исторический итог AB-14 до human session;
позднейшие запись и offline кандидат — в evidence выше.

### IP-35 — Локализация: таблицы строк, английский и китайский

Status: Blocked
Scope revision: localization-draft-R0, черновик по запросу пользователя 2026-10-02 (GI-11).
Dependencies: IP-00, IP-25, IP-26 (UI и Settings уже существуют; полный IP-26 не требуется).
Gate: approval [DECISION-0141](../decisions/0141-localization-infrastructure.md) и ответы на её open questions.
Packets: L-01…L-06 по [спецификации](modules/IP-35-localization.md); первый после approval — L-01.
Execution order: не включён; место в очереди назначает пользователь при approval.
Evidence: нет.

## Status maintenance rule

После изменения статуса/API/acceptance пересчитать готовность потребителей и Next Ready по Execution order. Implemented означает выполненный полный обязательный scope; Verified — фактически пройденные проверки с evidence. Исторический test count не переносится автоматически. Каталоги ведут completed/remaining IDs здесь; если ни один оставшийся packet не готов, указывать конкретный Blocked gate. В STATUS оставлять краткий результат, дату, revision и ссылку на подробное evidence в `evidence/`; старые проверки не читать при выборе следующего IP. Подробности — [WORKFLOW](WORKFLOW.md).
