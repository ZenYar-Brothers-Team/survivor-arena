# DECISION-0010 — Откат при частичном сбое GameplayCompositionRoot

Status: Approved

Date: 2026-09-15

Related IP: cross-cutting (Game.Bootstrap)

## Context

Ревью PR #6 (комментарий на `GameplayCompositionRoot.cs:73`) отметило, что
`Initialize()` вызывает `Initialize(...)` у семи подсистем подряд без какой-либо
компенсации при ошибке. Если, например, `passiveRuntime.Initialize` упадёт
(нет id пассивки в каталоге), уже успевшие инициализироваться подсистемы
(`draftRuntime`, `activeSkillRuntime` и т.д.) остаются подписанными на чужие
события и держат созданные `IDisposable` (executor, UI presenter/view/model),
которые никогда не освобождаются — сам композиционный корень при этом просто
логирует ошибку и выключает себя (`enabled = false`).

## Decision

- Каждая из семи подсистем (`PlayerCharacterRuntime`, `PlayerExperienceRuntime`,
  `LevelUpDraftRuntime`, `PlayerActiveSkillSetRuntime`, `PlayerPassiveSetRuntime`,
  `ContinuousFixtureEnemySpawner`, `GameplayUiRoot`) получила публичный метод
  `Shutdown()`, отменяющий ровно то, что сделал `Initialize()` — отписку от
  чужих событий, `Dispose()` владеемых объектов, сброс `_initialized = false`.
  Логика переиспользует то же, что уже делает `OnDestroy()` каждого типа —
  никакой новой seman­тики очистки не придумано.
- `GameplayCompositionRoot.Initialize()` накапливает список успешно
  инициализированных подсистем (`List<Action>` из их `Shutdown`) и в `catch`
  вызывает их в обратном порядке перед тем как перебросить исключение дальше.
  Верхнеуровневый `try/catch` в `Start()` не изменился — он по-прежнему
  логирует и выключает корень; откат лишь гарантирует, что между «ничего не
  инициализировано» и «всё инициализировано» не остаётся невидимого
  промежуточного состояния с висящими подписками.

## Consequences

- Подсистемы, у которых `Initialize()` не создаёт внешних подписок
  (`PlayerExperienceRuntime`, `ContinuousFixtureEnemySpawner` — она подписывается
  на `enemy.Despawned` только из `Update()`, после полной инициализации, а не
  из `Initialize()`), получили тривиальный `Shutdown()`, сбрасывающий только
  флаг — это осознанно, а не недосмотр.
- Паттерн «публичный `Shutdown()`, отменяющий `Initialize()`» обязателен для
  любой новой подсистемы, добавляемой в `GameplayCompositionRoot` — см. правило
  в `AGENTS.md`.
- Внешнее поведение при сбое (лог + `enabled = false`) не изменилось; изменилось
  только внутреннее состояние подсистем в момент сбоя.

## Approval

Пользователь явно утвердил 2026-09-15 в рамках повторного триажа комментариев
PR #6 («надо пофиксить» напротив пункта про отсутствие отката в
`GameplayCompositionRoot`), после согласования общего плана из пяти пунктов
через `EnterPlanMode`/`ExitPlanMode`.
