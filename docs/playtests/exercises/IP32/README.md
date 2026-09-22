# IP-32 — Synthetic accept/apply/rollback exercise

Это учебная проверка протокола в памяти, не игровой report, не балансный target и не реальное разрешение пользователя. Исполняемый [verify.py](verify.py) не применяет изменения к Assets и не является production approval gate. Реальный процесс выполняет человек/AI по [checklist](../../CHECKLIST.md).

Запуск из корня проекта: `python docs/playtests/exercises/IP32/verify.py`. Только Python standard library, путь проекта вычисляется от файла. Скрипт пересчитывает арифметику настоящего report и отдельно моделирует следующие решения:

- Baseline: synthetic объект SYNTHETIC-TRAINING-ONLY, `/damageHp`=10 HP, `/cooldownSeconds`=2 s.
- Proposal SYNTHETIC-r1: строка A `/damageHp` 10→12 HP; строка B `/cooldownSeconds` 2→1 s. Это произвольные учебные значения; соответствующего content ID/JSON в игре нет.
- Смоделированное approval выбирает только A на точном baseline fingerprint и revision. Строка B не применяется. Fingerprint этого упражнения — SHA256 canonical Python JSON, не configHash телеметрии.
- Applied copy: 12 HP, 2 s. Estimate `damageHp / cooldownSeconds`: 10/2=5 → 12/2=6 HP/s, +20%; не observed DPS и не результат прогона.
- Pending/reject/defer/no-change/insufficient-evidence, чужой row, новая revision и изменённый baseline отклоняются без мутаций.
- Rollback точной applied copy возвращает исходные значения; при последующих изменениях блокируется, чтобы не затереть их.
- Результат synthetic follow-up — сравнение значений и estimate, не ручное игровое evidence. Реальный follow-up для OBS-01 ещё не проведён.

Проверка пересчитывает SHA256 snapshots реального packet, DPS, damage/overkill и XP, сохраняет unsupported как unsupported. Before/after hashes всех игровых Content JSON должны совпасть. Вывод прогона: [verification.txt](verification.txt).
