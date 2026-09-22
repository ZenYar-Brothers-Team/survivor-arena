# IP-04 — Enemy lifecycle, contact damage и per-life identity

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-04-enemy-core.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Сохранить seek/contact/death/pooling/registry. Расширить authoritative lifecycle events без переноса drop/economy/export логики в EnemyRuntime.

## Зависимости

[IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Враги…», «Управление, бой и выживание»; Enemies schema; DECISION-0002/0003/0011; EnemyRuntime/Factory/Registry, Health; relevant TD-001/TD-003 только как проверка затронутых границ.

## Scope

Explicit per-spawn life identity, content ID, spawn/death/despawn reasons; immediate/repeating contact hits; reusable target/lifecycle adapter contract для IP-05. Death event несёт immutable position/source/life data для XP/drop/summary consumers. Ordinary category distinct from boss/Traveler. Жизненный цикл не зависит от конкретной реализации pickups.

## Out of Scope

Boss/Traveler behavior, production tuning, world-pickup policy, structured telemetry file writer.

## Acceptance criteria

Один life даёт максимум одно death event; cleanup/escape/despawn не kill. Pool rent даёт новую life identity, health/status/registry baseline. Контакт pause/end safe. Subscriber может забрать death payload после pool return без чтения изменившейся entity. Enemy passes ordinary field bounds.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HP effect виден через existing HUD, ID/life/reason в fixtures/DEV; необязательная постоянная полоса обычного enemy не вводится.

## Проверки

Spawn/register/death/unregister/reuse, sustained contact, duplicate damage/death, synchronous lethal callback и teardown; registry baseline restored. No reentrant mutation shared AoE buffers.

## Документационные изменения

Lifecycle/event contract и зависимости IP-05/06/13/28/31; mitigation для реально затронутого coupling, не blanket cleanup всего debt register.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-05](IP-05-active-skill-runtime.md), [IP-06](IP-06-xp-progression.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-13](IP-13-enemy-patterns.md), [IP-14](IP-14-wave-director.md), [IP-20](IP-20-production-enemies.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
