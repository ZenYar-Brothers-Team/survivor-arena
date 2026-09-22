# Design sync R2: различия, пробелы и владельцы решений

G-06…G-09 закрыты ответом пользователя с поправками: [DECISION-0017](../decisions/0017-combat-control-semantics.md), [итоговая таблица](proposals/2026-09-20-combat-gap-decisions.md). Knockback добавляется к движению/dash; low-HP damage фиксируется при активации. Реализация и evidence — только STATUS.

Дата: 2026-09-20. План принят пользователем и зарегистрирован по DECISION-0015. Сравнение ниже фиксирует исходный аудит старой реализации относительно принятого дизайна; таблица G-01…G-18/W-01 остаётся реестром конкретных незаполненных вопросов. Оперативные статусы исполнения находятся только в STATUS.

Перед IP-07 подготовлено [предложение G-01/G-03](proposals/2026-09-20-draft-gap-decisions.md). Оно ожидает ответа пользователя; short/empty/Book/queue rules ещё не закрыты.

## 1. Уточнённая исходная договорённость

Пользователь явно подтвердил: **все пять новых документов уже утверждены**. Game Design v2, Content Design v2 и Art Direction v2 **полностью заменили** содержимое соответствующих канонических файлов. По последней поправке пользователя отдельные копии старых трёх документов не создаются. Поэтому ниже новые документы — утверждённый целевой дизайн, а прежние канонические файлы и репозиторий — база текущей реализации. Повторное одобрение уже описанных правил, карточек или общего художественного направления не требуется.

Утверждение документа не создаёт отсутствующее число, не разрешает выбрать произвольный вариант внутреннего противоречия и не означает, что будущая ещё не созданная картинка принята. Три разных факта учитываются отдельно: **design approved**, **implementation data complete**, **implemented/verified**. Первый подтверждён пользователем; второй проверяется по использованным полям конкретной карточки; третий остаётся в `STATUS.md`. IP зарегистрированы в принятом плане; это не означает реализации нового поведения.

Источники из коммита `a2dd22a8eb61fb851610a94e09533c017277b287` автора `zhenia966` (`new docs`, 2026-09-19):

| Утверждённый источник (имя при импорте) | Действующий канонический путь / роль |
|---|---|
| [Game Design v2](../Game_design.md) | Полная замена `docs/Game_design.md` |
| [Content Design v2](../Content_design.md) | Полная замена `docs/Content_design.md` |
| [Art Direction v2](../art/ART_DIRECTION.md) | Полная замена `docs/art/ART_DIRECTION.md` |
| [UI / UX Design](../UI%20%20UX%20Design.md) | Новый утверждённый UX-контракт; раньше scope распределён между IP-10A, DECISION-0005 и будущим IP-26 |
| [Art Production](../art/Art%20Production.md) | Новый утверждённый inventory; дополняет `ASSET_PIPELINE.md`, не заменяет его |

Сравнение старого дизайна относится к базе `d9a1970`, нового — к пяти входным файлам коммита `a2dd22a8eb61fb851610a94e09533c017277b287`. Номера строк ниже — исторические координаты этих источников до нормализации Markdown; текущие файлы не обязаны совпадать с ними. Для работы IP используют разделы и content IDs.

## 2. Различия правил Game Design

