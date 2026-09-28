"""Production content: sources. Numeric inputs live in the approved authoring sources."""
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"


LATE_PACKET = ROOT / "docs/balance/late-skills-passives-v1.json"


SETS_PACKET = ROOT / "docs/balance/sets-v1.json"


ENEMIES_PACKET = ROOT / "docs/balance/enemies-v1.json"


FIELD002_PACKET = ROOT / "docs/balance/field002-v1.json"


BOSSES_PACKET = ROOT / "docs/balance/bosses-v1.json"


FIELD003_PACKET = ROOT / "docs/balance/field003-v1.json"


LAYOUTS_PACKET = ROOT / "docs/balance/field-layouts-v1.json"


CHARACTERS_PACKET = ROOT / "docs/balance/characters-v1.json"


TRAVELERS_PACKET = ROOT / "docs/balance/travelers-v1.json"


def load_baseline():
    data = json.loads(BASELINE.read_text(encoding="utf-8"))
    if data.get("approval") != "Approved":
        raise SystemExit("Baseline is not Approved; production content cannot be generated.")
    late = json.loads(LATE_PACKET.read_text(encoding="utf-8"))
    if not str(late.get("approval", "")).startswith("Approved"):
        raise SystemExit("Late skills/passives packet is not Approved; production content cannot be generated.")
    data["late"] = late
    late_sets = json.loads(SETS_PACKET.read_text(encoding="utf-8"))
    if not str(late_sets.get("approval", "")).startswith("Approved"):
        raise SystemExit("Sets packet is not Approved; production content cannot be generated.")
    data["lateSets"] = late_sets
    late_enemies = json.loads(ENEMIES_PACKET.read_text(encoding="utf-8"))
    if not str(late_enemies.get("approval", "")).startswith("Approved"):
        raise SystemExit("Enemies packet is not Approved; production content cannot be generated.")
    data["lateEnemies"] = late_enemies
    field_two = json.loads(FIELD002_PACKET.read_text(encoding="utf-8"))
    if not str(field_two.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-002 packet is not Approved; production content cannot be generated.")
    data["field002"] = field_two
    late_bosses_packet = json.loads(BOSSES_PACKET.read_text(encoding="utf-8"))
    if not str(late_bosses_packet.get("approval", "")).startswith("Approved"):
        raise SystemExit("Bosses packet is not Approved; production content cannot be generated.")
    data["lateBosses"] = late_bosses_packet
    field_three = json.loads(FIELD003_PACKET.read_text(encoding="utf-8"))
    if not str(field_three.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-003 packet is not Approved; production content cannot be generated.")
    data["field003"] = field_three
    layouts = json.loads(LAYOUTS_PACKET.read_text(encoding="utf-8"))
    if not str(layouts.get("approval", "")).startswith("Approved"):
        raise SystemExit("Field layouts packet is not Approved; production content cannot be generated.")
    data["layouts"] = {entry["presentationId"]: entry for entry in layouts["fields"]}
    roster = json.loads(CHARACTERS_PACKET.read_text(encoding="utf-8"))
    if not str(roster.get("approval", "")).startswith("Approved"):
        raise SystemExit("Characters packet is not Approved; production content cannot be generated.")
    data["lateCharacters"] = roster
    late_travelers = json.loads(TRAVELERS_PACKET.read_text(encoding="utf-8"))
    if not str(late_travelers.get("approval", "")).startswith("Approved"):
        raise SystemExit("Travelers packet is not Approved; production content cannot be generated.")
    data["lateTravelers"] = late_travelers
    return data


def content_design_names(prefix):
    """Card titles from Content Design, e.g. '#### PASSIVE-001 — Крепкое сердце'."""
    import re
    text = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    return {m.group(1): m.group(2).strip() for m in re.finditer(rf"^#+ ({prefix}-\d{{3}}) — (.+)$", text, re.M)}


def card_field(card_id, field):
    """'Эффект: ...' style line from a Content Design card."""
    import re
    text = (ROOT / "docs/Content_design.md").read_text(encoding="utf-8")
    block = re.search(rf"^#+ {card_id} — .+?(?=^#+ )", text, re.M | re.S).group(0)
    return re.search(rf"^{field}: (.+)$", block, re.M).group(1).strip()


# Complete input set for generation and check_project's reusable evidence fingerprint.
SOURCE_PATHS = tuple(str(path.relative_to(ROOT)).replace("\\", "/") for path in (
    BASELINE, LATE_PACKET, SETS_PACKET, ENEMIES_PACKET, FIELD002_PACKET,
    BOSSES_PACKET, FIELD003_PACKET, LAYOUTS_PACKET, CHARACTERS_PACKET, TRAVELERS_PACKET,
    ROOT / "docs/Content_design.md",
    ROOT / "Assets/Resources/Content/Presentation/FixtureFieldEnvironmentPresentation.json",
))
