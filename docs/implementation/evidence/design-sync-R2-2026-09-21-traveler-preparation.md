# Traveler rules и размер fixture arena

Date: 2026-09-21. Execution status/order: only [STATUS](../STATUS.md).

## Scope

Пользователь задал два экрана spawn distance, random types без повторов, последний
spawn за две минуты до конца и поручил выбрать HP/damage formula и простые effects.
Следующая команда разрешила сделать текущую арену около 20 экранов.
[DECISION-0035](../../decisions/0035-traveler-encounter-rules.md) фиксирует правила,
формулу с примером и явно озвученное толкование единицы: полная высота viewport.
GDD/CD, DESIGN_SYNC, IP-24/IP-29/IP-30 и readiness синхронизированы.

## Arena implementation

`FixtureArenaGeometry.json`: 20 screen heights × reference height 10 = 200 world
units по каждой стороне; wall thickness 0.5. Все три поля required/positive/finite,
результат произведения тоже валидируется. DTO и FixtureArenaGeometryCatalog находятся
в Game.Field; конфигурация используется editor baker и scene integration tests.

`FixtureArenaGeometryBaker.Bake` читает JSON и через Unity Editor API меняет четыре
wall positions/collider sizes в Gameplay scene. Проверяет orthographic camera reference
height; Menu Game/Fixtures/Bake Arena Geometry или batch executeMethod. Размер
запечён и доступен в сохранённой сцене до Start; runtime initialization не требуется.
Collider thickness/masks, obstacle, player spawn, camera zoom и wave counts сохранены.
Оба fixture fields используют эту же scene/environment.

Unity при SaveScene переупорядочил serialized object blocks и удалил старые поля,
которых уже нет в MonoBehaviour types. Сверка по fileID: добавленных/удалённых
объектов нет; геометрические изменения — четыре wall transforms и четыре colliders.
Новые raster assets не создавались, существующие sprite bindings не менялись.

## Verification

Перед bake и каждым тестовым запуском свежая Get-CimInstance проверка: Unity не
запущен. Unity 6000.6.0f1, batch -nographics, `scripts/Test-Unity.ps1`, filter `^Game\.`.
Финальные XML: **571/571 Game.* EditMode, 14/14 PlayMode, 0 failed, 0 skipped**,
third-party tests 0. Exit codes 0.

- GameplaySceneIntegrationTests проверяет 20 viewport heights, соответствие camera,
  сохранённые стены/colliders/маски и obstacle.
- WorldPickupSmokeTests теперь получает фактическую границу из wall collider;
  outside drop и reachable placement проверяются на увеличенной арене.
- Полный suite включает run, damage/death, XP/draft, skills, waves/enemy pool,
  UI/content/composition и GameplaySmokeTests.

Локальные артефакты: `TestResults/ArenaBake.log`,
`TestResults/Arena20-2026-09-21-EditMode.xml/.log`,
`TestResults/Arena20-2026-09-21-PlayMode.xml/.log`.
JSON/meta/link checks и `git diff --check` выполнены.

Это проверка изменения арены и существующей игры, не подтверждение Traveler runtime.
Production values/art и IP-12A gameplay density review не закрываются этим прогоном.
