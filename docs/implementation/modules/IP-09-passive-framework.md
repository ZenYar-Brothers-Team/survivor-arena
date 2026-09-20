# IP-09 — Passive modifier framework

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-09--passive-modifier-framework).

## Цель

Passive items через один расширяемый mechanism изменяют stats/run parameters и имеют уровни 1…6.

## Зависимости

IP-03, IP-06, IP-07.

## Scope

Keyed additive flat/percentage modifiers; additive percentage composition between sources; max HP, regen, movement, active damage, cooldown reduction through `base / (1 + reduction)`, disappearing-XP recovery, XP lifetime, incoming damage reduction capped at 99%, healing efficiency, picked-up XP; deterministic recompute; proportional current-HP rescale when max HP changes.

## Context

- Game Design: «Опыт и level-up», «Активные умения, пассивные предметы и слоты», «Управление, бой и выживание».
- Content Design: PASSIVE-001…010 как Draft compatibility matrix.

## UI / observability

Build panel показывает все acquired passives и уровни в шести стабильных слотах; изменившиеся HP/XP-related stats проявляются в соответствующем HUD.

## Acceptance criteria

Fixture passives изменяют несколько разных stat categories; level change пересчитывает результат; removing/rebuilding runtime state не double-counts effect; percentage sources stack additively; positive cooldown stays above zero without a hard minimum; incoming damage reduction is capped at 99%; max-HP changes preserve current health ratio; recovery/lifetime hooks работают.

## Проверки

Stacking, keyed replacement/removal, cooldown asymptote, 99% incoming-damage-reduction cap, proportional HP rescale, 0%/>0% recovery, max level.

## Out of scope

Production PASSIVE-001…010 — IP-18.
