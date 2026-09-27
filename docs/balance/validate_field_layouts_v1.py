"""Static checks for docs/balance/field-layouts-v1.json (DECISION-0068) and a reference run of the layout algorithm.

The runtime (C#) uses its own random source, so this script checks invariants over many seeds, not exact positions.
"""
import json
import math
import random
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/field-layouts-v1.json"
SIDE, WALL, SEEDS = 200.0, 1.0, 200
PACKET_COUNTS = {"FIELD-001": 64, "FIELD-002": 100, "FIELD-003": 107}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def rotate(pieces, degrees):
    result = []
    for p in pieces:
        x, y, w, h = p["x"], p["y"], p["width"], p["height"]
        for _ in range(degrees // 90):
            x, y, w, h = -y, x, h, w
        result.append(dict(p, x=x, y=y, width=w, height=h))
    return result


def bounds(pieces):
    return (min(p["x"] - p["width"] / 2 for p in pieces), min(p["y"] - p["height"] / 2 for p in pieces),
            max(p["x"] + p["width"] / 2 for p in pieces), max(p["y"] + p["height"] / 2 for p in pieces))


def gap(a, b):
    return max(abs(a["x"] - b["x"]) - (a["width"] + b["width"]) / 2, abs(a["y"] - b["y"]) - (a["height"] + b["height"]) / 2)


def point_distance(p, px, py):
    dx = max(abs(px - p["x"]) - p["width"] / 2, 0)
    dy = max(abs(py - p["y"]) - p["height"] / 2, 0)
    return math.hypot(dx, dy)


def generate(layout, seed):
    rng = random.Random(seed)
    inner = SIDE / 2 - WALL - layout["edgeMargin"]
    cells = int(2 * inner // layout["cellSize"])
    origin = -cells * layout["cellSize"] / 2
    weights = [p["weight"] for p in layout["patterns"]]
    placed = []
    for row in range(cells):
        for col in range(cells):
            x0 = origin + col * layout["cellSize"] + layout["cellMargin"]
            y0 = origin + row * layout["cellSize"] + layout["cellMargin"]
            x1 = x0 + layout["cellSize"] - 2 * layout["cellMargin"]
            y1 = y0 + layout["cellSize"] - 2 * layout["cellMargin"]
            in_cell = []
            for _ in range(layout["patternsPerCell"]):
                for _attempt in range(layout["placementAttempts"]):
                    pattern = rng.choices(layout["patterns"], weights)[0]
                    pieces = rotate(pattern["pieces"], rng.choice(pattern["rotations"]))
                    bx0, by0, bx1, by1 = bounds(pieces)
                    cx = rng.uniform(x0 - bx0, x1 - bx1)
                    cy = rng.uniform(y0 - by0, y1 - by1)
                    moved = [dict(p, x=p["x"] + cx, y=p["y"] + cy, cell=(row, col)) for p in pieces]
                    if any(point_distance(p, 0, 0) < layout["startClearRadius"] for p in moved):
                        continue
                    if any(gap(a, b) < layout["minPatternGap"] for a in moved for b in in_cell):
                        continue
                    in_cell.extend(moved)
                    break
            placed.extend(in_cell)
    return placed, cells


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"], "Review artifact only")
    require([f["fieldId"] for f in data["fields"]] == ["FIELD-001", "FIELD-002", "FIELD-003"], "Layouts for the first three fields")
    for layout in data["fields"]:
        fid = layout["fieldId"]
        interior = layout["cellSize"] - 2 * layout["cellMargin"]
        require(layout["patternsPerCell"] >= 1 and layout["placementAttempts"] >= 1 and layout["startClearRadius"] > 0, f"{fid}: parameters")
        for pattern in layout["patterns"]:
            require(pattern["weight"] > 0 and pattern["rotations"] and set(pattern["rotations"]) <= {0, 90, 180, 270},
                    f"{fid}.{pattern['id']}: weight/rotations")
            pieces = pattern["pieces"]
            for i, a in enumerate(pieces):
                for b in pieces[i + 1:]:
                    require(gap(a, b) >= 0.5, f"{fid}.{pattern['id']}: pieces overlap")
            for degrees in pattern["rotations"]:
                bx0, by0, bx1, by1 = bounds(rotate(pieces, degrees))
                require(bx1 - bx0 <= interior and by1 - by0 <= interior, f"{fid}.{pattern['id']}: does not fit its cell")
        counts, empty_cells = [], 0
        for seed in range(SEEDS):
            placed, cells = generate(layout, seed)
            counts.append(len(placed))
            occupied = {p["cell"] for p in placed}
            empty_cells = max(empty_cells, cells * cells - len(occupied))
            for p in placed:
                require(abs(p["x"]) + p["width"] / 2 <= SIDE / 2 - WALL and abs(p["y"]) + p["height"] / 2 <= SIDE / 2 - WALL,
                        f"{fid} seed {seed}: outside the arena")
                require(point_distance(p, 0, 0) >= layout["startClearRadius"] - 1e-6, f"{fid} seed {seed}: blocks the start")
            for i, a in enumerate(placed):
                for b in placed[i + 1:]:
                    if a["cell"] != b["cell"]:
                        require(gap(a, b) >= 2 * layout["cellMargin"] - 1e-6, f"{fid} seed {seed}: passage between cells too narrow")
        mean = sum(counts) / len(counts)
        require(abs(mean - PACKET_COUNTS[fid]) / PACKET_COUNTS[fid] <= 0.15,
                f"{fid}: mean {mean:.1f} pieces is not close to the approved {PACKET_COUNTS[fid]}")
        require(empty_cells == 0, f"{fid}: some seed leaves a cell empty (density must stay uniform)")
        print(f"  {fid}: {cells}×{cells} cells, pieces {min(counts)}…{max(counts)} (mean {mean:.1f}, approved layout "
              f"{PACKET_COUNTS[fid]}), passages ≥ {2 * layout['cellMargin']}")
    print(f"PASS: field layouts v1 — {len(data['fields'])} fields, {SEEDS} seeds each")


if __name__ == "__main__":
    main()
