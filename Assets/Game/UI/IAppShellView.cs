using System;
using Game.Settings;
namespace Game.UI
{
    public interface IAppShellView
    {
        event Action Play, Meta, Settings, Exit, MainMenu, Quit, Back, Apply, Keep, Revert, Save;
        event Action<float, float, float> Audio;
        event Action<bool> Shake, Preview;
        event Action<VideoMode> Video;
        void Render(AppShellViewState state);
    }
}
