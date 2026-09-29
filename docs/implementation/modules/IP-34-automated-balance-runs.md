# IP-34 — Автоматические прогоны баланса и прогрессии

Ревизия `automated-runs-v7`: базовая поставка по [DECISION-0097](../../decisions/0097-automated-balance-runs-v1.md),
дополнительные XP-focused, дуговой и herd-loop профили и диагностическая итерация — по поручениям пользователя 2026-09-29.
Текущий packet, его status, порядок и evidence — только в [STATUS](../STATUS.md#automated-runs-execution).
Этот файл — план реализации; описанные ниже новые файлы, API и команды являются целевыми, а не существующими возможностями.

## 1. Результат и границы первой поставки

Одна команда запускает несколько независимых историй профиля. В каждой истории бот
проходит последовательность обычных случайных забегов, получает штатные награды,
покупает улучшения и переходит по доступным картам. На выходе — машинные отчёты и
читаемая сводка: победы/поражения, достигнутые волны, развитие билда, попытки до
открытий и изменение профиля. Вторая конфигурация может быть сравнена с первой.

Два обязательных шаблона:

- `freshCampaign`: начать каждую независимую историю с нового production-профиля.
- `presetCampaign`: начать каждую историю с отдельной копии заданного профиля;
  дальше награды, покупки и открытия также сохраняются между забегами этой истории.

Первая версия использует обычную production-сцену и Unity player loop.
По [DECISION-0102](../../decisions/0102-balance-runner-presentation.md) массовые
прогоны по умолчанию скрыты и без звука; отдельный эксперимент можно явно показать
в оконном режиме SafeWindow, а звук включить отдельным opt-in. Это режим presentation, не другой
симулятор.
В конфигурации явно выбирается существующая скорость `1`, `2`, `3` или `5`;
первый smoke — на `1`, массовые серии могут использовать `5`. Нового ускорения,
ручного `Physics2D.Simulate`, переписывания clocks и обхода Update/FixedUpdate нет.
Одинаковые скорости сравниваются отдельно; 5× не объявляется эквивалентом 1× без проверки.
Запуск последовательный, один standalone player; Editor служит для разработки и smoke.

Каждый gameplay-run получает свежую случайность существующим способом.
`UseReferenceSeeds=false`; доступные фактические seeds сохраняются как диагностика.
Нет требования deterministic replay или переделки RNG от GUID ради первой поставки.
Seeded fake RNG в unit tests обязателен и не противоречит случайным production-прогонам.

### Out of scope v1

Автоматическое изменение баланса, целевой win rate, настройка экономики, новый игровой
контент/карты, нейросетевой игрок, обучение, сетевые LLM API, автоматический upload,
видеозапись/vision review, полноценный UI-бот, distributed/server simulation workers, параллельные
процессы, replay, продолжение оборванного забега, автоматическое исправление найденных
багов. Оптимизатор и визуальный ревьюер описаны как последующие поставки в §10.

## 2. Зависимости, Context и чтение перед packet

Обязательные framework dependencies: [IP-01](IP-01-run-lifecycle.md),
[IP-02](IP-02-player-movement.md), [IP-07](IP-07-level-up-draft.md),
[IP-16](IP-16-field-framework.md), [IP-25](IP-25-meta-progression.md),
[IP-31](IP-31-manual-run-telemetry.md).
Production prerequisite — именно FIELD-001 subset F1-09 из
[IP-27](IP-27-integration.md), не весь каталог IP-27.
Дополнительное поле включать только при наличии его production definition, environment,
timeline и encounter bindings; доступ в meta-каталоге сам по себе не доказывает готовность поля.

При начале: AGENTS → WORKFLOW → запись IP-34/packet в STATUS → этот файл.
Затем читать только Context выбранного packet и необходимые исходники.
Не запускать другие IP, не сканировать архив evidence или vendored skills.

| Packet | Дополнительный Context |
|---|---|
| AB-01 | IP-25; GDD «Мета-прогрессия», «Стартовая прогрессия»; `ProfileCodec`, `ProfileService`, `IProfileStore`, `MemoryProfileStore`; content-json rules |
| AB-02 | IP-02; GDD «Управление, бой и выживание»; `PlayerMover`, `EnemyRuntime`, `EnemyProjectileRuntime`, obstacle/XP/pickup queries; gameplay/foundation rules |
| AB-03 | IP-01/07/16; GDD «Структура забега», «Опыт и level-up», «Сеты», «World pickups», «Поля»; composition root, draft runtime, run outcome |
| AB-04 | IP-31, PLAYTEST_REPORT и BALANCE_WORKFLOW; recorder/session/provenance; phase/build/profile producers |
| AB-05 | IP-25 и GDD meta/starting progression; `ProfileRunBinding`, `MetaCatalog`, цены/условия только из каталога; карточки выбранных CHAR/FIELD/META |
| AB-06 | `scripts/README.md`, standalone performance builder/runner как пример доставки, smoke-check safety |
| AB-07 | §7 этого файла; IP-32/BALANCE_WORKFLOW; отчётный schema и test fixtures AB-04 |
| AB-08 | Acceptance предыдущих packets, STATUS, smoke-check; полные карточки реально включённых в пилот CHAR/FIELD и необходимые связанные карточки |

Для любых C# изменений прочитать csharp-code и профильные правила из AGENTS,
для тестов — unity-tests. Перед каждым Unity test/build запуском — repo smoke-check.
Новые игровые правила или missing content не заполнять настройками бота.

### Проверенные точки входа, которые следует переиспользовать

- `Assets/Game/Bootstrap/GameplayCompositionRoot.cs`: `ConfigureProfile`, `Play`,
  `TryStartCharacter`, `TryStartField`, `ProfileSaveTask`, `ReturnToProfileSelection`.
- `Assets/Game/Movement/Presenters/PlayerMover.cs`: общий расчёт движения/физики.
- `Assets/Game/Progression/Runtime/LevelUpDraftRuntime.cs`: `Select(id, revision)`,
  `Reroll(revision)`, `Banish(id, revision)`, events открытий/выборов/очереди.
- `Assets/Game/Meta/`: `ProfileCodec.Create/Decode/Encode`, `ProfileService`,
  `IProfileService.PurchaseAsync`, `ProfileRunBinding`, `MemoryProfileStore`.
- `Assets/Game/Telemetry/`: `PlaytestSession`, `RunTelemetryRecorder`, existing bounded
  aggregates; `Bootstrap/PlaytestComposition.cs` создаёт recorder только в development.
- `Assets/Game/Bootstrap/Diagnostics/Field001PerformanceBenchmark.cs` и
  `Bootstrap/Editor/Field001PerformanceBuild.cs`: пример CLI/build/profile isolation.
  Бессмертие, принудительная выдача билда и transform-перемещение этого benchmark
  запрещены в балансных сериях; существующий benchmark сохраняет своё назначение.

Известные ограничения для учёта в реализации: `setDetails` в telemetry unsupported;
часть боевой случайности привязана к `LifeId` GUID; standalone provenance сейчас не
получает commit через Editor-only Git path. AB-04/06 должны фиксировать реальные
coverage/build metadata, не обещая полного replay или per-set attribution.

## 3. Минимальная архитектура и ownership

| Слой | Ответственность и целевое расположение |
|---|---|
| Pure C# policies/contracts | `Assets/Game/Automation/`: experiment validation, observation DTOs, movement/draft/purchase decisions, campaign state machine. Один type на файл; отдельный asmdef, без зависимости на Bootstrap/UI |
| Unity adapters/composition | `Assets/Game/Bootstrap/Automation/`: read-only observation adapters, binding штатных commands, session lifecycle. Bootstrap зависит от Automation, обратной зависимости нет |
| Game-owned extension points | Минимальный источник движения в Movement и read-only snapshots/events у владельцев. Владельцы не зависят от Automation; Diagnostics остаётся foundation |
| Export/analysis | Существующий Telemetry + versioned automation sidecar. `scripts/balance/`: локальный запуск, сбор и сравнение. Python не моделирует бой и не начисляет награды |

Experiment JSON — внешний конфиг инструмента, образцы в `scripts/balance/examples/`.
Gameplay balance по-прежнему берётся из production catalogs; новые игровые numbers
не попадают в experiment или C#. Bot policy параметры — явно технические параметры
эксперимента, с units/ranges и resolved snapshot. Они не меняют характеристики игрока.

Автоматизация включается только явным флагом в отдельной development-сборке.
До normal profile/settings initialization подставляются изолированные stores;
если изоляцию не удалось установить, завершить запуск, не fallback-иться на player save.
Настройки окна/управления/звука также не должны переписывать пользовательские настройки.
В обычном запуске нет нового bot host, подписок и записи файлов.
Initialize/Shutdown симметричны, partial init откатывается в обратном порядке.

## 4. Контракт эксперимента и профиля

Обязательные группы config (точные DTO и schema examples поставляет AB-01):

| Поле | Семантика / validation |
|---|---|
| `schemaVersion`, `experimentId` | Версия формата; непустой ID. Неизвестная версия/поле — ошибка до запуска |
| `template`, `initialProfilePath` | Два template из §1; path обязателен только для preset, для fresh запрещён |
| `chains`, `maxRunsPerChain` | Положительные целые. N всегда уточняется: число историй профиля или забегов |
| `runSpeed` | Только 1/2/3/5; применение через RunController после Start каждого забега |
| `characterId`, `fieldRoute` | Один фиксированный герой в v1; ordered distinct field IDs. Герой доступен в начальном профиле, первое поле доступно и playable. Позднее закрытое поле допустимо в маршруте |
| `movementPolicy`, `draftPolicy`, `purchasePolicy` | ID, version и явные параметры; независимая случайность решений бота |
| `stopAfterRouteClear` | Явный bool; при false после завершения маршрута повторять последнее поле до лимита |
| `maxExperimentWallSeconds`, `runWallTimeoutSeconds`, `transitionTimeoutSeconds` | Конечные положительные wall seconds. Слишком малый budget разрешён как ограничение, но остановленный run отмечается incomplete, не loss |
| `outputDirectory` | Уникальный каталог внутри выделенного experiment root; не Assets/Resources, не player save. Существующие результаты не перезаписывать |

Новый профиль создаётся через `ProfileCodec.Create()` с реальными initial unlocks.
Не использовать `UnlockAllForDevelopmentAsync` в fresh. Preset — валидный serialized
`ProfileData`, прошедший `ProfileCodec.Decode`; копируется до каждой цепочки и больше
не меняет исходник. В AB-01 дать документированный локальный генератор preset из
декларативных starting currency/upgrades/unlocks/clearedFields, валидирующий каталог,
caps, personal ownership и стоимость уже купленных уровней для UpgradeSpending.
Не генерировать fake RunReceipt или выдавать эти grants за заработанную прогрессию.
Это лабораторная начальная точка; её hash и отличия от fresh обязательны в отчёте.

После каждого run сохраняются profile-before, terminal outcome, применённый receipt,
profile-after-reward и profile-after-purchases. Следующий run использует последний.
Другой chain начинает с исходного template, не с результата предыдущего chain.
Фиксированный профиль с восстановлением перед каждым run — возможная следующая опция,
не замена обязательной прогрессии двух campaign templates.

### Стратегии v1 — настройки бота, не продуктовые правила

- Драфт: `randomLegal` равновероятно выбирает только реально предложенные активные
  options; использует текущую captured revision. Никакой выдачи произвольного ID,
  изменения весов персонажа, unlocks, set thresholds или гарантированного сета.
  Базовый профиль не тратит reroll/banish: это явно записано. Поддержку их штатных
  commands проверить тестами; более умные стратегии не обязательны для первого MVP.
- Покупки: `cheapestPersonalUpgrade` после штатного сохранения результата покупает
  самый дешёвый доступный следующий уровень из явно заданного `allowedUpgradeIds`
  для выбранного героя; equal price → ordinal ID. После каждого успеха перечитать
  состояние/цену/expectedLevel. Остановиться при отсутствии доступных покупок или
  достижении явного `maxPurchasesPerIntermission` (целое >=0). Пустой список означает
  копить валюту. Не делать refunds, не покупать/менять героев автоматически.
- Карты: до победы повторять текущую карту; после победы перейти к следующей в
  `fieldRoute`, только если профиль открыл её и runtime умеет её запускать.
  Уже пройденные карты preset не заставляют пропускать первый элемент route:
  стартовая точка задаётся самим маршрутом. Недоступность следующей карты даёт
  `routeBlocked` с ID и причиной, не поражение и не скрытый возврат на другое поле.

Таким образом первые серии измеряют прогрессию одного фиксированного героя и явно
заданной стратегии расходов. Это ограничение отчёта; оно не представляет все стили игроков.

## 5. Контракт управления и lifecycle

Движение выдаётся как направление в общий PlayerMover до штатного расчёта скорости,
knockback и Rigidbody2D. Никаких transform teleport, изменённых colliders, immunity,
принудительного XP/draft/Book или изменения run duration в production-серии.
Обычные keyboard/mouse modes сохраняют поведение при отсутствии bot input source.

Первый movement bot — ограниченная эвристика: проверяет конечный набор направлений
и stop, отбрасывает пересечение player-only препятствий/границ, предпочитает меньшую
ближайшую угрозу и достижимый XP/полезный pickup, с hysteresis против дрожания.
Нужны enemy/projectile positions, радиусы/скорости, текущие видимые зоны угроз и
pickups в пределах наблюдения; hidden future RNG/будущие draft offers недоступны.
Не делать новый универсальный pathfinding. Для stuck — ограниченная смена направления,
затем запись botStuck; недоступный pickup пропускается (он может быть легален по GDD).
Радиусы, частота решений, горизонт предсказания и stuck timeout задаются в bot config.
AB-02 обязан документировать формулу оценки, units, допустимые диапазоны и пример;
выбрать проверяемые технические значения, не выдавать их за модель человека.

`safePickup/v1` оценивает stop и восемь направлений раз в
`decisionIntervalSeconds` игрового времени (0.02…2 s, пример 0.2). Радиус
наблюдения 1…50 world units (пример 12), горизонт `h` 0.05…3 simulation s
(пример 0.5), отступ от препятствий 0…2 world units (пример 0.15),
`stuckSeconds` 0.5…30 simulation s (пример 3). Все эти числа — техническая
конфигурация бота, а не значения игрока или модель человеческого поведения.
Для направления `d` и текущей позиции `p` прогноз игрока `p′ = p + d·v·h`,
где `v` — текущая штатная скорость в world units/s. У видимой угрозы
`q′ = q + velocity·h`; `exposure = max(0, (R−distance(p′,q′))/R)`,
`R = playerRadius + threatRadius + 2 world units`. Для beam distance — до
его отрезка, `R = playerRadius + halfWidth + 2`. Danger — сумма
`weight·exposure²` (enemy 3, projectile 10, zone 12, active beam 12,
beam warning 4, summon marker 1). Pickup attraction — сумма
`priority·(distance(p,target)−distance(p′,target))/max(1, distance(p,target))`
(XP 1, world pickup 2). Итог `attraction−10·danger` с hysteresis +0.1
предыдущему направлению; ties стабильны. Например, при player `(0,0)`,
скорости 3, XP `(3,0)`, h=0.5 и без угроз score движения вправо = 0.5,
а stop = 0.1, поэтому бот идёт вправо. При projectile `(2,0)` со скоростью
`(-2,0)` вес 10 делает прямое движение хуже уклонения.
Пересечение sweep с AABB препятствия, расширенным на player radius и padding,
и выход за bounds поля отбрасывают направление. Если прямая к pickup закрыта,
ближайший угол блокирующего AABB становится промежуточной целью; это локальный
detour, не pathfinding. Меньше 0.05 world units смещения в течение
`stuckSeconds` вызывает смену направления; после восьми неудачных попыток
отмечается `botStuck`. Переполненная или неподдержанная зона наблюдения
ставит `coverageIncomplete`, движение останавливается и не трактуется как
«угроз нет». В `v1` inverse-area `DangerWash` (safe-circles boss special)
пока отмечается неподдержанным, остальные видимые круги/лучи учитываются.
Период движения отсчитывается в simulation seconds; draft/intermission — wall time,
чтобы бот мог отвечать при gameplay pause. Никаких per-frame scene-wide FindObjects.

Campaign state machine:

`PrepareProfile → SelectRun → Running ↔ ResolveDraft → AwaitResultSave → Purchase → SelectRun`.

Любое состояние может завершиться `Completed`, `Stopped`, `Failed`; terminal reason
и потерянные данные записываются. `Completed` означает завершение задания по лимиту
или маршруту, не обязательную победу. Ручную/неизвестную паузу бот не снимает молча:
ждёт до transition timeout и пишет причину. Во время draft он не двигается.

Границы:

- Открытые/queued Book и XP drafts обрабатываются существующей очередью. 0 options
  разрешается владельцем; 1–2 options выбираются без создания дубликатов.
- Устаревший request/revision не повторять бесконечно: перечитать текущую сессию.
  После terminal outcome новые choices запрещены; результаты получает только RunModel.
- При смерти в последнем tick бот не определяет win/loss сам; читает authoritative
  `RunOutcome`. Награду применяет `ProfileRunBinding`, не automation второй раз.
- AwaitResultSave ждёт `ProfileSaveTask` и проверяет успешность/receipt. Ошибка сохранения
  останавливает chain; нельзя продолжать с придуманным балансом валюты.
- Stop, watchdog, crash и wall-budget не становятся loss. Даже если штатный Stop
  законно начислил награду, chain после такого run заканчивается и помечается truncated.
- На crash частичный пакет сохраняется; автоматического повтора того же run/chain нет.
  Так не исчезают неудобные результаты. Новый явный запуск имеет новый experiment ID.
- Между runs все подписки/пулы проходят штатный teardown. Новая цепочка не наследует
  профиль, RNG политики, movement intent и runtime state предыдущей.

## 6. Пакеты реализации

Зависимости в таблице описывают состав работ, не дублируют текущую очередь STATUS.
Один packet — самостоятельная задача для следующей модели. Не объединять packets
в большой переписывающий refactor. Сначала сверить актуальный код: существующие API
могли расшириться после записи этого плана.

<a id="ab-01"></a>
### AB-01 — Конфигурация эксперимента и изолированные профили

**Вход:** IP-25 и этот контракт. **Выход:** Automation contracts, строгая загрузка
experiment JSON, два examples и preset preparation; фабрика fresh/preset isolated
stores и immutable resolved config. Пока без бота и запуска серии.

**Приёмка:** malformed config, неизвестный ID, invalid personal levels, locked
starting character/field и output collision отвергаются до игры; two chains не
разделяют mutable profile; fresh равен текущему канону; preset исходник неизменен;
тестовый sentinel production save/settings не читается/не записывается.
**Checks:** Automation/Meta EditMode, schema fixtures, profile copy/round-trip.

Формат v1 и готовые конфигурации: [`scripts/balance/examples/fresh.json`](../../../scripts/balance/examples/fresh.json),
[`preset.json`](../../../scripts/balance/examples/preset.json). Декларативная
исходная точка создаётся [`prepare_preset.py`](../../../scripts/balance/prepare_preset.py)
по [инструкции](../../../scripts/balance/README.md); результат повторно проходит
`ProfileCodec` при загрузке. `ExperimentConfigLoader` фиксирует копию стартового
профиля в момент валидации, так что изменение исходного файла после неё не меняет
цепочки незаметно.

<a id="ab-02"></a>
### AB-02 — Наблюдение и движение бота

**Вход:** AB-01. **Выход:** небольшой input seam в Movement, read-only observation
adapters и movement policy из §5; явный diagnostics-only coverage snapshot.
Если угрозу нельзя наблюдать, добавить минимальный snapshot у владельца; не читать
private fields reflection и не принимать отсутствие наблюдения за отсутствие угрозы.

**Приёмка:** бот собирает достижимый XP в prepared scene, обходит препятствие,
выбирает безопасное направление перед известным projectile, корректно останавливается
на паузе/конце; stuck detection воспроизводится; обычное keyboard/mouse управление
проходит регрессию. Изменение observer/policy не потребляет gameplay RNG.
**Checks:** pure policy tests + Movement/Automation PlayMode fixtures; полный smoke
для реально изменённых общих movement/combat/lifecycle contracts по WORKFLOW §9.

<a id="ab-03"></a>
### AB-03 — Один автономный забег

**Вход:** AB-01/02. **Выход:** development host, randomLegal draft, выбор героя/поля
через существующие launchers, run state machine до результата и сохранения.
Проверочный запуск внутри Editor допускается через тестовый harness, не новую UI-панель.

**Приёмка:** fixture win/loss/stop, Book chain, short/empty draft, stale revision,
manual pause timeout, save failure и teardown; ни одного debug grant/health lock.
Один production FIELD-001 run заканчивается своим естественным исходом с доступной
телеметрией; победа не является критерием качества реализации бота.
**Checks:** Automation + Draft + Meta targeted tests; composed production smoke.

<a id="ab-04"></a>
### AB-04 — Отчёт о забеге и развитии

**Вход:** AB-03 и IP-31. **Выход:** schema §7, typed automation sidecar, provenance,
profile snapshots и фактическая история drafts/build/phases. Переиспользовать
измеренные counters telemetry; не парсить human-readable summary ради данных.
Новые adapters/events остаются у владельца, recorder только потребитель.

**Приёмка:** known fixture totals, damage vs healing/rescale, phase at death,
offered vs selected, позднее получение skill/set, неполное покрытие, переполнение
буфера, duplicate finalization, сбой экспорта; unavailable остаётся null/unsupported.
Частичный файл не считается complete; запись не выполняется в combat callback.
**Checks:** Telemetry/Automation unit tests, export integration и composed smoke.

<a id="ab-05"></a>
### AB-05 — Цепочки профиля и прогрессия по картам

**Вход:** AB-03/04. **Выход:** оба campaign templates, purchase/route policies §4,
межзабеговое сохранение и независимые chains, лимиты и причины остановки.

**Приёмка:** fixture loss даёт ровно штатную награду; покупка расходует её один раз
и усиливает следующий run; победа открывает следующее поле через обычный профиль;
запрещённая/не реализованная карта даёт routeBlocked; next chain сбрасывается к
начальному template; preset копируется; interrupted chain не продолжается автоматически.
**Checks:** Meta/Automation integration, повторный result callback, max caps,
недостаток валюты, отсутствие покупок, terminal route и два независимых chains.

<a id="ab-06"></a>
### AB-06 — Standalone delivery и локальный runner

**Вход:** AB-05. **Выход:** отдельный development build/flag, Python validate/run
commands, progress manifest/heartbeat, log/exit contract, worker=1, output isolation.
Целевой интерфейс (появится после packet):

```text
python scripts/balance/run.py --experiment <json> --player <exe> --output <new-directory>
python scripts/balance/analyze.py <experiment-directory>
python scripts/balance/compare.py <baseline-directory> <candidate-directory>
```

`analyze`/`compare` реализует AB-07, run не должен вызывать заглушки как готовый анализ.
Standalone получает build manifest (commit, dirty, Unity, executable/data hashes),
не пытается получить commit через несуществующий Editor Git path. Версию фиксируем
один раз на experiment; изменённые входные файлы требуют нового запуска.
Новый builder — сосед performance builder; не переписывать существующий benchmark.
Запуск helper-процесса на Windows — скрытый, без popup shell.

**Приёмка:** child success/crash/nonzero/hang, истечение wall budget, cancellation,
невалидный exe/config, повторный output path; результаты уже завершённых runs целы.
Плановая остановка с partial manifest отличается от execution failure. Runner
останавливает только собственный child, не закрывает Editor/другую игру. Нет silent retries.
**Checks:** Python unittest с fake executable/process adapter, отдельный настоящий
standalone smoke. Build/tests — по smoke-check; batch Editor не запускается поверх
открытого Editor. Использовать поддержанный безопасный путь или честно сообщить blocker.

<a id="ab-07"></a>
### AB-07 — Статистика и сравнение серий

**Вход:** schema AB-04 и outputs AB-06. **Выход:** Markdown summary + CSV/JSON таблицы,
проверка совместимости условий, агрегаты §7. Python standard library достаточно для
v1; dashboard/новая БД/ML dependencies не требуются.

**Приёмка:** exact fixture totals и denominators, empty/all-incomplete набор,
разные длительности/поля/профили, отсутствие побед, short chains, повреждённый report,
разные версии политики, разные скорости, incomplete milestones. Любой исключённый
run указан с причиной; favorable-only filtering запрещён.
**Checks:** Python unit tests на малых hand-calculated наборах; trace строки сводки
до experiment/chain/run ID. Сравнение не меняет config, код или production balance.

<a id="ab-08"></a>
### AB-08 — Пилот, измерение скорости и передача

**Вход:** AB-01…07. **Выход:** инструкция в `scripts/balance/README.md`, примеры двух
templates, инструкция запуска/остановки/чтения отчёта; evidence в implementation/evidence.

**Приёмка:** не менее двух independent chains по два естественно завершённых runs
на каждый template, с реальными output IDs. Итого минимум восемь production runs;
раннее поражение допустимо. Начинать с доступного FIELD-001, включать другие только
при валидных prerequisites; нулевая частота побед не скрывается и не запускает nerf.
Fixture acceptance AB-05 отдельно доказывает межкарточный переход, если production
бот пока не победил. В evidence различать эти два вида данных.

Проверить хотя бы один естественный production run на 1× и серию на выбранной
штатной скорости. При разных результатах не объявлять эффект скорости доказанным
по одной паре; проверить механические regressions и записать ограничение.
Провести отдельное измерение wall window 600 s на выбранном режиме: completed W/L,
incomplete/error, суммарные simulation seconds, затраты переходов и hardware/build.
Текущий незавершённый run после лимита остаётся censored; не терять его из отчёта.

**Checks:** полный `python scripts/check_project.py --scope full --graphics` после
стабилизации runtime и все Python tests; безопасная процедура Unity обязательна.
Production series не заменяют tests; зелёные tests не утверждают хороший баланс.
Сравнить production save/settings sentinels до/после; повторный обычный запуск
работает без automation. Записать ограничения бота и фактическую пропускную способность.

<a id="ab-09"></a>
### AB-09 — Отдельный профиль движения за опытом

По поручению пользователя добавить `experienceFocused/v1` как выбираемый ID
`movementPolicy` без изменения `safePickup/v1`, gameplay, драфта и покупок.
Оба профиля получают одинаковое видимое наблюдение и используют тот же контракт
границ, препятствий, coverage и stuck. Отличие только в оценке: XP gain получает
множитель 6, world pickup gain — 0.5, danger — 5 вместо 10. Gain и danger
определены в §5; множители безразмерны, относятся к технической эвристике, не
балансу игры. При XP `(3,0)`, скорости 3 и горизонте 0.5 s движение вправо
даёт gain 0.5 и score 3 без угроз. При отсутствии XP профиль по-прежнему
оценивает world pickups и угрозы. Нет гарантии собрать каждый drop или выжить.

**Приёмка:** оба ID выбираются отдельными JSON-конфигами, неизвестный ID
отклоняется; unit tests подтверждают приоритет XP перед обычным pickup,
допуск умеренного риска и избегание projectile; прежний профиль не меняет
решения в этих сценариях. После полного smoke провести хотя бы один тихий
реальный прогон и записать XP/result без заявления об улучшении win rate
по малой выборке.

**Checks:** policy/config EditMode, полный smoke-check, Python tests и один
обычный headless pilot на новом build. Видимый запуск — только по запросу.

<a id="ab-10"></a>
### AB-10 — Широкий обход к XP

Отдельный `orbitExperience/v1` выбирает одну из шести ближайших различных
видимых XP-целей, пропуская те, для которых безопасный маршрут не найден.
Если прямой
коридор к ней пересекает заметную угрозу, оценивает две дуги с промежуточной
точкой `w± = (p+x)/2 ± n·a`, где `p` — игрок, `x` — XP, `n` — единичная
нормаль к `x−p`, `a` — `arcOffsetWorldUnits` (2…10 world units, пример 6).
Камера Gameplay имеет высоту 10 world units: пример соответствует 0.6 высоты
экрана, значение 10 — полной высоте. Выбор стороны минимизирует оценку угроз
на двух сегментах маршрута; точки вне поля и пути через player-only obstacles
отклоняются. Если нет допустимой дуги, бот не проталкивается сквозь угрозу,
а возвращается к безопасной локальной оценке. Удерживает цель и waypoint,
пока XP видим и waypoint не достигнут (порог 1 world unit), чтобы избежать
дрожания. Не видит будущие спавны и не гарантирует безопасный маршрут.

**Приёмка:** отдельный ID в JSON, два прежних профиля неизменны; tests на
выбор широкой стороны, отказ от опасной ближайшей цели, устойчивость waypoint,
переход к XP, исчезновение XP,
границы/препятствия и отбрасывание неизвестной версии/ID. Тихий production
pilot с отчётом о выживании и XP, без заявления о статистическом преимуществе.

**Checks:** pure EditMode policy/config tests, полный smoke-check, Python tests,
отдельный build и один headless run. Визуал — только по явному запросу.

<a id="ab-11"></a>
### AB-11 — Заманивание толпы и возврат за XP

Отдельный `herdLoop/v1` следует видимому состоянию, без знания будущих спавнов.
В `Forage` при малом числе врагов выбирает ближайший XP и локально уклоняется.
Если в радиусе `crowdRadius` (3…20 world units; пример 8) видно не меньше
`crowdMinEnemies` (4…30; пример 8) врагов и минимум три врага перекрывают
коридор к XP дальше 2 units, переходит в `Lure`: уводит
кучу от запомненного XP примерно на `arcOffsetWorldUnits` (2…10; пример 6).
Через `lureSeconds` (1…15 simulation s; пример 7) либо при достижении 80%
дистанции переходит в `Sweep`: несколько секунд (`sweepSeconds` 1…15;
пример 5) движется по касательной вокруг видимого центра кучи, пока
коридор к XP не освободится (но не дольше удвоенного срока). Затем
`Collect` возвращается к запомненной области XP на срок `collectSeconds`
(1…30; пример 10); если XP вышел из видимости, память о точке сохраняется
только до конца этого короткого режима. Порог 35% HP прерывает сбор при
плотной куче. Все смены режима происходят в одном планировщике; пауза и
конец забега не продвигают игровые таймеры.

Локальный scorer оценивает худшую видимую опасность на трёх точках вперёд
(`h`, `2h`, `3h`, где `h` — `predictionSeconds`), а не только в конечной точке.
Штраф за опасность растёт линейно от 4 при полном HP до 9 при нуле HP;
притяжение goal зависит от режима (Forage 10, Lure/Sweep 8, Collect 14).
Эти безразмерные числа — техническая эвристика, не продуктовый баланс.
В отчёт добавляется число решений в каждом режиме. Старые профили и их
коэффициенты сохраняются.

**Приёмка:** tests подтверждают режимы при большой куче, локальный сбор при
малой, что снаряды не считаются врагами, сброс при неполном наблюдении,
валидацию config. Полный smoke, Python checks, один тихий production pilot;
результат записать вместе с expired XP и не выдавать один run за доказанное
улучшение. Визуал — только по отдельному запросу.

<a id="ab-12"></a>
### AB-12 — диагностический трек и адаптивный обход

`herdLoopAdaptive/v1` — отдельный исследовательский профиль: не меняет
поведение `herdLoop/v1` и игровых систем. В `Forage` сначала выбирает
ближайший видимый XP с коридором, перекрытым менее чем тремя врагами; если
такого нет, допускает цель за толпой. В `Sweep` переходит в `Collect` только
если проход открылся; после `2 × sweepSeconds` при закрытом проходе забывает
цель и на `collectSeconds` не выбирает XP в радиусе 1.5 world units от неё.
При HP ниже 50% закрывшийся в `Collect` проход также прерывает попытку.
Пауза/драфт не продвигают эти таймеры. При отсутствии другой видимой XP-цели
бот продолжает локально избегать опасностей, без знания будущих спавнов.

Для обоих herd-профилей sidecar содержит `movementTrace`: примерно один
снимок в simulation second и при смене режима с временем, позицией,
целью, направлением, HP fraction, числом врагов в радиусе толпы,
перекрывших текущий коридор и коридор к запомненному XP врагов, видимых
pickups и итоговым score. Память
ограничена последними 1024 записями; `movementTraceDropped` сообщает,
сколько ранних записей вытеснено. Это диагностический трек, не replay:
позиции спавнов и действия gameplay RNG он не восстанавливает.

**Приёмка:** EditMode tests проверяют выбор открытой цели, возврат после
открытия прохода, отказ от закрытой цели и сохранение старого профиля.
Полный Unity smoke, Python runner tests и несколько тихих production runs;
отчёт с XP, временем, режимами и ограничениями. Сравнение не объявлять
доказательством превосходства при разных seeds. Визуал не запускать без
отдельного запроса.

<a id="ab-13"></a>
### AB-13 — поиск целых траекторий с моделью преследования

Поручение 2026-09-29: ограниченный эксперимент с планированием маршрутов
вместо дальнейшего наращивания FSM. Отдельный `trajectorySearch/v1` использует
pure C# forward model и shooting search маршрутов с двумя промежуточными
точками, XP-целью и точкой выхода после сбора. Все кандидаты оцениваются на
одинаковом полном горизонте, в том числе после подбора XP. На каждом решении проверяет сохранённый маршрут, прямые
пути, выходы из опасности и выборку обходов; уточняет лучшие точки. Выполняется
только первое направление, затем план пересчитывается по свежему наблюдению.

Дополнительный Context: AB-02/12, `BotObservation`, `EnemyMovementController`,
`ExperienceDropRuntime`, текущие movement/XP/radius contracts. Gameplay и
параметры контента не меняются; сущности в прогнозе не изменяют живую сцену.
Ordinary Seek в модели поворачивает к прогнозируемому игроку; KeepDistance использует
текущие speed/range/tolerance. Прочие движения и летящие снаряды продолжают
текущую скорость, число таких врагов записывается. Будущие спавны/атаки,
убийства и сложные фазы, динамические status expiry и столкновения врагов
между собой не предсказываются. За пределами observation radius состояние
неизвестно; non-XP pickups видны, но в целевую функцию не входят.
Нужны явные оговорки при анализе переноса.

`trajectory` обязателен только у нового ID: `horizonSeconds` 4…15 simulation s,
`stepSeconds` 0.1…0.5 s, `candidateCount` 16…256, `contactPenalty` 10…200
безразмерных единиц score на risk-second, `clearance` 0…1 world unit.
Образец: 8 s, 0.2 s, 96 кандидатов, penalty 60, clearance 0.25.
`predictionSeconds` остаётся обязательным legacy полем общего schema, но
новый planner использует `trajectory.horizonSeconds/stepSeconds`.

Score = `5 × weightedXp + 1.5 × progress − penalty × (2 − hpFraction) ×
(riskSeconds + 0.5 × exitRisk) + 0.05 × continuity − driftCost`.
`weightedXp` — реально пересечённые в прогнозе радиусом подбора XP drops,
каждый максимум один раз, value из runtime и только до remaining lifetime;
вес во времени `1 − 0.25 × t / horizon`. `progress` — относительное сокращение
расстояния к цели, ограниченное −1…1 (1 после достижения радиуса цели);
`hpFraction` 0…1; `continuity` — dot
первого и предыдущего направлений (−1…1). `riskSeconds` — интеграл максимального
взвешенного геометрического exposure на шаге; `exitRisk` — минимум exposure
среди восьми полусекундных выходов из конечной позиции. Exposure линейно
растёт от 0 на границе clearance до 1 при контакте; threat weight нормирован
на обычного врага (3). `driftCost` — 0 при видимом XP, иначе 0.05 × смещение
конечной точки от начала в world units: мягкое предпочтение локальному обходу
вместо бесконечного прямого отхода. Это proxy риска, не прогноз урона в HP. Например,
1 XP на 4-й секунде из 8, полный progress, без риска и continuity дают 5.875.
Маршруты сквозь arena bounds и player obstacles исключаются.

Собственный RNG поиска не трогает gameplay RNG; сохранённый маршрут сбрасывается
при неполном наблюдении и stuck recovery. При паузе решения/таймеры не идут;
после конца забега host больше не управляет игроком. Пустые pickups оставляют
кандидаты ожидания/выхода и кривые обхода с возвратом; просроченный XP не даёт награды. Предсказанный путь,
XP, risk, число кандидатов, linear coverage и elapsed wall ms планирования идут
в bounded trace. Это длительность вызова, а не CPU time и не frame time.

**Приёмка:** воспроизводимые closed-loop сценарии с реальным Seek controller
в тестовой среде: XP за толпой, плотная толпа, препятствие и снаряд; сбор и
отсутствие геометрических контактов, отдельно budget test на 200 угроз.
Это не ручная приёмка человеком и не полноценный physics/combat replay.
Полный Unity smoke, Python checks, отдельный player и тихая production серия;
записать XP/смерти/expired XP и стоимость планирования. Если перенос плох,
зафиксировать пределы эксперимента, не называть его хорошим балансным ботом.

<a id="ab-14"></a>
### AB-14 — запись человеческих демонстраций движения

Поручение 2026-09-29: после интеграции `develop-evg` подготовить запись игры
человека. Обучение/ML-Agents, новая policy, видео и deterministic replay не входят
в этот packet. Context: AB-01/03/04/06, GDD «Управление, бой и выживание», «Опыт
и level-up», `PlayerMover`, active skill cooldown, run pause/outcome и существующие
observation/profile/export adapters; карточки примера CHAR-001 и FIELD-001.

`movementPolicy.id=human`, version 1, сохраняет штатный keyboard/mouse input;
остальные IDs сохраняют прежнее управление. Human требует 1×, одну chain и
`demonstration` config. Наличие config у bot разрешено для технических проверок,
но `controller=bot` в файлах исключает выдачу таких данных за человеческие.
Legacy movement settings всё ещё проходят schema validation, но в human не
управляют движением. Draft остаётся `randomLegal`; пример не покупает meta upgrades.
Профиль и settings изолированы существующим standalone bootstrap.

Опциональный `demonstration` отключает запись при отсутствии. Обязательные поля:
`schemaVersion=1`, `sampleIntervalSeconds` 0.02…0.5 simulation s,
`maxSamples` 1…1000000, `maxFileMegabytes` 1…2048 MiB,
`queueCapacity` 1…1024 сериализованных frames,
`maxEntitiesPerCollection` 1…2048. Пример: 0.1 s / 20000 / 256 MiB / 128 / 512.
Это пределы инструмента, не gameplay tuning; файл ограничен header + samples + footer.

Owner движения публикует текущий clamped analog intent непосредственно перед
применением velocity в FixedUpdate, только при Running. Recorder получает пару
«состояние перед физикой / действие этого шага»; action duration = fixedDeltaTime,
а не расстояние между редкими записями. Отдельные physics step/time и run elapsed
сохраняются: run clock может не меняться между несколькими fixed steps одного frame.
При 0.02 s physics и 0.1 s sampling первые samples на 0, 0.1, 0.2 physics seconds.
Фактические timestamps важнее номинального интервала. Предыдущее действие, HP,
XP/level, сборка, remaining cooldown, позиции/скорости угроз, XP, препятствия и
viewport входят в raw versioned JSONL; скрытые будущие RNG/spawns не читаются.

Сериализованные строки идут через ограниченную очередь в background file writer;
он не обращается к Unity objects. Переполнение samples/bytes/очереди и I/O error
не блокируют physics: фиксируют ошибку, прекращают запись и приводят host к
контролируемой остановке. Усечение списков и неполное hazard coverage помечаются
в sample; их нельзя молча использовать как полное наблюдение.
`demonstration.jsonl.partial` остаётся при сбое/оборванном процессе; чистый footer
и flush/close предшествуют переименованию в `demonstration.jsonl`.
Полнота записи не означает победу, экспертное качество или полный replay.

UI/observability: отдельный recorder launcher явно открывает SafeWindow без
звука (audio opt-in). Каждый human run стартует на штатной manual pause;
Space/Escape/«Продолжить» начинает движение. Manual pause и потеря фокуса не
записываются; фокус не снимает manual pause автоматически. Общий wall budget
действует и во время ожидания. Human не имеет короткого bot manual-pause timeout.
Закрытие окна запрашивает штатный stop и ожидает export; жёсткое убийство процесса
может оставить partial. При death/end запись отписывается, writer завершается;
host не объявляет экспорт завершённым до окончания writer. Новый HUD не нужен.

Приёмка: strict config и backward compatibility; pure recorder clock/writer
tests (ordering, limits, Unicode bytes, queue/I/O failure, idempotent completion);
PlayMode real movement intent, старт/пауза/end/cleanup без подмены обычного input;
Python validator/launcher tests; full graphics smoke, отдельный player, короткий
headless bot-labelled recording pilot и проверка JSONL. Реальная человеческая
демонстрация требует следующего явного запуска с участием пользователя и не
подменяется автоматическим smoke. Канон и баланс не изменяются.

## 7. Артефакты, метрики и корректное сравнение

```text
TestResults/balance/<experiment-id>/
  experiment.json          # resolved config + build/bot/schema metadata
  manifest.json            # все начатые chains/runs и их terminal/partial state
  summary.md, runs.csv, chains.csv
  chains/<chain-id>/
    initial-profile.json
    runs/<run-id>/
      run.json             # existing telemetry format, если доступен
      automation.json      # typed progression/choices/phase/profile metadata
      profile-before.json
      profile-after-reward.json
      profile-after-purchases.json
      player.log
```

Raw outputs не коммитятся автоматически; малые synthetic fixtures — в tests,
избранные результаты — по existing playtest/evidence process. Output root явный,
без auto-delete/retention. Final manifest записывается атомарно после flush данных.
Large histories — bounded/chunked; dropped counters и I/O errors обязательны.
Агрегаты сохраняются отдельно от timeline, чтобы overflow истории не скрывал totals.

Обязательные данные:

- experiment/chain/run IDs, run index в chain, template/policy versions; build,
  content/config/preset hashes, explicit overrides, available seeds и RNG coverage.
- character, field, timeline, runSpeed; start/terminal wall timestamps, running
  simulation seconds, pause/transition durations; outcome отдельно от completion reason.
- profile до/после, actual purchases/currency/receipts/unlocks и причины отказов.
- reached/death phase ID и index; phase-entry events, HP/level/XP на переходах;
  phase index сравним только внутри одного field/timeline.
- offered/selected IDs с levels, request origin/revision, timestamp; build changes
  и acquisition time сетов; сбор XP и applied damage/healing из поддержанных adapters.
- completeness/capabilities, errors, timeouts, botStuck. Неполный set-damage attribution
  не блокирует run/outcome/build metrics и не превращается в нулевой урон сета.

Сводка обязательно разделяет:

1. **Забеги:** W/L, duration/level/phase distributions по карте, герою, номеру попытки,
   template и starting upgrade state. Глобальное среднее выводить только с составом групп.
   Для каждой волны показывать reached, deaths и число runs с доступным наблюдением;
   уровень/HP на поздних минутах относится только к дожившим, с явным размером выборки.
2. **Истории профиля:** попытки и накопленное игровое время до первой победы/открытия
   каждого поля; расходы, уровни меты, потолок достигнутого маршрута. Недостигнутое
   событие — censored/не достигнуто к лимиту, не 0 и не бесконечность.
3. **Работу инструмента:** requested/started/completed counts, errors, incomplete,
   running time vs wall time, причины остановки. Быстрая серия ранних смертей не
   доказывает высокую скорость расчёта полных забегов.

Формулы (counts целые >=0, секунды >=0):

- `winRate = W / (W + L)` при `W+L>0`, иначе unavailable. W/L — только естественные
  wins/losses. Пример: 8 W, 12 L, 2 timeout → 40%, denominator 20; timeout=2 виден рядом.
- `effectiveSpeed = sum(runningSimulationSeconds) / experimentWallSeconds` при
  положительном wall denominator, учитывая и partial runs. Пример: 1800 игровых
  секунд за 600 wall seconds → 3×, независимо от выбранных 5×.
- `completedRunsPer10Minutes = 600 * completedRuns / experimentWallSeconds` при
  положительном denominator. Пример: 4 завершения за 600 s → 4; дополнительно дать
  среднюю игровую длительность, чтобы показатель не маскировал ранние смерти.

Runs одной campaign зависимы из-за общей меты. Их нельзя считать независимыми
наблюдениями для доверительного интервала общего win rate. V1 выдаёт описательные
распределения и число independent chains, без p-value/заявлений значимости.
При малом N вывод — предварительная статистика. Интервалы/cluster bootstrap возможны
позже по историям профиля, а не по всем runs как независимым.

Для baseline/candidate требуются одинаковые template, initial profile, route,
character, bot policy/version, budgets и runSpeed. Build/content hashes могут
различаться как предмет эксперимента и показываются явно. Случайные серии независимы;
paired-seed comparison не требуется. Финальные профили закономерно могут различаться.
Разные starting conditions/policies → отдельные группы, не скрытое объединение.
Сравнение skill damage/pick rate — описательная связь, не доказательство причинности:
учитывать exposure/time acquired, уровень и доступность предложения.

## 8. UI / observability

Нового player-facing UI нет. Progress — stdout и manifest: chain/run/field,
simulation time, profile stage, completed/error counts, output path. Существующий
UI может отрисовываться, но commands вызываются через игровые launchers/runtime;
это не black-box проверка кликов, layouts или читаемости.
В policy/observer/hot-path нет I/O, неограниченных списков и per-frame string formatting.
Scale-sensitive loops используют PerfGuard; наблюдение имеет ограниченный scope.

## 9. Работа следующей модели и завершение

1. Проверить STATUS и git diff; выбрать только названный packet или первый Ready
   в IP-34 очереди после разрешения на реализацию. В этом запросе поручена запись плана.
2. Прочитать packet + его Context, объявить scope и tests; перевести только его
   status в In progress. Не создавать заново уже существующие contracts.
3. Реализовать packet, выполнить checks, обновить STATUS/evidence и потребителей.
   Технические решения в рамках этого плана не требуют нового продуктового approval.
   Не переключать модель/effort автоматически: их выбирает пользователь.
4. При единичном поручении на packet остановиться после него; серию packets выполнять
   только если пользователь разрешил её явно. Никаких дополнительных agents по умолчанию.
5. После AB-08 не запускать оптимизатор, новые поля или LLM API автоматически.

Documentation impact: GDD/CD и игровые балансные значения не изменяются. Новые API,
политики бота, schema и инструкции — в owning code и `scripts/balance/README.md`.
STATUS остаётся единственным execution registry; этот файл не получает текущие statuses.
Завершение каждого packet не закрывает остальные acceptance IP-34.

Стартовый запрос для передачи:

> Реализуй IP-34 packet AB-01 по docs/implementation/modules/IP-34-automated-balance-runs.md.
> Сначала проверь STATUS и WORKFLOW. Только AB-01, без ускоренного симулятора,
> deterministic replay, изменения production balance и запуска следующих packets.

## 10. Последующие поставки и gates

- **Улучшение бота/покрытия:** structured draft strategies, reroll/banish policies,
  разные герои, reset-per-run режим, калибровка по человеческим прогонам. Сначала
  измерить bot failure и доступность угроз; слабый бот не является поводом облегчать игру.
- **Поиск багов:** scenario/fuzz policies и доказуемые инварианты с reproducible fixture,
  deduplication. V1 уже сохраняет exceptions/stuck/incomplete, но не обещает полный QA.
- **Подбор чисел:** отдельный implementation packet после v1. До его начала нужны
  целевые метрики/диапазоны, allowlist authoring paths и bounds, бюджет эксперимента,
  held-out случайные серии и процесс принятия. Candidate sources изолированы;
  генератор выпускает их outputs. Production apply — конкретный согласованный diff
  по BALANCE_WORKFLOW; не редактировать generated JSON и не переписывать механику.
- **Vision reviewer:** отдельный packet на кадры/клипы, sample/storage budget,
  модель/стоимость/передачу данных. Real-time rendered sessions, timestamps + telemetry,
  hypotheses отдельно от подтверждённых багов; AI не становится контроллером движения.
- **Скорость:** сначала измерения AB-08. Headless, parallel workers и fastest-possible
  simulation рассматриваются отдельно; результаты не обещаются исходя из числа CPU cores.

Отсутствие целевого win rate, поздних полей или внешнего AI не блокирует измерительный
v1. Неполная production-прогрессия должна отражаться в route scope и отчёте.
