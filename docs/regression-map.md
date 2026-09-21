# Regression map

Индекс **уже существующих** тестов, которые охраняют критические пути. Не заменяет evidence в `docs/implementation/STATUS.md`. Поддерживается скилом `/regression-map` (`update` / `check` / `bug`).
Составлено 2026-09-20 по `Assets/Game/**/Tests` и `PlayModeTests`; наличие классов проверено поиском, результаты прогона тут не фиксируются (см. `/smoke-check`).

| Критический путь | Охраняющие тесты (класс) | Вид | Статус |
|------------------|--------------------------|-----|--------|
| Жизненный цикл забега, пауза, конец | `Game.Run.Tests` (`RunModelTests`) | EditMode | OK |
| Движение игрока, границы | `Game.Movement.Tests` (`MovementVelocityCalculatorTests`, `GameplaySceneIntegrationTests`, `PlayerObstacleCollisionTests`) | EditMode | OK |
| Урон → смерть → конец забега | `CharacterRunBindingTests`, `CharacterHealthIntegrationTests`, `Game.Combat.Tests` (`HealthTests`) | EditMode | OK |
| Откат/повтор инициализации `PlayerCharacterRuntime` | `PlayerCharacterRuntimeLifecycleTests` | EditMode | OK (добавлено в ревью) |
| XP: дроп → подбор → уровень → пауза | `ExperienceProgressionTests`, `ExperienceDropTests`, `ExperienceIntegrationTests`, `RunSetupConfigTests` | EditMode | OK |
| Драфт: офферы, reroll, banish, очередь, сеты | `LevelUpDraftRuntimeTests`, `PlayerBuildAndDraftTests`, `SetFrameworkTests`, `PassiveFrameworkTests` | EditMode | OK |
| Откат `PlayerActiveSkillSetRuntime`/`PlayerPassiveSetRuntime` при сбое Initialize; passive повторный Initialize и Shutdown→Initialize без stale catalog/stacking | `PlayerActiveSkillSetRuntimeRollbackTests`, `PlayerPassiveSetRuntimeRollbackTests` | EditMode | OK (добавлено в ревью) |
| Активные навыки: срабатывание, кулдаун, эффекты, мины | `PlayerActiveSkillSetRuntimeTests`, `ActiveSkillTimingTests`, `ProjectileAndAreaTests`, `SceneActiveSkillEffectExecutorTests`, `ActiveSkillProgressionFrameworkTests` | EditMode | OK |
| Wave Director: фазы, хуки, лимиты, состав | `WaveDefinitionTests`, `WaveDirectorTests` | EditMode | OK |
| Спавн/деспавн врагов, пул, `EnemyRegistry` | `WaveSpawnerTests`, `EnemyRuntimeLifecycleTests`, `EnemySpawnerSceneIntegrationTests` | EditMode | OK |
| Паттерны движения/атаки врагов | `EnemyPatternTests`, `EnemyMovementAndContactTests` | EditMode | OK |
| Пул `GameObjectPool<T>` | `GameObjectPoolTests` | EditMode | OK (добавлено в ревью) |
| Общий валидатор чисел | `NumericValidationTests` | EditMode | OK (добавлено в ревью) |
| Загрузка контента: реестр, ссылки, каталоги | `ContentRegistryTests`, `FixtureRuntimeContentCatalogTests`, `FixtureCharacterCatalogTests` | EditMode | OK |
| Презентация спрайтов (композитор позы) | `Game.Presentation.Tests` (`ProceduralSpriteAnimatorTests` и др.) | EditMode | OK |
| Category imports, role/crop validation, generic presentation pause/fade/pool/disable и shake preference | `CategoryImportTests`, `GenericPresentationTests`, `ScreenShakeRequestGateTests`, `PresentationAdapterSmokeTests` | EditMode / PlayMode | OK; manual art/dense gameplay review отдельно |
| Composition root: сборка, откат при сбое | `GameplayCompositionSceneTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: откат при частичном сбое покрыт только косвенно (runtime-тесты выше), прямого теста `GameplayCompositionRoot.Initialize` с искусственным сбоем нет |
| UI: presenter ↔ ViewState, HUD-волна, dev-панель | `GameplayUiPresenterTests`, `GameplayUiAssetTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: `GameplayUiRuntimeModel` и `UiToolkitGameplayView` без прямых тестов (TD-040) |
| JSON-загрузка (`JsonContentFile`: нет файла, битый JSON, неизвестное поле) | — | — | GAP (TD-040) |
| Цель для навыков (`SceneEnemyTargetProvider`) и запуск снарядов (`SceneProjectileLauncher`) | — | — | GAP (TD-040) |

