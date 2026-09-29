"""Compare descriptive, unpaired campaign series only when starting conditions match."""
from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

from analyze import analyze_experiment, read_json


CONDITION_KEYS = ("template", "characterId", "fieldRoute", "movementPolicy", "draftPolicy",
                  "purchasePolicy", "runSpeed", "maxExperimentWallSeconds", "runWallTimeoutSeconds",
                  "transitionTimeoutSeconds", "maxRunsPerChain", "stopAfterRouteClear")


def conditions(root: Path) -> tuple[dict, dict]:
    experiment = read_json(root / "experiment.json")
    config = experiment["config"]
    selected = {key: config.get(key) for key in CONDITION_KEYS}
    selected["presetSha256"] = experiment.get("presetSha256")
    return selected, experiment.get("build", {})


def compare(baseline: Path, candidate: Path) -> str:
    base_conditions, base_build = conditions(baseline)
    candidate_conditions, candidate_build = conditions(candidate)
    differences = [key for key in base_conditions if base_conditions[key] != candidate_conditions[key]]
    if differences:
        details = "; ".join(f"{key}: {base_conditions[key]!r} != {candidate_conditions[key]!r}"
                            for key in differences)
        raise ValueError("Incompatible starting conditions: " + details)
    first = analyze_experiment(baseline)
    second = analyze_experiment(candidate)
    a, b = first["summary"], second["summary"]
    lines = ["# Balance series comparison", "",
             f"Baseline: {baseline.resolve()} ({a['independentChains']} independent chains).",
             f"Candidate: {candidate.resolve()} ({b['independentChains']} independent chains).",
             "", "| Metric | Baseline | Candidate |", "|---|---:|---:|",
             f"| Natural wins | {a['wins']} | {b['wins']} |",
             f"| Natural losses | {a['losses']} | {b['losses']} |",
             f"| Incomplete runs | {a['incompleteRuns']} | {b['incompleteRuns']} |",
             f"| Win rate (natural W/L only) | {format_rate(a['winRate'])} | {format_rate(b['winRate'])} |",
             f"| Completed runs / 10 wall min | {format_number(a['completedRunsPer10Minutes'])} | "
             f"{format_number(b['completedRunsPer10Minutes'])} |",
             f"| Effective simulation speed | {format_number(a['effectiveSpeed'])}× | "
             f"{format_number(b['effectiveSpeed'])}× |", "",
             "Build/content fingerprints (different hashes are shown, not treated as matching):", "",
             f"- Baseline commit {base_build.get('commit', 'unknown')}, exe {base_build.get('executableSha256', 'unknown')}, "
             f"data {base_build.get('dataSha256', 'unknown')}.",
             f"- Candidate commit {candidate_build.get('commit', 'unknown')}, exe {candidate_build.get('executableSha256', 'unknown')}, "
             f"data {candidate_build.get('dataSha256', 'unknown')}.",
             "", "Runs within each chain share progression. Random seeds are unpaired; no p-value or causal conclusion is reported.",
             "Small samples and incomplete/censored milestones limit interpretation."]
    group_a = {(row["template"], row["fieldId"], row["characterId"], row["runIndex"],
                row["startUpgradeState"]): row for row in first["groups"]}
    group_b = {(row["template"], row["fieldId"], row["characterId"], row["runIndex"],
                row["startUpgradeState"]): row for row in second["groups"]}
    lines.extend(["", "## Comparable groups", "",
                  "| Template / field / hero / attempt / upgrades | Baseline W/L (N) | Candidate W/L (N) |",
                  "|---|---:|---:|"])
    for key in sorted(set(group_a) | set(group_b), key=str):
        left, right = group_a.get(key), group_b.get(key)
        label = " / ".join(str(part) for part in key)
        def counts(item):
            return f"{item['wins']}/{item['losses']} ({item['completed']}/{item['runs']})" if item else "not observed"
        lines.append(f"| {label} | {counts(left)} | {counts(right)} |")
    for name, result in (("Baseline", first), ("Candidate", second)):
        if result["excluded"]:
            lines.extend(["", f"{name} excluded/incomplete:"])
            lines.extend(f"- {row['chainId']}/{row['runId']}: {row['reason']}" for row in result["excluded"])
    return "\n".join(lines) + "\n"


def format_rate(value: float | None) -> str:
    return f"{value:.1%}" if value is not None else "unavailable"


def format_number(value: float | None) -> str:
    return f"{value:.2f}" if value is not None else "unavailable"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("baseline_directory", type=Path)
    parser.add_argument("candidate_directory", type=Path)
    args = parser.parse_args()
    try:
        print(compare(args.baseline_directory, args.candidate_directory), end="")
        return 0
    except (OSError, ValueError, KeyError, TypeError, json.JSONDecodeError) as error:
        print(f"Balance comparison: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