| Область | Текущая база | Утверждённый target и работа в плане |
|---|---|---|
| Playable roster | Только маленькие гоблины, `Game_design.md:11,15,122` | Монстры разных видов и размеров, текущий roster преимущественно гоблинский, CHAR-006 — огр; `Game Design v2.md:11,15`, раздел «Персонажи». Контент/арт IP-22/IP-12A, без автоматического изменения collider |
| Спавн волн | Непрерывный, `Game_design.md`, «Структура забега…», «Враги…» | Continuous или burst, `Game Design v2.md:37,116`; расширение Wave Director IP-14, production schedules IP-24 |
| Knockback | Общего контракта нет | Дистанция в world units, явный source knockback, outgoing bonus, resistance 0–100% у игрока/врагов; `Game Design v2.md:54–57`; IP-03/IP-05/IP-09/IP-13 |
| Slow | Не определён; прежний SKILL-013 прямо исключал slow | Movement-only, strongest-wins, повторное наложение обновляет/продлевает duration; `Game Design v2.md:58–60`; IP-03/IP-05/IP-09/IP-13 |
| Healing pickup | Разрешены healing/regeneration, конкретный drop не определён | Potion от обычных врагов, configurable chance/heal, restoration modifiers и max-HP cap; v2:63; framework IP-28, production PICKUP-001 и drops IP-20 |
| XP pickup radius | Физический подбор; runtime уже использует base radius | Явный настраиваемый radius и его passive amplification, v2:68; stats/mapping IP-03/IP-09, существующие и новые drops IP-06, production IP-18 |
| Draft | Число предложений TBD | Три слота, v2:78; Book открывает дополнительный draft без XP/level, v2:73; IP-07/IP-10/IP-11/IP-28 |
| Empty eligible pool | Level сохраняется, окно пропускается, пауза снимается; `Game_design.md:27,63` | Обе оговорки отсутствуют в v2:27,72. Это потерянный при замене edge case, а не указание удалить работающую защиту. Требуется отдельный явный delta к target, см. G-01 |
| Haste | `base cooldown / (1 + sum reduction)`; `Game_design.md:77`, DECISION-0004 | Та же формула названа action speed; +20% frequency → 83.33% duration, `Game Design v2.md:88`. IP-03/IP-05/IP-09/IP-13 мигрирует термины/контракты без случайной смены математики |
| Sets | Дополнительные независимые способности; unspecified recipe size и per-set probability | 3–6 компонентов, единый independent `setDraftChance`, прошедшие сеты заполняют слоты первыми, при избытке берутся первые три по processing order; v2:91–110. IP-07/IP-10/IP-11 меняет выбор, IP-11 расширяет эффекты |
| Set effects | Framework extra ability | Passive buffs, component transforms, conditional synergy, economy/defense, редкие set-attacks; запрет рекурсивных set procs, fixed cooldown set-attacks вне action speed; v2:103–110; IP-11 |
| Travelers | Нет | Три роли, 0–3 на run по discrete probabilities, независимые uniform times 0:00–15:00, overlap допустим, lifetime/scaling, Book и offscreen arrows; v2:124–134; IP-29/IP-30 |
| Content readiness | Каталоги считались будущим content design | Конкретные каталоги утверждены, но schedules, timings, экономика и отдельные используемые числа остаются незаполненными; v2:180–190. Gate относится к отсутствующему значению, а не к повторному approval каталога |

Сохранённые инварианты: 15 минут и победа живого игрока по таймеру без обязательного убийства final boss; auto attacks и управление перемещением; camera follow без задержки; pause-aware contact damage; 6+6 slots и шесть уровней; additive percentage modifiers; incoming reduction cap 99%; сохранение доли HP при max-HP rescale; игрок-only collision обычных границ/obstacles; отсутствие уникальных character passives; optional midboss. Они не должны исчезнуть как побочный эффект миграции или художественной интеграции.

## 3. Полный охват Content Design

### 3.1. Количество и статус

В прежнем `Content_design.md` 93 карточки/записи имеют Draft-статус, прямо или на уровне раздела. Утверждённый target содержит **121 запись**; отсутствие индивидуального повторения статуса не отменяет approval раздела. Это production scope плана, а не список, который нужно снова предлагать пользователю на принятие. Поставка может идти по поднаборам, но остаток каталога нельзя молча исключить.

| Категория | Прежний каталог | Утверждённый target | Владельцы |
|---|---:|---:|---|
| Active skills | SKILL-001…015, 15 | SKILL-001…016, 16 | IP-08 → IP-17; combat/stat dependencies IP-03/IP-05/IP-09/IP-13 |
| Passives | PASSIVE-001…010, 10 | PASSIVE-001…014, 14 | IP-03/IP-05/IP-09/IP-13/IP-28 → IP-18 |
| Sets | SET-001…008, 8 | SET-001…020, 20 | IP-07/IP-10/IP-11 → IP-19 |
| Regular enemies | ENEMY-001…020, 20 | Те же 20 | IP-20 с IP-03/IP-05/IP-09/IP-13/IP-28 |
| Final bosses | BOSS-001…010, 10 | Те же 10 | IP-15/IP-21 |
| Midbosses | MIDBOSS-001…010, 10 | Те же 10 | IP-15/IP-21 |
| Characters | CHAR-001…010, 10 | Те же 10 | IP-22 |
| Fields | FIELD-001…010, 10 | Те же 10 | IP-16/IP-23 |
| Travelers | Нет | TRAVELER-001…010, 10 | IP-29/IP-30 |
| World pickup cards | Нет | PICKUP-001, 1 | Framework IP-28; production potion IP-20; Book card/reward links IP-30 |

