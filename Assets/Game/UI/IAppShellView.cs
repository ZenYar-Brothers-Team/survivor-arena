using System;
using Game.Settings;
namespace Game.UI
{
    public interface IAppShellView
    {
        event Action Play, Meta, Settings, Exit, MainMenu, Quit, Back, Apply, Keep, Revert, Save, DevelopmentUnlockAll, DevelopmentGrantCurrency, DevelopmentReset;
        event Action<float, float, float> Audio;
        event Action<bool> Shake, MouseMovement, Preview;
        event Action<VideoMode> Video;
        event Action<float> UiScaleChosen;
        void Render(AppShellViewState state);
    }
}
