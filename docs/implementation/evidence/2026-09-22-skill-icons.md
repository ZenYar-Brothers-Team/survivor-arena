# Skill icons — 2026-09-22

Authorization: the user asked to create skill icons and approved the complete presented set of 16 candidates. Execution status belongs only to [STATUS](../STATUS.md).

## Scope

The packet contains one approved transparent icon master for every production skill ID `SKILL-001…016`. Each candidate is preserved under `Art/Source/Skills/<skill-id>/icon/v001/concept-01.png`, promoted byte-identically to `selected-master.png`, and copied to the stable runtime path `Assets/Resources/Art/UI/Icons/Skills/<skill-id>-icon.png`. Unity applies the shared icon import profile: Sprite/Single, centered pivot, max size 256, sRGB, FullRect, bilinear, clamp, no mipmaps, and uncompressed texture format.

Every icon has an individual `asset-record.json` with the complete generation prompt, approval evidence, hashes, constraints, and runtime output. `Art/asset-manifest.json` contains 16 owner/role records.

## Runtime integration

All icons are configured as `SpriteRole.Icon` with stable `SKILL-XXX-VISUAL-ICON` IDs. Thirteen current fixture skills reference the mechanically corresponding production icon. `SKILL-002`, `SKILL-013`, and `SKILL-015` have no equivalent fixture skill, so their assets are imported and registered without a false gameplay mapping.

The active-skill definition now carries a typed icon reference. `GameplayUiPresenter` resolves it through the runtime `ContentRegistry` and supplies it to both draft cards and occupied active slots in HUD/Pause Build. Definitions without an icon remain valid for isolated tests and transitional fixture content.

## Verification

- User approval: complete 16-icon preview set accepted on 2026-09-22.
- `python scripts/validate-art-manifest.py`: **PASS**, 41 owner/role records; source/version identity and paths valid.
- Unity 6000.6.0f1 safe batch smoke: **653/653 Game.* EditMode and 25/25 PlayMode, zero skipped**.
- The added import regression verifies all 16 PNGs use Sprite/Single, max size 256, centered pivot and uncompressed texture format. Runtime catalog coverage verifies that every current active skill resolves a distinct non-null `SpriteRole.Icon`.

## Remaining review

The user accepted the current in-game UI and icon sizes on 2026-09-22; actual draft/build readability of the thirteen mapped icons is accepted ([original feedback](../../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят)). Production skill definitions, final bindings and actual slot review of the three unmapped icons remain IP-17 work.
