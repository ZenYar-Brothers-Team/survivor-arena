"""Production content: actors. Numeric inputs live in the approved authoring sources."""
from content.sources import content_design_names


CHARACTER_BASELINE_ID = "CHARACTER-BASELINE-001"


def characters(baseline):
    """CHAR-001 from baseline v1 plus CHAR-002…010 from characters-v1 (DECISION-0089).

    Draft weights cover every implemented skill; locked ones are filtered by profile access."""
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
        "startingSkillBoost": character["startingSkillBoost"],
        "presentation": {"role": " · ".join(character["highlights"]), "baselineId": CHARACTER_BASELINE_ID,
                         "cropId": f"{character['id']}-VISUAL-PORTRAIT", "iconId": f"{character['id']}-VISUAL-ICON",
                         "highlights": []},
    }] + [late_character(entry, implemented, names) for entry in baseline["lateCharacters"]["characters"]]


# Later characters share CHAR-001's approved motion profile until a per-character profile is authored.
SHARED_CHARACTER_MOTION = "CHAR-001-MOTION"


def late_character(entry, implemented_skills, names):
    boosted, blocked = set(entry["boostedSkills"]), set(entry["blockedSkills"])
    boosted_passives, blocked_passives = set(entry["boostedPassives"]), set(entry["blockedPassives"])

    def weight(entry_id, up, down):
        return 1.35 if entry_id in up else 0 if entry_id in down else 1

    passive_ids = [f"PASSIVE-{i:03d}" for i in range(1, 15)]
    character_id = entry["id"]
    return {
        "id": character_id, "displayName": names[character_id],
        "initiallyUnlocked": False, "startingActiveSkillId": entry["startingSkill"],
        "visualId": f"{character_id}-VISUAL-BODY", "motionProfileId": SHARED_CHARACTER_MOTION,
        "baseStats": entry["stats"],
        "draftWeights": [{"skillId": skill, "weight": weight(skill, boosted, blocked)}
                         for skill in sorted(implemented_skills)],
        "passiveDraftWeights": [{"passiveId": passive, "weight": weight(passive, boosted_passives, blocked_passives)}
                                for passive in passive_ids],
        "startingSkillBoost": entry["startingSkillBoost"],
        "presentation": {"role": entry["role"], "baselineId": CHARACTER_BASELINE_ID,
                         "cropId": f"{character_id}-VISUAL-PORTRAIT", "iconId": f"{character_id}-VISUAL-ICON",
                         "highlights": entry["highlights"]},
    }


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
                            "ENEMY-006": "ENEMY-005-VISUAL-PROJECTILE",
                            "ENEMY-010": "ENEMY-010-VISUAL-PROJECTILE",
                            "ENEMY-011": "ENEMY-011-VISUAL-PROJECTILE",
                            "ENEMY-012": "ENEMY-012-VISUAL-PROJECTILE",
                            "ENEMY-014": "ENEMY-014-VISUAL-PROJECTILE",
                            "ENEMY-015": "ENEMY-015-VISUAL-PROJECTILE",
                            "ENEMY-018": "ENEMY-018-VISUAL-PROJECTILE",
                            "ENEMY-019": "ENEMY-019-VISUAL-PROJECTILE"}


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
        def to_runtime_movement(source):
            kind = source["kind"]
            runtime = {"kind": kind}
            if kind in ("KeepDistance", "DistanceReposition", "Orbit", "OffsetPursuit"):
                runtime.update(preferredDistance=source["preferredDistance"], distanceTolerance=source["distanceTolerance"])
            if kind in ("BlockedSidestep", "ArcPassPursuit"):
                runtime["preferredDistance"] = source["preferredDistance"]
            if kind in ("Orbit", "Zigzag", "BlockedSidestep", "ArcPassPursuit"):
                runtime["lateralStrength"] = source["lateralStrength"]
            if kind in ("Zigzag", "ApproachRetreat", "CommittedPursuit", "OffsetPursuit", "ArcPassPursuit"):
                runtime["cycleSeconds"] = source["cycleSeconds"]
            if kind in ("OffsetPursuit", "ArcPassPursuit"):
                runtime["directPursuitSeconds"] = source["directPursuitSeconds"]
            if kind == "BlockedSidestep":
                runtime.update(blockedTriggerSeconds=source["blockedTriggerSeconds"],
                               blockedProgressFraction=source["blockedProgressFraction"],
                               sidestepSeconds=source["sidestepSeconds"],
                               sidestepCooldownSeconds=source["sidestepCooldownSeconds"],
                               sidestepNearDistance=source["sidestepNearDistance"],
                               sidestepNearSeconds=source["sidestepNearSeconds"])
            if kind == "InertialPursuit":
                runtime["turnResponseSeconds"] = source["turnResponseSeconds"]
            if kind == "DistanceReposition":
                if source["cycleSeconds"] != source["holdingSeconds"] + source["repositionSeconds"] or not source["alternateLateralDirection"]:
                    raise SystemExit(f"{enemy['id']}: unsupported reposition cycle")
                runtime.update(lateralStrength=source["lateralStrength"], cycleSeconds=source["cycleSeconds"],
                               repositionSeconds=source["repositionSeconds"])
            if kind == "TelegraphedDash":
                if source["direction"] != "snapshot-at-telegraph-start":
                    raise SystemExit(f"{enemy['id']}: unsupported dash direction policy")
                runtime.update(dashTelegraphSeconds=source["dashTelegraphSeconds"],
                               dashDurationSeconds=source["dashDurationSeconds"],
                               dashCooldownSeconds=source["dashCooldownSeconds"],
                               dashSpeedMultiplier=source["dashSpeedMultiplier"])
                if "showDashTelegraphLine" in source:
                    runtime["showDashTelegraphLine"] = source["showDashTelegraphLine"]
            return runtime

        kind = movement["kind"]
        runtime_movement = to_runtime_movement(movement)
        if kind == "TelegraphedDash":
            entry["dashContactControls"] = {"knockbackDistance": movement["dashKnockback"], "knockbackSeconds": kb_seconds}
        entry["movement"] = runtime_movement
        if "movementVariants" in enemy:
            entry["movementVariants"] = [
                {"chance": variant["chance"], "movement": to_runtime_movement(variant["movement"])}
                for variant in enemy["movementVariants"]
            ]
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


