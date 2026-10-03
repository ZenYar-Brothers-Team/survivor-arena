"""Builds one FIELD-004 trap as a 3D model (Blender) following the 2D candidate art and the TRAP-002 cross reference.

Run headless, one model per run (object names must stay unique inside a file):
    blender.exe -b --python build_trap_model.py -- TRAP-005
Writes Assets/Art/Traps/<id>.fbx (geometry, hierarchy, material names), <id>.blend and preview/<id>.png next to this script.
Conventions shared with the cross reference: Blender axes (Z up); a root empty with StationaryBase and RotatingHead children;
the head turns about Z through the pivot, and at rotation 0 it faces +X (screen right in game). Materials are named
CrossBlender_<key>; the Unity builder maps those names onto the project's TrapCrossMatte materials.
Every part gets a bevel and a continuous inverted-hull contour shell of CONTOUR thickness (one value for all models).

Style rules taken from the reference cross (revision 2 of this file):
- chunky parts with firm edges, a repeated rhythm of wooden shaft / iron socket / steel tip;
- bronze rivets on every iron plate, socket and band (RIVET_*), never bare iron surfaces;
- dark blue-grey iron (not light steel) for large surfaces; steel only for tips, blades and small accents;
- wood shown as planks (alternating wood / wood-light with seams) wherever a surface is large.
"""
import bpy, bmesh, math, sys, json
from pathlib import Path
from mathutils import Vector

ROOT = Path('D:/GitHub/survivor-arena')
OUT = ROOT / 'Art/Prototypes/field004-trap-models'
FBX_DIR = ROOT / 'Assets/Art/Traps'
# Contour shell thickness in model units. 0.009 flickered (about half a pixel in game), 0.042 read as too heavy; this is the
# agreed middle (about one pixel at the 10-unit-high game view with the 0.6 model scale).
CONTOUR = 0.020
RIVET_RADIUS = 0.032
RIVET_HEIGHT = 0.032
CYL_SEGMENTS = 20
# Three-legged bases are turned a little about the post: with a leg dead behind it, the tilted view hides that leg.
TRIPOD_YAW = 22
palette = json.loads((ROOT / 'Art/Prototypes/field004-trap-cross/model.json').read_text())['materials']

model_id = sys.argv[sys.argv.index('--') + 1]

bpy.ops.wm.read_factory_settings(use_empty=True)
scene = bpy.context.scene
scene.render.engine = 'BLENDER_WORKBENCH'
scene.render.resolution_x = scene.render.resolution_y = 640
scene.display.shading.light = 'FLAT'
scene.display.shading.color_type = 'MATERIAL'
scene.render.film_transparent = False
scene.world = bpy.data.worlds.new('W')
scene.world.color = (0.30, 0.25, 0.20)

mats = {}
def material(key, rgb):
    mat = bpy.data.materials.new('CrossBlender_' + key)
    mat.diffuse_color = (*rgb, 1)
    mats[key] = mat
for key, rgb in palette.items():
    material(key, rgb)
material('hub-steel', (0.48, 0.51, 0.55))
# Spear and bolt tips share the two tones of the projectile sprites (sampled from the spear sprite): the loaded spear and the
# fired spear are the same object. Light on the upper side, dark on the lower, fixed per face - no shadow that changes.
TIP_LIGHT = (0.60, 0.60, 0.62)
TIP_DARK = (0.66, 0.66, 0.69)  # the side that merged with the iron: now a touch lighter than TIP_LIGHT
material('tip-light', TIP_LIGHT)
material('tip-dark', TIP_DARK)
TIP_LIGHT_DIRECTION = (-0.3, 0.9)
mats['outline'].use_backface_culling = True

def empty(name, parent=None, location=(0, 0, 0)):
    ob = bpy.data.objects.new(name, None)
    scene.collection.objects.link(ob)
    ob.parent = parent
    ob.location = location
    return ob

root = empty(model_id)
base = empty('StationaryBase', root)
head = empty('RotatingHead', root)
counter = {'n': 0}

