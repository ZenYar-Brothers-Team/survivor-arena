# Academy contrast and alternative ground previews

User requested a larger difference between inactive and active seals and three
less uniform, less blurred alternatives to the current Academy ground.

## Contrast

`FixtureZoneSeals.inactiveVisibilityMultiplier` is 0.55, validated in [0,1].
For an inactive seal alpha = prior visibility × 0.55 × layer alpha; active
visibility and the existing application flash remain unchanged. Example: resting
pulsing seal visibility 0.14 becomes 0.077 before layer alpha; warning visibility
1 becomes 0.55, while full active visibility stays 1. The unchanged warning curve
continues to grow over the existing preparation period. No gameplay timing,
radius, payload or PNG change.

`Seal_InactiveStatesAreDimmer_ActiveBrightnessAndBoundaryUnchanged` covers pulsing
book and burst portal at warning/active/fade/rest samples and asserts both alpha
and unchanged area transform. Safe runner attempts could not execute while an
interactive Editor was open with unavailable REST; no new runtime PASS claimed.
The subsequent process-inspection approval was declined; no batch bypass used.

## Ground candidates

Built-in image_gen produced three opaque square previews under
`Art/Candidates/field006-ground-alternatives-2026-10-02/`:

- A-flagstones.png: irregular chipped slate slabs and repairs.
- B-bedrock.png: fractured geological layers, gravel and muted violet seams.
- C-terrazzo.png: embedded mineral chips and sparse worn inlay fragments.

All three were visually inspected. They have clearer dry texture and material
variation than the previous smooth mineral surface. B is the busiest; A has the
clearest broad shapes; C has the smallest detail. Seamless tiling is requested
in prompts but repeat quality at gameplay scale remains to be checked for the
selected candidate. Verbatim prompts and generation mode are in prompts.json.
User rejected A/B/C as too similar to rocks rather than a magical laboratory.
These remain historical previews outside Assets. No replacement art
packet is approved or applied yet; current ground remains connected.

Second preview set: `Art/Candidates/field006-laboratory-floors-2026-10-02/`,
generated with built-in image_gen and verbatim prompts.json:
D — crafted blue-gray laboratory ceramics; E — cut slabs with muted brass conduits;
F — dark plum panels with worn geometric calibration marks. Their explicit
constraint is constructed indoor laboratory flooring rather than natural rock.
These also require user selection before runtime replacement.
