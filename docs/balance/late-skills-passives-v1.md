# Поздние умения и пассивки — данные v1

Ревизия: `late-skills-passives-v1`, 2026-09-26. **Approved 2026-09-26** пользователем
([DECISION-0060](../decisions/0060-late-skills-passives-data-v1.md)).
Подготовлено по поручению пользователя 2026-09-26 («да, давай») на предложение собрать
пакет данных для IP-17/IP-18 по образцу F1-00. Это данные для approval, не production JSON
и не проверенный в игре баланс. Текущее исполнение и очередь — только
[STATUS](../implementation/STATUS.md).

Состав: SKILL-008/009/011/012/015/016 (остаток IP-17) и PASSIVE-006/010/013/014 (остаток IP-18).
Поведение, targeting, L2…L6 прибавки и все числа карточек взяты из
[Content Design](../Content_design.md) без изменений; здесь добавлены только
отсутствующие в карточках параметры (дистанции, скорости, hit size, lifetime, trigger radius,
задержки второй волны). Полные шесть строк каждого ID — в
[машиночитаемой таблице](late-skills-passives-v1.json) (формат `late-content-review-data`,
не DTO игры; `shared` действует на все уровни, строка уровня — итоговые значения).
Проверка: `python -X utf8 docs/balance/validate_late_skills_passives.py`.

## Общие правила (как в baseline v1)

