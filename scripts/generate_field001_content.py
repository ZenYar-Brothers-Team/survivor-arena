"""Generate FIELD-001 production content JSON from the approved balance baseline.

Source of numbers: docs/balance/field001-baseline-v1.json (Approved, DECISION-0053) and, for the
late skills/passives, docs/balance/late-skills-passives-v1.json (Approved, DECISION-0060).
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


LATE_PACKET = ROOT / "docs/balance/late-skills-passives-v1.json"
SETS_PACKET = ROOT / "docs/balance/sets-v1.json"
ENEMIES_PACKET = ROOT / "docs/balance/enemies-v1.json"
FIELD002_PACKET = ROOT / "docs/balance/field002-v1.json"
BOSSES_PACKET = ROOT / "docs/balance/bosses-v1.json"
FIELD003_PACKET = ROOT / "docs/balance/field003-v1.json"
LAYOUTS_PACKET = ROOT / "docs/balance/field-layouts-v1.json"


def load_baseline():
    data = json.loads(BASELINE.read_text(encoding="utf-8"))
    if data.get("approval") != "Approved":
        raise SystemExit("Baseline is not Approved; production content cannot be generated.")
    late = json.loads(LATE_PACKET.read_text(encoding="utf-8"))
    if not str(late.get("approval", "")).startswith("Approved"):
        raise SystemExit("Late skills/passives packet is not Approved; production content cannot be generated.")
    data["late"] = late
    late_sets = json.loads(SETS_PACKET.read_text(encoding="utf-8"))
    if not str(late_sets.get("approval", "")).startswith("Approved"):
        raise SystemExit("Sets packet is not Approved; production content cannot be generated.")
    data["lateSets"] = late_sets
    late_enemies = json.loads(ENEMIES_PACKET.read_text(encoding="utf-8"))
    if not str(late_enemies.get("approval", "")).startswith("Approved"):
        raise SystemExit("Enemies packet is not Approved; production content cannot be generated.")
    data["lateEnemies"] = late_enemies
    field_two = json.loads(FIELD002_PACKET.read_text(encoding="utf-8"))
    if not str(field_two.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-002 packet is not Approved; production content cannot be generated.")
    data["field002"] = field_two
    late_bosses_packet = json.loads(BOSSES_PACKET.read_text(encoding="utf-8"))
    if not str(late_bosses_packet.get("approval", "")).startswith("Approved"):
        raise SystemExit("Bosses packet is not Approved; production content cannot be generated.")
    data["lateBosses"] = late_bosses_packet
    field_three = json.loads(FIELD003_PACKET.read_text(encoding="utf-8"))
    if not str(field_three.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-003 packet is not Approved; production content cannot be generated.")
    data["field003"] = field_three
    layouts = json.loads(LAYOUTS_PACKET.read_text(encoding="utf-8"))
    if not str(layouts.get("approval", "")).startswith("Approved"):
        raise SystemExit("Field layouts packet is not Approved; production content cannot be generated.")
    data["layouts"] = {entry["presentationId"]: entry for entry in layouts["fields"]}
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


def wave(effects, ctrl, delay=0.0, damage_multiplier=1.0, rotation=0):
    entry = {"delaySeconds": delay, "rotationDegrees": rotation, "damageMultiplier": damage_multiplier, "effects": effects}
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
                strike = {"kind": "Strike", "radius": radius, "telegraphSeconds": row["telegraphSeconds"]}
                if "verticalScale" in shared:
                    strike["verticalScale"] = shared["verticalScale"]
                waves.append(wave([strike],
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
        elif sid == "SKILL-008":
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"])
            level["waves"] = [wave([{
                "kind": "ProjectileBurst", "projectileCount": 1, "layout": "Single", "spreadDegrees": 0,
                "pierceCount": 0, "speed": row["speed"], "lifetimeSeconds": row["lifetimeSeconds"],
                "collisionRadius": row["collisionRadius"],
                "behavior": {"ricochetCount": row["ricochetCount"], "ricochetRange": shared["ricochetRange"],
                             "ricochetRetention": shared["ricochetRetention"],
                             "repeatRicochetTargets": shared["repeatRicochetTargets"]}}], controls(kb, kb_seconds))]
        elif sid == "SKILL-009":
            level.update(targetingMode="Self")
            mine = {"kind": "Mine", "triggerRadius": shared["triggerRadius"], "blastRadius": row["blastRadius"],
                    "lifetimeSeconds": row["lifetimeSeconds"], "maxConcurrent": row["maxConcurrent"]}
            if row["secondaryDamageMultiplier"] > 0:
                mine.update(secondaryDelaySeconds=row["secondaryDelaySeconds"],
                            secondaryDamageMultiplier=row["secondaryDamageMultiplier"],
                            secondaryRadiusMultiplier=row["secondaryRadiusMultiplier"],
                            secondaryKnockbackMultiplier=row["secondaryKnockbackMultiplier"])
            level["waves"] = [wave([mine], controls(kb, kb_seconds))]
        elif sid in ("SKILL-011", "SKILL-015"):
            level.update(targetingMode="Self", initialDirectionDegrees=shared["initialDirectionDegrees"],
                         rotationPerActivationDegrees=shared.get("rotationPerActivationDegrees", 0))
            effect = {"kind": "ProjectileBurst", "projectileCount": row["count"], "layout": shared["layout"],
                      "spreadDegrees": 0, "pierceCount": 0, "speed": row["speed"],
                      "lifetimeSeconds": row["lifetimeSeconds"], "collisionRadius": row["collisionRadius"]}
            if shared.get("unlimitedPierce"):
                effect["behavior"] = {"unlimitedPierce": True}
            waves = [wave([effect], controls(kb, kb_seconds))]
            if row["waveCount"] >= 2:
                waves.append(wave([dict(effect)], controls(kb * row.get("secondKnockbackMultiplier", 1), kb_seconds),
                                  delay=row["secondDelaySeconds"], rotation=row["secondRotationDegrees"],
                                  damage_multiplier=row.get("secondDamageMultiplier", 1)))
            level["waves"] = waves
        elif sid == "SKILL-012":
            level.update(targetingMode="NearestEnemy", targetingRadius=shared["targetingRadius"],
                         baseDamage=row["damagePerTick"])
            level["waves"] = [wave([{
                "kind": "Beam", "durationSeconds": row["durationSeconds"],
                "tickIntervalSeconds": shared["tickIntervalSeconds"], "width": row["width"],
                "range": row["length"], "tracksTarget": row["tracksTarget"]}],
                controls(row["knockbackPerTick"], kb_seconds))]
        elif sid == "SKILL-016":
            level.update(targetingMode="Self", randomSeed=seed_base + number)
            level["waves"] = [wave([{
                "kind": "ProjectileBurst", "projectileCount": row["count"], "layout": shared["layout"],
                "spreadDegrees": 0, "pierceCount": pierce(row["maxHitTargets"]), "speed": row["initialSpeed"],
                "lifetimeSeconds": row["lifetimeSeconds"], "collisionRadius": row["collisionRadius"],
                "behavior": {"stopAfterSeconds": row["stopAfterSeconds"]}}], controls(kb, kb_seconds))]
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
    "SKILL-014": "SKILL-014-VISUAL-PROJECTILE", "SKILL-008": "SKILL-008-VISUAL-PROJECTILE",
    "SKILL-009": "SKILL-009-VISUAL-PROJECTILE", "SKILL-011": "SKILL-011-VISUAL-PROJECTILE",
    "SKILL-015": "SKILL-015-VISUAL-PROJECTILE", "SKILL-016": "SKILL-016-VISUAL-PROJECTILE",
}


def active_skills(baseline):
    kb_seconds = baseline["controls"]["nonzeroKnockbackSeconds"]
    seed_base = baseline["randomness"]["referenceSeeds"]["skillRandomBase"]
    names = content_design_names("SKILL")
    result = []
    for skill in sorted(baseline["skills"] + baseline["late"]["skills"], key=lambda item: item["id"]):
        entry = {"id": skill["id"], "displayName": skill.get("name", names[skill["id"]]),
                 "iconVisualId": f"{skill['id']}-VISUAL-ICON"}
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
    "effectRangeBonus": "effectRangeMultiplierBonus",
    # Late packet rows already use the runtime channel names (DECISION-0060).
    "disappearingXpRecoveryBonus": "disappearingXpRecoveryBonus",
    "pickedUpXpMultiplierBonus": "pickedUpXpMultiplierBonus",
    "effectRangeMultiplierBonus": "effectRangeMultiplierBonus",
    "lowHealthDamageMaxBonus": "lowHealthDamageMaxBonus",
}


def content_design_names(prefix):
    """Card titles from Content Design, e.g. '#### PASSIVE-001 — Крепкое сердце'."""
    import re
    text = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    return {m.group(1): m.group(2).strip() for m in re.finditer(rf"^#+ ({prefix}-\d{{3}}) — (.+)$", text, re.M)}


def passives(baseline):
    names = content_design_names("PASSIVE")
    result = []
    for passive in sorted(baseline["passives"] + baseline["late"]["passives"], key=lambda item: item["id"]):
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
    """CHAR-001 only: draft weights for every implemented skill; locked ones are filtered by profile access."""
    character = baseline["character"]
    implemented = set(baseline["initialRoster"]["actives"]) | {skill["id"] for skill in baseline["late"]["skills"]}
    late_weights = baseline["late"]["draftWeights"][character["id"]]
    names = content_design_names("CHAR")
    weights = [{"skillId": skill, "weight": late_weights.get(skill, weight)}
               for skill, weight in sorted(character["skillDraftWeights"].items()) if skill in implemented]
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


ENEMY_VISUALS = {  # Imported FIELD-001 body references; motion profiles remain shared by movement family.
    "ENEMY-001": ("ENEMY-001-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-002": ("ENEMY-002-VISUAL-BODY", "ENEMY-002-MOTION"),
    "ENEMY-003": ("ENEMY-003-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-004": ("ENEMY-004-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-005": ("ENEMY-005-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-006": ("ENEMY-006-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-007": ("ENEMY-007-VISUAL-BODY", "ENEMY-002-MOTION"),
    "ENEMY-008": ("ENEMY-008-VISUAL-BODY", "ENEMY-002-MOTION"),
    "ENEMY-009": ("ENEMY-009-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-010": ("ENEMY-010-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-011": ("ENEMY-011-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-012": ("ENEMY-012-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-013": ("ENEMY-013-VISUAL-BODY", "ENEMY-002-MOTION"),
    "ENEMY-014": ("ENEMY-014-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-015": ("ENEMY-015-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-016": ("ENEMY-016-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-017": ("ENEMY-017-VISUAL-BODY", "ENEMY-002-MOTION"),
    "ENEMY-018": ("ENEMY-018-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-019": ("ENEMY-019-VISUAL-BODY", "ENEMY-001-MOTION"),
    "ENEMY-020": ("ENEMY-020-VISUAL-BODY", "ENEMY-001-MOTION"),
}
ENEMY_PROJECTILE_VISUALS = {"ENEMY-004": "ENEMY-004-VISUAL-PROJECTILE", "ENEMY-005": "ENEMY-005-VISUAL-PROJECTILE",
                            "ENEMY-006": "ENEMY-005-VISUAL-PROJECTILE"}
CADENCES = {"windup-start-to-windup-start": "WindupStartToStart"}


def enemies(baseline):
    result = []
    potion_chance = baseline["pickups"]["basePotionChance"]
    for enemy in baseline["lateEnemies"]["enemies"]:
        # enemies-v1 (DECISION-0062) restates the shared ordinary potion chance; it is not a per-enemy table.
        if enemy["potionDropChance"] != potion_chance:
            raise SystemExit(f"{enemy['id']}: per-enemy potion chance is not supported")
    for enemy in sorted(baseline["enemies"] + baseline["lateEnemies"]["enemies"], key=lambda item: item["id"]):
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
        if kind in ("KeepDistance", "DistanceReposition", "Orbit"):
            runtime_movement.update(preferredDistance=movement["preferredDistance"], distanceTolerance=movement["distanceTolerance"])
        if kind in ("Orbit", "Zigzag"):
            runtime_movement["lateralStrength"] = movement["lateralStrength"]
        if kind in ("Zigzag", "ApproachRetreat"):
            runtime_movement["cycleSeconds"] = movement["cycleSeconds"]
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
            if "showDashTelegraphLine" in movement:
                runtime_movement["showDashTelegraphLine"] = movement["showDashTelegraphLine"]
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
            }
            if enemy["id"] in ENEMY_PROJECTILE_VISUALS:
                entry["attack"]["projectileVisualId"] = ENEMY_PROJECTILE_VISUALS[enemy["id"]]
            if attack["pattern"] == "Fan":
                entry["attack"]["spreadDegrees"] = attack["spreadDegrees"]
            if attack["pattern"] == "Spiral":
                entry["attack"]["rotationStepDegrees"] = attack["rotationStepDegrees"]
            if attack["pattern"] == "Explosive":
                entry["attack"]["explosionRadius"] = attack["explosionRadius"]
            if attack["pattern"] == "Burst":
                # Sequential shots with per-shot random aim deviation within ±spread/2 (DECISION-0055).
                entry["attack"]["burstIntervalSeconds"] = attack["burstIntervalSeconds"]
                entry["attack"]["spreadDegrees"] = attack["spreadDegrees"]
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


SET_ATTACK_TEMPLATES = {name: f"{name}-ATTACK" for name in
                        ("SET-013", "SET-015", "SET-016", "SET-017", "SET-018", "SET-019", "SET-020")}
SKILL_TRANSFORM_STATS = ("activeDamageBonus", "effectSizeBonus", "effectRangeBonus", "outgoingKnockbackBonus", "actionSpeedBonus")
SKILL_TRANSFORM_MECHANICS = {"projectileSpeedBonus": "projectileSpeedBonus", "orbitAngularSpeedBonus": "orbitAngularSpeedBonus"}


def mechanics(skill, values):
    return {"kind": "SkillMechanics", "skill": skill, "mechanics": values}


def late_set_effects(entry):
    """sets-v1 (DECISION-0061) review effects -> runtime SetEffect JSON."""
    effects = []
    for effect in entry["effects"]:
        kind = effect["kind"]
        if kind == "SkillTransform":
            stats = {PASSIVE_CHANNELS[k]: v for k, v in effect.items() if k in SKILL_TRANSFORM_STATS}
            if stats:
                effects.append({"kind": "SkillTransform", "skill": effect["skill"], "modifier": stats})
            extra = {SKILL_TRANSFORM_MECHANICS[k]: v for k, v in effect.items() if k in SKILL_TRANSFORM_MECHANICS}
            if extra:
                effects.append(mechanics(effect["skill"], extra))
            unknown = set(effect) - {"kind", "skill", "note"} - set(SKILL_TRANSFORM_STATS) - set(SKILL_TRANSFORM_MECHANICS)
            if unknown:
                raise SystemExit(f"{entry['id']}: unmapped transform fields {sorted(unknown)}")
        elif kind == "StatBuff":
            effects.append({"kind": "StatBuff", "modifier": {PASSIVE_CHANNELS[k]: v for k, v in effect.items() if k != "kind"}})
        elif kind == "LevelHeal":
            effects.append({"kind": "LevelHeal", "healFraction": effect["healFraction"]})
        elif kind == "ReturnPhaseTransform":
            if effect["disc"] != entry.get("discPolicy", "rebound-after-first-hit"):
                raise SystemExit(f"{entry['id']}: unsupported disc return policy")
            for skill in effect["skills"]:
                effects.append(mechanics(skill, {"returnDamageBonus": effect["returnDamageBonus"],
                                                 "returnSpeedBonus": effect["returnProjectileSpeedBonus"]}))
        elif kind == "ChainTransform":
            effects.append(mechanics(effect["skill"], {"extraChainTargets": effect["extraTargets"],
                                                       "chainJumpRangeBonus": effect["jumpRangeBonus"],
                                                       "chainFalloffReduction": 1 - effect["falloffMultiplier"]}))
        elif kind == "ProjectileReplacement":
            if not effect["explodeAtStop"]:
                raise SystemExit(f"{entry['id']}: heavy projectile must explode at stop")
            effects.append(mechanics(effect["skill"], {
                "heavyEveryNth": effect["everyNthProjectile"], "heavySizeMultiplier": effect["collisionRadiusMultiplier"],
                "heavyStopMultiplier": effect["stopTimeMultiplier"], "heavyExplosionRadius": effect["explosionRadius"],
                "heavyExplosionDamageMultiplier": effect["explosionDamageMultiplier"],
                "heavyExplosionKnockback": effect["explosionKnockback"]}))
        elif kind == "ExplosionRadiusBonus":
            skills = list(effect["skills"]) + (["SKILL-016"] if effect.get("includesHeavyJunk") else [])
            for skill in skills:
                effects.append(mechanics(skill, {"explosionRadiusBonus": effect["explosionRadiusBonus"]}))
        elif kind == "ExplosionTransform":
            for skill in effect["skills"]:
                effects.append(mechanics(skill, {"explosionDamageBonus": effect["explosionDamageBonus"],
                                                 "explosionRadiusBonus": effect["explosionRadiusBonus"]}))
        elif kind == "PierceBonus":
            effects.append(mechanics(effect["skill"], {"extraPierce": effect["extraPierce"]}))
        elif kind in ("IndependentAttack", "RewardProc"):
            if effect.get("actionSpeedScaling") or effect.get("countsAsSkillActivation") \
                    or not effect["genericDamageAndKnockbackScaling"]:
                raise SystemExit(f"{entry['id']}: set attack policy not expressible")
            if kind == "IndependentAttack" and effect["initialDelaySeconds"] != effect["cooldownSeconds"]:
                raise SystemExit(f"{entry['id']}: first attack must come after one full cooldown")
            if kind == "RewardProc" and effect["event"] != "potion-collected":
                raise SystemExit(f"{entry['id']}: unsupported reward event")
            effects.append({"kind": kind, "attackTemplate": SET_ATTACK_TEMPLATES[entry["id"]],
                            "cooldownSeconds": effect["cooldownSeconds"],
                            "scalesWithSizeAndRange": effect["effectSizeAndRangeScaling"]})
        else:
            raise SystemExit(f"{entry['id']}: no runtime mapping for {kind}")
    return effects


def card_field(card_id, field):
    """'Эффект: ...' style line from a Content Design card."""
    import re
    text = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    block = re.search(rf"^#+ {card_id} — .+?(?=^#+ )", text, re.M | re.S).group(0)
    return re.search(rf"^{field}: (.+)$", block, re.M).group(1).strip()


def sets(baseline):
    names = content_design_names("SET")
    result = []
    late = {entry["id"]: entry for entry in baseline["lateSets"]["sets"]}
    for entry in sorted(baseline["sets"] + list(late.values()), key=lambda item: item["id"]):
        if entry["id"] in late:
            recipe = [{"id": item, "kind": "ActiveSkill" if item.startswith("SKILL-") else "PassiveItem", "minimumLevel": level}
                      for item, level in entry["requirements"].items()]
            result.append({"id": entry["id"], "displayName": names[entry["id"]], "iconVisualId": f"{entry['id']}-VISUAL-ICON",
                           "description": card_field(entry["id"], "Эффект"), "recipe": recipe,
                           "effects": late_set_effects(entry)})
            continue
        recipe = [{"id": item, "kind": "ActiveSkill" if item.startswith("SKILL-") else "PassiveItem", "minimumLevel": level}
                  for item, level in entry["requirements"].items()]
        effects = []
        for effect in entry["effects"]:
            kind = effect["kind"]
            if kind == "SkillTransform":
                effects.append({"kind": "SkillTransform", "skill": effect["skill"], "modifier": {
                    PASSIVE_CHANNELS[k]: v for k, v in effect.items() if k not in ("kind", "skill")}})
            elif kind == "StatBuff":
                effects.append({"kind": "StatBuff", "modifier": {PASSIVE_CHANNELS[k]: v for k, v in effect.items() if k != "kind"}})
            elif kind in ("ConditionalPlayerKnockback", "ConditionalSkillDamage"):
                if effect["condition"] != "target-already-slowed":
                    raise SystemExit(f"{entry['id']}: unsupported condition")
                data = {"kind": "SlowedTargetBonus", "modifier": {
                    PASSIVE_CHANNELS[k]: v for k, v in effect.items() if k in ("activeDamageBonus", "outgoingKnockbackBonus")}}
                if "skill" in effect:
                    data["skill"] = effect["skill"]
                effects.append(data)
            elif kind == "ExistingOrbitSlow":
                if effect["radius"] != "current-orbit-radius":
                    raise SystemExit(f"{entry['id']}: unsupported aura radius")
                effects.append({"kind": "OrbitSlowAura", "skill": effect["skill"], "slowFraction": effect["slowFraction"],
                                "slowSeconds": effect["slowSeconds"], "refreshSeconds": effect["refreshSeconds"]})
            elif kind == "IndependentAttack":
                if effect["initialDelaySeconds"] != effect["cooldownSeconds"] or effect["actionSpeedScaling"] \
                        or effect["countsAsSkillActivation"] or not effect["genericDamageAndKnockbackScaling"]:
                    raise SystemExit(f"{entry['id']}: independent attack policy not expressible")
                effects.append({"kind": "IndependentAttack", "attackTemplate": SET_ATTACK_TEMPLATES[entry["id"]],
                                "cooldownSeconds": effect["cooldownSeconds"],
                                "scalesWithSizeAndRange": effect["effectSizeAndRangeScaling"]})
            else:
                raise SystemExit(f"{entry['id']}: no runtime mapping for {kind}")
        result.append({"id": entry["id"], "displayName": names[entry["id"]], "iconVisualId": f"{entry['id']}-VISUAL-ICON",
                       "description": card_field(entry["id"], "Эффект"), "recipe": recipe, "effects": effects})
    return result


def set_attacks(baseline):
    """Attack templates used only by set effects; never offered in draft."""
    result = []
    seed_base = baseline["randomness"]["referenceSeeds"]["setRandom"]
    kb_seconds = baseline["controls"]["nonzeroKnockbackSeconds"]
    for entry in baseline["sets"]:
        for effect in entry["effects"]:
            if effect["kind"] != "IndependentAttack":
                continue
            if effect["targeting"] != "RandomEnemy":
                raise SystemExit(f"{entry['id']}: unsupported set attack targeting")
            level = {"baseDamage": effect["damage"], "cooldownSeconds": effect["cooldownSeconds"], "actionSpeedBonus": 0,
                     "rotationPerActivationDegrees": 0, "targetingMode": "RandomEnemy",
                     "targetingRadius": effect["targetingRadius"], "randomSeed": seed_base + int(entry["id"].split("-")[1]),
                     "waves": [wave([dict({"kind": "Strike", "radius": effect["radius"], "telegraphSeconds": effect["telegraphSeconds"]},
                                         **({"verticalScale": effect["verticalScale"]} if "verticalScale" in effect else {}))],
                                    controls(effect["knockback"], effect["knockbackSeconds"] or kb_seconds))]}
            result.append({"id": SET_ATTACK_TEMPLATES[entry["id"]], "displayName": content_design_names("SET")[entry["id"]],
                           "iconVisualId": f"{entry['id']}-VISUAL-ICON", "levels": [level] * 6})
    for entry in baseline["lateSets"]["sets"]:
        for effect in entry["effects"]:
            if effect["kind"] not in ("IndependentAttack", "RewardProc"):
                continue
            number = int(entry["id"].split("-")[1])
            template = late_set_attack(entry["id"], effect, seed_base + number, kb_seconds)
            result.append(template)
    return sorted(result, key=lambda item: item["id"])


# Approved set projectile art (commit b595f9e, docs/implementation/evidence/2026-09-26-set-world-art.md).
SET_ATTACK_VISUALS = {name: f"{name}-VISUAL-PROJECTILE" for name in ("SET-016", "SET-018", "SET-019", "SET-020")}


def late_set_attack(set_id, effect, seed, kb_seconds):
    """Attack templates for sets-v1 set attacks; they reuse active-skill effect families."""
    pattern = effect["pattern"]
    kb = effect.get("knockback", 0)
    level = {"cooldownSeconds": effect["cooldownSeconds"], "actionSpeedBonus": 0, "rotationPerActivationDegrees": 0}
    if pattern == "FanOutDischarge":
        level.update(baseDamage=effect["primaryDamage"], targetingMode="NearestEnemy", targetingRadius=effect["targetingRadius"])
        level["waves"] = [wave([{"kind": "Chain", "targetCount": 1 + effect["branchTargets"], "jumpRange": effect["branchRadius"],
                                 "damageRetentionPerJump": effect["branchDamage"] / effect["primaryDamage"], "fanOut": True}],
                               controls(kb, kb_seconds))]
    elif pattern == "AreaAroundPlayer":
        level.update(baseDamage=effect["damage"], targetingMode="Self")
        level["waves"] = [wave([{"kind": "Area", "radius": effect["radius"], "expansionSeconds": 0.15}],
                               controls(kb, effect["knockbackSeconds"]))]
    elif pattern in ("Projectile", "ExplodingProjectile"):
        random_direction = effect["targeting"] == "IndependentRandom"
        if random_direction:
            level.update(targetingMode="Self", randomSeed=seed)
        elif effect["targeting"] == "MovementDirection":
            level.update(targetingMode="MovementDirection", initialDirectionDegrees=0)
        else:
            raise SystemExit(f"{set_id}: unsupported set projectile targeting")
        projectile = {"kind": "ProjectileBurst", "projectileCount": 1,
                      "layout": "IndependentRandom" if random_direction else "Single", "spreadDegrees": 0,
                      "speed": effect["speed"], "collisionRadius": effect["collisionRadius"]}
        if pattern == "Projectile":
            level["baseDamage"] = effect["damage"]
            projectile.update(pierceCount=pierce(effect["maxHitTargets"]), lifetimeSeconds=effect["range"] / effect["speed"])
            ctrl = controls(kb, effect["knockbackSeconds"], effect.get("slowFraction", 0), effect.get("slowSeconds", 0))
        else:
            level["baseDamage"] = effect["impactDamage"]
            projectile.update(pierceCount=0, lifetimeSeconds=effect["lifetimeSeconds"], impactAreaRadius=effect["explosionRadius"],
                              behavior={"explosionDamageMultiplier": effect["explosionDamage"] / effect["impactDamage"],
                                        "explodeOnExpiry": effect["explodeOnExpiry"],
                                        "explosionKnockbackMultiplier": effect["explosionKnockback"] / effect["impactKnockback"]})
            ctrl = controls(effect["impactKnockback"], effect["knockbackSeconds"])
        level["waves"] = [wave([projectile], ctrl)]
    else:
        raise SystemExit(f"{set_id}: unknown set attack pattern {pattern}")
    template = {"id": SET_ATTACK_TEMPLATES[set_id], "displayName": content_design_names("SET")[set_id],
                "iconVisualId": f"{set_id}-VISUAL-ICON"}
    if set_id in SET_ATTACK_VISUALS:
        template["visualId"] = SET_ATTACK_VISUALS[set_id]
    template["levels"] = [level] * 6
    return template


def boss_attack(attack, cooldown, cadence):
    data = {"pattern": "Fan" if attack["id"] == "fan" else "Ring", "damage": attack["damage"], "cooldownSeconds": cooldown,
            "projectileSpeed": attack["projectileSpeed"], "projectileLifetimeSeconds": attack["projectileLifetimeSeconds"],
            "projectileCount": attack["projectileCount"], "projectileRadius": attack["projectileRadius"],
            "telegraphSeconds": attack["telegraphSeconds"], "cadence": cadence,
            "controls": {"knockbackDistance": attack["knockback"], "knockbackSeconds": attack["knockbackSeconds"]},
            "projectileVisualId": "BOSS-001-VISUAL-PROJECTILE"}
    if attack["id"] == "fan":
        data["spreadDegrees"] = attack["spreadDegrees"]
    else:
        data["fixedOrientation"] = True  # ring starts at 0°, 36° step, no hidden rotation
    return data


def boss_teleport(teleport):
    """BOSS-001 teleport-slam (DECISION-0059); the runtime validates ranges."""
    return {"farDistance": teleport["farDistance"], "farSeconds": teleport["farSeconds"],
            "landingDistance": teleport["landingDistance"], "telegraphSeconds": teleport["telegraphSeconds"],
            "impactRadius": teleport["impactRadius"], "impactDamage": teleport["impactDamage"],
            "impactControls": {"knockbackDistance": teleport["impactKnockback"],
                               "knockbackSeconds": teleport["impactKnockbackSeconds"]},
            "impactEffectSeconds": teleport["impactEffectSeconds"],
            "telegraphColor": teleport["telegraphColor"], "impactColor": teleport["impactColor"]}


def bosses(baseline):
    names = {**content_design_names("BOSS"), **content_design_names("MIDBOSS")}
    boss, mid = baseline["boss"], baseline["midboss"]
    if boss["thresholdComparison"] != "strictly-less" or boss["attackSequence"] != ["fan", "ring"] \
            or boss["firstAttackDelaySeconds"] != boss["normalCooldownSeconds"] or boss["movement"] != "Seek":
        raise SystemExit("BOSS-001 policy not expressible by the runtime")
    cadence = CADENCES[boss["cadence"]]
    attacks = {a["id"]: a for a in boss["attacks"]}

    def body(entry, movement, extra=None, art=True):
        data = {"id": entry["id"], "maxHealth": entry["maxHealth"], "collisionSize": entry["collisionSize"],
                "movementSpeed": entry["movementSpeed"], "contactDamage": entry["contactDamage"],
                "contactDamageInterval": entry["contactDamageIntervalSeconds"], "experienceReward": entry["experienceReward"],
                "knockbackResistance": entry["knockbackResistance"],
                "contactControls": {"knockbackDistance": entry["contactKnockback"], "knockbackSeconds": entry["knockbackSeconds"]},
                "movement": movement}
        if art:
            data.update(visualId=f"{entry['id']}-VISUAL-BODY", motionProfileId="ENEMY-001-MOTION")
        data.update(extra or {})
        return data

    final = {
        "id": boss["id"], "displayName": names[boss["id"]], "hook": "FinalBoss",
        "spawnOffsetX": boss["spawnOffset"][0], "spawnOffsetY": boss["spawnOffset"][1],
        "keepAttackOrderOnPhaseChange": True, "strictHealthThreshold": True,
        "body": body(boss, {"kind": "Seek"}),
        "attacks": [{"id": f"{boss['id']}-{kind.upper()}", "attack": boss_attack(attacks[kind], boss["normalCooldownSeconds"], cadence)}
                    for kind in boss["attackSequence"]] +
                   [{"id": f"{boss['id']}-{kind.upper()}-ENRAGED", "attack": boss_attack(attacks[kind], boss["enragedCooldownSeconds"], cadence)}
                    for kind in boss["attackSequence"]],
        "teleport": boss_teleport(boss["teleport"]),
        "phases": [
            {"id": f"{boss['id']}-PHASE-1", "healthThreshold": 1,
             "attackEnemyIds": [f"{boss['id']}-{kind.upper()}" for kind in boss["attackSequence"]]},
            {"id": f"{boss['id']}-PHASE-2", "healthThreshold": boss["healthPhaseThreshold"],
             "attackEnemyIds": [f"{boss['id']}-{kind.upper()}-ENRAGED" for kind in boss["attackSequence"]]},
        ],
    }
    if mid["movement"] != "DoubleTelegraphedDash" or mid["direction"] != "snapshot-at-each-telegraph-start" \
            or mid["firstPairDelaySeconds"] != mid["recoveryAfterPairSeconds"] or mid["attack"] is not None:
        raise SystemExit("MIDBOSS-001 policy not expressible by the runtime")
    midboss = {
        "id": mid["id"], "displayName": names[mid["id"]], "hook": "MidBoss",
        "spawnOffsetX": mid["spawnOffset"][0], "spawnOffsetY": mid["spawnOffset"][1],
        "body": body(mid, {"kind": "TelegraphedDash", "dashTelegraphSeconds": mid["firstTelegraphSeconds"],
                           "dashDurationSeconds": mid["dashDurationSeconds"], "dashCooldownSeconds": mid["recoveryAfterPairSeconds"],
                           "dashSpeedMultiplier": mid["dashSpeedMultiplier"], "dashCount": mid["dashCount"],
                           "followUpTelegraphSeconds": mid["secondTelegraphSeconds"]},
                     {"dashContactControls": {"knockbackDistance": mid["dashKnockback"], "knockbackSeconds": mid["knockbackSeconds"]}}),
        "phases": [{"id": f"{mid['id']}-PHASE-1", "healthThreshold": 1, "attackEnemyIds": []}],
    }
    return [final, midboss] + field_two_bosses(baseline, names, body) + late_bosses(baseline, names, body)


def dash_ring(ring, kb_seconds):
    if ring["trigger"] != "dash-end" or ring["spreadDegrees"] != 360:
        raise SystemExit("dash volley must be a full ring on dash end")
    return {"pattern": "Ring", "damage": ring["damage"], "cooldownSeconds": 1, "projectileSpeed": ring["projectileSpeed"],
            "projectileLifetimeSeconds": ring["projectileLifetimeSeconds"], "projectileCount": ring["projectileCount"],
            "projectileRadius": ring["projectileRadius"], "telegraphSeconds": ring["telegraphSeconds"],
            "projectileVisualId": "BOSS-001-VISUAL-PROJECTILE",
            "controls": {"knockbackDistance": ring["knockback"], "knockbackSeconds": ring["knockbackSeconds"] or kb_seconds}}


def field_two_bosses(baseline, names, body):
    """BOSS-002 / MIDBOSS-002 (field002-v1, DECISION-0063): dash movement with a ring on dash end."""
    result = []
    for key, hook in (("boss", "FinalBoss"), ("midboss", "MidBoss")):
        entry = baseline["field002"][key]
        move = entry["movement"]
        if move["kind"] != "TelegraphedDash" or move["firstDashDelaySeconds"] != move["dashCooldownSeconds"]:
            raise SystemExit(f"{entry['id']}: dash timing not expressible by the runtime")
        extra = {"dashContactControls": {"knockbackDistance": move["dashKnockback"], "knockbackSeconds": entry["knockbackSeconds"]},
                 "dashEndAttack": dash_ring(entry["dashEndAttack"], entry["knockbackSeconds"])}
        enrage = entry.get("enrage")
        if enrage:
            if enrage["thresholdComparison"] != "strictly-less":
                raise SystemExit(f"{entry['id']}: enrage comparison not expressible")
            extra["dashEndRepeat"] = {"everyNthDash": enrage["everyNthDash"], "belowHealthFraction": enrage["healthThreshold"],
                                      "delaySeconds": enrage["extraVolleyDelaySeconds"],
                                      "rotationDegrees": enrage["extraVolleyRotationDegrees"]}
        movement = {"kind": "TelegraphedDash", "dashTelegraphSeconds": move["dashTelegraphSeconds"],
                    "dashDurationSeconds": move["dashDurationSeconds"], "dashCooldownSeconds": move["dashCooldownSeconds"],
                    "dashSpeedMultiplier": move["dashSpeedMultiplier"], "showDashTelegraphLine": move["showDashTelegraphLine"]}
        encounter = {"id": entry["id"], "displayName": names[entry["id"]], "hook": hook,
                     "spawnOffsetX": entry["spawnOffset"][0], "spawnOffsetY": entry["spawnOffset"][1],
                     "body": body(entry, movement, extra),
                     "phases": [{"id": f"{entry['id']}-PHASE-1", "healthThreshold": 1, "attackEnemyIds": []}]}
        if "teleport" in entry:
            encounter["teleport"] = boss_teleport(entry["teleport"])
        result.append(encounter)
    return result


# Presentation colors of the procedural boss hazards (DECISION-0066); RGBA 0…1.
HAZARD_COLORS = {
    "zone": ([1, 0.35, 0.15, 0.85], [1, 0.8, 0.45, 0.95]),
    "burning": ([1, 0.35, 0.15, 0.85], [1, 0.5, 0.15, 0.9]),
    "safe": ([1, 0.92, 0.6, 0.75], [0.55, 0.95, 1, 1]),
    "beam": ([1, 0.9, 0.5, 0.8], [1, 0.97, 0.8, 0.95]),
    "summon": [0.9, 0.3, 0.25, 0.9],
}
HAZARD_IMPACT_EFFECT_SECONDS = 0.45
BURN_TICK_SECONDS = 0.5
ORIENTATIONS = {"towardPlayer": "TowardPlayer", "awayFromDash": "AwayFromDash", "self": "Self"}


def late_controls(entry):
    ctrl = {"knockbackDistance": entry["knockback"], "knockbackSeconds": entry["knockbackSeconds"]}
    if entry.get("slowFraction"):
        ctrl.update(slowFraction=entry["slowFraction"], slowSeconds=entry["slowSeconds"])
    return ctrl


def late_projectile(a):
    data = {"pattern": a["pattern"], "damage": a["damage"], "cooldownSeconds": a["cooldownSeconds"] or 1,
            "projectileSpeed": a["projectileSpeed"], "projectileLifetimeSeconds": a["projectileLifetimeSeconds"],
            "projectileCount": a["projectileCount"], "projectileRadius": a["projectileRadius"],
            "telegraphSeconds": a["telegraphSeconds"], "cadence": "WindupStartToStart", "controls": late_controls(a),
            "projectileVisualId": "BOSS-001-VISUAL-PROJECTILE"}
    if a["pattern"] == "Fan" or (a["pattern"] == "Explosive" and a["projectileCount"] > 1):
        data["spreadDegrees"] = a["spreadDegrees"]
    if a["pattern"] == "Burst":
        data.update(spreadDegrees=a["spreadDegrees"], burstIntervalSeconds=a["burstIntervalSeconds"])
    if a["pattern"] == "Explosive":
        data["explosionRadius"] = a["explosionRadius"]
    if a["followUps"]:
        data["followUps"] = a["followUps"]
    if a["windupMovementMultiplier"] != 1:
        data["windupMovementMultiplier"] = a["windupMovementMultiplier"]
    return data


def late_zone(z):
    family = "safe" if z["placement"] == "SafeCircles" else "burning" if z["lingerSeconds"] else "zone"
    telegraph, impact = HAZARD_COLORS[family]
    data = {"placement": z["placement"], "count": z["count"], "radius": z["radius"], "fillSeconds": z["fillSeconds"],
            "damage": z["damage"], "controls": late_controls(z), "lingerSeconds": z["lingerSeconds"],
            "impactEffectSeconds": HAZARD_IMPACT_EFFECT_SECONDS, "telegraphColor": telegraph, "impactColor": impact}
    if z["placement"] in ("AroundPlayer", "SafeCircles"):
        data.update(scatterRadius=z["scatterRadius"], minSpacing=z["minSpacing"])
    if z["placement"] == "Trail":
        data["intervalSeconds"] = z["intervalSeconds"]
    if z["lingerSeconds"]:
        data.update(lingerDamagePerSecond=z["lingerDamagePerSecond"], lingerTickSeconds=BURN_TICK_SECONDS)
    return data


def late_beam(b):
    telegraph, beam = HAZARD_COLORS["beam"]
    return {"anglesDegrees": b["anglesDegrees"], "length": b["length"], "width": b["width"],
            "telegraphSeconds": b["telegraphSeconds"], "activeSeconds": b["activeSeconds"], "sweepDegrees": b["sweepDegrees"],
            "damage": b["damage"], "controls": late_controls(b), "telegraphColor": telegraph, "beamColor": beam}


def late_summon(m):
    return {"enemyId": m["enemyId"], "count": m["count"], "spawnDistance": m["spawnDistance"], "maxAlive": m["maxAlive"],
            "telegraphSeconds": m["telegraphSeconds"], "markerColor": HAZARD_COLORS["summon"]}


def late_step(step_id, a):
    if a["kind"] == "Projectile":
        return {"id": step_id, "attack": late_projectile(a)}
    step = {"id": step_id, "cooldownSeconds": a["cooldownSeconds"]}
    step[{"Zone": "zone", "Beam": "beam", "Summon": "summon"}[a["kind"]]] = {
        "Zone": late_zone, "Beam": late_beam, "Summon": late_summon}[a["kind"]](a)
    return step


def late_dash_entries(items):
    result = []
    for item in items:
        entry = {"delaySeconds": item["delaySeconds"], "orientation": ORIENTATIONS[item["orientation"]]}
        a = item["attack"]
        if a["kind"] == "Zone":
            entry["zone"] = late_zone(a)
        else:
            entry["attack"] = late_projectile(a)
        result.append(entry)
    return result


def late_movement(m):
    if m["kind"] == "Seek":
        return {"kind": "Seek"}
    if m["kind"] in ("KeepDistance", "Orbit"):
        data = {"kind": m["kind"], "preferredDistance": m["preferredDistance"], "distanceTolerance": m["distanceTolerance"]}
        if m["kind"] == "Orbit":
            data["lateralStrength"] = m["lateralStrength"]
        return data
    if m["kind"] != "TelegraphedDash" or m["firstDashDelaySeconds"] != m["dashCooldownSeconds"]:
        raise SystemExit(f"movement {m['kind']} not expressible by the runtime")
    data = {"kind": "TelegraphedDash", "dashTelegraphSeconds": m["dashTelegraphSeconds"],
            "dashDurationSeconds": m["dashDurationSeconds"], "dashCooldownSeconds": m["dashCooldownSeconds"],
            "dashSpeedMultiplier": m["dashSpeedMultiplier"], "dashCount": m["dashCount"],
            "showDashTelegraphLine": m["showDashTelegraphLine"]}
    if m["dashCount"] > 1:
        data["followUpTelegraphSeconds"] = m["followUpTelegraphSeconds"]
    return data


def late_bosses(baseline, names, body):
    """BOSS-003…010 / MIDBOSS-003…010 (bosses-v1, DECISION-0066): card attacks plus one signature zone/beam/summon."""
    result = []
    for entry in baseline["lateBosses"]["encounters"]:
        eid = entry["id"]
        extra = {}
        if entry["dashKnockback"] is not None:
            extra["dashContactControls"] = {"knockbackDistance": entry["dashKnockback"], "knockbackSeconds": entry["knockbackSeconds"]}
        dash_end = entry["dashEnd"]
        if dash_end:
            if dash_end["firesAfter"] != "last-dash-of-series":
                raise SystemExit(f"{eid}: dash-end timing not expressible by the runtime")
            extra["dashEndAttacks"] = late_dash_entries(dash_end["attacks"])
            replacement = dash_end.get("belowHealthReplacement")
            if replacement:
                extra["dashEndReplacement"] = {"belowHealthFraction": replacement["belowHealth"],
                                               "attacks": late_dash_entries(replacement["attacks"])}
        steps, phases = [], []
        for phase in entry["phases"]:
            if phase["thresholdComparison"] != "strictly-less":
                raise SystemExit(f"{eid}: phase comparison not expressible")
            ids = []
            for a in phase["attacks"]:
                step_id = f"{eid}-{phase['id']}-{a['id']}"
                step = late_step(step_id, a)
                emitted = next((known for known in steps if known["id"] == step_id), None)
                if emitted is None:
                    steps.append(step)
                elif emitted != step:
                    raise SystemExit(f"{step_id}: one step id with two different payloads")
                ids.append(step_id)
            data = {"id": f"{eid}-{phase['id']}", "healthThreshold": phase["belowHealth"], "attackEnemyIds": ids}
            if phase["movementOverride"]:
                data["movement"] = late_movement(phase["movementOverride"])
            phases.append(data)
        encounter = {"id": eid, "displayName": names[eid], "hook": entry["hook"],
                     "spawnOffsetX": entry["spawnOffset"][0], "spawnOffsetY": entry["spawnOffset"][1],
                     "keepAttackOrderOnPhaseChange": True, "strictHealthThreshold": True,
                     "body": body(entry, late_movement(entry["movement"]), extra, art=False),
                     "attacks": steps, "phases": phases}
        if entry["holdRangedDuringDash"]:
            encounter["holdAttacksDuringDash"] = True
        if entry["teleport"]:
            encounter["teleport"] = boss_teleport(entry["teleport"])
        result.append(encounter)
    return result


def travelers(baseline):
    schedule = baseline["travelerSchedule"]
    seeds = baseline["randomness"]["referenceSeeds"]
    if schedule["typeSelection"] != "uniform-without-replacement" or not schedule["timesIndependent"] \
            or schedule["spawnTimeIntervalSeconds"] != [0, 900 - schedule["endBufferSeconds"]]:
        raise SystemExit("Traveler schedule policy not expressible by the runtime")
    result = []
    for t in baseline["travelers"]:
        if t["attack"] is not None or t["movement"]["kind"] != "Seek" or t["bookDropCountOnKill"] != 1 or t["rewardOnEscape"] != 0:
            raise SystemExit(f"{t['id']}: unsupported Traveler policy")
        support = t["support"]
        result.append({
            "id": t["id"], "name": t["name"], "marker": t["marker"], "role": t["role"],
            "body": {"id": t["id"], "knockbackResistance": t["knockbackResistance"], "maxHealth": t["maxHealth"],
                     "visualId": f"{t['id']}-VISUAL-BODY", "motionProfileId": "ENEMY-001-MOTION",
                     "collisionSize": t["collisionSize"], "movementSpeed": t["movementSpeed"],
                     "contactDamage": t["contactDamage"], "contactDamageInterval": t["contactDamageIntervalSeconds"],
                     "experienceReward": t["experienceReward"], "movement": {"kind": "Seek"},
                     "contactControls": {"knockbackDistance": t["contactKnockback"],
                                         "knockbackSeconds": t["knockbackSeconds"] or baseline["controls"]["nonzeroKnockbackSeconds"]}},
            "presenceSeconds": t["presenceSeconds"], "wanderSeconds": t["wanderSeconds"], "restSeconds": t["restSeconds"],
            "avoidRadius": t["avoidRadius"], "avoidSeconds": t["avoidSeconds"], "guardOffset": t["guardOffset"],
            "support": support["kind"], "supportRadius": support["radius"], "reduction": support["reduction"],
            "resistance": support["resistance"], "shieldHp": support["shieldHp"], "shieldSeconds": support["shieldSeconds"],
            "supportCooldown": support["cooldownSeconds"], "supportTargets": support["supportTargets"], "color": t["color"],
        })
    def entry(schedule_id, seed, rank):
        # DECISION-0063: every field draws from the global pool of implemented Travelers, roles never repeat.
        return {"id": schedule_id, "travelerIds": [t["id"] for t in baseline["travelers"]],
                "countProbabilities": schedule["countProbabilities"], "seed": seed, "fieldRank": rank,
                "placementAttempts": schedule["placementAttempts"], "endBufferSeconds": schedule["endBufferSeconds"],
                "spawnScreenHeights": schedule["spawnScreenHeights"], "fieldGrowth": schedule["fieldGrowth"],
                "timeGrowth": schedule["timeGrowth"]}
    for key in ("field002", "field003"):
        if not baseline[key]["travelers"]["pool"].startswith("global"):
            raise SystemExit(f"{key} Traveler pool must be the global pool")
    return {"travelers": result, "schedules": [entry(schedule["id"], seeds["travelers"], schedule["fieldRank"]),
                                               entry("FIELD-002-TRAVELERS", seeds["travelers"] + 1000, 2),
                                               entry("FIELD-003-TRAVELERS", seeds["travelers"] + 2000, 3)]}


def fields(baseline):
    field = baseline["field"]
    names = content_design_names("FIELD")
    if field["obstaclesBlock"] != "player-only" or field["spawnPoint"] != [0, 0]:
        raise SystemExit("FIELD-001 geometry policy not expressible by the runtime")
    two = baseline["field002"]["field"]
    three = baseline["field003"]["field"]
    walls = ["Wall_Top", "Wall_Bottom", "Wall_Left", "Wall_Right"]
    return {
        "defaultFieldId": field["id"], "availableFieldIds": [field["id"], two["id"], three["id"]],
        # The Gameplay scene keeps its baked walls and SpawnPoint (DECISION-0054 section 9); FIELD-002 reuses the scene.
        "environments": [{"id": field["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": "FIELD-002-ENVIRONMENT", "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": three["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls}],
        "fields": [{"id": field["id"], "displayName": names[field["id"]], "description": field["description"],
                    "thumbnailPlaceholder": field["thumbnailPlaceholder"], "difficulty": field["difficulty"],
                    "thumbnailVisualId": field["thumbnailVisualId"],
                    "unlockDescription": field["unlockDescription"], "environmentId": field["environmentId"],
                    "timelineId": field["timelineId"], "travelerScheduleId": field["travelerScheduleId"],
                    "finalBossId": field["finalBossId"], "midBossId": field["midBossId"],
                    "enemyIds": [e["id"] for e in baseline["enemies"]]},
                   {"id": two["id"], "displayName": names[two["id"]], "description": card_field(two["id"], "Роль"),
                    "thumbnailPlaceholder": "Королевский тракт", "difficulty": two["difficulty"],
                    "thumbnailVisualId": "FIELD-002-VISUAL-BACKGROUND",
                    "unlockDescription": "Пройдите «Деревенскую окраину»", "environmentId": "FIELD-002-ENVIRONMENT",
                    "timelineId": "FIELD-002-TIMELINE", "travelerScheduleId": "FIELD-002-TRAVELERS",
                    "finalBossId": baseline["field002"]["boss"]["id"], "midBossId": baseline["field002"]["midboss"]["id"],
                    "enemyIds": baseline["field002"]["enemyPool"]},
                   # FIELD-003 (field003-v1, DECISION-0067): no approved thumbnail yet, the selection card shows its text.
                   {"id": three["id"], "displayName": names[three["id"]], "description": card_field(three["id"], "Роль"),
                    "thumbnailPlaceholder": three["thumbnailPlaceholder"], "difficulty": three["difficulty"],
                    "unlockDescription": three["unlockDescription"], "environmentId": three["environmentId"],
                    "timelineId": three["timelineId"], "travelerScheduleId": three["travelerScheduleId"],
                    "finalBossId": three["finalBossId"], "midBossId": three["midBossId"],
                    "enemyIds": baseline["field003"]["enemyPool"]}],
    }


def field_presentation(baseline):
    """Accepted FIELD-001 art/decor values from the fixture arena; obstacles are the 64 authored baseline rects."""
    fixture = json.loads((ROOT / "Assets/Resources/Content/Presentation/FixtureFieldEnvironmentPresentation.json")
                         .read_text(encoding="utf-8-sig"))[0]
    field = baseline["field"]
    data = dict(fixture, id="FIELD-001-PRESENTATION", environmentId=field["environmentId"],
                interiorObstacleCount=len(field["obstacles"]),
                nearObstacleCount=sum(1 for o in field["obstacles"] if abs(o["x"]) <= 20 and abs(o["y"]) <= 20))
    for o in field["obstacles"]:
        if o["rotationDegrees"] != 0 or o["kind"] not in ("Stump", "Fence"):
            raise SystemExit(f"{o['id']}: unsupported obstacle")
    data["obstacles"] = [{"id": o["id"], "kind": o["kind"], "x": o["x"], "y": o["y"], "width": o["width"], "height": o["height"]}
                         for o in field["obstacles"]]
    two = baseline["field002"]["field"]
    # Both authored obstacle families keep their gameplay rectangles; only the sprite family differs.
    obstacle_kinds = {"Rock": "Stump", "Column": "Column"}
    for o in two["obstacles"]:
        if o["rotationDegrees"] != 0 or o["kind"] not in obstacle_kinds:
            raise SystemExit(f"{o['id']}: unsupported obstacle")
    second = dict(data, id="FIELD-002-PRESENTATION", environmentId="FIELD-002-ENVIRONMENT",
                  groundVisualId="FIELD-002-VISUAL-GROUND", obstacleVisualId="FIELD-002-VISUAL-BOULDER",
                  columnVisualId="FIELD-002-VISUAL-COLUMN", shrineVisualId="FIELD-002-VISUAL-SHRINE",
                  shrineChance=0.025,
                  seed=data["seed"] + 1000, obstacleSeed=data["obstacleSeed"] + 1000,
                  interiorObstacleCount=len(two["obstacles"]),
                  nearObstacleCount=sum(1 for o in two["obstacles"] if abs(o["x"]) <= 20 and abs(o["y"]) <= 20))
    second["obstacles"] = [{"id": o["id"], "kind": obstacle_kinds[o["kind"]], "x": o["x"], "y": o["y"], "width": o["width"],
                            "height": o["height"]} for o in two["obstacles"]]
    three = baseline["field003"]["field"]
    ruin_kinds = {"Wall": "Fence", "Rubble": "Stump"}
    for o in three["obstacles"]:
        if o["rotationDegrees"] != 0 or o["kind"] not in ruin_kinds:
            raise SystemExit(f"{o['id']}: unsupported obstacle")
    if three["waterDecor"]["blocksMovement"]:
        raise SystemExit("FIELD-003 water must stay visual")
    third = dict(data, id="FIELD-003-PRESENTATION", environmentId=three["environmentId"],
                 obstacleVisualId="FIELD-002-VISUAL-BOULDER",
                 seed=data["seed"] + 2000, obstacleSeed=data["obstacleSeed"] + 2000,
                 interiorObstacleCount=len(three["obstacles"]),
                 nearObstacleCount=sum(1 for o in three["obstacles"] if abs(o["x"]) <= 20 and abs(o["y"]) <= 20))
    third["obstacles"] = [{"id": o["id"], "kind": ruin_kinds[o["kind"]], "x": o["x"], "y": o["y"], "width": o["width"],
                           "height": o["height"]} for o in three["obstacles"]]
    presentations = [data, second, third]
    # DECISION-0068: the first three fields generate their obstacles every run from patterns instead of a fixed list.
    wall = baseline["field"]["wallThickness"]
    for presentation in presentations:
        layout = baseline["layouts"][presentation["id"]]
        presentation.pop("obstacles")
        presentation["obstacleLayout"] = {
            "cellSize": layout["cellSize"], "patternsPerCell": layout["patternsPerCell"],
            "edgeMargin": layout["edgeMargin"] + wall, "cellMargin": layout["cellMargin"],
            "startClearRadius": layout["startClearRadius"], "minPatternGap": layout["minPatternGap"],
            "placementAttempts": layout["placementAttempts"], "referenceSeed": layout["referenceSeed"],
            "patterns": [{"id": pattern["id"], "weight": pattern["weight"], "rotations": pattern["rotations"],
                          "pieces": [{"kind": piece["kind"], "x": piece["x"], "y": piece["y"], "width": piece["width"],
                                      "height": piece["height"]} for piece in pattern["pieces"]]}
                         for pattern in layout["patterns"]]}
    return presentations


def minutes(seconds):
    return f"{int(seconds // 60)}:{int(seconds % 60):02d}"


def timeline(baseline):
    return field_timeline(baseline["timeline"], baseline, baseline["randomness"]["referenceSeeds"]["waves"], True)


def timeline_field002(baseline):
    return field_timeline(baseline["field002"]["timeline"], baseline, baseline["randomness"]["referenceSeeds"]["waves"] + 1000,
                          False)


def timeline_field003(baseline):
    return field_timeline(baseline["field003"]["timeline"], baseline, baseline["randomness"]["referenceSeeds"]["waves"] + 2000,
                          False)


def field_timeline(t, baseline, seed, neutral_modifiers):
    phases, clock = [], 0
    for p in t["phases"]:
        if p["startSeconds"] != clock:
            raise SystemExit(f"{p['id']}: phases must be contiguous")
        clock += p["durationSeconds"]
        modifiers = p["modifiers"]
        if neutral_modifiers and any(v != 1 for v in modifiers.values()):
            raise SystemExit(f"{p['id']}: baseline v1 keeps all wave multipliers at 1")
        phase = {"id": p["id"], "displayName": f"{minutes(p['startSeconds'])}–{minutes(clock)}", "tag": p["tag"],
                 "spawnMode": p["spawnMode"], "durationSeconds": p["durationSeconds"],
                 "spawnIntervalSeconds": p["spawnIntervalSeconds"], "maxAliveEnemies": p["maxAliveEnemies"],
                 "composition": [{"enemyId": k, "weight": v} for k, v in p["composition"].items() if v > 0],
                 "modifiers": modifiers}
        if p["burst"]:
            phase["burst"] = p["burst"]
        phases.append(phase)
    if clock != baseline["field"]["durationSeconds"]:
        raise SystemExit("timeline must cover the whole field duration")
    return {"id": t["id"], "seed": seed,
            "spawnRadius": baseline["field"]["spawnRadius"], "openingSpawn": baseline["field"]["openingSpawn"],
            "phases": phases, "hooks": t["hooks"]}


def run_setup(baseline):
    draft, xp = baseline["draft"], baseline["experience"]
    return {"startingCharacterId": baseline["character"]["id"],
            "draft": {"offerCount": draft["offerCount"], "setDraftChance": draft["setDraftChance"],
                      "seed": baseline["randomness"]["referenceSeeds"]["draft"], "initialRerolls": draft["initialRerolls"],
                      "initialBanishes": draft["initialBanishes"], "emptyBookCurrency": draft["emptyBookCurrency"]},
            "experience": {"levelThresholds": xp["levelThresholds"], "baseDropLifetimeSeconds": xp["baseDropLifetimeSeconds"]}}


TARGETS = {
    "Assets/Resources/Content/ActiveSkills/ProductionActiveSkills.json": active_skills,
    "Assets/Resources/Content/Passives/ProductionPassives.json": passives,
    "Assets/Resources/Content/Characters/ProductionCharacters.json": characters,
    "Assets/Resources/Content/Characters/ProductionCharacterBaseline.json": character_baseline,
    "Assets/Resources/Content/Enemies/ProductionEnemies.json": enemies,
    "Assets/Resources/Content/Pickups/ProductionPickups.json": pickups,
    "Assets/Resources/Content/Sets/ProductionSets.json": sets,
    "Assets/Resources/Content/Bosses/ProductionBosses.json": bosses,
    "Assets/Resources/Content/Travelers/ProductionTravelers.json": travelers,
    "Assets/Resources/Content/Fields/ProductionFields.json": fields,
    "Assets/Resources/Content/Presentation/ProductionFieldEnvironmentPresentation.json": field_presentation,
    "Assets/Resources/Content/Waves/ProductionWaveTimeline.json": timeline,
    "Assets/Resources/Content/Waves/ProductionWaveTimelineField002.json": timeline_field002,
    "Assets/Resources/Content/Waves/ProductionWaveTimelineField003.json": timeline_field003,
    "Assets/Resources/Content/Run/ProductionRunSetup.json": run_setup,
    "Assets/Resources/Content/ActiveSkills/ProductionSetAttacks.json": set_attacks,
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
