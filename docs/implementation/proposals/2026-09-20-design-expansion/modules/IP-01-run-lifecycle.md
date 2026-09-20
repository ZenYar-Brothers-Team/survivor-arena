# IP-01 — Run lifecycle, pause ownership и результат забега

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-01-run-lifecycle.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Переиспользовать RunModel/RunController, 15-minute timer и reason-based pause. Добавить независимый от dev recorder результат; старое evidence относится только к прежней ревизии.

## Зависимости

[IP-00](IP-00-content-contract.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

GDD «Структура забега и условия завершения», «Управление, бой и выживание»; UI §§2,6,15,16,23; RunModel/RunController и composition lifecycle.

## Scope

Start/running/pause/resume/won/lost, pause reasons, simulation clock; immutable run identity/terminal snapshot и приём вкладов владельцев kills/XP/build в минимальный RunOutcome. Contract reset/teardown для следующего run; aborted/retry/error — причины завершения session report, не новые gameplay victories и не правила наград. Producer contracts не требуют dependencies на gameplay consumers или exporter.

## Out of Scope

Rewards/economy, full navigation, diagnostic file export, deterministic replay, право на награду за Quit.

## Acceptance criteria

Alive at 15:00 даёт won, смерть раньше — lost; убийство boss не завершает run. Pause/end останавливают simulation. Terminal snapshot формируется один раз, повторный запрос не создаёт повторных rewards/events. RunOutcome доступен в non-development build без IP-31; ещё не подключённый producer обозначается unavailable, а полный обязательный Results состав проверяет IP-26. Reset меняет run ID и не переносит pause reasons.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Elapsed 00:00→15:00 и pause/end states передаются snapshot/presenter; IP-10A предоставляет общий renderer, IP-26 связывает результаты и Retry.

## Проверки

State transitions, independent pause reasons, shortened win/loss, terminal idempotency, fake outcome contributors, teardown/reinit и zero-time run. Существующую terminal ordering не менять молча.

## Документационные изменения

RunOutcome/time/terminal contract; обновлённый IP-01/evidence после реализации. Reward policy остаётся IP-25; UI confirmation/Retry policy IP-26.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md), [IP-07](IP-07-level-up-draft.md), [IP-10A](IP-10A-ui-foundation.md), [IP-15](IP-15-boss-framework.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
