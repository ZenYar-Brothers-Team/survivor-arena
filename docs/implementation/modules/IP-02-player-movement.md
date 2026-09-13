# IP-02 — Перемещение игрока и базовая геометрия поля

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-02--перемещение-игрока-и-базовая-геометрия-поля).

## Цель

Игрок управляет только перемещением персонажа и взаимодействует с коллизией окружения.

## Зависимости

IP-01.

## Scope

Movement input; movement speed stat hook; spawn point; field bounds; obstacle collision; pause integration.

## Context

- Game Design: «Управление, бой и выживание», «Поля».
- Content Design: schema-раздел «Fields»; FIELD-001…010 только как Draft compatibility targets геометрии, без production-импорта.

## Acceptance criteria

Движение работает во всех направлениях; configured obstacles блокируют проход; speed задаётся data/stat; pause останавливает движение.

## Проверки

Input, collision, speed modifier, pause.

## Out of scope

Конкретные десять field layouts, attacks, enemies.
