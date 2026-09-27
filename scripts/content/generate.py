"""Generate production JSON for all approved packets; --check never writes outputs."""
import sys
from pathlib import Path

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[1]))

import json
import argparse
from content.sources import ROOT, load_baseline
from content.skills import active_skills
from content.progression import passives, set_attacks, sets
from content.actors import character_baseline, characters, enemies, pickups, travelers
from content.bosses import bosses
from content.fields import field_presentation, fields, run_setup, timeline, timeline_field002, timeline_field003


TARGETS = {
    "Assets/Resources/Content/ActiveSkills/ProductionActiveSkills.json": active_skills,
    "Assets/Resources/Content/Passives/ProductionPassives.json": passives,
    "Assets/Resources/Content/Characters/ProductionCharacters.json": characters,
    "Assets/Resources/Content/Characters/ProductionCharacterBaseline.json": character_baseline,
    "Assets/Resources/Content/Enemies/ProductionEnemies.json": enemies,
    "Assets/Resources/Content/Pickups/ProductionPickups.json": pickups,
    "Assets/Resources/Content/Sets/ProductionSets.json": sets,
    "Assets/Resources/Content/Bosses/ProductionBosses.json": bosses,
    "Assets/Resources/Content/Travelers/ProductionTravelers.json": travelers,
    "Assets/Resources/Content/Fields/ProductionFields.json": fields,
    "Assets/Resources/Content/Presentation/ProductionFieldEnvironmentPresentation.json": field_presentation,
    "Assets/Resources/Content/Waves/ProductionWaveTimeline.json": timeline,
    "Assets/Resources/Content/Waves/ProductionWaveTimelineField002.json": timeline_field002,
    "Assets/Resources/Content/Waves/ProductionWaveTimelineField003.json": timeline_field003,
    "Assets/Resources/Content/Run/ProductionRunSetup.json": run_setup,
    "Assets/Resources/Content/ActiveSkills/ProductionSetAttacks.json": set_attacks,
}


def tidy(value):
    """Round float noise from multiplications (e.g. 3.3600000000000003) to 6 decimals."""
    if isinstance(value, float):
        rounded = round(value, 6)
        return int(rounded) if rounded.is_integer() else rounded
    if isinstance(value, list):
        return [tidy(item) for item in value]
    if isinstance(value, dict):
        return {key: tidy(item) for key, item in value.items()}
    return value


def render(value):
    return json.dumps(tidy(value), ensure_ascii=False, indent=2) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument("--check", action="store_true", help="fail when generated files differ")
    args = parser.parse_args()
    baseline = load_baseline()
    stale = []
    for relative, producer in TARGETS.items():
        path = ROOT / relative
        text = render(producer(baseline))
        current = path.read_text(encoding="utf-8") if path.exists() else None
        if current == text:
            continue
        stale.append(relative)
        if not args.check:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(text, encoding="utf-8", newline="\n")
    if args.check and stale:
        print("STALE: " + ", ".join(stale))
        return 1
    print(("UP TO DATE" if not stale else ("WROTE: " + ", ".join(stale))))
    return 0


if __name__ == "__main__":
    sys.exit(main())
