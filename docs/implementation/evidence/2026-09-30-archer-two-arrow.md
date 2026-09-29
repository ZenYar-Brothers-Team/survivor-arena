# 2026-09-30 — ENEMY-005: две стрелы

После человеческого прогона пользователь попросил уменьшить залп текущего
лучника первого поля с трёх стрел до двух и уточнил, что изменение должно
действовать на всех полях. [DECISION-0106](../../decisions/0106-archer-two-arrow-volley.md).

Источник `docs/balance/field001-baseline-v1.json` изменён только для
`ENEMY-005.attack.projectileCount`: 3 → 2. `ProductionEnemies.json` регенерирован;
прочие enemy IDs, урон стрелы, задержка, разброс и cooldown не менялись.
Карточка ENEMY-005 и baseline description синхронизированы.

Проверки: `python scripts/content/generate.py --check` — UP TO DATE;
`scripts/check_project.py --scope content` — STATIC PASS; Unity 6000.6.0f1,
targeted `ProductionEnemyCatalogTests` + `ProductionFieldContentTests` —
14/14 EditMode PASS, 0 failed/skipped,
`TestResults/checks/20260929T221610-314970Z/summary.json`. Тест проверяет
две последовательные стрелы и отсутствие третьей в этом залпе.

Запись `human-20260929T214730Z-5f0be49e` была сделана до этой правки и
остаётся помеченной сборкой с тремя стрелами; её нельзя выдавать за проверку
нового баланса. Ручной повтор после новой сборки открыт.
