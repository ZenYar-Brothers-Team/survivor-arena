# ENEMY-010…020 body art — 2026-09-26

The user accepted ENEMY-010 («подходит»), reviewed the eleven-body packet, requested changes only to ENEMY-020, then approved the final lighter angel with covered hips and ordered the set integrated: «отлично, встраивай набор в игру». The selected ENEMY-020 is candidate-05. All eleven approved inputs, exact SHA-256 values, final prompts and preparation recipes are recorded in their `Art/Source/Enemies/enemy-XXX/body/asset-record.json` files; the working packet is `TestResults/art-previews/late-enemies-2026-09-26/`.

`scripts/art_pipeline.py` planned and applied ENEMY-010…019, then ENEMY-020. Each body has an immutable `v001/concept-01.png`, selected master, 256×256 runtime PNG, provenance, exact-path import profile and `SpriteRole.Body` definition. Unity created PNG and directory `.meta` files during the safe import run. `ProductionEnemies.json` now binds all eleven `ENEMY-XXX-VISUAL-BODY` references; `scripts/generate_field001_content.py` preserves those references and supplies the shared motion profile by movement family. No HP, movement, attack, reward or collision-size values changed. `ProductionLateEnemyCatalogTests` now asserts these body bindings and retains the explicit unbound-projectile assertion for later ranged IDs.

| Body | PPU | Contact radius | Contact centerY |
|---|---:|---:|---:|
| ENEMY-010 | 160 | 0.270428 | 0.292851 |
| ENEMY-011 | 160 | 0.254664 | 0.412046 |
| ENEMY-012 | 160 | 0.268750 | 0.530931 |
| ENEMY-013 | 160 | 0.321563 | 0.449081 |
| ENEMY-014 | 128 | 0.464741 | 0.885725 |
| ENEMY-015 | 160 | 0.320779 | 0.448671 |
| ENEMY-016 | 128 | 0.454714 | 0.728751 |
| ENEMY-017 | 160 | 0.260367 | 0.363056 |
| ENEMY-018 | 160 | 0.264113 | 0.412536 |
| ENEMY-019 | 128 | 0.472115 | 0.900354 |
| ENEMY-020 | 128 | 0.569836 | 0.602924 |

The radii are the maximum inscribed circles in the alpha≥230 convex outer silhouettes on each 256px runtime PNG, centered on the sprite pivot axis and rounded downward to 0.000001 world units. `python scripts/fit-body-contacts.py` validated all saved profiles, including the eleven new bodies. The art pipeline used `cropAlpha: false` and `alphaNoiseCutoff: 16`; import pivots follow the bottom opaque contact line. `python scripts/generate_field001_content.py --check` reported `UP TO DATE`.

Verification on Unity 6000.6.0f1: safe `python scripts/check_project.py --scope art` passed **44/44 Game.* EditMode** and **138/138** manifest owner/role records (`TestResults/checks/20260926T150953-015071Z/summary.json`). The first full run found one obsolete placeholder assertion for ENEMY-010; after updating that assertion, safe `--scope full` passed **804/804 Game.* EditMode + 28/28 PlayMode**, zero failed/skipped, and **138/138** manifest records (`TestResults/checks/20260926T151145-571443Z/summary.json`).

Remaining: target-scale Gameplay review of the new body silhouettes, especially the similarity of ENEMY-010 to the earlier crossbowman and the large ENEMY-020 wings/sword; projectile art for the new ranged IDs is a separate per-ID gate. Automated import/catalog tests do not prove those visual judgments or complete the whole IP-20 scope.
