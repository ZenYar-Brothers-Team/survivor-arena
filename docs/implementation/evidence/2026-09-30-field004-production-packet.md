# FIELD-004 production packet — 2026-09-30

Scope: FIELD-004 «Рыцарский лагерь», DECISION-0129.

- Authoring source: `docs/balance/field004-v1.json`; production output is generated, never hand-edited.
- Static generation: `python scripts/content/generate.py --check` — PASS (`UP TO DATE`).
- Unity 6000.6.0f1 EditMode: `ProductionField004ContentTests` — 4/4 PASS, 0 failed/skipped (`TestResults/checks/20260930T173147-388737Z/summary.json`).
- Unity 6000.6.0f1 PlayMode: `ProductionField004SmokeTests` — 1/1 PASS, 0 failed/skipped (`TestResults/checks/20260930T173226-695904Z/summary.json`). The smoke unlocks FIELD-004 through the normal selection flow, starts the real scene, checks generated player-only colliders, and observes only FIELD-004 pool enemies.

Open review: one full player-driven 15-minute run is still required for actual difficulty, local pressure, obstacle readability and performance at gameplay scale.