Book описана как утверждённая механика, но отдельного ID/card у неё пока нет; в число 121 она не добавлена. `Wave / Encounter Content` остаётся пустым (`Content Design v2.md:1520`); production wave IDs/schedules отсутствуют. Meta upgrade/reward/price definitions также не предоставлены.

### 3.2. Active skills: поведение, а не только численные правки

Источник: прежние карточки `Content_design.md:89–209`; новые `Content Design v2.md:94–254`. Все 16 используют явный knockback, compound level progression и обновлённые связи с сетами. В target L2–L6 накапливаются; это отличается от passive values, которые являются итоговыми значениями уровня.

| ID | Существенный delta |
|---|---|
| SKILL-001 | «Искровой болт» → «Бросок камня»; L4 pierce заменён ricochet; heavy impact/size/knockback |
| SKILL-002 | Сохранён fan; обновлены range/speed/size/knockback, pierce и накопление уровней |
| SKILL-003 | Явный blade hitbox radius, его scaling отдельно от orbit radius; rotation speed и knockback |
| SKILL-004 | 28 → 8 base damage; knockback становится основным эффектом; вторичная волна получает отдельные radius/damage/knockback ratios |
| SKILL-005 | Nearest-enemy aim → направление движения, при остановке последнее ненулевое направление; direction фиксируется при запуске |
| SKILL-006 | Вместо once-per-outward/return-pass — повторные попадания через единый per-target cooldown 1 s, общий для всех существующих boomerangs этого skill; return damage/knockback modifiers |
| SKILL-007 | Последовательный chain от последней цели, nearest unhit в jump range; по уровням меняются falloff и range |
| SKILL-008 | Ricochet сохраняется; новые speed/size/knockback values. Return phase по-прежнему не описана, хотя её использует SET-002 |
| SKILL-009 | Изменены lifetime, second-explosion radius/damage/knockback и уровни; старейшая мина исчезает при превышении cap |
| SKILL-010 | Выбор случайного valid enemy в world targeting radius вместо ближайших; экран не определяет доступность; target position snapshot для telegraph |
| SKILL-011 | Расширены range/speed/size/knockback и двойная круговая очередь с half-step offset |
| SKILL-012 | Tick knockback, ширина/дальность/duration; поздний tracking луча требует точного runtime поведения |
| SKILL-013 | Три крупных burst shards без status → семь мелких с 20% slow на 1.5 s; плотный fan/уровни/пирсинг переписаны |
| SKILL-014 | Nearest aim → независимое random direction каждой сферы; impact/explosion имеют разные knockback и компоненты damage |
| SKILL-015 | Ориентация креста на врага → фиксированные горизонталь/вертикаль; затем диагонали и повтор с поворотом |
| SKILL-016 | Новое: random non-targeted trash projectiles, linear deceleration до нуля, despawn при остановке; pierce, count и size upgrades |

IP-08 перерабатывается на месте: проверенная прежняя ревизия не доказывает movement/random targeting, shared boomerang hit ledger, deceleration и новые effect semantics. Перед IP-17 нужно реализовать и проверить целевую ревизию IP-08; отдельного позднего skill-extension IP не будет.

### 3.3. Passives и stats

Источник: `Content_design.md:213–293` → `Content Design v2.md:258–366`.

- PASSIVE-001/003/004/006/008/009/010 сохраняют основные эффекты; изменены связи с сетами и статус. PASSIVE-004 по-прежнему описывает active-skill damage, тогда как generic player damage у set-attacks требует явной applicability.
- PASSIVE-002 «Живая ткань» → «Собиратель»: прежняя regeneration плюс относительный bonus potion drop chance; пример 5% × 1.6 = 8% уже задан и не нуждается в повторном approval.
- PASSIVE-005: новая action-speed терминология и denominator formula; процент «−25% cooldown» из старой карточки нельзя механически прочитать как duration × 0.75.
- PASSIVE-007: lifetime «Консервант опыта» полностью заменён pickup-radius «Магнит опыта» под прежним ID. Это semantic migration; старые записи баланса/сейвы нельзя считать эквивалентными.
- PASSIVE-011: incoming knockback resistance и outgoing knockback; PASSIVE-012: effect size/width/radius без travel/target range; PASSIVE-013: effective range без size; PASSIVE-014: динамический damage при низком HP, максимум при 10% HP и ниже.

