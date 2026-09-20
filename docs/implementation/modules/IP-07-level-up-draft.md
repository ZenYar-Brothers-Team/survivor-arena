# IP-07 — Level-up draft, 6+6 slots и base build progression

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-07--level-up-draft-66-slots-и-base-build-progression).

## Цель

Level-up ставит gameplay на pause и позволяет выбрать новый или улучшить существующий build entry.

## Зависимости

IP-01, IP-06.

## Scope

Unified active/passive draft pool; eligibility; 6 active + 6 passive slots; starting active slot; levels 1…6; no replacement; minimal functional chooser; resume after selection; preserve the gained level and skip the chooser when no eligible acquisition or upgrade remains.

## Context

- Game Design: «Опыт и level-up», «Активные умения, пассивные предметы и слоты».
- Content Design: schemas «Active Skills», «Passive Items»; реальные IDs не требуются до IP-17/IP-18.

## UI / observability

Level-up overlay показывает предложения и результат new/upgrade; выбор обновляет build panel и закрывает overlay после применения.

## Acceptance criteria

Valid draft opens on level-up; selected new item occupies correct slot; upgrade increases level; full slot blocks new item of that type; max-level entry not offered; choice resumes run; an exhausted pool consumes queued drafts without opening an empty window or leaving the run paused.

## Проверки

Fill 6+6, upgrade 1→6, invalid eligibility, pause/resume, repeated level-ups after the pool is exhausted.

## Out of scope

Reroll, banish, sets, weighted character pool.
