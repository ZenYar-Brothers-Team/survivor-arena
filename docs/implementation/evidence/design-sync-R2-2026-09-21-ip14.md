# IP-14 — Wave Director evidence, 2026-09-21

## IP-14

Scope revision: design-sync-R2. Запрос пользователя «делай следующий пункт» разрешил
следующий Ready по Execution order — IP-14; IP-04/IP-13 имеют target-revision evidence.
Gameplay density review IP-12A этим пакетом не закрывается. Текущее состояние исполнения
и дальнейшая очередь находятся только в [STATUS](../STATUS.md#ip-14--wave-director-continuous-и-burst-timeline).

## Реализация и coverage

- `WavePhaseDefinition`/`WaveBurstDefinition`, DTO и catalog: explicit required mode/seed,
  count≥0, finite offset≥0/window>0, window inside phase, per-mode presence validation.
  JSON pressure fixtures: 18/26/34, 1-second windows. Ordinary/elite/rest сохранены;
  эти числа не утверждают production encounter balance.
- `WaveDirector`: one-shot uncapped burst, open/closed window edges, skipped groups expire,
  last hold не продлевает окно; continuous suppression не догоняется. Phase-transition
  delta не включает время старых фаз; hook ordering deterministic; restart = новый director.
  Composition и circle geometry имеют независимые seeded RNG streams.
- `ContinuousFixtureEnemySpawner`: actual return count и immutable outcome/event для
  optional observers; missing target фиксируется как unavailable без повторного burst.
  Ordinary list включает burst; чужие Boss/Traveler не занимают capacity.
- `GameplayUiRuntimeModel`: существующая bounded DEV Run drawer показывает mode/window,
  count/consumption/cap и последний requested/actual/suppressed/deferred/expired/unavailable.
  `SpawnResolved` обновляет immutable ViewState через прежний presenter; disposal снимает
  подписку. Phase HUD и semantic IDs переиспользованы.
- `WaveBurstTests`: 0/1/count, cap overflow, pause/terminal, first/last window boundary,
  multi-phase skip, last hold, no replay, same-time hooks, seed/geometry, schema negatives.
  `WaveDirectorTests`/`WaveDefinitionTests` сохраняют cadence/scaling/composition coverage.
- `WaveSpawnerTests`: actual=0 при missing target, boss/traveler isolation, repeated load,
  actual positions repeat per seed, pool identity и registry baseline после shutdown.
- `WaveBurstSmokeTests`: real Gameplay composition, полный ordinary cap 8 → burst 18
  (alive=26), pause/resume, HUD/DEV outcome и collapsed drawer, terminal no-spawn,
  Shutdown cleanup, новое selection/start и новый burst. Реальные boss implementations
  не требуются: category fixtures/hooks проверяют доступный integration boundary.

## Checks

Runner: `scripts/Test-Unity.ps1`, batchmode/nographics, Unity **6000.6.0f1**.
Перед каждым запуском свежая проверка `Win32_Process`: открытого Unity Editor нет.
Исходное пользовательское изменение `Packages/manifest.json` (testables UnitySkills)
сохранено; фильтр `^Game\.` исключает third-party tests из project evidence.

- Targeted wave run: **39/39**, failed=0, skipped=0.
- Полный EditMode: **496/496**, failed=0, skipped=0.
  `TestResults/IP14-EditMode.xml`, `TestResults/IP14-EditMode.log`.
- Первый PlayMode: 8/9; новый smoke ожидал immediate despawn при terminal. В существующем
  contract enemies freeze, cleanup выполняется при owner Shutdown. Исправлено только
  ожидание теста, production поведение не менялось. Исходный XML:
  `TestResults/IP14-PlayMode-initial.xml`. Повторный полный PlayMode: **9/9**, failed=0,
  skipped=0; `TestResults/IP14-PlayMode.xml`, `TestResults/IP14-PlayMode.log`.
- При первом targeted запуске компиляция теста остановилась на ненужном namespace
  `Game.Content.Json`; тест читает fixture TextAsset через Resources. Повторный targeted
  и полный EditMode компилируются и проходят.
- Markdown file links и `git diff --check` прошли. Third-party tests не запускались.
  Required critical paths представлены полным Game.* набором: run/damage/XP/draft,
  skills, waves/pool, composition, UI и content loading. После успешного EditMode
  менялось только ожидание PlayMode-теста и документация; production/config не менялись.

### Load bound

Порог явно принят пользователем в этой задаче: группа 100, 10 циклов, empty-pool cold
≤250 ms и pooled spawn ≤50 ms на группу. Время измерено Stopwatch вокруг `Tick`,
включая actual EnemyFactory/Initialize/registry; constructors/fixture loading и assertions
вне замера. Cold означает первое создание в пустом owned pool внутри Editor suite,
не холодный запуск ОС/Unity. Warm — максимум из 9 последующих циклов.

Hardware: **AMD Ryzen 5 5600H with Radeon Graphics, RAM 15724 MB**, Windows, Unity
6000.6.0f1, batchmode/nographics. В полном EditMode: cold **10.342 ms**, warm max
**1.376 ms**, unique objects **100**, 10×100 successful spawn; registry возвращается
к baseline после каждого Shutdown, total owned objects остаётся 100. Targeted run:
cold 32.559 ms, warm max 1.805 ms. Обе проверки ниже принятого порога.

PerfGuard warning 2 ms остаётся диагностическим: вывод о load bound сделан по
Stopwatch assertions, не по отсутствию throttled warnings. Это не frame/FPS, GPU,
длительный physics stress или combined 3–4-set gameplay density evidence.

## Documentation impact

IP-14 schema/fixture rationale, DECISION-0014 explicit runtime supplement, consumer
IP-15/IP-24 contracts и readiness. Game/Content Design, production schedules, art и
IP-12A image/density approvals не менялись. W-01 реализован по DECISION-0029 без
нового product rule. Общие catalog gates G-11/G-14 остаются у production packets.

По отдельному разрешению пользователя в `docs/regression-map.md` добавлены две записи:
actual count при missing target и continuous timer при перескоке фаз. Это документирование
уже прошедших тестов; повторный Unity run для этой правки не требуется.
