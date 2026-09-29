"""Descriptive balance analysis; profile chains, not runs, are independent units."""
from __future__ import annotations

import argparse
import csv
import io
import json
from collections import defaultdict
from pathlib import Path
import statistics
import sys

from run import write_json


def read_json(path: Path) -> dict:
    value = json.loads(path.read_text(encoding="utf-8"))
    if not isinstance(value, dict):
        raise ValueError(f"Expected JSON object: {path}")
    return value


def mean(values: list[float]) -> float | None:
    return statistics.mean(values) if values else None


def upgrade_state(profile: dict | None, character: str | None) -> str | None:
    if profile is None or character is None:
        return None
    suffix = ":" + character
    levels = profile.get("upgrades", {})
    return ";".join(f"{key[:-len(suffix)]}={value}" for key, value in sorted(levels.items())
                    if key.endswith(suffix) and value > 0) or "none"


def discover_ids(root: Path, manifest_chain: dict) -> list[str]:
    ids = {str(row.get("runId")) for row in manifest_chain.get("runs", []) if row.get("runId")}
    folder = root / "chains" / manifest_chain["chainId"] / "runs"
    if folder.is_dir():
        ids.update(path.name for path in folder.iterdir() if path.is_dir())
    return sorted(ids)


def load_run(root: Path, chain_id: str, run_id: str) -> dict:
    folder = root / "chains" / chain_id / "runs" / run_id
    row = {"chainId": chain_id, "runId": run_id, "status": "incomplete", "exclusionReason": None}
    sidecar_path = folder / "automation.json"
    if not sidecar_path.is_file():
        row["exclusionReason"] = "missing automation.json"
        return row
    try:
        auto = read_json(sidecar_path)
        if auto.get("schemaVersion") != 1 or auto.get("chainId") != chain_id or auto.get("runId") != run_id:
            raise ValueError("automation schema or identity mismatch")
        row.update({"runIndex": auto.get("runIndex"), "template": auto.get("template"),
                    "characterId": auto.get("characterId"), "fieldId": auto.get("fieldId"),
                    "runSpeed": auto.get("runSpeed"), "outcome": auto.get("outcome"),
                    "completionReason": auto.get("completionReason"), "simulationSeconds": auto.get("simulationSeconds"),
                    "botStuck": auto.get("botStuck"), "coverageIncomplete": auto.get("coverageIncomplete"),
                    "stopReason": auto.get("stopReason"), "error": auto.get("error"),
                    "reachedPhaseIndex": (auto.get("recorder") or {}).get("reachedPhaseIndex"),
                    "reachedPhaseId": (auto.get("recorder") or {}).get("reachedPhaseId"),
                    "terminalHp": (auto.get("recorder") or {}).get("terminalHp"),
                    "terminalLevel": (auto.get("recorder") or {}).get("terminalLevel"),
                    "terminalExperience": (auto.get("recorder") or {}).get("terminalExperience"),
                    "offerCount": (auto.get("recorder") or {}).get("offerCount"),
                    "selectionCount": (auto.get("recorder") or {}).get("selectionCount"),
                    "droppedEvents": (auto.get("recorder") or {}).get("droppedEvents"),
                    "purchases": auto.get("purchases") or [],
                    "phaseEvents": [event for event in (auto.get("recorder") or {}).get("events", [])
                                    if event.get("type") == "phase"]})
        before = folder / "profile-before.json"
        after = folder / "profile-after-purchases.json"
        reward = folder / "profile-after-reward.json"
        before_data = read_json(before) if before.is_file() else None
        after_data = read_json(after) if after.is_file() else read_json(reward) if reward.is_file() else None
        row["startUpgradeState"] = upgrade_state(before_data, row["characterId"])
        row["currencyBefore"] = before_data.get("currency") if before_data else None
        row["currencyAfter"] = after_data.get("currency") if after_data else None
        row["unlockedAfter"] = sorted(after_data.get("unlocked", [])) if after_data else None
        row["upgradesAfter"] = after_data.get("upgrades") if after_data else None
        telemetry_path = folder / "run.json"
        if not telemetry_path.is_file():
            row["exclusionReason"] = "missing run.json"
            return row
        telemetry = read_json(telemetry_path)
        if telemetry.get("runId", "").replace("-", "").lower() != run_id.lower():
            raise ValueError("telemetry run ID mismatch")
        row["timelineId"] = (telemetry.get("provenance") or {}).get("resolved", {}).get("timeline")
        row["damageDealt"] = telemetry.get("appliedDamageDealt")
        row["damageTaken"] = telemetry.get("appliedDamageTaken")
        row["actualHealing"] = telemetry.get("actualHealing")
        row["runningSeconds"] = telemetry.get("runningSeconds")
        row["pauseWallSeconds"] = telemetry.get("pauseWallSeconds")
        row["xpAwarded"] = ((telemetry.get("producers") or {}).get("xp") or {}).get("totals", {}).get("totalAwarded")
        row["telemetryDroppedEvents"] = (telemetry.get("quality") or {}).get("droppedTimelineEvents")
        if row["completionReason"] == "completed" and row["outcome"] in ("Victory", "Defeat") and \
           telemetry.get("outcome", {}).get("reason") == row["outcome"] and \
           (telemetry.get("quality") or {}).get("incompleteReason") is None:
            row["status"] = "completed"
        else:
            row["exclusionReason"] = row["error"] or row["stopReason"] or "non-natural or incomplete run"
        return row
    except (OSError, ValueError, TypeError, KeyError) as error:
        row["status"] = "corruptReport"
        row["exclusionReason"] = str(error)
        return row


