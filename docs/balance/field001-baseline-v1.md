# FIELD-001 — стартовый баланс v1

Ревизия: `field001-baseline-v1`, 2026-09-23. **Approved 2026-09-24** по делегированию
пользователя ([DECISION-0053](../decisions/0053-field001-difficulty-and-baseline.md)):
принятые исходные данные F1-01…09, ещё не проверенный в игре баланс и не production JSON. Подготовлен по поручению
пользователя самостоятельно заполнить первый шаг. Цель сложности утверждена
отдельно в [DECISION-0053](../decisions/0053-field001-difficulty-and-baseline.md).
Текущее исполнение и очередь — только [STATUS](../implementation/STATUS.md#field001-execution).

Источник поведения и уже заданных чисел — [Content Design](../Content_design.md),
общих правил — [Game Design](../Game_design.md). Все предлагаемые значения находятся
в [машиночитаемой таблице](field001-baseline-v1.json); её формат `field001-review-data`
не является DTO игры. Поля `shared` применяются ко всем шести строкам `levels`;
строка содержит итоговые значения своего уровня, не прибавку к предыдущему.
Проверка таблицы: `python -X utf8 docs/balance/validate_field001_baseline.py`.

## Цель и границы

Первая победа достигается главным образом обучением движению, сбору XP и выбору
билда. Пройти 900 секунд новым CHAR-001 без постоянных upgrades реально. Первые
несколько попыток новичка, как ожидается, заканчиваются поражением; это гипотеза
о сложности, а не запрет на победу опытного игрока с первой попытки. Нет скрытого
усиления по номеру попытки, обязательной покупки или гарантированной смерти.

Ровно 10 active, 10 passive, 5 sets; шесть ordinary enemies, один mid-boss,
один final boss и по одному типу Путника каждой роли. Поздние unlocks сохраняются
по DECISION-0050 и доступны на первой карте после открытия. Здесь проверяем только
начальный набор и нулевые постоянные бонусы. Сложность 1/5 в выборе поля обозначает
относительное место в маршруте, а не обещает лёгкую победу.

Все L1–6 улучшения навыков/пассивов и базовые профили bosses/Travelers сохранены.
По дополнительному разрешению пользователя обычные враги также перебалансированы:
точная разница с карточками приведена ниже и в `ordinaryEnemyChanges`. Остальные
новые числа заполняют пробелы: XP, дистанции, intervals, set thresholds, расписание,
rewards и geometry. Все множители волн равны 1: давление создают плотность и
сочетания угроз, а не незаметное увеличение HP знакомого врага с каждой минутой.

## Клёпка, развитие и восстановление

| Параметр | v1 | Основание |
|---|---:|---|
| HP / движение | 100 / 3 world units/s | 100 HP из карточки; 3 — конкретизация 100% скорости |
| Начало | SKILL-001 L1, level 1, 6+6 слотов | Утверждённые правила |
| Базовая регенерация / возврат истёкшего XP | 0 / 0 | Не давать бесплатный sustain и удалённую прокачку |
| XP pickup radius / lifetime | 0.5 units / 45 s | За опытом нужно возвращаться, но есть время на обход; lifetime 60 → 45 s по плейтесту ([DECISION-0057](../decisions/0057-playtest-2026-09-25-fixes.md)); PASSIVE-007 L6 даёт ×3.33 = 1.665 units (было ×5 = 2.5, [DECISION-0055](../decisions/0055-playtest-2026-09-24-fixes.md) → [DECISION-0059](../decisions/0059-playtest-2026-09-25-evening-fixes.md)) |
| Draft | 3 предложения; 3 reroll, 2 banish за run | Позволяет исправить несколько неудачных выборов |
| setDraftChance | 0.50 для каждого подходящего сета | Рецепт открывает шанс предложения, не выдаёт сет |
| Веса Клёпки | SKILL-002/005/007: 1.35; 009/014: 0.70; остальные: 1 | Сохраняется направление карточки; закрытый 009 в draft не попадает |
| Пассивные веса | Все 1 | Нейтральный выбор; locked IDs фильтруются до розыгрыша |
| Зелье | 18 HP, базовый шанс 1.5% с ordinary kill | Ошибка ощутима; нет гарантированной компенсации каждого контакта |
| Зелье / Книга | contact radius 0.4; без expiry; scatter 0.3 | Бонус XP-radius не применяется; `lifetimeSeconds: null`, не 0 |
| Книга | PICKUP-002, 1 при убийстве Путника | Внеочередной draft, 0 XP от самой Книги; при пустом пуле 50 валюты |

Все прочие character multipliers равны 1, bonuses/resistances равны 0;
полный перечень — `character.stats`. Веса поздних IDs — только metadata, их
боевые определения в F1 не добавляются. Highlights: «Начинает с Броска камня»,
«Чаще находит иглы, копьё и молнию», «Без начальных бонусов к характеристикам».
Старые production-покупки сохраняются; fixture save не превращается в production
save. Новый профиль получает ровно начальный набор из DECISION-0050; правило
CHAR-002 (покупка за 100 после первого clear FIELD-001) не изменяется.

Стоимость перехода с уровня L на L+1:

`C(L) = 4 + floor(1.5 × (L − 1)) XP`, целый L=1…60.

Массив хранит 60 явных значений; после него цена остаётся 92 XP, как повторение
последнего порога в текущем `ExperienceProgression`, без нового level cap.
Например, C(1)=4, C(10)=17; для достижения L7 нужно суммарно 45 XP, L18 — 268,
L28 — 628, L40 — 1258. Переполнение XP сохраняется по действующим правилам.

Ориентиры для живого прогона без meta: к 2:00 L5–7, к 5:00 L13–18, к 8:00
L22–28, к концу L35–45. Это диагностические диапазоны, не гарантированные награды:
реальный темп зависит от убийств, маршрута сбора и отложенных выборов. Нельзя
подгонять игроку XP до этих отметок. При 39 level-ups у L40 есть бюджет развить
несколько атак и собрать 1–2 сета, но не прокачать все 12 слотов до L6.

Drop chance: `p = clamp(pBase × (1 + Σ bonus), 0, 1)`, все вероятности 0…1.
Например, PASSIVE-002 L6 даёт 0.015×1.6=2.4%, вместе с SET-006 — 2.775%.
Лечение: `min(18 × (1 + restorationBonus), maxHP − HP)`; с PASSIVE-009 L2
и SET-006 это 25.2 HP до ограничения здоровьем. Шанс с bosses/Travelers не
добавляется: они дают свои XP, Путники ещё Книгу. Пауза останавливает regen/timers.

## Активные умения: дополненные параметры

Все расстояния ниже в world units, время в running seconds. Точные шесть строк
каждого ID — `skills` в JSON. Указанные hit radii — gameplay-параметры снарядов;
body contact врагов измеряется отдельно по утверждённому изображению.

| ID | Дополнение к канонической карточке L1 | Существенные границы |
|---|---|---|
| SKILL-001 | targeting/range 5; speed 10; radius 0.16; ricochet search 3 | L3 speed 11.5 без роста дальности; L4 retention 0.8 damage/KB; нет возврата в уже поражённую цель |
| SKILL-002 | targeting 6; range 4; speed 9; radius 0.07 | L6 range 5.4; 11 игл, каждая максимум 2 цели |
| SKILL-003 | вращение 120°/s; per-blade/per-target hit interval 0.6 | Непрерывная орбита; L6 4 клинка, 180°/s, orbit radius 2.25, blade radius 0.496 |
| SKILL-004 | расширение от 0 до полного radius за 0.25 s | Каждая волна hit-once, не моментальный урон всему диску; вторая через 0.35 s |
| SKILL-005 | range 6; speed 12; half-width/collision radius 0.12 | 3 цели означает 2 дополнительных пробивания; L6 unlimited только до expiry |
| SKILL-006 | targeting 6; outbound range 4; speed 6; radius 0.22; lifetime 4 | L4 два под углом 20°; общий target cooldown 1 s для всех бумерангов данного skill |
| SKILL-007 | targeting 6; jump range 2.5 | L6 range 3.375 и damage 33.6: последний уровень добавляет damage, не range |
| SKILL-010 | targeting 8; следующие telegraphs стартуют с шагом 0.3 s | Каждая точка фиксируется при начале своего telegraph; цели только на экране, нет другой цели — случайная точка экрана в radius; область — эллипс radius × 0.7 по вертикали ([DECISION-0058](../decisions/0058-on-screen-targeting-and-strike-visual.md)) |
| SKILL-013 | targeting 6; range 4; speed 8; radius 0.10 | L6 radius 0.135: +15% и +20% от базы складываются; slow 30% на 2 s |
| SKILL-014 | speed 3; lifetime 1.8; range 5.4; radius 0.22 | L4 меняет size, не speed; L6 impact первой цели, взрыв при второй либо expiry |

Общий nonzero knockback duration 0.12 s. Нулевой knockback не создаёт impulse.
Направления и resistance — по карточкам/GDD; slow не складывается аддитивно.
Начальное направление до первого input — вправо (0°), rotation-per-activation 0°.
Неиспользуемые wave/effect damage multipliers =1; wave rotation/delay =0, кроме
явно описанных delayed waves. Неиспользуемые control values =0. Для обычного
projectile: stop-after=0, ricochet=0, repeat-target=false, explosion=0 и
explode-on-expiry=false, кроме явно заданного поведения выбранного skill.

Для projectile lifetime=`range/speed`, оба >0; например камень L3 живёт
5/11.5≈0.434783 s. При внешнем range bonus растёт только lifetime, не speed
одновременно. Ricochet не перезапускает lifetime. `maxHitTargets=n` переводится
в DTO `pierceCount=n−1`; `null` у копья L6 означает unlimited flag, не infinity.
У сферы explosion damage multiplier=`explosionDamage/impactDamage`; обе части
получают общий damage bonus один раз. Аналогично explosion KB multiplier задаётся
отношением explosion/impact KB. Прошедшая сквозь первую цель сфера L6 там не взрывается.

Числа уровня уже включают собственные L2…L6 прибавки. Например камень L6:
20×(1+0.30+0.25)=31 damage; нельзя ещё раз применить его level bonuses.
Cooldown=`baseCooldown/(1+intrinsicActionBonus+Σ externalActionBonus)`;
камень L5 с Метрономом L6: 1.2/(1+0.33+0.25)≈0.759494 s. Внутриволновые задержки,
slow duration и orbit hit interval этим не сокращаются. У непрерывной орбиты нет
второй копии от action speed и нет окна без клинков при смене уровня.

Цели выбираются только на видимом экране. Без валидной цели на экране target-dependent skill
не ждёт: направленный срабатывает в предыдущем направлении, удар по точке — в случайную точку
экрана в radius ([DECISION-0058](../decisions/0058-on-screen-targeting-and-strike-visual.md));
Self/MovementDirection/IndependentRandom
работают без enemy target. У SKILL-010 смерть выбранного врага после фиксации точки
не отменяет удар в эту точку и не перенаводит его. Delayed waves отменяются на
terminal state; пауза сохраняет оставшиеся интервалы.

## Пассивные предметы и сеты

Все 60 passive rows перенесены из карточек без ребаланса. Значения уровня заменяют
предыдущее значение, а не суммируются с ним. MaxHP меняет текущий HP пропорционально;
это не лечение. PASSIVE-002 одновременно даёт regen и potion chance, PASSIVE-011 —
resistance и outgoing knockback. Прочие отсутствующие stat channels нейтральны.

| Сет | Минимальные уровни компонентов | Дополнительный эффект v1 |
|---|---|---|
| SET-001 Тяжёлый боезапас | камень 3, точильный камень 2, пояс 2 | Только камню: +60% damage, +25% size, +35% outgoing KB |
| SET-004 Ледяной таран | волна 3, лёд 3, пояс 2 | +50% outgoing KB игрока по уже slowed цели; волне ещё +25% radius и +35% KB |
| SET-006 Полевой медик | сердце 3, собиратель 3, настойка 2 | +20% maxHP, +0.6 HP/s regen, +20% restoration, +25% относительного potion chance |
| SET-010 Холодная орбита | орбита 4, лёд 3, сапоги 2, кожа 2 | В текущем orbit radius slow 15%, refresh каждые 0.1 s, duration 0.25 s; орбите +40% damage по slowed |
| SET-017 Падающая звезда | небесный удар 4, метроном 3, замах 2 | 150 damage, radius 2.2, targeting 8, KB 1, telegraph 0.65 s, отдельный cooldown 7.5 s |

Рецепты неизменны; ниже thresholds сет не предлагается. Получение сета требует
отдельного выбора после появления в draft. SET-001 требует минимум 6 улучшений
после стартового камня плюс выбор сета, то есть не раньше L8 без Книг; SET-010 —
11 улучшений плюс выбор, не раньше L13. Никакого обещания выдачи в этом уровне нет.

Все внешние бонусы одного канала складываются до применения к resolved skill level.
Для wave L3 с поясом L2 и SET-004 по уже slowed цели:
KB=2.4×(1+0.2+0.35+0.5)=4.92 до resistance; по обычной цели 3.72.
У цели с resistance 20% первое значение становится 3.936. Predicate «уже slowed»
проверяется до применения slow текущего hit: первый ледяной hit не усиливает сам себя.
SET-010 обновляет slow независимо от blade hit; stronger slow сохраняет приоритет,
slow перестаёт обновляться сразу после выхода из radius/удаления источника.
Нет отдельной aura-entity или дополнительной hidden debuff stack.

SET-017 делает первую попытку через полные 7.5 s после выбора, затем с тем же
интервалом. Без цели попытка пропускается; следующий интервал полный. Snapshot точки
в начале telegraph. Generic damage/KB и size/range применяются один раз; action speed
не меняет cooldown, set cast не считается обычной skill activation и не вызывает
proc chains. Пауза/смерть/terminal state следуют общему lifecycle.

## Шесть обычных врагов

| ID | HP | Speed | Contact / XP | Дополнение |
|---|---:|---:|---|---|
| ENEMY-001 Селянин | 32 | 1.20 | 10 / 1 | Прямое преследование |
| ENEMY-002 Гонец | 24 | 2.25 | 8 / 1 | Прямое преследование; никаких fixture-писем или стрельбы |
| ENEMY-003 Дровосек | 150 | 0.80 | 20 / 3 | Медленный плотный заслон, KB resistance 20% |
| ENEMY-004 Пращник | 48 | 0.95 | 8 / 2 | Distance 5±0.5; projectile 10 damage каждые 2.4 s |
| ENEMY-005 Лучник | 56 | 1.10 | 9 / 3 | Distance 6±0.5; очередь 3×7 damage (интервал 0.18 s, ±6°) каждые 2.0 s; смена позиции ([DECISION-0055](../decisions/0055-playtest-2026-09-24-fixes.md)) |
| ENEMY-007 Гончая | 64 | 1.55 | 14 / 4 | Telegraph 0.55 s → dash 0.55 s на speed×3.2 → recovery 2.5 s ([DECISION-0055](../decisions/0055-playtest-2026-09-24-fixes.md)) |

Предлагаемый ребаланс относительно текущих карточек:

| ID | HP: было → v1 | Speed: было → v1 | Contact: было → v1 | Причина |
|---|---|---|---|---|
| 001 | 40 → 32 | 1.00 → 1.20 | 10 → 10 | Больше давления движением; AoE/апгрейды быстрее расчищают массу |
| 002 | 28 → 24 | 1.55 → 2.25 | 8 → 8 | Быстро перекрывает маршрут, но камень L2 убивает одним попаданием |
| 003 | 130 → 150 | 0.65 → 0.80 | 18 → 20 | Медленная крепкая цель, которую опасно проталкивать телом |
| 004 | 55 → 48 | 0.75 → 0.95 | 8 → 8 | Держит дистанцию увереннее; добравшись, игрок быстро убирает стрелка |
| 005 | 60 → 56 | 0.80 → 1.10 | 9 → 9 | Позиционирование заметнее; приоритетная цель не превращается в HP-стену |
| 007 | 70 → 64 | 0.90 → 1.55 | 15 → 14 | Чаще достигает опасной дистанции; урон за одну ошибку немного ниже |

Speed 3 у игрока всё ещё выше ordinary pursuit speed; dash Гончей 4.96 выше скорости
игрока, но заранее показан и не доворачивает. Первый Гонец требует двух базовых
камней (20+20), а с L2 — одного (26≥24). Селянин требует двух базовых камней;
SKILL-001 L2 с Точильным камнем L3 наносит 26×1.24=32.24 и убивает одним hit.
Это конкретные точки, в которых вложение в damage меняет расчистку. Шансы drops,
XP, collision size, resistance и knockback обычных врагов в этом ребалансе сохранены.

Contact interval у всех 1 s, первый contact hit сразу; общий invulnerability timer
между разными врагами не вводится. Все canonical knockbacks и collisionSize — в JSON.
Праща: windup 0.55 s, speed 4.5, lifetime 1.8, radius 0.14, KB 0.35.
Стрела: windup 0.45 s, speed 6.5, lifetime 1.5, radius 0.10, KB 0.10.
Cooldown считается от начала одного windup до начала следующего; первый windup
после полного cooldown от spawn. Aim фиксируется в начале windup; single projectile,
без pierce/explosion/slow. Стрелок продолжает движение во время windup. Лучник после
DECISION-0055 стреляет очередью Burst: 3 стрелы через 0.18 s, каждая со случайным
отклонением ±6° от текущего прицела.

Лучник 2 s держит дистанцию, затем 1 s перемещается по касательной с коррекцией
дистанции; цикл 3 s, lateral strength 0.5, знак касательной меняется каждый цикл.
Движение нормируется до speed 1.1, не ускоряется от сложения осей. Гончая фиксирует
направление в начале windup; в windup стоит, вне dash преследует. Красная линия прицела во время windup не рисуется (`showDashTelegraphLine: false`, DECISION-0057); у боссов линия сохраняется. Первый dash через
2.5 s после spawn (было 4.5, DECISION-0055); дистанция рывка 1.55×3.2×0.55=2.728. Contact и dash используют один
contact timer: переход фазы не даёт дополнительного instant hit.

При 100 HP без защиты нужно 10 контактов Селянина либо 5 Дровосека для смерти.
Это не время гарантированной жизни: разные источники могут попадать одновременно.
HP не растёт с минутой забега. Три melee-типа остаются минимум 74% любой смеси.

## Боссы и Путники

**MIDBOSS-001, 7:30:** 1200 HP, speed 0.85, contact 22 раз в 1 s, resistance 30%,
XP 60. Два рывка подряд: первый telegraph 0.65 s, dash 0.45 s; затем второй
telegraph 0.35 s с новым snapshot направления и dash 0.45 s. В каждом telegraph
стоит; dash speed 4.25 (base×5), длина 1.9125. После пары 5.5 s обычного pursuit,
первая пара также после 5.5 s. Contact KB 0.55, dash KB 0.8. Нет третьего скрытого
рывка или reset contact timer. Если игрок у центра врага — последнее валидное
направление, изначально вправо. В 7:30–8:30 ниже темп подкреплений.

**BOSS-001, 13:30:** 4500 HP, speed 0.7, contact 30 раз в 1 s, resistance 60%,
XP 150, contact KB 0.6. Чередует fan из 5 projectiles (18 damage, 70°, speed 4.8,
lifetime 3, radius 0.14, KB 0.25) и ring из 10 (14 damage, speed 3.8, lifetime 3,
radius 0.13, KB 0.15). Telegraph 0.7 s у обоих; интервал стартов windup 3.6 s,
первый спустя 3.6 s после spawn. Fan направлен в позицию игрока на начало telegraph;
ring начинается с 0°, шаг 36°, без скрытого вращения. Движение во время windup
сохраняется. При HP строго ниже 50% следующие интервалы 2.88 s (−20%); уже начатый
windup не ускоряется, очередь fan/ring не сбрасывается. Все KB duration 0.12 s.
Телепорт ([DECISION-0059](../decisions/0059-playtest-2026-09-25-evening-fixes.md)): игрок непрерывно дальше 5 units 5 s →
telegraph 0.6 s в случайной точке окружности 2.2 units вокруг игрока → перенос и удар радиусом 2.6:
20 damage, KB 0.5; эффект удара 0.45 s; таймер заново после удара, пауза его не двигает.

Boss offsets от игрока: mid (−8,0), final (+8,0); reachable/clamped placement по
действующему field contract. Убийство любого босса не завершает run, не даёт Книгу
или гарантированное Зелье. Неубитый mid-boss может остаться до final boss. Победа —
живым достичь 15:00; HP 4500 не является DPS-порогом обязательного убийства.

| Traveler | База HP / speed / contact | Presence / XP | Дополнение |
|---|---|---|---|
| TRAVELER-001 | 650 / 0.75 / 20 | 90 s / 12 | Преследует, interval 1 s; KB 0.55, resistance 25% |
| TRAVELER-002 | 560 / 1.00 / 0 | 75 s / 10 | Блуждает 3 s, отдыхает 0.5 s; при distance<3 уходит 1.5 s; resistance 15% |
| TRAVELER-005 | 1150 / 0.65 / 0 | 90 s / 18 | Guard offset 1; radius 3; reduction ordinary damage 20%; resistance 65% |

Награда за убийство — одна Книга плюс указанный отдельный XP drop; уход/cleanup
ничего не выдаёт. Support не помогает себе, другим Путникам или боссам; одинаковые
support effects берут сильнейший, не сумму. Нет ordinary целей — мирное блуждание.
Support прекращается сразу при death/escape. Marker: combat/nonaggressive/support;
цвета и уже утверждённый указатель направления сохраняются из presentation contract.

Вероятности количества 0/1/2/3: **15% / 40% / 35% / 10%**, среднее 1.4.
Три типа выбираются равномерно без повторов, времена независимо из [0,780] s;
возможны раннее появление и несколько рядом по времени. Не добавлять страховку
первой минуты или обязательного Путника. Spawn — две полные viewport heights от
игрока, 32 placement attempts с действующим fallback внутри поля.

Сохраняется `K=(1+0.10×(r−1))×(1+0.50×clamp(t/780,0,1))`, r=1 для FIELD-001,
t=running time при появлении. Например, t=390: K=1.25, громила 812.5 HP и 25 contact
damage. Только HP и ненулевой damage, без округления до целого; speed, support,
presence, XP и Книга не масштабируются. Сильного раннего Путника разрешено пропустить.

## Поле и 900 секунд волн

Предлагается сохранить масштаб знакомой арены: 200×200 units, reference viewport
height 10 (20 высот на сторону), wall thickness 1, старт (0,0). 64 одиночных
препятствия: 16 в центральной области ±20, остальные вне центрального квадрата ±22;
минимум 3 units между AABB. Типы: пень 1.2×1.0 и короткий плетень 2.4×0.5,
без поворота. Центры и габариты всех 64 записаны в JSON, а не оставлены на случайный
runtime генератор. Вокруг старта свободная область; стены/объекты блокируют только
игрока. Враги и pickups не получают новых collision exceptions.

Ordinary spawn radius 12, uniform angle по отдельному RNG. Первые 20 s (`field.openingSpawn`) ordinary враги появляются по тому же углу сразу за краем видимой камеры — прямоугольник обзора плюс 1 unit — чтобы первые враги были видны почти сразу ([DECISION-0057](../decisions/0057-playtest-2026-09-25-fixes.md)). Геометрия размещения
не смешивает random stream выбора типа врага. На границах используется текущий
field placement contract; нельзя спавнить внутри player collider. Collision rect
должен быть визуально читаемым на approved art, это проверка F1-08/09.

Ниже смеси идут в порядке **001 / 002 / 003 / 004 / 005 / 007**, веса в процентах.
«Каждые» — один enemy за interval, cap ограничивает только continuous.

| Время | Спавн | Cap | Смесь | Назначение |
|---|---|---:|---|---|
| 0:00–1:00 | каждые 1.25 s | 40 | 90/10/0/0/0/0 | Контакт, камень, первые уровни |
| 1:00–1:55 | 0.90 s | 60 | 70/25/5/0/0/0 | Первые тяжёлые цели |
| 1:55–2:00 | burst 12 | — | 70/25/5/0/0/0 | Первый обход группы |
| 2:00–2:30 | 2.40 s | 45 | 70/25/5/0/0/0 | Сбор XP |
| 2:30–3:30 | 0.65 s | 90 | 60/25/15/0/0/0 | Нужны несколько источников damage |
| 3:30–4:30 | 0.55 s | 110 | 55/22/15/8/0/0 | Первые пращники |
| 4:30–4:35 | burst 18 | — | 55/22/15/8/0/0 | Давление со стрельбой |
| 4:35–5:00 | 2.20 s | 80 | 55/22/15/8/0/0 | Сбор и смена маршрута |
| 5:00–6:00 | 0.50 s | 120 | 47/22/15/8/0/8 | Первый dash |
| 6:00–7:00 | 0.45 s | 140 | 40/22/15/8/7/8 | Лучник, весь roster |
| 7:00–7:05 | burst 18 | — | 40/22/15/8/7/8 | Проверка расчистки |
| 7:05–7:30 | 1.80 s | 100 | 50/20/15/5/5/5 | Окно перед mid-boss |
| 7:30–8:30 | 0.60 s | 120 | 45/20/15/7/6/7 | Mid-boss, меньше подкреплений |
| 8:30–9:30 | 0.38 s | 160 | 38/22/16/8/8/8 | Проверка связности билда |
| 9:30–9:35 | burst 24 | — | 38/22/16/8/8/8 | Массовая волна |
| 9:35–10:00 | 1.60 s | 110 | 50/20/15/5/5/5 | Передышка |
| 10:00–11:30 | 0.34 s | 180 | 35/24/16/8/8/9 | Длительное давление |
| 11:30–11:35 | burst 24 | — | 35/24/16/8/8/9 | Проверка late AoE/control |
| 11:35–12:00 | 1.40 s | 130 | 45/22/18/5/5/5 | Сбор перед финалом |
| 12:00–13:00 | 0.30 s | 200 | 33/25/16/8/8/10 | Пик обычной облавы |
| 13:00–13:30 | 1.20 s | 140 | 48/22/15/5/5/5 | Подготовка к final boss |
| 13:30–14:00 | 0.65 s | 160 | 45/22/15/6/5/7 | Чтение атак босса |
| 14:00–14:55 | 0.28 s | 200 | 35/25/15/8/7/10 | Последняя проверка маршрута |
| 14:55–15:00 | burst 20 | — | 50/25/15/4/3/3 | Последний прорыв |

Каждый burst появляется одной группой в начале своей 5-second фазы, offset 0,
window 1 s. Пропущенное окно не догоняется. Caps burst-строк в JSON технически
заполнены, но не участвуют в ограничении группы. Передышки не despawn уже живых
врагов; переполненный continuous cap просто прекращает новые появления.
При смене фазы spawn timer начинает новый interval по текущему WaveDirector.

Номинально без cap/пропусков фаз получается около 1735 ordinary spawn requests и
2883 XP при убийстве всех, по математическому ожиданию смеси. Это не прогноз kills
или полученного XP. При сборе порядка 34–55% этого бюджета достигаются L35–45.
Boss/Traveler XP считаются отдельно. Практическую доступность XP определит прогон.

Без deaths верхняя граница накопления, учитывающая порядок caps и bursts:
`aNext = max(aPrevious, phaseCap)` для continuous, `aNext=aPrevious+burstCount`
для burst, aStart=0. Получается **228 ordinary**, а не total cap 200; это
консервативная граница, поскольку фаза может не успеть заполнить свой cap. Ещё
до двух bosses и трёх Travelers отдельно. Удаление из-за смерти только уменьшает a.

## Воспроизводимость и критерии плейтеста

Reference seed 230923; подсистемы используют отдельные seeds, перечисленные в JSON.
Сравнительный набор run seeds: 230923, 230933, 230943, 230953, 230963. Geometry
фиксирована. Для обычного Retry новый seed, resolved seeds фиксируются в report.
Это воспроизводимость конфигурации/решений RNG, не обещание deterministic physics.

Первые проверки после F1-08 на нулевой meta, с обычным draft без принудительной
выдачи компонентов:

1. Движение + камень, намеренно не брать damage upgrades: записать, когда начался
   опасный backlog. Если простое круговое движение стабильно даёт 15:00, пересмотреть
   пересечение ranged/dash угроз и маршрутов, а не вводить обязательный meta урон.
2. Камень + SET-001 и control/sustain по ситуации; отдельно orbit/ice или sky/SET-017.
   Для принятия достижимости получить хотя бы по одной полной победе двумя разными
   билдами на разных seeds, без meta и debug помощи. Два успеха не оценивают win rate.
3. Проверить все пять reference seeds и записать уровень/HP/состав на 2/5/8/13/15
   минутах либо момент смерти. Не выкидывать неудачные runs из отчёта. Рецепт одного
   конкретного сета не должен оказаться обязательным для всех побед.
4. Ранний Traveler, три Travelers, mid+final одновременно, максимальный ordinary
   backlog, 6 concurrent skills и 3–4 доступных сета в stress harness. Это проверки
   крайних случаев, не замена честного draft-прогона.
5. Отдельно записать первые несколько попыток ещё не освоившего билд игрока и
   причины смерти. Если каждый run обрывается одинаково без понятного решения,
   исправлять читаемость/вариативность; если легко проходит с первого раза,
   повышать давление только после оценки опыта тестера и фактического build.

Версия считается подходящей для первой итерации, когда поражения объяснимы
контактом/позиционированием/запоздалой расчисткой, а освоенный zero-meta билд может
победить. Нельзя объявлять её сбалансированной по теоретическому DPS или одному run.
OBS-01 «опыт стреляет» и density gate IP-12A проверяются снова на полном составе.

## Производительность: предлагаемая цель, пока без измерений

Reference machine из read-only hardware query: AMD Ryzen 5 5600H, AMD Radeon(TM)
Graphics, установленная RAM около 16 GB. Honor Virtual Display Device — не целевой
GPU. Предлагается Windows x64 standalone, 1920×1080, 60 Hz; report фиксирует
фактический GPU, render scale, quality, build type и commit. Это локальная цель,
не объявленные минимальные системные требования для игроков.

После прогрева 30 s, в каждом из трёх 60-second окон (середина, финал, stress):
frame-time p95≤16.7 ms, p99≤33.3 ms, без gameplay stall>100 ms. Измерять кадры,
не выводить FPS из PerfGuard warnings; loading/paused время вынести отдельно.
Stress: 250 ordinary (запас над 228), 2 bosses, 3 Travelers, 6 skills L6,
3–4 сета, активные projectiles/XP/potions. За 10 повторов teardown/restart после
прогрева число leased pooled entities возвращается к 0, capacity стабилизируется,
рост managed memory после GC относительно первого прогретого конца ≤5 MB.

Существующий отдельный spawn bound сохраняется: 100 врагов ×10 циклов,
cold≤250 ms / pooled≤50 ms. Дополнительно full scene/run loading≤3 s на reference
machine после прогрева дискового cache. Эти budgets — предложения для F1-09;
здесь никаких runtime/FPS результатов нет. Pool prewarm подбирается по измеренным
пикам, не является gameplay cap и не отбрасывает burst/projectiles при заполнении.

## Матрица полей, источников и реализации

В таблице «новое» — значения этого предложения. Это audit реально существующих
DTO/исполнителей; нельзя копировать review JSON в Resources и ожидать загрузку.
Названные технические/presentation gates принадлежат соответствующим F1 packets.
Новые продуктовые числа ниже не остаются на hidden defaults.

| IDs / группа required fields | Канон / новые значения | DTO или обнаруженное ограничение | Владелец и приёмка |
|---|---|---|---|
| 10 skills: targeting/radius/seed/direction, damage/cooldown/action speed, waves/controls, per-kind payload | Карточки + `skills`, `controls`, `randomness`; neutral semantics выше | `ActiveSkillLevelData`, effect DTO; явные levels или lossless base+changes | F1-01/IP-17: загрузить все 60 строк, проверить level transitions и snapshot damage |
| SKILL-003: continuous collision orbit и per-blade hit history | Новые 120°/s, 0.6 s; continuous — уже канон | Текущий `OrbitEffect` требует finite duration; executor делает дискретные area ticks. Нужен persistent orbit с проверкой пересечения траектории, без stacking от action speed | F1-01/IP-17 + framework owner: нет пропущенных hits между ticks, нет исчезновения/дублирования при upgrade |
| SKILL-004: расширяющаяся волна | Новое expansion 0.25 s; expansion — канон | `AreaEffect` описывает только radius/damage; нужен progressive radius/hit-once contract | F1-01: до фронта урона нет, у фронта ровно один hit на wave |
| SKILL-010: distinct sequential targeting | Новые radius8/spacing0.3; разные цели — канон | Executor уже выбирает разные цели, но все при Schedule; предложение требует snapshot в начале каждого своего telegraph | F1-01: связать deferred targeting с telegraph, 0/1/2/3 валидные цели, death после telegraph |
| PASSIVE-001…005/007…009/011/012: level, modifier channels | Все значения канонические, `passives` | `CharacterStatModifierData`; пропущенные каналы = neutral, при mapping явно заполнить | F1-02/IP-18: все 60 уровней, ratio HP, damage reduction cap99%, resistance cap100% |
| Пять sets: recipe level requirements, typed effects, independent attack | Канонические recipes, новые thresholds/effects в `sets` | `SetEffectData` покрывает StatBuff/SkillTransform/IndependentAttack; не имеет target-slow predicate и existing-orbit aura payload | F1-05/IP-19: добавить типизированные conditional effects для SET-004/010; устранение source, strongest slow, без proc chains |
| SET-017: влияние size/range | Явное включение generic size/range для этой атаки в `sets`; generic damage/KB сохраняются | `ActiveSkillInstance` сейчас принудительно использует size/range=1 для set origin; нужен per-template opt-in, без изменения остальных set attacks | F1-05: размеры/дальность растут один раз; скорость отдельной атаки остаётся 7.5 s |
| CHAR-001: baseline, 17 stats, weights, starting skill, highlights | Канонические HP/направление весов; конкретные веса/скорость в `character` | `CharacterDefinitionData` и base stat mapper, production visual binding отдельно | F1-03/IP-22: нулевая meta, нормализованные derived stats, все weights с locked filtering |
| Draft, XP, rerolls, banishes, Book currency | `draft`, `experience`, `randomness` | `RunSetupConfigData`: seed должен попасть в DraftSettingsData; последний XP threshold повторяется | F1-03/IP-25/26: очереди, exhausted Book=50, reroll/banish consumption и pause |
| 6 ordinary: body stats/size/controls, movement, attack | Карточки → предложенный ребаланс `ordinaryEnemyChanges`; полные значения в `enemies` | `EnemyDefinitionData`, `EnemyMovementProfileData`, `EnemyAttackProfileData` | F1-04/IP-20: all refs resolve, contact/aim timing и ranged lifetime; после approval синхронизировать шесть карточек |
| ENEMY-005: периодическая смена позиции | Новые 2 s distance +1 s lateral | `KeepDistance` игнорирует lateral/cycle; `Orbit` вращается постоянно. Нужен именованный distance/reposition profile | F1-04: не притворяться, что добавление unused JSON fields реализует манёвр |
| Ranged/boss cadence, first delay, aim snapshot | Новая явная конкретизация cooldown/telegraph выше | `EnemyAttackController` сейчас стартует сразу, отсчитывает cooldown после выстрела и обновляет aim в windup. Для предложенной cadence нужен mapping либо явные поля timing policy; silently копировать числа нельзя | F1-04/06: first delay, интервал start-to-start, неподвижная aim marker, не менять прежние fixtures без миграции |
| Bosses: body, hook, phases, attacks, rewards | Базы карточек + `boss`, `midboss` | `BossEncounterData` с `AttackEnemyIds`; production attack payloads не должны требовать импорт чужих ordinary IDs. `TelegraphedDash` делает один dash, не пару | F1-06/IP-21: отдельные attack refs/inline payload по approved contract; two-dash sequence, строгий threshold<50% |
| Travelers: Body, role, presence, wander/rest/avoid/guard, support | Карточки + `travelers`; DTO roles Offensive/Wanderer/Protector | `TravelerDefinition` сейчас запрещает body visual refs; production adapter должен разрешить validated body. Неактивные support effects 0/None, но обязательные positive cooldown/target-count задаются 1/1 и не используются для None/Aura; Aura не ограничена одной целью | F1-07/IP-30: no attacks passive roles, scaling snapshot, XP+одна Book, support cleanup, production body |
| Traveler schedule: pool, probabilities, seed, placement/growth | Общий GDD + `travelerSchedule` | `TravelerScheduleData`; fixed count/type/time rules не заменять удобным scripted schedule | F1-07: 0–3, no repeats, early/late/overlap cases |
| PICKUP-001/002, XP visuals, chance maps, scatter/skin/feedback | Канонические reward/lifetime rules + `pickups` | `PickupCatalogData`: null lifetime = no expiry; explicit empty maps; визуальные ID XP/Book пока fixture-named | F1-04/07: production gameplay IDs с переиспользованием approved imagery, reachable drop, correct heal |
| FIELD-001 metadata/environment/obstacles | Канонический текст/сложность; `field` 64 rects | `FieldData`, `FieldEnvironmentData` SceneName/SpawnPointName/ObstacleNames требуют actual production scene binding | F1-08/IP-23: proposed names Gameplay/PlayerSpawn/FIELD-001-O01…O64 проверить при сборке сцены; thumbnail asset gate |
| Timeline phases/modifiers/hooks/seed/spawnRadius | `timeline`, `field.spawnRadius`, `randomness` | `WaveTimelineData`; burst mode sequential, no overlap/deferred burst | F1-08/IP-24: ровно900 s, все6 IDs, mid450/final810, cap и terminal ordering |
| Save mapping/unlocks | DECISION-0050 без изменения условий | Production metadata ещё содержит старый initial mapping | F1-03: migrate metadata, fixture separation, idempotent first clear; late gameplay не входит |
| VisualId/MotionProfileId, body contacts, icons/thumbnail/presentation | Manifest snapshot ниже; не выдуманные body radii | Required visual refs проверяются после import/authoring. CollisionSize карточки не равен автоматическому contact radius | В каждом owning packet: binding только approved assets; contact circle и 1080p/density review |

Нейтральные параметры неиспользуемых movement/attack kinds не должны менять смысл
профиля. Точные новые DTO имена и сериализация — реализация владельца с контрактными
тестами, не часть balance proposal. Числа выше остаются content data, не C# constants.
Если adapter не способен выразить approved поведение, исправляется adapter;
замена на похожую fixture механику требует отдельного design решения.

## Уже имеющийся арт и остающиеся visual gates

Read-only snapshot [asset manifest](../../Art/asset-manifest.json) — 40 подходящих
записей в `artInventory` JSON, с owner/role/stage/runtime path/missing. Никаких
изображений этим пакетом не создано, не изменено и не переутверждено.

| Область | Можно переиспользовать | Что ещё проверить/подготовить |
|---|---|---|
| Все 10 active, 10 passive, 5 sets | 25 icons, Image approved | Production binding и оставшиеся замечания actual-slot readability из manifest |
| Skills world | SKILL-001/003/006/014 projectile sprites, Image approved | World presentation 002/004/005/007/010/013 и пяти set effects; existing art не генерировать заново |
| Персонаж | CHAR-001 body, Image approved | Production visual ID/crop/binding; принятый body circle не пересчитывать по balance size |
| Ordinary | ENEMY-002 body Image approved; ENEMY-001 body Imported | ENEMY-001 approval/evidence gate отдельно; 003/004/005/007 body, sling/arrow art и contact authoring |
| Bosses/Travelers | Нет selected body entries для этих пяти IDs | Body/telegraph/attack presentation у F1-06/07 |
| Pickups | XP, PICKUP-001, Traveler Book images, Image approved | Связать Book с PICKUP-002, сохранить provenance; fixture visual ID не означает fixture gameplay data |
| FIELD-001 | Ground/fence/stump/bush/grass, Image approved | Scene/layout/collision alignment и thumbnail; decor не добавляет скрытой collision |

## Принятие и дальнейшая настройка

Review охватывает именно эту v1 вместе с JSON: numeric fills, thresholds, timing,
предложенные явные semantics там, где карточка их не конкретизировала. Approved
цель сложности уже действует; approval прежних карточек не выдаётся повторно.
По [BALANCE_WORKFLOW](../implementation/BALANCE_WORKFLOW.md#численные-и-механические-изменения)
новая конкретизация TBD сначала проходит approval. После него значения переносятся
в production DTO/JSON соответствующими F1 packets с их проверками. Art gates
закрываются отдельно. F1-01 автоматически этим документом не запускается.

Для первого tuning pass безопасный небольшой шаг: spawn intervals ±10%, XP costs
±10% с сохранением монотонности, potion chance в 1–2%, heal 15–22, set threshold
±1 уровень в пределах 1…6, setDraftChance 0.4–0.6, telegraph 0.45–0.8 s. Это диапазоны
для следующего предложения, не разрешение runtime randomization или молчаливого
тюнинга. Менять по одной причине за итерацию, сравнивать одинаковые seeds/zero-meta,
фиксировать реальные reports. Откат — возврат принятой версии content values;
сейчас runtime не изменён, поэтому runtime rollback не требуется.
