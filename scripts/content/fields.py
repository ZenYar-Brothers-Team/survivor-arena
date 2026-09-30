"""Production content: fields. Numeric inputs live in the approved authoring sources."""
import json
from content.sources import ROOT, card_field, content_design_names


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
                   {"id": three["id"], "displayName": names[three["id"]], "description": card_field(three["id"], "Роль"),
                    "thumbnailPlaceholder": three["thumbnailPlaceholder"], "difficulty": three["difficulty"],
                    "thumbnailVisualId": three["thumbnailVisualId"],
                    "unlockDescription": three["unlockDescription"], "environmentId": three["environmentId"],
                    "timelineId": three["timelineId"], "travelerScheduleId": three["travelerScheduleId"],
                    "finalBossId": three["finalBossId"], "midBossId": three["midBossId"],
                    "enemyIds": baseline["field003"]["enemyPool"]}],
    }


def field_presentation(baseline):
    """Accepted field art/decor values plus per-run obstacle layouts."""
    def layout_piece(piece):
        result = {"kind": piece["kind"], "x": piece["x"], "y": piece["y"],
                  "width": piece["width"], "height": piece["height"]}
        if "visualId" in piece:
            result["visualId"] = piece["visualId"]
        if "visualIds" in piece:
            result["visualIds"] = piece["visualIds"]
        return result

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
                 groundVisualId="FIELD-003-VISUAL-GROUND", fenceVisualId="FIELD-003-VISUAL-WALL",
                 obstacleVisualId="FIELD-003-VISUAL-RUBBLE", bushVisualId="FIELD-003-VISUAL-WATER",
                 seed=data["seed"] + 2000, obstacleSeed=data["obstacleSeed"] + 2000,
                 interiorObstacleCount=len(three["obstacles"]),
                 nearObstacleCount=sum(1 for o in three["obstacles"] if abs(o["x"]) <= 20 and abs(o["y"]) <= 20))
    third["obstacles"] = [{"id": o["id"], "kind": ruin_kinds[o["kind"]], "x": o["x"], "y": o["y"], "width": o["width"],
                           "height": o["height"]} for o in three["obstacles"]]
    presentations = [data, second, third]
    # DECISION-0069: the denser village outskirts reuses the FIELD-002 rock and adds a barrel.
    data["barrelVisualId"] = "FIELD-001-VISUAL-BARREL"
    data["rockVisualId"] = "FIELD-002-VISUAL-BOULDER"
    data["interiorObstacleCount"] = 288
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
                          "pieces": [layout_piece(piece) for piece in pattern["pieces"]]}
                         for pattern in layout["patterns"]]}
        if "startScreen" in layout:
            presentation["obstacleLayout"]["startScreen"] = layout["startScreen"]
    return presentations


def minutes(seconds):
    return f"{int(seconds // 60)}:{int(seconds % 60):02d}"


def blob_breakup_profile(baseline):
    return baseline["blobBreakupProfile"]


def timeline(baseline):
    return field_timeline(baseline["timeline"], baseline["field"], baseline["randomness"]["referenceSeeds"]["waves"], True,
                          baseline.get("blobBreakupProfile"))


def timeline_field002(baseline):
    return field_timeline(baseline["field002"]["timeline"], baseline["field002"]["field"], baseline["randomness"]["referenceSeeds"]["waves"] + 1000,
                          False, baseline.get("blobBreakupProfile"))


def timeline_field003(baseline):
    return field_timeline(baseline["field003"]["timeline"], baseline["field003"]["field"], baseline["randomness"]["referenceSeeds"]["waves"] + 2000,
                          False, baseline.get("blobBreakupProfile"))


def field_timeline(t, field, seed, neutral_modifiers, blob_breakup_default=None):
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
                 "spawnIntervalSeconds": p["spawnIntervalSeconds"],
                 "composition": [{"enemyId": k, "weight": v} for k, v in p["composition"].items() if v > 0],
                 "modifiers": modifiers}
        if "blobBreakup" in p or blob_breakup_default is not None:
            breakup = p["blobBreakup"] if "blobBreakup" in p else {}
            if breakup is not None:
                if blob_breakup_default is None:
                    raise SystemExit(f"{p['id']}: blobBreakup requires blobBreakupProfile")
                if not isinstance(breakup, dict) or set(breakup) - {"profileId", "enemyIds"}:
                    raise SystemExit(f"{p['id']}: blobBreakup accepts only profileId and enemyIds")
                breakup = {"profileId": breakup.get("profileId", blob_breakup_default["id"]),
                           **({"enemyIds": breakup["enemyIds"]} if "enemyIds" in breakup else {})}
            phase["blobBreakup"] = breakup
        if p["burst"]:
            phase["burst"] = p["burst"]
        phases.append(phase)
    if clock != field["durationSeconds"]:
        raise SystemExit("timeline must cover the whole field duration")
    result = {"id": t["id"], "seed": seed, "maxAliveEnemies": t["maxAliveEnemies"],
            "spawnRadius": field["spawnRadius"], "spawnOppositeBias": field["spawnOppositeBias"],
            "openingSpawn": field["openingSpawn"],
            "phases": phases, "hooks": t["hooks"]}
    if "openingIntensity" in field:
        result["openingIntensity"] = field["openingIntensity"]
    return result


def run_setup(baseline):
    draft, xp = baseline["draft"], baseline["experience"]
    return {"startingCharacterId": baseline["character"]["id"],
            "draft": {"offerCount": draft["offerCount"], "setDraftChance": draft["setDraftChance"],
                      "seed": baseline["randomness"]["referenceSeeds"]["draft"], "initialRerolls": draft["initialRerolls"],
                      "initialBanishes": draft["initialBanishes"], "emptyBookCurrency": draft["emptyBookCurrency"],
                      # DECISION-0093: a Traveler Book grants 1…3 choices by these weights.
                      "bookUpgradeCountWeights": baseline["lateTravelers"]["book"]["upgradeCountWeights"]},
            "experience": {"levelThresholds": xp["levelThresholds"], "baseDropLifetimeSeconds": xp["baseDropLifetimeSeconds"]},
            "hostileDamageMultiplier": baseline["combat"]["hostileDamageMultiplier"]}
