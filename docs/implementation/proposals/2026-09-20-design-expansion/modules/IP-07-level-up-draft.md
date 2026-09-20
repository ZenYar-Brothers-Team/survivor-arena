# IP-07 — Трёхслотовый драфт, request queue и build progression

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-07-level-up-draft.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Сохранить 6+6, start slot, no replacement, levels 1…6 и pause ownership. Обновить существующий draft, не создавать второй для Book.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-06](IP-06-xp-progression.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Опыт и level-up», «Активные умения…», «Путники» только Book trigger; UI §§7,12; BuildEntry/DraftPool/LevelUpDraftRuntime; current empty-pool behavior.

## Scope

3 offer slots; request origin LevelUp/Book и pending queue; ordinary sampling/eligibility; pluggable set-offer provider contract без dependency на IP-11; per-revision offers и apply once. Book request не трогает XP/level. Immutable option presentation payload включая current→new values; recipe projection подключает IP-11. Reroll/banish limits/actions принадлежат IP-10.

## Out of Scope

Set chance/order/effects (IP-11), реальные Book drops, Traveler schedule, UI navigation, counters recovery tuning.

## Acceptance criteria

New entry занимает правильный слот; start skill учитывается; levels/max/cap/banish/0-weight фильтры не дают invalid offers. 0/1/2 options обрабатываются по решению G-01 без фабрикации дубликатов. Book не заменяет следующий level-up. Resolve завершает один request и не снимает другие pause reasons. Terminal event отменяет/завершает pending queue по явному contract. Selection preview не мутирует build.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Origin heading, три позиции cards с честным short-pool состоянием, level delta и disabled reasons; next pending request остаётся видимым. Fixture Book intent тестирует framework без настоящего pickup.

## Проверки

Full 6+6, levels 1→6, 0/1/2/3 eligible, empty pool/only-set miss via fake provider, multiple XP/Book requests, invalid/double intent, death/end/other pause; presenter и PlayMode sequence.

## Документационные изменения

Уточнение empty/short/Book rules и queue ordering; bidirectional contracts с IP-10/IP-11/IP-12/IP-28; historical evidence отдельно.

## Gates и недостающие решения

G-01/G-03: short/empty handling, Book pool/consume semantics и event priority. Явно сохранить существующий earned-level empty skip предлагается как delta, а не молча вставить в утверждённый GDD. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
