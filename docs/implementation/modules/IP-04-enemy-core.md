# IP-04 — Enemy core: lifecycle, melee movement и contact damage

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-04--enemy-core).

## Цель

Fixture-враги спавнятся, преследуют игрока, наносят contact damage, получают damage и умирают.

## Зависимости

IP-02, IP-03.

## Scope

Enemy definition/runtime; HP; size/collision; speed; spawn/despawn; базовый seek-player movement; contact damage; death event; простой continuous fixture spawner.

## Context

- Game Design: «Враги, волны, элиты и боссы», «Управление, бой и выживание».
- Content Design: schema «Enemies»; ENEMY-001…020 как Draft compatibility targets параметров и разных ролей.

## Acceptance criteria

Enemy создаётся из definition; двигается; contact hit повреждает игрока; enemy death корректно фиксируется; pause/end останавливает spawn/simulation.

## Проверки

Lifecycle, contact damage, despawn, pause.

## Out of scope

Production enemies, ranged attacks, waves.
