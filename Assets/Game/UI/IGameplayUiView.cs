using System;
using Game.Content;
using Game.Presentation;

namespace Game.UI
{
    public interface IGameplayUiView
    {
        event Action<ContentId, Guid> DraftOptionSelected;
        event Action<Guid> DraftRerollRequested;
        event Action<Guid> DraftBanishModeRequested;
        event Action PauseRequested;
        event Action<int> SpeedRequested;
        event Action AddExperienceRequested;
        event Action AddLargeExperienceRequested;
        event Action AddRerollsRequested;
        event Action UnlockAllDraftEntriesRequested;
        event Action AddBookRequested;
        event Action ApplyDamageRequested;
        event Action ApplyHealingRequested;
        event Action ToggleHealthLockRequested;
        event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
        event Action PresentationResetRequested;

        void RenderHud(HudViewState state);
        void RenderDraft(DraftViewState state);
        void RenderRunOverlay(RunOverlayViewState state);
        void RenderBuild(BuildViewState state);
        void RenderCharacterSelection(CharacterSelectionViewState state);
        void RenderSkillObservability(SkillObservabilityViewState state);
        void RenderEnemyObservability(EnemyObservabilityViewState state);
        void RenderWaveObservability(WaveObservabilityViewState state);
        void SetDevelopmentControlsVisible(bool isVisible);
    }
}
