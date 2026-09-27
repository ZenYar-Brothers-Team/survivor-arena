# Procedural walk animation upgrade — 2026-09-26

## Context

User feedback after playtesting: locomotion presentation read as a generic
bob/sway rather than a walk. Requested "proper" animation instead, without
adding new raster art (no image-generation capability available in this
session; see [DECISION-0013](../../decisions/0013-procedural-sprite-presentation.md)
and `docs/art/ART_DIRECTION.md` — a purely code-side improvement stays inside
the project's own default rule that procedural motion is preferred over
frame-by-frame animation when it can carry the effect).

## Change

`Assets/Game/Presentation/ProceduralSpriteAnimator.cs` only; no changes to
`SpriteMotionProfile`, its JSON schema, or any content file — every existing
profile (`FIXTURE-MOTION-GOBLIN-AGILE`, `FIXTURE-MOTION-VILLAGER`,
`FIXTURE-MOTION-COURIER`, `CHAR-001-MOTION`, `ENEMY-001-MOTION`,
`ENEMY-002-MOTION`) picks the new behavior up automatically from its existing
tuning values.

1. **Speed-scaled step cadence.** The locomotion phase now advances by
   `deltaTime * BobFrequency * speedRatio`, not a fixed rate — slower travel
   takes slower strides instead of wobbling at a constant frequency
   regardless of how fast the body is actually moving.
2. **Footfall-shaped bob/stretch.** The signed locomotion sine is replaced
   by `|sin|^0.6`: the body only lifts on a footfall (never dips below the
   resting height) and the rise is quicker near contact, in place of an
   even sine wobble. Same `[0,1]` peak range as the previous `|sin|`, so the
   amplitude bounds `SpriteMotionProfile` already validates are unchanged.
3. **Lateral weight shift.** A small alternating left/right offset
   (`stepWave * BobAmplitude * 0.6 * speedRatio`) synced to the same step
   phase — secondary motion a pure vertical bob didn't have. Derived from
   the existing `BobAmplitude`, no new profile field.
4. **Weighted directional lean.** The velocity-driven tilt used to snap
   straight to its target every tick; it now approaches the target via
   exact exponential decay (`exp(-18 * dt)`), which composes exactly under
   repeated ticking and so stays frame-rate independent by construction
   (verified by the existing 30fps/120fps equivalence test). Idle sway is
   left unsmoothed on purpose — damping it would blunt the breathing wave
   the idle tests assert is visible.

`Y` offset and scale remain a function of `velocity.magnitude` only (never
direction), preserving the existing "diagonal speed matches horizontal speed
at the same magnitude" contract. Hit reaction and spawn pop are untouched —
only the ambient locomotion/idle pose changed.

## Verification

No test file changes; all existing behavioral contracts in
`ProceduralSpriteAnimatorTests`, `SpritePresentationRuntimeTests`, and
`GameplayPresentationSceneTests` cover the new formulas without modification
and pass:

```
python scripts/check_project.py --scope full
```

`727/727 Game.* EditMode`, `27/27 PlayMode`, `0 skipped`, Unity 6000.6.0f1,
manifest 103/103 owner/role records valid.

Manual gameplay-scale review (Gameplay showcase `Left`/`Right`/`Live`) is
still open — this evidence covers the automated safety-envelope and
frame-independence checks, not the user's visual acceptance.

## Scope note

IP-12A is already `Verified`; this is a same-scope quality follow-up inside
its existing contract (procedural motion under `VisualRoot`, safety
envelope, pause/reset semantics) — no IP status or Execution order change.
