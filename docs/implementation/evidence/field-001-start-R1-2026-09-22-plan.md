# FIELD-001 initial slice — design approval и подготовка плана

Дата: 2026-09-22. Scope: только документационная ревизия `field-001-start-R1`.
Основание: пользователь «апрувлю изменения» и «Составляй план», ограничив
его исходно открытым контентом. [DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md)
финализирован; [DECISION-0051](../../decisions/0051-field001-initial-slice.md)
фиксирует границу; [спецификация этапа](../milestones/FIELD-001-start.md).
Текущие статусы и очередь — только [STATUS](../STATUS.md#field001-execution).

## Read-only audit

- `Assets/Resources/Content/Meta/MetaEconomy.json` до реализации нового mapping:
  SKILL-008/PASSIVE-013/SET-002 имеют initial; SKILL-016 — FIELD-001;
  CHAR-002 — firstRun за 100. Это не соответствует новому approved mapping,
  поэтому прежние tests не подтверждают новую редакцию дизайна.
- `Assets/Game/Bootstrap/FixtureRuntimeContentCatalog.cs` загружает FixtureActiveSkills,
  FixturePassives, FixtureSets, FixtureEnemies/Bosses/Waves/Characters и fixture
  presentation. Готовые изображения не означают уже поставленные production IDs.
- CD явно оставляет required balance/encounter fields незаполненными: contact
  intervals, boss/mid-boss timings/rewards, Traveler lifetime/XP/support, set
  thresholds/effect values и Wave / Encounter Content. Для Book отсутствует
  production card/ID. Они включены в F1-00, не заполнены предположениями.
- На момент исходного аудита ENEMY-001…004 имели контекст FIELD-001; BOSS-001/MIDBOSS-001 связаны с ним
  канонически. TRAVELER-001/002/005 и initial 10/10/5 подтверждены DECISION-0050.

## Документационная проверка

Статическим Python-разбором проверено, все перечисленные проверки PASS:

- Все 16 skill, 14 passive, 20 set unlock entries уникальны и полны; стартовые
  10/10 дают ровно SET-001/004/006/010/017 по неизменённым 20 рецептам.
- Четыре unlock стадии остаются 11/11/10, 13/13/13, 14/14/18, 16/14/20.
  Gameplay поздними IDs отсутствует в scope этапа.
- Все десять F1 packet IDs присутствуют в scope и единственной очереди STATUS;
  prerequisite graph без циклов, queue следует dependencies, ссылки владельцев
  двусторонние. Только подготовка данных не требует готового production content.
- Локальные ссылки/явные anchors существуют; у спецификаций нет собственного
  execution status. Approved wording заменяет Proposed для DECISION-0050;
  старый mapping сохранён лишь как история решения.
- `git diff --check` для изменённых design/plan документов.

Фактический результат: 50 unlock entries, 20 неизменённых рецептов, 5 ступеней
открытий; 10 packet definitions и совпадающие prerequisites в STATUS; граф
ацикличен, очередь топологически корректна. Проверены 758 локальных ссылок и
новые explicit anchors. Общие статусы IP-25/26 пересчитаны только для новой
target delta; execution-статусов в milestone/спецификациях нет.

Runtime, JSON, C#, PNG и import settings этим заданием не менялись. Unity tests,
build и gameplay-плейтесты не запускались: они не доказывают корректность одной
редактуры плана и будут выполнены для реализованных пакетов по их acceptance.
Чужие существующие изменения `.meta` не входят в эту работу.

## Поправка пула обычных врагов — 2026-09-22

По [DECISION-0052](../../decisions/0052-field001-six-ordinary-enemies.md) пул
расширен до ENEMY-001…005 и ENEMY-007. CD contexts/pool, F1-00/04/08/09,
IP-20/23/24, STATUS и Art Production согласованы. Отложены ENEMY-006 и
ENEMY-008…020. Никаких runtime/asset изменений; recipe/unlock scope неизменён.

## Проверка перед коммитом — 2026-09-22

Проверен весь незакоммиченный набор: 29 Markdown-документов и девять Unity `.meta`.
`scripts/check_project.py --scope docs --paths <все 29 документов>`: STATIC PASS.
Повторная семантическая проверка: 50 unlock entries, 20 неизменённых рецептов,
пять ступеней открытий, шесть ordinary enemy IDs, полный непересекающийся enemy
backlog, десять packets с согласованными ацикличными prerequisites, 769 локальных
ссылок и explicit anchors — PASS. Исправлен список blockers IP-27: добавлены
переоткрытые IP-25/26; recipe/content scope не менялся.

Для каждого из девяти `.meta` сравнение с HEAD после удаления только конечных
пробелов дало полное совпадение; GUID и import settings сохранены. Это Unity
serialization whitespace, не изменение арта или импорта. Manifest validator:
83/83 owner/role records PASS; качество пикселей этой проверкой не оценивалось.
Изменений C#/gameplay JSON/PNG нет. Новый Unity run не нужен для семантически
неизменных `.meta` и документации; прежние runtime tests не объявляются новыми.
