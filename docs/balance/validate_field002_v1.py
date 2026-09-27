"""Static checks for docs/balance/field002-v1.json (review data for the FIELD-002 slice, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/field002-v1.json"
BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def card(content, card_id):
    block = re.search(r"^### " + card_id + r" —.*?(?=^### (?:BOSS|MIDBOSS)-\d{3} —|^## |\Z)", content, re.M | re.S)
    require(block is not None, f"Missing canonical card: {card_id}")
    text = block.group(0)
    hp = re.search(r"HP: (\d+)\. Размер / collision: ([\d.]+)\. Contact damage: (\d+)\.", text)
    kb = re.search(r"Knockback: contact ([\d.]+); dash contact ([\d.]+); circular projectile ([\d.]+)\. Knockback resistance (\d+)%", text)
    require(hp and kb, f"Card numbers not parseable: {card_id}")
    return {"maxHealth": int(hp[1]), "collisionSize": float(hp[2]), "contactDamage": int(hp[3]),
            "contactKnockback": float(kb[1]), "dashKnockback": float(kb[2]), "ringKnockback": float(kb[3]),
            "knockbackResistance": int(kb[4]) / 100}, text


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    base = json.loads(BASELINE.read_text(encoding="utf-8"))
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"], "Review artifact only")
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    pressure = data["fieldPressure"]
    pool = data["enemyPool"]
    field_one = {e["id"] for e in base["enemies"]}
    require(field_one < set(pool) and set(pool) - field_one == {"ENEMY-006", "ENEMY-008", "ENEMY-009"},
            "FIELD-002 pool = FIELD-001 six + ENEMY-006/008/009 (DECISION-0063)")
    for enemy_id in set(pool) - field_one:
        block = re.search(r"^### " + enemy_id + r" —.*?XP reward: \d+\. Контексты: FIELD-(\d{3})", content, re.M | re.S)
        require(block and block[1] == "002", f"{enemy_id}: card context must start at FIELD-002")
    first = data["timeline"]["phases"][0]["composition"]
    require(any(first[e] > 0 for e in data["newEnemiesInFirstWave"]) and set(data["newEnemiesInFirstWave"]) <= set(pool) - field_one,
            "A new enemy type must appear in the first wave (DECISION-0063)")

    # Timeline keeps its approved cadence; the ordinary-enemy ceiling is a shared technical safeguard.
    phases = data["timeline"]["phases"]
    require(data["timeline"]["maxAliveEnemies"] == base["timeline"]["maxAliveEnemies"] == 200,
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

    # Field: authored obstacles inside the arena, clear start, no overlapping pieces.
    field = data["field"]
    half = field["arenaWidth"] / 2 - field["wallThickness"]
    obstacles = field["obstacles"]
    clear = field["obstacleClusters"]["startClearRadius"]
    for o in obstacles:
        require(abs(o["x"]) + o["width"] / 2 < half and abs(o["y"]) + o["height"] / 2 < half, f"Outside arena: {o['id']}")
        require(math.hypot(o["x"], o["y"]) >= clear, f"Blocks the start area: {o['id']}")
    for i, a in enumerate(obstacles):
        for b in obstacles[i + 1:]:
            gap_x = abs(a["x"] - b["x"]) - (a["width"] + b["width"]) / 2
            gap_y = abs(a["y"] - b["y"]) - (a["height"] + b["height"]) / 2
            require(max(gap_x, gap_y) >= 0.5, f"Overlapping obstacles: {a['id']} {b['id']}")

    # Bosses: card numbers kept; missing timings present and readable.
    for key, card_id, ring_count in (("midboss", "MIDBOSS-002", 6), ("boss", "BOSS-002", 8)):
        entry = data[key]
        numbers, text = card(content, card_id)
        require(entry["id"] == card_id, f"{key} id")
        for field_name in ("maxHealth", "collisionSize", "contactDamage", "contactKnockback", "knockbackResistance"):
            require(math.isclose(entry[field_name], numbers[field_name]), f"{card_id}.{field_name} differs from the card")
        require(math.isclose(entry["movement"]["dashKnockback"], numbers["dashKnockback"]), f"{card_id}: dash knockback")
        ring = entry["dashEndAttack"]
        require(ring["projectileCount"] == ring_count and math.isclose(ring["knockback"], numbers["ringKnockback"])
                and ring["trigger"] == "dash-end", f"{card_id}: post-dash ring")
        require(entry["movement"]["dashTelegraphSeconds"] >= 0.6, f"{card_id}: readable dash wind-up")
        require(entry["movement"]["dashSpeedMultiplier"] * entry["movementSpeed"] > 3.0,
                f"{card_id}: dash must be a real threat above player speed")
    require(data["boss"]["dashEndAttack"]["damage"] == 16, "BOSS-002 ring damage is 16 on its card")
    require(data["boss"]["enrage"]["everyNthDash"] == 2 and data["boss"]["enrage"]["healthThreshold"] == 0.5,
            "BOSS-002 enrage: every second dash below 50%")
    require(data["boss"]["teleport"] == base["boss"]["teleport"], "Teleport reuses the BOSS-001 rule unchanged")
    require(data["travelers"]["pool"].startswith("global"), "Traveler pool is global with unique roles (DECISION-0063)")
    print(f"PASS: FIELD-002 slice — {len(phases)} phases, {len(obstacles)} obstacles, MIDBOSS-002, BOSS-002")


if __name__ == "__main__":
    main()
