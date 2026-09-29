"""Create an isolated laboratory profile from a declarative starting point.

This tool never opens the player's save. Unity's ProfileCodec remains the final
authority when the resulting profile is loaded into a campaign.
"""

import argparse
import json
from pathlib import Path


def _exact_keys(value, required, label):
    if not isinstance(value, dict) or set(value) != set(required):
        raise ValueError(f"{label} requires exactly: {', '.join(sorted(required))}")


def build_preset(declaration, catalog):
    _exact_keys(
        declaration,
        {"currency", "firstRun", "upgradesDisabled", "upgrades", "unlocked", "clearedFields"},
        "declaration",
    )
    currency = declaration["currency"]
    if type(currency) is not int or currency < 0:
        raise ValueError("currency must be a nonnegative integer")
    if type(declaration["firstRun"]) is not bool or type(declaration["upgradesDisabled"]) is not bool:
        raise ValueError("firstRun and upgradesDisabled must be boolean")
    upgrades = declaration["upgrades"]
    if not isinstance(upgrades, dict):
        raise ValueError("upgrades must be an object")
    for name in ("unlocked", "clearedFields"):
        if not isinstance(declaration[name], list) or len(declaration[name]) != len(set(declaration[name])):
            raise ValueError(f"{name} must be a list without duplicates")

    unlock_rules = {entry["id"]: entry for entry in catalog["unlocks"]}
    upgrade_rules = {entry["id"]: entry for entry in catalog["upgrades"]}
    unlocked = {entry["id"] for entry in catalog["unlocks"] if entry["condition"] == "initial"}
    for item in declaration["unlocked"]:
        if item not in unlock_rules:
            raise ValueError(f"unknown unlock: {item}")
        unlocked.add(item)
    cleared = set()
    for item in declaration["clearedFields"]:
        if item not in unlock_rules or unlock_rules[item]["kind"] != "field" or item not in unlocked:
            raise ValueError(f"invalid cleared field: {item}")
        cleared.add(item)

    spending = {}
    for key, level in upgrades.items():
        parts = key.split(":")
        if len(parts) != 2 or parts[0] not in upgrade_rules:
            raise ValueError(f"invalid personal upgrade key: {key}")
        rule = upgrade_rules[parts[0]]
        if not rule["personal"] or parts[1] not in unlocked or unlock_rules[parts[1]]["kind"] != "character":
            raise ValueError(f"invalid personal upgrade owner: {key}")
        if type(level) is not int or level < 0 or level > rule["cap"]:
            raise ValueError(f"invalid personal upgrade level: {key}")
        spending[key] = rule["priceCoefficient"] * level * (level + 1) // 2

    return {
        "schemaVersion": 2,
        "currency": currency,
        "firstRun": declaration["firstRun"],
        "upgradesDisabled": declaration["upgradesDisabled"],
        "upgrades": upgrades,
        "upgradeSpending": spending,
        "unlocked": sorted(unlocked),
        "clearedFields": sorted(cleared),
        "runs": {},
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--declaration", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    catalog_path = Path(__file__).resolve().parents[2] / "Assets/Resources/Content/Meta/MetaEconomy.json"
    declaration = json.loads(args.declaration.read_text(encoding="utf-8"))
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    result = build_preset(declaration, catalog)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as output:
        json.dump(result, output, ensure_ascii=False, indent=2)
        output.write("\n")


if __name__ == "__main__":
    main()
