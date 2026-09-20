---
name: content-audit
description: "Read-only audit of planned vs implemented content: counts and lists Content_design.md entities (characters, skills, passives, sets, enemies, bosses) against JSON under Assets/Resources/Content and IP status, separating fixture placeholders from production content."
argument-hint: "[characters | skills | passives | sets | enemies | bosses | 'full'] [--summary]"
user-invocable: true
allowed-tools: Read, Glob, Grep
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `content-audit`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Read-only; with a saved report only if the user asks (`docs/content-audit-<date>.md`). Do not edit design docs or `STATUS.md` — propose changes as text.

## 1. Planned content (from Content Design)
For each category (prefix → JSON folder):
`CHAR-` → `Characters/`, `SKILL-` → `ActiveSkills/`, `PASSIVE-` → `Passives/`, `SET-` → `Sets/`, `ENEMY-`/`BOSS-` → `Enemies/`, waves/encounters → `Waves/`.
Grep the IDs in `docs/Content_design.md` and note each entity's **status** (Approved / Draft / other marker the document uses). Read a full card only when the status is unclear.

## 2. Implemented content
List entries in `Assets/Resources/Content/<Category>/*.json`. Split into:
- **Fixture** (`FIXTURE-*` ids, `Fixture*.json`): pipeline placeholders — not production.
- **Production** (ids matching Content Design): implemented content.
Also check presentation references (`Presentation/` sprites and motion profiles) resolve for each entity where the config points at them.

## 3. Compare
- Planned production IDs with no implementation → `NOT STARTED`, or `BLOCKED (Draft)` if the card is Draft (Draft must not be implemented as production).
- Implemented production IDs with no Content Design card → `UNDOCUMENTED` (needs a decision).
- Implemented but values differ from the card → point to `/consistency-check`.
- For every gap, name the IP module in `docs/implementation/STATUS.md` that owns it (IP-17…IP-24 own thematic content per the setting-boundary note) rather than saying "missing".

## Output
```
## Content audit: <scope>
| Category | Planned (Approved) | Planned (Draft) | Production impl. | Fixture impl. | Gap |
### Per-category detail   (ID → status → owning IP)
### Undocumented / orphan entries
### Blockers (Draft or missing Approved content)
### Not checked
```
`--summary` prints only the table.
