using System;
using System.Collections.Generic;
using Game.Content;
using Game.Field;
using Game.Presentation;
namespace Game.UI
{
    public sealed class FieldSelectPresenter : IDisposable
    {
        private readonly FieldSelectionSession _session;
        private readonly IFieldSelectView _view;
        private readonly ContentRegistry _registry;
        private readonly IReadOnlyList<FieldPreviewEntry> _previews;
        private ContentId _inspected;
        public FieldSelectPresenter(FieldSelectionSession session, IFieldSelectView view, ContentRegistry registry = null, IReadOnlyList<FieldPreviewEntry> previews = null)
        {
            _previews = previews ?? Array.Empty<FieldPreviewEntry>();
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _registry = registry;
            _inspected = session.SelectedId;
            _view.Selected += Select; _view.StartRequested += Start; _view.BackRequested += Back;
            Refresh();
        }
        public void Refresh()
        {
            var cards = new List<FieldSelectCardViewState>();
            foreach (var field in _session.Roster.AllFields)
            {
                var reason = _session.Roster.GetLockReason(field.Id);
                // A closed map hides its name; the view blurs its picture.
                var thumbnail = field.Thumbnail.HasValue
                    ? field.Thumbnail.Value.Resolve(_registry ?? throw new InvalidOperationException("Field thumbnail requires a content registry."))
                    : null;
                thumbnail?.RequireRole(SpriteRole.Background);
                var summary = $"Сложность: {field.Difficulty}/5";
                if (reason != null) summary += "\n" + reason;
                cards.Add(new FieldSelectCardViewState(field.Id, new ContentCardViewState(reason != null ? "?" : field.DisplayName,
                    summary, summary, null, !_session.Started, field.Id == _inspected, reason != null),
                    field.ThumbnailPlaceholder, thumbnail?.Sprite, field.Difficulty, reason));
            }
            foreach (var preview in _previews)
            {
                // Closed, non-selectable stand-ins: blurred picture, hidden name, no difficulty until the map is defined.
                var picture = _registry != null && _registry.TryGet<SpriteDefinition>(new ContentId(preview.Id + "-VISUAL-BACKGROUND"), out var background)
                    ? background.Sprite : null;
                cards.Add(new FieldSelectCardViewState(preview.Id, new ContentCardViewState("?", preview.LockReason, preview.LockReason,
                    null, !_session.Started, false, true), "", picture, 0, preview.LockReason));
            }
            _view.Render(cards.AsReadOnly(), _session.CanStart && _inspected == _session.SelectedId);
        }
        private void Select(ContentId id)
        {
            if (_session.Started) return;
            foreach (var field in _session.Roster.AllFields)
                if (field.Id == id) { _inspected = id; _session.Select(id); break; }
            Refresh();
        }
        private void Start() { if (_inspected != _session.SelectedId || !_session.TryStart()) Refresh(); }
        private void Back() => _session.Back();
        public void Dispose()
        { _view.Selected -= Select; _view.StartRequested -= Start; _view.BackRequested -= Back; }
    }
}
