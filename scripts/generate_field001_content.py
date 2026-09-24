"""Generate FIELD-001 production content JSON from the approved balance baseline.

Source of numbers: docs/balance/field001-baseline-v1.json (Approved, DECISION-0053).
The baseline is a review format and is never loaded by Unity; this script maps it
losslessly into the runtime DTO layout. Run from the repository root:

    python scripts/generate_field001_content.py            # write files
    python scripts/generate_field001_content.py --check    # fail if files are stale

Only packets whose output is listed in TARGETS are generated.
"""
import argparse
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"


def load_baseline():
    data = json.loads(BASELINE.read_text(encoding="utf-8"))
    if data.get("approval") != "Approved":
        raise SystemExit("Baseline is not Approved; production content cannot be generated.")
    return data


def controls(knockback, kb_seconds, slow=0.0, slow_seconds=0.0):
    result = {}
    if knockback > 0:
        result["knockbackDistance"] = knockback
        result["knockbackSeconds"] = kb_seconds
    if slow > 0:
        result["slowFraction"] = slow
        result["slowSeconds"] = slow_seconds
    return result


def wave(effects, ctrl, delay=0.0, damage_multiplier=1.0):
    entry = {"delaySeconds": delay, "rotationDegrees": 0, "damageMultiplier": damage_multiplier, "effects": effects}
    if ctrl:
        entry["controls"] = ctrl
    return entry


def pierce(max_hit_targets):
    return 0 if max_hit_targets is None else max_hit_targets - 1


