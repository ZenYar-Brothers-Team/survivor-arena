"""One check entry point: scoped static checks, safe Unity routing, recorded evidence."""
import argparse
import datetime
import hashlib
import json
import math
import os
from pathlib import Path
import re
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET

from art_pipeline import ROOT, inside, json_bytes, read_json, sha


ART_FILTER = r"^Game\.(Presentation\.Tests\.|Bootstrap\.Tests\.FixtureRuntimeContentCatalogTests)"
PREVIEW_FILES = {
    "Assets/Resources/Content/Presentation/" + name + ".json" for name in (
        "FixtureGroundShadowPresentation", "FixtureEnemyDeathPresentation",
        "FixtureSpriteMotionProfiles", "FixturePresentationFeedback")}


class NotRun(RuntimeError):
    pass


class RunnerLock:
    """Serialize our runners; the OS releases the lock even if the caller crashes."""
    def __init__(self, root=ROOT):
        self.path = root / "TestResults/checks/runner.lock"
        self.stream = None

    def __enter__(self):
        import msvcrt
        self.path.parent.mkdir(parents=True, exist_ok=True)
        self.stream = self.path.open("a+b")
        self.stream.seek(0)
        try:
            msvcrt.locking(self.stream.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError as error:
            self.stream.close()
            raise NotRun("Another check runner already owns this project") from error
        return self

    def __exit__(self, *_):
        import msvcrt
        self.stream.seek(0)
        msvcrt.locking(self.stream.fileno(), msvcrt.LK_UNLCK, 1)
        self.stream.close()


def command(args, root=ROOT, **kwargs):
    result = subprocess.run(args, cwd=root, capture_output=True,
                            encoding="utf-8", errors="replace", **kwargs)
    if result.returncode:
        detail = (result.stdout + "\n" + result.stderr).strip()
        raise ValueError(f"{args[0]} failed ({result.returncode}): {detail[-4000:]}")
    return result.stdout


def changed_paths(root=ROOT):
    # -z preserves spaces/Unicode. Both staged and unstaged changes plus new files.
    paths = command(["git", "diff", "--name-only", "-z", "HEAD"], root).split("\0")
    paths += command(["git", "ls-files", "--others", "--exclude-standard", "-z"], root).split("\0")
    return sorted(set(p for p in paths if p))


def choose_scope(paths):
    if not paths:
        return "docs"
    if all(p.endswith(".md") for p in paths):
        return "docs"
    if all(p.startswith(("Art/", "Assets/Resources/Art/")) or p.endswith(".md") or
           p == "Assets/Resources/Content/Presentation/FixtureSprites.json" for p in paths):
        return "art"
    return "full"


def preview_diff(before, after, path=""):
    if type(before) is not type(after):
        # JSON 0 and 0.1 are compatible numeric presentation values.
        if type(before) not in (int, float) or type(after) not in (int, float):
            raise ValueError(f"Preview cannot change schema/type: {path}")
    if isinstance(before, dict):
        if before.keys() != after.keys():
            raise ValueError(f"Preview cannot add/remove fields: {path}")
        for key in before:
            preview_diff(before[key], after[key], path + "/" + key)
    elif isinstance(before, list):
        if len(before) != len(after):
            raise ValueError(f"Preview cannot add/remove records: {path}")
        for i, item in enumerate(before):
            preview_diff(item, after[i], path + "/" + str(i))
    elif before != after:
        if type(after) not in (int, float) or not math.isfinite(after):
            raise ValueError(f"Preview allows existing finite numeric values only: {path}")


def static_checks(scope, paths, base="HEAD", root=ROOT):
    # Unity emits empty YAML values with trailing spaces in generated .meta files.
    # Import/GUID checks cover them; do not fail the text whitespace check on those.
    text_paths = [p for p in paths if not p.endswith(".meta")]
    if text_paths:
        command(["git", "diff", "--check", "HEAD", "--", *text_paths], root)
    for relative in paths:
        path = inside(root, relative)
        if path.is_file() and path.suffix == ".json":
            read_json(path)
    if scope == "docs" and any(not p.endswith(".md") for p in paths):
        raise ValueError("docs scope accepts Markdown changes only")
    if scope == "visual-preview":
        if not paths or any(p not in PREVIEW_FILES for p in paths):
            raise ValueError("visual-preview requires explicit existing presentation-profile paths; see scripts/README.md")
        for path in paths:
            before = json.loads(command(["git", "show", base + ":" + path], root))
            preview_diff(before, read_json(root / path), path)


def fingerprint(root=ROOT, exclude=frozenset()):
    digest = hashlib.sha256()
    files = []
    for directory in ("Assets", "Packages", "ProjectSettings", "scripts"):
        files.extend(p for p in (root / directory).rglob("*") if p.is_file()
                     and "__pycache__" not in p.parts and p.suffix != ".pyc")
    files += [root / "Art/ImportProfiles.json", root / "Art/asset-manifest.json"]
    for path in sorted(files):
        if not path.is_file() or path.relative_to(root).as_posix() in exclude:
            continue
        digest.update(path.relative_to(root).as_posix().encode("utf-8") + b"\0")
        digest.update(hashlib.sha256(path.read_bytes()).digest())
    return digest.hexdigest()


def created_import_metadata(root, before_assets, before_fingerprint):
    """Permit only new .meta for assets/folders already present before the import."""
    added = {p.relative_to(root).as_posix() for p in (root / "Assets").rglob("*.meta")
             if p.relative_to(root).as_posix() not in before_assets and
             p.with_suffix("").relative_to(root).as_posix() in before_assets}
    return sorted(added) if added and fingerprint(root, added) == before_fingerprint else []


def editor_processes(root=ROOT):
    # A denied/unavailable process probe must never be interpreted as "Editor closed".
    script = ("$ErrorActionPreference='Stop'\n"
              "[Console]::OutputEncoding=[System.Text.Encoding]::UTF8\n"
              "@(Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" | "
              "Select-Object ProcessId,CommandLine) | ConvertTo-Json -Compress")
    try:
        output = command(["powershell", "-NoProfile", "-NonInteractive", "-Command", script], root, timeout=15)
        rows = json.loads(output.lstrip("\ufeff")) if output.strip() else []
    except (subprocess.SubprocessError, ValueError, OSError) as error:
        raise NotRun("Cannot inspect Unity processes. Run this command with permission for the process probe.") from error
    if isinstance(rows, dict):
        rows = [rows]
    matches = []
    for row in rows:
        line = row.get("CommandLine")
        if not line:
            raise NotRun("A Unity process has an unreadable command line; cannot safely launch batch mode.")
        match = re.search(r'-projectPath\s+(?:"([^"]+)"|(\S+))', line, re.I)
        if not match:
            raise NotRun("Unity process has no identifiable project path; confirm the Editor state manually.")
        project = Path(match.group(1) or match.group(2)).resolve()
        if os.path.normcase(str(project)) == os.path.normcase(str(root.resolve())):
            matches.append({"pid": row["ProcessId"], "batch": bool(re.search(r"-batchmode\b", line, re.I))})
    return matches


def ensure_unlocked(root=ROOT):
    for lock in (root / "Temp/UnityLockfile", root / "Library/UnityLockfile"):
        if lock.exists():
            try:
                # Unity holds this file exclusively. A stale, openable lock is harmless.
                with lock.open("r+b"):
                    pass
            except OSError as error:
                raise NotRun("Unity project lock is held; batch launch refused.") from error


def find_unity(root=ROOT, explicit=None):
    if explicit:
        path = Path(explicit)
        if not path.is_file():
            raise NotRun("Specified Unity executable does not exist")
        return path
    version = re.search(r"^m_EditorVersion:\s*(\S+)",
                        (root / "ProjectSettings/ProjectVersion.txt").read_text(), re.M).group(1)
    for parent in (Path("D:/Unity/editor"), Path("C:/Program Files/Unity/Hub/Editor")):
        path = parent / version / "Editor/Unity.exe"
        if path.is_file():
            return path
    raise NotRun(f"Unity {version} not found; pass --unity-path")


def xml_result(path):
    tree = ET.parse(path).getroot()
    cases = list(tree.iter("test-case"))
    project = [c for c in cases if c.get("fullname", c.get("classname", "")).startswith("Game.")]
    counts = {"total": len(project), "passed": 0, "failed": 0, "skipped": 0}
    failures = []
    for case in project:
        state = case.get("result")
        counts["passed" if state == "Passed" else "failed" if state == "Failed" else "skipped"] += 1
        if state == "Failed":
            failures.append({"name": case.get("fullname"), "message": case.findtext("failure/message", "")})
    if tree.get("result") not in ("Passed", "Success") or not project or counts["failed"] or counts["skipped"]:
        raise ValueError(json.dumps({"platformResult": tree.get("result"), "Game": counts, "failures": failures}))
    return {"Game": counts, "thirdParty": len(cases) - len(project), "xml": str(path)}


def run_batch(platform, test_filter, output, timeout, root=ROOT, unity_path=None):
    if editor_processes(root):
        raise NotRun("Unity opened before batch launch; rerun to select the Editor runner.")
    ensure_unlocked(root)
    unity = find_unity(root, unity_path)
    xml, log = output / (platform + ".xml"), output / (platform + ".log")
    args = [str(unity), "-batchmode", "-nographics", "-projectPath", str(root),
            "-runTests", "-testPlatform", platform, "-testFilter", test_filter,
            "-testResults", str(xml), "-logFile", str(log)]
    start = time.monotonic()
    with subprocess.Popen(args, cwd=root, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                          creationflags=getattr(subprocess, "CREATE_NO_WINDOW", 0)) as process:
        try:
            code = process.wait(timeout=timeout)
        except subprocess.TimeoutExpired as error:
            process.kill()  # Only the batch process this invocation owns, never an interactive Editor.
            process.wait(timeout=15)
            raise NotRun(f"{platform} exceeded {timeout}s. Inspect {log}") from error
    if not xml.is_file():
        tail = "\n".join(log.read_text(encoding="utf-8", errors="replace").splitlines()[-24:]) if log.exists() else "No log"
        raise NotRun(f"{platform} produced no result (exit {code}); {log}\n{tail}")
    result = xml_result(xml)
    if code:
        raise ValueError(f"Unity exited {code}; inspect {log}")
    return dict(result, platform=platform, runner="batch", seconds=round(time.monotonic() - start, 2), log=str(log))


def request(url, payload=None):
    data = None if payload is None else json_bytes(payload)
    req = urllib.request.Request(url, data=data, headers={"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=15) as response:
        value = json.load(response)
    if value.get("success") is False or value.get("error"):
        raise NotRun(f"UnitySkills refused the request: {json.dumps(value, ensure_ascii=False)}")
    # UnitySkills transport wraps skill return values in result.
    result = value.get("result", value)
    if isinstance(result, dict) and (result.get("success") is False or result.get("error")):
        raise NotRun(f"UnitySkills refused the request: {json.dumps(result, ensure_ascii=False)}")
    return result


def editor_groups(test_filter):
    # UnitySkills takes a literal class/namespace, NOT Unity CLI's regex syntax.
    if test_filter == ART_FILTER:
        return ["Game.Presentation.Tests", "Game.Bootstrap.Tests.FixtureRuntimeContentCatalogTests"]
    if test_filter == r"^Game\.":
        return ["Game"]
    literal = test_filter.removeprefix("^").replace(r"\.", ".").rstrip(".")
    if not re.fullmatch(r"Game(?:\.[A-Za-z_][A-Za-z0-9_]*)+", literal):
        raise NotRun("This regex cannot be translated to a UnitySkills group. Use an anchored literal namespace or closed-Editor batch.")
    return [literal]


def run_editor(platform, test_filter, output, timeout, url, root=ROOT):
    groups = editor_groups(test_filter)
    results = [run_editor_group(platform, group, output, timeout, url, root) for group in groups]
    combined = {"platform": platform, "runner": "UnitySkills", "groups": groups,
                "Game": {key: sum(r["Game"][key] for r in results) for key in ("total", "passed", "failed", "skipped")},
                "seconds": round(sum(r["seconds"] for r in results), 2), "groupResults": results}
    path = output / (platform + "-REST-summary.json")
    path.write_bytes(json_bytes(combined))
    return dict(combined, resultFile=str(path))


def run_editor_group(platform, group, output, timeout, url, root=ROOT):
    endpoint = urllib.parse.urlparse(url)
    if endpoint.scheme != "http" or endpoint.hostname not in ("localhost", "127.0.0.1", "::1") or endpoint.username or endpoint.password or endpoint.query or endpoint.path not in ("", "/"):
        raise NotRun("UnitySkills endpoint must be a local HTTP server")
    health = request(url + "/health")
    if health.get("projectName") != root.name:
        raise NotRun("UnitySkills endpoint belongs to a different project")
    if health.get("projectPath") and Path(health["projectPath"]).resolve() != root.resolve():
        raise NotRun("UnitySkills project path mismatch")
    if health.get("isPlaying") or health.get("isCompiling") or health.get("pendingCount", 0):
        raise NotRun("Editor is playing, compiling or has pending operations; finish those first")
    if health.get("currentMode") not in ("auto", "bypass"):
        raise NotRun("UnitySkills test_run requires an allowed execution mode; change it in the panel if desired")
    if platform == "PlayMode" and health.get("currentMode") != "bypass":
        raise NotRun("PlayMode requires user-enabled Bypass per smoke-check; no fallback to batch")
    request(url + "/skills/meta")
    schema = request(url + "/skills/schema?category=Test&wire=v2")
    schema_text = json.dumps(schema)
    if not all(name in schema_text for name in ("test_run", "test_get_result", '"filter"')):
        raise NotRun("Installed UnitySkills test schema differs; inspect it before running")
    # Local package TestRun has SupportsDryRun=false; the server enforces grants and serialization.
    job = request(url + "/skill/test_run", {"testMode": platform, "filter": group})
    if not job.get("jobId"):
        raise NotRun("UnitySkills returned no test job ID")
    started = time.monotonic()
    while time.monotonic() - started < timeout:
        result = request(url + "/skill/test_get_result", {"jobId": job["jobId"]})
        state = str(result.get("status", "")).lower()
        if state in ("completed", "failed", "cancelled", "canceled"):
            result_file = output / (platform + "-" + group + "-REST.json")
            result_file.write_bytes(json_bytes(result))
            counts = {"total": result.get("totalTests", 0), "passed": result.get("passedTests", 0),
                      "failed": result.get("failedTests", 0), "skipped": result.get("skippedTests", 0)}
            if state != "completed" or not counts["total"] or counts["passed"] != counts["total"]:
                raise ValueError(f"{platform}: {json.dumps(result, ensure_ascii=False)}")
            return {"platform": platform, "runner": "UnitySkills", "Game": counts,
                    "seconds": round(time.monotonic() - started, 2), "resultFile": str(result_file)}
        time.sleep(2)
    raise NotRun(f"Test job {job['jobId']} still active after timeout; inspect it before starting any other run")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--scope", choices=("auto", "docs", "art", "code", "full", "visual-preview"), default="auto")
    parser.add_argument("--paths", nargs="+")
    parser.add_argument("--base", default="HEAD", help="Baseline commit for visual-preview numeric-only diff")
    parser.add_argument("--filter", dest="test_filter")
    parser.add_argument("--platforms", nargs="+", choices=("EditMode", "PlayMode"))
    parser.add_argument("--unity-path")
    parser.add_argument("--unity-url", default="http://127.0.0.1:8090")
    parser.add_argument("--timeout", type=int, default=300)
    parser.add_argument("--plan", action="store_true")
    parser.add_argument("--reuse", action="store_true", help="Reuse matching successful batch receipt; reports original date")
    args = parser.parse_args()
    lock = None
    try:
        paths = args.paths if args.paths is not None else changed_paths()
        scope = choose_scope(paths) if args.scope == "auto" else args.scope
        if scope == "visual-preview" and not args.paths:
            raise ValueError("visual-preview requires --paths to make the scope explicit")
        if args.timeout <= 0:
            raise ValueError("timeout must be positive")
        test_filter = args.test_filter or (ART_FILTER if scope == "art" else r"^Game\.")
        if not test_filter.startswith("^Game\\."):
            raise ValueError("Filter must be anchored to ^Game\\. so third-party tests cannot masquerade as project evidence")
        platforms = args.platforms or (["EditMode"] if scope == "art" else ["EditMode", "PlayMode"])
        if scope == "full" and (args.test_filter or args.platforms):
            raise ValueError("full always runs all Game.* EditMode + PlayMode; use code for a custom subset")
        static_checks(scope, paths, args.base)
        if args.plan or scope in ("docs", "visual-preview"):
            print(json.dumps({"verdict": "PREVIEW ONLY" if scope == "visual-preview" else "PLAN" if args.plan else "STATIC PASS",
                              "scope": scope, "paths": paths, "Unity": "NOT RUN",
                              "plannedPlatforms": [] if scope in ("docs", "visual-preview") else platforms,
                              "filter": test_filter,
                              "previewMenu": "Tools > Survivor Arena > Presentation Fixture Review" if scope == "visual-preview" else None,
                              "note": "No runtime verification or visual approval implied."}, ensure_ascii=False))
            return 0
        lock = RunnerLock()
        lock.__enter__()
        processes = editor_processes()
        if any(p["batch"] for p in processes):
            raise NotRun("Another batch run already owns this project")
        before = fingerprint()
        before_assets = {p.relative_to(ROOT).as_posix() for p in (ROOT / "Assets").rglob("*")}
        key = sha(json_bytes({"fingerprint": before, "platforms": platforms, "filter": test_filter, "scope": scope}))
        receipt_path = ROOT / "TestResults/checks" / (key + ".json")
        if args.reuse and not processes and receipt_path.exists():
            receipt = read_json(receipt_path)
            if receipt.get("verdict") == "PASS" and receipt.get("fingerprint") == before and receipt.get("evidenceHashes") and all(Path(p).is_file() and sha(Path(p).read_bytes()) == h
                                                         for p, h in receipt["evidenceHashes"].items()):
                if scope in ("art", "full"):
                    print(command([sys.executable, "scripts/validate-art-manifest.py"]).strip(), flush=True)
                print(json.dumps(dict(receipt, verdict="REUSED PASS"), ensure_ascii=False))
                return 0
        stamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%dT%H%M%S-%fZ")
        output = ROOT / "TestResults/checks" / stamp
        output.mkdir(parents=True)
        results = []
        for platform in platforms:
            current = editor_processes()
            if any(p["batch"] for p in current):
                raise NotRun("A concurrent batch run started; no second test run launched")
            result = (run_editor(platform, test_filter, output, args.timeout, args.unity_url) if current else
                      run_batch(platform, test_filter, output, args.timeout, unity_path=args.unity_path))
            results.append(result)
            print(json.dumps(result, ensure_ascii=False), flush=True)
        if scope in ("art", "full"):
            print(command([sys.executable, "scripts/validate-art-manifest.py"]).strip(), flush=True)
        after = fingerprint()
        imported_meta = created_import_metadata(ROOT, before_assets, before) if after != before else []
        if after != before and not imported_meta:
            raise NotRun("Inputs changed during checks. Results saved; no reusable PASS recorded.")
        hashes = {r.get("xml", r.get("resultFile")): sha(Path(r.get("xml", r.get("resultFile"))).read_bytes()) for r in results}
        receipt = {"verdict": "PASS", "scope": scope, "checkedAt": stamp, "fingerprint": after,
                   "filter": test_filter, "results": results, "evidenceHashes": hashes,
                   "createdImportMetadata": imported_meta,
                   "unityVersion": (ROOT / "ProjectSettings/ProjectVersion.txt").read_text().strip()}
        # Only a fresh closed-Editor run is reusable; memory/unsaved scenes are not fingerprinted.
        if all(r["runner"] == "batch" for r in results):
            key = sha(json_bytes({"fingerprint": after, "platforms": platforms, "filter": test_filter, "scope": scope}))
            receipt_path = ROOT / "TestResults/checks" / (key + ".json")
            receipt_path.write_bytes(json_bytes(receipt))
        (output / "summary.json").write_bytes(json_bytes(receipt))
        print(f"PASS: {output / 'summary.json'}")
        return 0
    except NotRun as error:
        print(f"NOT RUN / INCOMPLETE: {error}", file=sys.stderr)
        return 2
    except (ValueError, OSError, KeyError, TypeError, subprocess.SubprocessError, ET.ParseError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    finally:
        if lock is not None and lock.stream is not None and not lock.stream.closed:
            lock.__exit__()


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    sys.exit(main())
