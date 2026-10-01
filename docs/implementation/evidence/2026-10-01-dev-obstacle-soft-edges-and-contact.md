# Dev obstacle edge and contact work

Scope: FIELD-DEV-BLOBS, 120 × 120 test map; production FIELD-002 remains unchanged. User requested soft borders for all twenty illustrations, tighter collision correspondence and an assessment of memory cost, then reiterated comparable world scale for ropes and bells.

## Collider correction

The previous authoring contours describe the original mask studies, while generated art differs in boundary shape. This causes displaced contact, notably on wet-medium, net-large and incense-small. scripts/art/fit_dev_blob_colliders.py authors dense-core contours using each runtime sprite's imported pivot and PPU. Disconnected debris and a 0.12 wu exterior strip are excluded. Dense dust filaments are removed with a 0.20 wu morphological opening; contours are simplified with a 0.14 wu tolerance and stored in the field's authoring JSON; ProductionFieldEnvironmentPresentation is regenerated.

Review: [old red / fitted green contours](../../../Art/Concepts/field-002-ruins-blob-art/collider-fit-review.png). This is an offline diagnostic composition, not a Unity screenshot. Physics and fixed sprite orientation are unchanged.

## Art candidates

Seventeen additional soft-edge candidates supplement the three previously selected ones. Their prompts are in Art/Concepts/field-002-ruins-blob-art/soft-edge-all-generation-plan.json. Additional detail-scale edits target the large and upright nets and the large incense patch. Final candidate paths are in soft-library-candidates.json; the equal-world-scale review is soft-library-world-scale-review.png. All were generated with the built-in image_gen. The user subsequently selected this exact set with «подключай новый арт». Art/Packets/field-dev-blobs-soft-edges-2026-10-01.json was validated and applied through art_pipeline: seventeen runtime images were replaced, while the three existing soft-edge images retained their bytes. All twenty PNGs match their approved inputs; all twenty meta hashes are unchanged. Collider contours were refitted against the newly installed artwork and content was regenerated.

## Memory findings

The current twenty runtime PNGs total 33.03 MiB on disk. Their combined source pixel count corresponds to 120 MiB in RGBA32 before Unity's max-size resizing and allocation overhead. This is a calculated estimate, not a profiler measurement. One 2170-pixel-wide image exceeds the 2048 import cap, so actual imported storage is slightly smaller.

SpriteAssetImportPostprocessor explicitly sets Uncompressed, isReadable=false and mipmapEnabled=false. FixtureSpriteCatalog.CreateFor invokes LoadConfiguredSprites, which Resources.Loads every registered sprite before filtering requested IDs. Therefore the loading boundary is broader than the selected map. Addressing file compression alone would not address resident texture cost.

Suggested next implementation: retain high-resolution masters outside Assets; derive runtime images at sufficient gameplay resolution, with smaller resolution budgets for small patches; use platform-supported GPU compression after soft-alpha quality review; load and release environment assets by selected field. Field-specific atlases can reduce texture switches, but do not themselves guarantee less memory and must not become one atlas containing every map. Addressables is an option for the field loading boundary, not a prerequisite for drawing these twenty sprites.

Official reference: [Unity GPU format fundamentals](https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/textures/textures-getting-started/texture-compression-formats/fundamentals), [platform format selection](https://docs.unity.com/en-us/engine/6000.0/manual/materials-and-shaders/textures/textures-getting-started/texture-compression-formats/texture-choose-format-by-platform). Unsupported formats can fall back to uncompressed storage. No texture-loading or compression policy was changed by this task.

## Checks

- Python collider regression tests: 2/2 PASS; concave bay, disconnected debris, pivot and twenty configured contours.
- Content generation/check: UP TO DATE / STATIC PASS.
- Approved new packet: art_pipeline PLAN then APPLIED (72 managed files); subsequent dry-run is expected to be idempotent.
- Manifest validator: PASS, 293 owner/role records.
- All twenty runtime PNG hashes match approved candidates, and all twenty existing meta files remain byte-identical.
- Unity art check: NOT RUN / INCOMPLETE because a Unity process has no identifiable project path. No batch Editor was launched over it.
- Offline fitter requires Pillow, NumPy and SciPy (available in the task environment).

Unity collision behavior and target-scale visual acceptance are still to be observed in the game. Execution status belongs only in STATUS.md.
