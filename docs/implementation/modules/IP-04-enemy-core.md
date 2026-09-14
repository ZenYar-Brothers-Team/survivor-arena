# IP-04 — Enemy core: lifecycle, melee movement и contact damage

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-04--enemy-core).

## Цель

Fixture-враги спавнятся, преследуют игрока, наносят contact damage, получают damage и умирают.

## Зависимости

IP-02, IP-03.

## Scope

Enemy definition/runtime; HP; size/collision; speed; spawn/despawn; базовый seek-player movement; contact damage при входе в контакт и повторными тиками через configured interval; death event; простой continuous fixture spawner.

## Context

- Game Design: «Враги, волны, элиты и боссы», «Управление, бой и выживание».
- Content Design: schema «Enemies»; ENEMY-001…020 как Draft compatibility targets параметров и разных ролей.

## Acceptance criteria

Enemy создаётся из definition; двигается и свободно пересекает границу поля и обычные препятствия; contact hit сразу повреждает игрока и повторяется по configured interval, поэтому долгий контакт с наносящим урон врагом может убить персонажа; enemy death корректно фиксируется; pause/end останавливает spawn/simulation/contact ticks.

## Проверки

Lifecycle, field-bound pass-through, immediate/repeated contact damage, sustained-contact death, despawn, pause.

## Out of scope

Production enemies, ranged attacks, waves.
