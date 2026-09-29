# IP-34 AB-12 — adaptive herd bot and decision trace, 2026-09-29

`herdLoopAdaptive/v1` is a separately selectable research profile. It favors
visible XP with an open enemy corridor, tries the existing lure/sweep when all
XP is blocked, and abandons a bank that remains blocked after a bounded sweep.
The old `herdLoop/v1` policy remains selectable. Both profiles report a bounded
`movementTrace` with simulation time, position, goal, direction, mode and last
transition reason, HP fraction, crowd count, blockers to current goal and
remembered bank, visible pickups and movement score. The trace samples about
once per simulation second and on mode changes, retains 1024 entries, and
reports the number evicted. It is not deterministic replay.

Verification in isolated `ip34-automation` worktree:

- Unity 6000.6.0f1 focused EditMode 34/34 and final full graphics smoke:
  `TestResults/checks/20260929T150517-469275Z/summary.json`, EditMode
  999/999, PlayMode 52/52, 0 failed/skipped. Tests cover open XP preference, blocked
  sweep abandonment, open-route return, bank-route diagnostics and config ID.
- Python balance runner tests: 18/18. Example JSON parsed successfully.
- Initial silent three-chain production pilot, before adding the separate
  `bankRouteBlockers` trace field but with identical movement logic:
  `TestResults/herd-adaptive-pilot/fresh-herd-loop-adaptive-example/manifest.json`.
  All three natural Defeat: 99.2 s / 5 XP / 7 expired;
  142.8 s / 7 XP / 17 expired; 58.9 s / 8 XP / no expiration event reported.
  All terminal level 1. Trace samples: 105, 155, 60; none evicted.
  Samples with zero movement direction: 13, 23, 17 respectively. These are
  observations, not a proven cause of death; danger, blocked waypoints and
  pauses need separate investigation. The runner used random gameplay seeds.
- Final silent three-chain production pilot:
  `TestResults/herd-adaptive-final/fresh-herd-loop-adaptive-example/manifest.json`,
  built player `TestResults/balance-build-herd-adaptive-final/build-manifest.json`.
  All three natural Defeat: 79.5 s / level 1 / 7 XP / 2 expired;
  89.9 s / level 2 / 15 XP / no expiration event reported;
  812.6 s / level 3 / 31 XP / 60 expired. All have the `bankRouteBlockers`
  field, 82/92/839 trace samples, none evicted, no botStuck or incomplete
  observation. Samples with zero movement direction: 7/82, 10/92, 285/839.
  A stationary sample does not by itself identify whether danger, walls or
  goal scoring caused the pause; the last run's 60 expired XP shows that
  survival did not guarantee collection.

The adaptive policy is inspectable and avoids one known failure (repeatedly
returning through a still-blocked corridor), but these runs do not establish
superior XP collection or survival. No production balance, visual window or
audio output was changed.
