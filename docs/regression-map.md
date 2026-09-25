# Regression map

Индекс **уже существующих** тестов, которые охраняют критические пути. Не заменяет evidence в `docs/implementation/STATUS.md`. Поддерживается скилом `/regression-map` (`update` / `check` / `bug`).
Составлено 2026-09-20 по `Assets/Game/**/Tests` и `PlayModeTests`; наличие классов проверено поиском, результаты прогона тут не фиксируются (см. `/smoke-check`).

| Критический путь | Охраняющие тесты (класс) | Вид | Статус |
|------------------|--------------------------|-----|--------|
| Жизненный цикл забега, пауза, конец | `Game.Run.Tests` (`RunModelTests`) | EditMode | OK |
| Движение игрока, границы | `Game.Movement.Tests` (`MovementVelocityCalculatorTests`, `GameplaySceneIntegrationTests`, `PlayerObstacleCollisionTests`) | EditMode | OK |
| Урон → смерть → конец забега | `CharacterRunBindingTests`, `CharacterHealthIntegrationTests`, `Game.Combat.Tests` (`HealthTests`) | EditMode | OK |
| Откат/повтор инициализации `PlayerCharacterRuntime` | `PlayerCharacterRuntimeLifecycleTests` | EditMode | OK (добавлено в ревью) |
| XP: дроп → подбор → уровень → пауза | `ExperienceProgressionTests`, `ExperienceDropTests`, `ExperienceIntegrationTests`, `RunSetupConfigTests` | EditMode | OK |
| Драфт: офферы, reroll, banish, очередь, сеты | `LevelUpDraftRuntimeTests`, `PlayerBuildAndDraftTests`, `SetFrameworkTests`, `PassiveFrameworkTests` | EditMode | OK |
| Откат `PlayerActiveSkillSetRuntime`/`PlayerPassiveSetRuntime` при сбое Initialize; passive повторный Initialize и Shutdown→Initialize без stale catalog/stacking | `PlayerActiveSkillSetRuntimeRollbackTests`, `PlayerPassiveSetRuntimeRollbackTests` | EditMode | OK (добавлено в ревью) |
| Активные навыки: срабатывание, кулдаун, эффекты, мины | `PlayerActiveSkillSetRuntimeTests`, `ActiveSkillTimingTests`, `ProjectileAndAreaTests`, `SceneActiveSkillEffectExecutorTests`, `ActiveSkillProgressionFrameworkTests` | EditMode | OK |
| Wave Director: фазы, хуки, лимиты, состав | `WaveDefinitionTests`, `WaveDirectorTests` | EditMode | OK |
| Спавн/деспавн врагов, пул, `EnemyRegistry` | `WaveSpawnerTests`, `EnemyRuntimeLifecycleTests`, `EnemySpawnerSceneIntegrationTests` | EditMode | OK |
| Паттерны движения/атаки врагов | `EnemyPatternTests`, `EnemyMovementAndContactTests` | EditMode | OK |
| Пул `GameObjectPool<T>` | `GameObjectPoolTests` | EditMode | OK (добавлено в ревью) |
| Общий валидатор чисел | `NumericValidationTests` | EditMode | OK (добавлено в ревью) |
| Загрузка контента: реестр, ссылки, каталоги | `ContentRegistryTests`, `FixtureRuntimeContentCatalogTests`, `FixtureCharacterCatalogTests` | EditMode | OK |
| Презентация спрайтов (композитор позы) | `Game.Presentation.Tests` (`ProceduralSpriteAnimatorTests` и др.) | EditMode | OK |
| Category imports, role/crop validation, generic presentation pause/fade/pool/disable и shake preference | `CategoryImportTests`, `GenericPresentationTests`, `ScreenShakeRequestGateTests`, `PresentationAdapterSmokeTests` | EditMode / PlayMode | OK; manual art/dense gameplay review отдельно |
| Composition root: сборка, откат при сбое | `GameplayCompositionSceneTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: откат при частичном сбое покрыт только косвенно (runtime-тесты выше), прямого теста `GameplayCompositionRoot.Initialize` с искусственным сбоем нет |
| UI: presenter ↔ ViewState, HUD-волна, dev-панель | `GameplayUiPresenterTests`, `GameplayUiAssetTests`, `GameplaySmokeTests` | EditMode / PlayMode | GAP: `GameplayUiRuntimeModel` и `UiToolkitGameplayView` без прямых тестов (TD-040) |
| JSON-загрузка (`JsonContentFile`: нет файла, битый JSON, неизвестное поле) | — | — | GAP (TD-040) |
| Цель для навыков (`SceneEnemyTargetProvider`) и запуск снарядов (`SceneProjectileLauncher`) | — | — | GAP (TD-040) |

## Багфиксы без регресс-теста

IP-31: `RunTelemetryRecorderTests.Snapshot_ContentIdDictionaryKeys_RetainOrdinalCase` защищает стабильные content IDs от camel-case преобразования ключей JSON. `PlaytestSmokeTests.Gameplay_LethalHitExportsLinkedPacket_AndPlaytestUiStaysCollapsed` проверяет scene reload, export и идемпотентный teardown; до ordered composition Shutdown reload давал NullReferenceException в UI/passive consumers после очистки Health/Stats. Дополнительно `PlaytestSessionTests.Shutdown_DuringLiveExport_PublishesFinalSnapshotAfterEarlierPacket` защищает final packet от перезаписи более ранним live export.
Нет открытых. Исправления ревью 2026-09-20 (A-3, A-4, A-6, T-1) сопровождаются регресс-тестами, перечисленными выше.

## IP-11 — regression guard

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Producer-first shutdown / scene reload with acquired sets | `SetFrameworkSmokeTests.SimultaneousSets_QueuedChoicesPauseProjectionAndShutdown` | PlayMode | См. IP-11 в [STATUS](implementation/STATUS.md) | Явный `player.Shutdown()` до root проверяет pre-teardown notification; без него UI/passive consumers обращаются к очищенным Health/Stats. Дополняет прежний случайный scene-reload guard IP-31. |

## IP-12 — regression guard

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Character selection → loadout → Shutdown → selection | `CharacterSelectionSmokeTests.Selection_LockedCannotStart_AlternateLoadoutAndReinitAreClean` | PlayMode | См. IP-12 в [STATUS](implementation/STATUS.md) | Без независимой panel/root UI Toolkit отвергает повторное открытие selection после HUD; проверяются новый run ID, сброс stats/modifiers/skill и actual character ID в telemetry. |

## IP-13 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Pool return inside impact callback | `EnemyPatternIntegrationTests.ImpactCallback_CanRentSameProjectileWithoutOldHitDespawningNewLife` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Snapshot и return перед callback защищают новую аренду от старого попадания. |
| Old run callback after projectile reinit | `EnemyPatternIntegrationTests.OldRunTerminalCallback_DoesNotDespawnReinitializedProjectile` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Старый multicast StateChanged не возвращает новую running life. |
| Pause phase / projectile cleanup | `EnemyPatternIntegrationTests.Pause_PreservesObservableMovementPhase`, `Projectile_PauseFreezesAndTerminalReturnsImmediatelyWithoutPhysicsTick`, `PoolReuse_ResetsSourceLifetimeVelocityRendererTrailAndOldRunSubscription` | EditMode | См. IP-13 в [STATUS](implementation/STATUS.md) | Сохранение phase, reset source/trail и немедленный terminal cleanup. |

## IP-14 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Actual spawn count при отсутствии target | `WaveSpawnerTests.Tick_MissingTarget_ReportsZeroActualWithoutRetryingBurst` | EditMode | См. IP-14 в [STATUS](implementation/STATUS.md) | Tick возвращает число созданных объектов; неисполненная группа отмечается unavailable и не повторяется. |
| Continuous timer при перескоке между фазами | `WaveBurstTests.Continuous_SkippedBoundary_ChargesOnlyTimeInCurrentPhaseAndDiscardsCapSuppression` | EditMode | См. IP-14 в [STATUS](implementation/STATUS.md) | Время старой фазы не начисляется новой; suppressed заявки не накапливаются для последующего спавна. |

## IP-15 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Boss HUD между периодическими refresh | `BossEncounterSmokeTests.Gameplay_BossBarTelegraphPauseAndTerminalCleanup` | PlayMode | См. IP-15 в [STATUS](implementation/STATUS.md) | Spawn/HP/despawn идут через producer Changed; bar/name появляются без ожидания HUD timer. |
| Burst длиннее своего cooldown | `BossCombatTests.Sequence_BurstLongerThanCooldown_CompletesTailThenMovesToNextAttack` | EditMode | См. IP-15 в [STATUS](implementation/STATUS.md) | Single-cycle controller завершает хвост, не начинает лишний burst перед переключением. |
| Spiral sequence wrap | `BossCombatTests.Sequence_SpiralRepeatedCycle_PreservesPatternRotation` | EditMode | См. IP-15 в [STATUS](implementation/STATUS.md) | Полный новый wind-up сохраняет накопленную rotation конкретного sequence slot. |
| Physics displacement босса на паузе | `BossEncounterTests.Pause_SuspendsBossPhysicsAndResumeRestoresItIncludingPoolReuse`, `BossEncounterSmokeTests.Gameplay_BossBarTelegraphPauseAndTerminalCleanup` | EditMode + PlayMode | См. IP-15 в [STATUS](implementation/STATUS.md) | Owner приостанавливает physics simulation, а не только velocity; resume/reuse восстанавливают body. |

## IP-29 — regression guards

| Path | Guarding test | Kind | Last verified | Notes |
|---|---|---|---|---|
| Deadline before damage/contact | `TravelerRuntimeTests.PeacefulContact_DoesNotDispatchPlayerCombat_ExpiredAttackerCannotHit`, `Spawn_TwoScreenHeights_KillDropsOneBook_TimeoutNone` | EditMode | См. IP-29 в [STATUS](implementation/STATUS.md) | Guard не позволяет порядку FixedUpdate/Update продлить встречу или выдать Book после deadline. |
| Support lifetime independent of source Update | `EnemyProtectionTests.Aura_DeadlineExpiresBeforeDamage_WithoutWaitingForSourceUpdate` | EditMode | См. IP-29 в [STATUS](implementation/STATUS.md) | Deadline источника проверяется до damage/control, без лишнего кадра aura. |
| Traveler body art survives HP/damage scaling (ghost Traveler) | `ProductionTravelerCatalogTests.Schedule_ScalesOnlyHpAndDamage_ByFieldRankAndTime` | EditMode | 2026-09-25, [DECISION-0059](decisions/0059-playtest-2026-09-25-evening-fixes.md) | Без `visual`/`motionProfile` в scaled definition `Initialize` падал посередине и оставлял неуязвимый объект без HUD. |
| Boss teleport-slam timing/pause/area | `BossTeleportControllerTests`, `ProductionBossCatalogTests.Catalog_DefinesFinalAndMidBoss_WithApprovedBodies` | EditMode | 2026-09-25, [DECISION-0059](decisions/0059-playtest-2026-09-25-evening-fixes.md) | Таймер 5 s сбрасывается при приближении, пауза не двигает таймер/telegraph, удар только в радиусе. |
| Traveler telemetry position export | `TravelerTelemetryTests.Export_IncludesScheduleLifeAndPlainPosition_AndDisposeUnsubscribes` | EditMode | См. IP-29 в [STATUS](implementation/STATUS.md) | Plain x/y предотвращают recursive Unity Vector2 serialization; dispose снимает events. |
| Traveler reachable movement with many field obstacles | `BoxPickupPlacementTests.PlaceFrom_ClearStep_ReturnsRequestedPoint`, `PlaceFrom_BlockedOrDisconnectedStep_MatchesFullProjection`, `PlaceFrom_SixtyFourObstacles_StaysWithinMovementBudget` | EditMode | 2026-09-24, 714/714 EditMode | Короткий свободный шаг обходится без полной сетки; перекрытый шаг сохраняет прежнюю проекцию. |
## IP-25 — regression guards

| Риск | Тесты | Вид | Evidence |
|---|---|---|---|
| Stable keys изменены JSON naming strategy / повторные покупки | `MetaProfileTests.Purchase_InsufficientLockedDuplicateAndCap_DoNotOverspend`, `Modifiers_GlobalAndPersonal_AddWithoutMutatingPreviousRun` | EditMode | IP-25 в STATUS |
| Повтор reward/Book после load/Retry и сбоя записи | `MetaProfileTests.Apply_ResultDuplicateAndReload_PayExactlyOnce`, `SaveFailure_PendingResultBlocksProgressAndRetriesWithoutDuplicate` | EditMode | IP-25 в STATUS |
| View уничтожен до владельца при смене сцены | `MetaProgressionSmokeTests.ProfileAndSelectionViewDestroyedFirst_ShutdownIsSafe`, `FieldViewDestroyedFirst_ShutdownIsSafe` | PlayMode | IP-25 в STATUS |
| Купленный бонус не применяется / применяется дважды | `MetaProgressionSmokeTests.Result_Purchase_Retry_AppliesUpgradeAndKeepsOneReward` | PlayMode | IP-25 в STATUS |

## Enemy body art integration (IP-12A/IP-20)

Shared enemy death presentation: `EnemyDeathPresentationSmokeTests.Death_StopsInPlace_PausesPresentation_AndDespawnsAfterSharedEffect` guards immediate gameplay removal, no death push, pause freeze and delayed despawn; `EnemyBodySmokeTests.GameplaySpawner_UsesApprovedVillager_AndResetsPooledBody` guards copying the animated sprite before baseline reset and restoring renderer visibility after pool reuse. [Death visibility OBS-01/02](playtests/2026-09-22_enemy-death-visibility.md).

Contact follow-up: `SpriteContactProfileTests` guards the filled outer silhouette, tangency without unused radial margin and profile validation; `BodyContactSmokeTests` checks eight contact directions; `EnemyBodyPresentationTests` checks radius after mixed pool reuse. [DECISION-0039 / evidence](implementation/evidence/2026-09-22-body-contact-circles.md), [OBS-01](playtests/2026-09-22_contact-gap.md).

EnemyBodyPresentationTests защищает child-only motion, hit/pause, mixed-pool reuse
и typed motion reference/scaling; EnemySpriteImportTests — import/reimport GUID,
size/pivot/transparent border; EnemyBodySmokeTests — реальную цепочку
composition → spawner → pooled enemy в Gameplay. Evidence: [enemy art](implementation/evidence/2026-09-21-enemy001-art.md).

## IP-26 — regression guards

| Риск | Тесты | Вид | Evidence |
|---|---|---|---|
| Первый Main Menu завершает ещё не начатый run | `CharacterSelectionSmokeTests.Selection_LockedCannotStart_AlternateLoadoutAndReinitAreClean`, `FieldSelectionSmokeTests.Selection_BackLockedAlternateFieldAndReinitialization_UseFreshConfiguration` | PlayMode | IP-26 в STATUS |
| Settings снимает чужую паузу или пропускает input в нижний экран | `AppShellPresenterTests.Settings_Back_ReturnsToOwningScreenAndBlocksBackgroundActions`, `AppShellSmokeTests.Menu_Settings_RunPause_Settings_Quit_Results_Retry` | EditMode/PlayMode | IP-26 в STATUS |
| Последняя revision теряется / invalid original перезаписывается | `SettingsServiceTests.Save_OverlappingWrite_PersistsNewestRevisionAndRetriesFailure`, `Load_InvalidDocument_PreservesBeforeReplacing` | EditMode | IP-26 в STATUS |
| Неподтверждённое видео сохраняется при выходе/таймауте | `SettingsServiceTests.Close_DuringVideoApply_WaitsThenRevertsAndSavesAudio`, `Preview_Timeout_RevertsWithoutPersistingCandidate`, `Load_UnsupportedSavedMode_UsesAndPersistsSafeWindow` | EditMode | IP-26 в STATUS |
| Shake продолжает работать после disable/pause/off/end или сдвигает gameplay anchor | `SettingsPresentationSmokeTests.Shake_Damage_PreservesCameraAnchorAndResetsOnPauseOffAndEnd` | PlayMode | IP-26 в STATUS |
| Master применяется дважды / каналы смешаны | `SettingsPresentationSmokeTests.Audio_Settings_RouteMasterOnceWithIndependentChannels` | PlayMode | IP-26 в STATUS |
| HUD terminal overlay перекрывает Results несмотря на enabled кнопки | `DefeatResultsSmokeTests.Defeat_SavedResult_IsAboveHudAndRetryStartsFreshRun` | PlayMode | [OBS-01](playtests/2026-09-21_defeat-ui.md#obs-01--после-поражения-нельзя-перезапустить-забег), IP-26 в STATUS |
| Фронт расширяющейся волны (SKILL-004) отстаёт на один кадр: волна, начавшаяся внутри tick, не продвигалась на остаток этого tick | `ProductionSkillPatternTests.ExpandingArea_DamagesOnlyWhenFrontArrives_OncePerWave` | EditMode | Unity-прогон 2026-09-24, [F1-01 evidence](implementation/evidence/field001-f1-01-2026-09-24.md#unity-прогон-2026-09-24) |
| Code-created particles без material рендерятся magenta-квадратами | `ProjectileLifecycleTests.ExplosionPresentation_ExpiryUsesReusableBurstAndWaitsForPauseAwareTail` | EditMode | [OBS-01/02](playtests/2026-09-24_e1e04fc4.md#obs-02--взрывная-сфера-выглядит-как-розовый-квадрат) |
| Прогресс сета в паузе игнорирует компоненты ниже требуемого уровня | `GameplayUiPresenterTests.SetProgress_CountsOwnedComponentsByPresence_AndNamesMissingOnes` | EditMode | [OBS-01](playtests/2026-09-24_9ae3826e.md#obs-01--прогресс-сетов-в-паузе-выглядит-неверным) |
| Вражеский снаряд без ореола читаемости | `HostileProjectileReadabilityTests.HostileProjectiles_AllHaveThreatHalo` | EditMode | [OBS-06](playtests/2026-09-24_e1e04fc4.md#obs-06--снаряд-пращника-плохо-читается) |
| Perf: `Pickup.ReachablePlacement` 20–30 ms — `BoxPickupPlacement.TryPlace` пересобирал сетку, LINQ и BFS-буферы на каждый вызов, O(препятствий) на узел | `BoxPickupPlacementTests.TryPlace_SixtyFourFieldObstacles_FullProjectionStaysWithinPerfGuardBudget` (падает без фикса: старый поиск ≈25–35 ms/вызов в .NET-harness), `TryPlace_AfterWarmUp_DoesNotAllocate`; поведение: `TryPlace_MatchesTheOriginalGridSearchExactly_OnRandomFields`, `TryPlace_ExactEqualDistanceTie_PicksTheSamePointAsTheOriginalSearch`, `TryPlace_ReusedBuffers_AcrossQueriesThatAddOrSkipExtraGridLines` (оракул `BoxPickupPlacementReference`) | EditMode | Живой плейтест 2026-09-25 (Editor.log), [DECISION-0056](decisions/0056-enemy-physics-layer.md) context; Unity-прогон 2026-09-25 PASS (726/726 EditMode) |
| Perf: orbit tick делал `EnemyDamageArea.Apply` (отдельный `OverlapCircle`) на каждый клинок | `EnemyDamageAreaCirclesTests.ApplyCircles_DamagesExactlyLikeOneApplyPerCircle`, `ApplyCircles_EnemyUnderTwoNeighbouringBlades_TakesOneHitPerBlade`, `ApplyCircles_ZeroRadiusOrNoCircles_DealsNoDamage`; существующий `SceneActiveSkillEffectExecutorTests` (orbit) | EditMode (Unity physics), PASS 2026-09-25 | Охраняют семантику батча, не время; число запросов тестом не измеряется. GAP: направление knockback от клинка не проверяется |
| Impact и explosion presenters добавляли `ParticleSystem` на один пуловый root снаряда: после попадания второй `AddComponent` возвращал null → NRE в `FixedUpdate` на каждом взрывающемся попадании (SKILL-014), FPS < 1 | `ProjectileLifecycleTests.ImpactThenExplosion_SamePooledProjectile_UsesSeparateParticleSystemsAcrossReuse` (падает без фикса: ошибка AddComponent + исключение в `TryImpact`); `ExplosionPresentation_ExpiryUsesReusableBurstAndWaitsForPauseAwareTail` (material на собственном child) | EditMode, PASS 2026-09-25 | Фикс 2026-09-25: каждый presenter владеет child-объектом `ImpactParticles`/`ExplosionParticles` (изоляция DECISION-0013). Проверяет независимые хвосты impact/explosion. Не ловит: забытый Clear при возврате в пул — `particleCount` в EditMode не наблюдаем |
