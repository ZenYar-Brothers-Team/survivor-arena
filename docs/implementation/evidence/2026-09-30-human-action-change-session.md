# 2026-09-30 — человеческая запись после action-change recorder

Плейтест выполнен по просьбе пользователя в отдельном worktree на
`develop-evg` `a568e9d33b82a7d42e509f1fb5aa4a9cc94096a6`. Первый запуск
ошибочно использовал старый player `e094afc` и был завершён как incomplete;
он не включён в этот dataset. В новом worktree сначала не развернулись Git LFS
sprites, поэтому первая сборка `a568e9d` не прошла bootstrap и не дала забегов.
После `git lfs checkout` (922 объекта) player собран повторно. Его manifest:
`TestResults/balance-build-human-a568e9d-lfs-20260930/build-manifest.json`,
data SHA256 `9244e057a2991b11209ae56c5c5f7cc304ca961b68d6a5636d0418ff5b7890f8`.
`dirty=true` отражает автоматически переписанные Unity settings в worktree.

Сеанс `human-20260930T161134Z-2959a45e` хранится локально в
`TestResults/demonstrations/`. Конфигурация: FIELD-001, CHAR-001, fresh
isolated profile, скорость 1×, `activeFirst15/v1`, звук выключен. Все пять
`demonstration.jsonl` прошли `validate_demonstration.py`: controller=human,
build provenance есть, 0 truncated и 0 incomplete observations. Четыре
завершённых поражением забега выбраны для обучения; пятый административно
остановлен общим 30-минутным бюджетом после 3,8 s и исключён.

| Run index | Run ID | Исход | Samples | SHA256 JSONL |
|---:|---|---|---:|---|
| 1 | `5908199922ae40678d301908abeddfa3` | Defeat, 250,37 s | 1 806 | `8986cc862ea9cbc9e2ae9cbabc6f5adfb8eb37841f40bc2c6fb3de22cd2ace4e` |
| 2 | `cdae89afa6b146cb9ab776dcb735c47a` | Defeat, 812,62 s | 5 652 | `4ef6189a72c8a52b0369e3b5aa2560eddb6fe72ee61cb213da996fe9e432afb9` |
| 3 | `eb28627ab3ea49028df4cee2a0f55add` | Defeat, 523,54 s | 3 858 | `ad020b08a2488fa18f03407b6a70b09ccf41b1cb76fcca52f37be048246ad17d` |
| 4 | `e30c05c7253b40d1a9ba6b3b60e2378a` | Defeat, 53,35 s | 371 | `db3296da9543154e8e77b4a30d282fcab4b388f29642c5c7282cf4de75a025d0` |
| 5 | `63b6c7fea9a14a909eabb3b6cbb903d6` | Aborted, experimentWallBudget | 28, excluded | `247d79818c5173a0f958eb5ce0df3607a3a8b166a072d9c3e8b70e05235756d8` |

Пользователь после сеанса: «Нет, проблем не заметил». Это отзыв о замеченных
проблемах, не приёмка баланса или всех визуальных изменений.

## Offline imitation candidate

`train_imitation.py` обучен на четырёх завершённых JSONL: 11 687 samples.
Каждый забег целиком удерживался для проверки; выходы локально в
`TestResults/imitation-human-a568e9d-20260930/`. SHA256 `model.json`:
`40e7e6b49a69381e544dcc0adcf3e989f9c2120248b3a1df0b81de81dbb569d4`.

| Held-out run | Accuracy MLP | Repeat previous | Смена направления MLP | Смен |
|---|---:|---:|---:|---:|
| 1 | 59,25% | 65,34% | 13,10% | 626 |
| 2 | 65,52% | 68,74% | 12,39% | 1 767 |
| 4 | 66,85% | 69,00% | 4,35% | 115 |
| 3 | 58,24% | 64,70% | 15,05% | 1 362 |

Модель распознаёт часть смен направления, но по общей точности уступает
повторению предыдущей команды на каждом held-out забеге. Она не выбрана игровой
policy. Closed-loop выживаемость, сбор XP и пригодность к балансным прогонам
не проверены. Ни production gameplay, ни content этим экспериментом не изменены.