def spear_mesh():
    vertices = [(0.5, 0, 0), (-0.5, 0, 0), (-0.12, 0.5, 0), (-0.12, -0.5, 0), (-0.12, 0, 0.5), (-0.12, 0, -0.5)]
    faces = [(0, 2, 4), (0, 4, 3), (1, 4, 2), (1, 3, 4), (0, 5, 2), (0, 3, 5), (1, 2, 5), (1, 5, 3)]
    mesh = bpy.data.meshes.new('Spear')
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    return mesh

def part(name, shape, pos, size, mat, group, rot=(0, 0, 0), bevel=True, contour=True, top=None, tip=False):
    """shape: box (x,y,z full size), cyl (diameter x, diameter y, height z; axis Z), spear (length x, width y, width z),
    torus (major diameter, minor diameter, unused). rot: Euler degrees applied in X, Y, Z order."""
    counter['n'] += 1
    parent = head if group == 'head' else base
    if shape == 'box':
        bpy.ops.mesh.primitive_cube_add(size=1)
        ob = bpy.context.object
    elif shape == 'cyl':
        bpy.ops.mesh.primitive_cylinder_add(vertices=CYL_SEGMENTS, radius=0.5, depth=1)
        ob = bpy.context.object
    elif shape == 'torus':
        bpy.ops.mesh.primitive_torus_add(major_radius=size[0] / 2, minor_radius=size[1] / 2, major_segments=28, minor_segments=12)
        ob = bpy.context.object
        size = (1, 1, 1)
    else:
        ob = bpy.data.objects.new(name, spear_mesh())
        scene.collection.objects.link(ob)
    ob.name = name
    ob.parent = parent
    ob.location = pos
    ob.rotation_euler = tuple(math.radians(a) for a in rot)
    ob.scale = size
    bpy.ops.object.select_all(action='DESELECT')
    ob.select_set(True)
    bpy.context.view_layer.objects.active = ob
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if tip:
        # Two-tone tip: faces on the light side of the head frame take the light material, the others the dark one.
        ob.data.materials.append(mats['tip-light'])
        ob.data.materials.append(mats['tip-dark'])
        for polygon in ob.data.polygons:
            cx, cy = rot2(polygon.center.x, polygon.center.y, rot[2])
            polygon.material_index = 0 if TIP_LIGHT_DIRECTION[0] * cx + TIP_LIGHT_DIRECTION[1] * cy > 0 else 1
    else:
        ob.data.materials.append(mats[mat])
    if bevel and shape != 'spear':
        modifier = ob.modifiers.new('Bevel', 'BEVEL')
        modifier.width = min(min(s for s in size if s > 0.0) * 0.10, 0.014) if shape != 'torus' else 0.008
        modifier.segments = 2
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    if top:
        # The flat upward faces take the lighter material; the rest keeps the dark one (axis-Z parts only).
        ob.data.materials.append(mats[top])
        for polygon in ob.data.polygons:
            if polygon.normal.z > 0.9:
                polygon.material_index = 1
    if contour:
        shell = bpy.data.objects.new(name + '_Contour', ob.data.copy())
        scene.collection.objects.link(shell)
        shell.parent = parent
        shell.location = ob.location
        shell.rotation_euler = ob.rotation_euler
        shell.data.materials.clear()
        shell.data.materials.append(mats['outline'])
        for polygon in shell.data.polygons:
            polygon.material_index = 0
        bm = bmesh.new()
        bm.from_mesh(shell.data)
        bm.normal_update()
        for v in bm.verts:
            v.co += v.normal * CONTOUR
        bmesh.ops.reverse_faces(bm, faces=list(bm.faces))
        bm.to_mesh(shell.data)
        bm.free()
    return ob

def polar(radius, degrees):
    a = math.radians(degrees)
    return (radius * math.cos(a), radius * math.sin(a))

def rot2(x, y, degrees):
    a = math.radians(degrees)
    return (x * math.cos(a) - y * math.sin(a), x * math.sin(a) + y * math.cos(a))

# ---------------------------------------------------------------- shared details
def rivet(name, pos, group, axis='z', rot=0.0):
    """One bronze rivet; axis z = on a top face, x = on a face looking along +X (rot yaws it)."""
    r = (0, 90, rot) if axis == 'x' else (0, 0, 0)
    part(name, 'cyl', pos, (RIVET_RADIUS * 2, RIVET_RADIUS * 2, RIVET_HEIGHT), 'bronze', group, r, bevel=False, contour=False)

