# DECISION-0011 — Пулинг часто создаваемых GameObject

Status: Approved

Date: 2026-09-15

Related IP: cross-cutting (Game.Progression, Game.Enemy, Game.ActiveSkill)

## Context

Ревью PR #6 (комментарий на `ExperienceDropFactory.cs:22`) отметило, что
паттерн «создать `GameObject`, навесить компоненты, потом `Destroy`»
независимо повторяется в трёх местах — дропы опыта (`ExperienceDropFactory`),
враги (`ContinuousFixtureEnemySpawner`/`EnemyFactory`) и мины активных умений
(`SceneActiveSkillEffectExecutor.PlaceMine`) — и все три создаются/уничтожаются
часто и в количестве по мере роста контента. Единой инфраструктуры пулинга в
репозитории не было.

## Decision

- Новый модуль `Game.Pooling` (без зависимостей, кроме UnityEngine) с
  `GameObjectPool<T> where T : Component` — `Rent()` берёт неактивный экземпляр
  из стека или зовёт `factory()`, `Return()` деактивирует, репарентит под
  опциональный `root`-`Transform` и кладёт обратно.
- Пулинг везде **опционален через явный параметр**, а не через скрытое
  статическое состояние: `ExperienceDropFactory.Spawn(...)` и
  `EnemyFactory.Spawn(...)` получили опциональный `pool = null` —
  без пула поведение идентично прежнему (`Destroy`/`DestroyImmediate`,
  `== null` после уничтожения). Это осознанный выбор: статический пул внутри
  фабрики означал бы переиспользование одного и того же экземпляра между
  независимыми юнит-тестами в рамках одного прогона (нет domain reload между
  тестами), что сломало бы тесты вида `Assert.IsTrue(_enemy == null)` после
  `Despawn()`. Явный параметр эту проблему исключает: пул передаётся только
  там, где владелец пула существует и осознанно его создаёт.
- Владелец пула — тот, чей жизненный цикл естественно совпадает с пулом:
  - `PlayerExperienceRuntime.DropPool` — пул дропов опыта живёт как дочерние
    объекты игрока; `EnemyRuntime.HandleDeath` передаёт его явно в `Spawn(...)`.
  - `ContinuousFixtureEnemySpawner` — свой пул врагов, создаётся в
    `Initialize()`, передаётся в `EnemyFactory.Spawn(...)`.
  - `SceneActiveSkillEffectExecutor` — свой пул маркеров мин (`SpriteRenderer`,
    у них нет отдельного компонента), с выделенным неактивным контейнером
    `Transform`, уничтожаемым вместе с экзекьютором в `Dispose()`.
- `ExperienceDropRuntime`/`EnemyRuntime` теперь поддерживают повторный
  `Initialize()` на переиспользуемом экземпляре: `ExperienceDropRuntime`
  сбрасывает `_consumed = false`; `EnemyRuntime` отписывается от старого
  `Health.Died`, сбрасывает `_despawned` и заново включает коллайдер вместо
  однократного `throw`-guard'а. Раньше единственным получателем без пула
  `Initialize()` бросал при повторном вызове — это поведение сохранено для
  всех прямых `AddComponent` + `Initialize()` вызовов (юнит-тесты не
  изменились).

## Consequences

- Существующие юнит-тесты не тронуты: все прямые вызовы `EnemyFactory.Spawn(...)`
  /`ExperienceDropFactory.Spawn(...)` без пула сохраняют старое поведение
  1-в-1 (включая `== null` после уничтожения).
- Три asmdef (`Game.Progression`, `Game.Enemy`, `Game.ActiveSkill`) получили
  ссылку на `Game.Pooling`.
- Не пуловались: `ActiveSkillProjectile`/снаряды и прочие визуальные эффекты —
  вне текущего запроса ревью, можно добавить по тому же паттерну позже.

## Approval

Пользователь явно утвердил 2026-09-15 в рамках повторного триажа комментариев
PR #6 («надо пофиксить» напротив пункта об отсутствии пулинга), после
согласования общего плана из пяти пунктов через `EnterPlanMode`/`ExitPlanMode`.
