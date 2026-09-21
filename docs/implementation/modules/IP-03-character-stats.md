# IP-03 — Character stats, Health и новые stat channels

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить общий Health, keyed composition, proportional current HP и caps. Добавить stat channels в существующую модель и JSON mapping.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-02](IP-02-player-movement.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Управление, бой и выживание», «Опыт и level-up», «Активные умения…», «Персонажи»; Content Characters/Passive Items schemas, PASSIVE-002/005/007/011…014, связанные CHAR cards только для используемых каналов; DECISION-0004/0006/0009/0012.

## Scope

Base/current max HP, damage/healing/regen, move speed; action-speed название с прежней denominator formula; incoming/outgoing knockback, XP pickup radius, effect size/range, potion drop multiplier, low-HP damage channels. Domain values/units и change notifications; low-HP stat пересчитывается при damage/heal/max-HP изменении. Shape applicability и activation-time consumption принадлежат IP-08/IP-05, реальный potion roll — IP-28.

## Out of Scope

Применение knockback/slow к цели, production passives, world drops, visual scaling коллайдера.

## Acceptance criteria

Percent sources складываются; cooldown=base/(1+sum actionSpeed), positive cooldown>0; damage reduction≤99%; max-HP change сохраняет долю. Resistance in [0,1]; no NaN/Infinity. Low-HP formula из PASSIVE-014 даёт 0 bonus при full HP и заданный maximum при ≤10%; смена HP не вызывает recursive Changed loop. No tuning defaults в DTO; per-kind required errors называют поле.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HP HUD и актуальные stat snapshots; player text говорит action speed, не прямое сокращение duration. DEV позволяет проверить effect channels через presenter intents.

## Проверки

Damage/death/heal/regen, stacking/key replacement/removal, cooldown/cap/HP ratio, low-HP границы 100%/10%/<10%, heal и max-HP recompute, missing fields; paused regen. Численные примеры берутся из утверждённых formulas, fixture tuning явно отдельно.


Контракт потребления skill stats и JSON L1…L6: [IP-08 parameter mapping](IP-08-active-skill-framework.md#контракт-параметров-для-потребителей), [DECISION-0021](../../decisions/0021-additive-skill-level-bonuses.md). Size/range применяются один раз executor-ом из activation snapshot; passive definitions не переписывают skill level data. Повторные проценты skill upgrades складываются к базе.

## Документационные изменения

Stat dictionary с units/ranges; JSON/domain mapping; терминологическая миграция DECISION-0004; синхронизация затронутых IP-06/08/09/12.

### Реализуемый stat / JSON contract

| Domain channel | Units / composition | Consumer |
|---|---|---|
| `ActionSpeedBonus` | доля, сумма неотрицательных источников; effective cooldown multiplier = base multiplier / (1 + sum) | IP-05/08/09 |
| `PickupRadius` | world units; base radius × (1 + sum radius bonuses) | IP-06 |
| `KnockbackResistance` | доля 0…1, base + sum с cap 1 | IP-05 |
| `OutgoingKnockbackMultiplier` | 1 + base bonus + sum outgoing bonuses | IP-05/08 |
| `EffectSizeMultiplier` / `EffectRangeMultiplier` | отдельные множители base × (1 + sum); применимость параметров не выбирается здесь | IP-08 |
| `PotionDropMultiplier` | относительный множитель base × (1 + sum), не прибавка процентных пунктов к шансу | IP-28 |
| `LowHealthDamageMultiplier` | 1 + sum maximum bonus × min(1, (1 − health ratio) / 0.9) | IP-05/08/09 |

`CharacterHealthStatBinding` связывает HealthChanged со stats и снимает подписку при Shutdown. Изменение max HP сохраняет долю HP, а изменение только damage multiplier не пересчитывает здоровье повторно. Low-HP multiplier потребляется при активации атаки по DECISION-0017; IP-05 сохраняет snapshot, IP-11 задаёт set propagation. Входящие нечисловые значения и переполнение composition отклоняются до публикации нового состояния; неудачная замена источника откатывается.

`CharacterBaseStatsMapper` общий для обоих character catalogs; каждое поле baseStats обязательно в JSON, включая явно записанные нейтральные значения. Отсутствующее поле названо в ошибке. Дополнительные modifier-поля могут отсутствовать: нейтральное значение берётся из domain `default(CharacterStatModifier)`. JSON/API `activeSkillCooldownReductionBonus` переименован в `actionSpeedBonus`, все существующие fixture references перенесены без изменения чисел. Новые runtime каналы доступны immutable HUD stat snapshot и в прокручиваемой вкладке DEV Run; damage/heal идут через прежние presenter intents.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017. IP-03 предоставляет текущие stat channels; IP-05 фиксирует damage при активации, IP-08 владеет applicability. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md), [IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-13](IP-13-enemy-patterns.md), [IP-25](IP-25-meta-progression.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

### Composition teardown notification

`PlayerCharacterRuntime.ShuttingDown` вызывается до очистки Health/Stats; reentry guard уже установлен. Composition owner может завершить UI/passive/set/XP consumers даже при producer-first Unity destruction. Обратный вызов Shutdown безопасен; root снимает подписку при завершении. Контракт и regression guard добавлены в IP-11, см. [DECISION-0025](../../decisions/0025-set-effect-source-and-ownership.md#teardown-observation).
