"""Read-only consistency checks for the F1-00 review artifact, not runtime tests."""

import itertools
import json
import math
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PATH = Path(__file__).with_name("field001-baseline-v1.json")


def require(condition, message):
    if not condition:
        raise ValueError(message)


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        require(key not in result, f"Duplicate JSON key: {key}")
        result[key] = value
    return result


def finite_tree(value, path="root"):
    if isinstance(value, float):
        require(math.isfinite(value), f"Non-finite number: {path}")
    elif isinstance(value, dict):
        for key, item in value.items():
            finite_tree(item, f"{path}.{key}")
    elif isinstance(value, list):
        for index, item in enumerate(value):
            finite_tree(item, f"{path}[{index}]")


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"), object_pairs_hook=unique_object)
    finite_tree(data)
    require(data["format"] == "field001-review-data" and not data["runtimeImportable"],
            "Review artifact must not advertise runtime compatibility")
    roster = data["initialRoster"]
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    for section, roster_key, count in [("skills", "actives", 10), ("passives", "passives", 10),
                                      ("sets", "sets", 5), ("enemies", "enemies", 6),
                                      ("travelers", "travelers", 3)]:
        entries = data[section]
        names = [entry["id"] for entry in entries]
        require(len(names) == len(set(names)) == count, f"Duplicate/count error: {section}")
        require(set(names) == set(roster[roster_key]), f"Roster mismatch: {section}")
        for name in names:
            require(re.search(r"^#{2,4}\s+" + name + r"\b", content, re.M),
                    f"Missing canonical card: {name}")

    for section in ["skills", "passives"]:
        for entry in data[section]:
            require([row["level"] for row in entry["levels"]] == list(range(1, 7)),
                    f"Expected L1-L6: {entry['id']}")
            for row in entry["levels"]:
                merged = {**entry.get("shared", {}), **row}
                if entry["id"] in ["SKILL-001", "SKILL-002", "SKILL-005", "SKILL-013"]:
                    require(math.isclose(merged["range"], merged["speed"] * merged["lifetimeSeconds"]),
                            f"Range/lifetime mismatch: {entry['id']} L{row['level']}")
    skills = {entry["id"]: entry for entry in data["skills"]}
    require([row["count"] for row in skills["SKILL-002"]["levels"]] == [3, 4, 5, 7, 9, 11],
            "SKILL-002 projectile progression changed")
    require([row["count"] for row in skills["SKILL-013"]["levels"]] == [4, 5, 6, 7, 9, 13],
            "SKILL-013 projectile progression changed")

    # Validate recipes against all 20 canonical recipes, not a second handwritten list.
    recipes = {}
    for match in re.finditer(r"^#### (SET-\d{3})[^\n]*\n(.*?)(?=^#### |\Z)", content, re.M | re.S):
        recipe = re.search(r"^Рецепт: (.+)$", match[2], re.M)
        require(recipe is not None, f"Missing recipe: {match[1]}")
        recipes[match[1]] = set(re.findall(r"(?:SKILL|PASSIVE)-\d{3}", recipe[1]))
    require(len(recipes) == 20, "Expected complete canonical set catalogue")
    initial_items = set(roster["actives"] + roster["passives"])
    available = {name for name, recipe in recipes.items() if recipe <= initial_items}
    require(available == set(roster["sets"]), "Initial item pool must enable exactly these five recipes")
    for entry in data["sets"]:
        require(set(entry["requirements"]) == recipes[entry["id"]], f"Recipe changed: {entry['id']}")
        require(all(isinstance(level, int) and 1 <= level <= 6 for level in entry["requirements"].values()),
                f"Invalid threshold: {entry['id']}")
    four_set_build = False
    for group in itertools.combinations(available, 4):
        items = set().union(*(recipes[name] for name in group))
        if (sum(item.startswith("SKILL-") for item in items) <= 6
                and sum(item.startswith("PASSIVE-") for item in items) <= 6):
            four_set_build = True
    require(four_set_build, "Stress scenario with four sets exceeds slot capacity")

    stats_schema = (ROOT / "Assets/Game/Character/Model/Json/CharacterBaseStatsData.cs").read_text()
    properties = re.findall(r"public float\? (\w+) \{", stats_schema)
    require(set(data["character"]["stats"]) == {p[0].lower() + p[1:] for p in properties},
            "Character stat matrix no longer covers every required DTO stat")
    costs = data["experience"]["levelThresholds"]
    # DECISION-0075: C'(L) = round(C(L) * (0.8 + 2.2 * exp(-(L-1) / 7.64))), non-decreasing, where
    # C(L) = 4 + floor(1.5 * (L - 1)); early levels cost 3x, the cumulative cost to L40 stays ~1258.
    expected, previous = [], 0
    for i in range(60):
        value = max(previous, round((4 + math.floor(1.5 * i)) * (0.8 + 2.2 * math.exp(-i / 7.64))))
        expected.append(value); previous = value
    require(costs == expected, "XP formula/array mismatch")
    require(costs[0] == 12 and sum(costs[:39]) == 1257 and costs[-1] == 74, "XP example mismatch")

    enemies = {entry["id"]: entry for entry in data["enemies"]}
    require(math.isclose(enemies["ENEMY-001"]["movementSpeed"], 0.96),
            "ENEMY-001 must remain 20% slower than its former 1.20 speed")
    for delta in data["ordinaryEnemyChanges"]:
        require(all(enemies[delta["id"]][key] == value for key, value in delta["proposed"].items()),
                f"Enemy before/after table out of sync: {delta['id']}")
    elapsed, nominal, xp, bursts = 0, 0, 0, 0
    intro = {}
    technical_cap = data["timeline"]["maxAliveEnemies"]
    require(technical_cap == 200, "Ordinary-enemy technical cap must be the shared 200")
    for phase in data["timeline"]["phases"]:
        require(phase["startSeconds"] == elapsed and phase["durationSeconds"] > 0,
                f"Timeline gap/overlap: {phase['id']}")
        weights = phase["composition"]
        require(set(weights) <= set(enemies) and sum(weights.values()) == 100
                and 2 <= len(weights) <= 4 and all(weight > 0 for weight in weights.values()),
                f"Invalid focused mixture: {phase['id']}")
        require(all(value == 1 for value in phase["modifiers"].values()), "Unexpected stat growth")
        require(phase["spawnIntervalSeconds"] > 0 and "maxAliveEnemies" not in phase, "Invalid cadence/phase cap")
        for name, weight in weights.items():
            if weight:
                intro.setdefault(name, elapsed)
        if phase["spawnMode"] == "Burst":
            burst = phase["burst"]
            require(0 <= burst["offsetSeconds"] < phase["durationSeconds"]
                    and 0 < burst["windowSeconds"] <= phase["durationSeconds"] - burst["offsetSeconds"],
                    "Burst window extends beyond its phase")
            count = burst["count"]
            require(isinstance(count, int) and count > 0, "Invalid burst count")
            bursts += count
        else:
            require(phase["spawnMode"] == "Continuous" and phase["burst"] is None, "Wrong phase kind")
            count = math.floor(phase["durationSeconds"] / phase["spawnIntervalSeconds"])
        nominal += count
        xp += count * sum(enemies[name]["experienceReward"] * weight for name, weight in weights.items()) / 100
        elapsed += phase["durationSeconds"]
    require(elapsed == data["field"]["durationSeconds"] == 900, "Run must last 900 seconds")
    require(intro["ENEMY-004"] == 230 and intro["ENEMY-007"] == 340 and intro["ENEMY-005"] == 430,
            "Threat introductions moved")
    require([(h["kind"], h["timeSeconds"]) for h in data["timeline"]["hooks"]]
            == [("MidBoss", data["midboss"]["spawnSeconds"]), ("FinalBoss", data["boss"]["spawnSeconds"])],
            "Hook/body spawn timing mismatch")
    require(technical_cap <= data["performanceProposal"]["ordinaryStressCount"], "Stress count below upper bound")

    obstacles = data["field"]["obstacles"]
    require(len(obstacles) == len({item["id"] for item in obstacles}) == 64, "Expected 64 distinct obstacles")
    for item in obstacles:
        require(abs(item["x"]) + item["width"] / 2 < 99 and abs(item["y"]) + item["height"] / 2 < 99,
                f"Obstacle outside playable bounds: {item['id']}")
        require(math.hypot(item["x"], item["y"]) - math.hypot(item["width"], item["height"]) / 2 > 3,
                f"Spawn clearance lost: {item['id']}")
    for a, b in itertools.combinations(obstacles, 2):
        require(abs(a["x"] - b["x"]) >= (a["width"] + b["width"]) / 2 + 3
                or abs(a["y"] - b["y"]) >= (a["height"] + b["height"]) / 2 + 3,
                f"Obstacle clearance below 3: {a['id']}/{b['id']}")
    probabilities = data["travelerSchedule"]["countProbabilities"]
    require(len(probabilities) == 4 and all(0 <= p <= 1 for p in probabilities)
            and math.isclose(sum(probabilities), 1), "Invalid Traveler probabilities")
    for entry in data["travelers"]:
        require(entry["presenceSeconds"] > 0 and entry["support"]["cooldownSeconds"] > 0
                and entry["support"]["supportTargets"] > 0, "Required Traveler numeric field missing")
        require(entry["role"] == "Offensive" or (entry["contactDamage"] == 0 and entry["attack"] is None),
                "Peaceful Traveler may not attack")
    for entry in data["pickups"]["definitions"]:
        require(entry["contactRadius"] > 0 and (entry["lifetimeSeconds"] is None or entry["lifetimeSeconds"] > 0),
                "Pickup lifetime must be null or positive")
    for entry in data["artInventory"]:
        if entry["runtime"]:
            require((ROOT / entry["runtime"]).is_file(), f"Missing art: {entry['runtime']}")
    print(f"PASS: 60 skill + 60 passive levels; exactly 5 canonical recipes; {elapsed}s / {len(data['timeline']['phases'])} phases")
    print(f"Nominal requests={nominal}; expected ordinary XP={xp:.1f}; bursts={bursts}; technical cap={technical_cap}")
    print(f"PASS: six-enemy delta, required character stats, Traveler/pickup values, 64 obstacle bounds/gaps, {len(data['artInventory'])} art paths")
    print("Static proposal validation only; runtime compatibility, difficulty and performance are not verified.")


if __name__ == "__main__":
    main()
