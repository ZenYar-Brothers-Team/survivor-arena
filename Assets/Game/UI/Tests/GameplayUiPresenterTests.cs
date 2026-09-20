using System;
using System.Collections.Generic;
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
                Assert.IsTrue(view.Draft.IsVisible);
                Assert.AreEqual(1, view.Draft.Options.Count);
                Assert.AreEqual("Fixture Passive", view.Draft.Options[0].Title);
                Assert.AreEqual(2, view.Draft.RemainingRerolls);
                Assert.AreEqual(PlayerBuild.ActiveSlotCapacity, view.Build.ActiveSlots.Count);
                Assert.AreEqual(PlayerBuild.PassiveSlotCapacity, view.Build.PassiveSlots.Count);
                Assert.AreEqual("Fixture Active", view.Build.ActiveSlots[0].Title);
                Assert.AreEqual(1, view.Build.ActiveSlots[0].Level);
                Assert.IsFalse(view.Build.PassiveSlots[0].IsOccupied);
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

        private static FakeModel CreateModel()
        {
            var definition = new BuildEntryDefinition("FIXTURE-PASSIVE-UI", BuildEntryKind.PassiveItem, "Fixture Passive");
            var active = new BuildEntryDefinition("FIXTURE-ACTIVE-UI", BuildEntryKind.ActiveSkill, "Fixture Active");
            var build = new PlayerBuild(active);
            var set = new SetDefinition(
                "FIXTURE-SET-UI",
                "Fixture Set",
                1f,
                new SetRecipeComponent(active.Id, BuildEntryKind.ActiveSkill, 1));
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
                RemainingSeconds = 125f,
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
            public float CurrentHealth { get; set; }
            public float MaxHealth { get; set; }
            public float ExperienceProgress01 { get; set; }
            public int Level { get; set; }
            public float RemainingSeconds { get; set; }
            public RunState RunState { get; set; }
            public bool IsDraftOpen { get; set; }
            public int RemainingRerolls { get; set; }
            public int RemainingBanishes { get; set; }
            public IReadOnlyList<DraftOption> DraftOptions { get; set; }
            public IReadOnlyList<BuildEntry> BuildEntries { get; set; }
            public IReadOnlyList<SetDefinition> SetDefinitions { get; set; }
            public CharacterDefinition SelectedCharacter { get; set; }
            public IReadOnlyList<CharacterDefinition> UnlockedCharacters { get; set; }
            public bool DevelopmentCommandsEnabled { get; set; }
            public string EnemyDevelopmentSummary { get; set; } = "Fixture enemy";
            public int WavePhaseNumber { get; set; } = 2;
            public int WavePhaseCount { get; set; } = 5;
            public string WavePhaseName { get; set; } = "Pressure";
            public WavePhaseTag WavePhaseTag { get; set; } = WavePhaseTag.Pressure;
            public string WaveDevelopmentSummary { get; set; } = "Fixture wave";
            public int RerollCalls { get; private set; }
            public ContentId LastBanished { get; private set; }
            public ContentId LastSelected { get; private set; }
            public int PauseCalls { get; private set; }
            public int AddExperienceCalls { get; private set; }
            public int DamageCalls { get; private set; }
            public int HealingCalls { get; private set; }
            public SpritePresentationPreviewMotion LastPreviewMotion { get; private set; }
            public int PresentationResetCalls { get; private set; }

            public bool SelectDraftOption(ContentId id) { LastSelected = id; return true; }
            public bool RerollDraft() { RerollCalls++; return true; }
            public bool BanishDraftOption(ContentId id) { LastBanished = id; return true; }
            public void TogglePause() => PauseCalls++;
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
            public event Action<ContentId> DraftOptionSelected;
            public event Action DraftRerollRequested;
            public event Action<ContentId> DraftBanishRequested;
            public event Action PauseRequested;
            public event Action AddExperienceRequested;
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
            public bool DevelopmentVisible { get; private set; }

            public void RenderHud(HudViewState state) => Hud = state;
            public void RenderDraft(DraftViewState state) => Draft = state;
            public void RenderRunOverlay(RunOverlayViewState state) => Overlay = state;
            public void RenderBuild(BuildViewState state) => Build = state;
            public void RenderCharacterSelection(CharacterSelectionViewState state) => Characters = state;
            public void RenderEnemyObservability(EnemyObservabilityViewState state) => EnemyObservation = state;
            public void RenderWaveObservability(WaveObservabilityViewState state) => WaveObservation = state;
            public void SetDevelopmentControlsVisible(bool isVisible) => DevelopmentVisible = isVisible;
            public void RaiseSelect(ContentId id) => DraftOptionSelected?.Invoke(id);
            public void RaiseReroll() => DraftRerollRequested?.Invoke();
            public void RaiseBanish(ContentId id) => DraftBanishRequested?.Invoke(id);
            public void RaisePause() => PauseRequested?.Invoke();
            public void RaiseAddExperience() => AddExperienceRequested?.Invoke();
            public void RaiseDamage() => ApplyDamageRequested?.Invoke();
            public void RaiseHealing() => ApplyHealingRequested?.Invoke();
            public void RaisePresentationMotion(SpritePresentationPreviewMotion previewMotion) =>
                PresentationMotionPreviewRequested?.Invoke(previewMotion);
            public void RaisePresentationReset() => PresentationResetRequested?.Invoke();
        }
    }
}
