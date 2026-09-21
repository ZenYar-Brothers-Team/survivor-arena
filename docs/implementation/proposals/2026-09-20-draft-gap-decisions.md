# Принятые поправки к драфту перед IP-07

Статус: правила приняты с поправками пользователя 2026-09-21. Источник исполнения — STATUS, эта запись не хранит статус IP.

- [DECISION-0019](../../decisions/0019-draft-set-backfill.md): после обычного заполнения оставшиеся позиции равновероятно занимают доступные сеты с неудачными бросками, без повторов.
- [DECISION-0020](../../decisions/0020-draft-requests-and-book-currency.md): только Книга, пустая в момент подбора, немедленно даёт валюту. Обычный пустой level-up сохраняет XP/level без компенсации. Book не меняет XP/level и использует обычный pool/shared controls.
- После дозаполнения 1–2 варианта отображаются вместе с неактивными пустыми позициями. Общая FIFO-очередь, atomic earned-level batches, pause ownership, stale revision protection и terminal cancellation следуют DECISION-0020.
- Production сумма/ID/lifetime Книги не назначаются framework fixture. Порядок успешных set checks и reroll/banish policy утверждены DECISION-0022: ordinal content ID, новые checks при reroll, сохранённые при banish.

Ранее предложенные отсутствие гарантированного дозаполнения и отсутствие компенсации за пустую Книгу отклонены пользователем и не являются целевыми правилами.
