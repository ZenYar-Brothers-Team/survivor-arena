# Карта проекта

Навигация по текущим путям, без копий дизайна, чисел и execution status.
Сначала [AGENTS](../AGENTS.md), для IP — [STATUS](implementation/STATUS.md) и Context выбранной спецификации.
Затем читать только нужную область ниже. Checks здесь помогают выбрать scope;
acceptance IP и [WORKFLOW §9](implementation/WORKFLOW.md#9-завершение-модуля) имеют приоритет.

## Быстрый поиск

- `rg --files Assets/Game/<Module>` — код и тесты нужной системы; не обходить Library, Art/Source и vendored skills.
- `rg -n '^### IP-33|^Status:' docs/implementation/STATUS.md` — найти запись IP, затем прочитать её полностью.
- `python scripts/content/read_card.py SKILL-006` — полная карточка из канона, включая вложенные и одноуровневые поля.
- `python scripts/content/read_card.py --list` — ID, названия и актуальные строки; индекс вычисляется из документа.
- `python scripts/check_project.py --plan` — набор проверок по изменениям; запуска Unity нет.

Имена полей ниже: `entryPoints` — точки входа; `designRefs` — канон/контракты;
`authoringSources` — редактируемые данные; `generatedOutputs` — результаты генерации;
`checks` — проверка области. Отсутствующее поле означает, что отдельного артефакта этого вида нет.

## Запуск, сборка систем и lifecycle

Академия: `EnemyZoneInfluence` хранит area/timed channels противников;
`PortalTransitState` и `PortalTransitRuntime` проецируют delayed transit,
`CameraFollowTarget.SetTransitFocus` задаёт плавный camera flight только игроку.
Authoring: `docs/balance/field006-zones-v1.json`, presentation `FixtureZoneSeals.json`;
пакеты оправы/портала — `Art/Packets/field006-zone-seal-v001-2026-10-02.json` и
`field006-portal-burst-v1.json`. Контракт: DECISION-0153 (текущий одиночный портал),
DECISION-0142 (legacy/shared targets); проверки: BurstPortalTests,
SharedZoneTargetsTests, EnemyZoneInfluenceTests, PortalTransitStateTests,
ZoneSealPresentationTests, ProductionFieldDevZonesSmokeTests.

- entryPoints: [GameplayCompositionRoot](../Assets/Game/Bootstrap/GameplayCompositionRoot.cs), [RuntimeContentCatalog](../Assets/Game/Bootstrap/RuntimeContentCatalog.cs), [Gameplay scene](../Assets/Scenes/Gameplay.unity), [Editor startup scene](../Assets/Game/Bootstrap/Editor/GameplaySceneStartup.cs).
- designRefs: [Game Design](Game_design.md), [IP-01](implementation/modules/IP-01-run-lifecycle.md), [rollback](decisions/0010-composition-root-rollback.md).
- authoringSources: параметры запуска в [FIELD-001 baseline](balance/field001-baseline-v1.json); fixture-конфигурации в [Content/Run](../Assets/Resources/Content/Run).
- generatedOutputs: [ProductionRunSetup.json](../Assets/Resources/Content/Run/ProductionRunSetup.json).
- checks: `--scope full`; integration/production smoke в [Bootstrap/PlayModeTests](../Assets/Game/Bootstrap/PlayModeTests).

Bootstrap связывает модули. `CreateFixture()` и `CreateProduction()` явно выбирают набор данных;
слово Fixture в существующем gameplay-классе не доказывает, что класс используется только в прототипе.
В интерактивном Unity Editor `GameplaySceneStartup` один раз за сессию открывает
`Gameplay.unity` и назначает её стартовой сценой кнопки Play; batch-запуски не затрагивает.

## Контент и генерация

- entryPoints: [JsonContentFile](../Assets/Game/Content/Json/JsonContentFile.cs), [generator/TARGETS](../scripts/content/generate.py), [sources/SOURCE_PATHS](../scripts/content/sources.py).
- designRefs: [Content Design](Content_design.md), [content-json rules](../.claude/rules/content-json.md), [JSON ownership](decisions/0009-json-content-config.md).
- authoringSources: утверждённые JSON в [docs/balance](balance), имена/поля карточек Content Design и fixture environment template; полный список вычисляется в `SOURCE_PATHS`.
- generatedOutputs: файлы из `TARGETS` под [Assets/Resources/Content](../Assets/Resources/Content). Остальные JSON редактируются непосредственно по owning catalog/DTO.
- checks: `python scripts/content/generate.py --check`; `--scope content` — только static/data; `--scope full` — также Unity.

Для файла из `TARGETS` править источник и выполнять генерацию; ручная правка результата будет перезаписана.
Разделение генератора и точные команды — [scripts/content/README](../scripts/content/README.md).

## Бой, умения, прогрессия и встречи

- entryPoints: [Combat](../Assets/Game/Combat), [PlayerActiveSkillSetRuntime](../Assets/Game/ActiveSkill/Runtime/PlayerActiveSkillSetRuntime.cs), [PlayerExperienceRuntime](../Assets/Game/Progression/Runtime/PlayerExperienceRuntime.cs), [Enemy/Runtime](../Assets/Game/Enemy/Runtime), [Traveler](../Assets/Game/Traveler), [Pickup](../Assets/Game/Pickup).
- designRefs: [Game Design](Game_design.md), полные карточки из [Content Design](Content_design.md), Context выбранного [IP](implementation/modules).
- authoringSources: [balance](balance); преобразования [skills](../scripts/content/skills.py), [progression](../scripts/content/progression.py), [actors](../scripts/content/actors.py), [bosses](../scripts/content/bosses.py).
- generatedOutputs: ActiveSkills, Passives, Sets, Characters, Enemies, Bosses, Travelers, Pickups из `TARGETS`.
- checks: Tests в owning module; для связей combat/lifecycle/pooling — полный smoke. Правила C#, gameplay и при enemy AI — enemy-ai из AGENTS.

## Поля и расписания

FIELD-010 screen events (DECISION-0157/0158): authoring [events packet](balance/field010-screen-events-v1.json) → generated ProductionFieldEnvironmentPresentation (`screenEvents`, `screenEventPresentation`),
pure rules [Game.ScreenEvents](../Assets/Game/ScreenEvents) (`ScreenEventRuntime` schedule/intensity wave, `ScreenEventSpawner` layouts, `ScreenEventSurvival` fairness, `ScreenHazard` geometry),
scene side [driver](../Assets/Game/Bootstrap/ScreenEventDriver.cs) + [player adapter](../Assets/Game/Bootstrap/PlayerScreenEventTarget.cs); art mechanics [brief](art/briefs/field010-screen-events-art-v1.md).
Art: [approved packet](../Art/Packets/field010-screen-events-v1.json) → masters/provenance + eight `Assets/Resources/Art/VFX/field-010-*.png`; typed [profile](../Assets/Game/Presentation/ScreenEventPresentationProfile.cs), pooled [view](../Assets/Game/Presentation/ScreenHazardArtView.cs), shared [resources](../Assets/Game/Presentation/ScreenEventArtResources.cs), analytic [shader](../Assets/Resources/Shaders/ScreenEventArtwork.shader).
Dev tab Events: `ScreenEventDevelopmentPresenter` / `UiToolkitScreenEventDevelopmentView` over `IScreenEventDevelopmentControl` (implemented by the driver).
Motion/audio: [DECISION-0159](decisions/0159-field010-strike-motion-and-audio.md), shader/profile tuning in the same authoring packet; `HazardStrikeStarted` → `RunAudioRuntime`, `ProductionAudio.json.screenEventCues` binds all 13 events to four existing audio families.

Original lightning/circular strike clips: [DECISION-0162](decisions/0162-field010-original-strike-audio.md), deterministic `Art/Prototypes/field010-audio-sketches/generate.py` → `Resources/Audio/Sfx/lightning-v1.wav` / `light-column-v1.wav`; `docs/audio/SOURCES.json` pins source/runtime hashes. `scripts/audio/check_audio.py` validates external OGG and original WAV provenance; `ProductionAudioCatalogTests` guards actual Resources loading/import policy.
Muted FIELD-010 palette/open arena: [DECISION-0160](decisions/0160-field010-muted-presentation.md); packet `field.groundTint` → ground renderer, strike color → artwork tint; no fixture stump fallback for screen-event arenas without their own obstacle layout.
Checks: Game.ScreenEvents.Tests, UI ScreenEventDevelopment*Tests, ProductionField010ScreenEventsTests, graphics ProductionField010ScreenEventsSmokeTests (captures to TestResults/field010-*.png).

FIELD-004 traps (DECISION-0156): authoring [trap packet](balance/field004-traps-v1.json) → generated
`trapLayout` in ProductionFieldEnvironmentPresentation (only FIELD-004). Pure domain/simulation in
[Game.Traps](../Assets/Game/Traps) (`TrapLayoutDefinition`, `TrapPlacementGenerator`, `TrapRuntime`); scene side
[TrapRuntimeDriver](../Assets/Game/Bootstrap/TrapRuntimeDriver.cs) (placeholder shapes keyed by trap id / projectile visual),
[player-only bodies](../Assets/Game/Bootstrap/TrapObstacleFactory.cs), [player adapter](../Assets/Game/Bootstrap/PlayerTrapTarget.cs).
3D set: Blender generators [build_trap_model.py](../Art/Prototypes/field004-trap-models/build_trap_model.py) (12 models) and
[export_unity_fbx.py](../Art/Prototypes/field004-trap-cross-blender/export_unity_fbx.py) (reference cross) → `Assets/Art/Traps/*.fbx` →
`Game.Traps.Editor.TrapPrefabBuilder` → prefab library by key; sprites: [packet](../Art/Packets/field004-trap-sprites-v1.json).
FIELD-004 start-screen review layout: `showcase` in the trap packet (generated by `scripts/content/fields.py`); the traps and the 100 x 100 arena belong to FIELD-004 only. Start-screen cross with 3D prefab: `startTraps`/`models` in the packet, [prefab list](../Assets/Resources/Content/Presentation/TrapPrefabLibrary.asset),
prefab source `Assets/Art/Prototypes/Field004TrapCross`. Art brief: [field004-traps-art-v1](art/briefs/field004-traps-art-v1.md). checks: TrapDefinitionTests, TrapPlacementTests,
TrapRuntimeTests, ProductionField004TrapsTests, PlayMode ProductionField004TrapsSmokeTests.

Monastery FIELD-007: [altar authoring](balance/field007-altars-v1.json),
[contract](decisions/0145-field007-altars-preview.md), [art packet](../Art/Packets/field007-altars-v1.json).
Type-specific art: [candidate gallery](../Art/Candidates/field007-altar-types-2026-10-02/index.html),
[visual contract](decisions/0150-field007-altar-type-visuals.md),
[approved type packet](../Art/Packets/field007-altar-types-v1.json).
`altarPresentation.effectVisualIds` maps all 14 effects to their own typed Prop;
shared braided contour is centered on actual radius, foundation state ring is separate.
Generated Fields/FieldEnvironmentPresentation from scripts/content/fields.py; assigned FIELD-007 ground;
temporary FIELD-001 encounters. [Altar view](../Assets/Game/Presentation/AltarPresentationRuntime.cs),
[sliding-screen placement](../Assets/Game/Zones/ZonePlacementRules.cs); ProfileService's existing Dev unlock.
checks: ProductionMonasteryContentTests, ZoneScreenDensityTests, ProductionMonasterySmokeTests.
Altar feedback: `FieldMapPreviewSource.Altars` feeds the Dev mini-map via `MapPreviewAltar`;
`AltarObstacleFactory` creates player-only foundation contact. Actual random radii are authored in
`field007-altars-v1.json` and shared by clearance, gameplay, world boundaries and map markers.

Academy FIELD-006 (plus regression «Тест 06»): [cycle contract](decisions/0142-academy-zone-seal-presentation.md),
[seal profile](../Assets/Resources/Content/Presentation/FixtureZoneSeals.json) (direct authoring),
[mesh seal](../Assets/Game/Presentation/ZoneSealPresentationRuntime.cs),
[random appearance chains](../Assets/Game/Zones/RandomZoneScheduler.cs) (opt-in layout
`randomSchedule`; general chains including the single burst portal, spawn near the player,
one reusable placement per kind per chain, [DECISION-0153](decisions/0153-academy-single-burst-portal.md)),
[Rift hit](../Assets/Game/Presentation/ZoneRiftHitPresentationRuntime.cs),
новые book/outward-arrow glyphs: [approved packet](../Art/Packets/field006-new-effect-glyphs-v1.json),
[rim midpoint contract](decisions/0152-academy-rim-midpoint-and-effect-glyphs.md),
[single burst portal](decisions/0153-academy-single-burst-portal.md) (5 world units,
shared chain, violet ground glyph; old upright raster retired),
[driver](../Assets/Game/Bootstrap/ZoneRuntimeDriver.cs). Academy authoring is
[field006-zones-v1.json](balance/field006-zones-v1.json); regression authoring is `field-dev-zones-v1.json`.
FIELD-001 waves are shared by reference; Dev unlock uses the existing ProfileService command.
Ground: [dark laboratory v003 packet](../Art/Packets/field006-ground-v003-dark-laboratory.json),
same FIELD-006-VISUAL-GROUND and runtime tile path; source/provenance under Art/Source/Fields/field-006/ground.
checks: ProductionField006ContentTests, ZonePreparationTests, ZoneSealPresentationTests,
ProductionFieldDevZonesSmokeTests (including actual FIELD-006 UI launch), ZoneSealVisualSmokeTests.

FIELD-009 platforms (user-directed): authoring [platform packet](balance/field009-platforms-v1.json) (`platformLayout` / `art`), [generator](../Assets/Game/Presentation/FieldPlatformLayoutGenerator.cs), [surface](../Assets/Game/Bootstrap/FieldPlatformSurfaceRuntime.cs), [holy-ground damage](../Assets/Game/Bootstrap/FieldVoidDamageDriver.cs), [study](prototypes/field009-platforms/generate.py). FIELD-001 waves shared; Dev unlock opens it. Checks: FieldPlatformLayoutGeneratorTests, FieldPlatformSurfaceTests, graphics ProductionField009SmokeTests.

FIELD-009 platform art: [brief](art/briefs/field009-platform-art-v1.md),
[candidate review](../Art/Candidates/field009-platform-art-2026-10-02/README.md),
[approved packet](../Art/Packets/field009-platform-art-v2-approved.json),
[contract](decisions/0147-field009-holy-ground-art.md),
[art definition](../Assets/Game/Presentation/FieldPlatformArtDefinition.cs),
[material sampler](../Assets/Resources/Shaders/FieldPlatformSurface.shader).
Runtime surface is `FieldPlatformSurfaceRuntime`; art sampling/bands remain
authored in `field009-platforms-v1.json` and refs resolve via sprite registry.
Selected D bridges: optional `platformLayout.art.bridgeVeil` drives bridge-local
procedural weave in the same shader; masonry contour remains only near round plazas.
Contract DECISION-0147; checks FieldPlatformSurfaceTests (UV/orientation, transparency,
safe width, no bridge masonry, required settings and cleanup), graphics ProductionField009SmokeTests.

FIELD-003 roads: authoring [road profile](balance/field003-roads-v1.json),
generated ProductionFieldEnvironmentPresentation, [generator](../Assets/Game/Presentation/FieldRoadLayoutGenerator.cs),
[shared surface](../Assets/Game/Bootstrap/FieldRoadSurfaceRuntime.cs),
[accepted references](prototypes/field003-roads/approved-v4/README.md).
RoadLayout supplies spawn and field Books; player-only contours do not enter Traveler placement clearance.
Road art: `roadLayout.art` in the same profile, [approved packet](../Art/Packets/field003-roads-art-v1.json),
[art definition](../Assets/Game/Presentation/FieldRoadArtDefinition.cs),
[world-UV shader](../Assets/Resources/Shaders/FieldRoadSurface.shader).
Checks: art scope, FieldRoadSurfaceTests, graphics ProductionField003SmokeTests.

- entryPoints: [Field](../Assets/Game/Field), [Enemy/Model/Wave](../Assets/Game/Enemy/Model/Wave), [field generation](../scripts/content/fields.py).
- designRefs: карточки FIELD и Wave / Encounter Content в [Content Design](Content_design.md); [FIELD-001 milestone](implementation/milestones/FIELD-001-start.md).
- authoringSources: [field001 baseline](balance/field001-baseline-v1.json), [field002](balance/field002-v1.json), [field003](balance/field003-v1.json), [field002/003 waves v2](balance/field-rhythm-v2.md), [layouts](balance/field-layouts-v1.json), [blob geometry of FIELD-002 + illustration library](balance/field-dev-blobs-v1.json) (DECISION-0132, 0136), [dev zones field](balance/field-dev-zones-v1.json) и модуль [Zones](../Assets/Game/Zones) (DECISION-0134, 0135).
- generatedOutputs: ProductionFields, ProductionFieldEnvironmentPresentation, ProductionWaveTimeline / Field002 / Field003 / Field004 и единый ProductionBlobBreakupProfile (числа разбивки вне расписаний волн).
- checks: generator `--check`, [Bootstrap/Tests](../Assets/Game/Bootstrap/Tests), [production field smoke](../Assets/Game/Bootstrap/PlayModeTests), ручные gates из STATUS.

## Звук

- entryPoints: [RunAudioRuntime](../Assets/Game/Audio/RunAudioRuntime.cs), [AudioRoutingRuntime](../Assets/Game/Audio/AudioRoutingRuntime.cs), [ProductionAudioCatalog](../Assets/Game/Audio/ProductionAudioCatalog.cs), [importer](../Assets/Game/Audio/Editor/ProductionAudioImportPostprocessor.cs).
- designRefs: [AUDIO_PLAN](audio/AUDIO_PLAN.md), [IP-33](implementation/modules/IP-33-production-audio.md), [DECISION-0038](decisions/0038-settings-and-field-difficulty.md), [audio rules](../.claude/rules/audio-code.md).
- authoringSources: [ProductionAudio.json](../Assets/Resources/Content/Audio/ProductionAudio.json), [SOURCES.json](audio/SOURCES.json), DTO в [Audio/Json](../Assets/Game/Audio/Json).
- resources: [Audio/Music](../Assets/Resources/Audio/Music), [Audio/Sfx](../Assets/Resources/Audio/Sfx), [Audio/Ambience](../Assets/Resources/Audio/Ambience). `fieldAmbiences` связывает `fieldId` с clip/gain; отсутствие записи означает отсутствие атмосферы.
- checks: `--scope audio` — integrity и [Audio EditMode tests](../Assets/Game/Audio/Tests); runtime/схема/пауза → `--scope full`. Художественная приёмка — отдельное прослушивание.

Audio зависит от Settings и событий gameplay; Settings хранит предпочтения и preview interface.
Bootstrap создаёт Audio. Gameplay и UI не зависят от Audio; нового глобального event bus нет.

## UI, профиль и настройки

- Results R1: `RunResultsProjection` / `RunResultsViewState`, `RunResultsPanel`, `UI/RunResults.uxml`
  и `RunResultsStyles.uss`; MetaPresenter/MetaScreen сохраняют ownership навигации и profile intents.
  Book upgrade gold: `MetaEconomy.json` → MetaCatalog → LevelUpDraftRuntime → DraftTotals.BookCurrency → saved receipt.
  Checks: `RunResultsTests`, `RunResultsSmokeTests`, `DraftRequestTests`, `MetaProfileTests`.

- Entry R1: `Assets/Game/UI/EntryUi.cs`, `Resources/UI/EntryStyles.uss`, Character/FieldSelectScreen и AppShellScreen; декорация `MenuIllustration` / `MenuAtmosphereElement`, настройки `Assets/Resources/Content/Presentation/MenuArtProfile.json`. Утверждённые слои готовятся пакетом `Art/Packets/ui-entry-r1.json`; проверки `UiEntrySmokeTests`, `MenuArtProfileTests`, `SpriteAssetImportTests`.

- Personal Meta R1: `MetaShopPanel` / `MetaShopProjection` / `UI/MetaShop.uxml` / `UI/MetaShopStyles.uss`; `ProfileService` owns purchases/refund and `ProfileData.UpgradeSpending` records actual cost. Production profile: `profile-meta-r1.json` (old test files untouched). Checks: `PersonalMetaTests`, `MetaShopTests`, `MetaShopSmokeTests`.
- Unlocks collection: `MetaUnlockPanel` renders all unlock view states from `MetaPresenter`, with type/state filters and a shared scroll. Reuses MetaShop assets and profile purchase intents; checks: `MetaShopTests`, `MetaShopSmokeTests`.
- Meta stat icons: `Art/Packets/ui-meta-stat-icons-r1.json` → `Art/Source/UI` + `Art/UI/Icons/Meta` runtime sprites; `RuntimeContentCatalog` includes `META-*-VISUAL-ICON`, `MetaShopProjection.UpgradeIcon` resolves them for the presenter. Checks: `RuntimeContentCatalogTests`, `MetaShopSmokeTests`.
- Settings R1: `SettingsPanel`, `AppShellScreen/Presenter/ViewState`, `UI/AppShell.uxml`, `UI/SettingsStyles.uss`; settings service owns persistence/video/audio preferences. Checks: `AppShellPresenterTests`, `SettingsPresentationSmokeTests`, `AppShellSmokeTests`, `SettingsServiceTests`.
- entryPoints: [GameplayUiRoot](../Assets/Game/UI/GameplayUiRoot.cs), [UI resources](../Assets/Game/UI/Resources/UI), [Meta](../Assets/Game/Meta), [Settings](../Assets/Game/Settings).
- designRefs: [UI/UX Design](UI%20%20UX%20Design.md), [IP-26](implementation/modules/IP-26-functional-ui.md), [UI rules](../.claude/rules/ui-code.md).
- authoringSources: [MetaEconomy.json](../Assets/Resources/Content/Meta/MetaEconomy.json), [SettingsDefaults.json](../Assets/Resources/Content/Settings/SettingsDefaults.json); UI resources по ссылке выше.
- checks: UI/Meta/Settings Tests, затем требуемый integration smoke. Development UI подчиняется тем же scoped rules.
- R2 HUD/Draft/Pause: [GameplayUiPresenter](../Assets/Game/UI/GameplayUiPresenter.cs)
  готовит данные, [GameplayUiCopy](../Assets/Game/UI/GameplayUiCopy.cs) сокращает текст,
  [RecipeComponentViewState](../Assets/Game/UI/RecipeComponentViewState.cs) разделяет
  наличие компонента, текущий/projected и требуемый уровни без разбора текста,
  [PauseBuildPanel](../Assets/Game/UI/PauseBuildPanel.cs) группирует сеты/показывает справку.
  [ProductionUiR2SmokeTests](../Assets/Game/Bootstrap/PlayModeTests/ProductionUiR2SmokeTests.cs)
  проверяет production composition; `check_project.py --graphics` включает capture.

## Арт и визуальная подача

- entryPoints: [Presentation](../Assets/Game/Presentation), [art_pipeline.py](../scripts/art_pipeline.py).
- designRefs: [ART_DIRECTION](art/ART_DIRECTION.md), [Art Production](art/Art%20Production.md), [ASSET_PIPELINE](art/ASSET_PIPELINE.md), [visual rules](../.claude/rules/visual-presentation.md).
- authoringSources: packets по [art-packet template](../Art/Templates/art-packet.template.json) и правилам scripts/README; provenance в [Art/Source](../Art/Source), [ImportProfiles](../Art/ImportProfiles.json).
- generatedOutputs: runtime PNG в [Resources/Art](../Assets/Resources/Art), [manifest](../Art/asset-manifest.json), [sprite registry](../Assets/Resources/Content/Presentation/FixtureSprites.json) через art pipeline.
- checks: `--scope art`; ограниченный numeric-only `visual-preview` — только по [scripts/README](../scripts/README.md). Автопроверки не заменяют визуальную приёмку.

## Телеметрия и ручные прогоны

- automated runs design: [IP-34](implementation/modules/IP-34-automated-balance-runs.md)
  описывает bot policies, campaign templates, standalone runner и human recorder.
- automation entryPoints: [Automation](../Assets/Game/Automation), [runtime adapters](../Assets/Game/Bootstrap/Automation),
  [build/run/analyze и human record launcher](../scripts/balance/README.md).
  JSONL `demonstration-observation-v1` пишет `DemonstrationRecordingSession` перед
  physics через read-only `PlayerMover.MovementIntentApplied`; проверяет `scripts/balance/validate_demonstration.py`.
  Исследовательское offline обучение и сравнение с baseline — `scripts/balance/train_imitation.py`;
  результаты под `TestResults/imitation-*` не загружаются в игру.
- entryPoints: [Telemetry](../Assets/Game/Telemetry), [PlaytestComposition](../Assets/Game/Bootstrap/PlaytestComposition.cs), [FIELD-001 performance harness](../Assets/Game/Bootstrap/Diagnostics/Field001PerformanceBenchmark.cs), [standalone benchmark builder](../Assets/Game/Bootstrap/Editor/Field001PerformanceBuild.cs).
- designRefs: [BALANCE_WORKFLOW](implementation/BALANCE_WORKFLOW.md), [playtests/README](playtests/README.md).
- authoringSources: выбранные run reports и исходные отзывы в [playtests](playtests); approved balance deltas затем в owning source JSON.
- checks: Telemetry Tests, соответствующий PlayMode smoke; воспроизводимый standalone benchmark пишет ignored raw report в `TestResults/performance/`, а принятый итог — в implementation evidence; пользовательские matrix/density/performance gates остаются в STATUS.

## Поддержка карты

При переносе точки входа обновлять эту карту и ссылки owning IP в той же правке.
Новые числа, решения, approval и execution status сюда не копировать.
Сборки (`*.asmdef`) задают фактические зависимости; карта не заменяет проверку компиляции.
