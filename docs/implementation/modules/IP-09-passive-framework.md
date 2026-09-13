# IP-09 — Passive modifier framework

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-09--passive-modifier-framework).

## Цель

Passive items через один расширяемый mechanism изменяют stats/run parameters и имеют уровни 1…6.

## Зависимости

IP-03, IP-06, IP-07.

## Scope

Additive/multiplicative stat modifiers; max HP, regen, movement, active damage, cooldown, disappearing-XP recovery, XP lifetime, incoming damage, healing efficiency, picked-up XP; deterministic recompute.

## Context

- Game Design: «Опыт и level-up», «Активные умения, пассивные предметы и слоты», «Управление, бой и выживание».
- Content Design: PASSIVE-001…010 как Draft compatibility matrix.

## Acceptance criteria

Fixture passives изменяют несколько разных stat categories; level change пересчитывает результат; removing/rebuilding runtime state не double-counts effect; recovery/lifetime hooks работают.

## Проверки

Stacking, modifier order, 0%/>0% recovery, max level.

## Out of scope

Production PASSIVE-001…010 — IP-18.