def skill_levels(skill, kb_seconds, seed_base):
    sid = skill["id"]
    shared = skill["shared"]
    number = int(sid.split("-")[1])
    levels = []
    for row in skill["levels"]:
        level = {
            "baseDamage": row.get("damage", row.get("impactDamage")),
            "cooldownSeconds": shared.get("cooldownSeconds"),
            "actionSpeedBonus": row.get("actionSpeedBonus", 0),
            "rotationPerActivationDegrees": 0,
        }
        kb = row.get("knockback", shared.get("knockback", 0))
        if sid == "SKILL-001":
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"])
            level["waves"] = [wave([{
                "kind": "ProjectileBurst", "projectileCount": row["count"],
                "layout": "Single" if row["count"] == 1 else "Fan", "spreadDegrees": 20,
                "pierceCount": 0, "speed": row["speed"], "lifetimeSeconds": row["lifetimeSeconds"],
                "collisionRadius": row["collisionRadius"],
                "behavior": {"ricochetCount": row["ricochetCount"], "ricochetRange": shared["ricochetRange"],
                             "ricochetRetention": row["ricochetRetention"],
                             "repeatRicochetTargets": shared["repeatRicochetTargets"],
                             "distinctNearestTargets": shared["distinctNearestTargets"]}}], controls(kb, kb_seconds))]
        elif sid in ("SKILL-002", "SKILL-013"):
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"])
            speed = row.get("speed", shared.get("speed"))
            ctrl = controls(kb, kb_seconds, row.get("slowFraction", 0), row.get("slowSeconds", 0))
            level["waves"] = [wave([{
                "kind": "ProjectileBurst", "projectileCount": row["count"], "layout": shared["layout"],
                "spreadDegrees": row["spreadDegrees"], "pierceCount": pierce(row["maxHitTargets"]),
                "speed": speed, "lifetimeSeconds": row["lifetimeSeconds"],
                "collisionRadius": row["collisionRadius"]}], ctrl)]
        elif sid == "SKILL-003":
            level.update(targetingMode="Self", cooldownSeconds=0.2)
            level["waves"] = [wave([{
                "kind": "Orbit", "persistent": True, "bladeCount": row["count"], "radius": row["orbitRadius"],
                "angularSpeedDegrees": row["angularSpeedDegrees"], "durationSeconds": 0,
                "hitCooldownSeconds": shared["perBladeTargetHitCooldownSeconds"],
                "bladeHitboxRadius": row["bladeHitboxRadius"]}], controls(kb, kb_seconds))]
        elif sid == "SKILL-004":
            level.update(targetingMode="Self")
            expansion = shared["waveExpansionSeconds"]
            waves = [wave([{"kind": "Area", "radius": row["radius"], "expansionSeconds": expansion}],
                          controls(kb, kb_seconds))]
            if row["waveCount"] >= 2:
                waves.append(wave([{"kind": "Area", "radius": row["radius"] * row["secondRadiusMultiplier"],
                                    "expansionSeconds": expansion}],
                                  controls(kb * row["secondKnockbackMultiplier"], kb_seconds),
                                  delay=row["secondDelaySeconds"], damage_multiplier=row["secondDamageMultiplier"]))
            level["waves"] = waves
        elif sid == "SKILL-005":
            level.update(targetingMode="MovementDirection", initialDirectionDegrees=shared["initialDirectionDegrees"])
            effect = {"kind": "ProjectileBurst", "projectileCount": 1, "layout": "Single", "spreadDegrees": 0,
                      "pierceCount": pierce(row["maxHitTargets"]), "speed": row["speed"],
                      "lifetimeSeconds": row["lifetimeSeconds"], "collisionRadius": row["collisionRadius"]}
            if row["maxHitTargets"] is None:
                effect["behavior"] = {"unlimitedPierce": True}
            level["waves"] = [wave([effect], controls(kb, kb_seconds))]
        elif sid == "SKILL-006":
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"])
            level["waves"] = [wave([{
                "kind": "Boomerang", "projectileCount": row["count"], "spreadDegrees": row["spreadDegrees"],
                "speed": row["speed"], "range": row["range"], "collisionRadius": row["collisionRadius"],
                "returnDamageMultiplier": row["returnDamageMultiplier"],
                "hitCooldownSeconds": shared["hitCooldownSeconds"],
                "returnKnockbackMultiplier": row["returnKnockbackMultiplier"],
                "lifetimeSeconds": shared["lifetimeSeconds"]}], controls(kb, kb_seconds))]
        elif sid == "SKILL-007":
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"])
            level["waves"] = [wave([{
                "kind": "Chain", "targetCount": row["targetCount"], "jumpRange": row["jumpRange"],
                "damageRetentionPerJump": row["damageRetentionPerJump"]}], controls(kb, kb_seconds))]
        elif sid == "SKILL-010":
            level.update(targetingMode="RandomEnemy", targetingRadius=shared["targetingRadius"],
                         randomSeed=seed_base + number)
            waves = []
            for i in range(row["strikeCount"]):
                third = i == 2
                radius = row["radius"] * (row["thirdRadiusMultiplier"] if third else 1)
                strike_kb = kb * (row["thirdKnockbackMultiplier"] if third else 1)
                waves.append(wave([{"kind": "Strike", "radius": radius, "telegraphSeconds": row["telegraphSeconds"]}],
                                  controls(strike_kb, kb_seconds), delay=round(shared["strikeSpacingSeconds"] * i, 6)))
            level["waves"] = waves
        elif sid == "SKILL-014":
            level.update(targetingMode="Self", randomSeed=seed_base + number)
            level["waves"] = [wave([{
                "kind": "ProjectileBurst", "projectileCount": row["count"], "layout": shared["layout"],
                "spreadDegrees": 0, "pierceCount": pierce(row["maxHitTargets"]), "speed": shared["speed"],
                "lifetimeSeconds": shared["lifetimeSeconds"], "collisionRadius": row["collisionRadius"],
                "impactAreaRadius": row["explosionRadius"],
                "behavior": {"explosionDamageMultiplier": row["explosionDamage"] / row["impactDamage"],
                             "explodeOnExpiry": shared["explodeOnExpiry"],
                             "explosionKnockbackMultiplier": row["explosionKnockback"] / row["impactKnockback"]}}],
                controls(row["impactKnockback"], kb_seconds))]
        else:
            raise SystemExit(f"No runtime mapping for {sid}")
        if level["cooldownSeconds"] is None:
            raise SystemExit(f"{sid} lacks cooldownSeconds")
        levels.append(level)
    return levels


