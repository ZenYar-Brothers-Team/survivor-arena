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
