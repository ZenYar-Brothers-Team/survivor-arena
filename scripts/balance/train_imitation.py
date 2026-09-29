"""Train and evaluate an offline movement imitation candidate from clean human runs.

The JSON artifact is research output, not an automatically selected game policy.
Each validation fold holds out a whole run to avoid adjacent-frame leakage.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path

import numpy as np
import sklearn
from sklearn.metrics import accuracy_score, f1_score
from sklearn.neural_network import MLPClassifier
from sklearn.preprocessing import StandardScaler

import validate_demonstration


DIRECTIONS = np.asarray([(0, 0), (1, 0), (0, 1), (-1, 0), (0, -1),
                         (2**-0.5, 2**-0.5), (-2**-0.5, 2**-0.5),
                         (-2**-0.5, -2**-0.5), (2**-0.5, -2**-0.5)], dtype=np.float32)
FEATURE_VERSION = "human-movement-features-v1"


def direction_index(vector: list[float]) -> int:
    if vector[0] * vector[0] + vector[1] * vector[1] < 0.01:
        return 0
    return 1 + int(np.argmax(DIRECTIONS[1:] @ np.asarray(vector, dtype=np.float32)))


def _nearest(collection: list[dict], position: list[float], radius: float, key: str = "position") -> tuple[float, float, float, dict | None]:
    best = None
    best_squared = radius * radius
    dx = dy = 0.0
    for item in collection:
        point = item[key]
        x, y = point[0] - position[0], point[1] - position[1]
        squared = x * x + y * y
        if squared < best_squared:
            best, best_squared, dx, dy = item, squared, x, y
    return dx / radius, dy / radius, math.sqrt(best_squared) / radius, best


def features(sample: dict) -> list[float]:
    observation = sample["observation"]
    player = observation["player"]
    position = player["position"]
    threats = observation["threats"]
    pickups = observation["pickups"]
    bounds = observation["arenaBounds"]
    result = [*sample["previousAction"],
              player["velocity"][0] / max(player["movementSpeed"], 0.1),
              player["velocity"][1] / max(player["movementSpeed"], 0.1),
              max(-1.0, min(1.0, position[0] / 100.0)),
              max(-1.0, min(1.0, position[1] / 100.0)),
              min(sample["runSeconds"] / 900.0, 1.0),
              min(player["level"] / 45.0, 1.0),
              player["hp"] / max(player["maxHp"], 1.0),
              min(len(threats) / 200.0, 1.0),
              min(len(pickups) / 150.0, 1.0),
              min((position[0] - bounds[0]) / 12.0, 1.0),
              min((bounds[2] - position[0]) / 12.0, 1.0),
              min((position[1] - bounds[1]) / 12.0, 1.0),
              min((bounds[3] - position[1]) / 12.0, 1.0)]
    tx, ty, td, closest_threat = _nearest(threats, position, 12.0)
    result.extend((tx, ty, td if closest_threat else 1.0))
    velocity = closest_threat.get("velocity", [0, 0]) if closest_threat else [0, 0]
    result.extend((velocity[0] / 5.0, velocity[1] / 5.0))
    xp = [item for item in pickups if item["isExperience"]]
    px, py, pd, closest_pickup = _nearest(xp, position, 12.0)
    result.extend((px, py, pd if closest_pickup else 1.0))
    # Eight local direction bins at two ranges. The values are bounded to keep
    # very dense late waves from dominating the normalized feature scale.
    for collection, scales in ((threats, (4.0, 10.0)), (xp, (4.0, 10.0))):
        bins = [[0.0] * 8 for _ in scales]
        for item in collection:
            x, y = item["position"][0] - position[0], item["position"][1] - position[1]
            distance = math.hypot(x, y)
            if distance < 0.001:
                continue
            sector = int((math.atan2(y, x) + math.pi / 8) / (math.pi / 4)) % 8
            for index, scale in enumerate(scales):
                if distance < scale:
                    bins[index][sector] += 1.0 - distance / scale
        for band in bins:
            result.extend(min(value / 8.0, 1.0) for value in band)
    # Distance to each nearby obstacle edge, split into four cardinal sectors.
    block = [0.0] * 4
    for rect in observation["obstacles"]:
        x = min(max(position[0], rect[0]), rect[2]) - position[0]
        y = min(max(position[1], rect[1]), rect[3]) - position[1]
        distance = math.hypot(x, y)
        if distance < 4.0:
            sector = (0 if x >= 0 else 2) if abs(x) >= abs(y) else (1 if y >= 0 else 3)
            block[sector] = max(block[sector], 1.0 - distance / 4.0)
    result.extend(block)
    return result


def load_run(path: Path) -> tuple[dict, np.ndarray, np.ndarray, np.ndarray]:
    quality = validate_demonstration.validate(path)
    if not quality["humanDemonstration"] or quality["truncatedSamples"] or quality["incompleteSamples"]:
        raise ValueError(f"Only complete human observations are eligible: {path}")
    vectors, labels, previous = [], [], []
    with path.open(encoding="utf-8") as source:
        next(source)
        for line in source:
            sample = json.loads(line)
            if sample["type"] != "sample":
                continue
            vectors.append(features(sample))
            labels.append(direction_index(sample["action"]))
            previous.append(direction_index(sample["previousAction"]))
    return quality, np.asarray(vectors, dtype=np.float32), np.asarray(labels, dtype=np.int64), np.asarray(previous, dtype=np.int64)


def score(target: np.ndarray, predicted: np.ndarray, previous: np.ndarray) -> dict:
    changing = target != previous
    cosine = (DIRECTIONS[target] * DIRECTIONS[predicted]).sum(axis=1)
    return {"samples": int(len(target)), "accuracy": float(accuracy_score(target, predicted)),
            "macroF1": float(f1_score(target, predicted, labels=list(range(9)), average="macro", zero_division=0)),
            "meanDirectionDot": float(cosine.mean()),
            "changedSamples": int(changing.sum()),
            "changedAccuracy": float((target[changing] == predicted[changing]).mean()) if changing.any() else None}


def fit(train_x: np.ndarray, train_y: np.ndarray) -> tuple[StandardScaler, MLPClassifier]:
    scaler = StandardScaler().fit(train_x)
    model = MLPClassifier(hidden_layer_sizes=(48, 24), activation="tanh", alpha=0.05,
                          batch_size=256, learning_rate_init=0.001, max_iter=90,
                          random_state=34, early_stopping=True, n_iter_no_change=8,
                          validation_fraction=0.15)
    model.fit(scaler.transform(train_x), train_y)
    return scaler, model


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for chunk in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def train(paths: list[Path], output: Path) -> dict:
    if len(paths) < 3:
        raise ValueError("At least three independent human runs are required for leave-one-run-out evaluation")
    loaded = [load_run(path) for path in paths]
    run_ids = [item[0]["runId"] for item in loaded]
    if len(set(run_ids)) != len(paths):
        raise ValueError("Each input must be a distinct run")
    folds = []
    for held in range(len(loaded)):
        x = np.concatenate([item[1] for i, item in enumerate(loaded) if i != held])
        y = np.concatenate([item[2] for i, item in enumerate(loaded) if i != held])
        scaler, model = fit(x, y)
        quality, test_x, test_y, previous = loaded[held]
        predicted = model.predict(scaler.transform(test_x))
        folds.append({"heldOutRunId": quality["runId"], "model": score(test_y, predicted, previous),
                      "previousActionBaseline": score(test_y, previous, previous),
                      "iterations": int(model.n_iter_)})
    all_x = np.concatenate([item[1] for item in loaded])
    all_y = np.concatenate([item[2] for item in loaded])
    scaler, model = fit(all_x, all_y)
    hashes = {item[0]["runId"]: sha256(path)
              for item, path in zip(loaded, paths)}
    artifact = {"schemaVersion": 1, "featureVersion": FEATURE_VERSION,
                "inputRunSha256": hashes, "directionOrder": DIRECTIONS.tolist(),
                "featureCount": int(all_x.shape[1]), "classes": model.classes_.tolist(),
                "scalerMean": scaler.mean_.tolist(), "scalerScale": scaler.scale_.tolist(),
                "weights": [weight.tolist() for weight in model.coefs_],
                "biases": [bias.tolist() for bias in model.intercepts_]}
    report = {"schemaVersion": 1, "featureVersion": FEATURE_VERSION,
              "sklearnVersion": sklearn.__version__, "numpyVersion": np.__version__,
              "runIds": run_ids, "folds": folds, "finalIterations": int(model.n_iter_),
              "selectedForGame": False,
              "note": "Offline imitation only; no closed-loop game performance claim"}
    output.mkdir(parents=True, exist_ok=True)
    (output / "model.json").write_text(json.dumps(artifact, separators=(",", ":")), encoding="utf-8")
    (output / "evaluation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    return report


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("recordings", nargs="+", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    report = train(args.recordings, args.output)
    print(json.dumps(report, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
