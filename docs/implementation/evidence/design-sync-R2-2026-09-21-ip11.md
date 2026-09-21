# IP-11 — implementation and verification evidence

Date: 2026-09-21. Scope revision: design-sync-R2. Execution status is recorded only in [STATUS](../STATUS.md).

## IP-11

User request «реализуй следующий пункт» explicitly authorized the next Ready packet. IP-07/08/09/10/10A prerequisites were checked in current STATUS; the pre-R2 implementation retained per-set chances, 1–2 component fixture recipes and tick-only abilities. No production set packet or balance change was authorized. Existing Packages/manifest.json modification was preserved.

### Implementation

- Global required `draft.setDraftChance`, independent ordinal checks, success priority, full request snapshots and uniform failed backfill. Removed probability fields from SetDefinition/DTO/fixture recipes. Pure DraftPool requires an injected policy when sets are present; fixture fallback loading belongs to LevelUpDraftRuntime, and Bootstrap injects the configured provider explicitly.
- Recipes enforce 3–6 unique components with level thresholds 1–6. Existing isolated tests now supply three components. Slot-free/level-less acquisition and duplicate exclusion remain in PlayerBuild.
- Real JSON fixtures cover keyed StatBuff, SkillTransform, ActivationProc, RewardProc, LevelHeal and IndependentAttack. OVERCHARGE/RECALL now have real effects; GUARD/STRIKE exercise defense/reward and fixed attacks. Numbers and reused L1 attack templates are explicitly non-production.
- SetEffectAbility owns subscriptions/counters/cooldowns/temporary buffs. SetEffectHost bridges XP, health, skill modifiers and individual pooled executors. Shared skill bonuses add; each key removes only its own contribution. Factory failure and Synchronize failure roll back additions.
- Source snapshot retains set content ID/owner/origin through delayed multiwaves/projectiles. Ordinary activations count once per activation, never once per wave; set/secondary proc sources cannot recursively drive counters. Set attacks use fixed timers and generic damage/outgoing knockback; per-skill transforms, global size/range and action speed do not automatically apply to them.
- UI projects current→next thresholds, highlights the selected component and completed recipes without granting a set. Closest recipes sort first, acquired last, stable ID breaks ties. Partial possession is visible on Pause even with zero fulfilled thresholds. Set cards/acquired list have effect descriptions; acquisition notification and DEV proc/source counters use existing IP-10A surfaces.

### Coverage and verification

Runner: scripts/Test-Unity.ps1, Unity 6000.6.0f1, batchmode/nographics. Fresh Win32_Process check before every invocation found no open interactive Unity Editor. Only `^Game\.` tests count as project evidence; no third-party tests selected.

Final result: **426/426 EditMode, 5/5 PlayMode, 0 failed, 0 skipped**, 2026-09-21. Third-party tests selected: 0. Final EditMode followed the producer-first teardown fix; final PlayMode included its deterministic regression assertion.

Local artifacts: [EditMode XML](../../../TestResults/IP11-EditMode.xml), [EditMode log](../../../TestResults/IP11-EditMode.log), [PlayMode XML](../../../TestResults/IP11-PlayMode.xml), [PlayMode log](../../../TestResults/IP11-PlayMode.log). TestResults remains ignored generated output. Changed JSON syntax, Markdown file links, new C# .meta files and `git diff --check` were also checked.

 EditMode covers lifecycle, damage/death, XP/draft/Book, reroll/banish, active skills, waves, spawn/pool, composition, UI and content loading. New/extended suites:

| Suite | Evidence |
|---|---|
| SetDraftPolicyTests / DraftBackfillTests | 0/1 probability; 0/1/2/3/5 successes; ordinal ordering; 2 ordinary+2 failed half-interval selection; zero ordinary+2 failed; full draft; banish/acquired exclusion |
| SetFrameworkTests / DraftRequestTests | Thresholds, 3-component migration, shared recipes, duplicate/slot invariants; Book same policy; preserved overflow/failed checks on banish and fresh checks on reroll/new request |
| SetEffectTests | Every configured family, keyed composition/remove, source rejection, counter cooldown, temporary buff expiry, pause, terminal, level heal and rollback |
| SetCombatIntegrationTests | Real skill cooldown/additive modifiers; set applicability; source retention in delayed multiwave projectiles; scheduled cleanup; shared host keys and Dispose |
| GameplayUiPresenterTests | Partial possession versus fulfilled count, completes-recipe projection, unchanged requirement and acquired states |
| SetFrameworkSmokeTests | Four real set acquisitions through queued Books; three-slot short pools; UI/acquisition notification; pause; actual composition Shutdown |

During development, an optional DraftOption struct argument caused a compile error and was corrected to nullable. First UI test run failed an obsolete empty-passive-slot assertion after adding required recipe components; corrected fixture expectation. First PlayMode setup directly mutated build, bypassing UI selection notifications; changed the smoke to acquire all sets through the real queue. These were corrected before final checks. A later complete PlayMode run also reproduced a real producer-first scene teardown failure (UI/passive NullReferenceException): script execution order did not guarantee root.OnDisable before player.OnDestroy. Player now notifies the root before clearing Health/Stats with a reentry guard. The four-set PlayMode smoke deterministically shuts down the player first; the regression map records this guard.

### Limits and documentation

No new raster assets or presentation replacement. Automated scene checks verify four simultaneous sets; no new manual artistic review is claimed. Production payload correctness/visual budgets/single-entity caps are IP-19, real potion binding is IP-28. Matrix and schema: [IP-11](../modules/IP-11-set-framework.md#реализованный-framework-contract). Architecture record: [DECISION-0025](../../decisions/0025-set-effect-source-and-ownership.md), Proposed for review, not a new product approval. GDD/CD remain unchanged; G-04/G-05/G-13 remain production gates.

Documentation impact: IP-11 schema/source/UI/compatibility matrix, IP-03 teardown, IP-08 activation/source, IP-10 global policy, IP-19/IP-28 adapter contracts, regression guard, STATUS and consumer readiness. OBS-01 remains open. No tuning from playtest feedback was applied.
