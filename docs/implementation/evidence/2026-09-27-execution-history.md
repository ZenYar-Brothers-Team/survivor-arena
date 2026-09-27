# История разрешений и проверок — снимок 2026-09-27

Датированные записи, вынесенные из STATUS при структурном рефакторинге.
Формулировки о состоянии и границах относятся к моменту соответствующего события;
это не текущие статусы, очередь или новое разрешение продолжать работу.
Актуальные границы, gates и порядок — только в [STATUS](../STATUS.md).
Прежние проверки не подтверждают изменённый код.

## История поручений

M-01: зарегистрирован принятый план и выполнена полная замена трёх design bodies без архивных копий старых документов; [DECISION-0015](../../decisions/0015-design-sync-r2.md). Код не изменён. Исторические tests не подтверждают новые требования. Все пять источников/121 target card approved; реальные missing data/semantics/assets gates сохраняются.

Подробности регистрации: [M-01 evidence](../evidence/design-sync-R2-2026-09-21.md#m-01).

## Утверждённый стартовый этап — 2026-09-22

Пользователь утвердил [DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md)
и поручил составить план только исходно открытого контента. [DECISION-0051](../../decisions/0051-field001-initial-slice.md)
и [FIELD-001 scope](../milestones/FIELD-001-start.md) фиксируют 10 skills / 10 passives /
5 sets, CHAR-001, ENEMY-001…005 и ENEMY-007, BOSS-001/MIDBOSS-001, TRAVELER-001/002/005,
PICKUP-001/Book и поле с production schedule. Остальной контент сохраняется в IP.
Поздние глобальные unlocks утверждены, но gameplay ими не входит в этот этап.
Поправка пользователя: [DECISION-0052](../../decisions/0052-field001-six-ordinary-enemies.md)
расширяет ordinary pool до шести за счёт ENEMY-005/007; F1-00/04/08/09 и art scope
синхронизированы. Новых runtime checks нет; очередь и зависимости сохраняются.

Изменён только дизайн/план. Runtime MetaEconomy.json переведён на DECISION-0050
в F1-03 (2026-09-24) вместе с миграцией при загрузке. IP-25/26 переоткрыты для новой delta;
их прежнее evidence сохранено как база. Остальные framework scope без изменения
поведения сохраняют своё состояние; production catalogs имеют прежние data/art
и новые packet dependencies. Approved art не означает production bindings.

Документационные проверки и read-only audit: [evidence](../evidence/field-001-start-R1-2026-09-22-plan.md).

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

Подготовлены [baseline v1](../../balance/field001-baseline-v1.md) и
[численные таблицы](../../balance/field001-baseline-v1.json),
[DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md).
F1-00 остаётся In progress до approval конкретной v1: 60 skill levels,
60 passive levels, 5 set recipes/effects, 6 ordinary, bosses/Travelers/pickups,
24 фазы, geometry и performance targets. API/art gaps явно закреплены за
реализующими packet owners; production IDs ещё не реализованы. Статическая проверка
и границы evidence — [F1-00 evidence](../evidence/field001-baseline-v1-2026-09-23.md).
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
([evidence](../evidence/field001-baseline-v1-2026-09-23.md#approval-2026-09-24)).

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

IP-31 проверен automated checks и реальным ручным run с report/feedback 2026-09-21. Наблюдение пользователя «опыт стреляет» сохранено в [записи прогона, OBS-01](../../playtests/2026-09-21_108ff5b3.md#obs-01--опыт-визуально-воспринимается-как-стреляющий-объект) для отдельной диагностики; причина не установлена. Пользователь одобрил хранение обработанных отзывов и выбранных reports в docs/playtests; процесс и шаблон синхронизированы. По последующему разрешению пользователя IP-32 завершён: реальный review с insufficient-evidence/no-change и synthetic accept/apply/rollback проверены. OBS-01 открыт; tuning не разрешён. Последующий явный запрос пользователя «реализуй следующий пункт» разрешил IP-11.

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
[DECISION-0033](../../decisions/0033-world-pickup-rules.md) Approved. GDD/CD, IP-28
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
Следующий модуль автоматически не начинать. [Evidence](../evidence/design-sync-R2-2026-09-21-traveler-preparation.md).
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
[DECISION-0038](../../decisions/0038-settings-and-field-difficulty.md) Approved:
G-16/G-20 resolved, CD/UI/GDD и IP-16/23/26 синхронизированы. IP-26 Ready:
все зависимости Verified целевой ревизии либо IP-12A Implemented (допустимо по
WORKFLOW); его gameplay density review сохраняется отдельно. Код не менялся,
settings implementation/новые runtime checks ещё не выполнялись. Это подготовка
правил, не разрешение автоматически начинать следующий IP.

Запрос «продолжай» разрешил реализацию IP-26 по DECISION-0038. Дальнейшие IP автоматически не начинать.

Отдельный пользовательский art packet 2026-09-22 добавил утверждённые projectile images: камень SKILL-001 подключён к `FIXTURE-SKILL-BOLT`, письмо — только к ranged `FIXTURE-ENEMY-FAN`. Общий data-driven presentation сохраняет круглый collider, вращает только visual child камня и даёт короткий pooled flash + material particles. Проверки: **647/647 Game.* EditMode, 25/25 PlayMode, 0 skipped**, Unity 6000.6.0f1. Это не регистрирует production SKILL-001 и не меняет approved melee card ENEMY-002; статусы IP-17/IP-20 сохраняются. [Evidence](../evidence/2026-09-22-projectile-art.md).

## История технических и визуальных проверок

Workflow tooling follow-up, 2026-09-22: Tooling packet: Implemented. Подготовка approved art и numeric-only visual-preview работают через общие команды; 18/18 tooling tests, реальный безопасный batch smoke **658/658 EditMode + 25/25 PlayMode**, 0 skipped, manifest 83 records. Повторное использование совпадающего evidence проверено; live REST остаётся непроверенным на открытом Editor (protocol/mode/filter flow покрыт изолированными тестами). [DECISION-0049](../../decisions/0049-art-workflow-automation-and-preview.md), [команды](../../../scripts/README.md), [evidence](../evidence/2026-09-22-workflow-tools.md). Обязательные acceptance IP, Execution order и Next Ready сохранены.



### Contact review follow-up — 2026-09-22

Пользователь принял максимальные вписанные круги goblin/villager и поручил закрепить метод как этап пайплайна. Radius 0.401431 / 0.330282, centerY 0.530976 / 0.469539; [ASSET_PIPELINE §22](../../art/ASSET_PIPELINE.md#22-подгонка-круга-контакта-для-world-body), [DECISION-0039](../../decisions/0039-conservative-body-contact-circles.md), [evidence](../evidence/2026-09-22-body-contact-circles.md#third-trial--maximum-inscribed-circles). Финальная runtime ревизия: 644/644 EditMode, 24/24 PlayMode, zero skipped. Последующее закрепление пайплайна меняет только документы. Общий gameplay/density gate IP-12A открыт; порядок IP не изменён.

### Enemy death presentation follow-up — 2026-09-22

По явному поручению пользователя мгновенное исчезновение заменено единым procedural tail для ordinary/boss/Traveler: squash, shrink/fade и dust; без специальных веток и без death push. Gameplay death/reward/untargeting остаются мгновенными, pool return задержан на 0.30 s и замораживается pause. Финальная проверка: 644/644 EditMode, 25/25 PlayMode, zero skipped. [DECISION-0040](../../decisions/0040-shared-enemy-death-presentation.md), [pipeline](../../art/ASSET_PIPELINE.md#23-единая-процедурная-смерть-врагов), [evidence](../evidence/2026-09-22-shared-enemy-death.md). Порядок IP не изменён.

Последующий пользовательский плейтест выявил невидимый death clone/pooled animated body. Порядок snapshot и восстановление renderer исправлены, regression обновлён. Пользователь повторно проверил Gameplay и принял результат 2026-09-22: «Сейчас выглядит хорошо», разрешил коммит и подтвердил запись полной процедуры. Финальный post-fix smoke: 644/644 EditMode, 25/25 PlayMode, zero skipped; подробности в evidence.

### Courier and ground-shadow art follow-up — 2026-09-22

По поручению выполнить art-пункты 1–3 вместе сгенерирован ENEMY-002 и подключён к неизменённому `FIXTURE-ENEMY-FAN` с отдельным быстрым motion profile. Пользователь отклонил v001 как испуганного и слишком похожего на playable goblin; v002 переделан в уверенного взрослого человеческого преследователя с отдельными пропорциями, позой и формой. Runtime 256×256; максимальный вписанный contact circle v002: radius 0.360855, centerY 0.453097. Player, ordinary enemies, bosses и Travelers используют один JSON-профиль ground shadow и общую процедурную 32×32 mask без отдельных PNG и physics. Финальный v002 smoke: 646/646 EditMode, 25/25 PlayMode, zero skipped. [DECISION-0041](../../decisions/0041-shared-procedural-ground-shadows.md), [pipeline](../../art/ASSET_PIPELINE.md#24-единая-процедурная-ground-shadow), [evidence](../evidence/2026-09-22-courier-and-ground-shadows.md). Текущий ENEMY-002 v002 и тени приняты пользователем 2026-09-22; production binding не выполнен; статусы IP и Execution order не изменены.

Последующее поручение синхронизировало правило с enemy-документами: Game Design, общий раздел Enemies и карточка ENEMY-002 в Content Design, Art Direction §8/generation/review и IP-20 теперь требуют отделять enemy identity от playable минимум по silhouette/proportions, posture/expression и costume/palette mass. Уверенный преследователь ENEMY-002 v002 является первым эталоном; [DECISION-0042](../../decisions/0042-enemy-player-visual-separation.md). Runtime не менялся, новый smoke не требовался; порядок IP не изменён.

Пользователь дополнительно уточнил общий silhouette constraint для кругового контакта: character/enemy body избегают крайнего вытяжения и чрезмерно длинных выступающих частей, но не обязаны быть круглыми. Правило синхронизировано в Game Design, Art Direction generation/review, Art Production, ASSET_PIPELINE §22, DECISION-0039 и acceptance IP-20/IP-22. Это art-authoring ограничение без runtime-изменений; новый smoke не требовался, порядок IP не изменён.

После gameplay-просмотра пользователь поручил сделать ground shadow немного больше и зависимой от ширины персонажа, а death dust — меньше и земляного цвета. Shadow width теперь один раз вычисляется как authored contact diameter × 1.2, height увеличена до 0.24, fallback width — до 0.86; runtime pixel analysis и per-frame work не добавлены. Dust size установлен 0.08, цвет — приглушённый коричнево-земляной. Финальный smoke: 646/646 EditMode, 25/25 PlayMode, zero skipped; пользовательский плейтест до коммита по поручению не ожидался. Порядок IP не изменён.

### Pickup art and drop-scatter follow-up — 2026-09-22

По поручению пользователя сгенерированы и подключены XP crystal, лечебное зелье и Traveler Book: три `SpriteRole.Pickup` runtime derivatives 256×256 с единым дешёвым visual-only bob/pulse. XP и world pickups получают отдельный seeded-разброс радиусом 0.30 world units вокруг source point; scatter RNG не расходует chance RNG, а Potion/Book после смещения сохраняют reachable placement. Финальный smoke: **651/651 EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. Изображения и их текущая gameplay-scale подача приняты пользователем 2026-09-22; статусы IP и Execution order не изменены. [DECISION-0043](../../decisions/0043-seeded-drop-scatter.md), [pipeline](../../art/ASSET_PIPELINE.md#26-pickup-sprites-bobpulse-и-разброс-drops), [evidence](../evidence/2026-09-22-pickup-art-and-scatter.md).

Последующий gameplay-feedback: XP и Зелье было трудно подбирать. Fixture XP pickup radius увеличен `0.20 → 0.50`, а общий contact radius Potion/Book — `0.22 → 0.40` world units. Визуальный размер, scatter и формулы modifiers не менялись; пользовательская повторная оценка ощущения открыта.

По следующему поручению подготовлен минимальный visual kit FIELD-001: tiled земля, плетень, пень, куст и трава. Один data-driven visual-only runtime накрывает существующую fixture-геометрию, не меняет colliders и seeded-расставляет редкий decor без physics; restart/shutdown очищает visual root и восстанавливает placeholder. Проверки: **651/651 Game.* EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. Пять изображений приняты пользователем в игре 2026-09-22; production geometry/metadata/thumbnail и статус IP-23 не меняются. [DECISION-0044](../../decisions/0044-field-environment-art-is-presentation-only.md), [pipeline](../../art/ASSET_PIPELINE.md#27-минимальный-environment-kit-и-visual-only-fixture-binding), [evidence](../evidence/2026-09-22-field001-environment-art.md).

Gameplay review выявил недостаточную плотность FIELD-001: boundary-плетень находился у края 200×200, дополнительный пень не создавался, decor встречался реже одного объекта на экран. По поручению пользователя fixture теперь создаёт 64 внутренних player-only obstacles (16 рядом со стартом), уплотняет visual decor и передаёт obstacle bounds в pickup/Traveler placement. Final Rush regular cap увеличен `24 → 200`; boss/Traveler и прочие GameObjects в него не входят. Проверки: **651/651 Game.* EditMode, 25/25 PlayMode, zero skipped**; 200-enemy spawn/pool benchmark (10 cycles): cold **20.182 ms**, warm max **2.717 ms**. Production IP-23 не объявляется завершённым. [DECISION-0045](../../decisions/0045-field-density-and-200-enemy-cap.md), [evidence](../evidence/2026-09-22-field001-environment-art.md#density-revision-after-gameplay-review).

Следующий gameplay review ограничил fixture-камень дальностью 5 world units (скорость 10 × lifetime 0.5 s, половина reference screen height) на всех уровнях и оставил внутренний плетень только горизонтальным. Остальные projectiles и obstacle geometry не менялись. Проверки: **652/652 Game.* EditMode, 25/25 PlayMode, zero skipped**, Unity 6000.6.0f1. [DECISION-0046](../../decisions/0046-stone-range-and-horizontal-fences.md), [projectile evidence](../evidence/2026-09-22-projectile-art.md), [field evidence](../evidence/2026-09-22-field001-environment-art.md#density-revision-after-gameplay-review).

### Skill icon art follow-up — 2026-09-22

Пользователь утвердил полный набор из 16 иконок `SKILL-001…016`. Для каждой сохранены immutable candidate/master, prompt/provenance и runtime import; все зарегистрированы как `SpriteRole.Icon`. Тринадцать существующих fixture-навыков временно ссылаются на механически соответствующие production icons, поэтому draft и occupied active slots в HUD/Pause Build показывают их через общий registry. `SKILL-002`, `SKILL-013` и `SKILL-015` импортированы без ложного fixture mapping. Проверки: manifest **41/41**, Unity 6000.6.0f1 **653/653 Game.* EditMode, 25/25 PlayMode, zero skipped**. Изображения approved; текущая UI-читаемость 13 подключённых иконок принята пользователем 2026-09-22. Production definitions/binding и UI review трёх неподключённых иконок остаются в IP-17, его статус и Execution order не изменены. [DECISION-0047](../../decisions/0047-skill-icon-fixture-mapping.md), [pipeline](../../art/ASSET_PIPELINE.md#28-пакет-ui-иконок-навыков), [evidence](../evidence/2026-09-22-skill-icons.md).

### Passive and set icon art follow-up — 2026-09-22

Пользователь утвердил полный представленный набор: 14 иконок `PASSIVE-001…014` и 20 иконок `SET-001…020`. Для каждой сохранены immutable candidate/master, prompt/provenance и runtime import; все 34 зарегистрированы как `SpriteRole.Icon`. Девять fixture-пассивок и четыре fixture-сета с ясным механическим соответствием получили typed icon references и показываются через общий registry в draft/build slots и acquired-set rows. Остальные изображения импортированы без ложного fixture mapping. Проверки: manifest **75/75**, Unity 6000.6.0f1 **655/655 Game.* EditMode, 25/25 PlayMode, zero skipped**. Изображения approved; текущая UI-читаемость девяти подключённых пассивок и четырёх сетов принята пользователем 2026-09-22. Production definitions/binding и UI review неподключённых иконок остаются в IP-18/IP-19, их статус и Execution order не изменены. [DECISION-0048](../../decisions/0048-passive-and-set-icon-fixture-mapping.md), [pipeline](../../art/ASSET_PIPELINE.md#29-пакеты-ui-иконок-пассивок-и-сетов), [evidence](../evidence/2026-09-22-passive-and-set-icons.md).

### Skill world-art follow-up — 2026-09-22

Пользователь утвердил четыре world candidates: одиночный орбитальный клинок `SKILL-003`, бумеранг `SKILL-006`, рикошетный диск `SKILL-008` и взрывную сферу `SKILL-014`. Для каждого сохранены immutable candidate/master, prompt/provenance и 256×256 runtime derivative; corresponding fixture levels наследуют typed `SpriteRole.Projectile` reference. Орбитальные клинки используют pooled visual-only renderers под owner transform; остальные — общий projectile child. Сфера запускает reusable particle-only explosion burst без отдельного raster и без задержки damage. Static checks: raster alpha/padding PASS, manifest **83/83**. После исправления двух неоднозначных `Object` cleanup calls Unity завершил импорт и проверил **658/658 Game.* EditMode, 0 skipped** на Unity 6000.6.0f1; Последующий tooling smoke подтвердил **25/25 PlayMode**, 0 skipped ([evidence](../evidence/2026-09-22-workflow-tools.md)). Статусы IP-17 и Execution order не изменены; UI icon slots и gameplay-scale world presentation приняты пользователем 2026-09-22; автоматический smoke подтверждён последующим общим прогоном. [pipeline](../../art/ASSET_PIPELINE.md#30-world-art-для-орбитального-клинка-бумеранга-рикошетного-диска-и-взрывной-сферы), [evidence](../evidence/2026-09-22-skill-world-art.md).

### Current in-game visual acceptance — 2026-09-22

Пользователь сообщил: «Если что, я посмотрел всё, что в игре, можно считать окей, запрувлено.» Приняты текущий интерфейс и размеры иконок, подключённые skill/passive/set icons, world sprites, взрыв сферы, body/projectile/pickup/shadow и окружение FIELD-001. Визуальная приёмка текущей реализации закрыта; увеличение иконок не требуется. Исходная цитата и границы evidence: [плейтест](../../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят). Manifest/provenance и affected evidence синхронизированы. Production definitions, отсутствующие gameplay-привязки и проверки с неуказанными условиями сохраняют свои gates; автоматический PlayMode smoke нового world-art подтверждён последующим tooling-прогоном: **25/25**, 0 skipped ([evidence](../evidence/2026-09-22-workflow-tools.md)). Изменены только документы и approval metadata; runtime/PNG/UI layout и Execution order не менялись.

### Traveler placement performance fix — 2026-09-24

Журнал Editor показал повторяющиеся `Pickup.ReachablePlacement` по 29–36 мс во время движения Путника: каждый кадр заново строилась сетка достижимости поля с 64 внутренними препятствиями. Для перемещения от уже достижимой точки к свободной точке без пересечения препятствий добавлена прямая проверка сегмента; перекрытый путь по-прежнему использует прежнюю полную проекцию. Это техническое исправление без изменения правил движения и поддержки. Регрессионные тесты: **714/714 Game.* EditMode**, **27/27 PlayMode**, zero skipped, Unity 6000.6.0f1. Статусы IP и Execution order не менялись.

Dev-панель 2026-09-26 (поручение пользователя): `+100 XP`, `+100 rerolls` и «Unlock all skills/passives/sets (run)» — только текущий забег, профиль не меняется; Unity 768/768 + 27/27. [Evidence](../evidence/2026-09-26-late-skills-passives.md#dev-панель-поручение-пользователя-2026-09-26). Статусы IP и Execution order не менялись.

### Procedural walk animation follow-up — 2026-09-26

По запросу пользователя ("нормальные анимации вместо покачиваний") `ProceduralSpriteAnimator` переработан без изменения `SpriteMotionProfile`, JSON-схемы или content-файлов: частота шага теперь масштабируется фактической скоростью, вертикальный bob/stretch синхронизирован с footfall (`|sin|^0.6`, тело только приподнимается, не проваливается ниже базовой линии), добавлен боковой weight-shift того же step-цикла (производный от существующего `BobAmplitude`), а directional lean сглажен точным экспоненциальным приближением (`exp(-18·dt)`), сохраняющим frame-rate independence по построению. Idle breathing/sway не тронуты. `Y`-offset и scale остаются функцией только `velocity.magnitude`, hit/spawn не менялись. Регрессия: **727/727 Game.* EditMode, 27/27 PlayMode**, zero skipped, Unity 6000.6.0f1, manifest 103/103. [Evidence](../evidence/2026-09-26-procedural-walk-animation.md). IP-12A остаётся Verified; это same-scope quality follow-up, статусы IP и Execution order не менялись. Пользовательский gameplay-scale review (Showcase `Left`/`Right`/`Live`) остаётся открытым.

<a id="field-001-follow-ups"></a>
## FIELD-001 follow-ups

F1-00…08 автопроверки PASS; новые изображения приняты 2026-09-24. 2026-09-25: отзывы двух прогонов 2026-09-24 обработаны — 9 OBS исправлено, 2 ждут повторной проверки, 1 отложен; изменены PASSIVE-007/ENEMY-005/ENEMY-007 ([DECISION-0055](../../decisions/0055-playtest-2026-09-24-fixes.md), [evidence](../evidence/field001-f1-09-playtest-fixes-2026-09-25.md)); Unity full PASS 2026-09-25 (EditMode 718/718, PlayMode 27/27); нужен повторный прогон для визуальной проверки. 2026-09-25: критический bugfix — исключение в `FixedUpdate` при взрыве снаряда после попадания (два `ParticleSystem` на одном root), [evidence](../evidence/2026-09-25-projectile-particles-crash.md), Unity full PASS (EditMode 727/727, PlayMode 27/27); в игре пользователь подтвердил исчезновение просадки («кажется пофиксилось»), без FPS-замеров. 2026-09-25: perf-фикс `Pickup.ReachablePlacement`/orbit area damage ([evidence](../evidence/2026-09-25-perf-placement-orbit.md)); слой врагов — [DECISION-0056](../../decisions/0056-enemy-physics-layer.md), см. ниже; Unity full PASS (EditMode 726/726, PlayMode 27/27), время в игре ещё не перепроверено. 2026-09-25: пять замечаний прогона `5233a664` исправлены — стартовый спавн у края экрана, свежие draft и wave seeds на run, иконка сета в паузе, опыт 45 s, без линии рывка гончей; боссы и путники снова получают свой арт вместо цветных квадратов ([DECISION-0057](../../decisions/0057-playtest-2026-09-25-fixes.md)); Unity full PASS 2026-09-25 (EditMode 741/741, PlayMode 27/27, `TestResults/checks/20260925T183434-473761Z/summary.json`); «опыт стреляет» (`108ff5b3` OBS-01) не воспроизводится по словам пользователя; нужен повторный прогон. 2026-09-25: атаки игрока выбирают цели/точки только на экране, без цели — предыдущее направление или случайная точка экрана; удар с небес — столб света раньше вспышки, круг и область урона сжаты по вертикали до 0.7 ([DECISION-0058](../../decisions/0058-on-screen-targeting-and-strike-visual.md)); Unity full PASS (EditMode 749/749, PlayMode 27/27, `TestResults/checks/20260925T194642-943680Z/summary.json`). 2026-09-25: путник-призрак (исключение при спавне из-за потери арта в `TravelerDefinition.Scale`) исправлен, PASSIVE-007 L3–L6 ослаблен (L6 1.665 units вместо 2.5), BOSS-001 телепортируется с telegraph и ударом, если игрок 5 s дальше 5 units ([DECISION-0059](../../decisions/0059-playtest-2026-09-25-evening-fixes.md)); приземление — случайная точка окружности вокруг игрока; Unity full PASS 2026-09-25 (EditMode 755/755, PlayMode 27/27, `TestResults/checks/20260925T204529-715654Z/summary.json`). 2026-09-26: DECISION-0056 утверждён пользователем — враги на отдельном физическом слое `Enemy`, пять area-запросов урона/ауры фильтруют только его (XP, снаряды, pickups и стены больше не проходят через поиск компонентов); Unity full PASS (EditMode 757/757, PlayMode 27/27, `TestResults/checks/20260926T062226-949358Z/summary.json`); в игре пользователь проверил: «проверил, всё хорошо» (2026-09-26), без замеров времени/FPS. 2026-09-26: пользователь прошёл быстрый забег без замечаний («проверил быстрым забегом, но мы можем идти дальше»); экспорт не сохранён, матрица этим не закрыта, приёмки F1-09 нет; пользователь разрешил идти дальше по плану, F1-09 остаётся открытым. 2026-09-26: по отзыву пользователя телепорт финального босса (BOSS-001 и BOSS-002) срабатывает через 2 s вместо 5, приземление 1.5 units от игрока, круг удара 3.5 — от удара нельзя уклониться на базовой скорости ([DECISION-0059](../../decisions/0059-playtest-2026-09-25-evening-fixes.md), пересмотр 2026-09-26); лечение регенерацией в телеметрии помечено источником `Regeneration` вместо `unknown`; EditMode затронутых тестов PASS 33/33 (`TestResults/checks/20260926T194450-704495Z/summary.json`); Unity full PASS (EditMode 806/806, PlayMode 28/28, `TestResults/checks/20260926T194552-432817Z/summary.json`); в игре не проверено. 2026-09-26: по отзыву пользователя ослаблены SKILL-014 (взрыв L1–L2 32 → 26, L3–L6 41.6 → 33.8) и SKILL-007 (cooldown 2.2 → 2.86 s) ([DECISION-0065](../../decisions/0065-skill-014-explosion-nerf.md)); Unity full PASS (EditMode 812/812, PlayMode 28/28, `TestResults/checks/20260926T203114-698286Z/summary.json`); в игре не проверено. Остаются реальные прогоны по матрице, performance bounds и приёмка ощущения карты. [Подготовка/матрица NOT RUN](../evidence/field001-f1-09-2026-09-24.md), [art review](../../playtests/2026-09-24_field001-art-acceptance.md)
