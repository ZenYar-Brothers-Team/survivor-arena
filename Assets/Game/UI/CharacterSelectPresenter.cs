using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
namespace Game.UI
{
    public sealed class CharacterSelectPresenter : IDisposable
    {
        private readonly CharacterSelectionSession _session;
        private readonly ContentRegistry _registry;
        private readonly ICharacterSelectView _view;
        private readonly Func<ContentId, string> _permanentSummary;
        public CharacterSelectPresenter(CharacterSelectionSession session, ContentRegistry registry, ICharacterSelectView view, Func<ContentId, string> permanentSummary = null)
        {
            _permanentSummary = permanentSummary;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Selected += Select;
            _view.StartRequested += Start;
            Refresh();
        }
        public void Refresh()
        {
            var cards = new List<CharacterSelectCardViewState>();
            foreach (var character in _session.Roster.AllCharacters)
            {
                var presentation = character.Presentation ?? throw new InvalidOperationException("Selection requires presentation metadata.");
                var baseline = presentation.Baseline.Resolve(_registry);
                var reason = _session.Roster.GetLockReason(character.Id);
                var summary = presentation.Role + "\nStarts with " + character.ResolveStartingActiveSkill(_registry).DisplayName;
                foreach (var field in presentation.Highlights)
                    summary += "\n" + CharacterHighlightFormatter.Format(character.BaseStats, baseline.Stats, field);
                if (_permanentSummary != null) summary += "\n" + _permanentSummary(character.Id);
                if (reason != null) summary += "\n" + reason;
                cards.Add(new CharacterSelectCardViewState(character.Id, new ContentCardViewState(
                    character.DisplayName, summary, summary, presentation.Icon.Resolve(_registry).Sprite,
                    !_session.Started, character.Id == _session.SelectedId, reason != null), presentation.Crop.Resolve(_registry).Sprite));
            }
            _view.Render(cards.AsReadOnly(), _session.CanStart);
        }
        private void Select(ContentId id) { _session.Select(id); Refresh(); }
        private void Start() { if (!_session.TryStart()) Refresh(); }
        public void Dispose()
        {
            _view.Selected -= Select;
            _view.StartRequested -= Start;
        }
    }
}
