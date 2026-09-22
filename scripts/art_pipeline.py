"""Prepare an explicitly approved art packet. Dry-run by default; never writes .meta."""
import argparse
import copy
import hashlib
import io
import json
import math
import os
from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = "Art/asset-manifest.json"
SPRITES = "Assets/Resources/Content/Presentation/FixtureSprites.json"
PROFILES = "Art/ImportProfiles.json"
ROLES = {s.lower(): s for s in (
    "Body", "Shadow", "Portrait", "Icon", "Projectile", "Pickup", "Telegraph",
    "Impact", "Weapon", "Mask", "Background", "Tile", "Prop")}


def sha(data):
    return hashlib.sha256(data).hexdigest()


def read_json(path):
    def reject(value):
        raise ValueError(f"Non-finite JSON number: {value}")
    def unique(pairs):
        result = {}
        for key, value in pairs:
            if key in result:
                raise ValueError(f"Duplicate JSON key: {key}")
            result[key] = value
        return result
    return json.loads(Path(path).read_text(encoding="utf-8-sig"),
                      parse_constant=reject, object_pairs_hook=unique)


def json_bytes(value):
    return (json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False) + "\n").encode("utf-8")


def inside(root, relative, prefix=None):
    relative = str(relative).replace("\\", "/")
    if Path(relative).is_absolute() or ":" in relative or ".." in relative.split("/"):
        raise ValueError(f"Expected a repository-relative path: {relative}")
    path = (root / relative).resolve()
    if not path.is_relative_to(root.resolve()) or (prefix and not relative.startswith(prefix)):
        raise ValueError(f"Path outside permitted tree: {relative}")
    return path


def required(obj, *keys):
    for key in keys:
        if not isinstance(obj.get(key), str) or not obj[key].strip() or "<" in obj[key]:
            raise ValueError(f"Missing or placeholder field: {key}")


class WritePlan:
    """Preflight all writes, preserve existing bytes on no-op, roll back our own writes."""
    def __init__(self, root):
        self.root = root.resolve()
        self.changes = {}

    def add(self, path, data):
        if not path.resolve().is_relative_to(self.root):
            raise ValueError(f"Write follows a link outside the repository: {path}")
        if path.suffix == ".meta":
            raise ValueError("Unity owns .meta files")
        before = path.read_bytes() if path.exists() else None
        if before != data:
            if path in self.changes and self.changes[path][1] != data:
                raise ValueError(f"Conflicting writes: {path}")
            self.changes[path] = (before, data)

    def add_json(self, path, value):
        if path.exists() and read_json(path) == value:
            return
        self.add(path, json_bytes(value))

    @staticmethod
    def replace(path, data):
        path.parent.mkdir(parents=True, exist_ok=True)
        # Exclusive sibling file; no fixed temp filename shared by other tasks.
        import tempfile
        with tempfile.NamedTemporaryFile(dir=path.parent, delete=False) as stream:
            temporary = Path(stream.name)
            stream.write(data)
        try:
            os.replace(temporary, path)
        finally:
            temporary.unlink(missing_ok=True)

    def apply(self):
        written = []
        try:
            for path, (before, data) in self.changes.items():
                actual = path.read_bytes() if path.exists() else None
                if actual != before:
                    raise ValueError(f"File changed since preflight: {path.relative_to(self.root)}")
                self.replace(path, data)
                written.append(path)
        except Exception:
            for path in reversed(written):
                before, data = self.changes[path]
                if path.read_bytes() != data:
                    continue  # Never roll back another writer's change.
                if before is None:
                    path.unlink()
                else:
                    self.replace(path, before)
            raise


