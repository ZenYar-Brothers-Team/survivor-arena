# DECISION-0022 — Set checks при reroll/banish

Дата: 2026-09-21
Статус: Approved
Основание: пользователь утвердил предложенные правила G-02 и реализацию IP-10 ответом «да».

## Решение

- При открытии каждого запроса (level-up или Book) доступные сеты проверяются в ordinal порядке стабильного content ID. Проверяются все доступные сеты, даже если первые три slots уже заполнены; успешные занимают slots в том же порядке.
- Reroll заново выполняет проверки всех доступных сетов и пересобирает предложения. Banish исключает выбранный content ID до конца run и пересобирает предложения, сохраняя результаты остальных set checks текущего запроса, включая не показанные успешные сеты.
- Ordinary fill и uniform backfill сохраняют DECISION-0019. Например, при успешных A/B/C/D и трёх slots видны A/B/C; banish B даёт A/C/D без нового броска. Неудачный E может участвовать в дозаполнении, но не становится успешным check.
- Следующий запрос имеет собственные проверки; Book использует тот же policy и общие run-local counters. Banish не отменяет уже полученные уровни/эффекты компонентов. Exhaustion через controls не создаёт валюту Книги (DECISION-0020).
- UI: Banish → выбор карточки → обычный chooser; Cancel возвращает chooser без расхода. Revision защищает от повторных/устаревших intents. Изменение revision или закрытие draft сбрасывает режим.

## Границы реализации

IP-10 владеет временем жизни snapshot, callback provider, controls и UI. IP-11 поставляет общий production set chance и set policy; существующий per-set fixture adapter не объявляется production реализацией. Production количества controls/recovery остаются CG-04, используемые framework fixtures берутся из JSON.

## Проверка и влияние

GDD «Опыт и level-up» / «Сеты», UI §7/§12, DESIGN_SYNC и IP-07/IP-10/IP-11/IP-28 синхронизируются. Проверить повторные reroll checks, banish без новых checks, overflow successful sets, ordinal order, short/empty/backfill, queue/pause, Book counters/no extra currency, reset и UI cancel/stale intents. IP-11 проверяет production probability policy отдельно.
