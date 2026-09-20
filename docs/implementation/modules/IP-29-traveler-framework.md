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

## Scope

отдельная temporary encounter category, count draw 0…3 один раз на run, индивидуальные random run-time spawn instants и presence lifetime; field/time scaling; offensive, nonaggressive wandering/avoidance и support roles; source-owned shields/auras только по явно утверждённым contracts; kill→Book, timeout→escape без Book; live list для нескольких Travelers и off-screen indicators. Собственный RNG stream не переставляет ordinary draft/wave outcomes. Дополнительные unresolved details: normalized probabilities/type selection, spawn geometry; endpoint 15:00 vs timer victory; simultaneous encounters; scaling formula/ranges, support targets/stack/expiry/death cleanup, contact=0 meanings, death at timeout precedence. Uniform spawn time не означает гарантированный полный lifetime до конца run. Подключить все player pattern families IP-08 к target registry; поддержка не является общей collision/pathfinding системой.

## Out of Scope

production Travelers/баланс/scaling coefficients, mandatory mid-boss semantics, advanced AI/pathfinding и точный timer UX без отдельного решения.

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

G-11/G-12/G-14: temporal/spatial/type/scaling/support semantics и required values. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-30](IP-30-production-travelers.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
