# FIELD-006 — одиночный импульсный портал

Contract: [DECISION-0153](../../decisions/0153-academy-single-burst-portal.md).
Current execution status belongs only to STATUS.md.

## Changes

User replaced the paired portal and dedicated lane with a single instant payload
on the shared general chains. Exact requested displacement: «давай это будет 5 единиц».
At warning completion an alive player inside the actual flattened area is passed
to the existing PlayerZoneTarget → PortalTransitRuntime path. No enemy teleport,
no linked exit zone and no lingering timed buff. Exit selection stays five units
from the player's activation position, inside margins and clear of obstacles.
No valid exit means no teleport; pause and large ticks keep one-shot semantics.

FIELD-006 authoring v6 and generated environment use one dormant pool placement
per chain for each of nine kinds (current tuning: six chains, 54 placements),
with no portal intervals/pair configuration.
Legacy paired gameplay data remain compatible.

New violet spiral was generated with the built-in image generator and accepted
by the user: «Да, подключить». `Art/Packets/field006-portal-burst-v1.json` PLAN then
APPLIED, seven files. Runtime glyph 256×256 RGBA, separate stationary ground layer,
vertical projection 0.8, violet rim tint. Runtime derivative visually inspected.
Old upright raster and meta removed from Assets, sprite registry/import profile
retired; historical master/provenance remain outside Assets.

## Verification

Zone/seal/content EditMode 138/138, zero failures/skips, Unity 6000.6.0f1;
`TestResults/checks/20261002T191407-833969Z/EditMode.xml`.
Runner could not write final PASS because another task changed inputs during checks.
Fresh final results follow below; no old PASS is used as new evidence.

Full graphics attempt on the revised code: EditMode 1438/1442, zero skips;
`TestResults/checks/20261002T191509-540462Z/EditMode.xml`. All portal/zone/seal tests passed.
Full runner stopped before PlayMode on the same four unrelated FIELD-009/Meta/UI
failures recorded in [the preceding art report](2026-10-02-academy-book-arrows-rim.md#checks):
`FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`
(Vector2 index 1 precision mismatch),
`MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`
(expected 11/11/6, actual 11/11/11),
`MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`
(5 expected strings vs 10 actual),
`MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`
(additional SET-023/024/026/030/034). No full PASS claimed.

Final relevant graphics run: 252/252 EditMode and 3/3 PlayMode, zero failures/skips;
`TestResults/checks/20261002T192918-823656Z/{EditMode,PlayMode}.xml`.
This includes all Zones and Presentation tests, production catalog/seal tests,
the actual Academy UI launch and real burst → PlayerZoneTarget → transit flow.
The latter verifies exact 5-unit arrival, hidden body, smooth camera flight,
pause and restored physics. Nine-effect `academy-seals-active.png` was inspected:
the portal uses the violet ground spiral/rim, book and arrows remain connected.
Concurrent input changes prevented a reusable PASS receipt; these are saved
test results, not a full-project PASS. A separate final `--scope art` attempt
was blocked by unavailable Editor REST (WinError 10061); its test classes are
included in the 252 passing EditMode tests above. Manifest audit: 334 records PASS.

The safe runner now recognizes only the installed Unity CLI auth broker's exact
`%LOCALAPPDATA%/Unity/bin/unity.exe --internal-auth-broker-serve` invocation,
matching executable and command path without extra arguments. This service
previously caused an unidentified-Editor false positive. Genuine/ambiguous
Editors, project locks and unavailable process data retain their safety checks.
`python -m unittest discover -s scripts/tests`: 32/32 PASS, including narrow
service exclusion and real-Editor visibility regressions.
