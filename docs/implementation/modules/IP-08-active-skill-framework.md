# IP-08 — Расширяемая active-skill progression и pattern framework

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-08--active-skill-progression-и-pattern-framework).

## Цель

Framework поддерживает 6-level active skills, включая qualitative behavior changes.

## Зависимости

IP-05, IP-07.

## Scope

Level-specific parameters; behavior composition/hooks; projectile count/pierce/fan/ring/four-ray diagonal cross; beam/tick; orbit; boomerang/return; chain/retarget; delayed AoE; mine/lifetime/explosion; multi-wave activation — без hard-code под один skill.

## Context

- Game Design: «Активные умения, пассивные предметы и слоты».
- Content Design: SKILL-001…015 как Draft compatibility matrix; сами значения не production.

## UI / observability

Build panel показывает все acquired active skills и уровни в шести стабильных слотах; qualitative pattern проверяется fixture gameplay и tests.

## Acceptance criteria

Несколько fixture skills демонстрируют разные behavior families; переходы уровней могут менять и числа, и pattern; draft/slot logic остаётся общей.

## Проверки

Representative patterns, distinct ring/diagonal-cross geometry, 1→6, max level, concurrent skills.

## Out of scope

Production implementation SKILL-001…015 — это IP-17.
