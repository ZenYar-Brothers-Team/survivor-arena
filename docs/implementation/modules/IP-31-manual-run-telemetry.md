# IP-31 — Локальная телеметрия ручных прогонов

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-03](IP-03-character-stats.md), [IP-04](IP-04-enemy-core.md), [IP-05](IP-05-active-skill-runtime.md), [IP-06](IP-06-xp-progression.md), [IP-07](IP-07-level-up-draft.md), [IP-08](IP-08-active-skill-framework.md), [IP-10](IP-10-reroll-banish.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

[контракт отчёта](../BALANCE_WORKFLOW.md); IP-27 telemetry scope; `RunModel`, `Health`, enemy/skill damage adapters, XP/draft runtime, wave/spawner; `PerfGuard`/`PerfLog`; UI §§16,21; `.claude/skills/balance-check/SKILL.md`, perf/test-quality/smoke skills.

## Scope

Optional development recorder потребляет RunOutcome IP-01, per-life events IP-04, source/applied-damage contract IP-05 и XP/draft producers IP-06/IP-07/IP-10. Versioned JSON report + readable summary + tester notes; config/build provenance, aggregate counters и bounded timeline; main-thread aggregation, export sink вне hot path, no-op/disabled release recorder. IP-31 не владеет mandatory player Results и не вводит combat mechanics. Boss/wave/set/Traveler/meta producers подключаются по мере готовности их owners, capabilities честно отражают coverage; полный encounter integration проверяет IP-27. Подробный формат — [balance loop](../BALANCE_WORKFLOW.md).

## Out of Scope

remote backend, automatic upload/LLM API, боты, полный deterministic replay, per-frame/per-hit unbounded combat log, научная значимость одной сессии, production analytics consent/UI.

## Acceptance criteria

один normal win, loss и aborted fixture run создают различимые отчёты; pause не входит в simulation DPS time; повторный end/export не удваивает counters/rewards; old spawn IDs не наследуются при pool reuse. 100 attempted damage в target с 10 HP записывает 10 applied, не 100 DPS damage. Отсутствующая attribution обозначена unknown, не выдумана. I/O failure не ломает run; bounded buffers отмечают dropped events; включение recorder не меняет RNG/игровые решения. Config hash и dirty state делают разные balance versions различимыми. Обязательные kills/time/level/build/sets в player Results поставляются producer→RunOutcome независимо от recorder; выключенный recorder не ломает игру или Results. Wave/sets/character details без готового adapter явно unsupported, не 0.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

компактная dev вкладка «Playtest»: recording/session ID, annotation marker, export/result/error; текстовый комментарий можно добавлять вне игры в companion file. IP-26 использует только compact Results summary, без обязательной analytics страницы или top-damage ranking до готовой attribution.

## Проверки

synthetic known outcomes/counter totals, overkill/heal/expiry/recovery, pause/end/abort, duplicate events, pool reuse, repeated init/rollback, snapshot/hash determinism, fake filesystem failure, no-op sink, bounded load/perf. Обязательно lethal hit → synchronous death/pool return до возврата applied amount, delayed projectile после despawn источника и его skill level-up: identity/level фиксируются до мутации. Release/no-recorder всё ещё имеет корректный RunOutcome. Один реальный manual run связывает report и комментарий. Synthetic tests не доказывают balance quality.

## Документационные изменения

Run-report schema/metric dictionary, local output/retention, capability/version coverage и performance evidence. IP-32 анализирует report; IP-27 проверяет full integration. Feature owners не зависят от IP-31 для gameplay: они публикуют собственные факты, recorder лишь потребитель.

## Gates и недостающие решения

Нет зависимости от production approval, полного art/UI/meta или новых encounters. Capabilities явно ограничены поставленными producers; diagnostic config budgets фиксируются до implementation. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md), [IP-32](IP-32-manual-ai-balance.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
