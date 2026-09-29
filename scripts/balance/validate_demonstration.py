"""Validate version-1 raw observation/action recordings. This does not judge player skill or train a model."""
from __future__ import annotations

import argparse
import json
import math
from pathlib import Path

MAX_LINE_BYTES = 8 * 1024 * 1024


def finite_tree(value) -> None:
    if isinstance(value, float) and not math.isfinite(value):
        raise ValueError("Non-finite value in recording")
    if isinstance(value, dict):
        for item in value.values():
            finite_tree(item)
    elif isinstance(value, list):
        for item in value:
            finite_tree(item)


def vector(value, name: str) -> None:
    if not isinstance(value, list) or len(value) != 2 or any(
            isinstance(v, bool) or not isinstance(v, (int, float)) or not math.isfinite(v) for v in value):
        raise ValueError(f"Invalid {name} vector")


def number(value, name: str, minimum: float = 0, integer: bool = False) -> None:
    allowed = (int,) if integer else (int, float)
    if isinstance(value, bool) or not isinstance(value, allowed) or not math.isfinite(value) or value < minimum:
        raise ValueError(f"Invalid {name}")


def bounds(value, name: str) -> None:
    if not isinstance(value, list) or len(value) != 4:
        raise ValueError(f"Invalid {name}")
    vector(value[:2], name)
    vector(value[2:], name)
    if value[0] >= value[2] or value[1] >= value[3]:
        raise ValueError(f"Empty {name}")


