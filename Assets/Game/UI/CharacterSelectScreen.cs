using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    /// <summary>Pre-run roster with read-only locked-hero inspection and explicit confirmation.</summary>
    public sealed class CharacterSelectScreen : ICharacterSelectView, IDisposable
    {
        private readonly GameObject _owner;
        private readonly PanelSettings _panel;
        private readonly VisualElement _cards;
        private readonly Button _start;
        private readonly CharacterSelectPresenter _presenter;
        private readonly Dictionary<ContentId, Button> _choices = new Dictionary<ContentId, Button>();
        private readonly Image _portrait;
        private readonly Image _skillIcon;
        private readonly VisualElement _detail;
        private readonly Label _name, _role, _skill, _boost, _highlights, _permanent, _lock, _selection;
        public UIDocument Document { get; }
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public CharacterSelectScreen(Transform parent, CharacterSelectionSession session, ContentRegistry registry, Func<ContentId, string> permanentSummary = null)
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
            // Separate panels require their own render and input order (IP-26).
            _panel.sortingOrder = Document.sortingOrder;
            var root = Document.rootVisualElement;
            root.name = GameplayUiElementIds.CharacterSelectScreen;
            EntryUi.Configure(root, _panel);
            root.AddToClassList("entry-page");
            FolioBackdrop.Attach(root);
            var header = EntryUi.Box("entry-header");
            header.Add(EntryUi.Label("Выбери персонажа", "entry-title"));
            header.Add(EntryUi.Label("1 · Персонаж    —    2 · Поле", "entry-steps"));
            root.Add(header);
            var workspace = EntryUi.Box("entry-workspace"); root.Add(workspace);
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("entry-roster");
            _cards = new VisualElement { name = GameplayUiElementIds.CharacterSelectCards };
            _cards.AddToClassList("entry-character-grid");
            scroll.Add(_cards);
            workspace.Add(scroll);
            var detail = EntryUi.Box("entry-character-detail"); workspace.Add(detail);
            FolioPanelTexture.Attach(detail);
            _detail = detail;
            var body = EntryUi.Box("entry-hero-well"); detail.Add(body);
            _portrait = EntryUi.Image(null, "entry-hero-image", GameplayUiElementIds.EntryPortrait); body.Add(_portrait);
            _lock = EntryUi.Label("", "entry-lock", GameplayUiElementIds.EntryLock); body.Add(_lock);
            var copy = new ScrollView(ScrollViewMode.Vertical); copy.AddToClassList("entry-character-copy"); detail.Add(copy);
            _name = EntryUi.Label("", "entry-name"); copy.Add(_name);
            _role = EntryUi.Label("", "entry-role"); copy.Add(_role);
            copy.Add(EntryUi.Label("СТАРТОВОЕ УМЕНИЕ", "entry-eyebrow"));
            var skillHeading = EntryUi.Box("entry-skill-heading"); copy.Add(skillHeading);
            _skillIcon = EntryUi.Image(null, "entry-skill-icon"); skillHeading.Add(_skillIcon);
            _skill = EntryUi.Label("", "entry-skill"); skillHeading.Add(_skill);
            _boost = EntryUi.Label("", "entry-copy"); copy.Add(_boost);
            _highlights = EntryUi.Label("", "entry-highlights"); copy.Add(_highlights);
            _permanent = EntryUi.Label("", "entry-muted"); copy.Add(_permanent);
            var footer = EntryUi.Box("entry-footer"); root.Add(footer);
            // The shell owns Main Menu routing and draws Back in this reserved space.
            footer.Add(EntryUi.Box("entry-back-space"));
            _selection = EntryUi.Label("", "entry-footer-detail"); footer.Add(_selection);
            _start = new Button(() => StartRequested?.Invoke()) { text = "Выбрать поле →", name = GameplayUiElementIds.CharacterSelectStart };
            _start.AddToClassList("entry-action"); _start.AddToClassList("entry-primary"); footer.Add(_start);
            _presenter = new CharacterSelectPresenter(session, registry, this, permanentSummary);
        }
        public void Render(IReadOnlyList<CharacterSelectCardViewState> cards, bool canStart)
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
                    card = new Button(() => Selected?.Invoke(id)) { name = GameplayUiElementIds.CharacterSelectCard(id.ToString()) };
                    card.AddToClassList("entry-choice"); card.AddToClassList("entry-character-choice");
                    card.Add(EntryUi.Image(state.Crop, "entry-character-crop", GameplayUiElementIds.CharacterSelectCrop));
                    card.Add(EntryUi.Label(state.Card.Title, "entry-choice-name"));
                    card.Add(EntryUi.Label("", "entry-choice-state", GameplayUiElementIds.CardStatus));
                    _choices.Add(id, card); _cards.Add(card);
                }
                EntryUi.Choice(card, state.Card);
                card.Q<Label>(className: "entry-choice-name").text = state.Card.IsLocked ? "?" : state.Card.Title;
                card.Q<Image>().tintColor = state.Card.IsLocked ? Color.black : Color.white;
                card.Q<Label>(GameplayUiElementIds.CardStatus).text = state.Card.IsLocked ? "" : state.Card.IsSelected ? "Выбран" : "Доступен";
                if (!state.Card.IsSelected) continue;
                _detail.EnableInClassList("entry-mystery", state.Card.IsLocked);
                _portrait.sprite = state.Crop; _portrait.tintColor = state.Card.IsLocked ? Color.black : Color.white;
                if (state.Card.IsLocked)
                {
                    _name.text = _role.text = _skill.text = _boost.text = _highlights.text = _permanent.text = _selection.text = "";
                    _skillIcon.sprite = null;
                    _lock.text = "?";
                    continue;
                }
                _name.text = state.Card.Title; _role.text = state.Role; _skill.text = state.Skill;
                _skillIcon.sprite = state.SkillIcon;
                _boost.text = EntryUi.Readable(state.Boost); _highlights.text = EntryUi.Readable(state.Highlights);
                _permanent.text = EntryUi.Readable(state.Permanent); _lock.text = EntryUi.Readable(state.LockReason);
                _selection.text = state.Card.Title;
            }
            if (cards.Count == 0) _selection.text = "Не удалось загрузить персонажей";
            _start.SetEnabled(canStart);
        }
        public void Dispose()
        {
            _presenter.Dispose();
            Selected = null;
            StartRequested = null;
            if (Document != null) Document.rootVisualElement?.Clear();
            if (Document != null) Document.enabled = false;
            if (Application.isPlaying) { UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_panel); }
            else { UnityEngine.Object.DestroyImmediate(_owner); UnityEngine.Object.DestroyImmediate(_panel); }
        }
    }
}
