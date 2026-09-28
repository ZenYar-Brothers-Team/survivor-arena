# Main Menu E — Shepotka foreground preview

Date: 2026-09-28
Parent: [depth preview](menu-depth-review.md), [proposal](../2026-09-28-ui-entry-r1.md).
Execution status: [STATUS](../../STATUS.md).

## Visual approval — 2026-09-28

After reviewing E with the v002 adult-looking Shepotka, the user said:
«отлично, всё принимается». This accepts the current illustrated menu candidate
and its layered movement, rays and medium-sized dust over both heroes.

The selected pair is fixed to these source bytes, freshly hashed at approval:

- Backplate: `TestResults/ui-entry-r1/menu-depth-background-v001.png`, SHA256
  `426e9c8993c2ef0b3bb3a02d28d2a201efb9990ee278ebdd45f5940a3a43ece5`.
- Foreground: `TestResults/ui-entry-r1/menu-depth-foreground-shepotka-v002.png`, SHA256
  `5137a189537ef1bff279b4c962da2199467376556f5b9ca680d1d4188ba3461a`.

The older appearance and rolled maps apply only to this illustration. CHAR-003
canonical data and gameplay body remain unchanged. Earlier variants are not
selected by this approval. Historical preview notes below describe their original
review state; this approval supersedes the pending visual choice for v002.

Approval is not Unity import/integration or runtime acceptance. Production art
preparation, final button/material work and Unity transfer remain separate steps
in STATUS; none were performed in the approval-recording turn.

For the requested commit, byte-identical approved review copies are retained in
`docs/implementation/proposals/ui-entry-r1/assets/menu-depth-background-v001.png`
and `docs/implementation/proposals/ui-entry-r1/assets/menu-depth-foreground-shepotka-v002.png`.
Their hashes are the approved values above; HTML E now reads these versioned
review assets. These are not Art/Source selected masters or Unity runtime imports.
The old ignored copies and unselected experiments remain local. No raster edits
or production art preparation were performed for the commit.

## User request and scope

Replace the right goblin in the current menu artwork with a version of Shepotka;
keep the left goblin and chase scene. Dust should cross the left goblin too, with
a size midway between the previous two versions. A subsequent clarification asks
for slightly stronger light motion, not greater brightness.

This is a preview-only illustration edit through the imagegen skill and built-in
image_gen, not a new gameplay body or production CHAR-003 binding. The approved
CHAR-003 sprite is an identity reference only. No new lore or gameplay rules.
The existing backplate with knights, UI and D variant are unchanged. Left-goblin
identity, pose and location are visually preserved; AI editing is not claimed
pixel-identical. Original foreground remains available and is not overwritten.

## Original v001 preview packet / provenance

- Edit target: `TestResults/ui-entry-r1/menu-depth-foreground-v001.png`.
  SHA256: `a4f7a4e00159e52b93e8f1be97325672320870bb6e88ef3c515a89b2467cdb4e`.
- Identity reference: `Assets/Resources/Art/Sprites/Characters/char-003/char-003-body.png`.
  SHA256: `b378d282a37b0e6c4204c417e58963ed0aae9cea20832d123052da66e9fcfe76`.
- Built-in generated output: `exec-52f35181-40a7-4741-9d73-a43ccea7739c.png`.
- Byte-identical workspace copy: `TestResults/ui-entry-r1/menu-depth-foreground-shepotka-v001.png`.
  SHA256: `55de9ca09c3e894a6f1d38d26c3680f69644c4217cee4d225f2a4b7c62d16abc`.
- PNG: 1672×941, RGBA, true transparency checked by browser alpha sampling.
- Consumer: HTML E foreground, retaining the existing 90% size / 7.5% left /
  10% top presentation fit. New path is individually allowlisted by the local
  review server. Source files and the original foreground remain unchanged.

No Assets, Art/Source, manifest, import settings or Unity bindings were changed.
The new image has not received final visual approval. Production art_pipeline.py
and Unity art checks are deliberately deferred until a candidate is selected;
no approval record is fabricated for a preview.

## v002 — adult appearance for this illustration only

The user requested an older-looking Shepotka and a different accessory, explicitly
limited to this picture. In the current E illustration she reads as a young adult:
less rounded facial proportions, smaller-looking eyes and slightly taller adult
proportions. The cross-body satchel/strap is replaced by rolled maps tied to the
waist belt. This does not change canonical CHAR-003 age, costume, body sprite or
gameplay data. Background, left-goblin identity, UI and all motion/dust values
are retained; AI edits are not claimed pixel-identical.

- Edit target: the v001 Shepotka foreground above, SHA256
  `55de9ca09c3e894a6f1d38d26c3680f69644c4217cee4d225f2a4b7c62d16abc`.
- Generator: built-in image_gen via imagegen skill; no CLI/API fallback.
- Generated output: `exec-e11dd42f-aae5-439c-adf7-7f80bc95a4fd.png`.
- Byte-identical workspace copy: `TestResults/ui-entry-r1/menu-depth-foreground-shepotka-v002.png`.
- SHA256: `5137a189537ef1bff279b4c962da2199467376556f5b9ca680d1d4188ba3461a`.
- Actual output: 1671×941 RGBA (one pixel narrower than requested); no resampling
  or raster post-edit. Existing CSS containment/fit retained and checked.
