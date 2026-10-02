"""Production content: sources. Numeric inputs live in the approved authoring sources."""
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]


BASELINE = ROOT / "docs/balance/field001-baseline-v1.json"


LATE_PACKET = ROOT / "docs/balance/late-skills-passives-v1.json"


SETS_PACKET = ROOT / "docs/balance/sets-v1.json"


LOW_SETS_PACKET = ROOT / "docs/balance/sets-low-v1.json"


ENEMIES_PACKET = ROOT / "docs/balance/enemies-v1.json"


FIELD002_PACKET = ROOT / "docs/balance/field002-v2.json"


BOSSES_PACKET = ROOT / "docs/balance/bosses-v1.json"


FIELD003_PACKET = ROOT / "docs/balance/field003-v2.json"
FIELD003_ROADS_PACKET = ROOT / "docs/balance/field003-roads-v1.json"


FIELD004_PACKET = ROOT / "docs/balance/field004-v1.json"


LAYOUTS_PACKET = ROOT / "docs/balance/field-layouts-v1.json"


FIELD_DEV_BLOBS_PACKET = ROOT / "docs/balance/field-dev-blobs-v1.json"


FIELD_DEV_ZONES_PACKET = ROOT / "docs/balance/field-dev-zones-v1.json"


FIELD_DEV_ALTARS_PACKET = ROOT / "docs/balance/field-dev-altars-v1.json"
FIELD006_PACKET = ROOT / "docs/balance/field006-zones-v1.json"
FIELD007_PACKET = ROOT / "docs/balance/field007-altars-v1.json"


CHARACTERS_PACKET = ROOT / "docs/balance/characters-v1.json"


TRAVELERS_PACKET = ROOT / "docs/balance/travelers-v1.json"


TRAVELERS_V2_PACKET = ROOT / "docs/balance/travelers-v2.json"


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
    low_sets = json.loads(LOW_SETS_PACKET.read_text(encoding="utf-8"))
    if not str(low_sets.get("approval", "")).startswith("Approved"):
        raise SystemExit("Low-tier sets packet is not Approved; production content cannot be generated.")
    data["lowSets"] = low_sets
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
    road_packet = json.loads(FIELD003_ROADS_PACKET.read_text(encoding="utf-8"))
    if not road_packet.get("approval", "").startswith("Approved"):
        raise SystemExit("FIELD-003 roads must be approved.")
    data["field003Roads"] = road_packet
    field_four = json.loads(FIELD004_PACKET.read_text(encoding="utf-8"))
    if not str(field_four.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-004 packet is not Approved; production content cannot be generated.")
    data["field004"] = field_four
    shared_cap = data["timeline"]["maxAliveEnemies"]
    if field_two["timeline"]["maxAliveEnemies"] != shared_cap or field_three["timeline"]["maxAliveEnemies"] != shared_cap or field_four["timeline"]["maxAliveEnemies"] != shared_cap:
        raise SystemExit("All production fields must share one ordinary-enemy cap.")
    layouts = json.loads(LAYOUTS_PACKET.read_text(encoding="utf-8"))
    if not str(layouts.get("approval", "")).startswith("Approved"):
        raise SystemExit("Field layouts packet is not Approved; production content cannot be generated.")
    data["layouts"] = {entry["presentationId"]: entry for entry in layouts["fields"]}
    dev_blobs = json.loads(FIELD_DEV_BLOBS_PACKET.read_text(encoding="utf-8"))
    if not str(dev_blobs.get("approval", "")).startswith("Approved"):
        raise SystemExit("Dev blobs packet is not Approved; production content cannot be generated.")
    data["devBlobs"] = dev_blobs
    dev_zones = json.loads(FIELD_DEV_ZONES_PACKET.read_text(encoding="utf-8"))
    if not str(dev_zones.get("approval", "")).startswith("Approved"):
        raise SystemExit("Dev zones packet is not Approved; production content cannot be generated.")
    data["devZones"] = dev_zones
    dev_altars = json.loads(FIELD_DEV_ALTARS_PACKET.read_text(encoding="utf-8"))
    if not str(dev_altars.get("approval", "")).startswith("Approved"):
        raise SystemExit("Dev altars packet is not Approved; production content cannot be generated.")
    data["devAltars"] = dev_altars
    academy = json.loads(FIELD006_PACKET.read_text(encoding="utf-8"))
    if not str(academy.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-006 zone preview packet is not Approved.")
    data["field006"] = academy
    monastery = json.loads(FIELD007_PACKET.read_text(encoding="utf-8"))
    if not str(monastery.get("approval", "")).startswith("Approved"):
        raise SystemExit("FIELD-007 altar preview packet is not Approved.")
    data["field007"] = monastery
    roster = json.loads(CHARACTERS_PACKET.read_text(encoding="utf-8"))
    if not str(roster.get("approval", "")).startswith("Approved"):
        raise SystemExit("Characters packet is not Approved; production content cannot be generated.")
    data["lateCharacters"] = roster
    late_travelers = json.loads(TRAVELERS_PACKET.read_text(encoding="utf-8"))
    if not str(late_travelers.get("approval", "")).startswith("Approved"):
        raise SystemExit("Travelers packet is not Approved; production content cannot be generated.")
    data["lateTravelers"] = late_travelers
    travelers_v2 = json.loads(TRAVELERS_V2_PACKET.read_text(encoding="utf-8"))
    if not str(travelers_v2.get("approval", "")).startswith("Approved"):
        raise SystemExit("Travelers v2 packet is not Approved; production content cannot be generated.")
    data["travelersV2"] = travelers_v2
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
    BASELINE, LATE_PACKET, SETS_PACKET, LOW_SETS_PACKET, ENEMIES_PACKET, FIELD002_PACKET,
    BOSSES_PACKET, FIELD003_PACKET, FIELD003_ROADS_PACKET, FIELD004_PACKET, FIELD006_PACKET, FIELD007_PACKET, LAYOUTS_PACKET, FIELD_DEV_BLOBS_PACKET, FIELD_DEV_ZONES_PACKET, FIELD_DEV_ALTARS_PACKET, CHARACTERS_PACKET, TRAVELERS_PACKET, TRAVELERS_V2_PACKET,
    ROOT / "docs/Content_design.md",
    ROOT / "Assets/Resources/Content/Presentation/FixtureFieldEnvironmentPresentation.json",
))
