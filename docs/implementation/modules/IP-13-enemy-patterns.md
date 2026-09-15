# IP-13 — Enemy movement families, ranged projectiles и attack patterns

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-13--enemy-movement-и-attack-patterns).

## Цель

Enemy framework поддерживает разнообразные movement/attack patterns без отдельной уникальной core-логики на каждый тип.

## Зависимости

IP-03, IP-04.

## Scope

Keep-distance, arc/orbit approach, zigzag, approach-retreat, telegraphed dash; player-only ordinary field geometry (enemy pathfinding is not required); enemy projectile lifecycle; single/fan/burst/ring/cross/spiral patterns; large explosive projectile; configurable attack cooldown/damage.

## Context

- Game Design: «Управление, бой и выживание», «Враги, волны, элиты и боссы».
- Content Design: ENEMY-001…020 как Draft compatibility matrix; BOSS/MIDBOSS patterns можно использовать как non-binding stress cases.

## UI / observability

Telegraphs и projectile patterns читаемы в gameplay; development observability показывает fixture pattern/phase без production-only HUD.

## Acceptance criteria

Fixture definitions покрывают melee, ranged, dash и несколько projectile patterns; новые combination definitions не требуют изменения wave/draft systems; pause/end корректны.

## Проверки

Hit/miss/lifetime, pattern geometry, dash telegraph, multiple definitions.

## Out of scope

Production ENEMY IDs — IP-20; production bosses — IP-21.
