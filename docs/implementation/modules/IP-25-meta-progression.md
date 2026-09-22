# IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Контракт реализуется отдельным Game.Meta и feature-owned UI; состав текущего packet и evidence определяет STATUS. Старый scope не является отдельным этапом.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-03](IP-03-character-stats.md), [IP-12](IP-12-character-framework.md), [IP-16](IP-16-field-framework.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
нулевую meta для проверки баланса и неизменный unlock mapping DECISION-0050.
Это Proposed packet по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
использовать как production data только после approval, затем выполнить проверки этого IP.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD «Мета-прогрессия»; только unlock/economy поля выбранных CHAR/FIELD/SKILL/SET и meta definitions; UI §§16–17; run identity/RunOutcome contract IP-01 и producer events IP-04/IP-06/IP-07/IP-11.

## Scope

versioned profile и миграция, currency/conditions/purchases/global+per-character upgrades; idempotent application завершённого run; один authoritative result для сохранения, UI и Retry. `RunOutcome.Contributions["draft"].DraftTotals.BookCurrency` — уже начисленная при подборе пустых Книг валюта (DECISION-0020); перенос в профиль не создаёт повторную награду. DTO snapshot находится в Run, прямой dependency на Progression не требуется. Failure/abort handling, награды и сохранение — DECISION-0037; суммы/каталог — раздел «Мета-экономика» Content Design.

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

CG-03/G-15 resolved по [DECISION-0037](../../decisions/0037-meta-economy-and-persistence.md). IP-25 поставляет реальные economy definitions; игровые production definitions/art поставляют их catalog IP. Framework fixtures отдельно от production content. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Character access boundary

Profile adapter реализует `ICharacterAccessProvider.GetLockReason(ContentId)` из [IP-12 API](IP-12-character-framework.md#framework-api-и-fixture-schema): null = unlocked, непустая причина = locked. Доступность повторно проверяется при запуске; цены/условия не хранятся в UI. `FixtureCharacterAccessProvider` остаётся fake-профилем для тестов без persistence.

Аналогичный `IFieldAccessProvider`/`FieldRoster` поставляет [IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema).
`RunOutcome.Selection.FieldId` даёт release-safe identity поля для unlock/reward processing;
«завершить FIELD» = выжить 900 running seconds (DECISION-0037). Fixture unlock list не становится production economy.

## Конкретный economy / persistence packet

Читать [DECISION-0037](../../decisions/0037-meta-economy-and-persistence.md) полностью и
раздел «Мета-экономика» CD. Scope включает JSON reward 5×L, Book 50, четыре META
upgrades, character purchase prices и полный unlock mapping. Production gameplay
не требуется запускать до его catalog IP: integration использует synthetic IDs,
а ссылки economy проверяются против approved content manifest.

Проверить L1→Quit=5; L20+2 Books=200 без второго начисления Book; startup failure=0;
первый terminal wins; field clear только при 900s живым, пауза исключена. Сохранение
RunId/reward/unlocks атомарно; retries/duplicate purchase intents идемпотентны.
Покрыть caps, additive global+personal bonuses, применение до Health init следующего
run, corrupted/backup/future-version/known-migration cases и pending save failure UI.
Hard-crash checkpoint/recovery и gameplay resume вне scope по условию пользователя.

## Runtime API / schema / reset

`Game.Meta.IProfileService` — профиль, баланс, access, levels, покупки и применение
terminal outcome; `ProfileService` владеет состояниями NotLoaded/Loading/Ready/Saving/
PendingResult/LoadError. `ProfileRunBinding` наблюдает только RunModel, без recorder.
`ProfileAccessProvider` реализует character/field access; build entries фильтруются
по IsUnlocked до создания draft. `Modifier(characterId)` даёт один source-owned
вклад global+personal stats; composition применяет его до Health init.

`ProfileCodec` schemaVersion=1: currency, firstRun, upgrades (stable keys META-ID
или META-ID:CharacterId), unlocked, clearedFields, runs (RunId→receipt). Регистр ID
сохраняется. IProfileMigration — явный шаг версии; неизвестная версия блокируется.
`MetaCatalogData`, `MetaUpgradeData`, `MetaUnlockData` задают JSON schema required
fields; unknown properties запрещены. Production/fixture catalogs разделены.

`FileProfileStore` выполняет IO вне main thread. Fixture application save:
`Application.persistentDataPath/fixture-profile-v1.json`, backup `.bak`, временный
`.tmp`. Production application должна использовать отдельный профиль со своим
production catalog, не переинтерпретировать FIXTURE IDs. Для tests инжектировать
MemoryProfileStore через ConfigureProfile до Start (см. ProfileSmokeScene).

Reset из load-error UI сохраняет повреждённые файлы с `.preserved-<guid>`;
не применяется к неизвестным версиям. Для полного ручного сброса fixture testing
при закрытом приложении переместить main и backup в отдельную папку; следующий
старт создаёт новый профиль. Настоящий gameplay run не восстанавливается после
жёсткого сбоя. UI semantic IDs — `GameplayUiElementIds.Meta*`, ресурс
`UI/MetaScreen`; HP/DMG text placeholders без новых raster assets.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-03](../milestones/FIELD-001-start.md#f1-03):
новый production profile 10/10/5 и DECISION-0050 unlock metadata; terminal integration в F1-08. Packet prerequisites: F1-00/01/02; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).

DECISION-0050 заменяет только unlock mapping/CHAR-002 gate предыдущего DECISION-0037. Новые условия применяются без боевой реализации поздних IDs; старое Verified не покрывает эту delta.
