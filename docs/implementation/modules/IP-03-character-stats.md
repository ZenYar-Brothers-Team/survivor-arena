# IP-03 — Character stats, HP, damage, healing и regeneration

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-03--character-stats-hp-damage-healing-и-regeneration).

## Цель

Runtime-персонаж имеет базовые характеристики, получает урон, лечится, регенерирует и умирает.

## Зависимости

IP-01, IP-02.

## Scope

Max/current HP; movement/damage/cooldown modifier hooks; incoming damage; heal; regen; death; disappearing-XP recovery stat placeholder.

## Context

- Game Design: «Управление, бой и выживание», «Опыт и level-up», «Персонажи».
- Content Design: schemas «Characters» и «Passive Items»; CHAR-001…010 и PASSIVE-001…010 только как Draft compatibility targets.

## UI / observability

HUD постоянно показывает current/max HP и немедленно отражает damage, healing, regeneration и изменение max HP. Development build предоставляет безопасные damage/heal actions.

## Acceptance criteria

Damage/heal/regen работают; HP capped max; death переводит run в lost; stats допускают base + modifiers без double counting.

## Проверки

Damage/heal/death, stat recompute, pause regeneration.

## Out of scope

Реальные characters/passives и их числа.
