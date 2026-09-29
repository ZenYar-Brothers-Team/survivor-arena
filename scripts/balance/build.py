"""Build a separate development balance player with the repository's Editor safety preflight."""
from __future__ import annotations

import argparse
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))
from check_project import editor_processes, ensure_unlocked, find_unity, NotRun  # noqa: E402


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path, help="New absolute or relative .exe path")
    args = parser.parse_args()
    output = args.output.resolve()
    if output.suffix.lower() != ".exe" or output.exists() or output.with_name(output.stem + "_Data").exists():
        parser.error("Output must be a new .exe and data folder")
    try:
        processes = editor_processes(ROOT)
        if processes:
            raise NotRun("Unity Editor is already open for this worktree; close it before a batch build")
        ensure_unlocked(ROOT)
        unity = find_unity(ROOT)
        # Repeat immediately before launch; an open Editor is never replaced or closed.
        if editor_processes(ROOT):
            raise NotRun("Unity Editor opened before build launch")
        ensure_unlocked(ROOT)
        log = output.with_suffix(".build.log")
        log.parent.mkdir(parents=True, exist_ok=True)
        command = [str(unity), "-batchmode", "-nographics", "-quit", "-projectPath", str(ROOT),
                   "-executeMethod", "Game.Bootstrap.Editor.AutomationBuild.Build",
                   f"--balance-build-output={output}", "-logFile", str(log)]
        result = subprocess.run(command, cwd=ROOT, timeout=3600, check=False)
        if result.returncode != 0:
            print(f"BUILD FAILED ({result.returncode}); see {log}", file=sys.stderr)
            return result.returncode or 1
        manifest = output.parent / "build-manifest.json"
        if not output.is_file() or not manifest.is_file():
            print(f"BUILD INCOMPLETE; see {log}", file=sys.stderr)
            return 1
        print(f"BUILT {output}\nMANIFEST {manifest}\nLOG {log}")
        return 0
    except (NotRun, OSError, subprocess.TimeoutExpired) as error:
        print(f"NOT RUN / INCOMPLETE: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
