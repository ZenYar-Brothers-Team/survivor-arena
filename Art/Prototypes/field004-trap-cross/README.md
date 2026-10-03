# TRAP-002: isolated 3D art prototype

User requested a volumetric analogue of the second FIELD-004 trap after rejecting the fit and screen-plane rotation of separate raster base/head candidates. This experiment evaluates real geometry; it does not approve a final art style or replace runtime presentation.

- Authoring input: `model.json` (palette, geometry, camera and preview settings).
- Unity prefab: `Assets/Art/Prototypes/Field004TrapCross/trap-002-cross.prefab`.
- Review scene: `Assets/Art/Prototypes/Field004TrapCross/trap-002-cross-review.unity`.
- Rebuild in Editor: **Tools > Survivor Arena > Art Prototypes > Build 3D Trap Cross**. Save existing scenes first. The command opens the isolated review scene.
- Browser review: `preview/index.html`, with 72 native Unity renders and an angle slider.
- Verification: `preview/verification.json`; batch build log: `TestResults/trap-cross-3d-build.log`.

The authoring scene uses XZ ground and Y up, with a tilted orthographic camera. Production gameplay uses XY; adapting projection and sorting remains separate work. Base and head are separate transforms sharing a physical axle. There are no gameplay or physics components. Editor code is excluded from player builds; the prefab is outside Resources and has no production binding.

This mesh experiment uses no generated/imported raster masters. Preview PNGs are Unity render outputs outside Assets; the raster art packet pipeline is not applicable to this prototype.

Revision 2 follows user feedback: camera elevation increased from about 28° to 57°, directional face shading removed, shadow casting/receiving disabled on all mesh renderers. Warm wood grain, mottled blue-grey iron, golden rivets and a thicker plum contour refer to the earlier TRAP-002 raster candidate. Pigment is procedural object-space colour, with no raster master edits.
