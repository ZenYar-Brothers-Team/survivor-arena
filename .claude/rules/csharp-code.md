---
paths:
  - "Assets/**/*.cs"
---

# C# repository conventions

Moved from the root AGENTS.md without changing their requirements. These apply
to all production and test C# changes, including scripts outside Assets.
Read the additional area-specific rules routed by AGENTS.md when applicable.

### Performance logging for scale-sensitive operations

If code does work whose cost scales with runtime entity counts (an operation over
every enemy, every projectile, every active mine, etc.) and is expected to stay
cheap only while those counts stay small, wrap it in
`Game.Diagnostics.PerfGuard.Measure("Descriptive.OperationName", warningMilliseconds)`
(a `using` scope). It measures the wrapped block and logs a `Debug.LogWarning`
only when the threshold is crossed, throttled per operation name so a
persistently slow path can't flood the console — safe to leave in permanently,
including on code that is fast today. Pick a threshold that is generous for
current fixture-scale content; the point is catching regressions as real
content and enemy counts grow, not micro-optimizing now.

Reference implementations: `Assets/Game/Diagnostics/PerfGuard.cs`, and its use in
`Assets/Game/Enemy/Model/EnemyRegistry.cs` (`TryFindNearest`) and
`Assets/Game/ActiveSkill/Runtime/SceneActiveSkillEffectExecutor.cs` (`TickMines`).
See [DECISION-0008](../../docs/decisions/0008-perf-logging.md).

### Entity/content values belong in config, not code

Never hardcode gameplay-entity values (damage, cooldowns, stats, effect
parameters, HP, drop tables, etc.) as C# literals or `[SerializeField]`
defaults on a MonoBehaviour. They belong in a JSON file under the single
shared `Assets/Resources/Content/<Category>/*.json` tree (one subfolder per
content type — `Enemies/`, `Characters/`, `Passives/`, `ActiveSkills/`, and so
on for new content types), loaded through a `Fixture*Catalog` (or, for real
production content, a `*Catalog`) that deserializes via
`Game.Content.Json.JsonContentFile` into DTOs, then maps those DTOs through
the existing validating domain constructors — JSON is purely an authoring
layer, domain types keep all validation. This keeps entity tuning in one
place that a future automated balance pass can read and edit directly as
plain files, without touching code.

DTOs must not carry tuning defaults either: a field is either required in the
JSON (declare it `float?`/`int?` and have the catalog reject a missing one by
name, or leave it non-nullable so validation fails on zero) or it has a
*neutral* default that means "leave unchanged" (a 1x multiplier, 0 bonus) and
that default is owned by one domain type (e.g. `WaveEnemyModifiers.Identity`),
not repeated in the DTO. Values that only matter for some kinds/patterns are
required per kind (see `FixtureEnemyCatalog`). Randomness seeds and spawn
geometry are content too and live in the JSON, not in `[SerializeField]`
fields on runtime components.

Reference implementations: `Assets/Game/ActiveSkill/Progression/FixtureActiveSkillCatalog.cs`
(includes the polymorphic-effect pattern via `ActiveSkillEffectJsonConverter`
and a `"kind"` discriminator — follow this for any future polymorphic content),
`Assets/Game/Enemy/Model/FixtureEnemyCatalog.cs`,
`Assets/Game/Character/Model/FixtureCharacterCatalog.cs`,
`Assets/Game/Progression/Passive/FixturePassiveCatalog.cs`.
See [DECISION-0009](../../docs/decisions/0009-json-content-config.md).

### Shared numeric validation

Constructor-argument validation for domain types (positive/non-negative/finite
checks, count checks, ranges) belongs in
`Game.Content.NumericValidation` — never re-implement a local
`ValidatePositive`/`ValidateNonNegative`/`ValidateFinite`-style private
static method on a new type. `Game.Content` has no dependencies of its own,
so it's reachable from every gameplay module; add a reference to it in a
module's `.asmdef` if it's missing rather than duplicating the check.

