# IP-08 — Active-skill levels, targeting и effect families

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Доработать существующий skill framework, который уже заменил IP-05 prototype. Не откладывать недостающие target mechanics в поздний IP.

## Зависимости

[IP-05](IP-05-active-skill-runtime.md), [IP-07](IP-07-level-up-draft.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD auto attacks/stat rules; Content SKILL-001…016 compatibility matrix, PASSIVE-012/013 parameter applicability; active definitions, targeting modes, executor и projectile pool.

## Scope

Сохранить projectile/AoE/beam/orbit/chain/boomerang/mine/multi-wave; добавить movement/last-direction, uniform random valid world target, fixed axes и independent random directions; sequential nearest-unhit chain; skill-wide per-target boomerang cooldown; linear deceleration/despawn. Cumulative multi-parameter L1…L6 resolver, distinct range/area/hitbox/projectile size и action speed applicability. Targets через IP-05 adapter, source/level snapshot контракт.

## Out of Scope

Production registration/art SKILL IDs, новые statuses, set recipes/effect policy, numerical rebalance.

## Acceptance criteria

Fixtures покрывают все новые families. SKILL-006 compatibility cooldown общий для projectiles одного skill/target. Random target не ограничен viewport; chain выбирает следующую цель от последней, no repeat. Decelerating projectile не остаётся collider после stop. Cumulative modifiers не применяются дважды; size и range не дублируют bonus. Six concurrent skills, pause/end/reset/pool stable. Будущие boss/Traveler category tests принадлежат их модулям, а не являются обратной зависимостью IP-08.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Build six active slots/levels, resolved upgrade delta; fixture pattern/source/target/ledger diagnostics, gameplay visible effects; production icons поставляет IP-17.

## Проверки

Seeded target/direction and nearest-unhit ordering, boomerang shared ledger, no-target/movement-zero fallback, falloff/pierce/deceleration/range, L1→L6 cumulative numbers, six-skill coexistence, pause/end/pool; representative PlayMode real runtime patterns.

## Документационные изменения

Effect→parameter applicability matrix и code/test trace до 016; синхронизация IP-17 и shared stats IP-03/IP-09; no-opinion balance values из approved configs.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017: использовать утверждённый map и snapshot при активации. G-04 return disc уточняется только для затронутого SKILL-008/SET-002; не выдумывать return внутри framework. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md).

## Потребители

