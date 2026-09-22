# Shared enemy death presentation — 2026-09-22

Authorization: [DECISION-0040](../../decisions/0040-shared-enemy-death-presentation.md). Execution status belongs only to [STATUS](../STATUS.md).

## Scope

One config-driven death presentation is passed by the composition root to ordinary enemies, bosses and Travelers. Lethal damage immediately removes gameplay participation and publishes death; a cloned visual performs squash, shrink, darken/fade and a five-particle dust burst before delayed pool return. The root position is unchanged and the rigidbody is made non-simulated, so the effect cannot add a death push. Pause freezes progression; cleanup returns immediately. Plain placeholder renderers and animated body rigs use the same component and algorithm.

Config: `Assets/Resources/Content/Presentation/FixtureEnemyDeathPresentation.json`. Runtime: `EnemyDeathPresentationRuntime` plus the existing `EnemyRuntime` lifecycle. No raster was generated or modified.

## Verification

`EnemyDeathPresentationSmokeTests` covers immediate Died versus delayed Despawned, registry removal, collider/physics shutdown, no displacement, pause freeze and eventual release. Existing enemy lifecycle, rewards, pool reuse, boss/Traveler and presentation suites remain regression coverage. Final counts are recorded after the full smoke check.

Final safe batch run on Unity 6000.6.0f1: **644/644 EditMode, 25/25 PlayMode, zero skipped**. The first attempt found a compile-time control-flow error and the next exposed an old smoke assertion that still required same-frame disappearance; both were corrected before this final full run. `git diff --check` passed after Unity-generated metadata normalization.

## Animated-body visibility correction

User observation [OBS-01/02](../../playtests/2026-09-22_enemy-death-visibility.md) exposed an ordering/reset defect: the animated villager's sprite was cleared before the death clone read it, and `SpriteRenderer.enabled` was not part of presentation baseline restoration. The death runtime now snapshots the active renderer before presentation shutdown; the baseline captures/restores enabled state. Gameplay smoke asserts the villager sprite on `DeathVisual`, then checks that the pooled body renderer is enabled and the old clone inactive. Final post-fix counts are recorded after rerun.

Post-fix Unity log reported no C# compilation errors. Пользователь повторно проверил Gameplay и принял визуальный результат 2026-09-22: «Сейчас выглядит хорошо». Финальный полный batch run выполняется после закрытия Editor перед коммитом; его результат указан ниже.

Final post-fix run on Unity 6000.6.0f1: **644/644 EditMode, 25/25 PlayMode, zero skipped**. The passed PlayMode suite includes the animated villager death clone and visible pooled reuse regression.
