# ENEMY-002 courier and shared ground shadows — 2026-09-22

Authorization: the user requested execution of art priorities 1–3 together. Shadow approach: [DECISION-0041](../../decisions/0041-shared-procedural-ground-shadows.md). The rejected v001 and required separation from playable identity are recorded in [DECISION-0042](../../decisions/0042-enemy-player-visual-separation.md). Execution status belongs only to [STATUS](../STATUS.md).

## Scope

ENEMY-002 was generated from the Approved content card with the accepted goblin and ENEMY-001 as style references. The user rejected v001 because its frightened expression, large eyes, hair, ragged clothing and vulnerable posture made it resemble the playable goblin. V002 redesigns the courier as a confident adult human pursuer with narrower focused eyes, longer human proportions, squared posture, intact burgundy courier uniform and a clear dispatch bag/letter. The unchanged 1254×1254 selected master and both exact prompts are recorded in `Art/Source/Enemies/enemy-002/asset-record.json`. A 256×256 transparent runtime derivative is imported at 160 PPU and connected to the unchanged `FIXTURE-ENEMY-FAN` definition for review. `FIXTURE-MOTION-COURIER` supplies lighter, quicker procedural body motion. This fixture binding does not register production ENEMY-002 gameplay data.

The v002 courier contact profile was fitted by the standard convex-envelope procedure: radius `0.360855`, centerY `0.453097`. The goblin and villager profiles remained byte-for-byte numerically unchanged. The runtime crop deliberately ignores generated pixels below alpha 10; otherwise almost invisible noise around the master shrinks the visible body during normalization.

One shared procedural ground-shadow profile now serves the player, ordinary enemies, bosses and Travelers. `GroundShadowSprite` creates one 32×32 radial mask; renderers reuse it. Shadow width is calculated once as authored contact diameter × `1.2`; height is `0.24` world units and fallback width is `0.86`. This makes broad characters cast a broader shadow without sprite analysis or per-frame work. Shadows align with each body's ground point, remain presentation-only and have no colliders.

## Verification

- `python scripts/fit-body-contacts.py`: all three body circles fit their filled convex outer envelopes; the courier reaches the computed maximum.
- `python scripts/validate-art-manifest.py`: source/provenance/runtime paths and owner-role uniqueness pass after manifest update.
- Final v002 Unity 6000.6.0f1 safe batch run: **646/646 Game.* EditMode, 25/25 PlayMode, zero skipped**.
- Courier import/reimport covers GUID, 256×256 size, alpha, 160 PPU, pivot, no mipmaps/readback and uncompressed import.
- Ground-shadow tests cover shared mask identity, contact-ground alignment, collision-size compensation, sorting and absence of colliders. Enemy body/pool tests cover enablement after reuse; full composition and gameplay smoke remain green.
- Reproducible comparison capture: `TestResults/enemy-002-shadow-review.png`; it uses the imported goblin, villager and courier v002 plus the exact shared runtime shadow sprite/profile.
- Width-tuning follow-up passed the full Unity 6000.6.0f1 safe batch run: **646/646 Game.* EditMode, 25/25 PlayMode, zero skipped**. The regression now verifies different contact radii produce different shadow widths while root-scale compensation preserves world size.

The first two sandboxed Unity attempts did not start tests because Unity Package Manager could not create its IPC channel. The same smoke check was rerun outside the sandbox; the only initial test failure was an exact floating-point assertion in the new shadow test, corrected to a numeric tolerance before the final pass.

## Remaining review

The user accepted the current courier v002 and common ground-shadow presentation in game on 2026-09-22 ([original feedback](../../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят)); image approval is recorded in provenance and manifest. Production ENEMY-002 binding, balance and field composition remain IP-20 work.
