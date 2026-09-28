"""Static checks for docs/balance/travelers-v1.json (review data for TRAVELER-003/004/006…010, not runtime JSON)."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/travelers-v1.json"
BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"
CONTENT_DESIGN = ROOT / "docs/Content_design.md"

NEW_IDS = ["TRAVELER-003", "TRAVELER-004", "TRAVELER-006", "TRAVELER-007", "TRAVELER-008", "TRAVELER-009", "TRAVELER-010"]
EXPECTED_ROLES = {"Offensive": 4, "Wanderer": 3, "Protector": 3}
# One progression tier (user, 2026-09-28): HP and contact damage stay within ±25% of the same-role FIELD-001 peer.
PEER_BAND = 0.25
SPEED_RANGE = (0.6, 1.2)
MOVEMENT_KINDS = {"Seek", "Zigzag", "TelegraphedDash"}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def card_role(content, traveler_id):
    block = re.search(r"^#### " + traveler_id + r" —(.*?)(?=^#### |^### |\Z)", content, re.M | re.S)
    require(block is not None, f"{traveler_id}: card not found")
    line = re.search(r"^Роль: (.*)$", block[1], re.M)
    require(line is not None, f"{traveler_id}: role line not found")
    text = line[1]
    if "боевой" in text:
        return "Offensive"
    if "неагрессивный" in text:
        return "Wanderer"
    require("защитник" in text, f"{traveler_id}: unknown card role '{text}'")
    return "Protector"


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    peers = {t["role"]: t for t in json.loads(BASELINE.read_text(encoding="utf-8"))["travelers"]}
    content = CONTENT_DESIGN.read_text(encoding="utf-8")

    require(data["format"] == "late-content-review-data" and data["revision"] == "travelers-v1", "format/revision")
    travelers = data["travelers"]
    require([t["id"] for t in travelers] == NEW_IDS, "TRAVELER-003/004/006…010 in order")

    roles = {role: 1 for role in EXPECTED_ROLES}  # one baseline peer per role
    markers = {peer["marker"] for peer in peers.values()}
    for t in travelers:
        tid, role = t["id"], t["role"]
        require(role == card_role(content, tid), f"{tid}: role differs from card")
        roles[role] += 1
        require(t["marker"] not in markers, f"{tid}: marker '{t['marker']}' repeats")
        markers.add(t["marker"])

        peer = peers[role]
        low, high = 1 - PEER_BAND, 1 + PEER_BAND
        require(low <= t["maxHealth"] / peer["maxHealth"] <= high,
                f"{tid}: HP {t['maxHealth']} outside ±25% of {peer['id']} ({peer['maxHealth']})")
        if peer["contactDamage"] > 0:
            require(low <= t["contactDamage"] / peer["contactDamage"] <= high, f"{tid}: contact damage outside peer band")
        else:
            require(t["contactDamage"] == 0 and t["attack"] is None, f"{tid}: {role} must not attack")
        require(SPEED_RANGE[0] <= t["movementSpeed"] <= SPEED_RANGE[1], f"{tid}: speed outside {SPEED_RANGE}")
        require(0 <= t["knockbackResistance"] <= 0.65, f"{tid}: knockback resistance")
        require(1.0 <= t["collisionSize"] <= 1.5, f"{tid}: collision size")

        reward = data["roleRewards"][role]
        require(t["experienceReward"] == reward["experienceReward"] == peer["experienceReward"], f"{tid}: XP differs from role")
        require(t["presenceSeconds"] == reward["presenceSeconds"] == peer["presenceSeconds"], f"{tid}: presence differs from role")

        require(t["movement"]["kind"] in MOVEMENT_KINDS, f"{tid}: movement kind")
        if t["movement"]["kind"] == "TelegraphedDash":
            require(t["dashContactKnockback"] is not None, f"{tid}: dash needs its own contact knockback")
        support = t["support"]
        expected_support = {"TRAVELER-007": "Shield", "TRAVELER-009": "Aura"}.get(tid, "None")
        require(support["kind"] == expected_support, f"{tid}: support kind")
        if support["kind"] == "Aura":
            require(0 < support["reduction"] <= peers["Protector"]["support"]["reduction"], f"{tid}: aura reduction must be small")
            require(0 < support["resistance"] <= 1, f"{tid}: aura resistance")
        if support["kind"] == "Shield":
            require(support["shieldHp"] > 0 and support["shieldSeconds"] > 0 and support["cooldownSeconds"] > support["shieldSeconds"],
                    f"{tid}: shield must expire before the next cast")
        if t["attack"] is not None:
            attack = t["attack"]
            require(attack["pattern"] == "Cross" and attack["projectileCount"] == 4, f"{tid}: card says a cross of 4")
            require(attack["cooldownSeconds"] == 2.6, f"{tid}: card cooldown 2.6 s")
            require(attack["damage"] < t["contactDamage"], f"{tid}: projectile hits weaker than contact")

    require(roles == EXPECTED_ROLES, f"role counts {roles} differ from {EXPECTED_ROLES}")
    print(f"PASS: {len(travelers)} new travelers, roles {roles}, all within ±{int(PEER_BAND * 100)}% of role peers")


if __name__ == "__main__":
    main()