def prepare_png(data, recipe):
    from PIL import Image
    with Image.open(io.BytesIO(data)) as source:
        if source.format != "PNG" or source.mode != "RGBA":
            raise ValueError("Approved input must be RGBA PNG")
        source.load()
        dimensions = f"{source.width}x{source.height}"
        mode = recipe.get("mode")
        if mode == "copy":
            return data, dimensions
        if mode != "fit":
            raise ValueError("preparation.mode must be copy or fit")
        size, padding = recipe.get("size"), recipe.get("padding")
        if type(size) is not int or size < 16 or size > 2048 or size & (size - 1):
            raise ValueError("fit size must be a power of two in [16, 2048]")
        if type(padding) is not int or not 2 <= padding < size // 2:
            raise ValueError("fit padding must leave at least two transparent pixels")
        if type(recipe.get("cropAlpha")) is not bool:
            raise ValueError("fit requires an explicit cropAlpha boolean")
        image = source.copy()
        if recipe["cropAlpha"]:
            bounds = image.getchannel("A").getbbox()
            if bounds is None:
                raise ValueError("Empty alpha silhouette")
            image = image.crop(bounds)
        image.thumbnail((size - padding * 2, size - padding * 2), Image.Resampling.LANCZOS)
        result = Image.new("RGBA", (size, size))
        result.alpha_composite(image, ((size - image.width) // 2, (size - image.height) // 2))
        output = io.BytesIO()
        result.save(output, format="PNG")
        return output.getvalue(), dimensions


def upsert(items, new, key):
    matches = [i for i, item in enumerate(items) if key(item) == key(new)]
    if len(matches) > 1:
        raise ValueError(f"Duplicate identity: {key(new)}")
    if matches:
        items[matches[0]] = new
    else:
        items.append(new)


def validate_profile(profile):
    required(profile, "reason")
    for field in ("pixelsPerUnit", "pivotX", "pivotY"):
        value = profile.get(field)
        if type(value) not in (int, float) or not math.isfinite(value):
            raise ValueError(f"Import profile needs finite {field}")
    size = profile.get("maxSize")
    if (profile["pixelsPerUnit"] <= 0 or not 0 <= profile["pivotX"] <= 1 or
            not 0 <= profile["pivotY"] <= 1 or type(size) is not int or
            not 32 <= size <= 8192 or size & (size - 1)):
        raise ValueError("Invalid import profile PPU, pivot or power-of-two maxSize")


def build_plan(packet, root=ROOT):
    root = root.resolve()
    if packet.get("schemaVersion") != 1 or packet.get("approvedBy") != "user":
        raise ValueError("Packet needs schemaVersion 1 and recorded user approval")
    required(packet, "approvedAt", "approvalEvidence")
    import datetime
    datetime.date.fromisoformat(packet["approvedAt"])
    if not packet.get("assets"):
        raise ValueError("Empty art packet")
    manifest = read_json(root / MANIFEST)
    sprites = read_json(root / SPRITES)
    profiles = read_json(root / PROFILES)
    plan = WritePlan(root)
    bindings = {}
    identities, destinations = set(), set()
    for asset in packet["assets"]:
        required(asset, "owner", "role", "visualId", "owningIp", "input", "sha256",
                 "sourceDirectory", "version", "runtime", "prompt", "generator", "artDirectionRevision")
        role = asset["role"]
        identity = (asset["owner"], role)
        if role not in ROLES or identity in identities:
            raise ValueError(f"Invalid/duplicate asset: {identity}")
        identities.add(identity)
        if not re.fullmatch(r"[A-Z0-9]+(?:-[A-Z0-9]+)+", asset["visualId"]):
            raise ValueError("visualId must be an explicit stable content ID")
        if not re.fullmatch(r"v\d{3}", asset["version"]):
            raise ValueError("version must be vNNN")
        directory = inside(root, asset["sourceDirectory"], "Art/Source/")
        runtime = inside(root, asset["runtime"], "Assets/Resources/Art/")
        if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)+\.png", runtime.name) or not runtime.stem.endswith("-" + role):
            raise ValueError("Runtime name must be lowercase kebab-case with a role suffix")
        if role in ("icon", "portrait") and not asset["runtime"].startswith("Assets/Resources/Art/UI/"):
            raise ValueError("Icon/portrait runtime paths must use the UI category")
        for target in (directory, runtime):
            if target in destinations:
                raise ValueError(f"Shared destination in packet: {target}")
            destinations.add(target)
        source_path = Path(asset["input"])
        if not source_path.is_absolute():
            source_path = inside(root, asset["input"])
        original = source_path.read_bytes()
        if sha(original) != asset["sha256"]:
            raise ValueError(f"Approved input hash mismatch: {identity}")
        recipe = asset.get("preparation", {})
        if role == "body" and recipe.get("mode") != "copy":
            raise ValueError("Body normalization needs authored ground-contact preparation; use an approved prepared input")
        derivative, dimensions = prepare_png(original, recipe)
        candidate = directory / asset["version"] / "concept-01.png"
        master = directory / "selected-master.png"
        record_path = directory / "asset-record.json"
        if candidate.exists() and candidate.read_bytes() != original:
            raise ValueError("Versioned candidates are immutable; use a new version")
        old_record = read_json(record_path) if record_path.exists() else {}
        if old_record and old_record.get("contentId") != asset["owner"]:
            raise ValueError("Source directory belongs to another owner")
        replacing = master.exists() and master.read_bytes() != original
        if replacing and (asset.get("replacementApproved") is not True or asset.get("replacesSha256") != sha(master.read_bytes())):
            raise ValueError("Replacement requires explicit approval and the previous master hash")
        resource = runtime.relative_to(root / "Assets/Resources").with_suffix("").as_posix()
        sprite = copy.deepcopy(asset.get("sprite", {}))
        sprite.update(id=asset["visualId"], resourcePath=resource, role=ROLES[role])
        if role == "body" and not all(k in sprite for k in ("contactRadius", "contactCenterY")):
            raise ValueError("Body needs explicitly authored contact values")
        if role == "projectile" and not sprite.get("projectile"):
            raise ValueError("Projectile needs its complete presentation profile")
        old_entries = [e for e in manifest["entries"] if (e["owner"], e["role"]) == identity]
        old_entry = old_entries[0] if old_entries else None
        if runtime.exists() and old_entry is None:
            raise ValueError("Refusing to overwrite a runtime PNG without its matching manifest entry")
        if old_entry and (old_entry.get("runtime") != asset["runtime"] or old_entry.get("visualId") != asset["visualId"]):
            raise ValueError("Runtime paths and IDs are stable; migrations are separate work")
        for entry in manifest["entries"]:
            if (entry["owner"], entry["role"]) != identity and (
                    entry.get("runtime") == asset["runtime"] or entry.get("visualId") == asset["visualId"]):
                raise ValueError("Runtime path or visual ID belongs to another asset")
        current_sprite = next((s for s in sprites if s["id"] == sprite["id"]), None)
        if current_sprite and current_sprite != sprite:
            raise ValueError("Existing sprite definition differs; keep its full profile or migrate separately")
        upsert(sprites, sprite, lambda item: item["id"])
        profile = asset.get("importProfile")
        if profile:
            profile = dict(profile, key=asset["runtime"])
            validate_profile(profile)
            current = next((p for p in profiles if p["key"] == profile["key"]), None)
            if current and current != profile:
                raise ValueError("Changing an existing import profile requires a separate reviewed change")
            upsert(profiles, profile, lambda p: p["key"])
        if not any(p["key"] == asset["runtime"] or (role != "body" and p["key"] == role) for p in profiles):
            raise ValueError("Missing category/exact import profile")
        record = copy.deepcopy(old_record)
        record.pop("generationPrompt", None)  # One verbatim prompt field, including after replacement.
        record.update(schemaVersion=1, contentId=asset["owner"], role=role, status="approved",
                      artDirectionRevision=asset["artDirectionRevision"], sourceKind=asset.get("sourceKind", "ai-generated"),
                      generator=asset["generator"], selectedVersion=asset["version"] + "/concept-01.png",
                      approvedAt=packet["approvedAt"], approvedBy="user", approvalEvidence=packet["approvalEvidence"],
                      prompt=asset["prompt"], constraints=asset.get("constraints", []), runtimeOutputs=[asset["runtime"]],
                      preparation={"sourceDimensions": dimensions, "sourceSha256": sha(original),
                                   "runtimeSha256": sha(derivative), "recipe": recipe})
        entry = dict(old_entry or {}, owner=asset["owner"], role=role, visualId=asset["visualId"],
                     method=asset.get("method", "Generate"), owningIp=asset["owningIp"],
                     source=master.relative_to(root).as_posix(), provenance=record_path.relative_to(root).as_posix(),
                     runtime=asset["runtime"], resourcePath=resource,
                     stage="Prepared", evidence=[packet["approvalEvidence"]],
                     missing="Unity import checks and target-scale visual review.")
        if old_entry and runtime.exists() and runtime.read_bytes() == derivative and not replacing:
            entry = old_entry  # Repeating preparation never downgrades existing review evidence.
        upsert(manifest["entries"], entry, lambda e: (e["owner"], e["role"]))
        for binding in asset.get("bindings", []):
            path = inside(root, binding["file"], "Assets/Resources/Content/")
            if path.suffix != ".json" or path == root / SPRITES:
                raise ValueError("Bindings target existing owner content JSON")
            if path not in bindings:
                bindings[path] = read_json(path)
            if not isinstance(bindings[path], list):
                raise ValueError("Binding file must contain an existing owner array")
            matches = [o for o in bindings[path] if o.get("id") == binding["ownerId"]]
            if len(matches) != 1:
                raise ValueError("Binding owner must already exist exactly once; no new production content")
            target = matches[0]
            parts = binding["pointer"].split("/")
            if parts[0] != "" or parts[-1] not in ("visualId", "iconVisualId", "bodyVisualId", "portraitVisualId"):
                raise ValueError("Binding pointer must end in a supported visual-reference field")
            for part in parts[1:-1]:
                if isinstance(target, list) and not part.isdigit():
                    raise ValueError("Binding array index must be non-negative")
                target = target[int(part)] if isinstance(target, list) else target[part]
            current = target.get(parts[-1])
            if current != sprite["id"] and ("expect" not in binding or current != binding["expect"]):
                raise ValueError("Binding changed since packet was authored")
            target[parts[-1]] = sprite["id"]
        for path, data in ((candidate, original), (master, original), (runtime, derivative)):
            plan.add(path, data)
        plan.add_json(record_path, record)
    plan.add_json(root / MANIFEST, manifest)
    plan.add_json(root / SPRITES, sprites)
    plan.add_json(root / PROFILES, profiles)
    for path, value in bindings.items():
        plan.add_json(path, value)
    return plan


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("packet", type=Path)
    parser.add_argument("--apply", action="store_true", help="Apply an already approved packet after preflight")
    args = parser.parse_args()
    try:
        plan = build_plan(read_json(args.packet))
        if args.apply:
            plan.apply()
        print(json.dumps({"result": "APPLIED" if args.apply else "PLAN", "changedFiles": len(plan.changes),
                          "paths": [p.relative_to(ROOT).as_posix() for p in plan.changes],
                          "next": "Run check_project.py --scope art; visual acceptance remains separate."}, ensure_ascii=False))
        return 0
    except (ValueError, OSError, KeyError, TypeError, ImportError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    sys.exit(main())
