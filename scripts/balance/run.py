"""One local development player at a time; no retries or production save access."""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
from datetime import datetime, timezone

WALL_BUDGET_SHUTDOWN_GRACE_SECONDS = 30


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat()


def digest_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def digest_tree(folder: Path) -> str:
    if not folder.is_dir():
        raise ValueError(f"Player data folder missing: {folder}")
    digest = hashlib.sha256()
    for path in sorted(folder.rglob("*"), key=lambda item: item.relative_to(folder).as_posix()):
        if path.is_file():
            relative = path.relative_to(folder).as_posix()
            digest.update(f"{relative}:{digest_file(path)}\n".encode("utf-8"))
    return digest.hexdigest()


def write_json(path: Path, value: dict) -> None:
    pending = path.with_name(path.name + ".pending")
    pending.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    os.replace(pending, path)


def write_text(path: Path, value: str) -> None:
    pending = path.with_name(path.name + ".pending")
    pending.write_text(value, encoding="utf-8")
    os.replace(pending, path)


def validate(experiment: Path, player: Path, output: Path) -> tuple[dict, dict, str, str | None]:
    if not experiment.is_file():
        raise ValueError(f"Experiment file missing: {experiment}")
    if not player.is_file() or player.suffix.lower() != ".exe":
        raise ValueError(f"Development player .exe missing: {player}")
    if output.exists() or output.parent == output:
        raise ValueError(f"Experiment output must be a new directory: {output}")
    raw = experiment.read_bytes()
    config = json.loads(raw)
    required = {"schemaVersion", "experimentId", "template", "chains", "maxRunsPerChain", "runSpeed",
                "characterId", "fieldRoute", "movementPolicy", "draftPolicy", "purchasePolicy",
                "stopAfterRouteClear", "maxExperimentWallSeconds", "runWallTimeoutSeconds",
                "transitionTimeoutSeconds", "outputDirectory"}
    if set(config) - (required | {"initialProfilePath"}) or required - set(config):
        raise ValueError("Experiment has missing or unknown top-level fields")
    if config["schemaVersion"] != 1 or config["template"] not in ("fresh", "preset"):
        raise ValueError("Unsupported experiment schema or template")
    if not all(isinstance(config[key], int) and not isinstance(config[key], bool) and config[key] > 0
               for key in ("chains", "maxRunsPerChain")):
        raise ValueError("chains and maxRunsPerChain must be positive integers")
    if config["runSpeed"] not in (1, 2, 3, 5):
        raise ValueError("runSpeed must be 1, 2, 3 or 5")
    if not isinstance(config["maxExperimentWallSeconds"], (int, float)) or config["maxExperimentWallSeconds"] <= 0:
        raise ValueError("maxExperimentWallSeconds must be positive")
    if output.name != config["outputDirectory"]:
        raise ValueError("--output directory name must match config outputDirectory")
    preset_hash = None
    if config["template"] == "preset":
        preset = (experiment.parent / config["initialProfilePath"]).resolve()
        if not preset.is_file():
            raise ValueError(f"Preset profile missing: {preset}")
        preset_hash = digest_file(preset)
    elif "initialProfilePath" in config:
        raise ValueError("fresh experiment forbids initialProfilePath")
    manifest_path = player.parent / "build-manifest.json"
    if not manifest_path.is_file():
        raise ValueError(f"Build manifest missing: {manifest_path}")
    build = json.loads(manifest_path.read_text(encoding="utf-8"))
    if build.get("buildType") != "Development+BALANCE_AUTOMATION":
        raise ValueError("Player is not an isolated balance development build")
    if digest_file(player) != build.get("executableSha256"):
        raise ValueError("Executable hash differs from build manifest")
    data_folder = player.with_name(player.stem + "_Data")
    if digest_tree(data_folder) != build.get("dataSha256"):
        raise ValueError("Player data hash differs from build manifest")
    return config, build, hashlib.sha256(raw).hexdigest(), preset_hash


