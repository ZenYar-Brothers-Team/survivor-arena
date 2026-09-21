# Conservative body circles — 2026-09-22

Authorization: [DECISION-0039](../../decisions/0039-conservative-body-contact-circles.md).
Observation: [OBS-01](../../playtests/2026-09-22_contact-gap.md).
Execution status belongs only to [STATUS](../STATUS.md).

## Change

Gameplay Player now has one CircleCollider2D instead of BoxCollider2D; the scene was saved through `FixtureContactBaker.BakePlayer` in Unity. Imported goblin and villager have JSON contact profiles. Both physics and targeting retain root-center coordinates; their body renderers shift downward by contactCenterY. Enemy root scaling is compensated for the fitted circle and visual. Existing root movement, contact callback/timer, damage and lifecycle mechanisms remain in use.

| Visual | Radius (world units) | Center above original foot pivot |
|---|---:|---:|
| Goblin | 0.11301 | 0.52631 |
| Villager | 0.09854 | 0.52438 |

Nominal center distance at contact is now 0.21155 world units, versus the previous horizontal box/circle threshold 0.9. Unity solver contact tolerance still applies. Significant visual overlap is intentional; art/motion itself is unchanged. Source and derivative PNGs were not edited in this follow-up.

The shared offline fit reads alpha >=230, searches on the flip axis in the central body band, shrinks the inscribed circle by two pixels and 25%, and writes two numeric values per visual. No texture readback, polygon construction, alpha lookup or additional collider occurs per runtime frame. This establishes constant simple geometry, not a measured FPS claim.

## Verification

- SpriteContactProfileTests checks a perimeter 10% larger than each circle against real PNG alpha, plus invalid data and root-scale compensation.
- BodyContactSmokeTests approaches from eight directions: separated circles do not damage; overlapping circles enter contact and damage once.
- EnemyBodyPresentationTests includes animated/plain/animated pool reuse and restored radius, child-only motion, pause and hit response.
- PlayerControlPhysicsTests runs in an empty restored Editor scene, eliminating interference from the Gameplay scene's baked actor.
- Static authoring capture: `PresentationReviewCapture.CaptureContacts`, `TestResults/body-contact-review.png`; cyan = goblin, yellow = villager. Pairs show nominal contact in eight directions, with mirrored sprites in the second row. This does not replace a gameplay feel/density review.
- Capture inspected: the bodies visibly overlap at nominal contact in all eight examples, with no visible separation between the two silhouettes. Overlap is substantial, especially vertically; this is a deliberate conservative first trial and remains subject to user feel review. Scene serialization whitespace was normalized after Unity save; no structural YAML edits were made.

Full `scripts/Test-Unity.ps1`: 644/644 EditMode, 24/24 PlayMode, zero skipped (2026-09-21 21:19 UTC / 2026-09-22 local). `validate-art-manifest.py`: PASS, 11 records. Initial runs exposed scene interference in an existing physics test; after isolating that test, the full suites passed. Unity was launched only after checking that no interactive project Editor was running.

## Limits

Only the two current imported bodies were fitted. Other enemies retain their former geometry; placeholder player uses the baked scene circle. New body art needs an authored fit and review. Animation may move the visual contour relative to the fixed circle; this is deliberately approximate contact, not pixel collision. The user has not yet approved the feel of the new overlap in a repeat playtest. Production ENEMY-001 balance and IP-12A density gates are unaffected.

## Second trial — user correction

The user rejected the initial circles as too small and requested roughly ×2.5 radii using a filled outer silhouette. Current radii: goblin 0.282525, villager 0.24635; centers unchanged. Nominal contact distance: 0.528875. The earlier table/opaque-margin check above describes the first trial only.

The authoring script and SpriteContactProfileTests now validate the filled convex outer envelope, ignoring gaps between limbs and body. The test also rejects the old tiny radii. The eight-direction gameplay test retains its contact/no-contact assertions; its upper bound remains below the original 0.9 contact distance. The latest capture replaces TestResults/body-contact-review.png. Maximum hull fit is available separately, but the requested ×2.5 step is used for this review; circles do not exactly touch the full convex envelope everywhere.

Second-trial verification: full EditMode 644/644 and PlayMode 24/24 passed, zero skipped. Offline outer-envelope check passed for both bodies; scene rebaked through Unity; git diff --check passed.

## Third trial — maximum inscribed circles

User authorized maximum fit, superseding the second trial's ×2.5 step. Offline linear optimization maximizes circle radius and vertical center inside the filled convex hull, with X fixed to the pivot so both facing directions fit the same circle. No inward margin remains except downward numeric rounding. Goblin radius=0.401431, centerY=0.530976; villager radius=0.330282, centerY=0.469539. Nominal center contact distance=0.731713. These are current values; earlier tables are trial history.

SpriteContactProfileTests now also requires tangency within one pixel of support-plane sampling tolerance, so the earlier undersized circles fail. The same eight-direction gameplay test verifies contact damage. The authoring optimization runs only offline; runtime physics and collider count are unchanged.

Third-trial full verification: 644/644 EditMode and 24/24 PlayMode passed, zero skipped; offline envelope validation and git diff --check passed.

User accepted the final circle fit and requested its inclusion in the standard art pipeline and a commit. ASSET_PIPELINE §22 now owns the reusable procedure; DECISION-0039 states the final accepted rule. The gameplay/density gate remains separate. Subsequent changes are documentation only.
