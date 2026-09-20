---
paths:
  - "Assets/Game/Enemy/**/*.cs"
---

# Enemy AI code rules

Adapted from the upstream `ai-code` rules. Repository rules in `AGENTS.md` win on conflict. Network-related upstream rules do not apply (single-player).

- **AI update budget**: per-frame enemy update cost is guarded with `PerfGuard.Measure(...)` at a threshold that is generous for fixture-scale content (upstream target: 2 ms per frame for all AI — verify with the Profiler, do not assume).
- **All AI parameters are tunable data** (distances, tolerances, cycle times, telegraph/dash timings, cooldowns, burst/pattern counts) in `Assets/Resources/Content/Enemies/*.json`; values needed only for some kinds are required per kind by `FixtureEnemyCatalog` — not defaulted in code.
- **Telegraph intent**: dashes, explosions, and attack bursts give the player a readable wind-up (configured telegraph time) before damage.
- **Debuggable**: current movement phase / attack state is observable through the development observability surface (no debug-only state hidden inside private fields); state transitions are the kind you can log in development builds.
- **Pause-safe**: no timers advance while the run is paused; controllers take explicit `deltaTime` and `isRunning` inputs (see `EnemyMovementController`, `EnemyAttackController`).
- **New behaviours extend the existing families** (movement kind / projectile pattern) through the definition + controller pattern rather than adding ad-hoc `if` chains in `EnemyRuntime`; prefer a data-driven table or a small strategy over growing a `switch`.
- **Group behaviour** (formations, roles) must be data-driven if introduced; wave composition is owned by `WaveDirector`, not by individual enemies.
