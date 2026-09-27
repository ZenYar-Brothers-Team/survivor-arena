# Плейтест 2026-09-27 — сеты, прыжок босса, пропсы полей, ослабление умений

Решение: [DECISION-0073](../../decisions/0073-playtest-2026-09-27-sets-boss-fields-balance.md).

## Что сделано

- Список сетов в Pause Build/draft/dev показывает только сеты draft pool забега; упущенные сеты
  (рецепт уже не выполнить: слоты, banish, недоступный компонент) идут последними в разделе MISSED SETS.
- `farDistance` телепорта BOSS-001…010: 5 → 6.25 units.
- FIELD-002/003: пропсы из пакета `field-obstacles-2026-09-27` выбираются как варианты каждого куска
  паттерна; каждый вариант гарантированно появляется в забеге; геометрия для сида не меняется.
- Урон 16 production-умений умножен на коэффициенты 0.45…0.95 (медиана 0.7) по числовой модели.

## Проверки

- Generator `--check` UP TO DATE; валидаторы field001 baseline, late skills/passives, sets, bosses,
  enemies, field002, field003, field layouts (200 сидов) — PASS.
- `python scripts/check_project.py --scope full` 2026-09-27: EditMode **866/866**, PlayMode **30/30**,
  0 failed/skipped; audio integrity 28/28; provenance 254 records PASS.
  Receipt: `TestResults/checks/20260927T162314-403128Z/summary.json`.
- `ProductionField003SmokeTests` теперь фиксирует HP игрока на 12 s smoke: стоящий на месте игрок с
  ослабленным стартовым умением мог погибнуть, а тест проверяет раскладку и пул врагов.

## Остаётся

Ощущение сложности FIELD-001…003 после ослабления, вид раздела упущенных сетов и частота пропсов
проверяет следующий ручной прогон пользователя.
