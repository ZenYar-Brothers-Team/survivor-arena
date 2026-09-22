# Skill world-art packet — 2026-09-22

Authorization: the user approved the four presented world-art candidates with «утверждаю». Execution status remains owned only by [STATUS](../STATUS.md).

## Scope

The approved packet contains one transparent world sprite for each requested skill:

- `SKILL-003-VISUAL-PROJECTILE`: one silver-and-bronze orbital blade;
- `SKILL-006-VISUAL-PROJECTILE`: one wooden, brass-capped boomerang with a teal grip;
- `SKILL-008-VISUAL-PROJECTILE`: one steel-and-bronze ricochet disk with an amber core and cyan rim notches;
- `SKILL-014-VISUAL-PROJECTILE`: one violet shell sphere with orange cracks and an amber core.

Each immutable 1254×1254 candidate/master, exact prompt, icon reference role, hashes and deterministic 256×256 preparation are recorded in `Art/Source/Skills/<skill-id>/asset-record.json`. The runtime files use centered projectile imports under `Assets/Resources/Art/Sprites/Skills/<skill-id>/`.

`FIXTURE-SKILL-ORBIT`, `FIXTURE-SKILL-BOOMERANG`, `FIXTURE-SKILL-RICOCHET` and `FIXTURE-SKILL-SPHERES` inherit the mechanically corresponding visual at every level. This is a fixture review mapping and does not register production skill definitions or resolve `SKILL-008` G-04.

Orbit presentation rents one visual-only renderer per authored blade, parents it below the owner, follows the existing radius/angular speed/duration and returns it on completion or cleanup. The other three objects use the shared projectile child, visual-only spin and pooled impact tail. Sphere explosion damage remains immediate; an optional, reusable particle-only explosion profile adds a soft flash and eight radial fragments on terminal impact or expiry. The effect owns no damage, collider or gameplay timer and can later be reused by mines and sets.

## Verification

- User approval: all four presented candidates accepted on 2026-09-22.
- Raster audit: every runtime image is 256×256 RGBA, has zero alpha on the outer two-pixel border and keeps its visible silhouette inside a centered 192×192 envelope.
- `python scripts/validate-art-manifest.py`: **83/83** owner/role records pass provenance/path validation.
- Unity completed the asset refresh and wrote full `TextureImporter` settings for the new PNGs.
- The first interactive compile exposed ambiguous `Object` references in the new explosion lifecycle test; both cleanup calls now explicitly use `UnityEngine.Object`. The regression suite then compiled and passed: **658/658 Game.* EditMode, 0 skipped**, Unity 6000.6.0f1 (`TestResults/EditMode.xml`).
- PlayMode smoke: **NOT RUN** for this follow-up; no PlayMode PASS is claimed.
- User in-game visual acceptance: current UI, all four world sprites and sphere explosion accepted on 2026-09-22 ([original feedback](../../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят)).

## Remaining review

Run the remaining automated PlayMode smoke for lifecycle/cleanup coverage. The current gameplay-scale visual review is accepted by the user. Production definitions and final bindings remain IP-17 work.
