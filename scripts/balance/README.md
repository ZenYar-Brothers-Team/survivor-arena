# Automated balance runs (IP-34)

The experiment contract is versioned JSON. See [`examples/fresh.json`](examples/fresh.json)
and [`examples/preset.json`](examples/preset.json). To select the XP-focused
movement bot, set `movementPolicy.id` to `experienceFocused` (version 1); the
original `safePickup` remains available. The IDs are separate research
profiles, not an in-game toggle. For a wide visible-XP detour use
[`examples/fresh-orbit-experience.json`](examples/fresh-orbit-experience.json):
`orbitExperience/v1` selects one XP target and, when the direct corridor is
dangerous, keeps a waypoint on the safer side. `arcOffsetWorldUnits` is the
sideways displacement (2–10 world units; 6 is roughly 0.6 of the Gameplay
camera height). It is still a research bot profile, not a gameplay change.
For a crowd-luring pattern use
[`examples/fresh-herd-loop.json`](examples/fresh-herd-loop.json): `herdLoop/v1`
switches between local foraging, luring enemies away from a remembered XP
area, sweeping around them, and returning. Its report includes
`movementModeDecisions`; high expired XP means the pattern did not actually
solve collection. It is not an in-game toggle or a human-play model.
For diagnostic runs use
[`examples/fresh-herd-loop-adaptive.json`](examples/fresh-herd-loop-adaptive.json):
`herdLoopAdaptive/v1` first favors XP with an open route, abandons a still
blocked bank after a bounded sweep, and temporarily avoids retrying it. Both
herd profiles include a bounded `movementTrace` in each `automation.json`
(position, goal, direction, mode/reason, HP, crowd/blockers, score); samples
are taken roughly once per simulation second and at mode changes. This trace
is diagnostic, not a replay or evidence of superior performance.

For whole-trajectory prediction use
[`examples/fresh-trajectory-search.json`](examples/fresh-trajectory-search.json):
`trajectorySearch/v1` evaluates waypoint routes against a moving crowd, scores
XP actually crossed by the predicted pickup radius, contact risk and an exit,
then executes only the first direction. Seek/KeepDistance respond to the
predicted player. Other enemy movement remains linear and is counted in the
trace. Future spawns, attacks, deaths and control expiry are not simulated.
The required `trajectory` settings bound horizon, integration step, candidate
count, contact penalty and clearance; ranges and objective are in IP-34 AB-13.
Trace entries include the selected predicted path, XP, contact-risk seconds,
planning elapsed milliseconds and approximation coverage. Contact risk is not HP
damage. This profile can be substantially more expensive than the heuristics;
check measured throughput before launching large batches. The initial production
pilot did not establish usable survival or superiority over other bots; see the
[experiment evidence](../../docs/implementation/evidence/2026-09-29-ip34-trajectory-bot.md).

`chains` counts independent profile histories; `maxRunsPerChain` limits runs
within each history. A fresh chain begins
with the production `ProfileCodec.Create()` state. A preset chain starts from a
copy of the referenced profile; completed runs and purchases alter only that copy.

All content identifiers are validated against the production Meta catalog before
launch. The first route field also needs a complete runtime binding. Each output
directory must be new and confined to the runner's experiment root. Runs use
random gameplay seeds. `runSpeed` is the game's existing 1×, 2×, 3× or 5× speed,
not a faster simulation engine.

To prepare a laboratory starting profile, edit
[`examples/starting-profile-declaration.json`](examples/starting-profile-declaration.json),
then run:

```powershell
python scripts/balance/prepare_preset.py --declaration scripts/balance/examples/starting-profile-declaration.json --output scripts/balance/examples/starting-profile.json
```

The output path must not exist. The generator validates IDs, ownership, caps and
recorded upgrade spending from the production Meta catalog. It starts with the
canonical initial unlocks and does not invent run receipts. `currency` and any
extra unlocks are a declared laboratory starting condition, not earned results.
Unity's `ProfileCodec` validates the resulting file again before use. Preserve
the original declaration and generated profile with experiment evidence.

## Build and run locally

Build a separate Windows development player from a closed-Editor worktree. The
builder uses the repository's Unity process/lock preflight and refuses to launch
batch Unity over an interactive Editor. It never replaces the normal game build:

```powershell
python scripts/balance/build.py --output TestResults/balance-build/balance.exe
python scripts/balance/run.py --experiment scripts/balance/examples/fresh.json --player TestResults/balance-build/balance.exe --output TestResults/fresh-field001-example --validate-only
python scripts/balance/run.py --experiment scripts/balance/examples/fresh.json --player TestResults/balance-build/balance.exe --output TestResults/fresh-field001-example
```

`--output` must be new and its final directory name must equal the config's
`outputDirectory`. For a second experiment, use a new experiment ID and output
name. Validation checks the executable and `_Data` hashes against the adjacent
`build-manifest.json`; the Unity worker revalidates content IDs, policy ranges,
profile and config. Source config and preset are hashed and checked again before
each chain. A changed input requires a new run. The runner launches one hidden
child player per independent chain and never touches the production profile or
settings. All profile and settings state stays in memory; only experiment
artifacts are written under `--output`.

By default the worker runs with Unity's `-batchmode` (no visible Player window
or user input) and mutes its own audio listener before scene load. For one
specific experiment add `--visual` to show the game window; it stays silent
unless you also add `--audio`. Neither switch affects your normal game or
system audio. The selected mode is recorded in the experiment and manifest.
For a single visible run, set `chains: 1` and `maxRunsPerChain: 1` in a new
experiment config, use a new output directory, and invoke `run.py` with
`--visual`. Add `--audio` only if sound is wanted for that run.
The visible player uses an isolated windowed SafeWindow video setting
(1280×720 on a desktop at least that large); the normal game's saved display
mode is not read or changed.

`manifest.json` is atomically refreshed during execution. `experiment.json`
captures the requested config and build fingerprint. Each chain has an initial
profile, progress heartbeat and summary. Each run has existing telemetry
`run.json`, typed `automation.json`, profile snapshots and `player.log`.
`completed` means the configured chain/route ended; `partial` means a planned
watchdog/route stop, not a loss; `failed` means process, input, export or save
failure. Ctrl+C stops only this runner's current child, keeps completed files,
and marks the manifest `cancelled`. There are no automatic retries or deletion.
At an experiment wall limit, the Unity worker gets up to 30 additional seconds
to export the current censored run and its chain summary. A hung worker is then
terminated; the manifest records actual elapsed wall time, including this grace.
No new chain starts after the configured limit.

Analyze any finished or partial experiment without filtering away inconvenient
runs. Outputs `analysis.json`, `runs.csv`, `chains.csv`, and `summary.md` in the
experiment directory. A comparison prints Markdown and refuses mismatched
starting profile, route, hero, policies, speed, or budgets:

```powershell
python scripts/balance/analyze.py TestResults/fresh-field001-example
python scripts/balance/compare.py TestResults/baseline TestResults/candidate
```

Win rate uses only naturally completed W/L. Timeouts, crashes, missing/corrupt
reports, and censored milestones remain visible. Runs in a profile chain are
dependent; the v1 summaries are descriptive, with no p-values or paired-seed
claims. Do not infer balance quality from one short smoke run.
