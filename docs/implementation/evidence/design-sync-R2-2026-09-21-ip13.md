# IP-13 — implementation and verification evidence

Date: 2026-09-21. Scope revision: design-sync-R2. Execution status is recorded only in [STATUS](../STATUS.md).

## IP-13

User request «комить и делай следующий пункт» authorized committing IP-12 and implementing the next Ready module. IP-12 is commit `2839fb7`. Existing `Packages/manifest.json` modification was excluded and preserved. IP-03/04/05 prerequisites were checked in current code and the final full suites, not accepted only from historical test counts.

### Implementation

- Preserved all six movement families and seven projectile patterns, source/control pipeline and category-neutral runtime. Added explicit ranged wind-up and observable Cooldown/Telegraphing/Bursting phases, phase timer, burst remainder and aim. Dash direction stays locked; pause preserves observable movement phase for orbit/zigzag/retreat as well as dash.
- Required JSON contact/attack knockback distance (including zero), separate dashContactControls for dash, required telegraphSeconds, spread for fan, interval for burst, explosion radius and spiral step. Shared domain numeric/control validation remains authoritative. Single/explosive count=1 and cross count=4 are validated. Added explicit Single fixture; existing synthetic fixtures now exercise nonzero controls/wind-up and dash resistance.
- EnemyRuntime selects dash controls only during Dashing, composes slow-scaled movement with independent additive knockback and keeps attack cadence independent. WaveEnemyScaler preserves controls/wind-up; no wave scheduling or draft-model changes. Runtime tick has PerfGuard instrumentation.
- Projectile retains immutable source/owner life and binds a specific RunModel. Pause zeros velocity without lifetime advancement; terminal events release immediately. Pool return/reinit/disable clear source/profile/target/run subscription/lifetime/velocity/trail and reset renderer/rotation on reuse.
- Impact copies source/profile/target before returning the component, then invokes combat. Reentrant callbacks may rent that same component without old impact code despawning the new life. An already queued old-run state callback is ignored when it does not match the current bound run's state.
- Existing gated DEV drawer displays movement/attack phase, timers, burst remainder, control state and last projectile source/life via its presenter. Dash/attack warning line uses a valid sprite material. No permanent ordinary-enemy HUD or new raster assets.

### Coverage and verification

Runner: `scripts/Test-Unity.ps1`, Unity **6000.6.0f1**, batchmode/nographics. Fresh Win32_Process checks before every invocation found no interactive Editor. Filter `^Game\.`; third-party tests selected: 0.

Final result: **467/467 EditMode, 7/7 PlayMode, 0 failed, 0 skipped**, 2026-09-21. Final code/config revision matches both successful runs; subsequent changes were docs only. Evidence paths: `TestResults/EditMode.xml`, `TestResults/PlayMode.xml`, matching logs.

New EditMode coverage: all seven fixture attack families telegraph, emit expected unit-direction/count geometry and hit with configured damage/lifetime/source; pause and immediate terminal cleanup; repeated pool reuse/source/trail reset; explosive radius hit/miss and ordinary expiry miss; impact-callback reuse; old-run callback after reinit; scaled dash + slow + independent impulse; phase preservation during pause; required/missing per-kind JSON fields and explicit-zero controls. Existing tests cover locked dash direction, burst/spiral timing, slow independent of cadence, source-after-shooter-death and resistance through actual player combat results.

`EnemyPatternSmokeTests.Patterns_TelegraphFirePauseAndRetainDeadShootersIdentity` spawns all ranged fixture families through real runtime controllers with Boss/Traveler categories, checks visible telegraph before damage, actual projectiles, pause/lifetime/velocity, real physics hit after shooter death/reuse with the original LifeId/category and immediate terminal cleanup. An initial test assertion checked a pooled component one frame too late, after a different enemy had rented it; corrected to observe return at the impact callback itself. All six pre-existing PlayMode smokes also pass.

Critical-path regressions covered: lifecycle/control/combat/death, XP/draft, active/passive/sets, waves/spawning/pooling, registry/catalog, composition/UI and telemetry. Content JSON parsing, new asset metadata, local links and `git diff --check` checked separately. PerfGuard may report cold fixture creation costs; no load benchmark or manual visual/art acceptance is claimed.

### Documentation and limits

[IP-13 schema/compatibility](../modules/IP-13-enemy-patterns.md#schema-и-runtime-contract) covers all ENEMY-001…020 compatibility targets; relevant boss/mid-boss combinations remain encounter-owner work. IP-15/20/21/29 integration contracts, regression guards and STATUS/readiness synchronized. [DECISION-0028](../../decisions/0028-enemy-pattern-lifecycle.md) is a Proposed technical architecture record, not product approval.

No production numbers/art, boss sequences, general pathfinding/formation AI, wave burst policy, or tuning from OBS-01. Production G-14 remains. IP-14 still requires W-01 decisions; IP-12A still has G-17/G-18. Framework completion does not remove those gates.
