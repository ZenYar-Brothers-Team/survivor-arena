# Tech-debt register

Не заменяет `docs/implementation/STATUS.md` (единственный источник статуса модулей). Формат и правила — скил `/tech-debt`.
Приоритет: P1 блокирует, P2 скоро, P3 когда-нибудь. Усилие: S <1 дня, M 1–3, L 3–7, XL >1 недели.
Источник большинства записей — `docs/reviews/2026-09-20-full-review.md` (ID находки в скобках).

## Architecture

| ID | Title | Files | Impact if left | Effort | Priority | Added | Source |
|----|-------|-------|----------------|--------|----------|-------|--------|
| TD-001 | Enemy зависит от Progression: награда за убийство через конкретные `PlayerExperienceRuntime`/`ExperienceDropFactory`/`DropPool`, фабрика берёт `target.GetComponent<PlayerExperienceRuntime>()` | `Enemy/Runtime/EnemyRuntime.cs`, `EnemyFactory.cs`, `Game.Enemy.asmdef` | Враг нельзя использовать/тестировать без модуля XP; asmdef-связь Enemy→Progression мешает новым источникам награды (боссы, события) | M | P2 | 2026-09-20 | review A-1 (нужен ADR: `IEnemyDeathRewardSink` в Game.Enemy) |
| TD-002 | ViewState/интерфейс UI-модели используют доменный `WavePhaseTag` из Enemy; `GameplayUiRuntimeModel` держит MonoBehaviour-спавнер ради dev-строки | `UI/IGameplayUiModel.cs`, `WaveViewState.cs`, `GameplayUiRuntimeModel.cs` | Утечка домена в презентацию (DECISION-0005) | S | P3 | 2026-09-20 | review A-2 |
| TD-003 | Статические изменяемые буферы `EnemyDamageArea` не реентерабельны; поведение не задокументировано (в отличие от `EnemyRegistry`) | `ActiveSkill/Runtime/EnemyDamageArea.cs` | Реентрантный вызов (урон → смерть → новый урон по области) испортит буфер | S | P3 | 2026-09-20 | review A-5 |
| TD-004 | `LevelUpDraftRuntime.Initialize` — две перегрузки по 10–11 параметров и скрытый `new SeededDraftRandom(0)` при `null` | `Progression/Runtime/LevelUpDraftRuntime.cs` | Хрупкий API, скрытый seed | S | P3 | 2026-09-20 | review C-10 |

## Code quality

| ID | Title | Files | Impact if left | Effort | Priority | Added | Source |
|----|-------|-------|----------------|--------|----------|-------|--------|
| TD-010 | `TickForTests` — тестовый хук в production-типе | `Progression/Runtime/ExperienceDropRuntime.cs` | Тестовый API в публичной поверхности | S | P3 | 2026-09-20 | review C-11 |
| TD-011 | Дублирование тестовых фейков (`RecordingExecutor`, `FixedTargetProvider`, `FakeReceiver`) в трёх файлах ActiveSkill/Tests | `ActiveSkill/Tests/*` | Правки в трёх местах; расхождение фейков | S | P3 | 2026-09-20 | review T (helpers) |

## Content/Config

