# IP-14 — Wave Director и threat-profile timeline

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-14--wave-director).

## Цель

Continuous spawn управляется последовательностью ordinary/pressure/hard-or-elite/rest phases.

## Зависимости

IP-04, IP-13.

## Scope

Wave timeline; enemy pool/composition; spawn intensity; per-wave overrides; phase tags; overall increasing difficulty without monotonicity of every stat; hooks for mid-boss/final phase. The existing live-enemy registry is the query source; production-density spawning must introduce pooled enemy lifecycle instead of global scene scans or repeated instantiate/destroy churn.

## Context

- Game Design: «Структура забега и условия завершения», «Враги, волны, элиты и боссы».
- Content Design: «Fields» enemy profiles; «Wave / Encounter Content» — сейчас пустой content gate; ENEMY IDs только для fixtures/compatibility до approval.

## Acceptance criteria

Debug timeline проходит несколько phases; composition/intensity changes; hard phase and respite visibly differ; later wave may trade speed/HP/damage rather than only scale all upward; pooled enemies register on activation and unregister on release without stale targets.

## Проверки

Transitions, config validation, pause/end, pooled reuse and registry cleanup under repeated spawn/despawn.

## Out of scope

Canonical 15-minute schedules — IP-24 после заполнения Wave / Encounter Content.
