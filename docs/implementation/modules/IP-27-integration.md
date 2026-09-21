# IP-27 — End-to-end integration, regression и content validation

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-00](IP-00-content-contract.md), [IP-01](IP-01-run-lifecycle.md), [IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md), [IP-09](IP-09-passive-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md), [IP-13](IP-13-enemy-patterns.md), [IP-14](IP-14-wave-director.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-17](IP-17-production-skills.md), [IP-18](IP-18-production-passives.md), [IP-19](IP-19-production-sets.md), [IP-20](IP-20-production-enemies.md), [IP-21](IP-21-production-bosses.md), [IP-22](IP-22-production-characters.md), [IP-23](IP-23-production-fields.md), [IP-24](IP-24-production-waves.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-28](IP-28-world-pickups.md), [IP-29](IP-29-traveler-framework.md), [IP-30](IP-30-production-travelers.md), [IP-31](IP-31-manual-run-telemetry.md), [IP-32](IP-32-manual-ai-balance.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD целиком для coverage; новые UI/Art documents по acceptance; все in-scope content cards/encounters, decisions и approved balance revisions. Старые design документы используются только как migration checklist сохранённых инвариантов.

## Scope

full-run/retry/profile flow, новые combat/active/set effects, Book/pickup/Traveler и burst interactions; config/assets/refs validation; reproducible manual→AI review packet и проверка approval/apply/retest/rollback пути на synthetic rehearsal либо реальном обоснованном одобренном patch; combined readability/performance. Реальный анализ может закончиться no-change/insufficient-evidence; tuning не нужен только ради галочки. Тестовые измерения не являются автоматическим доказательством финального баланса.

## Out of Scope

окончательно оптимальный баланс, маркетинг/Steam/release packaging, объявление полного каталога реализованным по representative smoke.

## Acceptance criteria

selection→15:00/win и defeat→results/retry работают; boss death не завершает run; 6+6 и несколько sets не ломают draft/pause. Все 121 утверждённые target cards, дополнительно заполненная Book card, schedules всех 10 полей и принятые economy definitions имеют требуемое behavior/assets/tests evidence. Representative fixture integration — промежуточная проверка; она не закрывает этот полный scope. Player/danger/pickup hierarchy читается, DEV не закрывает gameplay. Отчёт включает build/content version, run ID, seed/config и пометки вмешательства; он не смешивает fixture и production outcomes.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

full semantic UI contracts, export/save-error feedback telemetry, latest run summary; release build не показывает test controls/log internals.

## Проверки

relevant automated suites, validated imports/references, real 15-minute manual run и targeted density/boss/Traveler/multi-set cases, retry/persistence, metrics sanity и balance diff traceability. Hardware/build и measured budgets записаны рядом с performance evidence.

## Документационные изменения

coverage matrix и manual checklist, evidence по новым требованиям в STATUS; согласованные regression-map/debt updates проходят правила соответствующих helpers. Непройденные manual art gates указаны отдельно от автоматических тестов.

## Gates и недостающие решения

Только реальные missing required contracts/data/asset checks полного scope этого плана. Уменьшение каталога возможно лишь как отдельное явное изменение плана; один smoke не закрывает content-complete verification. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

Прямых модульных потребителей нет; результаты завершают план. Статус и evidence остаются в STATUS.

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.