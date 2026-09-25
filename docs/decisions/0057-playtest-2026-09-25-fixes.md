# DECISION-0057 — Исправления по плейтесту 2026-09-25

Status: Approved (поручение пользователя 2026-09-25 «по перечисленным 5 новым проблемам — иди по порядку, исправляй»; время опыта 45 s задал пользователь, остальные числа выбраны исполнителем, пересмотр — обычной правкой данных)
Date: 2026-09-25
Related IP: IP-14, IP-17, IP-11/IP-26 (UI), IP-12A
Related content IDs: FIELD-001-TIMELINE, ENEMY-007, SET-*
Источник: [2026-09-25_5233a664](../playtests/2026-09-25_5233a664.md)

## Context

Ручной прогон production FIELD-001 (commit `97f09d8`) дал пять замечаний в markers. Пользователь
поручил исправить их по порядку и уточнил время исчезновения опыта: 45 s вместо предложенных 30 s.

## Decision

### OBS-01 — стартовый спавн у края экрана

Ordinary враги появлялись на `spawnRadius` 12 units, тогда как половина высоты обзора — 5 units
(reference screen height 10), а первый continuous spawn — через 1.25 s. Первый враг доходил до
экрана несколько секунд. Новое необязательное поле таймлайна `openingSpawn`
(`durationSeconds` 20, `screenMargin` 1 unit, baseline `field.openingSpawn`): пока run time < 20 s,
spawner ставит врага по тому же seeded углу в точку, где луч из центра камеры выходит за
прямоугольник обзора, увеличенный на 1 unit (`WaveScreenEdgePlacement`). Камера следует за игроком,
размеры берутся из ортографической камеры в runtime. Без камеры или после 20 s — прежний радиус 12.
Интервалы, cap и состав фаз не меняются.

### OBS-02 — одинаковые умения между забегами

Драфт создавался с `SeededDraftRandom(setup.Draft.Seed)`, то есть с одним и тем же reference seed
в каждом run, вопреки `randomness.newRunSeedPolicy` baseline v1 («fresh seed per run, record
resolved seeds; reference seeds only for comparisons»). Теперь `GameplayCompositionRoot` берёт
свежий seed (`FreshRunSeed`) на каждый run; `UseReferenceSeeds` закрепляет reference seeds из JSON
для сравнительных прогонов и детерминированных smoke-тестов. Фактический seed записывается в provenance плейтеста
(`seeds.draft`, reference — `seeds.referenceDraft`). По уточнению пользователя 2026-09-25 («для волн
врагов тоже не нужна фиксация seed») так же получает свежий seed `WaveDirector` — состав и углы
спавна (тот же `UseReferenceSeeds`, provenance `seeds.wave` / `seeds.referenceWave`). По уточнению пользователя
2026-09-25 («пусть путники спавнятся тоже случайно») свежий seed получает и `TravelerEncounterRuntime`:
количество, типы и время путников, а также точки их появления и блуждание (тот же `UseReferenceSeeds`,
reference — `seed` расписания в JSON, provenance `seeds.traveler`). Pickup seeds в этом решении не меняются.

### OBS-03 — иконка сета в паузе

Раздел SET PROGRESS создавал карточки без иконки; иконку получали только собранные сеты.
`SetRecipeProgressViewState` несёт approved icon сета, карточка прогресса её показывает. Дефект
реализации, канон не меняется.

### OBS-04 — время жизни опыта

| ID | Было | Стало | Обоснование |
|---|---|---|---|
| FIELD-001 `experience.baseDropLifetimeSeconds` | 60 s | 45 s | Пользователь: «пусть лежит 45 секунд». Пауза и бонусы lifetime работают как раньше |

### OBS-05 — линия прицела рывка гончей

Во время dash telegraph враг рисовал красную линию прицела. Новый необязательный флаг движения
`showDashTelegraphLine` (нейтральное значение — линия показывается); у ENEMY-007 `false`. Windup
0.55 s, во время которого гончая стоит, сохранён как читаемое предупреждение. У боссов и fixture
dash-врагов линия остаётся; линия прицела дальних атак не меняется.

### Арт боссов и путников в игре (сообщение пользователя 2026-09-25)

Пользователь: у боссов и путников на первой карте вместо изображений цветные квадраты. Body-спрайты
MIDBOSS-001, BOSS-001, TRAVELER-001/002/005 были зарегистрированы и приняты 2026-09-24, но
`TravelerDefinition` не сообщал ссылки своего body, поэтому спрайты путников даже не попадали в production registry; `BossEncounterRuntime` и `TravelerEncounterRuntime` вызывали `EnemyFactory.Spawn` без sprite, motion,
contact и content registry, а `TravelerDefinition.Scale` пересобирает статы без ссылок на арт. Общий
`EnemyBodyVisual.Resolve` теперь даёт один и тот же арт обычным врагам, боссам и путникам; арт путника
берётся из немасштабированного body; role color путника тонирует только placeholder. Снаряды боссов
и путников получают registry: четыре атаки BOSS-001 стреляют approved `boss-001-projectile`
вместо placeholder. Дефект интеграции, канон не меняется;
contact circles боссов/путников — уже утверждённые данные их спрайтов (DECISION-0039).

## Consequences

Код: `WaveOpeningSpawnDefinition`, `WaveScreenEdgePlacement`, `WaveDirector.IsOpeningSpawnActive`,
spawner с камерой, `FreshRunSeed`, `DraftSeed` в composition root и provenance, иконка в
`SetRecipeProgressViewState`, `EnemyMovementProfile.ShowDashTelegraphLine`.
Данные: baseline v1 JSON/md, generator, `ProductionWaveTimeline.json`, `ProductionRunSetup.json`,
`ProductionEnemies.json`.
Тесты: геометрия и окно стартового спавна, разбор JSON, свежий seed, иконка сета в паузе,
lifetime 45 s, флаг линии у гончей и по умолчанию.
Unity-проверка 2026-09-25 — `check_project.py --scope full`: EditMode 741/741, PlayMode 27/27, 0 skipped, manifest PASS 103 (`TestResults/checks/20260925T183434-473761Z/summary.json`); ощущение старта, разнообразие драфта и вид гончей
проверяет повторный прогон пользователя.

## Approval

Пользователь, 2026-09-25: поручение исправить пять замечаний прогона `5233a664` и значение 45 s для
опыта. Направление изменений — цитаты пользователя в плейтесте.
