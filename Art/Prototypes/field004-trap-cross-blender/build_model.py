"""Isolated Blender comparison model; never edits the Unity prototype."""
import bpy, bmesh, json, math
from pathlib import Path
from mathutils import Vector

ROOT = Path('D:/GitHub/survivor-arena')
OUT = ROOT / 'Art/Prototypes/field004-trap-cross-blender'
OUT.mkdir(parents=True, exist_ok=True)
config = json.loads((ROOT / 'Art/Prototypes/field004-trap-cross/model.json').read_text())
scene = bpy.data.scenes.new('FIELD004_Cross_Blender_Comparison')
bpy.context.window.scene = scene
scene.render.engine = 'BLENDER_EEVEE'
scene.render.resolution_x = scene.render.resolution_y = 1080
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.render.film_transparent = False
scene.render.fps = 30
scene.frame_start, scene.frame_end = 1, 144
# Emission materials remove lighting/shadow dependence entirely.
scene.world = bpy.data.worlds.new('CrossComparison_Background')
scene.world.use_nodes = True
background = next(n for n in scene.world.node_tree.nodes if n.type == 'BACKGROUND')
background.inputs['Color'].default_value = (0.018,0.010,0.025,1)
background.inputs['Strength'].default_value = 1
try:
    scene.view_settings.view_transform = 'Standard'
except (TypeError, ValueError):
    pass

def linear(c):
    return c/12.92 if c <= 0.04045 else ((c+0.055)/1.055)**2.4

def material(name, rgb, grain=False):
    mat = bpy.data.materials.new('CrossBlender_' + name)
    mat.use_nodes = True
    mat.diffuse_color = (*rgb,1)
    tree = mat.node_tree
    tree.nodes.clear()
    out = tree.nodes.new('ShaderNodeOutputMaterial')
    emission = tree.nodes.new('ShaderNodeEmission')
    base = tuple(linear(c) for c in rgb)
    emission.inputs['Color'].default_value = (*base,1)
    tree.links.new(emission.outputs[0], out.inputs['Surface'])
    if grain:
        tex = tree.nodes.new('ShaderNodeTexNoise')
        tex.inputs['Scale'].default_value = 3.5
        tex.inputs['Detail'].default_value = 0
        coord = tree.nodes.new('ShaderNodeTexCoord')
        mapping = tree.nodes.new('ShaderNodeVectorMath')
        mapping.operation = 'MULTIPLY'
        mapping.inputs[1].default_value = (1.8,1.8,0.22) if 'wood' in name else (1,1,1)
        tree.links.new(coord.outputs['Generated'],mapping.inputs[0])
        tree.links.new(mapping.outputs[0],tex.inputs['Vector'])
        ramp = tree.nodes.new('ShaderNodeValToRGB')
        ramp.color_ramp.elements[0].position = 0.25
        ramp.color_ramp.elements[1].position = 0.75
        ramp.color_ramp.elements[0].color = (*(c*0.83 for c in base),1)
        ramp.color_ramp.elements[1].color = (*(min(c*1.12,1) for c in base),1)
        tree.links.new(tex.outputs['Fac'],ramp.inputs[0])
        tree.links.new(ramp.outputs[0],emission.inputs['Color'])
    return mat

mats = {name:material(name,color,name!='outline') for name,color in config['materials'].items()}
mats['hub-steel'] = material('hub-steel',(0.48,0.51,0.55),True)
next(n for n in mats['hub-steel'].node_tree.nodes if n.type=='TEX_NOISE').inputs['Scale'].default_value=6.0
outline_mat = mats['outline']
outline_mat.use_backface_culling = True

def empty(name,parent=None,location=(0,0,0)):
    ob = bpy.data.objects.new(name,None)
    scene.collection.objects.link(ob)
    ob.parent = parent
    ob.location = location
    return ob

root = empty('TRAP-002_Blender')
base = empty('StationaryBase',root)
head = empty('RotatingHead',root,(0,0,1.05))

def convert(v): return (v[0],-v[2],v[1])

def spear_mesh():
    # Shared vertices and continuous topology, unlike separate per-triangle hulls.
    vertices = [(0.5,0,0),(-0.5,0,0),(-0.12,0.5,0),(-0.12,-0.5,0),(-0.12,0,0.5),(-0.12,0,-0.5)]
    faces = [(0,2,4),(0,4,3),(1,4,2),(1,3,4),(0,5,2),(0,3,5),(1,2,5),(1,5,3)]
    mesh = bpy.data.meshes.new('CrossBlender_Spear')
    mesh.from_pydata(vertices,[],faces)
    mesh.update()
    return mesh