def rivet_ring(prefix, center, radius, count, group, start=0.0):
    for i in range(count):
        x, y = polar(radius, start + i * 360.0 / count)
        rivet(f'{prefix}-{i}', (center[0] + x, center[1] + y, center[2]), group)

def block(name, pos, size, group, rot=0.0, mat='iron', rivets=1, lit=False):
    """Iron block with a bronze rivet on its top; lit=True makes its flat top face the lighter steel material."""
    part(name, 'box', pos, size, mat, group, (0, 0, rot), top='hub-steel' if lit else None)
    long_x = size[0] >= size[1]
    span = (size[0] if long_x else size[1]) * 0.3
    for i in range(rivets):
        t = (i - (rivets - 1) / 2) * span * (2 / max(rivets - 1, 1)) if rivets > 1 else 0.0
        dx, dy = rot2(t if long_x else 0.0, 0.0 if long_x else t, rot)
        rivet(f'{name}-Rivet-{i}', (pos[0] + dx, pos[1] + dy, pos[2] + size[2] / 2 + RIVET_HEIGHT * 0.3), group)

def planks(prefix, center, size, group, count, along='x', z=0.0):
    """A wooden surface as planks: alternating tones with thin seams between them."""
    for i in range(count):
        t = (i - (count - 1) / 2) * size[1] / count if along == 'x' else (i - (count - 1) / 2) * size[0] / count
        part(f'{prefix}-{i}', 'box',
             (center[0] + (0 if along == 'x' else t), center[1] + (t if along == 'x' else 0), z),
             (size[0] if along == 'x' else size[0] / count * 0.94, size[1] / count * 0.94 if along == 'x' else size[1], size[2]),
             'wood' if i % 2 == 0 else 'wood-light', group)

# ---------------------------------------------------------------- bases
def tripod(post_h=0.7, hollow=False, iron_feet=False):
    """Three wooden legs at 120 degrees with iron end blocks, an iron plate, a post with collars. Returns the head pivot height."""
    for i, ang in enumerate((90 + TRIPOD_YAW, 210 + TRIPOD_YAW, 330 + TRIPOD_YAW)):
        x, y = polar(0.4, ang)
        part(f'Leg-{i}', 'box', (x, y, 0.07), (0.8, 0.26, 0.14), 'iron' if iron_feet else 'wood', 'base', (0, 0, ang))
        x2, y2 = polar(0.66, ang)
        block(f'Leg-Cap-{i}', (x2, y2, 0.15), (0.26, 0.36, 0.16), 'base', ang, rivets=1, lit=True)
        x3, y3 = polar(0.3, ang)
        block(f'Leg-Strap-{i}', (x3, y3, 0.15), (0.1, 0.32, 0.12), 'base', ang, rivets=0)
    part('Plate', 'cyl', (0, 0, 0.17), (0.64, 0.64, 0.12), 'iron', 'base', top='hub-steel')
    rivet_ring('Plate-Rivet', (0, 0, 0.242), 0.25, 5, 'base', 18)
    top = 0.2 + post_h
    part('Post', 'cyl', (0, 0, 0.23 + post_h / 2), (0.4, 0.4, post_h), 'wood', 'base')
    part('Collar-Low', 'cyl', (0, 0, 0.3), (0.5, 0.5, 0.1), 'iron', 'base')
    part('Collar-High', 'cyl', (0, 0, top - 0.04), (0.5, 0.5, 0.1), 'iron', 'base')
    rivet_ring('Collar-Rivet', (0, 0, top + 0.01), 0.2, 4, 'base', 45)
    if hollow:
        part('Hole', 'cyl', (0, 0, top + 0.012), (0.26, 0.26, 0.02), 'outline', 'base', contour=False, bevel=False)
    return top + 0.03

