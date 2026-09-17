# IP-05 — Active skill runtime и player damage pipeline

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-05--active-skill-runtime-и-player-damage-pipeline).

## Цель

Персонаж автоматически применяет active skill без ручной кнопки и может убивать врагов.

## Зависимости

IP-03, IP-04.

## Scope

Active-skill definition/runtime; auto trigger; cooldown; target/direction hook; player-origin damage; минимальные projectile/AoE primitives, достаточные для vertical slice; один fixture skill.

## Context

- Game Design: «Управление, бой и выживание», «Активные умения, пассивные предметы и слоты».
- Content Design: schema «Active Skills»; SKILL-001…015 читать только для понимания разнообразия patterns, не как approved content.

## UI / observability

Starting active skill и его уровень видны в build panel; auto-trigger остаётся наблюдаемым в gameplay без attack input.

## Acceptance criteria

Fixture skill auto-fires; не требует aim/attack input; damage проходит через единый enemy damage contract; pause/end останавливают trigger/projectiles.

## Проверки

Cooldown, target hook, hit/kill, pause.

## Out of scope

Все 15 skills, 6 уровней, sets.
