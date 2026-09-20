# IP-11 — Set recipes, priority draft policy и effect families

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-11-set-framework.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Расширить существующий set framework. Per-set chance/unified weighted sampling заменить здесь; fixture tick-only abilities заменить полноценными reusable effect families.

## Зависимости

[IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Сеты» и draft; SET-001…020 compatibility matrix + связанные компоненты только нужных cases; UI §§8–10; Art Direction §12.1; SetDefinition/DraftPool/ISetExtraAbility.

## Scope

Recipes 3–6 components/thresholds; independent checks единого global setDraftChance; successes first в 3 slots, deterministic processing order, ordinary provider заполняет остаток. Подключение к IP-07 provider, reroll/banish policy к IP-10. Slot-free level-less acquired sets. Keyed stat buffs, per-skill transforms, counters/procs, defense/economy и independent set-attacks; source propagation/non-recursion, fixed cooldown set attacks вне action speed; generic modifiers по approved applicability. Real potion event binding — IP-19/IP-28; framework использует typed fake reward event без обратной зависимости.

## Out of Scope

Production 20 recipes/числа/icons, invented set interactions, новая rarity/upgrade система, обязательный standalone attack у каждого set.

## Acceptance criteria

Chance 0/1 и 0/1/2/3/>3 successes дают согласованный состав; stable order не зависит от Dictionary iteration. Recipe threshold различает possession/levels, duplicate acquisition исключён; per-set probabilities не остаются вторым authority. Shared components/multiple sets compose без double modifiers. Set activation не триггерит рекурсивно other set counters; fixed vs skill cooldown tested. Shutdown снимает собственные buffs/subscriptions/timers; pool reset очищает effects. По одной настоящей fixture на каждую accepted effect family.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Draft set card, per-option recipe projection и completes/progress/already-enough states; Pause только progressed unacquired recipes, acquired list отдельно. Краткий acquisition feedback, world VFX вторичны; DEV proc/source counters.

## Проверки

Recipe truth tables/thresholds, global chance/order/short pool, fake Book policy, reroll/banish, shared recipes, proc source/counters/multiwave, fixed cooldown, buff expiry/remove/rollback; PlayMode several simultaneous sets and queued choices. Per-ID production correctness — IP-19.

## Документационные изменения

Новая selection/effect contract и замена outdated per-set chance text; source/proc DECISION; IP-19 и related UI/effect contracts. Existing verification сохраняется как historical baseline.

## Gates и недостающие решения

G-02/G-04/G-05/G-08/G-13: processing order/Book policies, disc-return и trash-explosion conflicts, modifier applicability, exact thresholds/effect values. Framework fixtures не назначают production значения. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
