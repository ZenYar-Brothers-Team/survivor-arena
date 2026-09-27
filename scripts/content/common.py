"""Production content: common. Numeric inputs live in the approved authoring sources."""



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
