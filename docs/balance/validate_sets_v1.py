"""Static checks for docs/balance/sets-v1.json (review data, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/sets-v1.json"
BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"


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
    recipes = {}
    for match in re.finditer(r"^#### (SET-\d{3})[^\n]*\n(.*?)(?=^#### |^### |\Z)", content, re.M | re.S):
        recipe = re.search(r"^Рецепт: (.+)$", match[2], re.M)
        require(recipe is not None, f"Missing recipe: {match[1]}")
        recipes[match[1]] = set(re.findall(r"(?:SKILL|PASSIVE)-\d{3}", recipe[1]))
    require(len(recipes) == 20, "Expected the complete canonical set catalogue")

    baseline = {entry["id"] for entry in json.loads(BASELINE.read_text(encoding="utf-8"))["sets"]}
    require(set(data["baselineSets"]) == baseline, "Baseline set list drifted from field001-baseline-v1")
    ids = [entry["id"] for entry in data["sets"]]
    require(len(ids) == len(set(ids)) == 15 and not set(ids) & baseline, "Expected the 15 non-baseline sets once each")
    require(set(ids) | baseline == set(recipes), "Packet plus baseline must cover SET-001…020")

    for entry in data["sets"]:
        name = entry["id"]
        requirements = entry["requirements"]
        require(set(requirements) == recipes[name], f"Recipe changed: {name}")
        require(all(isinstance(level, int) and 1 <= level <= 6 for level in requirements.values()),
                f"Invalid threshold: {name}")
        require(sum(k.startswith("SKILL-") for k in requirements) <= 6
                and sum(k.startswith("PASSIVE-") for k in requirements) <= 6, f"Recipe exceeds slot capacity: {name}")
        for effect in entry["effects"]:
            targets = set(effect.get("skills", [])) | ({effect["skill"]} if "skill" in effect else set())
            require(targets <= recipes[name], f"{name}: component buff targets a skill outside its recipe")
            if effect["kind"] in ("IndependentAttack", "RewardProc"):
                require(effect["cooldownSeconds"] > 0, f"{name}: set attack needs a positive cooldown")
                require(not effect["actionSpeedScaling"] if "actionSpeedScaling" in effect else True,
                        f"{name}: set attacks ignore action speed unless the card says so")
                require(not effect["countsAsSkillActivation"], f"{name}: set attacks never count as skill activations")
            if effect.get("maxConcurrent") == 1:
                life = effect.get("lifetimeSeconds", effect.get("range", 0) / max(effect.get("speed", 1), 1e-9))
                require(life < effect["cooldownSeconds"], f"{name}: one-entity cap requires lifetime < cooldown")

    for key, decision in data["decisions"].items():
        require(decision["chosen"] in decision["options"], f"Decision {key}: choice not among options")
    print(f"PASS: {len(ids)} sets validated against Content Design recipes (+{len(baseline)} baseline)")


if __name__ == "__main__":
    main()
