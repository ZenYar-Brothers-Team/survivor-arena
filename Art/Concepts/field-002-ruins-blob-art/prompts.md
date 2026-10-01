# Image generation prompts

Generator: OpenAI built-in image generation. Transparent background requested
for each output. Input image 1 in each request was the corresponding mask. The
other images were style references, never edit targets.

Historical selected library: `Art/Source/Environment/field-002-ruins-obstacles/library.json`. The v003 shared-world-scale requests
are preserved verbatim with reference filenames in `generation-plan-v3.json`.
The following sections retain historical requests, including superseded shapes.

Historical reference filenames below record the original requests. Superseded
preview PNGs and study masks were removed after final selection; installed
versions are preserved under `Art/Source/Environment/field-dev-blobs/`.

## Small Angular

References: `small-angular-mask.png`, existing FIELD-003 `ground/selected-master.png`,
existing FIELD-003 `water/selected-master.png`.

> Use case: stylized-concept. Create ONE gameplay environment sprite concept for a small obstacle zone in FIELD-002, the future ruined-border-field theme. Reference image 1 is the exact small angular footprint mask: preserve its outer polygon silhouette and orientation, with all visible art inside this boundary and ample transparent padding. Images 2 and 3 are STYLE REFERENCES ONLY for the existing game's dusty stone ground and muted blue-grey water colors, not targets to copy. Depict a shallow broken flagstone patch with a thin ring of cracked low curb stones, a few fallen stone fragments lying flat, damp blue-grey puddles, and sparse moss. Add a subtle worn ochre boundary marking that makes this whole patch read as a restricted ground area. Everything is ground-level or ankle-high; enemies can visibly walk through the area, even though the player will be blocked by a separate gameplay collider. No upright wall, house, pillar, large boulder, hole, bridge, or building. Stylized 2D storybook cutout, 3/4 top-down gameplay view, strong readable silhouette, thick slightly irregular deep-plum outer contour (#241B2B), matte desaturated earth and blue-grey stone, two or three large color masses, simple cel shading, restrained paper texture, neutral warm light. Transparent background, no ground plane outside the footprint, no cast shadow, no text, no watermark, no frame, no photorealism, no 3D rendering. Asset role: preview footprint art, small roughly 6 by 5 world units. Keep the contour and silhouette the same as image 1; only paint its interior.

## Medium Linear

References: `medium-linear-mask.png`, `small-angular-preview.png`, existing
FIELD-003 `ground/selected-master.png`.

> Use case: stylized-concept. Create ONE gameplay environment sprite preview for the MEDIUM obstacle zone of future FIELD-002 Ruins. Image 1 is the exact long, gently bent, tapering footprint mask. Preserve its outer silhouette, horizontal orientation, dimensions and padding; every visible element must remain inside it. Image 2 is the companion small ruin-zone sprite and is the main style/palette reference; match its line weight, dusty taupe stone, muted blue-grey damp patches, ochre boundary marks, matte storybook shading. Image 3 is the existing game's ground texture reference. Paint an elongated strip of collapsed paving and shallow drainage trench: low flat flagstones, a few small broken masonry pieces, puddled channels, sparse moss, and a continuous worn ochre line marking the edge as a restricted area. Keep it ground level, with no solid wall or barrier, so human enemy sprites crossing it will look plausible. The player is blocked by gameplay geometry but that is not depicted as a tall physical barrier. Stylized 2D storybook cutout, 3/4 top-down gameplay view, strong readable silhouette, thick slightly irregular deep-plum outline #241B2B, flat matte color masses, simple cel shading, light paper texture. Transparent background, no external ground plane, no shadow cast beyond the footprint. No people, houses, arches, upright walls, pillars, boulders, text, watermark, frame, photorealism or 3D. Asset role: preview obstacle footprint roughly 13 by 4 world units. Paint inside image 1, do not modify its outer contour.

## Large Round

References: `large-round-mask.png`, `small-angular-preview.png`,
`medium-linear-preview.png`, existing FIELD-003 `background/selected-master.png`.

