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
        float MaxHealth { get; }
        float ExperienceProgress01 { get; }
        int Level { get; }
        float RemainingSeconds { get; }
        RunState RunState { get; }
        bool IsDraftOpen { get; }
        int RemainingRerolls { get; }
        int RemainingBanishes { get; }
        IReadOnlyList<DraftOption> DraftOptions { get; }
        IReadOnlyList<BuildEntry> BuildEntries { get; }
        IReadOnlyList<SetDefinition> SetDefinitions { get; }
        CharacterDefinition SelectedCharacter { get; }
        IReadOnlyList<CharacterDefinition> UnlockedCharacters { get; }
        bool DevelopmentCommandsEnabled { get; }
        string EnemyDevelopmentSummary { get; }
        int WavePhaseNumber { get; }
        int WavePhaseCount { get; }
        string WavePhaseName { get; }
        WavePhaseTag WavePhaseTag { get; }
        string WaveDevelopmentSummary { get; }

        bool SelectDraftOption(ContentId id);
        bool RerollDraft();
        bool BanishDraftOption(ContentId id);
        void TogglePause();
        void AddFixtureExperience();
        void ApplyFixtureDamage();
        void ApplyFixtureHealing();
        void PreviewPresentationMotion(SpritePresentationPreviewMotion previewMotion);
        void ResetPresentation();
    }
}
