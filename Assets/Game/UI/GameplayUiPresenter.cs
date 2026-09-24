using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Presentation;
using Game.Progression;
using Game.Run;
using Game.ActiveSkill;
using Game.Content;
using UnityEngine;

namespace Game.UI
{
    public sealed class GameplayUiPresenter : IDisposable
    {
        private readonly IGameplayUiModel _model;
        private readonly IGameplayUiView _view;
        private readonly ContentRegistry _registry;
        private bool _started;
        private Guid _banishRevision;

        public GameplayUiPresenter(IGameplayUiModel model, IGameplayUiView view, ContentRegistry registry = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _registry = registry;
        }

        public void Start()
        {
            if (_started)
                throw new InvalidOperationException("Gameplay UI presenter is already started.");

            _model.Changed += RefreshAll;
            _view.DraftOptionSelected += HandleDraftOptionSelected;
            _view.DraftRerollRequested += HandleDraftRerollRequested;
            _view.DraftBanishModeRequested += HandleDraftBanishModeRequested;
            _view.PauseRequested += HandlePauseRequested;
            _view.SpeedRequested += HandleSpeedRequested;
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
                _model.Stats,
                _model.DevelopmentCommandsEnabled ? _model.ExperienceTotals : null, _model.BookCurrency, _model.Boss,
                _model.SpeedMultiplier, _model.RunState == RunState.Running));
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
                    sets.Add(new SetBuildViewState(entry.Definition.DisplayName,
                        (entry.Definition as SetDefinition)?.Description, ResolveIcon(entry.Definition)));
                    continue;
                }
                var slot = new BuildSlotViewState(entry.Definition.DisplayName, entry.Level, true,
                    BuildPassiveDetail(entry), ResolveIcon(entry.Definition));
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
                    isAcquired, string.Join("\n", ComponentDetails(definition, null)), HasPossession(definition)));
            }

            return new BuildViewState(active, passive, sets, progress);
        }

        private string BuildPassiveDetail(BuildEntry entry)
        {
            var definition = entry.Definition;
            var detail = new StringBuilder();
            foreach (var value in definition.CreateDraftPreview(0, entry.Level).Values)
            {
                if (detail.Length > 0) detail.Append("\n");
                detail.Append(value.Label).Append(": ")
                    .Append(value.Next.ToString("0.##", CultureInfo.InvariantCulture)).Append(value.Unit);
            }
            if (definition is PassiveProgressionDefinition passive && passive.GetLevel(entry.Level).LowHealthDamageMaxBonus > 0f && _model.Stats != null)
                detail.Append("\nCurrent low-HP damage: x")
                    .Append(_model.Stats.LowHealthDamageMultiplier.ToString("0.##", CultureInfo.InvariantCulture));
            return detail.ToString();
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

        private bool IsAcquired(SetDefinition set)
        {
            foreach (var entry in _model.BuildEntries) if (entry.Definition.Id == set.Id) return true;
            return false;
        }

        private int ComponentLevel(SetRecipeComponent component)
        {
            foreach (var entry in _model.BuildEntries)
                if (entry.Definition.Id == component.Id && entry.Definition.Kind == component.Kind) return entry.Level;
            return 0;
        }

        private bool HasPossession(SetDefinition set)
        {
            foreach (var component in set.Recipe) if (ComponentLevel(component) > 0) return true;
            return false;
        }

        private List<string> ComponentDetails(SetDefinition set, DraftOption? option)
        {
            var details = new List<string>();
            foreach (var component in set.Recipe)
            {
                var current = ComponentLevel(component);
                var selected = option.HasValue && option.Value.Definition.Id == component.Id;
                var projected = selected ? option.Value.Preview.NextLevel : current;
                var name = component.Id.ToString();
                foreach (var entry in _model.BuildEntries) if (entry.Definition.Id == component.Id) name = entry.Definition.DisplayName;
                if (selected) name = option.Value.Definition.DisplayName;
                var mark = current >= component.MinimumLevel ? "✓" : current > 0 ? "◐" : "○";
                details.Add($"{mark} {name} Lv.{current}" + (selected ? $" → {projected} [THIS OPTION]" : "") +
                    $" / required Lv.{component.MinimumLevel}");
            }
            return details;
        }

        private IReadOnlyList<RecipeProjectionViewState> ProjectRecipes(DraftOption option)
        {
            var related = new List<SetDefinition>();
            foreach (var set in _model.SetDefinitions)
            {
                var includes = set.Id == option.Definition.Id;
                foreach (var component in set.Recipe) includes |= component.Id == option.Definition.Id;
                if (includes) related.Add(set);
            }
            // Closest projected completion first; acquired recipes last; ordinal ID breaks ties.
            related.Sort((a, b) =>
            {
                var acquired = IsAcquired(a).CompareTo(IsAcquired(b));
                if (acquired != 0) return acquired;
                var remaining = (a.Recipe.Count - ProjectedCount(a, option)).CompareTo(b.Recipe.Count - ProjectedCount(b, option));
                return remaining != 0 ? remaining : string.CompareOrdinal(a.Id.ToString(), b.Id.ToString());
            });
            var result = new List<RecipeProjectionViewState>();
            foreach (var set in related)
            {
                var current = CountFulfilledComponents(set);
                var projected = ProjectedCount(set, option);
                result.Add(new RecipeProjectionViewState(set.DisplayName, current, projected, set.Recipe.Count,
                    !IsAcquired(set) && current < set.Recipe.Count && projected == set.Recipe.Count,
                    IsAcquired(set), ComponentDetails(set, option)));
            }
            return result.AsReadOnly();
        }

        private int ProjectedCount(SetDefinition set, DraftOption option)
        {
            var count = 0;
            foreach (var component in set.Recipe)
                if ((component.Id == option.Definition.Id ? option.Preview.NextLevel : ComponentLevel(component)) >= component.MinimumLevel) count++;
            return count;
        }

        private static void FillEmptySlots(List<BuildSlotViewState> slots, int capacity)
        {
            while (slots.Count < capacity)
                slots.Add(new BuildSlotViewState("Empty", 0, false));
        }

        private Sprite ResolveIcon(BuildEntryDefinition definition)
        {
            if (_registry == null)
                return null;

            var reference = definition switch
            {
                ActiveSkillProgressionDefinition activeSkill => activeSkill.Icon,
                PassiveProgressionDefinition passive => passive.Icon,
                SetDefinition set => set.Icon,
                _ => default
            };
            if (!reference.Id.IsValid || !reference.TryResolve(_registry, out var icon)) return null;

            icon.RequireRole(SpriteRole.Icon);
            return icon.Sprite;
        }

        private DraftViewState BuildDraftState()
        {
            if (!_model.IsDraftOpen || _model.DraftRevision != _banishRevision || _model.RemainingBanishes == 0)
                _banishRevision = Guid.Empty;
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
                if (option.Definition is SetDefinition set) detail.Append("\n").Append(set.Description);
                options[i] = new DraftOptionViewState(option.Definition.Id, option.Definition.DisplayName, detail.ToString(),
                    icon: ResolveIcon(option.Definition), isSet: option.Definition.Kind == BuildEntryKind.Set,
                    recipes: ProjectRecipes(option));
            }

            for (var i = source.Count; i < options.Length; i++)
                options[i] = new DraftOptionViewState(default, "No available option", "", false);
            var request = _model.CurrentDraftRequest;
            var heading = request?.Origin == DraftOrigin.Book ? "TRAVELER BOOK" :
                request?.EarnedLevel != null ? $"LEVEL UP · {request.EarnedLevel}" : "LEVEL UP";
            var next = _model.NextDraftRequest;
            var queue = next == null ? "" : $"Next: {(next.Origin == DraftOrigin.Book ? "Traveler Book" : $"Level {next.EarnedLevel}")} · {_model.PendingDraftCount - 1} queued";
            return new DraftViewState(true, _model.RemainingRerolls, _model.RemainingBanishes,
                options, _model.DraftRevision, heading, queue, _banishRevision != Guid.Empty);
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
            if (!_model.IsDraftOpen || revision != _model.DraftRevision) return;
            if (_banishRevision == revision)
            {
                if (_model.BanishDraftOption(id, revision)) _banishRevision = Guid.Empty;
            }
            else _model.SelectDraftOption(id, revision);
            RefreshAll();
        }

        private void HandleDraftRerollRequested(Guid revision)
        {
            if (_banishRevision != Guid.Empty || !_model.IsDraftOpen || revision != _model.DraftRevision) return;
            _model.RerollDraft(revision);
            RefreshAll();
        }

        private void HandleDraftBanishModeRequested(Guid revision)
        {
            if (!_model.IsDraftOpen || revision == Guid.Empty || revision != _model.DraftRevision || _model.RemainingBanishes <= 0) return;
            _banishRevision = _banishRevision == revision ? Guid.Empty : revision;
            RefreshAll();
        }

        private void HandlePauseRequested()
        {
            _model.TogglePause();
            RefreshAll();
        }

        private void HandleSpeedRequested(int multiplier)
        {
            if (_model.RunState != RunState.Running) return;
            if (_model.SetSpeed(multiplier)) RefreshHud();
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
            _view.DraftBanishModeRequested -= HandleDraftBanishModeRequested;
            _view.PauseRequested -= HandlePauseRequested;
            _view.SpeedRequested -= HandleSpeedRequested;
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
