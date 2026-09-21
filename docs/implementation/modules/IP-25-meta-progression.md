# IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-03](IP-03-character-stats.md), [IP-12](IP-12-character-framework.md), [IP-16](IP-16-field-framework.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD «Мета-прогрессия»; только unlock/economy поля выбранных CHAR/FIELD/SKILL/SET и meta definitions; UI §§16–17; run identity/RunOutcome contract IP-01 и producer events IP-04/IP-06/IP-07/IP-11.

## Scope

versioned profile и миграция, currency/conditions/purchases/global+per-character upgrades; idempotent application завершённого run; один authoritative result для сохранения, UI и Retry. `RunOutcome.Contributions["draft"].DraftTotals.BookCurrency` — уже начисленная при подборе пустых Книг валюта (DECISION-0020); перенос в профиль не создаёт повторную награду. DTO snapshot находится в Run, прямой dependency на Progression не требуется. Failure/abort handling задаётся явно, если reward при Quit Run ещё не описан.

## Out of Scope

выдуманные rewards/prices/upgrades, skill tree, cloud/online profile, превращение UI в владельца currency.

## Acceptance criteria

повторное открытие Results/Retry/load не начисляет reward дважды; недостаток валюты и locked conditions корректны; purchase/achievement unlock persists; corrupted/version-mismatch profile обрабатывается документированно; upgrades не double-count и действуют в следующем run по установленному lifecycle. Не зависеть от включённого IP-31 recorder; reward application использует run identity и model outcome.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

currency, upgrade icon/name/level/effect/price/Buy, condition unlock без фиктивной цены, result reward delta и save/error state; notifications новых unlocks.

## Проверки

save/load/migration, duplicate result/unlock/purchase intent, insufficient currency, condition unlock, failure recovery; presenter cards и PlayMode result→purchase→next run.

## Документационные изменения

result/reward idempotency, persistence schema и actual economic gaps, reset/testing procedure; prices и formulas через approval loop IP-32.

## Gates и недостающие решения

CG-03/G-15: prices/rewards/upgrades/achievement conditions и Quit reward semantics. Framework fixtures отдельно от production economy. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Character access boundary

Profile adapter реализует `ICharacterAccessProvider.GetLockReason(ContentId)` из [IP-12 API](IP-12-character-framework.md#framework-api-и-fixture-schema): null = unlocked, непустая причина = locked. Доступность повторно проверяется при запуске; цены/условия не хранятся в UI. `FixtureCharacterAccessProvider` остаётся fake-профилем для тестов без persistence.
