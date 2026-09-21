# DECISION-0035 — Traveler schedule, scaling и простые support rules

Status: Approved (явное поручение выбрать формулу/простые взаимодействия; interpretation единицы экрана ниже)
Date: 2026-09-21
Related IP: IP-16, IP-24, IP-26, IP-27, IP-29, IP-30

## Approval boundary

Пользователь указал: «появляются пусть на расстоянии 2 экранов», «выбор случайный
но без повторов», «появление ограничено не концом забега, а за 2 минуты до конца
забега», «формулу зависимости напиши сам. Меняется здоровье и урон у тех у кого
он есть», «По эффектам и взаимодействиям сделай всё максимально просто».
Формула и простые взаимодействия ниже выбраны в пределах этого явного поручения.
Production значения остальных TBD этим не утверждены. Следующим сообщением пользователь разрешил увеличить текущую арену «около 20 экранов».

## Schedule и типы

Число N ∈ {0,1,2,3} выбирается один раз по четырём JSON probabilities в [0,1],
сумма = 1 (с техническим float tolerance). Неверное распределение отвергается.
Типы выбираются равновероятно без возвращения из explicit unique field pool.
Для возможности N=3 pool содержит не менее трёх типов; неверный config отвергается,
число встреч не уменьшается скрыто. FIELD context карточки не означает exclusive
единственный доступный тип. Production field pools поставляет IP-24/IP-30.

Каждый момент независимо выбирается равномерно в [0, T−120] running seconds,
где T > 120 — configured длительность забега в секундах. При T=900 последнее
допустимое появление — 780 s (13:00). Близкие и одновременные появления разрешены.
Это явное изменение прежнего полного интервала [0,T] по запросу пользователя.
Отдельный seed/RNG stream не переставляет wave/draft outcomes.

Pause останавливает schedule, presence, cooldown и expiry support effects.
Presence задаётся отдельно в JSON, последние две минуты не гарантируют полного
presence lifetime. При terminal run все встречи/эффекты убираются без новых наград.
В точке deadline presence приоритет у escape; смертельный hit даёт Book только
строго до deadline и пока run Running. Уже выданные награды не повторяются.

## Scaling

`K = (1 + a × (r − 1)) × (1 + b × u)`

`u = clamp(t / (T − 120), 0, 1)`

- r — explicit progression rank поля, целое 1…10; это порядковая ступень поля,
  не спорная UI Difficulty шкала 1–5/1–10 (G-20 не закрывается).
- t — момент появления в running seconds, 0…T−120; T — длительность run >120 s.
- a=0.10 и b=0.50 — начальные безразмерные JSON coefficients. Допустимый диапазон
  каждого [0,1]; a управляет ростом между полями, b — ростом внутри run.
- K — фиксированный при spawn multiplier; при начальных коэффициентах 1…2.85.
- HP = baseHP × K; damage каждого contact/dash/projectile attack = baseDamage × K.
  Нулевой damage остаётся нулём. Значения конечные, baseHP >0, baseDamage ≥0.
- Скорость, knockback/resistance, cooldown, presence и support strength не масштабируются.

Пример: rank 5, t=390 s (6:30), T=900 s: u=0.5, K=1.4×1.25=1.75.
Базовые 1000 HP / 20 damage становятся 1750 HP / 35 damage.
Параметры и rank принадлежат content JSON; коэффициенты не зашиваются в C#.

## Минимальные взаимодействия

Support действует только на живых Ordinary enemies текущего run. Игрок, сам
источник, другие Travelers и все bosses исключены. Contact damage=0 не создаёт
damage/knockback/on-hit события и не превращает peaceful role в атакующую.

Ауры TRAVELER-005/009 — круг вокруг источника, без directional sector/line-of-sight.
Позиционирование TRAVELER-005 между игроком и группой сохраняется; отдельная
body-block/collision механика не добавляется. Эффект действует только внутри radius.
Несколько Traveler аур не складываются: для damage reduction и bonus knockback
resistance отдельно выбирается максимальное активное значение. Reduction ∈ [0,1),
итоговый knockback resistance ограничен 100%; базовый resistance сохраняется.

На враге максимум один Traveler shield. Cast TRAVELER-007 выбирает заданное число
ближайших Ordinary enemies в radius (tie — stable life order). Если shield отсутствует,
истёк или поглощён, создаётся новый. При повторном cast shield с capacity не меньше
текущего заменяет/полностью обновляет remaining HP и duration; более слабый игнорируется.
Новый принятый cast становится владельцем shield. Shield HP не суммируется, очереди нет.
Сначала применяется damage reduction, затем shield поглощает damage, остаток снимает
Health. Shield не меняет knockback и не является отдельной целью для атак игрока.

Shield duration >0 s, capacity >0 HP, cooldown >0 s, radius >0 world units и
target count >0 — explicit JSON; без скрытых чисел. Shield сохраняется после выхода
из radius до expiry. Уход/смерть источника немедленно снимает принадлежащий ему shield;
смерть цели снимает её shield. Aura source cleanup немедленен. End/rollback/pool return
удаляют все effects; снятие источника не снимает shield другого владельца.

## Spatial contract и увеличение fixture arena

Единица экрана конкретизирована как полная высота gameplay camera viewport в world
units, без camera shake. Spawn distance = 2 таких высоты от текущего центра игрока;
направление случайно среди допустимых точек внутри доступного игроку поля. Расстояние
одно во всех направлениях и не зависит от соотношения сторон окна. Нельзя скрыто
clamp точку ближе или создать недостижимого peaceful Traveler. Если geometry не
даёт допустимой точки, configuration validation/smoke должны выявлять это до игры.

По последующей команде пользователя арена увеличивается примерно до 20 экранов.
Принятая и озвученная интерпретация: 20 полных высот экрана по каждой стороне.
При orthographicSize=5 это внутренние 200×200 world units, стены на ±100.25 с
толщиной 0.5. Камера/скорость игрока/число врагов не меняются. Геометрия хранится в
FixtureArenaGeometry.json и запекается Unity Editor API; применяется к обоим текущим
fixture fields. Это размер текущей арены, не обязательный размер всех production fields.

## Consequences

G-11/G-12 и framework scaling semantics определены. Fixture geometry отдельно проверяется до завершения этого изменения.
Production presence/XP/support values, schedules/pools, Book card/ID/art остаются
IP-24/IP-30. Для IP-29 допустимы явно синтетические FIXTURE-* числа и минимум три
роли; это не выпуск десяти production definitions. Runtime checks ещё не выполнялись.
