# IP-27 — End-to-end integration, regression и content validation

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-27--end-to-end-integration).

## Цель

Доказать совместимость core systems, selective-context pipeline и подключённого content catalog.

## Зависимости

IP-00…IP-26 в пределах реализуемого scope. Content-complete checks требуют approved content modules.

## Scope

Regression scenarios; configuration/reference validators; seeded reproducible runs; interactions slots/passives/sets/characters/fields/waves/boss/meta; balance/debug telemetry needed to inspect content behavior.

## Context

- Game Design: весь GDD разрешено читать для финальной coverage-проверки.
- Content Design: только Approved catalog + current Wave / Encounter Content; Draft entries не считаются missing implementation.

## UI / observability

End-to-end smoke проверяет все semantic UI contracts, отсутствие недоступных player actions и достаточную debug telemetry для воспроизводимого разбора run.

## Acceptance criteria

Full run works selection→15:00/result; win/loss; XP expiry/recovery; six-slot builds; sets; character weights; field config; boss timer rule; persistence; no feature requires Raw Design geometry zones.

## Проверки

Automated regression where practical + reproducible manual smoke checklist.

## Out of scope

Final balance, assets, Steam/release pipeline.
