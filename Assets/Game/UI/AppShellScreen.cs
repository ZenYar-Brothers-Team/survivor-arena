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
        private readonly VisualElement _pauseActions;
        private readonly VisualElement _pauseHome;
        private readonly MenuIllustration _illustration;
        private readonly SettingsPanel _settingsPanel;
        public UIDocument Document { get; }
        public event Action Play, Meta, Settings, Exit, MainMenu, Quit, Back, Apply, Keep, Revert, Save, DevelopmentUnlockAll, DevelopmentGrantCurrency, DevelopmentReset;
        public event Action<float,float,float> Audio;
        public event Action<bool> Shake, MouseMovement, Preview;
        public event Action<VideoMode> Video;
        public event Action<float> UiScaleChosen;
        private T Q<T>(string id) where T:VisualElement => Document.rootVisualElement.Q<T>(id);
        public AppShellScreen(Transform parent)
        {
            _owner=new GameObject("App shell"); UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner,parent.gameObject.scene);
            _panel=ScriptableObject.CreateInstance<PanelSettings>(); _panel.scaleMode=PanelScaleMode.ScaleWithScreenSize; _panel.referenceResolution=new Vector2Int(1920,1080);
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("UI/GameplayTheme"); UiScale.Register(_panel);
            Document=_owner.AddComponent<UIDocument>(); Document.panelSettings=_panel; Document.sortingOrder=400;
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            Document.rootVisualElement.pickingMode=PickingMode.Ignore;
            Resources.Load<VisualTreeAsset>("UI/AppShell").CloneTree(Document.rootVisualElement);
            Document.rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("UI/AppShellStyles"));
            EntryUi.Configure(Document.rootVisualElement, _panel);
            _settingsPanel = new SettingsPanel(Document.rootVisualElement);
            _illustration = new MenuIllustration(Q<VisualElement>(GameplayUiElementIds.ShellMenu));
            Hook(GameplayUiElementIds.EntryDevelopmentToggle, () => Q<VisualElement>(GameplayUiElementIds.ShellDevelopment).ToggleInClassList("entry-dev-open"));
            Hook(GameplayUiElementIds.ShellPlay,()=>Play?.Invoke()); Hook(GameplayUiElementIds.ShellMeta,()=>Meta?.Invoke());
            Hook(GameplayUiElementIds.ShellSettings,()=>Settings?.Invoke()); Hook(GameplayUiElementIds.ShellExit,()=>Exit?.Invoke());
            Hook(GameplayUiElementIds.ShellDevelopmentUnlockAll,()=>DevelopmentUnlockAll?.Invoke());
            Hook(GameplayUiElementIds.ShellDevelopmentGrantCurrency,()=>DevelopmentGrantCurrency?.Invoke());
            Hook(GameplayUiElementIds.ShellDevelopmentReset,()=>DevelopmentReset?.Invoke());
            Hook(GameplayUiElementIds.ShellBack,()=>MainMenu?.Invoke()); Hook(GameplayUiElementIds.ShellPauseSettings,()=>Settings?.Invoke()); Hook(GameplayUiElementIds.ShellQuit,()=>Quit?.Invoke());
            _pauseActions = Q<VisualElement>(GameplayUiElementIds.ShellPause);
            _pauseHome = _pauseActions.parent;
            Q<Button>(GameplayUiElementIds.ShellPauseSettings).text = "Настройки";
            Q<Button>(GameplayUiElementIds.ShellQuit).text = "Завершить забег";
            Hook(GameplayUiElementIds.SettingsBack,()=>Back?.Invoke()); Hook(GameplayUiElementIds.SettingsApply,()=>Apply?.Invoke());
            Hook(GameplayUiElementIds.SettingsKeep,()=>Keep?.Invoke()); Hook(GameplayUiElementIds.SettingsRevert,()=>Revert?.Invoke()); Hook(GameplayUiElementIds.SettingsSave,()=>Save?.Invoke());
            Hook(GameplayUiElementIds.SettingsMusicPreview,()=>Preview?.Invoke(true)); Hook(GameplayUiElementIds.SettingsSfxPreview,()=>Preview?.Invoke(false));
            foreach(var id in new[]{GameplayUiElementIds.SettingsMaster,GameplayUiElementIds.SettingsMusic,GameplayUiElementIds.SettingsSfx})
                Q<Slider>(id).RegisterValueChangedCallback(_=>Audio?.Invoke(Q<Slider>(GameplayUiElementIds.SettingsMaster).value/100,Q<Slider>(GameplayUiElementIds.SettingsMusic).value/100,Q<Slider>(GameplayUiElementIds.SettingsSfx).value/100));
            Q<Toggle>(GameplayUiElementIds.SettingsShake).RegisterValueChangedCallback(e=>Shake?.Invoke(e.newValue));
            Q<Toggle>(GameplayUiElementIds.SettingsMouseMovement).RegisterValueChangedCallback(e=>MouseMovement?.Invoke(e.newValue));
            Q<DropdownField>(GameplayUiElementIds.SettingsWindow).choices = new System.Collections.Generic.List<string> { "На весь экран", "В окне" };
            Q<DropdownField>(GameplayUiElementIds.SettingsWindow).RegisterValueChangedCallback(e=>
                Video?.Invoke(e.newValue == "В окне" ? _state.SafeWindow : _state.Desktop));
            Q<DropdownField>(GameplayUiElementIds.SettingsUiScale).choices = SettingsSnapshot.UiScales.Select(UiScale.Label).ToList();
            Q<DropdownField>(GameplayUiElementIds.SettingsUiScale).RegisterValueChangedCallback(e =>
                UiScaleChosen?.Invoke(SettingsSnapshot.UiScales.First(step => UiScale.Label(step) == e.newValue)));
            Q<DropdownField>(GameplayUiElementIds.SettingsResolution).RegisterValueChangedCallback(e=> { var mode=_state.Modes.FirstOrDefault(m=>m.ToString()==e.newValue); if(mode!=null)Video?.Invoke(mode); });
        }
        private void Hook(string id,Action action) => Q<Button>(id).clicked+=action;
        public void AttachPauseActions(VisualElement footer) => (footer ?? _pauseHome).Add(_pauseActions);
        private void Visible(string id,bool visible) => (id == GameplayUiElementIds.ShellPause ? _pauseActions : Q<VisualElement>(id)).style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
        public void Render(AppShellViewState state)
        {
            if(_disposed||Document==null)return; _state=state;
            _panel.scaleMode = state.Menu || state.CharacterBack || state.Settings ? PanelScaleMode.ConstantPixelSize : PanelScaleMode.ScaleWithScreenSize;
            _illustration.SetVisible(state.Menu);
            Visible(GameplayUiElementIds.ShellMenu,state.Menu); Visible(GameplayUiElementIds.ShellBack,state.CharacterBack);
            Visible(GameplayUiElementIds.ShellPause,state.PauseActions); Visible(GameplayUiElementIds.ShellDevelopment,state.DevelopmentUnlock);
            Q<Button>(GameplayUiElementIds.ShellDevelopmentReset).text=state.DevelopmentResetArmed?"DEV: click again to reset ALL progression":"DEV: reset all progression";
            Visible(GameplayUiElementIds.SettingsBody,state.Settings);
            Q<Button>(GameplayUiElementIds.ShellPlay).SetEnabled(state.CanPlay); Q<Button>(GameplayUiElementIds.ShellMeta).SetEnabled(state.CanPlay);
            Q<Slider>(GameplayUiElementIds.SettingsMaster).SetValueWithoutNotify(state.Values.Master*100);
            Q<Slider>(GameplayUiElementIds.SettingsMusic).SetValueWithoutNotify(state.Values.Music*100);
            Q<Slider>(GameplayUiElementIds.SettingsSfx).SetValueWithoutNotify(state.Values.Sfx*100);
            Q<Toggle>(GameplayUiElementIds.SettingsShake).SetValueWithoutNotify(state.Values.Shake);
            Q<Toggle>(GameplayUiElementIds.SettingsMouseMovement).SetValueWithoutNotify(state.Values.MouseMovement);
            Q<DropdownField>(GameplayUiElementIds.SettingsUiScale).SetValueWithoutNotify(UiScale.Label(state.Values.UiScale));
            Q<DropdownField>(GameplayUiElementIds.SettingsWindow).SetValueWithoutNotify(state.Candidate.Borderless ? "На весь экран" : "В окне");
            var resolution=Q<DropdownField>(GameplayUiElementIds.SettingsResolution); resolution.choices=state.Modes.Select(m=>m.ToString()).ToList(); resolution.SetValueWithoutNotify(state.Candidate.ToString()); resolution.SetEnabled(!state.Candidate.Borderless&&!state.Busy&&!state.Confirming);
            Q<DropdownField>(GameplayUiElementIds.SettingsWindow).SetEnabled(!state.Busy&&!state.Confirming);
            Q<Button>(GameplayUiElementIds.SettingsApply).SetEnabled(state.CanApplyVideo);
            Q<Button>(GameplayUiElementIds.SettingsBack).SetEnabled(!state.Busy);
            var detail=NotificationCopy.Detail(state.Notification);
            Q<Label>(GameplayUiElementIds.ShellNotificationTitle).text=NotificationCopy.Title(state.Notification);
            Q<Label>(GameplayUiElementIds.ShellNotificationDetail).text=detail;
            Q<Label>(GameplayUiElementIds.ShellNotificationDetail).EnableInClassList("shell-empty",string.IsNullOrEmpty(detail));
            Visible(GameplayUiElementIds.ShellNotification,!state.Notification.IsEmpty&&!state.Settings);
            _settingsPanel.Render(state);
        }
        public void Dispose()
        {
            if(_disposed)return; _disposed=true;
            _illustration.Dispose();
            if(Document!=null) { Document.rootVisualElement?.Clear(); Document.enabled=false; }
            if(Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
