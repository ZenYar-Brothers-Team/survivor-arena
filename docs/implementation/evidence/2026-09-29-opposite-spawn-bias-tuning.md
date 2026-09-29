# Усиление противоположного спавна — 2026-09-29

По отзывам пользователя пробный `spawnOppositeBias` FIELD-001/002/003 повышен
с 0.8 до 0.9, затем до максимума 1.0 в authoring balance JSON. Production
timelines перегенерированы. Радиус 10 и алгоритм выбора угла не менялись.

Для фиксированного направления центра массы численный расчёт формулы
`oppositeAngle + u − bias × sin(u)` даёт 39.65% появлений в противоположном
секторе 45° при 0.9 вместо 35.30% при 0.8; в противоположной четверти
кольца 90° — 53.48% вместо 50.46%. При 1.0 соответствующие доли составляют
43.71% и 56.22%. Остальные сектора сохраняют ненулевую вероятность.

Проверки: генерация `WROTE` три timelines; `python scripts/check_project.py
--scope content` — STATIC PASS, `UP TO DATE`; production field EditMode
**17/17 PASS** для 0.9, 0 failed/skipped,
`TestResults/checks/20260929T205021-108549Z/summary.json`. Повторно для 1.0:
генерация `WROTE` три timelines, content STATIC PASS / `UP TO DATE`, production
field EditMode **17/17 PASS**, 0 failed/skipped,
`TestResults/checks/20260929T205425-647073Z/summary.json`. Визуальное
поведение в игровом прогоне ещё не оценено.
