# 2026-09-30 — cap replacement и human draft

## Основание и объём

По поручению пользователя заполненный лимит обычных врагов больше не останавливает
плановый спавн. Перед очередным появлением удаляется самый дальний от игрока
обычный враг. Boss, mid-boss, Traveler и призванные враги принадлежат другим
owners и не участвуют в regular cap. Удаление не вызывает lifecycle/death/reward
callbacks и не начисляет kills. Каноническое правило: [DECISION-0105](../../decisions/0105-continuous-cap-replacement.md).

Для человеческой записи выбран `activeFirst15/v1`: на уровнях 1…15 приоритет
предложенных активных умений, затем обычный равномерный выбор. Запись остаётся
двигательной демонстрацией; пользовательские build decisions не записываются.
Development HUD может предлагать скорость выше 1×, но human recorder немедленно
возвращает 1×, сохраняя запись.

## Предыдущая попытка записи

Сеанс `TestResults/demonstrations/human-20260929T211640Z-d988f684` завершился
`runFailed:humanSpeedChanged`, 1 incomplete run, 0 пригодных human samples для
обучения. Его `.partial` не включать в dataset. Первая sandboxed попытка
`human-20260929T211514Z-3bb73697` завершилась отказом доступа до забега.
Оба сеанса предшествуют этим изменениям.

## Проверки

- Затронутые EditMode: `TestResults/checks/20260929T213433-732604Z/summary.json`,
  44/44 Game.* PASS, 0 failed/skipped.
- Первый полный прогон: 1051/1051 EditMode и 57/57 PlayMode, 0 failed/skipped,
  generation/audio/art 269 PASS. Общий verdict **INCOMPLETE**, так как исходные
  файлы изменились во время проверки; он не используется как итоговый PASS.
- Итоговый полный graphics PASS: `TestResults/checks/20260929T214021-644810Z/summary.json`,
  Unity 6000.6.0f1, 1051/1051 EditMode и 57/57 PlayMode, 0 failed/skipped;
  generation/audio/art 269 PASS. Проверка выполнена после стабилизации исходников.
- Balance Python suite: 25/25 PASS (`python -m unittest discover -s scripts/balance -p 'test_*.py'`).
- Ручной gameplay review плотности, видимости исчезновений и нового player:
  ожидается после сборки.

## Влияние на документацию

GDD, IP-14/IP-24/IP-34, DECISION-0076/0105, regression map, recorder README и
`STATUS.md` синхронизированы. Production content JSON и балансные числа не менялись.
