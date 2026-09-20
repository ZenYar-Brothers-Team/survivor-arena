# IP-06 — XP lifecycle, effective pickup radius и progression

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Сохранить XP at death, pickup, expiry, recovery и queued level-up. Заменить чтение только BaseStats.PickupRadius на актуальный effective stat.

## Зависимости

[IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Опыт и level-up»; PASSIVE-006/007/010, Character XP schemas; PlayerExperienceRuntime/ExperienceDropRuntime/Factory, ExperienceProgression.

## Scope

XP-only pooled drops и pause-aware expiry; effective pickup radius для уже лежащих и новых drops; base vs awarded collected/expired/recovered counters/events для RunOutcome/telemetry. Level threshold/curve/lifetime остаются config. Non-XP pickups не прячутся в XP progression.

## Out of Scope

Book awards, potion health, draft selection, production XP balancing.

## Acceptance criteria

Death создаёт один drop с source life; expiry при recovery0 не даёт XP; pickup/recovery не удваиваются. Radius modifier немедленно действует на существующий drop. Multi-level award сохраняет thresholds и выдаёт ровно соответствующие level-up requests; pause/end freeze. Final current XP не подменяет lifetime awarded XP.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HUD XP/level; event diagnostics collected/expired/recovered. DEV add-XP intent помечается intervention для отчёта, если recorder включён.

## Проверки

Pickup/expiry/recovery including >0, modifiers, radius change before/after spawn, multiple thresholds, full lifecycle/pool reuse, pause/end; deterministic totals using synthetic drops.

## Документационные изменения

XP units/base-vs-award dictionary, fixture curve rationale; IP-07/IP-09/IP-31 use producer events.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-07](IP-07-level-up-draft.md), [IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
