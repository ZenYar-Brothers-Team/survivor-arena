# Regression map

IP-34 AB-14: `DemonstrationTests` проверяет clocks, bounded queue, UTF-8 byte/sample
limits, flush/footer, I/O failure и atomic promotion без overwrite.
`ExperimentConfigTests.Load_Human_RequiresRecordingOneChainAndNormalSpeed` защищает
выбор отдельного native-input режима. `DemonstrationRecordingTests` проверяет
initial/preserved pause, analog pre-physics pairs, overflow и drain перед Finished
при повторном stop (campaign/window callbacks не перезапускают ожидание);
fixture input не считается человеческой демонстрацией.
`AutomationRunHostTests.Host_ExperimentWallBudget_DoesNotRestartSaveWaitEveryFrame`
защищает завершение экспорта после общего wall budget.
`scripts/balance/test_demonstration.py` проверяет isolation launcher и rejection
partial/nonfinite/unordered records; оценка качества игрока остаётся вне validator.

IP-34 / DECISION-0098 integration: `AutomationRunHostTests.Host_FixtureVictory_UsesAuthoritativeOutcomeAndSavedReceipt`
and `AutomationCampaignHostTests.TwoLosses_UseOneProfileAndPurchaseStrengthensSecondRun`
expect no gold for the starting L1; existing fixture purchases use initial currency.

IP-34 AB-13: `BotTrajectoryPlannerTests` runs reproducible closed loops using
the production Seek controller for the test enemies: XP behind pursuing
crowds in three orientations, a denser/faster crowd and an obstacle detour.
It asserts collection plus no geometric contact, separately covers crossing
projectiles, expiration, observation reset, deterministic policy search and a
bounded 200-threat workload. These scenarios do not replace physics/combat
acceptance; production pilot evidence is owned by STATUS.
`Decide_NearbyXp_AllCandidatesUseTheFullHorizon` guards against evaluating
XP routes only until pickup while escape routes pay the full future risk;
`ClosedLoop_NoXpWithPursuers_UsesCurvedSearchAndSurvives` covers the empty-XP
case with curved routes rather than only eight straight escape headings.

IP-34 AB-12: `BotMovementPolicyTests.Decide_AdaptiveHerd_PrefersOpenXpRouteOverCloserBlockedXp`,
`Decide_AdaptiveHerd_StillBlockedAfterSweepAbandonsBankAndSelectsOtherXp` и
`Decide_AdaptiveHerd_OpenRouteAfterSweepReturnsToXp` охраняют выбор доступного
XP, отказ от безнадёжного обхода и возврат по открывшемуся коридору.
`ExperimentConfigTests.Load_HerdLoop_RequiresTypedSettingsAndKeepsOtherProfilesSeparate`
проверяет отдельный ID. Production-player pilot проверяет формат ограниченного
диагностического `movementTrace`; это не unit-test и не replay.

DECISION-0101: `GameplaySceneIntegrationTests.Camera_StopsAtEveryFieldEdge_ThenFollowsPlayerAgain`
проверяет обе оси у положительной и отрицательной границ, задержку камеры при
первом шаге внутрь, возобновление follow и ограничение render-only offset.

DECISION-0099: `EnemyPatternTests` проверяет OffsetPursuit, CommittedPursuit,
BlockedSidestep (включая малое замедление и срабатывание рядом при полной
скорости), ArcPassPursuit, InertialPursuit и
weighted fallback; `EnemyPatternSchemaTests` — обязательные поля и сумму chances;
`WaveDirectorTests.MovementSelection_IsSeededAndDoesNotChangeCompositionOrGeometryStreams`
защищает независимость seeded-потоков; `ProductionEnemyCatalogTests` фиксирует
ENEMY-001: 75% BlockedSidestep / по 5% Seek, OffsetPursuit, CommittedPursuit, ArcPassPursuit, InertialPursuit; усиленные параметры и скорость 1.056.

DECISION-0098: `MetaProfileTests.LevelReward_ExcludesStartingLevel_PreservesBookGold`
проверяет L1=0, L2=5 и независимую Book-награду; terminal reasons на L1
покрывает `Exit_StartedAtLevelOne_PaysNoLevelGold`, а сохранение/Retry —
`Apply_ResultDuplicateAndReload_PayExactlyOnce` и `MetaProgressionSmokeTests`.
`UiLayoutR2SmokeTests` проверяет material sprite и видимость слоя в Pause.

