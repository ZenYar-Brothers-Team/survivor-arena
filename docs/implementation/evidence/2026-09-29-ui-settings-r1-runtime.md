# Settings R1 — Unity, 2026-09-29

Основание: [DECISION-0093](../../decisions/0093-settings-folio-ui.md).
Статус и очередность — только [STATUS](../STATUS.md).

## Поставка

Принятая композиция перенесена в AppShell UXML и SettingsStyles; SettingsPanel
занимается presentation, AppShellPresenter — snapshots/intents. Звук слева,
изображение и управление справа, закреплённые шапка/footer, overflow центральной
области. Значения ползунков — целые проценты. Режим — dropdown, список разрешений
остаётся от IVideoDevice. Привязки движения показываются из фактического input,
поэтому список может быть длиннее условного WASD в HTML (включает device bindings).

Video confirmation использует текущий countdown SettingsService, затем возвращает
фокус к режиму. Background controls отключены при модальном окне; Tab переключает
его кнопки. Escape остаётся у существующего GameplayCompositionRoot, без второго
обработчика во View. Переход из Settings обратно в Pause сохраняет pause owners.
Новые настройки, реальные файлы пользователя и исходные defaults не менялись.
Новых растровых ассетов нет.

## Проверки

Safe runner (закрытый Editor → batch), Unity 6000.6.0f1, graphics:

- `python scripts/check_project.py --scope full --graphics`: 946/946 EditMode,
  39/39 PlayMode, 0 failed/skipped, third-party 0. Generated content актуален,
  audio integrity 28 файлов PASS, art audit 256 записей PASS.
  `TestResults/checks/20260929T062417-134197Z/summary.json` и XML/logs рядом.
- При просмотре captures обнаружено сжатие стандартных Dropdown/Toggle inputs.
  Добавлены min-width и geometry regression. После этих USS/test изменений:
  `python scripts/check_project.py --scope code --platforms PlayMode --filter '^Game\.Bootstrap\.PlayModeTests\.SettingsPresentationSmokeTests' --graphics`
  — 3/3 PASS, 0 failed/skipped; `TestResults/checks/20260929T062714-463807Z/summary.json`.
  Предыдущая попытка без anchored namespace отвергнута runner до запуска Unity.

Новые tests: viewstate Apply/countdown/Back/owner, 1080p/720p bounds без scroll,
проценты, ширина dropdown/toggle, confirmation, blocked Back и focus restoration.
Полный прогон включает существующие SettingsServiceTests (Keep/Revert/timeout,
invalid/save/fallback), audio/shake и AppShellSmokeTests (menu→Settings→run→pause→
Settings→quit→retry и сохранение двух pause owners), общие gameplay critical paths.

Снимки `TestResults/settings-r1-{1920x1080,1280x720}.png` и
`settings-r1-confirm-{1920x1080,1280x720}.png`; просмотрены финальный 720p и
confirmation 1080p. Ручные реальные video/audio на устройстве этим не заявляются:
тесты используют fake video и memory stores, пользовательский прогон остаётся отдельно.

Documentation impact: UI/UX §18, IP-26, decision/proposal, PROJECT_MAP,
regression-map и STATUS. Game/Content Design не менялись.

## Ручная приёмка

2026-09-29 пользователь подтвердил, что уже просмотрел экран в игре:
«там всё принимается». Settings R1 принят без дополнительных правок.
Повторный автоматический прогон при фиксации этой документальной приёмки не
выполнялся; последняя проверенная реализация и результаты указаны выше.