> Use case: stylized-concept. Create ONE gameplay environment sprite preview for a LARGE obstacle-zone footprint in future FIELD-002 Ruins. Image 1 is the exact broad asymmetric rounded footprint mask. Preserve its outer contour, orientation and generous transparent padding; paint entirely inside its boundary. Images 2 and 3 are companion small and medium ruin-zone sprite style references: match their deep plum contour, dusty taupe cracked paving, slate blue-grey wet areas, sparse moss, worn ochre perimeter marker, matte storybook cutout shading. Image 4 is the existing ruined-border-field background mood reference; do not copy its architecture. Depict an extensive low, open ruined courtyard floor: broad cracked stone mosaic, uneven shallow blue-grey pooled water in a few large regions, broken tile islands, sparse flat fallen blocks, patches of moss, and an old ochre ring/line close to the silhouette edge that signals a restricted area. Provide 2–3 large readable internal masses and less tiny clutter than the small sprite, suitable for an object roughly 22 by 19 world units. Keep the entire scene essentially flush with the ground; enemy soldiers walking through it must look plausible. No standing walls, building footprint, tower, high arch, big boulder, deep pit, impassable structure or fence. The gameplay collider blocks the player separately. Stylized 2D storybook cutout, 3/4 top-down game view, thick slightly irregular deep-plum outer outline #241B2B, matte desaturated earth and blue-grey stone, simple cel shading, restrained paper texture, neutral warm light. Transparent background, no external ground plane or cast shadow, no people, text, watermark, frame, photorealism or 3D. Keep image 1 silhouette exact.

## Ancient Royal Ward: Small Angular

References: `small-angular-mask.png`, existing FIELD-003 ground and background.

> Edit image 1, a fixed small angular silhouette mask. Create a NEW VISUAL STYLE for this exact FIELD-002 ruin-zone footprint: ANCIENT ROYAL WARD. Keep the same outer polygon and orientation, art strictly within its boundary, transparent canvas outside. It is a flat ruined flagstone mosaic with a few large pale limestone slabs, blue-grey cracked inlays, and a continuous luminous restrained ice-blue and warm gold protective seal along the inner edge. One simple geometric seal motif in the center, no readable letters or text. This is a ground-level ward that repels the playable monster but lets human pursuers cross, so nothing tall or physically solid should occupy the footprint. Existing FIELD-003 images 2 and 3 are palette/perspective references only: keep their muted storybook ruin materials, but this variant must be visibly different from the brown puddled paving study by having pale stone and magical boundary light instead of water. Stylized 2D storybook cutout, 3/4 top-down game sprite, matte cel shading, strong readable shape, thick slightly irregular deep-plum #241B2B outer contour, 2-3 broad color masses, neutral warm light, ample transparent padding. No wall, house, pillar, arch, boulder, water pool, vegetation mass, floor outside the mask, baked shadow, characters, UI, text, frame, watermark, photorealism or 3D.

## Ancient Royal Ward: Medium Linear

References: `medium-linear-mask.png`, `ward-small-angular-preview.png`.

> Edit image 1 into the MEDIUM LINEAR version of the ANCIENT ROYAL WARD environment sprite. Image 1 is the fixed gently bent horizontal strip mask; preserve its silhouette, orientation and transparent padding exactly. Image 2 is the companion small ward sprite and the visual style target. Fill the strip with pale cracked limestone flagstones, muted blue-grey masonry, a few large inlaid geometric stone tiles, and a continuous restrained pale ice-blue and warm-gold glowing protective circuit just inside the perimeter. The luminous line may bend with the strip and connect simple diamond-shaped nodes; no letters, symbols that resemble writing, or UI embellishment. Make it clearly the same ruin ward family as image 2 but with a long flowing layout, not a copied circular emblem. Ground level only: humans can walk through while the ward excludes the playable monster. Stylized 2D storybook cutout, 3/4 top-down gameplay sprite, thick slightly irregular #241B2B outer contour, matte broad color masses and simple cel shading. Transparent background. No water pool, plants, tall stones, wall, house, pillar, arch, large boulder, characters, ground beyond footprint, baked shadow, text, frame, watermark, photorealism or 3D.

