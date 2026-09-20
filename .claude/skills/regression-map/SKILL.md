---
name: regression-map
description: "Maintain docs/regression-map.md: which existing tests guard which critical gameplay paths and fixed bugs, and find fixed bugs or new features with no regression test. Use after a bug fix, after finishing an IP, or before a merge to develop."
argument-hint: "[update | check | 'bug <description or PR/commit>']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `regression-suite`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

A regression map is **a curated index of tests that already exist** in `Assets/Game/**/Tests` and `Assets/Game/**/PlayModeTests`; it is not a new test category and it never replaces `STATUS.md` evidence. File: `docs/regression-map.md`.

## Map format
Table per critical path: `Path | Guarding tests (Class.Method) | Kind (EditMode/PlayMode) | Last verified | Notes`.
Critical paths for this game (extend as IPs land): run lifecycle and pause/end; player movement/bounds; damage → death → run end; XP drop → pickup → level-up → draft → apply; active-skill fire/cooldown/level-up; passive apply/rollback; wave director phase/hook/spawn caps; enemy spawn/despawn/pool reuse and `EnemyRegistry` consistency; gameplay composition + `Shutdown()` rollback; UI HUD/observability rebuild; content loading + validation (JSON → definitions).

## update
1. Read the existing map (create it if missing).
2. Glob the tests; for each critical path, verify the listed tests still exist (`Grep` the `Class` and `Method`) — mark missing ones `STALE`.
3. Find paths with no guarding test → `GAP`.
4. Show the proposed diff and ask before writing.

## bug <description | PR | commit>
Find the fix (`git log`, `git show`), identify the behaviour that broke, and search for a test that would fail without the fix. If none: report `NO REGRESSION TEST` and propose (do not silently write) a test outline — name, arrange/act/assert, target file. If one exists: add it to the map under the affected path with a note on the bug.

## check
Read-only summary: number of critical paths, GAPs, STALE entries; recent commits touching `Assets/Game/**` production code without touching any test file (`git log --name-only`), listed as coverage-drift candidates.

Do not run the test suite here — that is `/smoke-check`. Verdict: CURRENT | DRIFT (n gaps) | STALE (n entries).
