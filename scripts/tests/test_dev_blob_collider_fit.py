"""Regression coverage for shifted pivots, concave cores and disconnected debris."""
import json
import sys
import unittest
from pathlib import Path

from PIL import Image, ImageDraw

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'art'))
from fit_dev_blob_colliders import fit, ROOT


def contains(points, x, y):
    inside = False
    for a, b in zip(points, points[1:]+points[:1]):
        if (a[1] > y) != (b[1] > y):
            if x < (b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0]:
                inside = not inside
    return inside


class DevBlobColliderFitTests(unittest.TestCase):
    def test_fit_preserves_concavity_and_excludes_disconnected_debris(self):
        image = Image.new('RGBA', (600, 400))
        draw = ImageDraw.Draw(image)
        draw.rectangle((50,50,350,250), fill=(100,100,100,255))
        draw.rectangle((180,50,220,150), fill=(0,0,0,0))
        draw.rectangle((420,100,460,140), fill=(100,100,100,255))
        points = fit(image, {'pixelsPerUnit':100,'pivotX':.5,'pivotY':.5})
        self.assertFalse(contains(points,-1,1), 'Concave bay must remain passable')
        self.assertTrue(contains(points,-1,0), 'Dense core must remain blocked')
        self.assertLess(max(p[0] for p in points), .4, 'Remote debris must not create an invisible wall')
        self.assertAlmostEqual(min(p[0] for p in points), -2.38, delta=.03)

    def test_configured_contours_match_frozen_collision_reference_and_import_pivots(self):
        packet = json.loads((ROOT/'Art/Packets/field-dev-blobs-soft-edges-2026-10-01.json').read_text(encoding='utf-8'))
        data = json.loads((ROOT/'docs/balance/field-dev-blobs-v1.json').read_text(encoding='utf-8'))
        for asset, item in zip(packet['assets'],data['blobLayout']['library']):
            with self.subTest(item=item['id']):
                self.assertEqual(asset['visualId'],item['visualId'])
                # Palette/ground-skirt replacements must not redefine collision.
                # The immutable selected version used for the approved fit remains
                # the geometry reference even after the runtime PNG is replaced.
                reference = ROOT/asset['sourceDirectory']/asset['version']/'concept-01.png'
                self.assertEqual(fit(reference,asset['importProfile']),item['points'])


if __name__ == '__main__': unittest.main()
