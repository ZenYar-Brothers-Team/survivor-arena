# Иконки SET-021…035 — визуальный review v001

Все 15 canonical cards прочитаны через `scripts/content/read_card.py`; owners Approved по DECISION-0138.
Генератор: OpenAI built-in image generation, отдельный запрос для каждого owner.
Точные промпты: [briefs.json](briefs.json). Исходники: `set-021-icon.png`…`set-035-icon.png` в этой папке, lossless копии результатов генератора.

![Все иконки, крупно и в 48 px](review-sheet.jpg)

Силуэты различают пары камня, льда и орбиты. Матовые поверхности, сливовый контур,
крупные формы и небольшие золотые связки продолжают ART_DIRECTION; рамки принадлежат UI.

Технический осмотр: 15 RGBA PNG, каждый 1254×1254. Максимальная alpha в двухпиксельной
внешней рамке — 0 или 1 из 255. [technical-review.json](technical-review.json) сохраняет размеры,
alpha bounds и border max. Для runtime выбран стандартный `fit` 256×256, padding 16,
cropAlpha true, alphaNoiseCutoff 2: удаляется только шум alpha=1; masters остаются неизменными.
Общий лист — обзорная композиция на фоне панели, исходные иконки не изменены.

Первоначальный [draft](packet-pending-approval.json) сохраняет исходные hashes и canonical paths.
Пользователь выбрал весь набор 2026-10-02: «утверждаю, встраивай в игру».
Фактическое approval и точные промпты сохранены в [утверждённом пакете](../../../../Art/Packets/low-tier-set-icons-2026-10-02.json).
Техническая поставка и результаты проверок — [evidence](../../evidence/2026-10-02-low-tier-set-icons.md).
Текущий execution status — только [STATUS](../../STATUS.md).
