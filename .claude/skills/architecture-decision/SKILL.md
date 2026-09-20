---
name: architecture-decision
description: "Guided authoring of a DECISION record in docs/decisions/ (context, alternatives considered, decision, consequences, approval) with a conflict check against existing decisions. Also 'retrofit' mode to add missing sections to an existing record. Use when AGENTS.md/docs/decisions/README.md say a decision record is required."
argument-hint: "[short title] | retrofit docs/decisions/NNNN-title.md"
user-invocable: true
allowed-tools: Read, Glob, Grep, Write, Edit, AskUserQuestion
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `architecture-decision`.
     Their registry/engine-reference/director-gate machinery is intentionally not used.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Records live in `docs/decisions/NNNN-short-title.md`, follow `docs/decisions/_template.md`, and are written **in Russian** like the existing ones. Numbers are never reused. **A record with status `Proposed` changes no canonical document.** Only the user can set `Approved`; never mark it Approved yourself and never edit `docs/Game_design.md` / `docs/Content_design.md` from this skill.

## 0. Mode
- Argument starts with `retrofit <path>` → retrofit mode (section 7).
- Empty argument → ask: "What decision are you documenting? Give a short title (e.g. `wave-director-timeline`)."
- Otherwise the argument is the title.

## 1. Load context (always first)
Read `docs/decisions/README.md` (when a record is required), `docs/decisions/_template.md`, `AGENTS.md` and the scoped rules it routes for the affected files, and the list of existing records (`Glob docs/decisions/*.md`; read titles/Status/Context of the topically related ones, e.g. config → 0009, health → 0006, pooling → 0011, waves → 0014). Read the related IP entry in `docs/implementation/STATUS.md` and its module spec if the title names one.

Unity is 6000.6.0f1, newer than the model's training data: when the decision depends on a Unity API or package behaviour, verify it against the installed package source/`Library/PackageCache`, the compiler, or a test — do not rely on memory — and put unverified points under "Risks".

## 2. Is a record warranted?
Per README, a record is for: a new system rule, a forced deviation from acceptance criteria/data contract, a changed responsibility between Game Design / Content Design / IP, a changed or migrated stable content ID, or a user-approved contested decision across layers. Ordinary technical detail or local refactoring is **not** recorded. If it does not qualify, say so and stop.

## 3. Next number
Highest existing `NNNN` + 1, zero-padded to 4 digits.

## 4. Conflict check (blocking)
List the existing decisions and repository rules that constrain this topic as **locked constraints** ("must not contradict"). If the proposal contradicts one, surface it before any drafting:
> "Conflict: this proposes X, but DECISION-NNNN established Y. Options: (1) align with it, (2) supersede DECISION-NNNN explicitly (that record gets `Superseded by` status), (3) justify an exception."
Do not continue until resolved or accepted as an explicit exception.

## 5. Confirm assumptions (AskUserQuestion, not open-ended)
Derive first, then ask confirm/adjust: problem in one sentence; 2–3 concrete alternatives; related IP and content IDs; affected layers (Game Design / Content Design / IP / code / tests / config); dependencies on other decisions; status = `Proposed`. Design questions that are not assumptions (data shape, ownership, contracts) are asked separately after the assumptions are confirmed. Do not draft until confirmed.

## 6. Draft
Use the template headings (`Status`, `Date`, `Related IP`, `Related content IDs`, `Context`, `Decision`, `Consequences`, `Approval`) and add these sections when they carry information (skip empty ones):
- **Alternatives considered** — per option: description, pros, cons, why rejected.
- **Consequences** split into: documents to update if approved, code/tests/config affected, accepted trade-offs, risks with mitigation.
- **Migration** — how existing code/data/IDs get from here to there.
- **Validation** — how we will know the decision was right (tests, PerfGuard thresholds, evidence for `STATUS.md`).
- **Related decisions** — links.
`Approval` states "Pending — not approved" until the user approves; then it records who and when (written by the user or after their explicit approval in chat).

Show the full draft and ask "May I write `docs/decisions/NNNN-title.md`?" Write only after a yes.

## 7. Retrofit mode
1. Read the existing record completely; scan headings against the template (+ optional sections above).
2. Report present vs missing (`Status` and `Approval` missing = blocking; `Alternatives considered`, `Validation` = recommended).
3. Ask "Shall I add the N missing sections? Existing content will not be modified."
4. Fill missing sections by asking the user or deriving from the code/history; append with Edit. **Never modify or delete existing text**, never change a `Status` on your own.

## 8. After writing
Tell the user which documents must change when the record is approved (design docs, IP spec, `STATUS.md` link under the related IP, `AGENTS.md` rule if it is a coding convention, code/tests). Do not make those edits until the record is approved.

Verdict: COMPLETE (written as Proposed) | BLOCKED (conflict unresolved or write declined) | NOT WARRANTED.
