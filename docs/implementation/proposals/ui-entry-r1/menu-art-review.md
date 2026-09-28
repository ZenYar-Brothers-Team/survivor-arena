# Main Menu D — preview art review

Date: 2026-09-28
Scope: illustration candidate for [UI entry R1](../2026-09-28-ui-entry-r1.md).
Execution status is owned by [STATUS](../../STATUS.md).

## Creative direction and boundary

The user requested a coherent illustration of goblins escaping from knights because
the separate goblin sprite looked disconnected from the landscape. Keep original
menu variant A for comparison; B is the landscape without a hero and C is the hero
on a plain panel. D uses one unified illustration without a separate body overlay.

Feedback on v001: the art looks good, but the goblins should be less pitiful so the
scene does not suggest cruelty; add a throwing action, for example a kettle toward
the knights. v002 implements a mischievous chase, confident smiles and a backward
kettle throw, with no impact or injury. The quiet left-hand menu area is preserved.

Follow-up on v002: the original knights were better; no defensive/dodging gesture.
The goblins should not smile, but should not have bruises or look pitiful either.
The throwing arm and kettle trajectory were unsatisfactory. The user allowed
another attempt or omitting the kettle. v003 takes the latter option: edit v001
faces/skin only, keep original knights and natural running poses, no kettle.

This feedback approves the direction, not the final v003 bytes or a Unity import.
No runtime asset ID, binding, selected-master or fabricated approval is created.
Character/field previews are unchanged. A/B/C remain selectable; v001/v002 are retained.

## Preview files and provenance

Generated/edited with the built-in image_gen tool using the imagegen skill; no CLI
or third-party generator. Original tool outputs are preserved. Workspace copies
are byte-identical opaque PNGs, outside Assets and ignored by Git. Do not commit
these images as runtime art before candidate selection.

| Revision | Workspace preview | SHA256 |
|---|---|---|
| v001 — original chase | `TestResults/ui-entry-r1/menu-escape-v001.png` | `64c1506f061b2c7797d3e0e8d6f6ea808f86c25bdc7bd95b3cabb34e49aca7df` |
| v002 — confident goblins / kettle | `TestResults/ui-entry-r1/menu-escape-v002.png` | `df830b1b8945566c69fae009ab2e0ebe2cedad24690cfa3d19ff4656c08f0932` |
| v003 — focused escape / no kettle | `TestResults/ui-entry-r1/menu-escape-v003.png` | `8c1b29282f879ab1e48ce3d9e4cf9d3b8c0f13ef79bba757f457597a99a7b46e` |

Original output directory:
`C:/Users/zheni/.codex/generated_images/01a0e6e1-cd6a-71f1-a906-110c7ab6bfa5/`.
v001: `exec-19818b33-074b-430e-8b5b-6f300152a4f9.png`;
v002: `exec-c033e5af-9ce3-463f-9e4b-dfb51e9352de.png`;
v003: `exec-8acd63c3-bd7d-452b-8cb9-f889f7e9b51e.png`.
Both v002 and v003 edit inputs were the workspace v001 file, inspected before editing.

