# Repository instructions

## Product and execution contract

- Canonical rules: `docs/Game_design.md`; entities/data: `docs/Content_design.md`;
  player flows: `docs/UI  UX Design.md`; art: `docs/art/ART_DIRECTION.md`,
  `docs/art/Art Production.md`, `docs/art/ASSET_PIPELINE.md`; scope: `docs/implementation/modules/`.
  The repository shows implementation state. IPs and code do not override design.
- For IP work follow `docs/implementation/WORKFLOW.md`. `docs/implementation/STATUS.md`
  is the only execution-status/order authority. Respect its user pause/stop boundaries;
  otherwise choose the named IP or first `Ready` in its explicit Execution order.
  Verify prerequisites for the target scope; historical verification is not new evidence.
- Read the selected module and only its Context sections, full referenced content cards,
  necessary code/tests and applicable rules below. Search headings/symbols before reading
  large files; do not reread unchanged material already in context.
- Use `docs/PROJECT_MAP.md` to find the owning code, authoring sources, generated outputs
  and checks. `python scripts/content/read_card.py <ID>` reads a full canonical card.
  For outputs listed in `scripts/content/generate.py:TARGETS`, edit their authoring
  inputs and regenerate; do not patch generated JSON directly.
- Test (dev) fields «Тест NN» (condition `dev`) sit below FIELD-001…010 and are not designed content; see
  `docs/decisions/0132-dev-blob-test-field.md` before adding or changing one.
- Never ship Draft content or invent a missing product rule. Existing approvals persist;
  approved documents do not resolve their remaining TBDs automatically.
- Complete required checks; synchronize STATUS and affected documents in the same change.
  Record cross-layer or user-approved deviations in `docs/decisions/`; technical workarounds
  do not become design. Never duplicate current IP status in specifications or evidence files.
- For art tasks, the AI follows `scripts/README.md`, prepares the art packet, runs
  `scripts/art_pipeline.py` and `scripts/check_project.py` itself, and reports the result.
  The user provides creative direction and any required visual approval; the user does not
  need to run these commands.

## Scoped rules (mandatory when applicable)

Read the matching files explicitly; `.claude/rules` also applies to Codex.
All paths below are relative to `.claude/rules/`. Do not load unrelated rules.

| Work area | Rules to read |
|---|---|
| Any C# production/test code | `csharp-code.md` |
| ActiveSkill, Bootstrap, Character, Combat, Enemy, Movement, Progression, Run | `gameplay-code.md` |
| Content, Pooling, Diagnostics foundation code | `foundation-code.md` |
| Enemy AI | `enemy-ai-code.md` plus gameplay rules |
| UI C#, UXML, USS or any development/debug surface | `ui-code.md` |
| Content JSON, DTO/catalog/schema changes | `content-json.md` |
| Audio code, clips, catalog, provenance or tooling | `audio-code.md` plus C#/content rules when applicable |
| Unity tests or test execution | `unity-tests.md`; execution: `.claude/skills/smoke-check/SKILL.md` (repo-relative) |
| Design, IP specifications, decisions | `design-docs.md` |
| Raster generation/edit/import/replacement/wiring or procedural sprite presentation | `visual-presentation.md`; read `docs/art/ART_DIRECTION.md` and `docs/art/ASSET_PIPELINE.md` (repo-relative) before any raster operation |

Before running Unity tests, use the smoke-check safety procedure; never run batch
mode over an open interactive Editor. Test scope and reruns follow WORKFLOW §9.

Helper descriptions, permissions and attribution: `.claude/README.md`.
Load only helpers needed for the task. Do not scan vendored skill directories or
start extra agents by default. Explicit user scope and required checks take priority
over context savings.
