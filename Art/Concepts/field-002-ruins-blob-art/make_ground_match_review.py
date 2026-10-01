"""Technical composition of unchanged sprites over the actual ruins floor.

No raster editing: review composites only. Gameplay collider data is never read
or written here; existing import scale and pivot are preserved in the review.
"""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[2]
SCALE = 13
CELL = 440


def main():
    packet = json.loads((PROJECT / 'Art/Packets/field-dev-blobs-soft-edges-2026-10-01.json').read_text(encoding='utf-8'))
    plan = json.loads((ROOT / 'ground-match-generation-plan.json').read_text(encoding='utf-8'))
    tasks = {t['id']: t for t in plan['tasks']}
    ground = Image.open(plan['groundReference']).convert('RGBA').resize((8 * SCALE, 8 * SCALE), Image.Resampling.LANCZOS)
    font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 18)
    records = []
    for mode in ('before', 'after'):
        page = Image.new('RGBA', (CELL * 4, CELL * 5))
        for y in range(0, page.height, ground.height):
            for x in range(0, page.width, ground.width):
                page.alpha_composite(ground, (x, y))
        draw = ImageDraw.Draw(page)
        for i, asset in enumerate(packet['assets']):
            item_id = asset['sourceDirectory'].rsplit('/', 1)[1]
            source = PROJECT / asset['sourceDirectory'] / asset['version'] / 'concept-01.png'
            candidate = PROJECT / tasks[item_id]['output'] if item_id in tasks else source
            image = Image.open(candidate if mode == 'after' else source).convert('RGBA')
            profile = asset['importProfile']
            ppu = profile['pixelsPerUnit']
            target = (round(image.width / ppu * SCALE), round(image.height / ppu * SCALE))
            sprite = image.resize(target, Image.Resampling.LANCZOS)
            cx, cy = (i % 4) * CELL + CELL // 2, (i // 4) * CELL + 196
            x = round(cx - profile['pivotX'] * target[0])
            y = round(cy - (1 - profile['pivotY']) * target[1])
            page.alpha_composite(sprite, (x, y))
            label_y = (i // 4) * CELL + 404
            draw.rectangle(((i % 4) * CELL + 8, label_y - 3, (i + 1) % 4 * CELL - 8 if (i + 1) % 4 else CELL * 4 - 8, label_y + 28), fill='#ece7df')
            label = item_id + (' / unchanged' if item_id not in tasks else '')
            draw.text(((i % 4) * CELL + 18, label_y), label, font=font, fill='#241b2b')
            if mode == 'after':
                original = Image.open(source)
                alpha = image.getchannel('A')
                records.append({'id': item_id, 'image': candidate.relative_to(PROJECT).as_posix(), 'canvas': list(image.size), 'originalCanvas': list(original.size), 'canvasPreserved': image.size == original.size, 'alphaRange': list(alpha.getextrema()), 'ppu': ppu, 'collider': 'unchanged'})
        page.convert('RGB').save(ROOT / f'ground-match-{mode}-actual-floor-review.png')
    (ROOT / 'ground-match-candidates.json').write_text(json.dumps(records, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
