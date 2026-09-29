import json
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import run as balance_run


class FakeChild:
    pid = 12345

    def __init__(self, output, chain_id, exit_code=0, hang=False):
        self.exit_code = exit_code
        self.hang = hang
        self.stopped = False
        folder = output / "chains" / chain_id
        folder.mkdir(parents=True)
        if exit_code == 0 and not hang:
            (folder / "chain-summary.json").write_text(json.dumps({"stopReason": "maxRunsPerChain"}), encoding="utf-8")
            run = folder / "runs" / f"run-{chain_id}"
            run.mkdir(parents=True)
            (run / "automation.json").write_text(json.dumps({"runId": run.name, "runIndex": 1,
                                                               "outcome": "Defeat", "completionReason": "completed"}), encoding="utf-8")

    def poll(self):
        return None if self.hang and not self.stopped else self.exit_code

    def wait(self, timeout=None):
        return self.exit_code

    def terminate(self):
        self.stopped = True

    def kill(self):
        self.stopped = True


class BalanceRunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.player = self.root / "balance.exe"
        self.player.write_bytes(b"player")
        data = self.root / "balance_Data"
        data.mkdir()
        (data / "resource.bin").write_bytes(b"data")
        (self.root / "build-manifest.json").write_text(json.dumps({
            "buildType": "Development+BALANCE_AUTOMATION",
            "executableSha256": balance_run.digest_file(self.player),
            "dataSha256": balance_run.digest_tree(data),
        }), encoding="utf-8")
        self.config = json.loads((Path(__file__).parent / "examples" / "fresh.json").read_text(encoding="utf-8"))
        self.config["outputDirectory"] = "result"
        self.config["chains"] = 2
        self.config["maxRunsPerChain"] = 1
        self.experiment = self.root / "experiment.json"
        self.experiment.write_text(json.dumps(self.config), encoding="utf-8")
        self.output = self.root / "result"

    def test_validate_rejects_hash_change_and_output_collision(self):
        balance_run.validate(self.experiment, self.player, self.output)
        self.player.write_bytes(b"changed")
        with self.assertRaisesRegex(ValueError, "Executable hash"):
            balance_run.validate(self.experiment, self.player, self.output)
        self.player.write_bytes(b"player")
        self.output.mkdir()
        with self.assertRaisesRegex(ValueError, "new directory"):
            balance_run.validate(self.experiment, self.player, self.output)

    def test_visual_is_opt_in_and_audio_is_separate(self):
        hidden = balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log")
        visual = balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log",
                                           visual=True)
        audible = balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log",
                                            visual=True, audio=True)
        self.assertIn("-batchmode", hidden)
        self.assertNotIn("--balance-visual", hidden)
        self.assertNotIn("-batchmode", visual)
        self.assertIn("--balance-visual", visual)
        self.assertNotIn("--balance-audio", visual)
        self.assertIn("--balance-audio", audible)
        with self.assertRaisesRegex(ValueError, "requires --visual"):
            balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log",
                                      audio=True)

    def test_visual_launch_does_not_inherit_hidden_window_flags(self):
        hidden = balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log")
        visual = balance_run.child_command(self.player, self.experiment, self.output, "chain-0001", self.root / "log",
                                           visual=True)
        with mock.patch.object(balance_run.subprocess, "Popen") as popen:
            balance_run.launch(hidden)
            hidden_options = popen.call_args.kwargs
            balance_run.launch(visual)
            visual_options = popen.call_args.kwargs
        if balance_run.os.name == "nt":
            self.assertIn("startupinfo", hidden_options)
            self.assertIn("creationflags", hidden_options)
        self.assertNotIn("startupinfo", visual_options)
        self.assertNotIn("creationflags", visual_options)

    def test_two_chains_are_sequential_and_manifest_preserves_runs(self):
        launches = []

        def launch(command):
            chain_id = command[-1].split("=", 1)[1]
            launches.append(chain_id)
            return FakeChild(self.output, chain_id)

        with mock.patch.object(balance_run, "launch", side_effect=launch):
            self.assertEqual(0, balance_run.run(self.experiment, self.player, self.output))
        self.assertEqual(["chain-0001", "chain-0002"], launches)
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("completed", manifest["state"])
        self.assertEqual(2, len(manifest["chains"]))
        self.assertEqual("Defeat", manifest["chains"][0]["runs"][0]["outcome"])

    def test_nonzero_child_stops_without_retry(self):
        launches = []

        def launch(command):
            chain_id = command[-1].split("=", 1)[1]
            launches.append(chain_id)
            return FakeChild(self.output, chain_id, exit_code=7)

        with mock.patch.object(balance_run, "launch", side_effect=launch):
            self.assertEqual(3, balance_run.run(self.experiment, self.player, self.output))
        self.assertEqual(["chain-0001"], launches)
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("failed", manifest["state"])
        self.assertEqual(7, manifest["chains"][0]["exitCode"])

    def test_crash_preserves_started_run_without_sidecar(self):
        def launch(command):
            child = FakeChild(self.output, "chain-0001", exit_code=7)
            progress = self.output / "chains" / "chain-0001" / "chain-progress.json"
            progress.write_text(json.dumps({"startedRuns": 1, "currentRunId": "deadbeef"}), encoding="utf-8")
            return child

        with mock.patch.object(balance_run, "launch", side_effect=launch):
            self.assertEqual(3, balance_run.run(self.experiment, self.player, self.output))
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("deadbeef", manifest["chains"][0]["runs"][0]["runId"])
        self.assertEqual("missingReport", manifest["chains"][0]["runs"][0]["state"])

    def test_hang_wall_budget_only_terminates_own_child(self):
        self.config["maxExperimentWallSeconds"] = 100
        self.experiment.write_text(json.dumps(self.config), encoding="utf-8")
        children = []

        def launch(command):
            child = FakeChild(self.output, "chain-0001", hang=True)
            children.append(child)
            return child

        ticks = iter([0, 0, 999, 999])
        with mock.patch.object(balance_run, "launch", side_effect=launch), \
             mock.patch.object(balance_run.time, "monotonic", side_effect=lambda: next(ticks, 999)), \
             mock.patch.object(balance_run.time, "sleep"):
            self.assertEqual(0, balance_run.run(self.experiment, self.player, self.output))
        self.assertTrue(children[0].stopped)
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("partial", manifest["state"])
        self.assertEqual("wallBudget", manifest["chains"][0]["state"])

    def test_wall_budget_does_not_start_another_chain(self):
        self.config["maxExperimentWallSeconds"] = 100
        self.experiment.write_text(json.dumps(self.config), encoding="utf-8")
        launches = []

        def launch(command):
            chain_id = command[-1].split("=", 1)[1]
            launches.append(chain_id)
            return FakeChild(self.output, chain_id)

        ticks = iter([0, 0, 101, 102])
        with mock.patch.object(balance_run, "launch", side_effect=launch), \
             mock.patch.object(balance_run.time, "monotonic", side_effect=lambda: next(ticks, 102)):
            self.assertEqual(0, balance_run.run(self.experiment, self.player, self.output))
        self.assertEqual(["chain-0001"], launches)
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("partial", manifest["state"])
        self.assertEqual("wallBudget", manifest["chains"][1]["state"])

    def test_worker_can_export_in_wall_budget_grace(self):
        self.config["chains"] = 1
        self.config["maxExperimentWallSeconds"] = 100
        self.experiment.write_text(json.dumps(self.config), encoding="utf-8")

        class GracefulChild(FakeChild):
            polls = 0

            def poll(self):
                self.polls += 1
                return None if self.polls == 1 else 0

        def launch(command):
            child = GracefulChild(self.output, "chain-0001", hang=True)
            folder = self.output / "chains" / "chain-0001"
            (folder / "chain-summary.json").write_text(
                json.dumps({"stopReason": "experimentWallBudget"}), encoding="utf-8")
            return child

        ticks = iter([0, 0, 100, 101])
        with mock.patch.object(balance_run, "launch", side_effect=launch), \
             mock.patch.object(balance_run.time, "monotonic", side_effect=lambda: next(ticks, 101)), \
             mock.patch.object(balance_run.time, "sleep"):
            self.assertEqual(0, balance_run.run(self.experiment, self.player, self.output))
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("partial", manifest["state"])
        self.assertEqual("partial", manifest["chains"][0]["state"])
        self.assertEqual(0, manifest["chains"][0]["exitCode"])

    def test_keyboard_cancel_keeps_partial_manifest(self):
        children = []

        def launch(command):
            child = FakeChild(self.output, "chain-0001", hang=True)
            children.append(child)
            return child

        with mock.patch.object(balance_run, "launch", side_effect=launch), \
             mock.patch.object(balance_run.time, "sleep", side_effect=KeyboardInterrupt):
            self.assertEqual(130, balance_run.run(self.experiment, self.player, self.output))
        self.assertTrue(children[0].stopped)
        manifest = json.loads((self.output / "manifest.json").read_text(encoding="utf-8"))
        self.assertEqual("cancelled", manifest["state"])


if __name__ == "__main__":
    unittest.main()
