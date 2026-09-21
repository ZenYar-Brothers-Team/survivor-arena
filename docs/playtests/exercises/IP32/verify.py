"""Synthetic workflow rehearsal only. No game/config writes, no real approval."""
import copy
import hashlib
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
REPORT = ROOT / "docs/playtests/reports/2026-09-21_108ff5b3/run.json"

def fingerprint(value):
    return hashlib.sha256(json.dumps(value, sort_keys=True, separators=(",", ":")).encode()).hexdigest()

def require(condition, message):
    if not condition:
        raise ValueError(message)

# This models a human-reviewed protocol, NOT an application feature or authorization service.
def apply_example(current, proposal, approval):
    require(approval is not None and approval["decision"] == "accept", "no accepted decision")
    require(approval["synthetic"] is True, "exercise only")
    require(approval["revision"] == proposal["revision"], "stale revision")
    require(approval["baseline"] == proposal["baseline"] == fingerprint(current), "stale baseline")
    selected = approval["rows"]
    require(selected and set(selected) <= set(proposal["rows"]), "invalid row selection")
    result = copy.deepcopy(current)
    for key in selected:
        row = proposal["rows"][key]
        require(result[row["field"]] == row["before"], "old value mismatch")
        require(row["after"] > 0, "invalid synthetic domain value")
        result[row["field"]] = row["after"]
    return result

def main():
    game_files = list((ROOT / "Assets/Resources/Content").rglob("*.json"))
    before_files = {str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in game_files}
    report = json.loads(REPORT.read_text(encoding="utf-8-sig"))
    require(report["reportId"] == "108ff5b3e8ed457e84704dcbfa25e0f8", "wrong report")
    for source in report["provenance"]["files"]:
        require(hashlib.sha256(source["snapshot"].encode()).hexdigest() == source["sha256"], "source hash")
    require(report["quality"]["incompleteReason"] is None, "incomplete report")
    require(report["producers"]["capabilities"]["setDetails"] == "unsupported", "missing is not zero")
    require(math.isclose(report["observedRunDps"], 108 / report["runningSeconds"], rel_tol=1e-7), "observed DPS")
    skills = next(json.loads(s["snapshot"]) for s in report["provenance"]["files"] if s["resource"].endswith("FixtureActiveSkills"))
    bolt = next(s for s in skills if s["id"] == "FIXTURE-SKILL-BOLT")["levels"][0]
    require(bolt["baseDamage"] == 4 and bolt["cooldownSeconds"] == 0.9, "estimate inputs")
    require(math.isclose(4 / 0.9, 4.444444444444445), "estimate DPS")
    require(17 * 2 == report["appliedDamageTaken"], "taken arithmetic")
    require(report["appliedDamageDealt"] == 124 - 16, "overkill arithmetic")
    require(report["producers"]["xp"]["groundBase"] == (8 + 2) * 1, "XP arithmetic")
    print("PASS real packet: 9 source hashes, quality, observed/estimate DPS, damage and XP arithmetic")

    # Arbitrary teaching values, no production ID, path or product target.
    base = {"id": "SYNTHETIC-TRAINING-ONLY", "damageHp": 10, "cooldownSeconds": 2}
    proposal = {"revision": "SYNTHETIC-r1", "baseline": fingerprint(base), "rows": {
        "A": {"field": "damageHp", "before": 10, "after": 12},
        "B": {"field": "cooldownSeconds", "before": 2, "after": 1}}}
    accepted = {"synthetic": True, "decision": "accept", "revision": proposal["revision"],
                "baseline": proposal["baseline"], "rows": ["A"]}
    cases = [("pending", base, proposal, None)]
    for decision in ["reject", "defer", "no-change", "insufficient-evidence"]:
        cases.append((decision, base, proposal, dict(accepted, decision=decision)))
    cases += [("revision changed", base, dict(proposal, revision="SYNTHETIC-r2"), accepted),
              ("baseline changed", dict(base, damageHp=11), proposal, accepted),
              ("unknown row", base, proposal, dict(accepted, rows=["C"]))]
    for label, current, candidate, approval in cases:
        old = copy.deepcopy(current)
        try:
            apply_example(current, candidate, approval)
        except ValueError:
            require(current == old, "rejected operation mutated input")
            print("PASS blocked:", label)
        else:
            raise AssertionError("should be blocked: " + label)
    applied = apply_example(base, proposal, accepted)
    require(applied == dict(base, damageHp=12), "partial selection must preserve B")
    require(base["damageHp"] == 10, "baseline mutated")
    require(applied["damageHp"] / applied["cooldownSeconds"] == 6, "synthetic after estimate")
    print("PASS synthetic partial accept: /damageHp 10 -> 12 HP, /cooldownSeconds unchanged at 2 s; estimate 5 -> 6 HP/s")
    # Guarded rollback: only the exact applied version is eligible.
    def rollback(current):
        require(fingerprint(current) == fingerprint(applied), "rollback baseline drift")
        return copy.deepcopy(base)
    require(rollback(applied) == base, "rollback failed")
    try:
        rollback(dict(applied, damageHp=13))
    except ValueError:
        print("PASS rollback refuses later edits")
    else:
        raise AssertionError("unsafe rollback")
    require({str(p): hashlib.sha256(p.read_bytes()).hexdigest() for p in game_files} == before_files, "game content changed")
    print("PASS synthetic rollback restores original; game JSON unchanged")
    print("Synthetic baseline fingerprint:", fingerprint(base))
    print("Synthetic applied fingerprint:", fingerprint(applied))
    print("NOT RUN: real gameplay follow-up, real tuning apply, Unity tests (documentation-only scope)")

if __name__ == "__main__":
    main()
