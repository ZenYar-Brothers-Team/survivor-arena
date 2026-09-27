"""Static checks for docs/balance/late-skills-passives-v1.json (review data, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/late-skills-passives-v1.json"
SKILLS = ["SKILL-008", "SKILL-009", "SKILL-011", "SKILL-012", "SKILL-015", "SKILL-016"]
PASSIVES = ["PASSIVE-006", "PASSIVE-010", "PASSIVE-013", "PASSIVE-014"]
CHANNELS = {"PASSIVE-006": "disappearingXpRecoveryBonus", "PASSIVE-010": "pickedUpXpMultiplierBonus",
            "PASSIVE-013": "effectRangeMultiplierBonus", "PASSIVE-014": "lowHealthDamageMaxBonus"}
# Card L1 numbers (Content_design.md) that the table must keep: (field, value). Damage after DECISION-0073.
CARD_L1 = {
    "SKILL-008": [("damage", 12.6), ("maxHits", 3), ("cooldownSeconds", 2.0), ("knockback", 0.25)],
    "SKILL-009": [("damage", 24), ("blastRadius", 1.5), ("cooldownSeconds", 3.0), ("maxConcurrent", 4), ("knockback", 0.7)],
    "SKILL-011": [("damage", 8.4), ("count", 8), ("cooldownSeconds", 2.4), ("knockback", 0.12)],
    "SKILL-012": [("damagePerTick", 7.5), ("tickIntervalSeconds", 0.2), ("durationSeconds", 0.8), ("cooldownSeconds", 3.0), ("knockbackPerTick", 0.04)],
    "SKILL-015": [("damage", 13.5), ("count", 4), ("cooldownSeconds", 3.5), ("knockback", 0.3)],
    "SKILL-016": [("damage", 2.8), ("count", 1), ("cooldownSeconds", 0.35), ("initialSpeed", 8.0), ("stopAfterSeconds", 1.4), ("knockback", 0.08)],
}
# Card qualitative level counts that must appear exactly.
CARD_COUNTS = {
    "SKILL-008": ("maxHits", [3, 4, 4, 6, 6, 8]),
    "SKILL-009": ("maxConcurrent", [4, 4, 4, 6, 6, 6]),
    "SKILL-011": ("count", [8, 10, 10, 12, 12, 12]),
    "SKILL-012": ("durationSeconds", [0.8, 1.1, 1.1, 1.1, 1.1, 1.5]),
    "SKILL-015": ("count", [4, 4, 4, 8, 8, 8]),
    "SKILL-016": ("count", [1, 1, 2, 2, 2, 3]),
}
CARD_PASSIVES = {"PASSIVE-006": [.1, .2, .3, .4, .5, .6], "PASSIVE-010": [.05, .1, .15, .2, .25, .3],
                 "PASSIVE-013": [.08, .16, .24, .32, .4, .5], "PASSIVE-014": [.15, .25, .35, .45, .55, .7]}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def finite_tree(value, path="root"):
    if isinstance(value, dict):
        for key, item in value.items():
            finite_tree(item, f"{path}.{key}")
    elif isinstance(value, list):
        for index, item in enumerate(value):
            finite_tree(item, f"{path}[{index}]")
    elif isinstance(value, float):
        require(math.isfinite(value), f"Non-finite number at {path}")


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    finite_tree(data)
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"],
            "Review artifact must not advertise runtime compatibility")
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    require([s["id"] for s in data["skills"]] == SKILLS, "Skill roster mismatch")
    require([p["id"] for p in data["passives"]] == PASSIVES, "Passive roster mismatch")
    for name in SKILLS + PASSIVES:
        require(re.search(r"^#{2,4}\s+" + name + r"\b", content, re.M), f"Missing canonical card: {name}")

    for entry in data["skills"]:
        name = entry["id"]
        require([row["level"] for row in entry["levels"]] == list(range(1, 7)), f"Expected L1-L6: {name}")
        rows = [{**entry["shared"], **row} for row in entry["levels"]]
        for field, value in CARD_L1[name]:
            require(math.isclose(rows[0][field], value), f"Card L1 value changed: {name}.{field}")
        field, values = CARD_COUNTS[name]
        require([row[field] for row in rows] == values, f"Card level progression changed: {name}.{field}")
        for row in rows:
            for key, value in row.items():
                if isinstance(value, (int, float)) and not isinstance(value, bool):
                    require(value >= 0, f"Negative value: {name} L{row['level']}.{key}")
            if "speed" in row and "range" in row and "lifetimeSeconds" in row:
                require(math.isclose(row["range"], row["speed"] * row["lifetimeSeconds"], rel_tol=1e-5),
                        f"Range/lifetime mismatch: {name} L{row['level']}")
            if "ricochetCount" in row:
                require(row["ricochetCount"] == row["maxHits"] - 1, f"Ricochet/hits mismatch: {name} L{row['level']}")
            if name == "SKILL-016":
                require(math.isclose(row["range"], row["initialSpeed"] * row["stopAfterSeconds"] / 2, rel_tol=1e-5)
                        and row["lifetimeSeconds"] == row["stopAfterSeconds"], f"Linear decay mismatch: L{row['level']}")
        for previous, current in zip(rows, rows[1:]):
            for key in ("damage", "damagePerTick", "knockback", "knockbackPerTick", "actionSpeedBonus"):
                if key in current:
                    require(current[key] >= previous[key], f"Level regression: {name} L{current['level']}.{key}")

    for entry in data["passives"]:
        name = entry["id"]
        channel = CHANNELS[name]
        require([row["level"] for row in entry["levels"]] == list(range(1, 7)), f"Expected L1-L6: {name}")
        require(all(set(row) == {"level", channel} for row in entry["levels"]), f"Unexpected channel: {name}")
        require([row[channel] for row in entry["levels"]] == CARD_PASSIVES[name], f"Card values changed: {name}")
    schema = (ROOT / "Assets/Game/Progression/Passive/Json/CharacterStatModifierData.cs").read_text(encoding="utf-8")
    fields = {p[0].lower() + p[1:] for p in re.findall(r"public float\?? (\w+) \{", schema)}
    require(set(CHANNELS.values()) <= fields, "Passive channel missing from runtime modifier schema")

    weights = data["draftWeights"]["CHAR-001"]
    require(set(weights) == set(SKILLS + PASSIVES) and all(w > 0 for w in weights.values()), "Draft weights incomplete")
    print(f"PASS: {len(SKILLS)} skills x 6 levels, {len(PASSIVES)} passives x 6 levels")


if __name__ == "__main__":
    main()