def child_command(player: Path, experiment: Path, output: Path, chain_id: str, log: Path,
                  visual: bool = False, audio: bool = False) -> list[str]:
    if audio and not visual:
        raise ValueError("--audio requires --visual")
    return [str(player), *([] if visual else ["-batchmode"]), "-logFile", str(log),
            "-screen-width", "640", "-screen-height", "360", "-screen-fullscreen", "0",
            f"--balance-experiment={experiment}", f"--balance-output-root={output.parent}",
            f"--balance-chain={chain_id}", *(["--balance-audio"] if audio else [])]


def launch(command: list[str]) -> subprocess.Popen:
    options = {}
    if os.name == "nt" and "-batchmode" in command:
        startup = subprocess.STARTUPINFO()
        startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startup.wShowWindow = subprocess.SW_HIDE
        options["startupinfo"] = startup
        options["creationflags"] = subprocess.CREATE_NO_WINDOW
    return subprocess.Popen(command, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL,
                            stderr=subprocess.DEVNULL, **options)


def stop_child(child: subprocess.Popen) -> None:
    if child.poll() is not None:
        return
    child.terminate()
    try:
        child.wait(timeout=10)
    except subprocess.TimeoutExpired:
        child.kill()
        child.wait(timeout=10)


def scan_runs(output: Path, chain_id: str) -> list[dict]:
    folder = output / "chains" / chain_id / "runs"
    result = []
    if not folder.is_dir():
        return result
    for run in sorted(folder.iterdir()):
        if not run.is_dir():
            continue
        sidecar = run / "automation.json"
        if sidecar.is_file():
            try:
                data = json.loads(sidecar.read_text(encoding="utf-8"))
                result.append({"runId": data.get("runId", run.name), "runIndex": data.get("runIndex"),
                               "outcome": data.get("outcome"), "completionReason": data.get("completionReason"),
                               "state": "completed" if data.get("completionReason") == "completed" else "incomplete"})
            except (OSError, ValueError):
                result.append({"runId": run.name, "state": "corruptReport"})
        else:
            result.append({"runId": run.name, "state": "missingReport"})
    return result


def include_started_run(chain: dict) -> None:
    """A crashed child must not make its last started run disappear from the manifest."""
    progress = chain.get("progress") or {}
    run_id = progress.get("currentRunId")
    if run_id and not any(item["runId"] == run_id for item in chain["runs"]):
        chain["runs"].append({"runId": run_id, "state": "missingReport",
                              "runIndex": progress.get("startedRuns"), "completionReason": "incomplete"})


def load_progress(output: Path, chain: dict) -> None:
    progress = output / "chains" / chain["chainId"] / "chain-progress.json"
    if progress.is_file():
        try:
            chain["progress"] = json.loads(progress.read_text(encoding="utf-8"))
        except (OSError, ValueError):
            pass


