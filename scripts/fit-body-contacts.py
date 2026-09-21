"""Review circles against a filled convex outer silhouette; never edits PNGs.

Default: validate current profiles. --radius-scale scales current radii once.
--fit-outer maximizes radius and vertical center on the flip axis, so the circle
fits both facing directions. --write saves the proposal. Requires Pillow, NumPy and SciPy.
"""
import argparse
import json
from pathlib import Path
import numpy as np
from PIL import Image
from scipy.spatial import ConvexHull
from scipy.optimize import linprog

parser = argparse.ArgumentParser()
parser.add_argument("--write", action="store_true")
mode = parser.add_mutually_exclusive_group()
mode.add_argument("--radius-scale", type=float, default=1.0)
mode.add_argument("--fit-outer", action="store_true")
args = parser.parse_args()
if not np.isfinite(args.radius_scale) or args.radius_scale <= 0:
    parser.error("radius-scale must be finite and positive")
root = Path(__file__).resolve().parent.parent
path = root / "Assets/Resources/Content/Presentation/FixtureSprites.json"
entries = json.loads(path.read_text(encoding="utf-8-sig"))
profiles = {p["key"]: p for p in json.loads((root / "Art/ImportProfiles.json").read_text(encoding="utf-8-sig"))}
for entry in entries:
    if "contactRadius" not in entry:
        continue
    asset = "Assets/Resources/" + entry["resourcePath"] + ".png"
    profile = profiles[asset]
    alpha = np.array(Image.open(root / asset).convert("RGBA"))[:, :, 3]
    ys, xs = np.where(alpha >= 230)
    hull = ConvexHull(np.column_stack((xs, ys)))
    ppu = profile["pixelsPerUnit"]
    center = np.array([profile["pivotX"] * alpha.shape[1],
                       alpha.shape[0] - .5 - profile["pivotY"] * alpha.shape[0] - entry["contactCenterY"] * ppu])
    if args.fit_outer:
        # Hull normals have unit length: n.center + radius <= -offset.
        # Keeping X at the pivot fits both the original and mirrored envelope.
        solution = linprog([0, -1],
            A_ub=np.column_stack((hull.equations[:, 1], np.ones(len(hull.equations)))),
            b_ub=-hull.equations[:, 2] - hull.equations[:, 0] * center[0],
            bounds=[(0, alpha.shape[0] - .5 - profile["pivotY"] * alpha.shape[0]), (0, None)],
            method="highs")
        if not solution.success:
            raise ValueError(f"No inscribed circle for {entry['id']}: {solution.message}")
        entry["contactCenterY"] = round((alpha.shape[0] - .5 - profile["pivotY"] * alpha.shape[0] - solution.x[0]) / ppu, 6)
        center[1] = alpha.shape[0] - .5 - profile["pivotY"] * alpha.shape[0] - entry["contactCenterY"] * ppu
    maximum = float(np.min(-(hull.equations[:, :2] @ center + hull.equations[:, 2]))) / ppu
    radius = np.floor(maximum * 1e6) / 1e6 if args.fit_outer else entry["contactRadius"] * args.radius_scale
    if radius <= 0 or radius > maximum + 1e-6:
        raise ValueError(f"Circle outside outer envelope: {entry['id']}; radius={radius}, max={maximum}")
    entry["contactRadius"] = round(radius, 6)
    print(entry["id"], "radius", entry["contactRadius"], "centerY", entry["contactCenterY"], "outer maximum", round(maximum, 6))
if args.write:
    path.write_text(json.dumps(entries, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
