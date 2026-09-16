# Repository instructions

The canonical product documents are `docs/Game_design.md`, `docs/Content_design.md`, and `docs/implementation/`.

When implementing an IP module:

1. Follow `docs/implementation/WORKFLOW.md`.
2. Use `docs/implementation/STATUS.md` as the only source of execution status and select the first numerically ordered `Ready` module unless the user names another module.
3. Read the selected file in `docs/implementation/modules/` and only the design sections and content IDs listed in its Context section.
4. Treat Game Design as canonical for system rules, Content Design for concrete entities, the IP module for scope, and the repository for implementation state.
5. Never implement Draft content as production content or silently invent a missing product rule.
6. Complete the module's checks, update `STATUS.md`, and synchronize affected design documents in the same change.
7. Record cross-layer or user-approved deviations in `docs/decisions/`; do not turn an implementation workaround into game design automatically.

Do not duplicate module status inside module specification files.

## Coding conventions

These apply to all code changes, not only IP module work.

### Visual art and procedural sprite presentation

Before generating, editing, importing, replacing, or wiring raster art, read
`docs/art/ART_DIRECTION.md` and `docs/art/ASSET_PIPELINE.md`. They are the
approved sources of truth for visual style, prompts, source/runtime paths,
naming, provenance, PNG preparation, Unity import settings, approval gates,
and safe replacement. Preview images do not enter `Assets`; only a
user-approved candidate is prepared as a runtime asset. Preserve an existing
runtime path and `.meta` GUID when an approved image is replaced.

Procedural sprite motion must live under an entity's child `VisualRoot`; never
animate the gameplay root, `Rigidbody2D`, collider transform/geometry, or
authoritative movement state for a visual effect. Apply body squash/stretch,
bob, tilt, recoil, facing, and transient renderer feedback through one pose
compositor/writer so independent channels cannot overwrite each other.
Presentation values belong in validated config profiles, pause advances no
presentation time, and `Shutdown()`/pool return must restore the captured
baseline pose and renderer state. See
[DECISION-0013](docs/decisions/0013-procedural-sprite-presentation.md).

### Development UI stays compact and non-obstructive

Development/debug controls must be hidden in non-development builds and
collapsed by default behind a small launcher in Editor/Development Build.
Expanded tooling uses a bounded drawer with thematic tabs and scrolling for
long content; never append new controls to an unbounded horizontal strip over
the gameplay viewport. At the 1920x1080 reference resolution, an expanded
debug surface should stay within 25% of viewport width and 45% of viewport
height unless a dedicated full-screen diagnostic view is explicitly required.
Debug commands still follow the normal View -> presenter intent -> model/runtime
boundary and must not make UI the owner of gameplay state. See DECISION-0005.

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
See [DECISION-0008](decisions/0008-perf-logging.md).

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

Reference implementations: `Assets/Game/ActiveSkill/Progression/FixtureActiveSkillCatalog.cs`
(includes the polymorphic-effect pattern via `ActiveSkillEffectJsonConverter`
and a `"kind"` discriminator — follow this for any future polymorphic content),
`Assets/Game/Enemy/Model/FixtureEnemyCatalog.cs`,
`Assets/Game/Character/Model/FixtureCharacterCatalog.cs`,
`Assets/Game/Progression/Passive/FixturePassiveCatalog.cs`.
See [DECISION-0009](decisions/0009-json-content-config.md).

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
See [DECISION-0012](decisions/0012-shared-numeric-validation.md).

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
See [DECISION-0011](decisions/0011-gameobject-pooling.md).

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
See [DECISION-0010](decisions/0010-composition-root-rollback.md).

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