| ID | Title | Files | Impact if left | Effort | Priority | Added | Source |
|----|-------|-------|----------------|--------|----------|-------|--------|
| TD-020 | Литералы эффектов навыков в коде: радиус лезвия орбиты `0.3f`, коэффициент вторичного взрыва мины `0.75f`, запас жизни бумеранга `returnAfter*2f+1f` | `ActiveSkill/Runtime/SceneActiveSkillEffectExecutor.cs` (стр. ~168, 222, 324) | Тюнинг навыков вне JSON; нарушение правила AGENTS.md | M | P2 | 2026-09-20 | review C-3 (новые обязательные поля в JSON эффектов, миграция 8 fixture-навыков) |
| TD-021 | Tuning-дефолты в доменных конструкторах: `burstIntervalSeconds = 0.15f`, `dashDurationSeconds = 0.4f` | `Enemy/Model/EnemyAttackProfile.cs`, `EnemyMovementProfile.cs` | Скрытые значения; каталог уже требует их из JSON | S | P3 | 2026-09-20 | review C-6 |
| TD-022 | Презентационные литералы (цвета, ширина телеграфа, радиус коллайдера `0.5f`, масштаб дропа `0.35f`) в коде | `EnemyRuntime.cs`, `EnemyProjectileRuntime.cs`, `ExperienceDropRuntime.cs`, `FixtureProjectileRuntime.cs`, `SceneActiveSkillEffectExecutor.cs` | DECISION-0013 требует профиль презентации в конфиге | M | P3 | 2026-09-20 | review C-7 |
| TD-023 | Длительность забега — сериализуемый дефолт из константы `RunModel.DefaultDurationSeconds` | `Run/Presenters/RunController.cs`, `Run/Model/RunModel.cs` | Значение не в JSON; RunController создаёт модель в `Awake` до composition root | M | P3 | 2026-09-20 | review C-8 |
| TD-024 | Позиция спавна — несидированный `UnityEngine.Random.insideUnitCircle`; выбор врага детерминирован, позиция нет | `Enemy/Runtime/ContinuousFixtureEnemySpawner.cs` | Невоспроизводимые забеги при одном seed; нужен владелец RNG (решение) | S | P3 | 2026-09-20 | review C-9 |
| TD-025 | В `Gameplay.unity` остались устаревшие сериализованные значения удалённых полей (`startingCharacterId`, `draft*`, `fixtureInitial*`, `baseDropLifetimeSeconds`, `fixtureLevelThresholds`, а раньше поля спавнера) | `Assets/Scenes/Gameplay.unity` | Мусор в YAML; уйдёт при пересохранении сцены в Editor | S | P3 | 2026-09-20 | review C-1/C-2 |
| TD-026 | Dev-команды с захардкоженными значениями (`AddFixtureExperience` 5f, урон/лечение 10f) | `UI/GameplayUiRuntimeModel.cs` | Только dev-инструмент | S | P3 | 2026-09-20 | review |

## Performance

| ID | Title | Files | Impact if left | Effort | Priority | Added | Source |
|----|-------|-------|----------------|--------|----------|-------|--------|
| TD-030 | Снаряды игрока не пулятся: `new GameObject` + `Destroy` на каждый выстрел (ADR-0011 оставил на потом) | `ActiveSkill/Runtime/FixtureProjectileFactory.cs`, `FixtureProjectileRuntime.cs` | GC/аллокации на самом частом объекте при росте темпа стрельбы | M | P2 | 2026-09-20 | review P-6, DECISION-0011 |
| TD-031 | `Update()` на каждом дропе опыта | `Progression/Runtime/ExperienceDropRuntime.cs` | Стоимость растёт линейно с числом дропов | M | P3 | 2026-09-20 | review P-3 |
| TD-032 | `SynchronizeBuild()` каждый кадр, хотя `SelectionApplied` уже синхронизирует | `ActiveSkill/Runtime/PlayerActiveSkillSetRuntime.cs` | Лишняя работа O(entries) в кадре | S | P3 | 2026-09-20 | review P-4 |
| TD-033 | `_aliveEnemies.Remove` — O(n) на деспавн | `Enemy/Runtime/ContinuousFixtureEnemySpawner.cs` | Заметно только при сотнях врагов; уже под `PerfGuard` на `Tick` | S | P3 | 2026-09-20 | review P-5 |

## Test

| ID | Title | Files | Impact if left | Effort | Priority | Added | Source |
|----|-------|-------|----------------|--------|----------|-------|--------|
| TD-040 | Нет прямых тестов: `GameplayUiRuntimeModel` (в т.ч. `DescribeWave`), `SceneEnemyTargetProvider`, `SceneProjectileLauncher`, `JsonContentFile` | `UI`, `ActiveSkill/Runtime`, `Content/Json` | Регрессии в этих типах ловятся только косвенно | M | P2 | 2026-09-20 | review T-3 |

## Docs / Dependency

Записей нет.
