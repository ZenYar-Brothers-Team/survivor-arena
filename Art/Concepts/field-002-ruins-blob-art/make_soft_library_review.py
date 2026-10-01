"""Compose unmodified new candidates at the current imported world scale."""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
PROJECT = ROOT.parents[2]

def main():
    lib = json.loads((PROJECT/'Art/Source/Environment/field-002-ruins-obstacles/library.json').read_text(encoding='utf-8'))
    packet = json.loads((PROJECT/'Art/Packets/field-dev-blobs-ruins-2026-10-01.json').read_text(encoding='utf-8'))
    page = Image.new('RGBA', (1760, 2200), '#e7dfd2')
    draw = ImageDraw.Draw(page)
    font = ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
    records = []
    for i, (item, asset) in enumerate(zip(lib['items'], packet['assets'])):
        f = ROOT/(item['id']+'-soft-edge-v2-preview.png')
        if not f.exists(): f = PROJECT/'Art/Source/Environment/field-002-ruins-obstacles'/item['image']
        variant = ROOT/(item['id']+'-detail-scale-v3-preview.png')
        if variant.exists(): f = variant
        variant = ROOT/(item['id']+'-detail-scale-v4-preview.png')
        if variant.exists(): f = variant
        original = Image.open(PROJECT/asset['runtime'])
        image = Image.open(f).convert('RGBA')
        # Match full canvas width to preserve the original detail/world scale.
        factor = image.width / original.width
        ppu = asset['importProfile']['pixelsPerUnit'] * factor
        target = (round(image.width/ppu*13), round(image.height/ppu*13))
        sprite = image.resize(target, Image.Resampling.LANCZOS)
        x, y = (i%4)*440+(440-target[0])//2, (i//4)*440+(400-target[1])//2
        page.alpha_composite(sprite,(x,y))
        draw.text(((i%4)*440+18,(i//4)*440+408),item['id']+' / 13 px per world unit',font=font,fill='#241b2b')
        records.append({'id':item['id'],'image':f.relative_to(PROJECT).as_posix(),'canvas':image.size,'referenceCanvas':original.size,'proposedPpu':ppu})
    page.convert('RGB').save(ROOT/'soft-library-world-scale-review.png')
    (ROOT/'soft-library-candidates.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')

if __name__ == '__main__': main()
