# IP-06 — XP drops, pickup, expiry и level progression

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-06--xp-drops-pickup-expiry-и-level-progression).

## Цель

Убитые враги оставляют физический XP, который можно собрать до исчезновения и получить level-up.

## Зависимости

IP-04, IP-05.

## Scope

XP drop at death position; pickup; XP bar; level threshold; expiry timer; base recovery 0%; recovery hook; level-up event.

## Context

- Game Design: «Опыт и level-up», «Ключевые параметры текущей версии».
- Content Design: PASSIVE-006, PASSIVE-007, PASSIVE-010 и CHAR-001…010 только как Draft compatibility targets для будущих XP modifiers.

## Acceptance criteria

XP появляется после death; pickup начисляет XP; expired drop удаляется; recovery=0 не начисляет его; threshold создаёт level-up; lifetime конфигурируем.

## Проверки

Pickup, expiry, multiple levels, pause.

## Out of scope

Draft UI, конкретные passive modifiers, финальная XP curve.