Reference implementation: `Assets/Game/Content/NumericValidation.cs`, used by
`Assets/Game/Combat/Health.cs`, `Assets/Game/Character/Model/CharacterBaseStats.cs`,
and the `Game.ActiveSkill.Progression` effect types (`MineEffect.cs`,
`AreaEffect.cs`, etc.).
See [DECISION-0012](../../docs/decisions/0012-shared-numeric-validation.md).

### Pool frequently spawned/destroyed GameObjects

Anything created and destroyed often at runtime (per enemy death, per
projectile, per skill effect) should use `Game.Pooling.GameObjectPool<T>`
instead of a bare `new GameObject(...)` + `Destroy(...)` pattern. Pooling is
always opt-in via an explicit `pool` parameter on the spawn method, defaulting
to `null` — never a hidden static pool inside a static factory, since a
process-wide static pool would leak reused instances across independent unit
tests within the same test run (no domain reload between individual
`[Test]`s). The pool's owner is whichever object's lifetime naturally matches
the pool's — a long-lived runtime, spawner, or executor — not the factory
itself. A type that supports being rented from a pool must also support a
second `Initialize()` call on the same instance (tearing down its previous
life first) instead of throwing on re-initialization.

Reference implementations: `Assets/Game/Pooling/GameObjectPool.cs`,
`Assets/Game/Progression/Runtime/ExperienceDropFactory.cs` (pool owned by
`PlayerExperienceRuntime.DropPool`), `Assets/Game/Enemy/Runtime/EnemyFactory.cs`
(pool owned by `ContinuousFixtureEnemySpawner`),
`Assets/Game/ActiveSkill/Runtime/SceneActiveSkillEffectExecutor.cs` (mine
marker pool owned by the executor itself).
See [DECISION-0011](../../docs/decisions/0011-gameobject-pooling.md).

### Composition-root subsystems roll back on partial init failure

Any subsystem `GameplayCompositionRoot` initializes must expose a public
`Shutdown()` that undoes exactly what its `Initialize()` did — unsubscribe
from other subsystems' events, `Dispose()` anything it created and owns, then
reset its own "initialized" flag. Reuse the same cleanup the type's
`OnDestroy()` already does rather than inventing new teardown logic; a type
with nothing external to unwind (e.g. no cross-object event subscriptions
made during `Initialize()`) still gets a `Shutdown()` that resets its flag,
so the composition root can treat "never initialized" and "rolled back" the
same way. `GameplayCompositionRoot.Initialize()` calls each subsystem's
`Shutdown()` in reverse order if a later subsystem's `Initialize()` throws,
so a failed composition never leaves earlier subsystems live-subscribed.

Reference implementation: `Assets/Game/Bootstrap/GameplayCompositionRoot.cs`,
and the `Shutdown()` methods on `PlayerCharacterRuntime`,
`LevelUpDraftRuntime`, `PlayerActiveSkillSetRuntime`, `PlayerPassiveSetRuntime`.
See [DECISION-0010](../../docs/decisions/0010-composition-root-rollback.md).

### One type per file, file name matches the type

Every `class`, `struct`, `interface`, and `enum` — production code and tests —
gets its own file, named exactly after the type (standard .NET convention).
Never add a second top-level type to an existing file "because it's small" or
"because it's related" — a family of small related types (e.g. several effect
DTOs, several exception types) still gets one file per type, grouped by
folder, not by file. This keeps file search, `git blame`, and IDE
go-to-definition meaningful, and avoids the kind of near-duplicate naming
(`PlayerActiveSkillRuntime` vs `PlayerActiveSkillSetRuntime`) that made a
dead, superseded implementation hard to tell apart from the live one.

When splitting an existing multi-type file, keep the original file (and its
`.meta` GUID) for whichever type already matches its name, and create new
files only for the others — don't discard and regenerate GUIDs for types that
didn't need to move.
