# IP-23 — Production Fields FIELD-001…010

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-23--production-fields).

## Цель

Подключить десять field definitions and geometry/environment configuration.

## Зависимости

IP-16; approved enemy/boss content as required; approval целевых FIELD IDs.

## Scope

Approved FIELD-001…010; player-only field bounds and ordinary obstacles; geometry/obstacles; environment refs/placeholders; enemy profile metadata; unlock metadata; boss/midboss refs only when approved.

## Context

- Game Design: «Поля».
- Content Design: полные карточки целевых FIELD IDs и только referenced ENEMY/BOSS/MIDBOSS IDs.

## Acceptance criteria

Все Approved fields selectable/loadable; geometry differences apply; invalid refs rejected; difficulty ordering metadata preserved without hard-coding field number into systems.

## Проверки

Load each field config, obstacle smoke, ref validation.

## Out of scope

Canonical per-minute wave schedule, если он ещё не определён.
