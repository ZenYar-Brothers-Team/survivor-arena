using System;
using System.Linq;
using Game.Settings;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    public sealed class AppShellScreen : IAppShellView, IDisposable
    {
        private readonly GameObject _owner;
        private readonly PanelSettings _panel;
        private AppShellViewState _state;
        private bool _disposed;
        public UIDocument Document { get; }
        public event Action Play, Meta, Settings, Exit, MainMenu, Quit, Back, Apply, Keep, Revert, Save;
        public event Action<float,float,float> Audio;
        public event Action<bool> Shake, Preview;
        public event Action<VideoMode> Video;
        private T Q<T>(string id) where T:VisualElement => Document.rootVisualElement.Q<T>(id);
        public AppShellScreen(Transform parent)
        {
            _owner=new GameObject("App shell"); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner,parent.gameObject.scene);
            _panel=ScriptableObject.CreateInstance<PanelSettings>(); _panel.scaleMode=PanelScaleMode.ScaleWithScreenSize; _panel.referenceResolution=new Vector2Int(1920,1080);
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            Document=_owner.AddComponent<UIDocument>(); Document.panelSettings=_panel; Document.sortingOrder=400;
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            Document.rootVisualElement.pickingMode=PickingMode.Ignore;
            Resources.Load<VisualTreeAsset>("UI/AppShell").CloneTree(Document.rootVisualElement);
            Document.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("UI/AppShellStyles"));
            Hook(GameplayUiElementIds.ShellPlay,()=>Play?.Invoke()); Hook(GameplayUiElementIds.ShellMeta,()=>Meta?.Invoke());
            Hook(GameplayUiElementIds.ShellSettings,()=>Settings?.Invoke()); Hook(GameplayUiElementIds.ShellExit,()=>Exit?.Invoke());
            Hook(GameplayUiElementIds.ShellBack,()=>MainMenu?.Invoke()); Hook(GameplayUiElementIds.ShellPauseSettings,()=>Settings?.Invoke()); Hook(GameplayUiElementIds.ShellQuit,()=>Quit?.Invoke());
            Hook(GameplayUiElementIds.SettingsBack,()=>Back?.Invoke()); Hook(GameplayUiElementIds.SettingsApply,()=>Apply?.Invoke());
            Hook(GameplayUiElementIds.SettingsKeep,()=>Keep?.Invoke()); Hook(GameplayUiElementIds.SettingsRevert,()=>Revert?.Invoke()); Hook(GameplayUiElementIds.SettingsSave,()=>Save?.Invoke());
            Hook(GameplayUiElementIds.SettingsMusicPreview,()=>Preview?.Invoke(true)); Hook(GameplayUiElementIds.SettingsSfxPreview,()=>Preview?.Invoke(false));
            foreach(var id in new[]{GameplayUiElementIds.SettingsMaster,GameplayUiElementIds.SettingsMusic,GameplayUiElementIds.SettingsSfx})
                Q<Slider>(id).RegisterValueChangedCallback(_=>Audio?.Invoke(Q<Slider>(GameplayUiElementIds.SettingsMaster).value/100,Q<Slider>(GameplayUiElementIds.SettingsMusic).value/100,Q<Slider>(GameplayUiElementIds.SettingsSfx).value/100));
            Q<Toggle>(GameplayUiElementIds.SettingsShake).RegisterValueChangedCallback(e=>Shake?.Invoke(e.newValue));
            Q<Toggle>(GameplayUiElementIds.SettingsWindow).RegisterValueChangedCallback(e=>
                Video?.Invoke(e.newValue?_state.SafeWindow:_state.Desktop));
            Q<DropdownField>(GameplayUiElementIds.SettingsResolution).RegisterValueChangedCallback(e=> { var mode=_state.Modes.FirstOrDefault(m=>m.ToString()==e.newValue); if(mode!=null)Video?.Invoke(mode); });
        }
        private void Hook(string id,Action action) => Q<Button>(id).clicked+=action;
        private void Visible(string id,bool visible) => Q<VisualElement>(id).style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
        public void Render(AppShellViewState state)
        {
            if(_disposed||Document==null)return; _state=state;
            Visible(GameplayUiElementIds.ShellMenu,state.Menu); Visible(GameplayUiElementIds.ShellBack,state.CharacterBack);
            Visible(GameplayUiElementIds.ShellPause,state.PauseActions); Visible(GameplayUiElementIds.SettingsBody,state.Settings);
            Q<Button>(GameplayUiElementIds.ShellPlay).SetEnabled(state.CanPlay); Q<Button>(GameplayUiElementIds.ShellMeta).SetEnabled(state.CanPlay);
            Q<Slider>(GameplayUiElementIds.SettingsMaster).SetValueWithoutNotify(state.Values.Master*100);
            Q<Slider>(GameplayUiElementIds.SettingsMusic).SetValueWithoutNotify(state.Values.Music*100);
            Q<Slider>(GameplayUiElementIds.SettingsSfx).SetValueWithoutNotify(state.Values.Sfx*100);
            Q<Toggle>(GameplayUiElementIds.SettingsShake).SetValueWithoutNotify(state.Values.Shake);
            Q<Toggle>(GameplayUiElementIds.SettingsWindow).SetValueWithoutNotify(!state.Candidate.Borderless);
            var resolution=Q<DropdownField>(GameplayUiElementIds.SettingsResolution); resolution.choices=state.Modes.Select(m=>m.ToString()).ToList(); resolution.SetValueWithoutNotify(state.Candidate.ToString()); resolution.SetEnabled(!state.Candidate.Borderless&&!state.Busy&&!state.Confirming);
            Q<Toggle>(GameplayUiElementIds.SettingsWindow).SetEnabled(!state.Busy&&!state.Confirming);
            Q<Button>(GameplayUiElementIds.SettingsApply).SetEnabled(!state.Busy&&!state.Confirming);
            Q<Button>(GameplayUiElementIds.SettingsBack).SetEnabled(!state.Busy);
            Visible(GameplayUiElementIds.SettingsKeep,state.Confirming); Visible(GameplayUiElementIds.SettingsRevert,state.Confirming);
            Q<Label>(GameplayUiElementIds.ShellNotification).text=state.Notification;
            Visible(GameplayUiElementIds.ShellNotification,!string.IsNullOrEmpty(state.Notification)&&!state.Settings);
            Q<Label>(GameplayUiElementIds.SettingsMessage).text=state.Message; Q<Label>(GameplayUiElementIds.SettingsBindings).text="Movement: "+state.Bindings;
            Q<Label>(GameplayUiElementIds.SettingsVideoStatus).text=state.VideoStatus;
        }
        public void Dispose()
        {
            if(_disposed)return; _disposed=true;
            if(Document!=null) { Document.rootVisualElement?.Clear(); Document.enabled=false; }
            if(Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
