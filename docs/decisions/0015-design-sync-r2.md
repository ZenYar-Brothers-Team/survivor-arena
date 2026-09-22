# DECISION-0015 — Утверждённый design sync R2 и регистрация Implementation Plan

Status: Approved
Date: 2026-09-20
Related IP: IP-00…IP-32, включая IP-10A/IP-12A; подготовительная миграция M-01
Related content IDs: 121 карточка принятого каталога; semantic replacements SKILL-001, PASSIVE-002/007, SET-001…008

## Context

Коммит `a2dd22a8eb61fb851610a94e09533c017277b287` автора zhenia966 добавил Game Design v2, Content Design v2, Art Direction v2, UI/UX Design и Art Production. Пользователь подтвердил approval всех пяти документов и полную замену прежних трёх. После ревью был принят переработанный план: обновление существующих IP до реализации, пять новых модулей и новая очередь.

Последняя поправка пользователя отменяет сохранение отдельных старых версий трёх документов. Запись фиксирует принятое решение; код этим документационным изменением не реализуется.

## Decision

1. Полностью заменить содержимое `docs/Game_design.md`, `docs/Content_design.md`, `docs/art/ART_DIRECTION.md` соответствующими утверждёнными источниками. Допустима только нормализация экспортного Markdown без смысловой правки. Архивные копии, backup-файлы и отдельный snapshot старых документов не создаются и не являются условием выполнения M-01.
2. UI/UX Design и Art Production включить в authority map; ASSET_PIPELINE сохраняет технический контракт. Входные v2-файлы не становятся параллельными активными документами.
3. Зарегистрировать 35 полных спецификаций ревизии `design-sync-R2`: 30 существующих IDs/filenames и новые IP-28…IP-32. Scope меняется у 28 существующих IP; IP-00/IP-02 сохраняют behavioral contracts. Дополнять работающий код, не восстанавливать удалённый single-skill prototype.
4. Использовать Execution order только из STATUS. WORKFLOW и AGENTS выбирают первый Ready в этой очереди с выполненными prerequisites/gates целевого scope. Номер IP не означает очередь.
5. Сохранить историческое implementation/verification evidence в STATUS. Оно относится к прежнему scope. Новые критерии не получают Verified из старых test counts; пересчитать готовность. IP-00/IP-02 сохраняют проверенное состояние без behavioral delta, IP-01 становится первым Ready.
6. Снять общий CG-01 Draft blocker для 121 утверждённой target-карточки. Недостающие числа, Book card/ID, schedules, economy и реальные внутренние конфликты остаются конкретными gates в DESIGN_SYNC. Принятие плана не выбирает автоматически решение открытого вопроса.
7. Runtime Results должны работать без diagnostic recorder. IP-31/IP-32 поставляют простой локальный цикл ручных прогонов и AI review; конкретное предложение чисел/механики применяется только после его approval. Нет auto-tuning, backend или обязательного LLM API.
8. Конкретные изображения по-прежнему проходят Asset Pipeline; approval design-документов не утверждает ещё не созданные картинки. Существующий concept approval CHAR-001 не отменяется, но связь с конкретным master/runtime проверяется отдельно.

## Дополнение к прежним решениям и semantic migrations

- **DECISION-0004:** математический контракт сохраняется; каноническое название бонуса теперь action speed. `cooldown = baseCooldown / (1 + sum(actionSpeedBonus))`, без прямого вычитания процента из duration. Текущие C# names/DTO не переименованы этой миграцией; это реализация IP-03/IP-09 и связанных владельцев.
- **DECISION-0014:** разделение director/spawner, pause-aware time и data-driven timeline сохраняются. Целевой scope IP-14 теперь включает continuous и burst; W-01 cap/catch-up/категории участников не решается автоматически. Исторические continuous tests не проверяют burst.
- **Semantic IDs:** SKILL-001 меняет прежний «Искровой болт» на «Бросок камня»; PASSIVE-002 расширен/переосмыслен, PASSIVE-007 меняет lifetime на pickup radius; SET-001…008 получают новые рецепты и эффекты. Это явно принятые замены карточек под прежними IDs, а не разрешение произвольно переиспользовать другие IDs.
- Старые telemetry/save/balance данные нельзя интерпретировать по новому каталогу лишь из-за совпадения ID. Владельцы persistence/report/content обязаны различать build/config/content revisions и при необходимости мигрировать либо отклонять несовместимые данные. Сейчас production save/telemetry migration не реализуется; существующие fixture JSON не переименовываются.
- Прежние решения и их approval/evidence не переписываются; эта запись связывает сохранённые инварианты с новым target scope.

## Consequences

Действуют [35 IP-модулей](../implementation/modules/), [новый workflow](../implementation/WORKFLOW.md) и [очередь/status](../implementation/STATUS.md). Различия и открытые решения — [DESIGN_SYNC](../implementation/DESIGN_SYNC.md); общий контракт/art ownership — [ASSET_PRODUCTION](../implementation/ASSET_PRODUCTION.md); ручной balance процесс — [BALANCE_WORKFLOW](../implementation/BALANCE_WORKFLOW.md).

Согласованные точечные правила обновлены в AGENTS и .claude/rules/design-docs.md/content-json.md. Helpers и hooks не изменены. Runtime, JSON, сцены, ассеты и тесты не меняются. Автоматические проверки этой поставки относятся к документации, ссылкам, scope/dependency consistency и очереди; Unity pass не заявляется.

## Approval

2026-09-20 пользователь подтвердил, что пять новых документов утверждены, разрешил менять порядок ещё не реализованных IP, затем принял переработанный план с одной поправкой: «не нужно сохранять старые версии трех документов, их можно будет просто подменить новыми. В остальном план принимается».

Approval относится к описанной миграции и плану, а не к невыбранным альтернативам открытых G/W вопросов, будущим balance patches или будущим изображениям.
