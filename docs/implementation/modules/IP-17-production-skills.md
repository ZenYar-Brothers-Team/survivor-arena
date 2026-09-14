# IP-17 — Production Active Skills SKILL-001…015

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-17--production-active-skills).

## Цель

Реализовать канонический active-skill catalog без копирования его полного описания в IP.

## Зависимости

IP-08; IP-13, если переиспользуются projectile/pattern primitives; approval целевых SKILL IDs.

## Scope

Только Approved SKILL-001…015; все levels 1…6 и interactions из Content Design; data/config values remain editable.

## Context

- Game Design: «Активные умения, пассивные предметы и слоты», «Управление, бой и выживание».
- Content Design: полные карточки целевых SKILL IDs и только связанные passive/set references, если они влияют на contract.

## UI / observability

Каждый Approved skill имеет production label/icon reference, draft presentation и build-slot state; отсутствующий presentation asset обнаруживается validator.

## Acceptance criteria

Каждый Approved skill воспроизводит card behavior на L1…L6; есть automated/manual smoke coverage по каждому ID; manual attack input не вводится.

## Проверки

Per-ID behavior matrix, level transitions, coexistence of 6 active skills.

## Out of scope

Rebalance или изменение Draft design; такие изменения возвращаются в Content Design.
