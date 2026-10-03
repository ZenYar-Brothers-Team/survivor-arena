"""Exports the Blender TRAP-002 cross (revision 3) as an FBX for Unity: geometry and hierarchy only.

Run headless:  blender.exe -b trap-002-cross.blend --python export_unity_fbx.py
The Blender materials are procedural emission shaders and do not transfer; the FBX only carries material names, which the Unity
builder (Assets/Game/Traps/Editor/TrapCrossPrefabBuilder.cs) maps onto the project's TrapCrossMatte materials.
The file keeps Blender's own axes (Z up); the Unity importer bakes the conversion (ModelImporter.bakeAxisConversion, set by the
Unity builder) so the prefab has identity rotation and unit scale: ground XZ, Y up.
"""
import bpy
import bmesh
import math
import re
from pathlib import Path

# The authored shell is 0.009 thick (about half a pixel in game, which flickers when the camera moves); 0.042 was too heavy.
# Only for this export (the .blend itself is not saved) the continuous shell is pushed out to 0.020 in total, the agreed middle
# shared by all trap models (build_trap_model.py CONTOUR).
CONTOUR_EXTRA = 0.011

ROOT = Path('D:/GitHub/survivor-arena')
TARGET = ROOT / 'Assets/Art/Traps/trap-002.fbx'
TARGET.parent.mkdir(parents=True, exist_ok=True)

scene = bpy.data.scenes['FIELD004_Cross_Blender_Comparison']
bpy.context.window.scene = scene
for ob in bpy.data.objects:
    ob.select_set(False)
for ob in scene.objects:
    if ob.type == 'MESH' and ob.name.endswith('_Contour'):
        mesh = bmesh.new()
        mesh.from_mesh(ob.data)
        mesh.normal_update()
        # The shell's faces are flipped, so its normals point inwards: outwards is against them.
        for vertex in mesh.verts:
            vertex.co -= vertex.normal * CONTOUR_EXTRA
        mesh.to_mesh(ob.data)
        mesh.free()
# The arm tips take the two tones of the projectile sprites (same colours as the other trap models), light on the upper side.
TIP_LIGHT = (0.60, 0.60, 0.62)
TIP_DARK = (0.66, 0.66, 0.69)  # the side that merged with the iron: now a touch lighter than TIP_LIGHT
TIP_LIGHT_DIRECTION = (-0.3, 0.9)
tip_materials = []
for name, rgb in (('tip-light', TIP_LIGHT), ('tip-dark', TIP_DARK)):
    material = bpy.data.materials.new('CrossBlender_' + name)
    material.diffuse_color = (*rgb, 1)
    tip_materials.append(material)
for ob in scene.objects:
    if ob.type == 'MESH' and re.match(r'^Arm-\d-Tip$', ob.name):
        ob.data.materials.clear()
        for material in tip_materials:
            ob.data.materials.append(material)
        yaw = ob.rotation_euler.z
        for polygon in ob.data.polygons:
            cx = polygon.center.x * math.cos(yaw) - polygon.center.y * math.sin(yaw)
            cy = polygon.center.x * math.sin(yaw) + polygon.center.y * math.cos(yaw)
            polygon.material_index = 0 if TIP_LIGHT_DIRECTION[0] * cx + TIP_LIGHT_DIRECTION[1] * cy > 0 else 1
exported = [ob for ob in scene.objects if ob.type in {'EMPTY', 'MESH'}]
for ob in exported:
    ob.select_set(True)
bpy.context.view_layer.objects.active = scene.objects['TRAP-002_Blender']
# The head keyframes only drive the preview render; Unity turns the head itself.
scene.frame_set(1)
bpy.ops.export_scene.fbx(
    filepath=str(TARGET), use_selection=True, object_types={'EMPTY', 'MESH'},
    apply_scale_options='FBX_SCALE_ALL', axis_forward='Y', axis_up='Z', bake_space_transform=False,
    use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False, bake_anim=False,
    use_custom_props=False, path_mode='AUTO')
print('FBX EXPORTED', TARGET, len(exported), 'objects')
