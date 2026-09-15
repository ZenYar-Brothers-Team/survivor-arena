# DECISION-0005 — Vertical UI delivery

Status: Approved  
Date: 2026-09-14

## Context

The original plan concentrated functional UI in IP-26. By IP-10 the prototype exposed gameplay mostly through runtime state and one IMGUI level-up block embedded in gameplay code. This made manual verification slow and left future systems without stable observable UI contracts.

## Decision

A narrow IP-10A UI Foundation is delivered before IP-11. It provides presentation contracts, HUD, level-up, pause/result shells, semantic element IDs and development-only controls. Each later system IP that introduces player-visible state must extend its ViewState/debug surface and tests in the same change. IP-26 remains responsible for the complete selection → run → result → meta → next-run flow rather than being the first UI implementation.

Every IP specification carries an explicit `UI / observability` section. Player-visible features own their vertical UI slice; infrastructure without player-facing state owns an equivalent diagnostic/test surface. Already implemented run, health, XP, draft and build systems are surfaced through IP-10A, including visible HP/XP bars and 6+6 build slots.

UI views do not own gameplay state. They render immutable snapshots and emit user intents through presenter contracts. Fixture/debug controls remain unavailable in non-development builds.

## Consequences

- Gameplay models remain independently testable.
- Presenters can be verified without loading a Unity scene.
- PlayMode tests can address UI through stable semantic names.
- UXML/USS become reviewable text assets suitable for AI-first iteration.
