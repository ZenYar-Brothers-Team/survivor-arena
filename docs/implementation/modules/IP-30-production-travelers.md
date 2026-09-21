# IP-30 — Production Travelers TRAVELER-001…010 и Book

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-29](IP-29-traveler-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

только полные одобренные TRAVELER-001…010 и связанные pickup/FIELD cards; принятые GDD Traveler rules; UI §§11–12; Art Production §§5,9; asset contracts.

## Scope

по-ID JSON definitions: HP/speed/contact/knockback/resistance, movement/attack/support, lifetime, visual/profile refs, reward refs; field/time scaling binding. Явные zero для неагрессивных вариантов. Все конкретные значения и формулы требуют отдельной completeness-проверки. Добавить production Book definition/reward refs и world/UI art. До binding отдельно заполнить отсутствующую Content Design карточку и утвердить её stable ID и параметры; источник самой Book-механики уже утверждён. ID не выводить автоматически из номера potion.

## Out of Scope

Повторное approval уже принятых Traveler designs, самостоятельное заполнение scaling/support/Book TBD, отдельная энциклопедия Travelers.

## Acceptance criteria

каждый in-scope Approved Traveler загружается по ID, демонстрирует роль и утверждённые scaled values; kill/escape/reward соответствует карточке; icon/body/pointer presentation читается; missing references rejected. Для каждого ID есть зафиксированный behavior+art review и тест/воспроизводимый smoke. Book имеет принятый stable ID, complete definition и approved runtime image; убийство Traveler создаёт одну награду, escape её не выдаёт. Production Book использует общий draft framework без копирования его правил.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

production name/HP/indicator; report содержит stable ID, spawn time, фактический scaling, outcome и Book result.

## Проверки

per-ID role/support/reward matrix, early/late field scaling boundaries, совместные появления; art audit/import checks/manual readability. Production Book reference validation и kill→drop→collect→draft, в том числе exhausted pool по принятому правилу.

## Документационные изменения

Утверждённые Traveler cards с заполненными required fields, manifest/provenance и per-ID checks; IP-24 production field bindings и IP-27 content coverage. Добавленная и отдельно согласованная Book card/ID, definition и image provenance; не повторное approval самой Book-механики.

## Gates и недостающие решения

G-03 Book draft semantics и G-10 contact/lifetime/reachability/pause rules утверждены DECISION-0020/0033; generic lifecycle реализует IP-28. Production Book card/ID/параметры, G-11/G-12/G-14/G-17 complete Traveler/scaling/support data и конкретные images остаются gates. Designs TRAVELER-001…010 уже approved. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.