## Ancient Royal Ward: Large Round

References: `large-round-mask.png`, `ward-small-angular-preview.png`,
`ward-medium-linear-preview.png`.

> Edit image 1 into the LARGE ROUND version of the ANCIENT ROYAL WARD environment sprite for the future FIELD-002 ruins. Image 1 is the fixed broad asymmetric rounded footprint mask: preserve its outer silhouette and orientation, with transparent background and ample padding. Images 2 and 3 are the companion small and medium ward sprites, and define the consistent style. Make a broad low ruined courtyard of pale cracked limestone and muted blue-grey stone mosaic. Along the irregular boundary, an old thin ice-blue and warm-gold protective circuit follows the footprint, with a handful of large diamond nodes. In the center, a restrained broken concentric stone inlay and one simple four-point glowing emblem. Some slabs are missing or fractured; it must still look like an ancient ruin rather than a new ornate floor. This ward repels playable monsters but allows human pursuers to cross, so all geometry is flat or ankle-high. Visibly different from brown muddy-water style: no puddles or vegetation mass. Stylized 2D storybook cutout, 3/4 top-down game view, 2-3 broad readable color masses, thick slightly irregular deep-plum #241B2B outline, matte color and simple cel shading. No standing walls, buildings, arches, towers, boulders, high columns, deep hole, characters, UI, readable writing, frame, watermark, shadow beyond footprint, photorealism or 3D. Preserve contour, paint only its interior.

## Overgrown Thorned Ruin: Small Angular

References: `small-angular-mask.png`, existing FIELD-003 background.

> Edit image 1, the fixed SMALL ANGULAR footprint mask, into a second new environment-art style for future FIELD-002 ruins: OVERGROWN THORNED RUIN. Preserve the outer polygon shape and orientation, all artwork within the footprint, transparent canvas outside. Show a mostly ground-level patch of dark olive moss, tough low thorny root tendrils, two or three large clusters of burgundy-brown bramble stems, and a few pale broken paving stones partly swallowed by growth. The perimeter is clearly marked by a dense but low continuous line of thorn roots and a few tiny muted amber thorn tips, not a stone wall. Human pursuers in boots and armor can plausibly push through or step over these low plants; the monster player treats the thorned zone as blocked. Keep the silhouette readable from far above; avoid tiny leaf noise. This must look strongly different from pale glowing royal ward and from brown puddled paving: vegetation dominates, no luminous runes, no water. Image 2 is only a reference for the existing game's 3/4 top-down ruin perspective and earthy muted palette. Stylized 2D storybook cutout game sprite, strong broad shapes, thick slightly irregular deep-plum #241B2B outer contour, matte olive, ochre and burgundy with simple cel shading and restrained paper texture. Transparent background with generous padding; no external ground plane or cast shadow. No tall tree, shrub wall, building, masonry wall, arch, pillar, boulder, characters, text, frame, watermark, photorealism or 3D.

## Overgrown Thorned Ruin: Medium Linear

References: `medium-linear-mask.png`, `thorn-small-angular-preview.png`.

> Edit image 1 into the MEDIUM LINEAR version of the OVERGROWN THORNED RUIN gameplay environment sprite. Preserve the exact long gently bent footprint and horizontal orientation of image 1; keep everything inside it and outside transparent. Image 2 is the companion small thorn-zone sprite and defines the style: dark olive moss, burgundy-brown bramble roots, muted amber thorns, partly buried pale broken paving, matte storybook colors, deep-plum outer contour. Paint a low tangled ribbon of thorn roots that follows the curved long footprint, with two widely separated clusters of thorny burgundy growth and a few fragmented old paving stones visible between. The border is a continuous low root braid, readable as a restricted zone without appearing to be a high impassable wall. Human pursuers in boots could step across or force their way through; the player avoids it. No magical glow, water pool, stone wall, house, arch, pillars, trees, high bushes or solid block. Stylized 2D storybook cutout, 3/4 top-down game sprite, broad readable color masses, thick slightly uneven deep-plum #241B2B outline, simple cel shading, restrained texture. Transparent background and ample padding; no external ground plane, cast shadow, characters, text, frame, watermark, photorealism or 3D. Keep the silhouette of image 1, don't turn it into a round patch.

