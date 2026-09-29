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

Новая запись человеком и оценка её влияния на модель ещё не проводились.