def chain_summary(chain: dict, rows: list[dict], fields: list[str], initial: dict | None) -> dict:
    ordered = sorted(rows, key=lambda row: row.get("runIndex") or 10**9)
    result = {"chainId": chain["chainId"], "state": chain.get("state"),
              "stopReason": (chain.get("summary") or {}).get("stopReason"),
              "startedRuns": (chain.get("summary") or {}).get("startedRuns", len(ordered)),
              "completedRuns": sum(row["status"] == "completed" for row in ordered),
              "totalSimulationSeconds": sum(row.get("simulationSeconds") or 0 for row in ordered),
              "totalSpent": sum(purchase.get("Price", purchase.get("price", 0))
                                for row in ordered for purchase in row.get("purchases", [])),
              "routeCeiling": max((fields.index(row["fieldId"]) for row in ordered
                                   if row.get("fieldId") in fields), default=None),
              "milestones": {}}
    for field in fields:
        first_win = next((row for row in ordered if row["status"] == "completed" and
                          row.get("outcome") == "Victory" and row.get("fieldId") == field), None)
        initial_unlock = initial is not None and field in initial.get("unlocked", [])
        first_unlock = None if initial_unlock else next((row for row in ordered
                                                         if field in (row.get("unlockedAfter") or [])), None)
        result["milestones"][field] = {
            "firstWinRunIndex": first_win.get("runIndex") if first_win else None,
            "firstWinAttempts": sum(row.get("fieldId") == field for row in ordered
                                    if first_win and (row.get("runIndex") or 0) <= first_win["runIndex"]) if first_win else None,
            "simulationSecondsToFirstWin": sum(row.get("simulationSeconds") or 0 for row in ordered
                                                if first_win and (row.get("runIndex") or 0) <= first_win["runIndex"])
                                            if first_win else None,
            "winCensored": first_win is None,
            "firstUnlockRunIndex": 0 if initial_unlock else first_unlock.get("runIndex") if first_unlock else None,
            "simulationSecondsToFirstUnlock": 0 if initial_unlock else sum(
                row.get("simulationSeconds") or 0 for row in ordered
                if first_unlock and (row.get("runIndex") or 0) <= first_unlock["runIndex"])
                if first_unlock else None,
            "unlockCensored": not initial_unlock and first_unlock is None,
        }
    result["finalUpgradeState"] = upgrade_state({"upgrades": ordered[-1].get("upgradesAfter") or {}},
                                                 ordered[-1].get("characterId")) if ordered else None
    return result


