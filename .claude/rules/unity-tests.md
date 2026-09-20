---
paths:
  - "Assets/Game/**/Tests/**/*.cs"
  - "Assets/Game/**/PlayModeTests/**/*.cs"
---

# Unity test rules

Adapted from the upstream `test-standards` rules. Repository rules win on conflict (see "Deliberately different").

## Structure
- One test class per file, file named after the class; test asmdef references every assembly whose types appear in **public signatures** it touches (optional-parameter types included — CS0012 otherwise).
- Clear arrange/act/assert; one behaviour per test; shared data/builders in a `*TestData` helper, not copy-pasted.
- Name tests `Subject_Condition_Expected` (e.g. `Advance_WhenNotRunningSpawnsNothingAndDoesNotTransition`).

## Isolation and determinism
- Unit tests do not depend on external state (filesystem, network, time, frame count); seeded RNG only.
- Integration/scene tests clean up after themselves: destroy created GameObjects in `TearDown`/`finally` (`DestroyImmediate`), restore static registries (compare `EnemyRegistry.Count` to a baseline captured in `SetUp`, never to 0).
- No shared mutable state and no hidden static pools between tests (Unity does not reload the domain between `[Test]`s).
- Mock/fake external dependencies through interfaces (fake model/view for presenters); tests stay fast.

## Unity specifics
- EditMode `AddComponent` does not call `Awake()` — use the reflection `InvokeAwake` helper where the code under test needs it.
- Do not call `Object.GetInstanceID()` (obsolete-as-error in Unity 6.6); compare object references (`AreSame`, `HashSet<T>`).

## Assertions and coverage
- Assert exact expected values from config where derivable, and the absence of side effects (registry back to baseline, events unsubscribed after `Shutdown()`).
- Performance tests specify an acceptable threshold and fail when exceeded (no "just print the time").
- **Every bug fix ships with a regression test** that fails without the fix; record it in `docs/regression-map.md` (`/regression-map`).
- Running the suite: `/smoke-check` — never launch batch tests while an interactive Editor has the project open.

## Deliberately different from upstream
- Upstream naming `test_[system]_[scenario]_[expected]` (snake_case, GDScript style) — C#/NUnit `Subject_Condition_Expected` is used here, matching all existing tests.
