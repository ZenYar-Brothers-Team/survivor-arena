# Невидимый периметр и стартовая частота FIELD-001 — 2026-09-29

По DECISION-0102 `FieldEnvironmentArtRuntime` больше не создаёт четыре
периметральных спрайта. Сцена скрывает placeholder-рендереры стен и оставляет
их player-only коллайдеры включёнными. Внутренние препятствия не менялись.
Неиспользуемое поле `fenceHeight` удалено из presentation schema и authoring
fixture; production presentation перегенерирован.

По DECISION-0103 FIELD-001 задаёт `openingIntensity` 30 s ×0.6.
`WaveDirector` масштабирует накопление времени только continuous таймера в
пределах этого окна и учитывает tick, пересекающий 30 s, без сброса прогресса.
FIELD-002/003 и burst-группы не меняются. Production timeline перегенерирован
из authoring balance JSON.

## Проверки

- `python scripts/content/generate.py`: обновлены presentation и FIELD-001 timeline.
- `python scripts/check_project.py --scope content`: STATIC PASS, generated content `UP TO DATE`.
- Прямой JSON-аудит: FIELD-001 содержит `30/0.6`, FIELD-002/003 не содержат
  `openingIntensity`, ни одно production presentation не содержит `fenceHeight` — PASS.
- Targeted EditMode запуск до изменения частоты: Game.* 107/107 PASS,
  `TestResults/checks/20260929T202458-808616Z/summary.json`; он **не** проверяет
  последующую правку частоты и новые тесты.
- Ранние попытки targeted EditMode в открытом Unity Editor были **NOT RUN**:
  объединённый regex не сопоставился UnitySkills, а локальный REST затем
  отказал в соединении. Позднее runner обнаружил закрытый Editor и выполнил
  финальный Enemy/Bootstrap EditMode **248/248 PASS**, 0 failed/skipped:
  `TestResults/checks/20260929T204103-387952Z/summary.json`.
- `GameplaySmokeTests` PlayMode **3/3 PASS**, 0 failed/skipped:
  `TestResults/checks/20260929T204224-027366Z/summary.json`.

Остаётся игровой просмотр невидимого края карты и интенсивности первых 30 секунд.
