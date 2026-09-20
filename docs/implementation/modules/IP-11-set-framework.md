# IP-11 — Set recipes, priority draft policy и effect families

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Расширить существующий set framework. Per-set chance/unified weighted sampling заменить здесь; fixture tick-only abilities заменить полноценными reusable effect families.

## Зависимости

[IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Сеты» и draft; SET-001…020 compatibility matrix + связанные компоненты только нужных cases; UI §§8–10; Art Direction §12.1; SetDefinition/DraftPool/ISetExtraAbility.

## Scope

Recipes 3–6 components/thresholds; independent checks единого global setDraftChance; successes first в 3 slots, deterministic processing order, ordinary provider заполняет остаток. Если ещё есть свободные позиции, доступные сеты с неудачной проверкой шанса дозаполняют их равновероятно, без повторов и повторного броска, по [DECISION-0019](../../decisions/0019-draft-set-backfill.md). Подключение к IP-07 provider, reroll/banish policy к IP-10. Slot-free level-less acquired sets. Keyed stat buffs, per-skill transforms, counters/procs, defense/economy и independent set-attacks; source propagation/non-recursion, fixed cooldown set attacks вне action speed; generic modifiers по approved applicability. Real potion event binding — IP-19/IP-28; framework использует typed fake reward event без обратной зависимости.

## Out of Scope

Production 20 recipes/числа/icons, invented set interactions, новая rarity/upgrade система, обязательный standalone attack у каждого set.

## Acceptance criteria

Chance 0/1 и 0/1/2/3/>3 successes дают согласованный состав; chance=0 не блокирует дозаполнение, если сет доступен. Проверить 2 ordinary + 2 failed sets → один сет с вероятностью 1/2; 0 ordinary + 2 failed sets → оба сета; полный draft не меняется; banished/acquired/ineligible sets не возвращаются через дозаполнение; stable order не зависит от Dictionary iteration. Recipe threshold различает possession/levels, duplicate acquisition исключён; per-set probabilities не остаются вторым authority. Shared components/multiple sets compose без double modifiers. Set activation не триггерит рекурсивно other set counters; fixed vs skill cooldown tested. Shutdown снимает собственные buffs/subscriptions/timers; pool reset очищает effects. По одной настоящей fixture на каждую accepted effect family.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Draft set card, per-option recipe projection и completes/progress/already-enough states; Pause только progressed unacquired recipes, acquired list отдельно. Краткий acquisition feedback, world VFX вторичны; DEV proc/source counters.

## Проверки

Recipe truth tables/thresholds, global chance/order/short pool/uniform backfill без повторов, fake Book policy, reroll/banish, shared recipes, proc source/counters/multiwave, fixed cooldown, buff expiry/remove/rollback; PlayMode several simultaneous sets and queued choices. Per-ID production correctness — IP-19.

## Документационные изменения

Новая selection/effect contract и замена outdated per-set chance text; source/proc DECISION; IP-19 и related UI/effect contracts. Existing verification сохраняется как historical baseline.

## Gates и недостающие решения

G-08 закрыт DECISION-0017; дозаполнение свободных позиций утверждено DECISION-0019. G-02/G-04/G-05/G-13 остаются: processing order/reroll-banish policies (Book ordinary pool уже утверждён DECISION-0020), disc-return и trash-explosion conflicts, exact thresholds/effect values. Framework fixtures не назначают production значения. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-19](IP-19-production-sets.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
