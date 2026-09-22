# Workflow tools — 2026-09-22

Authorization: пользователь поручил реализовать первые три предложения аудита: подготовку утверждённого арта, быстрые визуальные итерации и единый запуск проверок. Execution status принадлежит [STATUS](../STATUS.md). Решение об исключении для preview: [DECISION-0049](../../decisions/0049-art-workflow-automation-and-preview.md).

## Delivered scope

- `scripts/art_pipeline.py`: packet preflight, explicit approval/hash, immutable source versions, technical copy/fit, provenance/manifest/sprite registration, guarded existing-owner bindings, import overrides, GUID preservation, idempotence and rollback of own writes.
- `scripts/check_project.py`: auto/docs/art/code/full/visual-preview scopes; fresh Editor/process/lock checks; serialized batch/REST routing; bounded waits; strict project test counts; dated evidence and opt-in fingerprint reuse.
- `scripts/Test-Unity.ps1`: compatibility entry point delegates to the same safe runner.
- `scripts/README.md`, packet template, WORKFLOW, Asset Pipeline and applicable rules explain the normal path and the narrow numeric preview exception. No generation service or global model settings changed; no existing regression tests removed.

No live asset packet was applied during this work. Candidate preparation/rollback tests used synthetic specimens in temporary directories. A real approved SKILL-003 icon was used for read-only packet preflight: one provenance normalization planned, no PNG, sprite registry, binding or GUID changes. Existing unrelated world-art changes were preserved.

## Verification

- Final Python tooling checks: `python -m unittest discover -s scripts/tests -v` — **18/18**, 0.283 s. Cases include repeat/no-op, invalid hash and late packet failure, path escape, unmanaged overwrite, explicit replacement/new version, write rollback, concurrent modification, preview boundaries, process-probe failure, project matching, runner serialization, XML counts, first-import metadata handling, fingerprint invalidation and REST filter/mode/job flow.
- `Test-Unity.ps1` PowerShell parser — PASS.
- Real `visual-preview` invocation for `FixtureGroundShadowPresentation.json` — **PREVIEW ONLY**, no Unity launch, no runtime verification claimed.
- Real full run through `check_project.py --scope full`: **658/658 Game.* EditMode, 25/25 Game.* PlayMode, 0 skipped**, Unity **6000.6.0f1**. Owned Unity processes took 84.02 s and 33.98 s respectively. Manifest audit: **83 owner/role records**.
- Results: `TestResults/checks/20260922T195236-740414Z/{EditMode.xml,PlayMode.xml,EditMode.log,PlayMode.log,summary.json}`. Runtime fingerprint for that recorded run: `b7cef31e36f8edb9eba31bee6923e9d80dd63f0bd4e56d0ec57b665f246024f5`.
- Immediate `--scope full --reuse` returned **REUSED PASS** with the original date and no Unity process launch; command elapsed 1.34 s. Subsequent tooling-only refinement for expected first-import `.meta` creation invalidated this receipt's tool fingerprint, as intended, and was covered by the final Python tests. The Unity runtime/config/art inputs were not changed by that refinement; no second full Unity run was needed for the Python receipt policy.

The live runner verification used a closed Editor and batch mode. REST mode/namespace translation, polling and permission refusal were verified with controlled responses; no live Editor was opened solely to exercise that branch. A live REST check remains appropriate when the project is next open, without weakening its mode gates.

## Documentation impact

WORKFLOW §9, Asset Pipeline §19, content-json and visual-presentation rules, smoke-check and helper routing are synchronized. Production content gates and the separate IP-12A density acceptance remain unchanged. The complete smoke also supplies subsequent PlayMode evidence for the preexisting skill world-art changes in the tested working tree.
