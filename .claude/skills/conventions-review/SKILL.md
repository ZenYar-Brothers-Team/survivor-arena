---
name: conventions-review
description: "Read-only review of Unity C# files or a directory against this repository's own rules (AGENTS.md coding conventions and docs/decisions ADRs), plus Unity lifecycle/GC pitfalls. Use before a PR or after finishing an IP module. Not a replacement for the built-in /code-review."
argument-hint: "[path-to-file-or-directory | 'diff']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `code-review`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Review the target and report findings. **Do not edit any file.**

## 1. Scope
- Argument is a path: read every `.cs` file under it in full (skip `*.meta`).
- Argument is `diff` or empty: use `git diff origin/develop...HEAD --name-only` plus uncommitted changes; review the changed `.cs` files.
- Read `AGENTS.md` (Coding conventions) and the ADRs in `docs/decisions/` that apply (see step 2).

## 2. ADR compliance
Find `DECISION-NNNN` references in the files, in `docs/implementation/STATUS.md` for the touched IP, and by topic:
- config/JSON content → 0009; shared validation → 0012; pooling → 0011; root rollback → 0010;
  perf logging → 0008; health → 0006; visuals → 0007, 0013; waves → 0014; UI → 0005.

Classify each deviation: **VIOLATION** (blocking; pattern the ADR forbids), **DRIFT** (warning), **MINOR** (info).

## 3. Repository rules (each is a checkable rule from AGENTS.md)
- Gameplay values as C# literals or `[SerializeField]` defaults instead of JSON under `Assets/Resources/Content/<Category>/`; DTOs carrying tuning defaults; seeds/spawn geometry outside JSON.
- Local `ValidatePositive/NonNegative/Finite`-style helpers instead of `Game.Content.NumericValidation`.
- `new GameObject`/`Destroy` per spawn on frequently spawned things instead of `GameObjectPool<T>` (opt-in `pool` parameter, never a hidden static pool); a poolable type that throws on a second `Initialize()`.
- `GameplayCompositionRoot` subsystem without a `Shutdown()` that undoes exactly what `Initialize()` did.
- Scale-sensitive loops (per enemy/projectile/mine) without `PerfGuard.Measure(...)`.
- More than one top-level type per file, or file name not equal to type name.
- Procedural sprite motion applied to the gameplay root/`Rigidbody2D`/collider instead of the `VisualRoot` pose compositor.
- UI owning gameplay state (must be View → presenter intent → model/runtime); debug UI visible outside development builds.
- Production content implemented from a Draft entity or a made-up rule (`docs/Content_design.md` is canonical).

## 4. Unity pitfalls
- Allocations in `Update/FixedUpdate/LateUpdate` (LINQ, `new` collections, string concatenation, closures, boxing, `GetComponent` per frame).
- Time-dependent logic not using `Time.deltaTime`/run-model time; logic that ignores pause (`RunModel` state).
- Event subscribe without matching unsubscribe (`OnDestroy`/`Shutdown`); destroyed-object access via `== null`.
- `Awake/Start` ordering assumptions; work in `Awake` that requires another component's `Awake`.
- Obsolete-as-error APIs in Unity 6.6 (e.g. `Object.GetInstanceID()`).
- `Resources.Load`/JSON parsing per frame instead of once at composition.

## 5. Testability
- Is domain logic pure C# (no `MonoBehaviour`) where it can be? Is there a seam for tests, or hardcoded values/statics blocking it?
- New behaviour without an EditMode test; bug fix without a regression test.

## Output
```
## Review: <target>
### ADR compliance: NONE FOUND | COMPLIANT | DRIFT | VIOLATION   (list each with file:line)
### Repository rules: X findings                                   (rule → file:line → why)
### Unity pitfalls: X findings
### Testability: OK | GAPS
### Positive observations
### Required changes   (violations, blocking)
### Suggestions
### Verdict: APPROVED | APPROVED WITH SUGGESTIONS | CHANGES REQUIRED
```
Reference code as `path:line`. Report only findings you verified in the code; say "not checked" for anything you skipped.
