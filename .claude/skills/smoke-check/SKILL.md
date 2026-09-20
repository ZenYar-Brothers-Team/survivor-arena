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
1. **Interactive Unity Editor open on this project** (check for a `Unity.exe` process whose command line contains the project path and does **not** contain `-batchMode`, e.g. `Get-CimInstance Win32_Process -Filter "Name='Unity.exe'"`). **Do not run `scripts/Test-Unity.ps1`** — batch mode over a live Editor has crashed it and corrupts state. Instead:
   - If the UnitySkills server answers `GET http://localhost:8090/health` (project name must be `survivor-arena`): use `POST /skill/test_run` with `{"testMode":"EditMode"}` then poll `test_get_result` with the returned `jobId`. PlayMode via REST needs the user to enable Bypass mode (`MODE_FORBIDDEN` otherwise) — ask the user; do not work around it.
   - Otherwise ask the user to close the Editor or to run the Test Runner and paste the result. Report **NOT RUN**.
2. **No interactive Editor**: run `scripts/Test-Unity.ps1` (EditMode then PlayMode, `-batchmode -nographics`, results in `TestResults/*.xml`). Check its exit code and read the XML counts; a script that "exits 0" without result files is a failure to investigate, not a pass.

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
