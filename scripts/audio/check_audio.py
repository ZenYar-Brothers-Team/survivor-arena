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
    by_resource = {item["file"].removeprefix("Assets/Resources/").rsplit(".", 1)[0]: item["file"] for item in sources}
    assert len(by_resource) == len(sources), "Duplicate audio resource path (including extension aliases)"
    assert set(paths) <= set(by_resource), "Unrecorded audio reference"
    expected = {by_resource[path] for path in paths}
    actual = {path.relative_to(ROOT).as_posix() for path in AUDIO_DIR.rglob("*") if path.suffix.lower() in (".ogg", ".wav")}
    recorded = {item["file"] for item in sources}
    assert expected == actual == recorded, f"Audio reference mismatch: {(expected ^ actual) | (expected ^ recorded)}"
    assert len(recorded) == len(sources), "Duplicate source record"
    for item in sources:
        file = ROOT / item["file"]
        assert file.resolve().is_relative_to(AUDIO_DIR.resolve()), file
        assert hashlib.sha256(file.read_bytes()).hexdigest() == item["sha256"], file
        if item.get("sourceKind", "external") == "procedural":
            generator = (ROOT / item["source"]).resolve()
            assert generator.is_relative_to((ROOT / "Art/Prototypes").resolve()) and generator.is_file(), file
            assert hashlib.sha256(generator.read_bytes()).hexdigest() == item["sourceSha256"], generator
            assert item["license"] == "Project original" and item["author"] == "Codex (procedural synthesis)", file
            assert item["approval"] and item["seed"] >= 0, file
        else:
            assert item.get("sourceKind", "external") == "external", file
            assert item["license"] == "CC0 1.0" and item["page"].startswith("https://"), file
    ids = [cue["id"] for cue in catalog["cues"]]
    assert len(ids) == len(set(ids)), "Duplicate cue ID"
    bindings = catalog["screenEventCues"]
    event_ids = [entry["eventId"] for entry in bindings]
    assert len(event_ids) == len(set(event_ids)), "Duplicate screen-event cue binding"
    environments = json.loads((ROOT / "Assets/Resources/Content/Presentation/ProductionFieldEnvironmentPresentation.json").read_text(encoding="utf-8"))
    known_events = {event["id"] for env in environments for event in env.get("screenEvents", {}).get("events", [])}
    assert set(event_ids) == known_events, "All production screen events need one audio binding"
    assert all(entry["cue"] in ids for entry in bindings), "Unknown screen-event audio cue"
    print(f"PASS: {len(sources)} files, {len(ids)} cues, hashes/licenses/references valid")


if __name__ == "__main__":
    main()
