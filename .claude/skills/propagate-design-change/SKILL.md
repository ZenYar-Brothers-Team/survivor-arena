---
name: propagate-design-change
description: "Read-only impact analysis after a design/content document changes: finds IP modules, DECISION files, STATUS evidence, code, tests and JSON that depended on the old text and may now be stale. Use after the user edits Game_design.md or Content_design.md."
argument-hint: "[doc-path | 'git-diff']"
user-invocable: true
allowed-tools: Read, Glob, Grep, Bash
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `propagate-design-change`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only. Do not edit design docs, `STATUS.md`, code or JSON; produce a report and let the user decide (AGENTS.md: synchronize affected documents in the same change, record cross-layer deviations in `docs/decisions/`).

## 1. Find what changed
- Argument is a doc path: `git log -p -n 3 -- <path>` (and `git diff` for uncommitted edits) to see the changed sections.
- `git-diff` or empty: `git diff HEAD -- docs/` for uncommitted design changes.
Extract per changed section: the rules/IDs/numbers added, removed or altered.

## 2. Find dependents
For each changed rule/ID/number, grep:
- `docs/implementation/modules/*.md` (Context sections, acceptance criteria) and `docs/implementation/STATUS.md` (current entry; follow the affected IP's linked evidence if needed, not the whole archive);
- `docs/decisions/*.md` (a decision whose premise changed);
- other design documents (Game Design ↔ Content Design ↔ UI/UX ↔ art docs);
- code: `Assets/Game/**` (constants, comments citing the section), tests asserting the old numbers, and `Assets/Resources/Content/**/*.json`.

## 3. Classify each dependent
- **Unaffected** — still valid.
- **Needs review** — may be stale; say what assumption to re-check.
- **Likely stale** — contradicts the new text; says how (old value/new value, `path:line`).
- **Status impact** — an IP marked Implemented/Verified whose acceptance criteria or evidence no longer match; propose the status/evidence edit as text (Verified needs real test evidence, not text edits).

## Output
```
## Design change impact: <doc>
Changes detected: <N sections> (list)
### Likely stale        (file:line → old vs new)
### Needs review
### Unaffected
### Status impact (STATUS.md)
### Suggested next steps  (ordered: content/JSON, code, tests, docs, DECISION entry if a cross-layer deviation)
```
