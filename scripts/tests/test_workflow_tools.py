import copy
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import art_pipeline as art
import check_project as checks


class ArtPacketTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        from PIL import Image
        output = io.BytesIO()
        Image.new("RGBA", (32, 32), (100, 70, 30, 255)).save(output, format="PNG")
        self.source = output.getvalue()
        (self.root / "approved.png").write_bytes(self.source)
        for relative, content in (
            (art.MANIFEST, {"schemaVersion": 1, "entries": []}), (art.SPRITES, []),
            (art.PROFILES, [{"key": "icon", "maxSize": 256}]),
            ("Assets/Resources/Content/ActiveSkills/FixtureActiveSkills.json", [{"id": "FIXTURE-SKILL-ONE"}])):
            path = self.root / relative
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(art.json_bytes(content))
        self.packet = {"schemaVersion": 1, "approvedBy": "user", "approvedAt": "2026-09-22",
                       "approvalEvidence": "Synthetic test authorization; never production art.", "assets": [{
            "owner": "FIXTURE-ONE", "role": "icon", "visualId": "FIXTURE-ONE-VISUAL-ICON", "owningIp": "IP-12A",
            "input": "approved.png", "sha256": art.sha(self.source), "sourceDirectory": "Art/Source/Tests/fixture-one/icon",
            "version": "v001", "runtime": "Assets/Resources/Art/UI/Icons/fixture-one-icon.png",
            "prompt": "Synthetic test specimen", "generator": "test", "artDirectionRevision": "test",
            "preparation": {"mode": "fit", "size": 16, "padding": 2, "cropAlpha": True},
            "bindings": [{"file": "Assets/Resources/Content/ActiveSkills/FixtureActiveSkills.json",
                          "ownerId": "FIXTURE-SKILL-ONE", "pointer": "/iconVisualId", "expect": None}]}]}

    def apply(self):
        plan = art.build_plan(self.packet, self.root)
        plan.apply()
        return plan

    def test_packet_is_dry_until_apply_then_repeat_is_noop_and_preserves_guid(self):
        plan = art.build_plan(self.packet, self.root)
        runtime = self.root / self.packet["assets"][0]["runtime"]
        self.assertFalse(runtime.exists())
        plan.apply()
        meta = Path(str(runtime) + ".meta")
        meta.write_text("guid: existing-guid\n")
        self.assertEqual(0, len(self.apply().changes))
        self.assertEqual("guid: existing-guid\n", meta.read_text())
        from PIL import Image
        with Image.open(runtime) as image:
            self.assertEqual((16, 16), image.size)
            self.assertEqual(0, image.getpixel((0, 0))[3])

    def test_bad_second_asset_makes_no_partial_writes(self):
        other = copy.deepcopy(self.packet["assets"][0])
        other.update(owner="FIXTURE-TWO", visualId="FIXTURE-TWO-VISUAL-ICON", sha256="wrong",
                     sourceDirectory="Art/Source/Tests/fixture-two/icon",
                     runtime="Assets/Resources/Art/UI/Icons/fixture-two-icon.png")
        self.packet["assets"].append(other)
        with self.assertRaisesRegex(ValueError, "hash mismatch"):
            art.build_plan(self.packet, self.root)
        self.assertFalse((self.root / self.packet["assets"][0]["runtime"]).exists())

    def test_rejects_path_escape_unapproved_packets_and_unmanaged_files(self):
        for mutate in (lambda p: p.update(approvedBy="assistant"),
                       lambda p: p["assets"][0].update(runtime="../outside.png")):
            packet = copy.deepcopy(self.packet)
            mutate(packet)
            with self.assertRaises(ValueError):
                art.build_plan(packet, self.root)
        runtime = self.root / self.packet["assets"][0]["runtime"]
        runtime.parent.mkdir(parents=True)
        runtime.write_bytes(b"do not overwrite")
        with self.assertRaisesRegex(ValueError, "without its matching manifest"):
            self.apply()
        self.assertEqual(b"do not overwrite", runtime.read_bytes())

    def test_replacement_requires_new_version_and_previous_hash(self):
        self.apply()
        from PIL import Image
        source = self.root / "approved.png"
        Image.new("RGBA", (32, 32), (1, 2, 3, 255)).save(source)
        asset = self.packet["assets"][0]
        asset["sha256"] = art.sha(source.read_bytes())
        with self.assertRaisesRegex(ValueError, "immutable"):
            self.apply()
        asset["version"] = "v002"
        with self.assertRaisesRegex(ValueError, "Replacement requires"):
            self.apply()
        asset.update(replacementApproved=True, replacesSha256=art.sha(self.source))
        self.apply()
        self.assertEqual(self.source, (self.root / asset["sourceDirectory"] / "v001/concept-01.png").read_bytes())

    def test_rollback_preserves_original_files_on_write_failure(self):
        plan = art.build_plan(self.packet, self.root)
        real_replace = plan.replace
        count = 0
        def fail_once(path, data):
            nonlocal count
            count += 1
            if count == 3:
                raise OSError("simulated write failure")
            real_replace(path, data)
        with patch.object(plan, "replace", side_effect=fail_once), self.assertRaises(OSError):
            plan.apply()
        self.assertEqual([], art.read_json(self.root / art.SPRITES))
        self.assertFalse((self.root / self.packet["assets"][0]["sourceDirectory"] / "selected-master.png").exists())

    def test_concurrent_change_aborts_without_clobber(self):
        plan = art.build_plan(self.packet, self.root)
        record = next(p for p in plan.changes if p.name == "asset-record.json")
        record.parent.mkdir(parents=True)
        record.write_text("another writer")
        with self.assertRaisesRegex(ValueError, "changed since preflight"):
            plan.apply()
        self.assertEqual("another writer", record.read_text())