def analyze_experiment(root: Path) -> dict:
    root = root.resolve()
    experiment = read_json(root / "experiment.json")
    manifest = read_json(root / "manifest.json")
    config = experiment["config"]
    chains = manifest.get("chains", [])
    rows = [load_run(root, chain["chainId"], run_id)
            for chain in chains for run_id in discover_ids(root, chain)]
    by_chain = defaultdict(list)
    for row in rows:
        by_chain[row["chainId"]].append(row)
    summaries = []
    for chain in chains:
        initial_path = root / "chains" / chain["chainId"] / "initial-profile.json"
        try:
            initial = read_json(initial_path) if initial_path.is_file() else None
        except (OSError, ValueError):
            initial = None
        summaries.append(chain_summary(chain, by_chain[chain["chainId"]], config["fieldRoute"], initial))
    wins = sum(row["status"] == "completed" and row.get("outcome") == "Victory" for row in rows)
    losses = sum(row["status"] == "completed" and row.get("outcome") == "Defeat" for row in rows)
    completed = wins + losses
    elapsed = manifest.get("elapsedWallSeconds")
    simulation = sum(row.get("simulationSeconds") or 0 for row in rows)
    grouped = defaultdict(list)
    for row in rows:
        key = (row.get("template"), row.get("fieldId"), row.get("characterId"), row.get("runIndex"),
               row.get("startUpgradeState"))
        grouped[key].append(row)
    groups = []
    for key, members in sorted(grouped.items(), key=lambda pair: str(pair[0])):
        natural = [row for row in members if row["status"] == "completed"]
        groups.append({"template": key[0], "fieldId": key[1], "characterId": key[2],
                       "runIndex": key[3], "startUpgradeState": key[4], "runs": len(members),
                       "completed": len(natural), "wins": sum(row["outcome"] == "Victory" for row in natural),
                       "losses": sum(row["outcome"] == "Defeat" for row in natural),
                       "meanSimulationSeconds": mean([row["simulationSeconds"] for row in natural
                                                       if isinstance(row.get("simulationSeconds"), (int, float))]),
                       "meanTerminalLevel": mean([row["terminalLevel"] for row in natural
                                                  if isinstance(row.get("terminalLevel"), (int, float))]),
                       "simulationSecondsDistribution": sorted(row["simulationSeconds"] for row in natural
                                                                if isinstance(row.get("simulationSeconds"), (int, float))),
                       "terminalLevelDistribution": sorted(row["terminalLevel"] for row in natural
                                                           if isinstance(row.get("terminalLevel"), (int, float))),
                       "reachedPhaseDistribution": sorted(row["reachedPhaseIndex"] for row in natural
                                                          if isinstance(row.get("reachedPhaseIndex"), int))})
    phases = []
    phase_groups = defaultdict(dict)
    phase_coverage = defaultdict(set)
    for row in rows:
        if row["status"] == "corruptReport" or row.get("reachedPhaseIndex") is None:
            continue
        if row.get("phaseEvents"):
            phase_coverage[(row.get("fieldId"), row.get("timelineId"))].add(row["runId"])
        for event in row.get("phaseEvents", []):
            key = (row.get("fieldId"), row.get("timelineId"), event.get("phaseIndex"), event.get("phaseId"))
            phase_groups[key].setdefault(row["runId"], (row, event))
    for key, by_run in sorted(phase_groups.items(),
                              key=lambda pair: (str(pair[0][0]), str(pair[0][1]),
                                                pair[0][2] if isinstance(pair[0][2], int) else -1,
                                                str(pair[0][3]))):
        observations = list(by_run.values())
        phases.append({"fieldId": key[0], "timelineId": key[1], "phaseIndex": key[2], "phaseId": key[3],
                       "observedRuns": len(phase_coverage[(key[0], key[1])]),
                       "reached": len(observations),
                       "deaths": sum(row["status"] == "completed" and row.get("outcome") == "Defeat" and
                                     row.get("reachedPhaseIndex") == key[2]
                                     for row, _ in observations),
                       "meanHpAtEntry": mean([event["hp"] for _, event in observations if isinstance(event.get("hp"), (int, float))]),
                       "meanLevelAtEntry": mean([event["level"] for _, event in observations if isinstance(event.get("level"), (int, float))])})
    reported_started = sum((chain.get("summary") or {}).get("startedRuns") or
                           (chain.get("progress") or {}).get("startedRuns") or
                           len(by_chain[chain["chainId"]]) for chain in chains)
    summary = {"experimentId": config["experimentId"], "template": config["template"],
               "requestedChains": manifest.get("requestedChains"), "independentChains": len(chains),
               "requestedRuns": manifest.get("requestedRuns"), "startedRuns": reported_started,
               "untrackedStartedRuns": max(0, reported_started - len(rows)),
               "completedRuns": completed, "wins": wins, "losses": losses,
               "incompleteRuns": sum(row["status"] != "completed" for row in rows) +
                                 max(0, reported_started - len(rows)),
               "failedChains": sum(chain.get("state") == "failed" for chain in chains),
               "winRate": wins / completed if completed else None,
               "experimentWallSeconds": elapsed, "simulationSeconds": simulation,
               "effectiveSpeed": simulation / elapsed if isinstance(elapsed, (int, float)) and elapsed > 0 else None,
               "completedRunsPer10Minutes": 600 * completed / elapsed if isinstance(elapsed, (int, float)) and elapsed > 0 else None,
               "meanCompletedSimulationSeconds": mean([row["simulationSeconds"] for row in rows
                                                        if row["status"] == "completed" and isinstance(row.get("simulationSeconds"), (int, float))]),
               "manifestState": manifest.get("state")}
    exclusions = [{"chainId": row["chainId"], "runId": row["runId"], "reason": row["exclusionReason"]}
                  for row in rows if row["status"] != "completed"]
    if reported_started > len(rows):
        exclusions.append({"chainId": "unknown", "runId": "unknown",
                           "reason": f"{reported_started - len(rows)} started run(s) lack an identifiable run ID"})
    return {"schemaVersion": 1, "summary": summary, "runs": rows, "chains": summaries,
            "groups": groups, "phases": phases,
            "excluded": exclusions}


