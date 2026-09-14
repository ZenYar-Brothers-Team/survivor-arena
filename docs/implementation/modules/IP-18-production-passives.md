# IP-18 — Production Passive Items PASSIVE-001…010

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-18--production-passive-items).

## Цель

Подключить конкретный passive catalog через общий modifier framework.

## Зависимости

IP-09; approval соответствующих PASSIVE IDs.

## Scope

Approved PASSIVE-001…010; six level values; interactions exactly from Content Design.

## Context

- Game Design: «Опыт и level-up», «Управление, бой и выживание», «Активные умения, пассивные предметы и слоты».
- Content Design: полные карточки целевых PASSIVE IDs.

## UI / observability

Каждый Approved passive имеет production label/icon reference, draft presentation и build-slot state; level/effect text берётся из content data.

## Acceptance criteria

Каждый Approved passive даёт правильный level-specific effect; modifiers compose deterministically; XP/heal/cooldown/damage interactions match cards.

## Проверки

Per-ID matrix, stacking, 1→6.

## Out of scope

Balance redesign.
