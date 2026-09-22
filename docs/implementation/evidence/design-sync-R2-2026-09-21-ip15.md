# IP-15 — Boss encounter framework evidence

## IP-15

Дата: 2026-09-21. Scope revision: `design-sync-R2`.
Пользователь поручил «делаем следующий пункт»; выбран IP-15 по Execution order.
Prerequisites IP-01/IP-05/IP-08/IP-13/IP-14/IP-10A сверены с текущими STATUS/API.

## Реализация

- `BossEncounterDefinition`, `BossPhaseDefinition`, validating DTO/catalog и
  `Content/Bosses/FixtureBosses.json`: два явно synthetic encounters, обязательный
  final, optional mid, ordered attack refs, descending HP fractions. Registry
  валидирует ссылки; catalog source snapshot включает точный boss JSON.
- `BossCombatController`: highest-crossed irreversible phase, death priority,
  cancel pending attack/re-telegraph; sequence wrap после burst/cooldown,
  single-cycle режим для длинного burst и сохранение spiral rotation per slot.
- `BossEncounterRuntime`: one-shot wave hooks вне ordinary cap; отдельные pools,
  distinct life events, phase/source events, fresh director/reinitialize и Shutdown.
  Terminal очищает boss lives/projectiles, без влияния на timer victory.
- `EnemyRuntime`: optional boss combat adapter использует существующие movement,
  projectile/control/target contracts. Owner source — boss life, attack source —
  referenced profile ID. Pool Initialize восстанавливает physics simulation.
- Composition root подключает owner после wave director, rollback и shutdown;
  UI читает immutable BossViewState через presenter. `hud-boss-bar` показывает
  имя/HP final boss; timer и обычный HUD остаются. Midboss — только DEV phase
  observation; incoming использует существующий nonblocking notification slot.
  Spawn/HP/despawn Changed event обновляет HUD без ожидания periodic refresh.
- Pause отключает physics simulation boss bodies: даже contact resolution
  соседних enemies не двигает их. Resume/reuse восстанавливают simulation.

Основные файлы: `Assets/Game/Enemy/Model/Boss*`,
`Assets/Game/Enemy/Runtime/BossEncounterRuntime.cs`, `EnemyRuntime.cs`,
`Assets/Resources/Content/Bosses/FixtureBosses.json`,
`Assets/Game/Bootstrap/GameplayCompositionRoot.cs`,
`Assets/Game/UI/BossViewState.cs`, `GameplayUiRuntimeModel.cs`,
`UiToolkitGameplayView.cs`, `Resources/UI/GameplayUi.uxml`/`GameplayUiStyles.uss`.

## Checks

Runner: `scripts/Test-Unity.ps1`, Windows, Unity **6000.6.0f1**, batchmode/nographics,
Game.* filter. Перед каждым запуском свежая проверка `Win32_Process`:
интерактивный Editor отсутствовал; batch поверх интерактивного Editor не запускался.

Финальный обязательный набор после всех executable changes:

| Platform | Passed / total | Failed | Skipped | Results |
|---|---:|---:|---:|---|
| EditMode | **516 / 516** | 0 | 0 | `TestResults/IP15-EditMode.xml`, `TestResults/IP15-EditMode.log` |
| PlayMode | **10 / 10** | 0 | 0 | `TestResults/IP15-PlayMode.xml`, `TestResults/IP15-PlayMode.log` |

Third-party tests не включены. Полный Game.* набор покрывает run lifecycle,
damage/death, XP→draft, active skills, wave director, enemy/pool, composition,
UI presenter и content loading. Evidence относится к текущим executable artifacts;
после итогового прогона редактировалась только документация.

Новые проверки:

- BossCombatTests: exact thresholds, multiple crossing одним событием, healing,
  lethal/pause no-op, fresh wind-up, sequence wrap, long burst tail, spiral rotation,
  invalid/missing schema и unresolved attack reference.
- BossEncounterTests: full ordinary cap, multiple hooks и отсутствие replay,
  boss killed/alive at timer, player death, cleanup versus kill,
  old director после Shutdown, new life/phase/health, paused/end hook suppression,
  physics pause/resume/pool reuse. Registry всегда возвращается к исходному baseline.
- BossSkillTargetTests: все **13 fixture skill definitions**, discovery через
  SceneEnemyTargetProvider, real executor/AoE/beam/orbit/chain/mine и projectile
  TryImpact; проверены damage и boss category/source attribution. Projectile collision
  dispatch вызывается явно; это не заменяет отдельное physics/movement PlayMode evidence.
- BossHudTests и presenter fake-state: final HP/name и cleanup без DEV,
  incoming expiry по run time, pause freeze, semantic UXML ID.
- BossEncounterSmokeTests: реальная Gameplay scene → midboss без global bar →
  final boss с bar/name, layout timer выше bar, line telegraph, pause position/time/
  telegraph freeze, multiple phase crossing, timer win и cleanup.

Итерации: первоначальный compile остановился на test assembly references и имени
`Effects`; исправлено. Затем fixture Orbit test потребовал Physics2D.SyncTransforms
перед spatial query. Первый PlayMode выявил зависимость boss HUD от periodic refresh:
добавлен producer Changed event. Второй выявил collision displacement на паузе:
boss physics теперь приостановлена. После этого targeted smoke 1/1 и финальные
полные прогоны выше прошли. Проверка long burst/spiral добавлена при code review
sequence ownership, результаты входят в финальный EditMode.

JSON parsing, local Markdown file links и `git diff --check` прошли.
Ручной пользовательский visual/density review не проводился; headless PlayMode
подтверждает visibility/layout/freeze assertions, не production art readability.

## Documentation impact и границы

[IP-15](../modules/IP-15-boss-framework.md) содержит schema/units/example/edge cases,
fixture rationale и missing-rule list. IP-16/IP-21 consumer contracts, STATUS readiness,
[DECISION-0031](../../decisions/0031-boss-encounter-framework.md) (Proposed technical
record) и regression-map синхронизированы. GDD/CD/art не изменены.

BOSS-/MIDBOSS- production definitions, XP/rewards, точные schedules, delayed repeats,
post-dash/support/shield payload и assets остаются соответствующим catalog packets
и G-14. Fixture 500/160 HP, zero reward и пороги не объявлены production балансом.
IP-12A gameplay density review остаётся отдельной пользовательской проверкой.
Существовавшие до задачи изменения `Packages/manifest.json` сохранены.
