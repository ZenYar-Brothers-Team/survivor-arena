using System;
using Game.Settings;
namespace Game.UI.Tests
{
    public sealed class FakeAppShellView : IAppShellView
    {
        public event Action Play,Meta,Settings,Exit,MainMenu,Quit,Back,Apply,Keep,Revert,Save;
        public event Action<float,float,float> Audio;
        public event Action<bool> Shake,Preview;
        public event Action<VideoMode> Video;
        public AppShellViewState State;
        public void Render(AppShellViewState state){State=state;}
        public void OpenSettings()=>Settings?.Invoke();public void CloseSettings()=>Back?.Invoke();
        public void Start()=>Play?.Invoke();public void Stop()=>Quit?.Invoke();
    }
}
