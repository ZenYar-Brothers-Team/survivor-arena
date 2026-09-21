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
| Composition root: сборка, откат при сбое | `GameplayCompositionSceneTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: откат при частичном сбое покрыт только косвенно (runtime-тесты выше), прямого теста `GameplayCompositionRoot.Initialize` с искусственным сбоем нет |
| UI: presenter ↔ ViewState, HUD-волна, dev-панель | `GameplayUiPresenterTests`, `GameplayUiAssetTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: `GameplayUiRuntimeModel` и `UiToolkitGameplayView` без прямых тестов (TD-040) |
| JSON-загрузка (`JsonContentFile`: нет файла, битый JSON, неизвестное поле) | — | — | GAP (TD-040) |
| Цель для навыков (`SceneEnemyTargetProvider`) и запуск снарядов (`SceneProjectileLauncher`) | — | — | GAP (TD-040) |

## Багфиксы без регресс-теста
Нет открытых. Исправления ревью 2026-09-20 (A-3, A-4, A-6, T-1) сопровождаются регресс-тестами, перечисленными выше.
