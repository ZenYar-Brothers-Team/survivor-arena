# IP-07 — Трёхслотовый драфт, request queue и build progression

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить 6+6, start slot, no replacement, levels 1…6 и pause ownership. Обновить существующий draft, не создавать второй для Book.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-06](IP-06-xp-progression.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Опыт и level-up», «Активные умения…», «Путники» только Book trigger и «Мета-прогрессия» только immediate Book currency; DECISION-0019/0020; UI §§7,12; BuildEntry/DraftPool/LevelUpDraftRuntime; current empty-pool behavior.

## Scope

3 offer slots; request origin LevelUp/Book и pending queue; ordinary sampling/eligibility; pluggable set-offer provider contract без dependency на IP-11; per-revision offers и apply once. Book request не трогает XP/level. Immediate empty-Book currency producer, dedup pickup identity и RunOutcome contribution входят в scope; persistent profile принадлежит IP-25. Immutable option presentation payload включая current→new values; recipe projection подключает IP-11. Reroll/banish limits/actions принадлежат IP-10.

## Out of Scope

Set chance/order/effects (IP-11), реальные Book drops, Traveler schedule, UI navigation, counters recovery tuning.

## Acceptance criteria

New entry занимает правильный слот; start skill учитывается; levels/max/cap/banish/0-weight фильтры не дают invalid offers. 0/1/2 options считаются после дозаполнения доступными сетами по [DECISION-0019](../../decisions/0019-draft-set-backfill.md), без фабрикации дубликатов. Контракт provider не выдаёт empty только из-за неудачных set checks; fake provider проверяет эту границу до реализации set policy в IP-11. Только Книга, пустая при принятии подбора, немедленно начисляет валюту по [DECISION-0020](../../decisions/0020-draft-requests-and-book-currency.md); ordinary empty, reroll/banish и позднее исчерпание queued pool не создают её. Pickup ID/run ID исключают повтор и старый забег; итоговое начисление доступно в RunOutcome без recorder. Book не заменяет следующий level-up. Resolve завершает один request и не снимает другие pause reasons. Terminal event отменяет/завершает pending queue по явному contract. Selection preview не мутирует build.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Origin heading, три позиции cards с честным short-pool состоянием, level delta и disabled reasons; next pending request остаётся видимым. Fixture Book intent тестирует framework без настоящего pickup; он доступен только в DEV drawer и принимает подбор только при Running. HUD показывает начисленную за пустые Книги валюту, producer event и immutable outcome сохраняют source/run/request identity и totals.

## Проверки

Full 6+6, levels 1→6, 0/1/2/3 eligible, truly empty pool/only-set miss с дозаполнением via fake provider, multiple XP/Book requests, invalid/double intent, death/end/other pause, immediate/dedup Book currency, ordinary empty и banish-to-empty без валюты; presenter и PlayMode sequence.

## Документационные изменения

Уточнение empty/short/Book rules и queue ordering; bidirectional contracts с IP-10/IP-11/IP-12/IP-28; historical evidence отдельно.

## Gates и недостающие решения

G-01/G-03 для fixture draft contract закрыты DECISION-0019/0020. Production сумма/ID/lifetime Книги остаются отдельными данными; G-02 закрыт DECISION-0022; IP-10 владеет snapshot checks и controls, production global chance реализует IP-11. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-25](IP-25-meta-progression.md) через RunOutcome без прямой зависимости на Progression, [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт реализации framework

`LevelUpDraftRuntime.RequestBook(pickupId, sourceRunId, sourceContentId)` принимает уже признанный world producer подбор. IP-28 проверяет Running до подбора; API допускает завершение ранее принятой награды во время draft pause. Неподключённая nullable Book config означает недоступный producer, а не скрытую сумму. В composition root сумма обязательна из `draft.emptyBookCurrency` JSON; fixture `1` проверяет проводку и не задаёт production награду.

Каждый `DraftSession.Revision` меняется при reroll/banish и следующем request. UI отправляет захваченную revision, устаревшая session закрывается. `ExperienceProgression.LevelsEarned(first,last)` передаёт целый диапазон после атомарного XP commit и до legacy per-level events; очередь добавляет диапазон до `DraftOpened` callbacks.

`ISetDraftOfferProvider` выбирает успешные set offers из валидных входов; pool проверяет их уникальность/принадлежность, выполняет ordinary fill и uniform backfill. `FixtureSetDraftOfferProvider` временно использует существующие per-set fixture chances и порядок definitions; production global chance/stable policy поставляет IP-11. Это не закрывает его target scope.

`DraftOption.Preview` копирует current/next levels и definition values без применения выбора. Active preview показывает base damage/cooldown/waves, passive — изменения stat contributions; это не обещание итогового урона конкретной цели с resistance или текущими временными buffs. IP-08 расширяет payload деталями effect families, IP-11 — recipe projection.
