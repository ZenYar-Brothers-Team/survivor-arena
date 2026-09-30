# Превью эффекта замедления и скорость 0.5× — evidence, 2026-09-30

Решение: [DECISION-0108](../../decisions/0108-slow-status-look-preview.md).

## Реализация

- Состояние: `CombatControlState.IsSlowed`, `SlowRemaining01` (доля оставшегося времени самого долгого
  действующего источника; пауза не расходует).
- Вид: `SlowStatusPresentationRuntime` на VisualRoot врага — полоска (`SlowBar`, дочерняя VisualRoot,
  под точкой контакта), посинение (`SpritePresentationRuntime.SetStatusTint`, компонуется со вспышкой
  попадания), лёд (`SlowIce`, силуэт поверх тела) и обводка (8 силуэтов `SlowOutline*` за телом),
  шейдер `Resources/Shaders/SpriteSolidColor.shader`. Геймплейный root, коллайдер и движение не меняются.
  `Shutdown` компоновщика сбрасывает оттенок; оверлеи гасятся, когда замедление кончилось, враг умер
  или выбран «Нет».
- Управление: `SlowStatusPresentationDirector` на корне композиции раз в кадр обходит живых врагов
  (`PerfGuard` «Enemy.SlowStatusPresentation», 2 ms); выбор сохраняется между забегами сессии.
  DEV → Presentation: шесть кнопок, «Замедлить всех врагов» (40 % на 5 с из
  `FixtureSlowStatusPresentation.json`).
- Скорость: `RunModel.SpeedMultiplier` — `float`, допустимы 0.5/1/2/3/5; DEV-кнопка «0.5×». Автоматизация
  и валидатор демонстраций по-прежнему принимают только 1/2/3/5; запись демонстраций пишет целое число.

## Проверки

2026-09-30, Unity 6000.6.0f1, `python scripts/check_project.py --scope full --graphics`, batch:
**1081/1081 Game.* EditMode, 59/59 PlayMode, 0 failed/skipped**; generation, audio (28) и
art provenance (269) PASS. Результаты: `TestResults/checks/20260930T063137-594511Z/summary.json`.

Новые тесты: `SlowStatusPresentationTests` (профиль, отказ по имени поля, каждый вариант, сброс),
`CombatControlTests.SlowRemaining01_TracksLongestSourcePausesAndResets`, `SlowStatusPresenterTests`,
`SlowStatusSmokeTests` (production-враги: «Все» → видно, «Нет» → снято, замедление не меняется),
`RunModelTests` (0.5×), `GameplayUiAssetTests` (кнопки), `GameplayUiPresenterTests` (0.5× в HUD).

## Не проверено

Внешний вид вариантов — на выбор пользователя; выбор итогового варианта не сделан.
