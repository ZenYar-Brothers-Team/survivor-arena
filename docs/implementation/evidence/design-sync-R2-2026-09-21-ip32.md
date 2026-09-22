# design-sync-R2 — IP-32 evidence, 2026-09-21

## IP-32

Scope: [модуль](../modules/IP-32-manual-ai-balance.md); execution status и порядок — только [STATUS](../STATUS.md). Пользователь разрешил продолжить следующий пункт после IP-31. Реализация ограничена процессом review, документацией и synthetic упражнением; runtime/config/art не менялись.

## Поставка

[Checklist](../../playtests/CHECKLIST.md), [review/proposal template](../../playtests/REVIEW_TEMPLATE.md), обновлённый [BALANCE_WORKFLOW](../BALANCE_WORKFLOW.md), [реальный анализ](../../balance/balance-progression-2026-09-21.md) и [synthetic упражнение](../../playtests/exercises/IP32/README.md). Запись OBS-01 связана с review; исходная цитата сохранена.

## Acceptance и фактические проверки

| Требование | Проверка / evidence |
|---|---|
| Report/config → observation → hypothesis | Реальный review IP32-OBS01-r1, report 108ff5b3e8ed457e84704dcbfa25e0f8, marker 2 / sequence 10 / 17.8085632 s, OBS-01 |
| Разделение facts/estimates/uncertainty | Applied damage 108 / 29.9932442 = 3.60081088 HP/s; теоретический BOLT 4 / 0.9 = 4.44444444 HP/s отдельно; гипотеза presentation не объявлена диагнозом |
| Реальный review cycle | Insufficient-evidence / no-change; patch/approval не выдуманы; follow-up сценарий сформулирован, OBS открыт |
| Exact values/path/revision и selected approval | SYNTHETIC-r1, строка A /damageHp 10→12 HP, B /cooldownSeconds 2→1 s; simulated approval только A; applied copy оставляет B=2 |
| Pending/rejected/deferred не применяются | Восемь негативных случаев exercise: pending/reject/defer/no-change/insufficient-evidence/revision drift/baseline drift/unknown row; mutation отсутствует |
| Apply/rollback | Synthetic copy даёт estimate 5→6 HP/s, rollback восстанавливает baseline; rollback после новых правок отклоняется |
| Missing ≠ zero | Реальный setDetails остаётся unsupported; review сохраняет ограничения field/profile/RNG/coverage; pickup distance неизвестна |
| Нет ложного before/after сравнения | Один реальный fixture run; synthetic estimates не выдаются за ручной follow-up; условия следующего сценария перечислены |
| Сохранность игры | SHA256 всех Assets/Resources/Content JSON до/после exercise совпадают; script работает только с копиями объектов в памяти |

Выполнено: `python docs/playtests/exercises/IP32/verify.py`, exit 0. Полный наблюдённый вывод сохранён в [verification.txt](../../playtests/exercises/IP32/verification.txt). Пересчитаны 9 hashes source snapshots, damage/overkill/XP и DPS arithmetic. Synthetic fingerprints baseline/applied различны; rollback возвращает baseline. Это проверка протокола, не новый автоматический approval engine в игре.

Scoped consistency review: STATUS, IP-31/IP-32, BALANCE_WORKFLOW, PLAYTEST_REPORT, playtest README/templates и DECISION-0024 согласованы по ownership хранения/approval. GDD «Опыт и level-up» не описывает стрельбу XP; FIXTURE-FAN/BOLT остаются fixture IDs, не production Content cards. Не проводился аудит всего каталога, win-rate, balance quality или actual визуальное воспроизведение. Проверены локальные ссылки новых/изменённых Markdown и diff whitespace.

Unity EditMode/PlayMode не запускались: исполняемые игровые файлы, сцены и config не изменены (WORKFLOW §9). Ранее выполненные 407/407 и 4/4 относятся к IP-31 и не заявляются как новый прогон IP-32. Real apply/follow-up не выполнялись; учебное разрешение synthetic не является пользовательским approval на tuning.

## Documentation impact

BALANCE_WORKFLOW, playtest checklist/review/OBS-01, IP-32 specification и STATUS/readiness. GDD/CD и gameplay semantics не менялись; новой архитектурной DECISION не требуется. Принятое хранение Git следует DECISION-0024. Packages/manifest.json — прежнее пользовательское изменение, не включённое в scope. Следующий IP автоматически не запускается.
