"""Launch a windowed, isolated human recording; each invocation gets a new output directory."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import uuid

import run as balance_run


def prepare(template: Path, output_root: Path) -> tuple[Path, Path]:
    config = json.loads(template.read_text(encoding="utf-8"))
    if config.get("movementPolicy", {}).get("id") != "human":
        raise ValueError("The recording launcher accepts only a human template")
    balance_run.validate_recording(config)
    name = "human-" + datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ-") + uuid.uuid4().hex[:8]
    config["experimentId"] = name
    config["outputDirectory"] = name
    if config.get("template") == "preset":
        config["initialProfilePath"] = str((template.parent / config["initialProfilePath"]).resolve())
    config_folder = output_root / "configs"
    config_folder.mkdir(parents=True, exist_ok=True)
    path = config_folder / (name + ".json")
    with path.open("x", encoding="utf-8") as target:
        json.dump(config, target, ensure_ascii=False, indent=2)
        target.write("\n")
    return path, output_root / name


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--player", required=True, type=Path)
    parser.add_argument("--template", type=Path, default=Path(__file__).parent / "examples/human-demonstration.json")
    parser.add_argument("--output-root", type=Path, default=Path(__file__).resolve().parents[2] / "TestResults/demonstrations")
    parser.add_argument("--audio", action="store_true", help="Explicitly enable game audio; muted by default")
    args = parser.parse_args()
    try:
        if not args.player.is_file():
            raise ValueError(f"Build a development player first: {args.player}")
        config, output = prepare(args.template.resolve(), args.output_root.resolve())
        print(f"Recording output: {output}", flush=True)
        print("Windowed 1x play. Each run starts paused: Space/Escape/Continue to start. "
              "Move with the normal controls; drafts are chosen automatically. "
              "Use the game's exit or close the window to finish and flush recording.", flush=True)
        return balance_run.run(config, args.player.resolve(), output, visual=True, audio=args.audio)
    except (OSError, ValueError, KeyError) as error:
        parser.error(str(error))


if __name__ == "__main__":
    raise SystemExit(main())
