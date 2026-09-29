import json
import tempfile
import unittest
from pathlib import Path

from analyze import analyze_experiment, write_analysis
from compare import compare


def put(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value), encoding="utf-8")


class BalanceAnalysisTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "baseline"
        self.config = {"experimentId": "synthetic", "template": "fresh", "characterId": "CHAR-001",
                       "fieldRoute": ["FIELD-001"], "movementPolicy": {"id": "safePickup", "version": 1},
                       "draftPolicy": {"id": "randomLegal", "version": 1},
                       "purchasePolicy": {"id": "cheapestPersonalUpgrade", "version": 1},
                       "runSpeed": 5, "maxExperimentWallSeconds": 600, "runWallTimeoutSeconds": 300,
                       "transitionTimeoutSeconds": 20, "maxRunsPerChain": 2, "stopAfterRouteClear": False}
        put(self.root / "experiment.json", {"config": self.config, "presetSha256": None,
                                            "build": {"commit": "base", "executableSha256": "abc", "dataSha256": "xyz"}})
        self.chains = [{"chainId": "chain-0001", "state": "completed", "runs": []},
                       {"chainId": "chain-0002", "state": "completed", "runs": []}]
        for chain in self.chains:
            put(self.root / "chains" / chain["chainId"] / "initial-profile.json",
                {"currency": 0, "unlocked": ["FIELD-001"], "upgrades": {}})

    def manifest(self, wall=100):
        put(self.root / "manifest.json", {"state": "completed", "requestedChains": 2, "requestedRuns": 4,
                                           "elapsedWallSeconds": wall, "chains": self.chains})

    def add_run(self, chain, run_id, index, outcome, seconds, phase, completed=True):
        folder = self.root / "chains" / chain / "runs" / run_id
        item = next(value for value in self.chains if value["chainId"] == chain)
        item["runs"].append({"runId": run_id})
        put(folder / "automation.json", {"schemaVersion": 1, "experimentId": "synthetic",
              "chainId": chain, "runId": run_id, "runIndex": index, "template": "fresh",
              "characterId": "CHAR-001", "fieldId": "FIELD-001", "runSpeed": 5,
              "outcome": outcome, "completionReason": "completed" if completed else "incomplete",
              "simulationSeconds": seconds, "recorder": {"reachedPhaseIndex": phase,
                  "reachedPhaseId": f"PHASE-{phase}", "terminalHp": 0 if outcome == "Defeat" else 10,
                  "terminalLevel": 3, "terminalExperience": 2, "offerCount": 2,
                  "selectionCount": 2, "droppedEvents": 0,
                  "events": [{"type": "phase", "phaseIndex": 0, "phaseId": "PHASE-0", "hp": 10, "level": 1},
                             {"type": "phase", "phaseIndex": phase, "phaseId": f"PHASE-{phase}",
                              "hp": 5, "level": 3}]}, "purchases": [], "stopReason": "timeout" if not completed else None})
        put(folder / "run.json", {"runId": run_id, "outcome": {"reason": outcome},
            "provenance": {"resolved": {"timeline": "TIMELINE-1"}}, "runningSeconds": seconds,
            "pauseWallSeconds": 0, "appliedDamageDealt": 100, "appliedDamageTaken": 20,
            "actualHealing": 2, "quality": {"incompleteReason": None, "droppedTimelineEvents": 0},
            "producers": {"xp": {"totals": {"totalAwarded": 12}}}})
        put(folder / "profile-before.json", {"currency": 0, "unlocked": ["FIELD-001"], "upgrades": {}})
        put(folder / "profile-after-purchases.json", {"currency": 5, "unlocked": ["FIELD-001"],
                                                      "upgrades": {"META-003:CHAR-001": 1}})

    def test_exact_denominators_speed_phases_and_censored_milestones(self):
        self.add_run("chain-0001", "a" * 32, 1, "Victory", 100, 1)
        self.add_run("chain-0001", "b" * 32, 2, "Aborted", 50, 0, completed=False)
        self.add_run("chain-0002", "c" * 32, 1, "Defeat", 200, 1)
        self.manifest()
        result = analyze_experiment(self.root)
        summary = result["summary"]
        self.assertEqual((1, 1, 1), (summary["wins"], summary["losses"], summary["incompleteRuns"]))
        self.assertEqual(0.5, summary["winRate"])
        self.assertEqual(3.5, summary["effectiveSpeed"])
        self.assertEqual(12, summary["completedRunsPer10Minutes"])
        self.assertEqual(2, summary["independentChains"])
        self.assertEqual(1, result["chains"][0]["milestones"]["FIELD-001"]["firstWinAttempts"])
        self.assertTrue(result["chains"][1]["milestones"]["FIELD-001"]["winCensored"])
        self.assertEqual(1, next(row for row in result["phases"] if row["phaseIndex"] == 1)["deaths"])
        self.assertEqual(12, next(row for row in result["runs"] if row["runId"] == "a" * 32)["xpAwarded"])
        self.assertEqual("timeout", result["excluded"][0]["reason"])
        write_analysis(self.root, result)
        self.assertIn("50.0%", (self.root / "summary.md").read_text(encoding="utf-8"))
        self.assertTrue((self.root / "runs.csv").is_file())

    def test_empty_and_all_incomplete_have_unavailable_rate(self):
        self.manifest(wall=0)
        empty = analyze_experiment(self.root)
        self.assertIsNone(empty["summary"]["winRate"])
        self.assertIsNone(empty["summary"]["effectiveSpeed"])
        self.add_run("chain-0001", "d" * 32, 1, "Aborted", 20, 0, completed=False)
        self.manifest()
        incomplete = analyze_experiment(self.root)
        self.assertIsNone(incomplete["summary"]["winRate"])
        self.assertEqual(0, incomplete["summary"]["completedRuns"])
        self.assertEqual(1, incomplete["summary"]["incompleteRuns"])

    def test_missing_and_corrupt_reports_are_never_silent_losses(self):
        self.chains[0]["runs"] = [{"runId": "missing"}, {"runId": "corrupt"}]
        folder = self.root / "chains" / "chain-0001" / "runs" / "corrupt"
        folder.mkdir(parents=True)
        (folder / "automation.json").write_text("not json", encoding="utf-8")
        self.manifest()
        result = analyze_experiment(self.root)
        self.assertEqual(0, result["summary"]["losses"])
        self.assertEqual(2, result["summary"]["incompleteRuns"])
        self.assertEqual(2, len(result["excluded"]))

    def test_phase_rows_follow_numeric_phase_order(self):
        self.add_run("chain-0001", "a" * 32, 1, "Defeat", 100, 10)
        self.add_run("chain-0002", "b" * 32, 1, "Defeat", 80, 2)
        self.manifest()
        result = analyze_experiment(self.root)
        self.assertEqual([0, 2, 10], [row["phaseIndex"] for row in result["phases"]])

    def test_compare_rejects_policy_or_speed_mismatch(self):
        self.add_run("chain-0001", "a" * 32, 1, "Victory", 100, 1)
        self.manifest()
        other = Path(self.temp.name) / "candidate"
        put(other / "experiment.json", {"config": dict(self.config), "presetSha256": None,
                                        "build": {"commit": "candidate"}})
        put(other / "manifest.json", {"state": "completed", "requestedChains": 0, "requestedRuns": 0,
                                      "elapsedWallSeconds": 100, "chains": []})
        self.assertIn("Random seeds are unpaired", compare(self.root, other))
        changed = dict(self.config)
        changed["runSpeed"] = 3
        put(other / "experiment.json", {"config": changed, "presetSha256": None})
        with self.assertRaisesRegex(ValueError, "runSpeed"):
            compare(self.root, other)


if __name__ == "__main__":
    unittest.main()
