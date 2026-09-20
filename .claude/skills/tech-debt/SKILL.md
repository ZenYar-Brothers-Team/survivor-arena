---
name: tech-debt
description: "Maintain the technical-debt register at docs/tech-debt-register.md: scan the code for debt indicators, add an item, re-prioritize, or report. Use to record 'do it later' items instead of losing them."
argument-hint: "scan | add | prioritize | report"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash, Write, Edit, AskUserQuestion
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `tech-debt`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

The register is `docs/tech-debt-register.md`. It is **not** module status — execution status stays only in `docs/implementation/STATUS.md`. Debt items that block an IP are referenced from STATUS, not duplicated there.

If no subcommand is given, print the usage above and stop.

## Register format
One table per category, one row per item:

`ID (TD-NNN, never reused) | Title | Files | Impact if left | Effort (S <1d / M 1–3d / L 3–7d / XL >1w) | Priority (P1 blocks / P2 soon / P3 someday) | Added (date) | Source (PR/DECISION/comment link)`

Categories: **Architecture** (wrong abstraction, coupling, root init/teardown), **Code quality** (duplication, complexity, naming), **Test** (missing/weak/flaky tests), **Docs** (stale docs, ADR not updated), **Dependency** (packages, obsolete APIs), **Performance**, **Content/Config** (hardcoded tuning values, DTO defaults).

## scan
Read-only search, then propose entries (do not write until confirmed). Indicators:
- `TODO`, `FIXME`, `HACK`, `WORKAROUND`, `[Obsolete]`, `#pragma warning disable` in `Assets/Game/**`.
- Repository-rule violations listed in `AGENTS.md`: tuning literals or `[SerializeField]` defaults, local `Validate*` helpers, `new GameObject`/`Destroy` on hot paths, missing `Shutdown()`, multi-type files, missing `PerfGuard`.
- Very large files/methods; classes with many responsibilities.
- Skipped/ignored tests; production classes with no test file.
- Accepted-but-deferred decisions recorded in `docs/decisions/` or PR notes (for example "double `_executor.Tick()` accepted").
Show the candidate list with evidence (`path:line`) and ask which to add.

## add
Ask (plain text) for description, files, impact; use AskUserQuestion for category, effort and priority. Show the row, then ask "May I append this to docs/tech-debt-register.md?" before writing. Create the file with the format above if it does not exist.

## prioritize
Re-read the register, verify each item still exists in the code (drop or mark resolved otherwise), propose new P1/P2/P3 with reasons, and ask before rewriting.

## report
Summarize counts per category/priority, the oldest open items, P1 items, and items that block upcoming IP modules per `STATUS.md`. Read-only.

Always finish with a verdict line: COMPLETE, BLOCKED (user declined a write) or FAIL (bad usage).
