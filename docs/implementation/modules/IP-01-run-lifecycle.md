# IP-01 — Run lifecycle, pause ownership и результат забега

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Переиспользовать RunModel/RunController, 15-minute timer и reason-based pause. Добавить независимый от dev recorder результат; старое evidence относится только к прежней ревизии.

## Зависимости

[IP-00](IP-00-content-contract.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Структура забега и условия завершения», «Управление, бой и выживание»; UI §§2,6,15,16,23; RunModel/RunController и composition lifecycle.

## Scope

Start/running/pause/resume/won/lost, pause reasons, simulation clock; immutable run identity/terminal snapshot и приём вкладов владельцев kills/XP/build в минимальный RunOutcome. Contract reset/teardown для следующего run; aborted/retry/error — причины завершения session report, не новые gameplay victories и не правила наград. Producer contracts не требуют dependencies на gameplay consumers или exporter.

## Out of Scope

Rewards/economy, full navigation, diagnostic file export, deterministic replay, право на награду за Quit.

## Acceptance criteria

Alive at 15:00 даёт won, смерть раньше — lost; убийство boss не завершает run. Pause/end останавливают simulation. Terminal snapshot формируется один раз, повторный запрос не создаёт повторных rewards/events. RunOutcome доступен в non-development build без IP-31; ещё не подключённый producer обозначается unavailable, а полный обязательный Results состав проверяет IP-26. Reset меняет run ID и не переносит pause reasons.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Elapsed 00:00→15:00 и pause/end states передаются snapshot/presenter; IP-10A предоставляет общий renderer, IP-26 связывает результаты и Retry.

## Проверки

State transitions, independent pause reasons, shortened win/loss, terminal idempotency, fake outcome contributors, teardown/reinit и zero-time run. Существующую terminal ordering не менять молча.

## Документационные изменения

RunOutcome/time/terminal contract; обновлённый IP-01/evidence после реализации. Reward policy остаётся IP-25; UI confirmation/Retry policy IP-26.

### Контракт реализации

IP-31 observability extension: `PauseChanged(reason, added)` отражает accepted ownership transitions, включая дополнительные причины уже активной паузы. `RunOutcomeContribution.Sets` — nullable immutable acquired-set snapshots, отдельно от Build. GameplayCompositionRoot завершает run и снимает consumers в обратном порядке до очистки character/Health; diagnostic export не является prerequisite. См. [DECISION-0023](../../decisions/0023-local-playtest-recorder.md).

`RunModel.RunId` уникален для экземпляра session; `Outcome` доступен в обычной сборке. При завершении модель один раз копирует immutable вклады `IRunOutcomeContributor`, затем публикует `StateChanged`, прежний `Won`/`Lost` и `Completed`. Незаполненные nullable поля и отсутствующие producer keys означают unavailable; известные ноль и пустой build отличаются от отсутствующих данных. Ошибка capture отмечается ключом в `FailedContributors` и не мешает остановить simulation. Подключение настоящих kills/XP/build producers выполняют owning IP; IP-01 проверяет границу fake producers.

`Stop(Aborted/Retry/Error)` переводит session в технический `Stopped`, не вызывает Won/Lost и не определяет право на награды. Завершённый outcome больше не заменяется. `RunController.Shutdown()` останавливает session, сохраняет результат для чтения и идемпотентен; следующий `Initialize()` создаёт новый RunModel/RunId без пауз, contributors и подписок предыдущего забега. Координатор следующего забега должен сначала завершить session и снять подписки consumers, затем переинициализировать controller и заново связать consumers. Полный scene/world reset и Retry остаются IP-26.

HUD получает `ElapsedSeconds` через immutable `HudViewState` и показывает целые прошедшие секунды (`floor`), от 00:00 до 15:00. При совпадении событий в одном кадре сохраняется прежнее правило: первый вызов Tick/Kill, завершивший running model, фиксирует результат.

## Gates и недостающие решения

Нет дополнительных product gaps для указанного scope. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-02](IP-02-player-movement.md), [IP-03](IP-03-character-stats.md), [IP-07](IP-07-level-up-draft.md), [IP-10A](IP-10A-ui-foundation.md), [IP-15](IP-15-boss-framework.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-31](IP-31-manual-run-telemetry.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