- Consumer: current HTML E foreground. Earlier v001 and all original art preserved.
- Preview only, pending visual choice. No production pipeline/import or Unity change.

### Exact v002 edit prompt

```text
Use case: precise-object-edit.
Asset type: transparent foreground layer of an existing fantasy main-menu illustration, preview only.
Input image 1: edit target, 1672x941 RGBA landscape canvas with two running goblins. Change ONLY the female goblin on the RIGHT.
Primary request: make the right-hand Shepotka clearly a young ADULT goblin woman, visually in her mid-to-late twenties, rather than a child or teenager. This is a scene-specific illustration variant, not a redesign of the game character. Keep her recognizable patched dusty-teal hood/cape, dark brown hair, long pointed ears, olive skin, cream tunic and brown boots.
Age cues: visibly less round face, longer facial proportions and a defined jaw and cheekbones, smaller eyes relative to the face, a more mature alert expression, a slightly smaller head relative to a lean adult torso and limbs. Modestly lengthen her adult proportions within her existing placement; remain a small goblin and same storybook rendering. Do not make her elderly, glamorous, sexualized or photorealistic. Her expression is focused, capable and determined; not smiling, bruised, injured or helpless.
Accessory change: REMOVE her brown cross-body satchel completely, including the diagonal satchel strap. Replace it with a small distinctive bundle of two rolled parchment maps tied with a dusty-teal cord to her waist belt at the hip, clearly rolled scroll silhouettes rather than another bag. No readable writing. Both hands remain free in a natural running pose.
Invariants: preserve the LEFT goblin as faithfully as possible, his age, face, red scarf, teal shirt, brown satchel, running pose, proportions, size and location. Preserve right Shepotka's running direction, companion-facing gaze, approximate location and foot baseline. Keep landscape framing, generous transparent area on the left/top, existing scale and camera angle. No recentering or enlargement of the whole group.
Style: same hand-painted 2D storybook cutout, matte colors, dark-plum outlines, cel shading, restrained paper texture, warm existing light.
Output: genuinely transparent RGBA, same 1672x941 landscape canvas. NO background, knights, ground, shadows, particles, light rays, UI, text, watermark, extra characters or objects. No cropped ears, hood, hands or feet. Do not alter anything except the right character's adult appearance and requested accessory replacement.
```

## Atmosphere tuning

- Dust: 18 particles, 7.5–16.8 px before animated scale, the arithmetic midpoint
  between the earlier 3–6.6 px and 12–27 px versions. Radial edges and peak opacity
  0.46 retained. Starting positions alternate between the two character regions.
- Paint order is explicit: depth scene 0, dust 1, menu controls 2. Particles are
  non-interactive and restricted to the illustration side, never the buttons.
- Rays: angle oscillation ±3° and horizontal translation ±8% of each ray's width,
  previously ±1.2° and ±3%. Alternate durations 10/13/12 s instead of 14/19/17 s.
  Brightness, gradient, width and edge fade are unchanged. Rays stay behind heroes.
- Motion toggle, reduced-motion and hidden-tab handling remain in force.

Browser checks and visual inspection: [evidence](../../evidence/2026-09-28-ui-entry-r1-mockups.md).

## Exact edit prompt

```text
Use case: precise-object-edit.
Asset type: transparent foreground layer for an existing illustrated 16:9 main-menu parallax scene; a preview illustration, NOT a replacement gameplay sprite.
Input images: Image 1 is the edit target: two running goblins on a genuinely transparent 1672x941 canvas. Image 2 is ONLY the character identity/costume reference for Shepotka (CHAR-003), not its pose or square composition.
Primary request: replace ONLY the bulky goblin on the RIGHT in Image 1 with a running version of Shepotka from Image 2. Preserve the left goblin exactly as closely as possible: his face, red scarf, teal shirt, pose, size and location. Preserve the full landscape canvas, all transparent negative space, and the right character's approximate placement and foot baseline. Do not recenter or enlarge the two-character group.
Shepotka: slender young female goblin scout, olive-green skin, long ears, dark brown hair, expressive amber eyes, patched dusty-teal hood and short cape, cream patched tunic, dark cropped trousers, small brown cross-body satchel, worn brown boots. Clearly her identity from Image 2, not a bulky male in a hood. Fully clothed, natural nonsexualized proportions. Her running pose should fit the same chase: coming toward the viewer and slightly left, arms naturally pumping, head glancing toward her companion on the left. Focused and determined with a little alert concern, NOT smiling, crying, bruised, injured, or helpless. Natural anatomy and hands.
Style: match Image 1's hand-painted storybook cutout drawing, dark plum outline, matte warm colors, simple expressive cel shading and restrained paper texture. Match the existing warm light, no added glow.
Constraints: only the right character changes; retain the left character and both characters' placement. Output genuinely transparent RGBA; preserve transparency. No background, knights, ground, shadow plane, lighting beams, dust particles, UI, text, frame, logos, watermark, extra people or props, weapons or kettle. Do not crop feet, ears, scarf or hood. Maintain landscape 1672x941 framing.
```
