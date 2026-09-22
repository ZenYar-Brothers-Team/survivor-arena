# IP-32 — Ручные прогоны и AI-assisted balance review

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Новая самостоятельная возможность; в действующем плане нет отдельного владельца этого lifecycle/process. Использовать существующие подсистемы через перечисленные dependencies.

## Зависимости

[IP-31](IP-31-manual-run-telemetry.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
zero-meta критерии, seeds, tuning bounds и измеримые performance targets.
Это Proposed packet по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
использовать как production data только после approval, затем выполнить проверки этого IP.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

[BALANCE_WORKFLOW](../BALANCE_WORKFLOW.md); canonical Game/Content sections и JSON только выбранного balance domain; `.claude/skills/balance-check`, consistency/content audit, smoke-check, design rules.

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

[BALANCE_WORKFLOW](../BALANCE_WORKFLOW.md), [записи прогонов](../../playtests/README.md), [шаблон](../../playtests/TEMPLATE.md) и evidence владельца одобренного изменения. Один файл на reportId, отдельные OBS-NN, связи с immutable raw packet, proposal, diff/commit и повторным прогоном. Неизвестные ожидания не дописываются от имени tester-а; статус замечания не заменяет execution status IP. .claude balance-check остаётся read-only; optional skill edits — только по отдельному scope. Любой production IP может использовать workflow без mandatory dependency на завершение баланса всего каталога.

Поставляемые материалы: [checklist](../../playtests/CHECKLIST.md), [review template](../../playtests/REVIEW_TEMPLATE.md), [реальный review](../../balance/balance-progression-2026-09-21.md), [synthetic rehearsal](../../playtests/exercises/IP32/README.md). Результаты проверок — в [evidence](../evidence/design-sync-R2-2026-09-21-ip32.md#ip-32).

## Gates и недостающие решения

BG-01 для применения конкретных чисел/механик; отсутствие product target отмечается, а не заполняется AI. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.


## Потребитель — стартовый FIELD-001

F1-00 использует процесс подготовки baseline, F1-09 — цикл observation/proposal/approval/check/retest для стартовых IDs. Эта привязка не даёт blanket approval любым будущим числам.

Scope — [field-001-start-R1](../milestones/FIELD-001-start.md);
порядок, packet readiness и evidence — только [STATUS](../STATUS.md#field001-execution).
