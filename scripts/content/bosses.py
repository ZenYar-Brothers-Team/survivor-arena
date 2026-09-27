"""Production content: bosses. Numeric inputs live in the approved authoring sources."""
from content.sources import content_design_names
from content.actors import CADENCES


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


def boss_projectile_visual_id(content_id):
    """Each field pair shares the approved projectile family owned by its final boss."""
    field_number = int(content_id.rsplit("-", 1)[1])
    return f"BOSS-{field_number:03d}-VISUAL-PROJECTILE"


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


def dash_ring(ring, kb_seconds, visual_id):
    if ring["trigger"] != "dash-end" or ring["spreadDegrees"] != 360:
        raise SystemExit("dash volley must be a full ring on dash end")
    return {"pattern": "Ring", "damage": ring["damage"], "cooldownSeconds": 1, "projectileSpeed": ring["projectileSpeed"],
            "projectileLifetimeSeconds": ring["projectileLifetimeSeconds"], "projectileCount": ring["projectileCount"],
            "projectileRadius": ring["projectileRadius"], "telegraphSeconds": ring["telegraphSeconds"],
            "projectileVisualId": visual_id,
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
                 "dashEndAttack": dash_ring(entry["dashEndAttack"], entry["knockbackSeconds"],
                                             boss_projectile_visual_id(entry["id"]))}
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


def late_projectile(a, visual_id):
    data = {"pattern": a["pattern"], "damage": a["damage"], "cooldownSeconds": a["cooldownSeconds"] or 1,
            "projectileSpeed": a["projectileSpeed"], "projectileLifetimeSeconds": a["projectileLifetimeSeconds"],
            "projectileCount": a["projectileCount"], "projectileRadius": a["projectileRadius"],
            "telegraphSeconds": a["telegraphSeconds"], "cadence": "WindupStartToStart", "controls": late_controls(a),
            "projectileVisualId": visual_id}
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


def late_step(step_id, a, visual_id):
    if a["kind"] == "Projectile":
        return {"id": step_id, "attack": late_projectile(a, visual_id)}
    step = {"id": step_id, "cooldownSeconds": a["cooldownSeconds"]}
    step[{"Zone": "zone", "Beam": "beam", "Summon": "summon"}[a["kind"]]] = {
        "Zone": late_zone, "Beam": late_beam, "Summon": late_summon}[a["kind"]](a)
    return step


def late_dash_entries(items, visual_id):
    result = []
    for item in items:
        entry = {"delaySeconds": item["delaySeconds"], "orientation": ORIENTATIONS[item["orientation"]]}
        a = item["attack"]
        if a["kind"] == "Zone":
            entry["zone"] = late_zone(a)
        else:
            entry["attack"] = late_projectile(a, visual_id)
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
        projectile_visual_id = boss_projectile_visual_id(eid)
        extra = {}
        if entry["dashKnockback"] is not None:
            extra["dashContactControls"] = {"knockbackDistance": entry["dashKnockback"], "knockbackSeconds": entry["knockbackSeconds"]}
        dash_end = entry["dashEnd"]
        if dash_end:
            if dash_end["firesAfter"] != "last-dash-of-series":
                raise SystemExit(f"{eid}: dash-end timing not expressible by the runtime")
            extra["dashEndAttacks"] = late_dash_entries(dash_end["attacks"], projectile_visual_id)
            replacement = dash_end.get("belowHealthReplacement")
            if replacement:
                extra["dashEndReplacement"] = {"belowHealthFraction": replacement["belowHealth"],
                                               "attacks": late_dash_entries(replacement["attacks"], projectile_visual_id)}
        steps, phases = [], []
        for phase in entry["phases"]:
            if phase["thresholdComparison"] != "strictly-less":
                raise SystemExit(f"{eid}: phase comparison not expressible")
            ids = []
            for a in phase["attacks"]:
                step_id = f"{eid}-{phase['id']}-{a['id']}"
                step = late_step(step_id, a, projectile_visual_id)
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
                     "body": body(entry, late_movement(entry["movement"]), extra),
                     "attacks": steps, "phases": phases}
        if entry["holdRangedDuringDash"]:
            encounter["holdAttacksDuringDash"] = True
        if entry["teleport"]:
            encounter["teleport"] = boss_teleport(entry["teleport"])
        result.append(encounter)
    return result
