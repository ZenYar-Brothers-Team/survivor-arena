# IP-15 — Boss/mid-boss encounter framework

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Каркас использует общий EnemyRuntime/target registry, movement/projectile families IP-13 и hooks IP-14. BossEncounterRuntime владеет отдельными encounter lives/pools; BossCombatController — последовательностью атак и HP phases. Production definitions остаются IP-21.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-05](IP-05-active-skill-runtime.md), [IP-08](IP-08-active-skill-framework.md), [IP-13](IP-13-enemy-patterns.md), [IP-14](IP-14-wave-director.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD «Структура забега и условия завершения», «Враги, волны, элиты и боссы»; schemas и нужные attack/phase families BOSS-001…010/MIDBOSS-001…010; UI §§13–15; DECISION-0014 hooks и DECISION-0013 presentation boundary.

## Scope

configured phase thresholds и composition существующих attacks; optional midboss и обязательный final-boss hook; death/despawn/end distinction; final boss name/HP на HUD, читаемая preparation/telegraph. Если конкретная карточка требует ещё неописанный shield/support/phase rule, этот пробел выделяется до её реализации.

## Out of Scope

production BOSS/MIDBOSS definitions/assets (IP-21), самостоятельно назначенное время final spawn, обязательность убийства босса для victory.

## Acceptance criteria

hooks срабатывают однократно; boss alive/dead не меняет timer-based victory; crossing нескольких HP thresholds разрешается по определённому контракту; pause не продвигает attacks/phases. HP bar показан только пока final boss активен и убирается при death/despawn/end; обычный HUD/timer остаётся. Midboss diagnostic phase не превращается автоматически в второй global boss bar. Все используемые player skill patterns могут обнаружить/повредить boss через IP-05 target contract; final HP bar cleanup не зависит от dev telemetry.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

final HP/name + короткое boss incoming уведомление, optional midboss feedback по его роли; phase/attack/source IDs доступны в DEV/telemetry hooks. Cutscene не требуется.

## Проверки

thresholds/boundaries, boss killed/alive at timer, player death, multiple hooks, pause/end, stale subscriptions; PlayMode bar/telegraph visibility и freeze.

## Документационные изменения

boss schema, phase/telegraph ownership, missing-rule list по нужным карточкам, event contract для IP-31; final spawn times остаются encounter data IP-24.

## Gates и недостающие решения

G-07 закрыт DECISION-0017. G-14: используемые phase/attack fields; final timing configurable, production values не обязательны для synthetic framework. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-16](IP-16-field-framework.md), [IP-21](IP-21-production-bosses.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Enemy pattern integration

IP-14 предоставляет `HookTriggered` (same-time MidBoss → FinalBoss, one-shot catch-up
только при Running) и [wave runtime contract](IP-14-wave-director.md#runtime-и-fixture-schema).
Boss owner подписывается/отписывается со своим lifecycle, создаёт `EnemyCategory.Boss`
вне ordinary list wave spawner; заполненный regular cap не задерживает boss hook.
Victory по timer сохраняется независимо от исполнения hook и состояния босса.

Использовать [IP-13 schema/runtime contract](IP-13-enemy-patterns.md#schema-и-runtime-contract): explicit per-attack controls/wind-up, отдельный dash contact, immutable source/life snapshots и reset-safe projectile pool. Encounter owner задаёт category; новые комбинации profiles не меняют draft/wave models. Phase ordering/support/escape mechanics остаются scope этого owning packet, а fixture numbers не являются production balance.

## Fixture schema и phase contract

`Assets/Resources/Content/Bosses/FixtureBosses.json` → `BossEncounterData`/`BossPhaseData`
→ `FixtureBossCatalog` → immutable `BossEncounterDefinition`. Два synthetic ID:
`FIXTURE-BOSS-FINAL` и `FIXTURE-BOSS-MID`; они не поставляют BOSS-/MIDBOSS- карточки.

| Поле | Контракт / единицы |
|---|---|
| id / displayName / hook | Stable encounter identity, непустое имя; FinalBoss обязателен, MidBoss optional; один definition на hook kind |
| body | EnemyDefinitionData IP-13; id совпадает с encounter. HP >0, collision >0 world units, speed ≥0 units/s; contact interval >0 s, damage/reward ≥0; resistance 0…1 |
| spawnOffsetX/Y | Оба обязательны; finite world units относительно позиции игрока в момент hook; не используют ordinary cap или RNG потока волн |
| phases | Непустой ordered list; unique phase ID; первый healthThreshold=1, последующие строго убывают в (0,1) |
| attackEnemyIds | Непустая ordered sequence ссылок на EnemyDefinition с ranged Attack и telegraphSeconds >0; используются только attack profiles. Повтор ID означает следующий полный цикл той же атаки |

Доля HP `h = currentHealth / maxHealth`, обе величины в HP; `h ∈ [0,1]`.
При `h <= threshold` достигается фаза. Например, у fixture final 500 HP, пороги
0.6/0.3 соответствуют 300/150 HP; удар с 500 до 100 HP переводит сразу в последнюю
фазу одним событием, без промежуточных атак. Healing не возвращает фазу назад.
При HP=0 смерть имеет приоритет: phase/attack tick не выполняется.

Переход выполняется на следующем running combat tick, отменяет незавершённый
wind-up/остаток burst/cooldown предыдущей фазы и начинает первую атаку новой
с полным telegraph. Уже созданные projectiles сохраняют старые immutable profiles
и source/life ID. Movement profile и contact/dash controls принадлежат body и не
пересоздаются на HP transition. Последовательность атак повторяется; следующий
элемент начинается только после полного burst и cooldown текущего элемента.
Cooldown отсчитывается от первого shot по IP-13; если burst длиннее cooldown,
его хвост завершается до перехода к следующему элементу. Single-cycle режим
контроллера не запускает лишний burst. Каждый sequence slot сохраняет собственную
накопленную spiral rotation при повторном входе; HP phase change сбрасывает sequence.
Один tick не воспроизводит
пропущенные циклы/фазы задним числом; новая подготовка всегда остаётся читаемой.

Pause сохраняет phase, attack timers/aim, движение и telegraph. Encounter owner
выключает Rigidbody2D simulation боссов на паузе, чтобы overlap resolution соседних
коллайдеров не смещал их при нулевой velocity; Running и pool Initialize восстанавливают
simulation. HP-переходы
откладываются до running tick. Терминальное состояние синхронно despawn-ит обоих
боссов; смерть, cleanup и terminal не меняют timer victory. Shutdown снимает
подписки на конкретный director/run, освобождает active projectiles и готов к
повторной Initialize; fresh director требуется для нового расписания.

Fixture rationale: final 500 HP / mid 160 HP позволяют упражнять несколько фаз;
размеры 2.4/1.6 и offsets ±8 позволяют отличить отдельные encounter lives.
Награда 0 явно исключает присвоение production reward TBD. Пороги 1/.6/.3 и
1/.5 упражняют одиночный/множественный crossing. Fan/ring/cross/burst/single
переиспользуют synthetic IP-13 profiles. Final timing берётся только из existing
wave hook, новые production времена здесь не назначаются.

## UI, lifecycle и downstream contracts

`IBossEncounterRuntime` предоставляет final life/definition; UI runtime model
снимает immutable `BossViewState`, presenter передаёт его в HudViewState.
Событие `Changed` при spawn/HP/despawn обновляет HUD сразу, независимо от
периодического refresh; Dispose UI model снимает эту подписку.
`hud-boss-bar` показывает имя/HP только живого final boss; timer расположен выше,
обычный HUD снизу сохраняется. Один existing notification slot показывает
`BOSS INCOMING`; его expiry использует elapsed run time. Midboss не получает
global HP bar. DEV Run tab содержит phase/attack/life IDs через существующий
bounded drawer, без нового управления gameplay.

`LifeEvent` сохраняет IP-04 Died/Despawned reason contract; `CombatResolved`
передаёт target/source attribution; `PhaseChanged` публикует immutable
`BossPhaseEvent` (identity, previous/current phase IDs, выбранный attack ID).
Projectile owner — конкретная boss life, source content — attack profile ID.
IP-31 может подписываться как optional observer; UI/cleanup от recorder не зависят.
Catalog registry валидирует attack refs, source snapshot включает boss JSON.
IP-16 выбирает encounter definitions и timeline вместе; обязательный final hook
проверяется до подписок и spawn. IP-21 заменяет synthetic content после закрытия
per-card fields; [DECISION-0031](../../decisions/0031-boss-encounter-framework.md)
фиксирует техническую границу.

## Missing-rule list для production

Compatibility references: полностью рассмотрены BOSS-001 (fan/ring), BOSS-010
(несколько HP thresholds) и MIDBOSS-001/002 (dash/post-dash families). Их production
реализация не входит в fixture packet. G-14 сохраняет: exact speed/timings,
XP/rewards, угловые смещения и phase payload; post-dash coupling и серии рывков
требуют отдельной конфигурации/проверок IP-21. Повтор ranged pattern в текущей
sequence — полный цикл с cooldown, а не обещание delayed 0.4 s повторов BOSS-010.
Shield/support/escape механики не используются этим packet; при их потребности
IP-21/IP-29 сначала фиксируют missing rule, без ad-hoc поведения в EnemyRuntime.
Новые raster assets/анимации не поставляются: body placeholder и line telegraph
переиспользуют IP-13, без модификации gameplay transform ради visual motion.