SKILL_VISUALS = {  # projectile/orbit sprite per skill; procedural world effects have none
    "SKILL-001": "SKILL-001-VISUAL-PROJECTILE", "SKILL-002": "SKILL-002-VISUAL-PROJECTILE",
    "SKILL-003": "SKILL-003-VISUAL-PROJECTILE", "SKILL-005": "SKILL-005-VISUAL-PROJECTILE",
    "SKILL-006": "SKILL-006-VISUAL-PROJECTILE", "SKILL-013": "SKILL-013-VISUAL-PROJECTILE",
    "SKILL-014": "SKILL-014-VISUAL-PROJECTILE",
}


def active_skills(baseline):
    kb_seconds = baseline["controls"]["nonzeroKnockbackSeconds"]
    seed_base = baseline["randomness"]["referenceSeeds"]["skillRandomBase"]
    result = []
    for skill in baseline["skills"]:
        entry = {"id": skill["id"], "displayName": skill["name"], "iconVisualId": f"{skill['id']}-VISUAL-ICON"}
        if skill["id"] in SKILL_VISUALS:
            entry["visualId"] = SKILL_VISUALS[skill["id"]]
        entry["levels"] = skill_levels(skill, kb_seconds, seed_base)
        result.append(entry)
    return result


PASSIVE_CHANNELS = {  # baseline review field -> runtime CharacterStatModifierData field
    "maxHealthBonus": "maxHealthMultiplierBonus",
    "regenerationHpPerSecond": "healthRegenerationPerSecondBonus",
    "potionDropChanceBonus": "potionDropMultiplierBonus",
    "movementSpeedBonus": "movementSpeedMultiplierBonus",
    "activeDamageBonus": "activeSkillDamageMultiplierBonus",
    "actionSpeedBonus": "actionSpeedBonus",
    "pickupRadiusBonus": "pickupRadiusMultiplierBonus",
    "incomingDamageReduction": "incomingDamageReductionBonus",
    "healthRestorationBonus": "healthRestorationMultiplierBonus",
    "knockbackResistanceBonus": "knockbackResistanceBonus",
    "outgoingKnockbackBonus": "outgoingKnockbackBonus",
    "effectSizeBonus": "effectSizeMultiplierBonus",
}


def content_design_names(prefix):
    """Card titles from Content Design, e.g. '#### PASSIVE-001 — Крепкое сердце'."""
    import re
    text = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    return {m.group(1): m.group(2).strip() for m in re.finditer(rf"^#+ ({prefix}-\d{{3}}) — (.+)$", text, re.M)}


def passives(baseline):
    names = content_design_names("PASSIVE")
    result = []
    for passive in baseline["passives"]:
        levels = []
        for row in passive["levels"]:
            level = {}
            for key, value in row.items():
                if key == "level":
                    continue
                if key not in PASSIVE_CHANNELS:
                    raise SystemExit(f"{passive['id']}: no runtime channel for {key}")
                level[PASSIVE_CHANNELS[key]] = value
            levels.append(level)
        result.append({"id": passive["id"], "displayName": names[passive["id"]],
                       "iconVisualId": f"{passive['id']}-VISUAL-ICON", "levels": levels})
    return result


CHARACTER_BASELINE_ID = "CHARACTER-BASELINE-001"


def characters(baseline):
    """CHAR-001 only: draft weights for the ten startup skills (late weights stay baseline metadata)."""
    character = baseline["character"]
    startup = set(baseline["initialRoster"]["actives"])
    names = content_design_names("CHAR")
    weights = [{"skillId": skill, "weight": weight}
               for skill, weight in character["skillDraftWeights"].items() if skill in startup]
    return [{
        "id": character["id"], "displayName": names[character["id"]],
        "initiallyUnlocked": True, "startingActiveSkillId": character["startingSkill"],
        "visualId": f"{character['id']}-VISUAL-BODY", "motionProfileId": f"{character['id']}-MOTION",
        "baseStats": character["stats"], "draftWeights": weights,
        "presentation": {"role": " · ".join(character["highlights"]), "baselineId": CHARACTER_BASELINE_ID,
                         "cropId": f"{character['id']}-VISUAL-PORTRAIT", "iconId": f"{character['id']}-VISUAL-ICON",
                         "highlights": []},
    }]