class CheckRunnerTests(unittest.TestCase):
    def test_rest_job_is_polled_once_started_and_counts_are_recorded(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            def response(url, payload=None):
                if url.endswith("/health"):
                    return {"projectName": checks.ROOT.name, "currentMode": "bypass"}
                if "/skills/schema" in url:
                    return {"test_run": {"filter": "string"}, "test_get_result": {}}
                if url.endswith("/skills/meta"):
                    return {}
                if url.endswith("/skill/test_run"):
                    self.assertEqual("Game", payload["filter"])
                    return {"jobId": "job-one"}
                self.assertEqual({"jobId": "job-one"}, payload)
                return {"status": "completed", "totalTests": 2, "passedTests": 2,
                        "failedTests": 0, "skippedTests": 0}
            with patch.object(checks, "request", side_effect=response) as api:
                result = checks.run_editor("EditMode", r"^Game\.", output, 10, "http://127.0.0.1:8090")
            self.assertEqual(2, result["Game"]["passed"])
            self.assertTrue(Path(result["resultFile"]).is_file())
            self.assertEqual(1, sum(c.args[0].endswith("/skill/test_run") for c in api.call_args_list))

    def test_rest_refuses_wrong_project_and_playmode_permission_without_starting_job(self):
        for health in ({"projectName": "wrong", "currentMode": "bypass"},
                       {"projectName": checks.ROOT.name, "currentMode": "auto"}):
            with patch.object(checks, "request", return_value=health) as api:
                with self.assertRaises(checks.NotRun):
                    checks.run_editor("PlayMode", r"^Game\.", Path("unused"), 10, "http://127.0.0.1:8090")
                self.assertEqual(1, api.call_count)

    def test_editor_filter_translation_never_sends_regex_to_literal_api(self):
        self.assertEqual(["Game"], checks.editor_groups(r"^Game\."))
        self.assertEqual(["Game.Presentation.Tests"], checks.editor_groups(r"^Game\.Presentation\.Tests\."))
        self.assertEqual(2, len(checks.editor_groups(checks.ART_FILTER)))
        with self.assertRaises(checks.NotRun):
            checks.editor_groups(r"^Game\.(Combat|Enemy)")

    def test_runner_lock_rejects_a_second_runner_and_releases(self):
        with tempfile.TemporaryDirectory() as directory:
            with checks.RunnerLock(Path(directory)):
                with self.assertRaises(checks.NotRun):
                    with checks.RunnerLock(Path(directory)):
                        self.fail("Second runner acquired a live lock")
            with checks.RunnerLock(Path(directory)):
                pass

    def test_preview_rejects_ids_schema_and_nonfinite_values(self):
        checks.preview_diff({"id": "same", "scale": 1}, {"id": "same", "scale": 1.2})
        for value in ({"id": "other", "scale": 1}, {"scale": 1}, {"id": "same", "scale": float("nan")}):
            with self.assertRaises(ValueError):
                checks.preview_diff({"id": "same", "scale": 1}, value)

    def test_auto_scope_preserves_code_and_gameplay_checks(self):
        self.assertEqual("docs", checks.choose_scope(["docs/art/ASSET_PIPELINE.md"]))
        self.assertEqual("art", checks.choose_scope(["Assets/Resources/Art/a.png"]))
        self.assertEqual("full", checks.choose_scope(["Assets/Game/Presentation/Foo.cs"]))
        self.assertEqual("full", checks.choose_scope(["Assets/Resources/Content/Pickups/FixturePickups.json"]))

    def test_unreadable_process_probe_never_launches_batch(self):
        with patch.object(checks, "command", side_effect=subprocess.CalledProcessError(1, "probe")):
            with self.assertRaises(checks.NotRun):
                checks.editor_processes()
        with patch.object(checks, "editor_processes", return_value=[{"pid": 123, "batch": False}]), \
             patch.object(checks.subprocess, "Popen") as launch:
            with self.assertRaises(checks.NotRun):
                checks.run_batch("EditMode", "^Game\\.", Path("unused"), 1)
            launch.assert_not_called()

    def test_process_matching_does_not_confuse_sibling_projects(self):
        rows = [{"ProcessId": 1, "CommandLine": 'Unity.exe -projectPath "D:/GitHub/survivor-arena-other"'},
                {"ProcessId": 2, "CommandLine": 'Unity.exe -projectPath "D:/GitHub/survivor-arena" -batchmode'}]
        with patch.object(checks, "command", return_value=json.dumps(rows)):
            self.assertEqual([{"pid": 2, "batch": True}], checks.editor_processes(Path("D:/GitHub/survivor-arena")))

    def test_xml_counts_only_project_tests_and_rejects_empty_or_skipped(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "test.xml"
            path.write_text('<test-run result="Passed"><test-case fullname="Game.Foo.Test" result="Passed"/>'
                            '<test-case fullname="Vendor.Test" result="Passed"/></test-run>')
            result = checks.xml_result(path)
            self.assertEqual(1, result["Game"]["passed"])
            self.assertEqual(1, result["thirdParty"])
            for content in ('<test-run result="Passed"/>',
                            '<test-run result="Passed"><test-case fullname="Game.Foo" result="Skipped"/></test-run>'):
                path.write_text(content)
                with self.assertRaises(ValueError):
                    checks.xml_result(path)

    def test_fingerprint_ignores_docs_but_detects_runtime_change(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Assets").mkdir()
            image = root / "Assets/image.png"
            image.write_bytes(b"original")
            first = checks.fingerprint(root)
            (root / "notes.md").write_text("documentation only")
            self.assertEqual(first, checks.fingerprint(root))
            image.write_bytes(b"replacement")
            self.assertNotEqual(first, checks.fingerprint(root))

    def test_first_import_metadata_does_not_require_repeating_successful_tests(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Assets").mkdir()
            image = root / "Assets/approved.png"
            image.write_bytes(b"approved pixels")
            snapshot = {"Assets/approved.png"}
            before = checks.fingerprint(root)
            (root / "Assets/approved.png.meta").write_text("guid: newly-imported")
            self.assertEqual(["Assets/approved.png.meta"], checks.created_import_metadata(root, snapshot, before))
            image.write_bytes(b"concurrent edit")
            self.assertEqual([], checks.created_import_metadata(root, snapshot, before))

    def test_command_failure_reports_diagnostic_instead_of_entire_command(self):
        failed = subprocess.CompletedProcess(["git"], 2, stdout="specific error", stderr="")
        with patch.object(checks.subprocess, "run", return_value=failed):
            with self.assertRaisesRegex(ValueError, "specific error"):
                checks.command(["git", "diff"])


if __name__ == "__main__":
    unittest.main()
