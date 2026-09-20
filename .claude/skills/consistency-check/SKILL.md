---
name: consistency-check
description: "Read-only cross-check that Game_design.md, Content_design.md, IP module specs, STATUS.md and the JSON content under Assets/Resources/Content agree: same entity with different stats/names, formulas with different variables, IDs missing in one place. Use after editing design docs or content JSON."
argument-hint: "[entity ID | system name | 'full']"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `consistency-check`
     (their approach: grep-first against a registry, read full sections only for conflicts).
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only. There is no separate entity registry: the **registry is the set of stable IDs found in `docs/Content_design.md`**, and the implemented data is `Assets/Resources/Content/**/*.json`. Ignore the `* v2.md` duplicates unless the user names them. Never edit design docs.

## 1. Build the ID index (grep-first)
Grep IDs by prefix (`CHAR-`, `PASSIVE-`, `SKILL-`, `ENEMY-`, `BOSS-`, `SET-`, plus any prefixes present) in `docs/Content_design.md`, `docs/Game_design.md`, `docs/Implementation_plan.md`, `docs/implementation/**`, `docs/decisions/**`. Record for each ID: where defined (full card), where referenced.

## 2. Compare
- **Defined vs referenced**: IDs referenced but never defined; defined IDs referenced nowhere; renamed/migrated IDs still used in their old form (see `docs/decisions/` for migrations).
- **Same value, different documents**: for the IDs in scope, compare numbers (HP, damage, cooldown, duration, counts, level thresholds) and names between Content Design, Game Design, IP specs and DECISION files. Read a full section only when a mismatch needs context.
- **Formulas**: same formula written with different variables, units or constants in two places.
- **Implemented vs documented**: for each production (non-`FIXTURE-`) content JSON entry, does a matching, non-Draft Content Design card exist and do values agree? Fixture entries are exempt but must not use production IDs.
- **Status text vs repository**: `STATUS.md` claims (Verified/Implemented, test counts, evidence paths) that contradict files that exist.
- **Terminology**: same concept under two names (e.g. slot vs pool, phase vs wave) across documents.

## 3. Report
```
## Consistency check: <scope>
Documents/files scanned: <list>
### Conflicts        (ID → value A [doc:section] vs value B [doc:section/json:path])
### Missing / orphan IDs
### Stale status evidence
### Terminology drift
### No issues found in   (what was checked and clean)
### Not checked
```
For each conflict say which document is canonical per AGENTS.md (Game Design = rules, Content Design = entities, IP = scope, repository = implementation state) and propose the resolution as text only.
