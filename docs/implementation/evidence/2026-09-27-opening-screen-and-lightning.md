# Стартовый экран FIELD-001 и ранний урон SKILL-007 — 2026-09-27

Основание: два прямых отзыва пользователя 2026-09-27. Правила зафиксированы в [DECISION-0070](../../decisions/0070-field001-opening-screen-obstacles.md) и [DECISION-0071](../../decisions/0071-early-chain-lightning-damage.md).

## Изменения

- FIELD-001 `startScreen`: до общей генерации резервируются два обычных слота в противоположных центральных ячейках. Пень или бочка выбирается по сиду; объекты целиком помещаются в стартовом кадре и находятся дальше свободного круга радиуса 6. Остальная раскладка случайна; всего 288 препятствий.
- SKILL-007: damage L1/L2/L3 = 22/22/27.5 вместо 24/24/30. L4–L6 и другие параметры молнии сохранены.
- Источники `docs/balance/field-layouts-v1.json` и `field001-baseline-v1.json`, production JSON, генератор, Content Design и IP context согласованы.

## Проверки

- `python -X utf8 docs/balance/validate_field_layouts_v1.py`: PASS, 200 сидов каждого из трёх полей; FIELD-001 ровно 288 препятствий, два резервных объекта, проходы ≥4.
- `python -X utf8 docs/balance/validate_field001_baseline.py`: PASS; исторические 64 authored obstacles в baseline остаются review data, runtime использует layout.
- `python scripts/generate_field001_content.py --check`: UP TO DATE.
- `python scripts/check_project.py --scope full`: **856/856 EditMode, 30/30 PlayMode PASS**, art provenance 151/151 PASS, `TestResults/checks/20260927T102450-043722Z/summary.json`. EditMode проверяет стартовую пару на 100 сидах и числовые уровни SKILL-007; PlayMode проверяет минимум два полностью видимых collider через игровую камеру.
- Автоматические PlayMode-прогоны выполнены в беззвучном batch-режиме.

Субъективный баланс молнии и удобство движения между стартовыми объектами остаются для пользовательского прогона.
