---
name: smoke-check
description: "Run the project's EditMode and PlayMode test suites safely and report an honest PASS/FAIL with counts. Handles the 'Unity Editor already open' case (never launches batch mode over an interactive Editor). Use before marking an IP Verified or merging."
argument-hint: "[editmode | playmode | all]"
user-invocable: true
allowed-tools: Read, Bash, PowerShell, Glob, Grep
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `smoke-check`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Goal: produce **real** test evidence. Never report a pass you did not observe; a skipped run is "NOT RUN", not PASS.

## 1. Choose the runner (in this order)
Use `python scripts/check_project.py --scope full` for full smoke, `--scope art` for art import/catalog checks, or `--scope code --platforms EditMode --filter '^Game\.Combat\.'` for a known affected subset. Read [runner usage](../../../scripts/README.md) for custom paths and scopes. IP acceptance determines which scope is required.

The runner performs a fresh process/lock preflight before every launch. Open interactive Editor → UnitySkills REST on the matching project; closed Editor → installed Unity batch. Never run raw batch over an interactive Editor. `Test-Unity.ps1` now delegates to this same safe runner. Inability to inspect processes is **NOT RUN**, not evidence of a closed Editor.

PlayMode through REST still requires user-enabled Bypass. A mode/grant refusal must be surfaced; never fall back to another control channel to bypass it. Unavailable REST requires the user to close the Editor or run Test Runner manually. The runner never closes an interactive Editor.

Results and compact summaries are saved under `TestResults/checks/<timestamp>/`. Zero tests, failed/skipped/inconclusive tests or absent result files cannot pass. An explicit `--reuse` may return **REUSED PASS** with the original verification date when runtime/config/assets/tool hashes and evidence hashes match; never call it a new run. Visual-preview/static scopes do not verify a module.

## 2. Interpret results
- Count only `Game.*` tests as project evidence; tests from third-party packages in the same run are reported separately.
- Total, passed, failed, skipped per platform. Any failed test: list its full name and message; do not summarise as "some failures".
- Compile errors (`CS####`) or a run that never left "starting": report NOT RUN with the log excerpt from `Logs/Editor.log` / `TestResults/*.log`.

## 3. Critical-path check (from the tests that ran)
Confirm these are represented among passed tests: run lifecycle, damage/death, XP → level-up → draft, active skill, wave director, enemy spawn/pool, composition root, UI presenter, content loading, and the PlayMode gameplay smoke (`GameplaySmokeTests`). Missing coverage → point to `/regression-map`.

## Output
```
## Smoke check
Runner: UnitySkills REST | Test-Unity.ps1 | NOT RUN (reason)
EditMode: <passed>/<total> (<skipped> skipped)    PlayMode: <passed>/<total>
Third-party tests in run: <n passed / n failed>
Verdict: PASS | FAIL | NOT RUN
### Failures            (full name, message)
### Critical paths      (covered / not seen)
### Evidence line for STATUS.md   (date, Unity version, counts) — only if PASS
```
Never edit `STATUS.md` from this skill; give the evidence line to the user or the calling workflow.
