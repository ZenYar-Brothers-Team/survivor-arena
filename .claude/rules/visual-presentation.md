---
paths:
  - "Assets/Game/Presentation/**"
  - "Assets/Resources/Content/Presentation/**"
  - "Assets/**/*.png"
  - "docs/art/**"
---

# Visual art and procedural sprite presentation

Read for any art operation or procedural sprite presentation change, wherever
the files live; paths above are common entry points, not a scope exemption.

Before generating, editing, importing, replacing, or wiring raster art, read
`docs/art/ART_DIRECTION.md` and `docs/art/ASSET_PIPELINE.md`. They are the
approved sources of truth for visual style, prompts, source/runtime paths,
naming, provenance, PNG preparation, Unity import settings, approval gates,
and safe replacement. Preview images do not enter `Assets`; only a
user-approved candidate is prepared as a runtime asset. Preserve an existing
runtime path and `.meta` GUID when an approved image is replaced.

Use `scripts/art_pipeline.py` for explicit approved packets and `scripts/check_project.py`
for scoped checks. The approved numeric-only visual-preview exception and command
contracts are in `scripts/README.md`; it defers tests until a visual variant is selected,
without declaring runtime verification or changing final IP acceptance.

Procedural sprite motion must live under an entity's child `VisualRoot`; never
animate the gameplay root, `Rigidbody2D`, collider transform/geometry, or
authoritative movement state for a visual effect. Apply body squash/stretch,
bob, tilt, recoil, facing, and transient renderer feedback through one pose
compositor/writer so independent channels cannot overwrite each other.
Presentation values belong in validated config profiles, pause advances no
presentation time, and `Shutdown()`/pool return must restore the captured
baseline pose and renderer state. See
[DECISION-0013](../../docs/decisions/0013-procedural-sprite-presentation.md).
