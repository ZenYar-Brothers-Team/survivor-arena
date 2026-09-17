# IP-10 — Reroll и banish

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-10--reroll-и-banish).

## Цель

Игрок управляет случайностью level-up draft.

## Зависимости

IP-07.

## Scope

Reroll current offers; banish an offered entry for the current run and immediately rebuild the open draft; run-local configurable counters; eligibility integration; reset on new run. Invalid actions do not consume counters. If a successful reroll or banish leaves no eligible entries, the gained level remains, the pending draft resolves without another chooser, and the run does not remain paused.

## Context

- Game Design: «Опыт и level-up».
- Content Design: не требуется.

## UI / observability

Draft overlay показывает remaining counters, предоставляет reroll/banish actions и немедленно перерисовывает valid offers.

## Acceptance criteria

Reroll выдаёт новый valid draft и меняет состав предложений, когда в eligible pool существует альтернатива; successful reroll/banish with an exhausted pool closes the chooser without exception; banished entry не возвращается в run; exhausted counters reject actions without changing the draft; новый run reset.

## Проверки

Repeated drafts, alternative offer set, banish persistence, invalid/exhausted counter, no-options resolution, reset.

## Out of scope

Точные counts/recovery rules — balance TBD.