### 3.4. Sets: замена прежних восьми и расширение до двадцати

Источник: `Content_design.md:297–411` → `Content Design v2.md:369–563`. SET-001…008 используются для новых рецептов/эффектов, а не продолжают старые карточки без изменений. Например, старый SET-001 «Грозовая сеть» нельзя приписать новому SET-001 «Тяжёлый боезапас»; схожее название «Перегрузка сети» теперь у SET-013 с другой механикой.

| ID / target | Семейство эффекта и обязательный runtime |
|---|---|
| SET-001 Тяжёлый боезапас | Targeted buff rock damage/size/knockback |
| SET-002 Возвратный ритм | Return-phase modifiers boomerang/disc; открытый conflict disc return |
| SET-003 Грозовой проводник | Chain max targets/range/falloff |
| SET-004 Ледяной таран | Conditional outgoing knockback по slowed target + wave modifiers |
| SET-005 Жадность к знаниям | XP/recovery/radius buffs + heal при настоящем level-up |
| SET-006 Полевой медик | Max HP/regeneration/restoration/potion drop buffs |
| SET-007 Векторный шторм | Size/range/damage buffs трёх component skills |
| SET-008 Утилизатор | Каждый N-й trash projectile заменяется heavy variant, взрывающимся при остановке; один counter |
| SET-009 Линия пробоя | Damage/range/width/pierce линейных skills |
| SET-010 Холодная орбита | Slow в текущем orbit radius и damage bonus по slowed enemies |
| SET-011 Кинетический арсенал | Component projectile speed/damage/range/knockback |
| SET-012 Неподвижная крепость | Defensive stats + wave size/knockback, без reactive-damage proc |
| SET-013 Перегрузка сети | Редкий attack: ближайшая A → одновременные secondary branches, без дальнейшей цепочки |
| SET-014 Танец клинков | Постоянные component damage/action speed/size buffs; отдельного rhythm cycle нет |
| SET-015 Алхимия хаоса | Explosive component buffs + potion-pickup explosion с внутренним cooldown |
| SET-016 Выстрел великана | Редкий большой bolt по movement direction |
| SET-017 Падающая звезда | Random world target → telegraph → один сильный area strike |
| SET-018 Сфера разрушения | Random sphere/explosion, не более одной существующей сферы |
| SET-019 Ледяное копьё | Movement-direction pierce/slow/knockback shard |
| SET-020 Каменное ядро | Random heavy boulder, не более одного существующего |

Утверждены 3–6 компонентов, типы эффектов, fixed set-attack cooldown и запрет recursive procs. **Все** target set cards всё ещё требуют точных effect values и minimum component levels перед production implementation. Старые thresholds/числа не заполняют новые рецепты автоматически. IP-11 должен обеспечить provenance источников damage/control/proc, component-only applicability, cleanup и независимость наборов; IP-19 создаёт все двадцать definitions.

### 3.5. Враги, боссы, персонажи, поля и новые сущности

- **ENEMY-001…020:** прежние HP/speed/contact damage/behavior/XP/field contexts сохранены; добавлены явные knockback/resistance. Target `574–918`; повторный contact interval всё ещё TBD (`576`). Approved labels не означают, что отсутствующий интервал уже задан.
- **BOSS-001…010 и MIDBOSS-001…010:** прежние основные stats/patterns сохранены, добавлен knockback/resistance; same-number mapping к FIELD-001…010 теперь утверждён (`922–1207`). Exact attack/movement/timing/rewards остаются неполными. Midboss optional, наличие карточки не обязывает спавнить его в каждом schedule.
- **CHAR-001…010:** прежние numeric profiles, start-skill IDs, qualitative weight lists и unlock conditions сохранены, дополнены различимые возраст/пол/силуэт/архетипы (`1304–1404`). Шепоток → Шепотка; Шмыга → Тётка Шмыга; Искра → Бабка Искра; Юла → Дед Вертун; Светляк → Тётушка Светляк; Ночка → Старшой Ночка. Гром становится огром-беглецом. Визуальная масса не определяет collider.
- **FIELD-001…010:** geometry/visual themes/pressure/unlocks сохранены; заполнены final/midboss references (`1406–1520`). Реальные geometry placements, enemy pools и schedules ещё предстоит задать.
- **TRAVELER-001…010:** новые карточки (`1211–1302`). Боевые: 001/004/008/010; неагрессивные: 002/003/006; защитники: 005/007/009. Последним нужны support contracts: damage-reduction positioning, temporary shields, resistance/reduction aura. Это не просто новые значения HP для обычного преследователя.
- **PICKUP-001:** potion approved design / numbers TBD (`565–570`); источник normal enemy death, contact pickup, restoration modifier, no XP/draft. Book — отдельный payload и lifecycle, не вариант XP drop с фиктивным level-up.

