# Repository helpers

`AGENTS.md` contains the always-relevant contract and routes each task to the
matching files in `rules/`. Those rules also apply to Codex: read the relevant
files explicitly; do not assume that Claude's `paths` frontmatter loads them
in another tool. Paths in frontmatter are relative to the repository root.

Load only the skill needed for the current operation. A module does not require
every review skill, a full skill-directory scan, or an agent per review.
An explicit user request for a broader audit still defines the scope.

## Project skills, rules and hooks

Repository-specific helpers are read-only unless stated; none replaces
`docs/implementation/STATUS.md` or the canonical design documents.

- `/conventions-review`, `/perf-audit`, `/test-quality-review` — review code,
  hot paths and tests against the applicable `rules/` and `docs/decisions/`.
- `/smoke-check` — safe EditMode/PlayMode execution; never batch mode over an
  open interactive Editor.
- `scripts/art_pipeline.py` prepares explicit approved art packets; `scripts/check_project.py`
  selects scoped checks and the safe Unity runner. For the numeric-only visual-preview
  loop and command examples, read `scripts/README.md`; no extra review skill is required.
- `/regression-map`, `/tech-debt` — maintain `docs/regression-map.md` and
  `docs/tech-debt-register.md` (writes only after confirmation).
- `/design-review`, `/consistency-check`, `/content-audit`,
  `/propagate-design-change`, `/balance-check` — cross-check design, IPs,
  decisions and JSON; never edit Game/Content Design without user approval.
- `/asset-audit` — checks runtime art against `docs/art/ASSET_PIPELINE.md`.
- `/architecture-decision` — guided authoring or retrofit of a decision,
  Proposed only; checks conflicts against existing decisions.
- `hooks/validate-content-json.ps1` rejects syntactically invalid content JSON
  after an edit. Hook configuration stays in `settings.json`.

Read a module's current entry in STATUS first. Follow its evidence link only
when implementation details, coverage or a historical comparison are needed;
do not load the whole evidence archive. Historical records are dated facts,
not current approval, status or content gates.

Adapted helpers retain upstream attribution in `skills/THIRD_PARTY_NOTICES.md`.
