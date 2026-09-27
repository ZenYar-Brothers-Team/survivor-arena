# Boss and midboss projectile art — 2026-09-27

The user authorized attack art for every boss still using fallback presentation, reviewed the nine-candidate FIELD-002…010 gallery and approved the complete set: «Подтверждаю.» The selected candidates and exact prompt set remain in [the review directory](../../../Art/Candidates/boss-attacks-2026-09-27/); the approved packet is [saved with pinned SHA-256 values](../../../Art/Packets/boss-projectiles-002-010-2026-09-27.json).

The packet defines nine shared field families owned by `BOSS-002…010-VISUAL-PROJECTILE`. The matching midboss reuses its field's final-boss visual; `MIDBOSS-004` has no projectile attack. Zones, burning ground and beams remain procedural as required by IP-21. This replaces the temporary `BOSS-001-VISUAL-PROJECTILE` reference without changing damage, cadence, speed, radius, hitbox, phase order or hazard geometry.

`python scripts/art_pipeline.py TestResults/art-previews/boss-attacks-2026-09-27/approved-art-packet.json` planned 38 writes; `--apply` completed them. A repeat dry run planned zero changes. Versioned concepts, selected masters and provenance are under `Art/Source/Bosses/boss-002…010/projectile/`; 256×256 runtime derivatives are under `Assets/Resources/Art/Sprites/Bosses/boss-002…010/`. Each sprite definition has a projectile profile and the required pulsing coral threat halo.

The authoring generator now maps every FIELD-002…010 boss/midboss projectile payload to its shared family and regenerated `ProductionBosses.json`. Bootstrap coverage verifies the field-pair mapping and sprite resolution; the FIELD-002 dash-volley test verifies both encounters use `BOSS-002-VISUAL-PROJECTILE`. `python -X utf8 docs/balance/validate_bosses_v1.py` passed all 16 late encounters; `python scripts/content/generate.py --check` reported `UP TO DATE`; tooling tests passed 23/23.

Verification:

- Art scope: **50/50 Game.* EditMode**, zero failed/skipped; manifest **176/176** (`TestResults/checks/20260927T125557-368084Z/summary.json`).
- Focused repaired test scope: **6/6 Game.* EditMode**, zero failed/skipped (`TestResults/checks/20260927T125816-931231Z/summary.json`).
- Unity 6000.6.0f1 full smoke: **857/857 Game.* EditMode + 30/30 PlayMode**, zero failed/skipped; generated content and audio checks passed; manifest **176/176** (`TestResults/checks/20260927T125843-334689Z/summary.json`).

Target-scale gameplay review remains open. FIELD-003 exists, but FIELD-004…010 and their waves are not yet available, so all later families cannot yet be reviewed in their owning encounters. This keeps IP-21 `Blocked` on field/wave availability and manual gameplay-scale acceptance, not on missing raster art.
