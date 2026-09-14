# IP-10 — Reroll и banish

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-10--reroll-и-banish).

## Цель

Игрок управляет случайностью level-up draft.

## Зависимости

IP-07.

## Scope

Reroll current offers; banish an offered entry for the current run and immediately rebuild the open draft; run-local configurable counters; eligibility integration; reset on new run. Invalid actions do not consume counters. If banish exhausts all eligible entries, the pending draft resolves without leaving the run permanently paused.

## Context

- Game Design: «Опыт и level-up».
- Content Design: не требуется.

## Acceptance criteria

Reroll выдаёт новый valid draft и меняет состав предложений, когда в eligible pool существует альтернатива; banished entry не возвращается в run; exhausted counters reject actions without changing the draft; новый run reset.

## Проверки

Repeated drafts, alternative offer set, banish persistence, invalid/exhausted counter, no-options resolution, reset.

## Out of scope

Точные counts/recovery rules — balance TBD.