def character_baseline(baseline):
    return {"id": CHARACTER_BASELINE_ID, "baseStats": baseline["character"]["stats"]}


ENEMY_VISUALS = {  # approved/imported bodies; other startup bodies remain an explicit art gate (DECISION-0054)
    "ENEMY-001": ("ENEMY-001-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-002": ("ENEMY-002-VISUAL-BODY", "ENEMY-002-MOTION"),
}
ENEMY_PROJECTILE_VISUALS = {"ENEMY-004": "ENEMY-004-VISUAL-PROJECTILE", "ENEMY-005": "ENEMY-005-VISUAL-PROJECTILE"}
CADENCES = {"windup-start-to-windup-start": "WindupStartToStart"}


def enemies(baseline):
    result = []
    for enemy in baseline["enemies"]:
        kb_seconds = enemy["knockbackSeconds"]
        movement = dict(enemy["movement"])
        entry = {
            "id": enemy["id"], "knockbackResistance": enemy["knockbackResistance"], "maxHealth": enemy["maxHealth"],
            "collisionSize": enemy["collisionSize"], "movementSpeed": enemy["movementSpeed"],
            "contactDamage": enemy["contactDamage"], "contactDamageInterval": enemy["contactDamageIntervalSeconds"],
            "experienceReward": enemy["experienceReward"],
            "contactControls": {"knockbackDistance": enemy["contactKnockback"], "knockbackSeconds": kb_seconds},
        }
        if enemy["id"] in ENEMY_VISUALS:
            entry["visualId"], entry["motionProfileId"] = ENEMY_VISUALS[enemy["id"]]
        kind = movement["kind"]
        runtime_movement = {"kind": kind}
        if kind in ("KeepDistance", "DistanceReposition"):
            runtime_movement.update(preferredDistance=movement["preferredDistance"], distanceTolerance=movement["distanceTolerance"])
        if kind == "DistanceReposition":
            if movement["cycleSeconds"] != movement["holdingSeconds"] + movement["repositionSeconds"] or not movement["alternateLateralDirection"]:
                raise SystemExit(f"{enemy['id']}: unsupported reposition cycle")
            runtime_movement.update(lateralStrength=movement["lateralStrength"], cycleSeconds=movement["cycleSeconds"],
                                    repositionSeconds=movement["repositionSeconds"])
        if kind == "TelegraphedDash":
            if movement["direction"] != "snapshot-at-telegraph-start":
                raise SystemExit(f"{enemy['id']}: unsupported dash direction policy")
            runtime_movement.update(dashTelegraphSeconds=movement["dashTelegraphSeconds"],
                                    dashDurationSeconds=movement["dashDurationSeconds"],
                                    dashCooldownSeconds=movement["dashCooldownSeconds"],
                                    dashSpeedMultiplier=movement["dashSpeedMultiplier"])
            entry["dashContactControls"] = {"knockbackDistance": movement["dashKnockback"], "knockbackSeconds": kb_seconds}
        entry["movement"] = runtime_movement
        attack = enemy["attack"]
        if attack:
            if attack["initialDelaySeconds"] != attack["cooldownSeconds"] or attack["aimSnapshot"] != "windup-start":
                raise SystemExit(f"{enemy['id']}: attack timing not expressible by the runtime cadence")
            entry["attack"] = {
                "pattern": attack["pattern"], "damage": attack["damage"], "cooldownSeconds": attack["cooldownSeconds"],
                "projectileSpeed": attack["projectileSpeed"], "projectileLifetimeSeconds": attack["projectileLifetimeSeconds"],
                "projectileCount": attack["projectileCount"], "projectileRadius": attack["projectileRadius"],
                "telegraphSeconds": attack["telegraphSeconds"], "cadence": CADENCES[attack["cadence"]],
                "controls": {"knockbackDistance": attack["knockback"], "knockbackSeconds": attack["knockbackSeconds"]},
                "projectileVisualId": ENEMY_PROJECTILE_VISUALS[enemy["id"]],
            }
        result.append(entry)
    return result


# In-game accepted presentation scales (docs/playtests/2026-09-22_visual-acceptance.md) win over the review
# format's neutral 1.0; gameplay values below come from the baseline unchanged (DECISION-0054 section 6).
ACCEPTED_PICKUP_VISUAL_SCALES = {"Potion": 0.68, "Book": 0.7, "experience": 0.62}


def pickups(baseline):
    data = baseline["pickups"]
    if data["enemyChanceOverrides"] or data["fieldChanceOverrides"] or data["eligiblePotionSources"] != "ordinary-only":
        raise SystemExit("pickup overrides/eligibility need a runtime mapping review")
    seeds = baseline["randomness"]["referenceSeeds"]
    definitions = data["definitions"]
    by_kind = {d["kind"]: d for d in definitions}
    return {
        "potionId": by_kind["Potion"]["id"], "bookId": by_kind["Book"]["id"],
        "baseChance": data["basePotionChance"], "seed": seeds["potion"],
        "placementSkin": data["placementSkin"], "feedbackSeconds": data["feedbackSeconds"],
        "experienceVisualId": data["experienceVisualId"],
        "experienceVisualScale": ACCEPTED_PICKUP_VISUAL_SCALES["experience"],
        "dropScatterRadius": data["dropScatterRadius"], "dropScatterSeed": seeds["dropScatter"],
        "enemyChances": {}, "fieldChances": {},
        "pickups": [{"id": d["id"], "kind": d["kind"], "healing": d["healing"], "contactRadius": d["contactRadius"],
                     "lifetimeSeconds": d["lifetimeSeconds"], "marker": d["marker"], "color": d["color"],
                     "markerSize": d["markerSize"], "visualId": d["visualId"],
                     "visualScale": ACCEPTED_PICKUP_VISUAL_SCALES[d["kind"]]} for d in definitions],
    }


TARGETS = {
    "Assets/Resources/Content/ActiveSkills/ProductionActiveSkills.json": active_skills,
    "Assets/Resources/Content/Passives/ProductionPassives.json": passives,
    "Assets/Resources/Content/Characters/ProductionCharacters.json": characters,
    "Assets/Resources/Content/Characters/ProductionCharacterBaseline.json": character_baseline,
    "Assets/Resources/Content/Enemies/ProductionEnemies.json": enemies,
    "Assets/Resources/Content/Pickups/ProductionPickups.json": pickups,
}


def tidy(value):
    """Round float noise from multiplications (e.g. 3.3600000000000003) to 6 decimals."""
    if isinstance(value, float):
        rounded = round(value, 6)
        return int(rounded) if rounded.is_integer() else rounded
    if isinstance(value, list):
        return [tidy(item) for item in value]
    if isinstance(value, dict):
        return {key: tidy(item) for key, item in value.items()}
    return value


def render(value):
    return json.dumps(tidy(value), ensure_ascii=False, indent=2) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", action="store_true", help="fail when generated files differ")
    args = parser.parse_args()
    baseline = load_baseline()
    stale = []
    for relative, producer in TARGETS.items():
        path = ROOT / relative
        text = render(producer(baseline))
        current = path.read_text(encoding="utf-8") if path.exists() else None
        if current == text:
            continue
        stale.append(relative)
        if not args.check:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(text, encoding="utf-8", newline="\n")
    if args.check and stale:
        print("STALE: " + ", ".join(stale))
        return 1
    print(("UP TO DATE" if not stale else ("WROTE: " + ", ".join(stale))))
    return 0


if __name__ == "__main__":
    sys.exit(main())
