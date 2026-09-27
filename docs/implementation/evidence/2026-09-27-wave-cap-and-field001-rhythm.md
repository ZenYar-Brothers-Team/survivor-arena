# FIELD-001 wave cap и rhythm — 2026-09-27

Основание: прямой отзыв пользователя и [DECISION-0076](../../decisions/0076-wave-cap-and-field001-rhythm.md).

## Реализация

- `maxAliveEnemies=200` перенесён из каждой фазы на уровень timeline для fixture и
  production schedules FIELD-001/002/003. Domain/DTO/catalog, director, spawner и
  DEV observation используют один технический предел.
- Continuous и burst ordinary spawns соблюдают один предел. Burst запрашивается
  один раз, доступная часть появляется, подавленный остаток не переносится.
- FIELD-001 сокращён с 24 до 16 фаз. После стартовых 60/55/5 секунд основные
  combat-фазы длятся 70–90 секунд, четыре передышки — по 20 секунд, финальный
  burst — 5 секунд.
- В фазе FIELD-001 присутствуют 2–3 типа врагов, в двух поздних фазах — 4.
  Все ENEMY-001…005/007 остаются в расписании; boss hooks сохранены 450/810 s.
- Cadence выровнен так, чтобы изменение структуры не повышало общий поток:
  1736 номинальных ordinary requests против прежних ≈1735. Ожидаемый XP 2886.6
  против прежних ≈2883.

## Проверки

- `validate_field001_baseline.py`: PASS, 900 s / 16 phases, 1736 requests,
  expected ordinary XP 2886.6, technical cap 200.
- `validate_field002_v1.py`: PASS, 24 phases.
- `validate_field003_v1.py`: PASS, 24 phases.
- `scripts/content/generate.py --check`: UP TO DATE.
- Content scope: STATIC PASS.
- Targeted Unity 6000.6.0f1 EditMode: **293/293**, 0 failed, 0 skipped.
- Full safe check `TestResults/checks/20260927T182212-333820Z/summary.json`:
  **870/870 Game.* EditMode**, **30/30 PlayMode**, 0 failed, 0 skipped;
  audio integrity 28/28; provenance 254 records PASS.

Первый targeted запуск выявил конфликт имён локальных переменных до выполнения
тестов; следующий — одно устаревшее ожидание unit test. Первый full PlayMode run
выявил старое ожидание фазового cap=8 в smoke. Все три причины исправлены, итоговый
full run выше выполнен после исправлений.

## Открытая ручная проверка

Нужен плейтест полного FIELD-001: читаемость тематических составов, ощущение
70–90-second волн, достаточность 20-second передышек и отсутствие performance
просадок около технического предела. Автоматика не утверждает игровой баланс.
