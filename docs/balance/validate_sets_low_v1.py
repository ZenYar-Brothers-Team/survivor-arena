"""Static checks for docs/balance/sets-low-v1.json (review data, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/sets-low-v1.json"


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def numbers(value, path="root"):
    if isinstance(value, dict):
        for key, item in value.items():
            yield from numbers(item, f"{path}.{key}")
    elif isinstance(value, list):
        for index, item in enumerate(value):
            yield from numbers(item, f"{path}[{index}]")
    elif isinstance(value, (int, float)) and not isinstance(value, bool):
        yield path, value


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"],
            "Review artifact must not advertise runtime compatibility")
    for path, value in numbers(data):
        require(math.isfinite(value) and value >= 0, f"Negative or non-finite value at {path}")

    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    existing = []
    low_recipes = {entry["id"]: set(entry["requirements"]) for entry in data["sets"]}
    for match in re.finditer(r"^#### SET-(\d{3})[^\n]*\n(.*?)(?=^#### |^### |\Z)", content, re.M | re.S):
        recipe = re.search(r"^Рецепт: (.+)$", match[2], re.M)
        if not recipe:
            continue
        components = set(re.findall(r"(?:SKILL|PASSIVE)-\d{3}", recipe[1]))
        if int(match[1]) <= 20:
            existing.append(frozenset(components))
        else:  # a written low-tier card must match the packet
            require(low_recipes.get(f"SET-{match[1]}") == components, f"SET-{match[1]}: card recipe differs from packet")
    require(len(existing) == 20, "Expected the 20 canonical recipes")
    rules = data["recipeRules"]
    start = set(data["startAvailable"])
    sets = data["sets"]
    ids = [entry["id"] for entry in sets]
    require(len(ids) == len(set(ids)) == 15, "Expected 15 distinct sets")
    require(ids == [f"SET-0{n}" for n in range(21, 36)], "IDs must be SET-021…035")
    counts = {}
    seen = []
    startable = 0
    for entry in sets:
        name = entry["id"]
        req = entry["requirements"]
        require(rules["minComponents"] <= len(req) <= rules["maxComponents"], f"{name}: component count")
        require(rules["minLevelSum"] <= sum(req.values()) <= rules["maxLevelSum"], f"{name}: level sum")
        require(all(isinstance(v, int) and 1 <= v <= 6 for v in req.values()), f"{name}: threshold range")
        comps = frozenset(req)
        require(comps not in existing and comps not in seen, f"{name}: recipe duplicates an existing set")
        seen.append(comps)
        counts[entry["category"]] = counts.get(entry["category"], 0) + 1
        if comps <= start:
            startable += 1
        else:
            require(comps <= start | set(data["componentUnlockAfterField001"]), f"{name}: unknown component")
        if entry["category"] == "passive":
            require(all(k.startswith("PASSIVE-") for k in req), f"{name}: passive set must use only passives")
        for effect in entry["effects"]:
            targets = set(effect.get("skills", [])) | ({effect["skill"]} if "skill" in effect else set())
            require(targets <= comps, f"{name}: buff targets a skill outside its recipe")
            if effect["kind"] == "IndependentAttack":
                require(effect["cooldownSeconds"] > 0 and not effect["actionSpeedScaling"]
                        and not effect["countsAsSkillActivation"], f"{name}: set attack contract")
                if effect.get("maxConcurrent") == 1:
                    require(effect["lifetimeSeconds"] < effect["cooldownSeconds"], f"{name}: lifetime >= cooldown")
    require(counts == {"attack": 2, "passive": 3, "skill-boost": 10}, f"Category split wrong: {counts}")
    require(startable >= 4, "At least 4 sets must be available from the start")
    print(f"PASS: {len(ids)} low-tier sets, {startable} start-available, categories {counts}")


if __name__ == "__main__":
    main()
