"""Rebuild the review gallery; preserve the recorded approved packet when present."""
import hashlib
import html
import json
import math
import shutil
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
jobs = json.loads((HERE / 'generation-jobs.json').read_text(encoding='utf-8'))
assets = []
cards = []
names = {'HASTE': 'Ветер', 'HEAL': 'Исцеление', 'WARD': 'Защита', 'POWER': 'Сила',
         'STRIKE-HOLY': 'Священный гнев', 'STRIKE-CURSED': 'Проклятые удары',
         'LIFE': 'Жизнь', 'EXPERIENCE': 'Опыт'}
for job in jobs:
    target = HERE / job['file']
    if 'source' in job:
        shutil.copyfile(job['source'], target)
    owner = job['id']
    slug = owner.lower()
    assets.append(dict(owner=owner, role='prop', visualId=owner+'-VISUAL-PROP', owningIp='IP-23',
        input=target.relative_to(ROOT).as_posix(), sha256=hashlib.sha256(target.read_bytes()).hexdigest(),
        sourceDirectory=f'Art/Source/Environment/{slug}/prop', version='v001',
        runtime=f'Assets/Resources/Art/Sprites/Environment/{slug}/{slug}-prop.png',
        prompt=job['prompt'], generator='OpenAI built-in image generation', artDirectionRevision='2026-10-02',
        constraints=['preserve polarity foundation', 'type-specific top or relic', 'remove baked aura before runtime review'],
        preparation=dict(mode='fit', size=512, padding=24, cropAlpha=True, alphaNoiseCutoff=32),
        importProfile=dict(pixelsPerUnit=256, maxSize=512, pivotX=.5, pivotY=.046875,
            reason='Stationary altar/shrine Prop; gameplay contact remains independently authored.'),
        sprite={}, bindings=[]))
    kind = owner.replace('FIELD-007-ALTAR-', '').replace('FIELD-007-SHRINE-', '')
    label = names.get(kind, names.get(kind.replace('-CURSED', ''), kind))
    if '-SHRINE-' in owner and kind == 'POWER':
        label = 'Мощь'
    polarity = 'Святыня' if '-SHRINE-' in owner else 'Негативный' if '-CURSED' in owner else 'Позитивный'
    cards.append(f'<article><h2>{html.escape(label)}</h2><p>{polarity}</p><img src="{job["file"]}"><div class="small"><img src="{job["file"]}"></div><code>{owner}</code></article>')
packet = dict(schemaVersion=1, approvedBy='pending-user-review', approvedAt='2026-10-02',
    approvalEvidence='NOT APPROVED. User requested distinct types; these exact image bytes have not been selected. Altar aura cleanup still required.', assets=assets)
approved_path = ROOT/'Art/Packets/field007-altar-types-v1.json'
approved = approved_path.is_file()
if not approved:
    (HERE / 'art-packet.pending.json').write_text(json.dumps(packet, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
for job in jobs:
    job.pop('source', None)
    reference = Path(job['reference'])
    job['reference'] = reference.relative_to(ROOT).as_posix() if reference.is_absolute() else reference.as_posix()
(HERE/'generation-jobs.json').write_text(json.dumps(jobs, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

def strand(opposite):
    points = []
    for index in range(193):
        angle = math.pi*.5 - math.pi*2*index/192
        wave = math.cos((angle-math.pi*.5)*8) * (-1 if opposite else 1)
        radius = 140*(1+.06*wave*.5)
        points.append(f'{160+math.cos(angle)*radius:.3f},{160+math.sin(angle)*radius:.3f}')
    return ' '.join(points)

contours = ''.join(f'<div><svg viewBox="0 0 320 320"><circle cx="160" cy="160" r="140" fill="none" stroke="#fff" stroke-opacity=".4" stroke-dasharray="3 7"/><polyline points="{strand(False)}" fill="none" stroke="{color}" stroke-width="2.6"/><polyline points="{strand(True)}" fill="none" stroke="{color}" stroke-opacity=".65" stroke-width="2.6"/></svg><p>{name}</p></div>' for color, name in [('#62b8b7', 'Позитивный'), ('#ed756b', 'Негативный')])
page = '''<!doctype html><html lang="ru"><meta charset="utf-8"><title>Алтари и святыни FIELD-007</title>
<style>body{background:#282638;color:#f1e6d2;font:16px system-ui;margin:32px}h1{font-size:30px}p{color:#c9c0b3}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:20px}article{background:#333246;border:1px solid #696177;border-radius:14px;padding:18px;text-align:center}h2{font-size:20px;margin:0}img{width:100%;height:310px;object-fit:contain}.small{background:#4a5346;margin:12px 0;padding:12px}.small img{width:180px;height:180px}code{font-size:10px;overflow-wrap:anywhere}.contours{display:flex;gap:24px;text-align:center}.contours svg{width:280px;height:280px}</style>
<h1>Алтари и святыни · Монастырские сады</h1><p>Все 14 образов утверждены пользователем. Сверху — крупно, ниже — уменьшенный просмотр исходника. Runtime подготовлен отдельно через art pipeline.</p>
<h2>Общий нейтральный контур</h2><p>Две плавные линии с восемью петлями. Форма одинаковая; меняется цвет полярности. Белый пунктир — фактический радиус действия, строго посередине нитей. Это схема геометрии, не игровой capture; пунктир показан только здесь для объяснения.</p><div class="contours">'''+contours+'''</div><h2>Типы алтарей и святынь</h2><div class="grid">'''+''.join(cards)+'''</div><p>Изображения сохраняют утверждённые различия внутри пар. Уменьшение здесь не заменяет игровой просмотр.</p></html>'''
(HERE/'index.html').write_text(page, encoding='utf-8')
print(f'Prepared {len(assets)} unchanged previews, pending packet and gallery: {HERE.relative_to(ROOT)}')