## 4. UI / UX: утверждённый новый scope

Источник — `UI  UX Design.md`, разделы 1–23. UI не владеет gameplay state; существующий View → presenter → model/runtime boundary сохраняется.

| Target requirement | Отличие от текущего IP / owner |
|---|---|
| Main Menu → character/field selection → run → results → immediate Retry/Main Menu | Полный player flow в IP-26; selection по IP-12/IP-16/IP-22/IP-23; Retry сразу с теми же character/field уже утверждён (§16, строка 533), повторного подтверждения не добавлять |
| Timer сверху; final-boss-only HP; HP/XP/level снизу; компактные 6+6 и set icons | IP-10A + vertical slices IP-15/IP-29; текущий fixture/debug layout не является окончательным UX (§6) |
| Три draft cards, реальные numeric deltas, related set progress, closest recipes, full detail on hover/panel | IP-07/IP-10/IP-11/IP-10A; thresholds учитываются, possession не равняется выполнению recipe (§§7–9) |
| Completed recipe ≠ acquired set; отдельная set card; выбор без confirmation | IP-07/IP-10/IP-11/IP-10A; UI не активирует сет сам при завершении рецепта |
| Full-screen Pause/Build, stats/build/acquired sets и только recipes с progress | Player pause overlay не ограничен размером dev drawer; не добавлять обязательный full compendium (§10) |
| Traveler HP над каждым Traveler независимо от роли, offscreen arrows, без точного escape countdown в MVP | IP-29; поддержать одновременных Travelers (§11) |
| Book draft переиспользует cards, имеет собственный heading/accent и не показывает level-up presentation | IP-07/IP-10/IP-11/IP-28/IP-10A (§12) |
| Final boss bar/name, таймер главный, без intro cutscene | IP-15/IP-21 (§13). Не делать обязательную HP-полосу midboss в top boss area |
| Короткие неблокирующие notifications, ограничение перекрытия центра | IP-10A и события от владельцев механик (§14) |
| Compact Results, top-three damage только если данные доступны | IP-26/IP-31 (§16); не создавать advanced analytics MVP |
| Простые upgrades/unlock cards и цены/conditions | IP-25/IP-26 (§17); отсутствие economy values не разрешает выдумать цены |
| Audio volumes, resolution/fullscreen, screen-shake toggle, показ movement keys | IP-26 (§18); audio/settings ранее явно вне узкого IP-26 scope. Remap/localization не обязательны первой версии |
| Reusable states и UI shapes/text прежде generated decoration | IP-10A/IP-12A (§§19–23); final icon bindings производят владельцы контента |

Обязательные review points: empty/short pool vs 3-card layout; порядок queued drafts вместо безусловного resume после клика; Quit Run и завершение Results transition пока неполны; экранный shake не должен нарушать approved centered-camera gameplay contract. Последнее требует отдельного visual-offset контракта, а не задержки follow. Отсутствие wave label в списке target HUD не отменяет debug wave observability DECISION-0014: финальное расположение player wave indication нужно явно зафиксировать при UI reconciliation.

## 5. Art Direction и Art Production

### 5.1. Art Direction: полный replacement при сохранении совместимых основ

`Art Direction v2.md` сохраняет storybook cutout, 3/4 top-down, strong silhouette, flat matte/cel shading, procedural-ready pose, 1920×1080 reference, master/runtime size guidance, palette/value hierarchy, no gore, no baked ground shadows и generation/review contract. Существенные дополнения:

