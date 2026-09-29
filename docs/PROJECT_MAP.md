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

- entryPoints: [GameplayCompositionRoot](../Assets/Game/Bootstrap/GameplayCompositionRoot.cs), [RuntimeContentCatalog](../Assets/Game/Bootstrap/RuntimeContentCatalog.cs), [Gameplay scene](../Assets/Scenes/Gameplay.unity).
- designRefs: [Game Design](Game_design.md), [IP-01](implementation/modules/IP-01-run-lifecycle.md), [rollback](decisions/0010-composition-root-rollback.md).
- authoringSources: параметры запуска в [FIELD-001 baseline](balance/field001-baseline-v1.json); fixture-конфигурации в [Content/Run](../Assets/Resources/Content/Run).
- generatedOutputs: [ProductionRunSetup.json](../Assets/Resources/Content/Run/ProductionRunSetup.json).
- checks: `--scope full`; integration/production smoke в [Bootstrap/PlayModeTests](../Assets/Game/Bootstrap/PlayModeTests).

Bootstrap связывает модули. `CreateFixture()` и `CreateProduction()` явно выбирают набор данных;
слово Fixture в существующем gameplay-классе не доказывает, что класс используется только в прототипе.

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

- entryPoints: [Field](../Assets/Game/Field), [Enemy/Model/Wave](../Assets/Game/Enemy/Model/Wave), [field generation](../scripts/content/fields.py).
- designRefs: карточки FIELD и Wave / Encounter Content в [Content Design](Content_design.md); [FIELD-001 milestone](implementation/milestones/FIELD-001-start.md).
- authoringSources: [field001 baseline](balance/field001-baseline-v1.json), [field002](balance/field002-v1.json), [field003](balance/field003-v1.json), [layouts](balance/field-layouts-v1.json).
- generatedOutputs: ProductionFields, ProductionFieldEnvironmentPresentation, ProductionWaveTimeline / Field002 / Field003.
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

- entryPoints: [Telemetry](../Assets/Game/Telemetry), [PlaytestComposition](../Assets/Game/Bootstrap/PlaytestComposition.cs), [FIELD-001 performance harness](../Assets/Game/Bootstrap/Diagnostics/Field001PerformanceBenchmark.cs), [standalone benchmark builder](../Assets/Game/Bootstrap/Editor/Field001PerformanceBuild.cs).
- designRefs: [BALANCE_WORKFLOW](implementation/BALANCE_WORKFLOW.md), [playtests/README](playtests/README.md).
- authoringSources: выбранные run reports и исходные отзывы в [playtests](playtests); approved balance deltas затем в owning source JSON.
- checks: Telemetry Tests, соответствующий PlayMode smoke; воспроизводимый standalone benchmark пишет ignored raw report в `TestResults/performance/`, а принятый итог — в implementation evidence; пользовательские matrix/density/performance gates остаются в STATUS.

## Поддержка карты

При переносе точки входа обновлять эту карту и ссылки owning IP в той же правке.
Новые числа, решения, approval и execution status сюда не копировать.
Сборки (`*.asmdef`) задают фактические зависимости; карта не заменяет проверку компиляции.