Правила [baseline v1](field001-baseline-v1.md#активные-умения-дополненные-параметры)
действуют без изменений: проценты уровней складываются к базе
([DECISION-0021](../decisions/0021-additive-skill-level-bonuses.md)); для снаряда
`lifetime = range / speed`; внешний range bonus растит только lifetime (у затухающего
снаряда — время до остановки); `maxHitTargets = n` → `pierceCount = n − 1`; nonzero
knockback duration 0.12 s; цели только на видимом экране, без цели направленный навык
бьёт в предыдущем направлении ([DECISION-0058](../decisions/0058-on-screen-targeting-and-strike-visual.md)).
Применение size/range пассивок к каждому семейству — по
[таблице IP-08](../implementation/modules/IP-08-active-skill-framework.md#контракт-параметров-для-потребителей).
Все шесть умений реализуются существующими семействами framework; новых механик не требуется.

## Дополнения к карточкам умений

Расстояния в world units, время в running seconds.

| ID | Семейство | Дополнение к L1 | Существенные границы |
|---|---|---|---|
| SKILL-008 Рикошетный диск | Projectile + ricochet | targeting 6; hits L1–L3: 2/3/4; speed 9; radius 0.20; ricochet search 3; путь L1–L3: 9/12/15 | L4/L6: 6/8 попаданий; бюджет пути `6 + 3 × ricochet`; ricochet не перезапускает lifetime; повтор цели только если другой валидной нет; урон без затухания |
| SKILL-009 Магматическая мина | Mine | blast radius L1–L3: 0.75/1.5/1.875; trigger radius 0.8; lifetime 6 s; взрыв и при истечении | L4 lifetime 7.2 s, cap 6; при превышении cap исчезает старейшая; L6 второй взрыв через 0.4 s: 0.75 radius, 0.6 damage, 0.7 KB |
| SKILL-011 Спираль осколков | Projectile burst, Ring | count L1–L3: 4/7/10; range 4.5; speed 8; radius 0.12; поворот 15° за активацию | L4 12 осколков, speed 9.6; L6 вторая очередь через 0.15 s, повёрнута на половину углового шага; без пробивания |
| SKILL-012 Пульсирующий луч | Beam | duration L1–L3: 0.4/0.8/1.1 s; targeting 6; length 6; width 0.26 (было 0.40, [DECISION-0144](../decisions/0144-pulse-beam-follows-target.md)); tick 0.2 s | Поражает всех на линии; L4 length 7.2, width 0.403; L6 1.5 s, width 0.442; KB за tick от игрока |
| SKILL-015 Крест клинков | Projectile burst, Cross | range L1–L3: 2/4/5.2; speed 5 (было 10, [DECISION-0143](../decisions/0143-blade-cross-slower-waves.md)); half-width 0.30; оси от 0° | Неограниченное пробивание, одна волна бьёт цель один раз; L4 8 волн, range 5.8; L6 второй крест через 0.35 s, поворот 22.5°, полный damage/KB |
| SKILL-016 Разбрасыватель мусора | Decelerating projectile | stop/path L1–L3: 0.7 s/2.8, 1.4 s/5.6, 1.68 s/6.72; radius 0.12 | Исчезает при остановке; L3 2 снаряда; L4 скорость 9.6, путь 8.06, пробивает 1; L6 3 снаряда, radius 0.168 |

Выбор значений: targeting 6 совпадает с остальными ближними nearest-навыками стартового
набора; скорости и hit size — в диапазоне камня/игл/льда, чтобы снаряды читались одинаково.
Диску дан отдельный бюджет пути, потому что правило baseline «ricochet не перезапускает
lifetime» иначе не даёт сделать 8 попаданий. Мина живёт 6 s — примерно два cooldown, так что
L1 cap 4 заполняется только при долгом отсутствии врагов. Поворот спирали 15° повторяет
рисунок каждые 3 активации при шаге 45°.

Ориентир урона L1 по одной цели за секунду (без пассивок и сетов): диск 9, мина 13
(по площади), спираль 5, луч 13 (4 tick, всем на линии), крест 9, мусор 11 (случайно).
У стартовых: камень 17, копьё 14. Поздние навыки сильнее за счёт покрытия, а не
урона в одну цель; это проверяется реальным прогоном, а не таблицей.

## Пассивки

Все 24 строки перенесены из карточек без ребаланса, каждая — итоговое значение уровня.
Каналы уже есть во framework ([IP-09](../implementation/modules/IP-09-passive-framework.md#stat-applicability-и-владельцы-defaults)).

| ID | Канал JSON | L1…L6 | Граница |
|---|---|---|---|
| PASSIVE-006 Эхо памяти | `disappearingXpRecoveryBonus` | 0.10 … 0.60 | База CHAR-001 0; сумма ограничена 1; истёкший опыт начисляется один раз |
| PASSIVE-010 Талисман ученика | `pickedUpXpMultiplierBonus` | 0.05 … 0.30 | Только физический подбор; recovery PASSIVE-006 не усиливает |
| PASSIVE-013 Длинные руки | `effectRangeMultiplierBonus` | 0.08 … 0.50 | Не меняет width/radius/size; маппинг по таблице IP-08 |
| PASSIVE-014 Упрямство | `lowHealthDamageMaxBonus` | 0.15 … 0.70 | Множитель `1 + max × min(1, (1 − HP/maxHP) / 0.9)`, фиксируется при активации |

## Draft и доступ

Веса Клёпки: SKILL-009 0.70 (как в baseline v1), остальные девять ID — 1.
Unlock не меняется ([DECISION-0050](../decisions/0050-starting-content-and-unlocks.md)):
SKILL-008 и PASSIVE-013 — после прохождения FIELD-001; SKILL-009/016 и PASSIVE-006/010 —
FIELD-002; SKILL-012 и PASSIVE-014 — FIELD-003; SKILL-011/015 — FIELD-004. Сейчас в
игре есть только FIELD-001, поэтому реально попадут в draft только SKILL-008 и PASSIVE-013;
остальные будут реализованы и проверены тестами, но закрыты до появления своих полей.

## Арт

Иконки всех десяти ID утверждены и импортированы
([DECISION-0047](../decisions/0047-skill-icon-fixture-mapping.md),
[DECISION-0048](../decisions/0048-passive-and-set-icon-fixture-mapping.md)).
World sprite есть только у диска SKILL-008. Для мины, осколков спирали, луча, волн креста
и предметов мусора нужен отдельный art packet; без него production binding этих пяти
умений не заявляется (fixture fallback запрещён контрактом IP-17).

## После approval

1. Реализация: production definitions, иконки и unlock в JSON; per-ID тесты L1…L6 (IP-17/IP-18).
2. Отдельно: art packet для world visuals SKILL-009/011/012/015/016.
3. Балансные правки после плейтеста — через [BALANCE_WORKFLOW](../implementation/BALANCE_WORKFLOW.md).
