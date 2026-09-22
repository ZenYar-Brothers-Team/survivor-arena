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

Rendering API IP-10A: `DraftOptionViewState.Recipes` принимает ordered `RecipeProjectionViewState` с ready component/threshold strings; `SetRecipeProgressViewState.HasProgress` отделяет partial possession от fulfilled count. Producer IP-11 вычисляет и сортирует эти данные, renderer не выводит eligibility из текста.

## Проверки

Recipe truth tables/thresholds, global chance/order/short pool/uniform backfill без повторов, fake Book policy, reroll/banish, shared recipes, proc source/counters/multiwave, fixed cooldown, buff expiry/remove/rollback; PlayMode several simultaneous sets and queued choices. Per-ID production correctness — IP-19.

## Документационные изменения

Новая selection/effect contract и замена outdated per-set chance text; source/proc DECISION; IP-19 и related UI/effect contracts. Existing verification сохраняется как historical baseline.

## Gates и недостающие решения

G-08 закрыт DECISION-0017; дозаполнение свободных позиций утверждено DECISION-0019. G-02 закрыт DECISION-0022; provider возвращает все успешные checks, IP-10 сохраняет snapshot для banish и пересоздаёт при reroll. G-04/G-05/G-13 остаются: disc-return и trash-explosion conflicts, exact thresholds/effect values. Framework fixtures не назначают production значения. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-19](IP-19-production-sets.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Реализованный framework contract

Global setting `draft.setDraftChance` в `FixtureRunSetup.json`: [0,1], required при JSON load. Например 0.5 даёт каждому eligible set независимую вероятность 1/2; это fixture, не production balance. В SetDefinition/JSON поля per-set chance больше нет. Checks возвращают все successes в ordinal ID порядке; IP-10 request snapshot сохраняет overflow. Failed backfill использует тот же стабильный входной порядок и uniform sampling без повторов.

`SetDefinition` валидирует 3–6 разных components, каждый threshold 1–6. Новый JSON effect discriminator `kind` обязателен. `StatBuff`/`SkillTransform` требуют `modifier`; атаки/procs требуют `attackTemplate` и положительный `cooldownSeconds`; `ActivationProc` требует положительный `activationCount`; `LevelHeal` требует `healFraction` в (0,1]. `buffSeconds` ≥0 задаёт optional duration proc-buff; 0 означает отсутствие временного бафа. Значения seconds используют только running time. Например fraction 0.05 при max HP 100 запрашивает 5 HP до обычного health-restoration modifier/cap.

Runtime families: keyed stat buffs (включая defense/economy), additive per-skill transforms, ordinary-activation counters/procs, typed reward procs с cooldown, level healing и independent fixed-cooldown attacks. `SetEffectHost` — адаптер ActiveSkill/Character/XP; ownership/source contract записан в [DECISION-0025](../../decisions/0025-set-effect-source-and-ownership.md). Production payloads не регистрируются.

UI producer передаёт immutable ordered projections, текущий/ожидаемый threshold count, component detail с выделением текущего option. Partial possession выставляет HasProgress даже при нуле выполненных thresholds. Acquired sets отдельно; acquisition notification и DEV proc/source counters используют существующие surfaces IP-10A.

### Compatibility matrix SET-001…020

Это сопоставление семейств, не per-ID production verification. Конкретные per-skill parameter transforms (например return phase, chain targets, projectile speed/pierce, orbit condition) и attack payloads подключаются в IP-19 по полным approved данным; базовые channels не подменяют их.

| Cards | Семейство / граница |
|---|---|
| SET-001 | Per-skill damage/size/knockback; fixture OVERCHARGE |
| SET-002 | Per-skill transform; return-phase payload и disc-return G-04 остаются production gate |
| SET-003 | Per-skill transform; chain-specific targets/jump/falloff — production payload |
| SET-004 | Component transform/control synergy; slowed-target condition — production payload |
| SET-005 | StatBuff + LevelHeal; fixture RECALL |
| SET-006 | Defense/sustain StatBuff; fixture GUARD; potion application — IP-28 |
| SET-007 | Per-skill damage/size/range; fixture OVERCHARGE показывает keyed composition |
| SET-008 | Counter/transform family; actual projectile replacement и G-05 — production payload |
| SET-009 | Per-skill damage/range/size; pierce — production payload |
| SET-010 | Orbit/control transform; continuous orbit condition — production payload |
| SET-011 | Per-skill buffs; speed-specific payload — production adapter |
| SET-012 | Defensive StatBuff + component transform; fixture GUARD/OVERCHARGE |
| SET-013 | IndependentAttack; bounded branch payload — production data/adapter |
| SET-014 | Multi-skill damage/action-speed/size transforms; common-key ownership/composition |
| SET-015 | Skill transforms + RewardProc; fixture GUARD uses typed fake event, real potion — IP-28 |
| SET-016…020 | IndependentAttack; fixture STRIKE verifies fixed timer, source, generic damage/knockback. Exact directional/telegraph/projectile/cap payloads — IP-19 |

Fixture numbers относятся только к `FIXTURE-*`; G-04/G-05/G-13 и production art не закрываются этим контрактом.
