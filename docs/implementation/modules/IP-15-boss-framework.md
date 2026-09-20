# IP-15 — Boss/mid-boss encounter framework

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-05](IP-05-active-skill-runtime.md), [IP-08](IP-08-active-skill-framework.md), [IP-13](IP-13-enemy-patterns.md), [IP-14](IP-14-wave-director.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD «Структура забега и условия завершения», «Враги, волны, элиты и боссы»; schemas и нужные attack/phase families BOSS-001…010/MIDBOSS-001…010; UI §§13–15; DECISION-0014 hooks и DECISION-0013 presentation boundary.

## Scope

configured phase thresholds и composition существующих attacks; optional midboss и обязательный final-boss hook; death/despawn/end distinction; final boss name/HP на HUD, читаемая preparation/telegraph. Если конкретная карточка требует ещё неописанный shield/support/phase rule, этот пробел выделяется до её реализации.

## Out of Scope

production BOSS/MIDBOSS definitions/assets (IP-21), самостоятельно назначенное время final spawn, обязательность убийства босса для victory.

## Acceptance criteria

hooks срабатывают однократно; boss alive/dead не меняет timer-based victory; crossing нескольких HP thresholds разрешается по определённому контракту; pause не продвигает attacks/phases. HP bar показан только пока final boss активен и убирается при death/despawn/end; обычный HUD/timer остаётся. Midboss diagnostic phase не превращается автоматически в второй global boss bar. Все используемые player skill patterns могут обнаружить/повредить boss через IP-05 target contract; final HP bar cleanup не зависит от dev telemetry.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

final HP/name + короткое boss incoming уведомление, optional midboss feedback по его роли; phase/attack/source IDs доступны в DEV/telemetry hooks. Cutscene не требуется.

## Проверки

thresholds/boundaries, boss killed/alive at timer, player death, multiple hooks, pause/end, stale subscriptions; PlayMode bar/telegraph visibility и freeze.

## Документационные изменения

boss schema, phase/telegraph ownership, missing-rule list по нужным карточкам, event contract для IP-31; final spawn times остаются encounter data IP-24.

## Gates и недостающие решения

G-07 закрыт DECISION-0017. G-14: используемые phase/attack fields; final timing configurable, production values не обязательны для synthetic framework. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-16](IP-16-field-framework.md), [IP-21](IP-21-production-bosses.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
