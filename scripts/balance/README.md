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

## Record human movement demonstrations (AB-14)

This records **raw state/action pairs**, not video, deterministic replay, or a
trained bot. Use an up-to-date isolated development player. Launch explicitly:

```powershell
python scripts/balance/record.py --player TestResults/balance-build-demonstration/balance.exe
python scripts/balance/validate_demonstration.py TestResults/demonstrations/<human-experiment>
```

The launcher copies [`examples/human-demonstration.json`](examples/human-demonstration.json)
to a unique config/output folder under `TestResults/demonstrations`. It opens a
windowed, muted 1× player. Add `--audio` only if wanted. Each run starts paused:
Space, Escape or Continue resumes normal keyboard/mouse movement. Losing focus
pauses again. Draft choices are automatic `activeFirst15`: through player level 15,
an offered active skill is chosen uniformly; if none is offered, any offered
option is chosen uniformly. From level 16, all offered options are equally likely.
This recorder is for movement, not human build decisions. The example allows up to five runs in
one isolated fresh profile history and **no automatic meta purchases**. It never
reads/writes your regular profile or settings. To record another starting
progression, pass `--template` with a validated `preset` human config.
The development HUD may show faster speed buttons, but the human recorder
immediately restores 1× if one is pressed so the recording remains valid.

Close the game window to end the session cleanly; normal in-game exits are
recorded as administrative aborts, not defeats. The game flushes its recording
before reporting completion. Ctrl+C in the runner or forced process termination
can leave `.partial` data; keep it for diagnostics, not unqualified training.
Manual pauses have no short bot timeout, but the example's 30-minute wall budget
still includes paused time. Every new run waits for you to resume it.

Each run adds `demonstration.jsonl`: header with controller/config/build/profile
provenance, ordered samples, and one terminal footer. While open or incomplete
it is `demonstration.jsonl.partial`; only a clean non-empty stream is promoted.
`automation.json` contains a recording summary; normal telemetry, draft history,
seeds, outcomes and profile snapshots remain alongside it. Bot runs may opt into
the same recorder for technical tests and are explicitly labeled `controller=bot`.

Samples are taken before running physics steps (default every 0.2 simulation s,
plus every movement-intent change): player
state/stats, current skill levels and remaining cooldowns, threats, pickups,
obstacles, beams, enemy movement/attack phase, build and camera/arena bounds.
`action` is clamped world-space analog movement intent, not displacement or
knockback. It applies for `stepSeconds`, **not** until the next sampled frame.
`previousAction` means the preceding running physics step; `physicsStep` and
`physicsSeconds` count running physics separately from render-updated `runSeconds`.
Paused frames are omitted; idle actions while running are retained. This sparse
dataset cannot reconstruct every intervening input and is not a replay.
The new header has `samplingPolicy=periodicOrActionChange/v1`; each sample's
`captureReason` marks an interval sample, a direction change, or both. Older
recordings without those fields remain valid. The revised example allows
40,000 samples and 512 MiB per run; those are failure limits, not targets.

The explicit `demonstration` limits bound samples, UTF-8 file size (MiB), queue
length and each entity collection. Queue/file/sample exhaustion fails the run
and keeps a partial file instead of silently dropping samples. Entity caps and
known unsupported hazard observations are flagged per sample and counted in the
footer. The validator reports them without claiming completeness or expert skill.
Validate and inspect real human recordings before dataset selection/training;
automated fixtures and bot pilots are **not** human demonstrations. No learner,
ML package or balance change is installed by this recording feature.

## Offline movement imitation experiment

`train_imitation.py` fits a research MLP on clean, complete human JSONL only.
It requires NumPy and scikit-learn in the local Python environment. Pass at
least three distinct completed `demonstration.jsonl` files and an ignored
output directory, for example:

```powershell
python scripts/balance/train_imitation.py <run1.jsonl> <run2.jsonl> <run3.jsonl> --output TestResults/imitation-human-v1
```

The script validates every stream, rejects bot/partial/truncated/incomplete
recordings and duplicate run IDs, then holds out each entire run in turn.
`evaluation.json` compares nine-direction action accuracy, macro F1 and
direction-change accuracy against repeating the previous action. `model.json`
contains the fitted network and input hashes for reproducibility. The model is
**not** loaded by the game or selected as a bot policy: offline action matching
does not establish closed-loop survival, XP collection or balance quality.

To diagnose movement changes, use the separate two-stage research probe on at
least three naturally completed human recordings:

```powershell
python scripts/balance/probe_two_stage_imitation.py <run1.jsonl> <run2.jsonl> <run3.jsonl> --output TestResults/imitation-two-stage/evaluation.json
```

It holds out each run, trains a change gate and a direction model on the other
runs, and compares with repeating the previous action. The report also gives
change detection AUC and direction accuracy with oracle timing. It does not
serialize or select an in-game policy.

For a temporal diagnostic on the same completed recordings, pass earlier
observations to the two-stage probe:

```powershell
python scripts/balance/probe_temporal_imitation.py <run1.jsonl> <run2.jsonl> <run3.jsonl> --lags 0.4 1.2 --output TestResults/imitation-temporal/evaluation.json
```

Each lag uses the last sample at or before that many running physics seconds
ago, including the action known then. Missing history is marked explicitly.
The current action, future state and event capture reason are excluded from
features. The result still requires independent in-game validation before any
policy selection.

## Analyze experiments

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
