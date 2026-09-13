# IP-25 — Persistent profile, meta currency, unlocks и permanent progression

Оперативный статус и evidence хранятся только в [`STATUS.md`](../STATUS.md#ip-25--persistent-profile-и-meta-progression).

## Цель

Результат run сохраняется и меняет доступный контент/постоянные stats.

## Зависимости

IP-01, IP-12, IP-16. Production economy частично gated by Content Design/TBD.

## Scope

Versioned profile; meta currency; unlock state characters/fields/active skills/sets; purchase and condition hooks; win/loss rewards; global and per-character stat-upgrade framework.

## Context

- Game Design: «Мета-прогрессия».
- Content Design: approved unlock fields from CHAR/FIELD/SKILL/SET cards only; unrelated combat cards не читать.

## Acceptance criteria

Win/loss can award configured currency; purchase unlock and achievement unlock work; unlocked content persists; global/per-character upgrade hooks affect runtime stats.

## Проверки

Save/load, duplicate unlock, insufficient currency, version fallback.

## Out of scope

Придумывание final prices/rewards/upgrades/conditions. Если они не определены, используются explicit fixtures/config placeholders.