def drum(radius=0.6, height=0.5):
    part('Drum', 'cyl', (0, 0, height / 2), (radius * 2, radius * 2, height), 'wood', 'base')
    for k in range(10):
        x, y = polar(radius + 0.003, k * 36)
        part(f'Drum-Seam-{k}', 'box', (x, y, height / 2), (0.02, 0.03, height * 0.9), 'outline', 'base', (0, 0, k * 36), contour=False, bevel=False)
    for i, z in enumerate((0.1, height - 0.1)):
        part(f'Band-{i}', 'cyl', (0, 0, z), (radius * 2 + 0.06, radius * 2 + 0.06, 0.1), 'iron', 'base')
        rivet_ring(f'Band-Rivet-{i}', (0, 0, z + 0.04), radius * 0.92, 8, 'base', 22 * (i + 1))
    part('Top-Plate', 'cyl', (0, 0, height + 0.03), (radius * 1.4, radius * 1.4, 0.06), 'wood-light', 'base')
    part('Hub', 'cyl', (0, 0, height + 0.1), (0.3, 0.3, 0.1), 'iron', 'base', top='hub-steel')
    return height + 0.15

def disc(radius=0.58, height=0.26, post=True):
    part('Disc', 'cyl', (0, 0, height / 2), (radius * 2, radius * 2, height), 'wood', 'base')
    planks('Disc-Plank', (0, 0), (radius * 1.5, radius * 1.5, 0.04), 'base', 6, 'x', height + 0.0)
    part('Rim', 'cyl', (0, 0, height * 0.55), (radius * 2 + 0.06, radius * 2 + 0.06, 0.1), 'iron', 'base')
    rivet_ring('Rim-Rivet', (0, 0, height * 0.55 + 0.045), radius * 0.97, 10, 'base', 18)
    top = height + 0.04
    if post:
        part('Post', 'cyl', (0, 0, top + 0.16), (0.42, 0.42, 0.32), 'iron', 'base')
        rivet_ring('Post-Rivet', (0, 0, top + 0.34), 0.16, 4, 'base', 45)
        part('Hole', 'cyl', (0, 0, top + 0.332), (0.26, 0.26, 0.02), 'outline', 'base', contour=False, bevel=False)
        top += 0.36
    return top

def dish_with_feet():
    part('Dish', 'cyl', (0, 0, 0.2), (1.08, 1.08, 0.18), 'wood', 'base')
    part('Rim', 'cyl', (0, 0, 0.22), (1.14, 1.14, 0.08), 'iron', 'base')
    rivet_ring('Rim-Rivet', (0, 0, 0.27), 0.52, 10, 'base', 18)
    for i, ang in enumerate((90 + TRIPOD_YAW, 210 + TRIPOD_YAW, 330 + TRIPOD_YAW)):
        x, y = polar(0.52, ang)
        block(f'Foot-{i}', (x, y, 0.07), (0.32, 0.28, 0.14), 'base', ang, mat='wood', rivets=0)
    part('Hub', 'cyl', (0, 0, 0.34), (0.44, 0.44, 0.14), 'iron', 'base')
    rivet_ring('Hub-Rivet', (0, 0, 0.42), 0.17, 4, 'base', 45)
    return 0.4

def mound():
    part('Stone-Low', 'cyl', (0, 0, 0.16), (1.2, 1.2, 0.32), 'iron', 'base')
    for i in range(6):
        x, y = polar(0.6, i * 60 + 30)
        block(f'Stone-Block-{i}', (x, y, 0.2), (0.32, 0.3, 0.3), 'base', i * 60 + 30, rivets=1, lit=True)
    part('Stone-High', 'cyl', (0, 0, 0.45), (0.9, 0.9, 0.24), 'iron', 'base')
    rivet_ring('Stone-Rivet', (0, 0, 0.58), 0.34, 8, 'base', 10)
    part('Stone-Cap', 'cyl', (0, 0, 0.6), (0.66, 0.66, 0.1), 'iron', 'base')
    for i in range(8):
        x, y = polar(0.66, i * 45 + 28)
        part(f'Spike-{i}', 'spear', (x, y, 0.4), (0.46, 0.22, 0.22), 'steel', 'base', (0, -50, i * 45 + 28), contour=True)
    return 0.66

# ---------------------------------------------------------------- heads (relative to the pivot, +X forward)
def spear_on(name, x, y, z=0.0, length=0.55, width=0.26, yaw=0.0, mat='steel'):
    part(name, 'spear', (x, y, z), (length, width, width), mat, 'head', (0, 0, yaw), tip=True)

