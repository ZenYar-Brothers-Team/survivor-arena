---
paths:
  - "Assets/Game/Content/**/*.cs"
  - "Assets/Game/Pooling/**/*.cs"
  - "Assets/Game/Diagnostics/**/*.cs"
---

# Foundation (core) code rules

The upstream `engine-code` rules mapped onto this repo's dependency-free foundation modules (`Game.Content`, `Game.Pooling`, `Game.Diagnostics`). Repository rules in `AGENTS.md` win on conflict.

- **Dependency direction is strict**: foundation modules never depend on gameplay modules (Enemy, Character, ActiveSkill, ...). Keep `Game.Content` free of any dependency of its own so every module can reference it without cycles.
- **Zero allocations in hot paths**: pre-allocate, pool, reuse buffers (see `EnemyRegistry` query buffers, `GameObjectPool<T>`). `PerfGuard` must stay allocation-free (it is a `readonly struct`).
- **Deterministic cleanup**: anything that owns resources implements `IDisposable` or an explicit `Shutdown()`; no hidden static state that leaks between tests (no static pools).
- **Threading**: state which types are main-thread-only; nothing here is thread-safe unless documented.
- **Public API**: every public type/method has a doc comment saying what it guarantees and how to call it (a short usage example for non-obvious APIs). Changes to a public signature update all callers and tests in the same change; note the change in the related DECISION when it is cross-module.
- **Validation** lives here: numeric checks go through `Game.Content.NumericValidation` (never a private copy elsewhere).
- **Measure optimizations**: before/after numbers (or a `PerfGuard` threshold) are recorded in the PR or DECISION — no unmeasured "optimization".
- **Unity 6000.6 APIs**: verify against the compiler/package source rather than memory (e.g. `Object.GetInstanceID()` is obsolete-as-error).