UI folio follow-up: `SettingsPresentationSmokeTests.SettingsFolio_TwoResolutions_ConfirmationAndFocus`
проверяет folio-цвет стартовой камеры, скрытый `Obstacle_Fixture` и выключенное
по умолчанию управление мышью в модели/Toggle. `MetaPresenterTests.UpgradesToggle_WhileSaving_KeepsPurchaseReasonStable`
удерживает предметную причину покупки во время задержанной записи профиля.
`MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions` проверяет segmented
state controls и начальное `Закрыто` для умений/сетов в 1080p/720p.

Meta stat icons R1: MetaShopSmokeTests проверяет 12 ненулевых sprites из registry,
slot 32 px перед названием, отсутствие пересечения с title/level в 1080p и 720p.

DECISION-0094: `RunSetupConfigTests.ProductionSetup_StartsWithOneRerollAndOneBanish`
проверяет production-базу 1/1; `MetaShopSmokeTests` проверяет 2/2 после покупки +1.
Расход, общий остаток XP/Book и reset покрывают существующие DraftRequestTests /
LevelUpDraftRuntimeTests; isolated fixture 2/2 сохранён.

Meta feedback 2026-09-29: `MetaShopSmokeTests` открывает Meta прямо из Main Menu,
без предварительного Character Select, проверяет portrait/icon bindings, отсутствие
дублированного «?» на silhouette и стабильность row identity/scroll/position при
toggle. `MetaShopTests` проверяет точные стартовые 10/10/5 и presentation order.

Settings R1 (DECISION-0093): `AppShellPresenterTests.Settings_VideoConfirmation_UsesServiceCountdownAndReturnsToSettings`
проверяет projection service countdown/Apply и Back из подтверждения без закрытия Settings.
`SettingsPresentationSmokeTests.SettingsFolio_TwoResolutions_ConfirmationAndFocus` —
1080p/720p без прокрутки, проценты, modal input/focus и возврат к выбору режима.

Unlocks R1 (DECISION-0092): `MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`
проверяет полный каталог, начальные открытия и фильтры без скрытия карт/сетов.
`MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions` проверяет 70 карточек,
фильтры, пустое состояние, конец scroll и закреплённые действия в 1080p/720p.

Personal Meta R1 (DECISION-0091): `PersonalMetaTests` проверяет 12 каналов, личное
владение, независимые цены/пределы, отключение бонусов, возврат 999/1000/1001,
ошибку записи, stale/double intent и reload. `MetaShopTests` — подтверждение/отмена
и semantic IDs. `MetaShopSmokeTests` — production 1080p/720p, scroll, выбор героя,
покупка/возврат и стартовые счётчики перебросов/исключений. Scope/evidence — STATUS.

Results R1 / Book upgrade reward: `RunResultsTests`, `RunResultsSmokeTests`,
`DraftRequestTests.BookUpgrade_*`, `MetaProfileTests.MixedBookRewards_SaveRetryAndReload_PreserveExactReceipt`:
typed receipt/run identity, mixed unlock kinds, two viewport sizes/scroll, same-selection Retry,
20/50 alternatives, duplicate/cancel boundaries and persistence without double credit.

Menu atmosphere motion: `MenuArtProfileTests` проверяет уменьшенную скорость, криволинейный подъём, разные траектории частиц, повторяемость координат и неизменную амплитуду лучей; результат запуска — в STATUS.

Индекс **уже существующих** тестов, которые охраняют критические пути. Не заменяет evidence в `docs/implementation/STATUS.md`. Поддерживается скилом `/regression-map` (`update` / `check` / `bug`).
Составлено 2026-09-20 по `Assets/Game/**/Tests` и `PlayModeTests`; наличие классов проверено поиском, результаты прогона тут не фиксируются (см. `/smoke-check`).

