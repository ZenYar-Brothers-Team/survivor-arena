# Полное ревью: архитектура и код (2026-09-20)

Статус: In progress. Ветка: `develop-evg`. Инструменты: локальные скилы из `.claude/skills/` и правила из `.claude/rules/`.
Этот файл — рабочий журнал ревью (план → находки → исправления). Он **не** заменяет `docs/implementation/STATUS.md` (единственный источник статуса модулей).

## Объём

~9,7 тыс. строк production-кода (13 модулей `Assets/Game/*`, 28 asmdef), ~5,2 тыс. строк тестов, JSON-контент `Assets/Resources/Content`, runtime-арт `Assets/Resources/Art`, документация `docs/`.
Вне объёма: `Packages/`, `.claude/skills/unity-skills` (сторонний код), `docs/Game_design.md` / `docs/Content_design.md` (только чтение, правки — с одобрения пользователя).

## План

| # | Фаза | Скилы / правила | Что проверяем | Результат |
|---|------|-----------------|---------------|-----------|
| 0 | Базовая линия | `/smoke-check` | EditMode + PlayMode проходят до правок; чистое состояние git | Числа «до» |
| 1 | Архитектура | `/conventions-review` (раздел ADR), `foundation-code`, `gameplay-code` | Граф asmdef (направление зависимостей, циклы, лишние ссылки); соответствие DECISION-0001…0014; composition root и `Shutdown()`; границы UI ↔ gameplay; владение состоянием | Находки A-* |
| 2 | Код по модулям | `/conventions-review`, `gameplay-code`, `enemy-ai-code`, `ui-code` | Правила AGENTS.md (конфиг из JSON, `NumericValidation`, пулы, один тип на файл, `PerfGuard`, `VisualRoot`), Unity-ловушки (жизненный цикл, подписки, аллокации, obsolete API). Порядок по риску: Bootstrap → Enemy → ActiveSkill → Progression → Character/Combat/Run/Movement → Presentation → UI → Content/Pooling/Diagnostics | Находки C-* |
| 3 | Производительность | `/perf-audit` full, `PerfGuard` | Hot-path (Update/Tick), аллокации, пулы, недостающие `PerfGuard` | Находки P-* |
| 4 | Тесты | `/test-quality-review`, `unity-tests`, `/regression-map` | Слабые/недетерминированные тесты, непокрытые критические пути, evidence в STATUS ↔ реальные тесты | Находки T-*, `docs/regression-map.md` |
| 5 | Контент и документы | `/consistency-check`, `/content-audit`, `/design-review` (IP-14), `/balance-check` (fixture-волны/враги), `content-json`, `design-docs` | JSON ↔ DTO ↔ Content Design; ID; STATUS/DECISION ↔ код | Находки D-* |
| 6 | Ассеты | `/asset-audit` | `.meta`, нейминг, ссылки на спрайты, сироты | Находки R-* |
| 7 | Долг | `/tech-debt scan` | Реестр `docs/tech-debt-register.md` для того, что не чиним сейчас | Реестр |
| 8 | Триаж | — | Каждая находка: **Fix now** (однозначная, локальная), **Register** (в долг), **Decision** (нужен ответ пользователя / DECISION) | Таблица ниже |
| 9 | Исправления | правила модулей, `/architecture-decision` при межслойных решениях | Правки по приоритету (баги/риск → нарушения правил → стиль); после каждой группы — компиляция и тесты; синхронизация STATUS/DECISION/AGENTS при необходимости | Diff + evidence |
| 10 | Верификация | `/smoke-check` | EditMode + PlayMode после правок, сравнение с базой; повторный `/conventions-review` по изменённым файлам | Числа «после» |

### Правила исполнения
- Находка фиксируется только если проверена по коду на указанной строке (`path:line`); непроверенное помечается «not checked».
- Не правим: `docs/Game_design.md`, `docs/Content_design.md`, продуктовые правила. Решения между слоями — через `/architecture-decision` (Proposed) и вопрос пользователю.
- Не коммитим и не пушим без явной просьбы. Unity-шум (`.png.meta`, `InputSystem_Actions.inputactions`, `Assets/Temp.meta`) не трогаем и не коммитим.
- Тесты — через открытый Editor (UnitySkills `test_run`); закрывать Editor не требуется, пока сервер отвечает. PlayMode требует режима Bypass в панели UnitySkills.
- Каждое исправление сопровождается тестом там, где поведение меняется (регрессионный тест для бага).

## Что фактически проверено

