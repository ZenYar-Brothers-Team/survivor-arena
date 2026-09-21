using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    /// <summary>Player-facing pre-run fixture; later navigation can reuse its presenter.</summary>
    public sealed class CharacterSelectScreen : ICharacterSelectView, IDisposable
    {
        private readonly GameObject _owner;
        private readonly PanelSettings _panel;
        private readonly VisualElement _cards;
        private readonly Button _start;
        private readonly CharacterSelectPresenter _presenter;
        public UIDocument Document { get; }
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public CharacterSelectScreen(Transform parent, CharacterSelectionSession session, ContentRegistry registry)
        {
            _owner = new GameObject("Character Selection UI");
            // A nested UIDocument must inherit its parent's panel. Keep this independently
            // owned screen at the scene root so reopening it after a HUD run is safe.
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner, parent.gameObject.scene);
            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panel.referenceResolution = new Vector2Int(1920, 1080);
            _panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            Document = _owner.AddComponent<UIDocument>();
            Document.panelSettings = _panel;
            Document.sortingOrder = 200;
            var root = Document.rootVisualElement;
            root.name = GameplayUiElementIds.CharacterSelectScreen;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/GameplayUiStyles"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/CharacterSelectStyles"));
            root.AddToClassList("character-select-screen");
            var title = new Label("Choose your character");
            title.AddToClassList("character-select-title");
            root.Add(title);
            var subtitle = new Label("Fixture roster • portrait and icon placeholders");
            subtitle.AddToClassList("character-select-subtitle");
            root.Add(subtitle);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("character-select-scroll");
            _cards = new VisualElement { name = GameplayUiElementIds.CharacterSelectCards };
            _cards.AddToClassList("character-select-cards");
            scroll.Add(_cards);
            root.Add(scroll);
            _start = new Button(() => StartRequested?.Invoke()) { text = "Choose field", name = GameplayUiElementIds.CharacterSelectStart };
            _start.AddToClassList("character-select-start");
            root.Add(_start);
            _presenter = new CharacterSelectPresenter(session, registry, this);
        }
        public void Render(IReadOnlyList<CharacterSelectCardViewState> cards, bool canStart)
        {
            _cards.Clear();
            foreach (var state in cards)
            {
                var card = new ContentCard(state.Card, () => Selected?.Invoke(state.Id))
                    { name = GameplayUiElementIds.CharacterSelectCard(state.Id.ToString()) };
                card.AddToClassList("character-select-card");
                var crop = new Image { sprite = state.Crop, name = GameplayUiElementIds.CharacterSelectCrop, scaleMode = ScaleMode.ScaleToFit };
                crop.AddToClassList("character-select-crop");
                card.Insert(0, crop);
                _cards.Add(card);
            }
            _start.SetEnabled(canStart);
        }
        public void Dispose()
        {
            _presenter.Dispose();
            Selected = null;
            StartRequested = null;
            Document.rootVisualElement.Clear();
            Document.enabled = false;
            if (Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