def plank_band(name, length_y, thickness_x, height, count=2):
    part(name, 'box', (0, 0, 0), (thickness_x, length_y, height), 'wood', 'head')
    for i, y in enumerate((-length_y * 0.36, length_y * 0.36)):
        block(f'{name}-Band-{i}', (0, y, 0), (thickness_x + 0.05, 0.16, height + 0.05), 'head', 90, rivets=1)

def plank_spears():
    plank_band('Plank', 1.5, 0.3, 0.26)
    for i, y in enumerate((-0.56, -0.19, 0.19, 0.56)):
        block(f'Socket-{i}', (0.24, y, 0), (0.26, 0.2, 0.22), 'head', 0, rivets=1, lit=True)
        spear_on(f'Spear-{i}', 0.6, y, length=0.6, width=0.28)

def cross_head():
    for i in range(4):
        ang = i * 90
        x, y = polar(0.44, ang)
        part(f'Arm-{i}-Shaft', 'box', (x, y, 0), (0.8, 0.16, 0.16), 'wood', 'head', (0, 0, ang))
        x, y = polar(0.74, ang)
        block(f'Arm-{i}-Socket', (x, y, 0), (0.24, 0.26, 0.26), 'head', ang, rivets=1, lit=True)
        x, y = polar(1.0, ang)
        part(f'Arm-{i}-Tip', 'spear', (x, y, 0), (0.56, 0.28, 0.28), 'steel', 'head', (0, 0, ang), tip=True)
    part('Hub', 'cyl', (0, 0, 0.0), (0.56, 0.56, 0.26), 'iron', 'head', top='hub-steel')
    rivet_ring('Hub-Rivet', (0, 0, 0.14), 0.2, 4, 'head', 45)
    part('Hub-Cap', 'cyl', (0, 0, 0.16), (0.26, 0.26, 0.06), 'bronze', 'head')

def blade_wheel():
    part('Wheel', 'cyl', (0, 0, 0), (0.8, 0.8, 0.14), 'wood', 'head')
    part('Wheel-Rim', 'cyl', (0, 0, 0), (0.9, 0.9, 0.1), 'iron', 'head')
    rivet_ring('Wheel-Rivet', (0, 0, 0.07), 0.34, 8, 'head', 22)
    part('Wheel-Hub', 'cyl', (0, 0, 0.1), (0.3, 0.3, 0.1), 'iron', 'head', top='hub-steel')
    part('Wheel-Cap', 'cyl', (0, 0, 0.17), (0.16, 0.16, 0.06), 'bronze', 'head')
    for i in range(8):
        ang = i * 45
        x, y = polar(0.5, ang)
        block(f'Holder-{i}', (x, y, 0), (0.22, 0.2, 0.16), 'head', ang, rivets=1, lit=True)
        x, y = polar(0.74, ang + 8)
        part(f'Blade-{i}', 'spear', (x, y, 0), (0.56, 0.3, 0.16), 'steel', 'head', (0, 0, ang + 62))

