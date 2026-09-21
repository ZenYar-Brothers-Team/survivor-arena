using System;
using System.Collections.Generic;
using Game.Content;
using Game.Field;
namespace Game.UI
{
    public sealed class FieldSelectPresenter : IDisposable
    {
        private readonly FieldSelectionSession _session;
        private readonly IFieldSelectView _view;
        public FieldSelectPresenter(FieldSelectionSession session, IFieldSelectView view)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _view.Selected += Select; _view.StartRequested += Start; _view.BackRequested += Back;
            Refresh();
        }
        public void Refresh()
        {
            var cards = new List<FieldSelectCardViewState>();
            foreach (var field in _session.Roster.AllFields)
            {
                var reason = _session.Roster.GetLockReason(field.Id);
                var summary = field.Description + $"\nDifficulty: {field.Difficulty}/5";
                if (reason != null) summary += "\n" + reason;
                cards.Add(new FieldSelectCardViewState(field.Id, new ContentCardViewState(field.DisplayName,
                    summary, summary, null, !_session.Started, field.Id == _session.SelectedId, reason != null), field.ThumbnailPlaceholder));
            }
            _view.Render(cards.AsReadOnly(), _session.CanStart);
        }
        private void Select(ContentId id) { _session.Select(id); Refresh(); }
        private void Start() { if (!_session.TryStart()) Refresh(); }
        private void Back() => _session.Back();
        public void Dispose()
        { _view.Selected -= Select; _view.StartRequested -= Start; _view.BackRequested -= Back; }
    }
}
