# Main Menu E — layered depth preview

Date: 2026-09-28
Parent: [menu art review](menu-art-review.md), [proposal](../2026-09-28-ui-entry-r1.md).
Execution status: [STATUS](../../STATUS.md).

## Request and method

The user requested a slightly changing viewing angle, separate layers, dust and
subtle changing light if not overly complex. This experiment keeps D unchanged
and adds E. It is a modest 2.5D parallax, not full 3D, skeletal animation or video.

Built-in image_gen / imagegen skill produced two preview layers from the inspected
`TestResults/ui-entry-r1/menu-escape-v003.png` (SHA256
`8c1b29282f879ab1e48ce3d9e4cf9d3b8c0f13ef79bba757f457597a99a7b46e`).
The background fills behind removed goblins; the foreground contains both goblins
with genuine alpha. AI extraction is not guaranteed pixel-identical to the source.
The foreground was visually fitted with CSS width/height 90%, left 7.5%, top 10%,
not raster resampling or destructive edits. Each canvas is 1672×941.

The current E foreground replaces the right goblin with Shepotka; the original
pair remains preserved. See [Shepotka edit packet and prompt](menu-shepotka-review.md)
for the new source, hash and user-requested atmosphere revision.

## Preview packet / provenance

| Layer | Workspace path | SHA256 |
|---|---|---|
| Backplate, RGB | `TestResults/ui-entry-r1/menu-depth-background-v001.png` | `426e9c8993c2ef0b3bb3a02d28d2a201efb9990ee278ebdd45f5940a3a43ece5` |
| Goblins, RGBA | `TestResults/ui-entry-r1/menu-depth-foreground-v001.png` | `a4f7a4e00159e52b93e8f1be97325672320870bb6e88ef3c515a89b2467cdb4e` |

Original output directory:
`C:/Users/zheni/.codex/generated_images/01a0e6e1-cd6a-71f1-a906-110c7ab6bfa5/`.
Backplate: `exec-e19f3d6a-6a51-4163-8be8-d9bd398c3ca2.png`;
foreground: `exec-a7fe2f6e-26dc-42f3-b8e2-cf47fa3a19dc.png`.
Originals are preserved; workspace copies are ignored and individually served by
the loopback review server. No Assets, import profiles, GUIDs, sprite registry,
manifest or selected-master changed. Candidate approval is still required before
preparing an approved packet through art_pipeline.py and scoped Unity art checks.
The tool is intentionally not run with fabricated approval for these previews.

## Motion limits

Two CSS transforms have a 20-second alternate drift. Pointer target is clamped
to −1…1 on each axis and smoothed in one temporary requestAnimationFrame loop;
background pointer contribution is 0.3% X / 0.2% Y, foreground 0.8% / 0.5%.
Background scale 1.05 provides overscan. Small rotateY stays below 0.55 degrees;
no limb deformation. Three separate soft rays sit behind the opaque goblin layer,
not over their faces. Ray gradients have a maximum alpha of 0.72, multiplied by
animated opacity 0.6–0.85; their angles drift ±3 degrees over 10/13/12-second
alternate cycles. Ray widths are 23%/11%/17% of the effect region; a 16%-wide
edge fade avoids a hard clipping seam toward the menu. No foreground brightness
filter or blanket lighting overlay.
E has 18 continuously drifting dust dots (7.5–16.8 px before animated scale,
radial soft edges, opacity up to 0.46, staggered
11–17-second cycles) explicitly above both goblins in the illustration region;
their starting positions alternate between the two characters. D retains its seven dots. Light and
dust animate without pointer input. All values are review tuning, not production
config. Menu controls are in a separate layer.

Toggle and reduced-motion disable animated transforms/particles and pointer loop;
leaving the menu or hiding the tab cancels pointer interpolation and resets its
values. CSS clocks pause on hidden tabs. This does not add a game Settings entry.
CSS keyframes are not Unity USS; runtime port/lifecycle/performance must be checked
separately after selection. No FPS/GPU claim is made from the browser mockup.

## Checks

Browser verification at 720p/1080p tests layer transforms at 0/5/10/15/20 s, image
coverage, actual pointer response, stationary actions, true foreground alpha,
light range, toggle/reduced-motion and screen cleanup. It also tests the ray clock
advancing without pointer input, rays behind the foreground, no brightness filter
or blanket overlay, edge fading, and medium-sized translucent particles with radial
edges and changing transforms. Tests confirm the six-degree ray sweep and visible
particles crossing opaque pixels of both characters over the cycle. Agent inspected composed
E captures at both resolutions. Full details are in [evidence](../../evidence/2026-09-28-ui-entry-r1-mockups.md).

## Exact backplate prompt

```text
Use case: precise-object-edit. Asset type: backplate for subtle 2.5D menu parallax. Input image 1 is the exact edit target. Remove ONLY the TWO large foreground goblins completely, including their clothing, scarf, bags, boots and shadows. Seamlessly reconstruct the dirt path, small grass and village scenery behind their silhouettes, matching the adjacent marks, perspective, color and ink-paper drawing style. Preserve the THREE chasing knights exactly where they are, in the same running poses. Preserve the village, houses, fence, tree, foliage, sky, warm light and full dark empty LEFT 38 percent menu area. Exact same original 16:9 framing, proportions, composition and resolution; do not crop, zoom, shift, restyle or recompose. This is the background layer of the SAME image, not a new illustration. NO goblins, substitute characters, silhouettes, kettle, thrown objects, text or UI. Opaque output.
```

## Exact foreground prompt

```text
Use case: background-extraction. Asset type: transparent foreground layer for subtle 2.5D menu parallax. Input image 1 is the exact edit target. Extract ONLY the TWO large foreground goblins, with their complete existing clothing, scarf, satchel, boots, shoulder armor and carried gear, onto a genuinely transparent background. Preserve their existing pixels/appearance, focused NON-SMILING faces without injuries, poses, contours, sizes and EXACT POSITIONS within the original FULL 16:9 canvas. Do not recenter them, enlarge them, crop to their bounding box, change their facial expressions or redraw poses. The left half must remain transparent and empty: keep full original framing so overlaying this PNG onto the original places the goblins in the same coordinates. Remove all environment, the three knights, tree, path, grass, sky, background, ground dust and ground shadows. Clean antialiased cutout boundaries, no white/colored fringe, no glow, no checkerboard baked into pixels, no text, no extra objects, no kettle. Both goblins fully intact including fingers and boots, exactly as in the source.
```