def crossbow(scale=1.0, winch=False):
    s = scale
    part('Stock', 'box', (0.12 * s, 0, -0.02), (1.1 * s, 0.2 * s, 0.18 * s), 'wood', 'head')
    block('Stock-Plate', (0.0, 0, 0.12 * s), (0.34 * s, 0.26 * s, 0.06 * s), 'head', 0, rivets=1)
    block('Stock-Nose', (0.62 * s, 0, 0.0), (0.2 * s, 0.26 * s, 0.22 * s), 'head', 0, rivets=1, lit=True)
    pts = []
    for k in range(9):
        t = -1 + k * 0.25
        pts.append((0.42 * s - 0.36 * s * t * t, 0.66 * s * t))
    for k in range(8):
        (x0, y0), (x1, y1) = pts[k], pts[k + 1]
        mx, my = (x0 + x1) / 2, (y0 + y1) / 2
        ang = math.degrees(math.atan2(y1 - y0, x1 - x0))
        part(f'Limb-{k}', 'box', (mx, my, 0), (math.hypot(x1 - x0, y1 - y0) + 0.05 * s, 0.16 * s, 0.2 * s), 'wood', 'head', (0, 0, ang))
    part('Limb-Band', 'box', (pts[4][0], 0, 0), (0.2 * s, 0.34 * s, 0.26 * s), 'iron', 'head')
    part('String', 'box', (0.0, 0, 0.0), (0.04 * s, 1.34 * s, 0.04 * s), 'wood-light', 'head', contour=False, bevel=False)
    spear_on('Bolt', 0.55 * s, 0, 0.14 * s, length=0.78 * s, width=0.2 * s)
    for sign in (-1, 1):
        block(f'Tip-Plate-{sign}', (pts[0 if sign < 0 else 8][0], 0.66 * s * sign, 0), (0.18 * s, 0.2 * s, 0.26 * s), 'head', 0, rivets=1)
    if winch:
        part('Winch', 'cyl', (-0.42 * s, 0, 0.0), (0.26 * s, 0.26 * s, 0.34 * s), 'wood', 'head', (90, 0, 0))
        for sign in (-1, 1):
            part(f'Winch-Flange-{sign}', 'cyl', (-0.42 * s, 0.2 * s * sign, 0.0), (0.38 * s, 0.38 * s, 0.05 * s), 'iron', 'head', (90, 0, 0))
        part('Winch-Handle', 'box', (-0.42 * s, 0.3 * s, 0.1 * s), (0.06, 0.06, 0.3), 'wood', 'head')

def trident():
    part('Hub', 'cyl', (-0.3, 0, 0), (0.5, 0.5, 0.3), 'iron', 'head', (0, 90, 0))
    for i, sign in enumerate((-1, 0, 1)):
        ang = sign * 40
        dx, dy = polar(0.66, ang)
        cx, cy = -0.3 + dx, dy
        part(f'Tube-{i}', 'cyl', (cx, cy, 0), (0.22, 0.22, 0.76), 'wood', 'head', (0, 90, ang))
        for j, d in enumerate((0.32, 0.72)):
            bx, by = polar(d, ang)
            part(f'Band-{i}-{j}', 'cyl', (-0.3 + bx, by, 0), (0.29, 0.29, 0.09), 'iron', 'head', (0, 90, ang))
        qx, qy = polar(1.08, ang)
        spear_on(f'Tip-{i}', -0.3 + qx, qy, 0, length=0.44, width=0.24, yaw=ang)
        mx, my = polar(0.5, ang)
        rivet(f'Rivet-{i}', (-0.3 + mx, my, 0.14), 'head')

def gear_cannon():
    part('Housing', 'cyl', (0, 0, 0.12), (0.7, 0.7, 0.46), 'wood', 'head')
    for i, z in enumerate((-0.04, 0.26)):
        part(f'Housing-Band-{i}', 'cyl', (0, 0, z), (0.76, 0.76, 0.09), 'iron', 'head')
        rivet_ring(f'Housing-Rivet-{i}', (0, 0, z + 0.04), 0.34, 6, 'head', 10 * (i + 1))
    part('Gear', 'cyl', (0, 0, 0.4), (0.7, 0.7, 0.08), 'iron', 'head', top='hub-steel')
    for i in range(10):
        x, y = polar(0.36, i * 36)
        part(f'Tooth-{i}', 'box', (x, y, 0.4), (0.12, 0.12, 0.08), 'iron', 'head', (0, 0, i * 36))
    part('Gear-Cap', 'cyl', (0, 0, 0.48), (0.24, 0.24, 0.05), 'bronze', 'head')
    part('Barrel', 'box', (0.55, 0, 0.1), (0.86, 0.34, 0.32), 'iron', 'head')
    block('Barrel-Ring-A', (0.28, 0, 0.1), (0.1, 0.4, 0.38), 'head', 0, rivets=1)
    block('Barrel-Ring-B', (0.98, 0, 0.1), (0.12, 0.42, 0.4), 'head', 0, rivets=1)
    part('Barrel-Hole', 'box', (1.047, 0, 0.1), (0.02, 0.18, 0.18), 'outline', 'head', contour=False, bevel=False)

