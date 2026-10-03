# TRAP-002 Blender comparison

Revision 3: `Hub-Top` uses a separate darker blue-grey steel material (sRGB 0.48, 0.51, 0.55) with subtle broad object-space mottling (noise scale 6). Spear steel and geometry are unchanged. The source script reproduces the material override; the preview is rerendered.

Revision 2 addresses top-face flicker: shaft tops originally coincided with crossbar tops at local Z=0.085. Shafts are raised by 0.018 and the Z crossbar lowered by 0.012. The original script's crossbar-name condition was corrected to `Crossbar-Z`. Native preview frames were rerendered; Unity remains unchanged.

User requested a Blender analogue of the Unity cross trap and asked that shimmer be reduced only in the new version. The original Unity assets and preview are untouched.

- `trap-002-cross.blend`: editable Blender 5.2.2 LTS scene; separate `StationaryBase` and animated `RotatingHead`.
- `build_model.py`: reproducible scene authoring. Creates a new scene, preserves existing scenes, saves a copy to this folder.
- `preview/index.html`: synchronised old/new comparison; 144 Blender renders at 1080 px, 30 fps playback, alongside byte-identical original Unity renders.
- `build-verification.json`: native model build properties. Render log: `TestResults/trap-cross-blender-render.log`.

Same tilted orthographic camera and palette as Unity revision 2. Ground is XY, vertical axis Z (Blender convention). Head revolves around Z. All materials use emission, so there is no directional illumination or cast/received light shadow. Grain is broad, object-space procedural pigment, not screen-space noise.

Changes aimed at reducing shimmer: bevelled edges, 48-sided cylinders, welded spear vertices, continuous expanded contour shells, slight separation of coincident crossbar surfaces, 1080px antialiased native renders, and 2.5° rather than 5° preview angle increments. These reduce likely sources; no claim that the original shimmer has been reproduced or isolated. Blender render quality does not prove equivalent Unity runtime quality. The new model has not been integrated into gameplay.

Procedural materials need conversion or baking for a Unity import; no unsupported promise of automatic shader transfer. This is a separate 3D experiment outside Assets and outside the raster art packet pipeline.
