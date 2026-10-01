"""Author DEV obstacle contours from the selected dense raster cores.

This is offline collider authoring and a diagnostic overlay, not raster editing.
Soft dust, disconnected debris and small border irregularities are excluded.
"""
import argparse
import json
from pathlib import Path

import numpy as np
from scipy import ndimage
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]


def simplify(points, epsilon):
    if len(points) <= 2:
        return points
    a, b = np.array(points[0]), np.array(points[-1])
    values = np.array(points)
    ab = b-a
    t = np.clip(((values-a) @ ab) / max(float(ab @ ab), 1e-12), 0, 1)
    distances = np.linalg.norm(values-a-t[:, None]*ab, axis=1)
    i = int(np.argmax(distances))
    if distances[i] <= epsilon:
        return [points[0], points[-1]]
    return simplify(points[:i+1], epsilon)[:-1] + simplify(points[i:], epsilon)


def largest(mask):
    labels, count = ndimage.label(mask)
    if count == 0:
        raise ValueError('No dense core')
    sizes = np.bincount(labels.ravel())
    sizes[0] = 0
    return labels == sizes.argmax()


def contour(mask):
    # Oriented pixel edges. A filled component has one exterior loop after filling holes.
    edges = {}
    h, w = mask.shape
    for y, x in zip(*np.nonzero(mask)):
        if y == 0 or not mask[y-1, x]: edges[(x,y)] = (x+1,y)
        if x == w-1 or not mask[y,x+1]: edges[(x+1,y)] = (x+1,y+1)
        if y == h-1 or not mask[y+1,x]: edges[(x+1,y+1)] = (x,y+1)
        if x == 0 or not mask[y,x-1]: edges[(x,y+1)] = (x,y)
    loops = []
    while edges:
        start = next(iter(edges))
        point, loop = start, []
        while point in edges:
            loop.append(point)
            point = edges.pop(point)
            if point == start:
                loops.append(loop)
                break
    return max(loops, key=len)


def fit(path, profile):
    image = (path if isinstance(path, Image.Image) else Image.open(path)).convert('RGBA')
    factor = min(1., 768 / max(image.size))
    alpha = np.array(image.getchannel('A').resize(tuple(round(v*factor) for v in image.size), Image.Resampling.LANCZOS))
    ppu = profile['pixelsPerUnit'] * factor
    mask = largest(alpha >= 235)
    mask = ndimage.binary_fill_holes(mask)
    # Open with a 0.20 wu disk using distance fields: remove dense dust filaments
    # before tracing, while preserving the large concavities of the footprint.
    radius = max(1., ppu * .20)
    opened = ndimage.distance_transform_edt(mask) > radius
    mask = ndimage.distance_transform_edt(~opened) <= radius
    mask = largest(ndimage.binary_fill_holes(mask))
    # Remove narrow dust/roots; retain concave bays. The inset is 0.12 world units.
    mask = ndimage.distance_transform_edt(mask) > max(1., ppu * .12)
    mask = largest(ndimage.binary_fill_holes(mask))
    points = contour(mask)
    pivot = (profile['pivotX']*image.width*factor, (1-profile['pivotY'])*image.height*factor)
    world = [((x-pivot[0])/ppu, (pivot[1]-y)/ppu) for x,y in points]
    # Split the closed loop at its farthest vertex so RDP has two stable arcs.
    far = int(np.argmax(np.linalg.norm(np.array(world)-world[0], axis=1)))
    fitted = simplify(world[:far+1], .14)[:-1] + simplify(world[far:]+[world[0]], .14)[:-1]
    return [[round(x, 4), round(y, 4)] for x,y in fitted]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    packet = json.loads((ROOT/'Art/Packets/field-dev-blobs-soft-edges-2026-10-01.json').read_text(encoding='utf-8'))
    source = ROOT/'docs/balance/field-dev-blobs-v1.json'
    data = json.loads(source.read_text(encoding='utf-8'))
    library = data['blobLayout']['library']
    report = []
    page = Image.new('RGB', (1600, 2100), '#e7dfd2')
    font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 18)
    for i, (asset, entry) in enumerate(zip(packet['assets'], library)):
        assert asset['visualId'] == entry['visualId']
        path, profile = ROOT/asset['runtime'], asset['importProfile']
        old = entry['points']
        new = fit(path, profile)
        report.append({'id':entry['id'], 'oldPoints':old, 'points':new, 'insetWu':.12, 'simplificationWu':.14, 'openingWu':.20})
        sprite = Image.open(path).convert('RGBA')
        factor = min(360/sprite.width, 360/sprite.height)
        small = sprite.resize((round(sprite.width*factor), round(sprite.height*factor)), Image.Resampling.LANCZOS)
        x,y = 400*(i%4)+(400-small.width)//2, 420*(i//4)+15
        page.paste(small, (x,y), small)
        draw = ImageDraw.Draw(page)
        pivot = (x+sprite.width*factor*profile['pivotX'], y+sprite.height*factor*(1-profile['pivotY']))
        for points, color in [(old, '#e25a40'), (new, '#087a63')]:
            line = [(pivot[0]+px*profile['pixelsPerUnit']*factor, pivot[1]-py*profile['pixelsPerUnit']*factor) for px,py in points]
            draw.line(line+[line[0]], fill=color, width=2)
        draw.text((400*(i%4)+15,420*(i//4)+380), entry['id']+' / red old, green fitted',font=font,fill='#241b2b')
        entry['points'] = new
    output = ROOT/'Art/Concepts/field-002-ruins-blob-art'
    (output/'collider-fit-report.json').write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
    page.save(output/'collider-fit-review.png')
    if args.apply:
        data['revision'] = 'field-dev-blobs-v1.2'
        data['algorithm'][-1] = 'Коллайдеры подогнаны по плотному ядру текущих runtime PNG вокруг их импортированного pivot/PPU; мягкая пыль и отдельные обломки исключены. Offline authoring: scripts/art/fit_dev_blob_colliders.py, opening 0.20 wu, inset 0.12 wu, simplification 0.14 wu; runtime alpha не читается.'
        source.write_text(json.dumps(data, ensure_ascii=False, indent=1)+'\n',encoding='utf-8')
    print('Fitted 20 dense-core contours; points per item:', [len(x['points']) for x in report])


if __name__ == '__main__':
    main()
