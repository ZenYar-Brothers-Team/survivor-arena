# Общая ранняя прогрессия active skills — 2026-09-27

Основание: прямой пользовательский отзыв, зафиксированный в
[DECISION-0078](../../decisions/0078-active-skill-early-progression.md).

## Реализация

- SKILL-001…016 приведены к общей кривой: компактный L1, заметный L2, возврат
  прежнего состояния L3; L4–L6 не изменены.
- SKILL-001 — отдельное исключение пользователя: L1 damage `14 → 9.33` (÷1.5),
  затем `14` на L2 и прежние `18.2` + два камня на L3.
- Основные ранние параметры остальных навыков: projectile/blade count,
  impact/blast radius, max targets, range/path либо duration. Damage не
  уменьшался там, где ослабление покрытия уже давало нужный эффект.
- SKILL-007/008 сохраняют по две цели/попадания на L1, чтобы chain/ricochet
  механика оставалась рабочей.
- Канонические карточки, FIELD-001 и late balance packets, production JSON,
  validators, IP context, DESIGN_SYNC, STATUS и catalog tests синхронизированы.

Полная таблица L1/L2/L3 находится в
[DECISION-0078](../../decisions/0078-active-skill-early-progression.md#новые-ранние-кривые).

## Проверки

- `validate_field001_baseline.py` — PASS: 60 skill + 60 passive levels,
  точные early curves startup-навыков, 900 s / 16 phases.
- `validate_late_skills_passives.py` — PASS: 6 skills × 6 levels и 4 passives ×
  6 levels; точные early curves поздних навыков.
- `scripts/content/generate.py --check` — UP TO DATE.
- Targeted EditMode production catalogs:
  `ProductionActiveSkillCatalogTests|ProductionLateSkillCatalogTests` —
  **16/16 PASS**, failed 0, skipped 0;
  receipt `TestResults/checks/20260927T193218-788128Z/summary.json`.
- Safe full smoke, Unity 6000.6.0f1, batch после обязательного preflight:
  - EditMode: **870/870**, failed 0, skipped 0;
  - PlayMode: **30/30**, failed 0, skipped 0;
  - third-party tests: 0;
  - audio integrity: **28 files / 15 cues PASS**;
  - art provenance: **254/254 PASS**;
  - receipt: `TestResults/checks/20260927T193313-833849Z/summary.json`.

## Открытая ручная проверка

Автоматика подтверждает точные данные, загрузку и regressions, но не ощущение
темпа. В следующем забеге нужно проверить читаемость и полезность каждого L1,
ценность L2/L3 и раннюю выживаемость при одновременном новом ритме волн.
Статус IP-17 остаётся Implemented: прежний ручной gameplay-scale visual gate и
новый balance-feel review открыты.