[IP-09](IP-09-passive-framework.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-15](IP-15-boss-framework.md), [IP-17](IP-17-production-skills.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт параметров для потребителей

Основание: [DECISION-0017](../../decisions/0017-combat-control-semantics.md) и [DECISION-0021](../../decisions/0021-additive-skill-level-bonuses.md). Применение внешних stats выполняет executor по snapshot `ActiveSkillActivation`; JSON хранит собственные resolved параметры уровня без повторного применения passive bonuses.

| Effect / поиск | Size (PASSIVE-012) | Range (PASSIVE-013) | Не масштабируется этими stats |
| --- | --- | --- | --- |
| Projectile burst, ricochet, sphere | Collision radius, impact/explosion radius | Lifetime; ricochet search radius; для linear decay — время до остановки | Speed, count, pierce, retention |
| Boomerang | Collision radius | Дальность разворота и lifetime | Speed, shared target cooldown, return damage/knockback multipliers |
| Area / pulse / delayed blast | Radius, включая secondary radius | Только radius поиска цели, если он есть | Delay, число волн |
| Beam | Width | Length | Duration, tick interval |
| Orbit | Blade hitbox radius | Orbit radius | Angular speed, blade count, hit interval |
| Chain | Нет геометрического hit size | Jump range | Target count, damage retention |
| Mine | Blast radius, secondary blast radius | Trigger radius | Lifetime, cap, secondary delay |
| Nearest / random world targeting | Нет | Configured targeting radius | Равномерность random selection, viewport не участвует |

Action speed меняет только cooldown новой активации: `baseCooldown × characterBaseCooldownMultiplier / (1 + characterActionSpeedBonus + skillActionSpeedBonus)`. Damage, source/level, outgoing knockback, size и range фиксируются при активации; delayed waves сохраняют snapshot. Бумеранг хранит cooldown по жизни цели в ledger экземпляра навыка, общем для его снарядов и активаций. Pause не двигает его время; terminal/reset очищает его.

### JSON authoring L1…L6

`FixtureActiveSkillCatalog` принимает ровно один формат: прежний `levels` (шесть полных resolved уровней) либо `baseLevel` + шесть `levelChanges`. Каждый change содержит `overrides` абсолютных значений/поддеревьев и `bonuses` с конкретными путями, например `baseDamage` или `waves[0].effects[0].collisionRadius`. Путь должен существовать; counts/seeds задаются абсолютной заменой. Resolver работает при загрузке, не на каждом кадре.

Проценты на одном пути складываются к базе: `20 × (1 + .30 + .25) = 31`. Absolute override заменяет базу указанного пути и снимает накопленные bonuses этого пути/поддерева. `actionSpeedBonus` уже является аддитивной долей и суммируется непосредственно. Domain constructors валидируют каждый resolved уровень; выдача уровня или повторное чтение не мутирует definitions. Относительные secondary damage/radius/knockback остаются коэффициентами родительского эффекта. При замене целого массива waves автор указывает его полную конфигурацию.

Movement targeting требует configured initial direction; после ненулевого input сохраняет последнее направление. Random target/direction требует JSON seed, одинаковый на всех уровнях навыка. Это fixture reproducibility, не утверждение production seed policy.

### Compatibility trace (не регистрация production content)

Код: `Assets/Game/ActiveSkill/Progression`, `Model`, `Runtime`; fixtures: `Assets/Resources/Content/ActiveSkills/FixtureActiveSkills.json`. Тесты ниже находятся в `Assets/Game/ActiveSkill/Tests`.

| Content ID | Framework / fixture | Проверка контракта |
| --- | --- | --- |
| SKILL-001 | Burst, distinct nearest targets, optional ricochet / RICOCHET | `ProjectileLifecycleTests`, `ActiveSkillProgressionFrameworkTests` |
| SKILL-002 | Fan, lifetime, pierce, projectile size / BOLT | `SkillEffectMappingTests`, `ActiveSkillProgressionFrameworkTests` |
| SKILL-003 | Separate orbit radius / blade hitbox / ORBIT | `SkillEffectMappingTests.SpatialMapping_SeparatesAttackSizeFromReachInRealDamageQueries` |
| SKILL-004 | Area, relative damage, delayed waves / DELAYED | `CombatAttackPipelineTests`, `SceneActiveSkillEffectExecutorTests` |
| SKILL-005 | Current/last movement direction, pierce / MOVEMENT | `SkillTargetingTests`, `ProjectileLifecycleTests` |
| SKILL-006 | Return, shared cooldown and target-life identity / BOOMERANG | `ProjectileLifecycleTests.Boomerangs_ShareCooldownAcrossCastsAndPassesAndDistinguishReusedTargetLife` |
| SKILL-007 | Nearest-unhit sequential chain / CHAIN | `SkillEffectMappingTests.Chain_UsesLastHitAsOriginAndNeverRepeatsTargets` |
| SKILL-008 | Nearest-other ricochet, repeat fallback / RICOCHET | `ProjectileLifecycleTests`; return/set policy G-04 остаётся production gate |
| SKILL-009 | Mine cap, expiry, relative secondary blast / MINE | `SceneActiveSkillEffectExecutorTests`, `CombatAttackPipelineTests` |
| SKILL-010 | Uniform world target, captured point, distinct delayed waves / WORLD-TARGET | `SkillTargetingTests`, `SkillEffectMappingTests` |
| SKILL-011 | Ring, rotation per activation, delayed wave offset / RING | `ActiveSkillProgressionFrameworkTests`, `SceneActiveSkillEffectExecutorTests` |
| SKILL-012 | Beam width/range/ticks, existing target tracking / BEAM | `SkillEffectMappingTests`, `CombatAttackPipelineTests`; production tracking tuning проверяет IP-17 |
| SKILL-013 | Fan + control profile / BOLT | `CombatAttackPipelineTests.EveryEffectFamily_PreservesDamageSourceLevelAndControls` |
| SKILL-014 | Independent directions, impact + explosion, expiry, pierce / SPHERES | `ProjectileLifecycleTests`, `SkillEffectMappingTests` |
| SKILL-015 | Fixed 4/8 axes and wave rotation | `ActiveSkillProgressionFrameworkTests.DirectionGenerator_CreatesFanRingAndFixedAxisCross` |
| SKILL-016 | Independent directions, linear decay and stop cleanup / DECELERATING | `ProjectileLifecycleTests`, `ActiveSkillPatternSmokeTests` (Bootstrap PlayMode) |

Общие проверки: `SkillLevelResolutionTests` — additive levels; `PlayerActiveSkillSetRuntimeTests` — шесть навыков/pause/shutdown/reinit; `CombatAttackPipelineTests` — source/level/controls/snapshot. Build UI сохраняет шесть slots и levels, upgrade preview показывает resolved spatial deltas. Development Build tab показывает ID, level, targeting, casts, target point и ledger; он остаётся gated, collapsed и scrolling по DECISION-0005. Числа fixtures и placeholder presentation не заменяют production balancing/art IP-17.
