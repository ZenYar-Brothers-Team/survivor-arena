# Perf-фикс 2026-09-25: reachable placement и orbit area damage

Источник: живой плейтест 2026-09-25 (Editor.log пользователя) и статический `perf-audit`:
`Pickup.ReachablePlacement` 20–30 ms при пороге 5 ms (29 раз), `EnemyDamageArea.Apply` 20–31 ms при
пороге 2 ms (28 раз). Статус — только [STATUS](../STATUS.md#field001-execution).

## Изменения

1. **`BoxPickupPlacement.TryPlace`** — переработан без изменения API и результата. Сетка из границ,
   якоря и рёбер препятствий неизменна, поэтому в конструкторе один раз строятся её координаты,
   списки препятствий на каждую линию и флаги свободы узлов/рёбер. Вызов добавляет только линии через
   запрошенную точку, проверяет их узлы/рёбра и выполняет тот же BFS (тот же порядок соседей и
   tie-break) на переиспользуемых массивах; LINQ удалён и из `Free`.
2. **Orbit tick** (`SceneActiveSkillEffectExecutor.ExecuteOrbitTick`) — один `OverlapCircle` на всё
   кольцо (`EnemyDamageArea.ApplyCircles`), попадание по каждому клинку — по форме коллайдера
   (`Collider2D.ClosestPoint`), дедупликация и knockback — per blade, как раньше.
3. **Слой врагов** — не менялся: выделенного слоя нет, предложение — [DECISION-0056](../../decisions/0056-enemy-physics-layer.md) (Proposed).
4. **PerfGuard** — `SceneActiveSkillEffectExecutor.OrbitalBladeArea` (2 ms, как один area hit и Tick)
   и `PersistentOrbitState.BladeSweep` (2 ms) для production-орбиты SKILL-003.

## Измерения (.NET 8 harness, не Unity)

Один и тот же production-подобный набор (64 препятствия 1.2×1 / 2.4×0.5 в 200×200), запрос — центр
препятствия (полная проекция), 20 вызовов, прогретый JIT:

| | старый поиск (дословная копия) | новый |
|---|---|---|
| время на вызов | 24.5–33.9 ms | 1.5–1.7 ms |
| аллокации на вызов | много (списки, LINQ, массив, очередь) | 0 B |
| конструктор (один раз на поле) | — | 12–23 ms |

Старые цифры harness совпадают по порядку с Editor.log (20–30 ms). **Unity-время после фикса не
измерено**: нужен новый плейтест и отсутствие `[Perf] 'Pickup.ReachablePlacement'` в Editor.log.
Для orbit batching время не измерялось вовсе — число физических запросов снижено с BladeCount до 1
по коду; production SKILL-003 использует `PersistentOrbitState` (capsule sweep на клинок), который
этим изменением не батчится, только получил PerfGuard.

Гипотеза (не проверена): предупреждения `EnemyDamageArea.Apply` 20–31 ms в основном — вложенная
стоимость смерти врага внутри guard (drop placement через `TryPlace` 20–30 ms); на это указывают почти
равные счётчики (28 и 29) и диапазоны. Дополнительно запрос ловит trigger-коллайдеры XP радиусом до
2.5 units (DECISION-0056).

## Проверки

| Проверка | Результат |
|---|---|
| .NET harness compile | 0 errors |
| .NET harness NUnit | 431/753 PASS; регрессий 0 относительно `develop-evg` (`db5868d`); 12/12 `BoxPickupPlacementTests` PASS |
| Мутационная проверка оракула | порядок соседей — ловит `TryPlace_ExactEqualDistanceTie_…`; пропуск линии запроса — 415/416 расхождений; «узел на линии запроса всегда свободен» — эквивалентный мутант (узел внутри препятствия уже отсечён рёбрами) |
| `EnemyDamageAreaCirclesTests` | **NOT RUN** — нужен Unity physics |
| `smoke-check` (`check_project.py --scope full`) | **NOT RUN** — в облачной среде нет Unity (`msvcrt`/Unity недоступны) |
