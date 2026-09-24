# Implementation Status

Единственный источник execution status и Execution order; краткое evidence и ссылки на подробные записи. Спецификации и файлы evidence не содержат текущих статусов.

Last repository audit: 2026-09-24 (F1-00 Verified; F1-01…08 Implemented, Unity NOT RUN)
Plan revision: design-sync-R2; selected startup packets: field-001-start-R1
Current active packet: нет исполнимого без Unity — F1-09 ждёт Unity-проверки и прогонов пользователя; IP-12A gameplay density review остаётся открытым
Next Ready packet: нет. F1-00…F1-08 пройдены 2026-09-24 по разрешению пользователя; F1-09 Blocked до Unity-прогона (см. [очередь](#field001-execution)); к общему backlog не переходить.

M-01: зарегистрирован принятый план и выполнена полная замена трёх design bodies без архивных копий старых документов; [DECISION-0015](../decisions/0015-design-sync-r2.md). Код не изменён. Исторические tests не подтверждают новые требования. Все пять источников/121 target card approved; реальные missing data/semantics/assets gates сохраняются.

Подробности регистрации: [M-01 evidence](evidence/design-sync-R2-2026-09-21.md#m-01).

## Утверждённый стартовый этап — 2026-09-22

Пользователь утвердил [DECISION-0050](../decisions/0050-starting-content-and-unlocks.md)
и поручил составить план только исходно открытого контента. [DECISION-0051](../decisions/0051-field001-initial-slice.md)
и [FIELD-001 scope](milestones/FIELD-001-start.md) фиксируют 10 skills / 10 passives /
5 sets, CHAR-001, ENEMY-001…005 и ENEMY-007, BOSS-001/MIDBOSS-001, TRAVELER-001/002/005,
PICKUP-001/Book и поле с production schedule. Остальной контент сохраняется в IP.
Поздние глобальные unlocks утверждены, но gameplay ими не входит в этот этап.
Поправка пользователя: [DECISION-0052](../decisions/0052-field001-six-ordinary-enemies.md)
расширяет ordinary pool до шести за счёт ENEMY-005/007; F1-00/04/08/09 и art scope
синхронизированы. Новых runtime checks нет; очередь и зависимости сохраняются.

Изменён только дизайн/план. Runtime MetaEconomy.json переведён на DECISION-0050
в F1-03 (2026-09-24) вместе с миграцией при загрузке. IP-25/26 переоткрыты для новой delta;
их прежнее evidence сохранено как база. Остальные framework scope без изменения
поведения сохраняют своё состояние; production catalogs имеют прежние data/art
и новые packet dependencies. Approved art не означает production bindings.

Документационные проверки и read-only audit: [evidence](evidence/field-001-start-R1-2026-09-22-plan.md).

Первоначальная команда ограничивалась подготовкой плана. Последующее разрешение
на F1-00 и текущая граница зафиксированы ниже. После завершения этапа автоматически
к позднему backlog не переходить; более ранние stop boundaries остаются историей.

## Продолжение F1-00 — 2026-09-23

Пользователь разрешил выполнить первый шаг и самостоятельно предложить начальный
баланс для сложной первой карты. Уточнение: пройти без постоянной прокачки реально;
основной путь к первой победе — обучение и сборка билда, upgrades только помогают.
Разрешена подготовка конкретного baseline; runtime/F1-01 автоматически не начинать.
Эта команда снимает прежнюю planning-only границу только для F1-00.
Дополнительно пользователь разрешил корректировать существующие параметры врагов;
предложенная delta для шести ordinary IDs включена в baseline v1.

Подготовлены [baseline v1](../balance/field001-baseline-v1.md) и
[численные таблицы](../balance/field001-baseline-v1.json),
[DECISION-0053](../decisions/0053-field001-difficulty-and-baseline.md).
F1-00 остаётся In progress до approval конкретной v1: 60 skill levels,
60 passive levels, 5 set recipes/effects, 6 ordinary, bosses/Travelers/pickups,
24 фазы, geometry и performance targets. API/art gaps явно закреплены за
реализующими packet owners; production IDs ещё не реализованы. Статическая проверка
и границы evidence — [F1-00 evidence](evidence/field001-baseline-v1-2026-09-23.md).
Runtime/F1-01 не начаты; zero-meta победа и фактическая сложность ещё не проверялись.

## Автономное продолжение FIELD-001 — 2026-09-24

Пользователь поручил: утвердить/поправить и закрыть F1-00 (DECISION-0053 →
Approved), затем последовательно пройти F1-01…F1-09 по очереди ниже; вместо
ожидания ответа заводить DECISION-записи с выбором и обоснованием; арт готовить
и подключать по scripts/README и ASSET_PIPELINE без предварительного просмотра
(пользователь поправит вручную); после каждого пакета — safe smoke-check и
синхронизация STATUS/evidence; коммитить по ходу работы. Остановка — только на
решении без опоры в репозитории. После F1-09 к общему backlog не переходить.

F1-00 закрыт: baseline v1 Approved без изменений, canon синхронизирован
([evidence](evidence/field001-baseline-v1-2026-09-23.md#approval-2026-09-24)).

Среда исполнения этого продолжения — облачный Linux-контейнер **без Unity Editor**
(Unity download/licensing недоступны). Safe smoke-check `check_project.py` там
даёт NOT RUN. Вместо него каждый пакет проверяется compile/test harness на .NET 8:
все 41 asmdef компилируются против UnityEngine reference assemblies с заглушками
Editor/TestTools/InputSystem, NUnit-тесты без native Unity runtime исполняются
(базовая линия до изменений: 315 из 683 тестов исполнимы и проходят; остальные
требуют GameObject/сцен и считаются NOT RUN). Это не Unity evidence: пакеты с
кодом получают не выше `Implemented`, `Verified` требует Unity-прогона на машине
пользователя.

## Граница текущего продолжения

IP-01, IP-03…IP-10/IP-10A проверены для design-sync-R2; IP-00/IP-02 сохранены. Проверка перед IP-11 2026-09-21: 407/407 Game.* EditMode, 4/4 PlayMode, 0 skipped (Unity 6000.6.0f1); teardown defect исправлен в IP-31, см. его evidence. G-01/G-03 draft semantics закрыты DECISION-0019/0020: uniform set backfill, общая очередь, только пустая при подборе Книга немедленно начисляет валюту. Production сумма и pickup content не объявлены готовыми.

IP-10A завершён: reusable cards, HUD/Pause, projection contract и fake-state harness. Пользователь 2026-09-21 явно снял границу перед IP-31 и разрешил выполнить этот модуль. Разрешение не распространяется на автоматическое выполнение следующих IP. IP-11 был подготовлен для framework fixtures; Presentation policy IP-12 позднее утверждена DECISION-0026; G-15 относится к production unlock semantics.

IP-31 проверен automated checks и реальным ручным run с report/feedback 2026-09-21. Наблюдение пользователя «опыт стреляет» сохранено в [записи прогона, OBS-01](../playtests/2026-09-21_108ff5b3.md#obs-01--опыт-визуально-воспринимается-как-стреляющий-объект) для отдельной диагностики; причина не установлена. Пользователь одобрил хранение обработанных отзывов и выбранных reports в docs/playtests; процесс и шаблон синхронизированы. По последующему разрешению пользователя IP-32 завершён: реальный review с insufficient-evidence/no-change и synthetic accept/apply/rollback проверены. OBS-01 открыт; tuning не разрешён. Последующий явный запрос пользователя «реализуй следующий пункт» разрешил IP-11.

IP-11 завершён: global draft policy, recipes, реальные fixture effect families/source ownership и recipe UI. Финальная проверка 2026-09-21: 426/426 Game.* EditMode, 5/5 PlayMode, 0 skipped. При повторном run выявлен и исправлен producer-first teardown: pre-clear notification завершает consumers до Health/Stats; smoke воспроизводит этот порядок явно. После последующего approval DECISION-0026 следующий Ready пересчитан по Execution order: IP-12. Автоматически следующий модуль не начинать.

Пользователь разрешил IP-12 запросом «закомить и делай следующий». Предыдущий пакет сохранён коммитом `ee9945d`; разрешение на продолжение ограничено IP-12.

IP-12 завершён по этому разрешению: выбор до начала забега, отдельный baseline/ordered highlights, profile access boundary и повторный запуск. Проверки: 439/439 EditMode, 6/6 PlayMode, 0 skipped. Следующий Ready — IP-13; IP-12A удерживают G-17/G-18. На IP-13 автоматически не переходить.

Последующий запрос пользователя разрешил IP-13. Прежняя граница перед ним снята только для этого модуля; дальнейшие IP автоматически не начинать.

IP-13 завершён по последнему разрешению: 467/467 EditMode, 7/7 PlayMode, 0 skipped. Пересчёт очереди: Ready нет. IP-12A удерживают G-17/G-18, IP-14 — W-01; остальные незавершённые IP имеют их прямые/косвенные зависимости и собственные gates. Следующий конкретный planning packet — согласование W-01 для IP-14 либо закрытие art gates IP-12A; реализация не начинается автоматически.

Пользователь подтвердил продолжение с IP-12A и DECISION-0029, уточнив, что burst не ограничивается regular cap; затем подтвердил связь CHAR-001 concept с текущим fixture goblin. Граница перед IP-12A снята; дальнейшие IP автоматически не начинать. IP-13 сохранён коммитом `10a1d68`.

Следующий запрос «делай следующий пункт» разрешил IP-14 по Execution order. Модуль завершён:
496/496 Game.* EditMode, 9/9 PlayMode, 0 skipped. Утверждённый пользователем spawn-only
load bound (100 врагов, 10 циклов, cold ≤250 ms / pooled ≤50 ms) пройден. Следующий Ready
— IP-15; автоматически не начинать. Gameplay density review IP-12A остаётся открытым.

Запрос «делаем следующий пункт» разрешил IP-15. Framework завершён: **516/516 Game.*
EditMode, 10/10 PlayMode, 0 skipped** (2026-09-21, Unity 6000.6.0f1). Boss phases/hooks,
HP/name, pause physics и cleanup проверены; production G-14 остаётся у IP-21.
Следующий Ready — IP-16; автоматически не начинать.

Запрос «делай следующий шаг» разрешил IP-16. Fixture field selection/configuration
проверены: **538/538 Game.* EditMode, 12/12 PlayMode, 0 skipped** (2026-09-21,
Unity 6000.6.0f1). Ready нет: ближайший по очереди IP-28 удерживает G-10;
IP-25 — CG-03/G-15. Обнаружен G-20 (UI/IP difficulty 1–5 против CD 1–10),
решение запрошено; fixture шкала сохранена без production mapping.
Автоматически следующие IP и новые product rules не начинать.

Пользователь подтвердил шесть правил G-10 ответом «подтверждаю»:
[DECISION-0033](../decisions/0033-world-pickup-rules.md) Approved. GDD/CD, IP-28
и consumer gates синхронизированы. IP-28 пересчитан в Ready: dependencies целевой
ревизии Verified, IP-12A Implemented допустим по WORKFLOW; его density review
остаётся отдельным gate. Это approval правил; реализация IP-28 ещё не начиналась.

Запрос «работаем дальше» разрешил IP-28. Framework завершён: **571/571 Game.*
EditMode, 14/14 PlayMode, 0 skipped** (2026-09-21, Unity 6000.6.0f1).
Следующего Ready нет: IP-29 удерживают G-11/G-12/G-14, IP-25 — CG-03/G-15;
production packets имеют собственные data/art gates. Следующий planning packet —
правила encounter/scaling/support Путников для IP-29. Автоматически не начинать.
Пользователь уточнил Traveler rules и отдельно поручил увеличить текущую арену
примерно до 20 экранов. DECISION-0035 фиксирует правила и выбранную по поручению
формулу; арена 200×200 проверена (571/571 EditMode, 14/14 PlayMode). IP-29 Ready
для synthetic framework; эта подготовка не является реализацией Путников.
Следующий модуль автоматически не начинать. [Evidence](evidence/design-sync-R2-2026-09-21-traveler-preparation.md).
Запрос «реализуй» разрешил IP-29. Framework завершён: **598/598 Game.* EditMode,
15/15 PlayMode, 0 skipped**, Unity 6000.6.0f1 (2026-09-21). Ready после пересчёта
нет: IP-25 удерживают CG-03/G-15, остальные незавершённые packets — свои dependencies
и production/settings/art gates. Следующий planning packet — economy/reward/unlock
правила IP-25; реализация следующих IP автоматически не начинается.
Пользователь определил reward=5×level, Book=50, начисление при досрочном выходе,
field clear=15 минут выживания и поручил самостоятельно выбрать остальные простые
правила. DECISION-0037 Approved; GDD/CD/UI/IP и gates синхронизированы. IP-25 Ready:
IP-01/IP-03/IP-12/IP-16/IP-10A Verified целевой ревизии, CG-03/G-15 resolved.
Read-only проверка кода не нашла готового profile/checkpoint pipeline; hard-crash
recovery не включён по условию «если это ничего не стоит». Это подготовка дизайна,
не implementation evidence и не разрешение автоматически начинать реализацию.

Запрос «продолжай» разрешил реализацию IP-25 по DECISION-0037. Разрешение ограничено этим модулем.

IP-25 завершён по запросу «продолжай»: **624/624 Game.* EditMode, 18/18 PlayMode,
0 skipped**, Unity 6000.6.0f1, 2026-09-21. Production economy JSON, persistent profile,
reward/save idempotency и UI result→purchase→next run проверены. Runtime integration
использует отдельный fixture catalog/profile; production gameplay/art не объявлены
готовыми. Следующего Ready нет: IP-26 удерживает G-16 (settings/audio/shake);
G-20 остаётся для production field UI. Следующий planning packet — правила настроек
IP-26. Автоматически следующие IP не начинать.

Пользователь выбрал сложность 1–5 и поручил самостоятельно выбрать остальные
settings rules между привычным поведением жанра и простотой реализации.
[DECISION-0038](../decisions/0038-settings-and-field-difficulty.md) Approved:
G-16/G-20 resolved, CD/UI/GDD и IP-16/23/26 синхронизированы. IP-26 Ready:
все зависимости Verified целевой ревизии либо IP-12A Implemented (допустимо по
WORKFLOW); его gameplay density review сохраняется отдельно. Код не менялся,
settings implementation/новые runtime checks ещё не выполнялись. Это подготовка
правил, не разрешение автоматически начинать следующий IP.

Запрос «продолжай» разрешил реализацию IP-26 по DECISION-0038. Дальнейшие IP автоматически не начинать.

Отдельный пользовательский art packet 2026-09-22 добавил утверждённые projectile images: камень SKILL-001 подключён к `FIXTURE-SKILL-BOLT`, письмо — только к ranged `FIXTURE-ENEMY-FAN`. Общий data-driven presentation сохраняет круглый collider, вращает только visual child камня и даёт короткий pooled flash + material particles. Проверки: **647/647 Game.* EditMode, 25/25 PlayMode, 0 skipped**, Unity 6000.6.0f1. Это не регистрирует production SKILL-001 и не меняет approved melee card ENEMY-002; статусы IP-17/IP-20 сохраняются. [Evidence](evidence/2026-09-22-projectile-art.md).

## Execution order

При разрешении на исполнение выбирать первый Ready packet активного этапа ниже,
если пользователь не назвал другой scope. Пока этап активен, поздний backlog
автоматически не выбирать. Порядок IP после этапа сохранён во второй таблице.

<a id="field001-execution"></a>
### FIELD-001 initial slice — приоритетная очередь

Все packets относятся к `field-001-start-R1`. Status ниже относится к packet,
а не к полному каталожному IP. У всех ещё не начатых packets completed IDs: none,
implementation/verification evidence: none. F1-00 разрешён только для подготовки
данных; incomplete production ID по нему реализовывать нельзя.

| Приоритет | Packet / владельцы | Status | Prerequisites / конкретный gate |
|---:|---|---|---|
| 1 | [F1-00 — полные данные](milestones/FIELD-001-start.md#f1-00); IP-17…26/30/32 | Verified | 2026-09-24: baseline v1 Approved (DECISION-0053), canon синхронизирован; static validator PASS; [evidence](evidence/field001-baseline-v1-2026-09-23.md#approval-2026-09-24) |
| 2 | [F1-01 — 10 skills](milestones/FIELD-001-start.md#f1-01); IP-17 | Implemented | 2026-09-24: completed IDs SKILL-001…007/010/013/014 (L1–6, art/VFX bound in data); .NET harness PASS; Unity full 2026-09-24: EditMode 706/709 → после исправлений 709/709 ([запись](evidence/field001-f1-01-2026-09-24.md#unity-прогон-2026-09-24)); PlayMode 25/26 — smoke F1-08 исправлен ([запись](evidence/field001-f1-08-2026-09-24.md#unity-прогоны-2026-09-24)), нужен повторный `check_project.py --scope full`; [evidence](evidence/field001-f1-01-2026-09-24.md), [DECISION-0054](../decisions/0054-field001-autonomous-execution.md) |
| 3 | [F1-02 — 10 passives](milestones/FIELD-001-start.md#f1-02); IP-18 | Implemented | 2026-09-24: completed IDs PASSIVE-001…005/007…009/011/012 (L1–6, icons); .NET harness PASS, Unity NOT RUN; [evidence](evidence/field001-f1-02-2026-09-24.md) |
| 4 | [F1-03 — Клёпка/profile/UI](milestones/FIELD-001-start.md#f1-03); IP-22/25/26 | Implemented | 2026-09-24: CHAR-001 production definition/visual binding, MetaEconomy по DECISION-0050, миграция при загрузке; .NET harness PASS, Unity NOT RUN; [evidence](evidence/field001-f1-03-2026-09-24.md) |
| 5 | [F1-04 — enemies/potion](milestones/FIELD-001-start.md#f1-04); IP-20 | Implemented | 2026-09-24: ENEMY-001…005/007 + PICKUP-001 (данные/поведение/снаряды), .NET harness PASS, Unity NOT RUN; **art gate:** тела ENEMY-003/004/005/007; [evidence](evidence/field001-f1-04-2026-09-24.md) |
| 6 | [F1-05 — 5 sets](milestones/FIELD-001-start.md#f1-05); IP-19 | Implemented | 2026-09-24: SET-001/004/006/010/017 (пороги, эффекты, SET-017 attack/telegraph); .NET harness PASS, Unity NOT RUN; [evidence](evidence/field001-f1-05-2026-09-24.md) |
| 7 | [F1-06 — boss/mid-boss](milestones/FIELD-001-start.md#f1-06); IP-21 | Implemented | 2026-09-24: BOSS-001/MIDBOSS-001 (атаки, фазы, двойной рывок); .NET harness PASS, Unity NOT RUN; **art gate:** тела и снаряды боссов; [evidence](evidence/field001-f1-06-2026-09-24.md) |
| 8 | [F1-07 — 3 Travelers/Book](milestones/FIELD-001-start.md#f1-07); IP-30 | Implemented | 2026-09-24: TRAVELER-001/002/005 + FIELD-001 schedule, PICKUP-002; .NET harness PASS, Unity NOT RUN; **art gate:** тела Путников; [evidence](evidence/field001-f1-07-2026-09-24.md) |
| 9 | [F1-08 — production field/run](milestones/FIELD-001-start.md#f1-08); IP-23/24/25/26 | Implemented | 2026-09-24: FIELD-001 (поле, 900-s timeline, 64 authored player-only obstacles), production composition без fixture fallback, production профиль `profile-v1.json`; .NET harness PASS, Unity NOT RUN; [evidence](evidence/field001-f1-08-2026-09-24.md), [DECISION-0054 §9](../decisions/0054-field001-autonomous-execution.md#9-конкретизации-f1-08) |
| 10 | [F1-09 — доведение/приёмка](milestones/FIELD-001-start.md#f1-09); IP-27/12A/31/32 | Blocked | F1-00…08 Implemented. **Нужно от пользователя:** Unity `check_project.py --scope full` (F1-01…08 → Verified), реальные прогоны по матрице, performance bounds, приёмка ощущения карты; art gates — [бриф](../art/field001-open-art-requests.md). [Подготовка/матрица NOT RUN](evidence/field001-f1-09-2026-09-24.md) |

При завершении добавлять сюда completed IDs, дату/revision и evidence ссылку,
пересчитывать downstream. Успех стартового packet не закрывает весь IP; его
оставшиеся ID перечислены в записи владельца. Принятые baseline frameworks —
зависимости по именам в спецификации packet и записям ниже, не повторные работы.

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
Documentation impact: IP-01 terminal/time/teardown contract; GDD/CD rules unchanged.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-01).

### IP-02 — Перемещение игрока, камера и базовая геометрия

Status: Verified
Dependencies: IP-01
Current packet: Новой реализации не требуется; Context обновлён, существующее поведение сохранено.
Remaining gates: Нет дополнительных product gaps для текущего packet.
Remaining acceptance / IDs: Нет behavioral delta; новые интеграции проверяются в owning IP.
Target implementation evidence: [Подробности](evidence/design-sync-R2-2026-09-21.md#ip-02).
Target verification evidence: Сохранённые проверки [неизменного scope](evidence/pre-design-sync-R2.md#ip-02); M-01 проверяет документы/совместимость, Unity заново не запускался.
Documentation impact: Обновлены Context/источники/consumer links.
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
Remaining gates: Нет для fixture framework. G-02 закрыт approved DECISION-0022; G-03 — DECISION-0020. Production counts/recovery остаются CG-04; global set chance provider поставлен IP-11; production значение остаётся balance-data.
Remaining acceptance / IDs: Нет для обязательного scope IP-10.
Target implementation evidence: Snapshot всех checks, ordinal ID, сохранение при banish, mode/cancel/control hints — [IP-10 evidence](evidence/design-sync-R2-2026-09-21-ip10.md#ip-10).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 374/374, PlayMode 2/2 passed, 0 skipped. Условия, coverage и XML/log paths — по ссылке выше.
Documentation impact: Approved DECISION-0022, GDD/UI, DESIGN_SYNC/proposal, IP-07/IP-10/IP-10A/IP-11/IP-19/IP-28 и readiness consumers синхронизированы. Production balance не изменён.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-10).

### IP-10A — UI Foundation, reusable cards, HUD и test harness

Status: Verified
Dependencies: IP-01, IP-03, IP-06, IP-07, IP-10
Remaining gates: G-01/G-03 short/book states определены DECISION-0019/0020 и проверены IP-07. Foundation сохраняет существующий contract. Baseline-relative character filtering — IP-12.
Remaining acceptance / IDs: none for the foundation scope; real recipe/character semantics belong to IP-11/IP-12.
Target implementation evidence: Reusable cards, recipe projection contract, compact HUD/Pause grid, notifications, changed-state rendering — [IP-10A evidence](evidence/design-sync-R2-2026-09-21-ip10a.md#ip-10a).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: Game.* EditMode 383/383, PlayMode 3/3, 0 skipped; geometry/input and reviewed captures at 1920x1080 / 1280x720. Pre-existing post-results teardown exception recorded in evidence.
Documentation impact: Component/semantic contracts, IP-11/IP-12 consumers and readiness synchronized; no GDD/CD or production balance change.
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
Current packet: Checklist/review templates, реальный fixture review OBS-01 (insufficient-evidence / no-change) и synthetic accept/apply/rollback exercise.
Remaining gates: Нет для workflow scope; BG-01 и explicit approval сохраняются для будущего применения конкретных чисел/механик.
Remaining acceptance / IDs: Нет для workflow scope. OBS-01 остаётся открытым; диагностика и реальный follow-up описаны в review, исправление не заявлено.
Target implementation evidence: [IP-32 evidence](evidence/design-sync-R2-2026-09-21-ip32.md#ip-32), [реальный review](../balance/balance-progression-2026-09-21.md), [checklist](../playtests/CHECKLIST.md).
Target verification evidence: 2026-09-21, Python exercise exit 0: source hashes/arithmetic, 8 отказов, partial approval, apply/rollback/drift, Content JSON unchanged. Markdown links/diff checks. Unity не запускалась: runtime/config не менялись.
Documentation impact: BALANCE_WORKFLOW, playtest templates/review/OBS, IP-32 и readiness; GDD/CD без изменений, tuning не применён.
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

Status: Verified
Dependencies: IP-03, IP-04, IP-05
Current packet: Fixture movement/attack/control integration, explicit per-kind JSON и category-neutral projectile lifecycle.
Remaining gates: Нет для fixture framework. G-07 закрыт DECISION-0017. G-14 остаётся для production cards; новые wind-up/control values — synthetic fixtures.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-13 evidence](evidence/design-sync-R2-2026-09-21-ip13.md#ip-13), [schema/compatibility matrix](modules/IP-13-enemy-patterns.md#schema-и-runtime-contract).
Target verification evidence: 2026-09-21 — Unity 6000.6.0f1, **467/467 Game.* EditMode, 7/7 PlayMode, 0 skipped**. Все семь attack families, dash+slow+knockback, source после смерти/reuse стрелка, pool/terminal cleanup и representative physics smoke; [details](evidence/design-sync-R2-2026-09-21-ip13.md#coverage-and-verification).
Documentation impact: IP-13/IP-15/IP-20/IP-21/IP-29 contracts, DECISION-0028 (Proposed technical record), regression guards и consumer readiness. GDD/CD production values не изменены.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-13).

### IP-14 — Wave Director: continuous и burst timeline

Status: Verified
Dependencies: IP-04, IP-13
Current packet: Explicit continuous/burst schema, one-shot uncapped windows, skipped-window expiry, seeded composition/geometry, deterministic hooks, actual spawn outcomes и existing HUD/DEV projection.
Remaining gates: Нет для synthetic framework. W-01 выполнен по DECISION-0029; G-11/G-14 production schedules/Traveler timing остаются у catalog packets. IP-12A density review отдельно.
Remaining acceptance / IDs: none for the fixture framework packet.
Target implementation evidence: [IP-14 evidence](evidence/design-sync-R2-2026-09-21-ip14.md#ip-14), [runtime/schema](modules/IP-14-wave-director.md#runtime-и-fixture-schema).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: **496/496 Game.* EditMode, 9/9 PlayMode, 0 skipped**. 100 enemies ×10 cycles: empty-pool 10.342 ms, pooled max 1.376 ms; approved bounds 250/50 ms, unique objects 100, registry baseline restored. [Conditions/results](evidence/design-sync-R2-2026-09-21-ip14.md#checks).
Documentation impact: DECISION-0014 supplement, IP-14 schema/fixture rationale, IP-15/IP-24 consumer contracts и readiness. GDD/CD/art без изменений; production balance/FPS guarantees не заявлены.
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

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — новый production profile 10/10/5 и DECISION-0050 unlock metadata; terminal integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-03, IP-12, IP-16, IP-10A
Current packet: F1-03 по DECISION-0050/0051, затем F1-08 integration. Поздний gameplay не включён.
Remaining gates: F1-00/01/02; ещё не проверены новые production unlock/UI contracts и startup bindings.
Remaining acceptance / IDs: Unity verification; terminal/save/retry на production профиле — F1-09 matrix. F1-08: production `profile-v1.json` и composition по экономике профиля Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md). F1-03 subset (10/10/5, DECISION-0050 mapping, load-time migration) Implemented 2026-09-24 — [evidence](evidence/field001-f1-03-2026-09-24.md).
Prior implementation evidence (design-sync-R2): [IP-25 evidence](evidence/design-sync-R2-2026-09-21-ip25.md#implementation), [runtime/schema](modules/IP-25-meta-progression.md#runtime-api--schema--reset).
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1: **624/624 Game.* EditMode, 18/18 PlayMode, 0 skipped**; [coverage/results](evidence/design-sync-R2-2026-09-21-ip25.md#checks).
Documentation impact: IP-25 API/schema/save/reset и IP-26 consumers, regression map; GDD/CD правила DECISION-0037 сохранены. Fixture Book=50, новые raster assets не создавались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-25).

Target implementation evidence: none для field-001-start-R1 delta.
Target verification evidence: none для field-001-start-R1 delta; прежние smoke не переносятся автоматически.

### IP-26 — Functional UI и полный player flow

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — startup/locks/recipe UI; Results и actual-content integration в F1-08. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-01, IP-10A, IP-11, IP-12, IP-15, IP-16, IP-25, IP-28, IP-29, IP-12A
Current packet: F1-03 по DECISION-0050/0051, затем F1-08 integration. Поздний gameplay не включён.
Remaining gates: F1-00/01/02; ещё не проверены новые production unlock/UI contracts и startup bindings.
Remaining acceptance / IDs: Unity verification и F1-09 matrix (Results/save/retry на production composition). F1-08: selection/run/Results подключены к production composition — [evidence](evidence/field001-f1-08-2026-09-24.md). F1-03: production roster/lock reasons data — [evidence](evidence/field001-f1-03-2026-09-24.md).
Prior implementation evidence (design-sync-R2): Main Menu/full navigation, settings persistence/video rollback/audio routing/shake, notifications, result sets/special kills и permanent modifier display; [IP-26 evidence](evidence/design-sync-R2-2026-09-21-ip26.md#ip-26).
Documentation impact: DECISION-0038, GDD/CD/UI settings/difficulty, IP-12A/16/23/26 contracts, DESIGN_SYNC, regression map и consumer readiness.
Prior verification evidence (design-sync-R2): 2026-09-21, Unity 6000.6.0f1, **637/637 Game.* EditMode, 22/22 PlayMode, 0 skipped**, Windows release build exit 0. Interactive menu/settings/contrast checked at native 2560×1440; Пользователь сообщил «всё в порядке», кроме недоступного Retry после поражения; [OBS-01](../playtests/2026-09-21_defeat-ui.md#obs-01--после-поражения-нельзя-перезапустить-забег) воспроизведён и исправлен с failing-before/passing-after regression. После отчёта об исправлении пользователь явно поручил «ставь верифайд и комить»: оставшиеся manual acceptance gates закрыты его приёмкой. Новые измерения 1920×1080 или повторный ручной прогон не заявляются; см. evidence/DECISION-0038.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-26).

Target implementation evidence: none для field-001-start-R1 delta.
Target verification evidence: none для field-001-start-R1 delta; прежние smoke не переносятся автоматически.

### IP-17 — Production Active Skills SKILL-001…016

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-01 — SKILL-001…007/010/013/014. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-08, IP-10A, IP-12A
Blocked by: complete per-level parameters SKILL-001…016 и per-ID image gates.
Remaining gates: G-08/G-09 закрыты DECISION-0017; нужны полные параметры 16 skills; G-04 только если решение меняет SKILL-008; images проходят asset gates.
Remaining acceptance / IDs: SKILL-008/009/011/012/015/016 и Unity verification стартового поднабора.
Startup subset F1-01: SKILL-001…007/010/013/014 Implemented 2026-09-24 — [evidence](evidence/field001-f1-01-2026-09-24.md).
Target implementation evidence: F1-01 subset only.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-17).

### IP-18 — Production Passive Items PASSIVE-001…014

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-02 — PASSIVE-001…005/007…009/011/012. Required packets: F1-00/01; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-09, IP-10A, IP-12A, IP-28
Blocked by: собственные content/design gates ниже; prerequisite IP-28 выполнен.
Remaining gates: G-08/G-09 закрыты DECISION-0017; G-10 закрыт DECISION-0033/IP-28; полные значения 14 passives остаются; отсутствие конкретного runtime parameter не заполняется hidden default.
Remaining acceptance / IDs: PASSIVE-006/010/013/014 и Unity verification стартового поднабора.
Startup subset F1-02: PASSIVE-001…005/007…009/011/012 Implemented 2026-09-24 — [evidence](evidence/field001-f1-02-2026-09-24.md).
Target implementation evidence: F1-02 subset only.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-18).

### IP-19 — Production Sets SET-001…020

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-05 — SET-001/004/006/010/017. Required packets: F1-00/01/02/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-11, IP-17, IP-18, IP-28, IP-12A
Blocked by: IP-17 (Blocked, target scope), IP-18 (Blocked, target scope).
Remaining gates: G-08 закрыт DECISION-0017. G-02 закрыт DECISION-0022. G-04/G-05/G-13: recipes/effects approved, но thresholds/proc payload и два внутренних конфликта требуют закрытия.
Remaining acceptance / IDs: остальные 15 SET ID и Unity verification стартового поднабора.
Startup subset F1-05: SET-001/004/006/010/017 Implemented 2026-09-24 — [evidence](evidence/field001-f1-05-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-19).

### IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-04 — ENEMY-001…005, ENEMY-007 и PICKUP-001. Required packets: F1-00; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-04, IP-13, IP-28, IP-12A
Blocked by: собственные content/design gates ниже; prerequisite IP-28 выполнен.
Remaining gates: G-10 semantics/lifecycle закрыты DECISION-0033/IP-28. G-14: contact intervals, недостающие attack/drop/healing values; AG-01 для конкретных картинок. Approved design не означает complete JSON.
Remaining acceptance / IDs: ENEMY-006, ENEMY-008…020; тела ENEMY-003/004/005/007 (art gate); Unity verification стартового поднабора.
Startup subset F1-04: ENEMY-001…005/007 + PICKUP-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-04-2026-09-24.md).
Target implementation evidence: ENEMY-001 v002 принят пользователем; runtime 256×256 импортирован и подключён как body существующего FIXTURE-ENEMY-SEEKER с отдельным motion profile/child rig. Fixture ID, баланс и collider сохранены. Production ENEMY-001 binding не выполнен; G-14 и пользовательский gameplay/density review остаются. [Art integration evidence](evidence/2026-09-21-enemy001-art.md).
Target verification evidence: 2026-09-21, Unity 6000.6.0f1: 641/641 Game.* EditMode и 23/23 PlayMode, 0 skipped. Import/reimport GUID, registry refs, child-only motion, hit/pause, death/mixed-pool reuse и Gameplay spawner. [Условия и ограничения](evidence/2026-09-21-enemy001-art.md#verification).
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-20).

### IP-21 — Production Final Bosses и Mid-bosses

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-06 — BOSS-001 и MIDBOSS-001. Required packets: F1-00/01/04; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-15, IP-12A
Blocked by: G-14 production attack payload/rewards/timings и per-ID asset packet readiness.
Remaining gates: G-14: точные attack timings/phase payload, rewards и required fields каждой карточки.
Remaining acceptance / IDs: BOSS-002…010, MIDBOSS-002…010; тела/снаряды BOSS-001/MIDBOSS-001 (art gate); Unity verification.
Startup subset F1-06: BOSS-001, MIDBOSS-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-06-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-21).

### IP-22 — Production Characters CHAR-001…010

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-03 — CHAR-001; поздние character IDs только unlock metadata. Required packets: F1-00/01/02; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-12, IP-17, IP-12A
Blocked by: IP-17 (Blocked, target scope).
Remaining gates: G-14: weights; G-15 resolved по DECISION-0037, unlock metadata определены; concept/master identity подтверждена DECISION-0029, production runtime binding/art review остаются per-ID. CHAR-006 огр и прочие approved roster choices не переутверждаются.
Remaining acceptance / IDs: CHAR-002…010 и Unity verification CHAR-001.
Startup subset F1-03: CHAR-001 Implemented 2026-09-24 — [evidence](evidence/field001-f1-03-2026-09-24.md).
Target implementation evidence: F1-03 subset only.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-22).

### IP-23 — Production Fields FIELD-001…010

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 geometry/environment/metadata/thumbnail. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-16, IP-20, IP-21, IP-12A
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope).
Remaining gates: G-14: geometry/enemy pools; G-20 resolved по DECISION-0038; G-15 resolved по DECISION-0037. Весь approved mapping переносится, numeric schedules отдельно.
Remaining acceptance / IDs: FIELD-002…010, geometry/metadata/kits/thumbnails; FIELD-001 thumbnail image и Unity verification.
Startup subset F1-08: FIELD-001 geometry/obstacles/metadata/environment Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-23).

### IP-30 — Production Travelers TRAVELER-001…010 и Book

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-07 — TRAVELER-001/002/005 и production Book. Required packets: F1-00/01/02/04/05; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-29, IP-12A
Blocked by: production Book card/ID/параметры, required Traveler/support/XP/presence data и per-ID art gates; prerequisite IP-29 выполнен.
Remaining gates: G-03/G-10 semantics закрыты DECISION-0020/0033 и IP-28; G-11/G-12/scaling semantics — DECISION-0035. G-14/G-17, production Book card/ID/параметры, complete Traveler/support data и конкретные images. Designs TRAVELER-001…010 уже approved.
Remaining acceptance / IDs: TRAVELER-003/004/006…010; тела TRAVELER-001/002/005 (art gate); Unity verification.
Startup subset F1-07: TRAVELER-001/002/005, FIELD-001 schedule, PICKUP-002 Implemented 2026-09-24 — [evidence](evidence/field001-f1-07-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-30).

### IP-24 — Canonical Wave / Encounter Content и field bindings

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-08 — FIELD-001 900-second schedule и startup bindings. Required packets: F1-00…07; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-14, IP-20, IP-21, IP-23, IP-29, IP-30
Blocked by: IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-23 (Blocked, target scope), IP-30 (Blocked, target scope).
Remaining gates: CG-02/G-11/G-14/W-01: full per-field encounter/scaling packets; пустой Wave section не разрешает coding AI придумать канон.
Remaining acceptance / IDs: Полные production encounter schedules и bindings полей 002…010; CG-02/CG-04; Unity verification FIELD-001.
Startup subset F1-08: FIELD-001-TIMELINE (900 s, hooks 450/810) и startup bindings Implemented 2026-09-24 — [evidence](evidence/field001-f1-08-2026-09-24.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-24).

### IP-27 — End-to-end integration, regression и content validation

Status: Blocked
Scope revision: design-sync-R2 + field-001-start-R1 for selected startup packet.
Startup packet: F1-09 — полный стартовый run и приёмка FIELD-001 только initial content. Required packets: F1-00…08; authoritative readiness/evidence — [startup queue](#field001-execution).
Dependencies: IP-00, IP-01, IP-02, IP-03, IP-04, IP-05, IP-06, IP-07, IP-08, IP-09, IP-10, IP-10A, IP-11, IP-12, IP-12A, IP-13, IP-14, IP-15, IP-16, IP-17, IP-18, IP-19, IP-20, IP-21, IP-22, IP-23, IP-24, IP-25, IP-26, IP-28, IP-29, IP-30, IP-31, IP-32
Blocked by: IP-17 (Blocked, target scope), IP-18 (Blocked, target scope), IP-19 (Blocked, target scope), IP-20 (Blocked, target scope), IP-21 (Blocked, target scope), IP-22 (Blocked, target scope), IP-23 (Blocked, target scope), IP-24 (Blocked, target scope), IP-25 (Blocked, field-001-start-R1 delta), IP-26 (Blocked, field-001-start-R1 delta), IP-30 (Blocked, target scope).
Remaining gates: Только реальные missing required contracts/data/asset checks полного scope этого плана. Уменьшение каталога возможно лишь как отдельное явное изменение плана; один smoke не закрывает content-complete verification.
Remaining acceptance / IDs: Все criteria/IDs из [спецификации](modules/IP-27-integration.md).
Target implementation evidence: Нет для новых требований.
Target verification evidence: Новые checks не запускались.
Historical evidence: [До design-sync-R2](evidence/pre-design-sync-R2.md#ip-27).

## Status maintenance rule

Workflow tooling follow-up, 2026-09-22: Tooling packet: Implemented. Подготовка approved art и numeric-only visual-preview работают через общие команды; 18/18 tooling tests, реальный безопасный batch smoke **658/658 EditMode + 25/25 PlayMode**, 0 skipped, manifest 83 records. Повторное использование совпадающего evidence проверено; live REST остаётся непроверенным на открытом Editor (protocol/mode/filter flow покрыт изолированными тестами). [DECISION-0049](../decisions/0049-art-workflow-automation-and-preview.md), [команды](../../scripts/README.md), [evidence](evidence/2026-09-22-workflow-tools.md). Обязательные acceptance IP, Execution order и Next Ready сохранены.

После изменения статуса/API/acceptance пересчитать готовность потребителей и Next Ready по Execution order. Implemented означает выполненный полный обязательный scope; Verified — фактически пройденные проверки с evidence. Исторический test count не переносится автоматически. Каталоги ведут completed/remaining IDs здесь; если ни один оставшийся packet не готов, указывать конкретный Blocked gate. В STATUS оставлять краткий результат, дату, revision и ссылку на подробное evidence в `evidence/`; старые проверки не читать при выборе следующего IP. Подробности — [WORKFLOW](WORKFLOW.md).

### Contact review follow-up — 2026-09-22

Пользователь принял максимальные вписанные круги goblin/villager и поручил закрепить метод как этап пайплайна. Radius 0.401431 / 0.330282, centerY 0.530976 / 0.469539; [ASSET_PIPELINE §22](../art/ASSET_PIPELINE.md#22-подгонка-круга-контакта-для-world-body), [DECISION-0039](../decisions/0039-conservative-body-contact-circles.md), [evidence](evidence/2026-09-22-body-contact-circles.md#third-trial--maximum-inscribed-circles). Финальная runtime ревизия: 644/644 EditMode, 24/24 PlayMode, zero skipped. Последующее закрепление пайплайна меняет только документы. Общий gameplay/density gate IP-12A открыт; порядок IP не изменён.

### Enemy death presentation follow-up — 2026-09-22

По явному поручению пользователя мгновенное исчезновение заменено единым procedural tail для ordinary/boss/Traveler: squash, shrink/fade и dust; без специальных веток и без death push. Gameplay death/reward/untargeting остаются мгновенными, pool return задержан на 0.30 s и замораживается pause. Финальная проверка: 644/644 EditMode, 25/25 PlayMode, zero skipped. [DECISION-0040](../decisions/0040-shared-enemy-death-presentation.md), [pipeline](../art/ASSET_PIPELINE.md#23-единая-процедурная-смерть-врагов), [evidence](evidence/2026-09-22-shared-enemy-death.md). Порядок IP не изменён.

Последующий пользовательский плейтест выявил невидимый death clone/pooled animated body. Порядок snapshot и восстановление renderer исправлены, regression обновлён. Пользователь повторно проверил Gameplay и принял результат 2026-09-22: «Сейчас выглядит хорошо», разрешил коммит и подтвердил запись полной процедуры. Финальный post-fix smoke: 644/644 EditMode, 25/25 PlayMode, zero skipped; подробности в evidence.

### Courier and ground-shadow art follow-up — 2026-09-22

По поручению выполнить art-пункты 1–3 вместе сгенерирован ENEMY-002 и подключён к неизменённому `FIXTURE-ENEMY-FAN` с отдельным быстрым motion profile. Пользователь отклонил v001 как испуганного и слишком похожего на playable goblin; v002 переделан в уверенного взрослого человеческого преследователя с отдельными пропорциями, позой и формой. Runtime 256×256; максимальный вписанный contact circle v002: radius 0.360855, centerY 0.453097. Player, ordinary enemies, bosses и Travelers используют один JSON-профиль ground shadow и общую процедурную 32×32 mask без отдельных PNG и physics. Финальный v002 smoke: 646/646 EditMode, 25/25 PlayMode, zero skipped. [DECISION-0041](../decisions/0041-shared-procedural-ground-shadows.md), [pipeline](../art/ASSET_PIPELINE.md#24-единая-процедурная-ground-shadow), [evidence](evidence/2026-09-22-courier-and-ground-shadows.md). Текущий ENEMY-002 v002 и тени приняты пользователем 2026-09-22; production binding не выполнен; статусы IP и Execution order не изменены.

Последующее поручение синхронизировало правило с enemy-документами: Game Design, общий раздел Enemies и карточка ENEMY-002 в Content Design, Art Direction §8/generation/review и IP-20 теперь требуют отделять enemy identity от playable минимум по silhouette/proportions, posture/expression и costume/palette mass. Уверенный преследователь ENEMY-002 v002 является первым эталоном; [DECISION-0042](../decisions/0042-enemy-player-visual-separation.md). Runtime не менялся, новый smoke не требовался; порядок IP не изменён.

Пользователь дополнительно уточнил общий silhouette constraint для кругового контакта: character/enemy body избегают крайнего вытяжения и чрезмерно длинных выступающих частей, но не обязаны быть круглыми. Правило синхронизировано в Game Design, Art Direction generation/review, Art Production, ASSET_PIPELINE §22, DECISION-0039 и acceptance IP-20/IP-22. Это art-authoring ограничение без runtime-изменений; новый smoke не требовался, порядок IP не изменён.

После gameplay-просмотра пользователь поручил сделать ground shadow немного больше и зависимой от ширины персонажа, а death dust — меньше и земляного цвета. Shadow width теперь один раз вычисляется как authored contact diameter × 1.2, height увеличена до 0.24, fallback width — до 0.86; runtime pixel analysis и per-frame work не добавлены. Dust size установлен 0.08, цвет — приглушённый коричнево-земляной. Финальный smoke: 646/646 EditMode, 25/25 PlayMode, zero skipped; пользовательский плейтест до коммита по поручению не ожидался. Порядок IP не изменён.

### Pickup art and drop-scatter follow-up — 2026-09-22

По поручению пользователя сгенерированы и подключены XP crystal, лечебное зелье и Traveler Book: три `SpriteRole.Pickup` runtime derivatives 256×256 с единым дешёвым visual-only bob/pulse. XP и world pickups получают отдельный seeded-разброс радиусом 0.30 world units вокруг source point; scatter RNG не расходует chance RNG, а Potion/Book после смещения сохраняют reachable placement. Финальный smoke: **651/651 EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. Изображения и их текущая gameplay-scale подача приняты пользователем 2026-09-22; статусы IP и Execution order не изменены. [DECISION-0043](../decisions/0043-seeded-drop-scatter.md), [pipeline](../art/ASSET_PIPELINE.md#26-pickup-sprites-bobpulse-и-разброс-drops), [evidence](evidence/2026-09-22-pickup-art-and-scatter.md).

Последующий gameplay-feedback: XP и Зелье было трудно подбирать. Fixture XP pickup radius увеличен `0.20 → 0.50`, а общий contact radius Potion/Book — `0.22 → 0.40` world units. Визуальный размер, scatter и формулы modifiers не менялись; пользовательская повторная оценка ощущения открыта.

По следующему поручению подготовлен минимальный visual kit FIELD-001: tiled земля, плетень, пень, куст и трава. Один data-driven visual-only runtime накрывает существующую fixture-геометрию, не меняет colliders и seeded-расставляет редкий decor без physics; restart/shutdown очищает visual root и восстанавливает placeholder. Проверки: **651/651 Game.* EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. Пять изображений приняты пользователем в игре 2026-09-22; production geometry/metadata/thumbnail и статус IP-23 не меняются. [DECISION-0044](../decisions/0044-field-environment-art-is-presentation-only.md), [pipeline](../art/ASSET_PIPELINE.md#27-минимальный-environment-kit-и-visual-only-fixture-binding), [evidence](evidence/2026-09-22-field001-environment-art.md).

Gameplay review выявил недостаточную плотность FIELD-001: boundary-плетень находился у края 200×200, дополнительный пень не создавался, decor встречался реже одного объекта на экран. По поручению пользователя fixture теперь создаёт 64 внутренних player-only obstacles (16 рядом со стартом), уплотняет visual decor и передаёт obstacle bounds в pickup/Traveler placement. Final Rush regular cap увеличен `24 → 200`; boss/Traveler и прочие GameObjects в него не входят. Проверки: **651/651 Game.* EditMode, 25/25 PlayMode, zero skipped**; 200-enemy spawn/pool benchmark (10 cycles): cold **20.182 ms**, warm max **2.717 ms**. Production IP-23 не объявляется завершённым. [DECISION-0045](../decisions/0045-field-density-and-200-enemy-cap.md), [evidence](evidence/2026-09-22-field001-environment-art.md#density-revision-after-gameplay-review).

Следующий gameplay review ограничил fixture-камень дальностью 5 world units (скорость 10 × lifetime 0.5 s, половина reference screen height) на всех уровнях и оставил внутренний плетень только горизонтальным. Остальные projectiles и obstacle geometry не менялись. Проверки: **652/652 Game.* EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. [DECISION-0046](../decisions/0046-stone-range-and-horizontal-fences.md), [projectile evidence](evidence/2026-09-22-projectile-art.md), [field evidence](evidence/2026-09-22-field001-environment-art.md#density-revision-after-gameplay-review).

### Skill icon art follow-up — 2026-09-22

Пользователь утвердил полный набор из 16 иконок `SKILL-001…016`. Для каждой сохранены immutable candidate/master, prompt/provenance и runtime import; все зарегистрированы как `SpriteRole.Icon`. Тринадцать существующих fixture-навыков временно ссылаются на механически соответствующие production icons, поэтому draft и occupied active slots в HUD/Pause Build показывают их через общий registry. `SKILL-002`, `SKILL-013` и `SKILL-015` импортированы без ложного fixture mapping. Проверки: manifest **41/41**, Unity 6000.6.0f1 **653/653 Game.* EditMode, 25/25 PlayMode, zero skipped**. Изображения approved; текущая UI-читаемость 13 подключённых иконок принята пользователем 2026-09-22. Production definitions/binding и UI review трёх неподключённых иконок остаются в IP-17, его статус и Execution order не изменены. [DECISION-0047](../decisions/0047-skill-icon-fixture-mapping.md), [pipeline](../art/ASSET_PIPELINE.md#28-пакет-ui-иконок-навыков), [evidence](evidence/2026-09-22-skill-icons.md).

### Passive and set icon art follow-up — 2026-09-22

Пользователь утвердил полный представленный набор: 14 иконок `PASSIVE-001…014` и 20 иконок `SET-001…020`. Для каждой сохранены immutable candidate/master, prompt/provenance и runtime import; все 34 зарегистрированы как `SpriteRole.Icon`. Девять fixture-пассивок и четыре fixture-сета с ясным механическим соответствием получили typed icon references и показываются через общий registry в draft/build slots и acquired-set rows. Остальные изображения импортированы без ложного fixture mapping. Проверки: manifest **75/75**, Unity 6000.6.0f1 **655/655 Game.* EditMode, 25/25 PlayMode, zero skipped**. Изображения approved; текущая UI-читаемость девяти подключённых пассивок и четырёх сетов принята пользователем 2026-09-22. Production definitions/binding и UI review неподключённых иконок остаются в IP-18/IP-19, их статус и Execution order не изменены. [DECISION-0048](../decisions/0048-passive-and-set-icon-fixture-mapping.md), [pipeline](../art/ASSET_PIPELINE.md#29-пакеты-ui-иконок-пассивок-и-сетов), [evidence](evidence/2026-09-22-passive-and-set-icons.md).

### Skill world-art follow-up — 2026-09-22

Пользователь утвердил четыре world candidates: одиночный орбитальный клинок `SKILL-003`, бумеранг `SKILL-006`, рикошетный диск `SKILL-008` и взрывную сферу `SKILL-014`. Для каждого сохранены immutable candidate/master, prompt/provenance и 256×256 runtime derivative; corresponding fixture levels наследуют typed `SpriteRole.Projectile` reference. Орбитальные клинки используют pooled visual-only renderers под owner transform; остальные — общий projectile child. Сфера запускает reusable particle-only explosion burst без отдельного raster и без задержки damage. Static checks: raster alpha/padding PASS, manifest **83/83**. После исправления двух неоднозначных `Object` cleanup calls Unity завершил импорт и проверил **658/658 Game.* EditMode, 0 skipped** на Unity 6000.6.0f1; Последующий tooling smoke подтвердил **25/25 PlayMode**, 0 skipped ([evidence](evidence/2026-09-22-workflow-tools.md)). Статусы IP-17 и Execution order не изменены; UI icon slots и gameplay-scale world presentation приняты пользователем 2026-09-22; автоматический smoke подтверждён последующим общим прогоном. [pipeline](../art/ASSET_PIPELINE.md#30-world-art-для-орбитального-клинка-бумеранга-рикошетного-диска-и-взрывной-сферы), [evidence](evidence/2026-09-22-skill-world-art.md).

### Current in-game visual acceptance — 2026-09-22

Пользователь сообщил: «Если что, я посмотрел всё, что в игре, можно считать окей, запрувлено.» Приняты текущий интерфейс и размеры иконок, подключённые skill/passive/set icons, world sprites, взрыв сферы, body/projectile/pickup/shadow и окружение FIELD-001. Визуальная приёмка текущей реализации закрыта; увеличение иконок не требуется. Исходная цитата и границы evidence: [плейтест](../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят). Manifest/provenance и affected evidence синхронизированы. Production definitions, отсутствующие gameplay-привязки и проверки с неуказанными условиями сохраняют свои gates; автоматический PlayMode smoke нового world-art подтверждён последующим tooling-прогоном: **25/25**, 0 skipped ([evidence](evidence/2026-09-22-workflow-tools.md)). Изменены только документы и approval metadata; runtime/PNG/UI layout и Execution order не менялись.
