# Награда только за полученные уровни — 2026-09-29

По прямому замечанию пользователя начальный L1 больше не приносит 5 монет.
`ProfileService.ApplyAsync` сохраняет level reward как `rewardPerLevel × (L−1)`;
Book reward остаётся отдельным. Коэффициент production MetaEconomy 5 не менялся.
Старые сохранённые receipts возвращаются по RunId без пересчёта.

`MetaProfileTests.LevelReward_ExcludesStartingLevel_PreservesBookGold` проверяет
L1=0, L2=5, L1+Book=20, L3+Book=30; terminal reasons на L1 и duplicate/reload
покрываются соседними тестами. `MetaProgressionSmokeTests` проверяет формулу
в полном result→purchase→retry цикле, `RunResultsTests` — отображаемую сумму.

Полный safe graphics run после правки: Unity 6000.6.0f1,
`TestResults/checks/20260929T110557-960511Z/summary.json` — 969/969 EditMode,
39/39 PlayMode, 0 failed/skipped; generated content current, audio 28 PASS,
art 269 PASS. Первый прогон обнаружил устаревшее ожидание в Results-тесте,
оно исправлено. Попытка headless PlayMode завершилась сбоем Unity в offscreen
renderer без результатов; она не считается проверкой. Финальный graphics run
прошёл полностью.

Синхронизированы GDD, Content Design, UI/UX, DECISION-0037/0090/0098,
IP-25/26 и regression map. Изменение игрового баланса ограничено наградой
за стартовый уровень; условий открытия/сохранения и других цен не меняли.
