using System;
using Game.Content;
namespace Game.Progression
{
    public sealed class CharacterSelectionSession
    {
        private readonly ICharacterRunLauncher _launcher;
        public CharacterRoster Roster { get; }
        public ContentId SelectedId { get; private set; }
        public bool Started { get; private set; }
        public bool CanStart => !Started && Roster.TrySelect(SelectedId, out _);
        public CharacterSelectionSession(CharacterRoster roster, ContentId initial, ICharacterRunLauncher launcher)
        {
            Roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
            // No automatic fallback to the first roster entry.
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
            if (!CanStart || !_launcher.TryStartCharacter(SelectedId)) return false;
            Started = true;
            return true;
        }
    }
}
