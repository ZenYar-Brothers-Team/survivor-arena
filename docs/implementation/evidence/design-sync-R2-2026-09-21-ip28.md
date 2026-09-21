# IP-28

Date: 2026-09-21. Scope revision: design-sync-R2.
Execution status/order: only [STATUS](../STATUS.md).

## Implementation

- `Assets/Game/Pickup/`: validating definitions/JSON catalog, explicit life states,
  drop/run/source identity, ordinary-death chance policy, pooled world runtime,
  Health/Book/set reward target и reachable-placement boundary.
- `Assets/Resources/Content/Pickups/FixturePickups.json`: synthetic potion/Book,
  global chance 0.08, heal 20 HP, contact radius 0.22, no default expiry, seed и
  text-only marker parameters. Эти значения не являются production balance.
- Bootstrap fanout сохраняет XP rewards и независимо подключает potion death sink;
  geometry adapter читает существующую сцену; initialization rollback и Shutdown
  включают новый owner. Fixture registry валидирует definitions/override references.
- UI: PickupPresenter, immutable snapshot, HUD actual-heal/Book feedback, fixture
  commands/counters в development Build tab, release gating. HP/Book heading используют
  существующие surfaces. Telemetry: resolved events/counters, source/drop/run IDs,
  attempted/actual healing, content provenance.

API/schema, units, ordering и geometry limitation описаны в
[IP-28](../modules/IP-28-world-pickups.md#framework-api-и-fixture-schema).

## Checks

Runner: `scripts/Test-Unity.ps1`, Unity **6000.6.0f1**, batch `-nographics`.
Перед каждым запуском свежая Get-CimInstance проверка: Unity Editor отсутствовал.
Первый targeted EditMode `^Game\.Pickup\.`: 26/26. После добавления полного
coverage финальный filter `^Game\.`: **571/571 EditMode, 14/14 PlayMode,
0 failed, 0 skipped**. Third-party tests: 0. XML counts прочитаны, exit codes 0.
Локальные полные результаты:

- `TestResults/IP28-2026-09-21-EditMode.xml` и `.log`
- `TestResults/IP28-2026-09-21-PlayMode.xml` и `.log`

Coverage:

- PickupLifeTests: exactly-once/reentrancy, отказ/исключение callback, optional
  running-time lifetime, chance precedence/explicit zero/cap/endpoints, required
  JSON/typed references и invalid multipliers.
- BoxPickupPlacementTests: outside arena, inside obstacle, unchanged free point,
  disconnected region — результат в доступной игроку компоненте.
- WorldPickupRuntimeTests: actual restoration/max-HP/full-HP reward, huge XP radius
  не собирает world pickup, Book pause откладывает potion, empty Book currency без XP,
  rejected draft оставляет Book, duplicate death не даёт второго drop, ordinary
  death против despawn/boss/Traveler, pause expiry, terminal callback, callback
  Shutdown, pool reuse/new run/stale ID.
- PickupPresenterTests: dev/release gating, feedback и unsubscribe, semantic UXML IDs.
- WorldPickupSmokeTests: настоящая Gameplay scene, reachable placement и разные
  markers, heal/HUD layout, physical Book/draft/heading/pause/deferred potion,
  telemetry identity, shutdown/restart; set backfill до empty Book currency и
  full-HP potion запускает реальный fixture set effect.
- Полный regression suite сохраняет run lifecycle, damage/death, XP/draft/active
  skill, waves/spawn/pool, composition, content, UI и GameplaySmokeTests.

JSON syntax, новые asset/meta пары, relative documentation links и git diff --check
проверены. Ручной visual approval не заявлен: новые pickup visuals — текстовые
fixtures. IP-12A gameplay density review остаётся отдельным пользовательским gate.

## Documentation and decisions

Approved [DECISION-0033](../../decisions/0033-world-pickup-rules.md) отражена в GDD/CD,
DESIGN_SYNC и consumer specifications. [DECISION-0034](../../decisions/0034-world-pickup-ownership.md)
— Proposed technical record. Production numbers/Book ID/art не выдуманы.
Readiness потребителей пересчитана; следующие модули не запускались.
