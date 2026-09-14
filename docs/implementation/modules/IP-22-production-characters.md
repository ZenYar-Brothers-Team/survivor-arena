# IP-22 — Production Characters CHAR-001…010

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-22--production-characters).

## Цель

Подключить десять playable characters.

## Зависимости

IP-12, IP-17; approval целевых CHAR IDs и required starting SKILL IDs.

## Scope

Approved CHAR-001…010; base stats; starting skill; recovery; draft weights; approved unlock metadata.

## Context

- Game Design: «Персонажи».
- Content Design: полные карточки целевых CHAR IDs и referenced starting SKILL cards.

## UI / observability

Каждый Approved character имеет selection presentation, starting-skill/base-stat summary и locked/unlocked state.

## Acceptance criteria

Каждый character starts with correct skill/stats; weights match card; 0/low-weight semantics correct; no unique passive system is added.

## Проверки

Per-character start loadout, seeded weighted draft, stat snapshot.

## Out of scope

Meta price tuning and unapproved unlock conditions.
