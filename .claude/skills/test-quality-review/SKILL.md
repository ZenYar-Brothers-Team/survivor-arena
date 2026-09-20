---
name: test-quality-review
description: "Read-only review of test quality (not just existence) for a module: assertion strength, edge cases, determinism, isolation, naming, and whether the tests would actually catch a regression. Use after finishing an IP or before marking evidence in STATUS.md."
argument-hint: "[module folder e.g. Enemy | test file path | IP-NN]"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `test-evidence-review`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only. Passing and existing tests can still leave behaviour uncovered — this skill judges the tests, it does not run them (running is `/smoke-check`).

## 1. Scope
`Assets/Game/<Module>/Tests/*.cs` (EditMode) and `Assets/Game/**/PlayModeTests/*.cs`; for `IP-NN` take the module's acceptance criteria from `docs/implementation/modules/` and the evidence text in `docs/implementation/STATUS.md`.

## 2. Map criteria to tests
For every acceptance criterion / claimed evidence item, find the test(s) that exercise it. Flag **uncovered criteria** and **evidence text that names a test that does not exist or does not assert what is claimed** (STATUS statements like "25 new checks cover ..." must be traceable).

## 3. Per-test quality (read each test)
- **Assertion strength**: asserts on outcomes, not just "no exception"; tolerant comparisons for floats; exact expected values instead of `Greater(x, 0)` when an exact value is derivable; state after the action AND absence of side effects.
- **Edge cases**: pause / run ended / not running, empty and at-cap collections, zero/negative/NaN inputs for validators, repeated `Initialize`/`Shutdown`, pool reuse, event unsubscribe after `Shutdown`.
- **Determinism**: seeded RNG only, no time/frame dependence in EditMode, no reliance on test order or leaked static state (e.g. `EnemyRegistry.Count` baseline compared, not assumed 0).
- **Isolation/cleanup**: created GameObjects destroyed in `TearDown`/`finally`; scenes and static registries restored; no shared mutable fixtures.
- **Unity specifics**: `AddComponent` does not call `Awake()` in EditMode (tests that need it use reflection helpers); no obsolete-as-error APIs; asmdef references present.
- **Naming and structure**: descriptive `Subject_Condition_Expected`; arrange/act/assert visible; one behaviour per test; helper data in a `*TestData` fixture, not copy-pasted.
- **Mutation sanity**: pick 2–3 important assertions and reason whether a plausible bug (off-by-one, sign flip, missing null check, wrong phase index) would make the test fail. Say "would NOT catch" where it wouldn't.

## Output
```
## Test quality: <scope>
Tests reviewed: <N in M files>   Criteria covered: X/Y
Verdict: ADEQUATE | INCOMPLETE | WEAK
### Uncovered criteria
### Weak or misleading tests   (file:line → issue → suggested stronger assertion)
### Missing edge cases
### Determinism / isolation risks
### Evidence claims not traceable
### Positive observations
```
