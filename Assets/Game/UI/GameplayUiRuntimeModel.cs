using System;
using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.Run;

namespace Game.UI
{
    public sealed class GameplayUiRuntimeModel : IGameplayUiModel, IDisposable
    {
        private static readonly IReadOnlyList<DraftOption> NoDraftOptions = Array.Empty<DraftOption>();

        private readonly PlayerCharacterRuntime _player;
        private readonly PlayerExperienceRuntime _experience;
        private readonly LevelUpDraftRuntime _draft;
        private readonly RunController _run;
        private readonly SpritePresentationRuntime _presentation;
        private readonly IReadOnlyList<CharacterDefinition> _unlockedCharacters;
        private readonly ContinuousFixtureEnemySpawner _enemySpawner;
        private readonly List<BuildEntry> _buildEntries = new List<BuildEntry>();

        public event Action Changed;

        public float CurrentHealth => _player.Health.CurrentHealth;
        public float MaxHealth => _player.Health.MaxHealth;
        public float ExperienceProgress01 => _experience.Progression.Progress01;
        public int Level => _experience.Progression.Level;
        public float RemainingSeconds => Math.Max(0f, _run.Model.Duration - _run.Model.Elapsed);
        public RunState RunState => _run.Model.State;
        public bool IsDraftOpen => _draft.IsDraftOpen;
        public int RemainingRerolls => _draft.RemainingRerolls;
        public int RemainingBanishes => _draft.RemainingBanishes;
        public IReadOnlyList<DraftOption> DraftOptions => _draft.IsDraftOpen ? _draft.CurrentDraft.Options : NoDraftOptions;
        public IReadOnlyList<BuildEntry> BuildEntries => _buildEntries;
        public IReadOnlyList<SetDefinition> SetDefinitions => _draft.SetDefinitions;
        public CharacterDefinition SelectedCharacter => _draft.Character;
        public IReadOnlyList<CharacterDefinition> UnlockedCharacters => _unlockedCharacters;
        public bool DevelopmentCommandsEnabled { get; }
        public string EnemyDevelopmentSummary => _enemySpawner != null
            ? _enemySpawner.DevelopmentObservation
            : "Enemy fixtures unavailable";

        public GameplayUiRuntimeModel(
            PlayerCharacterRuntime player,
            PlayerExperienceRuntime experience,
            LevelUpDraftRuntime draft,
            RunController run,
            SpritePresentationRuntime presentation,
            bool developmentCommandsEnabled,
            IReadOnlyList<CharacterDefinition> unlockedCharacters = null,
            ContinuousFixtureEnemySpawner enemySpawner = null)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _presentation = presentation != null ? presentation : throw new ArgumentNullException(nameof(presentation));
            _unlockedCharacters = unlockedCharacters ?? Array.Empty<CharacterDefinition>();
            _enemySpawner = enemySpawner;
            DevelopmentCommandsEnabled = developmentCommandsEnabled;

            _player.Health.HealthChanged += HandleHealthChanged;
            _experience.Progression.ExperienceChanged += HandleExperienceChanged;
            _experience.Progression.LevelUp += HandleLevelUp;
            _draft.DraftOpened += HandleDraftOpened;
            _draft.SelectionApplied += HandleSelectionApplied;
            _run.Model.StateChanged += HandleRunStateChanged;
            RefreshBuildEntries();
        }

        public bool SelectDraftOption(ContentId id) => _draft.Select(id);
        public bool RerollDraft() => _draft.Reroll();
        public bool BanishDraftOption(ContentId id) => _draft.Banish(id);
        public void TogglePause() => _run.TogglePause();
        public void AddFixtureExperience() => _experience.AddPickedUpExperience(5f);
        public void ApplyFixtureDamage() => _player.TakeDamage(10f);
        public void ApplyFixtureHealing() => _player.Heal(10f);
        public void PreviewPresentationMotion(SpritePresentationPreviewMotion previewMotion) =>
            _presentation.SetPreviewMotion(previewMotion);
        public void ResetPresentation() => _presentation.ResetPresentation();

        private void HandleHealthChanged(float _, float __) => Changed?.Invoke();
        private void HandleExperienceChanged(float _, float __) => Changed?.Invoke();
        private void HandleLevelUp(int _) => Changed?.Invoke();
        private void HandleDraftOpened(IReadOnlyList<DraftOption> _) => Changed?.Invoke();
        private void HandleSelectionApplied(BuildSelectionResult _)
        {
            RefreshBuildEntries();
            Changed?.Invoke();
        }
        private void HandleRunStateChanged(RunState _) => Changed?.Invoke();

        private void RefreshBuildEntries()
        {
            _buildEntries.Clear();
            foreach (var entry in _draft.Build.Entries)
                _buildEntries.Add(entry);
            _buildEntries.Sort((left, right) => string.CompareOrdinal(
                left.Definition.Id.ToString(),
                right.Definition.Id.ToString()));
        }

        public void Dispose()
        {
            _player.Health.HealthChanged -= HandleHealthChanged;
            _experience.Progression.ExperienceChanged -= HandleExperienceChanged;
            _experience.Progression.LevelUp -= HandleLevelUp;
            _draft.DraftOpened -= HandleDraftOpened;
            _draft.SelectionApplied -= HandleSelectionApplied;
            _run.Model.StateChanged -= HandleRunStateChanged;
        }
    }
}
