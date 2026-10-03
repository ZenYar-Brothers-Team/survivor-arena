# FIELD-004 TRAP-002 3D experiment evidence

Revision 2: user requested no shadows, a view further from above, and closer resemblance to the previous raster illustration. Camera elevation is about 57°; the shader applies only object-space pigment, without directional shading. All mesh renderers have shadow casting/receiving off. The revised palette, wood grain, subdued metal wear and plum contour refer to the TRAP-002 candidate. Native Unity build and 72-frame rotation checks passed again; frame 000 was inspected. This remains a visual experiment, not final art approval.

Scope: user-requested volumetric analogue of «Крест», isolated from gameplay. Model source and review instructions: [prototype README](../../../Art/Prototypes/field004-trap-cross/README.md).

Unity 6000.6.0f1 batch build compiled the editor assembly and shader, saved a mesh prefab and review scene, rendered 72 frames, and exited with code 0. Completion marker: `TRAP CROSS PROTOTYPE COMPLETE` in `TestResults/trap-cross-3d-build.log`.

[Verification](../../../Art/Prototypes/field004-trap-cross/preview/verification.json) records 40 mesh parts, zero physics components, stationary base transforms, unchanged head attachment and unchanged spear orbit radius across the complete rotation. Frames 000 and 009 were visually inspected: geometry renders with wood/metal colours and a stable base; the head changes visible faces under the fixed tilted orthographic camera.

No Unity Test Runner execution or gameplay integration is claimed. The prototype authoring axes are XZ ground/Y up; the production XY projection and sorting have not been adapted. Final style requires visual feedback. No raster master or production art packet was changed; native render previews remain outside Assets. Execution status belongs only to STATUS.
