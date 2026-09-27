# Ранний рост снарядов SKILL-002/013 и скорость ENEMY-001 — 2026-09-27

Основание: прямой пользовательский отзыв, зафиксированный в
[DECISION-0077](../../decisions/0077-early-projectile-growth-and-enemy001-speed.md).

## Реализация

- SKILL-002: projectile counts L1–L6 изменены с `5/7/7/9/9/11` на
  `3/4/5/7/9/11`.
- SKILL-013: projectile counts L1–L6 изменены с `7/9/9/9/9/13` на
  `4/5/6/7/9/13`.
- Финальные counts, damage каждого снаряда, cooldown и остальные параметры
  обоих навыков сохранены.
- ENEMY-001 «Селянин с вилами»: movement speed `1.20 → 0.96`; это ровно −20%.
  HP, contact damage, collision, XP и Seek-поведение сохранены.
- SKILL-015 «Крест клинков» не изменён: после уточнения пользователя его
  временная незавершённая правка была полностью отменена до генерации данных.
- Канонические карточки, balance packets, generated production JSON, IP context,
  DESIGN_SYNC, STATUS и точные production tests синхронизированы.

## Автоматические проверки

- `python -X utf8 docs/balance/validate_field001_baseline.py` — PASS: 60 skill
  levels, 60 passive levels, 900 s / 16 phases; точные counts и speed входят в
  validator.
- `python -X utf8 docs/balance/validate_enemies_v1.py` — PASS: 14 поздних врагов
  остаются согласованы с карточками после обновления reference ENEMY-001.
- `python scripts/content/generate.py --check` — UP TO DATE.
- `git diff --check` — PASS; только предупреждения line-ending policy для уже
  затронутых файлов.
- Safe full smoke через `scripts/check_project.py --scope full`, Unity
  6000.6.0f1, batch после обязательного process/lock preflight:
  - EditMode: **870/870**, failed 0, skipped 0;
  - PlayMode: **30/30**, failed 0, skipped 0;
  - third-party tests: 0;
  - audio integrity: **28 files / 15 cues PASS**;
  - art provenance: **254/254 PASS**.

Итоговый receipt:
`TestResults/checks/20260927T191226-569482Z/summary.json`.

## Открытая ручная проверка

Автоматика подтверждает точные данные и runtime-загрузку, но не ощущение баланса.
В следующем прогоне нужно оценить силу SKILL-002/013 на L1–L4 и раннее давление
массовых ENEMY-001 при прежнем расписании волн. Статусы IP-17 (Implemented) и
IP-20 (Blocked только ручным gameplay-scale visual review поздних ID) не меняются.
