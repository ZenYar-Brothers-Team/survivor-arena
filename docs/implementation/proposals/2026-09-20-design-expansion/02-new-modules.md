# Пять новых IP-модулей

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующий план](../../README.md), очередь и статусы — только в [STATUS](../../STATUS.md). Этот файл не является текущим implementation packet.

Ревизия проекта `design-sync-R2`. Новые номера не зарегистрированы к исполнению. **Большинство новых требований реализуются обновлением существующих IP**, см. [карту 30 модулей](03-existing-modules-and-art.md) и [полные целевые спецификации](modules/).

| Полная спецификация | Самостоятельный scope | Почему отдельный IP |
|---|---|---|
| [IP-28](modules/IP-28-world-pickups.md) | World pickups | Lifecycle и reward dispatch для potion/Book; IP-06 остаётся XP-only. Fixtures проверяют framework; production potion — IP-20, Book — IP-30. |
| [IP-29](modules/IP-29-traveler-framework.md) | Traveler framework | Temporary encounters, три роли, seeded schedule, support cleanup, targetability, HP/arrows и Book reward. |
| [IP-30](modules/IP-30-production-travelers.md) | Production Travelers | 10 TRAVELER cards, их complete data/art/field bindings и production Book definition; не расширение обычных ENEMY IDs. |
| [IP-31](modules/IP-31-manual-run-telemetry.md) | Local playtest telemetry | Bounded recorder/report/export как потребитель gameplay events; mandatory Results не зависят от включения recorder. |
| [IP-32](modules/IP-32-manual-ai-balance.md) | Manual AI balance loop | Run/comments → конкретное AI предложение → approval → patch/check/retest; без auto-tuning/API/backend. |

Каждый packet содержит Context, hard dependencies, сохраняемую базу, Scope/Out of Scope, acceptance criteria, UI/observability, tests, документацию, gates и обратные dependency links. Ни один из этих файлов не хранит execution status.

## Что вернулось в существующие IP

| Требование | Владелец после переработки |
|---|---|
| Source-aware combat, target adapter, knockback/slow | IP-05; stats IP-03, passive mapping IP-09, enemy integration IP-13 |
| 3 slots, Book/level-up requests, очередь | IP-07; reroll/banish IP-10 |
| Set-priority/global chance и новые set effect families | IP-11 |
| Новые targeting/boomerang/deceleration/cumulative skill levels | IP-08 |
| Burst wave spawning | IP-14 |
| Reusable UI, HUD, draft/build details | IP-10A и feature-owned slices |
| Basic Settings и реальные preferences/services | IP-26 |
| Category importer, presentation adapters, asset inventory | IP-12A; production images у IP-17…23/IP-30 |
| Полная замена трёх design docs и регистрация плана | Этап M-01 в [05](05-migration-and-execution.md), не отдельный runtime IP |

## Условия новой работы

Утверждение пяти документов уже получено. Новые framework packets требуют только закрытия их действительных system gaps, не повторного approval механик. Полнота чисел, конкретные image gates и одобренный balance patch учитываются отдельно.

Новые модули стоят в общей [очереди](05-migration-and-execution.md), а не после завершения всех IP-00…27. IP-31/IP-32 идут сразу после адаптированного core/UI harness; IP-28/IP-29 — до полного UI и production catalogs; IP-30 — до production field encounters. Порядок не выводится из номера файла.
