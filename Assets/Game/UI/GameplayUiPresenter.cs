using System;
using System.Collections.Generic;
using Game.Progression;
using Game.Run;

namespace Game.UI
{
    public sealed class GameplayUiPresenter : IDisposable
    {
        private readonly IGameplayUiModel _model;
        private readonly IGameplayUiView _view;
        private bool _started;

        public GameplayUiPresenter(IGameplayUiModel model, IGameplayUiView view)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Start()
        {
            if (_started)
                throw new InvalidOperationException("Gameplay UI presenter is already started.");

            _model.Changed += RefreshAll;
            _view.DraftOptionSelected += HandleDraftOptionSelected;
            _view.DraftRerollRequested += HandleDraftRerollRequested;
            _view.DraftBanishRequested += HandleDraftBanishRequested;
            _view.PauseRequested += HandlePauseRequested;
            _view.AddExperienceRequested += HandleAddExperienceRequested;
            _view.ApplyDamageRequested += HandleApplyDamageRequested;
            _view.ApplyHealingRequested += HandleApplyHealingRequested;
            _started = true;
            _view.SetDevelopmentControlsVisible(_model.DevelopmentCommandsEnabled);
            RefreshAll();
        }

        public void RefreshHud()
        {
            _view.RenderHud(new HudViewState(
                _model.CurrentHealth,
                _model.MaxHealth,
                _model.ExperienceProgress01,
                _model.Level,
                _model.RemainingSeconds));
        }

        public void RefreshAll()
        {
            RefreshHud();
            _view.RenderDraft(BuildDraftState());
            _view.RenderBuild(BuildBuildState());
            _view.RenderRunOverlay(BuildRunOverlayState());
        }

        private BuildViewState BuildBuildState()
        {
            var active = new List<BuildSlotViewState>(PlayerBuild.ActiveSlotCapacity);
            var passive = new List<BuildSlotViewState>(PlayerBuild.PassiveSlotCapacity);
            foreach (var entry in _model.BuildEntries)
            {
                var slot = new BuildSlotViewState(entry.Definition.DisplayName, entry.Level, true);
                if (entry.Definition.Kind == BuildEntryKind.ActiveSkill)
                    active.Add(slot);
                else
                    passive.Add(slot);
            }

            FillEmptySlots(active, PlayerBuild.ActiveSlotCapacity);
            FillEmptySlots(passive, PlayerBuild.PassiveSlotCapacity);
            return new BuildViewState(active, passive);
        }

        private static void FillEmptySlots(List<BuildSlotViewState> slots, int capacity)
        {
            while (slots.Count < capacity)
                slots.Add(new BuildSlotViewState("Empty", 0, false));
        }

        private DraftViewState BuildDraftState()
        {
            if (!_model.IsDraftOpen)
                return new DraftViewState(false, _model.RemainingRerolls, _model.RemainingBanishes, Array.Empty<DraftOptionViewState>());

            var source = _model.DraftOptions;
            var options = new DraftOptionViewState[source.Count];
            for (var i = 0; i < source.Count; i++)
            {
                var option = source[i];
                var type = option.Definition.Kind == BuildEntryKind.ActiveSkill ? "Active" : "Passive";
                var detail = option.IsUpgrade ? $"{type} · level {option.ResultingLevel}" : $"{type} · new";
                options[i] = new DraftOptionViewState(option.Definition.Id, option.Definition.DisplayName, detail);
            }

            return new DraftViewState(true, _model.RemainingRerolls, _model.RemainingBanishes, options);
        }

        private RunOverlayViewState BuildRunOverlayState()
        {
            if (_model.RunState == RunState.Won)
                return new RunOverlayViewState(true, "RUN COMPLETE", false);
            if (_model.RunState == RunState.Lost)
                return new RunOverlayViewState(true, "RUN FAILED", false);
            if (_model.RunState == RunState.Paused && !_model.IsDraftOpen)
                return new RunOverlayViewState(true, "PAUSED", true);
            return new RunOverlayViewState(false, string.Empty, false);
        }

        private void HandleDraftOptionSelected(Game.Content.ContentId id)
        {
            _model.SelectDraftOption(id);
            RefreshAll();
        }

        private void HandleDraftRerollRequested()
        {
            _model.RerollDraft();
            RefreshAll();
        }

        private void HandleDraftBanishRequested(Game.Content.ContentId id)
        {
            _model.BanishDraftOption(id);
            RefreshAll();
        }

        private void HandlePauseRequested()
        {
            _model.TogglePause();
            RefreshAll();
        }

        private void HandleAddExperienceRequested()
        {
            if (_model.DevelopmentCommandsEnabled)
                _model.AddFixtureExperience();
        }

        private void HandleApplyDamageRequested()
        {
            if (_model.DevelopmentCommandsEnabled)
                _model.ApplyFixtureDamage();
        }

        private void HandleApplyHealingRequested()
        {
            if (_model.DevelopmentCommandsEnabled)
                _model.ApplyFixtureHealing();
        }

        public void Dispose()
        {
            if (!_started)
                return;
            _model.Changed -= RefreshAll;
            _view.DraftOptionSelected -= HandleDraftOptionSelected;
            _view.DraftRerollRequested -= HandleDraftRerollRequested;
            _view.DraftBanishRequested -= HandleDraftBanishRequested;
            _view.PauseRequested -= HandlePauseRequested;
            _view.AddExperienceRequested -= HandleAddExperienceRequested;
            _view.ApplyDamageRequested -= HandleApplyDamageRequested;
            _view.ApplyHealingRequested -= HandleApplyHealingRequested;
            _started = false;
        }
    }
}