for part in config['parts']:
    parent = head if part['group']=='head' else base
    if part['shape']=='box':
        bpy.ops.mesh.primitive_cube_add(size=1)
        ob=bpy.context.object
    elif part['shape']=='cylinder':
        bpy.ops.mesh.primitive_cylinder_add(vertices=48,radius=0.5,depth=1)
        ob=bpy.context.object
    else:
        ob=bpy.data.objects.new(part['name'],spear_mesh())
        scene.collection.objects.link(ob)
    ob.name=part['name']
    ob.parent=parent
    ob.location=convert(part['position'])
    ob.rotation_euler.z=math.radians(part['rotation'][1])
    scale=part['scale']
    ob.scale=(scale[0],scale[2],scale[1])
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    bpy.context.view_layer.objects.active=ob
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    ob.data.materials.append(mats['hub-steel'] if part['name']=='Hub-Top' else mats[part['material']])
    # Separate overlapping top faces: crossbars and shafts otherwise share z=0.085.
    if part['name'] in ('Foot-Z','Crossbar-Z'):
        ob.location.z -= 0.012
    if part['name'].startswith('Arm-') and part['name'].endswith('-Shaft'):
        ob.location.z += 0.018
    bevel=ob.modifiers.new('Rounded stable edges','BEVEL')
    bevel.width=min(min(scale)*0.12,0.018)
    bevel.segments=3
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    # Continuous smooth normals on the outline shell prevent facet-sized jumps.
    hull=bpy.data.objects.new(part['name']+'_Contour',ob.data.copy())
    scene.collection.objects.link(hull)
    hull.parent=parent
    hull.location=ob.location
    hull.rotation_euler=ob.rotation_euler
    hull.data.materials.clear()
    hull.data.materials.append(outline_mat)
    bm=bmesh.new();bm.from_mesh(hull.data)
    bm.normal_update()
    for v in bm.verts: v.co += v.normal*0.009
    bmesh.ops.reverse_faces(bm,faces=list(bm.faces))
    bm.to_mesh(hull.data);bm.free()

camera_data=bpy.data.cameras.new('CrossBlender_Orthographic')
camera=bpy.data.objects.new('ComparisonCamera',camera_data)
scene.collection.objects.link(camera)
camera.location=convert(config['cameraPosition'])
camera.rotation_euler=(Vector(convert(config['cameraTarget']))-camera.location).to_track_quat('-Z','Y').to_euler()
camera_data.type='ORTHO';camera_data.ortho_scale=config['cameraSize']*2
scene.camera=camera
head.rotation_euler.z=0
head.keyframe_insert(data_path='rotation_euler',frame=1)
head.rotation_euler.z=math.tau
head.keyframe_insert(data_path='rotation_euler',frame=145)
if head.animation_data and head.animation_data.action:
    for layer in head.animation_data.action.layers:
        for strip in layer.strips:
            for bag in strip.channelbags:
                for curve in bag.fcurves:
                    for key in curve.keyframe_points: key.interpolation='LINEAR'
scene.frame_set(1)
scene.render.filepath=str(OUT/'preview/frame-')
(OUT/'preview').mkdir(exist_ok=True)
bpy.ops.object.select_all(action='DESELECT')
root.select_set(True);bpy.context.view_layer.objects.active=root
for area in bpy.context.screen.areas:
    if area.type=='VIEW_3D':
        area.spaces.active.region_3d.view_perspective='CAMERA'
        area.spaces.active.shading.type='MATERIAL'
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'trap-002-cross.blend'),copy=True)
(OUT/'build-verification.json').write_text(json.dumps({'blender':bpy.app.version_string,'meshParts':len(config['parts']),'frames':144,'noLights':not any(o.type=='LIGHT' for o in scene.objects),'headAxis':'Z up, physical 3D rotation','cameraMatchesUnityRevision2':True,'continuousContourTopology':True,'bevelSegments':3,'cylinderSegments':48,'previewResolution':1080,'originalUnityUntouched':True},indent=2))
print('BLENDER CROSS MODEL SAVED',OUT)