## Overgrown Thorned Ruin: Large Round

References: `large-round-mask.png`, `thorn-small-angular-preview.png`,
`thorn-medium-linear-preview.png`.

> Edit image 1 into the LARGE ROUND OVERGROWN THORNED RUIN environment sprite for future FIELD-002. Image 1 is the fixed broad asymmetric rounded footprint mask: preserve the outer shape, orientation and transparent canvas. Images 2 and 3 are the same-family small and medium references. Fill this wide patch with dark olive and dusty gold groundcover, wide braided burgundy-brown thorn roots weaving across old partly buried limestone mosaic, three separated clusters of low red-leaf bramble, and several large pale fallen flagstone islands. A continuous low braided thorn root perimeter with sparse muted amber thorn points defines the restricted zone. From a gameplay height the vegetation and stones should resolve into 3-4 broad areas, not busy fine detail. Human pursuers can plausibly stride through these low plants while the playable monster cannot enter the thorned zone. Keep the scene low, no wall-like ring or mass taller than knee height. Distinct from pale luminous ward and brown puddled ruin styles: vegetation dominates, no rune glow, no water. Stylized 2D storybook cutout, 3/4 top-down game sprite, thick slightly irregular deep-plum #241B2B outer contour, matte desaturated olive/burgundy/ochre, simple cel shading, restrained texture. Transparent background with generous padding. No high wall, tree, building, arch, pillar, large solid boulder, characters, text, UI, frame, watermark, ground beyond footprint, baked shadow, photorealism or 3D. Keep exactly the same silhouette as image 1.

## Monster-catching ground net: Small Angular

References: `small-angular-mask.png`, existing FIELD-003 background.

> Use case: stylized-concept. Create a NEW GAMEPLAY ENVIRONMENT SPRITE CONCEPT for the SMALL ANGULAR footprint shown in image 1. Preserve image 1's outer polygon silhouette and orientation, transparent outside, generous padding. Concept: an abandoned royal MONSTER-CATCHING GROUND NET in the border ruins, not a paved surface. A broad coarse rope mesh lies loosely across dirt, weighted by four flat iron corner anchors and a few small stakes; a few tied copper bells and worn burgundy fabric knots mark it as a human-made trap. The rope has a subtle protective warm amber glint where strands intersect, implying it snags monsters while human pursuers can stride over the flat mesh. Large clearly legible rope lattice with open holes, not intricate tiny grid noise. Only tiny fragments of old stone ground underneath, no cracked mosaic, no blue-water puddles, no royal magic floor circle, no bramble vegetation. Image 2 is solely for existing ruined-border 3/4 top-down storybook palette and material mood. Thick slightly irregular deep-plum #241B2B outline, matte desaturated rope tan, rusted iron, burgundy knots, broad cel-shaded shapes, no 3D. The net and anchors stay flat/ankle-high so enemies walking across is plausible; no tall fence, wall, building, large boulder, hole, character, ground plane beyond footprint, shadow, text, UI, frame or watermark. Transparent background.

## Monster-catching ground net: Medium Linear

References: `medium-linear-mask.png`, `net-small-angular-preview.png`.

