---
name: perf-audit
description: "Static (read-only) performance audit of Unity C# for a system or the whole game: per-frame allocations, unbounded per-entity loops, missing pooling and missing PerfGuard scopes. Estimates only — it does not run the profiler."
argument-hint: "[system-folder e.g. Enemy | ActiveSkill | 'full']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `perf-profile`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Static analysis only. **Never claim measured numbers** — label every cost as an estimate and say what would need a Unity Profiler capture to confirm. Do not edit files.

## 1. Scope
Argument names a folder under `Assets/Game/` (`Enemy`, `ActiveSkill`, `Progression`, `UI`, ...) or `full`. Read `docs/decisions/0008-perf-logging.md` and `Assets/Game/Diagnostics/PerfGuard.cs` first; the project has no fixed frame/memory budget document, so state the assumed budget explicitly (default 60 fps = 16.67 ms) and mark it as an assumption.

## 2. Hot-path inventory
List every `Update`, `FixedUpdate`, `LateUpdate`, and any `Tick`/`Advance` method called from them. For each, note what it iterates (enemies via `EnemyRegistry`, projectiles, mines, drops, UI elements) and the expected order of growth.

## 3. Check
- **CPU**: nested loops over entity sets; searches/sorts over all enemies per frame; physics queries (`Overlap*`, `Raycast*`, `Physics2D.*`) per entity per frame; `GetComponent`, `FindObjectOfType`, `Camera.main`, LINQ, string ops in hot paths.
- **Allocations**: `new List/Array/closure` per frame, boxing, `foreach` over non-struct enumerators, event handlers allocating; reusable query buffers are the accepted pattern (see `EnemyRegistry`).
- **Pooling**: `Instantiate`/`Destroy`/`new GameObject` on per-enemy/projectile/drop/effect paths without `GameObjectPool<T>` (DECISION-0011).
- **Guards**: scale-sensitive operations lacking `PerfGuard.Measure("Name", threshold)`; thresholds not "generous for fixture-scale content".
- **Memory**: unbounded growth (lists that never shrink, subscriptions never removed, sprites/materials created per spawn).
- **Rendering/UI**: per-frame UI Toolkit rebuilds, per-frame text churn, per-spawn material instances, sorting-layer overdraw.
- **Loading**: synchronous `Resources.Load`/JSON parse outside composition/startup.

## Output
```
## Perf audit: <scope>
Assumed budget: <...> (assumption)
### Hotspots
| # | file:line | Issue | Growth (n = ...) | Estimated impact | Fix effort | Needs profiler? |
### Missing PerfGuard scopes   (file:line, suggested name + threshold)
### Recommendations (priority order)   each: location, approach, risk
### Quick wins (< 1 hour)
### Not checked
```
Only report issues you confirmed by reading the code at the cited line.