| Критический путь | Охраняющие тесты (класс) | Вид | Статус |
|------------------|--------------------------|-----|--------|
| Entry UI: locked inspection vs confirmation, two viewport sizes, 10-field grid and overflow, approved full-canvas menu imports | `UiEntrySmokeTests`, `CharacterSelectPresenterTests`, `FieldSelectPresenterTests`, `MenuArtProfileTests`, `SpriteAssetImportTests` | EditMode + PlayMode graphics | Covered |
| Жизненный цикл забега, пауза, конец | `Game.Run.Tests` (`RunModelTests`) | EditMode | OK |
| Движение игрока, границы | `Game.Movement.Tests` (`MovementVelocityCalculatorTests`, `GameplaySceneIntegrationTests`, `PlayerObstacleCollisionTests`) | EditMode | OK |
| Урон → смерть → конец забега | `CharacterRunBindingTests`, `CharacterHealthIntegrationTests`, `Game.Combat.Tests` (`HealthTests`) | EditMode | OK |
| Откат/повтор инициализации `PlayerCharacterRuntime` | `PlayerCharacterRuntimeLifecycleTests` | EditMode | OK (добавлено в ревью) |
| XP: дроп → подбор → уровень → пауза | `ExperienceProgressionTests`, `ExperienceDropTests`, `ExperienceIntegrationTests`, `RunSetupConfigTests` | EditMode | OK |
| Драфт: офферы, reroll, banish, очередь, сеты | `LevelUpDraftRuntimeTests`, `PlayerBuildAndDraftTests`, `SetFrameworkTests`, `PassiveFrameworkTests` | EditMode | OK |
| Откат `PlayerActiveSkillSetRuntime`/`PlayerPassiveSetRuntime` при сбое Initialize; passive повторный Initialize и Shutdown→Initialize без stale catalog/stacking | `PlayerActiveSkillSetRuntimeRollbackTests`, `PlayerPassiveSetRuntimeRollbackTests` | EditMode | OK (добавлено в ревью) |
| Активные навыки: срабатывание, кулдаун, эффекты, мины | `PlayerActiveSkillSetRuntimeTests`, `ActiveSkillTimingTests`, `ProjectileAndAreaTests`, `SceneActiveSkillEffectExecutorTests`, `ActiveSkillProgressionFrameworkTests` | EditMode | OK |
| Видимость сетов по мета-открытиям и упущенные сеты (DECISION-0073) | `SetReachabilityTests`, `GameplayUiPresenterTests.SetProgress_MissedSetsAreListedLast` | EditMode | OK; фильтр draft pool в `GameplayUiRuntimeModel` без прямого теста (TD-040) |
| Первый дроп зелья без подлага; свежий seed бросков дропа (DECISION-0074) | `WorldPickupRuntimeTests` (`Initialize_PrewarmsOnePooledVisual_…`, `DeathDrop_RollSequenceFollowsTheRunSeed`) | EditMode | OK |
| XP-кривая, урон врагов по игроку, специализация стартового умения (DECISION-0075) | `RunSetupConfigTests.ProductionSetup_…`, `ProductionCharacterCatalogTests`, `PlayerActiveSkillSetRuntimeTests.SkillModifier_…`, `WorldPickupSmokeTests` | EditMode / PlayMode | OK |
| Wave Director: фазы, хуки, лимиты, состав | `WaveDefinitionTests`, `WaveDirectorTests` | EditMode | OK |
| Спавн/деспавн врагов, пул, `EnemyRegistry` | `WaveSpawnerTests`, `EnemyRuntimeLifecycleTests`, `EnemySpawnerSceneIntegrationTests` | EditMode | OK |
| Паттерны движения/атаки врагов | `EnemyPatternTests`, `EnemyMovementAndContactTests` | EditMode | OK |
| Раскладка препятствий полей каждый забег: равномерность по ячейкам, свободный старт, проходы, сид (DECISION-0068); все визуальные варианты в каждом забеге (DECISION-0073) | `FieldObstacleLayoutGeneratorTests`, `ProductionFieldContentTests`, `ProductionField002ContentTests`, `ProductionField003ContentTests`, `ProductionField002SmokeTests`, `ProductionField003SmokeTests` | EditMode / PlayMode | OK |
| Особые атаки боссов (зоны, лучи, призыв), повтор залпа, атаки после рывка, фазовые рывки (DECISION-0066) | `BossHazardFieldTests`, `BossHazardRuntimeTests`, `BossSpecialStepTests`, `EnemyAttackFollowUpTests`, `EnemyDashEndAttacksTests`, `ProductionLateBossCatalogTests`, `BossHazardSmokeTests` | EditMode / PlayMode | OK |
| Пул `GameObjectPool<T>` | `GameObjectPoolTests` | EditMode | OK (добавлено в ревью) |
| Общий валидатор чисел | `NumericValidationTests` | EditMode | OK (добавлено в ревью) |
| Загрузка контента: реестр, ссылки, каталоги | `ContentRegistryTests`, `RuntimeContentCatalogTests`, `FixtureCharacterCatalogTests` | EditMode | OK |
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
| Противоположный спавн покрывает всё кольцо | `WaveDirectorTests.SelectSpawnAngle_BiasesOppositeCentroidButKeepsFullRing`, `SelectSpawnAngle_ZeroBiasMatchesUniformStream` | EditMode | См. IP-14 в [STATUS](implementation/STATUS.md) | Проверяет смещение, ненулевой охват всех секторов и воспроизводимость; реальную форму толпы проверяет плейтест. |

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

