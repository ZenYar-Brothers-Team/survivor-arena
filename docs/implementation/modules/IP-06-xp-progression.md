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

IP-31 observability extension: `DroppedBase` увеличивается только при новой регистрации drop, `GroundBase` уменьшается при снятии зарегистрированного drop. Reset — при Initialize; cleanup не начисляет XP. Collector получает эти totals отдельно от collected/expired/recovered awards, без per-frame scan. [Metric dictionary](../PLAYTEST_REPORT.md).

XP units/base-vs-award dictionary, fixture curve rationale; IP-07/IP-09/IP-31 use producer events.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-07](IP-07-level-up-draft.md), [IP-09](IP-09-passive-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-27](IP-27-integration.md), [IP-28](IP-28-world-pickups.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Контракт XP units / producer

Все количества — XP units, все радиусы — world units. `CollectedBase` — сумма номиналов физически подобранных drops; `CollectedAwarded = Σ(base × effective picked-up multiplier на pickup)`. `ExpiredBase` — сумма исчезнувших номиналов; `RecoveredAwarded = Σ(expired base × effective recovery на expiry)` без picked-up multiplier. `InterventionBase/Awarded` — DEV add-XP отдельно. `TotalAwarded = CollectedAwarded + RecoveredAwarded + InterventionAwarded`, а `Progression.CurrentExperience` — остаток внутри текущего уровня. Например, pickup 10 при x1.2 даёт 12 XP; expiry 8 при recovery0.5 даёт 4 XP; total16 независимо от потраченных на levels thresholds.

`ExperienceAwardEvent` предоставляет immutable run ID, nullable drop/source identity, origin и base/award; zero-recovery expiry тоже публикуется. `ExperienceDropIdentity.LifeId` меняется при каждом rent; source life/content не читаются из переиспользованного EnemyRuntime. `EnemyExperienceDropSink` deduplicates повторную доставку одного death life.

`PlayerExperienceRuntime` владеет contributor `experience` и drop pool. `Capture()` копирует level, current-level remainder и lifetime totals в `RunOutcomeContribution`. Shutdown снимает contributor/level subscriptions и убирает active/inactive drops без pickup/recovery reward. Конкретные queue decisions принадлежат IP-07, export — IP-31; DEV intent обозначен DevelopmentIntervention даже без recorder.

Fixture thresholds и lifetime остаются в `Resources/Content/Run/FixtureRunSetup.json`: короткие ранние thresholds позволяют быстро проверить level-up/pause/draft в ручном smoke; это не production XP curve. Последний threshold повторяется. Полное начисление рассчитывается до публикации level-up событий, чтобы result snapshot не терял остаток или часть award.

Детали cross-layer реализации: [DECISION-0018](../../decisions/0018-experience-accounting.md), Proposed для архитектурного ревью; новых product rules и production tuning не вводит.
