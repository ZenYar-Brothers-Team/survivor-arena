# Implementation Status

Этот файл — единственный source of truth для состояния исполнения Implementation Plan. Спецификации в `modules/` не содержат оперативных статусов.

Last repository audit: 2026-09-13  
Current next module: IP-03

## Foundation and playable core

### IP-00 — Контракт контента, стабильные ID и конфигурация

Status: Implemented  
Implementation evidence: commit `bb726e6`; `Assets/Game/Content/Model/ContentRegistry.cs` и связанные content contracts.  
Verification evidence: `Assets/Game/Content/Tests/ContentRegistryTests.cs` существует; в рамках миграции документации Unity Test Runner не запускался.  
Deviations: none recorded.  
Documentation impact: status synchronized with repository history.

### IP-01 — Run lifecycle, таймер, pause и завершение

Status: Implemented  
Implementation evidence: commit `bb726e6`; `Assets/Game/Run/Model/RunModel.cs` и `RunController.cs`.  
Verification evidence: `Assets/Game/Run/Tests/RunModelTests.cs` существует; в рамках миграции документации Unity Test Runner не запускался.  
Deviations: none recorded.  
Documentation impact: status synchronized with repository history.

### IP-02 — Перемещение игрока и базовая геометрия поля

Status: Implemented  
Implementation evidence: commit `9223059`; `Assets/Game/Movement/` и `Assets/Scenes/Gameplay.unity`.  
Verification evidence: movement/collision tests существуют; в рамках миграции документации Unity Test Runner не запускался.  
Deviations: none recorded.  
Documentation impact: status synchronized with repository history.

### IP-03 — Character stats, HP, damage, healing и regeneration

Status: Ready  
Implementation evidence: —  
Verification evidence: —  
Deviations: —  
Documentation impact: —

### IP-04 — Enemy core

Status: Blocked  
Blocked by: IP-03.  
Implementation evidence: —  
Verification evidence: —

### IP-05 — Active skill runtime и player damage pipeline

Status: Blocked  
Blocked by: IP-03, IP-04.  
Implementation evidence: —  
Verification evidence: —

### IP-06 — XP drops, pickup, expiry и level progression

Status: Blocked  
Blocked by: IP-04, IP-05.  
Implementation evidence: —  
Verification evidence: —

### IP-07 — Level-up draft, 6+6 slots и base build progression

Status: Blocked  
Blocked by: IP-06.  
Implementation evidence: —  
Verification evidence: —

## Build systems

### IP-08 — Active-skill progression и pattern framework

Status: Blocked  
Blocked by: IP-05, IP-07.

### IP-09 — Passive modifier framework

Status: Blocked  
Blocked by: IP-03, IP-06, IP-07.

### IP-10 — Reroll и banish

Status: Blocked  
Blocked by: IP-07; exact counts remain CG-04 TBD.

### IP-11 — Set framework

Status: Blocked  
Blocked by: IP-07, IP-08, IP-09.

### IP-12 — Character framework и weighted draft

Status: Blocked  
Blocked by: IP-07, IP-08, IP-09.

## Encounter systems

### IP-13 — Enemy movement и attack patterns

Status: Blocked  
Blocked by: IP-03, IP-04.

### IP-14 — Wave Director

Status: Blocked  
Blocked by: IP-04, IP-13; production schedules remain CG-02 gated.

### IP-15 — Boss/mid-boss framework

Status: Blocked  
Blocked by: IP-01, IP-13, IP-14.

### IP-16 — Field definitions и run configuration

Status: Blocked  
Blocked by: IP-02, IP-14, IP-15.

## Production content

### IP-17 — Production Active Skills

Status: Blocked  
Blocked by: IP-08 and CG-01 approval of target SKILL IDs.

### IP-18 — Production Passive Items

Status: Blocked  
Blocked by: IP-09 and CG-01 approval of target PASSIVE IDs.

### IP-19 — Production Sets

Status: Blocked  
Blocked by: IP-11, IP-17, IP-18 and CG-01 approval of target SET/component IDs.

### IP-20 — Production Enemies

Status: Blocked  
Blocked by: IP-04, IP-13 and CG-01 approval of target ENEMY IDs.

### IP-21 — Production Bosses и Mid-bosses

Status: Blocked  
Blocked by: IP-13, IP-15 and CG-01 approval of target BOSS/MIDBOSS IDs.

### IP-22 — Production Characters

Status: Blocked  
Blocked by: IP-12, IP-17 and CG-01 approval of target CHAR/starting SKILL IDs.

### IP-23 — Production Fields

Status: Blocked  
Blocked by: IP-16, required production content and CG-01 approval of target FIELD IDs.

### IP-24 — Canonical Wave / Encounter Content

Status: Blocked  
Blocked by: IP-14, IP-20, IP-21, IP-23 and CG-02 missing Approved Wave / Encounter Content.

## Meta, UI and integration

### IP-25 — Persistent profile и meta progression

Status: Blocked  
Blocked by: IP-12, IP-16; production economy remains CG-03 gated.

### IP-26 — Functional UI и полный player flow

Status: Blocked  
Blocked by: IP-07, IP-10, IP-16, IP-25.

### IP-27 — End-to-end integration

Status: Blocked  
Blocked by: all in-scope preceding system modules; content-complete verification also requires Approved production modules.

## Status maintenance rule

После изменения статуса нужно пересчитать готовность прямых dependants. Завершение модуля переводит его в `Implemented`; только успешное выполнение заявленных checks переводит его в `Verified`. Для каждого реализованного модуля обязательны implementation evidence, verification evidence, deviations и documentation impact.
