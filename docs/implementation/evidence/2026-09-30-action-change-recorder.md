# 2026-09-30 — запись смен направления для обучения

Основание: [offline кандидат](2026-09-30-human-imitation-candidate.md) на трёх
человеческих забегах уступил повторению предыдущего действия. При старом
периодическом sampling фактическая смена клавиши часто приходилась между
снимками, поэтому в наборе мало точных пар «состояние перед сменой / новое
действие».

В `DemonstrationRecordingSession` оставлен периодический snapshot и добавлен
snapshot на каждом физическом шаге, когда clamped movement intent меняется.
`samplingPolicy=periodicOrActionChange/v1` в header и `captureReason` в sample
различают причины; время и `stepSeconds` по-прежнему относятся к одному
физическому шагу. Старые human JSONL без этих полей валидны. Новый validator
отклоняет неверную метку. Пример recorder теперь использует 0.2 s, 40 000
samples, 512 MiB; это пределы сохранения, а не целевой размер. Игровое движение,
контент, баланс и обычный профиль не меняются.

Проверки до сборки:

- Python `scripts/balance/test_*.py`: 29/29 PASS.
- Новый `DemonstrationRecordingTests.ActionChange_BetweenPeriodicSamples_RecordsExactPhysicsStep`:
  1/1 PlayMode PASS с графикой; фиксирует поворот между плановыми снимками.
- Полный safe smoke Unity 6000.6.0f1 с графикой: 1051/1051 EditMode,
  58/58 PlayMode, 0 failed/skipped; generation/audio/art PASS.
  `TestResults/checks/20260929T224523-713784Z/summary.json`.
- Первая scoped PlayMode попытка без графики завершилась Unity crash без XML;
  повтор с графикой выявил ошибку тестового завершения во время ручной паузы.
  После исправления теста точечный PlayMode и полный smoke прошли.

Отдельный Development player собран из `e094afc`:
`TestResults/balance-build-imitation-20260930/balance.exe`, manifest
`build-manifest.json`, Unity 6000.6.0f1, executable SHA256
`718444f6a718fa668da96eb0a58445b009673310e6129152a63dbf7a92b8de00`,
data SHA256 `885fce4bcadd4d7c5b28d12bc1f36bd11872d604e8e91470d2ec696cd0f75c3f`.
Manifest `dirty=true` из-за двух автоматически переписанных URP settings при
сборке; после сверки этих изменений оба файла возвращены к Git версии.
Сборка включает [DECISION-0106](../../decisions/0106-archer-two-arrow-volley.md).

Короткий изолированный bot-labelled pilot
`TestResults/pilot-imitation-recorder-20260930`, run
`66ef2dc3ec1340cf82d371ebffc8094c`: 1141 samples, из них 832 periodic,
267 actionChange и 42 periodicAndActionChange. Validator PASS, 0 truncated и
0 incomplete. Запуск штатно остановлен `runWallTimeout` после 174.6 simulation s;
это administrative abort и не считается поражением. Он проверяет файл, а не
качество человеческой записи или модели. Новая запись человеком и оценка её
влияния на модель ещё не проводились.