> Make a MEDIUM LINEAR member of the MONSTER-CATCHING GROUND NET obstacle family. Image 1 is the exact bent long strip mask: preserve its overall narrow curved footprint, no rectangle or round expansion, transparent outside. Image 2 is the small net sprite and defines design language: rough tan braided rope, a small number of flat rusty iron anchors, tied burgundy cloth scraps, tiny copper bells, subtle warm amber glints at some knots, dusty ruined ground. For this variant, a long narrow trapping net snakes along the bent strip, with only 2-3 rows of BIG rope cells; weighted anchor plates and low pegs at the two ends and one bend. Most of the shape must read as flexible net and exposed dirt, not as a stone mosaic floor. Humans can step over the low loose mesh while it catches playable monsters. 3/4 top-down gameplay sprite, stylized 2D storybook cutout, matte desaturated materials, thick slightly uneven deep-plum #241B2B silhouette line, simple cel shading. No ground beyond the contour, no high posts, walls, houses, masonry platform, water, vines, circles of runes, characters, readable text, frame, watermark, photorealism, 3D or cast shadow. Transparent background and generous padding. Keep mask shape from image 1.

## Monster-catching ground net: Large Round

References: `large-round-mask.png`, `net-small-angular-preview.png`.

> Create the LARGE ROUND variant of a royal MONSTER-CATCHING GROUND NET obstacle for the future FIELD-002 ruins. Image 1 is the exact large asymmetric footprint; preserve its broad outline and orientation with TRANSPARENT RGBA outside, no haze or vignette outside the silhouette. Image 2 is the small companion net sprite and style reference. Paint an expansive irregular but clearly ground-level rope web covering much of the shape: a handful of broad woven lattice cells, heavier perimeter ropes, six flat iron anchor plates with low pegs, a few copper bells and tattered burgundy fabric ties, tiny warm amber enchanted knots. Underneath is mostly exposed dusty earth with sparse loose stones; this is a net/trap, not a stone-paved courtyard. Leave the rope mesh open enough that humans crossing it look plausible, while the magic catches monsters. Draw 3-4 large readable net sections, no dense tiny pattern. Stylized 2D storybook cutout in 3/4 top-down gameplay view, matte desaturated tan/rust/burgundy, thick slightly irregular deep-plum #241B2B outer contour, simple cel shading. Absolutely no external background scene, fog, vignette, cast shadow, wall, building, high pillar, boulder, water, plants, rune-circle flooring, people, letters, UI, frame, watermark, photorealism or 3D. Only the standalone transparent asset.

## Consecrated incense: Small Angular

References: `small-angular-mask.png`, existing FIELD-003 background.

> Create a wholly different concept for the SMALL ANGULAR ruin obstacle footprint in image 1: a CONSECRATED INCENSE EXCLUSION PATCH. Preserve image 1's angular outer footprint and orientation, with truly transparent background outside and no vignette. A single low weathered brass censer on a squat stone dish sits near one corner, surrounded by a broad ground-hugging carpet of pale ivory and cool blue incense smoke that fills the irregular area, a few warm ember points and ash traces. Several small brass boundary discs embedded flush in dusty earth give the smoke a clear continuous limit. The smoke is supernatural incense that repels playable monsters, but human pursuers can walk directly through it; there is no physical wall. Make smoke the dominant visual mass rather than stone paving. The censer must be ankle-high, no tall tripod. Existing FIELD-003 image 2 gives 3/4 top-down storybook ruin lighting and muted colors, but do not copy its walls or ground layout. Stylized 2D storybook cutout sprite, readable from game height, thick slightly irregular deep-plum #241B2B outer silhouette, matte cream/blue-grey smoke and aged brass, 2-3 broad cel-shaded shapes, subtle texture. No stone mosaic floor, net or ropes, brambles, water pool, wall, building, pillar, arch, humans, writing, UI, frame, watermark, baked shadow beyond the footprint, photorealism or 3D. Transparent RGBA canvas with ample padding.

## Consecrated incense: Medium Linear

References: `medium-linear-mask.png`, `incense-small-angular-preview.png`.

