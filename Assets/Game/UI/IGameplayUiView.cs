using System;
using Game.Content;
using Game.Presentation;

namespace Game.UI
{
    public interface IGameplayUiView
    {
        event Action<ContentId, Guid> DraftOptionSelected;
        event Action<Guid> DraftRerollRequested;
        event Action<ContentId, Guid> DraftBanishRequested;
        event Action PauseRequested;
        event Action AddExperienceRequested;
        event Action AddBookRequested;
        event Action ApplyDamageRequested;
        event Action ApplyHealingRequested;
        event Action<SpritePresentationPreviewMotion> PresentationMotionPreviewRequested;
        event Action PresentationResetRequested;

        void RenderHud(HudViewState state);
        void RenderDraft(DraftViewState state);
        void RenderRunOverlay(RunOverlayViewState state);
        void RenderBuild(BuildViewState state);
        void RenderCharacterSelection(CharacterSelectionViewState state);
        void RenderEnemyObservability(EnemyObservabilityViewState state);
        void RenderWaveObservability(WaveObservabilityViewState state);
        void SetDevelopmentControlsVisible(bool isVisible);
    }
}