- Sections 2/7: разнообразные playable-монстры, явное исключение CHAR-006 для массы/пропорций; возраст/пол/осанка/архетипы должны считываться независимо от статистики.
- Section 9.2: зелёная skin family больше не обязательна для всех future species; цель — различимость от фона/enemies. Это не разрешение потерять читаемость текущих зелёных гоблинов.
- Внутреннее противоречие target: §4.4 всё ещё требует «всегда … зелёный силуэт», тогда как §9.2 разрешает негоблинские palettes. Нужна небольшая явная поправка §4.4, например ограничить green текущей goblin family и сохранить общую role/readability иерархию; сам новый roster уже утверждён.
- Section 12.1 (строки 251–257): sets преимущественно изменяют existing VFX, отдельные attacks редкие и читаемые; combined check 3–4 активных sets. Это утверждённый visual requirement, не необязательная рекомендация в конце разработки.
- Section 17 остаётся fixture vertical slice; он сам по себе не превращается в production CHAR-001.

`ASSET_PIPELINE.md` остаётся техническим источником: source/master вне Assets, approved runtime derivative, stable path + `.meta` GUID, provenance, PNG/import checks, explicit image approval. Отдельный approval уже принятого CHAR-001 concept не запрашивается повторно; сначала нужно установить **какому конкретно существующему файлу** соответствует запись. Approval concept не доказывает runtime binding.

### 5.2. Inventory не заменяет execution status и repository evidence

`Art Production.md:20–28` различает NOT STARTED / GENERATED / REVIEW / APPROVED / IN GAME. Это состояния **ассета**, а не второе место хранения IP execution status. CHAR-001 body отмечен APPROVED с требованием проверить integration отдельно (`108`); многие generic effects стоят NOT STARTED, хотя fixture presentation уже существует. Перед изменением asset status нужен manifest/runtime/provenance audit, без автоматического сброса Verified IP-12A или объявления production art готовым.

Охват production: 10 playable bodies и reuse/crop для selection; 20 enemy bodies; 10 final bosses; 10 midbosses; 10 Travelers; assets реализуемых 16 skills; 16 skill icons, 14 passive icons, 20 set icons; potion/Book/XP; generic VFX; 10 field kits; только необходимые утверждённой геометрии obstacle/decor packs. Generated art, procedural-only и hybrid различаются; UI bars/cards/notifications/settings сначала делаются shapes/text, portraits и reroll/banish icons не обязательны отдельной генерацией.

Phase A/B/C (`Art Production.md:546–573`) задают поставку по реально доступному playable content, а не generation всего каталога сразу. Полный backlog остаётся в scope. Marketing/store art (§16) — отдельный поздний слой, вне первого playable build.

Словосочетание «knockback motion» в procedural-only checklist (§15) не разрешает реализовать gameplay knockback одним VisualRoot. IP-03/IP-05/IP-09/IP-13 владеет authoritative displacement; IP-12A — реакцией sprite. Аналогично projectile travel/orbit/telegraph timing происходят по gameplay contract, а арт отображает их. Все renderer motions используют VisualRoot/single writer, pause и pool-baseline restoration по DECISION-0013.

## 6. Реальные незаполненные вопросы и внутренние противоречия

Это список **добавочных решений/данных**, а не повторный запрос принять пять документов. Утверждённые части можно реализовывать по плану без ожидания всех остальных ответов; блокируется только зависимая ветка. Числа предлагаются в reviewable balance packet IP-32 и применяются после конкретного approval, как попросил пользователь.

