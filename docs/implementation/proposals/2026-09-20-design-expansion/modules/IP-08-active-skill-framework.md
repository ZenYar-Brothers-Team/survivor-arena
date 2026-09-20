# IP-08 — Active-skill levels, targeting и effect families

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-08-active-skill-framework.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Доработать существующий skill framework, который уже заменил IP-05 prototype. Не откладывать недостающие target mechanics в поздний IP.

## Зависимости

[IP-05](IP-05-active-skill-runtime.md), [IP-07](IP-07-level-up-draft.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD auto attacks/stat rules; Content SKILL-001…016 compatibility matrix, PASSIVE-012/013 parameter applicability; active definitions, targeting modes, executor и projectile pool.

## Scope

Сохранить projectile/AoE/beam/orbit/chain/boomerang/mine/multi-wave; добавить movement/last-direction, uniform random valid world target, fixed axes и independent random directions; sequential nearest-unhit chain; skill-wide per-target boomerang cooldown; linear deceleration/despawn. Cumulative multi-parameter L1…L6 resolver, distinct range/area/hitbox/projectile size и action speed applicability. Targets через IP-05 adapter, source/level snapshot контракт.

## Out of Scope

Production registration/art SKILL IDs, новые statuses, set recipes/effect policy, numerical rebalance.

## Acceptance criteria

Fixtures покрывают все новые families. SKILL-006 compatibility cooldown общий для projectiles одного skill/target. Random target не ограничен viewport; chain выбирает следующую цель от последней, no repeat. Decelerating projectile не остаётся collider после stop. Cumulative modifiers не применяются дважды; size и range не дублируют bonus. Six concurrent skills, pause/end/reset/pool stable. Будущие boss/Traveler category tests принадлежат их модулям, а не являются обратной зависимостью IP-08.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Build six active slots/levels, resolved upgrade delta; fixture pattern/source/target/ledger diagnostics, gameplay visible effects; production icons поставляет IP-17.

## Проверки

Seeded target/direction and nearest-unhit ordering, boomerang shared ledger, no-target/movement-zero fallback, falloff/pierce/deceleration/range, L1→L6 cumulative numbers, six-skill coexistence, pause/end/pool; representative PlayMode real runtime patterns.

## Документационные изменения

Effect→parameter applicability matrix и code/test trace до 016; синхронизация IP-17 и shared stats IP-03/IP-09; no-opinion balance values из approved configs.

## Gates и недостающие решения

G-08/G-09: applicability speed/size/range/snapshot. G-04 return disc уточняется только если выбранное решение меняет SKILL-008; не выдумывать return внутри framework. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-09](IP-09-passive-framework.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-15](IP-15-boss-framework.md), [IP-17](IP-17-production-skills.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
