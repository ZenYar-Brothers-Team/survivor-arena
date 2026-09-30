"""Static checks for docs/balance/travelers-v2.json (movement and support changes of DECISION-0120, not runtime JSON)."""
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PATH = ROOT / "docs/balance/travelers-v2.json"
STYLE_FIELDS = {
    "ZigzagEscape": {"zigzagAngleDegrees", "zigzagSeconds"},
    "DashEscape": {"escapeDashDistance", "escapeDashSeconds", "escapeDashCooldownSeconds"},
    "Orbit": {"orbitRadiusX", "orbitRadiusY", "orbitLeadDegrees", "teleportMinSeconds", "teleportMaxSeconds"},
}
SHAPES = {"Ring", "Hexagon", "Ripples", "Dots"}
WANDERERS = {"TRAVELER-002", "TRAVELER-003", "TRAVELER-006"}
SUPPORT_EXTRA = {"Aura": set(), "SpeedBurst": {"effectRadius", "speedBonus", "effectSeconds"}, "Heal": {"healAmount"}}


def require(condition, message):
    if not condition:
        print(f"FAIL: {message}")
        sys.exit(1)


def main():
    data = json.loads(PATH.read_text(encoding="utf-8"))
    require(data["revision"] == "travelers-v2" and data["approval"].startswith("Approved"), "revision/approval")
    require(0.1 <= data["supportVerticalScale"] <= 1, "supportVerticalScale range")
    styles = {}
    for tid, override in data["overrides"].items():
        style = override.get("movementStyle")
        if style:
            require(tid in WANDERERS, f"{tid}: only peaceful wanderers change movement")
            styles[style] = tid
            require(set(override["style"]) == STYLE_FIELDS[style], f"{tid}: {style} needs exactly {sorted(STYLE_FIELDS[style])}")
            require(all(v > 0 for v in override["style"].values()), f"{tid}: style values must be positive")
            if style == "Orbit":
                require(override["style"]["teleportMaxSeconds"] >= override["style"]["teleportMinSeconds"], f"{tid}: teleport interval")
                require(override["style"]["orbitLeadDegrees"] <= 90, f"{tid}: orbit lead")
                require(len(override["effectColor"]) == 4 and override["effectShape"] in SHAPES, f"{tid}: teleport effect color/shape")
        support = override.get("support")
        if support:
            kind = support["kind"]
            require(kind in SUPPORT_EXTRA, f"{tid}: support kind {kind}")
            require(support["radius"] > 0 and support["cooldownSeconds"] > 0, f"{tid}: radius/cooldown")
            require(SUPPORT_EXTRA[kind] <= set(support), f"{tid}: {kind} fields")
            require(len(override["effectColor"]) == 4 and override["effectShape"] in SHAPES, f"{tid}: support effect color/shape")
            if kind == "Aura":
                require(0 < support["reduction"] < 1, f"{tid}: aura reduction")
            if kind == "SpeedBurst":
                require(support["effectSeconds"] > 0 and 0 < support["speedBonus"] <= 1, f"{tid}: speed burst")
                require(support["effectRadius"] < support["radius"], f"{tid}: burst radius must be small")
    require(set(styles) == set(STYLE_FIELDS), "zigzag, dash and orbit are each used once")
    require({"TRAVELER-005", "TRAVELER-007", "TRAVELER-009"} <= set(data["overrides"]), "three protectors")
    schedule = data["schedule"]
    require(schedule["minCount"] >= 0 and len(schedule["countProbabilities"]) >= 1, "schedule count range")
    require(abs(sum(schedule["countProbabilities"]) - 1) < 1e-6, "schedule probabilities sum to 1")
    require(schedule["minCount"] == 1 and len(schedule["countProbabilities"]) == 5, "DECISION-0122: 1 to 5 Travelers")
    print(f"PASS: travelers-v2 {len(data['overrides'])} overrides")


if __name__ == "__main__":
    main()
