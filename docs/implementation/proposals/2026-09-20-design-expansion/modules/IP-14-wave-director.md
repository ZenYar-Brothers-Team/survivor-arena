# IP-14 — Wave Director: continuous и burst timeline

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-14-wave-director.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Доработать действующий директор и spawner, сохранив division Director decides/spawner executes, pooling и phase scaling.

## Зависимости

[IP-04](IP-04-enemy-core.md), [IP-13](IP-13-enemy-patterns.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Структура забега…», «Враги…»; Content Fields/empty Wave Encounter section; DECISION-0014; WaveDirector/timeline/spawn timer/spawner.

## Scope

Data-driven continuous/burst mode, count/window/composition/cap/geometry/seed; ordinary/pressure/elite/rest rhythm, independent modifiers, one-shot mid/final boss hooks. Pause-aware run time. Actual spawn/cap/deferral outcomes observable; no separate hidden default. Traveler schedule независим и не становится обычной wave type.

## Out of Scope

Production schedules, adaptive difficulty, Traveler RNG/type selection, profiler claims from throttled warnings.

## Acceptance criteria

Burst исполняется один раз в agreed window и cap policy; no backlog storm after pause/skip. Continuous behavior retained. Phase transitions/skips/last hold/hook boundaries deterministic на director level; later wave may be faster but frailer. Registry/pool stays consistent at repeated load. Spawn actual counts distinguish requested/suppressed/deferred. Production schedules не выводятся из fixture timeline.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Phase HUD; dev timeline/composition/actual cap/burst/hook diagnostics; producer fields доступны IP-31, но gameplay не зависит от recorder.

## Проверки

Continuous regression, burst 0/1/count/cap, phase skip/catch-up/same-time hook, pause/end/restart, pool reuse; load bound with recorded hardware/counts and approved threshold. Seeded decisions ≠ full physics replay.

## Документационные изменения

Update DECISION-0014 через явное дополнение, wave schema и fixture rationale; IP-15/24 consume target contract.

## Gates и недостающие решения

G-11/G-14 только в части shared event boundary/data; отдельный W-01: burst cap/drop-vs-defer, catch-up и whether bosses/Travelers count toward cap. Эти pressure rules до реализации не выбираются молча. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
