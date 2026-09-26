# Поздние умения и пассивки: реализация (IP-17/IP-18)

Дата: 2026-09-26. Основание: [DECISION-0060](../../decisions/0060-late-skills-passives-data-v1.md),
данные [late-skills-passives-v1](../../balance/late-skills-passives-v1.md). Статус — только [STATUS](../STATUS.md).

## Что сделано

- `scripts/generate_field001_content.py` читает второй утверждённый пакет и генерирует
  SKILL-008/009/011/012/015/016 и PASSIVE-006/010/013/014 в production JSON. Существующие
  10 умений, 10 пассивок и прочие поля CHAR-001 семантически не изменились (сравнение с HEAD).
- Маппинг: диск — projectile + ricochet (repeat только без другой цели); мина — Mine, secondary с L6;
  спираль — Ring + поворот 15° за активацию, L6 вторая очередь; луч — Beam, tracking только L6;
  крест — Cross, unlimited pierce, L6 второй крест 22.5°; мусор — IndependentRandom с seed и linear stop.
- CHAR-001 получил веса всех 16 умений (SKILL-009 0.7). Draft пул по-прежнему фильтруется профилем:
  новый профиль открывает ровно стартовые 10+10, поздние ID — по DECISION-0050.
- Процедурный луч SKILL-012 (выбор пользователя 2026-09-26): `SkillWorldEffectKind.Beam`,
  `ProceduralShapeSprites.Beam`, `SkillWorldEffectPresenter.BeamPulse` — на каждый tick урона
  свечение шириной полосы попадания и ядро толщиной профиля, затухание 0.24 s, пауза замораживает,
  terminal clear возвращает в пул. Профиль в `SkillWorldEffects.json`, запись в `Art/asset-manifest.json`.
- World art SKILL-009/011/015/016 не подключён: явный placeholder, список закреплён тестом
  (`AwaitingWorldArt`). Процедурные растровые кандидаты пользователь отклонил 2026-09-26; арт
  пользователь сгенерирует отдельно.

## Проверки

| Проверка | Результат |
|---|---|
| `python -X utf8 docs/balance/validate_late_skills_passives.py` | PASS; негативная проверка (изменённый урон карточки) — FAIL как ожидается |
| `python scripts/generate_field001_content.py --check` | UP TO DATE |
| `scripts/validate-art-manifest.py` | PASS, 104 records |
| Unity 6000.6.0f1, `check_project.py --scope full` | **EditMode 766/766, PlayMode 27/27, 0 skipped**; `TestResults/checks/20260926T075338-918913Z/summary.json` |

Первый полный прогон дал 764/766: два теста стартовой композиции предполагали ровно 10+10+5 и
«все production умения открыты с начала». Они переписаны на фактический контракт (35 build entries;
новый профиль открывает ровно стартовые ID) и прошли. Новые тесты: `ProductionLateSkillCatalogTests` (6),
`ProductionPassiveCatalogTests.LatePassives_…`/`Stubbornness_…`, `ProductionSkillPatternTests.BeamTick_…`.

## Не проверено

- Реальный прогон с поздними умениями: они закрыты до прохождения полей; в игре доступны только
  SKILL-008 и PASSIVE-013 после первого прохождения FIELD-001.
- Читаемость луча в игре и иконок PASSIVE-006/010/013/014 в draft/build slots.

## Dev-панель (поручение пользователя 2026-09-26)

Чтобы проверять закрытые поздние ID без прохождения полей, в development-панель (только Editor /
Development Build, DECISION-0005) добавлены команды через обычный путь View → presenter → model:

- Run: `+100 XP` рядом с прежней `+5 XP` (intervention XP, как и прежняя кнопка).
- Build: `+100 rerolls` — только на текущий забег (`DraftRunControls.GrantRerolls`), reset возвращает исходное число.
- Build: `Unlock all skills/passives/sets (run)` — добавляет все записи production-каталога в пул выбора текущего
  забега (`DraftPool.AddDefinitions`); профиль и сохранение не меняются, рецепты сетов по-прежнему нужны.

Проверки: `GameplayUiPresenterTests.DevelopmentGrants_UseTheirAmounts_AndAreIgnoredOutsideDevelopment`,
`LevelUpDraftRuntimeTests.DevelopmentCommands_GrantRerolls_AndAddLockedEntriesOnce`; Unity full PASS
**768/768 EditMode, 27/27 PlayMode**, `TestResults/checks/20260926T081803-613666Z/summary.json`.
