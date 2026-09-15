using System;
using Game.Content;

namespace Game.UI
{
    public interface IGameplayUiView
    {
        event Action<ContentId> DraftOptionSelected;
        event Action DraftRerollRequested;
        event Action<ContentId> DraftBanishRequested;
        event Action PauseRequested;
        event Action AddExperienceRequested;
        event Action ApplyDamageRequested;
        event Action ApplyHealingRequested;

        void RenderHud(HudViewState state);
        void RenderDraft(DraftViewState state);
        void RenderRunOverlay(RunOverlayViewState state);
        void RenderBuild(BuildViewState state);
        void SetDevelopmentControlsVisible(bool isVisible);
    }
}
