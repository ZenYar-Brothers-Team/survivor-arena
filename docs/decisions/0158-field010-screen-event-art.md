# DECISION-0158 — FIELD-010 screen-event artwork

Date: 2026-10-03. Status: Approved (visual selection and integration).

## Context and approval

FIELD-010 mechanics are defined by [DECISION-0157](0157-field010-screen-events.md).
The user requested art for those threats and, after the eight-image browser gallery,
approved the whole set: «хорошо, можешь встраивать в игру».
[Visual brief](../art/briefs/field010-screen-events-visual-candidates-v1.md) and
[approved packet](../../Art/Packets/field010-screen-events-v1.json) preserve the selection.
Execution evidence belongs to [STATUS](../implementation/STATUS.md).

## Decision

Use the eight approved PNGs for spear, blade, cloud wave, strip warning/strike,
circle warning/strike and safe-circle rim. Their preparation follows the repository
art pipeline, retaining original source hashes and generation prompts.

The authoring `field010-screen-events-v1.json` owns `screenEventPresentation`;
the content generator exports it into ProductionFieldEnvironmentPresentation.
Typed visual references and sprite roles are validated through the content registry.
Visual tuning (alpha, fill, palette, border width, repeat length, sorting) lives in JSON.

The runtime view uses the existing hazard geometry and elapsed event time. A shared
quad and materials apply the artwork with constant-width strip borders, repeated
interiors, a constant-width safe ring corridor and an analytic safe-disk cutout.
The ring corridor uses straight teal edges rather than stretching the circular PNG.
Sweep bodies are clipped at lane entrance/exit without changing their length.
Danger remains faintly filled so hollow ornament does not imply a safe interior.
Views are pooled under the event driver; no collider, damage, event timing or layout
changes are authorized by this art integration.

PNG approval is Gate B. Target-scale import inspection and the user's gameplay
visual/motion acceptance are separate gates under [Asset Pipeline](../art/ASSET_PIPELINE.md).
