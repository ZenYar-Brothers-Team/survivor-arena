using System;
using Game.Settings;
namespace Game.UI.Tests
{
    public sealed class FakeAppShellView : IAppShellView
    {
        public event Action Play,Meta,Settings,Exit,MainMenu,Quit,Back,Apply,Keep,Revert,Save,DevelopmentUnlockAll,DevelopmentReset;
        public event Action<float,float,float> Audio;
        public event Action<bool> Shake,MouseMovement,Preview;
        public event Action<float> UiScaleChosen;
        public void SetUiScale(float scale)=>UiScaleChosen?.Invoke(scale);
        public event Action<VideoMode> Video;
        public AppShellViewState State;
        public void Render(AppShellViewState state){State=state;}
        public void OpenSettings()=>Settings?.Invoke();public void CloseSettings()=>Back?.Invoke();
        public void UnlockAll()=>DevelopmentUnlockAll?.Invoke();public void ResetProgress()=>DevelopmentReset?.Invoke();public void Start()=>Play?.Invoke();public void Stop()=>Quit?.Invoke();
        public void SetMouseMovement(bool enabled)=>MouseMovement?.Invoke(enabled);
    }
}
