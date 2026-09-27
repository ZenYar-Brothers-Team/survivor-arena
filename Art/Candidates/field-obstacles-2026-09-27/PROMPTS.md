# Field obstacle candidates — 2026-09-27

Preview-only art packet. Do not copy these files into `Assets/` before visual approval.

## Shared generation direction

- Isolated 2D obstacle cutout for a top-down fantasy survival arena.
- Cozy painterly storybook rendering with watercolor/gouache texture.
- Three-quarter top-down view, readable silhouette, subtle dark-plum outline.
- Transparent background; no characters, readable text, logos, UI, frame, or watermark.
- Intended world-size tiers: `small` = barrel/stump scale, `medium` = about 1.35x small,
  `large` = about 1.6–1.9x small and never monumental.

Final world scale, collider footprint, alpha cleanup, trim, padding, and import settings are
authoring decisions for the approved integration pass; source canvas dimensions do not define
gameplay size.

## Existing-map audit

- FIELD-001 already has three small silhouettes (stump, barrel, boulder) and one larger fence.
  Generated only the missing medium and second large roles.
- FIELD-002 already has a small boulder and a column obstacle. The non-colliding shrine decor
  does not count. Generated one small, one medium, and two large roles.
- FIELD-003 already has small rubble and a large wall. Visual-only water does not count.
  Generated two small, one medium, and one large role.

## Generated candidates

| Field | Tier | Candidate |
|---|---|---|
| FIELD-001 | medium | `field-001/medium-hay-bales-candidate-01.png` |
| FIELD-001 | large | `field-001/large-village-handcart-candidate-01.png` |
| FIELD-002 | small | `field-002/small-roadside-milestone-candidate-01.png` |
| FIELD-002 | medium | `field-002/medium-roadside-bench-candidate-01.png` |
| FIELD-002 | large | `field-002/large-road-barricade-candidate-01.png` |
| FIELD-002 | large | `field-002/large-broken-wagon-candidate-01.png` |
| FIELD-003 | small | `field-003/small-broken-urns-candidate-01.png` |
| FIELD-003 | small | `field-003/small-fallen-capstone-candidate-01.png` |
| FIELD-003 | medium | `field-003/medium-collapsed-well-candidate-01.png` |
| FIELD-003 | large | `field-003/large-ruined-arch-candidate-01.png` |
| FIELD-004 | small | `field-004/small-supply-crate-candidate-01.png` |
| FIELD-004 | small | `field-004/small-brazier-candidate-01.png` |
| FIELD-004 | small | `field-004/small-training-dummy-candidate-01.png` |
| FIELD-004 | medium | `field-004/medium-supply-cart-candidate-01.png` |
| FIELD-004 | large | `field-004/large-palisade-candidate-01.png` |
| FIELD-004 | large | `field-004/large-tent-candidate-01.png` |
| FIELD-005 | small | `field-005/small-goods-crate-candidate-01.png` |
| FIELD-005 | small | `field-005/small-stone-planter-candidate-01.png` |
| FIELD-005 | small | `field-005/small-water-barrels-candidate-01.png` |
| FIELD-005 | medium | `field-005/medium-fountain-candidate-01.png` |
| FIELD-005 | large | `field-005/large-street-barricade-candidate-01.png` |
| FIELD-005 | large | `field-005/large-market-stall-candidate-01.png` |
| FIELD-006 | small | `field-006/small-crystal-cluster-candidate-01.png` |
| FIELD-006 | small | `field-006/small-arcane-pedestal-candidate-01.png` |
| FIELD-006 | small | `field-006/small-brass-mechanism-candidate-01.png` |
| FIELD-006 | medium | `field-006/medium-laboratory-table-candidate-01.png` |
| FIELD-006 | large | `field-006/large-colonnade-fragment-candidate-01.png` |
| FIELD-006 | large | `field-006/large-astrolabe-candidate-01.png` |
| FIELD-007 | small | `field-007/small-root-stump-candidate-01.png` |
| FIELD-007 | small | `field-007/small-herb-planter-candidate-01.png` |
| FIELD-007 | small | `field-007/small-cistern-candidate-01.png` |
| FIELD-007 | medium | `field-007/medium-hedge-block-candidate-01.png` |
| FIELD-007 | large | `field-007/large-root-mass-candidate-01.png` |
| FIELD-007 | large | `field-007/large-footbridge-candidate-01.png` |
| FIELD-008 | small | `field-008/small-armory-crate-candidate-01.png` |
| FIELD-008 | small | `field-008/small-brazier-candidate-01.png` |
| FIELD-008 | small | `field-008/small-shield-rack-candidate-01.png` |
| FIELD-008 | medium | `field-008/medium-statue-plinth-candidate-01.png` |
| FIELD-008 | large | `field-008/large-corridor-barricade-candidate-01.png` |
| FIELD-008 | large | `field-008/large-column-cluster-candidate-01.png` |
| FIELD-009 | small | `field-009/small-celestial-lantern-candidate-01.png` |
| FIELD-009 | small | `field-009/small-stone-fragment-candidate-01.png` |
| FIELD-009 | small | `field-009/small-celestial-pedestal-candidate-01.png` |
| FIELD-009 | medium | `field-009/medium-arch-fragment-candidate-01.png` |
| FIELD-009 | large | `field-009/large-column-cluster-candidate-01.png` |
| FIELD-009 | large | `field-009/large-celestial-arch-candidate-01.png` |
| FIELD-010 | small | `field-010/small-golden-brazier-candidate-01.png` |
| FIELD-010 | small | `field-010/small-ceremonial-pedestal-candidate-01.png` |
| FIELD-010 | small | `field-010/small-celestial-basin-candidate-01.png` |
| FIELD-010 | medium | `field-010/medium-gilded-column-base-candidate-01.png` |
| FIELD-010 | large | `field-010/large-balustrade-candidate-01.png` |
| FIELD-010 | large | `field-010/large-column-cluster-candidate-01.png` |

Generated with OpenAI image generation. FIELD-002 barricade and wagon received a focused cleanup
pass to reduce clutter and keep their silhouettes compact.
