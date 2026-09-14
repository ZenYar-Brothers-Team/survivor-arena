using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
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
                Assert.IsFalse(view.Overlay.IsVisible, "Draft pause must not show the generic pause overlay.");
                Assert.IsTrue(view.DevelopmentVisible);
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

                Assert.AreEqual(1, model.RerollCalls);
                Assert.AreEqual(id, model.LastBanished);
                Assert.AreEqual(id, model.LastSelected);
                Assert.AreEqual(1, model.PauseCalls);
                Assert.AreEqual(1, model.AddExperienceCalls);
                Assert.AreEqual(1, model.DamageCalls);
                Assert.AreEqual(1, model.HealingCalls);
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
            public bool DevelopmentCommandsEnabled { get; set; }
            public int RerollCalls { get; private set; }
            public ContentId LastBanished { get; private set; }
            public ContentId LastSelected { get; private set; }
            public int PauseCalls { get; private set; }
            public int AddExperienceCalls { get; private set; }
            public int DamageCalls { get; private set; }
            public int HealingCalls { get; private set; }

            public bool SelectDraftOption(ContentId id) { LastSelected = id; return true; }
            public bool RerollDraft() { RerollCalls++; return true; }
            public bool BanishDraftOption(ContentId id) { LastBanished = id; return true; }
            public void TogglePause() => PauseCalls++;
            public void AddFixtureExperience() => AddExperienceCalls++;
            public void ApplyFixtureDamage() => DamageCalls++;
            public void ApplyFixtureHealing() => HealingCalls++;
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
            public HudViewState Hud { get; private set; }
            public DraftViewState Draft { get; private set; }
            public RunOverlayViewState Overlay { get; private set; }
            public BuildViewState Build { get; private set; }
            public bool DevelopmentVisible { get; private set; }

            public void RenderHud(HudViewState state) => Hud = state;
            public void RenderDraft(DraftViewState state) => Draft = state;
            public void RenderRunOverlay(RunOverlayViewState state) => Overlay = state;
            public void RenderBuild(BuildViewState state) => Build = state;
            public void SetDevelopmentControlsVisible(bool isVisible) => DevelopmentVisible = isVisible;
            public void RaiseSelect(ContentId id) => DraftOptionSelected?.Invoke(id);
            public void RaiseReroll() => DraftRerollRequested?.Invoke();
            public void RaiseBanish(ContentId id) => DraftBanishRequested?.Invoke(id);
            public void RaisePause() => PauseRequested?.Invoke();
            public void RaiseAddExperience() => AddExperienceRequested?.Invoke();
            public void RaiseDamage() => ApplyDamageRequested?.Invoke();
            public void RaiseHealing() => ApplyHealingRequested?.Invoke();
        }
    }
}
