"""Static checks for docs/balance/characters-v1.json (review data for CHAR-002…010, not runtime JSON)."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/characters-v1.json"
BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"
SETS = ROOT / "Assets/Resources/Content/Sets/ProductionSets.json"
CONTENT_DESIGN = ROOT / "docs/Content_design.md"

SKILLS = {f"SKILL-{i:03d}" for i in range(1, 17)}
PASSIVES = {f"PASSIVE-{i:03d}" for i in range(1, 15)}
CHARACTERS = [f"CHAR-{i:03d}" for i in range(2, 11)]
PACKET_SETS = {f"SET-{i:03d}" for i in range(1, 21)}
# Design bounds of this packet: noticeable but not degenerate differences to CHAR-001.
RATIO_BOUNDS = {
    "maxHealth": (0.6, 1.7),
    "movementSpeed": (0.8, 1.3),
    "activeSkillDamageMultiplier": (0.85, 1.4),
    "activeSkillCooldownMultiplier": (0.75, 1.3),
    "incomingDamageMultiplier": (0.85, 1.0),
    "pickupRadius": (0.8, 1.6),
    "effectSizeMultiplier": (1.0, 1.4),
    "effectRangeMultiplier": (1.0, 1.4),
}
LOST_SETS_RANGE = (6, 8)
LOST_BY_CHARACTERS_MAX = 5


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def card_field(content, char_id, label):
    block = re.search(r"^#### " + char_id + r" —(.*?)(?=^#### |\Z)", content, re.M | re.S)
    require(block is not None, f"{char_id}: card not found")
    line = re.search(r"^" + label + r": (.*)$", block[1], re.M)
    require(line is not None, f"{char_id}: '{label}' not found in card")
    return line[1]


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    base = json.loads(BASELINE.read_text(encoding="utf-8"))["character"]["stats"]
    sets = json.loads(SETS.read_text(encoding="utf-8"))
    content = CONTENT_DESIGN.read_text(encoding="utf-8")
    # The lost-set bounds of this packet were approved over SET-001…020; low-tier SET-021…035 (DECISION-0138)
    # have their own validator (validate_sets_low_v1.py) and are outside this packet's rule.
    recipes = {s["id"]: {r["id"] for r in s["recipe"]} for s in sets if s["id"] in PACKET_SETS}
    require(len(recipes) == 20, "expected SET-001…020 in production sets")

    require(data["format"] == "late-content-review-data" and data["revision"] == "characters-v1", "format/revision")
    rule = data["weightRule"]
    require((rule["boosted"], rule["blocked"], rule["default"]) == (1.35, 0, 1), "weight rule 1.35/0/1")
    chars = data["characters"]
    require([c["id"] for c in chars] == CHARACTERS, "CHAR-002…010 in order")

    lost_by = {set_id: [] for set_id in recipes}
    for c in chars:
        cid = c["id"]
        require(set(c["stats"]) == set(base), f"{cid}: stats must list all {len(base)} baseline fields")
        start = card_field(content, cid, "Стартовое умение")
        require(c["startingSkill"] in start, f"{cid}: starting skill differs from card")
        for field, (low, high) in RATIO_BOUNDS.items():
            ratio = c["stats"][field] / base[field]
            require(low - 1e-9 <= ratio <= high + 1e-9, f"{cid}: {field} ratio {ratio:.2f} outside {low}…{high}")
        changed = [f for f in base if c["stats"][f] != base[f]]
        require(set(changed) <= set(c["highlights"]) | {"disappearingXpRecovery"},
                f"{cid}: every changed stat except XP recovery must be highlighted")
        require(any(c["stats"][f] / base[f] >= 1.3 or c["stats"][f] / base[f] <= 0.77
                    for f in RATIO_BOUNDS), f"{cid}: needs at least one ±30% main-stat difference")
        require(len(c["blockedSkills"]) == 2 and len(c["blockedPassives"]) == 2, f"{cid}: 2 + 2 blocks")
        require(set(c["blockedSkills"]) <= SKILLS and set(c["boostedSkills"]) <= SKILLS, f"{cid}: skill ids")
        require(set(c["blockedPassives"]) <= PASSIVES and set(c["boostedPassives"]) <= PASSIVES, f"{cid}: passive ids")
        require(len(c["boostedSkills"]) == 3 and len(c["boostedPassives"]) == 3, f"{cid}: 3 + 3 boosted")
        require(not set(c["blockedPassives"]) & set(c["boostedPassives"]), f"{cid}: passive both boosted and blocked")
        require(c["startingSkill"] not in c["blockedSkills"], f"{cid}: starting skill blocked")
        require(not set(c["blockedSkills"]) & set(c["boostedSkills"]), f"{cid}: skill both boosted and blocked")
        weights_line = card_field(content, cid, r"Draft weights \(\[DECISION-0089\]\(decisions/0089-characters-v1\.md\)\)")
        raised, lowered = weights_line.split("0 (не выпадают)")
        for entry in c["blockedSkills"] + c["blockedPassives"]:
            require(entry in lowered and entry not in raised, f"{cid}: blocked {entry} differs from card")
        for entry in c["boostedSkills"] + c["boostedPassives"]:
            require(entry in raised, f"{cid}: boosted {entry} differs from card")
        stats_line = card_field(content, cid, r"Базовые характеристики \(\[DECISION-0089\]\(decisions/0089-characters-v1\.md\)\)")
        require(stats_line.startswith(f"{c['stats']['maxHealth']:g} HP;"), f"{cid}: card HP differs from packet")

        blocked = set(c["blockedSkills"]) | set(c["blockedPassives"])
        lost = [s for s, recipe in recipes.items() if recipe & blocked]
        low, high = LOST_SETS_RANGE
        require(low <= len(lost) <= high, f"{cid}: loses {len(lost)} sets, expected {low}…{high}")
        own = [s for s, recipe in recipes.items() if c["startingSkill"] in recipe and s not in lost]
        require(own, f"{cid}: no reachable set uses the starting skill")
        for s in lost:
            lost_by[s].append(cid)

    for set_id, owners in lost_by.items():
        require(len(owners) <= LOST_BY_CHARACTERS_MAX, f"{set_id}: unreachable for {len(owners)} characters")
    blocked_sets = {tuple(sorted(s for s, o in lost_by.items() if c["id"] in o)) for c in chars}
    require(len(blocked_sets) == len(chars), "two characters lose exactly the same sets")

    print(f"PASS: {len(chars)} characters, set loss per character "
          f"{min(sum(c['id'] in o for o in lost_by.values()) for c in chars)}…"
          f"{max(sum(c['id'] in o for o in lost_by.values()) for c in chars)}, "
          f"max {max(len(o) for o in lost_by.values())} characters per unreachable set")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # non-ASCII report text; Windows consoles default to cp1251
    main()
