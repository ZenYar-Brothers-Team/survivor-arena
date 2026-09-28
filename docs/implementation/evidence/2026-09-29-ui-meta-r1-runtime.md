# Personal Meta R1 — runtime evidence, 2026-09-29

Execution status and remaining approvals live only in [STATUS](../STATUS.md).
Scope: approved personal-upgrade screen and economy, DECISION-0091; no new
gameplay content, Settings redesign or final approval of the full Unlocks tab.

## Implementation

- Production catalog: META-003…014, 12 personal types. Ten levels for stats,
  six for rerolls/banishes; price = base × next level independently per hero/type.
- Profile owns stat modifiers, startup draft counters, actual spending ledger,
  and an atomic selected-hero refund with fixed 1000-gold fee. Wallet + spending
  must cover the fee; zero final balance is allowed. Save failure leaves prior
  state, duplicate/stale/run-active requests cannot refund twice.
- The disabling toggle zeroes all bonuses including startup draft counters;
  purchases and refunds remain available. Unlocks/other heroes are preserved.
- MetaPresenter projects immutable data; MetaShopPanel renders the approved
  compact text-only cards, separate portrait roster and list scroll regions,
  and refund confirmation. No dropdown or borrowed set icons on upgrade cards.
- Production uses `profile-meta-r1.json`. The user cancelled migration because
  the game is pre-release. Old test saves are neither deleted nor rewritten;
  old global upgrades remain only in the isolated fixture catalog.
- Existing Unlocks purchases remain accessible in a separate tab; that tab is
  not claimed as a separately approved finished composition.

## Checks observed

Unity 6000.6.0f1, Windows, safe batch runner after fresh process/lock preflight.

- Prerequisite: 174/174 EditMode, 0 skipped, scoped Meta/UI/Character:
  `TestResults/checks/20260928T215419-502334Z/summary.json`.
- Initial implementation: 182/182 scoped EditMode, 0 skipped:
  `TestResults/checks/20260928T220402-082771Z/summary.json`.
- Full graphics run: 944/944 EditMode + 37/37 PlayMode, 0 failed/skipped,
  no third-party tests. Generated content up to date; audio 28 files/15 cues
  valid; art manifest 256 owner/role records valid.
  `TestResults/checks/20260928T221100-471925Z/summary.json`.
- The first targeted PlayMode attempt reached the assertions but failed at
  `LogAssert.NoUnexpectedReceived` on an incidental performance warning.
  Removed that overbroad assertion: this functional UI test does not establish
  frame-time acceptance; Unity's ordinary error/exception checks remain active.
- After the full run, visual inspection found the unchecked toggle box collapsed.
  Its input now has an explicit minimum size; the targeted test additionally
  checks its geometry and persists both toggle states. Follow-up graphics
  PlayMode PASS 1/1, 0 failed/skipped:
  `TestResults/checks/20260928T221337-242532Z/summary.json`.
  Final 720p capture inspected: checkbox visible, three complete rows retained.

Critical paths in the full suite: lifecycle, damage/death, XP/draft, active skills,
waves/pools, composition, UI, content loading and GameplaySmokeTests.
PersonalMetaTests covers ownership/channels/prices/caps and refund boundary,
failure, stale/double intent and reload. MetaShopTests covers confirmation/cancel.
MetaShopSmokeTests uses an isolated production profile seeded with 10000 gold and
all heroes to exercise roster overflow, purchases, selection, confirmation/cancel,
refund and exact startup counters (4 rerolls/3 banishes after one purchase each).

## Visual evidence

Real Unity panel captures, not browser previews:

- `TestResults/meta-personal-1920x1080.png`: all 12 cards visible in three columns.
- `TestResults/meta-personal-1280x720.png`: three full rows, two columns, remainder
  accessible by scrolling. Roster scroll is independent, footer remains visible.

Screenshots were inspected by the agent. Seeded gold/unlocks are test-only;
no real user profile was altered. Programmatic UI submit and toggle events are
not a claim of manual mouse/controller acceptance. User visual acceptance and
performance acceptance cannot be inferred from these automated checks.
