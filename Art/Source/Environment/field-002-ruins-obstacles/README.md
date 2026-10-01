# Ruins obstacle source handoff

Historical first handoff. The current approved, installed masters are under Art/Source/Environment/field-dev-blobs; current packet: Art/Packets/field-dev-blobs-soft-edges-2026-10-01.json. Seventeen members of this historical set have newer soft-edge versions.

Selected set of **20 PNGs**, five families with four unique footprints each. Source files copied byte for byte; no raster preparation or Unity integration has been performed.

`wet-small.png`, `ward-small.png` and `net-small.png` use the soft-edge candidates. The other seventeen use the current selected art library. These are the complete twenty objects, not twenty plus three duplicates.

[library.json](library.json) records source paths, SHA-256 hashes, footprint dimensions in world units, contour points and reference canvas bounds. The intended theme is the future second map (ruins); the existing production FIELD-002 road definition has not been changed. Current geometry work belongs to FIELD-DEV-BLOBS on a 120 × 120 test map.

## Integration notes

- Fixed authored size and orientation; do not randomly rotate or scale the illustrations.
- Larger obstacles contain more details at comparable world scale. The large net deliberately uses heavier cable.
- For the soft-edge images, use `coreReferenceBoundsPx` from the original artwork to establish the core scale. Fitting the expanded alpha bounds to the collision footprint would shrink the core. The outer visual skirt does not extend collision.
- `pointsWu` describes the contour study, not a verified runtime collider. Reconcile artwork and collision before integration.
- Obstacles block the player; enemies pass through them.
- Preserve edge clearance, passable gaps and the start area in random placement. No daily scheduling rule was added.
- These selected masters live outside Assets per ASSET_PIPELINE. Runtime derivatives, import profiles, resource bindings and collider checks belong to the later integration. Use scripts/art_pipeline.py for the approved runtime packet once those identifiers and profiles are defined.

Visual comparison for the three softened variants: [on the ruins floor](../../../Concepts/field-002-ruins-blob-art/soft-edge-on-ruins-comparison.png). Original generation prompts remain in that concept directory's generation-plan-v3.json, prompts.md and soft-edge-study.json.
