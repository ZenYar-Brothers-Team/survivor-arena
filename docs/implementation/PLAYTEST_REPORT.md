# Local playtest report v1

Контракт IP-31. Execution status — только [STATUS](STATUS.md). Процесс анализа/approval: [BALANCE_WORKFLOW](BALANCE_WORKFLOW.md). Это diagnostic schema, не gameplay balance.

## Получить пакет

В Editor/Development build открыть Gameplay и начать обычный прогон. DEV → Playtest показывает session ID и состояние записи. Ввести наблюдение (до 512 символов) и нажать Add marker: timeline сохранит marker ID и simulation time. Export во время игры создаёт явно `incomplete` packet; после victory/defeat/abort финальный packet создаётся автоматически. После финализации комментарии добавляются в `feedback.md`.

Путь: `Application.persistentDataPath/Playtests/<reportId>/`; абсолютный путь показывается после успешного экспорта. Windows обычно использует `%USERPROFILE%/AppData/LocalLow/<Company>/<Product>/Playtests/`. Файлы: `run.json`, `summary.md`, `feedback.md`. Повторная запись сохраняет report ID и feedback. Автоматического upload/retention нет; старые каталоги удаляет tester. В Assets/Resources ничего не пишется. Crash recovery не обещается.

Release использует DisabledPlaytestSession без подписок. Ошибка создания recorder пишет предупреждение и оставляет игру доступной; ошибка экспорта видна во вкладке, Export позволяет повторить попытку. UI не завершает run и не начисляет rewards. Остановка игровой сессии формирует Aborted через RunController, если run ещё не завершён.

## Schema и единицы

JSON: camelCase, `schemaVersion: 1`, `recorderVersion: IP-31/1`. Seconds — секунды, damage/heal — HP, XP — единицы опыта. `null`, `unknown`, `unsupported` не означают 0. Отсутствующий counter означает отсутствие наблюдённых событий только в пределах указанной coverage.

| Поле | Значение |
|---|---|
| reportId / runId / createdUtc | Пакет, игровая сессия, UTC начала записи; стабильны при повторном export |
| completionReason | completed/aborted/retry/error/incomplete; victory/defeat отдельно в outcome.reason |
| outcome | Immutable feature-owned RunOutcome; time, kills, level, build, acquired sets работают без recorder |
| provenance | commit либо unknown, nullable dirty, platform, Editor/Development, fixture marker |
| provenance.files | Resource path, SHA-256 точного UTF-8 текста, сам JSON; сохранены вместе с cached catalog |
| provenance.configHash | SHA-256 ordinal file list + canonical resolved settings; data/override меняют hash |
| provenance.resolved | Starting character/timeline, initial stats/build, run config, actual duration override, draft/wave seeds |
| provenance.rngUncovered | UnityEngine.Random spawn positions; нет deterministic replay. Authored skill seeds доступны в source snapshots |
| runningSeconds / elapsedSimulationSeconds | RunModel.Elapsed, без паузы |
| wallSeconds / pauseWallSeconds | Monotonic wall time до terminal; pause отдельно, ownership reasons — timeline |
| combat[] | source ID, captured nullable skill level, origin, target category, healing, results, attempted/applied/overkill |
| appliedDamageDealt / Taken / actualHealing | Measured results доступных adapters; max-HP rescale не healing |
| observedRunDps | appliedDamageDealt / runningSeconds при положительном времени, иначе null |
| combat[].equippedRunningSeconds | Simulation time с первого приобретения source ID; неизвестный source → null |
| combat[].observedEquippedDps | Applied HP этого source/level/category bucket / equipped time; не теоретическая сила skill |
| counters | Kills по content ID; draft resolution/control success/failure; XP collected/expired/intervention base и award отдельно |
| producers | XP totals, dropped/ground base, level/progress; build/sets/draft totals; ordinary kills; ordinary skill activation counts |
| producers.capabilities | Подключённые adapters; set effect/character baseline details явно unsupported |
| timeline[] | sequence, simulation seconds, kind, detail: state/pause, queue/offer/resolution/control, level, marker, legacy wave phase/hook |
| quality | Dropped counts, budgets, unsupported coverage, incomplete reason, direct Health limitation и контракт отображения I/O errors |

Примеры: 100 damage в target с 10 HP → attempted 100, applied 10, overkill 90. 300 applied за 60 simulation seconds при 20 wall seconds паузы → 5 HP/s. Base XP и awarded XP после modifiers нельзя складывать как независимые источники. Ground XP — зарегистрированные и ещё не снятые drops; cleanup/escape не kills.

## Coverage и lifecycle

PlayerCharacterRuntime и ordinary spawner пересылают существующие CombatResult с source/target identity, captured до callbacks. Source/level переживают despawn и level-up; lethal result доставляется после synchronous death/pool return. Recorder финализируется на LateUpdate/export boundary после возврата callbacks. Повторный terminal/export использует тот же immutable final snapshot.

XP producer хранит dropped/ground totals без per-frame scan. Draft публикует queued request и control attempt с request/revision до смены offer. RunOutcomeContribution хранит acquired sets отдельно от build и в release. Diagnostics не становятся gameplay dependency.

Legacy continuous wave phase/hook — наблюдения текущего fixture, hook не считается boss spawn. Boss, Traveler, meta, wave cap decisions, phase combat aggregation, set effect details, per-projectile counts, FPS/p95 не поддержаны. Прямой вызов Health минуя feature adapter не даёт combat attribution. Missing adapter обозначен capabilities; такой тест нельзя интерпретировать как нулевой урон отсутствующей системы.

## Budgets и стоимость

До implementation зафиксированы: 2048 timeline entries, по 256 combat buckets/named counters/equipped sources, 4096 combined dedup identities, 512 символов на detail, 2 MiB config snapshot, 8 MiB JSON. Constructor валидирует limits. При overflow новые timeline/key/identity отклоняются с dropped counter, dedup IDs не вытесняются; отчёт неполон. Byte overflow показывает Export error.

Combat aggregate update — main-thread O(1), без I/O и per-hit strings. Snapshot O(bounded data) под PerfGuard, вне damage callback. Worker получает immutable строки, публикует JSON последним через pending file, сохраняет feedback. Live/final exports упорядочены, чтобы live packet не затёр final. Serialization на main thread может дать разовый spike; PerfGuard — warning, не FPS guarantee.

## Ручная проверка

1. Начать Gameplay, поиграть движением и поставить marker с фактическим наблюдением.
2. Сделать паузу, продолжить; закончить проигрышем или остановить Play Mode.
3. Дождаться Saved либо проверить каталог после остановки; сопоставить report/run IDs в трёх файлах и marker time.
4. Записать observed vs expected и причину окончания в feedback, сообщить путь к пакету. Synthetic smoke не считается ручным прогоном.
