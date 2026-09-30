"""Evaluate a two-stage human movement model without selecting an in-game policy.

Each fold holds out an entire demonstration run. The gate predicts whether to
change direction; the direction model trains only on real changes. An action is
changed only when its joint probability exceeds the probability of keeping the
previous action. This is an exploratory offline metric, not a gameplay result.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
from sklearn.ensemble import HistGradientBoostingClassifier
from sklearn.metrics import average_precision_score, roc_auc_score

import train_imitation as base


def new_model() -> HistGradientBoostingClassifier:
    return HistGradientBoostingClassifier(max_iter=90, max_leaf_nodes=15,
                                          l2_regularization=2.0, random_state=34)


def select_actions(change_probability: np.ndarray, direction_probability: np.ndarray,
                   direction_classes: np.ndarray, previous: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    probabilities = np.zeros((len(previous), 9), dtype=np.float64)
    probabilities[:, direction_classes] = direction_probability
    probabilities[np.arange(len(previous)), previous] = 0.0
    probabilities /= np.maximum(probabilities.sum(axis=1, keepdims=True), 1e-12)
    direction = probabilities.argmax(axis=1)
    change = change_probability * probabilities[np.arange(len(previous)), direction] > 1.0 - change_probability
    return np.where(change, direction, previous), change


def evaluate(paths: list[Path], loader=None, feature_version: str = base.FEATURE_VERSION,
             model_name: str = "two-stage-hist-gradient-boosting-v1") -> dict:
    if len(paths) < 3:
        raise ValueError("At least three independent completed human runs are required")
    loader = loader or base.load_run
    loaded = [(path, *loader(path)) for path in paths]
    run_ids = [item[1]["runId"] for item in loaded]
    if len(set(run_ids)) != len(run_ids):
        raise ValueError("Each input must be a distinct run")
    for path, quality, *_ in loaded:
        if quality["outcome"] not in ("Defeat", "Victory") or quality["reason"] != quality["outcome"]:
            raise ValueError(f"Only naturally completed human runs are eligible: {path}")

    folds = []
    for held in range(len(loaded)):
        training = [item for index, item in enumerate(loaded) if index != held]
        train_x = np.concatenate([item[2] for item in training])
        train_y = np.concatenate([item[3] for item in training])
        train_previous = np.concatenate([item[4] for item in training])
        changed = train_y != train_previous
        gate = new_model().fit(train_x, changed.astype(np.int64))
        direction = new_model().fit(train_x[changed], train_y[changed])

        _, quality, test_x, target, previous = loaded[held]
        change_probability = gate.predict_proba(test_x)[:, list(gate.classes_).index(1)]
        direction_probability = direction.predict_proba(test_x)
        predicted, predicted_change = select_actions(change_probability, direction_probability,
                                                      direction.classes_, previous)
        actual_change = target != previous
        # Oracle timing diagnoses the direction model; it is never used to make a prediction.
        oracle_direction, _ = select_actions(np.ones(len(target)), direction_probability,
                                             direction.classes_, previous)
        folds.append({
            "heldOutRunId": quality["runId"],
            "model": base.score(target, predicted, previous),
            "previousActionBaseline": base.score(target, previous, previous),
            "changePrevalence": float(actual_change.mean()),
            "predictedChangeCount": int(predicted_change.sum()),
            "triggerPrecision": float((predicted_change & actual_change).sum() / max(predicted_change.sum(), 1)),
            "triggerRecall": float((predicted_change & actual_change).sum() / max(actual_change.sum(), 1)),
            "gateRocAuc": float(roc_auc_score(actual_change, change_probability)),
            "gateAveragePrecision": float(average_precision_score(actual_change, change_probability)),
            "oracleDirectionAccuracy": float((oracle_direction[actual_change] == target[actual_change]).mean()),
        })
    return {"schemaVersion": 1, "featureVersion": feature_version,
            "model": model_name, "selectedForGame": False,
            "inputRunSha256": {item[1]["runId"]: base.sha256(item[0]) for item in loaded},
            "folds": folds}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("recordings", nargs="+", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    report = evaluate(args.recordings)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "folds": report["folds"]}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
