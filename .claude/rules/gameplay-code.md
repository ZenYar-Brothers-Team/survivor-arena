---
paths:
  - "Assets/Game/ActiveSkill/**/*.cs"
  - "Assets/Game/Bootstrap/**/*.cs"
  - "Assets/Game/Character/**/*.cs"
  - "Assets/Game/Combat/**/*.cs"
  - "Assets/Game/Enemy/**/*.cs"
  - "Assets/Game/Movement/**/*.cs"
  - "Assets/Game/Progression/**/*.cs"
  - "Assets/Game/Run/**/*.cs"
---

# Gameplay code rules

Adapted from the upstream `gameplay-code` rule set (Claude-Code-Game-Studios); repository rules in `AGENTS.md` win on conflict.

- **All gameplay values come from JSON config** (`Assets/Resources/Content/<Category>/*.json`) — never C# literals or `[SerializeField]` defaults (DECISION-0009). DTOs carry no tuning defaults.
- **Frame-rate independence**: time-dependent logic uses delta time / run-model time; everything is **pause-aware** (no time advances while the run is not `Running`).
- **No direct references to UI code** from gameplay: cross-system communication through events/interfaces; UI is View → presenter intent → model/runtime (DECISION-0005).
- **Every gameplay system exposes a clear interface** (`I*`) and takes its dependencies through `Initialize(...)`/constructors — no static singletons for game state. Composition happens in `GameplayCompositionRoot`; each subsystem has `Shutdown()` that undoes `Initialize()` (DECISION-0010).
- **State machines** have an explicit, documented set of states and transitions (enum + one place that changes state), not scattered boolean flags.
- **Logic separate from presentation**: domain rules in pure C# types testable in EditMode; `MonoBehaviour`s are thin runtime adapters. Every new domain rule ships with an EditMode test.
- **Traceability**: XML/`//` comment naming the design source (Game Design section, Content Design ID or DECISION) for non-obvious rules; production content only from Approved (non-Draft) cards.
- **Scale-sensitive work** wrapped in `PerfGuard.Measure(...)` (DECISION-0008); frequently spawned objects pooled through `GameObjectPool<T>` (DECISION-0011); constructor validation through `Game.Content.NumericValidation` (DECISION-0012).
- One type per file, file named after the type.
