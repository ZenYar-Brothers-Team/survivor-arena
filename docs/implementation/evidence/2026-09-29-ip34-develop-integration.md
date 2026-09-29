# IP-34 worktree integration of develop-evg, 2026-09-29

User requested all current `develop-evg` changes before human-demonstration work.
Fetched `origin/develop-evg`; local `develop-evg` at `69ea7d1` contains the remote
and eleven additional local commits. Merged that local branch into
`ip34-automation` at `a0fbeff`, without modifying the primary checkout.

Integration includes the folio/UI changes, earned-level reward correction,
anti-blob enemy movement, opposite-centroid spawns and bounded camera follow.
Both sides of the regression-map conflict were retained. The automation
presentation ADR was renumbered from 0098 to 0102, with links updated, because
the incoming earned-level reward ADR already owns 0098. No decision semantics
or gameplay balance were changed by conflict resolution.

The three pre-existing local Unity settings were temporarily stashed at
`22af751cd06d262650f3288c3d6929173634b026` and reapplied. SHA-256 hashes of all
three match before/after; these settings are not included in the merge commit.

Initial full smoke found two obsolete automation-test expectations: a run at
starting L1 no longer earns five gold. Assertions were updated to the incoming
DECISION-0098 while preserving receipt/purchase checks. Three Markdown hard
breaks in that ADR were normalized to satisfy the repository whitespace check.

Final verification: Unity 6000.6.0f1, full graphics, 1034/1034 EditMode and
52/52 PlayMode, zero failed/skipped; content generation, audio integrity and
art manifest audit (269 records) passed. Evidence:
`TestResults/checks/20260929T192325-526744Z/summary.json`.
This is integration/regression verification, not new human visual acceptance.

Earlier bot pilots remain measurements of their original gameplay revision.
They must not be pooled with future recordings/runs on the anti-blob build.
Documentation impact: execution STATUS, regression map and ADR links aligned;
incoming canonical design changes retained unchanged.
