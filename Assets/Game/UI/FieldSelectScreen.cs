using System;
using System.Collections.Generic;
using Game.Content;
using Game.Field;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    public sealed class FieldSelectScreen : IFieldSelectView, IDisposable
    {
        private readonly GameObject _owner;
        private readonly PanelSettings _panel;
        private readonly VisualElement _cards;
        private readonly Button _start;
        private readonly FieldSelectPresenter _presenter;
        public UIDocument Document { get; }
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public event Action BackRequested;
        public FieldSelectScreen(Transform parent, FieldSelectionSession session, ContentRegistry registry)
        {
            _owner = new GameObject("Field Selection UI");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner, parent.gameObject.scene);
            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panel.referenceResolution = new Vector2Int(1920, 1080);
            _panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            Document = _owner.AddComponent<UIDocument>();
            Document.panelSettings = _panel;
            Document.sortingOrder = 200;
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            var root = Document.rootVisualElement;
            root.name = GameplayUiElementIds.FieldSelectScreen;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/GameplayUiStyles"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/CharacterSelectStyles"));
            root.AddToClassList("character-select-screen");
            var tree = Resources.Load<VisualTreeAsset>("UI/FieldSelect");
            tree.CloneTree(root);
            _cards = root.Q<VisualElement>(GameplayUiElementIds.FieldSelectCards);
            _start = root.Q<Button>(GameplayUiElementIds.FieldSelectStart);
            _start.clicked += () => StartRequested?.Invoke();
            root.Q<Button>(GameplayUiElementIds.FieldSelectBack).clicked += () => BackRequested?.Invoke();
            _presenter = new FieldSelectPresenter(session, this, registry);
        }
        public void Render(IReadOnlyList<FieldSelectCardViewState> cards, bool canStart)
        {
            _cards.Clear();
            foreach (var state in cards)
            {
                var card = new ContentCard(state.Card, () => Selected?.Invoke(state.Id))
                    { name = GameplayUiElementIds.FieldSelectCard(state.Id.ToString()) };
                card.AddToClassList("character-select-card");
                VisualElement thumbnail;
                if (state.Thumbnail != null)
                {
                    thumbnail = new Image { sprite = state.Thumbnail, scaleMode = ScaleMode.ScaleAndCrop,
                        name = GameplayUiElementIds.FieldSelectThumbnail };
                    thumbnail.AddToClassList("field-thumbnail-image");
                }
                else
                {
                    thumbnail = new Label(state.ThumbnailPlaceholder) { name = GameplayUiElementIds.FieldSelectThumbnail };
                    thumbnail.AddToClassList("field-thumbnail-placeholder");
                }
                card.Insert(0, thumbnail);
                _cards.Add(card);
            }
            _start.SetEnabled(canStart);
        }
        public void Dispose()
        {
            _presenter.Dispose(); Selected = null; StartRequested = null; BackRequested = null;
            if (Document != null) { Document.rootVisualElement?.Clear(); Document.enabled = false; }
            if (Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
