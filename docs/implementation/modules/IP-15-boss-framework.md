# IP-15 — Boss/mid-boss encounter framework и final phase

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-15--bossmid-boss-framework).

## Цель

Field/run может запускать optional mid-boss и обязательного final boss shortly before run end; boss death не является victory.

## Зависимости

IP-01, IP-13, IP-14.

## Scope

Boss entity definition; phases by HP thresholds; movement/attack pattern composition; mid-boss hook; final-boss spawn hook; boss death; coexistence with timer-based run end.

## Context

- Game Design: «Структура забега и условия завершения», «Враги, волны, элиты и боссы».
- Content Design: BOSS-001…010 и MIDBOSS-001…010 как Draft compatibility targets.

## Acceptance criteria

Fixture mid-boss optional; fixture final boss spawns by configured time; phase transition changes behavior; killing final boss does not set won; alive at 15:00 does.

## Проверки

Boss alive/killed at timer, player death, phase threshold, pause.

## Out of scope

Production bosses — IP-21; exact final spawn time remains config/TBD.
