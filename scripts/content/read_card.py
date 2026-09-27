"""Print one complete canonical content card, without copying it into an index."""
import argparse
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[2]
CARD = re.compile(r"^#{1,6}\s+((?:SKILL|PASSIVE|SET|ENEMY|MIDBOSS|BOSS|TRAVELER|CHAR|FIELD|PICKUP)-\d{3})\s+[—–-]\s+(.+)$")
SECTIONS = {
    "Active Skills", "Passive Items", "Sets", "Мета-экономика",
    "Стартовый набор и дальнейшие открытия — DECISION-0050", "World Pickups",
    "Enemies", "Bosses", "Mini-bosses / Mid-bosses", "Travelers / Путники", "Characters", "Fields", "Wave / Encounter Content",
}


def cards(text):
    """Keep field headings inside enemy/boss cards even when they use the card's heading level."""
    lines = text.splitlines()
    entries = []
    current = None
    for index, line in enumerate(lines):
        match = CARD.match(line)
        heading = re.match(r"^#{1,6}\s+(.+)$", line)
        boundary = match or (heading and heading.group(1) in SECTIONS)
        if boundary and current is not None:
            card_id, title, start = current
            entries.append((card_id, title, start + 1, "\n".join(lines[start:index]).rstrip()))
            current = None
        if match:
            current = (match.group(1), match.group(2), index)
    if current is not None:
        card_id, title, start = current
        entries.append((card_id, title, start + 1, "\n".join(lines[start:]).rstrip()))
    ids = [entry[0] for entry in entries]
    if len(ids) != len(set(ids)):
        raise ValueError("Duplicate canonical content-card headings")
    return entries


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("id", nargs="?", help="Stable content ID, e.g. SKILL-006")
    parser.add_argument("--list", action="store_true", help="List IDs, titles and source lines only")
    args = parser.parse_args()
    entries = cards((ROOT / "docs/Content_design.md").read_text(encoding="utf-8"))
    if args.list:
        for card_id, title, line, _ in entries:
            print(f"{card_id} | {title} | docs/Content_design.md:{line}")
        return
    matches = [entry for entry in entries if entry[0] == args.id]
    if not matches:
        parser.error("Supply an existing content ID, or use --list")
    _, _, line, text = matches[0]
    print(f"Source: docs/Content_design.md:{line}\n\n{text}")


if __name__ == "__main__":
    main()
