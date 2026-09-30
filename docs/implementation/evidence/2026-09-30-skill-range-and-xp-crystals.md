# Дальность умений и XP-кристаллы — 2026-09-30

## Изменения

- [DECISION-0117](../../decisions/0117-skill-reach-and-xp-crystal-scale.md):
  `effectRangeMultiplier` CHAR-001…010 умножен на 0.8 в двух утверждённых
  источниках баланса. Типовая база 1 → 0.8; CHAR-008 1.4 → 1.12;
  CHAR-010 1.25 → 1.0. Параметры каждого умения и прибавки уровней не
  переписаны; действующий канал дальности применяет новое значение ко всем
  шестнадцати active skills.
- XP `experienceVisualScale` production и fixture 0.62 → 0.527.
  Базовый `pickupRadius` CHAR-001…010 умножен на 0.85: типовой 0.5 → 0.425,
  CHAR-007 0.8 → 0.68, CHAR-009 0.4 → 0.34. `ExperienceDropRuntime`
  использует этот радиус и для проверки расстояния, и для триггера кристалла.
  Относительные бонусы PASSIVE-007 сохраняются.
- Production Characters, CharacterBaseline и Pickups регенерированы из
  authoring sources. Растровый ресурс XP не изменён; новый арт-пакет не нужен.

## Проверки

- `python scripts/content/generate.py --check`: UP TO DATE.
- `python docs/balance/validate_characters_v1.py`: PASS (9 поздних персонажей).
- `python docs/balance/validate_field001_baseline.py`: первый прогон выявил
  устаревшее ожидание скорости ENEMY-001 и старый stress count. Оба значения
  согласованы с DECISION-0099/0115; повторный прогон PASS.
- Первый `python scripts/check_project.py --scope full --graphics`:
  EditMode 1090/1090 PASS, PlayMode 58/59; один сбой
  `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`:
  ожидается 70 карточек вкладки «Открытия», найдено 61. Это UI-сценарий,
  который не использует изменённые поля дальности или XP; тот full verdict — FAIL.
  Результаты: `TestResults/checks/20260930T104843-353945Z/`.
- После согласования трёх параллельных наборов правок повторный full graphics
  PASS: Unity 6000.6.0f1, 1101/1101 EditMode + 59/59 PlayMode, 0 failed/skipped,
  generation/audio/art manifest 270/270 PASS
  (`TestResults/checks/20260930T115751-305090Z/summary.json`).
- `python scripts/validate-art-manifest.py`: 270/270 PASS.
- Отдельный `--scope art --graphics` не стартовал из-за занятого project
  runner lock; новых результатов Unity от этой попытки нет.

Игровое ощущение новой дальности и читаемость меньших кристаллов требуют
пользовательской визуальной оценки.