def ring_ports():
    part('Ring', 'torus', (0, 0, 0.06), (0.84, 0.32, 0), 'wood', 'head')
    part('Core', 'cyl', (0, 0, 0.0), (0.46, 0.46, 0.2), 'iron', 'head')
    part('Core-Hole', 'cyl', (0, 0, 0.105), (0.26, 0.26, 0.02), 'outline', 'head', contour=False, bevel=False)
    for i in range(8):
        ang = i * 45
        x, y = polar(0.44, ang)
        block(f'Port-Block-{i}', (x, y, 0.1), (0.22, 0.24, 0.18), 'head', ang, rivets=1, lit=True)
        x, y = polar(0.62, ang)
        part(f'Port-{i}', 'cyl', (x, y, 0.1), (0.17, 0.17, 0.2), 'iron', 'head', (0, 90, ang))
        x, y = polar(0.72, ang)
        part(f'Port-Hole-{i}', 'cyl', (x, y, 0.1), (0.09, 0.09, 0.02), 'outline', 'head', (0, 90, ang), contour=False, bevel=False)

def double_spear_bar():
    part('Bar', 'cyl', (0, 0, 0), (0.24, 0.24, 1.6), 'wood', 'head', (0, 90, 0))
    for i, x in enumerate((-0.52, 0.52)):
        part(f'Collar-{i}', 'cyl', (x, 0, 0), (0.34, 0.34, 0.16), 'iron', 'head', (0, 90, 0))
        rivet(f'Collar-Rivet-{i}', (x, 0, 0.17), 'head')
    part('Hub', 'cyl', (0, 0, 0), (0.5, 0.5, 0.26), 'iron', 'head', top='hub-steel')
    rivet_ring('Hub-Rivet', (0, 0, 0.14), 0.19, 4, 'head', 45)
    part('Hub-Cap', 'cyl', (0, 0, 0.16), (0.24, 0.24, 0.06), 'bronze', 'head')
    spear_on('Tip-Front', 1.02, 0, 0, length=0.66, width=0.34)
    spear_on('Tip-Back', -1.02, 0, 0, length=0.66, width=0.34, yaw=180)

def plank_barrels():
    plank_band('Plank', 1.5, 0.3, 0.26)
    for i, y in enumerate((-0.56, -0.19, 0.19, 0.56)):
        part(f'Barrel-{i}', 'cyl', (0.38, y, 0), (0.22, 0.22, 0.5), 'iron', 'head', (0, 90, 0))
        part(f'Barrel-Ring-{i}', 'cyl', (0.62, y, 0), (0.28, 0.28, 0.09), 'hub-steel', 'head', (0, 90, 0))
        rivet(f'Barrel-Rivet-{i}', (0.5, y, 0.12), 'head')
        part(f'Barrel-Hole-{i}', 'cyl', (0.668, y, 0), (0.12, 0.12, 0.02), 'outline', 'head', (0, 90, 0), contour=False, bevel=False)

def cannon_flared():
    part('Trunnion', 'cyl', (0, 0, 0), (0.5, 0.5, 0.8), 'wood', 'head', (90, 0, 0))
    for sign in (-1, 1):
        part(f'Trunnion-Cap-{sign}', 'cyl', (0, 0.44 * sign, 0), (0.34, 0.34, 0.1), 'iron', 'head', (90, 0, 0))
        rivet(f'Trunnion-Rivet-{sign}', (0, 0.44 * sign, 0.2), 'head')
    part('Barrel', 'cyl', (0.5, 0, 0), (0.62, 0.62, 0.9), 'iron', 'head', (0, 90, 0))
    for i, x in enumerate((0.2, 0.6)):
        part(f'Barrel-Band-{i}', 'cyl', (x, 0, 0), (0.7, 0.7, 0.1), 'iron', 'head', (0, 90, 0))
        rivet(f'Barrel-Band-Rivet-{i}', (x, 0, 0.36), 'head')
    part('Flare', 'cyl', (0.98, 0, 0), (0.94, 0.94, 0.22), 'iron', 'head', (0, 90, 0))
    part('Flare-Hole', 'cyl', (1.095, 0, 0), (0.62, 0.62, 0.02), 'outline', 'head', (0, 90, 0), contour=False, bevel=False)