| Gap | Что уже решено / чего недостаёт | Владелец / проверяемое закрытие |
|---|---|---|
| G-01 Empty/short draft | **Resolved — DECISION-0019/0020.** Uniform set backfill; 1–2 cards + inactive slots. Ordinary empty сохраняет XP/level без валюты; только пустая Книга при подборе немедленно даёт валюту | IP-07/IP-10/IP-11; 0/1/2 options только после дозаполнения, only-sets failed roll; empty reward lifecycle |
| G-02 Draft set selection | Independent global chance и set-first утверждены; **uniform backfill утверждён DECISION-0019**, в том числе при chance=0. Book ordinary pool/shared controls утверждены DECISION-0020. **Resolved — DECISION-0022:** ordinal content ID order, reroll повторяет checks, banish сохраняет результаты остальных сетов | IP-07/IP-10/IP-11; deterministic examples, uniform backfill без повторов, pool/origin policy; обычные веса не применяются к дозаполнению |
| G-03 Book lifecycle | Extra draft без XP/level утверждён; [предложение](proposals/2026-09-20-draft-gap-decisions.md) описывает ordinary pool/consume/FIFO/terminal contract. **Draft contract resolved — DECISION-0020:** ordinary pool/shared controls, immediate empty-at-pickup currency, FIFO/revisions/terminal cancellation; production ID/card/lifetime остаются отдельным gate | IP-07/IP-10/IP-11/IP-28 — reason/queue/pause contract; IP-30 — production Book card/ID и полные параметры |
| G-04 SET-002 | `Content Design v2.md:393–400` усиливает return disc, но SKILL-008:166–174 не содержит return phase | IP-11/IP-08; решить только эту недостающую семантику до реализации затронутого set |
| G-05 SET-015 / trash | SET-015:510–518 бафает explosion у SKILL-016, у которого base explosion нет. SET-008 добавляет его, но set-to-set amplification по умолчанию запрещено | IP-11; applicability без непредусмотренной proc chain |
| G-06 Slow lifetime | **Resolved — DECISION-0017.** Source = owner life + content ID + channel; refresh заменяет magnitude/reset duration; strongest-wins, слабые таймеры сохраняются | IP-03/IP-05/IP-09/IP-13; overlap/expiry/reapply/pool reset |
| G-07 Knockback displacement | **Resolved — DECISION-0017.** Равномерная дополнительная velocity поверх movement/dash; explicit duration, новый hit заменяет residual, zero direction не смещает, стены не копят дистанцию | IP-03/IP-05/IP-09/IP-13; player-only collision и pause/end |
| G-08 Parameter applicability | **Resolved — DECISION-0017.** Action speed→activation cooldown; size→hit shape; range→target/travel/beam length/orbit radius. Один scale на управляющий параметр | IP-08 реализует map; IP-11 сохраняет explicit set exceptions |
| G-09 Low-HP damage | **Resolved — DECISION-0017.** HP/stat-dependent coefficient кэшируется; итоговый damage snapshot при активации сохраняется во всех delayed hits; derived damage не усиливается повторно | IP-05/IP-08/IP-11; heal between cast/hit, owner teardown, immutable source |
| G-10 Potion | **Semantics resolved — DECISION-0033.** Full-HP consumption, contact без XP radius, optional paused lifetime, reachable drop, enemy→field→global chance + multiplier/cap 100%, stable pickup order и pause/terminal boundary утверждены. Production base chance/heal остаются TBD | IP-28 — fixture implementation; IP-20 — production PICKUP-001/drop values; IP-30 — Book card/ID/параметры; IP-32 — tuning proposal при необходимости |
| G-11 Traveler timing | **Resolved — DECISION-0035.** Пользователь заменил full-run interval на [0,T−120]; random types без повторов, spawn distance две высоты viewport, separate RNG, pause/terminal/timeout ordering | IP-29 реализует; текущая fixture arena увеличена до 20 высот viewport по стороне по отдельной команде пользователя |
| G-12 Traveler support | **Resolved — DECISION-0035.** Только ordinary targets; круговые ауры max-per-stat, один обновляемый shield на цель, reduction → shield → Health, source-owned cleanup; zero-contact без hit events | IP-29: overlap/expiry/death/rollback tests; production strengths/durations — IP-30 |
| G-13 Set numbers | Все 20 effects/recipes утверждены; exact thresholds и effect numbers TBD | IP-19/IP-32; complete validated data по каждому ID, без копирования старого unrelated recipe |
| G-14 Encounter/content numbers | ENEMY contact intervals; boss speed/attack/reward details; traveler production presence/XP/support values (framework scaling определён DECISION-0035); numeric character weights; field pools/geometry/schedules отсутствуют или неполны | IP-20…24/IP-30/IP-32; per-ID/per-field completeness, отдельные production schedule IDs |
| G-15 Meta and run exit | **Resolved — DECISION-0037.** Reward 5×level, Book 50, Quit/handled error payout; clear=900s alive; простой каталог upgrades/unlocks и atomic profile/save failure policy | IP-25/IP-26; no duplicate reward/retry double-start и terminal precedence |
| G-16 Presentation/settings | **Resolved — DECISION-0038.** Defaults, separate settings save/error policy, video Apply/Keep/10s Revert, Master/Music/SFX routing и bounded visual shake | IP-26 реализует service/consumer и checks; IP-12A request boundary переиспользуется, baseline follow сохранён |
| W-01 Burst pressure policy | **Resolved — DECISION-0029.** Burst обходит regular cap; boss/Traveler исключены из cap; pause-aware window, expired windows не догоняются | IP-14; runtime и boundary/load tests остаются обязательными |
| G-17 Art evidence | Пользователь 2026-09-21 подтвердил CHAR-001 concept = fixture satchel goblin v002; связь записана в asset-record, master hash совпадает с v002 | IP-12A: актуализировать inventory/manifest по фактам; production binding/crop review отдельно IP-22 |
| G-18 Player palette | **Resolved — DECISION-0029.** Зелёная skin-family текущих гоблинов, читаемый силуэт для всех playable species | IP-12A; §4.4 Art Direction согласован с §9.2 |
| G-19 Character Select baseline/highlights | **Resolved — DECISION-0026.** Отдельная явная база сравнения и authored ordered highlights; без автоматического порога значимости и выбора базы из roster | IP-12 fixture data/validation; IP-22 production values и per-character highlights. Прежняя ссылка на G-15 для presentation gap исправлена |
| G-20 Field difficulty scale | **Resolved — DECISION-0038.** Единая шкала 1–5; explicit CD mapping 1,1,2,2,3,3,4,4,5,5 | IP-23/IP-26 переносят metadata; fixture difficulty и отдельный Traveler rank/scaling не меняются |

