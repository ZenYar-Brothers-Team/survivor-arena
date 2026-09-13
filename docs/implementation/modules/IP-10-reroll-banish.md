# IP-10 — Reroll и banish

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-10--reroll-и-banish).

## Цель

Игрок управляет случайностью level-up draft.

## Зависимости

IP-07.

## Scope

Reroll current offers; banish entry for current run; run-local counters; eligibility integration; reset on new run.

## Context

- Game Design: «Опыт и level-up».
- Content Design: не требуется.

## Acceptance criteria

Reroll выдаёт новый valid draft; banished entry не возвращается в run; counters работают; новый run reset.

## Проверки

Repeated drafts, exhausted counter, reset.

## Out of scope

Точные counts/recovery rules — balance TBD.
