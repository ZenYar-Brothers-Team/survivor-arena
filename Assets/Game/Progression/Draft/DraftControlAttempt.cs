using System;
using Game.Content;

namespace Game.Progression
{
    /// <summary>Request/revision captured before reroll/banish can replace or exhaust the offer.</summary>
    public readonly struct DraftControlAttempt
    {
        public string Action { get; }
        public Guid RequestId { get; }
        public Guid Revision { get; }
        public ContentId? SelectedId { get; }
        public bool Succeeded { get; }
        public DraftControlAttempt(string action, Guid requestId, Guid revision, ContentId? selectedId, bool succeeded)
        { Action = action; RequestId = requestId; Revision = revision; SelectedId = selectedId; Succeeded = succeeded; }
    }
}
