# Камера у границы поля — 2026-09-29

По DECISION-0101 камера получает сторону поля от `GameplayCompositionRoot` при
инициализации забега. `CameraFollowTarget` ограничивает центр с учётом текущего
размера ортографической камеры и её aspect. Зависимость `Game.Movement` от
`Game.Field` не добавлена: конфигурацию передаёт уже существующий composition
root. Screen Shake ограничивает временную позицию в render callback и сразу
возвращает базовую позицию камеры. Физические стены игрока не менялись.

## Проверки

- Первый запуск тестов не стартовал из-за обнаруженной Unity циклической
  зависимости при прямом обращении `Game.Movement` к `Game.Field`. Связь
  устранена передачей размера поля из `GameplayCompositionRoot`.
- `python scripts/check_project.py --scope code --platforms EditMode --filter '^Game\.(Movement|Bootstrap)\.Tests\.'`: Game.* **53/53 PASS**, 0 failed/skipped, Unity batch после финальной правки; `TestResults/checks/20260929T145606-863327Z/summary.json`.
- `python scripts/check_project.py --scope code --platforms PlayMode --filter '^Game\.Bootstrap\.PlayModeTests\.'`: **NOT RUN / INCOMPLETE** — Unity завершился в `RenderPipelineManager.DoRenderLoop_Internal` до result XML; `TestResults/checks/20260929T145405-032686Z/PlayMode.log`. Это не считается проверкой PlayMode.

Визуальный результат у четырёх границ требует просмотра в игре.
