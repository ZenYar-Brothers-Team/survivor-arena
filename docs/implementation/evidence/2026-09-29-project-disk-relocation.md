# Disk relocation — 2026-09-29

User explicitly requested moving the entire automation checkout to D:, then also
the primary project and saved game data where safe. The primary project was
already physically `D:\GitHub\survivor-arena`; it was not moved. Unity is also
installed on D:. User closed the main Editor after saving; process inventory
confirmed no running Unity/game before moving saved data. No Editor was killed.

## Moved data and verification

| Data | Physical destination | Verification |
| --- | --- | --- |
| Automation worktree, including Library, all builds/results and local changes | `D:\GitHub\survivor-arena-automation` | 49,984 files, 6,606,834,844 bytes; every SHA-256 and length matched |
| Game persistent data: attempted move, then safe rollback below | `D:\GameData\survivor-arena` now retains a backup, not active saves | Initial move: 15,989 files, 1,135,823,331 bytes; every SHA-256 and length matched |
| Additional profile/settings JSON and backup copies | `D:\GameData\survivor-arena-profile-backup-20260929` | Independent copies checked against their source hashes before moving |

Cross-volume moves used exact validated source/destination paths, refused
existing destinations, checked free space and active processes, and detected no
nested reparse points. No data was discarded. The game-data destination retains
the original effective user/System/Administrators permissions, protected from
broader inherited permissions on D:.

The automation-worktree compatibility junction remains at:

- `C:\Users\zheni\.codex\worktrees\ip34-automation\survivor-arena`
  → `D:\GitHub\survivor-arena-automation`.

This is a link, not a second copy. Existing Git/Codex worktree identity is
preserved. Git HEAD stayed
`4679609525f56f520533e3eef66b1d5bd0209b7f`; status before/after contains only the same
two unrelated URP settings diffs. `git rev-parse HEAD` worked through both
worktree paths; `--show-toplevel` at the physical destination returns D:.

Free space immediately after verification: C: **9.21 GiB**, D: **39.62 GiB**.
Receipts with inventories and hashes are local artifacts, not tracked payloads:

- `D:\GitHub\survivor-arena\TestResults\ip34-relocation-20260929.json`
  (verified at `2026-09-29T20:22:10Z`).
- `D:\GameData\survivor-arena-relocation-20260929.json`
  (verified at `2026-09-29T20:11:59Z`).

The old **worktree** path must remain a junction while its consumers use it. Do
not recreate an independent checkout there or recursively clean its contents:
that would affect the live D: checkout. Windows/Unity global preferences outside
this game's data directory were not migrated.

## Runtime compatibility and saved-data rollback

Full graphics check `20260929T202234-804300Z` ran from the physical D: checkout:
1043/1043 EditMode passed; 55/56 PlayMode passed. The single failure was
`PlaytestSmokeTests.Gameplay_LethalHitExportsLinkedPacket_AndPlaytestUiStaysCollapsed`:
the standard playtest exporter could not create a report directory below the
LocalLow junction. This is a relocation regression, not a passing smoke.

An isolated `System.IO` probe with Unity's bundled Mono reproduced the problem:
`Directory.CreateDirectory` returned, but the directory did not exist; writing
then threw `DirectoryNotFoundException`. A .NET/PowerShell probe reproduced it
too. Direct D: access and the C: automation-worktree junction passed. Both
PowerShell-created and Windows-native junctions under this LocalLow parent
failed. The exact Windows-level cause was not established. No game-code or
security-policy workaround was introduced.

Under the user's condition "without negative consequences", active saved data
was restored to the physical standard directory:

`C:\Users\zheni\AppData\LocalLow\DefaultCompany\survivor-arena`

The user closed the Editor again. A concurrent main-project batch test changed
`TestResults.xml` during the first staging copy; hash verification stopped before
switching the active path. After that test exited, the staging copy was refreshed
and **15,992 files / 1,135,469,256 bytes** were checked against current D: data by
SHA-256. A final no-game/Editor and source-stability check preceded replacement
of the junction with the verified physical directory. No source data was deleted;
the full D: copy remains a static backup and includes the latest user saves
available at restoration time. It is not a second active save location or an
automatically synchronized backup.

Restore receipt: `D:\GameData\survivor-arena-restore-20260929.json`,
`verified-restored`, completed `2026-09-29T20:46:12Z`. Independent JSON backups
from before the first move also remain at the path in the table above.
Post-restore Mono probes successfully created/read/deleted their own temporary
files in both the active root and `Playtests`; diagnostic junctions were removed
nonrecursively, without touching target data. Free space: C: **7.50 GiB**,
D: **39.62 GiB** before the next build.

Subsequent Unity verification/build/pilot are recorded in
[AB-14 evidence](2026-09-29-ip34-demonstration-recording.md).
