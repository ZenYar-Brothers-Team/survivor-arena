---
paths:
  - "Assets/Resources/Content/**/*.json"
---

# Content JSON rules

Adapted from the upstream `data-files` rules. Files here are the authoring layer for gameplay content ([DECISION-0009](../../docs/decisions/0009-json-content-config.md)); domain types keep all validation. Repository rules win on conflict (see "Deliberately different" below).

## Format and naming
- Valid JSON only (no comments, no trailing commas) — invalid JSON blocks loading; the `validate-content-json` hook checks syntax after every edit.
- Property names are camelCase; enums are strings (`StringEnumConverter`).
- One file per content category under `Assets/Resources/Content/<Category>/` (`Enemies/`, `Characters/`, `Passives/`, `ActiveSkills/`, `Waves/`, `Sets/`, `Presentation/`), named `Fixture<Thing>.json` for placeholder sets. New category = new subfolder + `Fixture*Catalog`/`*Catalog` in the owning module.

## Schema is documented in code
- The DTO class (`Assets/Game/<Module>/**/Json/*Data.cs`) **is** the schema; the catalog maps it through validating domain constructors. Adding a JSON field = DTO field + catalog mapping + domain validation + test in the same change. Unknown properties are errors (`MissingMemberHandling.Error`).
- Meaning of numbers (units, safe ranges, what they affect) is documented in `docs/Content_design.md` (or the DTO/domain XML comment for fixture-only fields) since JSON has no comments.

## Values
- Every field a catalog marks required (`float?`/`int?` DTO fields, per-kind fields) must be present — never rely on a code default; a missing one throws by name at load.
- **No tuning defaults** anywhere. The only defaults are neutral ones ("leave unchanged": 1x multiplier, 0 bonus) owned by a single domain type (e.g. `WaveEnemyModifiers.Identity`).
- Seeds and spawn geometry are content and live here.

## Integrity
- **No orphaned entries**: every entry is referenced by a catalog/registry, another JSON (`enemyId`, sprite id, skill id), or a test. Cross-references must resolve — the content registry validates them; keep `FixtureRuntimeContentCatalogTests` current.
- IDs are stable and never reused for unrelated entities. The user-approved replacements in DECISION-0015 (SKILL-001, PASSIVE-002/007, SET-001…008) are explicit semantic migrations, not permission to silently reuse other IDs. Keep content/build revisions and old-versus-new meaning distinguishable in saves, telemetry and balance evidence; do not reinterpret historical data as the new card. `FIXTURE-*` ids are non-production placeholders; never use a production id (`CHAR-`, `SKILL-`, `PASSIVE-`, `ENEMY-`, `BOSS-`, `SET-`) for fixture data, and never add production content from a Draft card in `docs/Content_design.md`.
- Breaking schema changes (renamed/removed field or id) migrate all existing JSON, tests, and docs in the same change and get a DECISION when an id changes.
- After editing values run the affected EditMode tests (`/smoke-check`) — tests assert configured values. The user-approved numeric-only visual-preview loop in `scripts/README.md` may defer those tests until a visual variant is selected; it is restricted to the four listed presentation profiles and never counts as runtime verification. Gameplay values, schema/ID changes and final IP acceptance retain their checks.

## Deliberately different from upstream
- Upstream: snake_case file names (`[system]_[name].json`) — here files are `Fixture<Thing>.json` (PascalCase), matching existing content and `.meta` GUIDs.
- Upstream: "include sensible defaults for all optional fields" — here the opposite (no tuning defaults; see Values).
- Upstream: "version data files on breaking changes" — no schema-version field exists here; migrations are done in-place with the catalog change.
