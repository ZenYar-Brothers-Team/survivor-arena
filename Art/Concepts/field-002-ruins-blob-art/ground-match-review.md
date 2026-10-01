# Ground palette review — 2026-10-01

Visual candidates for FIELD-DEV-BLOBS / «Тест 02», using the actual FIELD-003
ruins ground tile. The user selected the after sheet with «Подключай и комить».
The sixteen selected candidates were installed through the art pipeline.

## Changes

- Wet, ward, net, incense: all four sizes/orientations edited against the actual
  ground reference. Soil, grout and perimeter vegetation now use muted taupe,
  brown-grey and olive tones; yellow ground casts are subdued. Transparent,
  mottled edge pixels form part of each PNG.
- Thorn: all four current masters retained; their ground already blends.
- Internal object arrangement and authored world scale are retained visually.
  Fourteen of sixteen edits have exactly the original canvas dimensions.
  Ward-large differs by one pixel in width; ward-upright by one in height
  (less than 0.1%). Import profiles, pivots and PPU remain unchanged.
- Gameplay contours remain frozen. Do not run the collider fitting script on
  these visual candidates; any additional ground skirt is nonblocking.

## Review and packet

`ground-match-before-actual-floor-review.png` and
`ground-match-after-actual-floor-review.png` show all twenty sprites over the
same actual floor at 13 pixels per world unit (floor repeat: eight units).
These are technical compositions of unchanged input PNGs, not edits to art.
`ground-match-generation-plan.json` records all prompts and the ground reference;
`ground-match-candidates.json` records dimensions and alpha extrema.

The approved replacement packet is
`Art/Packets/field-dev-blobs-ground-match-2026-10-01.json`. Dry run PLAN and
apply APPLIED: 65 files, including sixteen immutable versions, selected masters,
runtime PNGs and provenance records plus the manifest. Runtime paths and IDs
are stable. No `.meta` was changed.

## Checks

All sixteen generated images are RGBA with transparent pixels and nonzero
alpha. All twenty review entries preserve the existing import profile.
Before replacement, `check_project.py --scope art`: PASS, 77/77 EditMode tests and 293 manifest
owner/role records. Receipt:
`TestResults/checks/20261001T151745-789850Z/summary.json`.
That receipt covers the prior installed art and is not verification of this
replacement. After replacement, `check_project.py --scope art`: NOT RUN /
INCOMPLETE, Unity process has no identifiable project path. The safe runner
was not bypassed.

After replacement, `validate-art-manifest.py`: PASS, 293 owner/role records.
`python -m unittest discover -s scripts/tests -p test_dev_blob_collider_fit.py -v`:
2/2 PASS, checking configured contours against the immutable collision reference.
SHA-256 invariant comparison: 23/23 unchanged files, covering authoring contours,
generated environment presentation, import profiles and all twenty sprite metadata
files (`TestResults/ground-match-invariants.json`). No gameplay code was changed.