def run(experiment: Path, player: Path, output: Path, visual: bool = False, audio: bool = False) -> int:
    if audio and not visual:
        raise ValueError("--audio requires --visual")
    config, build, config_hash, preset_hash = validate(experiment, player, output)
    output.mkdir(parents=True, exist_ok=False)
    write_json(output / "experiment.json", {"schemaVersion": 1, "config": config,
               "sourceConfigSha256": config_hash, "presetSha256": preset_hash,
               "build": build, "startedUtc": utc_now(),
               "presentation": {"visual": visual, "audio": audio}})
    started = time.monotonic()
    manifest = {"schemaVersion": 1, "experimentId": config["experimentId"],
                "requestedChains": config["chains"], "requestedRuns": config["chains"] * config["maxRunsPerChain"],
                "workerCount": 1, "startedUtc": utc_now(), "chains": [], "state": "running",
                "presentation": {"visual": visual, "audio": audio}}
    write_json(output / "manifest.json", manifest)
    for index in range(1, config["chains"] + 1):
        chain_id = f"chain-{index:04d}"
        chain = {"chainId": chain_id, "state": "starting", "runs": [], "startedUtc": utc_now()}
        manifest["chains"].append(chain)
        write_json(output / "manifest.json", manifest)
        if time.monotonic() - started >= config["maxExperimentWallSeconds"]:
            chain["state"] = "wallBudget"
            chain["endedUtc"] = utc_now()
            write_json(output / "manifest.json", manifest)
            break
        if digest_file(experiment) != config_hash:
            chain["state"] = "inputChanged"
            break
        if preset_hash and digest_file((experiment.parent / config["initialProfilePath"]).resolve()) != preset_hash:
            chain["state"] = "inputChanged"
            break
        if digest_file(player) != build["executableSha256"] or \
           digest_tree(player.with_name(player.stem + "_Data")) != build["dataSha256"]:
            chain["state"] = "inputChanged"
            break
        log = output / f"{chain_id}-player.log"
        child = None
        try:
            child = launch(child_command(player, experiment, output, chain_id, log, visual, audio))
            chain["state"] = "running"
            chain["pid"] = child.pid
            while child.poll() is None:
                elapsed = time.monotonic() - started
                if elapsed >= config["maxExperimentWallSeconds"] + WALL_BUDGET_SHUTDOWN_GRACE_SECONDS:
                    chain["state"] = "wallBudget"
                    stop_child(child)
                    break
                load_progress(output, chain)
                chain["runs"] = scan_runs(output, chain_id)
                include_started_run(chain)
                manifest["updatedUtc"] = utc_now()
                manifest["elapsedWallSeconds"] = round(elapsed, 3)
                write_json(output / "manifest.json", manifest)
                time.sleep(2)
            chain["exitCode"] = child.wait()
            load_progress(output, chain)
            chain["runs"] = scan_runs(output, chain_id)
            include_started_run(chain)
            summary = output / "chains" / chain_id / "chain-summary.json"
            if summary.is_file():
                chain["summary"] = json.loads(summary.read_text(encoding="utf-8"))
            if chain["state"] == "running":
                if chain["exitCode"] == 0 and "summary" in chain:
                    reason = chain["summary"].get("stopReason")
                    chain["state"] = "completed" if reason in ("maxRunsPerChain", "routeCleared") else "partial"
                else:
                    chain["state"] = "failed"
            for run_data in chain["runs"]:
                run_log = output / "chains" / chain_id / "runs" / run_data["runId"] / "player.log"
                if log.is_file() and not run_log.exists():
                    shutil.copyfile(log, run_log)
        except KeyboardInterrupt:
            if child is not None:
                stop_child(child)
            chain["state"] = "cancelled"
            manifest["state"] = "cancelled"
            manifest["endedUtc"] = utc_now()
            write_json(output / "manifest.json", manifest)
            return 130
        except (OSError, ValueError) as error:
            if child is not None:
                stop_child(child)
            chain["state"] = "failed"
            chain["error"] = str(error)
        chain["endedUtc"] = utc_now()
        write_json(output / "manifest.json", manifest)
        if chain["state"] != "completed":
            break
    states = [chain["state"] for chain in manifest["chains"]]
    manifest["state"] = "completed" if len(states) == config["chains"] and all(state == "completed" for state in states) else \
        "partial" if states and states[-1] in ("wallBudget", "partial") else "failed"
    manifest["endedUtc"] = utc_now()
    manifest["elapsedWallSeconds"] = round(time.monotonic() - started, 3)
    write_json(output / "manifest.json", manifest)
    return 0 if manifest["state"] in ("completed", "partial") else 3


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--experiment", type=Path, required=True)
    parser.add_argument("--player", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--validate-only", action="store_true")
    parser.add_argument("--visual", action="store_true", help="Show this run's game window")
    parser.add_argument("--audio", action="store_true", help="Enable game sound; requires --visual")
    args = parser.parse_args()
    try:
        if args.audio and not args.visual:
            raise ValueError("--audio requires --visual")
        experiment, player, output = (value.resolve() for value in (args.experiment, args.player, args.output))
        if args.validate_only:
            validate(experiment, player, output)
            print("VALID")
            return 0
        return run(experiment, player, output, args.visual, args.audio)
    except (OSError, ValueError, json.JSONDecodeError) as error:
        print(f"Balance runner: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
