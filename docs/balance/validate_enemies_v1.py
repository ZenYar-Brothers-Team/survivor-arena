"""Static checks for docs/balance/enemies-v1.json (review data, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/enemies-v1.json"
IDS = ["ENEMY-006"] + [f"ENEMY-{n:03d}" for n in range(8, 21)]
# Attack numbers written in card prose: (pattern, damage, cooldown, count, spread, burst interval, explosion radius).
CARD_ATTACKS = {
    "ENEMY-006": ("Fan", 10, 3.0, 3, 35, 0, 0), "ENEMY-010": ("Burst", 9, 3.2, 3, 0, 0.15, 0),
    "ENEMY-011": ("Ring", 9, 3.8, 8, 0, 0, 0), "ENEMY-012": ("Single", 18, 3.0, 1, 0, 0, 0),
    "ENEMY-014": ("Explosive", 24, 4.0, 1, 0, 0, 1.0), "ENEMY-015": ("Cross", 13, 3.3, 4, 0, 0, 0),
    "ENEMY-018": ("Spiral", 12, 3.4, 10, 0, 0, 0), "ENEMY-019": ("Fan", 15, 3.5, 5, None, 0, 0),
}
CARD_MOVEMENT = {"ENEMY-008": "Orbit", "ENEMY-009": "Seek", "ENEMY-013": "ApproachRetreat", "ENEMY-016": "TelegraphedDash",
                 "ENEMY-017": "Zigzag", "ENEMY-019": "Seek", "ENEMY-020": "Seek"}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def card_numbers(content, enemy_id):
    block = re.search(r"^### " + enemy_id + r" —.*?(?=^### ENEMY-\d{3} —|\Z)", content, re.M | re.S)
    require(block is not None, f"Missing canonical card: {enemy_id}")
    text = block.group(0)
    size = re.search(r"Размер / collision: ([\d.]+)\. HP: (\d+)\. Speed: ([\d.]+)\. Contact damage: (\d+)\.", text)
    kb = re.search(r"Knockback: contact ([\d.]+);.*?Knockback resistance (\d+)%", text)
    xp = re.search(r"XP reward: (\d+)\.", text)
    require(size and kb and xp, f"Card numbers not parseable: {enemy_id}")
    return {"collisionSize": float(size[1]), "maxHealth": int(size[2]), "cardSpeed": float(size[3]),
            "contactDamage": int(size[4]), "contactKnockback": float(kb[1]), "knockbackResistance": int(kb[2]) / 100,
            "experienceReward": int(xp[1])}


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"],
            "Review artifact must not advertise runtime compatibility")
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    enemies = {entry["id"]: entry for entry in data["enemies"]}
    require(list(enemies) == IDS, "Expected ENEMY-006 and ENEMY-008…020 once each, in order")
    changes = {entry["id"]: entry for entry in data["cardChanges"]}
    player_speed = data["reference"]["playerSpeed"]

    for enemy_id, entry in enemies.items():
        card = card_numbers(content, enemy_id)
        for key in ("collisionSize", "maxHealth", "contactDamage", "contactKnockback", "knockbackResistance", "experienceReward"):
            require(math.isclose(entry[key], card[key]), f"{enemy_id}.{key} differs from the card without a listed change")
        change = changes[enemy_id]
        require(set(change["before"]) == {"movementSpeed"} and math.isclose(change["before"]["movementSpeed"], card["cardSpeed"]),
                f"{enemy_id}: change table must start from the card speed")
        require(math.isclose(change["proposed"]["movementSpeed"], entry["movementSpeed"]), f"{enemy_id}: change table out of sync")
        require(0 < entry["movementSpeed"] < player_speed, f"{enemy_id}: ordinary pursuit must stay slower than the player")
        require(entry["contactDamageIntervalSeconds"] > 0 and entry["knockbackSeconds"] > 0, f"{enemy_id}: timers")
        require(0 < entry["potionDropChance"] <= 1, f"{enemy_id}: potion chance")
        movement = entry["movement"]
        if enemy_id in CARD_MOVEMENT:
            require(movement["kind"] == CARD_MOVEMENT[enemy_id], f"{enemy_id}: movement family changed")
        attack = entry["attack"]
        require((attack is not None) == (enemy_id in CARD_ATTACKS), f"{enemy_id}: ranged attack presence differs from the card")
        if attack:
            pattern, damage, cooldown, count, spread, burst, explosion = CARD_ATTACKS[enemy_id]
            require(attack["pattern"] == pattern and attack["damage"] == damage and math.isclose(attack["cooldownSeconds"], cooldown)
                    and attack["projectileCount"] == count, f"{enemy_id}: card attack numbers changed")
            require(spread is None or attack["spreadDegrees"] == spread, f"{enemy_id}: spread changed")
            require(math.isclose(attack["burstIntervalSeconds"], burst) and math.isclose(attack["explosionRadius"], explosion),
                    f"{enemy_id}: burst/explosion changed")
            require(attack["telegraphSeconds"] > 0 and attack["projectileRadius"] > 0 and attack["knockbackSeconds"] > 0,
                    f"{enemy_id}: readable wind-up and geometry required")
            require(math.isclose(attack["initialDelaySeconds"], attack["cooldownSeconds"]), f"{enemy_id}: first shot after a full cooldown")
            reach = attack["projectileSpeed"] * attack["projectileLifetimeSeconds"]
            if movement["kind"] == "KeepDistance":
                require(reach >= movement["preferredDistance"] + movement["distanceTolerance"] + 1,
                        f"{enemy_id}: projectiles must reach the player from the held distance")
            if pattern == "Spiral":
                require(attack["rotationStepDegrees"] > 0, f"{enemy_id}: spiral needs a rotation step")
        if movement["kind"] == "TelegraphedDash":
            require(movement["dashTelegraphSeconds"] > 0 and movement["dashDurationSeconds"] > 0 and movement["dashKnockback"] > 0,
                    f"{enemy_id}: dash timings")
    print(f"PASS: {len(enemies)} enemies validated against Content Design cards")


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")  # non-ASCII report text; Windows consoles default to cp1251
    main()