> Create the MEDIUM LINEAR version of the CONSECRATED INCENSE EXCLUSION PATCH shown in image 2. Image 1 is the fixed long gently curved mask; preserve that bent narrow footprint and horizontal orientation. True transparent canvas outside the footprint, no vignette or shadow beyond it. Along this low ruined-earth strip, arrange three small weathered brass incense dishes, separated by broad pale ivory and cool blue smoke plumes that drift and overlap at ground level to fill the entire curved strip. Small ember flecks and flush brass marker discs define the two edges. The smoke is a monster-repelling consecrated haze; human pursuers can pass through it freely. Smoke, rather than stone, net, vegetation, or mosaic, must dominate. Keep censers ankle-high with no tripods or wall. Match image 2's matte storybook colors and shape language. Stylized 2D storybook cutout, 3/4 top-down gameplay view, thick irregular deep-plum #241B2B footprint outline, broad readable masses, simple cel shading, restrained paper texture. No stone floor mosaic, rope net, thorn vines, tall structure, house, wall, pillar, arch, water pool, people, writing, frame, watermark, photorealism or 3D. Standalone transparent sprite only.

## Consecrated incense: Large Round

References: `large-round-mask.png`, `incense-small-angular-preview.png`,
`incense-medium-linear-preview.png`.

> Create the LARGE ROUND version of the CONSECRATED INCENSE EXCLUSION PATCH for future FIELD-002 ruins. Image 1 is the fixed broad asymmetric rounded mask; preserve its exact outline and orientation, fully transparent outside. Images 2 and 3 are companion small/medium incense sprites and define the visual language. The large footprint is a broad supernatural carpet of ground-hugging ivory and blue-grey incense smoke, with five LOW weathered brass censers spaced irregularly, a few dim orange ember flecks, ash traces and flush brass boundary discs. Several large smoke swirls overlap but leave glimpses of dusty ruin soil; smoke clearly dominates, not a paved floor. The consecrated haze repels monsters but human pursuers walk through unhindered. The contour should be readable at gameplay scale without becoming a hard wall or raised cloud bank. Stylized 2D storybook cutout in 3/4 top-down game view, matte broad shapes, deep-plum #241B2B irregular outer outline, simple cel shading, restrained paper texture, generous transparent padding. No dense repeated tile pattern, mosaic, net, vines, water, standing walls, building, pillar, arch, tall censer, characters, text, UI, frame, watermark, ground beyond footprint, cast shadow, vignette, photorealism or 3D. Standalone transparent PNG concept.

## Selected small v002 replacements

Built-in image generation, transparent background. First reference is the
new contour mask; second is the previous same-family small art, used for style.

### Royal ward small v002

References: `ward-small-angular-v2-mask.png`, `ward-small-angular-preview.png`.

> Use case: stylized-concept. Create a NEW 2D game obstacle illustration for FIRST reference's irregular seven-corner footprint, keeping its distinctive boundary. SECOND reference provides the pale-limestone ancient royal ward art style only. Fill the new shape with cracked cream stone paving, broken blue-and-gold protection circuit near the perimeter, one off-center simple star seal. Fixed 3/4 top-down view and orientation; ground level, no wall or upright structure. A player is blocked by the magical floor but human pursuers pass over it. Genuinely transparent outside the exact first-reference silhouette, no glow or shadow outside, deep plum contour, no text/characters/background.

### Thorns small v002

References: `thorn-small-angular-v2-mask.png`, `thorn-small-angular-preview.png`.

> Use case stylized-concept. Make one new top-down storybook game obstacle sprite. FIRST image is a unique irregular angular footprint with an inward notch; keep that recognizable fixed outline. SECOND image is the overgrown thorned ruin style reference only. Inside new shape, intertwine low burgundy-brown thorn roots and muted olive groundcover around a few partly buried pale stone shards; different arrangement from the reference. Deep plum edge, matte 3/4 top-down lighting. Ground-hugging plants so human pursuers can step across while player cannot. Transparent outside, no tree, wall, building, text, character, glow or exterior shadow.

### Ground net small v002

References: `net-small-angular-v2-mask.png`, `net-small-angular-preview.png`.

