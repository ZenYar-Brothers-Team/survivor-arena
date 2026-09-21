# IP-09 evidence — 2026-09-21

<a id="ip-09"></a>
## IP-09

Scope revision: design-sync-R2. Текущий execution status хранится только в [STATUS](../STATUS.md#ip-09--passive-modifiers-и-новые-stat-effects).

### Реализация и compatibility

- Существующие CharacterStats/DTO/catalog/preview уже содержат все новые каналы; повторной реализации stat composition не потребовалось. [Mapping table](../modules/IP-09-passive-framework.md#stat-applicability-и-владельцы-defaults) связывает все PASSIVE-001…014 с consumer/fixture.
- `Assets/Resources/Content/Passives/FixturePassives.json`: к VITALITY/HASTE/MEMORY добавлены COLLECTOR, PICKUP-RADIUS, KNOCKBACK, SIZE, RANGE, LOW-HEALTH, каждый с шестью final-value уровнями. Production IDs/icons не регистрируются. Старый MEMORY lifetime сохранён как generic fixture capability; PASSIVE-007 использует pickup radius и не меняет lifetime/recovery.
- `PlayerPassiveSetRuntime`: repeated Initialize сначала снимает предыдущие эффекты/subscription; Shutdown очищает catalog, позволяя повторно запустить runtime. Failed initialization по-прежнему откатывает applied keys. Игрок не получает возможности освобождать build slots.
- `BuildSlotViewState.Detail`, presenter и tooltip существующих semantic slot IDs: final passive values и текущий low-HP multiplier доступны вне DEV. `GameplayUiRuntimeModel` подписывается на stat changes с симметричным Dispose; HP changes обновляют динамический snapshot. Current/next draft previews уже предоставлены feature definition.

### Проверки acceptance и regressions

| Область | Наблюдаемое покрытие |
|---|---|
| JSON/catalog L1…L6, все каналы | PassiveFrameworkTests: загруженные definitions применяются повторно одним ключом; проверяются final values и neutral removal; новые previews current/next; FixtureRuntimeContentCatalogTests: общий registry и references |
| HP ratio, stacking, cooldown/caps | PassiveFrameworkTests, CharacterStatsTests, CharacterHealthIntegrationTests, CharacterStatChannelsTests |
| Relative potion multiplier / PASSIVE-007 | CollectorAndMagnet: 5% × 1.6 = 8%, radius 2 × 1.6 = 3.2, lifetime/recovery остаются 0 |
| Low HP / activation snapshot | CharacterStatChannelsTests: 100/55/10/5% HP, healing и max HP; CombatAttackPipelineTests: delayed damage сохраняет activation snapshot |
| Existing drops, recovery без double award | ExperienceAccountingTests, ExperienceDropTests |
| Size/range applicability, speed/timing independence | SkillEffectMappingTests, SkillTargetingTests, ActiveSkillTimingTests |
| Repeated Initialize / Shutdown / rollback | PlayerPassiveSetRuntimeRollbackTests, включая новый regression test stale catalog / no stacking |
| UI без DEV и immutable snapshot | GameplayUiPresenterTests.PassiveDetails_InReleaseModeRefreshLowHealthWithoutMutatingOldSnapshot |
| Реальный UI / gameplay | GameplaySmokeTests: шесть passive slots, tooltip, current low-HP detail меняется от урона и возвращается после лечения; общий run/draft/XP/composition smoke |

### Условия выполнения

Unity 6000.6.0f1 (f7f8ed4d1e24), Windows, `scripts/Test-Unity.ps1`, filter `^Game\.`. Перед каждым запуском проверены процессы Unity: интерактивный Editor отсутствовал. Первый sandbox attempt не дошёл до tests из-за Package Manager IPC; повтор вне sandbox выполнил suite. UnitySkills REST не использовался.

- EditMode, 2026-09-21 07:06:03–07:06:04 UTC: **369/369 passed, failed 0, skipped 0**.
- PlayMode, 2026-09-21 07:07:20–07:07:22 UTC: **2/2 passed, failed 0, skipped 0**.
- Third-party tests: 0 в обоих прогонах.
- Evidence files: `TestResults/IP09-2026-09-21-EditMode.xml`, `.log`, `TestResults/IP09-2026-09-21-PlayMode.xml`, `.log` (локальные ignored artifacts).
- EditMode проверил финальный production/config код; последующее изменение затронуло только PlayMode smoke, проверенный отдельным PlayMode run. Дальнейшие правки — документационные.
- Critical paths присутствуют в passed suites: run lifecycle, damage/death, XP/draft, active skills, wave director, enemy spawn/pool, composition, UI presenter, content loading, gameplay smoke.

Нового raster/UI layout нет; ручная оценка новых production icons не заявляется. Actual potion roll/cap — IP-28/G-10. Отклонений от принятого product design нет; архитектурной переработки нет. GDD/CD не изменены. Существовавшая до работы правка Packages/manifest.json сохранена.
