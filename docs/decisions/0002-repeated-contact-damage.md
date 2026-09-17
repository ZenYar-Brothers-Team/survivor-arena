# DECISION-0002 — Повторный урон при длительном контакте

Status: Approved

Date: 2026-09-13

Related IP: IP-04

Related content IDs: ENEMY-001…020 как Draft compatibility targets

## Context

Первая реализация IP-04 наносила contact damage только один раз при `OnCollisionEnter2D`. Непрерывное соприкосновение с врагом не наносило новый урон и само по себе не могло привести к смерти персонажа.

## Decision

Враг наносит contact damage сразу при входе в контакт, затем повторяет урон через настраиваемый `contactDamageInterval`, пока контакт сохраняется. Пауза и состояния `Won`/`Lost` не начисляют время и не наносят contact damage. При выходе из контакта таймер сбрасывается.

## Consequences

- Game Design фиксирует общее правило длительного contact damage.
- IP-04 включает repeated ticks и sustained-contact death в acceptance criteria и checks.
- `EnemyDefinition` получает обязательный положительный `contactDamageInterval`.
- Точные интервалы production-врагов остаются content/balance-data и не утверждаются этим решением.

## Approval

Пользователь явно утвердил уточнение 2026-09-13 сообщением: «да, сделай это уточнение и реализуй».
