# DECISION-0023 — Optional local playtest recorder

Status: Proposed

Date: 2026-09-21

Related IP: IP-01, IP-04, IP-06, IP-07, IP-10, IP-10A, IP-31, IP-32

## Context

IP-31 требует local report без превращения telemetry в gameplay dependency. Lethal damage синхронно завершает run/возвращает врага в pool до возврата CombatResult. Диагностика должна сохранить result без повторения rewards/RNG. Required Results работают без recorder.

## Implementation record

Game.Telemetry зависит от features, Game.Diagnostics остаётся без gameplay references. Bootstrap подключает PlaytestSession после feature owners, перед UI, только в Editor/development. Rollback удаляет подписки. Main-thread bounded aggregation финализируется на LateUpdate/teardown boundary; immutable payload отправляется worker sink. I/O failure не меняет run.

Feature-owned extensions: spawner relays CombatResolved/LifeEvent; XP tracks dropped/ground totals; draft publishes queue/control attempts; run exposes pause ownership transitions. RunOutcomeContribution получает nullable Sets snapshot. Producers не ссылаются на recorder/UI. Existing damage pipeline не заменяется.

Composition root также владеет полным Shutdown: capture run → UI unsubscribe → diagnostic final snapshot → spawner/passive/skill/draft/XP/presentation → character. Повторный Shutdown ничего не делает. Это требуется для terminal export и устраняет обнаруженный scene-reload дефект: UI и пассивы обращались к уже очищенным Health/Stats.

## Consequences

Повторный export сохраняет feedback; non-final packet помечен incomplete. Cached catalog хранит исходный JSON рядом с definitions; commit/dirty unknown допустимы при недоступном metadata. Формат/coverage — [report contract](../implementation/PLAYTEST_REPORT.md).

Это техническая межслойная запись для architecture review, не новое product approval. Gameplay rules и balance numbers не менялись; автоматического разрешения на AI patches нет.
