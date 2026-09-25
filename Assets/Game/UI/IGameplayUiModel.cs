using System;
using System.Collections.Generic;
using Game.Content;
using Game.Enemy;
using Game.Progression;
using Game.Presentation;
using Game.Run;

namespace Game.UI
{
    public interface IGameplayUiModel
    {
        event Action Changed;

        float CurrentHealth { get; }
        BossViewState Boss { get; }
        float MaxHealth { get; }
        float ExperienceProgress01 { get; }
        int Level { get; }
        float ElapsedSeconds { get; }
        CharacterStatsViewState Stats { get; }
        RunExperienceSnapshot ExperienceTotals { get; }
        RunState RunState { get; }
        int SpeedMultiplier { get; }
        bool IsDraftOpen { get; }
        Guid DraftRevision { get; }
        DraftRequest CurrentDraftRequest { get; }
        DraftRequest NextDraftRequest { get; }
        int PendingDraftCount { get; }
        long BookCurrency { get; }
        int RemainingRerolls { get; }
        int RemainingBanishes { get; }
        IReadOnlyList<DraftOption> DraftOptions { get; }
        IReadOnlyList<BuildEntry> BuildEntries { get; }
        IReadOnlyList<SetDefinition> SetDefinitions { get; }
        /// <summary>Display name of a draftable build entry (owned or not); null when unknown.</summary>
        string FindBuildEntryName(ContentId id);
        CharacterDefinition SelectedCharacter { get; }
        IReadOnlyList<CharacterDefinition> UnlockedCharacters { get; }
        bool DevelopmentCommandsEnabled { get; }
        string SkillDevelopmentSummary { get; }
        string EnemyDevelopmentSummary { get; }
        int WavePhaseNumber { get; }
        int WavePhaseCount { get; }
        string WavePhaseName { get; }
        WavePhaseTag WavePhaseTag { get; }
        string WaveDevelopmentSummary { get; }

        bool SelectDraftOption(ContentId id, Guid revision);
        bool RerollDraft(Guid revision);
        bool BanishDraftOption(ContentId id, Guid revision);
        void TogglePause();
        bool SetSpeed(int multiplier);
        void AddFixtureExperience();
        void AddFixtureBook();
        void ApplyFixtureDamage();
        void ApplyFixtureHealing();
        void PreviewPresentationMotion(SpritePresentationPreviewMotion previewMotion);
        void ResetPresentation();
    }
}
