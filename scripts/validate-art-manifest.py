"""Read-only provenance/path audit. Does not evaluate pixels or approve artwork."""
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parent.parent
manifest = json.loads((root / "Art/asset-manifest.json").read_text(encoding="utf-8"))
seen = set()
for entry in manifest["entries"]:
    key = (entry["owner"], entry["role"])
    assert key not in seen, f"Duplicate owner/role: {key}"
    seen.add(key)
    assert entry["method"] in ("Generate", "Procedural", "Hybrid"), key
    assert entry["stage"] in ("Brief ready", "Generated", "Review", "Image approved", "Prepared", "Imported", "Integrated", "Verified in game"), key
    assert entry["evidence"] and entry["owningIp"], key
    for field in ("source", "provenance", "runtime"):
        if entry[field]:
            path = (root / entry[field]).resolve()
            assert path.is_relative_to(root) and path.is_file(), (key, field)
    if entry["source"]:
        assert not entry["source"].startswith("Assets/"), key
    if entry["resourcePath"]:
        assert entry["runtime"] == "Assets/Resources/" + entry["resourcePath"] + ".png", key
        assert (root / (entry["runtime"] + ".meta")).is_file(), key
record_path = root / "Art/Source/Characters/fixture-character-agile/asset-record.json"
record = json.loads(record_path.read_text(encoding="utf-8"))
master = record_path.parent / "selected-master.png"
selected = record_path.parent / record["selectedVersion"]
assert hashlib.sha256(master.read_bytes()).digest() == hashlib.sha256(selected.read_bytes()).digest()
assert any(b["contentId"] == "CHAR-001" and b["confirmedBy"] == "user" for b in record["conceptBindings"])
print(f"PASS: {len(seen)} owner/role records; source/version identity and paths valid. Pixel quality not evaluated.")
