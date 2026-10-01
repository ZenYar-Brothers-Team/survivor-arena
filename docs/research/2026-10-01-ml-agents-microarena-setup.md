# Running the MicroArena ML-Agents training locally

Companion to
[2026-10-01-ml-agents-training-snapshot.md](2026-10-01-ml-agents-training-snapshot.md).
That document is a research snapshot only; its code never left a local
uncommitted worktree. This is a fresh rebuild of its best-performing
experiment (the self-contained MicroArena seek/avoid task), not a
byte-identical reproduction — see the scope note in
`Assets/Game/MLAgents/MicroArenaObservationEncoder.cs`. Full FIELD-001
integration is intentionally not rebuilt: the snapshot shows it did not
learn anything with the budget it was given.

## One-time setup

1. Install Python **3.10** specifically — `mlagents==1.1.0` and `torch==2.2.2`
   do not support newer interpreters. On Windows: `winget install --id
   Python.Python.3.10 -e`.
2. From the repo root:
   ```powershell
   py -3.10 -m venv scripts/ml_agents/.venv
   scripts\ml_agents\.venv\Scripts\python -m pip install --upgrade pip
   scripts\ml_agents\.venv\Scripts\python -m pip install torch==2.2.2 --index-url https://download.pytorch.org/whl/cpu
   scripts\ml_agents\.venv\Scripts\python -m pip install mlagents==1.1.0
   ```
   The venv lives under `scripts/ml_agents/.venv/` (gitignored); nothing
   installs outside the repo.
3. The `com.unity.ml-agents` Unity package (4.0.0, matching the snapshot) is
   already embedded under `Packages/com.unity.ml-agents/` with the one-line
   patch its Match3 sample needs for Unity 6000.6.0f1 (`Object.GetInstanceID()`
   is obsolete-as-error there; replaced with `GetHashCode()` — see that file's
   inline comment). Nothing to do here; Unity resolves it like any other
   package on first open.

## Running a training stage

Two ready-to-use trainer configs under `scripts/ml_agents/`, matching
documented curriculum stages:

- `stage-01-stationary-short.yaml` — stationary threat, short distance. The
  first stage with clear measured learning in the snapshot.
- `stage-02-moving-threat.yaml` — moving threat, mixed start distance. The
  most advanced documented stage; warm-start it from stage 1's checkpoint.

```powershell
scripts\ml_agents\.venv\Scripts\mlagents-learn scripts/ml_agents/stage-01-stationary-short.yaml --run-id=stage01
```

The trainer prints `Listening on port 5004. Start training by pressing the
Play button in the Unity Editor.` and waits. Open
`Assets/Scenes/MLMicroArena.unity` in the Unity Editor and press **Play** —
training starts immediately; progress prints to the trainer console and
TensorBoard logs land under `results/stage01/` (gitignored).

To warm-start stage 2 from stage 1's checkpoint:

```powershell
scripts\ml_agents\.venv\Scripts\mlagents-learn scripts/ml_agents/stage-02-moving-threat.yaml --run-id=stage02 --initialize-from=stage01
```

To try the scene by hand first (no Python trainer, no training) — open the
scene, select the `MicroArena` object's Agent component's **Behavior Type**
and set it to **Heuristic Only** in the Inspector, then press Play: WASD/arrow
keys drive the player directly via `MicroArenaAgent.Heuristic`.

## Curriculum parameters

Every `environment_parameters` entry in the stage YAMLs maps directly to a
`MicroArenaConfig` field, read via `Academy.Instance.EnvironmentParameters`
in `MicroArenaAgent.ResolveConfig()` — adjust `start_jitter`, `threat_amplitude`,
`threat_frequency_hz`, `time_limit_seconds`, etc. per stage, or add a new
stage YAML for a layout the snapshot didn't try (e.g. two threats — not
implemented; the current agent/config only model one).

## Known limitation on this machine

`scripts/check_project.py`'s automated PlayMode batch test runner
(`-nographics`) currently crashes natively on this machine on unmodified
`develop-evg` as well — confirmed by removing this package entirely and
retesting. It is unrelated to this change and does not affect interactive
Play mode (a different code path from the headless batch test runner), so it
does not block training here. See the separate follow-up task for that bug.