def validate(path: Path) -> dict:
    if path.name.endswith(".partial"):
        raise ValueError("Partial recording: keep for diagnosis, not unqualified training")
    count = 0
    last_step = -1
    last_physics = -1.0
    last_run = -1.0
    truncated = incomplete = stationary = 0
    header = footer = None
    with path.open("rb") as source:
        while True:
            line = source.readline(MAX_LINE_BYTES + 1)
            if not line:
                break
            if len(line) > MAX_LINE_BYTES or not line.endswith(b"\n"):
                raise ValueError("Oversized or incomplete JSONL line")
            item = json.loads(line)
            finite_tree(item)
            if not isinstance(item, dict):
                raise ValueError("Each JSONL record must be an object")
            kind = item.get("type")
            if header is None:
                if kind != "header" or item.get("schemaVersion") != 1 or item.get("controller") not in ("human", "bot"):
                    raise ValueError("Missing version-1 header/controller")
                if item.get("observationSchema") != "demonstration-observation-v1" or item.get("actionSchema") != "world-movement-intent-v1":
                    raise ValueError("Unsupported observation/action contract")
                header = item
                continue
            if footer is not None:
                raise ValueError("Data after terminal footer")
            if kind == "end":
                footer = item
                continue
            if kind != "sample" or item.get("sampleIndex") != count:
                raise ValueError("Sample ordering/index mismatch")
            number(item["sampleIndex"], "sample index", integer=True)
            step, physics, running = item["physicsStep"], item["physicsSeconds"], item["runSeconds"]
            number(step, "physics step", integer=True)
            number(physics, "physics time")
            number(running, "run time")
            number(item["stepSeconds"], "step duration")
            if step <= last_step or physics <= last_physics or running < last_run or item["stepSeconds"] <= 0:
                raise ValueError("Non-monotonic or invalid sample clock")
            number(item["runSpeed"], "run speed", integer=True)
            if item["runSpeed"] not in (1, 2, 3, 5) or (header["controller"] == "human" and item["runSpeed"] != 1):
                raise ValueError("Invalid speed for controller")
            vector(item["action"], "action")
            vector(item["previousAction"], "previousAction")
            if any(sum(v * v for v in item[key]) > 1.00001 for key in ("action", "previousAction")):
                raise ValueError("Action is outside the unit circle")
            if header.get("samplingPolicy") == "periodicOrActionChange/v1":
                reason = item.get("captureReason")
                changed = sum((a - b) ** 2 for a, b in zip(item["action"], item["previousAction"])) > 1e-10
                if reason not in ("periodic", "actionChange", "periodicAndActionChange") or \
                        (reason == "periodic" and changed) or (reason != "periodic" and not changed):
                    raise ValueError("Invalid action-change capture reason")
            elif header.get("samplingPolicy") is not None:
                raise ValueError("Unsupported sampling policy")
            observation = item["observation"]
            player = observation["player"]
            vector(player["position"], "player position")
            vector(player["velocity"], "player velocity")
            bounds(observation["arenaBounds"], "arena bounds")
            if observation["viewportBounds"] is not None:
                bounds(observation["viewportBounds"], "viewport bounds")
            for key in ("movementSpeed", "radius", "pickupRadius", "hp", "maxHp", "experience", "damageMultiplier", "xpMultiplier"):
                number(player[key], "player " + key)
            number(player["level"], "player level", minimum=1, integer=True)
            number(player["actionSpeedBonus"], "action speed bonus", minimum=-float("inf"))
            for collection in ("threats", "enemyStates", "pickups", "obstacles", "beams", "skills", "build"):
                if not isinstance(observation[collection], list):
                    raise ValueError(f"Invalid {collection} collection")
            for skill in observation["skills"]:
                number(skill["cooldownRemainingSeconds"], "remaining cooldown")
                number(skill["level"], "skill level", minimum=1, integer=True)
                number(skill["activations"], "skill activations", integer=True)
                vector(skill["aimDirection"], "skill aim")
            for threat in observation["threats"]:
                vector(threat["position"], "threat position")
                vector(threat["velocity"], "threat velocity")
                number(threat["radius"], "threat radius")
            for enemy in observation["enemyStates"]:
                vector(enemy["position"], "enemy position")
                number(enemy["hp"], "enemy hp")
                number(enemy["maxHp"], "enemy max hp")
            for pickup in observation["pickups"]:
                vector(pickup["position"], "pickup position")
                number(pickup["value"], "pickup value")
                if pickup["remainingSeconds"] is not None:
                    number(pickup["remainingSeconds"], "pickup lifetime")
            for obstacle in observation["obstacles"]:
                bounds(obstacle, "obstacle bounds")
            for beam in observation["beams"]:
                vector(beam["start"], "beam start")
                vector(beam["end"], "beam end")
                number(beam["halfWidth"], "beam half width")
            number(observation["truncatedEntities"], "truncation count", integer=True)
            if not isinstance(observation["coverageComplete"], bool):
                raise ValueError("Invalid observation coverage flag")
            truncated += observation["truncatedEntities"] > 0
            incomplete += observation["coverageComplete"] is not True
            stationary += item["action"] == [0, 0]
            last_step, last_physics, last_run = step, physics, running
            count += 1
    if header is None or footer is None or footer.get("recordingComplete") is not True or count == 0:
        raise ValueError("No clean non-empty recording footer")
    if footer.get("samples") != count or footer.get("rejectedSamples") != 0 or footer.get("error") is not None:
        raise ValueError("Footer counts/error mismatch")
    if footer.get("truncatedObservationSamples") != truncated or footer.get("incompleteObservationSamples") != incomplete:
        raise ValueError("Observation-quality counts mismatch")
    number(footer["physicsSteps"], "terminal physics steps", minimum=last_step + 1, integer=True)
    return {"path": str(path), "controller": header["controller"], "runId": header.get("runId"),
            "samples": count, "lastPhysicsSeconds": last_physics, "lastRunSeconds": last_run,
            "stationarySamples": stationary, "truncatedSamples": truncated, "incompleteSamples": incomplete,
            "buildProvenancePresent": isinstance(header.get("build"), dict), "outcome": footer.get("outcome"),
            "reason": footer.get("reason"), "humanDemonstration": header["controller"] == "human"}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("path", type=Path, help="A JSONL file or experiment directory")
    args = parser.parse_args()
    paths = sorted(args.path.rglob("demonstration.jsonl*")) if args.path.is_dir() else [args.path]
    if not paths:
        parser.error("No demonstration recordings found")
    failed = False
    for path in paths:
        try:
            print(json.dumps(validate(path), ensure_ascii=False))
        except (OSError, ValueError, KeyError, TypeError) as error:
            failed = True
            print(json.dumps({"path": str(path), "error": str(error)}, ensure_ascii=False))
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
