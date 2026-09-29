import copy
import json
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import record
import run as balance_run
import validate_demonstration as recording


class DemonstrationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.template = Path(__file__).parent / "examples/human-demonstration.json"
        self.config = json.loads(self.template.read_text(encoding="utf-8"))

    def test_launcher_creates_unique_isolated_configs_without_launching_or_changing_template(self):
        original = self.template.read_bytes()
        first, output1 = record.prepare(self.template, self.root)
        second, output2 = record.prepare(self.template, self.root)
        self.assertNotEqual(first, second)
        self.assertNotEqual(output1, output2)
        self.assertFalse(output1.exists())
        data = json.loads(first.read_text(encoding="utf-8"))
        self.assertEqual(output1.name, data["outputDirectory"])
        self.assertEqual("human", data["movementPolicy"]["id"])
        self.assertEqual(1, data["runSpeed"])
        self.assertEqual(original, self.template.read_bytes())

    def test_preset_path_is_resolved_before_relocating_config(self):
        self.config["template"] = "preset"
        self.config["initialProfilePath"] = "profile.json"
        template = self.root / "preset.json"
        template.write_text(json.dumps(self.config), encoding="utf-8")
        config, _ = record.prepare(template, self.root / "output")
        actual = json.loads(config.read_text(encoding="utf-8"))
        self.assertEqual(str((self.root / "profile.json").resolve()), actual["initialProfilePath"])

    def test_invalid_recording_budgets_fail_before_output(self):
        for key, value in (("queueCapacity", 0), ("sampleIntervalSeconds", float("nan")),
                           ("maxSamples", True), ("maxFileMegabytes", 2049), ("maxEntitiesPerCollection", 0)):
            with self.subTest(key=key):
                config = copy.deepcopy(self.config)
                config["demonstration"][key] = value
                with self.assertRaises(ValueError):
                    balance_run.validate_recording(config)
        for key, value in (("runSpeed", 5), ("chains", 2), ("demonstration", None)):
            config = copy.deepcopy(self.config)
            config[key] = value
            with self.assertRaises(ValueError):
                balance_run.validate_recording(config)

    def test_human_requires_visual_before_output_or_child(self):
        output = self.root / "uncreated"
        with mock.patch.object(balance_run, "validate", return_value=(self.config, {}, "hash", None)), \
                mock.patch.object(balance_run, "launch") as launch:
            with self.assertRaisesRegex(ValueError, "requires --visual"):
                balance_run.run(self.template, self.root / "player.exe", output)
            launch.assert_not_called()
        self.assertFalse(output.exists())

    def rows(self, controller="human"):
        observation = {"player": {"position": [1, 2], "velocity": [0, 0], "movementSpeed": 3,
                                  "radius": 0.2, "pickupRadius": 0.5, "hp": 80, "maxHp": 100,
                                  "experience": 3, "level": 2, "damageMultiplier": 1,
                                  "xpMultiplier": 1, "actionSpeedBonus": 0},
                       "arenaBounds": [-10, -10, 10, 10], "viewportBounds": None,
                       "coverageComplete": True, "truncatedEntities": 0}
        observation.update({key: [] for key in ("threats", "enemyStates", "pickups", "obstacles", "beams", "skills", "build")})
        header = {"type": "header", "schemaVersion": 1, "controller": controller, "runId": "fixture",
                  "observationSchema": "demonstration-observation-v1", "actionSchema": "world-movement-intent-v1"}
        sample = {"type": "sample", "sampleIndex": 0, "physicsStep": 0, "physicsSeconds": 0,
                  "runSeconds": 0.01, "stepSeconds": 0.02, "runSpeed": 1, "action": [0.5, -0.25],
                  "previousAction": [0, 0], "observation": observation}
        footer = {"type": "end", "recordingComplete": True, "samples": 1, "rejectedSamples": 0,
                  "error": None, "truncatedObservationSamples": 0, "incompleteObservationSamples": 0,
                  "physicsSteps": 1, "reason": "Defeat", "outcome": "Defeat"}
        return [header, sample, footer]

    def write_rows(self, rows, name="demonstration.jsonl"):
        path = self.root / name
        path.write_text("".join(json.dumps(row) + "\n" for row in rows), encoding="utf-8")
        return path

    def test_validator_preserves_controller_quality_and_missing_provenance(self):
        rows = self.rows("bot")
        rows[1]["observation"]["truncatedEntities"] = 3
        rows[1]["observation"]["coverageComplete"] = False
        rows[2]["truncatedObservationSamples"] = 1
        rows[2]["incompleteObservationSamples"] = 1
        result = recording.validate(self.write_rows(rows))
        self.assertFalse(result["humanDemonstration"])
        self.assertFalse(result["buildProvenancePresent"])
        self.assertEqual(1, result["truncatedSamples"])
        self.assertEqual(1, result["incompleteSamples"])
        self.assertEqual(1, result["samples"])

    def test_validator_rejects_partial_missing_footer_counts_and_nonfinite(self):
        invalid = []
        rows = self.rows(); rows.pop(); invalid.append(rows)
        rows = self.rows(); rows[2]["samples"] = 2; invalid.append(rows)
        rows = self.rows(); rows[1]["action"][0] = float("nan"); invalid.append(rows)
        rows = self.rows(); rows[1]["action"] = [1, 1]; invalid.append(rows)
        rows = self.rows(); rows[1]["previousAction"] = [1, 1]; invalid.append(rows)
        rows = self.rows(); rows[1]["physicsStep"] = 0.5; invalid.append(rows)
        rows = self.rows(); rows[1]["runSeconds"] = -1; invalid.append(rows)
        rows = self.rows(); rows[1]["runSpeed"] = 5; invalid.append(rows)
        rows = self.rows(); rows[1]["runSpeed"] = True; invalid.append(rows)
        rows = self.rows(); rows.append(rows[1]); invalid.append(rows)
        rows = self.rows(); rows.insert(2, copy.deepcopy(rows[1])); invalid.append(rows)
        for index, rows in enumerate(invalid):
            with self.subTest(index=index), self.assertRaises(ValueError):
                recording.validate(self.write_rows(rows))
        with self.assertRaisesRegex(ValueError, "Partial"):
            recording.validate(self.write_rows(self.rows(), "demonstration.jsonl.partial"))

    def test_validator_accepts_nonconsecutive_physics_steps_but_not_reversed_time(self):
        rows = self.rows()
        next_sample = copy.deepcopy(rows[1])
        next_sample.update(sampleIndex=1, physicsStep=5, physicsSeconds=0.1, runSeconds=0.09)
        rows.insert(2, next_sample)
        rows[3].update(samples=2, physicsSteps=6)
        self.assertEqual(2, recording.validate(self.write_rows(rows))["samples"])
        next_sample["physicsSeconds"] = 0
        with self.assertRaisesRegex(ValueError, "clock"):
            recording.validate(self.write_rows(rows))


if __name__ == "__main__":
    unittest.main()