- **Прочитано целиком:** `GameplayCompositionRoot`, `EnemyRuntime`, `EnemyFactory`, `EnemyProjectileRuntime`, `EnemyRegistry`, `EnemyDamageArea`, `ContinuousFixtureEnemySpawner`, `SceneActiveSkillEffectExecutor`, `FixtureProjectileRuntime/Factory`, `PlayerActiveSkillSetRuntime`, `PlayerPassiveSetRuntime`, `PlayerCharacterRuntime`, `PlayerExperienceRuntime`, `ExperienceDropRuntime/Factory`, `LevelUpDraftRuntime`, `Health`, `NumericValidation`, `GameObjectPool`, `SpritePresentationRuntime`, `RunController`, `PlayerMover`, `GameplayUiRoot`, `GameplayUiRuntimeModel`.
- **Проверено скриптами по всему `Assets/Game`:** граф asmdef ↔ фактические `using`, один тип на файл и имя файла, локальные `Validate*`, `new GameObject/Instantiate/Destroy`, `Find*/Resources.Load/GetInstanceID`, `[SerializeField]` со значениями, `PerfGuard`, `Debug.Log`, Update-методы, баланс подписок `+=`/`-=`, числовые литералы, JSON ↔ ссылки ↔ `.meta`, продовые типы без упоминания в тестах.
- **Не читалось построчно (проверено только скриптами/тестами):** чистые модели `Enemy/Model`, `Progression/Model|Draft|Sets|Passive`, `ActiveSkill/Progression`, `Presentation` (кроме runtime), `Character/Model`, `Movement/Model`, `UiToolkitGameplayView`, `GameplayUiPresenter`. `balance-check` по fixture-данным не выполнялся (значения — заглушки).

## Находки

Серьёзность: **H** — реальный дефект/риск, **M** — нарушение правил репозитория, **L** — улучшение.

### Архитектура
| ID | Sev | Где | Суть | Решение |
|----|-----|-----|------|---------|
| A-1 | M | `Enemy/Runtime/EnemyRuntime.cs:27,219-227`, `EnemyFactory.cs:35`, `Game.Enemy.asmdef` | Враг знает конкретный `PlayerExperienceRuntime`, `DropPool`, `ExperienceDropFactory`; фабрика достаёт его через `target.GetComponent`. Модуль Enemy зависит от Progression (награда за убийство — забота Progression). | **Decision/Register** — нужен интерфейс награды (`IEnemyDeathRewardSink`) и ADR; средний рефакторинг |
| A-2 | L | `UI/IGameplayUiModel.cs`, `WaveViewState.cs`, `GameplayUiRuntimeModel.cs:25,45` | ViewState/интерфейс модели тянут доменный `WavePhaseTag` из Enemy; модель держит MonoBehaviour-спавнер ради dev-строки | Register |
| A-3 | H | `ActiveSkill/Runtime/PlayerActiveSkillSetRuntime.cs:76-78`, `Progression/Runtime/PlayerPassiveSetRuntime.cs:58-60` | Подписка на `SelectionApplied` до `_initialized = true`; если `SynchronizeBuild()` бросит, `Shutdown()` сразу выходит (`!_initialized`) и подписка (и применённые модификаторы) остаются — откат по DECISION-0010 не срабатывает | **Fix now** + тесты |
| A-4 | H | `Character/Presenters/PlayerCharacterRuntime.cs:55-61` | `Update()` без проверки `_initialized`: после `Shutdown()` (откат composition root) `Health == null` → `NullReferenceException` каждый кадр | **Fix now** + тест |
| A-5 | L | `EnemyDamageArea.cs:9-10`, `EnemyRegistry.cs:13` | Статические изменяемые буферы/реестр: `EnemyDamageArea` не реентерабелен (документировано только для реестра) | Register (документировать) |
| A-6 | L | `Progression/Runtime/LevelUpDraftRuntime.cs:256-268` | `Shutdown()` не сбрасывает `_pendingDrafts`, `CurrentDraft`, `Build`, `Controls`, `_pool` — повторный `Initialize` унаследует хвост | **Fix now** |

