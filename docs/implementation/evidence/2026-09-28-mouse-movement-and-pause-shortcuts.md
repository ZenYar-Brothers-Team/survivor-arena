# Mouse movement и pause shortcuts — 2026-09-28

## Реализация

- `Mouse movement` добавлен в app-scoped Settings, default Off; schema v2 сохраняет
  preference, а schema v1 загружается с Off без потери audio/shake/video.
- `PlayerMover` читает keyboard input по умолчанию. При включённой настройке он
  переводит screen pointer в world point и подаёт нормализованное направление;
  круг радиусом 1.0 world unit возвращает zero input.
- `Escape`, `Space` и right mouse проходят через единый manual-pause shortcut.
  Он работает только для Running/manual pause, не снимает draft/system pause и
  игнорируется при открытых Settings.
- Settings UI содержит toggle и показывает актуальные keyboard bindings вместе с
  тремя shortcuts паузы.

## Проверки

- Targeted EditMode: `126/126 Game.*`, 0 skipped — Settings persistence/migration,
  UI intent/assets, movement direction/deadzone и затронутые Bootstrap tests.
  Evidence: `TestResults/checks/20260928T101026-780699Z/summary.json`.
- Targeted PlayMode: `1/1 Game.*`, 0 skipped — composed mouse movement, deadzone и
  manual pause toggle. Evidence:
  `TestResults/checks/20260928T101624-580500Z/summary.json`.
- Full smoke после синхронизации кода и документов: `870/870 Game.* EditMode`,
  `31/31 Game.* PlayMode`, 0 skipped; content generation `UP TO DATE`, audio
  integrity `28 files / 15 cues`, art provenance `254` PASS. Evidence:
  `TestResults/checks/20260928T102150-570397Z/summary.json`.

Ручная проверка ощущения радиуса и поведения реальной мыши в standalone не
проводилась; automated PlayMode использует детерминированную позицию pointer.
