using System;
using Game.Content;
namespace Game.Field
{
    public sealed class FieldSelectionSession
    {
        private readonly IFieldRunLauncher _launcher;
        public FieldRoster Roster { get; }
        public ContentId SelectedId { get; private set; }
        public bool Started { get; private set; }
        public bool CanStart => !Started && Roster.TrySelect(SelectedId, out _);
        public FieldSelectionSession(FieldRoster roster, ContentId initial, IFieldRunLauncher launcher)
        {
            Roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            SelectedId = initial;
        }
        public bool Select(ContentId id)
        {
            if (Started || !Roster.TrySelect(id, out _)) return false;
            SelectedId = id;
            return true;
        }
        public bool TryStart()
        {
            if (!CanStart || !_launcher.TryStartField(SelectedId)) return false;
            Started = true;
            return true;
        }
        public void Back() { if (!Started) _launcher.BackToCharacters(); }
    }
}