> Use case stylized-concept. Create a NEW small royal monster-catching ground net illustration for a game. First image defines a UNIQUE irregular polygon footprint with a clear inward notch on its right edge; preserve that boundary, no generic circle or hexagon. Second image is only style reference: laid-flat thick tan braided rope net, amber knots, a couple of low rusted iron anchors, burgundy fabric scraps, dusty ruin soil. Adapt rope lattice to bend around the notch. Fixed angled top-down orientation, deep plum outer contour, transparent outside. Low obstacle so human pursuers cross while the player is stopped by trap magic. No upright posts, wall, house, characters, text, external shadow or glow.

## Fourth fixed upright variants (historical 20-asset library)

Each request used built-in image generation with a genuinely transparent
background. Reference image 1 is the named new upright contour mask; reference
image 2 is the matching family's earlier medium illustration as a style guide.
Neither reference image was a pixel-edit target. The first wet-upright output
was regenerated to improve alignment; the saved preview is the last variant.

### Wet broken paving upright

References: `wet-upright-round-mask.png`, `medium-linear-preview.png`.

> Use case stylized-concept. One transparent 2D game sprite in FIRST reference's vertical footprint, same painterly angled top-down style as SECOND reference: ground-level broken tan stone paving and small blue-gray rain puddles, sparse moss, deep plum outer contour. Fixed north-south orientation. Transparent outside, no houses/walls/people/text.

### Ancient royal ward upright

References: `ward-upright-angular-mask.png`, `ward-medium-linear-preview.png`.

> Use case stylized-concept. One transparent 2D game obstacle sprite. FIRST reference defines a jagged upright north-south ground footprint, SECOND defines the painterly angled top-down ancient royal ward style. Fill footprint with pale cracked limestone tesserae and a broken geometric blue-and-gold protective sigil, small diamond insets, faded blue light. Vary motif placement from reference. Deep plum outer outline, flat low ground relief only; no columns, walls, people, text, backdrop, floating effects. Player blocks on footprint, spectral enemies can pass through; fixed orientation, no rotation.

### Overgrown thorns upright

References: `thorn-upright-linear-mask.png`, `thorn-medium-linear-preview.png`.

> Use case stylized-concept. Create one transparent 2D game obstacle cutout. FIRST reference is an extremely long narrow upright ground footprint; SECOND reference is the painterly angled top-down overgrown thorned ruins art style. New arrangement: north-south length of dense intertwining dark brown roots and low burgundy thorn vines over flattened olive moss, a few broken pale paving fragments caught inside, small gold thorns. Keep width narrow and silhouette from first reference, deep-plum boundary, flat ground-hugging form. Fixed vertical orientation, no wall, trunk, house, people, text or backdrop. Player collides with this, spectral enemies may walk over it.

### Monster-catching net upright

References: `net-upright-angular-mask.png`, `net-medium-linear-preview.png`.

> Use case stylized-concept. ONE transparent 2D game obstacle sprite. FIRST reference is a jagged upright footprint. SECOND reference is the painterly top-down royal monster-catching net style: thick braided tan rope, gold knots, burgundy cloth tatters, low metal anchor plates, dusty ground. Compose a north-south laid-flat trap net stretched diagonally between small ground pegs within the FIRST silhouette; visually distinct asymmetric net arrangement. Fixed upright orientation, deep plum outer outline, transparent outside. Ground-level snagging obstacle only, no high posts, cage, walls, house, character, text or backdrop; spectral enemies can pass while the player must avoid.

### Consecrated incense upright

References: `incense-upright-round-mask.png`, `incense-medium-linear-preview.png`.

> Use case stylized-concept. ONE transparent 2D game obstacle sprite. FIRST reference defines elongated upright irregular ground silhouette, SECOND reference defines painterly 3/4 top-down consecrated incense motif. New asymmetric arrangement: two small low brass incense bowls set in cracked paving, a broad still-looking bank of ivory and powder-blue aromatic haze lying over the ground between them, tiny warm embers and a few white flowers. The haze is a fixed illustrated magical residue (no motion required), dense enough to explain why player avoids walking through yet spectral foes can cross. Match deep plum boundary, fixed upright orientation, transparent outside. No tall smoke plume, buildings, people, text or backdrop.
