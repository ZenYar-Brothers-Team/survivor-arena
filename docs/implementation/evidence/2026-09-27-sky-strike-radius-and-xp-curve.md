# SKILL-010 radius и ранняя XP curve — 2026-09-27

## Основание и scope

- Источник: завершённый плейтест `091d834ec84d420ba40ae50066cc1c05`, [разбор](../../playtests/2026-09-27_091d834e.md).
- Решение: [DECISION-0079](../../decisions/0079-playtest-sky-strike-radius-and-xp-curve.md).
- Изменены только progression radius SKILL-010 и production XP thresholds. Damage, cooldown, число ударов, enemy XP rewards и количество dropped/collected XP не менялись.
- Наблюдение о лагах в районе 5-й и 10-й минут сохранено как отдельный открытый performance follow-up; этот balance batch его не закрывает.

## Реализация

- Source baseline: `docs/balance/field001-baseline-v1.json`.
- Generated runtime catalogs: `ProductionActiveSkills.json`, `ProductionRunSetup.json`.
- SKILL-010 radius L1–L6: `0.8/1.3/1.8/1.8/1.8/1.8`; L6 third-strike multiplier: `1.35`, итоговый radius `2.43`.
- XP formula и округление записаны в `docs/balance/field001-baseline-v1.md`; production-массив содержит 60 thresholds, после чего runtime повторяет последнее значение.
- First 10 thresholds: `10/11/14/14/16/17/18/19/20/21`.

## Арифметика

- Первые 10 переходов: `200 → 160 XP`, ровно `−20%`.
- Достижение L40, сумма L1–L39: `1257 → 1257 XP`.
- Достижение L37, сумма L1–L36: `1111 → 1094 XP`, `−1.5%`.
- Последний явный L60 threshold: `74 → 85 XP`. При продолжении до L100 runtime повторяет это значение, поэтому сверхпоздняя cumulative curve становится выше прежней; это не влияет на равенство к целевой точке L40.

## Проверки

- `python scripts/content/generate.py --check` — PASS, generated outputs актуальны.
- `python docs/balance/validate_field001_baseline.py` — PASS.
- Safe full smoke через repository batch runner — PASS, Unity `6000.6.0f1`:
  - EditMode `870/870`, skipped `0`;
  - PlayMode `30/30`, skipped `0`;
  - third-party tests `0`;
  - art manifest/provenance `254/254` PASS;
  - audio integrity `28` files / `15` cues PASS.
- Summary: `TestResults/checks/20260927T204517-509733Z/summary.json`.

## Открытая ручная проверка

В сопоставимом забеге проверить время достижения L5/L10, итоговый уровень, долю и equipped DPS SKILL-010, а также ощущение его радиуса. Лаги около 5-й/10-й минут проверять отдельно с frame-time/Profiler capture: исходный отчёт FPS и p95 не содержит.
