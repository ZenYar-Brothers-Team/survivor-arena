# Working-tree commit audit — 2026-10-02

User explicitly authorized committing all current work, with a check for junk.
Scope includes Academy scheduling/effect art/burst portal/contrast, monastery
typed altar art and mirrored effects, timed player effect bars, removal of the
obsolete dev altar field, generator/schema/tests and associated documentation.
Pending dark Academy floor is a preview outside Assets; its replacement approval
remains open. Its source reference candidates/prompts are useful provenance.
Rejected rocky ground variants remain local and are excluded from this commit.

No logs, TestResults, Library, caches, binaries or temporary helper files are
staged. The added PNGs/source records are art deliverables using existing Git LFS
rules. Whitespace-only importer noise in the FIELD-009 texture meta was removed;
trailing spaces in added Unity metas and Markdown were cleaned. Audit of 221
staged added/modified files passed JSON syntax, conflict-marker, Unity meta/GUID
and temporary-path checks; staged whitespace check passed.

Fresh full graphics attempt: generation and audio PASS; EditMode 1439/1443,
zero skipped, `TestResults/checks/20261002T201017-743272Z/EditMode.xml`.
The same four pre-existing failures prevent full PASS:

- `FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`:
  Vector2 index 1 precision mismatch (printed values both 24, -2.75).
- `MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`:
  expected (11,11,6), actual (11,11,11).
- `MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`:
  expected 5 strings, actual 10.
- `MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`:
  additional SET-023/024/026/030/034.

Separate graphics PlayMode: 4/4 passed, zero failures/skips,
`TestResults/checks/20261002T201356-834608Z/PlayMode.xml`: actual Academy launch,
real five-unit burst portal/camera/pause, nine-effect seals, legacy zones field
and monastery launch. Runner did not issue reusable PASS because inputs changed
during import/checks. No full runtime PASS claimed. Manifest: 334 records PASS;
Python workflow tools: 32/32 PASS. Final shrine/contrast EditMode subset had
25/25 PASS before this broad audit; source values are 20 s for both shrine rewards.
