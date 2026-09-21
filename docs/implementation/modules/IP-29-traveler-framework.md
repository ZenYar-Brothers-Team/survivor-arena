# IP-29 — Traveler encounter framework

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-08](IP-08-active-skill-framework.md), [IP-13](IP-13-enemy-patterns.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-28](IP-28-world-pickups.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

утверждённый GDD «Путники»; Content Travelers schema и TRAVELER-001…010 для покрытия утверждённых behavior families; production числа не переносятся автоматически в synthetic fixtures; UI §§11–12; field/wave definitions; pool/registry/Health; Art Production §5.

[IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema) предоставляет optional
typed `FieldTravelerScheduleDefinition` reference и resolved field configuration.
`TravelerScheduleDefinition` добавляет payload/consumer и его lifecycle; composition
отвергает bare token без payload. Оба текущих fixture fields имеют свои schedule
bindings; они не задают production Traveler schedules.

## Scope

отдельная temporary encounter category, count draw 0…3 один раз на run, индивидуальные random run-time spawn instants и presence lifetime; field/time scaling; offensive, nonaggressive wandering/avoidance и support roles; source-owned shields/auras только по явно утверждённым contracts; kill→Book, timeout→escape без Book; live list для нескольких Travelers и off-screen indicators. Собственный RNG stream не переставляет ordinary draft/wave outcomes. DECISION-0035 задаёт normalized probabilities, uniform type selection без повторов, расстояние двух высот экрана, окно [0,T−120], HP/damage scaling и простые support/cleanup/timeout rules. Uniform spawn time не означает гарантированный полный lifetime до конца run. Подключить все player pattern families IP-08 к target registry; поддержка не является общей collision/pathfinding системой.

## Out of Scope

production Traveler definitions/окна присутствия/XP/art, production field pools, mandatory mid-boss semantics, advanced AI/pathfinding и точный timer UX без отдельного решения. Начальные scaling coefficients задаёт DECISION-0035; дальнейший tuning отдельно.

## Acceptance criteria

deterministic count/schedule fixtures, 0/1/2/3 cases, разрешены близкие/одновременные spawns; pause не тратит присутствие; каждого active Traveler можно найти по корректной стрелке вне viewport, внутри стрелка скрыта; HP всех отображается. Все действующие active pattern families могут обнаружить, повредить и убить Traveler через target contract; support filters выбирают утверждённую ordinary-enemy категорию. Timeout и kill различаются, Book максимум один; cleanup всех support effects при despawn/end; zero-damage peaceful role не атакует скрыто.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HP bar, различимые несколько pointers без перекрытия HUD; optional precise escape timer не обязательный MVP; dev ID/role/spawn/presence/scaling и report events.

## Проверки

count distribution validation/seed, time endpoints, concurrent actors, pause/end/timeout/death, off-screen projection, support ownership and pool return; PlayMode find/kill/pick up/resolve Book.

## Документационные изменения

GDD Travelers/Content schema, encounter lifecycle и support cleanup decision, optional field refs. IP-16 не зависит от IP-29; production field bindings выполняют IP-24/IP-30. Полный target acquisition/hit regression принадлежит этому модулю.

## Gates и недостающие решения

G-11/G-12 и framework scaling semantics закрыты [DECISION-0035](../../decisions/0035-traveler-encounter-rules.md). Для fixture framework использовать explicit synthetic presence/XP/support/attack values и пул минимум трёх разных типов. G-14 production required values остаются у IP-24/IP-30. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-30](IP-30-production-travelers.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Enemy pattern integration

Использовать [IP-13 schema/runtime contract](IP-13-enemy-patterns.md#schema-и-runtime-contract): explicit per-attack controls/wind-up, отдельный dash contact, immutable source/life snapshots и reset-safe projectile pool. Encounter owner задаёт category; новые комбинации profiles не меняют draft/wave models. Phase ordering/support/escape mechanics остаются scope этого owning packet, а fixture numbers не являются production balance.

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.

## Framework API и schema

`Game.Traveler/Json` и `Content/Travelers/FixtureTravelers.json` задают восемь
FIXTURE-* definitions и два schedules. Definition: id/name/marker/RGBA color, role,
validated Enemy body с тем же ID, presenceSeconds, wanderSeconds/restSeconds,
avoidRadius/avoidSeconds, guardOffset, support kind/radius, reduction/resistance,
shieldHp/shieldSeconds/supportCooldown/supportTargets. Времена — running seconds,
расстояния — world units, HP/damage — health points, fractions ∈ [0,1] (reduction <1).
Обязательные поля, включая явные нули неиспользуемых каналов, не получают hidden defaults.
Peaceful body не может иметь attack или ненулевой contact damage.

Schedule наследует `FieldTravelerScheduleDefinition`: unique travelerIds (≥3), четыре
normalized countProbabilities, seed, fieldRank 1…10, placementAttempts >0,
endBufferSeconds=120, spawnScreenHeights=2, fieldGrowth=0.10, timeGrowth=0.50.
Units/formula и пример — DECISION-0035. Definitions/schedules регистрируются через
ContentRegistry; typed refs не используют production IDs. RNG расписания отделён
от пространственного/behavior stream и от wave/draft RNG.

`TravelerEncounterRuntime.Initialize/Shutdown` — run owner; `Tick` обрабатывает
scheduled arrivals и presence deadlines, `Spawn` создаёт life для одного определения.
Available actor → Killed/Book либо Escaped/no reward; terminal/rollback → Cancelled.
`EnemyRuntime.ConfigureEncounter` принимает движение и guard активности; поэтому
удар на deadline не выигрывает гонку с escape. Scene placement учитывает body radius
и player reachability. Длительный скачок run time не воскрешает уже истёкшее окно.

`EnemyProtection` выполняет общий approved shield/aura contract, а Traveler owner
отбирает только Ordinary targets текущего run и снимает effects по source life ID.
Шаги паузы не изменяют physics, schedule, presence и support cooldowns. Pool reuse
сбрасывает protection/controls/guard и получает новый life ID.

`ITravelerRuntime` предоставляет schedule, immutable snapshots, life/combat events и
development spawn intent. `TravelerPresenter`/`UiToolkitTravelerView` показывают HP
всех ролей, цветовые fixture markers, off-screen arrows и dev observation/команду.
Точный escape countdown остаётся только dev-данными. PlaytestSession сохраняет
schedule/seed provenance, scale, identity, outcome, applied damage и связь Book с life.
Ownership: [DECISION-0036](../../decisions/0036-traveler-runtime-ownership.md).
