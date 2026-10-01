"""Static checks for docs/balance/field003-v1.json (review data for FIELD-003, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/field003-v1.json"
F1 = ROOT / "docs/balance/field001-baseline-v1.json"
F2 = ROOT / "docs/balance/field002-v1.json"
BOSSES = ROOT / "docs/balance/bosses-v1.json"
TECHNICAL_ORDINARY_CAP = 300
# DECISION-0136 later moved card contexts (ENEMY-010 to FIELD-002, ENEMY-008 off FIELD-002). This v1 packet is
# checked against the card as it stood at its approval; the current card is accepted as that approved successor.
LATER_CONTEXTS = {"ENEMY-008": (3, 7), "ENEMY-010": (2, 8)}
HISTORIC_CONTEXTS = {"ENEMY-008": (2, 7), "ENEMY-010": (4, 8)}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def contexts(content, enemy_id):
    block = re.search(r"^### " + enemy_id + r" —.*?Контексты: FIELD-(\d{3})…(\d{3})", content, re.M | re.S)
    require(block is not None, f"{enemy_id}: card contexts not parseable")
    return int(block[1]), int(block[2])


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    one = json.loads(F1.read_text(encoding="utf-8"))
    two = json.loads(F2.read_text(encoding="utf-8"))
    bosses = {e["id"]: e for e in json.loads(BOSSES.read_text(encoding="utf-8"))["encounters"]}
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"], "Review artifact only")
    pressure = data["fieldPressure"]
    pool = data["enemyPool"]

    # Pool: FIELD-002 pool plus the new first-wave type; every type allowed on FIELD-003 by its card (or the proposed change).
    require(pool[:len(two["enemyPool"])] == two["enemyPool"] and set(pool) - set(two["enemyPool"]) == set(data["newEnemiesInFirstWave"]),
            "FIELD-003 pool = FIELD-002 pool + the new first-wave type")
    changes = {c["id"]: c for c in data["cardChanges"]}
    for enemy_id in pool:
        start, end = contexts(content, enemy_id)
        if enemy_id in LATER_CONTEXTS and (start, end) == LATER_CONTEXTS[enemy_id]:
            start, end = HISTORIC_CONTEXTS[enemy_id]
        if enemy_id in changes:
            change = changes[enemy_id]
            require(change["before"] == f"FIELD-{start:03d}…{end:03d}" or change["proposed"] == f"FIELD-{start:03d}…{end:03d}",
                    f"{enemy_id}: card change must start from (or already equal) the card")
            start = int(change["proposed"][6:9])
        require(start <= 3 <= end, f"{enemy_id}: not allowed on FIELD-003 by its card")
    first = data["timeline"]["phases"][0]["composition"]
    require(all(first[e] > 0 for e in data["newEnemiesInFirstWave"]), "The new type appears in the first wave (DECISION-0063)")

    # Timeline keeps its approved cadence; every field shares one technical ordinary-enemy ceiling.
    phases, prev = data["timeline"]["phases"], two["timeline"]["phases"]
    require(data["timeline"]["maxAliveEnemies"] == one["timeline"]["maxAliveEnemies"]
            == two["timeline"]["maxAliveEnemies"] == TECHNICAL_ORDINARY_CAP,
            "All fields use the shared technical ordinary-enemy cap")
    elapsed = 0
    for p in phases:
        require(p["startSeconds"] == elapsed and p["durationSeconds"] > 0,
                f"Timeline gap/drift: {p['id']}")
        elapsed += p["durationSeconds"]
        require(set(p["composition"]) == set(pool) and sum(p["composition"].values()) == 100
                and min(p["composition"].values()) >= 0, f"Invalid mixture: {p['id']}")
        require(p["modifiers"] == {"healthMultiplier": pressure["healthMultiplier"], "speedMultiplier": 1,
                                   "contactDamageMultiplier": pressure["damageMultiplier"],
                                   "attackDamageMultiplier": pressure["damageMultiplier"]}, f"Modifiers: {p['id']}")
        require("maxAliveEnemies" not in p, f"Phase-specific cap returned: {p['id']}")
        if p["spawnMode"] == "Continuous":
            require(p["spawnIntervalSeconds"] > 0, f"Invalid cadence: {p['id']}")
        else:
            require(p["burst"]["count"] > 0, f"Invalid burst: {p['id']}")
    require(elapsed == 900, "Timeline must cover 900 s")
    require([h["timeSeconds"] for h in data["timeline"]["hooks"]] == [450, 810], "Boss hooks at 7:30 / 13:30")
    require(pressure["healthMultiplier"] > two["fieldPressure"]["healthMultiplier"]
            and pressure["damageMultiplier"] > two["fieldPressure"]["damageMultiplier"], "Harder than FIELD-002")
    nominal = data["nominal"]
    require(nominal["FIELD-003"]["health"] > nominal["FIELD-002"]["health"] > nominal["FIELD-001"]["health"], "Total health grows")
    require(nominal["FIELD-003"]["rangedShare"] > nominal["FIELD-002"]["rangedShare"],
            "More enemies attacking from behind others (card FIELD-003)")
    pressure_before_half = sum(1 for p in phases if p["tag"] == "Pressure" and p["startSeconds"] < 450)
    pressure_before_half_f2 = sum(1 for p in prev if p["tag"] == "Pressure" and p["startSeconds"] < 450)
    require(pressure_before_half > pressure_before_half_f2, "Pressure peaks come earlier / more often (card FIELD-003)")

    # Field: authored ruins inside the arena, clear start, no overlapping pieces, wide passages between clusters.
    field = data["field"]
    require(field["id"] == "FIELD-003" and field["difficulty"] == 2, "FIELD-003, difficulty 2/5 (card)")
    half = field["arenaWidth"] / 2 - field["wallThickness"]
    obstacles = field["obstacles"]
    clear = field["obstacleClusters"]["startClearRadius"]
    for o in obstacles:
        require(o["kind"] in ("Wall", "Rubble") and o["rotationDegrees"] == 0, f"Unsupported obstacle: {o['id']}")
        require(abs(o["x"]) + o["width"] / 2 < half and abs(o["y"]) + o["height"] / 2 < half, f"Outside arena: {o['id']}")
        require(math.hypot(o["x"], o["y"]) >= clear, f"Blocks the start area: {o['id']}")
    for i, a in enumerate(obstacles):
        for b in obstacles[i + 1:]:
            gap_x = abs(a["x"] - b["x"]) - (a["width"] + b["width"]) / 2
            gap_y = abs(a["y"] - b["y"]) - (a["height"] + b["height"]) / 2
            require(max(gap_x, gap_y) >= 0.5, f"Overlapping obstacles: {a['id']} {b['id']}")
            if a["cluster"] != b["cluster"]:
                require(max(gap_x, gap_y) >= 4, f"Passage narrower than 4 units: {a['id']} {b['id']}")
    require(not field["waterDecor"]["blocksMovement"], "Water stays visual (card FIELD-003)")

    # Bosses and Travelers.
    for key, hook in (("finalBossId", "FinalBoss"), ("midBossId", "MidBoss")):
        boss = bosses[field[key]]
        require(boss["field"] == "FIELD-003" and boss["hook"] == hook, f"{field[key]}: bosses-v1 binds it to FIELD-003")
    require(data["travelers"]["pool"].startswith("global"), "Traveler pool is global with unique roles (DECISION-0063)")
    print(f"PASS: FIELD-003 — {len(phases)} phases, {len(obstacles)} obstacles, pool {len(pool)}, "
          f"HP ×{nominal['FIELD-003']['health'] / nominal['FIELD-001']['health']:.2f} to FIELD-001")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # non-ASCII report text; Windows consoles default to cp1251
    main()
