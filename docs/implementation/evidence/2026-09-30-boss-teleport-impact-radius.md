# Радиус удара прыжка финальных боссов — 2026-09-30

[DECISION-0128](../../decisions/0128-boss-teleport-impact-radius.md) уменьшает
`impactRadius` BOSS-001…010 с 3.5 до 2.8 world units. Все остальные параметры
телепорта сохранены. Production JSON пересобран из утверждённых balance inputs.

`python scripts/content/generate.py --check` — PASS. Целевой Unity EditMode
прогон не выполнен: открытый Editor отказал в соединении test runner
(`WinError 10061`).
