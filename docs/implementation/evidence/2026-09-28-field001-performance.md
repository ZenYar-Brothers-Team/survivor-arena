# FIELD-001 — пользовательская приёмка и performance benchmark, 2026-09-28

## Итог

- Пользователь подтвердил полный ручной прогон FIELD-001: поле работает хорошо, замечаний нет, результат принят.
- Первый точный stress-run выявил CPU bottleneck и не прошёл frame-time bounds.
- После профилирования и упрощения Путников повторный Windows standalone benchmark прошёл все три минутных окна, загрузку сцены/забега и аудит 10 перезапусков.
- F1-09 manual и performance gates закрыты.

## Среда и воспроизводимость

- Revision: `240e66df47e08e607951d10125e8c88dca42c37a`, dirty build (включает benchmark harness и проверяемую оптимизацию).
- Unity `6000.6.0f1`, WindowsPlayer Development, Direct3D11.
- AMD Ryzen 5 5600H with Radeon Graphics, 12 logical cores, 15,724 MB RAM.
- AMD Radeon Graphics, reported graphics memory 7,862 MB.
- `1920x1080`, Ultra, render scale `1.0`; Windows сообщил `29.97 Hz`.
- Runtime harness: `Assets/Game/Bootstrap/Diagnostics/Field001PerformanceBenchmark.cs`.
- Build/profile tools: `Assets/Game/Bootstrap/Editor/Field001PerformanceBuild.cs`, `Field001ProfileAnalyzer.cs`.
- Raw local outputs: `TestResults/performance/field001-report.json`, `field001-stress.raw`, profiler summary и player logs (ignored build artifacts).
- Standalone использует изолированный memory profile; production save не изменяется. Benchmark-код включается только в Editor или при define `FIELD001_PERFORMANCE_BENCHMARK`.

## Исходный FAIL и профилирование

Первый exact stress `250 ordinary + 2 bosses + 3 Travelers + 6 skills L6 + 4 sets`:

- frame p95 `61.590 ms`, p99 `488.577 ms`, max `534.432 ms`;
- CPU p95 `61.591 ms`, GPU p95 `2.189 ms` — bottleneck CPU-side;
- 15-second binary Unity Profiler capture: 850 main-thread frames, p95 `19.481 ms`, max `55.239 ms`;
- около `7,967.9 ms` суммарного sampled time находилось в дорогих `EnemyRuntime.FixedUpdate` вызовах при `8,481.2 ms` всего script fixed update; Physics2D заняла `1,284.6 ms`, render pipeline — около `502.2 ms`.

Подтверждённые причины:

1. Путник-защитник искал наиболее плотную группу сравнением всех ordinary enemies со всеми на каждом physics tick (`O(n²)`).
2. Traveler movement повторно выполнял полный player-reachability BFS при пересечении player-only obstacle, хотя контракт поля разрешает Путникам проходить эти препятствия.

## Изменение

[DECISION-0082](../../decisions/0082-simple-traveler-protector-targeting.md):

- раз в 10 секунд или после потери цели Путник рассматривает 4 ближайших ordinary enemies;
- выбирает кандидата с наиболее плотным окружением в `supportRadius`, при равенстве — ближайшего к игроку;
- стоимость редкого выбора `O(4n)`; между выборами используется сохранённая цель;
- движение игнорирует player-only obstacles и только clamp-ится границами арены;
- spawn использует bounded rejection по bounds + obstacle exclusion;
- удалены неиспользуемые `IPickupPlacement`, `TryPlaceFrom`, connected-component BFS, его большие кэши и reference implementation.

Density, support radius/effects, lifetime, rewards, `250 + 2 + 3` stress composition и balance values не снижались.

## Финальный benchmark

Budget: p95 `<=16.7 ms`, p99 `<=33.3 ms`, ни одного stall `>100 ms`; scene/run load `<=3 s`; managed growth после GC за 10 restart `<=5 MB`.

| Проверка | Наполнение | p95 frame | p99 frame | max frame | p95 CPU | p95 GPU | Итог |
|---|---:|---:|---:|---:|---:|---:|---|
| Scene + run load | — | — | — | `2308.76 ms` load | — | — | PASS |
| Minute 5 | 24 ordinary, 6 skills, 4 sets | `16.676 ms` | `16.747 ms` | `36.477 ms` | `16.689 ms` | `1.939 ms` | PASS |
| Minute 10 | 53 ordinary, 1 boss, 6 skills, 4 sets | `16.675 ms` | `16.715 ms` | `37.307 ms` | `16.690 ms` | `1.932 ms` | PASS |
| Final stress | 250 ordinary, 2 bosses, 3 Travelers, 6 skills, 4 sets | `16.673 ms` | `16.680 ms` | `37.367 ms` | `16.683 ms` | `2.262 ms` | **PASS** |

All three measured windows reported zero `GC.Alloc` recorder events.

## Restart audit

- 10/10 cycles returned enemy and projectile leased counts to `0` and `EnemyRegistry` to `0`.
- Capacity stayed stable: enemies `255`, projectiles `214`.
- Managed memory after forced GC: `15,564,800 -> 15,720,448` bytes, growth `155,648` bytes (`0.148 MB`), below the `5 MB` budget.
- Restart audit: PASS.

## Verification

- Targeted Pickup + Traveler EditMode after simplification: 61/61 PASS, `TestResults/checks/20260928T082257-019227Z/summary.json`.
- Final full project smoke after all source changes: 865/865 EditMode + 30/30 PlayMode, 0 failed/skipped; static generation/audio integrity and 254 provenance records PASS, `TestResults/checks/20260928T083714-133823Z/summary.json`.
- Benchmark warning stack traces were disabled so diagnostic formatting did not distort measured windows.
- Final standalone report: overall PASS.
