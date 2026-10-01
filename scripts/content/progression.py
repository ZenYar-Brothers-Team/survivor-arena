"""Production content: progression. Numeric inputs live in the approved authoring sources."""
from content.sources import card_field, content_design_names
from content.common import controls, pierce, wave


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


SET_ATTACK_TEMPLATES = {name: f"{name}-ATTACK" for name in
                        ("SET-013", "SET-015", "SET-016", "SET-017", "SET-018", "SET-019", "SET-020", "SET-021", "SET-022")}


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


LOW_STAT_CHANNELS = {  # sets-low-v1 field -> runtime CharacterStatModifierData field (DECISION-0138)
    "damageBonus": "activeSkillDamageMultiplierBonus", "sizeBonus": "effectSizeMultiplierBonus",
    "rangeBonus": "effectRangeMultiplierBonus", "outgoingKnockbackBonus": "outgoingKnockbackBonus",
    "actionSpeedBonus": "actionSpeedBonus", "maxHpBonus": "maxHealthMultiplierBonus",
    "movementSpeedBonus": "movementSpeedMultiplierBonus", "incomingDamageReduction": "incomingDamageReductionBonus",
    "pickupRadiusBonus": "pickupRadiusMultiplierBonus", "regenPerSecond": "healthRegenerationPerSecondBonus",
}


LOW_MECHANICS = {  # sets-low-v1 field -> SkillMechanicBonus field
    "projectileSpeedBonus": "projectileSpeedBonus", "rotationSpeedBonus": "orbitAngularSpeedBonus",
    "extraPierce": "extraPierce", "extraProjectiles": "extraProjectiles", "jumpRangeBonus": "chainJumpRangeBonus",
    "slowStrengthBonus": "slowStrengthBonus", "explosionDamageBonus": "explosionDamageBonus",
    "explosionRadiusBonus": "explosionRadiusBonus",
}


def low_set_effects(entry):
    """sets-low-v1 (DECISION-0138) review effects -> runtime SetEffect JSON."""
    effects = []
    for effect in entry["effects"]:
        kind = effect["kind"]
        if kind == "SkillTransform":
            for skill in effect.get("skills", [effect.get("skill")]):
                stats = {LOW_STAT_CHANNELS[k]: v for k, v in effect.items() if k in LOW_STAT_CHANNELS}
                extra = {LOW_MECHANICS[k]: v for k, v in effect.items() if k in LOW_MECHANICS}
                unknown = set(effect) - {"kind", "skill", "skills"} - set(LOW_STAT_CHANNELS) - set(LOW_MECHANICS)
                if unknown:
                    raise SystemExit(f"{entry['id']}: unmapped transform fields {sorted(unknown)}")
                if stats:
                    effects.append({"kind": "SkillTransform", "skill": skill, "modifier": stats})
                if extra:
                    effects.append(mechanics(skill, extra))
        elif kind == "StatBuff":
            effects.append({"kind": "StatBuff", "modifier": {LOW_STAT_CHANNELS[k]: v for k, v in effect.items() if k != "kind"}})
        elif kind == "IndependentAttack":
            if effect["actionSpeedScaling"] or effect["countsAsSkillActivation"] or not effect["genericDamageAndKnockbackScaling"] \
                    or effect["initialDelaySeconds"] != effect["cooldownSeconds"]:
                raise SystemExit(f"{entry['id']}: set attack policy not expressible")
            effects.append({"kind": kind, "attackTemplate": SET_ATTACK_TEMPLATES[entry["id"]],
                            "cooldownSeconds": effect["cooldownSeconds"],
                            "scalesWithSizeAndRange": effect["effectSizeAndRangeScaling"]})
        else:
            raise SystemExit(f"{entry['id']}: no runtime mapping for {kind}")
    return effects


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
                aura = {"kind": "OrbitSlowAura", "skill": effect["skill"], "slowFraction": effect["slowFraction"],
                        "slowSeconds": effect["slowSeconds"], "refreshSeconds": effect["refreshSeconds"]}
                if effect.get("damageTakenBonus"):  # SET-010 vulnerability (DECISION-0139)
                    aura["damageTakenBonus"] = effect["damageTakenBonus"]
                effects.append(aura)
            elif kind == "GrantedSkillSlow":  # SET-004 wave slows by itself (DECISION-0139)
                effects.append(mechanics(effect["skill"], {"grantedSlowFraction": effect["slowFraction"],
                                                           "grantedSlowSeconds": effect["slowSeconds"]}))
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
    for entry in baseline["lowSets"]["sets"]:
        recipe = [{"id": item, "kind": "ActiveSkill" if item.startswith("SKILL-") else "PassiveItem", "minimumLevel": level}
                  for item, level in entry["requirements"].items()]
        result.append({"id": entry["id"], "displayName": names[entry["id"]],
                       "description": card_field(entry["id"], "Эффект"), "recipe": recipe,
                       "effects": low_set_effects(entry)})  # no iconVisualId until icons are added
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
    for entry in baseline["lowSets"]["sets"]:
        for effect in entry["effects"]:
            if effect["kind"] == "IndependentAttack":
                result.append(late_set_attack(entry["id"], effect, seed_base + int(entry["id"].split("-")[1]), kb_seconds))
    return sorted(result, key=lambda item: item["id"])


# Approved set projectile art (commit b595f9e, docs/implementation/evidence/2026-09-26-set-world-art.md).
SET_ATTACK_VISUALS = {name: f"{name}-VISUAL-PROJECTILE" for name in ("SET-016", "SET-018", "SET-019", "SET-020")}


LOW_SET_IDS = ("SET-021", "SET-022")  # no art yet (DECISION-0138)


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
    elif pattern == "Cone":
        # Instant cone from the caster toward the nearest enemy (SET-022); the seed points it a random way with no enemy
        # in range (DECISION-0138 addendum 2026-10-02).
        level.update(baseDamage=effect["damage"], targetingMode="NearestEnemy", targetingRadius=effect["targetingRadius"],
                     randomSeed=seed)
        level["waves"] = [wave([{"kind": "Area", "radius": effect["radius"], "arcDegrees": effect["arcDegrees"]}],
                               controls(kb, effect["knockbackSeconds"]))]
    else:
        raise SystemExit(f"{set_id}: unknown set attack pattern {pattern}")
    template = {"id": SET_ATTACK_TEMPLATES[set_id], "displayName": content_design_names("SET")[set_id]}
    if set_id not in LOW_SET_IDS:
        template["iconVisualId"] = f"{set_id}-VISUAL-ICON"
    if set_id in SET_ATTACK_VISUALS:
        template["visualId"] = SET_ATTACK_VISUALS[set_id]
    template["levels"] = [level] * 6
    return template