### Код и правила репозитория
| ID | Sev | Где | Суть | Решение |
|----|-----|-----|------|---------|
| C-1 | M | `PlayerExperienceRuntime.cs:19-23` | Пороги уровней `{5,10,15}` и `baseDropLifetimeSeconds = 60` — сериализуемые значения на MonoBehaviour (правило: конфиг из JSON) | **Fix now** → JSON |
| C-2 | M | `GameplayCompositionRoot.cs:46-59` | `draftSeed = 12345`, `draftOfferCount`, `fixtureInitialRerolls/Banishes`, `startingCharacterId` — значения и seed в сериализуемых полях (правило: seed/геометрия — контент) | **Fix now** → JSON |
| C-3 | M | `SceneActiveSkillEffectExecutor.cs:222,168,324` | Литералы в эффектах: радиус лезвия орбиты `0.3f`, коэффициент вторичного взрыва мины `0.75f`, запас жизни бумеранга `returnAfter*2f+1f` | **Register** (требует новых полей в JSON эффектов и смены схемы 8 скилов — отдельный шаг) |
| C-4 | L | `EnemyRuntime.cs:130` | `_target.GetComponent<PlayerCharacterRuntime>()` на каждый выстрел в `FixedUpdate` | **Fix now** (кэш в `Initialize`) |
| C-5 | L | `EnemyRuntime.cs:93,236` | Старый `Health` не `Dispose()`-ится при повторном `Initialize`/`Despawn` из пула (утечки нет — профиль владеет `Health`, но непоследовательно) | **Fix now** |
| C-6 | L | `EnemyAttackProfile.cs:28`, `EnemyMovementProfile.cs:27` | Доменные конструкторы несут tuning-дефолты (`burstIntervalSeconds=0.15f`, `dashDurationSeconds=0.4f`), хотя каталог уже требует их из JSON | Register |
| C-7 | L | `EnemyRuntime`, `EnemyProjectileRuntime`, `ExperienceDropRuntime`, `FixtureProjectileRuntime`, `SceneActiveSkillEffectExecutor` | Презентационные литералы (цвета, ширина телеграфа, радиус коллайдера `0.5f`, масштаб `0.35f`) в коде; DECISION-0013 требует профиль в конфиге | Register |
| C-8 | L | `Run/Presenters/RunController.cs:8` | Длительность забега — сериализуемый дефолт `RunModel.DefaultDurationSeconds` | Register (единственный владелец — константа Game Design 15 мин) |
| C-9 | L | `ContinuousFixtureEnemySpawner.cs:86` | Позиция спавна — несидированный `UnityEngine.Random.insideUnitCircle` (выбор врага детерминирован, позиция нет) | Register/Decision (кто владеет RNG) |
| C-10 | L | `LevelUpDraftRuntime.cs:54-112,135` | Две перегрузки `Initialize` с 10–11 параметрами; скрытый `new SeededDraftRandom(0)` | Register |
| C-11 | L | `ExperienceDropRuntime.cs:93` | `TickForTests` — тестовый хук в production-типе | Register |

### Производительность (статическая оценка, без профайлера)
| ID | Sev | Где | Суть | Решение |
|----|-----|-----|------|---------|
| P-1 | M | `UI/GameplayUiPresenter.cs:41-56` | `RefreshHud` каждые 0,1 с строит `EnemyDevelopmentSummary` (интерполяция) и `WaveDevelopmentSummary` (StringBuilder + ~8 `ToString`) **даже когда dev-панель скрыта / не dev-сборка** | **Fix now** (строить только при `DevelopmentCommandsEnabled`) |
| P-2 | M | `SceneActiveSkillEffectExecutor.Tick`, `EnemyDamageArea.Apply`, `ContinuousFixtureEnemySpawner.Tick` | `PerfGuard` стоит только на `TickMines` и `TryFindNearest`; лучи/цепи/область/спавн без guard | **Fix now** (guard на `Tick` исполнителя и спавнера) |
| P-3 | L | `ExperienceDropRuntime.cs:66` | `Update()` на каждый дроп опыта (сотни при плотных волнах) | Register |
| P-4 | L | `PlayerActiveSkillSetRuntime.cs:86` | `SynchronizeBuild()` каждый кадр, хотя `SelectionApplied` уже синхронизирует | Register |
| P-5 | L | `ContinuousFixtureEnemySpawner.cs:110` | `_aliveEnemies.Remove` — O(n) на деспавн | Register |
| P-6 | M | `ActiveSkill/Runtime/FixtureProjectileFactory.cs:17`, `FixtureProjectileRuntime.cs:156` | Снаряды игрока — `new GameObject` + `Destroy` на каждый выстрел (самый частый объект). ADR-0011 явно оставил «на потом» | **Register** (пулинг по паттерну DECISION-0011) |

### Тесты
| ID | Sev | Где | Суть | Решение |
|----|-----|-----|------|---------|
| T-1 | M | `Assets/Game/Pooling` | У `GameObjectPool<T>` нет ни одного собственного теста (0 тестов в модуле). Плюс `Return` не защищён от двойного возврата → один объект может быть выдан дважды | **Fix now** (guard + тесты) |
| T-2 | M | `Content/NumericValidation.cs` | Общий валидатор без прямых тестов (проверяется только косвенно) | **Fix now** |
| T-3 | L | UI/ActiveSkill/Content | Нет прямых тестов: `GameplayUiRuntimeModel` (в т.ч. `DescribeWave`), `SceneEnemyTargetProvider`, `SceneProjectileLauncher`, `JsonContentFile` | Register |
| T-4 | L | `docs/regression-map.md` | Карты регрессионного покрытия нет | Создать (см. фазу 7) |