def csv_text(rows: list[dict], fields: list[str]) -> str:
    output = io.StringIO(newline="")
    writer = csv.DictWriter(output, fieldnames=fields, extrasaction="ignore")
    writer.writeheader()
    for row in rows:
        writer.writerow({key: json.dumps(value, ensure_ascii=False) if isinstance(value, (dict, list)) else value
                         for key, value in row.items()})
    return output.getvalue()


def write_analysis(root: Path, result: dict) -> None:
    write_json(root / "analysis.json", result)
    from run import write_text
    run_fields = ["chainId", "runId", "runIndex", "status", "exclusionReason", "template", "fieldId",
                  "characterId", "startUpgradeState", "outcome", "simulationSeconds", "reachedPhaseIndex",
                  "terminalLevel", "damageDealt", "damageTaken", "actualHealing", "xpAwarded", "stopReason"]
    chain_fields = ["chainId", "state", "stopReason", "startedRuns", "completedRuns",
                    "totalSimulationSeconds", "totalSpent", "routeCeiling", "finalUpgradeState", "milestones"]
    write_text(root / "runs.csv", csv_text(result["runs"], run_fields))
    write_text(root / "chains.csv", csv_text(result["chains"], chain_fields))
    s = result["summary"]
    lines = [f"# Balance summary — {s['experimentId']}", "",
             f"Chains: {s['independentChains']} independent / {s['requestedChains']} requested. ",
             f"Runs: {s['completedRuns']} natural W/L ({s['wins']} W, {s['losses']} L), "
             f"{s['incompleteRuns']} incomplete / {s['requestedRuns']} requested.",
             f"Win rate: {s['winRate']:.1%}." if s["winRate"] is not None else "Win rate: unavailable (no natural W/L).",
             f"Effective speed: {s['effectiveSpeed']:.2f}×; completed runs/10 min: {s['completedRunsPer10Minutes']:.2f}."
             if s["effectiveSpeed"] is not None else "Effective speed/runs per 10 min: unavailable (no positive wall interval).",
             "", "Runs in the same chain share meta progression; these are descriptive statistics, not independent-run confidence intervals.",
             "Small samples are preliminary. Random gameplay seeds are unpaired. No causal skill-damage conclusion is implied.",
             "", "## Excluded or incomplete", ""]
    lines.extend(f"- {item['chainId']}/{item['runId']}: {item['reason']}" for item in result["excluded"])
    if not result["excluded"]:
        lines.append("None.")
    lines.extend(["", "## Groups", ""])
    for group in result["groups"]:
        lines.append(f"- {group['template']} / {group['fieldId']} / {group['characterId']} / attempt {group['runIndex']} / "
                     f"upgrades {group['startUpgradeState']}: {group['wins']} W, {group['losses']} L "
                     f"({group['completed']}/{group['runs']} natural).")
    write_text(root / "summary.md", "\n".join(lines) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("experiment_directory", type=Path)
    args = parser.parse_args()
    try:
        result = analyze_experiment(args.experiment_directory)
        write_analysis(args.experiment_directory, result)
        print(json.dumps(result["summary"], ensure_ascii=False, indent=2))
        return 0
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Balance analysis: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
