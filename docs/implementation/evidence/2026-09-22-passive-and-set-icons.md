# Passive and set icons — 2026-09-22

Authorization: the user requested passive and set icon generation and approved the complete presented collection with «Утверждаю, всё». Execution status belongs only to [STATUS](../STATUS.md).

## Scope

The packet contains one approved transparent icon master for every production passive ID `PASSIVE-001…014` and set ID `SET-001…020`. Each candidate is preserved under its `icon/v001/concept-01.png`, promoted byte-identically to `selected-master.png`, and copied to a stable runtime path under `Assets/Resources/Art/UI/Icons/Passives/` or `Sets/`. Unity applies the shared icon import profile: Sprite/Single, centered pivot, max size 256, sRGB, FullRect, bilinear, clamp, no mipmaps and uncompressed texture format.

Every icon has an individual `asset-record.json` with generation brief, approval evidence, hashes, constraints and runtime output. `Art/asset-manifest.json` contains one owner/role record per icon.

## Runtime integration

All icons are configured as `SpriteRole.Icon`. Nine current fixture passives reference mechanically corresponding production icons: Vitality/001, Collector/002, Haste/005, Memory/006, Pickup Radius/007, Knockback/011, Size/012, Range/013 and Low Health/014. Four fixture sets map to SET-001, SET-005, SET-012 and SET-016. Other approved icons are imported and registered without false gameplay mappings.

Passive and set definitions now carry typed icon references and expose them to content validation. `GameplayUiPresenter` resolves passive icons for draft/build slots and set icons for acquired-set rows through the shared registry. Definitions without an icon remain valid for isolated tests.

## Verification

- User approval: complete 34-icon preview set accepted on 2026-09-22.
- `python scripts/validate-art-manifest.py`: **PASS**, 75 owner/role records; source/version identity and paths valid.
- Unity 6000.6.0f1 safe batch smoke: **655/655 Game.* EditMode and 25/25 PlayMode, zero skipped**.
- Import regression covers all Skills, Passives and Sets icon folders. Runtime catalog coverage verifies that all mapped fixture passives and sets resolve distinct non-null `SpriteRole.Icon` assets.

## Remaining review

The user accepted the current in-game UI and icon sizes on 2026-09-22; actual draft/build/set-row readability of the nine mapped passives and four mapped sets is accepted ([original feedback](../../playtests/2026-09-22_visual-acceptance.md#obs-01--текущий-вид-игры-принят)). Production definitions, final bindings and actual slot review of currently unmapped icons remain IP-18/IP-19 work.
