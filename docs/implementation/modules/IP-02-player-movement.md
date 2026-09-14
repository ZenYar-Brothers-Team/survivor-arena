# IP-02 — Перемещение игрока и базовая геометрия поля

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-02--перемещение-игрока-и-базовая-геометрия-поля).

## Цель

Игрок управляет только перемещением персонажа, взаимодействует с коллизией окружения и при движении остаётся в центре экрана.

## Зависимости

IP-01.

## Scope

Movement input; movement speed stat hook; spawn point; player-only field bounds and ordinary obstacle collision; pause integration; camera follow без задержки и screen-space offset.

## Context

- Game Design: «Управление, бой и выживание», «Поля».
- Content Design: schema-раздел «Fields»; FIELD-001…010 только как Draft compatibility targets геометрии, без production-импорта.

## UI / observability

Отдельный production HUD не требуется; fixture и PlayMode checks должны позволять проверить ввод, положение игрока, camera centering и player-only collision.

## Acceptance criteria

Движение работает во всех направлениях; field bounds и configured ordinary obstacles блокируют игрока и проходимы для остальных игровых сущностей; speed задаётся data/stat; pause останавливает движение; камера сохраняет depth и удерживает world position персонажа в центре viewport при его перемещении.

## Проверки

Input, player-only field-bound collision, obstacle collision, speed modifier, pause, camera follow wiring, viewport center after player displacement.

## Out of scope

Конкретные десять field layouts, attacks, enemies.