# In-game accepted presentation scales (docs/playtests/2026-09-22_visual-acceptance.md),
# with XP reduced by DECISION-0117, win over the review format's neutral 1.0.
# Other gameplay values below come from the baseline (DECISION-0054 section 6).
ACCEPTED_PICKUP_VISUAL_SCALES = {"Potion": 0.68, "Book": 0.7, "experience": 0.527}


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


ROLE_COLORS = {"Offensive": [1, 0.6, 0.15, 1], "Wanderer": [0.3, 0.85, 1, 1], "Protector": [0.65, 0.4, 1, 1]}


def late_traveler(t, baseline):
    """TRAVELER-003/004/006…010 from travelers-v1 (DECISION-0088): one progression tier with the FIELD-001 peers."""
    knockback_seconds = baseline["controls"]["nonzeroKnockbackSeconds"]
    body = {"id": t["id"], "knockbackResistance": t["knockbackResistance"], "maxHealth": t["maxHealth"],
            "visualId": f"{t['id']}-VISUAL-BODY", "motionProfileId": "ENEMY-001-MOTION",
            "collisionSize": t["collisionSize"], "movementSpeed": t["movementSpeed"],
            "contactDamage": t["contactDamage"], "contactDamageInterval": 1,
            "experienceReward": t["experienceReward"], "movement": t["movement"],
            "contactControls": {"knockbackDistance": t["contactKnockback"], "knockbackSeconds": knockback_seconds}}
    if t["dashContactKnockback"] is not None:
        body["dashContactControls"] = {"knockbackDistance": t["dashContactKnockback"], "knockbackSeconds": knockback_seconds}
    if t["attack"] is not None:
        attack = dict(t["attack"])
        attack["controls"] = {"knockbackDistance": attack.pop("knockbackDistance"), "knockbackSeconds": knockback_seconds}
        body["attack"] = attack
    support = t["support"]
    return {
        "id": t["id"], "name": content_design_names("TRAVELER")[t["id"]], "marker": t["marker"], "role": t["role"],
        "body": body, "presenceSeconds": t["presenceSeconds"], "wanderSeconds": t["wanderSeconds"],
        "restSeconds": t["restSeconds"], "avoidRadius": t["avoidRadius"], "avoidSeconds": t["avoidSeconds"],
        "guardOffset": 1, "support": support["kind"], "supportRadius": support["radius"],
        "reduction": support["reduction"], "resistance": support["resistance"], "shieldHp": support["shieldHp"],
        "shieldSeconds": support["shieldSeconds"], "supportCooldown": support["cooldownSeconds"],
        "supportTargets": support["supportTargets"], "color": ROLE_COLORS[t["role"]],
    }


def apply_travelers_v2(entry, v2):
    """DECISION-0120: movement style of peaceful Travelers, and Aura/SpeedBurst/Heal support of protectors."""
    override = v2["overrides"].get(entry["id"], {})
    entry["movementStyle"] = override.get("movementStyle", "Wander")
    entry.update(override.get("style", {}))
    if "avoidRadius" in override:
        entry["avoidRadius"] = override["avoidRadius"]
    if "support" in override:
        support = dict(override["support"])
        entry["support"] = support.pop("kind")
        entry["supportRadius"] = support.pop("radius")
        entry["supportCooldown"] = support.pop("cooldownSeconds")
        for key, value in support.items():
            entry[key] = value
    if "effectColor" in override:
        entry["effectColor"] = override["effectColor"]
        entry["effectShape"] = override["effectShape"]
    if entry["support"] != "None":
        entry["supportVerticalScale"] = v2["supportVerticalScale"]
    return entry


def travelers(baseline):
    v2 = baseline["travelersV2"]
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
    result += [late_traveler(t, baseline) for t in baseline["lateTravelers"]["travelers"]]
    result = [apply_travelers_v2(t, v2) for t in result]
    pool = sorted(t["id"] for t in result)

    def entry(schedule_id, seed, rank):
        # DECISION-0063: every field draws from the global pool of implemented Travelers, roles never repeat.
        return {"id": schedule_id, "travelerIds": pool,
                "minCount": v2["schedule"]["minCount"], "countProbabilities": v2["schedule"]["countProbabilities"],
                "seed": seed, "fieldRank": rank,
                "placementAttempts": schedule["placementAttempts"], "endBufferSeconds": schedule["endBufferSeconds"],
                "spawnScreenHeights": schedule["spawnScreenHeights"], "fieldGrowth": schedule["fieldGrowth"],
                "timeGrowth": schedule["timeGrowth"], "initialHealthMultiplier": schedule["initialHealthMultiplier"]}
    for key in ("field002", "field003"):
        if not baseline[key]["travelers"]["pool"].startswith("global"):
            raise SystemExit(f"{key} Traveler pool must be the global pool")
    return {"travelers": result, "schedules": [entry(schedule["id"], seeds["travelers"], schedule["fieldRank"]),
                                               entry("FIELD-002-TRAVELERS", seeds["travelers"] + 1000, 2),
                                               entry("FIELD-003-TRAVELERS", seeds["travelers"] + 2000, 3)]}
