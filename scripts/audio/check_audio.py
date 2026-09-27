"""Validate the shipped audio files, provenance hashes and Unity cue references."""

import hashlib
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "docs/audio/SOURCES.json"
CATALOG = ROOT / "Assets/Resources/Content/Audio/ProductionAudio.json"
AUDIO_DIR = ROOT / "Assets/Resources/Audio"


def main() -> None:
    sources = json.loads(MANIFEST.read_text(encoding="utf-8"))["assets"]
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    paths = [catalog[key] for key in (
        "menuMusic", "battleMusic", "bossMusic", "victoryMusic", "defeatMusic"
    )]
    ambiences = catalog["fieldAmbiences"]
    paths += [entry["clip"] for entry in ambiences]
    field_ids = [entry["fieldId"] for entry in ambiences]
    assert len(field_ids) == len(set(field_ids)), "Duplicate field ambience binding"
    fields = json.loads((ROOT / "Assets/Resources/Content/Fields/ProductionFields.json").read_text(encoding="utf-8"))
    assert set(field_ids) <= {field["id"] for field in fields["fields"]}, "Unknown ambience field"
    paths += [path for cue in catalog["cues"] for path in cue["clips"]]
    expected = {"Assets/Resources/" + path + ".ogg" for path in paths}
    actual = {str(path.relative_to(ROOT)).replace("\\", "/") for path in AUDIO_DIR.rglob("*.ogg")}
    recorded = {item["file"] for item in sources}
    assert expected == actual == recorded, f"Audio reference mismatch: {(expected ^ actual) | (expected ^ recorded)}"
    assert len(recorded) == len(sources), "Duplicate source record"
    for item in sources:
        file = ROOT / item["file"]
        assert file.resolve().is_relative_to(AUDIO_DIR.resolve()), file
        assert hashlib.sha256(file.read_bytes()).hexdigest() == item["sha256"], file
        assert item["license"] == "CC0 1.0" and item["page"].startswith("https://"), file
    ids = [cue["id"] for cue in catalog["cues"]]
    assert len(ids) == len(set(ids)), "Duplicate cue ID"
    print(f"PASS: {len(sources)} files, {len(ids)} cues, hashes/licenses/references valid")


if __name__ == "__main__":
    main()
