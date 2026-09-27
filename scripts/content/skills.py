"""Production content: skills. Numeric inputs live in the approved authoring sources."""
from content.sources import content_design_names
from content.common import controls, pierce, wave


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
