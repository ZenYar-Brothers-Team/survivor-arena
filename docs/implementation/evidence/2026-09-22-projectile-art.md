# Stone and courier projectile art — 2026-09-22

Authorization: the user approved both presented candidates, asked to continue with their related effects, and specified that the stone should rotate slightly during attack. Execution status belongs only to [STATUS](../STATUS.md).

## Scope

The approved SKILL-001 image is a compact grey-brown faceted stone. Its immutable 1254×1254 selected master, exact generation prompt, approval and deterministic 256×256 runtime preparation are recorded in `Art/Source/Skills/skill-001/asset-record.json`. The current fixture skill `FIXTURE-SKILL-BOLT` references this projectile at every level for gameplay review; this does not register the production SKILL-001 definition.

The approved courier projectile is a compact cream dispatch letter tied with dark cord and a burgundy wax seal. It belongs only to the currently ranged `FIXTURE-ENEMY-FAN`; the canonical ENEMY-002 card remains melee. Source and preparation evidence are in `Art/Source/Enemies/fixture-enemy-fan-projectile/asset-record.json`.

Both sprites use a `SpriteRole.Projectile` presentation profile. The child renderer aligns its authored right-facing direction with travel while the circle collider stays on an unrotated physics root. The stone turns at 140 degrees per second; the letter keeps a stable heading. A shared per-projectile particle emitter creates one brief flash and three material-colored flecks. It allocates no object per hit, survives a terminal hit just long enough to show the tail, pauses with the run, and is cleared on pool reuse.

Gameplay review revision: the fixture stone keeps speed 10 but its lifetime is 0.5 seconds at every level, limiting straight travel to 5 world units, half of the 10-unit reference screen height. This changes only `FIXTURE-SKILL-BOLT`; other projectile families retain their authored ranges.

## Verification

- User image approval is recorded in both asset records.
- `python scripts/validate-art-manifest.py`: 17 owner/role records pass source/version identity and path validation.
- Unity 6000.6.0f1 safe batch smoke: **647/647 Game.* EditMode and 25/25 PlayMode, zero skipped**. The added lifecycle check covers visual-only spin, unchanged circle radius, immediate collider shutdown, pause-frozen impact tail and delayed pool return.
- Range revision smoke: **652/652 Game.* EditMode and 25/25 PlayMode, zero skipped**. The content regression verifies 5 world units at every fixture-bolt level.

The first full run passed 647/647 EditMode but exposed two PlayMode regressions: older ring/cross fixture references resolve to intentional `Unspecified` placeholders. The resolver now keeps those placeholders on the legacy fallback and requires `SpriteRole.Projectile` only for configured art; the complete rerun passed.

## Remaining review

Gameplay-scale size, spin speed and impact readability need the user's visual review. Production SKILL-001 registration remains IP-17 work; production ENEMY-002 behavior remains IP-20 work.
