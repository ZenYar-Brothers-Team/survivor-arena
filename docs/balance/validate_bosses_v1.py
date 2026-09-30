"""Static checks for docs/balance/bosses-v1.json (review data, not runtime JSON)."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/bosses-v1.json"
IDS = [f"BOSS-{n:03d}" for n in range(3, 11)] + [f"MIDBOSS-{n:03d}" for n in range(3, 11)]
# Projectile counts written in card prose: encounter -> {attack id: count}.
CARD_COUNTS = {
    "BOSS-003": {"FAN3": 3}, "BOSS-004": {"FAN6": 6}, "BOSS-005": {"CROSS": 4, "FAN5": 5}, "BOSS-006": {"RING12": 12},
    "BOSS-007": {"EXPLOSIVE": 1, "EXPLOSIVE-FAN3": 3}, "BOSS-010": {"FAN9": 9, "CROSS": 4},
    "MIDBOSS-003": {"FAN5x2": 5}, "MIDBOSS-005": {"CROSS": 4}, "MIDBOSS-006": {"RING10": 10, "BOLT": 1},
    "MIDBOSS-007": {"REAR-FAN5": 5}, "MIDBOSS-008": {"FAN3": 3}, "MIDBOSS-009": {"RING12": 12, "FAN7": 7},
    "MIDBOSS-010": {"RING8": 8, "CROSS": 4},
}
DASHERS = {"BOSS-004", "BOSS-007", "BOSS-009", "MIDBOSS-004", "MIDBOSS-007", "MIDBOSS-010"}
PATTERNS = {"Single", "Fan", "Burst", "Ring", "Cross", "Spiral", "Explosive"}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def card(content, eid):
    block = re.search(r"^### " + eid + r" —.*?(?=^### (?:MID)?BOSS-\d{3} —|^### Mini-bosses|^### Travelers|\Z)", content, re.M | re.S)
    require(block is not None, f"Missing canonical card: {eid}")
    text = block.group(0)
    body = re.search(r"HP: (\d+)\. Размер / collision: ([\d.]+)\. Contact damage: (\d+)\.", text)
    kb = re.search(r"Knockback: contact ([\d.]+);(.*?)Knockback resistance (\d+)%", text)
    require(body and kb, f"Card numbers not parseable: {eid}")
    dash = re.search(r"dash contact (\d+(?:\.\d+)?)", kb[2])
    threshold = re.findall(r"после (\d+)% HP", text)
    return {"maxHealth": int(body[1]), "collisionSize": float(body[2]), "contactDamage": int(body[3]),
            "contactKnockback": float(kb[1]), "dashKnockback": float(dash[1]) if dash else None,
            "knockbackResistance": int(kb[3]) / 100, "thresholds": [int(t) / 100 for t in threshold]}


def all_attacks(entry):
    for phase in entry["phases"]:
        for attack in phase["attacks"]:
            yield attack
    dash_end = entry["dashEnd"] or {}
    for item in dash_end.get("attacks", []) + dash_end.get("belowHealthReplacement", {}).get("attacks", []):
        yield item["attack"]


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    require(data["format"] == "late-content-review-data" and not data["runtimeImportable"],
            "Review artifact must not advertise runtime compatibility")
    content = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    ref = data["reference"]
    roles = ref["damageRoleOfContact"]
    encounters = {e["id"]: e for e in data["encounters"]}
    require(list(encounters) == IDS, "Expected BOSS-003…010 and MIDBOSS-003…010 once each, in order")
    users = {}
    for ext in data["runtimeExtensions"] + data["newAttackFamilies"]:
        for eid in ext["users"]:
            users.setdefault(eid, set()).add(ext["id"])

    for eid, e in encounters.items():
        n = int(eid[-3:])
        final = eid.startswith("BOSS")
        c = card(content, eid)
        for key in ("maxHealth", "collisionSize", "contactDamage", "contactKnockback", "knockbackResistance"):
            require(math.isclose(e[key], c[key]), f"{eid}.{key} differs from the card")
        require((e["dashKnockback"] is None) == (c["dashKnockback"] is None) and
                (c["dashKnockback"] is None or math.isclose(e["dashKnockback"], c["dashKnockback"])), f"{eid}: dash knockback vs card")
        require((eid in DASHERS) == (e["movement"]["kind"] == "TelegraphedDash"), f"{eid}: dash movement vs card")
        require(e["field"] == f"FIELD-{n:03d}" and e["hook"] == ("FinalBoss" if final else "MidBoss"), f"{eid}: field/hook")
        require(e["experienceReward"] == (150 + 30 * (n - 1) if final else 60 + 15 * (n - 1)), f"{eid}: XP formula")
        require(e["spawnSeconds"] == (810 if final else 450) and not e["deathEndsRun"], f"{eid}: spawn/victory policy")
        require(0 < e["movementSpeed"] < ref["playerSpeed"] / 2, f"{eid}: boss pursuit must stay well below player speed")

        # Phases: first starts at full HP, thresholds strictly decrease and match the card.
        below = [p["belowHealth"] for p in e["phases"]]
        require(below[0] == 1.0 and all(a > b for a, b in zip(below, below[1:])), f"{eid}: phase thresholds order")
        require(below[1:] == c["thresholds"], f"{eid}: phase thresholds {below[1:]} vs card {c['thresholds']}")
        require(all(p["thresholdComparison"] == "strictly-less" for p in e["phases"]), f"{eid}: threshold comparison")

        # Projectile attacks: damage from the role formula, readable geometry, card counts.
        counts = CARD_COUNTS.get(eid, {})
        attacks = list(all_attacks(e))
        shots = [a for a in attacks if a["kind"] == "Projectile"]
        for a in shots:
            require(a["pattern"] in PATTERNS, f"{eid}.{a['id']}: pattern")
            require(a["damage"] == int(e["contactDamage"] * roles[a["role"]] + 0.5), f"{eid}.{a['id']}: damage formula")
            require(a["projectileSpeed"] > 0 and a["projectileLifetimeSeconds"] > 0 and a["projectileRadius"] > 0
                    and a["knockback"] > 0 and a["knockbackSeconds"] > 0, f"{eid}.{a['id']}: geometry/controls")
            if a["id"] in counts:
                require(a["projectileCount"] == counts[a["id"]], f"{eid}.{a['id']}: projectile count vs card")
            if a["pattern"] == "Cross":
                require(a["projectileCount"] == 4, f"{eid}.{a['id']}: cross is four projectiles")
            if a["pattern"] == "Explosive":
                require(a["explosionRadius"] > 0, f"{eid}.{a['id']}: explosion radius")
            delays = [f["delaySeconds"] for f in a["followUps"]]
            require(all(d > 0 for d in delays) and delays == sorted(delays), f"{eid}.{a['id']}: follow-up delays")
            require(0 < a["windupMovementMultiplier"] <= 1, f"{eid}.{a['id']}: windup movement multiplier")
        require(all(k in {a["id"] for a in shots} for k in counts), f"{eid}: card attack missing")

        # Signature attacks: telegraphed, escapable at base speed, never a one-shot of the base 100 HP.
        speed = ref["playerSpeed"]
        base_hp = ref["playerBaseHealth"]
        for a in attacks:
            if a["kind"] == "Zone":
                require(a["damage"] == int(e["contactDamage"] * roles["zone"] + 0.5), f"{eid}.{a['id']}: zone damage formula")
                require(a["damage"] < base_hp and a["fillSeconds"] >= 0.5 and a["radius"] > 0 and a["count"] >= 1,
                        f"{eid}.{a['id']}: zone must be readable and not lethal from full base HP")
                if a["placement"] == "AroundSelf":
                    require(a["radius"] - e["collisionSize"] / 2 <= speed * a["fillSeconds"],
                            f"{eid}.{a['id']}: player touching the boss must be able to leave the zone")
                elif a["placement"] == "SafeCircles":
                    require(a["count"] >= 2 and a["radius"] >= 1.5 and a["minSpacing"] >= 2 * a["radius"]
                            and a["scatterRadius"] - a["radius"] <= speed * a["fillSeconds"] * 0.5,
                            f"{eid}.{a['id']}: a safe circle must be reachable in half the fill time")
                else:
                    require(a["radius"] <= speed * a["fillSeconds"], f"{eid}.{a['id']}: zone must be escapable from its center")
                if a["placement"] in ("AroundPlayer", "SafeCircles"):
                    require(a["scatterRadius"] > 0 and a["minSpacing"] > 0, f"{eid}.{a['id']}: scatter geometry")
                if a["placement"] == "Trail":
                    require(a["count"] > 1 and a["intervalSeconds"] > 0, f"{eid}.{a['id']}: trail timing")
                require((a["lingerSeconds"] > 0) == (a["lingerDamagePerSecond"] > 0), f"{eid}.{a['id']}: burning ground")
                if a["lingerSeconds"]:
                    require(a["lingerDamagePerSecond"] == int(e["contactDamage"] * roles["burn"] + 0.5),
                            f"{eid}.{a['id']}: burn formula")
                require(a["slowFraction"] == 0 or (0 < a["slowFraction"] < 1 and a["slowSeconds"] > 0), f"{eid}.{a['id']}: slow")
            elif a["kind"] == "Beam":
                require(a["damage"] == int(e["contactDamage"] * roles["beam"] + 0.5) and a["damage"] < base_hp,
                        f"{eid}.{a['id']}: beam damage formula / not lethal")
                require(a["length"] >= 20 and a["telegraphSeconds"] >= 0.8 and a["activeSeconds"] > 0
                        and a["width"] / 2 <= speed * a["telegraphSeconds"], f"{eid}.{a['id']}: full-screen, readable beam")
                angles = a["anglesDegrees"]
                require(len(angles) == len(set(angles)) and all(abs(x) <= 360 for x in angles), f"{eid}.{a['id']}: beam angles")
            elif a["kind"] == "Summon":
                ctx = re.search(r"^### " + a["enemyId"] + r" —.*?Контексты: FIELD-(\d{3})…(\d{3})", content, re.M | re.S)
                require(ctx is not None and int(ctx[1]) <= n <= int(ctx[2]),
                        f"{eid}.{a['id']}: {a['enemyId']} not in FIELD-{n:03d} context")
                require(0 < a["count"] <= a["maxAlive"] and a["telegraphSeconds"] > 0 and a["spawnDistance"] >= 4,
                        f"{eid}.{a['id']}: summon limits")
            else:
                require(a["kind"] == "Projectile", f"{eid}.{a['id']}: unknown attack kind")
        for p in e["phases"]:
            for a in p["attacks"]:
                wind = a.get("telegraphSeconds", a.get("fillSeconds", 0))
                require(a["cooldownSeconds"] > 0 and wind > 0, f"{eid}.{a['id']}: sequence item needs a cooldown and a warning")
                if a["kind"] == "Projectile":
                    require(a["cooldownSeconds"] > a["telegraphSeconds"], f"{eid}.{a['id']}: wind-up must fit the cooldown")
                if a["kind"] == "Zone":
                    require(a["intervalSeconds"] * (a["count"] - 1) + a["fillSeconds"] <= a["cooldownSeconds"],
                            f"{eid}.{a['id']}: zones must land before the next item starts")
        require(bool(attacks), f"{eid}: no attacks")
        require(any(a["kind"] != "Projectile" for a in attacks), f"{eid}: every boss needs a signature attack")

        # Held distance must be inside the teleport trigger and within projectile reach.
        move = e["movement"]
        if move["kind"] in ("KeepDistance", "Orbit"):
            far = move["preferredDistance"] + move["distanceTolerance"]
            if e["teleport"]:
                require(far <= e["teleport"]["farDistance"], f"{eid}: held distance would trigger the teleport")
            for a in shots:
                require(a["projectileSpeed"] * a["projectileLifetimeSeconds"] >= far + 1, f"{eid}.{a['id']}: reach")
        for m in [move] + [p["movementOverride"] for p in e["phases"] if p["movementOverride"]]:
            if m["kind"] == "TelegraphedDash":
                require(m["dashTelegraphSeconds"] > 0 and m["dashDurationSeconds"] > 0 and m["dashCooldownSeconds"] > 0
                        and m["dashCount"] >= 1 and (m["dashCount"] == 1) == (m["followUpTelegraphSeconds"] == 0),
                        f"{eid}: dash timings")
        if e["teleport"]:
            expected_impact = (20 + 2 * (n - 2)) / 1.5  # DECISION-0080; JSON stores six decimals.
            require(abs(e["teleport"]["impactDamage"] - expected_impact) < 1e-6,
                    f"{eid}: teleport impact formula")
            t = e["teleport"]
            require(t["landingDistance"] + ref["playerSpeed"] * t["telegraphSeconds"] < t["impactRadius"],
                    f"{eid}: teleport slam must stay unavoidable at base speed (DECISION-0059)")

        # Every extension/family the data needs is declared, and nothing declared is unused.
        needed = set()
        if any(a["followUps"] for a in shots):
            needed.add("E1")
        if any(a["pattern"] == "Explosive" and a["projectileCount"] > 1 for a in shots):
            needed.add("E2")
        if any(p["movementOverride"] for p in e["phases"]):
            needed.add("E3")
        if e["dashEnd"]:
            needed.add("E4")
        if e["holdRangedDuringDash"]:
            needed.add("E5")
        if any(a["windupMovementMultiplier"] != 1 for a in shots):
            needed.add("E6")
        for family, kind in (("F1", "Zone"), ("F2", "Beam"), ("F3", "Summon")):
            if any(a["kind"] == kind for a in attacks):
                needed.add(family)
        require(needed == users.get(eid, set()), f"{eid}: runtime extensions {sorted(needed)} vs declared {sorted(users.get(eid, set()))}")

    print(f"PASS: {len(encounters)} encounters validated against Content Design cards")


if __name__ == "__main__":
    main()
