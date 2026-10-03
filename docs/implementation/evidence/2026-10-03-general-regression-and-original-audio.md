# General regression corrections and original FIELD-010 audio — 2026-10-03

Scope: user requested investigation of the ten outstanding failures, confirmed the five additional startup sets remain unlocked, then approved the separately auditioned original lightning/light-column clips for lightning and circular strikes. Decisions: [0161](../../decisions/0161-startup-sets-and-regression-checks.md), [0162](../../decisions/0162-field010-original-strike-audio.md). Execution status is maintained only in [STATUS](../STATUS.md).

## Ten failures and corrections

| Prior failure | Cause and correction |
| --- | --- |
| FieldPlatformSurfaceTests, diagonal local weave UV | Float projection differs by a few ULPs; use 0.00001 tolerance and retain geometry/safe-width assertions. |
| MetaProfileTests, startup sets | Old five-set expectation; assert all ten specific startup IDs. |
| MetaProfileTests, field-clear grants | Old totals; preserve grants and assert 11/12/13/14 unlocked sets. |
| MetaShopTests, fresh profile | Same stale startup-set expectation; assert ten starting sets. |
| MetaShopSmokeTests, card count | Expanded approved catalog has 85 non-dev cards and 25 locked sets, rather than 70/15. |
| GameplaySmokeTests, DEV speed | DEV deliberately ignores keyboard Submit; drive pointer down/up after layout. |
| ProductionMonasterySmokeTests, DEV map | Same input contract; pointer click and wait for visible tab layout. |
| UiFoundationSmokeTests, playtest layout | DEV tab was never selected through keyboard Submit; pointer click selects the tab. |
| UiLayoutR2SmokeTests, DEV focus | DEV intentionally rejects keyboard focus; guard that behavior and Space pause isolation. |
| UiEntrySmokeTests, field card bounds | DEV cards have a separate scroll section; check designed cards on-screen and DEV reachability after scroll. Overflow sample uses 21 cards, above the fitting capacity of 20. |

These corrections change tests, not production economy or input behavior. The first targeted five-test run exposed three remaining layout/overflow issues; those were corrected before the full run. Historical failure reports in earlier evidence remain intact.

Fresh full safe graphics batch, Unity 6000.6.0f1, closed Editor: `TestResults/checks/20261003T201042-993894Z/summary.json`. EditMode **1607/1607**, PlayMode **73/73**, no failures or skips; third-party tests 0. Art manifest 347 PASS, generated content up to date, audio integrity 55 files / 41 cues PASS. This full receipt predates the following clip import.

## Original audio import and final scoped checks

Deterministic generator: `Art/Prototypes/field010-audio-sketches/generate.py`; original synthesis without third-party samples. Approved preview bytes copied exactly to runtime:

| Cue | Runtime clip | Seconds | SHA-256 |
| --- | --- | --- | --- |
| screen.lightning | Audio/Sfx/lightning-v1 | 1.05 | 794aba0180b9f19af4a48885a65c81e0105513d869972f058dba1ce17d151cbe |
| screen.circle | Audio/Sfx/light-column-v1 | 1.20 | 5d209d5acee78e23eee9d864b8ce73927d7e3104a9b4ceb53fc801d9accfc00d |

Both WAVs are 44.1 kHz stereo PCM16, peak −3.35 dBFS; existing importer produces short DecompressOnLoad clips with Vorbis quality 0.75. Unity generated their metadata. Provenance records runtime and generator hashes, seeds and user approval in `docs/audio/SOURCES.json`; validator supports WAV/OGG and retains external CC0 checks. Cue gain 0.60, cooldown, priority, voice budget and strike-start trigger remain unchanged.

Final safe graphics audio/FIELD-010 batch: `TestResults/checks/20261003T202027-334167Z/summary.json`. EditMode **7/7**, PlayMode **3/3**, no failures or skips; integrity **57 files / 41 cues PASS**, content generation up to date. Catalog tests load both actual clips and verify import policy. Live circular strike test verifies the new clip, pause/resume and teardown; other FIELD-010 graphics guards and all 195 captures also pass. Automatic tests remain silent on system speakers.

Local motion gallery rebuilt with WAV-aware clip lookup: five animated examples and four listening families. Separate sound-only page remains available. Playback requires a user gesture. Full-suite PASS belongs to the earlier receipt; the final import was checked with the relevant scoped suite rather than claimed as a new full run. No edits to Game_design.md or Content_design.md.