def fan5():
    part('Hub', 'cyl', (-0.24, 0, 0), (0.6, 0.6, 0.3), 'iron', 'head', top='hub-steel')
    rivet_ring('Hub-Rivet', (-0.24, 0, 0.16), 0.22, 5, 'head', 18)
    part('Hub-Cap', 'cyl', (-0.24, 0, 0.18), (0.22, 0.22, 0.06), 'bronze', 'head')
    for i, ang in enumerate((-52, -26, 0, 26, 52)):
        cx, cy = polar(0.6, ang)
        part(f'Tube-{i}', 'cyl', (-0.24 + cx, cy, 0), (0.2, 0.2, 0.74), 'wood', 'head', (0, 90, ang))
        for j, d in enumerate((0.38, 0.8)):
            bx, by = polar(d, ang)
            part(f'Band-{i}-{j}', 'cyl', (-0.24 + bx, by, 0), (0.25, 0.25, 0.08), 'iron', 'head', (0, 90, ang))
        mx, my = polar(0.98, ang)
        part(f'Hole-{i}', 'cyl', (-0.24 + mx, my, 0), (0.12, 0.12, 0.02), 'outline', 'head', (0, 90, ang), contour=False, bevel=False)

# ---------------------------------------------------------------- the thirteen
def build():
    pivot = {
        'TRAP-001': lambda: (tripod(0.7, hollow=True), plank_spears),
        'TRAP-003': lambda: (tripod(0.7), cross_head),
        'TRAP-004': lambda: (drum(), blade_wheel),
        'TRAP-005': lambda: (tripod(0.72), lambda: crossbow(1.0)),
        'TRAP-006': lambda: (tripod(0.6, hollow=True, iron_feet=True), trident),
        'TRAP-007': lambda: (disc(), gear_cannon),
        'TRAP-008': lambda: (dish_with_feet(), ring_ports),
        'TRAP-009': lambda: (tripod(0.85, hollow=True), lambda: crossbow(1.3, winch=True)),
        'TRAP-010': lambda: (disc(post=False), double_spear_bar),
        'TRAP-011': lambda: (tripod(0.7, hollow=True), plank_barrels),
        'TRAP-012': lambda: (mound(), cannon_flared),
        'TRAP-013': lambda: (tripod(0.7, hollow=True), fan5),
    }[model_id]
    top, make_head = pivot()
    head.location = (0, 0, top + 0.1 if model_id in ('TRAP-004', 'TRAP-007', 'TRAP-010') else top)
    make_head()

build()

# Preview: the same tilted orthographic view the game uses, to compare with the 2D candidate.
cam_data = bpy.data.cameras.new('Preview')
cam_data.type = 'ORTHO'
cam_data.ortho_scale = 3.4
cam = bpy.data.objects.new('Preview', cam_data)
scene.collection.objects.link(cam)
tilt = math.radians(54)
target = Vector((0, 0, 0.6))
cam.location = target + Vector((0, -8 * math.cos(tilt), 8 * math.sin(tilt)))
cam.rotation_euler = (target - cam.location).to_track_quat('-Z', 'Y').to_euler()
scene.camera = cam
(OUT / 'preview').mkdir(parents=True, exist_ok=True)
scene.render.filepath = str(OUT / 'preview' / (model_id + '.png'))
for ob in bpy.data.objects:
    if ob.name.endswith('_Contour'):
        ob.hide_render = True  # preview shows the painted parts only; the shells are still exported
bpy.ops.render.render(write_still=True)
for ob in bpy.data.objects:
    ob.hide_render = False

FBX_DIR.mkdir(parents=True, exist_ok=True)
for ob in bpy.data.objects:
    ob.select_set(ob.type in {'EMPTY', 'MESH'})
bpy.context.view_layer.objects.active = root
bpy.ops.export_scene.fbx(
    filepath=str(FBX_DIR / (model_id.lower() + '.fbx')), use_selection=True, object_types={'EMPTY', 'MESH'},
    apply_scale_options='FBX_SCALE_ALL', axis_forward='Y', axis_up='Z', bake_space_transform=False,
    use_mesh_modifiers=True, mesh_smooth_type='FACE', add_leaf_bones=False, bake_anim=False, use_custom_props=False)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT / (model_id.lower() + '.blend')), copy=True)
print('TRAP MODEL BUILT', model_id, len([o for o in bpy.data.objects if o.type == 'MESH']), 'meshes')