### Контент, документы, ассеты
- **Контент:** все JSON парсятся; id уникальны внутри файлов; спрайт/профиль движения не «сироты»; `resourcePath` резолвятся. В JSON только `FIXTURE-*` — production-контента ещё нет, что соответствует `STATUS.md` (IP-17…IP-24).
- **Документы:** STATUS ↔ код согласованы по IP-14; ADR-0011 честно фиксирует, что снаряды игрока не пулятся. Расхождений Game/Content Design ↔ код не найдено (проверялись только ID/структура, не значения).
- **Ассеты:** единственный runtime-PNG (`fixture-character-agile-body.png`) имеет `.meta`; пиксельные параметры не проверялись (изображение не открывалось).

## Триаж и исправления

Порядок исправлений: A-4 → A-3 → T-1 → T-2 → A-6 → C-4/C-5 → P-1/P-2 → C-1/C-2. Остальное (Register) уходит в `docs/tech-debt-register.md`; A-1 и C-3 требуют решения и ADR.

Статус правок: **внесены и скомпилированы (ошибок CS в `Logs/Editor.log` нет), тесты ещё не запускались** — итоговые числа в разделе «Верификация».

| Находка | Правка | Тест |
|---------|--------|------|
| A-4 | `PlayerCharacterRuntime.Update` выходит при `!_initialized` | `PlayerCharacterRuntimeLifecycleTests` (3) |
| A-3 | `PlayerActiveSkillSetRuntime`/`PlayerPassiveSetRuntime.Initialize`: подписка после успешной синхронизации + очистка состояния при сбое (`ReleaseAppliedModifiers`); в `GameplayCompositionRoot` исполнитель навыков регистрируется на откат до `Initialize` | `PlayerActiveSkillSetRuntimeRollbackTests` (2), `PlayerPassiveSetRuntimeRollbackTests` (2) |
| A-6 | `LevelUpDraftRuntime.Shutdown` сбрасывает `CurrentDraft`, `_pendingDrafts`, `Build`, `Controls`, `_pool`, `_draftRandom` | `LevelUpDraftRuntimeTests.Shutdown_ClearsOpenDraftAndPendingState_SoReinitializeStartsClean` |
| T-1 | `GameObjectPool<T>`: защита от двойного `Return` (`HashSet`), `InactiveCount`; новая сборка `Game.Pooling.Tests` | `GameObjectPoolTests` (8) |
| T-2 | — | `NumericValidationTests` (≈35 кейсов) |
| C-4, C-5 | `EnemyRuntime`: игрок-цель кэшируется в `Initialize`; старый `Health` `Dispose()`-ится при повторном `Initialize`/`OnDestroy` | покрыто `WaveSpawnerTests`, `EnemyRuntimeLifecycleTests` |
| P-1 | `GameplayUiPresenter.RefreshHud` не строит dev-строки, если `DevelopmentCommandsEnabled == false` | `GameplayUiPresenterTests.ProductionModel_DoesNotBuildDevelopmentObservability` |
| P-2 | `PerfGuard` на `SceneActiveSkillEffectExecutor.Tick` (2 мс) и `ContinuousFixtureEnemySpawner.Tick` (2 мс) | — (порог/лог-гарды) |
| C-1, C-2 | Новая категория контента `Resources/Content/Run/FixtureRunSetup.json` (стартовый персонаж, draft: offerCount/seed/rerolls/banishes, XP: пороги и базовое время жизни дропа) → `RunSetupConfig`/`DraftSettings`/`ExperienceSettings` + `FixtureRunSetupCatalog` (обязательные поля, ошибка по имени). Из `GameplayCompositionRoot` и `PlayerExperienceRuntime` удалены сериализуемые значения; `PlayerExperienceRuntime.Initialize` теперь принимает `ExperienceSettings` | `RunSetupConfigTests` (6), `FixtureRuntimeContentCatalogTests`, `GameplayCompositionSceneTests`, `ExperienceIntegrationTests` (обновлены) |

В реестр техдолга (`docs/tech-debt-register.md`, TD-001…TD-040) ушло: A-1, A-2, A-5, C-3, C-6…C-11, P-3…P-6, T-3. Карта покрытия — `docs/regression-map.md`.

## Верификация

_(заполняется после фазы 10)_