Raster contact profiles: `python scripts/fit-body-contacts.py` checks every saved circle against the current PNG, PPU and pivot. It caught ENEMY-007's stale v001-sized collider after its body was scaled to v002; rerun after any body PNG or contact-profile change. [Refit evidence](implementation/evidence/2026-09-26-enemy007-contact-refit.md).

EnemyBodyPresentationTests защищает child-only motion, hit/pause, mixed-pool reuse
и typed motion reference/scaling; EnemySpriteImportTests — import/reimport GUID,
size/pivot/transparent border; EnemyBodySmokeTests — реальную цепочку
composition → spawner → pooled enemy в Gameplay. Evidence: [enemy art](implementation/evidence/2026-09-21-enemy001-art.md).

## IP-26 — regression guards

UI layout R2 / [review 2026-09-28 OBS-01…03](playtests/2026-09-28_ui-card-layout.md):
`UiFoundationTests.DraftCard_TypeLabel_UsesSemanticElementAndVisualClass` охраняет
подписи и icon/type/level header; `UiLayoutR2Tests.DraftRecipes_RenderProjectedIconTitleAndProgress`
— projection icon/name/progress (включая null icon). `ProductionUiR2SmokeTests.Production_HudDraftPause_AnchorsAndRetry_WithTwoResolutionCaptures`
проверяет production sprites, горизонтальную геометрию шапки, крупные type labels
без pill и левое выравнивание в 720p/1080p. Без исправления новые assertions падают.

Тот же review OBS-04…07: `GameplayUiPresenterTests.RecipeProjection_PartialThresholdCompletesAndAlreadyEnoughAreDistinct`
охраняет owned count независимо от thresholds; `UiLayoutR2Tests.DraftComponents_PresenceAndCurrentLevelMet_AreIndependent`
— ✓/○ и зелёный текущий threshold, не projected. `LevelDraftHeading_OmitsEarnedAndNextLevels_KeepingOnlyQueueCount`
— отсутствие уровней в шапке при сохранённом числе выборов. `CooldownCopy_UsesReciprocalFrequency`
и `PercentCopy_RoundsToWholeWithoutNegativeZero` — math/rounding;
`ProductionUiR2SmokeTests.StoneCopy_HidesDerivedFlightLifetime_ButPreservesTrueLifetimeChanges`
— фактический SKILL-001 и сохранение содержательного lifetime change.

