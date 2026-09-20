---
name: balance-check
description: "Read-only balance analysis of JSON content under Assets/Resources/Content (combat, enemies, progression/XP, passives, waves) against Content_design.md intent: outliers, dominant options, dead zones, broken curves. Use after changing any tuning value."
argument-hint: "[combat | enemies | progression | waves | path-to-json]"
user-invocable: true
allowed-tools: Read, Glob, Grep, AskUserQuestion
---

<!-- Adapted from Donchitos/Claude-Code-Game-Studios (MIT, commit 984023d) skill `balance-check`.
     See .claude/skills/THIRD_PARTY_NOTICES.md. -->

Analyse numbers; **do not change any value** and do not write files unless the user asks for a saved report (then write to `docs/balance/balance-<domain>-<date>.md`).

## 1. Domain and data
Map the argument to data files (ask if empty):
- combat → `Content/ActiveSkills/*.json`, `Content/Sets/*.json`, `Content/Characters/*.json`
- enemies → `Content/Enemies/*.json`
- progression → `Content/Passives/*.json` and the XP/level thresholds in the character/progression config
- waves → `Content/Waves/*.json` (phase durations, intervals, caps, per-phase modifiers)

(all under `Assets/Resources/`). Note every file read — list them in the report. Files named `Fixture*` are **non-production placeholders**: report findings but label them as fixture data, and never present them as final balance.

## 2. Design baseline
Read only the relevant parts of `docs/Content_design.md` (full card of each ID under review) and `docs/Game_design.md` (run length, slot limits). Content marked Draft is not a target. If the design gives no number for a value, say "no design target" instead of inventing one.

## 3. Checks
- **Combat**: DPS per skill per level (damage x count / cooldown), time-to-kill vs enemy HP at the level a player typically has, strictly dominant skills/passives, upgrade levels that are flat or regress, stacking that can break scaling.
- **Enemies**: HP/damage/speed relative to a baseline enemy; contact damage vs player HP; projectile enemies with unavoidable damage; elites/bosses vs their multipliers.
- **Progression**: XP curve monotonic, level intervals (time to next level at expected kill rate), dead zones, power spikes, pickup radius vs magnet ranges.
- **Waves**: spawn rate = `1/spawnIntervalSeconds` capped by `maxAliveEnemies`; total spawn budget per phase; rest phases actually lower pressure; per-phase modifiers vs enemy base stats (effective HP/speed/damage); phases tile the run duration with no gap; hooks fall inside the run.

Show the arithmetic for every conclusion (formula, inputs, result). Mark assumptions (kill rate, player DPS) explicitly.

## Output
```
## Balance check: <domain>
Data sources: <files>
Health: HEALTHY | CONCERNS | CRITICAL
### Findings   (id/file:line → number → why it matters → suggested direction, not a value)
### Outliers / dominant options
### Assumptions
### Recommended follow-ups
```
