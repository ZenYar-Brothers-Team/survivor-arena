"""Static checks for docs/balance/field002-v2.json and field003-v2.json: FIELD-002/003 on the FIELD-001 rhythm (review data)."""
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
F1 = ROOT / "docs/balance/field001-baseline-v1.json"
ENEMIES = ROOT / "Assets/Resources/Content/Enemies/ProductionEnemies.json"
TECHNICAL_ORDINARY_CAP = 300
MAX_TYPES_PER_PHASE = 7


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def load(name):
    return json.loads((ROOT / f"docs/balance/{name}.json").read_text(encoding="utf-8"))


def nominal(phases, enemies):
    spawns = health = 0
    for p in phases:
        n = p["burst"]["count"] if p["spawnMode"] == "Burst" else p["durationSeconds"] / p["spawnIntervalSeconds"]
        for enemy_id, weight in p["composition"].items():
            spawns += n * weight / 100
            health += n * weight / 100 * enemies[enemy_id]["maxHealth"] * p["modifiers"]["healthMultiplier"]
    return spawns, health


def check(field, data, previous, one, enemies):
    name = f"FIELD-00{field}"
    pressure, pool = data["fieldPressure"], data["enemyPool"]
    phases, reference = data["timeline"]["phases"], one["timeline"]["phases"]
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"], f"{name}: review artifact only")
    # DECISION-0136: the ruins map in slot 2 swaps the tract-themed ENEMY-008 for the fort-themed ENEMY-010.
    expected_pool = [("ENEMY-010" if e == "ENEMY-008" else e) for e in previous["enemyPool"]] if field == 2 else previous["enemyPool"]
    require(data["enemyPool"] == expected_pool, f"{name}: enemy pool unchanged from v1 (except the DECISION-0136 swap)")
    require(data["timeline"]["maxAliveEnemies"] == TECHNICAL_ORDINARY_CAP, f"{name}: shared technical cap")
    require([h["timeSeconds"] for h in data["timeline"]["hooks"]] == [450, 810], f"{name}: boss hooks at 7:30 / 13:30")

    # Rhythm: same 16 phases, modes and order as FIELD-001; rests only shortened, combat phases absorb the seconds.
    require(len(phases) == len(reference), f"{name}: FIELD-001 phase count")
    elapsed = 0
    for p, r in zip(phases, reference):
        require(p["startSeconds"] == elapsed and p["durationSeconds"] > 0, f"{name}: timeline gap/drift at {p['id']}")
        elapsed += p["durationSeconds"]
        require(p["spawnMode"] == r["spawnMode"], f"{p['id']}: spawn mode differs from FIELD-001")
        require(p["tag"] == r["tag"] or (r["tag"] == "Ordinary" and p["tag"] == "Pressure"), f"{p['id']}: tag may only escalate")
        if r["tag"] == "Rest":
            require(p["durationSeconds"] == pressure["restSeconds"] <= r["durationSeconds"], f"{p['id']}: rest length")
        require(set(p["composition"]) == set(pool) and sum(p["composition"].values()) == 100
                and min(p["composition"].values()) >= 0, f"{p['id']}: invalid mixture")
        types = sum(1 for v in p["composition"].values() if v > 0)
        require(2 <= types <= MAX_TYPES_PER_PHASE, f"{p['id']}: {types} types (FIELD-001 style keeps phases focused)")
        require(p["modifiers"] == {"healthMultiplier": pressure["healthMultiplier"], "speedMultiplier": 1,
                                   "contactDamageMultiplier": pressure["damageMultiplier"],
                                   "attackDamageMultiplier": pressure["damageMultiplier"]}, f"{p['id']}: modifiers")
        if p["spawnMode"] == "Continuous":
            require(0 < p["spawnIntervalSeconds"] < r["spawnIntervalSeconds"], f"{p['id']}: denser than FIELD-001")
        else:
            require(p["burst"]["count"] > r["burst"]["count"], f"{p['id']}: burst larger than FIELD-001")
    require(elapsed == 900, f"{name}: timeline must cover 900 s")
    first = phases[0]["composition"]
    require(all(first[e] > 0 for e in data["newEnemiesInFirstWave"]), f"{name}: new type in the first wave (DECISION-0063)")
    for enemy_id in pool:
        require(any(p["composition"][enemy_id] > 0 for p in phases), f"{name}: {enemy_id} never spawns")

    spawns, health = nominal(phases, enemies)
    require(round(health) == data["nominal"][name]["health"], f"{name}: nominal health out of date")
    return spawns, health


def main():
    one = json.loads(F1.read_text(encoding="utf-8"))
    enemies = {e["id"]: e for e in json.loads(ENEMIES.read_text(encoding="utf-8-sig"))}
    two, three = load("field002-v2"), load("field003-v2")
    f1_spawns, f1_health = nominal([dict(p, composition={k: v for k, v in p["composition"].items() if v})
                                    for p in one["timeline"]["phases"]], enemies)
    _, h2 = check(2, two, load("field002-v1"), one, enemies)
    _, h3 = check(3, three, load("field003-v1"), one, enemies)
    require(f1_health < h2 < h3, "Total health grows field by field")
    require(three["fieldPressure"]["healthMultiplier"] > two["fieldPressure"]["healthMultiplier"] > 1, "HP multiplier grows")
    require(three["fieldPressure"]["damageMultiplier"] > two["fieldPressure"]["damageMultiplier"] > 1, "Damage multiplier grows")
    # Card FIELD-002: shorter breaks than FIELD-001.
    require(two["fieldPressure"]["restSeconds"] < 20, "FIELD-002 rests shorter than FIELD-001")
    # Card FIELD-003: more enemies attacking from behind others; earlier peaks and more elite phases.
    require(three["nominal"]["FIELD-003"]["rangedShare"] > two["nominal"]["FIELD-002"]["rangedShare"], "FIELD-003 more ranged")
    early = [sum(1 for p in d["timeline"]["phases"] if p["tag"] == "Pressure" and p["startSeconds"] < 450) for d in (two, three)]
    require(early[1] > early[0], "FIELD-003 has more pressure phases before the mid-boss")
    print(f"PASS: FIELD-002 HP ×{h2 / f1_health:.2f}, FIELD-003 HP ×{h3 / f1_health:.2f} to FIELD-001; 16 phases each")


if __name__ == "__main__":
    main()
