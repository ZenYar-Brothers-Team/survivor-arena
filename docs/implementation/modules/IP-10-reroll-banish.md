# IP-10 — Reroll/banish для обновлённого драфта

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить run-local counters, invalid-action protection и reset. Распространение на Book и set offers задаётся явно.

## Зависимости

[IP-07](IP-07-level-up-draft.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Опыт и level-up», «Сеты»; UI §7 Banish/Reroll и §12 Book; draft runtime/counters.

## Scope

Reroll/rebuild по текущему request revision; Banish mode → select card → normal chooser; origin-aware policy/counters hooks; callback provider для set reroll semantics без reverse dependency на IP-11. Counters и recovery — config, no view-owned mutation.

## Out of Scope

Final counts/recovery без data proposal, собственный duplicate Book runtime, set effect execution.

## Acceptance criteria

Successful reroll меняет offers при наличии альтернативы; invalid/exhausted/double intent не расходует counters. Banished entry не возвращается в run. Empty result определяется только после дозаполнения по DECISION-0019, которое также исключает banished sets; затем request разрешается по G-01; next queued request и other pause не теряются. Новый run сбрасывает counters/banishes; Book применение следует G-02/G-03.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Remaining counts, explicit Banish mode/cancel/disabled reasons, updated card set, семантические intents; mechanic работает до финальных icons.

## Проверки

Alternative offers, exhausted counters/pool, persistence through queued drafts, mode cancel и double click; pure presenter и PlayMode flow. Set-specific chance reroll tests в IP-11.

## Документационные изменения

Origin/policy table с IP-07/IP-11/IP-28; Context/criteria/evidence revised.

## Gates и недостающие решения

G-03 Book ordinary pool/shared counters и empty-at-pickup currency закрыт DECISION-0020. G-02 закрыт DECISION-0022: ordinal content ID, reroll повторяет все checks, banish сохраняет результаты остальных сетов, включая не показанные успешные. Валюта не выдаётся повторно при исчерпании pool через controls. Численные counters остаются CG-04. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Origin / policy contract (DECISION-0022)

| Операция | Level-up | Book |
|---|---|---|
| Открытие | Новый snapshot всех eligible set checks, ordinal ID | То же; XP/level неизменны |
| Reroll | Новый snapshot, общий run counter | То же |
| Banish | Run-local ID exclusion; тот же snapshot остальных checks | То же; без повторной currency |
| Empty после backfill | Завершить request и перейти к следующему, сохранить другие pause | То же; immediate currency только при изначально пустом pickup |
| Новый run | Новый runtime/build/counters/banishes/snapshots | То же |

`SetDraftCheckState` хранит результаты provider для одного opening/reroll. Provider получает все eligible inputs и capacity равный их числу; возвращает все успешные, включая overflow, без дублей. `DraftPool` нормализует порядок, берет первые slots, выполняет ordinary fill/backfill. Banish фильтрует banished IDs, не вызывает provider повторно. Production global chance реализует IP-11.

Reroll гарантирует изменение набора, когда есть допустимая замена после применения set-first policy; при том же полном наборе приоритетных сетов первые три по ID сохраняются. Controls расходуются один раз на валидную операцию, даже когда pool не содержит альтернатив. Fixture counts/reset берутся из existing progression JSON; production recovery не назначается.

UI использует immutable `DraftViewState`, presenter-owned banish revision, semantic IDs `draft-banish-mode` и `draft-control-hint`. Click card в Banish вызывает banish intent, Cancel не изменяет gameplay. Отдельный экран/изображения не нужны.