Тот же review OBS-08: `UiFoundationTests.PauseRecipes_ZeroOwnedAttainable_ShowsZeroAndKeepsAcquiredAndMissedSeparate`
защищает видимость достижимого рецепта с `0/N` и отдельные acquired/missed;
`UiLayoutR2SmokeTests.DenseRecipes_OnePauseScroll_PopupAndDraftInspectionNeverCommit`
проверяет 12 достижимых рецептов, в том числе неначатый, общий scroll и popup
без Resume/selection в 720p/1080p. Старый фильтр `!HasProgress` ломает оба теста.

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
| Retired perf path: `Pickup.ReachablePlacement` BFS больше не существует; pickups уже используют bounds-only placement, Traveler movement проходит player-only obstacles, spawn проверяет только bounds + obstacle exclusion | `BoxPickupPlacementTests` (`Contains`, `ClampToBounds`, no allocations), `TravelerRuntimeTests.MovementClamp_IgnoresPlayerOnlyObstacles_ButKeepsArenaBounds` | EditMode + standalone stress | [DECISION-0075](decisions/0075-progression-specialization-and-survivability.md), [DECISION-0082](decisions/0082-simple-traveler-protector-targeting.md) |
| Perf: orbit tick делал `EnemyDamageArea.Apply` (отдельный `OverlapCircle`) на каждый клинок | `EnemyDamageAreaCirclesTests.ApplyCircles_DamagesExactlyLikeOneApplyPerCircle`, `ApplyCircles_EnemyUnderTwoNeighbouringBlades_TakesOneHitPerBlade`, `ApplyCircles_ZeroRadiusOrNoCircles_DealsNoDamage`; существующий `SceneActiveSkillEffectExecutorTests` (orbit) | EditMode (Unity physics), PASS 2026-09-25 | Охраняют семантику батча, не время; число запросов тестом не измеряется. GAP: направление knockback от клинка не проверяется |
| Perf: area-запросы урона (`EnemyDamageArea`, expanding area, orbit, аура SET-010) брали все коллайдеры, включая XP drops/снаряды/pickups, и искали компонент у каждого | `EnemyPhysicsLayerTests.AreaQuery_IgnoresNonEnemyTriggers_AndDamageIsUnchanged` (100 trigger-коллайдеров рядом; без фикса запрос возвращает 101), `SpawnedEnemy_IsOnEnemyLayer_PlayerIsNot`; семантика урона — существующие `EnemyDamageAreaCirclesTests`, `ProjectileAndAreaTests`, `SetEffectTests` | EditMode (Unity physics), PASS 2026-09-26 (757/757) | [DECISION-0056](decisions/0056-enemy-physics-layer.md). В игре пользователь подтвердил 2026-09-26 («всё хорошо»), время не измерялось |
| Поздние умения/пассивки (DECISION-0060): данные L1…L6, доступ по профилю, процедурный луч | `ProductionLateSkillCatalogTests` (6), `ProductionPassiveCatalogTests.LatePassives_MapCardValuesToTheirSingleChannel`, `Stubbornness_ScalesLinearlyWithMissingHealth_AndCapsAtTenPercent`, `ProductionStartupContentTests.StartupDefinitions_…` (новый профиль открывает ровно стартовые 10+10), `ProductionSkillPatternTests.BeamTick_DrawsGlowAndCoreOverTheHitBand_ThenFades` | EditMode, PASS 2026-09-26 (766/766) | GAP: реальный прогон с поздними ID и читаемость луча не проверены |
| Сеты SET-002…020 (DECISION-0061): skill-specific механики, тяжёлый мусор без усиления от SET-015, веерный разряд | `SetMechanicsExecutorTests` (9), `ProductionLateSetCatalogTests` (6), `SetEffectTests.Catalog_AllAcceptedFamiliesHaveRealConfiguredFixtures` | EditMode, PASS 2026-09-26 (784/784) | GAP: реальный прогон и сочетание 3–4 сетов не проверены |
| Враги ENEMY-006, 008…020 (DECISION-0062): карточные числа, скорость ×1.3, паттерны IP-13, без заимствованного арта | `ProductionLateEnemyCatalogTests` (6), `ProductionEnemyCatalogTests.Catalog_ContainsAllTwentyEnemies_WithStartupSixRebalanced` | EditMode, PASS 2026-09-26 (790/790) | GAP: в игре не встречаются до полей IP-23/24 |
| Срез FIELD-002 (DECISION-0063): кольцо в конце рывка, боссы/поле/волны второго поля, Путники без повторов ролей | `EnemyDashVolleyControllerTests` (4), `ProductionField002ContentTests` (5), `TravelerScheduleTests.RoleAwareDraw_NeverRepeatsARole_AndCapsCountByRoles`, PlayMode `ProductionField002SmokeTests` | EditMode + PlayMode, PASS 2026-09-26 (802/802 + 28/28) | GAP: 15-минутный прогон и сложность FIELD-002 не проверены |
| Impact и explosion presenters добавляли `ParticleSystem` на один пуловый root снаряда: после попадания второй `AddComponent` возвращал null → NRE в `FixedUpdate` на каждом взрывающемся попадании (SKILL-014), FPS < 1 | `ProjectileLifecycleTests.ImpactThenExplosion_SamePooledProjectile_UsesSeparateParticleSystemsAcrossReuse` (падает без фикса: ошибка AddComponent + исключение в `TryImpact`); `ExplosionPresentation_ExpiryUsesReusableBurstAndWaitsForPauseAwareTail` (material на собственном child) | EditMode, PASS 2026-09-25 | Фикс 2026-09-25: каждый presenter владеет child-объектом `ImpactParticles`/`ExplosionParticles` (изоляция DECISION-0013). Проверяет независимые хвосты impact/explosion. Не ловит: забытый Clear при возврате в пул — `particleCount` в EditMode не наблюдаем |
| World art сетов: отдельные атаки не получали каталог визуалов, а тяжёлый мусор SET-008 использовал обычную крышку SKILL-016 | `SetCombatIntegrationTests.SetAttack_UsesRegisteredProjectileSprite`, `SetMechanicsExecutorTests.HeavyReplacement_UsesSetSpriteWhileOrdinaryJunkKeepsSkillSprite` | EditMode | 2026-09-26 set world-art integration; runtime/visual acceptance записываются в IP-19 STATUS |
| Лечение регенерацией шло с источником `default` → в телеметрии строка `unknown`/`Unknown` (прогон 794c2696, 66 817 событий) | `PlayerCharacterRuntimeLifecycleTests.Update_WhileRunning_ReportsRegenerationWithItsOwnSource`, `RunTelemetryRecorderTests.ContentlessSourceWithKnownOrigin_IsLabelledByOrigin` | EditMode | Фикс 2026-09-26: origin `Regeneration`, строка отчёта подписана по origin. Не ловит: другие вызовы `Heal(float)` без источника (DEV-кнопка лечения остаётся `Unknown`) |
| Отключение постоянных улучшений (DECISION-0064): нулевой вклад без возврата, сохранение, запрет в забеге/при ошибке записи, миграция профиля v1→v2, галочка только на экране магазина | `MetaProfileTests.UpgradesDisabled_RemovesBonusWithoutRefund_PersistsAndRestores`, `MetaProfileTests.UpgradesDisabled_CannotChangeDuringRun_OrWhenSaveFails`, `MetaProfileTests.Load_V1Profile_MigratesWithUpgradesActive`, `MetaPresenterTests.UpgradesToggle_OnlyInShop_DisablesUpgradesAndMarksCards` | EditMode | GAP: применение нулевого модификатора в реальном старте забега проверено только через `Modifier`, без PlayMode-сценария |
| DEV-сброс прогрессии: новый профиль с сохранением прежних файлов, запрет в забеге, подтверждение вторым кликом и сброс подтверждения вне главного меню | `MetaProfileTests.DevelopmentReset_ReturnsToNewProfile_PreservingPreviousFiles`, `AppShellPresenterTests.DevelopmentReset_NeedsSecondClick_AndDisarmsOutsideMainMenu`, `AppShellPresenterTests.Assets_AllShellSemanticIdsExist` | EditMode | GAP: composition (`ResetProgressionForDevelopment`, повторные уведомления об открытиях) без прямого теста |
| Визуальный балансный прогон становился полноэкранным: пустое изолированное хранилище Settings применяло desktop borderless поверх параметров запуска | `SettingsServiceTests.Load_SeededAutomationWindow_AppliesWindowedModeWithoutRewriting`, `BalanceRunnerTests.test_visual_is_opt_in_and_audio_is_separate` | EditMode + Python | Визуальный worker получает изолированный оконный 1280×720 профиль; обычные настройки игрока и скрытые прогоны не меняются. Фактический размер окна требует проверки в standalone |
