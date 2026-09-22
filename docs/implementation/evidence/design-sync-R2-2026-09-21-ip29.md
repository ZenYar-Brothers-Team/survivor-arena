# IP-29

Date: 2026-09-21. Scope revision: design-sync-R2.
Execution status/order: only [STATUS](../STATUS.md).

## Implementation

- `Assets/Game/Traveler/`: validating JSON definitions/catalog, field schedule
  payload, deterministic count/times/type selection, independent RNG streams,
  presence/escape/kill owner, reachable placement и peaceful/support steering.
- `Content/Travelers/FixtureTravelers.json`: восемь synthetic fixtures — bruiser,
  dash, cross, wander, rest, guard aura, resistance aura, shield; два field schedules.
  Count probabilities 0.10/0.25/0.35/0.30, seed 4189, presence 60 s и прочие числа
  являются fixture data, не production балансом. Formula/window/distance следуют
  approved DECISION-0035; production TRAVELER-001…010 не зарегистрированы.
- Появление — две полные высоты viewport от игрока внутри player-reachable области.
  Geometry adapter использует clearance ≥ largest Traveler radius. Текущая арена
  200×200 подготовлена [предыдущим изменением](design-sync-R2-2026-09-21-traveler-preparation.md).
- `EnemyRuntime` обслуживает pooled Traveler targets, existing offensive patterns
  и optional `IEnemyMovementDriver`. Guard до outgoing/incoming hit и FixedUpdate
  обеспечивает приоритет escape на deadline. Zero contact не создаёт combat event.
- `EnemyProtection`: max-per-channel aura, one shield, expiry/source cleanup,
  reduction → shield → Health, capped resistance. Health results сохраняют requested
  и actual values. Shield/aura deadline не ждёт следующего source Update.
- Bootstrap подключает registry, field payload, rollback, XP sink и WorldPickupRuntime.
  Убийство создаёт одну Book с source life/content/run identity; escape/terminal — нет.
- UI: keyed HP bars всех ролей, viewport pointers с отдельными lanes, цветовые fixture
  actors и text labels; dev observations/spawn в bounded Build tab. Production art
  и точный player-facing escape countdown не поставлялись.
- Telemetry: schedule/provenance, snapshots, life/outcome/combat events, scale/HP,
  plain x/y и source identity Book. Dispose снимает subscriptions.

Технический API/schema: [IP-29](../modules/IP-29-traveler-framework.md#framework-api-и-schema).
Межслойная запись [DECISION-0036](../../decisions/0036-traveler-runtime-ownership.md)
имеет Proposed, не меняет approved правила DECISION-0035.

## Verification

Unity **6000.6.0f1**, `scripts/Test-Unity.ps1`, batch `-nographics`. Перед каждым
запуском выполнена свежая elevated Get-CimInstance проверка: Editor отсутствовал.
Финальные full filter `^Game\.`: **598/598 EditMode, 15/15 PlayMode,
0 failed, 0 skipped**, third-party tests 0; XML counts и exit codes проверены.

Локальные полные результаты:

- `TestResults/IP29-2026-09-21-EditMode.xml` / `.log`
- `TestResults/IP29-2026-09-21-PlayMode.xml` / `.log`

Coverage:

- TravelerScheduleTests: 0/1/2/3, normalized probabilities, no-repeat types,
  seed reproducibility, exact zero/cutoff и simultaneous times, missing/duplicate
  references/required fields, HP/contact/projectile-only scaling и zero damage.
- TravelerRuntimeTests: точная distance, kill→one Book, deadline→escape без Book,
  pause/no damage/physics freeze, terminal cleanup, simultaneous lives, pool reuse,
  clean reinit, ordinary-only support, aura exit/source removal, shield death cleanup,
  wander/avoid/pause/reachability, peaceful contact и expired outgoing hit.
- EnemyProtectionTests: max-per-channel overlap, shield refresh/replacement/weak
  rejection/owner cleanup/expiry, reduction before absorption, aura deadline before hit.
- BossSkillTargetTests, Traveler case: каждое fixture active skill family обнаруживает,
  повреждает и убивает Traveler через тот же общий pipeline; source attribution
  сохраняется. Boss case остаётся отдельным regression case.
- TravelerPresenterTests: multiple noncoincident pointers, on-screen arrow suppression,
  HP fraction, removals, semantic IDs, release dev gating и unsubscribe.
- TravelerTelemetryTests: реальный export с ненулевой position/schedule/outcome и
  проверкой unsubscribe, без рекурсивной Unity Vector2 serialization.
- TravelerSmokeTests: реальная Gameplay scene, two actors/two pointers, ненулевой
  layout, pause, приближение/скрытие pointer, убийство/source-linked Book, draft
  pause/selection, escape без дополнительной Book, telemetry и новый run.
- Existing full suite сохраняет lifecycle, damage/death, XP/level/draft, active
  skills, waves/spawn/pool, UI/content/composition и GameplaySmokeTests.

Промежуточные failures не являются PASS: missing assembly/using references;
два Enemy tests без инициализированного RunModel выявили nullable clock regression,
исправленную с сохранением этих guards; усиленный lethal skill test обращался к
Position уже уничтоженной цели — harness прекращает dispatch после смерти.
Финальные прогоны перечислены выше.

JSON syntax, asset/meta pairs, documentation links и git diff --check проверены.
Manual production art approval и плотный gameplay review IP-12A не заявлены.

## Consumer readiness

IP-24/IP-26/IP-27/IP-30 получают готовый framework contract. Production required
values/pools/Book card/art остаются у своих владельцев. IP-25 сохраняет CG-03/G-15;
дальнейшие IP автоматически не запускались. Полные текущие статусы только в STATUS.
