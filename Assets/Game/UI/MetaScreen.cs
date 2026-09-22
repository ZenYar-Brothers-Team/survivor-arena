using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    /// <summary>View-only profile/results surface, independently owned from the per-run HUD.</summary>
    public sealed class MetaScreen : IMetaView, IDisposable
    {
        private bool _disposed;
        private readonly GameObject _owner;
        private readonly PanelSettings _panel;
        private readonly VisualElement _body;
        private readonly ScrollView _cards;
        private readonly DropdownField _characters;
        public UIDocument Document { get; }
        public event Action ShopRequested, CloseRequested, RetryRequested, SelectionRequested, QuitRequested, SaveRequested, ResetRequested;
        public event Action<string> CharacterRequested;
        public event Action<MetaCardViewState> PurchaseRequested;
        public MetaScreen(Transform parent)
        {
            _owner = new GameObject("Profile UI");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner, parent.gameObject.scene);
            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize; _panel.referenceResolution = new Vector2Int(1920, 1080);
            _panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            Document = _owner.AddComponent<UIDocument>(); Document.panelSettings = _panel; Document.sortingOrder = 300;
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            var root = Document.rootVisualElement; root.pickingMode = PickingMode.Ignore;
            Resources.Load<VisualTreeAsset>("UI/MetaScreen").CloneTree(root);
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/MetaScreenStyles"));
            _body = root.Q(GameplayUiElementIds.MetaBody); _cards = root.Q<ScrollView>(GameplayUiElementIds.MetaCards);
            _characters = root.Q<DropdownField>(GameplayUiElementIds.MetaCharacter);
            _characters.RegisterValueChangedCallback(e => CharacterRequested?.Invoke(e.newValue));
            Hook(GameplayUiElementIds.MetaOpen, () => ShopRequested?.Invoke()); Hook(GameplayUiElementIds.MetaClose, () => CloseRequested?.Invoke());
            Hook(GameplayUiElementIds.MetaRetry, () => RetryRequested?.Invoke()); Hook(GameplayUiElementIds.MetaSelection, () => SelectionRequested?.Invoke());
            Hook(GameplayUiElementIds.MetaQuit, () => QuitRequested?.Invoke()); Hook(GameplayUiElementIds.MetaSave, () => SaveRequested?.Invoke());
            Hook(GameplayUiElementIds.MetaReset, () => ResetRequested?.Invoke());
        }
        private void Hook(string id, Action action) => Document.rootVisualElement.Q<Button>(id).clicked += action;
        private void Button(string id, bool visible, bool enabled = true)
        { var b = Document.rootVisualElement.Q<Button>(id); b.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None; b.SetEnabled(enabled); }
        public void Render(MetaViewState state)
        {
            if (_disposed || Document == null || Document.rootVisualElement == null) return;
            var root = Document.rootVisualElement;
            _body.style.display = state.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            root.Q<Label>(GameplayUiElementIds.MetaTitle).text = state.Title;
            root.Q<Label>(GameplayUiElementIds.MetaSummary).text = state.Summary;
            root.Q<Label>(GameplayUiElementIds.MetaMessage).text = state.Message;
            Button(GameplayUiElementIds.MetaOpen, false);
            Button(GameplayUiElementIds.MetaQuit, false);
            Button(GameplayUiElementIds.MetaClose, state.Cards.Count > 0, state.CanContinue);
            Button(GameplayUiElementIds.MetaRetry, state.IsResults, state.CanContinue);
            Button(GameplayUiElementIds.MetaSelection, state.IsResults, state.CanContinue);
            Button(GameplayUiElementIds.MetaSave, state.IsError || state.IsResults && !state.CanContinue);
            Button(GameplayUiElementIds.MetaReset, state.CanReset);
            _characters.style.display = state.Cards.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _characters.choices = new List<string>(state.Characters); _characters.SetValueWithoutNotify(state.SelectedCharacter);
            _cards.Clear();
            foreach (var stateCard in state.Cards)
            {
                var row = new VisualElement { name = GameplayUiElementIds.MetaCard(stateCard.Id) }; row.AddToClassList("meta-card");
                row.Add(new Label(stateCard.Text)); row.Add(new Label(stateCard.Detail));
                var buy = new Button(() => PurchaseRequested?.Invoke(stateCard)) { text = "Buy", name = GameplayUiElementIds.MetaBuy(stateCard.Id) };
                buy.SetEnabled(stateCard.CanBuy); row.Add(buy); _cards.Add(row);
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (Document != null) { Document.rootVisualElement?.Clear(); Document.enabled = false; }
            if (Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
