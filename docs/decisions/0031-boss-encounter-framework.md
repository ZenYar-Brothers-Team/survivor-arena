# DECISION-0031 — Boss encounter ownership и synthetic phases

Status: Proposed

Date: 2026-09-21

Related IP: IP-05, IP-13, IP-14, IP-15, IP-16, IP-21, IP-31

## Context

Пользователь поручил следующий пункт после IP-14. IP-15 требует конфигурируемых
HP phases, композиции существующих attacks, uncapped boss hooks и final HUD.
Production phase payload/rewards остаются G-14; timer victory уже утверждён GDD.

## Implementation record

BossEncounterRuntime подписывается на wave hooks и bound RunModel; владеет
отдельными enemy/projectile pools. EnemyRuntime остаётся единым target и combat
adapter для ordinary/boss/Traveler. BossCombatController задаёт optional attack
sequence поверх EnemyAttackController; movement/contact ownership не переносится.

Фазы — strictly descending health fractions; highest reached phase wins once,
healing не откатывает, death приоритетнее. На переходе pending attack отменяется,
следующая атака получает полный wind-up; emitted projectiles не переписываются.
Это явный synthetic framework contract из acceptance IP-15, не утверждение
неполного production phase payload. Формулы, пример и boundaries — в
[IP-15](../implementation/modules/IP-15-boss-framework.md#fixture-schema-и-phase-contract).

Final HUD читает producer напрямую через immutable ViewState; dev/telemetry
не владеют visibility или cleanup. Phase/life/combat events содержат immutable
attribution для optional IP-31 consumer. Shutdown разрывает старые подписки;
новый director означает новое расписание, повторный hook старого не создаёт life.

## Limits

Техническая межслойная запись для review. Product approvals, BOSS-/MIDBOSS-
definitions/assets и GDD/CD не изменены. Shield/support/escape, post-dash attacks
и production repeat payload требуют соответствующих owning packets. Никакие
fixture numbers не назначают production баланс.