## 7. Репозиторий: где расширять существующую реализацию

- `Assets/Game/Progression/Sets/SetDefinition.cs` хранит chance на каждом set и допускает любой непустой recipe. `Draft/DraftPool.cs` сначала chance-фильтрует sets, затем weighted-selects весь pool; target priority-first отсутствует. Priority selection принадлежит обновлённому IP-11, request queue — IP-07, reroll/banish — IP-10; это не изменение одной цифры.
- `Sets/FixtureSetExtraAbility.cs` считает ticks/time. Успешная проверка прежней ревизии IP-11 не означает наличия production transformations/procs; они входят в целевой scope того же IP.
- `ActiveSkill/Progression/ActiveSkillTargetingMode.cs` содержит только Self/NearestEnemy. Target movement/random-world/fixed-axis и новые lifecycle effects требуют IP-08.
- `Character/Model/CharacterStats.cs` уже содержит additive keyed sources, denominator cooldown и caps, но не новые knockback/range/size/drop/low-HP channels; IP-03/IP-05/IP-09/IP-13 дополняет контракт.
- `Progression/Runtime/PlayerExperienceRuntime.cs:27` читает `BaseStats.PickupRadius`, а ExperienceDropFactory передаёт radius при spawn. Новый buff должен влиять и на уже лежащий опыт; проверка только будущих drops недостаточна.
- Enemy projectile Burst pattern не является burst **wave spawn**. DECISION-0014 реализует continuous timer/cap и boss hooks; IP-14 расширяет его и сохраняет pause/restart/catch-up determinism.
- Fixture presentation, fixture enemies/skills/sets и fixture wave timeline не являются production реализацией совпадающего по смыслу `CHAR-/SKILL-/ENEMY-` ID. Репозиторный факт не определяется подписью в Art Production.

## 8. Применённая миграция M-01

Три canonical bodies заменены полностью, без архивных копий старых документов. Входные v2-файлы остаются материалами первоначального импорта; актуальные изменения выполняются только в canonical destinations. Зарегистрированы все 35 спецификаций, новая Execution order в STATUS и одинаковое правило выбора в WORKFLOW/AGENTS.

CG-01 отражает уже полученное approval 121 карточки. Три слота драфта определены, а недостающие production schedules, economy, tuning и перечисленные выше edge cases сохраняются как gates. Принятие плана не выбирает автоматически один из вариантов в открытом вопросе. Решение и границы миграции — [DECISION-0015](../decisions/0015-design-sync-r2.md).

## 9. Проверка покрытия и границы

Исходная сверка покрыла обе версии Game/Content, различия Art Direction, новый UX и Art Production, действующие решения и релевантные runtime границы. Проверены все группы 121 target IDs, но не изобретались production schedules, мета-экономика, отсутствующие значения и изображения. Unity tests здесь не запускались: это проект документации, не утверждение о новом runtime verification.

Действующие спецификации всех 35 модулей — [modules](modules/), источники и milestones — [README](README.md), общий контракт и поставка assets — [ASSET_PRODUCTION](ASSET_PRODUCTION.md), ручной цикл баланса — [BALANCE_WORKFLOW](BALANCE_WORKFLOW.md). Эта сверка не является вторым execution-status registry.
