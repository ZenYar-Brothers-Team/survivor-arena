# IP-34 AB-13 — trajectory-search experiment, 2026-09-29

`trajectorySearch/v1` is a separately selectable research profile. It replaces
the old reactive direction scorer with a bounded whole-route search. Existing
profiles remain selectable and retain their scoring. The example is
[`fresh-trajectory-search.json`](../../../scripts/balance/examples/fresh-trajectory-search.json).

The planner evaluates 96 routes over eight simulation seconds in 0.2 s steps,
including the period after collecting XP. Pursuers react to the predicted
player, so indirect routes can lure enemies away and return behind them. Only
the first direction is applied, with replanning every 0.2 simulation seconds.
The search has its own RNG and does not consume gameplay randomness. It reads
actual speed, scaled collision radii, XP amount, pickup radius and drop lifetime.
The bounded trace records the selected route, XP, risk and planning duration.

## Verification

Performed in the isolated `ip34-automation` worktree, Unity 6000.6.0f1:

- Focused EditMode: 48/48, `TestResults/checks/20260929T181458-090417Z/summary.json`.
- Final full graphics smoke: **1013/1013 EditMode + 52/52 PlayMode**, zero failed
  or skipped, `TestResults/checks/20260929T181828-575953Z/summary.json`.
  Content generation and audio integrity checks also passed.
- Python balance runner tests: 18/18. Example JSON parses successfully.
- An earlier all-tests-passing smoke at `20260929T181614-231584Z` was invalidated
  by a concurrent example-config edit. It is not the final verification evidence;
  the repeated full run above has a PASS verdict for the final inputs.

Closed-loop tests use the production `EnemyMovementController(Seek)` to move
enemies independently of the planner's forecast, at 0.1 s integration and 0.2 s
decision intervals. Results are geometric tests, not full Unity physics/combat:

| Scenario | Result |
|---|---|
| XP behind eight pursuers, rotated 0/90/180 degrees | Collected at 2.5/2.3/2.4 s; zero contacts |
| Dense crowd of 25 faster pursuers | Collected at 3.9 s; zero contacts |
| XP behind a wall | Detour completed at 2.6 s |
| No XP, three pursuers, bounded arena | Survived 30 s without contact; full search budget used |

Additional tests cover a crossing projectile in forecast, expiration, incomplete
coverage reset, search repeatability, config bounds and equal full horizons for
nearby-XP/escape candidates. A 200-threat workload stays below the generous
500 ms test ceiling; a measured planning call took 143.52 ms in focused tests.
This is a bounded-work check, not a real-time guarantee at high enemy counts.

## Final production series

Three independent fresh profiles, CHAR-001 / FIELD-001, random gameplay seeds,
random legal drafts, existing 5x game speed, headless and muted. No preset
upgrades, physics acceleration or production balance changes.

Artifacts:

- `TestResults/trajectory-final/fresh-trajectory-search-example/manifest.json`
- `TestResults/balance-build-trajectory-final/build-manifest.json`
- Per-run `automation.json` and `run.json` under the series' `chains` directory.

The development player was built on base commit `52221dc` plus this experiment's
uncommitted source/config changes. The build manifest explicitly records
`dirty: true` and executable/data hashes; it is not a pristine build of that
base commit. Only documentation changed after the final build/checks.

| Chain | Outcome | Simulation seconds | Level | Collected XP | Expired XP | Kills |
|---|---|---:|---:|---:|---:|---:|
| 0001 | Defeat | 84.08 | 2 | 14 | 0 reported | 15 |
| 0002 | Defeat | 91.74 | 1 | 7 | 1 | 13 |
| 0003 | Defeat | 111.99 | 2 | 11 | 1 | 14 |

All three chains completed naturally, with exit code 0 and complete supported
telemetry. No botStuck, incomplete observation, evicted movement samples or
dropped recorder events. The whole serial series took 69.077 wall seconds,
including process startup/teardown. This throughput reflects short losing runs,
not the cost of a successful full-length run.

Trace samples: 83/91/111. Sampled planning-call median / nearest-rank p95 / max
elapsed milliseconds were 13.80/36.27/43.79, 7.85/19.82/28.50, and
6.60/16.55/26.71. These are roughly once-per-simulation-second samples, not all
decisions, not process CPU time and not frame-time/FPS measurements.

All recorded incoming damage came from ENEMY-001/002. No sampled enemy needed
the unsupported linear-motion fallback. Chains 0002/0003 approached the arena
edge near death; chain 0001 died away from the edge. Thus edge drift or complex
boss attacks alone cannot explain all failures. Player movement speed and the
ordinary-enemy speed source were checked against runtime code; this did not
identify a simple speed-contract mismatch.

## Earlier pilot and limitations

The first series is retained separately at
`TestResults/trajectory-pilot/fresh-trajectory-search-example/manifest.json`:
three natural defeats at 89.5/88.5/68.6 s with 13/2/2 XP. That revision stopped
XP-route evaluation at collection while escapes used the full horizon, and
explored only straight escapes when no XP was visible. Both biases were fixed
and regression-tested. Its decision interval was 0.3 s rather than the final
0.2 s. Do not pool the revisions or infer an improvement from unpaired seeds.

The model only sees the observation radius. It does not predict future spawns,
enemy attacks, player weapon hits/kills and new XP, control expiry, enemy-body
interactions, or complex enemy phases. Non-XP pickups are not rewarded. Contact
risk is a geometric proxy, not damage prediction. The test crowd is simpler
than the live spawner/combat system. Supported motion coverage is not complete
world-model coverage.

**Conclusion:** the prototype demonstrates predictive detours in controlled
scenarios, but the production pilot does not establish a usable balancing bot
or superiority over previous profiles. Low kill counts and level 1–2 at death
are consistent with a missing combat/progression objective; that is a hypothesis,
not an isolated cause. Three random runs do not prove the approach impossible.
Further work needs a separately scoped experiment: diagnose model error against
recorded live states and include combat/XP generation, or test learned behavior.
Do not treat this profile's win rate as an estimate of human balance.

Documentation impact: IP-34 contract, execution STATUS, regression map and
balance-runner README synchronized. GDD/CD, gameplay rules, content balance,
visual defaults and audio defaults unchanged. No visual run was requested or
launched for this experiment.
