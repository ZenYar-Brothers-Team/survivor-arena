# F1-09 — исправления по плейтестам 2026-09-24

Дата: 2026-09-25. Scope: `field-001-start-R1`, F1-09 цикл «прогон → OBS → исправление →
повторная оценка». Решения и числа — [DECISION-0055](../../decisions/0055-playtest-2026-09-24-fixes.md).
Статус — только [STATUS](../STATUS.md#field001-execution).

## Отзывы

| Запись | OBS | Итог |
|---|---|---|
| [e1e04fc4](../../playtests/2026-09-24_e1e04fc4.md) | 01, 02 розовые эффекты | исправлено: material для code-created particles |
| | 03 небесный удар | исправлено: столб света над кругом |
| | 04, 05 взрывные сферы | диагностировано: дефекта очереди/коллайдера в коде нет; повторная проверка после исправления OBS-02 |
| | 06 снаряды врагов | исправлено: threat halo всем hostile projectiles + правило Art Production + тест |
| | 07 размер гончей | исправлено: ENEMY-007 body v002 вдвое меньше, коллайдер тот же |
| | 08 магнит опыта | исправлено: PASSIVE-007 до 2.5 units на L6 |
| | 09 лучник | исправлено: очередь 3 стрел ±6° |
| | 10 гончая | исправлено: рывок раз в 2.5 s |
| [9ae3826e](../../playtests/2026-09-24_9ae3826e.md) | 01 сеты в паузе | исправлено: владение по наличию + видимый список компонентов |
| | 02 сложность | отложено до следующего прогона |

## Проверки 2026-09-25

| Проверка | Результат |
|---|---|
| `python scripts/generate_field001_content.py --check` | UP TO DATE |
| `python scripts/validate-art-manifest.py` | PASS, 103 records |
| .NET harness compile (все asmdef) | 0 errors |
| .NET harness NUnit | 425/745 PASS против 421/741 на `develop-evg` (`db5868d`); регрессий 0; +4 новых теста PASS |
| Unity EditMode/PlayMode | **NOT RUN** — в облачной среде нет Unity; нужен `python scripts/check_project.py --scope full` |

Новые/изменённые тесты: `ProjectileLifecycleTests` (particle material — Unity-only),
`ProductionEnemyCatalogTests` (залп лучника, обязательный random, рывок 2.5 s),
`HostileProjectileReadabilityTests`, `GameplayUiPresenterTests.SetProgress_…`,
`ProductionPassiveCatalogTests` (радиус 0.7/2.5 — Unity-only).

Визуальный результат (цвет частиц, столб света, ореол, размер гончей) проверяется только в игре.
