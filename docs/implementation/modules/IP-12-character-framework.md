# IP-12 — Character framework и weighted draft

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-12--character-framework-и-weighted-draft).

## Цель

Character selection меняет base stats, starting skill и draft weights.

## Зависимости

IP-07, IP-08, IP-09.

## Scope

Character definition; starting active skill in slot; base stats; disappearing-XP recovery; per-skill weights including zero; only unlocked selection.

## Context

- Game Design: «Персонажи», «Активные умения, пассивные предметы и слоты».
- Content Design: CHAR-001…010 как Draft compatibility targets; referenced starting SKILL IDs.

## UI / observability

Character selection fixture показывает unlocked characters, starting skill и краткий base-stat snapshot; weighted draft остаётся диагностируемым seeded tests/debug state.

## Acceptance criteria

Два fixtures дают разные stats/start skill/weights; 0 weight исключает random appearance; passive modifiers корректно накладываются на base stats.

## Проверки

Seeded weighted draft, starting loadout, locked character.

## Out of scope

Production CHAR-001…010 — IP-22.
