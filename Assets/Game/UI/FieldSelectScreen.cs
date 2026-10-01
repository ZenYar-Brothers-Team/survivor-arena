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
        private readonly Dictionary<ContentId, Button> _choices = new Dictionary<ContentId, Button>();
        private readonly Label _detail;
        private readonly ScrollView _scroll;
        private const int Columns = 5;
        // Up to this many fields share the screen without scrolling (4 full rows); more fields keep the same card size and scroll.
        private const int FittingFieldCount = 20;
        // Field backgrounds are authored at 512x341; the card picture keeps that ratio while there is room for it.
        private const float PictureAspect = 341f / 512f;
        public UIDocument Document { get; }
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public event Action BackRequested;
        public FieldSelectScreen(Transform parent, FieldSelectionSession session, ContentRegistry registry, Game.Progression.CharacterDefinition character = null,
            IReadOnlyList<FieldPreviewEntry> previews = null)
        {
            _owner = new GameObject("Field Selection UI");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(_owner, parent.gameObject.scene);
            _panel = ScriptableObject.CreateInstance<PanelSettings>();
            _panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            _panel.referenceResolution = new Vector2Int(1920, 1080);
            UiScale.Register(_panel);
            _panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/GameplayTheme");
            Document = _owner.AddComponent<UIDocument>();
            Document.panelSettings = _panel;
            Document.sortingOrder = 200;
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            var root = Document.rootVisualElement;
            root.name = GameplayUiElementIds.FieldSelectScreen;
            EntryUi.Configure(root, _panel);
            var tree = Resources.Load<VisualTreeAsset>("UI/FieldSelect");
            tree.CloneTree(root);
            FolioBackdrop.Attach(root.Q<VisualElement>(className: "entry-page"));
            _cards = root.Q<VisualElement>(GameplayUiElementIds.FieldSelectCards);
            _scroll = root.Q<ScrollView>(className: "entry-field-scroll");
            _scroll.RegisterCallback<GeometryChangedEvent>(_ => FitPictures());
            _cards.RegisterCallback<GeometryChangedEvent>(_ => FitPictures());
            _start = root.Q<Button>(GameplayUiElementIds.FieldSelectStart);
            _detail = root.Q<Label>(GameplayUiElementIds.EntryFieldDetail);
            if (character?.Presentation != null)
            {
                var hero = root.Q<VisualElement>(GameplayUiElementIds.EntryFieldHero);
                hero.Add(EntryUi.Image(character.Presentation.Crop.Resolve(registry).Sprite, "entry-footer-portrait"));
                hero.Add(EntryUi.Label(character.DisplayName, "entry-muted"));
            }
            _start.clicked += () => StartRequested?.Invoke();
            root.Q<Button>(GameplayUiElementIds.FieldSelectBack).clicked += () => BackRequested?.Invoke();
            _presenter = new FieldSelectPresenter(session, this, registry, previews);
        }
        public void Render(IReadOnlyList<FieldSelectCardViewState> cards, bool canStart)
        {
            var current = new HashSet<ContentId>();
            foreach (var state in cards) current.Add(state.Id);
            foreach (var id in new List<ContentId>(_choices.Keys))
                if (!current.Contains(id)) { _choices[id].RemoveFromHierarchy(); _choices.Remove(id); }
            foreach (var state in cards)
            {
                if (!_choices.TryGetValue(state.Id, out var card))
                {
                    var id = state.Id;
                    card = new Button(() => Selected?.Invoke(id)) { name = GameplayUiElementIds.FieldSelectCard(id.ToString()) };
                    card.AddToClassList("entry-choice"); card.AddToClassList("entry-field-choice");
                    var image = EntryUi.Image(state.Thumbnail, "entry-field-image", GameplayUiElementIds.FieldSelectThumbnail);
                    image.scaleMode = ScaleMode.ScaleAndCrop;
                    card.Add(image);
                    card.Add(EntryUi.Label("", "entry-choice-name", GameplayUiElementIds.FieldSelectName));
                    card.Add(EntryUi.Label("", "entry-choice-state", GameplayUiElementIds.CardStatus));
                    _choices.Add(id, card); _cards.Add(card);
                }
                EntryUi.Choice(card, state.Card);
                var picture = card.Q<Image>(GameplayUiElementIds.FieldSelectThumbnail);
                if (state.Card.IsLocked) BlurredThumbnail.Apply(picture, state.Thumbnail);
                else { picture.image = null; picture.sprite = state.Thumbnail; }
                card.Q<Label>(GameplayUiElementIds.FieldSelectName).text = state.Card.Title;
                card.Q<Label>(GameplayUiElementIds.CardStatus).text = state.Card.IsLocked ? "Закрыто" : state.Card.IsSelected ? "Выбрано" : "Доступно";
                if (state.Card.IsSelected) _detail.text = state.Card.Title + (state.LockReason == null ? "" : "  ·  " + EntryUi.Readable(state.LockReason));
            }
            if (cards.Count == 0) _detail.text = "Не удалось загрузить поля";
            _start.SetEnabled(canStart);
        }
        // Picture height follows the card width (no vertical cropping on wide screens) but yields so every row stays on screen.
        private void FitPictures()
        {
            if (_choices.Count == 0) return;
            var area = _scroll.contentViewport.resolvedStyle.height;
            using var cards = _choices.Values.GetEnumerator();
            if (!cards.MoveNext() || float.IsNaN(area) || area <= 0) return;
            var card = cards.Current;
            var picture = card.Q<Image>(GameplayUiElementIds.FieldSelectThumbnail);
            var style = card.resolvedStyle;
            var inner = style.width - style.paddingLeft - style.paddingRight - style.borderLeftWidth - style.borderRightWidth;
            var text = style.height - picture.resolvedStyle.height;
            if (float.IsNaN(inner) || inner <= 0 || float.IsNaN(text) || text <= 0) return;
            var rows = FittingRows(_choices.Count);
            var natural = inner * PictureAspect;
            var height = Mathf.Clamp(area / rows - style.marginBottom - text, 60f, natural);
            foreach (var choice in _choices.Values)
            {
                var image = choice.Q<Image>(GameplayUiElementIds.FieldSelectThumbnail);
                if (Mathf.Abs(image.resolvedStyle.height - height) > 0.5f) image.style.height = height;
            }
        }
        /// <summary>Rows the card pictures are sized for: every row shares the screen up to 20 fields, extra rows scroll.</summary>
        public static int FittingRows(int fieldCount) => Mathf.CeilToInt(Mathf.Min(fieldCount, FittingFieldCount) / (float)Columns);
        public void Dispose()
        {
            _presenter.Dispose(); Selected = null; StartRequested = null; BackRequested = null;
            if (Document != null) { Document.rootVisualElement?.Clear(); Document.enabled = false; }
            if (Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
