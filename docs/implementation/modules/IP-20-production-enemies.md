# IP-20 — Production Enemies ENEMY-001…020

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-20--production-enemies).

## Цель

Реализовать 20 concrete enemy definitions.

## Зависимости

IP-04, IP-13; approval целевых ENEMY IDs.

## Scope

Approved ENEMY-001…020; HP/size/speed/contact damage/movement/ranged patterns/XP/context constraints.

## Context

- Game Design: «Враги, волны, элиты и боссы», «Управление, бой и выживание».
- Content Design: полные карточки целевых ENEMY IDs.

## UI / observability

Player-facing UI добавляется только для явно телеграфируемых attacks; development fixture identifies enemy ID/pattern для per-ID smoke.

## Acceptance criteria

Каждый enemy создаётся по ID и демонстрирует card behavior; XP reward корректен; field-context metadata доступно wave system.

## Проверки

Per-ID smoke matrix; ranged/melee/dash coverage.

## Out of scope

Canonical wave schedules and rebalancing.
