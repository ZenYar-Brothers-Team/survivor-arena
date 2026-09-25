using System;
using System.Collections.Generic;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class GameplayUiPresenterTests
    {
        [Test]
        public void Boss_ProductionHud_ProjectsHealthAndClearsWithoutDevelopmentCommands()
        {
            var model = CreateModel();
            model.DevelopmentCommandsEnabled = false;
            model.Boss = new BossViewState(Guid.NewGuid(), "Commander", 150, 500);
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();
            Assert.IsTrue(view.Hud.Boss.Visible);
            Assert.AreEqual(150, view.Hud.Boss.CurrentHealth);
            Assert.AreEqual("Commander", view.Hud.Boss.Name);
            model.Boss = default;
            presenter.RefreshHud();
            Assert.IsFalse(view.Hud.Boss.Visible);
            Assert.AreEqual(125, view.Hud.ElapsedSeconds);
        }

        [Test]
        public void Start_RendersImmutableSnapshotsAndDevelopmentVisibility()
        {
            var model = CreateModel();
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();

                Assert.AreEqual(75f, view.Hud.CurrentHealth);
                Assert.AreEqual(100f, view.Hud.MaxHealth);
                Assert.AreEqual(0.4f, view.Hud.ExperienceProgress01);
                Assert.AreEqual(3, view.Hud.Level);
                Assert.AreEqual(125f, view.Hud.ElapsedSeconds);
                Assert.IsTrue(view.Draft.IsVisible);
                Assert.AreEqual(3, view.Draft.Options.Count);
                Assert.IsFalse(view.Draft.Options[1].IsEnabled);
                Assert.IsFalse(view.Draft.Options[2].IsEnabled);
                Assert.AreEqual("Fixture Passive", view.Draft.Options[0].Title);
                Assert.AreEqual(2, view.Draft.RemainingRerolls);
                Assert.AreEqual(PlayerBuild.ActiveSlotCapacity, view.Build.ActiveSlots.Count);
                Assert.AreEqual(PlayerBuild.PassiveSlotCapacity, view.Build.PassiveSlots.Count);
                Assert.AreEqual("Fixture Active", view.Build.ActiveSlots[0].Title);
                Assert.AreEqual(1, view.Build.ActiveSlots[0].Level);
                Assert.IsFalse(view.Build.PassiveSlots[2].IsOccupied);
                Assert.AreEqual(1, view.Build.Sets.Count);
                Assert.AreEqual("Fixture Set", view.Build.Sets[0].Title);
                Assert.AreEqual(1, view.Build.SetRecipeProgress.Count);
                Assert.IsTrue(view.Build.SetRecipeProgress[0].IsAcquired);
                Assert.IsFalse(view.Overlay.IsVisible, "Draft pause must not show the generic pause overlay.");
                Assert.IsTrue(view.DevelopmentVisible);
                Assert.AreEqual(1, view.Characters.Characters.Count);
                Assert.IsTrue(view.Characters.Characters[0].IsSelected);
                Assert.AreEqual("FIXTURE-ACTIVE-UI", view.Characters.Characters[0].StartingSkillId);
            }
        }

        [Test]
        public void ViewIntents_AreForwardedToModelCommands()
        {
            var model = CreateModel();
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                var id = model.DraftOptions[0].Definition.Id;

                view.RaiseReroll();
                view.RaiseBanish(id);
                view.RaiseSelect(id);
                view.RaisePause();
                view.RaiseAddExperience();
                view.RaiseDamage();
                view.RaiseHealing();
                view.RaisePresentationMotion(SpritePresentationPreviewMotion.Left);
                view.RaisePresentationReset();

                Assert.AreEqual(1, model.RerollCalls);
                Assert.AreEqual(id, model.LastBanished);
                Assert.AreEqual(id, model.LastSelected);
                Assert.AreEqual(1, model.PauseCalls);
                Assert.AreEqual(1, model.AddExperienceCalls);
                Assert.AreEqual(1, model.DamageCalls);
                Assert.AreEqual(1, model.HealingCalls);
                Assert.AreEqual(SpritePresentationPreviewMotion.Left, model.LastPreviewMotion);
                Assert.AreEqual(1, model.PresentationResetCalls);
            }
        }

        [Test]
        public void SpeedIntent_UpdatesRunningHudAndIgnoresPausedRun()
        {
            var model = CreateModel();
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();
            model.RunState = RunState.Running;
            model.RaiseChanged();

            view.RaiseSpeed(3);
            Assert.AreEqual(3, model.SpeedMultiplier);
            Assert.AreEqual(3, view.Hud.SpeedMultiplier);
            Assert.IsTrue(view.Hud.CanChangeSpeed);

            model.RunState = RunState.Paused;
            model.RaiseChanged();
            view.RaiseSpeed(5);
            Assert.AreEqual(3, model.SpeedMultiplier);
            Assert.IsFalse(view.Hud.CanChangeSpeed);
        }

        [Test]
        public void BanishMode_CancelIsFree_AndNewRevisionAndClosedDraftResetMode()
        {
            var model = CreateModel();
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();
            view.RaiseBanishMode();
            Assert.IsTrue(view.Draft.IsBanishMode);
            Assert.IsFalse(view.Draft.CanReroll);
            view.RaiseReroll();
            Assert.AreEqual(0, model.RerollCalls);
            view.RaiseBanishMode();
            Assert.IsFalse(view.Draft.IsBanishMode);
            Assert.IsFalse(model.LastBanished.IsValid);
            Assert.AreEqual(1, model.RemainingBanishes);
            view.RaiseBanishMode();
            var old = view.Draft.Revision;
            model.DraftRevision = Guid.NewGuid();
            model.RaiseChanged();
            Assert.IsFalse(view.Draft.IsBanishMode);
            view.RaiseBanishMode(old);
            Assert.IsFalse(view.Draft.IsBanishMode);
            view.RaiseBanishMode();
            model.IsDraftOpen = false;
            model.RaiseChanged();
            Assert.IsFalse(view.Draft.IsBanishMode);
            model.IsDraftOpen = true;
            model.RemainingBanishes = 0;
            model.RemainingRerolls = 0;
            model.RaiseChanged();
            view.RaiseBanishMode();
            Assert.IsFalse(view.Draft.IsBanishMode);
            Assert.IsFalse(view.Draft.CanBanish);
            StringAssert.Contains("No rerolls or banishes", view.Draft.ControlHint);
        }

        [Test]
        public void ModelChange_RebuildsRunResultOverlay()
        {
            var model = CreateModel();
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                model.IsDraftOpen = false;
                model.RunState = RunState.Won;
                model.RaiseChanged();

                Assert.IsTrue(view.Overlay.IsVisible);
                Assert.AreEqual("RUN COMPLETE", view.Overlay.Title);
                Assert.IsFalse(view.Overlay.CanResume);
            }
        }

        [Test]
        public void WaveTransition_RebuildsHudWaveAndObservability()
        {
            var model = CreateModel();
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                Assert.AreEqual(2, view.Hud.Wave.PhaseNumber);
                Assert.AreEqual(5, view.Hud.Wave.PhaseCount);
                Assert.AreEqual("Pressure", view.Hud.Wave.DisplayName);
                Assert.AreEqual(WavePhaseTag.Pressure, view.Hud.Wave.Tag);
                Assert.AreEqual("Fixture wave", view.WaveObservation.Summary);
                Assert.AreEqual("Fixture skills", view.SkillObservation.Summary);

                model.WavePhaseNumber = 3;
                model.WavePhaseName = "Respite";
                model.WavePhaseTag = WavePhaseTag.Rest;
                model.WaveDevelopmentSummary = "Rest phase";
                model.RaiseChanged();

                Assert.AreEqual(3, view.Hud.Wave.PhaseNumber);
                Assert.AreEqual("Respite", view.Hud.Wave.DisplayName);
                Assert.AreEqual(WavePhaseTag.Rest, view.Hud.Wave.Tag);
                Assert.AreEqual("Rest phase", view.WaveObservation.Summary);
            }
        }

        [Test]
        public void ProductionModel_HidesDevelopmentControls()
        {
            var model = CreateModel();
            model.DevelopmentCommandsEnabled = false;
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                Assert.IsFalse(view.DevelopmentVisible);
            }
        }

        [Test]
        public void ProductionModel_DoesNotBuildDevelopmentObservability()
        {
            var model = CreateModel();
            model.DevelopmentCommandsEnabled = false;
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                model.RaiseChanged();

                Assert.IsNull(view.EnemyObservation);
                Assert.IsNull(view.WaveObservation);
                Assert.AreEqual(model.WavePhaseNumber, view.Hud.Wave.PhaseNumber, "The HUD wave badge is not development-only.");
            }
        }

        [Test]
        public void BookOriginQueueCurrencyAndRevision_ArePreservedByPresenter()
        {
            var model = CreateModel();
            var run = Guid.NewGuid();
            model.CurrentDraftRequest = DraftRequest.ForBook(run, Guid.NewGuid(), new ContentId("FIXTURE-BOOK"));
            model.NextDraftRequest = DraftRequest.ForLevel(run, 4);
            model.PendingDraftCount = 2;
            model.BookCurrency = 7;
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();
            Assert.AreEqual("TRAVELER BOOK", view.Draft.Heading);
            StringAssert.Contains("Level 4", view.Draft.QueueDetail);
            Assert.AreEqual(model.DraftRevision, view.Draft.Revision);
            Assert.AreEqual(7, view.Hud.BookCurrency);
            view.RaiseSelect(model.DraftOptions[0].Definition.Id);
            Assert.AreEqual(model.DraftRevision, model.LastRevision);
            view.RaiseBook();
            Assert.AreEqual(1, model.BookCalls);
            model.DevelopmentCommandsEnabled = false;
            view.RaiseBook();
            Assert.AreEqual(1, model.BookCalls);
        }

        [Test]
        public void PassiveDetails_InReleaseModeRefreshLowHealthWithoutMutatingOldSnapshot()
        {
            var model = CreateModel();
            model.DevelopmentCommandsEnabled = false;
            var definition = FixturePassiveCatalog.Create().Single(x => x.Id.ToString() == "FIXTURE-PASSIVE-LOW-HEALTH");
            var build = new PlayerBuild(new BuildEntryDefinition("FIXTURE-SKILL-UI", BuildEntryKind.ActiveSkill, "Skill"));
            build.Apply(definition);
            model.BuildEntries = new List<BuildEntry>(build.Entries);
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            stats.SetModifier("passive", definition.GetLevel(1));
            model.Stats = new CharacterStatsViewState(stats);
            var view = new FakeView();
            using (var presenter = new GameplayUiPresenter(model, view))
            {
                presenter.Start();
                var previous = view.Build.PassiveSlots[0];
                StringAssert.Contains("Max low-HP damage: 15%", previous.Detail);
                StringAssert.Contains("Current low-HP damage: x1", previous.Detail);
                stats.UpdateHealthRatio(0.1f);
                model.Stats = new CharacterStatsViewState(stats);
                presenter.RefreshAll();
                StringAssert.Contains("Current low-HP damage: x1.15", view.Build.PassiveSlots[0].Detail);
                Assert.AreNotEqual(previous.Detail, view.Build.PassiveSlots[0].Detail);
                Assert.AreEqual(6, view.Build.PassiveSlots.Count);
            }
        }

        [Test]
        public void RecipeProjection_PartialThresholdCompletesAndAlreadyEnoughAreDistinct()
        {
            var active = new BuildEntryDefinition("FIXTURE-A", BuildEntryKind.ActiveSkill, "Active");
            var p1 = new BuildEntryDefinition("FIXTURE-P1", BuildEntryKind.PassiveItem, "First");
            var p2 = new BuildEntryDefinition("FIXTURE-P2", BuildEntryKind.PassiveItem, "Second");
            var set = new SetDefinition("FIXTURE-SET", "Set", new SetRecipeComponent(active.Id, active.Kind, 2),
                new SetRecipeComponent(p1.Id, p1.Kind, 1), new SetRecipeComponent(p2.Id, p2.Kind, 1));
            var build = new PlayerBuild(active);
            var model = CreateModel(); model.SetDefinitions = new[] { set };
            model.BuildEntries = new List<BuildEntry>(build.Entries);
            model.DraftOptions = new[] { new DraftOption(active, true, 2) };
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();
            Assert.IsTrue(view.Build.SetRecipeProgress[0].HasProgress);
            Assert.AreEqual(0, view.Build.SetRecipeProgress[0].FulfilledComponents);
            Assert.AreEqual(1, view.Draft.Options[0].Recipes[0].Projected);
            Assert.IsFalse(view.Draft.Options[0].Recipes[0].CompletesRecipe);
            StringAssert.Contains("required Lv.2", view.Draft.Options[0].Recipes[0].Detail);
            build.Apply(p1); build.Apply(p2); model.BuildEntries = new List<BuildEntry>(build.Entries);
            presenter.RefreshAll();
            Assert.IsTrue(view.Draft.Options[0].Recipes[0].CompletesRecipe);
            Assert.AreEqual(0, view.Build.Sets.Count, "Completing a recipe does not acquire the set.");
            build.Apply(active); model.BuildEntries = new List<BuildEntry>(build.Entries);
            model.DraftOptions = new[] { new DraftOption(active, true, 3) }; presenter.RefreshAll();
            Assert.IsFalse(view.Draft.Options[0].Recipes[0].CompletesRecipe);
            StringAssert.Contains("requirement unchanged", view.Draft.Options[0].Recipes[0].Summary);
            build.Apply(set); model.BuildEntries = new List<BuildEntry>(build.Entries); presenter.RefreshAll();
            Assert.IsTrue(view.Draft.Options[0].Recipes[0].IsAcquired);
        }

        [Test]
        public void SetProgress_CountsOwnedComponentsByPresence_AndNamesMissingOnes()
        {
            var active = new BuildEntryDefinition("FIXTURE-A", BuildEntryKind.ActiveSkill, "Active");
            var owned = new BuildEntryDefinition("FIXTURE-P1", BuildEntryKind.PassiveItem, "Owned passive");
            var missing = new BuildEntryDefinition("FIXTURE-P2", BuildEntryKind.PassiveItem, "Missing passive");
            var set = new SetDefinition("FIXTURE-SET", "Set", new SetRecipeComponent(active.Id, active.Kind, 3),
                new SetRecipeComponent(owned.Id, owned.Kind, 2), new SetRecipeComponent(missing.Id, missing.Kind, 1));
            var build = new PlayerBuild(active);
            build.Apply(owned);
            var model = CreateModel();
            model.SetDefinitions = new[] { set };
            model.BuildEntries = new List<BuildEntry>(build.Entries);
            model.Names[missing.Id] = missing.DisplayName;
            var view = new FakeView();
            using var presenter = new GameplayUiPresenter(model, view);
            presenter.Start();

            var progress = view.Build.SetRecipeProgress[0];
            Assert.AreEqual(0, progress.FulfilledComponents, "No level requirement is met yet.");
            Assert.AreEqual(2, progress.OwnedComponents, "Both owned components count regardless of level.");
            StringAssert.Contains("✓ Active Lv.1 / required Lv.3", progress.Components);
            StringAssert.Contains("✓ Owned passive Lv.1 / required Lv.2", progress.Components);
            StringAssert.Contains("○ Missing passive not owned / required Lv.1", progress.Components);
        }

        private static FakeModel CreateModel()
        {
            var definition = new BuildEntryDefinition("FIXTURE-PASSIVE-UI", BuildEntryKind.PassiveItem, "Fixture Passive");
            var active = new BuildEntryDefinition("FIXTURE-ACTIVE-UI", BuildEntryKind.ActiveSkill, "Fixture Active");
            var build = new PlayerBuild(active);
            var set = new SetDefinition(
                "FIXTURE-SET-UI",
                "Fixture Set",
                new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1),
                new SetRecipeComponent("FIXTURE-UI-R1", BuildEntryKind.PassiveItem, 1),
                new SetRecipeComponent("FIXTURE-UI-R2", BuildEntryKind.PassiveItem, 1));
            build.Apply(new BuildEntryDefinition("FIXTURE-UI-R1", BuildEntryKind.PassiveItem, "R1"));
            build.Apply(new BuildEntryDefinition("FIXTURE-UI-R2", BuildEntryKind.PassiveItem, "R2"));
            build.Apply(set);
            var character = new CharacterDefinition(
                "FIXTURE-CHARACTER-UI",
                "Fixture Character",
                new CharacterBaseStats(100f, 3f),
                active.Id);
            return new FakeModel
            {
                CurrentHealth = 75f,
                MaxHealth = 100f,
                ExperienceProgress01 = 0.4f,
                Level = 3,
                ElapsedSeconds = 125f,
                RunState = RunState.Paused,
                IsDraftOpen = true,
                RemainingRerolls = 2,
                RemainingBanishes = 1,
                DraftOptions = new[] { new DraftOption(definition, false, 1) },
                BuildEntries = new List<BuildEntry>(build.Entries),
                SetDefinitions = new[] { set },
                SelectedCharacter = character,
                UnlockedCharacters = new[] { character },
                DevelopmentCommandsEnabled = true
            };
        }

        private sealed class FakeModel : IGameplayUiModel
        {
            public event Action Changed;
            public RunExperienceSnapshot ExperienceTotals => new RunExperienceSnapshot(10f, 12f, 8f, 4f, 0f, 0f);
            public float CurrentHealth { get; set; }
            public BossViewState Boss { get; set; }
            public float MaxHealth { get; set; }
            public float ExperienceProgress01 { get; set; }
            public int Level { get; set; }
            public float ElapsedSeconds { get; set; }
            public CharacterStatsViewState Stats { get; set; } = new CharacterStatsViewState(new CharacterStats(new CharacterBaseStats(100f, 3f)));
            public RunState RunState { get; set; }
            public int SpeedMultiplier { get; private set; } = 1;
            public bool IsDraftOpen { get; set; }
            public Guid DraftRevision { get; set; } = Guid.NewGuid();
            public DraftRequest CurrentDraftRequest { get; set; }
            public DraftRequest NextDraftRequest { get; set; }
            public int PendingDraftCount { get; set; }
            public long BookCurrency { get; set; }
            public int RemainingRerolls { get; set; }
            public int RemainingBanishes { get; set; }
            public IReadOnlyList<DraftOption> DraftOptions { get; set; }
            public IReadOnlyList<BuildEntry> BuildEntries { get; set; }
            public IReadOnlyList<SetDefinition> SetDefinitions { get; set; }
            public Dictionary<ContentId, string> Names { get; } = new Dictionary<ContentId, string>();
            public string FindBuildEntryName(ContentId id) => Names.TryGetValue(id, out var name) ? name : null;
            public CharacterDefinition SelectedCharacter { get; set; }
            public IReadOnlyList<CharacterDefinition> UnlockedCharacters { get; set; }
            public bool DevelopmentCommandsEnabled { get; set; }
            public string SkillDevelopmentSummary { get; set; } = "Fixture skills";
            public string EnemyDevelopmentSummary { get; set; } = "Fixture enemy";
            public int WavePhaseNumber { get; set; } = 2;
            public int WavePhaseCount { get; set; } = 5;
            public string WavePhaseName { get; set; } = "Pressure";
            public WavePhaseTag WavePhaseTag { get; set; } = WavePhaseTag.Pressure;
            public string WaveDevelopmentSummary { get; set; } = "Fixture wave";
            public int RerollCalls { get; private set; }
            public Guid LastRevision { get; private set; }
            public int BookCalls { get; private set; }
            public ContentId LastBanished { get; private set; }
            public ContentId LastSelected { get; private set; }
            public int PauseCalls { get; private set; }
            public int AddExperienceCalls { get; private set; }
            public int DamageCalls { get; private set; }
            public int HealingCalls { get; private set; }
            public SpritePresentationPreviewMotion LastPreviewMotion { get; private set; }
            public int PresentationResetCalls { get; private set; }

            public bool SelectDraftOption(ContentId id, Guid revision) { LastSelected = id; LastRevision = revision; return true; }
            public bool RerollDraft(Guid revision) { RerollCalls++; return true; }
            public bool BanishDraftOption(ContentId id, Guid revision) { LastBanished = id; return true; }
            public void TogglePause() => PauseCalls++;
            public bool SetSpeed(int multiplier) { SpeedMultiplier = multiplier; Changed?.Invoke(); return true; }
            public void AddFixtureBook() { BookCalls++; }
            public void AddFixtureExperience() => AddExperienceCalls++;
            public void ApplyFixtureDamage() => DamageCalls++;
            public void ApplyFixtureHealing() => HealingCalls++;
            public void PreviewPresentationMotion(SpritePresentationPreviewMotion previewMotion) =>
                LastPreviewMotion = previewMotion;
            public void ResetPresentation() => PresentationResetCalls++;
            public void RaiseChanged() => Changed?.Invoke();
        }

        private sealed class FakeView : IGameplayUiView
        {
            public event Action<ContentId, Guid> DraftOptionSelected;
            public event Action<Guid> DraftRerollRequested;
            public event Action<Guid> DraftBanishModeRequested;
            public event Action PauseRequested;
            public event Action<int> SpeedRequested;
            public event Action AddExperienceRequested;
        public event Action AddBookRequested;
            public event Action ApplyDamageRequested;
            public event Action ApplyHealingRequested;
            public event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
            public event Action PresentationResetRequested;
            public HudViewState Hud { get; private set; }
            public DraftViewState Draft { get; private set; }
            public RunOverlayViewState Overlay { get; private set; }
            public BuildViewState Build { get; private set; }
            public CharacterSelectionViewState Characters { get; private set; }
            public EnemyObservabilityViewState EnemyObservation { get; private set; }
            public WaveObservabilityViewState WaveObservation { get; private set; }
            public SkillObservabilityViewState SkillObservation { get; private set; }
            public bool DevelopmentVisible { get; private set; }

            public void RenderHud(HudViewState state) => Hud = state;
            public void RenderDraft(DraftViewState state) => Draft = state;
            public void RenderRunOverlay(RunOverlayViewState state) => Overlay = state;
            public void RenderBuild(BuildViewState state) => Build = state;
            public void RenderCharacterSelection(CharacterSelectionViewState state) => Characters = state;
            public void RenderSkillObservability(SkillObservabilityViewState state) => SkillObservation = state;
            public void RenderEnemyObservability(EnemyObservabilityViewState state) => EnemyObservation = state;
            public void RenderWaveObservability(WaveObservabilityViewState state) => WaveObservation = state;
            public void SetDevelopmentControlsVisible(bool isVisible) => DevelopmentVisible = isVisible;
            public void RaiseSelect(ContentId id) => DraftOptionSelected?.Invoke(id, Draft.Revision);
            public void RaiseReroll() => DraftRerollRequested?.Invoke(Draft.Revision);
            public void RaiseBanishMode(Guid? revision = null) => DraftBanishModeRequested?.Invoke(revision ?? Draft.Revision);
            public void RaiseBanish(ContentId id) { RaiseBanishMode(); RaiseSelect(id); }
            public void RaisePause() => PauseRequested?.Invoke();
            public void RaiseSpeed(int multiplier) => SpeedRequested?.Invoke(multiplier);
            public void RaiseBook() => AddBookRequested?.Invoke();
            public void RaiseAddExperience() => AddExperienceRequested?.Invoke();
            public void RaiseDamage() => ApplyDamageRequested?.Invoke();
            public void RaiseHealing() => ApplyHealingRequested?.Invoke();
            public void RaisePresentationMotion(SpritePresentationPreviewMotion previewMotion) =>
                PresentationMotionPreviewRequested?.Invoke(previewMotion);
            public void RaisePresentationReset() => PresentationResetRequested?.Invoke();
        }
    }
}
