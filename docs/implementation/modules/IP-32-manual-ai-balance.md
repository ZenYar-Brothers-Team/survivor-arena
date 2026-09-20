# IP-32 — Ручные прогоны и AI-assisted balance review

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-31](IP-31-manual-run-telemetry.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

[04-balance-loop](../BALANCE_WORKFLOW.md); canonical Game/Content sections и JSON только выбранного balance domain; `.claude/skills/balance-check`, consistency/content audit, smoke-check, design rules.

## Scope

checklist ручного сценария, шаблоны tester notes, AI analysis и change proposal; before/after data diff с JSON paths/content IDs/units; evidence/confidence; утверждение выбранного предложения; проверки и повторные comparable runs; rollback. Первое применение ограничить одним согласованным hypothesis batch. Механические предложения проходят design/DECISION маршрут, затем отдельную реализацию соответствующего IP.

## Out of Scope

auto-tuning, automatic balance deploy, Google Sheets как новый source of truth, оптимизация под AI player, финальный баланс всего каталога.

## Acceptance criteria

trace report IDs/config hashes → observation → hypothesis → exact proposed values/rule → explicit approval → applied diff → checks → follow-up observations. AI может ответить «данных недостаточно». Нет автоматического принятия suggestion или target win rate. Fixture results помечены fixture; нет ложного сравнения разных fields/meta profiles/test conditions. Rejected/deferred proposals не попадают в JSON.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

player UI не нужен; reviewable Markdown packet и readable diff; в игре только IP-31 instrumentation.

## Проверки

пройти один цикл на выбранном fixture hypothesis; reject/no-change/insufficient-evidence — допустимые реальные результаты. Accept/apply/rollback путь проверить на явно synthetic dry run либо первом действительно обоснованном одобренном patch; не менять баланс только ради завершения workflow. Пересчитать arithmetic examples; отличать estimate от measured damage.

## Документационные изменения

docs/balance workflow/templates и evidence владельца одобренного изменения. .claude balance-check остаётся read-only; optional skill edits — только по отдельному scope. Любой production IP может использовать workflow без mandatory dependency на завершение баланса всего каталога.

## Gates и недостающие решения

BG-01 для применения конкретных чисел/механик; отсутствие product target отмечается, а не заполняется AI. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
