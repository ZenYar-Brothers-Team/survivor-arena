# Проба спавна против центра массы — 2026-09-29

По поручению пользователя обычный спавн FIELD-001/002/003 смещён к направлению,
противоположному приблизительному центру массы живых обычных врагов. Настраиваемый
`spawnOppositeBias = 0.8` оставляет ненулевой шанс на всём кольце. На один tick
появления читается не более 16 позиций из существующего списка; на одного
врага выбирается один угол без поиска пути и повторных попыток.

Для тех же полей обычный радиус изменён с 12 на 10 world units. Стартовое
20-секундное появление у края экрана сохранено. Authoring balance JSON
перегенерированы в три production timeline; движение врагов не менялось.

## Проверки

- `python scripts/content/generate.py`: WROTE три production timeline.
- `python scripts/check_project.py --scope code --platforms EditMode --filter '^Game\.(Enemy|Bootstrap)\.Tests\.'`: Unity batch, Game.* **247/247 PASS**, 0 failed/skipped, third-party 0.
- `python scripts/check_project.py --scope content`: STATIC PASS, generated content `UP TO DATE`.
- Результат: `TestResults/checks/20260929T144233-239029Z/summary.json`.
- `git diff --check`: без ошибок.

Новый алгоритм и числа ещё ожидают игрового просмотра: эти проверки не измеряют
визуальную форму толпы или субъективное давление на игрока.
