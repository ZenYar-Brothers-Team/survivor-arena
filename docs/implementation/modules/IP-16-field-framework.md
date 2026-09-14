# IP-16 — Field definitions, selection и run configuration

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-16--field-definitions-и-run-configuration).

## Цель

Один core run запускается на разных fields с разной geometry, enemy/wave/boss configuration.

## Зависимости

IP-02, IP-14, IP-15.

## Scope

Field definition; selectable unlocked field; player-only field bounds; environment reference; obstacle layout; enemy pool/wave schedule ref; final boss ref; optional mid-boss ref; one field initially available.

## Context

- Game Design: «Поля».
- Content Design: FIELD-001…010 как Draft compatibility targets; recommended BOSS/MIDBOSS mapping is Draft and non-binding until approved.

## Acceptance criteria

Два fixture fields загружают разные configuration; locked field unavailable; field-specific references validated by ID; same gameplay systems reused.

## Проверки

Selection, invalid ref, geometry config, locked state.

## Out of scope

Production FIELD-001…010 — IP-23; canonical wave bindings — IP-24.
