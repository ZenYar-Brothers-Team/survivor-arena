# Масштаб тел и сравнение снарядов — 2026-09-30

## Изменения

- [DECISION-0114](../../decisions/0114-body-scale-and-boomerang.md): PPU десяти
  playable body ×4/3, двадцати обычных enemy body ×1.25. PNG, pivot,
  portraits, bosses, mid-bosses и Travelers не изменены.
- `fit-body-contacts.py --fit-outer --write` пересчитал contact circles для
  затронутых body и их fixture aliases. Ground shadows используют новый
  radius автоматически. Версия import postprocessor повышена для реимпорта.
- Бумеранг: visualScale 1.4 → 1.6 при прежнем hit radius 0.16 (L1).
  Остальные снаряды и радиусы умений без изменений.
- [Картинка в общем игровом масштабе](../../art/reviews/2026-09-30-body-scale.png):
  Клёпка, Селянин, малый/крупный враги, Гром, mini-boss, boss и восемь
  снарядов. Это композиция существующих runtime PNG с текущими PPU и базовыми
  visualScale, а не захват динамического боя.

## Проверки

- `python scripts/fit-body-contacts.py` — все сохранённые круги внутри внешних
  силуэтов, без ошибок.
- `python scripts/content/generate.py --check` — UP TO DATE.
- Unity 6000.6.0f1, art graphics: **63/63 EditMode PASS**, manifest 269/269;
  `TestResults/checks/20260930T091843-679058Z/summary.json`.
- Unity 6000.6.0f1, full graphics: **1085/1085 EditMode + 59/59 PlayMode PASS**,
  0 failed/skipped; manifest 269/269, generation/audio PASS;
  `TestResults/checks/20260930T091916-526918Z/summary.json`.
- Unity создал обновлённые `.meta` с новым PPU для 30 body PNG; исходные PNG
  и их GUID сохранились. Один устаревший PPU assertion был обновлён перед
  успешным повторным art/full запуском.

Снимок и автоматические проверки не заменяют оценку анимации, плотной толпы
и удобства попаданий в реальном забеге.
