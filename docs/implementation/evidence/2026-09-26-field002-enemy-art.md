# FIELD-002 enemy art — 2026-09-26

The user reviewed the three displayed candidates for ENEMY-006, ENEMY-008 and ENEMY-009 and confirmed them: «подтверждаю». The exact approved input bytes and reproducible packet are under `TestResults/art-previews/field002-enemies-2026-09-26/approved-art-packet.json`. `scripts/art_pipeline.py` reported PLAN (16 writes), then APPLIED; repeating the plan after content generation reported zero changes. Versioned candidates, selected masters, source records, import profiles, runtime PNGs and sprite catalog entries were saved. The exact generation prompts were recovered from the local task rollout and written verbatim into the packet and source records; each SHA-256 binds the approved input image.

| Owner | Runtime visual | Body profile |
|---|---|---|
| ENEMY-006 | `ENEMY-006-VISUAL-BODY` | 256px / 160 PPU; radius 0.336619, centerY 0.888349; `ENEMY-001-MOTION` |
| ENEMY-008 | `ENEMY-008-VISUAL-BODY` | 256px / 128 PPU; radius 0.603173, centerY 0.730440; `ENEMY-002-MOTION` |
| ENEMY-009 | `ENEMY-009-VISUAL-BODY` | 256px / 160 PPU; radius 0.458403, centerY 0.727409; `ENEMY-001-MOTION` |

All three bodies are bound in `ProductionEnemies.json` and preserved by `scripts/generate_field001_content.py --check`. ENEMY-006's bolt reuses `ENEMY-005-VISUAL-PROJECTILE`. The official `scripts/fit-body-contacts.py` validated the three fitted contact circles together with all registered bodies. No gameplay collision or enemy behavior values changed.

Verification: safe Unity art scope passed **44/44 Game.* EditMode** and **127** manifest owner/role records (`TestResults/checks/20260926T141316-936517Z/summary.json`). Safe full smoke passed **802/802 Game.* EditMode + 28/28 PlayMode**, zero failed/skipped, and **127** manifest records (`TestResults/checks/20260926T141345-670435Z/summary.json`). Gameplay-scale FIELD-002 visual review and the F2-06 manual run remain open.
