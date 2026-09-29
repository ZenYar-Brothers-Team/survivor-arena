# Пробел и фокус игрового UI — 2026-09-29

В `PauseBuildPanel.ConsumePauseShortcut` проверка фокуса охватывала весь
`GameplayUi` root. Когда во время забега фокус получала видимая DEV-кнопка,
метод возвращал `true` и `GameplayCompositionRoot` не переключал manual pause.
Теперь `Space` перехватывается сфокусированным элементом только внутри
видимого экрана паузы. Логика всплывающей справки сета и одноразового
поглощения того же кадра сохранена.

`UiLayoutR2SmokeTests.GameplaySpace_WhenDevelopmentButtonHasFocus_IsNotConsumedByPausePanel`
воспроизводит случай с фокусом DEV и скрытым экраном паузы. Без ограничения
области фокуса тест должен вернуть `true`. Прежние UI-layout проверки
фокусированных элементов Pause остаются.

Unity PlayMode targeted **1/1 PASS**, 0 failed/skipped:
`TestResults/checks/20260929T203953-697273Z/summary.json`. Runner обнаружил
закрытый Editor и безопасно использовал batch. Существующий `GameplaySmokeTests`
также прошёл **3/3 PASS**:
`TestResults/checks/20260929T204224-027366Z/summary.json`.
Непосредственный игровой просмотр клавиши пользователем ещё открыт.
