# Объединение develop-evg-wt с develop-evg

Date: 2026-09-28
Execution status: [STATUS](../STATUS.md).

## Scope

По запросу пользователя «перенеси в эту ветку результаты из соседнего worktree»
объединены две линии истории: текущая `develop-evg` на `767212d` и соседняя
`develop-evg-wt` на `9e0c51f`, общий предок `40cab02`.

Входящие коммиты:

- `3d557ea`: CHAR-002…010, характеристики, веса активных/пассивных компонентов
  и блокировки драфта, production bindings, генератор и проверки.
- `9e0c51f`: TRAVELER-003/004/006…010, общий уровень прогрессии по ролям,
  production profiles и сохранение attack cadence/follow-ups/windup при Scale.

Незакоммиченный `ProjectSettings/Packages/com.unity.learn.iet-framework/Settings.json`
в исходном worktree не переносился и не изменялся. Worktree и его ветка сохранены.

## Resolution

- Единственный Git conflict — строка blockers IP-27 в STATUS. Сохранены обе
  дельты: IP-10A больше не блокирует после приёмки UI, IP-22/IP-30 больше не
  перечисляются как Blocked после входящей реализации. UI entry R1 и его
  выбранный арт не потеряны.
- В двух ветках номер DECISION-0087 обозначал разные решения. Решение о
  персонажах перенумеровано в [0089](../../decisions/0089-characters-v1.md),
  с обновлением ссылок, комментариев и regex статического validator.
  [0087](../../decisions/0087-character-silhouettes-and-field-grid.md) остаётся
  решением о силуэтах/полях, [0088](../../decisions/0088-travelers-v1.md) — о путниках.
  Approval и игровой смысл обоих входящих решений не менялись.
- Убраны trailing whitespace в заголовках входящих ADR, мешавшие preflight.
- Production JSON совпадают с исходной веткой; дополнительных gameplay-правок
  при merge не внесено. Старые target-evidence строки IP-22/IP-30 согласованы
  с перенесёнными результатами и свежей проверкой объединённой версии.

## Verification

На объединённом дереве, Unity 6000.6.0f1, безопасный batch runner с graphics:

- `python scripts/check_project.py --scope full --graphics` — PASS.
- EditMode: 921/921 passed, 0 failed/skipped; PlayMode: 34/34 passed,
  0 failed/skipped. Third-party tests: 0.
- Generation check: UP TO DATE. Audio: 28 files / 15 cues PASS.
- Art provenance: 254 owner/role records PASS.
- `validate_characters_v1.py`: 9 characters, 6–8 недоступных сетов,
  разные наборы блокировок — PASS.
- `validate_travelers_v1.py`: 7 новых путников, распределение ролей 4/3/3,
  HP/contact в пределах ±25% от role peers — PASS.

Результаты: `TestResults/checks/20260928T184120-318706Z/summary.json`,
`EditMode.xml`, `PlayMode.xml` и логи в той же папке. В passed cases представлены
run lifecycle, combat/health, experience/draft, active skills, waves/spawn/pool,
composition, UI, content loading и GameplaySmokeTests; новые catalog tests:
24 character cases и 13 traveler cases. Первый preflight остановился на пробелах
в ADR до запуска Unity; после исправления выполнен указанный полный прогон.

Это интеграционная проверка, не ручная приёмка новых персонажей/путников и не
основание объявить весь production scope Verified. Unity-перенос HTML-макета
в эту операцию не входит. Никакие новые IP автоматически не начинались.
