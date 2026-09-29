using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    internal sealed class SettingsPanel
    {
        private readonly VisualElement _root;
        private bool _modal;
        private bool _open;
        private T Q<T>(string id) where T : VisualElement => _root.Q<T>(id);
        public SettingsPanel(VisualElement root)
        {
            _root = root;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/FolioChromeStyles"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/SettingsStyles"));
            FolioBackdrop.Attach(Q<VisualElement>(GameplayUiElementIds.SettingsBody));
            FolioPanelTexture.Attach(root.Q(className: "settings-folio"));
            Q<VisualElement>(GameplayUiElementIds.SettingsModal).RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Tab)
                {
                    e.PreventDefault(); e.StopPropagation();
                    var keep = Q<Button>(GameplayUiElementIds.SettingsKeep);
                    var back = Q<Button>(GameplayUiElementIds.SettingsRevert);
                    (_root.panel?.focusController?.focusedElement == back ? keep : back).Focus();
                }
            });
        }
        public void Render(AppShellViewState state)
        {
            Q<Label>(GameplayUiElementIds.SettingsMasterValue).text = (state.Values.Master * 100).ToString("0") + "%";
            Q<Label>(GameplayUiElementIds.SettingsMusicValue).text = (state.Values.Music * 100).ToString("0") + "%";
            Q<Label>(GameplayUiElementIds.SettingsSfxValue).text = (state.Values.Sfx * 100).ToString("0") + "%";
            Q<Label>(GameplayUiElementIds.SettingsOrigin).text = state.SettingsFromPause ? "Игра на паузе" : "Главное меню";
            Q<Label>(GameplayUiElementIds.SettingsBindings).text = "Движение    " + (state.Values.MouseMovement ? "К указателю мыши" : state.Bindings);
            Q<Label>(GameplayUiElementIds.SettingsVideoStatus).text = state.Busy ? "Применяем режим…" : state.VideoStatus;
            Q<Label>(GameplayUiElementIds.SettingsConfirmStatus).text = state.VideoStatus;
            Q<Label>(GameplayUiElementIds.SettingsMessage).text = SettingsMessage(state.Message);
            Q<Button>(GameplayUiElementIds.SettingsSave).EnableInClassList("settings-hidden", !state.CanRetrySettingsSave);
            var modal = state.Settings && (state.Confirming || _modal && state.Busy);
            Q<VisualElement>(GameplayUiElementIds.SettingsModal).EnableInClassList("settings-hidden", !modal);
            Q<VisualElement>(GameplayUiElementIds.SettingsContent).SetEnabled(!modal && !state.Busy);
            Q<VisualElement>(GameplayUiElementIds.SettingsFooter).SetEnabled(!modal);
            Q<Button>(GameplayUiElementIds.SettingsKeep).SetEnabled(state.Confirming);
            Q<Button>(GameplayUiElementIds.SettingsRevert).SetEnabled(state.Confirming);
            if (modal && !_modal) Q<Button>(GameplayUiElementIds.SettingsRevert).Focus();
            else if (!modal && _modal && state.Settings) Q<DropdownField>(GameplayUiElementIds.SettingsWindow).Focus();
            else if (state.Settings && !_open) Q<Slider>(GameplayUiElementIds.SettingsMaster).Focus();
            else if (!state.Settings && _open)
                Q<Button>(state.Menu ? GameplayUiElementIds.ShellSettings : GameplayUiElementIds.ShellPauseSettings)?.Focus();
            _modal = modal; _open = state.Settings;
        }
        private static string SettingsMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return "Изменения сохраняются при выходе.";
            if (message.StartsWith("Settings not saved:")) return "Не удалось сохранить. Настройки действуют до выхода из игры.";
            if (message.StartsWith("Settings reset")) return "Включены настройки по умолчанию. Исходный файл сохранён.";
            if (message.StartsWith("Settings could not be read:")) return "Не удалось прочитать настройки. Включены значения по умолчанию.";
            if (message.StartsWith("Unavailable video mode")) return "Включён безопасный оконный режим.";
            if (message.StartsWith("Unsupported video")) return "Этот видеорежим недоступен.";
            if (message.StartsWith("Video rollback failed:")) return "Не удалось вернуть прежний видеорежим.";
            if (message.StartsWith("Video")) return "Не удалось применить видеорежим. Проверьте текущие параметры.";
            return message;
        }
    }
}
