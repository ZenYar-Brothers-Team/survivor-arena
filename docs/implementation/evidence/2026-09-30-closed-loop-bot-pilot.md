# 2026-09-30 — замкнутая проба автономного бота

После вопроса пользователя о ненаблюдаемой «цели в голове» проверен другой
сигнал обучения: результат самого забега. В этой пробе изменялся один параметр
существующей `trajectorySearch/v1` — штраф за прогнозируемый контакт с врагом
(`contactPenalty`). Игровые правила, content, награды и Unity runtime не менялись.
Это поиск параметра по исходу игры, а не восстановление намерения игрока из
записи клавиш.

## Условия и воспроизводимость

- Изолированный worktree закреплён на `develop-evg` commit
  `4eac2a4cf2815d69c4d2d7a22917bd934acbce83`. Сборка Unity 6000.6.0f1:
  `TestResults/balance-build-closedloop-4eac2a4-isolated-r2/build-manifest.json`,
  Development + `BALANCE_AUTOMATION`, executable SHA-256
  `718444f6a718fa668da96eb0a58445b009673310e6129152a63dbf7a92b8de00`,
  data SHA-256
  `89c249fe6acd3df76340e965076a9ea455d1dfdd2670acfa2af7741d5cd04938`.
  Manifest помечен `dirty: true`: Unity изменил три importer `.meta`, два URP
  settings asset и package settings при импорте. C# и production content в
  worktree не менялись.
- Все серии: fresh profile, CHAR-001 / FIELD-001, три независимые цепочки по
  одному забегу, 5× игровая скорость, `randomLegal/v1` draft,
  `cheapestPersonalUpgrade/v1` purchase, без изображения и звука. Сиды
  генерируются отдельно для каждого забега; это не парное сравнение по сидам.
- Конфиги: `TestResults/closedloop-20260930/configs/trajectory-contact-{20,60,120}-4eac2a4.json`.
  Они скопированы из `scripts/balance/examples/fresh-trajectory-search.json`;
  изменены только `experimentId`, `outputDirectory` и `contactPenalty`.
  Бюджеты: 1200 s на серию, 720 s wall timeout на забег.
- Локальные результаты: `TestResults/closedloop-20260930/isolated-runs/`.
  Для каждого варианта `manifest.json`, `runs.csv`, `analysis.json` и
  per-run `run.json` / `automation.json`. Все три анализа выполнены
  `python scripts/balance/analyze.py <experiment-directory>`.

## Результаты подбора параметра

| contactPenalty | Исход | Время до поражения, s | Среднее / медиана, s | XP за забег | Терминальный уровень |
|---:|---|---|---:|---|---|
| 20 | 3/3 Defeat | 75.31 / 68.30 / 127.81 | 90.47 / 75.31 | 9 / 13 / 5 | 1 / 2 / 1 |
| 60 (пример) | 3/3 Defeat | 129.38 / 76.11 / 113.51 | 106.33 / 113.51 | 6 / 11 / 13 | 1 / 2 / 2 |
| 120 | 3/3 Defeat | 82.28 / 214.26 / 113.52 | 136.69 / 113.52 | 7 / 4 / 13 | 1 / 1 / 2 |

Каждая серия завершилась со статусом `completed`, 3/3 natural completions,
нулём неполных забегов и нулём failed chains. Во всех девяти `run.json`
отсутствует `incompleteReason`, dropped timeline/aggregate/identity = 0;
все показывают 100 полученного урона и natural Defeat. Длинный забег 120
достиг 214 s, но собрал лишь 4 XP и остался на уровне 1. Средний срок жизни
этого варианта вырос из-за одного забега; медиана почти совпала с примером 60.
По девяти непарным забегам нельзя выбрать улучшенную policy или оценить её
win rate для балансировки.

## Контрольный существующий профиль

`herdLoopAdaptive/v1` запущен на том же player и общих условиях из
`scripts/balance/examples/fresh-herd-loop-adaptive.json`. В локальном
`TestResults/closedloop-20260930/configs/adaptive-herd-4eac2a4.json`
поменялись только имя опыта, output directory и общий wall budget серии
с 900 на 1200 s. Конфиг прошёл `run.py --validate-only`; результат —
`TestResults/closedloop-20260930/isolated-runs/adaptive-herd-4eac2a4/`.

| Chain | Исход | Время, s | XP | Уровень | Убийства |
|---|---|---:|---:|---:|---:|
| 0001 | Defeat | 108.47 | 8 | 1 | 22 |
| 0002 | Defeat | 454.65 | 15 | 2 | 81 |
| 0003 | Defeat | 562.13 | 26 | 3 | 69 |

Контроль также дал 3/3 natural defeats, ноль неполных runs/failed chains,
нулевые dropped counters и пустой `incompleteReason`. Среднее время —
375.08 s, медиана — 454.65 s. Два более долгих забега и больший счёт убийств
делают этот профиль полезной отправной точкой для следующей итерации, но
три непарных рандомизированных запуска не устанавливают его превосходство.
Даже 562 s закончились на уровне 3 с 26 XP, без завершения поля.

## Границы вывода

В `run.json` для этих player-запусков поле `provenance.commit` равно `unknown`;
commit и хеши подтверждаются отдельным build manifest. Это ограничение
атрибуции в runtime report, а не основание считать девять исходов сбоями.
Поддерживаются player combat, XP, draft и ordinary enemy combat; подробности
боссов, phase combat, FPS/p95 и часть meta/set атрибуции не записываются.
Прежняя [проверка планировщика](2026-09-29-ip34-trajectory-bot.md) уже
отмечала, что его модель не прогнозирует урон от оружия, убийства и появление
нового XP. Слабая прогрессия здесь согласуется с этим ограничением, но не
изолирует его как причину поражения.

Первая сборка из основного рабочего дерева захватила параллельную незавершённую
правку FIELD-004; запуск остановился на bootstrap из-за отсутствующего на том
снимке interior content и исключён из результатов. Первая попытка сборки
изолированного worktree встретила transient IL postprocessor error;
повторная incremental сборка дала указанный выше player. Эти технические
попытки не считаются игровыми забегами.

**Решение:** ни один вариант `trajectorySearch` не выбран новой policy;
изменение одного штрафа не устранило ранние поражения и слабую прогрессию.
`herdLoopAdaptive` остаётся существующим экспериментальным ориентиром, а не
валидным ботом для балансировки. Следующий технический вопрос — проверить,
как автономная policy превращает выживание в урон, убийства и сбор XP, прежде
чем расширять поиск параметров. Производственный баланс и правила игры не
менялись.
