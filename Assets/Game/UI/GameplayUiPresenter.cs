using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Presentation;
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
            _view.AddBookRequested += HandleAddBookRequested;
            _view.ApplyDamageRequested += HandleApplyDamageRequested;
            _view.ApplyHealingRequested += HandleApplyHealingRequested;
            _view.PresentationMotionPreviewRequested += HandlePresentationMotionPreviewRequested;
            _view.PresentationResetRequested += HandlePresentationResetRequested;
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
                _model.ElapsedSeconds,
                new WaveViewState(
                    _model.WavePhaseNumber,
                    _model.WavePhaseCount,
                    _model.WavePhaseName,
                    _model.WavePhaseTag),
                _model.DevelopmentCommandsEnabled ? _model.Stats : null,
                _model.DevelopmentCommandsEnabled ? _model.ExperienceTotals : null, _model.BookCurrency));
            // The summaries allocate (string building) and only feed the development
            // panel, which is not shown outside development builds — skip the work there.
            if (!_model.DevelopmentCommandsEnabled)
                return;

            _view.RenderSkillObservability(new SkillObservabilityViewState(_model.SkillDevelopmentSummary));
            _view.RenderEnemyObservability(new EnemyObservabilityViewState(_model.EnemyDevelopmentSummary));
            _view.RenderWaveObservability(new WaveObservabilityViewState(_model.WaveDevelopmentSummary));
        }

        public void RefreshAll()
        {
            RefreshHud();
            _view.RenderDraft(BuildDraftState());
            _view.RenderBuild(BuildBuildState());
            _view.RenderCharacterSelection(BuildCharacterSelectionState());
            _view.RenderRunOverlay(BuildRunOverlayState());
        }

        private CharacterSelectionViewState BuildCharacterSelectionState()
        {
            var characters = new CharacterOptionViewState[_model.UnlockedCharacters.Count];
            for (var i = 0; i < characters.Length; i++)
            {
                var character = _model.UnlockedCharacters[i];
                var stats = character.BaseStats;
                characters[i] = new CharacterOptionViewState(
                    character.Id,
                    character.DisplayName,
                    character.StartingActiveSkill.Id.ToString(),
                    stats.MaxHealth,
                    stats.MovementSpeed,
                    stats.ActiveSkillDamageMultiplier,
                    stats.ActiveSkillCooldownMultiplier,
                    stats.DisappearingXpRecovery,
                    _model.SelectedCharacter != null && _model.SelectedCharacter.Id == character.Id);
            }
            return new CharacterSelectionViewState(characters);
        }

        private BuildViewState BuildBuildState()
        {
            var active = new List<BuildSlotViewState>(PlayerBuild.ActiveSlotCapacity);
            var passive = new List<BuildSlotViewState>(PlayerBuild.PassiveSlotCapacity);
            var sets = new List<SetBuildViewState>();
            foreach (var entry in _model.BuildEntries)
            {
                if (entry.Definition.Kind == BuildEntryKind.Set)
                {
                    sets.Add(new SetBuildViewState(entry.Definition.DisplayName));
                    continue;
                }
                var slot = new BuildSlotViewState(entry.Definition.DisplayName, entry.Level, true);
                if (entry.Definition.Kind == BuildEntryKind.ActiveSkill)
                    active.Add(slot);
                else
                    passive.Add(slot);
            }

            FillEmptySlots(active, PlayerBuild.ActiveSlotCapacity);
            FillEmptySlots(passive, PlayerBuild.PassiveSlotCapacity);

            var progress = new List<SetRecipeProgressViewState>(_model.SetDefinitions.Count);
            for (var i = 0; i < _model.SetDefinitions.Count; i++)
            {
                var definition = _model.SetDefinitions[i];
                var isAcquired = false;
                for (var entryIndex = 0; entryIndex < _model.BuildEntries.Count; entryIndex++)
                {
                    if (_model.BuildEntries[entryIndex].Definition.Id == definition.Id)
                    {
                        isAcquired = true;
                        break;
                    }
                }
                var fulfilled = CountFulfilledComponents(definition);
                progress.Add(new SetRecipeProgressViewState(
                    definition.DisplayName,
                    fulfilled,
                    definition.Recipe.Count,
                    fulfilled == definition.Recipe.Count && !isAcquired,
                    isAcquired));
            }

            return new BuildViewState(active, passive, sets, progress);
        }

        private int CountFulfilledComponents(SetDefinition definition)
        {
            var fulfilled = 0;
            for (var i = 0; i < definition.Recipe.Count; i++)
            {
                var component = definition.Recipe[i];
                for (var entryIndex = 0; entryIndex < _model.BuildEntries.Count; entryIndex++)
                {
                    var entry = _model.BuildEntries[entryIndex];
                    if (entry.Definition.Id == component.Id &&
                        entry.Definition.Kind == component.Kind &&
                        entry.Level >= component.MinimumLevel)
                    {
                        fulfilled++;
                        break;
                    }
                }
            }
            return fulfilled;
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
            var options = new DraftOptionViewState[3];
            for (var i = 0; i < source.Count; i++)
            {
                var option = source[i];
                var type = option.Definition.Kind switch
                {
                    BuildEntryKind.ActiveSkill => "Active",
                    BuildEntryKind.PassiveItem => "Passive",
                    BuildEntryKind.Set => "Set",
                    _ => "Unknown"
                };
                var detail = new StringBuilder(option.IsUpgrade
                    ? $"{type} · level {option.Preview.CurrentLevel} → {option.Preview.NextLevel}"
                    : $"{type} · new");
                foreach (var value in option.Preview.Values)
                    detail.Append("\n").Append(value.Label).Append(": ")
                        .Append(value.Current.ToString("0.##", CultureInfo.InvariantCulture)).Append(value.Unit)
                        .Append(" → ").Append(value.Next.ToString("0.##", CultureInfo.InvariantCulture)).Append(value.Unit);
                options[i] = new DraftOptionViewState(option.Definition.Id, option.Definition.DisplayName, detail.ToString());
            }

            for (var i = source.Count; i < options.Length; i++)
                options[i] = new DraftOptionViewState(default, "No available option", "", false);
            var request = _model.CurrentDraftRequest;
            var heading = request?.Origin == DraftOrigin.Book ? "TRAVELER BOOK" :
                request?.EarnedLevel != null ? $"LEVEL UP · {request.EarnedLevel}" : "LEVEL UP";
            var next = _model.NextDraftRequest;
            var queue = next == null ? "" : $"Next: {(next.Origin == DraftOrigin.Book ? "Traveler Book" : $"Level {next.EarnedLevel}")} · {_model.PendingDraftCount - 1} queued";
            return new DraftViewState(true, _model.RemainingRerolls, _model.RemainingBanishes,
                options, _model.DraftRevision, heading, queue);
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

        private void HandleDraftOptionSelected(Game.Content.ContentId id, Guid revision)
        {
            _model.SelectDraftOption(id, revision);
            RefreshAll();
        }

        private void HandleDraftRerollRequested(Guid revision)
        {
            _model.RerollDraft(revision);
            RefreshAll();
        }

        private void HandleDraftBanishRequested(Game.Content.ContentId id, Guid revision)
        {
            _model.BanishDraftOption(id, revision);
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

        private void HandleAddBookRequested()
        {
            if (_model.DevelopmentCommandsEnabled) _model.AddFixtureBook();
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

        private void HandlePresentationMotionPreviewRequested(SpritePresentationPreviewMotion previewMotion)
        {
            if (_model.DevelopmentCommandsEnabled)
                _model.PreviewPresentationMotion(previewMotion);
        }

        private void HandlePresentationResetRequested()
        {
            if (_model.DevelopmentCommandsEnabled)
                _model.ResetPresentation();
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
            _view.AddBookRequested -= HandleAddBookRequested;
            _view.ApplyDamageRequested -= HandleApplyDamageRequested;
            _view.ApplyHealingRequested -= HandleApplyHealingRequested;
            _view.PresentationMotionPreviewRequested -= HandlePresentationMotionPreviewRequested;
            _view.PresentationResetRequested -= HandlePresentationResetRequested;
            _started = false;
        }
    }
}
