"""Probe whether past observations help detect human movement changes.

Only snapshots at or before each requested lag are used. The current action,
capture reason and future observations are never features. Output is offline
research evidence, not an in-game movement policy.
"""
from __future__ import annotations

import argparse
from bisect import bisect_right
import json
from pathlib import Path

import numpy as np

import probe_two_stage_imitation as two_stage
import train_imitation as base


def temporal_vector(current: np.ndarray, physics_seconds: float, lags: tuple[float, ...],
                    history_times: list[float], history_vectors: list[np.ndarray]) -> np.ndarray:
    result = [current]
    width = len(current) + 2  # Historical snapshot plus its then-known action.
    for lag in lags:
        index = bisect_right(history_times, physics_seconds - lag) - 1
        if index < 0:
            result.extend((np.zeros(width, dtype=np.float32), np.asarray([0.0], dtype=np.float32)))
        else:
            result.extend((history_vectors[index], np.asarray([1.0], dtype=np.float32)))
    return np.concatenate(result)


def load_run(path: Path, lags: tuple[float, ...]) -> tuple[dict, np.ndarray, np.ndarray, np.ndarray]:
    quality = base.validate_demonstration.validate(path)
    if not quality["humanDemonstration"] or quality["truncatedSamples"] or quality["incompleteSamples"]:
        raise ValueError(f"Only complete human observations are eligible: {path}")
    vectors, labels, previous = [], [], []
    history_times: list[float] = []
    history_vectors: list[np.ndarray] = []
    with path.open(encoding="utf-8") as source:
        next(source)
        for line in source:
            sample = json.loads(line)
            if sample["type"] != "sample":
                continue
            current = np.asarray(base.features(sample), dtype=np.float32)
            physics_seconds = float(sample["physicsSeconds"])
            vectors.append(temporal_vector(current, physics_seconds, lags, history_times, history_vectors))
            labels.append(base.direction_index(sample["action"]))
            previous.append(base.direction_index(sample["previousAction"]))
            history_times.append(physics_seconds)
            history_vectors.append(np.concatenate((current, np.asarray(sample["action"], dtype=np.float32))))
    return quality, np.asarray(vectors, dtype=np.float32), np.asarray(labels, dtype=np.int64), np.asarray(previous, dtype=np.int64)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("recordings", nargs="+", type=Path)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--lags", type=float, nargs="+", default=[0.4, 1.2],
                        help="Past observation ages in running physics seconds")
    args = parser.parse_args()
    lags = tuple(args.lags)
    if any(lag <= 0 for lag in lags) or list(lags) != sorted(set(lags)):
        parser.error("lags must be distinct, positive and ascending")
    report = two_stage.evaluate(args.recordings, loader=lambda path: load_run(path, lags),
                                feature_version=f"human-movement-temporal-v1:{','.join(map(str, lags))}",
                                model_name="two-stage-temporal-hist-gradient-boosting-v1")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "folds": report["folds"]}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
