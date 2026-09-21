using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Game.Character;
using Game.ActiveSkill;
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
        private readonly PlayerActiveSkillSetRuntime _skills;
        private readonly PlayerExperienceRuntime _experience;
        private readonly LevelUpDraftRuntime _draft;
        private readonly RunController _run;
        private readonly SpritePresentationRuntime _presentation;
        private readonly IReadOnlyList<CharacterDefinition> _unlockedCharacters;
        private readonly ContinuousFixtureEnemySpawner _enemySpawner;
        private readonly WaveDirector _waveDirector;
        private readonly List<BuildEntry> _buildEntries = new List<BuildEntry>();

        public event Action Changed;

        public float CurrentHealth => _player.Health.CurrentHealth;
        public float MaxHealth => _player.Health.MaxHealth;
        public float ExperienceProgress01 => _experience.Progression.Progress01;
        public int Level => _experience.Progression.Level;
        public RunExperienceSnapshot ExperienceTotals => _experience.Totals;
        public float ElapsedSeconds => _run.Model.Elapsed;
        public CharacterStatsViewState Stats => new CharacterStatsViewState(_player.Stats, _player.Controls);
        public RunState RunState => _run.Model.State;
        public bool IsDraftOpen => _draft.IsDraftOpen;
        public Guid DraftRevision => _draft.Revision;
        public DraftRequest CurrentDraftRequest => _draft.CurrentRequest;
        public DraftRequest NextDraftRequest => _draft.NextRequest;
        public int PendingDraftCount => _draft.PendingDraftCount;
        public long BookCurrency => _draft.BookCurrency;
        public int RemainingRerolls => _draft.RemainingRerolls;
        public int RemainingBanishes => _draft.RemainingBanishes;
        public IReadOnlyList<DraftOption> DraftOptions => _draft.IsDraftOpen ? _draft.CurrentDraft.Options : NoDraftOptions;
        public IReadOnlyList<BuildEntry> BuildEntries => _buildEntries;
        public IReadOnlyList<SetDefinition> SetDefinitions => _draft.SetDefinitions;
        public CharacterDefinition SelectedCharacter => _draft.Character;
        public IReadOnlyList<CharacterDefinition> UnlockedCharacters => _unlockedCharacters;
        public bool DevelopmentCommandsEnabled { get; }
        public string SkillDevelopmentSummary => _skills != null ? _skills.DevelopmentObservation : "Skills unavailable";
        public string EnemyDevelopmentSummary => _enemySpawner != null
            ? _enemySpawner.DevelopmentObservation
            : "Enemy fixtures unavailable";
        public int WavePhaseNumber => _waveDirector != null ? _waveDirector.CurrentPhaseIndex + 1 : 0;
        public int WavePhaseCount => _waveDirector != null ? _waveDirector.PhaseCount : 0;
        public string WavePhaseName => _waveDirector != null ? _waveDirector.CurrentPhase.DisplayName : "—";
        public WavePhaseTag WavePhaseTag => _waveDirector != null ? _waveDirector.CurrentPhase.Tag : WavePhaseTag.Ordinary;
        public string WaveDevelopmentSummary => _waveDirector != null
            ? DescribeWave(_waveDirector, _enemySpawner.AliveCount)
            : "Wave director unavailable";

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
            _skills = player.GetComponent<PlayerActiveSkillSetRuntime>();
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _run = run ?? throw new ArgumentNullException(nameof(run));
            _presentation = presentation != null ? presentation : throw new ArgumentNullException(nameof(presentation));
            _unlockedCharacters = unlockedCharacters ?? Array.Empty<CharacterDefinition>();
            _enemySpawner = enemySpawner;
            _waveDirector = enemySpawner != null ? enemySpawner.Director : null;
            DevelopmentCommandsEnabled = developmentCommandsEnabled;

            _player.Health.HealthChanged += HandleHealthChanged;
            _experience.Progression.ExperienceChanged += HandleExperienceChanged;
            _experience.Progression.LevelUp += HandleLevelUp;
            _draft.Changed += HandleDraftChanged;
            _draft.SelectionApplied += HandleSelectionApplied;
            _run.Model.StateChanged += HandleRunStateChanged;
            if (_waveDirector != null)
                _waveDirector.PhaseChanged += HandleWavePhaseChanged;
            RefreshBuildEntries();
        }

        public bool SelectDraftOption(ContentId id, Guid revision) => _draft.Select(id, revision);
        public bool RerollDraft(Guid revision) => _draft.Reroll(revision);
        public bool BanishDraftOption(ContentId id, Guid revision) => _draft.Banish(id, revision);
        public void TogglePause() => _run.TogglePause();
        public void AddFixtureExperience() => _experience.AddInterventionExperience(5f);
        public void AddFixtureBook()
        {
            if (DevelopmentCommandsEnabled && _run.Model.State == RunState.Running)
                _draft.RequestBook(Guid.NewGuid(), _run.Model.RunId, new ContentId("FIXTURE-BOOK"));
        }
        public void ApplyFixtureDamage() => _player.TakeDamage(10f);
        public void ApplyFixtureHealing() => _player.Heal(10f);
        public void PreviewPresentationMotion(SpritePresentationPreviewMotion previewMotion) =>
            _presentation.SetPreviewMotion(previewMotion);
        public void ResetPresentation() => _presentation.ResetPresentation();

        private void HandleHealthChanged(float _, float __) => Changed?.Invoke();
        private void HandleExperienceChanged(float _, float __) => Changed?.Invoke();
        private void HandleLevelUp(int _) => Changed?.Invoke();
        private void HandleDraftChanged() => Changed?.Invoke();
        private void HandleSelectionApplied(BuildSelectionResult _)
        {
            RefreshBuildEntries();
            Changed?.Invoke();
        }
        private void HandleRunStateChanged(RunState _) => Changed?.Invoke();
        private void HandleWavePhaseChanged(WavePhaseDefinition _, int __) => Changed?.Invoke();

        private static string DescribeWave(WaveDirector director, int aliveEnemies)
        {
            var phase = director.CurrentPhase;
            var mix = new StringBuilder();
            for (var i = 0; i < phase.Composition.Count; i++)
            {
                if (i > 0)
                    mix.Append(", ");
                mix.Append(phase.Composition[i].Enemy.Id.ToString().Replace("FIXTURE-ENEMY-", string.Empty))
                    .Append(" x").Append(phase.Composition[i].Weight.ToString("0.#", CultureInfo.InvariantCulture));
            }

            var modifiers = phase.Modifiers;
            var summary = new StringBuilder()
                .Append("T ").Append(FormatClock(director.Elapsed))
                .Append(" · ").Append(director.CurrentPhaseIndex + 1).Append('/').Append(director.PhaseCount)
                .Append(' ').Append(phase.DisplayName).Append(" [").Append(phase.Tag).Append("]\n")
                .Append("Spawn every ").Append(phase.SpawnIntervalSeconds.ToString("0.##", CultureInfo.InvariantCulture))
                .Append("s · cap ").Append(phase.MaxAliveEnemies)
                .Append(" · alive ").Append(aliveEnemies).Append('\n')
                .Append("Mix: ").Append(mix).Append('\n')
                .Append("Mods: HP x").Append(modifiers.HealthMultiplier.ToString("0.##", CultureInfo.InvariantCulture))
                .Append(" SPD x").Append(modifiers.SpeedMultiplier.ToString("0.##", CultureInfo.InvariantCulture))
                .Append(" TOUCH x").Append(modifiers.ContactDamageMultiplier.ToString("0.##", CultureInfo.InvariantCulture))
                .Append(" SHOT x").Append(modifiers.AttackDamageMultiplier.ToString("0.##", CultureInfo.InvariantCulture))
                .Append('\n');
            var next = director.NextHook;
            summary.Append("Next hook: ")
                .Append(next != null ? $"{next.Kind} @ {FormatClock(next.TimeSeconds)}" : "none");
            return summary.ToString();
        }

        private static string FormatClock(float seconds)
        {
            var whole = Math.Max(0, (int)seconds);
            return $"{whole / 60:00}:{whole % 60:00}";
        }

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
            _draft.Changed -= HandleDraftChanged;
            _draft.SelectionApplied -= HandleSelectionApplied;
            _run.Model.StateChanged -= HandleRunStateChanged;
            if (_waveDirector != null)
                _waveDirector.PhaseChanged -= HandleWavePhaseChanged;
        }
    }
}
