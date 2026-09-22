# FIELD-001 — законченный забег на стартовом наборе

Scope revision: `field-001-start-R1`, вложенный этап общего `design-sync-R2`.
Основание: [DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md),
[DECISION-0051](../../decisions/0051-field001-initial-slice.md).
Поправка состава ordinary enemies: [DECISION-0052](../../decisions/0052-field001-six-ordinary-enemies.md), ровно шесть IDs.
Очередь, текущие packet/IP статусы, completed IDs и evidence — только
[STATUS](../STATUS.md#field001-execution). Здесь находятся scope, зависимости и приёмка.

## Результат этапа

На новом профиле игрок выбирает Клёпку и Деревенскую окраину, играет полноценный
15-минутный забег с production-контентом, собирает разные билды из 10/10/5,
встречает обычных врагов, мини-босса, босса и возможных Путников, получает результат.
Ритм, подбор, контакт, читаемость и сила билдов приняты пользователем в игре.
Gameplay не подменяет отсутствующие production definitions тестовыми fixtures.

Это приёмка стартового опыта. Она не означает готовность всех возможных повторных
заходов на FIELD-001 после дальнейших открытий или готовность полной мета-игры.

## Точный состав и владельцы

| Область | Входит | Владелец |
|---|---|---|
| Active skills | SKILL-001…007, SKILL-010, SKILL-013, SKILL-014; все уровни 1–6 | [IP-17](../modules/IP-17-production-skills.md) |
| Passives | PASSIVE-001…005, PASSIVE-007…009, PASSIVE-011, PASSIVE-012; все уровни 1–6 | [IP-18](../modules/IP-18-production-passives.md) |
| Sets | SET-001, SET-004, SET-006, SET-010, SET-017; recipes, effects, UI/VFX | [IP-19](../modules/IP-19-production-sets.md) |
| Персонаж | CHAR-001, starting SKILL-001, stats/weights, body/selection crop | [IP-22](../modules/IP-22-production-characters.md) |
| Ordinary enemies | ENEMY-001 Селянин, 002 Гонец, 003 Дровосек, 004 Пращник, 005 Королевский лучник, 007 Охотничья гончая | [IP-20](../modules/IP-20-production-enemies.md) |
| Bosses | BOSS-001 Староста-герой; MIDBOSS-001 Старший загонщик | [IP-21](../modules/IP-21-production-bosses.md) |
| Travelers | TRAVELER-001 Дорожный громила, 002 Бродячий стрелок, 005 Паломник со щитом | [IP-30](../modules/IP-30-production-travelers.md) |
| Pickups | XP, PICKUP-001 Зелье; Книга (production ID/card завершается в F1-00) | IP-20/IP-30; frameworks IP-06/28 |
| Карта | FIELD-001 geometry, environment, thumbnail, metadata и 900-second encounter schedule | [IP-23](../modules/IP-23-production-fields.md), [IP-24](../modules/IP-24-production-waves.md) |
| Profile/UI | Новый профиль 10/10/5, стартовый выбор, draft/build, результат/save; необходимые изменения mapping | [IP-25](../modules/IP-25-meta-progression.md), [IP-26](../modules/IP-26-functional-ui.md) |
| Приёмка | Integration, density/performance/readability, реальные прогоны и исправления | [IP-27](../modules/IP-27-integration.md), IP-12A/31/32 |

Точные имена, поведение и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Набор автоматически сверяется с пятью существующими рецептами; число слотов 6+6.

## Context и переиспользуемая база

Читать WORKFLOW и записи владельцев в STATUS, затем только их Context и полные
карточки ID из таблицы. GDD: бой/XP/draft/sets, Путники, поля, персонажи и
мета-прогрессия. UI: выбор, HUD/draft/Pause Build, Путники/Книга, Results и Unlocks.
Art: нужные роли этих ID и действующий pipeline. Не загружать весь поздний каталог.

Переиспользовать проверенные lifecycle/combat/draft/skill/passive/set/character/
enemy/wave/boss/field/pickup/Traveler frameworks, профиль, UI и telemetry.
Проверить API и требуемый scope перед соответствующим packet; исторический test
count не заменяет свежую проверку изменённой конфигурации.

Read-only audit 2026-09-22: Gameplay composition использует
`Assets/Game/Bootstrap/FixtureRuntimeContentCatalog.cs`; `MetaEconomy.json`
содержит прежние initial/fieldClear/firstRun условия. Принятые иконки и world art
частично подключены к fixtures. Перенос production ID, данных и binding — работа
этого этапа, а не уже выполненный результат. Approved изображения не генерировать
заново без необходимости. Spawn-only benchmark не является доказательством FPS.

## Общая граница пакетов

Пакет — поднабор работ указанных IP, не новый независимый модуль. Для перехода
требуемые packet dependencies проверяются на этой scope revision; наличие Blocked
у полного каталога само по себе не блокирует готовый поднабор. Точные dependencies
и результаты проверок дублируются не в evidence, а фиксируются в STATUS.

Production assets/data, относящиеся к одному ID, проходят свои gates до его
production binding. Без полного набора можно использовать отдельный тестовый
harness с явными fixtures, но нельзя объявлять его готовой первой картой.
Общие ошибки frameworks исправляются у их владельца с проверкой потребителей.

<a id="f1-00"></a>
## F1-00 — Полный стартовый data packet и критерии баланса

Владельцы: IP-17…24/25/26/30/32. Dependencies: утверждённые DECISION-0050/0051;
чтение фактических schemas/validators/framework APIs, без требования готовых новых assets.

Результат: единый reviewable baseline только для выбранных ID. Матрица
`ID → required fields → canonical value/source → missing value → owner → acceptance`
и список уже пригодного art по manifest. Предложить конкретные недостающие числа
с units/ranges/rationale и согласовать оставшиеся продуктовые неоднозначности.
Уже принятые механики, рецепты и изображения не переутверждать.

Обязательно закрыть перед соответствующей реализацией:

- Skill L1–6 payloads: range/lifetime/speed/hit intervals/targeting/slow и прочие
  реально обязательные fields; passive mappings без hidden defaults.
- Пять set thresholds/effect values, в том числе SET-017 attack payload;
  global setDraftChance, production reroll/banish counts, XP progression/lifetime.
- CHAR-001 numeric weights/baseline/highlights; initial meta roster и миграция
  нового mapping, не выдающая 15/14/19 на новом production-профиле.
- Enemy contact intervals, sling/arrow projectile payload, archer reposition/distance и hound dash timing/speed/range, potion chance/heal/contact
  data; body contact authoring отдельно от визуального размера.
- Boss/mid-boss speed/telegraph/attack intervals/projectiles/dash/reward/phase
  boundaries; mid-boss и final-boss моменты в 900-second расписании.
- Traveler presence/XP/escape, wander/flee/support radius и reduction, базовое
  распределение 0–3, production Book ID/card/values. Сохранять утверждённый scaling.
- FIELD-001 geometry/pool/timeline: pressure/rest, continuous/burst counts/rates,
  caps и reachable placement. Fixture 200×200, 64 obstacles и cap 200 — материал
  для предложения, не автоматически принятые production числа.
- Целевая конфигурация устройства/разрешения и измеримые frame-time/load/pool
  bounds для итоговой проверки; не выдавать spawn benchmark за gameplay budget.

Приёмка: у каждого требуемого runtime field есть явное значение и источник либо
остаётся названный gate, не Ready для зависимой реализации. Для полного завершения
F1-00 все стартовые data gates закрыты и baseline принят; ссылки/IDs/units и пять
рецептов проверены. Изменения канона вносятся в его документы вместе с решениями.
Не писать gameplay-код и не производить арт внутри этого подготовительного packet.

<a id="f1-01"></a>
## F1-01 — Десять production active skills

Владелец IP-17; dependencies F1-00, framework IP-08/10A/12A для нужного scope.
Перенести выбранные 10 ID со всеми L1–6 и фактическим behavior; восполнить отсутствующие
executor-возможности только для них. Подключить approved icons и необходимые
world effects через общие presentation contracts. Работать per-ID, не тащить шесть
закрытых активок. Проверки: per-level/transition/targeting/hit rules, шесть concurrent
skills, pause/end/pool reuse, UI deltas и реальная читаемость механик.

<a id="f1-02"></a>
## F1-02 — Десять production passives

Владелец IP-18; dependencies F1-00/01, frameworks IP-09/28. Все десять определений
L1–6, icons/effect text, modifiers и замена уровней. Проверить HP ratio, healing,
potion chance, action speed, damage, pickup radius, reduction/knockback/size и их
границы. Общие pickup contracts допустимо тестировать с injected producer; реальный
PICKUP-001 binding обязателен к F1-05/08. Новые world sprites для passive не нужны.

<a id="f1-03"></a>
## F1-03 — Клёпка, стартовый профиль и выбор

Владельцы IP-22/25/26; dependencies F1-00/01/02, frameworks IP-12/16/10A.
Production CHAR-001 body/crop/stats/starting stone/weights. Новый отдельный
production-профиль имеет ровно 10/10/5 и CHAR-001/FIELD-001; понятные selection и
lock reasons. Не увеличивать начальный пул из-за уже импортированных иконок.

Синхронизировать economy unlock metadata с DECISION-0050, включая future IDs и
условие CHAR-002, только как данные доступа: боевую реализацию этих IDs не делать.
Проверить initial roster, locked filtering, zero weights, save/load/migration,
profile separation и отсутствие двойных unlocks/rewards. Старые production-покупки
не отзываются; fixture save не мигрирует в production автоматически.
Массовая реализация поздних персонажей и экранов их игрового контента исключена.

<a id="f1-04"></a>
## F1-04 — Шесть обычных врагов и зелье

Владелец IP-20; dependencies F1-00, frameworks IP-04/13/28/12A.
Production ENEMY-001…005, ENEMY-007 и PICKUP-001, все combat/drop/presentation bindings.
Гонец — быстрый melee по карточке, не стреляющий fixture с его картинкой.
Пращник получает собственную одобренную атаку камнем. Лучник сохраняет дистанцию, меняет позицию и стреляет быстрым одиночным снарядом; Гончая выполняет короткий рывок в зафиксированную позицию игрока. Новые body assets пройти
через art packet/pipeline; готовые тела Селянина/Гонца переиспользовать.

Проверить AI/attack/contact, per-life смерть/drop/XP, pause/end/pool reset,
проходимость enemy через obstacles, health/drop modifiers и доступность pickups.
Реальная оценка силуэтов/контакта в толпе входит в приёмку; полный density gate — F1-09.

<a id="f1-05"></a>
## F1-05 — Пять production sets

Владелец IP-19; dependencies F1-00/01/02/04, framework IP-11.
Полные recipes/thresholds/effects выбранных пяти ID и SET-017 attack/VFX. Проверить
recipe fulfilled отдельно от acquired, выдачу через обычный/Book draft и backfill,
banish, shared components, no recursion/source rules, potion/slow/orbit integration.

На initial pool каждый из остальных 15 рецептов должен иметь отсутствующий
компонент; locked set не появляется через reroll/backfill. Проверить совместимую
сборку SET-001/004/010/017: она помещается в 6+6. SET-006 проверяется отдельным
sustain-билдом. Не требовать одновременной сборки всех пяти.

<a id="f1-06"></a>
## F1-06 — Босс и мини-босс первой карты

Владелец IP-21; dependencies F1-00/01/04, frameworks IP-15/12A.
BOSS-001 и MIDBOSS-001 definitions, bodies, attacks/telegraphs, HP/name и rewards.
Проверить чередование веера/круга и 50% phase boundary Старосты, два рывка
Загонщика, damage/control, pause/terminal/pool cleanup. Убийство босса не завершает
run; выживание до 15:00 даёт победу и при живом боссе. Attack readability принять
в actual-scale harness; совместное давление со schedule проверить в F1-08/09.

<a id="f1-07"></a>
## F1-07 — Три Путника и production Книга

Владелец IP-30; dependencies F1-00/01/02/04/05, frameworks IP-29/28.
Только TRAVELER-001/002/005 и production Book с утверждённым в F1-00 ID.
Полные movement/attack/support/presence/XP данные, art и edge indicators.
Проверить боевое преследование, неагрессивное блуждание/уход и защиту ordinary
enemies; pause, scaling early/late, removal source effects, kill vs escape.

Проверить 0/1/2/3 appearances без повторов, допустимые близкие по времени события,
kill→Book→draft в initial pool, очередь и exhausted-pool currency. Три роли
обязательно воспроизводятся тестовыми seeds/harness, случайный прогон не заменяет
coverage. Семь поздних Путников не поставлять; прежние FIELD-002/005 contexts
сохранить как metadata без реализации этих полей.

<a id="f1-08"></a>
## F1-08 — Production FIELD-001 и полный забег

Владельцы IP-23/24/25/26; dependencies F1-00…07. Production geometry/environment,
metadata/thumbnail и 900-second timeline из F1-00. Первое поле: открытое пространство,
редкие обходимые obstacles, мягкий рост давления с передышками, немного пращников
с середины, постепенное введение Лучника/Гончей, mid-boss/final boss и три разрешённых Traveler types. Все шесть ordinary IDs должны появляться в полном расписании; их общий cap не увеличивается автоматически из-за разнообразия.

Собрать реальную composition, включающую только стартовые production definitions.
Убрать fixture fallback из этого пути; optional future unlock metadata не требует
создания поздних боевых definitions. Boundary/obstacles блокируют только игрока;
pickup/Traveler placement reachable. Все UI states и telemetry используют actual IDs.

Проверить load/restart/shutdown, field refs, timeline boundaries/same-time events,
cap/burst accounting, pause/resume, death/victory и отмену draft, Results/save/retry.
После поражения повтор с неизменным стартовым pool — in scope. После победы —
проверить сохранение награды, clear и unlock receipts; последующий gameplay с
расширенным pool вне этого этапа. Не подавлять законные unlocks и не подменять
недоставленный контент fixtures. Приёмочный повтор стартового run — на новом
изолированном профиле; это не постоянное правило FIELD-001.

<a id="f1-09"></a>
## F1-09 — Плейтест, доведение ощущений и приёмка

Владельцы IP-27/12A/31/32, исправления у владельцев затронутых систем;
dependencies F1-00…08. Цикл: реальный run → отзыв/OBS и report → конкретное
предложение изменения → необходимые approval → реализация → targeted checks →
повторная оценка. Telemetry pass не закрывает замечание игрока автоматически.

Минимальная матрица приёмки:

| Сценарий | Проверяемый результат |
|---|---|
| Новый профиль | Только CHAR-001/FIELD-001; 10/10/5 meta pool, никаких поздних выдач |
| Основной run | Реальные 15 минут, все фазы и mid/final boss по schedule; победа по таймеру |
| Разные билды | Полные прогоны с тремя различными направлениями: stone/ranged, slow/orbit, sustain; все пять set effects проверены дополнительно |
| Управление/награды | Контакт и pickup предсказуемы, игрок не застревает; XP/зелье/Книга видимы и подбираются |
| Темп | Каждая запланированная передышка/всплеск различима; нет непредусмотренной пустоты или скачка давления, открытые OBS закрыты повторной проверкой |
| Опасности | Telegraph соответствует hit geometry/timing; враги, projectiles, boss/Traveler indicators читаются среди навыков |
| Terminal/save | Поражение, Quit, пауза/draft около конца, Victory с живым боссом, duplicate/save failure/retry работают без второй награды |
| Нагрузка | Peak комбинация волн, 6 skills, совместимые sets, boss/Travelers проходит bounds из F1-00; нет unbounded pool/object growth |

Автотесты и ручные forced-build harness дают покрытие механик; жизнеспособность
билдов и темп подтверждаются реальными прогонами, не одним ускоренным smoke.
Результаты записывать в docs/playtests и evidence; все критичные gameplay/readability
замечания имеют исправление и повторную проверку. Пользователь отдельно принимает
целостное ощущение стартовой карты. Остаточные некритичные вопросы перечислены
явно; они не маскируют незавершённые обязательные mechanics/assets.

## Проверки и правила завершения

Для каждого packet выполнить checks его owning IP в выбранном scope плюс изменения
контрактов и нужные regression tests. Перед каждым Unity run — smoke-check safety;
не batch поверх открытого Editor. Для art AI самостоятельно готовит art packet,
запускает `scripts/art_pipeline.py` и `scripts/check_project.py` по scripts/README.
После стабилизации F1-08/09 — полный обязательный EditMode/PlayMode набор, build
проверка интегрированного player flow и manual matrix. Повторы — только после
влияющего изменения, ошибки или новой конкретной неопределённости.

Численные/visual approval действуют в своей области: принятая картинка не означает
готовую механику, а успешный тест не означает хороший темп. При завершении packet
обновить STATUS, evidence и remaining IDs владельца; consumers пересчитать по
явным packet dependencies. После финальной приёмки закрывается этот этап, не весь
каталог. Очередь общего backlog возобновляется только по следующей команде пользователя.

## Что остаётся в общем плане

- SKILL-008/009/011/012/015/016; PASSIVE-006/010/013/014;
  SET-002/003/005/007/008/009/011/012/013/014/015/016/018/019/020.
- CHAR-002…010, ENEMY-006 и ENEMY-008…020, BOSS-002…010, MIDBOSS-002…010,
  TRAVELER-003/004/006…010, FIELD-002…010 и их schedules/assets.
- Gameplay после поздних unlocks, включая повторный FIELD-001 с ними, и полная
  мета-прогрессия/UI при всех открытых каталогах; полный IP-27 regression/content completeness.
- Нерешённые G-04/G-05 для поздних SET-002/015 и прочие gates вне стартового состава.

В стартовый этап не добавляются новые content IDs, rework approved art, новые
системы статусов/оружия или redesign общих frameworks без конкретной необходимости.