## Багфиксы без регресс-теста

IP-31: `RunTelemetryRecorderTests.Snapshot_ContentIdDictionaryKeys_RetainOrdinalCase` защищает стабильные content IDs от camel-case преобразования ключей JSON. `PlaytestSmokeTests.Gameplay_LethalHitExportsLinkedPacket_AndPlaytestUiStaysCollapsed` проверяет scene reload, export и идемпотентный teardown; до ordered composition Shutdown reload давал NullReferenceException в UI/passive consumers после очистки Health/Stats. Дополнительно `PlaytestSessionTests.Shutdown_DuringLiveExport_PublishesFinalSnapshotAfterEarlierPacket` защищает final packet от перезаписи более ранним live export.
Нет открытых. Исправления ревью 2026-09-20 (A-3, A-4, A-6, T-1) сопровождаются регресс-тестами, перечисленными выше.

## IP-11 — regression guard

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Producer-first shutdown / scene reload with acquired sets | `SetFrameworkSmokeTests.SimultaneousSets_QueuedChoicesPauseProjectionAndShutdown` | PlayMode | См. IP-11 в [STATUS](implementation/STATUS.md) | Явный `player.Shutdown()` до root проверяет pre-teardown notification; без него UI/passive consumers обращаются к очищенным Health/Stats. Дополняет прежний случайный scene-reload guard IP-31. |

## IP-12 — regression guard

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Character selection → loadout → Shutdown → selection | `CharacterSelectionSmokeTests.Selection_LockedCannotStart_AlternateLoadoutAndReinitAreClean` | PlayMode | См. IP-12 в [STATUS](implementation/STATUS.md) | Без независимой panel/root UI Toolkit отвергает повторное открытие selection после HUD; проверяются новый run ID, сброс stats/modifiers/skill и actual character ID в telemetry. |

## IP-13 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Pool return inside impact callback | `EnemyPatternIntegrationTests.ImpactCallback_CanRentSameProjectileWithoutOldHitDespawningNewLife` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Snapshot и return перед callback защищают новую аренду от старого попадания. |
| Old run callback after projectile reinit | `EnemyPatternIntegrationTests.OldRunTerminalCallback_DoesNotDespawnReinitializedProjectile` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Старый multicast StateChanged не возвращает новую running life. |
| Pause phase / projectile cleanup | `EnemyPatternIntegrationTests.Pause_PreservesObservableMovementPhase`, `Projectile_PauseFreezesAndTerminalReturnsImmediatelyWithoutPhysicsTick`, `PoolReuse_ResetsSourceLifetimeVelocityRendererTrailAndOldRunSubscription` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Сохранение phase, reset source/trail и немедленный terminal cleanup. |

## IP-14 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Actual spawn count при отсутствии target | `WaveSpawnerTests.Tick_MissingTarget_ReportsZeroActualWithoutRetryingBurst` | EditMode | См. IP-14 в [STATUS](implementation/STATUS.md) | Tick возвращает число созданных объектов; неисполненная группа отмечается unavailable и не повторяется. |
| Continuous timer при перескоке между фазами | `WaveBurstTests.Continuous_SkippedBoundary_ChargesOnlyTimeInCurrentPhaseAndDiscardsCapSuppression` | EditMode | См. IP-14 в [STATUS](implementation/STATUS.md) | Время старой фазы не начисляется новой; suppressed заявки не накапливаются для последующего спавна. |
