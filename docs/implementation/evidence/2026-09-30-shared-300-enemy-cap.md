# Единый технический предел 300 обычных врагов — 2026-09-30

Решение: [DECISION-0115](../../decisions/0115-shared-300-enemy-cap.md). Scope: IP-14/IP-24, production FIELD-001/002/003 и fixture timelines.

## Изменение

- `maxAliveEnemies` повышен до 300 в authoring packets всех трёх полей; три production JSON заново получены `scripts/content/generate.py`.
- `scripts/content/sources.py` отклоняет несовпадение лимита FIELD-002/003 с FIELD-001. Fixture timelines и catalog assertions синхронизированы.
- Нагрузочный EditMode-тест спавнера теперь создаёт 300 врагов в десяти циклах, проверяет повторное использование пула и прежние пределы времени. Runtime спавнер уже читает cap из timeline, поэтому его алгоритм не менялся.

## Проверки

- `python scripts/content/generate.py --check` — UP TO DATE.
- Прямое чтение шести authoring/production timelines — все `maxAliveEnemies=300`.
- `python -X utf8 docs/balance/validate_field003_v1.py` и `validate_field_rhythm_v2.py` — PASS.
- `python scripts/check_project.py --scope code --platforms EditMode --filter '^Game\.(Enemy|Bootstrap)\.'` — Unity 6000.6.0f1 batch, Game.* **252/252 PASS**, 0 failed/skipped; `TestResults/checks/20260930T091057-568529Z/summary.json`.
- PlayMode без graphics — NOT RUN/INCOMPLETE: Unity упал в `RenderPipelineManager.DoRenderLoop_Internal` до XML, `TestResults/checks/20260930T091242-695524Z/PlayMode.log`.
- Первый повтор PlayMode с graphics выполнил **59/59 PASS**, но входные Assets изменились во время запуска другим рабочим процессом, поэтому runner не записал reusable PASS. XML: `TestResults/checks/20260930T091517-469507Z/PlayMode.xml`.
- Повтор на стабильных inputs: `python scripts/check_project.py --scope code --platforms PlayMode --filter '^Game\.Bootstrap\.' --graphics` — Unity 6000.6.0f1 batch, Game.* **59/59 PASS**, 0 failed/skipped; `TestResults/checks/20260930T092205-124948Z/summary.json`.
- `validate_field001_baseline.py` и `validate_field002_v1.py` сейчас падают на уже существующих в общей рабочей папке изменениях скорости ENEMY-001 и teleport boss; проверка самого cap у них не достигнута. Отдельное чтение cap и генератор прошли.

## Открыто

- Полный check остаётся зависимым от согласования параллельных balance-изменений, на которых падают два валидатора; измерить frame time в игре с 300 одновременно живыми ordinary. Тест спавна/пула не является гарантией FPS.
