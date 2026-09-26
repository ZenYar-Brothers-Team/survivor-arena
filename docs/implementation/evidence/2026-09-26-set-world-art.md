# SET-008/016/018/019/020 world art integration — 2026-09-26

The user approved the five presented transparent projectile candidates with «Подтверждаю». The approved SHA-256 values and final prompts are in the immutable `Art/Source/Sets/set-*/projectile/asset-record.json` records; each has a `v001/concept-01.png`, `selected-master.png`, and a 256×256 runtime derivative. The `art_pipeline.py` dry run and apply both completed for the same approved packet. Unity generated import `.meta` files for all five new paths.

| Set | Visual ID | Runtime use |
|---|---|
| SET-008 | `SET-008-VISUAL-PROJECTILE` | Only the heavy replacement from SKILL-016 uses the new junk sprite; ordinary junk keeps its SKILL-016 sprite. |
| SET-016 | `SET-016-VISUAL-PROJECTILE` | Set attack template projectile. |
| SET-018 | `SET-018-VISUAL-PROJECTILE` | Set attack template sphere; shared explosion presenter retained. |
| SET-019 | `SET-019-VISUAL-PROJECTILE` | Set attack template ice spear. |
| SET-020 | `SET-020-VISUAL-PROJECTILE` | Set attack template boulder. |

`SetEffectHost` now passes the production content registry to set attack executors, allowing their configured visual IDs to resolve. The `SET-008` heavy replacement resolves its distinct visual without changing collision, damage, or projectile sequence rules. Regression tests cover both routes.

Checks: `python scripts/check_project.py --scope art` PASS, 44/44 Game EditMode and manifest 117/117. `python scripts/check_project.py --scope full` PASS, 792/792 Game EditMode and 27/27 Game PlayMode, manifest 117/117; runner batch preflight confirmed no open interactive Unity Editor. Final evidence: `TestResults/checks/20260926T123640-331720Z/summary.json`. An earlier full run found a missing presentation profile in the newly added test fixture; this was fixed before the final PASS.

The source images were approved. Runtime gameplay-scale readability and combined 3–4 set review remain open; these checks are not established by the automated tests.