Open [D with menu](http://127.0.0.1:4180/?menu=D) using the local preview server.
All three PNGs are individually allowlisted; the server does not expose TestResults as
an unrestricted directory. A fresh checkout does not contain these ignored local
candidates: restore the exact files before opening D or running the full browser
check. A/B/C and the other screens use existing repository art.

## Approval and preparation gate

D motion preview uses v003 bytes unchanged: a slow whole-image CSS transform
and seven low-opacity procedural dust dots, isolated from menu controls. No image
editing, layer extraction, raster preparation or video generation occurs. This is
not character animation. Details and lifecycle limits are in the proposal/README.

The later E experiment adds separately generated background/foreground layers;
its distinct preview packet, exact prompts and provenance are in
[menu-depth-review.md](menu-depth-review.md). D and all its original files remain.

This is a preview/brief packet, not an approved runtime art-pipeline packet.
Per scripts/README.md §1, art_pipeline.py requires a selected candidate and real
approval evidence before preparing any source/master/runtime packet. It is not run
for these unapproved previews. After selection, prepare the approved packet with
its hash, run art_pipeline.py dry-run/apply and check_project.py --scope art using
the smoke-check safety procedure. Browser review does not replace those checks.
No Unity tests are needed or claimed for this HTML-only iteration.

## v003 — exact edit prompt (current preview)

```text
Use case: precise-object-edit. Asset type: main-menu illustration preview, revision v003.
Input image 1 is the ORIGINAL chase illustration and the exact edit target. Preserve this image as closely as possible; make a restrained local edit only to the two goblins' facial expressions and visible skin marks.
User direction: a middle ground between frightened, pitiful victims and smiling pranksters. Both goblins are seriously concentrating on escaping. Their faces show alertness, determination and mild tension from running, NOT happiness, grinning, laughing, terror or pleading. Mouths closed or very slightly parted for breathing, with no visible smiling teeth and no turned-up smile corners. Brows naturally focused, not dramatically raised in distress and not villainously angry. Keep their expressive goblin identities and eye directions. Remove bruise-like purple or red patches, black eyes, scratches, injuries, tears and distressed flushed patches from faces and exposed skin; healthy even olive-green goblin skin with normal cel-shading and the original paper texture. Do not make them babies or cute mascots.
ABSOLUTE INVARIANTS: keep all three knights EXACTLY as in the input: same pursuit positions, running poses, faces, equipment, spear and shield. No defensive gesture, dodging, raised hands, surprised reaction or new facial expression on any knight. Keep both goblins' original running body poses, normal compact bent arms and hands from this input, clothes, satchel, scarf, shoulder armor, proportions, silhouettes and positions. No throwing, no awkward twist, no extra arm, no new jump. NO KETTLE or any other airborne object, projectile or trajectory line: the user allowed omitting it. Preserve the village, path, fence, trees, entire left dark low-detail menu area, warm lighting, colors, drawing style, texture, perspective and 16:9 composition. Keep all details outside the two goblins' face/skin edits unchanged. No text, logo, watermark, border or UI. Opaque image.
```

## v001 — exact generation prompt

```text
Use case: illustration-story.
Asset type: PREVIEW candidate for a fantasy survival game's main menu background, landscape 16:9, ideally 1920x1080 or larger. One cohesive hand-drawn narrative illustration, no UI or lettering.
Primary request: a lively escape scene of small goblins fleeing an overzealous pursuit of human knights and guards along the edge of a fairy-tale village. The goblins are the sympathetic protagonists, frightened but resourceful, trying to survive rather than attack anyone.
Composition: reserve the left 38 percent as quiet deep charcoal-plum shadow and low-detail foliage, with a gradual natural transition into the painted scene, specifically for an existing left-aligned title and vertical menu to be overlaid later. Do not draw any letters, buttons, frames or actual UI there. Place all important faces and action within the right 60 percent, away from the edges. A readable curving diagonal path leads from an upper-right pursuing group toward two goblins escaping toward the lower center-right; their bodies, feet, gazes, dust and pursuers must all agree on the same chase direction. Show the two fleeing goblins in full body, with clear silhouettes, and about three pursuing human knights/guards farther behind, not an army of tiny figures.
Subjects: one youthful olive/moss-green goblin with a large expressive head, broad pointed ears, a rusty-red scarf, dusty teal patched tunic, simple small satchel, compact limbs and anxious determination; beside him a stockier older goblin in worn rust-brown work clothes, sturdy boots and improvised shoulder protection. Humans are visibly human, taller with smaller heads in relation to their bodies, upright disciplined shapes, steel and burgundy/linen equipment. No goblin faces or giant cute eyes on the humans. They are running to catch the goblins, not stabbing them.
Environment: simplified village edge with low woven fence, a distant thatched roof and a few broad trees, a winding dirt path and simple grass shapes. Setting supports the chase, with no need for a whole town panorama.
Style: unified stylized 2D storybook cutout illustration across CHARACTERS AND ENVIRONMENT. Thick, slightly irregular deep-plum ink contours, chunky shapes, matte flat colors, restrained two-tone cel shading, subtle dry-paper grain. A few bold forms instead of realistic detail. The background must NOT be a realistic oil painting behind cartoon sprites: everyone and everything is drawn together using the same brush, simplification, light and perspective. Warm ironic fairy tale, 70 percent charm and resourcefulness, 30 percent danger; not baby mascot art.
Palette: deep plum #241B2B / #302936 in shadows, olive goblin skin, rust/clay cloth, muted vegetation and earth, warm parchment highlights, restrained gold only on small armor details. A soft warm late-afternoon light on the figures; cooler quiet left side. Keep the silhouettes vivid without artificial glows.
Constraints: one complete opaque illustration, not transparent; no text, title, labels, logo, watermark, decorative border, UI elements, split panels, collage, copied sprite pasted onto background, photo-realism, 3D plastic rendering, pixel art, polished metal glare, violent injuries or gore. No extra limbs, cropped faces, oversized weapon protrusions, confusing tangles. The left menu space must remain genuinely readable and dark. This is a new scene concept, not a change to any production sprite.
```

## v002 — exact edit prompt

```text
Use case: precise-object-edit. Asset type: 16:9 main menu illustration, preview revision v002.
Input image 1 is the exact edit target. Edit this illustration, do not create a new composition.
Primary request: keep the beautiful unified scene, but make the goblins less pitiful and frightened, more cheeky, resourceful and spirited, and add a readable action of one goblin throwing a kettle back toward the pursuing knights. The mood should feel like a mischievous adventurous chase, not cruelty toward helpless creatures.
Change only: facial expressions and the thrower's upper-body/arm pose, plus the airborne kettle and small motion accents. Give the youthful scarf-wearing goblin a determined mischievous half-smile, relaxed confident brows and alert eyes instead of a scared pleading expression. The stocky older goblin should have a roguish confident grin, glance back over his shoulder and clearly be finishing a backward throw with one arm while still running forward. Show ONE small battered metal tea kettle with a distinct spout and handle flying behind him toward the lead knight, high enough to have an unambiguous silhouette, between thrower and pursuer. A subtle curved motion streak connects the released throwing hand to the kettle and indicates travel toward the knights, not toward the goblins. The lead knight may react with startled surprise, raising his shield; keep this a comic evasive moment, no contact or injuries. Keep anatomical arms/hands coherent, exactly two arms per goblin.
Invariants: preserve the same two goblins and their faces' identity, relative sizes, outfits, scarf, satchel and shoulder armor; same three human pursuers, same beautiful painted village, path, tree, warm evening light, palette, hand-drawn ink-and-paper texture, camera, 16:9 framing and full-body running silhouettes. Preserve the left 38 percent dark quiet empty area for menu text EXACTLY as it is: do not add characters, kettle, lettering, icons or ornaments there. All chase action stays on the right. Keep the coherent illustration style across every subject; do not paste in a differently rendered prop. No text, logos, UI, border, blood, bruises, tears, terrified pleading expressions, hitting, hot liquid splash or extra characters. Opaque image, no transparency.
```
