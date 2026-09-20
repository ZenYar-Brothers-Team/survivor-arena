---
paths:
  - "docs/Game_design.md"
  - "docs/Content_design.md"
  - "docs/UI  UX Design.md"
  - "docs/implementation/modules/**/*.md"
  - "docs/decisions/**/*.md"
---

# Design and specification document rules

Adapted from the upstream `design-docs` rules. Repository rules in `AGENTS.md` and `docs/implementation/WORKFLOW.md` win on conflict.

## Ownership (from AGENTS.md)
- Game Design = general rules; Content Design = concrete entities/behaviour/balance data; IP module = scope, dependencies, acceptance criteria, out of scope; DECISION = approved deviations. `STATUS.md` is the only execution status.
- **Do not edit `docs/Game_design.md` or `docs/Content_design.md` without explicit user approval.** Never silently invent a missing product rule; ask or record a Proposed DECISION.
- Draft content is never implemented as production content. Do not treat the `* v2.md` variants as canonical unless the user says so.

## Quality bar for new or changed specifications
- **Formulas** define every variable, its unit and expected range, and include a worked example.
- **Edge cases** state exactly what happens (pause, death, run end, simultaneous events, empty/at-cap collections) — not "handle gracefully".
- **Dependencies are bidirectional**: if A depends on B, B's document mentions A (and IP dependency lists match `STATUS.md`).
- **Tuning knobs** name the safe range and which gameplay aspect they affect; balance values link to their rationale or formula and become JSON content, not code.
- **Acceptance criteria are testable**: a person or an automated test can decide pass/fail. No hand-waving ("should feel good").
- Concrete content IDs are stable; renaming or migrating an id requires a DECISION.
- Setting boundary: runtime contracts stay setting-neutral; thematic production content is owned by the IPs `STATUS.md` names.

## Authoring process
- Write incrementally: skeleton first, then one section at a time with user approval between sections; write each approved section to the file immediately so decisions persist.
- When a design document changes, synchronize affected IP specs, DECISION files and `STATUS.md` evidence in the same change (`/propagate-design-change`, `/consistency-check`).

## Not adopted from upstream
- The mandatory 8-section template (Overview, Player Fantasy, Detailed Rules, Formulas, Edge Cases, Dependencies, Tuning Knobs, Acceptance Criteria) — existing documents have their own structure; applying it would mean restructuring `Game_design.md`, which needs user approval. Use it as a checklist for new system sections only if the user asks.
