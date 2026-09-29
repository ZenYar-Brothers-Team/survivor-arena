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
        public UIDocument Document { get; }
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public event Action BackRequested;
        public FieldSelectScreen(Transform parent, FieldSelectionSession session, ContentRegistry registry, Game.Progression.CharacterDefinition character = null)
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
            EntryUi.Configure(root, _panel);
            var tree = Resources.Load<VisualTreeAsset>("UI/FieldSelect");
            tree.CloneTree(root);
            FolioBackdrop.Attach(root.Q<VisualElement>(className: "entry-page"));
            _cards = root.Q<VisualElement>(GameplayUiElementIds.FieldSelectCards);
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
            _presenter = new FieldSelectPresenter(session, this, registry);
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
                    image.scaleMode = ScaleMode.ScaleAndCrop; card.Add(image);
                    card.Add(EntryUi.Label(state.Card.Title, "entry-choice-name"));
                    card.Add(CreateDifficulty());
                    card.Add(EntryUi.Label("", "entry-choice-state", GameplayUiElementIds.CardStatus));
                    _choices.Add(id, card); _cards.Add(card);
                }
                EntryUi.Choice(card, state.Card);
                var difficulty = card.Q<VisualElement>(GameplayUiElementIds.EntryFieldDifficulty);
                for (var i = 0; i < difficulty.childCount; i++)
                    difficulty[i].EnableInClassList("entry-sword-filled", i < state.Difficulty);
                card.Q<Label>(GameplayUiElementIds.CardStatus).text = state.Card.IsLocked ? "Закрыто" : state.Card.IsSelected ? "Выбрано" : "Доступно";
                if (state.Card.IsSelected) _detail.text = state.Card.Title + (state.LockReason == null ? "" : "  ·  " + EntryUi.Readable(state.LockReason));
            }
            if (cards.Count == 0) _detail.text = "Не удалось загрузить поля";
            _start.SetEnabled(canStart);
        }
        private static VisualElement CreateDifficulty()
        {
            var row = EntryUi.Box("entry-difficulty");
            row.name = GameplayUiElementIds.EntryFieldDifficulty;
            row.pickingMode = PickingMode.Ignore;
            for (var i = 0; i < 5; i++)
            {
                var slot = EntryUi.Box("entry-sword-slot");
                slot.pickingMode = PickingMode.Ignore;
                var sword = EntryUi.Box("entry-sword");
                sword.pickingMode = PickingMode.Ignore;
                foreach (var part in new[] { "tip", "blade", "guard", "grip" })
                {
                    var shape = EntryUi.Box("entry-sword-" + part);
                    shape.pickingMode = PickingMode.Ignore;
                    sword.Add(shape);
                }
                slot.Add(sword);
                row.Add(slot);
            }
            return row;
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
