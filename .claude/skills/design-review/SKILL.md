---
name: design-review
description: "Read-only review of a design section (Game_design.md / Content_design.md section, an IP module spec, or a DECISION draft) for completeness, internal consistency and implementability against this repo. Use before implementing an IP or approving a decision."
argument-hint: "[doc-path#section | IP-NN | DECISION-NNNN]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `design-review`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only. **Never edit `docs/Game_design.md` or `docs/Content_design.md`** — report proposed changes as text; they need explicit user approval (AGENTS.md: design is canonical, do not silently invent rules).

## 1. Resolve the target
- `IP-NN` → `docs/implementation/modules/IP-NN-*.md` plus its `STATUS.md` entry; read only the Game Design sections and content IDs its Context section lists.
- `doc-path#section` → that section only (`docs/Game_design.md`, `docs/Content_design.md`, `docs/UI  UX Design.md`, `docs/art/*`). Ignore the `* v2.md` duplicates unless the user names them.
- `DECISION-NNNN` → the file in `docs/decisions/` and the documents it says it affects.

## 2. Checks
- **Completeness**: is every rule stated with numbers, ranges, states and triggers? Missing edge cases (pause, death, run end, simultaneous events, empty pools, cap reached)?
- **Internal consistency**: same entity/term with different values or names inside the section; formulas whose inputs are undefined; contradictory rules.
- **Cross-document**: conflicts with other Game Design/Content Design sections, with other IP modules (scope/out-of-scope overlap, dependencies not reflected in `STATUS.md`), or with existing `docs/decisions/` entries.
- **Content gate**: every referenced ID exists; Draft entities are not treated as production content; IDs are stable and used consistently (`CHAR-`, `PASSIVE-`, `SKILL-`, `ENEMY-`, `BOSS-`, `SET-`).
- **Implementability**: can it be built on the current architecture (JSON config per DECISION-0009, composition root, pooling, pause-aware time)? What contract/data shape is missing? Is behaviour testable (acceptance criteria measurable)?
- **Config vs code**: are tunable values expressed as data (JSON) rather than implied constants?
- **Setting boundary**: runtime contracts stay setting-neutral; thematic content belongs to the IPs `STATUS.md` names.

## Output
```
## Design review: <target>
Verdict: READY | NEEDS REVISION | BLOCKED (decision or content gate missing)
### Blocking issues        (quote the sentence, cite doc path + section, say why)
### Recommended changes    (text only, as proposal)
### Questions for the user (decisions only the user can make)
### Implementability notes (what the code needs)
### Checked / not checked
```
