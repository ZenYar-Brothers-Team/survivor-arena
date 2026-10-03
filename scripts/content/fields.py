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
    four = baseline["field004"]["field"]
    six = baseline["field006"]["field"]
    seven = baseline["field007"]["field"]
    walls = ["Wall_Top", "Wall_Bottom", "Wall_Left", "Wall_Right"]
    nine = baseline["field009"]["field"]
    zone_devs = [baseline["devZones"]["field"]]
    return {
        "defaultFieldId": field["id"], "availableFieldIds": [field["id"], two["id"], three["id"], four["id"], six["id"], seven["id"], nine["id"]] + [item["id"] for item in zone_devs],
        # The Gameplay scene keeps its baked walls and SpawnPoint (DECISION-0054 section 9); FIELD-002 reuses the scene.
        "environments": [{"id": field["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": "FIELD-002-ENVIRONMENT", "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": three["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": four["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                          "obstacleNames": walls},
                         {"id": seven["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint", "obstacleNames": walls},
                         {"id": six["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint", "obstacleNames": walls},
                         {"id": nine["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint", "obstacleNames": walls},
                         ] + [{"id": item["environmentId"], "sceneName": field["sceneName"], "spawnPointName": "SpawnPoint",
                               "obstacleNames": walls} for item in zone_devs],
        "fields": [{"id": field["id"], "displayName": names[field["id"]], "description": field["description"],
                    "thumbnailPlaceholder": field["thumbnailPlaceholder"], "difficulty": field["difficulty"],
                    "thumbnailVisualId": field["thumbnailVisualId"],
                    "unlockDescription": field["unlockDescription"], "environmentId": field["environmentId"],
                    "timelineId": field["timelineId"], "travelerScheduleId": field["travelerScheduleId"],
                    "finalBossId": field["finalBossId"], "midBossId": field["midBossId"],
                    "enemyIds": [e["id"] for e in baseline["enemies"]]},
                   # Map transfer (DECISION-0136): Content Design names slot 2 "Пограничные руины" and slot 3 "Королевский тракт";
                   # collection art stays keyed by field id, descriptions and gates stay with the slot.
                   {"id": two["id"], "displayName": names[two["id"]], "description": card_field(two["id"], "Роль"),
                    "thumbnailPlaceholder": three["thumbnailPlaceholder"], "difficulty": two["difficulty"],
                    "thumbnailVisualId": "FIELD-002-VISUAL-BACKGROUND",
                    "unlockDescription": "Пройдите «Деревенскую окраину» или убейте 3000 обычных врагов на ней", "environmentId": "FIELD-002-ENVIRONMENT",
                    "timelineId": "FIELD-002-TIMELINE", "travelerScheduleId": "FIELD-002-TRAVELERS",
                    "finalBossId": baseline["field002"]["boss"]["id"], "midBossId": baseline["field002"]["midboss"]["id"],
                    "enemyIds": baseline["field002"]["enemyPool"]},
                   {"id": three["id"], "displayName": names[three["id"]], "description": card_field(three["id"], "Роль"),
                    "thumbnailPlaceholder": "Королевский тракт", "difficulty": three["difficulty"],
                    "thumbnailVisualId": three["thumbnailVisualId"],
                    "unlockDescription": f"Пройдите «{names[two['id']]}»", "environmentId": three["environmentId"],
                    "timelineId": three["timelineId"], "travelerScheduleId": three["travelerScheduleId"],
                    "finalBossId": three["finalBossId"], "midBossId": three["midBossId"],
                    "enemyIds": baseline["field003"]["enemyPool"]},
                   {"id": four["id"], "displayName": names[four["id"]], "description": card_field(four["id"], "Роль"),
                    "thumbnailPlaceholder": four["thumbnailPlaceholder"], "difficulty": four["difficulty"],
                    "thumbnailVisualId": four["thumbnailVisualId"], "unlockDescription": f"Пройдите «{names[three['id']]}»",
                    "environmentId": four["environmentId"], "timelineId": four["timelineId"],
                    "travelerScheduleId": four["travelerScheduleId"], "finalBossId": four["finalBossId"],
                    "midBossId": four["midBossId"], "enemyIds": baseline["field004"]["enemyPool"]},
                   # DECISION-0142 follow-up: the academy is playable now, sharing FIELD-001 encounters by reference.
                   {"id": six["id"], "displayName": names[six["id"]], "description": six["description"],
                    "thumbnailPlaceholder": names[six["id"]], "difficulty": six["difficulty"],
                    "thumbnailVisualId": "FIELD-006-VISUAL-BACKGROUND", "unlockDescription": six["unlockDescription"],
                    "environmentId": six["environmentId"], "timelineId": field["timelineId"],
                    "travelerScheduleId": field["travelerScheduleId"], "finalBossId": field["finalBossId"],
                    "midBossId": field["midBossId"], "enemyIds": [e["id"] for e in baseline["enemies"]]},
                   {"id": seven["id"], "displayName": names[seven["id"]], "description": seven["description"],
                    "thumbnailPlaceholder": names[seven["id"]], "difficulty": seven["difficulty"],
                    "thumbnailVisualId": "FIELD-007-VISUAL-BACKGROUND", "unlockDescription": seven["unlockDescription"],
                    "environmentId": seven["environmentId"], "timelineId": field["timelineId"],
                    "travelerScheduleId": field["travelerScheduleId"], "finalBossId": field["finalBossId"],
                    "midBossId": field["midBossId"], "enemyIds": [e["id"] for e in baseline["enemies"]]},
                   {"id": nine["id"], "displayName": names[nine["id"]], "description": nine["description"],
                    "thumbnailPlaceholder": names[nine["id"]], "difficulty": nine["difficulty"],
                    "thumbnailVisualId": "FIELD-009-VISUAL-BACKGROUND", "unlockDescription": nine["unlockDescription"],
                    "environmentId": nine["environmentId"], "timelineId": field["timelineId"],
                    "travelerScheduleId": field["travelerScheduleId"], "finalBossId": field["finalBossId"],
                    "midBossId": field["midBossId"], "enemyIds": [e["id"] for e in baseline["enemies"]]},
                   ] + [
                   # Development-only effect-zone test field (zones): same shared FIELD-001 spawn settings.
                   {"id": item["id"], "displayName": item["displayName"], "description": item["description"],
                    "thumbnailPlaceholder": item["thumbnailPlaceholder"], "difficulty": item["difficulty"],
                    "thumbnailVisualId": field["thumbnailVisualId"], "unlockDescription": item["unlockDescription"],
                    "environmentId": item["environmentId"], "timelineId": field["timelineId"],
                    "travelerScheduleId": field["travelerScheduleId"], "finalBossId": field["finalBossId"],
                    "midBossId": field["midBossId"], "enemyIds": [e["id"] for e in baseline["enemies"]], "testField": True}
                   for item in zone_devs],
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
    if three["waterDecor"]["blocksMovement"]:
        raise SystemExit("FIELD-003 water must stay visual")
    # Map transfer (user instruction 2026-10-01): FIELD-003 takes the former FIELD-002 look and obstacle layout
    # (ground, decor, obstacle sprites, per-run patterns); its own id, environment and seeds stay.
    three_ground = "FIELD-003-VISUAL-GROUND"
    third = dict(second, id="FIELD-003-PRESENTATION", environmentId=three["environmentId"],
                 seed=data["seed"] + 2000, obstacleSeed=data["obstacleSeed"] + 2000)
    third["obstacles"] = list(second["obstacles"])
    second["groundVisualId"] = three_ground  # slot 2 now uses the ruins ground; slot 3 keeps the tract ground copied above
    four = baseline["field004"]["field"]
    fourth = dict(data, id="FIELD-004-PRESENTATION", environmentId=four["environmentId"],
                  groundVisualId="FIELD-004-VISUAL-GROUND", fenceVisualId="FIELD-004-VISUAL-PALISADE",
                  obstacleVisualId="FIELD-004-VISUAL-SUPPLY-CRATE", seed=data["seed"] + 3000,
                  obstacleSeed=data["obstacleSeed"] + 3000, interiorObstacleCount=40, nearObstacleCount=4)
    presentations = [data, second, third, fourth]
    # DECISION-0069: the denser village outskirts reuses the FIELD-002 rock and adds a barrel.
    data["barrelVisualId"] = "FIELD-001-VISUAL-BARREL"
    data["rockVisualId"] = "FIELD-002-VISUAL-BOULDER"
    data["interiorObstacleCount"] = 288
    # DECISION-0068: the first three fields generate their obstacles every run from patterns instead of a fixed list.
    wall = baseline["field"]["wallThickness"]
    for presentation in presentations:
        if presentation["id"] == "FIELD-002-PRESENTATION":
            continue  # FIELD-002 gets the generated illustrated blobs below
        if presentation["id"] == "FIELD-003-PRESENTATION":
            layout = baseline["layouts"]["FIELD-002-PRESENTATION"]  # the former second-map pattern layout
        elif presentation["id"] == "FIELD-004-PRESENTATION":
            layout = baseline["field004"]["layout"]
        else:
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
    # Map transfer (user instruction 2026-10-01, DECISION-0136): FIELD-002 keeps its art and decor, but its obstacles are
    # the per-run illustrated blobs of the former test field "Тест 02" in a compact arena.
    dev_packet = baseline["devBlobs"]
    dev_field = dev_packet["field"]
    layout = dev_packet["blobLayout"]
    # A library start blob is one of the listed blobs; a procedural one is an extra obstacle.
    count = len(layout["blobs"]) + (0 if layout["startScreen"].get("libraryIds") else 1)
    presentations[1].pop("obstacles")
    # The ruins map has no shrines or columns: a walkable shrine sprite read as an obstacle (the tract map keeps them as decor).
    for key in ("shrineVisualId", "columnVisualId", "shrineChance"):
        presentations[1].pop(key, None)
    presentations[1].update(interiorObstacleCount=count, nearObstacleCount=0,
                            arenaSideLength=dev_field["arenaSideLength"], blobLayout=layout)
    # Zone/altar previews include both regression fields and permanent FIELD-006/007 IDs.
    # The index keeps each field's seeds stable (index 1 belonged to the removed "Тест 07" altar field).
    for index, zones_packet in ((0, baseline["devZones"]), (2, baseline["field007"]), (3, baseline["field006"])):
        zones_field = zones_packet["field"]
        obstacles = zones_packet["obstacles"]
        library = [item for item in dev_packet["blobLayout"]["library"] if item["id"] in obstacles["libraryIds"]]
        if len(library) != len(obstacles["libraryIds"]):
            raise SystemExit("Dev zones obstacles reference unknown library items")
        # A field may also author its own illustrated obstacles (ownLibrary: id, visualId, outline points) and how many of each to place.
        own = obstacles.get("ownLibrary", [])
        copies = obstacles.get("copies", 1)
        if {item["id"] for item in own} & {item["id"] for item in library}:
            raise SystemExit("Own obstacle ids clash with shared library ids")
        # The runtime places each library item once, so a repeated prop is a numbered copy with the same art and outline.
        if copies > 1:
            own = [dict(item, id=f"{item['id']}-{n}") for item in own for n in range(1, copies + 1)]
        library = library + own
        blob_layout = {key: value for key, value in obstacles.items() if key not in ("libraryIds", "ownLibrary", "copies")}
        blob_layout.update(library=library, blobs=[{"libraryId": item["id"]} for item in library])
        zones = {key: value for key, value in presentations[0].items() if key not in ("obstacleLayout", "obstacles")}
        zones.update(id=zones_field["presentationId"], environmentId=zones_field["environmentId"],
                     groundVisualId=zones_field["groundVisualId"],
                     seed=presentations[0]["seed"] + 5000 + index * 1000,
                     obstacleSeed=presentations[0]["obstacleSeed"] + 5000 + index * 1000,
                     interiorObstacleCount=len(blob_layout["blobs"]), nearObstacleCount=0,
                     arenaSideLength=zones_field["arenaSideLength"], blobLayout=blob_layout, zoneLayout=zones_packet["zoneLayout"])
        if "decorationChance" in zones_packet:  # a field may switch off the shared first-map grass and bushes
            zones["decorationChance"] = zones_packet["decorationChance"]
        if not library:
            zones.pop("blobLayout", None)
            zones["decorationChance"] = 0
        if "altarPresentation" in zones_packet:
            zones["altarPresentation"] = zones_packet["altarPresentation"]
            zones["decorationChance"] = 0
            if not library:
                zones.pop("blobLayout", None)
        if "obstacleLayout" in zones_packet:
            zones.pop("blobLayout", None)
            zones["obstacleLayout"] = zones_packet["obstacleLayout"]
            layout = zones_packet["obstacleLayout"]
            cells = int((zones_field["arenaSideLength"] - 2 * layout["edgeMargin"]) // layout["cellSize"])
            zones["interiorObstacleCount"] = cells * cells * layout["patternsPerCell"]
        presentations.append(zones)
    # FIELD-009: flat-color circular platforms and bridges over a damaging void.
    platforms_packet = baseline["field009"]
    platforms_field = platforms_packet["field"]
    platforms = {key: value for key, value in presentations[0].items() if key not in ("obstacleLayout", "obstacles")}
    platforms.update(id=platforms_field["presentationId"], environmentId=platforms_field["environmentId"],
                     seed=presentations[0]["seed"] + 9000, obstacleSeed=presentations[0]["obstacleSeed"] + 9000,
                     interiorObstacleCount=1, nearObstacleCount=0, decorationChance=0,
                     arenaSideLength=platforms_field["arenaSideLength"], platformLayout=platforms_packet["platformLayout"])
    presentations.append(platforms)
    # Approved sparse road network replaces FIELD-003's former obstacle patterns only.
    third.pop("obstacleLayout", None)
    for key in ("columnVisualId", "shrineVisualId", "shrineChance"):
        third.pop(key, None)
    third.update(groundVisualId=data["groundVisualId"], arenaSideLength=baseline["field003Roads"]["roadLayout"]["arenaSideLength"],
                 interiorObstacleCount=1, nearObstacleCount=0, decorationChance=0,
                 roadLayout=baseline["field003Roads"]["roadLayout"],
                 roadFallbackLayouts=baseline["field003Roads"]["fallbackLayouts"])
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


def timeline_field004(baseline):
    return field_timeline(baseline["field004"]["timeline"], baseline["field004"]["field"], baseline["randomness"]["referenceSeeds"]["waves"] + 3000,
                          False, baseline.get("blobBreakupProfile"))


def field_timeline(t, field, seed, neutral_modifiers, blob_breakup_default=None):
    phases, clock = [], 0
    for p in t["phases"]:
        if p["startSeconds"] != clock:
def raid_profile(baseline):
    return baseline["raidProfile"]


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
                      "bookUpgradeCountWeights": baseline["lateTravelers"]["book"]["upgradeCountWeights"],
                      # Early-run guarantee: min active skills in drafts 1..4 of a run.
                      "activeSkillGuarantee": draft["activeSkillGuarantee"]},
            "experience": {"levelThresholds": xp["levelThresholds"], "baseDropLifetimeSeconds": xp["baseDropLifetimeSeconds"]},
            "hostileDamageMultiplier": baseline["combat"]["hostileDamageMultiplier"]}
