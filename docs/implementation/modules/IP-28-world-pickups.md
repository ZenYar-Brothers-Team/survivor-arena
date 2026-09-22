# IP-28 — World pickup framework: зелье лечения и Book

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-05](IP-05-active-skill-runtime.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-11](IP-11-set-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Управление, бой и выживание», «Опыт и level-up», «Путники»; Content PICKUP-001 и PASSIVE-002/009; существующие `Health.Heal`, XP drop lifecycle/pool; UI §12; Art Production §9; canonical asset pipeline.

## Scope

Typed pickup definitions/rewards, death-drop hook для обычных врагов, pooled lifecycle, heal application и Book→draft request. Scope завершается на явно синтетических FIXTURE-* definitions для обоих reward types. Drop chance, healing, radius/lifetime и seed задаются config, без скрытых defaults. Реализовать согласованные правила full-HP pickup, activation radius, связи с XP radius, нескольких одновременных rewards и death/timer/collect ordering. Схема принимает будущие production IDs; готовые production potion/Book definitions и их изображения поставляют IP-20/IP-30.

## Утверждённые правила G-10

[DECISION-0033](../../decisions/0033-world-pickup-rules.md): full-HP зелье расходуется
с actual heal 0 и pickup effects; contact pickup независим от XP radius; по умолчанию
нет timed expiry, configured lifetime идёт только в running-time; недоступный drop
переносится в ближайшую доступную игроку точку. Chance выбирается enemy → field →
global, затем relative multiplier и cap 100%. Одновременные pickup intents имеют
стабильный порядок; Book pause откладывает оставшиеся, terminal не даёт новых наград.
Реализация документирует технический ключ порядка и reachable-point adapter.
Tests покрывают каждый пункт; production числа для этого framework не требуются.

## Out of Scope

Production pickup definitions/картинки, финальные drop rates, loot rarity, inventory, новая экономика и Traveler behavior.

## Acceptance criteria

Один lifecycle даёт награду максимум один раз; heal учитывает restoration multiplier и max HP, actual healing отличается от attempted; chance=0/1 проверяемы. Book не даёт XP/level и использует общий draft/queue; source drop ID/run ID сохранены; empty-at-pickup Book немедленно начисляет валюту по DECISION-0020. При duplicate collect нет второго request/начисления; pool оценивается после set backfill. Pause/end/escape не создают случайных наград; pooled reuse возвращает baseline. Fixtures подтверждают оба reward types и принятые edge cases. Отсутствующий production Book ID не изобретается и не мешает проверке generic framework на FIXTURE-BOOK.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

различимые pickup visuals и короткий feedback; Book heading через IP-07/IP-10/IP-11, HP через текущий HUD; dev drop/pickup/expiry counters и telemetry hooks.

## Проверки

kill vs despawn, duplicate collect, full HP, failed draft request, simultaneous rewards, pause/reset, pool lifecycle; PlayMode heal + Book flow.

## Документационные изменения

Pickup/reward schema, fixture rationale и принятые уточнения lifecycle. Зафиксировать ownership: production PICKUP-001, его drop bindings и art — IP-20; production Book card/ID, definition и art — IP-30. IP-18/IP-19/IP-20/IP-29/IP-30/IP-26/IP-27 используют один reward contract.

## Gates и недостающие решения

G-01/G-03 draft pool/consume/queue/empty-Book currency закрыты DECISION-0019/0020. G-02 закрыт DECISION-0022; Book использует IP-10 controls без собственной policy. G-10 semantics закрыты DECISION-0033. Недостающие production числа и Book ID/card блокируют соответствующие production packets IP-20/IP-30, а не этот fixture framework. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-18](IP-18-production-passives.md), [IP-19](IP-19-production-sets.md), [IP-20](IP-20-production-enemies.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Set reward boundary

IP-11 предоставляет `SetRewardEvent`/`ISetEffectHost.Rewarded`; `PlayerPickupRewardTarget` связывает событие с принятым potion pickup через composition root, сохраняя drop/run/content identity и exactly-once semantics. Обычные heal/regeneration/level-heal не являются этим событием. Set/SecondaryProc sources не наполняют чужие counters; см. [DECISION-0025](../../decisions/0025-set-effect-source-and-ownership.md).

## Framework API и fixture schema

`Game.Pickup` владеет `PickupDefinition`, `PickupLife`, `PickupIdentity` и
`WorldPickupRuntime`. `Spawn(definition, position, sourceLifeId, sourceContentId)`
создаёт отдельный drop ID; `TryCollect(visual, expectedDropId)` отвергает stale
pooled references. Lifecycle: Available → Claiming → Collected; отказ возвращает
Available, expiry и cleanup завершают жизнь без reward. Исключение reward callback
отменяет claim без повторной попытки, чтобы не выдать неопределённый эффект дважды.
`IPickupRewardTarget` отделяет lifecycle от Health/draft/set consumers.

`Content/Pickups/FixturePickups.json` содержит bindings `potionId`/`bookId`, required
`baseChance`, `seed`, `placementSkin`, `feedbackSeconds`, explicit `enemyChances` и
`fieldChances` maps, массив definitions. Definition: `id`, `kind`, `healing` (HP,
Potion > 0, Book = 0), `contactRadius` (> 0 world units), optional `lifetimeSeconds`
(null — без expiry, иначе > 0 running seconds), `marker`, RGBA `color`, `markerSize`.
Chance ∈ [0,1], relative multiplier ≥ 0: enemy override → field override → global;
итог min(1, chance × multiplier). Пример: 0.08 × 1.5 = 0.12; explicit 0 остаётся 0.
Overrides валидируют typed Enemy/Field references. Healing затем использует существующий
restoration channel и max HP; UI/telemetry показывают actual, не attempted healing.

`EnemyRewardSink` независимо передаёт authoritative death XP и pickup consumers;
drop roll выполняется один раз на ordinary enemy life. Book consumer IP-29 вызывает
общий Spawn; отдельный draft/XP pipeline ему не нужен. Контакт измеряется от
`Collider2D.ClosestPoint`, XP radius не участвует. Tick обходит snapshot в порядке
возрастающего spawn sequence; для каждого drop expiry проверяется перед contact.
После Book pause обработка остальных ждёт Running; terminal отменяет свободные drops.

`IPickupPlacement` — boundary геометрии. Fixture adapter берёт axis-aligned стены
и obstacle существующей Gameplay scene, сужает арену/расширяет obstacle на player
footprint + skin и выбирает ближайшую точку в связной со spawn области. Другая
геометрия требует своего adapter; алгоритм не объявляет production navigation готовой.

`IPickupRuntime` отдаёт immutable `PickupSnapshot`, Changed/Resolved и dev spawn intent.
`PickupPresenter`/`UiToolkitPickupView` показывают краткий HUD feedback, а fixture
кнопки/counters находятся в существующей development Build tab. Telemetry записывает
resolved state, drop/run/source identity и requested/actual heal, snapshot counters и
JSON provenance. `+`/`BOOK` — различимые text-only placeholders; production art не
создан. Межслойная схема: [DECISION-0034](../../decisions/0034-world-pickup-ownership.md).
